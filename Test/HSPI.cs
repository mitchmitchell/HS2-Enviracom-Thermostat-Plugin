using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Threading;
using Scheduler;
using Scheduler.Classes;

namespace HSPI_ENVIRACOM_MANAGER
{
	/// <summary>
	/// Summary description for HSPI.
	/// </summary>
    [Serializable()]
    public class HSPI : MarshalByRefObject
	{
		/*
		 * NOTE:  HSPI is the interface used by HomeSeer to get into our application.
		 * The HSPI object may be instantiated multiple times from HomeSeer and a 
		 * Singleton Pattern is really desired, so I used the Singleton Pattern on 
		 * my EnviracomApp object and the HSPI object passes everything through to that
		 * object.
		 */
        #region "  ENUMS and constants  "
        public const string HSPI_PLUGIN_NAME = "Enviracom Manager";

		#endregion 

		#region "  Members  "
		private clsEnviracomApp			m_objApp			= clsEnviracomApp.GetInstance();
		#endregion

		#region "  Accessor Methods for Members  "
		#endregion

		#region "  Constructors and Destructors   "
		public HSPI()
		{
		}

		~HSPI()
		{
		}
		#endregion

		#region "  Initialization and Cleanup   "
		#endregion

		/*
		 *	This section is for the shared procedures that are common
		 *	to all types of interfaces
		 */
		#region	"  Common Plug-In Interface Procedures  " 
		public string	Name()
		{
			//	Short m_sUnitName for this plugin
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose,"clsHSPI::Name");
            return clsEnviracomApp.PLUGIN_IN_NAME_SHORT;
		}

