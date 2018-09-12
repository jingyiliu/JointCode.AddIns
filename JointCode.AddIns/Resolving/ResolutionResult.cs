using System;
using System.Collections.Generic;
using System.IO;

namespace JointCode.AddIns.Resolving
{
    //[Serializable]
    class ResolutionResult
    {
        List<string> _messages, _errors;

        internal bool NewAddinsFound { get; set; }
        //internal Stream AddinStorageSteam { get; set; } // todo: 用 MemoryStream 跨域传回插件元数据，然后转为 FileStream 在原 AppDomain 保存到 AddinStorage？

        internal bool HasMessage { get { return _messages != null || _errors != null; } }

        internal void AddMessage(string message)
        {
            _messages = _messages ?? new List<string>();
            _messages.Add(message);
        }

        internal void AddError(string message)
        {
            _errors = _errors ?? new List<string>();
            _errors.Add(message);
        }

        internal string GetFormattedString()
        {
            string result = string.Empty;
            if (_messages != null)
            {
                result += "The addin resolution returned the following messages:" + Environment.NewLine;
                foreach (var message in _messages)
                    result += message + Environment.NewLine;
            }
            if (_errors != null)
            {
                result += "The addin resolution returned the following errors:" + Environment.NewLine;
                foreach (var error in _errors)
                    result += error + Environment.NewLine;
            }
            return result;
        }
    }
}