using MP2BootstrapperApp.Models;
using MP2BootstrapperApp.ViewModels;

namespace MP2BootstrapperApp.WizardSteps
{
  public class WelcomeStep : IStep
  {
    public void Enter(Wizard wizard, Package model)
    {
      //viewModel.Content = null;
      wizard.NextStep = null;
    }
  }
}
