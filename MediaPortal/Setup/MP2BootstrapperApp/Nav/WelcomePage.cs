using MP2BootstrapperApp.ViewModels;

namespace MP2BootstrapperApp.Nav
{
  public class WelcomePage : IState
  {
    private static WelcomePage instance = new WelcomePage();
    
    private WelcomePage() {}

    public static WelcomePage GetInstance => instance;

    public void GoToOverview(Wizard wizard, InstallWizardViewModel viewModel)
    {
      wizard.State = OverviewPage.GetInstance;
      viewModel.Content = new OverviewViewModel();
      viewModel.NextCommand = new RelayCommand<int>(i => {});
    }

    public void GoToWelcome(Wizard wizard, InstallWizardViewModel viewModel)
    {
      wizard.State = instance;
    }
  }
}
