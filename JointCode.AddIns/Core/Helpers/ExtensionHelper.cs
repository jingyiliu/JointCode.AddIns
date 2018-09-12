
using System;
using JointCode.AddIns.Resolving.Assets;

namespace JointCode.AddIns.Core.Helpers
{
    class ExtensionHelper
    {
        internal static string GetExtensionPointPath(IExtensionPointPathInfo pathInfo)
        {
            return pathInfo.Name;
        }

        internal static string GetExtensionPointName(string path)
        {
            var index = path.IndexOf(SysConstants.PathSeparator);
            return index >= 0 ? path.Substring(0, index) : path;
        }

        internal static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (path[0] == ' ' || path[path.Length - 1] == ' ')
                path = path.Trim();

            if (path == string.Empty)
                return null;

            return path[path.Length - 1] == SysConstants.PathSeparator 
                ? path.Substring(0, path.Length - 1) 
                : path;
        }
    }
}
