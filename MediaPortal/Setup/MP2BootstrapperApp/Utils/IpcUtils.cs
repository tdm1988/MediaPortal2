﻿#region Copyright (C) 2007-2021 Team MediaPortal

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
using MediaPortal.Utilities.Process;
using System;

namespace MP2BootstrapperApp.Utils
{
  public static class IpcUtils
  {
    public static bool StopMediaPortalProcesses()
    {
      try
      {
        ServiceRegistration.Get<ILogger>().Info("Stopping running MP2 processes");
        // the server service needs quite some time to exit, so we specify an timeout of 30 seconds
        // this timeout is used for every single process
        // also, if any process has not exited after this time, kill it
        IpcClient.ShutdownAllApplications(30000, true);
        return true;
      }
      catch (Exception ex)
      {
        ServiceRegistration.Get<ILogger>().Info("StopProcesses: Error: " + ex.Message);
        return false;
      }
    }
  }
}