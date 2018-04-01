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

            using (var db = new Db(Path.Combine(BasePath, "database.db")))
            {
                Application.Run(new MainForm(db));
            }
        }
    }
}
