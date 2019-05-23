namespace FusionIRC.Forms.ChannelProperties
{
    sealed partial class FrmChannelProperties
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
            this.btnClose = new System.Windows.Forms.Button();
            this.lblTopic = new System.Windows.Forms.Label();
            this.txtTopic = new ircCore.Controls.ChildWindows.Input.ColorBox.ColorTextBox();
            this.gbModes = new System.Windows.Forms.GroupBox();
            this.txtKey = new System.Windows.Forms.TextBox();
            this.lblUsers = new System.Windows.Forms.Label();
            this.txtLimit = new System.Windows.Forms.TextBox();
            this.chkSecret = new System.Windows.Forms.CheckBox();
            this.chkPrivate = new System.Windows.Forms.CheckBox();
            this.chkKey = new System.Windows.Forms.CheckBox();
            this.chkLimit = new System.Windows.Forms.CheckBox();
            this.chkModerated = new System.Windows.Forms.CheckBox();
            this.chkInvite = new System.Windows.Forms.CheckBox();
            this.chkMessages = new System.Windows.Forms.CheckBox();
            this.chkTopic = new System.Windows.Forms.CheckBox();
            this.tabUsers = new System.Windows.Forms.TabControl();
            this.tabBans = new System.Windows.Forms.TabPage();
            this.tabExcepts = new System.Windows.Forms.TabPage();
            this.tabInvites = new System.Windows.Forms.TabPage();
            this.btnOk = new System.Windows.Forms.Button();
            this.gbModes.SuspendLayout();
            this.tabUsers.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(295, 384);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // lblTopic
            // 
            this.lblTopic.AutoSize = true;
            this.lblTopic.BackColor = System.Drawing.Color.Transparent;
            this.lblTopic.Location = new System.Drawing.Point(9, 9);
            this.lblTopic.Name = "lblTopic";
            this.lblTopic.Size = new System.Drawing.Size(39, 15);
            this.lblTopic.TabIndex = 1;
            this.lblTopic.Text = "Topic:";
            // 
            // txtTopic
            // 
            this.txtTopic.AllowMultiLinePaste = false;
            this.txtTopic.ConfirmPaste = false;
            this.txtTopic.ConfirmPasteLines = 0;
            this.txtTopic.IsMultiLinePaste = false;
            this.txtTopic.IsNormalTextbox = false;
            this.txtTopic.Location = new System.Drawing.Point(12, 27);
            this.txtTopic.Name = "txtTopic";
            this.txtTopic.ProcessCodes = true;
            this.txtTopic.Size = new System.Drawing.Size(358, 23);
            this.txtTopic.TabIndex = 2;
            // 
            // gbModes
            // 
            this.gbModes.BackColor = System.Drawing.Color.Transparent;
            this.gbModes.Controls.Add(this.txtKey);
            this.gbModes.Controls.Add(this.lblUsers);
            this.gbModes.Controls.Add(this.txtLimit);
            this.gbModes.Controls.Add(this.chkSecret);
            this.gbModes.Controls.Add(this.chkPrivate);
            this.gbModes.Controls.Add(this.chkKey);
            this.gbModes.Controls.Add(this.chkLimit);
            this.gbModes.Controls.Add(this.chkModerated);
            this.gbModes.Controls.Add(this.chkInvite);
            this.gbModes.Controls.Add(this.chkMessages);
            this.gbModes.Controls.Add(this.chkTopic);
            this.gbModes.Location = new System.Drawing.Point(12, 56);
            this.gbModes.Name = "gbModes";
            this.gbModes.Size = new System.Drawing.Size(358, 123);
            this.gbModes.TabIndex = 3;
            this.gbModes.TabStop = false;
            this.gbModes.Text = "Modes:";
            // 
            // txtKey
            // 
            this.txtKey.Location = new System.Drawing.Point(274, 45);
            this.txtKey.Name = "txtKey";
            this.txtKey.Size = new System.Drawing.Size(77, 23);
            this.txtKey.TabIndex = 10;
            // 
            // lblUsers
            // 
            this.lblUsers.AutoSize = true;
            this.lblUsers.Location = new System.Drawing.Point(317, 23);
            this.lblUsers.Name = "lblUsers";
            this.lblUsers.Size = new System.Drawing.Size(34, 15);
            this.lblUsers.TabIndex = 9;
            this.lblUsers.Text = "users";
            // 
            // txtLimit
            // 
            this.txtLimit.Location = new System.Drawing.Point(274, 20);
            this.txtLimit.Name = "txtLimit";
            this.txtLimit.Size = new System.Drawing.Size(37, 23);
            this.txtLimit.TabIndex = 8;
            this.txtLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // chkSecret
            // 
            this.chkSecret.AutoSize = true;
            this.chkSecret.Location = new System.Drawing.Point(179, 97);
            this.chkSecret.Name = "chkSecret";
            this.chkSecret.Size = new System.Drawing.Size(82, 19);
            this.chkSecret.TabIndex = 7;
            this.chkSecret.Text = "Secret (+s)";
            this.chkSecret.UseVisualStyleBackColor = true;
            // 
            // chkPrivate
            // 
            this.chkPrivate.AutoSize = true;
            this.chkPrivate.Location = new System.Drawing.Point(179, 72);
            this.chkPrivate.Name = "chkPrivate";
            this.chkPrivate.Size = new System.Drawing.Size(88, 19);
            this.chkPrivate.TabIndex = 6;
            this.chkPrivate.Text = "Private (+p)";
            this.chkPrivate.UseVisualStyleBackColor = true;
            // 
            // chkKey
            // 
            this.chkKey.AutoSize = true;
            this.chkKey.Location = new System.Drawing.Point(179, 47);
            this.chkKey.Name = "chkKey";
            this.chkKey.Size = new System.Drawing.Size(88, 19);
            this.chkKey.TabIndex = 5;
            this.chkKey.Text = "Key set (+k)";
            this.chkKey.UseVisualStyleBackColor = true;
            // 
            // chkLimit
            // 
            this.chkLimit.AutoSize = true;
            this.chkLimit.Location = new System.Drawing.Point(179, 22);
            this.chkLimit.Name = "chkLimit";
            this.chkLimit.Size = new System.Drawing.Size(89, 19);
            this.chkLimit.TabIndex = 4;
            this.chkLimit.Text = "Limit to (+l)";
            this.chkLimit.UseVisualStyleBackColor = true;
            // 
            // chkModerated
            // 
            this.chkModerated.AutoSize = true;
            this.chkModerated.Location = new System.Drawing.Point(6, 97);
            this.chkModerated.Name = "chkModerated";
            this.chkModerated.Size = new System.Drawing.Size(114, 19);
            this.chkModerated.TabIndex = 3;
            this.chkModerated.Text = "Moderated (+m)";
            this.chkModerated.UseVisualStyleBackColor = true;
            // 
            // chkInvite
            // 
            this.chkInvite.AutoSize = true;
            this.chkInvite.Location = new System.Drawing.Point(6, 72);
            this.chkInvite.Name = "chkInvite";
            this.chkInvite.Size = new System.Drawing.Size(124, 19);
            this.chkInvite.TabIndex = 2;
            this.chkInvite.Text = "Invitation only (+i)";
            this.chkInvite.UseVisualStyleBackColor = true;
            // 
            // chkMessages
            // 
            this.chkMessages.AutoSize = true;
            this.chkMessages.Location = new System.Drawing.Point(6, 47);
            this.chkMessages.Name = "chkMessages";
            this.chkMessages.Size = new System.Drawing.Size(166, 19);
            this.chkMessages.TabIndex = 1;
            this.chkMessages.Text = "No external messages (+n)";
            this.chkMessages.UseVisualStyleBackColor = true;
            // 
            // chkTopic
            // 
            this.chkTopic.AutoSize = true;
            this.chkTopic.Location = new System.Drawing.Point(6, 22);
            this.chkTopic.Name = "chkTopic";
            this.chkTopic.Size = new System.Drawing.Size(147, 19);
            this.chkTopic.TabIndex = 0;
            this.chkTopic.Text = "Only OPS set topic (+t)";
            this.chkTopic.UseVisualStyleBackColor = true;
            // 
            // tabUsers
            // 
            this.tabUsers.Controls.Add(this.tabBans);
            this.tabUsers.Controls.Add(this.tabExcepts);
            this.tabUsers.Controls.Add(this.tabInvites);
            this.tabUsers.Location = new System.Drawing.Point(12, 185);
            this.tabUsers.Name = "tabUsers";
            this.tabUsers.SelectedIndex = 0;
            this.tabUsers.Size = new System.Drawing.Size(358, 193);
            this.tabUsers.TabIndex = 4;
            // 
            // tabBans
            // 
            this.tabBans.Location = new System.Drawing.Point(4, 24);
            this.tabBans.Name = "tabBans";
            this.tabBans.Padding = new System.Windows.Forms.Padding(3);
            this.tabBans.Size = new System.Drawing.Size(350, 165);
            this.tabBans.TabIndex = 0;
            this.tabBans.Text = "Bans:";
            this.tabBans.UseVisualStyleBackColor = true;
            // 
            // tabExcepts
            // 
            this.tabExcepts.Location = new System.Drawing.Point(4, 24);
            this.tabExcepts.Name = "tabExcepts";
            this.tabExcepts.Padding = new System.Windows.Forms.Padding(3);
            this.tabExcepts.Size = new System.Drawing.Size(350, 165);
            this.tabExcepts.TabIndex = 1;
            this.tabExcepts.Text = "Excepts:";
            this.tabExcepts.UseVisualStyleBackColor = true;
            // 
            // tabInvites
            // 
            this.tabInvites.Location = new System.Drawing.Point(4, 24);
            this.tabInvites.Name = "tabInvites";
            this.tabInvites.Padding = new System.Windows.Forms.Padding(3);
            this.tabInvites.Size = new System.Drawing.Size(350, 165);
            this.tabInvites.TabIndex = 2;
            this.tabInvites.Text = "Invites:";
            this.tabInvites.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(214, 384);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 5;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // FrmChannelProperties
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 419);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.tabUsers);
            this.Controls.Add(this.gbModes);
            this.Controls.Add(this.txtTopic);
            this.Controls.Add(this.lblTopic);
            this.Controls.Add(this.btnClose);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmChannelProperties";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Channel properties:";
            this.gbModes.ResumeLayout(false);
            this.gbModes.PerformLayout();
            this.tabUsers.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblTopic;
        private ircCore.Controls.ChildWindows.Input.ColorBox.ColorTextBox txtTopic;
        private System.Windows.Forms.GroupBox gbModes;
        private System.Windows.Forms.TextBox txtKey;
        private System.Windows.Forms.Label lblUsers;
        private System.Windows.Forms.TextBox txtLimit;
        private System.Windows.Forms.CheckBox chkSecret;
        private System.Windows.Forms.CheckBox chkPrivate;
        private System.Windows.Forms.CheckBox chkKey;
        private System.Windows.Forms.CheckBox chkLimit;
        private System.Windows.Forms.CheckBox chkModerated;
        private System.Windows.Forms.CheckBox chkInvite;
        private System.Windows.Forms.CheckBox chkMessages;
        private System.Windows.Forms.CheckBox chkTopic;
        private System.Windows.Forms.TabControl tabUsers;
        private System.Windows.Forms.TabPage tabBans;
        private System.Windows.Forms.TabPage tabExcepts;
        private System.Windows.Forms.TabPage tabInvites;
        private System.Windows.Forms.Button btnOk;
    }
}