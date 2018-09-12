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
    /// A DefaultMessageDialog class that writes output to the console.
    /// </summary>
    //[Serializable]
    class DefaultMessageDialog : IMessageDialog
    {
        //List<string> _messages;

        //public bool HasMessage { get { return _messages != null; } }

        //public void SetProgress(double progress)
        //{
        //}

        //public void AddMessage(string message)
        //{
        //    _messages = _messages ?? new List<string>();
        //    _messages.Add("Message: " + message);
        //}

        //public void AddWarning(string message)
        //{
        //    _messages = _messages ?? new List<string>();
        //    _messages.Add("Warning: " + message);
        //}

        //public void AddError(string message)
        //{
        //    _messages = _messages ?? new List<string>();
        //    _messages.Add("Error: " + message);
        //}

        public void Show(string message)
        {
            Show(message, "Information");
        }

        public bool Confirm(string message)
        {
            return Confirm(message, "Confirm");
        }

        public void Show(string message, string title)
        {
        }

        public bool Confirm(string message, string title)
        {
            return true;
        }
    }
}