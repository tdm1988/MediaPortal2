using System.Windows.Input;

namespace MP2BootstrapperApp.ViewModels
{
  public abstract class PageViewModelBase : IPage
  {
    public PageViewModelBase(string header, string buttonNextContent)
    {
      Header = header;
      ButtonNextContent = buttonNextContent;
    }
    
    public string Header { get; }
    
    public string ButtonNextContent { get; }
    
    public ICommand NextCommand { get; set; }
    
    public ICommand BackCommand { get; set; }
  }
}
