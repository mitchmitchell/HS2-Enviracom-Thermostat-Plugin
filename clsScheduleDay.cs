using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace HSPI_ENVIRACOM_MANAGER
{
    class clsScheduleDay
    {
        private clsEnviracomApp     m_objApp = clsEnviracomApp.GetInstance();
        private clsSchedulePeriod[] m_objSchedulePeriods = new clsSchedulePeriod[4];

        #region "  Constructors and Destructors  "

        //	Constructor
        public clsScheduleDay()
		{
            for (int i = 0; i < 4; i++)
                m_objSchedulePeriods[i] = new clsSchedulePeriod();

        }
        #endregion


        public void SetSchedule(int iSchedulePeriod, int iScheduleTime, float fHeatSetPoint, float fCoolSetPoint, int iFanMode)
        {
            if (iSchedulePeriod == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    m_objSchedulePeriods[i].CoolSetpoint = fCoolSetPoint;
                    m_objSchedulePeriods[i].HeatSetpoint = fHeatSetPoint;
                    m_objSchedulePeriods[i].FanMode = iFanMode;
                    m_objSchedulePeriods[i].Time = new TimeSpan(iScheduleTime / 60, iScheduleTime % 60, 0);
                }
                clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsZoneManager::SetSchedule: all schedule periods set to Cool: " + fCoolSetPoint.ToString("00.00") + " Heat: " + fHeatSetPoint.ToString("00.00") + " Fan: " + iFanMode.ToString());
            }
            else if ((iSchedulePeriod >= 1) && (iSchedulePeriod <= 4))
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

        public string GenerateHTMLTable(int thermonum, int rownum)
        {
            StringBuilder p = new StringBuilder();

            for (int i = 0; i < 4; i++)
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
