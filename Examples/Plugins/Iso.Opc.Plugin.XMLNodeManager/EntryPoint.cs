using System;
using System.Collections.Generic;
using System.IO;
using Iso.Opc.Core.Implementations;
using Iso.Opc.Plugin.XMLNodeManager.Models;
using Opc.Ua;

namespace Iso.Opc.Plugin.XMLNodeManager
{
    public class EntryPoint : AbstractApplicationNodeManagerPlugin
    {
        #region Fields
        private PropertyState _systemStatusPropertyState;
        #endregion
        public EntryPoint()
        {
            base.ApplicationName = "XML Server Node Manager";
            base.Author = "Ola";
            base.Description = "XML Plugin Test";
            base.Version = "1.0.0.0";
            string directoryName = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            if (string.IsNullOrEmpty(directoryName)) 
                return;
            string xmlFilePath = Path.Combine(directoryName, "plugin/xml_example.xml");
            base.ResourcePath = xmlFilePath;
        }

        public override void BindNodeStateActions(NodeState nodeState)
        {
            switch (nodeState)
            {
                case MethodState methodNodeState when methodNodeState.DisplayName.Text == RaspberryPiNode.MethodNameGetDoubleTheValue:
                    methodNodeState.OnCallMethod = GetDoubleTheValue;
                    break;
                case MethodState methodNodeState when methodNodeState.DisplayName.Text == RaspberryPiNode.MethodNameGetVoltage:
                    methodNodeState.OnCallMethod = GetVoltage;
                    break;
                case MethodState methodNodeState:
                    if (methodNodeState.DisplayName.Text == RaspberryPiNode.MethodNameUpdateSystemStatus) 
                        methodNodeState.OnCallMethod = UpdateSystemStatus; 
                    break; 
                case PropertyState propertyState:
                    if (propertyState.DisplayName.Text == RaspberryPiNode.VariableNameSystemStatus)
                        _systemStatusPropertyState = propertyState;
                    break;
            }
        }

        private static ServiceResult GetDoubleTheValue(ISystemContext context, MethodState method, IList<object> inputArguments, IList<object> outputArguments)
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
        private static ServiceResult GetVoltage(ISystemContext context, MethodState method, IList<object> inputArguments, IList<object> outputArguments)
        {
            Random random = new Random();
            outputArguments[0] = random.NextDouble();
            return ServiceResult.Good;
        }
        private ServiceResult UpdateSystemStatus(ISystemContext context, MethodState method, IList<object> inputArguments, IList<object> outputArguments)
        {
            if (inputArguments.Count != 1)
                return StatusCodes.BadArgumentsMissing;
            _systemStatusPropertyState.Value = inputArguments[0];
            // signal update to state node.
            lock (ApplicationNodeManager.Lock)
            {
                _systemStatusPropertyState.ClearChangeMasks(ApplicationNodeManager.SystemContext, true);
            }
            return ServiceResult.Good;
        }
    }
}
