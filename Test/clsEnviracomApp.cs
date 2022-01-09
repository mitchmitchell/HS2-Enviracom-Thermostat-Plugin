using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using Scheduler;
using Scheduler.Classes;

namespace HSPI_ENVIRACOM_MANAGER
{
    [Serializable()]
    public class clsEnviracomApp : MarshalByRefObject
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
		public const string		PLUGIN_IN_NAME_SHORT =	HSPI.HSPI_PLUGIN_NAME;

		#region "    Configuration Enums and Constants  "
		//	Config file m_sUnitName
		private const string	CFG_FILE =				@"config\enviracomhvac.config";

		//	Configuration keys
        public const string     CFG_KEY_ISZONED  = "envZoned";
        public const string     CFG_KEY_LOGLEVEL = "logLevel";
        public const string     CFG_KEY_MAXUNITS = "numUnits";
        public const string     CFG_KEY_UNIT = "Unit";
        public const string     CFG_KEY_PORT = "Port";
        public const string     CFG_KEY_ZONE = "Zone";
        public const string     CFG_KEY_COUNT = "Count";
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

        private const int       PERIODIC_TICK_TIME = 10; // in seconds

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
        private PeriodicList<clsResponse>           m_objResponseList = new PeriodicList<clsResponse>(new TimeSpan(0, 0, PERIODIC_TICK_TIME),delegate(clsResponse rsp)
                                                                                                                                             {
                                                                                                                                                 return rsp.ticks == 0;
                                                                                                                                             });
        private clsMessageMgr                       m_objMessageMgr = null; //new clsMessageMgr();  Cannot Initialize classes that call GetInstance to access EnviracomApp object until first call to GetInstance returns
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

        public clsMessageMgr MessageMgr
        {
			get { return m_objMessageMgr; }
        }

        public hsapplication ifHSApp
		{
			get { return m_ifHSApp; }
		}

        public string Name
        {
            get
            {
                if (m_bDeviceAttached == true)
                    return m_objAttachedDevice.Name;
                else
                    return "";
            }
        }

        public string Location
        {
            get
            {
                if (m_bDeviceAttached == true)
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
            get {

                LogInformation("clsEnviracomApp::Units: Returning " + m_objHvacUnitList.ToString());

                return m_objHvacUnitList;
            }
        }

        public List<clsThermostatZone> Thermostats
        {
            get {

                LogInformation("clsEnviracomApp::Thermostats: Returning " + m_objThermostatList.ToString());

                return m_objThermostatList;
            }
        }

        public List<clsScheduleDay> Schedules
        {
            get {
                List<clsScheduleDay> sl = new List<clsScheduleDay>();

                foreach (clsThermostatZone tz in m_objThermostatList)
                {
                    foreach (clsScheduleDay sd in tz.ScheduleDays)
                    {
                        sl.Add(sd);
                    }
                }
                LogInformation("clsEnviracomApp::Schedules: Returning " + sl.ToString());

                return sl; 
            }
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
            m_objMessageMgr = new clsMessageMgr();

           //	Now that we have the HomeSeer object, open up the configuration
            InitConfiguration(); // go get configuration information from config file

            //	Init our tracing capability				
            InitTrace();

            InitResponses();

            InitUnits();

            InitZones();

            //	Trace this event after we init the trace (otherwise it won't show up)
            TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::Initialize");
		}

		public void Cleanup()
		{
            TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::Cleanup");

            m_objResponseList.Stop();

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
            clsThermostatZone tz;
            clsHvacController hc;

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
                // TODO - need to handle things like device renaming etc I think here @@@@@
                if ((tz = GetZone(dev)) != null) {
                    tz.UpdateZoneDevice(dev);
                } else if ((hc = GetUnit(dev)) != null) {
                    hc.UpdateUnitDevice(dev);
                } else if ((m_objAttachedDevice.hc == dev.hc) && (m_objAttachedDevice.dc == dev.dc)) {
                    m_objAttachedDevice = dev;
                } else
                    return;
            }
            catch (Exception ex)
            {
                // watch that we do not create an infinite loop by generating another event.
                TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::HSEvent exception received: '" + ex.ToString() + "'");
            }
        }

        #endregion

        #region "    Testing and Diagnostics  "

#if false
        public void SendMessageToUnit(int unitno, string szMessage)
        {
            // need to check that we have successfully initialized                if (m_objSACommMgr != null)
            {
                TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::SendMessageToUnit '" + szMessage + "'");

                //	send the message to the correct unit
                m_objHvacUnitList[unitno].SendMessage(szMessage);

            }
        }

