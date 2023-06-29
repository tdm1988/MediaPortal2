#region Copyright (C) 2007-2020 Team MediaPortal

/*
    Copyright (C) 2007-2020 Team MediaPortal
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

using MediaPortal.Common;
using MediaPortal.Common.Commands;
using MediaPortal.Plugins.SlimTv.Client.Helpers;
using MediaPortal.Plugins.SlimTv.Interfaces;
using MediaPortal.Plugins.SlimTv.Interfaces.Items;
using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UI.Presentation.Workflow;
using MediaPortal.UiComponents.Media.MediaLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaPortal.Common.General;
using MediaPortal.Plugins.SlimTv.Client.Models;
using MediaPortal.Utilities;
using MediaPortal.UI.ContentLists;

namespace MediaPortal.Plugins.SlimTv.Client.MediaLists
{
  public class BaseSchedulesMediaListProvider : SlimTvMediaListProviderBase
  {
    private ICollection<Tuple<ISchedule, ProgramProperties>> _currentSchedules = new List<Tuple<ISchedule, ProgramProperties>>();

    protected ParameterlessMethod _showSchedules;

    protected ListItem CreateScheduleItem(ISchedule schedule, ProgramProperties program)
    {
      ListItem item = null;
      DateTime start = schedule.StartTime;
      DateTime end = schedule.EndTime;
      if (program != null)
      {
        item = new ProgramListItem(program)
        {
          Command = new MethodDelegateCommand(_showSchedules),
        };
        item.SetLabel("Name", schedule.Name);
        start = program.StartTime;
        end = program.EndTime;
      }
      if (item == null)
      {
        item = new ListItem("Name", schedule.Name)
        {
          Command = new MethodDelegateCommand(_showSchedules),
        };
      }
      item.SetLabel("ChannelName", program?.ChannelName ?? "");
      item.SetLabel("StartTime", start.FormatProgramStartTime());
      item.SetLabel("EndTime", end.FormatProgramEndTime());
      item.SetLabel("ScheduleType", string.Format("[SlimTvClient.ScheduleRecordingType_{0}]", schedule.RecordingType));
      item.AdditionalProperties[SlimTvClientModelBase.KEY_PROP_SCHEDULE] = schedule;
      return item;
    }

    protected int ChannelAndProgramStartTimeComparison(Tuple<ISchedule, ProgramProperties> p1, Tuple<ISchedule, ProgramProperties> p2)
    {
      double timeDiff = (p1.Item2.StartTime - p2.Item2.StartTime).TotalSeconds;
      if (timeDiff != 0)
        return timeDiff > 0 ? 1 : -1;

      var schedule1 = p1.Item1;
      var schedule2 = p2.Item1;

      // The "Once" schedule should appear first
      if (schedule1.RecordingType == ScheduleRecordingType.Once && schedule2.RecordingType != ScheduleRecordingType.Once)
        return -1;
      if (schedule1.RecordingType != ScheduleRecordingType.Once && schedule2.RecordingType == ScheduleRecordingType.Once)
        return 1;

      int res;
      if (schedule1.RecordingType == ScheduleRecordingType.Once && schedule2.RecordingType == ScheduleRecordingType.Once)
      {
        res = DateTime.Compare(schedule1.StartTime, schedule2.StartTime);
        if (res != 0)
          return res;
      }

      res = String.Compare(schedule1.Name, schedule2.Name, StringComparison.InvariantCultureIgnoreCase);
      if (res != 0)
        return res;

      string channel1Name = p1.Item2 != null ? p1.Item2.ChannelName : string.Empty;
      string channel2Name = p2.Item2 != null ? p2.Item2.ChannelName : string.Empty;
      return String.Compare(channel1Name, channel2Name, StringComparison.InvariantCultureIgnoreCase);
    }

    public override async Task<bool> UpdateItemsAsync(int maxItems, UpdateReason updateReason, ICollection<object> updatedObjects)
    {
      if (!TryInitTvHandler())
        return false;
      var tvHandlerScheduleControl = _tvHandler.ScheduleControl;
      if (tvHandlerScheduleControl == null)
        return false;

      if (!updateReason.HasFlag(UpdateReason.Forced) && !updateReason.HasFlag(UpdateReason.PeriodicMinute))
        return true;

      var scheduleResult = await tvHandlerScheduleControl.GetSchedulesAsync();
      if (!scheduleResult.Success)
        return false;

      var schedules = scheduleResult.Result;
      var scheduleSortList = new List<Tuple<ISchedule, ProgramProperties>>();
      var channels = new Dictionary<int, MediaType>();
      foreach (ISchedule schedule in schedules)
      {
        if (schedule.ChannelId > 0 && !channels.ContainsKey(schedule.ChannelId))
        {
          var channelResult = _tvHandler.ChannelAndGroupInfo?.GetChannelAsync(schedule.ChannelId);
          if (channelResult != null && channelResult.Result.Success)
            channels[schedule.ChannelId] = channelResult.Result.Result.MediaType;
        }

        if (channels.ContainsKey(schedule.ChannelId) && _mediaType == MediaType.TV && channels[schedule.ChannelId] != MediaType.TV)
          continue;
        if (channels.ContainsKey(schedule.ChannelId) && _mediaType == MediaType.Radio && channels[schedule.ChannelId] != MediaType.Radio)
          continue;

        var programResult = await tvHandlerScheduleControl.GetProgramsForScheduleAsync(schedule);
        if (!programResult.Success || programResult.Result.Count == 0)
          continue;
        ProgramProperties programProperties = new ProgramProperties();
        programProperties.SetProgram(programResult.Result.OrderBy(p => p.StartTime).First());
        scheduleSortList.Add(new Tuple<ISchedule, ProgramProperties>(schedule, programProperties));
      }
      scheduleSortList.Sort(ChannelAndProgramStartTimeComparison);

      var scheduleList = new List<Tuple<ISchedule, ProgramProperties>>(scheduleSortList.Take(maxItems));

      if (_currentSchedules.Select(s => s.Item1.ScheduleId).SequenceEqual(scheduleList.Select(s => s.Item1.ScheduleId)))
        return true;

      ListItem[] items = scheduleList.Select(s => CreateScheduleItem(s.Item1, s.Item2)).ToArray();
      lock (_allItems.SyncRoot)
      {
        _currentSchedules = scheduleList;
        _allItems.Clear();
        CollectionUtils.AddAll(_allItems, items);
      }
      _allItems.FireChange();
      return true;
    }
  }
}
