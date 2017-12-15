//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.Collections.Generic;
using JointCode.AddIns.Core;
using JointCode.AddIns.Resolving.Assets;
using JointCode.AddIns.Resolving.Assets.Files;

namespace JointCode.AddIns.Parsing.Xml.Assets
{
    class XmlManifest
    {
        internal ManifestFileXml ManifestFile { get; set; }
        internal List<AssemblyFileXml> AssemblyFiles { get; set; }
        internal List<DataFileXml> DataFiles { get; set; }

        internal AddinHeaderXml AddinHeader { get; set; }
        //internal ActivationXml Activation { get; set; }
        internal ExtensionDeclarationXml ExtensionDeclaration { get; set; }
        internal ExtensionImplementationXml ExtensionImplementation { get; set; }

        internal bool Introspect(IMessageDialog dialog)
        {
            var result = true;
            if (AddinHeader == null)
            {
                dialog.AddError("");
                result = false;
            }
            else
            {
                result &= AddinHeader.Introspect(dialog);
            }

            if (ExtensionDeclaration == null && ExtensionImplementation == null)
            {
                dialog.AddError("");
                result = false;
            }

            if (ExtensionDeclaration != null)
                result &= ExtensionDeclaration.Introspect(dialog);

            if (ExtensionImplementation != null)
                result &= ExtensionImplementation.Introspect(dialog);

            return result;
        }

        internal bool TryParse(IMessageDialog dialog, out AddinResolution result)
        {
            result = new NewAddinResolution();
            AddinHeaderResolution addinHeader;
            if (!AddinHeader.TryParse(dialog, result, out addinHeader))
                return false;
            List<ExtensionPointResolution> extensionPoints = null;
            List<ExtensionBuilderResolutionGroup> extensionBuilderGroups = null;
            if (ExtensionDeclaration != null && !ExtensionDeclaration.TryParse(dialog, result, out extensionPoints, out extensionBuilderGroups))
                return false;
            List<ExtensionResolutionGroup> extensionGroups = null;
            if (ExtensionImplementation != null && !ExtensionImplementation.TryParse(dialog, result, out extensionGroups))
                return false;

            result.AddinHeader = addinHeader;
            result.ManifestFile = ManifestFile;
            result.ExtensionBuilderGroups = extensionBuilderGroups;
            result.ExtensionPoints = extensionPoints;
            result.ExtensionGroups = extensionGroups;
            result.RunningStatus = addinHeader.Enabled ? AddinRunningStatus.Enabled : AddinRunningStatus.Disabled;

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
