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
        //List<ReferencedAddinRecordSet> _referencedAddinSets;
        //List<ReferencedAddinRecord> _referencedAddins;
        //List<ReferencedAssemblyRecordSet> _referencedAssemblySets;
        //List<ReferencedApplicationAssemblyRecord> _referencedApplicationAssemblies;
        List<ReferencedAssemblyRecord> _referencedAssemblies;
        List<ExtendedAddinRecord> _extendedAddins;
        // todo: extension points that this addin provides [extension builder or extensions] to extend it, including those extension points that defined in this addin.
        List<int> _extendedExtensionPoints;

        #region Dependences
        // !!ReferencedAssemblies 和 ExtendedAddins 是一个启动插件时关心的内容，它们覆盖了一个插件的全部 (100%) 直接（不含间接）外部依赖项。在运行时，插件在启动之前，会
        //   先启动其所有依赖的插件。其中，插件的扩展插件集合 (ExtendedAddins) 中包含的每一个 (ExtendedAddin) 是唯一确定的（例如一个 Extension 的上级 Extension 是哪一个
        //   插件声明的，这个是唯一确定的）；但插件程序集引用了哪些插件 (ReferencedAssemblies) 却非如此。比如说一个插件的程序集引用了 log4net.dll 这个程序集，但有可能多
        //   个插件都提供了这个程序集，这种情况下，多个插件都可以满足该引用需求。
        // !!ReferencedAssemblies 和 ExtendedExtensionPoints 是加载 ExtensionPoint 时关心的内容。
        // ReferencedAssemblies：表示因为程序集引用而导致的依赖项。这些依赖项必须在插件启动之前注册到运行时 AssemblyResolver。
        // ExtendedAddins：表示由于扩展点/扩展系统而导致的依赖项。
        // ExtendedExtensionPoints：表示插件扩展的 ExtensionPoint。在加载 ExtensionPoint 时，插件引擎按照插件间的依赖顺序依次检查各个插件是否扩展了 ExtensionPoint。如果
        //                          某个插件确实扩展了 ExtensionPoint，插件引擎先获取该插件提供的 ExtensionBuilder/ExtensionPoint 等类型，将它们注册到 RuntimeExtensionLoader，
        //                          然后加载 Extension）。

        // 具有说来，考虑到运行时插件的加载方式和顺序，有以下几种依赖关系需要解析：
        // 1. 一个 Extension 对上级 Extension 的依赖关系（将上级的 Addin 和 ExtensionPoint 添加到 ExtendedAddins 和 ExtendedExtensionPoints）
        // 2. 一个 Extension 对 IExtensionBuilder<TExtension> / ICompositeExtensionBuilder<TExtension> 等具体实现类和 TExtension 类型的依赖关系（添加到 ReferencedAssemblies，因为对于这些类型的依赖并不是直接通过程序集引用产生的依赖 [这种引用可以通过 AssemblyResolver 自动解析]，而是通过 Extension 引用 ExtensionBuilder 这种隐式方式，所以这里必须显式添加 [这种引用无法通过 AssemblyResolver 自动解析]）
        // 3. 一个 Extension 对同级扩展 (Sibling Extension) 的依赖关系（将同级的 Addin 添加到 ExtendedAddins）
        // 4. 一个 Extension 对 ExtensionPoint 的依赖关系（添加到 ExtendedAddins 和 ExtendedExtensionPoints）
        // 5. 一个 ExtensionBuilder<TExtension> 对上级 ExtensionBuilder 的依赖关系（添加到 ExtendedAddins）
        // 6. 一个 ExtensionBuilder<TExtension> 对 ExtensionPoint 的依赖关系（添加到 ExtendedAddins 和 ExtendedExtensionPoints）
        // 7. 一个 ExtensionBuilder<TExtension> 具体实现类对自身实现类型和 TExtension 等类型的依赖关系（添加到 ReferencedAssemblies）
        // 8. 一个 ExtensionPoint<TExtension, TExtensionRoot> 对自身实现类型、TExtension 和 TExtensionRoot 等类型的依赖关系（添加到 ReferencedAssemblies）

        /// <summary>
        /// The assemblies that is provided by other addins (not by the runtime or application itself) and referenced by this addin.
        /// </summary>
        internal List<ReferencedAssemblyRecord> ReferencedAssemblies { get { return _referencedAssemblies; } }

        /// <summary>
        /// The addins that contains parent extensions / extension points for which this addin provide extension builders / extensions to extend.
        /// </summary>
        internal List<ExtendedAddinRecord> ExtendedAddins { get { return _extendedAddins; } }

        /// <summary>
        /// The uid of extension points that this addin extended (provide extension builders / extensions for it).
        /// </summary>
        internal List<int> ExtendedExtensionPoints { get { return _extendedExtensionPoints; } }

        ///// <summary>
        ///// Assemblies referenced by assemblies of this addin that is provided by application itself.
        ///// </summary>
        //internal List<ReferencedApplicationAssemblyRecord> ReferencedApplicationAssemblies { get { return _referencedApplicationAssemblies; } }

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

        //internal void AddReferencedApplicationAssembly(ReferencedApplicationAssemblyRecord item)
        //{
        //    _referencedApplicationAssemblies = _referencedApplicationAssemblies ?? new List<ReferencedApplicationAssemblyRecord>();
        //    _referencedApplicationAssemblies.Add(item);
        //}

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

        // todo: add extension points extended by the extension builders of this addin to the list
        // adds an extended extension point, including an extension point that this addin provides extension builder or extensions to extend it.
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

        internal bool ExtendsAddin(int addinUid)
        {
            if (_extendedAddins == null)
                return false;
            foreach (var extendedAddin in _extendedAddins)
            {
                if (extendedAddin.Uid == addinUid)
                    return true;
            }
            return false;
        }

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
    }
}
