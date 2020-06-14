using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Client.Properties;
using Iso.Opc.ApplicationManager;
using Iso.Opc.ApplicationManager.Models;
using Opc.Ua;
using Opc.Ua.Client;

namespace Client
{
    public partial class MainForm : Form
    {
        #region Constants

        private const string ApplicationName = "Basic Client";
        private const string ApplicationUri = "urn:localhost:UA:BasicClient";
        private readonly ApplicationType ApplicationType = ApplicationType.Client;

        #endregion

        #region Fields

        private List<TreeNode> _monitoredTreeNodes;
        private TreeNode _selectedTreeNode;
        private NodeId _selectedObjectId;
        private NodeId _selectedMethodId;
        private ApplicationInstanceManager _applicationInstanceManager;
        private readonly StringCollection _globalDiscoveryServerUrls;
        private readonly StringCollection _globalDiscoveryServerWellKnownUrls;

        #endregion

        #region Constructor

        public MainForm()
        {
            InitializeComponent();
            _globalDiscoveryServerUrls = new StringCollection {"opc.tcp://localhost:58810/UADiscovery"};
            _globalDiscoveryServerWellKnownUrls = new StringCollection {"opc.tcp://localhost:58810/UADiscovery"};
            _monitoredTreeNodes = new List<TreeNode>();
            // Load the images in an ImageList.
            ImageList imageList = new ImageList();
            imageList.Images.Add(Resources.folder_cog);
            imageList.Images.Add(Resources.folder_create);
            imageList.Images.Add(Resources.folder_magnifier);
            // Assign the ImageList to the TreeView.
            objectTreeView.ImageList = imageList;
        }

        #endregion

        #region Private Methods

        private void Connect(bool connectToServer = false)
        {
            try
            {
                connectButton.Enabled = false;
                disconnectButton.Enabled = true;
                objectTreeView.Nodes.Clear();
                objectTreeView.Enabled = false;
                if (!connectToServer)
                {
                    _applicationInstanceManager = new ApplicationInstanceManager(ApplicationName, ApplicationUri,
                        null, null, null, null, _globalDiscoveryServerUrls, _globalDiscoveryServerWellKnownUrls,
                        ApplicationType, true);
                }

                if (globalDiscoveryServerUseCheckBox.Checked && !connectToServer)
                {
                    globalDiscoveryServerConnectionStatusPanel.BackColor = Color.Red;
                    string gdsUserName = globalDiscoveryServerUseSecurityCheckBox.Checked
                        ? globalDiscoveryServerUserNameTextBox.Text
                        : null;
                    string gdsUserPassword = globalDiscoveryServerUseSecurityCheckBox.Checked
                        ? globalDiscoveryServerPasswordTextBox.Text
                        : null;
                    bool connectedToGDS =
                        _applicationInstanceManager.ConnectToGlobalDiscoveryServer(
                            globalDiscoveryServerDiscoveryURLTextBox.Text, gdsUserName, gdsUserPassword);
                    if (!connectedToGDS)
                    {
                        Disconnect();
                        return;
                    }

                    if (registerApplicationCheckBox.Checked)
                        _applicationInstanceManager.RegisterApplication();
                    if (requestNewCertificateCheckBox.Checked)
                        _applicationInstanceManager.RequestNewCertificatePullMode();
                    List<ServerOnNetwork> serversOnNetwork = _applicationInstanceManager.QueryServers();
                    discoveredServersListView.Items.Clear();
                    if (serversOnNetwork == null || !serversOnNetwork.Any())
                    {
                        Disconnect();
                        return;
                    }

                    ListViewItem[] discoveredServersListViewItems =
                        (from x in serversOnNetwork select new ListViewItem(x.DiscoveryUrl)).ToArray();
                    discoveredServersListView.Items.AddRange(discoveredServersListViewItems);
                    globalDiscoveryServerConnectionStatusPanel.BackColor = Color.Green;
                }

                if (!connectToServer && globalDiscoveryServerUseCheckBox.Checked)
                    return;
                connectionStatusPanel.BackColor = Color.Red;
                string userName = useSecurityCheckBox.Checked ? serverUserNameTextBox.Text : null;
                string userPassword = useSecurityCheckBox.Checked ? serverPasswordTextBox.Text : null;
                bool connectedToServer =
                    _applicationInstanceManager.ConnectToServer(serverDiscoveryURLTextBox.Text, userName, userPassword);
                if (!connectedToServer)
                    return;

                TreeNode[] browsedObjects =
                    {new TreeNode(Root.NameObjects), new TreeNode(Root.NameTypes), new TreeNode(Root.NameViews)};
                objectTreeView.Enabled = true;
                objectTreeView.Nodes.AddRange(browsedObjects);
                connectionStatusPanel.BackColor = Color.Green;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Disconnect();
            }
        }

