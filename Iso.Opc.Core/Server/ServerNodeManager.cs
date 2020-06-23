using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Iso.Opc.Core.Implementations;
using Iso.Opc.Core.Plugin;
using Opc.Ua;
using Opc.Ua.Server;

namespace Iso.Opc.Core.Server
{
    public sealed partial class ServerNodeManager : CustomNodeManager2
    {
        #region Fields
        private readonly string _pluginDirectory;
        private readonly ApplicationNodeManagerPluginService _applicationNodeManagerPluginService;
        private uint _nextNodeId;
        #endregion

        public ServerNodeManager(IServerInternal server, ApplicationConfiguration applicationConfiguration) 
            : base(server, applicationConfiguration)
        { 
            NamespaceUris = new List<string> { $"http://{Dns.GetHostName()}/UA/Default" };
            SystemContext.NodeIdFactory = this;
            _nextNodeId = 0;
            if (!string.IsNullOrEmpty(_pluginDirectory)) 
                return;
            string directoryName = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            if (string.IsNullOrEmpty(directoryName))
                return;
            _pluginDirectory = Path.Combine(directoryName,"plugin");
            if (!Directory.Exists(_pluginDirectory))
                Directory.CreateDirectory(_pluginDirectory);
            _applicationNodeManagerPluginService = new ApplicationNodeManagerPluginService(_pluginDirectory);
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            lock (Lock)
            {
                foreach (AbstractApplicationNodeManagerPlugin abstractApplicationNodeManagerPlugin in _applicationNodeManagerPluginService.PluginBaseNodeManagers)
                {
                    abstractApplicationNodeManagerPlugin.Initialise(this);
                    //Get current namespace uris
                    List<string> temporaryNamespaceUris = NamespaceUris.ToList();
                    //Add the new namespaces
                    temporaryNamespaceUris.AddRange(abstractApplicationNodeManagerPlugin.NamespaceUris);
                    //override the current namespace uris
                    NamespaceUris = temporaryNamespaceUris.Distinct();
                }
            }
        }
    }
}
