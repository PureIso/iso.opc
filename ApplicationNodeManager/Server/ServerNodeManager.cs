using System.Collections.Generic;
using Opc.Ua;
using Opc.Ua.Server;
using Namespaces = Iso.Opc.ApplicationNodeManager.Models.Namespaces;

namespace Iso.Opc.ApplicationNodeManager.Server
{
    public sealed partial class ServerNodeManager : CustomNodeManager2
    {
        private ApplicationConfiguration _applicationConfiguration;
        private List<BaseDataVariableState> _baseDataVariableStates;

        public ServerNodeManager(IServerInternal server, ApplicationConfiguration applicationConfiguration) 
            : base(server, applicationConfiguration)
        {
            List<string> namespaceUris = new List<string>
            {
                Namespaces.BasicApplications,
                Namespaces.Methods                 
            };
            NamespaceUris = namespaceUris;
            SystemContext.NodeIdFactory = this;
            _applicationConfiguration = applicationConfiguration;
            _baseDataVariableStates = new List<BaseDataVariableState>();
        }
    }
}
