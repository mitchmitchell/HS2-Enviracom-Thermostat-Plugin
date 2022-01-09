using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Scheduler;
using Scheduler.Classes;
using HSPI_ENVIRACOM_MANAGER;

namespace EnviracomManagerWeb
{
    public partial class EnviracomSchedules : System.Web.UI.Page
    {
        protected HSPI m_objPlugin;
        protected hsapplication m_objHomeSeer;
        protected void Page_Load(object sender, EventArgs e)
        {
            LoadHomeSeerObjects();
            LoadPluginObjects();
            LoadPageObjects();
        }
        protected void LoadHomeSeerObjects()
        {
            if (Application["HSPI_ENVIRACOM_MANAGER_HOMESEER"] == null)
            {
                object objApplicationItem = Context.Items["Content"];

                if (objApplicationItem == null || objApplicationItem.GetType().ToString() != "Scheduler.hsapplication")
                {
                    throw new Exception("Unknown Context. Can't get a pointer to HomeSeer");
                }

                m_objHomeSeer = (hsapplication)objApplicationItem;
                Application["HSPI_ENVIRACOM_MANAGER_HOMESEER"] = objApplicationItem;
            }
            else
            {
                m_objHomeSeer = (hsapplication)Application["HSPI_ENVIRACOM_MANAGER_HOMESEER"];
            }
        }

        protected bool LoadPluginObjects()
        {
            object objPluginReference = m_objHomeSeer.Plugin(HSPI.HSPI_PLUGIN_NAME);

            if (objPluginReference != null)
            {
                m_objPlugin = (HSPI)objPluginReference;
                return true;
            }
            return false;
        }

        protected bool LoadPageObjects()
        {
            DataSet  ds = m_objPlugin.WebMgr().ScheduleDataSet();
            m_gvSchedulesGridView.DataSource = ds;
            m_gvSchedulesGridView.DataBind();
            return true;
        }
    }
}