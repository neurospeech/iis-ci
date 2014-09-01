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

        public async Task<List<ISourceItem>> FetchAllFiles(BuildConfig config)
        {
            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient()) {
                using (var stream = await client.GetStreamAsync(config.SourceUrl)) {
                    using (var fs = File.OpenWrite(zipFile)) {
                        await stream.CopyToAsync(fs);
                    }
                }
            }

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
                        using (var fs = fi.OpenWrite()) {
                            await s.CopyToAsync(fs);
                        }
                    }                    
                }
            }

            // analyze and decompress zip file...
            throw new NotImplementedException();
        }

        public Task DownloadAsync(BuildConfig config, ISourceItem item, string filePath)
        {
            return Task.FromResult(0);
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
    }
}
