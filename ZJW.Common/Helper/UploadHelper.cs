using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using ZJW.Common.Ext;

namespace ZJW.Common.Helper
{
    public class UploadHelper
    {
        /// <summary>
        /// 得到上传文件目录
        /// </summary>
        /// <param name="applicationPath">应用程序路径</param>
        /// <param name="server">web请求服务</param>
        /// <param name="type">上传文件分类,创建字目录</param>
        /// <returns>上传文件目录</returns>
        public static string CheckFileUpLoadDirectory(string applicationPath, System.Web.HttpServerUtilityBase server, EnumState.UploadFileType type)
        {
            string dic = server.MapPath(applicationPath + "/LogFile/UpFile/" + GetFileUpLoadPath(type));
            if (!Directory.Exists(dic))
            {
                Directory.CreateDirectory(dic);
            }
            return dic;
        }

        /// <summary>
        /// 得到上传文件目录
        /// </summary>
        public static string CheckFileUpLoadDirectory(string applicationPath, System.Web.HttpServerUtility server, EnumState.UploadFileType type)
        {
            string dic = server.MapPath(applicationPath + "/LogFile/UpFile/" + GetFileUpLoadPath(type));
            if (!Directory.Exists(dic))
            {
                Directory.CreateDirectory(dic);
            }
            return dic;
        }

        /// <summary>
        /// 存放路径,存放数据库
        /// </summary>
        /// <returns></returns>
        public static string GetFileUpLoadPath(EnumState.UploadFileType type)
        {
            string dic = type.ToString() + "/";
            dic += DateTime.Today.Year + "/";
            dic += DateTime.Today.Month + "/";
            dic += DateTime.Today.Day + "/";

            return dic;
        }

        public static void Upload(HttpPostedFileBase file, string fullfilename)
        {
            file.SaveAs(fullfilename);
        }

        /// <summary>
        /// 上传图片Job组件方法
        /// </summary>
        /// <param name="absolutePath">绝对路径</param>
        /// <param name="relativePath">相对路径</param>
        /// <param name="returnMessage">当错误时，需要返回错误信息，当图片大小大于2048 Kb时，返回“bigPicture”,
        /// 当图片不存在或出现异常时，抛出错误信息，当正确是返回空字符串</param>
        /// <param name="returnServicePath">公共相对路径</param>
        /// <returns>上传是否成功，true-成功，false-失败</returns>
        public static bool UploadImageByJob(string absolutePath, string relativePath, ref string returnMessage, ref string returnServicePath, ref string returnAbsolutePath)
        {
            bool boolean = false;
            int lastIndex = absolutePath.LastIndexOf('/');
            string fileName = absolutePath.Substring((lastIndex + 1), (absolutePath.Length - lastIndex - 1));
            try
            {

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(absolutePath);
                request.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Natas.Robot)";
                request.Timeout = 3000;

                WebResponse response = request.GetResponse();
                Int64 length = response.ContentLength / 1024;
                if (length > 2048)
                {
                    boolean = false;
                    returnMessage = "bigPicture";
                    returnServicePath = string.Empty;
                    return boolean;
                }
                else
                {
                    Stream stream = response.GetResponseStream();

                    //ImageUploader uploader = new ImageUploader();
                    //string remotePath = uploader.Upload("/cruises", SwapStream(stream), fileName);
                    string remotePath = "";

                    stream.Close();
                    response.Close();

                    returnAbsolutePath = remotePath;

                    int index = remotePath.IndexOf("/cruises");
                    if (index < 0) { returnServicePath = remotePath; }
                    else
                    {
                        returnServicePath = remotePath.Substring(index);
                    }

                    boolean = true;
                    returnMessage = string.Empty;
                    return boolean;
                }
            }
            catch (Exception ex)
            {
                boolean = false;
                returnServicePath = string.Empty;
                returnMessage = ex.Message;
                return boolean;
            }
        }

        /// <summary>
        /// 设定图片大小
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public static string SetImageSize(string absolutePath, HttpServerUtility server)
        {
            int lastIndex = absolutePath.LastIndexOf('/');
            string fileName = absolutePath.Substring((lastIndex + 1), (absolutePath.Length - lastIndex - 1));

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(absolutePath);
            request.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Natas.Robot)";
            request.Timeout = 3000;

            WebResponse response = request.GetResponse();

            Stream stream = response.GetResponseStream();
            string returnPath = server.MapPath("~/uploadfile/TempImages/");
            if (!Directory.Exists(returnPath))
            {
                Directory.CreateDirectory(returnPath);
            }

            string newFileName = "originalImage" + System.IO.Path.GetExtension(fileName);

            Image imgObject = Image.FromStream(stream);
            for (double i = 0; i < 1; i = i + 0.1)
            {
                if (File.Exists(returnPath + newFileName))
                {
                    File.Delete(returnPath + newFileName);
                }
                KiResizeImage(imgObject, i, System.IO.Path.GetExtension(fileName), returnPath + newFileName);
                FileInfo fileInfo = new FileInfo(returnPath + newFileName);
                if ((fileInfo.Length / 1024) > 2048)
                {
                    continue;
                }
                else
                {
                    break;
                }
            }
            stream.Close();
            response.Close();

