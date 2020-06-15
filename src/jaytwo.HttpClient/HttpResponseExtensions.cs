using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using jaytwo.AsyncHelper;
using jaytwo.HttpClient.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
            if (httpResponse.Content != null)
            {
                if (httpResponse.Content is string)
                {
                    return (string)httpResponse.Content;
                }
                else if (httpResponse.Content is byte[])
                {
                    // TODO: read the headers in case a different encoding is defined
                    var asBytes = (byte[])httpResponse.Content;
                    return Encoding.UTF8.GetString(asBytes, 0, asBytes.Length);
                }
                else if (httpResponse.Content is HttpContent)
                {
                    return httpResponse.AsHttpContent().ReadAsStringAsync().AwaitSynchronously();
                }
                else
                {
                    throw new NotSupportedException($"Unsupported content object type: {httpResponse.Content.GetType()}");
                }
            }

            return null;
        }

        public static async Task<string> AsString(this Task<HttpResponse> httpResponseTask)
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.AsString();
        }

        public static byte[] AsByteArray(this HttpResponse httpResponse)
        {
            if (httpResponse.Content != null)
            {
                if (httpResponse.Content is byte[])
                {
                    return (byte[])httpResponse.Content;
                }
                else if (httpResponse.Content is string)
                {
                    // TODO: read the headers in case a different encoding is defined
                    var asString = (string)httpResponse.Content;
                    return Encoding.UTF8.GetBytes(asString);
                }
                else if (httpResponse.Content is HttpContent)
                {
                    return httpResponse.AsHttpContent().ReadAsByteArrayAsync().AwaitSynchronously();
                }
                else
                {
                    throw new NotSupportedException($"Unsupported content object type: {httpResponse.Content.GetType()}");
                }
            }

            return null;
        }

        public static async Task<byte[]> AsByteArray(this Task<HttpResponse> httpResponseTask)
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.AsByteArray();
        }

        public static T AsAnonymousType<T>(this HttpResponse httpResponse, T anonymousTypeObject)
        {
            return httpResponse.As<T>();
        }

        public static async Task<HttpContent> AsHttpContent(this Task<HttpResponse> httpResponseTask)
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.AsHttpContent();
        }

        public static HttpContent AsHttpContent(this HttpResponse httpResponse)
        {
            var asHttpContent = httpResponse.Content as HttpContent;
            asHttpContent?.LoadIntoBufferAsync().AwaitSynchronously(); // TODO: only load into buffer if under a certain size

            if (asHttpContent == null)
            {
                throw new InvalidOperationException("Content not preserved as HttpContent.");
            }

            return asHttpContent;
        }

        public static async Task<Stream> AsStream(this Task<HttpResponse> httpResponseTask)
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.AsStream();
        }

        public static Stream AsStream(this HttpResponse httpResponse)
        {
            return httpResponse.AsHttpContent().ReadAsStreamAsync().AwaitSynchronously();
        }

        public static async Task<T> AsAnonymousType<T>(this Task<HttpResponse> httpResponseTask, T anonymousTypeObject)
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.AsAnonymousType<T>(anonymousTypeObject);
        }

        public static T As<T>(this HttpResponse httpResponse)
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)httpResponse.AsString();
            }
            else if (typeof(T) == typeof(byte[]))
            {
                return (T)(object)httpResponse.AsByteArray();
            }
            else if (typeof(T) == typeof(Stream))
            {
                return (T)(object)httpResponse.AsStream();
            }
            else if (typeof(T) == typeof(HttpContent))
            {
                return (T)(object)httpResponse.AsHttpContent();
            }

            return AutoDeserializeAs(
                httpResponse,
                jsonDelegate: DeserializeJsonAs<T>,
                xmlDelegate: DeserializeXmlAs<T>);
        }

        public static async Task<T> As<T>(this Task<HttpResponse> httpResponseTask)
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.As<T>();
        }

        public static IDictionary<string, object> AsDictionary(this HttpResponse httpResponse) => AsDictionary(httpResponse, null);

        public static IDictionary<string, object> AsDictionary(this HttpResponse httpResponse, StringComparer keyComparer)
        {
            return AutoDeserializeAs(
                httpResponse,
                jsonDelegate: x => JsonAsDictionary(x, keyComparer),
                xmlDelegate: x => XmlAsDictionary(x, keyComparer));
        }

        public static async Task<IDictionary<string, object>> AsDictionary(this Task<HttpResponse> httpResponseTask)
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.AsDictionary();
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

        public static string GetHeaderValue(this HttpResponse httpResponse, string key)
        {
            var header = httpResponse.Headers?.Where(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).ToList();

            if (header.Any())
            {
                return header.First().Value; // TODO: care about headers with multiple values
            }

            return null;
        }

        internal static T AutoDeserializeAs<T>(HttpResponse httpResponse, Func<string, T> jsonDelegate, Func<string, T> xmlDelegate)
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
