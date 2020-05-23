#region Copyright (C) 2007-2017 Team MediaPortal

/*
    Copyright (C) 2007-2017 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using System.Xml.Linq;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using MP2BootstrapperApp.BootstrapperWrapper;
using MP2BootstrapperApp.ChainPackages;
using MP2BootstrapperApp.Commands;
using MP2BootstrapperApp.Models;

namespace MP2BootstrapperApp.ViewModels
{
  public class MainViewModel : ObservableBase
  {
    private InstallState _state;
    private readonly IDispatcher _dispatcher;
    private ICommand _cancelCommand;
    private IPage _content;

    public MainViewModel(IBootstrapperApplicationModel model, IDispatcher dispatcher)
    {
      _dispatcher = dispatcher;
      PackageContext packageContext = new PackageContext();
      InstallViewModel = new InstallViewModel(this, model, packageContext, dispatcher);
      UpdateViewModel = new UpdateViewModel(this, model, packageContext);
      ProgressViewModel = new ProgressViewModel(model);
      CompleteViewModel = new CompleteViewModel(this);
      Content = InstallViewModel;
    }

    private InstallViewModel InstallViewModel { get; }
    
    public UpdateViewModel UpdateViewModel { get; }
    
    public ProgressViewModel ProgressViewModel { get; }
    
    public CompleteViewModel CompleteViewModel { get; }
    
    public IPage Content
    {
      get { return _content; }
      set { Set(ref _content, value); }
    }

    public InstallState State
    {
      get { return _state; }
      set { Set(ref _state, value); }
    }

    public ICommand CancelCommand
    {
      get
      {
        return _cancelCommand ?? (_cancelCommand = new RelayCommand(o =>
        {
          if (State == InstallState.Applying)
          {
            State = InstallState.Canceled;
          }
          else
          {
            _dispatcher.InvokeShutdown();
          }
        }, o  => State != InstallState.Canceled));
      }
    }
  }
}
