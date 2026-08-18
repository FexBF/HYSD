using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace OxygenCylinderControl
{
    /// <summary>
    /// 氧气瓶自定义控件 —— 纯外观绘制
    /// </summary>
    public class OxygenCylinderControl : Control
    {
        public OxygenCylinderControl()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);
            Size = new Size(200, 500);
            BackColor = Color.Transparent;
        }

        private string _displayText = "O₂";

        /// <summary>
        /// 瓶体中间显示的文字，修改后自动刷新
        /// </summary>
        public string DisplayText
        {
            get => _displayText;
            set
            {
                if (_displayText != value)
                {
                    _displayText = value;
                    Invalidate();
                }
            }
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            DrawCylinder(g);
        }

        /* ========== 主绘制入口 ========== */
        private void DrawCylinder(Graphics g)
        {
            float w = Width;
            float h = Height;
            float cx = w / 2f;

            float bodyW = w * 0.48f;
            float bodyTop = h * 0.24f;
            float bodyBot = h * 0.87f;
            float neckW = w * 0.14f;
            float neckTop = h * 0.085f;
            float neckBot = h * 0.195f;
            float guardW = w * 0.58f;
            float guardH = h * 0.028f;
            float guardY = neckBot + h * 0.006f;
            float valveW = neckW * 1.85f;
            float valveH = h * 0.038f;

            DrawShadow(g, cx, bodyBot + 4, bodyW * 1.15f);
            DrawBody(g, cx, bodyTop, bodyBot, bodyW, neckBot, neckW);
            DrawBase(g, cx, bodyBot, bodyW);
            DrawNeck(g, cx, neckTop, neckBot, neckW);
            DrawGuard(g, cx, guardY, guardH, guardW);
            DrawValve(g, cx, neckTop, neckW, valveW, valveH, w);
            //DrawLabel(g, cx, bodyTop, bodyBot, bodyW);
            DrawO2Mark(g, cx, bodyTop, bodyBot, bodyW);
            DrawHighlight(g, cx, bodyTop, bodyBot, bodyW);
        }

        /* ========== 1. 地面阴影 ========== */
        private void DrawShadow(Graphics g, float cx, float y, float w)
        {
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(cx - w / 2, y, w, 18);
                using (var grad = new PathGradientBrush(path))
                {
                    grad.CenterPoint = new PointF(cx, y + 9);
                    grad.CenterColor = Color.FromArgb(50, 50, 55);
                    grad.SurroundColors = new[] { Color.Transparent };
                    grad.FocusScales = new PointF(0.2f, 0.15f);
                    g.FillPath(grad, path);
                }
            }
        }

        /* ========== 2. 瓶体 ========== */
        private void DrawBody(Graphics g, float cx, float bodyTop, float bodyBot,
                              float bodyW, float neckBot, float neckW)
        {
            using (var path = BuildBodyPath(cx, bodyTop, bodyBot, bodyW, neckBot, neckW))
            {
                using (var grad = new LinearGradientBrush(
                    new PointF(cx - bodyW / 2, 0),
                    new PointF(cx + bodyW / 2, 0),
                    Color.FromArgb(0, 60, 130),
                    Color.FromArgb(0, 60, 130)))
                {
                    grad.InterpolationColors = new ColorBlend
                    {
                        Positions = new[] { 0f, 0.10f, 0.28f, 0.48f, 0.75f, 1f },
                        Colors = new[]
                        {
                            Color.FromArgb(0, 45, 105),
                            Color.FromArgb(10, 85, 165),
                            Color.FromArgb(65, 155, 235),
                            Color.FromArgb(20, 105, 190),
                            Color.FromArgb(3, 60, 130),
                            Color.FromArgb(0, 35, 88),
                        }
                    };
                    g.FillPath(grad, path);
                }

                using (var pen = new Pen(Color.FromArgb(0, 30, 75), 1.6f))
                    g.DrawPath(pen, path);
            }
        }

        private GraphicsPath BuildBodyPath(float cx, float bodyTop, float bodyBot,
                                           float bodyW, float neckBot, float neckW)
        {
            float bL = cx - bodyW / 2, bR = cx + bodyW / 2;
            float nL = cx - neckW / 2, nR = cx + neckW / 2;
            float sH = bodyTop - neckBot;

            var p = new GraphicsPath();
            p.StartFigure();
            p.AddLine(bL, bodyTop, bL, bodyBot);
            p.AddBezier(bL, bodyBot, bL, bodyBot + 3, bR, bodyBot + 3, bR, bodyBot);
            p.AddLine(bR, bodyBot, bR, bodyTop);
            p.AddBezier(bR, bodyTop, bR, bodyTop - sH * 0.35f,
                        nR, neckBot + sH * 0.65f, nR, neckBot);
            p.AddLine(nR, neckBot, nL, neckBot);
            p.AddBezier(nL, neckBot, nL, neckBot + sH * 0.65f,
                        bL, bodyTop - sH * 0.35f, bL, bodyTop);
            p.CloseFigure();
            return p;
        }

        /* ========== 3. 底座环 ========== */
        private void DrawBase(Graphics g, float cx, float bodyBot, float bodyW)
        {
            float bw = bodyW * 1.06f, bh = 7f, bl = cx - bw / 2;
            using (var grad = new LinearGradientBrush(
                new PointF(bl, 0), new PointF(bl + bw, 0),
                Color.FromArgb(100, 104, 112),
                Color.FromArgb(55, 58, 64)))
            {
                g.FillRectangle(grad, bl, bodyBot - 1, bw, bh);
            }
            using (var pen = new Pen(Color.FromArgb(35, 38, 44), 1f))
                g.DrawRectangle(pen, bl, bodyBot - 1, bw, bh);
        }

        /* ========== 4. 瓶颈 ========== */
        private void DrawNeck(Graphics g, float cx, float neckTop, float neckBot, float neckW)
        {
            float nL = cx - neckW / 2;
            using (var grad = new LinearGradientBrush(
                new PointF(nL, 0), new PointF(nL + neckW, 0),
                Color.FromArgb(150, 155, 162),
                Color.FromArgb(150, 155, 162)))
            {
                grad.InterpolationColors = new ColorBlend
                {
                    Positions = new[] { 0f, 0.18f, 0.38f, 0.6f, 0.85f, 1f },
                    Colors = new[]
                    {
                        Color.FromArgb(115, 120, 128),
                        Color.FromArgb(175, 180, 188),
                        Color.FromArgb(210, 214, 222),
                        Color.FromArgb(188, 192, 200),
                        Color.FromArgb(148, 152, 160),
                        Color.FromArgb(108, 112, 118),
                    }
                };
                g.FillRectangle(grad, nL, neckTop, neckW, neckBot - neckTop);
            }
            using (var pen = new Pen(Color.FromArgb(75, 78, 85), 1.2f))
                g.DrawRectangle(pen, nL, neckTop, neckW, neckBot - neckTop);

            float dy = (neckBot - neckTop) * 0.32f;
            using (var pen = new Pen(Color.FromArgb(85, 88, 95), 0.8f))
            {
                g.DrawLine(pen, nL, neckTop + dy, nL + neckW, neckTop + dy);
                g.DrawLine(pen, nL, neckTop + dy * 2, nL + neckW, neckTop + dy * 2);
            }
        }

        /* ========== 5. 护圈 ========== */
        private void DrawGuard(Graphics g, float cx, float guardY, float guardH, float guardW)
        {
            float gL = cx - guardW / 2;
            float r = Math.Min(guardH, guardW / 2);
            using (var path = CreateRoundRect(gL, guardY, guardW, guardH, r))
            using (var grad = new LinearGradientBrush(
                new PointF(gL, 0), new PointF(gL + guardW, 0),
                Color.FromArgb(130, 134, 142),
                Color.FromArgb(130, 134, 142)))
            {
                grad.InterpolationColors = new ColorBlend
                {
                    Positions = new[] { 0f, 0.12f, 0.38f, 0.62f, 0.88f, 1f },
                    Colors = new[]
                    {
                        Color.FromArgb(85, 88, 95),
                        Color.FromArgb(140, 144, 152),
                        Color.FromArgb(178, 182, 190),
                        Color.FromArgb(155, 158, 166),
                        Color.FromArgb(118, 122, 128),
                        Color.FromArgb(80, 83, 90),
                    }
                };
                g.FillPath(grad, path);
                using (var pen = new Pen(Color.FromArgb(65, 68, 75), 1.2f))
                    g.DrawPath(pen, path);
            }
        }

        /* ========== 6. 瓶阀总成 ========== */
        private void DrawValve(Graphics g, float cx, float neckTop, float neckW,
                               float valveW, float valveH, float totalW)
        {
            float vL = cx - valveW / 2;
            float vTop = neckTop - valveH * 0.25f;

            // 阀门主体
            using (var path = CreateRoundRect(vL, vTop, valveW, valveH, 3f))
            using (var grad = new LinearGradientBrush(
                new PointF(vL, 0), new PointF(vL + valveW, 0),
                Color.FromArgb(148, 152, 160),
                Color.FromArgb(148, 152, 160)))
            {
                grad.InterpolationColors = new ColorBlend
                {
                    Positions = new[] { 0f, 0.18f, 0.42f, 0.68f, 1f },
                    Colors = new[]
                    {
                        Color.FromArgb(105, 108, 116),
                        Color.FromArgb(168, 172, 180),
                        Color.FromArgb(200, 204, 212),
                        Color.FromArgb(158, 162, 168),
                        Color.FromArgb(95, 98, 106),
                    }
                };
                g.FillPath(grad, path);
                using (var pen = new Pen(Color.FromArgb(65, 68, 75), 1f))
                    g.DrawPath(pen, path);
            }

            // 手轮横杆
            float barW = neckW * 3.8f;
            float barH = valveH * 0.32f;
            float barL = cx - barW / 2;
            float barTop = vTop - barH - 2f;

            using (var grad = new LinearGradientBrush(
                new PointF(barL, 0), new PointF(barL + barW, 0),
                Color.FromArgb(138, 142, 150),
                Color.FromArgb(138, 142, 150)))
            {
                grad.InterpolationColors = new ColorBlend
                {
                    Positions = new[] { 0f, 0.25f, 0.5f, 0.78f, 1f },
                    Colors = new[]
                    {
                        Color.FromArgb(95, 98, 106),
                        Color.FromArgb(158, 162, 170),
                        Color.FromArgb(188, 192, 200),
                        Color.FromArgb(148, 152, 158),
                        Color.FromArgb(90, 93, 100),
                    }
                };
                g.FillRectangle(grad, barL, barTop, barW, barH);
            }
            using (var pen = new Pen(Color.FromArgb(60, 63, 70), 1f))
                g.DrawRectangle(pen, barL, barTop, barW, barH);

            // 手轮两端旋钮
            float knobRx = barH * 0.75f;
            float knobRy = barH * 0.65f;
            DrawKnob(g, barL + knobRx * 0.4f, barTop + barH / 2, knobRx, knobRy);
            DrawKnob(g, barL + barW - knobRx * 0.4f, barTop + barH / 2, knobRx, knobRy);

            // 中心立柱
            float stemW = neckW * 0.45f;
            float stemL = cx - stemW / 2;
            using (var grad = new LinearGradientBrush(
                new PointF(stemL, 0), new PointF(stemL + stemW, 0),
                Color.FromArgb(155, 158, 166),
                Color.FromArgb(118, 122, 128)))
            {
                g.FillRectangle(grad, stemL, barTop + barH, stemW, vTop - barTop - barH);
            }

            // 出气接口
            float pipeW = neckW * 0.38f;
            float pipeH = valveH * 0.7f;
            float pipeL = vL + valveW + 1;
            float pipeT = vTop + valveH * 0.15f;
            using (var grad = new LinearGradientBrush(
                new PointF(pipeL, 0), new PointF(pipeL + pipeW, 0),
                Color.FromArgb(128, 132, 140),
                Color.FromArgb(98, 102, 108)))
            {
                g.FillRectangle(grad, pipeL, pipeT, pipeW, pipeH);
            }
            using (var pen = new Pen(Color.FromArgb(65, 68, 75), 1f))
                g.DrawRectangle(pen, pipeL, pipeT, pipeW, pipeH);

            // 法兰
            float flangeW = pipeW * 1.35f;
            float flangeH = pipeH * 0.35f;
            float flangeL = pipeL + pipeW - 1;
            using (var grad = new LinearGradientBrush(
                new PointF(flangeL, 0), new PointF(flangeL + flangeW, 0),
                Color.FromArgb(135, 138, 146),
                Color.FromArgb(105, 108, 115)))
            {
                g.FillRectangle(grad, flangeL, pipeT + (pipeH - flangeH) / 2, flangeW, flangeH);
            }
        }

        private void DrawKnob(Graphics g, float cx, float cy, float rx, float ry)
        {
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(cx - rx, cy - ry, rx * 2, ry * 2);
                using (var grad = new PathGradientBrush(path))
                {
                    grad.CenterPoint = new PointF(cx - rx * 0.2f, cy - ry * 0.25f);
                    grad.CenterColor = Color.FromArgb(195, 198, 205);
                    grad.SurroundColors = new[] { Color.FromArgb(88, 92, 100) };
                    grad.FocusScales = new PointF(0.5f, 0.5f);
                    g.FillPath(grad, path);
                }
                using (var pen = new Pen(Color.FromArgb(65, 68, 75), 0.8f))
                    g.DrawPath(pen, path);
            }
        }

        ///* ========== 7. 铭牌标签 ========== */
        //private void DrawLabel(Graphics g, float cx, float bodyTop, float bodyBot, float bodyW)
        //{
        //    float lw = bodyW * 0.72f;
        //    float lh = (bodyBot - bodyTop) * 0.32f;
        //    float lL = cx - lw / 2;
        //    float lT = bodyTop + (bodyBot - bodyTop) * 0.18f;

        //    using (var path = CreateRoundRect(lL, lT, lw, lh, 3f))
        //    {
        //        using (var grad = new LinearGradientBrush(
        //            new PointF(lL, lT), new PointF(lL + lw, lT + lh),
        //            Color.FromArgb(248, 250, 255),
        //            Color.FromArgb(228, 233, 242)))
        //        {
        //            g.FillPath(grad, path);
        //        }
        //        using (var pen = new Pen(Color.FromArgb(175, 180, 192), 0.8f))
        //            g.DrawPath(pen, path);
        //    }

        //    using (var pen = new Pen(Color.FromArgb(198, 203, 214), 0.5f))
        //    {
        //        g.DrawLine(pen, lL + 5, lT + lh * 0.40f, lL + lw - 5, lT + lh * 0.40f);
        //        g.DrawLine(pen, lL + 5, lT + lh * 0.70f, lL + lw - 5, lT + lh * 0.70f);
        //    }

        //    float fs = Math.Max(7f, lw * 0.145f);
        //    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        //    using (var font = new Font("Microsoft YaHei", fs, FontStyle.Bold))
        //    using (var brush = new SolidBrush(Color.FromArgb(35, 45, 65)))
        //    {
        //        g.DrawString("医用氧气", font, brush, new RectangleF(lL, lT, lw, lh * 0.38f), sf);
        //    }

        //    float sfs = Math.Max(5.5f, fs * 0.72f);
        //    using (var sFont = new Font("Microsoft YaHei", sfs))
        //    {
        //        g.DrawString("O₂  Medical Oxygen", sFont,
        //            new SolidBrush(Color.FromArgb(75, 85, 108)),
        //            new RectangleF(lL, lT + lh * 0.38f, lw, lh * 0.30f), sf);
        //        g.DrawString("40L  15MPa", sFont,
        //            new SolidBrush(Color.FromArgb(100, 108, 128)),
        //            new RectangleF(lL, lT + lh * 0.68f, lw, lh * 0.30f), sf);
        //    }
        //}

        /* ========== 8. O₂ 大标识 ========== */
        private void DrawO2Mark(Graphics g, float cx, float bodyTop, float bodyBot, float bodyW)
        {
            if (string.IsNullOrEmpty(_displayText)) return;

            float textY = bodyTop + (bodyBot - bodyTop) * 0.5f;
            float fs = Math.Max(12f, bodyW * 0.85f);

            using (var font = new Font("Microsoft YaHei", fs, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.FromArgb(160, 215, 255, 100)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(_displayText, font, brush, cx, textY, sf);
            }
        }


        /* ========== 9. 高光反射 ========== */
        private void DrawHighlight(Graphics g, float cx, float bodyTop, float bodyBot, float bodyW)
        {
            float h1W = bodyW * 0.055f;
            float h1L = cx - bodyW * 0.19f;
            using (var grad = new LinearGradientBrush(
                new PointF(h1L, 0), new PointF(h1L + h1W, 0),
                Color.Transparent, Color.Transparent))
            {
                grad.InterpolationColors = new ColorBlend
                {
                    Positions = new[] { 0f, 0.25f, 0.5f, 0.8f, 1f },
                    Colors = new[]
                    {
                        Color.Transparent,
                        Color.FromArgb(35, 135, 215, 55),
                        Color.FromArgb(65, 175, 250, 85),
                        Color.FromArgb(25, 125, 205, 45),
                        Color.Transparent,
                    }
                };
                g.FillRectangle(grad, h1L, bodyTop + 8, h1W, bodyBot - bodyTop - 20);
            }

            float h2W = bodyW * 0.018f;
            float h2L = cx - bodyW * 0.165f;
            using (var grad = new LinearGradientBrush(
                new PointF(h2L, 0), new PointF(h2L + h2W, 0),
                Color.Transparent, Color.Transparent))
            {
                grad.InterpolationColors = new ColorBlend
                {
                    Positions = new[] { 0f, 0.25f, 0.5f, 0.8f, 1f },
                    Colors = new[]
                    {
                        Color.Transparent,
                        Color.FromArgb(75, 195, 255, 95),
                        Color.FromArgb(130, 228, 255, 135),
                        Color.FromArgb(55, 185, 250, 75),
                        Color.Transparent,
                    }
                };
                g.FillRectangle(grad, h2L, bodyTop + 14, h2W, bodyBot - bodyTop - 30);
            }
        }

        /* ========== 工具方法 ========== */
        private static GraphicsPath CreateRoundRect(float x, float y, float w, float h, float r)
        {
            r = Math.Min(r, Math.Min(w / 2, h / 2));
            var p = new GraphicsPath();
            p.AddArc(x, y, r * 2, r * 2, 180, 90);
            p.AddLine(x + r, y, x + w - r, y);
            p.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
            p.AddLine(x + w, y + r, x + w, y + h - r);
            p.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
            p.AddLine(x + w - r, y + h, x + r, y + h);
            p.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
            p.AddLine(x, y + h - r, x, y + r);
            p.CloseFigure();
            return p;
        }

    }
}