using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISCI
{
    public class BuildConfig
    {

        public string SourceType { get; set; }

        public string SourceUrl { get; set; }

        public string Domain { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Collection { get; set; }

        public string RootFolder { get; set; }

        public string SolutionPath { get; set; }

        public string WebProjectPath { get; set; }

        public bool UseMSBuild { get; set; }
        public string MSBuildConfig { get; set; }
        public string MSBuildParameters { get; set; }

        public bool DeleteLocalFiles { get; set; }



    }
}
