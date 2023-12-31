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

using MediaPortal.Common.Configuration.ConfigurationClasses;

namespace MediaPortal.Plugins.SlimTv.Client.Settings.Configuration
{
  public class EpgNumberOfHoursSetting : LimitedNumberSelect
  {
    public override void Load()
    {
      _type = NumberType.FloatingPoint;
      _step = 0.5;
      _lowerLimit = 1.0;
      _upperLimit = 4.0;
      _value = SettingsManager.Load<SlimTvClientSettings>().EpgVisibleHours;
    }

    public override void Save()
    {
      SlimTvClientSettings settings = SettingsManager.Load<SlimTvClientSettings>();
      settings.EpgVisibleHours = _value;
      SettingsManager.Save(settings);
    }
  }
}