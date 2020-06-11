using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jaytwo.HttpClient.Exceptions
{
    public class RequestTimedOutException : Exception
    {
        public RequestTimedOutException(Exception innerException)
            : base("Request Timed Out", innerException)
        {
        }
    }
}
