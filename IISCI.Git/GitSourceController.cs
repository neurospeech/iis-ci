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

        public Task<List<ISourceItem>> FetchAllFiles(BuildConfig config)
        {

            DirectoryInfo gitFolder = new DirectoryInfo(config.BuildFolder + "\\git");
            if (!gitFolder.Exists)
            {
                gitFolder.Create();


                Console.WriteLine("Cloning repository " + config.SourceUrl);

                CloneOptions clone = new CloneOptions();
                clone.CredentialsProvider = CredentialsHandler;
                var rep = Repository.Clone(config.SourceUrl, gitFolder.FullName, clone);

                Console.WriteLine("Repository clone successful");
            }
            else {
                Console.WriteLine("Fetching remote Repository");
                var rep = new Repository(gitFolder.FullName);
                FetchOptions options = new FetchOptions();
                options.CredentialsProvider = CredentialsHandler;
                Remote remote = rep.Network.Remotes["origin"];
                rep.Fetch(remote.Name, options);
                Console.WriteLine("Fetch successful");
            }

            List<ISourceItem> files = new List<ISourceItem>();

            EnumerateFiles(gitFolder, files, "" );


            Parallel.ForEach(files, file =>
            {
                var md5 = System.Security.Cryptography.MD5.Create();
                ((GitSourceItem)file).Version = Convert.ToBase64String(md5.ComputeHash(File.ReadAllBytes(file.Url)));
            });


            return Task.FromResult(files);
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
