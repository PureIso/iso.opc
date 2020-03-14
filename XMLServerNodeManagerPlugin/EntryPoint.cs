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
            if (nodeState is MethodState methodNodeState)
            {
                Console.WriteLine($"Method State: {methodNodeState.BrowseName}");
                if (methodNodeState.DisplayName.Text == RaspberryPiNode.MethodNameGetDoubleTheValue)
                {
                    methodNodeState.OnCallMethod = GetDoubleTheValue;
                }
                else if (methodNodeState.DisplayName.Text == RaspberryPiNode.MethodNameGetVoltage)
                {
                    methodNodeState.OnCallMethod = GetVoltage;
                }
                else if (methodNodeState.DisplayName.Text == RaspberryPiNode.MethodNameUpdateSystemStatus)
                {
                    methodNodeState.OnCallMethod = UpdateSystemStatus;
                }
            }
            else if (nodeState is PropertyState propertyState)
            {
                Console.WriteLine($"Property State: {propertyState.BrowseName}");
                Console.WriteLine($"Property Display State: {propertyState.DisplayName.Text}");
                if (propertyState.DisplayName.Text == RaspberryPiNode.VariableNameSystemStatus)
                {
                    _systemStatusPropertyState = propertyState;
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
