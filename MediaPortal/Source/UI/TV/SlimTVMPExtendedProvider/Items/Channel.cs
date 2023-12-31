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

using System;
using System.Collections.Generic;
using MediaPortal.Plugins.SlimTv.Interfaces.Items;

namespace MediaPortal.Plugins.SlimTv.Providers.Items
{
  public class Channel : IChannel
  {
    #region IChannel Member

    public int ServerIndex { get; set; }

    public int ChannelId { get; set; }

    public int ChannelNumber { get; set; }

    public string Name { get; set; }

    public MediaType MediaType { get; set; }

    public bool EpgHasGaps { get; set; }

    public String ExternalId { get; set; }

    public bool GrapEpg { get; set; }

    public DateTime? LastGrabTime { get; set; }

    public int TimesWatched { get; set; }

    public DateTime? TotalTimeWatched { get; set; }

    public bool VisibleInGuide { get; set; }

    public List<string> GroupNames { get; set; }

    #endregion
  }
}
