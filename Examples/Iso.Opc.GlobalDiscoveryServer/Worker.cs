using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iso.Opc.Core;
using Iso.Opc.Core.Database;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Gds.Server;

namespace Iso.Opc.GlobalDiscoveryServer
{
    public class Worker : BackgroundService
    {
        #region Constants
        private const string ApplicationName = "Basic Global Discovery Server";
        private const string ApplicationUri = "urn:localhost:UA:BasicLGlobalDiscoveryServer";
        private const string DiscoveryEndpointUrl = "opc.tcp://localhost:58810/UADiscovery";
        private const string DiscoveryEndpointApplicationUri = "urn:localhost:OPCFoundation:BasicLGlobalDiscoveryServer";
        private readonly ApplicationType _applicationType = ApplicationType.DiscoveryServer;
        private readonly ILogger<Worker> _logger;
        #endregion

        #region Fields
        private ApplicationInstanceManager _applicationInstanceManager;
        private MainServer _mainServer;
        #endregion

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Worker started at: {DateTime.Now}");
            //server settings
            StringCollection baseAddress = new StringCollection
            {
                DiscoveryEndpointUrl
            };
            StringCollection serverCapabilities = new StringCollection { "LDS", "GDS" };
            StringCollection discoveryUrls = new StringCollection
            {
                "opc.tcp://localhost:58810/UADiscovery"
            };
            //Initialise
            _applicationInstanceManager = new ApplicationInstanceManager(ApplicationName, ApplicationUri,
                baseAddress, 
                serverCapabilities,
                DiscoveryEndpointUrl,
                DiscoveryEndpointApplicationUri,
                discoveryUrls, 
                null, 
                _applicationType, 
                true);

            string directoryName = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            if (!string.IsNullOrEmpty(directoryName))
            {
                string databaseDirectory = Path.Combine(directoryName, "gds\\database");
                if (!Directory.Exists(databaseDirectory))
                    Directory.CreateDirectory(databaseDirectory);
                string databaseFile = Path.Combine(databaseDirectory, "gds.database.json");
                if (!File.Exists(databaseFile))
                    File.Create(databaseFile).Close();

                ApplicationsDatabase applicationDatabase = ApplicationsDatabase.Load(databaseFile);
                CertificateGroup certificateGroup = new CertificateGroup();

                _mainServer = new MainServer(
                    applicationDatabase,
                    applicationDatabase,
                    certificateGroup);
                _mainServer.Start(_applicationInstanceManager.ApplicationInstance.ApplicationConfiguration);
            }
            
            await base.StartAsync(cancellationToken);
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Worker stopped at: {DateTime.Now}");
            _mainServer.Dispose();
            return base.StopAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
