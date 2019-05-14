namespace FusionIRC.Forms.Theming.Controls
{
    partial class ThemeBackgrounds
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
            this.bsConsole = new FusionIRC.Forms.Theming.Controls.BackgroundStrip();
            this.bsChannel = new FusionIRC.Forms.Theming.Controls.BackgroundStrip();
            this.bsPrivate = new FusionIRC.Forms.Theming.Controls.BackgroundStrip();
            this.bsDcc = new FusionIRC.Forms.Theming.Controls.BackgroundStrip();
            this.SuspendLayout();
            // 
            // bsConsole
            // 
            this.bsConsole.BackColor = System.Drawing.Color.Transparent;
            this.bsConsole.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bsConsole.Header = "Console:";
            this.bsConsole.LayoutStyle = ircCore.Controls.ChildWindows.OutputDisplay.Helpers.BackgroundImageLayoutStyles.None;
            this.bsConsole.Location = new System.Drawing.Point(18, 3);
            this.bsConsole.MaximumSize = new System.Drawing.Size(405, 78);
            this.bsConsole.MinimumSize = new System.Drawing.Size(405, 78);
            this.bsConsole.Name = "bsConsole";
            this.bsConsole.SelectedImage = null;
            this.bsConsole.Size = new System.Drawing.Size(405, 78);
            this.bsConsole.TabIndex = 0;
            // 
            // bsChannel
            // 
            this.bsChannel.BackColor = System.Drawing.Color.Transparent;
            this.bsChannel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bsChannel.Header = "Channel windows:";
            this.bsChannel.LayoutStyle = ircCore.Controls.ChildWindows.OutputDisplay.Helpers.BackgroundImageLayoutStyles.None;
            this.bsChannel.Location = new System.Drawing.Point(18, 87);
            this.bsChannel.MaximumSize = new System.Drawing.Size(405, 78);
            this.bsChannel.MinimumSize = new System.Drawing.Size(405, 78);
            this.bsChannel.Name = "bsChannel";
            this.bsChannel.SelectedImage = null;
            this.bsChannel.Size = new System.Drawing.Size(405, 78);
            this.bsChannel.TabIndex = 1;
            // 
            // bsPrivate
            // 
            this.bsPrivate.BackColor = System.Drawing.Color.Transparent;
            this.bsPrivate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bsPrivate.Header = "Private chat windows:";
            this.bsPrivate.LayoutStyle = ircCore.Controls.ChildWindows.OutputDisplay.Helpers.BackgroundImageLayoutStyles.None;
            this.bsPrivate.Location = new System.Drawing.Point(18, 171);
            this.bsPrivate.MaximumSize = new System.Drawing.Size(405, 78);
            this.bsPrivate.MinimumSize = new System.Drawing.Size(405, 78);
            this.bsPrivate.Name = "bsPrivate";
            this.bsPrivate.SelectedImage = null;
            this.bsPrivate.Size = new System.Drawing.Size(405, 78);
            this.bsPrivate.TabIndex = 2;
            // 
            // bsDcc
            // 
            this.bsDcc.BackColor = System.Drawing.Color.Transparent;
            this.bsDcc.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bsDcc.Header = "DCC chat windows:";
            this.bsDcc.LayoutStyle = ircCore.Controls.ChildWindows.OutputDisplay.Helpers.BackgroundImageLayoutStyles.None;
            this.bsDcc.Location = new System.Drawing.Point(18, 255);
            this.bsDcc.MaximumSize = new System.Drawing.Size(405, 78);
            this.bsDcc.MinimumSize = new System.Drawing.Size(405, 78);
            this.bsDcc.Name = "bsDcc";
            this.bsDcc.SelectedImage = null;
            this.bsDcc.Size = new System.Drawing.Size(405, 78);
            this.bsDcc.TabIndex = 3;
            // 
            // ThemeBackgrounds
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.bsDcc);
            this.Controls.Add(this.bsPrivate);
            this.Controls.Add(this.bsChannel);
            this.Controls.Add(this.bsConsole);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ThemeBackgrounds";
            this.Size = new System.Drawing.Size(438, 360);
            this.ResumeLayout(false);

        }

        #endregion

        private BackgroundStrip bsConsole;
        private BackgroundStrip bsChannel;
        private BackgroundStrip bsPrivate;
        private BackgroundStrip bsDcc;
    }
}
