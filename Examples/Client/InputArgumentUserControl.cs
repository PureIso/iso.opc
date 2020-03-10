using Opc.Ua;
using System.Windows.Forms;

namespace Client
{
    public partial class InputArgumentUserControl : UserControl
    {
        #region Properties
        /// <summary>
        /// Casts a value to the specified target type and return the object value
        /// </summary>
        public object ValueInput => string.IsNullOrEmpty(valueInputTextBox.Text) ? TypeInfo.Cast("0", TypeInfo.BuiltInType) : TypeInfo.Cast(valueInputTextBox.Text,TypeInfo.BuiltInType);
        public TypeInfo TypeInfo { get; private set; }
        #endregion

        public InputArgumentUserControl()
        {
            InitializeComponent();
        }

        #region Methods
        public void Initialise(string value, string description, string name, TypeInfo typeInfo)
        {
            TypeInfo = typeInfo;
            valueInputTextBox.Text = string.IsNullOrEmpty(value)?"0":value;
            inputArgumentDescriptionLabel.Text = $"Description: {description}";
            inputArgumentNameLabel.Text = $"{name}:";
            inputArgumentTypeLabel.Text = $"{typeInfo.BuiltInType.ToString()}";
        }
        #endregion
    }
}
