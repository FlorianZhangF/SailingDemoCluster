using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using SailingDemo.ClusterClient.Services;

namespace SailingDemo.ClusterClient
{
    public class Startup
    {
        private static bool IsRegisterService = false;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            //注册服务
            //var result = WebService.Get("http://localhost:8000/RegisteService");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            //Console.WriteLine(this.Configuration["ip"]);
            //Console.WriteLine(this.Configuration["port"]);

            //如果未注册
            Console.WriteLine($"http://{Configuration["ip"]}:{Configuration["port"]}");
            var result = WebService.Get($"http://localhost:8000/RegisteService?serviceName=TestService&serviceUrl=http://{Configuration["ip"]}:{Configuration["port"]}");
            Console.WriteLine(result);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            Console.WriteLine($"ThreadId:{Thread.GetCurrentProcessorId()}");

            //app.Use(next => new RequestDelegate(async context =>
            //  {
            //      await next.Invoke(context);
            //      if (!IsRegisterService)
            //      {
            //          //如果未注册
            //          Console.WriteLine($"http://{Configuration["ip"]}:{Configuration["port"]}");
            //          var result = WebService.Get($"https://localhost:44321/RegisteService?serviceUrl=http://{Configuration["ip"]}:{Configuration["port"]}");
            //          Console.WriteLine(result);
            //          IsRegisterService = true;
            //      }
            //  }));
        }
    }
}
