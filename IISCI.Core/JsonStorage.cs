using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISCI
{
    public class JsonStorage
    {


        public static T Read<T>(TextReader reader)
        {
            JsonSerializer js = new JsonSerializer();

            using (JsonTextReader jsonReader = new JsonTextReader(reader))
            {

                return js.Deserialize<T>(jsonReader);
            }
        }

        public static Dictionary<string, object> Read(string content) {
            JsonSerializer js = new JsonSerializer();
            using (StringReader sr = new StringReader(content))
            {
                using (JsonTextReader reader = new JsonTextReader(sr)) {
                    return (Dictionary<string, object>)js.Deserialize(reader);
                }
            }
        }

        public static void Write(object obj, TextWriter writer) 
        {
            JsonSerializer js = new JsonSerializer();
            js.Formatting = Formatting.Indented;
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            {
                js.Serialize(jsonWriter, obj);
            }
        }

        public static void WriteFile(object obj, string filePath){
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Directory.Exists) {
                fileInfo.Directory.Create();
            }
            if (fileInfo.Exists) {
                fileInfo.Delete();
            }
            using (StreamWriter sw = new StreamWriter(filePath)) {
                Write(obj, sw);
            }
        }

        public static T ReadFile<T>(string filePath) {
            using (StreamReader sr = new StreamReader(filePath)) {
                return Read<T>(sr);
            }
        }

    }
}
