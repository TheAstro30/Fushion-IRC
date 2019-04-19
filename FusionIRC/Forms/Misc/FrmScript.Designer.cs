namespace FusionIRC.Forms.Misc
{
    partial class FrmScript
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmScript));
            this.btnClose = new System.Windows.Forms.Button();
            this.tbLayout = new System.Windows.Forms.TableLayoutPanel();
            this.menu = new System.Windows.Forms.MenuStrip();
            this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.tbButtons = new System.Windows.Forms.TableLayoutPanel();
            this.txtEdit = new ircScript.Controls.ScriptEditor();
            this.tbLayout.SuspendLayout();
            this.menu.SuspendLayout();
            this.tbButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(119, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // tbLayout
            // 
            this.tbLayout.ColumnCount = 1;
            this.tbLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tbLayout.Controls.Add(this.txtEdit, 0, 0);
            this.tbLayout.Controls.Add(this.tbButtons, 0, 1);
            this.tbLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbLayout.Location = new System.Drawing.Point(0, 24);
            this.tbLayout.Name = "tbLayout";
            this.tbLayout.RowCount = 2;
            this.tbLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tbLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tbLayout.Size = new System.Drawing.Size(350, 332);
            this.tbLayout.TabIndex = 2;
            // 
            // menu
            // 
            this.menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.mnuEdit});
            this.menu.Location = new System.Drawing.Point(0, 0);
            this.menu.Name = "menu";
            this.menu.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.menu.Size = new System.Drawing.Size(350, 24);
            this.menu.TabIndex = 3;
            this.menu.Text = "_menu";
            // 
            // mnuFile
            // 
            this.mnuFile.Name = "mnuFile";
            this.mnuFile.Size = new System.Drawing.Size(37, 20);
            this.mnuFile.Text = "&File";
            // 
            // mnuEdit
            // 
            this.mnuEdit.Name = "mnuEdit";
            this.mnuEdit.Size = new System.Drawing.Size(39, 20);
            this.mnuEdit.Text = "&Edit";
            // 
            // tbButtons
            // 
            this.tbButtons.ColumnCount = 2;
            this.tbButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58.24742F));
            this.tbButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 41.75258F));
            this.tbButtons.Controls.Add(this.btnClose, 1, 0);
            this.tbButtons.Dock = System.Windows.Forms.DockStyle.Right;
            this.tbButtons.Location = new System.Drawing.Point(147, 299);
            this.tbButtons.Name = "tbButtons";
            this.tbButtons.RowCount = 1;
            this.tbButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tbButtons.Size = new System.Drawing.Size(200, 30);
            this.tbButtons.TabIndex = 1;
            // 
            // txtEdit
            // 
            this.txtEdit.BackColor = System.Drawing.SystemColors.Window;
            this.txtEdit.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.txtEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEdit.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEdit.Lines = new string[0];
            this.txtEdit.Location = new System.Drawing.Point(3, 3);
            this.txtEdit.Name = "txtEdit";
            this.txtEdit.Size = new System.Drawing.Size(344, 290);
            this.txtEdit.TabIndex = 0;
            // 
            // FrmScript
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 356);
            this.Controls.Add(this.tbLayout);
            this.Controls.Add(this.menu);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menu;
            this.MinimumSize = new System.Drawing.Size(366, 395);
            this.Name = "FrmScript";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FusionIRC - Script Editor";
            this.tbLayout.ResumeLayout(false);
            this.menu.ResumeLayout(false);
            this.menu.PerformLayout();
            this.tbButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ircScript.Controls.ScriptEditor txtEdit;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.TableLayoutPanel tbLayout;
        private System.Windows.Forms.TableLayoutPanel tbButtons;
        private System.Windows.Forms.MenuStrip menu;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem mnuEdit;


    }
}