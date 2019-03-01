namespace ZJW.Common.Ext
{
    public class EnumState
    {

        public enum CommonLogType
        {
            Common,
            WeChat,
            Apply,
            Job,
            SQL,
        }
        public enum CommonLogSubType
        {
            CommonLog,
            QRCodeLogin,
            SaveUserInfo,
            SendNotGottenMsg,
            SendNotBoughtMsg,
            SendBoughtMsg,
            QRCodeLoginResp,
            index,
            SendTxtMsg,
            #region Job
            Instance,
            Execute,
            GerberWechat,
            SampleRemind,
            CalcAchiev,
            SendAchiev,
            CGAndSSR,
            RepaireMobile,
            #endregion
        }
        public enum LogServiceType
        {
        }
        public enum LogLevel
        {
            Info,
            Error,
            Warn,
            Job,
        }

        /// <summary>
        /// 上传文件分类
        /// </summary>
        public enum UploadFileType
        {
            QRCode,
            Avatar,
        }

        /// <summary>
        /// 缩略图图片模式
        /// </summary>
        public enum ThumbnailImageMode
        {
            /// <summary>
            /// 指定高宽缩放，可能变形
            /// </summary>
            HW = 0,
            /// <summary>
            /// 指定宽，高按比例
            /// </summary>
            W = 1,
            /// <summary>
            /// 指定高，宽按比例
            /// </summary>
            H = 2,
            /// <summary>
            /// 根据缩略图宽和高按比例裁剪，不变形
            /// </summary>
            Cat = 3
        }
    }
}
