using MP2BootstrapperApp.Models;
using MP2BootstrapperApp.ViewModels;

namespace MP2BootstrapperApp.WizardSteps
{
  public interface IStep
  {
    void Enter(Wizard wizard, Package model);
  }
}
