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
    public partial class HeatRecipe : UserControl
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
        public HeatRecipe(ILogger logger, SqlSugarClient db, IOmronPlcService plc)
        {
            InitializeComponent();
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
            comboBox1.DataSource = _db.Queryable<HeatRecipeMain>().Select(c => c.RecipeName).ToList();
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
                var recipeId = _db.Queryable<HeatRecipeMain>()
                    .Where(m => m.RecipeName == alphaNumTextBox1.Text)
                    .Select(m => m.RecipeID)
                    .First(); // 返回 int，若无记录可能为 0

                // 再基于常量 recipeId 查询子表
                var dataSet = _db.Queryable<HeatRecipeDataSet>()
                    .Where(c => c.RecipeID == recipeId && c.SetIndex == _currentIndex)
                    .First();

                var dataSets = _db.Queryable<HeatRecipeDataSet>()
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
                            _labels[ds.SetIndex - 1].Text = ds.HTime.ToString();
                        }
                        if (ds.HTime > 0)
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
                    toggleSwitchs1.IsOn = dataSet.MFC2SW;
                    toggleSwitchs2.IsOn = dataSet.MFC3SW;
                    toggleSwitchs3.IsOn = dataSet.ARC8SW;
                    toggleSwitchs4.IsOn = dataSet.ARC7SW;
                    numTextBox1.Text = dataSet.MFC3SV.ToString();
                    numTextBox2.Text = dataSet.ARC8SV.ToString();
                    numTextBox3.Text = dataSet.ARC7SV.ToString();
                    numTextBox4.Text = dataSet.HTime.ToString();
                    numTextBox5.Text = dataSet.UpHeat.ToString();
                    numTextBox6.Text = dataSet.DnHeat.ToString();
                    numTextBox7.Text = dataSet.Rotation.ToString();
                    numTextBox8.Text = dataSet.CoolTemp.ToString();
                    numTextBox9.Text = dataSet.Cool.ToString("0.0");
                    numTextBox10.Text = dataSet.Pressure.ToString("0.0000");
                    numTextBox11.Text = dataSet.HighTemp.ToString("0.0");
                    numTextBox12.Text = dataSet.LowTemp.ToString("0.0");
                    numTextBox13.Text = dataSet.MFC2SV.ToString();
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
            var recipeid = _db.Queryable<HeatRecipeMain>().Where(c => c.RecipeName == alphaNumTextBox1.Text).Select(c => c.RecipeID).First();
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
                HeatRecipeDataSet dataSet = new HeatRecipeDataSet();
                dataSet.RecipeID = recipeid;
                dataSet.SetIndex = _currentIndex;
                dataSet.UpHeat = Convert.ToUInt16(numTextBox5.Text);
                dataSet.DnHeat = Convert.ToUInt16(numTextBox6.Text);
                dataSet.Rotation = Convert.ToUInt16(numTextBox7.Text);
                dataSet.MFC2SW = toggleSwitchs1.IsOn;
                dataSet.MFC2SV = Convert.ToUInt16(numTextBox13.Text);
                dataSet.MFC3SW = toggleSwitchs2.IsOn;
                dataSet.MFC3SV = Convert.ToUInt16(numTextBox1.Text);
                dataSet.ARC7SW = toggleSwitchs4.IsOn;
                dataSet.ARC7SV = Convert.ToUInt16(numTextBox3.Text);
                dataSet.ARC8SW = toggleSwitchs3.IsOn;
                dataSet.ARC8SV = Convert.ToUInt16(numTextBox2.Text);
                dataSet.CoolTemp = Convert.ToUInt16(numTextBox8.Text);
                dataSet.HTime = Convert.ToUInt16(numTextBox4.Text);
                dataSet.LowTemp = Convert.ToSingle(numTextBox12.Text);
                dataSet.HighTemp = Convert.ToSingle(numTextBox11.Text);
                dataSet.Pressure = Convert.ToSingle(numTextBox10.Text);
                dataSet.Cool = Convert.ToSingle(numTextBox9.Text);
                _db.Updateable(dataSet).Where(c => c.SetIndex == _currentIndex && c.RecipeID == recipeid).ExecuteCommandHasChange();
                _labels[_currentIndex - 1].Text = numTextBox4.Text;

                var dataSets = _db.Queryable<HeatRecipeDataSet>()
                   .Where(c => c.RecipeID == recipeid)
                   .ToList();
                foreach (var ds in dataSets)
                {
                    if (ds.HTime > 0)
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
            numTextBox13.Text = "0";
            numTextBox14.Text = "0";
            numTextBox15.Text = "0";
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
                _db.Insertable(new HeatRecipeMain
                {
                    RecipeName = alphaNumTextBox1.Text,
                }).ExecuteCommand();
                //创建子配方数据
                // 先获取 recipeId
                var recipeId = _db.Queryable<HeatRecipeMain>()
                    .Where(m => m.RecipeName == alphaNumTextBox1.Text)
                    .Select(m => m.RecipeID)
                    .First();
                for (int i = 1; i < 16; i++)
                {
                    HeatRecipeDataSet dataSet = new HeatRecipeDataSet();
                    dataSet.RecipeID = recipeId;
                    dataSet.SetIndex = i;
                    _db.Insertable(dataSet).ExecuteCommand();
                }
                var ds = _db.Queryable<HeatRecipeMain>().Select(c => c.RecipeName).ToList();
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
                var recipeId = _db.Queryable<HeatRecipeMain>()
                    .Where(m => m.RecipeName == alphaNumTextBox1.Text)
                    .Select(m => m.RecipeID)
                    .First(); // 返回 int，若无记录可能为 0
                // 删除关联的子表数据
                _db.Deleteable<HeatRecipeDataSet>().Where(ds => ds.RecipeID == recipeId).ExecuteCommand();
                _db.Deleteable<HeatRecipeMain>().Where(c => c.RecipeName == alphaNumTextBox1.Text).ExecuteCommand();
                comboBox1.Text = alphaNumTextBox1.Text = "";
                comboBox1.DataSource = _db.Queryable<HeatRecipeMain>().Select(c => c.RecipeName).ToList();
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
            if (_db.Queryable<HeatRecipeMain>().Where(c => c.RecipeName == alphaNumTextBox1.Text).Select(c => c.RecipeID).First() == 0)
            {
                ModernMessageBox.Show("当前配方不存在!请先创建并保存配方!");
                return;
            }
            try
            {
                var recipeId = _db.Queryable<HeatRecipeMain>()
               .Where(m => m.RecipeName == alphaNumTextBox1.Text)
               .Select(m => m.RecipeID)
               .First(); // 返回 int，若无记录可能为 0

                var dataSet = _db.Queryable<HeatRecipeDataSet>().Where(c => c.RecipeID == recipeId && c.SetIndex == Convert.ToUInt16(numTextBox14.Text))
                    .First();

                if (dataSet != null && _plc != null && _plc.IsConnected)
                {
                    var data = new HeatRecipeData
                    {
                        UpHeat = dataSet.UpHeat,
                        DnHeat = dataSet.DnHeat,
                        Rotation = dataSet.Rotation,
                        MFC2SW = dataSet.MFC2SW,
                        MFC2SV = dataSet.MFC2SV,
                        MFC3SW = dataSet.MFC3SW,
                        MFC3SV = dataSet.MFC3SV,
                        ARC7SW = dataSet.ARC7SW,
                        ARC7SV = dataSet.ARC7SV,
                        ARC8SW = dataSet.ARC8SW,
                        ARC8SV = dataSet.ARC8SV,
                        CoolTemp = dataSet.CoolTemp,
                        HTime = dataSet.HTime,
                        LowTemp = dataSet.LowTemp,
                        HighTemp = dataSet.HighTemp,
                        Pressure = dataSet.Pressure,
                        Cool = dataSet.Cool
                    };
                    OperateResult result = _plc.WriteCustomer($"D{21 + 21 * (dataSet.SetIndex - 1)}", data);
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
            if (_db.Queryable<HeatRecipeMain>().Where(c => c.RecipeName == alphaNumTextBox1.Text).Select(c => c.RecipeID).First() == 0)
            {
                ModernMessageBox.Show("当前配方不存在!请先创建并保存配方!");
                return;
            }
            try
            {
                var recipeId = _db.Queryable<HeatRecipeMain>()
               .Where(m => m.RecipeName == alphaNumTextBox1.Text)
               .Select(m => m.RecipeID)
               .First(); // 返回 int，若无记录可能为 0

                var dataSets = _db.Queryable<HeatRecipeDataSet>().Where(c => c.RecipeID == recipeId).OrderBy(c => c.SetIndex)
                    .ToList();

                if (dataSets != null && _plc != null && dataSets.Count != 0 && _plc.IsConnected)
                {
                    foreach (var dataSet in dataSets)
                    {
                        var data = new HeatRecipeData
                        {
                            UpHeat = dataSet.UpHeat,
                            DnHeat = dataSet.DnHeat,
                            Rotation = dataSet.Rotation,
                            MFC2SW = dataSet.MFC2SW,
                            MFC2SV = dataSet.MFC2SV,
                            MFC3SW = dataSet.MFC3SW,
                            MFC3SV = dataSet.MFC3SV,
                            ARC7SW = dataSet.ARC7SW,
                            ARC7SV = dataSet.ARC7SV,
                            ARC8SW = dataSet.ARC8SW,
                            ARC8SV = dataSet.ARC8SV,
                            CoolTemp = dataSet.CoolTemp,
                            HTime = dataSet.HTime,
                            LowTemp = dataSet.LowTemp,
                            HighTemp = dataSet.HighTemp,
                            Pressure = dataSet.Pressure,
                            Cool = dataSet.Cool
                        };
                        //分批下载
                        _plc.WriteCustomer($"D{21 + 21 * (dataSet.SetIndex - 1)}", data);
                    }
                    //下载总序
                    _plc.Write("D340", Convert.ToUInt16(numTextBox15.Text));
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

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            AutofacConfig.isHeatChange = true;
        }
    }
}
