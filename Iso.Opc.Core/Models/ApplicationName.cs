using System;

namespace Iso.Opc.Core.Models
{
    [Serializable]
    public class ApplicationName
    {
        public Guid ApplicationId { get; set; }
        public string Locale { get; set; }
        public string Text { get; set; }
    }
}
