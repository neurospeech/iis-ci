using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IISCI.Web.Controllers
{
    public abstract class BaseController : Controller
    {

        protected ServerManager ServerManager {get; private set;}

        public static string IISStore = null;


        public BaseController()
        {

            ServerManager = new ServerManager();

            if (IISStore == null)
            {
                IISStore = System.Web.Configuration.WebConfigurationManager.AppSettings["IISCI.Store"];
            }

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                ServerManager.Dispose();
                ServerManager = null;
            }
        }
    }
}