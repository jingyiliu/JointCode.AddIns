//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.ComponentModel.Design;
using JointCode.AddIns.Core.Runtime;
using JointCode.Common.Logging;

namespace JointCode.AddIns.Core
{
    class AddinContext : IAddinContext
    {
        readonly RuntimeSystem _runtimeSystem;
        readonly AddinFileSystem _addinFileSystem;

        internal AddinContext(RuntimeSystem runtimeSystem, AddinFileSystem addinFileSystem)
        {
            _runtimeSystem = runtimeSystem;
            _addinFileSystem = addinFileSystem;
        }

        #region IAddinContext Members

        public RuntimeSystem RuntimeSystem { get { return _runtimeSystem; } }
        public AddinFileSystem AddinFileSystem { get { return _addinFileSystem; } }

        public IServiceContainer ServiceContainer
        {
            get { throw new NotImplementedException(); }
        }

        //public IEventAggregator EventAggregator
        //{
        //    get { throw new NotImplementedException(); }
        //}

        public ILogger Logger
        {
            get { throw new NotImplementedException(); }
        }

        //public IStringLocalizer StringLocalizer
        //{
        //    get { throw new NotImplementedException(); }
        //}

        #endregion

        //    public void RegisterCondition(string extensionPath, Condition condition)
        //    {
        //        if (_extConditions == null)
        //            _extConditions = new Dictionary<string, Condition>(new StringEqualityComparer());
        //        _extConditions.Add(extensionPath, condition);
        //    }

        //    public void UnregisterCondition(string extensionPath)
        //    {
        //        if (_extConditions == null)
        //            return;
        //        _extConditions.Remove(extensionPath);
        //    }

        //    internal Condition GetCondition(string extensionPath)
        //    {
        //        if (_extConditions == null)
        //            return null;
        //        Condition condition;
        //        if (_extConditions.TryGetValue(extensionPath, out condition))
        //            return condition;
        //        return null;
    }
}