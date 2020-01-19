using MP2BootstrapperApp.Models;
using MP2BootstrapperApp.ViewModels;

namespace MP2BootstrapperApp.Nav
{
  public class OverviewPage : IState
  {
    public void Enter(Wizard wizard, InstallWizardViewModel viewModel)
    {
      viewModel.Content = new Page2ViewModel();
      wizard.NextState = new FinishPage();
    }
  }
}
