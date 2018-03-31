using System;
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
        private readonly Api _api;
        private readonly Db _db;
        private NotificationsForm _notifications;

        public MainForm(Api api, Db db)
        {
            if (api == null)
                throw new ArgumentNullException(nameof(api));
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            _api = api;
            _db = db;

            InitializeComponent();

            _notifyIcon.ContextMenu = _notifyIconMenu;

            _notifications = new NotificationsForm(api, db);

#if DEBUG
            // Lower the timer for debug to make it easier to test the API.
            _timer.Interval = 1000;
#endif

            Disposed += MainForm_Disposed;

            var feed = LoadFeed();
            if (feed != null)
                _notifications.LoadFeed(feed);

#if DEBUG
            Shown += (s, e) => _notifications.Show();
#endif
        }

        private Feed LoadFeed()
        {
            FeedUser user;
            var notifications = new List<FeedNotification>();

            using (var ctx = _db.OpenContext())
            {
                int userId;

                using (var reader = ctx.ExecuteReader("SELECT rowid, name, points, badges_bronze, badges_silver, badges_gold FROM user"))
                {
                    if (!reader.Read())
                        return null;

                    userId = (int)(long)reader["rowid"];

                    user = new FeedUser(
                        (string)reader["name"],
                        (int)reader["points"],
                        new[]
                        {
                            new FeedUserBadge("bronze", (int)reader["badges_bronze"]),
                            new FeedUserBadge("silver", (int)reader["badges_silver"]),
                            new FeedUserBadge("gold", (int)reader["badges_gold"])
                        }
                    );
                }

                using (var reader = ctx.ExecuteReader(
                    "SELECT rowid, userid, datetime, kind, user, poster, title, message, parentid, postid FROM notification WHERE userid = @userid ORDER BY datetime DESC",
                    ("@userid", userId)
                ))
                {
                    while (reader.Read())
                    {
                        notifications.Add(new FeedNotification(
                            (long)reader["rowid"],
                            DbUtil.ParseDateTime((string)reader["datetime"]),
                            (string)reader["kind"],
                            reader["user"] as string,
                            reader["poster"] as string,
                            (string)reader["title"],
                            reader["message"] as string,
                            reader["parentid"] as int?,
                            reader["postid"] as int?
                        ));
                    }
                }
            }

            return new Feed(user, notifications);
        }

        private void MainForm_Disposed(object sender, EventArgs e)
        {
            _notifications.Dispose();
            _notifications = null;
        }

#if !DEBUG
        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(false);
        }
#endif

        private void _timer_Tick(object sender, EventArgs e)
        {
            GetFeed();
        }

        private void GetFeed()
        {
            try
            {
                DoGetFeed();
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Failed to get feed: {0}", ex.Message);
            }
        }

        private void DoGetFeed()
        {
            DateTime? lastNotification = null;
            int? userId = null;

            using (var ctx = _db.OpenContext())
            {
                using (var reader = ctx.ExecuteReader("SELECT rowid, last_notification FROM user"))
                {
                    if (reader.Read())
                    {
                        userId = reader.GetInt32(0);
                        lastNotification = DbUtil.ParseDateTime(reader.GetString(1));
                    }
                }

                ctx.Commit();
            }

            var feed = _api.GetFeed(lastNotification);

            using (var ctx = _db.OpenContext())
            {
                string name = feed.User.Name;
                int points = feed.User.Points;
                int badgesBronze = 0;
                int badgesSilver = 0;
                int badgesGold = 0;
                lastNotification = feed.Notifications.Select(p => (DateTime?)p.DateTime).Max() ?? lastNotification;

                foreach (var badge in feed.User.Badges)
                {
                    switch (badge.Type)
                    {
                        case "bronze":
                            badgesBronze = badge.Count;
                            break;
                        case "silver":
                            badgesSilver = badge.Count;
                            break;
                        case "gold":
                            badgesGold = badge.Count;
                            break;
                    }
                }

                string sql;
                if (userId.HasValue)
                    sql = "UPDATE user SET name = @name, points = @points, badges_bronze = @badges_bronze, badges_silver = @badges_silver, badges_gold = @badges_gold, last_notification = @last_notification WHERE rowid = @id";
                else
                    sql = "INSERT INTO user (name, points, badges_bronze, badges_silver, badges_gold, last_notification) VALUES (@name, @points, @badges_bronze, @badges_silver, @badges_gold, @last_notification)";

                ctx.ExecuteNonQuery(
                    sql,
                    ("@name", name),
                    ("@points", points),
                    ("@badges_bronze", badgesBronze),
                    ("@badges_silver", badgesSilver),
                    ("@badges_gold", badgesGold),
                    ("@last_notification", DbUtil.PrintDateTimeOpt(lastNotification)),
                    ("@id", userId)
                );

                if (!userId.HasValue)
                    userId = (int)ctx.LastInsertRowId;

                foreach (var notification in feed.Notifications)
                {
                    ctx.ExecuteNonQuery(
                        "INSERT INTO notification (userid, datetime, kind, user, poster, title, message, parentid, postid) VALUES (@userid, @datetime, @kind, @user, @poster, @title, @message, @parentid, @postid)",
                        ("@userid", userId.Value),
                        ("@datetime", DbUtil.PrintDateTime(notification.DateTime)),
                        ("@kind", notification.Kind),
                        ("@user", notification.User),
                        ("@poster", notification.Poster),
                        ("@title", notification.Title),
                        ("@message", notification.Message),
                        ("@parentid", notification.ParentId),
                        ("@postid", notification.PostId)
                    );

                    notification.Id = ctx.LastInsertRowId;
                }

                ctx.Commit();
            }

            _notifications.LoadFeed(feed);
        }

        private void _exitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void _notifyIcon_Click(object sender, EventArgs e)
        {
            _notifications.Show();
        }
    }
}
