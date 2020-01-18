using MP2BootstrapperApp.ViewModels;

namespace MP2BootstrapperApp.Nav
{
  public class Wizard
  {
    private IState _state;
    private InstallWizardViewModel _viewModel;

    public IState State
    {
      set => _state = value;
    }

    public Wizard(InstallWizardViewModel viewModel)
    {
      _viewModel = viewModel;
      _state = WelcomePage.GetInstance;
    }

    public void GoToOverview()
    {
      _state.GoToOverview(this, _viewModel);
    }

    public void GoToWelcome()
    {
      _state.GoToWelcome(this, _viewModel);
    }
  }
}
