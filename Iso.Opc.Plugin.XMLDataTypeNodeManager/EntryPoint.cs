using System;
using System.Collections.Generic;
using System.IO;
using Iso.Opc.Core.Implementations;
using Iso.Opc.Plugin.XMLDataTypeNodeManager.Models;
using Opc.Ua;

namespace Iso.Opc.Plugin.XMLDataTypeNodeManager
{
    public class EntryPoint : AbstractApplicationNodeManagerPlugin
    {
        #region Fields
        //private PropertyState _systemStatusPropertyState;
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
                //case PropertyState propertyState:
                //    if (propertyState.DisplayName.Text == RaspberryPiNode.VariableNameSystemStatus)
                //        _systemStatusPropertyState = propertyState;
                //    break;
            }
        }

        private static ServiceResult AddApplication(ISystemContext context, MethodState method, IList<object> inputArguments, IList<object> outputArguments)
        {
            if (inputArguments.Count != 1)
                return StatusCodes.BadArgumentsMissing;
            // check the data type of the input arguments.
            uint? value = inputArguments[0] as uint?;
            if (value == null)
                return StatusCodes.BadTypeMismatch;
            outputArguments[0] = value * value;
            return ServiceResult.Good;
        }
    }
}
