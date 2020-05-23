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
using System.Linq;
using System.Windows.Input;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using MP2BootstrapperApp.BootstrapperWrapper;
using MP2BootstrapperApp.ChainPackages;
using MP2BootstrapperApp.Commands;
using MP2BootstrapperApp.Models;

namespace MP2BootstrapperApp.ViewModels
{
  public class InstallViewModel : ObservableBase, IPage
  {
    private readonly MainViewModel _viewModel;
    private readonly IBootstrapperApplicationModel _model;
    private readonly IDispatcher _dispatcher;
    private ICommand _installClientAndServerCommand;
    private ICommand _installClientCommand;
    private ICommand _installServerCommand;
    private readonly PackageContext _packageContext;

    public InstallViewModel(MainViewModel viewModel, IBootstrapperApplicationModel model, PackageContext packageContext, IDispatcher dispatcher)
    {
      _dispatcher = dispatcher;
      _viewModel = viewModel;
      _model = model;
      _packageContext = packageContext;
      WireUpEventHandlers();
    }

    public string Header
    {
      get { return "Install"; }
    }

    public ICommand InstallClientAndServerCommand
    {
      get { return _installClientAndServerCommand ?? (_installClientAndServerCommand = new RelayCommand(o => InstallClientAndServer())); }
    }
    
    public ICommand InstallClientCommand
    {
      get { return _installClientCommand ?? (_installClientCommand = new RelayCommand(o => InstallClient())); }
    }

    public ICommand InstallServerCommand
    {
      get { return _installServerCommand ?? (_installServerCommand = new RelayCommand(o => InstallServer())); }
    }
    
    private void InstallClientAndServer()
    {
      foreach (BundlePackage package in _model.BundlePackages)
      {
        if (package.CurrentInstallState != PackageState.Present)
        {
          package.RequestedInstallState = RequestState.Present;
        }
      }
      _model.PlanAction(LaunchAction.Install);
    }

    private void InstallClient()
    {
      foreach (BundlePackage package in _model.BundlePackages)
      {
        if (package.CurrentInstallState == PackageState.Present || package.GetId() == PackageId.MP2Server)
        {
          continue;
        }
        package.RequestedInstallState = RequestState.Present;
      }
      _model.PlanAction(LaunchAction.Install);
    }
    
    private void InstallServer()
    {
      foreach (BundlePackage package in _model.BundlePackages)
      {
        if (package.CurrentInstallState == PackageState.Present || package.GetId() == PackageId.MP2Client || package.GetId() == PackageId.LAVFilters)
        {
          continue;
        }
        package.RequestedInstallState = RequestState.Present;
      }
      _model.PlanAction(LaunchAction.Install);
    }
    
    private void DetectComplete(object sender, DetectCompleteEventArgs e)
    {

    }
    
    private void ResolveSource(object sender, ResolveSourceEventArgs e)
    {
      e.Result = !string.IsNullOrEmpty(e.DownloadSource) ? Result.Download : Result.Ok;
    }
    
    private void PlanComplete(object sender, PlanCompleteEventArgs e)
    {  
      if (_viewModel.State == InstallState.Canceled)
      {
        _dispatcher.InvokeShutdown();
        return;
      }
      _model.ApplyAction();
    }

    private void ApplyBegin(object sender, ApplyBeginEventArgs e)
    {
      _viewModel.State = InstallState.Applying;
      _viewModel.Content = _viewModel.ProgressViewModel;
    }

    private void ExecutePackageBegin(object sender, ExecutePackageBeginEventArgs e)
    {
      if (_viewModel.State == InstallState.Canceled)
      {
        e.Result = Result.Cancel;
      }
    }

    private void ExecutePackageComplete(object sender, ExecutePackageCompleteEventArgs e)
    {
      if (_viewModel.State == InstallState.Canceled)
      {
        e.Result = Result.Cancel;
      }
    }

    private void ApplyComplete(object sender, ApplyCompleteEventArgs e)
    {
      _model.FinalResult = e.Status;
      _viewModel.State = e.Status >= 0 ? InstallState.Applied : InstallState.Failed;
      _viewModel.Content = _viewModel.CompleteViewModel;
    }

    private void PlanPackageBegin(object sender, PlanPackageBeginEventArgs planPackageBeginEventArgs)
    {
      UpdatePackageRequestState(planPackageBeginEventArgs);
    }

    private void UpdatePackageRequestState(PlanPackageBeginEventArgs planPackageBeginEventArgs)
    {
      if (Enum.TryParse(planPackageBeginEventArgs.PackageId, out PackageId id))
      {
        BundlePackage bundlePackage = _model.BundlePackages.FirstOrDefault(p => p.GetId() == id);
        if (bundlePackage != null)
        {
          planPackageBeginEventArgs.State = bundlePackage.RequestedInstallState;
        }
      }
    }
    
    private void DetectedPackageComplete(object sender, DetectPackageCompleteEventArgs detectPackageCompleteEventArgs)
    {
      UpdatePackageCurrentState(detectPackageCompleteEventArgs);
    }

    private void UpdatePackageCurrentState(DetectPackageCompleteEventArgs detectPackageCompleteEventArgs)
    {
      if (Enum.TryParse(detectPackageCompleteEventArgs.PackageId, out PackageId detectedPackageId))
      {
        BundlePackage bundlePackage = _model.BundlePackages.FirstOrDefault(pkg => pkg.GetId() == detectedPackageId);
        if (bundlePackage != null)
        {
          PackageId bundlePackageId = bundlePackage.GetId();
          Version installedVersion = _packageContext.GetInstalledVersion(bundlePackageId);
          bundlePackage.InstalledVersion = installedVersion;
          bundlePackage.CurrentInstallState = GetInstallState(installedVersion, bundlePackage.GetVersion());
        }
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
    
    private void WireUpEventHandlers()
    {
      _model.BootstrapperApplication.WrapperDetectComplete += DetectComplete; 
      _model.BootstrapperApplication.WrapperDetectPackageComplete += DetectedPackageComplete;
      _model.BootstrapperApplication.WrapperPlanComplete += PlanComplete;
      _model.BootstrapperApplication.WrapperApplyComplete += ApplyComplete;
      _model.BootstrapperApplication.WrapperApplyBegin += ApplyBegin;
      _model.BootstrapperApplication.WrapperExecutePackageBegin += ExecutePackageBegin;
      _model.BootstrapperApplication.WrapperExecutePackageComplete += ExecutePackageComplete;
      _model.BootstrapperApplication.WrapperPlanPackageBegin += PlanPackageBegin;
      _model.BootstrapperApplication.WrapperResolveSource += ResolveSource;


    }
  }
}
