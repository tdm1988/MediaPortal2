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

using MediaPortal.Common;
using MediaPortal.Common.Exceptions;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.MediaManagement.MLQueries;
using MediaPortal.Common.SystemCommunication;
using MediaPortal.UI.ServerCommunication;
using MediaPortal.UI.Services.UserManagement;
using MediaPortal.UiComponents.Media.FilterTrees;
using MediaPortal.UiComponents.Media.General;
using MediaPortal.UiComponents.Media.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaPortal.Common.UserManagement;
using MediaPortal.Common.UserProfileDataManagement;
using MediaPortal.UiComponents.Media.Helpers;

namespace MediaPortal.UiComponents.Media.FilterCriteria
{
  /// <summary>
  /// Filter criterion which creates a filter by a simple attribute value.
  /// </summary>
  public class RelationshipMLFilterCriterion : MLFilterCriterion
  {
    protected Guid _role;
    protected Guid _linkedRole;
    protected IEnumerable<Guid> _necessaryMIATypeIds;
    protected IEnumerable<Guid> _optionalMIATypeIds;
    protected ISortInformation _sortInformation;

    // The attribute to use to group the results if required.
    // Can be overriden in derived classes to group by a different attribute.
    protected MediaItemAspectMetadata.AttributeSpecification _groupByAttributeType = MediaAspect.ATTR_TITLE;

    public RelationshipMLFilterCriterion(Guid role, Guid linkedRole, IEnumerable<Guid> necessaryMIATypeIds, IEnumerable<Guid> optionalMIATypeIds, ISortInformation sortInformation)
    {
      _role = role;
      _linkedRole = linkedRole;
      _necessaryMIATypeIds = necessaryMIATypeIds;
      _optionalMIATypeIds = optionalMIATypeIds;
      _sortInformation = sortInformation;
    }

    #region Base overrides

    public override async Task<ICollection<FilterValue>> GetAvailableValuesAsync(IEnumerable<Guid> necessaryMIATypeIds, IFilter selectAttributeFilter, IFilter filter)
    {
      IContentDirectory cd = ServiceRegistration.Get<IServerConnectionManager>().ContentDirectory;
      if (cd == null)
        throw new NotConnectedException("The MediaLibrary is not connected");
      
      UserProfile userProfile = null;
      IUserManagement userProfileDataManagement = ServiceRegistration.Get<IUserManagement>();
      if (userProfileDataManagement != null && userProfileDataManagement.IsValidUser)
      {
        userProfile = userProfileDataManagement.CurrentUser;
      }

      var mias = (_necessaryMIATypeIds ?? necessaryMIATypeIds)?.ToList();
      var optMias = (_optionalMIATypeIds != null ? _optionalMIATypeIds.Except(mias) : null)?.ToList();
      bool showVirtual = VirtualMediaHelper.ShowVirtualMedia(necessaryMIATypeIds);
      IFilter queryFilter = new FilteredRelationshipFilter(_role, _linkedRole, filter);
      if (selectAttributeFilter != null)
        queryFilter = BooleanCombinationFilter.CombineFilters(BooleanOperator.And, selectAttributeFilter, queryFilter);

      MediaItemQuery query = new MediaItemQuery(mias, optMias, UserHelper.GetUserRestrictionFilter(mias, userProfile, queryFilter));
      if (_sortInformation != null)
        query.SortInformation = new List<ISortInformation> { _sortInformation };

      IList<MediaItem> items = await cd.SearchAsync(query, true, userProfile?.ProfileId, showVirtual).ConfigureAwait(false);
      CertificationHelper.ConvertCertifications(items);
      IList<FilterValue> result = new List<FilterValue>(items.Count);
      foreach (MediaItem item in items)
      {
        string name;
        MediaItemAspect.TryGetAttribute(item.Aspects, MediaAspect.ATTR_TITLE, out name);
        result.Add(new FilterValue(name,
          new FilterTreePath(_role),
          null,
          item,
          this));
      }

      //ToDo: Add support for an empty entry for all filtered items that don't have this relationship
      //The below works OK in simple cases but results in an extra, relatively long running query. Maybe this should be handled
      //at the server for all relationship queries...
      //IFilter emptyRelationshipFilter = new NotFilter(new RelationshipFilter(_linkedRole, _role, Guid.Empty));
      //queryFilter = filter != null ? BooleanCombinationFilter.CombineFilters(BooleanOperator.And, filter, emptyRelationshipFilter) : emptyRelationshipFilter;
      //int numEmptyEntries = cd.CountMediaItems(necessaryMIATypeIds, queryFilter, true, showVirtual);
      //if(numEmptyEntries > 0)
      //  result.Insert(0, new FilterValue(Consts.RES_VALUE_EMPTY_TITLE, emptyRelationshipFilter, null, this));

      return result;
    }

    protected virtual string GetDisplayName(object groupKey)
    {
      return string.Format("{0}", groupKey).Trim();
    }

    public override async Task<ICollection<FilterValue>> GroupValuesAsync(ICollection<Guid> necessaryMIATypeIds, IFilter selectAttributeFilter, IFilter filter)
    {
      if (_groupByAttributeType == null)
        return null;

      IContentDirectory cd = ServiceRegistration.Get<IServerConnectionManager>().ContentDirectory;
      if (cd == null)
        throw new NotConnectedException("The MediaLibrary is not connected");

      bool showVirtual = VirtualMediaHelper.ShowVirtualMedia(necessaryMIATypeIds);

      if (_necessaryMIATypeIds != null)
        necessaryMIATypeIds = _necessaryMIATypeIds.ToList();
      IList<MLQueryResultGroup> valueGroups = await cd.GroupValueGroupsAsync(_groupByAttributeType, selectAttributeFilter, ProjectionFunction.None,
          necessaryMIATypeIds, filter, true, GroupingFunction.FirstCharacter, showVirtual).ConfigureAwait(false);
      IList<FilterValue> result = new List<FilterValue>(valueGroups.Count);
      int numEmptyEntries = 0;
      foreach (MLQueryResultGroup group in valueGroups)
      {
        string name = string.Format("{0}", group.GroupKey);
        name = name.Trim();
        if (name == string.Empty)
          numEmptyEntries += group.NumItemsInGroup;
        else
          result.Add(new FilterValue(name, new FilterTreePath(_role), group.AdditionalFilter, group.NumItemsInGroup, this));
      }
      if (numEmptyEntries > 0)
        result.Insert(0, new FilterValue(Consts.RES_VALUE_EMPTY_TITLE, new EmptyFilter(_groupByAttributeType), null, numEmptyEntries, this));
      return result;
    }

    #endregion
  }
}
