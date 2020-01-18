namespace MP2BootstrapperApp.Nav
{
  public abstract class PageViewModelBase : ViewModelBase, IPage
  {
    public PageViewModelBase(string title)
    {
      Title = title;
    }

    public string Title { get; }
    
    public bool CanExecute { get; }
  }
}
