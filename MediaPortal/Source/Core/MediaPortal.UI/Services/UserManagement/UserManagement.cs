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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaPortal.Common;
using MediaPortal.Common.Async;
using MediaPortal.Common.Services.ServerCommunication;
using MediaPortal.Common.Settings;
using MediaPortal.Common.SystemResolver;
using MediaPortal.Common.UserManagement;
using MediaPortal.Common.UserProfileDataManagement;
using MediaPortal.UI.General;
using MediaPortal.UI.ServerCommunication;

namespace MediaPortal.UI.Services.UserManagement
{
  public class UserManagement : IUserManagement
  {
    public static UserProfile UNKNOWN_USER = new UserProfile(Guid.Empty, "Unknown");

    private UserProfile _currentUser = null;
    private bool _applyRestrictions = false;
    private ICollection<string> _restrictionGroups = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
    private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    private UserProfile _defaultUser = null;

    public bool IsValidUser
    {
      get { return _currentUser != UNKNOWN_USER; }
    }

    public UserProfile CurrentUser
    {
      get
      {
        if (_currentUser == null || !IsValidUser)
          _currentUser = GetOrCreateDefaultUser().TryWait();
        _currentUser = _currentUser ?? UNKNOWN_USER;
        return _currentUser;
      }
      set
      {
        var newUser = value ?? UNKNOWN_USER;
        bool changed = !Equals(_currentUser, newUser);
        _currentUser = newUser;
        if (changed && IsValidUser)
        {
          // Set new user name to allow overriding settings, but only if explicit user management is enabled
          var settingsManager = ServiceRegistration.Get<ISettingsManager>();
          if (settingsManager.Load<UserSettings>().EnableUserLogin)
            settingsManager.ChangeUserContext(_currentUser?.ProfileId.ToString());

          UserMessaging.SendUserMessage(UserMessaging.MessageType.UserChanged);
        }
      }
    }

    public IUserProfileDataManagement UserProfileDataManagement
    {
      get
      {
        UPnPClientControlPoint controlPoint = ServiceRegistration.Get<IServerConnectionManager>().ControlPoint;
        return controlPoint != null ? controlPoint.UserProfileDataManagementService : null;
      }
    }

    public void RegisterRestrictionGroup(string restrictionGroup)
    {
      if (!string.IsNullOrWhiteSpace(restrictionGroup))
        foreach (var group in restrictionGroup.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
        {
          _restrictionGroups.Add(group);
        }
    }

    public ICollection<string> RestrictionGroups
    {
      get { return _restrictionGroups; }
    }

    public bool CheckUserAccess(IUserRestriction restrictedElement)
    {
      if (!IsValidUser || !CurrentUser.EnableRestrictionGroups || string.IsNullOrEmpty(restrictedElement.RestrictionGroup))
        return true;

      foreach (var group in restrictedElement.RestrictionGroup.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
        if (CurrentUser.RestrictionGroups.Contains(group))
          return true;
      return false;
    }

    public bool ApplyUserRestriction
    {
      get { return _applyRestrictions; }
      set { _applyRestrictions = value; }
    }

    public async Task<UserProfile> GetOrCreateDefaultUser()
    {
      if (_defaultUser != null)
        return _defaultUser;

      await _lock.WaitAsync();
      try
      {
        Guid systemId = Guid.Parse(ServiceRegistration.Get<ISystemResolver>().LocalSystemId);
        IUserProfileDataManagement updm = UserProfileDataManagement;
        if (updm == null)
          return null;

        var result = await updm.GetProfileAsync(systemId);
        if (result.Success)
        {
          _defaultUser = result.Result;
          return _defaultUser;
        }

        // First check if there is an "old" client profile with same name but different ID. This happens only for older versions.
        // This needs to be done to avoid unique constraint violations when creating the new profile by name.
        // If client profile exists rename it and convert it to a user profile so it can be deleted or used otherwise.
        string profileName = SystemInformation.ComputerName;
        var existingProfile = await updm.GetProfileByNameAsync(profileName);
        if (existingProfile.Success && existingProfile.Result.ProfileId != systemId)
          if (await updm.ChangeProfileIdAsync(existingProfile.Result.ProfileId, systemId))
          {
            result = await updm.GetProfileAsync(systemId);
            if (result.Success)
            {
              _defaultUser = result.Result;
              return _defaultUser;
            }
          }

        // Create a login profile which uses the LocalSystemId and the associated ComputerName
        Guid profileId = await updm.CreateClientProfileAsync(systemId, profileName);
        result = await updm.GetProfileAsync(profileId);
        if (result.Success)
        {
          _defaultUser = result.Result;
          return _defaultUser;
        }

        return null;
      }
      finally
      {
        _lock.Release();
      }
    }
  }

  public static class UserManagementExtensions
  {
    public static async Task<bool> NotifyUsage(this IUserManagement userManagement, string scope, string usedItem)
    {
      if (userManagement == null || userManagement.UserProfileDataManagement == null || !userManagement.IsValidUser)
        return false;

      return await userManagement.UserProfileDataManagement.NotifyFeatureUsageAsync(userManagement.CurrentUser.ProfileId, scope, usedItem);
    }
  }
}
