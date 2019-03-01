using System;
using System.Configuration;

namespace ZJW.Common.Helper
{
    public class ConfigHelper
    {
        /// <summary>
        /// 获取\App_Data\Appkeys.config里面的AppSettings
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetExeAppSettings(string key)
        {
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = AppDomain.CurrentDomain.BaseDirectory + @"\App_Data\Appkeys.config";// 这里对应你app文件的路径
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            string val = config.AppSettings.Settings[key].Value;
            return val;
        }
    }
}
