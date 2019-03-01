using System;
using System.Configuration;

namespace ZJW.Common.Helper
{
    public class Tool
    {
        /// <summary>
        /// 系统默认时间 1900-01-01 
        /// </summary>
        public static readonly DateTime DefaultSysDate = new DateTime(1900, 1, 1);

        /// <summary>
        /// 判断是否为测试环境
        /// <para>true:测试,false:非测试</para>
        /// </summary>
        public static bool IsTestApplication
        {
            get
            {
                string AreaDeploy = GetApplication;
                if (!string.IsNullOrEmpty(AreaDeploy) && AreaDeploy.ToLower() == "test")
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 判断是否为测试或预发环境
        /// <para>true:测试或预发,false:正式</para>
        /// </summary>
        public static bool IsStageApplication
        {
            get
            {
                string AreaDeploy = GetApplication;
                if (!string.IsNullOrEmpty(AreaDeploy) &&
                    (AreaDeploy.ToLower() == "test" || AreaDeploy.ToLower() == "stage"))
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 获取环境配置
        /// </summary>
        public static string GetApplication
        {
            get
            {
                return ValidateHelper.FinalString(ConfigurationManager.AppSettings["AreaDeploy"]);
            }
        }

        /// <summary>
        /// 获取站点的虚拟目录
        /// </summary>
        public static string GetApplicationPath
        {
            get
            {
                var path = ConfigString("ApplicationPath");
                if (!string.IsNullOrEmpty(path))
                {
                    return string.Format("/{0}", ConfigString("ApplicationPath"));
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        #region 根据ip判断配置文件

        /// <summary>
        /// 根据ip判断读取的配置文件,测试配置keyname默认加"_Test"
        /// </summary>
        /// <param name="keyName">keyName</param>
        /// <returns></returns>
        public static string ConfigString(string keyName)
        {
            return ConfigString(keyName, "_Test");
        }

        /// <summary>
        /// 根据ip判断读取的配置文件
        /// </summary>
        /// <param name="keyName">keyName</param>
        /// <param name="testSuffix">测试的key后缀</param>
        /// <returns></returns>
        public static string ConfigString(string keyName, string testSuffix)
        {
            if (string.IsNullOrEmpty(keyName))
                return string.Empty;
            if (IsTestApplication)
            {
                keyName += ValidateHelper.FinalString(testSuffix);
            }

            return ValidateHelper.FinalString(ConfigurationManager.AppSettings[keyName]);
        }
        #endregion

        /// <summary>
        /// 生成随机编码
        /// </summary>
        /// <param name="codeCount"></param>
        /// <param name="type">1:数字,2:字母,3:混合</param>
        /// <returns></returns>
        public static string CreateRandomCode(int codeCount, int type)
        {
            // 函数功能:产生数字和字符混合的随机字符串
            var character = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var number = "0123456789";
            var mix = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string allChar;
            switch (type)
            {
                case 1:
                    allChar = number; break;
                case 2:
                    allChar = character; break;
                case 3:
                    allChar = mix; break;
                default:
                    allChar = mix; break;
            }

            char[] allCharArray = allChar.ToCharArray();
            string randomCode = "";
            Random rand = new Random((int)DateTime.Now.Ticks);
            for (int i = 0; i < codeCount; i++)
            {
                int r = rand.Next(allChar.Length - 1);
                randomCode += allCharArray.GetValue(r);
            }
            return randomCode;
        }

        /// <summary>
        /// 获取时间戳-统计到秒
        /// </summary>
        /// <returns></returns>
        public static double GetTimeStamp(DateTime t)
        {
            return Math.Floor((t - new DateTime(1970, 1, 1)).TotalSeconds);
        }

        /// <summary>
        /// 根据时间戳转为时间
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static DateTime GetTimeByStamp(double s)
        {
            return (new DateTime(1970, 1, 1)).AddSeconds(s);
        }
    }
}