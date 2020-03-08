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
            this.referenceGroupBox = new System.Windows.Forms.GroupBox();
            this.inputArgumentsPanel = new System.Windows.Forms.Panel();
            this.objectTreeView = new System.Windows.Forms.TreeView();
            this.discoveredServersListView = new System.Windows.Forms.ListView();
            this.discoveredServersColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.serverConnectContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.attributeGroupBox = new System.Windows.Forms.GroupBox();
            this.attributesListView = new System.Windows.Forms.ListView();
            this.attributeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.valueColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.referenceDescriptionContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.monitorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.callToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.informationRichTextBox = new System.Windows.Forms.RichTextBox();
            this.callMethodButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.referenceGroupBox.SuspendLayout();
            this.serverConnectContextMenuStrip.SuspendLayout();
            this.attributeGroupBox.SuspendLayout();
            this.mainMenuStrip.SuspendLayout();
            this.referenceDescriptionContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // getDiscoveryServerTrustedListButton
            // 
            this.getDiscoveryServerTrustedListButton.Location = new System.Drawing.Point(302, 22);
            this.getDiscoveryServerTrustedListButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.getDiscoveryServerTrustedListButton.Name = "getDiscoveryServerTrustedListButton";
            this.getDiscoveryServerTrustedListButton.Size = new System.Drawing.Size(428, 30);
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
            this.connectButton.Size = new System.Drawing.Size(338, 27);
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
            this.groupBox1.Location = new System.Drawing.Point(11, 30);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Size = new System.Drawing.Size(738, 210);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Custom Connection";
            // 
            // requestNewCertificateCheckBox
            // 
            this.requestNewCertificateCheckBox.AutoSize = true;
            this.requestNewCertificateCheckBox.Location = new System.Drawing.Point(392, 82);
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
            this.registerApplicationCheckBox.Location = new System.Drawing.Point(162, 82);
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
            this.globalDiscoveryServerConnectionStatusPanel.Size = new System.Drawing.Size(14, 12);
            this.globalDiscoveryServerConnectionStatusPanel.TabIndex = 17;
            // 
            // disconnectButton
            // 
            this.disconnectButton.Enabled = false;
            this.disconnectButton.Location = new System.Drawing.Point(392, 170);
            this.disconnectButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(338, 27);
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
            this.connectionStatusPanel.Size = new System.Drawing.Size(14, 12);
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
            this.serverDiscoveryURLTextBox.Location = new System.Drawing.Point(162, 110);
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
            this.serverUserNameTextBox.Location = new System.Drawing.Point(162, 138);
            this.serverUserNameTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.serverUserNameTextBox.Name = "serverUserNameTextBox";
            this.serverUserNameTextBox.Size = new System.Drawing.Size(135, 22);
            this.serverUserNameTextBox.TabIndex = 2;
            this.serverUserNameTextBox.Text = "sysadmin";
            // 
            // globalDiscoveryServerPasswordTextBox
            // 
            this.globalDiscoveryServerPasswordTextBox.Location = new System.Drawing.Point(392, 50);
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
            this.globalDiscoveryServerUserNameTextBox.Location = new System.Drawing.Point(162, 50);
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
            this.globalDiscoveryServerDiscoveryURLTextBox.Location = new System.Drawing.Point(162, 22);
            this.globalDiscoveryServerDiscoveryURLTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.globalDiscoveryServerDiscoveryURLTextBox.Name = "globalDiscoveryServerDiscoveryURLTextBox";
            this.globalDiscoveryServerDiscoveryURLTextBox.Size = new System.Drawing.Size(358, 22);
            this.globalDiscoveryServerDiscoveryURLTextBox.TabIndex = 7;
            this.globalDiscoveryServerDiscoveryURLTextBox.Text = "opc.tcp://localhost:58810/UADiscovery";
            // 
            // referenceGroupBox
            // 
            this.referenceGroupBox.Controls.Add(this.callMethodButton);
            this.referenceGroupBox.Controls.Add(this.inputArgumentsPanel);
            this.referenceGroupBox.Controls.Add(this.objectTreeView);
            this.referenceGroupBox.Controls.Add(this.getDiscoveryServerTrustedListButton);
            this.referenceGroupBox.Location = new System.Drawing.Point(11, 358);
            this.referenceGroupBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.referenceGroupBox.Name = "referenceGroupBox";
            this.referenceGroupBox.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.referenceGroupBox.Size = new System.Drawing.Size(738, 296);
            this.referenceGroupBox.TabIndex = 4;
            this.referenceGroupBox.TabStop = false;
            this.referenceGroupBox.Text = "Reference Descriptions";
            // 
            // inputArgumentsPanel
            // 
            this.inputArgumentsPanel.Location = new System.Drawing.Point(302, 60);
            this.inputArgumentsPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.inputArgumentsPanel.Name = "inputArgumentsPanel";
            this.inputArgumentsPanel.Size = new System.Drawing.Size(428, 188);
            this.inputArgumentsPanel.TabIndex = 3;
            // 
            // objectTreeView
            // 
            this.objectTreeView.Enabled = false;
            this.objectTreeView.Location = new System.Drawing.Point(12, 22);
            this.objectTreeView.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.objectTreeView.Name = "objectTreeView";
            this.objectTreeView.Size = new System.Drawing.Size(285, 268);
            this.objectTreeView.TabIndex = 2;
            this.objectTreeView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ObjectTreeViewMouseDoubleClick);
            this.objectTreeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ObjectTreeViewMouseDown);
            // 
            // discoveredServersListView
            // 
            this.discoveredServersListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.discoveredServersColumnHeader});
            this.discoveredServersListView.FullRowSelect = true;
            this.discoveredServersListView.GridLines = true;
            this.discoveredServersListView.HideSelection = false;
            this.discoveredServersListView.Location = new System.Drawing.Point(11, 249);
            this.discoveredServersListView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.discoveredServersListView.Name = "discoveredServersListView";
            this.discoveredServersListView.Size = new System.Drawing.Size(738, 105);
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
            // attributeGroupBox
            // 
            this.attributeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.attributeGroupBox.Controls.Add(this.attributesListView);
            this.attributeGroupBox.Location = new System.Drawing.Point(755, 30);
            this.attributeGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.attributeGroupBox.Name = "attributeGroupBox";
            this.attributeGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.attributeGroupBox.Size = new System.Drawing.Size(347, 624);
            this.attributeGroupBox.TabIndex = 6;
            this.attributeGroupBox.TabStop = false;
            this.attributeGroupBox.Text = "Attributes";
            // 
            // attributesListView
            // 
            this.attributesListView.Alignment = System.Windows.Forms.ListViewAlignment.SnapToGrid;
            this.attributesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.attributesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.attributeColumnHeader,
            this.valueColumnHeader});
            this.attributesListView.FullRowSelect = true;
            this.attributesListView.GridLines = true;
            this.attributesListView.HideSelection = false;
            this.attributesListView.Location = new System.Drawing.Point(7, 22);
            this.attributesListView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.attributesListView.Name = "attributesListView";
            this.attributesListView.Size = new System.Drawing.Size(330, 594);
            this.attributesListView.TabIndex = 6;
            this.attributesListView.UseCompatibleStateImageBehavior = false;
            this.attributesListView.View = System.Windows.Forms.View.Details;
            // 
            // attributeColumnHeader
            // 
            this.attributeColumnHeader.Text = "Attribute";
            this.attributeColumnHeader.Width = 179;
            // 
            // valueColumnHeader
            // 
            this.valueColumnHeader.Text = "Value";
            this.valueColumnHeader.Width = 135;
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.mainMenuStrip.Size = new System.Drawing.Size(1101, 28);
            this.mainMenuStrip.TabIndex = 7;
            this.mainMenuStrip.Text = "menuStrip1";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(62, 24);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // referenceDescriptionContextMenuStrip
            // 
            this.referenceDescriptionContextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.referenceDescriptionContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.monitorToolStripMenuItem,
            this.callToolStripMenuItem});
            this.referenceDescriptionContextMenuStrip.Name = "referenceDescriptionContextMenuStrip";
            this.referenceDescriptionContextMenuStrip.Size = new System.Drawing.Size(132, 52);
            // 
            // monitorToolStripMenuItem
            // 
            this.monitorToolStripMenuItem.Name = "monitorToolStripMenuItem";
            this.monitorToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.monitorToolStripMenuItem.Text = "Monitor";
            this.monitorToolStripMenuItem.Click += new System.EventHandler(this.MonitorToolStripMenuItemClick);
            // 
            // callToolStripMenuItem
            // 
            this.callToolStripMenuItem.Name = "callToolStripMenuItem";
            this.callToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.callToolStripMenuItem.Text = "Call";
            this.callToolStripMenuItem.Click += new System.EventHandler(this.CallToolStripMenuItemClick);
            // 
            // informationRichTextBox
            // 
            this.informationRichTextBox.BackColor = System.Drawing.SystemColors.InfoText;
            this.informationRichTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.informationRichTextBox.ForeColor = System.Drawing.Color.White;
            this.informationRichTextBox.Location = new System.Drawing.Point(0, 660);
            this.informationRichTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.informationRichTextBox.Name = "informationRichTextBox";
            this.informationRichTextBox.ReadOnly = true;
            this.informationRichTextBox.Size = new System.Drawing.Size(1101, 78);
            this.informationRichTextBox.TabIndex = 3;
            this.informationRichTextBox.Text = "";
            // 
            // callMethodButton
            // 
            this.callMethodButton.Enabled = false;
            this.callMethodButton.Location = new System.Drawing.Point(302, 260);
            this.callMethodButton.Margin = new System.Windows.Forms.Padding(4);
            this.callMethodButton.Name = "callMethodButton";
            this.callMethodButton.Size = new System.Drawing.Size(428, 30);
            this.callMethodButton.TabIndex = 4;
            this.callMethodButton.Text = "Call Method";
            this.callMethodButton.UseVisualStyleBackColor = true;
            this.callMethodButton.Click += new System.EventHandler(this.CallMethodButtonClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1101, 738);
            this.Controls.Add(this.informationRichTextBox);
            this.Controls.Add(this.mainMenuStrip);
            this.Controls.Add(this.attributeGroupBox);
            this.Controls.Add(this.discoveredServersListView);
            this.Controls.Add(this.referenceGroupBox);
            this.Controls.Add(this.groupBox1);
            this.MainMenuStrip = this.mainMenuStrip;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(1119, 785);
            this.Name = "MainForm";
            this.Text = "Client";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.referenceGroupBox.ResumeLayout(false);
            this.serverConnectContextMenuStrip.ResumeLayout(false);
            this.attributeGroupBox.ResumeLayout(false);
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.referenceDescriptionContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.GroupBox referenceGroupBox;
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
        private System.Windows.Forms.TreeView objectTreeView;
        private System.Windows.Forms.Panel connectionStatusPanel;
        private System.Windows.Forms.CheckBox globalDiscoveryServerUseCheckBox;
        private System.Windows.Forms.Button disconnectButton;
        private System.Windows.Forms.Panel globalDiscoveryServerConnectionStatusPanel;
        private System.Windows.Forms.CheckBox registerApplicationCheckBox;
        private System.Windows.Forms.CheckBox requestNewCertificateCheckBox;
        private System.Windows.Forms.GroupBox attributeGroupBox;
        private System.Windows.Forms.ListView attributesListView;
        private System.Windows.Forms.ColumnHeader attributeColumnHeader;
        private System.Windows.Forms.ColumnHeader valueColumnHeader;
        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip referenceDescriptionContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem monitorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem callToolStripMenuItem;
        private System.Windows.Forms.RichTextBox informationRichTextBox;
        private System.Windows.Forms.Panel inputArgumentsPanel;
        private System.Windows.Forms.Button callMethodButton;
    }
}

