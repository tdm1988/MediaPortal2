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
using System.Threading.Tasks;
using MediaPortal.Common;
using MediaPortal.Common.Commands;
using MediaPortal.Common.General;
using MediaPortal.Common.Localization;
using MediaPortal.Common.Logging;
using MediaPortal.Common.PluginManager;
using MediaPortal.Common.PluginManager.Exceptions;
using MediaPortal.Plugins.SlimTv.Client.Helpers;
using MediaPortal.Plugins.SlimTv.Interfaces;
using MediaPortal.Plugins.SlimTv.Interfaces.Extensions;
using MediaPortal.Plugins.SlimTv.Interfaces.Items;
using MediaPortal.Plugins.SlimTv.Client.Messaging;
using MediaPortal.UiComponents.Media.General;
using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UI.Presentation.Models;
using MediaPortal.UI.Presentation.Screens;
using MediaPortal.UI.Presentation.Workflow;

namespace MediaPortal.Plugins.SlimTv.Client.Models
{
  /// <summary>
  /// <see cref="SlimTvModelBase"/> provides basic features for all derived models, i.e. channel group and channel selection.
  /// </summary>
  public abstract class SlimTvModelBase : BaseTimerControlledModel, IWorkflowModel
  {
    #region Protected fields

    protected ITvHandler _tvHandler;
    protected ItemsList _channelGroupList = new ItemsList();
    protected ItemsList _singlechannelList = new ItemsList();
    protected IPluginItemStateTracker _slimTvExtensionsPluginItemStateTracker;
    protected Dictionary<Guid, TvExtension> _programExtensions;

    protected IList<IProgram> _programs;
    protected bool _isInitialized;

    protected AbstractProperty _dialogHeaderProperty = null;
    protected readonly ItemsList _dialogActionsList = new ItemsList();

    public struct TvExtension
    {
      public string Caption;
      public IProgramAction Extension;
    }

    #endregion

    #region Constructor

    protected SlimTvModelBase()
      : this(5000)
    { }

    protected SlimTvModelBase(long updateInterval)
      : base(true, updateInterval)
    {
    }

    #endregion

    #region GUI properties and methods

    /// <summary>
    /// Gets the list of all available channel groups.
    /// </summary>
    public ItemsList ChannelGroupList
    {
      get { return _channelGroupList; }
    }

    /// <summary>
    /// Exposes the list of channels in current group.
    /// </summary>
    public ItemsList SingleChannelList
    {
      get { return _singlechannelList; }
    }

    /// <summary>
    /// Gets the currently selected channel group, or <c>null</c> if not initialized.
    /// </summary>
    public IChannelGroup CurrentChannelGroup
    {
      get
      {
        return ChannelContext.Instance.ChannelGroups.Current;
      }
    }

    /// <summary>
    /// Skips group index to next one.
    /// </summary>
    public void NextGroup()
    {
      ChannelContext.Instance.ChannelGroups.MoveNext();
      SetGroup();
    }

    /// <summary>
    /// Skips group index to previous one.
    /// </summary>
    public void PrevGroup()
    {
      ChannelContext.Instance.ChannelGroups.MovePrevious();
      SetGroup();
    }

    protected virtual void SetGroup()
    {
    }

    /// <summary>
    /// Opens the group selection dialog.
    /// </summary>
    public void SelectGroup()
    {
      ChannelGroupList.Clear();
      for (int index = 0; index < ChannelContext.Instance.ChannelGroups.Count; index++)
      {
        var channelGroup = ChannelContext.Instance.ChannelGroups[index];
        int groupIndex = index;
        ListItem channel = new ListItem(Consts.KEY_NAME, channelGroup.Name)
        {
          Command = new MethodDelegateCommand(() => ChannelContext.Instance.ChannelGroups.SetIndex(groupIndex)),
          Selected = groupIndex == ChannelContext.Instance.ChannelGroups.CurrentIndex
        };
        ChannelGroupList.Add(channel);
      }
      ChannelGroupList.FireChange();
      ServiceRegistration.Get<IScreenManager>().ShowDialog("DialogChooseGroup");
    }

