namespace FusionIRC.Forms.Settings.Controls.Connection
{
    partial class ConnectionOptions
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.chkInvalid = new System.Windows.Forms.CheckBox();
            this.chkSsl = new System.Windows.Forms.CheckBox();
            this.chkFave = new System.Windows.Forms.CheckBox();
            this.chkConnect = new System.Windows.Forms.CheckBox();
            this.gbOptions = new System.Windows.Forms.GroupBox();
            this.chkNext = new System.Windows.Forms.CheckBox();
            this.lblSecs = new System.Windows.Forms.Label();
            this.txtDelay = new System.Windows.Forms.TextBox();
            this.lblDelay = new System.Windows.Forms.Label();
            this.lblRetry = new System.Windows.Forms.Label();
            this.txtRetry = new System.Windows.Forms.TextBox();
            this.chkRetry = new System.Windows.Forms.CheckBox();
            this.chkRecon = new System.Windows.Forms.CheckBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.gbOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkInvalid
            // 
            this.chkInvalid.AutoSize = true;
            this.chkInvalid.Location = new System.Drawing.Point(108, 156);
            this.chkInvalid.Name = "chkInvalid";
            this.chkInvalid.Size = new System.Drawing.Size(257, 19);
            this.chkInvalid.TabIndex = 8;
            this.chkInvalid.Text = "Automatically accept invalid SSL certificates";
            this.chkInvalid.UseVisualStyleBackColor = true;
            // 
            // chkSsl
            // 
            this.chkSsl.AutoSize = true;
            this.chkSsl.Location = new System.Drawing.Point(88, 131);
            this.chkSsl.Name = "chkSsl";
            this.chkSsl.Size = new System.Drawing.Size(211, 19);
            this.chkSsl.TabIndex = 7;
            this.chkSsl.Text = "Accept SSL authentication requests";
            this.chkSsl.UseVisualStyleBackColor = true;
            // 
            // chkFave
            // 
            this.chkFave.AutoSize = true;
            this.chkFave.Location = new System.Drawing.Point(88, 106);
            this.chkFave.Name = "chkFave";
            this.chkFave.Size = new System.Drawing.Size(247, 19);
            this.chkFave.TabIndex = 6;
            this.chkFave.Text = "Show channel favorites dialog on connect";
            this.chkFave.UseVisualStyleBackColor = true;
            // 
            // chkConnect
            // 
            this.chkConnect.AutoSize = true;
            this.chkConnect.Location = new System.Drawing.Point(88, 81);
            this.chkConnect.Name = "chkConnect";
            this.chkConnect.Size = new System.Drawing.Size(240, 19);
            this.chkConnect.TabIndex = 5;
            this.chkConnect.Text = "Show connect to location dialog on start";
            this.chkConnect.UseVisualStyleBackColor = true;
            // 
            // gbOptions
            // 
            this.gbOptions.Controls.Add(this.chkNext);
            this.gbOptions.Controls.Add(this.lblSecs);
            this.gbOptions.Controls.Add(this.txtDelay);
            this.gbOptions.Controls.Add(this.lblDelay);
            this.gbOptions.Controls.Add(this.lblRetry);
            this.gbOptions.Controls.Add(this.txtRetry);
            this.gbOptions.Controls.Add(this.chkRetry);
            this.gbOptions.Enabled = false;
            this.gbOptions.Location = new System.Drawing.Point(80, 210);
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.Size = new System.Drawing.Size(257, 105);
            this.gbOptions.TabIndex = 4;
            this.gbOptions.TabStop = false;
            this.gbOptions.Text = "Retry options:";
            // 
            // chkNext
            // 
            this.chkNext.AutoSize = true;
            this.chkNext.Location = new System.Drawing.Point(8, 78);
            this.chkNext.Name = "chkNext";
            this.chkNext.Size = new System.Drawing.Size(195, 19);
            this.chkNext.TabIndex = 9;
            this.chkNext.Text = "Try next server in network group";
            this.chkNext.UseVisualStyleBackColor = true;
            // 
            // lblSecs
            // 
            this.lblSecs.AutoSize = true;
            this.lblSecs.Location = new System.Drawing.Point(198, 52);
            this.lblSecs.Name = "lblSecs";
            this.lblSecs.Size = new System.Drawing.Size(50, 15);
            this.lblSecs.TabIndex = 8;
            this.lblSecs.Text = "seconds";
            // 
            // txtDelay
            // 
            this.txtDelay.Location = new System.Drawing.Point(148, 49);
            this.txtDelay.MaxLength = 3;
            this.txtDelay.Name = "txtDelay";
            this.txtDelay.Size = new System.Drawing.Size(44, 23);
            this.txtDelay.TabIndex = 7;
            this.txtDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblDelay
            // 
            this.lblDelay.AutoSize = true;
            this.lblDelay.Location = new System.Drawing.Point(5, 52);
            this.lblDelay.Name = "lblDelay";
            this.lblDelay.Size = new System.Drawing.Size(137, 15);
            this.lblDelay.TabIndex = 6;
            this.lblDelay.Text = "Delay between attempts:";
            // 
            // lblRetry
            // 
            this.lblRetry.AutoSize = true;
            this.lblRetry.Location = new System.Drawing.Point(198, 23);
            this.lblRetry.Name = "lblRetry";
            this.lblRetry.Size = new System.Drawing.Size(36, 15);
            this.lblRetry.TabIndex = 5;
            this.lblRetry.Text = "times";
            // 
            // txtRetry
            // 
            this.txtRetry.Location = new System.Drawing.Point(148, 20);
            this.txtRetry.MaxLength = 3;
            this.txtRetry.Name = "txtRetry";
            this.txtRetry.Size = new System.Drawing.Size(44, 23);
            this.txtRetry.TabIndex = 4;
            this.txtRetry.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // chkRetry
            // 
            this.chkRetry.AutoSize = true;
            this.chkRetry.Location = new System.Drawing.Point(8, 22);
            this.chkRetry.Name = "chkRetry";
            this.chkRetry.Size = new System.Drawing.Size(119, 19);
            this.chkRetry.TabIndex = 3;
            this.chkRetry.Text = "Retry connection:";
            this.chkRetry.UseVisualStyleBackColor = true;
            // 
            // chkRecon
            // 
            this.chkRecon.AutoSize = true;
            this.chkRecon.Location = new System.Drawing.Point(88, 56);
            this.chkRecon.Name = "chkRecon";
            this.chkRecon.Size = new System.Drawing.Size(235, 19);
            this.chkRecon.TabIndex = 2;
            this.chkRecon.Text = "Reconnect automatically on disconnect";
            this.chkRecon.UseVisualStyleBackColor = true;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(169, 181);
            this.txtPort.MaxLength = 5;
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(53, 23);
            this.txtPort.TabIndex = 1;
            this.txtPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(90, 184);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(73, 15);
            this.lblPort.TabIndex = 0;
            this.lblPort.Text = "Default port:";
            // 
            // ConnectionOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkInvalid);
            this.Controls.Add(this.chkSsl);
            this.Controls.Add(this.chkFave);
            this.Controls.Add(this.chkConnect);
            this.Controls.Add(this.gbOptions);
            this.Controls.Add(this.chkRecon);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.lblPort);
            this.Name = "ConnectionOptions";
            this.gbOptions.ResumeLayout(false);
            this.gbOptions.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.CheckBox chkRecon;
        private System.Windows.Forms.CheckBox chkRetry;
        private System.Windows.Forms.GroupBox gbOptions;
        private System.Windows.Forms.CheckBox chkNext;
        private System.Windows.Forms.Label lblSecs;
        private System.Windows.Forms.TextBox txtDelay;
        private System.Windows.Forms.Label lblDelay;
        private System.Windows.Forms.Label lblRetry;
        private System.Windows.Forms.TextBox txtRetry;
        private System.Windows.Forms.CheckBox chkConnect;
        private System.Windows.Forms.CheckBox chkFave;
        private System.Windows.Forms.CheckBox chkSsl;
        private System.Windows.Forms.CheckBox chkInvalid;
    }
}
