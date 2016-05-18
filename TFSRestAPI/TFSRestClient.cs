using IISCI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TFSRestAPI
{
    public class TFSRestClient : IDisposable
    {

        private string baseUrl;

        System.Net.CookieContainer cookies = new CookieContainer();
        CredentialCache credentials;

        System.Net.Http.HttpClient client;
        private HttpClientHandler clientHandler;

        public TFSRestClient()
        {

        }

        protected void InitializeClient(BuildConfig config) 
        {
            baseUrl = config.SourceUrl;

            //credentials = new NetworkCredential(config.Username, config.Password, config.Domain);
            credentials = new CredentialCache();
            credentials.Add(new Uri(config.SourceUrl), "NTLM", new NetworkCredential(config.Username, config.Password, config.Domain));


            clientHandler = new System.Net.Http.HttpClientHandler();
            clientHandler.CookieContainer = cookies;
            clientHandler.Credentials = credentials;
            clientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            clientHandler.UseCookies = true;
            

            client = new HttpClient(clientHandler);
            client.Timeout = TimeSpan.FromMinutes(10);

        }

        private async Task<string> Invoke(string url, object p, HttpMethod method)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            string requestUrl = baseUrl + url;
            HttpRequestMessage request = new HttpRequestMessage(method, requestUrl);
            if (p != null)
            {
                var input = new StringContent(js.Serialize(p), Encoding.UTF8, "application/json; charset=utf-8");
                request.Content = input;
            }
            var r = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!r.IsSuccessStatusCode)
            {
                var content = await r.Content.ReadAsStringAsync();
                throw new TFSRestClientException(r.StatusCode, r.ReasonPhrase, requestUrl + "\r\n" + content);
            }

            return await r.Content.ReadAsStringAsync();
        }

        protected async Task DownloadAsync(string url, Stream outputStream)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            string requestUrl = baseUrl + url;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            var r = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!r.IsSuccessStatusCode)
            {
                var content = await r.Content.ReadAsStringAsync();
                throw new TFSRestClientException(r.StatusCode, r.ReasonPhrase, requestUrl + "\r\n" + content);
            }

            var input = await r.Content.ReadAsStreamAsync();
            await input.CopyToAsync(outputStream);
        }



        protected async Task<T> Get<T>(string url, object p = null)
        {
            var content = await Invoke(url, p, HttpMethod.Get);

            return JsonConvert.DeserializeObject<T>(content);
        }

        public class CustomResolver : SimpleTypeResolver
        {
            public static CustomResolver Instance = new CustomResolver();

            private CustomResolver()
            {

            }

            public override Type ResolveType(string id)
            {
                Type t =  base.ResolveType(id);
                if (t == null) {
                    return typeof(Dictionary<string, object>);
                }
                return t;
            }

            public override string ResolveTypeId(Type type)
            {
                return base.ResolveTypeId(type);
            }
        }

        protected async Task<dynamic> Get(string url, object p = null) {
            var content = await Invoke(url, p, HttpMethod.Get);

            JavaScriptSerializer js = new JavaScriptSerializer(CustomResolver.Instance);
            object value = js.Deserialize<Dictionary<string, object>>(content);
            return ResultDictionary.BuildResult(value);
        }


        public void Dispose()
        {
            if (client != null) {
                client.Dispose();
                client = null;
            }

            if (clientHandler != null) {
                clientHandler.Dispose();
                clientHandler = null;
            }
        }
    }

    public class ResultDictionary : DynamicObject
    {
        private Dictionary<string, object> values = new Dictionary<string, object>();

        public static dynamic BuildResult(object value) {
            Dictionary<string, object> v = value as Dictionary<string, object>;
            if (v != null)
            {
                return new ResultDictionary(v);
            }

            System.Collections.IList ien = value as System.Collections.IList;
            if (ien != null) {
                List<dynamic> list = new List<dynamic>();
                foreach (var item in ien)
                {
                    list.Add(BuildResult(item));
                }
                return list;
            }

            return value;
        }

        public ResultDictionary(Dictionary<string,object> input)
        {
            foreach (var item in input)
            {
                values[item.Key] = BuildResult(item.Value);
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            object v = null;
            if (values.TryGetValue(binder.Name, out v))
            {
                result = v;
                return true;
            }
            Type t = binder.ReturnType;
            if (t == typeof(bool))
            {
                result = false;
                return true;
            }
            if(t==typeof(int) || t==typeof(long)){
                result = 0;
                return true;
            }
            if (t == typeof(string)) {
                result = null;
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return values.Keys;
        }
    }
}
