using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using jaytwo.AsyncHelper;
using jaytwo.FluentUri;
using jaytwo.HttpClient.Authentication;
using jaytwo.HttpClient.Authentication.Basic;
#if !NETSTANDARD1_1
using jaytwo.HttpClient.Authentication.Digest;
#endif
#if !NETSTANDARD1_1
using jaytwo.HttpClient.Authentication.OAuth10a;
#endif
using jaytwo.HttpClient.Authentication.Token;
using jaytwo.MimeHelper;
using jaytwo.UrlHelper;
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

        public static HttpRequest WithMethod(this HttpRequest httpRequest, string method)
        {
            return httpRequest.WithMethod(new HttpMethod(method));
        }

        public static HttpRequest WithBaseUri(this HttpRequest httpRequest, Uri uri)
        {
            if (httpRequest.Uri == null)
            {
                httpRequest.Uri = uri;
            }
            else
            {
                httpRequest.Uri = new Uri(uri, httpRequest.Uri);
            }

            return httpRequest;
        }

        public static HttpRequest WithBaseUri(this HttpRequest httpRequest, string pathOrUri)
        {
            return httpRequest.WithBaseUri(new Uri(pathOrUri, UriKind.RelativeOrAbsolute));
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

        public static HttpRequest WithUri(this HttpRequest httpRequest, string pathFormat, params string[] formatArgs)
        {
            return httpRequest.WithUri(Url.Format(pathFormat, formatArgs));
        }

        public static HttpRequest WithUri(this HttpRequest httpRequest, string pathFormat, params object[] formatArgs)
        {
            return httpRequest.WithUri(Url.Format(pathFormat, formatArgs));
        }

        public static HttpRequest WithPath(this HttpRequest httpRequest, string path)
        {
            if (httpRequest.Uri != null)
            {
                return httpRequest.WithUri(httpRequest.Uri.WithPath(path));
            }
            else
            {
                return httpRequest.WithUri(path);
            }
        }

        public static HttpRequest WithPath(this HttpRequest httpRequest, string pathFormat, params string[] formatArgs)
        {
            return httpRequest.WithPath(Url.Format(pathFormat, formatArgs));
        }

        public static HttpRequest WithPath(this HttpRequest httpRequest, string pathFormat, params object[] formatArgs)
        {
            return httpRequest.WithPath(Url.Format(pathFormat, formatArgs));
        }

        public static HttpRequest WithQuery(this HttpRequest httpRequest, string data)
        {
            if (httpRequest.Uri != null)
            {
                return httpRequest.WithUri(httpRequest.Uri.WithQuery(data));
            }
            else
            {
                return httpRequest.WithUri("?" + data.TrimStart('?'));
            }
        }

        public static HttpRequest WithQuery(this HttpRequest httpRequest, object data)
        {
            return httpRequest.WithQuery(QueryString.Serialize(data));
        }

        public static HttpRequest WithQuery(this HttpRequest httpRequest, IDictionary<string, object> data)
        {
            return httpRequest.WithQuery(QueryString.Serialize(data));
        }

        public static HttpRequest WithQuery(this HttpRequest httpRequest, IDictionary<string, string[]> data)
        {
            return httpRequest.WithQuery(QueryString.Serialize(data));
        }

        public static HttpRequest WithQuery(this HttpRequest httpRequest, IDictionary<string, string> data)
        {
            return httpRequest.WithQuery(QueryString.Serialize(data));
        }

#if NETFRAMEWORK || NETSTANDARD2
        public static HttpRequest WithQuery(this HttpRequest httpRequest, NameValueCollection data)
        {
            return httpRequest.WithQuery(QueryString.Serialize(data));
        }
#endif

        public static HttpRequest WithQueryParameter(this HttpRequest httpRequest, string key, string value)
        {
            if (httpRequest.Uri != null)
            {
                return httpRequest.WithUri(httpRequest.Uri.WithQueryParameter(key, value));
            }
            else
            {
                return httpRequest.WithQuery(new Dictionary<string, string>() { { key, value } });
            }
        }

        public static HttpRequest WithQueryParameter(this HttpRequest httpRequest, string key, object value)
        {
            if (httpRequest.Uri != null)
            {
                return httpRequest.WithUri(httpRequest.Uri.WithQueryParameter(key, value));
            }
            else
            {
                return httpRequest.WithQuery(new Dictionary<string, object>() { { key, value } });
            }
        }

        public static HttpRequest WithQueryParameter(this HttpRequest httpRequest, string key, string[] values)
        {
            if (httpRequest.Uri != null)
            {
                return httpRequest.WithUri(httpRequest.Uri.WithQueryParameter(key, values));
            }
            else
            {
                return httpRequest.WithQuery(new Dictionary<string, string[]>() { { key, values } });
            }
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

        public static HttpRequest WithExpectedStatusCode(this HttpRequest httpRequest, HttpStatusCode expectedStatusCode)
        {
            return httpRequest.WithExpectedStatusCodes(expectedStatusCode);
        }

        public static HttpRequest WithExpectedStatusCodes(this HttpRequest httpRequest, params HttpStatusCode[] expectedStatusCodes)
        {
            httpRequest.ExpectedStatusCodes = expectedStatusCodes;

            return httpRequest;
        }

        public static HttpRequest WithAuthenticationProvider(this HttpRequest httpRequest, Func<IAuthenticationProvider> authenticationProviderDelegate)
        {
            var authenticationProvider = authenticationProviderDelegate.Invoke();
            httpRequest.AuthenticationProvider = authenticationProvider;

            return httpRequest;
        }

        public static HttpRequest WithAuthenticationProvider(this HttpRequest httpRequest, IAuthenticationProvider authenticationProvider)
        {
            return httpRequest.WithAuthenticationProvider(() => authenticationProvider);
        }

        public static HttpRequest WithBasicAuthentication(this HttpRequest httpRequest, string user, string pass)
        {
            return httpRequest.WithAuthenticationProvider(new BasicAuthenticationProvider(user, pass));
        }

#if !NETSTANDARD1_1
        public static HttpRequest WithDigestAuthentication(this HttpRequest httpRequest, string user, string pass)
        {
            return httpRequest.WithAuthenticationProvider(new DigestAuthenticationProvider(user, pass));
        }
#endif

        public static HttpRequest WithTokenAuthentication(this HttpRequest httpRequest, string token)
        {
            return httpRequest.WithAuthenticationProvider(new TokenAuthenticationProvider(token));
        }

        public static HttpRequest WithTokenAuthentication(this HttpRequest httpRequest, Func<string> tokenDelegate)
        {
            return httpRequest.WithAuthenticationProvider(new TokenAuthenticationProvider(tokenDelegate));
        }

        public static HttpRequest WithTokenAuthentication(this HttpRequest httpRequest, ITokenProvider tokenProvider)
        {
            return httpRequest.WithAuthenticationProvider(new TokenAuthenticationProvider(tokenProvider));
        }

#if !NETSTANDARD1_1
        public static HttpRequest WithOAuth10aAuthentication(this HttpRequest httpRequest, string consumerKey, string consumerSecret)
        {
            return httpRequest.WithAuthenticationProvider(new OAuth10aAuthenticationProvider(consumerKey, consumerSecret));
        }

        public static HttpRequest WithOAuth10aAuthentication(this HttpRequest httpRequest, string consumerKey, string consumerSecret, string token, string tokenSecret)
        {
            return httpRequest.WithAuthenticationProvider(new OAuth10aAuthenticationProvider(consumerKey, consumerSecret, token, tokenSecret));
        }
#endif

        public static HttpRequest WithJsonContent(this HttpRequest httpRequest, object data)
        {
            httpRequest.Content = JsonConvert.SerializeObject(data);
            httpRequest.ContentType = MediaType.application_json;

            return httpRequest;
        }

        public static HttpRequest WithFormDataContent(this HttpRequest httpRequest, object data)
        {
            httpRequest.Content = QueryString.Serialize(data);
            httpRequest.ContentType = MediaType.application_x_www_form_urlencoded;

            return httpRequest;
        }

        public static HttpRequest WithContent(this HttpRequest httpRequest, HttpContent content)
        {
            httpRequest.ContentType = content.Headers.ContentType.ToString();

            if (ContentTypeEvaluator.IsStringContent(content))
            {
                httpRequest.BinaryContent = content.ReadAsByteArrayAsync().AwaitSynchronously();
            }
            else
            {
                httpRequest.Content = content.ReadAsStringAsync().AwaitSynchronously();
            }

            return httpRequest;
        }

        public static HttpRequest WithHttpVersion(this HttpRequest httpRequest, Version httpVersion)
        {
            httpRequest.HttpVersion = httpVersion;
            return httpRequest;
        }

        public static Task<HttpResponse> SendWith(this HttpRequest httpRequest, Func<HttpRequest, Task<HttpResponse>> sendDelegate) => sendDelegate.Invoke(httpRequest);

        public static Task<HttpResponse> SendWith(this HttpRequest httpRequest, IHttpClient httpClient) => httpClient.SendAsync(httpRequest);

        public static Task<HttpResponse> SendWith(this HttpRequest httpRequest, Func<IHttpClient> httpClientDelegate) => httpClientDelegate.Invoke().SendAsync(httpRequest);
    }
}