    public void SelectChannel()
    {
      SingleChannelList.Clear();
      for (int index = 0; index < ChannelContext.Instance.Channels.Count; index++)
      {
        var channel = ChannelContext.Instance.Channels[index];
        int channelIndex = index;
        ListItem channelItem = new ListItem(Consts.KEY_NAME, channel.Name)
        {
          Command = new MethodDelegateCommand(() => ChannelContext.Instance.Channels.SetIndex(channelIndex)),
          Selected = channelIndex == ChannelContext.Instance.Channels.CurrentIndex
        };
        SingleChannelList.Add(channelItem);
      }
      SingleChannelList.FireChange();
      ServiceRegistration.Get<IScreenManager>().ShowDialog("DialogChooseChannel");
    }

    /// <summary>
    /// Gets the currently selected channel, or <c>null</c> if not initilalized.
    /// </summary>
    public IChannel CurrentChannel
    {
      get
      {
        return ChannelContext.Instance.Channels.Current;
      }
    }

    /// <summary>
    /// Skips group index to next one.
    /// </summary>
    public void NextChannel()
    {
      ChannelContext.Instance.Channels.MoveNext();
      SetChannel();
    }

    /// <summary>
    /// Skips group index to previous one.
    /// </summary>
    public void PrevChannel()
    {
      ChannelContext.Instance.Channels.MovePrevious();
      SetChannel();
    }

    /// <summary>
    /// Sets the current channel based on <see cref="ChannelContext"/>
    /// </summary>
    protected virtual void SetChannel()
    {
    }

    public static async Task TuneChannel(IChannel channel)
    {
      IWorkflowManager workflowManager = ServiceRegistration.Get<IWorkflowManager>();
      SlimTvClientModel model = workflowManager.GetModel(SlimTvClientModel.MODEL_ID) as SlimTvClientModel;
      if (model != null)
      {
        await model.Tune(channel);
        // Always switch to fullscreen
        workflowManager.NavigatePush(Consts.WF_STATE_ID_FULLSCREEN_VIDEO);
      }
    }

    /// <summary>
    /// Exposes the list of available series recording types or other user choices.
    /// </summary>
    public ItemsList DialogActionsList
    {
      get { return _dialogActionsList; }
    }

    /// <summary>
    /// Exposes the user dialog header.
    /// </summary>
    public string DialogHeader
    {
      get { return (string)_dialogHeaderProperty.GetValue(); }
      set { _dialogHeaderProperty.SetValue(value); }
    }

    /// <summary>
    /// Exposes the user dialog header.
    /// </summary>
    public AbstractProperty DialogHeaderProperty
    {
      get { return _dialogHeaderProperty; }
    }

    public void ExecProgramAction(ListItem item)
    {
      if (item == null)
        return;
      if (item.Command != null)
        item.Command.Execute();
    }

    #endregion

    #region Members

    #region Inits and Updates

    protected virtual void InitModel()
    {
      if (_tvHandler == null)
      {
        ITvHandler tvHandler = ServiceRegistration.Get<ITvHandler>();
        tvHandler.Initialize();
        if (tvHandler.ChannelAndGroupInfo == null)
          return;
        _tvHandler = tvHandler;
      }

      _dialogHeaderProperty = new WProperty(typeof(string), string.Empty);
    }

    protected void FillChannelGroupList()
    {
      _channelGroupList.Clear();
      for (int idx = 0; idx < ChannelContext.Instance.ChannelGroups.Count; idx++)
      {
        IChannelGroup group = ChannelContext.Instance.ChannelGroups[idx];
        ListItem channelGroupItem = new ListItem(Consts.KEY_NAME, group.Name)
        {
          Command = new MethodDelegateCommand(() =>
          {
            if (ChannelContext.Instance.ChannelGroups.MoveTo(g => g == group))
            {
              //SetGroup();
            }
          }),
          Selected = group == ChannelContext.Instance.ChannelGroups.Current
        };
        _channelGroupList.Add(channelGroupItem);
      }
      _channelGroupList.FireChange();
    }

    protected void GetCurrentChannelGroup()
    {
      if (_tvHandler.ChannelAndGroupInfo != null && _tvHandler.ChannelAndGroupInfo.SelectedChannelGroupId != 0)
        ChannelContext.Instance.ChannelGroups.MoveTo(group => group.ChannelGroupId == _tvHandler.ChannelAndGroupInfo.SelectedChannelGroupId);
    }

