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

            var dictionary = args
                .Select(x => x.Split(new char[] { '=' }, 2).Select(s => s.Trim()))
                .Where(x => x.Count() == 2)
                .ToDictionary(x => x.FirstOrDefault(), x => x.LastOrDefault());



            string id = dictionary["id"];
            string configPath = dictionary["config"].Trim('"');
            string buildFolder = dictionary["build"].Trim('"');

            bool redeploy = false;
            string v;
            if (dictionary.TryGetValue("redeploy", out v)) {
                redeploy = v?.Equals("yes", StringComparison.OrdinalIgnoreCase) ?? false;
            }



            string configXDT = null;
            if (args.Length > 2)
            {
                configXDT = args[2];
            }

            BuildConfig config = JsonStorage.ReadFile<BuildConfig>(configPath);
            
            config.BuildFolder = buildFolder;

            var lastBuild = Execute(config, redeploy);
            JsonStorage.WriteFile(lastBuild, config.BuildResult);

            if (lastBuild.Success)
            {
                Console.WriteLine(lastBuild.Log);
            }
            else {
                Console.Error.WriteLine(lastBuild.Error ?? "Something went wrong...");
                Environment.ExitCode = lastBuild.ExitCode;
            }

        }

        private static LastBuild Execute(BuildConfig config, bool redeploy = false)
        {
            try
            {

                string buildFolder = config.BuildFolder;

                var result = DownloadFilesAsync(config, buildFolder).Result;

                if (!redeploy)
                {
                    var lb = JsonStorage.ReadFileOrDefault<LastBuild>(config.BuildResult);
                    if (lb != null && lb.LastResult == result.LastVersion && string.IsNullOrWhiteSpace(lb.Error)  )
                    {
                        lb.Log = $"{lb.LastResult} = {result.LastVersion} \r\n+++++++++++++++++++++ No changes to deploy +++++++++++++++++++++";
                        lb.ExitCode = 0;
                        lb.Error = "";
                        return lb;
                    }
                }

                if (config.UseMSBuild)
                {
                    var buildCommand = new MSBuildCommand()
                    {
                        Solution = config.SolutionPath,
                        BuildFolder = buildFolder,
                        Parameters = config.MSBuildParameters,
                        BuildConfig = config.MSBuildConfig
                    };

                    var lastBuild = buildCommand.Build();
                    if (!lastBuild.Success)
                    {
                        return lastBuild;
                    }

                    string webConfig = XDTService.Instance.Process(config);

                    IISManager.Instance.DeployFiles(config, webConfig);

                    lastBuild.Log += "\r\n+++++++++++++++++++++ Deployment Successful !!! +++++++++++++++++++++";

                    lastBuild.LastResult = result.LastVersion;

                    return lastBuild;
                }
                else {
                    throw new NotImplementedException();
                }

            }
            catch (Exception ex) {
                return new LastBuild { 
                    Error = ex.ToString(),
                    ExitCode = -1,
                    Time = DateTime.UtcNow
                };
            }

        }

        public class DownloadResult {
            public int UpdatedFiles { get; set; }
            public string LastVersion { get; set; }
        }

        static async Task<DownloadResult> DownloadFilesAsync(BuildConfig config, string buildFolder)
        {
            using (ISourceController ctrl = GetController(config))
            {
                using (LocalRepository rep = new LocalRepository(buildFolder, config.SiteId))
                {
                    ctrl.Initialize(config);

                    SourceRepository r = await ctrl.FetchAllFiles(config);

                    List<ISourceItem> remoteItems = r.Files;
                    var changes = rep.GetChanges(remoteItems).ToList();

                    var changeTypes = changes
                        .Where(x=>!x.RepositoryFile.IsDirectory)
                        .GroupBy(x => x.Type);
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

                    return new DownloadResult {
                        LastVersion = r.LatestVersion,
                        UpdatedFiles = updatedFiles.Count()
                    };
                }
            }
        }

        private static ISourceController GetController(BuildConfig config)
        {
            string sourceType = config.SourceType.ToLower();

            switch (sourceType)
            {
                //case "tfs2012":
                //    return new TFS2012Client();
                case "tfs2015":
                    return new TFS2015Client();
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
