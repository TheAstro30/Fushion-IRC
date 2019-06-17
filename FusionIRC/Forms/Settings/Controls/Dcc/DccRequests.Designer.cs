namespace FusionIRC.Forms.Settings.Controls.Dcc
{
    partial class DccRequests
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
            this.gbGet = new System.Windows.Forms.GroupBox();
            this.cmbExists = new System.Windows.Forms.ComboBox();
            this.lblExists = new System.Windows.Forms.Label();
            this.rbGetIgnore = new System.Windows.Forms.RadioButton();
            this.rbGetAuto = new System.Windows.Forms.RadioButton();
            this.rbGetShow = new System.Windows.Forms.RadioButton();
            this.gbChat = new System.Windows.Forms.GroupBox();
            this.rbChatIgnore = new System.Windows.Forms.RadioButton();
            this.rbChatAuto = new System.Windows.Forms.RadioButton();
            this.rbChatShow = new System.Windows.Forms.RadioButton();
            this.gbGet.SuspendLayout();
            this.gbChat.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbGet
            // 
            this.gbGet.Controls.Add(this.cmbExists);
            this.gbGet.Controls.Add(this.lblExists);
            this.gbGet.Controls.Add(this.rbGetIgnore);
            this.gbGet.Controls.Add(this.rbGetAuto);
            this.gbGet.Controls.Add(this.rbGetShow);
            this.gbGet.Location = new System.Drawing.Point(91, 61);
            this.gbGet.Name = "gbGet";
            this.gbGet.Size = new System.Drawing.Size(254, 130);
            this.gbGet.TabIndex = 0;
            this.gbGet.TabStop = false;
            this.gbGet.Text = "On DCC Get requests:";
            // 
            // cmbExists
            // 
            this.cmbExists.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbExists.FormattingEnabled = true;
            this.cmbExists.Location = new System.Drawing.Point(95, 97);
            this.cmbExists.Name = "cmbExists";
            this.cmbExists.Size = new System.Drawing.Size(91, 23);
            this.cmbExists.TabIndex = 4;
            // 
            // lblExists
            // 
            this.lblExists.AutoSize = true;
            this.lblExists.Location = new System.Drawing.Point(22, 100);
            this.lblExists.Name = "lblExists";
            this.lblExists.Size = new System.Drawing.Size(67, 15);
            this.lblExists.TabIndex = 3;
            this.lblExists.Text = "If file exists:";
            // 
            // rbGetIgnore
            // 
            this.rbGetIgnore.AutoSize = true;
            this.rbGetIgnore.Location = new System.Drawing.Point(6, 72);
            this.rbGetIgnore.Name = "rbGetIgnore";
            this.rbGetIgnore.Size = new System.Drawing.Size(121, 19);
            this.rbGetIgnore.TabIndex = 2;
            this.rbGetIgnore.TabStop = true;
            this.rbGetIgnore.Text = "Ignore all requests";
            this.rbGetIgnore.UseVisualStyleBackColor = true;
            // 
            // rbGetAuto
            // 
            this.rbGetAuto.AutoSize = true;
            this.rbGetAuto.Location = new System.Drawing.Point(6, 47);
            this.rbGetAuto.Name = "rbGetAuto";
            this.rbGetAuto.Size = new System.Drawing.Size(220, 19);
            this.rbGetAuto.TabIndex = 1;
            this.rbGetAuto.TabStop = true;
            this.rbGetAuto.Text = "Auto-accept file (not recommended)";
            this.rbGetAuto.UseVisualStyleBackColor = true;
            // 
            // rbGetShow
            // 
            this.rbGetShow.AutoSize = true;
            this.rbGetShow.Location = new System.Drawing.Point(6, 22);
            this.rbGetShow.Name = "rbGetShow";
            this.rbGetShow.Size = new System.Drawing.Size(221, 19);
            this.rbGetShow.TabIndex = 0;
            this.rbGetShow.TabStop = true;
            this.rbGetShow.Text = "Show request dialog (recommended)";
            this.rbGetShow.UseVisualStyleBackColor = true;
            // 
            // gbChat
            // 
            this.gbChat.Controls.Add(this.rbChatIgnore);
            this.gbChat.Controls.Add(this.rbChatAuto);
            this.gbChat.Controls.Add(this.rbChatShow);
            this.gbChat.Location = new System.Drawing.Point(91, 197);
            this.gbChat.Name = "gbChat";
            this.gbChat.Size = new System.Drawing.Size(254, 104);
            this.gbChat.TabIndex = 1;
            this.gbChat.TabStop = false;
            this.gbChat.Text = "On DCC Chat requests:";
            // 
            // rbChatIgnore
            // 
            this.rbChatIgnore.AutoSize = true;
            this.rbChatIgnore.Location = new System.Drawing.Point(6, 72);
            this.rbChatIgnore.Name = "rbChatIgnore";
            this.rbChatIgnore.Size = new System.Drawing.Size(121, 19);
            this.rbChatIgnore.TabIndex = 2;
            this.rbChatIgnore.TabStop = true;
            this.rbChatIgnore.Text = "Ignore all requests";
            this.rbChatIgnore.UseVisualStyleBackColor = true;
            // 
            // rbChatAuto
            // 
            this.rbChatAuto.AutoSize = true;
            this.rbChatAuto.Location = new System.Drawing.Point(6, 47);
            this.rbChatAuto.Name = "rbChatAuto";
            this.rbChatAuto.Size = new System.Drawing.Size(230, 19);
            this.rbChatAuto.TabIndex = 1;
            this.rbChatAuto.TabStop = true;
            this.rbChatAuto.Text = "Auto-accept request (not recommend)";
            this.rbChatAuto.UseVisualStyleBackColor = true;
            // 
            // rbChatShow
            // 
            this.rbChatShow.AutoSize = true;
            this.rbChatShow.Location = new System.Drawing.Point(6, 22);
            this.rbChatShow.Name = "rbChatShow";
            this.rbChatShow.Size = new System.Drawing.Size(221, 19);
            this.rbChatShow.TabIndex = 0;
            this.rbChatShow.TabStop = true;
            this.rbChatShow.Text = "Show request dialog (recommended)";
            this.rbChatShow.UseVisualStyleBackColor = true;
            // 
            // DccRequests
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbChat);
            this.Controls.Add(this.gbGet);
            this.Name = "DccRequests";
            this.gbGet.ResumeLayout(false);
            this.gbGet.PerformLayout();
            this.gbChat.ResumeLayout(false);
            this.gbChat.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbGet;
        private System.Windows.Forms.ComboBox cmbExists;
        private System.Windows.Forms.Label lblExists;
        private System.Windows.Forms.RadioButton rbGetIgnore;
        private System.Windows.Forms.RadioButton rbGetAuto;
        private System.Windows.Forms.RadioButton rbGetShow;
        private System.Windows.Forms.GroupBox gbChat;
        private System.Windows.Forms.RadioButton rbChatIgnore;
        private System.Windows.Forms.RadioButton rbChatAuto;
        private System.Windows.Forms.RadioButton rbChatShow;
    }
}
