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
  public class UpdateViewModel : ObservableBase, IPage
  {
    private readonly IBootstrapperApplicationModel _model;
    private readonly MainViewModel _viewModel;
    
    private ICommand _updateCommand;
    private ICommand _repairCommand;
    private ICommand _uninstallCommand;
    

    public UpdateViewModel(MainViewModel viewModel, IBootstrapperApplicationModel model)
    {
      _viewModel = viewModel;
      _model = model;
    }

    public string Header
    {
      get { return "Update"; }
    }
    
    public ICommand UpdateCommand
    {
      get { return _updateCommand ?? (_updateCommand = new RelayCommand(o => Update())); }
    }
    
    public ICommand RepairCommand
    {
      get { return _repairCommand ?? (_repairCommand = new RelayCommand(o => Repair())); }
    }

    public ICommand UninstallCommand
    {
      get { return _uninstallCommand ?? (_uninstallCommand = new RelayCommand(o => Uninstall())); }
    }
    
    private void Update()
    {
    }
    
    private void Repair()
    {
    }

    private void Uninstall()
    {
    }
  }
}
