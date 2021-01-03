// <copyright file="Program.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OldWeb
{
    /// <summary>
    /// Entrance to the web service.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entrance to program.
        /// </summary>
        /// <param name="args">Program Arguments.</param>
        public static void Main(string[] args)
        {
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), @"assets"));
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates Host Builder.
        /// </summary>
        /// <param name="args">Program Arguments.</param>
        /// <returns>Host Builder.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://*:5000");
                });
    }
}
