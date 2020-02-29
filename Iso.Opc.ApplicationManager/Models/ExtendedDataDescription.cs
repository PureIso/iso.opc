using System.Collections.Generic;

namespace Iso.Opc.ApplicationManager.Models
{
    public class ExtendedDataDescription
    {
        public DataDescription ParentReferenceDescription { get; }
        public List<DataDescription> VariableReferenceDescriptions { get; set; }
        public List<DataDescription> MethodReferenceDescriptions { get; set; }

        public ExtendedDataDescription(DataDescription parent)
        {
            ParentReferenceDescription = parent;
            VariableReferenceDescriptions = new List<DataDescription>();
            MethodReferenceDescriptions = new List<DataDescription>();
        }
    }
}
