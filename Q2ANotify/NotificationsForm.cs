using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Q2ANotify.Database;
using Q2ANotify.Q2AApi;

namespace Q2ANotify
{
    public partial class NotificationsForm : Form
    {
        private const int FormMargin = 10;

        private readonly Synchronizer _synchronizer;
        private FeedUser _user;
        private readonly List<FeedNotification> _notifications = new List<FeedNotification>();
        private readonly int _dpi;

        public NotificationsForm(Synchronizer synchronizer)
        {
            if (synchronizer == null)
                throw new ArgumentNullException(nameof(synchronizer));

            _synchronizer = synchronizer;

            InitializeComponent();

            using (var g = CreateGraphics())
            {
                _dpi = (int)g.DpiX;
            }

            Font = SystemFonts.MessageBoxFont;

            _elementControl.MaximumSize = new Size(Scale(350), int.MaxValue);
            _elementControl.MinimumSize = new Size(Scale(350), 0);

            Deactivate += (s, e) => Visible = false;

            var feed = synchronizer.LoadFeedFromDatabase();
            if (feed != null)
                LoadFeed(feed);

            synchronizer.FeedUpdated += (s, e) => LoadFeed(e.Feed);
        }

        protected override void SetVisibleCore(bool value)
        {
            if (value && _user != null)
                _elementControl.Content = BuildContent();

            base.SetVisibleCore(value);
        }

        private void LoadFeed(Feed feed)
        {
            _user = feed.User;
            _notifications.InsertRange(0, feed.Notifications);

            RebuildContent();
        }

        private void RebuildContent()
        {
            if (Visible)
                _elementControl.Content = BuildContent();
            else
                _elementControl.Content = null;
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            var taskbar = Taskbar.Find();

            switch (taskbar.Position)
            {
                case Taskbar.TaskbarPosition.Left:
                    x = taskbar.Bounds.Right + FormMargin;
                    y = taskbar.Bounds.Bottom - FormMargin - height;
                    break;
                case Taskbar.TaskbarPosition.Top:
                    x = taskbar.Bounds.Right - FormMargin - width;
                    y = taskbar.Bounds.Bottom + FormMargin;
                    break;
                case Taskbar.TaskbarPosition.Right:
                    x = taskbar.Bounds.Left - FormMargin - width;
                    y = taskbar.Bounds.Bottom - FormMargin - height;
                    break;
                case Taskbar.TaskbarPosition.Bottom:
                default:
                    x = taskbar.Bounds.Right - FormMargin - width;
                    y = taskbar.Bounds.Top - FormMargin - height;
                    break;
            }

            specified = BoundsSpecified.All;

            base.SetBoundsCore(x, y, width, height, specified);
        }

        private void NotificationsForm_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                Activate();
                // NativeMethods.SetForegroundWindow(NativeMethods.GetWindow(Handle, NativeMethods.GW_ENABLEDPOPUP));
            }
        }

        private void CloseNotification(long id)
        {
            _synchronizer.DeleteNotification(id);

            _notifications.RemoveAll(p => p.Id == id);

            RebuildContent();
        }

        private void DismissAll()
        {
            _synchronizer.DeleteAllNotifications();

            _notifications.Clear();

            RebuildContent();
        }

        private void NotificationsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Visible = false;
            }
        }
    }
}
