using System.Windows.Forms;
using Opc.Ua;

namespace Client
{
    public partial class InputArgumentUserControl : UserControl
    {
        #region Properties
        public object ValueInput => valueInputTextBox.Text;
    	#endregion

        public InputArgumentUserControl(Argument argument)
        {
            InitializeComponent();
            valueInputTextBox.Text = "0";
            inputArgumentDescriptionLabel.Text = $"Description: {argument.Description.Text}";
            inputArgumentNameLabel.Text = $"Name: {argument.Name}";
            inputArgumentTypeLabel.Text = $"Name: {argument.DataType}";
        }
    }
}
