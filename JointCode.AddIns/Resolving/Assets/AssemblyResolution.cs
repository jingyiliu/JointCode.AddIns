//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Core.Runtime;
using JointCode.Common.Conversion;
using JointCode.Common.Helpers;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using JointCode.Common.Extensions;

namespace JointCode.AddIns.Resolving.Assets
{
    enum AssemblyKind
    {
        Default,
        //ProbableAssembly, // assemblies provided by runtime (.net/mono) or the application itself.
        ApplicationAssembly, // assemblies provided by the application itself.
        RuntimeAssembly, // assemblies provided by runtime (.net/mono).
        AddinAssembly // assemblies provided by addins.
    }

    class CecilAssemblyKey : AssemblyKey
    {
        private CecilAssemblyKey(string name, Version version, CultureInfo cultrue, byte[] publicKeyToken)
            : base(name, version, cultrue, publicKeyToken) { }
        internal static CecilAssemblyKey Create(AssemblyNameDefinition assemblyName)
        {
            return new CecilAssemblyKey(assemblyName.Name, assemblyName.Version, new CultureInfo(assemblyName.Culture), assemblyName.PublicKeyToken);
        }
        internal static CecilAssemblyKey Create(AssemblyNameReference assemblyName)
        {
            return new CecilAssemblyKey(assemblyName.Name, assemblyName.Version, new CultureInfo(assemblyName.Culture), assemblyName.PublicKeyToken);
        }
    }

    partial class AssemblyResolution
    {
        internal static AssemblyResolution CreateAddinAssembly(AddinResolution addin, AssemblyFileResolution assemblyFile)
        {
            return new AssemblyResolution(addin, assemblyFile);
        }

        // @assemblyName: AssemblyName 的字符串形式，不是程序集文件路径
        internal static AssemblyResolution GetProbableAssembly(string assemblyName)
        {
            var assembly = _assemblyResolver.ResolveAssembly(assemblyName, _cecilReaderParameters);
            return assembly == null 
                ? null 
                : new AssemblyResolution(assembly.AssemblyDefinition, 
                    assembly.IsRuntimeProvided ? AssemblyKind.RuntimeAssembly : AssemblyKind.ApplicationAssembly);
        }

        internal static AssemblyDefinition GetAssemblyDefinitionFor(Type type)
        {
            // The JointCode.AddIns.dll assembly itself
            var runtimeAssemblyFullName = type.Assembly.FullName;
            var cecilAssemblyName = AssemblyNameReference.Parse(runtimeAssemblyFullName);
            var cad = _assemblyResolver.ResolveAssembly(cecilAssemblyName, _cecilReaderParameters);
            if (!cad.IsRuntimeProvided)
                throw new NotSupportedException(string.Format("The type [{0}] is not supported for extension data!" , type.ToFullTypeName()));
            return cad.AssemblyDefinition;
        }

        #region 改变程序集解析器 (AssemblyResolver) 的行为

        // 添加解析程序集时，程序集的探测路径 (probing directory)
        internal static void AddSearchDirectory(string directory)
        {
            _assemblyResolver.AddSearchDirectory(directory);
        }

        // 删除解析程序集时，程序集的探测路径 (probing directory)
        internal static void RemoveSearchDirectory(string directory)
        {
            _assemblyResolver.RemoveSearchDirectory(directory);
        }

        // 获取解析程序集时，程序集的探测路径 (probing directory)
        internal static List<string> GetSearchDirectories()
        {
            return _assemblyResolver.GetSearchDirectories();
        }

        // 添加平台版本到平台文件夹的映射
        internal static void AddRuntimeVersion(Platform platform, AssemblyVersion version, string dir)
        {
            _assemblyResolver.AddRuntimeVersion(platform, version, dir);
        }

        // 删除平台版本到平台文件夹的映射
        internal static void RemoveRuntimeVersion(Platform platform, AssemblyVersion version)
        {
            _assemblyResolver.RemoveRuntimeVersion(platform, version);
        } 

        #endregion
    }

    /// <summary>
    /// Represent an assembly during resolution.
    /// Notes that to determine whether two <see cref="AssemblyResolution"/>s are the same, you can not use <see cref="object.ReferenceEquals"/>,
    /// try to use the <see cref="AssemblyResolution.Equals(AssemblyResolution)"/> instead.
    /// </summary>
    partial class AssemblyResolution : Resolvable, IEquatable<AssemblyResolution>
    {
        // The JointCode.AddIns.dll assembly itself
        static AssemblyDefinition _thisAssembly;
        static ReaderParameters _cecilReaderParameters;
        static ResolutionTimeAssemblyResolver _assemblyResolver;

