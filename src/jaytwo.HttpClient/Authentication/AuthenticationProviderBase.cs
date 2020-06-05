using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jaytwo.HttpClient.Constants;

namespace jaytwo.HttpClient.Authentication
{
    public class AuthenticationProviderBase : IAuthenticationProvider
    {
        // override either of these methods, but don't do both

        public virtual Task AuthenticateAsync(HttpRequest request)
        {
            Authenticate(request);

#if NETFRAMEWORK || NETSTANDARD1_1
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        public virtual void Authenticate(HttpRequest request)
        {
        }
    }
}
