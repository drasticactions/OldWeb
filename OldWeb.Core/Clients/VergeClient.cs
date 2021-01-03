// <copyright file="VergeClient.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using OldWeb.Core.Entities;
using OldWeb.Core.Interfaces;
using OldWeb.Core.Utilities;

namespace OldWeb.Core.Clients
{
    /// <summary>
    /// Client for The Verge.
    /// TODO: Could be the base for a generic blog/rss client.
    /// </summary>
    public class VergeClient : BaseClient
    {
        /// <summary>
        /// Verge HostName.
        /// </summary>
        public new const string BaseHostName = "www.theverge.com";

        public override async Task<Result> GetAsync(Uri uri, string queryString)
        {
            if (uri.Segments.Length == 6)
            {
                return await this.GetArticlePage(uri).ConfigureAwait(false);
            }

            if (uri.Segments.Length == 1)
            {
                return await this.GetRootPage().ConfigureAwait(false);
            }

            return await base.GetAsync(uri).ConfigureAwait(false);
        }

        private async Task<Result> GetArticlePage(Uri articleUri)
        {
            var stringResult = await this.Client.GetStringAsync(articleUri).ConfigureAwait(false);
            var document = await this.Parser.ParseDocumentAsync(stringResult).ConfigureAwait(false);
            var webTitle = document.Title;
            var articleTitle = document.QuerySelector(".c-page-title").Text();
            var articleSummary = document.QuerySelector(".c-entry-summary").Text();
            var articleByline = document.QuerySelector(".c-byline-wrapper").Text();
            var articleHeroImage = document.QuerySelector(".e-image--hero");
            var articleHeroImageSrc = string.Empty;
            if (articleHeroImage != null)
            {
                articleHeroImageSrc = articleHeroImage.QuerySelector("img")?.GetAttribute("src");
            }

            var articleHtml = await this.ModifyBaseHtml(document.QuerySelector(".c-entry-content").OuterHtml, BaseHostName).ConfigureAwait(false);
            var template = GetTemplate("article", BaseHostName);
            var html = template(new VergeArticle() { ArticleByline = articleByline, ArticleHeroImage = articleHeroImageSrc, ArticleTitle = articleTitle, ArticleHtml = articleHtml, WebTitle = webTitle});
            return new Result(null, html, articleUri.ToString());
        }

        private async Task<Result> GetRootPage()
        {
            XmlReader reader = XmlReader.Create("https://www.theverge.com/rss/index.xml");
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            reader.Close();
            foreach (var item in feed.Items)
            {
                if (item.Content is TextSyndicationContent textContent)
                {
                    var firstHtml = await this.ModifyBaseHtml(textContent.Text, BaseHostName).ConfigureAwait(false);
                    firstHtml = await this.ModifyPostHtml(firstHtml).ConfigureAwait(false);
                    item.Content = new TextSyndicationContent(firstHtml);
                }
            }

            var template = GetTemplate("index", BaseHostName);
            var html = template(feed);
            return new Result(null, html, "https://www.theverge.com");
        }

        private async Task<string> ModifyPostHtml(string html)
        {
            var document = await this.Parser.ParseDocumentAsync(html).ConfigureAwait(false);

            foreach (var node in document.QuerySelectorAll("img"))
            {
                node.SetAttribute("width", $"320");
            }

            return document.DocumentElement.OuterHtml;
        }
    }

    public class VergeArticle
    {
        public string WebTitle { get; set; }

        public string ArticleTitle { get; set; }

        public string ArticleByline { get; set; }

        public string ArticleHtml { get; set; }

        public string ArticleHeroImage { get; set; }

        public bool HasHeroImage => string.IsNullOrEmpty(this.ArticleHeroImage) == false;
    }
}
