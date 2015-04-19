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
                            output.WriteLine("<div>{0}</div>", HttpUtility.HtmlEncode(o));
                        },
                                                o =>
                        {
                            error.WriteLine("<div>{0}</div>", HttpUtility.HtmlEncode(o));
                        });

                    Output = "<div>" + output.GetStringBuilder().ToString() + "</div>";
                    string er = error.GetStringBuilder().ToString().Trim();
                    if (er.Length > 0)
                    {
                        Error = "<div style='color:red'>" + er + "</div>";
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