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
using JointCode.Common.IO;

namespace JointCode.AddIns.Core
{
	static class UidProvider
	{
		internal const int InvalidAssemblyUid = int.MinValue;
		internal const int InvalidExtensionBuilderUid = int.MinValue;
		internal const int InvalidExtensionPointUid = int.MinValue;
		internal const int InvalidAddinUid = int.MinValue;
		
        static int _addinUid;
        static int _assemblyUid;
        static int _extensionBuilderUid;
        static int _extensionPointUid;
        
        static UidProvider()
        {
            _addinUid = InvalidAddinUid;
            _assemblyUid = InvalidAssemblyUid;
            _extensionBuilderUid = InvalidExtensionBuilderUid;
            _extensionPointUid = InvalidExtensionPointUid;
        }

        internal static int GetNextExtensionBuilderUid()
        {
        	return ++_extensionBuilderUid;
        }
        
        internal static int GetNextExtensionPointUid()
        {
        	return ++_extensionPointUid;
        }
        
        internal static int GetNextAssemblyUid()
        {
        	return ++_assemblyUid;
        }
        
        internal static int GetNextAddinUid()
        {
        	return ++_addinUid;
        }
        
        internal static void Read(Stream reader)
        {
        	_addinUid = reader.ReadInt32();
        	_assemblyUid = reader.ReadInt32();
        	_extensionPointUid = reader.ReadInt32();
        	_extensionBuilderUid = reader.ReadInt32();
        }
        
        internal static void Write(Stream writer)
        {
        	writer.WriteInt32(_addinUid);
        	writer.WriteInt32(_assemblyUid);
        	writer.WriteInt32(_extensionPointUid);
        	writer.WriteInt32(_extensionBuilderUid);
        }
	}
}
