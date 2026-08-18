using HYSDControls;
using LockDemo;
using Newtonsoft.Json.Linq;
using QuarterRingDemo;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using thinger.CommonControlLib;
using TriangleDemo;
using ValveDemo;
using YourNamespace;

namespace HYSD
{
    public partial class Vacuum : UserControl, IPollablePage
    {
        private readonly IOmronPlcService _plc;
        private readonly ILogger _logger;
        private readonly IPLCAddressService _address;
        private readonly Dictionary<string, string> _addressMapping;
        private readonly IReadDataService _readData;
        private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer() { Interval = 1000 };

        public Vacuum(IOmronPlcService plc, ILogger logger, IPLCAddressService address, IReadDataService readData)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            _plc = plc;
            _logger = logger;
            _address = address;
            _readData = readData;
            try
            {
                _addressMapping = _address.GetAddressMapping(_address.ReadSheet(), 0, "Name", "Address");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            rjButton_Close_Click(null, null);
            _timer.Tick += (s, e) =>
            {
                _State5 = !_State5;
                this.ZPMotor.ColorA = _State5 ? Color.Red : Color.Silver;
            };
            // ★ 轮询由 FormMain.SwitchPage 通过 IPollablePage.StartPolling() 启动
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

        private void DoWork()
        {
            try
            {
                if (_plc != null && _plc.IsConnected && _readData.isRunning)
                {
                    if (this.IsHandleCreated && !this.IsDisposed)
                        this.BeginInvoke((Action)(() => PlcDataEx()));
                }
                else
                {
                    _State1 = false;// 连接断开时重置状态，等待重新连接后数据初始化
                }
            }
            catch (Exception ex)
            {
                // 处理异常，例如日志记录
                _logger.Debug(ex.Message);
            }
        }
        bool _State1 = false, _State5;
        private void PlcDataEx()
        {
            //数据初始化 只读一次
            if (!_State1)
            {
                //涂层后关机按钮状态
                _TCHGJState = (bool)_readData.TryGetValueW("涂层后关机按钮状态");
                //粗抽状态
                _CCState = (bool)_readData.TryGetValueW("粗抽按钮状态");
                //高真空状态
                _GZKState = (bool)_readData.TryGetValueW("高真空按钮状态");
                //放气状态
                _FQState = (bool)_readData.TryGetValueW("放气按钮状态");
                //停机状态
                _TJState = (bool)_readData.TryGetValueW("停机按钮状态");
                //加热供电按钮状态
                _HeatState = (bool)_readData.TryGetValueW("加热供电按钮状态");
                //加热输出按钮状态
                _HeatOutState = (bool)_readData.TryGetValueW("加热输出按钮状态");

                _State1 = true;
            }
            //转速设定
            if (!txt_Speed.IsEditing)
                this.txt_Speed.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("转速设定")).ToString().Trim());
            //出炉温度设定
            if (!txt_CRWD.IsEditing)
                this.txt_CRWD.UpdateValueFromPlc(((float)_readData.TryGetValueD29("出炉温度设定")).ToString().Trim());
            //上温度设定
            if (!txt_SWD.IsEditing)
                this.txt_SWD.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("上温度设定")).ToString().Trim());
            //下温度设定
            if (!txt_XWD.IsEditing)
                this.txt_XWD.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("下温度设定")).ToString().Trim());
            //管道压力
            this.lbl_Piranil.Text = ((string)_readData.TryGetValueD19("管道压力")) + "mbar";
            //分子泵转速百分比
            this.lbl_FzSpeed.Text = ((ushort)_readData.TryGetValueD29("分子泵转速百分比")).ToString().Trim() + "%";
            //薄膜规100D压力
            this.lbl_100D.Text = ((string)_readData.TryGetValueD19("薄膜规100D压力")) + "mbar";
            //腔体Pirani2压力
            this.lbl_Piranil2.Text = ((string)_readData.TryGetValueD19("腔体Pirani2压力")) + "mbar";
            //Penning压力
            this.lbl_Penning.Text = ((string)_readData.TryGetValueD19("Penning压力")) + "mbar";
            //CDG20压力
            this.lbl_CDG20.Text = ((string)_readData.TryGetValueD19("CDG20压力")) + "mbar";
            //实际频率
            this.lbl_ActualPL.Text = (((ushort)_readData.TryGetValueD29("实际频率")) / 10.0f).ToString("F1").Trim();
            //上温度
            this.lbl_SWD.Text = ((float)_readData.TryGetValueD29("上温度")).ToString("F1").Trim();
            //下温度
            this.lbl_XWD.Text = ((float)_readData.TryGetValueD29("下温度")).ToString("F1").Trim();

            //旋片泵状态
            switch ((ushort)_readData.TryGetValueD15("旋片泵状态"))
            {
                case 0:
                    if (this.pumpControl1.State == PumpState.Off) break;
                    this.pipeControl1.IsFlowing = false;
                    this.pumpControl1.State = PumpState.Off;
                    break;
                case 1:
                    if (this.pumpControl1.State == PumpState.Running) break;
                    this.pipeControl1.IsFlowing = true;
                    this.pumpControl1.State = PumpState.Running;
                    break;
                case 2:
                    if (this.pumpControl1.State == PumpState.Overload) break;
                    this.pipeControl1.IsFlowing = true;
                    this.pumpControl1.State = PumpState.Overload;
                    break;
                default:
                    break;
            }
            //罗茨泵状态
            switch ((ushort)_readData.TryGetValueD15("罗茨泵状态"))
            {
                case 0:
                    if (this.pumpControl2.State == PumpState.Off) break;
                    this.pipeControl2.IsFlowing = false;
                    this.pipeControl4.IsFlowing = false;
                    this.pipeControl5.IsFlowing = false;
                    this.pumpControl2.State = PumpState.Off;
                    break;
                case 1:
                    if (this.pumpControl2.State == PumpState.Running) break;
                    this.pipeControl2.IsFlowing = true;
                    this.pipeControl4.IsFlowing = true;
                    this.pipeControl5.IsFlowing = true;
                    this.pumpControl2.State = PumpState.Running;
                    break;
                case 2:
                    if (this.pumpControl2.State == PumpState.Overload) break;
                    this.pipeControl2.IsFlowing = true;
                    this.pipeControl4.IsFlowing = true;
                    this.pipeControl5.IsFlowing = true;
                    this.pumpControl2.State = PumpState.Overload;
                    break;
                default:
                    break;
            }
            //分子泵状态
            switch ((ushort)_readData.TryGetValueD15("分子泵状态"))
            {
                case 0:
                    if (this.pumpControl3.State == PumpState.Off) break;
                    this.pipeControl10.IsFlowing = false;
                    this.pumpControl3.State = PumpState.Off;
                    break;
                case 1:
                    if (this.pumpControl3.State == PumpState.Running) break;
                    this.pipeControl10.IsFlowing = true;
                    this.pumpControl3.State = PumpState.Running;
                    break;
                case 2:
                    if (this.pumpControl3.State == PumpState.Overload) break;
                    this.pipeControl10.IsFlowing = true;
                    this.pumpControl3.State = PumpState.Overload;
                    break;
                default:
                    break;
            }
            //下阀状态
            switch ((ushort)_readData.TryGetValueD15("下阀状态"))
            {
                case 0:
                    //if (this.V_XF.IsOpen == false) break;
                    this.V_XF.ClosedColor = Color.Silver;
                    this.V_XF.IsOpen = false;
                    this.pipeControl3.IsFlowing = false;
                    break;
                case 1:
                    //if (this.V_XF.IsOpen == true) break;
                    this.V_XF.OpenColor = Color.LimeGreen;
                    this.V_XF.IsOpen = true;
                    this.pipeControl3.IsFlowing = true;
                    break;
                case 3:
                    //if (this.V_XF.ClosedColor == Color.Red) break;
                    this.V_XF.ClosedColor = Color.Red;
                    this.V_XF.IsOpen = false;
                    break;
                default:
                    break;
            }
            //上阀状态
            switch ((ushort)_readData.TryGetValueD15("上阀状态"))
            {
                case 0:
                    //if (this.V_SF.IsOpen == false) break;
                    this.V_SF.ClosedColor = Color.Silver;
                    this.V_SF.IsOpen = false;
                    this.pipeControl8.IsFlowing = false;
                    break;
                case 1:
                    //if (this.V_SF.IsOpen == true) break;
                    this.V_SF.IsOpen = true;
                    this.V_SF.OpenColor = Color.LimeGreen;
                    this.pipeControl8.IsFlowing = true;
                    break;
                case 3:
                    //if (this.V_SF.ClosedColor == Color.Red) break;
                    this.V_SF.ClosedColor = Color.Red;
                    this.V_SF.IsOpen = false;
                    break;
                default:
                    break;
            }
            //插板阀状态
            switch ((ushort)_readData.TryGetValueD15("插板阀状态"))
            {
                case 2:
                    //if (this.V_CBF.ClosedColor == Color.Silver) break;
                    this.V_CBF.ClosedColor = Color.Silver;
                    this.V_CBF.IsOpen = false;
                    this.pipeControl7.IsFlowing = false;
                    break;
                case 1:
                    //if (this.V_CBF.OpenColor == Color.LimeGreen) break;
                    this.V_CBF.IsOpen = true;
                    this.V_CBF.OpenColor = Color.LimeGreen;
                    this.pipeControl7.IsFlowing = true;
                    break;
                case 3:
                    //if (this.V_CBF.ClosedColor == Color.Red) break;
                    this.V_CBF.ClosedColor = Color.Red;
                    this.V_CBF.IsOpen = false;
                    break;
                default:
                    break;
            }
            //放气阀状态
            switch ((ushort)_readData.TryGetValueD15("放气阀状态"))
            {
                case 0:
                    //if (this.V_FQF.ClosedColor == Color.Silver) break;
                    this.V_FQF.ClosedColor = Color.Silver;
                    this.V_FQF.IsOpen = false;
                    this.pipeControl6.IsFlowing = false;
                    this.pipeControl9.IsFlowing = false;
                    break;
                case 1:
                    //if (this.V_FQF.OpenColor == Color.LimeGreen) break;
                    this.V_FQF.IsOpen = true;
                    this.V_FQF.OpenColor = Color.LimeGreen;
                    this.pipeControl6.IsFlowing = true;
                    this.pipeControl9.IsFlowing = true;
                    break;
                case 3:
                    //if (this.V_FQF.ClosedColor == Color.Red) break;
                    this.V_FQF.ClosedColor = Color.Red;
                    this.V_FQF.IsOpen = false;
                    break;
                default:
                    break;
            }
            //薄膜规阀状态
            switch ((ushort)_readData.TryGetValueD15("薄膜规阀状态"))
            {
                case 2:
                    //if (this.V_BMG.ClosedColor == Color.Silver) break;
                    this.V_BMG.ClosedColor = Color.Silver;
                    this.V_BMG.IsOpen = false;
                    this.TG_BMGF.PipeLineActive = false;
                    break;
                case 1:
                    //if (this.V_BMG.OpenColor == Color.LimeGreen) break;
                    this.V_BMG.IsOpen = true;
                    this.V_BMG.OpenColor = Color.LimeGreen;
                    this.TG_BMGF.PipeLineActive = true;
                    break;
                case 3:
                    //if (this.V_BMG.ClosedColor == Color.Red) break;
                    this.V_BMG.ClosedColor = Color.Red;
                    this.V_BMG.IsOpen = false;
                    break;
                default:
                    break;
            }
            //分子泵供电状态LED
            if ((bool)_readData.TryGetValueC("分子泵供电状态LED"))
            {
                if (this.Led_FzPower.LedState != 1)
                    this.Led_FzPower.LedState = 1;
            }
            else
            {
                if (this.Led_FzPower.LedState != 0)
                    this.Led_FzPower.LedState = 0;
            }
            //分子泵状态LED
            if ((bool)_readData.TryGetValueC("分子泵状态LED"))
            {
                if (this.Led_FzState.LedState != 1)
                    this.Led_FzState.LedState = 1;
            }
            else
            {
                if (this.Led_FzState.LedState != 0)
                    this.Led_FzState.LedState = 0;
            }
            //空气压缩压力状态LED
            if ((bool)_readData.TryGetValueC("空气压缩压力状态LED"))
            {
                if (this.Led_AirState.LedState != 1)
                    this.Led_AirState.LedState = 1;
            }
            else
            {
                if (this.Led_AirState.LedState != 0)
                    this.Led_AirState.LedState = 0;
            }

            //转盘电机状态
            switch ((ushort)_readData.TryGetValueD19("转盘电机状态"))
            {
                case 0:
                    _timer.Enabled = false;
                    if (this.ZPMotor.ColorA == Color.Silver) break;
                    this.ZPMotor.ColorA = Color.Silver;
                    this.ZPMotor.Flowing = false;
                    break;
                case 1:
                    _timer.Enabled = false;
                    if (this.ZPMotor.ColorA == Color.LimeGreen) break;
                    this.ZPMotor.ColorA = Color.LimeGreen;
                    this.ZPMotor.Flowing = true;
                    break;
                case 3:
                    _timer.Enabled = true;
                    this.ZPMotor.Flowing = false;
                    break;
                default:
                    break;
            }
            //原点显示
            if ((bool)_readData.TryGetValueC("原点显示"))
            {
                if (this.Tg_ZP.Color != Color.LimeGreen)
                    this.Tg_ZP.Color = Color.LimeGreen;
            }
            else
            {
                if (this.Tg_ZP.Color != Color.Silver)
                    this.Tg_ZP.Color = Color.Silver;
            }
            //转盘电机启动按钮
            if ((bool)_readData.TryGetValueW("转盘电机启动按钮"))
            {
                if (this.RjButton_Start.BackColor != Color.LimeGreen)
                {
                    this.RjButton_Start.BackColor = Color.LimeGreen;
                    this.RjButton_Start.Text = "运行中";
                }
            }
            else
            {
                if (this.RjButton_Start.BackColor != Color.Silver)
                {
                    this.RjButton_Start.BackColor = Color.Silver;
                    this.RjButton_Start.Text = "启动";
                }
            }
            //转盘电机回原点按钮
            if ((bool)_readData.TryGetValueW("转盘电机回原点按钮"))
            {
                if (this.RjButton_Home.BackColor != Color.LimeGreen)
                {
                    this.RjButton_Home.BackColor = Color.LimeGreen;
                    this.RjButton_Home.Text = "回原点中";
                }
            }
            else
            {
                if (this.RjButton_Home.BackColor != Color.Silver)
                {
                    this.RjButton_Home.BackColor = Color.Silver;
                    this.RjButton_Home.Text = "回原点";
                }
            }
            //加热供电按钮状态
            if ((bool)_readData.TryGetValueW("加热供电按钮状态"))
            {
                if (this.RjButton_HeatPower.BackColor != Color.LimeGreen)
                    this.RjButton_HeatPower.BackColor = Color.LimeGreen;
            }
            else
            {
                if (this.RjButton_HeatPower.BackColor != Color.Silver)
                    this.RjButton_HeatPower.BackColor = Color.Silver;
            }
            //加热输出按钮状态
            if ((bool)_readData.TryGetValueW("加热输出按钮状态"))
            {
                if (this.RjButton_HeatOut.BackColor != Color.LimeGreen)
                    this.RjButton_HeatOut.BackColor = Color.LimeGreen;
            }
            else
            {
                if (this.RjButton_HeatOut.BackColor != Color.Silver)
                    this.RjButton_HeatOut.BackColor = Color.Silver;
            }
            //粗抽完成状态LED
            if ((bool)_readData.TryGetValueW("粗抽完成状态LED"))
            {
                if (this.Led_CC.LedState != 1)
                    this.Led_CC.LedState = 1;
            }
            else
            {
                if (this.Led_CC.LedState != 0)
                    this.Led_CC.LedState = 0;
            }
            //粗抽按钮状态
            if ((bool)_readData.TryGetValueW("粗抽按钮状态"))
            {
                if (this.rjButton_CC.BackColor != Color.LimeGreen)
                    this.rjButton_CC.BackColor = Color.LimeGreen;
            }
            else
            {
                if (this.rjButton_CC.BackColor != Color.Silver)
                    this.rjButton_CC.BackColor = Color.Silver;
            }
            //高真空按钮状态
            if ((bool)_readData.TryGetValueW("高真空按钮状态"))
            {
                if (this.rjButton_GZK.BackColor != Color.LimeGreen)
                    this.rjButton_GZK.BackColor = Color.LimeGreen;
            }
            else
            {
                if (this.rjButton_GZK.BackColor != Color.Silver)
                    this.rjButton_GZK.BackColor = Color.Silver;
            }
            //高真空完成状态LED
            if ((bool)_readData.TryGetValueW("高真空完成状态LED"))
            {
                if (this.Led_GZK.LedState != 1)
                    this.Led_GZK.LedState = 1;
            }
            else
            {
                if (this.Led_GZK.LedState != 0)
                    this.Led_GZK.LedState = 0;
            }
            //放气按钮状态
            if ((bool)_readData.TryGetValueW("放气按钮状态"))
            {
                if (this.rjButton_FQ.BackColor != Color.LimeGreen)
                    this.rjButton_FQ.BackColor = Color.LimeGreen;
            }
            else
            {
                if (this.rjButton_FQ.BackColor != Color.Silver)
                    this.rjButton_FQ.BackColor = Color.Silver;
            }
            //停机按钮状态
            if ((bool)_readData.TryGetValueW("停机按钮状态"))
            {
                if (this.RjButton_Stop.BackColor != Color.LimeGreen)
                    this.RjButton_Stop.BackColor = Color.LimeGreen;
            }
            else
            {
                if (this.RjButton_Stop.BackColor != Color.Silver)
                    this.RjButton_Stop.BackColor = Color.Silver;
            }
            //涂层后关机按钮状态
            if ((bool)_readData.TryGetValueW("涂层后关机按钮状态"))
            {
                if (this.rjButton_TCHGJ.BackColor != Color.LimeGreen)
                    this.rjButton_TCHGJ.BackColor = Color.LimeGreen;
            }
            else
            {
                if (this.rjButton_TCHGJ.BackColor != Color.Silver)
                    this.rjButton_TCHGJ.BackColor = Color.Silver;
            }
            //旋片泵启动按钮状态
            if ((bool)_readData.TryGetValueW("旋片泵启动按钮状态"))
            {
                if (this.rjButton10.BackColor != Color.LimeGreen)
                    this.rjButton10.BackColor = Color.LimeGreen;
            }
            else
            {
                if (this.rjButton10.BackColor != Color.Silver)
                    this.rjButton10.BackColor = Color.Silver;
            }
            ////旋片泵关闭按钮
            //if ((bool)_readData.TryGetValueW("旋片泵关闭按钮状态"))
            //{
            //    if (this.rjButton11.BackColor != Color.LimeGreen)
            //        this.rjButton11.BackColor = Color.LimeGreen;
            //}
            //else
            //{
            //    if (this.rjButton11.BackColor != Color.Silver)
            //        this.rjButton11.BackColor = Color.Silver;
            //}
            //罗茨泵启动按钮状态
            if ((bool)_readData.TryGetValueW("罗茨泵启动按钮状态"))
            {
                if (this.rjButton12.BackColor != Color.LimeGreen)
                    this.rjButton12.BackColor = Color.LimeGreen;
            }
            else
            {
                if (this.rjButton12.BackColor != Color.Silver)
                    this.rjButton12.BackColor = Color.Silver;
            }
            //分子泵供电按钮状态
            if ((bool)_readData.TryGetValueW("分子泵供电按钮状态"))
            {
                if (this.rjButton14.BackColor != Color.LimeGreen)
                    this.rjButton14.BackColor = Color.LimeGreen;
            }
            else
            {
                if (this.rjButton14.BackColor != Color.Silver)
                    this.rjButton14.BackColor = Color.Silver;
            }
            //分子泵启动按钮状态
            if ((bool)_readData.TryGetValueW("分子泵启动按钮状态"))
            {
                if (this.rjButton16.BackColor != Color.LimeGreen)
                    this.rjButton16.BackColor = Color.LimeGreen;
            }
            else
            {
                if (this.rjButton16.BackColor != Color.Silver)
                    this.rjButton16.BackColor = Color.Silver;
            }
            //上阀开启按钮状态
            if ((bool)_readData.TryGetValueW("上阀开启按钮状态"))
            {
                if (this.rjButton18.BackColor != Color.LimeGreen)
                    this.rjButton18.BackColor = Color.LimeGreen;
            }
            else
            {
                if (this.rjButton18.BackColor != Color.Silver)
                    this.rjButton18.BackColor = Color.Silver;
            }
            //下阀开启按钮状态
            if ((bool)_readData.TryGetValueW("下阀开启按钮状态"))
            {
                if (this.rjButton20.BackColor != Color.LimeGreen)
                    this.rjButton20.BackColor = Color.LimeGreen;
            }
            else
            {
                if (this.rjButton20.BackColor != Color.Silver)
                    this.rjButton20.BackColor = Color.Silver;
            }
            //CDG阀开启按钮状态
            if ((bool)_readData.TryGetValueW("CDG阀开启按钮状态"))
            {
                if (this.rjButton22.BackColor != Color.LimeGreen)
                    this.rjButton22.BackColor = Color.LimeGreen;
            }
            else
            {
                if (this.rjButton22.BackColor != Color.Silver)
                    this.rjButton22.BackColor = Color.Silver;
            }
            //插板阀开启按钮状态
            if ((bool)_readData.TryGetValueW("插板阀开启按钮状态"))
            {
                if (this.rjButton24.BackColor != Color.LimeGreen)
                    this.rjButton24.BackColor = Color.LimeGreen;
            }
            else
            {
                if (this.rjButton24.BackColor != Color.Silver)
                    this.rjButton24.BackColor = Color.Silver;
            }
            //放气阀开启按钮状态
            if ((bool)_readData.TryGetValueW("放气阀开启按钮状态"))
            {
                if (this.rjButton26.BackColor != Color.LimeGreen)
                    this.rjButton26.BackColor = Color.LimeGreen;
            }
            else
            {
                if (this.rjButton26.BackColor != Color.Silver)
                    this.rjButton26.BackColor = Color.Silver;
            }
            //前门状态
            switch ((bool)_readData.TryGetValueC("前门状态"))
            {
                case false:
                    if (!this.LC_QM.IsLocked) break;
                    this.LC_QM.IsLocked = false;
                    break;
                case true:
                    if (this.LC_QM.IsLocked) break;
                    this.LC_QM.IsLocked = true;
                    break;
                default:
                    break;
            }
            //上气阀状态
            switch ((bool)_readData.TryGetValueC("上气阀状态"))
            {
                case false:
                    if (!this.LC_SQF.IsLocked) break;
                    this.LC_SQF.IsLocked = false;
                    break;
                case true:
                    if (this.LC_SQF.IsLocked) break;
                    this.LC_SQF.IsLocked = true;
                    break;
                default:
                    break;
            }
            //下气阀状态
            switch ((bool)_readData.TryGetValueC("下气阀状态"))
            {
                case false:
                    if (!this.LC_XQF.IsLocked) break;
                    this.LC_XQF.IsLocked = false;
                    break;
                case true:
                    if (this.LC_XQF.IsLocked) break;
                    this.LC_XQF.IsLocked = true;
                    break;
                default:
                    break;
            }
        }

        bool _HeatState, _HeatOutState, _CCState, _GZKState, _FQState, _TJState, _TCHGJState;

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

        private void CommonWrite(string address, bool value)
        {
            if (_plc == null || !_plc.IsConnected) return;
            try
            {
                _plc.Write(address, value);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
        }

        private void rjButton_Open_Click(object sender, EventArgs e)
        {
            this.panel19.Visible = true;
            this.rjButton_Open.Text = "手动设置";
            this.rjButton_Open.BackColor = Color.LimeGreen;
        }

        private void rjButton_Close_Click(object sender, EventArgs e)
        {
            this.panel19.Visible = false;
            this.rjButton_Open.Text = "手动开";
            this.rjButton_Open.BackColor = Color.Silver;
        }

        private void RjButton_HeatPower_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            _HeatState = !_HeatState;
            CommonWrite(_addressMapping["加热供电按钮状态"], _HeatState);
        }

        private void RjButton_HeatOut_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            _HeatOutState = !_HeatOutState;
            CommonWrite(_addressMapping["加热输出按钮状态"], _HeatOutState);
        }

        private void RjButton_Start_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            CommonWrite(_addressMapping["转盘电机启动按钮"], true);
        }
        //旋片泵启动按钮
        private void rjButton10_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            CommonWrite(_addressMapping["旋片泵启动按钮状态"], true);
        }

        bool _CloseState1;
        //旋片泵关闭按钮
        private async void rjButton11_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_CloseState1) return;
            _CloseState1 = true;
            try
            {
                CommonWrite(_addressMapping["旋片泵关闭按钮状态"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["旋片泵关闭按钮状态"], false);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
            finally
            {
                _CloseState1 = false;
            }
        }
        //罗茨泵启动按钮
        private void rjButton12_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            CommonWrite(_addressMapping["罗茨泵启动按钮状态"], true);
        }

        //罗茨泵关闭按钮
        private void rjButton13_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            CommonWrite(_addressMapping["罗茨泵启动按钮状态"], false);
        }

        //分子泵供电按钮
        private void rjButton14_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            CommonWrite(_addressMapping["分子泵供电按钮状态"], true);
        }
        //分子泵关电按钮
        private void rjButton15_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            CommonWrite(_addressMapping["分子泵供电按钮状态"], false);
        }
        //分子泵启动按钮
        private void rjButton16_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            CommonWrite(_addressMapping["分子泵启动按钮状态"], true);
        }
        //分子泵关闭按钮
        private void rjButton17_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            CommonWrite(_addressMapping["分子泵启动按钮状态"], false);
        }
        //上阀开启按钮
        private void rjButton18_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            CommonWrite(_addressMapping["上阀开启按钮状态"], true);
        }
        //上阀关闭按钮
        private void rjButton19_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            CommonWrite(_addressMapping["上阀开启按钮状态"], false);
        }
        //下阀开启按钮
        private void rjButton20_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            CommonWrite(_addressMapping["下阀开启按钮状态"], true);
        }
        //下阀关闭按钮
        private void rjButton21_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            CommonWrite(_addressMapping["下阀开启按钮状态"], false);
        }
        //CDG阀开启按钮
        private void rjButton22_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            CommonWrite(_addressMapping["CDG阀开启按钮状态"], true);
        }
        //CDG阀关闭按钮
        private void rjButton23_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            CommonWrite(_addressMapping["CDG阀开启按钮状态"], false);
        }
        //插板阀开启按钮
        private void rjButton24_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            CommonWrite(_addressMapping["插板阀开启按钮状态"], true);
        }
        //插板阀关闭按钮
        private void rjButton25_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            CommonWrite(_addressMapping["插板阀开启按钮状态"], false);
        }
        //放气阀开启按钮
        private void rjButton26_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            CommonWrite(_addressMapping["放气阀开启按钮状态"], true);
        }
        //放气阀关闭按钮
        private void rjButton27_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            CommonWrite(_addressMapping["放气阀开启按钮状态"], false);
        }
        bool _CloseState2, _CloseState3,_CloseState4;
        //上锁按钮
        private async void rjButton1_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_CloseState2) return;
            _CloseState2 = true;
            try
            {
                CommonWrite(_addressMapping["上锁按钮"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["上锁按钮"], false);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
            finally
            {
                _CloseState2 = false;
            }
        }
        //解锁按钮
        private async void rjButton2_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_CloseState3) return;
            _CloseState3 = true;
            try
            {
                CommonWrite(_addressMapping["解锁按钮"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["解锁按钮"], false);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
            finally
            {
                _CloseState3 = false;
            }
        }

        private void txt_Speed_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["转速设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }

        private void txt_CRWD_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            try
            {
                if (_plc != null && _plc.IsConnected)
                {
                    _plc.Write(_addressMapping["出炉温度设定"], Convert.ToSingle(e.DecimalValue.Value));
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

        private void txt_SWD_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["上温度设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }

        private void txt_XWD_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["下温度设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }

        private void rjButton_TCHGJ_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            _TCHGJState = !_TCHGJState;
            CommonWrite(_addressMapping["涂层后关机按钮状态"], _TCHGJState);
        }

        private void rjButton_TJ_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            _TJState = !_TJState;
            CommonWrite(_addressMapping["停机按钮状态"], _TJState);
        }

        private void rjButton_FQ_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            _FQState = !_FQState;
            CommonWrite(_addressMapping["放气按钮状态"], _FQState);
        }

        private void rjButton_GZK_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            _GZKState = !_GZKState;
            CommonWrite(_addressMapping["高真空按钮状态"], _GZKState);
        }

        private void rjButton_CC_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            _CCState = !_CCState;
            CommonWrite(_addressMapping["粗抽按钮状态"], _CCState);
        }

        private async void RjButton_Stop_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_CloseState4) return;
            _CloseState4 = true;
            try
            {
                CommonWrite(_addressMapping["转盘电机停止按钮"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["转盘电机停止按钮"], false);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
            finally
            {
                _CloseState4 = false;
            }
        }

        private void RjButton_Home_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            CommonWrite(_addressMapping["转盘电机回原点按钮"], true);
        }
    }
}