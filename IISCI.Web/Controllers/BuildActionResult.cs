using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public override void ExecuteResult(ControllerContext context)
        {
            var HttpContext = context.HttpContext;
            var Request = HttpContext.Request;
            var Server = HttpContext.Server;
            var Response = HttpContext.Response.Output;

            var executable = Server.MapPath("/") + "\\bin\\IISCI.build.exe";

            var p = Process.GetProcesses().FirstOrDefault(x => string.Equals(x.StartInfo.FileName, executable, StringComparison.OrdinalIgnoreCase));
            if (p != null) {
                Response.WriteLine("Deployment already in progress, try after sometime.");
                return;
            }
            Response.WriteLine("<html><script type='text/javascript'>");
            Response.WriteLine("function log(txt,error){");
            Response.WriteLine("var line = document.createElement('PRE');");
            Response.WriteLine("line.textContent = txt;");
            Response.WriteLine("if(error) { line.style.color= 'red'; }");
            Response.WriteLine("document.getElementById('logger').appendChild(line);");
            Response.WriteLine("}");
            Response.WriteLine("</script><body><div id='logger'>");

            int n = ProcessHelper.Execute(
                executable,
                parameters,
                s =>
                {
                    Response.WriteLine(Log(s,false));
                    Response.Flush();
                },
                s =>
                {
                    Response.WriteLine(Log(s,true));
                    Response.Flush();
                });

        }

        private string Log(string text, bool error)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            text = js.Serialize(text);
            return "<script type='text/javascript'>log(" + text + "," + (error ? "true": "false") + ")</script>";
        }
    }
}