using System.Collections.Generic;
using Opc.Ua;
using Opc.Ua.Server;

namespace Iso.Opc.Interface
{
    public interface IApplicationNodeManagerPlugin
    {
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

        CustomNodeManager2 ApplicationNodeManager { get; set; }

        void Initialise(IDictionary<NodeId, IList<IReference>> externalReferences, ISystemContext context, NodeState predefinedNode);
    }
}
