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
using JointCode.Common.Extensions;

namespace JointCode.AddIns.Parsing.Xml.Assets
{
    class ActivationXml
    {
    	List<ActivatorXml> _activators;
    	
        internal List<ActivatorXml> Activators { get { return _activators; } }

        internal void AddActivator(ActivatorXml activator)
        {
            if (_activators == null)
                _activators = new List<ActivatorXml>();
            _activators.Add(activator);
        }

        internal bool Introspect(IMessageDialog dialog)
        {
            if (_activators == null)
                return true;
            var success = true;
            foreach (var activator in _activators)
                success &= activator.Introspect(dialog);
            return success;
        }
    }

    class ActivatorXml
    {
        internal string TypeName { get; set; }

        internal bool Introspect(IMessageDialog dialog)
        {
            bool result = true;
            if (TypeName.IsNullOrWhiteSpace())
            {
                dialog.AddError("An activator must at least provide a type name!");
                result = false;
            }
            return result;
        }
	
        //		internal bool TryParse(IMessenger messenger, out List<ExtensionResolution> exResolutions)
        //		{
        //		}
    }
}