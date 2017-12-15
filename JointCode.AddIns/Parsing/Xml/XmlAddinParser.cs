//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using JointCode.AddIns.Core;
using JointCode.AddIns.Core.FileScanning;
using JointCode.AddIns.Core.Helpers;
using JointCode.AddIns.Resolving.Assets.Files;
using JointCode.AddIns.Parsing.Xml.Assets;
using JointCode.AddIns.Resolving.Assets;

namespace JointCode.AddIns.Parsing.Xml
{
    class XmlAddinParser : AddinParser
    {
        const string Addin = "Addin";
        const string Header = "Header";
        const string Extensions = "Extensions";
        const string Declaration = "Declaration";
        const string Implementation = "Implementation";

        const string Guid = "Guid";
        const string Category = "Category";
        const string FriendName = "FriendName";
        const string Description = "Description";
        const string Version = "Version";
        const string CompatVersion = "CompatVersion";
        const string Url = "Url";
        const string Enabled = "Enabled";

        const string AttributeType = "type";
        const string AttributeDescription = "description";
        const string AttributePath = "path";
        const string AttributeId = "id";
        const string AttributeInsertBefore = "insertBefore";
        const string AttributeInsertAfter = "insertAfter";
        
        XmlNode _headerNode, _declarationNode, _implementationNode;

        bool IsManifest(IMessageDialog dialog, string manifestFile)
        {
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(manifestFile);
            }
            catch(Exception ex)
            {
                // log
                dialog.AddError(ex.Message);
                return false;
            }

            XmlNode rootNode = xmlDoc.DocumentElement;
            if (rootNode == null || !Addin.Equals(XmlHelper.GetNodeName(rootNode)) || !rootNode.HasChildNodes)
                return false;

            _headerNode = rootNode[Header];
            if (_headerNode == null)
                return false;

            var extensionsNode = rootNode[Extensions];
            if (extensionsNode == null)
                return false;
            _declarationNode = extensionsNode[Declaration];
            _implementationNode = extensionsNode[Implementation];

            if ((_declarationNode == null || _declarationNode.NodeType != XmlNodeType.Element || !_declarationNode.HasChildNodes)
                && (_implementationNode == null || _implementationNode.NodeType != XmlNodeType.Element || !_implementationNode.HasChildNodes))
                return false;
            else
                return true;
        }

        AddinHeaderXml ReadHeader()
        {
            if (_headerNode == null || _headerNode.NodeType != XmlNodeType.Element || !_headerNode.HasChildNodes)
                return null;

            var header = new AddinHeaderXml();

            foreach (XmlNode node in _headerNode.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue;

                string val;

                if (header.Guid == null)
                {
                    val = XmlHelper.GetMatchingNodeValue(node, Guid);
                    if (val != null)
                    {
                        header.Guid = val;
                        continue;
                    }
                }
                if (header.AddinCategory == null)
                {
                    val = XmlHelper.GetMatchingNodeValue(node, Category);
                    if (val != null)
                    {
                        header.AddinCategory = val;
                        continue;
                    }
                }
                if (header.FriendName == null)
                {
                    val = XmlHelper.GetMatchingNodeValue(node, FriendName);
                    if (val != null)
                    {
                        header.FriendName = val;
                        continue;
                    }
                }
                if (header.Description == null)
                {
                    val = XmlHelper.GetMatchingNodeValue(node, Description);
                    if (val != null)
                    {
                        header.Description = val;
                        continue;
                    }
                }
                if (header.Version == null)
                {
                    val = XmlHelper.GetMatchingNodeValue(node, Version);
                    if (val != null)
                    {
                        header.Version = val;
                        continue;
                    }
                }
                if (header.CompatVersion == null)
                {
                    val = XmlHelper.GetMatchingNodeValue(node, CompatVersion);
                    if (val != null)
                    {
                        header.CompatVersion = val;
                        continue;
                    }
                }
                if (header.Url == null)
                {
                    val = XmlHelper.GetMatchingNodeValue(node, Url);
                    if (val != null)
                    {
                        header.Url = val;
                        continue;
                    }
                }
                if (header.Enabled == null)
                {
                    val = XmlHelper.GetMatchingNodeValue(node, Enabled);
                    if (val != null)
                    {
                        header.Enabled = val;
                        continue;
                    }
                }

                header.AddProperty(XmlHelper.GetNodeName(node), XmlHelper.GetNodeValue(node));
            }

            return header;
        }

