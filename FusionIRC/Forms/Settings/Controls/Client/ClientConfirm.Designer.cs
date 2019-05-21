namespace FusionIRC.Forms.Settings.Controls.Client
{
    partial class ClientConfirm
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
            this.lblConsole = new System.Windows.Forms.Label();
            this.cmbConsole = new System.Windows.Forms.ComboBox();
            this.lblApp = new System.Windows.Forms.Label();
            this.cmbApp = new System.Windows.Forms.ComboBox();
            this.chkUrl = new System.Windows.Forms.CheckBox();
            this.chkPaste = new System.Windows.Forms.CheckBox();
            this.txtPaste = new System.Windows.Forms.TextBox();
            this.lblLines = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblConsole
            // 
            this.lblConsole.AutoSize = true;
            this.lblConsole.Location = new System.Drawing.Point(85, 85);
            this.lblConsole.Name = "lblConsole";
            this.lblConsole.Size = new System.Drawing.Size(225, 15);
            this.lblConsole.TabIndex = 0;
            this.lblConsole.Text = "Confirm when closing a console window:";
            // 
            // cmbConsole
            // 
            this.cmbConsole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbConsole.FormattingEnabled = true;
            this.cmbConsole.Location = new System.Drawing.Point(88, 103);
            this.cmbConsole.Name = "cmbConsole";
            this.cmbConsole.Size = new System.Drawing.Size(121, 23);
            this.cmbConsole.TabIndex = 1;
            // 
            // lblApp
            // 
            this.lblApp.AutoSize = true;
            this.lblApp.Location = new System.Drawing.Point(85, 146);
            this.lblApp.Name = "lblApp";
            this.lblApp.Size = new System.Drawing.Size(189, 15);
            this.lblApp.TabIndex = 2;
            this.lblApp.Text = "Confirm when closing application:";
            // 
            // cmbApp
            // 
            this.cmbApp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbApp.FormattingEnabled = true;
            this.cmbApp.Location = new System.Drawing.Point(88, 164);
            this.cmbApp.Name = "cmbApp";
            this.cmbApp.Size = new System.Drawing.Size(121, 23);
            this.cmbApp.TabIndex = 3;
            // 
            // chkUrl
            // 
            this.chkUrl.AutoSize = true;
            this.chkUrl.Location = new System.Drawing.Point(88, 210);
            this.chkUrl.Name = "chkUrl";
            this.chkUrl.Size = new System.Drawing.Size(259, 19);
            this.chkUrl.TabIndex = 4;
            this.chkUrl.Text = "Show warning dialog when clicking on URLs";
            this.chkUrl.UseVisualStyleBackColor = true;
            // 
            // chkPaste
            // 
            this.chkPaste.AutoSize = true;
            this.chkPaste.Location = new System.Drawing.Point(88, 242);
            this.chkPaste.Name = "chkPaste";
            this.chkPaste.Size = new System.Drawing.Size(144, 19);
            this.chkPaste.TabIndex = 5;
            this.chkPaste.Text = "Confirm when pasting";
            this.chkPaste.UseVisualStyleBackColor = true;
            // 
            // txtPaste
            // 
            this.txtPaste.Location = new System.Drawing.Point(238, 240);
            this.txtPaste.MaxLength = 2;
            this.txtPaste.Name = "txtPaste";
            this.txtPaste.Size = new System.Drawing.Size(29, 23);
            this.txtPaste.TabIndex = 6;
            this.txtPaste.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblLines
            // 
            this.lblLines.AutoSize = true;
            this.lblLines.Location = new System.Drawing.Point(273, 243);
            this.lblLines.Name = "lblLines";
            this.lblLines.Size = new System.Drawing.Size(67, 15);
            this.lblLines.TabIndex = 7;
            this.lblLines.Text = "lines of text";
            // 
            // ClientConfirm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblLines);
            this.Controls.Add(this.txtPaste);
            this.Controls.Add(this.chkPaste);
            this.Controls.Add(this.chkUrl);
            this.Controls.Add(this.cmbApp);
            this.Controls.Add(this.lblApp);
            this.Controls.Add(this.cmbConsole);
            this.Controls.Add(this.lblConsole);
            this.Name = "ClientConfirm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblConsole;
        private System.Windows.Forms.ComboBox cmbConsole;
        private System.Windows.Forms.Label lblApp;
        private System.Windows.Forms.ComboBox cmbApp;
        private System.Windows.Forms.CheckBox chkUrl;
        private System.Windows.Forms.CheckBox chkPaste;
        private System.Windows.Forms.TextBox txtPaste;
        private System.Windows.Forms.Label lblLines;
    }
}
