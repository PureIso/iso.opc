using System;
using System.Collections.Generic;
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

            objectListView.Items.Clear();
            ListViewItem[] browsedObjects = (from x in _applicationInstanceManager.ReferenceDescriptionDictionary select new ListViewItem(x.Value.DisplayName.Text)).ToArray();
            objectListView.Items.AddRange(browsedObjects);

            _applicationInstanceManager.GetControllersAttributeReferenceDescriptions();
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
    }
}
