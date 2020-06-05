using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jaytwo.HttpClient.Constants;

namespace jaytwo.HttpClient.Authentication.Basic
{
    public class BasicAuthenticationProvider : AuthenticationProviderBase, IAuthenticationProvider
    {
        private readonly string _user;
        private readonly string _pass;

        public BasicAuthenticationProvider(string user, string pass)
        {
            _user = user;
            _pass = pass;
        }

        public override void Authenticate(HttpRequest request)
        {
            var combined = $"{_user}:{_pass}";
            var bytes = Encoding.UTF8.GetBytes(combined);
            var base64 = Convert.ToBase64String(bytes);

            request.Headers[Headers.Authorization] = $"Basic {base64}";
        }
    }
}
