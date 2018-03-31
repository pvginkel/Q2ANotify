using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GdiPresentation;

namespace Q2ANotify.Support
{
    public class NonStretchingBorder : Border
    {
        protected override Size MeasureOverride(Size desiredSize)
        {
            base.MeasureOverride(desiredSize);

            return Size.Empty;
        }
    }
}
