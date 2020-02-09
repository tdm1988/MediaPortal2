using MP2BootstrapperApp.Models;
using MP2BootstrapperApp.ViewModels;

namespace MP2BootstrapperApp.WizardSteps
{
  public class Wizard
  {
    private readonly InstallWizardViewModel _viewModel;
    private readonly Package _model;
    
    public Wizard(InstallWizardViewModel viewModel, Package model)
    {
      _viewModel = viewModel;
      _model = model;
    }

    public IStep NextStep { get; set; }

    public void Start()
    {
      NextStep = new WelcomeStep();
      NextStep.Enter(this, _viewModel, _model);
    }

    public void ChangeStep()
    {
      NextStep.Enter(this, _viewModel, _model);
    }
  }
}
