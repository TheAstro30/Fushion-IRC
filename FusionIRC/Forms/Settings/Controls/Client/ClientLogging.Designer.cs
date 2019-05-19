namespace FusionIRC.Forms.Settings.Controls.Client
{
    partial class ClientLogging
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
            this.lblPath = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.btnPath = new System.Windows.Forms.Button();
            this.lblKeep = new System.Windows.Forms.Label();
            this.cmbKeep = new System.Windows.Forms.ComboBox();
            this.cmbReload = new System.Windows.Forms.ComboBox();
            this.lblReload = new System.Windows.Forms.Label();
            this.gbOptions = new System.Windows.Forms.GroupBox();
            this.chkDate = new System.Windows.Forms.CheckBox();
            this.chkFolder = new System.Windows.Forms.CheckBox();
            this.chkStrip = new System.Windows.Forms.CheckBox();
            this.gbOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblPath
            // 
            this.lblPath.AutoSize = true;
            this.lblPath.Location = new System.Drawing.Point(62, 82);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(81, 15);
            this.lblPath.TabIndex = 0;
            this.lblPath.Text = "Logging path:";
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(149, 78);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(176, 23);
            this.txtPath.TabIndex = 1;
            // 
            // btnPath
            // 
            this.btnPath.Location = new System.Drawing.Point(331, 78);
            this.btnPath.Name = "btnPath";
            this.btnPath.Size = new System.Drawing.Size(30, 23);
            this.btnPath.TabIndex = 2;
            this.btnPath.Text = "...";
            this.btnPath.UseVisualStyleBackColor = true;
            // 
            // lblKeep
            // 
            this.lblKeep.AutoSize = true;
            this.lblKeep.Location = new System.Drawing.Point(118, 128);
            this.lblKeep.Name = "lblKeep";
            this.lblKeep.Size = new System.Drawing.Size(79, 15);
            this.lblKeep.TabIndex = 3;
            this.lblKeep.Text = "Keep logs for:";
            // 
            // cmbKeep
            // 
            this.cmbKeep.FormattingEnabled = true;
            this.cmbKeep.Location = new System.Drawing.Point(213, 125);
            this.cmbKeep.Name = "cmbKeep";
            this.cmbKeep.Size = new System.Drawing.Size(95, 23);
            this.cmbKeep.TabIndex = 4;
            // 
            // cmbReload
            // 
            this.cmbReload.FormattingEnabled = true;
            this.cmbReload.Location = new System.Drawing.Point(213, 154);
            this.cmbReload.Name = "cmbReload";
            this.cmbReload.Size = new System.Drawing.Size(95, 23);
            this.cmbReload.TabIndex = 6;
            // 
            // lblReload
            // 
            this.lblReload.AutoSize = true;
            this.lblReload.Location = new System.Drawing.Point(118, 157);
            this.lblReload.Name = "lblReload";
            this.lblReload.Size = new System.Drawing.Size(89, 15);
            this.lblReload.TabIndex = 5;
            this.lblReload.Text = "Reload logs for:";
            // 
            // gbOptions
            // 
            this.gbOptions.Controls.Add(this.chkStrip);
            this.gbOptions.Controls.Add(this.chkDate);
            this.gbOptions.Controls.Add(this.chkFolder);
            this.gbOptions.Location = new System.Drawing.Point(99, 195);
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.Size = new System.Drawing.Size(246, 100);
            this.gbOptions.TabIndex = 7;
            this.gbOptions.TabStop = false;
            this.gbOptions.Text = "Options:";
            // 
            // chkDate
            // 
            this.chkDate.AutoSize = true;
            this.chkDate.Location = new System.Drawing.Point(6, 47);
            this.chkDate.Name = "chkDate";
            this.chkDate.Size = new System.Drawing.Size(136, 19);
            this.chkDate.TabIndex = 1;
            this.chkDate.Text = "Date each log by day";
            this.chkDate.UseVisualStyleBackColor = true;
            // 
            // chkFolder
            // 
            this.chkFolder.AutoSize = true;
            this.chkFolder.Location = new System.Drawing.Point(6, 22);
            this.chkFolder.Name = "chkFolder";
            this.chkFolder.Size = new System.Drawing.Size(238, 19);
            this.chkFolder.TabIndex = 0;
            this.chkFolder.Text = "Create separate folders for each network";
            this.chkFolder.UseVisualStyleBackColor = true;
            // 
            // chkStrip
            // 
            this.chkStrip.AutoSize = true;
            this.chkStrip.Location = new System.Drawing.Point(6, 72);
            this.chkStrip.Name = "chkStrip";
            this.chkStrip.Size = new System.Drawing.Size(125, 19);
            this.chkStrip.TabIndex = 2;
            this.chkStrip.Text = "Strip control codes";
            this.chkStrip.UseVisualStyleBackColor = true;
            // 
            // ClientLogging
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbOptions);
            this.Controls.Add(this.cmbReload);
            this.Controls.Add(this.lblReload);
            this.Controls.Add(this.cmbKeep);
            this.Controls.Add(this.lblKeep);
            this.Controls.Add(this.btnPath);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.lblPath);
            this.Name = "ClientLogging";
            this.gbOptions.ResumeLayout(false);
            this.gbOptions.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Button btnPath;
        private System.Windows.Forms.Label lblKeep;
        private System.Windows.Forms.ComboBox cmbKeep;
        private System.Windows.Forms.ComboBox cmbReload;
        private System.Windows.Forms.Label lblReload;
        private System.Windows.Forms.GroupBox gbOptions;
        private System.Windows.Forms.CheckBox chkDate;
        private System.Windows.Forms.CheckBox chkFolder;
        private System.Windows.Forms.CheckBox chkStrip;

    }
}
