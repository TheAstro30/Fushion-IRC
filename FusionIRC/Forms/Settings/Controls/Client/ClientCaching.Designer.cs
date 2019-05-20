namespace FusionIRC.Forms.Settings.Controls.Client
{
    partial class ClientCaching
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
            this.lblText = new System.Windows.Forms.Label();
            this.txtText = new System.Windows.Forms.TextBox();
            this.lblLines = new System.Windows.Forms.Label();
            this.lblInput = new System.Windows.Forms.Label();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.lblCommands = new System.Windows.Forms.Label();
            this.lblChat = new System.Windows.Forms.Label();
            this.lblEntries = new System.Windows.Forms.Label();
            this.txtChat = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblText
            // 
            this.lblText.AutoSize = true;
            this.lblText.Location = new System.Drawing.Point(78, 119);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(108, 15);
            this.lblText.TabIndex = 0;
            this.lblText.Text = "Window text buffer";
            // 
            // txtText
            // 
            this.txtText.Location = new System.Drawing.Point(192, 116);
            this.txtText.MaxLength = 4;
            this.txtText.Name = "txtText";
            this.txtText.Size = new System.Drawing.Size(61, 23);
            this.txtText.TabIndex = 1;
            this.txtText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblLines
            // 
            this.lblLines.AutoSize = true;
            this.lblLines.Location = new System.Drawing.Point(259, 119);
            this.lblLines.Name = "lblLines";
            this.lblLines.Size = new System.Drawing.Size(31, 15);
            this.lblLines.TabIndex = 2;
            this.lblLines.Text = "lines";
            // 
            // lblInput
            // 
            this.lblInput.AutoSize = true;
            this.lblInput.Location = new System.Drawing.Point(107, 168);
            this.lblInput.Name = "lblInput";
            this.lblInput.Size = new System.Drawing.Size(79, 15);
            this.lblInput.TabIndex = 3;
            this.lblInput.Text = "Input text box";
            // 
            // txtInput
            // 
            this.txtInput.Location = new System.Drawing.Point(192, 165);
            this.txtInput.MaxLength = 3;
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(61, 23);
            this.txtInput.TabIndex = 4;
            this.txtInput.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblCommands
            // 
            this.lblCommands.AutoSize = true;
            this.lblCommands.Location = new System.Drawing.Point(259, 168);
            this.lblCommands.Name = "lblCommands";
            this.lblCommands.Size = new System.Drawing.Size(67, 15);
            this.lblCommands.TabIndex = 5;
            this.lblCommands.Text = "commands";
            // 
            // lblChat
            // 
            this.lblChat.AutoSize = true;
            this.lblChat.Location = new System.Drawing.Point(91, 216);
            this.lblChat.Name = "lblChat";
            this.lblChat.Size = new System.Drawing.Size(95, 15);
            this.lblChat.TabIndex = 6;
            this.lblChat.Text = "Chat find history";
            // 
            // lblEntries
            // 
            this.lblEntries.AutoSize = true;
            this.lblEntries.Location = new System.Drawing.Point(259, 216);
            this.lblEntries.Name = "lblEntries";
            this.lblEntries.Size = new System.Drawing.Size(42, 15);
            this.lblEntries.TabIndex = 8;
            this.lblEntries.Text = "entries";
            // 
            // txtChat
            // 
            this.txtChat.Location = new System.Drawing.Point(192, 213);
            this.txtChat.MaxLength = 3;
            this.txtChat.Name = "txtChat";
            this.txtChat.Size = new System.Drawing.Size(61, 23);
            this.txtChat.TabIndex = 7;
            this.txtChat.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ClientCaching
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblEntries);
            this.Controls.Add(this.txtChat);
            this.Controls.Add(this.lblChat);
            this.Controls.Add(this.lblCommands);
            this.Controls.Add(this.txtInput);
            this.Controls.Add(this.lblInput);
            this.Controls.Add(this.lblLines);
            this.Controls.Add(this.txtText);
            this.Controls.Add(this.lblText);
            this.Name = "ClientCaching";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblText;
        private System.Windows.Forms.TextBox txtText;
        private System.Windows.Forms.Label lblLines;
        private System.Windows.Forms.Label lblInput;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.Label lblCommands;
        private System.Windows.Forms.Label lblChat;
        private System.Windows.Forms.Label lblEntries;
        private System.Windows.Forms.TextBox txtChat;
    }
}
