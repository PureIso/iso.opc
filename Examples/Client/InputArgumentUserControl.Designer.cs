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
            this.valueInputTextBox = new System.Windows.Forms.TextBox();
            this.inputArgumentTypeLabel = new System.Windows.Forms.Label();
            this.inputArgumentDescriptionLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // inputArgumentNameLabel
            // 
            this.inputArgumentNameLabel.Location = new System.Drawing.Point(2, 6);
            this.inputArgumentNameLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.inputArgumentNameLabel.Name = "inputArgumentNameLabel";
            this.inputArgumentNameLabel.Size = new System.Drawing.Size(100, 15);
            this.inputArgumentNameLabel.TabIndex = 0;
            this.inputArgumentNameLabel.Text = "Name";
            this.inputArgumentNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // valueInputTextBox
            // 
            this.valueInputTextBox.Location = new System.Drawing.Point(106, 3);
            this.valueInputTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.valueInputTextBox.Name = "valueInputTextBox";
            this.valueInputTextBox.Size = new System.Drawing.Size(76, 20);
            this.valueInputTextBox.TabIndex = 1;
            // 
            // inputArgumentTypeLabel
            // 
            this.inputArgumentTypeLabel.Location = new System.Drawing.Point(186, 6);
            this.inputArgumentTypeLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.inputArgumentTypeLabel.Name = "inputArgumentTypeLabel";
            this.inputArgumentTypeLabel.Size = new System.Drawing.Size(100, 15);
            this.inputArgumentTypeLabel.TabIndex = 2;
            this.inputArgumentTypeLabel.Text = "Type";
            this.inputArgumentTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // inputArgumentDescriptionLabel
            // 
            this.inputArgumentDescriptionLabel.Location = new System.Drawing.Point(5, 32);
            this.inputArgumentDescriptionLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.inputArgumentDescriptionLabel.Name = "inputArgumentDescriptionLabel";
            this.inputArgumentDescriptionLabel.Size = new System.Drawing.Size(281, 18);
            this.inputArgumentDescriptionLabel.TabIndex = 1;
            this.inputArgumentDescriptionLabel.Text = "Input Argument Description";
            // 
            // InputArgumentUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.inputArgumentDescriptionLabel);
            this.Controls.Add(this.inputArgumentTypeLabel);
            this.Controls.Add(this.valueInputTextBox);
            this.Controls.Add(this.inputArgumentNameLabel);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximumSize = new System.Drawing.Size(290, 55);
            this.MinimumSize = new System.Drawing.Size(290, 55);
            this.Name = "InputArgumentUserControl";
            this.Size = new System.Drawing.Size(290, 55);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label inputArgumentNameLabel;
        private System.Windows.Forms.TextBox valueInputTextBox;
        private System.Windows.Forms.Label inputArgumentTypeLabel;
        private System.Windows.Forms.Label inputArgumentDescriptionLabel;
    }
}