    protected void GetCurrentChannel()
    {
      if (_tvHandler.ChannelAndGroupInfo != null && _tvHandler.ChannelAndGroupInfo.SelectedChannelId != 0)
        ChannelContext.Instance.Channels.MoveTo(channel => channel.ChannelId == _tvHandler.ChannelAndGroupInfo.SelectedChannelId);
    }

    protected void SetCurrentChannelGroup()
    {
      if (_tvHandler.ChannelAndGroupInfo != null && CurrentChannelGroup != null)
        _tvHandler.ChannelAndGroupInfo.SelectedChannelGroupId = CurrentChannelGroup.ChannelGroupId;
    }

    protected void SetCurrentChannel()
    {
      if (_tvHandler.ChannelAndGroupInfo != null && CurrentChannel != null)
        _tvHandler.ChannelAndGroupInfo.SelectedChannelId = CurrentChannel.ChannelId;
    }

    #endregion

    #region Recording related

    /// <summary>
    /// Add series recording options for program
    /// </summary>
    protected void AddRecordingOptions(ItemsList items, IProgram program, RecordingStatus status)
    {
      bool isRecording = status.HasFlag(RecordingStatus.Recording);
      if (program != null)
      {
        bool isRunning = DateTime.Now >= program.StartTime && DateTime.Now <= program.EndTime;
        ILocalization localization = ServiceRegistration.Get<ILocalization>();
        if (status.HasFlag(RecordingStatus.SeriesScheduled))
        {
          if(isRecording || program.EndTime > DateTime.Now)
            items.Add(new ListItem(Consts.KEY_NAME, localization.ToString(isRecording ? "[SlimTvClient.StopCurrentRecording]"
              : "[SlimTvClient.DeleteSingle]", program.Title))
            {
              Command = new AsyncMethodDelegateCommand(() => CreateOrDeleteSchedule(program, ScheduleRecordingType.Once))
            });
          items.Add(new ListItem(Consts.KEY_NAME, "[SlimTvClient.DeleteFullSchedule]")
          {
            Command = new AsyncMethodDelegateCommand(() => CreateOrDeleteSchedule(program, ScheduleRecordingType.EveryTimeOnEveryChannel))
          });
        }
        else
        {
          string prompt = null;
          if (isRecording)
            prompt = "[SlimTvClient.StopCurrentRecording]";
          else if (isRunning)
            prompt = "[SlimTvClient.RecordCurrentProgram]";
          else if (status.HasFlag(RecordingStatus.Scheduled))
            prompt = "[SlimTvClient.DeleteSchedule]";
          else if (program.EndTime > DateTime.Now)
            prompt = "[SlimTvClient.CreateSchedule]";
          if(prompt != null)
            items.Add(new ListItem(Consts.KEY_NAME, localization.ToString(prompt, program.Title))
            {
              Command = new AsyncMethodDelegateCommand(() => CreateOrDeleteSchedule(program))
            });
          items.Add(
            new ListItem(Consts.KEY_NAME, "[SlimTvClient.RecordSeries]")
            {
              Command = new MethodDelegateCommand(() => SlimTvExtScheduleModel.RecordSeries(program))
            });
        }
      }
      if (_programExtensions == null)
        BuildExtensions();
      ILocalization loc = ServiceRegistration.Get<ILocalization>();
      foreach (KeyValuePair<Guid, TvExtension> programExtension in _programExtensions)
      {
        TvExtension extension = programExtension.Value;
        // First check if this extension applies for the selected program
        if (!extension.Extension.IsAvailable(program))
          continue;

        items.Add(
            new ListItem(Consts.KEY_NAME, loc.ToString(extension.Caption))
            {
              Command = new MethodDelegateCommand(() => extension.Extension.ProgramAction(program))
            });
      }
    }

