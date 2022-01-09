using System;
using System.Diagnostics;
using System.Collections;
using System.Text;
using Scheduler;
using Scheduler.Classes;

namespace HSPI_ENVIRACOM_MANAGER
{
	/// <summary>
	/// This object represents a Zone.  Initially all states are set to 
	/// the clean states, and the unknown State will be set
	/// </summary>
	public class clsThermostatZone
	{
        #region "  Enumerations and Constants  "
        private enum Status
        {
            UNKNOWN = 0,
            CONNECTED = 1,
            EQUIPMENT_FAULT = 2
        };

        public enum SystemSwitchValues
        {
            Uninitialized = -1,
            EmergencyHeat = 0,
            Heat = 1,
            Off = 2,
            Cool = 3,
            Auto = 4,
            Unknown_1 = 5,
            Unknown_2 = 6,
            Unknown_3 = 7
        };

        public enum FanSwitchValues
        {
            Uninitialized = -1,
            AutoNormal = 0,
            AutoCirculate = 1,
            OnNormal = 2,
            OnCirculate = 3
        };

        public enum CirculateFanValues
        {
            Uninitialized = -1,
            Off = 0,
            On = 1
        };

        public enum BlowerFanValues
        {
            Uninitialized = -1,
            Off = 0,
            Slow = 1,
            Fast = 2
        };

        public enum DamperStatusValues
        {
            Uninitialized = -1,
            Closed = 0,
            Open = 1
        };

        public enum AuxHeatStatusValues
        {
            Uninitialized = -1,
            Off = 0,
            StageOne = 1,
            StageTwo = 2
        };

        public enum AutoModeValues
        {
            Uninitialized = -1,
            Heat = 0,
            Cool = 1
        };

        public enum SystemStateValues
        {
            Uninitialized = -1,
            HeatModeInactive = 1,
            HeatModeActive = 2,
            CoolModeInactive = 3,
            CoolModeActive = 4
        };

        public enum StageStateValues
        {
            Uninitialized = -1,
            ConventionalInactive = 1,
            ConventionalActive = 2,
            HeatPumpInactive = 3,
            HeatPumpActive = 4,
            FossilFuelInactive = 5,
            FossilFuelActive = 6,
            ElectricInactive = 7,
            ElectricActive = 8
        };

        public enum HoldStatusValues
        {
            Uninitialized = -1,
            Program = 0,
            Temporary = 1,
            Hold = 2
        };

        public enum StateActivityValues
        {
            Uninitialized = -1,
            Off = 0,
            Heat = 1,
            Cool = 2,
            Fan = 3
        };

        private const string DT_HVAC_ZONE = "Enviracom Thermostat";
        private const string ZONE_LOCATION = "HVAC";
        private const char IOMISC_SEPERATOR = '^';
        private const int IOMISC_ZONE_INDEX = 0;
        private const int IOMISC_UNIT_INDEX = 1;
        #endregion

        #region "  Structures  "
        #endregion

        #region "  Members  "
		private int			        m_iZoneNumber = 0;
        private int                 m_iUnitNumber = 0;
        private int                 m_iThermostatNumber = 0;
        private int                 m_iRemainingFilterTime = 0;

        private bool                m_bDeviceAttached = false;

        private float               m_fOutdoorTemperature = 0.0F;
        private float               m_fCurrentTemperature = 0.0F;
        private float               m_fCurrentHumidity = 0.0F;
        private float               m_fCurHeatSetpoint = 0.0F;
        private float               m_fCurCoolSetpoint = 0.0F;
        private float               m_fMaxHeatSetpoint = 0.0F;
        private float               m_fMaxCoolSetpoint = 0.0F;
        private float               m_fMinHeatSetpoint = 0.0F;
        private float               m_fMinCoolSetpoint = 0.0F;
        private float               m_fCurHumiditySetpoint = 0.0F;
        private float               m_fMaxHumiditySetpoint = 0.0F;
        private float               m_fMinHumiditySetpoint = 0.0F;
        private float               m_fCurDehumiditySetpoint = 0.0F;
        private float               m_fMaxDehumiditySetpoint = 0.0F;
        private float               m_fMinDehumiditySetpoint = 0.0F;

        private AuxHeatStatusValues m_eAuxHeatStatus = AuxHeatStatusValues.Uninitialized;
        private BlowerFanValues     m_eBlowerFanStatus = BlowerFanValues.Uninitialized;
        private CirculateFanValues  m_eCirculateFanStatus = CirculateFanValues.Uninitialized;
        private DamperStatusValues m_eDamperStatus = DamperStatusValues.Uninitialized;
        private FanSwitchValues     m_eFanSwitch = FanSwitchValues.Uninitialized;
        private StateActivityValues m_eSystemStateActivity = StateActivityValues.Uninitialized;
        private HoldStatusValues    m_eCurrentStatus = HoldStatusValues.Uninitialized;
        private SystemSwitchValues  m_eSystemSwitch = SystemSwitchValues.Uninitialized;
        private AutoModeValues      m_eAutoCurrentMode = AutoModeValues.Uninitialized;
        private SystemStateValues   m_eSystemState = SystemStateValues.Uninitialized;
        private StageStateValues[]  m_eStateStates = new StageStateValues[4];

