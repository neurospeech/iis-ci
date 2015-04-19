using Microsoft.Web.Administration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace IISCI.Web.Controllers
{
    public class IISController : BaseController
    {



        public IISController()
        {
        }


        [Authorize]
        public ActionResult Sites()
        {

            var sites = ServerManager.Sites.Select(x => new
            {
                Id = x.Name,
                IISID = x.Id,
                Name = x.Name,
                Bindings = x.Bindings.Select(y => y.Host),
                LastBuild = JsonStorage.ReadFileOrDefault<LastBuild>(IISStore + "\\" + x.Name + "\\last-build.json")
            }).ToList();

            return Json( sites, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Build(string id, bool reset = false, string key = null)
        {
            string buildPath = IISStore + "\\" + id;

            string commandLine = id + " \"" + buildPath + "\"" ;

            string path = IISStore + "\\" + id + "\\build-config.json";
            var model = JsonStorage.ReadFile<BuildConfig>(path);
            if (!string.IsNullOrWhiteSpace(key))
            {
                if (model.TriggerKey != key)
                    throw new UnauthorizedAccessException();
            }


            if (reset) {
                var file = new System.IO.FileInfo(buildPath + "\\local-repository.json");
                if (file.Exists) {
                    file.Delete();
                }
            }

            return new BuildActionResult(model,commandLine);


        }

        [HttpGet]
        [Authorize]
        public ActionResult GetBuildConfig(string id)
        {
            string path = IISStore + "\\" + id + "\\build-config.json";
            BuildConfig config = JsonStorage.ReadFileOrDefault<BuildConfig>(path);
            if (string.IsNullOrWhiteSpace(config.TriggerKey))
            {
                config.TriggerKey = CreateBuildKey();
                JsonStorage.WriteFile(config, path);
            }
            return Json(config, JsonRequestBehavior.AllowGet);
        }

        private string CreateBuildKey() {
            var key = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) 
                + Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                + Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            return key.Replace("=", "").Replace("+", "").Replace("\\", "").Replace("/", "");
        }

        [Authorize]
        [ValidateInput(false)]
        public ActionResult UpdateBuildConfig(string id)
        {
            string path = IISStore + "\\" + id + "\\build-config.json";

            string formValue = Request.Form["formModel"];

            var model = JsonConvert.DeserializeObject<BuildConfig>(formValue);

            JsonStorage.WriteFile(model, path);

            string lastBuildFile = IISStore + "\\" + id + "\\last-build.json";
            var lastBuild = JsonStorage.ReadFileOrDefault<LastBuild>(lastBuildFile);
            if (lastBuild == null) {
                lastBuild = new LastBuild();
            }
            lastBuild.Error = "Config changed";
            JsonStorage.WriteFile(lastBuild, lastBuildFile);
            return Json(model);
        }

        [Authorize]
        public ActionResult GenerateBuildKey(){
            return Json(CreateBuildKey(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult BuildTrigger(string id, string key, bool reset = false, bool async = true)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new UnauthorizedAccessException();
            if (async)
            {
                UriBuilder uri = new UriBuilder(Request.Url);
                if (string.IsNullOrWhiteSpace(uri.Query))
                {
                    uri.Query = "async=false";
                }
                else
                {
                    uri.Query += "&async=false";
                }
                IISWebRequest.Instance.Invoke(uri.Uri.ToString());
                return Content("Request queued");
            }
            return Build(id,reset,key);
        }
    }



}