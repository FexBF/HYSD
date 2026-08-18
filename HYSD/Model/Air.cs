using HYSDControls;
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

namespace HYSD
{
    public partial class Air : UserControl, IPollablePage
    {
        private readonly IOmronPlcService _plc;
        private readonly IPLCAddressService _address;
        private readonly ILogger _logger;
        private readonly Dictionary<string, string> _addressMapping;
        private readonly IReadDataService _readData;
        private bool _State6;
        public Air(IOmronPlcService plc, IPLCAddressService address, ILogger logger, IReadDataService readData)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            _plc = plc;
            _address = address;
            _logger = logger;
            _readData = readData;
            try
            {
                _addressMapping = _address.GetAddressMapping(_address.ReadSheet(), 5, "Name", "Address");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
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
                    isChecked = false;// 断开连接时重置状态，以便重新读取开关状态
                    if (this.IsHandleCreated && !this.IsDisposed)
                        this.BeginInvoke((Action)(() =>
                        {
                            toggleSwitchs1.IsPlcConnected = false;
                            toggleSwitchs2.IsPlcConnected = false;
                            toggleSwitchs3.IsPlcConnected = false;
                            toggleSwitchs4.IsPlcConnected = false;
                            toggleSwitchs5.IsPlcConnected = false;
                            toggleSwitchs6.IsPlcConnected = false;
                            toggleSwitchs7.IsPlcConnected = false;
                            toggleSwitchs8.IsPlcConnected = false;
                        }));
                }
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
        }

