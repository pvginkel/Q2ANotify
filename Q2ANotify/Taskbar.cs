using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Q2ANotify
{
    // Taken from https://stackoverflow.com/questions/2712247/net-how-to-place-my-window-near-the-notification-area-systray.

    public class Taskbar
    {
        private const string ClassName = "Shell_TrayWnd";

        public static Taskbar Find()
        {
            return new Taskbar();
        }

        public Rectangle Bounds { get; }
        public TaskbarPosition Position { get; }
        public Point Location => Bounds.Location;
        public Size Size => Bounds.Size;
        // Always returns false under Windows 7.
        public bool AlwaysOnTop { get; }
        public bool AutoHide { get; }

        private Taskbar()
        {
            var taskbarHandle = NativeMethods.User32.FindWindow(ClassName, null);

            var data = new NativeMethods.APPBARDATA
            {
                cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.APPBARDATA)),
                hWnd = taskbarHandle
            };

            var result = NativeMethods.Shell32.SHAppBarMessage(NativeMethods.ABM.GetTaskbarPos, ref data);
            if (result == IntPtr.Zero)
                throw new InvalidOperationException();

            Position = (TaskbarPosition)data.uEdge;
            Bounds = Rectangle.FromLTRB(data.rc.left, data.rc.top, data.rc.right, data.rc.bottom);

            data.cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.APPBARDATA));
            result = NativeMethods.Shell32.SHAppBarMessage(NativeMethods.ABM.GetState, ref data);
            int state = result.ToInt32();
            AlwaysOnTop = (state & NativeMethods.ABS.AlwaysOnTop) == NativeMethods.ABS.AlwaysOnTop;
            AutoHide = (state & NativeMethods.ABS.Autohide) == NativeMethods.ABS.Autohide;
        }

        public enum TaskbarPosition
        {
            Unknown = -1,
            Left,
            Top,
            Right,
            Bottom,
        }
    }
}
