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
            this.inputArgumentNameLabel.Location = new System.Drawing.Point(3, 7);
            this.inputArgumentNameLabel.Name = "inputArgumentNameLabel";
            this.inputArgumentNameLabel.Size = new System.Drawing.Size(170, 18);
            this.inputArgumentNameLabel.TabIndex = 0;
            this.inputArgumentNameLabel.Text = "Name";
            // 
            // valueInputTextBox
            // 
            this.valueInputTextBox.Location = new System.Drawing.Point(178, 5);
            this.valueInputTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.valueInputTextBox.Name = "valueInputTextBox";
            this.valueInputTextBox.Size = new System.Drawing.Size(146, 22);
            this.valueInputTextBox.TabIndex = 1;
            // 
            // inputArgumentTypeLabel
            // 
            this.inputArgumentTypeLabel.Location = new System.Drawing.Point(329, 7);
            this.inputArgumentTypeLabel.Name = "inputArgumentTypeLabel";
            this.inputArgumentTypeLabel.Size = new System.Drawing.Size(68, 18);
            this.inputArgumentTypeLabel.TabIndex = 2;
            this.inputArgumentTypeLabel.Text = "Type";
            // 
            // inputArgumentDescriptionLabel
            // 
            this.inputArgumentDescriptionLabel.Location = new System.Drawing.Point(6, 39);
            this.inputArgumentDescriptionLabel.Name = "inputArgumentDescriptionLabel";
            this.inputArgumentDescriptionLabel.Size = new System.Drawing.Size(391, 22);
            this.inputArgumentDescriptionLabel.TabIndex = 1;
            this.inputArgumentDescriptionLabel.Text = "Input Argument Description";
            // 
            // InputArgumentUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.inputArgumentDescriptionLabel);
            this.Controls.Add(this.inputArgumentTypeLabel);
            this.Controls.Add(this.valueInputTextBox);
            this.Controls.Add(this.inputArgumentNameLabel);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "InputArgumentUserControl";
            this.Size = new System.Drawing.Size(400, 68);
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
