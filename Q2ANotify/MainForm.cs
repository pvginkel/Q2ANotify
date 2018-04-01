﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Q2ANotify.Database;
using Q2ANotify.Q2AApi;

namespace Q2ANotify
{
    public partial class MainForm : Form
    {
        private readonly Db _db;
        private Synchronizer _synchronizer;
        private NotificationsForm _notifications;
        private FeedNotification _lastNotificationNotified;

        public MainForm(Db db)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            _db = db;

            InitializeComponent();

            _notifyIcon.ContextMenu = _notifyIconMenu;

            Disposed += MainForm_Disposed;

            Load += MainForm_Load;

#if DEBUG
            Shown += (s, e) => _notifications?.Show();
#endif
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Api api = null;

            var credentials = LoadCredentials();
            if (credentials?.Password != null)
            {
                api = new Api(credentials);

                try
                {
                    api.Authenticate();
                }
                catch
                {
                    api = null;
                }
            }

            if (api != null)
                OpenSynchronizer(api);
            else
                Login();
        }

        private void Login()
        {
            var credentials = LoadCredentials();
            Api api;

            using (var form = new LoginForm(credentials))
            {
                if (form.ShowDialog() != DialogResult.OK)
                    return;

                credentials = form.Credentials;
                api = form.Api;
            }

            SaveCredentials(credentials);

            OpenSynchronizer(api);
        }

        private void MainForm_Disposed(object sender, EventArgs e)
        {
            CloseSynchronizer();
        }

        private void OpenSynchronizer(Api api)
        {
            _synchronizer = new Synchronizer(api, _db);

            _synchronizer.FeedUpdated += _synchronizer_FeedUpdated;

            _notifications = new NotificationsForm(_synchronizer);
        }

        private void _synchronizer_FeedUpdated(object sender, FeedEventArgs e)
        {
            if (e.Feed.Notifications.Count > 0)
                ShowNotificationPopup(e.Feed.Notifications[0]);
        }

        private void CloseSynchronizer()
        {
            if (_notifications != null)
            {
                _notifications.Dispose();
                _notifications = null;
            }

            if (_synchronizer != null)
            {
                _synchronizer.Dispose();
                _synchronizer = null;
            }
        }

#if !DEBUG
        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(false);
        }
#endif

        private void ShowNotificationPopup(FeedNotification notification)
        {
            string title = notification.Title;
            string text = notification.Message;
            _lastNotificationNotified = notification;

            if (String.IsNullOrEmpty(text))
            {
                text = title;
                title = Text;
            }

            _notifyIcon.ShowBalloonTip((int)TimeSpan.FromSeconds(12).TotalMilliseconds, text, title, ToolTipIcon.Info);
        }

        private void _exitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void _notifyIcon_Click(object sender, EventArgs e)
        {
            _notifications?.Show();
        }

        private void _notifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            int? postId = _lastNotificationNotified?.ParentId;
            if (postId.HasValue)
                _synchronizer?.OpenPost(postId.Value);
        }

        private Credentials LoadCredentials()
        {
            using (var key = Program.BaseKey)
            {
                string url = key.GetValue("URL") as string;
                string userName = key.GetValue("User name") as string;
                string password = key.GetValue("Password") as string;

                if (url != null && userName != null)
                {
                    return new Credentials(
                        url,
                        userName,
                        password == null ? null : Encryption.Decrypt(password)
                    );
                }
            }

            return null;
        }

        private void SaveCredentials(Credentials credentials)
        {
            using (var key = Program.BaseKey)
            {
                key.SetValue("URL", credentials.Url);
                key.SetValue("User name", credentials.UserName);
                key.SetValue("Password", Encryption.Encrypt(credentials.Password));
            }
        }

        private void Logout()
        {
            using (var key = Program.BaseKey)
            {
                key.DeleteValue("Password", false);
            }
        }

        private void _notifyIconMenu_Popup(object sender, EventArgs e)
        {
            _loginMenuItem.Visible = _synchronizer == null;
            _logoutMenuItem.Visible = _synchronizer != null;
        }

        private void _loginMenuItem_Click(object sender, EventArgs e)
        {
            Login();
        }

        private void _logoutMenuItem_Click(object sender, EventArgs e)
        {
            Logout();

            CloseSynchronizer();
        }
    }
}
