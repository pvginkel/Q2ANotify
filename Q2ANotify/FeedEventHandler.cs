using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Q2ANotify.Q2AApi;

namespace Q2ANotify
{
    public class FeedEventArgs : EventArgs
    {
        public Feed Feed { get; }

        public FeedEventArgs(Feed feed)
        {
            if (feed == null)
                throw new ArgumentNullException(nameof(feed));

            Feed = feed;
        }
    }

    public delegate void FeedEventHandler(object sender, FeedEventArgs e);
}
