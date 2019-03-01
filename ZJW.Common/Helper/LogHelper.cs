using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ZJW.Common.Ext;

namespace ZJW.Common.Helper
{
    /// <summary>
    /// 使用LOG4NET记录日志的功能，log4net.config里要配置相应的节点
    /// </summary>
    public class LogHelper
    {
        //log4net日志专用
        public static readonly log4net.ILog logInfo = log4net.LogManager.GetLogger("logInfo");
        public static readonly log4net.ILog logError = log4net.LogManager.GetLogger("logError");
        public static readonly log4net.ILog logWarn = log4net.LogManager.GetLogger("logWarn");
        public static readonly log4net.ILog logJob = log4net.LogManager.GetLogger("logJob");

        public static void SetConfig()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static void SetConfig(FileInfo configFile)
        {
            log4net.Config.XmlConfigurator.Configure(configFile);
        }

        /// <summary>
        /// 普通记录日志
        /// </summary>
        /// <param name="info"></param>
        public static void WriteLog(string info)
        {
            WriteCommonLog(EnumState.CommonLogType.Common, EnumState.CommonLogSubType.CommonLog, info, EnumState.LogLevel.Info);
        }

        /// <summary>
        /// 错误简要日志
        /// </summary>
        /// <param name="info"></param>
        /// <param name="se"></param>
        public static void WriteErrorLog(string info, Exception se)
        {
            info = info + " 错误信息：" + se.ToString();
            WriteCommonLog(EnumState.CommonLogType.Common, EnumState.CommonLogSubType.CommonLog, info, EnumState.LogLevel.Error);
        }

        /// <summary>
        /// 错误详细日志
        /// </summary>
        /// <param name="info"></param>
        /// <param name="se"></param>
        public static void WriteErrorFullInfo(string info, Exception se)
        {
            info = info + " 错误信息：" + se.Message
                + "\r\n堆栈信息：" + se.StackTrace;
            WriteCommonLog(EnumState.CommonLogType.Common, EnumState.CommonLogSubType.CommonLog, info, EnumState.LogLevel.Error);
        }

        /// <summary>
        /// 错误详细日志
        /// </summary>
        /// <param name="info"></param>
        /// <param name="se"></param>
        public static void WriteErrorFullInfo(EnumState.CommonLogType commonLogType, EnumState.CommonLogSubType commonLogSubType, string message
            , Exception se, EnumState.LogLevel level = EnumState.LogLevel.Error)
        {
            message = message + " 错误信息：" + se.Message
                + "\r\n堆栈信息：" + se.StackTrace;
            WriteCommonLog(commonLogType, commonLogSubType, message, level);
        }

        /// <summary>
        /// 写日志到不同的log文件中
        /// </summary>
        /// <param name="LogContent">日志内容</param>
        /// <param name="type">日志类</param>
        public static void WriteLog(string LogContent, EnumState.LogServiceType type)
        {
            WriteCommonLog(type.ToString(), type.ToString(), LogContent, EnumState.LogLevel.Info);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryName">大分类</param>
        /// <param name="subcategoryName">小分类</param>
        /// <param name="message">日志内容</param>
        /// <param name="customFilter">自定义日志筛选条件，对应于日志查询系统的“文本过滤2”</param>
        /// <param name="customItems">自定义日志项，eg.{params:balabala...}</param>
        public static void WriteCommonLog(EnumState.CommonLogType commonLogType, EnumState.CommonLogSubType commonLogSubType, string message, EnumState.LogLevel LogType)
        {
            string categoryName = commonLogType.ToString();

            string subcategoryName = commonLogSubType.ToString();

            WriteCommonLog(categoryName, subcategoryName, message, LogType);
        }

        /// <summary>
        /// 记录天网日志
        /// </summary>
        /// <param name="categoryName">大分类</param>
        /// <param name="subcategoryName">小分类</param>
        /// <param name="message">日志内容</param>
        public static void WriteCommonLog(string categoryName, string subcategoryName, string message, EnumState.LogLevel LogType)
        {
            new Task(() =>
            {
                StackTrace stackTrace = new StackTrace(true);
                MethodBase method = stackTrace.GetFrame(1).GetMethod();//获取记录日志的方法
                string serviceName = "";
                if (null != method)
                {
                    serviceName = method.Name;
                }
                var logmsg = string.Format("\r\n[{0}-{1}]:{2}\r\n{3}", categoryName, subcategoryName, message, serviceName);
                switch (LogType)
                {
                    case EnumState.LogLevel.Info:
                        logInfo.Info(logmsg); break;
                    case EnumState.LogLevel.Error:
                        logError.Error(logmsg); break;
                    case EnumState.LogLevel.Warn:
                        logWarn.Info(logmsg); break;
                    case EnumState.LogLevel.Job:
                        logJob.Info(logmsg); break;
                }
            }).Start();
        }
    }
}
