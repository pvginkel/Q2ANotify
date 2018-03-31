using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Q2ANotify.Q2AApi
{
    public class FeedUserBadge
    {
        public string Type { get; }
        public int Count { get; }

        public FeedUserBadge(string type, int count)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Type = type;
            Count = count;
        }
    }
}
