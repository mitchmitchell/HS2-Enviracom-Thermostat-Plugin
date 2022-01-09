using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace HSPI_ENVIRACOM_MANAGER
{
    /// <summary>
    /// 
    /// </summary>
    public class clsMessageMgr
    {
        #region "  Enumerations and Constants   "
        private const int MsgAdapterStringField = 0;
        private const int MsgPriorityField = 0;
        private const int MsgClassIdField = 1;
        private const int MsgInstanceIdField = 2;
        private const int MsgTypeField = 3;
        private const int MsgNumDataBytesField = 4;
        private const int MsgStartDataBytesField = 5;

        private const int   MsgDataByteUnknown_0 = 0;
        private const int   MsgDataByteUnknown_1 = 1;
        private const int   MsgDataByteUnknown_2 = 2;
        private const int   MsgDataByteUnknown_3 = 3;
        private const int   MsgDataByteUnknown_4 = 4;
        private const int   MsgDataByteUnknown_5 = 5;
        private const int   MsgDataByteUnknown_6 = 6;
        private const int   MsgDataByteUnknown_7 = 7;
        private const int	MsgDataByteAllowedModesHi = 1;
        private const int	MsgDataByteAllowedModesLo = 2;
        private const int	MsgDataByteAuxHeatStatus = 0;
        private const int	MsgDataByteBlowerFanSpeed = 0;
        private const int	MsgDataByteCoolingHeatPumpState = 0;
        private const int	MsgDataByteCoolSetPointHi = 3;
        private const int	MsgDataByteCoolSetPointLo = 4;
        private const int   MsgDataByteDamperStatus = 0;
        private const int   MsgDataByteDay = 4;
        private const int	MsgDataByteDehumidifierMode = 0;
        private const int	MsgDataByteDeHumidifyLowerLimit = 2;
        private const int	MsgDataByteDeHumidifySetPoint = 0;
        private const int	MsgDataByteDeHumidifyUpperLimit = 1;
        private const int	MsgDataByteErrorNumber = 3;
        private const int	MsgDataByteExternalDehumidifierState = 1;
        private const int	MsgDataByteExternalVentilationState = 1;
        private const int	MsgDataByteFanMode = 7;
        private const int	MsgDataByteFanModeOne = 0;
        private const int	MsgDataByteFanModeTwo = 1;
        private const int	MsgDataByteFanStatus = 1;
        private const int	MsgDataByteHeatingHeatPumpState = 0;
        private const int	MsgDataByteHeatSetPointHi = 0;
        private const int	MsgDataByteHeatSetPointLo = 1;
        private const int	MsgDataByteHoldStatus = 2;
        private const int	MsgDataByteHourDow = 3;
        private const int	MsgDataByteHumidifierState = 1;
        private const int	MsgDataByteHumidityLowerLimit = 2;
        private const int	MsgDataByteHumiditySetPoint = 0;
        private const int	MsgDataByteHumidityUpperLimit = 1;
        private const int	MsgDataByteMinutes = 2;
        private const int	MsgDataByteMonth = 5;
        private const int	MsgDataByteOutdoorTempHi = 0;
        private const int	MsgDataByteOutdoorTempLo = 1;
        private const int	MsgDataBytePeriodDay = 0;
        private const int	MsgDataBytePeriodTimeHi = 1;
        private const int	MsgDataBytePeriodTimeLo = 2;
        private const int	MsgDataByteRecovery = 6;
        private const int	MsgDataByteRemainingFilterTime = 0;
        private const int	MsgDataByteRoomHumidity = 0;
        private const int	MsgDataByteRoomTemp = 0;
        private const int	MsgDataByteScheduleCoolSetPointHi = 5;
        private const int	MsgDataByteScheduleCoolSetPointLo = 6;
        private const int	MsgDataByteScheduleHeatSetPointHi = 3;
        private const int	MsgDataByteScheduleHeatSetPointLo = 4;
        private const int	MsgDataByteSeconds = 1;
        private const int	MsgDataByteSensorFaultandUnits = 1;
        private const int	MsgDataByteStageFourState = 6;
        private const int	MsgDataByteStageOneState = 3;
        private const int	MsgDataByteStageThreeState = 5;
        private const int	MsgDataByteStageTwoState = 4;
        private const int	MsgDataByteSystemState = 2;
        private const int	MsgDataByteSystemSwitch = 0;
        private const int	MsgDataByteTotalFilterTime = 1;
        private const int	MsgDataByteYearHi = 6;
        private const int	MsgDataByteYearLo = 6;
        private const int	MsgDataByteZoneManager = 0;

        private const string LOG_MESSAGES = "enviracom_sa_messages.log";
        #endregion

        #region "  Structures    "
        struct Message
        {
            public delegate void ProcessMessageDelegate(string[] sList, Message strMsg);

            public string text;
            public string desc;
            public int    dlen;
            public ProcessMessageDelegate proc;
            public int    unit;

            public Message(string sText, string sDesc, ProcessMessageDelegate pDlgt, int iLen)
            {
                text = sText;
                desc = sDesc;
                proc = pDlgt;
                dlen = iLen;
                unit = -1;
            }
        };
        #endregion

        #region "   Members   "
        //	Members
        private Dictionary<string, Message> m_dctMessageDictionary = new Dictionary<string,Message>();
        private clsEnviracomApp             m_objApp = clsEnviracomApp.GetInstance();
        #endregion

        #region "  Accessor Methods for Members  "
        #endregion

        #region "  Constructors and Destructors   "
        public clsMessageMgr()
        {
            m_dctMessageDictionary.Add("3FFF",      new Message("3FFF", "HVAC_UNKNOWNJ",                    TraceReceivedMessage, 0));
            m_dctMessageDictionary.Add("3EE0",      new Message("3EE0", "HVAC_ZONEMANAGER",                 ReceiveZoneManagerMessage, 1));
            m_dctMessageDictionary.Add("3E72 10",   new Message("3E72 10", "HVAC_UNKNOWNK",                 TraceReceivedMessage, 1));
            m_dctMessageDictionary.Add("3E72 11",   new Message("3E72 11", "HVAC_UNKNOWNL",                 TraceReceivedMessage, 1));
            m_dctMessageDictionary.Add("3E72 41",   new Message("3E72 41", "HVAC_UNKNOWNM",                 TraceReceivedMessage, 1));
            m_dctMessageDictionary.Add("3E72 42",   new Message("3E72 42", "HVAC_UNKNOWNN",                 TraceReceivedMessage, 1));
            m_dctMessageDictionary.Add("3E72 51",   new Message("3E72 51", "HVAC_UNKNOWN1",                 TraceReceivedMessage, 1));
            m_dctMessageDictionary.Add("3E70 10",   new Message("3E70 10", "HVAC_BLOWER_FAN_STATUS",        ReceiveBlowerFanStatusMessage, 4));
            m_dctMessageDictionary.Add("3E70 11",   new Message("3E70 11", "HVAC_AUX_HEAT_STATUS",          ReceiveAuxHeatStatusMessage, 4));
            m_dctMessageDictionary.Add("3E70 41",   new Message("3E70 41", "HVAC_HEATING_HEATPUMP_STATUS",  ReceiveHeatingHeatPumpStatusMessage, 4));
            m_dctMessageDictionary.Add("3E70 42",   new Message("3E70 42", "HVAC_COOLING_HEATPUMP_STATUS",  ReceiveCoolingHeatPumpStatusMessage, 4));
            m_dctMessageDictionary.Add("3C00",      new Message("3C00", "HVAC_UNKNOWNC",                    TraceReceivedMessage, 1));
            m_dctMessageDictionary.Add("31F0",      new Message("31F0", "HVAC_CIRCULATE_FAN_STATUS",        ReceiveCirculateFanMessage, 2));
            m_dctMessageDictionary.Add("31E5",      new Message("31E5", "HVAC_DEHUMIDIFIER_STATUS",         ReceiveDehumidifierStateMessage, 2));
            m_dctMessageDictionary.Add("31E3",      new Message("31E3", "HVAC_HUMIDIFIER_STATUS",           ReceiveHumidifierStateMessage, 2));
            m_dctMessageDictionary.Add("31E0",      new Message("31E0", "HVAC_EXTERNAL_VENTILATION_STATUS", ReceiveExternalVentilationStateMessage, 2));
            m_dctMessageDictionary.Add("3180 3B",   new Message("3180 3B", "HVAC_AQUASTAT_CIRCULATING_PUMP_STATUS",          ReceiveAquastatCirculatingPumpStatusMessage, 1));
            m_dctMessageDictionary.Add("3180",      new Message("3180", "HVAC_DAMPER_STATUS",               ReceiveDamperStatusMessage, 1));
            m_dctMessageDictionary.Add("313F",      new Message("313F", "HVAC_TIME_REPORT",                 ReceiveTimeMessage, 8));
            m_dctMessageDictionary.Add("3120",      new Message("3120", "HVAC_ERROR_NUMBER",                ReceiveErrorMessage, 6));
            m_dctMessageDictionary.Add("3110",      new Message("3110", "HVAC_SYSTEM_STATE",                ReceiveSystemStateMessage, 4));
            m_dctMessageDictionary.Add("3100",      new Message("3100", "HVAC_EQUIPMENT_FAULT",             ReceiveEquipmentFaultMessage, 4));
            m_dctMessageDictionary.Add("2330",      new Message("2330", "HVAC_SETPOINT",                    ReceiveSetPointMessage, 8));
            m_dctMessageDictionary.Add("22D0",      new Message("22D0", "HVAC_SYSTEM_SWITCH",               ReceiveSystemSwitchMessage, 3));
            m_dctMessageDictionary.Add("22C9",      new Message("22C9", "HVAC_UNKNOWNI",                    TraceReceivedMessage, 0));
            m_dctMessageDictionary.Add("22C0",      new Message("22C0", "HVAC_FAN_SWITCH",                  ReceiveFanSwitchMessage, 2));
            m_dctMessageDictionary.Add("22A1",      new Message("22A1", "HVAC_DEHUMIDFIER_SETPOINT",        ReceiveDehumidifierSetPointMessage, 3));
            m_dctMessageDictionary.Add("22A0",      new Message("22A0", "HVAC_HUMIDFIER_SETPOINT",          ReceiveHumidifierSetPointMessage, 3));
            m_dctMessageDictionary.Add("1F80",      new Message("1F80", "HVAC_SCHEDULE",                    ReceiveScheduleMessage, 8));
            m_dctMessageDictionary.Add("12C0",      new Message("12C0", "HVAC_ROOM_TEMP",                   ReceiveRoomTempMessage, 2));
            m_dctMessageDictionary.Add("12A0",      new Message("12A0", "HVAC_HUMIDITY",                    ReceiveRoomHumidityMessage, 1));
            m_dctMessageDictionary.Add("1290",      new Message("1290", "HVAC_OUTDOOR_TEMP",                ReceiveOutdoorTempMessage, 2));
            m_dctMessageDictionary.Add("1210",      new Message("1210", "HVAC_UNKNOWNA",                    TraceReceivedMessage, 5));
            m_dctMessageDictionary.Add("120F",      new Message("120F", "HVAC_UNKNOWNB",                    TraceReceivedMessage, 5));
            m_dctMessageDictionary.Add("11C0",      new Message("11C0", "HVAC_UNKNOWN2",                    TraceReceivedMessage, 5));
            m_dctMessageDictionary.Add("11B0",      new Message("11B0", "HVAC_UNKNOWN3",                    TraceReceivedMessage, 4));
            m_dctMessageDictionary.Add("11A0",      new Message("11A0", "HVAC_UNKNOWN4",                    TraceReceivedMessage, 4));
            m_dctMessageDictionary.Add("1180",      new Message("1180", "HVAC_UNKNOWN5",                    TraceReceivedMessage, 5));
            m_dctMessageDictionary.Add("1170",      new Message("1170", "HVAC_UNKNOWN6",                    TraceReceivedMessage, 3));
            m_dctMessageDictionary.Add("1150",      new Message("1150", "HVAC_UNKNOWN7",                    TraceReceivedMessage, 3));
            m_dctMessageDictionary.Add("1120",      new Message("1120", "HVAC_UVLAMPTIMER",                 TraceReceivedMessage, 3));
            m_dctMessageDictionary.Add("10F0",      new Message("10F0", "HVAC_UNKNOWN8",                    TraceReceivedMessage, 6));
            m_dctMessageDictionary.Add("10E7",      new Message("10E7", "HVAC_UNKNOWNE",                    TraceReceivedMessage, 4));
            m_dctMessageDictionary.Add("10D9",      new Message("10D9", "HVAC_UNKNOWNF",                    TraceReceivedMessage, 2));
            m_dctMessageDictionary.Add("10D0",      new Message("10D0", "HVAC_AIR_FILTER",                  ReceiveAirFilterTimerMessage, 2));
            m_dctMessageDictionary.Add("1040",      new Message("1040", "HVAC_LIMITPOINTS",                 TraceReceivedMessage, 8));
            m_dctMessageDictionary.Add("0A03",      new Message("0A03", "HVAC_UNKNOWN9",                    TraceReceivedMessage, 8));
            m_dctMessageDictionary.Add("0A02",      new Message("0A02", "HVAC_UNKNOWND",                    TraceReceivedMessage, 8));
            m_dctMessageDictionary.Add("0A01",      new Message("0A01", "HVAC_DEADBAND",                    TraceReceivedMessage, 8));
        }

        ~clsMessageMgr()
        {
        }
        #endregion

        #region "  Static Methods  "
        #endregion

        #region "  Initialization and Cleanup  "
        #endregion

        #region "  Public Methods   "

        public void Receive(string sMessage, int iUnit)
        {
            Message msg;
            try
            {

                //	Parse the message into the relevant pieces
                string[] sList = sMessage.Split(' ');

                if (m_dctMessageDictionary.TryGetValue(sList[MsgClassIdField] + " " + sList[MsgInstanceIdField], out msg) == true)
                {
                    msg.unit = iUnit;
                    msg.proc(sList, msg);
                }
                else if (m_dctMessageDictionary.TryGetValue(sList[MsgClassIdField], out msg) == true)
                {
                    msg.unit = iUnit;
                    msg.proc(sList, msg);
                }
                else
                {
                    clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::Receive: did not find message '" + sMessage + "' in dictionary");
                    //	For now, add the message to our Message.Log so that I have a 
                    //	complete list of possible messages that may be on the wire
                    AddMessageToLog(sMessage);
                }
            }
            catch (Exception ex)
            {
                //	This isn't good, log it appropriately
                clsEnviracomApp.TraceEvent(TraceEventType.Error, "clsMessageMgr::Receive: Reporting exception: " + ex.ToString());
            }
        }


        #region "  Public Format Message Methods   "

        #region "  Public Format Change Message Methods   "

        public string FormatChangeSetPointsMessage(int iZoneNumber, int iHeatSetPoint, int iHoldStatus, int iCoolSetPoint, int iUnknown_5, int iRecovery, int iPeriod, int iUnknown_7)
        {
            string szMessage = "M 2330 " + iZoneNumber.ToString("X2") + " C 08 " + FormatWordString(iHeatSetPoint) + " " + iHoldStatus.ToString("X2") + " " + FormatWordString(iCoolSetPoint) + " " + iUnknown_5.ToString("X2") + " " + iRecovery.ToString("X1") + iPeriod.ToString("X1") + " " + iUnknown_7.ToString("X2");
            szMessage += " " + CalculateCheckSum(szMessage);
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::FormatChangeSetPointsMessage: message is '" + szMessage + "'");
            //	Log the information to Homeseer
            clsEnviracomApp.LogInformation("Transmit change set points message: '" + szMessage + "'");
            return szMessage;
        }

        public string FormatChangeTimeMessage(int iZoneNumber, int iDayOfWeek, int iMonth, int iDay, int iYear, int iHours, int iMinutes, int iSeconds)
        {
            byte bHoursAndDow = (byte)((byte)(iHours & 0x1F) | (byte)((iDayOfWeek & 0x07) << 5));
            string sMessage = "M 313F " + iZoneNumber.ToString("X2") + " C 08 3B " + iSeconds.ToString("X2") + " " + iMinutes.ToString("X2") + " " + bHoursAndDow.ToString("X2") + " " + iDay.ToString("X2") + " " + iMonth.ToString("X2") + " " + FormatWordString(iYear);
            sMessage += " " + CalculateCheckSum(sMessage);
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::FormatChangeTimeMessage: message is '" + sMessage + "'");
            //	Log the information to Homeseer
            clsEnviracomApp.LogInformation("Transmit change time message: '" + sMessage + "'");
            return sMessage;
        }

        public string FormatChangeModeMessage(int iZoneNumber, int iSwitchSetting)
        {
            string szMessage = "M 22D0 " + iZoneNumber.ToString("X2") + " C 03 " + iSwitchSetting.ToString("X2") + " 00 00";
            szMessage += " " + CalculateCheckSum(szMessage);
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::FormatChangeModeMessage: message is '" + szMessage + "'");
            //	Log the information to Homeseer
            clsEnviracomApp.LogInformation("Transmit change mode message: '" + szMessage + "'");
            return szMessage;
        }

        public string FormatChangeFanMessage(int iZoneNumber, int iFanModeOne, int iFanModeTwo)
        {
            string szMessage = "M 22C0 " + iZoneNumber.ToString("X2") + " C 02 " + iFanModeOne.ToString("X2") + " " + iFanModeTwo.ToString("X2");
            szMessage += " " + CalculateCheckSum(szMessage);
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::FormatChangeFanMessage: message is '" + szMessage + "'");
            //	Log the information to Homeseer
            clsEnviracomApp.LogInformation("Transmit change fan message: '" + szMessage + "'");
            return szMessage;
        }

        #endregion

        #region "  Public Format Query Message Methods   "

        public string FormatQueryFilterMessage(int iZoneNumber)
        {
            string szMessage = "M 10D0 " + iZoneNumber.ToString("X2") + " Q 00";
            szMessage += " " + CalculateCheckSum(szMessage);
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::FormatQueryFilterMessage: message is '" + szMessage + "'");
            //	Log the information to Homeseer
            clsEnviracomApp.LogInformation("Transmit filter message: '" + szMessage + "'");
            return szMessage;
        }

        public string FormatQueryTempMessage(int iZoneNumber)
        {
            string szMessage = "M 12C0 " + iZoneNumber.ToString("X2") + " Q 00";
            szMessage += " " + CalculateCheckSum(szMessage);
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::FormatQueryTempMessage: message is '" + szMessage + "'");
            //	Log the information to Homeseer
            clsEnviracomApp.LogInformation("Transmit query temp message: '" + szMessage + "'");
            return szMessage;
        }

        public string FormatQuerySetPointsMessage(int iZoneNumber)
        {
            string szMessage = "M 2330 " + iZoneNumber.ToString("X2") + " Q 00";
            szMessage += " " + CalculateCheckSum(szMessage);
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::FormatQuerySetPointsMessage: message is '" + szMessage + "'");
            //	Log the information to Homeseer
            clsEnviracomApp.LogInformation("Transmit query set points message: '" + szMessage + "'");
            return szMessage;
        }

        public string FormatQuerySystemSwitchMessage(int iZoneNumber)
        {
            string szMessage = "M 22D0 " + iZoneNumber.ToString("X2") + " Q 00";
            szMessage += " " + CalculateCheckSum(szMessage);
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::FormatQuerySystemSwitchMessage: message is '" + szMessage + "'");
            //	Log the information to Homeseer
            clsEnviracomApp.LogInformation("Transmit query system switch message: '" + szMessage + "'");
            return szMessage;
        }

        public string FormatQueryZoneManagerMessage()
        {
            string szMessage = "M 3EE0 00 Q 00";
            szMessage += " " + CalculateCheckSum(szMessage);
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::FormatQueryZoneManagerMessage: message is '" + szMessage + "'");
            //	Log the information to Homeseer
            clsEnviracomApp.LogInformation("Transmit query zone manager message: '" + szMessage + "'");
            return szMessage;
        }

        public string FormatQuerySystemStateMessage(int iZoneNumber)
        {
            string szMessage = "M 3110 " + iZoneNumber.ToString("X2") + " Q 00";
            szMessage += " " + CalculateCheckSum(szMessage);
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::FormatQuerySystemStateMessage: message is '" + szMessage + "'");
            //	Log the information to Homeseer
            clsEnviracomApp.LogInformation("Transmit query system state message: '" + szMessage + "'");
            return szMessage;
        }

        public string FormatQuerySetPointLimitsMessage(int iZoneNumber)
        {
            string szMessage = "M 1040 " + iZoneNumber.ToString("X2") + " Q 00";
            szMessage += " " + CalculateCheckSum(szMessage);
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::FormatQuerySetPointLimitsMessage: message is '" + szMessage + "'");
            //	Log the information to Homeseer
            clsEnviracomApp.LogInformation("Transmit query set point limits message: '" + szMessage + "'");
            return szMessage;
        }

        public string FormatQuerySchedulesMessage(int iZoneNumber)
        {
            string szMessage = "M 1F80 " + iZoneNumber.ToString("X2") + " Q 01 00";
            szMessage += " " + CalculateCheckSum(szMessage);
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::FormatQueryAllSchedulesMessage: message is '" + szMessage + "'");
            //	Log the information to Homeseer
            clsEnviracomApp.LogInformation("Transmit query all schedules message: '" + szMessage + "'");
            return szMessage;
        }

        public string FormatQueryUnknownCMessage()
        {
            string szMessage = "M 3C00 00 Q 01 00";
            szMessage += " " + CalculateCheckSum(szMessage);
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::FormatQueryUnknwonCMessage: message is '" + szMessage + "'");
            //	Log the information to Homeseer
            clsEnviracomApp.LogInformation("Transmit query unknownc message: '" + szMessage + "'");
            return szMessage;
        }
        #endregion


        #endregion

        #endregion

        #region "  Private Methods   "

        #region "  Private Receive Message Methods   "


        private void ReceiveZoneManagerMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField], strMsg.dlen) == false)
                return;

            int iZoneManager = Convert.ToInt32(sList[MsgDataByteZoneManager + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveZoneManagerMessage: zone manager '" + iZoneManager + "'");

            m_objApp.ProcessZoneManagerMessage(strMsg.unit, iZoneManager);
            TraceReceivedMessage(sList, strMsg);
        }

        private void ReceiveBlowerFanStatusMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField], strMsg.dlen) == false)
                return;

            int iBlowerFanSpeed = Convert.ToInt32(sList[MsgDataByteBlowerFanSpeed + MsgStartDataBytesField], 16);
            int iUnknown_1 = Convert.ToByte(sList[MsgDataByteUnknown_1 + MsgStartDataBytesField], 16);
            int iUnknown_2 = Convert.ToByte(sList[MsgDataByteUnknown_2 + MsgStartDataBytesField], 16);
            int iUnknown_3 = Convert.ToByte(sList[MsgDataByteUnknown_3 + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveBlowerFanStatusMessage: blower fan speed is '" + iBlowerFanSpeed + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveBlowerFanStatusMessage: unknown byte one is '" + iUnknown_1 + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveBlowerFanStatusMessage: unknown byte two is '" + iUnknown_2 + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveBlowerFanStatusMessage: unknown byte three is '" + iUnknown_3 + "'");

            m_objApp.ProcessBlowerFanSpeedMessage(strMsg.unit, iBlowerFanSpeed, iUnknown_1, iUnknown_2, iUnknown_3);
        }

        private void ReceiveAuxHeatStatusMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iAuxHeatStatus = Convert.ToInt32(sList[MsgDataByteAuxHeatStatus + MsgStartDataBytesField], 16);
            int iUnknown_1 = Convert.ToByte(sList[MsgDataByteUnknown_1 + MsgStartDataBytesField], 16);
            int iUnknown_2 = Convert.ToByte(sList[MsgDataByteUnknown_2 + MsgStartDataBytesField], 16);
            int iUnknown_3 = Convert.ToByte(sList[MsgDataByteUnknown_3 + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveAuxHeatStatusMessage: aux heat status is '" + iAuxHeatStatus + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveAuxHeatStatusMessage: unknown byte one is '" + iUnknown_1 + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveAuxHeatStatusMessage: unknown byte two is '" + iUnknown_2 + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveAuxHeatStatusMessage: unknown byte three is '" + iUnknown_3 + "'");

            m_objApp.ProcessAuxHeatStatusMessage(strMsg.unit, iAuxHeatStatus, iUnknown_1, iUnknown_2, iUnknown_3);
        }

        private void ReceiveCoolingHeatPumpStatusMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iCoolingHeatPumpState = Convert.ToInt32(sList[MsgDataByteCoolingHeatPumpState + MsgStartDataBytesField], 16);
            int iUnknown_1 = Convert.ToByte(sList[MsgDataByteUnknown_1 + MsgStartDataBytesField], 16);
            int iUnknown_2 = Convert.ToByte(sList[MsgDataByteUnknown_2 + MsgStartDataBytesField], 16);
            int iUnknown_3 = Convert.ToByte(sList[MsgDataByteUnknown_3 + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveCoolingHeatPumpStatusMessage: cooling heat pump state is '" + iCoolingHeatPumpState + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveCoolingHeatPumpStatusMessage: unknown byte one is '" + iUnknown_1 + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveCoolingHeatPumpStatusMessage: unknown byte two is '" + iUnknown_2 + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveCoolingHeatPumpStatusMessage: unknown byte three is '" + iUnknown_3 + "'");

            m_objApp.ProcessCoolingHeatPumpStatusMessage(strMsg.unit, iCoolingHeatPumpState, iUnknown_1, iUnknown_2, iUnknown_3);
        }


        private void ReceiveHeatingHeatPumpStatusMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iHeatingHeatPumpState = Convert.ToInt32(sList[MsgDataByteHeatingHeatPumpState + MsgStartDataBytesField], 16);
            int iUnknown_1 = Convert.ToByte(sList[MsgDataByteUnknown_1 + MsgStartDataBytesField], 16);
            int iUnknown_2 = Convert.ToByte(sList[MsgDataByteUnknown_2 + MsgStartDataBytesField], 16);
            int iUnknown_3 = Convert.ToByte(sList[MsgDataByteUnknown_3 + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveHeatingHeatPumpStatusMessage: heating heat pump state is '" + iHeatingHeatPumpState + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveHeatingHeatPumpStatusMessage: unknown byte one is '" + iUnknown_1 + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveHeatingHeatPumpStatusMessage: unknown byte two is '" + iUnknown_2 + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveHeatingHeatPumpStatusMessage: unknown byte three is '" + iUnknown_3 + "'");

            m_objApp.ProcessHeatingHeatPumpStatusMessage(strMsg.unit, iHeatingHeatPumpState, iUnknown_1, iUnknown_2, iUnknown_3);
        }

        private void ReceiveCirculateFanMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField], strMsg.dlen) == false)
                return;

            int iZoneNumber = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iUnknown_0 = Convert.ToInt32(sList[MsgDataByteUnknown_0 + MsgStartDataBytesField], 16);
            int iCirculateFanStatus = Convert.ToByte(sList[MsgDataByteFanStatus + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveCirculateFanMessage: unknown byte zero is '" + iUnknown_0 + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveCirculateFanMessage: fan state is '" + iCirculateFanStatus + "'");

            m_objApp.ProcessCirculateFanMessage(strMsg.unit, iZoneNumber, iUnknown_0, iCirculateFanStatus);
        }


        // 3180 Message-Damper Status (by zone) 
        // Message Id - - - - - - - - 3180 
        // Instance Id (Zone Id)- - 01 - - Zone 1 <> Zone 9
        // Type - - - - - - - - - - - - - R 
        // Num Data Bytes - - - - - 01 
        // Data Byte 1- - - - - - - - N - - -00-Damper Closed, C8-Damper Open
        // Checksum - - - - - - - - - x 
        // Example M 3180 01 R 01 C8 79 - -Zone 1 damper open
        private void ReceiveDamperStatusMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField], strMsg.dlen) == false)
                return;

            int iZoneNumber = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iDamperStatus = Convert.ToInt32(sList[MsgDataByteDamperStatus + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveDamperStatusMessage: damper state is '" + iDamperStatus + "'");

            m_objApp.ProcessDamperStatusMessage(strMsg.unit, iZoneNumber, iDamperStatus);
        }

        private void ReceiveAquastatCirculatingPumpStatusMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField], strMsg.dlen) == false)
                return;

            int iZoneNumber = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iDamperStatus = Convert.ToInt32(sList[MsgDataByteDamperStatus + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveDamperStatusMessage: damper state is '" + iDamperStatus + "'");

            m_objApp.ProcessAquastatCirculatingPumpStatusMessage(strMsg.unit, iZoneNumber, iDamperStatus);
        }



        private void ReceiveDehumidifierStateMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iZoneNumber = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iDehumidifierMode = Convert.ToInt32(sList[MsgDataByteDehumidifierMode + MsgStartDataBytesField], 16);
            int iExternalDehumidifierState = Convert.ToByte(sList[MsgDataByteExternalDehumidifierState + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveDehumidifierStateMessage: dehumidifier mode is '" + iDehumidifierMode + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveDehumidifierStateMessage: external dehumidifier state is '" + iExternalDehumidifierState + "'");

            m_objApp.ProcessDehumidifierStateMessage(strMsg.unit, iZoneNumber, iDehumidifierMode, iExternalDehumidifierState);
        }

        private void ReceiveHumidifierStateMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iZoneNumber = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iUnknown_0 = Convert.ToInt32(sList[MsgDataByteUnknown_0 + MsgStartDataBytesField], 16);
            int iHumidifierState = Convert.ToByte(sList[MsgDataByteHumidifierState + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveHumidifierStateMessage: iDehumidifySetPoint is '" + iUnknown_0 + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveHumidifierStateMessage: external humidifier state is '" + iHumidifierState + "'");

            m_objApp.ProcessHumidifierStateMessage(strMsg.unit, iZoneNumber, iUnknown_0, iHumidifierState);
        }

        private void ReceiveExternalVentilationStateMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iZoneNumber = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iUnknown_0 = Convert.ToInt32(sList[MsgDataByteUnknown_0 + MsgStartDataBytesField], 16);
            int iExternalVentilationState = Convert.ToByte(sList[MsgDataByteExternalVentilationState + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveExternalVentilationStateMessage: unknown byte zero is '" + iUnknown_0 + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveExternalVentilationStateMessage: external ventilation state is '" + iExternalVentilationState + "'");

            m_objApp.ProcessExternalVentilationStateMessage(strMsg.unit, iZoneNumber, iUnknown_0, iExternalVentilationState);
        }

        private void ReceiveTimeMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iZoneNumber = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iSeconds = Convert.ToInt32(sList[MsgDataByteSeconds + MsgStartDataBytesField], 16);
            int iMinutes = Convert.ToInt32(sList[MsgDataByteMinutes + MsgStartDataBytesField], 16);
            int iDayOfWeek = (Convert.ToByte(sList[MsgDataByteHourDow + MsgStartDataBytesField], 16) & 0xE0) >> 5;
            int iHours = (Convert.ToByte(sList[MsgDataByteHourDow + MsgStartDataBytesField], 16) & 0x1F);
            int iDay = Convert.ToInt32(sList[MsgDataByteDay + MsgStartDataBytesField], 16);
            int iMonth = Convert.ToInt32(sList[MsgDataByteMonth + MsgStartDataBytesField], 16);
            int iYear = (Convert.ToInt32(sList[MsgDataByteYearHi + MsgStartDataBytesField] + sList[MsgDataByteYearLo + MsgStartDataBytesField], 16));

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveTimeMessage: date and time -- day of week '" + iDayOfWeek + " -- " + iMonth + "/" + iDay + "/" + iYear + " " + iHours.ToString("00") + ":" + iMinutes.ToString("00") + ":" + iSeconds.ToString("00") + "'");
            m_objApp.ProcessTimeMessage(strMsg.unit, iZoneNumber, iDayOfWeek, iMonth, iDay, iYear, iHours, iMinutes, iSeconds);
        }

        private void ReceiveErrorMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iZoneNumber = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iUnknown_0 = Convert.ToInt32(sList[MsgDataByteUnknown_1 + MsgStartDataBytesField], 16);
            int iUnknown_1 = Convert.ToByte(sList[MsgDataByteUnknown_2 + MsgStartDataBytesField], 16);
            int iErrorNumber = Convert.ToByte(sList[MsgDataByteErrorNumber + MsgStartDataBytesField], 16);
            int iUnknown_3 = Convert.ToByte(sList[MsgDataByteUnknown_4 + MsgStartDataBytesField], 16);
            int iUnknown_4 = Convert.ToByte(sList[MsgDataByteUnknown_5 + MsgStartDataBytesField], 16);
            int iUnknown_5 = Convert.ToByte(sList[MsgDataByteUnknown_6 + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveErrorMessage: Error number: '" + iErrorNumber + "'");

            m_objApp.ProcessErrorMessage(strMsg.unit, iZoneNumber, iUnknown_0, iUnknown_1, iErrorNumber, iUnknown_3, iUnknown_4, iUnknown_5);
        }

        private void ReceiveSystemStateMessage(string[] sList, Message strMsg)
        {
            // NOTE:  This message has a variable number of databytes depending on the system configuration -- 4 is the observed minimum so far

            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iZoneNumber     = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iUnknown_0      = Convert.ToInt32(sList[MsgDataByteUnknown_0 + MsgStartDataBytesField], 16);
            int iUnknown_1      = Convert.ToInt32(sList[MsgDataByteUnknown_1 + MsgStartDataBytesField], 16);
            int SystemState     = Convert.ToInt32(sList[MsgDataByteSystemState + MsgStartDataBytesField], 16);
            int StageOneState   = 0;
            int StageTwoState   = 0;
            int StageThreeState = 0;
            int StageFourState  = 0;

            int iNumberDataBytes = Convert.ToInt32(sList[MsgNumDataBytesField], 16);

            if (iNumberDataBytes > 3)
                StageOneState   = Convert.ToInt32(sList[MsgDataByteStageOneState + MsgStartDataBytesField], 16);
            
            if (iNumberDataBytes > 4)
                StageTwoState = Convert.ToInt32(sList[MsgDataByteStageTwoState + MsgStartDataBytesField], 16);

            if (iNumberDataBytes > 5)
                StageThreeState = Convert.ToInt32(sList[MsgDataByteStageThreeState + MsgStartDataBytesField], 16);

            if (iNumberDataBytes > 6)
                StageFourState = Convert.ToInt32(sList[MsgDataByteStageFourState + MsgStartDataBytesField], 16);


            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveSystemStateMessage: MsgDataByteUnknown_1 is '" + iUnknown_0 + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveSystemStateMessage: MsgDataByteUnknown_2 is '" + iUnknown_1 + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveSystemStateMessage: MsgDataByteSystemState is '" + SystemState + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveSystemStateMessage: MsgDataByteStageOneState is '" + StageOneState + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveSystemStateMessage: MsgDataByteStageTwoState is '" + StageTwoState + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveSystemStateMessage: MsgDataByteStageThreeState is '" + StageThreeState + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveSystemStateMessage: MsgDataByteStageFourState is '" + StageFourState + "'");

            m_objApp.ProcessSystemStateMessage(strMsg.unit, iZoneNumber, iUnknown_0, iUnknown_1, SystemState, StageOneState, StageTwoState, StageThreeState, StageFourState);
        }

        private void ReceiveEquipmentFaultMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iZoneNumber = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iUnknown_0 = Convert.ToInt32(sList[MsgDataByteUnknown_0 + MsgStartDataBytesField], 16);
            int iUnknown_1 = Convert.ToByte(sList[MsgDataByteUnknown_1 + MsgStartDataBytesField], 16);
            int iUnknown_2 = Convert.ToByte(sList[MsgDataByteUnknown_2 + MsgStartDataBytesField], 16);
            int iUnknown_3 = Convert.ToByte(sList[MsgDataByteUnknown_3 + MsgStartDataBytesField], 16);
            int iUnknown_4 = Convert.ToByte(sList[MsgDataByteUnknown_4 + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveEquipmentFaultMessage: unknown byte zero is '" + iUnknown_0 + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveEquipmentFaultMessage: unknown byte one is '" + iUnknown_1 + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveEquipmentFaultMessage: unknown byte two is '" + iUnknown_2 + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveEquipmentFaultMessage: unknown byte three is '" + iUnknown_3 + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveEquipmentFaultMessage: unknown byte four is '" + iUnknown_4 + "'");

            m_objApp.ProcessEquipmentFaultMessage(strMsg.unit, iZoneNumber, iUnknown_0, iUnknown_1, iUnknown_2, iUnknown_3, iUnknown_4);
        }

        private void ReceiveOutdoorTempMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iZoneNumber = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iOutdoorTemperature = Convert.ToInt16(sList[MsgDataByteOutdoorTempHi + MsgStartDataBytesField] + sList[MsgDataByteOutdoorTempLo + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveOutdoorTempMessage: outdoor temperature is '" + iOutdoorTemperature + "'");

            m_objApp.ProcessOutdoorTempMessage(strMsg.unit, iZoneNumber, iOutdoorTemperature);
        }

        private void ReceiveSetPointMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iZoneNumber = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iHeatSetPoint = Convert.ToInt32(sList[MsgDataByteHeatSetPointHi + MsgStartDataBytesField] + sList[MsgDataByteHeatSetPointLo + MsgStartDataBytesField], 16);
            int iHoldStatus = Convert.ToInt32(sList[MsgDataByteHoldStatus + MsgStartDataBytesField], 16);
            int iCoolSetPoint = Convert.ToInt32(sList[MsgDataByteCoolSetPointHi + MsgStartDataBytesField] + sList[MsgDataByteCoolSetPointLo + MsgStartDataBytesField], 16);
            int iUnknown_5 = Convert.ToByte(sList[MsgDataByteUnknown_5 + MsgStartDataBytesField], 16);
            int iRecovery = (Convert.ToByte(sList[MsgDataByteRecovery + MsgStartDataBytesField], 16) & 0xF0) >> 4;
            int iPeriod = (Convert.ToByte(sList[MsgDataByteRecovery + MsgStartDataBytesField], 16) & 0x0F);
            int iUnknown_7 = Convert.ToByte(sList[MsgDataByteUnknown_7 + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveSetPointMessage: heat set point is '" + iHeatSetPoint + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveSetPointMessage: hold status is '" + iHoldStatus + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveSetPointMessage: cool set point is '" + iCoolSetPoint + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveSetPointMessage: unknown byte five is '" + iUnknown_5 + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveSetPointMessage: recovery is '" + iRecovery + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveSetPointMessage: period is '" + iPeriod + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveSetPointMessage: unknown byte seven is '" + iUnknown_7 + "'");

            m_objApp.ProcessSetPointMessage(strMsg.unit, iZoneNumber, iHeatSetPoint, iHoldStatus, iCoolSetPoint, iUnknown_5, iRecovery, iUnknown_7);
        }
        private void ReceiveSystemSwitchMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iZoneNumber = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iSystemSwitch = (Convert.ToInt32(sList[MsgDataByteSystemSwitch + MsgStartDataBytesField], 16) & 0x07);
            int iAutoCurrentMode = ((Convert.ToInt32(sList[MsgDataByteSystemSwitch + MsgStartDataBytesField], 16) & 0x08) >> 3);
            int iExtraModeByte = ((Convert.ToInt32(sList[MsgDataByteSystemSwitch + MsgStartDataBytesField], 16) & 0xF0) >> 4);
            int iAllowedModes = Convert.ToInt32(sList[MsgDataByteAllowedModesHi + MsgStartDataBytesField] + sList[MsgDataByteAllowedModesLo + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveSystemSwitchMessage: system switch is '" + iSystemSwitch + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveSystemSwitchMessage: auto current mode is '" + iAutoCurrentMode + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveSystemSwitchMessage: extra mode bits are '" + iExtraModeByte + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveSystemSwitchMessage: allowed modes is '" + iAllowedModes + "'");

            m_objApp.ProcessSystemSwitchMessage(strMsg.unit, iZoneNumber, iSystemSwitch, iAutoCurrentMode, iExtraModeByte, iAllowedModes);
        }
        private void ReceiveFanSwitchMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iZoneNumber = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iFanModeOne = Convert.ToInt32(sList[MsgDataByteFanModeOne + MsgStartDataBytesField], 16);
            int iFanModeTwo = Convert.ToInt32(sList[MsgDataByteFanModeTwo + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveFanSwitchMessage: MsgDataByteFanModeOne is '" + iFanModeOne + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveFanSwitchMessage: MsgDataByteFanModeTwo is '" + iFanModeTwo + "'");

            m_objApp.ProcessFanSwitchMessage(strMsg.unit, iZoneNumber, iFanModeOne, iFanModeTwo);
        }
        private void ReceiveDehumidifierSetPointMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iZoneNumber = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iDehumidifySetPoint = Convert.ToInt32(sList[MsgDataByteDeHumidifySetPoint + MsgStartDataBytesField], 16);
            int iDehumidifyUpperLimit = Convert.ToInt32(sList[MsgDataByteDeHumidifyUpperLimit + MsgStartDataBytesField], 16);
            int iDehumidifyLowerLimit = Convert.ToInt32(sList[MsgDataByteDeHumidifyLowerLimit + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveDehumidifierSetPointMessage: dehumidifier set point is '" + iDehumidifySetPoint + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveDehumidifierSetPointMessage: dehumidifier upper limit is '" + iDehumidifyUpperLimit + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveDehumidifierSetPointMessage: dehumidifier lower limit is '" + iDehumidifyLowerLimit + "'");

            m_objApp.ProcessDehumidifierSetPointMessage(strMsg.unit, iZoneNumber, iDehumidifySetPoint, iDehumidifyUpperLimit, iDehumidifyLowerLimit);

        }
        private void ReceiveHumidifierSetPointMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iZoneNumber = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iHumiditySetPoint = Convert.ToInt32(sList[MsgDataByteHumiditySetPoint + MsgStartDataBytesField], 16);
            int iHumidityUpperLimit = Convert.ToInt32(sList[MsgDataByteHumidityUpperLimit + MsgStartDataBytesField], 16);
            int iHumidityLowerLimit = Convert.ToInt32(sList[MsgDataByteHumidityLowerLimit + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveHumidifierSetPointMessage: humidifier set point is '" + iHumiditySetPoint + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveHumidifierSetPointMessage: humidifier upper limit is '" + iHumidityUpperLimit + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveHumidifierSetPointMessage: humidifier lower limit is '" + iHumidityLowerLimit + "'");

            m_objApp.ProcessHumidifierSetPointMessage(strMsg.unit, iZoneNumber, iHumiditySetPoint, iHumidityUpperLimit, iHumidityLowerLimit);

        }

        private void ReceiveScheduleMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iZoneNumber = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iSchedulePeriod = ((Convert.ToInt32(sList[MsgDataBytePeriodDay + MsgStartDataBytesField], 16) & 0xF0) >> 4);
            int iScheduleDay = (Convert.ToInt32(sList[MsgDataBytePeriodDay + MsgStartDataBytesField], 16) & 0x0F);
            int iScheduleTime = (Convert.ToInt32(sList[MsgDataBytePeriodTimeHi + MsgStartDataBytesField] + sList[MsgDataBytePeriodTimeLo + MsgStartDataBytesField], 16) & 0x07FF);
            int iHeatSetPoint = Convert.ToInt32(sList[MsgDataByteScheduleHeatSetPointHi + MsgStartDataBytesField] + sList[MsgDataByteScheduleHeatSetPointLo + MsgStartDataBytesField], 16);
            int iCoolSetPoint = Convert.ToInt32(sList[MsgDataByteScheduleCoolSetPointHi + MsgStartDataBytesField] + sList[MsgDataByteScheduleCoolSetPointLo + MsgStartDataBytesField], 16);
            int iFanMode = Convert.ToInt32(sList[MsgDataByteFanMode + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveScheduleMessage: MsgDataBytePeriodDay (period) is '" + iSchedulePeriod + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveScheduleMessage: MsgDataBytePeriodDay (day) is '" + iScheduleDay + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveScheduleMessage: MsgDataBytePeriodTime is '" + iScheduleTime + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveScheduleMessage: MsgDataByteHeatSetPoint is '" + iHeatSetPoint + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveScheduleMessage: MsgDataByteCoolSetPoint is '" + iCoolSetPoint + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveScheduleMessage: MsgDataByteFanMode is '" + iFanMode + "'");

            m_objApp.ProcessScheduleMessage(strMsg.unit, iZoneNumber, iScheduleDay, iSchedulePeriod, iScheduleTime, iHeatSetPoint, iCoolSetPoint, iFanMode);
        }

        private void ReceiveRoomTempMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iZoneNumber = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iRoomTemperature = Convert.ToInt32(sList[MsgDataByteRoomTemp + MsgStartDataBytesField], 16);
            bool bRoomSensorFault = (Convert.ToByte(sList[MsgDataByteSensorFaultandUnits + MsgStartDataBytesField], 16) & 0x80) == 0x80;
            bool bRoomDegreeCelsius = (Convert.ToByte(sList[MsgDataByteSensorFaultandUnits + MsgStartDataBytesField], 16) & 0x01) == 0x01;

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveRoomTempMessage: temperature is '" + iRoomTemperature + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveRoomTempMessage: sensor fault is '" + bRoomSensorFault + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveRoomTempMessage: degrees unit is '" + (bRoomDegreeCelsius == true ? "Celsius" : "Fahrenheit") + "'");

            m_objApp.ProcessRoomTempMessage(strMsg.unit, iZoneNumber, iRoomTemperature, bRoomSensorFault, bRoomDegreeCelsius);
        }

        private void ReceiveRoomHumidityMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iZoneNumber = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iRoomHumidity = Convert.ToInt32(sList[MsgDataByteRoomHumidity + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveRoomHumidityMessage: humidity is '" + iRoomHumidity + "'");

            m_objApp.ProcessRoomHumidityMessage(strMsg.unit, iZoneNumber, iRoomHumidity);
        }

        private void ReceiveAirFilterTimerMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            if (VerifyMessageDataLength(sList[MsgNumDataBytesField],  strMsg.dlen) == false)
                return;

            int iZoneNumber = Convert.ToInt32(sList[MsgInstanceIdField], 16);
            SeenZoneInstance(strMsg.unit,iZoneNumber);

            int iRemainingFilterTime = Convert.ToInt32(sList[MsgDataByteRemainingFilterTime + MsgStartDataBytesField], 16);
            int iTotalFilterTime = Convert.ToByte(sList[MsgDataByteTotalFilterTime + MsgStartDataBytesField], 16);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveAirFilterMessage: remaining filter time is '" + iRemainingFilterTime + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::ReceiveAirFilterMessage: total filter time is '" + iTotalFilterTime + "'");

            m_objApp.ProcessAirFilterMessage(strMsg.unit, iZoneNumber, iRemainingFilterTime, iTotalFilterTime);
        }
        #endregion


        #region "  Private Internal Methods   "
        private void SeenZoneInstance(int iUnitNumber, int iZoneNumber)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::SeenZoneInstance: saw zone instance id: '" + iZoneNumber + "' on unit: '" + iUnitNumber + "'");
            m_objApp.SeenZoneInstance(iUnitNumber, iZoneNumber);
        }

        private string FormatWordString(int iWordValue)
        {
            byte bHi = (byte)((iWordValue & 0xFF00) >> 8);
            byte bLo = (byte)(iWordValue & 0x00FF);
            return bHi.ToString("X2") + " " + bLo.ToString("X2");
        }

        private void AddMessageToLog(string sMessage)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(m_objApp.ifHSApp.GetAppPath() + "\\" + LOG_MESSAGES))
                {
                    sw.WriteLine(sMessage);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception)
            {
                //	Do nothing, this file is info only so not worried if it
                //	doesn't get created or has errors
            }
        }

        private string CalculateCheckSum(string sMessage)
        {
            string[] sList = sMessage.Split(' ');

            byte Chksum = 0;

            try
            {
                switch (sList[0])
                {
                    case "H":
                        Chksum = 0x80;
                        break;

                    case "M":
                        Chksum = 0x40;
                        break;

                    case "L":
                        Chksum = 0x00;
                        break;

                    default:
                        clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::CalculateCheckSum: bad priority: " + sList[0]);
                        return "00";
                }
                UInt16 temp = Convert.ToUInt16(sList[1], 16); // process the message class id as two seperate bytes
                Chksum ^= (byte)((temp & 0xFF00) >> 8);
                Chksum ^= (byte)(temp & 0x00FF);
                Chksum ^= Convert.ToByte(sList[2], 16); // process the instance number

                // process the service
                switch (sList[3])
                {
                    case "C":
                        Chksum ^= 0x80;
                        break;

                    case "R":
                        Chksum ^= 0x40;
                        break;

                    case "Q":
                        Chksum ^= 0x00;
                        break;

                    default:
                        clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::CalculateCheckSum: bad service type: " + sList[3]);
                        return "00";
                }
                Chksum ^= Convert.ToByte(sList[4], 16); // process the count of databytes
                // process the data bytes
                for (int i = 5; i <= (sList.GetUpperBound(0)); i++)
                {
                    Chksum ^= Convert.ToByte(sList[i], 16); // process each databyte
                }

            }
            catch (Exception ex)
            {
                //	This isn't good, log it appropriately
                clsEnviracomApp.TraceEvent(TraceEventType.Error, "clsMessageMgr::CalculateCheckSum: Reporting exception: " + ex.ToString() + " during checksum calculation");
                return "00";
            }
            return Chksum.ToString("X2");
        }

        private bool VerifyCheckSum(string[] sList)
        {
            byte Chksum = 0;

            try
            {
                // process the prioity character
                switch (sList[0])
                {
                    case "H":
                        Chksum = 0x80;
                        break;

                    case "M":
                        Chksum = 0x40;
                        break;

                    case "L":
                        Chksum = 0x00;
                        break;

                    default:
                        clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::VerifyCheckSum: bad priority: " + sList[0]);
                        return false;
                }

                UInt16 temp = Convert.ToUInt16(sList[1], 16); // process the message class id as two seperate bytes

                Chksum ^= (byte)((temp & 0xFF00) >> 8);
                Chksum ^= (byte)(temp & 0x00FF);

                Chksum ^= Convert.ToByte(sList[2], 16); // process the instance number

                // process the service
                switch (sList[3])
                {
                    case "C":
                        Chksum ^= 0x80;
                        break;

                    case "R":
                        Chksum ^= 0x40;
                        break;

                    case "Q":
                        Chksum ^= 0x00;
                        break;

                    default:
                        clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::VerifyCheckSum: bad service type: " + sList[3]);
                        return false;
                }

                Chksum ^= Convert.ToByte(sList[4],16); // process the count of databytes
                // process the data bytes
                for (int i = 5; i < (sList.GetUpperBound(0)); i++)
                {
                    Chksum ^= Convert.ToByte(sList[i], 16); // process each databyte
                }

            }
            catch (Exception ex)
            {
                //	This isn't good, log it appropriately
                clsEnviracomApp.TraceEvent(TraceEventType.Error, "clsMessageMgr::VerifyCheckSum: Reporting exception: " + ex.ToString() + " during checksum validation");
                return false;
            }
            return (Chksum == Convert.ToByte(sList[sList.GetUpperBound(0)], 16)); // confirm the checksums match
        }

        private bool VerifyMessageDataLength(string sNumBytes, int iExpectedBytes)
        {
            if (Convert.ToInt32(sNumBytes, 16) < iExpectedBytes)
            {
                //	This isn't good, log it appropriately
                clsEnviracomApp.TraceEvent(TraceEventType.Error, "clsMessageMgr::VerifyMessageDataLength: message data bytes less than expected minumum: '" + sNumBytes + "' < '" + iExpectedBytes + "'");
                return false;
            }
            else
                return true;
        }

        private void TraceReceivedMessage(string[] sList, Message strMsg)
        {
            if (VerifyCheckSum(sList) == false)
                return;
            VerifyMessageDataLength(sList[MsgNumDataBytesField], strMsg.dlen);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::TraceReceivedMessage: message priority is '" + sList[MsgPriorityField] + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::TraceReceivedMessage: message class id is '" + sList[MsgClassIdField] + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::TraceReceivedMessage: message instance id is '" + sList[MsgInstanceIdField] + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::TraceReceivedMessage: message type is '" + sList[MsgTypeField] + "'");
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::TraceReceivedMessage: message number of data bytes is '" + sList[MsgNumDataBytesField] + "'");

            for (int i = MsgStartDataBytesField; i < (sList.GetUpperBound(0)); i++)
               clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::TraceReceivedMessage: message data byte(" + (i - MsgStartDataBytesField) + ") is '" + sList[i] + "'");

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsMessageMgr::TraceReceivedMessage: message checksum is '" + sList[sList.GetUpperBound(0)] + "'");
            //	Log the information to Homeseer
            clsEnviracomApp.LogInformation("Received Message: '" + strMsg.text + " as " + strMsg.desc + "'");
        }
        #endregion
        #endregion
    }
}
