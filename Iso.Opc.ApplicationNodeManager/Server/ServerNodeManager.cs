using System;
using System.Collections.Generic;
using System.IO;
using Opc.Ua;
using Opc.Ua.Server;
using Namespaces = Iso.Opc.ApplicationNodeManager.Models.Namespaces;

namespace Iso.Opc.ApplicationNodeManager.Server
{
    public sealed partial class ServerNodeManager : CustomNodeManager2
    {
        #region Fields
        private ApplicationConfiguration _applicationConfiguration;
        private List<BaseDataVariableState> _baseDataVariableStates;
        #endregion

        #region Properties
        public string PredefinedXMLNodeDirectory { get; set; }
        #endregion

        public ServerNodeManager(IServerInternal server, ApplicationConfiguration applicationConfiguration) 
            : base(server, applicationConfiguration, Namespaces.BasicApplications)
        {
            SystemContext.NodeIdFactory = this;
            _applicationConfiguration = applicationConfiguration;
            _baseDataVariableStates = new List<BaseDataVariableState>();
            if (!string.IsNullOrEmpty(PredefinedXMLNodeDirectory)) 
                return;
            PredefinedXMLNodeDirectory = AppDomain.CurrentDomain.BaseDirectory + "predefined_models";
            if (!Directory.Exists(PredefinedXMLNodeDirectory))
                Directory.CreateDirectory(PredefinedXMLNodeDirectory);
        }
    }
}
