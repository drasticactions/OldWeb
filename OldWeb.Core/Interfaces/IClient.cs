// <copyright file="IClient.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using OldWeb.Core.Entities;

namespace OldWeb.Core.Interfaces
{
    /// <summary>
    /// Client handler for sites.
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// The base host name for the site.
        /// </summary>
        public const string BaseHostName = "";

        /// <summary>
        /// The User Agent for the proxied requests.
        /// </summary>
        public const string UserAgent = "";

        /// <summary>
        /// Performs a GET Result on the given Uri.
        /// </summary>
        /// <param name="uri">Uri of the request.</param>
        /// <param name="queryString">Original query string.</param>
        /// <returns><see cref="Result"/>.</returns>
        Task<Result> GetAsync(Uri uri, string queryString);

        /// <summary>
        /// Performs a GET HttpResponseMessage on the given Uri.
        /// </summary>
        /// <param name="uri">Uri of the request.</param>
        /// <param name="queryString">Original query string.</param>
        /// <returns><see cref="HttpResponseMessage"/>.</returns>
        Task<HttpResponseMessage> GetResponseAsync(Uri uri, string queryString);
    }
}
