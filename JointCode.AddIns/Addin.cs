using JointCode.AddIns.Core;
using JointCode.AddIns.Core.Runtime;
using JointCode.AddIns.Extension;
using JointCode.AddIns.Metadata;
using JointCode.Common;
using JointCode.Common.Extensions;
using System;
using System.Collections.Generic;

namespace JointCode.AddIns
{
    public class AddinStatusChangedEventArgs : EventArgs
    {
        internal AddinStatusChangedEventArgs(AddinStatus addinStatus, Addin addin)
        {
            AddinStatus = addinStatus;
            Addin = addin;
        }
        public AddinStatus AddinStatus { get; private set; }
        public Addin Addin { get; private set; }
    }

    public class Addin
    {
        List<Addin> _parentAddins; // addins that this addin depends on (referenced or extended by this addins), i.e, parent addins.
        List<Addin> _childAddins; // addins that depends on this addin (reference or extend this addins), i.e, child addins.
        bool _started = false; // todo: thread safety???

        IAddinActivator _addinActivator;
        readonly AddinEngine _addinEngine;
        readonly DefaultAddinContext _addinContext;
        readonly AddinRecord _addinRecord;
        readonly AddinRuntime _addinRuntime;
        readonly AddinFile _addinFile;
        readonly AddinExtension _addinExtension;

        /// <summary>
        /// Create an <see cref="Addin"/> instance.
        /// </summary>
        /// <param name="addinEngine"></param>
        /// <param name="addinFramework"></param>
        /// <param name="addinRecord"></param>
        internal Addin(AddinEngine addinEngine, AddinFramework addinFramework, AddinRecord addinRecord)
        {
            _addinEngine = addinEngine;
            _addinRecord = addinRecord;

            _addinContext = new DefaultAddinContext(addinFramework, this);
            _addinRuntime = new AddinRuntime(addinEngine.RuntimeAssemblyResolver, this);
            _addinFile = new AddinFile(addinRecord);
            _addinExtension = new AddinExtension(addinRecord, addinEngine, _addinContext);
        }

        /// <param name="parentAddins">addins that this addin directly depends on (referenced or extended by this addins).</param>
        internal void SetParentAddins(List<Addin> parentAddins)
        {
            if (_parentAddins != null)
                throw new InvalidOperationException("Addin initialization error!");
            _parentAddins = parentAddins;
        }

        /// <param name="childAddins">addins that directly depends on this addin (refers to or extends this addins).</param>
        internal void SetChildAddins(List<Addin> childAddins)
        {
            if (_childAddins != null)
                throw new InvalidOperationException("Addin initialization error!");
            _childAddins = childAddins;
        }

        internal AddinRecord AddinRecord { get { return _addinRecord; } }
        internal List<Addin> ParentAddins { get { return _parentAddins; } }

        public event EventHandler<AddinStatusChangedEventArgs> StatusChanged;

        public IAddinContext Context { get { return _addinContext; } }
        public bool Started { get { return _started; } }
        public bool Enabled { get { return _addinRecord.Enabled; } }
        public AddinHeader Header { get { return _addinRecord.AddinHeader; } }
        public AddinRuntime Runtime { get { return _addinRuntime; } }
        public AddinFile File { get { return _addinFile; } }
        public AddinExtension Extension { get { return _addinExtension; } }

        internal void ThrowIfAddinIsDisabled()
        {
            if (_addinRecord.Enabled)
                return;

            if (_addinRecord.AddinHeader.Name.IsNullOrWhiteSpace())
                throw new InvalidOperationException(string.Format("The addin [{0}] is in disabled status!", _addinRecord.AddinId.Guid));
            else
                throw new InvalidOperationException(string.Format("The addin [{0}] [{1}] is in disabled status!", _addinRecord.AddinHeader.Name, _addinRecord.AddinId.Guid));
        }

