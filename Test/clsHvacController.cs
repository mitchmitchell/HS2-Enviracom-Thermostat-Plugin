using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.IO.Ports;
using Scheduler;
using Scheduler.Classes;


namespace HSPI_ENVIRACOM_MANAGER
{
	/// <summary>
	/// 
	/// </summary>
	public class clsHvacController
	{
		#region "  Enumerations and Constants   "
		private enum Status
		{
			UNKNOWN         =	0,
			CONNECTED       =	1,
			EQUIPMENT_FAULT =	2
		};

		//  HVAC Panel Type
        private const string DT_HVAC_UNIT = "Enviracom Unit";
        private const string HVAC_LOCATION = "HVAC";
		#endregion

		#region "  Structures    "
		#endregion 

		#region "  Members   "

        private bool            m_bZoneManagerPresent = false;
        private bool            m_bDeviceAttached = false;
        private bool            m_bUnitEnabled = false;
        private DeviceClass     m_objAttachedDevice;
		private clsEnviracomApp	m_objApp = clsEnviracomApp.GetInstance();
        private int             m_iUnitNumber = 0;
        private string          m_sComPortName = "COM0";
        private clsSACommMgr    m_objSACommMgr = new clsSACommMgr();
		private string[]		m_sStatusMessages = 
		{
			"Uninitialized",
			"Connected",
			"Equipment Fault"
		};
		#endregion 

        #region "  Accessor Methods for Members  "
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

        public int UnitNumber
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

        public string Port
        {
            get
            {
                return m_sComPortName;
            }
            set
            {
                m_sComPortName = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return m_bUnitEnabled;
            }
            set
            {
                m_bUnitEnabled = value;
            }
        }

