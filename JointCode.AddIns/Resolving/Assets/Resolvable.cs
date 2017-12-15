//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.Common.Conversion;

namespace JointCode.AddIns.Resolving.Assets
{
    // represent an asset that needs to resolve.
    abstract class Resolvable
    {
        readonly AddinResolution _declaringAddin;
		
        protected Resolvable(AddinResolution declaringAddin) { _declaringAddin = declaringAddin; }
		
        /// <summary>
        /// the addin that provides this asset.
        /// if this value is null, it means that this asset is provided by application or the runtime.
        /// </summary>
        internal AddinResolution DeclaringAddin { get { return _declaringAddin; } }

        internal abstract ResolutionStatus Resolve(IMessageDialog dialog, ConvertionManager convertionManager, ResolutionContext ctx);

        // @returns ResolutionStatus.Success when the type is:
        // 1. provided by the runtime (gac), and therefore no need to resolve (it is always there).
        // 2. defined in the same addin as the current asset (extension or extension point/builder), therefore it's self-sufficient and no need to resolve.
        // 3. defined in an addin that has been resolved.
        protected ResolutionStatus ResolveType(TypeResolution type)
        {
            var addin = type.Assembly.DeclaringAddin;
            return (addin == null // the type is provided by runtime
                   || ReferenceEquals(addin, _declaringAddin)) // the type is defined in the same addin as the current asset
                ? ResolutionStatus.Success
                : addin.ResolutionStatus; // the type is defined in an addin that has been resolved
        }

        protected ResolutionStatus ResolveAddin(Resolvable otherAsset)
        {
            return (ReferenceEquals(otherAsset.DeclaringAddin, DeclaringAddin) || otherAsset.DeclaringAddin == null) 
                ? ResolutionStatus.Success 
                : otherAsset.DeclaringAddin.ResolutionStatus;
        }
    }
}