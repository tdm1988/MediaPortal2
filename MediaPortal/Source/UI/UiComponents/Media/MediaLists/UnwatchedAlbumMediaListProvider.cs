#region Copyright (C) 2007-2021 Team MediaPortal

/*
    Copyright (C) 2007-2021 Team MediaPortal
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

using MediaPortal.Common.Commands;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.UiComponents.Media.General;
using MediaPortal.UiComponents.Media.Models.Navigation;
using MediaPortal.UiComponents.Media.Models.NavigationModel;

namespace MediaPortal.UiComponents.Media.MediaLists
{
  public class UnwatchedAlbumMediaListProvider : BaseUnwatchedRelationshipMediaListProvider
  {
    public UnwatchedAlbumMediaListProvider()
    {
      _changeAspectId = AudioAspect.ASPECT_ID;
      _role = AudioAlbumAspect.ROLE_ALBUM;
      _necessaryMias = Consts.NECESSARY_ALBUM_MIAS;
      _linkedRole = AudioAspect.ROLE_TRACK;
      _necessaryLinkedMias = Consts.NECESSARY_AUDIO_MIAS;
      _playableContainerConverterAction = item => new AlbumFilterItem(item)
      {
        Command = new MethodDelegateCommand(() => AudioNavigationInitializer.NavigateToAlbum(item.MediaItemId))
      };
      _navigationInitializerType = typeof(AudioNavigationInitializer);
    }
  }
}
