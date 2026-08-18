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
using static System.Windows.Forms.AxHost;

namespace HYSD
{
    public partial class SetUp : UserControl, IPollablePage
    {
        private readonly IOmronPlcService _plc;
        private readonly ILogger _logger;
        private readonly IPLCAddressService _address;
        private readonly Dictionary<string, string> _addressMapping;
        private readonly IReadDataService _readData;
        private bool _CloseState3;
        private bool _CloseState4;

        public SetUp(IOmronPlcService plc, ILogger logger, IPLCAddressService address, IReadDataService readData)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            _plc = plc;
            _logger = logger;
            _address = address;
            _readData = readData;
            try
            {
                _addressMapping = _address.GetAddressMapping(_address.ReadSheet(), 13, "Name", "Address");
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
                // 处理异常，例如日志记录
                _logger.Debug(ex.Message);
            }
        }

        private void PlcDataEx()
        {
            //旋片泵
            pump1.IsRunning = (bool)_readData.TryGetValueC("旋片泵运行");
            label5.Text = ((ushort)_readData.TryGetValueD19("旋片泵运行时间")).ToString() + "小时";
            label6.Text = (bool)_readData.TryGetValueC("170.06") ? "保养时间到" : "正常使用";
            circularProgressBar1.Value = (ushort)_readData.TryGetValueD19("旋片泵运行时间") * 100 / 5000 > 100 ? 100 : (ushort)_readData.TryGetValueD19("旋片泵运行时间") * 100 / 5000;
            circularProgressBar1.Text = circularProgressBar1.Value.ToString() + "%";
            //罗茨泵
            pump2.IsRunning = (bool)_readData.TryGetValueC("罗茨泵运行");
            label10.Text = ((ushort)_readData.TryGetValueD19("罗茨泵运行时间")).ToString() + "小时";
            label8.Text = (bool)_readData.TryGetValueC("180.07") ? "保养时间到" : "正常使用";
            circularProgressBar2.Value = (ushort)_readData.TryGetValueD19("罗茨泵运行时间") * 100 / 5000 > 100 ? 100 : (ushort)_readData.TryGetValueD19("罗茨泵运行时间") * 100 / 5000;
            circularProgressBar2.Text = circularProgressBar2.Value.ToString() + "%";
            //炉内温度
            label21.Text = ((float)_readData.TryGetValueD29("最高环境温度")).ToString("F1") + "°C";
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

        //旋片泵复位
        private async void rjButton12_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_CloseState3) return;
            _CloseState3 = true;
            try
            {
                CommonWrite(_addressMapping["旋片泵保养复位"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["旋片泵保养复位"], false);
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

        //罗茨泵复位
        private async void rjButton1_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_CloseState4) return;
            _CloseState4 = true;
            try
            {
                CommonWrite(_addressMapping["罗茨泵保养复位"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["罗茨泵保养复位"], false);
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

        //复位报警
        private void rjButton10_Click(object sender, EventArgs e)
        {

        }
    }
}
