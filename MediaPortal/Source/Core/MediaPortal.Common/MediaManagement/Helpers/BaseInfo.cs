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
using System.Collections.Generic;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using System.IO;
using MediaPortal.Utilities.Graphics;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using DuoVia.FuzzyStrings;
using MediaPortal.Utilities;
using System.Linq;

namespace MediaPortal.Common.MediaManagement.Helpers
{
  /// <summary>
  /// <see cref="BaseInfo"/> contains metadata information about a thumbnail item.
  /// </summary>
  public abstract class BaseInfo
  {
    public const int MAX_LEVENSHTEIN_DIST = 4;
    public const int LEVENSHTEIN_MATCH_THRESHOLD = 10;

    /// <summary>
    /// Maximum cover image width. Larger images will be scaled down to fit this dimension.
    /// </summary>
    public const int MAX_COVER_WIDTH = 512;

    /// <summary>
    /// Maximum cover image height. Larger images will be scaled down to fit this dimension.
    /// </summary>
    public const int MAX_COVER_HEIGHT = 512;

    /// <summary>
    /// Binary data for the thumbnail image.
    /// </summary>
    public byte[] Thumbnail { get; set; } = null;
    private bool _hasThumbnail = false;

    public IDictionary<Guid, IList<MediaItemAspect>> LinkedAspects { get; private set; }

    public bool HasThumbnail
    {
      get { return _hasThumbnail || Thumbnail != null; }
      set { _hasThumbnail = value; }
    }

    public bool AllowOnlineReSearch { get; set; } = false;

    public bool ForceOnlineSearch { get; set; } = false;

    public bool IsDirty { get; private set; } = false;

    public bool HasChanged { get; set; } = false;

    public DateTime? LastChanged { get; set; } = null;

    public DateTime? DateAdded { get; set; } = null;

    public IList<string> DataProviders { get; set; } = new List<string>();

    public bool IsRefreshed
    {
      get
      {
        if (LastChanged.HasValue && DateAdded.HasValue)
          return LastChanged.Value > DateAdded.Value;
        return false;
      }
    }

    public abstract bool IsBaseInfoPresent { get; }

    public abstract bool HasExternalId { get; }

    public virtual bool MatchNames(string name1, string name2)
    {
      return CompareNames(name1, name2);
    }

    public virtual bool OnlineMatchNames(string name1, string name2)
    {
      return CompareNames(name1, name2);
    }

    protected bool CompareNames(string name1, string name2, double threshold = 0.62, int distance = 0)
    {
      if (string.IsNullOrEmpty(name1) || string.IsNullOrEmpty(name2))
        return false;
      if(Math.Max(name1.Length, name2.Length) < LEVENSHTEIN_MATCH_THRESHOLD || distance > 0)
      {
        //Dice Coefficients are not good against short strings
        distance = distance <= 0 ? MAX_LEVENSHTEIN_DIST : distance;
        double dist = StringUtils.GetLevenshteinDistance(StringUtils.RemoveDiacritics(name1).ToLowerInvariant(), StringUtils.RemoveDiacritics(name2).ToLowerInvariant());
        return dist < distance;
      }
      double dice = StringUtils.RemoveDiacritics(name1).ToLowerInvariant().DiceCoefficient(StringUtils.RemoveDiacritics(name2).ToLowerInvariant());
      return dice > threshold;
    }

    /// <summary>
    /// Used to replace all "." and "_" that are not followed by a word character.
    /// <example>Replaces <c>"Once.Upon.A.Time.S01E13"</c> to <c>"Once Upon A Time S01E13"</c>, but keeps the <c>"."</c> inside
    /// <c>"Dr. House"</c>.</example>
    /// </summary>
    private const string CLEAN_WHITESPACE_REGEX = @"[\.|_](\S|$)";
    private const string SORT_REGEX = @"(^The\s+)|(^An?\s+)|(^De[rsmn]\s+)|(^Die\s+)|(^Das\s+)|(^Ein(e[srmn]?)?\s+)";
    private const string CLEAN_REGEX = @"<[^>]+>|&nbsp;";
    private const string CLEAN_NAME_REGEX = @"[���\s'.-]";

    #region Members

    /// <summary>
    /// Copies the contained series information into MediaItemAspect.
    /// </summary>
    /// <param name="aspectData">Dictionary with extracted aspects.</param>
    public virtual bool SetThumbnailMetadata(IDictionary<Guid, IList<MediaItemAspect>> aspectData)
    {
      if (Thumbnail == null || Thumbnail.Length == 0)
        return false;

      try
      {
        using (MemoryStream stream = new MemoryStream(Thumbnail))
        using (MemoryStream resized = (MemoryStream)ImageUtilities.ResizeImage(stream, ImageFormat.Jpeg, MAX_COVER_WIDTH, MAX_COVER_HEIGHT))
        {
          if (resized == null)
            return false;
          MediaItemAspect.SetAttribute(aspectData, ThumbnailLargeAspect.ATTR_THUMBNAIL, resized.ToArray());
        }
        HasThumbnail = true;
        Thumbnail = null;
        return true;
      }
      // Decoding of invalid image data can fail, but main MediaItem is correct.
      catch { }
      Thumbnail = null;
      return false;
    }

    public static string GetSortTitle(string title)
    {
      if (string.IsNullOrEmpty(title)) return null;
      return Regex.Replace(title, SORT_REGEX, "").Trim();
    }

    public static string CleanString(string value)
    {
      if (string.IsNullOrEmpty(value)) return null;
      return Regex.Replace(Regex.Replace(value, CLEAN_REGEX, "").Trim(), @"\s{2,}", " ");
    }

