using System;
using System.Collections.Generic;
using System.Text;

namespace JointCode.AddIns.Shell.AddinsSharedLib
{
    public class CommonClass
    {
        public string GetLocation()
        {
            var asm = typeof(CommonClass).Assembly;
            return asm.GetName().Name + "|" + asm.CodeBase;
        }

        public string GetLoadedAssemblies()
        {
            var resutl = string.Empty;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (!assembly.GlobalAssemblyCache)
                    resutl += assembly.GetName().Name + ": "  + assembly.Location + Environment.NewLine;
            return resutl;
        }
    }
}
