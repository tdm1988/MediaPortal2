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

using System.Collections.Generic;
using MediaPortal.Common.Settings;

namespace MediaPortal.Plugins.InputDeviceManager
{
  public class InputManagerSettings
  {
    #region Variables

    private List<InputDevice> _inputDevices;

    #endregion Variables

    #region Properties

    /// <summary>
    /// Gets the transceiver list.
    /// </summary>
    [Setting(SettingScope.Global)]
    public List<InputDevice> InputDevices
    {
      get { return _inputDevices; }
      set { _inputDevices = value; }
    }

    #endregion Properties
  }
}