        ExtensionDeclarationXml ReadDeclaration()
        {
            if (_declarationNode == null || _declarationNode.NodeType != XmlNodeType.Element || !_declarationNode.HasChildNodes)
                return null;

            var declaration = new ExtensionDeclarationXml();
            for (int i = 0; i < _declarationNode.ChildNodes.Count; i++)
            {
                var node = _declarationNode.ChildNodes[i];
                if (node.NodeType != XmlNodeType.Element || node.Attributes == null || node.Attributes.Count == 0)
                    continue; // not a valid definition

                var epTypeName = XmlHelper.GetMatchingAttribueValue(node, AttributeType); //Get the extension point type
                if (!string.IsNullOrEmpty(epTypeName)) //'Type' attribute defined: this is a valid extension point
                {
                    // This is an extension point that defined in this addin
                    var epId = XmlHelper.GetNodeName(node);
                    var ep = new ExtensionPointXml
                    {
                        Id = epId,
                        TypeName = epTypeName,
                        Description = XmlHelper.GetMatchingAttribueValue(node, AttributeDescription)
                    };

                    declaration.AddExtensionPoint(ep);

                    if (node.HasChildNodes)
                    {
                        for (int j = 0; j < node.ChildNodes.Count; j++)
                        {
                            var eb = ReadExtensionBuilder(node.ChildNodes[j], epId, epId);
                            if (eb != null)
                                ep.AddChild(eb);
                        }
                    }
                }
                else
                {
                    // This might be an extension point that defined in another addin
                    var ebGroupPath = XmlHelper.GetMatchingAttribueValue(node, AttributePath);
                    if (ebGroupPath == null) // not a valid definition
                        continue;
                    var ebGroup = ReadExtensionBuilderGroup(node, ebGroupPath);
                    if (ebGroup != null)
                        declaration.AddExtensionBuilderGroup(ebGroup);
                }
            }

            return declaration;
        }

        ExtensionBuilderXmlGroup ReadExtensionBuilderGroup(XmlNode node, string parentPath)
        {
            if (node.NodeType != XmlNodeType.Element || !node.HasChildNodes)
                return null;

            var result = new ExtensionBuilderXmlGroup {ParentPath = parentPath};
            var extensionPointId = StringHelper.GetExtensionPointId(parentPath);

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                var childNode = node.ChildNodes[i];
                var eb = ReadExtensionBuilder(childNode, parentPath, extensionPointId);
                if (eb != null)
                    result.AddChild(eb);
            }

