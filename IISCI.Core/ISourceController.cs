using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISCI
{
    public interface ISourceController: IDisposable
    {
        void Initialize(BuildConfig config);

        Task<SourceRepository> FetchAllFiles(BuildConfig config);

        Task DownloadAsync(BuildConfig config, ISourceItem item, string filePath);
    }

    public class SourceRepository {

        public string LatestVersion { get; set; }

        public List<ISourceItem> Files { get; }
            = new List<ISourceItem>();
    }
}
