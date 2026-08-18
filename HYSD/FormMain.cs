using Autofac;
using Serilog;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace HYSD
{
    public enum Primary
    {
        Red50 = 16772078,
        Red100 = 16764370,
        Red200 = 15702682,
        Red300 = 15037299,
        Red400 = 15684432,
        Red500 = 16007990,
        Red600 = 15022389,
        Red700 = 13840175,
        Red800 = 12986408,
        Red900 = 12000284,
        Pink50 = 16573676,
        Pink100 = 16301008,
        Pink200 = 16027569,
        Pink300 = 15753874,
        Pink400 = 15483002,
        Pink500 = 15277667,
        Pink600 = 14162784,
        Pink700 = 12720219,
        Pink800 = 11342935,
        Pink900 = 8916559,
        Purple50 = 15984117,
        Purple100 = 14794471,
        Purple200 = 13538264,
        Purple300 = 12216520,
        Purple400 = 11225020,
        Purple500 = 10233776,
        Purple600 = 9315498,
        Purple700 = 8069026,
        Purple800 = 6953882,
        Purple900 = 4854924,
        DeepPurple50 = 15591414,
        DeepPurple100 = 13747433,
        DeepPurple200 = 11771355,
        DeepPurple300 = 9795021,
        DeepPurple400 = 8280002,
        DeepPurple500 = 6765239,
        DeepPurple600 = 6174129,
        DeepPurple700 = 5320104,
        DeepPurple800 = 4532128,
        DeepPurple900 = 3218322,
        Indigo50 = 15264502,
        Indigo100 = 12962537,
        Indigo200 = 10463450,
        Indigo300 = 7964363,
        Indigo400 = 6056896,
        Indigo500 = 4149685,
        Indigo600 = 3754411,
        Indigo700 = 3162015,
        Indigo800 = 2635155,
        Indigo900 = 1713022,
        Blue50 = 14938877,
        Blue100 = 12312315,
        Blue200 = 9489145,
        Blue300 = 6600182,
        Blue400 = 4367861,
        Blue500 = 2201331,
        Blue600 = 2001125,
        Blue700 = 1668818,
        Blue800 = 1402304,
        Blue900 = 870305,
        LightBlue50 = 14808574,
        LightBlue100 = 11789820,
        LightBlue200 = 8508666,
        LightBlue300 = 5227511,
        LightBlue400 = 2733814,
        LightBlue500 = 240116,
        LightBlue600 = 236517,
        LightBlue700 = 166097,
        LightBlue800 = 161725,
        LightBlue900 = 87963,
        Cyan50 = 14743546,
        Cyan100 = 11725810,
        Cyan200 = 8445674,
        Cyan300 = 5099745,
        Cyan400 = 2541274,
        Cyan500 = 48340,
        Cyan600 = 44225,
        Cyan700 = 38823,
        Cyan800 = 33679,
        Cyan900 = 24676,
        Teal50 = 14742257,
        Teal100 = 11722715,
        Teal200 = 8440772,
        Teal300 = 5093036,
        Teal400 = 2533018,
        Teal500 = 38536,
        Teal600 = 35195,
        Teal700 = 31083,
        Teal800 = 26972,
        Teal900 = 19776,
        Green50 = 15267305,
        Green100 = 13166281,
        Green200 = 10868391,
        Green300 = 8505220,
        Green400 = 6732650,
        Green500 = 5025616,
        Green600 = 4431943,
        Green700 = 3706428,
        Green800 = 3046706,
        Green900 = 1793568,
        LightGreen50 = 15857897,
        LightGreen100 = 14478792,
        LightGreen200 = 12968357,
        LightGreen300 = 11457921,
        LightGreen400 = 10275941,
        LightGreen500 = 9159498,
        LightGreen600 = 8172354,
        LightGreen700 = 6856504,
        LightGreen800 = 5606191,
        LightGreen900 = 3369246,
        Lime50 = 16382951,
        Lime100 = 15791299,
        Lime200 = 15134364,
        Lime300 = 14477173,
        Lime400 = 13951319,
        Lime500 = 13491257,
        Lime600 = 12634675,
        Lime700 = 11514923,
        Lime800 = 10394916,
        Lime900 = 8550167,
        Yellow50 = 16776679,
        Yellow100 = 16775620,
        Yellow200 = 16774557,
        Yellow300 = 16773494,
        Yellow400 = 16772696,
        Yellow500 = 16771899,
        Yellow600 = 16635957,
        Yellow700 = 16498733,
        Yellow800 = 16361509,
        Yellow900 = 16088855,
        Amber50 = 16775393,
        Amber100 = 16772275,
        Amber200 = 16769154,
        Amber300 = 16766287,
        Amber400 = 16763432,
        Amber500 = 16761095,
        Amber600 = 16757504,
        Amber700 = 16752640,
        Amber800 = 16748288,
        Amber900 = 16740096,
        Orange50 = 16774112,
        Orange100 = 16769202,
        Orange200 = 16764032,
        Orange300 = 16758605,
        Orange400 = 16754470,
        Orange500 = 16750592,
        Orange600 = 16485376,
        Orange700 = 16088064,
        Orange800 = 15690752,
        Orange900 = 15094016,
        DeepOrange50 = 16509415,
        DeepOrange100 = 16764092,
        DeepOrange200 = 16755601,
        DeepOrange300 = 16747109,
        DeepOrange400 = 16740419,
        DeepOrange500 = 16733986,
        DeepOrange600 = 16011550,
        DeepOrange700 = 15092249,
        DeepOrange800 = 14172949,
        DeepOrange900 = 12531212,
        Brown50 = 15723497,
        Brown100 = 14142664,
        Brown200 = 12364452,
        Brown300 = 10586239,
        Brown400 = 9268835,
        Brown500 = 7951688,
        Brown600 = 7162945,
        Brown700 = 6111287,
        Brown800 = 5125166,
        Brown900 = 4073251,
        Grey50 = 16448250,
        Grey100 = 16119285,
        Grey200 = 15658734,
        Grey300 = 14737632,
        Grey400 = 12434877,
        Grey500 = 10395294,
        Grey600 = 7697781,
        Grey700 = 6381921,
        Grey800 = 4342338,
        Grey900 = 2171169,
        BlueGrey50 = 15527921,
        BlueGrey100 = 13621468,
        BlueGrey200 = 11583173,
        BlueGrey300 = 9479342,
        BlueGrey400 = 7901340,
        BlueGrey500 = 6323595,
        BlueGrey600 = 5533306,
        BlueGrey700 = 4545124,
        BlueGrey800 = 3622735,
        BlueGrey900 = 2503224
    }
    public partial class FormMain : Form
    {
        private readonly SqlSugarClient _db;
        private readonly ILogger _logger;
        private readonly ILifetimeScope _scope; // Autofac 的生命周期作用域

        // 颜色定义
        private readonly Color _activeColor = Color.FromArgb(32, 32, 32);
        private readonly Color _inactiveColor = Color.Transparent;
        private readonly Color _hoverColor;

        // 记录当前选中的 Label，防止重复刷新
        private Label _currentActiveLabel;
        private readonly IColorService _colorService;
        private readonly IOmronPlcService _plc;
        private readonly IPLCAddressService _address;
        private readonly IReadDataService _readData;
        private readonly Dictionary<string, string> _addressMapping;
        private CancellationTokenSource _cts;

        // 新增类字段（放在类内其他私有字段旁）
        private readonly Image _imgPlc1;
        private readonly Image _imgPlc2;

        public FormMain(ILogger logger, SqlSugarClient db, ILifetimeScope scope, IColorService colorService, IOmronPlcService plc, IPLCAddressService address, IReadDataService readData)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            _logger = logger;
            _db = db;
            _scope = scope;
            _colorService = colorService;
            _hoverColor = _colorService.ToColor((int)Primary.Indigo300);
            _plc = plc;
            _address = address;
            _readData = readData;
            _readData.Start();

            // ★ 给 lbl_Time 开启双缓冲，避免每秒 Text 赋值时直接画到屏幕
            typeof(Label).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.SetProperty,
                null, this.lbl_Time, new object[] { true });
            typeof(Panel).InvokeMember("DoubleBuffered",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.SetProperty,
             null, this.MiddlePanel, new object[] { true });
            try
            {
                _addressMapping = _address.GetAddressMapping(_address.ReadSheet(), 2, "Name", "Address");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            this.lbl_Time.Text = DateTime.Now.ToString(" HH:mm:ss \nyyyy/MM/dd");
            // 预加载资源为共享实例，避免每次赋值创建新对象/泄露 GDI
            _imgPlc1 = global::HYSD.Properties.Resources.PLC1;
            _imgPlc2 = global::HYSD.Properties.Resources.PLC2;


            // ★ 事件驱动：订阅 PLC 数据刷新事件，在 UI 线程刷新顶栏 PLC 指示
            // 事件由后台读取线程触发，需 BeginInvoke 切回 UI 线程后再调用 DoWork
            _readData.DataUpdated += OnPlcDataUpdated;
            // ★ 事件驱动：订阅 PLC 连接状态变化事件，仅在状态跃迁时切换图标
            _readData.ConnectionChanged += OnPlcConnectionChanged;

            alarmIndicatorLight1.AlarmClicked += (s, e) =>
            {
                // 切换到报警页面
                if (_currentActiveLabel != lbl_Alarm)
                {
                    SetInactiveStyle(_currentActiveLabel);
                    SetActiveStyle(lbl_Alarm);
                    _currentActiveLabel = lbl_Alarm;
                    SwitchPage<Alarm>();
                }
            };
            bool _isConnect = false;
            this.Load += (s, e) =>
            {
                // ★ 启动加速：Alarm 延迟到窗体绘制后初始化
                // Load 事件触发时句柄已创建，BeginInvoke 安全
                // BeginInvoke 投递到消息队列末尾，让窗体先完成首次绘制
                this.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        Alarm page = _scope.Resolve<Alarm>();
                        page.StartPolling();
                        HistoryData historyPage = _scope.Resolve<HistoryData>();
                        historyPage.StartPolling();

                        // ★★★ 软件启动时解析 JK 画面，触发其构造函数中的"自动选中上次批次数据库 + 开启涂层数据监控线程"逻辑 ★★★
                        // JK 注册为 SingleInstance，Resolve 仅在首次调用时执行构造函数；
                        // 构造函数内会调用 AutoStartMonitoringOnStartup()，自动选中最近一次生成的 .db 并启动监控线程。
                        // 此处不调用 StartPolling()，避免与后续 SwitchPage<JK>() 重复订阅 DataUpdated 事件。
                        // 注意：JK 是默认显示画面（lbl_JK 默认激活），用户首次点击 JK 标签时 SwitchPage 会复用此实例。
                        var jkPage = _scope.Resolve<JK>();
                    }
                    catch (Exception ex) { _logger.Debug(ex.Message); }
                }));

                //// 🌟 遍历窗体上的所有 Panel，强行开启双缓冲
                //foreach (Control c in this.Controls)
                //{
                //    if (c is Panel)
                //    {
                //        // 使用反射，突破 protected 限制
                //        typeof(Panel).GetProperty("DoubleBuffered",
                //            System.Reflection.BindingFlags.Instance |
                //            System.Reflection.BindingFlags.NonPublic)?
                //            .SetValue(c, true, null);
                //    }
                //}

                //开启多线程
                _cts = new CancellationTokenSource();
                Task.Run(() =>
                {
                    // ★ 启动加速：建表移到后台线程，不阻塞 UI 首次绘制
                    // InitTables 是 7 张表的 CREATE TABLE IF NOT EXISTS，同步执行约 50-200ms
                    try
                    {
                        _db.CodeFirst.InitTables(typeof(HeatRecipeMain), typeof(HeatRecipeDataSet), typeof(QtksRecipeMain), typeof(QtksRecipeDataSet), typeof(TCRecipeMain), typeof(TCRecipeDataSet), typeof(TCData));
                    }
                    catch (Exception ex) { _logger.Debug(ex.Message); }

                    while (!_cts.Token.IsCancellationRequested)
                    {
                        if (!_isConnect)
                        {
                            var result = _plc.ReadInt16(_addressMapping["运行状态"]);
                            if (!result.IsSuccess)
                            {
                                _logger.Debug($"读取 PLC 运行状态失败: {result.Message}");
                                _isConnect = false;
                                Thread.Sleep(5000);   // ★ 5 秒后再试，不要退出
                                continue;             // ← 改 return → continue
                            }
                            _isConnect = true;
                        }
                        Thread.Sleep(5000); // 每5秒更新一次
                    }
                }, _cts.Token);
            };
            this.RjButton_Close.FlatAppearance.BorderColor = _colorService.ToColor((int)Primary.Indigo900);
            this.RjButton_Mini.FlatAppearance.BorderColor = _colorService.ToColor((int)Primary.Indigo900);
            this.RjButton_Mini.FlatAppearance.MouseDownBackColor = _colorService.ToColor((int)Primary.Indigo300);
            this.RjButton_Mini.FlatAppearance.MouseOverBackColor = _colorService.ToColor((int)Primary.Indigo300);
            this.TopPanel.BackColor = _colorService.ToColor((int)Primary.Indigo900);
            this.MiddlePanel.BackColor = _colorService.ToColor((int)Primary.Indigo800);
            this.LeftPanel.BackColor = _colorService.ToColor((int)Primary.Indigo700);

            // 初始化：默认选中 Home
            SetActiveStyle(lbl_JK);
            _currentActiveLabel = lbl_JK;

            #region 绑定事件
            BindLabelEvents<Vacuum>(lbl_Vacuum);
            BindLabelEvents<HPower>(lbl_HPower);
            BindLabelEvents<PYXQ>(lbl_PYXQ);
            BindLabelEvents<McPower>(lbl_McPower);
            BindLabelEvents<Air>(lbl_Air);
            BindLabelEvents<Alarm>(lbl_Alarm);
            BindLabelEvents<JK>(lbl_JK);
            BindLabelEvents<ChooseRp>(lbl_ChooseRp);
            BindLabelEvents<HeatRecipe>(lbl_HeatRp);
            BindLabelEvents<QtksRecipe>(lbl_QtksRp);
            BindLabelEvents<TCRP>(lbl_TcRp);
            BindLabelEvents<HistoryData>(lbl_HistoryData);
            BindLabelEvents<Water>(lbl_Water);
            BindLabelEvents<SetUp>(lbl_SetUp);
            BindLabelEvents<PLCData>(lbl_PlcData);
            #endregion

            // 初始加载首页
            // ★ 启动加速：先显示 JK 首页，Alarm 延迟到 Load 事件后初始化
            // （构造函数里句柄未创建，BeginInvoke 会报错，改在 Load 里调用）
            SwitchPage<JK>();
        }

        private void DoWork()
        {
            try
            {
                AlarmCheck();
                this.lbl_Time.Text = DateTime.Now.ToString(" HH:mm:ss \nyyyy/MM/dd");
                if (_plc != null && _plc.IsConnected && _readData.isRunning)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        PlcDataEx();
                    }));
                }
                else
                {
                    _isChecked = false; // 连接断开时重置检查状态
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex.Message);
            }
        }

        /// <summary>★ 事件驱动：DataUpdated 回调（后台读取线程触发），切回 UI 线程刷新顶栏 PLC 指示</summary>
        private void OnPlcDataUpdated(object sender, EventArgs e)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            try { this.BeginInvoke((Action)DoWork); }
            catch (InvalidOperationException) { /* 句柄未就绪，忽略 */ }
            catch { /* 吞掉单次异常，保持订阅存活 */ }
        }

        /// <summary>★ 事件驱动：ConnectionChanged 回调（后台读取线程触发），切回 UI 线程切换连接图标</summary>
        private void OnPlcConnectionChanged(object sender, bool connected)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            try { this.BeginInvoke((Action)(() => UpdatePlcConnectionIcon(connected))); }
            catch (InvalidOperationException) { /* 句柄未就绪，忽略 */ }
            catch { }
        }

        /// <summary>仅在状态变化时替换图片，使用 ReferenceEquals 做引用比较避免 GDI 泄漏</summary>
        private void UpdatePlcConnectionIcon(bool connected)
        {
            var img = connected ? _imgPlc1 : _imgPlc2;
            if (!object.ReferenceEquals(pictureBox2.Image, img))
            {
                pictureBox2.Image = img;
            }
        }

        private void AlarmCheck()
        {
            if (AutofacConfig._alarmDatas.Count > 0)
            {
                if (this.alarmIndicatorLight1.IsAlarming) return;
                this.alarmIndicatorLight1.IsAlarming = true;
            }
            else if (AutofacConfig._alarmDatas.Count == 0)
            {
                if (!this.alarmIndicatorLight1.IsAlarming) return;
                this.alarmIndicatorLight1.IsAlarming = false;
            }
        }
        bool _isChecked = false;
        private void PlcDataEx()
        {
            if (!_isChecked)
            {
                _alarmState = (bool)_readData.TryGetValueW("报警声");
                _isChecked = true;
            }
            switch ((ushort)_readData.TryGetValueD19("运行状态"))
            {
                case 0:
                    if (this.lbl_State.Text == "停止") break;
                    this.lbl_State.Text = "停止";
                    this.lbl_State.ForeColor = Color.White;
                    break;
                case 1:
                    if (this.lbl_State.Text == "加热运行中") break;
                    this.lbl_State.Text = "加热运行中";
                    this.lbl_State.ForeColor = Color.FromArgb(0, 255, 64);
                    break;
                case 2:
                    if (this.lbl_State.Text == "气体刻蚀运行中") break;
                    this.lbl_State.Text = "气体刻蚀运行中";
                    this.lbl_State.ForeColor = Color.FromArgb(0, 255, 64);
                    break;
                case 3:
                    if (this.lbl_State.Text == "涂层运行中") break;
                    this.lbl_State.Text = "涂层运行中";
                    this.lbl_State.ForeColor = Color.FromArgb(0, 255, 64);
                    break;
                default:
                    break;
            }

            switch ((bool)_readData.TryGetValueW("三角显示"))
            {
                case false:
                    if (this.Tg_State.Color == Color.Silver) break;
                    this.Tg_State.Color = Color.Silver;
                    break;
                case true:
                    if (this.Tg_State.Color == Color.FromArgb(0, 255, 64)) break;
                    this.Tg_State.Color = Color.FromArgb(0, 255, 64);
                    break;
                default:
                    break;
            }
            //指示灯红
            switch ((ushort)_readData.TryGetValueD19("指示灯红"))
            {
                case 0:
                    if (this.lbl_Red.BackColor == Color.Silver) break;
                    this.lbl_Red.BackColor = Color.Silver;
                    break;
                case 1:
                    if (this.lbl_Red.BackColor == Color.Red) break;
                    this.lbl_Red.BackColor = Color.Red;
                    break;
                default:
                    break;
            }
            //指示灯黄
            switch ((ushort)_readData.TryGetValueD15("指示灯黄"))
            {
                case 0:
                    if (this.lbl_Yellow.BackColor == Color.Silver) break;
                    this.lbl_Yellow.BackColor = Color.Silver;
                    break;
                case 1:
                    if (this.lbl_Yellow.BackColor == Color.Yellow) break;
                    this.lbl_Yellow.BackColor = Color.Yellow;
                    break;
                default:
                    break;
            }
            //指示灯绿
            switch ((bool)_readData.TryGetValueW("指示灯绿"))
            {
                case false:
                    if (this.lbl_Green.BackColor == Color.Silver) break;
                    this.lbl_Green.BackColor = Color.Silver;
                    break;
                case true:
                    if (this.lbl_Green.BackColor == Color.Lime) break;
                    this.lbl_Green.BackColor = Color.Lime;
                    break;
                default:
                    break;
            }
            //启动按钮
            switch ((bool)_readData.TryGetValueW("启动按钮"))
            {
                case false:
                    if (this.RjButton_Start.BackColor == Color.Silver) break;
                    this.RjButton_Start.BackColor = Color.Silver;
                    break;
                case true:
                    if (this.RjButton_Start.BackColor == Color.LimeGreen) break;
                    this.RjButton_Start.BackColor = Color.LimeGreen;
                    break;
                default:
                    break;
            }
            //停止按钮
            switch ((bool)_readData.TryGetValueW("停止按钮"))
            {
                case false:
                    if (this.RjButton__Stop.BackColor == Color.Silver) break;
                    this.RjButton__Stop.BackColor = Color.Silver;
                    break;
                case true:
                    if (this.RjButton__Stop.BackColor == Color.Yellow) break;
                    this.RjButton__Stop.BackColor = Color.Yellow;
                    break;
                default:
                    break;
            }
            //复位按钮
            switch ((bool)_readData.TryGetValueW("复位按钮"))
            {
                case false:
                    if (this.RjButton_Reset.BackColor == Color.Silver) break;
                    this.RjButton_Reset.BackColor = Color.Silver;
                    break;
                case true:
                    if (this.RjButton_Reset.BackColor == Color.Cyan) break;
                    this.RjButton_Reset.BackColor = Color.Cyan;
                    break;
                default:
                    break;
            }
        }

        #region 页面切换
        private void BindLabelEvents<T>(Label lbl) where T : UserControl
        {
            lbl.Cursor = Cursors.Hand; // 鼠标变手型
            lbl.Click += (s, e) =>
            {
                if (_currentActiveLabel == lbl) return; // 如果点击的是当前页，不操作

                // 1. 重置旧标签
                SetInactiveStyle(_currentActiveLabel);

                // 2. 激活新标签
                SetActiveStyle(lbl);
                _currentActiveLabel = lbl;

                // 3. 切换页面
                SwitchPage<T>();
            };

            lbl.MouseEnter += (s, e) =>
            {
                if (lbl != _currentActiveLabel) lbl.BackColor = _hoverColor;
            };

            lbl.MouseLeave += (s, e) =>
            {
                if (lbl != _currentActiveLabel) lbl.BackColor = _inactiveColor;
            };
        }

        private void SetActiveStyle(Label lbl)
        {
            if (lbl == null) return;
            lbl.BackColor = _activeColor;
        }

        private void SetInactiveStyle(Label lbl)
        {
            if (lbl == null) return;
            lbl.BackColor = _inactiveColor;
        }

        // 核心方法：通过泛型动态创建并切换页面
        private void SwitchPage<T>() where T : UserControl
        {
            // ★ 修复：切走旧页面前，停止其后台轮询循环。
            // 原代码只做 Controls.Clear()，但页面注册为 SingleInstance，
            // 旧实例的后台 Task.Run 循环仍在运行，每秒向已不可见的控件投递 BeginInvoke，
            // 造成线程泄漏 + CPU 空转。切换 N 次就泄漏 N 个线程。
            foreach (Control c in MainPanel.Controls)
            {
                if (c is IPollablePage oldPage)
                {
                    if (oldPage is Alarm || oldPage is HistoryData) continue;
                    try { oldPage.StopPolling(); } catch { }
                }
            }

            // 1. 清空当前容器里的页面
            MainPanel.Controls.Clear();

            // 2. 用 Autofac 解析出新的页面实例（会自动注入它需要的 Service）
            var page = _scope.Resolve<T>();

            // 3. 设置填满容器并加入
            // page.Dock = DockStyle.Fill;
            MainPanel.Controls.Add(page);

            // ★ 切入新页面后，启动其轮询循环
            if (page is IPollablePage newPage)
            {
                if (newPage is HistoryData || newPage is Alarm) return;
                try { newPage.StartPolling(); } catch { }
            }
        }
        #endregion

        #region 无边框拖动
        private Point mPoint;
        private void Panel_MouseDown(object sender, MouseEventArgs e)
        {
            mPoint = new Point(e.X, e.Y);
        }
        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Location = new Point(this.Location.X + e.X - mPoint.X, this.Location.Y + e.Y - mPoint.Y);
            }
        }
        #endregion

        #region 关闭窗口
        private void RjButton_Close_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        #endregion

        #region 最小化
        private void RjButton_Mini_Click(object sender, EventArgs e)
        {
            // 将窗体状态设置为最小化
            this.WindowState = FormWindowState.Minimized;
        }
        #endregion

        private void RjButton_Start_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;

            try
            {
                _plc.Write(_addressMapping["启动按钮"], true);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
        }

        private void RjButton__Stop_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;

            try
            {
                _plc.Write(_addressMapping["停止按钮"], true);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
        }

        bool isResetting = false; // 防止重复点击复位按钮
        private async void RjButton_Reset_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;

            if (isResetting) return; // 如果正在复位，直接返回
            isResetting = true;

            try
            {
                _plc.Write(_addressMapping["复位按钮"], true);
                await Task.Delay(1000);
                _plc.Write(_addressMapping["复位按钮"], false);
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
            finally
            {
                isResetting = false; // 无论成功还是失败，都重置状态
            }
        }

        bool _alarmState; // 当前报警状态
        //报警声
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            _alarmState = !_alarmState;
            try
            {
                if (_plc != null && _plc.IsConnected)
                {
                    _plc.Write(_addressMapping["报警声"], _alarmState);
                    pictureBox1.Image = _alarmState ? global::HYSD.Properties.Resources.报警声音关 : global::HYSD.Properties.Resources.报警声音开;
                }
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
        }
    }
}