        private void Disconnect()
        {
            connectButton.Enabled = true;
            _applicationInstanceManager?.Session?.Close();
            disconnectButton.Enabled = false;
            objectTreeView.Nodes.Clear();
            objectTreeView.Enabled = false;
            connectionStatusPanel.BackColor = Color.Red;
            globalDiscoveryServerConnectionStatusPanel.BackColor = Color.Red;
            _applicationInstanceManager = null;
        }

        private void PopulateAttributeListView(AttributeData attributeData)
        {
            ListViewItem[] items =
            {
                new ListViewItem("NodeId") {SubItems = {Convert.ToString(attributeData.NodeId)}},
                new ListViewItem("NodeClass") {SubItems = {Convert.ToString(attributeData.NodeClass)}},
                new ListViewItem("BrowseName") {SubItems = {Convert.ToString(attributeData.BrowseName)}},
                new ListViewItem("DisplayName") {SubItems = {Convert.ToString(attributeData.DisplayName)}},
                new ListViewItem("Description") {SubItems = {Convert.ToString(attributeData.Description)}},
                new ListViewItem("WriteMask") {SubItems = {Convert.ToString(attributeData.WriteMask)}},
                new ListViewItem("UserWriteMask") {SubItems = {Convert.ToString(attributeData.UserWriteMask)}},
                new ListViewItem("IsAbstract") {SubItems = {Convert.ToString(attributeData.IsAbstract)}},
                new ListViewItem("Symmetric") {SubItems = {Convert.ToString(attributeData.Symmetric)}},
                new ListViewItem("InverseName") {SubItems = {Convert.ToString(attributeData.InverseName)}},
                new ListViewItem("ContainsNoLoops") {SubItems = {Convert.ToString(attributeData.ContainsNoLoops)}},
                new ListViewItem("EventNotifier") {SubItems = {Convert.ToString(attributeData.EventNotifierString)}},
                new ListViewItem("Value") {SubItems = {Convert.ToString(attributeData.Value)}},
                new ListViewItem("DataType") {SubItems = {Convert.ToString(attributeData.DataType)}},
                new ListViewItem("ValueRank") {SubItems = {Convert.ToString(attributeData.ValueRankString)}},
                new ListViewItem("ArrayDimensions") {SubItems = {Convert.ToString(attributeData.ArrayDimensions)}},
                new ListViewItem("AccessLevel") {SubItems = {Convert.ToString(attributeData.AccessLevelString)}},
                new ListViewItem("UserAccessLevel")
                    {SubItems = {Convert.ToString(attributeData.UserAccessLevelString)}},
                new ListViewItem("MinimumSamplingInterval")
                    {SubItems = {Convert.ToString(attributeData.MinimumSamplingInterval)}},
                new ListViewItem("Historizing") {SubItems = {Convert.ToString(attributeData.Historizing)}},
                new ListViewItem("Executable") {SubItems = {Convert.ToString(attributeData.Executable)}},
                new ListViewItem("UserExecutable") {SubItems = {Convert.ToString(attributeData.UserExecutable)}},
                new ListViewItem("DataTypeDefinition")
                    {SubItems = {Convert.ToString(attributeData.DataTypeDefinition)}},
                new ListViewItem("RolePermissions") {SubItems = {Convert.ToString(attributeData.RolePermissions)}},
                new ListViewItem("UserRolePermissions")
                    {SubItems = {Convert.ToString(attributeData.UserRolePermissions)}},
                new ListViewItem("AccessRestrictions")
                    {SubItems = {Convert.ToString(attributeData.AccessRestrictions)}},
                new ListViewItem("AccessLevelEx") {SubItems = {Convert.ToString(attributeData.AccessLevelEx)}}
            };
            attributesListView.Items.AddRange(items);
        }

