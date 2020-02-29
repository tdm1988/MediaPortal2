using System.Windows.Input;

namespace MP2BootstrapperApp.ViewModels
{
  public abstract class PageViewModelBase : ObservableBase, IPage
  {
    public PageViewModelBase(string header)
    {
      Header = header;
    }
    
    public string Header { get; }
  }
}
