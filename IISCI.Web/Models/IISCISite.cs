using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace IISCI.Web.Models
{
    public class IISCISite
    {
        public IISCISite(string storePath, string path)
        {
            StorePath = storePath;
            Path = path;

            if (!Directory.Exists(storePath))
                Directory.CreateDirectory(storePath);
        }

        public string StorePath { get; set; }

        public string Path { get; set; }

        public T LoadConfig<T>()
        {
            string name = StorePath + "\\" + typeof(T).Name + ".json";
            if (!File.Exists(name))
                return Activator.CreateInstance<T>();
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Deserialize<T>(File.ReadAllText(name));
        }

        public void SaveConfig(object config)
        {
            string name = StorePath + "\\" + config.GetType().Name + ".json";
            JavaScriptSerializer js = new JavaScriptSerializer();
            File.WriteAllText( name, js.Serialize(config));
        }


    }
}