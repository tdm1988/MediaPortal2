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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using System.Xml.Linq;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using MP2BootstrapperApp.BootstrapperWrapper;
using MP2BootstrapperApp.ChainPackages;
using MP2BootstrapperApp.Commands;
using MP2BootstrapperApp.Models;

namespace MP2BootstrapperApp.ViewModels
{
  public class MainViewModel : ObservableBase
  {
    public enum InstallState
    {
      Initializing,
      Present,
      NotPresent,
      Applaying,
      Canceled
    }

    private readonly IBootstrapperApplicationModel _model;
    private readonly IDispatcher _dispatcher;
    private string _header;
    private InstallState _state;
    private int _progress;
    private int _cacheProgress;
    private int _executeProgress;
    private readonly PackageContext _packageContext;

    private readonly InstallViewModel _installViewModel;
    private readonly UpdateViewModel _updateViewModel;

    private IPage _content;

    public MainViewModel(IBootstrapperApplicationModel model, IDispatcher dispatcher)
    {
      _model = model;
      _dispatcher = dispatcher;
      _packageContext = new PackageContext();
      _updateViewModel = new UpdateViewModel(this, model);
      _installViewModel = new InstallViewModel(this, model);
      Content = _updateViewModel; // set to a initialization view 
      WireUpEventHandlers();
      ComputeBundlePackages();
    }
    
    public IPage Content
    {
      get { return _content; }
      set { Set(ref _content, value); }
    }

    public InstallState State
    {
      get { return _state; }
      private set { Set(ref _state, value); }
    }
  
    public string Header
    {
      get { return _header; }
      set { Set(ref _header, value); }
    }

    public ICommand CancelCommand { get; }
    
    public IList<BundlePackage> BundlePackages { get; set; }

    public int Progress
    {
      get { return _progress; }
      set { Set(ref _progress, value); }
    }

