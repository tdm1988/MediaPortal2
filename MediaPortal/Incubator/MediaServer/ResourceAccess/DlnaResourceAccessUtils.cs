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

using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.ResourceAccess;
using MediaPortal.Common.Services.ResourceAccess.Settings;
using MediaPortal.Common.Settings;
using MediaPortal.Extensions.MediaServer.DLNA;
using MediaPortal.Extensions.MediaServer.Profiles;
using MediaPortal.Utilities.Network;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Extensions.TranscodingService.Interfaces;
using MediaPortal.Extensions.TranscodingService.Interfaces.Helpers;
using MediaPortal.Extensions.TranscodingService.Interfaces.Transcoding;
using MediaPortal.Common.FanArt;

namespace MediaPortal.Extensions.MediaServer.ResourceAccess
{
  public static class DlnaResourceAccessUtils
  {
    /// <summary>
    /// Base HTTP path for resource access, e.g. "/GetDlnaResource".
    /// </summary>
    public const string RESOURCE_ACCESS_PATH = "/GetDlnaResource";
    public const string TV_CHANNEL_RESOURCE = "TV_CHANNEL_";
    public const string RADIO_CHANNEL_RESOURCE = "RADIO_CHANNEL_";

    /// <summary>
    /// Argument name for the resource path argument, e.g. "MediaItem".
    /// </summary>
    public const string RESOURCE_PATH_ARGUMENT_NAME = "ResourcePath";

    public const string SYNTAX = RESOURCE_ACCESS_PATH + "/[media item id]";

    public const int DEFAULT_IMAGE_SIZE = 160;

    public static string GetResourceUrl(string mediaItem, Guid clientId)
    {
      return RESOURCE_ACCESS_PATH + "/" + mediaItem + "?id=" + clientId.ToString();
    }

    public static bool ParseMediaItem(Uri resourceUri, out Guid mediaItemGuid)
    {
      mediaItemGuid = Guid.Empty;
      try
      {
        var r = Regex.Match(resourceUri.PathAndQuery, RESOURCE_ACCESS_PATH + @"\/([\w-]*)\/?");
        var mediaItem = r.Groups[1].Value;
        if (mediaItem.Contains("."))
        {
          mediaItem = mediaItem.Substring(0, mediaItem.IndexOf("."));
        }
        if (Guid.TryParse(mediaItem, out mediaItemGuid))
        {
          return true;
        }
      }
      catch (Exception e)
      {
        ServiceRegistration.Get<ILogger>().Warn("ParseMediaItem: Failed with input url {0}", e, resourceUri.OriginalString);
      }

      return false;
    }

    public static bool ParseRadioChannel(Uri resourceUri, out int radioChannel)
    {
      radioChannel = 0;
      try
      {
        var r = Regex.Match(resourceUri.PathAndQuery, RESOURCE_ACCESS_PATH + $@"\/{RADIO_CHANNEL_RESOURCE}([\w-]*)\/?");
        var channel = r.Groups[1].Value;
        if (int.TryParse(channel, out radioChannel))
        {
          return true;
        }
      }
      catch (Exception e)
      {
        ServiceRegistration.Get<ILogger>().Warn("ParseRadioChannel: Failed with input url {0}", e, resourceUri.OriginalString);
      }
      return false;
    }

    public static bool ParseTVChannel(Uri resourceUri, out int tvChannel)
    {
      tvChannel = 0;
      try
      {
        var r = Regex.Match(resourceUri.PathAndQuery, RESOURCE_ACCESS_PATH + $@"\/{TV_CHANNEL_RESOURCE}([\w-]*)\/?");
        var channel = r.Groups[1].Value;
        if (int.TryParse(channel, out tvChannel))
        {
          return true;
        }
      }
      catch (Exception e)
      {
        ServiceRegistration.Get<ILogger>().Warn("ParseTVChannel: Failed with input url {0}", e, resourceUri.OriginalString);
      }
      return false;
    }

    public static bool UseSoftCodedSubtitle(EndPointSettings client, out SubtitleCodec targetCodec, out string targetMime)
    {
      targetCodec = SubtitleCodec.Unknown;
      targetMime = "text/plain";
      if (client.Profile.MediaTranscoding?.SubtitleSettings?.SubtitleMode == SubtitleSupport.SoftCoded)
      {
        targetCodec = client.Profile.MediaTranscoding.SubtitleSettings.SubtitlesSupported[0].Format;
        if (string.IsNullOrEmpty(client.Profile.MediaTranscoding.SubtitleSettings.SubtitlesSupported[0].Mime) == false)
          targetMime = client.Profile.MediaTranscoding.SubtitleSettings.SubtitlesSupported[0].Mime;
        else
          targetMime = SubtitleHelper.GetSubtitleMime(targetCodec);
        return true;
      }
      return false;
    }

