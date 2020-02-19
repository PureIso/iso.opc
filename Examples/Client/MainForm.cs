using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Iso.Opc.ApplicationManager;
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
        }
        #endregion

        #region Handlers
        private void CustomConnectionButtonClick(object sender, EventArgs e)
        { 
            _applicationInstanceManager = new ApplicationInstanceManager(ApplicationName, ApplicationUri, 
                null, null, null, null, _globalDiscoveryServerUrls, _globalDiscoveryServerWellKnownUrls, ApplicationType);
            string gdsUserName = globalDiscoveryServerUseSecurityCheckBox.Checked ? globalDiscoveryServerUserNameTextBox.Text : null;
            string gdsUserPassword = globalDiscoveryServerUseSecurityCheckBox.Checked ? globalDiscoveryServerPasswordTextBox.Text : null; 
            bool connectedToGDS = _applicationInstanceManager.ConnectToGlobalDiscoveryServer(globalDiscoveryServerDiscoveryURLTextBox.Text, gdsUserName, gdsUserPassword); 
            if (connectedToGDS) 
            { 
                _applicationInstanceManager.RegisterApplication(); 
                _applicationInstanceManager.RequestNewCertificatePullMode(); 
                List<ServerOnNetwork> serversOnNetwork = _applicationInstanceManager.QueryServers();
                discoveredServersListView.Items.Clear();
                if (serversOnNetwork != null && serversOnNetwork.Any())
                {
                    ListViewItem[] discoveredServersListViewItems = (from x in serversOnNetwork select new ListViewItem(x.DiscoveryUrl)).ToArray();
                    discoveredServersListView.Items.AddRange(discoveredServersListViewItems);
                }
            } 
            string userName = useSecurityCheckBox.Checked ? serverUserNameTextBox.Text : null; 
            string userPassword = useSecurityCheckBox.Checked ? serverPasswordTextBox.Text : null; 
            bool connectedToServer = _applicationInstanceManager.ConnectToServer(serverDiscoveryURLTextBox.Text, userName, userPassword);
            if (!connectedToServer) 
                return;

            TreeNode[] browsedObjects = (from x in _applicationInstanceManager.ReferenceDescriptionDictionary select new TreeNode(x.Value.DisplayName.Text)).ToArray();
            objectTreeView.Nodes.Clear();
            objectTreeView.Nodes.AddRange(browsedObjects);
            _applicationInstanceManager.GetControllersReferenceDescriptions();
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
        #endregion


        private void button1_Click(object sender, EventArgs e)
        {
            ReferenceDescription objectReference =
                _applicationInstanceManager.ExtendedReferenceDescriptions[0].ParentReferenceDescription;
            ReferenceDescription methodReference =
                _applicationInstanceManager.ExtendedReferenceDescriptions[0].MethodReferenceDescriptions[0];
            NodeId objectNodeId = new NodeId(objectReference.NodeId.Identifier, objectReference.NodeId.NamespaceIndex);
            NodeId methodNodeId = new NodeId(methodReference.NodeId.Identifier, methodReference.NodeId.NamespaceIndex);
            _applicationInstanceManager.Session.Call(objectNodeId, methodNodeId, 1, 100);
        }

        private void ObjectTreeViewMouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode parentNode = objectTreeView.SelectedNode;
            ReferenceDescription objectReference =
                _applicationInstanceManager.ReferenceDescriptionDictionary[parentNode.Text];
            List<ReferenceDescription> referenceDescription =  _applicationInstanceManager.BrowseReferenceDescription(objectReference);
            TreeNode[] browsedObjects = (from x in referenceDescription select new TreeNode(x.DisplayName.Text)).ToArray();
            parentNode.Nodes.AddRange(browsedObjects);
            parentNode.Expand();
        }
    }
}
