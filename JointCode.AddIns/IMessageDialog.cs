//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;

namespace JointCode.AddIns
{
    /// <summary>
    /// A message dialog.
    /// Notes that the instance of this interface will be passed across <see cref="AppDomain"/>, thus its implementation 
    /// type must be marked with <see cref="SerializableAttribute"/>.
    /// </summary>
    public interface IMessageDialog
    {
        bool HasMessage { get; }

        void SetProgress(double progress);

        void AddWarning(string message);
        void AddError(string message);
        //void AddError(string message, Exception exception);

        void Show();
        bool Confirm();
    }
}