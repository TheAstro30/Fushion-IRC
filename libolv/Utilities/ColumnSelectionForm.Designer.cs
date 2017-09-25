namespace libolv.Utilities
{
    partial class ColumnSelectionForm
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
            if (disposing && (components != null)) {
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
            this.buttonMoveUp = new System.Windows.Forms.Button();
            this.buttonMoveDown = new System.Windows.Forms.Button();
            this.buttonShow = new System.Windows.Forms.Button();
            this.buttonHide = new System.Windows.Forms.Button();
            this.lblChoose = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.olv = new ObjectListView();
            this.olvColumn1 = ((OlvColumn)(new OlvColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.olv)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonMoveUp
            // 
            this.buttonMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMoveUp.Location = new System.Drawing.Point(295, 31);
            this.buttonMoveUp.Name = "buttonMoveUp";
            this.buttonMoveUp.Size = new System.Drawing.Size(87, 23);
            this.buttonMoveUp.TabIndex = 1;
            this.buttonMoveUp.Text = "Move &Up";
            this.buttonMoveUp.UseVisualStyleBackColor = true;
            this.buttonMoveUp.Click += new System.EventHandler(this.ButtonMoveUpClick);
            // 
            // buttonMoveDown
            // 
            this.buttonMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMoveDown.Location = new System.Drawing.Point(295, 60);
            this.buttonMoveDown.Name = "buttonMoveDown";
            this.buttonMoveDown.Size = new System.Drawing.Size(87, 23);
            this.buttonMoveDown.TabIndex = 2;
            this.buttonMoveDown.Text = "Move &Down";
            this.buttonMoveDown.UseVisualStyleBackColor = true;
            this.buttonMoveDown.Click += new System.EventHandler(this.ButtonMoveDownClick);
            // 
            // buttonShow
            // 
            this.buttonShow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonShow.Location = new System.Drawing.Point(295, 89);
            this.buttonShow.Name = "buttonShow";
            this.buttonShow.Size = new System.Drawing.Size(87, 23);
            this.buttonShow.TabIndex = 3;
            this.buttonShow.Text = "&Show";
            this.buttonShow.UseVisualStyleBackColor = true;
            this.buttonShow.Click += new System.EventHandler(this.ButtonShowClick);
            // 
            // buttonHide
            // 
            this.buttonHide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonHide.Location = new System.Drawing.Point(295, 118);
            this.buttonHide.Name = "buttonHide";
            this.buttonHide.Size = new System.Drawing.Size(87, 23);
            this.buttonHide.TabIndex = 4;
            this.buttonHide.Text = "&Hide";
            this.buttonHide.UseVisualStyleBackColor = true;
            this.buttonHide.Click += new System.EventHandler(this.ButtonHideClick);
            // 
            // lblChoose
            // 
            this.lblChoose.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblChoose.BackColor = System.Drawing.SystemColors.Control;
            this.lblChoose.Location = new System.Drawing.Point(13, 9);
            this.lblChoose.Name = "lblChoose";
            this.lblChoose.Size = new System.Drawing.Size(366, 19);
            this.lblChoose.TabIndex = 5;
            this.lblChoose.Text = "Choose the columns you want to see in this list. ";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(198, 304);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(87, 23);
            this.buttonOK.TabIndex = 6;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(295, 304);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(87, 23);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
            // 
            // olv
            // 
            this.olv.AllColumns.Add(this.olvColumn1);
            this.olv.AlternateRowBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.olv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.olv.CellEditActivation = ObjectListView.CellEditActivateMode.SingleClick;
            this.olv.CheckBoxes = true;
            this.olv.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1});
            this.olv.FullRowSelect = true;
            this.olv.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.olv.HideSelection = false;
            this.olv.Location = new System.Drawing.Point(12, 31);
            this.olv.MultiSelect = false;
            this.olv.Name = "olv";
            this.olv.ShowGroups = false;
            this.olv.ShowSortIndicators = false;
            this.olv.Size = new System.Drawing.Size(273, 259);
            this.olv.TabIndex = 0;
            this.olv.UseCompatibleStateImageBehavior = false;
            this.olv.View = System.Windows.Forms.View.Details;
            this.olv.SelectionChanged += new System.EventHandler(this.ObjectListView1SelectionChanged);
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "Text";
            this.olvColumn1.CellPadding = null;
            this.olvColumn1.Text = "Column";
            this.olvColumn1.Width = 267;
            // 
            // ColumnSelectionForm
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(391, 339);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.lblChoose);
            this.Controls.Add(this.buttonHide);
            this.Controls.Add(this.buttonShow);
            this.Controls.Add(this.buttonMoveDown);
            this.Controls.Add(this.buttonMoveUp);
            this.Controls.Add(this.olv);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ColumnSelectionForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Column Selection";
            ((System.ComponentModel.ISupportInitialize)(this.olv)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private ObjectListView olv;
        private System.Windows.Forms.Button buttonMoveUp;
        private System.Windows.Forms.Button buttonMoveDown;
        private System.Windows.Forms.Button buttonShow;
        private System.Windows.Forms.Button buttonHide;
        private OlvColumn olvColumn1;
        private System.Windows.Forms.Label lblChoose;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}