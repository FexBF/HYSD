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
    public partial class McPower : UserControl, IPollablePage
    {
        private readonly IOmronPlcService _plc;
        private readonly IPLCAddressService _address;
        private readonly ILogger _logger;
        private readonly Dictionary<string, string> _addressMapping;
        private readonly IReadDataService _readData;
        private bool _b7State, _b8State;
        private bool _r7State, _r8State, _r1State, _r2State;

        public McPower(IOmronPlcService plc, IPLCAddressService address, ILogger logger, IReadDataService readData)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            _plc = plc;
            _address = address;
            _logger = logger;
            _readData = readData;
            try
            {
                _addressMapping = _address.GetAddressMapping(_address.ReadSheet(), 4, "Name", "Address");
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
                    isChecked = false;// 断开连接时重置开关状态，以便重新连接后正确读取状态
                    if (this.IsHandleCreated && !this.IsDisposed)
                        this.BeginInvoke((Action)(() =>
                        {
                            // 断开连接时将所有开关设置为未连接状态
                            this.toggleSwitchs1.IsPlcConnected = false;
                            this.toggleSwitchs2.IsPlcConnected = false;
                            this.toggleSwitchs3.IsPlcConnected = false;
                            this.toggleSwitchs4.IsPlcConnected = false;
                            this.toggleSwitchs5.IsPlcConnected = false;
                            this.toggleSwitchs6.IsPlcConnected = false;
                            this.toggleSwitchs7.IsPlcConnected = false;
                            this.toggleSwitchs8.IsPlcConnected = false;
                            this.toggleSwitchs9.IsPlcConnected = false;
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
            #region 读取开关 只读一次
            if (!isChecked)
            {
                //弧源7引弧按钮
                _b7State = (bool)_readData.TryGetValueW("弧源7引弧按钮");
                //弧源8引弧按钮
                _b8State = (bool)_readData.TryGetValueW("弧源8引弧按钮");
                isChecked = true;
            }
            #endregion

            //电源总电源
            this.toggleSwitchs1.IsOn = (bool)_readData.TryGetValueW("电源总电源");
            this.toggleSwitchs1.IsPlcConnected = true;
            //弧电源互锁
            this.toggleSwitchs2.IsOn = (bool)_readData.TryGetValueW("7和8弧电源互锁");
            this.toggleSwitchs2.IsPlcConnected = true;
            //弧源7电流开关
            this.toggleSwitchs3.IsOn = (bool)_readData.TryGetValueW("弧源7电流开关");
            this.toggleSwitchs3.IsPlcConnected = true;
            //弧源8电流开关
            this.toggleSwitchs4.IsOn = (bool)_readData.TryGetValueW("弧源8电流开关");
            this.toggleSwitchs4.IsPlcConnected = true;
            //脉冲电源1开关
            this.toggleSwitchs5.IsOn = (bool)_readData.TryGetValueW("脉冲电源1开关");
            this.toggleSwitchs5.IsPlcConnected = true;
            //脉冲电源2开关
            this.toggleSwitchs6.IsOn = (bool)_readData.TryGetValueW("脉冲电源2开关");
            this.toggleSwitchs6.IsPlcConnected = true;
            //双断模式
            this.toggleSwitchs7.IsOn = (bool)_readData.TryGetValueW("双断模式");
            this.toggleSwitchs7.IsPlcConnected = true;
            //加热模式
            this.toggleSwitchs8.IsOn = (bool)_readData.TryGetValueW("加热模式");
            this.toggleSwitchs8.IsPlcConnected = true;
            //镀膜模式
            this.toggleSwitchs9.IsOn = (bool)_readData.TryGetValueW("镀膜模式");
            this.toggleSwitchs9.IsPlcConnected = true;

            //弧源7电流设定   
            if (!textBox7.IsEditing)
                this.textBox7.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("弧源7电流设定")).ToString().Trim());
            //弧源8电流设定   
            if (!textBox8.IsEditing)
                this.textBox8.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("弧源8电流设定")).ToString().Trim());
            //脉冲电源1电流设定
            if (!textBox1.IsEditing)
                this.textBox1.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("脉冲电源1电流设定")).ToString().Trim());
            //脉冲电源1开通脉宽
            if (!textBox2.IsEditing)
                this.textBox2.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("脉冲电源1开通脉宽")).ToString().Trim());
            //脉冲电源1关断脉宽
            if (!textBox3.IsEditing)
                this.textBox3.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("脉冲电源1关断脉宽")).ToString().Trim());
            //脉冲电源2电流设定
            if (!textBox6.IsEditing)
                this.textBox6.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("脉冲电源2电流设定")).ToString().Trim());
            //脉冲电源2开通脉宽
            if (!textBox5.IsEditing)
                this.textBox5.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("脉冲电源2开通脉宽")).ToString().Trim());
            //脉冲电源2关断脉宽
            if (!textBox4.IsEditing)
                this.textBox4.UpdateValueFromPlc(((ushort)_readData.TryGetValueD29("脉冲电源2关断脉宽")).ToString().Trim());

            #region 弧源7数据
            //弧源7引弧按钮
            switch ((bool)_readData.TryGetValueW("弧源7引弧按钮"))
            {
                case true:
                    if (this.rjButton14.BackColor == Color.LimeGreen) break;
                    this.rjButton14.BackColor = Color.LimeGreen;
                    break;
                case false:
                    if (this.rjButton14.BackColor == Color.Silver) break;
                    this.rjButton14.BackColor = Color.Silver;
                    break;
                default:
                    break;
            }
            //弧源7复位按钮
            switch ((bool)_readData.TryGetValueW("弧源7复位按钮"))
            {
                case true:
                    if (this.rjButton13.BackColor == Color.LimeGreen) break;
                    this.rjButton13.BackColor = Color.LimeGreen;
                    break;
                case false:
                    if (this.rjButton13.BackColor == Color.Silver) break;
                    this.rjButton13.BackColor = Color.Silver;
                    break;
                default:
                    break;
            }
            //弧源7电压
            this.label50.Text = ((float)_readData.TryGetValueD29("弧源7电压")).ToString("F1") + "V";
            //弧源7电流
            this.label49.Text = ((float)_readData.TryGetValueD29("弧源7电流")).ToString("F1") + "A";
            #endregion

            #region 弧源8数据
            //弧源8引弧按钮
            switch ((bool)_readData.TryGetValueW("弧源8引弧按钮"))
            {
                case true:
                    if (this.rjButton16.BackColor == Color.LimeGreen) break;
                    this.rjButton16.BackColor = Color.LimeGreen;
                    break;
                case false:
                    if (this.rjButton16.BackColor == Color.Silver) break;
                    this.rjButton16.BackColor = Color.Silver;
                    break;
                default:
                    break;
            }
            //弧源8复位按钮
            switch ((bool)_readData.TryGetValueW("弧源8复位按钮"))
            {
                case true:
                    if (this.rjButton15.BackColor == Color.LimeGreen) break;
                    this.rjButton15.BackColor = Color.LimeGreen;
                    break;
                case false:
                    if (this.rjButton15.BackColor == Color.Silver) break;
                    this.rjButton15.BackColor = Color.Silver;
                    break;
                default:
                    break;
            }
            //弧源8电压
            this.label58.Text = ((float)_readData.TryGetValueD29("弧源8电压")).ToString("F1") + "V";
            //弧源8电流
            this.label57.Text = ((float)_readData.TryGetValueD29("弧源8电流")).ToString("F1") + "A";
            #endregion

            #region 脉冲电源1数据
            //脉冲电源1电流
            this.label10.Text = ((float)_readData.TryGetValueD29("脉冲电源1电流")).ToString("F1") + "A";
            //脉冲电源1电压
            this.label9.Text = ((float)_readData.TryGetValueD29("脉冲电源1电压")).ToString("F1") + "V";
            //脉冲电源1占空比
            this.label8.Text = ((float)_readData.TryGetValueD29("脉冲电源1占空比")).ToString("F1") + "%";
            //脉冲电源1频率
            this.label7.Text = ((ushort)_readData.TryGetValueD29("脉冲电源1频率")).ToString() + "KHZ";
            #endregion

            #region 脉冲电源2数据
            //脉冲电源2电流
            this.label14.Text = ((float)_readData.TryGetValueD29("脉冲电源2电流")).ToString("F1") + "A";
            //脉冲电源2电压
            this.label13.Text = ((float)_readData.TryGetValueD29("脉冲电源2电压")).ToString("F1") + "V";
            //脉冲电源2占空比
            this.label12.Text = ((float)_readData.TryGetValueD29("脉冲电源2占空比")).ToString("F1") + "%";
            //脉冲电源2频率
            this.label11.Text = ((ushort)_readData.TryGetValueD29("脉冲电源2频率")).ToString() + "KHZ";
            #endregion
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

        #region 弧源7
        private void textBox7_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["弧源7电流设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        //弧源7电流开关按钮
        private void toggleSwitchs3_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["弧源7电流开关"], toggleSwitchs3.IsOn);
        }

        //弧源7引弧按钮
        private void rjButton14_Click(object sender, EventArgs e)
        {

            if (_plc == null || !_plc.IsConnected) return;
            _b7State = !_b7State;
            CommonWrite(_addressMapping["弧源7引弧按钮"], _b7State);
        }
        //弧源7复位按钮
        private async void rjButton13_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_r7State) return;
            _r7State = true;
            try
            {
                CommonWrite(_addressMapping["弧源7复位按钮"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["弧源7复位按钮"], false);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
            finally
            {
                _r7State = false;
            }
        }
        #endregion

        #region 弧源8
        private void textBox8_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["弧源8电流设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        //弧源8电流开关按钮
        private void toggleSwitchs4_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["弧源8电流开关"], toggleSwitchs4.IsOn);
        }

        //弧源8引弧按钮
        private void rjButton16_Click(object sender, EventArgs e)
        {

            if (_plc == null || !_plc.IsConnected) return;
            _b8State = !_b8State;
            CommonWrite(_addressMapping["弧源8引弧按钮"], _b8State);
        }

        //弧源8复位按钮
        private async void rjButton15_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_r8State) return;
            _r8State = true;
            try
            {
                CommonWrite(_addressMapping["弧源8复位按钮"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["弧源8复位按钮"], false);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
            finally
            {
                _r8State = false;
            }
        }
        #endregion

        #region 电源总电源
        private void toggleSwitchs1_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["电源总电源"], toggleSwitchs1.IsOn);
        }
        #endregion

        #region 弧电源互锁
        private void toggleSwitchs2_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["弧电源互锁"], toggleSwitchs2.IsOn);
        }
        #endregion

        #region 脉冲电源1
        //脉冲电源1开关
        private void toggleSwitchs5_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["脉冲电源1开关"], toggleSwitchs5.IsOn);
        }
        //脉冲电源1复位按钮
        private async void rjButton4_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_r1State) return;
            _r1State = true;
            try
            {
                CommonWrite(_addressMapping["脉冲电源1复位按钮"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["脉冲电源1复位按钮"], false);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
            finally
            {
                _r1State = false;
            }
        }

        #region 脉冲电源1电流设定
        private void textBox1_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["脉冲电源1电流设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }

        #endregion

        #region 脉冲电源1开通脉宽
        private void textBox2_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["脉冲电源1开通脉宽"], Convert.ToUInt16(e.DecimalValue.Value));
        }

        #endregion

        #region 脉冲电源1关断脉宽
        private void textBox3_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["脉冲电源1关断脉宽"], Convert.ToUInt16(e.DecimalValue.Value));
        }

        #endregion

        #endregion

        #region 脉冲电源2
        //脉冲电源2开关
        private void toggleSwitchs6_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["脉冲电源2开关"], toggleSwitchs6.IsOn);
        }

        //脉冲电源2复位按钮
        private async void rjButton5_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_r2State) return;
            _r2State = true;
            try
            {
                CommonWrite(_addressMapping["脉冲电源2复位按钮"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["脉冲电源2复位按钮"], false);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
            finally
            {
                _r2State = false;
            }
        }

        #region 脉冲电源2电流设定
        private void textBox6_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["脉冲电源2电流设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        #endregion

        #region 脉冲电源2开通脉宽
        private void textBox5_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["脉冲电源2开通脉宽"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        #endregion

        #region 脉冲电源2关断脉宽
        private void textBox4_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["脉冲电源2关断脉宽"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        #endregion

        #endregion

        //双断模式
        private void toggleSwitchs7_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["双断模式"], toggleSwitchs7.IsOn);
        }
        //加热模式
        private void toggleSwitchs8_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["加热模式"], toggleSwitchs8.IsOn);
        }
        //镀膜模式
        private void toggleSwitchs9_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["镀膜模式"], toggleSwitchs9.IsOn);
        }
    }
}
