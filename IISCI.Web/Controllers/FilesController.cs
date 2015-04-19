using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IISCI.Web.Controllers
{
    [Authorize]
    public class FilesController: BaseController
    {


        public ActionResult Raw(string id, string path) {
            if (path == null) path = "";
            path = path.Replace("/" + id, "\\");
            FileInfo file = GetFile(id, path);
            if (!file.Exists)
                return HttpNotFound();
            var mime = MimeMapping.GetMimeMapping(file.FullName);
            return File(file.FullName, mime);
        }

        public ActionResult Folders(string id) {
            DirectoryInfo folder = GetFolder(id);
            return Json(GetChildren(folder), JsonRequestBehavior.AllowGet);

        }

        public ActionResult Files(string id, string path) {
            var dir = GetFolder(id).Parent;
            if (path == null) path = "";
            path = path.Replace("/" + id, "\\");
            dir = new DirectoryInfo(dir.FullName + "\\" + path);
            //if (!dir.Exists)
            //    return HttpNotFound("Not found " + dir.FullName);
            var files = dir.EnumerateFileSystemInfos().Select(x => new { 
                x.Name,
                IsDirectory = x is DirectoryInfo,
                Size = (x is FileInfo) ? ((FileInfo)x).Length : 0 ,
                LastModified = x.LastWriteTimeUtc
            }).OrderByDescending(x=>x.IsDirectory).ThenBy(x=>x.Name);

            return Json(files, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(string id, string path) {
            var dir = GetFolder(id).Parent;
            path = path.Replace("/" + id,"\\");
            var file = new FileInfo(dir.FullName + "\\" + path);
            if (file.Exists) {
                file.Delete();
                return Json("ok");
            }
            dir = new DirectoryInfo(dir.FullName + "\\" + path);
            if (dir.Exists) {
                dir.Delete(true);
                return Json("ok");
            }
            return HttpNotFound();
        }

        private FileInfo GetFile(string id, string path) {
            DirectoryInfo dir = GetFolder(id);
            return new FileInfo(dir.FullName + "\\" + path.Replace("/", "\\"));
        }

        private DirectoryInfo GetFolder(string id)
        {
            var site = ServerManager.Sites.FirstOrDefault(x => x.Name == id);
            var app = site.Applications.FirstOrDefault();
            var vdir = app.VirtualDirectories.FirstOrDefault();
            return new DirectoryInfo(vdir.PhysicalPath);
        }

        private IEnumerable<DirectoryItem> GetChildren(DirectoryInfo dir, string p = "", int depth = 1) {

            yield return new DirectoryItem(dir.Name, p, depth);

            foreach (var item in dir.EnumerateDirectories().OrderBy(x=>x.Name))
            {
                foreach (var child in GetChildren(item, p + "/" +  dir.Name , depth+1))
                {
                    yield return child;
                }
            }
        }

        public class DirectoryItem
        {
            public string Name { get; set; }
            public int Depth { get; set; }
            public string Path { get; set; }
            public bool IsRoot { get; set; }
            public DirectoryItem(string name, string parent, int depth)
            {
                Name = name;
                Path = parent + "/" + Name;
                Depth = depth;
            }
        }

    }
}