//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.Collections.Generic;
using JointCode.AddIns.Core.Runtime;
using JointCode.AddIns.Metadata.Assets;

namespace JointCode.AddIns.Core
{
    class Addin
    {
        readonly IAddinContext _adnContext;

        internal Addin(Dictionary<Guid, Addin> guid2Addins, RuntimeSystem runtimeSystem,
            AddinIndexRecord addinIndex)
        {
            var addinFileSystem = new AddinFileSystem(guid2Addins, addinIndex);
            _adnContext = new AddinContext(runtimeSystem, addinFileSystem);
        }

        internal IAddinContext AddinContext { get { return _adnContext; } }
        internal AddinIndexRecord AddinIndexRecord { get { return _adnContext.AddinFileSystem.AddinIndexRecord; } }
        internal AddinBodyRecord AddinBodyRecord { get; set; }
    }
}
