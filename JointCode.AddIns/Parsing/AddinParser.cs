//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Core.FileScanning;
using JointCode.Common.Logging;

namespace JointCode.AddIns.Parsing
{
    abstract class AddinParser
    {
        internal abstract bool TryParse(/*ILogger logger, */ScanFilePack scanFilePack, out AddinManifest addinManifest);
    }
}
