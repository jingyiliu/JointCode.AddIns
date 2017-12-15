//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;

namespace JointCode.AddIns.Core
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    public class OperationResult
    {
        static readonly OperationResult _sucessResult = new OperationResult();
        public static OperationResult SucessResult { get { return _sucessResult; } }

        public bool Sucess { get; internal set; }
        public string Message { get; internal set; }
        public Exception Exception { get; internal set; }
        public int ReturnCode { get; internal set; }
    }
}
