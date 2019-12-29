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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaPortal.Common.Logging;
using MediaPortal.Common.Messaging;
using MediaPortal.Common.Settings;
using MediaPortal.Common.UserManagement;

namespace MediaPortal.Common.Services.Settings
{
  public static class SettingsChangeWatcher
  {
    private const int MAX_CONCURRENT_CALLS = 1;
    private const int LONG_RUNNING_WARNING_THRESHOLD_MSEC = 30000;

    private static readonly object SYNC_OBJECT = new object();
    private static readonly List<InvocationHandler> INVOCATION_LIST = new List<InvocationHandler>();
    private static readonly TaskFactory INVOCATION_FACTORY = new TaskFactory(CancellationToken.None, TaskCreationOptions.LongRunning, TaskContinuationOptions.DenyChildAttach, System.Threading.Tasks.TaskScheduler.Default);

    private static AsynchronousMessageQueue _messageQueue;
    private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public static bool IsRunning => _messageQueue != null;
    public static int InvocationCount => INVOCATION_LIST.Count;

    private class InvocationHandler
    {
      public InvocationHandler(MessageReceivedHandler handler)
      {
        Handler = handler;
      }
      public SemaphoreSlim HandlerReady { get; } = new SemaphoreSlim(MAX_CONCURRENT_CALLS, MAX_CONCURRENT_CALLS);
      public MessageReceivedHandler Handler { get; }
    }

    internal static event MessageReceivedHandler MessageReceived
    {
      add
      {
        lock (SYNC_OBJECT)
        {
          StartSettingWatcher();
          INVOCATION_LIST.Add(new InvocationHandler(value));
        }
      }
      remove
      {
        lock (SYNC_OBJECT)
        {
          var first = INVOCATION_LIST.FirstOrDefault(i => i.Handler == value);
          if (first != null)
            INVOCATION_LIST.Remove(first);
          if (INVOCATION_LIST.Count == 0)
            StopSettingWatcher();
        }
      }
    }

    private static void OnMessageReceived(AsynchronousMessageQueue queue, SystemMessage message)
    {
      if (message.ChannelName == UserMessaging.CHANNEL || message.ChannelName == SettingsManagerMessaging.CHANNEL)
      {
        lock (SYNC_OBJECT)
        {
          foreach (var invocation in INVOCATION_LIST)
          {
            INVOCATION_FACTORY.StartNew(() =>
            {
              ProcessInvocation(invocation, queue, message);
            }, _cancellationTokenSource.Token);
          }
        }
      }
    }

    private static async void ProcessInvocation(InvocationHandler invocation, AsynchronousMessageQueue queue, SystemMessage message)
    {
      if (MAX_CONCURRENT_CALLS == 0 || await invocation.HandlerReady.WaitAsync(LONG_RUNNING_WARNING_THRESHOLD_MSEC, _cancellationTokenSource.Token))
      {
        try
        {
          invocation.Handler.Invoke(queue, message);
        }
        catch (Exception e)
        {
          ServiceRegistration.Get<ILogger>().Error("Unhandled exception in message SettingChangedWatcher handler when handling a message of type '{0}'",
              e, message.MessageType);
        }
        if (MAX_CONCURRENT_CALLS > 0)
          invocation.HandlerReady.Release();
      }
      else if (LONG_RUNNING_WARNING_THRESHOLD_MSEC > 0)
      {
        ServiceRegistration.Get<ILogger>().Warn("Event handler running for more than {1} milliseconds detected in SettingChangedWatcher for messages of type '{0}'",
          message.MessageType, LONG_RUNNING_WARNING_THRESHOLD_MSEC);
      }
    }

    private static void StartSettingWatcher()
    {
      if (_messageQueue == null)
      {
        _cancellationTokenSource = new CancellationTokenSource();
        _messageQueue = new AsynchronousMessageQueue(nameof(SettingsChangeWatcher), new[] { SettingsManagerMessaging.CHANNEL, UserMessaging.CHANNEL });
        _messageQueue.MessageReceived += OnMessageReceived;
        _messageQueue.Start();
      }
    }

    private static void StopSettingWatcher()
    {
      _cancellationTokenSource.Cancel();
      _messageQueue?.Shutdown();
      _messageQueue?.Dispose();
      _messageQueue = null;
    }
  }

