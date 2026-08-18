using System.Reflection;
using System.Windows.Forms;

namespace HYSD
{
    public static class DataGridViewExtensions
    {
        /// <summary>
        /// 开启双缓冲 + 性能优化配置，所有 DataGridView 初始化时调用一次即可。
        /// </summary>
        public static void OptimizeForPerformance(this DataGridView dgv)
        {
            // 1. 双缓冲（反射开启 protected 的 DoubleBuffered）
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty,
                null, dgv, new object[] { true });

            // 2. 禁用自动列宽计算（最卡的一项），改为手动或 Fill
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            // 3. 禁用行高自动计算
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            // 4. 固定行高，避免每行都测量字体
            dgv.RowTemplate.Height = 40;

            // 5. 禁用自动生成列（如果还没设）
            dgv.AutoGenerateColumns = false;

            // 6. 关闭列头排序（避免点击列头时全表排序卡顿）
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        /// <summary>
        /// 高效更新数据源：只绑定一次，后续用 ResetBindings 通知刷新。
        /// 替代原来的 DataSource = null; DataSource = list;
        /// </summary>
        public static void SetDataSource<T>(this DataGridView dgv, System.Collections.Generic.List<T> list)
        {
            if (dgv.DataSource is BindingSource bs && bs.List is System.ComponentModel.BindingList<T>)
            {
                // 已有 BindingSource，直接替换数据
                var bindingList = (System.ComponentModel.BindingList<T>)bs.List;
                bindingList.RaiseListChangedEvents = false;
                bindingList.Clear();
                foreach (var item in list)
                    bindingList.Add(item);
                bindingList.RaiseListChangedEvents = true;
                bs.ResetBindings(false);
            }
            else
            {
                // 首次绑定，用 BindingSource 包一层
                var bindingList = new System.ComponentModel.BindingList<T>(list);
                var source = new BindingSource { DataSource = bindingList };
                dgv.DataSource = source;
            }
        }
    }
}
