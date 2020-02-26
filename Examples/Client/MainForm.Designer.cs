﻿namespace Client
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
            this.components = new System.ComponentModel.Container();
            this.getDiscoveryServerTrustedListButton = new System.Windows.Forms.Button();
            this.connectButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.requestNewCertificateCheckBox = new System.Windows.Forms.CheckBox();
            this.registerApplicationCheckBox = new System.Windows.Forms.CheckBox();
            this.globalDiscoveryServerConnectionStatusPanel = new System.Windows.Forms.Panel();
            this.disconnectButton = new System.Windows.Forms.Button();
            this.globalDiscoveryServerUseCheckBox = new System.Windows.Forms.CheckBox();
            this.connectionStatusPanel = new System.Windows.Forms.Panel();
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
            this.testOutputTextBox = new System.Windows.Forms.TextBox();
            this.objectTreeView = new System.Windows.Forms.TreeView();
            this.button1 = new System.Windows.Forms.Button();
            this.discoveredServersListView = new System.Windows.Forms.ListView();
            this.discoveredServersColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.serverConnectContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.serverConnectContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // getDiscoveryServerTrustedListButton
            // 
            this.getDiscoveryServerTrustedListButton.Location = new System.Drawing.Point(301, 293);
            this.getDiscoveryServerTrustedListButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.getDiscoveryServerTrustedListButton.Name = "getDiscoveryServerTrustedListButton";
            this.getDiscoveryServerTrustedListButton.Size = new System.Drawing.Size(336, 31);
            this.getDiscoveryServerTrustedListButton.TabIndex = 1;
            this.getDiscoveryServerTrustedListButton.Text = "Get Discovery Server Trusted List";
            this.getDiscoveryServerTrustedListButton.UseVisualStyleBackColor = true;
            this.getDiscoveryServerTrustedListButton.Click += new System.EventHandler(this.GetDiscoveryServerTrustedListButtonClick);
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(5, 170);
            this.connectButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(337, 27);
            this.connectButton.TabIndex = 2;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.ConnectButtonClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.requestNewCertificateCheckBox);
            this.groupBox1.Controls.Add(this.registerApplicationCheckBox);
            this.groupBox1.Controls.Add(this.globalDiscoveryServerConnectionStatusPanel);
            this.groupBox1.Controls.Add(this.disconnectButton);
            this.groupBox1.Controls.Add(this.globalDiscoveryServerUseCheckBox);
            this.groupBox1.Controls.Add(this.connectionStatusPanel);
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
            this.groupBox1.Controls.Add(this.connectButton);
            this.groupBox1.Location = new System.Drawing.Point(11, 10);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Size = new System.Drawing.Size(752, 210);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Custom Connection";
            // 
            // requestNewCertificateCheckBox
            // 
            this.requestNewCertificateCheckBox.AutoSize = true;
            this.requestNewCertificateCheckBox.Location = new System.Drawing.Point(392, 81);
            this.requestNewCertificateCheckBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.requestNewCertificateCheckBox.Name = "requestNewCertificateCheckBox";
            this.requestNewCertificateCheckBox.Size = new System.Drawing.Size(185, 21);
            this.requestNewCertificateCheckBox.TabIndex = 16;
            this.requestNewCertificateCheckBox.Text = "Request new certificate?";
            this.requestNewCertificateCheckBox.UseVisualStyleBackColor = true;
            // 
            // registerApplicationCheckBox
            // 
            this.registerApplicationCheckBox.AutoSize = true;
            this.registerApplicationCheckBox.Location = new System.Drawing.Point(161, 81);
            this.registerApplicationCheckBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.registerApplicationCheckBox.Name = "registerApplicationCheckBox";
            this.registerApplicationCheckBox.Size = new System.Drawing.Size(164, 21);
            this.registerApplicationCheckBox.TabIndex = 17;
            this.registerApplicationCheckBox.Text = "Register Application?";
            this.registerApplicationCheckBox.UseVisualStyleBackColor = true;
            // 
            // globalDiscoveryServerConnectionStatusPanel
            // 
            this.globalDiscoveryServerConnectionStatusPanel.BackColor = System.Drawing.Color.Red;
            this.globalDiscoveryServerConnectionStatusPanel.Location = new System.Drawing.Point(525, 30);
            this.globalDiscoveryServerConnectionStatusPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.globalDiscoveryServerConnectionStatusPanel.Name = "globalDiscoveryServerConnectionStatusPanel";
            this.globalDiscoveryServerConnectionStatusPanel.Size = new System.Drawing.Size(15, 12);
            this.globalDiscoveryServerConnectionStatusPanel.TabIndex = 17;
            // 
            // disconnectButton
            // 
            this.disconnectButton.Enabled = false;
            this.disconnectButton.Location = new System.Drawing.Point(392, 170);
            this.disconnectButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(337, 27);
            this.disconnectButton.TabIndex = 16;
            this.disconnectButton.Text = "Disconnect";
            this.disconnectButton.UseVisualStyleBackColor = true;
            this.disconnectButton.Click += new System.EventHandler(this.DisconnectButtonClick);
            // 
            // globalDiscoveryServerUseCheckBox
            // 
            this.globalDiscoveryServerUseCheckBox.AutoSize = true;
            this.globalDiscoveryServerUseCheckBox.Checked = true;
            this.globalDiscoveryServerUseCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.globalDiscoveryServerUseCheckBox.Location = new System.Drawing.Point(599, 26);
            this.globalDiscoveryServerUseCheckBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.globalDiscoveryServerUseCheckBox.Name = "globalDiscoveryServerUseCheckBox";
            this.globalDiscoveryServerUseCheckBox.Size = new System.Drawing.Size(97, 21);
            this.globalDiscoveryServerUseCheckBox.TabIndex = 15;
            this.globalDiscoveryServerUseCheckBox.Text = "Use GDS?";
            this.globalDiscoveryServerUseCheckBox.UseVisualStyleBackColor = true;
            this.globalDiscoveryServerUseCheckBox.CheckedChanged += new System.EventHandler(this.GlobalDiscoveryServerUseCheckBoxCheckedChanged);
            // 
            // connectionStatusPanel
            // 
            this.connectionStatusPanel.BackColor = System.Drawing.Color.Red;
            this.connectionStatusPanel.Location = new System.Drawing.Point(524, 118);
            this.connectionStatusPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.connectionStatusPanel.Name = "connectionStatusPanel";
            this.connectionStatusPanel.Size = new System.Drawing.Size(15, 12);
            this.connectionStatusPanel.TabIndex = 14;
            // 
            // globalDiscoveryServerUseSecurityCheckBox
            // 
            this.globalDiscoveryServerUseSecurityCheckBox.AutoSize = true;
            this.globalDiscoveryServerUseSecurityCheckBox.Checked = true;
            this.globalDiscoveryServerUseSecurityCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.globalDiscoveryServerUseSecurityCheckBox.Location = new System.Drawing.Point(599, 53);
            this.globalDiscoveryServerUseSecurityCheckBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.globalDiscoveryServerUseSecurityCheckBox.Name = "globalDiscoveryServerUseSecurityCheckBox";
            this.globalDiscoveryServerUseSecurityCheckBox.Size = new System.Drawing.Size(118, 21);
            this.globalDiscoveryServerUseSecurityCheckBox.TabIndex = 13;
            this.globalDiscoveryServerUseSecurityCheckBox.Text = "Use Security?";
            this.globalDiscoveryServerUseSecurityCheckBox.UseVisualStyleBackColor = true;
            this.globalDiscoveryServerUseSecurityCheckBox.CheckedChanged += new System.EventHandler(this.GlobalDiscoveryServerUseSecurityCheckBoxCheckedChanged);
            // 
            // serverDiscoveryURLTextBox
            // 
            this.serverDiscoveryURLTextBox.Enabled = false;
            this.serverDiscoveryURLTextBox.Location = new System.Drawing.Point(161, 110);
            this.serverDiscoveryURLTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.serverDiscoveryURLTextBox.Name = "serverDiscoveryURLTextBox";
            this.serverDiscoveryURLTextBox.Size = new System.Drawing.Size(356, 22);
            this.serverDiscoveryURLTextBox.TabIndex = 0;
            this.serverDiscoveryURLTextBox.Text = "opc.tcp://localhost:48001/BasicServer";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 114);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(152, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Server Discovery URL:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(313, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(73, 17);
            this.label4.TabIndex = 12;
            this.label4.Text = "Password:";
            // 
            // serverUserNameTextBox
            // 
            this.serverUserNameTextBox.Enabled = false;
            this.serverUserNameTextBox.Location = new System.Drawing.Point(161, 138);
            this.serverUserNameTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.serverUserNameTextBox.Name = "serverUserNameTextBox";
            this.serverUserNameTextBox.Size = new System.Drawing.Size(135, 22);
            this.serverUserNameTextBox.TabIndex = 2;
            this.serverUserNameTextBox.Text = "sysadmin";
            // 
            // globalDiscoveryServerPasswordTextBox
            // 
            this.globalDiscoveryServerPasswordTextBox.Location = new System.Drawing.Point(392, 49);
            this.globalDiscoveryServerPasswordTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.globalDiscoveryServerPasswordTextBox.Name = "globalDiscoveryServerPasswordTextBox";
            this.globalDiscoveryServerPasswordTextBox.PasswordChar = '*';
            this.globalDiscoveryServerPasswordTextBox.Size = new System.Drawing.Size(127, 22);
            this.globalDiscoveryServerPasswordTextBox.TabIndex = 11;
            this.globalDiscoveryServerPasswordTextBox.Text = "demo";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(72, 142);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "User Name:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(73, 53);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 17);
            this.label5.TabIndex = 10;
            this.label5.Text = "User Name:";
            // 
            // serverPasswordTextBox
            // 
            this.serverPasswordTextBox.Enabled = false;
            this.serverPasswordTextBox.Location = new System.Drawing.Point(392, 138);
            this.serverPasswordTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.serverPasswordTextBox.Name = "serverPasswordTextBox";
            this.serverPasswordTextBox.PasswordChar = '*';
            this.serverPasswordTextBox.Size = new System.Drawing.Size(125, 22);
            this.serverPasswordTextBox.TabIndex = 4;
            this.serverPasswordTextBox.Text = "demo";
            // 
            // globalDiscoveryServerUserNameTextBox
            // 
            this.globalDiscoveryServerUserNameTextBox.Location = new System.Drawing.Point(161, 49);
            this.globalDiscoveryServerUserNameTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.globalDiscoveryServerUserNameTextBox.Name = "globalDiscoveryServerUserNameTextBox";
            this.globalDiscoveryServerUserNameTextBox.Size = new System.Drawing.Size(135, 22);
            this.globalDiscoveryServerUserNameTextBox.TabIndex = 9;
            this.globalDiscoveryServerUserNameTextBox.Text = "appadmin";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(312, 142);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 17);
            this.label3.TabIndex = 5;
            this.label3.Text = "Password:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(140, 17);
            this.label6.TabIndex = 8;
            this.label6.Text = "GDS Discovery URL:";
            // 
            // useSecurityCheckBox
            // 
            this.useSecurityCheckBox.AutoSize = true;
            this.useSecurityCheckBox.Checked = true;
            this.useSecurityCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useSecurityCheckBox.Enabled = false;
            this.useSecurityCheckBox.Location = new System.Drawing.Point(597, 142);
            this.useSecurityCheckBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.useSecurityCheckBox.Name = "useSecurityCheckBox";
            this.useSecurityCheckBox.Size = new System.Drawing.Size(118, 21);
            this.useSecurityCheckBox.TabIndex = 6;
            this.useSecurityCheckBox.Text = "Use Security?";
            this.useSecurityCheckBox.UseVisualStyleBackColor = true;
            this.useSecurityCheckBox.CheckedChanged += new System.EventHandler(this.UseSecurityCheckBoxCheckedChanged);
            // 
            // globalDiscoveryServerDiscoveryURLTextBox
            // 
            this.globalDiscoveryServerDiscoveryURLTextBox.Location = new System.Drawing.Point(161, 22);
            this.globalDiscoveryServerDiscoveryURLTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.globalDiscoveryServerDiscoveryURLTextBox.Name = "globalDiscoveryServerDiscoveryURLTextBox";
            this.globalDiscoveryServerDiscoveryURLTextBox.Size = new System.Drawing.Size(357, 22);
            this.globalDiscoveryServerDiscoveryURLTextBox.TabIndex = 7;
            this.globalDiscoveryServerDiscoveryURLTextBox.Text = "opc.tcp://localhost:58810/UADiscovery";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.testOutputTextBox);
            this.groupBox2.Controls.Add(this.objectTreeView);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.getDiscoveryServerTrustedListButton);
            this.groupBox2.Location = new System.Drawing.Point(11, 341);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox2.Size = new System.Drawing.Size(752, 331);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Reference Descriptions";
            // 
            // testOutputTextBox
            // 
            this.testOutputTextBox.Location = new System.Drawing.Point(304, 60);
            this.testOutputTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.testOutputTextBox.Multiline = true;
            this.testOutputTextBox.Name = "testOutputTextBox";
            this.testOutputTextBox.Size = new System.Drawing.Size(333, 121);
            this.testOutputTextBox.TabIndex = 3;
            // 
            // objectTreeView
            // 
            this.objectTreeView.Enabled = false;
            this.objectTreeView.Location = new System.Drawing.Point(9, 22);
            this.objectTreeView.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.objectTreeView.Name = "objectTreeView";
            this.objectTreeView.Size = new System.Drawing.Size(285, 302);
            this.objectTreeView.TabIndex = 2;
            this.objectTreeView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ObjectTreeViewMouseDoubleClick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(301, 22);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(337, 31);
            this.button1.TabIndex = 1;
            this.button1.Text = "Test";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // discoveredServersListView
            // 
            this.discoveredServersListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.discoveredServersColumnHeader});
            this.discoveredServersListView.FullRowSelect = true;
            this.discoveredServersListView.GridLines = true;
            this.discoveredServersListView.HideSelection = false;
            this.discoveredServersListView.Location = new System.Drawing.Point(11, 228);
            this.discoveredServersListView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.discoveredServersListView.Name = "discoveredServersListView";
            this.discoveredServersListView.Size = new System.Drawing.Size(751, 105);
            this.discoveredServersListView.TabIndex = 5;
            this.discoveredServersListView.UseCompatibleStateImageBehavior = false;
            this.discoveredServersListView.View = System.Windows.Forms.View.Details;
            this.discoveredServersListView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DiscoveredServersListViewMouseDown);
            // 
            // discoveredServersColumnHeader
            // 
            this.discoveredServersColumnHeader.Text = "Servers";
            this.discoveredServersColumnHeader.Width = 492;
            // 
            // serverConnectContextMenuStrip
            // 
            this.serverConnectContextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.serverConnectContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToolStripMenuItem});
            this.serverConnectContextMenuStrip.Name = "referenceContextMenuStrip";
            this.serverConnectContextMenuStrip.Size = new System.Drawing.Size(133, 28);
            // 
            // connectToolStripMenuItem
            // 
            this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            this.connectToolStripMenuItem.Size = new System.Drawing.Size(132, 24);
            this.connectToolStripMenuItem.Text = "Connect";
            this.connectToolStripMenuItem.Click += new System.EventHandler(this.ConnectToolStripMenuItemClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(773, 687);
            this.Controls.Add(this.discoveredServersListView);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "MainForm";
            this.Text = "Client";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.serverConnectContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button getDiscoveryServerTrustedListButton;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox serverDiscoveryURLTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox serverPasswordTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox serverUserNameTextBox;
        private System.Windows.Forms.CheckBox useSecurityCheckBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox globalDiscoveryServerUseSecurityCheckBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox globalDiscoveryServerPasswordTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox globalDiscoveryServerUserNameTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox globalDiscoveryServerDiscoveryURLTextBox;
        private System.Windows.Forms.ListView discoveredServersListView;
        private System.Windows.Forms.ColumnHeader discoveredServersColumnHeader;
        private System.Windows.Forms.ContextMenuStrip serverConnectContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TreeView objectTreeView;
        private System.Windows.Forms.Panel connectionStatusPanel;
        private System.Windows.Forms.CheckBox globalDiscoveryServerUseCheckBox;
        private System.Windows.Forms.Button disconnectButton;
        private System.Windows.Forms.Panel globalDiscoveryServerConnectionStatusPanel;
        private System.Windows.Forms.CheckBox registerApplicationCheckBox;
        private System.Windows.Forms.CheckBox requestNewCertificateCheckBox;
        private System.Windows.Forms.TextBox testOutputTextBox;
    }
}

