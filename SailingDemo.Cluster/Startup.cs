using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SailingDemo.Cluster.Services;
using SailingDemo.Cluster.Utils;

namespace SailingDemo.Cluster
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            //设置Redis的连接字符串
            var redisConnectionString = ((ConfigurationSection)Configuration.GetSection("RedisConnectionStrings:Connection")).Value;
            var redisInstanceName = ((ConfigurationSection)Configuration.GetSection("RedisConnectionStrings:InstanceName")).Value;
            services.AddSingleton(new RedisCacheHelper(redisConnectionString, redisInstanceName));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async context =>
            {
                if (context.Request.Path.ToString().ToUpper() == "/RegisteService".ToUpper())
                {
                    //注册服务
                    var serverUrl = context.Request.Query["serviceUrl"].ToString();
                    var serviceName = context.Request.Query["serviceName"].ToString();
                    ClusterService.RegisterServer(serviceName, serverUrl);
                    await context.Response.WriteAsync($"{serverUrl}注册成功");
                }
                else if (context.Request.Path.ToString().ToUpper() == "/favicon.ico".ToUpper())
                {
                    await context.Response.WriteAsync($"favicon.ico不处理");
                }
                else
                {
                    context.Response.ContentType = "text/plain;charset=utf-8";
                    var path = context.Request.Path.ToString();
                    var serviceName = string.IsNullOrWhiteSpace(path) ? "" : context.Request.Path.ToString().Split('/')[1];
                    await context.Response.WriteAsync(ClusterService.ForwardRequestGet(serviceName, path.Substring(path.IndexOf(serviceName) + serviceName.Length)));
                }
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //启动健康检查，每十秒检查所有接口一次
            var timer = new Timer(state =>
            {
                ClusterService.HealthCheck();
            }, null, 15000, 30000);

        }
    }
}
