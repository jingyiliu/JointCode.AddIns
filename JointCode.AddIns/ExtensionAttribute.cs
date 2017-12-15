//#if NET20

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public class ExtensionAttribute : Attribute { }
}

//#endif