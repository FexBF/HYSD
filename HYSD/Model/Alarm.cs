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
    public partial class Alarm : UserControl, IPollablePage
    {
        private readonly ILogger _logger;
        private readonly IOmronPlcService _plc;
        private readonly IPLCAddressService _address;
        private readonly Dictionary<string, string> _addressMapping;
        private readonly IReadDataService _readData;

        public Alarm(ILogger logger, IOmronPlcService plc, IPLCAddressService address, IReadDataService readData)
        {
            InitializeComponent();
            initializeDataGridView();
            dataGridView1.OptimizeForPerformance();
            this.DoubleBuffered = true;
            _logger = logger;
            _address = address;
            _plc = plc;
            _readData = readData;
            try
            {
                _addressMapping = _address.GetAddressMapping(_address.ReadSheet(), 6, "Address", "Content");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            //this.Load += async (s, e) => await Alarm_Load(s, e);
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

        private void initializeDataGridView()
        {
            dataGridView1.AutoGenerateColumns = false;

            // 基本行为
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToOrderColumns = false;
            dataGridView1.RowHeadersVisible = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 视觉
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.Fixed3D;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridView1.GridColor = Color.LightGray;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.AliceBlue;
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.White;
            dataGridView1.RowTemplate.Height = 24;
            dataGridView1.DefaultCellStyle.Font = new Font("微软雅黑", 9F);
            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            // 性能优化 - 双缓冲
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, dataGridView1, new object[] { true });
        }

        /// <summary>
        /// 给DataGridView添加行号
        /// </summary>
        /// <param name="dgv">dgv控件</param>
        /// <param name="e">dgv参数</param>
        public static void DgvRowPostPaint(DataGridView dgv, DataGridViewRowPostPaintEventArgs e)
        {
            try
            {
                //添加行号 
                SolidBrush solidBrush = new SolidBrush(dgv.RowHeadersDefaultCellStyle.ForeColor);
                string lineNo = (e.RowIndex + 1).ToString();
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                StringFormat sf = new StringFormat();
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;
                e.Graphics.DrawString(lineNo, e.InheritedRowStyle.Font, solidBrush, new Rectangle(e.RowBounds.Location.X, e.RowBounds.Location.Y, dgv.RowHeadersWidth, dgv.RowTemplate.Height), sf);
            }
            catch (Exception ex)
            {
                MessageBox.Show("添加行号时发生错误，错误信息：" + ex.Message, "操作失败");
            }
        }

        private int _alarmCount = 0;
        bool _isShow = true;
        private void DoWork()
        {
            try
            {
                if (_plc != null && _plc.IsConnected && _readData.isRunning)
                {
                    var toRemovePlc = AutofacConfig._alarmDatas.FirstOrDefault(a => a.Address == "PLC0");
                    if (toRemovePlc != null)
                    {
                        AutofacConfig._alarmDatas.Remove(toRemovePlc);
                    }
                    foreach (var kvp in _addressMapping)
                    {
                        if ((bool)_readData.TryGetValueC(kvp.Key))
                        {
                            AutofacConfig._alarmDatas.Add(new AlarmData { Address = kvp.Key, ErrText = kvp.Value, AlarmTime = DateTime.Now });
                        }
                        else
                        {
                            var toRemove = AutofacConfig._alarmDatas.FirstOrDefault(a => a.Address == kvp.Key);
                            if (toRemove != null)
                            {
                                AutofacConfig._alarmDatas.Remove(toRemove);
                            }
                        }
                    }
                }
                else
                {
                    // ★ 修复：断线时只添加一次 "PLC已断开连接" 报警，避免集合无限增长
                    // 原代码每次都 new AlarmData 并 Add，导致 _alarmDatas 每500ms +1，
                    // DataGridView 每500ms 重新绑定越来越大的数据集，UI 线程被阻塞 → 鼠标残影
                    var existing = AutofacConfig._alarmDatas.FirstOrDefault(a => a.Address == "PLC0");
                    if (existing == null)
                    {
                        AutofacConfig._alarmDatas.Add(new AlarmData { Address = "PLC0", ErrText = "PLC已断开连接,请检查网线!", AlarmTime = DateTime.Now });
                    }
                }

                if (AutofacConfig._alarmDatas.Count != _alarmCount || !_isShow)
                {
                    if (this.IsHandleCreated && !this.IsDisposed)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            dataGridView1.SetDataSource(AutofacConfig._alarmDatas.ToList());
                        }));
                        _isShow = true;
                    }
                    else
                    {
                        _isShow = false;
                    }
                }
                _alarmCount = AutofacConfig._alarmDatas.Count;
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }
        }

        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            DgvRowPostPaint(sender as DataGridView, e);
        }
    }
}
