namespace MP2BootstrapperApp.ViewModels
{
  public class CompleteViewModel: ObservableBase, IPage
  {
    private readonly MainViewModel _viewModel;
    
    public CompleteViewModel(MainViewModel viewModel)
    {
      _viewModel = viewModel;
    }
    
    public string Header
    {
      get
      {
        return "Completed";
      }
    }
    
    public string Result
    {
      get
      {
        return _viewModel.State == InstallState.Applied ? "Success" : "Failure";
      }
    }
  }
}