        private static void PopulateTreeNode(TreeNode parentNode, IEnumerable<TreeNode> children)
        {
            foreach (TreeNode child in children)
            {
                if (parentNode.Nodes.ContainsKey(child.Name))
                    continue;
                parentNode.Nodes.Add(child);
            }

            parentNode.Expand();
        }

        private void AddInputArgumentUserControl(string value, string description, string name, TypeInfo typeInfo)
        {
            ArgumentUserControl argumentUserControl = new ArgumentUserControl {Dock = DockStyle.Top};
            argumentUserControl.Initialise(value, description, name, typeInfo);
            inputArgumentsPanel.Controls.Add(argumentUserControl);
        }

        private void AddOutputArgumentUserControl(string value, string description, string name, TypeInfo typeInfo)
        {
            ArgumentUserControl argumentUserControl = new ArgumentUserControl {Dock = DockStyle.Top};
            argumentUserControl.Initialise(value, description, name, typeInfo);
            outputArgumentsPanel.Controls.Add(argumentUserControl);
        }
        private void AddMonitoredVariableUserControl(string value, string description, string name, TypeInfo typeInfo, NodeId nodeId)
        {
            ArgumentUserControl argumentUserControl = new ArgumentUserControl { Dock = DockStyle.Top };
            argumentUserControl.Initialise(value, description, name, typeInfo);
            argumentUserControl.Name = nodeId.ToString();
            monitoredVariablePanel.Controls.Add(argumentUserControl);
        }

        private void SetOutputArgumentValueForUserControl(List<object> values)
        {
            if (values.Count != outputArgumentsPanel.Controls.Count)
                return;
            for (int index = 0; index < outputArgumentsPanel.Controls.Count; index++)
            {
                ArgumentUserControl inputArgumentUserControl =
                    (ArgumentUserControl) outputArgumentsPanel.Controls[index];
                inputArgumentUserControl.ValueInput = values[index];
            }
        }
        private List<object> GetInputArgumentFromUserControl()
        {
            List<object> arguments = new List<object>();
            foreach (ArgumentUserControl inputArgumentUserControl in inputArgumentsPanel.Controls)
            {
                arguments.Add(inputArgumentUserControl.ValueInput);
            }
            return arguments;
        }

