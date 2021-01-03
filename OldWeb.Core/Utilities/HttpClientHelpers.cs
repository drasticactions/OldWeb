// <copyright file="HttpClientHelpers.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OldWeb.Core.Utilities
{
    /// <summary>
    /// Http Client Helpers.
    /// </summary>
    public static class HttpClientHelpers
    {
        /// <summary>
        /// Read Html out of <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="message"><see cref="HttpResponseMessage"/></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<string> ReadHtmlAsync(HttpResponseMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var stream = await message.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var reader = new StreamReader(stream);
            string html = reader.ReadToEnd();
            return html;
        }

        /// <summary>
        /// Converts links to absolute links.
        /// </summary>
        /// <param name="href">Original href.</param>
        /// <param name="hostname">Hostname.</param>
        /// <returns>Updated string.</returns>
        public static string ConvertRelativeLinkToAbsolute(string href, string hostname)
        {
            if (string.IsNullOrEmpty(href))
            {
                return string.Empty;
            }

            if (href.StartsWith("//www", StringComparison.OrdinalIgnoreCase))
            {
                return $"http:{href}";
            }

            if (href.StartsWith("//", StringComparison.OrdinalIgnoreCase))
            {
                return $"{hostname}/{href}";
            }

            if (href.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                return $"http://{hostname}{href}";
            }

            return $"{href}";
        }

        /// <summary>
        /// Parses a query string for a given URL.
        /// </summary>
        /// <param name="s">The URL or query string to be parsed.</param>
        /// <returns>A key value dictionary.</returns>
        public static Dictionary<string, string> ParseQueryString(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            var nvc = new Dictionary<string, string>();

            // remove anything other than query string from url
            if (s.Contains("?"))
            {
                s = s.Substring(s.IndexOf('?') + 1);
            }

            foreach (string vp in Regex.Split(s, "&"))
            {
                string[] singlePair = Regex.Split(vp, "=");
                if (singlePair.Length == 2)
                {
                    nvc.Add(singlePair[0], singlePair[1]);
                }
                else
                {
                    // only one key with no value specified in query string
                    nvc.Add(singlePair[0], string.Empty);
                }
            }

            return nvc;
        }
    }
}
