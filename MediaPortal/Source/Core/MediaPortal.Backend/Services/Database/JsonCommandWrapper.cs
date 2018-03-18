#region Copyright (C) 2007-2017 Team MediaPortal

/*
    Copyright (C) 2007-2017 Team MediaPortal
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
using System.Data;
using System.Data.Common;
using Newtonsoft.Json;

namespace MediaPortal.Backend.Services.Database
{
  public class JsonCommandWrapper
  {
    public string CommandText;
    public CommandType CommandType;
    public List<DbParameterWrapper> Parameters = new List<DbParameterWrapper>();

    public string Serialize()
    {
      return JsonConvert.SerializeObject(this);
    }

    public static JsonCommandWrapper Deserialized(string cmdString)
    {
      return JsonConvert.DeserializeObject<JsonCommandWrapper>(cmdString);
    }

    public void FromCommand(IDbCommand cmd)
    {
      CommandType = cmd.CommandType;
      CommandText = cmd.CommandText;
      foreach (DbParameter parameter in cmd.Parameters)
      {
        var pw = new DbParameterWrapper();
        pw.FromParam(parameter);
        Parameters.Add(pw);
      }
    }

    public void ToCommand(IDbCommand cmd)
    {
      cmd.CommandType = CommandType;
      cmd.CommandText = CommandText;
      foreach (DbParameterWrapper wrapper in Parameters)
      {
        var param = cmd.CreateParameter();
        wrapper.ToParam(param);
        cmd.Parameters.Add(param);
      }
    }
  }

  public class DbParameterWrapper
  {
    public string ParameterName;
    public DbType DbType;
    public ParameterDirection Direction;
    public int Size;
    public object Value;

    public void FromParam(DbParameter parameter)
    {
      ParameterName = parameter.ParameterName;
      DbType = parameter.DbType;
      Size = parameter.Size;
      Value = parameter.Value;
      Direction = parameter.Direction;
    }

    public void ToParam(IDbDataParameter parameter)
    {
      parameter.ParameterName = ParameterName;
      parameter.DbType = DbType;
      parameter.Size = Size;
      if (DbType == DbType.Guid && Value is String)
        parameter.Value = Guid.Parse(Value as string);
      else
        parameter.Value = Value;
      parameter.Direction = Direction;
    }
  }
}
