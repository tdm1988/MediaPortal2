#region Copyright (C) 2007-2015 Team MediaPortal

/*
    Copyright (C) 2007-2015 Team MediaPortal
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

using MediaPortal.Common.MediaManagement.Helpers;
using System;
using System.Collections.Generic;
using System.Device.Location;

namespace MediaPortal.Extensions.OnlineLibraries.Libraries
{
  /// <summary>
  /// Service to download metadata from online sources.
  /// </summary>
  public interface IOnlineMatcherService
  {
    #region Audio

    bool AssignMissingMusicGenreIds(List<GenreInfo> genres);
    bool FindAndUpdateTrack(TrackInfo trackInfo, bool importOnly);
    bool UpdateAlbumPersons(AlbumInfo albumInfo, string occupation, bool importOnly);
    bool UpdateTrackPersons(TrackInfo trackInfo, string occupation, bool importOnly);
    bool UpdateAlbumCompanies(AlbumInfo albumInfo, string companyType, bool importOnly);
    bool UpdateAlbum(AlbumInfo albumInfo, bool updateTrackList, bool importOnly);
    bool DownloadAudioFanArt(Guid mediaItemId, BaseInfo mediaItemInfo);
    void StoreAudioPersonMatch(PersonInfo person);
    void StoreAudioCompanyMatch(CompanyInfo company);

    #endregion

    #region Movie

    bool AssignMissingMovieGenreIds(List<GenreInfo> genres);
    bool FindAndUpdateMovie(MovieInfo movieInfo, bool importOnly);
    bool UpdatePersons(MovieInfo movieInfo, string occupation, bool importOnly);
    bool UpdateCharacters(MovieInfo movieInfo, bool importOnly);
    bool UpdateCollection(MovieCollectionInfo collectionInfo, bool updateMovieList, bool importOnly);
    bool UpdateCompanies(MovieInfo movieInfo, string companyType, bool importOnly);
    bool DownloadMovieFanArt(Guid mediaItemId, BaseInfo mediaItemInfo);
    void StoreMoviePersonMatch(PersonInfo person);
    void StoreMovieCharacterMatch(CharacterInfo character);
    void StoreMovieCompanyMatch(CompanyInfo company);

    #endregion

    #region Series

    bool AssignMissingSeriesGenreIds(List<GenreInfo> genres);
    bool FindAndUpdateEpisode(EpisodeInfo episodeInfo, bool importOnly);
    bool UpdateEpisodePersons(EpisodeInfo episodeInfo, string occupation, bool importOnly);
    bool UpdateEpisodeCharacters(EpisodeInfo episodeInfo, bool importOnly);
    bool UpdateSeason(SeasonInfo seasonInfo, bool importOnly);
    bool UpdateSeries(SeriesInfo seriesInfo, bool updateEpisodeList, bool importOnly);
    bool UpdateSeriesPersons(SeriesInfo seriesInfo, string occupation, bool importOnly);
    bool UpdateSeriesCharacters(SeriesInfo seriesInfo, bool importOnly);
    bool UpdateSeriesCompanies(SeriesInfo seriesInfo, string companyType, bool importOnly);
    bool DownloadSeriesFanArt(Guid mediaItemId, BaseInfo mediaItemInfo);
    void StoreSeriesPersonMatch(PersonInfo person);
    void StoreSeriesCharacterMatch(CharacterInfo character);
    void StoreSeriesCompanyMatch(CompanyInfo company);

    #endregion
  }
}