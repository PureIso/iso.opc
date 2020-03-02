﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Client.Properties;
using Iso.Opc.ApplicationManager;
using Iso.Opc.ApplicationManager.Models;
using Opc.Ua;

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

        private ImageList _imageList;
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
            _imageList = new ImageList();
            _imageList.Images.Add(Resources.folder_cog);
            _imageList.Images.Add(Resources.folder_create);
            _imageList.Images.Add(Resources.folder_magnifier);
            // Assign the ImageList to the TreeView.
            objectTreeView.ImageList = _imageList;
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
                        null, null, null, null, _globalDiscoveryServerUrls, _globalDiscoveryServerWellKnownUrls, ApplicationType, true);
                }
                if (globalDiscoveryServerUseCheckBox.Checked && !connectToServer)
                {
                    globalDiscoveryServerConnectionStatusPanel.BackColor = Color.Red;
                    string gdsUserName = globalDiscoveryServerUseSecurityCheckBox.Checked ? globalDiscoveryServerUserNameTextBox.Text : null;
                    string gdsUserPassword = globalDiscoveryServerUseSecurityCheckBox.Checked ? globalDiscoveryServerPasswordTextBox.Text : null;
                    bool connectedToGDS = _applicationInstanceManager.ConnectToGlobalDiscoveryServer(globalDiscoveryServerDiscoveryURLTextBox.Text, gdsUserName, gdsUserPassword);
                    if (!connectedToGDS)
                    {
                        Disconnect();
                        return;
                    }
                    if(registerApplicationCheckBox.Checked) 
                        _applicationInstanceManager.RegisterApplication();
                    if(requestNewCertificateCheckBox.Checked) 
                        _applicationInstanceManager.RequestNewCertificatePullMode();
                    List<ServerOnNetwork> serversOnNetwork = _applicationInstanceManager.QueryServers();
                    discoveredServersListView.Items.Clear();
                    if (serversOnNetwork == null || !serversOnNetwork.Any())
                    {
                        Disconnect();
                        return;
                    }
                    ListViewItem[] discoveredServersListViewItems = (from x in serversOnNetwork select new ListViewItem(x.DiscoveryUrl)).ToArray();
                    discoveredServersListView.Items.AddRange(discoveredServersListViewItems);
                    globalDiscoveryServerConnectionStatusPanel.BackColor = Color.Green;
                }

                if (!connectToServer && globalDiscoveryServerUseCheckBox.Checked) 
                    return;
                connectionStatusPanel.BackColor = Color.Red;
                string userName = useSecurityCheckBox.Checked ? serverUserNameTextBox.Text : null;
                string userPassword = useSecurityCheckBox.Checked ? serverPasswordTextBox.Text : null;
                bool connectedToServer = _applicationInstanceManager.ConnectToServer(serverDiscoveryURLTextBox.Text, userName, userPassword);
                if (!connectedToServer)
                    return;

                TreeNode[] browsedObjects = { new TreeNode(Root.NameObjects), new TreeNode(Root.NameTypes) , new TreeNode(Root.NameViews) };
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
        private void GetDiscoveryServerTrustedListButtonClick(object sender, EventArgs e)
        {
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

            serverUserNameTextBox.Enabled = !globalDiscoveryServerUseCheckBox.Checked;
            serverPasswordTextBox.Enabled = !globalDiscoveryServerUseCheckBox.Checked;
            serverDiscoveryURLTextBox.Enabled = !globalDiscoveryServerUseCheckBox.Checked;
            useSecurityCheckBox.Enabled = !globalDiscoveryServerUseCheckBox.Checked;
        }
        private void DiscoveredServersListViewMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) 
                return;
            ListViewItem selectedItem = (sender as ListView)?.GetItemAt(e.X,e.Y);
            if (selectedItem == null)
                return;
            serverDiscoveryURLTextBox.Text = selectedItem.Text;
            Point point = (Point) ((ListView) sender)?.PointToScreen(e.Location);
            serverConnectContextMenuStrip.Show(point);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //DataDescription objectReference =
            //    _applicationInstanceManager.ExtendedReferenceDescriptions[0].DataDescription;
            //DataDescription methodReference =
            //    _applicationInstanceManager.ExtendedReferenceDescriptions[0].MethodDataDescriptions[0];
            //NodeId objectNodeId = new NodeId(objectReference.ReferenceDescription.NodeId.Identifier, objectReference.ReferenceDescription.NodeId.NamespaceIndex);
            //NodeId methodNodeId = new NodeId(methodReference.ReferenceDescription.NodeId.Identifier, methodReference.ReferenceDescription.NodeId.NamespaceIndex);
            //object[] arguments = new object[2];
            //arguments[0] = Convert.ToUInt32(1);
            //arguments[1] = Convert.ToUInt32(100);
            //testOutputTextBox.Clear();
            //IList<object> outputArguments = _applicationInstanceManager.Session.Call(objectNodeId, methodNodeId, arguments);
            //foreach (object outputArgument in outputArguments)
            //{
            //    testOutputTextBox.Text += $"{outputArgument}\r\n";
            //}
        }

        private void ObjectTreeViewMouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode parentNode = objectTreeView.SelectedNode;
            ExtendedDataDescription objectReference = null;
            if (_applicationInstanceManager.FlatExtendedDataDescriptionDictionary.ContainsKey(parentNode.Text))
            {
                objectReference = _applicationInstanceManager.FlatExtendedDataDescriptionDictionary[parentNode.Text];
            }
            if (objectReference == null)
                return;

            TreeNode[] browsedObjects;
            if (objectReference.MethodDataDescriptions != null)
            {
                browsedObjects = (from x in objectReference.MethodDataDescriptions select new TreeNode(x.DataDescription.ReferenceDescription.BrowseName.Name, 0, 0)).ToArray();
                parentNode.Nodes.AddRange(browsedObjects);
                parentNode.Expand();
            }

            if (objectReference.VariableDataDescriptions != null)
            {
                browsedObjects = (from x in objectReference.VariableDataDescriptions select new TreeNode(x.ReferenceDescription.BrowseName.Name, 1, 1)).ToArray();
                parentNode.Nodes.AddRange(browsedObjects);
                parentNode.Expand();
            }

            if (objectReference.ObjectDataDescriptions == null) 
                return;
            browsedObjects = (from x in objectReference.ObjectDataDescriptions select new TreeNode(x.DataDescription.ReferenceDescription.BrowseName.Name, 2, 2)).ToArray();
            parentNode.Nodes.AddRange(browsedObjects);
            parentNode.Expand();
        }
        #endregion
    }
}