    protected void BuildExtensions()
    {
      _programExtensions = new Dictionary<Guid, TvExtension>();
      _slimTvExtensionsPluginItemStateTracker = new FixedItemStateTracker("SlimTvHandler - Extension registration");

      IPluginManager pluginManager = ServiceRegistration.Get<IPluginManager>();
      foreach (PluginItemMetadata itemMetadata in pluginManager.GetAllPluginItemMetadata(SlimTvExtensionBuilder.SLIMTVEXTENSIONPATH))
      {
        try
        {
          SlimTvProgramExtension slimTvProgramExtension = pluginManager.RequestPluginItem<SlimTvProgramExtension>(
              SlimTvExtensionBuilder.SLIMTVEXTENSIONPATH, itemMetadata.Id, _slimTvExtensionsPluginItemStateTracker);
          if (slimTvProgramExtension == null)
            ServiceRegistration.Get<ILogger>().Warn("Could not instantiate SlimTv extension with id '{0}'", itemMetadata.Id);
          else
          {
            Type extensionClass = slimTvProgramExtension.ExtensionClass;
            if (extensionClass == null)
              throw new PluginInvalidStateException("Could not find class type for extension {0}", slimTvProgramExtension.Caption);
            IProgramAction action = Activator.CreateInstance(extensionClass) as IProgramAction;
            if (action == null)
              throw new PluginInvalidStateException("Could not create IProgramAction instance of class {0}", extensionClass);
            _programExtensions[slimTvProgramExtension.Id] = new TvExtension { Caption = slimTvProgramExtension.Caption, Extension = action };
          }
        } catch (PluginInvalidStateException e)
        {
          ServiceRegistration.Get<ILogger>().Warn("Cannot add SlimTv extension with id '{0}'", e, itemMetadata.Id);
        }
      }
    }

    protected virtual async Task<RecordingStatus?> CreateOrDeleteSchedule(IProgram program, ScheduleRecordingType recordingType = ScheduleRecordingType.Once)
    {
      IScheduleControlAsync scheduleControl = _tvHandler.ScheduleControl;
      RecordingStatus? newStatus = null;
      if (scheduleControl != null)
      {
        RecordingStatus? recordingStatus = await GetRecordingStatusAsync(program);
        if (recordingStatus.HasValue && (recordingStatus.Value.HasFlag(RecordingStatus.Scheduled) || recordingStatus.Value.HasFlag(RecordingStatus.SeriesScheduled)))
        {
          if (await scheduleControl.RemoveScheduleForProgramAsync(program, recordingType))
            newStatus = RecordingStatus.None;
        }
        else
        {
          var result = await scheduleControl.CreateScheduleAsync(program, recordingType);
          if (result.Success)
            newStatus = recordingType == ScheduleRecordingType.Once ? RecordingStatus.Scheduled : RecordingStatus.SeriesScheduled;
        }
        if (newStatus != null)
        {
          UpdateRecordingStatus(program, (RecordingStatus)newStatus, recordingType);
          SlimTvClientMessaging.SendSlimTvProgramChangedMessage(program);
        }
      }
      return newStatus;
    }

    protected virtual async Task<RecordingStatus?> CreateSchedule(IProgram program, ScheduleRecordingType recordingType = ScheduleRecordingType.Once)
    {
      IScheduleControlAsync scheduleControl = _tvHandler.ScheduleControl;
      RecordingStatus? newStatus = null;
      if (scheduleControl != null)
      {
        RecordingStatus? recordingStatus = await GetRecordingStatusAsync(program);
        if (recordingStatus.HasValue && (recordingStatus.Value.HasFlag(RecordingStatus.Scheduled) || recordingStatus.Value.HasFlag(RecordingStatus.SeriesScheduled)))
        {
          // Delete any existing schedule
          if (await scheduleControl.RemoveScheduleForProgramAsync(program, recordingStatus.Value.HasFlag(RecordingStatus.SeriesScheduled) ? ScheduleRecordingType.EveryTimeOnEveryChannel : ScheduleRecordingType.Once))
            newStatus = RecordingStatus.None;
        }
        // Now create new schedule
        var result = await scheduleControl.CreateScheduleAsync(program, recordingType);
        if (result.Success)
        { 
          newStatus = recordingType == ScheduleRecordingType.Once ? RecordingStatus.Scheduled : RecordingStatus.SeriesScheduled;
          UpdateRecordingStatus(program, (RecordingStatus)newStatus, recordingType);
          SlimTvClientMessaging.SendSlimTvProgramChangedMessage(program);
        }
      }
      return newStatus;
    }

