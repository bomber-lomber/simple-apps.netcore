using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace App1.StaticFiles
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", GetMainPage());
                endpoints.MapGet("/status", GetApplicationStatus(env));
                endpoints.MapGet("/static-files", GetFilesList(env));
                endpoints.MapGet("/static-files/{**path}", GetFile(env));
            });
        }

        private static RequestDelegate GetFile(IWebHostEnvironment env)
        {
            return async context =>
            {
                var path = context.Request.RouteValues["path"].ToString();
                var file = env.WebRootFileProvider.GetFileInfo(path);
                await context.Response.SendFileAsync(file);
            };
        }

        private static RequestDelegate GetFilesList(IWebHostEnvironment env)
        {
            return async context =>
            {
                var list = string.Empty;
                list += BuildFilesList(env.WebRootFileProvider, "/");
                await context.Response.WriteAsync($@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <title>Static Files</title>
</head>
<body>
    <h1>The static files list</h1>
    <h2>{env.WebRootPath}:</h2>
<ul>
{list}
</ul>
</body>
</html>");
            };
        }

        private static string BuildFilesList(IFileProvider webRootFileProvider, string dirRelativePath)
        {
            var list = string.Empty;
            var directory = webRootFileProvider.GetDirectoryContents(dirRelativePath);
            foreach (var item in directory)
            {
                if (item.IsDirectory)
                {
                    list += BuildFilesList(webRootFileProvider, $"{dirRelativePath}{item.Name}/");
                }
                else
                {
                    var fileUrl = $"static-files{dirRelativePath}{item.Name}";
                    list += $"<li><a href=\"{fileUrl}\">{fileUrl}</a></li>";
                }
            }

            return list;
        }

        private static RequestDelegate GetMainPage()
        {
            return async context =>
            {
                await context.Response.WriteAsync("<h1>Hello World!</h1>Status: <a href='/status'>click here</a>.<br />Static files: <a href='/static-files'>click here</a>.");
            };
        }

        private static RequestDelegate GetApplicationStatus(IWebHostEnvironment env)
        {
            return async context =>
            {

                await context.Response.WriteAsync($@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <title>Status</title>
</head>
<body>
    <h1>Status</h1>
<h2>env.ApplicationName</h2>
<strong>{env.ApplicationName}</strong>
<h2>env.EnvironmentName</h2>
<strong>{env.EnvironmentName}</strong>
<h2>env.ContentRootPath</h2>
<strong>{env.ContentRootPath}</strong>
<h2>env.WebRootPath</h2>
<strong>{env.WebRootPath}</strong>
</body>
</html>");
            };
        }
    }
}
