using ircCore.Controls.ChildWindows.OutputDisplay;
using ircCore.Controls.ChildWindows.OutputDisplay.Helpers;

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
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.outputWindow1 = new OutputWindow();
            this.inputBox1 = new ircCore.Controls.ChildWindows.Input.InputWindow();
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
            this.outputWindow1.BackgroundImageLayout = BackgroundImageLayoutStyles.Tile;
            this.outputWindow1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.outputWindow1.IndentWidth = 16;
            this.outputWindow1.LineMarkerColor = System.Drawing.Color.Red;
            this.outputWindow1.LineSpacingStyle = LineSpacingStyle.Single;
            this.outputWindow1.Location = new System.Drawing.Point(0, 0);
            this.outputWindow1.MaximumLines = 500;
            this.outputWindow1.Name = "outputWindow1";
            this.outputWindow1.ShowLineMarker = true;
            this.outputWindow1.Size = new System.Drawing.Size(431, 303);
            this.outputWindow1.TabIndex = 0;
            this.outputWindow1.OnUrlDoubleClicked += new System.Action<string>(this.outputWindow1_OnUrlDoubleClicked);
            this.outputWindow1.OnWindowDoubleClicked += new System.Action(this.outputWindow1_OnWindowDoubleClicked);
            this.outputWindow1.OnWindowRightClicked += new System.Action(this.outputWindow1_OnWindowRightClicked);
            // 
            // inputBox1
            // 
            this.inputBox1.BackColor = System.Drawing.SystemColors.Window;
            this.inputBox1.Location = new System.Drawing.Point(0, 327);
            this.inputBox1.Name = "inputBox1";
            this.inputBox1.SelectionLength = 0;
            this.inputBox1.SelectionStart = 0;
            this.inputBox1.Size = new System.Drawing.Size(651, 19);
            this.inputBox1.TabIndex = 1;
            // 
            // FrmTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(651, 346);
            this.Controls.Add(this.inputBox1);
            this.Controls.Add(this.outputWindow1);
            this.MinimumSize = new System.Drawing.Size(100, 100);
            this.Name = "FrmTest";
            this.ShowInTaskbar = false;
            this.Text = "FrmTest";
            this.Resize += new System.EventHandler(this.FrmTest_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private ircCore.Controls.ChildWindows.Input.InputWindow inputBox1;
        private OutputWindow outputWindow1;
        private System.Windows.Forms.Timer timer1;
        
    }
}