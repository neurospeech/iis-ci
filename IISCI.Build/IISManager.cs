using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IISCI.Build
{
    public class IISManager
    {
        public static IISManager Instance = new IISManager();


        internal void DeployFiles(BuildConfig config, string webConfig)
        {
            using (ServerManager mgr = new ServerManager()) {
                Site site = null;
                if (!string.IsNullOrWhiteSpace(config.SiteId))
                {
                    site = mgr.Sites.FirstOrDefault(x => x.Name == config.SiteId);
                }
                else
                {
                    site = mgr.Sites.FirstOrDefault(x => x.Bindings.Any(a => a.Host == config.SiteHost));
                }
                if (site == null) {
                    throw new KeyNotFoundException("No site with binding " + config.SiteHost + " found in IIS");
                }

                var app = site.Applications.FirstOrDefault();
                site.Stop();

                // copy all files...
                var dir = app.VirtualDirectories.FirstOrDefault();
                var rootFolder = dir.PhysicalPath;

                if (config.UseMSBuild) {
                    DeployWebProject(config, rootFolder);
                }

                FileInfo configFile = new FileInfo(rootFolder + "\\Web.Config");
                if (configFile.Exists)
                {
                    configFile.Delete();
                }
                
                File.WriteAllText(configFile.FullName, webConfig , UnicodeEncoding.Unicode);

                site.Start();
            }
        }

        private void DeployWebProject(BuildConfig config, string rootFolder)
        {

            FileInfo webProjectFile = new FileInfo(config.BuildFolder + "\\Source\\" + config.WebProjectPath);
            DirectoryInfo dir = webProjectFile.Directory;

            XDocument doc = XDocument.Load(webProjectFile.FullName);

            List<WebProjectFile> files = GetContentList(doc.Descendants()).ToList();

            foreach (var file in files)
            {
                var sourcePath = dir.FullName + "\\" + file.FilePath;
                var targetPath = rootFolder + "\\" + file.WebPath;
                string targetDir = System.IO.Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(targetDir)) {
                    Directory.CreateDirectory(targetDir);
                }
                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, targetPath, true);
                }
            }

            dir = new DirectoryInfo(dir.FullName + "\\bin");

            DirectoryInfo tdir = new DirectoryInfo(rootFolder + "\\bin");
            if (!tdir.Exists) {
                tdir.Create();
            }

            foreach (var dll in dir.EnumerateFiles())
            {
                var targetPath = rootFolder + "\\bin\\" + dll.Name;
                dll.CopyTo(targetPath, true);
            }
        }

        private IEnumerable<WebProjectFile> GetContentList(IEnumerable<XElement> enumerable)
        {
            foreach (var item in enumerable.Where(x=>x.Name.LocalName == "Content"))
            {
                var at = item.Attributes().FirstOrDefault(x => x.Name.LocalName == "Include");
                if (at != null) {

                    var link = item.Elements().FirstOrDefault(x => x.Name.LocalName == "Link");
                    if (link != null) {
                        yield return new WebProjectFile { 
                            WebPath = link.Value,
                            FilePath = at.Value
                        };
                        continue;
                    }

                    yield return new WebProjectFile { 
                        WebPath = at.Value ,
                        FilePath = at.Value
                    };
                }
            }
        }

        public class WebProjectFile {
            public string WebPath { get; set; }
            public string FilePath { get; set; }
        }
    }
}
