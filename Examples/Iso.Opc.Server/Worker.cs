using System;
using System.Threading;
using System.Threading.Tasks;
using Iso.Opc.Core;
using Iso.Opc.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Ua;

namespace Iso.OPC.Server
{
    public class Worker : BackgroundService
    {
        #region Constants
        private const string ApplicationName = "Basic Server";
        private const string ApplicationUri = "urn:localhost:UA:BasicServer";
        private const string DiscoveryEndpointUrl = "opc.tcp://localhost:48001/BasicServer";
        private const string DiscoveryEndpointApplicationUri = "urn:localhost:OPCFoundation:BasicServer";
        private readonly ApplicationType _applicationType = ApplicationType.Server;
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
            StringCollection baseAddress = new StringCollection
            {
                DiscoveryEndpointUrl
            };
            StringCollection serverCapabilities = new StringCollection { "DA" };
            StringCollection discoveryUrls = new StringCollection
            {
                "opc.tcp://opc.ua.global.discovery.server:58810/UADiscovery"
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
            try
            {
                _mainServer = new MainServer(_applicationInstanceManager);
                _mainServer.Start(_applicationInstanceManager.ApplicationInstance.ApplicationConfiguration);
                bool connected = _applicationInstanceManager.ConnectToGlobalDiscoveryServer("opc.tcp://opc.ua.global.discovery.server:58810/UADiscovery", "appadmin", "demo");
                if (connected)
                {
                    _applicationInstanceManager.RegisterApplication();
                    _applicationInstanceManager.RequestNewCertificatePullMode();
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Exception: ", ex.StackTrace);
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
