using System;
using System.Drawing;
using System.IO;
using System.Net;
using ZJW.Common.Ext;

namespace ZJW.Common.Helper
{
    public class ImageHelper
    {
        public static byte[] GetImage(string url)
        {
            WebClient myWebClient = new WebClient();
            return myWebClient.DownloadData(url);
        }

        public static string UploadDataURIscheme(System.Web.HttpServerUtilityBase server, string dataURI, out string msg)
        {
            var webUrl = string.Empty;
            msg = "";
            try
            {
                var ext = "";
                var b64 = ImageHelper.GetB64DataURIscheme(dataURI, out ext);
                var path = UploadHelper.CheckFileUpLoadDirectory("~", server, EnumState.UploadFileType.Avatar);
                var pcurl = $"{path}{Guid.NewGuid().ToString()}.{ext}";
                byte[] byteArray = Convert.FromBase64String(b64);
                MemoryStream imgStream = new MemoryStream(byteArray);
                Image img = Image.FromStream(imgStream);
                img.Save(pcurl);
                webUrl = "/" + pcurl.Replace(server.MapPath("~"), "").Replace("\\", "/");
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return webUrl;
        }

        /// <summary>
        /// 获取64位编码图片的编码信息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static string GetB64DataURIscheme(string data, out string ext)
        {
            var b64 = data.Split(",".ToCharArray(), 2);
            ext = GetImageExt(b64[0]);
            return b64[1];
        }

        /// <summary>
        /// 获取图片扩展名
        /// </summary>
        /// <param name="imgDataURIHead"></param>
        /// <returns></returns>
        public static string GetImageExt(string imgDataURIHead)
        {
            if (imgDataURIHead.Contains("data:image/gif"))
            {
                return "gif";
            }
            else if (imgDataURIHead.Contains("data:image/png"))
            {
                return "png";
            }
            else if (imgDataURIHead.Contains("data:image/jpeg"))
            {
                return "jpg";
            }
            else if (imgDataURIHead.Contains("data:image/x-icon"))
            {
                return "icon";
            }
            else if (imgDataURIHead.Contains("data:image/bmp"))
            {
                return "bmp";
            }
            return "jpg";
        }
    }
}