        AssemblyDefinition _assemblyDef;
        CecilAssemblyKey _assemblykey;

        readonly AssemblyFileResolution _assemblyFile;
        AssemblyKind _assemblyKind;
        bool _assemblyReferencesGot;
        Assembly _assembly;

        static AssemblyResolution()
        {
            Initialize(); 
        }

        static void Initialize()
        {
            if (_assemblyResolver != null)
                return;
            _assemblyResolver = new ResolutionTimeAssemblyResolver();
            _cecilReaderParameters = new ReaderParameters { AssemblyResolver = _assemblyResolver };
            // The JointCode.AddIns.dll assembly itself
            var runtimeAssemblyFullName = typeof(AssemblyResolution).Assembly.FullName;
            var cecilAssemblyName = AssemblyNameReference.Parse(runtimeAssemblyFullName);
            _thisAssembly = _assemblyResolver.Resolve(cecilAssemblyName);
        }

        internal static void DisposeInternal()
        {
            _cecilReaderParameters = null;
            _assemblyResolver = null;
            _thisAssembly = null;
        }

        // 插件程序集
        private AssemblyResolution(AddinResolution declaringAddin, AssemblyFileResolution assemblyFile)
            : base(declaringAddin)
        {
            Initialize();
            _assemblyKind = AssemblyKind.AddinAssembly;
            _assemblyFile = assemblyFile;
        }

        // 可探测 (probable) 程序集（包括运行时和应用程序提供的程序集）
        private AssemblyResolution(AssemblyDefinition assemblyDef, AssemblyKind assemblyKind)
            : base(null)
        {
            Initialize();
            _assemblyKind = assemblyKind;
            _assemblyDef = assemblyDef;
            _assemblykey = CecilAssemblyKey.Create(_assemblyDef.Name);
        }

        internal static ResolutionTimeAssemblyResolver AssemblyResolver { get { return _assemblyResolver; } }
        internal static AssemblyDefinition ThisAssembly { get { return _thisAssembly; } }

        internal AssemblyKind AssemblyKind { get { return _assemblyKind; } }
        internal AssemblyFileResolution AssemblyFile { get { return _assemblyFile; } }
        internal AssemblyKey AssemblyKey { get { return _assemblykey; } }

        internal int Uid
        {
            get { return _assemblyFile.Uid; } 
            set { _assemblyFile.Uid = value; }
        }

        /// <summary>
        /// Tries to read the assembly with Mono.Cecil.
        /// </summary>
        /// <returns>retuns true if it can be read with Mono.Cecil; otherwise, return false.</returns>
        internal bool TryLoad()
        {
            // 如果是可探测 (probable) 程序集（包括运行时和应用程序提供的程序集），直接返回 true
            if (_assemblyDef != null)
                return true;

            AssemblyNameDefinition assemblyName;
            var assemblyPath = GetAbsolutePathForNonProbableAssemblies();
            try
            {
                _assemblyDef = AssemblyDefinition.ReadAssembly(assemblyPath, _cecilReaderParameters);
                assemblyName = _assemblyDef.Name;
            }
            catch (Exception e)
            {
                // log
                return false;
            }

            _assemblykey = CecilAssemblyKey.Create(assemblyName);
#if NET_4_0
            if (_assemblyKind == AssemblyKind.AddinAssembly) 
                _assemblyFile.IsPublic = IsPublicAssembly();
#endif
            return true;
        }

        string GetAbsolutePathForNonProbableAssemblies()
        {
            if (_assemblyKind == AssemblyKind.AddinAssembly)
                return Path.Combine(DeclaringAddin.ManifestFile.Directory, _assemblyFile.FilePath);
            if (_assemblyKind == AssemblyKind.ApplicationAssembly)
                return Path.Combine(SystemHelper.AppDirectory, _assemblyFile.FilePath);
            throw new InvalidOperationException();
        }

#if NET_4_0
        bool IsPublicAssembly()
        {
            if (!_assemblyDef.HasCustomAttributes) 
                return true;
    		
            foreach (var attrib in _assemblyDef.CustomAttributes) 
            {
                if (!attrib.HasProperties)
                    continue;

                TypeDefinition attribType;
                try
                {
                    attribType = attrib.AttributeType.Resolve();
                }
                catch
                {
                    return false;
                }
                if (!ReferenceEquals(attribType, _addinAssemblyAttributeType))
                    continue;
    			
                foreach (var prop in attrib.Properties)
                {
                    if (prop.Name == AddinAssemblyAttribute.PropertyIsPublic) 
                        return (bool)prop.Argument.Value;
                }
            }
    		
            return false;
        }
#endif

