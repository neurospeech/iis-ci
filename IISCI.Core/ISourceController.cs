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

        Task<List<ISourceItem>> FetchAllFiles(BuildConfig config);

        Task DownloadAsync(BuildConfig config, ISourceItem item, string filePath);
    }
}
