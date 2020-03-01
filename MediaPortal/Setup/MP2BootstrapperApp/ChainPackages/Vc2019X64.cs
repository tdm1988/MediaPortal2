using System;
using System.IO;

namespace MP2BootstrapperApp.ChainPackages
{
  public class Vc2019X64 : IPackage
  {
    private readonly IPackageChecker _packageChecker;

    public Vc2019X64(IPackageChecker packageChecker)
    {
      _packageChecker = packageChecker;
    }

    public Version GetInstalledVersion()
    {
      const string mfc140Dll = "mfc140.dll";
      string vc2019X64Path = Path.Combine(Environment.SystemDirectory, mfc140Dll);

      if (!_packageChecker.Exists(vc2019X64Path))
      {
        return new Version();
      }
      int majorVersion = _packageChecker.GetFileMajorVersion(vc2019X64Path);
      int minorVersion = _packageChecker.GetFileMinorVersion(vc2019X64Path);
      int buildVersion = _packageChecker.GetFileBuildVersion(vc2019X64Path);
      int revision = _packageChecker.GetFilePrivateVersion(vc2019X64Path);
      
      return new Version(majorVersion, minorVersion, buildVersion, revision);
    }
  }
}
