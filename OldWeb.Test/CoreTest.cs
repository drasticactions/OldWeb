// <copyright file="CoreTest.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using OldWeb.Core.Clients;
using Xunit;

namespace OldWeb.Test
{
    public class CoreTest
    {
        [Fact]
        public async Task BaseClientTest()
        {
            var testClient = new BaseClient();
            var result = await testClient.GetAsync(new Uri("https://www.asahi.com/"), string.Empty).ConfigureAwait(false);
            System.IO.File.WriteAllText("test.html", result.ResultText);
        }

        [Fact]
        public async Task VergeClientTest()
        {
            var testClient = new VergeClient();
            var result = await testClient.GetAsync(new Uri("https://www.theverge.com/"), string.Empty).ConfigureAwait(false);
            System.IO.File.WriteAllText("test.html", result.ResultText);
        }

        [Fact]
        public async Task YouTubeClientTest()
        {
            var testClient = new YouTubeClient();
            var result = await testClient.GetAsync(new Uri("https://www.youtube.com/"), string.Empty).ConfigureAwait(false);
            System.IO.File.WriteAllText("test.html", result.ResultText);
        }
    }
}
