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

using System;
using MediaPortal.Common.UserProfileDataManagement;

namespace MediaPortal.UiComponents.Media.Extensions
{
  /// <summary>
  /// Plugin item builder for <c>MediaItemActionBuilder</c> plugin items.
  /// </summary>
  public class MediaItemActionBuilder : BaseActionBuilder<MediaItemActionExtension>
  {
    public const string MEDIA_EXTENSION_PATH = "/Media/Extensions";

    public MediaItemActionBuilder()
      : base((Type type, string caption, string sort, string restrictionGroup, string id) => new MediaItemActionExtension(type, caption, sort, restrictionGroup, id))
    {
    }
  }
  
  /// <summary>
  /// <see cref="MediaItemActionExtension"/> holds extension metadata.
  /// </summary>
  public class MediaItemActionExtension : IUserRestriction
  {
    /// <summary>
    /// Gets the registered type.
    /// </summary>
    public Type ExtensionClass { get; private set; }

    /// <summary>
    /// Gets the caption of the entry, should be defined as localized string (<example>[Media.MyProgramExtension]</example>).
    /// </summary>
    public string Caption { get; private set; }

    /// <summary>
    /// Gets a sorting text. The order of actions will be defined by this string.
    /// </summary>
    public string Sort { get; private set; }

    /// <summary>
    /// Gets an optional user restriction group to allow limiting user access.
    /// </summary>
    public string RestrictionGroup { get; set; }

    /// <summary>
    /// Unique ID of extension.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the instance of the action.
    /// </summary>
    public IMediaItemAction Action { get; set; }

    public MediaItemActionExtension(Type type, string caption, string sort, string restrictionGroup, string id)
    {
      ExtensionClass = type;
      Caption = caption;
      Sort = sort;
      RestrictionGroup = restrictionGroup;
      Id = new Guid(id);
    }
  }
}
