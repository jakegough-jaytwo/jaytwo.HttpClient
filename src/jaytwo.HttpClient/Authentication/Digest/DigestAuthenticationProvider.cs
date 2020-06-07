#if !NETSTANDARD1_1
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using jaytwo.HttpClient.Constants;

namespace jaytwo.HttpClient.Authentication.Digest
{
    public class DigestAuthenticationProvider : AuthenticationProviderBase, IAuthenticationProvider
    {
        private readonly IHttpClient _httpClient;
        private readonly string _username;
        private readonly string _password;

        public DigestAuthenticationProvider(IHttpClient httpClient, string user, string pass)
        {
            _httpClient = httpClient;
            _username = user;
            _password = pass;
        }

        public DigestAuthenticationProvider(string user, string pass)
            : this(new HttpClient(), user, pass)
        {
        }

        internal Func<string> ClientNonceGenerateDelegate { get; set; } = () => Guid.NewGuid().ToString();

        public override async Task AuthenticateAsync(HttpRequest request)
        {
            var digestServerParams = await GetDigestServerParams(request.Uri);

            var clientNonce = ClientNonceGenerateDelegate.Invoke();
            var nonceCount = 1;
            var headerValue = GetDigesAuthorizationtHeader(digestServerParams, request, clientNonce, nonceCount);

            request.Headers[Headers.Authorization] = headerValue;
        }

        internal async Task<DigestServerParams> GetDigestServerParams(Uri uri)
        {
            var unauthenticatedResponse = await _httpClient.GetAsync(request =>
            {
                request
                    .WithUri(uri)
                    .WithExpectedStatusCode(HttpStatusCode.Unauthorized);
            });

            var wwwAuthenticateHeader = unauthenticatedResponse.GetHeaderValue("www-authenticate");
            return DigestServerParams.Parse(wwwAuthenticateHeader);
        }

        internal string GetDigesAuthorizationtHeader(DigestServerParams digestServerParams, HttpRequest request, string clientNonce, int nonceCount)
        {
            var uri = request.Uri.PathAndQuery;
            var nonceCountAsString = $"{nonceCount}".PadLeft(8, '0'); // padleft not strictly necessary, but it makes the documented example work
            var response = DigestCalculator.GetResponse(digestServerParams, request, uri, _username, _password, clientNonce, nonceCountAsString);

            var data = new Dictionary<string, string>()
            {
                { "username",  _username },
                { "realm",  digestServerParams.Realm },
                { "nonce", digestServerParams.Nonce },
                { "uri", uri },
                { "response", response },
                { "qop", digestServerParams.Qop },
                { "nc", nonceCountAsString },
                { "cnonce", clientNonce },
                { "opaque", digestServerParams.Opaque },
            };

            var result = DigestServerParams.SerializeDictionary(data);
            return result;
        }
    }
}
#endif
