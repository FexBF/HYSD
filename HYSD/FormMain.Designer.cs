namespace HYSD
{
    partial class FormMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                // ★ 事件驱动：窗体释放时取消订阅，避免单例 ReadDataService 持有已释放窗体的引用
                try
                {
                    if (_readData != null)
                    {
                        _readData.DataUpdated -= OnPlcDataUpdated;
                        _readData.ConnectionChanged -= OnPlcConnectionChanged;
                        _readData.Stop();
                    }
                    _cts?.Cancel();
                    _cts?.Dispose();
                    _cts = null;
                }
                catch { }
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.TopPanel = new System.Windows.Forms.Panel();
            this.RjButton_Mini = new RJCodeAdvance.RJControls.RJButton();
            this.RjButton_Close = new RJCodeAdvance.RJControls.RJButton();
            this.MiddlePanel = new System.Windows.Forms.Panel();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lbl_Green = new System.Windows.Forms.Label();
            this.lbl_Yellow = new System.Windows.Forms.Label();
            this.lbl_Red = new System.Windows.Forms.Label();
            this.StatePanel = new System.Windows.Forms.Panel();
            this.Tg_State = new TriangleDemo.TriangleControl();
            this.lbl_State = new System.Windows.Forms.Label();
            this.RjButton_Reset = new RJCodeAdvance.RJControls.RJButton();
            this.RjButton__Stop = new RJCodeAdvance.RJControls.RJButton();
            this.RjButton_Start = new RJCodeAdvance.RJControls.RJButton();
            this.lbl_Time = new System.Windows.Forms.Label();
            this.LeftPanel = new System.Windows.Forms.Panel();
            this.lbl_PlcData = new System.Windows.Forms.Label();
            this.lbl_SetUp = new System.Windows.Forms.Label();
            this.lbl_Water = new System.Windows.Forms.Label();
            this.lbl_HistoryData = new System.Windows.Forms.Label();
            this.lbl_TcRp = new System.Windows.Forms.Label();
            this.lbl_QtksRp = new System.Windows.Forms.Label();
            this.lbl_HeatRp = new System.Windows.Forms.Label();
            this.lbl_ChooseRp = new System.Windows.Forms.Label();
            this.lbl_JK = new System.Windows.Forms.Label();
            this.lbl_Alarm = new System.Windows.Forms.Label();
            this.lbl_Air = new System.Windows.Forms.Label();
            this.lbl_McPower = new System.Windows.Forms.Label();
            this.lbl_PYXQ = new System.Windows.Forms.Label();
            this.lbl_HPower = new System.Windows.Forms.Label();
            this.lbl_Vacuum = new System.Windows.Forms.Label();
            this.MainPanel = new System.Windows.Forms.Panel();
            this.alarmIndicatorLight1 = new HYSDControls.AlarmIndicatorLight();
            this.TopPanel.SuspendLayout();
            this.MiddlePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.StatePanel.SuspendLayout();
            this.LeftPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // TopPanel
            // 
            this.TopPanel.BackColor = System.Drawing.Color.SteelBlue;
            this.TopPanel.Controls.Add(this.RjButton_Mini);
            this.TopPanel.Controls.Add(this.RjButton_Close);
            this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TopPanel.Location = new System.Drawing.Point(0, 0);
            this.TopPanel.Name = "TopPanel";
            this.TopPanel.Size = new System.Drawing.Size(1440, 40);
            this.TopPanel.TabIndex = 0;
            this.TopPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Panel_MouseDown);
            this.TopPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Panel_MouseMove);
            // 
            // RjButton_Mini
            // 
            this.RjButton_Mini.BackColor = System.Drawing.Color.Transparent;
            this.RjButton_Mini.BackgroundColor = System.Drawing.Color.Transparent;
            this.RjButton_Mini.BorderColor = System.Drawing.Color.Transparent;
            this.RjButton_Mini.BorderRadius = 0;
            this.RjButton_Mini.BorderSize = 0;
            this.RjButton_Mini.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.RjButton_Mini.FlatAppearance.BorderSize = 0;
            this.RjButton_Mini.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.RjButton_Mini.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.RjButton_Mini.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RjButton_Mini.Font = new System.Drawing.Font("Arial Rounded MT Bold", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RjButton_Mini.ForeColor = System.Drawing.Color.White;
            this.RjButton_Mini.Location = new System.Drawing.Point(1358, 0);
            this.RjButton_Mini.Name = "RjButton_Mini";
            this.RjButton_Mini.Size = new System.Drawing.Size(40, 40);
            this.RjButton_Mini.TabIndex = 1;
            this.RjButton_Mini.Text = "_";
            this.RjButton_Mini.TextColor = System.Drawing.Color.White;
            this.RjButton_Mini.UseVisualStyleBackColor = false;
            this.RjButton_Mini.Click += new System.EventHandler(this.RjButton_Mini_Click);
            // 
            // RjButton_Close
            // 
            this.RjButton_Close.BackColor = System.Drawing.Color.Transparent;
            this.RjButton_Close.BackgroundColor = System.Drawing.Color.Transparent;
            this.RjButton_Close.BorderColor = System.Drawing.Color.Transparent;
            this.RjButton_Close.BorderRadius = 0;
            this.RjButton_Close.BorderSize = 0;
            this.RjButton_Close.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.RjButton_Close.FlatAppearance.BorderSize = 0;
            this.RjButton_Close.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Red;
            this.RjButton_Close.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
            this.RjButton_Close.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RjButton_Close.Font = new System.Drawing.Font("Arial Rounded MT Bold", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RjButton_Close.ForeColor = System.Drawing.Color.White;
            this.RjButton_Close.Location = new System.Drawing.Point(1400, 0);
            this.RjButton_Close.Name = "RjButton_Close";
            this.RjButton_Close.Size = new System.Drawing.Size(40, 40);
            this.RjButton_Close.TabIndex = 0;
            this.RjButton_Close.Text = "X";
            this.RjButton_Close.TextColor = System.Drawing.Color.White;
            this.RjButton_Close.UseVisualStyleBackColor = false;
            this.RjButton_Close.Click += new System.EventHandler(this.RjButton_Close_Click);
            // 
            // MiddlePanel
            // 
            this.MiddlePanel.BackColor = System.Drawing.Color.SteelBlue;
            this.MiddlePanel.Controls.Add(this.pictureBox3);
            this.MiddlePanel.Controls.Add(this.pictureBox2);
            this.MiddlePanel.Controls.Add(this.alarmIndicatorLight1);
            this.MiddlePanel.Controls.Add(this.pictureBox1);
            this.MiddlePanel.Controls.Add(this.lbl_Green);
            this.MiddlePanel.Controls.Add(this.lbl_Yellow);
            this.MiddlePanel.Controls.Add(this.lbl_Red);
            this.MiddlePanel.Controls.Add(this.StatePanel);
            this.MiddlePanel.Controls.Add(this.lbl_Time);
            this.MiddlePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.MiddlePanel.Location = new System.Drawing.Point(0, 40);
            this.MiddlePanel.Name = "MiddlePanel";
            this.MiddlePanel.Size = new System.Drawing.Size(1440, 80);
            this.MiddlePanel.TabIndex = 2;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::HYSD.Properties.Resources.微信图片_20260427204449_4_35;
            this.pictureBox3.Location = new System.Drawing.Point(0, 0);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(200, 80);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 11;
            this.pictureBox3.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::HYSD.Properties.Resources.PLC2;
            this.pictureBox2.Location = new System.Drawing.Point(1187, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(71, 80);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox2.TabIndex = 10;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Image = global::HYSD.Properties.Resources.报警声音开;
            this.pictureBox1.Location = new System.Drawing.Point(1061, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(74, 80);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // lbl_Green
            // 
            this.lbl_Green.BackColor = System.Drawing.Color.Silver;
            this.lbl_Green.Location = new System.Drawing.Point(432, 56);
            this.lbl_Green.Name = "lbl_Green";
            this.lbl_Green.Size = new System.Drawing.Size(68, 13);
            this.lbl_Green.TabIndex = 7;
            // 
            // lbl_Yellow
            // 
            this.lbl_Yellow.BackColor = System.Drawing.Color.Silver;
            this.lbl_Yellow.Location = new System.Drawing.Point(432, 33);
            this.lbl_Yellow.Name = "lbl_Yellow";
            this.lbl_Yellow.Size = new System.Drawing.Size(68, 13);
            this.lbl_Yellow.TabIndex = 6;
            // 
            // lbl_Red
            // 
            this.lbl_Red.BackColor = System.Drawing.Color.Silver;
            this.lbl_Red.Location = new System.Drawing.Point(432, 10);
            this.lbl_Red.Name = "lbl_Red";
            this.lbl_Red.Size = new System.Drawing.Size(68, 13);
            this.lbl_Red.TabIndex = 5;
            // 
            // StatePanel
            // 
            this.StatePanel.BackColor = System.Drawing.SystemColors.ControlText;
            this.StatePanel.Controls.Add(this.Tg_State);
            this.StatePanel.Controls.Add(this.lbl_State);
            this.StatePanel.Controls.Add(this.RjButton_Reset);
            this.StatePanel.Controls.Add(this.RjButton__Stop);
            this.StatePanel.Controls.Add(this.RjButton_Start);
            this.StatePanel.Location = new System.Drawing.Point(518, 6);
            this.StatePanel.Name = "StatePanel";
            this.StatePanel.Size = new System.Drawing.Size(537, 68);
            this.StatePanel.TabIndex = 4;
            // 
            // Tg_State
            // 
            this.Tg_State.Color = System.Drawing.Color.Silver;
            this.Tg_State.Location = new System.Drawing.Point(9, 18);
            this.Tg_State.Name = "Tg_State";
            this.Tg_State.Rotation = 180F;
            this.Tg_State.Size = new System.Drawing.Size(40, 30);
            this.Tg_State.TabIndex = 4;
            this.Tg_State.Text = "triangleControl1";
            // 
            // lbl_State
            // 
            this.lbl_State.Font = new System.Drawing.Font("楷体", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_State.ForeColor = System.Drawing.Color.White;
            this.lbl_State.Location = new System.Drawing.Point(55, 15);
            this.lbl_State.Name = "lbl_State";
            this.lbl_State.Size = new System.Drawing.Size(173, 40);
            this.lbl_State.TabIndex = 3;
            this.lbl_State.Text = "停止";
            this.lbl_State.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // RjButton_Reset
            // 
            this.RjButton_Reset.BackColor = System.Drawing.Color.Silver;
            this.RjButton_Reset.BackgroundColor = System.Drawing.Color.Silver;
            this.RjButton_Reset.BorderColor = System.Drawing.Color.PaleVioletRed;
            this.RjButton_Reset.BorderRadius = 0;
            this.RjButton_Reset.BorderSize = 0;
            this.RjButton_Reset.FlatAppearance.BorderSize = 0;
            this.RjButton_Reset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RjButton_Reset.Font = new System.Drawing.Font("楷体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RjButton_Reset.ForeColor = System.Drawing.SystemColors.ControlText;
            this.RjButton_Reset.Location = new System.Drawing.Point(442, 15);
            this.RjButton_Reset.Name = "RjButton_Reset";
            this.RjButton_Reset.Size = new System.Drawing.Size(79, 40);
            this.RjButton_Reset.TabIndex = 2;
            this.RjButton_Reset.Text = "复位";
            this.RjButton_Reset.TextColor = System.Drawing.SystemColors.ControlText;
            this.RjButton_Reset.UseVisualStyleBackColor = false;
            this.RjButton_Reset.Click += new System.EventHandler(this.RjButton_Reset_Click);
            // 
            // RjButton__Stop
            // 
            this.RjButton__Stop.BackColor = System.Drawing.Color.Silver;
            this.RjButton__Stop.BackgroundColor = System.Drawing.Color.Silver;
            this.RjButton__Stop.BorderColor = System.Drawing.Color.PaleVioletRed;
            this.RjButton__Stop.BorderRadius = 0;
            this.RjButton__Stop.BorderSize = 0;
            this.RjButton__Stop.FlatAppearance.BorderSize = 0;
            this.RjButton__Stop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RjButton__Stop.Font = new System.Drawing.Font("楷体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RjButton__Stop.ForeColor = System.Drawing.SystemColors.ControlText;
            this.RjButton__Stop.Location = new System.Drawing.Point(345, 15);
            this.RjButton__Stop.Name = "RjButton__Stop";
            this.RjButton__Stop.Size = new System.Drawing.Size(79, 40);
            this.RjButton__Stop.TabIndex = 1;
            this.RjButton__Stop.Text = "停止";
            this.RjButton__Stop.TextColor = System.Drawing.SystemColors.ControlText;
            this.RjButton__Stop.UseVisualStyleBackColor = false;
            this.RjButton__Stop.Click += new System.EventHandler(this.RjButton__Stop_Click);
            // 
            // RjButton_Start
            // 
            this.RjButton_Start.BackColor = System.Drawing.Color.Silver;
            this.RjButton_Start.BackgroundColor = System.Drawing.Color.Silver;
            this.RjButton_Start.BorderColor = System.Drawing.Color.PaleVioletRed;
            this.RjButton_Start.BorderRadius = 0;
            this.RjButton_Start.BorderSize = 0;
            this.RjButton_Start.FlatAppearance.BorderSize = 0;
            this.RjButton_Start.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RjButton_Start.Font = new System.Drawing.Font("楷体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RjButton_Start.ForeColor = System.Drawing.SystemColors.ControlText;
            this.RjButton_Start.Location = new System.Drawing.Point(248, 15);
            this.RjButton_Start.Name = "RjButton_Start";
            this.RjButton_Start.Size = new System.Drawing.Size(79, 40);
            this.RjButton_Start.TabIndex = 0;
            this.RjButton_Start.Text = "启动";
            this.RjButton_Start.TextColor = System.Drawing.SystemColors.ControlText;
            this.RjButton_Start.UseVisualStyleBackColor = false;
            this.RjButton_Start.Click += new System.EventHandler(this.RjButton_Start_Click);
            // 
            // lbl_Time
            // 
            this.lbl_Time.Font = new System.Drawing.Font("楷体", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_Time.ForeColor = System.Drawing.SystemColors.Control;
            this.lbl_Time.Location = new System.Drawing.Point(1313, 3);
            this.lbl_Time.Name = "lbl_Time";
            this.lbl_Time.Size = new System.Drawing.Size(124, 77);
            this.lbl_Time.TabIndex = 3;
            this.lbl_Time.Text = "1";
            this.lbl_Time.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LeftPanel
            // 
            this.LeftPanel.BackColor = System.Drawing.Color.SteelBlue;
            this.LeftPanel.Controls.Add(this.lbl_PlcData);
            this.LeftPanel.Controls.Add(this.lbl_SetUp);
            this.LeftPanel.Controls.Add(this.lbl_Water);
            this.LeftPanel.Controls.Add(this.lbl_HistoryData);
            this.LeftPanel.Controls.Add(this.lbl_TcRp);
            this.LeftPanel.Controls.Add(this.lbl_QtksRp);
            this.LeftPanel.Controls.Add(this.lbl_HeatRp);
            this.LeftPanel.Controls.Add(this.lbl_ChooseRp);
            this.LeftPanel.Controls.Add(this.lbl_JK);
            this.LeftPanel.Controls.Add(this.lbl_Alarm);
            this.LeftPanel.Controls.Add(this.lbl_Air);
            this.LeftPanel.Controls.Add(this.lbl_McPower);
            this.LeftPanel.Controls.Add(this.lbl_PYXQ);
            this.LeftPanel.Controls.Add(this.lbl_HPower);
            this.LeftPanel.Controls.Add(this.lbl_Vacuum);
            this.LeftPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.LeftPanel.Location = new System.Drawing.Point(0, 120);
            this.LeftPanel.Name = "LeftPanel";
            this.LeftPanel.Size = new System.Drawing.Size(200, 780);
            this.LeftPanel.TabIndex = 3;
            // 
            // lbl_PlcData
            // 
            this.lbl_PlcData.BackColor = System.Drawing.Color.Transparent;
            this.lbl_PlcData.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_PlcData.ForeColor = System.Drawing.Color.White;
            this.lbl_PlcData.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_PlcData.Location = new System.Drawing.Point(0, 700);
            this.lbl_PlcData.Name = "lbl_PlcData";
            this.lbl_PlcData.Size = new System.Drawing.Size(200, 50);
            this.lbl_PlcData.TabIndex = 14;
            this.lbl_PlcData.Text = "PLC数据";
            this.lbl_PlcData.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_SetUp
            // 
            this.lbl_SetUp.BackColor = System.Drawing.Color.Transparent;
            this.lbl_SetUp.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_SetUp.ForeColor = System.Drawing.Color.White;
            this.lbl_SetUp.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_SetUp.Location = new System.Drawing.Point(0, 650);
            this.lbl_SetUp.Name = "lbl_SetUp";
            this.lbl_SetUp.Size = new System.Drawing.Size(200, 50);
            this.lbl_SetUp.TabIndex = 13;
            this.lbl_SetUp.Text = "设置";
            this.lbl_SetUp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_Water
            // 
            this.lbl_Water.BackColor = System.Drawing.Color.Transparent;
            this.lbl_Water.Cursor = System.Windows.Forms.Cursors.Default;
            this.lbl_Water.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_Water.ForeColor = System.Drawing.Color.White;
            this.lbl_Water.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_Water.Location = new System.Drawing.Point(0, 600);
            this.lbl_Water.Name = "lbl_Water";
            this.lbl_Water.Size = new System.Drawing.Size(200, 50);
            this.lbl_Water.TabIndex = 12;
            this.lbl_Water.Text = "水回路";
            this.lbl_Water.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_HistoryData
            // 
            this.lbl_HistoryData.BackColor = System.Drawing.Color.Transparent;
            this.lbl_HistoryData.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_HistoryData.ForeColor = System.Drawing.Color.White;
            this.lbl_HistoryData.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_HistoryData.Location = new System.Drawing.Point(0, 550);
            this.lbl_HistoryData.Name = "lbl_HistoryData";
            this.lbl_HistoryData.Size = new System.Drawing.Size(200, 50);
            this.lbl_HistoryData.TabIndex = 11;
            this.lbl_HistoryData.Text = "历史数据";
            this.lbl_HistoryData.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_TcRp
            // 
            this.lbl_TcRp.BackColor = System.Drawing.Color.Transparent;
            this.lbl_TcRp.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_TcRp.ForeColor = System.Drawing.Color.White;
            this.lbl_TcRp.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_TcRp.Location = new System.Drawing.Point(0, 500);
            this.lbl_TcRp.Name = "lbl_TcRp";
            this.lbl_TcRp.Size = new System.Drawing.Size(200, 50);
            this.lbl_TcRp.TabIndex = 10;
            this.lbl_TcRp.Text = "涂层配方";
            this.lbl_TcRp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_QtksRp
            // 
            this.lbl_QtksRp.BackColor = System.Drawing.Color.Transparent;
            this.lbl_QtksRp.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_QtksRp.ForeColor = System.Drawing.Color.White;
            this.lbl_QtksRp.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_QtksRp.Location = new System.Drawing.Point(0, 450);
            this.lbl_QtksRp.Name = "lbl_QtksRp";
            this.lbl_QtksRp.Size = new System.Drawing.Size(200, 50);
            this.lbl_QtksRp.TabIndex = 9;
            this.lbl_QtksRp.Text = "气体刻蚀配方";
            this.lbl_QtksRp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_HeatRp
            // 
            this.lbl_HeatRp.BackColor = System.Drawing.Color.Transparent;
            this.lbl_HeatRp.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_HeatRp.ForeColor = System.Drawing.Color.White;
            this.lbl_HeatRp.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_HeatRp.Location = new System.Drawing.Point(0, 400);
            this.lbl_HeatRp.Name = "lbl_HeatRp";
            this.lbl_HeatRp.Size = new System.Drawing.Size(200, 50);
            this.lbl_HeatRp.TabIndex = 8;
            this.lbl_HeatRp.Text = "加热配方";
            this.lbl_HeatRp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_ChooseRp
            // 
            this.lbl_ChooseRp.BackColor = System.Drawing.Color.Transparent;
            this.lbl_ChooseRp.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_ChooseRp.ForeColor = System.Drawing.Color.White;
            this.lbl_ChooseRp.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_ChooseRp.Location = new System.Drawing.Point(0, 350);
            this.lbl_ChooseRp.Name = "lbl_ChooseRp";
            this.lbl_ChooseRp.Size = new System.Drawing.Size(200, 50);
            this.lbl_ChooseRp.TabIndex = 7;
            this.lbl_ChooseRp.Text = "选择配方";
            this.lbl_ChooseRp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_JK
            // 
            this.lbl_JK.BackColor = System.Drawing.Color.Transparent;
            this.lbl_JK.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_JK.ForeColor = System.Drawing.Color.White;
            this.lbl_JK.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_JK.Location = new System.Drawing.Point(0, 300);
            this.lbl_JK.Name = "lbl_JK";
            this.lbl_JK.Size = new System.Drawing.Size(200, 50);
            this.lbl_JK.TabIndex = 6;
            this.lbl_JK.Text = "监控";
            this.lbl_JK.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_Alarm
            // 
            this.lbl_Alarm.BackColor = System.Drawing.Color.Transparent;
            this.lbl_Alarm.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_Alarm.ForeColor = System.Drawing.Color.White;
            this.lbl_Alarm.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_Alarm.Location = new System.Drawing.Point(0, 250);
            this.lbl_Alarm.Name = "lbl_Alarm";
            this.lbl_Alarm.Size = new System.Drawing.Size(200, 50);
            this.lbl_Alarm.TabIndex = 5;
            this.lbl_Alarm.Text = "实时报警";
            this.lbl_Alarm.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_Air
            // 
            this.lbl_Air.BackColor = System.Drawing.Color.Transparent;
            this.lbl_Air.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_Air.ForeColor = System.Drawing.Color.White;
            this.lbl_Air.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_Air.Location = new System.Drawing.Point(0, 200);
            this.lbl_Air.Name = "lbl_Air";
            this.lbl_Air.Size = new System.Drawing.Size(200, 50);
            this.lbl_Air.TabIndex = 4;
            this.lbl_Air.Text = "气体";
            this.lbl_Air.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_McPower
            // 
            this.lbl_McPower.BackColor = System.Drawing.Color.Transparent;
            this.lbl_McPower.Cursor = System.Windows.Forms.Cursors.Default;
            this.lbl_McPower.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_McPower.ForeColor = System.Drawing.Color.White;
            this.lbl_McPower.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_McPower.Location = new System.Drawing.Point(0, 150);
            this.lbl_McPower.Name = "lbl_McPower";
            this.lbl_McPower.Size = new System.Drawing.Size(200, 50);
            this.lbl_McPower.TabIndex = 3;
            this.lbl_McPower.Text = "脉冲电源";
            this.lbl_McPower.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_PYXQ
            // 
            this.lbl_PYXQ.BackColor = System.Drawing.Color.Transparent;
            this.lbl_PYXQ.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_PYXQ.ForeColor = System.Drawing.Color.White;
            this.lbl_PYXQ.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_PYXQ.Location = new System.Drawing.Point(0, 100);
            this.lbl_PYXQ.Name = "lbl_PYXQ";
            this.lbl_PYXQ.Size = new System.Drawing.Size(200, 50);
            this.lbl_PYXQ.TabIndex = 2;
            this.lbl_PYXQ.Text = "偏压和线圈";
            this.lbl_PYXQ.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_HPower
            // 
            this.lbl_HPower.BackColor = System.Drawing.Color.Transparent;
            this.lbl_HPower.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_HPower.ForeColor = System.Drawing.Color.White;
            this.lbl_HPower.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_HPower.Location = new System.Drawing.Point(0, 50);
            this.lbl_HPower.Name = "lbl_HPower";
            this.lbl_HPower.Size = new System.Drawing.Size(200, 50);
            this.lbl_HPower.TabIndex = 1;
            this.lbl_HPower.Text = "弧电源";
            this.lbl_HPower.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_Vacuum
            // 
            this.lbl_Vacuum.BackColor = System.Drawing.Color.Transparent;
            this.lbl_Vacuum.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_Vacuum.ForeColor = System.Drawing.Color.White;
            this.lbl_Vacuum.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_Vacuum.Location = new System.Drawing.Point(0, 0);
            this.lbl_Vacuum.Name = "lbl_Vacuum";
            this.lbl_Vacuum.Size = new System.Drawing.Size(200, 50);
            this.lbl_Vacuum.TabIndex = 0;
            this.lbl_Vacuum.Text = "真空";
            this.lbl_Vacuum.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainPanel
            // 
            this.MainPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPanel.Location = new System.Drawing.Point(200, 120);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(1240, 780);
            this.MainPanel.TabIndex = 4;
            // 
            // alarmIndicatorLight1
            // 
            this.alarmIndicatorLight1.AlarmColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(173)))), ((int)(((byte)(58)))));
            this.alarmIndicatorLight1.Cursor = System.Windows.Forms.Cursors.Default;
            this.alarmIndicatorLight1.IdleColor = System.Drawing.Color.White;
            this.alarmIndicatorLight1.IsAlarming = false;
            this.alarmIndicatorLight1.Location = new System.Drawing.Point(1141, 21);
            this.alarmIndicatorLight1.Name = "alarmIndicatorLight1";
            this.alarmIndicatorLight1.SignColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.alarmIndicatorLight1.Size = new System.Drawing.Size(40, 40);
            this.alarmIndicatorLight1.TabIndex = 9;
            this.alarmIndicatorLight1.Text = "alarmIndicatorLight1";
            // 
            // FormMain
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1440, 900);
            this.Controls.Add(this.MainPanel);
            this.Controls.Add(this.LeftPanel);
            this.Controls.Add(this.MiddlePanel);
            this.Controls.Add(this.TopPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "恒宇时代";
            this.TopPanel.ResumeLayout(false);
            this.MiddlePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.StatePanel.ResumeLayout(false);
            this.LeftPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel TopPanel;
        private System.Windows.Forms.Panel MiddlePanel;
        private System.Windows.Forms.Panel LeftPanel;
        private RJCodeAdvance.RJControls.RJButton RjButton_Close;
        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.Label lbl_Vacuum;
        private System.Windows.Forms.Label lbl_HeatRp;
        private System.Windows.Forms.Label lbl_JK;
        private System.Windows.Forms.Label lbl_Alarm;
        private System.Windows.Forms.Label lbl_Air;
        private System.Windows.Forms.Label lbl_PYXQ;
        private System.Windows.Forms.Label lbl_HPower;
        private RJCodeAdvance.RJControls.RJButton RjButton_Mini;
        private System.Windows.Forms.Label lbl_McPower;
        private System.Windows.Forms.Label lbl_ChooseRp;
        private System.Windows.Forms.Label lbl_PlcData;
        private System.Windows.Forms.Label lbl_SetUp;
        private System.Windows.Forms.Label lbl_Water;
        private System.Windows.Forms.Label lbl_HistoryData;
        private System.Windows.Forms.Label lbl_TcRp;
        private System.Windows.Forms.Label lbl_QtksRp;
        private RJCodeAdvance.RJControls.RJButton RjButton_Start;
        private RJCodeAdvance.RJControls.RJButton RjButton__Stop;
        private RJCodeAdvance.RJControls.RJButton RjButton_Reset;
        private System.Windows.Forms.Label lbl_Time;
        private System.Windows.Forms.Panel StatePanel;
        private System.Windows.Forms.Label lbl_State;
        private System.Windows.Forms.Label lbl_Yellow;
        private System.Windows.Forms.Label lbl_Red;
        private System.Windows.Forms.Label lbl_Green;
        private TriangleDemo.TriangleControl Tg_State;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private HYSDControls.AlarmIndicatorLight alarmIndicatorLight1;
    }
}

