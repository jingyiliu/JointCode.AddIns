
namespace JointCode.AddIns.UiLib
{
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
