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
            if (args.Length > 2)
            {
                configXDT = args[2];
            }

            BuildConfig config = JsonStorage.ReadFile<BuildConfig>(buildFolder + "\\build-config.json");
            config.SiteHost = siteRoot;
            int id = 0;
            if (int.TryParse(config.SiteHost, out id)) {
                config.SiteHost = null;
                config.SiteId = id;
            }
            config.BuildFolder = buildFolder;

            string log;
            string error;

            TextWriter oldWriter = Console.Out;
            using (StringWriter outWriter = new StringWriter())
            {
                Console.SetOut(outWriter);
                var oldError = Console.Error;
                using (StringWriter errorWriter = new StringWriter())
                {
                    Console.SetError(errorWriter);
                    Execute(config);
                    errorWriter.Flush();
                    Console.SetError(oldError);
                    error = errorWriter.ToString();
                }
                outWriter.Flush();
                Console.SetOut(oldWriter);
                log = outWriter.ToString();
            }

            var lastBuild = new LastBuild(){ 
                Time = DateTime.UtcNow,
                ExitCode =Environment.ExitCode,
                Log = log,
                Error = error
            };

            JsonStorage.WriteFile(lastBuild, buildFolder + "\\last-build.json");

            if (!string.IsNullOrWhiteSpace(log)) {
                Console.Out.Write(log);
            }
            if (!string.IsNullOrWhiteSpace(error)) {
                Console.Error.Write(error);
            }

        }

        private static void Execute(BuildConfig config)
        {
            try
            {

                string buildFolder = config.BuildFolder;

                var result = DownloadFilesAsync(config, buildFolder).Result;

                Console.WriteLine(result);

                if (config.UseMSBuild)
                {
                    string batchFileContents = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild ";
                    batchFileContents += "\"" + buildFolder + "\\source\\" + config.SolutionPath + "\"";
                    batchFileContents += " /t:Build ";
                    batchFileContents += " /p:Configuration=" + config.MSBuildConfig;

                    string batchFile = buildFolder + "\\msbuild.bat";

                    File.WriteAllText(batchFile, batchFileContents);


                    ProcessHelper.Execute(batchFile);


                    // transform...

                    XDTService.Instance.Process(config);

                    IISManager.Instance.DeployFiles(config);

                    Console.WriteLine("+++++++++++++++++++++ Deployment Successful !!! +++++++++++++++++++++");
                }

            }
            catch (Exception ex)
            {
                Environment.ExitCode = -1;
                Console.Error.WriteLine(ex.ToString());
                Console.Error.WriteLine("************************* Deployment failed ***************************");
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
                        return await SyncAsync(ctrl,config, rep);
                    }
                }
            }
            catch (Exception ex) {
                return ex.ToString();
            }
        }

        private static async Task<string> SyncAsync(ISourceController ctrl, BuildConfig config, LocalRepository rep)
        {
            ctrl.Initialize(config);


            try
            {
                List<ISourceItem> remoteItems = await ctrl.FetchAllFiles(config);
                var changes = rep.GetChanges(remoteItems);

                var updatedFiles = changes.Where(x => x.Type == ChangeType.Added || x.Type == ChangeType.Modified)
                    .Select(x => x.RepositoryFile).Where(x => !x.IsDirectory).ToList();

                foreach (var slice in updatedFiles.Slice(10))
                {
                    var downloadList = slice.Select(x => {
                        string filePath = rep.LocalFolder + x.Folder + "/" + x.Name;
                        System.IO.FileInfo finfo = new System.IO.FileInfo(filePath);
                        if (!finfo.Directory.Exists)
                        {
                            finfo.Directory.Create();
                        }
                        return ctrl.DownloadAsync(config, x, filePath); 
                    });

                    await Task.WhenAll(downloadList);
                    rep.UpdateFiles(slice);
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return null;
        }

        private static ISourceController GetController(BuildConfig config)
        {
            string sourceType = config.SourceType.ToLower();

            switch (sourceType)
            {
                case "tfs2012":
                    return new TFS2012Client();
                case "zipurl":
                    return new ZipSourceController();
                case "git":
                    return new Git.GitSourceController();
                default:
                    break;
            }

            throw new NotImplementedException("SourceControl does not exist for " + config.SourceType);
        }
    }
}
