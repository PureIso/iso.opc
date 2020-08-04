using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Iso.Opc.Core.Implementations;
using Iso.Opc.Plugin.XMLDataTypeNodeManager.Models;
using Opc.Ua;

namespace Iso.Opc.Plugin.XMLDataTypeNodeManager
{
    public class EntryPoint : AbstractApplicationNodeManagerPlugin
    {
        #region Fields
        private List<string> _applications;
        #endregion
        public EntryPoint()
        {
            base.ApplicationName = "XMLDataTypeServerNodeManager";
            base.Author = "Ola";
            base.Description = "XML Data Type Plugin Test";
            base.Version = "1.0.0.0";
            string directoryName = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            if (string.IsNullOrEmpty(directoryName))
                return;
            string xmlFilePath = Path.Combine(directoryName, "plugin/xml_example_types.xml");
            base.ResourcePath = xmlFilePath;
        }

        public override void BindNodeStateActions(NodeState nodeState)
        {
            switch (nodeState)
            {
                case MethodState methodNodeState when methodNodeState.DisplayName.Text == PLCControllerNode.MethodNameAddApplication:
                    methodNodeState.OnCallMethod = AddApplication;
                    break;
            }
        }

        private ServiceResult AddApplication(ISystemContext context, MethodState method, IList<object> inputArguments, IList<object> outputArguments)
        {
            if (inputArguments.Count != 1)
                return StatusCodes.BadArgumentsMissing;
            // check the data type of the input arguments.
            string value = inputArguments[0] as string;
            if (value == null)
                return StatusCodes.BadTypeMismatch;
            if (_applications == null)
                _applications = new List<string>();
            _applications.Add(value);
            //Report
            TranslationInfo info = new TranslationInfo(
                "AddApplication",
                "en-US",
                "The Confirm method was called.");
            AuditUpdateMethodEventState auditUpdateMethodEventState = new AuditUpdateMethodEventState(method);
            auditUpdateMethodEventState.Initialize(
                context,
                method,
                EventSeverity.Low,
                new LocalizedText(info),
                ServiceResult.IsGood(StatusCodes.Good),
                DateTime.UtcNow);
            auditUpdateMethodEventState.SourceName.Value = "Attribute/Call";
            auditUpdateMethodEventState.MethodId = new PropertyState<NodeId>(method)
            {
                Value = method.NodeId
            };
            auditUpdateMethodEventState.InputArguments = new PropertyState<object[]>(method)
            {
                Value = new object[] { inputArguments }
            };
            auditUpdateMethodEventState.SetChildValue(context, BrowseNames.InputArguments, inputArguments.ToArray(), true);
            bool valid = auditUpdateMethodEventState.Validate(context);
            if (valid)
                ApplicationNodeManager.Server.ReportEvent(auditUpdateMethodEventState);
            return ServiceResult.Good;
        }
    }
}
