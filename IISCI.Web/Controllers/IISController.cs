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
                LastBuild = JsonStorage.ReadFileOrDefault<LastBuild>(GetBuildConfigModel(x.Name).BuildFolder + "\\last-build.json")
            }).ToList();

            return Json( sites, JsonRequestBehavior.AllowGet);
        }

        private string GetConfigPath(string id) {
            return IISStore + "\\config\\" + id + ".json";
        }

        [Authorize]
        public ActionResult Build(string id, bool reset = false, string key = null)
        {


            string configPath = GetConfigPath(id);


            var model = JsonStorage.ReadFile<BuildConfig>(configPath);


            if (!string.IsNullOrWhiteSpace(key))
            {
                if (model.TriggerKey != key)
                    throw new UnauthorizedAccessException();
            }

            SaveConfig(id, configPath, model);

            string buildPath = model.BuildFolder;
            string commandLine = "id=" + id + " config=\"" + configPath + "\" build=\"" + buildPath + "\"";

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
            BuildConfig config = GetBuildConfigModel(id);
            return Json(config, JsonRequestBehavior.AllowGet);
        }

        private BuildConfig GetBuildConfigModel(string id)
        {
            string path = GetConfigPath(id);
            BuildConfig config = JsonStorage.ReadFileOrDefault<BuildConfig>(path);
            SaveConfig(id, path, config);
            return config;
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
            string path = GetConfigPath(id);

            string formValue = Request.Form["formModel"];

            var model = JsonConvert.DeserializeObject<BuildConfig>(formValue);
            SaveConfig(id, path, model, false);

            string lastBuildFile = model.BuildFolder + "\\last-build.json";
            var lastBuild = JsonStorage.ReadFileOrDefault<LastBuild>(lastBuildFile);
            if (lastBuild == null)
            {
                lastBuild = new LastBuild();
            }
            lastBuild.Error = "Config changed";
            JsonStorage.WriteFile(lastBuild, lastBuildFile);
            return Json(model);
        }

        private void SaveConfig(string id, string path, BuildConfig model, bool onlyIfModified = true)
        {
            bool modified = false;
            if (string.IsNullOrWhiteSpace(model.SiteId))
            {
                model.SiteId = id;
                modified = true;
            }
            if (string.IsNullOrWhiteSpace(model.TriggerKey))
            {
                model.TriggerKey = CreateBuildKey();
                modified = true;
            }

            string bf = model.BuildFolder;
            model.BuildFolder = IISStore + "\\" + BuildSourceMaps.Instance.Get(model.BuildSourceKey).Id;
            if (bf != model.BuildFolder) {
                modified = true;
            }

            if (onlyIfModified)
            {
                if (!modified)
                    return;
            }
            JsonStorage.WriteFile(model, path);
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


    public class BuildSourceMap {
        public string Id { get; set; }
        public string SourceKey { get; set; }
    }
}