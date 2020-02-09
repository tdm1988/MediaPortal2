using MP2BootstrapperApp.Models;
using MP2BootstrapperApp.ViewModels;

namespace MP2BootstrapperApp.WizardSteps
{
  public class FinishStep : IStep
  {
    public void Enter(Wizard wizard, Package model)
    {
      //viewModel.Content = new FinishPageViewModel(wizard, model);
      wizard.NextStep = new WelcomeStep();
    }
  }
}
