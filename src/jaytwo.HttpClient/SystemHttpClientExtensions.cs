using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using jaytwo.FluentUri;
using jaytwo.HttpClient.Authentication;
using jaytwo.HttpClient.Authentication.Basic;
#if !NETSTANDARD1_1
using jaytwo.HttpClient.Authentication.OAuth10a;
#endif
using jaytwo.HttpClient.Authentication.Token;
using jaytwo.MimeHelper;
using Newtonsoft.Json;

namespace jaytwo.HttpClient
{
    public static class SystemHttpClientExtensions
    {
        public static System.Net.Http.HttpClient WithBaseUri(this System.Net.Http.HttpClient httpClient, string baseUri)
        {
            return httpClient.WithBaseUri(new Uri(baseUri, UriKind.Absolute));
        }

        public static System.Net.Http.HttpClient WithBaseUri(this System.Net.Http.HttpClient httpClient, Uri baseUri)
        {
            httpClient.BaseAddress = baseUri;

            return httpClient;
        }

        //public static HttpClient WithTimeout(this HttpClient httpClient, TimeSpan? timeout)
        //{
        //    httpClient.Timeout = timeout;

        //    return httpClient;
        //}

        //        public static HttpClient WithDefaultTimeout(this HttpClient httpClient) => httpClient.WithTimeout(null);

        //        public static HttpClient WithAuthenticationProvider(this HttpClient httpClient, Func<IAuthenticationProvider> authenticationProviderDelegate)
        //        {
        //            var authenticationProvider = authenticationProviderDelegate.Invoke();
        //            httpClient.AuthenticationProvider = authenticationProvider;

        //            return httpClient;
        //        }

        //        public static HttpClient WithAuthenticationProvider(this HttpClient httpClient, IAuthenticationProvider authenticationProvider)
        //        {
        //            return httpClient.WithAuthenticationProvider(() => authenticationProvider);
        //        }

        //        public static HttpClient WithBasicAuthentication(this HttpClient httpClient, string user, string pass)
        //        {
        //            return httpClient.WithAuthenticationProvider(new BasicAuthenticationProvider(user, pass));
        //        }

        //        public static HttpClient WithTokenAuthentication(this HttpClient httpClient, string token)
        //        {
        //            return httpClient.WithAuthenticationProvider(new TokenAuthenticationProvider(token));
        //        }

        //        public static HttpClient WithTokenAuthentication(this HttpClient httpClient, Func<string> tokenDelegate)
        //        {
        //            return httpClient.WithAuthenticationProvider(new TokenAuthenticationProvider(tokenDelegate));
        //        }

        //        public static HttpClient WithTokenAuthentication(this HttpClient httpClient, ITokenProvider tokenProvider)
        //        {
        //            return httpClient.WithAuthenticationProvider(new TokenAuthenticationProvider(tokenProvider));
        //        }

        //#if !NETSTANDARD1_1
        //        public static HttpClient WithOAuth10aAuthentication(this HttpClient httpClient, string consumerKey, string consumerSecret)
        //        {
        //            return httpClient.WithAuthenticationProvider(new OAuth10aAuthenticationProvider(consumerKey, consumerSecret));
        //        }

        //        public static HttpClient WithOAuth10aAuthentication(this HttpClient httpClient, string consumerKey, string consumerSecret, string token, string tokenSecret)
        //        {
        //            return httpClient.WithAuthenticationProvider(new OAuth10aAuthenticationProvider(consumerKey, consumerSecret, token, tokenSecret));
        //        }
        //#endif

        //        public static HttpClient WithHttpVersion(this HttpClient httpClient, Version httpVersion)
        //        {
        //            httpClient.HttpVersion = httpVersion;
        //            return httpClient;
        //        }
        public static Task<HttpResponseMessage> DeleteAsync(this System.Net.Http.HttpClient httpClient, Action<HttpRequestMessage> requestBuilder)
                    => SendAsync(httpClient, HttpMethod.Delete, requestBuilder);

        public static Task<HttpResponseMessage> DeleteAsync(this System.Net.Http.HttpClient httpClient, Uri uri)
            => httpClient.DeleteAsync(x => x.WithUri(uri));

        public static Task<HttpResponseMessage> DeleteAsync(this System.Net.Http.HttpClient httpClient, string pathOrUri)
            => httpClient.DeleteAsync(x => x.WithUri(pathOrUri));

        public static Task<HttpResponseMessage> DeleteAsync(this System.Net.Http.HttpClient httpClient, string pathFormat, params string[] formatArgs)
            => httpClient.DeleteAsync(x => x.WithUri(pathFormat, formatArgs));

        public static Task<HttpResponseMessage> DeleteAsync(this System.Net.Http.HttpClient httpClient, string pathFormat, params object[] formatArgs)
            => httpClient.DeleteAsync(x => x.WithUri(pathFormat, formatArgs));

        public static Task<HttpResponseMessage> GetAsync(this System.Net.Http.HttpClient httpClient, Action<HttpRequestMessage> requestBuilder)
            => SendAsync(httpClient, HttpMethod.Get, requestBuilder);

