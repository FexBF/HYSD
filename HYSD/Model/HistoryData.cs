using Autofac;
using Serilog;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HYSD
{
    public partial class HistoryData : UserControl, IPollablePage
    {
        private readonly IOmronPlcService _plc;
        private readonly ILogger _logger;
        private readonly IReadDataService _readData;
        private readonly SqlSugarClient _db;

        /// <summary>配方监控服务（用于判断当前是否有批次正在记录）</summary>
        private readonly IRecipeMonitorService _monitor;

        /// <summary>
        /// 当前查看的批次数据库 SqlSugarClient。
        /// 为 null 时表示查看实时 SQLite 数据库（_db）；
        /// 不为 null 时表示查看某个已保存的批次数据库。
        /// </summary>
        private SqlSugarClient _viewDb;

        /// <summary>当前查看的批次数据库文件路径；为 null 表示实时模式</summary>
        private string _currentViewPath;

        // ★ 新增 UI 控件（代码动态创建，无需修改 Designer.cs）
        private Label lblDbTitle;
        private ComboBox cboBatchDb;
        private Button btnRefreshDb;
        private Label lblViewMode;

        private int pageIndex = 1;      // 当前页码
        private int pageSize = 15;      // 每页显示条数
        private int totalCount = 0;     // 总数据条数

        public HistoryData(IOmronPlcService plc, ILogger logger, IReadDataService readData, SqlSugarClient db)
        {
            InitializeComponent();
            _plc = plc;
            _logger = logger;
            _readData = readData;
            _db = db;

            // 从 DI 容器获取配方监控服务（单例），用于判断当前是否正在记录
            try
            {
                _monitor = AutofacConfig.Container.Resolve<IRecipeMonitorService>();
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "获取 IRecipeMonitorService 失败");
            }

            // ★ 轮询由 FormMain.SwitchPage 通过 IPollablePage.StartPolling() 启动
            InitDataGridViewStyle(dataGridView1);
            dataGridView1.OptimizeForPerformance();
            // 开始时间
            dtpStart.Value = DateTime.Now.AddDays(-7); // 默认查最近7天

            // ★ 新增：初始化批次数据库选择控件
            InitBatchDbControls();
            RefreshDbList();
        }

        // ★ 事件驱动：由 FormMain.SwitchPage 通过 IPollablePage.StartPolling() 订阅 DataUpdated 事件
        private bool _subscribed;

        public void StartPolling()
        {
            if (_subscribed) return;
            _readData.DataUpdated += OnPlcDataUpdated;
            _subscribed = true;
        }

        public void StopPolling()
        {
            if (!_subscribed) return;
            _readData.DataUpdated -= OnPlcDataUpdated;
            _subscribed = false;
        }

        public bool IsPolling => _subscribed;

        /// <summary>DataUpdated 事件回调（后台读取线程触发）：复用既有 DoWork 完成连接检查 + UI 线程切换</summary>
        private void OnPlcDataUpdated(object sender, EventArgs e)
        {
            DoWork();
        }

        private void DoWork()
        {
            try
            {
                if (_plc != null && _plc.IsConnected)
                {
                    if (this.IsHandleCreated && !this.IsDisposed)
                        this.BeginInvoke((Action)(() => PlcDataEx()));
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex.Message);
            }
        }


        public void InitDataGridViewStyle(DataGridView dgv)
        {
            // 1. 禁用自动生成列（防止数据源有多余字段显示出来）
            dgv.AutoGenerateColumns = false;

            // 2. 全局设置：列标题居中
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // 3. 全局设置：单元格内容居中
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // 4. 逐列设置：关闭排序（解决视觉偏左的核心问题）
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private EdgeDetector _sensor1Edge = new EdgeDetector();
        /// <summary>
        /// 实时数据记录逻辑（写入主 SQLite 数据库）。
        /// ★ 修改：若配方监控服务已启动（正在写入批次数据库），则跳过此处写入，避免重复记录。
        /// </summary>
        private void PlcDataEx()
        {
            if (_readData != null && !_readData.isRunning)
            {
                return;
            }
            // ★ 若配方监控服务正在运行，则不写入实时库，避免重复记录
            if (_monitor != null && _monitor.IsMonitoring)
            {
                return;
            }

            if (_sensor1Edge.DetectRisingEdge((bool)_readData.TryGetValueW("涂层数据记录位")))
            {
                var ds = new TCData();
                ds.DateTime = DateTime.Now;
                ds.UpHeat = (float)Math.Round((float)_readData.TryGetValueD29("上温度"), 1, MidpointRounding.AwayFromZero);
                ds.DnHeat = (float)Math.Round((float)_readData.TryGetValueD29("下温度"), 1, MidpointRounding.AwayFromZero);
                ds.Rotation = (ushort)_readData.TryGetValueD29("转速");
                ds.N2SV = (ushort)_readData.TryGetValueD29("氮气实际流量");
                ds.H2SV = (ushort)_readData.TryGetValueD29("氢气实际流量");
                ds.ArSV = (ushort)_readData.TryGetValueD29("氩气实际流量");
                ds.WaterTemp = (float)Math.Round((float)_readData.TryGetValueD29("冰水机实际温度"), 1, MidpointRounding.AwayFromZero);
                ds.B1Curr = (float)Math.Round((float)_readData.TryGetValueD29("弧源1电流"), 1, MidpointRounding.AwayFromZero);
                ds.B1Volt = (float)Math.Round((float)_readData.TryGetValueD29("弧源1电压"), 1, MidpointRounding.AwayFromZero);
                ds.B2Curr = (float)Math.Round((float)_readData.TryGetValueD29("弧源2电流"), 1, MidpointRounding.AwayFromZero);
                ds.B2Volt = (float)Math.Round((float)_readData.TryGetValueD29("弧源2电压"), 1, MidpointRounding.AwayFromZero);
                ds.B3Curr = (float)Math.Round((float)_readData.TryGetValueD29("弧源3电流"), 1, MidpointRounding.AwayFromZero);
                ds.B3Volt = (float)Math.Round((float)_readData.TryGetValueD29("弧源3电压"), 1, MidpointRounding.AwayFromZero);
                ds.B4Curr = (float)Math.Round((float)_readData.TryGetValueD29("弧源4电流"), 1, MidpointRounding.AwayFromZero);
                ds.B4Volt = (float)Math.Round((float)_readData.TryGetValueD29("弧源4电压"), 1, MidpointRounding.AwayFromZero);
                ds.B5Curr = (float)Math.Round((float)_readData.TryGetValueD29("弧源5电流"), 1, MidpointRounding.AwayFromZero);
                ds.B5Volt = (float)Math.Round((float)_readData.TryGetValueD29("弧源5电压"), 1, MidpointRounding.AwayFromZero);
                ds.B6Curr = (float)Math.Round((float)_readData.TryGetValueD29("弧源6电流"), 1, MidpointRounding.AwayFromZero);
                ds.B6Volt = (float)Math.Round((float)_readData.TryGetValueD29("弧源6电压"), 1, MidpointRounding.AwayFromZero);
                ds.B7Curr = (float)Math.Round((float)_readData.TryGetValueD29("弧源7电流"), 1, MidpointRounding.AwayFromZero);
                ds.B7Volt = (float)Math.Round((float)_readData.TryGetValueD29("弧源7电压"), 1, MidpointRounding.AwayFromZero);
                ds.B8Curr = (float)Math.Round((float)_readData.TryGetValueD29("弧源8电流"), 1, MidpointRounding.AwayFromZero);
                ds.B8Volt = (float)Math.Round((float)_readData.TryGetValueD29("弧源8电压"), 1, MidpointRounding.AwayFromZero);
                ds.BiasVolt = (ushort)_readData.TryGetValueD29("实际电压PYXQ");
                ds.BiasCurr = (float)Math.Round((float)_readData.TryGetValueD19("实际电流PYXQ"), 1, MidpointRounding.AwayFromZero);
                ds.Pluse1Curr = (float)Math.Round((float)_readData.TryGetValueD29("脉冲电源1电流"), 1, MidpointRounding.AwayFromZero);
                ds.Pluse1KHz = (ushort)_readData.TryGetValueD29("脉冲电源1频率");
                ds.Pluse1Duty = (float)Math.Round((float)_readData.TryGetValueD29("脉冲电源1占空比"), 1, MidpointRounding.AwayFromZero);
                ds.Pluse2Curr = (float)Math.Round((float)_readData.TryGetValueD29("脉冲电源2电流"), 1, MidpointRounding.AwayFromZero);
                ds.Pluse2KHz = (ushort)_readData.TryGetValueD29("脉冲电源2频率");
                ds.Pluse2Duty = (float)Math.Round((float)_readData.TryGetValueD29("脉冲电源2占空比"), 1, MidpointRounding.AwayFromZero);
                ds.CoilCurr = (float)Math.Round((float)_readData.TryGetValueD29("线圈总电流"), 1, MidpointRounding.AwayFromZero);
                ds.CoilVolt = (float)Math.Round((float)_readData.TryGetValueD29("线圈总电压"), 1, MidpointRounding.AwayFromZero);
                ds.Penning = (string)_readData.TryGetValueD19("Penning压力");
                ds.CDG100 = (string)_readData.TryGetValueD19("薄膜规100D压力");
                ds.Pirani1 = (string)_readData.TryGetValueD19("管道压力");
                ds.Pirani2 = (string)_readData.TryGetValueD19("腔体Pirani2压力");
                ds.Water1 = (float)Math.Round((float)_readData.TryGetValueD29("腔体水流量"), 1, MidpointRounding.AwayFromZero);
                ds.Water2 = (float)Math.Round((float)_readData.TryGetValueD29("靶1和4水流量"), 1, MidpointRounding.AwayFromZero);
                ds.Water3 = (float)Math.Round((float)_readData.TryGetValueD29("靶2和3水流量"), 1, MidpointRounding.AwayFromZero);
                ds.Water4 = (float)Math.Round((float)_readData.TryGetValueD29("靶5和6水流量"), 1, MidpointRounding.AwayFromZero);
                ds.Water5 = (float)Math.Round((float)_readData.TryGetValueD29("靶7和8水流量"), 1, MidpointRounding.AwayFromZero);
                ds.Water6 = (float)Math.Round((float)_readData.TryGetValueD29("靶座水流量"), 1, MidpointRounding.AwayFromZero);
                ds.Water7 = (float)Math.Round((float)_readData.TryGetValueD29("电源水流量"), 1, MidpointRounding.AwayFromZero);
                ds.Water8 = (float)Math.Round((float)_readData.TryGetValueD29("分子泵水流量"), 1, MidpointRounding.AwayFromZero);
                ds.Water9 = (float)Math.Round((float)_readData.TryGetValueD29("罗茨泵水流量"), 1, MidpointRounding.AwayFromZero);
                _db.Insertable(ds).ExecuteCommand();
            }
        }

        // 4. 核心逻辑：加载数据
        private void LoadData()
        {
            // ⚠️ 关键点：处理时间查询的边界问题
            // 如果用户选的是日期，结束时间需要加上 23:59:59，否则只会查到当天 00:00:00 的数据
            DateTime startTime = dtpStart.Value.Date; // 当天 00:00:00
            DateTime endTime = dtpEnd.Value.Date.AddDays(1).AddSeconds(-1); // 当天 23:59:59

            // ★ 根据当前查看模式选择数据源：实时库(_db) 或 批次库(_viewDb)
            SqlSugarClient queryDb = _viewDb ?? _db;

            // 查询总条数和数据（SqlSugar 同步分页用法）
            var list = queryDb.Queryable<TCData>()
                .Where(o => o.DateTime >= startTime && o.DateTime <= endTime)
                .OrderByDescending(o => o.DateTime) // 默认按时间倒序
                .ToPageList(pageIndex, pageSize, ref totalCount);

            // 绑定数据源
            dataGridView1.SetDataSource(list);

            // 结合上一个问题的解答：绑定数据后设置列标题居中且不排序
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            // 更新分页UI状态
            UpdatePageUI();
        }

        // 5. 更新分页按钮和页码信息
        private void UpdatePageUI()
        {
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            lblPageInfo.Text = $"当前页数: {pageIndex}/{totalPages}  总条数: {totalCount}";
            btnPrev.Enabled = pageIndex > 1;
            btnNext.Enabled = pageIndex < totalPages;
        }

        private void rjButton12_Click(object sender, EventArgs e)
        {
            pageIndex = 1;
            LoadData();
        }

        private void rjButton1_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (pageIndex > 1)
            {
                pageIndex--;
                LoadData();
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            if (pageIndex < totalPages)
            {
                pageIndex++;
                LoadData();
            }
        }

        // ★★★ 以下为本次新增：批次数据库选择控件相关方法 ★★★

        /// <summary>
        /// 初始化批次数据库选择控件（代码动态创建，无需修改 Designer.cs）。
        /// 在画面顶部添加：标签 + 下拉框 + 刷新按钮 + 模式提示标签。
        /// </summary>
        private void InitBatchDbControls()
        {
            // 1. 下移 DataGridView，腾出第二行空间
            dataGridView1.Location = new System.Drawing.Point(3, 98);
            dataGridView1.Size = new System.Drawing.Size(1234, 679);
            // 标签：批次数据库
            lblDbTitle = new Label
            {
                Text = "批次数据库:",
                Font = new Font("楷体", 12F),
                ForeColor = Color.Silver,
                AutoSize = true,
                Location = new Point(3, 55),
                BackColor = Color.Transparent
            };

            // 下拉框：列出所有已保存的批次数据库
            cboBatchDb = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(95, 52),
                Size = new Size(280, 32),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 10F),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };
            cboBatchDb.SelectedIndexChanged += cboBatchDb_SelectedIndexChanged;

            // 刷新按钮
            btnRefreshDb = new Button
            {
                Text = "刷新",
                Font = new Font("楷体", 11F),
                Location = new Point(383, 51),
                Size = new Size(70, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Silver,
                ForeColor = Color.Black
            };
            btnRefreshDb.Click += btnRefreshDb_Click;

            // 模式提示标签
            lblViewMode = new Label
            {
                Text = "[实时数据模式]",
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                ForeColor = Color.LightGreen,
                AutoSize = true,
                Location = new Point(465, 55),
                BackColor = Color.Transparent
            };

            // 添加到画面
            this.Controls.Add(lblDbTitle);
            this.Controls.Add(cboBatchDb);
            this.Controls.Add(btnRefreshDb);
            this.Controls.Add(lblViewMode);

            // ★ 确保新控件在顶层（后添加的默认在底层）
            lblDbTitle.BringToFront();
            cboBatchDb.BringToFront();
            btnRefreshDb.BringToFront();
            lblViewMode.BringToFront();
        }

        /// <summary>
        /// 刷新批次数据库下拉框列表。
        /// 扫描 Databases\ 目录下所有 .db 文件，显示"批次名 (时间, N条)"格式。
        /// </summary>
        private void RefreshDbList()
        {
            // 暂时移除事件，避免填充时触发
            cboBatchDb.SelectedIndexChanged -= cboBatchDb_SelectedIndexChanged;

            object prevSelected = cboBatchDb.SelectedItem;
            cboBatchDb.Items.Clear();

            // 第一项：实时数据（主 SQLite 数据库）
            cboBatchDb.Items.Add(new BatchDbItem
            {
                DbPath = null,
                DisplayName = "实时数据 (SQLite)"
            });

            // 列出所有已保存的批次数据库
            var dbFiles = BatchDbHelper.ListDatabases();
            foreach (var dbPath in dbFiles)
            {
                string name = BatchDbHelper.GetDisplayName(dbPath);
                int count = BatchDbHelper.GetRecordCount(dbPath);
                string time = File.GetLastWriteTime(dbPath).ToString("MM-dd HH:mm");
                string display = $"{name}  ({time}, {count}条)";

                cboBatchDb.Items.Add(new BatchDbItem
                {
                    DbPath = dbPath,
                    DisplayName = display
                });
            }

            // 恢复之前的选择，或默认选第一项
            if (prevSelected != null)
            {
                // 尝试按 DbPath 匹配
                var prev = prevSelected as BatchDbItem;
                if (prev != null)
                {
                    foreach (var item in cboBatchDb.Items)
                    {
                        if (((BatchDbItem)item).DbPath == prev.DbPath)
                        {
                            cboBatchDb.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
            if (cboBatchDb.SelectedIndex < 0)
            {
                cboBatchDb.SelectedIndex = 0;
            }

            // 恢复事件
            cboBatchDb.SelectedIndexChanged += cboBatchDb_SelectedIndexChanged;
        }

        /// <summary>下拉框选择变化时触发：切换查看的数据库</summary>
        private void cboBatchDb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboBatchDb.SelectedItem == null) return;

            var item = (BatchDbItem)cboBatchDb.SelectedItem;

            // 释放旧的 _viewDb
            if (_viewDb != null)
            {
                try { _viewDb.Dispose(); } catch { }
                _viewDb = null;
            }
            _currentViewPath = null;

            if (item.DbPath == null)
            {
                // 实时模式
                lblViewMode.Text = "[实时数据模式]";
                lblViewMode.ForeColor = Color.LightGreen;
            }
            else
            {
                // 批次查看模式
                try
                {
                    _viewDb = BatchDbHelper.CreateClient(item.DbPath);
                    _currentViewPath = item.DbPath;
                    lblViewMode.Text = $"[查看批次: {BatchDbHelper.GetDisplayName(item.DbPath)}]";
                    lblViewMode.ForeColor = Color.LightSkyBlue;
                    _logger.Information("已打开批次数据库: {Path}", item.DbPath);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "打开批次数据库失败: {Name}", item.DisplayName);
                    ModernMessageBox.Show($"打开数据库失败：{ex.Message}", "错误");
                    _viewDb = null;
                    _currentViewPath = null;
                }
            }

            // 重置到第一页并加载数据
            pageIndex = 1;
            LoadData();
        }

        /// <summary>刷新数据库列表按钮点击</summary>
        private void btnRefreshDb_Click(object sender, EventArgs e)
        {
            RefreshDbList();
        }

        /// <summary>
        /// 批次数据库下拉框项。
        /// DbPath 为 null 表示实时数据（主 SQLite 库）；否则为批次数据库文件完整路径。
        /// </summary>
        private class BatchDbItem
        {
            public string DbPath { get; set; }
            public string DisplayName { get; set; }

            public override string ToString() => DisplayName ?? string.Empty;
        }
    }

    /// <summary>
    /// 上升沿下降沿检测
    /// </summary>
    public class EdgeDetector
    {
        private bool _lastState = false;

        /// <summary>
        /// 更新信号状态并检测上升沿
        /// </summary>
        /// <param name="currentState">当前信号状态</param>
        /// <returns>是否发生上升沿</returns>
        public bool DetectRisingEdge(bool currentState)
        {
            bool isRisingEdge = currentState && !_lastState;
            _lastState = currentState;
            return isRisingEdge;
        }

        // 顺带提供下降沿检测
        public bool DetectFallingEdge(bool currentState)
        {
            bool isFallingEdge = !currentState && _lastState;
            _lastState = currentState;
            return isFallingEdge;
        }

        // 重置状态（可选）
        public void Reset()
        {
            _lastState = false;
        }
    }
}
