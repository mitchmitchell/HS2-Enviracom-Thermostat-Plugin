using System;
using System.Collections.Generic;
using System.Text;

namespace HSPI_ENVIRACOM_MANAGER
{
    class clsSchedulePeriod
    {
        private TimeSpan    m_dtTime;
        private float       m_fHeatSetpoint;
        private float       m_fCoolSetpoint;
        private int         m_iFanMode;

        public float HeatSetpoint
        {
            get
            {
                return m_fHeatSetpoint;
            }
            set
            {
                m_fHeatSetpoint = value;
            }
        }
        public float CoolSetpoint
        {
            get
            {
                return m_fCoolSetpoint;
            }
            set
            {
                m_fCoolSetpoint = value;
            }
        }

        public int FanMode
        {
            get
            {
                return m_iFanMode;
            }
            set
            {
                m_iFanMode = value;
            }
        }

        public TimeSpan Time
        {
            get
            {
                return m_dtTime;
            }
            set
            {
                m_dtTime = value;
            }
        }

    }
}
