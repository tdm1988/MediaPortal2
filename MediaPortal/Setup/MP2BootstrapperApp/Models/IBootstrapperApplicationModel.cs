using System.Collections.Generic;
using System.Windows;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using MP2BootstrapperApp.BootstrapperWrapper;

namespace MP2BootstrapperApp.Models
{
  public interface IBootstrapperApplicationModel
  {
    IBootstrapperApp BootstrapperApplication { get; }
    IList<BundlePackage> BundlePackages { get; }
    int FinalResult { get; set; }
    void SetWindowHandle(Window view);
    void PlanAction(LaunchAction action);
    void ApplyAction();
    void LogMessage(LogLevel logLevel, string message);
  }
}
