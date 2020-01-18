using MP2BootstrapperApp.ViewModels;

namespace MP2BootstrapperApp.Nav
{
  public interface IState
  {
    void GoToOverview(Wizard wizard, InstallWizardViewModel viewModel);
    void GoToWelcome(Wizard wizard, InstallWizardViewModel viewModel);
  }
}
