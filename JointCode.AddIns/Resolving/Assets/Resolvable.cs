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
        protected ResolutionStatus _resolutionStatus = ResolutionStatus.Pending;

        protected Resolvable(AddinResolution declaringAddin) { _declaringAddin = declaringAddin; }
		
        /// <summary>
        /// the addin that provides this asset.
        /// if this value is null, it means that this asset is provided by application or the runtime.
        /// </summary>
        internal AddinResolution DeclaringAddin { get { return _declaringAddin; } }

        // 在执行子对象的解析时，利用此属性来判断父对象的解析状态（例如 Extension 和 ExtensionBuilder），而不直接执行解析。因为直接解析父对象时，如果父子对象之间存在循环依赖，则会造成无限循环解析
        /// <summary>
        /// get the resolution status without doing the resolution
        /// </summary>
        internal ResolutionStatus ResolutionStatus { get { return _resolutionStatus; } }

        internal ResolutionStatus Resolve(ResolutionResult resolutionResult, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            if (_resolutionStatus.IsFailed())
                return ResolutionStatus.Failed;
            if (_resolutionStatus.IsSuccess())
                return ResolutionStatus.Success;
            _resolutionStatus = DoResolve(resolutionResult, convertionManager, ctx);
            return _resolutionStatus;
        }

        protected abstract ResolutionStatus DoResolve(ResolutionResult resolutionResult, ConvertionManager convertionManager, ResolutionContext ctx);

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

    abstract class TypeConstrainedResolvable : Resolvable
    {
        protected TypeConstrainedResolvable(AddinResolution declaringAddin) : base(declaringAddin) { }
        internal TypeResolution Type { get; set; }
    }
}