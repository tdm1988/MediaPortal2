#region Copyright (C) 2007-2020 Team MediaPortal

/*
    Copyright (C) 2007-2020 Team MediaPortal
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

using System.Collections.Generic;
using MediaPortal.Common.MediaManagement.MLQueries;
using MediaPortal.Extensions.MediaServer.Profiles;

namespace MediaPortal.Extensions.MediaServer.Objects.MediaLibrary
{
  class MediaLibraryMovieActorItem : MediaLibraryContainer, IDirectoryPerson
  {
    public MediaLibraryMovieActorItem(string id, string title, IFilter filter, EndPointSettings client)
      : base(id, title, NECESSARY_MOVIE_MIA_TYPE_IDS, OPTIONAL_MOVIE_MIA_TYPE_IDS, filter, client)
    {
    }

    public override string Class
    {
      get { return "object.container.person"; }
    }

    public IList<string> Language { get; set; }
  }
}
