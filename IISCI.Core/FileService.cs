using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IISCI
{
    public class FileService
    {

        public static FileService Instance = new FileService();

        public void Run(Action action, int max = 3)
        {
            while (true)
            {
                try
                {
                    action();
                    break;
                }
                catch
                {
                    if (max == 0)
                        throw;
                    Thread.Sleep(1000);
                }
                max--;
            }
        }

        public void Copy(string sourceFile, string destinationFile, bool overwrite = true)
        {
            Run(() => {
                File.Copy(sourceFile, destinationFile, overwrite);
            });
        }

        public void WriteAllText(string filePath, string text)
        {
            WriteAllText(filePath, text, Encoding.Unicode);
        }


        public void WriteAllText(string filePath, string text, Encoding encoding)
        {
            Run(() => {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.WriteAllText(filePath, text, encoding);
            });
        }

    }
}
