using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace IISCI.Web.Controllers
{
    public class BuildActionResult: ActionResult
    {
        string parameters;

        BuildConfig config;

        public BuildActionResult(BuildConfig config, string cmdLine)
        {
            parameters = cmdLine;
            this.config = config;
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

                Response.WriteLine("<html>");
                Response.WriteLine("<body><div id='logger'>");

                Response.Flush();

                IISCIProcess p = new IISCIProcess(executable, parameters);
                p.Run();

                Response.WriteLine(p.Error);
                Response.Flush();
                Response.WriteLine(p.Output);
                Response.Flush();


                if (p.Success) {
                    if (config.StartUrls!=null) {
                        foreach (var url in config.StartUrls)
                        {
                            IISWebRequest.Instance.Invoke(config.SiteId, url.Url);
                        }
                    }
                }

                if(string.IsNullOrWhiteSpace(config.Notify))
                    return;

                try {

                    using (SmtpClient smtp = new SmtpClient()) {

                        MailMessage msg = new MailMessage();
                        msg.From = new MailAddress("no-reply@800casting.com","IISCI Build");
                        foreach (var item in config.Notify.Split(',',';').Where(x=> !string.IsNullOrWhiteSpace(x)))
                        {
                            if (!item.Contains('@'))
                                continue;
                            msg.To.Add(new MailAddress(item));
                        }
                        msg.Subject = string.Format("IISCI-Build: {0} for {1}", (p.Success ? "Success" : "Failed" ) , config.SiteId) ;
                        msg.IsBodyHtml = true;
                        msg.Body = "<div><h2>" + config.SiteId + "</h2>" + p.Error + p.Output + "</div><hr size='1'/><div style='text-align:right'><a href='https://github.com/neurospeech/iis-ci' target='_blank'>IISCI by NeuroSpeech&reg;</a></div>";

                        smtp.Send(msg);
                    }


                }
                catch (Exception ex) {
                    Response.WriteLine(ex.ToString());
                }

            }
            finally {
                lock (BuildInProgress)
                {
                    BuildInProgress.Remove(parameters);
                }
            }

        }

    }
};