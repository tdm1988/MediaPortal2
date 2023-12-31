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

using MediaPortal.Common.Settings;

namespace MediaPortal.Plugins.SlimTv.Providers.Settings
{
  class MPExtendedProviderSettings
  {
    /// <summary>
    /// Holds the host name or IP adress of the TV4home service (running on same machine as TvServer).
    /// </summary>
    [Setting(SettingScope.User, "localhost")]
    public string TvServerHost { get; set; }
    
    /// <summary>
    /// When MPExtended is configured to require authentication, this Username is used for establishing the connection.
    /// </summary>
    [Setting(SettingScope.User)]
    public string Username { get; set; }

    /// <summary>
    /// When MPExtended is configured to require authentication, this Password is used for establishing the connection.
    /// </summary>
    [Setting(SettingScope.User)]
    public string Password { get; set; }

    /// <summary>
    /// Holds the last selected channel group ID.
    /// </summary>
    [Setting(SettingScope.User)]
    public int LastChannelGroupId { get; set; }
  }
}
