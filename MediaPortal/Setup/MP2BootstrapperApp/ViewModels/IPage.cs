namespace MP2BootstrapperApp.Nav
{
  public interface IPage
  {
    string Title { get; }
    
    bool CanExecute { get; }
  }
}
