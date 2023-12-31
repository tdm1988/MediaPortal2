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
using MediaPortal.Common;
using MediaPortal.Common.Configuration.ConfigurationClasses;
using System.Globalization;
using MediaPortal.Common.Localization;
using System.Collections.Generic;
using MediaPortal.Extensions.OnlineLibraries;
using MediaPortal.Common.Settings;

namespace MediaPortal.Plugins.ServerSettings.Settings.Configuration
{
  public class ServerAudioLanguage : SingleSelectionList, IDisposable
  {
    #region Variables

    private IList<CultureInfo> _cultures;

    #endregion

    public ServerAudioLanguage()
    {
      Enabled = false;
      ConnectionMonitor.Instance.RegisterConfiguration(this);

      List<CultureInfo> cultures = new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.SpecificCultures));
      cultures.Sort(CompareByName);
      _cultures = cultures;
    }

    protected static int CompareByName(CultureInfo culture1, CultureInfo culture2)
    {
      return string.Compare(culture1.DisplayName, culture2.DisplayName);
    }

    public override void Load()
    {
      if (!Enabled)
        return;
      IServerSettingsClient serverSettings = ServiceRegistration.Get<IServerSettingsClient>();
      OnlineLibrarySettings settings = serverSettings.Load<OnlineLibrarySettings>();
      if (string.IsNullOrEmpty(settings.MusicLanguageCulture))
        settings.MusicLanguageCulture = "en-US";
      CultureInfo current = new CultureInfo(settings.MusicLanguageCulture);
      _items = new List<IResourceString>(_cultures.Count);
      for (int i = 0; i < _cultures.Count; i++)
      {
        CultureInfo ci = _cultures[i];
        _items.Add(LocalizationHelper.CreateStaticString(ci.DisplayName));
        if (ci.Equals(current))
          Selected = i;
      }
    }

    public override void Save()
    {
      if (!Enabled)
        return;

      base.Save();

      ISettingsManager localSettings = ServiceRegistration.Get<ISettingsManager>();
      IServerSettingsClient serverSettings = ServiceRegistration.Get<IServerSettingsClient>();
      OnlineLibrarySettings settings = serverSettings.Load<OnlineLibrarySettings>();
      settings.MusicLanguageCulture = _cultures[Selected].Name;
      serverSettings.Save(settings);    
      localSettings.Save(settings);   
    }

    public void Dispose()
    {
      ConnectionMonitor.Instance.UnregisterConfiguration(this);
    }
  }
}
