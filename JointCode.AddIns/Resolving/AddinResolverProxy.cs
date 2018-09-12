//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Core;
using JointCode.AddIns.Core.Data;
using JointCode.AddIns.Core.FileScanning;
using JointCode.AddIns.Core.Storage;
using JointCode.AddIns.Metadata;
using JointCode.AddIns.Resolving.Assets;
using JointCode.Common.Conversion;

namespace JointCode.AddIns.Resolving
{
    class AddinResolverProxy //: MarshalByRefObject
    {
        // @return value: whether the persistence file has been updated.
        public ResolutionResult Resolve(INameConvention nameConvention, AddinFileSettings fileSettings, AssemblyLoadPolicy assemblyLoadPolicy,
            AddinStorage addinStorage, AddinRelationManager relationManager,
            ScanFilePackResult scanFilePackResult)
        {
            var ctx = new ResolutionContext();
            var cm = new ConvertionManager();
            InitializeDataTransformers(ctx, cm);

            if (assemblyLoadPolicy.PrivateAssemblyProbingDirectories != null)
            {
                foreach (var privateAssemblyProbingDirectory in assemblyLoadPolicy.PrivateAssemblyProbingDirectories)
                    AssemblyResolution.AddSearchDirectory(privateAssemblyProbingDirectory);
            }

            var resolver = new DefaultAddinResolver(addinStorage, relationManager, cm);
            // 强制 ExtensionBuilder 节点应用 NameConvention
            return resolver.Resolve(nameConvention, ctx, scanFilePackResult);
        }

        static void InitializeDataTransformers(ResolutionContext ctx, ConvertionManager cm)
        {
            var transformers = new DataTransformer[]
            {
                new StringDataTransformer(), 
                new TypeHandleDataTransformer(), 
                new VersionDataTransformer(),
                new GuidDataTransformer(),
                new DateTimeDataTransformer(),
                new TimeSpanDataTransformer(),
                
                new BooleanDataTransformer(),
                new CharDataTransformer(),
                new SByteDataTransformer(),
                new ByteDataTransformer(),
                new Int16DataTransformer(),
                new UInt16DataTransformer(),
                new Int32DataTransformer(),
                new UInt32DataTransformer(),
                new Int64DataTransformer(),
                new UInt64DataTransformer(),
                new SingleDataTransformer(),
                new DoubleDataTransformer(),
                new DecimalDataTransformer(),
            };

            foreach (var transformer in transformers)
                transformer.Intialize(ctx, cm);
        }
    }
}