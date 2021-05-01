using System;
using System.Collections.Generic;

namespace Automation.Concord
{

    public static class MessageCodeMap
    {
        static Dictionary<string, MessageType> inboundMessageMap;
        static Dictionary<string, MessageType> outboundMessageMap;

        static MessageCodeMap()
        {
            #region Initialization
            outboundMessageMap = new Dictionary<string, MessageType>();
            outboundMessageMap.Add("20", MessageType.DynamicDataRefreshRequest);
            outboundMessageMap.Add("40", MessageType.Keypress);
            outboundMessageMap.Add("02", MessageType.FullEquipmentListRequest);
            outboundMessageMap.Add("0203", MessageType.SingleEquipmentListRequest);
            outboundMessageMap.Add("0204", MessageType.SingleEquipmentListRequest);
            outboundMessageMap.Add("0205", MessageType.SingleEquipmentListRequest);
            outboundMessageMap.Add("0206", MessageType.SingleEquipmentListRequest);
            outboundMessageMap.Add("0207", MessageType.SingleEquipmentListRequest);
            outboundMessageMap.Add("0209", MessageType.SingleEquipmentListRequest);
            outboundMessageMap.Add("020A", MessageType.SingleEquipmentListRequest);
            outboundMessageMap.Add("020B", MessageType.SingleEquipmentListRequest);
            outboundMessageMap.Add("020C", MessageType.SingleEquipmentListRequest);

            inboundMessageMap = new Dictionary<string, MessageType>();
            inboundMessageMap.Add("01", MessageType.PanelType);
            inboundMessageMap.Add("02", MessageType.AutomationEventLost);
            inboundMessageMap.Add("20", MessageType.ClearAutomationDynamicImage);
            inboundMessageMap.Add("21", MessageType.ZoneStatus);
            inboundMessageMap.Add("2201", MessageType.ArmingLevel);
            inboundMessageMap.Add("2203", MessageType.EntryExitDelay);
            inboundMessageMap.Add("2202", MessageType.AlarmTrouble);
            inboundMessageMap.Add("2204", MessageType.SirenSetup);
            inboundMessageMap.Add("2205", MessageType.SirenSynchronize);
            inboundMessageMap.Add("2206", MessageType.SirenGo);
            inboundMessageMap.Add("2209", MessageType.TouchpadDisplay);
            inboundMessageMap.Add("220B", MessageType.SirenStop);
            inboundMessageMap.Add("220C", MessageType.FeatureState);
            inboundMessageMap.Add("220D", MessageType.Temperature);
            inboundMessageMap.Add("220E", MessageType.TimeAndDate);
            inboundMessageMap.Add("2301", MessageType.LightsState);
            inboundMessageMap.Add("2302", MessageType.UserLights);
            inboundMessageMap.Add("2303", MessageType.Keyfob);
            inboundMessageMap.Add("03", MessageType.EquipmentListZone);
            inboundMessageMap.Add("04", MessageType.EquipmentListPartition);
            inboundMessageMap.Add("05", MessageType.EquipmentListSuperBusDevice);
            inboundMessageMap.Add("06", MessageType.EquipmentListSuperBusDeviceCapabilities);
            inboundMessageMap.Add("07", MessageType.EquipmentListOutput);
            inboundMessageMap.Add("09", MessageType.EquipmentListUser);
            inboundMessageMap.Add("0A", MessageType.EquipmentListSchedule);
            inboundMessageMap.Add("0B", MessageType.EquipmentListScheduledEvent);
            inboundMessageMap.Add("0C", MessageType.EquipmentListLightToSensor);
            inboundMessageMap.Add("08", MessageType.EquipmentListComplete);
            inboundMessageMap.Add("0360", MessageType.Reserved);
            inboundMessageMap.Add("0399", MessageType.Reserved);
            inboundMessageMap.Add("0398", MessageType.Reserved);
            #endregion
        }

        private static MessageType MapProtocolCommand(Dictionary<string, MessageType> mappingTable, string message)
        {
            if (string.IsNullOrEmpty(message) || message.Length < 6) throw new ArgumentOutOfRangeException("message");

            MessageType command = MessageType.Unknown;
            string data = message.Substring(2);
            string commandToken = "";

            if (data.Length >= 4)
            {
                //check for a two byte command first
                commandToken = data.Substring(0, 4);

                if (mappingTable.ContainsKey(commandToken))
                {
                    command = mappingTable[commandToken];
                    return command;
                }
            }

            if (data.Length >= 2)
            {
                //then check for a single byte command 
                commandToken = data.Substring(0, 2);

                if (mappingTable.ContainsKey(commandToken))
                {
                    command = mappingTable[commandToken];
                    return command;
                }
            }

            return command;
        }

        public static MessageType MapOutgoingProtocolMessage(string message)
        {
            return MapProtocolCommand(outboundMessageMap, message);
        }

        public static MessageType MapInboundProtocolMessage(string message)
        {
            return MapProtocolCommand(inboundMessageMap, message);
        }

