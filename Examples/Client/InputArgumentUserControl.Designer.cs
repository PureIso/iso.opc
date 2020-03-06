namespace Client
{
    partial class InputArgumentUserControl
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
            this.inputArgumentNameLabel = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.inputArgumentTypeLabel = new System.Windows.Forms.Label();
            this.inputArgumentDescriptionLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // inputArgumentNameLabel
            // 
            this.inputArgumentNameLabel.Location = new System.Drawing.Point(3, 9);
            this.inputArgumentNameLabel.Name = "inputArgumentNameLabel";
            this.inputArgumentNameLabel.Size = new System.Drawing.Size(191, 23);
            this.inputArgumentNameLabel.TabIndex = 0;
            this.inputArgumentNameLabel.Text = "Name";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(200, 6);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(164, 26);
            this.textBox1.TabIndex = 1;
            // 
            // inputArgumentTypeLabel
            // 
            this.inputArgumentTypeLabel.Location = new System.Drawing.Point(370, 9);
            this.inputArgumentTypeLabel.Name = "inputArgumentTypeLabel";
            this.inputArgumentTypeLabel.Size = new System.Drawing.Size(77, 23);
            this.inputArgumentTypeLabel.TabIndex = 2;
            this.inputArgumentTypeLabel.Text = "Type";
            // 
            // inputArgumentDescriptionLabel
            // 
            this.inputArgumentDescriptionLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.inputArgumentDescriptionLabel.Location = new System.Drawing.Point(7, 35);
            this.inputArgumentDescriptionLabel.MaximumSize = new System.Drawing.Size(440, 46);
            this.inputArgumentDescriptionLabel.MinimumSize = new System.Drawing.Size(440, 46);
            this.inputArgumentDescriptionLabel.Name = "inputArgumentDescriptionLabel";
            this.inputArgumentDescriptionLabel.Size = new System.Drawing.Size(440, 46);
            this.inputArgumentDescriptionLabel.TabIndex = 1;
            this.inputArgumentDescriptionLabel.Text = "Input Argument Description";
            // 
            // InputArgumentUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.inputArgumentDescriptionLabel);
            this.Controls.Add(this.inputArgumentTypeLabel);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.inputArgumentNameLabel);
            this.Name = "InputArgumentUserControl";
            this.Size = new System.Drawing.Size(450, 85);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label inputArgumentNameLabel;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label inputArgumentTypeLabel;
        private System.Windows.Forms.Label inputArgumentDescriptionLabel;
    }
}
