namespace Iso.Opc.Client
{
    partial class ArgumentUserControl
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
            this.inputArgumentNameLabel.Location = new System.Drawing.Point(3, 9);
            this.inputArgumentNameLabel.Name = "inputArgumentNameLabel";
            this.inputArgumentNameLabel.Size = new System.Drawing.Size(150, 23);
            this.inputArgumentNameLabel.TabIndex = 0;
            this.inputArgumentNameLabel.Text = "Name";
            this.inputArgumentNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // valueInputTextBox
            // 
            this.valueInputTextBox.Location = new System.Drawing.Point(159, 5);
            this.valueInputTextBox.Name = "valueInputTextBox";
            this.valueInputTextBox.Size = new System.Drawing.Size(112, 26);
            this.valueInputTextBox.TabIndex = 1;
            // 
            // inputArgumentTypeLabel
            // 
            this.inputArgumentTypeLabel.Location = new System.Drawing.Point(279, 9);
            this.inputArgumentTypeLabel.Name = "inputArgumentTypeLabel";
            this.inputArgumentTypeLabel.Size = new System.Drawing.Size(150, 23);
            this.inputArgumentTypeLabel.TabIndex = 2;
            this.inputArgumentTypeLabel.Text = "Type";
            this.inputArgumentTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // inputArgumentDescriptionLabel
            // 
            this.inputArgumentDescriptionLabel.Location = new System.Drawing.Point(8, 49);
            this.inputArgumentDescriptionLabel.Name = "inputArgumentDescriptionLabel";
            this.inputArgumentDescriptionLabel.Size = new System.Drawing.Size(422, 28);
            this.inputArgumentDescriptionLabel.TabIndex = 1;
            this.inputArgumentDescriptionLabel.Text = "Argument Description";
            // 
            // ArgumentUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.inputArgumentDescriptionLabel);
            this.Controls.Add(this.inputArgumentTypeLabel);
            this.Controls.Add(this.valueInputTextBox);
            this.Controls.Add(this.inputArgumentNameLabel);
            this.MaximumSize = new System.Drawing.Size(435, 85);
            this.MinimumSize = new System.Drawing.Size(435, 85);
            this.Name = "ArgumentUserControl";
            this.Size = new System.Drawing.Size(435, 85);
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
