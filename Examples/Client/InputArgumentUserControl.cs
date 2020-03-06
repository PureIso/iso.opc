using System.Windows.Forms;
using Opc.Ua;

namespace Client
{
    public partial class InputArgumentUserControl : UserControl
    {
        #region Properties
    	public Argument Argument { get; set;}
    	#endregion

        public InputArgumentUserControl(Argument argument)
        {
            InitializeComponent();
            Argument = argument;
            inputArgumentDescriptionLabel.Text = $"Description: {argument.Description.Text}";
            inputArgumentNameLabel.Text = $"Name: {argument.Name}";
            inputArgumentTypeLabel.Text = $"Name: {argument.DataType}";
        }
    }
}