        private clsScheduleDay[]    m_objScheduleDays = new clsScheduleDay[7];
        private clsEnviracomApp     m_objApp = clsEnviracomApp.GetInstance();
        private DeviceClass         m_objAttachedDevice;
        #endregion

        #region "  Accessor Methods for Members  "

        public int ThermostatNumber
		{
			get
			{
				return m_iThermostatNumber;
			}
			set
			{
				m_iThermostatNumber = value;
			}
		}
        public int Zone
		{
			get
			{
				return m_iZoneNumber;
			}
			set
			{
				m_iZoneNumber = value;
			}
		}
        public int Unit
        {
            get
            {
                return m_iUnitNumber;
            }
            set
            {
                m_iUnitNumber = value;
            }
        }
        public int RemainingFilterTime
        {
            get
            {
                return m_iRemainingFilterTime;
            }
            set
            {
                m_iRemainingFilterTime = value;
            }
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
        public DeviceClass Device
        {
            get
            {
                return m_objAttachedDevice;
            }
            set
            {
                m_objAttachedDevice = value;
            }
        }
        public bool DeviceAttached
        {
            get
            {
                return m_bDeviceAttached;
            }
        }
        public HoldStatusValues HoldStatus
		{
			get
			{
				return m_eCurrentStatus;
			}
			set
			{
				m_eCurrentStatus = value;
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

        public float OutdoorTemp
        {
            get
            {
                return m_fOutdoorTemperature;
            }
            set
            {
                m_fOutdoorTemperature = value;
            }
        }
        public float CurrentTemp
        {
            get
            {
                return m_fCurrentTemperature;
            }
            set
            {
                m_fCurrentTemperature = value;
                SetZoneDeviceStatus(value.ToString("00"));
            }
        }
        public float HeatSetPoint
		{
			get
			{
				return m_fCurHeatSetpoint;
            }
			set
			{
                m_fCurHeatSetpoint = value;
			}
		}
        public float CoolSetPoint
		{
			get
			{
				return m_fCurCoolSetpoint;
            }
			set
			{
                m_fCurCoolSetpoint = value;
			}
		}
        public float CurrentHumidity
        {
            get
            {
                return m_fCurrentHumidity;
            }
            set
            {
                m_fCurrentHumidity = value;
            }
        }
        public float HumiditySetpoint
        {
            get
            {
                return m_fCurHumiditySetpoint;
            }
            set
            {
                m_fCurHumiditySetpoint = value;
            }
        }
        public float DehumiditySetpoint
        {
            get
            {
                return m_fCurDehumiditySetpoint;
            }
            set
            {
                m_fCurDehumiditySetpoint = value;
            }
        }


        public float MaxHeatSetpoint
        {
            get
            {
                return m_fMaxHeatSetpoint;
            }
            set
            {
                m_fMaxHeatSetpoint = value;
            }
        }
        public float MaxCoolSetpoint
        {
            get
            {
                return m_fMaxCoolSetpoint;
            }
            set
            {
                m_fMaxCoolSetpoint = value;
            }
        }
        public float MinHeatSetpoint
        {
            get
            {
                return m_fMinHeatSetpoint;
            }
            set
            {
                m_fMinHeatSetpoint = value;
            }
        }
        public float MinCoolSetpoint
        {
            get
            {
                return m_fMinCoolSetpoint;
            }
            set
            {
                m_fMinCoolSetpoint = value;
            }
        }
        public float MaxHumiditySetpoint
        {
            get
            {
                return m_fMaxHumiditySetpoint;
            }
            set
            {
                m_fMaxHumiditySetpoint = value;
            }
        }
        public float MinHumiditySetpoint
        {
            get
            {
                return m_fMinHumiditySetpoint;
            }
            set
            {
                m_fMinHumiditySetpoint = value;
            }
        }
        public float MaxDehumiditySetpoint
        {
            get
            {
                return m_fMaxDehumiditySetpoint;
            }
            set
            {
                m_fMaxDehumiditySetpoint = value;
            }
        }
        public float MinDehumiditySetpoint
        {
            get
            {
                return m_fMinDehumiditySetpoint;
            }
            set
            {
                m_fMinDehumiditySetpoint = value;
            }
        }

        
        
        public SystemSwitchValues SystemSwitch
        {
            get
            {
                return m_eSystemSwitch;
            }
            set
            {
                m_eSystemSwitch = value;
            }
        }
        public FanSwitchValues FanSwitch
        {
            get
            {
                return m_eFanSwitch;
            }
            set
            {
                m_eFanSwitch = value;
            }
        }
        public CirculateFanValues CirculateFanStatus
        {
            get
            {
                return m_eCirculateFanStatus;
            }
            set
            {
                m_eCirculateFanStatus = value;
            }
        }
        public DamperStatusValues DamperStatus
        {
            get
            {
                return m_eDamperStatus;
            }
            set
            {
                m_eDamperStatus = value;
            }
        }

        public BlowerFanValues BlowerFanStatus
        {
            get
            {
                return m_eBlowerFanStatus;
            }
            set
            {
                m_eBlowerFanStatus = value;
            }
        }
        public AuxHeatStatusValues AuxHeatStatus
        {
            get
            {
                return m_eAuxHeatStatus;
            }
            set
            {
                m_eAuxHeatStatus = value;
            }
        }
        public StateActivityValues SystemStateActivity
        {
            get
            {
                return m_eSystemStateActivity;
            }
            set
            {
                m_eSystemStateActivity = value;
            }
        }
        public AutoModeValues AutoCurrentMode
        {
            get
            {
                return m_eAutoCurrentMode;
            }
            set
            {
                m_eAutoCurrentMode = value;
            }
        }
        public SystemStateValues SystemState
        {
            get
            {
                return m_eSystemState;
            }
            set
            {
                m_eSystemState = value;
            }
        }
        public StageStateValues StageOneState
        {
            get
            {
                return m_eStateStates[0];
            }
            set
            {
                m_eStateStates[0] = value;
            }
        }
        public StageStateValues StageTwoState
        {
            get
            {
                return m_eStateStates[1];
            }
            set
            {
                m_eStateStates[1] = value;
            }
        }
        public StageStateValues StageThreeState
        {
            get
            {
                return m_eStateStates[2];
            }
            set
            {
                m_eStateStates[2] = value;
            }
        }
        public StageStateValues StageFourState
        {
            get
            {
                return m_eStateStates[3];
            }
            set
            {
                m_eStateStates[3] = value;
            }
        }
        #endregion

        #region "  Constructors and Destructors  "

        //	Constructor
		public clsThermostatZone()
		{
            for (int i = 0; i < 7; i++)
                m_objScheduleDays[i] = new clsScheduleDay();
            for (int i = 0; i < 3; i++)
                m_eStateStates[i] = StageStateValues.Uninitialized;

        }
        #endregion

        #region "  Initialization and Cleanup  "
        #endregion

        #region "  Public Methods  "
        public bool AttachHomeseerZoneDevice()
        {
            DeviceClass dev = FindHomeseerZoneDevice();
            if (dev != null)
            {
                AttachDevice(dev);
                return true;
            }
            else
            {
                if (!CreateHomeseerZoneDevice())
                    throw new System.Exception("Failed to attach homeseer device to Enviracom Zone " + m_iZoneNumber.ToString() + " on Unit " + m_iUnitNumber.ToString());
                return false;
            }
        }

        public void AttachDevice(DeviceClass dev)
        {
            m_objAttachedDevice = dev;
            m_objApp.SetUsedDevCode(Convert.ToInt32(m_objAttachedDevice.dc));
            m_bDeviceAttached = true;
            m_objApp.ifHSApp.SetDeviceValue(
                m_objAttachedDevice.hc + m_objAttachedDevice.dc,
                (int)Status.CONNECTED);
        }

        public void DetachDevice()
        {
            m_objApp.ifHSApp.SetDeviceValue(
                m_objAttachedDevice.hc + m_objAttachedDevice.dc,
                (int)Status.UNKNOWN);
            m_bDeviceAttached = false;
            m_objApp.FreeDevCode(Convert.ToInt32(m_objAttachedDevice.dc));
            m_objAttachedDevice = null;
        }


        public void SetSchedule(int iScheduleDay, int iSchedulePeriod, int iScheduleTime, float fHeatSetPoint, float fCoolSetPoint, int iFanMode)
        {
            if (iScheduleDay == 0)
            {
                foreach (clsScheduleDay sd in m_objScheduleDays)
                {
                    sd.SetSchedule(iSchedulePeriod, iScheduleTime, fHeatSetPoint, fCoolSetPoint, iFanMode);
                }
            }
            else if ((iScheduleDay > 0) && (iScheduleDay <= 7))
            {
                m_objScheduleDays[iScheduleDay - 1].SetSchedule(iSchedulePeriod, iScheduleTime, fHeatSetPoint, fCoolSetPoint, iFanMode);
            }
            else
            {
                clsEnviracomApp.TraceEvent(TraceEventType.Error, "clsZoneManager::SetSchedule: schedule day out of range: '" + iScheduleDay.ToString() + "'");
            }

        }

        #region "  public Web UI Methods  "
        // Builds this stat's portion of the Configuration HTML page.
        // This page allows the user to specify a status deviec from a drop down list of all defined HS devices, to select the items that are displayed for 
        // this stat, to select the controls to display for this stat, and to enable or disable this stat

        public string BuildConfigPage()
        {
            StringBuilder p = new StringBuilder();
            string classname = ((m_iThermostatNumber % 2) == 0 ? "tableroweven" : "tablerowodd");

            p.Append("<tr>\n");
            p.Append("<td noWrap class=\"" + classname + "\" align=center valign=middle>" + clsEnviracomApp.GetThermostatHomeseerIndex(m_iThermostatNumber) + "</td>\n");
            p.Append("<td noWrap class=\"" + classname + "\" align=left valign=middle width='30%%'>\n");	  
            p.Append("<table border='0' width='100%' cellspacing='0' cellpadding='0'><tr>");
            p.Append("<td width='15%' align=right>" + GetStringHTML(Name) + "</td>\n<td width='85%'>");
            p.Append("</td></tr></table></td>");

// Build the table of display and control options
	
            p.Append("<td class=\"" + classname + "\" align=center>\n");
            p.Append("<TABLE cellSpacing=1 cellPadding=1 border=0>\n");
//	str += m_StatDevice->BuildDisplayOptions(m_ThermoNumber, classname);
            p.Append("</table>\n");	
            p.Append("</td>\n");
            p.Append("<td class=\"" + classname + "\" align=center>\n");
            p.Append("<TABLE cellSpacing=1 cellPadding=1 border=0>\n");
//	str += m_StatDevice->BuildControlOptions(m_ThermoNumber, classname);
            p.Append("</table>\n");	
            p.Append("</td>\n");
//	m_StatDevice->get_Enable(&fl);
            p.Append("<td class=\"" + classname + "\" align=center>\n");

// Finally add the enable checkbox

            p.Append("<INPUT id=\"Enabled" + m_iThermostatNumber + "\" class=\"formcheckbox\" type=\"checkbox\" s=\"Enabled" + m_iThermostatNumber + "\"" + " CHECKED" + ">");
            p.Append("</td>\n</tr>\n");

            return p.ToString();
        }
	

// Builds this stat's portion of the main HTML page. A table with controls and outputs for each item the stat controls like mode, csp, hsp, etc.

        public string BuildThermoString()
        {
 			StringBuilder	p = new StringBuilder();

            p.Append(GetNumberHTML());
            p.Append(GetDeviceCodeHTML());
            p.Append(GetDeviceNameHTML());
            p.Append(GetSystemSwitchHTML());
            p.Append(GetCirculateFanHTML());
            p.Append(GetBlowerFanHTML());
            p.Append(GetActiveModeHTML());
            p.Append(GetCurrentStatusHTML());
            p.Append(GetSystemStateHTML());
            p.Append(GetStageOneStateHTML());
            p.Append(GetStageTwoStateHTML());
            p.Append(GetStageThreeStateHTML());
            p.Append(GetStageFourStateHTML()); 
            p.Append(GetCurrentTempHTML());
            p.Append(GetCSPHTML());
            p.Append(GetHSPHTML());
            p.Append(GetCurrentHumidityHTML());
            p.Append(GetHumidifySPHTML());
            p.Append(GetDehumidifySPHTML());


            p.Append(GetFilterDaysHTML());
            p.Append("</tr>\n");
#if false	
	str += GetDeviceNameHTML((admin?2:1));
	if (opts[0]) str += GetModeHTML();
	if (opts[1]) str += GetFanStatHTML();
	if (opts[2]) str += GetCurrentStatHTML();
	if (opts[3]) str += GetCurrentWeekHTML();
	if (opts[4]) str += GetLastWeekHTML();
	if (opts[5]) str += GetHoldHTML();
	if (opts[6]) str += GetCurrentTempHTML();
	if (opts[7]) str += GetCSPHTML();
	if (opts[8]) str += GetHSPHTML();
	if (opts[9]) str += GetFilterDaysHTML();
	if (!admin) {
	  str += _T("</tr>\n");
	  return str;
	}
	str += _T("</tr>\n<tr>\n");

	if (opts[0]) str += GetModeControlHTML();
	if (opts[1]) str += GetFanControlHTML();
	if (opts[2])  {
	  str += _T("<td align=\"center\" class=\"");str+=_T("tablerowodd");str+= _T("\">&nbsp;</td>\n");
	}
	if (opts[3]) {
	    str += _T("<td align=\"center\" class=\"");str+=_T("tablerowodd");str+= _T("\">&nbsp;</td>\n");
	}
	if (opts[4]) {
	  str += _T("<td align=\"center\" class=\"");str+=_T("tablerowodd");str+= _T("\">&nbsp;</td>\n");
	}
	if (opts[5]) str += GetHoldControlHTML();
	if (opts[6]) {
	    str += _T("<td align=\"center\" class=\"");str+=_T("tablerowodd");str+= _T("\">&nbsp;</td>\n");
	}
	if (opts[7]) {
	  if (m_IsF) str += GetCSPControlHTML(tempStrF);
	  else str += GetCSPControlHTML(tempStrC);
	}
	if (opts[8]) {
	  if (m_IsF) str += GetHSPControlHTML(tempStrF);
	  else str += GetHSPControlHTML(tempStrC);
	}
	if (opts[9]) {
	  str += GetFilterDaysControlHTML();
//	  str += _T("<td align=\"center\" class=\"");str+=_T("tablerowodd");str+= _T("\">&nbsp;</td>\n");
	}

	str += _T("</tr>\n");

#endif
	        return p.ToString();
        }

        // Builds the HTML page that display's this stat's schedule

        public string BuildSchedulePage()
        {
            StringBuilder p = new StringBuilder();
            string classname = ((m_iThermostatNumber % 2) == 0 ? "tableroweven" : "tablerowodd");

            // Display the stat's s and have it span 2 rows
	
            p.Append("<tr>\n");
            p.Append(GetDeviceNameHTML());

            // Create a table for each schedule

            int rownum = 1;
            foreach (clsScheduleDay sd in m_objScheduleDays)
            {
                p.Append("<td class=\"" + classname + "\" align=center>\n");
                p.Append(sd.GenerateHTMLTable(m_iThermostatNumber, rownum));
                p.Append("</td>\n");
                rownum++;
            }
            return p.ToString();
        }
	


        
        #endregion


        #endregion

        #region "  Private Methods    "
        private int GetZoneUnit(string sIoMisc)
        {
            return Convert.ToInt32(sIoMisc.Split(IOMISC_SEPERATOR)[IOMISC_UNIT_INDEX]);
        }

        private int GetZoneNumber(string sIoMisc)
        {
            return Convert.ToInt32(sIoMisc.Split(IOMISC_SEPERATOR)[IOMISC_ZONE_INDEX]);
        }

        private void SetZoneDeviceStatus(string sStatus)
        {
            //	Check if we have a homeseer device attached to us
            if (m_bDeviceAttached == true)
            {
                //	Make sure the device still exists in HS (can't figure how to 
                //	get notified if the device gets deleted, so we'll just check on status
                //	changes
                int nRef = m_objApp.ifHSApp.GetDeviceRef(HouseCode + DevCode);
                if (nRef == -1)
                {
                    //	It's been deleted...
                    m_objApp.FreeDevCode(Convert.ToInt32(DevCode));
                    DetachDevice();
                    clsEnviracomApp.LogInformation(
                        "clsThermostatZone::SetZoneStatus: Detected zone (" +
                        m_iZoneNumber.ToString() + ":" +
                        Name +
                        ") was deleted in HomeSeer");
                }
            }

            clsEnviracomApp.LogInformation(
                "clsThermostatZone::SetZoneDeviceStatus: Setting zone (" +
                m_iZoneNumber.ToString() + ":" +
                Name +
                ") status to " + sStatus);

            //	Make sure it is in HomeSeer before notifying it
            if (m_bDeviceAttached == true)
            {
                m_objApp.ifHSApp.SetDeviceString(
                    HouseCode + DevCode,
                    sStatus,
                    true);
                m_objApp.ifHSApp.SetDeviceValue(
                    HouseCode + DevCode,
                    (int)Status.CONNECTED);
            }
        }

        private bool CreateHomeseerZoneDevice()
        {
            DeviceClass dev;

            //  Create the device
            dev = m_objApp.ifHSApp.NewDeviceEx("Unit " + m_iUnitNumber.ToString() + " Thermostat " + m_iZoneNumber.ToString());
            SetHomeseerZoneDeviceAttributes(dev);
            //	Finally, tell the zone what it's dev code is
            AttachDevice(dev);

            clsEnviracomApp.LogInformation(
                "clsZoneManager::GetZone: Created zone (" +
                m_iZoneNumber.ToString() + ":" +
                Name +
                ") at device code " +
                dev.dc);

            return true;
        }

        private void DeleteHomeseerZoneDevice()
        {

            //	remove the zone from HS and the dev code from EnviracomApp
            if (m_bDeviceAttached == true)
            {
                int nRef = m_objApp.ifHSApp.GetDeviceRef(HouseCode + DevCode);
                clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsZoneManager::DeleteHomeseerZoneDevice: Deleting device (" +
                    DevCode + ":" +
                    Name + ")");
                DetachDevice();
                m_objApp.ifHSApp.DeleteDevice(nRef);
            }
        }

        private void SetHomeseerZoneDeviceAttributes(DeviceClass dev)
        {
            //  set all the relevant information
            dev.location = ZONE_LOCATION;
            dev.hc = m_objApp.HouseCode;
            dev.dc = Convert.ToString(m_objApp.GetNextFreeDevCode());
            dev.@interface = clsEnviracomApp.PLUGIN_IN_NAME_SHORT;
            dev.misc = HomeSeer.MISC_THERM;
            dev.dev_type_string = DT_HVAC_ZONE;
            dev.iotype = HomeSeer.IOTYPE_CONTROL;
            dev.iomisc = Convert.ToString(m_iZoneNumber) + IOMISC_SEPERATOR + Convert.ToString(m_iUnitNumber);
            dev.values =
                "Uninitialized" + Convert.ToChar(2) + Convert.ToString((int)Status.UNKNOWN) + Convert.ToChar(1) +
                "Connected" + Convert.ToChar(2) + Convert.ToString((int)Status.CONNECTED) + Convert.ToChar(1) +
                "Fault" + Convert.ToChar(2) + Convert.ToString((int)Status.EQUIPMENT_FAULT);
        }

        private DeviceClass FindHomeseerZoneDevice()
        {
            clsDeviceEnumeration de;
            DeviceClass dev;
            string sHouseCode = m_objApp.HouseCode;

            //  Enumerate through all the devices looking for one of mine
            de = (clsDeviceEnumeration)m_objApp.ifHSApp.GetDeviceEnumerator();
            while (!de.Finished)
            {
                dev = (DeviceClass)de.GetNext();
                if ((dev.dev_type_string == DT_HVAC_ZONE) && (GetZoneUnit(dev.iomisc) == m_iUnitNumber) && (GetZoneNumber(dev.iomisc) == m_iZoneNumber))
                {
                    return dev;
                }
            }
            return null;
        }
        #region " Private Web UI Methods "
        // The routines below are used in constructing the thermostat's web page setup interface
        private string GetStringHTML(string s)
        {
            return s.Replace(" ", "&nbsp;");
        }

        // Function that formats the thermostat number as a table cell

        private string GetNumberHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + clsEnviracomApp.GetThermostatHomeseerIndex(m_iThermostatNumber) + "</td>\n";
        }

        // Function that formats the stat device as a table cell

        private string GetDeviceCodeHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + HouseCode + DevCode + "</td>\n";
        }

        // Function that formats the stat name as a table cell

        private string GetDeviceNameHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + Location + "&nbsp;" + Name + "</td>\n";
        }