    private static IPAddress GetLocalIp()
    {
      ServerSettings settings = ServiceRegistration.Get<ISettingsManager>().Load<ServerSettings>();
      bool useIPv4 = settings.UseIPv4;
      bool useIPv6 = settings.UseIPv6;

      var host = Dns.GetHostEntry(Dns.GetHostName());
      foreach (var ip in host.AddressList)
      {
        if (IPAddress.IsLoopback(ip))
          continue;

        if (useIPv4 && ip.AddressFamily == AddressFamily.InterNetwork)
          return ip;

        if (useIPv6 && ip.AddressFamily == AddressFamily.InterNetworkV6)
          return ip;
      }
      return null;
    }

    public static bool IsSoftCodedSubtitleAvailable(DlnaMediaItem dlnaItem, EndPointSettings client)
    {
      if (client.Profile.MediaTranscoding.SubtitleSettings.SubtitleMode != SubtitleSupport.SoftCoded)
      {
        return false;
      }
      if (dlnaItem.IsTranscoded && dlnaItem.IsVideo)
      {
        VideoTranscoding video = (VideoTranscoding)dlnaItem.TranscodingParameter;
        return video?.SourceSubtitles.Count > 0;
      }
      else if (dlnaItem.IsVideo)
      {
        VideoTranscoding subtitleVideo = (VideoTranscoding)dlnaItem.SubtitleTranscodingParameter;
        if (subtitleVideo?.SourceSubtitles.Count > 0)
          return true;

        return dlnaItem.Subtitles.Count > 0;
      }
      return false;
    }

    public static string GetFanArtMediaType(MediaItem item)
    {
      string mediaType = FanArtMediaTypes.Undefined;
      if (item.Aspects.ContainsKey(ImageAspect.ASPECT_ID)) mediaType = FanArtMediaTypes.Image;
      else if (item.Aspects.ContainsKey(MovieAspect.ASPECT_ID)) mediaType = FanArtMediaTypes.Movie;
      else if (item.Aspects.ContainsKey(MovieCollectionAspect.ASPECT_ID)) mediaType = FanArtMediaTypes.MovieCollection;
      else if (item.Aspects.ContainsKey(SeriesAspect.ASPECT_ID)) mediaType = FanArtMediaTypes.Series;
      else if (item.Aspects.ContainsKey(SeasonAspect.ASPECT_ID)) mediaType = FanArtMediaTypes.SeriesSeason;
      else if (item.Aspects.ContainsKey(AudioAspect.ASPECT_ID)) mediaType = FanArtMediaTypes.Audio;
      else if (item.Aspects.ContainsKey(AudioAlbumAspect.ASPECT_ID)) mediaType = FanArtMediaTypes.Album;
      else if (item.Aspects.ContainsKey(EpisodeAspect.ASPECT_ID)) mediaType = FanArtMediaTypes.Episode;
      else if (item.Aspects.ContainsKey(CharacterAspect.ASPECT_ID)) mediaType = FanArtMediaTypes.Character;
      return mediaType;
    }

    public static string GetThumbnailBaseURL(Guid mediaItemId, EndPointSettings client, string fanartType = null)
    {
      string mediaType = fanartType ?? FanArtMediaTypes.Undefined;

      // Using MP2's FanArtService provides access to all kind of resources, thumbnails from ML and also local fanart from filesystem
      string url = string.Format("{0}/FanartService?mediatype={1}&fanarttype={2}&name={3}&width={4}&height={5}",
        GetBaseResourceURL(), mediaType, FanArtTypes.Thumbnail, mediaItemId,
        client.Profile?.Settings.Thumbnails.MaxWidth ?? DEFAULT_IMAGE_SIZE, client.Profile?.Settings.Thumbnails.MaxHeight ?? DEFAULT_IMAGE_SIZE);
      return url;
    }

    public static string GetChannelLogoBaseURL(string channelName, EndPointSettings client, bool isTV)
    {
      string mediaType = isTV ? FanArtMediaTypes.ChannelTv : FanArtMediaTypes.ChannelRadio;
      string url = string.Format("{0}/FanartService?mediatype={1}&fanarttype={2}l&name={3}&width={4}&height={5}",
          GetBaseResourceURL(), mediaType, FanArtTypes.Thumbnail, WebUtility.UrlEncode(channelName),
          client.Profile?.Settings.Thumbnails.MaxWidth ?? DEFAULT_IMAGE_SIZE, client.Profile?.Settings.Thumbnails.MaxHeight ?? DEFAULT_IMAGE_SIZE);
      return url;
    }

    public static string GetSubtitleBaseURL(Guid mediaItemId, EndPointSettings client, out string subMime, out string subExtension)
    {
      SubtitleCodec codec = SubtitleCodec.Unknown;
      subMime = null;
      subExtension = null;

      if (UseSoftCodedSubtitle(client, out codec, out subMime) == true)
      {
        subExtension = SubtitleHelper.GetSubtitleExtension(codec);
        string subType = codec.ToString();
        return string.Format(GetBaseResourceURL()
                    + GetResourceUrl(mediaItemId.ToString(), client.ClientId)
                    + "&aspect=SUBTITLE&type={0}&file=subtitle.{1}", subType, subExtension);
      }
      return null;
    }

    public static string GetBaseResourceURL()
    {
      var rs = ServiceRegistration.Get<IResourceServer>();
      return rs.GetServiceUrl(GetLocalIp());
    }
  }
}
