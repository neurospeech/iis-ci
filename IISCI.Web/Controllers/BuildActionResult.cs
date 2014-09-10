using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace IISCI.Web.Controllers
{
    public class BuildActionResult: ActionResult
    {
        string parameters;

        public BuildActionResult(string cmdLine)
        {
            parameters = cmdLine;
        }

        private static List<string> BuildInProgress = new List<string>();

        public override void ExecuteResult(ControllerContext context)
        {
            var HttpContext = context.HttpContext;
            var Request = HttpContext.Request;
            var Server = HttpContext.Server;
            var Response = HttpContext.Response.Output;

            lock (BuildInProgress)
            {
                if (BuildInProgress.Contains(parameters))
                {
                    Response.WriteLine("Build already in progress");
                    Response.Flush();
                    return;
                }
                BuildInProgress.Add(parameters);
            }

            try
            {

                var executable = Server.MapPath("/") + "\\bin\\IISCI.build.exe";

                Response.WriteLine("<html><script type='text/javascript'>");
                Response.WriteLine("function log(txt,error){");
                Response.WriteLine("var line = document.createElement('PRE');");
                Response.WriteLine("line.textContent = txt;");
                Response.WriteLine("if(error) { line.style.color= 'red'; }");
                Response.WriteLine("document.getElementById('logger').appendChild(line);");
                Response.WriteLine("setTimeout( function(){  line.scrollIntoView(); }, 100);");
                Response.WriteLine("}");
                Response.WriteLine("</script><body><div id='logger'>");

                Response.Flush();

                Log(Response,"Executing command " + executable + " " + parameters);

                int n = ProcessHelper.Execute(
                    executable,
                    parameters,
                    s =>
                    {
                        Log(Response, s, false);
                    },
                    s =>
                    {
                        Log(Response, s, true);
                    });
            }
            finally {
                lock (BuildInProgress)
                {
                    BuildInProgress.Remove(parameters);
                }
            }

        }

        private void Log(TextWriter writer, string text, bool error = false)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            text = js.Serialize(text);
            writer.WriteLine("<script type='text/javascript'>log(" + text + "," + (error ? "true": "false") + ")</script>");
            writer.Flush();
        }
    }
}