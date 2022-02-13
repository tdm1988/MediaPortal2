﻿#region Copyright (C) 2007-2017 Team MediaPortal

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

using MP2BootstrapperApp.ChainPackages;
using System.Collections.Generic;
using System.Linq;

namespace MP2BootstrapperApp.FeatureSelection
{
  public class CombinedFeatures : AbstractFeature
  {
    public CombinedFeatures(params IFeature[] features)
      : this((IEnumerable<IFeature>)features)
    {
    }

    public CombinedFeatures(IEnumerable<IFeature> features)
    {
      _excludePackages = new HashSet<PackageId>(features.Select(f => (IEnumerable<PackageId>)f.ExcludePackages).Aggregate((p1, p2) => p1.Intersect(p2)));

      _excludeFeatures = new HashSet<string>(features.Select(f => (IEnumerable<string>)f.ExcludeFeatures).Aggregate((f1, f2) => f1.Intersect(f2)));
    }
  }
}