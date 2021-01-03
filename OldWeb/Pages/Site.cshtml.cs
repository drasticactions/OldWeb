// <copyright file="Site.cshtml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OldWeb.Core.Interfaces;

namespace OldWeb.Pages
{
    public class SiteModel : PageModel
    {
        public string Html { get; set; }

        public async Task OnGet(string url)
        {
            var uri = new Uri(url);
            var hostname = uri.Host;
            var client = Startup.AutofacContainer.ResolveOptionalKeyed<IClient>(hostname);
            if (client == null)
            {
                client = Startup.AutofacContainer.ResolveKeyed<IClient>("default");
            }

            var test = await client.GetAsync(uri, this.Request.QueryString.HasValue ? this.Request.QueryString.Value : string.Empty).ConfigureAwait(false);
            this.Html = test.ResultText;
        }
    }
}
