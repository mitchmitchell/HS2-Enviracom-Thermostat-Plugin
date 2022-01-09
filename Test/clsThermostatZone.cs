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

        private const string DT_HVAC_ZONE = "Enviracom Thermostat";
        private const string ZONE_LOCATION = "HVAC";
        private const char IOMISC_SEPERATOR = '^';
        private const int IOMISC_ZONE_INDEX = 0;
        private const int IOMISC_UNIT_INDEX = 1;
        #endregion

        #region "  Structures  "
        #endregion

        #region "  Members  "
        private clsThermostatInformation    m_objThermostat = new clsThermostatInformation();
        private int                         m_iThermostatNumber = 0;
        private bool                        m_bDeviceAttached = false;
        private DeviceClass                 m_objAttachedDevice = null;
        private clsEnviracomApp             m_objApp = clsEnviracomApp.GetInstance();
        #endregion

        #region "  Accessor Methods for Members  "

        // This is an internal number used to find thermostats because homeseer does not have any concept of zone
        // However, homeseer bases thermostat numbers starting at 1 and the plugin starts at 0
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
                return m_objThermostat.Zone;
			}
			set
			{
				m_objThermostat.Zone = value;
			}
		}
        public int Unit
        {
            get
            {
                return m_objThermostat.Unit;
            }
            set
            {
                m_objThermostat.Unit = value;
            }
        }
        public int RemainingFilterTime
        {
            get
            {
                return m_objThermostat.RemainingFilterTime;
            }
            set
            {
                m_objThermostat.RemainingFilterTime = value;
            }
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

        public bool DeviceAttached
        {
            get
            {
                return m_bDeviceAttached;
            }
        }
        public clsThermostatInformation.HoldStatusValues HoldStatus
		{
			get
			{
                return m_objThermostat.HoldStatus;
			}
			set
			{
                m_objThermostat.HoldStatus = value;
			}
		}
        public float OutdoorTemp
        {
            get
            {
                return m_objThermostat.OutdoorTemp;
            }
            set
            {
                m_objThermostat.OutdoorTemp = value;
            }
        }
        public float CurrentTemp
        {
            get
            {
                return m_objThermostat.CurrentTemp;
            }
            set
            {
                m_objThermostat.CurrentTemp = value;
                SetZoneDeviceStatus(value.ToString("00"));
            }
        }
        public float HeatSetPoint
		{
			get
			{
                return m_objThermostat.HeatSetPoint;
            }
			set
			{
                m_objThermostat.HeatSetPoint = value;
			}
		}
        public float CoolSetPoint
		{
			get
			{
                return m_objThermostat.CoolSetPoint;
            }
			set
			{
                m_objThermostat.CoolSetPoint = value;
			}
		}
        public float CurrentHumidity
        {
            get
            {
                return m_objThermostat.CurrentHumidity;
            }
            set
            {
                m_objThermostat.CurrentHumidity = value;
            }
        }
        public float HumiditySetpoint
        {
            get
            {
                return m_objThermostat.HumiditySetpoint;
            }
            set
            {
                m_objThermostat.HumiditySetpoint = value;
            }
        }
        public float DehumiditySetpoint
        {
            get
            {
                return m_objThermostat.DehumiditySetpoint;
            }
            set
            {
                m_objThermostat.DehumiditySetpoint = value;
            }
        }


        public float MaxHeatSetpoint
        {
            get
            {
                return m_objThermostat.MaxHeatSetpoint;
            }
            set
            {
                m_objThermostat.MaxHeatSetpoint = value;
            }
        }
        public float MaxCoolSetpoint
        {
            get
            {
                return m_objThermostat.MaxCoolSetpoint;
            }
            set
            {
                m_objThermostat.MaxCoolSetpoint = value;
            }
        }
        public float MinHeatSetpoint
        {
            get
            {
                return m_objThermostat.MinHeatSetpoint;
            }
            set
            {
                m_objThermostat.MinHeatSetpoint = value;
            }
        }
        public float MinCoolSetpoint
        {
            get
            {
                return m_objThermostat.MinCoolSetpoint;
            }
            set
            {
                m_objThermostat.MinCoolSetpoint = value;
            }
        }
        public float MaxHumiditySetpoint
        {
            get
            {
                return m_objThermostat.MaxHumiditySetpoint;
            }
            set
            {
                m_objThermostat.MaxHumiditySetpoint = value;
            }
        }
        public float MinHumiditySetpoint
        {
            get
            {
                return m_objThermostat.MinHumiditySetpoint;
            }
            set
            {
                m_objThermostat.MinHumiditySetpoint = value;
            }
        }
        public float MaxDehumiditySetpoint
        {
            get
            {
                return m_objThermostat.MaxDehumiditySetpoint;
            }
            set
            {
                m_objThermostat.MaxDehumiditySetpoint = value;
            }
        }
        public float MinDehumiditySetpoint
        {
            get
            {
                return m_objThermostat.MinDehumiditySetpoint;
            }
            set
            {
                m_objThermostat.MinDehumiditySetpoint = value;
            }
        }
        public clsThermostatInformation.SystemSwitchValues SystemSwitch
        {
            get
            {
                return m_objThermostat.SystemSwitch;
            }
            set
            {
                m_objThermostat.SystemSwitch = value;
            }
        }
        public clsThermostatInformation.FanSwitchValues FanSwitch
        {
            get
            {
                return m_objThermostat.FanSwitch;
            }
            set
            {
                m_objThermostat.FanSwitch = value;
            }
        }
        public clsThermostatInformation.CirculateFanValues CirculateFanStatus
        {
            get
            {
                return m_objThermostat.CirculateFanStatus;
            }
            set
            {
                m_objThermostat.CirculateFanStatus = value;
            }
        }
        public clsThermostatInformation.DamperStatusValues DamperStatus
        {
            get
            {
                return m_objThermostat.DamperStatus;
            }
            set
            {
                m_objThermostat.DamperStatus = value;
            }
        }

        public clsThermostatInformation.BlowerFanValues BlowerFanStatus
        {
            get
            {
                return m_objThermostat.BlowerFanStatus;
            }
            set
            {
                m_objThermostat.BlowerFanStatus = value;
            }
        }
        public clsThermostatInformation.AuxHeatStatusValues AuxHeatStatus
        {
            get
            {
                return m_objThermostat.AuxHeatStatus;
            }
            set
            {
                m_objThermostat.AuxHeatStatus = value;
            }
        }
        public clsThermostatInformation.StateActivityValues SystemStateActivity
        {
            get
            {
                return m_objThermostat.SystemStateActivity;
            }
            set
            {
                m_objThermostat.SystemStateActivity = value;
            }
        }
        public clsThermostatInformation.AutoModeValues AutoCurrentMode
        {
            get
            {
                return m_objThermostat.AutoCurrentMode;
            }
            set
            {
                m_objThermostat.AutoCurrentMode = value;
            }
        }
        public clsThermostatInformation.SystemStateValues SystemState
        {
            get
            {
                return m_objThermostat.SystemState;
            }
            set
            {
                m_objThermostat.SystemState = value;
            }
        }
        public clsThermostatInformation.StageStateValues StageOneState
        {
            get
            {
                return m_objThermostat.StageOneState;
            }
            set
            {
                m_objThermostat.StageOneState = value;
            }
        }
        public clsThermostatInformation.StageStateValues StageTwoState
        {
            get
            {
                return m_objThermostat.StageTwoState;
            }
            set
            {
                m_objThermostat.StageTwoState = value;
            }
        }
        public clsThermostatInformation.StageStateValues StageThreeState
        {
            get
            {
                return m_objThermostat.StageThreeState;
            }
            set
            {
                m_objThermostat.StageThreeState = value;
            }
        }
        public clsThermostatInformation.StageStateValues StageFourState
        {
            get
            {
                return m_objThermostat.StageFourState;
            }
            set
            {
                m_objThermostat.StageFourState = value;
            }
        }
        public clsScheduleDay[] ScheduleDays
        {
            get
            {
                return m_objThermostat.ScheduleDays;
            }
        }
 
        #endregion

        #region "  Constructors and Destructors  "
        #endregion

        #region "  Initialization and Cleanup  "
        #endregion

        #region "  Public Methods  "
        public bool AttachHomeseerZoneDevice()
        {
            DeviceClass dev = FindHomeseerZoneDevice();
            if (dev != null)
            {
                AttachZoneDevice(dev);
                return true;
            }
            else
            {
                if (!CreateHomeseerZoneDevice())
                    throw new System.Exception("Failed to attach homeseer device to Enviracom Zone " + Zone.ToString() + " on Unit " + Unit.ToString());
                return false;
            }
        }

        public void AttachZoneDevice(DeviceClass dev)
        {
            m_objAttachedDevice = dev;
            m_objApp.SetUsedDevCode(Convert.ToInt32(m_objAttachedDevice.dc));
            m_bDeviceAttached = true;
            m_objApp.ifHSApp.SetDeviceValue(
                m_objAttachedDevice.hc + m_objAttachedDevice.dc,
                (int)Status.CONNECTED);
        }

        public void UpdateZoneDevice(DeviceClass dev)
        {
            m_objAttachedDevice = dev;
        }

        public void DetachZoneDevice()
        {
            m_objApp.ifHSApp.SetDeviceValue(
                m_objAttachedDevice.hc + m_objAttachedDevice.dc,
                (int)Status.UNKNOWN);
            m_bDeviceAttached = false;
            m_objApp.FreeDevCode(Convert.ToInt32(m_objAttachedDevice.dc));
            m_objAttachedDevice = null;
        }

        public void HSEvent(object[] parms)
        {
            int nRefCode;
            DeviceClass dev;
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsThermostatZone::HSEvent");

            try
            {
                //	Check if the object being referenced is from my plug-in
                nRefCode = (int)parms[3];
                dev = clsEnviracomApp.GetInstance().ifHSApp.GetDeviceByRef(nRefCode);
                if (dev == null)
                    return;
                if (dev.@interface != clsEnviracomApp.PLUGIN_IN_NAME_SHORT)
                    return;

                //	This is one of my devices, now find out which one
                //				if ( nRefCode != m_nRefDevCode )
                //				{
                //	Check in the zone manager to see if it is one of those objects
                //					m_objZoneManager.HSEvent( dev );
                //				}
            }
            catch (Exception)
            {
            }
        }

        public void EnableHomeSeerAccess()
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsThermostatZone::EnableHomeSeerAccess: Enabling HomeSeer communication");

            //	Find the hvac controller device in HomeSeer.  Note that we always
            //	must have a hvac device, so we will force creation if we 
            //	cannot find it.
            AttachHomeseerZoneDevice();
        }

        public void DisableHomeSeerAccess()
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsThermostatZone::DisableHomeSeerAccess: Disabling HomeSeer communication");
            DetachZoneDevice();
        }


        public void SetSchedule(int iScheduleDay, int iSchedulePeriod, int iScheduleTime, float fHeatSetPoint, float fCoolSetPoint, int iFanMode)
        {
            m_objThermostat.SetSchedule(iScheduleDay, iSchedulePeriod, iScheduleTime, fHeatSetPoint, fCoolSetPoint, iFanMode);
        }

        #region "  public Web UI Methods  "
        // Builds this stat's portion of the Configuration HTML page.
        // This page allows the user to specify a status deviec from a drop down list of all defined HS devices, to select the items that are displayed for 
        // this stat, to select the controls to display for this stat, and to enable or disable this stat

        public string BuildConfigPage()
        {
            StringBuilder p = new StringBuilder();
            string classname = ((ThermostatNumber % 2) == 0 ? "tableroweven" : "tablerowodd");

            p.Append("<tr>\n");
            p.Append("<td noWrap class=\"" + classname + "\" align=center valign=middle>" + clsEnviracomApp.GetThermostatHomeseerIndex(ThermostatNumber) + "</td>\n");
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

            p.Append("<INPUT id=\"Enabled" + ThermostatNumber + "\" class=\"formcheckbox\" type=\"checkbox\" s=\"Enabled" + ThermostatNumber + "\"" + " CHECKED" + ">");
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

	        return p.ToString();
        }

        // Builds the HTML page that display's this stat's schedule

        public string BuildSchedulePage()
        {
            StringBuilder p = new StringBuilder();
            string classname = ((ThermostatNumber % 2) == 0 ? "tableroweven" : "tablerowodd");

            // Display the stat's s and have it span 2 rows
	
            p.Append("<tr>\n");
            p.Append(GetDeviceNameHTML());

            // Create a table for each schedule

            int rownum = 1;
            foreach (clsScheduleDay sd in ScheduleDays)
            {
                p.Append("<td class=\"" + classname + "\" align=center>\n");
                p.Append(sd.GenerateScheduleHTMLTable(ThermostatNumber, rownum));
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
                    DetachZoneDevice();
                    clsEnviracomApp.LogInformation(
                        "clsThermostatZone::SetZoneStatus: Detected zone (" +
                        Zone.ToString() + ":" +
                        Name +
                        ") was deleted in HomeSeer");
                }
                else
                {

                    clsEnviracomApp.LogInformation(
                        "clsThermostatZone::SetZoneDeviceStatus: Setting zone (" +
                        Zone.ToString() + ":" +
                        Name +
                        ") status to " + sStatus);

                    m_objApp.ifHSApp.SetDeviceString(
                        HouseCode + DevCode,
                        sStatus,
                        true);

                    m_objApp.ifHSApp.SetDeviceValue(
                        HouseCode + DevCode,
                        (int)Status.CONNECTED);
                }
            }
            else
                clsEnviracomApp.TraceEvent(TraceEventType.Error, "clsThermostatZone::SetZoneDeviceStatus: attempting to set status on a homeseer device while disconnected.");
        }

        private bool CreateHomeseerZoneDevice()
        {
            DeviceClass dev;

            //  Create the device
            dev = m_objApp.ifHSApp.NewDeviceEx("Unit " + Unit.ToString() + " Thermostat " + Zone.ToString());
            SetHomeseerZoneDeviceAttributes(dev);
            //	Finally, tell the zone what it's dev code is
            AttachZoneDevice(dev);

            clsEnviracomApp.LogInformation(
                "clsZoneManager::GetZone: Created zone (" +
                Zone.ToString() + ":" +
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
                DetachZoneDevice();
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
            dev.iomisc = Convert.ToString(Zone) + IOMISC_SEPERATOR + Convert.ToString(Unit);
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
                if ((dev.dev_type_string == DT_HVAC_ZONE) && (GetZoneUnit(dev.iomisc) == Unit) && (GetZoneNumber(dev.iomisc) == Zone))
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
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + clsEnviracomApp.GetThermostatHomeseerIndex(ThermostatNumber) + "</td>\n";
        }

        // Function that formats the stat device as a table cell

        private string GetDeviceCodeHTML()
        {
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + HouseCode + DevCode + "</td>\n";
        }

        // Function that formats the stat name as a table cell

        private string GetDeviceNameHTML()
        {
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + Location + "&nbsp;" + Name + "</td>\n";
        }

        // Return the stat mode in HTML

        private string GetSystemSwitchHTML()
        {
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + SystemSwitch.ToString() + "</td>\n";
        }

        // Return the circulate fan status in HTML

        private string GetCirculateFanHTML()
        {
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + CirculateFanStatus.ToString() + "</td>\n";
        }

        // Return the blower fan status in HTML

        private string GetBlowerFanHTML()
        {
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + BlowerFanStatus.ToString() + "</td>\n";
        }

        // Return the fan status in HTML

        private string GetActiveModeHTML()
        {
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + SystemStateActivity.ToString() + "</td>\n";
        }
        
        // Return the current operating status in HTML

        private string GetCurrentStatusHTML()
        {
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + HoldStatus.ToString() + "</td>\n";
        }
        private string GetSystemStateHTML()
        {
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + SystemState.ToString() + "</td>\n";
        }
        private string GetStageOneStateHTML()
        {
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + StageOneState.ToString() + "</td>\n";
        }
        private string GetStageTwoStateHTML()
        {
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + StageTwoState.ToString() + "</td>\n";
        }
        private string GetStageThreeStateHTML()
        {
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + StageThreeState.ToString() + "</td>\n";
        }
        private string GetStageFourStateHTML()
        {
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + StageFourState.ToString() + "</td>\n";
        }

        // Return the Days until Filter Change in HTML

        private string GetFilterDaysHTML()
        {
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + RemainingFilterTime.ToString() + "</td>\n";
        }
        
        // Return the current temperature in HTML

        private string GetCurrentTempHTML()
        {
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + CurrentTemp.ToString("00") + "</td>\n";
        }

        // Return the CSP in HTML

        private string GetCSPHTML()
        {
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + CoolSetPoint.ToString("00") + "</td>\n";
        }

        // Return the HSP in HTML

        private string GetHSPHTML()
        {
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + HeatSetPoint.ToString("00") + "</td>\n";
        }

        // Return the Current Humidity in HTML

        private string GetCurrentHumidityHTML()
        {
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + CurrentHumidity.ToString("00") + "</td>\n";
        }

        // Return the Humidify Set Point in HTML

        private string GetHumidifySPHTML()
        {
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + HumiditySetpoint.ToString("00") + "</td>\n";
        }

        // Return the Dehumidify Set Point in HTML

        private string GetDehumidifySPHTML()
        {
            return "<td align=\"center\" class=\"" + (((ThermostatNumber % 2) == 0) ? "tablerowodd" : "tableroweven") + "\">" + DehumiditySetpoint.ToString("00") + "</td>\n";
        }


        private int GetThermostatHomeseerIndex(int tstatno)
        {
            return tstatno + 1;
        }


        #endregion

        #endregion
    }
}
