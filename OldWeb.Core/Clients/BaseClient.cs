// <copyright file="BaseClient.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using HandlebarsDotNet;
using OldWeb.Core.Entities;
using OldWeb.Core.Interfaces;
using OldWeb.Core.Utilities;

namespace OldWeb.Core.Clients
{
    /// <summary>
    /// Base Client.
    /// Used when we don't have a hostname for the other clients.
    /// Removes CSS and Javascript.
    /// </summary>
    public class BaseClient : IClient
    {
        /// <summary>
        /// The base host name for the site.
        /// In this case, it's the fall back in case we don't
        /// have a custom client in place.
        /// </summary>
        public const string BaseHostName = "default";

        /// <summary>
        /// The User Agent for the proxied requests.
        /// </summary>
        public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2593.0 Safari/537.36";

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseClient"/> class.
        /// </summary>
        public BaseClient()
        {
            this.Client = new HttpClient();
            this.Client.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue()
            {
                NoCache = true,
            };
            this.Client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            this.Parser = new HtmlParser();
        }

        public HttpClient Client { get; set; }

        public HtmlParser Parser { get; set; }

        /// <inheritdoc/>
        public virtual async Task<Result> GetAsync(Uri uri, string queryString = "")
        {
            string html = string.Empty;
            this.Client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.UtcNow;
            HttpResponseMessage result = await this.Client.GetAsync(uri).ConfigureAwait(false);
            html = await HttpClientHelpers.ReadHtmlAsync(result).ConfigureAwait(false);
            return new Result(result, await this.ModifyBaseHtml(html, uri.Host).ConfigureAwait(false), uri.AbsoluteUri);
        }

        /// <inheritdoc/>
        public virtual async Task<HttpResponseMessage> GetResponseAsync(Uri uri, string queryString = "")
        {
            //this.client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.UtcNow;
            var httpResponseMessage = await this.Client.GetAsync(uri).ConfigureAwait(false);
            return httpResponseMessage;
        }

        public async Task<string> ModifyBaseHtml(string html, string hostname)
        {
            var document = await this.Parser.ParseDocumentAsync(html).ConfigureAwait(false);

            foreach (var node in document.QuerySelectorAll("script"))
            {
                node.Remove();
            }

            foreach (var node in document.QuerySelectorAll("link"))
            {
                node.Remove();
            }

            foreach (var node in document.QuerySelectorAll("img"))
            {
                var src = HttpClientHelpers.ConvertRelativeLinkToAbsolute(node.GetAttribute("src"), hostname);
                if (string.IsNullOrEmpty(src))
                {
                    node.Remove();
                    continue;
                }

                node.SetAttribute("src", $"/file?img={src}");
            }

            foreach (var node in document.QuerySelectorAll("a"))
            {
                var href = HttpClientHelpers.ConvertRelativeLinkToAbsolute(node.GetAttribute("href"), hostname);
                if (string.IsNullOrEmpty(href))
                {
                    node.Remove();
                    continue;
                }

                if (href.StartsWith('#'))
                {
                    continue;
                }

                if (href.StartsWith("javascript", StringComparison.OrdinalIgnoreCase))
                {
                    node.Remove();
                    continue;
                }

                node.SetAttribute("href", $"/site?url={href}");
            }

            return document.DocumentElement.OuterHtml;
        }

        public static Func<object, string> GetTemplate(string path, string hostname)
        {
            var dir = System.Reflection.Assembly.GetAssembly(typeof(BaseClient)).Location;
            var html = File.ReadAllText(Path.Combine(Path.GetDirectoryName(dir), "assets", hostname, path, $"{path}.html.hbs"));
            return Handlebars.Compile(html);
        }
    }
}
