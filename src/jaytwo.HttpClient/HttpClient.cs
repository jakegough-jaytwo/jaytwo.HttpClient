using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using jaytwo.HttpClient.Authentication;
using jaytwo.HttpClient.Exceptions;

namespace jaytwo.HttpClient
{
    public class HttpClient : IHttpClient
    {
        private static System.Net.Http.HttpClient _defaultSystemHttpClient = new System.Net.Http.HttpClient();

        public HttpClient()
        {
        }

        public HttpClient(System.Net.Http.HttpClient systemHttpClient)
        {
            this.WithSystemHttpClient(systemHttpClient);
        }

        public HttpClient(string baseUri)
        {
            this.WithBaseUri(baseUri);
        }

        public HttpClient(Uri baseUri)
        {
            this.WithBaseUri(baseUri);
        }

        public static TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(30);

        public static HttpClient Default { get; } = new HttpClient();

        public System.Net.Http.HttpClient SystemHttpClient { get; internal set; }

        public IAuthenticationProvider AuthenticationProvider { get; set; }

        public Uri BaseUri { get; set; }

        public TimeSpan? Timeout { get; set; }

        public Version HttpVersion { get; set; } = new Version(1, 1);

        public Task<HttpResponse> SendAsync<TRequest>(TRequest request)
            where TRequest : HttpRequestBase
            => SendAsync(request, CancellationToken.None);

        public async virtual Task<HttpResponse> SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
            where TRequest : HttpRequestBase
        {
            if (request.Method == null)
            {
                throw new ArgumentException($"{nameof(request)}.{nameof(request.Method)} is required.");
            }

            request.Uri = GetUri(request.Uri);
            if (request.Method == null)
            {
                throw new ArgumentException($"{nameof(request)}.{nameof(Uri)} is required.");
            }

            var authenticationProvider = request.AuthenticationProvider ?? AuthenticationProvider;
            if (authenticationProvider != null)
            {
                await authenticationProvider.AuthenticateAsync(request);
            }

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = request.Method,
                RequestUri = request.Uri,
                Content = GetRequestHttpContent(request),
            };

            var httpVersion = request.HttpVersion ?? HttpVersion;
            if (httpVersion != null)
            {
                httpRequestMessage.Version = httpVersion;
            }

            foreach (var header in request.Headers)
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            var timeout = request.Timeout ?? Timeout ?? DefaultTimeout;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using (var httpResponseMessage = await SendWithTimeoutAsync(httpRequestMessage, cancellationToken, timeout))
                {
                    stopwatch.Stop();

                    var response = new HttpResponse()
                    {
                        Request = request,
                        StatusCode = httpResponseMessage.StatusCode,
                        Headers = GetHeaders(httpResponseMessage),
                        Elapsed = stopwatch.Elapsed,
                    };

                    if (httpResponseMessage.Content != null)
                    {
                        if (ContentTypeEvaluator.IsStringContent(httpResponseMessage.Content))
                        {
                            response.Content = await httpResponseMessage.Content.ReadAsStringAsync();
                        }
                        else
                        {
                            response.ContentBytes = await httpResponseMessage.Content.ReadAsByteArrayAsync();
                        }
                    }

                    response.EnsureExpectedStatusCode();

                    return response;
                }
            }
            finally
            {
                if (!(request is IDisposable))
                {
                    httpRequestMessage.Dispose();
                }
            }
        }

        private static IDictionary<string, string> GetHeaders(HttpResponseMessage httpResponseMessage)
        {
            var result = new Dictionary<string, string>();

            foreach (var header in httpResponseMessage.Headers)
            {
                result.Add(header.Key, header.Value.First()); // TODO: care about multiple headers with the same key
            }

            foreach (var header in httpResponseMessage?.Content.Headers)
            {
                result.Add(header.Key, header.Value.First()); // TODO: care about multiple headers with the same key
            }

            return result;
        }

        private static HttpContent GetRequestHttpContent<TRequest>(TRequest request)
            where TRequest : HttpRequestBase
        {
            HttpContent content = null;

            if (request.Content is string)
            {
                content = new StringContent((string)request.Content, Encoding.UTF8, request.ContentType);
            }
            else if (request.Content is byte[])
            {
                content = new ByteArrayContent((byte[])request.Content);
            }
            else if (request.Content is Stream)
            {
                content = new StreamContent((Stream)request.Content);
            }
            else if (request.Content is HttpContent)
            {
                content = (HttpContent)request.Content;
            }
            else if (request.Content != null)
            {
                throw new NotSupportedException($"Unsupported content type conversion from: {request.Content.GetType()}.  Content must be either string, byte[], Stream, or HttpContent.");
            }

            return content;
        }

        private Uri GetUri(Uri requestUri)
        {
            if (BaseUri != null && !requestUri.IsAbsoluteUri)
            {
                return new Uri(BaseUri, requestUri);
            }
            else
            {
                return requestUri;
            }
        }

        private async Task<HttpResponseMessage> SendWithTimeoutAsync(
            HttpRequestMessage httpRequestMessage,
            CancellationToken cancellationToken,
            TimeSpan timeout)
        {
            var systemHttpClient = SystemHttpClient ?? _defaultSystemHttpClient;

            using (var timeoutCancellationTokenSource = new CancellationTokenSource(timeout))
            using (var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellationTokenSource.Token))
            {
                try
                {
                    var response = await systemHttpClient.SendAsync(
                        request: httpRequestMessage,
                        completionOption: HttpCompletionOption.ResponseContentRead,
                        cancellationToken: timeoutCancellationTokenSource.Token);

                    return response;
                }
                catch (TaskCanceledException taskCanceledException) when (timeoutCancellationTokenSource.IsCancellationRequested)
                {
                    throw new RequestTimedOutException(taskCanceledException);
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}
