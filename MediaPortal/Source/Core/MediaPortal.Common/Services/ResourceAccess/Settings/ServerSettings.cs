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

using System.Linq;
using MediaPortal.Common.Logging;
using MediaPortal.Common.Settings;
using System.Collections.Generic;

namespace MediaPortal.Common.Services.ResourceAccess.Settings
{
  public class ServerSettings
  {
    protected int _httpServerPort = 0;
    protected bool _useIPv4 = true;
    protected bool _useIPv6 = false;
    protected bool _limitIPEndpoints = false;
    protected string _ipAddressBindings = null;
    protected bool _webAutorizationEnabled = true;

    [Setting(SettingScope.Global)]
    public int HttpServerPort
    {
      get { return _httpServerPort; }
      set { _httpServerPort = value; }
    }

    [Setting(SettingScope.Global)]
    public bool UseIPv4
    {
      get { return _useIPv4; }
      set { _useIPv4 = value; }
    }

    [Setting(SettingScope.Global, false)]
    public bool UseIPv6
    {
      get { return _useIPv6; }
      set { _useIPv6 = value; }
    }

    [Setting(SettingScope.Global)]
    public string IPAddressBindings
    {
      get { return _ipAddressBindings; }
      set { _ipAddressBindings = value; }
    }

    [Setting(SettingScope.Global, false)]
    public bool LimitIPEndpoints
    {
      get { return _limitIPEndpoints; }
      set { _limitIPEndpoints = value; }
    }

    [Setting(SettingScope.Global, true)]
    public bool WebAutorizationEnabled
    {
      get { return _webAutorizationEnabled; }
      set { _webAutorizationEnabled = value; }
    }

    /// <summary>
    /// Gets a list of <see cref="IPAddressBindings"/> that are already split by <c>,</c>.
    /// </summary>
    public List<string> IPAddressBindingsList
    {
      get
      {
        if (string.IsNullOrWhiteSpace(IPAddressBindings))
          return null;
        ServiceRegistration.Get<ILogger>().Debug("ServerSettings: Allow connections only for IP address bindings {0}", IPAddressBindings);
        return new List<string>(IPAddressBindings.Split(new[] { ',' }).Select(a => a.Trim()));
      }
    }
  }
}
