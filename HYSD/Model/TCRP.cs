using HslCommunication;
using Serilog;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HYSD
{
    public partial class TCRP : UserControl
    {
        // 定义初始样式和选中样式
        private readonly Color _normalBg = Color.Silver;
        private readonly Color _normalFont = Color.Black;
        private readonly Color _activeBg = Color.Aqua;
        private readonly Color _activeFont = Color.Black;

        // 用 List 管理所有按钮，方便遍历
        private List<Button> _buttons = new List<Button>();
        private readonly List<Label> _labels = new List<Label>();
        private readonly ILogger _logger;
        private readonly SqlSugarClient _db;
        private readonly IOmronPlcService _plc;
        public TCRP(ILogger logger, SqlSugarClient db, IOmronPlcService plc)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            _logger = logger;
            _db = db;
            _plc = plc;
            InitButtonGroup();
        }


        private void InitButtonGroup()
        {
            // 把需要联动的按钮加入列表
            _buttons.Add(rjButton1);
            _buttons.Add(rjButton2);
            _buttons.Add(rjButton3);
            _buttons.Add(rjButton4);
            _buttons.Add(rjButton5);
            _buttons.Add(rjButton6);
            _buttons.Add(rjButton7);
            _buttons.Add(rjButton8);
            _buttons.Add(rjButton9);
            _buttons.Add(rjButton10);
            _buttons.Add(rjButton11);
            _buttons.Add(rjButton12);
            _buttons.Add(rjButton13);
            _buttons.Add(rjButton14);
            _buttons.Add(rjButton15);
            _buttons.Add(rjButton32);
            _buttons.Add(rjButton31);
            _buttons.Add(rjButton30);
            _buttons.Add(rjButton29);
            _buttons.Add(rjButton28);
            _buttons.Add(rjButton27);
            _buttons.Add(rjButton26);
            _buttons.Add(rjButton25);
            _buttons.Add(rjButton24);
            _buttons.Add(rjButton23);
            _buttons.Add(rjButton22);
            _buttons.Add(rjButton21);
            _buttons.Add(rjButton20);
            _buttons.Add(rjButton19);
            _buttons.Add(rjButton18);
            _labels.Add(label5);
            _labels.Add(label6);
            _labels.Add(label7);
            _labels.Add(label8);
            _labels.Add(label9);
            _labels.Add(label10);
            _labels.Add(label11);
            _labels.Add(label12);
            _labels.Add(label13);
            _labels.Add(label14);
            _labels.Add(label15);
            _labels.Add(label16);
            _labels.Add(label17);
            _labels.Add(label18);
            _labels.Add(label19);
            _labels.Add(label87);
            _labels.Add(label86);
            _labels.Add(label85);
            _labels.Add(label84);
            _labels.Add(label83);
            _labels.Add(label82);
            _labels.Add(label81);
            _labels.Add(label80);
            _labels.Add(label79);
            _labels.Add(label78);
            _labels.Add(label77);
            _labels.Add(label76);
            _labels.Add(label75);
            _labels.Add(label74);
            _labels.Add(label73);
            // 给每个按钮绑定同一个事件
            foreach (var btn in _buttons)
            {
                btn.BackColor = _normalBg;
                btn.ForeColor = _normalFont;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = Color.Silver;
                btn.Cursor = Cursors.Hand;
                btn.Click += ButtonGroup_Click;
            }
            //await Task.Run(() =>
            //{
            //    // 异步加载配方名到 comboBox1
            //    var recipeNames = _db.Queryable<TCRecipeMain>().Select(c => c.RecipeName).ToList();
            //    this.BeginInvoke((Action)(() =>
            //    {
            //        comboBox1.DataSource = recipeNames;
            //        alphaNumTextBox1.Text = comboBox1.SelectedItem?.ToString() ?? "";
            //        ButtonGroup_Click(rjButton1, EventArgs.Empty); // 默认选中第一个按钮
            //    }));
            //});
            comboBox1.DataSource = _db.Queryable<TCRecipeMain>().Select(c => c.RecipeName).ToList();
            alphaNumTextBox1.Text = comboBox1.SelectedItem?.ToString() ?? "";
            ButtonGroup_Click(rjButton1, EventArgs.Empty); // 默认选中第一个按钮
        }


        int _currentIndex; // 当前选中按钮的索引
        private List<int> _setIndexs = new List<int>(); // 已经设置过的序列表
        // 核心：所有按钮共用这一个点击事件
        private void ButtonGroup_Click(object sender, EventArgs e)
        {
            try
            {
                var clickedBtn = (Button)sender;
                int i = 1;
                // 1. 先把所有按钮恢复初始状态
                foreach (var btn in _buttons)
                {
                    btn.BackColor = _normalBg;
                    btn.ForeColor = _normalFont;
                    btn.FlatAppearance.BorderColor = Color.Silver;
                    btn.Text = $"{i}"; // 可根据需要设置默认文本
                    i++;
                }

                // 2. 再把当前点击的按钮设为选中状态
                clickedBtn.BackColor = _activeBg;
                clickedBtn.ForeColor = _activeFont;
                clickedBtn.FlatAppearance.BorderColor = _activeBg;
                clickedBtn.Text = "编辑"; // 可根据需要设置按钮文本

                _currentIndex = _buttons.IndexOf(clickedBtn) + 1; // 记录当前选中按钮的索引（1-15）
                numTextBox14.Text = _currentIndex.ToString();

                // 查询ID为1的配方，并加载它的所有数据集，按组号排序
                // 先获取 recipeId
                var recipeId = _db.Queryable<TCRecipeMain>()
                    .Where(m => m.RecipeName == alphaNumTextBox1.Text)
                    .Select(m => m.RecipeID)
                    .First(); // 返回 int，若无记录可能为 0

                // 再基于常量 recipeId 查询子表
                var dataSet = _db.Queryable<TCRecipeDataSet>()
                    .Where(c => c.RecipeID == recipeId && c.SetIndex == _currentIndex)
                    .First();

                var dataSets = _db.Queryable<TCRecipeDataSet>()
                    .Where(c => c.RecipeID == recipeId)
                    .ToList();
                if (dataSets?.Count != 0)
                {
                    foreach (var label in _labels)
                    {
                        label.Text = "0";
                    }
                    foreach (var ds in dataSets)
                    {
                        if (ds.SetIndex != 0)
                        {
                            _labels[ds.SetIndex - 1].Text = ds.CTime.ToString();
                        }
                        if (ds.CTime > 0)
                        {
                            _setIndexs.Add(ds.SetIndex);
                        }
                    }
                }
                else
                {
                    // 如果没有数据集，清空界面上的控件
                    rjButtonInit_Click(null, null);
                    foreach (var label in _labels)
                    {
                        label.Text = "0";
                    }
                }
                numTextBox15.Text = _setIndexs.Count > 0 ? _setIndexs.Max().ToString() : "0";
                _setIndexs.Clear();

                if (dataSet != null)
                {
                    toggleSwitchs1.IsOn = dataSet.PF;
                    toggleSwitchs2.IsOn = dataSet.MFC1SW;
                    toggleSwitchs3.IsOn = dataSet.MFC4SW;
                    toggleSwitchs4.IsOn = dataSet.MFC3SW;
                    toggleSwitchs5.IsOn = dataSet.BiasSW;
                    toggleSwitchs6.IsOn = dataSet.ARC2SW;
                    toggleSwitchs7.IsOn = dataSet.ARC3SW;
                    toggleSwitchs8.IsOn = dataSet.ARC5SW;
                    toggleSwitchs9.IsOn = dataSet.ARC6SW;
                    toggleSwitchs10.IsOn = dataSet.ARC4SW;
                    toggleSwitchs11.IsOn = dataSet.ARC1SW;
                    toggleSwitchs12.IsOn = dataSet.CoilSW;
                    numTextBox1.Text = dataSet.MFC1SV.ToString();
                    numTextBox2.Text = dataSet.MFC4SV.ToString();
                    numTextBox3.Text = dataSet.MFC3SV.ToString();
                    numTextBox4.Text = dataSet.CTime.ToString();
                    numTextBox5.Text = dataSet.UpHeat.ToString();
                    numTextBox6.Text = dataSet.DnHeat.ToString();
                    numTextBox7.Text = dataSet.Rotation.ToString();
                    numTextBox8.Text = dataSet.CoolTemp.ToString();
                    numTextBox9.Text = dataSet.Cool.ToString("0.0");
                    numTextBox10.Text = dataSet.Pressure.ToString("0.0000");
                    numTextBox11.Text = dataSet.HighTemp.ToString("0.0");
                    numTextBox12.Text = dataSet.LowTemp.ToString("0.0");
                    numTextBox13.Text = dataSet.CDG100DSV.ToString("0.0000");
                    numTextBox16.Text = dataSet.BiasVolt.ToString();
                    numTextBox17.Text = dataSet.BiasThe.ToString();
                    numTextBox18.Text = dataSet.BiasDuty.ToString();
                    numTextBox19.Text = dataSet.BiasKHz.ToString();
                    numTextBox20.Text = dataSet.ARC2SV.ToString();
                    numTextBox21.Text = dataSet.ARC3SV.ToString();
                    numTextBox22.Text = dataSet.ARC6SV.ToString();
                    numTextBox23.Text = dataSet.ARC5SV.ToString();
                    numTextBox24.Text = dataSet.ARC4SV.ToString();
                    numTextBox25.Text = dataSet.ARC1SV.ToString();
                    numTextBox26.Text = dataSet.CoilL.ToString();
                    numTextBox27.Text = dataSet.CoilT2.ToString();
                    numTextBox28.Text = dataSet.CoilT1.ToString();
                    numTextBox29.Text = dataSet.CoilT0.ToString();
                    numTextBox30.Text = dataSet.CoilH.ToString();
                    numTextBox31.Text = dataSet.CoilT3.ToString();
                }
                else
                {
                    // 如果没有数据集，清空界面上的控件
                    rjButtonInit_Click(null, null);
                }
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }

        }

        //保存配方
        private void rjButtonSave_Click(object sender, EventArgs e)
        {
            var recipeid = _db.Queryable<TCRecipeMain>().Where(c => c.RecipeName == alphaNumTextBox1.Text).Select(c => c.RecipeID).First();
            if (string.IsNullOrEmpty(alphaNumTextBox1.Text))
            {
                ModernMessageBox.Show("配方名不能为空!");
                return;
            }
            if (recipeid == 0)
            {
                ModernMessageBox.Show("当前配方不存在!请先创建并保存配方!");
                return;
            }
            try
            {
                TCRecipeDataSet dataSet = new TCRecipeDataSet();
                dataSet.RecipeID = recipeid;
                dataSet.SetIndex = _currentIndex;

                dataSet.PF = toggleSwitchs1.IsOn;
                dataSet.MFC1SW = toggleSwitchs2.IsOn;
                dataSet.MFC4SW = toggleSwitchs3.IsOn;
                dataSet.MFC3SW = toggleSwitchs4.IsOn;
                dataSet.BiasSW = toggleSwitchs5.IsOn;
                dataSet.ARC2SW = toggleSwitchs6.IsOn;
                dataSet.ARC3SW = toggleSwitchs7.IsOn;
                dataSet.ARC5SW = toggleSwitchs8.IsOn;
                dataSet.ARC6SW = toggleSwitchs9.IsOn;
                dataSet.ARC4SW = toggleSwitchs10.IsOn;
                dataSet.ARC1SW = toggleSwitchs11.IsOn;
                dataSet.CoilSW = toggleSwitchs12.IsOn;
                dataSet.MFC1SV = Convert.ToUInt16(numTextBox1.Text);
                dataSet.MFC4SV = Convert.ToUInt16(numTextBox2.Text);
                dataSet.MFC3SV = Convert.ToUInt16(numTextBox3.Text);
                dataSet.CTime = Convert.ToUInt16(numTextBox4.Text);
                dataSet.UpHeat = Convert.ToUInt16(numTextBox5.Text);
                dataSet.DnHeat = Convert.ToUInt16(numTextBox6.Text);
                dataSet.Rotation = Convert.ToUInt16(numTextBox7.Text);
                dataSet.CoolTemp = Convert.ToUInt16(numTextBox8.Text);
                dataSet.Cool = Convert.ToSingle(numTextBox9.Text);
                dataSet.Pressure = Convert.ToSingle(numTextBox10.Text);
                dataSet.HighTemp = Convert.ToSingle(numTextBox11.Text);
                dataSet.LowTemp = Convert.ToSingle(numTextBox12.Text);
                dataSet.CDG100DSV = Convert.ToSingle(numTextBox13.Text);
                dataSet.BiasVolt = Convert.ToUInt16(numTextBox16.Text);
                dataSet.BiasThe = Convert.ToUInt16(numTextBox17.Text);
                dataSet.BiasDuty = Convert.ToUInt16(numTextBox18.Text);
                dataSet.BiasKHz = Convert.ToUInt16(numTextBox19.Text);
                dataSet.ARC2SV = Convert.ToUInt16(numTextBox20.Text);
                dataSet.ARC3SV = Convert.ToUInt16(numTextBox21.Text);
                dataSet.ARC6SV = Convert.ToUInt16(numTextBox22.Text);
                dataSet.ARC5SV = Convert.ToUInt16(numTextBox23.Text);
                dataSet.ARC4SV = Convert.ToUInt16(numTextBox24.Text);
                dataSet.ARC1SV = Convert.ToUInt16(numTextBox25.Text);
                dataSet.CoilL = Convert.ToUInt16(numTextBox26.Text);
                dataSet.CoilT2 = Convert.ToUInt16(numTextBox27.Text);
                dataSet.CoilT1 = Convert.ToUInt16(numTextBox28.Text);
                dataSet.CoilT0 = Convert.ToUInt16(numTextBox29.Text);
                dataSet.CoilH = Convert.ToUInt16(numTextBox30.Text);
                dataSet.CoilT3 = Convert.ToUInt16(numTextBox31.Text);


                _db.Updateable(dataSet).Where(c => c.SetIndex == _currentIndex && c.RecipeID == recipeid).ExecuteCommandHasChange();
                _labels[_currentIndex - 1].Text = numTextBox4.Text;

                var dataSets = _db.Queryable<TCRecipeDataSet>()
                .Where(c => c.RecipeID == recipeid)
                .ToList();
                foreach (var ds in dataSets)
                {
                    if (ds.CTime > 0)
                    {
                        _setIndexs.Add(ds.SetIndex);
                    }
                }
                numTextBox15.Text = _setIndexs.Count > 0 ? _setIndexs.Max().ToString() : "0";
                _setIndexs.Clear();
            }
            catch (Exception ex)
            {
                _logger.Debug(ex.Message);
                ModernMessageBox.Show("保存失败!错误信息: " + ex.Message);
            }

        }

        //初始化配方数据
        private void rjButtonInit_Click(object sender, EventArgs e)
        {
            toggleSwitchs1.IsOn = false;
            toggleSwitchs2.IsOn = false;
            toggleSwitchs3.IsOn = false;
            toggleSwitchs4.IsOn = false;
            toggleSwitchs5.IsOn = false;
            toggleSwitchs6.IsOn = false;
            toggleSwitchs7.IsOn = false;
            toggleSwitchs8.IsOn = false;
            toggleSwitchs9.IsOn = false;
            toggleSwitchs10.IsOn = false;
            toggleSwitchs11.IsOn = false;
            toggleSwitchs12.IsOn = false;
            numTextBox1.Text = "0";
            numTextBox2.Text = "0";
            numTextBox3.Text = "0";
            numTextBox4.Text = "0";
            numTextBox5.Text = "0";
            numTextBox6.Text = "0";
            numTextBox7.Text = "0";
            numTextBox8.Text = "0";
            numTextBox9.Text = "0.0";
            numTextBox10.Text = "0.0000";
            numTextBox11.Text = "0.0";
            numTextBox12.Text = "0.0";
            numTextBox13.Text = "0.0000";
            numTextBox14.Text = "0";
            numTextBox15.Text = "0";
            numTextBox16.Text = "0";
            numTextBox17.Text = "0";
            numTextBox18.Text = "0";
            numTextBox19.Text = "0";
            numTextBox20.Text = "0";
            numTextBox21.Text = "0";
            numTextBox22.Text = "0";
            numTextBox23.Text = "0";
            numTextBox24.Text = "0";
            numTextBox25.Text = "0";
            numTextBox26.Text = "0";
            numTextBox27.Text = "0";
            numTextBox28.Text = "0";
            numTextBox29.Text = "0";
            numTextBox30.Text = "0";
            numTextBox31.Text = "0";
            label22.Text = "0.0E+000mbar";
        }

        //保存配方名
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(alphaNumTextBox1.Text))
            {
                ModernMessageBox.Show("配方名不能为空!");
                return;
            }
            try
            {
                //创建配方
                _db.Insertable(new TCRecipeMain
                {
                    RecipeName = alphaNumTextBox1.Text,
                }).ExecuteCommand();
                //创建子配方数据
                // 先获取 recipeId
                var recipeId = _db.Queryable<TCRecipeMain>()
                    .Where(m => m.RecipeName == alphaNumTextBox1.Text)
                    .Select(m => m.RecipeID)
                    .First();
                for (int i = 1; i < 31; i++)
                {
                    TCRecipeDataSet dataSet = new TCRecipeDataSet();
                    dataSet.RecipeID = recipeId;
                    dataSet.SetIndex = i;
                    _db.Insertable(dataSet).ExecuteCommand();
                }
                var ds = _db.Queryable<TCRecipeMain>().Select(c => c.RecipeName).ToList();
                comboBox1.DataSource = ds;
                comboBox1.SelectedIndex = ds.Count - 1;
                ModernMessageBox.Show("保存成功!");
            }
            catch (Exception ex)
            {
                _logger.Debug(ex.Message);

                // SQLite 唯一约束冲突的错误消息通常包含 "CONSTRAINT" 和 "UNIQUE"
                if (ex.Message.Contains("constraint") && ex.Message.Contains("UNIQUE"))
                {
                    ModernMessageBox.Show("该名字已存在，保存失败！");
                }
                else
                {
                    ModernMessageBox.Show("保存失败：" + ex.Message);
                }
            }

        }

        //删除配方及其数据
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(alphaNumTextBox1.Text))
            {
                ModernMessageBox.Show("配方名不能为空!");
                return;
            }
            try
            {
                DialogResult result = ModernMessageBox.Show("确定删除该配方吗?");
                if (result == DialogResult.Cancel) return;
                // 先获取 recipeId
                var recipeId = _db.Queryable<TCRecipeMain>()
                    .Where(m => m.RecipeName == alphaNumTextBox1.Text)
                    .Select(m => m.RecipeID)
                    .First(); // 返回 int，若无记录可能为 0
                // 删除关联的子表数据
                _db.Deleteable<TCRecipeDataSet>().Where(ds => ds.RecipeID == recipeId).ExecuteCommand();
                _db.Deleteable<TCRecipeMain>().Where(c => c.RecipeName == alphaNumTextBox1.Text).ExecuteCommand();
                comboBox1.Text = alphaNumTextBox1.Text = "";
                comboBox1.DataSource = _db.Queryable<TCRecipeMain>().Select(c => c.RecipeName).ToList();
                _db.Ado.CommitTran();
            }
            catch (Exception ex)
            {
                _db.Ado.RollbackTran();
                _logger.Debug(ex.Message);
                ModernMessageBox.Show("删除失败!错误信息: " + ex.Message);
            }
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboBox1.Text))
            {
                alphaNumTextBox1.Text = "";
                ButtonGroup_Click(rjButton1, EventArgs.Empty); // 切换配方时默认选中第一个按钮
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            alphaNumTextBox1.Text = comboBox1.SelectedItem?.ToString() ?? "";
            ButtonGroup_Click(rjButton1, EventArgs.Empty); // 切换配方时默认选中第一个按钮
        }

        //单序下载
        private void rjButton16_Click(object sender, EventArgs e)
        {
            if (numTextBox14.Text == "0")
            {
                ModernMessageBox.Show("序不能为0!");
                return;
            }
            if (string.IsNullOrEmpty(alphaNumTextBox1.Text))
            {
                ModernMessageBox.Show("配方名不能为空!");
                return;
            }
            if (_db.Queryable<TCRecipeMain>().Where(c => c.RecipeName == alphaNumTextBox1.Text).Select(c => c.RecipeID).First() == 0)
            {
                ModernMessageBox.Show("当前配方不存在!请先创建并保存配方!");
                return;
            }
            try
            {
                var recipeId = _db.Queryable<TCRecipeMain>()
               .Where(m => m.RecipeName == alphaNumTextBox1.Text)
               .Select(m => m.RecipeID)
               .First(); // 返回 int，若无记录可能为 0

                var dataSet = _db.Queryable<TCRecipeDataSet>().Where(c => c.RecipeID == recipeId && c.SetIndex == Convert.ToUInt16(numTextBox14.Text))
                    .First();

                if (dataSet != null && _plc != null && _plc.IsConnected)
                {
                    var data = new TCRecipeData
                    {
                        UpHeat = dataSet.UpHeat,
                        DnHeat = dataSet.DnHeat,
                        Rotation = dataSet.Rotation,
                        MFC1SW = dataSet.MFC1SW,
                        MFC1SV = dataSet.MFC1SV,
                        MFC2SW = dataSet.MFC2SW,
                        MFC2SV = dataSet.MFC2SV,
                        MFC3SW = dataSet.MFC3SW,
                        MFC3SV = dataSet.MFC3SV,
                        MFC4SW = dataSet.MFC4SW,
                        MFC4SV = dataSet.MFC4SV,
                        CoolTemp = dataSet.CoolTemp,
                        CTime = dataSet.CTime,
                        BiasSW = dataSet.BiasSW,
                        BiasKHz = dataSet.BiasKHz,
                        BiasVolt = dataSet.BiasVolt,
                        ARC1SW = dataSet.ARC1SW,
                        ARC1SV = dataSet.ARC1SV,
                        ARC2SW = dataSet.ARC2SW,
                        ARC2SV = dataSet.ARC2SV,
                        ARC3SW = dataSet.ARC3SW,
                        ARC3SV = dataSet.ARC3SV,
                        ARC4SW = dataSet.ARC4SW,
                        ARC4SV = dataSet.ARC4SV,
                        ARC5SW = dataSet.ARC5SW,
                        ARC5SV = dataSet.ARC5SV,
                        ARC6SW = dataSet.ARC6SW,
                        ARC6SV = dataSet.ARC6SV,
                        CoilSW = dataSet.CoilSW,
                        BiasDuty = dataSet.BiasDuty,
                        BiasThe = dataSet.BiasThe,
                        CoilH = dataSet.CoilH,
                        CoilT0 = dataSet.CoilT0,
                        CoilT1 = dataSet.CoilT1,
                        CoilL = dataSet.CoilL,
                        CoilT2 = dataSet.CoilT2,
                        CoilT3 = dataSet.CoilT3,
                        PF = dataSet.PF,
                        CDG100DSV = dataSet.CDG100DSV,
                        LowTemp = dataSet.LowTemp,
                        HighTemp = dataSet.HighTemp,
                        Pressure = dataSet.Pressure,
                        Cool = dataSet.Cool,

                    };
                    OperateResult result = _plc.WriteCustomer($"D{2048 + 48 * (dataSet.SetIndex - 1)}", data);
                    if (result.IsSuccess)
                    {
                        ModernMessageBox.Show("单序下载成功!");
                    }
                    else
                    {
                        ModernMessageBox.Show("单序下载失败!");
                    }
                }
                else
                {
                    ModernMessageBox.Show("单序下载失败!");
                }
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
            }

        }
        //总序下载
        private void rjButton17_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(alphaNumTextBox1.Text))
            {
                ModernMessageBox.Show("配方名不能为空!");
                return;
            }
            if (_db.Queryable<TCRecipeMain>().Where(c => c.RecipeName == alphaNumTextBox1.Text).Select(c => c.RecipeID).First() == 0)
            {
                ModernMessageBox.Show("当前配方不存在!请先创建并保存配方!");
                return;
            }
            try
            {
                var recipeId = _db.Queryable<TCRecipeMain>()
               .Where(m => m.RecipeName == alphaNumTextBox1.Text)
               .Select(m => m.RecipeID)
               .First(); // 返回 int，若无记录可能为 0

                var dataSets = _db.Queryable<TCRecipeDataSet>().Where(c => c.RecipeID == recipeId).OrderBy(c => c.SetIndex)
                    .ToList();

                if (dataSets != null && _plc != null && dataSets.Count != 0 && _plc.IsConnected)
                {
                    foreach (var dataSet in dataSets)
                    {
                        var data = new TCRecipeData
                        {
                            UpHeat = dataSet.UpHeat,
                            DnHeat = dataSet.DnHeat,
                            Rotation = dataSet.Rotation,
                            MFC1SW = dataSet.MFC1SW,
                            MFC1SV = dataSet.MFC1SV,
                            MFC2SW = dataSet.MFC2SW,
                            MFC2SV = dataSet.MFC2SV,
                            MFC3SW = dataSet.MFC3SW,
                            MFC3SV = dataSet.MFC3SV,
                            MFC4SW = dataSet.MFC4SW,
                            MFC4SV = dataSet.MFC4SV,
                            CoolTemp = dataSet.CoolTemp,
                            CTime = dataSet.CTime,
                            BiasSW = dataSet.BiasSW,
                            BiasKHz = dataSet.BiasKHz,
                            BiasVolt = dataSet.BiasVolt,
                            ARC1SW = dataSet.ARC1SW,
                            ARC1SV = dataSet.ARC1SV,
                            ARC2SW = dataSet.ARC2SW,
                            ARC2SV = dataSet.ARC2SV,
                            ARC3SW = dataSet.ARC3SW,
                            ARC3SV = dataSet.ARC3SV,
                            ARC4SW = dataSet.ARC4SW,
                            ARC4SV = dataSet.ARC4SV,
                            ARC5SW = dataSet.ARC5SW,
                            ARC5SV = dataSet.ARC5SV,
                            ARC6SW = dataSet.ARC6SW,
                            ARC6SV = dataSet.ARC6SV,
                            CoilSW = dataSet.CoilSW,
                            BiasDuty = dataSet.BiasDuty,
                            BiasThe = dataSet.BiasThe,
                            CoilH = dataSet.CoilH,
                            CoilT0 = dataSet.CoilT0,
                            CoilT1 = dataSet.CoilT1,
                            CoilL = dataSet.CoilL,
                            CoilT2 = dataSet.CoilT2,
                            CoilT3 = dataSet.CoilT3,
                            PF = dataSet.PF,
                            CDG100DSV = dataSet.CDG100DSV,
                            LowTemp = dataSet.LowTemp,
                            HighTemp = dataSet.HighTemp,
                            Pressure = dataSet.Pressure,
                            Cool = dataSet.Cool,
                        };
                        //分批下载
                        _plc.WriteCustomer($"D{2048 + 48 * (dataSet.SetIndex - 1)}", data);
                    }
                    //下载总序
                    _plc.Write("D348", Convert.ToUInt16(numTextBox15.Text));
                    ModernMessageBox.Show("配方下载成功!");
                }
                else
                {
                    ModernMessageBox.Show("配方下载失败!");
                }
            }
            catch (Exception ex)
            {

                _logger.Debug(ex.Message);
                ModernMessageBox.Show("配方下载出错:" + ex.Message);
            }
        }

        private void rjButton33_Click(object sender, EventArgs e)
        {
            label22.Text = Convert.ToSingle(numTextBox13.Text).ToString("E1") + "mbar";
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            AutofacConfig.isTCRPChange = true;
        }
    }
}
