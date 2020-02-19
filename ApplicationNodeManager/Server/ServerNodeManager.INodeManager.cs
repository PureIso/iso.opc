using System;
using System.Collections.Generic;
using System.IO;
using Opc.Ua;
using Opc.Ua.Export;
using Opc.Ua.Server;
using LocalizedText = Opc.Ua.LocalizedText;

namespace Iso.Opc.ApplicationNodeManager.Server
{
    public sealed partial class ServerNodeManager
    {
        #region INodeManager Members
        /// <summary>
        /// Does any initialization required before the address space can be used.
        /// </summary>
        /// <remarks>
        /// The externalReferences is an out parameter that allows the node manager to link to nodes
        /// in other node managers. For example, the 'Objects' node is managed by the CoreNodeManager and
        /// should have a reference to the root folder node(s) exposed by this node manager.  
        /// </remarks>
        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            lock (Lock)
            {
                CreateProcessNode(externalReferences);
                foreach (string file in Directory.EnumerateFiles(PredefinedXMLNodeDirectory, "*.xml"))
                {
                   ImportXMLModels(externalReferences, file);
                }
            }
        }

        private void ImportXMLModels(IDictionary<NodeId, IList<IReference>> externalReferences, string resourcePath)
        {
            try
            {
                List<string> namespaceUriList = new List<string>();
                NodeStateCollection predefinedNodeStateCollection = new NodeStateCollection();
                Stream stream = new FileStream(resourcePath, FileMode.Open);
                UANodeSet uaNodeSet = UANodeSet.Read(stream);
                namespaceUriList.AddRange(NamespaceUris);
                // Update namespace table
                if (uaNodeSet.ServerUris != null)
                {
                    foreach (string namespaceUri in uaNodeSet.NamespaceUris)
                    {
                        namespaceUriList.Add(namespaceUri);
                        SystemContext.NamespaceUris.GetIndexOrAppend(namespaceUri);
                    }
                }
                NamespaceUris = namespaceUriList;
                // Update server table
                if (uaNodeSet.ServerUris != null)
                {
                    foreach (string serverUri in uaNodeSet.ServerUris)
                    {
                        SystemContext.ServerUris.GetIndexOrAppend(serverUri);
                    }
                }
                uaNodeSet.Import(SystemContext, predefinedNodeStateCollection);
                NodeStateCollection parsedNodeState = new NodeStateCollection();
                foreach (NodeState nodeState in predefinedNodeStateCollection)
                {
                    NodeState bindNodeState = BindNodeStates(externalReferences, nodeState, ref parsedNodeState);
                    parsedNodeState.Add(bindNodeState);
                }
                foreach (NodeState nodeState in parsedNodeState)
                {
                    AddPredefinedNode(SystemContext, nodeState);
                }
                AddReverseReferences(externalReferences);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Import XML exception: {e.StackTrace}");
            }
        }

        private BaseObjectState _previousBaseNode;
        private MethodState _previousMethod;

        private ServiceResult OnGeneratedMethod(ISystemContext context, MethodState method, IList<object> inputArguments, IList<object> outputArguments)
        {
            Console.WriteLine($"Method Called - Displayed Name: {method.DisplayName}");
            return ServiceResult.Good;
        }

        private ServiceResult OnGeneratedMethod2(ISystemContext context, MethodState method, NodeId objectId, IList<object> inputArguments, IList<object> outputArguments)
        {
            Console.WriteLine($"Method Called - Displayed Name: {method.DisplayName}");
            return ServiceResult.Good;
        }


