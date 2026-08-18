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
    public partial class PLCTCRecipeData : UserControl
    {
        private readonly IOmronPlcService _plc;
        private readonly ILogger _logger;
        public PLCTCRecipeData(IOmronPlcService plc, ILogger logger)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            _plc = plc;
            _logger = logger;
            dgv_Coat.AutoGenerateColumns = false;
            dgv_Coat.OptimizeForPerformance();   // ★ 加这行
        }

        private List<TCRecipeData> _tcRecipes;
        bool isUp = false;
        private async void rjButton1_Click(object sender, EventArgs e)
        {
            if (_plc == null || !_plc.IsConnected) return;

            if (isUp) return; // 如果正在上载，直接返回
            isUp = true;
            // ★ 修复：原代码在 UI 线程同步读 30 次 PLC + 1 次 ReadUInt16，
            // 每次最多 3 秒超时，合计可能冻结 UI 近百秒。
            // 改为后台线程读取，完成后回 UI 线程绑定数据。
            List<TCRecipeData> recipes = null;
            ushort recipeNo = 0;
            bool ok = false;
            string errMsg = null;
            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                {
                    recipes = new List<TCRecipeData>();
                    for (int i = 1; i < 31; i++)
                    {
                        var r = _plc.ReadCustomer<TCRecipeData>($"D{2048 + 48 * (i - 1)}");
                        if (!r.IsSuccess) { errMsg = r.Message; return; }
                        var h = r.Content;
                        h.ID = i;
                        recipes.Add(h);
                    }
                    var rn = _plc.ReadUInt16("D348");
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
                    _tcRecipes = recipes;
                    dgv_Coat.SetDataSource(_tcRecipes);
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
