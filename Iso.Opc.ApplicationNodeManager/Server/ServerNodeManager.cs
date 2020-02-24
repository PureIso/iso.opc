using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Iso.Opc.ApplicationNodeManager.Plugin;
using Opc.Ua;
using Opc.Ua.Server;

namespace Iso.Opc.ApplicationNodeManager.Server
{
    public sealed partial class ServerNodeManager : CustomNodeManager2
    {
        #region Fields
        private readonly string _pluginDirectory;
        private ApplicationNodeManagerPluginService _applicationNodeManagerPluginService;
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
        }
    }
}
