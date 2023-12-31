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

using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.MediaManagement.MLQueries;
using MediaPortal.UI.ContentLists;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaPortal.UiComponents.Media.MediaLists
{
  public abstract class BaseLatestMediaListProvider : BaseMediaListProvider
  {
    protected override async Task<MediaItemQuery> CreateQueryAsync()
    {
      IFilter filter = await AppendUserFilterAsync(GetNavigationFilter(_navigationInitializerType), _necessaryMias);
      return new MediaItemQuery(_necessaryMias, _optionalMias, filter)
      {
        SortInformation = new List<ISortInformation> { new AttributeSortInformation(ImporterAspect.ATTR_DATEADDED, SortDirection.Descending) }
      };
    }

    protected override bool ShouldUpdate(UpdateReason updateReason, ICollection<object> updatedObjects)
    {
      return updateReason.HasFlag(UpdateReason.ImportComplete) || updateReason.HasFlag(UpdateReason.UserChanged) || base.ShouldUpdate(updateReason, updatedObjects);
    }
  }

  public abstract class BaseLatestRelationshipMediaListProvider : BaseLatestMediaListProvider
  {
    protected Guid _role;
    protected Guid _linkedRole;
    protected IEnumerable<Guid> _necessaryLinkedMias;

    protected override async Task<MediaItemQuery> CreateQueryAsync()
    {
      Guid? userProfile = CurrentUserProfile?.ProfileId;
      IFilter filter = userProfile.HasValue ? new FilteredRelationshipFilter(_role, _linkedRole, await AppendUserFilterAsync(null, _necessaryLinkedMias)) : null;
      return new MediaItemQuery(_necessaryMias, _optionalMias, filter)
      {
        SubqueryFilter = GetNavigationFilter(_navigationInitializerType),
        SortInformation = new List<ISortInformation> { new ChildAggregateAttributeSortInformation(_role, _linkedRole, ImporterAspect.ATTR_DATEADDED, AggregateFunction.Max, SortDirection.Descending) }
      };
    }
  }
}
