namespace FusionIRC.Forms.DirectClientConnection
{
    partial class FrmDccManager
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
            this.olvFiles = new libolv.ObjectListView();
            ((System.ComponentModel.ISupportInitialize)(this.olvFiles)).BeginInit();
            this.SuspendLayout();
            // 
            // olvFiles
            // 
            this.olvFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvFiles.FullRowSelect = true;
            this.olvFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.olvFiles.HideSelection = false;
            this.olvFiles.Location = new System.Drawing.Point(0, 0);
            this.olvFiles.Name = "olvFiles";
            this.olvFiles.OwnerDraw = true;
            this.olvFiles.Size = new System.Drawing.Size(597, 262);
            this.olvFiles.TabIndex = 0;
            this.olvFiles.UseCompatibleStateImageBehavior = false;
            this.olvFiles.View = System.Windows.Forms.View.Details;
            // 
            // FrmDccManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(597, 262);
            this.Controls.Add(this.olvFiles);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(613, 301);
            this.Name = "FrmDccManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FusionIRC - DCC Transfers Manager";
            ((System.ComponentModel.ISupportInitialize)(this.olvFiles)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private libolv.ObjectListView olvFiles;
    }
}