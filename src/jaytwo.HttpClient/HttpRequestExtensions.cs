using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
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
        public static TRequest WithMethod<TRequest>(this TRequest httpRequest, HttpMethod method)
            where TRequest : HttpRequestBase
        {
            httpRequest.Method = method;
            return httpRequest;
        }

        public static TRequest WithMethod<TRequest>(this TRequest httpRequest, string method)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithMethod(new HttpMethod(method));
        }

        public static TRequest WithBaseUri<TRequest>(this TRequest httpRequest, Uri uri)
            where TRequest : HttpRequestBase
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

        public static TRequest WithBaseUri<TRequest>(this TRequest httpRequest, string pathOrUri)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithBaseUri(new Uri(pathOrUri, UriKind.RelativeOrAbsolute));
        }

        public static TRequest WithUri<TRequest>(this TRequest httpRequest, Uri uri)
            where TRequest : HttpRequestBase
        {
            httpRequest.Uri = uri;
            return httpRequest;
        }

        public static TRequest WithUri<TRequest>(this TRequest httpRequest, string pathOrUri)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithUri(new Uri(pathOrUri, UriKind.RelativeOrAbsolute));
        }

        public static TRequest WithUri<TRequest>(this TRequest httpRequest, string pathFormat, params string[] formatArgs)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithUri(Url.Format(pathFormat, formatArgs));
        }

        public static TRequest WithUri<TRequest>(this TRequest httpRequest, string pathFormat, params object[] formatArgs)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithUri(Url.Format(pathFormat, formatArgs));
        }

        public static TRequest WithPath<TRequest>(this TRequest httpRequest, string path)
            where TRequest : HttpRequestBase
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

        public static TRequest WithPath<TRequest>(this TRequest httpRequest, string pathFormat, params string[] formatArgs)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithPath(Url.Format(pathFormat, formatArgs));
        }

        public static TRequest WithPath<TRequest>(this TRequest httpRequest, string pathFormat, params object[] formatArgs)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithPath(Url.Format(pathFormat, formatArgs));
        }

        public static TRequest WithQuery<TRequest>(this TRequest httpRequest, string data)
            where TRequest : HttpRequestBase
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

        public static TRequest WithQuery<TRequest>(this TRequest httpRequest, object data)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithQuery(QueryString.Serialize(data));
        }

        public static TRequest WithQuery<TRequest>(this TRequest httpRequest, IDictionary<string, object> data)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithQuery(QueryString.Serialize(data));
        }

        public static TRequest WithQuery<TRequest>(this TRequest httpRequest, IDictionary<string, string[]> data)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithQuery(QueryString.Serialize(data));
        }

        public static TRequest WithQuery<TRequest>(this TRequest httpRequest, IDictionary<string, string> data)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithQuery(QueryString.Serialize(data));
        }

#if NETFRAMEWORK || NETSTANDARD2
        public static TRequest WithQuery<TRequest>(this TRequest httpRequest, NameValueCollection data)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithQuery(QueryString.Serialize(data));
        }
#endif

        public static TRequest WithQueryParameter<TRequest>(this TRequest httpRequest, string key, string value)
            where TRequest : HttpRequestBase
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

        public static TRequest WithQueryParameter<TRequest>(this TRequest httpRequest, string key, object value)
            where TRequest : HttpRequestBase
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

        public static TRequest WithQueryParameter<TRequest>(this TRequest httpRequest, string key, string[] values)
            where TRequest : HttpRequestBase
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

        public static TRequest WithHeader<TRequest>(this TRequest httpRequest, string key, string value)
            where TRequest : HttpRequestBase
        {
            var headers = httpRequest.Headers ?? new Dictionary<string, string>();
            headers[key] = value;
            httpRequest.Headers = headers;

            return httpRequest;
        }

        public static TRequest WithContentType<TRequest>(this TRequest httpRequest, string contentType)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithHeader(Constants.Headers.ContentType, contentType);
        }

        public static TRequest WithTimeout<TRequest>(this TRequest httpRequest, TimeSpan? timeout)
            where TRequest : HttpRequestBase
        {
            httpRequest.Timeout = timeout;

            return httpRequest;
        }

        public static TRequest WithDefaultTimeout<TRequest>(this TRequest httpRequest)
            where TRequest : HttpRequestBase
            => httpRequest.WithTimeout(null);

        public static TRequest WithExpectedStatusCode<TRequest>(this TRequest httpRequest, HttpStatusCode expectedStatusCode)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithExpectedStatusCodes(expectedStatusCode);
        }

        public static TRequest WithExpectedStatusCodes<TRequest>(this TRequest httpRequest, params HttpStatusCode[] expectedStatusCodes)
            where TRequest : HttpRequestBase
        {
            httpRequest.ExpectedStatusCodes = expectedStatusCodes;

            return httpRequest;
        }

        public static TRequest WithAuthenticationProvider<TRequest>(this TRequest httpRequest, Func<IAuthenticationProvider> authenticationProviderDelegate)
            where TRequest : HttpRequestBase
        {
            var authenticationProvider = authenticationProviderDelegate.Invoke();
            httpRequest.AuthenticationProvider = authenticationProvider;

            return httpRequest;
        }

        public static TRequest WithAuthenticationProvider<TRequest>(this TRequest httpRequest, IAuthenticationProvider authenticationProvider)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithAuthenticationProvider(() => authenticationProvider);
        }

        public static TRequest WithBasicAuthentication<TRequest>(this TRequest httpRequest, string user, string pass)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithAuthenticationProvider(new BasicAuthenticationProvider(user, pass));
        }

