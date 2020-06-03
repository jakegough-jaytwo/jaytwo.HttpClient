using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace jaytwo.HttpClient
{
    public class HttpRequest
    {
        public IAuthenticationProvider AuthenticationProvider { get; set; }

        public Uri Uri { get; set; }

        public TimeSpan? Timeout { get; set; }

        public HttpMethod Method { get; set; }

        public string ContentType { get; set; }

        public string Content { get; set; }

        public byte[] BinaryContent { get; set; }

        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    }
}
