using System;

namespace Iso.Opc.ApplicationNodeManager.Models
{
    [Serializable]
    public class CertificateRequest
    {
        public Guid RequestId { get; set; }
        public Guid ApplicationId { get; set; }
        public int State { get; set; }
        public string CertificateGroupId { get; set; }
        public string CertificateTypeId { get; set; }
        public byte[] CertificateSigningRequest { get; set; }
        public string SubjectName { get; set; }
        public string[] DomainNames { get; set; }
        public string PrivateKeyFormat { get; set; }
        public string PrivateKeyPassword { get; set; }
        public string AuthorityId { get; set; }
        public byte[] Certificate { get; set; }
    }
}
