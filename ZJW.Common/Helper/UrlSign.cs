using System.Collections.Generic;
using System.Linq;

namespace ZJW.Common.Helper
{
    /// <summary>
    /// 地址栏签名方法
    /// </summary>
    public class UrlSign
    {
        /// <summary>
        /// 用MD5方式加签名
        /// <para>
        /// 1.str = "a=1&b=1&c=1...&z=1"
        /// 2.sign = MD5(str+crypKey)
        /// 3.retrun $"{urlHead}?{str}&{sign}"
        /// </para>
        /// </summary>
        /// <param name="param"></param>
        /// <param name="urlHead"></param>
        /// <param name="crypKey"></param>
        /// <returns></returns>
        public string MD5Sign(Dictionary<string, string> param, string urlHead, string crypKey)
        {
            if (param == null || !param.Any())
                return string.Empty;

            var list = new List<string>();
            var paramSort = param.OrderBy(o => o.Key).ToList();
            paramSort.ForEach(p =>
            {
                list.Add($"{p.Key}={p.Value}");
            });
            var paramStr = string.Join("&", list);
            var sign = CryptionHelper.Md5(paramStr + crypKey);

            return $"{urlHead}?{paramStr}&{sign}";
        }
    }
}
