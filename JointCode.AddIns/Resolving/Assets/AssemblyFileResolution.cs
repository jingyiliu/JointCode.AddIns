//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Core.Storage;
using System;

namespace JointCode.AddIns.Resolving.Assets
{
    class AssemblyFileResolution : DataFileResolution
    { 
        internal AssemblyFileResolution() { Uid = UidStorage.InvalidAssemblyUid; }

#if NET_4_0
        //// Only .net 4.0 provide AppDomain.AssemblyResolve event with an RequestingAssembly as event parameter, which 
        //// can be used to determined what addin initializes the assembly resolution request, that we can distinguish 
        //// different addin.
        //internal bool IsPublic { get; set; }
#endif
        internal int Uid { get; set; }
        internal DateTime LastWriteTime { get; set; }
    }
}