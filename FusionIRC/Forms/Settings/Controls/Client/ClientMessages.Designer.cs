namespace FusionIRC.Forms.Settings.Controls.Client
{
    partial class ClientMessages
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
            this.chkStrip = new System.Windows.Forms.CheckBox();
            this.lblQuit = new System.Windows.Forms.Label();
            this.txtQuit = new ircCore.Controls.ChildWindows.Input.ColorBox.ColorTextBox();
            this.txtPart = new ircCore.Controls.ChildWindows.Input.ColorBox.ColorTextBox();
            this.lblPart = new System.Windows.Forms.Label();
            this.txtFinger = new ircCore.Controls.ChildWindows.Input.ColorBox.ColorTextBox();
            this.lblFinger = new System.Windows.Forms.Label();
            this.lblCommand = new System.Windows.Forms.Label();
            this.txtCommand = new System.Windows.Forms.TextBox();
            this.lblLine = new System.Windows.Forms.Label();
            this.cmbSpacing = new System.Windows.Forms.ComboBox();
            this.lblPadding = new System.Windows.Forms.Label();
            this.txtPadding = new System.Windows.Forms.TextBox();
            this.lblPixels = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // chkStrip
            // 
            this.chkStrip.AutoSize = true;
            this.chkStrip.Location = new System.Drawing.Point(60, 67);
            this.chkStrip.Name = "chkStrip";
            this.chkStrip.Size = new System.Drawing.Size(230, 19);
            this.chkStrip.TabIndex = 0;
            this.chkStrip.Text = "Strip control codes from incoming text";
            this.chkStrip.UseVisualStyleBackColor = true;
            // 
            // lblQuit
            // 
            this.lblQuit.AutoSize = true;
            this.lblQuit.Location = new System.Drawing.Point(57, 89);
            this.lblQuit.Name = "lblQuit";
            this.lblQuit.Size = new System.Drawing.Size(82, 15);
            this.lblQuit.TabIndex = 1;
            this.lblQuit.Text = "Quit message:";
            // 
            // txtQuit
            // 
            this.txtQuit.AllowMultiLinePaste = false;
            this.txtQuit.IsMultiLinePaste = false;
            this.txtQuit.IsNormalTextbox = false;
            this.txtQuit.Location = new System.Drawing.Point(60, 107);
            this.txtQuit.Name = "txtQuit";
            this.txtQuit.ProcessCodes = true;
            this.txtQuit.Size = new System.Drawing.Size(303, 23);
            this.txtQuit.TabIndex = 2;
            // 
            // txtPart
            // 
            this.txtPart.AllowMultiLinePaste = false;
            this.txtPart.IsMultiLinePaste = false;
            this.txtPart.IsNormalTextbox = false;
            this.txtPart.Location = new System.Drawing.Point(60, 151);
            this.txtPart.Name = "txtPart";
            this.txtPart.ProcessCodes = true;
            this.txtPart.Size = new System.Drawing.Size(303, 23);
            this.txtPart.TabIndex = 4;
            // 
            // lblPart
            // 
            this.lblPart.AutoSize = true;
            this.lblPart.Location = new System.Drawing.Point(57, 133);
            this.lblPart.Name = "lblPart";
            this.lblPart.Size = new System.Drawing.Size(80, 15);
            this.lblPart.TabIndex = 3;
            this.lblPart.Text = "Part message:";
            // 
            // txtFinger
            // 
            this.txtFinger.AllowMultiLinePaste = false;
            this.txtFinger.IsMultiLinePaste = false;
            this.txtFinger.IsNormalTextbox = false;
            this.txtFinger.Location = new System.Drawing.Point(60, 195);
            this.txtFinger.Name = "txtFinger";
            this.txtFinger.ProcessCodes = true;
            this.txtFinger.Size = new System.Drawing.Size(303, 23);
            this.txtFinger.TabIndex = 6;
            // 
            // lblFinger
            // 
            this.lblFinger.AutoSize = true;
            this.lblFinger.Location = new System.Drawing.Point(57, 177);
            this.lblFinger.Name = "lblFinger";
            this.lblFinger.Size = new System.Drawing.Size(72, 15);
            this.lblFinger.TabIndex = 5;
            this.lblFinger.Text = "Finger reply:";
            // 
            // lblCommand
            // 
            this.lblCommand.AutoSize = true;
            this.lblCommand.Location = new System.Drawing.Point(57, 227);
            this.lblCommand.Name = "lblCommand";
            this.lblCommand.Size = new System.Drawing.Size(151, 15);
            this.lblCommand.TabIndex = 7;
            this.lblCommand.Text = "Command character prefix:";
            // 
            // txtCommand
            // 
            this.txtCommand.Location = new System.Drawing.Point(214, 224);
            this.txtCommand.MaxLength = 1;
            this.txtCommand.Name = "txtCommand";
            this.txtCommand.Size = new System.Drawing.Size(29, 23);
            this.txtCommand.TabIndex = 8;
            this.txtCommand.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblLine
            // 
            this.lblLine.AutoSize = true;
            this.lblLine.Location = new System.Drawing.Point(57, 256);
            this.lblLine.Name = "lblLine";
            this.lblLine.Size = new System.Drawing.Size(142, 15);
            this.lblLine.TabIndex = 9;
            this.lblLine.Text = "Window text line spacing:";
            // 
            // cmbSpacing
            // 
            this.cmbSpacing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSpacing.FormattingEnabled = true;
            this.cmbSpacing.Location = new System.Drawing.Point(214, 253);
            this.cmbSpacing.Name = "cmbSpacing";
            this.cmbSpacing.Size = new System.Drawing.Size(149, 23);
            this.cmbSpacing.TabIndex = 10;
            // 
            // lblPadding
            // 
            this.lblPadding.AutoSize = true;
            this.lblPadding.Location = new System.Drawing.Point(57, 285);
            this.lblPadding.Name = "lblPadding";
            this.lblPadding.Size = new System.Drawing.Size(142, 15);
            this.lblPadding.TabIndex = 11;
            this.lblPadding.Text = "Window text line padding";
            // 
            // txtPadding
            // 
            this.txtPadding.Location = new System.Drawing.Point(214, 282);
            this.txtPadding.MaxLength = 1;
            this.txtPadding.Name = "txtPadding";
            this.txtPadding.Size = new System.Drawing.Size(29, 23);
            this.txtPadding.TabIndex = 12;
            this.txtPadding.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblPixels
            // 
            this.lblPixels.AutoSize = true;
            this.lblPixels.Location = new System.Drawing.Point(252, 285);
            this.lblPixels.Name = "lblPixels";
            this.lblPixels.Size = new System.Drawing.Size(36, 15);
            this.lblPixels.TabIndex = 13;
            this.lblPixels.Text = "pixels";
            // 
            // ClientMessages
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblPixels);
            this.Controls.Add(this.txtPadding);
            this.Controls.Add(this.lblPadding);
            this.Controls.Add(this.cmbSpacing);
            this.Controls.Add(this.lblLine);
            this.Controls.Add(this.txtCommand);
            this.Controls.Add(this.lblCommand);
            this.Controls.Add(this.txtFinger);
            this.Controls.Add(this.lblFinger);
            this.Controls.Add(this.txtPart);
            this.Controls.Add(this.lblPart);
            this.Controls.Add(this.txtQuit);
            this.Controls.Add(this.lblQuit);
            this.Controls.Add(this.chkStrip);
            this.Name = "ClientMessages";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkStrip;
        private System.Windows.Forms.Label lblQuit;
        private ircCore.Controls.ChildWindows.Input.ColorBox.ColorTextBox txtQuit;
        private ircCore.Controls.ChildWindows.Input.ColorBox.ColorTextBox txtPart;
        private System.Windows.Forms.Label lblPart;
        private ircCore.Controls.ChildWindows.Input.ColorBox.ColorTextBox txtFinger;
        private System.Windows.Forms.Label lblFinger;
        private System.Windows.Forms.Label lblCommand;
        private System.Windows.Forms.TextBox txtCommand;
        private System.Windows.Forms.Label lblLine;
        private System.Windows.Forms.ComboBox cmbSpacing;
        private System.Windows.Forms.Label lblPadding;
        private System.Windows.Forms.TextBox txtPadding;
        private System.Windows.Forms.Label lblPixels;
    }
}
