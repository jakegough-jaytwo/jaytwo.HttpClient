using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using jaytwo.MimeHelper;

namespace jaytwo.HttpClient
{
    public class HttpClient : IHttpClient
    {
        private static System.Net.Http.HttpClient _systemHttpClient = new System.Net.Http.HttpClient();

        public HttpClient(string baseUri)
            : this(new Uri(baseUri))
        {
        }

        public HttpClient(Uri baseUri)
        {
            BaseUri = baseUri;
        }

        public static TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(30);

        public IAuthenticationProvider AuthenticationProvider { get; set; }

        public Uri BaseUri { get; set; }

        public TimeSpan? Timeout { get; set; }

        public Task<HttpResponse> SubmitAsync(HttpRequest request) => SubmitAsync(request, CancellationToken.None);

        public async virtual Task<HttpResponse> SubmitAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            request.Uri = GetUri(request.Uri);

            var authenticationProvider = request.AuthenticationProvider ?? AuthenticationProvider;
            authenticationProvider?.Authenticate(request);

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = request.Method,
                RequestUri = request.Uri,
                Content = GetRequestHttpContent(request),
            };

            foreach (var header in request.Headers)
            {
                httpRequestMessage.Headers.Add(header.Key, header.Value);
            }

            var timeout = request.Timeout ?? Timeout ?? DefaultTimeout;
            var stopwatch = Stopwatch.StartNew();
            using (httpRequestMessage)
            using (var httpResponseMessage = await SendWithTimeoutAsync(httpRequestMessage, cancellationToken, timeout))
            {
                stopwatch.Stop();

                var response = new HttpResponse()
                {
                    Request = request,
                    StatusCode = httpResponseMessage.StatusCode,
                    ContentType = httpResponseMessage.Content?.Headers?.ContentType?.ToString(),
                    ContentLength = httpResponseMessage.Content?.Headers?.ContentLength ?? 0,
                    Headers = null, // TODO
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
                        response.BinaryContent = await httpResponseMessage.Content.ReadAsByteArrayAsync();
                    }
                }

                return response;
            }
        }

        private static HttpContent GetRequestHttpContent(HttpRequest request)
        {
            HttpContent content = null;

            if (request.BinaryContent != null)
            {
                content = new ByteArrayContent(request.BinaryContent);
            }
            else if (request.Content != null)
            {
                content = new StringContent(request.Content, Encoding.UTF8, request.ContentType);
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

        private Task<HttpResponseMessage> SendWithTimeoutAsync(
            HttpRequestMessage httpRequestMessage,
            CancellationToken cancellationToken,
            TimeSpan timeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource(timeout))
            using (var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellationTokenSource.Token))
            {
                return _systemHttpClient.SendAsync(
                    request: httpRequestMessage,
                    completionOption: HttpCompletionOption.ResponseContentRead,
                    cancellationToken: combinedCancellationTokenSource.Token);
            }
        }
    }
}
