using MP2BootstrapperApp.Models;
using MP2BootstrapperApp.ViewModels;

namespace MP2BootstrapperApp.Nav
{
  public class Wizard
  {
    private InstallWizardViewModel _viewModel;
    
    public Wizard(InstallWizardViewModel viewModel)
    {
      _viewModel = viewModel;
    }

    public IState NextState { get; set; }

    public void Start()
    {
      NextState = new WelcomePage();
      NextState.Enter(this, _viewModel);
    }

    public void ChangeState()
    {
      NextState.Enter(this, _viewModel);
    }
    
  }
}
