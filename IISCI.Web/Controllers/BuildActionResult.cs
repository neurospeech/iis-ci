using IISCI.Web.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using static IISCI.Web.Controllers.IISController;

namespace IISCI.Web.Controllers
{
    public class BuildActionResult: ActionResult
    {
        string parameters;

        BuildConfig config;

        public BuildActionResult(BuildConfig config, string cmdLine, bool reset, string settingsPath)
        {
            parameters = cmdLine;
            this.config = config;
            this.reset = reset;
            this.settingsPath = settingsPath;
        }

        private static Dictionary<string,string> DeployInProgress = new Dictionary<string, string>();

        private static Dictionary<string, string> BuildInProgress = new Dictionary<string, string>();

        private bool reset;
        private string settingsPath;

        public override void ExecuteResult(ControllerContext context)
        {
            var HttpContext = context.HttpContext;
            var Request = HttpContext.Request;
            var Server = HttpContext.Server;
            var Response = HttpContext.Response.Output;

            try
            {
                lock (DeployInProgress)
                {
                    if (DeployInProgress.ContainsKey(config.SiteId))
                    {
                        Response.WriteLine("Deploy already in progress");
                        Response.Flush();
                        return;
                    }
                    DeployInProgress[config.SiteId] = config.SiteId;
                }

                lock (BuildInProgress) {
                    if (BuildInProgress.ContainsKey(config.BuildFolder)) {
                        Response.WriteLine("MSBuild already in progress, retry after sometime");
                        Response.Flush();

                        return;
                    }
                    BuildInProgress[config.BuildFolder] = config.BuildFolder;
                }


                if (reset)
                {
                    /*var file = new System.IO.FileInfo(config.BuildFolder + "\\local-repository.json");
                    if (file.Exists)
                    {
                        file.Delete();
                    }*/
                    var dir = new System.IO.DirectoryInfo(config.BuildFolder);
                    if (dir.Exists) {
                        dir.Delete(true);
                    }
                }

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

                    /*using (SmtpClient smtp = new SmtpClient()) {

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
                    }*/

                    string subject = string.Format("IISCI-Build: {0} for {1}", (p.Success ? "Success" : "Failed"), config.SiteId);
                    string body = "<div><h2>" + config.SiteId + "</h2>" + p.Error + p.Output + "</div><hr size='1'/><div style='text-align:right'><a href='https://github.com/neurospeech/iis-ci' target='_blank'>IISCI by NeuroSpeech&reg;</a></div>";
                    List<string> recipients = new List<string>();
                    foreach (var item in config.Notify.Split(',', ';').Where(x => !string.IsNullOrWhiteSpace(x)))
                    {
                        if (!item.Contains('@'))
                            continue;
                        recipients.Add(item);
                    }


                    SettingsModel settings = JsonStorage.ReadFileOrDefault<SettingsModel>(settingsPath);

                    SmtpService.Instance.Send(settings, subject, body, recipients);


                }
                catch (Exception ex) {
                    Response.WriteLine(ex.ToString());
                }

            }
            finally {
                lock (DeployInProgress)
                {
                    DeployInProgress.Remove(config.SiteId);
                }
                lock (BuildInProgress) {
                    BuildInProgress.Remove(config.BuildFolder);
                }
            }

        }

    }
};