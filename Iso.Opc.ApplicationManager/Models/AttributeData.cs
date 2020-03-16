using Opc.Ua;
using System;
//using System.Text.RegularExpressions;

namespace Iso.Opc.ApplicationManager.Models
{
    public class AttributeData
    {
        #region Research Reference
        //https://documentation.unified-automation.com/uasdkhp/1.0.0/html/_l2_ua_node_classes.html
        #endregion

        #region Constants
        private const uint MaxAttributeDataCount = 27;
        #endregion

        #region Properties
        /// <summary>
        /// Base Node Class
        /// NodeId = 1
        /// Use: Mandatory
        /// DataType: NodeId
        /// Description: Uniquely identifies a Node in an OPC UA server and is used to address the Node in the OPC UA Services
        /// </summary>
        public NodeId NodeId { get; set; }
        /// <summary>
        /// Base Node Class
        /// NodeClass = 2
        /// Use: Mandatory
        /// DataType: NodeClass
        /// Description: An enumeration identifying the NodeClass of a Node such as Object, Variable or Method
        /// </summary>
        public NodeClass NodeClass { get; set; }
        /// <summary>
        /// Base Node Class
        /// BrowseName = 3
        /// Use: Mandatory
        /// DataType: QualifiedName
        /// Description: Identifies the Node when browsing the OPC UA server. It is not localized
        /// </summary>
        public QualifiedName BrowseName { get; set; }
        /// <summary>
        /// Base Node Class
        /// DisplayName = 4
        /// Use: Mandatory
        /// DataType: LocalizedText
        /// Description: Contains the Name of the Node that should be used to display the name in a user interface. Therefore, it is localized
        /// </summary>
        public LocalizedText DisplayName { get; set; }
        /// <summary>
        /// Base Node Class
        /// Description = 5
        /// Use: Optional
        /// DataType: LocalizedText
        /// Description: This optional Attribute contains a localized textual description of the Node
        /// </summary>
        public LocalizedText Description { get; set; }
        /// <summary>
        /// Base Node Class
        /// WriteMask = 6
        /// Use: Optional
        /// DataType: UInt32
        /// Description: Is optional and specifies which Attributes of the Node are writable, i.e., can be modified by an OPC UA client
        /// </summary>
        public AttributeWriteMask WriteMask { get; set; }
        /// <summary>
        /// Base Node Class
        /// UserWriteMask = 7
        /// Use: Optional
        /// DataType: UInt32
        /// Description: Is optional and specifies which Attributes of the Node can be modified by the user currently connected to the server
        /// </summary>
        public AttributeWriteMask UserWriteMask { get; set; }
        /// <summary>
        /// ReferenceType && VariableType && ObjectType && DataType
        /// IsAbstract = 8
        /// Use: Mandatory
        /// DataType: Boolean
        /// Description: [ReferenceType] Specifies if the ReferenceType can be used for References or is only used for organizational purposes in the ReferenceType hierarchy
        /// Description2: [VariableType] This Attribute indicates if the VariableType is abstract and therefore cannot directly be used as type definition
        /// Description3: [ObjectType] This Attribute indicates whether the ObjectType is concrete or abstract and therefore cannot directly be used as type definition
        /// Description4: [DataType] Indicates whether the DataType is abstract. An abstract DataType can be used in the DataType Attribute. However, concrete values must be of a concrete DataType
        /// </summary>
        public bool IsAbstract { get; set; }
        /// <summary>
        /// ReferenceType
        /// Symmetric = 9
        /// Use: Mandatory
        /// DataType: Boolean
        /// Description: Indicates whether the Reference is symmetric, i.e., whether the meaning is the same in forward and inverse direction
        /// </summary>
        public bool Symmetric { get; set; }
        /// <summary>
        /// ReferenceType
        /// InverseName = 10
        /// Use: Optional
        /// DataType: LocalizedText
        /// Description: This optional Attribute specifies the semantic of the Reference in inverse direction. It can only be applied for nonsymmetric References and must be provided if such a ReferenceType is not abstract
        /// </summary>
        public LocalizedText InverseName { get; set; }
        /// <summary>
        /// View
        /// ContainsNoLoops = 11
        /// Use: Mandatory
        /// DataType: Boolean
        /// Description: This Attributes indicates whether the Nodes contained in the View do span a nonlooping hierarchy when following hierarchical References
        /// </summary>
        public bool ContainsNoLoops { get; set; }
        /// <summary>
        /// View
        /// EventNotifier = 12
        /// Use: Mandatory
        /// DataType: Byte
        /// Description: This Attribute represents a bit mask that identifies whether the View can be used to subscribe to Events and whether the history of Events is accessible and changeable
        /// </summary>
        public byte EventNotifier { get; set; }
        public string EventNotifierString { get; set; }
        /// <summary>
        /// VariableType && Variable
        /// Value = 13
        /// Use: [VariableType] Optional && [Variable] Mandatory
        /// DataType: Base Data Type
        /// Description: [VariableType] This optional Attribute defines a default value for instances of this VariableType. The data type of the value is specified by the DataType, ValueRank, and ArrayDimensions Attributes
        /// Description: [Variable] The actual value of the Variable. The data type of the value is specified by the DataType, ValueRank, and ArrayDimensions Attributes
        /// </summary>
        public Variant Value { get; set; }
        /// <summary>
        /// VariableType && Variable
        /// DataType = 14
        /// Use: Mandatory
        /// DataType: NodeId
        /// Description: [VariableType] Defines the DataType of the Value Attribute for instances of this type as well as for the Value Attribute of the VariableType if provided
        /// Description2: [Variable] DataTypes are represented as Nodes in the Address Space. This Attribute contains a NodeId of such a Node and thus defines the DataType of the Value Attribute
        /// </summary>
        public NodeId DataType { get; set; }
        /// <summary>
        /// VariableType && Variable
        /// ValueRank = 15
        /// Use: Mandatory
        /// DataType: Int32
        /// Description: [VariableType] Identifies if the value is an array and when it is an array it allows specifying the dimensions of the array
        /// </summary>
        public int ValueRank { get; set; }
        public string ValueRankString { get; set; }
        /// <summary>
        /// VariableType && Variable
        /// ArrayDimensions = 16
        /// Use: Optional
        /// DataType: UInt32[]
        /// Description: This optional Attribute allows specifying the size of an array and can only be used if the value is an array. For each dimension of the array a corresponding entry defines the length of the dimension
        /// </summary>
        public ReadOnlyList<int> ArrayDimensions { get; set; }
        /// <summary>
        /// Variable
        /// AccessLevel = 17
        /// Use: Mandatory
        /// DataType: Byte
        /// Description: A bit mask indicating whether the current value of the Value Attribute is readable and writable as well as whether the history of the value is readable and changeable
        /// </summary>
        public byte AccessLevel { get; set; }
        public string AccessLevelString { get; set; }
        /// <summary>
        /// Variable
        /// UserAccessLevel = 18
        /// Use: Mandatory
        /// DataType: Byte
        /// Description: Contains the same information as the AccessLevel but takes user access rights into account
        /// </summary>
        public byte UserAccessLevel { get; set; }
        public string UserAccessLevelString { get; set; }
        /// <summary>
        /// Variable
        /// MinimumSamplingInterval = 19
        /// Use: Optional
        /// DataType: Double
        /// Description: This optional Attribute provides the information how fast the OPC UA server can detect changes of the Value Attribute. 
        /// For Values not directly managed by the server, e.g., the temperature of a temperature sensor, the server may need to scan the device for changes (polling) 
        /// and thus is not able to detect changes faster than this minimum interval
        /// </summary>
        public double MinimumSamplingInterval { get; set; }
        /// <summary>
        /// Variable
        /// Historizing = 20
        /// Use: Mandatory
        /// DataType: Boolean
        /// Description: Indicates whether the server currently collects history for the Value. The AccessLevel Attribute does not provide that information, it only specifies whether some history is available
        /// </summary>
        public bool Historizing { get; set; }
        /// <summary>
        /// Method
        /// Executable = 21
        /// Use: Mandatory
        /// DataType: Boolean
        /// Description: A flag indicating if the Method can be invoked at the moment
        /// </summary>
        public bool Executable { get; set; }
        /// <summary>
        /// Method
        /// UserExecutable = 22
        /// Use: Mandatory
        /// DataType: Boolean
        /// Description: Same as the Executable Attribute taking user access rights into account
        /// </summary>
        public bool UserExecutable { get; set; }

