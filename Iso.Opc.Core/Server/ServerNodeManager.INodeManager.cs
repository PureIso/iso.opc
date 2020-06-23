using System.Collections.Generic;
using Iso.Opc.Core.Implementations;
using Opc.Ua;
using Opc.Ua.Server;

namespace Iso.Opc.Core.Server
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
                foreach (AbstractApplicationNodeManagerPlugin abstractApplicationNodeManagerPlugin in _applicationNodeManagerPluginService.PluginBaseNodeManagers)
                {
                    abstractApplicationNodeManagerPlugin.ExternalReferences = externalReferences;
                    abstractApplicationNodeManagerPlugin.BindNodeStateCollection();
                    if (abstractApplicationNodeManagerPlugin.NodeStateCollection == null)
                        continue;
                    foreach (NodeState nodeState in abstractApplicationNodeManagerPlugin.NodeStateCollection)
                    {
                        AddPredefinedNode(SystemContext, nodeState);
                    }
                }
            }
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
                foreach (AbstractApplicationNodeManagerPlugin abstractApplicationNodeManagerPlugin in _applicationNodeManagerPluginService.PluginBaseNodeManagers)
                {
                    abstractApplicationNodeManagerPlugin.DeleteAddressSpace();
                }
                base.DeleteAddressSpace();
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
                if (PredefinedNodes == null)
                    return null;
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
        }
        #endregion
    }
}
