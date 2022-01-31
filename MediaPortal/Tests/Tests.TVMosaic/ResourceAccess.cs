﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TvMosaic.API;
using TvMosaicMetadataExtractor.ResourceAccess;

namespace Test.TVMosaic
{
  [TestFixture]
  public class ResourceAccess
  {
    [TestCase("/", Description = "Root path")]
    [TestCase("8F94B459-EFC0-4D91-9B29-EC3D72E92677:E44367A7-6293-4492-8C07-0E551195B99F", Description = "Recorded TV container path")]
    [TestCase("8F94B459-EFC0-4D91-9B29-EC3D72E92677:E44367A7-6293-4492-8C07-0E551195B99F/4-1768061841233232138-1642197600", Description = "Recorded TV item path")]
    public void AreValidPathsConsideredResources(string resourcePath)
    {
      Assert.IsTrue(new TvMosaicResourceProvider().IsResource(resourcePath));
    }

    [TestCase("8F94B459-EFC0-4D91-9B29-EC3D72E92677:E44367A7-6293-4492-8C07-0E551195B99F/4-1768061841233232138-1642197600", Description = "Recorded TV item path")]
    public void AreItemPathsConsideredFiles(string resourcePath)
    {
      Assert.IsTrue(new TvMosaicResourceAccessor(null, resourcePath).IsFile);
    }

    [TestCase("/", Description = "Root path")]
    [TestCase("8F94B459-EFC0-4D91-9B29-EC3D72E92677:E44367A7-6293-4492-8C07-0E551195B99F", Description = "Recorded TV container path")]
    public void AreContainerPathsNotConsideredFiles(string resourcePath)
    {
      Assert.IsFalse(new TvMosaicResourceAccessor(null, resourcePath).IsFile);
    }

    [Test]
    public void DoesRootPathReturnDirectories()
    {
      var ra = new TvMosaicResourceAccessor(null, "/");
      var cra = ra.GetChildDirectories();
      CollectionAssert.IsNotEmpty(cra);
      Assert.AreEqual("8F94B459-EFC0-4D91-9B29-EC3D72E92677:E44367A7-6293-4492-8C07-0E551195B99F", cra.First().ResourcePathName);
    }

    [TestCase("8F94B459-EFC0-4D91-9B29-EC3D72E92677:E44367A7-6293-4492-8C07-0E551195B99F", Description = "Recorded TV container path")]
    [TestCase("8F94B459-EFC0-4D91-9B29-EC3D72E92677:E44367A7-6293-4492-8C07-0E551195B99F/4-1768061841233232138-1642197600", Description = "Recorded TV item path")]
    public void DoesObjectPathNotReturnDirectories(string resourcePath)
    {
      var ra = new TvMosaicResourceAccessor(null, resourcePath);
      var cra = ra.GetChildDirectories();
      Assert.IsNull(cra);
    }

    [TestCase("8F94B459-EFC0-4D91-9B29-EC3D72E92677:E44367A7-6293-4492-8C07-0E551195B99F", Description = "Recorded TV container path")]
    public void DoesContainerPathReturnFiles(string resourcePath)
    {
      var ra = new TvMosaicResourceAccessor(null, resourcePath, new MockTvMosaicNavigator());
      var cra = ra.GetFiles();
      CollectionAssert.IsNotEmpty(cra);
      Assert.AreEqual("8F94B459-EFC0-4D91-9B29-EC3D72E92677:E44367A7-6293-4492-8C07-0E551195B99F/4-1768061841233232138-1642197600", cra.First().ResourcePathName);
    }

    [TestCase("/", Description = "Root path")]
    [TestCase("8F94B459-EFC0-4D91-9B29-EC3D72E92677:E44367A7-6293-4492-8C07-0E551195B99F/4-1768061841233232138-1642197600", Description = "Recorded TV item path")]
    public void DoesRootOrItemPathNotReturnFiles(string resourcePath)
    {
      var ra = new TvMosaicResourceAccessor(null, resourcePath, new MockTvMosaicNavigator());
      var cra = ra.GetFiles();
      Assert.IsNull(cra);
    }
  }

  class MockTvMosaicNavigator : ITvMosaicNavigator
  {
    public ICollection<string> GetRootContainerIds()
    {
      return new TvMosaicNavigator().GetRootContainerIds();
    }

    public Items GetChildItems(string containerId)
    {
      // Recorded TV container
      if (containerId == "8F94B459-EFC0-4D91-9B29-EC3D72E92677:E44367A7-6293-4492-8C07-0E551195B99F")
        return new Items
        {
          new RecordedTV
          {
            ObjectID = "8F94B459-EFC0-4D91-9B29-EC3D72E92677:E44367A7-6293-4492-8C07-0E551195B99F/4-1768061841233232138-1642197600",
            ParentID = "8F94B459-EFC0-4D91-9B29-EC3D72E92677:E44367A7-6293-4492-8C07-0E551195B99F",
          },
          new RecordedTV
          {
            ObjectID = "8F94B459-EFC0-4D91-9B29-EC3D72E92677:E44367A7-6293-4492-8C07-0E551195B99F/5-4402021787311710102-1642201500",
            ParentID = "8F94B459-EFC0-4D91-9B29-EC3D72E92677:E44367A7-6293-4492-8C07-0E551195B99F",
          }
        };

      throw new InvalidOperationException();
    }

    public RecordedTV GetItem(string itemId)
    {
      throw new NotImplementedException();
    }

    public bool ObjectExists(string objectId)
    {
      switch (objectId)
      {
        case "8F94B459-EFC0-4D91-9B29-EC3D72E92677:E44367A7-6293-4492-8C07-0E551195B99F":
        case "8F94B459-EFC0-4D91-9B29-EC3D72E92677:E44367A7-6293-4492-8C07-0E551195B99F/4-1768061841233232138-1642197600":
        case "8F94B459-EFC0-4D91-9B29-EC3D72E92677:E44367A7-6293-4492-8C07-0E551195B99F/5-4402021787311710102-1642201500":
          return true;
        default:
          return false;
      }       
    }

    public string GetObjectFriendlyName(string objectId)
    {
      throw new NotImplementedException();
    }
  }
}