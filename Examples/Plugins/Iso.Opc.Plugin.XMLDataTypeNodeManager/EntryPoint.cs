using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Iso.Opc.Core.Implementations;
using Iso.Opc.Plugin.XMLDataTypeNodeManager.Models;
using Opc.Ua;

namespace Iso.Opc.Plugin.XMLDataTypeNodeManager
{
    public class Processor : IEncodeable
    {
        public static string DisplayName = "Processor";
        private ExtensionObject _extensionObject;
        public string Name { get; set; }
        public double MaxSpeed { get; set; }
        public double MinSpeed { get; set; }
        public bool IsEqual(IEncodeable encodeable)
        {
            if (Object.ReferenceEquals(this, encodeable))
            {
                return true;
            }

            return false;
        }

        public ExpandedNodeId TypeId => NodeId.Null;

        public ExpandedNodeId BinaryEncodingId => NodeId.Null;

        public ExpandedNodeId XmlEncodingId => NodeId.Null;

        public void Encode(IEncoder encoder)
        {
            encoder.WriteString("Name", Name);
            encoder.WriteDouble("MaxSpeed", MaxSpeed);
            encoder.WriteDouble("MinSpeed", MinSpeed);
        }

        public void Decode(IDecoder decoder)
        {
            Name = decoder.ReadString("Name");
            MaxSpeed = decoder.ReadDouble("MaxSpeed");
            MinSpeed = decoder.ReadDouble("MinSpeed");
        }

        public override string ToString() => $"{{ Name={Name}; MaxSpeed={MaxSpeed}; MinSpeed={MinSpeed}; }}";

    }
    public class EntryPoint : AbstractApplicationNodeManagerPlugin
    {
        #region Fields
        private List<string> _applications;
        private Processor _processor;
        #endregion

        public EntryPoint()
        {
            base.ApplicationName = "XMLDataTypeServerNodeManager";
            base.Author = "Ola";
            base.Description = "XML Data Type Plugin Test";
            base.Version = "1.0.0.0";
            string directoryName = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            if (string.IsNullOrEmpty(directoryName))
                return;
            string xmlFilePath = Path.Combine(directoryName, "plugin/xml_example_types.xml");
            base.ResourcePath = xmlFilePath;
        }

        public override void BindNodeStateActions(NodeState nodeState)
        {
            switch (nodeState)
            {
                case MethodState methodNodeState when methodNodeState.DisplayName.Text == PLCControllerNode.MethodNameAddApplication:
                    methodNodeState.OnCallMethod = AddApplication;
                    break;
                case DataTypeState dataTypeState when dataTypeState.DisplayName.Text == Processor.DisplayName:
                    // add the types defined in the quickstart information model library to the factory.
                    ApplicationNodeManager.Server.Factory.AddEncodeableTypes(typeof(Processor).Assembly);
                    ApplicationNodeManager.Server.Factory.AddEncodeableTypes(this.GetType().Assembly);
                    EncodeableFactory.GlobalFactory.AddEncodeableType(typeof(Processor));
                    _processor = ExtensionObject.ToEncodeable(new ExtensionObject(dataTypeState.NodeId)) as Processor;
                    break;
            }
        }

        private ServiceResult AddApplication(ISystemContext context, MethodState method, IList<object> inputArguments, IList<object> outputArguments)
        {
            if (inputArguments.Count != 1)
                return StatusCodes.BadArgumentsMissing;
            // check the data type of the input arguments.
            string value = inputArguments[0] as string;
            if (value == null)
                return StatusCodes.BadTypeMismatch;

            if (_applications == null)
                _applications = new List<string>();
            _applications.Add(value);

            if (_processor == null)
            {
                _processor = new Processor();
                _processor.Name = "Turbo";
                _processor.MinSpeed = 0;
                _processor.MaxSpeed = 0;
            }
            _processor.MinSpeed = 1;
            _processor.MaxSpeed += 1;

            //Could not encode outgoing 
            outputArguments[0] = _applications.ToArray();
            outputArguments[1] = _processor;

            //Report
            TranslationInfo info = new TranslationInfo(
                "AddApplication",
                "en-US",
                "The Confirm method was called.");
            AuditUpdateMethodEventState auditUpdateMethodEventState = new AuditUpdateMethodEventState(method);
            auditUpdateMethodEventState.Initialize(
                context,
                method,
                EventSeverity.Low,
                new LocalizedText(info),
                ServiceResult.IsGood(StatusCodes.Good),
                DateTime.UtcNow);
            auditUpdateMethodEventState.SourceName.Value = "Attribute/Call";
            auditUpdateMethodEventState.MethodId = new PropertyState<NodeId>(method)
            {
                Value = method.NodeId
            };
            auditUpdateMethodEventState.InputArguments = new PropertyState<object[]>(method)
            {
                Value = new object[] { inputArguments }
            };
            auditUpdateMethodEventState.SetChildValue(context, BrowseNames.InputArguments, inputArguments.ToArray(), true);
            bool valid = auditUpdateMethodEventState.Validate(context);
            if (valid)
                ApplicationNodeManager.Server.ReportEvent(auditUpdateMethodEventState);
            return ServiceResult.Good;
        }
    }
}
