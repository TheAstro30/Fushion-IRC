namespace FusionIRC.Forms.Theming.Controls
{
    partial class ThemeSounds
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
            this.lvSound = new libolv.ObjectListView();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnDefault = new System.Windows.Forms.Button();
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnNone = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.lvSound)).BeginInit();
            this.SuspendLayout();
            // 
            // lvSound
            // 
            this.lvSound.CheckBoxes = true;
            this.lvSound.FullRowSelect = true;
            this.lvSound.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvSound.HideSelection = false;
            this.lvSound.Location = new System.Drawing.Point(3, 3);
            this.lvSound.MultiSelect = false;
            this.lvSound.Name = "lvSound";
            this.lvSound.Size = new System.Drawing.Size(351, 354);
            this.lvSound.TabIndex = 0;
            this.lvSound.UseCompatibleStateImageBehavior = false;
            this.lvSound.View = System.Windows.Forms.View.Details;
            // 
            // btnTest
            // 
            this.btnTest.Enabled = false;
            this.btnTest.Location = new System.Drawing.Point(360, 3);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 1;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(360, 32);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            // 
            // btnDefault
            // 
            this.btnDefault.Enabled = false;
            this.btnDefault.Location = new System.Drawing.Point(360, 276);
            this.btnDefault.Name = "btnDefault";
            this.btnDefault.Size = new System.Drawing.Size(75, 23);
            this.btnDefault.TabIndex = 3;
            this.btnDefault.Text = "Default";
            this.btnDefault.UseVisualStyleBackColor = true;
            // 
            // btnSelect
            // 
            this.btnSelect.Enabled = false;
            this.btnSelect.Location = new System.Drawing.Point(360, 305);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(75, 23);
            this.btnSelect.TabIndex = 4;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = true;
            // 
            // btnNone
            // 
            this.btnNone.Enabled = false;
            this.btnNone.Location = new System.Drawing.Point(360, 334);
            this.btnNone.Name = "btnNone";
            this.btnNone.Size = new System.Drawing.Size(75, 23);
            this.btnNone.TabIndex = 5;
            this.btnNone.Text = "None";
            this.btnNone.UseVisualStyleBackColor = true;
            // 
            // ThemeSounds
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.btnNone);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnDefault);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.lvSound);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ThemeSounds";
            this.Size = new System.Drawing.Size(438, 360);
            ((System.ComponentModel.ISupportInitialize)(this.lvSound)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private libolv.ObjectListView lvSound;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnDefault;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnNone;
    }
}
