using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IISCI.Web.Controllers
{
    [Authorize(Users="*")]
    public class IISController : Controller
    {

        public static string IISStore = null;

        ServerManager ServerManager;

        public IISController()
        {
            ServerManager = new ServerManager();

            if (IISStore == null) {
                IISStore = System.Web.Configuration.WebConfigurationManager.AppSettings["IISCI.Store"];
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing) {
                ServerManager.Dispose();
                ServerManager = null;
            }
        }

        public ActionResult Sites() {

            var sites = ServerManager.Sites.Select(x => new
            {
                ID = x.Id,
                Name = x.Name,
                Bindings = x.Bindings.Select(y => y.Host)
            }).ToList();

            return Json( sites, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Build(int id) {
            string buildPath = IISStore + "\\" + id;

            string commandLine = id + " \"" + buildPath + "\"" ;

            return new BuildActionResult(commandLine);


        }

        [HttpGet]
        public ActionResult BuildConfig(int id) {
            BuildConfig config = GetBuildConfig(id);
            return Json(config, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateBuildConfig(int id, BuildConfig model) {
            string path = IISStore + "\\" + id + "\\build-config.json";
            JsonStorage.WriteFile(model, path);
            return Json(model);
        }


        private IISCI.BuildConfig GetBuildConfig(int id)
        {
            BuildConfig config = null;
            string path = IISStore + "\\" + id + "\\build-config.json";
            if (System.IO.File.Exists(path))
            {
                config = JsonStorage.ReadFile<BuildConfig>(path);
            }
            else
            {
                config = new BuildConfig();
            }

            return config;
        }


    }
}