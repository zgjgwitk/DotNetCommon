using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ZJW.Common.Helper
{
    public class XmlHelper
    {
        #region 自行解析xml
        /// <summary>
        /// 获取请求结果
        /// </summary>
        /// <param name="requestUrl">请求地址</param>
        /// <param name="timeout">超时时间(秒)</param>
        /// <param name="requestXml">请求xml内容</param>
        /// <param name="isPost">是否post提交</param>
        /// <param name="msg">抛出的错误信息</param>
        /// <returns>返回请求结果</returns>
        public static string HttpPostWebRequest(string requestUrl, int timeout, string requestXml, bool isPost, out string msg)
        {
            return HttpPostWebRequest(requestUrl, timeout, requestXml, isPost, "utf-8", out msg);
        }

        /// <summary>
        /// 获取请求结果
        /// </summary>
        /// <param name="requestUrl">请求地址</param>
        /// <param name="timeout">超时时间(秒)</param>
        /// <param name="requestXml">请求xml内容</param>
        /// <param name="isPost">是否post提交</param>
        /// <param name="encoding">编码格式 例如:utf-8</param>
        /// <param name="msg">抛出的错误信息</param>
        /// <returns>返回请求结果</returns>
        public static string HttpPostWebRequest(string requestUrl, int timeout, string requestXml, bool isPost, string encoding, out string msg)
        {
            msg = string.Empty;
            var result = string.Empty;
            try
            {
                var bytes = Encoding.GetEncoding(encoding).GetBytes(requestXml ?? string.Empty);
                var request = (HttpWebRequest)WebRequest.Create(requestUrl);
                request.ContentType = "application/x-www-form-urlencoded";
                request.Referer = requestUrl;
                request.Method = isPost ? "POST" : "GET";
                request.ContentLength = bytes.Length;
                request.Timeout = timeout * 1000;
                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Close();
                }

                var response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    var reader = new StreamReader(responseStream, Encoding.GetEncoding(encoding));
                    result = reader.ReadToEnd();
                    reader.Close();
                    responseStream.Close();
                    request.Abort();
                    response.Close();
                    return result.Trim();
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message + ex.StackTrace;
            }

            return result;
        }

        /// <summary>
        /// get方式输出结果
        /// </summary>
        /// <param name="responseUrl"></param>
        /// <param name="timeOut">秒</param>
        /// <param name="msg"> </param>
        public static void HttpGetWebResponse(string responseUrl, int timeOut, out string msg)
        {
            msg = string.Empty;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(responseUrl);
                request.Method = "GET";
                request.Timeout = timeOut * 1000;
                request.ContentType = "application/x-www-form-urlencoded";
                request.GetResponse();
                request.Abort();
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
        }

        /// <summary>
        /// get方式输出结果
        /// </summary>
        /// <param name="responseUrl"></param>
        /// <param name="timeOut">秒</param>
        /// <param name="responseParams">输出参数</param>
        /// <param name="msg"> </param>
        public static void HttpGetWebResponse(string responseUrl, int timeOut, string responseParams, out string msg)
        {
            var url = responseUrl;
            if (url.Trim().Contains("?"))
            {
                url += "&" + responseParams;
            }
            else
            {
                url += "?" + responseParams;
            }

            HttpGetWebResponse(url, timeOut, out msg);
        }

        /// <summary>
        /// 通过POST提交方式获取XML数据
        /// </summary>
        /// <param name="requestXml">请求XML内容</param>
        /// <param name="url">请求URL</param>
        /// <param name="inputCharset">请求字符集</param>
        /// <returns></returns>
        public static XmlDocument GetXmlByPost(string requestXml, string url, string inputCharset)
        {
            string msg;
            var resultInfo = HttpPostWebRequest(url, 30, requestXml, true, out msg);
            if (string.IsNullOrEmpty(resultInfo))
            {
                return null;
            }
            var xml = new XmlDocument();
            xml.LoadXml(resultInfo);

            return xml;
        }

        /// <summary>
        /// 获取通知地址
        /// </summary>
        /// <param name="responseUrl"></param>
        /// <param name="responseParams"></param>
        /// <returns></returns>
        public static string GetWebResponseUrl(string responseUrl, string responseParams)
        {
            var url = responseUrl;
            if (url.Trim().Contains("?"))
            {
                url += "&" + responseParams;
            }
            else
            {
                url += "?" + responseParams;
            }

            return url;
        }

        /// <summary>
        /// 获取节点值
        /// </summary>
        /// <param name="xmlD"></param>
        /// <param name="selectSingleNode"></param>
        /// <returns></returns>
        public static string GetSingleNodeValue(XmlDocument xmlD, string selectSingleNode)
        {
            var result = string.Empty;
            if (xmlD != null)
            {
                var node = xmlD.SelectSingleNode(selectSingleNode);
                if (node != null)
                {
                    result = node.InnerText;
                }
            }

            return result;
        }
        /// <summary>
        /// 从<param name="parentNode">父节点中</param>查询
        /// <param name="childNodeName">子节点</param>返回其中的Text
        /// </summary>
        /// <param name="parentNode">父节点</param>
        /// <param name="childNodeName">子节点名称</param>
        /// <returns></returns>
        public static string GetNodeText(XmlNode parentNode, string childNodeName)
        {
            if (parentNode == null)
            {
                return string.Empty;
            }
            var node = parentNode.SelectSingleNode(childNodeName);
            return node == null ? "" : node.InnerText.Trim();
        }

        /// <summary>
        /// 获取节点的xml
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="childNodeName"></param>
        /// <returns></returns>
        public static string GetNodeXml(XmlNode parentNode, string childNodeName)
        {
            if (Equals(parentNode, null))
            {
                return "";
            }
            var node = parentNode.SelectSingleNode(childNodeName);
            if (Equals(node, null))
            {
                return "<" + childNodeName + " />";
            }

            return node.OuterXml;
        }
        #endregion

        #region 反序列化
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="xml">XML字符串</param>
        /// <returns></returns>
        public static object Deserialize(Type type, string xml)
        {
            try
            {
                using (StringReader sr = new StringReader(xml))
                {
                    XmlSerializer xmldes = new XmlSerializer(type);
                    return xmldes.Deserialize(sr);
                }
            }
            catch (Exception e)
            {

                return null;
            }
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="type"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static object Deserialize(Type type, Stream stream)
        {
            XmlSerializer xmldes = new XmlSerializer(type);
            return xmldes.Deserialize(stream);
        }
        #endregion

        #region 序列化
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static string Serializer(Type type, object obj)
        {
            MemoryStream Stream = new MemoryStream();
            XmlSerializer xml = new XmlSerializer(type);
            try
            {
                //序列化对象
                xml.Serialize(Stream, obj);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            Stream.Position = 0;
            StreamReader sr = new StreamReader(Stream);
            string str = sr.ReadToEnd();

            sr.Dispose();
            Stream.Dispose();

            return str;
        }

        #endregion

        //下面是测试代码：
        //1. 实体对象转换到Xml
        private void test()
        {
            Student stu1 = new Student() { Name = "okbase", Age = 10 };
            string xml = XmlHelper.Serializer(typeof(Student), stu1);
            Console.Write(xml);

            //2. Xml转换到实体对象
            Student stu2 = XmlHelper.Deserialize(typeof(Student), xml) as Student;
            Console.Write(string.Format("名字:{0},年龄:{1}", stu2.Name, stu2.Age));

            //3. DataTable转换到Xml
            // 生成DataTable对象用于测试
            DataTable dt1 = new DataTable("mytable");   // 必须指明DataTable名称
            dt1.Columns.Add("Dosage", typeof(int));
            dt1.Columns.Add("Drug", typeof(string));
            dt1.Columns.Add("Patient", typeof(string));
            dt1.Columns.Add("Date", typeof(DateTime));

            // 添加行
            dt1.Rows.Add(25, "Indocin", "David", DateTime.Now);
            dt1.Rows.Add(50, "Enebrel", "Sam", DateTime.Now);
            dt1.Rows.Add(10, "Hydralazine", "Christoff", DateTime.Now);
            dt1.Rows.Add(21, "Combivent", "Janet", DateTime.Now);
            dt1.Rows.Add(100, "Dilantin", "Melanie", DateTime.Now);

            // 序列化
            xml = XmlHelper.Serializer(typeof(DataTable), dt1);
            Console.Write(xml);

            //4. Xml转换到DataTable
            // 反序列化

            DataTable dt2 = XmlHelper.Deserialize(typeof(DataTable), xml) as DataTable;

            // 输出测试结果
            foreach (DataRow dr in dt2.Rows)
            {
                foreach (DataColumn col in dt2.Columns)
                {
                    Console.Write(dr[col].ToString() + " ");
                }

                Console.Write("\r\n");
            }

            //5. List转换到Xml
            // 生成List对象用于测试

            List<Student> list1 = new List<Student>(3);

            list1.Add(new Student() { Name = "okbase", Age = 10 });
            list1.Add(new Student() { Name = "csdn", Age = 15 });
            // 序列化
            xml = XmlHelper.Serializer(typeof(List<Student>), list1);
            Console.Write(xml);

            //6. Xml转换到List

            List<Student> list2 = XmlHelper.Deserialize(typeof(List<Student>), xml) as List<Student>;
            foreach (Student stu in list2)
            {
                Console.WriteLine(stu.Name + "," + stu.Age.ToString());
            }
        }

        public class Student
        {
            public string Name { set; get; }
            public int Age { set; get; }
        }
    }
}
