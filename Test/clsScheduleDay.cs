using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace HSPI_ENVIRACOM_MANAGER
{
    public class clsScheduleDay
    {
        #region "  Enumerations and Constants   "
        public const int MAXIMUM_SCHEDULE_PERIODS = 4;
        #endregion

        #region "  Members  "

        private clsEnviracomApp     m_objApp = clsEnviracomApp.GetInstance();
        private clsSchedulePeriod[] m_objSchedulePeriods = new clsSchedulePeriod[MAXIMUM_SCHEDULE_PERIODS];

        #endregion

        #region "  Constructors and Destructors  "

        //	Constructor
        public clsScheduleDay()
		{
            for (int i = 0; i < MAXIMUM_SCHEDULE_PERIODS; i++)
                m_objSchedulePeriods[i] = new clsSchedulePeriod();

        }
        #endregion
        #region " Properties "

        public string WakeTime
        {
            get
            {
                return m_objSchedulePeriods[0].Time.ToString();
            }
        }


        public string WakeCoolSetPoint
        {
            get
            {
                return m_objSchedulePeriods[0].CoolSetpoint.ToString();
            }
        }


        public string WakeHeatSetPoint
        {
            get
            {
                return m_objSchedulePeriods[0].HeatSetpoint.ToString();
            }
        }


        public string WakeFanMode
        {
            get
            {
                return m_objSchedulePeriods[0].FanMode.ToString();
            }
        }


        public string LeaveTime
        {
            get
            {
                return m_objSchedulePeriods[1].Time.ToString();
            }
        }

        public string LeaveCoolSetPoint
        {
            get
            {
                return m_objSchedulePeriods[1].CoolSetpoint.ToString();
            }
        }


        public string LeaveHeatSetPoint
        {
            get
            {
                return m_objSchedulePeriods[1].HeatSetpoint.ToString();
            }
        }


        public string LeaveFanMode
        {
            get
            {
                return m_objSchedulePeriods[1].FanMode.ToString();
            }
        }



        public string ReturnTime
        {
            get
            {
                return m_objSchedulePeriods[2].Time.ToString();
            }
        }

        public string ReturnCoolSetPoint
        {
            get
            {
                return m_objSchedulePeriods[2].CoolSetpoint.ToString();
            }
        }


        public string ReturnHeatSetPoint
        {
            get
            {
                return m_objSchedulePeriods[2].HeatSetpoint.ToString();
            }
        }


        public string ReturnFanMode
        {
            get
            {
                return m_objSchedulePeriods[2].FanMode.ToString();
            }
        }




        public string SleepTime
        {
            get
            {
                return m_objSchedulePeriods[3].Time.ToString();
            }
        }

        public string SleepCoolSetPoint
        {
            get
            {
                return m_objSchedulePeriods[3].CoolSetpoint.ToString();
            }
        }


        public string SleepHeatSetPoint
        {
            get
            {
                return m_objSchedulePeriods[3].HeatSetpoint.ToString();
            }
        }


        public string SleepFanMode
        {
            get
            {
                return m_objSchedulePeriods[3].FanMode.ToString();
            }
        }




        #endregion
        public void SetSchedule(int iSchedulePeriod, int iScheduleTime, float fHeatSetPoint, float fCoolSetPoint, int iFanMode)
        {
            if (iSchedulePeriod == 0)
            {
                for (int i = 0; i < MAXIMUM_SCHEDULE_PERIODS; i++)
                {
                    m_objSchedulePeriods[i].CoolSetpoint = fCoolSetPoint;
                    m_objSchedulePeriods[i].HeatSetpoint = fHeatSetPoint;
                    m_objSchedulePeriods[i].FanMode = iFanMode;
                    m_objSchedulePeriods[i].Time = new TimeSpan(iScheduleTime / 60, iScheduleTime % 60, 0);
                }
                clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsZoneManager::SetSchedule: all schedule periods set to Cool: " + fCoolSetPoint.ToString("00.00") + " Heat: " + fHeatSetPoint.ToString("00.00") + " Fan: " + iFanMode.ToString());
            }
            else if ((iSchedulePeriod >= 1) && (iSchedulePeriod <= MAXIMUM_SCHEDULE_PERIODS))
            {
                m_objSchedulePeriods[iSchedulePeriod - 1].CoolSetpoint = fCoolSetPoint;
                m_objSchedulePeriods[iSchedulePeriod - 1].HeatSetpoint = fHeatSetPoint;
                m_objSchedulePeriods[iSchedulePeriod - 1].FanMode = iFanMode;
                m_objSchedulePeriods[iSchedulePeriod - 1].Time = new TimeSpan(iScheduleTime / 60, iScheduleTime % 60, 0); new TimeSpan(iScheduleTime / 60, iScheduleTime % 60, 0);
                clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsZoneManager::SetSchedule: schedule period '" + iSchedulePeriod.ToString() + "' set to Cool: " + fCoolSetPoint.ToString("00.00") + " Heat: " + fHeatSetPoint.ToString("00.00") + " Fan: " + iFanMode.ToString());
            }
            else
            {
                clsEnviracomApp.TraceEvent(TraceEventType.Error, "clsZoneManager::SetSchedule: schedule period out of range: '" + iSchedulePeriod.ToString() + "'");
            }
        }

        // Display the schedule as an HTML table

        public string GenerateScheduleHTMLTable(int thermonum, int rownum)
        {
            StringBuilder p = new StringBuilder();

            for (int i = 0; i < MAXIMUM_SCHEDULE_PERIODS; i++)
            {
                p.Append(GeneratePeriodTable(thermonum, rownum, i));
            }

            return p.ToString();
        }
        

        private string GeneratePeriodTable(int thermonum, int rownum, int period)
        {
            StringBuilder p = new StringBuilder();

            p.Append("<table cellSpacing=\"1\" cellPadding=\"1\" border=\"1\" id=\"Table3\">\n");
            p.Append("<tr>\n");
            p.Append("<td noWrap class=\"tablecolumn\">Start Time</TD>\n");
            p.Append("<td noWrap class=\"tablecolumn\">Cool Setpt</TD>\n");
            p.Append("<td noWrap class=\"tablecolumn\">Heat Setpt</TD>\n");
            p.Append("<td noWrap class=\"tablecolumn\">Fan Mode</TD>\n");
            p.Append("</tr>\n");
            p.Append("<tr>\n");

            // Time
            p.Append("<td noWrap class=\"tableroweven\" align=\"center\">\n");
            p.Append("<select id=\"Time" + thermonum + "_" + rownum + "_" + period + "\" s=\"TIM" + thermonum + "_" + rownum + "_" + period + "\">");

            if (m_objSchedulePeriods[period].Time.Days > 0)
                p.Append("<option value=\"1\" selected>" + "--" + ":" + "--" + "</option>\n");
            else
                p.Append("<option value=\"1\" selected>" + m_objSchedulePeriods[period].Time.Hours.ToString("00") + ":" + m_objSchedulePeriods[period].Time.Minutes.ToString("00") + "</option>\n");
            p.Append("</select></td>\n");

            // CSP
            p.Append("<td noWrap class=\"tableroweven\" align=\"center\">\n");
            p.Append("<select id=\"CSP" + thermonum + "_" + rownum + "_" + period + "\" s=\"CSP" + thermonum + "_" + rownum + "_" + period + "\">");
            p.Append("<option value=\"1\" selected>" + m_objSchedulePeriods[period].CoolSetpoint.ToString("00") + "</option>\n");
            p.Append("</select></td>\n");

            // HSP
            p.Append("<td noWrap class=\"tableroweven\" align=\"center\">\n");
            p.Append("<select id=\"HSP" + thermonum + "_" + rownum + "_" + period + "\" s=\"HSP" + thermonum + "_" + rownum + "_" + period + "\">");
            p.Append("<option value=\"1\" selected>" + m_objSchedulePeriods[period].HeatSetpoint.ToString("00") + "</option>\n");
            p.Append("</select></td>\n");

            // Fan Mode
            p.Append("<td noWrap class=\"tableroweven\" align=\"center\">\n");
            p.Append("<select id=\"HSP" + thermonum + "_" + rownum + "_" + period + "\" s=\"FMD" + thermonum + "_" + rownum + "_" + period + "\">");
            p.Append("<option value=\"1\" selected>" + m_objSchedulePeriods[period].FanMode + "</option>\n");
            p.Append("</select></td>\n");

            p.Append("</tr>\n");
            p.Append("</table>\n");

            return p.ToString();
        }

    }
}
