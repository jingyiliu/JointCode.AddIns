using System;
using JointCode.AddIns.Core;
using JointCode.AddIns.Metadata;
using JointCode.AddIns.Metadata.Assets;

namespace JointCode.AddIns.Extension
{
    public class AddinExtension
    {
        readonly AddinRecord _addinRecord;
        readonly DefaultAddinContext _addinContext;
        readonly AddinEngine _addinEngine;

        internal AddinExtension(AddinRecord addinRecord, AddinEngine addinEngine, DefaultAddinContext addinContext)
        {
            _addinRecord = addinRecord;
            _addinEngine = addinEngine;
            _addinContext = addinContext;
        }

        //public event EventHandler<AddinStatusChangedEventArgs> ExtensionChanged;
        //public void AddExtensionPoint() { }
        //public void AddExtensionBuilder() { }
        //public void AddExtension() { }

        // 该方法应该在 AddinEngine 级别提供，且应在 AddinEngine 初始化之后、Addin 加载之前便可获取，并可同时获取其下的 ExtensionBuilder 和 Extension 等内容
        public ExtensionPointDescription[] GetExtensionPointDescriptions()
        {
            if (_addinRecord.ExtensionPoints == null)
                return null;
            var result = new ExtensionPointDescription[_addinRecord.ExtensionPoints.Count];
            for (int i = 0; i < _addinRecord.ExtensionPoints.Count; i++)
                result[i] = _addinRecord.ExtensionPoints[i].ExtensionPointDescription;
            return result;
        }

        // register extension builders and load extensions that extends an extenstion point declared in another addin.
        // this method is called when the specified extension point is loading.
        internal void LoadInto(ExtensionPointRecord extensionPointRecord)
        {
            _addinContext.Addin.ThrowIfAddinIsDisabled();
            if (!_addinContext.Addin.Start())
                return;
            _addinEngine.LoadIntoExtensionPoint(_addinContext, extensionPointRecord);
        }

        internal bool TryLoadExtensionPoint(ExtensionPointRecord epRecord, object extensionRoot)
        {
            _addinContext.Addin.ThrowIfAddinIsDisabled();
            if (!_addinContext.Addin.Start())
                return false;
            if (epRecord == null)
                return false;
            //if (epRecord.Loaded)
            //    return true;
            return _addinEngine.TryLoadExtensionPoint(_addinContext, epRecord, extensionRoot);
        }

        internal void LoadExtensionPoint(ExtensionPointRecord epRecord, object extensionRoot)
        {
            _addinContext.Addin.ThrowIfAddinIsDisabled();
            if (!_addinContext.Addin.Start())
                return;
            if (epRecord == null)
                return;
            //if (epRecord.Loaded)
            //    return;
            _addinEngine.LoadExtensionPoint(_addinContext, epRecord, extensionRoot);
        }

        internal void UnloadExtensionPoint(ExtensionPointRecord epRecord)
        {
            _addinContext.Addin.ThrowIfAddinIsDisabled();
            if (epRecord == null)
                return;
            //if (!epRecord.Loaded)
            //    return;
            _addinEngine.UnloadExtensionPoint(_addinContext, epRecord);
        }

        ///// <summary>
        ///// Tries to load the extension point identified by the type of <see cref="TExtensionRoot"/>.
        ///// If no matching extension point found, just exit the method silently.
        ///// If the addin has not been started, this will starts the addin as well.
        ///// </summary>
        ///// <typeparam name="TExtensionRoot">The type of the extension root.</typeparam>
        ///// <param name="extensionRoot">The extension root.</param>
        ///// <returns></returns>
        //public bool TryLoadExtensionPoint<TExtensionRoot>(TExtensionRoot extensionRoot)
        //{
        //    ThrowIfAddinIsDisabled();
        //    if (!Start())
        //        return false;
        //    var extensionPointPath = _addinContext.Framework.NameConvention.GetExtensionPointName(typeof(TExtensionRoot));
        //    var epRecord = _addinRecord.GetExtensionPoint(extensionPointPath);
        //    if (epRecord == null)
        //        return false;
        //    if (epRecord.Loaded)
        //        return true;
        //    return _addinEngine.TryLoadExtensionPoint(this, epRecord, extensionRoot);
        //}

        //public bool TryLoadExtensionPoint(string extensionPointPath, object extensionRoot)
        //{
        //    ThrowIfAddinIsDisabled();
        //    if (!Start())
        //        return false;
        //    var epRecord = _addinRecord.GetExtensionPoint(extensionPointPath);
        //    if (epRecord == null)
        //        return false;
        //    if (epRecord.Loaded)
        //        return true;
        //    return _addinEngine.TryLoadExtensionPoint(this, epRecord, extensionRoot);
        //}

        //public void LoadExtensionPoint<TExtensionRoot>(TExtensionRoot extensionRoot)
        //{
        //    ThrowIfAddinIsDisabled();
        //    if (!Start())
        //        return;
        //    var extensionPointPath = _addinContext.Framework.NameConvention.GetExtensionPointName(typeof(TExtensionRoot));
        //    var epRecord = _addinRecord.GetExtensionPoint(extensionPointPath);
        //    if (epRecord == null)
        //        throw _addinEngine.GetExtensionPointNotFoundException(typeof(TExtensionRoot), extensionPointPath);
        //    if (epRecord.Loaded)
        //        return;
        //    _addinEngine.LoadExtensionPoint(this, epRecord, extensionRoot);
        //}

        //public void LoadExtensionPoint(string extensionPointPath, object extensionRoot)
        //{
        //    ThrowIfAddinIsDisabled();
        //    if (!Start())
        //        return;
        //    var epRecord = _addinRecord.GetExtensionPoint(extensionPointPath);
        //    if (epRecord == null)
        //        throw _addinEngine.GetExtensionPointNotFoundException(extensionPointPath);
        //    if (epRecord.Loaded)
        //        return;
        //    _addinEngine.LoadExtensionPoint(this, epRecord, extensionRoot);
        //}

        //public void UnloadExtensionPoint(Type extensionRootType)
        //{
        //    ThrowIfAddinIsDisabled();

        //    var extensionPointPath = _addinContext.Framework.NameConvention.GetExtensionPointName(extensionRootType);
        //    var epRecord = _addinRecord.GetExtensionPoint(extensionPointPath);
        //    if (epRecord == null)
        //        throw _addinEngine.GetExtensionPointNotFoundException(extensionRootType, extensionPointPath);

        //    _addinEngine.UnloadExtensionPoint(this, epRecord);
        //}

        //public void UnloadExtensionPoint(string extensionPointPath)
        //{
        //    ThrowIfAddinIsDisabled();

        //    var epRecord = _addinRecord.GetExtensionPoint(extensionPointPath);
        //    if (epRecord == null)
        //        throw _addinEngine.GetExtensionPointNotFoundException(extensionPointPath);

        //    _addinEngine.UnloadExtensionPoint(this, epRecord);
        //}

        internal void UnloadExtensionPoints()
        {
            if (_addinRecord.ExtensionPoints == null)
                return;
            foreach (var extensionPointRecord in _addinRecord.ExtensionPoints)
                _addinEngine.UnloadExtensionPoint(_addinContext, extensionPointRecord);
        }
    }
}
