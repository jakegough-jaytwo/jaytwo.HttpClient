using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using jaytwo.FluentUri;
using jaytwo.HttpClient.Authentication.Basic;
using jaytwo.HttpClient.Authentication.Token;
using jaytwo.MimeHelper;
using Newtonsoft.Json;

namespace jaytwo.HttpClient
{
    public static class HttpClientExtensions
    {
        public static HttpClient WithSystemHttpClient(this HttpClient httpClient, System.Net.Http.HttpClient systemHttpClient)
        {
            httpClient.SystemHttpClient = systemHttpClient;

            return httpClient;
        }

        public static HttpClient WithBaseUri(this HttpClient httpClient, string baseUri)
        {
            return httpClient.WithBaseUri(new Uri(baseUri, UriKind.Absolute));
        }

        public static HttpClient WithBaseUri(this HttpClient httpClient, Uri baseUri)
        {
            httpClient.BaseUri = baseUri;

            return httpClient;
        }

        public static HttpClient WithTimeout(this HttpClient httpClient, TimeSpan? timeout)
        {
            httpClient.Timeout = timeout;

            return httpClient;
        }

        public static HttpClient WithDefaultTimeout(this HttpClient httpClient) => httpClient.WithTimeout(null);

        public static HttpClient WithAuthenticationProvider(this HttpClient httpClient, IAuthenticationProvider authenticationProvider)
        {
            httpClient.AuthenticationProvider = authenticationProvider;

            return httpClient;
        }

        public static HttpClient WithBasicAuthentication(this HttpClient httpClient, string user, string pass)
        {
            return httpClient.WithAuthenticationProvider(new BasicAuthenticationProvider(user, pass));
        }

        public static HttpClient WithTokenAuthentication(this HttpClient httpClient, string token)
        {
            return httpClient.WithAuthenticationProvider(new TokenAuthenticationProvider(token));
        }

        public static HttpClient WithTokenAuthentication(this HttpClient httpClient, Func<string> tokenDelegate)
        {
            return httpClient.WithAuthenticationProvider(new TokenAuthenticationProvider(tokenDelegate));
        }

        public static HttpClient WithTokenAuthentication(this HttpClient httpClient, ITokenProvider tokenProvider)
        {
            return httpClient.WithAuthenticationProvider(new TokenAuthenticationProvider(tokenProvider));
        }

        public static Task<HttpResponse> GetAsync(this HttpClient httpClient, string pathOrUri)
        {
            var uri = new Uri(pathOrUri, UriKind.RelativeOrAbsolute);
            return httpClient.GetAsync(uri);
        }

        public static async Task<HttpResponse> GetAsync(this HttpClient httpClient, Uri uri)
        {
            var request = new HttpRequest()
                .WithMethod(HttpMethod.Get)
                .WithUri(uri);

            var response = await httpClient.SendAsync(request);
            response.EnsureExpectedStatusCode();

            return response;
        }

        public static async Task<string> GetAsStringAsync(this HttpClient httpClient, Uri uri)
        {
            var response = await httpClient.GetAsync(uri);
            return response.Content;
        }

        public static Task<string> GetAsStringAsync(this HttpClient httpClient, string pathOrUri)
        {
            var uri = new Uri(pathOrUri, UriKind.RelativeOrAbsolute);
            return httpClient.GetAsStringAsync(uri);
        }
    }
}
