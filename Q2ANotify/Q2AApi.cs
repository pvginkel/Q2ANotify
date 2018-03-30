using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Q2ANotify
{
    public class Q2AApi
    {
        private static readonly Regex CodeRe = new Regex(@"<input[^>]*name *= *""code""[^>]*value *= *""([^""]*)""[^>]*>", RegexOptions.IgnoreCase);

        private readonly Q2ACredentials _credentials;
        private readonly CookieContainer _cookieContainer = new CookieContainer();

        public Q2AApi(Q2ACredentials credentials)
        {
            if (credentials == null)
                throw new ArgumentNullException(nameof(credentials));

            _credentials = credentials;
        }

        public void Authenticate()
        {
            string code = GetFormCode("login");

            string post = UriEncode(new Dictionary<string, string>
            {
                ["emailhandle"] = _credentials.UserName,
                ["password"] = _credentials.Password,
                ["remember"] = "1",
                ["dologin"] = "1",
                ["code"] = code
            });

            DoRequest("login?to=/", post);

            if (_cookieContainer.GetCookies(new Uri(_credentials.Url))["qa_session"] == null)
                throw new IOException("Invalid user name or password");
        }

        private string GetFormCode(string url)
        {
            string html = DoRequest(url);
            var match = CodeRe.Match(html);
            if (!match.Success)
                throw new InvalidOperationException("Cannot get form code");
            return match.Groups[1].Value.Trim();
        }

        private string DoRequest(string url, string post = null, string contentType = null)
        {
            url = _credentials.Url.TrimEnd('/') + "/" + url.TrimStart('/');

            if (post != null && contentType == null)
                contentType = "application/x-www-form-urlencoded";

            var request = (HttpWebRequest)WebRequest.Create(url);

            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.CookieContainer = _cookieContainer;
            request.Method = post == null ? "GET" : "POST";
            request.ContentType = contentType;

            if (post != null)
            {
                var bytes = Encoding.UTF8.GetBytes(post);

                request.ContentLength = bytes.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                if (stream == null)
                    return null;

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private string UriEncode(Dictionary<string, string> values)
        {
            var sb = new StringBuilder();

            foreach (var entry in values)
            {
                if (sb.Length > 0)
                    sb.Append('&');
                sb.Append(Uri.EscapeDataString(entry.Key)).Append('=').Append(Uri.EscapeDataString(entry.Value));
            }

            return sb.ToString();
        }
    }
}
