using System;
using System.Collections.Generic;
using Epsagon.Dotnet.Core.Configuration;
using Newtonsoft.Json;
using OpenTracing;
using OpenTracing.Tag;

namespace Epsagon.Dotnet.Core
{
    public static class Utils
    {
        private static IEpsagonConfiguration _config;
        public static IEpsagonConfiguration CurrentConfig => _config;

        public static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                ContractResolver = new JsonLowerCaseUnderscoreContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        public static Dictionary<string, string> DeserializeObject(string str)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(str, new JsonSerializerSettings {
                ContractResolver = new JsonLowerCaseUnderscoreContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        public static void SetDataIfNeeded(this ISpan span, string tagName, object input)
        {
            SetDataIfNeeded(span, tagName, JsonConvert.SerializeObject(input));
        }

        public static void SetDataIfNeeded(this ISpan span, string tagName, string input) {
            if (!Utils.CurrentConfig.MetadataOnly)
            {
                span.SetTag(tagName, input);
            }
        }

        public static void SetDataIfNeeded(this ISpan span, string tagName, int input) {
            if (!Utils.CurrentConfig.MetadataOnly)
            {
                span.SetTag(tagName, input);
            }
        }

        public static void AddException(this ISpan span, Exception e)
        {
            Tags.Error.Set(span, true);
            span.SetTag("event.error_code", 2); // exception
            span.SetTag("error.message", e.Message);
            span.SetTag("error.stack_trace", e.StackTrace);
            span.SetTag("error.type", e.GetType().Name);
            span.SetTag("error.time", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0);
        }

        public static void RegisterConfiguration(IEpsagonConfiguration configuration)
        {
            _config = configuration;
        }

        public static bool IsNullOrDefault<T>(T argument)
        {
            if (argument is ValueType || argument != null)
            {
                return object.Equals(argument, GetDefault(argument.GetType()));
            }
            return true;
        }
        private static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public static double ToUnixTime(this DateTime dateTime) {
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds() / 1000.0;
        }
    }
}