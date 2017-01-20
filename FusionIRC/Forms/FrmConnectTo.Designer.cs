namespace FusionIRC.Forms
{
    partial class FrmConnectTo
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
            this.lblAddress = new System.Windows.Forms.Label();
            this.txtAddress = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.lblHeader = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.lblChannels = new System.Windows.Forms.Label();
            this.txtChannels = new System.Windows.Forms.TextBox();
            this.chkNewWindow = new System.Windows.Forms.CheckBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblAddress
            // 
            this.lblAddress.AutoSize = true;
            this.lblAddress.BackColor = System.Drawing.Color.Transparent;
            this.lblAddress.Location = new System.Drawing.Point(12, 53);
            this.lblAddress.Name = "lblAddress";
            this.lblAddress.Size = new System.Drawing.Size(85, 15);
            this.lblAddress.TabIndex = 0;
            this.lblAddress.Text = "Server address:";
            // 
            // txtAddress
            // 
            this.txtAddress.Location = new System.Drawing.Point(15, 71);
            this.txtAddress.MaxLength = 200;
            this.txtAddress.Name = "txtAddress";
            this.txtAddress.Size = new System.Drawing.Size(215, 23);
            this.txtAddress.TabIndex = 1;
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.BackColor = System.Drawing.Color.Transparent;
            this.lblPort.Location = new System.Drawing.Point(233, 53);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(32, 15);
            this.lblPort.TabIndex = 2;
            this.lblPort.Text = "Port:";
            // 
            // lblHeader
            // 
            this.lblHeader.BackColor = System.Drawing.Color.Transparent;
            this.lblHeader.Location = new System.Drawing.Point(12, 9);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(387, 35);
            this.lblHeader.TabIndex = 3;
            this.lblHeader.Text = "You can specify an IRC server to connect to directly here. Use \'+\' in the port fi" +
                "eld to specify the server is SSL";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(236, 71);
            this.txtPort.MaxLength = 5;
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(66, 23);
            this.txtPort.TabIndex = 4;
            // 
            // lblChannels
            // 
            this.lblChannels.AutoSize = true;
            this.lblChannels.BackColor = System.Drawing.Color.Transparent;
            this.lblChannels.Location = new System.Drawing.Point(12, 97);
            this.lblChannels.Name = "lblChannels";
            this.lblChannels.Size = new System.Drawing.Size(249, 15);
            this.lblChannels.TabIndex = 5;
            this.lblChannels.Text = "Channels to join on connect (separated by \',\'):";
            // 
            // txtChannels
            // 
            this.txtChannels.Location = new System.Drawing.Point(15, 115);
            this.txtChannels.Name = "txtChannels";
            this.txtChannels.Size = new System.Drawing.Size(384, 23);
            this.txtChannels.TabIndex = 6;
            // 
            // chkNewWindow
            // 
            this.chkNewWindow.AutoSize = true;
            this.chkNewWindow.BackColor = System.Drawing.Color.Transparent;
            this.chkNewWindow.Location = new System.Drawing.Point(15, 144);
            this.chkNewWindow.Name = "chkNewWindow";
            this.chkNewWindow.Size = new System.Drawing.Size(158, 19);
            this.chkNewWindow.TabIndex = 7;
            this.chkNewWindow.Text = "New connection window";
            this.chkNewWindow.UseVisualStyleBackColor = false;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(243, 180);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 8;
            this.btnConnect.Tag = "CONNECT";
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(324, 180);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 9;
            this.btnClose.Tag = "CLOSE";
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // FrmConnectTo
            // 
            this.AcceptButton = this.btnConnect;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(411, 215);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.chkNewWindow);
            this.Controls.Add(this.txtChannels);
            this.Controls.Add(this.lblChannels);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.txtAddress);
            this.Controls.Add(this.lblAddress);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmConnectTo";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Connect To Location";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblAddress;
        private System.Windows.Forms.TextBox txtAddress;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblChannels;
        private System.Windows.Forms.TextBox txtChannels;
        private System.Windows.Forms.CheckBox chkNewWindow;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnClose;
    }
}