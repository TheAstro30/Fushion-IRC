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
            this.txtEdit = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtEdit
            // 
            this.txtEdit.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEdit.Location = new System.Drawing.Point(12, 12);
            this.txtEdit.Multiline = true;
            this.txtEdit.Name = "txtEdit";
            this.txtEdit.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtEdit.Size = new System.Drawing.Size(326, 291);
            this.txtEdit.TabIndex = 0;
            this.txtEdit.WordWrap = false;
            // 
            // FrmScript
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 356);
            this.Controls.Add(this.txtEdit);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(366, 395);
            this.Name = "FrmScript";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FusionIRC - Alias Editor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtEdit;
    }
}