        public void SendMessageToAllUnits(string szMessage)
        {
            // need to check that we have successfully initialized                if (m_objSACommMgr != null)
            {
                foreach (clsHvacController objUnit in m_objHvacUnitList)
                {
                    if (objUnit.Enabled)
                    {
                        clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::SendMessageToAllUnits -> unit " + objUnit.UnitNumber);
                        objUnit.SendMessage(szMessage);
                    }
                }
            }
        }
        public void SendSetTimeMessageToAllUnits()
        {
            //Day of week is encoded differently between Enviracom and Windows
            // Enviracom encodes as follows:
            // Bits 0-3:	Day, 0 = Mon, 1 = Tue, . . ., 6 = Sun.
            // .NET encodes as follows:
            //Sunday = 0,
            //Monday = 1,
            //Tuesday = 2,
            //Wednesday = 3,
            //Thursday = 4,
            //Friday = 5,
            //Saturday = 6

            DateTime dt = DateTime.Now;


            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::SendSetTimeMessageToAllUnits " + (int)(dt.DayOfWeek == 0 ? DayOfWeek.Saturday : (dt.DayOfWeek - 1)) + " " + dt.Day + "/" + dt.Month + "/" + dt.Year + " -- " + dt.Hour + ":" + dt.Minute + ":" + dt.Second);
            string msg = m_objMessageMgr.FormatChangeTimeMessage(0, (int)(dt.DayOfWeek == 0 ? DayOfWeek.Saturday : (dt.DayOfWeek - 1)), dt.Month, dt.Day, dt.Year, dt.Hour, dt.Minute, dt.Second);
            SendMessageToAllUnits(msg);
        }
        public void SendTestMessageToAllUnits()
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "SendTestMessageToAllUnits to all units");
//             string msg = m_objMessageMgr.FormatQuerySystemSwitchMessage(i);
//             SendMessageToAllUnits(msg);
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
#endif
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
        public void SetHomeSeerCallback(clsHSPI objHSPI)
        {
            //	Assign locally 
            m_ifHSPI = objHSPI;
            m_ifHSApp = (hsapplication)m_ifHSPI.GetHSIface();
        }

        public void SetHomeSeerEventCallback(object objEventHandler)
        {
            int nEvent = HomeSeer.EV_TYPE_CONFIG_CHANGE;
            ifHSApp.RegisterEventCB(ref nEvent, ref objEventHandler);
        }

        public void EnableHomeSeerAccess()
        {
            //	Log the change
            TraceEvent(TraceEventType.Information, "clsEnviracomApp::EnableHomeSeerAccess: enabling homeseer interfaces");

            SetHomeSeerEventCallback(this);

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
            TraceEvent(TraceEventType.Information, "clsEnviracomApp::DisableHomeSeerAccess: disabling homeseer interfaces");

            foreach (clsHvacController objUnit in m_objHvacUnitList)
            {
                if (objUnit.Enabled)
                    objUnit.DisableHomeSeerAccess();
            }

            foreach (clsThermostatZone objZone in m_objThermostatList)
            {
                objZone.DisableHomeSeerAccess();
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
                    else if ((iZoneNumber >= 1) && (iZoneNumber <= 9)) // if we errornously thought we had an unzoned system before...
                    {
                        unit.IsZoneManager = true; // change what we thought to what is...

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
                clsEnviracomApp.TraceEvent(TraceEventType.Error, "clsEnviracomApp::SeenZoneInstance unit not found: " + iUnitNumber + " for zone: " + iZoneNumber);
            }
        }

        #region "  Message Processing Methods  "

        public void ProcessZoneManagerMessage(int iUnitNumber, int iZoneManager)
        {
            if (iZoneManager > 0)
                m_objHvacUnitList[iUnitNumber].IsZoneManager = true;

            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingZoneManagerMessage zone manager: " + iZoneManager);
        }


        public void ProcessRoomTempMessage(int iUnitNumber, int iZoneNumber, int iTemperature, bool bRoomSensorFault, bool bRoomDegreeCelsius)
        {
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber,iZoneNumber)) != null)
            {
                zone.CurrentTemp = iTemperature;
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingRoomTempMessage for zone: " + iZoneNumber);
        }

        public void ProcessRoomHumidityMessage(int iUnitNumber, int iZoneNumber, int iRoomHumidity)
        {
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber,iZoneNumber)) != null)
            {
                zone.CurrentHumidity = iRoomHumidity;
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingRoomHumidityMessage for zone: " + iZoneNumber);
        }
        
        
        public void ProcessOutdoorTempMessage(int iUnitNumber, int iZoneNumber, int iOutdoorTemperature)
        {
            float fOutdoorTemperatureC = ((float)(iOutdoorTemperature)) / (float)100;
            float fOutdoorTemperatureF = (fOutdoorTemperatureC * ((float)1.8)) + (float)32;


            foreach (clsThermostatZone zone in m_objThermostatList)
            {
                zone.OutdoorTemp = fOutdoorTemperatureF;
            }
            m_objHvacUnitList[iUnitNumber].SetUnitDeviceStatus(fOutdoorTemperatureF.ToString("00"));
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingOutdoorTempMessage for zone: " + iZoneNumber);
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
                        zone.HoldStatus = clsThermostatInformation.HoldStatusValues.Program;
                        break;
                    case 0x01:
                        zone.HoldStatus = clsThermostatInformation.HoldStatusValues.Temporary;
                        break;
                    case 0x02:
                        zone.HoldStatus = clsThermostatInformation.HoldStatusValues.Hold;
                        break;
                    default:
                        zone.HoldStatus = clsThermostatInformation.HoldStatusValues.Uninitialized;
                        TraceEvent(TraceEventType.Warning, "clsEnviracomApp::ProcessSetPointMessage: unexpected value received for hold status: " + iHoldStatus);
                        break;
                }
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingSetPointMessage for zone: " + iZoneNumber);
        }
        public void ProcessTimeMessage(int iUnitNumber, int iZoneNumber, int iDayOfWeek, int iMonth, int iDay, int iYear, int iHours, int iMinutes, int iSeconds)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingTimeMessage for zone: " + iZoneNumber);
        }
        public void ProcessErrorMessage(int iUnitNumber, int iZoneNumber, int iUnknown_0, int iUnknown_1, int iErrorNumber, int iUnknown_3, int iUnknown_4, int iUnknown_5)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingErrorMessage for zone: " + iZoneNumber);
        }
        public void ProcessSystemSwitchMessage(int iUnitNumber, int iZoneNumber, int iSystemSwitch, int iAutoCurrentMode, int iExtraModeByte, int iAllowedModes)
        {
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber, iZoneNumber)) != null)
            {
                zone.SystemSwitch = (clsThermostatInformation.SystemSwitchValues)(iSystemSwitch);
                zone.AutoCurrentMode = (clsThermostatInformation.AutoModeValues)(iAutoCurrentMode);
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingSystemSwitchMessage for zone: " + iZoneNumber);
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
                        zone.SystemState = clsThermostatInformation.SystemStateValues.HeatModeInactive;
                        zone.SystemStateActivity = clsThermostatInformation.StateActivityValues.Off;
                        break;
                    case 0x11:
                        zone.SystemState = clsThermostatInformation.SystemStateValues.HeatModeActive;
                        zone.SystemStateActivity = clsThermostatInformation.StateActivityValues.Heat;
                        break;
                    case 0x20:
                        zone.SystemState = clsThermostatInformation.SystemStateValues.CoolModeInactive;
                        zone.SystemStateActivity = clsThermostatInformation.StateActivityValues.Off;
                        break;
                    case 0x21:
                        zone.SystemState = clsThermostatInformation.SystemStateValues.CoolModeActive;
                        zone.SystemStateActivity = clsThermostatInformation.StateActivityValues.Cool;
                        break;
                    default:
                        zone.SystemState = clsThermostatInformation.SystemStateValues.Uninitialized;
                        zone.SystemStateActivity = clsThermostatInformation.StateActivityValues.Uninitialized;
                        TraceEvent(TraceEventType.Warning, "clsEnviracomApp::ProcessSystemStateMessage: unexpected value received for system state: " + iSystemState);
                        break;
                }
                switch (iStageOneState)
                {
                    case 0x10:
                        zone.StageOneState = clsThermostatInformation.StageStateValues.ConventionalInactive;
                        break;
                    case 0x11:
                        zone.StageOneState = clsThermostatInformation.StageStateValues.ConventionalActive;
                        break;
                    case 0x40:
                        zone.StageOneState = clsThermostatInformation.StageStateValues.HeatPumpInactive;
                        break;
                    case 0x41:
                        zone.StageOneState = clsThermostatInformation.StageStateValues.HeatPumpActive;
                        break;
                    case 0x00:
                        zone.StageOneState = clsThermostatInformation.StageStateValues.Uninitialized;
                        break;
                    default:
                        zone.StageOneState = clsThermostatInformation.StageStateValues.Uninitialized;
                        TraceEvent(TraceEventType.Warning, "clsEnviracomApp::ProcessSystemStateMessage: unexpected value received for stage one state: " + iStageOneState);
                        break;
                }
                switch (iStageTwoState)
                {
                    case 0x10:
                        zone.StageTwoState = clsThermostatInformation.StageStateValues.ConventionalInactive;
                        break;
                    case 0x11:
                        zone.StageTwoState = clsThermostatInformation.StageStateValues.ConventionalActive;
                        break;
                    case 0x40:
                        zone.StageTwoState = clsThermostatInformation.StageStateValues.HeatPumpInactive;
                        break;
                    case 0x41:
                        zone.StageTwoState = clsThermostatInformation.StageStateValues.HeatPumpActive;
                        break;
                    case 0x00:
                        zone.StageTwoState = clsThermostatInformation.StageStateValues.Uninitialized;
                        break;
                    default:
                        zone.StageTwoState = clsThermostatInformation.StageStateValues.Uninitialized;
                        TraceEvent(TraceEventType.Warning, "clsEnviracomApp::ProcessSystemStateMessage: unexpected value received for stage two state: " + iStageTwoState);
                        break;
                }
                switch (iStageThreeState)
                {
                    case 0x10:
                        zone.StageThreeState = clsThermostatInformation.StageStateValues.FossilFuelInactive;
                        break;
                    case 0x11:
                        zone.StageThreeState = clsThermostatInformation.StageStateValues.FossilFuelActive;
                        break;
                    case 0x50:
                        zone.StageThreeState = clsThermostatInformation.StageStateValues.ElectricInactive;
                        break;
                    case 0x51:
                        zone.StageThreeState = clsThermostatInformation.StageStateValues.ElectricActive;
                        break;
                    case 0x00:
                        zone.StageThreeState = clsThermostatInformation.StageStateValues.Uninitialized;
                        break;
                    default:
                        zone.StageThreeState = clsThermostatInformation.StageStateValues.Uninitialized;
                        TraceEvent(TraceEventType.Warning, "clsEnviracomApp::ProcessSystemStateMessage: unexpected value received for stage three state: " + iStageThreeState);
                        break;
                }
                switch (iStageFourState)
                {
                    case 0x10:
                        zone.StageFourState = clsThermostatInformation.StageStateValues.FossilFuelInactive;
                        break;
                    case 0x11:
                        zone.StageFourState = clsThermostatInformation.StageStateValues.FossilFuelActive;
                        break;
                    case 0x50:
                        zone.StageFourState = clsThermostatInformation.StageStateValues.ElectricInactive;
                        break;
                    case 0x51:
                        zone.StageFourState = clsThermostatInformation.StageStateValues.ElectricActive;
                        break;
                    case 0x00:
                        zone.StageFourState = clsThermostatInformation.StageStateValues.Uninitialized;
                        break;
                    default:
                        zone.StageFourState = clsThermostatInformation.StageStateValues.Uninitialized;
                        TraceEvent(TraceEventType.Warning, "clsEnviracomApp::ProcessSystemStateMessage: unexpected value received for stage four state: " + iStageFourState);
                        break;
                }
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingSystemStateMessage for zone: " + iZoneNumber);
        }
        public void ProcessEquipmentFaultMessage(int iUnitNumber, int iZoneNumber, int iUnknown_0, int iUnknown_1, int iUnknown_2, int iUnknown_3, int iUnknown_4)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingEquipmentFaultMessage for zone: " + iZoneNumber);
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
                            zone.BlowerFanStatus = clsThermostatInformation.BlowerFanValues.Off;
                            break;
                        case 0x64:
                            zone.BlowerFanStatus = clsThermostatInformation.BlowerFanValues.Slow;
                            break;
                        case 0xC8:
                            zone.BlowerFanStatus = clsThermostatInformation.BlowerFanValues.Fast;
                            break;
                        default:
                            zone.BlowerFanStatus = clsThermostatInformation.BlowerFanValues.Uninitialized;
                            TraceEvent(TraceEventType.Warning, "clsEnviracomApp::ProcessBlowerFanSpeedMessage: unexpected value received for fan speed: " + iBlowerFanSpeed);
                            break;
                    }
                    clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessBlowerFanSpeedMessage for unit: " + zone.Unit + " zone: " + zone.Zone);
                }
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingBlowerFanSpeedMessage");
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
                            zone.AuxHeatStatus = clsThermostatInformation.AuxHeatStatusValues.Off;
                            break;
                        case 0x64:
                            zone.AuxHeatStatus = clsThermostatInformation.AuxHeatStatusValues.StageOne;
                            break;
                        case 0xC8:
                            zone.AuxHeatStatus = clsThermostatInformation.AuxHeatStatusValues.StageTwo;
                            break;
                        default:
                            zone.AuxHeatStatus = clsThermostatInformation.AuxHeatStatusValues.Uninitialized;
                            TraceEvent(TraceEventType.Warning, "clsEnviracomApp::ProcessAuxHeatStatusMessage: unexpected value received for aux heat status: " + iAuxHeatStatus);
                            break;
                    }
                    clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessAuxHeatStatusMessage for unit: " + zone.Unit + " zone: " + zone.Zone);
                }
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingAuxHeatStatusMessage");
        }
        public void ProcessCoolingHeatPumpStatusMessage(int iUnitNumber, int iCoolingHeatPumpState, int iUnknown_1, int iUnknown_2, int iUnknown_3)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingCoolingHeatPumpMessage");
        }
        public void ProcessHeatingHeatPumpStatusMessage(int iUnitNumber, int iHeatingHeatPumpState, int iUnknown_1, int iUnknown_2, int iUnknown_3)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingHeatingHeatPumpMessage");
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
                                zone.CirculateFanStatus = clsThermostatInformation.CirculateFanValues.Off;
                                break;
                            case 0xC8:
                                zone.CirculateFanStatus = clsThermostatInformation.CirculateFanValues.On;
                                break;
                            default:
                                zone.CirculateFanStatus = clsThermostatInformation.CirculateFanValues.Uninitialized;
                                TraceEvent(TraceEventType.Warning, "clsEnviracomApp::ProcessSystemStateMessage: unexpected value received for fan state: " + iCirculateFanStatus);
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
                            zone.CirculateFanStatus = clsThermostatInformation.CirculateFanValues.Off;
                            break;
                        case 0xC8:
                            zone.CirculateFanStatus = clsThermostatInformation.CirculateFanValues.On;
                            break;
                        default:
                            zone.CirculateFanStatus = clsThermostatInformation.CirculateFanValues.Uninitialized;
                            TraceEvent(TraceEventType.Warning, "clsEnviracomApp::ProcessSystemStateMessage: unexpected value received for fan state: " + iCirculateFanStatus);
                            break;
                    }
                }
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingCirculateFanMessage for zone: " + iZoneNumber + " value " + iCirculateFanStatus);
        }
        public void ProcessDehumidifierStateMessage(int iUnitNumber,int iZoneNumber, int iDehumidifierMode, int iExternalDehumidifierState)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingDehumidifierStateMessage for zone: " + iZoneNumber);
        }
        public void ProcessHumidifierStateMessage(int iUnitNumber,int iZoneNumber, int iUnknown_0, int iHumidifierState)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingHumidifierStateMessage for zone: " + iZoneNumber);
        }
        public void ProcessExternalVentilationStateMessage(int iUnitNumber,int iZoneNumber, int iUnknown_0, int iExternalVentilationState)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingExternalVentilationStateMessage for zone: " + iZoneNumber);
        }
        public void ProcessFanSwitchMessage(int iUnitNumber,int iZoneNumber, int iFanModeOne, int iFanModeTwo)
        {
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber, iZoneNumber)) != null)
            {
                if (iFanModeOne == 0 && iFanModeTwo == 0x20)
                    zone.FanSwitch = clsThermostatInformation.FanSwitchValues.AutoNormal;
                else if (iFanModeOne == 0 && iFanModeTwo == 0xA0)
                    zone.FanSwitch = clsThermostatInformation.FanSwitchValues.AutoCirculate;
                else if (iFanModeOne == 0xC8 && iFanModeTwo == 0x20)
                    zone.FanSwitch = clsThermostatInformation.FanSwitchValues.OnNormal;
                else if (iFanModeOne == 0xC8 && iFanModeTwo == 0xA0)
                    zone.FanSwitch = clsThermostatInformation.FanSwitchValues.OnCirculate;
                else
                    zone.FanSwitch = clsThermostatInformation.FanSwitchValues.Uninitialized;
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingFanModeMessage for zone: " + iZoneNumber);
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
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingDehumidifierSetPointMessage for zone: " + iZoneNumber);
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
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingHumidifierSetPointMessage for zone: " + iZoneNumber);
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
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingScheduleMessage for zone: " + iZoneNumber);
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
                        clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingAirFilterMessage for unit: " + zone.Unit + " zone: " + zone.Zone);
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
                    clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::ProcessingAirFilterMessage for zone: " + iZoneNumber);
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
                                zone.DamperStatus = clsThermostatInformation.DamperStatusValues.Closed;
                                break;
                            case 0xC8:
                                zone.DamperStatus = clsThermostatInformation.DamperStatusValues.Open;
                                break;
                            default:
                                zone.DamperStatus = clsThermostatInformation.DamperStatusValues.Uninitialized;
                                TraceEvent(TraceEventType.Warning, "clsEnviracomApp::ProcessDamperStatusMessage: unexpected value received for damper state: " + iDamperStatus);
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
                            zone.DamperStatus = clsThermostatInformation.DamperStatusValues.Closed;
                            break;
                        case 0xC8:
                            zone.DamperStatus = clsThermostatInformation.DamperStatusValues.Open;
                            break;
                        default:
                            zone.DamperStatus = clsThermostatInformation.DamperStatusValues.Uninitialized;
                            TraceEvent(TraceEventType.Warning, "clsEnviracomApp::ProcessDamperStatusMessage: unexpected value received for damper state: " + iDamperStatus);
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
                clsEnviracomApp.TraceEvent(TraceEventType.Warning, "clsEnviracomApp::ProcessingAquastatStatusMessage for invalid zone instance " + iZoneNumber);
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
        public void CreateModeMessage(int iUnitNumber, int iZoneNumber, clsThermostatInformation.SystemSwitchValues eModeStatus)
        {
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber, iZoneNumber)) != null)
            {
                switch (eModeStatus)
                {
                    case clsThermostatInformation.SystemSwitchValues.EmergencyHeat:
                    case clsThermostatInformation.SystemSwitchValues.Heat:
                    case clsThermostatInformation.SystemSwitchValues.Off:
                    case clsThermostatInformation.SystemSwitchValues.Cool:
                    case clsThermostatInformation.SystemSwitchValues.Auto:
                        string msg = MessageMgr.FormatChangeModeMessage(iZoneNumber, (int)eModeStatus);
                        m_objHvacUnitList[zone.Unit].SendMessage(msg);
                        TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateModeMessage for zone: " + iZoneNumber);
                        break;
                    case clsThermostatInformation.SystemSwitchValues.Uninitialized:
                    default:
                        TraceEvent(TraceEventType.Warning, "clsEnviracomApp::CreateModeMessage: unexpected value received for mode status: " + eModeStatus.ToString());
                        break;
                }
            }
        }
        public void CreateHoldMessage(int iUnitNumber, int iZoneNumber, clsThermostatInformation.HoldStatusValues eHoldStatus)
        {
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber, iZoneNumber)) != null)
            {
                int iCoolSetPoint = ConvertTemperatureSetPoint(zone.CoolSetPoint);
                int iHeatSetPoint = ConvertTemperatureSetPoint(zone.HeatSetPoint);
                switch (eHoldStatus)
                {
                    case clsThermostatInformation.HoldStatusValues.Program:
                    case clsThermostatInformation.HoldStatusValues.Temporary:
                    case clsThermostatInformation.HoldStatusValues.Hold:
                        string msg = MessageMgr.FormatChangeSetPointsMessage(iZoneNumber, iHeatSetPoint, (int)eHoldStatus, iCoolSetPoint, 0, 0, 0, 0);
                        m_objHvacUnitList[zone.Unit].SendMessage(msg);
                        TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateHoldMessage for zone: " + iZoneNumber);
                        break;
                    case clsThermostatInformation.HoldStatusValues.Uninitialized:
                    default:
                        TraceEvent(TraceEventType.Warning, "clsEnviracomApp::CreateHoldMessage: unexpected value received for hold status: " + zone.HoldStatus.ToString());
                        break;
                }
            }
        }
        public void CreateQuerySchedulesMessage()
        {
            string msg = MessageMgr.FormatQuerySchedulesMessage(0);


            foreach (clsHvacController objUnit in m_objHvacUnitList)
            {
                objUnit.SendMessage(msg);
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateQuerySchedulesMessage");
        }
        public void CreateQuerySchedulesMessage(int iUnitNumber, int iZoneNumber)
        {
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber, iZoneNumber)) != null)
            {
                string msg = MessageMgr.FormatQuerySchedulesMessage(zone.Zone);

                m_objHvacUnitList[zone.Unit].SendMessage(msg);
                clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateQuerySchedulesMessage");
            }
            else
                clsEnviracomApp.TraceEvent(TraceEventType.Warning, "clsEnviracomApp::CreateQuerySchedulesMessage zone not found zone: " + iZoneNumber + " Unit: " + iUnitNumber);
        }

        public void CreateSetClockMessage()
        {

            //Day of week is encoded differently between Enviracom and Windows
            // Enviracom encodes as follows:
            // Bits 0-3:	Day, 0 = Mon, 1 = Tue, . . ., 6 = Sun.
            // .NET encodes as follows:
            //Sunday = 0,
            //Monday = 1,
            //Tuesday = 2,
            //Wednesday = 3,
            //Thursday = 4,
            //Friday = 5,
            //Saturday = 6

            DateTime dt = DateTime.Now;

            CreateTimeMessage((int)(dt.DayOfWeek == 0 ? DayOfWeek.Saturday : (dt.DayOfWeek - 1)), dt.Month, dt.Day, dt.Year, dt.Hour, dt.Minute, dt.Second);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateSetClockMessage: " + (int)(dt.DayOfWeek == 0 ? DayOfWeek.Saturday : (dt.DayOfWeek - 1)) + " " + dt.Day + "/" + dt.Month + "/" + dt.Year + " -- " + dt.Hour + ":" + dt.Minute + ":" + dt.Second);
        }
        public void CreateTimeMessage(int iDayOfWeek, int iMonth, int iDay, int iYear, int iHours, int iMinutes, int iSeconds)
        {
            string msg = MessageMgr.FormatChangeTimeMessage(0, iDayOfWeek, iMonth, iDay, iYear, iHours, iMinutes, iSeconds);

            foreach (clsHvacController objUnit in m_objHvacUnitList)
            {
                objUnit.SendMessage(msg);
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::CreateTimeMessage: " + iDayOfWeek + " " + iDay + "/" + iMonth + "/" + iYear + " -- " + iHours + ":" + iMinutes + ":" + iSeconds);
 
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
        public void CreateFanSwitchMessage(int iUnitNumber, int iZoneNumber,  clsThermostatInformation.FanSwitchValues eFanSwitchStatus)
        {

            int iFanModeOne = -1;
            int iFanModeTwo = -1;
            clsThermostatZone zone;

            //	See if we can find it in our zone list
            if ((zone = GetZone(iUnitNumber, iZoneNumber)) != null)
            {

                switch (eFanSwitchStatus)
                {
                    case clsThermostatInformation.FanSwitchValues.AutoNormal:
                        iFanModeOne = 0;
                        iFanModeTwo = 0x20;
                        break;
                    case clsThermostatInformation.FanSwitchValues.AutoCirculate:
                        iFanModeOne = 0;
                        iFanModeTwo = 0xA0;
                        break;
                    case clsThermostatInformation.FanSwitchValues.OnNormal:
                        iFanModeOne = 0xC8;
                        iFanModeTwo = 0x20;
                        break;
                    case clsThermostatInformation.FanSwitchValues.OnCirculate:
                        iFanModeOne = 0xC8;
                        iFanModeTwo = 0xA0;
                        break;
                    case clsThermostatInformation.FanSwitchValues.Uninitialized:
                    default:
                        TraceEvent(TraceEventType.Warning, "clsEnviracomApp::CreateFanSwitchMessage: unexpected value received for fan switch status: " + eFanSwitchStatus.ToString());
                        break;
                }
            }

            if (eFanSwitchStatus != clsThermostatInformation.FanSwitchValues.Uninitialized) 
            {
                string msg = MessageMgr.FormatChangeFanMessage(iZoneNumber, iFanModeOne, iFanModeTwo);
                m_objHvacUnitList[zone.Unit].SendMessage(msg);
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

                string msg = MessageMgr.FormatChangeSetPointsMessage(iZoneNumber, iHeatSetPoint, (int)zone.HoldStatus, iCoolSetPoint, 0, 0, 0, 0);
                m_objHvacUnitList[zone.Unit].SendMessage(msg);
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

            for (int i = 1; i <= Count; i++)
            {
                try
                {

                    string portname = GetConfig(CFG_KEY_UNIT + i.ToString("0") + CFG_KEY_PORT, "NOTFOUND");
                    TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::InitUnits: " + CFG_KEY_UNIT + i.ToString("0") + CFG_KEY_PORT + " returned portname: " + portname);
                    if (portname != "NOTFOUND")
                    {
                        clsHvacController objUnit = new clsHvacController();
                        objUnit.UnitNumber = i - 1; // reinvestigate whether we want to have zero based unit numbers @@@@@
                        objUnit.Port = portname;
                        objUnit.Enabled = true;
                        m_objHvacUnitList.Add(objUnit);
                        objUnit.Initialize(); // set up comm manager threads etc
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

        private void InitZones() // need to modify so we initialize zones based on config file rather than discovery @@@@@
        {
            int Count = Convert.ToInt32(GetConfig(CFG_KEY_MAXUNITS, "1"));

            for (int i = 1; i <= Count; i++)
            {
                InitUnitZones(Count);
            }
        }



        private void InitUnitZones(int iUnitNumber) // need to modify so we initialize zones based on config file rather than discovery @@@@@
        {
            int Count = Convert.ToInt32(GetConfig(CFG_KEY_UNIT + iUnitNumber.ToString("0") + CFG_KEY_COUNT, "0"));

            for (int i = 1; i <= Count; i++)
            {
                try
                {

                    string devicecode = GetConfig(CFG_KEY_UNIT + iUnitNumber.ToString("0") + CFG_KEY_ZONE + i.ToString("0"), "NOTFOUND");
                    TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::InitUnits: " + CFG_KEY_UNIT + iUnitNumber.ToString("0") + CFG_KEY_ZONE + i.ToString("0") + " returned zone device code: " + devicecode);
                    if (devicecode != "NOTFOUND")
                    {
//                        TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::InitUnits: " + objUnit.Name + " number: " + objUnit.UnitNumber + " on port " + objUnit.Port);
                    }
                }
                catch (Exception ex)
                {
                    //	If we get an exception, that is bad, so let everyone know
                    TraceEvent(TraceEventType.Critical, "clsHEnviracomApp::InitZones: Received exception: " + ex.ToString());
                }
            }

        }



        private void InitResponses()
        {
            //            m_objResponseList.DoPeriodic += new PeriodicList<clsResponse>.DoPeriodicDlgt(ProcessPeriodicEvent);
            //            m_objResponseList.DoImmediate += new PeriodicList<clsResponse>.DoImmediateDlgt(ProcessResponseEvent);
            //            m_objResponseList.DoException += new PeriodicList<clsResponse>.DoExceptionDlgt(ProcessExceptionEvent);
            //            m_objResponseList.AddToPeriodicList(new clsResponse("FRED", 5));
            //            m_objResponseList.AddToPeriodicList(new clsResponse("BOB", 10));
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



        private void DeleteHomeseerMasterDevice() // remove, we don't ever want to delete the device @@@@@
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
            dev.hc = Convert.ToString(Convert.ToChar(m_ifHSPI.GetNextFreeIOCode())); //@@@@@
            dev.dc = Convert.ToString(GetLastFreeDevCode());    //@@@@@
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

        private clsThermostatZone GetZone(DeviceClass dev)
        {
            return m_objThermostatList.Find(delegate (clsThermostatZone tz)
            {
                return ((tz.DevCode == dev.dc) && (tz.HouseCode == dev.hc));
            });
        }

        private clsHvacController GetUnit(DeviceClass dev)
        {
            return m_objHvacUnitList.Find(delegate (clsHvacController hc)
            {
                return ((hc.DevCode == dev.dc) && (hc.HouseCode == dev.hc));
            });
        }


        private void DeleteZone(int nUnit, int nNumber)
        {
            clsThermostatZone objZone;

            //	Get the zone
            objZone = GetZone(nUnit, nNumber);

            //	Remove the zone from the list
            lock (m_objThermostatList)
            {
                m_objThermostatList.Remove(objZone);
            }

            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsEnviracomApp::DeleteZone: Deleting zone (" +
                nNumber.ToString() + ":" +
                objZone.Name + ")");

            //	Finally, remove the zone and dev code from EnviracomApp
            if (objZone.DeviceAttached == true)
            {
                objZone.DetachZoneDevice();
            }
        }

        private clsThermostatZone CreateZone(int iUnitNumber, int iZoneNumber)
        {
            clsThermostatZone objZone;

            objZone = new clsThermostatZone();
            objZone.Zone = iZoneNumber;
            objZone.Unit = iUnitNumber;
            objZone.AttachHomeseerZoneDevice();

            //	Add the zone from the list
            lock (m_objThermostatList)
            {
                objZone.ThermostatNumber = NumberOfThermostats;
                m_objThermostatList.Add(objZone);
            }

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

        private bool ProcessPeriodicEvent(object sender, PeriodicListEventArgs<clsResponse> args)
        {
            TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessPeriodicEvent: processing a periodic event: " + args.Work.name + " ticks: " + args.Work.ticks);
            return --args.Work.ticks == 0;
        }

        private bool ProcessResponseEvent(object sender, PeriodicListEventArgs<clsResponse> args)
        {
            TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessResponseEvent: processing a response event: " + args.Work.name + " ticks: " + args.Work.ticks);
            return true;
        }

        private void ProcessExceptionEvent(object sender, PeriodicListExceptionEventArgs args)
        {
            TraceEvent(TraceEventType.Verbose, "clsEnviracomApp::ProcessExceptionEvent: processing an exception event: " + args.Exception.Message);
        }

        public int GetThermostatInternalIndex(int hststatno)
        {
            return hststatno - 1;
        }

        public static int GetThermostatHomeseerIndex(int emtstatno)
        {
            return emtstatno + 1;
        }

        public int GetThermostatHomeseerIndex(string name)
        {
            clsThermostatZone therm = m_objThermostatList.Find(delegate(clsThermostatZone tz)
            {
                return tz.Name == name;
            });

            if (therm != null)
            {
                TraceEvent(TraceEventType.Information, "clsEnviracomApp::GetThermostatHomeseerIndex: found thermostat name: \"" + name + "\" number: " + therm.ThermostatNumber);
                return GetThermostatHomeseerIndex(therm.ThermostatNumber);//therm.ThermostatNumber; // shouldn't this be GetThermostatHomeseerIndex(therm.ThermostatNumber)???
            }
            else
            {
                TraceEvent(TraceEventType.Information, "clsEnviracomApp::GetThermostatHomeseerIndex: did NOT find thermstat: " + name);
//                TraceEvent(TraceEventType.Information, "clsEnviracomApp::m_objThermostatList: " + m_objThermostatList.ToString());
                return -1;
            }
        }

        public int GetThermostatHomeseerIndex(string dc, string hc)
        {
            clsThermostatZone therm = m_objThermostatList.Find(delegate(clsThermostatZone tz)
            {
                return ((tz.DevCode == dc) && (tz.HouseCode == hc));
            });

            if (therm != null)
                return therm.ThermostatNumber;
            else
                return -1;
        }

        #endregion
    }
}