    private void Cancel()
    {
      if (State == InstallState.Applaying)
      {
        State = InstallState.Canceled;
      }
      else
      {
        _dispatcher.InvokeShutdown();
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
        BundlePackage bundlePackage = BundlePackages.FirstOrDefault(pkg => pkg.GetId() == detectedPackageId);
        if (bundlePackage != null)
        {
          PackageId bundlePackageId = bundlePackage.GetId();
          Version installed = _packageContext.GetInstalledVersion(bundlePackageId);
          bundlePackage.InstalledVersion = installed;
          bundlePackage.CurrentInstallState = GetInstallState(installed, bundlePackage.GetVersion());
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

    private void DetectRelatedBundle(object sender, DetectRelatedBundleEventArgs e)
    {
      
    }

    private void PlanComplete(object sender, PlanCompleteEventArgs e)
    {  
      if (State == InstallState.Canceled)
      {
        _dispatcher.InvokeShutdown();
        return;
      }
      _model.ApplyAction();
    }

    private void ApplyBegin(object sender, ApplyBeginEventArgs e)
    {
      State = InstallState.Applaying;
    }

    private void ExecutePackageBegin(object sender, ExecutePackageBeginEventArgs e)
    {
      if (State == InstallState.Canceled)
      {
        e.Result = Result.Cancel;
      }
    }

    private void ExecutePackageComplete(object sender, ExecutePackageCompleteEventArgs e)
    {
      if (State == InstallState.Canceled)
      {
        e.Result = Result.Cancel;
      }
    }

    private void ApplyComplete(object sender, ApplyCompleteEventArgs e)
    {
      _model.FinalResult = e.Status;
    }

    private void PlanPackageBegin(object sender, PlanPackageBeginEventArgs planPackageBeginEventArgs)
    {
      UpdatePackageRequestState(planPackageBeginEventArgs);
    }
    
    private void InstallClientAndServer()
    {
      foreach (BundlePackage package in BundlePackages)
      {
        if (package.CurrentInstallState != PackageState.Present)
        {
          package.RequestedInstallState = RequestState.Present;
        }
      }
      _model.PlanAction(LaunchAction.Install);
    }

    private void UpdatePackageRequestState(PlanPackageBeginEventArgs planPackageBeginEventArgs)
    {
      if (Enum.TryParse(planPackageBeginEventArgs.PackageId, out PackageId id))
      {
        BundlePackage bundlePackage = BundlePackages.FirstOrDefault(p => p.GetId() == id);
        if (bundlePackage != null)
        {
          planPackageBeginEventArgs.State = bundlePackage.RequestedInstallState;
        }
      }
    }

    private void ResolveSource(object sender, ResolveSourceEventArgs e)
    {
      e.Result = !string.IsNullOrEmpty(e.DownloadSource) ? Result.Download : Result.Ok;
    }

    private void CacheAcquireProgress(object sender, CacheAcquireProgressEventArgs e)
    {
      _cacheProgress = e.OverallPercentage;
      Progress = (_cacheProgress + _executeProgress) / 2;
    }

    private void ExecuteProgress(object sender, ExecuteProgressEventArgs e)
    {
      _executeProgress = e.OverallPercentage;
      Progress = (_cacheProgress + _executeProgress) / 2;
    }

    private void WireUpEventHandlers()
    {
      _model.BootstrapperApplication.WrapperDetectRelatedBundle += DetectRelatedBundle;
      _model.BootstrapperApplication.WrapperDetectPackageComplete += DetectedPackageComplete;
      _model.BootstrapperApplication.WrapperPlanComplete += PlanComplete;
      _model.BootstrapperApplication.WrapperApplyComplete += ApplyComplete;
      _model.BootstrapperApplication.WrapperApplyBegin += ApplyBegin;
      _model.BootstrapperApplication.WrapperExecutePackageBegin += ExecutePackageBegin;
      _model.BootstrapperApplication.WrapperExecutePackageComplete += ExecutePackageComplete;
      _model.BootstrapperApplication.WrapperPlanPackageBegin += PlanPackageBegin;
      _model.BootstrapperApplication.WrapperResolveSource += ResolveSource;
      _model.BootstrapperApplication.WrapperCacheAcquireProgress += CacheAcquireProgress;
      _model.BootstrapperApplication.WrapperExecuteProgress += ExecuteProgress;
    }

    private void ComputeBundlePackages()
    {
      IEnumerable<BundlePackage> packages = new List<BundlePackage>();
    
      XNamespace manifestNamespace = "http://schemas.microsoft.com/wix/2010/BootstrapperApplicationData";
    
      string manifestPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      if (manifestPath != null)
      {
        const string bootstrapperApplicationData = "BootstrapperApplicationData";
        const string xmlExtension = ".xml";
        string bootstrapperDataFilePath = Path.Combine(manifestPath, bootstrapperApplicationData + xmlExtension);
        XElement bundleManifestData;
    
        using (StreamReader reader = new StreamReader(bootstrapperDataFilePath))
        {
          string xml = reader.ReadToEnd();
          XDocument xDoc = XDocument.Parse(xml);
          bundleManifestData = xDoc.Element(manifestNamespace + bootstrapperApplicationData);
        }
    
        const string wixMbaPrereqInfo = "WixMbaPrereqInformation";
        IList<BootstrapperAppPrereqPackage> mbaPrereqPackages = bundleManifestData?.Descendants(manifestNamespace + wixMbaPrereqInfo)
          .Select(x => new BootstrapperAppPrereqPackage(x))
          .ToList();
    
        const string wixPackageProperties = "WixPackageProperties";
        packages = bundleManifestData?.Descendants(manifestNamespace + wixPackageProperties)
          .Select(x => new BundlePackage(x))
          .Where(pkg => mbaPrereqPackages.All(preReq => preReq.PackageId != pkg.GetId()));
      }
    
      BundlePackages = packages != null
        ? new ReadOnlyCollection<BundlePackage>(packages.ToList())
        : new ReadOnlyCollection<BundlePackage>(new List<BundlePackage>());
    }
  }
}
