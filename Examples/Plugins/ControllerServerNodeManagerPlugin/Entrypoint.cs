using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Iso.Opc.Interface;
using Opc.Ua;
using Opc.Ua.Server;

namespace ControllerServerNodeManagerPlugin
{
    /// <summary>
    /// Controller ser node manager plugin entry point
    /// </summary>
    public class EntryPoint : AbstractApplicationNodeManagerPlugin
    {
        #region Fields
        private readonly object _processLock = new object();
        private uint _state;
        private Dictionary<string, AlarmConditionState>  m_alarms = new Dictionary<string, AlarmConditionState>();
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
            base.NamespaceUris = new List<string> { $"http://{Dns.GetHostName()}/UA/Default" };
        }

        #region Overridden Methods
        public override void Initialise(CustomNodeManager2 nodeManager)
        {
            ApplicationNodeManager = nodeManager;
            ushort namespaceIndex = ApplicationNodeManager.SystemContext.NamespaceUris.GetIndexOrAppend(NamespaceUris[0]);
            /* ***************************************** */
            /* ControllerType                            */
            /* ***************************************** */
            BaseObjectState controller = new BaseObjectState(null)
            {
                NodeId = new NodeId(1, namespaceIndex),
                BrowseName = new QualifiedName("Controllers", namespaceIndex),
                DisplayName = new LocalizedText("Controllers"),
                EventNotifier = EventNotifiers.SubscribeToEvents | EventNotifiers.HistoryRead | EventNotifiers.HistoryWrite,
                TypeDefinitionId = ObjectTypeIds.BaseObjectType,
            };
            controller.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);
            //Variables
            PropertyState<uint> state = new PropertyState<uint>(controller)
            {
                NodeId = new NodeId(2, namespaceIndex),
                BrowseName = new QualifiedName("State", namespaceIndex),
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
                NodeId = new NodeId(3, namespaceIndex),
                BrowseName = new QualifiedName("Start", namespaceIndex),
                DisplayName = new LocalizedText("Start"),
                ReferenceTypeId = ReferenceTypeIds.HasComponent,
                UserExecutable = true,
                Executable = true
            };
            //Method - Input
            start.InputArguments = new PropertyState<Argument[]>(start)
            {
                NodeId = new NodeId(4, namespaceIndex),
                BrowseName = BrowseNames.InputArguments,
                DisplayName = new LocalizedText(BrowseNames.InputArguments),
                TypeDefinitionId = VariableTypeIds.PropertyType,
                ReferenceTypeId = ReferenceTypeIds.HasProperty,
                DataType = DataTypeIds.Argument,
                ValueRank = ValueRanks.OneDimension,
                OnReadUserAccessLevel = OnReadUserAccessLevel,
                OnSimpleWriteValue = OnWriteValue,
                OnReportEvent = (context, node, target) =>
                {
                    Console.WriteLine("ssssssssss");
                },
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
                NodeId = new NodeId(5, namespaceIndex),
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
            controller.AddReference(ReferenceTypeIds.HasNotifier,true,ObjectIds.Server);
            //create status object
            //AggregationModel.

            NodeStateCollection = new NodeStateCollection { controller };
        }
        public override void BindNodeStateCollection()
        {
            if (ExternalReferences == null)
                return;
            // ensure the process object can be found via the server object. 
            if (!ExternalReferences.TryGetValue(ObjectIds.ObjectsFolder, out IList<IReference> references))
            {
                ExternalReferences[ObjectIds.ObjectsFolder] = references = new List<IReference>();
            }
            references.Add(new NodeStateReference(ReferenceTypeIds.Organizes, false, NodeStateCollection[0].NodeId));
        }
        #endregion

        #region Methods
        private ServiceResult OnStart(ISystemContext context, MethodState method, IList<object> inputArguments, IList<object> outputArguments)
        {
            // all arguments must be provided.
            if (inputArguments.Count < 2)
                return StatusCodes.BadArgumentsMissing;
            // check the data type of the input arguments.
            uint? initialState = inputArguments[0] as uint?;
            uint? finalState = inputArguments[1] as uint?;
            if (initialState == null || finalState == null)
                return StatusCodes.BadTypeMismatch; 
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
                //method.OnReportEvent
            }
            // signal update to state node.
            lock (ApplicationNodeManager.Lock)
            {
                _stateNode.Value = _state;
                _stateNode.ClearChangeMasks(ApplicationNodeManager.SystemContext, true);
            }

            //BaseEventState stopMonitoringEvent = new BaseEventState(method);
            //BaseEventState onStartBaseEvent = new BaseEventState(method);
            TranslationInfo info = new TranslationInfo(
                "OnStart",
                "en-US",
                "The Confirm method was called.");
            //onStartBaseEvent.Initialize(context,method,EventSeverity.High,new LocalizedText(info));
            //bool valid = onStartBaseEvent.Validate(context);
            //// Report the event at Server level
            //ApplicationNodeManager.Server.ReportEvent(onStartBaseEvent);

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
            if(valid) 
                ApplicationNodeManager.Server.ReportEvent(auditUpdateMethodEventState);
            return ServiceResult.Good;
        }
        private void OnUpdateProcess(object state)
        {
            try
            {
                lock (_processLock)
                {
                    // check if increasing.
                    if (_state < _finalState)
                        _state++;
                    // check if decreasing.
                    else if (_state > _finalState)
                        _state--;
                    // check if all done.
                    else
                    {
                        _processTimer.Dispose();
                        _processTimer = null;
                    }
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