#if !NETSTANDARD1_1
        public static TRequest WithDigestAuthentication<TRequest>(this TRequest httpRequest, string user, string pass)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithAuthenticationProvider(new DigestAuthenticationProvider(user, pass));
        }
#endif

        public static TRequest WithTokenAuthentication<TRequest>(this TRequest httpRequest, string token)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithAuthenticationProvider(new TokenAuthenticationProvider(token));
        }

        public static TRequest WithTokenAuthentication<TRequest>(this TRequest httpRequest, Func<string> tokenDelegate)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithAuthenticationProvider(new TokenAuthenticationProvider(tokenDelegate));
        }

        public static TRequest WithTokenAuthentication<TRequest>(this TRequest httpRequest, ITokenProvider tokenProvider)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithAuthenticationProvider(new TokenAuthenticationProvider(tokenProvider));
        }

#if !NETSTANDARD1_1
        public static TRequest WithOAuth10aAuthentication<TRequest>(this TRequest httpRequest, string consumerKey, string consumerSecret)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithAuthenticationProvider(new OAuth10aAuthenticationProvider(consumerKey, consumerSecret));
        }

        public static TRequest WithOAuth10aAuthentication<TRequest>(this TRequest httpRequest, string consumerKey, string consumerSecret, string token, string tokenSecret)
            where TRequest : HttpRequestBase
        {
            return httpRequest.WithAuthenticationProvider(new OAuth10aAuthenticationProvider(consumerKey, consumerSecret, token, tokenSecret));
        }
#endif

        public static TRequest WithJsonContent<TRequest>(this TRequest httpRequest, object data)
            where TRequest : HttpRequestBase
        {
            httpRequest.Content = JsonConvert.SerializeObject(data);
            httpRequest.WithContentType(MediaType.application_json);

            return httpRequest;
        }

        public static TRequest WithFormDataContent<TRequest>(this TRequest httpRequest, object data)
            where TRequest : HttpRequestBase
        {
            httpRequest.Content = QueryString.Serialize(data);
            httpRequest.WithContentType(MediaType.application_x_www_form_urlencoded);

            return httpRequest;
        }

        public static TRequest WithContent<TRequest>(this TRequest httpRequest, byte[] content)
            where TRequest : HttpRequestBase
        {
            httpRequest.Content = content;
            return httpRequest;
        }

        public static TRequest WithContent<TRequest>(this TRequest httpRequest, Stream content)
            where TRequest : HttpRequestBase
        {
            httpRequest.Content = content;
            return httpRequest;
        }

        public static TRequest WithContent<TRequest>(this TRequest httpRequest, HttpContent content)
            where TRequest : HttpRequestBase
        {
            httpRequest.Content = content;
            return httpRequest;
        }

        public static TRequest WithHttpVersion<TRequest>(this TRequest httpRequest, Version httpVersion)
            where TRequest : HttpRequestBase
        {
            httpRequest.HttpVersion = httpVersion;
            return httpRequest;
        }

        public static string GetHeaderValue<TRequest>(this TRequest httpResponse, string key)
            where TRequest : HttpRequestBase
        {
            var header = httpResponse.Headers?.Where(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).ToList();

            if (header.Any())
            {
                return header.First().Value; // TODO: care about headers with multiple values
            }

            return null;
        }

        public static Task<HttpResponse> SendWith<TRequest>(this TRequest httpRequest, Func<TRequest, Task<HttpResponse>> sendDelegate)
            where TRequest : HttpRequestBase
            => sendDelegate.Invoke(httpRequest);

        public static Task<HttpResponse> SendWith<TRequest>(this TRequest httpRequest, IHttpClient httpClient)
            where TRequest : HttpRequestBase
            => httpClient.SendAsync(httpRequest);

        public static Task<HttpResponse> SendWith<TRequest>(this TRequest httpRequest, Func<IHttpClient> httpClientDelegate)
            where TRequest : HttpRequestBase
            => httpClientDelegate.Invoke().SendAsync(httpRequest);
    }
}
