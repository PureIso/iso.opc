﻿using System;
using System.Collections.Generic;
using System.IO;
using Iso.Opc.Core.Interfaces;
using Opc.Ua;
using Opc.Ua.Export;
using Opc.Ua.Server;
using LocalizedText = Opc.Ua.LocalizedText;

namespace Iso.Opc.Core.Implementations
{
    /// <summary>
    /// The abstract interface to application node manager
    /// </summary>
    public abstract class AbstractApplicationNodeManagerPlugin : IApplicationNodeManagerPlugin
    {
        #region Virtual Properties
        public virtual string ApplicationName { get; set; }
        public virtual string Description { get; set; }
        public virtual string Author { get; set; }
        public virtual string Version { get; set; }
        public virtual string ResourcePath { get; set; }
        public virtual CustomNodeManager2 ApplicationNodeManager { get; set; }
        public virtual List<string> NamespaceUris { get; set; }
        public virtual List<string> ServerUris { get; set; }
        public virtual NodeStateCollection NodeStateCollection { get; set; }
        public virtual IDictionary<NodeId, IList<IReference>> ExternalReferences { get; set; }
        #endregion

        #region Private Fields
        private BaseObjectState _previousBaseNode;
        private DataTypeState _previousDataType;
        private MethodState _previousMethod;
        #endregion

