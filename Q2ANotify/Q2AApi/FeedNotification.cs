using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Q2ANotify.Q2AApi
{
    public class FeedNotification
    {
        public DateTime DateTime { get; }
        public string Kind { get; }
        public string User { get; }
        public string Poster { get; }
        public string Title { get; }
        public string Message { get; }
        public int? ParentId { get; }
        public int? PostId { get; }

        public FeedNotification(DateTime dateTime, string kind, string user, string poster, string title, string message, int? parentId, int? postId)
        {
            if (kind == null)
                throw new ArgumentNullException(nameof(kind));
            if (title == null)
                throw new ArgumentNullException(nameof(title));

            DateTime = dateTime;
            Kind = kind;
            User = user;
            Poster = poster;
            Title = title;
            Message = message;
            ParentId = parentId;
            PostId = postId;
        }
    }
}
