using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Q2ANotify.Q2AApi
{
    public class Api
    {
        private static readonly Regex CodeRe = new Regex(@"<input[^>]*name *= *""code""[^>]*value *= *""([^""]*)""[^>]*>", RegexOptions.IgnoreCase);
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        private readonly Credentials _credentials;
        private readonly CookieContainer _cookieContainer = new CookieContainer();

        public Api(Credentials credentials)
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

        public string GetPostLink(int postId)
        {
            return _credentials.Url.TrimEnd('/') + "/" + postId + "/";
        }

        public Feed GetFeed(DateTime? since)
        {
            string url = "notify-get-updates";
            if (since.HasValue)
                url += "?since=" + Uri.EscapeDataString(since.Value.ToString(DateTimeFormat));

            string response = DoRequest(url);

            JObject obj;

            using (var reader = new StringReader(response))
            using (var json = new JsonTextReader(reader))
            {
                json.DateParseHandling = DateParseHandling.None;
                json.FloatParseHandling = FloatParseHandling.Decimal;

                obj = JObject.Load(json);
            }

            FeedUser user = null;
            IList<FeedNotification> notifications = null;

            foreach (var entry in obj)
            {
                switch (entry.Key)
                {
                    case "user":
                        user = ParseFeedUser((JObject)entry.Value);
                        break;
                    case "feed":
                        notifications = ParseFeedNotifications((JArray)entry.Value);
                        break;
                }
            }

            return new Feed(user, notifications);
        }

        private FeedUser ParseFeedUser(JObject obj)
        {
            var badges = new List<FeedUserBadge>();

            foreach (var entry in (JObject)obj["badges"])
            {
                badges.Add(new FeedUserBadge(
                    entry.Key,
                    (int)entry.Value
                ));
            }

            return new FeedUser(
                (string)obj["name"],
                (int)obj["points"],
                badges
            );
        }

        private IList<FeedNotification> ParseFeedNotifications(JArray array)
        {
            var notifications = new List<FeedNotification>();

            foreach (JObject entry in array)
            {
                notifications.Add(new FeedNotification(
                    null,
                    DateTime.ParseExact((string)entry["datetime"], DateTimeFormat, CultureInfo.InvariantCulture),
                    (string)entry["kind"],
                    (string)entry["user"],
                    (string)entry["poster"],
                    (string)entry["title"],
                    (string)entry["message"],
                    (int?)entry["parentid"],
                    (int?)entry["postid"]
                ));
            }

            return notifications;
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
