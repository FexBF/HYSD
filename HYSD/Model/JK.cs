using Autofac;
using Newtonsoft.Json.Linq;
using Serilog;
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
    public partial class JK : UserControl, IPollablePage
    {
        private readonly IReadDataService _readData;
        private readonly IOmronPlcService _plc;
        private readonly ILogger _logger;
        private readonly IPLCAddressService _address;
        private readonly Dictionary<string, string> _addressMapping;

        /// <summary>配方监控服务（用于软件启动时自动开启涂层数据监控线程）</summary>
        private readonly IRecipeMonitorService _monitor;

        public JK(IReadDataService readData, IOmronPlcService plc, ILogger logger, IPLCAddressService address, IRecipeMonitorService monitor)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            // ★ 修复闪烁：递归为所有子控件（116 个 Label、16 个 SquareLedControl 等）开启双缓冲。
            // this.DoubleBuffered 只对 JK 本身生效，子控件默认 false，每次 Text 变化直接画到屏幕 → 闪烁。
            UIHelper.EnableDoubleBuffering(this);
            _readData = readData;
            _plc = plc;
            _logger = logger;
            _address = address;
            _monitor = monitor;
            try
            {
                _addressMapping = _address.GetAddressMapping(_address.ReadSheet(), 15, "Name", "Address");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            // ★ 轮询由 FormMain.SwitchPage 通过 IPollablePage.StartPolling() 启动

            // ★★★ 软件启动时自动选中上次生成的批次数据库，并开启涂层数据监控线程 ★★★
            AutoStartMonitoringOnStartup();
        }


        /// <summary>
        /// 软件启动时自动选中上次生成的批次数据库并启动涂层数据监控线程。
        /// 
        /// 实现逻辑：
        /// 1. 通过 BatchDbHelper.GetLatestDatabase() 获取最近一次生成的 .db 文件
        /// 2. 若存在历史数据库：
        ///    - 将批次名称回填到 alphaNumTextBox1（记录名批次输入框）
        ///    - 调用 IRecipeMonitorService.Start() 启动后台监控线程
        ///      （该线程订阅 IReadDataService.DataUpdated 事件，检测"涂层数据记录位"上升沿，
        ///       自动将 TCData 写入当前批次数据库）
        /// 3. 若无历史数据库：仅记录日志，不弹窗打扰用户
        /// 
        /// 设计要点：
        /// - 监控服务为 SingleInstance，重复调用 Start 会先停止旧的再启动新的，安全可重入
        /// - 监控线程独立于页面生命周期，即使 JK 画面未显示，数据仍会被记录
        /// - 不弹任何确认框，实现"打开软件即自动监控"的无感体验
        /// </summary>
        private void AutoStartMonitoringOnStartup()
        {
            try
            {
                string latestDbPath = BatchDbHelper.GetLatestDatabase();
                if (string.IsNullOrEmpty(latestDbPath))
                {
                    _logger.Information("未找到历史批次数据库，跳过自动启动监控（等待用户手动输入批次名称）");
                    return;
                }

                string batchName = BatchDbHelper.GetDisplayName(latestDbPath);

                // 1. 回填批次名称到输入框（选中上次生成的数据库）
                if (alphaNumTextBox1 != null)
                {
                    alphaNumTextBox1.Text = batchName;
                }

                // 2. 启动涂层数据监控线程
                _monitor.Start(latestDbPath, batchName);

                _logger.Information(
                    "软件启动自动监控已开启，批次: {Batch}, 数据库: {Path}",
                    batchName, latestDbPath);
            }
            catch (Exception ex)
            {
                // 自动启动失败不应阻断软件正常使用，仅记录错误日志
                _logger.Error(ex, "软件启动时自动开启涂层数据监控失败");
            }
        }


        // ★ 事件驱动：由 FormMain.SwitchPage 通过 IPollablePage.StartPolling() 订阅 DataUpdated 事件
        private bool _subscribed;

public  void StartPolling()
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
        private long _lastUiUpdateTicks = 0;
        private const long UiUpdateIntervalTicks = 1000L * TimeSpan.TicksPerMillisecond; // 1秒
        private void DoWork()
        {
            try
            {
                if (_plc != null && _plc.IsConnected && _readData.isRunning)
                {
                    // ★ 节流：DataUpdated 每500ms触发一次，但UI刷新降到1秒/次
                    var now = DateTime.UtcNow.Ticks;
                    if (now - _lastUiUpdateTicks < UiUpdateIntervalTicks) return;
                    _lastUiUpdateTicks = now;

                    if (this.IsHandleCreated && !this.IsDisposed)
                        this.BeginInvoke((Action)(() => PlcDataEx()));
                }
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
        }
        private long _lastRecipeReadTicks = 0;
        private const long RecipeReadIntervalTicks = 5000L * TimeSpan.TicksPerMillisecond; // 5秒
        private void PlcDataEx()
        {
            // ★ 优化：批量更新几十个控件前挂起布局，避免每个 Text 赋值都触发一次重排/重绘。
            // 配对使用 ResumeLayout(true) 恢复并立即应用布局。
            this.SuspendLayout();
            try
            {
                //压力计CDG020
                label29.SetTextIfChanged(((string)_readData.TryGetValueD19("CDG20压力")).Trim() + "mbar");
                //前级Pirani1
                label30.SetTextIfChanged(((string)_readData.TryGetValueD19("管道压力")).Trim() + "mbar");
                //腔体Pirani2
                label32.SetTextIfChanged(((string)_readData.TryGetValueD19("腔体Pirani2压力")).Trim() + "mbar");
                //潘宁Penning
                label34.SetTextIfChanged(((string)_readData.TryGetValueD19("Penning压力")).Trim() + "mbar");
                //薄膜规CDG0100D
                label36.SetTextIfChanged(((string)_readData.TryGetValueD19("薄膜规100D压力")).Trim() + "mbar");
                //压力值
                label39.SetTextIfChanged(((string)_readData.TryGetValueD19("压力值")).Trim() + "mbar");
                //转盘电机状态
                switch ((ushort)_readData.TryGetValueD19("转盘电机状态"))
                {
                    case 0:
                        if (this.ZPMotor.ColorA == Color.Silver) break;
                        this.ZPMotor.ColorA = Color.Silver;
                        this.ZPMotor.Flowing = false;
                        break;
                    case 1:
                        if (this.ZPMotor.ColorA == Color.LimeGreen) break;
                        this.ZPMotor.ColorA = Color.LimeGreen;
                        this.ZPMotor.Flowing = true;
                        break;
                    case 3:
                        this.ZPMotor.Flowing = false;
                        break;
                    default:
                        break;
                }
                //薄膜规阀状态
                switch ((ushort)_readData.TryGetValueD15("薄膜规阀状态"))
                {
                    case 2:
                       // if (this.V_BMG.ClosedColor == Color.Silver) break;
                        this.V_BMG.ClosedColor = Color.Silver;
                        this.V_BMG.IsOpen = false;
                        this.TG_BMGF.PipeLineActive = false;
                        break;
                    case 1:
                       // if (this.V_BMG.OpenColor == Color.LimeGreen) break;
                        this.V_BMG.IsOpen = true;
                        this.V_BMG.OpenColor = Color.LimeGreen;
                        this.TG_BMGF.PipeLineActive = true;
                        break;
                    case 3:
                       // if (this.V_BMG.ClosedColor == Color.Red) break;
                        this.V_BMG.ClosedColor = Color.Red;
                        this.V_BMG.IsOpen = false;
                        break;
                    default:
                        break;
                }
                //控制按钮
                switch ((bool)_readData.TryGetValueW("控制按钮"))
                {
                    case true:
                        rjButton4.Text = "压力控制";
                        rjButton4.BackColor = Color.LimeGreen;
                        break;
                    case false:
                        rjButton4.Text = "流量控制";
                        rjButton4.BackColor = Color.Silver;
                        break;
                    default:
                        break;
                }
                //弧源1
                squareLedControl1.IsOn = toggleSwitch1.IsChecked = (bool)_readData.TryGetValueW("弧源1电流开关");
                label45.SetTextIfChanged(((ushort)_readData.TryGetValueD29("弧源1电流设定")).ToString().Trim() + "A");
                label46.SetTextIfChanged(((float)_readData.TryGetValueD29("弧源1电流")).ToString("F1").Trim() + "A");
                label47.SetTextIfChanged(((float)_readData.TryGetValueD29("弧源1电压")).ToString("F1").Trim() + "V");
                //弧源2
                squareLedControl2.IsOn = toggleSwitch2.IsChecked = (bool)_readData.TryGetValueW("弧源2电流开关");
                label52.SetTextIfChanged(((ushort)_readData.TryGetValueD29("弧源2电流设定")).ToString().Trim() + "A");
                label51.SetTextIfChanged(((float)_readData.TryGetValueD29("弧源2电流")).ToString("F1").Trim() + "A");
                label50.SetTextIfChanged(((float)_readData.TryGetValueD29("弧源2电压")).ToString("F1").Trim() + "V");
                //弧源3
                squareLedControl3.IsOn = toggleSwitch3.IsChecked = (bool)_readData.TryGetValueW("弧源3电流开关");
                label57.SetTextIfChanged(((ushort)_readData.TryGetValueD29("弧源3电流设定")).ToString().Trim() + "A");
                label56.SetTextIfChanged(((float)_readData.TryGetValueD29("弧源3电流")).ToString("F1").Trim() + "A");
                label55.SetTextIfChanged(((float)_readData.TryGetValueD29("弧源3电压")).ToString("F1").Trim() + "V");
                //弧源4
                squareLedControl4.IsOn = toggleSwitch4.IsChecked = (bool)_readData.TryGetValueW("弧源4电流开关");
                label62.SetTextIfChanged(((ushort)_readData.TryGetValueD29("弧源4电流设定")).ToString().Trim() + "A");
                label61.SetTextIfChanged(((float)_readData.TryGetValueD29("弧源4电流")).ToString("F1").Trim() + "A");
                label60.SetTextIfChanged(((float)_readData.TryGetValueD29("弧源4电压")).ToString("F1").Trim() + "V");
                //弧源5
                squareLedControl5.IsOn = toggleSwitch5.IsChecked = (bool)_readData.TryGetValueW("弧源5电流开关");
                label67.SetTextIfChanged(((ushort)_readData.TryGetValueD29("弧源5电流设定")).ToString().Trim() + "A");
                label66.SetTextIfChanged(((float)_readData.TryGetValueD29("弧源5电流")).ToString("F1").Trim() + "A");
                label65.SetTextIfChanged(((float)_readData.TryGetValueD29("弧源5电压")).ToString("F1").Trim() + "V");
                //弧源6
                squareLedControl6.IsOn = toggleSwitch6.IsChecked = (bool)_readData.TryGetValueW("弧源6电流开关");
                label72.SetTextIfChanged(((ushort)_readData.TryGetValueD29("弧源6电流设定")).ToString().Trim() + "A");
                label71.SetTextIfChanged(((float)_readData.TryGetValueD29("弧源6电流")).ToString("F1").Trim() + "A");
                label70.SetTextIfChanged(((float)_readData.TryGetValueD29("弧源6电压")).ToString("F1").Trim() + "V");
                //弧源7
                squareLedControl7.IsOn = toggleSwitch7.IsChecked = (bool)_readData.TryGetValueW("弧源7电流开关");
                label77.SetTextIfChanged(((ushort)_readData.TryGetValueD29("弧源7电流设定")).ToString().Trim() + "A");
                label76.SetTextIfChanged(((float)_readData.TryGetValueD29("弧源7电流")).ToString("F1").Trim() + "A");
                label75.SetTextIfChanged(((float)_readData.TryGetValueD29("弧源7电压")).ToString("F1").Trim() + "V");
                //弧源8
                squareLedControl8.IsOn = toggleSwitch8.IsChecked = (bool)_readData.TryGetValueW("弧源8电流开关");
                label82.SetTextIfChanged(((ushort)_readData.TryGetValueD29("弧源8电流设定")).ToString().Trim() + "A");
                label81.SetTextIfChanged(((float)_readData.TryGetValueD29("弧源8电流")).ToString("F1").Trim() + "A");
                label80.SetTextIfChanged(((float)_readData.TryGetValueD29("弧源8电压")).ToString("F1").Trim() + "V");
                //偏压
                squareLedControl9.IsOn = (bool)_readData.TryGetValueW("偏压");
                label87.SetTextIfChanged(((ushort)_readData.TryGetValueD29("电压设定")).ToString().Trim() + "V");
                label86.SetTextIfChanged("偏压:" + ((ushort)_readData.TryGetValueD29("实际电压PYXQ")).ToString().Trim() + "V");
                label85.SetTextIfChanged("电流:" + ((float)_readData.TryGetValueD19("实际电流PYXQ")).ToString("F1").Trim() + "A");
                label98.SetTextIfChanged("电弧:" + ((ushort)_readData.TryGetValueD29("电弧PYXQ")).ToString().Trim());
                //脉冲1
                squareLedControl10.IsOn = (bool)_readData.TryGetValueW("脉冲电源1开关");
                label91.SetTextIfChanged(((ushort)_readData.TryGetValueD29("脉冲电源1电流设定")).ToString().Trim() + "A");
                label90.SetTextIfChanged("电压:" + ((float)_readData.TryGetValueD29("脉冲电源1电压")).ToString("F1") + "V");
                label89.SetTextIfChanged("电流:" + ((float)_readData.TryGetValueD29("脉冲电源1电流")).ToString("F1") + "A");
                label97.SetTextIfChanged("占空比:" + ((float)_readData.TryGetValueD29("脉冲电源1占空比")).ToString("F1") + "%");
                //脉冲2
                squareLedControl11.IsOn = (bool)_readData.TryGetValueW("脉冲电源2开关");
                label95.SetTextIfChanged(((ushort)_readData.TryGetValueD29("脉冲电源2电流设定")).ToString().Trim() + "A");
                label93.SetTextIfChanged("电压:" + ((float)_readData.TryGetValueD29("脉冲电源2电压")).ToString("F1") + "V");
                label94.SetTextIfChanged("电流:" + ((float)_readData.TryGetValueD29("脉冲电源2电流")).ToString("F1") + "A");
                label96.SetTextIfChanged("占空比:" + ((float)_readData.TryGetValueD29("脉冲电源2占空比")).ToString("F1") + "%");
                //加热总电源
                toggleSwitch9.IsChecked = (bool)_readData.TryGetValueC("加热总电源");
                //电源总电源
                toggleSwitch10.IsChecked = (bool)_readData.TryGetValueC("电源总电源");
                //分子泵状态
                toggleSwitch11.IsChecked = (bool)_readData.TryGetValueC("分子泵状态LED");
                //加热模式
                toggleSwitch12.IsChecked = (bool)_readData.TryGetValueW("加热模式");
                //涂层和刻蚀模式
                toggleSwitch14.IsChecked = (bool)_readData.TryGetValueW("镀膜模式");
                //线圈
                squareLedControl16.IsOn = (bool)_readData.TryGetValueW("线圈电源");
                label117.SetTextIfChanged(((ushort)_readData.TryGetValueD29("峰值高电流") / 10.0).ToString("F1").Trim() + "A");
                label116.SetTextIfChanged("电流:" + ((float)_readData.TryGetValueD29("线圈总电流")).ToString("F1").Trim() + "A");
                //冰水机
                squareLedControl12.IsOn = (ushort)_readData.TryGetValueD29("冰水机状态") == 1;
                label106.SetTextIfChanged(((ushort)_readData.TryGetValueD29("冰水机温度设定")).ToString().Trim() + "℃");
                label107.SetTextIfChanged("温度:" + ((float)_readData.TryGetValueD29("冰水机实际温度")).ToString("F1").Trim() + "℃");
                //转架电机
                squareLedControl13.IsOn = (ushort)_readData.TryGetValueD19("转盘电机状态") == 1;
                label109.SetTextIfChanged(((ushort)_readData.TryGetValueD29("转速设定")).ToString().Trim() + "rpm");
                label110.SetTextIfChanged("频率:" + (((ushort)_readData.TryGetValueD29("实际频率")) / 10.0f).ToString("F1").Trim() + "Hz");
                //上加热
                squareLedControl14.IsOn = (bool)_readData.TryGetValueW("加热输出按钮状态");
                label112.SetTextIfChanged(((ushort)_readData.TryGetValueD29("上温度设定")).ToString().Trim() + "℃");
                label113.SetTextIfChanged("上温度:" + ((float)_readData.TryGetValueD29("上温度")).ToString("F1").Trim() + "℃");
                //下加热
                squareLedControl15.IsOn = (bool)_readData.TryGetValueW("加热输出按钮状态");
                label115.SetTextIfChanged(((ushort)_readData.TryGetValueD29("下温度设定")).ToString().Trim() + "℃");
                label118.SetTextIfChanged("下温度:" + ((float)_readData.TryGetValueD29("下温度")).ToString("F1").Trim() + "℃");
                //N2
                thingerFlowControl1.PipeLineActive = valveControl1.IsOpen = (bool)_readData.TryGetValueW("隔膜阀开关1");
                label25.SetTextIfChanged(((ushort)_readData.TryGetValueD29("氮气流量设定")).ToString() + "sccm");
                label26.SetTextIfChanged(((ushort)_readData.TryGetValueD29("氮气实际流量")).ToString() + "sccm");
                label119.ForeColor = valveControl1.IsOpen ? Color.Lime : Color.Silver;
                //H2
                thingerFlowControl2.PipeLineActive = valveControl2.IsOpen = (bool)_readData.TryGetValueW("隔膜阀开关2");
                label38.SetTextIfChanged(((ushort)_readData.TryGetValueD29("氢气流量设定")).ToString() + "sccm");
                label27.SetTextIfChanged(((ushort)_readData.TryGetValueD29("氢气实际流量")).ToString() + "sccm");
                label24.ForeColor = valveControl2.IsOpen ? Color.Lime : Color.Silver;
                //Ar
                thingerFlowControl3.PipeLineActive = valveControl3.IsOpen = (bool)_readData.TryGetValueW("隔膜阀开关3");
                label41.SetTextIfChanged(((ushort)_readData.TryGetValueD29("氩气流量设定")).ToString() + "sccm");
                label40.SetTextIfChanged(((ushort)_readData.TryGetValueD29("氩气实际流量")).ToString() + "sccm");
                label23.ForeColor = valveControl3.IsOpen ? Color.Lime : Color.Silver;
                //X
                thingerFlowControl4.PipeLineActive = valveControl4.IsOpen = (bool)_readData.TryGetValueW("隔膜阀开关4");
                label43.SetTextIfChanged(((ushort)_readData.TryGetValueD29("备用气体流量设定")).ToString() + "sccm");
                label42.SetTextIfChanged(((ushort)_readData.TryGetValueD29("备用气体实际流量")).ToString() + "sccm");
                label22.ForeColor = valveControl4.IsOpen ? Color.Lime : Color.Silver;
                //水流量
                if ((ushort)_readData.TryGetValueD29("冰水机状态") == 0)
                {
                    label17.ForeColor = Color.Red;
                    label17.Text = "异常";
                }
                else if ((ushort)_readData.TryGetValueD29("冰水机状态") == 1)
                {
                    label17.ForeColor = Color.Lime;
                    label17.Text = "正常";
                }
                //电源水温
                label20.Text = ((float)_readData.TryGetValueD29("电源水温")).ToString("F1");
                // ★ 修复：原代码在 UI 线程同步读 PLC（_plc.ReadUInt16），每次最多阻塞 3 秒，
                // 三处合计最多卡死 UI 9 秒，且与 ReadDataService 抢夺非线程安全的 socket。
                // 改为异步读取：投递到线程池，完成后回 UI 线程更新，UI 不再阻塞。
                // 若配方地址已纳入 ReadDataService 的批量读取区域，更推荐直接取缓存。
                var nowTicks = DateTime.UtcNow.Ticks;
                if (nowTicks - _lastRecipeReadTicks >= RecipeReadIntervalTicks)
                {
                    _lastRecipeReadTicks = nowTicks;
                    ReadRecipeAsync("D340", label5);
                    ReadRecipeAsync("D946", label6);
                    ReadRecipeAsync("D348", label7);
                }
                //加热时间
                txt_Heatime.Text = ((ushort)_readData.TryGetValueD10("加热时间")).ToString();
                label13.ForeColor = txt_Heatime.Text == "0" ? Color.Silver : Color.Lime;
                //刻蚀时间
                txt_KSTime.Text = ((ushort)_readData.TryGetValueD10("刻蚀时间")).ToString();
                label12.ForeColor = txt_KSTime.Text == "0" ? Color.Silver : Color.Lime;
                //涂层时间
                txt_TCTime.Text = ((ushort)_readData.TryGetValueD10("涂层时间")).ToString();
                label11.ForeColor = txt_TCTime.Text == "0" ? Color.Silver : Color.Lime;
                //当前序
                label15.SetTextIfChanged(((ushort)_readData.TryGetValueD19("当前序")).ToString());
            }
            finally
            {
                this.ResumeLayout(true);
            }
        }

        /// <summary>
        /// 异步读取配方值并回 UI 线程更新 Label，避免阻塞 UI。
        /// </summary>
        private async void ReadRecipeAsync(string address, Label target)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (target == null || target.IsDisposed) return;

            ushort value = 0;
            bool ok = false;
            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                {
                    var r = _plc.ReadUInt16(address);
                    if (r.IsSuccess) { value = r.Content; ok = true; }
                });
            }
            catch { return; }

            if (!ok) return;
            if (target.IsDisposed) return;
            try
            {
                if (target.InvokeRequired)
                    target.BeginInvoke(new Action(() => target.Text = value.ToString()));
                else
                    target.Text = value.ToString();
            }
            catch { /* 控件已释放 */ }
        }

        private void ConKeyDown(string address, ushort value)
        {
            try
            {
                if (_plc != null && _plc.IsConnected)
                {
                    _plc.Write(address, value);
                    // 将焦点设置到窗体，当前控件失去焦点
                    this.ActiveControl = null;
                }
                // 将焦点设置到窗体，当前控件失去焦点
                this.ActiveControl = null;
            }
            catch (Exception ex)
            {
                _logger.Debug(ex.Message);
            }
        }
        private void txt_Heatime_NumPadOkPressed(object sender, HYSDControls.NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["加热时间"], Convert.ToUInt16(e.DecimalValue.Value));
        }

        private void txt_KSTime_NumPadOkPressed(object sender, HYSDControls.NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["刻蚀时间"], Convert.ToUInt16(e.DecimalValue.Value));
        }

        private void txt_TCTime_NumPadOkPressed(object sender, HYSDControls.NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["涂层时间"], Convert.ToUInt16(e.DecimalValue.Value));
        }

        // ★★★ 以下为本次新增功能：alphaNumTextBox1 输入批次名称 → 创建 SQLite 数据库 → 启动配方监控 ★★★

        /// <summary>
        /// alphaNumTextBox1 的 OK 事件处理：
        /// 1. 读取输入的批次名称
        /// 2. 创建以该名称命名的 SQLite 数据库（Databases\批次名.db）
        /// 3. 弹出确认对话框，询问是否启动配方监控
        /// 4. 确认后启动后台监控线程，自动记录配方运行数据
        /// </summary>
        private void alphaNumTextBox1_AlphaNumPadOkPressed(object sender, EventArgs e)
        {
            try
            {
                // 1. 读取并校验批次名称
                string batchName = alphaNumTextBox1.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(batchName))
                {
                    ModernMessageBox.Show("请输入批次名称！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 2. 创建 SQLite 批次数据库
                string dbPath = BatchDbHelper.CreateDatabase(batchName);
                string fileName = Path.GetFileName(dbPath);

                // 3. 确认对话框——询问是否启动监控线程
                var result = ModernMessageBox.Show(
                    $"已创建批次数据库：{fileName}\n" +
                    $"批次名称：{batchName}\n\n" +
                    $"是否启动配方数据监控？\n" +
                    $"（启动后，每次涂层配方运行将自动记录数据到该数据库）",
                    "确认启动监控",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    _logger.Information("用户取消启动配方监控，批次: {Batch}", batchName);
                    return;
                }

                // 4. 启动配方监控服务（后台线程，独立于页面生命周期）
                var monitor = AutofacConfig.Container.Resolve<IRecipeMonitorService>();
                monitor.Start(dbPath, batchName);

                ModernMessageBox.Show(
                    $"批次 [{batchName}] 监控已启动！\n\n" +
                    $"配方运行数据将自动记录到：\n{fileName}\n\n" +
                    $"可在「历史数据」画面选择该批次查看记录内容。",
                    "启动成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                _logger.Information("用户启动配方监控，批次: {Batch}, 数据库: {Path}", batchName, dbPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "启动配方监控失败");
                ModernMessageBox.Show(
                    $"启动失败：{ex.Message}",
                    "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
