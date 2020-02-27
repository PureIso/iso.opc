using System;
using System.Collections.Generic;
using System.Threading;
using Iso.Opc.Interface;
using Opc.Ua;
using Opc.Ua.Server;

namespace ControllerServerNodeManagerPlugin
{
    public class EntryPoint : AbstractApplicationNodeManagerPlugin
    {
        #region Fields
        private readonly object _processLock = new object();
        private uint _state;
        private uint _finalState;
        private Timer _processTimer;
        private PropertyState<uint> _stateNode;
        #endregion

        public EntryPoint()
        {
            base.ApplicationName = "Controller Server Node Manager";
            base.Author = "Ola";
            base.Description = "Plugin Test";
            base.Version = "1.0.0.0";
        }

        #region Overridden Methods
        public override void Initialise(CustomNodeManager2 nodeManager, IDictionary<NodeId, IList<IReference>> externalReferences, string resourcePath = null)
        {
            base.ApplicationNodeManager = nodeManager;
            /* ***************************************** */
            /* ControllerType                            */
            /* ***************************************** */
            BaseObjectState controller = new BaseObjectState(null)
            {
                NodeId = new NodeId(1, nodeManager.NamespaceIndex),
                BrowseName = new QualifiedName("Controllers", nodeManager.NamespaceIndex),
                DisplayName = new LocalizedText("Controllers"),
                TypeDefinitionId = ObjectTypeIds.BaseObjectType
            };

            // ensure the process object can be found via the server object. 
            if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out IList<IReference> references))
            {
                externalReferences[ObjectIds.ObjectsFolder] = references = new List<IReference>();
            }
            controller.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);
            references.Add(new NodeStateReference(ReferenceTypeIds.Organizes, false, controller.NodeId));

            //Variables
            PropertyState<uint> state = new PropertyState<uint>(controller)
            {
                NodeId = new NodeId(2, nodeManager.NamespaceIndex),
                BrowseName = new QualifiedName("State", nodeManager.NamespaceIndex),
                DisplayName = new LocalizedText("State"),
                TypeDefinitionId = VariableTypeIds.PropertyType,
                ReferenceTypeId = ReferenceTypeIds.HasProperty,
                DataType = DataTypeIds.UInt32,
                ValueRank = ValueRanks.Scalar,
                AccessLevel = AccessLevels.CurrentReadOrWrite
            };
            lock (ApplicationNodeManager.Lock)
            {
                _stateNode = state;
            }
            controller.AddChild(state);
            //Method
            MethodState start = new MethodState(controller)
            {
                NodeId = new NodeId(3, nodeManager.NamespaceIndex),
                BrowseName = new QualifiedName("Start", nodeManager.NamespaceIndex),
                DisplayName = new LocalizedText("Start"),
                ReferenceTypeId = ReferenceTypeIds.HasComponent,
                UserExecutable = true,
                Executable = true
            };

            //Method - Input
            start.InputArguments = new PropertyState<Argument[]>(start)
            {
                NodeId = new NodeId(4, nodeManager.NamespaceIndex),
                BrowseName = BrowseNames.InputArguments,
                DisplayName = new LocalizedText(BrowseNames.InputArguments),
                TypeDefinitionId = VariableTypeIds.PropertyType,
                ReferenceTypeId = ReferenceTypeIds.HasProperty,
                DataType = DataTypeIds.Argument,
                ValueRank = ValueRanks.OneDimension,
                OnReadUserAccessLevel = OnReadUserAccessLevel,
                OnSimpleWriteValue = OnWriteValue
            };

            Argument[] inputArguments = new Argument[2];
            inputArguments[0] = new Argument
            {
                Name = "Initial State",
                Description = "The initialize state for the process.",
                DataType = DataTypeIds.UInt32,
                ValueRank = ValueRanks.Scalar,
            };
            inputArguments[1] = new Argument
            {
                Name = "Final State",
                Description = "The final state for the process.",
                DataType = DataTypeIds.UInt32,
                ValueRank = ValueRanks.Scalar
            };
            start.InputArguments.Value = inputArguments;

            //Method - Output
            start.OutputArguments = new PropertyState<Argument[]>(start)
            {
                NodeId = new NodeId(5, nodeManager.NamespaceIndex),
                BrowseName = BrowseNames.OutputArguments,
                TypeDefinitionId = VariableTypeIds.PropertyType,
                ReferenceTypeId = ReferenceTypeIds.HasProperty,
                DataType = DataTypeIds.Argument,
                ValueRank = ValueRanks.OneDimension,
                OnReadUserAccessLevel = OnReadUserAccessLevel,
                OnSimpleWriteValue = OnWriteValue,
            };
            start.OutputArguments.DisplayName = start.OutputArguments.BrowseName.Name;

            Argument[] outputArguments = new Argument[2];
            outputArguments[0] = new Argument
            {
                Name = "Revised Initial State",
                Description = "The revised initialize state for the process.",
                DataType = DataTypeIds.UInt32,
                ValueRank = ValueRanks.Scalar
            };
            outputArguments[1] = new Argument
            {
                Name = "Revised Final State",
                Description = "The revised final state for the process.",
                DataType = DataTypeIds.UInt32,
                ValueRank = ValueRanks.Scalar
            };
            start.OutputArguments.Value = inputArguments;
            start.OnCallMethod = OnStart;
            controller.AddChild(start);

            base.NodeStateCollection = new NodeStateCollection {controller};
        }
        #endregion

        #region
        /// <summary>
        /// Called when the Start method is called.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="method">The method.</param>
        /// <param name="inputArguments">The input arguments.</param>
        /// <param name="outputArguments">The output arguments.</param>
        /// <returns></returns>
        private ServiceResult OnStart(ISystemContext context, MethodState method, IList<object> inputArguments, IList<object> outputArguments)
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
            lock (ApplicationNodeManager.Lock)
            {
                _stateNode.Value = _state;
                _stateNode.ClearChangeMasks(ApplicationNodeManager.SystemContext, true);
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
                lock (ApplicationNodeManager.Lock)
                {
                    _stateNode.Value = _state;
                    _stateNode.ClearChangeMasks(ApplicationNodeManager.SystemContext, true);
                }
            }
            catch (Exception e)
            {
                Utils.Trace(e, "Unexpected error updating process.");
            }
        }
        #endregion
    }
}
