using System;
using System.Collections.Generic;
using Iso.Opc.Interface;
using Opc.Ua;

namespace XMLServerNodeManagerPlugin
{
    public class EntryPoint : AbstractApplicationNodeManagerPlugin
    {
        public EntryPoint()
        {
            base.ApplicationName = "XML Server Node Manager";
            base.Author = "Ola";
            base.Description = "XML Plugin Test";
            base.Version = "1.0.0.0";
            base.ResourcePath = AppDomain.CurrentDomain.BaseDirectory + "plugin\\xml_example.xml";
        }

        private PropertyState _systemStatusPropertyState;

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
                {
                    if (methodNodeState.DisplayName.Text == RaspberryPiNode.MethodNameUpdateSystemStatus)
                    {
                        methodNodeState.OnCallMethod = UpdateSystemStatus;
                    }
                    break;
                }
                case PropertyState propertyState:
                {
                    if (propertyState.DisplayName.Text == RaspberryPiNode.VariableNameSystemStatus)
                    {
                        _systemStatusPropertyState = propertyState;
                    }
                    break;
                }
            }
        }

        private ServiceResult GetDoubleTheValue(ISystemContext context, MethodState method, IList<object> inputArguments, IList<object> outputArguments)
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
        private ServiceResult GetVoltage(ISystemContext context, MethodState method, IList<object> inputArguments, IList<object> outputArguments)
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
