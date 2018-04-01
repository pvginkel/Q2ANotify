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
    public class Synchronizer : IDisposable
    {
        private readonly Db _db;
        private readonly Api _api;
        private Timer _timer;
        private bool _disposed;

        public event FeedEventHandler FeedUpdated;

        public Synchronizer(Api api, Db db)
        {
            if (api == null)
                throw new ArgumentNullException(nameof(api));
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            _api = api;
            _db = db;

            _timer = new Timer
            {
#if !DEBUG
                Interval = 60000
#else
                // Lower the timer for debug to make it easier to test the API.
                Interval = 1000
#endif
            };

            _timer.Tick += (s, e) => GetFeed();

            _timer.Start();
        }

        public Feed LoadFeedFromDatabase()
        {
            FeedUser user;
            var notifications = new List<FeedNotification>();

            using (var ctx = _db.OpenContext())
            {
                using (var reader = ctx.ExecuteReader("SELECT name, points, badges_bronze, badges_silver, badges_gold FROM user"))
                {
                    if (!reader.Read())
                        return null;

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

                using (var reader = ctx.ExecuteReader("SELECT rowid, datetime, kind, user, poster, title, message, parentid, postid FROM notification ORDER BY datetime DESC"))
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
            bool haveUser;

            using (var ctx = _db.OpenContext())
            {
                using (var reader = ctx.ExecuteReader("SELECT last_notification FROM user"))
                {
                    haveUser = reader.Read();
                    if (haveUser)
                        lastNotification = DbUtil.ParseDateTime(reader.GetString(0));
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
                if (haveUser)
                    sql = "UPDATE user SET name = @name, points = @points, badges_bronze = @badges_bronze, badges_silver = @badges_silver, badges_gold = @badges_gold, last_notification = @last_notification";
                else
                    sql = "INSERT INTO user (name, points, badges_bronze, badges_silver, badges_gold, last_notification) VALUES (@name, @points, @badges_bronze, @badges_silver, @badges_gold, @last_notification)";

                ctx.ExecuteNonQuery(
                    sql,
                    ("@name", name),
                    ("@points", points),
                    ("@badges_bronze", badgesBronze),
                    ("@badges_silver", badgesSilver),
                    ("@badges_gold", badgesGold),
                    ("@last_notification", DbUtil.PrintDateTimeOpt(lastNotification))
                );

                foreach (var notification in feed.Notifications)
                {
                    ctx.ExecuteNonQuery(
                        "INSERT INTO notification (datetime, kind, user, poster, title, message, parentid, postid) VALUES (@datetime, @kind, @user, @poster, @title, @message, @parentid, @postid)",
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

            OnFeedUpdated(new FeedEventArgs(feed));
        }

        protected virtual void OnFeedUpdated(FeedEventArgs e)
        {
            FeedUpdated?.Invoke(this, e);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }

                _disposed = true;
            }
        }

        public void OpenPost(int postId)
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

        public void DeleteNotification(long id)
        {
            using (var ctx = _db.OpenContext())
            {
                ctx.ExecuteNonQuery("DELETE FROM notification WHERE rowid = @id", ("@id", id));
                ctx.Commit();
            }
        }

        public void DeleteAllNotifications()
        {
            using (var ctx = _db.OpenContext())
            {
                ctx.ExecuteNonQuery("DELETE FROM notification");
                ctx.Commit();
            }
        }
    }
}
