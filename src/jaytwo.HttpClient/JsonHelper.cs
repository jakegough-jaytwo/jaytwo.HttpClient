using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace jaytwo.HttpClient
{
    public class JsonHelper
    {
        public static IDictionary<string, object> ToDictionary(string json) => ToDictionary(json, null);

        public static IDictionary<string, object> ToDictionary(string json, StringComparer keyComparer)
        {
            var jobject = JObject.Parse(json);
            return ToDictionary(jobject, keyComparer);
        }

        public static IDictionary<string, object> ToDictionary(JObject jobject) => ToDictionary(jobject, null);

        public static IDictionary<string, object> ToDictionary(JObject jobject, StringComparer keyComparer)
        {
            return ToPlainObject(jobject, keyComparer) as IDictionary<string, object>;
        }

        private static object ToPlainObject(JToken token, StringComparer keyComparer)
        {
            // from https://stackoverflow.com/a/59162359

            var keyComparerOrDefault = keyComparer ?? StringComparer.Ordinal;

            switch (token)
            {
                case JObject jObject:
                    return ((IEnumerable<KeyValuePair<string, JToken>>)jObject).ToDictionary(j => j.Key, j => ToPlainObject(j.Value, keyComparerOrDefault), keyComparerOrDefault);

                case JArray jArray:
                    return jArray.Select(x => ToPlainObject(x, keyComparerOrDefault)).ToList();

                case JValue jValue:
                    return jValue.Value;

                default:
                    throw new Exception($"Unsupported type: {token.GetType()}");
            }
        }
    }
}
