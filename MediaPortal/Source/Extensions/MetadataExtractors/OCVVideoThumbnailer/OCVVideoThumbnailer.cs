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
using System.Threading;
using System.Threading.Tasks;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.ResourceAccess;
using OpenCvSharp;

namespace MediaPortal.Extensions.MetadataExtractors.OCVVideoThumbnailer
{
  /// <summary>
  /// MediaPortal 2 metadata extractor to exctract thumbnails from videos.
  /// </summary>
  public class OCVVideoThumbnailer : IMetadataExtractor
  {
    #region Constants

    /// <summary>
    /// GUID string for the OCVVideoThumbnailer metadata extractor.
    /// </summary>
    public const string METADATAEXTRACTOR_ID_STR = "31F58266-C67A-499A-9286-94FB6FB276EB";

    /// <summary>
    /// Video metadata extractor GUID.
    /// </summary>
    public static Guid METADATAEXTRACTOR_ID = new Guid(METADATAEXTRACTOR_ID_STR);

    protected const double DEFAULT_THUMBNAIL_OFFSET = 1.0 / 3.0;
    protected const int MAX_CONCURRENT_OPENCV = 10;

    #endregion

    #region Protected fields and classes

    protected static ICollection<MediaCategory> MEDIA_CATEGORIES = new List<MediaCategory>();
    protected MetadataExtractorMetadata _metadata;
    protected static readonly SemaphoreSlim OPENCV_THROTTLE_LOCK = new SemaphoreSlim(MAX_CONCURRENT_OPENCV, MAX_CONCURRENT_OPENCV);

    #endregion

    #region Ctor

    static OCVVideoThumbnailer()
    {
      MEDIA_CATEGORIES.Add(DefaultMediaCategories.Video);
    }

    public OCVVideoThumbnailer()
    {
      // Creating thumbs with this MetadataExtractor takes much longer than downloading them from the internet.
      // This MetadataExtractor only creates thumbs if the ThumbnailLargeAspect has not been filled before.
      // ToDo: Correct this once we have a better priority system
      _metadata = new MetadataExtractorMetadata(METADATAEXTRACTOR_ID, "OpenCV video thumbnail extractor", MetadataExtractorPriority.FallBack, true,
          MEDIA_CATEGORIES, new[]
              {
                ThumbnailLargeAspect.Metadata
              });
    }

    #endregion

    #region IMetadataExtractor implementation

    public MetadataExtractorMetadata Metadata
    {
      get { return _metadata; }
    }

    public async Task<bool> TryExtractMetadataAsync(IResourceAccessor mediaItemAccessor, IDictionary<Guid, IList<MediaItemAspect>> extractedAspectData, bool forceQuickMode)
    {
      try
      {
        if (forceQuickMode)
          return false;

        byte[] thumb;
        // We only want to create missing thumbnails here, so check for existing ones first
        if (MediaItemAspect.TryGetAttribute(extractedAspectData, ThumbnailLargeAspect.ATTR_THUMBNAIL, out thumb) && thumb != null)
          return false;

        if (!(mediaItemAccessor is IFileSystemResourceAccessor))
          return false;
        using (LocalFsResourceAccessorHelper rah = new LocalFsResourceAccessorHelper(mediaItemAccessor))
          return await ExtractThumbnailAsync(rah.LocalFsResourceAccessor, extractedAspectData).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        // Only log at the info level here - And simply return false. This lets the caller know that we
        // couldn't perform our task here.
        ServiceRegistration.Get<ILogger>().Error("OCVVideoThumbnailer: Exception reading resource '{0}' (Text: '{1}')", e, mediaItemAccessor.CanonicalLocalResourcePath, e.Message);
      }
      return false;
    }

