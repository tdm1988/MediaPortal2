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

using MediaPortal.UiComponents.Media.General;
using MediaPortal.UiComponents.Media.Models.Navigation;
using MediaPortal.UiComponents.Media.Models.NavigationModel;

namespace MediaPortal.UiComponents.Media.MediaLists
{
  public class LatestImageMediaListProvider : BaseLatestMediaListProvider
  {
    public LatestImageMediaListProvider()
    {
      _necessaryMias = Consts.NECESSARY_IMAGE_MIAS;
      _playableConverterAction = item => new ImageItem(item);
      _navigationInitializerType = typeof(ImagesNavigationInitializer);
    }
  }
}
