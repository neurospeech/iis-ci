using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TFSRestAPI;

namespace IISCI.Web.Controllers
{
    public class TFSController : Controller
    {

        TFS2012Client client = null;

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            client = new TFS2012Client("", "", "", "");
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

    }
}
