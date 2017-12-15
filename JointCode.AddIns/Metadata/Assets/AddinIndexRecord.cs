//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.IO;
using System.Collections.Generic;
using JointCode.AddIns.Core;
using JointCode.Common;
using JointCode.Common.IO;

namespace JointCode.AddIns.Metadata.Assets
{
    class AddinIndexRecordSet : List<AddinIndexRecord> { }

    class AddinIndexRecord : ISerializableRecord
    {
        internal static MyFunc<AddinIndexRecord> Factory = () => new AddinIndexRecord();

        readonly AddinFilePack _addinFilePack;
        List<BaseExtensionPointRecord> _extensionPoints;

        //List<ReferencedAddinRecordSet> _referencedAddinSets;
        //List<ReferencedAddinRecord> _referencedAddins;
        //List<ReferencedAssemblyRecordSet> _referencedAssemblySets;
        List<ReferencedAssemblyRecord> _referencedAssemblies;
        List<ExtendedAddinRecord> _extendedAddins;
        List<int> _extendedExtensionPoints;

        internal AddinIndexRecord(AddinFilePack addinFilePack) { _addinFilePack = addinFilePack; OperationStatus = AddinOperationStatus.Unaffected; }
        internal AddinIndexRecord() { _addinFilePack = new AddinFilePack(); OperationStatus = AddinOperationStatus.Unaffected; }

        internal bool AssembliesRegistered { get; set; }

        internal AddinOperationStatus OperationStatus { get; set; }
        internal AddinRunningStatus RunningStatus { get; set; }

        #region Addin
        internal AddinHeaderRecord AddinHeader { get; set; }
        internal int Uid { get { return AddinHeader.AddinId.Uid; } }
        internal Guid Guid { get { return AddinHeader.AddinId.Guid; } }
        internal AddinId AddinId { get { return AddinHeader.AddinId; } }
        /// <summary>
        /// The uid of extension points provided by this addin.
        /// </summary>
        internal List<BaseExtensionPointRecord> ExtensionPoints { get { return _extensionPoints; } } 
        #endregion

        internal string AddinDirectory { get { return _addinFilePack.AddinDirectory; } }

        #region Files
        internal AddinFilePack AddinFilePack { get { return _addinFilePack; } }
        internal ManifestFileRecord ManifestFile
        {
            get { return _addinFilePack.ManifestFile; }
            set { _addinFilePack.ManifestFile = value; }
        }
        internal List<DataFileRecord> DataFiles { get { return _addinFilePack.DataFiles; } }
        internal List<AssemblyFileRecord> AssemblyFiles { get { return _addinFilePack.AssemblyFiles; } }
        #endregion
      
        #region Dependences
        /// <summary>
        /// Assemblies referenced by assemblies of this addin.
        /// </summary>
        internal List<ReferencedAssemblyRecord> ReferencedAssemblies { get { return _referencedAssemblies; } }

        /// <summary>
        /// The addins that contains parent extensions / extension points for which this addin provide extensions to extend.
        /// </summary>
        internal List<ExtendedAddinRecord> ExtendedAddins { get { return _extendedAddins; } }

        /// <summary>
        /// The uid of extension points that this addin extended.
        /// </summary>
        internal List<int> ExtendedExtensionPoints { get { return _extendedExtensionPoints; } }

        ///// <summary>
        ///// Assemblies to be referenced by assemblies of this addin which does not belong to any addins (assemblies found 
        ///// in application directory or other locations). Different locations can provide the same assembly, so it is an 
        ///// assembly set.
        ///// If an assembly reference is provided by the application and another addins at the same time, then we'll choose 
        ///// to refernece to the application assemblies.
        ///// </summary>
        //internal List<ReferencedAssemblyRecordSet> ReferencedAssemblySets { get { return _referencedAssemblySets; } }

