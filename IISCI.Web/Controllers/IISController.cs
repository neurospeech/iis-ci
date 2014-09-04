using Microsoft.Web.Administration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IISCI.Web.Controllers
{
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

        [Authorize]
        public ActionResult Sites()
        {

            var sites = ServerManager.Sites.Select(x => new
            {
                Id = x.Id,
                Name = x.Name,
                Bindings = x.Bindings.Select(y => y.Host),
                LastBuild = JsonStorage.ReadFileOrDefault<LastBuild>(IISStore + "\\" + x.Id + "\\last-build.json")
            }).ToList();

            return Json( sites, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Build(int id)
        {
            string buildPath = IISStore + "\\" + id;

            string commandLine = id + " \"" + buildPath + "\"" ;

            return new BuildActionResult(commandLine);


        }

        [HttpGet]
        [Authorize]
        public ActionResult GetBuildConfig(int id)
        {
            string path = IISStore + "\\" + id + "\\build-config.json";
            BuildConfig config = JsonStorage.ReadFileOrDefault<BuildConfig>(path);
            if (string.IsNullOrWhiteSpace(config.TriggerKey))
            {
                config.TriggerKey = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                JsonStorage.WriteFile(config, path);
            }
            return Json(config, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult UpdateBuildConfig(int id)
        {
            string path = IISStore + "\\" + id + "\\build-config.json";

            string formValue = Request.Form["formModel"];

            var model = JsonConvert.DeserializeObject<BuildConfig>(formValue);

            JsonStorage.WriteFile(model, path);
            return Json(model);
        }

        public ActionResult BuildTrigger(int id, string key)
        {
            string path = IISStore + "\\" + id + "\\build-config.json";
            var model = JsonStorage.ReadFile<BuildConfig>(path);
            return Build(id);
        }
    }
}