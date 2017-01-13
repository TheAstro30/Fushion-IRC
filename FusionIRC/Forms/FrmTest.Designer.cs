using ircCore.Controls.ChildWindows.IrcWindow;
using ircCore.Controls.ChildWindows.IrcWindow.Helpers;

namespace FusionIRC.Forms
{
    partial class FrmTest
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmTest));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.outputWindow1 = new ircCore.Controls.ChildWindows.IrcWindow.OutputWindow();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Interval = 2000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // outputWindow1
            // 
            this.outputWindow1.AutoScroll = true;
            this.outputWindow1.AutoScrollMinSize = new System.Drawing.Size(0, 1);
            this.outputWindow1.BackColor = System.Drawing.SystemColors.Window;
            this.outputWindow1.BackgroundImage = null;
            this.outputWindow1.BackgroundImageLayout = ircCore.Controls.ChildWindows.IrcWindow.Helpers.BackgroundImageLayoutStyles.Tile;            
            this.outputWindow1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputWindow1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.outputWindow1.IndentWidth = 16;
            this.outputWindow1.LineMarkerColor = System.Drawing.Color.Red;
            this.outputWindow1.LineSpacingStyle = ircCore.Controls.ChildWindows.IrcWindow.Helpers.LineSpacingStyle.Single;
            this.outputWindow1.Location = new System.Drawing.Point(0, 0);
            this.outputWindow1.MaximumLines = 500;
            this.outputWindow1.Name = "outputWindow1";
            this.outputWindow1.ShowLineMarker = true;
            this.outputWindow1.Size = new System.Drawing.Size(651, 346);
            this.outputWindow1.TabIndex = 0;
            this.outputWindow1.OnUrlDoubleClicked += new System.Action<string>(this.outputWindow1_OnUrlDoubleClicked);
            this.outputWindow1.OnWindowDoubleClicked += new System.Action(this.outputWindow1_OnWindowDoubleClicked);
            this.outputWindow1.OnWindowRightClicked += new System.Action(this.outputWindow1_OnWindowRightClicked);
            // 
            // FrmTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(651, 346);
            this.Controls.Add(this.outputWindow1);
            this.MinimumSize = new System.Drawing.Size(100, 100);
            this.Name = "FrmTest";
            this.ShowInTaskbar = false;
            this.Text = "FrmTest";
            this.ResumeLayout(false);

        }

        #endregion

        private OutputWindow outputWindow1;
        private System.Windows.Forms.Timer timer1;
    }
}