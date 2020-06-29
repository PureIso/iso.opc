using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Iso.Opc.Core.Implementations;
using Opc.Ua;
using Opc.Ua.Server;
using ApplicationNodeManagerPluginService = Iso.Opc.Core.Plugin.ApplicationNodeManagerPluginService;

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
            try
            {
                NamespaceUris = new System.Collections.Generic.List<string> { $"http://{Dns.GetHostName()}/UA/Default" };
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
            catch (Exception e)
            {
                Console.WriteLine(e);
                Utils.Trace(e.StackTrace);
            }
        }
    }
}
