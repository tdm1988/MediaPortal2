using System;
using System.Collections.Generic;

namespace MP2BootstrapperApp.Nav
{
  public class MainViewModel : ViewModelBase
  {
    private IPage content;
    
    public IPage Content
    {
      get { return content; }
      private set { Set(ref content, value); }
    }

    public RelayCommand<int> NavigateCommand
    {
      get { return new RelayCommand<int>(Navigate); }
    }

    private readonly Dictionary<int, Lazy<IPage>> pages = new Dictionary<int, Lazy<IPage>>
    {
      [1] = new Lazy<IPage>(() => new OverviewViewModel()),
      [2] = new Lazy<IPage>(() => new Page2ViewModel())
    };

    public MainViewModel()
    {
      Navigate(1);
    }

    private void Navigate(int value)
    {
      Content = pages[value].Value;
    }
  }
}
