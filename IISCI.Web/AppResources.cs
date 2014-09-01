using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IISCI.Web
{
    public class AppResources
    {

        private static HtmlResource jQuery = HtmlResource.CreateScript("/scripts/jquery-1.11.1.js");

        private static HtmlResource AtomsStyle = HtmlResource.CreateStyleSheet("/style/atoms/atoms.css");
        public static HtmlResource AtomsScript = HtmlResource.CreateScript("/scripts/atoms-debug.js", AtomsStyle, jQuery);
        private static HtmlResource AppScript = HtmlResource.CreateScript("/scripts/app.js", AtomsScript);

    }
}