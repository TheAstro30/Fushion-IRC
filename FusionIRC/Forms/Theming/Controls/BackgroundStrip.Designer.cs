namespace FusionIRC.Forms.Theming.Controls
{
    partial class BackgroundStrip
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
            this.lblHeader = new System.Windows.Forms.Label();
            this.txtImage = new System.Windows.Forms.TextBox();
            this.btnSelect = new System.Windows.Forms.Button();
            this.pnlPreview = new System.Windows.Forms.Panel();
            this.lbllLayout = new System.Windows.Forms.Label();
            this.cmbLayout = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Location = new System.Drawing.Point(3, 3);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(45, 15);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "Header";
            // 
            // txtImage
            // 
            this.txtImage.Location = new System.Drawing.Point(6, 21);
            this.txtImage.Name = "txtImage";
            this.txtImage.ReadOnly = true;
            this.txtImage.Size = new System.Drawing.Size(256, 23);
            this.txtImage.TabIndex = 1;
            // 
            // btnSelect
            // 
            this.btnSelect.Location = new System.Drawing.Point(268, 21);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(30, 23);
            this.btnSelect.TabIndex = 2;
            this.btnSelect.Text = "...";
            this.btnSelect.UseVisualStyleBackColor = true;
            // 
            // pnlPreview
            // 
            this.pnlPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlPreview.Location = new System.Drawing.Point(304, 3);
            this.pnlPreview.Name = "pnlPreview";
            this.pnlPreview.Size = new System.Drawing.Size(98, 68);
            this.pnlPreview.TabIndex = 3;
            // 
            // lbllLayout
            // 
            this.lbllLayout.AutoSize = true;
            this.lbllLayout.Location = new System.Drawing.Point(3, 51);
            this.lbllLayout.Name = "lbllLayout";
            this.lbllLayout.Size = new System.Drawing.Size(79, 15);
            this.lbllLayout.TabIndex = 4;
            this.lbllLayout.Text = "Image layout:";
            // 
            // cmbLayout
            // 
            this.cmbLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLayout.FormattingEnabled = true;
            this.cmbLayout.Location = new System.Drawing.Point(88, 48);
            this.cmbLayout.Name = "cmbLayout";
            this.cmbLayout.Size = new System.Drawing.Size(174, 23);
            this.cmbLayout.TabIndex = 5;
            // 
            // BackgroundStrip
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.cmbLayout);
            this.Controls.Add(this.lbllLayout);
            this.Controls.Add(this.pnlPreview);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.txtImage);
            this.Controls.Add(this.lblHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximumSize = new System.Drawing.Size(405, 78);
            this.MinimumSize = new System.Drawing.Size(405, 78);
            this.Name = "BackgroundStrip";
            this.Size = new System.Drawing.Size(405, 78);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.TextBox txtImage;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Panel pnlPreview;
        private System.Windows.Forms.Label lbllLayout;
        private System.Windows.Forms.ComboBox cmbLayout;
    }
}
