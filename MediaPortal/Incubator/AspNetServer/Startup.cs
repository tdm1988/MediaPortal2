﻿#region Copyright (C) 2007-2015 Team MediaPortal

/*
    Copyright (C) 2007-2015 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System.Collections.Generic;
using System.Linq;
using MediaPortal.Plugins.AspNetServer.PlatformServices;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;

namespace MediaPortal.Plugins.AspNetServer
{
  public class Item
  {
    public int Id;
    public string Name;
  }

  [Route("api/[Controller]")]
  public class ItemsController : Controller
  {
    public static List<Item> Items = new List<Item>
    {
      new Item { Id = 1, Name = "First Test Item" },
      new Item { Id = 2, Name = "Second Test Item" },
    };

    [HttpGet]
    public IEnumerable<Item> Get()
    {
      return Items;
    }

    [HttpGet("{id}")]
    public Item Get(int id)
    {
      return Items.FirstOrDefault(item => item.Id == id);
    }
  }

  public class Startup
  {
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddSingleton<IApplicationEnvironment>(new MP2ApplicationEnvironment());
      services.AddTransient(typeof(ILibraryManager), typeof(MP2LibraryManager));
      services.AddMvc();
    }

    public void Configure(IApplicationBuilder app)
    {
      app.UseFileServer(new FileServerOptions
      {
        FileProvider = new PhysicalFileProvider(@"C:\"),
        RequestPath = new PathString("/StaticFiles"),
        EnableDirectoryBrowsing = true,
      });
      app.UseMvc();
      app.Run(context => context.Response.WriteAsync("Hello World"));
    }
  }

}