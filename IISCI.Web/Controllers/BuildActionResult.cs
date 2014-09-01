using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IISCI.Web.Controllers
{
    public class BuildActionResult: ActionResult
    {
        string parameters;

        public BuildActionResult(string cmdLine)
        {
            parameters = cmdLine;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var HttpContext = context.HttpContext;
            var Request = HttpContext.Request;
            var Server = HttpContext.Server;
            var Response = HttpContext.Response.Output;

            var executable = Server.MapPath("/") + "\\bin\\IISCI.build.exe";

            int n = ProcessHelper.Execute(
                executable, 
                parameters, 
                s => Response.WriteLine(s), 
                s => Response.WriteLine(s));

        }
    }
}