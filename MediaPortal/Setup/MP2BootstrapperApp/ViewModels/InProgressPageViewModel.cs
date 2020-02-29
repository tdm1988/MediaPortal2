
using MP2BootstrapperApp.Commands;
using MP2BootstrapperApp.Models;
using MP2BootstrapperApp.WizardSteps;

namespace MP2BootstrapperApp.ViewModels
{
  public class InProgressPageViewModel : PageViewModelBase
  {
    private static string buttonNextContent = "next";
    private static string header = "finish";
    private readonly Package _model;
    
    public InProgressPageViewModel(Package model) : base(header)
    {
    }
  }
}
