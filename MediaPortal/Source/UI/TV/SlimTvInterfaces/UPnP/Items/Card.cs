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
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.Plugins.SlimTv.Interfaces.Items;

namespace MediaPortal.Plugins.SlimTv.Interfaces.UPnP.Items
{
  public class Card : ICard
  {
    private static XmlSerializer _xmlSerializer;

    #region ICard Member

    public int CardId { get; set; }

    public string Name { get; set; }

    public bool EpgIsGrabbing { get; set; }

    public bool HasCam { get; set; }

    public SlimTvCamType CamType { get; set; }

    public int DecryptLimit { get; set; }

    public string DevicePath { get; set; }

    public bool Enabled { get; set; }

    public string RecordingFolder { get; set; }

    public int RecordingFormat { get; set; }

    public string TimeshiftFolder { get; set; }

    public int Priority { get; set; }

    public bool PreloadCard { get; set; }

    public bool SupportSubChannels { get; set; }

    #endregion

    /// <summary>
    /// Serializes this Channel instance to the given <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">Writer to write the XML serialization to.</param>
    public void Serialize(XmlWriter writer)
    {
      XmlSerializer xs = GetOrCreateXMLSerializer();
      xs.Serialize(writer, this);
    }

    /// <summary>
    /// Deserializes a Card instance from a given XML fragment.
    /// </summary>
    /// <param name="str">XML fragment containing a serialized user profile instance.</param>
    /// <returns>Deserialized instance.</returns>
    public static Card Deserialize(string str)
    {
      XmlSerializer xs = GetOrCreateXMLSerializer();
      using (StringReader reader = new StringReader(str))
        return xs.Deserialize(reader) as Card;
    }

    /// <summary>
    /// Deserializes a Card instance from a given <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">XML reader containing a serialized user profile instance.</param>
    /// <returns>Deserialized instance.</returns>
    public static Card Deserialize(XmlReader reader)
    {
      XmlSerializer xs = GetOrCreateXMLSerializer();
      return xs.Deserialize(reader) as Card;
    }

    protected static XmlSerializer GetOrCreateXMLSerializer()
    {
      return _xmlSerializer ?? (_xmlSerializer = new XmlSerializer(typeof(Card)));
    }
  }
}