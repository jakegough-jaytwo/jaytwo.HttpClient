using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace jaytwo.HttpClient
{
    public static class HttpResponseExtensions
    {
        public static HttpResponse EnsureSuccessStatusCode(this HttpResponse httpResponse)
        {
            if ((int)httpResponse.StatusCode < 200 || (int)httpResponse.StatusCode >= 300)
            {
                throw new HttpRequestException($"Unsuccessful status code: {(int)httpResponse.StatusCode} ({httpResponse.StatusCode})");
            }

            return httpResponse;
        }

        public static T AsAnonymousType<T>(this HttpResponse httpResponse, T anonymousTypeObject)
        {
            return JsonConvert.DeserializeAnonymousType(httpResponse.Content, anonymousTypeObject);
        }

        public static T As<T>(this HttpResponse httpResponse)
        {
            return JsonConvert.DeserializeObject<T>(httpResponse.Content);
        }
    }
}
