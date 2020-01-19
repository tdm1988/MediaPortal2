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
using MP2BootstrapperApp.Models;
using MP2BootstrapperApp.Nav;

namespace MP2BootstrapperApp.ViewModels
{
  public class InstallWizardViewModel : ViewModelBase
  {
    public enum InstallState
    {
      Initializing,
      Present,
      NotPresent,
      Applaying,
      Canceled
    }

    private readonly IBootstrapperApplicationModel _bootstrapperApplicationModel;
    private string _header;
    private bool _canExec;
    private string _buttonNextContent;
    private string _buttonBackContent;
    private string _buttonCancelContent;
    private string _errorCode;
    private InstallState _state;
    private int _progress;
    private int _cacheProgress;
    private int _executeProgress;
    private readonly PackageContext _packageContext;
    private readonly IDispatcher _dispatcher;
    private Wizard _wizard;
    
    private InstallType _installType;
    private EActionType _actionType;

    private IPage content;
    
    public IPage Content
    {
      get { return content; }
      set { Set(ref content, value); }
    }

  /*  public RelayCommand<int> NavigateCommand
    {
      get { return new RelayCommand<int>(Navigate, CanExecute); }
    } */

    private bool CanExecute(int page)
    {
      IPage newPage = pages[page].Value;
      return newPage.CanExecute;
    }

    private readonly Dictionary<int, Lazy<IPage>> pages = new Dictionary<int, Lazy<IPage>>
    {
      [1] = new Lazy<IPage>(() => new OverviewViewModel()),
      [2] = new Lazy<IPage>(() => new Page2ViewModel())
    };
    
    public InstallWizardViewModel(IBootstrapperApplicationModel model, IDispatcher dispatcher)
    {

      _bootstrapperApplicationModel = model;
      _dispatcher = dispatcher;
      State = InstallState.Initializing;
      _packageContext = new PackageContext();

      Content = new OverviewViewModel();
    //  WireUpEventHandlers();
    //  ComputeBundlePackages();

     // NextCommand = new RelayCommand<int>(i => _wizard.ChangeState(this));
      //  NextCommand = new DelegateCommand(Next, () => true);  
  //    BackCommand = new DelegateCommand(Back, () => true);
    //  CancelCommand = new DelegateCommand(Cancel, () => State != InstallState.Canceled);
      ButtonNextContent = "next";
      ButtonBackContent = "back";
      ButtonCancelContent = "cancel";
      Navigate(1);
    }

    private void Navigate(int page)
    {
      Content = pages[page].Value;
      switch (page)
      {
        case 1:
          doSomethingForOne();
          break;
        case 2:
          doSomethingForOne();
          break;
      }
    }

    private void doSomethingForOne()
    {
      
    }

    private void Next()
    {
      switch (ActionType)
      {
        case EActionType.Install:
          Install();
          break;
        case EActionType.Update:
          Update();
          break;
        case EActionType.Modify:
          break;
        case EActionType.Repair:
          break;
        case EActionType.Uninstall:
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
    
    private void Install()
    {
      switch (InstallType)
      {
        case InstallType.ClientServer:
          InstallClientAndServer();
          break;
        case InstallType.Client:
          break;
        case InstallType.Server:
          break;
        case InstallType.Custom:
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
    
    private void Update()
    {
      //update installation
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
      _bootstrapperApplicationModel.PlanAction(LaunchAction.Install);
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
    
    public bool CanExec
    {
      get { return _canExec; }
      set { Set(ref _canExec, value); }
    }

    public string ButtonNextContent
    {
      get { return _buttonNextContent; }
      set { Set(ref _buttonNextContent, value); }
    }

    public string ButtonBackContent
    {
      get { return _buttonBackContent; }
      set { Set(ref _buttonBackContent, value); }
    }

    public string ButtonCancelContent
    {
      get { return _buttonCancelContent; }
      set { Set(ref _buttonCancelContent, value); }
    }
    
    public string ErrorCode
    {
      get { return _errorCode; }
      set { Set(ref _errorCode, value); }
    }

    public ICommand CancelCommand { get; }
    public ICommand NextCommand { get; set; }
    public ICommand BackCommand { get; }

    public InstallType InstallType
    {
      get { return _installType; }
      set { Set(ref _installType, value); }
    }
    
    public EActionType ActionType
    {
      get { return _actionType; }
      set { Set(ref _actionType, value); }
    }
    
    public ReadOnlyCollection<BundlePackage> BundlePackages { get; private set; }

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
      //CurrentPage = new InstallExistTypePageViewModel(this);
    }

    private void PlanComplete(object sender, PlanCompleteEventArgs e)
    {
      if (State == InstallState.Canceled)
      {
        _dispatcher.InvokeShutdown();
        return;
      }
      _bootstrapperApplicationModel.ApplyAction();
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

      _bootstrapperApplicationModel.FinalResult = e.Status;
      ErrorCode = "Error code: 0x" + e.Status.ToString("x8");
    }

    private void PlanPackageBegin(object sender, PlanPackageBeginEventArgs planPackageBeginEventArgs)
    {
      UpdatePackageRequestState(planPackageBeginEventArgs);
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
      _bootstrapperApplicationModel.BootstrapperApplication.WrapperDetectRelatedBundle += DetectRelatedBundle;
      _bootstrapperApplicationModel.BootstrapperApplication.WrapperDetectPackageComplete += DetectedPackageComplete;
      _bootstrapperApplicationModel.BootstrapperApplication.WrapperPlanComplete += PlanComplete;
      _bootstrapperApplicationModel.BootstrapperApplication.WrapperApplyComplete += ApplyComplete;
      _bootstrapperApplicationModel.BootstrapperApplication.WrapperApplyBegin += ApplyBegin;
      _bootstrapperApplicationModel.BootstrapperApplication.WrapperExecutePackageBegin += ExecutePackageBegin;
      _bootstrapperApplicationModel.BootstrapperApplication.WrapperExecutePackageComplete += ExecutePackageComplete;
      _bootstrapperApplicationModel.BootstrapperApplication.WrapperPlanPackageBegin += PlanPackageBegin;
      _bootstrapperApplicationModel.BootstrapperApplication.WrapperResolveSource += ResolveSource;
      _bootstrapperApplicationModel.BootstrapperApplication.WrapperCacheAcquireProgress += CacheAcquireProgress;
      _bootstrapperApplicationModel.BootstrapperApplication.WrapperExecuteProgress += ExecuteProgress;
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