    private async Task<bool> ExtractThumbnailAsync(ILocalFsResourceAccessor lfsra, IDictionary<Guid, IList<MediaItemAspect>> extractedAspectData)
    {
      // We can only work on files and make sure this file was detected by a lower MDE before (title is set then).
      // VideoAspect must be present to be sure it is actually a video resource.
      if (!lfsra.IsFile || !extractedAspectData.ContainsKey(VideoStreamAspect.ASPECT_ID))
        return false;

      //ServiceRegistration.Get<ILogger>().Info("OCVVideoThumbnailer: Evaluate {0}", lfsra.ResourceName);

      bool isPrimaryResource = false;
      IList<MultipleMediaItemAspect> resourceAspects;
      if (MediaItemAspect.TryGetAspects(extractedAspectData, ProviderResourceAspect.Metadata, out resourceAspects))
      {
        foreach (MultipleMediaItemAspect pra in resourceAspects)
        {
          string accessorPath = (string)pra.GetAttributeValue(ProviderResourceAspect.ATTR_RESOURCE_ACCESSOR_PATH);
          ResourcePath resourcePath = ResourcePath.Deserialize(accessorPath);
          if (resourcePath.Equals(lfsra.CanonicalLocalResourcePath))
          {
            if (pra.GetAttributeValue<int?>(ProviderResourceAspect.ATTR_TYPE) == ProviderResourceAspect.TYPE_PRIMARY)
            {
              isPrimaryResource = true;
              break;
            }
          }
        }
      }

      if (!isPrimaryResource) //Ignore subtitles
        return false;

      // Check for a reasonable time offset
      int defaultVideoOffset = 720;
      long videoDuration;
      double width = 0;
      double height = 0;
      double downscale = 2; // Reduces the video frame size to a half of original
      IList<MultipleMediaItemAspect> videoAspects;
      if (MediaItemAspect.TryGetAspects(extractedAspectData, VideoStreamAspect.Metadata, out videoAspects))
      {
        if ((videoDuration = videoAspects[0].GetAttributeValue<long>(VideoStreamAspect.ATTR_DURATION)) > 0)
        {
          if (defaultVideoOffset > videoDuration * DEFAULT_THUMBNAIL_OFFSET)
            defaultVideoOffset = Convert.ToInt32(videoDuration * DEFAULT_THUMBNAIL_OFFSET);
        }

        width = videoAspects[0].GetAttributeValue<int>(VideoStreamAspect.ATTR_WIDTH);
        height = videoAspects[0].GetAttributeValue<int>(VideoStreamAspect.ATTR_HEIGHT);
        downscale = width / 256.0; //256 is max size of large thumbnail aspect
      }

      await OPENCV_THROTTLE_LOCK.WaitAsync().ConfigureAwait(false);
      try
      {
        using (lfsra.EnsureLocalFileSystemAccess())
        {
          using (VideoCapture capture = new VideoCapture())
          {
            capture.Open(lfsra.LocalFileSystemPath);
            int capturePos = defaultVideoOffset * 1000;
            if (capture.FrameCount > 0 && capture.Fps > 0)
            {
              var duration = capture.FrameCount / capture.Fps;
              if (defaultVideoOffset > duration)
                capturePos = Convert.ToInt32(duration * DEFAULT_THUMBNAIL_OFFSET * 1000);
            }

            if (capture.FrameWidth > 0 && width == 0)
            {
              width = capture.FrameWidth;
              downscale = width / 256.0; //256 is max size of large thumbnail aspect
            }

            capture.PosMsec = capturePos;
            using (var mat = capture.RetrieveMat())
            {
              if (mat.Height > 0 && mat.Width > 0)
              {
                width = mat.Width;
                height = mat.Height;
                ServiceRegistration.Get<ILogger>().Debug("OCVVideoThumbnailer: Scaling thumbnail of size {1}x{2} for resource '{0}'", lfsra.LocalFileSystemPath, width, height);
                using (var scaledMat = mat.Resize(new OpenCvSharp.Size(width / downscale, height / downscale)))
                {
                  var binary = scaledMat.ToBytes();
                  MediaItemAspect.SetAttribute(extractedAspectData, ThumbnailLargeAspect.ATTR_THUMBNAIL, binary);
                  ServiceRegistration.Get<ILogger>().Info("OCVVideoThumbnailer: Successfully created thumbnail for resource '{0}'", lfsra.LocalFileSystemPath);
                }
              }
              else
              {
                ServiceRegistration.Get<ILogger>().Warn("OCVVideoThumbnailer: Failed to create thumbnail for resource '{0}'", lfsra.LocalFileSystemPath);
              }
            }
          }
        }
      }
      finally
      {
        OPENCV_THROTTLE_LOCK.Release();
      }
      return true;
    }

    public bool IsDirectorySingleResource(IResourceAccessor mediaItemAccessor)
    {
      return false;
    }

    public bool IsStubResource(IResourceAccessor mediaItemAccessor)
    {
      return false;
    }

    public bool TryExtractStubItems(IResourceAccessor mediaItemAccessor, ICollection<IDictionary<Guid, IList<MediaItemAspect>>> extractedStubAspectData)
    {
      return false;
    }

    public Task<IList<MediaItemSearchResult>> SearchForMatchesAsync(IDictionary<Guid, IList<MediaItemAspect>> searchAspectData, ICollection<string> searchCategories)
    {
      return Task.FromResult<IList<MediaItemSearchResult>>(null);
    }

    public Task<bool> AddMatchedAspectDetailsAsync(IDictionary<Guid, IList<MediaItemAspect>> matchedAspectData)
    {
      return Task.FromResult(false);
    }

    public Task<bool> DownloadMetadataAsync(Guid mediaItemId, IDictionary<Guid, IList<MediaItemAspect>> aspectData)
    {
      return Task.FromResult(false);
    }

    #endregion
  }
}
