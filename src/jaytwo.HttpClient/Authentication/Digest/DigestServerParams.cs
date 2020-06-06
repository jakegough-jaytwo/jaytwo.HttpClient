using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jaytwo.HttpClient.Authentication.Digest
{
    internal class DigestServerParams
    {
        public string Qop { get; set; }

        public string Nonce { get; set; }

        public string Realm { get; set; }

        public string Opaque { get; set; }

        public string Algorithm { get; set; }

        public string Stale { get; set; }

        public static DigestServerParams Parse(string wwwAuthenticateHeader)
        {
            if (!wwwAuthenticateHeader.StartsWith("Digest "))
            {
                throw new InvalidOperationException("Not a digest header!");
            }

            wwwAuthenticateHeader = wwwAuthenticateHeader.Substring(7);

            var headerValues = wwwAuthenticateHeader
                .Split(',')
                .Select(x => x.Trim())
                .Select(x => x.Split('='))
                .ToDictionary(x => x.First(), x => x.Last().TrimStart('"').TrimEnd('"'));

            var result = new DigestServerParams()
            {
                Qop = headerValues["qop"],
                Nonce = headerValues["nonce"],
                Realm = headerValues["realm"],
                Opaque = headerValues["opaque"],
                Algorithm = headerValues["algorithm"],
                Stale = headerValues["stale"],
            };

            return result;
        }

        public bool IsQopEmpty() => string.IsNullOrWhiteSpace(Qop);

        public bool IsQopAuth() => string.Equals(Qop, "auth", StringComparison.OrdinalIgnoreCase);

        public bool IsQopAuthInt() => string.Equals(Qop, "auth-int", StringComparison.OrdinalIgnoreCase);

        public bool IsAlgorithmEmpty() => string.IsNullOrWhiteSpace(Algorithm);

        public bool IsAlgorithmMd5() => string.Equals(Algorithm, "MD5", StringComparison.OrdinalIgnoreCase);

        public bool IsAlgorithmMd5Sess() => string.Equals(Algorithm, "MD5-sess", StringComparison.OrdinalIgnoreCase);
    }
}
