// <copyright file="FileController.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Logging;
using OldWeb.Core.Interfaces;
using Svg;
using Xabe.FFmpeg;
using YoutubeExplode;

namespace OldWeb.Controllers
{
    /// <summary>
    /// File Proxy.
    /// Used to get images (and other files) from sites that are secured.
    /// Basically, it's a really stupid controller that only returns file requests.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {
        private IClient client;
        private string assemblyPath;
        private string outputPath;
        private YoutubeClient youtubeClient;

        public FileController(IClient client)
        {
            this.client = client;
            this.assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.outputPath = Path.Join(this.assemblyPath, "output");
            Directory.CreateDirectory(this.outputPath);
            this.youtubeClient = new YoutubeClient();
        }

        /// <summary>
        /// Get Request.
        /// </summary>
        /// <returns>Action Result.</returns>
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            if (this.Request.Query.ContainsKey("img"))
            {
                var test = this.Request.Query["img"].ToString();
                if (test.Contains("base64,"))
                {
                    return this.NoContent();
                }

                var imgurl = new Uri(this.Request.Query["img"]);
                var result = await this.client.GetResponseAsync(imgurl, string.Empty).ConfigureAwait(false);
                var contentStream = await result.Content.ReadAsStreamAsync().ConfigureAwait(false);

                // TODO: It should actually return the real content type, not just png.
                // TODO: Convert SVGs to images.
                // TODO: Support base64 images.
                // But these old browsers don't seem to care about that, so good enough for now!
                return this.File(contentStream, "image/png", true);
            }
            else if (this.Request.Query.ContainsKey("video"))
            {
                string name = this.Request.Query.ContainsKey("name") ? this.Request.Query["name"].ToString() : Path.GetTempFileName();
                string type = this.Request.Query.ContainsKey("type") ? this.Request.Query["type"].ToString() : string.Empty;

                var url = string.Empty;
                switch (type)
                {
                    case "youtube":
                        var videoId = this.Request.Query["video"].ToString();
                        var video = await this.youtubeClient.Videos.GetAsync(videoId).ConfigureAwait(false);
                        var streamManifest = await this.youtubeClient.Videos.Streams.GetManifestAsync(videoId).ConfigureAwait(false);
                        var streamInfo = streamManifest.GetMuxed().Where(n => n.Container == YoutubeExplode.Videos.Streams.Container.Mp4);
                        var streamurl = streamInfo.FirstOrDefault();
                        url = streamurl.Url;
                        break;
                    default:
                        url = this.Request.Query["video"].ToString();
                        break;
                }

                var path = Path.Join(this.outputPath, $"{name}.wmv");
                if (!System.IO.File.Exists(path))
                {
                    var conversionResult = await FFmpeg.Conversions.New()
            .AddParameter($"-i {url}")
            .AddParameter($"-user-agent \"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2593.0 Safari/537.36\"")
            .AddParameter($"-vf scale=320:-1")
            .AddParameter($"-b:v 24k")
            .AddParameter($"-vcodec wmv1")
            .AddParameter($"-acodec wmav1")
            .AddParameter($"-b:a 24k")
            .SetOutput(path)
            .Start().ConfigureAwait(false);
                }

                return this.File(System.IO.File.OpenRead(path), "video/x-ms-wmv", $"{name}.wmv", true);
            }

            return this.NoContent();
        }
    }
}
