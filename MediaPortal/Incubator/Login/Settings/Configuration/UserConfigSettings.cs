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
using MediaPortal.Common.Configuration.ConfigurationClasses;
using MediaPortal.Common.Settings;
using MediaPortal.UI.Services.UserManagement;

namespace MediaPortal.UiComponents.Login.Settings.Configuration
{
  public class UserConfigSettings : YesNo
  {
    public override void Load()
    {
      if (!Enabled)
        return;
      ISettingsManager localSettings = ServiceRegistration.Get<ISettingsManager>();
      UserSettings settings = localSettings.Load<UserSettings>();
      Yes = settings.EnableUserLogin;
    }

    public override void Save()
    {
      if (!Enabled)
        return;

      base.Save();

      ISettingsManager localSettings = ServiceRegistration.Get<ISettingsManager>();
      UserSettings settings = localSettings.Load<UserSettings>();
      settings.EnableUserLogin = Yes;
      localSettings.Save(settings);
      UserSettingStorage.UserLoginEnabled = Yes;
    }
  }
}
