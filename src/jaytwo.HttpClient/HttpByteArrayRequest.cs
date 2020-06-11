using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace jaytwo.HttpClient
{
    public class HttpByteArrayRequest : HttpRequestBase
    {
        public HttpByteArrayRequest()
        {
        }

        public HttpByteArrayRequest(string pathOrUri)
            : base(pathOrUri)
        {
        }

        public HttpByteArrayRequest(Uri uri)
            : base(uri)
        {
        }

        public HttpByteArrayRequest(HttpMethod method)
            : base(method)
        {
        }

        public HttpByteArrayRequest(HttpMethod method, string uri)
            : base(method, uri)
        {
        }

        public HttpByteArrayRequest(HttpMethod method, Uri uri)
            : base(method, uri)
        {
        }

        public new byte[] Content
        {
            get
            {
                return (byte[])base.Content;
            }

            set
            {
                base.Content = value;
            }
        }
    }
}
