namespace FusionIRC.Forms
{
    partial class FrmClientWindow
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
            this.toolBar = new System.Windows.Forms.ToolStrip();
            this.SwitchViewSplitter = new System.Windows.Forms.Splitter();
            this.switchTree = new FusionIRC.Controls.SwitchView.WindowTreeView();
            this.SuspendLayout();
            // 
            // toolBar
            // 
            this.toolBar.AutoSize = false;
            this.toolBar.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolBar.Location = new System.Drawing.Point(0, 0);
            this.toolBar.Name = "toolBar";
            this.toolBar.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolBar.Size = new System.Drawing.Size(953, 33);
            this.toolBar.TabIndex = 2;
            this.toolBar.Text = "toolStrip1";
            // 
            // SwitchViewSplitter
            // 
            this.SwitchViewSplitter.Location = new System.Drawing.Point(160, 33);
            this.SwitchViewSplitter.MinExtra = 60;
            this.SwitchViewSplitter.MinSize = 80;
            this.SwitchViewSplitter.Name = "SwitchViewSplitter";
            this.SwitchViewSplitter.Size = new System.Drawing.Size(1, 521);
            this.SwitchViewSplitter.TabIndex = 4;
            this.SwitchViewSplitter.TabStop = false;
            // 
            // switchTree
            // 
            this.switchTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.switchTree.Dock = System.Windows.Forms.DockStyle.Left;
            this.switchTree.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.switchTree.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.switchTree.HideSelection = false;
            this.switchTree.Location = new System.Drawing.Point(0, 33);
            this.switchTree.Name = "switchTree";
            this.switchTree.ShowPlusMinus = false;
            this.switchTree.ShowRootLines = false;
            this.switchTree.Size = new System.Drawing.Size(160, 521);
            this.switchTree.TabIndex = 0;
            // 
            // FrmClientWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(953, 554);
            this.Controls.Add(this.SwitchViewSplitter);
            this.Controls.Add(this.switchTree);
            this.Controls.Add(this.toolBar);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IsMdiContainer = true;
            this.Name = "FrmClientWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FusionIRC";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolBar;
        public Controls.SwitchView.WindowTreeView switchTree;
        private System.Windows.Forms.Splitter SwitchViewSplitter;
        



    }
}

