
namespace JointCode.AddIns.Core.Helpers
{
    class StringHelper
    {
        internal static string GetExtensionPointId(string path)
        {
            var index = path.IndexOf(SysConstants.PathSeparator);
            return index >= 0 ? path.Substring(0, index) : path;
        }
    }
}
