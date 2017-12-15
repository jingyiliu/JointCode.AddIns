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
using System.IO;
using JointCode.Common.IO;

namespace JointCode.AddIns.Metadata.Assets
{
    class AddinBodyRecord
    {
        internal readonly Guid Guid;

        List<ExtensionPointRecord> _extensionPoints;
		List<ExtensionBuilderRecordGroup> _ebRecordGroups;
		List<ExtensionRecordGroup> _exRecordGroups;
		
		internal AddinBodyRecord(Guid guid) { Guid = guid; }

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

        internal ExtensionPointRecord GetExtensionPoint(string extensionPointId)
        {
            if (_extensionPoints == null)
                return null;
            foreach (var ep in _extensionPoints)
            {
                if (ep.Id == extensionPointId)
                    return ep;
            }
            return null;
        }

        /// <summary>
        /// Gets an extension builder group used to build extensions for the given extension point.
        /// </summary>
        /// <param name="extensionPointId">The extension point id.</param>
		internal ExtensionBuilderRecordGroup GetExtensionBuilderGroup(string extensionPointId)
		{
			if (_ebRecordGroups == null) 
				return null;
			foreach (var ebRecordGroup in _ebRecordGroups) 
			{
				if (ebRecordGroup.ParentPath.StartsWith(extensionPointId))
					return ebRecordGroup;
			}
			return null;
		}
		
        /// <summary>
        /// Gets an extension group that extends the given extension point.
        /// </summary>
        /// <param name="extensionPointId">The extension point id.</param>
		internal ExtensionRecordGroup GetExtensionGroup(string extensionPointId)
		{
			if (_exRecordGroups == null) 
				return null;
			foreach (var exRecordGroup in _exRecordGroups) 
			{
				if (exRecordGroup.ParentPath.StartsWith(extensionPointId))
					return exRecordGroup;
			}
			return null;
		}
		
		internal bool Read(Stream reader)
        {
            _extensionPoints = RecordHelpers.Read(reader, ref ExtensionPointRecord.Factory);
            _ebRecordGroups = RecordHelpers.Read(reader, ref ExtensionBuilderRecordGroup.Factory);
            _exRecordGroups = RecordHelpers.Read(reader, ref ExtensionRecordGroup.Factory);
		    var position = reader.Position;
		    var length = reader.ReadInt64();
		    return position == length;
        }

        internal void Write(Stream writer)
        {
            RecordHelpers.Write(writer, _extensionPoints);
            RecordHelpers.Write(writer, _ebRecordGroups);
            RecordHelpers.Write(writer, _exRecordGroups);
            writer.WriteInt64(writer.Position);
        }
    }
}
