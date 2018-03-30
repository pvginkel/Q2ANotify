using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Q2ANotify
{
    public class Q2ACredentials
    {
        public string Url { get; }
        public string UserName { get; }
        public string Password { get; }

        public Q2ACredentials(string url, string userName, string password)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            if (userName == null)
                throw new ArgumentNullException(nameof(userName));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            Url = url;
            UserName = userName;
            Password = password;
        }
    }
}