        public static Task<HttpResponseMessage> GetAsync(this System.Net.Http.HttpClient httpClient, Uri uri)
            => httpClient.GetAsync(x => x.WithUri(uri));

        public static Task<HttpResponseMessage> GetAsync(this System.Net.Http.HttpClient httpClient, string pathOrUri)
            => httpClient.GetAsync(x => x.WithUri(pathOrUri));

        public static Task<HttpResponseMessage> GetAsync(this System.Net.Http.HttpClient httpClient, string pathFormat, params string[] formatArgs)
            => httpClient.GetAsync(x => x.WithUri(pathFormat, formatArgs));

        public static Task<HttpResponseMessage> GetAsync(this System.Net.Http.HttpClient httpClient, string pathFormat, params object[] formatArgs)
            => httpClient.GetAsync(x => x.WithUri(pathFormat, formatArgs));

        public static Task<HttpResponseMessage> HeadAsync(this System.Net.Http.HttpClient httpClient, Action<HttpRequestMessage> requestBuilder)
            => SendAsync(httpClient, HttpMethod.Head, requestBuilder);

        public static Task<HttpResponseMessage> HeadAsync(this System.Net.Http.HttpClient httpClient, Uri uri)
            => httpClient.HeadAsync(x => x.WithUri(uri));

        public static Task<HttpResponseMessage> HeadAsync(this System.Net.Http.HttpClient httpClient, string pathOrUri)
            => httpClient.HeadAsync(x => x.WithUri(pathOrUri));

        public static Task<HttpResponseMessage> HeadAsync(this System.Net.Http.HttpClient httpClient, string pathFormat, params string[] formatArgs)
            => httpClient.HeadAsync(x => x.WithUri(pathFormat, formatArgs));

        public static Task<HttpResponseMessage> HeadAsync(this System.Net.Http.HttpClient httpClient, string pathFormat, params object[] formatArgs)
            => httpClient.HeadAsync(x => x.WithUri(pathFormat, formatArgs));

        public static Task<HttpResponseMessage> OptionsAsync(this System.Net.Http.HttpClient httpClient, Action<HttpRequestMessage> requestBuilder)
            => SendAsync(httpClient, HttpMethod.Options, requestBuilder);

        public static Task<HttpResponseMessage> PatchAsync(this System.Net.Http.HttpClient httpClient, Action<HttpRequestMessage> requestBuilder)
            => SendAsync(httpClient, new HttpMethod("PATCH"), requestBuilder);

        public static Task<HttpResponseMessage> PostAsync(this System.Net.Http.HttpClient httpClient, Action<HttpRequestMessage> requestBuilder)
            => SendAsync(httpClient, HttpMethod.Post, requestBuilder);

        public static Task<HttpResponseMessage> PutAsync(this System.Net.Http.HttpClient httpClient, Action<HttpRequestMessage> requestBuilder)
            => SendAsync(httpClient, HttpMethod.Put, requestBuilder);

        public static Task<HttpResponseMessage> TraceAsync(this System.Net.Http.HttpClient httpClient, Action<HttpRequestMessage> requestBuilder)
            => SendAsync(httpClient, HttpMethod.Trace, requestBuilder);

        public static Task<HttpResponseMessage> SendAsync(this System.Net.Http.HttpClient httpClient, Action<HttpRequestMessage> requestBuilder)
        {
            using (var request = new HttpRequestMessage())
            {
                requestBuilder.Invoke(request);
                return httpClient.SendAsync(request);
            }
        }

        public static async Task<byte[]> DownloadByteArrayAsync(this System.Net.Http.HttpClient httpClient, Action<HttpRequestMessage> requestBuilder)
        {
            using (var httpResponse = await SendAsync(httpClient, requestBuilder))
            {
                return await httpResponse.Content.ReadAsByteArrayAsync();
            }
        }

        public static async Task<string> DownloadStringAsync(this System.Net.Http.HttpClient httpClient, Action<HttpRequestMessage> requestBuilder)
        {
            using (var httpResponse = await SendAsync(httpClient, requestBuilder))
            {
                return await httpResponse.Content.ReadAsStringAsync();
            }
        }

        public static async Task<Stream> DownloadStreamAsync(this System.Net.Http.HttpClient httpClient, Action<HttpRequestMessage> requestBuilder)
        {
            var content = await DownloadContentAsync(httpClient, requestBuilder);
            return await content.ReadAsStreamAsync();
        }

        public static async Task<HttpContent> DownloadContentAsync(this System.Net.Http.HttpClient httpClient, Action<HttpRequestMessage> requestBuilder)
        {
            var httpResponse = await SendAsync(httpClient, requestBuilder);
            return httpResponse.Content;
        }

        private static Task<HttpResponseMessage> SendAsync(System.Net.Http.HttpClient httpClient, HttpMethod method, Action<HttpRequestMessage> requestBuilder)
        {
            return httpClient.SendAsync(request =>
            {
                request.WithMethod(method);
                requestBuilder.Invoke(request);
            });
        }
    }
}
