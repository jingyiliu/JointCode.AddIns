//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Metadata.Assets;
using JointCode.Common.Conversion;
using System;

namespace JointCode.AddIns.Resolving.Assets
{
    /// <summary>
    /// The addin activator
    /// </summary>
    class AddinActivatorResolution : TypeConstrainedResolvable
    {
        readonly string _typeName;

        internal AddinActivatorResolution(AddinResolution declaringAddin, string typeName)
            : base(declaringAddin)
        {
            _typeName = typeName;
        }

        //internal TypeResolution Type { get; set; }
        internal string TypeName { get { return _typeName; } }

        bool ApplyRules(ResolutionResult resolutionResult, ResolutionContext ctx)
        {
            var result = true;
            if (!Type.IsClass || Type.IsAbstract)
            {
                resolutionResult.AddError(string.Format("The specified addin activator type [{0}] is not a concrete class!", Type.TypeName));
                result = false;
            }
            if (!this.DeclaresInSameAddin())
            {
                resolutionResult.AddError(string.Format(
                    "The addin activator type [{0}] is expected to be defined and declared in a same addin, while its defining addin is [{1}], and its declaring addin is [{2}], which is not the same as the former!",
                    Type.TypeName, Type.Assembly.DeclaringAddin.AddinId.Guid, DeclaringAddin.AddinId.Guid));
                result = false;
            }
            if (!this.InheritFromAddinActivatorInterface(resolutionResult, ctx))
            {
                resolutionResult.AddError(string.Format("The specified addin activator type [{0}] does not implement the required interface (IAddinActivator)!", Type.TypeName));
                result = false;
            }
            if (!this.Type.HasPublicParameterLessConstructor())
            {
                resolutionResult.AddError(string.Format("The specified addin activator type [{0}] do not have a public parameter-less constructor!", Type.TypeName));
                result = false;
            }
            return result;
        }

        protected override ResolutionStatus DoResolve(ResolutionResult resolutionResult, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            if (Type == null)
            {
                Type = ctx.GetUniqueAddinType(DeclaringAddin, TypeName);
                if (Type == null)
                {
                    resolutionResult.AddError(string.Format("Can not find the specified addin activator type [{0}]!", TypeName));
                    return ResolutionStatus.Failed;
                }

                if (!ApplyRules(resolutionResult, ctx))
                    return ResolutionStatus.Failed;

                //if (Type.Assembly.DeclaringAddin != null &&
                //    !ReferenceEquals(Type.Assembly.DeclaringAddin, DeclaringAddin))
                //{
                //    AssemblyResolutionSet assemblySet;
                //    if (!ctx.TryGetAssemblySet(Type.Assembly.AssemblyKey, out assemblySet))
                //        throw new Exception();
                //    DeclaringAddin.AddReferencedAssemblySet(assemblySet);
                //}
            }

            return ResolveType(Type);
        }

        internal AddinActivatorRecord ToRecord()
        {
            return new AddinActivatorRecord(Type.Assembly.Uid, Type.TypeName);
        }
    }
}