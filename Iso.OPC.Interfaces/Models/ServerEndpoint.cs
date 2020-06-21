using System;

namespace Iso.Opc.Core.Models
{
    [Serializable]
    public class ServerEndpoint
    {
        public Guid ApplicationId { get; set; }
        public string DiscoveryUrl { get; set; }
    }
}
