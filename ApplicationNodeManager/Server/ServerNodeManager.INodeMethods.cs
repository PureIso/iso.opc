using System;
using System.Collections.Generic;
using System.Threading;
using Opc.Ua;

namespace Iso.Opc.ApplicationNodeManager.Server
{
    public sealed partial class ServerNodeManager
    {
        private object m_processLock = new object();
        private uint _state;
        private uint m_finalState;
        private Timer m_processTimer;
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
            /* ***************************************** */
            /* AirConditionerControllerType              */
            /* ***************************************** */
            BaseObjectState ProcessController = new BaseObjectState(controller)
            {
                NodeId = new NodeId(6, NamespaceIndex),
                BrowseName = new QualifiedName("ProcessControllerType", NamespaceIndex),
                DisplayName = new LocalizedText("ProcessControllerType"),
                TypeDefinitionId = ObjectTypeIds.BaseObjectType,
                //UserRolePermissions = AccessLevels.
            };
            
            //Variables
            PropertyState<uint> state = new PropertyState<uint>(ProcessController)
            {
                NodeId = new NodeId(2, NamespaceIndex),
                BrowseName = new QualifiedName("State", NamespaceIndex),
                DisplayName = new LocalizedText("State"),
                TypeDefinitionId = VariableTypeIds.PropertyType,
                ReferenceTypeId = ReferenceTypeIds.HasProperty,
                DataType = DataTypeIds.UInt32,
                ValueRank = ValueRanks.Scalar
            };
            ProcessController.AddChild(state);
            //Method
            MethodState start = new MethodState(ProcessController)
            {
                NodeId = new NodeId(3, NamespaceIndex),
                BrowseName = new QualifiedName("Start", NamespaceIndex),
                DisplayName = new LocalizedText("Start"),
                ReferenceTypeId = ReferenceTypeIds.HasComponent,
                UserExecutable = true,
                Executable = true,
               // AccessRestrictions = AccessRestrictionType.None
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
            //Method - Output
            start.OutputArguments = new PropertyState<Argument[]>(start);
            start.OutputArguments.NodeId = new NodeId(5, NamespaceIndex);
            start.OutputArguments.BrowseName = BrowseNames.OutputArguments;
            start.OutputArguments.DisplayName = start.OutputArguments.BrowseName.Name;
            start.OutputArguments.TypeDefinitionId = VariableTypeIds.PropertyType;
            start.OutputArguments.ReferenceTypeId = ReferenceTypeIds.HasProperty;
            start.OutputArguments.DataType = DataTypeIds.Argument;
            start.OutputArguments.ValueRank = ValueRanks.OneDimension;
            ProcessController.AddChild(start);
            controller.AddChild(ProcessController);
            MethodState stop = new MethodState(ProcessController)
            {
                NodeId = new NodeId(7, NamespaceIndex),
                BrowseName = new QualifiedName("Stop", NamespaceIndex),
                DisplayName = new LocalizedText("Stop"),
                ReferenceTypeId = ReferenceTypeIds.HasComponent,
                UserExecutable = true,
                Executable = true,
                AccessRestrictions = AccessRestrictionType.SigningRequired,
                //UserRolePermissions = RolePermissionTypeCollection
            };
            //Method - Input
            stop.InputArguments = new PropertyState<Argument[]>(stop)
            {
                NodeId = new NodeId(4, NamespaceIndex),
                BrowseName = BrowseNames.InputArguments,
                DisplayName = new LocalizedText(BrowseNames.InputArguments),
                TypeDefinitionId = VariableTypeIds.PropertyType,
                ReferenceTypeId = ReferenceTypeIds.HasProperty,
                DataType = DataTypeIds.Argument,
                ValueRank = ValueRanks.OneDimension
            };
            //Method - Output
            stop.OutputArguments = new PropertyState<Argument[]>(stop)
            {
                NodeId = new NodeId(5, NamespaceIndex),
                BrowseName = BrowseNames.OutputArguments
            };
            stop.OutputArguments.DisplayName = stop.OutputArguments.BrowseName.Name;
            stop.OutputArguments.TypeDefinitionId = VariableTypeIds.PropertyType;
            stop.OutputArguments.ReferenceTypeId = ReferenceTypeIds.HasProperty;
            stop.OutputArguments.DataType = DataTypeIds.Argument;
            stop.OutputArguments.ValueRank = ValueRanks.OneDimension;
            ProcessController.AddChild(stop);
            controller.AddChild(ProcessController);
            // ensure the process object can be found via the server object. 
            if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out IList<IReference> references))
            {
                references = new List<IReference>();
                externalReferences[ObjectIds.ObjectsFolder] = references;
            }
            controller.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);
            references.Add(new NodeStateReference(ReferenceTypeIds.Organizes, false, controller.NodeId));

            // a property to report the process state.

            //_stateNode = state;
            //process.AddChild(state);

            // a method to start the process.
            //MethodState start = new MethodState(process)
            //{
            //    NodeId = new NodeId(3, NamespaceIndex),
            //    BrowseName = new QualifiedName("Start", NamespaceIndex),
            //    ReferenceTypeId = ReferenceTypeIds.HasComponent,
            //    UserExecutable = true,
            //    Executable = true
            //};
            //start.DisplayName = start.BrowseName.Name;

            // add input arguments.
            //start.InputArguments = new PropertyState<Argument[]>(start)
            //{
            //    NodeId = new NodeId(4, NamespaceIndex),
            //    BrowseName = BrowseNames.InputArguments,
            //    TypeDefinitionId = VariableTypeIds.PropertyType,
            //    ReferenceTypeId = ReferenceTypeIds.HasProperty,
            //    DataType = DataTypeIds.Argument,
            //    ValueRank = ValueRanks.OneDimension
            //};
            //start.InputArguments.DisplayName = start.InputArguments.BrowseName.Name;

            //Argument[] args = new Argument[2];
            //args[0] = new Argument
            //{
            //    Name = "Initial State",
            //    Description = "The initialize state for the process.",
            //    DataType = DataTypeIds.UInt32,
            //    ValueRank = ValueRanks.Scalar
            //};

            //args[1] = new Argument
            //{
            //    Name = "Final State",
            //    Description = "The final state for the process.",
            //    DataType = DataTypeIds.UInt32,
            //    ValueRank = ValueRanks.Scalar
            //};

            //start.InputArguments.Value = args;



            //args = new Argument[2];
            //args[0] = new Argument();
            //args[0].Name = "Revised Initial State";
            //args[0].Description = "The revised initialize state for the process.";
            //args[0].DataType = DataTypeIds.UInt32;
            //args[0].ValueRank = ValueRanks.Scalar;

            //args[1] = new Argument();
            //args[1].Name = "Revised Final State";
            //args[1].Description = "The revised final state for the process.";
            //args[1].DataType = DataTypeIds.UInt32;
            //args[1].ValueRank = ValueRanks.Scalar;

            //start.OutputArguments.Value = args;

            //process.AddChild(start);
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

            lock (m_processLock)
            {
                // check if the process is running.
                if (m_processTimer != null)
                {
                    m_processTimer.Dispose();
                    m_processTimer = null;
                }

                // start the process.
                _state = initialState.Value;
                m_finalState = finalState.Value;
                m_processTimer = new Timer(OnUpdateProcess, null, 1000, 1000);

                // the calling function sets default values for all output arguments.
                // only need to update them here.
                outputArguments[0] = _state;
                outputArguments[1] = m_finalState;
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
                lock (m_processLock)
                {
                    // check if increasing.
                    if (_state < m_finalState)
                    {
                        _state++;
                    }

                    // check if decreasing.
                    else if (_state > m_finalState)
                    {
                        _state--;
                    }

                    // check if all done.
                    else
                    {
                        m_processTimer.Dispose();
                        m_processTimer = null;
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
