//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using Mono.Cecil;

//namespace JointCode.AddIns.Core
//{
//    interface IReflection
//    {
//        Stream GetResourceStream(string assemblyFile, string resourceName);
//    }

//    class CecilReflection : IReflection
//    {
//        // using mono.cecil, you can get resource stream, modify the content of resource, replace, add or remove the resource.
//        public Stream GetResourceStream(string assemblyFile, string resourceName)
//        {
//            var asmDef = AssemblyDefinition.ReadAssembly(assemblyFile);
//            foreach (var module in asmDef.Modules)
//            {
//                if (!module.HasResources)
//                    continue;
//                foreach (var resource in module.Resources)
//                {
//                    if (resource.Name != resourceName)
//                        continue;
//                    var embeddedResource = resource as EmbeddedResource;
//                    return embeddedResource == null ? null : embeddedResource.GetResourceStream();
//                }
//            }
//            return null;
//        }
//    }
//}
