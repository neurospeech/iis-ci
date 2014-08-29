using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TFSRestAPI;

namespace IISCI.Web.Controllers
{
    public class TFSController : AppController
    {

        TFS2012Client client = null;

        IISCI.Web.Models.IISCISite Site;

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            TFSPull pull = Site.LoadConfig<TFSPull>();

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            client.Dispose();
        }

        public async Task<ActionResult> Projects() {
            var result = await client.GetCollections();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Source(string collection, string path) {
            var result = await client.GetSourceItems(collection, path);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Settings() { 
            return Json(Site.LoadConfig<TFSPull>(), JsonRequestBehavior.AllowGet );
        }

        public ActionResult SaveSettings(TFSPull pull)
        {
            Site.SaveConfig(pull);
            return Json("Ok");
        }

    }

    public class TFSPull {

        public TFSPull()
        {
            Port = 443;
            Secure = true;
        }

        

        public string Domain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool Secure { get; set; }
        public string Collection { get; set; }
        public string Project { get; set; }
        public string Path { get; set; }
        public bool Build { get; set; }
    }
}
