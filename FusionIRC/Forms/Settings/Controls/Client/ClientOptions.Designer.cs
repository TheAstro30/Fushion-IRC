namespace FusionIRC.Forms.Settings.Controls.Client
{
    partial class ClientOptions
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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("General", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Channels", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Show...", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Show menubar");
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("Show toolbar");
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Always keep channel windows open");
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("Re-join open channels on connect");
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("Re-join channels on kick");
            System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem("Auto-join channel on invite (not recommended)");
            System.Windows.Forms.ListViewItem listViewItem7 = new System.Windows.Forms.ListViewItem("Show favorites dialog on connect");
            System.Windows.Forms.ListViewItem listViewItem8 = new System.Windows.Forms.ListViewItem("Client ping/pong event in console");
            System.Windows.Forms.ListViewItem listViewItem9 = new System.Windows.Forms.ListViewItem("MOTD on connect");
            this.lvOptions = new System.Windows.Forms.ListView();
            this.cHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // lvOptions
            // 
            this.lvOptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lvOptions.CheckBoxes = true;
            this.lvOptions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.cHeader});
            listViewGroup1.Header = "General";
            listViewGroup1.Name = "gGeneral";
            listViewGroup2.Header = "Channels";
            listViewGroup2.Name = "gChannels";
            listViewGroup3.Header = "Show...";
            listViewGroup3.Name = "gShow";
            this.lvOptions.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2,
            listViewGroup3});
            this.lvOptions.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            listViewItem1.Group = listViewGroup1;
            listViewItem1.StateImageIndex = 0;
            listViewItem2.Group = listViewGroup1;
            listViewItem2.StateImageIndex = 0;
            listViewItem3.Group = listViewGroup2;
            listViewItem3.StateImageIndex = 0;
            listViewItem4.Group = listViewGroup2;
            listViewItem4.StateImageIndex = 0;
            listViewItem5.Group = listViewGroup2;
            listViewItem5.StateImageIndex = 0;
            listViewItem6.Group = listViewGroup2;
            listViewItem6.StateImageIndex = 0;
            listViewItem7.Group = listViewGroup2;
            listViewItem7.StateImageIndex = 0;
            listViewItem8.Group = listViewGroup3;
            listViewItem8.StateImageIndex = 0;
            listViewItem9.Group = listViewGroup3;
            listViewItem9.StateImageIndex = 0;
            this.lvOptions.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4,
            listViewItem5,
            listViewItem6,
            listViewItem7,
            listViewItem8,
            listViewItem9});
            this.lvOptions.Location = new System.Drawing.Point(3, 39);
            this.lvOptions.MultiSelect = false;
            this.lvOptions.Name = "lvOptions";
            this.lvOptions.Size = new System.Drawing.Size(421, 305);
            this.lvOptions.TabIndex = 0;
            this.lvOptions.UseCompatibleStateImageBehavior = false;
            this.lvOptions.View = System.Windows.Forms.View.Details;
            // 
            // cHeader
            // 
            this.cHeader.Text = "Header";
            this.cHeader.Width = 300;
            // 
            // ClientOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lvOptions);
            this.Name = "ClientOptions";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvOptions;
        private System.Windows.Forms.ColumnHeader cHeader;
    }
}
