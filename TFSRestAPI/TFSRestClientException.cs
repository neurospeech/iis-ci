using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFSRestAPI
{
    public class TFSRestClientException : Exception
    {
        public System.Net.HttpStatusCode StatusCode { get; private set; }
        public string StatusDescription { get; private set; }
        public string Content { get; set; }

        public TFSRestClientException(System.Net.HttpStatusCode httpStatusCode, string p, string content)
            :base(httpStatusCode.ToString() + " - " + p + "\r\n" + content)
        {
            // TODO: Complete member initialization
            this.StatusCode = httpStatusCode;
            this.StatusDescription = p;
            this.Content = content;
        }
    }
}
