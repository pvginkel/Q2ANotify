using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Q2ANotify.Q2AApi;

namespace Q2ANotify
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this._notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this._notifyIconMenu = new System.Windows.Forms.ContextMenu();
            this._exitMenuItem = new System.Windows.Forms.MenuItem();
            this._loginMenuItem = new System.Windows.Forms.MenuItem();
            this._logoutMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // _notifyIcon
            // 
            this._notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("_notifyIcon.Icon")));
            this._notifyIcon.Text = "Q2A Notifications";
            this._notifyIcon.Visible = true;
            this._notifyIcon.BalloonTipClicked += new System.EventHandler(this._notifyIcon_BalloonTipClicked);
            this._notifyIcon.Click += new System.EventHandler(this._notifyIcon_Click);
            // 
            // _notifyIconMenu
            // 
            this._notifyIconMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._loginMenuItem,
            this._logoutMenuItem,
            this.menuItem3,
            this._exitMenuItem});
            this._notifyIconMenu.Popup += new System.EventHandler(this._notifyIconMenu_Popup);
            // 
            // _exitMenuItem
            // 
            this._exitMenuItem.Index = 3;
            this._exitMenuItem.Text = "E&xit";
            this._exitMenuItem.Click += new System.EventHandler(this._exitMenuItem_Click);
            // 
            // _loginMenuItem
            // 
            this._loginMenuItem.Index = 0;
            this._loginMenuItem.Text = "Login";
            this._loginMenuItem.Click += new System.EventHandler(this._loginMenuItem_Click);
            // 
            // _logoutMenuItem
            // 
            this._logoutMenuItem.Index = 1;
            this._logoutMenuItem.Text = "Logout";
            this._logoutMenuItem.Click += new System.EventHandler(this._logoutMenuItem_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 2;
            this.menuItem3.Text = "-";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "MainForm";
            this.Text = "Q2A Notify";
            this.ResumeLayout(false);

        }

        #endregion
        private NotifyIcon _notifyIcon;
        private ContextMenu _notifyIconMenu;
        private MenuItem _exitMenuItem;
        private MenuItem _loginMenuItem;
        private MenuItem _logoutMenuItem;
        private MenuItem menuItem3;
    }
}