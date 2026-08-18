using HYSDControls;
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
    public partial class Water : UserControl, IPollablePage
    {
        private readonly IReadDataService _readData;
        private readonly IOmronPlcService _plc;
        private readonly ILogger _logger;
        private readonly IPLCAddressService _address;
        private readonly Dictionary<string, string> _addressMapping;
        private Color c = Color.FromArgb(0, 205, 209);
        public Water(IReadDataService readData, IOmronPlcService plc, ILogger logger, IPLCAddressService address)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            _readData = readData;
            _plc = plc;
            _logger = logger;
            _address = address;
            try
            {
                _addressMapping = _address.GetAddressMapping(_address.ReadSheet(), 12, "Name", "Address");
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
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
        }

        private void PlcDataEx()
        {
            //冰水机
            if (!numTextBox1.IsEditing)
                numTextBox1.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("冰水机温度设定")).ToString().Trim());
            label3.Text = ((float)_readData.TryGetValueD29("冰水机实际温度")).ToString("F1") + "℃";
            rjButton_Close.BackColor = (bool)_readData.TryGetValueW("冰水机关机") ? Color.Lime : Color.Silver;
            rjButton_Mute.BackColor = (bool)_readData.TryGetValueW("冰水机消音") ? Color.Lime : Color.Silver;
            rjButton_Reset.BackColor = (bool)_readData.TryGetValueW("冰水机复位") ? Color.Lime : Color.Silver;
            thingerLED_State.LedState = (ushort)_readData.TryGetValueD29("冰水机状态");
            if ((ushort)_readData.TryGetValueD29("冰水机状态") == 0)
            {
                rjButton_Start.BackColor = Color.Silver;
                rjButton_Start.Text = "启动";
            }
            else if ((ushort)_readData.TryGetValueD29("冰水机状态") == 1)
            {
                rjButton_Start.BackColor = Color.Lime;
                rjButton_Start.Text = "运行";
            }
            //腔体
            if ((bool)_readData.TryGetValueW("腔体缺水"))
            {
                label7.Text = "腔体正常";
                label7.BackColor = c;
                label7.ForeColor = Color.Black;
            }
            else
            {
                label7.Text = "腔体缺水";
                label7.BackColor = Color.Red;
                label7.ForeColor = Color.White;
            }
            label8.Text = ((float)_readData.TryGetValueD29("腔体水流量")).ToString("F1") + "L/MIN";
            label9.Text = ((float)_readData.TryGetValueD29("腔体水温")).ToString("F1") + "℃";
            //靶1和4
            if ((bool)_readData.TryGetValueW("靶1和4缺水"))
            {
                label10.Text = "靶1和4正常";
                label10.BackColor = c;
                label10.ForeColor = Color.Black;
            }
            else
            {
                label10.Text = "靶1和4缺水";
                label10.BackColor = Color.Red;
                label10.ForeColor = Color.White;
            }
            label11.Text = ((float)_readData.TryGetValueD29("靶1和4水流量")).ToString("F1") + "L/MIN";
            label12.Text = ((float)_readData.TryGetValueD29("靶1和4水温")).ToString("F1") + "℃";
            //靶2和3
            if ((bool)_readData.TryGetValueW("靶2和3缺水"))
            {
                label13.Text = "靶2和3正常";
                label13.BackColor = c;
                label13.ForeColor = Color.Black;
            }
            else
            {
                label13.Text = "靶2和3缺水";
                label13.BackColor = Color.Red;
                label13.ForeColor = Color.White;
            }
            label14.Text = ((float)_readData.TryGetValueD29("靶2和3水流量")).ToString("F1") + "L/MIN";
            label15.Text = ((float)_readData.TryGetValueD29("靶2和3水温")).ToString("F1") + "℃";
            //靶5和6
            if ((bool)_readData.TryGetValueW("靶5和6缺水"))
            {
                label16.Text = "靶5和6正常";
                label16.BackColor = c;
                label16.ForeColor = Color.Black;
            }
            else
            {
                label16.Text = "靶5和6缺水";
                label16.BackColor = Color.Red;
                label16.ForeColor = Color.White;
            }
            label17.Text = ((float)_readData.TryGetValueD29("靶5和6水流量")).ToString("F1") + "L/MIN";
            label18.Text = ((float)_readData.TryGetValueD29("靶5和6水温")).ToString("F1") + "℃";
            //靶7和8
            if ((bool)_readData.TryGetValueW("靶7和8缺水"))
            {
                label19.Text = "靶7和8正常";
                label19.BackColor = c;
                label19.ForeColor = Color.Black;
            }
            else
            {
                label19.Text = "靶7和8缺水";
                label19.BackColor = Color.Red;
                label19.ForeColor = Color.White;
            }
            label20.Text = ((float)_readData.TryGetValueD29("靶7和8水流量")).ToString("F1") + "L/MIN";
            label21.Text = ((float)_readData.TryGetValueD29("靶7和8水温")).ToString("F1") + "℃";
            //靶座
            if ((bool)_readData.TryGetValueW("靶座缺水"))
            {
                label22.Text = "磁流体正常";
                label22.BackColor = c;
                label22.ForeColor = Color.Black;

                label28.Text = "靶座正常";
                label28.BackColor = c;
                label28.ForeColor = Color.Black;
            }
            else
            {
                label22.Text = "磁流体缺水";
                label22.BackColor = Color.Red;
                label22.ForeColor = Color.White;

                label28.Text = "靶座缺水";
                label28.BackColor = Color.Red;
                label28.ForeColor = Color.White;
            }
            label23.Text = ((float)_readData.TryGetValueD29("靶座水流量")).ToString("F1") + "L/MIN";
            label24.Text = ((float)_readData.TryGetValueD29("靶座水温")).ToString("F1") + "℃";
            //电源

            if ((bool)_readData.TryGetValueW("电源缺水"))
            {
                label25.Text = "偏压电源正常";
                label25.BackColor = c;
                label25.ForeColor = Color.Black;

                label29.Text = "脉冲电源正常";
                label29.BackColor = c;
                label29.ForeColor = Color.Black;

                label31.Text = "线圈电流正常";
                label31.BackColor = c;
                label31.ForeColor = Color.Black;
            }
            else
            {
                label25.Text = "偏压电源缺水";
                label25.BackColor = Color.Red;
                label25.ForeColor = Color.White;

                label29.Text = "脉冲电源缺水";
                label29.BackColor = Color.Red;
                label29.ForeColor = Color.White;

                label31.Text = "线圈电流缺水";
                label31.BackColor = Color.Red;
                label31.ForeColor = Color.White;
            }
            label26.Text = ((float)_readData.TryGetValueD29("电源水流量")).ToString("F1") + "L/MIN";
            label27.Text = ((float)_readData.TryGetValueD29("电源水温")).ToString("F1") + "℃";
            //罗茨泵
            if ((bool)_readData.TryGetValueW("罗茨泵缺水"))
            {
                label37.Text = "罗茨泵正常";
                label37.BackColor = c;
                label37.ForeColor = Color.Black;
            }
            else
            {
                label37.Text = "罗茨泵缺水";
                label37.BackColor = Color.Red;
                label37.ForeColor = Color.White;
            }
            label33.Text = ((float)_readData.TryGetValueD29("罗茨泵水流量")).ToString("F1") + "L/MIN";
            label34.Text = ((float)_readData.TryGetValueD29("罗茨泵水温")).ToString("F1") + "℃";
            //分子泵
            if ((bool)_readData.TryGetValueW("分子泵缺水"))
            {
                label30.Text = "分子泵正常";
                label30.BackColor = c;
                label30.ForeColor = Color.Black;
            }
            else
            {
                label30.Text = "分子泵缺水";
                label30.BackColor = Color.Red;
                label30.ForeColor = Color.White;
            }
            label39.Text = ((float)_readData.TryGetValueD29("分子泵水流量")).ToString("F1") + "L/MIN";
            label40.Text = ((float)_readData.TryGetValueD29("分子泵水温")).ToString("F1") + "℃";
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

        bool _state2, _state3, _state4, _state5;

        private async void rjButton_Reset_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_state5) return;
            _state5 = true;
            try
            {
                CommonWrite(_addressMapping["冰水机复位"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["冰水机复位"], false);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
            finally
            {
                _state5 = false;
            }
        }

        private void numTextBox1_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["冰水机温度设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }

        private async void rjButton_Mute_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_state4) return;
            _state4 = true;
            try
            {
                CommonWrite(_addressMapping["冰水机消音"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["冰水机消音"], false);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
            finally
            {
                _state4 = false;
            }
        }

        private async void rjButton_Close_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_state3) return;
            _state3 = true;
            try
            {
                CommonWrite(_addressMapping["冰水机关机"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["冰水机关机"], false);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
            finally
            {
                _state3 = false;
            }
        }

        private async void rjButton_Start_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_state2) return;
            _state2 = true;
            try
            {
                CommonWrite(_addressMapping["冰水机启动"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["冰水机启动"], false);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
            finally
            {
                _state2 = false;
            }
        }
    }
}
