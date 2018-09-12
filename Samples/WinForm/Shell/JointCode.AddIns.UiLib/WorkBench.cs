
using System.Windows.Forms;

namespace JointCode.AddIns.UiLib
{
    class MessageDialog : IMessageDialog
    {
        public void Show(string message)
        {
            Show(message, "Information");
        }

        public void Show(string message, string title)
        {
            MessageBox.Show(message, title);
        }

        public bool Confirm(string message)
        {
            return Confirm(message, "Confirm");
        }

        public bool Confirm(string message, string title)
        {
            var result = MessageBox.Show(message, title);
            return result == DialogResult.OK || result == DialogResult.Yes;
        }
    }

    public static class WorkBench
    {
        static MainForm _mainForm;

        public static MainForm MainForm
        {
            get
            {
                _mainForm = _mainForm ?? new MainForm();
                return _mainForm;
            }
        }
    }
}
