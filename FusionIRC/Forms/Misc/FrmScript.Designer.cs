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
            this.txtEdit = new ircScript.Controls.ScriptEditor();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtEdit
            // 
            this.txtEdit.CaseSensitive = false;
            this.txtEdit.FilterAutoComplete = false;
            this.txtEdit.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEdit.Location = new System.Drawing.Point(12, 12);
            this.txtEdit.MaxUndoRedoSteps = 50;
            this.txtEdit.Name = "txtEdit";
            this.txtEdit.Size = new System.Drawing.Size(326, 297);
            this.txtEdit.TabIndex = 0;
            this.txtEdit.Text = "";
            this.txtEdit.WordWrap = false;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(263, 321);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // FrmScript
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 356);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.txtEdit);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(366, 395);
            this.Name = "FrmScript";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FusionIRC - Alias Editor";
            this.ResumeLayout(false);

        }

        #endregion

        private ircScript.Controls.ScriptEditor txtEdit;
        private System.Windows.Forms.Button btnClose;


    }
}