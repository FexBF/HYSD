using System.Reflection;
using System.Windows.Forms;

namespace HYSD
{
    /// <summary>
    /// UI 辅助工具：解决 WinForms 控件闪烁问题。
    /// </summary>
    public static class UIHelper
    {
        /// <summary>
        /// 递归地为控件及其所有子控件开启双缓冲，消除闪烁。
        ///
        /// 原理：WinForms 的 Control.DoubleBuffered 是 protected 属性，
        /// 外部无法直接设置。Label、PictureBox 等控件默认 DoubleBuffered=false，
        /// 每次 Text/属性变化都直接画到屏幕，造成闪烁。
        /// 用反射将其设为 true，让控件先画到内存位图再一次性输出到屏幕。
        /// </summary>
        public static void EnableDoubleBuffering(Control control)
        {
            if (control == null) return;

            // 用反射设置 DoubleBuffered = true
            var prop = control.GetType().GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(control, true, null);
            }

            // 递归处理所有子控件
            foreach (Control child in control.Controls)
            {
                EnableDoubleBuffering(child);
            }
        }

        /// <summary>
        /// ★ 优化丝滑：仅当新值与当前值不同时才赋值，避免无效重绘。
        ///
        /// 原理：Label.Text 的 setter 即使新旧值相同也会触发 Invalidate → 重绘。
        /// JK 每秒刷新 100+ 个 Label，大部分值其实没变（如"弧源1电压"稳态时不变），
        /// 但每次都赋值会导致 100+ 次无效重绘，叠加造成稳态刷新卡顿。
        /// 此扩展方法先比较再赋值，值未变则跳过，减少 90%+ 的无效重绘。
        ///
        /// 用法：label1.SetTextIfChanged("123A");
        /// </summary>
        public static void SetTextIfChanged(this Label label, string text)
        {
            if (label == null) return;
            if (!ReferenceEquals(label.Text, text) && label.Text != text)
            {
                label.Text = text;
            }
        }
    }
}