        private NodeState BindNodeStates(IDictionary<NodeId, IList<IReference>> externalReferences, NodeState nodeState, ref NodeStateCollection bindedNodeStates)
        {
            switch (nodeState.NodeClass)
            {
                case NodeClass.Object:
                    if (!(nodeState is BaseObjectState baseObjectState))
                        return nodeState;
                    _previousBaseNode = baseObjectState;
                    //Bind previous method now since it will be cleared
                    if (_previousMethod != null)
                    {
                        int index = bindedNodeStates.FindIndex(x => x.NodeId == _previousMethod.NodeId);
                        if (index == -1)
                            bindedNodeStates.Add(_previousMethod);
                        else
                            bindedNodeStates[index] = _previousMethod;
                    }

                    // ensure the process object can be found via the server object. 
                    if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out IList<IReference> references))
                    {
                        externalReferences[ObjectIds.ObjectsFolder] = references = new List<IReference>();
                    }
                    references.Add(new NodeStateReference(ReferenceTypeIds.Organizes, false, _previousBaseNode.NodeId));
                    _previousMethod = null;
                    break;
                case NodeClass.Method:
                    if (!(nodeState is MethodState methodState))
                        return nodeState;
                    methodState.OnCallMethod = OnGeneratedMethod;
                    methodState.OnCallMethod2 = OnGeneratedMethod2;
                    _previousBaseNode?.AddChild(methodState);
                    if (_previousMethod != null)
                    {
                        int index = bindedNodeStates.FindIndex(x => x.NodeId == _previousMethod.NodeId);
                        if (index == -1)
                            bindedNodeStates.Add(_previousMethod);
                        else
                            bindedNodeStates[index] = _previousMethod;
                    }
                    _previousMethod = methodState;
                    return methodState;
                case NodeClass.Variable:
                    if (_previousMethod != null)
                    {
                        if (!(nodeState is PropertyState propertyState))
                            return nodeState;
                        if (propertyState.DisplayName == BrowseNames.InputArguments)
                        {
                            _previousMethod.InputArguments = new PropertyState<Argument[]>(_previousMethod)
                            {
                                NodeId = propertyState.NodeId,
                                BrowseName = propertyState.BrowseName,
                                DisplayName = propertyState.DisplayName,
                                TypeDefinitionId = propertyState.TypeDefinitionId,
                                ReferenceTypeId = propertyState.ReferenceTypeId,
                                DataType = propertyState.DataType,
                                ValueRank = propertyState.ValueRank,
                                Value = ExtensionObject.ToArray(propertyState.Value, typeof(Argument)) as Argument[]
                            };
                        }
                        else if (propertyState.DisplayName == BrowseNames.OutputArguments)
                        {
                            _previousMethod.OutputArguments = new PropertyState<Argument[]>(_previousMethod)
                            {
                                NodeId = propertyState.NodeId,
                                BrowseName = propertyState.BrowseName,
                                DisplayName = propertyState.DisplayName,
                                TypeDefinitionId = propertyState.TypeDefinitionId,
                                ReferenceTypeId = propertyState.ReferenceTypeId,
                                DataType = propertyState.DataType,
                                ValueRank = propertyState.ValueRank,
                                Value = ExtensionObject.ToArray(propertyState.Value, typeof(Argument)) as Argument[]
                            };
                        }
                    }
                    break;
                default:
                    if (_previousBaseNode != null)
                    {
                        int index = bindedNodeStates.FindIndex(x => x.NodeId == _previousBaseNode.NodeId);
                        if (index == -1)
                            bindedNodeStates.Add(_previousBaseNode);
                        else
                            bindedNodeStates[index] = _previousBaseNode;
                    }
                    if (_previousMethod != null)
                    {
                        int index = bindedNodeStates.FindIndex(x => x.NodeId == _previousMethod.NodeId);
                        if (index == -1)
                            bindedNodeStates.Add(_previousMethod);
                        else
                            bindedNodeStates[index] = _previousMethod;
                    }
                    _previousBaseNode = null;
                    _previousMethod = null;
                    break;
            }
            return nodeState;
        }
        public ServiceResult OnWriteValue(ISystemContext context, NodeState node, ref object value)
        {
            if (context.UserIdentity == null || context.UserIdentity.TokenType == UserTokenType.Anonymous)
            {
                TranslationInfo info = new TranslationInfo(
                    "BadUserAccessDenied",
                    "en-US",
                    "User cannot change value.");
                return new ServiceResult(StatusCodes.BadUserAccessDenied, new LocalizedText(info));
            }
            // attempt to update file system.
            try
            {
                string filePath = value as string;
                if (node is PropertyState<string> variable && !string.IsNullOrEmpty(variable.Value))
                {
                    FileInfo file = new FileInfo(variable.Value);
                    if (file.Exists)
                    {
                        file.Delete();
                    }
                }
                if (!string.IsNullOrEmpty(filePath))
                {
                    FileInfo file = new FileInfo(filePath);
                    using (StreamWriter writer = file.CreateText())
                    {
                        writer.WriteLine(System.Security.Principal.WindowsIdentity.GetCurrent().Name);
                    }
                }
                value = filePath;
            }
            catch (Exception e)
            {
                return ServiceResult.Create(e, StatusCodes.BadUserAccessDenied, "Could not update file system.");
            }
            return ServiceResult.Good;
        }
        public ServiceResult OnReadUserAccessLevel(ISystemContext context, NodeState node, ref byte value)
        {
            if (context.UserIdentity == null || context.UserIdentity.TokenType == UserTokenType.Anonymous)
            {
                value = AccessLevels.CurrentRead;
            }
            else
            {
                value = AccessLevels.CurrentReadOrWrite;
            }
            return ServiceResult.Good;
        }
        #endregion

        #region Overridden Methods
        /// <summary>
        /// Frees any resources allocated for the address space.
        /// </summary>
        public override void DeleteAddressSpace()
        {
            lock (Lock)
            {
                // TBD
            }
        }
        /// <summary>
        /// Returns a unique handle for the node.
        /// </summary>
        protected override NodeHandle GetManagerHandle(ServerSystemContext context, NodeId nodeId, IDictionary<NodeId, NodeState> cache)
        {
            lock (Lock)
            {
                // quickly exclude nodes that are not in the namespace. 
                if (!IsNodeIdInNamespace(nodeId))
                {
                    return null;
                }
                if (!PredefinedNodes.TryGetValue(nodeId, out NodeState node))
                {
                    return null;
                }
                NodeHandle handle = new NodeHandle
                {
                    NodeId = nodeId,
                    Node = node,
                    Validated = true
                };
                return handle;
            }
        }
        /// <summary>
        /// Verifies that the specified node exists.
        /// </summary>
        protected override NodeState ValidateNode(ServerSystemContext context, NodeHandle handle, IDictionary<NodeId, NodeState> cache)
        {
            // not valid if no root.
            if (handle == null)
            {
                return null;
            }
            // check if previously validated.
            return handle.Validated ? handle.Node : null;
            // TBD
        }
        #endregion
    }
}