        private void MonitoredItemNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            try
            {
                if (e.NotificationValue is EventFieldList notification)
                {
                    
                    foreach (Variant notificationEventField in notification.EventFields)
                    {
                        if (notificationEventField.Value == null)
                            InformationDisplay($"Event Notification value: NULL");
                        else if (notificationEventField.TypeInfo.BuiltInType == BuiltInType.NodeId)
                        {
                            INode node = _applicationInstanceManager.Session.NodeCache.Find((NodeId)notificationEventField.Value);
                            if (node != null)
                            {
                                
                                InformationDisplay($"Event Notification [BuiltInType.NodeId] value: {node.ToString()}");
                                InformationDisplay($"Event Notification [Node Class] value: {node.NodeClass.ToString()}");
                                if (node.NodeClass == NodeClass.Method)
                                {
                                    ReferenceDescription referenceDescription = new ReferenceDescription();
                                    referenceDescription.NodeId = node.NodeId;
                                    AttributeData attributeData = _applicationInstanceManager.ReadAttributes(referenceDescription);
                                    InformationDisplay($"Event Notification [Attribute Browse Name] value: {attributeData.BrowseName.ToString()}");
                                    InformationDisplay($"Event Notification [Attribute Value] value: {attributeData.Value.ToString()}");
                                    //DataDescription inputDataDescription = methodReference.VariableDataDescriptions.FirstOrDefault(x =>
                                    //    x.AttributeData.BrowseName.Name == NameVariables.InputArguments);
                                    //DataDescription outputDataDescription = methodReference.VariableDataDescriptions.FirstOrDefault(x =>
                                    //    x.AttributeData.BrowseName.Name == NameVariables.OutputArguments);

                                    ////get all argument information
                                    //ExtensionObject[] inputExtensionObjects =
                                    //    (ExtensionObject[])inputDataDescription?.AttributeData.Value.Value;
                                    //ExtensionObject[] outputExtensionObjects =
                                    //    (ExtensionObject[])outputDataDescription?.AttributeData.Value.Value;
                                    //inputArgumentsPanel.Controls.Clear();
                                    //outputArgumentsPanel.Controls.Clear();
                                    //callMethodButton.Enabled = true;
                                    //if (inputExtensionObjects != null)
                                    //{
                                    //    foreach (ExtensionObject extensionObject in inputExtensionObjects)
                                    //    {
                                    //        Argument argument = (Argument)extensionObject.Body;
                                    //        Variant defaultValue = new Variant(TypeInfo.GetDefaultValue(argument.DataType, argument.ValueRank));
                                    //        if (defaultValue.Value == null)
                                    //            defaultValue.Value = "";
                                    //        AddInputArgumentUserControl(defaultValue.Value.ToString(), argument.Description.Text, argument.Name,
                                    //            defaultValue.TypeInfo);
                                    //    }
                                    //}
                                }
                            }
                        }
                        else if (notificationEventField.TypeInfo.BuiltInType == BuiltInType.Variant)
                        {
                            DateTime value = (DateTime)notificationEventField.Value;
                            InformationDisplay($"Event Notification [BuiltInType.Variant] value: {value.ToString()}");
                        }
                        else if (notificationEventField.TypeInfo.BuiltInType == BuiltInType.DateTime)
                        {
                            DateTime value = (DateTime)notificationEventField.Value;
                            InformationDisplay($"Event Notification [BuiltInType.DateTime] value: {value.ToString()}");
                            //if (m_filter.Fields[ii - 1].InstanceDeclaration.DisplayName.Contains("Time"))
                            //{
                            //    text = value.ToLocalTime().ToString("HH:mm:ss.fff");
                            //}
                            //else
                            //{
                            //    text = value.ToLocalTime().ToString("yyyy-MM-dd");
                            //}
                        }
                        else
                        {
                            InformationDisplay($"Event Notification [UNKNOWN] value: {notificationEventField.ToString()}");
                        }
                    }
                    
                }
                else if (e.NotificationValue is MonitoredItemNotification monitoredItemNotification)
                {
                    for (int index = 0; index < monitoredVariablePanel.Controls.Count; index++)
                    {
                        if (monitoredVariablePanel.Controls[index].Name != monitoredItem.ResolvedNodeId.ToString())
                            continue;
                        ArgumentUserControl argumentUserControl =
                            (ArgumentUserControl)monitoredVariablePanel.Controls[index];
                        argumentUserControl.ValueInput = monitoredItemNotification.Value.WrappedValue.ToString();
                    }
                }

               
            }
            catch (Exception ex)
            {
                InformationDisplay($"Monitored Item Notification exception: {ex.StackTrace}");
            }
        }

        private void InformationDisplay(string message)
        {
            if (informationRichTextBox.InvokeRequired)
                informationRichTextBox.Invoke(
                    new MethodInvoker(delegate { InformationDisplay(message); }));
            else
            {
                informationRichTextBox.AppendText($"[{DateTime.Now:dd/MM/yyyy hh:mm:ss}] {message}\n");
                informationRichTextBox.ScrollToCaret();
            }
        }

        #endregion