        internal bool TryGetType(string typeName, out TypeResolution result)
        {
            foreach (var module in _assemblyDef.Modules)
            {
                if (!module.HasTypes)
                    continue;
                foreach (var type in module.Types)
                {
                    if (type.FullName == typeName)
                    {
                        result = new TypeResolution(this, type);
                        return true;
                    }
                }
            }
            result = null;
            return false;
        }

        // Gets the required assembly references for this assembly, which is is not provided by the runtime 
        // (.net, mono, ..., GAC assembly), nor the the JointCode.AddIns.dll itself. 
        // Notes that an assembly can refer to another assembly that is provided by the same addin.
        internal List<AssemblyKey> GetRequiredAssemblyReferences()
        {
            List<AssemblyKey> result = null;
            foreach (var module in _assemblyDef.Modules)
            {
                if (!module.HasAssemblyReferences)
                    continue;
                foreach (var assemblyReference in module.AssemblyReferences)
                {
                    // if the referenced assembly is provided by runtime (.net, mono, etc) which can be loaded by probing mechanism, 
                    // then the return value is not null; otherwise, returns null.
                    var applicationOrRuntimeAssembly = _assemblyResolver.ResolveAssembly(assemblyReference, _cecilReaderParameters);
                    if (applicationOrRuntimeAssembly != null)
                    {
                        //if (applicationOrRuntimeAssembly.IsRuntimeProvided 
                        //    || ReferenceEquals(applicationOrRuntimeAssembly.AssemblyDefinition, ThisAssembly)) 
                        //    continue; // 这是运行时（而非应用程序本身）提供的程序集，或是 JointCode.AddIns.dll 程序集自身
                        continue;
                    }
                    var assemblyKey = CecilAssemblyKey.Create(assemblyReference);
                    result = result ?? new List<AssemblyKey>();
                    result.Add(assemblyKey);
                }
            }
            return result;
        }

        #region IEquatable implementation

        public bool Equals(AssemblyResolution other)
        {
            return other != null && ReferenceEquals(_assemblyDef, other._assemblyDef);
        }

        #endregion

        protected override ResolutionStatus DoResolve(ResolutionResult resolutionResult, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            if (_assemblyReferencesGot)
                return DeclaringAddin == null ? ResolutionStatus.Success : DeclaringAddin.ResolutionStatus;

            List<AssemblyResolutionSet> refAsmSets;
            if (!ctx.TryGetRequiredAssemblyReferences(resolutionResult, this, out refAsmSets))
                return ResolutionStatus.Failed;
            _assemblyReferencesGot = true;
            if (refAsmSets != null)
            {
                foreach (var refAsmSet in refAsmSets)
                    DeclaringAddin.AddReferencedAssemblySet(refAsmSet);
            }
            return DeclaringAddin == null ? ResolutionStatus.Success : DeclaringAddin.ResolutionStatus;
        }

        //internal Assembly GetAssembly()
        //{
        //    if (_assembly != null)
        //        return _assembly;
        //    if (_assemblyKind == AssemblyKind.RuntimeAssembly)
        //        throw new InvalidOperationException("Should not load a runtime assembly in this way!");
        //    _assembly = Assembly.LoadFile(GetAbsolutePathForNonProbableAssemblies());
        //    return _assembly;
        //}
    }

    // 在一个应用程序中，相同程序集（版本也相同）只允许加载一次。
    // 因此，如果有插件 a 依赖于某程序集（例如 ICSharpCode.SharpZipLib.dll），即使有多个插件都提供了该程序集，
    // 也只需加载其中任意一个插件即可满足插件 a 的需求。
    class AssemblyResolutionSet : List<AssemblyResolution>
    {
        //readonly List<AssemblyResolution> _assemblies = new List<AssemblyResolution>();

        internal int Uid { get { return this[0].Uid; } }

        // get the lowest version of an assembly set
        internal Version Version
        {
            get
            { 
                Version ver = null;
                foreach (var item in this)
                {
                    if (ver == null || ver > item.AssemblyKey.Version)
                        ver = item.AssemblyKey.Version;
                }
                return ver;
            }
        }

        internal ResolutionStatus Resolve(ResolutionResult resolutionResult, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            var result = ResolutionStatus.Success;
            for (int i = 0; i < Count; i++)
            {
                var status = this[i].Resolve(resolutionResult, convertionManager, ctx);
                if (status == ResolutionStatus.Success)
                    return ResolutionStatus.Success;
                result |= status;
            }
            return result;
        }
    }
}