using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Mvc;

namespace ZJW.Common.Helper
{
    public class JsonHelper
    {
        /// <summary>
        /// 传入一个URL和一个对象，返回一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="request"></param>
        /// <param name="resultData"></param>
        public static void LoadDataByUrl<T>(string url, object request, Action<T> resultData)
        {
            T temp = default(T);
            string jsonData = GetStrJsonByUrl(url, request);
            //将响应转为实体
            if (!string.IsNullOrEmpty(jsonData))
            {
                temp = Deserialize<T>(jsonData);
            }
            resultData(temp);

        }

        //根据URL获取JSON字符串
        public static string GetJsonByUrl(string url)
        {
            try
            {
                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
                System.IO.Stream responseStream = response.GetResponseStream();
                System.IO.StreamReader sr = new System.IO.StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
                string responseText = sr.ReadToEnd();
                sr.Close();
                sr.Dispose();
                responseStream.Close();
                return responseText;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static void LoadDataByUrl<T>(string url, Action<T> resultData)
        {
            T temp = default(T);
            string jsonData = GetJsonByUrl(url);
            //将响应转为实体
            if (!string.IsNullOrEmpty(jsonData))
            {
                temp = Deserialize<T>(jsonData);
            }
            resultData(temp);

        }

        //public static string Serializer<T>(T obj)
        //{
        //    DataContractJsonSerializer json = new DataContractJsonSerializer(obj.GetType());
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        json.WriteObject(stream, obj);
        //        string szJson = Encoding.UTF8.GetString(stream.ToArray());
        //        return szJson;
        //    }
        //}

        public static string Serializer(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        //[Obsolete]
        //public static T Deserialize<T>(string jsonString)
        //{
        //    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
        //    MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
        //    T obj = (T)ser.ReadObject(ms);
        //    return obj;
        //}

        public static T Deserialize<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        /// <summary> 
        /// Json数据绑定类 
        /// </summary> 
        /// <typeparam name="T"></typeparam> 
        public class JsonBinder<T> : IModelBinder
        {
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                IList<T> list = new List<T>();
                //从请求中获取提交的参数数据 
                var json = controllerContext.HttpContext.Request.Form[bindingContext.ModelName] as string;
                //提交参数是对象 
                if (json.StartsWith("{") && json.EndsWith("}"))
                {
                    JObject jsonBody = JObject.Parse(json);
                    JsonSerializer js = new JsonSerializer();
                    object obj = js.Deserialize(jsonBody.CreateReader(), typeof(T));
                    list.Add((T)obj);
                    return list;
                }
                //提交参数是数组 
                if (json.StartsWith("[") && json.EndsWith("]"))
                {
                    JArray jsonRsp = JArray.Parse(json);
                    if (jsonRsp != null)
                    {
                        for (int i = 0; i < jsonRsp.Count; i++)
                        {
                            JsonSerializer js = new JsonSerializer();
                            object obj = js.Deserialize(jsonRsp[i].CreateReader(), typeof(T));
                            list.Add((T)obj);
                        }
                    }
                    return list;
                }
                return null;
            }
        }
        /// <summary>
        /// 请求一个URL，并POST一些参数过去
        /// </summary>
        /// <param name="url"></param>
        /// <param name="request">POST过去的数据</param>
        /// <returns></returns>
        public static string GetStrJsonByUrl(string url, object request)
        {
            string strPost = Stringify(request);
            //发送POST请求
            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            byte[] arrB = encode.GetBytes(strPost);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
            myReq.Method = "POST";
            //myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            myReq.ContentLength = arrB.Length;
            Stream outStream = myReq.GetRequestStream();
            outStream.Write(arrB, 0, arrB.Length);
            outStream.Close();

            HttpWebResponse myResp;
            try
            {
                //接收HTTP做出的响应
                myResp = (HttpWebResponse)myReq.GetResponse();
            }
            catch (WebException ex)
            {
                myResp = (HttpWebResponse)ex.Response;
            }
            Stream ReceiveStream = myResp.GetResponseStream();
            StreamReader readStream = new StreamReader(ReceiveStream, encode);
            Char[] read = new Char[256];
            int count = readStream.Read(read, 0, 256);
            string str = null;
            while (count > 0)
            {
                str += new String(read, 0, count);
                count = readStream.Read(read, 0, 256);
            }
            readStream.Close();
            myResp.Close();
            return str;

        }

        public static string GetStrJsonByUrl2(string url, string strPost)
        {
            //发送POST请求
            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            byte[] arrB = encode.GetBytes(strPost);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
            myReq.Method = "POST";
            myReq.ContentType = "application/x-www-form-urlencoded";
            //myReq.ContentType = "text/xml";
            myReq.ContentLength = arrB.Length;
            Stream outStream = myReq.GetRequestStream();
            outStream.Write(arrB, 0, arrB.Length);
            outStream.Close();

            HttpWebResponse myResp;
            try
            {
                //接收HTTP做出的响应
                myResp = (HttpWebResponse)myReq.GetResponse();
            }
            catch (WebException ex)
            {
                myResp = (HttpWebResponse)ex.Response;
            }
            Stream ReceiveStream = myResp.GetResponseStream();
            StreamReader readStream = new StreamReader(ReceiveStream, encode);
            Char[] read = new Char[256];
            int count = readStream.Read(read, 0, 256);
            string str = null;
            while (count > 0)
            {
                str += new String(read, 0, count);
                count = readStream.Read(read, 0, 256);
            }
            readStream.Close();
            myResp.Close();
            return str;

        }

        /// <summary>
        /// 将一下对象序列化为一个JSON格式的字符串
        /// </summary>
        /// <param name="jsonObject"></param>
        /// <returns></returns>
        public static string Stringify(object jsonObject)
        {
            using (var ms = new MemoryStream())
            {
                new DataContractJsonSerializer(jsonObject.GetType()).WriteObject(ms, jsonObject);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// Json转objList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static List<T> JsonToObjList<T>(string json)
        {
            List<T> model = JsonConvert.DeserializeObject<List<T>>(json);
            return model;
        }

        /// <summary>
        /// Json转obj
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T JsonToObj<T>(string json)
        {
            T model = JsonConvert.DeserializeObject<T>(json);
            return model;
        }
    }
}