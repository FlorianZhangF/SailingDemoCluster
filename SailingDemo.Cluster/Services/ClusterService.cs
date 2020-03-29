using Newtonsoft.Json;
using SailingDemo.Cluster.Models;
using SailingDemo.Cluster.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SailingDemo.Cluster.Services
{
    public static class ClusterService
    {
        //public static IDictionary<string, List<ServerModel>> serverDic = new Dictionary<string, List<ServerModel>>();

        private static string ServerDictionayKeyName = "ClusterServerDictionary";

        private static int index = 0;

        private static readonly object oLock = new object();

        private static Dictionary<string, List<ServerModel>> GetServerDic()
        {
            lock (oLock)
            {
                var serverDic = RedisCacheHelper.Get<Dictionary<string, List<ServerModel>>>(ServerDictionayKeyName);
                if (serverDic == null)
                {
                    return new Dictionary<string, List<ServerModel>>();
                }
                else
                {
                    return serverDic;
                }
            }
        }

        private static bool SetServerDic(IDictionary<string, List<ServerModel>> serverDic)
        {
            lock (oLock)
            {
                return RedisCacheHelper.SetStringValue(ServerDictionayKeyName, JsonConvert.SerializeObject(serverDic ?? new Dictionary<string, List<ServerModel>>()));

            }
        }


        /// <summary>
        /// 健康检查
        /// </summary>
        public static async void HealthCheck()
        {
            var serverDic = GetServerDic();
            foreach (var service in serverDic)
            {
                var serverList = service.Value;
                for (var i = 0; i < serverList.Count; i++)
                {
                    if (await WebService.Get(serverList[i].HealthCheckUrl) == null)
                    {
                        Console.WriteLine($"Remove {serverList[i].ServerUrl} HealthCheckUrl {serverList[i].HealthCheckUrl}");
                        serverList.Remove(serverList[i--]);
                        lock (oLock)
                        {
                            if (index > 0)
                            {
                                index--;
                            }
                        }
                    }
                }
            }
            SetServerDic(serverDic);
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="serverUrl"></param>
        public static void RegisterServer(string serviceName, string serverUrl)
        {
            var serverDic = GetServerDic();
            if (!serverDic.ContainsKey(serviceName))
            {
                serverDic.Add(serviceName, new List<ServerModel>());
            }
            var serverList = serverDic[serviceName];
            var existServer = false;
            foreach (var item in serverList)
            {
                if (item.ServerUrl == serverUrl)
                {
                    existServer = true;
                    break;
                }
            }
            if (!existServer)
            {
                serverList.Add(new ServerModel()
                {
                    ServerUrl = serverUrl,
                    HealthCheckUrl = serverUrl + "/WeatherForecast"
                });
            }

            SetServerDic(serverDic);
        }

        /// <summary>
        /// 转发Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string ForwardRequestGet(string serviceName, string path)
        {
            var serverDic = GetServerDic();
            Console.WriteLine($"index:{index} list:{JsonConvert.SerializeObject(serverDic)}");
            var url = GetServerUrl(serviceName);
            Console.WriteLine($"url:{url}");
            if (string.IsNullOrWhiteSpace(url))
            {
                return "无可用服务器";
            }
            else
            {
                return WebService.Get(url + path).Result;
            }
        }

        /// <summary>
        /// 获取可用的请求地址
        /// </summary>
        private static string GetServerUrl(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                return "";
            }

            var serverDic = GetServerDic();
            if (!serverDic.ContainsKey(serviceName) || serverDic[serviceName].Count == 0)
            {
                return "";
            }
            else
            {
                lock (oLock)
                {
                    //轮询取服务器
                    var serverList = serverDic[serviceName];
                    string url = serverList[index++].ServerUrl;
                    index %= serverList.Count;
                    return url;
                }
            }
        }


    }
}
