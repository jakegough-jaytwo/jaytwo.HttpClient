using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace jaytwo.HttpClient
{
    public class HttpResponse
    {
        public HttpRequest Request { get; set; }

        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        public HttpStatusCode StatusCode { get; set; }

        public string Content { get; set; }

        public byte[] BinaryContent { get; set; }

        public long ContentLength { get; set; }

        public string ContentType { get; set; }

        public TimeSpan Elapsed { get; set; }
    }
}
