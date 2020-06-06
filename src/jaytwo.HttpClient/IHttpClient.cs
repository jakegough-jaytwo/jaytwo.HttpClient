using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jaytwo.HttpClient
{
    public interface IHttpClient
    {
        Task<HttpResponse> SendAsync(HttpRequest request);

        HttpRequest CreateRequest();
    }
}
