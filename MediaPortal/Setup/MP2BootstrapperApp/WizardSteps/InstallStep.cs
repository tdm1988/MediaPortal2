using MP2BootstrapperApp.Commands;
using MP2BootstrapperApp.Models;
using MP2BootstrapperApp.ViewModels;

namespace MP2BootstrapperApp.WizardSteps
{
  public class InstallStep : IStep
  {
    public void Enter(Wizard wizard, InstallWizardViewModel viewModel, Package model, IBootstrapperApplicationModel applicationModel)
    {
      viewModel.Content = new InstallNewTypePageViewModel(model);
      viewModel.NextCommand = new RelayCommand(o => { SetNextStep(wizard, viewModel, model, applicationModel); });
      viewModel.BackCommand = new RelayCommand(o => { SetBackStep(wizard, viewModel, model, applicationModel); });
    }

    private static void SetNextStep(Wizard wizard, InstallWizardViewModel viewModel, Package model, IBootstrapperApplicationModel applicationModel)
    {
      switch (model.InstallType)
      {
        case InstallType.Client:
          wizard.InstallClient(viewModel, model, applicationModel);
          break;
        case InstallType.Custom:
          wizard.Finish(viewModel, model, applicationModel);
          break;
      }
    }
    
    private static void SetBackStep(Wizard wizard, InstallWizardViewModel viewModel, Package model, IBootstrapperApplicationModel applicationModel)
    {
      throw new System.NotImplementedException();
    }
  }
}

