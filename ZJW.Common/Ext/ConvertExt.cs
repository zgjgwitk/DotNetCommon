using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using ZJW.Common.Helper;

namespace ZJW.Common.Ext
{
    /// <summary>字条串转换扩展</summary>
    public static class ConvertExt
    {
        /// <summary>数据库空时间 </summary>
        public static readonly DateTime NullSqlDateTime = Tool.DefaultSysDate;//((DateTime)System.Data.SqlTypes.SqlDateTime.Null);

        public static bool ToBoolean(this string source)
        {
            bool reValue;
            bool.TryParse(source, out reValue);
            return reValue;
        }
        /// <summary>转化为Byte型</summary>
        public static Byte ToByte(this string source)
        {
            Byte reValue;
            Byte.TryParse(source, out reValue);
            return reValue;
        }
        /// <summary> 转化为Short型</summary>
        public static short ToShort(this string source)
        {
            short reValue;
            short.TryParse(source, out reValue);
            return reValue;
        }
        /// <summary>转化为Short型</summary>
        public static short ToInt16(this string source)
        {
            short reValue;
            short.TryParse(source, out reValue);
            return reValue;
        }

        /// <summary>转化为int32型</summary>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static int ToInt32(this string source, int defaultValue = 0)
        {
            int reValue;
            return Int32.TryParse(source, out reValue) ? reValue : defaultValue;
        }
        /// <summary>转化为int64型</summary>
        public static long ToInt64(this string source)
        {
            long reValue;
            Int64.TryParse(source, out reValue);
            return reValue;
        }
        /// <summary>转化为Float型</summary>
        public static float ToFloat(this string source)
        {
            float reValue;
            float.TryParse(source, out reValue);
            return reValue;
        }
        /// <summary>转化为Double型</summary>
        public static Double ToDouble(this string source)
        {
            Double reValue;
            Double.TryParse(source, out reValue);
            return reValue;
        }
        /// <summary>转化为decimal型</summary>
        public static decimal ToDecimal(this string source)
        {
            decimal reValue;
            decimal.TryParse(source, out reValue);
            return reValue;
        }
        /// <summary>转化为日期为空里返回NullSqlDateTime,byark</summary>
        public static DateTime ToDateTime(this string source)
        {
            DateTime reValue;
            return DateTime.TryParse(source, out reValue) ? reValue : NullSqlDateTime;
        }
        /// <summary>转化为日期为空里返回NullSqlDateTime,byark</summary>
        public static DateTime ToDateTimeByNum(this string source)
        {
            //20050102010101
            DateTime reValue = NullSqlDateTime;
            if (source.Length == 14)
            {
                if (!DateTime.TryParse(source.Substring(0, 4) + "-" + source.Substring(4, 2) + "-" + source.Substring(6, 2) + " "
                    + source.Substring(8, 2) + ":" + source.Substring(10, 2) + ":" + source.Substring(12, 2), out reValue))
                    reValue = NullSqlDateTime;
            }
            return reValue;
        }
        /// <summary>转化为数字类型的日期</summary>
        public static decimal ToDateTimeDecimal(this string source)
        {
            DateTime reValue;
            return DateTime.TryParse(source, out reValue) ? reValue.ToString("yyyyMMddHHmmss").ToDecimal() : 0;
        }
        /// <summary>将时间转换成数字</summary>
        public static decimal ToDateTimeDecimal(this DateTime source)
        {
            return source.ToString("yyyyMMddHHmmss").ToDecimal();
        }
        /// <summary>将时间转换成字串</summary>
        public static string ToISO8601DateString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fff");
        }

        #region 将Dunull字段赋值

        /// <summary>
        /// 字符串是否为DateTime类型
        /// </summary>
        /// <param name="dateString"></param>
        /// <returns></returns>
        public static bool IsDateTime(object dateObj)
        {
            if (dateObj == null)
                return false;
            DateTime dtTmp;
            if (DateTime.TryParse(dateObj.ToString(), out dtTmp))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 将Dunull字段赋值转为时间
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime ToTime(object time)
        {
            DateTime dt;
            if (DateTime.TryParse(time.ToString(), out dt))
            {
                return dt;
            }
            return new DateTime(1900, 1, 1);
        }
        #endregion

        /// <summary> 将IP转换为long型</summary>
        public static long ToIPLong(this string ip)
        {
            byte[] bytes = IPAddress.Parse(ip).GetAddressBytes();
            return (long)bytes[3] + (((uint)bytes[2]) << 8) + (((uint)bytes[1]) << 16) + (((uint)bytes[0]) << 24);
        }
        /// <summary> 将Int64转换成IPAddress</summary>
        public static IPAddress ToIPAddress(this Int64 source)
        {
            Byte[] b = new Byte[4];
            for (int i = 0; i < 4; i++)
                b[3 - i] = (Byte)(source >> 8 * i & 255);
            return (new IPAddress(b));
        }
        /// <summary>将已经为 HTTP 传输进行过 HTML 编码的字符串转换为已解码的字符串</summary>
        public static string HtmlDecode(this string s)
        {
            return HttpUtility.HtmlDecode(s);
        }
        /// <summary>将字符串转换为 HTML 编码的字符串</summary>
        public static string HtmlEncode(this string s)
        {
            return HttpUtility.HtmlEncode(s);
        }
        /// <summary>对 URL 字符串进行编码</summary>
        public static string UrlEncode(this string s)
        {
            return HttpUtility.UrlEncode(s);
        }
        /// <summary>将已经为在 URL 中传输而编码的字符串转换为解码的字符串</summary>
        public static string UrlDecode(this string s)
        {
            return HttpUtility.UrlDecode(s);
        }
        /// <summary>转义</summary>
        public static string RegexDecode(this string s)
        {
            return Regex.Unescape(s);
        }
    }
}
