using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Q2ANotify
{
    public class ScaledImage
    {
        private readonly Bitmap _bitmap;
        private readonly Size _size;
        private readonly Dictionary<(float DpiX, float DpiY), Bitmap> _cache = new Dictionary<(float DpiX, float DpiY), Bitmap>();

        public ScaledImage(Bitmap bitmap, Size size)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            _bitmap = bitmap;
            _size = size;
        }

        public Bitmap GetScaled(float dpiX, float dpiY)
        {
            if (!_cache.TryGetValue((dpiX, dpiY), out var bitmap))
            {
                var targetSize = new Size(
                    (int)((dpiX / 96) * _size.Width),
                    (int)((dpiY / 96) * _size.Height)
                );

                bitmap = new Bitmap(targetSize.Width, targetSize.Height);

                using (var g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    g.DrawImage(
                        _bitmap,
                        0,
                        0,
                        targetSize.Width,
                        targetSize.Height
                    );
                }

                _cache.Add((dpiX, dpiY), bitmap);
            }

            return bitmap;
        }
    }
}
