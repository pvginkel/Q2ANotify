using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Q2ANotify.Q2AApi
{
    public class Feed
    {
        public FeedUser User { get; }
        public IList<FeedNotification> Notifications { get; }

        public Feed(FeedUser user, IList<FeedNotification> notifications)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (notifications == null)
                throw new ArgumentNullException(nameof(notifications));

            User = user;
            Notifications = notifications;
        }
    }
}
