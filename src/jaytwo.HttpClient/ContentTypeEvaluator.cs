using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using jaytwo.MimeHelper;

namespace jaytwo.HttpClient
{
    internal static class ContentTypeEvaluator
    {
        public static bool IsBinaryContent(HttpContent content)
        {
            var mediaType = content.Headers?.ContentType?.MediaType ?? string.Empty;
            return IsBinaryMediaType(mediaType);
        }

        public static bool IsBinaryMediaType(string mediaType)
        {
            var knownBinaryMediaTypes = new[]
            {
                MediaType.application_octet_stream,
            };

            var binaryMediaTypePrefixes = new[]
            {
                "image/",
                "audio/",
                "video/",
                "font/",
                "application/vnd.openxmlformats-officedocument.",
                "application/vnd.ms-",
            };

            var binaryMediaTypeSuffixes = new[]
            {
                "/zip",
                "/pdf",
                "-compressed",
            };

            var isKnownBinaryMediaType = knownBinaryMediaTypes.Contains(mediaType);
            var hasBinaryMediaTypePrefix = binaryMediaTypePrefixes.Any(x => mediaType.StartsWith(x));
            var hasBinaryMediaTypeSuffix = binaryMediaTypeSuffixes.Any(x => mediaType.EndsWith(x));

            var result = isKnownBinaryMediaType || hasBinaryMediaTypePrefix || hasBinaryMediaTypeSuffix;
            return result;
        }

        public static bool IsStringContent(HttpContent content)
        {
            var mediaType = content.Headers?.ContentType?.MediaType ?? string.Empty;
            return IsStringMediaType(mediaType);
        }

        public static bool IsStringMediaType(string mediaType)
        {
            var knownStringMediaTypes = new[]
            {
                MediaType.multipart_form_data,
                MediaType.application_x_www_form_urlencoded,
            };

            var stringMediaTypePrefixes = new[]
            {
                "text/"
            };

            var stringMediaTypeSuffixes = new[]
            {
                "/json",
                "+json",
                "/xml",
                "+xml",
                "/javascript",
            };

            var isKnownStringMediaType = knownStringMediaTypes.Contains(mediaType);
            var hasStringMediaTypePrefix = stringMediaTypePrefixes.Any(x => mediaType.StartsWith(x));
            var hasStringMediaTypeSuffix = stringMediaTypeSuffixes.Any(x => mediaType.EndsWith(x));

            var isStringMediaType = isKnownStringMediaType || hasStringMediaTypePrefix || hasStringMediaTypeSuffix;
            var isBinaryMediaType = IsBinaryMediaType(mediaType);

            var result = isStringMediaType && !isBinaryMediaType;
            return result;
        }
    }
}
