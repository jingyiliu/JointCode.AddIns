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
     //插件激活器，它是一个插件启动和停止的入口。当插件被启动时，激活器的 Start 方法会被调用；如果是被停止，则其 Stop 方法会被调用。
     //一般而言，一个插件会在 Start 方法中向系统提供功能、注册服务、申请资源（例如线程或内存空间...）等，在 Stop 方法会执行回收操作，
     //比如关闭功能、卸载服务、释放资源等。
     //需要注意的是，在 Start 方法中申请的资源必须在 Stop 方法中得到释放，而且一个插件的 Start/Stop 方法在运行过程可能会被调用多次，
     //必须确保再次调用 Start/Stop 方法不会出现异常。
    /// <summary>
    /// The addin activator. This is the only entry to the addin.
    /// </summary>
    public interface IAddinActivator
	{
	    void Start(IAddinContext context);
	    void Stop(IAddinContext context);
	}
}
