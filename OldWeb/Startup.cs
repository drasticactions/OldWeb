// <copyright file="Startup.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using OldWeb.Core.Clients;
using OldWeb.Core.Interfaces;

namespace OldWeb
{
    /// <summary>
    /// ASP.NET Core Startup.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">ASP.NET Core Configuration.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the Application Configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        public static ILifetimeScope AutofacContainer { get; set; }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">Services Collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterType<BaseClient>().As<IClient>();
            builder.RegisterType<BaseClient>().Named<IClient>("default");
            builder.RegisterType<VergeClient>().Named<IClient>(VergeClient.BaseHostName);
            builder.RegisterType<YouTubeClient>().Named<IClient>(YouTubeClient.BaseHostName);

        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Application Builder.</param>
        /// <param name="env">Web Host Environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            AutofacContainer = app.ApplicationServices.GetAutofacRoot();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            var dir = System.Reflection.Assembly.GetAssembly(typeof(Startup)).Location;

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Path.GetDirectoryName(dir), @"assets")),
                RequestPath = new PathString("/assets"),
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
