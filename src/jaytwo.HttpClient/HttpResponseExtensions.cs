using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using jaytwo.HttpClient.Authentication.Token.OpenIdConnect;
using jaytwo.HttpClient.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace jaytwo.HttpClient
{
    public static class HttpResponseExtensions
    {
        public static bool IsExpectedStatusCode<TRequest>(this HttpResponse<TRequest> httpResponse)
            where TRequest : HttpRequestBase
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

        public static HttpResponse<TRequest> EnsureExpectedStatusCode<TRequest>(this HttpResponse<TRequest> httpResponse)
            where TRequest : HttpRequestBase
        {
            if (!IsExpectedStatusCode(httpResponse))
            {
                throw new UnexpectedStatusCodeException(httpResponse.StatusCode, httpResponse);
            }

            return httpResponse;
        }

        public static string AsString<TRequest>(this HttpResponse<TRequest> httpResponse)
            where TRequest : HttpRequestBase
        {
            if (httpResponse.Content != null)
            {
                return httpResponse.Content;
            }
            else if (httpResponse.ContentBytes != null)
            {
                // TODO: read the headers in case a different encoding is defined
                return Encoding.UTF8.GetString(httpResponse.ContentBytes, 0, httpResponse.ContentBytes.Length);
            }
            else
            {
                return null;
            }
        }

        public static async Task<string> AsString<TRequest>(this Task<HttpResponse<TRequest>> httpResponseTask)
            where TRequest : HttpRequestBase
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.AsString();
        }

        public static byte[] AsByteArray<TRequest>(this HttpResponse<TRequest> httpResponse)
            where TRequest : HttpRequestBase
        {
            if (httpResponse.Content != null)
            {
                return Encoding.UTF8.GetBytes(httpResponse.Content);
            }
            else
            {
                return httpResponse.ContentBytes;
            }
        }

        public static async Task<byte[]> AsByteArray<TRequest>(this Task<HttpResponse<TRequest>> httpResponseTask)
            where TRequest : HttpRequestBase
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.AsByteArray();
        }

        public static T AsAnonymousType<T, TRequest>(this HttpResponse<TRequest> httpResponse, T anonymousTypeObject)
            where TRequest : HttpRequestBase
        {
            return httpResponse.As<T, TRequest>();
        }

        public static async Task<T> AsAnonymousType<T, TRequest>(this Task<HttpResponse<TRequest>> httpResponseTask, T anonymousTypeObject)
            where TRequest : HttpRequestBase
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.AsAnonymousType(anonymousTypeObject);
        }

        public static T As<T, TRequest>(this HttpResponse<TRequest> httpResponse)
            where TRequest : HttpRequestBase
        {
            return AutoDeserializeAs(
                httpResponse,
                jsonDelegate: DeserializeJsonAs<T>,
                xmlDelegate: DeserializeXmlAs<T>);
        }

        public static async Task<T> As<T, TRequest>(this Task<HttpResponse<TRequest>> httpResponseTask)
            where TRequest : HttpRequestBase
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.As<T, TRequest>();
        }

        public static IDictionary<string, object> AsDictionary<TRequest>(this HttpResponse<TRequest> httpResponse)
            where TRequest : HttpRequestBase
            => AsDictionary(httpResponse, null);

        public static IDictionary<string, object> AsDictionary<TRequest>(this HttpResponse<TRequest> httpResponse, StringComparer keyComparer)
            where TRequest : HttpRequestBase
        {
            return AutoDeserializeAs(
                httpResponse,
                jsonDelegate: x => JsonAsDictionary(x, keyComparer),
                xmlDelegate: x => XmlAsDictionary(x, keyComparer));
        }

        public static async Task<IDictionary<string, object>> AsDictionary<TRequest>(this Task<HttpResponse<TRequest>> httpResponseTask)
            where TRequest : HttpRequestBase
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.AsDictionary();
        }

        public static T ParseWith<T, TRequest>(this HttpResponse<TRequest> httpResponse, Func<HttpResponse<TRequest>, T> parseDelegate)
            where TRequest : HttpRequestBase
        {
            return parseDelegate.Invoke(httpResponse);
        }

        public static async Task<T> ParseWith<T, TRequest>(this Task<HttpResponse<TRequest>> httpResponseTask, Func<HttpResponse<TRequest>, T> parseDelegate)
            where TRequest : HttpRequestBase
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.ParseWith(parseDelegate);
        }

        public static T ParseWith<T, TRequest>(this HttpResponse<TRequest> httpResponse, Func<string, T> parseDelegate)
            where TRequest : HttpRequestBase
        {
            var asString = httpResponse.AsString();
            return parseDelegate.Invoke(asString);
        }

        public static async Task<T> ParseWith<T, TRequest>(this Task<HttpResponse<TRequest>> httpResponseTask, Func<string, T> parseDelegate)
            where TRequest : HttpRequestBase
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.ParseWith(parseDelegate);
        }

        public static string GetHeaderValue<TRequest>(this HttpResponse<TRequest> httpResponse, string key)
            where TRequest : HttpRequestBase
        {
            var header = httpResponse.Headers?.Where(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).ToList();

            if (header.Any())
            {
                return header.First().Value; // TODO: care about headers with multiple values
            }

            return null;
        }

        internal static T AutoDeserializeAs<T, TRequest>(this HttpResponse<TRequest> httpResponse, Func<string, T> jsonDelegate, Func<string, T> xmlDelegate)
            where TRequest : HttpRequestBase
        {
            bool isJson = false;
            bool isXml = false;
            if (ContentTypeEvaluator.IsJsonContent(httpResponse))
            {
                isJson = true;
            }
            else if (ContentTypeEvaluator.IsXmlContent(httpResponse))
            {
                isXml = true;
            }
            else if (ContentTypeEvaluator.CouldBeJsonContent(httpResponse))
            {
                isJson = true;
            }
            else if (ContentTypeEvaluator.CouldBeXmlContent(httpResponse))
            {
                isXml = true;
            }

            var asString = httpResponse.AsString();
            if (isJson)
            {
                return jsonDelegate.Invoke(asString);
            }
            else if (isXml)
            {
                return xmlDelegate.Invoke(asString);
            }

            throw new InvalidOperationException("Data must be JSON or XML to automatically deserialize.");
        }

        internal static T DeserializeJsonAs<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        internal static IDictionary<string, object> JsonAsDictionary(string json, StringComparer comparer)
        {
            return JsonHelper.ToDictionary(json, comparer);
        }

        internal static T DeserializeXmlAs<T>(string xml)
        {
            var xDocument = XDocument.Parse(xml);
            var jsonText = JsonConvert.SerializeXNode(xDocument);
            var result = DeserializeJsonAs<T>(jsonText);
            return result;
        }

        internal static IDictionary<string, object> XmlAsDictionary(string xml, StringComparer comparer)
        {
            var xDocument = XDocument.Parse(xml);
            var jsonText = JsonConvert.SerializeXNode(xDocument);
            var result = JsonAsDictionary(jsonText, comparer);
            return result;
        }
    }
}
