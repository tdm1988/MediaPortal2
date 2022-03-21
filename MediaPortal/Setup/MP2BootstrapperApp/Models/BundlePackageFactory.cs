﻿#region Copyright (C) 2007-2021 Team MediaPortal

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

using MP2BootstrapperApp.ChainPackages;
using System;
using System.Xml.Linq;

namespace MP2BootstrapperApp.Models
{
  public class BundlePackageFactory
  {
    protected IPackageContext _packageContext;

    public BundlePackageFactory(IPackageContext packageContext)
    {
      _packageContext = packageContext;
    }

    public IBundlePackage CreatePackage(XElement packageElement)
    {
      string packageIdString = packageElement.Attribute("Package")?.Value;
      PackageId packageId = Enum.TryParse(packageIdString, out PackageId id) ? id : PackageId.Unknown;

      if (!_packageContext.TryGetPackage(packageId, out IPackage package))
        throw new InvalidOperationException($"{nameof(BundlePackageFactory)}: {nameof(_packageContext)} does not contain package info for bundle package with id {packageIdString}");

      bool isMsiPackage = string.Equals(packageElement.Attribute("PackageType")?.Value, "Msi", StringComparison.InvariantCultureIgnoreCase);

      return isMsiPackage ? new BundleMsiPackage(packageId, packageElement, package) : new BundlePackage(packageId, packageElement, package);
    }

    public IBundlePackageFeature CreatePackageFeature(XElement featureElement)
    {
      string packageIdString = featureElement.Attribute("Package")?.Value;
      PackageId packageId = Enum.TryParse(packageIdString, out PackageId id) ? id : PackageId.Unknown;

      if (!_packageContext.TryGetPackage(packageId, out IPackage package))
        throw new InvalidOperationException($"{nameof(BundlePackageFactory)}: {nameof(_packageContext)} does not contain package info for bundle package with id {packageIdString}");

      string featureIdString = featureElement.Attribute("Feature")?.Value;
      FeatureId featureId = Enum.TryParse(featureIdString, out FeatureId fid) ? fid : FeatureId.Unknown;

      return new BundlePackageFeature(featureId, featureElement, package);
    }
  }
}
