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

namespace JointCode.AddIns.Extension.Loaders
{
    class ExtensionLoaderCollection : IEnumerable<ExtensionLoader>
    {
        List<ExtensionLoader> _loaders;

        public int Count
        {
            get
            {
                if (_loaders == null)
                    return 0;
                return _loaders.Count;
            }
        }

        public ExtensionLoader this[int index]
        {
            get
            {
                if (_loaders == null || index > _loaders.Count)
                    throw new ArgumentOutOfRangeException();
                return _loaders[index];
            }
            set
            {
                if (_loaders == null || index > _loaders.Count)
                    throw new ArgumentOutOfRangeException();
                _loaders[index] = value;
            }
        }

        public void Add(ExtensionLoader loader)
        {
            if (_loaders == null)
                _loaders = new List<ExtensionLoader>();
            _loaders.Add(loader);
        }

        public void Insert(int index, ExtensionLoader loader)
        {
            if (_loaders == null)
                _loaders = new List<ExtensionLoader>();
            _loaders.Insert(index, loader);
        }

        public void Remove(ExtensionLoader loader)
        {
            if (_loaders == null)
                return;
            _loaders.Remove(loader);
        }

        #region IEnumerable<ExtensionLoader> Members

        public IEnumerator<ExtensionLoader> GetEnumerator()
        {
            if (_loaders == null)
                yield return null;
            for (int i = 0; i < _loaders.Count; i++)
                yield return _loaders[i];
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
