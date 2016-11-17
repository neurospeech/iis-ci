using IISCI;
using Microsoft.TeamFoundation.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace TFSRestAPI
{
    public class TFS2015Client : ISourceController
    {

        TfsTeamProjectCollection tpc;

        public VersionControlServer Server { get; private set; }

        public void Dispose()
        {
            if (tpc != null)
            {
                tpc.Dispose();
            }
        }

        public Task DownloadAsync(BuildConfig config, ISourceItem item, string filePath)
        {
            return Task.Run(()=> {
                
                var local = item as LocalRepositoryFile;
                var file = local==null ? (item as TFS2015FileItem) : local.Source as TFS2015FileItem;
                if (file == null) {
                    throw new InvalidOperationException("Input=" + (item == null ? "null" : item.ToString()));
                }
                file.Item.DownloadFile(filePath);
            });
        }

        public Task<List<ISourceItem>> FetchAllFiles(BuildConfig config)
        {
            return Task.Run(() =>
            {
                List<ISourceItem> list = new List<ISourceItem>();
                var result = Server.GetItems(config.RootFolder, VersionSpec.Latest, RecursionType.Full, DeletedState.Any, ItemType.Any);
                foreach (var item in result.Items) {
                    if (item.DeletionId == 0)
                    {
                        list.Add(new TFS2015FileItem(item, config));
                    }
                }
                return list;
            });
        }

        public void Initialize(BuildConfig config)
        {
            NetworkCredential credentials = new NetworkCredential(config.Username, config.Password);
            BasicAuthCredential basicCredentials = new BasicAuthCredential(credentials);
            TfsClientCredentials cred = new TfsClientCredentials(basicCredentials);
            cred.AllowInteractive = false;
            tpc = new TfsTeamProjectCollection(new Uri(config.SourceUrl + "/" + config.Collection),cred);
            
            tpc.Authenticate();

            

            this.Server = tpc.GetService<Microsoft.TeamFoundation.VersionControl.Client.VersionControlServer>();
            
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
        }

    }
}
