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

using System.Collections.Generic;
using System.Xml.Linq;
using MediaPortal.Common.Logging;
using MediaPortal.Extensions.MetadataExtractors.StubMetadataExtractors.Settings;
using MediaPortal.Extensions.MetadataExtractors.StubMetadataExtractors.Stubs;
using System.Linq;

namespace MediaPortal.Extensions.MetadataExtractors.StubMetadataExtractors.StubReaders
{
  /// <summary>
  /// Reads the content of a stub-file for albums into <see cref="AlbumStub"/> objects
  /// </summary>
  /// <remarks>
  /// There is a TryRead method for any known child element of the stub-file's root element and a
  /// TryWrite method for any MIA-Attribute we store values in.
  /// </remarks>
  class StubAlbumReader : StubReaderBase<AlbumStub>
  {
    #region Consts

    /// <summary>
    /// The name of the root element in a valid stub-file for albums
    /// </summary>
    private const string ALBUM_ROOT_ELEMENT_NAME = "discstub";

    #endregion

    #region Ctor

    /// <summary>
    /// Instantiates a <see cref="StubAlbumReader"/> object
    /// </summary>
    /// <param name="debugLogger">Debug logger to log to</param>
    /// <param name="miNumber">Unique number of the MediaItem for which the stub-file is parsed</param>
    /// <param name="importOnly">If true, no long lasting operations such as parsing images are performed</param>
    /// <param name="settings">Settings of the <see cref="StubMovieMetadataExtractor"/></param>
    public StubAlbumReader(ILogger debugLogger, long miNumber, bool importOnly, StubAudioMetadataExtractorSettings settings)
      : base(debugLogger, miNumber, importOnly, settings)
    {
      _settings = settings;
      InitializeSupportedElements();
    }

    #endregion

    #region Ctor helpers

    /// <summary>
    /// Adds a delegate for each xml element in a movie stub-file that is understood by this MetadataExtractor to StubReaderBase._supportedElements
    /// </summary>
    private void InitializeSupportedElements()
    {
      _supportedElements.Add("disc", new TryReadElementDelegate(TryReadDisc));
      _supportedElements.Add("title", new TryReadElementDelegate(TryReadTitle));
      _supportedElements.Add("message", new TryReadElementDelegate(TryReadMessage));
      _supportedElements.Add("cd", new TryReadElementDelegate(TryReadCd));
      _supportedElements.Add("tracks", new TryReadElementDelegate(TryReadTracks));
      _supportedElements.Add("artists", new TryReadElementDelegate(TryReadArtists));
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Gets the <see cref="AlbumStub"/> objects generated by this class
    /// </summary>
    /// <returns>List of <see cref="AlbumStub"/> objects</returns>
    public List<AlbumStub> GetAlbumStubs()
    {
      return _stubs.Where(s => s.Valid).ToList();
    }

    #endregion

    #region Reader methods for direct child elements of the root element

    /// <summary>
    /// Tries to read the title
    /// </summary>
    /// <param name="element"><see cref="XElement"/> to read from</param>
    /// <returns><c>true</c> if a value was found in <paramref name="element"/>; otherwise <c>false</c></returns>
    private bool TryReadTitle(XElement element)
    {
      // Example of a valid element:
      // <title>Album Name</title>
      _currentStub.Title = ParseSimpleString(element);
      if (_currentStub.Title == null)
        _currentStub.Valid = false;
      return _currentStub.Title != null;
    }

    /// <summary>
    /// Tries to read the name of the disc
    /// </summary>
    /// <param name="element"><see cref="XElement"/> to read from</param>
    /// <returns><c>true</c> if a value was found in <paramref name="element"/>; otherwise <c>false</c></returns>
    private bool TryReadDisc(XElement element)
    {
      // Example of a valid element:
      // <disc>Album vol. 1</disc>
      return ((_currentStub.DiscName = ParseSimpleString(element)) != null);
    }

    /// <summary>
    /// Tries to read the message to show when the disc is needed
    /// </summary>
    /// <param name="element"><see cref="XElement"/> to read from</param>
    /// <returns><c>true</c> if a value was found in <paramref name="element"/>; otherwise <c>false</c></returns>
    private bool TryReadMessage(XElement element)
    {
      // Example of a valid element:
      // <message>Please insert disc</message>
      return ((_currentStub.Message = ParseSimpleString(element)) != null);
    }

    /// <summary>
    /// Tries to read the CD number
    /// </summary>
    /// <param name="element"><see cref="XElement"/> to read from</param>
    /// <returns><c>true</c> if a value was found in <paramref name="element"/>; otherwise <c>false</c></returns>
    private bool TryReadCd(XElement element)
    {
      // Example of a valid element:
      // <cd>1</cd>
      return ((_currentStub.Cd = ParseSimpleInt(element)) != null);
    }

    /// <summary>
    /// Tries to read the number of tracks
    /// </summary>
    /// <param name="element"><see cref="XElement"/> to read from</param>
    /// <returns><c>true</c> if a value was found in <paramref name="element"/>; otherwise <c>false</c></returns>
    private bool TryReadTracks(XElement element)
    {
      // Example of a valid element:
      // <tracks>1</tracks>
      _currentStub.Tracks = ParseSimpleInt(element);
      if (!_currentStub.Tracks.HasValue)
        _currentStub.Valid = false;
      return _currentStub.Tracks.HasValue;
    }

    /// <summary>
    /// Tries to read the album artists
    /// </summary>
    /// <param name="element"><see cref="XElement"/> to read from</param>
    /// <returns><c>true</c> if a value was found in <paramref name="element"/>; otherwise <c>false</c></returns>
    private bool TryReadArtists(XElement element)
    {
      // Example of a valid element:
      // <artitsts>Artist Name</artitsts>
      _currentStub.Artists = ParseCharacterSeparatedStrings(element, _currentStub.Artists);
      if (_currentStub.Artists == null || _currentStub.Artists.Count == 0)
        _currentStub.Valid = false;

      return _currentStub.Artists != null && _currentStub.Artists.Count > 0;
    }

    #endregion

    #region General helper methods

    /// <summary>
    /// Ignores the respective element
    /// </summary>
    /// <param name="element"><see cref="XElement"/> to ignore</param>
    /// <returns><c>false</c></returns>
    /// <remarks>
    /// We use this method as TryReadElementDelegate for elements, of which we know that they are irrelevant in the context of a movie,
    /// but which are nevertheless contained in some albums's stub-files. Having this method registered as handler delegate avoids that
    /// the respective xml element is logged as unknown element.
    /// </remarks>
    private static bool Ignore(XElement element)
    {
      return false;
    }

    #endregion

    #region BaseOverrides

    /// <summary>
    /// Checks whether the <paramref name="itemRootElement"/>'s name is "album"
    /// </summary>
    /// <param name="itemRootElement">Element to check</param>
    /// <returns><c>true</c> if the element's name is "album"; else <c>false</c></returns>
    protected override bool CanReadItemRootElementTree(XElement itemRootElement)
    {
      var itemRootElementName = itemRootElement.Name.ToString();
      if (itemRootElementName == ALBUM_ROOT_ELEMENT_NAME)
        return true;
      _debugLogger.Warn("[#{0}]: Cannot extract metadata; name of the item root element is {1} instead of {2}", _miNumber, itemRootElementName, ALBUM_ROOT_ELEMENT_NAME);
      return false;
    }

    #endregion
  }
}
