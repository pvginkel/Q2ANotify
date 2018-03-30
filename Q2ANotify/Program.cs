using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Q2ANotify
{
    public static class Program
    {
        public static RegistryKey BaseKey => Registry.CurrentUser.CreateSubKey(@"Software\Q2ANotify");

        [STAThread]
        public static void Main()
        {
            ServicePointManager.Expect100Continue = false;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Q2AApi api = null;

            var credentials = LoadCredentials();
            if (credentials != null)
            {
                api = new Q2AApi(credentials);

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

            Application.Run(new MainForm(api));
        }

        private static void SaveCredentials(Q2ACredentials credentials)
        {
            using (var key = BaseKey)
            {
                key.SetValue("URL", credentials.Url);
                key.SetValue("User name", credentials.UserName);
                key.SetValue("Password", Encryption.Encrypt(credentials.Password));
            }
        }

        private static Q2ACredentials LoadCredentials()
        {
            using (var key = BaseKey)
            {
                string url = key.GetValue("URL") as string;
                string userName = key.GetValue("User name") as string;
                string password = key.GetValue("Password") as string;

                if (url != null && userName != null && password != null)
                {
                    return new Q2ACredentials(
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
