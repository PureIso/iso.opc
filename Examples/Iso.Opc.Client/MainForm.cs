using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Iso.Opc.Client.Properties;
using Iso.Opc.Core;
using Iso.Opc.Core.Models;
using Opc.Ua;
using Opc.Ua.Client;

namespace Iso.Opc.Client
{
    public partial class MainForm : Form
    {
        #region Constants
        private const string ApplicationName = "Basic Client";
        private const string ApplicationUri = "urn:localhost:UA:BasicClient";
        private readonly ApplicationType _applicationType = ApplicationType.Client;
        #endregion

        #region Fields
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
                        _applicationType, true);
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
                _applicationInstanceManager.GetRootExtendedDataDescriptions();
                if (!connectedToServer)
                    return;

                List<TreeNode> browsedObjects = new List<TreeNode>();
                if(_applicationInstanceManager.FlatExtendedDataDescriptionDictionary.ContainsKey(ObjectIds.ObjectsFolder.Identifier.ToString()))
                {
                    ExtendedDataDescription objectReference = _applicationInstanceManager.FlatExtendedDataDescriptionDictionary[ObjectIds.ObjectsFolder.Identifier.ToString()];
                    browsedObjects.Add(new TreeNode()
                    {
                        Text = objectReference.DataDescription.AttributeData.BrowseName.Name,
                        Tag = objectReference.DataDescription.AttributeData
                    });
                }
                if (_applicationInstanceManager.FlatExtendedDataDescriptionDictionary.ContainsKey(ObjectIds.TypesFolder.Identifier.ToString()))
                {
                    ExtendedDataDescription typesReference = _applicationInstanceManager.FlatExtendedDataDescriptionDictionary[ObjectIds.TypesFolder.Identifier.ToString()];
                    browsedObjects.Add(new TreeNode()
                    {
                        Text = typesReference.DataDescription.AttributeData.BrowseName.Name,
                        Tag = typesReference.DataDescription.AttributeData
                    });
                }
                if (_applicationInstanceManager.FlatExtendedDataDescriptionDictionary.ContainsKey(ObjectIds.ViewsFolder.Identifier.ToString()))
                {
                    ExtendedDataDescription viewReference = _applicationInstanceManager.FlatExtendedDataDescriptionDictionary[ObjectIds.ViewsFolder.Identifier.ToString()];
                    browsedObjects.Add(new TreeNode()
                    {
                        Text = viewReference.DataDescription.AttributeData.BrowseName.Name,
                        Tag = viewReference.DataDescription.AttributeData
                    });
                }
                objectTreeView.Enabled = true;
                objectTreeView.Nodes.AddRange(browsedObjects.ToArray());
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
        private static readonly NodeId[] KnownEventTypes = new[]
        {
            ObjectTypeIds.BaseEventType,
            ObjectTypeIds.ConditionType,
            ObjectTypeIds.DialogConditionType,
            ObjectTypeIds.AlarmConditionType,
            ObjectTypeIds.ExclusiveLimitAlarmType,
            ObjectTypeIds.NonExclusiveLimitAlarmType,
            ObjectTypeIds.AuditEventType,
            ObjectTypeIds.AuditUpdateMethodEventType
        };
        private void MonitoredItemNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        { 
            try
            {
                if (!(e.NotificationValue is MonitoredItemNotification monitoredItemNotification))
                    return;
                for (int index = 0; index < monitoredVariablePanel.Controls.Count; index++)
                {
                    if (monitoredVariablePanel.Controls[index].Name != monitoredItem.ResolvedNodeId.ToString())
                        continue;
                    ArgumentUserControl argumentUserControl =
                        (ArgumentUserControl)monitoredVariablePanel.Controls[index];
                    argumentUserControl.ValueInput = monitoredItemNotification.Value.WrappedValue.ToString();
                }
            }
            catch (Exception ex)
            {
                InformationDisplay($"Monitored Item Notification exception: {ex.StackTrace}");
            }
        }
        private void MonitorMethodUpdateNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            try
            {
                if (!(e.NotificationValue is EventFieldList notification)) 
                    return;
                NodeId eventTypeId = null;
                if (!(monitoredItem.Status.Filter is EventFilter filter))
                    return;
                for (int index = 0; index < filter.SelectClauses.Count; index++)
                {
                    SimpleAttributeOperand simpleAttributeOperand = filter.SelectClauses[index];
                    if (simpleAttributeOperand.BrowsePath.Count != 1 ||
                        simpleAttributeOperand.BrowsePath[0] != BrowseNames.EventType)
                        continue;
                    eventTypeId = notification.EventFields[index].Value as NodeId;
                }

                // look up the known event type.
                Dictionary<NodeId, NodeId> eventTypeMappings = new Dictionary<NodeId, NodeId>();
                if (eventTypeId == null || NodeId.IsNull(eventTypeId))
                    return;
                if (!eventTypeMappings.TryGetValue(eventTypeId, out NodeId knownTypeId))
                {
                    // check for a known type
                    if (KnownEventTypes.Any(nodeId => nodeId == eventTypeId))
                    {
                        knownTypeId = eventTypeId;
                        eventTypeMappings.Add(eventTypeId, eventTypeId);
                    }

                    // browse for the supertypes of the event type.
                    if (knownTypeId == null)
                    {
                        ReferenceDescriptionCollection supertypes = new ReferenceDescriptionCollection();
                        // find all of the children of the field.
                        BrowseDescription nodeToBrowse = new BrowseDescription
                        {
                            NodeId = eventTypeId,
                            BrowseDirection = BrowseDirection.Inverse,
                            ReferenceTypeId = ReferenceTypeIds.HasSubtype,
                            IncludeSubtypes = false, // more efficient to use IncludeSubtypes=False when possible.
                            NodeClassMask = 0, // the HasSubtype reference already restricts the targets to Types. 
                            ResultMask = (uint) BrowseResultMask.All
                        };

                        ReferenceDescriptionCollection
                            references = _applicationInstanceManager.Browse(nodeToBrowse);
                        while (references != null && references.Count > 0)
                        {
                            // should never be more than one supertype.
                            supertypes.Add(references[0]);
                            // only follow references within this server.
                            if (references[0].NodeId.IsAbsolute)
                            {
                                break;
                            }

                            // get the references for the next level up.
                            nodeToBrowse.NodeId = (NodeId) references[0].NodeId;
                            references = _applicationInstanceManager.Browse(nodeToBrowse);
                        }

                        // find the first super type that matches a known event type.
                        foreach (ReferenceDescription referenceDescription in supertypes)
                        {
                            foreach (NodeId nodeId in KnownEventTypes)
                            {
                                if (nodeId != referenceDescription.NodeId)
                                    continue;
                                knownTypeId = nodeId;
                                eventTypeMappings.Add(eventTypeId, knownTypeId);
                                break;
                            }

                            if (knownTypeId != null)
                                break;
                        }
                    }
                }

                if (knownTypeId == null)
                    return;
                // all of the known event types have a UInt32 as identifier.
                uint? id = knownTypeId.Identifier as uint?;
                if (id == null)
                    return;
                // construct the event based on the known event type.
                BaseEventState baseEventState = null;

                switch (id.Value)
                {
                    case ObjectTypes.ConditionType:
                    {
                        baseEventState = new ConditionState(null);
                        break;
                    }
                    case ObjectTypes.DialogConditionType:
                    {
                        baseEventState = new DialogConditionState(null);
                        break;
                    }
                    case ObjectTypes.AlarmConditionType:
                    {
                        baseEventState = new AlarmConditionState(null);
                        break;
                    }
                    case ObjectTypes.ExclusiveLimitAlarmType:
                    {
                        baseEventState = new ExclusiveLimitAlarmState(null);
                        break;
                    }
                    case ObjectTypes.NonExclusiveLimitAlarmType:
                    {
                        baseEventState = new NonExclusiveLimitAlarmState(null);
                        break;
                    }
                    case ObjectTypes.AuditEventType:
                    {
                        baseEventState = new AuditEventState(null);
                        break;
                    }
                    case ObjectTypes.AuditUpdateMethodEventType:
                    {
                        baseEventState = new AuditUpdateMethodEventState(null);
                        break;
                    }
                    default:
                    {
                        baseEventState = new BaseEventState(null);
                        break;
                    }
                }

                // get the filter which defines the contents of the notification.
                filter = monitoredItem.Status.Filter as EventFilter;
                // initialize the event with the values in the notification.
                baseEventState.Update(_applicationInstanceManager.Session.SystemContext, filter.SelectClauses,
                    notification);
                // save the original notification.
                baseEventState.Handle = notification;
                // construct the audit object.
                if (baseEventState is AuditUpdateMethodEventState audit)
                {
                    // look up the condition type metadata in the local cache.
                    string sourceName = "";
                    if (audit.SourceName.Value != null)
                        sourceName = Utils.Format("{0}", audit.SourceName.Value);
                    string type = "";
                    if (audit.TypeDefinitionId != null)
                        type = Utils.Format("{0}",
                            _applicationInstanceManager.Session.NodeCache.Find(audit.TypeDefinitionId));

                    string method = "";
                    if (audit.MethodId != null)
                        method = Utils.Format("{0}",
                            _applicationInstanceManager.Session.NodeCache.Find(
                                BaseVariableState.GetValue(audit.MethodId)));

                    string status = "";
                    if (audit.Status != null)
                        status = Utils.Format("{0}", audit.Status.Value);

                    string time = "";
                    if (audit.Time != null)
                        time = Utils.Format("{0:HH:mm:ss.fff}", audit.Time.Value.ToLocalTime());

                    string message = "";
                    if (audit.Message != null)
                        message = Utils.Format("{0}", audit.Message.Value);

                    string inputArguments = "";
                    if (audit.InputArguments != null)
                        inputArguments = Utils.Format("{0}", new Variant(audit.InputArguments.Value));


                    InformationDisplay(
                        $"sourceName: {sourceName}, type:{type}, method:{method}, status:{status}, time:{time}, message:{message}, inputArguments:{inputArguments}");

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
                    monitorMethodUpdateToolStripMenuItem.Enabled = false;
                    callToolStripMenuItem.Enabled = false;
                    break;
                case NodeClass.Method:
                    monitorToolStripMenuItem.Enabled = false;
                    monitorMethodUpdateToolStripMenuItem.Enabled = true;
                    callToolStripMenuItem.Enabled = true;
                    break;
                default: 
                    _selectedTreeNode = null;
                    _selectedObjectId = null;
                    _selectedMethodId = null;
                    monitorToolStripMenuItem.Enabled = false;
                    callToolStripMenuItem.Enabled = false;
                    monitorMethodUpdateToolStripMenuItem.Enabled = false;
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
            Variant defaultValue = new Variant(TypeInfo.GetDefaultValue(attributeData.DataType, attributeData.ValueRank));
            if (defaultValue.Value == null)
                defaultValue.Value = ""; 
            AddMonitoredVariableUserControl(defaultValue.Value.ToString(), attributeData.Description.Text, 
                attributeData.DisplayName.Text, defaultValue.TypeInfo, attributeData.NodeId);
            _applicationInstanceManager.SubscribeToNode(attributeData.NodeId, MonitoredItemNotification, 500);
        }

        private void MonitorMethodUpdateToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!(_selectedTreeNode?.Tag is AttributeData))
                return;
            _applicationInstanceManager.SubscribeToAuditUpdateMethodEvent(
                MonitorMethodUpdateNotification, 500);
        }

        private void CallToolStripMenuItemClick(object sender, EventArgs e)
        {
            inputArgumentsPanel.Controls.Clear();
            outputArgumentsPanel.Controls.Clear();
            _selectedObjectId = null;
            _selectedMethodId = null;
            if (!(_selectedTreeNode?.Tag is AttributeData attributeData))
                return;
            if (!(_selectedTreeNode.Parent?.Tag is AttributeData parentAttributeData))
                return;
            _selectedObjectId = parentAttributeData.NodeId;
            _selectedMethodId = attributeData.NodeId;

            ExtendedDataDescription methodReference = _applicationInstanceManager.FlatExtendedDataDescriptionDictionary[attributeData.NodeId.Identifier.ToString()];
            //extract input/output descriptions
            DataDescription inputDataDescription = methodReference.VariableDataDescriptions.FirstOrDefault(x =>
                x.AttributeData.BrowseName.Name == NameVariables.InputArguments);
            DataDescription outputDataDescription = methodReference.VariableDataDescriptions.FirstOrDefault(x =>
                x.AttributeData.BrowseName.Name == NameVariables.OutputArguments);
            //get all argument information
            ExtensionObject[] inputExtensionObjects =
                (ExtensionObject[]) inputDataDescription?.AttributeData.Value.Value;
            ExtensionObject[] outputExtensionObjects =
                (ExtensionObject[]) outputDataDescription?.AttributeData.Value.Value;
            
            callMethodButton.Enabled = true;
            if (inputExtensionObjects != null)
            {
                foreach (ExtensionObject extensionObject in inputExtensionObjects)
                {
                    Argument argument = (Argument) extensionObject.Body;
                    Variant defaultValue = new Variant(TypeInfo.GetDefaultValue(argument.DataType, argument.ValueRank));
                    if (defaultValue.Value == null)
                        defaultValue.Value = "";
                    AddInputArgumentUserControl(defaultValue.Value?.ToString(), argument.Description.Text, argument.Name,
                        defaultValue.TypeInfo);
                }
            }
            if (outputExtensionObjects == null)
                return;
            foreach (ExtensionObject extensionObject in outputExtensionObjects)
            {
                Argument argument = (Argument) extensionObject.Body;
                Variant defaultValue = new Variant(TypeInfo.GetDefaultValue(argument.DataType, argument.ValueRank));
                AddOutputArgumentUserControl(defaultValue.Value?.ToString(), argument.Description.Text, argument.Name,
                    defaultValue.TypeInfo);
            }
        }

        private void ObjectTreeViewMouseDoubleClick(object sender, MouseEventArgs e)
        {
            attributesListView.Items.Clear();
            TreeNode parentNode = objectTreeView.SelectedNode;
            if (parentNode == null)
                return;
            if (!(parentNode?.Tag is AttributeData parentNodeAttributeData))
                return;
            if (!_applicationInstanceManager.FlatExtendedDataDescriptionDictionary.ContainsKey(parentNodeAttributeData.NodeId.Identifier.ToString()))
                return;
            ExtendedDataDescription objectReference = _applicationInstanceManager.FlatExtendedDataDescriptionDictionary[parentNodeAttributeData.NodeId.Identifier.ToString()];
            PopulateAttributeListView(parentNodeAttributeData);

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