using IISCI.Web.Models;
using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IISCI.Web.Controllers
{
    public class AppController : Controller
    {

        #region Site
        public ServerManager IISServer { get; private set; }

        public Microsoft.Web.Administration.Site IISSite { get; private set; }

        public AppController()
        {
            IISServer = new ServerManager();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (IISServer != null)
                IISServer.Dispose();
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {

            base.Initialize(requestContext);

            string host = Request.Url.Host;

            IISSite = IISServer.Sites.First(x => x.Bindings.Any(y => y.Host != null && string.Equals(y.Host, host, StringComparison.CurrentCultureIgnoreCase)));

        } 
        #endregion

        public ActionResult Index() {

            this.Register(HtmlResource.CreateScriptModel("model", new { Site = IISSite.Name, Bindings = IISSite.Bindings.Select(x => new { 
                x.Host,
                x.Protocol
            }) }));
            
            return View();
        } 

        public ActionResult VirtualDirectories() {

            return Json(IISSite.Applications.Select(x => new { 
                x.Path
            }), JsonRequestBehavior.AllowGet);
        }

    }
}