        #region Handlers
        private void DisconnectButtonClick(object sender, EventArgs e)
        {
            Disconnect();
        }
        private void ConnectButtonClick(object sender, EventArgs e)
        {
            Connect();
        }
        private void ConnectToolStripMenuItemClick(object sender, EventArgs e)
        {
            Connect(true);
        }
        private void DiscoveryServerTrustedListButtonClick(object sender, EventArgs e)
        {
            //TODO investigate why not all gds server certs are transferred
            _applicationInstanceManager.GetAndMergeWithGlobalDiscoveryTrustedList();
        }
        private void UseSecurityCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            serverUserNameTextBox.Enabled = useSecurityCheckBox.Checked;
            serverPasswordTextBox.Enabled = useSecurityCheckBox.Checked;
        }
        private void GlobalDiscoveryServerUseSecurityCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            globalDiscoveryServerUserNameTextBox.Enabled = globalDiscoveryServerUseSecurityCheckBox.Checked;
            globalDiscoveryServerPasswordTextBox.Enabled = globalDiscoveryServerUseSecurityCheckBox.Checked;
        }
        private void GlobalDiscoveryServerUseCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            globalDiscoveryServerDiscoveryURLTextBox.Enabled = globalDiscoveryServerUseCheckBox.Checked;
            globalDiscoveryServerUserNameTextBox.Enabled = globalDiscoveryServerUseCheckBox.Checked;
            globalDiscoveryServerPasswordTextBox.Enabled = globalDiscoveryServerUseCheckBox.Checked;
            globalDiscoveryServerUseSecurityCheckBox.Enabled = globalDiscoveryServerUseCheckBox.Checked;
            globalDiscoveryServerTrustedListButton.Enabled = globalDiscoveryServerUseCheckBox.Checked;

            serverUserNameTextBox.Enabled = !globalDiscoveryServerUseCheckBox.Checked;
            serverPasswordTextBox.Enabled = !globalDiscoveryServerUseCheckBox.Checked;
            serverDiscoveryURLTextBox.Enabled = !globalDiscoveryServerUseCheckBox.Checked;
            useSecurityCheckBox.Enabled = !globalDiscoveryServerUseCheckBox.Checked;
        }
        private void DiscoveredServersListViewMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            ListViewItem selectedItem = (sender as ListView)?.GetItemAt(e.X, e.Y);
            if (selectedItem == null)
                return;
            serverDiscoveryURLTextBox.Text = selectedItem.Text;
            Point point = (Point) ((ListView) sender)?.PointToScreen(e.Location);
            serverConnectContextMenuStrip.Show(point);
        }

        private void ObjectTreeViewMouseDown(object sender, MouseEventArgs e)
        {
            _selectedTreeNode = null;
            _selectedObjectId = null;
            _selectedMethodId = null;
            if (e.Button != MouseButtons.Right)
                return;
            //get selected node using pattern matching and null propagation
            _selectedTreeNode = (sender as TreeView)?.SelectedNode;
            if (!(_selectedTreeNode?.Tag is AttributeData attributeData))
                return;
            switch (attributeData.NodeClass)
            {
                case NodeClass.Variable:
                    monitorToolStripMenuItem.Enabled = true;
                    callToolStripMenuItem.Enabled = true;
                    break;
                case NodeClass.Method:
                {
                    monitorToolStripMenuItem.Enabled = true;
                    callToolStripMenuItem.Enabled = true;
                    break;
                }
                default:
                    // _selectedTreeNode = null;
                    monitorToolStripMenuItem.Enabled = true;
                    callToolStripMenuItem.Enabled = true;
                    break;
            }

            if (sender == null)
                return;
            Point point = (Point) ((TreeView) sender)?.PointToScreen(e.Location);
            referenceDescriptionContextMenuStrip.Show(point);
        }

        private void MonitorToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!(_selectedTreeNode?.Tag is AttributeData attributeData))
                return;
            if (_monitoredTreeNodes.Contains(_selectedTreeNode)) 
                return;
            _monitoredTreeNodes.Add(_selectedTreeNode);
            Variant defaultValue = new Variant(TypeInfo.GetDefaultValue(attributeData.DataType, attributeData.ValueRank));
            if (defaultValue.Value == null)
                defaultValue.Value = ""; 
            AddMonitoredVariableUserControl(defaultValue.Value.ToString(), attributeData.Description.Text, 
                attributeData.DisplayName.Text, defaultValue.TypeInfo, attributeData.NodeId);
            _applicationInstanceManager.SubscribeToNode(attributeData.NodeId, MonitoredItemNotification, 500);
        }

        private void CallToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!(_selectedTreeNode?.Tag is AttributeData attributeData))
                return;
            //since we have a method we need to validate the arguments and object
            //get parent attribute data
            TreeNode selectedParentNode = _selectedTreeNode.Parent;
            if (!(selectedParentNode?.Tag is AttributeData parentAttributeData))
                return;
            _selectedObjectId = parentAttributeData.NodeId;
            _selectedMethodId = attributeData.NodeId;
            ExtendedDataDescription methodReference =
                _applicationInstanceManager.FlatExtendedDataDescriptionDictionary[attributeData.BrowseName.Name];
            //extract input arguments
            DataDescription inputDataDescription = methodReference.VariableDataDescriptions.FirstOrDefault(x =>
                x.AttributeData.BrowseName.Name == NameVariables.InputArguments);
            DataDescription outputDataDescription = methodReference.VariableDataDescriptions.FirstOrDefault(x =>
                x.AttributeData.BrowseName.Name == NameVariables.OutputArguments);
            //get all argument information
            ExtensionObject[] inputExtensionObjects =
                (ExtensionObject[]) inputDataDescription?.AttributeData.Value.Value;
            ExtensionObject[] outputExtensionObjects =
                (ExtensionObject[]) outputDataDescription?.AttributeData.Value.Value;
            inputArgumentsPanel.Controls.Clear();
            outputArgumentsPanel.Controls.Clear();
            callMethodButton.Enabled = true;
            if (inputExtensionObjects != null)
            {
                foreach (ExtensionObject extensionObject in inputExtensionObjects)
                {
                    Argument argument = (Argument) extensionObject.Body;
                    Variant defaultValue = new Variant(TypeInfo.GetDefaultValue(argument.DataType, argument.ValueRank));
                    if (defaultValue.Value == null)
                        defaultValue.Value = "";
                    AddInputArgumentUserControl(defaultValue.Value.ToString(), argument.Description.Text, argument.Name,
                        defaultValue.TypeInfo);
                }
            }
            if (outputExtensionObjects == null)
                return;
            foreach (ExtensionObject extensionObject in outputExtensionObjects)
            {
                Argument argument = (Argument) extensionObject.Body;
                Variant defaultValue = new Variant(TypeInfo.GetDefaultValue(argument.DataType, argument.ValueRank));
                AddOutputArgumentUserControl(defaultValue.Value.ToString(), argument.Description.Text, argument.Name,
                    defaultValue.TypeInfo);
            }
        }

        private void ObjectTreeViewMouseDoubleClick(object sender, MouseEventArgs e)
        {
            attributesListView.Items.Clear();
            TreeNode parentNode = objectTreeView.SelectedNode;
            if (parentNode == null)
                return;
            ExtendedDataDescription objectReference = null;
            if (_applicationInstanceManager.FlatExtendedDataDescriptionDictionary.ContainsKey(parentNode.Text))
            {
                objectReference = _applicationInstanceManager.FlatExtendedDataDescriptionDictionary[parentNode.Text];
            }
            if (objectReference == null)
                return;
            PopulateAttributeListView(objectReference.DataDescription.AttributeData);
            TreeNode[] browsedObjects;
            if (objectReference.MethodDataDescriptions != null)
            {
                browsedObjects = (from x in objectReference.MethodDataDescriptions
                    select new TreeNode(x.DataDescription.ReferenceDescription.BrowseName.Name, 0, 0)
                    {
                        Name = x.DataDescription.ReferenceDescription.BrowseName.Name,
                        Tag = x.DataDescription.AttributeData
                    }).ToArray();
                PopulateTreeNode(parentNode, browsedObjects);
            }

            if (objectReference.VariableDataDescriptions != null)
            {
                browsedObjects = (from x in objectReference.VariableDataDescriptions
                    select new TreeNode(x.ReferenceDescription.BrowseName.Name, 1, 1)
                    {
                        Name = x.ReferenceDescription.BrowseName.Name,
                        Tag = x.AttributeData
                    }).ToArray();
                PopulateTreeNode(parentNode, browsedObjects);
            }

            if (objectReference.ObjectDataDescriptions == null)
                return;
            browsedObjects = (from x in objectReference.ObjectDataDescriptions
                select new TreeNode(x.DataDescription.ReferenceDescription.BrowseName.Name, 2, 2)
                {
                    Name = x.DataDescription.ReferenceDescription.BrowseName.Name,
                    Tag = x.DataDescription.AttributeData
                }).ToArray();
            PopulateTreeNode(parentNode, browsedObjects);
        }

        private void CallMethodButtonClick(object sender, EventArgs e)
        {
            if (_selectedObjectId == null || _selectedMethodId == null)
                return;
            List<object> arguments = GetInputArgumentFromUserControl();
            IList<object> outputArguments =
                _applicationInstanceManager.Session.Call(_selectedObjectId, _selectedMethodId, arguments.Count >0?arguments.ToArray():null);
            SetOutputArgumentValueForUserControl(outputArguments.ToList());
        }
        #endregion
    }
}