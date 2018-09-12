//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Parsing.Xml.Assets;
using JointCode.AddIns.Resolving;
using JointCode.AddIns.Resolving.Assets;
using System.Collections.Generic;

namespace JointCode.AddIns.Parsing.Xml
{
    class XmlAddinManifest : AddinManifest
    {
        internal ManifestFileXml ManifestFile { get; set; }
        internal List<AssemblyFileXml> AssemblyFiles { get; set; }
        internal List<DataFileXml> DataFiles { get; set; }

        internal AddinHeaderXml AddinHeader { get; set; }
        internal AddinActivatorXml AddinActivator { get; set; }
        internal ExtensionSchemaXml ExtensionSchema { get; set; }
        internal ExtensionsXml Extensions { get; set; }

        internal override bool Introspect(INameConvention nameConvention, ResolutionResult resolutionResult)
        {
            var result = true;
            if (AddinHeader == null)
            {
                resolutionResult.AddError(
                    string.Format("The addin located at [{0}] does not define any addin properties (name, guid, etc) in the Addin node of manifest file, which is required!", ManifestFile.Directory));
                result = false;
            }
            else
            {
                result &= AddinHeader.Introspect(ManifestFile.Directory, resolutionResult);
            }
            if (!result)
                return false;

            if (AddinActivator != null)
                result &= AddinActivator.Introspect(resolutionResult);

            if (ExtensionSchema == null && Extensions == null)
            {
                resolutionResult.AddError(string.Format(
                    "A valid addin manifest file must contain either an extension schema node, or an extension node, or both, while the addin [{0}] located at [{1}] does not provide any of them!", AddinHeader.Name, ManifestFile.Directory));
                result = false;
            }

            if (ExtensionSchema != null)
                result &= ExtensionSchema.Introspect(nameConvention, resolutionResult);

            if (Extensions != null)
                result &= Extensions.Introspect(resolutionResult);

            return result;
        }

        internal override bool TryParse(ResolutionResult resolutionResult, out AddinResolution result)
        {
            result = new NewOrUpdatedAddinResolution();

            bool enabled; AddinHeaderResolution addinHeader;
            if (!AddinHeader.TryParse(resolutionResult, result, out enabled, out addinHeader))
                return false;

            List<ExtensionPointResolution> extensionPoints = null; List<ExtensionBuilderResolutionGroup> extensionBuilderGroups = null;
            if (ExtensionSchema != null && !ExtensionSchema.TryParse(resolutionResult, result, out extensionPoints, out extensionBuilderGroups))
                return false;

            List<ExtensionResolutionGroup> extensionGroups = null;
            if (Extensions != null && !Extensions.TryParse(resolutionResult, result, out extensionGroups))
                return false;

            AddinActivatorResolution addinActivator = null;
            if (AddinActivator != null && !AddinActivator.TryParse(resolutionResult, result, out addinActivator))
                return false;

            result.Enabled = enabled;
            result.AddinHeader = addinHeader;
            result.ManifestFile = ManifestFile;
            result.ExtensionBuilderGroups = extensionBuilderGroups;
            result.ExtensionPoints = extensionPoints;
            result.ExtensionGroups = extensionGroups;
            result.AddinActivator = addinActivator;

            if (AssemblyFiles != null)
            {
                result.Assemblies = new List<AssemblyResolution>(AssemblyFiles.Count);
                foreach (var assemblyFile in AssemblyFiles)
                    result.Assemblies.Add(AssemblyResolution.CreateAddinAssembly(result, assemblyFile));
            }

            if (DataFiles != null)
            {
                result.DataFiles = new List<DataFileResolution>(DataFiles.Count);
                foreach (var dataFile in DataFiles)
                    result.DataFiles.Add(dataFile);
            }

            return true;
        }
    }
}
