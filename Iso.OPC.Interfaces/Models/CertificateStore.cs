using System;

namespace Iso.Opc.Core.Models
{
    [Serializable]
    public class CertificateStore
    {
        public CertificateStore()
        {
            TrustListId = Guid.NewGuid();
        }
        public string Path { get; set; }
        public string AuthorityId { get; set; }
        public Guid TrustListId { get; private set; }
    }
}
