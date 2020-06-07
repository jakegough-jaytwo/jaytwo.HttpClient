using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jaytwo.HttpClient.Authentication
{
    public interface IAuthenticationProvider
    {
        Task AuthenticateAsync(HttpRequest request);
    }
}
