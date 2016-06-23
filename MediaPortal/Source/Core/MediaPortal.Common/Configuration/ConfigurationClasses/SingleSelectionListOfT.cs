#region Copyright (C) 2007-2015 Team MediaPortal

/*
    Copyright (C) 2007-2015 Team MediaPortal
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
using System.Globalization;
using System.Linq;
using MediaPortal.Common.Localization;

namespace MediaPortal.Common.Configuration.ConfigurationClasses
{
  /// <summary>
  /// Base class for configuration setting classes for configuring a single selection of an enum.
  /// The derived class has to override the methods Load() and Save() to set or save the value.
  /// To achive that the options which are represented by the enum members are localized there have
  /// to be new entries created in the strings file. One entry for each enum member with the full
  /// qualified name of the enum member, for example:
  /// 
  /// Enum definition:
  /// ================================================================================================
  /// 
  /// namespace MediaPortal.Plugins.MyPlugin.Settings
  /// {
  ///   [Serializable]
  ///   public enum DirectionOptionEnum
  ///   {
  ///     Left,
  ///     Right,
  ///     Up,
  ///     Down
  ///   }
  /// }
  /// 
  /// Config setting classes:
  /// ================================================================================================
  /// 
  /// public class MyEnumSelectionConfigSetting : SingleSelectionList≤DirectionOptionEnum≥
  /// {
  ///   public override void Load()
  ///   {
  ///     base.Load();
  ///
  ///     var settings = SettingsManager.Load≤PlugginSettingsContainer≥();
  ///
  ///     Value = settings.Direction;
  ///   }
  ///
  ///   public override void Save()
  ///   {
  ///     base.Save();
  ///
  ///     var settings = SettingsManager.Load≤PluginSettingsContainer≥();
  ///
  ///     settings.Direction = Value;
  ///
  ///     SettingsManager.Save(settings);
  ///   }
  /// }
  /// 
  /// public class PluginSettingsContainer
  /// {
  ///   protected DirectionOptionEnum _direction;
  ///
  ///   [Setting(SettingScope.Global)]
  ///   public DirectionOptionEnum Direction
  ///   {
  ///     get { return _direction; }
  ///     set { _direction = value; }
  ///   }
  /// }
  /// 
  /// strings file
  /// ================================================================================================
  /// 
  /// ≤? xml version="1.0" encoding="utf-8"?≥
  /// ≤resources≥
  ///   ≤string name = "MediaPortal.Plugins.MyPlugin.Settings.DirectionOptionEnum.Left"≥Turn left≤/string≥
  ///   ≤string name = "MediaPortal.Plugins.MyPlugin.Settings.DirectionOptionEnum.Right"≥Turn right≤/string≥
  ///   ≤string name = "MediaPortal.Plugins.MyPlugin.Settings.DirectionOptionEnum.Up"≥Go up≤/string≥
  ///   ≤string name = "MediaPortal.Plugins.MyPlugin.Settings.DirectionOptionEnum.Down"≥Go down≤/string≥
  /// ≤/resources≥
  /// 
  /// </summary>
  public abstract class SingleSelectionList<T> : SingleSelectionList
    where T : struct, IConvertible
  {
    #region Variables

    #region Private

    private readonly IDictionary<T, int> _selectionListDictionary = new Dictionary<T, int>();
    private readonly IDictionary<int, T> _selectionListDictionaryReverse = new Dictionary<int, T>();
    private T _value;

    #endregion

    #endregion

    #region Constructor

    protected SingleSelectionList()
    {
      if (!typeof(T).IsEnum)
      {
        throw new ArgumentException("SingleSelectionList<T> instance error: T must be an enumerated type (enum)");
      }

      var currentSelectPosition = 0;
      Enum.GetValues(typeof(T))
        .Cast<T>()
        .ToList()
        .ForEach(enumMember =>
        {
          Add(enumMember, currentSelectPosition++);
        });
    }

    #endregion

    #region Methods

    #region Private

    #region Clear

    /// <summary>
    /// Clear the selection
    /// </summary>
    private void Clear()
    {
      Selected = -1;
    }

    #endregion

    #region Add

    /// <summary>
    /// Add a possible selection for an enum member to the list
    /// </summary>
    /// <param name="optionEnumMemberGeneric">The enum member</param>
    /// <param name="enumMemberGenericSelectPosition">The position in the list of this enum member</param>
    private void Add(T optionEnumMemberGeneric, int enumMemberGenericSelectPosition)
    {
      var optionEnumMemberType = optionEnumMemberGeneric.GetType();
      var optionEnumMemberTypeFullname = optionEnumMemberType.FullName;
      var optionEnumMemberMembername = optionEnumMemberGeneric.ToString(CultureInfo.InvariantCulture);
      var localizationRessource = $"[{optionEnumMemberTypeFullname}.{optionEnumMemberMembername}]";
      var res = LocalizationHelper.CreateResourceString(localizationRessource);

      _items.Add(res);

      _selectionListDictionary.Add(optionEnumMemberGeneric, enumMemberGenericSelectPosition);
      _selectionListDictionaryReverse.Add(enumMemberGenericSelectPosition, optionEnumMemberGeneric);
    }

    #endregion

    #endregion

    #region Base overrides

    #region Load

    public override void Load()
    {
      base.Load();

      Clear();
    }

    #endregion

    #endregion

    #region Protected

    #region Value

    /// <summary>
    /// The value selection
    /// </summary>
    protected T Value
    {
      get
      {
        return Selected == -1 ? new T() : _selectionListDictionaryReverse[Selected];
      }

      set
      {
        if (_value.Equals(value))
        {
          Selected = _selectionListDictionary[value];
          return;
        }
        _value = value;
        Selected = _selectionListDictionary[_value];

        NotifyChange();
      }
    }

    #endregion

    #endregion

    #endregion
  }

}
