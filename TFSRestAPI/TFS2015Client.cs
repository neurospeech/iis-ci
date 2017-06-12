using IISCI;
using Microsoft.TeamFoundation.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.IO;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace TFSRestAPI
{
    public class TFS2015Client : ISourceController
    {

        TfsTeamProjectCollection tpc;

        public VersionControlServer Server { get; private set; }
        public VssConnection Conn { get; private set; }

        public void Dispose()
        {
            if (tpc != null)
            {
                tpc.Dispose();
            }
        }

        public Task DownloadAsync(BuildConfig config, ISourceItem item, string filePath)
        {
            return Task.Run(async ()=> {
                
                var local = item as LocalRepositoryFile;
                if (local != null)
                    return;

                if (!File.Exists(filePath)) {
                    await item.DownloadAsync(filePath);
                    return;
                }

                string t = Path.GetTempFileName();
                await item.DownloadAsync(t);
                var existing = File.ReadAllBytes(filePath);
                var modified = File.ReadAllBytes(t);

                // are both same..
                if (!AreEqual(existing, modified)) {
                    File.Delete(filePath);
                    File.Copy(t, filePath);
                    File.Delete(t);
                }
            });
        }

        private bool AreEqual(byte[] existing, byte[] modified)
        {
            if (existing.Length != modified.Length)
                return false;
            return existing.SequenceEqual(modified);
        }

        public Task<SourceRepository> FetchAllFiles(BuildConfig config)
        {
            return Task.Run(async () =>
            {
                if (this.Conn != null) {
                    using (var client = this.Conn.GetClient<TfvcHttpClient>()) {
                        List<TfvcItem> files = await client.GetItemsAsync(config.RootFolder, VersionControlRecursionType.Full);
                        SourceRepository repo = new SourceRepository();
                        repo.Files.AddRange( files.Where(x=>x.DeletionId == 0).Select(x => new TFSWebFileItem(x,Conn,config) ));
                        return repo;
                    }
                }

                SourceRepository result = new SourceRepository();
                result.LatestVersion = Server.GetLatestChangesetId().ToString();
                List<ISourceItem> list = result.Files;
                var items = Server.GetItems(config.RootFolder, VersionSpec.Latest, RecursionType.Full, DeletedState.Any, ItemType.Any);
                foreach (var item in items.Items) {
                    if (item.DeletionId == 0)
                    {
                        list.Add(new TFS2015FileItem(item, config));
                    }
                }
                return result;
            });
        }

        public class TFSWebFileItem : ISourceItem
        {
            private TfvcItem x;
            private VssConnection conn;

            public TFSWebFileItem(TfvcItem x, VssConnection conn, BuildConfig config)
            {
                this.x = x;
                this.conn = conn;
                string path = x.Path;
                path = path.Substring(config.RootFolder.Length);
                Name = System.IO.Path.GetFileName(path);
                Folder = path;
                IsDirectory = x.IsFolder;
                if (!x.IsFolder) {
                    Folder = Folder.Substring(0, Folder.Length - Name.Length);
                }
                Url = x.Url;
                this.Version = x.ChangesetVersion.ToString();
            }

            public string Name { get; set; }

            public string Folder { get; set; }

            public bool IsDirectory { get; set; }

            public string Url {get;set;}

            public string Version { get; set; }

            public async Task DownloadAsync(string filePath)
            {
                using (var client = conn.GetClient<TfvcHttpClient>())
                {
                    using (var stream = await client.GetItemContentAsync(x.Path)) {
                        using (var ostream = File.OpenWrite(filePath)) {
                            await stream.CopyToAsync(ostream);
                        }
                    }
                }
            }
        }

        public void Initialize(BuildConfig config)
        {

            string username = string.IsNullOrWhiteSpace(config.Username) ? string.Empty : config.Username;

            if (username == string.Empty)
            {
                InitializeWebApi(config);
                return;
            }

            VssCredentials c = null;

            c = new VssCredentials(new VssBasicCredential(string.IsNullOrWhiteSpace(config.Username) ? String.Empty : config.Username, config.Password));


            c.PromptType = CredentialPromptType.DoNotPrompt;

            tpc = new TfsTeamProjectCollection(new Uri(config.SourceUrl + "/" + config.Collection),c);
            
            tpc.Authenticate();

            

            this.Server = tpc.GetService<Microsoft.TeamFoundation.VersionControl.Client.VersionControlServer>();
            
        }

        private void InitializeWebApi(BuildConfig config)
        {
            VssCredentials c = null;

            c = new VssCredentials(new VssBasicCredential(string.IsNullOrWhiteSpace(config.Username) ? String.Empty : config.Username, config.Password));


            c.PromptType = CredentialPromptType.DoNotPrompt;

            this.Conn = new VssConnection(new Uri(config.SourceUrl + "/" + config.Collection), c);

            //this.Client = conn.GetClient<TfvcHttpClient>();

        }

        public class TFS2015FileItem : ISourceItem
        {
            public Item Item { get; }

            public TFS2015FileItem(Item item,BuildConfig config)
            {
                this.Item = item;
                Version = item.ChangesetId.ToString();
                string path = item.ServerItem;
                path = path.Substring(config.RootFolder.Length);
                this.Folder = path;
                this.IsDirectory = item.ItemType == ItemType.Folder;
                this.Name = System.IO.Path.GetFileName(path);
                if (!this.IsDirectory) {
                    Folder = Folder.Substring(0, Folder.Length - Name.Length);
                }
                this.Url = item.ServerItem;
            }

            public string Folder
            {
                get;
            }

            public bool IsDirectory
            {
                get;
            }

            public string Name
            {
                get;
            }

            public string Url
            {
                get;
            }

            public string Version
            {
                get;
            }

            public Task DownloadAsync(string filePath)
            {
                Item.DownloadFile(filePath);
                return Task.CompletedTask;
            }
        }

    }
}
