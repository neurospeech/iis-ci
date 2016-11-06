using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public static string GetMSBuildPath() {

            using (var msBuildKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\MSBuild")) {
                string version = msBuildKey.GetSubKeyNames().OrderByDescending(x => {
                    decimal v = 0;
                    decimal.TryParse(x,out v);
                    return v;
                }).FirstOrDefault();

                using (var v = msBuildKey.OpenSubKey(version)) {
                    string path = "\"" + (string)v.GetValue("MSBuildOverrideTasksPath", null);
                    if (path != null) {
                        path += "msbuild\"";
                        return path;
                    }
                }
            }
            return null;
        }

        public LastBuild Build() {
            FileInfo errorFile = new FileInfo(BuildFolder + "\\errors.txt");
            if (errorFile.Exists)
                errorFile.Delete();

            string batchFileContents = PackageRestore();
            batchFileContents += GetMSBuildPath() ?? @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild";
            batchFileContents += " ";
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
                

                int n = ProcessHelper.Execute(
                    batchFile, 
                    "",
                    BuildFolder
                    , o => sw.WriteLine(o), e => { });

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

        private string PackageRestore()
        {

            string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string nuget = folder + "\\Nuget.exe restore \"" + BuildFolder + "\\source\\"  + Solution + "\"";
            return nuget + "\r\n";
        }
    }

}
