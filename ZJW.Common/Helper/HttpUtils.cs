using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace ZJW.Common.Helper
{

    /// <summary>
    ///HttpUtils 的摘要说明
    /// </summary>
    public static class HttpUtils
    {
        public static string ParseUrl(string urlStr)
        {
            string str = HttpContext.Current.Request.QueryString[urlStr];
            if (!string.IsNullOrEmpty(str))
            {
                return HttpUtility.UrlDecode(str);
            }
            else
            {
                return "";
            }
        }

        public static string PostData()
        {
            string postStr = string.Empty;
            Stream s = HttpContext.Current.Request.InputStream;
            byte[] b = new byte[s.Length];
            s.Read(b, 0, (int)s.Length);
            postStr = Encoding.UTF8.GetString(b);
            if (!string.IsNullOrEmpty(postStr))
            {
                return postStr;
            }
            else
            {
                return "";
            }

            //using (var reader = new System.IO.StreamReader(HttpContext.Current.Request.InputStream, Encoding.UTF8))
            //{
            //    String xmlData = reader.ReadToEnd();
            //    if (!string.IsNullOrEmpty(xmlData))
            //    {
            //        return xmlData;
            //    }
            //    else
            //    {
            //        return "";
            //    }
            //}
        }

        #region
        /// <summary>
        /// POST请求接口
        /// </summary>
        /// <param name="url"></param>
        /// <param name="jsonParas">请求实体</param>
        /// <param name="parasType">1:json格式参数,其他:xml格式参数</param>
        /// <returns>返回结果</returns>
        public static string HttpPost(string url, string jsonParas, int parasType = 0)
        {
            string result = string.Empty;
            //创建一个HTTP请求  
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            //POST请求方式  
            myRequest.Method = "POST";
            //内容类型
            if (parasType == 1)
                myRequest.ContentType = "application/json;charset=utf-8";
            else
                myRequest.ContentType = "application/xml;charset=utf-8";

            //设置参数，并进行URL编码  将Json字符串转化为字节  
            byte[] payload = HttpUtility.UrlEncodeToBytes(jsonParas, Encoding.UTF8);
            myRequest.ContentLength = payload.Length;

            try
            {

                //发送请求，获得请求流 
                Stream writer = myRequest.GetRequestStream();//获取用于写入请求数据的Stream对象
                                                             //将请求参数写入流
                writer.Write(payload, 0, payload.Length);
                writer.Close();//关闭请求流

                /*Stream myRequestStream = myRequest.GetRequestStream();
                StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("gb2312"));
                myStreamWriter.Write(jsonParas);
                myStreamWriter.Close();*/

                //http请求响应 
                HttpWebResponse HttpWResp = (HttpWebResponse)myRequest.GetResponse();
                //获得响应流
                Stream myStream = HttpWResp.GetResponseStream();
                //用特定的字符编码为指定的流初始化
                StreamReader sr = new StreamReader(myStream, Encoding.UTF8);
                result = sr.ReadToEnd();
                sr.Close();
                myStream.Close();
                HttpWResp.Close();
            }
            catch (WebException exp)
            {
                HttpWebResponse res = (HttpWebResponse)exp.Response;
                StreamReader sr = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
                result = sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                result = "错误：" + ex.Message;
            }
            return result;
        }
        #endregion

        #region 第二个POST请求
        /// <summary>
        /// POST请求接口
        /// </summary>
        /// <param name="postUrl"></param>
        /// <param name="paramData"></param>
        /// <param name="dataEncode"></param>
        /// <returns></returns>
        public static string PostWebRequest(string postUrl, string paramData, Encoding dataEncode)
        {
            string ret = string.Empty;
            try
            {
                byte[] byteArray = dataEncode.GetBytes(paramData); //转化
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
                webReq.Method = "POST";
                webReq.ContentType = "application/x-www-form-urlencoded";

                webReq.ContentLength = byteArray.Length;
                Stream newStream = webReq.GetRequestStream();
                newStream.Write(byteArray, 0, byteArray.Length);//写入参数
                newStream.Close();
                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), dataEncode);
                ret = sr.ReadToEnd();
                sr.Close();
                response.Close();
                newStream.Close();
            }
            catch (Exception ex)
            {
                return ex.Message;
                // MessageBox.Show(ex.Message);
            }
            return ret;
        }
        #endregion


        /// <summary>  
        /// GET请求与获取结果  
        /// </summary>  
        public static string HttpGet(string url, string postDataStr = "")
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

    }
}