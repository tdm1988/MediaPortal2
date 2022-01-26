﻿#region Copyright (C) 2007-2017 Team MediaPortal

/*
    Copyright (C) 2007-2017 Team MediaPortal
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

using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using MP2BootstrapperApp.Models;

namespace MP2BootstrapperApp.WizardSteps
{
  public class UpdateStep : AbstractInstallStep, IStep
  {
    public UpdateStep(IBootstrapperApplicationModel bootstrapperApplicationModel)
      : base(bootstrapperApplicationModel)
    {
    }
    
    public IStep Next()
    {
      foreach (var bundlePackage in _bootstrapperApplicationModel.BundlePackages)
      {
        if (bundlePackage.CurrentInstallState == PackageState.Absent)
        {
          bundlePackage.RequestedInstallState = RequestState.Present;
        }
      }
      return new InstallOverviewStep(_bootstrapperApplicationModel);
    }

    public bool CanGoNext()
    {
      return true;
    }

    public bool CanGoBack()
    {
      return true;
    }
  }
}