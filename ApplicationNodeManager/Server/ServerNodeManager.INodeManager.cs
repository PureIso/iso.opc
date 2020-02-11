﻿using System;
using System.Collections.Generic;
using System.IO;
using Opc.Ua;
using Opc.Ua.Server;

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
            }
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
                PropertyState<string> variable = node as PropertyState<string>;
                if (!string.IsNullOrEmpty(variable.Value))
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
                NodeState node = null;
                if (!PredefinedNodes.TryGetValue(nodeId, out node))
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
        protected override NodeState ValidateNode(ServerSystemContext context,NodeHandle handle,IDictionary<NodeId, NodeState> cache)
        {
            // not valid if no root.
            if (handle == null)
            {
                return null;
            }
            // check if previously validated.
            if (handle.Validated)
            {
                return handle.Node;
            }

            // TBD

            return null;
        }
        #endregion

        #region Overridden Methods
        #endregion
    }
}
