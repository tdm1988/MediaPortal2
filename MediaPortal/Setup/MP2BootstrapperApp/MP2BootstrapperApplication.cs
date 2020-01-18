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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Xml.Linq;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using MP2BootstrapperApp.BootstrapperWrapper;
using MP2BootstrapperApp.Models;
using MP2BootstrapperApp.Nav;
using MP2BootstrapperApp.ViewModels;
using MP2BootstrapperApp.Views;

namespace MP2BootstrapperApp
{
  /// <summary>
  /// A custom bootstrapper application. 
  /// </summary>
  public class MP2BootstrapperApplication : BootstrapperApplicationWrapper
  {
    private IDispatcher _dispatcher;
    private IBootstrapperApplicationModel _model;
    private Wizard _wizard;

    protected override void Run()
    {
      _dispatcher = new DispatcherWrapper();

      MessageBox.Show("dd");

      _model = new BootstrapperApplicationModel(this);
      
      InstallWizardViewModel viewModel = new InstallWizardViewModel(_model, _dispatcher);
      InstallWizardView view = new InstallWizardView(viewModel);

      _wizard = new Wizard(viewModel);
      _model.SetWindowHandle(view);

      Engine.Detect();

      view.Show();
      _dispatcher.Run();
      Engine.Quit(_model.FinalResult);
    }
    
    private void WireUpEventHandlers()
    {
      _model.BootstrapperApplication.WrapperDetectRelatedBundle += DetectRelatedBundle2;
  /*    _model.BootstrapperApplication.WrapperDetectPackageComplete += DetectedPackageComplete;
      _model.BootstrapperApplication.WrapperPlanComplete += PlanComplete;
      _model.BootstrapperApplication.WrapperApplyComplete += ApplyComplete;
      _model.BootstrapperApplication.WrapperApplyBegin += ApplyBegin;
      _model.BootstrapperApplication.WrapperExecutePackageBegin += ExecutePackageBegin;
      _model.BootstrapperApplication.WrapperExecutePackageComplete += ExecutePackageComplete;
      _model.BootstrapperApplication.WrapperPlanPackageBegin += PlanPackageBegin;
      _model.BootstrapperApplication.WrapperResolveSource += ResolveSource;
      _model.BootstrapperApplication.WrapperCacheAcquireProgress += CacheAcquireProgress;
      _model.BootstrapperApplication.WrapperExecuteProgress += ExecuteProgress; */
    }
    
    private void DetectRelatedBundle2(object sender, DetectRelatedBundleEventArgs e)
    {
      _wizard.GoToOverview();
      //CurrentPage = new InstallExistTypePageViewModel(this);
    }
    
    private List<BundlePackage> ComputeBundlePackages()
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
      return packages != null ? packages.ToList() : new List<BundlePackage>();
    }
  }
}
