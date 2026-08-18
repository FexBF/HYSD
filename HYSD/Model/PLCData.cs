using Autofac;
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
    public partial class PLCData : UserControl
    {
        private readonly ILifetimeScope _scope; // Autofac 的生命周期作用域
        public PLCData(ILifetimeScope scope)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            _scope = scope;
            rjButton_HeatRecipe_Click(null, null); // 默认显示热处理配方数据
        }

        private void rjButton_HeatRecipe_Click(object sender, EventArgs e)
        {
            rjButton_HeatRecipe.BackColor = Color.Cyan;
            rjButton_QtksRecipe.BackColor = Color.Silver;
            rjButton_TCRecipe.BackColor = Color.Silver;
            foreach (var item in panel1.Controls)
            {
                if (item is PLCHeatRecipeData)
                {
                    return;
                }
            }
            this.panel1.Controls.Clear();
            PLCHeatRecipeData Recipe = _scope.Resolve<PLCHeatRecipeData>();
            this.panel1.Controls.Add(Recipe);
        }

        private void rjButton_QtksRecipe_Click(object sender, EventArgs e)
        {
            rjButton_QtksRecipe.BackColor = Color.Cyan;
            rjButton_HeatRecipe.BackColor = Color.Silver;
            rjButton_TCRecipe.BackColor = Color.Silver;
            foreach (var item in panel1.Controls)
            {
                if (item is PLCQtksRecipeData)
                {
                    return;
                }
            }
            this.panel1.Controls.Clear();
            PLCQtksRecipeData Recipe = _scope.Resolve<PLCQtksRecipeData>();
            this.panel1.Controls.Add(Recipe);
        }

        private void rjButton_TCRecipe_Click(object sender, EventArgs e)
        {
            rjButton_TCRecipe .BackColor= Color.Cyan;
            rjButton_HeatRecipe.BackColor = Color.Silver;
            rjButton_QtksRecipe.BackColor = Color.Silver;
            foreach (var item in panel1.Controls)
            {
                if (item is PLCTCRecipeData)
                {
                    return;
                }
            }
            this.panel1.Controls.Clear();
            PLCTCRecipeData Recipe = _scope.Resolve<PLCTCRecipeData>();
            this.panel1.Controls.Add(Recipe);
        }
    }
}
