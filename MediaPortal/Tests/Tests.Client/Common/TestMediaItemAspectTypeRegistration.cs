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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaPortal.Common.MediaManagement;

namespace Tests.Common
{
  public class TestMediaItemAspectTypeRegistration : IMediaItemAspectTypeRegistration
  {
    protected IDictionary<Guid, MediaItemAspectMetadata> _locallyKnownMediaItemAspectTypes = new ConcurrentDictionary<Guid, MediaItemAspectMetadata>();
    protected IDictionary<Guid, MediaItemAspectMetadata> _locallySupportedReimportMediaItemAspectTypes = new ConcurrentDictionary<Guid, MediaItemAspectMetadata>();

    public IDictionary<Guid, MediaItemAspectMetadata> LocallyKnownMediaItemAspectTypes
    {
      get { return _locallyKnownMediaItemAspectTypes; }
    }

    public IDictionary<Guid, MediaItemAspectMetadata> LocallySupportedReimportMediaItemAspectTypes
    {
      get { return _locallySupportedReimportMediaItemAspectTypes; }
    }

    public async Task RegisterLocallyKnownMediaItemAspectTypeAsync(IEnumerable<MediaItemAspectMetadata> miaTypes)
    {
      await Task.WhenAll(miaTypes.Select(RegisterLocallyKnownMediaItemAspectTypeAsync));
    }

    public Task RegisterLocallyKnownMediaItemAspectTypeAsync(MediaItemAspectMetadata miaType)
    {
      Console.WriteLine("Registering " + miaType.Name);
      if (_locallyKnownMediaItemAspectTypes.ContainsKey(miaType.AspectId))
        return Task.CompletedTask;
      _locallyKnownMediaItemAspectTypes.Add(miaType.AspectId, miaType);
      return Task.CompletedTask;
    }

    public Task RegisterLocallySupportedReimportMediaItemAspectTypeAsync(MediaItemAspectMetadata miaType)
    {
      Console.WriteLine("Registering reimport support " + miaType.Name);
      if (_locallySupportedReimportMediaItemAspectTypes.ContainsKey(miaType.AspectId))
        return Task.CompletedTask;
      _locallySupportedReimportMediaItemAspectTypes.Add(miaType.AspectId, miaType);
      return Task.CompletedTask;
    }
  }
}
