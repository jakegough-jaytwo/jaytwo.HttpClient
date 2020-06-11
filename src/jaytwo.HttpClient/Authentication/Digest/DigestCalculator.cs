#if !NETSTANDARD1_1
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace jaytwo.HttpClient.Authentication.Digest
{
    internal class DigestCalculator
    {
        public static string GetResponse(DigestServerParams digestServerParams, HttpRequest request, string uri, string username, string password, string clientNonce, string nonceCount)
        {
            var method = $"{request.Method}";
            var selectedQop = SelectQop(digestServerParams.Qop);

            var ha1 = GetHA1(digestServerParams.Algorithm, digestServerParams.Realm, digestServerParams.Nonce, username, password, clientNonce);
            var ha2 = GetHA2(selectedQop, uri, method, request);
            var response = GetResponse(selectedQop, digestServerParams.Nonce, ha1, ha2, clientNonce, nonceCount);

            return response;
        }

        public static string GetHA1(string algorithm, string realm, string nonce, string username, string password, string clientNonce)
        {
            if (string.IsNullOrEmpty(algorithm) || algorithm == Algorithm.Md5)
            {
                return GetMd5(username, realm, password);
            }
            else if (algorithm == Algorithm.Md5Sess) // rare-ish
            {
                return GetMd5(GetMd5(username, realm, password), nonce, clientNonce);
            }

            throw new NotSupportedException("Unsupported algorithm directive: " + algorithm);
        }

        public static string GetHA2(string qop, string uri, string method, HttpRequest request)
        {
            //if (string.IsNullOrEmpty(qop) || qop == Qop.Auth)
            //{
            //    return GetMd5(method, uri);
            //}
            //else if (qop == Qop.AuthInt) // super rare
            //{
            //    var bodyBytes = request.BinaryContent ?? Encoding.UTF8.GetBytes(request.Content ?? string.Empty);
            //    return GetMd5(method, uri, GetMd5(bodyBytes));
            //}

            throw new NotSupportedException("Unsupported qop directive: " + qop);
        }

        public static string GetResponse(string qop, string nonce, string ha1, string ha2, string clientNonce, string nonceCount)
        {
            if (string.IsNullOrEmpty(qop))
            {
                return GetMd5(ha1, nonce, ha2);
            }
            else if (qop == Qop.Auth || qop == Qop.AuthInt)
            {
                return GetMd5(ha1, nonce, nonceCount, clientNonce, qop, ha2);
            }

            throw new NotSupportedException("Unsupported qop directive: " + qop);
        }

        public static string SelectQop(string qop)
        {
            var normalizedQopOptions = (qop ?? string.Empty).Split(',').Select(x => x.ToLower()).ToList();

            if (!normalizedQopOptions.Any())
            {
                return string.Empty;
            }
            else if (normalizedQopOptions.Contains(Qop.Auth))
            {
                return Qop.Auth;
            }
            else if (normalizedQopOptions.Contains(Qop.AuthInt))
            {
                return Qop.AuthInt;
            }
            else
            {
                return qop;
            }
        }

        public static string GetMd5(params string[] values) => GetMd5(string.Join(":", values));

        public static string GetMd5(string stringToHash) => GetMd5(Encoding.UTF8.GetBytes(stringToHash));

        public static string GetMd5(byte[] bytes)
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
