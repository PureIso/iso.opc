namespace Client
{
    partial class MainForm
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
            this.getDiscoveryServerTrustedListButton = new System.Windows.Forms.Button();
            this.customConnectionButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.globalDiscoveryServerUseSecurityCheckBox = new System.Windows.Forms.CheckBox();
            this.serverDiscoveryURLTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.serverUserNameTextBox = new System.Windows.Forms.TextBox();
            this.globalDiscoveryServerPasswordTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.serverPasswordTextBox = new System.Windows.Forms.TextBox();
            this.globalDiscoveryServerUserNameTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.useSecurityCheckBox = new System.Windows.Forms.CheckBox();
            this.globalDiscoveryServerDiscoveryURLTextBox = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.objectListView = new System.Windows.Forms.ListView();
            this.objectsHolumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.discoveredServersListView = new System.Windows.Forms.ListView();
            this.discoveredServersColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // getDiscoveryServerTrustedListButton
            // 
            this.getDiscoveryServerTrustedListButton.Location = new System.Drawing.Point(419, 172);
            this.getDiscoveryServerTrustedListButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.getDiscoveryServerTrustedListButton.Name = "getDiscoveryServerTrustedListButton";
            this.getDiscoveryServerTrustedListButton.Size = new System.Drawing.Size(403, 35);
            this.getDiscoveryServerTrustedListButton.TabIndex = 1;
            this.getDiscoveryServerTrustedListButton.Text = "Get Discovery Server Trusted List";
            this.getDiscoveryServerTrustedListButton.UseVisualStyleBackColor = true;
            this.getDiscoveryServerTrustedListButton.Click += new System.EventHandler(this.GetDiscoveryServerTrustedListButtonClick);
            // 
            // customConnectionButton
            // 
            this.customConnectionButton.Location = new System.Drawing.Point(8, 172);
            this.customConnectionButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.customConnectionButton.Name = "customConnectionButton";
            this.customConnectionButton.Size = new System.Drawing.Size(403, 35);
            this.customConnectionButton.TabIndex = 2;
            this.customConnectionButton.Text = "Custom Connection";
            this.customConnectionButton.UseVisualStyleBackColor = true;
            this.customConnectionButton.Click += new System.EventHandler(this.CustomConnectionButtonClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.globalDiscoveryServerUseSecurityCheckBox);
            this.groupBox1.Controls.Add(this.serverDiscoveryURLTextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.serverUserNameTextBox);
            this.groupBox1.Controls.Add(this.globalDiscoveryServerPasswordTextBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.serverPasswordTextBox);
            this.groupBox1.Controls.Add(this.globalDiscoveryServerUserNameTextBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.useSecurityCheckBox);
            this.groupBox1.Controls.Add(this.globalDiscoveryServerDiscoveryURLTextBox);
            this.groupBox1.Controls.Add(this.customConnectionButton);
            this.groupBox1.Controls.Add(this.getDiscoveryServerTrustedListButton);
            this.groupBox1.Location = new System.Drawing.Point(12, 13);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Size = new System.Drawing.Size(841, 216);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Custom Connection";
            // 
            // globalDiscoveryServerUseSecurityCheckBox
            // 
            this.globalDiscoveryServerUseSecurityCheckBox.AutoSize = true;
            this.globalDiscoveryServerUseSecurityCheckBox.Checked = true;
            this.globalDiscoveryServerUseSecurityCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.globalDiscoveryServerUseSecurityCheckBox.Location = new System.Drawing.Point(593, 62);
            this.globalDiscoveryServerUseSecurityCheckBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.globalDiscoveryServerUseSecurityCheckBox.Name = "globalDiscoveryServerUseSecurityCheckBox";
            this.globalDiscoveryServerUseSecurityCheckBox.Size = new System.Drawing.Size(134, 24);
            this.globalDiscoveryServerUseSecurityCheckBox.TabIndex = 13;
            this.globalDiscoveryServerUseSecurityCheckBox.Text = "Use Security?";
            this.globalDiscoveryServerUseSecurityCheckBox.UseVisualStyleBackColor = true;
            this.globalDiscoveryServerUseSecurityCheckBox.CheckedChanged += new System.EventHandler(this.GlobalDiscoveryServerUseSecurityCheckBoxCheckedChanged);
            // 
            // serverDiscoveryURLTextBox
            // 
            this.serverDiscoveryURLTextBox.Location = new System.Drawing.Point(183, 96);
            this.serverDiscoveryURLTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.serverDiscoveryURLTextBox.Name = "serverDiscoveryURLTextBox";
            this.serverDiscoveryURLTextBox.Size = new System.Drawing.Size(402, 26);
            this.serverDiscoveryURLTextBox.TabIndex = 0;
            this.serverDiscoveryURLTextBox.Text = "opc.tcp://localhost:48001/BasicServer";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 102);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(168, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Server Discovery URL:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(352, 66);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 20);
            this.label4.TabIndex = 12;
            this.label4.Text = "Password:";
            // 
            // serverUserNameTextBox
            // 
            this.serverUserNameTextBox.Location = new System.Drawing.Point(183, 131);
            this.serverUserNameTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.serverUserNameTextBox.Name = "serverUserNameTextBox";
            this.serverUserNameTextBox.Size = new System.Drawing.Size(151, 26);
            this.serverUserNameTextBox.TabIndex = 2;
            this.serverUserNameTextBox.Text = "sysadmin";
            // 
            // globalDiscoveryServerPasswordTextBox
            // 
            this.globalDiscoveryServerPasswordTextBox.Location = new System.Drawing.Point(441, 62);
            this.globalDiscoveryServerPasswordTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.globalDiscoveryServerPasswordTextBox.Name = "globalDiscoveryServerPasswordTextBox";
            this.globalDiscoveryServerPasswordTextBox.PasswordChar = '*';
            this.globalDiscoveryServerPasswordTextBox.Size = new System.Drawing.Size(144, 26);
            this.globalDiscoveryServerPasswordTextBox.TabIndex = 11;
            this.globalDiscoveryServerPasswordTextBox.Text = "demo";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(83, 135);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "User Name:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(82, 66);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(93, 20);
            this.label5.TabIndex = 10;
            this.label5.Text = "User Name:";
            // 
            // serverPasswordTextBox
            // 
            this.serverPasswordTextBox.Location = new System.Drawing.Point(442, 131);
            this.serverPasswordTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.serverPasswordTextBox.Name = "serverPasswordTextBox";
            this.serverPasswordTextBox.PasswordChar = '*';
            this.serverPasswordTextBox.Size = new System.Drawing.Size(144, 26);
            this.serverPasswordTextBox.TabIndex = 4;
            this.serverPasswordTextBox.Text = "demo";
            // 
            // globalDiscoveryServerUserNameTextBox
            // 
            this.globalDiscoveryServerUserNameTextBox.Location = new System.Drawing.Point(182, 62);
            this.globalDiscoveryServerUserNameTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.globalDiscoveryServerUserNameTextBox.Name = "globalDiscoveryServerUserNameTextBox";
            this.globalDiscoveryServerUserNameTextBox.Size = new System.Drawing.Size(151, 26);
            this.globalDiscoveryServerUserNameTextBox.TabIndex = 9;
            this.globalDiscoveryServerUserNameTextBox.Text = "appadmin";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(353, 135);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "Password:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 33);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(158, 20);
            this.label6.TabIndex = 8;
            this.label6.Text = "GDS Discovery URL:";
            // 
            // useSecurityCheckBox
            // 
            this.useSecurityCheckBox.AutoSize = true;
            this.useSecurityCheckBox.Checked = true;
            this.useSecurityCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useSecurityCheckBox.Location = new System.Drawing.Point(594, 131);
            this.useSecurityCheckBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.useSecurityCheckBox.Name = "useSecurityCheckBox";
            this.useSecurityCheckBox.Size = new System.Drawing.Size(134, 24);
            this.useSecurityCheckBox.TabIndex = 6;
            this.useSecurityCheckBox.Text = "Use Security?";
            this.useSecurityCheckBox.UseVisualStyleBackColor = true;
            this.useSecurityCheckBox.CheckedChanged += new System.EventHandler(this.UseSecurityCheckBoxCheckedChanged);
            // 
            // globalDiscoveryServerDiscoveryURLTextBox
            // 
            this.globalDiscoveryServerDiscoveryURLTextBox.Location = new System.Drawing.Point(182, 27);
            this.globalDiscoveryServerDiscoveryURLTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.globalDiscoveryServerDiscoveryURLTextBox.Name = "globalDiscoveryServerDiscoveryURLTextBox";
            this.globalDiscoveryServerDiscoveryURLTextBox.Size = new System.Drawing.Size(402, 26);
            this.globalDiscoveryServerDiscoveryURLTextBox.TabIndex = 7;
            this.globalDiscoveryServerDiscoveryURLTextBox.Text = "opc.tcp://localhost:58810/UADiscovery";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.objectListView);
            this.groupBox2.Location = new System.Drawing.Point(12, 378);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox2.Size = new System.Drawing.Size(841, 312);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Reference Descriptions";
            // 
            // objectListView
            // 
            this.objectListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.objectsHolumnHeader});
            this.objectListView.FullRowSelect = true;
            this.objectListView.GridLines = true;
            this.objectListView.HideSelection = false;
            this.objectListView.Location = new System.Drawing.Point(11, 27);
            this.objectListView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.objectListView.Name = "objectListView";
            this.objectListView.Size = new System.Drawing.Size(283, 277);
            this.objectListView.TabIndex = 0;
            this.objectListView.UseCompatibleStateImageBehavior = false;
            this.objectListView.View = System.Windows.Forms.View.Details;
            // 
            // objectsHolumnHeader
            // 
            this.objectsHolumnHeader.Text = "Objects";
            this.objectsHolumnHeader.Width = 223;
            // 
            // discoveredServersListView
            // 
            this.discoveredServersListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.discoveredServersColumnHeader});
            this.discoveredServersListView.FullRowSelect = true;
            this.discoveredServersListView.GridLines = true;
            this.discoveredServersListView.HideSelection = false;
            this.discoveredServersListView.Location = new System.Drawing.Point(12, 237);
            this.discoveredServersListView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.discoveredServersListView.Name = "discoveredServersListView";
            this.discoveredServersListView.Size = new System.Drawing.Size(841, 133);
            this.discoveredServersListView.TabIndex = 5;
            this.discoveredServersListView.UseCompatibleStateImageBehavior = false;
            this.discoveredServersListView.View = System.Windows.Forms.View.Details;
            // 
            // discoveredServersColumnHeader
            // 
            this.discoveredServersColumnHeader.Text = "Servers";
            this.discoveredServersColumnHeader.Width = 723;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(871, 703);
            this.Controls.Add(this.discoveredServersListView);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MainForm";
            this.Text = "Client";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button getDiscoveryServerTrustedListButton;
        private System.Windows.Forms.Button customConnectionButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox serverDiscoveryURLTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox serverPasswordTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox serverUserNameTextBox;
        private System.Windows.Forms.CheckBox useSecurityCheckBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListView objectListView;
        private System.Windows.Forms.ColumnHeader objectsHolumnHeader;
        private System.Windows.Forms.CheckBox globalDiscoveryServerUseSecurityCheckBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox globalDiscoveryServerPasswordTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox globalDiscoveryServerUserNameTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox globalDiscoveryServerDiscoveryURLTextBox;
        private System.Windows.Forms.ListView discoveredServersListView;
        private System.Windows.Forms.ColumnHeader discoveredServersColumnHeader;
    }
}

