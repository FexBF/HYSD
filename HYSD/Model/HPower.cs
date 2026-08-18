using HYSDControls;
using RJCodeAdvance.RJControls;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HYSD
{
    public partial class HPower : UserControl, IPollablePage
    {
        private readonly IOmronPlcService _plc;
        private readonly IPLCAddressService _address;
        private readonly ILogger _logger;
        private readonly Dictionary<string, string> _addressMapping;
        private readonly IReadDataService _readData;
        public HPower(IOmronPlcService plc, IPLCAddressService address, ILogger logger, IReadDataService readData)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            initProgressBar();
            _plc = plc;
            _address = address;
            _logger = logger;
            _readData = readData;
            try
            {
                _addressMapping = _address.GetAddressMapping(_address.ReadSheet(), 1, "Name", "Address");
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

        private void initProgressBar()
        {
            circularProgressBar1.Value = 0;
            circularProgressBar2.Value = 0;
            circularProgressBar3.Value = 0;
            circularProgressBar4.Value = 0;
            circularProgressBar5.Value = 0;
            circularProgressBar6.Value = 0;
            circularProgressBar7.Value = 0;
            circularProgressBar8.Value = 0;
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
                    isChecked = false;// 连接断开时重置检测状态，以便重新检测
                    if (this.IsHandleCreated && !this.IsDisposed)
                        this.BeginInvoke((Action)(() =>
                        {
                            // 断开连接时将所有开关状态设置为未连接
                            toggleSwitchs1.IsPlcConnected = false;
                            toggleSwitchs2.IsPlcConnected = false;
                            toggleSwitchs3.IsPlcConnected = false;
                            toggleSwitchs4.IsPlcConnected = false;
                            toggleSwitchs5.IsPlcConnected = false;
                            toggleSwitchs6.IsPlcConnected = false;
                            toggleSwitchs7.IsPlcConnected = false;
                            toggleSwitchs8.IsPlcConnected = false;
                            toggleSwitchs9.IsPlcConnected = false;
                            toggleSwitchs10.IsPlcConnected = false;
                            toggleSwitchs11.IsPlcConnected = false;
                            toggleSwitchs12.IsPlcConnected = false;
                            toggleSwitchs13.IsPlcConnected = false;
                            toggleSwitchs14.IsPlcConnected = false;
                            toggleSwitchs15.IsPlcConnected = false;
                            toggleSwitchs16.IsPlcConnected = false;
                            toggleSwitchs17.IsPlcConnected = false;
                            toggleSwitchs18.IsPlcConnected = false;
                            toggleSwitchs19.IsPlcConnected = false;
                            toggleSwitchs20.IsPlcConnected = false;
                            toggleSwitchs21.IsPlcConnected = false;
                            toggleSwitchs22.IsPlcConnected = false;
                            toggleSwitchs23.IsPlcConnected = false;
                            toggleSwitchs24.IsPlcConnected = false;
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
            //弧源1进度条
            var currentValue1 = (ushort)_readData.TryGetValueD29("弧源1电流设定");
            this.circularProgressBar1.Value = currentValue1;
            this.circularProgressBar1.Text = (currentValue1 / 2).ToString() + "%";
            //弧源2进度条
            var currentValue2 = (ushort)_readData.TryGetValueD29("弧源2电流设定");
            this.circularProgressBar2.Value = currentValue2;
            this.circularProgressBar2.Text = (currentValue2 / 2).ToString() + "%";
            //弧源3进度条
            var currentValue3 = (ushort)_readData.TryGetValueD29("弧源3电流设定");
            this.circularProgressBar3.Value = currentValue3;
            this.circularProgressBar3.Text = (currentValue3 / 2).ToString() + "%";
            //弧源4进度条
            var currentValue4 = (ushort)_readData.TryGetValueD29("弧源4电流设定");
            this.circularProgressBar4.Value = currentValue4;
            this.circularProgressBar4.Text = (currentValue4 / 2).ToString() + "%";
            //弧源5进度条
            var currentValue5 = (ushort)_readData.TryGetValueD29("弧源5电流设定");
            this.circularProgressBar5.Value = currentValue5;
            this.circularProgressBar5.Text = (currentValue5 / 2).ToString() + "%";
            //弧源6进度条
            var currentValue6 = (ushort)_readData.TryGetValueD29("弧源6电流设定");
            this.circularProgressBar6.Value = currentValue6;
            this.circularProgressBar6.Text = (currentValue6 / 2).ToString() + "%";
            //弧源7进度条
            var currentValue7 = (ushort)_readData.TryGetValueD29("弧源7电流设定");
            this.circularProgressBar7.Value = currentValue7;
            this.circularProgressBar7.Text = (currentValue7 / 2).ToString() + "%";
            //弧源8进度条
            var currentValue8 = (ushort)_readData.TryGetValueD29("弧源8电流设定");
            this.circularProgressBar8.Value = currentValue8;
            this.circularProgressBar8.Text = (currentValue8 / 2).ToString() + "%";

            //弧源1电流开关
            toggleSwitchs17.IsOn = toggleSwitchs1.IsOn = (bool)_readData.TryGetValueW("弧源1电流开关");
            toggleSwitchs17.IsPlcConnected = toggleSwitchs1.IsPlcConnected = true;
            //弧源2电流开关
            toggleSwitchs18.IsOn = toggleSwitchs2.IsOn = (bool)_readData.TryGetValueW("弧源2电流开关");
            toggleSwitchs18.IsPlcConnected = toggleSwitchs2.IsPlcConnected = true;
            //弧源3电流开关
            toggleSwitchs19.IsOn = toggleSwitchs3.IsOn = (bool)_readData.TryGetValueW("弧源3电流开关");
            toggleSwitchs19.IsPlcConnected = toggleSwitchs3.IsPlcConnected = true;
            //弧源4电流开关
            toggleSwitchs20.IsOn = toggleSwitchs4.IsOn = (bool)_readData.TryGetValueW("弧源4电流开关");
            toggleSwitchs20.IsPlcConnected = toggleSwitchs4.IsPlcConnected = true;
            //弧源5电流开关
            toggleSwitchs21.IsOn = toggleSwitchs5.IsOn = (bool)_readData.TryGetValueW("弧源5电流开关");
            toggleSwitchs21.IsPlcConnected = toggleSwitchs5.IsPlcConnected = true;
            //弧源6电流开关
            toggleSwitchs22.IsOn = toggleSwitchs6.IsOn = (bool)_readData.TryGetValueW("弧源6电流开关");
            toggleSwitchs22.IsPlcConnected = toggleSwitchs6.IsPlcConnected = true;
            //弧源7电流开关
            toggleSwitchs23.IsOn = toggleSwitchs7.IsOn = (bool)_readData.TryGetValueW("弧源7电流开关");
            toggleSwitchs23.IsPlcConnected = toggleSwitchs7.IsPlcConnected = true;
            //弧源8电流开关
            toggleSwitchs24.IsOn = toggleSwitchs8.IsOn = (bool)_readData.TryGetValueW("弧源8电流开关");
            toggleSwitchs24.IsPlcConnected = toggleSwitchs8.IsPlcConnected = true;
            //电源总电源
            toggleSwitchs9.IsOn = (bool)_readData.TryGetValueW("电源总电源");
            toggleSwitchs9.IsPlcConnected = true;
            //1和2电源互锁
            toggleSwitchs10.IsOn = (bool)_readData.TryGetValueW("1和2弧电源互锁");
            toggleSwitchs10.IsPlcConnected = true;
            //3和4电源互锁
            toggleSwitchs11.IsOn = (bool)_readData.TryGetValueW("3和4弧电源互锁");
            toggleSwitchs11.IsPlcConnected = true;
            //5和6电源互锁
            toggleSwitchs12.IsOn = (bool)_readData.TryGetValueW("5和6弧电源互锁");
            toggleSwitchs12.IsPlcConnected = true;
            //7和8电源互锁
            toggleSwitchs13.IsOn = (bool)_readData.TryGetValueW("7和8弧电源互锁");
            toggleSwitchs13.IsPlcConnected = true;
            //双断模式
            toggleSwitchs14.IsOn = (bool)_readData.TryGetValueW("双断模式");
            toggleSwitchs14.IsPlcConnected = true;
            //加热模式
            toggleSwitchs15.IsOn = (bool)_readData.TryGetValueW("加热模式");
            toggleSwitchs15.IsPlcConnected = true;
            //镀膜模式
            toggleSwitchs16.IsOn = (bool)_readData.TryGetValueW("镀膜模式");
            toggleSwitchs16.IsPlcConnected = true;


            #region 检测开关 只检测一次
            if (!isChecked)
            {
                //弧源1引弧按钮
                _b1State = (bool)_readData.TryGetValueW("弧源1引弧按钮");
                //弧源2引弧按钮
                _b2State = (bool)_readData.TryGetValueW("弧源2引弧按钮");
                //弧源3引弧按钮
                _b3State = (bool)_readData.TryGetValueW("弧源3引弧按钮");
                //弧源4引弧按钮
                _b4State = (bool)_readData.TryGetValueW("弧源4引弧按钮");
                //弧源5引弧按钮
                _b5State = (bool)_readData.TryGetValueW("弧源5引弧按钮");
                //弧源6引弧按钮
                _b6State = (bool)_readData.TryGetValueW("弧源6引弧按钮");
                //弧源7引弧按钮
                _b7State = (bool)_readData.TryGetValueW("弧源7引弧按钮");
                //弧源8引弧按钮
                _b8State = (bool)_readData.TryGetValueW("弧源8引弧按钮");

                isChecked = true;
            }

            #endregion

            //弧源1电流设定
            if (!textBox1.IsEditing)
                this.textBox1.UpdateValueFromPlc(currentValue1.ToString().Trim());
            //弧源2电流设定
            if (!textBox2.IsEditing)
                this.textBox2.UpdateValueFromPlc(currentValue2.ToString().Trim());
            //弧源3电流设定
            if (!textBox3.IsEditing)
                this.textBox3.UpdateValueFromPlc(currentValue3.ToString().Trim());
            //弧源4电流设定
            if (!textBox4.IsEditing)
                this.textBox4.UpdateValueFromPlc(currentValue4.ToString().Trim());
            //弧源5电流设定
            if (!textBox5.IsEditing)
                this.textBox5.UpdateValueFromPlc(currentValue5.ToString().Trim());
            //弧源6电流设定   
            if (!textBox6.IsEditing)
                this.textBox6.UpdateValueFromPlc(currentValue6.ToString().Trim());
            //弧源7电流设定   
            if (!textBox7.IsEditing)
                this.textBox7.UpdateValueFromPlc(currentValue7.ToString().Trim());
            //弧源8电流设定   
            if (!textBox8.IsEditing)
                this.textBox8.UpdateValueFromPlc(currentValue8.ToString().Trim());

            #region 弧源1数据
            //弧源1引弧按钮
            switch ((bool)_readData.TryGetValueW("弧源1引弧按钮"))
            {
                case true:
                    if (this.rjButton1.BackColor == Color.LimeGreen) break;
                    this.rjButton1.BackColor = Color.LimeGreen;
                    break;
                case false:
                    if (this.rjButton1.BackColor == Color.Silver) break;
                    this.rjButton1.BackColor = Color.Silver;
                    break;
                default:
                    break;
            }
            //弧源1复位按钮
            switch ((bool)_readData.TryGetValueW("弧源1复位按钮"))
            {
                case true:
                    if (this.rjButton2.BackColor == Color.LimeGreen) break;
                    this.rjButton2.BackColor = Color.LimeGreen;
                    break;
                case false:
                    if (this.rjButton2.BackColor == Color.Silver) break;
                    this.rjButton2.BackColor = Color.Silver;
                    break;
                default:
                    break;
            }
            //弧源1电压
            this.label7.Text = ((float)_readData.TryGetValueD29("弧源1电压")).ToString("F1") + "V";
            //弧源1电流
            this.label8.Text = ((float)_readData.TryGetValueD29("弧源1电流")).ToString("F1") + "A";
            #endregion

            #region 弧源2数据
            //弧源2引弧按钮
            switch ((bool)_readData.TryGetValueW("弧源2引弧按钮"))
            {
                case true:
                    if (this.rjButton4.BackColor == Color.LimeGreen) break;
                    this.rjButton4.BackColor = Color.LimeGreen;
                    break;
                case false:
                    if (this.rjButton4.BackColor == Color.Silver) break;
                    this.rjButton4.BackColor = Color.Silver;
                    break;
                default:
                    break;
            }
            //弧源2复位按钮
            switch ((bool)_readData.TryGetValueW("弧源2复位按钮"))
            {
                case true:
                    if (this.rjButton3.BackColor == Color.LimeGreen) break;
                    this.rjButton3.BackColor = Color.LimeGreen;
                    break;
                case false:
                    if (this.rjButton3.BackColor == Color.Silver) break;
                    this.rjButton3.BackColor = Color.Silver;
                    break;
                default:
                    break;
            }
            //弧源2电压
            this.label10.Text = ((float)_readData.TryGetValueD29("弧源2电压")).ToString("F1") + "V";
            //弧源2电流
            this.label9.Text = ((float)_readData.TryGetValueD29("弧源2电流")).ToString("F1") + "A";
            #endregion

            #region 弧源3数据
            //弧源3引弧按钮
            switch ((bool)_readData.TryGetValueW("弧源3引弧按钮"))
            {
                case true:
                    if (this.rjButton6.BackColor == Color.LimeGreen) break;
                    this.rjButton6.BackColor = Color.LimeGreen;
                    break;
                case false:
                    if (this.rjButton6.BackColor == Color.Silver) break;
                    this.rjButton6.BackColor = Color.Silver;
                    break;
                default:
                    break;
            }
            //弧源3复位按钮
            switch ((bool)_readData.TryGetValueW("弧源3复位按钮"))
            {
                case true:
                    if (this.rjButton5.BackColor == Color.LimeGreen) break;
                    this.rjButton5.BackColor = Color.LimeGreen;
                    break;
                case false:
                    if (this.rjButton5.BackColor == Color.Silver) break;
                    this.rjButton5.BackColor = Color.Silver;
                    break;
                default:
                    break;
            }
            //弧源3电压
            this.label18.Text = ((float)_readData.TryGetValueD29("弧源3电压")).ToString("F1") + "V";
            //弧源3电流
            this.label17.Text = ((float)_readData.TryGetValueD29("弧源3电流")).ToString("F1") + "A";
            #endregion

            #region 弧源4数据
            //弧源4引弧按钮
            switch ((bool)_readData.TryGetValueW("弧源4引弧按钮"))
            {
                case true:
                    if (this.rjButton8.BackColor == Color.LimeGreen) break;
                    this.rjButton8.BackColor = Color.LimeGreen;
                    break;
                case false:
                    if (this.rjButton8.BackColor == Color.Silver) break;
                    this.rjButton8.BackColor = Color.Silver;
                    break;
                default:
                    break;
            }
            //弧源4复位按钮
            switch ((bool)_readData.TryGetValueW("弧源4复位按钮"))
            {
                case true:
                    if (this.rjButton7.BackColor == Color.LimeGreen) break;
                    this.rjButton7.BackColor = Color.LimeGreen;
                    break;
                case false:
                    if (this.rjButton7.BackColor == Color.Silver) break;
                    this.rjButton7.BackColor = Color.Silver;
                    break;
                default:
                    break;
            }
            //弧源4电压
            this.label26.Text = ((float)_readData.TryGetValueD29("弧源4电压")).ToString("F1") + "V";
            //弧源4电流
            this.label25.Text = ((float)_readData.TryGetValueD29("弧源4电流")).ToString("F1") + "A";
            #endregion

            #region 弧源5数据
            //弧源5引弧按钮
            switch ((bool)_readData.TryGetValueW("弧源5引弧按钮"))
            {
                case true:
                    if (this.rjButton10.BackColor == Color.LimeGreen) break;
                    this.rjButton10.BackColor = Color.LimeGreen;
                    break;
                case false:
                    if (this.rjButton10.BackColor == Color.Silver) break;
                    this.rjButton10.BackColor = Color.Silver;
                    break;
                default:
                    break;
            }
            //弧源5复位按钮
            switch ((bool)_readData.TryGetValueW("弧源5复位按钮"))
            {
                case true:
                    if (this.rjButton9.BackColor == Color.LimeGreen) break;
                    this.rjButton9.BackColor = Color.LimeGreen;
                    break;
                case false:
                    if (this.rjButton9.BackColor == Color.Silver) break;
                    this.rjButton9.BackColor = Color.Silver;
                    break;
                default:
                    break;
            }
            //弧源5电压
            this.label34.Text = ((float)_readData.TryGetValueD29("弧源5电压")).ToString("F1") + "V";
            //弧源5电流
            this.label33.Text = ((float)_readData.TryGetValueD29("弧源5电流")).ToString("F1") + "A";
            #endregion

            #region 弧源6数据
            //弧源6引弧按钮
            switch ((bool)_readData.TryGetValueW("弧源6引弧按钮"))
            {
                case true:
                    if (this.rjButton12.BackColor == Color.LimeGreen) break;
                    this.rjButton12.BackColor = Color.LimeGreen;
                    break;
                case false:
                    if (this.rjButton12.BackColor == Color.Silver) break;
                    this.rjButton12.BackColor = Color.Silver;
                    break;
                default:
                    break;
            }
            //弧源6复位按钮
            switch ((bool)_readData.TryGetValueW("弧源6复位按钮"))
            {
                case true:
                    if (this.rjButton11.BackColor == Color.LimeGreen) break;
                    this.rjButton11.BackColor = Color.LimeGreen;
                    break;
                case false:
                    if (this.rjButton11.BackColor == Color.Silver) break;
                    this.rjButton11.BackColor = Color.Silver;
                    break;
                default:
                    break;
            }
            //弧源6电压
            this.label42.Text = ((float)_readData.TryGetValueD29("弧源6电压")).ToString("F1") + "V";
            //弧源6电流
            this.label41.Text = ((float)_readData.TryGetValueD29("弧源6电流")).ToString("F1") + "A";
            #endregion

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

        bool _b1State, _b2State, _b3State, _b4State, _b5State, _b6State, _b7State, _b8State;
        bool _r1State, _r2State, _r3State, _r4State, _r5State, _r6State, _r7State, _r8State;

        #region 弧源1
        private void textBox1_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["弧源1电流设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }

        //弧源1电流开关按钮
        private void toggleSwitchs1_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["弧源1电流开关"], toggleSwitchs1.IsOn);
        }
        //弧源1引弧按钮
        private void rjButton1_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            _b1State = !_b1State;
            CommonWrite(_addressMapping["弧源1引弧按钮"], _b1State);
        }
        //弧源1复位按钮
        private async void rjButton2_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_r1State) return;
            _r1State = true;
            try
            {
                CommonWrite(_addressMapping["弧源1复位按钮"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["弧源1复位按钮"], false);
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

        #endregion

        #region 弧源2
        private void textBox2_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["弧源2电流设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        //弧源2电流开关按钮
        private void toggleSwitchs2_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["弧源2电流开关"], toggleSwitchs2.IsOn);
        }
        //弧源2引弧按钮
        private void rjButton4_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            _b2State = !_b2State;
            CommonWrite(_addressMapping["弧源2引弧按钮"], _b2State);
        }
        //弧源2复位按钮
        private async void rjButton3_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_r2State) return;
            _r2State = true;
            try
            {
                CommonWrite(_addressMapping["弧源2复位按钮"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["弧源2复位按钮"], false);
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
        #endregion

        #region 弧源3
        private void textBox3_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["弧源3电流设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        //弧源3电流开关按钮
        private void toggleSwitchs3_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["弧源3电流开关"], toggleSwitchs3.IsOn);
        }
        //弧源3引弧按钮
        private void rjButton6_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            _b3State = !_b3State;
            CommonWrite(_addressMapping["弧源3引弧按钮"], _b3State);
        }
        //弧源3复位按钮
        private async void rjButton5_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_r3State) return;
            _r3State = true;
            try
            {
                CommonWrite(_addressMapping["弧源3复位按钮"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["弧源3复位按钮"], false);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
            finally
            {
                _r3State = false;
            }
        }
        #endregion

        #region 弧源4
        private void textBox4_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["弧源4电流设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        //弧源4电流开关按钮
        private void toggleSwitchs4_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["弧源4电流开关"], toggleSwitchs4.IsOn);
        }
        //弧源4引弧按钮
        private void rjButton8_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            _b4State = !_b4State;
            CommonWrite(_addressMapping["弧源4引弧按钮"], _b4State);
        }
        //弧源4复位按钮
        private async void rjButton7_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_r4State) return;
            _r4State = true;
            try
            {
                CommonWrite(_addressMapping["弧源4复位按钮"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["弧源4复位按钮"], false);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
            finally
            {
                _r4State = false;
            }
        }
        #endregion

        #region 弧源5
        private void textBox5_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["弧源5电流设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        //弧源5电流开关按钮
        private void toggleSwitchs5_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["弧源5电流开关"], toggleSwitchs5.IsOn);
        }
        //弧源5引弧按钮
        private void rjButton10_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            _b5State = !_b5State;
            CommonWrite(_addressMapping["弧源5引弧按钮"], _b5State);
        }

        //弧源5复位按钮
        private async void rjButton9_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_r5State) return;
            _r5State = true;
            try
            {
                CommonWrite(_addressMapping["弧源5复位按钮"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["弧源5复位按钮"], false);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
            finally
            {
                _r5State = false;
            }
        }
        #endregion

        #region 弧源6
        private void textBox6_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["弧源6电流设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        //弧源6电流开关按钮
        private void toggleSwitchs6_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["弧源6电流开关"], toggleSwitchs6.IsOn);
        }
        //弧源6引弧按钮
        private void rjButton12_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            _b6State = !_b6State;
            CommonWrite(_addressMapping["弧源6引弧按钮"], _b6State);
        }

        //弧源6复位按钮
        private async void rjButton11_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (_r6State) return;
            _r6State = true;
            try
            {
                CommonWrite(_addressMapping["弧源6复位按钮"], true);
                await Task.Delay(1000);
                CommonWrite(_addressMapping["弧源6复位按钮"], false);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
            finally
            {
                _r6State = false;
            }
        }
        #endregion

        #region 弧源7
        private void textBox7_NumPadOkPressed(object sender, NumPadValueChangedEventArgs e)
        {
            ConKeyDown(_addressMapping["弧源7电流设定"], Convert.ToUInt16(e.DecimalValue.Value));
        }
        //弧源7电流开关按钮
        private void toggleSwitchs7_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["弧源7电流开关"], toggleSwitchs7.IsOn);
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
        private void toggleSwitchs8_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["弧源8电流开关"], toggleSwitchs8.IsOn);
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
        private void toggleSwitchs9_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["电源总电源"], toggleSwitchs9.IsOn);
        }
        #endregion

        #region 1和2弧电源互锁
        private void toggleSwitchs10_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["1和2弧电源互锁"], toggleSwitchs10.IsOn);
        }
        #endregion

        #region 3和4弧电源互锁
        private void toggleSwitchs11_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["3和4弧电源互锁"], toggleSwitchs11.IsOn);
        }
        #endregion

        #region 5和6弧电源互锁
        private void toggleSwitchs12_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["5和6弧电源互锁"], toggleSwitchs12.IsOn);
        }
        #endregion

        #region 7和8弧电源互锁
        private void toggleSwitchs13_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["7和8弧电源互锁"], toggleSwitchs13.IsOn);
        }
        #endregion

        #region 双断模式
        private void toggleSwitchs14_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["双断模式"], toggleSwitchs14.IsOn);
        }
        #endregion

        #region 加热模式
        private void toggleSwitchs15_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["加热模式"], toggleSwitchs15.IsOn);
        }
        #endregion

        #region 镀膜模式
        private void toggleSwitchs16_Toggled(object sender, EventArgs e)
        {
            CommonWrite(_addressMapping["镀膜模式"], toggleSwitchs16.IsOn);
        }
        #endregion
    }
}
