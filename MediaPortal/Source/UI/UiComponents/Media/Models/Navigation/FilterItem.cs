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

using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.UiComponents.Media.Extensions;
using MediaPortal.UiComponents.Media.General;
using MediaPortal.UiComponents.Media.Views;

namespace MediaPortal.UiComponents.Media.Models.Navigation
{
  /// <summary>
  /// Holds a GUI item which represents a filter choice.
  /// </summary>
  public class FilterItem : ContainerItem, IViewListItem
  {
    public FilterItem(string name, int? numItems)
      : base(numItems)
    {
      SimpleTitle = name;
    }

    public FilterItem(string id, string name, int? numItems)
      : base(numItems)
    {
      Id = id;
      SimpleTitle = name;
    }

    public FilterItem()
    {
    }

    public MediaItem MediaItem
    {
      get
      {
        object mi;
        AdditionalProperties.TryGetValue(Consts.KEY_MEDIA_ITEM, out mi);
        return mi as MediaItem;
      }
      set
      {
        AdditionalProperties[Consts.KEY_MEDIA_ITEM] = value;
        Update(value);
      }
    }

    public bool? Virtual
    {
      get { return (bool?)AdditionalProperties[Consts.KEY_VIRTUAL]; }
      set { AdditionalProperties[Consts.KEY_VIRTUAL] = value; }
    }

    public View View { get; set; }

    public virtual void Update(MediaItem mediaItem)
    {
      if (mediaItem != null)
      {
        SingleMediaItemAspect mediaAspect;
        if (MediaItemAspect.TryGetAspect(mediaItem.Aspects, MediaAspect.Metadata, out mediaAspect))
        {
          SimpleTitle = (string)mediaAspect[MediaAspect.ATTR_TITLE];
          SortString = (string)mediaAspect[MediaAspect.ATTR_SORT_TITLE];
          Virtual = (bool?)mediaAspect[MediaAspect.ATTR_ISVIRTUAL];
        }
      }
    }
  }
}