            FileStream fs = new FileStream(returnPath + newFileName, FileMode.Open);
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, int.Parse(fs.Length.ToString()));
            fs.Close();
            fs.Dispose();

            MemoryStream ms = new MemoryStream(buffer);
            string remotePath = ImageUploaderMethod("/cruises", ms, fileName);

            int index = remotePath.IndexOf("/cruises");
            if (index < 0) { return remotePath; }
            else
            {
                return remotePath.Substring(index);
            }
        }

        /// <summary>
        /// 重新画图
        /// </summary>
        /// <param name="originalImage"></param>
        /// <param name="ratio"></param>
        /// <param name="extention"></param>
        /// <param name="saveFileName"></param>
        private static void KiResizeImage(Image originalImage, double ratio, string extention, string saveFileName)
        {
            int ow = originalImage.Width;
            int oh = originalImage.Height;

            int towidth = originalImage.Width - Convert.ToInt32(originalImage.Width * ratio);
            int toheight = (originalImage.Height) * towidth / originalImage.Width;//等比例缩放

            //新建一个bmp图片
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(towidth, toheight);

            //新建一个画板
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量插值法
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            g.Clear(System.Drawing.Color.Transparent);

            //在指定位置并且按指定大小绘制原图片的指定部分
            g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, towidth, toheight),
            new System.Drawing.Rectangle(0, 0, ow, oh),
            System.Drawing.GraphicsUnit.Pixel);

            try
            {
                //以jpg格式保存缩略图
                switch (extention.ToLower())
                {
                    case ".jpg":
                        bitmap.Save(saveFileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case ".jpeg":
                        bitmap.Save(saveFileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case ".gif":
                        bitmap.Save(saveFileName, System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                    case ".png":
                        bitmap.Save(saveFileName, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    case ".icon":
                        bitmap.Save(saveFileName, System.Drawing.Imaging.ImageFormat.Icon);
                        break;
                    case ".bmp":
                        bitmap.Save(saveFileName, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                }

            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                bitmap.Dispose();
                g.Dispose();
            }
            //return ms;
        }

        /// <summary>
        /// 上传图片接口
        /// </summary>
        /// <param name="imageUrl">原图片路径</param>
        /// <param name="applicationPath">应用程序目录：/youlun或者context.Request.ApplicationPath</param>
        /// <param name="server">server类：context.Server.MapPath</param>
        /// <param name="fileType">上传类型：Cruises</param>
        /// <param name="isWatermark">是否有水印：false</param>
        /// <returns>成功返回上传目录，不包含UploadFiles，失败返回string.Empty</returns>
        public static string UploadFileByUrl(string imageUrl, string applicationPath, HttpServerUtility server, EnumState.UploadFileType fileType, bool isWatermark)
        {
            int lastIndex = imageUrl.LastIndexOf('/');
            string fileName = imageUrl.Substring((lastIndex + 1), (imageUrl.Length - lastIndex - 1));
            string newFileName = Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(fileName);
            string uploadFilePath = GetFileUpLoadPath(fileType);
            string dic = server.MapPath(applicationPath + "/UploadFile/" + uploadFilePath).Replace(".Interface", "").Replace("/Interface", "").Replace("/interface", "").Replace("\\Interface", "").Replace("\\interface", "");
            string watermarkPath = server.MapPath(applicationPath + "/Content/Images/watermark/");
            try
            {

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(imageUrl);
                request.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Natas.Robot)";
                request.Timeout = 3000;

                WebResponse response = request.GetResponse();
                Int64 length = response.ContentLength / 1024;
                if (length > 5048)
                {
                    return "bigPicture";
                }
                else
                {
                    Stream stream = response.GetResponseStream();

                    stream.Close();
                    response.Close();

                    HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(imageUrl);
                    request1.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Natas.Robot)";
                    request1.Timeout = 3000;

                    WebResponse response1 = request1.GetResponse();
                    Stream stream1 = response1.GetResponseStream();

                    string fullFileName = (dic + newFileName).Replace("/Interface", "").Replace("/interface", "").Replace("\\Interface", "").Replace("\\interface", "");
                    Image imgObject = Image.FromStream(stream1);
                    if (isWatermark) MakeWatermark(imgObject, watermarkPath);  //添加水印
                    if (!Directory.Exists(dic))
                    {
                        Directory.CreateDirectory(dic);
                    }
                    imgObject.Save(fullFileName);

                    stream1.Close();
                    response1.Close();

                    //生成缩略图
                    HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create(imageUrl);
                    request2.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Natas.Robot)";
                    request2.Timeout = 3000;

                    WebResponse response2 = request2.GetResponse();
                    Stream stream2 = response2.GetResponseStream();
                    MakeThumbnail(fullFileName, Image.FromStream(stream2), null, isWatermark, watermarkPath);

                    stream2.Close();
                    response2.Close();

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return uploadFilePath + newFileName;
        }

        /// <summary>
        /// 上传图片接口
        /// </summary>
        /// <param name="imageUrl">原图片路径集合</param>
        /// <param name="applicationPath">应用程序目录：/youlun或者context.Request.ApplicationPath</param>
        /// <param name="server">server类：this.Server.MapPath</param>
        /// <param name="fileType">上传类型：Cruises</param>
        /// <param name="isWatermark">是否有水印：false</param>
        /// <returns>成功返回上传目录的集合，不包含UploadFiles，失败返回null</returns>
        public static bool UploadFileByImageUrls(ref IList<CruiseImageInfo> list, string applicationPath, HttpServerUtilityBase server, EnumState.UploadFileType fileType, bool isWatermark)
        {
            if (list != null && list.Count > 0)
            {
                foreach (var item in list)
                {
                    int lastIndex = item.ImageSourceUrl.LastIndexOf('/');
                    string fileName = item.ImageSourceUrl.Substring((lastIndex + 1), (item.ImageSourceUrl.Length - lastIndex - 1));
                    string newFileName = Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(fileName);
                    string uploadFilePath = GetFileUpLoadPath(fileType);
                    string dic = string.Empty;

                    dic = server.MapPath(applicationPath + "/UploadFile/" + uploadFilePath);

                    string watermarkPath = server.MapPath(applicationPath + "/Content/Images/watermark/");
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(item.ImageSourceUrl);
                        request.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Natas.Robot)";
                        request.Timeout = 3000;

                        WebResponse response = request.GetResponse();
                        Int64 length = response.ContentLength / 1024;
                        if (length > 5048)
                        {
                            return true;
                        }
                        else
                        {
                            Stream stream = response.GetResponseStream();

                            //ImageUploader uploader = new ImageUploader();
                            //string remotePath = uploader.Upload("/cruises", SwapStream(stream), fileName);
                            string remotePath = "";

                            stream.Close();
                            response.Close();

                            HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(item.ImageSourceUrl);
                            request1.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Natas.Robot)";
                            request1.Timeout = 3000;

                            WebResponse response1 = request1.GetResponse();
                            Stream stream1 = response1.GetResponseStream();

                            string fullFileName = dic + newFileName;
                            Image imgObject = Image.FromStream(stream1);
                            if (isWatermark) MakeWatermark(imgObject, watermarkPath);  //添加水印
                            if (!Directory.Exists(dic))
                            {
                                Directory.CreateDirectory(dic);
                            }
                            imgObject.Save(fullFileName);

                            stream1.Close();
                            response1.Close();

                            //生成缩略图
                            HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create(item.ImageSourceUrl);
                            request2.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Natas.Robot)";
                            request2.Timeout = 3000;

                            WebResponse response2 = request2.GetResponse();
                            Stream stream2 = response2.GetResponseStream();
                            MakeThumbnail(fullFileName, Image.FromStream(stream2), null, isWatermark, watermarkPath);

                            stream2.Close();
                            response2.Close();

                            Task task = Task.Factory.StartNew(() =>
                            {
                                SaveImageMapping(uploadFilePath + newFileName, remotePath);
                            }).ContinueWith((s) =>
                            {
                                if (s.IsFaulted)
                                { }
                            });

                            item.FileName = fileName;
                            item.OriginalImage = uploadFilePath + newFileName;
                            item.MappingImage = remotePath;
                        }
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="file">文件对象</param>
        /// <param name="applicationPath">应用程序目录</param>
        /// <param name="server">server类</param>
        /// <param name="fileType">上传类型</param>
        /// <param name="aryParam">缩略图参数，只对图片有效</param>
        /// <param name="isWatermark">是否有水印，只对图片有效</param>
        /// <returns>成功返回上传目录，不包含UploadFiles，失败返回string.Empty</returns>
        public static string Upload(HttpPostedFileBase file, string applicationPath, HttpServerUtilityBase server, EnumState.UploadFileType fileType, List<ThumbnailImageParam> aryParam, bool isWatermark)
        {
            byte[] bytes = new byte[file.InputStream.Length];
            file.InputStream.Read(bytes, 0, bytes.Length);
            // 将 byte[] 转成 Stream
            MemoryStream ms = new MemoryStream(bytes);

            string newFileName = Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(file.FileName);
            string uploadFilePath = GetFileUpLoadPath(fileType);
            string dic = server.MapPath(applicationPath + "/UploadFile/" + uploadFilePath);
            string watermarkPath = server.MapPath(applicationPath + "/Content/Images/watermark/");
            if (!Directory.Exists(dic))
            {
                Directory.CreateDirectory(dic);
            }
            try
            {
                string fullFileName = dic + newFileName;
                if (!CheckIsImage(fullFileName))
                {
                    //非图片直接保存
                    file.SaveAs(fullFileName);
                    return uploadFilePath + newFileName;
                }

                Image imgObject = Image.FromStream(file.InputStream);
                if (isWatermark) MakeWatermark(imgObject, watermarkPath);  //添加水印
                imgObject.Save(fullFileName);

                //生成缩略图
                MakeThumbnail(fullFileName, Image.FromStream(file.InputStream), aryParam, isWatermark, watermarkPath);

                //调用公共组件，并保存关联表
                //if (fileType == EnumState.UploadFileType.QRCode)
                //{
                //    Task task = Task.Factory.StartNew(() =>
                //    {
                //        string remotePath = ImageUploaderMethod("/cruises", ms, file.FileName);
                //        SaveImageMapping(uploadFilePath + newFileName, remotePath);
                //    }).ContinueWith((s) =>
                //    {
                //        if (s.IsFaulted)
                //        { }
                //    });
                //}
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
            return uploadFilePath + newFileName;
        }

        private static void SaveImageMapping(string local, string remote)
        {
            //if (string.IsNullOrEmpty(remote))
            //    return;
            //int index = remote.IndexOf("/cruises");
            //if (index < 0) return;
            //string sql = string.Format(" insert into [ImageMapping]([OriginalImage],[MappingImage]) values('{0}','{1}')", local, remote.Substring(index));
            //Database db = DatabaseFactory.GetWriteDatabase("TCCruiseResource");
            //SqlHelper.ExecuteNonQuery(db, sql);
        }

        public static Dictionary<string, string> Upload(HttpPostedFileBase file, string applicationPath, HttpServerUtilityBase server, EnumState.UploadFileType fileType, bool isWatermark, string fileCategory)
        {

            return Upload(file, applicationPath, server, fileType, null, isWatermark, fileCategory);
        }


        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="file">文件对象</param>
        /// <param name="applicationPath">应用程序目录</param>
        /// <param name="server">server类</param>
        /// <param name="fileType">上传类型</param>
        /// <param name="aryParam">缩略图参数，只对图片有效</param>
        /// <param name="isWatermark">是否有水印，只对图片有效</param>
        /// <returns>成功返回上传目录，不包含UploadFiles，失败返回string.Empty</returns>
        public static Dictionary<string, string> Upload(HttpPostedFileBase file, string applicationPath, HttpServerUtilityBase server, EnumState.UploadFileType fileType, List<ThumbnailImageParam> aryParam, bool isWatermark, string fileCategory)
        {
            byte[] bytes = new byte[file.InputStream.Length];
            file.InputStream.Read(bytes, 0, bytes.Length);
            // 将 byte[] 转成 Stream
            MemoryStream ms = new MemoryStream(bytes);
            Dictionary<string, string> dicpic = new Dictionary<string, string>();
            string newFileName = string.Empty;
            if (!string.IsNullOrEmpty(fileCategory) && fileCategory.Equals("CruiseLineAttachmentForTravel"))
            {
                newFileName = file.FileName;
                //int lastIndex = newFileName.LastIndexOf("\\");
                //if (lastIndex > 0)
                //{
                //    newFileName = newFileName.Substring((lastIndex + 1), (newFileName.Length - lastIndex - 1));
                //}
                //newFileName = StringHelper.ChangeSpecialCharacters(newFileName);
            }
            else
            {
                newFileName = Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(file.FileName);
            }
            //newFileName = Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(file.FileName);
            string uploadFilePath = GetFileUpLoadPath(fileType);
            string dic = server.MapPath(applicationPath + "/UploadFile/" + uploadFilePath);
            string watermarkPath = server.MapPath(applicationPath + "/Content/Images/watermark/");
            if (!Directory.Exists(dic))
            {
                Directory.CreateDirectory(dic);
            }
            try
            {
                string fullFileName = dic + newFileName;
                if (!CheckIsImage(fullFileName))
                {
                    if (!string.IsNullOrEmpty(fileCategory) && (fileCategory.Equals("CruiseLineAttachment") || fileCategory.Equals("CruiseLineAttachmentForTravel")))
                    {
                        if (System.IO.File.Exists(fullFileName))
                        {
                            System.IO.File.Delete(fullFileName);
                        }
                    }
                    //非图片直接保存
                    file.SaveAs(fullFileName);
                    dicpic.Add("local", uploadFilePath + newFileName);
                    dicpic.Add("public", "");
                    return dicpic;//uploadFilePath + newFileName;
                }

                Image imgObject = Image.FromStream(file.InputStream);
                if (isWatermark) MakeWatermark(imgObject, watermarkPath);  //添加水印
                imgObject.Save(fullFileName);

                //生成缩略图
                MakeThumbnail(fullFileName, Image.FromStream(file.InputStream), aryParam, isWatermark, watermarkPath);

                string remotePath = string.Empty;
                //调用公共组件，并保存关联表
                //if (fileType == EnumState.UploadFileType.QRCode)
                //{
                //    remotePath = ImageUploaderMethod("/cruises", ms, file.FileName);
                //    Task task = Task.Factory.StartNew(() =>
                //    {
                //        SaveImageMapping(uploadFilePath + newFileName, remotePath);
                //    }).ContinueWith((s) =>
                //    {
                //        if (s.IsFaulted)
                //        { }
                //    });
                //}
                dicpic.Add("local", uploadFilePath + newFileName);
                dicpic.Add("public", remotePath);
            }
            catch (Exception ex)
            {
                //return dicpic;//string.Empty;
                dicpic.Add("local", "");
                dicpic.Add("public", "");
                return dicpic;
            }
            return dicpic;//uploadFilePath + newFileName;
        }

        public static Dictionary<string, string> Upload(HttpPostedFileBase file, string applicationPath, HttpServerUtilityBase server, EnumState.UploadFileType fileType)
        {
            return Upload(file, applicationPath, server, fileType, true, string.Empty);
        }

        /// <summary>
        ///  生成缩略图
        /// </summary>
        /// <param name="fullFileName"></param>
        /// <param name="originalImage"></param>
        /// <param name="aryParam"></param>
        private static void MakeThumbnail(string fullFileName, Image originalImage, List<ThumbnailImageParam> aryParam, bool isWatermark, string watermarkPath)
        {
            if (originalImage == null || aryParam == null || aryParam.Count == 0)
            {
                return;
            }

            string ext = Path.GetExtension(fullFileName);
            string fileNameFixed = fullFileName.Substring(0, fullFileName.LastIndexOf('.'));
            foreach (var imgParam in aryParam)
            {
                if (imgParam == null) continue;
                MemoryStream ms = MakeThumbnailReturnMemoryStream(originalImage, imgParam.ImgWidth, imgParam.ImgHeight, imgParam.ImgMode, ext, true);
                Image imgObject = Image.FromStream(ms);
                if (isWatermark) MakeWatermark(imgObject, watermarkPath);  //添加水印
                imgObject.Save(string.Format("{0}_{1}_{2}{3}", fileNameFixed, imgParam.ImgWidth.ToString(), imgParam.ImgHeight.ToString(), ext));
            }
        }

        /// <summary>
        /// 生成缩略图返回MemoryStream
        /// </summary>
        /// <param name="originalImage">System.Drawing.Image</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="mode">生成缩略图的方式</param>
        /// <param name="extention">文件格式</param>
        /// <param name="isMake">是否处理图片，true 处理，false 不处理即原图</param>
        private static MemoryStream MakeThumbnailReturnMemoryStream(Image originalImage, int width, int height, EnumState.ThumbnailImageMode mode, string extention, bool isMake)
        {
            int towidth = originalImage.Width;
            int toheight = originalImage.Height;

            int x = 0;
            int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;

            //生成缩略图
            if (isMake)
            {
                towidth = width;
                toheight = height;

                switch (mode)
                {
                    case EnumState.ThumbnailImageMode.HW://指定高宽缩放（可能变形） 
                        break;
                    case EnumState.ThumbnailImageMode.W://指定宽，高按比例 
                        toheight = originalImage.Height * width / originalImage.Width;
                        break;
                    case EnumState.ThumbnailImageMode.H://指定高，宽按比例
                        towidth = originalImage.Width * height / originalImage.Height;
                        break;
                    case EnumState.ThumbnailImageMode.Cat://指定高宽裁减（不变形） 
                        if ((double)originalImage.Width / (double)originalImage.Height > (double)towidth / (double)toheight)
                        {
                            oh = originalImage.Height;
                            ow = originalImage.Height * towidth / toheight;
                            y = 0;
                            x = (originalImage.Width - ow) / 2;
                        }
                        else
                        {
                            ow = originalImage.Width;
                            oh = originalImage.Width * height / towidth;
                            x = 0;
                            y = (originalImage.Height - oh) / 2;
                        }
                        break;
                    default:
                        break;
                }
            }

            //新建一个bmp图片
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(towidth, toheight);

            //新建一个画板
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量插值法
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            g.Clear(System.Drawing.Color.Transparent);

            //在指定位置并且按指定大小绘制原图片的指定部分
            g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, towidth, toheight),
            new System.Drawing.Rectangle(x, y, ow, oh),
            System.Drawing.GraphicsUnit.Pixel);

            MemoryStream ms = new MemoryStream();
            try
            {
                //以jpg格式保存缩略图
                switch (extention.ToLower())
                {
                    case ".jpg":
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case ".jpeg":
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case ".gif":
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                    case ".png":
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    case ".icon":
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Icon);
                        break;
                    case ".bmp":
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                }

            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                bitmap.Dispose();
                g.Dispose();
            }
            return ms;
        }

        /// <summary>
        /// 图片加水印
        /// </summary>
        /// <param name="originalImage"></param>
        /// <param name="watermarkPath"></param>
        private static void MakeWatermark(Image originalImage, string watermarkPath)
        {
            try
            {
                if (originalImage.Width <= 250)
                    watermarkPath += "logo_small.png";
                else
                    watermarkPath += "logo.png";
                System.Drawing.Image copyImage = System.Drawing.Image.FromFile(watermarkPath);
                Graphics g = Graphics.FromImage(originalImage);
                g.DrawImage(copyImage,
                    new Rectangle(originalImage.Width - copyImage.Width, originalImage.Height - copyImage.Height, copyImage.Width, copyImage.Height)
                    , 0
                    , 0
                    , copyImage.Width
                    , copyImage.Height
                    , GraphicsUnit.Pixel);
                g.Dispose();

            }
            catch { }
        }

        /// <summary>
        /// 判断是否是图片
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static bool CheckIsImage(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToUpper();
            return (ext.Equals(".JPG") || ext.Equals(".BMP") || ext.Equals(".PNG") || ext.Equals(".GIF"));
        }

        /// <summary>
        /// 上传图片公共组件
        /// </summary>
        /// <param name="pathPrefix">"/cruises"</param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string ImageUploaderMethod(string pathPrefix, MemoryStream ms, string fileName)
        {
            return "";
            //try
            //{
            //    ImageUploader uploader = new ImageUploader();
            //    string strPicUrl = uploader.Upload(pathPrefix, ms, fileName);

            //    return strPicUrl;
            //}
            //catch (Exception ex)
            //{
            //    return ex.GetBaseException().Message;
            //}
            //finally
            //{
            //    ms.Close();
            //}
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="file">文件对象</param>
        /// <param name="applicationPath">应用程序目录</param>
        /// <param name="server">server类</param>
        /// <param name="fileType">上传类型</param>
        /// <param name="aryParam">缩略图参数，只对图片有效</param>
        /// <param name="isWatermark">是否有水印，只对图片有效</param>
        /// <returns>成功返回上传目录，不包含UploadFiles，失败返回string.Empty</returns>
        public static string UploadAttachmentByUrl(string fileUrl, string applicationPath, HttpServerUtility server, EnumState.UploadFileType fileType, ref Int64 fileSize)
        {
            int lastIndex = fileUrl.LastIndexOf('/');
            string fileName = fileUrl.Substring((lastIndex + 1), (fileUrl.Length - lastIndex - 1));
            string newFileName = fileName;//Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(file.FileName);
            string uploadFilePath = GetFileUpLoadPath(fileType);
            string dic = server.MapPath(applicationPath + "/UploadFile/" + uploadFilePath).Replace(".Interface", "").Replace("/Interface", "").Replace("/interface", "").Replace("\\Interface", "").Replace("\\interface", "");
            if (!Directory.Exists(dic))
            {
                Directory.CreateDirectory(dic);
            }
            try
            {
                //由于原名称保存所以要判断是否存在
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fileUrl);
                request.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Natas.Robot)";
                request.Timeout = 3000;

                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();

                byte[] arrayByte = new byte[1024];
                int imgLong = (int)response.ContentLength;
                int l = 0;

                string fullFileName = (dic + newFileName).Replace("/Interface", "").Replace("/interface", "").Replace("\\Interface", "").Replace("\\interface", "");
                //由于原名称保存所以要判断是否存在
                if (System.IO.File.Exists(fullFileName))
                {
                    System.IO.File.Delete(fullFileName);
                }
                FileStream fs = new FileStream(fullFileName, FileMode.Create);
                while (l < imgLong)
                {
                    int i = stream.Read(arrayByte, 0, 1024);
                    fs.Write(arrayByte, 0, i);
                    l += i;
                }
                fs.Close();
                stream.Close();
                response.Close();

                System.IO.FileInfo tmpFile = new System.IO.FileInfo(fullFileName);  //FileInfo类提供创建
                if (tmpFile != null)    //判断文件是否存在
                {
                    fileSize = tmpFile.Length / 1024;    //Length文件字节长度单位kb
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return uploadFilePath + newFileName;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="file">文件对象</param>
        /// <param name="applicationPath">应用程序目录</param>
        /// <param name="server">server类</param>
        /// <param name="fileType">上传类型</param>
        /// <param name="aryParam">缩略图参数，只对图片有效</param>
        /// <param name="isWatermark">是否有水印，只对图片有效</param>
        /// <returns>成功返回上传目录，不包含UploadFiles，失败返回string.Empty</returns>
        public static string UploadAttachmentByUrl(string fileUrl, string applicationPath, HttpServerUtilityBase server, EnumState.UploadFileType fileType, ref Int64 fileSize)
        {
            int lastIndex = fileUrl.Replace("\\", "/").LastIndexOf('/');
            string fileName = fileUrl.Substring((lastIndex + 1), (fileUrl.Length - lastIndex - 1));
            string fileExtension = fileName.Substring(fileName.LastIndexOf(".") + 1);
            string newFileName = Guid.NewGuid().ToString() + "." + fileExtension;

            //int lastIndex = fileUrl.LastIndexOf('/');
            //string fileName = fileUrl.Substring((lastIndex + 1), (fileUrl.Length - lastIndex - 1));
            //string newFileName = fileName;//Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(file.FileName);
            //string newFileName = Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(file.FileName);
            string uploadFilePath = GetFileUpLoadPath(fileType);
            string dic = server.MapPath(applicationPath + "/UploadFile/" + uploadFilePath).Replace(".Interface", "").Replace("/Interface", "").Replace("/interface", "").Replace("\\Interface", "").Replace("\\interface", "");
            if (!Directory.Exists(dic))
            {
                Directory.CreateDirectory(dic);
            }
            try
            {
                //由于原名称保存所以要判断是否存在
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fileUrl);
                request.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Natas.Robot)";
                request.Timeout = 3000;

                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();

                byte[] arrayByte = new byte[1024];
                int imgLong = (int)response.ContentLength;
                int l = 0;

                string fullFileName = (dic + newFileName).Replace("/Interface", "").Replace("/interface", "").Replace("\\Interface", "").Replace("\\interface", "");
                //由于原名称保存所以要判断是否存在
                if (System.IO.File.Exists(fullFileName))
                {
                    System.IO.File.Delete(fullFileName);
                }
                FileStream fs = new FileStream(fullFileName, FileMode.Create);
                while (l < imgLong)
                {
                    int i = stream.Read(arrayByte, 0, 1024);
                    fs.Write(arrayByte, 0, i);
                    l += i;
                }
                fs.Close();
                stream.Close();
                response.Close();

                System.IO.FileInfo tmpFile = new System.IO.FileInfo(fullFileName);  //FileInfo类提供创建
                if (tmpFile != null)    //判断文件是否存在
                {
                    fileSize = tmpFile.Length / 1024;    //Length文件字节长度单位kb
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return uploadFilePath + newFileName;
        }

        /// <summary>
        /// 上传图片接口
        /// </summary>
        /// <param name="imageUrl">原图片路径集合</param>
        /// <param name="applicationPath">应用程序目录：/youlun或者context.Request.ApplicationPath</param>
        /// <param name="server">server类：this.Server.MapPath</param>
        /// <param name="fileType">上传类型：Cruises</param>
        /// <param name="isWatermark">是否有水印：false</param>
        /// <returns>成功返回上传目录的集合，不包含UploadFiles，失败返回null</returns>
        public static bool UploadFileImageUrlsByInterface(ref IList<CruiseImageInfo> list, string applicationPath, HttpServerUtility server, EnumState.UploadFileType fileType, bool isWatermark)
        {
            if (list != null && list.Count > 0)
            {
                foreach (var item in list)
                {
                    int lastIndex = item.ImageSourceUrl.LastIndexOf('/');
                    string fileName = item.ImageSourceUrl.Substring((lastIndex + 1), (item.ImageSourceUrl.Length - lastIndex - 1));
                    string newFileName = Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(fileName);
                    string uploadFilePath = GetFileUpLoadPath(fileType);
                    string dic = string.Empty;

                    dic = server.MapPath(applicationPath + "/UploadFile/" + uploadFilePath).Replace(".Interface", "").Replace("/Interface", "").Replace("/interface", "").Replace("\\Interface", "").Replace("\\interface", "");

                    string watermarkPath = server.MapPath(applicationPath + "/Content/Images/watermark/");
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(item.ImageSourceUrl);
                        request.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Natas.Robot)";
                        request.Timeout = 3000;

                        WebResponse response = request.GetResponse();
                        Int64 length = response.ContentLength / 1024;
                        if (length > 2048)
                        {
                            return true;
                        }
                        else
                        {
                            Stream stream = response.GetResponseStream();

                            stream.Close();
                            response.Close();

                            HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(item.ImageSourceUrl);
                            request1.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Natas.Robot)";
                            request1.Timeout = 3000;

                            WebResponse response1 = request1.GetResponse();
                            Stream stream1 = response1.GetResponseStream();

                            string fullFileName = (dic + newFileName).Replace("/Interface", "").Replace("/interface", "").Replace("\\Interface", "").Replace("\\interface", "");
                            Image imgObject = Image.FromStream(stream1);
                            if (isWatermark) MakeWatermark(imgObject, watermarkPath);  //添加水印
                            if (!Directory.Exists(dic))
                            {
                                Directory.CreateDirectory(dic);
                            }
                            imgObject.Save(fullFileName);

                            stream1.Close();
                            response1.Close();

                            //生成缩略图
                            HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create(item.ImageSourceUrl);
                            request2.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Natas.Robot)";
                            request2.Timeout = 3000;

                            WebResponse response2 = request2.GetResponse();
                            Stream stream2 = response2.GetResponseStream();
                            MakeThumbnail(fullFileName, Image.FromStream(stream2), null, isWatermark, watermarkPath);

                            stream2.Close();
                            response2.Close();

                            item.FileName = fileName;
                            item.OriginalImage = uploadFilePath + newFileName;
                            //item.MappingImage = remotePath;
                            item.MappingImage = string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 参考的 UploadImageByJob 通过绝对路径上传图片到file表和ImgMapping表
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ResponseInfo<bool> UploadImageByAbsolutePath(string absolutePath, HttpContext context, EnumState.UploadFileType type)
        {
            var res = new ResponseInfo<bool>();
            //var res = new ReturnJsonMessage();
            int lastIndex = absolutePath.LastIndexOf('/');
            string fileName = absolutePath.Substring((lastIndex + 1), (absolutePath.Length - lastIndex - 1));
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(absolutePath);
                request.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Natas.Robot)";
                request.Timeout = 3000;
                WebResponse response = request.GetResponse();
                Int64 length = response.ContentLength / 1024;
                if (length > 2048)
                {
                    absolutePath = SetImageSizeReturnAbsolutePath(absolutePath, context);
                    UploadImageByAbsolutePath(absolutePath, context, type);
                }
                else
                {
                    Stream stream = response.GetResponseStream();
                    //var uploader = new ImageUploader();
                    //string remotePath = uploader.Upload("/cruises", SwapStream(stream), fileName);
                    string remotePath = "";
                    stream.Close();
                    response.Close();

                    var request2 = (HttpWebRequest)WebRequest.Create(absolutePath);
                    request2.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Natas.Robot)";
                    request2.Timeout = 3000;
                    WebResponse response2 = request2.GetResponse();
                    Stream stream2 = response2.GetResponseStream();
                    string uploadFilePath = GetFileUpLoadPath(type);
                    string newFileName = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(fileName);
                    string dic = context.Server.MapPath(context.Request.Url.Segments[0] + context.Request.Url.Segments[1] + "/UploadFile/" + uploadFilePath).Replace(".Interface", "").Replace("/Interface", "").Replace("/interface", "").Replace("\\Interface", "").Replace("\\interface", "");

                    if (!Directory.Exists(dic))
                    {
                        Directory.CreateDirectory(dic);
                    }
                    Image imgObject = Image.FromStream(stream2);
                    imgObject.Save(dic + newFileName);
                    stream2.Close();

                    response2.Close();
                    res.result = true;
                    res.msg = uploadFilePath + newFileName; //路径
                    //res.Text = remotePath; //公共的路径
                    //res.ReturnIdentity = length.ToString(); //文件大小
                    //res.ReturnValue = fileName.Split('.')[1]; //文件类型
                    return res;
                }
            }
            catch (Exception ex)
            {
                res.result = false;
                res.msg = "上传图片异常：" + ex.Message;
                return res;
            }
            return res;
        }

        /// <summary>
        /// 设定图片大小 返回本地的绝对路径
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string SetImageSizeReturnAbsolutePath(string absolutePath, HttpContext context)
        {
            int lastIndex = absolutePath.LastIndexOf('/');
            string fileName = absolutePath.Substring((lastIndex + 1), (absolutePath.Length - lastIndex - 1));
            var request = (HttpWebRequest)WebRequest.Create(absolutePath);
            request.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Natas.Robot)";
            request.Timeout = 3000;
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            string returnPath = context.Server.MapPath("~/uploadfile/TempImages/");
            if (!Directory.Exists(returnPath))
            {
                Directory.CreateDirectory(returnPath);
            }
            string newFileName = "originalImage" + System.IO.Path.GetExtension(fileName);
            Image imgObject = Image.FromStream(stream);
            for (double i = 0; i < 1; i = i + 0.1)
            {
                if (File.Exists(returnPath + newFileName))
                {
                    File.Delete(returnPath + newFileName);
                }
                KiResizeImage(imgObject, i, System.IO.Path.GetExtension(fileName), returnPath + newFileName);
                FileInfo fileInfo = new FileInfo(returnPath + newFileName);
                if ((fileInfo.Length / 1024) > 2048)
                {
                    continue;
                }
                else
                {
                    break;
                }
            }
            if (stream != null) stream.Close();
            response.Close();
            return returnPath + newFileName;
        }

        private static MemoryStream SwapStream(Stream inStream)
        {
            MemoryStream ms = new MemoryStream();
            byte[] buffer = new byte[1024];

            while (true)
            {
                int sz = inStream.Read(buffer, 0, 1024);
                if (sz == 0) break;
                ms.Write(buffer, 0, sz);
            }
            ms.Position = 0;
            return ms;
        }


        #region
        /// <summary>
        /// 下载网络图片到本地
        /// </summary>
        /// <param name="url">网络地址</param>
        /// <param name="path">本地保存路径</param>
        public static void DownloadImage(string url, string path)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.ServicePoint.Expect100Continue = false;
            req.Method = "GET";
            req.KeepAlive = true;
            req.ContentType = "image/*";
            HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();
            System.IO.Stream stream = null;
            try
            {
                // 以字符流的方式读取HTTP响应
                stream = rsp.GetResponseStream();
                Image.FromStream(stream).Save(path);
            }
            finally
            {
                // 释放资源
                if (stream != null) stream.Close();
                if (rsp != null) rsp.Close();
            }
        }
        #endregion
    }

    /// <summary>
    /// 上传缩略图片参数
    /// </summary>
    public class ThumbnailImageParam
    {
        private int imgWidth = 0;
        private int imgHeight = 0;
        private EnumState.ThumbnailImageMode imgMode = EnumState.ThumbnailImageMode.Cat;

        /// <summary>
        /// 图片宽带
        /// </summary>
        public int ImgWidth
        {
            get
            {
                return imgWidth;
            }
            set
            {
                imgWidth = value;
            }
        }
        /// <summary>
        /// 图片高度
        /// </summary>
        public int ImgHeight
        {
            get
            {
                return imgHeight;
            }
            set
            {
                imgHeight = value;
            }
        }
        /// <summary>
        /// 图片模式，HW:指定高宽缩放；W:指定宽，高按比例；H:指定高，宽按比例；Cat:根据缩略图宽和高按比例裁剪
        /// </summary>
        public EnumState.ThumbnailImageMode ImgMode
        {
            get
            {
                return imgMode;
            }
            set
            {
                imgMode = value;
            }
        }
    }

    public class CruiseImageInfo
    {
        public CruiseImageInfo()
        {
            ImageSourceUrl = string.Empty;
            FileName = string.Empty;
            OriginalImage = string.Empty;
            MappingImage = string.Empty;
            ItemId = 0;
            ItemName = string.Empty;
            ImageType = 1;
        }

        /// <summary>
        /// 类别ID
        /// </summary>
        public string DesitemId { get; set; }

        /// <summary>
        /// 对应COM返回id
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// 图片类型1 邮轮图片；2 景区图片；3 国外景区图片
        /// </summary>
        public int ImageType { get; set; }

        /// <summary>
        /// 对应景区名称或者邮轮名称
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 源文件缩略路径（景区和国外景区sql方式存在）
        /// </summary>
        public string ImageSmallUrl { get; set; }
        /// <summary>
        /// 源文件路径
        /// </summary>
        public string ImageSourceUrl { get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 返回本地图片路径
        /// </summary>
        public string OriginalImage { get; set; }
        /// <summary>
        /// 返回公共图片路径
        /// </summary>
        public string MappingImage { get; set; }
    }
}
