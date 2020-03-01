using System;
using System.IO;

namespace MP2BootstrapperApp.ChainPackages
{
  public class Vc2019X86 : IPackage
  {
    private readonly IPackageChecker _packageChecker;

    public Vc2019X86(IPackageChecker packageChecker)
    {
      _packageChecker = packageChecker;
    }

    public Version GetInstalledVersion()
    {
      const string mfc140Dll = "mfc140.dll";
      string systemWow64Path = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86); 
      string vc2019X86Path = Path.Combine(systemWow64Path, mfc140Dll);

      if (!_packageChecker.Exists(vc2019X86Path))
      {
        return new Version();
      }
      int majorVersion = _packageChecker.GetFileMajorVersion(vc2019X86Path);
      int minorVersion = _packageChecker.GetFileMinorVersion(vc2019X86Path);
      int buildVersion = _packageChecker.GetFileBuildVersion(vc2019X86Path);
      int revision = _packageChecker.GetFilePrivateVersion(vc2019X86Path);
      
      return new Version(majorVersion, minorVersion, buildVersion, revision);
    }
  }
}
