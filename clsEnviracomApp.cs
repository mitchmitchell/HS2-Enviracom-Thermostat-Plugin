using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using Scheduler;
using Scheduler.Classes;

namespace HSPI_ENVIRACOM_MANAGER
{
	class clsEnviracomApp
	{
		#region "  Enumerations and Constants   "

        private enum Status
        {
            UNKNOWN = 0,
            CONNECTED = 1,
            EQUIPMENT_FAULT = 2
        };
        //	Public string used for devices to identify them as belonging to 
        //	this plug-in
		public const string		PLUGIN_IN_NAME_SHORT =	"Enviracom Manager";

		#region "    Configuration Enums and Constants  "
		//	Config file m_sUnitName
		private const string	CFG_FILE =				@"config\enviracomhvac.config";

		//	Configuration keys
        public const string     CFG_KEY_ISZONED = "envZoned";
        public const string     CFG_KEY_LOGLEVEL = "logLevel";
        public const string     CFG_KEY_MAXUNITS = "numUnits";
        public const string     CFG_KEY_UNITPORT = "UnitPort_";
        public const string     CFG_KEY_MSGDELAY = "msgDelay";
        public const string     CFG_KEY_MSGTIMEOUT = "msgTimeOut";
        public const string     CFG_KEY_MSGWAITACK = "msgWaitAck";
        public const string     CFG_KEY_BAUDRATE = "msgBaudRate";

		//	Configuration values
		public const string		CFG_LOGLEVEL_NONE =		"0";
		public const string		CFG_LOGLEVEL_CRITICAL =	"1";
		public const string		CFG_LOGLEVEL_ERROR =	"2";
		public const string		CFG_LOGLEVEL_WARNING =	"3";
		public const string		CFG_LOGLEVEL_INFO =		"4";
        public const string     CFG_LOGLEVEL_VERBOSE =  "5";
        #endregion

		//	Private constants
        private const int       HVAC_MAXIMUM_DEVCODE = 99;
        private const string    LOG_PREFIX = "enviracom_messages_";
        private const string    HVAC_MASTER_LOCATION = "HVAC";
        private const string    HVAC_MASTER_TYPE = "Enviracom Master";
        private const string    HVAC_MASTER_NAME = "Enviracom Master Device";
        private const string    HVAC_MASTER_MAGIC = "Enviracom Master Device";
        private const string    HVAC_MASTER_DEVCODE = "99";

        private const int       PERIODIC_TICK_TIME = 1; // in seconds

        #endregion

		#region "  Members  "
		static clsEnviracomApp						s_objSingletonInstance = null;
		static readonly object						s_objLock = new object();

        private bool                                m_bDeviceAttached = false;
        private DeviceClass                         m_objAttachedDevice = null;

        private int                                 m_iMsgDelay = 0;
        private int                                 m_iMsgTimeOut = 0;
        private int                                 m_iMsgWaitAck = 0;

        private int                                 m_iBaudRate = 19200;
        private TraceEventType                      m_eTraceLevel = TraceEventType.Critical;

        private List<clsHvacController>             m_objHvacUnitList =  new List<clsHvacController>();
        private List<clsThermostatZone>             m_objThermostatList = new List<clsThermostatZone>();
        private PeriodicList<clsResponse>           m_objResponseList = new PeriodicList<clsResponse>(new TimeSpan(0, 0, PERIODIC_TICK_TIME));
        private Configuration                       m_cfgApp = null;	// Configuration access (simple object)
		private TraceSource							m_trcApp = null;	// For diagnostic tracing (simple object)
		private clsHSPI								m_ifHSPI = null;	// Interface to HomeSeer HSPI
		private hsapplication						m_ifHSApp = null;	// Interface to HomeSeer Application
        bool[]                                      m_bUsedDevCodes = new bool[HVAC_MAXIMUM_DEVCODE + 1];

		#endregion

		#region "  Accessor Methods for Members  "
		//	Accessor method to get one and only instance of this Singleton object
		static public clsEnviracomApp GetInstance()
		{
            if (s_objSingletonInstance == null)
                lock (s_objLock)
			    {
                    if (s_objSingletonInstance == null)
                    {
					    s_objSingletonInstance = new clsEnviracomApp();
				    }
                }

            return s_objSingletonInstance;
        }

        public hsapplication ifHSApp
		{
			get { return m_ifHSApp; }
		}

        public string Name
        {
            get
            {
                if (m_bDeviceAttached)
                    return m_objAttachedDevice.Name;
                else
                    return "";
            }
        }

        public string Location
        {
            get
            {
                if (m_bDeviceAttached)
                    return m_objAttachedDevice.location;
                else
                    return "";
            }
        }

        public string DevCode
        {
            get
            {
                if (m_bDeviceAttached == true)
                    return m_objAttachedDevice.dc;
                else
                    return "";
            }
        }

        public string HouseCode
        {
            get
            {
                if (m_bDeviceAttached == true)
                    return m_objAttachedDevice.hc;
                else
                    return "";
            }
        }

        public int MsgDelay
        {
            get { return m_iMsgDelay; }
        }
        public int MsgTimeOut
        {
            get { return m_iMsgTimeOut; }
        }
        public int MsgWaitAck
        {
            get { return m_iMsgWaitAck; }
        }

        public int BaudRate
        {
            get { return m_iBaudRate; }
        }


        public int NumberOfThermostats
        {
            get { return m_objThermostatList.Count; }
        }
        public int NumberOfUnits
        {
            get { return m_objHvacUnitList.Count; }
        }
        public List<clsHvacController> Units
        {
            get { return m_objHvacUnitList; }
        }

        public List<clsThermostatZone> Thermostats
        {
            get { return m_objThermostatList; }
        }

        #endregion

		#region "  Constructor and Destructor   "
		//	Static constructor used because this object is a Singleton
		static clsEnviracomApp()
		{
		}

		//	Primary constructor is private so no one can instantiate this 
		//	Singleton object directly
		private clsEnviracomApp()
		{
		}

		~clsEnviracomApp()
		{
		}
		#endregion

		#region "  Public Methods  "

        public bool AttachHomeseerMasterDevice()
        {
            DeviceClass dev = FindHomeseerMasterDevice();
            if (dev != null)
            {
                AttachDevice(dev);
                return true;
            }
            else
            {
                if (!CreateHomeseerMasterDevice())
                    throw new System.Exception("Failed to create homeseer master device for Enviracom plugin");
                return false;
            }
        }

        public void AttachDevice(DeviceClass dev)
        {
            m_objAttachedDevice = dev;
            SetUsedDevCode(Convert.ToInt32(m_objAttachedDevice.dc));
            m_bDeviceAttached = true;
            ifHSApp.SetDeviceValue(
                m_objAttachedDevice.hc + m_objAttachedDevice.dc,
                (int)Status.CONNECTED);
        }

        public void DetachDevice()
        {
            ifHSApp.SetDeviceValue(
                m_objAttachedDevice.hc + m_objAttachedDevice.dc,
                (int)Status.UNKNOWN);
            m_bDeviceAttached = false;
            FreeDevCode(Convert.ToInt32(m_objAttachedDevice.dc));
            m_objAttachedDevice = null;
        }



		#region "    Initialization and Cleanup  "
		public void Initialize()
		{
           //	Now that we have the HomeSeer object, open up the configuration
            InitConfiguration();

            //	Init our tracing capability				
            InitTrace();
            InitUnits();

            //	Trace this event after we init the trace (otherwise it won't show up)
            TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::Initialize");
		}

