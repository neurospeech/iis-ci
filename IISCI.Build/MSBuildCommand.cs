using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISCI.Build
{
    public class MSBuildCommand
    {

        public string BuildFolder { get; set; }

        public string Solution { get; set; }

        public string BuildConfig { get; set; }

        public string Parameters { get; set; }

        public MSBuildCommand()
        {
                
        }

        public LastBuild Build() {
            FileInfo errorFile = new FileInfo(BuildFolder + "\\errors.txt");
            if (errorFile.Exists)
                errorFile.Delete();

            string batchFileContents = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild ";
            batchFileContents += "\"" + BuildFolder + "\\source\\" + Solution + "\"";
            batchFileContents += " /t:Build ";
            batchFileContents += " /p:Configuration=" + BuildConfig;
            batchFileContents += " /flp1:logfile=errors.txt;errorsonly";
            if (!string.IsNullOrWhiteSpace(Parameters))
            {
                batchFileContents += " " + Parameters;
            }

            string batchFile = BuildFolder + "\\msbuild.bat";

            File.WriteAllText(batchFile, batchFileContents);

            using (StringWriter sw = new StringWriter())
            {
                int n = ProcessHelper.Execute(batchFile, "", o => sw.WriteLine(o), e => { });

                string error = null;

                if (n != 0)
                {
                    error = errorFile.Exists ? File.ReadAllText(errorFile.FullName) : "";
                }

                return new LastBuild { 
                    Error = error,
                    Log = sw.GetStringBuilder().ToString(),
                    ExitCode = n,
                    Time = DateTime.UtcNow
                };
            }

       }

    }

}