        bool isChecked = false;
        private void PlcDataEx()
        {
            #region 读取开关状态 只读一次
            if (!isChecked)
            {
                //控制按钮
                _State6 = (bool)_readData.TryGetValueW("控制按钮");
                isChecked = true;
            }

            //隔膜阀开关1
            toggleSwitchs1.IsOn = (bool)_readData.TryGetValueW("隔膜阀开关1");
            toggleSwitchs1.IsPlcConnected = true;
            //隔膜阀开关2
            toggleSwitchs3.IsOn = (bool)_readData.TryGetValueW("隔膜阀开关2");
            toggleSwitchs3.IsPlcConnected = true;
            //隔膜阀开关3
            toggleSwitchs5.IsOn = (bool)_readData.TryGetValueW("隔膜阀开关3");
            toggleSwitchs5.IsPlcConnected = true;
            //隔膜阀开关4
            toggleSwitchs7.IsOn = (bool)_readData.TryGetValueW("隔膜阀开关4");
            toggleSwitchs7.IsPlcConnected = true;
            //手动排气1
            toggleSwitchs2.IsOn = (bool)_readData.TryGetValueW("手动排气1");
            toggleSwitchs2.IsPlcConnected = true;
            //手动排气2
            toggleSwitchs4.IsOn = (bool)_readData.TryGetValueW("手动排气2");
            toggleSwitchs4.IsPlcConnected = true;
            //手动排气3
            toggleSwitchs6.IsOn = (bool)_readData.TryGetValueW("手动排气3");
            toggleSwitchs6.IsPlcConnected = true;
            //手动排气4
            toggleSwitchs8.IsOn = (bool)_readData.TryGetValueW("手动排气4");
            toggleSwitchs8.IsPlcConnected = true;

            //氮气流量设定
            if (!textBox1.IsEditing)
                textBox1.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("氮气流量设定")).ToString().Trim());
            //氢气流量设定
            if (!textBox2.IsEditing)
                textBox2.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("氢气流量设定")).ToString().Trim());
            //氩气流量设定
            if (!textBox4.IsEditing)
                textBox4.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("氩气流量设定")).ToString().Trim());
            //备用气体流量设定
            if (!textBox3.IsEditing)
                textBox3.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("备用气体流量设定")).ToString().Trim());
            //压力值设定
            if (!textBox5.IsEditing)
                textBox5.UpdateValueFromPlc(((float)_readData.TryGetValueD29("压力值设定")).ToString("F3").Trim());

            #endregion
            //压力计CDG020
            label29.Text = ((string)_readData.TryGetValueD19("CDG20压力")).Trim() + "mbar";
            //前级Pirani1
            label30.Text = ((string)_readData.TryGetValueD19("管道压力")).Trim() + "mbar";
            //腔体Pirani2
            label32.Text = ((string)_readData.TryGetValueD19("腔体Pirani2压力")).Trim() + "mbar";
            //潘宁Penning
            label34.Text = ((string)_readData.TryGetValueD19("Penning压力")).Trim() + "mbar";
            //薄膜规CDG0100D
            label36.Text = ((string)_readData.TryGetValueD19("薄膜规100D压力")).Trim() + "mbar";
            //压力值
            label39.Text = ((string)_readData.TryGetValueD19("压力值")).Trim() + "mbar";
            //氮气实际流量
            label16.Text = ((ushort)_readData.TryGetValueD29("氮气实际流量")).ToString().Trim() + "SCCM";
            //氢气实际流量
            label19.Text = ((ushort)_readData.TryGetValueD29("氢气实际流量")).ToString().Trim() + "SCCM";
            //氩气实际流量
            label25.Text = ((ushort)_readData.TryGetValueD29("氩气实际流量")).ToString().Trim() + "SCCM";
            //备用气体实际流量
            label22.Text = ((ushort)_readData.TryGetValueD29("备用气体实际流量")).ToString().Trim() + "SCCM";

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
            //隔膜阀开关1
            switch ((bool)_readData.TryGetValueW("隔膜阀开关1"))
            {
                case true:
                    valveControl1.IsOpen = true;
                    pipeControl1.IsFlowing = true;
                    pipeControl2.IsFlowing = true;
                    thingerPumpBasic1.IsRun = true;
                    break;
                case false:
                    valveControl1.IsOpen = false;
                    pipeControl1.IsFlowing = false;
                    pipeControl2.IsFlowing = false;
                    thingerPumpBasic1.IsRun = false;
                    break;
                default:
                    break;
            }
            //隔膜阀开关2
            switch ((bool)_readData.TryGetValueW("隔膜阀开关2"))
            {
                case true:
                    valveControl2.IsOpen = true;
                    pipeControl3.IsFlowing = true;
                    pipeControl4.IsFlowing = true;
                    thingerPumpBasic2.IsRun = true;
                    break;
                case false:
                    valveControl2.IsOpen = false;
                    pipeControl3.IsFlowing = false;
                    pipeControl4.IsFlowing = false;
                    thingerPumpBasic2.IsRun = false;
                    break;
                default:
                    break;
            }
            //隔膜阀开关3
            switch ((bool)_readData.TryGetValueW("隔膜阀开关3"))
            {
                case true:
                    valveControl3.IsOpen = true;
                    pipeControl5.IsFlowing = true;
                    pipeControl6.IsFlowing = true;
                    thingerPumpBasic3.IsRun = true;
                    break;
                case false:
                    valveControl3.IsOpen = false;
                    pipeControl5.IsFlowing = false;
                    pipeControl6.IsFlowing = false;
                    thingerPumpBasic3.IsRun = false;
                    break;
                default:
                    break;
            }
            //隔膜阀开关4
            switch ((bool)_readData.TryGetValueW("隔膜阀开关4"))
            {
                case true:
                    valveControl4.IsOpen = true;
                    pipeControl7.IsFlowing = true;
                    pipeControl8.IsFlowing = true;
                    thingerPumpBasic4.IsRun = true;
                    break;
                case false:
                    valveControl4.IsOpen = false;
                    pipeControl7.IsFlowing = false;
                    pipeControl8.IsFlowing = false;
                    thingerPumpBasic4.IsRun = false;
                    break;
                default:
                    break;
            }
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

        #region 氮气
        //隔膜阀开关1
        private void toggleSwitchs1_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["隔膜阀开关1"], toggleSwitchs1.IsOn);
        }
        //手动排气1
        private void toggleSwitchs2_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["手动排气1"], toggleSwitchs2.IsOn);
        }

        #region 氮气流量设定
        private void textBox1_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["氮气流量设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }

        #endregion

        #endregion

        #region 氢气
        //隔膜阀开关2
        private void toggleSwitchs3_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["隔膜阀开关2"], toggleSwitchs3.IsOn);
        }

        //手动排气2
        private void toggleSwitchs4_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["手动排气2"], toggleSwitchs4.IsOn);
        }
        #region 氢气流量设定
        private void textBox2_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["氢气流量设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        #endregion

        #endregion

        #region 氩气
        //隔膜阀开关3
        private void toggleSwitchs5_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["隔膜阀开关3"], toggleSwitchs5.IsOn);
        }
        //手动排气3
        private void toggleSwitchs6_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["手动排气3"], toggleSwitchs6.IsOn);
        }
        #region 氩气流量设定
        private void textBox4_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["氩气流量设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        #endregion

        #endregion

        #region 备用气体
        //隔膜阀开关4
        private void toggleSwitchs7_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["隔膜阀开关4"], toggleSwitchs7.IsOn);
        }
        //手动排气4
        private void toggleSwitchs8_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["手动排气4"], toggleSwitchs8.IsOn);
        }
        #region 备用气体流量设定
        private void textBox3_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["备用气体流量设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        #endregion

        #endregion

        #region 压力值设定
        private void textBox5_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            try
            {
                if (_plc != null && _plc.IsConnected)
                {
                    _plc.Write(_addressMapping["压力值设定"], Convert.ToSingle(e.DecimalValue.Value));
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

        #endregion

        #region 流量控制
        private void rjButton4_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            _State6 = !_State6;
            CommonWrite(_addressMapping["控制按钮"], _State6);
        }
        #endregion
    }
}
