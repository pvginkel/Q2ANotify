using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GdiPresentation;

namespace Q2ANotify.Support
{
    public class Button : ContentElement
    {
        public event EventHandler Click;

        public Button()
        {
            Background = SolidBrush.Transparent;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Capture = true;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (Capture)
            {
                Capture = false;

                OnClick();
            }
        }

        protected virtual void OnClick()
        {
            Click?.Invoke(this, EventArgs.Empty);
        }
    }
}
