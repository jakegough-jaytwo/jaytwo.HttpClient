using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jaytwo.HttpClient
{
    public interface IAuthenticationProvider
    {
        Task AuthenticateAsync(HttpRequest request);
    }
}