            return result.Children.Count > 0 ? result : null;
        }

        ExtensionBuilderXml ReadExtensionBuilder(XmlNode node, string parentPath, string extensionPointId)
        {
            var result = DoReadExtensionBuilder(node, parentPath, extensionPointId);
            if (result == null)
                return null;
            if (node.HasChildNodes)
            {
                for (int i = 0; i < node.ChildNodes.Count; i++)
                    ReadExtensionBuilderRecursively(node.ChildNodes[i], result, extensionPointId);
            }
            return result;
        }

        void ReadExtensionBuilderRecursively(XmlNode node, ExtensionBuilderXml parent, string extensionPointId)
        {
            var eb = DoReadExtensionBuilder(node, parent.Path, extensionPointId);
            if (eb == null)
                return;
            parent.AddChild(eb);
            // recursively add children
            if (node.HasChildNodes)
            {
                for (int i = 0; i < node.ChildNodes.Count; i++)
                    ReadExtensionBuilderRecursively(node.ChildNodes[i], eb, extensionPointId);
            }
        }

        static ExtensionBuilderXml DoReadExtensionBuilder(XmlNode node, string parentPath, string extensionPointId)
        {
            if (node.NodeType != XmlNodeType.Element)
                return null;

            if (node.Attributes == null || node.Attributes.Count == 0) // a referenced extension builder
            {
                return new ReferencedExtensionBuilderXml
                {
                    Id = XmlHelper.GetNodeName(node),
                    ParentPath = parentPath,
                    ExtensionPointId = extensionPointId
                };
            }
            else // a normal extension builder
            {
                var typeName = XmlHelper.GetMatchingAttribueValue(node, AttributeType); // get the extension builder type
                if (string.IsNullOrEmpty(typeName))
                    return null;

                return new DeclaredExtensionBuilderXml
                {
                    TypeName = typeName,
                    Id = XmlHelper.GetNodeName(node),
                    Description = XmlHelper.GetMatchingAttribueValue(node, AttributeDescription),
                    ParentPath = parentPath,
                    ExtensionPointId = extensionPointId
                };
            }
        }

        ExtensionImplementationXml ReadImplementation()
        {
            if (_implementationNode == null || _implementationNode.NodeType != XmlNodeType.Element || !_implementationNode.HasChildNodes)
                return null;

            var result = new ExtensionImplementationXml();
            for (int i = 0; i < _implementationNode.ChildNodes.Count; i++)
            {
                var childNode = _implementationNode.ChildNodes[i];
                ExtensionXmlGroup exGroup;
                if (childNode.Attributes != null && childNode.Attributes.Count > 0)
                {
                    // this is an extension group that extends an extension point which might defined in the same addin or another addin.
                    var parentPath = XmlHelper.GetMatchingAttribueValue(childNode, AttributePath);
                    if (parentPath == null)
                        continue;
                    exGroup = ReadExtensionGroup(childNode, parentPath, false);
                }
                else
                {
                    // this is an extension group that extends an extension point directly.
                    var parentPath = XmlHelper.GetNodeName(childNode);
                    exGroup = ReadExtensionGroup(childNode, parentPath, true);
                }

                if (exGroup != null)
                    result.AddExtensionGroup(exGroup);
            }

            return result;
        }

        ExtensionXmlGroup ReadExtensionGroup(XmlNode node, string parentPath, bool isExtensionPoint)
        {
            if (node == null || node.NodeType != XmlNodeType.Element || !node.HasChildNodes)
                return null;

            var extensionPointId = isExtensionPoint ? parentPath : StringHelper.GetExtensionPointId(parentPath);

            var result = new ExtensionXmlGroup { ParentPath = parentPath, RootIsExtensionPoint = isExtensionPoint };

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                var childNode = node.ChildNodes[i];
                var extension = ReadExtension(childNode, extensionPointId, parentPath);
                if (extension != null)
                    result.AddChild(extension);
            }
            return result;
        }

        ExtensionXml ReadExtension(XmlNode node, string extensionPointId, string parentPath)
        {
            var extension = DoReadExtension(node, extensionPointId, parentPath);
            if (extension == null)
                return null;
            if (node.HasChildNodes)
            {
                for (int i = 0; i < node.ChildNodes.Count; i++)
                    DoReadExtensionRecursively(node.ChildNodes[i], extensionPointId, extension);
            }
            return extension;
        }

        ExtensionXml DoReadExtension(XmlNode node, string extensionPointId, string parentPath)
        {
            if (node.NodeType != XmlNodeType.Element || node.Attributes == null || node.Attributes.Count == 0)
                return null;

            var head = new ExtensionHeadXml
            {
                // get the extension builder path. this must be as the same as the ExtensionBuilder.Path
                ExtensionBuilderPath = extensionPointId + SysConstants.PathSeparator + XmlHelper.GetNodeName(node),
                ParentPath = parentPath
            };
            var data = new ExtensionDataXml();

            for (int i = 0; i < node.Attributes.Count; i++)
            {
                // no repeated definition now (e.g, one xml node has 2 id attribute defined)
                var attrib = node.Attributes[i];
                if (head.Id == null && XmlHelper.AttribueNameEquals(attrib, AttributeId))
                {
                    head.Id = XmlHelper.GetAttribueValue(attrib) ?? i.ToString();
                }
                else if (head.SiblingId == null
                    && (XmlHelper.AttribueNameEquals(attrib, AttributeInsertBefore) || XmlHelper.AttribueNameEquals(attrib, AttributeInsertAfter)))
                {
                    if (XmlHelper.AttribueNameEquals(attrib, AttributeInsertBefore))
                    {
                        head.SiblingId = XmlHelper.GetAttribueValue(attrib);
                        head.RelativePosition = RelativePosition.Before;
                    }
                    else if (XmlHelper.AttribueNameEquals(attrib, AttributeInsertAfter))
                    {
                        head.SiblingId = XmlHelper.GetAttribueValue(attrib);
                        head.RelativePosition = RelativePosition.After;
                    }
                }
                else
                {
                    data.Add(XmlHelper.GetAttribueName(attrib), XmlHelper.GetAttribueValue(attrib));
                }
            }
            
            return new ExtensionXml { Head = head, Data = data };
        }

        void DoReadExtensionRecursively(XmlNode node, string extensionPointId, ExtensionXml parent)
        {
            var extension = DoReadExtension(node, extensionPointId, parent.Head.Path);
            if (extension == null)
                return;
            parent.AddChild(extension);

            if (node.HasChildNodes)
            {
                for (int i = 0; i < node.ChildNodes.Count; i++)
                    DoReadExtensionRecursively(node.ChildNodes[i], extensionPointId, extension);
            }
        }

        internal override bool TryParse(IMessageDialog dialog, FilePack filePack, out AddinResolution resolution)
        {
            resolution = null;
            if (!IsManifest(dialog, filePack.ManifestFile))
                return false;

            var manifest = new XmlManifest
            {
                AddinHeader = ReadHeader(),
                ExtensionDeclaration = ReadDeclaration(),
                ExtensionImplementation = ReadImplementation(),
            };

            var addinDir = Path.Combine(filePack.AddinProbeDirectory, filePack.AddinDirectoryName);
            var manifestFilePath = IoHelper.GetRelativePath(filePack.ManifestFile, addinDir);
            manifest.ManifestFile = new ManifestFileXml
            {
                Directory = addinDir,
                FilePath = manifestFilePath,
                LastWriteTime = IoHelper.GetLastWriteTime(filePack.ManifestFile),
                FileHash = IoHelper.GetFileHash(filePack.ManifestFile)
            };

            if (filePack.AssemblyFiles != null)
            {
                manifest.AssemblyFiles = new List<AssemblyFileXml>();
                foreach (var assemblyFile in filePack.AssemblyFiles)
                {
                    var asmFile = new AssemblyFileXml
                    {
                        FilePath = IoHelper.GetRelativePath(assemblyFile, addinDir),
                        LastWriteTime = IoHelper.GetLastWriteTime(assemblyFile)
                    };
                    manifest.AssemblyFiles.Add(asmFile);
                }
            }

            if (filePack.DataFiles != null)
            {
                manifest.DataFiles = new List<DataFileXml>();
                foreach (var dataFile in filePack.DataFiles)
                {
                    var dtFile = new DataFileXml { FilePath = IoHelper.GetRelativePath(dataFile, addinDir) };
                    manifest.DataFiles.Add(dtFile);
                }
            }

            return manifest.Introspect(dialog) && manifest.TryParse(dialog, out resolution);
        }
    }
}
