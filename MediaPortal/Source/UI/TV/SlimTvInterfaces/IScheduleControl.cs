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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaPortal.Common.Async;
using MediaPortal.Common.Services.ServerCommunication;
using MediaPortal.Plugins.SlimTv.Interfaces.Items;

namespace MediaPortal.Plugins.SlimTv.Interfaces
{
  [Flags]
  public enum RecordingStatus
  {
    None,
    Scheduled = 1,
    SeriesScheduled = 2,
    RuleScheduled = 4,
    Recording = 8,
    RecordingOnce = 16,
    RecordingSeries = 32,
    RecordingManual = 64
  }

  public enum ScheduleRecordingType
  {
    Once,
    Daily,
    Weekly,
    EveryTimeOnThisChannel,
    EveryTimeOnEveryChannel,
    Weekends,
    WorkingDays,
    WeeklyEveryTimeOnThisChannel
  }

  public interface IScheduleControlAsync
  {
    /// <summary>
    /// Creates a single or extended series schedule.
    /// </summary>
    /// <param name="program">Program to record.</param>
    /// <param name="recordingType">Schedule recording type.</param>
    /// <returns>
    /// <see cref="AsyncResult{T}.Success"/> <c>true</c> if successful.
    /// <see cref="AsyncResult{T}.Result"/> Returns the schedule instance.
    /// </returns>
    Task<AsyncResult<ISchedule>> CreateScheduleAsync(IProgram program, ScheduleRecordingType recordingType);

    /// <summary>
    /// Creates a single or extended series schedule.
    /// </summary>
    /// <param name="channel">Channel to record.</param>
    /// <param name="from">Recording time from.</param>
    /// <param name="to">Recording time to.</param>
    /// <param name="recordingType">Schedule recording type.</param>
    /// <param name="schedule">Returns the schedule instance.</param>
    /// <returns>
    /// <see cref="AsyncResult{T}.Success"/> <c>true</c> if successful.
    /// <see cref="AsyncResult{T}.Result"/> Returns the schedule instance.
    /// </returns>
    Task<AsyncResult<ISchedule>> CreateScheduleByTimeAsync(IChannel channel, DateTime from, DateTime to, ScheduleRecordingType recordingType);

    /// <summary>
    /// Creates a single or extended series schedule.
    /// </summary>
    /// <param name="channel">Channel to record.</param>
    /// <param name="title">Title of the recording</param>
    /// <param name="from">Recording time from.</param>
    /// <param name="to">Recording time to.</param>
    /// <param name="recordingType">Schedule recording type.</param>
    /// <param name="schedule">Returns the schedule instance.</param>
    /// <returns>
    /// <see cref="AsyncResult{T}.Success"/> <c>true</c> if successful.
    /// <see cref="AsyncResult{T}.Result"/> Returns the schedule instance.
    /// </returns>
    Task<AsyncResult<ISchedule>> CreateScheduleByTimeAsync(IChannel channel, string title, DateTime from, DateTime to, ScheduleRecordingType recordingType);

    /// <summary>
    /// Creates a single or extended series schedule.
    /// </summary>
    /// <param name="channel">Channel to record.</param>
    /// <param name="title">Title of the recording</param>
    /// <param name="from">Recording time from.</param>
    /// <param name="to">Recording time to.</param>
    /// <param name="recordingType">Schedule recording type.</param>
    /// <param name="priority">Schedule priority</param>
    /// <param name="schedule">Returns the schedule instance.</param>
    /// <param name="preRecordInterval">Prerecording interval</param>
    /// <param name="postRecordInterval">Postrecording interval</param>
    /// <param name="directory">Recording directory</param>
    /// <returns>
    /// <see cref="AsyncResult{T}.Success"/> <c>true</c> if successful.
    /// <see cref="AsyncResult{T}.Result"/> Returns the schedule instance.
    /// </returns>
    Task<AsyncResult<ISchedule>> CreateScheduleDetailedAsync(IChannel channel, string title, DateTime from, DateTime to, ScheduleRecordingType recordingType, int preRecordInterval, int postRecordInterval, string directory, int priority);

    /// <summary>
    /// Edits a given <paramref name="schedule"/>.
    /// </summary>
    /// <param name="channel">Channel to record.</param>
    /// <param name="title">Title of the recording</param>
    /// <param name="recordingType">Schedule recording type.</param>
    /// <param name="preRecordInterval">Prerecording interval</param>
    /// <param name="postRecordInterval">Postrecording interval</param>
    /// <param name="directory">Recording directory</param>
    /// <param name="priority">Schedule priority</param>
    /// <param name="from">Recording time from.</param>
    /// <param name="to">Recording time to.</param>
    /// <param name="schedule">Schedule to edit.</param>
    /// <returns></returns>
    Task<bool> EditScheduleAsync(ISchedule schedule, IChannel channel = null, string title = null, DateTime? from = null, DateTime? to = null, ScheduleRecordingType? recordingType = null, int? preRecordInterval = null, int? postRecordInterval = null, string directory = null, int? priority = null);