    /// <summary>
    /// Cleans up strings by replacing unwanted characters (<c>'.'</c>, <c>'_'</c>) by spaces.
    /// </summary>
    public static string CleanupWhiteSpaces(string str)
    {
      return str == null ? null : Regex.Replace(str, CLEAN_WHITESPACE_REGEX, " $1").Trim(' ', '-').Replace("  ", " ");
    }

    public static bool IsVirtualResource(IDictionary<Guid, IList<MediaItemAspect>> aspectData)
    {
      IList<MultipleMediaItemAspect> providerResourceAspects;
      if (MediaItemAspect.TryGetAspects(aspectData, ProviderResourceAspect.Metadata, out providerResourceAspects))
      {
        foreach(var pra in providerResourceAspects)
        {
          if (pra.GetAttributeValue<int>(ProviderResourceAspect.ATTR_TYPE) == ProviderResourceAspect.TYPE_VIRTUAL)
            return true;
        }
        return false;
      }
      return true;
    }

    public static bool HasRelationship(IDictionary<Guid, IList<MediaItemAspect>> aspectData, Guid linkedRole)
    {
      bool relationshipFound = false;
      IList<MultipleMediaItemAspect> relationships;
      if (MediaItemAspect.TryGetAspects(aspectData, RelationshipAspect.Metadata, out relationships))
      {
        foreach (MultipleMediaItemAspect relationship in relationships)
        {
          if (relationship.GetAttributeValue<Guid>(RelationshipAspect.ATTR_LINKED_ROLE) == linkedRole ||
            relationship.GetAttributeValue<Guid>(RelationshipAspect.ATTR_ROLE) == linkedRole)
          {
            relationshipFound = true;
            break;
          }
        }
      }
      return relationshipFound;
    }

    public static bool TryGetLinkedIds(IDictionary<Guid, IList<MediaItemAspect>> aspectData, Guid linkedRole, out IList<Guid> linkedIds)
    {
      IList<MultipleMediaItemAspect> relationships;
      if (MediaItemAspect.TryGetAspects(aspectData, RelationshipAspect.Metadata, out relationships))
      {
        linkedIds = new List<Guid>();
        foreach (MultipleMediaItemAspect relationship in relationships)
          if (relationship.GetAttributeValue<Guid>(RelationshipAspect.ATTR_LINKED_ROLE) == linkedRole)
            linkedIds.Add(relationship.GetAttributeValue<Guid>(RelationshipAspect.ATTR_LINKED_ID));
        return linkedIds.Count > 0;
      }
      linkedIds = null;
      return false;
    }

    public static int CountRelationships(IDictionary<Guid, IList<MediaItemAspect>> aspectData, Guid linkedRole)
    {
      int count = 0;
      IList<MultipleMediaItemAspect> relationships;
      if (MediaItemAspect.TryGetAspects(aspectData, RelationshipAspect.Metadata, out relationships))
      {
        foreach (MultipleMediaItemAspect relationship in relationships)
        {
          if (relationship.GetAttributeValue<Guid>(RelationshipAspect.ATTR_LINKED_ROLE) == linkedRole ||
            relationship.GetAttributeValue<Guid>(RelationshipAspect.ATTR_ROLE) == linkedRole)
          {
            count++;
          }
        }
      }
      return count;
    }

    protected void GetMetadataChanged(IDictionary<Guid, IList<MediaItemAspect>> aspectData)
    {
      SingleMediaItemAspect importerAspect;
      if (MediaItemAspect.TryGetAspect(aspectData, ImporterAspect.Metadata, out importerAspect))
      {
        IsDirty = importerAspect.GetAttributeValue<bool>(ImporterAspect.ATTR_DIRTY);
        HasChanged = importerAspect.GetAttributeValue<bool>(ImporterAspect.ATTR_DIRTY);
        LastChanged = importerAspect.GetAttributeValue<DateTime?>(ImporterAspect.ATTR_LAST_IMPORT_DATE);
        DateAdded = importerAspect.GetAttributeValue<DateTime?>(ImporterAspect.ATTR_DATEADDED);
      }
    }

    public abstract bool SetMetadata(IDictionary<Guid, IList<MediaItemAspect>> aspectData, bool force = false);

    public abstract bool FromMetadata(IDictionary<Guid, IList<MediaItemAspect>> aspectData);

    public abstract bool MergeWith(object other, bool overwriteShorterStrings = true, bool updatePrimaryChildList = false);

    public virtual bool SetLinkedMetadata()
    {
      if (LinkedAspects == null)
        return false;
      return SetMetadata(LinkedAspects);
    }

    public virtual bool FromLinkedMetadata(IDictionary<Guid, IList<MediaItemAspect>> aspectData)
    {
      if (aspectData == null)
        return false;
      LinkedAspects = aspectData;
      return FromMetadata(aspectData);
    }

    public abstract void AssignNameId();

    public virtual bool FromString(string name)
    {
      return false;
    }

    public virtual bool CopyIdsFrom<T>(T otherInstance)
    {
      return false;
    }

    public virtual T CloneBasicInstance<T>()
    {
      return default(T);
    }

    public static string GetNameId(string name)
    {
      if (!string.IsNullOrEmpty(name))
      {
        string nameId = CleanString(CleanupWhiteSpaces(name));
        if (!string.IsNullOrEmpty(nameId))
          nameId = StringUtils.RemoveDiacritics(nameId);
        if (!string.IsNullOrEmpty(nameId))
          nameId = Regex.Replace(nameId, CLEAN_NAME_REGEX, "").ToLowerInvariant();
        if (!string.IsNullOrEmpty(nameId))
          return nameId;
      }
      return null;
    }

    protected void MergeDataProviders(BaseInfo other)
    {
      DataProviders = DataProviders.Except(other.DataProviders).Concat(other.DataProviders).ToList();
    }

    #endregion
  }
}
