using MP2BootstrapperApp.ViewModels;

namespace MP2BootstrapperApp.Nav
{
  public class OverviewPage : IState
  {
    private OverviewPage instance = new OverviewPage();

    private OverviewPage() {}

    public static OverviewPage GetInstance => GetInstance;

    public void GoToOverview(Wizard wizard, InstallWizardViewModel viewModel)
    {
      throw new System.NotImplementedException();
    }

    public void GoToWelcome(Wizard wizard, InstallWizardViewModel viewModel)
    {
      throw new System.NotImplementedException();
    }
  }
}
