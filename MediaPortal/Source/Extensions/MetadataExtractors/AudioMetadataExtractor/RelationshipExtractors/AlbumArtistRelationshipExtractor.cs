#region Copyright (C) 2007-2017 Team MediaPortal

/*
    Copyright (C) 2007-2017 Team MediaPortal
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
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.MediaManagement.Helpers;
using MediaPortal.Common.MediaManagement.MLQueries;
using MediaPortal.Extensions.OnlineLibraries;
using MediaPortal.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaPortal.Extensions.MetadataExtractors.AudioMetadataExtractor
{
  class AlbumArtistRelationshipExtractor : IRelationshipRoleExtractor
  {
    private static readonly Guid[] ROLE_ASPECTS = { AudioAlbumAspect.ASPECT_ID };
    private static readonly Guid[] LINKED_ROLE_ASPECTS = { PersonAspect.ASPECT_ID };

    public bool BuildRelationship
    {
      get { return true; }
    }

    public Guid Role
    {
      get { return AudioAlbumAspect.ROLE_ALBUM; }
    }

    public Guid[] RoleAspects
    {
      get { return ROLE_ASPECTS; }
    }

    public Guid LinkedRole
    {
      get { return PersonAspect.ROLE_ALBUMARTIST; }
    }

    public Guid[] LinkedRoleAspects
    {
      get { return LINKED_ROLE_ASPECTS; }
    }

    public Guid[] MatchAspects
    {
      get { return PersonInfo.EQUALITY_ASPECTS; }
    }

    public IFilter GetSearchFilter(IDictionary<Guid, IList<MediaItemAspect>> extractedAspects)
    {
      if (!extractedAspects.ContainsKey(PersonAspect.ASPECT_ID))
        return null;
      return RelationshipExtractorUtils.CreateExternalItemFilter(extractedAspects, ExternalIdentifierAspect.TYPE_PERSON);
    }

    public ICollection<string> GetExternalIdentifiers(IDictionary<Guid, IList<MediaItemAspect>> extractedAspects)
    {
      if (!extractedAspects.ContainsKey(PersonAspect.ASPECT_ID))
        return new List<string>();
      return RelationshipExtractorUtils.CreateExternalItemIdentifiers(extractedAspects, ExternalIdentifierAspect.TYPE_PERSON);
    }

    public Task<bool> TryExtractRelationshipsAsync(IDictionary<Guid, IList<MediaItemAspect>> aspects, IList<IDictionary<Guid, IList<MediaItemAspect>>> extractedLinkedAspects)
    {
      if (!AudioMetadataExtractor.IncludeArtistDetails)
        return Task.FromResult(false);

      AlbumInfo albumInfo = new AlbumInfo();
      if (!albumInfo.FromMetadata(aspects))
        return Task.FromResult(false);

      AudioRelationshipExtractor.UpdatePersons(aspects, albumInfo.Artists, true);

      int count = 0;
      if (!AudioMetadataExtractor.SkipOnlineSearches)
      {
        OnlineMatcherService.Instance.UpdateAlbumPersons(albumInfo, PersonAspect.OCCUPATION_ARTIST, false);
        count = albumInfo.Artists.Where(p => p.HasExternalId).Count();
        if (!albumInfo.IsRefreshed)
          albumInfo.HasChanged = true; //Force save to update external Ids for metadata found by other MDEs
      }
      else
      {
        count = albumInfo.Artists.Where(p => !string.IsNullOrEmpty(p.Name)).Count();
      }

      if (albumInfo.Artists.Count == 0)
        return Task.FromResult(false);

      if (BaseInfo.CountRelationships(aspects, LinkedRole) < count || (BaseInfo.CountRelationships(aspects, LinkedRole) == 0 && albumInfo.Artists.Count > 0))
        albumInfo.HasChanged = true; //Force save if no relationship exists

      if (!albumInfo.HasChanged)
        return Task.FromResult(false);
      
      foreach (PersonInfo person in albumInfo.Artists)
      {
        person.AssignNameId();
        person.HasChanged = albumInfo.HasChanged;
        IDictionary<Guid, IList<MediaItemAspect>> personAspects = new Dictionary<Guid, IList<MediaItemAspect>>();
        person.SetMetadata(personAspects);

        if (personAspects.ContainsKey(ExternalIdentifierAspect.ASPECT_ID))
          extractedLinkedAspects.Add(personAspects);
      }
      return Task.FromResult(extractedLinkedAspects.Count > 0);
    }

    public bool TryMatch(IDictionary<Guid, IList<MediaItemAspect>> extractedAspects, IDictionary<Guid, IList<MediaItemAspect>> existingAspects)
    {
      if (!existingAspects.ContainsKey(PersonAspect.ASPECT_ID))
        return false;

      PersonInfo linkedPerson = new PersonInfo();
      if (!linkedPerson.FromMetadata(extractedAspects))
        return false;

      PersonInfo existingPerson = new PersonInfo();
      if (!existingPerson.FromMetadata(existingAspects))
        return false;

      return linkedPerson.Equals(existingPerson);
    }

    public bool TryGetRelationshipIndex(IDictionary<Guid, IList<MediaItemAspect>> aspects, IDictionary<Guid, IList<MediaItemAspect>> linkedAspects, out int index)
    {
      index = -1;

      SingleMediaItemAspect linkedAspect;
      if (!MediaItemAspect.TryGetAspect(linkedAspects, PersonAspect.Metadata, out linkedAspect))
        return false;

      string name = linkedAspect.GetAttributeValue<string>(PersonAspect.ATTR_PERSON_NAME);

      SingleMediaItemAspect aspect;
      if (!MediaItemAspect.TryGetAspect(aspects, AudioAlbumAspect.Metadata, out aspect))
        return false;

      IEnumerable<string> persons = aspect.GetCollectionAttribute<string>(AudioAlbumAspect.ATTR_ARTISTS);
      List<string> nameList = new SafeList<string>(persons);

      index = nameList.IndexOf(name);
      return index >= 0;
    }

    internal static ILogger Logger
    {
      get { return ServiceRegistration.Get<ILogger>(); }
    }
  }
}
