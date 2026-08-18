using CircularProgressBar;
using HYSDControls;
using Serilog;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace HYSD
{
    public partial class ChooseRp : UserControl, IPollablePage
    {
        private readonly ILogger _logger;
        private readonly IOmronPlcService _plc;
        private readonly SqlSugarClient _db;
        private readonly IReadDataService _readData;
        public ChooseRp(ILogger logger, SqlSugarClient db, IOmronPlcService plc, IReadDataService readData)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            _logger = logger;
            _db = db;
            _plc = plc;
            _readData = readData;
            ProgressBar_A.Value = 0;
            ProgressBar_B.Value = 0;
            ProgressBar_C.Value = 0;
            comboBox1.DataSource = _db.Queryable<HeatRecipeMain>().Select(c => c.RecipeName).ToList();
            comboBox2.DataSource = _db.Queryable<QtksRecipeMain>().Select(c => c.RecipeName).ToList();
            comboBox3.DataSource = _db.Queryable<TCRecipeMain>().Select(c => c.RecipeName).ToList();
            // ★ 轮询由 FormMain.SwitchPage 通过 IPollablePage.StartPolling() 启动
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

        /// <summary>DataUpdated 事件回调（后台读取线程触发）：检查配方变更标志并刷新下拉框</summary>
        private void OnPlcDataUpdated(object sender, EventArgs e)
        {
            CheckRecipeChanges();
        }

        /// <summary>检查 DB 配方变更标志，按需刷新三个下拉框数据源</summary>
        private void CheckRecipeChanges()
        {
            try
            {
                if (AutofacConfig.isHeatChange)
                {
                    comboBox1.Invoke(new Action(() =>
                    {
                        comboBox1.DataSource = null; // Clear existing data source
                        comboBox1.DataSource = _db.Queryable<HeatRecipeMain>().Select(c => c.RecipeName).ToList();
                    }));
                    AutofacConfig.isHeatChange = false;
                }
                if (AutofacConfig.isQtksChange)
                {
                    comboBox2.Invoke(new Action(() =>
                    {
                        comboBox2.DataSource = null; // Clear existing data source
                        comboBox2.DataSource = _db.Queryable<QtksRecipeMain>().Select(c => c.RecipeName).ToList();
                    }));
                    AutofacConfig.isQtksChange = false;
                }
                if (AutofacConfig.isTCRPChange)
                {
                    comboBox3.Invoke(new Action(() =>
                    {
                        comboBox3.DataSource = null; // Clear existing data source
                        comboBox3.DataSource = _db.Queryable<TCRecipeMain>().Select(c => c.RecipeName).ToList();
                    }));
                    AutofacConfig.isTCRPChange = false;
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex.Message);
            }
        }

        List<int> _Htimes = new List<int>();
        List<int> _Atimes = new List<int>();
        List<int> _Ctimes = new List<int>();
        /// <summary>
        /// 丝滑过渡动画的核心方法
        /// </summary>
        /// <param name="startValue">起始值</param>
        /// <param name="targetValue">目标值</param>
        private async Task AnimateProgress(int startValue, int targetValue, CircularProgressBar.CircularProgressBar circularProgressBar1)
        {
            // 用浮点数计算，保证丝滑
            double currentProgress = startValue;

            // 判断是增加还是减少
            bool isIncreasing = targetValue > startValue;

            while (true)
            {
                // 缓动公式
                currentProgress += (targetValue - currentProgress) * 0.1;

                // 判断是否到达目标（增加时超过目标，减少时低于目标）
                if ((isIncreasing && currentProgress >= targetValue) ||
                   (!isIncreasing && currentProgress <= targetValue))
                {
                    currentProgress = targetValue; // 确保精确到达
                }

                // 四舍五入成整数显示
                int displayValue = (int)Math.Round(currentProgress);

                // 同步更新 Value 和 Text
                if (circularProgressBar1.Value != displayValue)
                {
                    circularProgressBar1.Value = displayValue;
                    circularProgressBar1.Text = displayValue + "%";

                    // 【关键】强制UI线程立即重绘，防止画面残留/卡顿
                    circularProgressBar1.Refresh();
                }

                // 如果已经到达目标，跳出循环
                if (Math.Abs(currentProgress - targetValue) < 0.1)
                {
                    break;
                }

                // 等待 16 毫秒 (60FPS)
                await Task.Delay(16);
            }

            // 兜底赋值，确保最终显示绝对正确
            circularProgressBar1.Value = targetValue;
            circularProgressBar1.Text = targetValue + "%";
            circularProgressBar1.Refresh();
        }
        private async void rjButtonD1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboBox1.Text))
            {
                ModernMessageBox.Show("配方名不能为空!");
                return;
            }
            // 防止动画还没播完又点击
            if (!rjButtonD1.Enabled) return;
            rjButtonD1.Enabled = false;

            // ==========================================
            // 第一步：丝滑地降到 0%（消除上一次的进度）
            // ==========================================
            ProgressBar_A.Value = 0; // 立即重置为0，防止残留
            ProgressBar_A.Text = "0%";
            await Task.Delay(600); // 短暂延迟，确保UI更新显示为0%
            // ==========================================
            // 第二步：丝滑地升到 100%
            // ==========================================
            await AnimateProgress(0, 100, ProgressBar_A);

            try
            {
                var recipeId = _db.Queryable<HeatRecipeMain>()
               .Where(m => m.RecipeName == comboBox1.Text)
               .Select(m => m.RecipeID)
               .First(); // 返回 int，若无记录可能为 0

                var dataSets = _db.Queryable<HeatRecipeDataSet>().Where(c => c.RecipeID == recipeId).OrderBy(c => c.SetIndex)
                    .ToList();

                if (dataSets != null && _plc != null && dataSets.Count != 0 && _plc.IsConnected)
                {
                    foreach (var dataSet in dataSets)
                    {
                        var data = new HeatRecipeData
                        {
                            UpHeat = dataSet.UpHeat,
                            DnHeat = dataSet.DnHeat,
                            Rotation = dataSet.Rotation,
                            MFC2SW = dataSet.MFC2SW,
                            MFC2SV = dataSet.MFC2SV,
                            MFC3SW = dataSet.MFC3SW,
                            MFC3SV = dataSet.MFC3SV,
                            ARC7SW = dataSet.ARC7SW,
                            ARC7SV = dataSet.ARC7SV,
                            ARC8SW = dataSet.ARC8SW,
                            ARC8SV = dataSet.ARC8SV,
                            CoolTemp = dataSet.CoolTemp,
                            HTime = dataSet.HTime,
                            LowTemp = dataSet.LowTemp,
                            HighTemp = dataSet.HighTemp,
                            Pressure = dataSet.Pressure,
                            Cool = dataSet.Cool
                        };
                        //分批下载
                        _plc.WriteCustomer($"D{21 + 21 * (dataSet.SetIndex - 1)}", data);
                        if (dataSet.HTime > 0)
                        {
                            _Htimes.Add(dataSet.SetIndex);
                        }
                    }
                    //下载总序
                    _plc.Write("D340", _Htimes.Count > 0 ? _Htimes.Max() : 0);
                    _Htimes.Clear();
                }
                else
                {
                    ProgressBar_A.Value = 0; // 重置进度条
                    ProgressBar_A.Text = "0%";
                }
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
                ProgressBar_A.Value = 0; // 重置进度条
                ProgressBar_A.Text = "0%";
                ModernMessageBox.Show("配方下载出错:" + ex.Message);
            }
            finally
            {
                // 确保无论成功与否都恢复按钮状态
                rjButtonD1.Enabled = true;
            }
        }


        private async void rjButtonD2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboBox2.Text))
            {
                ModernMessageBox.Show("配方名不能为空!");
                return;
            }

            // 防止动画还没播完又点击
            if (!rjButtonD2.Enabled) return;
            rjButtonD2.Enabled = false;

            // ==========================================
            // 第一步：丝滑地降到 0%（消除上一次的进度）
            // ==========================================
            ProgressBar_B.Value = 0; // 立即重置为0，防止残留
            ProgressBar_B.Text = "0%";
            await Task.Delay(600); // 短暂延迟，确保UI更新显示为0%
            // ==========================================
            // 第二步：丝滑地升到 100%
            // ==========================================
            await AnimateProgress(0, 100, ProgressBar_B);

            try
            {
                var recipeId = _db.Queryable<QtksRecipeMain>()
               .Where(m => m.RecipeName == comboBox2.Text)
               .Select(m => m.RecipeID)
               .First(); // 返回 int，若无记录可能为 0

                var dataSets = _db.Queryable<QtksRecipeDataSet>().Where(c => c.RecipeID == recipeId).OrderBy(c => c.SetIndex)
                    .ToList();

                if (dataSets != null && _plc != null && dataSets.Count != 0 && _plc.IsConnected)
                {
                    foreach (var dataSet in dataSets)
                    {
                        var data = new QtksRecipeData
                        {
                            UpHeat = dataSet.UpHeat,
                            DnHeat = dataSet.DnHeat,
                            Rotation = dataSet.Rotation,
                            MFC2SW = dataSet.MFC2SW,
                            MFC2SV = dataSet.MFC2SV,
                            MFC3SW = dataSet.MFC3SW,
                            MFC3SV = dataSet.MFC3SV,
                            CoolTemp = dataSet.CoolTemp,
                            ATime = dataSet.ATime,
                            BiasSW = dataSet.BiasSW,
                            BiasKHz = dataSet.BiasKHz,
                            BiasDuty = dataSet.BiasDuty,
                            BiasThe = dataSet.BiasThe,
                            BiasVolt = dataSet.BiasVolt,
                            ARC7SW = dataSet.ARC7SW,
                            ARC7SV = dataSet.ARC7SV,
                            ARC8SW = dataSet.ARC8SW,
                            ARC8SV = dataSet.ARC8SV,
                            Pluse1SW = dataSet.Pluse1SW,
                            Pluse1Curr = dataSet.Pluse1Curr,
                            Pluse1ONtime = dataSet.Pluse1ONtime,
                            Pluse1OFFtime = dataSet.Pluse1OFFtime,
                            Pluse2SW = dataSet.Pluse2SW,
                            Pluse2Curr = dataSet.Pluse2Curr,
                            Pluse2ONtime = dataSet.Pluse2ONtime,
                            Pluse2OFFtime = dataSet.Pluse2OFFtime,
                            LowTemp = dataSet.LowTemp,
                            HighTemp = dataSet.HighTemp,
                            Pressure = dataSet.Pressure,
                            Cool = dataSet.Cool,
                        };
                        if (dataSet.ATime > 0)
                        {
                            _Atimes.Add(dataSet.SetIndex);
                        }
                        //分批下载
                        _plc.WriteCustomer($"D{434 + 34 * (dataSet.SetIndex - 1)}", data);
                    }
                    //下载总序
                    _plc.Write("D946", _Atimes.Count > 0 ? _Atimes.Max() : 0);
                    _Atimes.Clear();
                }
                else
                {
                    ProgressBar_B.Value = 0; // 重置进度条
                    ProgressBar_B.Text = "0%";
                }
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
                ProgressBar_B.Value = 0; // 重置进度条
                ProgressBar_B.Text = "0%";
                ModernMessageBox.Show("配方下载出错:" + ex.Message);
            }
            finally
            {
                // 确保无论成功与否都恢复按钮状态
                rjButtonD2.Enabled = true;
            }
        }

        private async void rjButtonD3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboBox3.Text))
            {
                ModernMessageBox.Show("配方名不能为空!");
                return;
            }

            // 防止动画还没播完又点击
            if (!rjButtonD3.Enabled) return;
            rjButtonD3.Enabled = false;

            // ==========================================
            // 第一步：丝滑地降到 0%（消除上一次的进度）
            // ==========================================
            ProgressBar_C.Value = 0; // 立即重置为0，防止残留
            ProgressBar_C.Text = "0%";
            await Task.Delay(600); // 短暂延迟，确保UI更新显示为0%
            // ==========================================
            // 第二步：丝滑地升到 100%
            // ==========================================
            await AnimateProgress(0, 100, ProgressBar_C);

            try
            {
                var recipeId = _db.Queryable<TCRecipeMain>()
               .Where(m => m.RecipeName == comboBox3.Text)
               .Select(m => m.RecipeID)
               .First(); // 返回 int，若无记录可能为 0

                var dataSets = _db.Queryable<TCRecipeDataSet>().Where(c => c.RecipeID == recipeId).OrderBy(c => c.SetIndex)
                    .ToList();

                if (dataSets != null && _plc != null && dataSets.Count != 0 && _plc.IsConnected)
                {
                    foreach (var dataSet in dataSets)
                    {
                        var data = new TCRecipeData
                        {
                            UpHeat = dataSet.UpHeat,
                            DnHeat = dataSet.DnHeat,
                            Rotation = dataSet.Rotation,
                            MFC1SW = dataSet.MFC1SW,
                            MFC1SV = dataSet.MFC1SV,
                            MFC2SW = dataSet.MFC2SW,
                            MFC2SV = dataSet.MFC2SV,
                            MFC3SW = dataSet.MFC3SW,
                            MFC3SV = dataSet.MFC3SV,
                            MFC4SW = dataSet.MFC4SW,
                            MFC4SV = dataSet.MFC4SV,
                            CoolTemp = dataSet.CoolTemp,
                            CTime = dataSet.CTime,
                            BiasSW = dataSet.BiasSW,
                            BiasKHz = dataSet.BiasKHz,
                            BiasVolt = dataSet.BiasVolt,
                            ARC1SW = dataSet.ARC1SW,
                            ARC1SV = dataSet.ARC1SV,
                            ARC2SW = dataSet.ARC2SW,
                            ARC2SV = dataSet.ARC2SV,
                            ARC3SW = dataSet.ARC3SW,
                            ARC3SV = dataSet.ARC3SV,
                            ARC4SW = dataSet.ARC4SW,
                            ARC4SV = dataSet.ARC4SV,
                            ARC5SW = dataSet.ARC5SW,
                            ARC5SV = dataSet.ARC5SV,
                            ARC6SW = dataSet.ARC6SW,
                            ARC6SV = dataSet.ARC6SV,
                            CoilSW = dataSet.CoilSW,
                            BiasDuty = dataSet.BiasDuty,
                            BiasThe = dataSet.BiasThe,
                            CoilH = dataSet.CoilH,
                            CoilT0 = dataSet.CoilT0,
                            CoilT1 = dataSet.CoilT1,
                            CoilL = dataSet.CoilL,
                            CoilT2 = dataSet.CoilT2,
                            CoilT3 = dataSet.CoilT3,
                            PF = dataSet.PF,
                            CDG100DSV = dataSet.CDG100DSV,
                            LowTemp = dataSet.LowTemp,
                            HighTemp = dataSet.HighTemp,
                            Pressure = dataSet.Pressure,
                            Cool = dataSet.Cool,
                        };
                        //分批下载
                        _plc.WriteCustomer($"D{2048 + 48 * (dataSet.SetIndex - 1)}", data);
                        if (dataSet.CTime > 0)
                        {
                            _Ctimes.Add(dataSet.SetIndex);
                        }
                    }
                    //下载总序
                    _plc.Write("D348", _Ctimes.Count > 0 ? _Ctimes.Max() : 0);
                    _Ctimes.Clear();
                }
                else
                {
                    ProgressBar_C.Value = 0; // 重置进度条
                    ProgressBar_C.Text = "0%";
                }
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
                ProgressBar_C.Value = 0; // 重置进度条
                ProgressBar_C.Text = "0%";
                ModernMessageBox.Show("配方下载出错:" + ex.Message);
            }
            finally
            {
                // 确保无论成功与否都恢复按钮状态
                rjButtonD3.Enabled = true;
            }
        }
    }
}
