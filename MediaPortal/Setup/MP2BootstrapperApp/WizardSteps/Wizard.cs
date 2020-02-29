using MP2BootstrapperApp.Models;
using MP2BootstrapperApp.ViewModels;

namespace MP2BootstrapperApp.WizardSteps
{
  public class Wizard
  {
    public IStep NextStep { get; set; }

    public void Start(InstallWizardViewModel viewModel, Package package, IBootstrapperApplicationModel applicationModel)
    {
      NextStep = new WelcomeStep();
      NextStep.Enter(this, viewModel, package, applicationModel);
    }
    
    public void Finish(InstallWizardViewModel viewModel, Package package, IBootstrapperApplicationModel applicationModel)
    {
      NextStep = new FinishStep();
      NextStep.Enter(this, viewModel, package, applicationModel);
    }
    
    public void Install(InstallWizardViewModel viewModel, Package package, IBootstrapperApplicationModel applicationModel)
    {
      NextStep = new InstallStep();
      NextStep.Enter(this, viewModel, package, applicationModel);
    }

    public void ChangeStep(InstallWizardViewModel viewModel, Package package, IBootstrapperApplicationModel applicationModel)
    {
      NextStep.Enter(this, viewModel, package, applicationModel);
    }

    public void InstallClient(InstallWizardViewModel viewModel, Package package, IBootstrapperApplicationModel applicationModel)
    {
      throw new System.NotImplementedException();
    }
  }
}
