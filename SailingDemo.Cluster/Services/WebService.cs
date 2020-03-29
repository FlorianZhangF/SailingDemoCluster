using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SailingDemo.Cluster.Services
{
    public class WebService
    {
        //private static readonly object LockObj = new object();
        //private static HttpClient client = null;
        //static WebService()
        //{
        //    GetInstance();
        //}
        //public static HttpClient GetInstance()
        //{

        //    if (client == null)
        //    {
        //        lock (LockObj)
        //        {
        //            if (client == null)
        //            {
        //                client = new HttpClient();
        //                //{
        //                //    Timeout = new TimeSpan(10000)//十秒超时
        //                //};
        //            }
        //        }
        //    }
        //    return client;
        //}
        public static async Task<string> PostAsync(string url, string strJson)//post异步请求方法
        {
            try
            {
                HttpContent content = new StringContent(strJson);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                //由HttpClient发出异步Post请求
                using (var client = new HttpClient())
                {
                    HttpResponseMessage res = await client.PostAsync(url, content);
                    if (res.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string str = res.Content.ReadAsStringAsync().Result;
                        return str;
                    }
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string Post(string url, string strJson)//post同步请求方法
        {
            try
            {
                HttpContent content = new StringContent(strJson);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                //client.DefaultRequestHeaders.Connection.Add("keep-alive");
                //由HttpClient发出Post请求
                using (var client = new HttpClient())
                {
                    Task<HttpResponseMessage> res = client.PostAsync(url, content);
                    if (res.Result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string str = res.Result.Content.ReadAsStringAsync().Result;
                        return str;
                    }
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<string> Get(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    return client.GetStringAsync(url).Result;
                }  
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(WebService)} Exception {ex.Message} {ex.StackTrace.ToString()}");
                return null;
            }
        }

    }
}