        public bool IsZoneManager
        {
            get
            {
                return m_bZoneManagerPresent;
            }
            set
            {
                m_bZoneManagerPresent = value;
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

        #endregion

		#region "  Constructors and Destructors   "
		public clsHvacController()
		{
		}
		~clsHvacController()
		{
		}
		#endregion

		#region "  Initialization and Cleanup  "
		public void Initialize()
		{
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHvacController::Initialize: calling clsSACommMgr::Initialize with port: " + m_sComPortName);
            m_objSACommMgr.Initialize(this);
        }

		public void Cleanup()
		{
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHvacController::Cleanup");
            //	Then cleanup our objects
            m_objSACommMgr.Cleanup();
		}
		#endregion

		#region "  Public Methods  "

        public bool AttachHomeseerUnitDevice()
        {
            DeviceClass dev = FindHomeseerUnitDevice();
            if (dev != null)
            {
                AttachUnitDevice(dev);
                return true;
            }
            else
            {
                if (!CreateHomeseerUnitDevice())
                    throw new System.Exception("Failed to attach homeseer device to Enviracom Unit " + m_iUnitNumber.ToString());
                return false;
            }
        }

        public void AttachUnitDevice(DeviceClass dev)
        {
            m_objAttachedDevice = dev;
            m_objApp.SetUsedDevCode(Convert.ToInt32(m_objAttachedDevice.dc));
            m_bDeviceAttached = true;
            m_objApp.ifHSApp.SetDeviceValue(
                m_objAttachedDevice.hc + m_objAttachedDevice.dc,
                (int)Status.CONNECTED);
        }
        public void UpdateUnitDevice(DeviceClass dev)
        {
            m_objAttachedDevice = dev;
        }

        public void DetachUnitDevice()
        {
            m_objApp.ifHSApp.SetDeviceValue(
                m_objAttachedDevice.hc + m_objAttachedDevice.dc,
                (int)Status.UNKNOWN);
            m_bDeviceAttached = false;
            m_objApp.FreeDevCode(Convert.ToInt32(m_objAttachedDevice.dc));
            m_objAttachedDevice = null;
        }


		public void HSEvent( object []  parms )
		{
			int				nRefCode;
			DeviceClass		dev;
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHvacController::HSEvent");

			try
			{
				//	Check if the object being referenced is from my plug-in
				nRefCode = (int)parms[3];
				dev = clsEnviracomApp.GetInstance().ifHSApp.GetDeviceByRef( nRefCode );
				if ( dev == null )
					return;
				if ( dev.@interface != clsEnviracomApp.PLUGIN_IN_NAME_SHORT )
					return;

				//	This is one of my devices, now find out which one
//				if ( nRefCode != m_nRefDevCode )
//				{
					//	Check in the zone manager to see if it is one of those objects
//					m_objZoneManager.HSEvent( dev );
//				}
			}
			catch ( Exception )
			{
			}
		}

        public void EnableHomeSeerAccess()
		{
			clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHvacController::EnableHomeSeerAccess: Enabling HomeSeer communication" );

			//	Find the hvac controller device in HomeSeer.  Note that we always
			//	must have a hvac device, so we will force creation if we 
			//	cannot find it.
            AttachHomeseerUnitDevice();

            //	Go ahead and start the communications object up now that we are enabled.
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHvacController::EnableHomeSeerAccess: calling clsSACommMgr::Startup with port: " + m_sComPortName);

            m_objSACommMgr.Startup();
            SendMessage(m_objApp.MessageMgr.FormatQueryZoneManagerMessage());
            for (int i = 0; i <= 9; i++) // query all possible zone number zero through nine - get zoned and unzoned possiblities
                SendMessage(m_objApp.MessageMgr.FormatQuerySystemStateMessage(i));
//            SendMessage(m_objApp.MessageMgr.FormatQuerySchedulesMessage(0));
		}

		public void DisableHomeSeerAccess()
		{
			clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHvacController::DisableHomeSeerAccess: Disabling HomeSeer communication" );
            DetachUnitDevice();
            m_objSACommMgr.Shutdown();
		}

        public void SendMessage(string szMessage)
        {
            // need to check that we have successfully initialized                if (m_objSACommMgr != null)
            {
                clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsHvacController::SendMessage '" + szMessage + "' to Unit " + m_iUnitNumber);

                m_objSACommMgr.TransmitMessage(szMessage);

            }
        }

        public void SetUnitDeviceStatus(string sStatus)
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
                    DetachUnitDevice();
                    clsEnviracomApp.LogInformation(
                        "clsHvacController::SetUnitStatus: Detected unit (" +
                        UnitNumber.ToString() + ":" +
                        Name +
                        ") was deleted in HomeSeer");
                }
                else
                {

                    clsEnviracomApp.LogInformation(
                        "clsHvacController::SetUnitStatus: Setting unit (" +
                        UnitNumber.ToString() + ":" +
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
                clsEnviracomApp.TraceEvent(TraceEventType.Error, "clsHvacController::SetUnitStatus: attempting to set status on a homeseer device while disconnected.");
        }



#if false
        public void SendSetTimeMessageToUnit()
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


            clsEnviracomApp.TraceEvent(TraceEventType.Verbose,"clsSACommMgr::TransmitTestMessage " + (int)(dt.DayOfWeek == 0 ? DayOfWeek.Saturday : (dt.DayOfWeek - 1)) + " " + dt.Day + "/" + dt.Month + "/" + dt.Year + " -- " + dt.Hour + ":" + dt.Minute + ":" + dt.Second);
            m_objSACommMgr.TransmitMessage(m_objApp.MessageMgr.FormatChangeTimeMessage(0, (int)(dt.DayOfWeek == 0 ? DayOfWeek.Saturday : (dt.DayOfWeek - 1)), dt.Month, dt.Day, dt.Year, dt.Hour, dt.Minute, dt.Second));
        }
        public void SendQueryTempMessageToUnit(int zone)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsSACommMgr::TransmitQueryTempMessage");
            m_objSACommMgr.TransmitMessage(m_objApp.MessageMgr.FormatQueryTempMessage(zone));
        }
        public void SendQuerySetPointsMessageToUnit(int zone)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsSACommMgr::TransmitQuerySetPointsMessage");
            m_objSACommMgr.TransmitMessage(m_objApp.MessageMgr.FormatQuerySetPointsMessage(zone));
        }
        public void SendQuerySetPointLimitsMessageToUnit(int zone)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsSACommMgr::TransmitQuerySetPointLimitsMessage");
            m_objSACommMgr.TransmitMessage(m_objApp.MessageMgr.FormatQuerySetPointLimitsMessage(zone));
        }
        public void SendQueryAllSchedulesMessageToUnit()
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsSACommMgr::TransmitQueryAllSchedulesMessage");
            m_objSACommMgr.TransmitMessage(m_objApp.MessageMgr.FormatQueryAllSchedulesMessage());
        }
        public void SendQuerySystemSwitchMessageToUnit(int zone)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsSACommMgr::TransmitQuerySystemSwitchMessage");
            m_objSACommMgr.TransmitMessage(m_objApp.MessageMgr.FormatQuerySystemSwitchMessage(zone));
        }
        public void SendQuerySystemStateMessageToUnit(int zone)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsSACommMgr::TransmitQuerySystemStateMessage");
            m_objSACommMgr.TransmitMessage(m_objApp.MessageMgr.FormatQuerySystemStateMessage(zone));
        }
        public void SendFilterMessageToUnit(int zone)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsSACommMgr::TransmitFilterMessage");
            m_objSACommMgr.TransmitMessage(m_objApp.MessageMgr.FormatQueryFilterMessage(zone));
        }
        public void SendTestMessageToUnit()
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsSACommMgr::TransmitTestMessage");
            for (int i = 0; i < 10; i++)
            {
                m_objSACommMgr.TransmitMessage(m_objApp.MessageMgr.FormatQuerySystemSwitchMessage(i));
            }
        }
        public void SendHoldMessageToUnit(int iZoneNumber, int iHeatSetPoint, int iHoldStatus, int iCoolSetPoint, int iUnknown_5, int iRecovery, int iPeriod, int iUnknown_7)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsSACommMgr::TransmitHoldMessage");
            m_objSACommMgr.TransmitMessage(m_objApp.MessageMgr.FormatChangeSetPointsMessage(iZoneNumber, iHeatSetPoint, iHoldStatus, iCoolSetPoint, iUnknown_5, iRecovery, iPeriod, iUnknown_7));
        }
        public void SendModeMessageToUnit(int iZoneNumber, int iSwitchSetting)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsSACommMgr::TransmitModeMessage");
            m_objSACommMgr.TransmitMessage(m_objApp.MessageMgr.FormatChangeModeMessage(iZoneNumber, iSwitchSetting));
        }

        public void SendFanMessageToUnit(int iZoneNumber, int iFanModeOne, int iFanModeTwo)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsSACommMgr::TransmitFanMessage");
            m_objSACommMgr.TransmitMessage(m_objApp.MessageMgr.FormatChangeFanMessage(iZoneNumber, iFanModeOne, iFanModeTwo));
        }

        public void SendSetPointMessageToUnit(int iZoneNumber, int iHeatSetPoint, int iHoldStatus, int iCoolSetPoint, int iUnknown_5, int iRecovery, int iPeriod, int iUnknown_7)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsSACommMgr::TransmitSetPointMessage");
            m_objSACommMgr.TransmitMessage(m_objApp.MessageMgr.FormatChangeSetPointsMessage(iZoneNumber, iHeatSetPoint, iHoldStatus, iCoolSetPoint, iUnknown_5, iRecovery, iPeriod, iUnknown_7));
        }
