using MP2BootstrapperApp.Commands;
using MP2BootstrapperApp.Models;
using MP2BootstrapperApp.ViewModels;

namespace MP2BootstrapperApp.WizardSteps
{
  public class FinishStep : IStep
  {
    public void Enter(Wizard wizard, InstallWizardViewModel viewModel, Package model, IBootstrapperApplicationModel applicationModel)
    {
      viewModel.Content = new FinishPageViewModel(model);
      viewModel.NextCommand = new RelayCommand(o =>
      {
        wizard.NextStep = new ProductExistsStep();
        viewModel.Content = new InstallExistTypePageViewModel(model);
        wizard.ChangeStep(viewModel, model, applicationModel);
      });
      viewModel.BackCommand = new RelayCommand(o =>
      {
        wizard.NextStep = new WelcomeStep();
        viewModel.Content = new WelcomePageViewModel(model);
        wizard.ChangeStep(viewModel, model, applicationModel);
      });
    }
  }
}
