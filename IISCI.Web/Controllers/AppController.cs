using IISCI.Web.Models;
using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IISCI.Web.Controllers
{
    public abstract class AppController : Controller
    {

        private static Dictionary<string, IISCISite> SiteCache = new Dictionary<string, IISCISite>();
        private IISCISite LoadSite(string host) { 
            IISCISite s = null;
            lock (SiteCache)
            {
                if (SiteCache.TryGetValue(host, out s))
                    return s;
                string path = null;
                using (ServerManager svr = new ServerManager())
                {
                    var site = svr.Sites.First(x => x.Bindings.Any(y => y.Host != null && string.Equals(y.Host, host, StringComparison.CurrentCultureIgnoreCase)));
                    var app = site.Applications.First();
                    var vir = app.VirtualDirectories.First();
                    path = vir.PhysicalPath;
                }
                s = new IISCISite(MvcApplication.StoreFolder.FullName + "\\" + host + "\\", path);
                SiteCache[host] = s;
            }
            return s;
        }

        public IISCISite Site { get; private set; }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            
            base.Initialize(requestContext);

            string host = Request.Url.Host;


            Site = LoadSite(host);



        }

    }
}