		public void Cleanup()
		{
            TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::Cleanup");

            CleanupUnits();
			if ( m_trcApp != null )
			{
                m_trcApp.Flush();
				m_trcApp.Close();
				m_trcApp = null;
			}
			m_ifHSApp = null;
			m_ifHSPI = null;
        }

        public void HSEvent(object[] parms)
        {
            int nRefCode;
            DeviceClass dev;

            TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::HSEvent called, params[0]: " + parms[0].ToString() + " params[1]: " + parms[1].ToString() + " params[2]: " + parms[2].ToString() + " params[3]: " + parms[3].ToString());

            try
            {
                //	Check if the object being referenced is from my plug-in
                nRefCode = (int)parms[3];
                dev = ifHSApp.GetDeviceByRef(nRefCode);
                if (dev == null)
                    return;
                if (dev.@interface != clsEnviracomApp.PLUGIN_IN_NAME_SHORT)
                    return;
            }
            catch (Exception ex)
            {
                TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::HSEvent exception received: '" + ex.ToString() + "'");
            }
        }

        #endregion

        #region "    Testing and Diagnostics  "

        public void SendMessageToPanel(int unitno, string szMessage)
        {
            // need to check that we have successfully initialized                if (m_objSACommMgr != null)
            {
                TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::SendMessageToUnit '" + szMessage + "'");

                //	send the message to the correct unit
                m_objHvacUnitList[unitno].SendMessageToUnit(szMessage);

            }
        }
        public void SendSetTimeMessageToAllUnits()
        {
            foreach (clsHvacController objUnit in m_objHvacUnitList)
            {
                if (objUnit.Enabled)
                    objUnit.SendSetTimeMessageToUnit();
            }

        }
        public void SendTestMessageToAllUnits()
        {
            //	send the message to the correct unit
            foreach (clsHvacController objUnit in m_objHvacUnitList)
            {
                if (objUnit.Enabled)
                    objUnit.SendTestMessageToUnit();
            }
        }
        public void SendFilterMessageToAllUnits()
        {
            foreach (clsHvacController objUnit in m_objHvacUnitList)
            {
                if (objUnit.Enabled)
                    objUnit.SendFilterMessageToUnit(0);
            }

        }
        public void SendQueryAllSchedulesMessageToAllUnits()
        {
            foreach (clsHvacController objUnit in m_objHvacUnitList)
            {
                if (objUnit.Enabled)
                    objUnit.SendQueryAllSchedulesMessageToUnit();
            }

        }
        public void SendQueryTempMessageToAllZones()
        {
            foreach (clsThermostatZone objZone in m_objThermostatList)
            {
                m_objHvacUnitList[objZone.Unit].SendQueryTempMessageToUnit(objZone.Zone);
            }
        }
        public void SendQuerySetPointsMessageToAllZones()
        {
            foreach (clsThermostatZone objZone in m_objThermostatList)
            {
                m_objHvacUnitList[objZone.Unit].SendQuerySetPointsMessageToUnit(objZone.Zone);
            }
        }
        public void SendQuerySetPointLimitsMessageToAllZones()
        {
            foreach (clsThermostatZone objZone in m_objThermostatList)
            {
                m_objHvacUnitList[objZone.Unit].SendQuerySetPointLimitsMessageToUnit(objZone.Zone);
            }
        }
        public void SendQuerySystemSwitchMessageToAllZones()
        {
            foreach (clsThermostatZone objZone in m_objThermostatList)
            {
                m_objHvacUnitList[objZone.Unit].SendQuerySystemSwitchMessageToUnit(objZone.Zone);
            }
        }
        public void SendQuerySystemStateMessageToAllZones()
        {
            foreach (clsThermostatZone objZone in m_objThermostatList)
            {
                m_objHvacUnitList[objZone.Unit].SendQuerySystemStateMessageToUnit(objZone.Zone);
            }
        }
        #endregion

		#region "    Tracing and Diagnostics  "
        static public void TraceEvent(TraceEventType eType, string sMessage)
		{
            GetInstance().InternalTraceEvent(eType, sMessage);
		}

		static public void LogInformation( string sMessage )
		{
            GetInstance().InternalLogInformation(sMessage);
        }

        public void SetTraceLevel(string sLevel)
        {
            TextWriterTraceListener txt;

            txt = (TextWriterTraceListener) m_trcApp.Listeners[1];
            switch (sLevel)
            {
                case CFG_LOGLEVEL_NONE:
                    m_eTraceLevel = TraceEventType.Critical;
                    break;
                case CFG_LOGLEVEL_CRITICAL:
                    txt.Filter = new EventTypeFilter(SourceLevels.Critical);
                    m_trcApp.Switch.Level = SourceLevels.Critical;
                    m_eTraceLevel = TraceEventType.Critical;
                    break;
                case CFG_LOGLEVEL_ERROR:
                    txt.Filter = new EventTypeFilter(SourceLevels.Error);
                    m_trcApp.Switch.Level = SourceLevels.Error;
                    m_eTraceLevel = TraceEventType.Error;
                    break;
                case CFG_LOGLEVEL_WARNING:
                    txt.Filter = new EventTypeFilter(SourceLevels.Warning);
                    m_trcApp.Switch.Level = SourceLevels.Warning;
                    m_eTraceLevel = TraceEventType.Warning;
                    break;
                case CFG_LOGLEVEL_INFO:
                    txt.Filter = new EventTypeFilter(SourceLevels.Information);
                    m_trcApp.Switch.Level = SourceLevels.Information;
                    m_eTraceLevel = TraceEventType.Information;
                    break;
                case CFG_LOGLEVEL_VERBOSE:
                    txt.Filter = new EventTypeFilter(SourceLevels.Verbose);
                    m_trcApp.Switch.Level = SourceLevels.Verbose;
                    m_eTraceLevel = TraceEventType.Verbose;
                    break;
            }
        }

		public string GetConfig( string sConfig, string sDefault )
		{
			string								sRet = null;
			KeyValueConfigurationElement		kvElem;

			kvElem = m_cfgApp.AppSettings.Settings[sConfig];
			if ( kvElem == null )
				sRet = sDefault;
			else
				sRet = kvElem.Value;

			return sRet;
		}

        public void SetConfig(string sKey, string sVal)
		{
			try
			{
				if ( m_cfgApp.AppSettings.Settings[sKey] != null )
				{
					m_cfgApp.AppSettings.Settings.Remove(
						sKey );
				}
			}
			catch ( Exception )
			{
				//	This happens if it doesn't exist, ignore it
			}
			m_cfgApp.AppSettings.Settings.Add(
				sKey,
				sVal );

			// Save the configuration file.
			m_cfgApp.Save( ConfigurationSaveMode.Modified );

			// Force a reload of a changed section.
			ConfigurationManager.RefreshSection( "appSettings" );

			//	Check if this was the trace level
			if ( sKey == CFG_KEY_LOGLEVEL )
				SetTraceLevel( sVal );
		}

		#endregion

		#region "    HomeSeer access methods  "
		public void SetHomeSeerCallback( clsHSPI objHSPI )
		{
			//	Assign locally 
			m_ifHSPI = objHSPI;
			m_ifHSApp = (hsapplication)m_ifHSPI.GetHSIface();
		}

		public void EnableHomeSeerAccess()
        {
            //	Log the change
            TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::EnableHomeSeerAccess: enabling homeseer interfaces");


            //	First, go out and enable callbacks on config changes from HS, so we can keep our 
            //	object's names in synch
            int nEvent = HomeSeer.EV_TYPE_CONFIG_CHANGE;
            object objMaster = (object)this;
            ifHSApp.RegisterEventCB(ref nEvent, ref objMaster);

            AttachHomeseerMasterDevice();

            foreach (clsHvacController objUnit in m_objHvacUnitList)
            {
                if (objUnit.Enabled)
                    objUnit.EnableHomeSeerAccess();
            }
        }

