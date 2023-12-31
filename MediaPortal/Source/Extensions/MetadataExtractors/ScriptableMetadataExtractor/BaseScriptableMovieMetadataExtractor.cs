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
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.ResourceAccess;
using MediaPortal.Common.Settings;
using MediaPortal.Extensions.MetadataExtractors.ScriptableMetadataExtractor.Data;
using MediaPortal.Extensions.MetadataExtractors.ScriptableMetadataExtractor.Settings;
using MediaPortal.Extensions.OnlineLibraries;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MediaPortal.Common.Services.Settings;

namespace MediaPortal.Extensions.MetadataExtractors.ScriptableMetadataExtractor
{
  /// <summary>
  /// MediaPortal 2 base scriptable metadata extractor implementation.
  /// </summary>
  public abstract class BaseScriptableMovieMetadataExtractor : IMetadataExtractor
  {
    #region Constants

    public const string MEDIA_CATEGORY_NAME_MOVIE = "Movie";

    private static ConcurrentBag<string> _metadataExtractorCustomCategories = null;
    private static SettingsChangeWatcher<ScriptableMetadataExtractorSettings> _scriptableSettingWatcher = null;
    private static ConcurrentDictionary<string, string> _customSettings = new ConcurrentDictionary<string, string>();
    private static string _defaultUserAgent = null;
    private static Dictionary<string, List<ScriptableScraperMovieMatcher>> _matchers = new Dictionary<string, List<ScriptableScraperMovieMatcher>>();

    private const string SAMPLE_KEY = "Setting";
    private const string SAMPLE_VALUE = "Value";

    private string _category;
    private MetadataExtractorMetadata _metadata;

    public MetadataExtractorMetadata Metadata => _metadata;

    #endregion

    #region Ctor

    static BaseScriptableMovieMetadataExtractor()
    {
      ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
      var settings = settingsManager.Load<ScriptableMetadataExtractorSettings>();
      if (settings.CustomSettings.Count == 0 && settings.DefaultUserAgent == null)
      {
        settings.DefaultUserAgent = "";
        settings.CustomSettings.Add($"{SAMPLE_KEY}={SAMPLE_VALUE}");
        settingsManager.Save(settings); //Save sample settings so they can be edited
      }

      _scriptableSettingWatcher = new SettingsChangeWatcher<ScriptableMetadataExtractorSettings>();
      _scriptableSettingWatcher.SettingsChanged += (sender, args) =>
      {
        Dictionary<string, string> customSettings = new Dictionary<string, string>();
        foreach (var settingPair in _scriptableSettingWatcher.Settings.CustomSettings)
        {
          var split = settingPair.Split('=');
          if (split.Length == 2)
          {
            if (string.IsNullOrWhiteSpace(split[0]) || split[0] == SAMPLE_KEY)
              continue; //Ignore empty or sample entry

            customSettings.Add(split[0], split[1]);
          }
        }

        //Add update custom settings
        foreach (var setting in customSettings)
          _customSettings.AddOrUpdate(setting.Key, setting.Value, (k, v) => setting.Value);

        //Delete removed custom settings
        foreach (var settingKey in _customSettings.Keys.Except(customSettings.Keys))
          _customSettings.TryRemove(settingKey, out _);

        //Update custom user agent
        if (!string.IsNullOrWhiteSpace(_scriptableSettingWatcher.Settings.DefaultUserAgent))
          _defaultUserAgent = _scriptableSettingWatcher.Settings.DefaultUserAgent;
      };
      _scriptableSettingWatcher.Refresh();

      LoadScripts();
    }

    public BaseScriptableMovieMetadataExtractor(string id)
    {
      try
      {
        bool loaded = _metadataExtractorCustomCategories.TryTake(out _category);

        List<MediaCategory> mediaCategories = new List<MediaCategory>();
        if (loaded)
        {
          MediaCategory mediaCategory;
          IMediaAccessor mediaAccessor = ServiceRegistration.Get<IMediaAccessor>();
          if (!mediaAccessor.MediaCategories.TryGetValue(_category, out mediaCategory))
            mediaCategory = mediaAccessor.RegisterMediaCategory(_category, new List<MediaCategory> { DefaultMediaCategories.Video });
          mediaCategories.Add(mediaCategory);
        }

        _metadata = new MetadataExtractorMetadata(new Guid(id), $"Scriptable movie metadata extractor ({(loaded ? _category : "Disabled")})", MetadataExtractorPriority.External, true,
            mediaCategories, new MediaItemAspectMetadata[] { MediaAspect.Metadata, MovieAspect.Metadata });

        if (loaded && _matchers.ContainsKey(_category))
          OnlineMatcherService.RegisterMovieMatchers(_matchers[_category].ToArray(), _category);
      }
      catch (Exception ex)
      {
        ServiceRegistration.Get<ILogger>().Error("ScriptableMetadataExtractor: Error initializing metadata extractor", ex);
      }
    }

    private static void LoadScripts()
    {
      try
      {
        _metadataExtractorCustomCategories = new ConcurrentBag<string>();

        //Load latest version of scripts
        Assembly assembly = Assembly.GetExecutingAssembly();
        Dictionary<int, ScriptableScript> scripts = new Dictionary<int, ScriptableScript>();
        foreach (var file in Directory.EnumerateFiles(Path.Combine(Path.GetDirectoryName(assembly.Location), "MovieScraperScripts\\"), "*.xml"))
        {
          var script = new ScriptableScript();
          if (script.Load(file))
          {
            if (string.IsNullOrEmpty(script.Category))
              script.Category = MEDIA_CATEGORY_NAME_MOVIE;

            if (!scripts.ContainsKey(script.ScriptID))
              scripts.Add(script.ScriptID, script);
            else if (Version.TryParse(scripts[script.ScriptID].Version, out var curVer) && Version.TryParse(script.Version, out var newVer) && newVer > curVer)
              scripts[script.ScriptID] = script;
          }
        }

        //Categorize scripts
        foreach (var script in scripts.Values)
        {
          if (!_matchers.ContainsKey(script.Category))
            _matchers.Add(script.Category, new List<ScriptableScraperMovieMatcher>());
          _matchers[script.Category].Add(new ScriptableScraperMovieMatcher(script));
        }

        //Store custom MDE movie categories
        foreach (var matcher in _matchers)
        {
          if (!_metadataExtractorCustomCategories.Contains(matcher.Key))
            _metadataExtractorCustomCategories.Add(matcher.Key);
        }
      }
      catch (Exception ex)
      {
        ServiceRegistration.Get<ILogger>().Error("ScriptableMetadataExtractor: Error initializing scripts", ex);
      }
    }

    public void Dispose()
    {
      _scriptableSettingWatcher.Dispose();
    }

    #endregion

    #region Static methods

    public static void AddOrUpdateParamsFromCustomSettings(Dictionary<string, string> paramList)
    {
      foreach (var setting in _customSettings)
        paramList[setting.Key] = setting.Value;

      if (!string.IsNullOrWhiteSpace(_defaultUserAgent))
        paramList["settings.defaultuseragent"] = _defaultUserAgent;
    }

    public Task<bool> TryExtractMetadataAsync(IResourceAccessor mediaItemAccessor, IDictionary<Guid, IList<MediaItemAspect>> extractedAspectData, bool forceQuickMode)
    {
      return Task.FromResult(false);
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