        // Return the stat mode in HTML

        private string GetSystemSwitchHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + SystemSwitch.ToString() + "</td>\n";
        }

        // Return the circulate fan status in HTML

        private string GetCirculateFanHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + CirculateFanStatus.ToString() + "</td>\n";
        }

        // Return the blower fan status in HTML

        private string GetBlowerFanHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + BlowerFanStatus.ToString() + "</td>\n";
        }

        // Return the fan status in HTML

        private string GetActiveModeHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + SystemStateActivity.ToString() + "</td>\n";
        }
        
        // Return the current operating status in HTML

        private string GetCurrentStatusHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + HoldStatus.ToString() + "</td>\n";
        }
        private string GetSystemStateHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + SystemState.ToString() + "</td>\n";
        }
        private string GetStageOneStateHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + StageOneState + "</td>\n";
        }
        private string GetStageTwoStateHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + StageTwoState + "</td>\n";
        }
        private string GetStageThreeStateHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + StageThreeState + "</td>\n";
        }
        private string GetStageFourStateHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + StageFourState + "</td>\n";
        }

        // Return the Days until Filter Change in HTML

        private string GetFilterDaysHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + RemainingFilterTime + "</td>\n";
        }
        
        // Return a temperature limit in HTML