        // 除了实现启动插件的目的之外，该方法还要避免插件启动 (Start) 顺序的依赖，即无论以任何顺序启动插件，都不会造成意外。
        // 此外，该方法必须确保多次调用不会出错。
        /// <summary>
        /// Starts the addin. After an addin has been loaded, you can:
        /// 1. extend extension points provided by other addins which has been already started, 
        /// 2. refers to the assemblies provided by this addin from another one, 
        /// 3. load extension points provided by this addin, 
        /// 4. request a type / resource or create an object from this addin, etc.
        /// An addin is started when：
        /// 1. this method is invoked explicitly.
        /// 2. the first extension point of this addin is loaded.
        /// 3. the first assembly of this addin gets loaded into runtime.
        /// 4. an extension point which is defined in another addin and extended by this addin gets loaded.
        /// </summary>
        /// <returns>true if the addin is enabled; otherwise false.</returns>
        public bool Start()
        {
            ThrowIfAddinIsDisabled();
            //if (!_addinRecord.Enabled)
            //{
            //    //_addinContext.Framework.MessageDialog.Show("The addin has been disabled, please enable it before you start it!", "Information");
            //    return false;
            //}

            if (_started)
                return true;

            if (!StartParentAddins())
                return false;

            return StartThisAddin();
        }

        bool StartParentAddins()
        {
            if (_parentAddins == null)
                return true;
            //if (!_addinContext.Framework.MessageDialog.Confirm
            //    ("The following addins that this addin denpends on has not started yet, if you start this addin, they will start too! Do you really want to start all these addins?", "Confirmation"))
            //    return false;
            return OperateOnParentAddins(addin => addin.Start());
        }

        bool StartThisAddin()
        {
            if (_started)
                return true;

            // after all referenced addins started, change the status of this addin, activate it, and load extensions for loaded extension points (if there is any).
            _started = true; // 必须在 ActivateThisAddin 方法之前设置此值，否则加载插件程序集时会再调用此方法，造成死循环。
            // register all assemblies of this addin to the assembly resolver, for getting them ready to be loaded into runtime, but not load them yet.
            _addinRuntime.RegisterAssemblies();

            try
            {
                OnAddinStatusChanged(AddinStatus.Starting);
                // activate the addin. this method might load needed assemblies.
                ActivateThisAddin();
                // if there is any loaded extension points for which this addin extends, loads the extension builders and extensions of this addin [addinRecord] 
                // that extending the extension point.
                _addinEngine.LoadIntoLoadedExtensionPoints(_addinContext, _addinRecord);
                OnAddinStatusChanged(AddinStatus.Started);
                return true;
            }
            catch (Exception e)
            {
                _started = false;
                _addinRuntime.UnregisterAssemblies();

                // log, show message, ...
                return false;
            }
        }

        void OnAddinStatusChanged(AddinStatus status)
        {
            var evt = StatusChanged;
            if (evt != null)
                evt.Invoke(this, new AddinStatusChangedEventArgs(status, this));
        }

        void ActivateThisAddin()
        {
            var activator = GetAddinActivator();
            if (activator != null)
                activator.Start(_addinContext);
        }

        // 该方法必须确保多次调用不会出错
        /// <summary>
        /// Stops the addin.
        /// </summary>
        public bool Stop()
        {
            ThrowIfAddinIsDisabled();
            if (!_started)
                return true;

            if (!StopChildAddins())
                return false;

            return StopThisAddin();
        }

        bool StopChildAddins()
        {
            if (_childAddins == null)
                return true;
            //List<Addin> unstoppedChildAddins;
            //if (!_addinEngine.CanStop(_addinRecord, out unstoppedChildAddins))
            //{
            //    _addinContext.Framework.MessageDialog.Show("The following addins that this addin denpends on has not started yet, please start them before you start this addin!", "Information");
            //    return false;
            //}
            return OperateOnChildAddins(addin => addin.Stop());
        }

