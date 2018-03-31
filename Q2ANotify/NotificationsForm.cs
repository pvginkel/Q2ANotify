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

        private readonly Api _api;
        private readonly Db _db;
        private FeedUser _user;
        private readonly List<FeedNotification> _notifications = new List<FeedNotification>();
        private readonly float _dpiX;
        private readonly float _dpiY;

        public NotificationsForm(Api api, Db db)
        {
            if (api == null)
                throw new ArgumentNullException(nameof(api));
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            _api = api;
            _db = db;

            InitializeComponent();

            using (var g = CreateGraphics())
            {
                _dpiX = g.DpiX;
                _dpiY = g.DpiY;
            }

            Deactivate += (s, e) => Visible = false;

            Font = SystemFonts.MessageBoxFont;
        }

        public void LoadFeed(Feed feed)
        {
            if (feed == null)
                throw new ArgumentNullException(nameof(feed));

            _user = feed.User;
            _notifications.InsertRange(0, feed.Notifications);

            _elementControl.Content = BuildContent();
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

        private void OpenPost(int postId)
        {
            try
            {
                Process.Start(_api.GetPostLink(postId));
            }
            catch
            {
                // Ignore exceptions. This call may fail, e.g. when Firefox needs to update.
            }
        }

        private void CloseNotification(long id)
        {
            using (var ctx = _db.OpenContext())
            {
                ctx.ExecuteNonQuery("DELETE FROM notification WHERE rowid = @id", ("@id", id));
                ctx.Commit();
            }

            _notifications.RemoveAll(p => p.Id == id);

            _elementControl.Content = BuildContent();
        }

        private void DismissAll()
        {
            using (var ctx = _db.OpenContext())
            {
                ctx.ExecuteNonQuery("DELETE FROM notification");
                ctx.Commit();
            }

            _notifications.Clear();

            _elementControl.Content = BuildContent();
        }
    }
}
