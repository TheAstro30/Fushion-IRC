namespace FusionIRC.Forms.Theming.Controls
{
    partial class ThemeColors
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
            this.lstWindowColors = new System.Windows.Forms.ListBox();
            this.lblHeader = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.pnlColor = new System.Windows.Forms.Panel();
            this.lblCurrentColor = new System.Windows.Forms.Label();
            this.lstEventColors = new System.Windows.Forms.ListBox();
            this.lblWindow = new System.Windows.Forms.Label();
            this.lblEvents = new System.Windows.Forms.Label();
            this.colorSelector = new ircCore.Controls.ChildWindows.Input.ColorBox.ColorSelectionBox();
            this.SuspendLayout();
            // 
            // lstWindowColors
            // 
            this.lstWindowColors.FormattingEnabled = true;
            this.lstWindowColors.IntegralHeight = false;
            this.lstWindowColors.ItemHeight = 15;
            this.lstWindowColors.Location = new System.Drawing.Point(15, 58);
            this.lstWindowColors.Name = "lstWindowColors";
            this.lstWindowColors.Size = new System.Drawing.Size(194, 124);
            this.lstWindowColors.TabIndex = 1;
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Location = new System.Drawing.Point(12, 18);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(416, 15);
            this.lblHeader.TabIndex = 2;
            this.lblHeader.Text = "You can change the default colors for each window/message event type here:";
            // 
            // lblInfo
            // 
            this.lblInfo.Location = new System.Drawing.Point(222, 200);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(340, 32);
            this.lblInfo.TabIndex = 3;
            this.lblInfo.Text = "Note: To change the RGB values of the each of the 16 colors above, right-click it" +
                "s associated box";
            // 
            // pnlColor
            // 
            this.pnlColor.BackColor = System.Drawing.SystemColors.Control;
            this.pnlColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlColor.Location = new System.Drawing.Point(307, 120);
            this.pnlColor.Name = "pnlColor";
            this.pnlColor.Size = new System.Drawing.Size(24, 24);
            this.pnlColor.TabIndex = 4;
            // 
            // lblCurrentColor
            // 
            this.lblCurrentColor.AutoSize = true;
            this.lblCurrentColor.Location = new System.Drawing.Point(222, 123);
            this.lblCurrentColor.Name = "lblCurrentColor";
            this.lblCurrentColor.Size = new System.Drawing.Size(80, 15);
            this.lblCurrentColor.TabIndex = 5;
            this.lblCurrentColor.Text = "Current color:";
            // 
            // lstEventColors
            // 
            this.lstEventColors.FormattingEnabled = true;
            this.lstEventColors.IntegralHeight = false;
            this.lstEventColors.ItemHeight = 15;
            this.lstEventColors.Location = new System.Drawing.Point(15, 203);
            this.lstEventColors.Name = "lstEventColors";
            this.lstEventColors.Size = new System.Drawing.Size(194, 194);
            this.lstEventColors.TabIndex = 6;
            // 
            // lblWindow
            // 
            this.lblWindow.AutoSize = true;
            this.lblWindow.Location = new System.Drawing.Point(12, 40);
            this.lblWindow.Name = "lblWindow";
            this.lblWindow.Size = new System.Drawing.Size(89, 15);
            this.lblWindow.TabIndex = 7;
            this.lblWindow.Text = "Window colors:";
            // 
            // lblEvents
            // 
            this.lblEvents.AutoSize = true;
            this.lblEvents.Location = new System.Drawing.Point(12, 185);
            this.lblEvents.Name = "lblEvents";
            this.lblEvents.Size = new System.Drawing.Size(115, 15);
            this.lblEvents.TabIndex = 8;
            this.lblEvents.Text = "Default event colors:";
            // 
            // colorSelector
            // 
            this.colorSelector.Location = new System.Drawing.Point(224, 161);
            this.colorSelector.Name = "colorSelector";
            this.colorSelector.SelectedColor = 0;
            this.colorSelector.ShowFocusRectangle = true;
            this.colorSelector.Size = new System.Drawing.Size(337, 22);
            this.colorSelector.TabIndex = 0;
            this.colorSelector.Text = "colorSelectionBox1";
            // 
            // ThemeColors
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.lblEvents);
            this.Controls.Add(this.lblWindow);
            this.Controls.Add(this.lstEventColors);
            this.Controls.Add(this.lblCurrentColor);
            this.Controls.Add(this.pnlColor);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.lstWindowColors);
            this.Controls.Add(this.colorSelector);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ThemeColors";
            this.Size = new System.Drawing.Size(577, 416);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ircCore.Controls.ChildWindows.Input.ColorBox.ColorSelectionBox colorSelector;
        private System.Windows.Forms.ListBox lstWindowColors;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Panel pnlColor;
        private System.Windows.Forms.Label lblCurrentColor;
        private System.Windows.Forms.ListBox lstEventColors;
        private System.Windows.Forms.Label lblWindow;
        private System.Windows.Forms.Label lblEvents;
    }
}