        bool StopThisAddin()
        {
            if (!_started)
                return true;

            try
            {
                OnAddinStatusChanged(AddinStatus.Stopping);
                // if there is any extension points has been loaded for which this addin extends, unloads the extension builders and extensions of this addin [addinRecord] 
                // that extending the extension point.
                _addinEngine.UnloadFromLoadedExtensionPoints(_addinContext);
                // unload extension points of this addin, if it's loaded.
                _addinExtension.UnloadExtensionPoints();
                // deactivate the addin.
                DeactivateThisAddin();
                OnAddinStatusChanged(AddinStatus.Stopped);
                // unregister all assemblies of this addin from the assembly resolver. 
                _addinRuntime.UnregisterAssemblies();
                _started = false;

                return true;
            }
            catch (Exception e)
            {
                // log, show message, ...
                throw;
            }
        }

        void DeactivateThisAddin()
        {
            var activator = GetAddinActivator();
            if (activator != null)
                activator.Stop(_addinContext);
        }

        IAddinActivator GetAddinActivator()
        {
            if (_addinActivator != null)
                return _addinActivator;

            var activatorRecord = _addinRecord.AddinActivator;
            if (activatorRecord == null)
                return null;
            var activatorType = _addinRuntime.GetType(activatorRecord.AssemblyUid, activatorRecord.TypeName);
            _addinActivator = Activator.CreateInstance(activatorType) as IAddinActivator;

            return _addinActivator;
        }

        // 该方法必须确保多次调用不会出错
        /// <summary>
        /// Enable the addin, so that it can be started the next time.
        /// </summary>
        /// <returns></returns>
        public bool Enable()
        {
            if (_addinRecord.Enabled)
                return true;

            if (!EnableParentAddins())
                return false;

            EnableThisAddin();
            return true;
        }

        bool EnableParentAddins()
        {
            if (_parentAddins == null)
                return true;
            //if (!_addinContext.Framework.MessageDialog.Confirm
            //    ("The following addins that this addin denpends on has not started yet, if you start this addin, they will start too! Do you really want to start all these addins?", "Confirmation"))
            //    return false;
            return OperateOnParentAddins(addin => addin.Enable());
        }

        void EnableThisAddin()
        {
            _addinRecord.Enabled = true;
            // write to storage file 
            _addinEngine.UpdateStorageFile();
            StartThisAddin();
        }

        // 该方法必须确保多次调用不会出错
        /// <summary>
        /// Disable the addin, so that it can not be started the next time.
        /// </summary>
        /// <returns></returns>
        public bool Disable()
        {
            if (!_addinRecord.Enabled)
                return true;

            if (!DisableChildAddins())
                return false;

            DisableThisAddin();
            return true;
        }

        bool DisableChildAddins()
        {
            if (_childAddins == null)
                return true;
            //List<Addin> unstartedParentAddins;
            //if (!_addinEngine.CanDisable(_addinRecord, out unstartedParentAddins))
            //{
            //    _addinContext.Framework.MessageDialog.Show("The following addins that this addin denpends on has not started yet, please start them before you start this addin!", "Information");
            //    return false;
            //}
            return OperateOnChildAddins(addin => addin.Disable());
        }

        void DisableThisAddin()
        {
            StopThisAddin();
            _addinRecord.Enabled = false;
            _addinEngine.UpdateStorageFile();
        }

        bool OperateOnParentAddins(MyFunc<Addin, bool> func)
        {
            for (int i = _parentAddins.Count - 1; i >= 0; i--)
            {
                var parentAddin = _parentAddins[i];
                if (!func(parentAddin))
                    return false;
            }
            return true;
        }

        bool OperateOnChildAddins(MyFunc<Addin, bool> func)
        {
            for (int i = _childAddins.Count - 1; i >= 0; i--)
            {
                var childAddin = _childAddins[i];
                if (!func(childAddin))
                    return false;
            }
            return true;
        }
    }
}