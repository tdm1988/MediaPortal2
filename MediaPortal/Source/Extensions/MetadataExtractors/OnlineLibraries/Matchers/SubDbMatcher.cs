#region Copyright (C) 2007-2021 Team MediaPortal

/*
    Copyright (C) 2007-2021 Team MediaPortal
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

using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Extensions.OnlineLibraries.Wrappers;
using System;
using System.Threading.Tasks;

namespace MediaPortal.Extensions.OnlineLibraries.Matchers
{
  public class SubDbMatcher : SubtitleMatcher<string>
  {
    public const string NAME = "Thesubdb.com";

    #region Init

    public SubDbMatcher() : base(NAME)
    {
      //Will be overridden if the user enables it in settings
      Enabled = true;
    }

    public override Task<bool> InitWrapperAsync(bool useHttps)
    {
      try
      {
        SubDbWrapper wrapper = new SubDbWrapper(NAME);
        if (wrapper.Init())
        {
          _wrapper = wrapper;
          return Task.FromResult(true);
        }
      }
      catch (Exception ex)
      {
        ServiceRegistration.Get<ILogger>().Error("SubDbMatcher: Error initializing wrapper", ex);
      }
      return Task.FromResult(false);
    }

    #endregion
  }
}
