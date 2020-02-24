using Opc.Ua;

namespace Iso.Opc.ApplicationNodeManager.Server
{
    public sealed partial class ServerNodeManager
    {
        #region INodeIdFactory Members
        /// <summary>
        /// Creates the NodeId for the specified node.
        /// </summary>
        public override NodeId New(ISystemContext context, NodeState node)
        {
            if (!(node is BaseInstanceState instance) || instance.Parent == null)
                return node.NodeId;
            return new NodeId(++_nextNodeId, NamespaceIndex);
        }
        #endregion
    }
}