    protected virtual bool UpdateRecordingStatus(IProgram program, RecordingStatus status, ScheduleRecordingType type)
    {
      IProgramRecordingStatus recordingStatus = program as IProgramRecordingStatus;
      if (recordingStatus == null || recordingStatus.RecordingStatus == status)
        return false;
      RecordingStatus oldStatus = recordingStatus.RecordingStatus;
      recordingStatus.RecordingStatus = (RecordingStatus)status;
      OnRecordingStatusChanged(program, oldStatus, status, type);
      return true;
    }

    protected virtual void OnRecordingStatusChanged(IProgram program, RecordingStatus oldStatus, RecordingStatus newStatus, ScheduleRecordingType type)
    {
    }

    protected virtual async Task<RecordingStatus?> GetRecordingStatusAsync(IProgram program)
    {
      IScheduleControlAsync scheduleControl = _tvHandler.ScheduleControl;
      if (scheduleControl == null)
        return null;

      var result = await scheduleControl.GetRecordingStatusAsync(program);
      if (result.Success)
        return result.Result;
      return null;
    }

    #endregion

    #region Channel, groups and programs

    //protected abstract void UpdateCurrentChannel();

    //protected virtual void UpdateChannels()
    //{
    //  IChannelGroup group = CurrentChannelGroup;
    //  if (group != null)
    //  {
    //    //IList<IChannel> channels;
    //    //if (_tvHandler.ChannelAndGroupInfo.GetChannels(group, out channels))
    //    //{
    //    //  ChannelContext.Instance.Channels.Clear();
    //    //  ChannelContext.Instance.Channels.AddRange(channels);
    //    //}

    //    //// Now current channel group / channel is only set for tuning
    //    //// SetCurrentChannelGroup();
    //    //UpdateCurrentChannel();
    //    UpdatePrograms();
    //  }
    //}

    #endregion

    #endregion

    #region IWorkflowModel implementation

    public abstract Guid ModelId { get; }

    public virtual bool CanEnterState(NavigationContext oldContext, NavigationContext newContext)
    {
      InitModel();
      return _tvHandler != null;
    }

    public virtual void EnterModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
      Attach();
    }

    public virtual void ExitModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
      Detach();
    }

    public virtual void ChangeModelContext(NavigationContext oldContext, NavigationContext newContext, bool push)
    {
    }

    public virtual void Deactivate(NavigationContext oldContext, NavigationContext newContext)
    {
      Detach();
    }

    public virtual void Reactivate(NavigationContext oldContext, NavigationContext newContext)
    {
      InitModel();
      Attach();
    }

    private void Attach()
    {
      ChannelContext.Instance.ChannelGroups.OnListChanged += OnChannelGroupsChanged;
      ChannelContext.Instance.ChannelGroups.OnCurrentChanged += OnCurrentGroupChanged;
      ChannelContext.Instance.Channels.OnListChanged += OnChannelsChanged;
      ChannelContext.Instance.Channels.OnCurrentChanged += OnCurrentChannelChanged;
    }

    private void Detach()
    {
      ChannelContext.Instance.ChannelGroups.OnListChanged -= OnChannelGroupsChanged;
      ChannelContext.Instance.ChannelGroups.OnCurrentChanged -= OnCurrentGroupChanged;
      ChannelContext.Instance.Channels.OnListChanged -= OnChannelsChanged;
      ChannelContext.Instance.Channels.OnCurrentChanged -= OnCurrentChannelChanged;
    }

    protected virtual void OnChannelGroupsChanged(object sender, EventArgs e)
    {
      FillChannelGroupList();
    }

    protected virtual void OnChannelsChanged(object sender, EventArgs e)
    {
    }

    protected virtual void OnCurrentGroupChanged(int oldindex, int newindex)
    {
      SetCurrentChannelGroup();
    }

    protected virtual void OnCurrentChannelChanged(int oldindex, int newindex)
    {
    }

    public virtual void UpdateMenuActions(NavigationContext context, IDictionary<Guid, WorkflowAction> actions)
    {
    }

    public ScreenUpdateMode UpdateScreen(NavigationContext context, ref string screen)
    {
      return ScreenUpdateMode.AutoWorkflowManager;
    }

    #endregion
  }
}