        /// <summary>
        /// DataTypeDefinition = 23
        /// Reference (pointer) to a type description which defines the node
        /// The DTD tells you what the standard type is for an object so that a Client device knows what to expect in terms of Node structure
        /// ref: https://reference.opcfoundation.org/v104/Core/docs/Part6/F.12/
        /// </summary>
        public DataTypeDefinition DataTypeDefinition { get; set; }
        /// <summary>
        /// RolePermissions = 24
        /// ref: https://reference.opcfoundation.org/v104/Core/DataTypes/RolePermissionType/
        /// </summary>
        public RolePermissionTypeCollection RolePermissions { get; set; }
        /// <summary>
        /// UserRolePermissions = 25
        /// </summary>
        public RolePermissionTypeCollection UserRolePermissions { get; set; }
        /// <summary>
        /// AccessRestrictions = 26
        /// ref: https://reference.opcfoundation.org/v104/Core/DataTypes/AccessRestrictionType/
        /// </summary>
        public AccessRestrictionType AccessRestrictions { get; set; }
        /// <summary>
        /// AccessLevelEx = 27
        /// </summary>
        public AccessLevelExType AccessLevelEx { get; set; }
        #endregion

        public void Initialise(DataValueCollection dataValueCollection)
        {
            if (dataValueCollection.Count != MaxAttributeDataCount)
                return;
            //Base Node Class
            NodeId = dataValueCollection[0].Value as NodeId;
            NodeClass = (NodeClass)dataValueCollection[1].Value;
            BrowseName = dataValueCollection[2].Value as QualifiedName;
            DisplayName = dataValueCollection[3].Value as LocalizedText;
            Description = dataValueCollection[4].Value as LocalizedText;
            if (Description == null)
                Description = "N/A";
            WriteMask = (AttributeWriteMask)dataValueCollection[5].Value;
            UserWriteMask = (AttributeWriteMask)dataValueCollection[6].Value;
            //ReferenceType && VariableType && ObjectType && DataType
            IsAbstract = (bool?) dataValueCollection[7].Value ?? false;
            //ReferenceType
            Symmetric = (bool?) dataValueCollection[8].Value ?? false;
            InverseName = dataValueCollection[9].Value as LocalizedText;
            //View
            ContainsNoLoops = (bool?) dataValueCollection[10].Value ?? false;
            EventNotifier = (byte?) dataValueCollection[11].Value ?? 0;
            EventNotifierString = EventNotifierToString(EventNotifier);
            //VariableType && Variable
            if(NodeClass == NodeClass.Variable || NodeClass == NodeClass.VariableType)
            {
                Value = new Variant(dataValueCollection[12].Value);
                DataType = dataValueCollection[13].Value as NodeId;
                ValueRank = (int?)dataValueCollection[14].Value ?? 0;
                ValueRankString = ValueRankToString(ValueRank);
                ArrayDimensions = dataValueCollection[15].Value as ReadOnlyList<int>;
            }
           
            //Variable
            AccessLevel = (byte?) dataValueCollection[16].Value ?? 3;
            AccessLevelString = AccessLevelToString(AccessLevel);
            UserAccessLevel = (byte?) dataValueCollection[17].Value ?? 3;
            UserAccessLevelString = AccessLevelToString(UserAccessLevel);
            MinimumSamplingInterval = (double?) dataValueCollection[18].Value ?? 0;
            Historizing = (bool?) dataValueCollection[19].Value ?? false;
            //Method
            Executable = (bool?) dataValueCollection[20].Value ?? false;
            UserExecutable = (bool?) dataValueCollection[21].Value ?? false;

            //Others
            DataTypeDefinition = dataValueCollection[22].Value as DataTypeDefinition;
            RolePermissions = dataValueCollection[23].Value as RolePermissionTypeCollection;
            UserRolePermissions = dataValueCollection[24].Value as RolePermissionTypeCollection;
            AccessRestrictions = dataValueCollection[25].Value == null? AccessRestrictionType.None : (AccessRestrictionType)Enum.ToObject(typeof(AccessRestrictionType), dataValueCollection[25].Value);
            AccessLevelEx = dataValueCollection[26].Value == null ? AccessLevelExType.None : (AccessLevelExType)Enum.ToObject(typeof(AccessLevelExType), dataValueCollection[26].Value);
        }

