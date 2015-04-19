using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace IISCI.Web.Controllers
{
    public class IISCIProcess
    {

        private string program;
        private string arguments;

        public string Output { get; private set; }
        public string Error { get; private set; }

        public IISCIProcess(string prog, string args)
        {
            program = prog;
            arguments = args;

        }

        public int Run()
        {

            using (StringWriter output = new StringWriter())
            {
                using (StringWriter error = new StringWriter())
                {

                    int n = ProcessHelper.Execute(program, arguments,
                        o =>
                        {
                            output.WriteLine(HttpUtility.HtmlEncode(o));
                        },
                                                o =>
                        {
                            error.WriteLine(HttpUtility.HtmlEncode(o));
                        });

                    Output = "<pre>" + output.GetStringBuilder().ToString() + "</pre>";
                    string er = error.GetStringBuilder().ToString().Trim();
                    if (er.Length > 0)
                    {
                        Error = "<pre style='color:red'>" + er + "</pre>";
                    }
                    else {
                        Error = "";
                    }

                    return n;
                }
            }

        }



    }
}