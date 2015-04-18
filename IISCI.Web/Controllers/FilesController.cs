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
            FileInfo file = GetFile(id,path);
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
            var dir = GetFolder(id);
            path = path.Replace("/", "\\");
            dir = new DirectoryInfo(dir.FullName + "\\" + path);
            if (!dir.Exists)
                return HttpNotFound();
            var files = dir.EnumerateFiles().Select(x => new { 
                x.Name,
                Size = x.Length,
                LastModified = x.LastWriteTimeUtc
            });

            return Json(files, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(string id, string path) {
            var dir = GetFolder(id);
            path = path.Replace("/","\\");
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
            var site = ServerManager.Sites.FirstOrDefault(x => x.Bindings.Any(b => b.Host == id));
            var app = site.Applications.FirstOrDefault();
            var vdir = app.VirtualDirectories.FirstOrDefault();
            return new DirectoryInfo(vdir.PhysicalPath);
        }

        private IEnumerable<DirectoryItem> GetChildren(DirectoryInfo dir, string p = "", int depth = 1) {

            yield return new DirectoryItem(dir.Name, "", 0);

            foreach (var item in dir.EnumerateDirectories().OrderBy(x=>x.Name))
            {
                yield return new DirectoryItem (p + item.Name, p,depth);
                foreach (var child in GetChildren(item,item.Name + "/" , depth+1))
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
            public DirectoryItem(string name, string parent, int depth)
            {
                Name = name;
                Path = parent + "/" +  name;
                Depth = depth;
            }
        }

    }
}