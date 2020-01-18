using System;
using System.Windows.Input;

namespace MP2BootstrapperApp.Nav
{
  public class RelayCommand<T> : ICommand
  {
    private readonly Action<T> execute;
    private readonly Predicate<T> canExecute;

    public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
    {
      this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
      this.canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
      return canExecute == null || canExecute((T)parameter);
    }

    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    public void Execute(object parameter)
    {
      execute(parameter == null ? default(T) : (T)Convert.ChangeType(parameter, typeof(T)));
    }
  }
}
