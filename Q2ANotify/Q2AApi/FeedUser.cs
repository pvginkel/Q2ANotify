using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Q2ANotify.Q2AApi
{
    public class FeedUser
    {
        public string Name { get; }
        public int Points { get; }
        public IList<FeedUserBadge> Badges { get; }

        public FeedUser(string name, int points, IList<FeedUserBadge> badges)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (badges == null)
                throw new ArgumentNullException(nameof(badges));

            Name = name;
            Points = points;
            Badges = badges;
        }
    }
}
