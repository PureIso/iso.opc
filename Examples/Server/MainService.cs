using Opc.Ua;
using System.ServiceProcess;
using Iso.Opc.ApplicationManager;


namespace Server
{
    public partial class MainService : ServiceBase
    {
        #region Constants
        private const string ApplicationName = "Basic Server";
        private const string ApplicationUri = "urn:localhost:UA:BasicServer";
        private const string DiscoveryEndpointUrl = "opc.tcp://localhost:48001/BasicServer";
        private const string DiscoveryEndpointApplicationUri = "urn:localhost:OPCFoundation:BasicServer";
        private readonly ApplicationType ApplicationType = ApplicationType.Server;
        #endregion

        #region Fields
        private ApplicationInstanceManager _applicationInstanceManager;
        private MainServer _mainServer;
        #endregion

        #region Constructor
        public MainService()
        {
            InitializeComponent();
            ServiceName = ApplicationName;
            CanStop = true;
            CanPauseAndContinue = true;
            AutoLog = true;
        }
        #endregion

        #region Internal Methods
        internal void OnDebug()
        {
            OnStart(null);
        }
        #endregion

        #region Protected Methods
        protected override void OnStart(string[] args)
        {
            
            //server settings
            StringCollection baseAddress = new StringCollection
            {
                "opc.tcp://localhost:48001/BasicServer"
            };
            StringCollection serverCapabilities = new StringCollection{"DA"};
            StringCollection discoveryUrls = new StringCollection
            {
               "opc.tcp://localhost:58810/UADiscovery"
            };

            //Initialise
            _applicationInstanceManager = new ApplicationInstanceManager(ApplicationName, ApplicationUri,
                baseAddress, serverCapabilities, DiscoveryEndpointUrl, DiscoveryEndpointApplicationUri, discoveryUrls, null, ApplicationType);
            _mainServer = new MainServer(_applicationInstanceManager);
            _mainServer.Start(_applicationInstanceManager.ApplicationInstance.ApplicationConfiguration);
            bool connected = _applicationInstanceManager.ConnectToGlobalDiscoveryServer("opc.tcp://localhost:58810/UADiscovery","appadmin", "demo");
            if (!connected)
                return;
            _applicationInstanceManager.RegisterApplication();
            _applicationInstanceManager.RequestNewCertificatePullMode();
        }
        protected override void OnStop()
        {
            _mainServer.Dispose();
            Program.AutoResetEvent.Set();
        }
        #endregion
    }
}
