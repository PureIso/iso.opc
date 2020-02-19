using System;
using System.Collections.Generic;
using System.Threading;
using Opc.Ua;

namespace Iso.Opc.ApplicationNodeManager.Server
{
    public sealed partial class ServerNodeManager
    {
        private readonly object _processLock = new object();
        private uint _state;
        private uint _finalState;
        private Timer _processTimer;
        private PropertyState<uint> _stateNode;

        private void CreateProcessNode(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            /* ***************************************** */
            /* ControllerType                            */
            /* ***************************************** */
            BaseObjectState controller = new BaseObjectState(null)
            {
                NodeId = new NodeId(1, NamespaceIndex),
                BrowseName = new QualifiedName("Controllers", NamespaceIndex),
                DisplayName = new LocalizedText("Controllers"),
                TypeDefinitionId = ObjectTypeIds.BaseObjectType
            };

            // ensure the process object can be found via the server object. 
            IList<IReference> references = null;
            if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out references))
            {
                externalReferences[ObjectIds.ObjectsFolder] = references = new List<IReference>();
            }
            controller.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);
            references.Add(new NodeStateReference(ReferenceTypeIds.Organizes, false, controller.NodeId));

            //Variables
            PropertyState<uint> state = new PropertyState<uint>(controller)
            {
                NodeId = new NodeId(2, NamespaceIndex),
                BrowseName = new QualifiedName("State", NamespaceIndex),
                DisplayName = new LocalizedText("State"),
                TypeDefinitionId = VariableTypeIds.PropertyType,
                ReferenceTypeId = ReferenceTypeIds.HasProperty,
                DataType = DataTypeIds.UInt32,
                ValueRank = ValueRanks.Scalar
            };
            _stateNode = state;
            controller.AddChild(state);

            //Method
            MethodState start = new MethodState(controller)
            {
                NodeId = new NodeId(3, NamespaceIndex),
                BrowseName = new QualifiedName("Start", NamespaceIndex),
                DisplayName = new LocalizedText("Start"),
                ReferenceTypeId = ReferenceTypeIds.HasComponent,
                UserExecutable = true,
                Executable = true
            };

            //Method - Input
            start.InputArguments = new PropertyState<Argument[]>(start)
            {
                NodeId = new NodeId(4, NamespaceIndex),
                BrowseName = BrowseNames.InputArguments,
                DisplayName = new LocalizedText(BrowseNames.InputArguments),
                TypeDefinitionId = VariableTypeIds.PropertyType,
                ReferenceTypeId = ReferenceTypeIds.HasProperty,
                DataType = DataTypeIds.Argument,
                ValueRank = ValueRanks.OneDimension
            };

            Argument[] args = new Argument[2];
            args[0] = new Argument();
            args[0].Name = "Initial State";
            args[0].Description = "The initialize state for the process.";
            args[0].DataType = DataTypeIds.UInt32;
            args[0].ValueRank = ValueRanks.Scalar;

            args[1] = new Argument();
            args[1].Name = "Final State";
            args[1].Description = "The final state for the process.";
            args[1].DataType = DataTypeIds.UInt32;
            args[1].ValueRank = ValueRanks.Scalar;

            start.InputArguments.Value = args;

            //Method - Output
            start.OutputArguments = new PropertyState<Argument[]>(start);
            start.OutputArguments.NodeId = new NodeId(5, NamespaceIndex);
            start.OutputArguments.BrowseName = BrowseNames.OutputArguments;
            start.OutputArguments.DisplayName = start.OutputArguments.BrowseName.Name;
            start.OutputArguments.TypeDefinitionId = VariableTypeIds.PropertyType;
            start.OutputArguments.ReferenceTypeId = ReferenceTypeIds.HasProperty;
            start.OutputArguments.DataType = DataTypeIds.Argument;
            start.OutputArguments.ValueRank = ValueRanks.OneDimension;

            args = new Argument[2];
            args[0] = new Argument();
            args[0].Name = "Revised Initial State";
            args[0].Description = "The revised initialize state for the process.";
            args[0].DataType = DataTypeIds.UInt32;
            args[0].ValueRank = ValueRanks.Scalar;

            args[1] = new Argument();
            args[1].Name = "Revised Final State";
            args[1].Description = "The revised final state for the process.";
            args[1].DataType = DataTypeIds.UInt32;
            args[1].ValueRank = ValueRanks.Scalar;

            start.OutputArguments.Value = args;

            controller.AddChild(start);

            // save in dictionary. 
            AddPredefinedNode(SystemContext, controller);

            // set up method handlers. 
            start.OnCallMethod = new GenericMethodCalledEventHandler(OnStart);
        }

        /// <summary>
        /// Called when the Start method is called.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="method">The method.</param>
        /// <param name="inputArguments">The input arguments.</param>
        /// <param name="outputArguments">The output arguments.</param>
        /// <returns></returns>
        public ServiceResult OnStart(
            ISystemContext context,
            MethodState method,
            IList<object> inputArguments,
            IList<object> outputArguments)
        {
            // all arguments must be provided.
            if (inputArguments.Count < 2)
            {
                return StatusCodes.BadArgumentsMissing;
            }

            // check the data type of the input arguments.
            uint? initialState = inputArguments[0] as uint?;
            uint? finalState = inputArguments[1] as uint?;

            if (initialState == null || finalState == null)
            {
                return StatusCodes.BadTypeMismatch;
            }

            lock (_processLock)
            {
                // check if the process is running.
                if (_processTimer != null)
                {
                    _processTimer.Dispose();
                    _processTimer = null;
                }

                // start the process.
                _state = initialState.Value;
                _finalState = finalState.Value;
                _processTimer = new Timer(OnUpdateProcess, null, 1000, 1000);

                // the calling function sets default values for all output arguments.
                // only need to update them here.
                outputArguments[0] = _state;
                outputArguments[1] = _finalState;
            }

            // signal update to state node.
            lock (Lock)
            {
                _stateNode.Value = _state;
                _stateNode.ClearChangeMasks(SystemContext, true);
            }

            return ServiceResult.Good;
        }

        /// <summary>
        /// Called when updating the process.
        /// </summary>
        /// <param name="state">The state.</param>
        private void OnUpdateProcess(object state)
        {
            try
            {
                lock (_processLock)
                {
                    // check if increasing.
                    if (_state < _finalState)
                    {
                        _state++;
                    }

                    // check if decreasing.
                    else if (_state > _finalState)
                    {
                        _state--;
                    }

                    // check if all done.
                    else
                    {
                        _processTimer.Dispose();
                        _processTimer = null;
                    };
                }

                // signal update to state node.
                lock (Lock)
                {
                    _stateNode.Value = _state;
                    _stateNode.ClearChangeMasks(SystemContext, true);
                }
            }
            catch (Exception e)
            {
                Utils.Trace(e, "Unexpected error updating process.");
            }
        }
    }
}
