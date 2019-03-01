using System.Text.RegularExpressions;
using System.Web;

namespace ZJW.Common.Helper
{
    public class RequestHelper
    {
        /// <summary>
        /// 从Request.QueryString或者Request.Form中获取字符串
        /// </summary>
        /// <param name="key">参数Key</param>
        /// <returns>字符串</returns>
        public static string GetStringFromParameters(string key)
        {
            string Value = string.Empty;
            if (!string.IsNullOrEmpty(key))
            {
                Value = !string.IsNullOrEmpty(HttpContext.Current.Request[key]) ? HttpContext.Current.Request[key] : string.Empty;
            }
            return Value;
        }

        /// <summary>
        /// 从Request.QueryString或者Request.Form中获取字符串
        /// </summary>
        /// <param name="keys">参数Key数组</param>
        /// <param name="spliter">分隔符</param>
        /// <returns>字符串</returns>
        public static string GetStringFromParameters(string keys, string spliter)
        {
            string Value = string.Empty;

            string Key = RequestHelper.GetNotEmptyKey(keys, spliter);

            if (!string.IsNullOrEmpty(Key))
            {
                Value = RequestHelper.GetStringFromParameters(Key);
            }

            return Value;
        }

        /// <summary>
        /// 从Request.QueryString或者Request.Form中获取整型数字
        /// </summary>
        /// <param name="key">参数Key</param>
        /// <returns>整型数字</returns>
        public static int GetIntFromParameters(string key)
        {
            int Value = default(int);
            if (!string.IsNullOrEmpty(key))
            {
                Value = (!string.IsNullOrEmpty(HttpContext.Current.Request[key]) && IsNumeric(HttpContext.Current.Request[key])) ? int.Parse(HttpContext.Current.Request[key]) : default(int);
            }
            return Value;
        }

        /// <summary>
        /// 从参数Key数组中找出第一个不为空的Key
        /// </summary>
        /// <param name="keys">参数Key数组</param>
        /// <param name="spliter">分隔符</param>
        /// <returns>Key</returns>
        private static string GetNotEmptyKey(string keys, string spliter)
        {
            string Key = string.Empty;

            if (string.IsNullOrEmpty(keys))
            {
                return keys;
            }

            if (keys.Contains(spliter))
            {
                string[] KeysString = keys.Split(spliter.ToCharArray());

                for (int Index = 0; Index < KeysString.Length; Index++)
                {
                    if (!string.IsNullOrEmpty(HttpContext.Current.Request[KeysString[Index]]))
                    {
                        Key = KeysString[Index];
                        break;
                    }
                }
            }
            else
            {
                Key = keys;
            }

            return Key;
        }

        /// <summary>
        /// 验证字符串是否为数字（正则表达式）（true = 是数字, false = 不是数字）
        /// </summary>
        /// <param name="validatedString">被验证的字符串</param>
        /// <returns>true = 是数字, false = 不是数字</returns>
        public static bool IsNumeric(string validatedString)
        {
            const string NumericPattern = @"^[-]?\d+[.]?\d*$";

            return Regex.IsMatch(validatedString, NumericPattern);
        }
    }
}
