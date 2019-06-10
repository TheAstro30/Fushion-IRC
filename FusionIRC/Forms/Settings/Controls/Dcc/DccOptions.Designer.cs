namespace FusionIRC.Forms.Settings.Controls.Dcc
{
    partial class DccOptions
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
            this.gbPorts = new System.Windows.Forms.GroupBox();
            this.lblMin = new System.Windows.Forms.Label();
            this.txtMin = new System.Windows.Forms.TextBox();
            this.txtMax = new System.Windows.Forms.TextBox();
            this.lblMax = new System.Windows.Forms.Label();
            this.chkRandomize = new System.Windows.Forms.CheckBox();
            this.chkBind = new System.Windows.Forms.CheckBox();
            this.txtBindAddress = new System.Windows.Forms.TextBox();
            this.cmbPacketSize = new System.Windows.Forms.ComboBox();
            this.lblPacketSize = new System.Windows.Forms.Label();
            this.gbTimeout = new System.Windows.Forms.GroupBox();
            this.lblRequests = new System.Windows.Forms.Label();
            this.txtRequests = new System.Windows.Forms.TextBox();
            this.txtTransfers = new System.Windows.Forms.TextBox();
            this.lblTransfers = new System.Windows.Forms.Label();
            this.txtConnections = new System.Windows.Forms.TextBox();
            this.lblConnections = new System.Windows.Forms.Label();
            this.gbFilter = new System.Windows.Forms.GroupBox();
            this.lblFilter = new System.Windows.Forms.Label();
            this.cmbMethod = new System.Windows.Forms.ComboBox();
            this.lvExt = new libolv.ObjectListView();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.chkDialog = new System.Windows.Forms.CheckBox();
            this.lblBytes = new System.Windows.Forms.Label();
            this.gbPorts.SuspendLayout();
            this.gbTimeout.SuspendLayout();
            this.gbFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lvExt)).BeginInit();
            this.SuspendLayout();
            // 
            // gbPorts
            // 
            this.gbPorts.Controls.Add(this.lblBytes);
            this.gbPorts.Controls.Add(this.lblPacketSize);
            this.gbPorts.Controls.Add(this.cmbPacketSize);
            this.gbPorts.Controls.Add(this.txtBindAddress);
            this.gbPorts.Controls.Add(this.chkBind);
            this.gbPorts.Controls.Add(this.chkRandomize);
            this.gbPorts.Controls.Add(this.txtMax);
            this.gbPorts.Controls.Add(this.lblMax);
            this.gbPorts.Controls.Add(this.txtMin);
            this.gbPorts.Controls.Add(this.lblMin);
            this.gbPorts.Location = new System.Drawing.Point(43, 41);
            this.gbPorts.Name = "gbPorts";
            this.gbPorts.Size = new System.Drawing.Size(346, 110);
            this.gbPorts.TabIndex = 0;
            this.gbPorts.TabStop = false;
            this.gbPorts.Text = "General:";
            // 
            // lblMin
            // 
            this.lblMin.AutoSize = true;
            this.lblMin.Location = new System.Drawing.Point(6, 25);
            this.lblMin.Name = "lblMin";
            this.lblMin.Size = new System.Drawing.Size(56, 15);
            this.lblMin.TabIndex = 0;
            this.lblMin.Text = "Min port:";
            // 
            // txtMin
            // 
            this.txtMin.Location = new System.Drawing.Point(68, 22);
            this.txtMin.MaxLength = 5;
            this.txtMin.Name = "txtMin";
            this.txtMin.Size = new System.Drawing.Size(50, 23);
            this.txtMin.TabIndex = 1;
            this.txtMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtMax
            // 
            this.txtMax.Location = new System.Drawing.Point(187, 22);
            this.txtMax.MaxLength = 5;
            this.txtMax.Name = "txtMax";
            this.txtMax.Size = new System.Drawing.Size(50, 23);
            this.txtMax.TabIndex = 3;
            this.txtMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblMax
            // 
            this.lblMax.AutoSize = true;
            this.lblMax.Location = new System.Drawing.Point(124, 25);
            this.lblMax.Name = "lblMax";
            this.lblMax.Size = new System.Drawing.Size(57, 15);
            this.lblMax.TabIndex = 2;
            this.lblMax.Text = "Max port:";
            // 
            // chkRandomize
            // 
            this.chkRandomize.AutoSize = true;
            this.chkRandomize.Location = new System.Drawing.Point(243, 25);
            this.chkRandomize.Name = "chkRandomize";
            this.chkRandomize.Size = new System.Drawing.Size(85, 19);
            this.chkRandomize.TabIndex = 4;
            this.chkRandomize.Text = "Randomize";
            this.chkRandomize.UseVisualStyleBackColor = true;
            // 
            // chkBind
            // 
            this.chkBind.AutoSize = true;
            this.chkBind.Location = new System.Drawing.Point(68, 53);
            this.chkBind.Name = "chkBind";
            this.chkBind.Size = new System.Drawing.Size(80, 19);
            this.chkBind.TabIndex = 5;
            this.chkBind.Text = "Bind to IP:";
            this.chkBind.UseVisualStyleBackColor = true;
            // 
            // txtBindAddress
            // 
            this.txtBindAddress.Location = new System.Drawing.Point(154, 51);
            this.txtBindAddress.Name = "txtBindAddress";
            this.txtBindAddress.Size = new System.Drawing.Size(126, 23);
            this.txtBindAddress.TabIndex = 6;
            // 
            // cmbPacketSize
            // 
            this.cmbPacketSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPacketSize.FormattingEnabled = true;
            this.cmbPacketSize.Location = new System.Drawing.Point(155, 80);
            this.cmbPacketSize.Name = "cmbPacketSize";
            this.cmbPacketSize.Size = new System.Drawing.Size(73, 23);
            this.cmbPacketSize.TabIndex = 7;
            // 
            // lblPacketSize
            // 
            this.lblPacketSize.AutoSize = true;
            this.lblPacketSize.Location = new System.Drawing.Point(53, 83);
            this.lblPacketSize.Name = "lblPacketSize";
            this.lblPacketSize.Size = new System.Drawing.Size(96, 15);
            this.lblPacketSize.TabIndex = 8;
            this.lblPacketSize.Text = "Send packet size:";
            // 
            // gbTimeout
            // 
            this.gbTimeout.Controls.Add(this.txtConnections);
            this.gbTimeout.Controls.Add(this.lblConnections);
            this.gbTimeout.Controls.Add(this.txtTransfers);
            this.gbTimeout.Controls.Add(this.lblTransfers);
            this.gbTimeout.Controls.Add(this.txtRequests);
            this.gbTimeout.Controls.Add(this.lblRequests);
            this.gbTimeout.Location = new System.Drawing.Point(43, 157);
            this.gbTimeout.Name = "gbTimeout";
            this.gbTimeout.Size = new System.Drawing.Size(346, 68);
            this.gbTimeout.TabIndex = 1;
            this.gbTimeout.TabStop = false;
            this.gbTimeout.Text = "Time-outs (in seconds):";
            // 
            // lblRequests
            // 
            this.lblRequests.AutoSize = true;
            this.lblRequests.Location = new System.Drawing.Point(6, 19);
            this.lblRequests.Name = "lblRequests";
            this.lblRequests.Size = new System.Drawing.Size(105, 15);
            this.lblRequests.TabIndex = 0;
            this.lblRequests.Text = "Get/send requests:";
            // 
            // txtRequests
            // 
            this.txtRequests.Location = new System.Drawing.Point(9, 37);
            this.txtRequests.MaxLength = 3;
            this.txtRequests.Name = "txtRequests";
            this.txtRequests.Size = new System.Drawing.Size(33, 23);
            this.txtRequests.TabIndex = 2;
            this.txtRequests.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtTransfers
            // 
            this.txtTransfers.Location = new System.Drawing.Point(120, 37);
            this.txtTransfers.MaxLength = 3;
            this.txtTransfers.Name = "txtTransfers";
            this.txtTransfers.Size = new System.Drawing.Size(33, 23);
            this.txtTransfers.TabIndex = 4;
            this.txtTransfers.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblTransfers
            // 
            this.lblTransfers.AutoSize = true;
            this.lblTransfers.Location = new System.Drawing.Point(117, 19);
            this.lblTransfers.Name = "lblTransfers";
            this.lblTransfers.Size = new System.Drawing.Size(106, 15);
            this.lblTransfers.TabIndex = 3;
            this.lblTransfers.Text = "Get/send transfers:";
            // 
            // txtConnections
            // 
            this.txtConnections.Location = new System.Drawing.Point(234, 37);
            this.txtConnections.MaxLength = 3;
            this.txtConnections.Name = "txtConnections";
            this.txtConnections.Size = new System.Drawing.Size(33, 23);
            this.txtConnections.TabIndex = 6;
            this.txtConnections.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblConnections
            // 
            this.lblConnections.AutoSize = true;
            this.lblConnections.Location = new System.Drawing.Point(229, 19);
            this.lblConnections.Name = "lblConnections";
            this.lblConnections.Size = new System.Drawing.Size(103, 15);
            this.lblConnections.TabIndex = 5;
            this.lblConnections.Text = "Chat connections:";
            // 
            // gbFilter
            // 
            this.gbFilter.Controls.Add(this.chkDialog);
            this.gbFilter.Controls.Add(this.btnDelete);
            this.gbFilter.Controls.Add(this.btnAdd);
            this.gbFilter.Controls.Add(this.lvExt);
            this.gbFilter.Controls.Add(this.cmbMethod);
            this.gbFilter.Controls.Add(this.lblFilter);
            this.gbFilter.Location = new System.Drawing.Point(43, 231);
            this.gbFilter.Name = "gbFilter";
            this.gbFilter.Size = new System.Drawing.Size(346, 105);
            this.gbFilter.TabIndex = 2;
            this.gbFilter.TabStop = false;
            this.gbFilter.Text = "Filter:";
            // 
            // lblFilter
            // 
            this.lblFilter.AutoSize = true;
            this.lblFilter.Location = new System.Drawing.Point(6, 19);
            this.lblFilter.Name = "lblFilter";
            this.lblFilter.Size = new System.Drawing.Size(81, 15);
            this.lblFilter.TabIndex = 0;
            this.lblFilter.Text = "Filter method:";
            // 
            // cmbMethod
            // 
            this.cmbMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMethod.FormattingEnabled = true;
            this.cmbMethod.Location = new System.Drawing.Point(9, 37);
            this.cmbMethod.Name = "cmbMethod";
            this.cmbMethod.Size = new System.Drawing.Size(140, 23);
            this.cmbMethod.TabIndex = 1;
            // 
            // lvExt
            // 
            this.lvExt.FullRowSelect = true;
            this.lvExt.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvExt.HideSelection = false;
            this.lvExt.Location = new System.Drawing.Point(155, 16);
            this.lvExt.Name = "lvExt";
            this.lvExt.Size = new System.Drawing.Size(104, 81);
            this.lvExt.TabIndex = 2;
            this.lvExt.UseCompatibleStateImageBehavior = false;
            this.lvExt.View = System.Windows.Forms.View.Details;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(265, 15);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 3;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            this.btnDelete.Enabled = false;
            this.btnDelete.Location = new System.Drawing.Point(265, 44);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 4;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            // 
            // chkDialog
            // 
            this.chkDialog.AutoSize = true;
            this.chkDialog.Location = new System.Drawing.Point(9, 78);
            this.chkDialog.Name = "chkDialog";
            this.chkDialog.Size = new System.Drawing.Size(140, 19);
            this.chkDialog.TabIndex = 5;
            this.chkDialog.Text = "Show rejection dialog";
            this.chkDialog.UseVisualStyleBackColor = true;
            // 
            // lblBytes
            // 
            this.lblBytes.AutoSize = true;
            this.lblBytes.Location = new System.Drawing.Point(234, 83);
            this.lblBytes.Name = "lblBytes";
            this.lblBytes.Size = new System.Drawing.Size(35, 15);
            this.lblBytes.TabIndex = 9;
            this.lblBytes.Text = "bytes";
            // 
            // DccOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbFilter);
            this.Controls.Add(this.gbTimeout);
            this.Controls.Add(this.gbPorts);
            this.Name = "DccOptions";
            this.gbPorts.ResumeLayout(false);
            this.gbPorts.PerformLayout();
            this.gbTimeout.ResumeLayout(false);
            this.gbTimeout.PerformLayout();
            this.gbFilter.ResumeLayout(false);
            this.gbFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lvExt)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbPorts;
        private System.Windows.Forms.Label lblPacketSize;
        private System.Windows.Forms.ComboBox cmbPacketSize;
        private System.Windows.Forms.TextBox txtBindAddress;
        private System.Windows.Forms.CheckBox chkBind;
        private System.Windows.Forms.CheckBox chkRandomize;
        private System.Windows.Forms.TextBox txtMax;
        private System.Windows.Forms.Label lblMax;
        private System.Windows.Forms.TextBox txtMin;
        private System.Windows.Forms.Label lblMin;
        private System.Windows.Forms.GroupBox gbTimeout;
        private System.Windows.Forms.TextBox txtConnections;
        private System.Windows.Forms.Label lblConnections;
        private System.Windows.Forms.TextBox txtTransfers;
        private System.Windows.Forms.Label lblTransfers;
        private System.Windows.Forms.TextBox txtRequests;
        private System.Windows.Forms.Label lblRequests;
        private System.Windows.Forms.GroupBox gbFilter;
        private libolv.ObjectListView lvExt;
        private System.Windows.Forms.ComboBox cmbMethod;
        private System.Windows.Forms.Label lblFilter;
        private System.Windows.Forms.CheckBox chkDialog;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Label lblBytes;
    }
}
