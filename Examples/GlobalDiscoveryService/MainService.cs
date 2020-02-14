
using Opc.Ua;
using Opc.Ua.Gds.Server;
using System;
using System.IO;
using System.ServiceProcess;
using Iso.Opc.ApplicationManager;
using Iso.Opc.ApplicationNodeManager.Database;

namespace LocalDiscoveryService
{
    public partial class MainService : ServiceBase
    {
        #region Constants
        private const string ApplicationName = "Basic Global Discovery Server";
        private const string ApplicationUri = "urn:localhost:UA:BasicLocalDiscoveryServer";
        private readonly ApplicationType ApplicationType = ApplicationType.DiscoveryServer;
        #endregion

        #region Fields
        private ApplicationInstanceManager _applicationInstanceManager;
        private readonly StringCollection _globalDiscoveryServerUrls;
        #endregion

        #region Constructor
        public MainService()
        {
            InitializeComponent();
            ServiceName = "OPC-UA Basic Global Discovery Server";
            CanStop = true;
            CanPauseAndContinue = true;
            AutoLog = true;

            _globalDiscoveryServerUrls = new StringCollection {"opc.tcp://localhost:58810/UADiscovery"};
        }


        #endregion

        #region Protected Methods
        protected override void OnStart(string[] args)
        {
            //server settings
            StringCollection baseAddress = new StringCollection
            {
                 "opc.tcp://localhost:58810/UADiscovery"
            };
            StringCollection serverCapabilities = new StringCollection { "LDS","GDS" };
            //Discovery Endpoint
            string endpointUrl = "opc.tcp://localhost:58810/UADiscovery";
            string endpointApplicationUri = "urn:localhost:OPCFoundation:BasicLocalDiscoveryServer";
            //Initialise
            _applicationInstanceManager = new ApplicationInstanceManager(ApplicationName, ApplicationUri,
                baseAddress, serverCapabilities, endpointUrl, endpointApplicationUri, _globalDiscoveryServerUrls, null, ApplicationType);
            string databaseDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\gds\\database";
            if (!Directory.Exists(databaseDirectory))
                Directory.CreateDirectory(databaseDirectory);
            string databaseFile = databaseDirectory + "\\gds.database.json";
            if (!File.Exists(databaseFile))
                File.Create(databaseFile).Close();
            ApplicationsDatabase applicationDatabase = ApplicationsDatabase.Load(databaseFile);
            CertificateGroup certificateGroup = new CertificateGroup();
            MainLocalDiscoveryServer mainDiscoveryServer = new MainLocalDiscoveryServer(
                applicationDatabase,
                applicationDatabase,
                certificateGroup);
            mainDiscoveryServer.Start(_applicationInstanceManager.ApplicationInstance.ApplicationConfiguration);
            foreach (EndpointDescription endpointDescription in mainDiscoveryServer.GetEndpoints())
            {
                Console.WriteLine($"Endpoint: {endpointDescription.EndpointUrl}");
            }
        }

        protected override void OnStop()
        {
            Program.AutoResetEvent.Set();
        }
        #endregion

        #region Internal Methods
        internal void OnDebug()
        {
            OnStart(null);
        }
        #endregion
    }
}
