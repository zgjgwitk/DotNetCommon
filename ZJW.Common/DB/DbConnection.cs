using ZJW.Common.Helper;

namespace ZJW.Common.DB
{
    public static class DbConnection
    {
        /// <summary>
        /// 数据库加密秘钥
        /// </summary>
        private const string DBSecretKey = "NestleGerber" + "zkjM,!,SCV";
        private const string DBIv = "rmsieuuxptsqlnbg";

        public static string SqlConnStrStore
        {
            get { return DBStringDecrypt(Tool.ConfigString("SqlConnStrStore")); }
        }
        public static string SqlConnStrGerberWechat
        {
            get { return DBStringDecrypt(Tool.ConfigString("SqlConnStrGerberWechat")); }
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="connstr"></param>
        /// <returns></returns>
        public static string DBStringEncryption(string connstr)
        {
            return CryptionHelper.AESEncrypt(connstr, DBSecretKey, DBIv);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="connstr"></param>
        /// <returns></returns>
        public static string DBStringDecrypt(string connstr)
        {
            return CryptionHelper.AESDecrypt(connstr, DBSecretKey, DBIv);
        }
    }
}
