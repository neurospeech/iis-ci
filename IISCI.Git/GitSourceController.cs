using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISCI.Git
{
    public class GitSourceController : ISourceController
    {
        BuildConfig config = null;

        public void Initialize(BuildConfig config)
        {
            this.config = config;
        }

        public Task<SourceRepository> FetchAllFiles(BuildConfig config)
        {

            var result = new SourceRepository();

            string gitFolder = config.BuildFolder + "\\git";

            //// temporary hack...
            //// git-pull does not work
            //if (Directory.Exists(gitFolder)) {
            //    Directory.Delete(gitFolder, true);
            //}

            if (!Directory.Exists(gitFolder))
            {
                Directory.CreateDirectory(gitFolder);


                Console.WriteLine("Cloning repository " + config.SourceUrl);

                CloneOptions clone = new CloneOptions();
                clone.CredentialsProvider = CredentialsHandler;
                Repository.Clone(config.SourceUrl, gitFolder, clone);

                Console.WriteLine("Repository clone successful");
            }

            Console.WriteLine("Fetching remote Repository");
            using (var rep = new Repository(gitFolder))
            {
                FetchOptions options = new FetchOptions();
                options.CredentialsProvider = CredentialsHandler;
                Remote remote = rep.Network.Remotes["origin"];

                //Commands.Fetch(rep,remote.Name,)

                //rep.Fetch(remote.Name, options);
                var master = rep.Branches[config.SourceBranch ?? "master"];
                
                var merge = Commands.Pull(rep, new Signature("IISCI", "IISCI.IISCI@IISCI.IISCI", DateTime.Now), new PullOptions()
                {
                    FetchOptions = options,
                    MergeOptions = new MergeOptions()
                    {
                        MergeFileFavor = MergeFileFavor.Theirs,
                        CommitOnSuccess = true
                    }
                });

                if (config.SourceBranch != "master")
                {
                    Commands.Checkout(rep, master);
                }

                Console.WriteLine("Fetch successful");

                result.LatestVersion = Convert.ToBase64String(rep.Head.Tip.Id.RawId);

                

                
            }


            List<ISourceItem> files = result.Files;

            EnumerateFiles( new DirectoryInfo(gitFolder), files, "" );


            //var md5 = System.Security.Cryptography.MD5.Create();

            Parallel.ForEach(files, file =>
            {
                ((GitSourceItem)file).Version = HashService.Instance.ComputeHash(file.Url);
            });

            

            return Task.FromResult(result);
        }

        private void EnumerateFiles(DirectoryInfo gitFolder, List<ISourceItem> list, string rootFolder)
        {
            foreach (var item in gitFolder.EnumerateDirectories())
            {
                if (item.Name == ".git")
                {
                    continue;
                }
                EnumerateFiles(item, list, rootFolder + "\\" + item.Name);
            }
            foreach (var item in gitFolder.EnumerateFiles())
            {
                GitSourceItem sourceItem = new GitSourceItem { 
                    Name = item.Name,
                    Folder = rootFolder,
                    Url = item.FullName
                };
                list.Add(sourceItem);
            }
        }

        Credentials CredentialsHandler(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            return new UsernamePasswordCredentials { Username = config.Username, Password = config.Password };
        }

        public async Task DownloadAsync(BuildConfig config, ISourceItem item, string filePath)
        {
            using (var source = File.OpenRead(item.Url))
            {
                using (var destination = File.OpenWrite(filePath))
                {
                    await source.CopyToAsync(destination);
                }
            }
        }

        public void Dispose()
        {
            
        }
    }

    public class GitSourceItem : ISourceItem
    {
        public string Name
        {
            get;
            set;
        }

        public string Folder
        {
            get;
            set;
        }

        public bool IsDirectory
        {
            get;
            set;
        }

        public string Url
        {
            get;
            set;
        }

        public string Version
        {
            get;
            set;
        }
    }

}
