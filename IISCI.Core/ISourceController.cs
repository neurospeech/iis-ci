using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISCI
{
    public interface ISourceController: IDisposable
    {
        Task<string> SyncAsync(BuildConfig config, LocalRepository localRepository);

        Task DownloadAsync(BuildConfig config, ISourceItem item, string filePath);
    }
}
