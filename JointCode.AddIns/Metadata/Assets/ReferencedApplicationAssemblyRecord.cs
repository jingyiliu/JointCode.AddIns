using JointCode.Common;

namespace JointCode.AddIns.Metadata.Assets
{
    class ReferencedApplicationAssemblyRecord : ReferencedAssemblyRecord
    {
        internal new static MyFunc<ReferencedApplicationAssemblyRecord> Factory = () => new ReferencedApplicationAssemblyRecord();
    }
}