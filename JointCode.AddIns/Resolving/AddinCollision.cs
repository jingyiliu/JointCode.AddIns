//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.Collections.Generic;
using JointCode.AddIns.Resolving.Assets;

namespace JointCode.AddIns.Resolving
{
    class AddinCollision
    {
        readonly Dictionary<CollisionKey, List<AddinResolution>> _key2CollisionItems = new Dictionary<CollisionKey, List<AddinResolution>>();

        internal int Count { get { return _key2CollisionItems.Count; } }
        internal IEnumerable<List<AddinResolution>> Items { get { return _key2CollisionItems.Values; } }

        internal void Add(CollisionKey key, AddinResolution collisionAddin1, AddinResolution collisionAddin2)
        {
            List<AddinResolution> collisionAddins;
            if (!_key2CollisionItems.TryGetValue(key, out collisionAddins))
            {
                collisionAddins = new List<AddinResolution>();
                _key2CollisionItems.Add(key, collisionAddins);
            }
            collisionAddins.Add(collisionAddin1);
            collisionAddins.Add(collisionAddin2);
        }

        /// <summary>
        /// Trim colliding addins that failed to resolve.
        /// </summary>
        internal void Trim()
        {
            var tmpList = new List<KeyValuePair<CollisionKey, List<AddinResolution>>>(_key2CollisionItems);
            foreach (var kv in tmpList)
            {
                for (int i = kv.Value.Count - 1; i >= 0; i--)
                {
                    if (kv.Value[i].ResolutionStatus == ResolutionStatus.Failed)
                        kv.Value.RemoveAt(i);
                }
                if (kv.Value.Count <= 1)
                    _key2CollisionItems.Remove(kv.Key);
            }
        }
    }
}