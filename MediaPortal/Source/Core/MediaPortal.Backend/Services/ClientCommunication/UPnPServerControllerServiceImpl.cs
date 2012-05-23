#region Copyright (C) 2007-2012 Team MediaPortal

/*
    Copyright (C) 2007-2012 Team MediaPortal
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
using MediaPortal.Backend.ClientCommunication;
using MediaPortal.Common;
using MediaPortal.Common.General;
using MediaPortal.Common.Messaging;
using MediaPortal.Common.SystemResolver;
using MediaPortal.Common.UPnP;
using MediaPortal.Utilities.UPnP;
using UPnP.Infrastructure.Common;
using UPnP.Infrastructure.Dv;
using UPnP.Infrastructure.Dv.DeviceTree;

namespace MediaPortal.Backend.Services.ClientCommunication
{
  /// <summary>
  /// Provides the UPnP service implementation for the MediaPortal 2 server controller interface.
  /// </summary>
  public class UPnPServerControllerServiceImpl : DvService
  {
    protected DvStateVariable AttachedClientsChangeCounter;
    protected DvStateVariable ConnectedClientsChangeCounter;

    protected UInt32 _attachedClientsChangeCt = 0;
    protected UInt32 _connectedClientsChangeCt = 0;

    protected AsynchronousMessageQueue _messageQueue;

    public UPnPServerControllerServiceImpl() : base(
        UPnPTypesAndIds.SERVER_CONTROLLER_SERVICE_TYPE, UPnPTypesAndIds.SERVER_CONTROLLER_SERVICE_TYPE_VERSION,
        UPnPTypesAndIds.SERVER_CONTROLLER_SERVICE_ID)
    {
      // Used for system ID strings
      DvStateVariable A_ARG_TYPE_SystemId = new DvStateVariable("A_ARG_TYPE_SystemId", new DvStandardDataType(UPnPStandardDataType.String))
          {
            SendEvents = false
          };
      AddStateVariable(A_ARG_TYPE_SystemId);

      // Used for bool values
      DvStateVariable A_ARG_TYPE_Bool = new DvStateVariable("A_ARG_TYPE_Bool", new DvStandardDataType(UPnPStandardDataType.Boolean))
          {
            SendEvents = false
          };
      AddStateVariable(A_ARG_TYPE_Bool);

      // Used to transport a system name - contains the hostname string
      DvStateVariable A_ARG_TYPE_SystemName = new DvStateVariable("A_ARG_TYPE_SystemName", new DvStandardDataType(UPnPStandardDataType.String))
          {
            SendEvents = false
          };
      AddStateVariable(A_ARG_TYPE_SystemName);

      // Used to transport an enumeration of MPClientMetadata objects
      DvStateVariable A_ARG_TYPE_ClientMetadataEnumeration = new DvStateVariable("A_ARG_TYPE_ClientMetadataEnumeration", new DvExtendedDataType(UPnPExtendedDataTypes.DtMPClientMetadataEnumeration))
        {
            SendEvents = false
        };
      AddStateVariable(A_ARG_TYPE_ClientMetadataEnumeration);

      // CSV of GUID strings
      DvStateVariable A_ARG_TYPE_UuidEnumeration = new DvStateVariable("A_ARG_TYPE_UuidEnumeration", new DvStandardDataType(UPnPStandardDataType.String))
          {
            SendEvents = false
          };
      AddStateVariable(A_ARG_TYPE_UuidEnumeration);

      AttachedClientsChangeCounter = new DvStateVariable("AttachedClientsChangeCounter", new DvStandardDataType(UPnPStandardDataType.Ui4))
        {
            SendEvents = true,
            Value = (uint) 0
        };
      AddStateVariable(AttachedClientsChangeCounter);

      // Csv of client's system ids
      ConnectedClientsChangeCounter = new DvStateVariable("ConnectedClientsChangeCounter", new DvStandardDataType(UPnPStandardDataType.Ui4))
        {
            SendEvents = true,
            Value = (uint) 0
        };
      AddStateVariable(ConnectedClientsChangeCounter);

      // More state variables go here

      DvAction attachClientAction = new DvAction("AttachClient", OnAttachClient,
          new DvArgument[] {
            new DvArgument("ClientSystemId", A_ARG_TYPE_SystemId, ArgumentDirection.In),
          },
          new DvArgument[] {
          });
      AddAction(attachClientAction);

      DvAction detachClientAction = new DvAction("DetachClient", OnDetachClient,
          new DvArgument[] {
            new DvArgument("ClientSystemId", A_ARG_TYPE_SystemId, ArgumentDirection.In),
          },
          new DvArgument[] {
          });
      AddAction(detachClientAction);

      DvAction getAttachedClientsAction = new DvAction("GetAttachedClients", OnGetAttachedClients,
          new DvArgument[] {
          },
          new DvArgument[] {
            new DvArgument("AttachedClients", A_ARG_TYPE_ClientMetadataEnumeration, ArgumentDirection.Out, true),
          });
      AddAction(getAttachedClientsAction);

      DvAction getConnectedClientsAction = new DvAction("GetConnectedClients", OnGetConnectedClients,
          new DvArgument[] {
          },
          new DvArgument[] {
            new DvArgument("ConnectedClients", A_ARG_TYPE_UuidEnumeration, ArgumentDirection.Out, true),
          });
      AddAction(getConnectedClientsAction);

      DvAction getSystemNameForSytemIdAction = new DvAction("GetSystemNameForSystemId", OnGetSystemNameForSytemId,
          new DvArgument[] {
            new DvArgument("SystemId", A_ARG_TYPE_SystemId, ArgumentDirection.In),
          },
          new DvArgument[] {
            new DvArgument("SystemName", A_ARG_TYPE_SystemName, ArgumentDirection.Out),
          });
      AddAction(getSystemNameForSytemIdAction);

      // More actions go here

      _messageQueue = new AsynchronousMessageQueue(this, new string[]
        {
            ClientManagerMessaging.CHANNEL,
        });
      _messageQueue.MessageReceived += OnMessageReceived;
      _messageQueue.Start();
    }

    public override void Dispose()
    {
      base.Dispose();
      _messageQueue.Shutdown();
    }

    private void OnMessageReceived(AsynchronousMessageQueue queue, SystemMessage message)
    {
      if (message.ChannelName == ClientManagerMessaging.CHANNEL)
      {
        ClientManagerMessaging.MessageType messageType = (ClientManagerMessaging.MessageType) message.MessageType;
        switch (messageType)
        {
          case ClientManagerMessaging.MessageType.ClientAttached:
          case ClientManagerMessaging.MessageType.ClientDetached:
            AttachedClientsChangeCounter.Value = ++_attachedClientsChangeCt;
            break;
          case ClientManagerMessaging.MessageType.ClientOnline:
          case ClientManagerMessaging.MessageType.ClientOffline:
            ConnectedClientsChangeCounter.Value = ++_connectedClientsChangeCt;
            break;
        }
      }
    }

    static UPnPError OnAttachClient(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      string clientSystemId = (string) inParams[0];
      ServiceRegistration.Get<IClientManager>().AttachClient(clientSystemId);
      outParams = null;
      return null;
    }

    static UPnPError OnDetachClient(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      string clientSystemId = (string) inParams[0];
      ServiceRegistration.Get<IClientManager>().DetachClientAndRemoveShares(clientSystemId);
      outParams = null;
      return null;
    }

    static UPnPError OnGetAttachedClients(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      outParams = new List<object> {ServiceRegistration.Get<IClientManager>().AttachedClients.Values};
      return null;
    }

    static UPnPError OnGetConnectedClients(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      outParams = new List<object> {MarshallingHelper.SerializeStringEnumerationToCsv(
          ServiceRegistration.Get<IClientManager>().ConnectedClients.Select(clientConnection => clientConnection.Descriptor.MPFrontendServerUUID))};
      return null;
    }

    static UPnPError OnGetSystemNameForSytemId(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      string systemId = (string) inParams[0];
      SystemName result = ServiceRegistration.Get<ISystemResolver>().GetSystemNameForSystemId(systemId);
      outParams = new List<object> {result == null ? null : result.HostName};
      return null;
    }
  }
}
