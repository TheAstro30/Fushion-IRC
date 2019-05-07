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
            this.lblPort = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.chkRecon = new System.Windows.Forms.CheckBox();
            this.chkRetry = new System.Windows.Forms.CheckBox();
            this.gbOptions = new System.Windows.Forms.GroupBox();
            this.chkNext = new System.Windows.Forms.CheckBox();
            this.lblSecs = new System.Windows.Forms.Label();
            this.txtDelay = new System.Windows.Forms.TextBox();
            this.lblDelay = new System.Windows.Forms.Label();
            this.lblRetry = new System.Windows.Forms.Label();
            this.txtRetry = new System.Windows.Forms.TextBox();
            this.gbOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(93, 101);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(73, 15);
            this.lblPort.TabIndex = 0;
            this.lblPort.Text = "Default port:";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(172, 98);
            this.txtPort.MaxLength = 5;
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(53, 23);
            this.txtPort.TabIndex = 1;
            this.txtPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // chkRecon
            // 
            this.chkRecon.AutoSize = true;
            this.chkRecon.Location = new System.Drawing.Point(96, 133);
            this.chkRecon.Name = "chkRecon";
            this.chkRecon.Size = new System.Drawing.Size(160, 19);
            this.chkRecon.TabIndex = 2;
            this.chkRecon.Text = "Reconnect on disconnect";
            this.chkRecon.UseVisualStyleBackColor = true;
            // 
            // chkRetry
            // 
            this.chkRetry.AutoSize = true;
            this.chkRetry.Location = new System.Drawing.Point(10, 22);
            this.chkRetry.Name = "chkRetry";
            this.chkRetry.Size = new System.Drawing.Size(119, 19);
            this.chkRetry.TabIndex = 3;
            this.chkRetry.Text = "Retry connection:";
            this.chkRetry.UseVisualStyleBackColor = true;
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
            this.gbOptions.Location = new System.Drawing.Point(86, 165);
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.Size = new System.Drawing.Size(257, 105);
            this.gbOptions.TabIndex = 4;
            this.gbOptions.TabStop = false;
            this.gbOptions.Text = "Retry options:";
            // 
            // chkNext
            // 
            this.chkNext.AutoSize = true;
            this.chkNext.Location = new System.Drawing.Point(10, 78);
            this.chkNext.Name = "chkNext";
            this.chkNext.Size = new System.Drawing.Size(195, 19);
            this.chkNext.TabIndex = 9;
            this.chkNext.Text = "Try next server in network group";
            this.chkNext.UseVisualStyleBackColor = true;
            // 
            // lblSecs
            // 
            this.lblSecs.AutoSize = true;
            this.lblSecs.Location = new System.Drawing.Point(200, 52);
            this.lblSecs.Name = "lblSecs";
            this.lblSecs.Size = new System.Drawing.Size(50, 15);
            this.lblSecs.TabIndex = 8;
            this.lblSecs.Text = "seconds";
            // 
            // txtDelay
            // 
            this.txtDelay.Location = new System.Drawing.Point(150, 49);
            this.txtDelay.MaxLength = 3;
            this.txtDelay.Name = "txtDelay";
            this.txtDelay.Size = new System.Drawing.Size(44, 23);
            this.txtDelay.TabIndex = 7;
            this.txtDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblDelay
            // 
            this.lblDelay.AutoSize = true;
            this.lblDelay.Location = new System.Drawing.Point(7, 52);
            this.lblDelay.Name = "lblDelay";
            this.lblDelay.Size = new System.Drawing.Size(137, 15);
            this.lblDelay.TabIndex = 6;
            this.lblDelay.Text = "Delay between attempts:";
            // 
            // lblRetry
            // 
            this.lblRetry.AutoSize = true;
            this.lblRetry.Location = new System.Drawing.Point(200, 23);
            this.lblRetry.Name = "lblRetry";
            this.lblRetry.Size = new System.Drawing.Size(36, 15);
            this.lblRetry.TabIndex = 5;
            this.lblRetry.Text = "times";
            // 
            // txtRetry
            // 
            this.txtRetry.Location = new System.Drawing.Point(150, 20);
            this.txtRetry.MaxLength = 3;
            this.txtRetry.Name = "txtRetry";
            this.txtRetry.Size = new System.Drawing.Size(44, 23);
            this.txtRetry.TabIndex = 4;
            this.txtRetry.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ConnectionOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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
    }
}
