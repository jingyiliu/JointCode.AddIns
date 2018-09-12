using JointCode.AddIns.Resolving;

namespace JointCode.AddIns.Parsing
{
    abstract class AddinManifest
    {
        internal abstract bool Introspect(INameConvention nameConvention, ResolutionResult resolutionResult);
        internal abstract bool TryParse(ResolutionResult resolutionResult, out AddinResolution result);
    }
}