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
        private readonly string _username;
        private readonly string _password;

        public DigestAuthenticationProvider(string user, string pass)
        {
            _username = user;
            _password = pass;
        }

        public override async Task AuthenticateAsync(HttpRequest request)
        {
            var digestServerParams = await GetDigestServerParams(request.Uri);

            var clientNonce = Guid.NewGuid().ToString();
            var nonceCount = 1;
            var data = GetDigestData(digestServerParams, request, clientNonce, nonceCount);

            request.Headers[Headers.Authorization] = $"Digest {data}";
        }

        internal async Task<DigestServerParams> GetDigestServerParams(Uri uri)
        {
            var unauthenticatedResponse = await new HttpClient().GetAsync(request =>
            {
                request
                    .WithUri(uri)
                    .WithExpectedStatusCode(HttpStatusCode.Unauthorized);
            });

            var wwwAuthenticateHeader = unauthenticatedResponse.GetHeaderValue("www-authenticate");
            return DigestServerParams.Parse(wwwAuthenticateHeader);
        }

        internal string GetDigestData(DigestServerParams digestServerParams, HttpRequest request, string clientNonce, int nonceCount)
        {
            var uri = request.Uri.PathAndQuery;
            var method = $"{request.Method}";

            var ha1 = GetHA1(digestServerParams, clientNonce);
            var ha2 = GetHA2(digestServerParams, uri, method, request);
            var response = GetResponse(digestServerParams, ha1, ha2, nonceCount, clientNonce);

            var data = new[]
            {
                $"username=\"{_username}\"",
                $"realm=\"{digestServerParams.Realm}\"",
                $"nonce=\"{digestServerParams.Nonce}\"",
                $"uri=\"{uri}\"",
                $"response=\"{response}\"",
                $"qop={digestServerParams.Qop}",
                $"nc={nonceCount}",
                $"cnonce=\"{clientNonce}\"",
                $"opaque=\"{digestServerParams.Opaque}\"",
            };

            var result = string.Join(", ", data);
            return result;
        }

        internal string GetHA1(DigestServerParams digestServerParams, string clientNonce)
        {
            if (digestServerParams.IsAlgorithmEmpty() || digestServerParams.IsAlgorithmMd5())
            {
                return GetMd5(_username, digestServerParams.Realm, _password);
            }
            else if (digestServerParams.IsAlgorithmMd5Sess())
            {
                return GetMd5(GetMd5(_username, digestServerParams.Realm, _password), digestServerParams.Nonce, clientNonce);
            }

            throw new NotSupportedException("Unsupported algorithm directive: " + digestServerParams.Algorithm);
        }

        internal string GetHA2(DigestServerParams digestServerParams, string uri, string method, HttpRequest request)
        {
            if (digestServerParams.IsQopEmpty() || digestServerParams.IsQopAuth())
            {
                return GetMd5(method, uri);
            }
            else if (digestServerParams.IsQopAuthInt())
            {
                var bodyBytes = request.BinaryContent ?? Encoding.UTF8.GetBytes(request.Content ?? string.Empty);
                return GetMd5(method, uri, GetMd5(bodyBytes));
            }

            throw new NotSupportedException("Unsupported qop directive: " + digestServerParams.Qop);
        }

        internal string GetResponse(DigestServerParams digestServerParams, string ha1, string ha2, int nonceCount, string clientNonce)
        {
            if (digestServerParams.IsQopEmpty())
            {
                return GetMd5(ha1, digestServerParams.Nonce, ha2);
            }
            else if (digestServerParams.IsQopAuth() || digestServerParams.IsQopAuthInt())
            {
                return GetMd5(ha1, digestServerParams.Nonce, $"{nonceCount}", clientNonce, digestServerParams.Qop, ha2);
            }

            throw new NotSupportedException("Unsupported qop directive: " + digestServerParams.Qop);
        }

        private string GetMd5(params string[] values) => GetMd5(string.Join(":", values));

        private string GetMd5(string stringToHash) => GetMd5(Encoding.UTF8.GetBytes(stringToHash));

        private string GetMd5(byte[] bytes)
        {
            using (var md5 = MD5.Create())
            {
                var hashBytes = md5.ComputeHash(bytes);
                var hashHex = BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
                return hashHex;
            }
        }
    }
}
#endif
