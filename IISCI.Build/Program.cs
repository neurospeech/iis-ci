using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFSRestAPI;

namespace IISCI.Build
{
    class Program
    {
        static void Main(string[] args)
        {

            string siteRoot = args[0];

            string buildFolder = args[1];

            string configXDT = null;
            if (args.Length > 2) {
                configXDT = args[2];
            }

            BuildConfig config = JsonStorage.ReadFile<BuildConfig>(buildFolder + "\\build-config.json");

            string buildLog = buildFolder + "\\build-log.txt";

            var result = DownloadFilesAsync(config, buildFolder).Result;

            Console.WriteLine(result);

            if (!string.IsNullOrWhiteSpace(result))
            {
                File.WriteAllText(buildLog, result);
            }

            if (config.UseMSBuild) {
                string batchFileContents = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild ";
                batchFileContents +=  "\"" + buildFolder + "\\source\\" + config.SolutionPath + "\"";
                batchFileContents += " /t:Build ";
                batchFileContents += " /p:Configuration=" + config.MSBuildConfig;
                batchFileContents += " > build-log.txt";

                string batchFile = buildFolder + "\\msbuild.bat";

                File.WriteAllText(batchFile, batchFileContents);

                using (StringWriter logWriter = new StringWriter()) {
                    using (StringWriter errorWriter = new StringWriter()) {
                        ProcessHelper.Execute(batchFile, "", logWriter, errorWriter);

                        result = errorWriter.ToString();
                    }
                    result += logWriter.ToString();
                }

                Console.WriteLine(result);
                File.AppendAllText(buildLog,result);
            }

        }

        static async Task<string> DownloadFilesAsync(BuildConfig config, string buildFolder)
        {
            try
            {
                using (ISourceController ctrl = GetController(config))
                {
                    using (LocalRepository rep = new LocalRepository(buildFolder))
                    {
                        return await ctrl.SyncAsync(config, rep);
                    }
                }
            }
            catch (Exception ex) {
                return ex.ToString();
            }
        }

        private static ISourceController GetController(BuildConfig config)
        {
            string sourceType = config.SourceType.ToLower();

            switch (sourceType)
            {
                case "tfs2012":
                    return new TFS2012Client();
                default:
                    break;
            }

            throw new NotImplementedException("SourceControl does not exist for " + config.SourceType);
        }
    }
}
