namespace FusionIRC.Forms.Misc
{
    partial class FrmChannelList
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
            this.lblList = new System.Windows.Forms.Label();
            this.lblMatch = new System.Windows.Forms.Label();
            this.cmbMatch = new System.Windows.Forms.ComboBox();
            this.btnClearMatch = new System.Windows.Forms.Button();
            this.lblNumber = new System.Windows.Forms.Label();
            this.lblMinimum = new System.Windows.Forms.Label();
            this.txtMinimum = new System.Windows.Forms.TextBox();
            this.txtMaximum = new System.Windows.Forms.TextBox();
            this.lblMaximum = new System.Windows.Forms.Label();
            this.btnCache = new System.Windows.Forms.Button();
            this.btnList = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblCache = new System.Windows.Forms.Label();
            this.cmbCache = new System.Windows.Forms.ComboBox();
            this.btnClearCache = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblList
            // 
            this.lblList.AutoSize = true;
            this.lblList.BackColor = System.Drawing.Color.Transparent;
            this.lblList.Location = new System.Drawing.Point(12, 9);
            this.lblList.Name = "lblList";
            this.lblList.Size = new System.Drawing.Size(205, 15);
            this.lblList.TabIndex = 0;
            this.lblList.Text = "List channels matching the following:";
            // 
            // lblMatch
            // 
            this.lblMatch.AutoSize = true;
            this.lblMatch.BackColor = System.Drawing.Color.Transparent;
            this.lblMatch.Location = new System.Drawing.Point(12, 38);
            this.lblMatch.Name = "lblMatch";
            this.lblMatch.Size = new System.Drawing.Size(66, 15);
            this.lblMatch.TabIndex = 1;
            this.lblMatch.Text = "Match text:";
            // 
            // cmbMatch
            // 
            this.cmbMatch.FormattingEnabled = true;
            this.cmbMatch.Location = new System.Drawing.Point(15, 56);
            this.cmbMatch.Name = "cmbMatch";
            this.cmbMatch.Size = new System.Drawing.Size(215, 23);
            this.cmbMatch.TabIndex = 2;
            // 
            // btnClearMatch
            // 
            this.btnClearMatch.Location = new System.Drawing.Point(236, 56);
            this.btnClearMatch.Name = "btnClearMatch";
            this.btnClearMatch.Size = new System.Drawing.Size(75, 23);
            this.btnClearMatch.TabIndex = 3;
            this.btnClearMatch.Tag = "CLEARMATCH";
            this.btnClearMatch.Text = "Clear";
            this.btnClearMatch.UseVisualStyleBackColor = true;
            // 
            // lblNumber
            // 
            this.lblNumber.AutoSize = true;
            this.lblNumber.BackColor = System.Drawing.Color.Transparent;
            this.lblNumber.Location = new System.Drawing.Point(12, 94);
            this.lblNumber.Name = "lblNumber";
            this.lblNumber.Size = new System.Drawing.Size(165, 15);
            this.lblNumber.TabIndex = 4;
            this.lblNumber.Text = "Number of users in a channel:";
            // 
            // lblMinimum
            // 
            this.lblMinimum.AutoSize = true;
            this.lblMinimum.BackColor = System.Drawing.Color.Transparent;
            this.lblMinimum.Location = new System.Drawing.Point(12, 115);
            this.lblMinimum.Name = "lblMinimum";
            this.lblMinimum.Size = new System.Drawing.Size(63, 15);
            this.lblMinimum.TabIndex = 5;
            this.lblMinimum.Text = "Minimum:";
            // 
            // txtMinimum
            // 
            this.txtMinimum.Location = new System.Drawing.Point(81, 112);
            this.txtMinimum.MaxLength = 6;
            this.txtMinimum.Name = "txtMinimum";
            this.txtMinimum.Size = new System.Drawing.Size(43, 23);
            this.txtMinimum.TabIndex = 6;
            // 
            // txtMaximum
            // 
            this.txtMaximum.Location = new System.Drawing.Point(200, 112);
            this.txtMaximum.MaxLength = 6;
            this.txtMaximum.Name = "txtMaximum";
            this.txtMaximum.Size = new System.Drawing.Size(43, 23);
            this.txtMaximum.TabIndex = 8;
            // 
            // lblMaximum
            // 
            this.lblMaximum.AutoSize = true;
            this.lblMaximum.BackColor = System.Drawing.Color.Transparent;
            this.lblMaximum.Location = new System.Drawing.Point(130, 115);
            this.lblMaximum.Name = "lblMaximum";
            this.lblMaximum.Size = new System.Drawing.Size(64, 15);
            this.lblMaximum.TabIndex = 7;
            this.lblMaximum.Text = "Maximum:";
            // 
            // btnCache
            // 
            this.btnCache.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnCache.Location = new System.Drawing.Point(74, 205);
            this.btnCache.Name = "btnCache";
            this.btnCache.Size = new System.Drawing.Size(75, 23);
            this.btnCache.TabIndex = 10;
            this.btnCache.Tag = "CACHE";
            this.btnCache.Text = "Cache";
            this.btnCache.UseVisualStyleBackColor = true;
            // 
            // btnList
            // 
            this.btnList.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnList.Location = new System.Drawing.Point(155, 205);
            this.btnList.Name = "btnList";
            this.btnList.Size = new System.Drawing.Size(75, 23);
            this.btnList.TabIndex = 11;
            this.btnList.Tag = "LIST";
            this.btnList.Text = "List";
            this.btnList.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(236, 205);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 12;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // lblCache
            // 
            this.lblCache.AutoSize = true;
            this.lblCache.BackColor = System.Drawing.Color.Transparent;
            this.lblCache.Location = new System.Drawing.Point(12, 151);
            this.lblCache.Name = "lblCache";
            this.lblCache.Size = new System.Drawing.Size(113, 15);
            this.lblCache.TabIndex = 13;
            this.lblCache.Text = "Cache results to file:";
            // 
            // cmbCache
            // 
            this.cmbCache.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCache.FormattingEnabled = true;
            this.cmbCache.Location = new System.Drawing.Point(15, 169);
            this.cmbCache.Name = "cmbCache";
            this.cmbCache.Size = new System.Drawing.Size(215, 23);
            this.cmbCache.TabIndex = 14;
            // 
            // btnClearCache
            // 
            this.btnClearCache.Location = new System.Drawing.Point(236, 169);
            this.btnClearCache.Name = "btnClearCache";
            this.btnClearCache.Size = new System.Drawing.Size(75, 23);
            this.btnClearCache.TabIndex = 15;
            this.btnClearCache.Tag = "CLEARCACHE";
            this.btnClearCache.Text = "Clear";
            this.btnClearCache.UseVisualStyleBackColor = true;
            // 
            // FrmChannelList
            // 
            this.AcceptButton = this.btnList;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(323, 240);
            this.Controls.Add(this.btnClearCache);
            this.Controls.Add(this.cmbCache);
            this.Controls.Add(this.lblCache);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnList);
            this.Controls.Add(this.btnCache);
            this.Controls.Add(this.txtMaximum);
            this.Controls.Add(this.lblMaximum);
            this.Controls.Add(this.txtMinimum);
            this.Controls.Add(this.lblMinimum);
            this.Controls.Add(this.lblNumber);
            this.Controls.Add(this.btnClearMatch);
            this.Controls.Add(this.cmbMatch);
            this.Controls.Add(this.lblMatch);
            this.Controls.Add(this.lblList);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmChannelList";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Channels";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblList;
        private System.Windows.Forms.Label lblMatch;
        private System.Windows.Forms.ComboBox cmbMatch;
        private System.Windows.Forms.Button btnClearMatch;
        private System.Windows.Forms.Label lblNumber;
        private System.Windows.Forms.Label lblMinimum;
        private System.Windows.Forms.TextBox txtMinimum;
        private System.Windows.Forms.TextBox txtMaximum;
        private System.Windows.Forms.Label lblMaximum;
        private System.Windows.Forms.Button btnCache;
        private System.Windows.Forms.Button btnList;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblCache;
        private System.Windows.Forms.ComboBox cmbCache;
        private System.Windows.Forms.Button btnClearCache;
    }
}