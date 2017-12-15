//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.Collections.Generic;

namespace JointCode.AddIns.Core
{
    /// <summary>
    /// A ConsoleMessageDialog class that writes output to the console.
    /// </summary>
    [Serializable]
    class ConsoleMessageDialog : IMessageDialog
    {
        List<string> _messages;
 
        public bool HasMessage { get { return _messages != null; } }

        public void SetProgress(double progress)
        {
        }

        public void AddWarning(string message)
        {
            _messages = _messages ?? new List<string>();
            _messages.Add("Warning: " + message);
        }

        public void AddError(string message)
        {
            _messages = _messages ?? new List<string>();
            _messages.Add("Error: " + message);
        }

        public void Show()
        {
            if (_messages == null)
                return;
            var result = string.Empty;
            foreach (var message in _messages)
                result += message + Environment.NewLine;
            Console.Write(result);
        }

        public bool Confirm()
        {
            return false;
        }
    }
}