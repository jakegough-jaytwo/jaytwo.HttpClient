using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using jaytwo.HttpClient.Exceptions;
using Newtonsoft.Json;

namespace jaytwo.HttpClient
{
    public static class HttpResponseExtensions
    {
        public static bool IsExpectedStatusCode(this HttpResponse httpResponse)
        {
            if (httpResponse?.Request?.ExpectedStatusCodes?.Any() ?? false)
            {
                return httpResponse.Request.ExpectedStatusCodes.Contains(httpResponse.StatusCode);
            }
            else
            {
                return (int)httpResponse.StatusCode >= 200 && (int)httpResponse.StatusCode < 300;
            }
        }

        public static HttpResponse EnsureExpectedStatusCode(this HttpResponse httpResponse)
        {
            if (!IsExpectedStatusCode(httpResponse))
            {
                throw new UnexpectedStatusCodeException(httpResponse.StatusCode, httpResponse);
            }

            return httpResponse;
        }

        public static string AsString(this HttpResponse httpResponse)
        {
            // TODO: return string even if the content gets mapped to BinaryContent
            return httpResponse.Content;
        }

        public static async Task<string> AsString(this Task<HttpResponse> httpResponseTask)
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.AsString();
        }

        public static T AsAnonymousType<T>(this HttpResponse httpResponse, T anonymousTypeObject)
        {
            var asString = httpResponse.AsString();
            return JsonConvert.DeserializeAnonymousType(asString, anonymousTypeObject);
        }

        public static async Task<T> AsAnonymousType<T>(this Task<HttpResponse> httpResponseTask, T anonymousTypeObject)
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.AsAnonymousType<T>(anonymousTypeObject);
        }

        public static T As<T>(this HttpResponse httpResponse)
        {
            var asString = httpResponse.AsString();
            return JsonConvert.DeserializeObject<T>(asString);
        }

        public static async Task<T> As<T>(this Task<HttpResponse> httpResponseTask)
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.As<T>();
        }

        public static T ParseWith<T>(this HttpResponse httpResponse, Func<HttpResponse, T> parseDelegate)
        {
            return parseDelegate.Invoke(httpResponse);
        }

        public static async Task<T> ParseWith<T>(this Task<HttpResponse> httpResponseTask, Func<HttpResponse, T> parseDelegate)
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.ParseWith<T>(parseDelegate);
        }

        public static T ParseWith<T>(this HttpResponse httpResponse, Func<string, T> parseDelegate)
        {
            var asString = httpResponse.AsString();
            return parseDelegate.Invoke(asString);
        }

        public static async Task<T> ParseWith<T>(this Task<HttpResponse> httpResponseTask, Func<string, T> parseDelegate)
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.ParseWith<T>(parseDelegate);
        }
    }
}
