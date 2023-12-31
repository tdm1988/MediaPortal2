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

using MediaPortal.UI.SkinEngine.MarkupExtensions;
using MediaPortal.UI.SkinEngine.Xaml;
using System;
using System.Globalization;
using MediaPortal.Common;
using MediaPortal.Common.Localization;

namespace MediaPortal.Plugins.SlimTv.Client.Controls
{
  public class SlimTvGuideTimeFormatConverter : IMultiValueConverter
  {
    public bool Convert(IDataDescriptor[] values, Type targetType, object parameter, out object result)
    {
      result = null;
      DateTime dtVal = (DateTime)values[0].Value;
      double durationPerc;
      if (double.TryParse(parameter as string, NumberStyles.Any, CultureInfo.InvariantCulture, out durationPerc) &&
        values.Length > 1 && values[1].Value is double)
        dtVal = dtVal.AddHours((double)values[1].Value * durationPerc);

      result = dtVal.ToString("t", ServiceRegistration.Get<ILocalization>().CurrentCulture);
      return true;
    }
  }
}
