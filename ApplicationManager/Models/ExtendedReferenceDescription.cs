using System.Collections.Generic;
using Opc.Ua;

namespace Iso.Opc.ApplicationManager.Models
{
    public class ExtendedReferenceDescription
    {
        public ReferenceDescription ParentReferenceDescription { get; }
        public List<ReferenceDescription> VariableReferenceDescriptions { get; set; }
        public List<ReferenceDescription> MethodReferenceDescriptions { get; set; }

        public ExtendedReferenceDescription(ReferenceDescription parent)
        {
            ParentReferenceDescription = parent;
            VariableReferenceDescriptions = new List<ReferenceDescription>();
            MethodReferenceDescriptions = new List<ReferenceDescription>();
        }
    }
}