        private static string EventNotifierToString(byte eventNotifier)
        {
            switch (eventNotifier)
            {
                case EventNotifiers.None: return nameof(EventNotifiers.None);
                case EventNotifiers.SubscribeToEvents: return nameof(EventNotifiers.SubscribeToEvents);
                case EventNotifiers.HistoryRead: return nameof(EventNotifiers.HistoryRead);
                case EventNotifiers.HistoryWrite: return nameof(EventNotifiers.HistoryWrite);
                default: return nameof(EventNotifiers.None);
            }
        }
        private static string AccessLevelToString(byte accessLevel)
        {
            switch (accessLevel)
            {
                case AccessLevels.None: return nameof(AccessLevels.None);
                case AccessLevels.CurrentRead: return nameof(AccessLevels.CurrentRead);
                case AccessLevels.CurrentWrite: return nameof(AccessLevels.CurrentWrite);
                case AccessLevels.CurrentReadOrWrite: return nameof(AccessLevels.CurrentReadOrWrite);
                case AccessLevels.HistoryRead: return nameof(AccessLevels.HistoryRead);
                case AccessLevels.HistoryWrite: return nameof(AccessLevels.HistoryWrite);
                case AccessLevels.HistoryReadOrWrite: return nameof(AccessLevels.HistoryReadOrWrite);
                case AccessLevels.SemanticChange: return nameof(AccessLevels.SemanticChange);
                case AccessLevels.StatusWrite: return nameof(AccessLevels.StatusWrite);
                case AccessLevels.TimestampWrite: return nameof(AccessLevels.TimestampWrite);
                default: return nameof(AccessLevels.CurrentReadOrWrite);
            }
        }
        private static string ValueRankToString(int valueRank)
        {
            switch (valueRank)
            {
                case ValueRanks.Any: return nameof(ValueRanks.Any);
                case ValueRanks.Scalar: return nameof(ValueRanks.Scalar);
                case ValueRanks.ScalarOrOneDimension: return nameof(ValueRanks.ScalarOrOneDimension);
                case ValueRanks.OneOrMoreDimensions: return nameof(ValueRanks.OneOrMoreDimensions);
                case ValueRanks.OneDimension: return nameof(ValueRanks.OneDimension);
                case ValueRanks.TwoDimensions: return nameof(ValueRanks.TwoDimensions);
                default: return nameof(ValueRanks.Scalar);
            }
        }
        //private static object ParseArgument(string value)
        //{
        //    if (string.IsNullOrEmpty(value))
        //        return value;
        //    string splitOn = @"\{([^{}]*)\}";
        //    string[] words = Regex.Split(value, splitOn);
        //    Argument[] arguments = new Argument[words.Length];
        //    foreach(string word in words)
        //    {
        //        string[] values = word.Split('|');
        //        if (values.Length != 5)
        //            return value;
        //        Argument argument = new Argument();
        //        argument.Name = values[0];
        //        argument.DataType = new NodeId(values[1]);
        //        argument.ValueRank = Int32.Parse(values[2]);
        //        argument.ArrayDimensions = (values[3] as object) as UInt32Collection;
        //        argument.Description = values[4];
        //    }
        //    //Regex filterRegex = new Regex(Regex.Escape("{") + "([^{}]*)" + Regex.Escape("}"));
        //    return null;
        //}
    }
}
