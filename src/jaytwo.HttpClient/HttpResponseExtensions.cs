using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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
            if (httpResponse.Content != null)
            {
                return httpResponse.Content;
            }
            else if (httpResponse.BinaryContent != null)
            {
                // TODO: read the headers in case a different encoding is defined
                return Encoding.UTF8.GetString(httpResponse.BinaryContent, 0, httpResponse.BinaryContent.Length);
            }
            else
            {
                return null;
            }
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
                return Encoding.UTF8.GetBytes(httpResponse.Content);
            }
            else
            {
                return httpResponse.BinaryContent;
            }
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

        public static async Task<T> AsAnonymousType<T>(this Task<HttpResponse> httpResponseTask, T anonymousTypeObject)
        {
            var httpResponse = await httpResponseTask;
            return httpResponse.AsAnonymousType<T>(anonymousTypeObject);
        }

        public static T As<T>(this HttpResponse httpResponse)
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

            if (isJson)
            {
                return httpResponse.DeserializeJsonAs<T>();
            }
            else if (isXml)
            {
                return httpResponse.DeserializeXmlAs<T>();
            }

            throw new InvalidOperationException("Data must be JSON or XML to automatically deserialize.");
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

        public static string GetHeaderValue(this HttpResponse httpResponse, string key)
        {
            var header = httpResponse.Headers?.Where(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).ToList();

            if (header.Any())
            {
                return header.Single().Value;
            }

            return null;
        }

        internal static T DeserializeJsonAs<T>(this HttpResponse httpResponse)
        {
            var asString = httpResponse.AsString();
            return JsonConvert.DeserializeObject<T>(asString);
        }

        internal static T DeserializeXmlAs<T>(this HttpResponse httpResponse)
        {
            var xml = httpResponse.AsString();
            var xDocument = XDocument.Parse(xml);
            var jsonText = JsonConvert.SerializeXNode(xDocument);
            var result = JsonConvert.DeserializeObject<T>(jsonText);
            return result;
        }
    }
}
