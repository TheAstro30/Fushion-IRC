namespace FusionIRC.Forms.Settings.Controls.Client
{
    partial class ClientSystemTray
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
            this.gbTray = new System.Windows.Forms.GroupBox();
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnDefault = new System.Windows.Forms.Button();
            this.pnlIcon = new System.Windows.Forms.Panel();
            this.chkBalloon = new System.Windows.Forms.CheckBox();
            this.chkMinimized = new System.Windows.Forms.CheckBox();
            this.chkAlways = new System.Windows.Forms.CheckBox();
            this.gbTray.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbTray
            // 
            this.gbTray.Controls.Add(this.btnSelect);
            this.gbTray.Controls.Add(this.btnDefault);
            this.gbTray.Controls.Add(this.pnlIcon);
            this.gbTray.Location = new System.Drawing.Point(112, 168);
            this.gbTray.Name = "gbTray";
            this.gbTray.Size = new System.Drawing.Size(160, 95);
            this.gbTray.TabIndex = 3;
            this.gbTray.TabStop = false;
            this.gbTray.Text = "Tray icon:";
            // 
            // btnSelect
            // 
            this.btnSelect.Location = new System.Drawing.Point(76, 62);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(75, 23);
            this.btnSelect.TabIndex = 2;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = true;
            // 
            // btnDefault
            // 
            this.btnDefault.Location = new System.Drawing.Point(76, 33);
            this.btnDefault.Name = "btnDefault";
            this.btnDefault.Size = new System.Drawing.Size(75, 23);
            this.btnDefault.TabIndex = 1;
            this.btnDefault.Text = "Default";
            this.btnDefault.UseVisualStyleBackColor = true;
            // 
            // pnlIcon
            // 
            this.pnlIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlIcon.Location = new System.Drawing.Point(6, 22);
            this.pnlIcon.Name = "pnlIcon";
            this.pnlIcon.Size = new System.Drawing.Size(64, 64);
            this.pnlIcon.TabIndex = 0;
            // 
            // chkBalloon
            // 
            this.chkBalloon.AutoSize = true;
            this.chkBalloon.Location = new System.Drawing.Point(112, 143);
            this.chkBalloon.Name = "chkBalloon";
            this.chkBalloon.Size = new System.Drawing.Size(141, 19);
            this.chkBalloon.TabIndex = 2;
            this.chkBalloon.Text = "Show balloon tooltips";
            this.chkBalloon.UseVisualStyleBackColor = true;
            // 
            // chkMinimized
            // 
            this.chkMinimized.AutoSize = true;
            this.chkMinimized.Location = new System.Drawing.Point(112, 118);
            this.chkMinimized.Name = "chkMinimized";
            this.chkMinimized.Size = new System.Drawing.Size(221, 19);
            this.chkMinimized.TabIndex = 1;
            this.chkMinimized.Text = "Place in system tray when minimized";
            this.chkMinimized.UseVisualStyleBackColor = true;
            // 
            // chkAlways
            // 
            this.chkAlways.AutoSize = true;
            this.chkAlways.Location = new System.Drawing.Point(112, 93);
            this.chkAlways.Name = "chkAlways";
            this.chkAlways.Size = new System.Drawing.Size(183, 19);
            this.chkAlways.TabIndex = 0;
            this.chkAlways.Text = "Always show system tray icon";
            this.chkAlways.UseVisualStyleBackColor = true;
            // 
            // ClientSystemTray
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbTray);
            this.Controls.Add(this.chkBalloon);
            this.Controls.Add(this.chkMinimized);
            this.Controls.Add(this.chkAlways);
            this.Name = "ClientSystemTray";
            this.gbTray.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkAlways;
        private System.Windows.Forms.CheckBox chkMinimized;
        private System.Windows.Forms.CheckBox chkBalloon;
        private System.Windows.Forms.GroupBox gbTray;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnDefault;
        private System.Windows.Forms.Panel pnlIcon;

    }
}
