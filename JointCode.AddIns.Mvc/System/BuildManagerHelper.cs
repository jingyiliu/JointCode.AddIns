using System.Collections.Generic;
using System.Reflection;
using System.Web.Compilation;

namespace JointCode.AddIns.Mvc.System
{
    sealed class BuildManagerHelper
    {
        static HashSet<Assembly> s_dynamicallyAddedReferencedAssembly;

        static BuildManagerHelper()
        {
            var flds = typeof(BuildManager).GetFields(BindingFlags.Static | BindingFlags.NonPublic);
            foreach (var fld in flds)
            {
                if (fld.Name == "s_dynamicallyAddedReferencedAssembly")
                {
                    s_dynamicallyAddedReferencedAssembly = fld.GetValue(null) as HashSet<Assembly>;
                    break;
                }
                var fldType = fld.FieldType;
                if (fldType.IsGenericType)
                {
                    
                }
                //if (fld != null)
                //    s_dynamicallyAddedReferencedAssembly = fld.GetValue(null) as HashSet<Assembly>;
            }
        }

        internal static void Release()
        {
            s_dynamicallyAddedReferencedAssembly = null;
        }

        internal static void AddReferencedAssemblyNormally(Assembly asm)
        { BuildManager.AddReferencedAssembly(asm); }

        internal static void AddReferencedAssembly(Assembly asm)
        {
            if (s_dynamicallyAddedReferencedAssembly != null)
                s_dynamicallyAddedReferencedAssembly.Add(asm);
        }

        internal static void RemoveReferencedAssembly(Assembly asm)
        {
            if (s_dynamicallyAddedReferencedAssembly != null)
                s_dynamicallyAddedReferencedAssembly.Remove(asm);
        }
    }
}