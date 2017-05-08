using IISCI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TFSRestAPI
{

    //public class WrappedArray<T> {
    //    public T[] __wrappedArray { get; set; }
    //}

    //public class TFSFileItem : ISourceItem {
    //    public long id { get; set; }
    //    public int itemType { get; set; }
    //    public string serverItem { get; set; }
    //    public string version { get; set; }
    //    public long deletionId { get; set; }

    //    public string Folder { get; set; }
    //    public bool IsDirectory { get { return itemType == 1; } }
    //    public string Name { get { return System.IO.Path.GetFileName(serverItem); } }
    //    public string Url { get { return serverItem; } }
    //    public string Version { get { return this.version; } }
    //}

    //public class TFS2012Client : TFSRestClient, ISourceController
    //{

    //    public TFS2012Client()
    //    {

    //    }

    //    public void Initialize(BuildConfig config) {
    //        InitializeClient(config);
    //    }

    //    public async Task<List<ISourceItem>> FetchAllFiles(BuildConfig config)
    //    {

    //        string collection = config.Collection;
    //        string rootFolder = config.RootFolder;
    //        string path = rootFolder;

    //        string url = "/tfs/" + collection + "/_api/_versioncontrol/items?__v=1&type=Any&recursion=Full&path=" ;
    //        if (!path.StartsWith("$")) {
    //            path = "$/" + collection + path;
    //        }

    //        path = HttpUtility.UrlEncode(path);

    //        var result = await Get<WrappedArray<TFSFileItem>>(url + path);

    //        List<ISourceItem> list = new List<ISourceItem>();

    //        foreach (var item in result.__wrappedArray)
    //        {
    //            if (item.deletionId == 0)
    //            {
    //                list.Add(item);
    //                item.Folder = item.Url.Substring(rootFolder.Length);
    //                if (!item.IsDirectory)
    //                {
    //                    item.Folder = item.Folder.Substring(0, item.Folder.Length - item.Name.Length);
    //                }
    //            }
    //        }
    //        return list;
    //    }

    //    public async Task<IEnumerable<TFSCollection>> GetCollections(string selectedHostId = null) {

    //        string url = null;
    //        if (selectedHostId != null)
    //        {
    //            url += "/tfs/_api/_common/GetCollectionJumpList?__v=1&navigationContextPackage={}&selectedHostId=" + selectedHostId;
    //        }
    //        else {
    //            url += "/tfs/_api/_common/GetJumpList?__v=1&navigationContextPackage={}&showStoppedCollections=false";
    //        }
    //        dynamic result = await Get(url);
    //        List<TFSCollection> collections = new List<TFSCollection>();
    //        foreach (dynamic coll in result.__wrappedArray) {

    //            TFSCollection tcoll = new TFSCollection();
    //            tcoll.Name = coll.name;
    //            tcoll.Path = coll.path;
    //            tcoll.Url = coll.url;
    //            tcoll.BrowseUrl = coll.browseUrl;
    //            tcoll.CollectionID = coll.collectionId;
    //            collections.Add(tcoll);
    //            List<TFSProject> plist = new List<TFSProject>();

    //            foreach (dynamic project in coll.projects)
    //            {
    //                TFSProject p = new TFSProject();
    //                p.Name = project.name;
    //                p.Path = project.path;
    //                p.Url = project.url;
    //                plist.Add(p);
    //            }

    //            tcoll.Projects = plist.ToArray();

    //        }

    //        if (selectedHostId == null) { 
    //            // get all children except first...
    //            foreach (var coll in collections.Skip(1)) {
    //                var r = await GetCollections(coll.CollectionID.ToString());
    //                if (r.Any()) {
    //                    var c = r.FirstOrDefault();
    //                    if (c.Projects.Any()) {
    //                        coll.Projects = c.Projects;
    //                    }
    //                }
    //            }
    //        }

    //        return collections;
    //    }

    //    public async Task DownloadAsync(BuildConfig config, ISourceItem item, string filePath){
    //        string url = "/tfs/" + config.Collection + "/_api/_versionControl/itemContent?path=";
    //        url += HttpUtility.UrlEncode(item.Url);
    //        using(var fs = System.IO.File.OpenWrite(filePath)){
    //            await DownloadAsync(url, fs);
    //        }

    //    }


    //}

    //public class TFSItem {
        
    //    public string Name { get; set; }
    //    public string Path { get; set; }
    //    public string Url { get; set; }
    //    public string BrowseUrl { get; set; }
    //}

    //public class TFSCollection : TFSItem {
    //    public string CollectionID { get; set; }
    //    public TFSProject[] Projects { get; set; }
    //}

    //public class TFSProject : TFSItem {
    //}

    //public class TFSSourceItem : TFSItem, ISourceItem
    //{
    //    public bool IsDirectory { get; set; }
    //    public bool IsBranch { get; set; }
    //    public int ChangeSet { get; set; }
    //    public string Folder { get; set; }
    //    public long ID { get; set; }

    //    string ISourceItem.Version
    //    {
    //        get
    //        {
    //            return ChangeSet.ToString();
    //        }
    //    }

    //}
}
