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

using System.Xml.Serialization;

namespace MediaPortal.Common.MediaManagement.MLQueries
{
  /// <summary>
  /// Marker interface for sorting information.
  /// </summary>
  public class DataSortInformation : ISortInformation
  {
    protected string _userDataKey;
    protected SortDirection _sortDirection;

    public DataSortInformation(string userDataKey, SortDirection sortDirection)
    {
      _userDataKey = userDataKey;
      _sortDirection = sortDirection;
    }

    [XmlIgnore]
    public string UserDataKey
    {
      get { return _userDataKey; }
      set { _userDataKey = value; }
    }

    public SortDirection Direction
    {
      get { return _sortDirection; }
      set { _sortDirection = value; }
    }

    #region Additional members for the XML serialization

    internal DataSortInformation() { }

    /// <summary>
    /// For internal use of the XML serialization system only.
    /// </summary>
    [XmlElement("UserDataKey", IsNullable = false)]
    public string XML_UserDataKey
    {
      get { return _userDataKey; }
      set { _userDataKey = value; }
    }

    #endregion
  }
}
