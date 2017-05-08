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

    public class SettingsModel
    {
        public string SMTPHost { get; set; }
        public int SMTPPort { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; } = "IISCI Build";
        public string Username { get; set; }
        public string Password { get; set; }
        public bool SSL { get; set; }
    }

    public class IISController : BaseController
    {



        public IISController()
        {
        }



        public string SettingsPath {
            get {
                return IISStore + "\\settings.json";
            }
        }

        [Authorize]
        public ActionResult Settings() {
            return Json( JsonStorage.ReadFileOrDefault<SettingsModel>(SettingsPath), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult UpdateSettings(SettingsModel model) {

            JsonStorage.WriteFile(model, SettingsPath);
            return Content("\"Ok\"");
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
                LastBuild = JsonStorage.ReadFileOrDefault<LastBuild>(GetBuildConfigModel(x.Name).BuildResult)
            }).ToList().OrderBy(x=>x.Id);

            return Json( sites, JsonRequestBehavior.AllowGet);
        }

        private string GetConfigPath(string id) {
            return IISStore + "\\config\\" + id + ".json";
        }

        [Authorize]
        public ActionResult Build(string id, bool redeploy= false, bool reset = false, string key = null)
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
            if (redeploy) {
                commandLine += " redeploy=true";
            }


            return new BuildActionResult(model,commandLine, reset, SettingsPath);


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
        public ActionResult UpdateBuildConfig(string id, BuildConfig model)
        {
            string path = GetConfigPath(id);

            SaveConfig(id, path, model, false);

            string lastBuildFile = model.BuildResult;
            var lastBuild = JsonStorage.ReadFileOrDefault<LastBuild>(lastBuildFile);
            if (lastBuild == null)
            {
                lastBuild = new LastBuild();
            }
            lastBuild.Error = "Config changed";
            lastBuild.LastResult = null;
            JsonStorage.WriteFile(lastBuild, lastBuildFile);
            return Json(model);
        }

        private void SaveConfig(string id, string path, BuildConfig model, bool onlyIfModified = true)
        {
            bool modified = false;
            if (model.SiteId != id)
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
            model.BuildFolder = IISStore + "\\sites\\" + BuildSourceMaps.Instance.Get(model.BuildSourceKey).Id;
            if (bf != model.BuildFolder) {
                modified = true;
            }

            string br = model.BuildResult;
            model.BuildResult = IISStore + "\\result\\" + id + ".json";

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
            return Build(id,true,reset,key);
        }
    }


    public class BuildSourceMap {
        public string Id { get; set; }
        public string SourceKey { get; set; }
    }
}