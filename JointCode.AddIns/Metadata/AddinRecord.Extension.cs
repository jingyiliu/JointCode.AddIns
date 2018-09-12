//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.Collections.Generic;
using JointCode.AddIns.Metadata.Assets;

namespace JointCode.AddIns.Metadata
{
    partial class AddinRecord
    {
        List<ExtensionPointRecord> _extensionPoints;
		List<ExtensionBuilderRecordGroup> _ebRecordGroups;
		List<ExtensionRecordGroup> _exRecordGroups;
		
        internal List<ExtensionPointRecord> ExtensionPoints { get { return _extensionPoints; } } 
        internal List<ExtensionBuilderRecordGroup> ExtensionBuilderGroups { get { return _ebRecordGroups; } }
        internal List<ExtensionRecordGroup> ExtensionGroups { get { return _exRecordGroups; } }

        #region Add Methods
        internal void AddExtensionPoint(ExtensionPointRecord item)
        {
            _extensionPoints = _extensionPoints ?? new List<ExtensionPointRecord>();
            _extensionPoints.Add(item);
        }

        internal void AddExtensionBuilderGroup(ExtensionBuilderRecordGroup item)
        {
            _ebRecordGroups = _ebRecordGroups ?? new List<ExtensionBuilderRecordGroup>();
            _ebRecordGroups.Add(item);
        }

        internal void AddExtensionGroup(ExtensionRecordGroup item)
        {
            _exRecordGroups = _exRecordGroups ?? new List<ExtensionRecordGroup>();
            _exRecordGroups.Add(item);
        } 
        #endregion

        #region Utils
        internal List<ExtensionBuilderRecord> GetAllExtensionBuilders()
        {
            if (ExtensionPoints == null && ExtensionBuilderGroups == null)
                return null;

            var extensionBuilders = new List<ExtensionBuilderRecord>();

            if (ExtensionPoints != null)
            {
                foreach (var extensionPoint in ExtensionPoints)
                {
                    if (extensionPoint.Children != null)
                        extensionBuilders.AddRange(extensionPoint.Children);
                }

                for (int i = 0; i < extensionBuilders.Count; i++)
                {
                    var extensionBuilder = extensionBuilders[i];
                    if (extensionBuilder.Children != null)
                        extensionBuilders.AddRange(extensionBuilder.Children);
                }
            }

            if (ExtensionBuilderGroups != null)
            {
                var startIndex = extensionBuilders.Count;
                foreach (var exBuilderGroup in ExtensionBuilderGroups)
                    extensionBuilders.AddRange(exBuilderGroup.Children);

                for (int i = startIndex; i < extensionBuilders.Count; i++)
                {
                    var extensionBuilder = extensionBuilders[i];
                    if (extensionBuilder.Children != null)
                        extensionBuilders.AddRange(extensionBuilder.Children);
                }
            }

            return extensionBuilders;
        } 
        #endregion

        internal ExtensionPointRecord GetExtensionPoint(string extensionPointPath)
        {
            if (_extensionPoints == null)
                return null;
            foreach (var ep in _extensionPoints)
            {
                if (ep.Path == extensionPointPath)
                    return ep;
            }
            return null;
        }

        /// <summary>
        /// Gets an extension builder group used to build extensions for the given extension point.
        /// </summary>
        /// <param name="extensionPointPath">The extension point id.</param>
		internal ExtensionBuilderRecordGroup GetExtensionBuilderGroup(string extensionPointPath)
		{
			if (_ebRecordGroups == null) 
				return null;
			foreach (var ebRecordGroup in _ebRecordGroups) 
			{
				if (ebRecordGroup.ParentPath.StartsWith(extensionPointPath))
					return ebRecordGroup;
			}
			return null;
		}
		
        /// <summary>
        /// Gets an extension group that extends the given extension point.
        /// </summary>
        /// <param name="extensionPointPath">The extension point id.</param>
		internal ExtensionRecordGroup GetExtensionGroup(string extensionPointPath)
		{
			if (_exRecordGroups == null) 
				return null;
			foreach (var exRecordGroup in _exRecordGroups) 
			{
				if (exRecordGroup.ParentPath.StartsWith(extensionPointPath))
					return exRecordGroup;
			}
			return null;
		}
    }
}
