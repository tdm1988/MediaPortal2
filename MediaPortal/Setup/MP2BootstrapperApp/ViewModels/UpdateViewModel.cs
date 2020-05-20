#region Copyright (C) 2007-2017 Team MediaPortal

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using MP2BootstrapperApp.ChainPackages;
using MP2BootstrapperApp.Commands;
using MP2BootstrapperApp.Models;


namespace MP2BootstrapperApp.ViewModels
{
  public class UpdateViewModel : ObservableBase, IPage
  {
    private readonly IBootstrapperApplicationModel _model;
    private readonly MainViewModel _viewModel;
    private ICommand _updateCommand;
    private ICommand _repairCommand;
    private ICommand _uninstallCommand;
    private readonly PackageContext _packageContext;

    public UpdateViewModel(MainViewModel viewModel, IBootstrapperApplicationModel model, PackageContext packageContext)
    {
      _viewModel = viewModel;
      _model = model;
      _packageContext = packageContext;
      WireUpEventHandlers();
    }

    public string Header
    {
      get { return "Update"; }
    }
    
    public ICommand UpdateCommand
    {
      get { return _updateCommand ?? (_updateCommand = new RelayCommand(o => Update())); }
    }
    
    public ICommand RepairCommand
    {
      get { return _repairCommand ?? (_repairCommand = new RelayCommand(o => Repair())); }
    }
    
    public ICommand UninstallCommand
    {
      get { return _uninstallCommand ?? (_uninstallCommand = new RelayCommand(o => Uninstall())); }
    }
    
    private void WireUpEventHandlers()
    {
      _model.BootstrapperApplication.WrapperDetectRelatedBundle += DetectRelatedBundle;
    }

    private void DetectRelatedBundle(object sender, DetectRelatedBundleEventArgs e)
    {
      _viewModel.Content = _viewModel.UpdateViewModel;
      _viewModel.State = InstallState.Present;
    }

    private void Update()
    {
      foreach (BundlePackage package in _model.BundlePackages)
      {
        if (Enum.TryParse(package.GetId().ToString(), out PackageId id))
        {
          BundlePackage bundlePackage = _model.BundlePackages.FirstOrDefault(pkg => pkg.GetId() == id);
          if (bundlePackage != null)
          {
            SetRequestState(bundlePackage, RequestState.Present);
          }
        }
      }
      _model.PlanAction(LaunchAction.UpdateReplace);
    }

    private void SetRequestState(BundlePackage bundlePackage, RequestState requestState)
    {
      PackageId bundlePackageId = bundlePackage.GetId();
      Version installedVersion = _packageContext.GetInstalledVersion(bundlePackageId);
      PackageState installedPackageState = GetInstallState(installedVersion, bundlePackage.GetVersion());

      if (IsMediaPortalPackage(bundlePackageId) && installedPackageState == PackageState.Present)
      {
        bundlePackage.RequestedInstallState = requestState;
      }
    }
    
    private PackageState GetInstallState(Version installed, Version bundled)
    {
      PackageState state;
      int comparisonResult = installed.CompareTo(bundled);
      if (comparisonResult > 0)
      {
        state = PackageState.Present;
      }
      else if (comparisonResult < 0)
      {
        state = PackageState.Absent;
      }
      else
      {
        state = PackageState.Present;
      }
      return state;
    }
    
    private bool IsMediaPortalPackage(PackageId bundlePackageId)
    {
      return bundlePackageId == PackageId.MP2Client || bundlePackageId == PackageId.MP2Common || bundlePackageId == PackageId.MP2Server;
    }

    private void Repair()
    {
      foreach (BundlePackage package in _model.BundlePackages)
      {
        if (Enum.TryParse(package.GetId().ToString(), out PackageId id))
        {
          BundlePackage bundlePackage = _model.BundlePackages.FirstOrDefault(pkg => pkg.GetId() == id);
          if (bundlePackage != null)
          {
            SetRequestState(bundlePackage, RequestState.Repair);
          }
        }
      }
      _model.PlanAction(LaunchAction.Repair);
    }

    private void Uninstall()
    {
      foreach (BundlePackage package in _model.BundlePackages)
      {
        if (Enum.TryParse(package.GetId().ToString(), out PackageId id))
        {
          BundlePackage bundlePackage = _model.BundlePackages.FirstOrDefault(pkg => pkg.GetId() == id);
          if (bundlePackage != null)
          {
            SetRequestState(bundlePackage, RequestState.ForceAbsent);
          }
        }
      }
      _model.PlanAction(LaunchAction.Uninstall);
    }
  }
}
