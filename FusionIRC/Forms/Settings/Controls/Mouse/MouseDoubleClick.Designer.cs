namespace FusionIRC.Forms.Settings.Controls.Mouse
{
    partial class MouseDoubleClick
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
            this.lblInfo = new System.Windows.Forms.Label();
            this.lblConsole = new System.Windows.Forms.Label();
            this.txtConsole = new System.Windows.Forms.TextBox();
            this.txtChannel = new System.Windows.Forms.TextBox();
            this.lblChannel = new System.Windows.Forms.Label();
            this.txtQuery = new System.Windows.Forms.TextBox();
            this.lblQuery = new System.Windows.Forms.Label();
            this.txtNicklist = new System.Windows.Forms.TextBox();
            this.lblNicklist = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblInfo
            // 
            this.lblInfo.Location = new System.Drawing.Point(77, 92);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(290, 35);
            this.lblInfo.TabIndex = 0;
            this.lblInfo.Text = "FusionIRC can perform the following commands on double-click of a particular wind" +
                "ow:";
            // 
            // lblConsole
            // 
            this.lblConsole.AutoSize = true;
            this.lblConsole.Location = new System.Drawing.Point(65, 149);
            this.lblConsole.Name = "lblConsole";
            this.lblConsole.Size = new System.Drawing.Size(53, 15);
            this.lblConsole.TabIndex = 1;
            this.lblConsole.Text = "Console:";
            // 
            // txtConsole
            // 
            this.txtConsole.Location = new System.Drawing.Point(124, 146);
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.Size = new System.Drawing.Size(240, 23);
            this.txtConsole.TabIndex = 2;
            // 
            // txtChannel
            // 
            this.txtChannel.Location = new System.Drawing.Point(124, 175);
            this.txtChannel.Name = "txtChannel";
            this.txtChannel.Size = new System.Drawing.Size(240, 23);
            this.txtChannel.TabIndex = 4;
            // 
            // lblChannel
            // 
            this.lblChannel.AutoSize = true;
            this.lblChannel.Location = new System.Drawing.Point(65, 178);
            this.lblChannel.Name = "lblChannel";
            this.lblChannel.Size = new System.Drawing.Size(54, 15);
            this.lblChannel.TabIndex = 3;
            this.lblChannel.Text = "Channel:";
            // 
            // txtQuery
            // 
            this.txtQuery.Location = new System.Drawing.Point(124, 204);
            this.txtQuery.Name = "txtQuery";
            this.txtQuery.Size = new System.Drawing.Size(240, 23);
            this.txtQuery.TabIndex = 6;
            // 
            // lblQuery
            // 
            this.lblQuery.AutoSize = true;
            this.lblQuery.Location = new System.Drawing.Point(77, 207);
            this.lblQuery.Name = "lblQuery";
            this.lblQuery.Size = new System.Drawing.Size(42, 15);
            this.lblQuery.TabIndex = 5;
            this.lblQuery.Text = "Query:";
            // 
            // txtNicklist
            // 
            this.txtNicklist.Location = new System.Drawing.Point(124, 233);
            this.txtNicklist.Name = "txtNicklist";
            this.txtNicklist.Size = new System.Drawing.Size(240, 23);
            this.txtNicklist.TabIndex = 8;
            // 
            // lblNicklist
            // 
            this.lblNicklist.AutoSize = true;
            this.lblNicklist.Location = new System.Drawing.Point(70, 236);
            this.lblNicklist.Name = "lblNicklist";
            this.lblNicklist.Size = new System.Drawing.Size(49, 15);
            this.lblNicklist.TabIndex = 7;
            this.lblNicklist.Text = "Nicklist:";
            // 
            // MouseDoubleClick
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtNicklist);
            this.Controls.Add(this.lblNicklist);
            this.Controls.Add(this.txtQuery);
            this.Controls.Add(this.lblQuery);
            this.Controls.Add(this.txtChannel);
            this.Controls.Add(this.lblChannel);
            this.Controls.Add(this.txtConsole);
            this.Controls.Add(this.lblConsole);
            this.Controls.Add(this.lblInfo);
            this.Name = "MouseDoubleClick";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Label lblConsole;
        private System.Windows.Forms.TextBox txtConsole;
        private System.Windows.Forms.TextBox txtChannel;
        private System.Windows.Forms.Label lblChannel;
        private System.Windows.Forms.TextBox txtQuery;
        private System.Windows.Forms.Label lblQuery;
        private System.Windows.Forms.TextBox txtNicklist;
        private System.Windows.Forms.Label lblNicklist;
    }
}
