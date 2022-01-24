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
using MP2BootstrapperApp.BootstrapperWrapper;
using MP2BootstrapperApp.ChainPackages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Xml.Linq;

namespace MP2BootstrapperApp.Models
{
  /// <summary>
  /// Model class for the <see cref="MP2BootstrapperApplication"/>
  /// </summary>
  public class BootstrapperApplicationModel : IBootstrapperApplicationModel
  {
    private IntPtr _hwnd;
    private readonly PackageContext _packageContext;

    public BootstrapperApplicationModel(IBootstrapperApp bootstreApplication)
    {
      BootstrapperApplication = bootstreApplication;
      _hwnd = IntPtr.Zero;
      _packageContext = new PackageContext();
      WireUpEventHandlers();
      ComputeBundlePackages();
    }

    public IBootstrapperApp BootstrapperApplication { get; }
   
    public int FinalResult { get; set; }

    public ReadOnlyCollection<BundlePackage> BundlePackages { get; private set; }

    public void SetWindowHandle(Window view)
    {
      _hwnd = new WindowInteropHelper(view).Handle;
    }

    public void PlanAction(LaunchAction action)
    {
      BootstrapperApplication.Engine.Plan(action);
    }

    public void ApplyAction()
    {
      BootstrapperApplication.Engine.Apply(_hwnd);
    }

    public void LogMessage(LogLevel logLevel, string message)
    {
      BootstrapperApplication.Engine.Log(logLevel, message);
    }

    protected void DetectedPackageComplete(object sender, DetectPackageCompleteEventArgs detectPackageCompleteEventArgs)
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

    protected void ApplyComplete(object sender, ApplyCompleteEventArgs e)
    {
      FinalResult = e.Status;
    }

    protected void PlanPackageBegin(object sender, PlanPackageBeginEventArgs planPackageBeginEventArgs)
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
      if (!string.IsNullOrEmpty(e.DownloadSource))
      {
        e.Result = Result.Download;
      }
      else
      {
        e.Result = Result.Ok;
      }
    }

    private void WireUpEventHandlers()
    {
      BootstrapperApplication.WrapperDetectPackageComplete += DetectedPackageComplete;
      BootstrapperApplication.WrapperApplyComplete += ApplyComplete;
      BootstrapperApplication.WrapperPlanPackageBegin += PlanPackageBegin;
      BootstrapperApplication.WrapperResolveSource += ResolveSource;
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
