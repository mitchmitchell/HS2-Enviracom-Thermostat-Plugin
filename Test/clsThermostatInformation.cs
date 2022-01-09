using System;
using System.Diagnostics;
using System.Collections;
using System.Text;
using Scheduler;
using Scheduler.Classes;

namespace HSPI_ENVIRACOM_MANAGER
{
	/// <summary>
	/// This object represents the information about a thermostat but does not include homeseer access methods and web UI methods that are contained in clsThermostatZone.
    /// This allows the object to be used to carry parameter information to the message formatting routines without carring the baggage of the full zone device.
	/// </summary>
	public class clsThermostatInformation
	{
        #region "  Enumerations and Constants  "
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

        #endregion

        #region "  Structures  "
        #endregion

        #region "  Members  "
		private int			        m_iZoneNumber = 0;
        private int                 m_iUnitNumber = 0;
        private int                 m_iThermostatNumber = 0;
        private int                 m_iRemainingFilterTime = 0;

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
        private DamperStatusValues  m_eDamperStatus = DamperStatusValues.Uninitialized;
        private FanSwitchValues     m_eFanSwitch = FanSwitchValues.Uninitialized;
        private StateActivityValues m_eSystemStateActivity = StateActivityValues.Uninitialized;
        private HoldStatusValues    m_eCurrentStatus = HoldStatusValues.Uninitialized;
        private SystemSwitchValues  m_eSystemSwitch = SystemSwitchValues.Uninitialized;
        private AutoModeValues      m_eAutoCurrentMode = AutoModeValues.Uninitialized;
        private SystemStateValues   m_eSystemState = SystemStateValues.Uninitialized;
        private StageStateValues[]  m_eStateStates = new StageStateValues[4];

        private clsScheduleDay[]    m_objScheduleDays = new clsScheduleDay[7];
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
        public clsScheduleDay[] ScheduleDays
        {
            get
            {
                return m_objScheduleDays;
            }
        }
        #endregion

        #region "  Public Methods  "
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
        #endregion



        #region "  Constructors and Destructors  "

        //	Constructor
		public clsThermostatInformation()
		{
            for (int i = 0; i < 7; i++)
                m_objScheduleDays[i] = new clsScheduleDay();
            for (int i = 0; i < 4; i++)
                m_eStateStates[i] = StageStateValues.Uninitialized;

        }
        #endregion
    }
}
