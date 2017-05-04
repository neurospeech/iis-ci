using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IISCI.Web.Controllers
{
    public class BuildSourceMaps
    {

        public readonly List<BuildSourceMap> List = new List<BuildSourceMap>();

        public static BuildSourceMaps Instance = new BuildSourceMaps();

        private string IISStore;

        public BuildSourceMap Get(string sourceKey) {

            if (IISStore == null)
            {
                IISStore = System.Web.Configuration.WebConfigurationManager.AppSettings["IISCI.Store"];
            }


            lock (Instance) {
                var existingList = IISStore + "\\source-map.json";

                var list = JsonStorage.ReadFileOrDefault<List<BuildSourceMap>>(existingList);
                List.Clear();
                List.AddRange(list);

                var map = List.FirstOrDefault(x => x.SourceKey == sourceKey);
                if (map == null) {
                    map = new BuildSourceMap {
                        Id = "Source-" + (List.Count + 1),
                        SourceKey = sourceKey
                    };
                    List.Add(map);
                    JsonStorage.WriteFile(List, existingList);
                }
                return map;
            }

        }

    }
}