using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISCI
{
    public class ZipSourceController: ISourceController
    {
        private string zipFile = null;
        private string zipFolder = null;

        public void Initialize(BuildConfig config)
        {
            zipFile = config.BuildFolder = "\\_source.zip";
            if(File.Exists(zipFile)){
                File.Delete(zipFile);
            }

            zipFolder = config.BuildFolder = "\\_zipSource";
            if (Directory.Exists(zipFolder)) {
                Directory.Delete(zipFolder, true);
            }
            Directory.CreateDirectory(zipFolder);
        }

        public async Task<SourceRepository> FetchAllFiles(BuildConfig config)
        {
            var result = new SourceRepository();

            


            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient()) {
                using (var stream = await client.GetStreamAsync(config.SourceUrl)) {
                    using (var fs = File.OpenWrite(zipFile)) {
                        await stream.CopyToAsync(fs);
                    }
                }
            }

            List<ISourceItem> files = result.Files;

            result.LatestVersion = HashService.Instance.ComputeHash(zipFile);

            using (ZipFile zip = new ZipFile(zipFile)) {
                foreach (ZipEntry entry in zip)
                {
                    if (entry.IsDirectory)
                        continue;
                    using (var s = zip.GetInputStream(entry)) {
                        string filePath = zipFolder + "\\" + entry.Name;
                        FileInfo fi = new FileInfo(filePath);
                        if (!fi.Directory.Exists)
                        {
                            fi.Directory.Create();
                        }
                        using (FileStream fs = fi.OpenWrite()) {
                            await fs.CopyToAsync(s);
                        }

                        string fileName = System.IO.Path.GetFileName(filePath);

                        ZipSourceItem file = new ZipSourceItem { 
                            Url = filePath,
                            Name = System.IO.Path.GetFileName(filePath),
                            Folder = entry.Name.Substring(0,entry.Name.Length-fileName.Length),
                            IsDirectory = false
                        };
                        files.Add(file);
                    }                    
                }
            }


            Parallel.ForEach(files, file => {
                ((ZipSourceItem)file).Version = HashService.Instance.ComputeHash(file.Url);
            });

            return result;
        }

        public async Task DownloadAsync(BuildConfig config, ISourceItem item, string filePath)
        {
            using (var source = File.OpenRead(item.Url)) {
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

    public class ZipSourceItem : ISourceItem
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

        public Task DownloadAsync(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
