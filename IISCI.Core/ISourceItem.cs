using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISCI
{
    public interface ISourceItem
    {

        string Name { get; }

        string Folder { get; }

        bool IsDirectory { get; }

        string Url { get; }

        string Version { get;  }

        Task DownloadAsync(string filePath);
    }
}
