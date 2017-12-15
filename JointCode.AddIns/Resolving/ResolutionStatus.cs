//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;

namespace JointCode.AddIns.Resolving
{
    // when resolving the dependencies, we can expect that all dependencies of an asset has been retrieved, but we 
    // can not assure that all dependencies of an dependency has already been retrieved too.
    // 1. when an dependency is unavailable, the resolution is failed.
    // 2. when the dependency of an dependency is not ready, the resolution status of current asset is pending.
    [Flags]
    enum ResolutionStatus 
    {
        /// <summary>
        /// all the parent addins and the addin itself has been resolved successfully.
        /// </summary>
        Success = 1,
        /// <summary>
        /// at least one of the parent addin has not resolved yet.
        /// </summary>
        Pending = 2,
        /// <summary>
        /// at least one dependency of the addin is not satisfied.
        /// </summary>
        Failed = 4, 
    }

    static class ResolutionStatusExtensions
    {
        internal static bool IsFailed(this ResolutionStatus status)
        {
            return (status & ResolutionStatus.Failed) != 0;
        }

        internal static bool IsPending(this ResolutionStatus status)
        {
            return (status & ResolutionStatus.Pending) != 0;
        }

        internal static bool IsSuccess(this ResolutionStatus status)
        {
            return status == ResolutionStatus.Success;
        }
    }
}