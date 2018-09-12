using System;

namespace JointCode.AddIns.Core
{
    class DefaultAssemblyLoadPolicy : AssemblyLoadPolicy
    {
        public override AssemblyLoadMethod GetAssemblyLoadMethod(Addin addin)
        {
            return AssemblyLoadMethod.LoadBytes;
        }
    }
}
