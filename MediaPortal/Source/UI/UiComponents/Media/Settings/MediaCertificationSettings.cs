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

namespace MediaPortal.UiComponents.Media.Settings
{
  public class MediaCertificationSettings
  {
    /// <summary>
    /// Two letter country/region code for the content certification system to use for displaying movie certifications.
    /// For "US", "G", "PG", "PG-13" etc. would be displayed. For an empty string ("") the original certification imported is used.
    /// </summary>
    [Setting(SettingScope.User, "")]
    public string DisplayMovieCertificationCountry { get; set; }

    /// <summary>
    /// Two letter country/region code for the content certification system to use for displaying series certifications.
    /// For "US", "TV-G", "TV-PG", "TV-14" etc. would be displayed. For an empty string ("") the original certification imported is used.
    /// </summary>
    [Setting(SettingScope.User, "")]
    public string DisplaySeriesCertificationCountry { get; set; }
  }
}
