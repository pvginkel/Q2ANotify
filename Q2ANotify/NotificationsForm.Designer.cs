namespace Q2ANotify
{
    partial class NotificationsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this._elementControl = new GdiPresentation.ElementControl();
            this.SuspendLayout();
            // 
            // _elementControl
            // 
            this._elementControl.AutoSize = true;
            this._elementControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._elementControl.Location = new System.Drawing.Point(0, 0);
            this._elementControl.Name = "_elementControl";
            this._elementControl.ResizeTarget = null;
            this._elementControl.Size = new System.Drawing.Size(0, 0);
            this._elementControl.TabIndex = 0;
            // 
            // NotificationsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(434, 497);
            this.Controls.Add(this._elementControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "NotificationsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "NotificationsForm";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NotificationsForm_FormClosing);
            this.VisibleChanged += new System.EventHandler(this.NotificationsForm_VisibleChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private GdiPresentation.ElementControl _elementControl;
    }
}