		public void DisableHomeSeerAccess()
		{
            //	Log the change
            TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::DisableHomeSeerAccess: disabling homeseer interfaces");

            foreach (clsHvacController objUnit in m_objHvacUnitList)
            {
                if (objUnit.Enabled)
                    objUnit.DisableHomeSeerAccess();
            }

            foreach (clsThermostatZone objZone in m_objThermostatList)
            {
                DeleteZone(objZone.Unit, objZone.Zone);
            }

            DetachDevice();

            //	Reset the used device codes
			ResetUsedDevCodes();
		}

        public int GetNextFreeDevCode()
        {
            int i = 1;
            bool bTemp = false;

            //	Find one that is free
            for (; i < m_bUsedDevCodes.GetUpperBound(0); i++)
            {
                //	Attempt to get the value
                bTemp = m_bUsedDevCodes[i];
                if (bTemp == false) // lock only if this code is available
                {
                    lock (m_bUsedDevCodes)
                    {
                        bTemp = m_bUsedDevCodes[i];
                        if (bTemp == false) // double check that code still available
                        {
                            //	Reserve this dev code
                            m_bUsedDevCodes[i] = true;
                            break;
                        }
                    }
                }
            }

            LogInformation("clsEnviracomApp::GetNextFreeDevCode: Returning " + i.ToString());

            if (i > HVAC_MAXIMUM_DEVCODE)
            {
                TraceEvent(TraceEventType.Critical, "clsEnviracomApp::GetnextFreeDevCode: Too many devices");
            }

            return i;
        }

        public int GetLastFreeDevCode()
        {
            int i = m_bUsedDevCodes.GetUpperBound(0);
            bool bTemp = false;

            //	Find one that is free from the top down
            for (; i >= 0; i--)
            {
                //	Attempt to get the value
                bTemp = m_bUsedDevCodes[i];
                if (bTemp == false)
                {
                    lock (m_bUsedDevCodes)
                    {
                        bTemp = m_bUsedDevCodes[i];
                        if (bTemp == false) // double check that code still available
                        {
                            //	Reserve this dev code
                            m_bUsedDevCodes[i] = true;
                            break;
                        }
                    }
                }
            }

            LogInformation("clsEnviracomApp::GetLastFreeDevCode: Returning " + i.ToString());

            return i;
        }

        public void SetUsedDevCode(int nCode)
		{
			LogInformation("clsEnviracomApp::SetUsedDevCode: Setting used code of " + nCode.ToString() );

			//	Set this code in our list
            if (nCode <= HVAC_MAXIMUM_DEVCODE)
                lock (m_bUsedDevCodes)
                {
                    m_bUsedDevCodes[nCode] = true;
                }
		}

		public void FreeDevCode( int nCode )
		{
            LogInformation("clsEnviracomApp::FreeDevCode: Freeing used code of " + nCode.ToString());

            //	Remove this code from our list
            if (nCode <= HVAC_MAXIMUM_DEVCODE)
                lock (m_bUsedDevCodes)
                {
                    m_bUsedDevCodes[nCode] = false;
                }
        }

