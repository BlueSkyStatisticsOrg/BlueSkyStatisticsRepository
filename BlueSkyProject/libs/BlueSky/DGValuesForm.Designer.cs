namespace AnalyticsUnlimited.Client_WPF
{
    partial class DGValuesForm
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
            this.ValGrpBox = new System.Windows.Forms.GroupBox();
            this.ValLstBox = new System.Windows.Forms.ListBox();
            this.ValRemvBut = new System.Windows.Forms.Button();
            this.ValChngBut = new System.Windows.Forms.Button();
            this.ValAddBut = new System.Windows.Forms.Button();
            this.LabeltextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ValOKBut = new System.Windows.Forms.Button();
            this.ValCancelBut = new System.Windows.Forms.Button();
            this.moveUp = new System.Windows.Forms.Button();
            this.moveDown = new System.Windows.Forms.Button();
            this.ValGrpBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ValGrpBox
            // 
            this.ValGrpBox.Controls.Add(this.moveDown);
            this.ValGrpBox.Controls.Add(this.moveUp);
            this.ValGrpBox.Controls.Add(this.ValLstBox);
            this.ValGrpBox.Controls.Add(this.ValRemvBut);
            this.ValGrpBox.Controls.Add(this.ValChngBut);
            this.ValGrpBox.Controls.Add(this.ValAddBut);
            this.ValGrpBox.Controls.Add(this.LabeltextBox);
            this.ValGrpBox.Controls.Add(this.label2);
            this.ValGrpBox.Location = new System.Drawing.Point(9, 13);
            this.ValGrpBox.Name = "ValGrpBox";
            this.ValGrpBox.Size = new System.Drawing.Size(355, 219);
            this.ValGrpBox.TabIndex = 0;
            this.ValGrpBox.TabStop = false;
            this.ValGrpBox.Text = "Set Value Lables";
            // 
            // ValLstBox
            // 
            this.ValLstBox.FormattingEnabled = true;
            this.ValLstBox.Location = new System.Drawing.Point(76, 78);
            this.ValLstBox.Name = "ValLstBox";
            this.ValLstBox.Size = new System.Drawing.Size(206, 108);
            this.ValLstBox.TabIndex = 8;
            this.ValLstBox.SelectedIndexChanged += new System.EventHandler(this.ValLstBox_SelectedIndexChanged);
            // 
            // ValRemvBut
            // 
            this.ValRemvBut.Location = new System.Drawing.Point(14, 164);
            this.ValRemvBut.Name = "ValRemvBut";
            this.ValRemvBut.Size = new System.Drawing.Size(56, 23);
            this.ValRemvBut.TabIndex = 7;
            this.ValRemvBut.Text = "&Remove";
            this.ValRemvBut.UseVisualStyleBackColor = true;
            this.ValRemvBut.Click += new System.EventHandler(this.ValRemvBut_Click);
            // 
            // ValChngBut
            // 
            this.ValChngBut.Location = new System.Drawing.Point(13, 120);
            this.ValChngBut.Name = "ValChngBut";
            this.ValChngBut.Size = new System.Drawing.Size(56, 23);
            this.ValChngBut.TabIndex = 6;
            this.ValChngBut.Text = "Chang&e";
            this.ValChngBut.UseVisualStyleBackColor = true;
            this.ValChngBut.Click += new System.EventHandler(this.ValChngBut_Click);
            // 
            // ValAddBut
            // 
            this.ValAddBut.Location = new System.Drawing.Point(13, 76);
            this.ValAddBut.Name = "ValAddBut";
            this.ValAddBut.Size = new System.Drawing.Size(56, 23);
            this.ValAddBut.TabIndex = 5;
            this.ValAddBut.Text = "&Add";
            this.ValAddBut.UseVisualStyleBackColor = true;
            this.ValAddBut.Click += new System.EventHandler(this.ValAddBut_Click);
            // 
            // LabeltextBox
            // 
            this.LabeltextBox.Location = new System.Drawing.Point(59, 38);
            this.LabeltextBox.Name = "LabeltextBox";
            this.LabeltextBox.Size = new System.Drawing.Size(223, 20);
            this.LabeltextBox.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Label:";
            // 
            // ValOKBut
            // 
            this.ValOKBut.Location = new System.Drawing.Point(89, 237);
            this.ValOKBut.Name = "ValOKBut";
            this.ValOKBut.Size = new System.Drawing.Size(54, 23);
            this.ValOKBut.TabIndex = 1;
            this.ValOKBut.Text = "&Ok";
            this.ValOKBut.UseVisualStyleBackColor = true;
            this.ValOKBut.Click += new System.EventHandler(this.ValOKBut_Click);
            // 
            // ValCancelBut
            // 
            this.ValCancelBut.Location = new System.Drawing.Point(223, 237);
            this.ValCancelBut.Name = "ValCancelBut";
            this.ValCancelBut.Size = new System.Drawing.Size(54, 23);
            this.ValCancelBut.TabIndex = 2;
            this.ValCancelBut.Text = "&Cancel";
            this.ValCancelBut.UseVisualStyleBackColor = true;
            this.ValCancelBut.Click += new System.EventHandler(this.ValCancelBut_Click);
            // 
            // moveUp
            // 
            this.moveUp.Location = new System.Drawing.Point(289, 79);
            this.moveUp.Name = "moveUp";
            this.moveUp.Size = new System.Drawing.Size(49, 22);
            this.moveUp.TabIndex = 9;
            this.moveUp.Text = "&Up";
            this.moveUp.UseVisualStyleBackColor = true;
            this.moveUp.Click += new System.EventHandler(this.moveUp_Click);
            // 
            // moveDown
            // 
            this.moveDown.Location = new System.Drawing.Point(288, 162);
            this.moveDown.Name = "moveDown";
            this.moveDown.Size = new System.Drawing.Size(50, 24);
            this.moveDown.TabIndex = 10;
            this.moveDown.Text = "&Down";
            this.moveDown.UseVisualStyleBackColor = true;
            this.moveDown.Click += new System.EventHandler(this.moveDown_Click);
            // 
            // DGValuesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 274);
            this.Controls.Add(this.ValCancelBut);
            this.Controls.Add(this.ValOKBut);
            this.Controls.Add(this.ValGrpBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DGValuesForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Value Labels";
            this.ValGrpBox.ResumeLayout(false);
            this.ValGrpBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox ValGrpBox;
        private System.Windows.Forms.Button ValRemvBut;
        private System.Windows.Forms.Button ValChngBut;
        private System.Windows.Forms.Button ValAddBut;
        private System.Windows.Forms.TextBox LabeltextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox ValLstBox;
        private System.Windows.Forms.Button ValOKBut;
        private System.Windows.Forms.Button ValCancelBut;
        private System.Windows.Forms.Button moveUp;
        private System.Windows.Forms.Button moveDown;
    }
}