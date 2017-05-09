using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace IISCI.Web.Controllers
{
    public abstract class BackgroundTaskActionResult : ActionResult
    {

        public BackgroundTaskActionResult()
        {
        }

        private object lockObject = new object();

        private bool finished = false;

        private StringWriter logger = new StringWriter();

        public sealed override void ExecuteResult(ControllerContext context)
        {
            var response = context.HttpContext.Response;
            string rootPath = context.HttpContext.Server.MapPath("/");
            ThreadPool.QueueUserWorkItem((a) => {

                OnExecute( rootPath , logger);

                lock (lockObject) {
                    finished = true;
                }
            });

            while (true)
            {
                Thread.Sleep(1000);
                lock (lockObject)
                {
                    if (finished) {
                        response.Write(logger.GetStringBuilder().ToString());
                        response.Flush();
                        return;
                    }
                }

                response.Write(" ");
                response.Flush();
            }
        }

        protected abstract void OnExecute(string rootPath, StringWriter logger);
    }
}