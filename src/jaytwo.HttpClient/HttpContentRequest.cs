using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace jaytwo.HttpClient
{
    public class HttpContentRequest : HttpRequestBase, IDisposable
    {
        public HttpContentRequest()
        {
        }

        public HttpContentRequest(string pathOrUri)
            : base(pathOrUri)
        {
        }

        public HttpContentRequest(Uri uri)
            : base(uri)
        {
        }

        public HttpContentRequest(HttpMethod method)
            : base(method)
        {
        }

        public HttpContentRequest(HttpMethod method, string uri)
            : base(method, uri)
        {
        }

        public HttpContentRequest(HttpMethod method, Uri uri)
            : base(method, uri)
        {
        }

        public new HttpContent Content
        {
            get
            {
                return (HttpContent)base.Content;
            }

            set
            {
                base.Content = value;
            }
        }

        public void Dispose()
        {
            Content?.Dispose();
        }
    }
}