#if false
        private string GetTempLimitHTML(CString tempStr, CString pref, double d)
{
	CString	str;
	CString	str1;
	int		temp;

	str = _T("");
	str1.Format(_T("<SELECT id=\"%s%d\" name=\"%s%d\">\n"), LPCTSTR(pref), m_ThermoNumber, LPCTSTR(pref), m_ThermoNumber);
	str += str1;
	temp = (int) (d + 0.5);
	str += SelectTemperatureHTML(tempStr, temp, m_IsF);
	str += _T("</SELECT>\n");
	return str;
}
#endif

// Return the current temperature in HTML

        private string GetCurrentTempHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + m_fCurrentTemperature.ToString("00") + "</td>\n";
        }

// Return the CSP in HTML

        private string GetCSPHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + m_fCurCoolSetpoint.ToString("00") + "</td>\n";
        }

// Return the HSP in HTML

        private string GetHSPHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + m_fCurHeatSetpoint.ToString("00") + "</td>\n";
        }

        // Return the Current Humidity in HTML

        private string GetCurrentHumidityHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + m_fCurrentHumidity.ToString("00") + "</td>\n";
        }

        // Return the Humidify Set Point in HTML

        private string GetHumidifySPHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + m_fCurHumiditySetpoint.ToString("00") + "</td>\n";
        }

        // Return the Dehumidify Set Point in HTML

        private string GetDehumidifySPHTML()
        {
            return "<td align=\"center\" class=\"" + (((m_iThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + m_fCurDehumiditySetpoint.ToString("00") + "</td>\n";
        }


                private int GetThermostatHomeseerIndex(int tstatno)
        {
            return tstatno + 1;
        }

// Return a series of HTML buttons that can be used to control the mode
#if false
        private string GetModeControlHTML()
{
	CString	str;
	CString	str1;
	CString	degstr;
	BOOL	opt[4];
	BOOL	fl;
	BOOL	aux;
	BOOL	hpr;
	int		i;
	int		cnt = 0;
	int		btncnt = 0;

	get_SupportsAux(&aux);
	get_SupportsHPR(&hpr);
	str = _T("");
	str += _T("<td align=\"center\" class=\"");str+=_T("tablerowodd");str+= _T("\">\n");
	m_StatDevice->get_DisplayMode(&fl);
	if (!fl) {
	  str += _T("&nbsp;</td>");
	  return str;
	}
	m_StatDevice->get_HeatControl(&opt[0]);
	m_StatDevice->get_CoolControl(&opt[1]);
	m_StatDevice->get_AutoControl(&opt[2]);
	m_StatDevice->get_OffControl(&opt[3]);
	for (i=0;i<4;i++) if (opt[i]) cnt++;
	if (cnt == 0) {
	  str += _T("&nbsp;</td>");
	  return str;
	}
	if (opt[3]) {
	  str1.Format(_T("<INPUT class=\"formbutton\" id=\"OffButton_%d\" type=\"button\" value=\"Off\" name=\"OffButton_%d\" onclick=\"Action('%d', 'Off')\">\n"), m_ThermoNumber, m_ThermoNumber, m_ThermoNumber);
	  str += str1;
	  btncnt++;
	}
	if (opt[2]) {
	  str1.Format(_T("<INPUT class=\"formbutton\" id=\"AutoButton_%d\" type=\"button\" value=\"Auto\" name=\"AutoButton_%d\" onclick=\"Action('%d', 'Auto')\">\n"), m_ThermoNumber, m_ThermoNumber, m_ThermoNumber);
	  str += str1;
	  btncnt++;
	}
	if ((btncnt % 2) == 0) str += _T("<br>");
	if (opt[0]) {
	  str1.Format(_T("<INPUT class=\"formbutton\" id=\"HeatButton_%d\" type=\"button\" value=\"Heat\" name=\"HeatButton_%d\" onclick=\"Action('%d', 'Heat')\">\n"), m_ThermoNumber, m_ThermoNumber, m_ThermoNumber);
	  str += str1;
	  btncnt++;
	  if (aux) {
	    if ((btncnt % 2) == 0) str += _T("<br>");
	    str1.Format(_T("<INPUT class=\"formbutton\" id=\"AuxButton_%d\" type=\"button\" value=\"Aux\" name=\"AuxButton_%d\" onclick=\"Action('%d', 'Aux')\">\n"), m_ThermoNumber, m_ThermoNumber, m_ThermoNumber);
	    str += str1;
	    btncnt++;
	  }
	  if (hpr) {
	    if ((btncnt % 2) == 0) str += _T("<br>");
	    str1.Format(_T("<INPUT class=\"formbutton\" id=\"HPRButton_%d\" type=\"button\" value=\"HPR\" name=\"HPRButton_%d\" onclick=\"Action('%d', 'HPR')\">\n"), m_ThermoNumber, m_ThermoNumber, m_ThermoNumber);
	    str += str1;
	    btncnt++;
	  }
	}
	if ((btncnt % 2) == 0) str += _T("<br>");
	if (opt[1]) {
	  str1.Format(_T("<INPUT class=\"formbutton\" id=\"CoolButton_%d\" type=\"button\" value=\"Cool\" name=\"CoolButton_%d\" onclick=\"Action('%d', 'Cool')\">\n"), m_ThermoNumber, m_ThermoNumber, m_ThermoNumber);
	  str += str1;
	  btncnt++;
	}
	str += _T("</td>\n");
	return str;
}
// Return an HTML button that can be used to reset the filter days

        private string GetFilterDaysControlHTML()
{
	CString	str;
	CString	str1;

	str = _T("");
	str += _T("<td align=\"center\" class=\"");str+=_T("tablerowodd");str+= _T("\">\n");
	str1.Format(_T("<INPUT class=\"formbutton\" id=\"FilterButton_%d\" type=\"button\" value=\"Reset\" name=\"FilterButton_%d\" onclick=\"Action('%d', 'FilterReset')\">\n"), m_ThermoNumber, m_ThermoNumber, m_ThermoNumber);
	str += str1;
	str += _T("</td>\n");
	return str;
}

// Return a series of HTML buttons that can be used to control the fan mode

        private string GetFanControlHTML()
{
	CString	str;
	CString	str1;
	BOOL	onflag;
	BOOL	offflag;
	BOOL	fl;

	str = _T("");
	m_StatDevice->get_DisplayFanStatus(&fl);
	m_StatDevice->get_FanOnControl(&onflag);
	m_StatDevice->get_FanOffControl(&offflag);
	str += _T("<td align=\"center\" class=\"");str+=_T("tablerowodd");str+= _T("\">\n");
	if ((!onflag && !offflag) || !fl) {
	  str += _T("&nbsp;</td>");
	  return str;
	}
	if (onflag) {
	  str1.Format(_T("<INPUT class=\"formbutton\" id=\"FanonButton_%d\" type=\"button\" value=\"On\" name=\"FanonButton_%d\" onclick=\"Action('%d', 'Fanon')\">\n"), m_ThermoNumber, m_ThermoNumber, m_ThermoNumber);
	  str += str1;
	  if (offflag) str += _T("<br>");
	}
	if (offflag) {
	  str1.Format(_T("<INPUT class=\"formbutton\" id=\"FanAutoButton_%d\" type=\"button\" value=\"Auto\" name=\"FanAutoButton_%d\" onclick=\"Action('%d', 'Fanauto')\">\n"), m_ThermoNumber, m_ThermoNumber, m_ThermoNumber);
	  str += str1;
	}
	str += _T("</td>\n");
	return str;
}

// Return a series of HTML buttons that can be used to control the hold state

        private string GetHoldControlHTML()
{
	CString	str;
	CString	str1;
	BOOL	fl;
	BOOL	fl1;

	str = _T("");
	str += _T("<td align=\"center\" class=\"");str+=_T("tablerowodd");str+= _T("\">\n");
	m_StatDevice->get_DisplayHoldStatus(&fl);
	m_StatDevice->get_ToggleHoldControl(&fl1);
	if (!fl || !fl) {
	  str += _T("&nbsp;</td>");
	  return str;
	}
//	str1.Format(_T("<INPUT class=\"formbutton\" id=\"ToggleButton\" type=\"button\" value=\"Toggle\" name=\"ToggleButton\" onclick=\"Action('%d', 'Togglehold')\">\n"), m_ThermoNumber);
	str1.Format(_T("<INPUT class=\"formbutton\" id=\"HoldOn_%d\" type=\"button\" value=\"On\" name=\"HoldOn_%d\" onclick=\"Action('%d', 'HoldOn')\">\n"), m_ThermoNumber, m_ThermoNumber, m_ThermoNumber);
	str += str1;
	str += _T("<br>\n");
	str1.Format(_T("<INPUT class=\"formbutton\" id=\"HoldOff_%d\" type=\"button\" value=\"Off\" name=\"HoldOff_%d\" onclick=\"Action('%d', 'HoldOff')\">\n"), m_ThermoNumber, m_ThermoNumber, m_ThermoNumber);
	str += str1;
	str += _T("</td>\n");
	return str;
}

// Return a HTML drop down list of temperature for the CSP

        private string GetCSPControlHTML(CString tempStr)
{
	CString	str;
	CString	str1;
	BOOL	fl;
	int		temp;

	str = _T("");
	m_StatDevice->get_DisplayCoolSetpoint(&fl);
	str += _T("<td align=\"center\" class=\"");str+=_T("tablerowodd");str+= _T("\">\n");
	if (!fl) {;
	  str += _T("&nbsp;</td>");
	  return str;
	}
	str1.Format(_T("<SELECT id=\"CSP%d\" name=\"CSP%d\" onchange=\"Action('%d', 'CSPSet')\">\n"), m_ThermoNumber, m_ThermoNumber, m_ThermoNumber);
	str += str1;
	temp = (int) (m_CSP + 0.5);
	str += SelectTemperatureHTML(tempStr, temp, m_IsF);
//	str += GetTemperatureString(temp);
	str += _T("</SELECT></TD>\n");
	return str;
}

// Return a HTML drop down list of temperature for the HSP

        private string GetHSPControlHTML(CString tempStr)
{
	CString	str;
	CString	str1;
	BOOL	fl;
	int		temp;

	str = _T("");
	m_StatDevice->get_DisplayHeatSetpoint(&fl);
	str += _T("<td align=\"center\" class=\"");str+=_T("tablerowodd");str+= _T("\">\n");
	if (!fl) {;
	  str += _T("&nbsp;</td>");
	  return str;
	}
	str1.Format(_T("<SELECT id=\"HSP%d\" name=\"HSP%d\" onchange=\"Action('%d', 'HSPSet')\">\n"), m_ThermoNumber, m_ThermoNumber, m_ThermoNumber);
	str += str1;
	temp = (int) (m_HSP + 0.5);
	str += SelectTemperatureHTML(tempStr, temp, m_IsF);
//	str += GetTemperatureString(temp);
	str += _T("</SELECT></TD>\n");
	return str;
}
#endif
#if false
        private string BuildScheduleCell(CString classname, CTypedPtrList<CPtrList, CString*>* fList)
{
	CString								str;
	CString								str1;

	str = _T("");
	str1.Format(_T("<td class=\"%s\" align=\"center\" valign=\"top\">\n"), LPCTSTR(classname));
	str += str1;
	if (fList->GetCount() > 0) {
	  str += _T("Existing Schedule Files<br>\n");
	  str1.Format(_T("<SELECT id=\"LoadSched%d\" name=\"LoadSched%d\">\n"), m_ThermoNumber, m_ThermoNumber);
	  str += str1;
	  str += LoadTPFFileNames(fList);
	  str += _T("</SELECT><br>\n");
	  str1.Format(_T("<INPUT class=\"formbutton\" id=\"LoadSched%d\" type=\"button\" value=\"Load\" name=\"LoadSched%d\" onclick=\"Action('%d', 'Load')\">\n"), m_ThermoNumber, m_ThermoNumber, m_ThermoNumber);
	  str += str1;
	  str1.Format(_T("<INPUT class=\"formbutton\" id=\"SaveSched%d\" type=\"button\" value=\"Save\" name=\"SaveSched%d\" onclick=\"Action('%d', 'Save')\">\n"), m_ThermoNumber, m_ThermoNumber, m_ThermoNumber);
	  str += str1;
	}
	else {
	  str += _T("&nbsp;\n");
	}
	str1.Format(_T("</td>\n<td class=\"%s\" align=\"center\" valign=\"top\">\n"), LPCTSTR(classname));
	str += str1;
	str += _T("Save Schedule File As<br>\n");
	str1.Format(_T("<INPUT id=\"SaveFName%d\" type=\"text\" name=\"SaveFName%d\"><br>\n"), m_ThermoNumber, m_ThermoNumber);
	str += str1;
	str1.Format(_T("<INPUT class=\"formbutton\" id=\"SaveAsSched%d\" type=\"button\" value=\"SaveAs\" name=\"SaveAsSched%d\" onclick=\"Action('%d', 'SaveAs')\"><br>\n"), m_ThermoNumber, m_ThermoNumber, m_ThermoNumber);
	str += str1;
	str += _T("</td>\n");
	return str;
}
#endif

        #endregion

        #endregion
    }
}
