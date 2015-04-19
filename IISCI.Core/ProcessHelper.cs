using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IISCI
{
    public class ProcessHelper
    {
        public static int Execute(
            string program,
            string arguments,
            Action<string> consoleAction,
            Action<string> errorAction)
        {

            ProcessStartInfo info = new ProcessStartInfo(program, arguments);
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.RedirectStandardError = true;
            info.RedirectStandardOutput = true;
            info.WorkingDirectory = System.IO.Path.GetDirectoryName(program);

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
                consoleAction(e.Data);
            };

            p.ErrorDataReceived += (s, e) =>
            {
                errorAction(e.Data);
            };

            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            

            wait.WaitOne(5 * 60 * 1000);

            return p.ExitCode;

        }


        public static void Execute(string batchFile)
        {
            int exitCode = 0;
            using (StringWriter errorWriter = new StringWriter()) {
                using (StringWriter consoleWriter = new StringWriter())
                {
                    exitCode = ProcessHelper.Execute(batchFile, "",
                        a => consoleWriter.WriteLine(a),
                        a => errorWriter.WriteLine(a));

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