        public static Message CreateMessage(string message)
        {
            MessageType command = MessageCodeMap.MapInboundProtocolMessage(message);
            Message result = null;

            switch (command)
            {
                case MessageType.Unknown:
                    result = new InboundMessages.Unknown(message);
                    break;
                case MessageType.AlarmTrouble:
                    InboundMessages.AlarmTrouble general = new InboundMessages.AlarmTrouble(message);

                    switch (general.GeneralEventType)
                    {
                        case AlertClass.Alarm:
                        case AlertClass.AlarmCancel:
                        case AlertClass.AlarmRestoral:
                            result = new Automation.Concord.InboundMessages.Alerts.Alarm(message);
                            break;
                        case AlertClass.FireTrouble:
                        case AlertClass.FireTroubleRestoral:
                        case AlertClass.NonFireTrouble:
                        case AlertClass.NonFireTroubleRestoral:
                            result = new Automation.Concord.InboundMessages.Alerts.Trouble(message);
                            break;
                        case AlertClass.Bypass:
                        case AlertClass.Unbypass:
                            result = new Automation.Concord.InboundMessages.Alerts.Bypass(message);
                            break;
                        case AlertClass.Opening:
                        case AlertClass.Closing:
                            result = new Automation.Concord.InboundMessages.Alerts.OpeningClosing(message);
                            break;
                        case AlertClass.PartitionEvent:
                            result = new Automation.Concord.InboundMessages.Alerts.PartitionEvent(message);
                            break;
                        case AlertClass.PartitionTest:
                            result = new Automation.Concord.InboundMessages.Alerts.PartitionTest(message);
                            break;
                        case AlertClass.SystemEvent:
                            result = new Automation.Concord.InboundMessages.Alerts.SystemEvent(message);
                            break;
                        case AlertClass.SystemTrouble:
                        case AlertClass.SystemTroubleRestoral:
                            result = new Automation.Concord.InboundMessages.Alerts.SystemTrouble(message);
                            break;
                        case AlertClass.SystemConfigurationChange:
                            result = new Automation.Concord.InboundMessages.Alerts.SystemConfigurationChange(message);
                            break;
                        default:
                            result = general;
                            break;
                    }
                    break;
                case MessageType.ArmingLevel:
                    result = new InboundMessages.ArmingLevelState(message);
                    break;
                case MessageType.AutomationEventLost:
                    result = new InboundMessages.AutomationEventLost(message);
                    break;
                case MessageType.ClearAutomationDynamicImage:
                    result = new InboundMessages.ClearAutomationDynamicImage(message);
                    break;
                case MessageType.EntryExitDelay:
                    result = new InboundMessages.EntryExitDelay(message);
                    break;
                case MessageType.EquipmentListComplete:
                    result = new InboundMessages.EquipmentListComplete(message);
                    break;
                case MessageType.EquipmentListLightToSensor:
                    result = new InboundMessages.EquipmentListLightToSensor(message);
                    break;
                case MessageType.EquipmentListOutput:
                    result = new InboundMessages.EquipmentListOutput(message);
                    break;
                case MessageType.EquipmentListPartition:
                    result = new InboundMessages.EquipmentListPartition(message);
                    break;
                case MessageType.EquipmentListScheduledEvent:
                    result = new InboundMessages.EquipmentListScheduledEvent(message);
                    break;
                case MessageType.EquipmentListSchedule:
                    result = new InboundMessages.EquipmentListSchedule(message);
                    break;
                case MessageType.EquipmentListSuperBusDevice:
                    result = new InboundMessages.EquipmentListSuperBusDevice(message);
                    break;
                case MessageType.EquipmentListSuperBusDeviceCapabilities:
                    result = new InboundMessages.EquipmentListSuperBusDeviceCapabilities(message);
                    break;
                case MessageType.EquipmentListUser:
                    result = new InboundMessages.EquipmentListUser(message);
                    break;
                case MessageType.EquipmentListZone:
                    result = new InboundMessages.EquipmentListZone(message);
                    break;
                case MessageType.FeatureState:
                    result = new InboundMessages.FeatureState(message);
                    break;
                case MessageType.Keyfob:
                    result = new InboundMessages.Keyfob(message);
                    break;
                case MessageType.LightsState:
                    result = new InboundMessages.LightsState(message);
                    break;
                case MessageType.PanelType:
                    result = new InboundMessages.PanelType(message);
                    break;
                case MessageType.Reserved:
                    result = new InboundMessages.Reserved(message);
                    break;
                case MessageType.SirenGo:
                    result = new InboundMessages.SirenGo(message);
                    break;
                case MessageType.SirenSetup:
                    result = new InboundMessages.SirenSetup(message);
                    break;
                case MessageType.SirenStop:
                    result = new InboundMessages.SirenStop(message);
                    break;
                case MessageType.SirenSynchronize:
                    result = new InboundMessages.SirenSynchronize(message);
                    break;
                case MessageType.Temperature:
                    result = new InboundMessages.Temperature(message);
                    break;
                case MessageType.TimeAndDate:
                    result = new InboundMessages.TimeAndDate(message);
                    break;
                case MessageType.TouchpadDisplay:
                    result = new InboundMessages.TouchpadDisplay(message);
                    break;
                case MessageType.UserLights:
                    result = new InboundMessages.UserLights(message);
                    break;
                case MessageType.ZoneStatus:
                    result = new InboundMessages.ZoneStatus(message);
                    break;
            }

            if (result == null)
                result = new InboundMessages.Unknown(message);

            return result;
        }


    }

}
