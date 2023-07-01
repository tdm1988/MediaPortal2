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

using MediaPortal.Plugins.SlimTv.Client.Models.Navigation;
using MediaPortal.Plugins.SlimTv.Client.Models.ScreenData.FilterCriteria;
using MediaPortal.Plugins.SlimTv.Client.TvHandler;
using MediaPortal.UiComponents.Media.Models.ScreenData;

namespace MediaPortal.Plugins.SlimTv.Client.Models.ScreenData
{
  /// <summary>
  /// Screen that allows filtering recordings by TV or radio.
  /// </summary>
  public class RecordingsFilterByMediaTypeScreenData : AbstractFiltersScreenData<MediaTypeFilterItem>
  {
    public RecordingsFilterByMediaTypeScreenData()
      : base(SlimTvConsts.SCREEN_RECORDINGS_FILTER_BY_MEDIA_TYPE, SlimTvConsts.RES_FILTER_BY_MEDIA_TYPE_MENU_ITEM, SlimTvConsts.RES_FILTER_MEDIA_TYPE_NAVBAR_DISPLAY_LABEL, new FilterByMediaTypeCriterion())
    { }

    public override AbstractFiltersScreenData<MediaTypeFilterItem> Derive()
    {
      return new RecordingsFilterByMediaTypeScreenData();
    }
  }
}
