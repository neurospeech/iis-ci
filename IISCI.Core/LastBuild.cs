using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISCI
{
    public class LastBuild
    {
        public string Error { get; set; }
        public string Log { get; set; }
        public DateTime? Time { get; set; }
        public int ExitCode { get; set; }
    }
}