        public HomeSeer.UserRight GetUserRight(string sUser)
        {
            string[] UserPairs = null;
            SortedList HSUsers = new SortedList();


            try
            {
                string sAllUsers = ifHSApp.GetUsers();
                UserPairs = sAllUsers.Split(',');
            }
            catch (Exception ex)
            {
                TraceEvent(TraceEventType.Error, "clsEnviracomApp::GetUserRight: exception " + ex.ToString());
                return HomeSeer.UserRight.User_Invalid;
            }

            try
            {
                HSUsers.Clear();
                for (int idx = 0; idx <= UserPairs.GetUpperBound(0); idx++)
                {
                    string sTemp = UserPairs[idx];
                    string[] User = sTemp.Split('|');
                    sTemp = User[0].Trim().ToUpper();
                    int iRight = Convert.ToInt32(User[1].Trim());
                    HomeSeer.HSUser HSU = new HomeSeer.HSUser();
                    HSU.UserName = sTemp;
                    HSU.Rights = (HomeSeer.UserRight)iRight;
                    try
                    {
                        HSUsers.Add(sTemp, HSU);
                    }
                    catch (Exception ex)
                    {
                        TraceEvent(TraceEventType.Error, "clsEnviracomApp::GetUserRight: exception processing HomeSeer users: " + ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                TraceEvent(TraceEventType.Error, "clsEnviracomApp::GetUserRight: exception processing HomeSeer users: " + ex.ToString() );
            }

            foreach (DictionaryEntry DE in HSUsers)
            {
                HomeSeer.HSUser HSU = (HomeSeer.HSUser)DE.Value;
                if (HSU.UserName == sUser.Trim().ToUpper())
                {
                    return HSU.Rights;
                }
            }

            return HomeSeer.UserRight.User_Invalid;
        }

		#endregion

        #region "  Themostat Management Methods  "

        public void SeenZoneInstance(int iUnitNumber, int iZoneNumber)
        {
            clsHvacController unit;

            if ((unit = GetUnit(iUnitNumber)) != null)
            {
                if (unit.IsZoneManager == true)
                {
                    if ((iZoneNumber >= 1) && (iZoneNumber <= 9)) // make sure the zone number is a valid physical zone
                    {
                        m_objHvacUnitList[iUnitNumber].IsZonedSystem = true;

                        if (GetZone(iUnitNumber, iZoneNumber) == null)
                        {
                            CreateZone(iUnitNumber, iZoneNumber);
                            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::SeenZoneInstance: created zone instance: " + iZoneNumber);
                        }
                        else
                        {
                            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::SeenZoneInstance: found existing zone instance: " + iZoneNumber);
                        }
                    }
                }
                else
                {
                    if (iZoneNumber == 0) // make sure the zone number is a valid physical zone -- non zoned system is always zero
                    {
                        if (GetZone(iUnitNumber, iZoneNumber) == null)
                        {
                            CreateZone(iUnitNumber, iZoneNumber);
                            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::SeenZoneInstance: created zone instance: " + iZoneNumber);
                        }
                        else
                        {
                            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::SeenZoneInstance: found existing zone instance: " + iZoneNumber);
                        }
                    }
                }
            }
            else
            {
                clsEnviracomApp.TraceEvent(TraceEventType.Critical, "clsEnviracomApp::SeenZoneInstance unit not found: " + iUnitNumber + " for zone: " + iZoneNumber);
            }
        }


        #region "  Message Processing Methods  "

        public void ProcessZoneManagerMessage(int iUnitNumber, int iZoneManager)
        {
            if (iZoneManager > 0)
                m_objHvacUnitList[iUnitNumber].IsZoneManager = true;

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingZoneManagerMessage zone manager: " + iZoneManager);
        }


        public void ProcessRoomTempMessage(int iUnitNumber, int iZoneNumber, int iTemperature, bool bRoomSensorFault, bool bRoomDegreeCelsius)
        {
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber,iZoneNumber)) != null)
            {
                zone.CurrentTemp = iTemperature;
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingRoomTempMessage for zone: " + iZoneNumber);
        }

        public void ProcessRoomHumidityMessage(int iUnitNumber, int iZoneNumber, int iRoomHumidity)
        {
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber,iZoneNumber)) != null)
            {
                zone.CurrentHumidity = iRoomHumidity;
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingRoomHumidityMessage for zone: " + iZoneNumber);
        }
        
        
        public void ProcessOutdoorTempMessage(int iUnitNumber, int iZoneNumber, int iOutdoorTemperature)
        {
            float fOutdoorTemperatureC = ((float)(iOutdoorTemperature)) / (float)100;
            float fOutdoorTemperatureF = (fOutdoorTemperatureC * ((float)1.8)) + (float)32;


            foreach (clsThermostatZone zone in m_objThermostatList)
            {
                zone.OutdoorTemp = fOutdoorTemperatureF;
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingOutdoorTempMessage for zone: " + iZoneNumber);
        }
        public void ProcessSetPointMessage(int iUnitNumber, int iZoneNumber, int iHeatSetPoint, int iHoldStatus, int iCoolSetPoint, int iUnknown_5, int iRecovery, int iUnknown_7)
        {
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber,iZoneNumber)) != null)
            {
                zone.CoolSetPoint = ConvertTemperatureSetPoint(iCoolSetPoint);
                zone.HeatSetPoint = ConvertTemperatureSetPoint(iHeatSetPoint);
                switch (iHoldStatus)
                {
                    case 0x00:
                        zone.HoldStatus = clsThermostatZone.HoldStatusValues.Program;
                        break;
                    case 0x01:
                        zone.HoldStatus = clsThermostatZone.HoldStatusValues.Temporary;
                        break;
                    case 0x02:
                        zone.HoldStatus = clsThermostatZone.HoldStatusValues.Hold;
                        break;
                    default:
                        zone.HoldStatus = clsThermostatZone.HoldStatusValues.Uninitialized;
                        TraceEvent(TraceEventType.Critical, "clsEnviracomApp::ProcessSetPointMessage: unexpected value received for hold status: " + iHoldStatus);
                        break;
                }
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingSetPointMessage for zone: " + iZoneNumber);
        }
        public void ProcessTimeMessage(int iUnitNumber, int iZoneNumber, int iDayOfWeek, int iMonth, int iDay, int iYear, int iHours, int iMinutes, int iSeconds)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingTimeMessage for zone: " + iZoneNumber);
        }
        public void ProcessErrorMessage(int iUnitNumber, int iZoneNumber, int iUnknown_0, int iUnknown_1, int iErrorNumber, int iUnknown_3, int iUnknown_4, int iUnknown_5)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingErrorMessage for zone: " + iZoneNumber);
        }
        public void ProcessSystemSwitchMessage(int iUnitNumber, int iZoneNumber, int iSystemSwitch, int iAutoCurrentMode, int iExtraModeByte, int iAllowedModes)
        {
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber, iZoneNumber)) != null)
            {
                zone.SystemSwitch = (clsThermostatZone.SystemSwitchValues)(iSystemSwitch);
                zone.AutoCurrentMode = (clsThermostatZone.AutoModeValues)(iAutoCurrentMode);
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingSystemSwitchMessage for zone: " + iZoneNumber);
        }
        public void ProcessSystemStateMessage(int iUnitNumber, int iZoneNumber, int iUnknown_0, int iUnknown_1, int iSystemState, int iStageOneState, int iStageTwoState, int iStageThreeState, int iStageFourState)
        {
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber, iZoneNumber)) != null)
            {
                switch (iSystemState)
                {
                    case 0x10:
                        zone.SystemState = clsThermostatZone.SystemStateValues.HeatModeInactive;
                        zone.SystemStateActivity = clsThermostatZone.StateActivityValues.Off;
                        break;
                    case 0x11:
                        zone.SystemState = clsThermostatZone.SystemStateValues.HeatModeActive;
                        zone.SystemStateActivity = clsThermostatZone.StateActivityValues.Heat;
                        break;
                    case 0x20:
                        zone.SystemState = clsThermostatZone.SystemStateValues.CoolModeInactive;
                        zone.SystemStateActivity = clsThermostatZone.StateActivityValues.Off;
                        break;
                    case 0x21:
                        zone.SystemState = clsThermostatZone.SystemStateValues.CoolModeActive;
                        zone.SystemStateActivity = clsThermostatZone.StateActivityValues.Cool;
                        break;
                    default:
                        zone.SystemState = clsThermostatZone.SystemStateValues.Uninitialized;
                        zone.SystemStateActivity = clsThermostatZone.StateActivityValues.Uninitialized;
                        TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessSystemStateMessage: unexpected value received for system state: " + iSystemState);
                        break;
                }
                switch (iStageOneState)
                {
                    case 0x10:
                        zone.StageOneState = clsThermostatZone.StageStateValues.ConventionalInactive;
                        break;
                    case 0x11:
                        zone.StageOneState = clsThermostatZone.StageStateValues.ConventionalActive;
                        break;
                    case 0x40:
                        zone.StageOneState = clsThermostatZone.StageStateValues.HeatPumpInactive;
                        break;
                    case 0x41:
                        zone.StageOneState = clsThermostatZone.StageStateValues.HeatPumpActive;
                        break;
                    case 0x00:
                        zone.StageOneState = clsThermostatZone.StageStateValues.Uninitialized;
                        break;
                    default:
                        zone.StageOneState = clsThermostatZone.StageStateValues.Uninitialized;
                        TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessSystemStateMessage: unexpected value received for stage one state: " + iStageOneState);
                        break;
                }
                switch (iStageTwoState)
                {
                    case 0x10:
                        zone.StageTwoState = clsThermostatZone.StageStateValues.ConventionalInactive;
                        break;
                    case 0x11:
                        zone.StageTwoState = clsThermostatZone.StageStateValues.ConventionalActive;
                        break;
                    case 0x40:
                        zone.StageTwoState = clsThermostatZone.StageStateValues.HeatPumpInactive;
                        break;
                    case 0x41:
                        zone.StageTwoState = clsThermostatZone.StageStateValues.HeatPumpActive;
                        break;
                    case 0x00:
                        zone.StageTwoState = clsThermostatZone.StageStateValues.Uninitialized;
                        break;
                    default:
                        zone.StageTwoState = clsThermostatZone.StageStateValues.Uninitialized;
                        TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessSystemStateMessage: unexpected value received for stage two state: " + iStageTwoState);
                        break;
                }
                switch (iStageThreeState)
                {
                    case 0x10:
                        zone.StageThreeState = clsThermostatZone.StageStateValues.FossilFuelInactive;
                        break;
                    case 0x11:
                        zone.StageThreeState = clsThermostatZone.StageStateValues.FossilFuelActive;
                        break;
                    case 0x50:
                        zone.StageThreeState = clsThermostatZone.StageStateValues.ElectricInactive;
                        break;
                    case 0x51:
                        zone.StageThreeState = clsThermostatZone.StageStateValues.ElectricActive;
                        break;
                    case 0x00:
                        zone.StageThreeState = clsThermostatZone.StageStateValues.Uninitialized;
                        break;
                    default:
                        zone.StageThreeState = clsThermostatZone.StageStateValues.Uninitialized;
                        TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessSystemStateMessage: unexpected value received for stage three state: " + iStageThreeState);
                        break;
                }
                switch (iStageFourState)
                {
                    case 0x10:
                        zone.StageFourState = clsThermostatZone.StageStateValues.FossilFuelInactive;
                        break;
                    case 0x11:
                        zone.StageFourState = clsThermostatZone.StageStateValues.FossilFuelActive;
                        break;
                    case 0x50:
                        zone.StageFourState = clsThermostatZone.StageStateValues.ElectricInactive;
                        break;
                    case 0x51:
                        zone.StageFourState = clsThermostatZone.StageStateValues.ElectricActive;
                        break;
                    case 0x00:
                        zone.StageFourState = clsThermostatZone.StageStateValues.Uninitialized;
                        break;
                    default:
                        zone.StageFourState = clsThermostatZone.StageStateValues.Uninitialized;
                        TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessSystemStateMessage: unexpected value received for stage four state: " + iStageFourState);
                        break;
                }
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingSystemStateMessage for zone: " + iZoneNumber);
        }
        public void ProcessEquipmentFaultMessage(int iUnitNumber, int iZoneNumber, int iUnknown_0, int iUnknown_1, int iUnknown_2, int iUnknown_3, int iUnknown_4)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingEquipmentFaultMessage for zone: " + iZoneNumber);
        }
        public void ProcessBlowerFanSpeedMessage(int iUnitNumber, int iBlowerFanSpeed, int iUnknown_1, int iUnknown_2, int iUnknown_3)
        {
            foreach (clsThermostatZone zone in m_objThermostatList)
            {
                if (zone.Unit == iUnitNumber)
                {
                    switch (iBlowerFanSpeed)
                    {
                        case 0x00:
                            zone.BlowerFanStatus = clsThermostatZone.BlowerFanValues.Off;
                            break;
                        case 0x64:
                            zone.BlowerFanStatus = clsThermostatZone.BlowerFanValues.Slow;
                            break;
                        case 0xC8:
                            zone.BlowerFanStatus = clsThermostatZone.BlowerFanValues.Fast;
                            break;
                        default:
                            zone.BlowerFanStatus = clsThermostatZone.BlowerFanValues.Uninitialized;
                            TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessBlowerFanSpeedMessage: unexpected value received for fan speed: " + iBlowerFanSpeed);
                            break;
                    }
                    clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessBlowerFanSpeedMessage for unit: " + zone.Unit + " zone: " + zone.Zone);
                }
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingBlowerFanSpeedMessage");
        }
        public void ProcessAuxHeatStatusMessage(int iUnitNumber, int iAuxHeatStatus, int iUnknown_1, int iUnknown_2, int iUnknown_3)
        {
            foreach (clsThermostatZone zone in m_objThermostatList)
            {
                if (zone.Unit == iUnitNumber)
                {
                    switch (iAuxHeatStatus)
                    {
                        case 0x00:
                            zone.AuxHeatStatus = clsThermostatZone.AuxHeatStatusValues.Off;
                            break;
                        case 0x64:
                            zone.AuxHeatStatus = clsThermostatZone.AuxHeatStatusValues.StageOne;
                            break;
                        case 0xC8:
                            zone.AuxHeatStatus = clsThermostatZone.AuxHeatStatusValues.StageTwo;
                            break;
                        default:
                            zone.AuxHeatStatus = clsThermostatZone.AuxHeatStatusValues.Uninitialized;
                            TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessAuxHeatStatusMessage: unexpected value received for aux heat status: " + iAuxHeatStatus);
                            break;
                    }
                    clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessAuxHeatStatusMessage for unit: " + zone.Unit + " zone: " + zone.Zone);
                }
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingAuxHeatStatusMessage");
        }
        public void ProcessCoolingHeatPumpStatusMessage(int iUnitNumber, int iCoolingHeatPumpState, int iUnknown_1, int iUnknown_2, int iUnknown_3)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingCoolingHeatPumpMessage");
        }
        public void ProcessHeatingHeatPumpStatusMessage(int iUnitNumber, int iHeatingHeatPumpState, int iUnknown_1, int iUnknown_2, int iUnknown_3)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingHeatingHeatPumpMessage");
        }
        public void ProcessCirculateFanMessage(int iUnitNumber, int iZoneNumber, int iUnknown_0, int iCirculateFanStatus)
        {
            if (iZoneNumber == 0)
            {
                foreach (clsThermostatZone zone in m_objThermostatList)
                {
                    if (zone.Unit == iUnitNumber)
                    {
                        switch (iCirculateFanStatus)
                        {
                            case 0x00:
                                zone.CirculateFanStatus = clsThermostatZone.CirculateFanValues.Off;
                                break;
                            case 0xC8:
                                zone.CirculateFanStatus = clsThermostatZone.CirculateFanValues.On;
                                break;
                            default:
                                zone.CirculateFanStatus = clsThermostatZone.CirculateFanValues.Uninitialized;
                                TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessSystemStateMessage: unexpected value received for fan state: " + iCirculateFanStatus);
                                break;
                        }
                    }
                }
            }
            else
            {
                clsThermostatZone zone;

                //	See if we can find it in our zone list
                if ((zone = GetZone(iUnitNumber, iZoneNumber)) != null)
                {
                    switch (iCirculateFanStatus)
                    {
                        case 0x00:
                            zone.CirculateFanStatus = clsThermostatZone.CirculateFanValues.Off;
                            break;
                        case 0xC8:
                            zone.CirculateFanStatus = clsThermostatZone.CirculateFanValues.On;
                            break;
                        default:
                            zone.CirculateFanStatus = clsThermostatZone.CirculateFanValues.Uninitialized;
                            TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessSystemStateMessage: unexpected value received for fan state: " + iCirculateFanStatus);
                            break;
                    }
                }
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingCirculateFanMessage for zone: " + iZoneNumber + " value " + iCirculateFanStatus);
        }
        public void ProcessDehumidifierStateMessage(int iUnitNumber,int iZoneNumber, int iDehumidifierMode, int iExternalDehumidifierState)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingDehumidifierStateMessage for zone: " + iZoneNumber);
        }
        public void ProcessHumidifierStateMessage(int iUnitNumber,int iZoneNumber, int iUnknown_0, int iHumidifierState)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingHumidifierStateMessage for zone: " + iZoneNumber);
        }
        public void ProcessExternalVentilationStateMessage(int iUnitNumber,int iZoneNumber, int iUnknown_0, int iExternalVentilationState)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingExternalVentilationStateMessage for zone: " + iZoneNumber);
        }
        public void ProcessFanSwitchMessage(int iUnitNumber,int iZoneNumber, int iFanModeOne, int iFanModeTwo)
        {
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber, iZoneNumber)) != null)
            {
                if (iFanModeOne == 0 && iFanModeTwo == 0x20)
                    zone.FanSwitch = clsThermostatZone.FanSwitchValues.AutoNormal;
                else if (iFanModeOne == 0 && iFanModeTwo == 0xA0)
                    zone.FanSwitch = clsThermostatZone.FanSwitchValues.AutoCirculate;
                else if (iFanModeOne == 0xC8 && iFanModeTwo == 0x20)
                    zone.FanSwitch = clsThermostatZone.FanSwitchValues.OnNormal;
                else if (iFanModeOne == 0xC8 && iFanModeTwo == 0xA0)
                    zone.FanSwitch = clsThermostatZone.FanSwitchValues.OnCirculate;
                else
                    zone.FanSwitch = clsThermostatZone.FanSwitchValues.Uninitialized;
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingFanModeMessage for zone: " + iZoneNumber);
        }

        public void ProcessDehumidifierSetPointMessage(int iUnitNumber,int iZoneNumber, int iDehumidifySetPoint, int iDehumidifyUpperLimit, int iDehumidifyLowerLimit)
        {
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber, iZoneNumber)) != null)
            {
                zone.DehumiditySetpoint = iDehumidifySetPoint / 2;
                zone.MaxDehumiditySetpoint = iDehumidifyUpperLimit / 2;
                zone.MinDehumiditySetpoint = iDehumidifyLowerLimit / 2;
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingDehumidifierSetPointMessage for zone: " + iZoneNumber);
        }
        public void ProcessHumidifierSetPointMessage(int iUnitNumber,int iZoneNumber, int iHumidifySetPoint, int iHumidifyUpperLimit, int iHumidifyLowerLimit)
        {
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber, iZoneNumber)) != null)
            {
                zone.HumiditySetpoint = iHumidifySetPoint / 2;
                zone.MaxHumiditySetpoint = iHumidifyUpperLimit / 2;
                zone.MinHumiditySetpoint = iHumidifyLowerLimit / 2;
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingHumidifierSetPointMessage for zone: " + iZoneNumber);
        }

        public void ProcessScheduleMessage(int iUnitNumber,int iZoneNumber, int iScheduleDay, int iSchedulePeriod, int iScheduleTime, int iHeatSetPoint, int iCoolSetPoint, int iFanMode)
        {
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber,iZoneNumber)) != null)
            {
                float fCoolSetPoint = ((((float)(iCoolSetPoint)) / (float)100) * ((float)1.8)) + (float)32;
                float fHeatSetPoint = ((((float)(iHeatSetPoint)) / (float)100) * ((float)1.8)) + (float)32;
                zone.SetSchedule(iScheduleDay, iSchedulePeriod, iScheduleTime, fHeatSetPoint, fCoolSetPoint, iFanMode);
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingScheduleMessage for zone: " + iZoneNumber);
        }

        public void ProcessAirFilterMessage(int iUnitNumber,int iZoneNumber, int iRemainingFilterTime, int iTotalFilterTime)
        {
            if (iZoneNumber == 0)
            {
                foreach (clsThermostatZone zone in m_objThermostatList)
                {
                    if (zone.Unit == iUnitNumber)
                    {
                        zone.RemainingFilterTime = iRemainingFilterTime;
                        clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingAirFilterMessage for unit: " + zone.Unit + " zone: " + zone.Zone);
                    }
                }
            }
            else
            {
                clsThermostatZone zone;

                //	See if we can find it in our zone list
                if ((zone = GetZone(iUnitNumber, iZoneNumber)) != null)
                {
                    zone.RemainingFilterTime = iRemainingFilterTime;
                    clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingAirFilterMessage for zone: " + iZoneNumber);
                }
            }
        }


        public void ProcessDamperStatusMessage(int iUnitNumber, int iZoneNumber, int iDamperStatus)
        {
            if (iZoneNumber == 0)
            {
                foreach (clsThermostatZone zone in m_objThermostatList)
                {
                    if (zone.Unit == iUnitNumber)
                    {
                        switch (iDamperStatus)
                        {
                            case 0x00:
                                zone.DamperStatus = clsThermostatZone.DamperStatusValues.Closed;
                                break;
                            case 0xC8:
                                zone.DamperStatus = clsThermostatZone.DamperStatusValues.Open;
                                break;
                            default:
                                zone.DamperStatus = clsThermostatZone.DamperStatusValues.Uninitialized;
                                TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessDamperStatusMessage: unexpected value received for damper state: " + iDamperStatus);
                                break;
                        }
                        clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingDamperStatusMessage for unit: " + zone.Unit + " zone: " + zone.Zone);
                    }
                }
            }
            else
            {
                clsThermostatZone zone;

                //	See if we can find it in our zone list
                if ((zone = GetZone(iUnitNumber, iZoneNumber)) != null)
                {
                    switch (iDamperStatus)
                    {
                        case 0x00:
                            zone.DamperStatus = clsThermostatZone.DamperStatusValues.Closed;
                            break;
                        case 0xC8:
                            zone.DamperStatus = clsThermostatZone.DamperStatusValues.Open;
                            break;
                        default:
                            zone.DamperStatus = clsThermostatZone.DamperStatusValues.Uninitialized;
                            TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessDamperStatusMessage: unexpected value received for damper state: " + iDamperStatus);
                            break;
                    }
                    clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingDamperStatusMessage for unit: " + zone.Unit + " zone: " + zone.Zone);
                }
            }
        }

        public void ProcessAquastatCirculatingPumpStatusMessage(int iUnitNumber, int iZoneNumber, int iDamperStatus)
        {
            if (iZoneNumber == 0x3B)
            {
                clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingAquastatStatusMessage for L7224U Aquastat");
            }
            else
            {
                clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingAquastatStatusMessage for invalid zone instance " + iZoneNumber);
            }
        }

        #endregion
        #region "  Message Creation Methods  "
        public void CreateRoomTempMessage(int iUnitNumber, int iZoneNumber, int iTemperature, bool bRoomSensorFault, bool bRoomDegreeCelsius)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateRoomTempMessage for zone: " + iZoneNumber);
        }
        public void CreateOutdoorTempMessage(int iUnitNumber, int iZoneNumber, int iOutdoorTemperature)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateOutdoorTempMessage for zone: " + iZoneNumber);
        }
        public void CreateModeMessage(int iUnitNumber, int iZoneNumber, clsThermostatZone.SystemSwitchValues eModeStatus)
        {
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber, iZoneNumber)) != null)
            {
                switch (eModeStatus)
                {
                    case clsThermostatZone.SystemSwitchValues.EmergencyHeat:
                    case clsThermostatZone.SystemSwitchValues.Heat:
                    case clsThermostatZone.SystemSwitchValues.Off:
                    case clsThermostatZone.SystemSwitchValues.Cool:
                    case clsThermostatZone.SystemSwitchValues.Auto:
                        m_objHvacUnitList[zone.Unit].SendModeMessageToUnit(iZoneNumber, (int)eModeStatus);
                        TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateModeMessage for zone: " + iZoneNumber);
                        break;
                    case clsThermostatZone.SystemSwitchValues.Uninitialized:
                    default:
                        TraceEvent(TraceEventType.Critical, "clsEnviracomApp::CreateModeMessage: unexpected value received for mode status: " + eModeStatus.ToString());
                        break;
                }
            }
        }
        public void CreateHoldMessage(int iUnitNumber, int iZoneNumber, clsThermostatZone.HoldStatusValues eHoldStatus)
        {
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber, iZoneNumber)) != null)
            {
                int iCoolSetPoint = ConvertTemperatureSetPoint(zone.CoolSetPoint);
                int iHeatSetPoint = ConvertTemperatureSetPoint(zone.HeatSetPoint);
                switch (eHoldStatus)
                {
                    case clsThermostatZone.HoldStatusValues.Program:
                    case clsThermostatZone.HoldStatusValues.Temporary:
                    case clsThermostatZone.HoldStatusValues.Hold:
                        m_objHvacUnitList[zone.Unit].SendHoldMessageToUnit(iZoneNumber, iHeatSetPoint, (int)eHoldStatus, iCoolSetPoint, 0, 0, 0, 0);
                        TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateHoldMessage for zone: " + iZoneNumber);
                        break;
                    case clsThermostatZone.HoldStatusValues.Uninitialized:
                    default:
                        TraceEvent(TraceEventType.Critical, "clsEnviracomApp::CreateHoldMessage: unexpected value received for hold status: " + zone.HoldStatus.ToString());
                        break;
                }
            }
        }
        public void CreateTimeMessage(int iUnitNumber, int iZoneNumber, int iDayOfWeek, int iMonth, int iDay, int iYear, int iHours, int iMinutes, int iSeconds)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateTimeMessage for zone: " + iZoneNumber);
        }
        public void CreateErrorMessage(int iUnitNumber, int iZoneNumber, int iUnknown_0, int iUnknown_1, int iErrorNumber, int iUnknown_3, int iUnknown_4, int iUnknown_5)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateErrorMessage for zone: " + iZoneNumber);
        }
        public void CreateSystemSwitchMessage(int iUnitNumber, int iZoneNumber, int iSystemSwitch, int iAutoCurrentMode, int iExtraModeByte, int iAllowedModes)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateSystemSwitchMessage for zone: " + iZoneNumber);
        }
        public void CreateSystemStateMessage(int iUnitNumber, int iZoneNumber, int iUnknown_0, int iUnknown_1, int iSystemState, int iStageOneState, int iStageTwoState, int iStageThreeState, int iStageFourState)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateSystemStateMessage for zone: " + iZoneNumber);
        }
        public void CreateEquipmentFaultMessage(int iUnitNumber, int iZoneNumber, int iUnknown_0, int iUnknown_1, int iUnknown_2, int iUnknown_3, int iUnknown_4)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateEquipmentFaultMessage for zone: " + iZoneNumber);
        }
        public void CreateHeatPumpStatusMessage(int iUnitNumber, int iZoneNumber, int iHeatpumpState, int iUnknown_1, int iUnknown_2, int iUnknown_3)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateHeatPumpMessage for zone: " + iZoneNumber);
        }
        public void CreateCirculateFanMessage(int iUnitNumber, int iZoneNumber, int iUnknown_0, int iFanState)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateCirculateFanMessage for zone: " + iZoneNumber);
        }
        public void CreateDehumidifierStateMessage(int iUnitNumber, int iZoneNumber, int iDehumidifierMode, int iExternalDehumidifierState)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateDehumidifierStateMessage for zone: " + iZoneNumber);
        }
        public void CreateHumidifierStateMessage(int iUnitNumber, int iZoneNumber, int iUnknown_0, int iHumidifierState)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateHumidifierStateMessage for zone: " + iZoneNumber);
        }
        public void CreateExternalVentilationStateMessage(int iUnitNumber, int iZoneNumber, int iUnknown_0, int iExternalVentilationState)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateExternalVentilationStateMessage for zone: " + iZoneNumber);
        }
        public void CreateFanSwitchMessage(int iUnitNumber, int iZoneNumber,  clsThermostatZone.FanSwitchValues eFanSwitchStatus)
        {

            int iFanModeOne = -1;
            int iFanModeTwo = -1;
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber, iZoneNumber)) != null)
            {

                switch (eFanSwitchStatus)
                {
                    case clsThermostatZone.FanSwitchValues.AutoNormal:
                        iFanModeOne = 0;
                        iFanModeTwo = 0x20;
                        break;
                    case clsThermostatZone.FanSwitchValues.AutoCirculate:
                        iFanModeOne = 0;
                        iFanModeTwo = 0xA0;
                        break;
                    case clsThermostatZone.FanSwitchValues.OnNormal:
                        iFanModeOne = 0xC8;
                        iFanModeTwo = 0x20;
                        break;
                    case clsThermostatZone.FanSwitchValues.OnCirculate:
                        iFanModeOne = 0xC8;
                        iFanModeTwo = 0xA0;
                        break;
                    case clsThermostatZone.FanSwitchValues.Uninitialized:
                    default:
                        TraceEvent(TraceEventType.Critical, "clsEnviracomApp::CreateFanSwitchMessage: unexpected value received for fan switch status: " + eFanSwitchStatus.ToString());
                        break;
                }
            }

            if (eFanSwitchStatus != clsThermostatZone.FanSwitchValues.Uninitialized) 
            {
                m_objHvacUnitList[zone.Unit].SendFanMessageToUnit(iZoneNumber, iFanModeOne, iFanModeTwo);
            }
            
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateFanModeMessage for zone: " + iZoneNumber);
        }
        public void CreateScheduleMessage(int iUnitNumber, int iZoneNumber, int iScheduleDay, int iSchedulePeriod, int iScheduleTime, int iHeatSetPoint, int iCoolSetPoint, int iFanMode)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateScheduleMessage for zone: " + iZoneNumber);
        }
        public void CreateAirFilterMessage(int iUnitNumber, int iZoneNumber, int iRemainingFilterTime, int iTotalFilterTime, int iRemainingFilterPercent)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessingAirFilterMessage for zone: " + iZoneNumber);
        }
        public void CreateSetPointMessage(int iUnitNumber, int iZoneNumber, int iHeatSetPoint,  int iCoolSetPoint)
        {
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber, iZoneNumber)) != null)
            {
                if (iCoolSetPoint == 0)
                {
                    iCoolSetPoint = ConvertTemperatureSetPoint(zone.CoolSetPoint);
                }
                else
                {
                    iCoolSetPoint = (int)(((iCoolSetPoint - 32) / 1.8) * 100);
                }

                if (iHeatSetPoint == 0)
                {
                    iHeatSetPoint = ConvertTemperatureSetPoint(zone.HeatSetPoint);
                }
                else
                {
                    iHeatSetPoint = (int)(((iHeatSetPoint - 32) / 1.8) * 100);
                }

                m_objHvacUnitList[zone.Unit].SendSetPointMessageToUnit(iZoneNumber, iHeatSetPoint, (int)zone.HoldStatus, iCoolSetPoint, 0, 0, 0, 0);
            } 
            
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateSetPointMessage for zone: " + iZoneNumber);
        }

        #endregion

        #endregion
        #endregion

        #region "  Private Methods  "
        #region "    Initialization and cleanup  "
        private void InitConfiguration()
		{
			string					sCfgLoc;
			ExeConfigurationFileMap	configFile;

			//	Set the location for the config file
            sCfgLoc = m_ifHSApp.GetAppPath() + "\\"+ CFG_FILE;
			configFile = new ExeConfigurationFileMap();
			configFile.ExeConfigFilename = sCfgLoc;
			try
			{
				m_cfgApp = ConfigurationManager.OpenMappedExeConfiguration(configFile, ConfigurationUserLevel.None );

                m_iMsgDelay = Convert.ToInt32(GetConfig(CFG_KEY_MSGDELAY, "1000"));
                m_iMsgTimeOut = Convert.ToInt32(GetConfig(CFG_KEY_MSGTIMEOUT, "2000"));
                m_iMsgWaitAck = Convert.ToInt32(GetConfig(CFG_KEY_MSGWAITACK, "1000"));
                m_iBaudRate = Convert.ToInt32(GetConfig(CFG_KEY_BAUDRATE, "19200"));
            }
			catch ( Exception ex )
			{
				Debug.WriteLine( "clsEnviracomApp::InitConfiguration : Exception " +
					ex.ToString() );
			}
        }

        private void InitUnits()
        {
            int Count = Convert.ToInt32(GetConfig(CFG_KEY_MAXUNITS, "1"));

            for (int i = 0; i < Count; i++)
            {
                try
                {

                    string portname = GetConfig(CFG_KEY_UNITPORT + i.ToString("0"), "NOTFOUND");
                    TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::InitUnits: " + CFG_KEY_UNITPORT + i.ToString("0") + " returned portname: " + portname);
                    if (portname != "NOTFOUND")
                    {
                        clsHvacController objUnit = new clsHvacController();
                        objUnit.UnitNumber = i;
                        objUnit.Port = portname;
                        objUnit.Enabled = true;
                        m_objHvacUnitList.Add(objUnit);
                        objUnit.Initialize();
                        TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::InitUnits: " + objUnit.Name + " number: " + objUnit.UnitNumber + " on port " + objUnit.Port);
                    }
                }
                catch (Exception ex)
                {
                    //	If we get an exception, that is bad, so let everyone know
                    TraceEvent(TraceEventType.Critical, "clsHEnviracomApp::InitUnits: Received exception: " + ex.ToString());
                }
            }


        }


		private void InitTrace()
		{
            TextWriterTraceListener         txt;
			string							sFileName;
			string							sLevel;

			//	Create the new TraceSource object
            m_trcApp = new TraceSource(PLUGIN_IN_NAME_SHORT);
			m_trcApp.Switch.Level = SourceLevels.Verbose;

			//	Create a text file based on the date
            sFileName = m_ifHSApp.GetAppPath() + "\\" + LOG_PREFIX + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            txt = new TextWriterTraceListener(sFileName);
			txt.Name = "DiagLog";
			txt.Filter = new EventTypeFilter( SourceLevels.Off );  // Will be set later

			//	Now, add our listener to the trace object
			m_trcApp.Listeners.Add( txt );

			//	Check if we have a setting for the logLevel
			sLevel = GetConfig( CFG_KEY_LOGLEVEL, CFG_LOGLEVEL_VERBOSE );
			SetTraceLevel( sLevel );
		}

        private void CleanupUnits()
        {

            foreach (clsHvacController objUnit in m_objHvacUnitList)
            {
                try
                {
                    objUnit.Cleanup();
                }
                catch (Exception ex)
                {
                    //	If we get an exception just trace it for diagnostic purposes
                    TraceEvent(TraceEventType.Warning, "clsEnviracomApp::CleanupUnits: Received exception: " + ex.ToString());
                }
            }
            m_objHvacUnitList.Clear();
        }


        public void SetUnitList(clsHvacController[] lList)
		{
		}


		#endregion

		#region "    HomeSeer access methods  "
		private void ResetUsedDevCodes()
		{
            lock (m_bUsedDevCodes)
            {
                //	Clear the list
                m_bUsedDevCodes = new bool[HVAC_MAXIMUM_DEVCODE + 1];
            }
		}

        private bool CreateHomeseerMasterDevice()
        {
            DeviceClass dev;

            //  Create the device
            dev = ifHSApp.NewDeviceEx(HVAC_MASTER_NAME);
            SetHomeseerMasterDeviceAttributes(dev);
            //	Finally, tell the zone what it's dev code is
            AttachDevice(dev);

            clsEnviracomApp.LogInformation(
                "clsHvacController::CreateHomeseerMasterDevice: Created Enviracom Master Device at device code " +
                dev.dc);

            return true;
        }



        private void DeleteHomeseerMasterDevice()
        {

            //	remove the unit from HS and the dev code from EnviracomApp
            if (m_bDeviceAttached == true)
            {
                int nRef = ifHSApp.GetDeviceRef(HouseCode + DevCode);
                clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::DeleteHomeseerMasterDevice: Deleting device (" +
                    DevCode + ":" +
                    Name + ")");
                DetachDevice();
                ifHSApp.DeleteDevice(nRef);
            }
        }


        private void SetHomeseerMasterDeviceAttributes(DeviceClass dev)
        {
            //  set all the relevant information
            dev.location = HVAC_MASTER_LOCATION;
            dev.hc = Convert.ToString(Convert.ToChar(m_ifHSPI.GetNextFreeIOCode()));
            dev.dc = Convert.ToString(GetLastFreeDevCode());
            dev.@interface = clsEnviracomApp.PLUGIN_IN_NAME_SHORT;
            dev.misc = HomeSeer.MISC_STATUS_ONLY;
            dev.dev_type_string = HVAC_MASTER_TYPE;
            dev.iotype = HomeSeer.IOTYPE_CONTROL;
            dev.iomisc = HVAC_MASTER_MAGIC;
            dev.values =
                "Uninitialized" + Convert.ToChar(2) + Convert.ToString((int)Status.UNKNOWN) + Convert.ToChar(1) +
                "Connected" + Convert.ToChar(2) + Convert.ToString((int)Status.CONNECTED) + Convert.ToChar(1) +
                "Fault" + Convert.ToChar(2) + Convert.ToString((int)Status.EQUIPMENT_FAULT);
        }

        private DeviceClass FindHomeseerMasterDevice()
        {
            clsDeviceEnumeration de;
            DeviceClass dev;

            //  Enumerate through all the devices looking for one of mine
            de = (clsDeviceEnumeration)ifHSApp.GetDeviceEnumerator();
            while (!de.Finished)
            {
                dev = (DeviceClass)de.GetNext();
                if ((dev.dev_type_string == HVAC_MASTER_TYPE) && (dev.iomisc == HVAC_MASTER_MAGIC))
                {
                    return dev;
                }
            }
            return null;
        }


		#endregion

		#region "    Tracing and Diagnostics  "
        private void WriteHomeseerLog(TraceEventType eType, string szMessage)
        {
            if (ifHSApp != null)
                ifHSApp.WriteLog(PLUGIN_IN_NAME_SHORT,eType.ToString() + "::" + szMessage);
        }

        private void InternalTraceEvent(TraceEventType eType, string sMessage)
        {
            //	Send this to our TraceSource if it is valid
            if (m_trcApp != null)
                m_trcApp.TraceEvent(eType, 0, sMessage);
            if (eType <= m_eTraceLevel)
                WriteHomeseerLog(eType, sMessage);
        }

        private void InternalLogInformation(string sMessage)
        {
            //	Send this to our TraceSource if it is valid
            if (m_trcApp != null)
                m_trcApp.TraceInformation(sMessage);
            WriteHomeseerLog(TraceEventType.Information, sMessage);
        }




        #endregion

        #region "    Themostat Management Methods  "

        private clsThermostatZone GetZone(int nUnit, int nZone)
        {
            return m_objThermostatList.Find(delegate(clsThermostatZone tz)
            {
                return tz.Zone == nZone && tz.Unit == nUnit;
            });
        }

        private clsHvacController GetUnit(int nUnit)
        {
            return m_objHvacUnitList.Find(delegate(clsHvacController hc)
            {
                return hc.UnitNumber == nUnit;
            });
        }

        private void DeleteZone(int nUnit, int nNumber)
        {
            clsThermostatZone objZone;

            //	Get the zone
            objZone = GetZone(nUnit, nNumber);

            //	Remove the zone from the list
            m_objThermostatList.Remove(objZone);

            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::DeleteZone: Deleting zone (" +
                nNumber.ToString() + ":" +
                objZone.Name + ")");

            //	Finally, remove the zone and dev code from EnviracomApp
            if (objZone.DeviceAttached == true)
            {
                objZone.DetachDevice();
            }
        }

        private clsThermostatZone CreateZone(int iUnitNumber, int iZoneNumber)
        {
            clsThermostatZone objZone;

            objZone = new clsThermostatZone();
            objZone.Zone = iZoneNumber;
            objZone.Unit = iUnitNumber;
            objZone.ThermostatNumber = NumberOfThermostats;
            objZone.AttachHomeseerZoneDevice();

            //	Add the zone from the list
            m_objThermostatList.Add(objZone);

            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::CreateZone: Creating zone (" +
                iZoneNumber.ToString() + ":" +
                objZone.Name + ")");

            return objZone;
        }
        #endregion

        private float ConvertTemperatureSetPoint(int iSetPoint)
        {
            return ((((float)(iSetPoint)) / (float)100) * ((float)1.8)) + (float)32;
        }

        private int ConvertTemperatureSetPoint(float fSetPoint)
        {
            return Convert.ToInt32( (fSetPoint - ((float)32)) / ((float)1.8)) * 100;
        }

        public bool ProcessPeriodicEvent(object sender, PeriodicListEventArgs<string> args)
        {
            TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessPeriodicEvent: processing a periodic event: " + args.Work);
            return false;
        }

        static public int GetThermostatInternalIndex(int hststatno)
        {
            return hststatno - 1;
        }

        static public int GetThermostatHomeseerIndex(int tstatno)
        {
            return tstatno + 1;
        }

        #endregion
    }
}
