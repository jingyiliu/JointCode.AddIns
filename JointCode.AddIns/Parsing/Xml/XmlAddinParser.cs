//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Core;
using JointCode.AddIns.Core.FileScanning;
using JointCode.AddIns.Core.Helpers;
using JointCode.AddIns.Extension;
using JointCode.AddIns.Parsing.Xml.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace JointCode.AddIns.Parsing.Xml
{
    class XmlAddinParser : AddinParser
    {
        const string NodeAddin = "Addin";
        //const string NodeHeader = "Header";
        const string NodeExtensions = "Extensions";
        const string NodeExtensionSchema = "ExtensionSchema";
        const string NodeActivator = "Activator";
        const string NodeFiles = "Files";
        const string NodeFile = "File";
        const string NodeAssembly = "Assembly";

        const string AttributeGuid = "guid";
        const string AttributeCategory = "category";
        const string AttributeName = "name";
        const string AttributeVersion = "version";
        const string AttributeCompatVersion = "compatVersion";
        const string AttributeEnabled = "enabled";

        //const string Guid = "Guid";
        //const string Category = "Category";
        //const string Name = "Name";
        //const string Description = "Description";
        //const string Version = "Version";
        //const string CompatVersion = "CompatVersion";
        //const string Enabled = "Enabled";
        //const string Url = "Url";

        const string AttributeType = "type";
        const string AttributeDescription = "description";
        const string AttributePath = "path";
        const string AttributeId = "id";
        const string AttributeInsertBefore = "insertBefore";
        const string AttributeInsertAfter = "insertAfter";
        
        XmlNode _rootNode, _extensionSchemaNode, _extensionsNode, _activatorNode, _filesNode; // _headerNode

        bool ShouldParse(/*ILogger logger, */string manifestFile)
        {
            var xmlDoc = new XmlDocument(); 
            try
            {
                xmlDoc.Load(manifestFile); 
            }
            catch(Exception ex)
            {
                //logger.Error(ex); // 不是 xml 冒充 xml，或者文件 IO 冲突，所以这是异常，需要记录
                return false;
            }

            _rootNode = xmlDoc.DocumentElement;
            if (_rootNode == null || _rootNode.NodeType != XmlNodeType.Element || _rootNode.Attributes == null || _rootNode.Attributes.Count == 0 
                || !NodeAddin.Equals(XmlHelper.GetNodeName(_rootNode)))
                return false;

            //_headerNode = rootNode[NodeHeader];
            //if (_headerNode == null)
            //    return false;

            _extensionSchemaNode = _rootNode[NodeExtensionSchema];
            _extensionsNode = _rootNode[NodeExtensions];
            _activatorNode = _rootNode[NodeActivator];
            _filesNode = _rootNode[NodeFiles];

            if ((_extensionSchemaNode == null || _extensionSchemaNode.NodeType != XmlNodeType.Element || !_extensionSchemaNode.HasChildNodes)
                && (_extensionsNode == null || _extensionsNode.NodeType != XmlNodeType.Element || !_extensionsNode.HasChildNodes))
                return false;
            else
                return true;
        }

        AddinHeaderXml ReadHeader()
        {
            var header = new AddinHeaderXml();

            for (int i = 0; i < _rootNode.Attributes.Count; i++)
            {
                var attrib = _rootNode.Attributes[i];
                string val;

                if (header.Guid == null)
                {
                    val = XmlHelper.GetMatchingAttribueValue(attrib, AttributeGuid);
                    if (val != null)
                    {
                        header.Guid = val;
                        continue;
                    }
                }
                if (header.AddinCategory == null)
                {
                    val = XmlHelper.GetMatchingAttribueValue(attrib, AttributeCategory);
                    if (val != null)
                    {
                        header.AddinCategory = val;
                        continue;
                    }
                }
                if (header.Name == null)
                {
                    val = XmlHelper.GetMatchingAttribueValue(attrib, AttributeName);
                    if (val != null)
                    {
                        header.Name = val;
                        continue;
                    }
                }
                if (header.Description == null)
                {
                    val = XmlHelper.GetMatchingAttribueValue(attrib, AttributeDescription);
                    if (val != null)
                    {
                        header.Description = val;
                        continue;
                    }
                }
                if (header.Version == null)
                {
                    val = XmlHelper.GetMatchingAttribueValue(attrib, AttributeVersion);
                    if (val != null)
                    {
                        header.Version = val;
                        continue;
                    }
                }
                if (header.CompatVersion == null)
                {
                    val = XmlHelper.GetMatchingAttribueValue(attrib, AttributeCompatVersion);
                    if (val != null)
                    {
                        header.CompatVersion = val;
                        continue;
                    }
                }
                if (header.Enabled == null)
                {
                    val = XmlHelper.GetMatchingAttribueValue(attrib, AttributeEnabled);
                    if (val != null)
                    {
                        header.Enabled = val;
                        continue;
                    }
                }

                header.AddProperty(XmlHelper.GetAttribueName(attrib), XmlHelper.GetAttribueValue(attrib));
            }

            //foreach (XmlNode node in _headerNode.ChildNodes)
            //{
            //    if (node.NodeType != XmlNodeType.Element)
            //        continue;
            //    string val;
            //    if (header.Guid == null)
            //    {
            //        val = XmlHelper.GetMatchingNodeValue(node, Guid);
            //        if (val != null)
            //        {
            //            header.Guid = val;
            //            continue;
            //        }
            //    }
            //    if (header.AddinCategory == null)
            //    {
            //        val = XmlHelper.GetMatchingNodeValue(node, Category);
            //        if (val != null)
            //        {
            //            header.AddinCategory = val;
            //            continue;
            //        }
            //    }
            //    if (header.Name == null)
            //    {
            //        val = XmlHelper.GetMatchingNodeValue(node, Name);
            //        if (val != null)
            //        {
            //            header.Name = val;
            //            continue;
            //        }
            //    }
            //    if (header.Description == null)
            //    {
            //        val = XmlHelper.GetMatchingNodeValue(node, Description);
            //        if (val != null)
            //        {
            //            header.Description = val;
            //            continue;
            //        }
            //    }
            //    if (header.Version == null)
            //    {
            //        val = XmlHelper.GetMatchingNodeValue(node, Version);
            //        if (val != null)
            //        {
            //            header.Version = val;
            //            continue;
            //        }
            //    }
            //    if (header.CompatVersion == null)
            //    {
            //        val = XmlHelper.GetMatchingNodeValue(node, CompatVersion);
            //        if (val != null)
            //        {
            //            header.CompatVersion = val;
            //            continue;
            //        }
            //    }
            //    if (header.Enabled == null)
            //    {
            //        val = XmlHelper.GetMatchingNodeValue(node, Enabled);
            //        if (val != null)
            //        {
            //            header.Enabled = val;
            //            continue;
            //        }
            //    }
            //    header.AddProperty(XmlHelper.GetNodeName(node), XmlHelper.GetNodeValue(node));
            //}

            return header;
        }

        AddinActivatorXml ReadAddinActivator()
        {
            if (_activatorNode == null || _activatorNode.NodeType != XmlNodeType.Element || _activatorNode.Attributes == null || _activatorNode.Attributes.Count == 0)
                return null;

            var typeName = XmlHelper.GetMatchingAttribueValue(_activatorNode, AttributeType); 
            if (string.IsNullOrEmpty(typeName))
                return null;

            return new AddinActivatorXml
            {
                TypeName = typeName,
            };
        }

        ExtensionSchemaXml ReadExtensionSchema()
        {
            if (_extensionSchemaNode == null || _extensionSchemaNode.NodeType != XmlNodeType.Element || !_extensionSchemaNode.HasChildNodes)
                return null;

            var schema = new ExtensionSchemaXml();
            for (int i = 0; i < _extensionSchemaNode.ChildNodes.Count; i++)
            {
                var node = _extensionSchemaNode.ChildNodes[i];
                if (node.NodeType != XmlNodeType.Element || node.Attributes == null || node.Attributes.Count == 0)
                    continue; // not a valid definition

                var epTypeName = XmlHelper.GetMatchingAttribueValue(node, AttributeType); //Get the extension point type
                if (!string.IsNullOrEmpty(epTypeName)) //'Type' attribute defined: this is a valid extension point
                {
                    // This is an extension point that defined in this addin
                    var epName = XmlHelper.GetNodeName(node);
                    var ep = new ExtensionPointXml
                    {
                        Name = epName,
                        TypeName = epTypeName,
                        Description = XmlHelper.GetMatchingAttribueValue(node, AttributeDescription)
                    };

                    schema.AddExtensionPoint(ep);

                    if (node.HasChildNodes)
                    {
                        for (int j = 0; j < node.ChildNodes.Count; j++)
                        {
                            var eb = ReadExtensionBuilder(node.ChildNodes[j], epName, epName);
                            if (eb != null)
                                ep.AddChild(eb);
                        }
                    }
                }
                else
                {
                    // This might be an extension point that defined in another addin
                    var ebGroupPath = XmlHelper.GetMatchingAttribueValue(node, AttributePath);
                    ebGroupPath = ExtensionHelper.NormalizePath(ebGroupPath);
                    if (ebGroupPath == null) // not a valid definition
                        continue;

                    var ebGroup = ReadExtensionBuilderGroup(node, ebGroupPath);
                    if (ebGroup != null)
                        schema.AddExtensionBuilderGroup(ebGroup);
                }
            }

            return schema.ExtensionBuilderGroups == null && schema.ExtensionPoints == null ? null : schema;
        }

        ExtensionBuilderXmlGroup ReadExtensionBuilderGroup(XmlNode node, string parentPath)
        {
            if (node.NodeType != XmlNodeType.Element || !node.HasChildNodes)
                return null;

            var result = new ExtensionBuilderXmlGroup {ParentPath = parentPath};
            var extensionPointPath = ExtensionHelper.GetExtensionPointName(parentPath);

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                var childNode = node.ChildNodes[i];
                var eb = ReadExtensionBuilder(childNode, parentPath, extensionPointPath);
                if (eb != null)
                    result.AddChild(eb);
            }

            return result.Children.Count > 0 ? result : null;
        }

        ExtensionBuilderXml ReadExtensionBuilder(XmlNode node, string parentPath, string extensionPointPath)
        {
            var result = DoReadExtensionBuilder(node, parentPath, extensionPointPath);
            if (result == null)
                return null;
            if (node.HasChildNodes)
            {
                for (int i = 0; i < node.ChildNodes.Count; i++)
                    ReadExtensionBuilderRecursively(node.ChildNodes[i], result, extensionPointPath);
            }
            return result;
        }

        void ReadExtensionBuilderRecursively(XmlNode node, ExtensionBuilderXml parent, string extensionPointPath)
        {
            var eb = DoReadExtensionBuilder(node, parent.Path, extensionPointPath);
            if (eb == null)
                return;
            parent.AddChild(eb);
            // recursively add children
            if (node.HasChildNodes)
            {
                for (int i = 0; i < node.ChildNodes.Count; i++)
                    ReadExtensionBuilderRecursively(node.ChildNodes[i], eb, extensionPointPath);
            }
        }

        static ExtensionBuilderXml DoReadExtensionBuilder(XmlNode node, string parentPath, string extensionPointPath)
        {
            if (node.NodeType != XmlNodeType.Element)
                return null;

            if (node.Attributes == null || node.Attributes.Count == 0) // a referenced extension builder
            {
                return new ReferencedExtensionBuilderXml
                {
                    Name = XmlHelper.GetNodeName(node),
                    ParentPath = parentPath,
                    ExtensionPointName = extensionPointPath
                };
            }
            else // a normal extension builder
            {
                var typeName = XmlHelper.GetMatchingAttribueValue(node, AttributeType); // get the extension builder type
                if (string.IsNullOrEmpty(typeName))
                    return null;

                var ebName = XmlHelper.GetNodeName(node);

                return new DeclaredExtensionBuilderXml
                {
                    TypeName = typeName,
                    Name = ebName,
                    Description = XmlHelper.GetMatchingAttribueValue(node, AttributeDescription),
                    ParentPath = parentPath,
                    ExtensionPointName = extensionPointPath
                };
            }
        }

        ExtensionsXml ReadExtensions()
        {
            if (_extensionsNode == null || _extensionsNode.NodeType != XmlNodeType.Element || !_extensionsNode.HasChildNodes)
                return null;

            var result = new ExtensionsXml();
            for (int i = 0; i < _extensionsNode.ChildNodes.Count; i++)
            {
                var childNode = _extensionsNode.ChildNodes[i];
                ExtensionXmlGroup exGroup;
                if (childNode.Attributes != null && childNode.Attributes.Count > 0)
                {
                    // this is an extension group that extends an extension point which might be defined in the same addin or another addin.
                    var parentPath = XmlHelper.GetMatchingAttribueValue(childNode, AttributePath);
                    parentPath = ExtensionHelper.NormalizePath(parentPath);
                    if (parentPath == null)
                        continue;
                    exGroup = ReadExtensionGroup(childNode, parentPath, false);
                }
                else
                {
                    // this is an extension group that extends an extension point directly, the extension point itself might be defined in the same addin or another addin..
                    var parentPath = XmlHelper.GetNodeName(childNode);
                    exGroup = ReadExtensionGroup(childNode, parentPath, true);
                }

                if (exGroup != null)
                    result.AddExtensionGroup(exGroup);
            }

            return result.ExtensionGroups == null ? null : result;
        }

        ExtensionXmlGroup ReadExtensionGroup(XmlNode node, string parentPath, bool isExtensionPoint)
        {
            if (node == null || node.NodeType != XmlNodeType.Element || !node.HasChildNodes)
                return null;

            var extensionPointPath = isExtensionPoint ? parentPath : ExtensionHelper.GetExtensionPointName(parentPath);

            var result = new ExtensionXmlGroup { ParentPath = parentPath, RootIsExtensionPoint = isExtensionPoint };

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                var childNode = node.ChildNodes[i];
                var extension = ReadExtension(childNode, extensionPointPath, parentPath);
                if (extension != null)
                    result.AddChild(extension);
            }
            return result;
        }

        ExtensionXml ReadExtension(XmlNode node, string extensionPointPath, string parentPath)
        {
            var extension = DoReadExtension(node, extensionPointPath, parentPath);
            if (extension == null)
                return null;
            if (node.HasChildNodes)
            {
                for (int i = 0; i < node.ChildNodes.Count; i++)
                    DoReadExtensionRecursively(node.ChildNodes[i], extensionPointPath, extension);
            }
            return extension;
        }

        ExtensionXml DoReadExtension(XmlNode node, string extensionPointPath, string parentPath)
        {
            if (node.NodeType != XmlNodeType.Element)// || node.Attributes == null || node.Attributes.Count == 0)
                return null;

            var head = new ExtensionHeadXml
            {
                // get the extension builder path. this must be as the same as the ExtensionBuilder.Path
                ExtensionBuilderPath = extensionPointPath + SysConstants.PathSeparator + XmlHelper.GetNodeName(node),
                ParentPath = parentPath
            };

            if (node.Attributes == null || node.Attributes.Count == 0)
                return new ExtensionXml { Head = head };

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

        void DoReadExtensionRecursively(XmlNode node, string extensionPointPath, ExtensionXml parent)
        {
            var extension = DoReadExtension(node, extensionPointPath, parent.Head.Path);
            if (extension == null)
                return;
            parent.AddChild(extension);

            if (node.HasChildNodes)
            {
                for (int i = 0; i < node.ChildNodes.Count; i++)
                    DoReadExtensionRecursively(node.ChildNodes[i], extensionPointPath, extension);
            }
        }

        bool TryReadFiles(string addinDir, out List<AssemblyFileXml> assemblyFiles, out List<DataFileXml> dataFiles)
        {
            assemblyFiles = null;
            dataFiles = null;

            if (_filesNode == null || _filesNode.NodeType != XmlNodeType.Element || !_filesNode.HasChildNodes)
                return false;

            for (int i = 0; i < _filesNode.ChildNodes.Count; i++)
            {
                var childNode = _filesNode.ChildNodes[i];
                var path = XmlHelper.GetMatchingAttribueValue(childNode, AttributePath);

                string fullPath;
                if (Path.IsPathRooted(path))
                {
                    if (!File.Exists(path))
                        continue;
                    fullPath = path;
                    path = IoHelper.GetRelativePath(path, addinDir);
                }
                else
                {
                    fullPath = Path.Combine(addinDir, path);
                    if (!File.Exists(fullPath))
                        continue;
                }

                if (XmlHelper.IsMatchingNode(childNode, NodeFile))
                {
                    var dtFile = new DataFileXml { FilePath = path };
                    dataFiles = dataFiles ?? new List<DataFileXml>();
                    dataFiles.Add(dtFile);
                }
                else if (XmlHelper.IsMatchingNode(childNode, NodeAssembly))
                {
                    var asmFile = new AssemblyFileXml
                    {
                        FilePath = path,
                        LastWriteTime = IoHelper.GetLastWriteTime(fullPath)
                    };
                    assemblyFiles = assemblyFiles ?? new List<AssemblyFileXml>();
                    assemblyFiles.Add(asmFile);
                }
            }

            return assemblyFiles != null || dataFiles != null;
        }

        internal override bool TryParse(/*ILogger logger, */ScanFilePack scanFilePack, out AddinManifest addinManifest)
        {
            addinManifest = null;
            if (!ShouldParse(scanFilePack.ManifestFile))
                return false;

            var header = ReadHeader();

            var extensionSchema = ReadExtensionSchema();
            var extensions = ReadExtensions();

            //if (extensionSchema == null && extensions == null)
            //    return false;

            var manifest = new XmlAddinManifest
            {
                AddinHeader = header,
                ExtensionSchema = extensionSchema,
                Extensions = extensions,
                AddinActivator = ReadAddinActivator(),
            };

            var addinDir = Path.Combine(scanFilePack.AddinProbingDirectory, scanFilePack.AddinDirectory);
            var manifestFilePath = IoHelper.GetRelativePath(scanFilePack.ManifestFile, addinDir);
            var fi = IoHelper.GetFileInfo(scanFilePack.ManifestFile);
            manifest.ManifestFile = new ManifestFileXml
            {
                Directory = addinDir,
                FilePath = manifestFilePath,
                LastWriteTime = fi.LastWriteTime,
                FileLength = fi.Length,
                FileHash = IoHelper.GetFileHash(scanFilePack.ManifestFile),
            };

            // if the manifest file does not contains a Files node, then use the ScanFilePack
            List<AssemblyFileXml> assemblyFiles;
            List<DataFileXml> dataFiles;

            if (!TryReadFiles(addinDir, out assemblyFiles, out dataFiles))
            {
                if (scanFilePack.AssemblyFiles != null)
                {
                    assemblyFiles = new List<AssemblyFileXml>();
                    foreach (var assemblyFile in scanFilePack.AssemblyFiles)
                    {
                        var asmFile = new AssemblyFileXml
                        {
                            FilePath = IoHelper.GetRelativePath(assemblyFile, addinDir),
                            LastWriteTime = IoHelper.GetLastWriteTime(assemblyFile)
                        };
                        assemblyFiles.Add(asmFile);
                    }
                }

                if (scanFilePack.DataFiles != null)
                {
                    dataFiles = new List<DataFileXml>();
                    foreach (var dataFile in scanFilePack.DataFiles)
                    {
                        var dtFile = new DataFileXml { FilePath = IoHelper.GetRelativePath(dataFile, addinDir) };
                        dataFiles.Add(dtFile);
                    }
                }
            }

            manifest.AssemblyFiles = assemblyFiles;
            manifest.DataFiles = dataFiles;

            addinManifest = manifest;
            return true;
        }
    }
}
