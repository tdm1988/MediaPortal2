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
using MediaPortal.Common;
using MediaPortal.Common.Settings;
using MediaPortal.Plugins.SlimTv.Client.Settings;
using MediaPortal.UI.Presentation.Workflow;

namespace MediaPortal.Plugins.SlimTv.Client.Models
{
  /// <summary>
  /// ChannelZapModel model provides additional seeking methods for media playback.
  /// </summary>
  public class GuideChannelZapModel : ChannelZapModel
  {
    new public const string MODEL_ID_STR = "D2AEB709-4127-42E3-A9A7-F87F230AA145";

    override public Guid ModelId
    {
      get { return new Guid(MODEL_ID_STR); }
    }

    public GuideChannelZapModel()
    {
    }

    protected override void ZapChannel(int number)
    {
      IWorkflowManager workflowManager = ServiceRegistration.Get<IWorkflowManager>();
      SlimTvMultiChannelGuideModel guide = workflowManager.GetModel(SlimTvMultiChannelGuideModel.MODEL_ID) as SlimTvMultiChannelGuideModel;
      if (guide == null)
        return;
      SlimTvClientSettings settings = ServiceRegistration.Get<ISettingsManager>().Load<SlimTvClientSettings>();
      if (settings.ZapByChannelIndex)
      {
        // Channel index starts by 0, user enters 1 based numbers
        number--;
        guide.GoToChannelIndex(number);
      } else
      {
        guide.GoToChannelNumber(number);
      }
    }
  }
}

