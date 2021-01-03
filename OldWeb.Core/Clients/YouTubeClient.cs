// <copyright file="YouTubeClient.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AngleSharp.Html.Parser;
using OldWeb.Core.Entities;
using OldWeb.Core.Interfaces;
using OldWeb.Core.Utilities;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace OldWeb.Core.Clients
{
    /// <summary>
    /// YouTube Client.
    /// </summary>
    public class YouTubeClient : BaseClient
    {
        private YoutubeClient youtubeClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="YouTubeClient"/> class.
        /// </summary>
        public YouTubeClient()
            : base()
        {
            this.youtubeClient = new YoutubeClient();
        }

        /// <summary>
        /// YouTube HostName.
        /// </summary>
        public new const string BaseHostName = "www.youtube.com";

        public override async Task<Result> GetAsync(Uri uri, string queryString)
        {
            if (uri.Segments.Length > 1)
            {
                switch (uri.Segments[1])
                {
                    case "results":
                        return await this.GetResultPage(queryString).ConfigureAwait(false);
                    case "channel/":
                        return await this.GetChannelPage(uri.Segments[2], queryString).ConfigureAwait(false);
                    case "c/":
                        return await this.GetUserPage(uri.Segments[2], queryString).ConfigureAwait(false);
                    case "user/":
                        return await this.GetUserPage(uri.Segments[2], queryString).ConfigureAwait(false);
                    case "watch":
                        return await this.GetWatchPage(uri.Query).ConfigureAwait(false);
                    default:
                        return await base.GetAsync(uri).ConfigureAwait(false);
                }
            }

            if (uri.Segments.Length == 1)
            {
                return await this.GetRootPage().ConfigureAwait(false);
            }

            return await base.GetAsync(uri).ConfigureAwait(false);
        }

        private async Task<Result> GetWatchPage(string queryString)
        {
            var query = HttpClientHelpers.ParseQueryString(queryString);
            var videoId = query["v"];
            var video = await this.youtubeClient.Videos.GetAsync(videoId).ConfigureAwait(false);
            var template = GetTemplate("watch", BaseHostName);
            var html = template(new YouTubeVideoEntity() { Video = video, VideoUrl = videoId });
            return new Result(null, html, $"https://www.youtube.com/watch?v={videoId}");
        }

        private async Task<Result> GetUserPage(string userId, string queryString)
        {
            var channel = await this.youtubeClient.Channels.GetAsync(userId).ConfigureAwait(false);

            var query = HttpClientHelpers.ParseQueryString(queryString);
            string currentPageString = string.Empty;
            query.TryGetValue("p", out currentPageString);
            int currentPage = string.IsNullOrEmpty(currentPageString) ? 1 : Convert.ToInt32(currentPageString);

            var vid = await this.youtubeClient.Channels.GetUploadsAsync(channel.Id);
            var realVids = vid.Skip(currentPage > 1 ? currentPage * 10 : 0).Take(currentPage * 10).ToList();
            var groupedVids = realVids.Select((video, index) => new { video, index }).GroupBy(g => g.index / 4, i => i.video);
            var template = GetTemplate("channel", BaseHostName);
            var html = template(new YouTubeChannelEntity() { Videos = groupedVids, CurrentPage = currentPage, Channel = channel });
            return new Result(null, html, $"https://www.youtube.com/user/{userId}");
        }

        private async Task<Result> GetChannelPage(string channelId, string queryString)
        {
            var channel = await this.youtubeClient.Channels.GetAsync(channelId).ConfigureAwait(false);

            var query = HttpClientHelpers.ParseQueryString(queryString);
            string currentPageString = string.Empty;
            query.TryGetValue("p", out currentPageString);
            int currentPage = string.IsNullOrEmpty(currentPageString) ? 1 : Convert.ToInt32(currentPageString);

            var vid = await this.youtubeClient.Channels.GetUploadsAsync(channel.Id);
            var realVids = vid.Skip(currentPage > 1 ? currentPage * 10 : 0).Take(currentPage * 10).ToList();
            var groupedVids = realVids.Select((video, index) => new { video, index }).GroupBy(g => g.index / 4, i => i.video);
            var template = GetTemplate("channel", BaseHostName);
            var html = template(new YouTubeChannelEntity() { Videos = groupedVids, CurrentPage = currentPage, Channel = channel });
            return new Result(null, html, $"https://www.youtube.com/channel/{channelId}");
        }

        private async Task<Result> GetResultPage(string queryString)
        {
            var query = HttpClientHelpers.ParseQueryString(queryString);
            var searchQuery = WebUtility.UrlDecode(query["search_query"]);
            string currentPageString = string.Empty;
            query.TryGetValue("p", out currentPageString);
            int currentPage = string.IsNullOrEmpty(currentPageString) ? 1 : Convert.ToInt32(currentPageString);

            var videos = (await this.youtubeClient.Search.GetVideosAsync(searchQuery, currentPage, 1)).ToList();
            var test = videos.Select((video, index) => new { video, index }).GroupBy(g => g.index / 4, i => i.video);
            var template = GetTemplate("results", BaseHostName);
            var html = template(new YouTubeResultEntity() { Videos = test, CurrentPage = currentPage, SearchString = searchQuery });
            return new Result(null, html, "https://www.youtube.com/results");
        }

        private async Task<Result> GetRootPage()
        {
            var template = GetTemplate("index", BaseHostName);
            var html = template(new object());
            return new Result(null, html, "https://www.youtube.com");
        }
    }

    public class YouTubeVideoEntity
    {
        public YoutubeExplode.Videos.Video Video { get; set; }

        public string VideoUrl { get; set; }
    }

    public class YouTubeChannelEntity
    {
        public IEnumerable<IGrouping<int, YoutubeExplode.Videos.Video>> Videos { get; set; }

        public int CurrentPage { get; set; } = 1;

        public int PreviousPage => this.CurrentPage - 1;

        public int NextPage => this.CurrentPage + 1;

        public bool HasPreviousPage => (this.CurrentPage - 1) >= 1;

        public YoutubeExplode.Channels.Channel Channel { get; set; }
    }

    public class YouTubeResultEntity
    {
        public IEnumerable<IGrouping<int, YoutubeExplode.Videos.Video>> Videos { get; set; }

        public int CurrentPage { get; set; } = 1;

        public int PreviousPage => this.CurrentPage - 1;

        public int NextPage => this.CurrentPage + 1;

        public bool HasPreviousPage => (this.CurrentPage - 1) >= 1;

        public string SearchString { get; set; }
    }
}
