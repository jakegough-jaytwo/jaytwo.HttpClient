using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace jaytwo.HttpClient.Exceptions
{
    public class UnexpectedStatusCodeException : Exception
    {
        public UnexpectedStatusCodeException(HttpStatusCode statusCode, HttpResponse response)
            : base(GetMessage(statusCode))
        {
            Response = response;
            StatusCode = response.StatusCode;
        }

        public HttpStatusCode StatusCode { get; }

        public HttpResponse Response { get; }

        private static string GetMessage(HttpStatusCode statusCode)
        {
            return $"Unexpected status code: {(int)statusCode} ({statusCode})";
        }
    }
}
