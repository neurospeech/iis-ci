using System;
using System.Collections.Generic;
using System.Dynamic;
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
        HttpClientHandler clientHandler;
        HttpClient client;

        public TFSRestClient(string domain, string username, string password, string host, bool secure = true, int port=443)
        {
            if (secure)
            {
                baseUrl = string.Format("https://{0}:{1}", host, port);
            }
            else {
                baseUrl = string.Format("http://{0}:{1}", host, port);
            }

            clientHandler = new HttpClientHandler();
            clientHandler.CookieContainer = new System.Net.CookieContainer();
            clientHandler.Credentials = new NetworkCredential(username, password, domain);
            clientHandler.UseDefaultCredentials = true;
            clientHandler.UseCookies = true;
            client = new HttpClient(clientHandler);
            
        }

        private async Task<T> Invoke<T>(string url, object p, HttpMethod method, Func<HttpContent,Task<T>> func)
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


            if (r.IsSuccessStatusCode)
            {
                return await func(r.Content);
            }
            else
            {
                var content = await r.Content.ReadAsStringAsync();
                throw new TFSRestClientException(r.StatusCode, r.ReasonPhrase, content);
            }
        }


        protected async Task<T> Get<T>(string url, object p = null)
        {
            return await Invoke(url, p, HttpMethod.Get, async response => {
                var content = await response.ReadAsStringAsync();
                JavaScriptSerializer js = new JavaScriptSerializer();
                return js.Deserialize<T>(content);
            });
        }

        protected async Task<dynamic> Get(string url, object p = null) {
            return await Invoke(url, p, HttpMethod.Get, async response =>
            {
                var content = await response.ReadAsStringAsync();
                JavaScriptSerializer js = new JavaScriptSerializer();
                return ResultDictionary.BuildResult(js.DeserializeObject(content));
            });
        }


        public void Dispose()
        {
            client.Dispose();
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
