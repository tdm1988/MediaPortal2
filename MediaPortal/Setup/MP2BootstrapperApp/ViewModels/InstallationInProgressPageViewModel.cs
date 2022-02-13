﻿
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

using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using MP2BootstrapperApp.ChainPackages;
using MP2BootstrapperApp.Models;
using MP2BootstrapperApp.WizardSteps;
using System;

namespace MP2BootstrapperApp.ViewModels
{
  public class InstallationInProgressPageViewModel : InstallWizardPageViewModelBase
  {

    const string DEFAULT_ACTION = "Processing...";
    protected IBootstrapperApplicationModel _bootstrapperModel;

    protected readonly object _syncObj = new object();
    protected int _cacheProgress;
    protected int _executeProgress;
    protected int _progress;
    protected string _currentPackage;
    protected string _currentAction;

    public InstallationInProgressPageViewModel(InstallationInProgressStep step)
      : base(step)
    {
      _bootstrapperModel = step.BootstrapperApplicationModel;
      _currentAction = DEFAULT_ACTION;

      Header = GetHeaderForAction(_bootstrapperModel.PlannedAction);
    }

    public int Progress
    {
      get { return _progress; }
      set { SetProperty(ref _progress, value); }
    }

    public string CurrentPackage
    {
      get { return _currentPackage; }
      set { SetProperty(ref _currentPackage, value); }
    }

    public string CurrentAction
    {
      get { return _currentAction; }
      set { SetProperty(ref _currentAction, value); }
    }

    public override void Attach()
    {
      base.Attach();
      AttachProgressHandlers();
    }

    public override void Detach()
    {
      base.Detach();
      DetachProgressHandlers();
    }

    private string GetHeaderForAction(LaunchAction action)
    {
      switch (action)
      {
        case LaunchAction.Install:
          return "Installing";
        case LaunchAction.Modify:
          return "Modifying";
        case LaunchAction.Repair:
          return "Repairing";
        case LaunchAction.Uninstall:
          return "Uninstalling";
        default:
          return "Processing";
      }
    }

    private void AttachProgressHandlers()
    {
      _bootstrapperModel.BootstrapperApplication.CacheAcquireProgress += CacheAquireProgress;
      _bootstrapperModel.BootstrapperApplication.ExecuteProgress += ExecuteProgress;
      _bootstrapperModel.BootstrapperApplication.ExecuteMsiMessage += ExecuteMsiMessage;
    }

    private void DetachProgressHandlers()
    {
      _bootstrapperModel.BootstrapperApplication.CacheAcquireProgress -= CacheAquireProgress;
      _bootstrapperModel.BootstrapperApplication.ExecuteProgress -= ExecuteProgress;
      _bootstrapperModel.BootstrapperApplication.ExecuteMsiMessage -= ExecuteMsiMessage;
    }

    private void CacheAquireProgress(object sender, CacheAcquireProgressEventArgs e)
    {
      string packageName = Enum.TryParse(e.PackageOrContainerId, out PackageId packageId) ? packageId.ToString() : string.Empty;
      lock (_syncObj)
      {
        _cacheProgress = e.OverallPercentage;
        Progress = (_cacheProgress + _executeProgress) / 2;
        CurrentPackage = packageName;
        CurrentAction = $"Caching: {packageName}";
      }
    }

    private void ExecuteProgress(object sender, ExecuteProgressEventArgs e)
    {
      string packageName = Enum.TryParse(e.PackageId, out PackageId packageId) ? packageId.ToString() : string.Empty;
      lock (_syncObj)
      {
        _executeProgress = e.OverallPercentage;
        Progress = (_cacheProgress + _executeProgress) / 2;
        if (packageName != CurrentPackage)
        {
          CurrentPackage = packageName;
          // Reset the message if the package has changed
          CurrentAction = DEFAULT_ACTION;
        }
      }
    }

    private void ExecuteMsiMessage(object sender, ExecuteMsiMessageEventArgs e)
    {
      if (e.MessageType == InstallMessage.ActionStart)
      {
        _bootstrapperModel.LogMessage(LogLevel.Standard, $"Display: {e.DisplayParameters}, Message: {e.Message}, Data: {string.Join(",,", e.Data ?? new string[0])}");
        
        // This is quite noisy, the best I can tell is that human readable messages for display seem to be the second argument in the message data, when present
        if (e.Data != null && e.Data.Count > 1 && !string.IsNullOrEmpty(e.Data[1]))
        {
          string packageName = Enum.TryParse(e.PackageId, out PackageId packageId) ? packageId.ToString() : string.Empty;
          lock (_syncObj)
          {
            CurrentPackage = packageName;
            CurrentAction = e.Data[1].Trim();
          }
        }
      }
    }
  }
}