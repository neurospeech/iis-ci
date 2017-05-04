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
        public BuildConfig()
        {
            UseMSBuild = true;
            DeployNewFolder = true;
        }

        public string SiteId { get; set; }

        public string SiteHost { get; set; }

        public string SiteRoot { get; set; }

        public string BuildFolder { get; set; }

        public string SourceType { get; set; }

        public string SourceUrl { get; set; }

        public string SourceBranch { get; set; } = "master";

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

        public bool DeployNewFolder { get; set; }

        public bool DeleteLocalFiles { get; set; }

        public string Notify { get; set; }

        private List<StartUrl> _StartUrls = new List<StartUrl>();
        public StartUrl[] StartUrls { 
            get {
                return _StartUrls.ToArray();
            }
            set {
                _StartUrls.Clear();
                if (value == null)
                    return;
                _StartUrls.AddRange(value);
            }
        }

        private List<BuildAppSetting> _AppSettings = new List<BuildAppSetting>();
        public BuildAppSetting[] AppSettings { get {
            return _AppSettings.ToArray();
        }
            set {
                _AppSettings.Clear();
                if (value == null)
                    return;
                _AppSettings.AddRange(value);
            }
        }

        private List<BuildConnectionString> _ConnectionStrings = new List<BuildConnectionString>();
        public BuildConnectionString[] ConnectionStrings { get {
            return _ConnectionStrings.ToArray();
        }
            set {
                _ConnectionStrings.Clear();
                if (value == null)
                    return;
                _ConnectionStrings.AddRange(value);
            }
        }

        public string TriggerKey { get; set; }

        public string CustomXDT { get; set; }

    }

    public class BuildAppSetting {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class BuildConnectionString {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string ProviderName { get; set; }
    }

    public class StartUrl {
        public string Url { get; set; }
    }
}
