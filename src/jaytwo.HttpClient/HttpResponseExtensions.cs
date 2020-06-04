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

        public static T AsAnonymousType<T>(this HttpResponse httpResponse, T anonymousTypeObject)
        {
            return JsonConvert.DeserializeAnonymousType(httpResponse.Content, anonymousTypeObject);
        }

        public static async Task<T> AsAnonymousType<T>(this Task<HttpResponse> httpResponseTask, T anonymousTypeObject)
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.AsAnonymousType<T>(anonymousTypeObject);
        }

        public static T As<T>(this HttpResponse httpResponse)
        {
            return JsonConvert.DeserializeObject<T>(httpResponse.Content);
        }

        public static async Task<T> As<T>(this Task<HttpResponse> httpResponseTask)
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.As<T>();
        }
    }
}
