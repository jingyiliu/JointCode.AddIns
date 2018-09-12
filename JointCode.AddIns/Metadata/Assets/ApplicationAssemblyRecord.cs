using JointCode.AddIns.Core.Runtime;
using JointCode.Common;
using System;
using System.Globalization;
using System.IO;

namespace JointCode.AddIns.Metadata.Assets
{
    class ApplicationAssemblyRecord : AssemblyKey, ISerializableRecord
    {
        internal static MyFunc<ApplicationAssemblyRecord> Factory = () => new ApplicationAssemblyRecord();

        private ApplicationAssemblyRecord() { }

        internal ApplicationAssemblyRecord(string name, Version version, CultureInfo cultrue, byte[] publicKeyToken) 
            : base(name, version, cultrue, publicKeyToken)
        { }

        /// <summary>
        /// Gets or sets the unique identifier (uid) of the application assembly.
        /// </summary>
        internal int Uid { get; set; }

        public void Read(Stream reader)
        {
        }

        public void Write(Stream writer)
        {
        }
    }
}