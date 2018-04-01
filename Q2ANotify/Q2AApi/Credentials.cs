using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Q2ANotify.Q2AApi
{
    public class Credentials
    {
        public string Url { get; }
        public string UserName { get; }
        public string Password { get; }

        public Credentials(string url, string userName, string password)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            if (userName == null)
                throw new ArgumentNullException(nameof(userName));

            Url = url;
            UserName = userName;
            Password = password;
        }
    }
}
