using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using jaytwo.FluentUri;
using jaytwo.MimeHelper;
using Newtonsoft.Json;

namespace jaytwo.HttpClient
{
    public static class HttpRequestExtensions
    {
        public static HttpRequest WithMethod(this HttpRequest httpRequest, HttpMethod method)
        {
            httpRequest.Method = method;
            return httpRequest;
        }

        public static HttpRequest WithUri(this HttpRequest httpRequest, Uri uri)
        {
            httpRequest.Uri = uri;
            return httpRequest;
        }

        public static HttpRequest WithUri(this HttpRequest httpRequest, string pathOrUri)
        {
            return httpRequest.WithUri(new Uri(pathOrUri, UriKind.RelativeOrAbsolute));
        }

        public static HttpRequest WithPath(this HttpRequest httpRequest, string path)
        {
            httpRequest.Uri = httpRequest.Uri?.WithPath(path);
            return httpRequest;
        }

        public static HttpRequest WithQuery(this HttpRequest httpRequest, object data)
        {
            httpRequest.Uri = httpRequest.Uri?.WithQuery(data);
            return httpRequest;
        }

        public static HttpRequest WithQuery(this HttpRequest httpRequest, IDictionary<string, object> data)
        {
            httpRequest.Uri = httpRequest.Uri?.WithQuery(data);
            return httpRequest;
        }

        public static HttpRequest WithQuery(this HttpRequest httpRequest, IDictionary<string, string[]> data)
        {
            httpRequest.Uri = httpRequest.Uri?.WithQuery(data);
            return httpRequest;
        }

        public static HttpRequest WithQuery(this HttpRequest httpRequest, IDictionary<string, string> data)
        {
            httpRequest.Uri = httpRequest.Uri?.WithQuery(data);
            return httpRequest;
        }

#if NETFRAMEWORK || NETSTANDARD2
        public static HttpRequest WithQuery(this HttpRequest httpRequest, NameValueCollection data)
        {
            httpRequest.Uri = httpRequest.Uri?.WithQuery(data);
            return httpRequest;
        }
#endif

        public static HttpRequest WithQuery(this HttpRequest httpRequest, string data)
        {
            httpRequest.Uri = httpRequest.Uri?.WithQuery(data);
            return httpRequest;
        }

        public static HttpRequest WithQueryParameter(this HttpRequest httpRequest, string key, string value)
        {
            httpRequest.Uri = httpRequest.Uri?.WithQueryParameter(key, value);
            return httpRequest;
        }

        public static HttpRequest WithQueryParameter(this HttpRequest httpRequest, string key, object value)
        {
            httpRequest.Uri = httpRequest.Uri?.WithQueryParameter(key, value);
            return httpRequest;
        }

        public static HttpRequest WithQueryParameter(this HttpRequest httpRequest, string key, string[] values)
        {
            httpRequest.Uri = httpRequest.Uri?.WithQueryParameter(key, values);
            return httpRequest;
        }

        public static HttpRequest WithHeader(this HttpRequest httpRequest, string key, string value)
        {
            var headers = httpRequest.Headers ?? new Dictionary<string, string>();
            headers[key] = value;
            httpRequest.Headers = headers;

            return httpRequest;
        }

        public static HttpRequest WithContentType(this HttpRequest httpRequest, string contentType)
        {
            httpRequest.ContentType = contentType;

            return httpRequest;
        }

        public static HttpRequest WithTimeout(this HttpRequest httpRequest, TimeSpan? timeout)
        {
            httpRequest.Timeout = timeout;

            return httpRequest;
        }

        public static HttpRequest WithDefaultTimeout(this HttpRequest httpRequest) => httpRequest.WithTimeout(null);

        public static HttpRequest WithAuthenticationProvider(this HttpRequest httpRequest, IAuthenticationProvider authenticationProvider)
        {
            httpRequest.AuthenticationProvider = authenticationProvider;

            return httpRequest;
        }

        public static HttpRequest WithBasicAuthentication(this HttpRequest httpRequest, string user, string pass)
        {
            return httpRequest.WithAuthenticationProvider(new BasicAuthenticationProvider(user, pass));
        }

        public static HttpRequest WithJsonContent(this HttpRequest httpRequest, object data)
        {
            httpRequest.Content = JsonConvert.SerializeObject(data);
            httpRequest.ContentType = MediaType.application_json;

            return httpRequest;
        }

        public static HttpRequest WithFormDataContent(this HttpRequest httpRequest, object data)
        {
            httpRequest.Content = QueryStringUtility.GetQueryString(data);
            httpRequest.ContentType = MediaType.application_x_www_form_urlencoded;

            return httpRequest;
        }
    }
}
