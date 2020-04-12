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

using System.Windows.Input;
using MP2BootstrapperApp.Commands;
using MP2BootstrapperApp.Models;

namespace MP2BootstrapperApp.ViewModels
{
  public class InstallViewModel : ObservableBase, IPage
  {
    private ICommand _installClientAndServerCommand;
    private ICommand _installClient;
    private ICommand _installServer;
    private readonly Package _model;

    public InstallViewModel(MainViewModel viewModel, IBootstrapperApplicationModel model)
    {
    }
    
    public string Header
    {
      get { return "Install"; }
    }

    public ICommand InstallClientAndServerCommand
    {
      get { return _installClientAndServerCommand ?? (_installClientAndServerCommand = new RelayCommand(o => InstallClientAndServer())); }
    }
    
    public ICommand InstallClientCommand
    {
      get { return _installClient ?? (_installClient = new RelayCommand(o => InstallClient())); }
    }

    public ICommand InstallServerCommand
    {
      get { return _installServer ?? (_installServer = new RelayCommand(o => InstallServer())); }
    }

    private void InstallClient()
    {
    }
    
    private void InstallServer()
    {
    }

    private void InstallClientAndServer()
    {
    }
  }
}
