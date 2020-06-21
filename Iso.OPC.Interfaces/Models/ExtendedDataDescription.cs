using System.Collections.Generic;

namespace Iso.Opc.Core.Models
{
    public class ExtendedDataDescription
    {
        public DataDescription DataDescription { get; set; }
        public List<DataDescription> VariableDataDescriptions { get; set; }
        public List<ExtendedDataDescription> MethodDataDescriptions { get; set; }
        public List<ExtendedDataDescription> ObjectDataDescriptions { get; set; }

        public ExtendedDataDescription()
        {
            DataDescription = new DataDescription();
            VariableDataDescriptions = new List<DataDescription>();
            MethodDataDescriptions = new List<ExtendedDataDescription>();
            ObjectDataDescriptions = new List<ExtendedDataDescription>();
        }
        public ExtendedDataDescription(DataDescription dataDescription)
        {
            DataDescription = dataDescription;
            VariableDataDescriptions = new List<DataDescription>();
            MethodDataDescriptions = new List<ExtendedDataDescription>();
            ObjectDataDescriptions = new List<ExtendedDataDescription>();
        }
    }
}
