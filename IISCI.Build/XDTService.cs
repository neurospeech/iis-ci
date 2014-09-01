using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace IISCI.Build
{
    public class XDTService
    {

        public static XDTService Instance = new XDTService();

        public void Process(BuildConfig config){
            string buildFolder = config.BuildFolder;

            string buildXDT = buildFolder + "\\build.xdt";


            File.WriteAllText(buildXDT, CreateXDT(config));

            string webConfig = Path.GetDirectoryName( config.BuildFolder + "\\Source\\" + config.WebProjectPath )  + "\\web.config";

            if (File.Exists(webConfig))
            {
                Transform(webConfig, buildXDT);

                if (config.CustomXDT != null)
                {
                    string customXDT = buildFolder + "\\custom.xdt";
                    File.WriteAllText(customXDT, config.CustomXDT);
                    Transform(webConfig, customXDT);
                }
            }
            
        }


        private string CreateXDT(BuildConfig config) {

            XNamespace xdt = "http://schemas.microsoft.com/XML-Document-Transform";

            var doc = XDocument.Parse("<?xml version=\"1.0\"?><configuration></configuration>");

            var connectionStrings = new XElement(XName.Get("connectionStrings"));
            doc.Root.Add(connectionStrings);
            foreach (var item in config.ConnectionStrings)
            {
                XElement cnstr = new XElement(XName.Get("add"));
                connectionStrings.Add(cnstr);
                cnstr.SetAttributeValue(XName.Get("name"), item.Name);
                cnstr.SetAttributeValue(XName.Get("connectionString"), item.ConnectionString);
                cnstr.SetAttributeValue(xdt + "Transform", "SetAttributes");
                cnstr.SetAttributeValue(xdt + "Locator", "Match(name)");
                if(item.ProviderName!=null){
                    cnstr.SetAttributeValue(XName.Get("providerName"), item.ProviderName);
                }
            }
            var appSettings = new XElement(XName.Get("appSettings"));
            doc.Root.Add(appSettings);
            foreach (var item in config.AppSettings)
            {
                XElement cnstr = new XElement(XName.Get("add"));
                appSettings.Add(cnstr);
                cnstr.SetAttributeValue(XName.Get("key"), item.Key);
                cnstr.SetAttributeValue(XName.Get("value"), item.Value);
                cnstr.SetAttributeValue(xdt + "Transform", "SetAttributes");
                cnstr.SetAttributeValue(xdt + "Locator", "Match(key)");
            }

            return doc.ToString(SaveOptions.OmitDuplicateNamespaces);
        }

        private void Transform(string filePath, string xdtPath) 
        {
            Microsoft.Web.XmlTransform.XmlTransformation xtr = new Microsoft.Web.XmlTransform.XmlTransformation(xdtPath);

            XmlDocument doc = new XmlDocument();
            using (var fs = File.OpenRead(filePath))
            {
                doc.Load(fs);
            }

            xtr.Apply(doc);

            File.Delete(filePath);

            doc.Save(filePath);
            
        }

    }
}
