using System;
using System.Collections.Generic;
using System.Text;

namespace JointCode.AddIns.AppLib
{
    public class NameProvider
    {
        public string GetAllAssemblyNames()
        {
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            var result = string.Empty;
            foreach (var asm in asms)
                result += asm.FullName + "|";
            return result;
        }

        public string GetName(string input)
        {
            return typeof(NameProvider).Assembly.Location + "@" + input;
        }
    }
}
