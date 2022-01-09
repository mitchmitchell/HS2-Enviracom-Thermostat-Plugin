using System;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace EnviracomManagerWeb
{
    public class Global : System.Web.HttpApplication
    {
        public static object hs;

        protected void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            IDictionary props = new Hashtable();
            Belikov.GenuineChannels.GenuineTcp.GenuineTcpChannel channel = new Belikov.GenuineChannels.GenuineTcp.GenuineTcpChannel(props, null, null);

            System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(channel, false);
            hs = (Scheduler.hsapplication)Activator.GetObject(typeof(Scheduler.hsapplication), "gtcp://" + "grendel.magnoliamanor.local" + ":8737/hs_server.rem");
            Application["HSPI_ENVIRACOM_MANAGER_HOMESEER"] = hs;

        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}