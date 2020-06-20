using System.Collections.Generic;
using Opc.Ua;
using Opc.Ua.Server;

namespace Iso.Opc.Interface.Interfaces
{
    /// <summary>
    /// The interface to application node manager
    /// </summary>
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
        /// <summary>
        /// The path to required resource file
        /// </summary>
        string ResourcePath { get; set; }
        /// <summary>
        /// The system node manager
        /// </summary>
        CustomNodeManager2 ApplicationNodeManager { get; set; }
        /// <summary>
        /// The required namespace uris
        /// </summary>
        List<string> NamespaceUris { get; set; }
        /// <summary>
        /// The required server uris
        /// </summary>
        List<string> ServerUris { get; set; }
        /// <summary>
        /// The node state collection
        /// </summary>
        NodeStateCollection NodeStateCollection { get; set; }
        /// <summary>
        /// The external references
        /// </summary>
        IDictionary<NodeId, IList<IReference>> ExternalReferences { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Initialise the plugin by setting namespaces and uris
        /// </summary>
        /// <param name="nodeManager">The node manager</param>
        void Initialise(CustomNodeManager2 nodeManager);
        /// <summary>
        /// Delete and clearing system objects and memories
        /// </summary>
        void DeleteAddressSpace();
        /// <summary>
        /// Bind a group of node states
        /// </summary>
        void BindNodeStateCollection();
        /// <summary>
        /// Bind a node state to a method or a variable
        /// </summary>
        /// <param name="nodeState">The node state</param>
        void BindNodeStateActions(NodeState nodeState);
        /// <summary>
        /// Bind a node state and append it to a collection
        /// </summary>
        /// <param name="externalReferences">The external references</param>
        /// <param name="nodeState">The node state</param>
        /// <param name="noteStateCollectionToBind">The node state collection</param>
        /// <returns>The bind node state</returns>
        NodeState BindNodeStates(IDictionary<NodeId, IList<IReference>> externalReferences, NodeState nodeState, ref NodeStateCollection noteStateCollectionToBind);
        /// <summary>
        /// Validate write access and write to a node
        /// </summary>
        /// <param name="context">The system context</param>
        /// <param name="node">The node state</param>
        /// <param name="value">The value to write</param>
        /// <returns>The action result</returns>
        ServiceResult OnWriteValue(ISystemContext context, NodeState node, ref object value);
        /// <summary>
        /// Validate the read user access level and read from a node
        /// </summary>
        /// <param name="context">The system context</param>
        /// <param name="node">The node state</param>
        /// <param name="value">The value to read</param>
        /// <returns>The action result</returns>
        ServiceResult OnReadUserAccessLevel(ISystemContext context, NodeState node, ref byte value);
        /// <summary>
        /// A method to be added to a method state node # 1
        /// </summary>
        /// <param name="context">The system context</param>
        /// <param name="method">The method state</param>
        /// <param name="inputArguments">The input arguments</param>
        /// <param name="outputArguments">The output arguments</param>
        /// <returns>The action result</returns>
        ServiceResult OnGeneratedEmptyMethod(ISystemContext context, MethodState method, IList<object> inputArguments, IList<object> outputArguments);
        /// <summary>
        /// A method to be added to a method state node # 2
        /// </summary>
        /// <param name="context">The system context</param>
        /// <param name="method">The method state</param>
        /// <param name="objectId">The object node state</param>
        /// <param name="inputArguments">The input arguments</param>
        /// <param name="outputArguments">The output arguments</param>
        /// <returns>The action result</returns>
        ServiceResult OnGeneratedEmptyMethod2(ISystemContext context, MethodState method, NodeId objectId, IList<object> inputArguments, IList<object> outputArguments);
        #endregion
    }
}