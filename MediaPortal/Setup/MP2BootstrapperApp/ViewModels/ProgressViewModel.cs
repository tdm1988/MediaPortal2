using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using MP2BootstrapperApp.Models;

namespace MP2BootstrapperApp.ViewModels
{
  public class ProgressViewModel : ObservableBase, IPage
  {
    private readonly IBootstrapperApplicationModel _model;
    private int _progress;
    private int _cacheProgress;
    private int _executeProgress;
    
    public ProgressViewModel(IBootstrapperApplicationModel model)
    {
      _model = model;
      WireUpEventHandlers();
    }
    
    public int Progress
    {
      get { return _progress; }
      set { Set(ref _progress, value); }
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
      _model.BootstrapperApplication.WrapperCacheAcquireProgress += CacheAcquireProgress;
      _model.BootstrapperApplication.WrapperExecuteProgress += ExecuteProgress;
    }

    public string Header
    {
      get
      {
        return "Installing...";
      }
    }
  }
}
