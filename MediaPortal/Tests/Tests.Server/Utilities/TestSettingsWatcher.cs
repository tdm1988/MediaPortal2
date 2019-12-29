#region Copyright (C) 2007-2018 Team MediaPortal

/*
    Copyright (C) 2007-2018 Team MediaPortal
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
using MediaPortal.Common.Messaging;
using MediaPortal.Common.Services.Logging;
using MediaPortal.Common.Services.Messaging;
using MediaPortal.Common.Services.Settings;
using MediaPortal.Common.Settings;
using MediaPortal.Utilities.Cache;
using NUnit.Framework;

namespace Tests.Server.Utilities
{
  public class TestSettingsManager : ISettingsManager
  {
    #region ISettingsManager Members

    public SettingsType Load<SettingsType>() where SettingsType : class
    {
      return (SettingsType)Load(typeof(SettingsType));
    }

    public object Load(Type settingsType)
    {
      return Activator.CreateInstance(settingsType);
    }

    public void Save(object settingsObject)
    {
      // Send a message that the setting has been changed.
      Type t = settingsObject.GetType();
      SettingsManagerMessaging.SendSettingsChangeMessage(t);
    }

    public void StartBatchUpdate() { }

    public void EndBatchUpdate() { }

    public void CancelBatchUpdate() { }

    public void ClearCache() { }

    public void ChangeUserContext(string userName) { }

    public void RemoveSettingsData(Type settingsType, bool user, bool global) { }

    public void RemoveAllSettingsData(bool user, bool global) { }

    #endregion
  }

  public class TestSetting
  {
    [Setting(SettingScope.Global, true)]
    public bool Test { get; set; }
  }

  [TestFixture]
  public class TestSettingsWatcher
  {
    private const int WATCHER_COUNT = 300;
    private const int WAIT_TIMEOUT = 1000;
    private const int MANY_FACTOR = 10;

    private static SemaphoreSlim _testSync = new SemaphoreSlim(1, 1);

    [OneTimeSetUp]
    public void Init()
    {
      ServiceRegistration.Set<IMessageBroker>(new MessageBroker());
      ServiceRegistration.Set<ISettingsManager>(new TestSettingsManager());
      ServiceRegistration.Set<ILogger>(new NoLogger());
    }

    private List<SettingsChangeWatcher<TestSetting>> CreateWatchers(int count, EventHandler eventHandler, bool shareEvent)
    {
      List<SettingsChangeWatcher<TestSetting>> watchers = new List<SettingsChangeWatcher<TestSetting>>();
      for (int i = 0; i < count; i++)
      {
        var watcher = new SettingsChangeWatcher<TestSetting>();
        if (!shareEvent)
        {
          EventHandler evt = (s, e) =>
          {
            eventHandler.Invoke(s, e);
          };
          watcher.SettingsChanged += evt;
        }
        else
        {
          watcher.SettingsChanged += eventHandler;
        }
        watchers.Add(watcher);
      }
      return watchers;
    }

    private void DisposeWatchers(List<SettingsChangeWatcher<TestSetting>> watchers, bool skipFirst = false)
    {
      for (int i = 0; i < watchers.Count; i++)
      {
        if (i == 0 && skipFirst)
          continue;

        watchers[i].Dispose();
      }
    }

    private List<SettingsChangeWatcherOld<TestSetting>> CreateOldWatchers(int count, EventHandler eventHandler, bool shareEvent)
    {
      List<SettingsChangeWatcherOld<TestSetting>> watchers = new List<SettingsChangeWatcherOld<TestSetting>>();
      for (int i = 0; i < count; i++)
      {
        var watcher = new SettingsChangeWatcherOld<TestSetting>();
        if (!shareEvent)
        {
          EventHandler evt = (s, e) =>
          {
            eventHandler.Invoke(s, e);
          };
          watcher.SettingsChanged += evt;
        }
        else
        {
          watcher.SettingsChanged += eventHandler;
        }
        watchers.Add(watcher);
      }
      return watchers;
    }

    private void DisposeOldWatchers(List<SettingsChangeWatcherOld<TestSetting>> watchers, bool skipFirst = false)
    {
      for (int i = 0; i < watchers.Count; i++)
      {
        if (i == 0 && skipFirst)
          continue;

        watchers[i].Dispose();
      }
    }

    private void WaitForEvents(int timeout, Func<bool> isDone)
    {
      int waitCount = timeout / 100;
      while (!isDone() && waitCount > 0)
      {
        waitCount--;
        Thread.Sleep(100);
      }
    }

    private void FireEvents()
    {
      var settings = ServiceRegistration.Get<ISettingsManager>().Load<TestSetting>();
      ServiceRegistration.Get<ISettingsManager>().Save(settings);
    }

    [Test, Order(1)]
    public void TestSettingChangeStartStop()
    {
      _testSync.Wait();
      try
      {
        //Arrange
        EventHandler evt = (s, e) => { };
        var watchers = CreateWatchers(WATCHER_COUNT, evt, false);

        //Act
        bool started = SettingsChangeWatcher.IsRunning;
        DisposeWatchers(watchers, true);
        bool stillStarted = SettingsChangeWatcher.IsRunning;
        watchers[0].Dispose();

        //Assert
        Assert.IsTrue(started, "SettingsChangeWatcher never started");
        Assert.IsTrue(stillStarted, "SettingsChangeWatcher stopped too early");
        Assert.IsTrue(!SettingsChangeWatcher.IsRunning, "SettingsChangeWatcher still running");
      }
      finally
      {
        _testSync.Release();
      }
    }

    [Test, Order(2)]
    public void TestSettingChangeStartStopSharedEvent()
    {
      _testSync.Wait();
      try
      {
        //Arrange
        EventHandler evt = (s, e) => { };
        var watchers = CreateWatchers(WATCHER_COUNT, evt, true);

        //Act
        bool started = SettingsChangeWatcher.IsRunning;
        DisposeWatchers(watchers, true);
        bool stillStarted = SettingsChangeWatcher.IsRunning;
        watchers[0].Dispose();

        //Assert
        Assert.IsTrue(started, "SettingsChangeWatcher never started");
        Assert.IsTrue(stillStarted, "SettingsChangeWatcher stopped too early");
        Assert.IsTrue(!SettingsChangeWatcher.IsRunning, "SettingsChangeWatcher still running");
      }
      finally
      {
        _testSync.Release();
      }
    }

    [Test, Order(3)]
    public void TestSettingChangeLongRunningAbort()
    {
      _testSync.Wait();
      try
      {
        //Arrange
        int eventTriggerCount = 0;
        int expectedCount = WATCHER_COUNT;
        EventHandler evt = (s, e) =>
        {
          var count = Interlocked.Increment(ref eventTriggerCount);
          Thread.Sleep(WAIT_TIMEOUT * 2);
        };
        var watchers = CreateWatchers(expectedCount, evt, false);

        //Act
        bool started = SettingsChangeWatcher.IsRunning;
        FireEvents();
        WaitForEvents(WAIT_TIMEOUT, () => eventTriggerCount >= expectedCount);
        DisposeWatchers(watchers);

        //Assert
        Assert.IsTrue(started, "SettingsChangeWatcher never started");
        Assert.AreEqual(expectedCount, eventTriggerCount);
        Assert.IsTrue(!SettingsChangeWatcher.IsRunning, "SettingsChangeWatcher still running");
      }
      finally
      {
        _testSync.Release();
      }
    }

    [Test, Order(4)]
    public void TestSettingChangeEvents()
    {
      _testSync.Wait();
      try
      {
        //Arrange
        int eventTriggerCount = 0;
        int expectedCount = WATCHER_COUNT;
        EventHandler evt = (s, e) =>
        {
          Interlocked.Increment(ref eventTriggerCount);
        };
        var watchers = CreateWatchers(expectedCount, evt, false);

        //Act
        FireEvents();
        WaitForEvents(WAIT_TIMEOUT, () => eventTriggerCount >= WATCHER_COUNT);

        //Assert
        Assert.AreEqual(expectedCount, eventTriggerCount);

        DisposeWatchers(watchers);
      }
      finally
      {
        _testSync.Release();
      }
    }

    [Test, Order(5)]
    public void TestSettingChangeEventsMany()
    {
      _testSync.Wait();
      try
      {
        //Arrange
        int eventTriggerCount = 0;
        int expectedCount = WATCHER_COUNT * MANY_FACTOR;
        EventHandler evt = (s, e) =>
        {
          Interlocked.Increment(ref eventTriggerCount);
        };
        var watchers = CreateWatchers(expectedCount, evt, false);

        //Act
        FireEvents();
        WaitForEvents(WAIT_TIMEOUT * MANY_FACTOR, () => eventTriggerCount >= WATCHER_COUNT * MANY_FACTOR);

        //Assert
        Assert.AreEqual(expectedCount, eventTriggerCount);

        DisposeWatchers(watchers);
      }
      finally
      {
        _testSync.Release();
      }
    }

    [Test, Order(6)]
    public void TestSettingChangeException()
    {
      _testSync.Wait();
      try
      {
        //Arrange
        int eventTriggerCount = 0;
        int exceptionCount = 0;
        int expectedCount = WATCHER_COUNT;
        EventHandler evt = (s, e) =>
        {
          var count = Interlocked.Increment(ref eventTriggerCount);
          if (count == 1 || count == WATCHER_COUNT / 2)
          {
            Interlocked.Increment(ref exceptionCount);
            throw new Exception("Test");
          }
        };
        var watchers = CreateWatchers(expectedCount, evt, false);

        //Act
        FireEvents();
        WaitForEvents(WAIT_TIMEOUT, () => eventTriggerCount >= WATCHER_COUNT);

        //Assert
        Assert.AreEqual(exceptionCount, 2);
        Assert.AreEqual(expectedCount, eventTriggerCount);

        DisposeWatchers(watchers);
      }
      finally
      {
        _testSync.Release();
      }
    }

    [Test, Order(7)]
    public void TestSettingChangeLongRunning()
    {
      _testSync.Wait();
      try
      {
        //Arrange
        int eventTriggerCount = 0;
        int expectedCount = WATCHER_COUNT;
        EventHandler evt = (s, e) =>
        {
          var count = Interlocked.Increment(ref eventTriggerCount);
          Thread.Sleep(WAIT_TIMEOUT * 2);
        };
        var watchers = CreateWatchers(expectedCount, evt, false);

        //Act
        FireEvents();
        FireEvents(); //Fire events again with all events blocked
        WaitForEvents(WAIT_TIMEOUT, () => eventTriggerCount >= expectedCount);

        //Assert
        Assert.AreEqual(expectedCount, eventTriggerCount);

        DisposeWatchers(watchers);
      }
      finally
      {
        _testSync.Release();
      }
    }


    [Test, Order(10)]
    public void TestSettingChangeOldEvents()
    {
      _testSync.Wait();
      try
      {
        //Arrange
        int eventTriggerCount = 0;
        int expectedCount = WATCHER_COUNT;
        EventHandler evt = (s, e) =>
        {
          Interlocked.Increment(ref eventTriggerCount);
        };
        var watchers = CreateOldWatchers(expectedCount, evt, false);

        //Act
        FireEvents();
        WaitForEvents(WAIT_TIMEOUT, () => eventTriggerCount >= WATCHER_COUNT);

        //Assert
        Assert.AreEqual(expectedCount, eventTriggerCount);

        DisposeOldWatchers(watchers);
      }
      finally
      {
        _testSync.Release();
      }
    }

    [Test, Order(11)]
    public void TestSettingChangeOldException()
    {
      _testSync.Wait();
      try
      {
        //Arrange
        int eventTriggerCount = 0;
        int exceptionCount = 0;
        int expectedCount = WATCHER_COUNT;
        EventHandler evt = (s, e) =>
        {
          var count = Interlocked.Increment(ref eventTriggerCount);
          if (count == 1 || count == WATCHER_COUNT / 2)
          {
            Interlocked.Increment(ref exceptionCount);
            throw new Exception("Test");
          }
        };
        var watchers = CreateOldWatchers(expectedCount, evt, false);

        //Act
        FireEvents();
        WaitForEvents(WAIT_TIMEOUT, () => eventTriggerCount >= WATCHER_COUNT);

        //Assert
        Assert.AreEqual(exceptionCount, 2);
        Assert.AreEqual(expectedCount, eventTriggerCount);

        DisposeOldWatchers(watchers);
      }
      finally
      {
        _testSync.Release();
      }
    }

    [Test, Order(12)]
    public void TestSettingChangeOldLongRunning()
    {
      _testSync.Wait();
      try
      {
        //Arrange
        int eventTriggerCount = 0;
        int expectedCount = WATCHER_COUNT;
        EventHandler evt = (s, e) =>
        {
          var count = Interlocked.Increment(ref eventTriggerCount);
          Thread.Sleep(WAIT_TIMEOUT * 2);
        };
        var watchers = CreateOldWatchers(expectedCount, evt, false);

        //Act
        FireEvents();
        FireEvents(); //Fire events again with all events blocked
        WaitForEvents(WAIT_TIMEOUT, () => eventTriggerCount >= expectedCount);

        //Assert
        Assert.AreEqual(expectedCount, eventTriggerCount);

        DisposeOldWatchers(watchers);
      }
      finally
      {
        _testSync.Release();
      }
    }

    [Test, Order(13)]
    public void TestSettingChangeOldEventsMany()
    {
      _testSync.Wait();
      try
      {
        //Arrange
        int eventTriggerCount = 0;
        int expectedCount = WATCHER_COUNT * MANY_FACTOR;
        EventHandler evt = (s, e) =>
        {
          Interlocked.Increment(ref eventTriggerCount);
        };
        var watchers = CreateOldWatchers(expectedCount, evt, false);

        //Act
        FireEvents();
        WaitForEvents(WAIT_TIMEOUT * MANY_FACTOR, () => eventTriggerCount >= WATCHER_COUNT * MANY_FACTOR);

        //Assert
        Assert.AreEqual(expectedCount, eventTriggerCount);

        DisposeOldWatchers(watchers);
      }
      finally
      {
        _testSync.Release();
      }
    }
  }
}
