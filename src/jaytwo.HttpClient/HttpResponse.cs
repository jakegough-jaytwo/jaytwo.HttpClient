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

        public object Content { get; set; }

        public long ContentLength => Convert.ToInt64(this.GetHeaderValue(Constants.Headers.ContentLength));

        public string ContentType => this.GetHeaderValue(Constants.Headers.ContentType);

        public TimeSpan Elapsed { get; set; }
    }
}