        #region Virtual Methods
        public virtual void Initialise(CustomNodeManager2 nodeManager)
        {
            ApplicationNodeManager = nodeManager;
            if (string.IsNullOrEmpty(ResourcePath))
                return;
            NamespaceUris = new List<string>();
            NodeStateCollection predefinedNodeStateCollection = new NodeStateCollection();
            Stream stream = new FileStream(ResourcePath, FileMode.Open);
            UANodeSet uaNodeSet = UANodeSet.Read(stream);
            NamespaceUris.AddRange(ApplicationNodeManager.NamespaceUris);
            // Update namespace table
            if (uaNodeSet.NamespaceUris != null)
            {
                foreach (string namespaceUri in uaNodeSet.NamespaceUris)
                {
                    NamespaceUris.Add(namespaceUri);
                    ApplicationNodeManager.SystemContext.NamespaceUris.GetIndexOrAppend(namespaceUri);
                }
            }
            // Update server table
            if (uaNodeSet.ServerUris != null)
            {
                foreach (string serverUri in uaNodeSet.ServerUris)
                {
                    ServerUris.Add(serverUri);
                    ApplicationNodeManager.SystemContext.ServerUris.GetIndexOrAppend(serverUri);
                }
            }
            uaNodeSet.Import(ApplicationNodeManager.SystemContext, predefinedNodeStateCollection);
            NodeStateCollection = predefinedNodeStateCollection;
        }
        public virtual void BindNodeStateCollection()
        {
            if (ExternalReferences == null)
                return;
            if (NodeStateCollection == null)
                return;
            NodeStateCollection parsedNodeState = new NodeStateCollection();
            foreach (NodeState nodeState in NodeStateCollection)
            {
                NodeState bindNodeState = BindNodeStates(ExternalReferences, nodeState, ref parsedNodeState);
                parsedNodeState.Add(bindNodeState);
            }
        }
        public virtual void BindNodeStateActions(NodeState nodeState)
        {
            if (!(nodeState is MethodState methodNodeState))
                return;
            methodNodeState.OnCallMethod = OnGeneratedEmptyMethod;
            methodNodeState.OnCallMethod2 = OnGeneratedEmptyMethod2;
        }
        public virtual void DeleteAddressSpace()
        {
            _previousBaseNode = null;
            _previousMethod = null;
            NodeStateCollection.Clear();
        }
        public virtual ServiceResult OnWriteValue(ISystemContext context, NodeState node, ref object value)
        {
            if (context.UserIdentity == null || context.UserIdentity.TokenType == UserTokenType.Anonymous)
            {
                TranslationInfo info = new TranslationInfo(
                    "BadUserAccessDenied",
                    "en-US",
                    "User cannot change value.");
                return new ServiceResult(StatusCodes.BadUserAccessDenied, new LocalizedText(info));
            }
            try
            {
                // attempt to update file system.
                string filePath = value as string;
                if (node is PropertyState<string> variable && !string.IsNullOrEmpty(variable.Value))
                {
                    FileInfo file = new FileInfo(variable.Value);
                    if (file.Exists)
                        file.Delete();
                }
                if (!string.IsNullOrEmpty(filePath))
                {
                    using (StreamWriter writer = new FileInfo(filePath).CreateText())
                    {
                        //TODO:
                        //writer.WriteLine(System.Security.Principal.WindowsIdentity.GetCurrent().Name);
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
        public virtual ServiceResult OnReadUserAccessLevel(ISystemContext context, NodeState node, ref byte value)
        {
            if (context.UserIdentity == null || context.UserIdentity.TokenType == UserTokenType.Anonymous)
                value = AccessLevels.CurrentRead;
            else
                value = AccessLevels.CurrentReadOrWrite;
            return ServiceResult.Good;
        }
        public virtual NodeState BindNodeStates(IDictionary<NodeId, IList<IReference>> externalReferences, NodeState nodeState, ref NodeStateCollection nodeStateCollectionToBind)
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
                        int index = nodeStateCollectionToBind.FindIndex(x => x.NodeId == _previousMethod.NodeId);
                        if (index == -1)
                            nodeStateCollectionToBind.Add(_previousMethod);
                        else
                            nodeStateCollectionToBind[index] = _previousMethod;
                    }
                    // ensure the process object can be found via the server object. 
                    if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out IList<IReference> references))
                    {
                        externalReferences[ObjectIds.ObjectsFolder] = references = new List<IReference>();
                    }
                    references.Add(new NodeStateReference(ReferenceTypeIds.Organizes, false, _previousBaseNode.NodeId));
                    _previousMethod = null;
                    break;
                case NodeClass.DataType:
                    if (!(nodeState is DataTypeState dataTypeState))
                        return nodeState;
                    BindNodeStateActions(dataTypeState);
                    _previousDataType = dataTypeState;
                    int index2 = nodeStateCollectionToBind.FindIndex(x => x.NodeId == _previousDataType.NodeId);
                    if (index2 == -1)
                        nodeStateCollectionToBind.Add(_previousDataType);
                    else
                        nodeStateCollectionToBind[index2] = _previousDataType;
                    break;
                case NodeClass.Method:
                    if (!(nodeState is MethodState methodState))
                        return nodeState;
                    BindNodeStateActions(methodState);
                    _previousBaseNode?.AddChild(methodState);
                    if (_previousMethod != null)
                    {
                        int index = nodeStateCollectionToBind.FindIndex(x => x.NodeId == _previousMethod.NodeId);
                        if (index == -1)
                            nodeStateCollectionToBind.Add(_previousMethod);
                        else
                            nodeStateCollectionToBind[index] = _previousMethod;
                    }
                    _previousMethod = methodState;
                    return methodState;
                case NodeClass.Variable:
                    if (_previousMethod != null)
                    {
                        if (!(nodeState is PropertyState propertyState))
                            return nodeState;
                        BindNodeStateActions(propertyState);
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
                                Value = ExtensionObject.ToArray(propertyState.Value, typeof(Argument)) as Argument[],
                                OnReadUserAccessLevel = OnReadUserAccessLevel,
                                OnSimpleWriteValue = OnWriteValue
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
                                Value = ExtensionObject.ToArray(propertyState.Value, typeof(Argument)) as Argument[],
                                OnReadUserAccessLevel = OnReadUserAccessLevel,
                                OnSimpleWriteValue = OnWriteValue
                            };
                        }
                    }
                    break;
                default:
                    if (_previousBaseNode != null)
                    {
                        int index = nodeStateCollectionToBind.FindIndex(x => x.NodeId == _previousBaseNode.NodeId);
                        if (index == -1)
                            nodeStateCollectionToBind.Add(_previousBaseNode);
                        else
                            nodeStateCollectionToBind[index] = _previousBaseNode;
                    }
                    if (_previousMethod != null)
                    {
                        int index = nodeStateCollectionToBind.FindIndex(x => x.NodeId == _previousMethod.NodeId);
                        if (index == -1)
                            nodeStateCollectionToBind.Add(_previousMethod);
                        else
                            nodeStateCollectionToBind[index] = _previousMethod;
                    }
                    _previousBaseNode = null;
                    _previousMethod = null;
                    break;
            }
            return nodeState;
        }
        public virtual ServiceResult OnGeneratedEmptyMethod(ISystemContext context, MethodState method, IList<object> inputArguments, IList<object> outputArguments)
        {
            Console.WriteLine($"Method Called - Displayed Name: {method.DisplayName}");
            return ServiceResult.Good;
        }
        public virtual ServiceResult OnGeneratedEmptyMethod2(ISystemContext context, MethodState method, NodeId objectId, IList<object> inputArguments, IList<object> outputArguments)
        {
            Console.WriteLine($"Method Called - Displayed Name: {method.DisplayName}");
            return ServiceResult.Good;
        }
        #endregion
    }
}
