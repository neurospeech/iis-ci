using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IISCI.Web.Controllers
{
    public abstract class BaseController : Controller
    {

        protected ServerManager ServerManager {get; private set;}

        public string IISStore {
            get{
                var f = new DirectoryInfo(Server.MapPath("/"));
                return f.Parent.FullName + "\\store";
            }
        }


        public BaseController()
        {

            ServerManager = new ServerManager();

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