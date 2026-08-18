using Serilog;
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
    public partial class PLCQtksRecipeData : UserControl
    {
        private readonly IOmronPlcService _plc;
        private readonly ILogger _logger;
        public PLCQtksRecipeData(IOmronPlcService plc, ILogger logger)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            _plc = plc;
            _logger = logger;
            dgv_AEGD.AutoGenerateColumns = false;
            dgv_AEGD.OptimizeForPerformance();
        }

        private List<QtksRecipeData> _qtksRecipes;
        bool isUp = false;
        private async void rjButton1_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;
            if (isUp) return; // 如果正在上载，直接返回
            isUp = true;
            // ★ 修复：原代码在 UI 线程同步读 15 次 PLC + 1 次 ReadUInt16，
            // 每次最多 3 秒超时，合计可能冻结 UI 数十秒。
            // 改为后台线程读取，完成后回 UI 线程绑定数据。
            List<QtksRecipeData> recipes = null;
            ushort recipeNo = 0;
            bool ok = false;
            string errMsg = null;
            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                {
                    recipes = new List<QtksRecipeData>();
                    for (int i = 1; i < 16; i++)
                    {
                        var r = _plc.ReadCustomer<QtksRecipeData>($"D{434 + 34 * (i - 1)}");
                        if (!r.IsSuccess) { errMsg = r.Message; return; }
                        var h = r.Content;
                        h.ID = i;
                        recipes.Add(h);
                    }
                    var rn = _plc.ReadUInt16("D946");
                    if (rn.IsSuccess) recipeNo = rn.Content;
                    ok = true;
                });
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            try
            {
                if (ok && recipes != null)
                {
                    _qtksRecipes = recipes;
                    dgv_AEGD.SetDataSource(_qtksRecipes);
                    label3.Text = recipeNo.ToString();
                }
                else if (errMsg != null)
                {
                    _logger.Debug(errMsg);
                    ModernMessageBox.Show("读取失败!" + errMsg, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                isUp = false;
            }
        }
    }
}

//try
//{
//    if (_plc != null && _plc.IsConnected)
//    {
//        _qtksRecipes = new List<QtksRecipeData>();
//        for (int i = 1; i < 16; i++)
//        {
//            var h = _plc.ReadCustomer<QtksRecipeData>($"D{434 + 34 * (i - 1)}").Content;
//            h.ID = i;
//            _qtksRecipes.Add(h);
//        }
//        // 显示数据
//        dgv_AEGD.DataSource = null;
//        dgv_AEGD.DataSource = _qtksRecipes;
//        label3.Text = _plc.ReadUInt16("D946").Content.ToString();
//    }
//}
//catch (Exception ex)
//{

//    _logger.Debug(ex.Message);
//    ModernMessageBox.Show("读取失败!" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
//}
