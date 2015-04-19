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

        static bool HasChanges = true;

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

            if (HasChanges)
            {
                var lastBuild = new LastBuild()
                {
                    Time = DateTime.UtcNow,
                    ExitCode = Environment.ExitCode,
                    Log = log,
                    Error = error
                };

                JsonStorage.WriteFile(lastBuild, buildFolder + "\\last-build.json");
            }

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

                if (result == 0) {
                    var lb = JsonStorage.ReadFileOrDefault<LastBuild>(buildFolder + "\\last-build.json");
                    if (lb == null || string.IsNullOrWhiteSpace(lb.Error))
                    {
                        Console.WriteLine("+++++++++++++++++++++ No changes to deploy +++++++++++++++++++++");
                        HasChanges = false;
                        return;
                    }
                }

                if (config.UseMSBuild)
                {
                    string errorLog = buildFolder + "\\errors.txt";
                    if (File.Exists(errorLog)) {
                        File.Delete(errorLog);
                    }

                    string batchFileContents = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild ";
                    batchFileContents += "\"" + buildFolder + "\\source\\" + config.SolutionPath + "\"";
                    batchFileContents += " /t:Build ";
                    batchFileContents += " /p:Configuration=" + config.MSBuildConfig;
                    batchFileContents += " /flp1:logfile=errors.txt;errorsonly";
                    if (!string.IsNullOrWhiteSpace(config.MSBuildParameters))
                    {
                        batchFileContents += " " + config.MSBuildParameters;
                    }

                    string batchFile = buildFolder + "\\msbuild.bat";

                    File.WriteAllText(batchFile, batchFileContents);


                    int n = ProcessHelper.Execute(batchFile, "", o => Console.WriteLine(o), e => {  });
                    if (n != 0) {
                        string error = File.Exists(errorLog) ? File.ReadAllText(errorLog) : "";
                        throw new InvalidOperationException(error);
                    }

                    // transform...

                    string webConfig = XDTService.Instance.Process(config);

                    IISManager.Instance.DeployFiles(config,webConfig);

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

        static async Task<int> DownloadFilesAsync(BuildConfig config, string buildFolder)
        {
            using (ISourceController ctrl = GetController(config))
            {
                using (LocalRepository rep = new LocalRepository(buildFolder))
                {
                    ctrl.Initialize(config);

                    List<ISourceItem> remoteItems = await ctrl.FetchAllFiles(config);
                    var changes = rep.GetChanges(remoteItems).ToList();

                    var changeTypes = changes.GroupBy(x => x.Type);
                    foreach (var item in changeTypes)
                    {
                        Console.WriteLine("Changes {0}: {1}",item.Key, item.Count());
                    }

                    var updatedFiles = changes.Where(x => x.Type == ChangeType.Added || x.Type == ChangeType.Modified)
                        .Select(x => x.RepositoryFile).Where(x => !x.IsDirectory).ToList();

                    foreach (var item in changes.Where(x => x.Type == ChangeType.Removed))
                    {
                        string filePath = item.RepositoryFile.FilePath;
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }

                    foreach (var slice in updatedFiles.Slice(25))
                    {
                        var downloadList = slice.Select(x =>
                        {
                            string filePath = rep.LocalFolder + x.Folder + "/" + x.Name;
                            System.IO.FileInfo finfo = new System.IO.FileInfo(filePath);
                            if (finfo.Exists)
                            {
                                finfo.Delete();
                            }
                            else
                            {
                                if (!finfo.Directory.Exists)
                                {
                                    finfo.Directory.Create();
                                }
                            }
                            x.FilePath = filePath;
                            return ctrl.DownloadAsync(config, x, filePath);
                        });

                        await Task.WhenAll(downloadList);
                        rep.UpdateFiles(slice);
                    }

                    return updatedFiles.Count();
                }
            }
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
