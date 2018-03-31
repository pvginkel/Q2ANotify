using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Q2ANotify.Q2AApi;

namespace Q2ANotify
{
    public partial class LoginForm : Form
    {
        public Credentials Credentials { get; private set; }
        public Api Api { get; private set; }

        public LoginForm(Credentials credentials)
        {
            InitializeComponent();

            Font = SystemFonts.MessageBoxFont;

            _url.Text = credentials?.Url;
            _userName.Text = credentials?.UserName;
            _password.Text = credentials?.Password;

            UpdateEnabled();
        }

        private void UpdateEnabled()
        {
            _loginButton.Enabled =
                _url.Text.Length > 0 &&
                _userName.Text.Length > 0 &&
                _password.Text.Length > 0;
        }

        private void _cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void _url_TextChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void _userName_TextChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void _password_TextChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void _loginButton_Click(object sender, EventArgs e)
        {
            Credentials = new Credentials(
                _url.Text,
                _userName.Text,
                _password.Text
            );

            Api = new Api(Credentials);

            try
            {
                Api.Authenticate();

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Invalid user name or password:" + Environment.NewLine + Environment.NewLine + ex.Message,
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
