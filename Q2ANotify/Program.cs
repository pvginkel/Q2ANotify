using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Q2ANotify.Database;
using Q2ANotify.Q2AApi;

namespace Q2ANotify
{
    public static class Program
    {
        public static RegistryKey BaseKey => Registry.CurrentUser.CreateSubKey(@"Software\Q2ANotify");
        public static string BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Q2ANotify");

        [STAThread]
        public static void Main()
        {
            ServicePointManager.Expect100Continue = false;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Api api = null;

            var credentials = LoadCredentials();
            if (credentials != null)
            {
                api = new Api(credentials);

                try
                {
                    api.Authenticate();
                }
                catch
                {
                    api = null;
                }
            }

            if (api == null)
            {
                using (var form = new LoginForm(credentials))
                {
                    if (form.ShowDialog() != DialogResult.OK)
                        return;

                    credentials = form.Credentials;
                    api = form.Api;
                }

                SaveCredentials(credentials);
            }

            using (var db = new Db(Path.Combine(BasePath, "database.db")))
            {
                Application.Run(new MainForm(api, db));
            }
        }

        private static void SaveCredentials(Credentials credentials)
        {
            using (var key = BaseKey)
            {
                key.SetValue("URL", credentials.Url);
                key.SetValue("User name", credentials.UserName);
                key.SetValue("Password", Encryption.Encrypt(credentials.Password));
            }
        }

        private static Credentials LoadCredentials()
        {
            using (var key = BaseKey)
            {
                string url = key.GetValue("URL") as string;
                string userName = key.GetValue("User name") as string;
                string password = key.GetValue("Password") as string;

                if (url != null && userName != null && password != null)
                {
                    return new Credentials(
                        url,
                        userName,
                        Encryption.Decrypt(password)
                    );
                }
            }

            return null;
        }
    }
}
