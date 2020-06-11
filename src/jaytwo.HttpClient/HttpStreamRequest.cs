using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace jaytwo.HttpClient
{
    public class HttpStreamRequest : HttpRequestBase, IDisposable
    {
        public HttpStreamRequest()
        {
        }

        public HttpStreamRequest(string pathOrUri)
            : base(pathOrUri)
        {
        }

        public HttpStreamRequest(Uri uri)
            : base(uri)
        {
        }

        public HttpStreamRequest(HttpMethod method)
            : base(method)
        {
        }

        public HttpStreamRequest(HttpMethod method, string uri)
            : base(method, uri)
        {
        }

        public HttpStreamRequest(HttpMethod method, Uri uri)
            : base(method, uri)
        {
        }

        public new Stream Content
        {
            get
            {
                return (Stream)base.Content;
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
