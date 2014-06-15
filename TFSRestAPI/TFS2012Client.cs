using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSRestAPI
{
    public class TFS2012Client : TFSRestClient
    {

        public TFS2012Client(string domain, string username, string password, string host, bool secure = true, int port = 443)
            :base(domain,username,password,host,secure,port)
        {

        }

        public async Task<IEnumerable<TFSSourceItem>> GetSourceItems(string collection, string path = null) {

            string url = "/tfs/" + collection + "/_api/_versioncontrol/items?__v=1&type=Any&recursion=OneLevel&path=" ;
            if (!path.StartsWith("$")) {
                path = "$" + path;
            }

            dynamic result = await Get(url);

            List<TFSSourceItem> list = new List<TFSSourceItem>();

            foreach (dynamic item in result.__wrappedArray)
            {
                TFSSourceItem sourceItem = new TFSSourceItem();
                sourceItem.Path = item.serverItem;
                sourceItem.Name = System.IO.Path.GetFileName(sourceItem.Path.Substring(1));
                sourceItem.IsDirectory = item.itemType == 1;
                sourceItem.IsBranch = item.isBranch;
                sourceItem.ID = item.id;
                sourceItem.ChangeSet = item.changeset;
                list.Add(sourceItem);
            }

            return list;
        }

        public async Task<IEnumerable<TFSCollection>> GetCollections(string selectedHostId = null) {

            string url = null;
            if (selectedHostId != null)
            {
                url += "/tfs/_api/_common/GetCollectionJumpList?__v=1&navigationContextPackage={}&selectedHostId=" + selectedHostId;
            }
            else {
                url += "/tfs/_api/_common/GetJumpList?__v=1&navigationContextPackage={}&showStoppedCollections=false";
            }
            dynamic result = await Get(url);
            List<TFSCollection> collections = new List<TFSCollection>();
            foreach (dynamic coll in result.__wrappedArray) {

                TFSCollection tcoll = new TFSCollection();
                tcoll.Name = coll.name;
                tcoll.Path = coll.path;
                tcoll.Url = coll.url;
                tcoll.BrowseUrl = coll.browseUrl;
                tcoll.CollectionID = coll.collectionId;
                collections.Add(tcoll);
                List<TFSProject> plist = new List<TFSProject>();

                foreach (dynamic project in coll.projects)
                {
                    TFSProject p = new TFSProject();
                    p.Name = project.name;
                    p.Path = project.path;
                    p.Url = project.url;
                    plist.Add(p);
                }

                tcoll.Projects = plist.ToArray();

            }

            if (selectedHostId == null) { 
                // get all children except first...
                foreach (var coll in collections.Skip(1)) {
                    var r = await GetCollections(coll.CollectionID.ToString());
                    if (r.Any()) {
                        var c = r.FirstOrDefault();
                        if (c.Projects.Any()) {
                            coll.Projects = c.Projects;
                        }
                    }
                }
            }

            return collections;
        }

    }

    public class TFSItem {
        
        public string Name { get; set; }
        public string Path { get; set; }
        public string Url { get; set; }
        public string BrowseUrl { get; set; }
    }

    public class TFSCollection : TFSItem {
        public string CollectionID { get; set; }
        public TFSProject[] Projects { get; set; }
    }

    public class TFSProject : TFSItem {
    }

    public class TFSSourceItem : TFSItem
    {
        public bool IsDirectory { get; set; }
        public bool IsBranch { get; set; }
        public int ChangeSet { get; set; }
        public long ID { get; set; }
    }
}
