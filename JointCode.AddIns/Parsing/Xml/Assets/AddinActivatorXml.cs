//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Resolving;
using JointCode.AddIns.Resolving.Assets;
using JointCode.Common.Extensions;

namespace JointCode.AddIns.Parsing.Xml.Assets
{
    //class AddinActivationXml
    //{
    //	List<AddinActivatorXml> _activators;

    //    internal List<AddinActivatorXml> Activators { get { return _activators; } }

    //    internal void AddActivator(AddinActivatorXml addinActivator)
    //    {
    //        if (_activators == null)
    //            _activators = new List<AddinActivatorXml>();
    //        _activators.Add(addinActivator);
    //    }

    //    internal bool Introspect(ResolutionResult resolutionResult)
    //    {
    //        if (_activators == null)
    //            return true;
    //        var success = true;
    //        foreach (var activator in _activators)
    //            success &= activator.Introspect(dialog);
    //        return success;
    //    }
    //}

    class AddinActivatorXml
    {
        internal string TypeName { get; set; }

        internal bool Introspect(ResolutionResult resolutionResult)
        {
            bool result = true;
            if (TypeName.IsNullOrWhiteSpace())
            {
                resolutionResult.AddError("An addin activator must have at least a type name!");
                result = false;
            }
            return result;
        }

        internal bool TryParse(ResolutionResult resolutionResult, AddinResolution addin, out AddinActivatorResolution exResolutions)
        {
            exResolutions = new AddinActivatorResolution(addin, TypeName);
            return true;
        }
    }
}