using JointCode.AddIns.Metadata.Assets;
using JointCode.AddIns.Resolving.Assets;
using Mono.Cecil;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using JointCode.AddIns.Metadata;

namespace JointCode.AddIns.Core.Dependencies
{
    static class AssemblyDependencyHelper
    {
        internal static AssemblyDependency Create(AssemblyNameReference assemblyName)
        {
            return new AssemblyDependency(assemblyName.Name, assemblyName.Version, new CultureInfo(assemblyName.Culture), assemblyName.PublicKeyToken);
        }
    }

    class DependedApplicationAssemblyResolver
    {
        static AssemblyDefinition _thisAssembly;
        readonly ResolutionTimeAssemblyResolver _assemblyResolver;
        readonly ReaderParameters _cecilReaderParameters;

        internal DependedApplicationAssemblyResolver()
        {
            _assemblyResolver = new ResolutionTimeAssemblyResolver();
            _cecilReaderParameters = new ReaderParameters { AssemblyResolver = _assemblyResolver };

            // The JointCode.AddIns.dll assembly itself
            if (_thisAssembly == null)
            {
                var runtimeAssemblyFullName = typeof(AssemblyResolution).Assembly.FullName;
                var cecilAssemblyName = AssemblyNameReference.Parse(runtimeAssemblyFullName);
                _thisAssembly = _assemblyResolver.Resolve(cecilAssemblyName);
            }
        }

        // Gets the required assembly references for this assembly, which is is provided by the application itself, 
        // and it is not the the JointCode.AddIns.dll. 
        internal AssemblyDependency[] GetRequiredApplicationAssemblyDependencies(AddinRecord addinRecord)
        {
            if (addinRecord.AssemblyFiles == null || addinRecord.AssemblyFiles.Count == 0)
                return null;

            var result = new List<AssemblyDependency>();
            foreach (var assemblyFile in addinRecord.AssemblyFiles)
                DoGetRequiredApplicationAssemblyDependencies(addinRecord, assemblyFile, ref result);

            return result.Count == 0 ? null : result.ToArray();
        }

        void DoGetRequiredApplicationAssemblyDependencies(AddinRecord addinRecord, AssemblyFileRecord assemblyFileRecord,
            ref List<AssemblyDependency> result)
        {
            AssemblyDefinition assemblyDef;
            if (!TryLoad(addinRecord, assemblyFileRecord, out assemblyDef))
                return;

            foreach (var module in assemblyDef.Modules)
            {
                if (!module.HasAssemblyReferences)
                    continue;

                foreach (var assemblyReference in module.AssemblyReferences)
                {
                    // if the referenced assembly is provided by runtime (.net, mono, etc) or application itself which can be loaded by probing 
                    // mechanism, then the return value is not null; otherwise, returns null.
                    var applicationOrRuntimeAssembly = _assemblyResolver.ResolveAssembly(assemblyReference, _cecilReaderParameters);
                    if (applicationOrRuntimeAssembly == null 
                        || applicationOrRuntimeAssembly.IsRuntimeProvided 
                        || ReferenceEquals(applicationOrRuntimeAssembly.AssemblyDefinition, _thisAssembly))
                        continue;

                    // 这是应用程序本身提供的程序集，而且不是 JointCode.AddIns.dll 程序集自身
                    var assemblyDependency = AssemblyDependencyHelper.Create(assemblyReference);
                    if (!result.Contains(assemblyDependency))
                        result.Add(assemblyDependency);
                }
            }
        }

        bool TryLoad(AddinRecord addinRecord, AssemblyFileRecord assemblyFileRecord, out AssemblyDefinition result)
        {
            result = null;
            var assemblyPath = GetAbsolutePath(addinRecord, assemblyFileRecord);
            try
            {
                result = AssemblyDefinition.ReadAssembly(assemblyPath, _cecilReaderParameters);
                return true;
            }
            catch// (Exception e)
            {
                // log
                return false;
            }
        }

        string GetAbsolutePath(AddinRecord addinRecord, AssemblyFileRecord assemblyFileRecord)
        {
            return Path.Combine(addinRecord.ManifestFile.Directory, assemblyFileRecord.FilePath);
        }
    }
}