    /// <summary>
    /// Deletes a schedule for the given <paramref name="program"/>. If the <paramref name="recordingType"/> is set to <see cref="ScheduleRecordingType.Once"/>,
    /// only the actual program schedule will be removed. If any other series type is used, the full schedule will be removed (including all single schedules).
    /// </summary>
    /// <param name="program">Program to cancel.</param>
    /// <param name="recordingType">Schedule recording type.</param>
    /// <returns><c>true</c> if successful.</returns>
    Task<bool> RemoveScheduleForProgramAsync(IProgram program, ScheduleRecordingType recordingType); // ISchedule schedule ?

    /// <summary>
    /// Deletes a given <paramref name="schedule"/>.
    /// </summary>
    /// <param name="schedule">Schedule to delete.</param>
    /// <returns><c>true</c> if successful.</returns>
    Task<bool> RemoveScheduleAsync(ISchedule schedule);

    /// <summary>
    /// Undo the canceling of a recording
    /// </summary>
    /// <param name="program">Program to uncancel.</param>
    /// <returns></returns>
    Task<bool> UnCancelScheduleAsync(IProgram program);

    /// <summary>
    /// Gets the <paramref name="recordingStatus"/> for the given <paramref name="program"/>.
    /// </summary>
    /// <param name="program">Program.</param>
    /// <returns>
    /// <see cref="AsyncResult{T}.Success"/> <c>true</c> if successful.
    /// <see cref="AsyncResult{T}.Result"/> Recording/Scheduling status.
    /// </returns>
    Task<AsyncResult<RecordingStatus>> GetRecordingStatusAsync(IProgram program);

    /// <summary>
    /// Gets the file or stream name of currently running recording of the given <paramref name="program"/>.
    /// </summary>
    /// <param name="program">Program.</param>
    /// <returns>
    /// <see cref="AsyncResult{T}.Success"/> <c>true</c> if successful.
    /// <see cref="AsyncResult{T}.Result"/> Returns the filename or stream url.
    /// </returns>
    Task<AsyncResult<string>> GetRecordingFileOrStreamAsync(IProgram program);

    /// <summary>
    /// Tries to get a list of programs for the given <paramref name="schedule"/>.
    /// </summary>
    /// <param name="schedule">Schedule</param>
    /// <returns>
    /// <see cref="AsyncResult{T}.Success"/> <c>true</c> if at least one program could be found.
    /// <see cref="AsyncResult{T}.Result"/> programs.
    /// </returns>
    Task<AsyncResult<IList<IProgram>>> GetProgramsForScheduleAsync(ISchedule schedule);

    /// <summary>
    /// Gets the list of all available schedules.
    /// </summary>
    /// <returns>
    /// <see cref="AsyncResult{T}.Success"/> <c>true</c> if at least one schedule could be found.
    /// <see cref="AsyncResult{T}.Result"/> schedules.
    /// </returns>
    Task<AsyncResult<IList<ISchedule>>> GetSchedulesAsync();

    /// <summary>
    /// Gets the list of all canceled programs.
    /// </summary>
    /// <returns>
    /// <see cref="AsyncResult{T}.Success"/> <c>true</c> if at least one canceled schedule could be found.
    /// <see cref="AsyncResult{T}.Result"/> programs.
    /// </returns>
    Task<AsyncResult<IList<IProgram>>> GetCanceledSchedulesAsync();

    /// <summary>
    /// Checks if the given <paramref name="fileName"/> refers to an active recording.
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <returns>
    /// <see cref="AsyncResult{T}.Success"/> <c>true</c> if a schedule could be found.
    /// <see cref="AsyncResult{T}.Result"/> Outputs associated schedule.
    /// </returns>
    Task<AsyncResult<ISchedule>> IsCurrentlyRecordingAsync(string fileName);

    /// <summary>
    /// Gets all conflicting programs for a schedule.
    /// </summary>
    /// <param name="schedule">The schedule to get conflicting programs for.</param>
    /// <returns>
    /// <see cref="AsyncResult{T}.Success"/> <c>true</c> if successful.
    /// <see cref="AsyncResult{T}.Result"/> Returns a list conflicting programs for the schedule.
    /// </returns>
    Task<AsyncResult<IList<IProgram>>> GetConflictsForScheduleAsync(ISchedule schedule);
  }
}
