using Opc.Ua;

namespace Iso.Opc.Core.Models
{
    public class DataDescription
    {
        #region Properties
        public ReferenceDescription ReferenceDescription { get; set; }
        public AttributeData AttributeData { get; set; }
        #endregion
    }
}
