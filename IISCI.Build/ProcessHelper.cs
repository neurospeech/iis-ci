using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IISCI.Build
{
    public class ProcessHelper
    {
        public static int Execute(
            string program,
            string arguements,
            StringWriter console,
            StringWriter errors)
        {

            ProcessStartInfo info = new ProcessStartInfo(program, arguements);
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.RedirectStandardError = true;
            info.RedirectStandardOutput = true;


            Process p = new Process();
            p.StartInfo = info;
            p.EnableRaisingEvents = true;

            AutoResetEvent wait = new AutoResetEvent(false);

            p.Exited += (s, e) =>
            {
                wait.Set();
            };

            p.OutputDataReceived += (s, e) =>
            {
                console.WriteLine(e.Data);
            };

            p.ErrorDataReceived += (s, e) =>
            {
                errors.WriteLine(e.Data);
            };

            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            

            wait.WaitOne(5 * 60 * 1000);

            return p.ExitCode;

        }


        internal static void Execute(string batchFile)
        {
            int exitCode = 0;
            using (StringWriter errorWriter = new StringWriter()) {
                using (StringWriter consoleWriter = new StringWriter())
                {
                    exitCode = ProcessHelper.Execute(batchFile, "", consoleWriter, errorWriter);

                    Console.WriteLine(consoleWriter.ToString());
                }
                Console.WriteLine(errorWriter.ToString());
            }

            if (exitCode != 0) {
                throw new InvalidOperationException("Process.Execute failed !!!");
            }

        }
    }
}
