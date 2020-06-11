using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using jaytwo.HttpClient.Authentication;

namespace jaytwo.HttpClient
{
    public class HttpRequest : HttpRequestBase
    {
        public HttpRequest()
        {
        }

        public HttpRequest(string pathOrUri)
            : base(pathOrUri)
        {
        }

        public HttpRequest(Uri uri)
            : base(uri)
        {
        }

        public HttpRequest(HttpMethod method)
            : base(method)
        {
        }

        public HttpRequest(HttpMethod method, string uri)
            : base(method, uri)
        {
        }

        public HttpRequest(HttpMethod method, Uri uri)
            : base(method, uri)
        {
        }

        public new string Content
        {
            get
            {
                return (string)base.Content;
            }

            set
            {
                base.Content = value;
            }
        }
    }
}
