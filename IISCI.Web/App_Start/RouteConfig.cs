using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace IISCI.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");


            routes.MapRoute(
                name: "Files",
                url: "files/{action}/{id}/{*path}",
                defaults: new { controller = "Files", action = "Raw" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}/{key}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, key = UrlParameter.Optional }
            );



        }
    }

}