  /// <summary>
  /// <see cref="SettingsChangeWatcher{T}"/> provides a generic watcher for settings that automatically refreshes
  /// the <see cref="Settings"/> if it gets notified using <see cref="SettingsManagerMessaging"/>.
  /// </summary>
  /// <typeparam name="T">Settings type.</typeparam>
  public class SettingsChangeWatcher<T> : IDisposable
    where T : class
  {
    #region Fields

    protected T _settings;

    private readonly bool _updateOnUserChange;
    private readonly object _syncObject = new object();

    #endregion

    public SettingsChangeWatcher()
      : this(false) { }

    public SettingsChangeWatcher(bool updateOnUserChange)
    {
      _updateOnUserChange = updateOnUserChange;

      SettingsChangeWatcher.MessageReceived += OnMessageReceived;
    }

    /// <summary>
    /// Informs listeners that the current setting has been changed.
    /// </summary>
    public EventHandler SettingsChanged;

    /// <summary>
    /// Gets the current setting. This property will automatically return new values after changes.
    /// </summary>
    public T Settings
    {
      get
      {
        return _settings ?? (_settings = ServiceRegistration.Get<ISettingsManager>().Load<T>());
      }
    }

    /// <summary>
    /// Refreshes the settings asynchronously.
    /// </summary>
    public void RefreshAsync()
    {
      Task.Run(() => Refresh());
    }

    /// <summary>
    /// Forces the refresh of the settings.
    /// </summary>
    public void Refresh()
    {
      _settings = null;
      if (SettingsChanged != null)
        SettingsChanged(this, EventArgs.Empty);
    }

    #region Message handling

    protected void OnMessageReceived(AsynchronousMessageQueue queue, SystemMessage message)
    {
      if (_updateOnUserChange && message.ChannelName == UserMessaging.CHANNEL)
      {
        UserMessaging.MessageType messageType = (UserMessaging.MessageType)message.MessageType;
        switch (messageType)
        {
          // If the user has been changed, refresh the settings in every case.
          case UserMessaging.MessageType.UserChanged:
            RefreshAsync();
            break;
        }
      }
      if (message.ChannelName == SettingsManagerMessaging.CHANNEL)
      {
        SettingsManagerMessaging.MessageType messageType = (SettingsManagerMessaging.MessageType)message.MessageType;
        switch (messageType)
        {
          case SettingsManagerMessaging.MessageType.SettingsChanged:
            Type settingsType = (Type)message.MessageData[SettingsManagerMessaging.SETTINGSTYPE];
            // If our contained Type has been changed, clear the cache and reload it
            if (typeof(T) == settingsType)
              Refresh();
            break;
        }
      }
    }

    #endregion

    #region IDisposable members

    public void Dispose()
    {
      SettingsChangeWatcher.MessageReceived -= OnMessageReceived;
    }

    #endregion
  }

  public class SettingsChangeWatcherOld<T> : IDisposable
    where T : class
  {
    #region Fields

    protected AsynchronousMessageQueue _messageQueue;
    protected T _settings;

    #endregion

    public SettingsChangeWatcherOld()
      : this(false) { }

    public SettingsChangeWatcherOld(bool updateOnUserChange)
    {
      List<string> channels = new List<string> { SettingsManagerMessaging.CHANNEL };
      if (updateOnUserChange)
        channels.Add(UserMessaging.CHANNEL);
      _messageQueue = new AsynchronousMessageQueue(this, channels);
      _messageQueue.MessageReceived += OnMessageReceived;
      _messageQueue.Start();
    }

    /// <summary>
    /// Informs listeners that the current setting has been changed.
    /// </summary>
    public EventHandler SettingsChanged;

    /// <summary>
    /// Gets the current setting. This property will automatically return new values after changes.
    /// </summary>
    public T Settings
    {
      get
      {
        return _settings ?? (_settings = ServiceRegistration.Get<ISettingsManager>().Load<T>());
      }
    }

    /// <summary>
    /// Refreshes the settings asynchronously.
    /// </summary>
    public void RefreshAsync()
    {
      Task.Run(() => Refresh());
    }

    /// <summary>
    /// Forces the refresh of the settings.
    /// </summary>
    public void Refresh()
    {
      _settings = null;
      if (SettingsChanged != null)
        SettingsChanged(this, EventArgs.Empty);
    }

    #region Message handling

    protected void OnMessageReceived(AsynchronousMessageQueue queue, SystemMessage message)
    {
      if (message.ChannelName == UserMessaging.CHANNEL)
      {
        UserMessaging.MessageType messageType = (UserMessaging.MessageType)message.MessageType;
        switch (messageType)
        {
          // If the user has been changed, refresh the settings in every case.
          case UserMessaging.MessageType.UserChanged:
            RefreshAsync();
            break;
        }
      }
      if (message.ChannelName == SettingsManagerMessaging.CHANNEL)
      {
        SettingsManagerMessaging.MessageType messageType = (SettingsManagerMessaging.MessageType)message.MessageType;
        switch (messageType)
        {
          case SettingsManagerMessaging.MessageType.SettingsChanged:
            Type settingsType = (Type)message.MessageData[SettingsManagerMessaging.SETTINGSTYPE];
            // If our contained Type has been changed, clear the cache and reload it
            if (typeof(T) == settingsType)
              Refresh();
            break;
        }
      }
    }

    #endregion

    #region IDisposable members

    public void Dispose()
    {
      _messageQueue.MessageReceived -= OnMessageReceived;
      _messageQueue.Shutdown();
    }

    #endregion
  }
}
