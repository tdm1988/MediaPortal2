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

using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UI.SkinEngine.MpfElements.Converters;
using MediaPortal.UiComponents.Nereus.Models.HomeContent;
using MediaPortal.UiComponents.SkinBase.General;
using System;
using System.Globalization;

namespace MediaPortal.UiComponents.Nereus.Converters
{
  public class ListToGridConverter : AbstractSingleDirectionConverter
  {
    public override bool Convert(object val, Type targetType, object parameter, CultureInfo culture, out object result)
    {
      // We can only work on ItemsLists
      ItemsList list = val as ItemsList;
      if (list == null)
      {
        result = null;
        return false;
      }

      string name = parameter as string;
      ListItem gridItem = new ListItem(Consts.KEY_NAME, name ?? string.Empty);
      gridItem.AdditionalProperties["SubItems"] = list;
      result = gridItem;
      return true;
    }
  }
}
