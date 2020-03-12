using System.Collections.Generic;
using Opc.Ua;
using Opc.Ua.Server;

namespace Iso.Opc.Interface
{
    public interface IApplicationNodeManagerPlugin
    {
        #region Properties
        /// <summary>
        /// The Application Name
        /// </summary>
        string ApplicationName { get; set; }

        /// <summary>
        /// The application Description
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// The Application Author
        /// </summary>
        string Author { get; set; }

        /// <summary>
        /// The Application Version
        /// </summary>
        string Version { get; set; }

        string ResourcePath { get; set; }

        CustomNodeManager2 ApplicationNodeManager { get; set; }

        List<string> NamespaceUris { get; set; }

        List<string> ServerUris { get; set; }

        NodeStateCollection NodeStateCollection { get; set; }
        #endregion

        #region Methods
        void DeleteAddressSpace();
        void BindMethod(MethodState methodState);
        void Initialise(CustomNodeManager2 nodeManager, IDictionary<NodeId, IList<IReference>> externalReferences, string resourcePath = null);
        ServiceResult OnWriteValue(ISystemContext context, NodeState node, ref object value);
        ServiceResult OnReadUserAccessLevel(ISystemContext context, NodeState node, ref byte value);
        NodeState BindNodeStates(IDictionary<NodeId, IList<IReference>> externalReferences, NodeState nodeState,
            ref NodeStateCollection noteStateCollectionToBind);
        ServiceResult OnGeneratedEmptyMethod(ISystemContext context, MethodState method, IList<object> inputArguments,
            IList<object> outputArguments);
        ServiceResult OnGeneratedEmptyMethod2(ISystemContext context, MethodState method, NodeId objectId,
            IList<object> inputArguments, IList<object> outputArguments);
        #endregion
    }
}