        ///// <summary>
        ///// The addins that contains assemblies to be referenced by assemblies of this addin. 
        ///// An assembly reference can be provided by several other addins, but any one of them satisfies the reference 
        ///// requirement (so it is an addin set).
        ///// </summary>
        //internal List<ReferencedAddinRecordSet> ReferencedAddinSets { get { return _referencedAddinSets; } }

        ///// <summary>
        ///// The addins that contains the <see cref="IExtensionBuilder"/> / <see cref="IExtensionPoint"/> types for which this 
        ///// addin provides extension data.
        ///// </summary>
        //internal List<ReferencedAddinRecord> ReferencedAddins { get { return _referencedAddins; } }
        #endregion

        #region Add Methods
        internal void AddExtensionPoint(BaseExtensionPointRecord item)
        {
            _extensionPoints = _extensionPoints ?? new List<BaseExtensionPointRecord>();
            _extensionPoints.Add(item);
        }

        internal void AddReferencedAssembly(ReferencedAssemblyRecord item)
        {
            _referencedAssemblies = _referencedAssemblies ?? new List<ReferencedAssemblyRecord>();
            _referencedAssemblies.Add(item);
        }

        internal void AddExtendedAddin(ExtendedAddinRecord item)
        {
            _extendedAddins = _extendedAddins ?? new List<ExtendedAddinRecord>();
            _extendedAddins.Add(item);
        }

        internal void AddExtendedExtensionPoint(int item)
        {
            _extendedExtensionPoints = _extendedExtensionPoints ?? new List<int>();
            _extendedExtensionPoints.Add(item);
        }

        //internal void AddReferencedAddinSet(ReferencedAddinRecordSet item)
        //{
        //    _referencedAddinSets = _referencedAddinSets ?? new List<ReferencedAddinRecordSet>();
        //    _referencedAddinSets.Add(item);
        //}

        //internal void AddReferencedAssemblySet(ReferencedAssemblyRecordSet item)
        //{
        //    _referencedAssemblySets = _referencedAssemblySets ?? new List<ReferencedAssemblyRecordSet>();
        //    _referencedAssemblySets.Add(item);
        //}

        //internal void AddReferencedAddin(ReferencedAddinRecord item)
        //{
        //    _referencedAddins = _referencedAddins ?? new List<ReferencedAddinRecord>();
        //    _referencedAddins.Add(item);
        //}
        #endregion

        internal bool ExtendsExtensionPoint(int extensionPointUid)
        {
            return _extendedExtensionPoints == null
                ? false
                : _extendedExtensionPoints.Contains(extensionPointUid);
        }

        internal bool RefersToAssembly(int assemblyUid)
        {
            if (_referencedAssemblies == null)
                return false;
            foreach (var referencedAssembly in _referencedAssemblies)
            {
                if (referencedAssembly.Uid == assemblyUid)
                    return true;
            }
            return false;
        }

        public void Read(Stream reader)
        {
            RunningStatus = (AddinRunningStatus)reader.ReadSByte();
            _addinFilePack.Read(reader);

            AddinHeader = new AddinHeaderRecord();
            AddinHeader.Read(reader);

            _extensionPoints = RecordHelpers.Read(reader, ref BaseExtensionPointRecord.Factory);
            _referencedAssemblies = RecordHelpers.Read(reader, ref ReferencedAssemblyRecord.Factory);
            _extendedAddins = RecordHelpers.Read(reader, ref ExtendedAddinRecord.Factory);
            _extendedExtensionPoints = RecordHelpers.Read(reader);
        }

        public void Write(Stream writer)
        {
            writer.WriteSByte((sbyte)RunningStatus);
            _addinFilePack.Write(writer);

            AddinHeader.Write(writer);
            
            RecordHelpers.Write(writer, _extensionPoints);
            RecordHelpers.Write(writer, _referencedAssemblies);
            RecordHelpers.Write(writer, _extendedAddins);
            RecordHelpers.Write(writer, _extendedExtensionPoints);
        }
    }
}
