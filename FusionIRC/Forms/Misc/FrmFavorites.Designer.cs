namespace FusionIRC.Forms.Misc
{
    partial class FrmFavorites
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
            this.btnClose = new System.Windows.Forms.Button();
            this.chkShow = new System.Windows.Forms.CheckBox();
            this.lvFave = new libolv.ObjectListView();
            this.colChan = ((libolv.OlvColumn)(new libolv.OlvColumn()));
            this.colDesc = ((libolv.OlvColumn)(new libolv.OlvColumn()));
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnJoin = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.lvFave)).BeginInit();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Location = new System.Drawing.Point(295, 367);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // chkShow
            // 
            this.chkShow.AutoSize = true;
            this.chkShow.Location = new System.Drawing.Point(12, 371);
            this.chkShow.Name = "chkShow";
            this.chkShow.Size = new System.Drawing.Size(154, 19);
            this.chkShow.TabIndex = 1;
            this.chkShow.Text = "Show dialog on connect";
            this.chkShow.UseVisualStyleBackColor = true;
            // 
            // lvFave
            // 
            this.lvFave.AllColumns.Add(this.colChan);
            this.lvFave.AllColumns.Add(this.colDesc);
            this.lvFave.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colChan,
            this.colDesc});
            this.lvFave.FullRowSelect = true;
            this.lvFave.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvFave.Location = new System.Drawing.Point(12, 12);
            this.lvFave.MultiSelect = false;
            this.lvFave.Name = "lvFave";
            this.lvFave.Size = new System.Drawing.Size(277, 341);
            this.lvFave.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvFave.TabIndex = 2;
            this.lvFave.UseCompatibleStateImageBehavior = false;
            this.lvFave.View = System.Windows.Forms.View.Details;
            // 
            // colChan
            // 
            this.colChan.AspectName = "Name";
            this.colChan.CellPadding = null;
            this.colChan.IsEditable = false;
            this.colChan.Sortable = false;
            this.colChan.Text = "Channel:";
            this.colChan.Width = 120;
            // 
            // colDesc
            // 
            this.colDesc.AspectName = "Description";
            this.colDesc.CellPadding = null;
            this.colDesc.IsEditable = false;
            this.colDesc.Sortable = false;
            this.colDesc.Text = "Description:";
            this.colDesc.Width = 300;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(295, 12);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 3;
            this.btnAdd.Tag = "ADD";
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            // 
            // btnEdit
            // 
            this.btnEdit.Enabled = false;
            this.btnEdit.Location = new System.Drawing.Point(295, 54);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(75, 23);
            this.btnEdit.TabIndex = 4;
            this.btnEdit.Tag = "EDIT";
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            this.btnDelete.Enabled = false;
            this.btnDelete.Location = new System.Drawing.Point(295, 83);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 5;
            this.btnDelete.Tag = "DELETE";
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnClear
            // 
            this.btnClear.Enabled = false;
            this.btnClear.Location = new System.Drawing.Point(295, 112);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 6;
            this.btnClear.Tag = "CLEAR";
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            // 
            // btnJoin
            // 
            this.btnJoin.Enabled = false;
            this.btnJoin.Location = new System.Drawing.Point(295, 330);
            this.btnJoin.Name = "btnJoin";
            this.btnJoin.Size = new System.Drawing.Size(75, 23);
            this.btnJoin.TabIndex = 7;
            this.btnJoin.Tag = "JOIN";
            this.btnJoin.Text = "Join";
            this.btnJoin.UseVisualStyleBackColor = true;
            // 
            // FrmFavorites
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 402);
            this.Controls.Add(this.btnJoin);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.lvFave);
            this.Controls.Add(this.chkShow);
            this.Controls.Add(this.btnClose);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FrmFavorites";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Channel Favorites";
            ((System.ComponentModel.ISupportInitialize)(this.lvFave)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.CheckBox chkShow;
        private libolv.ObjectListView lvFave;
        private libolv.OlvColumn colChan;
        private libolv.OlvColumn colDesc;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnJoin;
    }
}