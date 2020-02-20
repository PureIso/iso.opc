using System;
using System.Collections.Generic;

namespace Iso.Opc.ApplicationNodeManager.Models
{
    [Serializable]
    public class Application
    {
        public Application()
        {
            Certificate = new Dictionary<string, byte[]>();
            TrustListId = new Dictionary<string, Guid>();
        }
        public uint ID { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationUri { get; set; }
        public string ApplicationName { get; set; }
        public int ApplicationType { get; set; }
        public string ProductUri { get; set; }
        public string ServerCapabilities { get; set; }
        public Dictionary<string, byte[]> Certificate { get; }
        public Dictionary<string, Guid> TrustListId { get; }
    }
}
