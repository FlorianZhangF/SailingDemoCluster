using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SailingDemo.Cluster.Models
{
    /// <summary>
    /// 注册的服务
    /// </summary>
    public class ServerModel
    {
        /// <summary>
        /// IP加端口
        /// </summary>
        public string ServerUrl { get; set; }
        /// <summary>
        /// 健康检查请求地址
        /// </summary>
        public string HealthCheckUrl { get; set; }
    }
}
