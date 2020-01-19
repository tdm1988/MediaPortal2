using MP2BootstrapperApp.ViewModels;

namespace MP2BootstrapperApp.Nav
{
  public class FinishPage : IState
  {
    public void Enter(Wizard wizard, InstallWizardViewModel viewModel)
    {
      viewModel.Content = new Page2ViewModel();
      wizard.NextState = new WelcomePage();
    }
  }
}
