using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GdiPresentation;

namespace Q2ANotify.Support
{
    public class Circle : Border
    {
        public SolidBrush Brush { get; set; }

        protected override void OnPaint(ElementPaintEventArgs e)
        {
            base.OnPaint(e);

            var smoothingMode = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var brush = new System.Drawing.SolidBrush((System.Drawing.Color)Brush.Color))
            {
                e.Graphics.FillEllipse(
                    brush,
                    (System.Drawing.Rectangle)e.Bounds
                );
            }

            e.Graphics.SmoothingMode = smoothingMode;
        }
    }
}
