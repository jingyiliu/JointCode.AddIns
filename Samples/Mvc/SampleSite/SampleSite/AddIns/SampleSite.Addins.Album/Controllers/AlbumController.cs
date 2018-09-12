using SampleSite.Addins.Album.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;
using JointCode.AddIns.Mvc;

namespace SampleSite.Addins.Album.Controllers
{
    public class AlbumController : Controller
    {
        public ActionResult Index()
        {
            var addinModels = new List<AppAddinModel>();
            var addins = JcMvc.AddinEngine.GetStartedAddins();

            foreach (var addin in addins)
            {
                AppAddinModel p;
                addinModels.Add(p = new AppAddinModel()
                {
                    Id = addin.Header.AddinId.Guid.ToString(),
                    Name = addin.Header.Name,
                    Description = addin.Header.Description,
                });

                var assemblies = addin.Runtime.LoadAssemblies();
                if (assemblies.Length == 0)
                    continue;

                AppAssembly apass = null;
                Assembly a = assemblies[0];
                DateTime lastModified = DateTime.Now;

                try
                {
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(a.Location);
                    lastModified = fileInfo.LastWriteTime;
                }
                catch
                {
                }

                apass = new AppAssembly()
                {
                    Name = a.ManifestModule.Name,
                    CodeBase = a.CodeBase,
                    Version = a.GetName().Version.ToString(),
                    Date = lastModified.ToString()
                };

                p.AssemblyInfo = apass;
            }

            //ViewData["AppAddinModel"] = addinModels;

            return View(addinModels);
        }
    }
}