using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Web;

namespace ZJW.Common.Helper
{
    public class CacheHelper
    {
        public static string TokenKey = "&OeT8Q*VoiiKsRwD";

        static System.Web.Caching.Cache Cache = HttpRuntime.Cache;

        public static void Set(string key, object data)
        {
            Cache.Insert(key, data);
        }
        public static void Set(string key, object data, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            Cache.Insert(key, data, null, absoluteExpiration, slidingExpiration);
        }

        public static object Get(string Key)
        {
            return Cache[Key];
        }

        public static T Get<T>(string key)
        {
            return (T)Cache[key];
        }

        public static bool IsSet(string key)
        {
            return Cache[key] != null;
        }

        public static void Remove(string Key)
        {
            if (Cache[Key] != null)
            {
                Cache.Remove(Key);
            }
        }

        public static void RemoveByPattern(string pattern)
        {
            IDictionaryEnumerator enumerator = Cache.GetEnumerator();
            Regex rgx = new Regex(pattern, (RegexOptions.Singleline | (RegexOptions.Compiled | RegexOptions.IgnoreCase)));
            while (enumerator.MoveNext())
            {
                if (rgx.IsMatch(enumerator.Key.ToString()))
                {
                    Cache.Remove(enumerator.Key.ToString());
                }
            }
        }

        public static void Clear()
        {
            IDictionaryEnumerator enumerator = Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Cache.Remove(enumerator.Key.ToString());
            }
        }

    }
}
