using MP2BootstrapperApp.Models;
using MP2BootstrapperApp.ViewModels;

namespace MP2BootstrapperApp.Nav
{
  public interface IState
  {
    void Enter(Wizard wizard, InstallWizardViewModel viewModel);
  }
}
