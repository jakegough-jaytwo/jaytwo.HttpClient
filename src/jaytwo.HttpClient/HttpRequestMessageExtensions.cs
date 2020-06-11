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
    public static class HttpRequestMessageExtensions
    {
        public static HttpRequestMessage WithMethod(this HttpRequestMessage httpRequest, HttpMethod method)
        {
            httpRequest.Method = method;
            return httpRequest;
        }

        public static HttpRequestMessage WithMethod(this HttpRequestMessage httpRequest, string method)
        {
            return httpRequest.WithMethod(new HttpMethod(method));
        }

        public static HttpRequestMessage WithBaseUri(this HttpRequestMessage httpRequest, Uri uri)
        {
            if (httpRequest.RequestUri == null)
            {
                httpRequest.RequestUri = uri;
            }
            else
            {
                httpRequest.RequestUri = new Uri(uri, httpRequest.RequestUri);
            }

            return httpRequest;
        }

        public static HttpRequestMessage WithBaseUri(this HttpRequestMessage httpRequest, string pathOrUri)
        {
            return httpRequest.WithBaseUri(new Uri(pathOrUri, UriKind.RelativeOrAbsolute));
        }

        public static HttpRequestMessage WithUri(this HttpRequestMessage httpRequest, Uri uri)
        {
            httpRequest.RequestUri = uri;
            return httpRequest;
        }

        public static HttpRequestMessage WithUri(this HttpRequestMessage httpRequest, string pathOrUri)
        {
            return httpRequest.WithUri(new Uri(pathOrUri, UriKind.RelativeOrAbsolute));
        }

        public static HttpRequestMessage WithUri(this HttpRequestMessage httpRequest, string pathFormat, params string[] formatArgs)
        {
            return httpRequest.WithUri(Url.Format(pathFormat, formatArgs));
        }

        public static HttpRequestMessage WithUri(this HttpRequestMessage httpRequest, string pathFormat, params object[] formatArgs)
        {
            return httpRequest.WithUri(Url.Format(pathFormat, formatArgs));
        }

        public static HttpRequestMessage WithPath(this HttpRequestMessage httpRequest, string path)
        {
            if (httpRequest.RequestUri != null)
            {
                return httpRequest.WithUri(httpRequest.RequestUri.WithPath(path));
            }
            else
            {
                return httpRequest.WithUri(path);
            }
        }

        public static HttpRequestMessage WithPath(this HttpRequestMessage httpRequest, string pathFormat, params string[] formatArgs)
        {
            return httpRequest.WithPath(Url.Format(pathFormat, formatArgs));
        }

        public static HttpRequestMessage WithPath(this HttpRequestMessage httpRequest, string pathFormat, params object[] formatArgs)
        {
            return httpRequest.WithPath(Url.Format(pathFormat, formatArgs));
        }

        public static HttpRequestMessage WithQuery(this HttpRequestMessage httpRequest, string data)
        {
            if (httpRequest.RequestUri != null)
            {
                return httpRequest.WithUri(httpRequest.RequestUri.WithQuery(data));
            }
            else
            {
                return httpRequest.WithUri("?" + data.TrimStart('?'));
            }
        }

        public static HttpRequestMessage WithQuery(this HttpRequestMessage httpRequest, object data)
        {
            return httpRequest.WithQuery(QueryString.Serialize(data));
        }

        public static HttpRequestMessage WithQuery(this HttpRequestMessage httpRequest, IDictionary<string, object> data)
        {
            return httpRequest.WithQuery(QueryString.Serialize(data));
        }

        public static HttpRequestMessage WithQuery(this HttpRequestMessage httpRequest, IDictionary<string, string[]> data)
        {
            return httpRequest.WithQuery(QueryString.Serialize(data));
        }

        public static HttpRequestMessage WithQuery(this HttpRequestMessage httpRequest, IDictionary<string, string> data)
        {
            return httpRequest.WithQuery(QueryString.Serialize(data));
        }

#if NETFRAMEWORK || NETSTANDARD2
        public static HttpRequestMessage WithQuery(this HttpRequestMessage httpRequest, NameValueCollection data)
        {
            return httpRequest.WithQuery(QueryString.Serialize(data));
        }
#endif

        public static HttpRequestMessage WithQueryParameter(this HttpRequestMessage httpRequest, string key, string value)
        {
            if (httpRequest.RequestUri != null)
            {
                return httpRequest.WithUri(httpRequest.RequestUri.WithQueryParameter(key, value));
            }
            else
            {
                return httpRequest.WithQuery(new Dictionary<string, string>() { { key, value } });
            }
        }

        public static HttpRequestMessage WithQueryParameter(this HttpRequestMessage httpRequest, string key, object value)
        {
            if (httpRequest.RequestUri != null)
            {
                return httpRequest.WithUri(httpRequest.RequestUri.WithQueryParameter(key, value));
            }
            else
            {
                return httpRequest.WithQuery(new Dictionary<string, object>() { { key, value } });
            }
        }

        public static HttpRequestMessage WithQueryParameter(this HttpRequestMessage httpRequest, string key, string[] values)
        {
            if (httpRequest.RequestUri != null)
            {
                return httpRequest.WithUri(httpRequest.RequestUri.WithQueryParameter(key, values));
            }
            else
            {
                return httpRequest.WithQuery(new Dictionary<string, string[]>() { { key, values } });
            }
        }

        public static HttpRequestMessage WithHeader(this HttpRequestMessage httpRequest, string key, string value)
        {
            httpRequest.Headers.TryAddWithoutValidation(key, value);
            return httpRequest;
        }

        public static HttpRequestMessage WithContentType(this HttpRequestMessage httpRequest, string contentType)
        {
            return httpRequest.WithHeader(Constants.Headers.ContentType, contentType);
        }

        //public static HttpRequest WithTimeout(this HttpRequest httpRequest, TimeSpan? timeout)
        //{
        //    httpRequest.Timeout = timeout;

        //    return httpRequest;
        //}

        //public static HttpRequest WithDefaultTimeout(this HttpRequest httpRequest) => httpRequest.WithTimeout(null);

        //public static HttpRequest WithExpectedStatusCode(this HttpRequest httpRequest, HttpStatusCode expectedStatusCode)
        //{
        //    return httpRequest.WithExpectedStatusCodes(expectedStatusCode);
        //}

        //public static HttpRequest WithExpectedStatusCodes(this HttpRequest httpRequest, params HttpStatusCode[] expectedStatusCodes)
        //{
        //    httpRequest.ExpectedStatusCodes = expectedStatusCodes;

        //    return httpRequest;
        //}

        //public static HttpRequest WithAuthenticationProvider(this HttpRequest httpRequest, Func<IAuthenticationProvider> authenticationProviderDelegate)
        //{
        //    var authenticationProvider = authenticationProviderDelegate.Invoke();
        //    httpRequest.AuthenticationProvider = authenticationProvider;

        //    return httpRequest;
        //}

        //public static HttpRequest WithAuthenticationProvider(this HttpRequest httpRequest, IAuthenticationProvider authenticationProvider)
        //{
        //    return httpRequest.WithAuthenticationProvider(() => authenticationProvider);
        //}

        //        public static HttpRequest WithBasicAuthentication(this HttpRequest httpRequest, string user, string pass)
        //        {
        //            return httpRequest.WithAuthenticationProvider(new BasicAuthenticationProvider(user, pass));
        //        }

        //#if !NETSTANDARD1_1
        //        public static HttpRequest WithDigestAuthentication(this HttpRequest httpRequest, string user, string pass)
        //        {
        //            return httpRequest.WithAuthenticationProvider(new DigestAuthenticationProvider(user, pass));
        //        }
        //#endif

        //        public static HttpRequest WithTokenAuthentication(this HttpRequest httpRequest, string token)
        //        {
        //            return httpRequest.WithAuthenticationProvider(new TokenAuthenticationProvider(token));
        //        }

        //        public static HttpRequest WithTokenAuthentication(this HttpRequest httpRequest, Func<string> tokenDelegate)
        //        {
        //            return httpRequest.WithAuthenticationProvider(new TokenAuthenticationProvider(tokenDelegate));
        //        }

        //        public static HttpRequest WithTokenAuthentication(this HttpRequest httpRequest, ITokenProvider tokenProvider)
        //        {
        //            return httpRequest.WithAuthenticationProvider(new TokenAuthenticationProvider(tokenProvider));
        //        }

        //#if !NETSTANDARD1_1
        //        public static HttpRequest WithOAuth10aAuthentication(this HttpRequest httpRequest, string consumerKey, string consumerSecret)
        //        {
        //            return httpRequest.WithAuthenticationProvider(new OAuth10aAuthenticationProvider(consumerKey, consumerSecret));
        //        }

        //        public static HttpRequest WithOAuth10aAuthentication(this HttpRequest httpRequest, string consumerKey, string consumerSecret, string token, string tokenSecret)
        //        {
        //            return httpRequest.WithAuthenticationProvider(new OAuth10aAuthenticationProvider(consumerKey, consumerSecret, token, tokenSecret));
        //        }
        //#endif

        public static HttpRequestMessage WithJsonContent(this HttpRequestMessage httpRequest, object data)
        {
            httpRequest.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, MediaType.application_json);
            return httpRequest;
        }

        public static HttpRequestMessage WithFormDataContent(this HttpRequestMessage httpRequest, object data)
        {
            httpRequest.Content = new StringContent(QueryString.Serialize(data), Encoding.UTF8, MediaType.application_x_www_form_urlencoded);
            return httpRequest;
        }

        public static HttpRequestMessage WithContent(this HttpRequestMessage httpRequest, HttpContent content)
        {
            httpRequest.Content = content;
            return httpRequest;
        }

        public static HttpRequestMessage WithHttpVersion(this HttpRequestMessage httpRequest, Version httpVersion)
        {
            httpRequest.Version = httpVersion;
            return httpRequest;
        }

        //public static string GetHeaderValue(this HttpRequestMessage httpResponse, string key)
        //{
        //    var header = httpResponse.Headers?.Where(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).ToList();

        //    if (header.Any())
        //    {
        //        return header.Single().Value;
        //    }

        //    return null;
        //}

        public static Task<HttpResponseMessage> SendWith(this HttpRequestMessage httpRequest, Func<HttpRequestMessage, Task<HttpResponseMessage>> sendDelegate) => sendDelegate.Invoke(httpRequest);

        public static Task<HttpResponseMessage> SendWith(this HttpRequestMessage httpRequest, System.Net.Http.HttpClient httpClient) => httpClient.SendAsync(httpRequest);

        public static Task<HttpResponseMessage> SendWith(this HttpRequestMessage httpRequest, Func<System.Net.Http.HttpClient> httpClientDelegate) => httpClientDelegate.Invoke().SendAsync(httpRequest);
    }
}
