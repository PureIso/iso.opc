using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Iso.Opc.ApplicationNodeManager.Plugin;
using Iso.Opc.Interface;
using Opc.Ua;
using Opc.Ua.Server;

namespace Iso.Opc.ApplicationNodeManager.Server
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
            _pluginDirectory = AppDomain.CurrentDomain.BaseDirectory + "plugin";
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