		public int		Capabilities()
		{
			//	OR the capabilities together to let HS know what this plug-in
			//	is capable of doing
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose,"clsHSPI::Capabilities");
            return HomeSeer.CA_THERM | HomeSeer.CA_IO;
		}

		public int		AccessLevel()
		{
			//	Return the type of licensing that the plug-in uses
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose,"clsHSPI::AccessLevel");
            return HomeSeer.AL_FREE;
		}

		public bool		HSCOMPort()
		{
			//	TRUE if plug-in uses COM ports and you want to use HS pages 
			//	for configuration, FALSE otherwise
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose,"clsHSPI::HSCOMPort");
            return false;
		}

		public bool		SupportsHS2()
		{
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose,"clsHSPI::SupportsHS2");
            return true;
		}

        public bool SupportsConfigDevice()
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::SupportsConfigDevice");
            return false;
        }

        public bool SupportsConfigDevice2()
		{
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::SupportsConfigDevice2");
            return false;
		}

        public bool SupportsConditionUI()
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::SupportsCondtionUI");
            return false;
        }

        public bool SupportsTriggerUI()
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::TriggerUI");
            return false;
        }

        public bool SupportsActionUI()
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::ActionUI");
            return false;
        }
		public void	RegisterCallback(ref object frm)
		{
			//	Set the global information for the HS interfaces here now that we have them
			m_objApp.SetHomeSeerCallback( (clsHSPI)frm );
            m_objApp.Initialize();
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose,"clsHSPI::RegisterCallback");
		}

		public short	InterfaceStatus()
		{
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose,"clsHSPI::InterfaceStatus");
			//	TODO ... get the real interface status (if I have any)
			return HomeSeer.IS_ERR_NONE;
		}

		#endregion 

		/*
		 *	This section is for the procedures that are necessary for an I/O
		 *	device.  This is known as an Other type in the capabilities method.
		 */
		#region "  I/O - Other Plug-In Related Procedures  "
		public string InitIO(long port)
		{
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose,"clsHSPI::InitIO");
            try
			{
	
				//	Register the config link here
                object cfg = new clsWebManager();
                m_objApp.ifHSApp.RegisterConfigLink(ref cfg, clsEnviracomApp.PLUGIN_IN_NAME_SHORT);
                m_objApp.ifHSApp.RegisterLinkEx(ref cfg, clsEnviracomApp.PLUGIN_IN_NAME_SHORT);

                //	Tell the hvac controller to go find it's device
				m_objApp.EnableHomeSeerAccess();
			}
			catch ( Exception ex )
			{
                clsEnviracomApp.TraceEvent(TraceEventType.Critical, "clsHSPI::InitIO: Failed to init: Exception: " + ex.ToString());
                return "HSPI::InitIO: Failed to init: Exception: " + ex.ToString();
            }

			return null;
		}

        public void SetIO(DeviceClass device, string houseCode, string deviceCode, int command, int brightness, int data1, int data2)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::SetIO");

            if (houseCode == m_objApp.HouseCode)
            {
                // make sure we own the device before we mess with it.
            }
        }

		public void ShutdownIO()
		{
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose,"clsHSPI::ShutdownIO");
            try
			{

				//	Tell the hvac controller to disable HomeSeer access
				m_objApp.DisableHomeSeerAccess();
			}
			catch ( Exception ex )
			{
                clsEnviracomApp.TraceEvent(TraceEventType.Critical, "clsHSPI::ShutdownIO: Failed to shutdown: Exception: " + ex.ToString());
			}
		}

		public void ConfigIO()
		{
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose,"clsHSPI::ConfigIO");
        }

        #endregion

        #region "  Test Message Procedures  "
        #endregion

        #region "     Public Methods      "

        #region " Thermostat API Procedures "

        //Information Return Values
        //For the thermostat Mode:
        //0=Off, 1=Heat, 2=Cool, 3=Auto, 4=Aux (If Supported, a.k.a. Emergency Heat)
        //For the thermostat fan mode:
        //0=Auto, 1=On
        //For the thermostat hold mode:
        //0=Off Hold, 1=Hold (if Supported)
        //For the Cmd____ functions:
        //0=Success, n <> 0 =Failure Error Code

        #region " Thermostat Configuration Properties/Functions "


        // 1's based number representing the number of thermostats that the plug-in is managing.
        public int NThermostats()
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::NThermostats");
            return m_objApp.NumberOfThermostats;
        }

        // Call ThermLoc(x) for each thermostat to retrieve the location name for the thermostat – the location is part of the thermostat’s name, and if the plug-in does not name the thermostat using the HomeSeer “Location Name” syntax, then a null value (“”) should be returned.
        public string ThermLoc(int HSThermostat)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::ThermLoc");
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);
            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))
                return m_objApp.Thermostats[Thermostat].Location;
            else
                return "";
        }
        // Call ThermName(x) for each thermostat to retrieve the name for the thermostat.
        public string ThermName(int HSThermostat)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::ThermName");
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);
            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))
                return m_objApp.Thermostats[Thermostat].Name;
            else
                return "";
        }
        // Call NTemps(x) for each thermostat that the plug-in supports, and it should return a 1’s based integer representing the number of temperature readings that each thermostat supports (e.g. a thermostat might support an indoor and outdoor temperature reading)
        public int NTemps(int HSThermostat)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::NTemps");
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);
            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))
                return 1;
            else
                return 0;
        }
        // Call SupportsStat(x,y) for each temperature reading that each thermostat supports to determine if there is a full thermostat there or (Return=False) if it is only a temperature sensor.
        public bool SupportsStat(int HSThermostat, int Sensor)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::SupportsStat");
            return true;
        }
        // Call SupportsCoolSet(x,y) for each SupportsStat=True to determine if the thermostat supports a separate cool setpoint. If the Boolean return is False, then the setpoint is referred to as “Setpoint”. If the return is True, then the primary setpoint is referred to as HEAT and the secondary setpoint is COOL.
        public bool SupportsCoolSet(int HSThermostat, int Sensor)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::SupportsCoolStat");
            return true;
        }
        // Call SupportsDirect(x,y) for each SupportsStat=True to determine if the thermostat supports direct temperature setting. The return of False means that the thermostat supports setbacks rather than direct temperature setting.
        public bool SupportsDirect(int HSThermostat, int Sensor)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::SupportsDirect");
            return true;
        }
        // Call SupportsHold(x,y) for each SupportsStat=True, to determine if the thermostat supports a hold mode on the thermostat.
        public bool SupportsHold(int HSThermostat, int Sensor)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::SupportsHold");
            return true;
        }
        // Call SupportsHoldOverride(x,y) for each SupportsStat=True to determine if the thermostat supports overriding the settings of the thermostat when it is in the HOLD mode. The return value indicating True if commands sent while in Hold mode are carried out by the thermostat.
        public bool SupportsHoldOverride(int HSThermostat, int Sensor)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::SupportsHoldOverride");
            return true;
        }
        // Call NumSetbacks(x,y) for each SupportsDirect that returns False to determine how many setbacks the thermostat supports. The returned value is a 1’s based integer.
        public int NumSetbacks(int HSThermostat, int Sensor)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::NumSetbacks");
            return clsScheduleDay.MAXIMUM_SCHEDULE_PERIODS;
        }
        // Call SupportsAux(x,y) for each SupportsStat=True to determine if the thermostat supports an auxiliary (emergency) heat mode.
        public bool SupportsAux(int HSThermostat, int Sensor)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::SupportsAux");
            return true;
        }
        // Call SupportsOperating(x,y) for each SupportsStat=True to determine if the thermostat supports a call to return whether the thermostat is currently calling for heat or cool. If the return is True, then you can ask the thermostat if it is currently heating or cooling.
        public bool SupportsOperating(int HSThermostat, int Sensor)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::SupportsOperating");
            return true;
        }
        // Call ThermAddress(x) for each thermostat to obtain it's hardware native address. For an X-10 thermostat, this would be the house code and letter code. For an RS-485 thermostat, it would be the address number. For a Z-Wave thermostat, this is the Z-Wave Node Zone.
        public string ThermAddress(int HSThermostat)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::ThermAddress");
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);
            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))
                return m_objApp.Thermostats[Thermostat].Unit.ToString() + "^" + m_objApp.Thermostats[Thermostat].Zone.ToString();
            else
                return "";
        }

        #endregion

        #region " Thermostat Information Properties/Functions "

        //Current Temperature
        public double GetTemp(int HSThermostat, int Sensor)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::GetTemp");
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);
            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))
                return m_objApp.Thermostats[Thermostat].CurrentTemp;
            else
                return 0;
        }
        //Heating Setpoint Temperature
        public double GetHeatSet(int HSThermostat, int Sensor)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::GetHeatSet");
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);
            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))
                return m_objApp.Thermostats[Thermostat].HeatSetPoint;
            else
                return 0;
        }
        //Cooling Setpoint Temperature
        public double GetCoolSet(int HSThermostat, int Sensor)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::GetCoolSet");
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);
            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))
                return m_objApp.Thermostats[Thermostat].CoolSetPoint;
            else
                return 0;
        }
        //Current Thermostat Mode Setting = 0=Off, 1=Heat, 2=Cool, 3=Auto, 4=Aux (If Supported, a.k.a. Emergency Heat)
        public int GetModeSet(int HSThermostat, int Sensor)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::GetModeSet");
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);
            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))
            {
                int iModeSet = -1;
                switch (m_objApp.Thermostats[Thermostat].SystemSwitch)
                {
                    case clsThermostatInformation.SystemSwitchValues.EmergencyHeat:
                        iModeSet = 4;
                        break;
                    case clsThermostatInformation.SystemSwitchValues.Heat:
                        iModeSet = 1;
                        break;
                    case clsThermostatInformation.SystemSwitchValues.Off:
                        iModeSet = 0;
                        break;
                    case clsThermostatInformation.SystemSwitchValues.Cool:
                        iModeSet = 2;
                        break;
                    case clsThermostatInformation.SystemSwitchValues.Auto:
                        iModeSet = 3;
                        break;
                    case clsThermostatInformation.SystemSwitchValues.Uninitialized:
                    case clsThermostatInformation.SystemSwitchValues.Unknown_1:
                    case clsThermostatInformation.SystemSwitchValues.Unknown_2:
                    case clsThermostatInformation.SystemSwitchValues.Unknown_3:
                    default:
                        iModeSet = -1;
                        break;
                }
                return iModeSet;
            }
            else
                return -1;
        }
        //Current Thermostat Fan Mode Setting 0=Auto, 1=On
        public int GetFanMode(int HSThermostat, int Sensor)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::GetFanMode");
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);
            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))
            {
                int iFanMode = -1;
                switch (m_objApp.Thermostats[Thermostat].FanSwitch)
                {
                    case clsThermostatInformation.FanSwitchValues.AutoCirculate:
                    case clsThermostatInformation.FanSwitchValues.AutoNormal:
                        iFanMode = 0;
                        break;
                    case clsThermostatInformation.FanSwitchValues.OnCirculate:
                    case clsThermostatInformation.FanSwitchValues.OnNormal:
                        iFanMode = 1;
                        break;
                    case clsThermostatInformation.FanSwitchValues.Uninitialized:
                    default:
                        iFanMode = -1;
                        break;
                }
                return iFanMode;
            }
            else
                return -1;
        }
        //Current Thermostat Hold Mode Setting 0=Off Hold, 1=Hold (if Supported)
        public int GetHoldMode(int HSThermostat, int Sensor)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::GetHoldMode");
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);
            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))
            {
                int iModeSet = -1;
                switch (m_objApp.Thermostats[Thermostat].HoldStatus)
                {
                    case clsThermostatInformation.HoldStatusValues.Hold:
                        iModeSet = 1;
                        break;
                    case clsThermostatInformation.HoldStatusValues.Program:
                        iModeSet = 0;
                        break;
                    case clsThermostatInformation.HoldStatusValues.Temporary:
                        iModeSet = 0;
                        break;
                    case clsThermostatInformation.HoldStatusValues.Uninitialized:
                    default:
                        iModeSet = -1;
                        break;
                }
                return iModeSet;
            }
            else
                return -1;
        }
        //Current Thermostat Operating Mode. e.g. GetModeSet may return AUTO, and GetCurrentMode can then be used to determine if it is currently COOL, HEAT, or AUX) 0=Off, 1=Heat, 2=Cool, 4=Aux
        public int GetCurrentMode(int HSThermostat, int Sensor)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::GetCurrentMode");
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);
            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))
            {
                int iCurrentMode = -1;
                switch (m_objApp.Thermostats[Thermostat].SystemState)
                {
                    case clsThermostatInformation.SystemStateValues.CoolModeActive:
                        iCurrentMode = 2;
                        break;
                    case clsThermostatInformation.SystemStateValues.HeatModeActive:
                        iCurrentMode = 1;
                        break;
                    case clsThermostatInformation.SystemStateValues.CoolModeInactive:
                    case clsThermostatInformation.SystemStateValues.HeatModeInactive:
                        iCurrentMode = 0;
                        break;
                    case clsThermostatInformation.SystemStateValues.Uninitialized:
                    default:
                        iCurrentMode = -1;
                        break;
                }
                return iCurrentMode;
            }
            else
                return -1;
        }
        //True if the thermostat is currently calling for Heat or Cool   @@@@@
        public bool GetOperating(int HSThermostat, int Sensor)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsHSPI::GetOperating");
            return true;
        }

        #endregion

        #region " Thermostat Set Properties or Methods "
        // Set the heat setpoint temperature.
        public int CmdSetHeat(int HSThermostat, double Temperature, int Sensor)
        {
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);
            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))
            {

                int iUnitNumber = m_objApp.Thermostats[Thermostat].Unit;
                int iZoneNumber = m_objApp.Thermostats[Thermostat].Zone;

                m_objApp.CreateSetPointMessage(iUnitNumber, iZoneNumber, Convert.ToInt16(Temperature), 0);

            }
            else
            {
                clsEnviracomApp.TraceEvent(TraceEventType.Warning, "clsHSPI::CmdSetHeat called with invalid thermostat number: " + Thermostat + " Homeseer thermostat number: " + HSThermostat);
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::CmdSetHeat");
            return 0;
        }
        // Set the cool setpoint temperature.
        public int CmdSetCool(int HSThermostat, double Temperature, int Sensor)
        {
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);
            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))
            {

                int iUnitNumber = m_objApp.Thermostats[Thermostat].Unit;
                int iZoneNumber = m_objApp.Thermostats[Thermostat].Zone;

                m_objApp.CreateSetPointMessage(iUnitNumber, iZoneNumber, 0, Convert.ToInt16(Temperature));
            
            }
            else
            {
                clsEnviracomApp.TraceEvent(TraceEventType.Warning, "clsHSPI::CmdSetCool called with invalid thermostat number: " + Thermostat + " Homeseer thermostat number: " + HSThermostat);
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::CmdSetCool");
            return 0;
        }
        // Set the thermostat operating mode.
        //For the thermostat Mode:
        //0=Off, 1=Heat, 2=Cool, 3=Auto, 4=Aux (If Supported, a.k.a. Emergency Heat)
        public int CmdSetMode(int HSThermostat, int Mode, int Sensor)
        {
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);
            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))
            {
                int iUnitNumber = m_objApp.Thermostats[Thermostat].Unit;
                int iZoneNumber = m_objApp.Thermostats[Thermostat].Zone;

                clsThermostatInformation.SystemSwitchValues eModeStatus = clsThermostatInformation.SystemSwitchValues.Uninitialized;
                
                //Translate Homeseer Modes to Enviracom Modes
                switch (Mode)
                {
                    case 0:
                        eModeStatus = clsThermostatInformation.SystemSwitchValues.Off;
                        break;
                    case 1:
                        eModeStatus = clsThermostatInformation.SystemSwitchValues.Heat;
                        break;
                    case 2:
                        eModeStatus = clsThermostatInformation.SystemSwitchValues.Cool;
                        break;
                    case 3:
                        eModeStatus = clsThermostatInformation.SystemSwitchValues.Auto;
                        break;
                    case 4:
                        eModeStatus = clsThermostatInformation.SystemSwitchValues.EmergencyHeat;
                        break;
                }

                m_objApp.CreateModeMessage(iUnitNumber, iZoneNumber, eModeStatus);
            }
            else
            {
                clsEnviracomApp.TraceEvent(TraceEventType.Warning, "clsHSPI::CmdSetMode called with invalid thermostat number: " + Thermostat + " Homeseer thermostat number: " + HSThermostat);
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::CmdSetMode");
            return 0;
        }
        // Set the thermostat fan operating mode.
        //For the thermostat fan mode:
        //0=Auto, 1=On
        public int CmdSetFan(int HSThermostat, int Mode, int Sensor)
        {
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);
            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))
            {
                int iUnitNumber = m_objApp.Thermostats[Thermostat].Unit;
                int iZoneNumber = m_objApp.Thermostats[Thermostat].Zone;

                clsThermostatInformation.FanSwitchValues eFanSwitchStatus = clsThermostatInformation.FanSwitchValues.Uninitialized;

                //Translate Homeseer Modes to Enviracom Modes
                //Because we don't know if Auto Circulate is on or not and Homeseer doesn't support it anyway
                //we are getting it's state and setting it to whatever it was
                //while modifying the fan state when possible

                
                //Change Mode To "AUTO" when it is currently in "ON"
                if ((Mode == 0) && ((m_objApp.Thermostats[Thermostat].FanSwitch == clsThermostatInformation.FanSwitchValues.OnCirculate) || (m_objApp.Thermostats[Thermostat].FanSwitch == clsThermostatInformation.FanSwitchValues.OnNormal)))
                {
                    //Change OnCirculate to AutoCirculate or OnNormal to AutoNormal
                    eFanSwitchStatus = (m_objApp.Thermostats[Thermostat].FanSwitch == clsThermostatInformation.FanSwitchValues.OnCirculate ? clsThermostatInformation.FanSwitchValues.AutoCirculate : clsThermostatInformation.FanSwitchValues.AutoNormal);
                }

                //Change Mode To "ON" when it is currently in "AUTO"
                if ((Mode == 1) && ((m_objApp.Thermostats[Thermostat].FanSwitch == clsThermostatInformation.FanSwitchValues.AutoCirculate) || (m_objApp.Thermostats[Thermostat].FanSwitch == clsThermostatInformation.FanSwitchValues.AutoNormal)))
                {
                    //Change AutoCirculate to OnCirculate or AutoNormal to OnNormal
                    eFanSwitchStatus = (m_objApp.Thermostats[Thermostat].FanSwitch == clsThermostatInformation.FanSwitchValues.AutoCirculate ? clsThermostatInformation.FanSwitchValues.OnCirculate : clsThermostatInformation.FanSwitchValues.OnNormal);
                }

                if (eFanSwitchStatus != clsThermostatInformation.FanSwitchValues.Uninitialized)
                {
                    m_objApp.CreateFanSwitchMessage(iUnitNumber, iZoneNumber, eFanSwitchStatus);
                }
            }
            else
            {
                clsEnviracomApp.TraceEvent(TraceEventType.Warning, "clsHSPI::CmdSetFan called with invalid thermostat number: " + Thermostat + " Homeseer thermostat number: " + HSThermostat);
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::CmdSetFan");
            return 0;
        }
        // Set the thermostat hold mode. (If supported)
        //For the thermostat hold mode:
        //0=Off Hold, 1=Hold (if Supported)
        public int CmdSetHold(int HSThermostat, int Mode, int Sensor)
        {
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);
            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))
            {
                int iUnitNumber = m_objApp.Thermostats[Thermostat].Unit;
                int iZoneNumber = m_objApp.Thermostats[Thermostat].Zone;
    
                clsThermostatInformation.HoldStatusValues eHoldStatus = (Mode == 0 ? clsThermostatInformation.HoldStatusValues.Program : clsThermostatInformation.HoldStatusValues.Hold);

                m_objApp.CreateHoldMessage(iUnitNumber, iZoneNumber, eHoldStatus);
            }
            else
            {
                clsEnviracomApp.TraceEvent(TraceEventType.Warning, "clsHSPI::CmdSetHold called with invalid thermostat number: " + Thermostat + " Homeseer thermostat number: " + HSThermostat);
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::CmdSetHold");
            return 0;
        }

        #endregion


        #region " Not Homeseer Defined Thermostat Interfaces "

        #region " Get Interfacs "

        //Current Outdoor Temperature -- Courtesy of Matt Brandes

        public double GetOutdoorTemp(int HSThermostat, int Sensor)
        {

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::GetOutdoorTemp");
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);

            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))

                return m_objApp.Thermostats[Thermostat].OutdoorTemp;

            else

                return 0;

        }

        //Current Humidity -- Courtesy of Matt Brandes

        public double GetHumidity(int HSThermostat, int Sensor)
        {

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::GetHumidity");
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);

            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))

                return m_objApp.Thermostats[Thermostat].CurrentHumidity;

            else

                return 0;

        }

        //Get the homeseer thermostat number given the device name

        public int GetThermostatByName(string name)
        {
            return m_objApp.GetThermostatHomeseerIndex(name);
        }

        //Get the homeseer thermostat number given the house code and device code

        public int GetThermostatByCode(string dc, string hc)
        {
            return m_objApp.GetThermostatHomeseerIndex(dc,hc);
        }

        #endregion

        #region " CmdSet Interfacs "

        // Set the fan Recirculate mode.
        public int CmdSetRecirc(int HSThermostat, bool bReCirculateOn, int Sensor)
        {
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);
            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))
            {
                int iUnitNumber = m_objApp.Thermostats[Thermostat].Unit;
                int iZoneNumber = m_objApp.Thermostats[Thermostat].Zone;

                //clsThermostatZone.FanSwitchValues eFanSwitchCurrentStatus = m_objApp.Thermostats[Thermostat].FanSwitch;
                clsThermostatInformation.FanSwitchValues eFanSwitchStatus = clsThermostatInformation.FanSwitchValues.Uninitialized;

                //Turn off Recirc -- it is already on
                //Leave Fan Mode Alone
                if ((bReCirculateOn == false) && ((m_objApp.Thermostats[Thermostat].FanSwitch == clsThermostatInformation.FanSwitchValues.AutoCirculate) || (m_objApp.Thermostats[Thermostat].FanSwitch == clsThermostatInformation.FanSwitchValues.OnCirculate)))
                {
                    //Change OnCirculate to OnNormal or AutoCirculate to AutoNormal
                    clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::CmdSetRecirc Turning Off Recirculate");
                    eFanSwitchStatus = (m_objApp.Thermostats[Thermostat].FanSwitch == clsThermostatInformation.FanSwitchValues.AutoCirculate ? clsThermostatInformation.FanSwitchValues.AutoNormal : clsThermostatInformation.FanSwitchValues.OnNormal);
                }

                //Turn on Recirc -- it is currently off
                //Leave Fan Mode Alone
                if ((bReCirculateOn == true) && ((m_objApp.Thermostats[Thermostat].FanSwitch == clsThermostatInformation.FanSwitchValues.AutoNormal) || (m_objApp.Thermostats[Thermostat].FanSwitch == clsThermostatInformation.FanSwitchValues.OnNormal)))
                {
                    //Change OnNormal to OnCirculate or AutoNormal to AutoCirculate
                    clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::CmdSetRecirc Turning On Recirculate");
                    eFanSwitchStatus = (m_objApp.Thermostats[Thermostat].FanSwitch == clsThermostatInformation.FanSwitchValues.AutoNormal ? clsThermostatInformation.FanSwitchValues.AutoCirculate : clsThermostatInformation.FanSwitchValues.OnCirculate);
                }

                clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::CmdSetRecirc eFanSwitchStatus value is " + eFanSwitchStatus.ToString());

                if (eFanSwitchStatus != clsThermostatInformation.FanSwitchValues.Uninitialized)
                {
                    m_objApp.CreateFanSwitchMessage(iUnitNumber, iZoneNumber, eFanSwitchStatus);
                }
            }
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::CmdSetRecirc");
            return 0;
        }

        // Set the clock to current time.
        public int CmdSetClock()
        {
            m_objApp.CreateSetClockMessage();
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::CmdSetClock");
            return 0;
        }

        // Tell each thermostat to send its schedule -- TODO replace this with a proper get and set api for scheduling
        public int CmdQuerySchedules(int HSThermostat)
        {
            int Thermostat = m_objApp.GetThermostatInternalIndex(HSThermostat);
            if ((Thermostat >= 0) && (Thermostat < m_objApp.NumberOfThermostats))
            {
                int iUnitNumber = m_objApp.Thermostats[Thermostat].Unit;
                int iZoneNumber = m_objApp.Thermostats[Thermostat].Zone;

                m_objApp.CreateQuerySchedulesMessage(iUnitNumber, iZoneNumber);

                clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::CmdQuerySchedules for Homeseer Thermostat: " + HSThermostat + " Unit: " + iUnitNumber + " Zone: " + iZoneNumber);
            }
            else
            {
                m_objApp.CreateQuerySchedulesMessage();

                clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHSPI::CmdQuerySchedules for all thermostats");
            }
            return 0;
        }
        #endregion

        #endregion

        #region " Not Homeseer Defined Web Support Interfaces "

        public clsWebManager WebMgr()
        {
            clsWebManager objWebMgr = new clsWebManager();

            return objWebMgr;
        }

        #endregion

        #endregion
        #endregion
    }

}
