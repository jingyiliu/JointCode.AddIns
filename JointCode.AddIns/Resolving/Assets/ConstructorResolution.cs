//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.Collections.Generic;
using Mono.Cecil;

namespace JointCode.AddIns.Resolving.Assets
{
    class ConstructorResolution
    {
        readonly TypeResolution _declaringType;
        readonly MethodDefinition _ctor;
    	
        internal ConstructorResolution(TypeResolution declaringType, MethodDefinition ctor) 
        {
            _declaringType = declaringType;
            _ctor = ctor;
        }
    	
        internal TypeResolution DeclaringType { get { return _declaringType; } }
        internal bool IsPublic { get { return _ctor.IsPublic; } }
    	
        internal List<ParameterResolution> GetParameters()
        {
            if (!_ctor.HasParameters) 
                return null;
            var result = new List<ParameterResolution>(_ctor.Parameters.Count);
            for (int i = 0; i < result.Count; i++)
            {
                //var parameter = _ctor.Parameters[i];
                //parameter.ParameterType.Resolve().
                result.Add(ParameterResolution.DefaultInstance);
            }
            return result;
        }
    }
}