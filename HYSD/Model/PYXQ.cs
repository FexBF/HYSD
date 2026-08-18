using HYSDControls;
using RJCodeAdvance.RJControls;
using Serilog;
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

namespace HYSD
{
    public partial class PYXQ : UserControl, IPollablePage
    {
        private readonly IOmronPlcService _plc;
        private readonly IPLCAddressService _address;
        private readonly ILogger _logger;
        private readonly Dictionary<string, string> _addressMapping;
        private readonly IReadDataService _readData;
        public PYXQ(IOmronPlcService plc, IPLCAddressService address, ILogger logger, IReadDataService readData)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            _plc = plc;
            _address = address;
            _logger = logger;
            _readData = readData;
            try
            {
                _addressMapping = _address.GetAddressMapping(_address.ReadSheet(), 3, "Name", "Address");
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
                    if (this.IsHandleCreated && !this.IsDisposed)
                        this.BeginInvoke((Action)(() =>
                        {
                            toggleSwitchs1.IsPlcConnected = false;
                            toggleSwitchs2.IsPlcConnected = false;
                        }));

                }
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
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

        private void textBox1_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["峰值高电流"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        private void textBox4_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["递增T0"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        private void textBox2_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["保持T1"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        private void textBox5_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["递减T2"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        private void textBox6_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["谷值小电流"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        private void textBox3_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["保持T3"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        private void textBox7_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["占空比设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        private void textBox8_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["频率设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        private void textBox9_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["电压设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        private void textBox10_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["电流阈值设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }

        /// <summary>
        /// 偏压
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toggleSwitchs2_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["偏压"], toggleSwitchs2.IsOn);
        }
        /// <summary>
        /// 线圈电源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toggleSwitchs1_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["线圈电源"], toggleSwitchs1.IsOn);
        }
        private void PlcDataEx()
        {

            //线圈电源
            toggleSwitchs1.IsOn = (bool)_readData.TryGetValueW("线圈电源");
            toggleSwitchs1.IsPlcConnected = true;
            //偏压
            toggleSwitchs2.IsOn = (bool)_readData.TryGetValueW("偏压");
            toggleSwitchs2.IsPlcConnected = true;
            //峰值高电流
            if (!textBox1.IsEditing)
                textBox1.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("峰值高电流")).ToString().Trim());
            //递增T0
            if (!textBox4.IsEditing)
                textBox4.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("递增T0")).ToString().Trim());
            //保持T1
            if (!textBox2.IsEditing)
                textBox2.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("保持T1")).ToString().Trim());
            //递减T2
            if (!textBox5.IsEditing)
                textBox5.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("递减T2")).ToString().Trim());
            //保持T3
            if (!textBox3.IsEditing)
                textBox3.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("保持T3")).ToString().Trim());
            //谷值小电流
            if (!textBox6.IsEditing)
                textBox6.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("谷值小电流")).ToString().Trim());
            //占空比设定
            if (!textBox7.IsEditing)
                textBox7.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("占空比设定")).ToString().Trim());
            //频率设定
            if (!textBox8.IsEditing)
                textBox8.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("频率设定")).ToString().Trim());
            //电压设定
            if (!textBox9.IsEditing)
                textBox9.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("电压设定")).ToString().Trim());
            //电流阈值设定
            if (!textBox10.IsEditing)
                textBox10.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("电流阈值设定")).ToString().Trim());

            #region 数据显示
            //1#电流
            label13.Text = ((float)_readData.TryGetValueD29("1#电流")).ToString("F1").Trim();
            //2#电流
            label14.Text = ((float)_readData.TryGetValueD29("2#电流")).ToString("F1").Trim();
            //3#电流
            label9.Text = ((float)_readData.TryGetValueD29("3#电流")).ToString("F1").Trim();
            //4#电流
            label10.Text = ((float)_readData.TryGetValueD29("4#电流")).ToString("F1").Trim();
            //5#电流
            label11.Text = ((float)_readData.TryGetValueD29("5#电流")).ToString("F1").Trim();
            //6#电流
            label12.Text = ((float)_readData.TryGetValueD29("6#电流")).ToString("F1").Trim();
            //7#电流
            label15.Text = ((float)_readData.TryGetValueD29("7#电流")).ToString("F1").Trim();
            //8#电流
            label16.Text = ((float)_readData.TryGetValueD29("8#电流")).ToString("F1").Trim();

            //实际电压PYXQ
            label21.Text = ((ushort)_readData.TryGetValueD29("实际电压PYXQ")).ToString().Trim();
            //实际电流PYXQ
            label26.Text = ((float)_readData.TryGetValueD19("实际电流PYXQ")).ToString("F1").Trim();
            //实际功率PYXQ
            label25.Text = ((float)_readData.TryGetValueD19("实际功率PYXQ")).ToString("F1").Trim();
            //实际占空比PYXQ
            label27.Text = ((ushort)_readData.TryGetValueD29("实际占空比PYXQ")).ToString().Trim();
            //实际频率PYXQ
            label28.Text = ((ushort)_readData.TryGetValueD29("实际频率PYXQ")).ToString().Trim();
            //状态灯PYXQ
            switch ((ushort)_readData.TryGetValueD29("状态灯PYXQ"))
            {
                case 1:
                    if (this.thingerLED1.LedState == 1) break;
                    this.thingerLED1.LedState = 1;
                    break;
                case 0:
                    if (this.thingerLED1.LedState == 0) break;
                    this.thingerLED1.LedState = 0;
                    break;
                default:
                    break;
            }
            //报警灯PYXQ
            switch ((ushort)_readData.TryGetValueD29("报警灯PYXQ"))
            {
                case 1:
                    if (this.thingerLED2.IsBlink) break;
                    this.thingerLED2.IsBlink = true;
                    break;
                case 0:
                    if (!this.thingerLED2.IsBlink) break;
                    this.thingerLED2.IsBlink = false;
                    break;
                default:
                    break;
            }
            #endregion
        }
    }
}
