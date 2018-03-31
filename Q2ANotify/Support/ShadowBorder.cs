using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GdiPresentation;

namespace Q2ANotify.Support
{
    public class ShadowBorder : ContentElement
    {
        public Color BorderStartColor { get; set; }
        public Color BorderEndColor { get; set; }
        public int BorderWidth { get; set; }
        public Thickness Padding { get; set; }

        protected override Size MeasureOverride(Size desiredSize)
        {
            var size = new Size(Padding.Horizontal + BorderWidth * 2, Padding.Vertical + BorderWidth * 2);
            desiredSize = base.MeasureOverride(new Size(desiredSize.Width - size.Width, desiredSize.Height - size.Height));
            return new Size(desiredSize.Width + size.Width, desiredSize.Height + size.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Content == null)
                return base.ArrangeOverride(finalSize);
            Content.Arrange(new Rect(Padding.Left + BorderWidth, Padding.Top + BorderWidth, finalSize.Width - (Padding.Horizontal + BorderWidth * 2), finalSize.Height - (Padding.Vertical + BorderWidth * 2)));
            return Content.Size;
        }

        protected override void OnPaintBackground(ElementPaintEventArgs e)
        {
            var bounds = new System.Drawing.Rectangle(
                e.Bounds.Left,
                e.Bounds.Top,
                e.Bounds.Width - 1,
                e.Bounds.Height - 1
            );

            for (int i = 0; i < BorderWidth; i++)
            {
                double offset = BorderWidth > 1 ? (double)i / (BorderWidth - 1) : 0;
                var color = InterpolateBorderColor(offset);

                using (var pen = new System.Drawing.Pen(color, 1))
                {
                    e.Graphics.DrawRectangle(
                        pen,
                        bounds
                    );
                }

                bounds.Inflate(-1, -1);
            }
        }

        private System.Drawing.Color InterpolateBorderColor(double offset)
        {
            return System.Drawing.Color.FromArgb(
                InterpolateColor(BorderStartColor.A, BorderEndColor.A, offset),
                InterpolateColor(BorderStartColor.R, BorderEndColor.R, offset),
                InterpolateColor(BorderStartColor.G, BorderEndColor.G, offset),
                InterpolateColor(BorderStartColor.B, BorderEndColor.B, offset)
            );
        }

        private int InterpolateColor(int start, int end, double offset)
        {
            int result = start + (int)((end - start) * offset);
            return Math.Min(Math.Max(result, 0), 255);
        }
    }
}
