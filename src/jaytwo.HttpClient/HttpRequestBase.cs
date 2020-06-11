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
    public class HttpRequestBase
    {
        public HttpRequestBase()
        {
        }

        public HttpRequestBase(string pathOrUri)
        {
            this.WithUri(pathOrUri);
        }

        public HttpRequestBase(Uri uri)
        {
            this.WithUri(uri);
        }

        public HttpRequestBase(HttpMethod method)
        {
            this.WithMethod(method);
        }

        public HttpRequestBase(HttpMethod method, string uri)
        {
            this.WithMethod(method).WithUri(uri);
        }

        public HttpRequestBase(HttpMethod method, Uri uri)
        {
            this.WithMethod(method).WithUri(uri);
        }

        public IAuthenticationProvider AuthenticationProvider { get; set; }

        public Uri Uri { get; set; }

        public TimeSpan? Timeout { get; set; }

        public HttpMethod Method { get; set; }

        public string ContentType => this.GetHeaderValue(Constants.Headers.ContentType);

        public object Content { get; set; }

        public Version HttpVersion { get; set; }

        public IList<HttpStatusCode> ExpectedStatusCodes { get; set; }

        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    }
}
