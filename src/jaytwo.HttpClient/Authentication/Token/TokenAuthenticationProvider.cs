using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jaytwo.HttpClient.Constants;

namespace jaytwo.HttpClient.Authentication.Token
{
    public class TokenAuthenticationProvider : AuthenticationProviderBase, IAuthenticationProvider
    {
        private readonly ITokenProvider _tokenProvider;

        public TokenAuthenticationProvider(string token)
            : this(() => token)
        {
        }

        public TokenAuthenticationProvider(Func<string> tokenDelegate)
            : this(new DelegateTokenProvider(tokenDelegate))
        {
        }

        public TokenAuthenticationProvider(ITokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        public override async Task AuthenticateAsync(HttpRequest request)
        {
            var token = await _tokenProvider.GetTokenAsync();
            request.Headers[Headers.Authorization] = $"Bearer {token}";
        }
    }
}
