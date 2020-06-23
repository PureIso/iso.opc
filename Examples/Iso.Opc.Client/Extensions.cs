using System.Windows.Forms;

namespace Iso.Opc.Client
{
    public static class Extensions
    {
        public static void SetTextThreadSafe(this Control control, string text)
        {
            if (control.InvokeRequired)
                control.Invoke(new MethodInvoker(delegate { SetTextThreadSafe(control, text); }));
            else
            {
                control.Text = text;
            }
        }
    }
}