#endif
        #endregion

        #region "  public Web UI Methods  "

        // Builds this units's portion of the units HTML page.

        public string BuildUnitPage()
        {
            StringBuilder p = new StringBuilder();
            string classname = ((m_iUnitNumber % 2) == 0 ? "tableroweven" : "tablerowodd");

            p.Append("<tr>\n");

            p.Append("<td noWrap class=\"" + classname + "\" align=center valign=middle>" + m_iUnitNumber + "</td>\n");
            p.Append("<td noWrap class=\"" + classname + "\" align=left valign=middle width='50%%'>\n");
            p.Append("<table border='0' width='100%' cellspacing='0' cellpadding='0'><tr>");
            p.Append("<td width='15%' align=right>" + GetStringHTML(Name) + "</td>\n<td width='85%'>");
            p.Append("</td>");
            p.Append("</tr>");
            p.Append("</table>");
            p.Append("</td>");

            // Comm port drop down list

            p.Append("<td class=\"" + classname + "\" align=center>\n");
            p.Append("<table cellSpacing=1 cellPadding=1 border=0>\n");
//@@@@            p.Append("<select class=\"formdropdown\" s=\"" + clsEnviracomApp.CFG_KEY_UNITPORT + m_iUnitNumber + "\" style=\"width: 100%%\" ID=\"UnitCom" + m_iUnitNumber + "\"size=\"1\">");

            foreach (string name in SerialPort.GetPortNames())
            {
                if ((m_sComPortName.ToLower()) == name.ToLower())
                    p.Append(" <option value=\"" + name + "\" selected>" + name + "\n");
                else
                    p.Append(" <option value=\"" + name + "\">" + name + "\n");
            }
            p.Append("</option>");
            p.Append("</select>\n");
            p.Append("</table>\n");
            p.Append("</td>\n");
            p.Append("</tr>\n");
            return p.ToString();
        }

        #endregion

		#region "  Private Methods   "
        private string GetStringHTML(string s)
        {
            return s.Replace(" ", "&nbsp;");
        }

        private int GetUnitNumber(string sIoMisc)
        {
            return Convert.ToInt32(sIoMisc);
        }

        private bool CreateHomeseerUnitDevice()
        {
            DeviceClass dev;

            //  Create the device
            dev = m_objApp.ifHSApp.NewDeviceEx(DT_HVAC_UNIT + " " + m_iUnitNumber.ToString());
            SetHomeseerUnitDeviceAttributes(dev);
            //	Finally, tell the zone what it's dev code is
            AttachUnitDevice(dev);

            clsEnviracomApp.TraceEvent(TraceEventType.Verbose,
                "clsHvacController::CreateHomeseerUnitDevice: Created unit (" +
                m_iUnitNumber.ToString() + ":" +
                Name +
                ") at device code " +
                dev.dc);

            return true;
        }

        private void DeleteHomeseerUnitDevice()
        {

            //	remove the unit from HS and the dev code from EnviracomApp
            if (m_bDeviceAttached == true)
            {
                int nRef = m_objApp.ifHSApp.GetDeviceRef(HouseCode + DevCode);
                clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsZoneManager::DeleteHomeseerUnitDevice: Deleting device (" +
                    DevCode + ":" +
                    Name + ")");
                DetachUnitDevice();
                m_objApp.ifHSApp.DeleteDevice(nRef);
            }
        }

        private void SetHomeseerUnitDeviceAttributes(DeviceClass dev)
        {
            //  set all the relevant information
            dev.location = HVAC_LOCATION;
            dev.hc = m_objApp.HouseCode;
            dev.dc = Convert.ToString(m_objApp.GetLastFreeDevCode());
            dev.@interface = clsEnviracomApp.PLUGIN_IN_NAME_SHORT;
            dev.misc = HomeSeer.MISC_STATUS_ONLY;
            dev.dev_type_string = DT_HVAC_UNIT;
            dev.iotype = HomeSeer.IOTYPE_INPUT;
            dev.iomisc = Convert.ToString(m_iUnitNumber);
            dev.values =
                "Uninitialized" + Convert.ToChar(2) + Convert.ToString((int)Status.UNKNOWN) + Convert.ToChar(1) +
                "Connected" + Convert.ToChar(2) + Convert.ToString((int)Status.CONNECTED) + Convert.ToChar(1) +
                "Fault" + Convert.ToChar(2) + Convert.ToString((int)Status.EQUIPMENT_FAULT);
        }

        private DeviceClass FindHomeseerUnitDevice()
        {
            clsDeviceEnumeration de;
            DeviceClass dev;

            //  Enumerate through all the devices looking for one of mine
            de = (clsDeviceEnumeration)m_objApp.ifHSApp.GetDeviceEnumerator();
            while (!de.Finished)
            {
                dev = (DeviceClass)de.GetNext();
                if ((dev.dev_type_string == DT_HVAC_UNIT) && (GetUnitNumber(dev.iomisc) == m_iUnitNumber))
                {
                    return dev;
                }
            }
            return null;
        }

		#endregion
	}
}
