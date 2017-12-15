//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.Collections.Generic;
using JointCode.AddIns.Core.FileScanning;
using JointCode.AddIns.Parsing;
using JointCode.AddIns.Parsing.Xml;
using JointCode.AddIns.Metadata;
using JointCode.Common.Conversion;

namespace JointCode.AddIns.Resolving
{
    abstract partial class AddinResolver
	{
        protected readonly IndexManager _indexManager;
        protected readonly BodyRepository _bodyRepo;
        protected readonly ConvertionManager _convertionManager;
        readonly List<AddinParser> _addinParsers;

        protected AddinResolver(IndexManager indexManager, BodyRepository bodyRepo, ConvertionManager convertionManager)
        {
            _indexManager = indexManager;
            _bodyRepo = bodyRepo;
            _convertionManager = convertionManager;
            _addinParsers = new List<AddinParser> { new XmlAddinParser() };
        }

        internal abstract bool Resolve(IMessageDialog dialog, FilePackResult filePackResult);
	}
}
