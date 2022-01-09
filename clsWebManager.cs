using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace HSPI_ENVIRACOM_MANAGER
{
	/// <summary>
	/// 
	/// </summary>
	public class clsWebManager
	{
		//	Public members required by HomeSeer
		public string	link;
		public string	linktext;
		public string	page_title;
        private clsEnviracomApp m_objApp = clsEnviracomApp.GetInstance();

		public clsWebManager()
		{
			link =			"enviracom_config";
			linktext =		"Enviracom Configuration";
			page_title =	"Enviracom Application Configuration";
		}



		public string GenPage(ref string lnk)
		{
			StringBuilder	p = new StringBuilder();

            p.Append(GenLinksMenu());

            if (lnk.Contains("page=1") == true)
            {
                p.Append(BuildThermoPage());
            }
            else if (lnk.Contains("page=2") == true)
            {
                p.Append(BuildUnitsPage());
            }
            else if (lnk.Contains("page=3") == true)
            {
                p.Append(BuildSchedulePage());
            }
            else if (lnk.Contains("page=4") == true)
            {
                p.Append(BuildConfigPage());
            }
            else if (lnk.Contains("page=5") == true)
            {
                p.Append(BuildDevicePage());
            }
            else if (lnk.Contains("page=6") == true)
            {
                p.Append(BuildAdvancedPage());
            }
            else
            {
                p.Append(BuildThermoPage());
            }
			return p.ToString();
		}
        public string BuildThermoPage()
        {
            StringBuilder p = new StringBuilder();
            clsEnviracomApp.TraceEvent(System.Diagnostics.TraceEventType.Verbose, "clsWebManager::BuildThermoPage");

            p.Append("<form method=\"post\" s=\"form1\">");

            // Build the table that shows the configuration of each thermostat and provides controls to operate the stat
            // The items and controls displayed are determined by what options are set in the status device object

            p.Append("<table border=\"1\" cellspacing = \"0\" cellpadding = \"5\">\n<tr>\n");
            p.Append("<tr><td class=\"tableheader\" colspan=\"18\">Enviracom&nbsp;Thermostat&nbsp;Status</td></tr>");
            p.Append("<td class=\"tablecolumn\">Number</td>\n");
            p.Append("<td class=\"tablecolumn\">Homeseer<br>Device</td>\n");
            p.Append("<td class=\"tablecolumn\">Thermostat&nbsp;Name</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>System<br>Switch</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Circulate<br>Fan&nbsp;Mode</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Blower<br>Fan&nbsp;Speed</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Fan<br>Mode</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>System<br>Status</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>System<br>State</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Stage&nbsp;One<br>State</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Stage&nbsp;Two<br>State</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Stage&nbsp;Three<br>State</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Stage&nbsp;Four<br>State</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Current<br>Temp</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Cool<br>Set&nbsp;Point</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Heat<br>Set&nbsp;Point</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Current<br>Humidity</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Humidify<br>Set&nbsp;Point</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Dehumidify<br>Set&nbsp;Point</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Time&nbsp;to<br>Filter&nbsp;Chg</td>\n");
            p.Append("</tr>");

            // Let each stat object build its own row in the table
            foreach (clsThermostatZone zone in m_objApp.Thermostats)
            {
                p.Append(zone.BuildThermoString());
                clsEnviracomApp.TraceEvent(System.Diagnostics.TraceEventType.Verbose, "clsWebManager::BuildThermoPage calling clsThermostatZone::BuildThermoString for zone " + zone.Zone);
            }

            p.Append("</table>");

            // Configure the hidden data values

            p.Append("<input type=\"hidden\" s=\"ref_page\" value=\"" + link + "\">\n");
            p.Append("<input type=\"hidden\" s=\"special_info\" value=\"3\">\n");
            p.Append("<input type=\"hidden\" s=\"action\" value=\"\">\n");
            p.Append("<input type=\"hidden\" s=\"Thermostat\" value=\"\">\n");
            p.Append("</form>\n");

            // Functions used by the web page to add, delete and update thermostats

            // Script
            p.Append("<script language=\"JavaScript\">\n");
            p.Append("function Action(thermo, doaction) {\n");
            p.Append("document.form1.action.value = doaction;\n");
            p.Append("document.form1.Thermostat.value = thermo;\n");
            p.Append("document.form1.submit();\n");
            p.Append("}\n");
            p.Append("</script>\n");

            return p.ToString();
        }
        public string BuildUnitsPage()
        {
            StringBuilder p = new StringBuilder();
            p.Append("<form method=\"post\" s=\"form1\">");

            // If no units exist, provide a button to add a unit and then return

            if (m_objApp.NumberOfUnits < 1)
            {
                p.Append("<P>\n<table cellSpacing=\"0\" cellPadding=\"0\" width=\"0%\" border=\"0\">\n<tr>");
                p.Append("<td align=\"center\" width=\"50%\" rowSpan=\"1\"><font color=\"red\">No HVAC units defined</font><br>&nbsp;</td>\n");
                p.Append("</tr><tr>\n");
                p.Append("<td align=\"center\">\n");
                p.Append("<input class=\"formbutton\" id=\"AddUnit\" onclick=\"Action('0', 'AddUnit')\" type=\"button\" value=\"Add Hvac Unit\" s=\"AddUnit\">\n &nbsp;&nbsp; \n");
                p.Append("</td></tr>\n");
                p.Append("</table>\n</P>\n");
                p.Append("<input type=\"hidden\" s=\"Action\" value=\"\">\n");
                p.Append("<input type=\"hidden\" s=\"Unit\" value=\"\">\n");
                p.Append("<input type=\"hidden\" s=\"ref_page\" value=\"" + link + "\">");
                p.Append("</form>\n");
                p.Append("\n<script language=\"JavaScript\">\n");
                p.Append("function Action(unit, doaction) {\n");
                p.Append("document.form1.action.value = doaction;\n");
                p.Append("document.form1.Unit.value = unit;\n");
                p.Append("document.form1.submit();\n");
                p.Append("}\n");
                p.Append("</script>\n");
                return p.ToString();
            }

            // Build the table that shows the configuration of each thermostat and provides controls to operate the stat
            // The items and controls displayed are determined by what options are set in the status device object

            p.Append("<table border=\"1\" cellspacing = \"0\" cellpadding = \"5\">\n<tr>\n");
            p.Append("<tr><td class=\"tableheader\" colspan=\"8\">Enviracom Controller Configuration</td></tr>");
            p.Append("<td class=\"tablecolumn\">Unit#</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Name</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Serial Port</td>\n");
            p.Append("</tr>");

            // Let each stat object build its own row in the table
            foreach (clsHvacController ctrlr in m_objApp.Units)
            {
                p.Append(ctrlr.BuildUnitPage());
            }

            // Functions used by the web page to add, delete and update thermostats


            // Add controls to add a new unit or delete the last unit

            p.Append("<tr>\n");
            p.Append("<td colspan=\"5\" class=\"" + ((m_objApp.NumberOfUnits % 2) == 1 ? "tableroweven" : "tablerowodd") + "\" align=\"center\">");
            p.Append("<input class=\"formbutton\" id=\"AddUnit\" onclick=\"DoAddUnit('" + m_objApp.NumberOfUnits + "', 'AddUnit')\" type=\"button\" value=\"Add New Unit\" s=\"AddUnit\">\n &nbsp;&nbsp; \n");
            p.Append("<input class=\"formbutton\" id=\"DelUnit\" onclick=\"DoDelUnit('" + (m_objApp.NumberOfUnits - 1) + "', 'DelUnit')\" type=\"button\"value=\"Remove Unit # " + (m_objApp.NumberOfUnits - 1) + "\" s=\"DelUnit\">\n");
            p.Append("</td></tr>\n");
            p.Append("</table>");

            p.Append("<p><input class=\"formbutton\" id=\"SavUnit\" onclick=\"DoSavUnit('" + m_objApp.NumberOfUnits + "', 'SavUnit')\" type=\"button\" value=\"Save Unit Settings\" s=\"SavUnit\">\n &nbsp;&nbsp; \n");

            // Config the hidden data values

            p.Append("<input type=\"hidden\" s=\"ref_page\" value=\"" + link + "\">\n");
            p.Append("<input type=\"hidden\" s=\"Action\" value=\"\">\n");
            p.Append("<input type=\"hidden\" s=\"Unit\" value=\"\">\n");
            p.Append("</form>\n");

            // Script

            p.Append("<script language=\"JavaScript\">\n");
            p.Append("function DoAddUnit(unit, doaction) {\n");
            p.Append("  document.form1.Action.value = doaction;\n");
            p.Append("  document.form1.Unit.value = unit;\n");
            p.Append("  document.form1.submit();\n");
            p.Append("}\n\n");
            p.Append("function DoSavUnit(unit, doaction) {\n");
            p.Append("  document.form1.Action.value = doaction;\n");
            p.Append("  document.form1.Unit.value = unit;\n");
            p.Append("  document.form1.submit();\n");
            p.Append("}\n\n");
            p.Append("function DoDelUnit(unit, doaction) {\n");
            p.Append("  if (confirm(\"Are you sure you want to delete the unit - " + m_objApp.Units[m_objApp.NumberOfUnits - 1].Name + "?\")) {\n");
            p.Append("    document.form1.Action.value = doaction;\n");
            p.Append("    document.form1.Unit.value = unit;\n");
            p.Append("    document.form1.submit();\n");
            p.Append("  }\n");
            p.Append("}\n");
            p.Append("</script>\n");

            return p.ToString();
        }
        public string BuildSchedulePage()
        {
            StringBuilder p = new StringBuilder();

            p.Append("<form method=\"post\" s=\"form1\">");

            // Build the table that shows the configuration of each thermostat and provides controls to operate the stat
            // The items and controls displayed are determined by what options are set in the status device object

            p.Append("<table border=\"1\" cellspacing = \"0\" cellpadding = \"5\">\n<tr>\n");
            p.Append("<tr><td class=\"tableheader\" colspan=\"8\">Enviracom Thermostat Schedule Configuration</td></tr>");
            p.Append("<td class=\"tablecolumn\">Thermostat</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Sunday Schedule</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Monday Schedule</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Tuesday Schedule</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Wednesday Schedule</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Thursday Schedule</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Friday Schedule</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Saturday Schedule</td>\n");
            p.Append("</tr>");

            // Let each stat object build its own row in the table
            foreach (clsThermostatZone zone in m_objApp.Thermostats)
            {
                p.Append(zone.BuildSchedulePage());
                clsEnviracomApp.TraceEvent(System.Diagnostics.TraceEventType.Verbose, "clsWebManager::BuildSchedulePage calling clsThermostatZone::BuildSchedulePage zone " + zone.Zone);
            }

            p.Append("</table>");

            // Configure the hidden data values

            p.Append("<input type=\"hidden\" s=\"ref_page\" value=\"" + link + "\">\n");
            p.Append("<input type=\"hidden\" s=\"special_info\" value=\"3\">\n");
            p.Append("<p><input class=\"formbutton\" type=\"submit\" value=\"Submit\" s=\"Submit\"></p>\n");
            p.Append("<input type=\"hidden\" s=\"action\" value=\"\">\n");
            p.Append("<input type=\"hidden\" s=\"Thermostat\" value=\"\">\n");
            p.Append("</form>\n");

            // Functions used by the web page to add, delete and update thermostats

            p.Append("<script language=\"JavaScript\">\n");
            p.Append("function Action(thermo, doaction) {\n");
            p.Append("  if (doaction != \"SaveAs\") {\n");
            p.Append("      document.form1.action.value = doaction;\n");
            p.Append("      document.form1.Thermostat.value = thermo;\n");
            p.Append("      document.form1.submit();\n");
            p.Append("  } else {\n");
            p.Append("	    d2 = \"SaveFName\" + thermo;\n");
            p.Append("	    if (document.form1.elements(d2).value == \"\") {\n");
            p.Append("	        alert(\"You must specify a file s to save the schedule to.\");\n");
            p.Append("	    } else {\n");
            p.Append("	        document.form1.action.value = doaction;\n");
            p.Append("	        document.form1.Thermostat.value = thermo;\n");
            p.Append("	        document.form1.submit();\n");
            p.Append("	    }\n");
            p.Append("  }\n");
            p.Append("}\n");
            p.Append("</script>\n");

            return p.ToString();
        }
        public string BuildConfigPage()
        {
            StringBuilder p = new StringBuilder();
            clsEnviracomApp.TraceEvent(System.Diagnostics.TraceEventType.Verbose, "clsWebManager::BuildConfigPage");
            p.Append("<form method=\"post\" s=\"form1\">");
            p.Append("<table border=\"1\" cellspacing = \"0\" cellpadding = \"5\">\n<tr>\n");
            p.Append("<tr><td class=\"tableheader\" colspan=\"5\">Enviracom Thermostat Status Configuration</td></tr>");
            p.Append("<tr>");
            p.Append("<td class=\"tablecolumn\" align=center>Address</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Thermostat</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Display</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Show Controls</td>\n");
            p.Append("<td class=\"tablecolumn\" align=center>Enabled</td>\n");
            p.Append("</tr>");

            // Let each stat object build its own row in the table
            foreach (clsThermostatZone zone in m_objApp.Thermostats)
            {
                p.Append(zone.BuildConfigPage());
                clsEnviracomApp.TraceEvent(System.Diagnostics.TraceEventType.Verbose, "clsWebManager::BuildConfigPage calling clsThermostatZone::BuildConfigPage for zone " + zone.Zone);
            }

            // Add controls to add a new thermostat or delete the last thermostat
            p.Append("<tr>\n");
            p.Append("<td colspan=\"5\" class=\"" + ((m_objApp.NumberOfThermostats % 2) == 1 ? "tableroweven" : "tablerowodd") + "\" align=\"center\">");
            p.Append("<INPUT class=\"formbutton\" id=\"AddThermo\" onclick=\"Action('0', 'AddThermo')\" type=\"button\" value=\"Add New Thermostat\" s=\"AddThermo\">\n &nbsp;&nbsp; \n");
            if (m_objApp.NumberOfThermostats > 0)
            {
                p.Append("<INPUT class=\"formbutton\" id=\"DelThermo\" onclick=\"DoDelThermo('0', 'DelThermo')\" type=\"button\"value=\"Remove Thermostat #" + m_objApp.NumberOfThermostats + "\" s=\"DelThermo\">\n");
            }
            p.Append("</td></tr>\n");
            p.Append("</table>");

            // Config the hidden data values

            p.Append("<input type=\"hidden\" s=\"ref_page\" value=\"" + link + "\">\n");
            p.Append("<input type=\"hidden\" s=\"special_info\" value=\"3\">\n");
            p.Append("<input type=\"hidden\" s=\"action\" value=\"\">\n");
            p.Append("<input type=\"hidden\" s=\"Thermostat\" value=\"\">\n");
            p.Append("<P><INPUT class=\"formbutton\" type=\"submit\" value=\"Save\" s=\"Submit\"></P>\n");
            p.Append("</form>\n");
            // Script
            p.Append("<script language=\"JavaScript\">\n");
            p.Append("function OptionChecked(flag, number) {\n");
            p.Append("  d1 = \"StatDev\" + number;\n");
            p.Append("  d2 = \"StatName\" + number;\n");
            p.Append("  d3 = \"StatLoc\" + number;\n");
            p.Append("  if (flag == \"true\") {\n");
            p.Append("    document.form1.elements(d1).disabled = false;\n");
            p.Append("    document.form1.elements(d2).disabled = true;\n");
            p.Append("    document.form1.elements(d3).disabled = true;\n");
            p.Append("  } else {\n");
            p.Append("    document.form1.elements(d1).disabled = true;\n");
            p.Append("    document.form1.elements(d2).disabled = false;\n");
            p.Append("    document.form1.elements(d3).disabled = false;\n");
            p.Append("  }\n");
            p.Append(" }\n\n");
            p.Append("function Action(thermo, doaction) {\n");
            p.Append("document.form1.action.value = doaction;\n");
            p.Append("document.form1.Thermostat.value = thermo;\n");
            p.Append("document.form1.submit();\n");
            p.Append("}\n");
            if (m_objApp.NumberOfThermostats > 0)
            {
                p.Append("\nfunction DoDelThermo(thermo, doaction) {\n");
                p.Append("if (confirm(\"Are you sure you want to delete Thermostat - " + m_objApp.Thermostats[m_objApp.NumberOfThermostats - 1].Name + "?\")) {\n");
                p.Append("document.form1.action.value = doaction;\n");
                p.Append("document.form1.Thermostat.value = thermo;\n");
                p.Append("document.form1.submit();\n");
                p.Append("}\n");
            }
            p.Append("}\n");
            p.Append("</script>\n");
            return p.ToString();
        }
        public string BuildDevicePage()
        {
            StringBuilder p = new StringBuilder();
            return p.ToString();
        }
        public string BuildAdvancedPage()
        {
            StringBuilder p = new StringBuilder();
            return p.ToString();
        }

        public void PagePut(ref string sFieldList)
        {
            string[] sList = sFieldList.Split('&');
            string pg;
            Dictionary<string,string> dctParams = new Dictionary<string,string>();

            clsEnviracomApp.TraceEvent(System.Diagnostics.TraceEventType.Verbose, "clsWebManager::PagePut '" + sFieldList + "'");
            clsEnviracomApp.TraceEvent(System.Diagnostics.TraceEventType.Verbose, "clsWebManager::PagePut sList count '" + sList.GetUpperBound(0) + "'");

            foreach (string sl in sList)
            {
                string[] sListParams = sl.Split('=');
                if (!dctParams.ContainsKey(sListParams[0]))
                {
                    dctParams.Add(sListParams[0], sListParams[1]);
                    clsEnviracomApp.TraceEvent(System.Diagnostics.TraceEventType.Verbose, "clsWebManager::PagePut adding key: '" + sListParams[0] + "' value: '" + sListParams[1] + "'");
                }
                else
                    clsEnviracomApp.TraceEvent(System.Diagnostics.TraceEventType.Verbose, "clsWebManager::PagePut duplicate key: '" + sListParams[0] + "' value: '" + sListParams[1] + "'");
            }


            if (dctParams.TryGetValue("page", out pg) == true)
            {
                switch (pg)
                {
                    case "1":
                        PutThermoPage(dctParams);
                        break;
                    case "2":
                        PutUnitsPage(dctParams);
                        break;
                    case "3":
                        PutSchedulePage(dctParams);
                        break;
                    case "4":
                        PutConfigPage(dctParams);
                        break;
                    case "5":
                        PutDevicePage(dctParams);
                        break;
                    case "6":
                        PutAdvancedPage(dctParams);
                        break;
                    default:
                        clsEnviracomApp.TraceEvent(System.Diagnostics.TraceEventType.Verbose, "clsWebManager::PagePut invalid page value: '" + pg + "'");
                        break;
                }
            }
            else
            {
                clsEnviracomApp.TraceEvent(System.Diagnostics.TraceEventType.Verbose, "clsWebManager::PagePut page parameter not found");
            }
        }

        public void PutThermoPage(Dictionary<string, string> dctParams)
        {
            clsEnviracomApp.TraceEvent(System.Diagnostics.TraceEventType.Verbose, "clsWebManager::PutThermoPage '" + dctParams.ToString() + "'");
        }
        public void PutUnitsPage(Dictionary<string, string> dctParams)
        {
            clsEnviracomApp.TraceEvent(System.Diagnostics.TraceEventType.Verbose, "clsWebManager::PutUnitsPage");
            foreach( KeyValuePair<string, string> kvp in dctParams )
            {
                /*
                numUnits" value="3" />
    <add key="UnitPort_0" value="6" />
    <add key="UnitPort_1" value="7" />
    <add key="UnitPort_2" value="5" />
    <add key="logLevel" value="5" />
    <add key="msgDelay" value="1000" />
    <add key="msgTimeOut" value="2000" />
    <add key="msgWaitAck" value="1000" />
    <add key="msgBaudRate" value="19200" />
                 *         public const string     CFG_KEY_LOGLEVEL = "logLevel";
        public const string     CFG_KEY_MAXUNITS = "numUnits";
        public const string     CFG_KEY_UNITPORT = "UnitPort_";
        public const string     CFG_KEY_MSGDELAY = "msgDelay";
        public const string     CFG_KEY_MSGTIMEOUT = "msgTimeOut";
        public const string     CFG_KEY_MSGWAITACK = "msgWaitAck";
        public const string     CFG_KEY_BAUDRATE = "msgBaudRate";

                 * */
                clsEnviracomApp.TraceEvent(System.Diagnostics.TraceEventType.Verbose, "clsWebManager::PutUnitsPage kvp key: '" + kvp.Key + "' value: '" + kvp.Value + "'");
                if (kvp.Key.Contains(clsEnviracomApp.CFG_KEY_UNITPORT))
                    m_objApp.SetConfig(kvp.Key, kvp.Value);
            }
        }
        public void PutSchedulePage(Dictionary<string, string> dctParams)
        {
            clsEnviracomApp.TraceEvent(System.Diagnostics.TraceEventType.Verbose, "clsWebManager::PutSchedulePage '" + dctParams.ToString() + "'");
        }
        public void PutConfigPage(Dictionary<string, string> dctParams)
        {
            clsEnviracomApp.TraceEvent(System.Diagnostics.TraceEventType.Verbose, "clsWebManager::PutConfigPage '" + dctParams.ToString() + "'");
        }
        public void PutDevicePage(Dictionary<string, string> dctParams)
        {
            clsEnviracomApp.TraceEvent(System.Diagnostics.TraceEventType.Verbose, "clsWebManager::PutDevicePage '" + dctParams.ToString() + "'");
        }
        public void PutAdvancedPage(Dictionary<string,string> dctParams)
        {
            clsEnviracomApp.TraceEvent(System.Diagnostics.TraceEventType.Verbose, "clsWebManager::PutAdvancedPage '" + dctParams.ToString() + "'");
        }



        // Create the in-line HTML menu that lets the user select which attirbute will be set-up
        public string GenLinksMenu()
        {
            StringBuilder p = new StringBuilder();

            p.Append(@"<p>");
            p.Append(@"<A href=""/" + link + @"?page=1"">Main</A>");
            p.Append(@"&nbsp;| <A href=""/" + link + @"?page=2"">Units</A>");
            p.Append(@"&nbsp;| <A href=""/" + link + @"?page=3"">Schedule</A>");
            p.Append(@"&nbsp;| <A href=""/" + link + @"?page=4"">Configuration</A>");
            p.Append(@"&nbsp;| <A href=""/" + link + @"?page=5"">Devices</A>");
            p.Append(@"&nbsp;| <A href=""/" + link + @"?page=6"">Advanced Setup</A>");
            p.Append(@"</p>");

            return p.ToString();
        }


    }
}
