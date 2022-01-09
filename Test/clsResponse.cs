using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace HSPI_ENVIRACOM_MANAGER
{
    /// <summary>
    /// 
    /// </summary>
    class clsResponse
    {
        #region "  Enumerations and Constants   "
        #endregion

        #region "  Structures    "
        #endregion

        #region "   Members   "
        public string name;
        public int ticks;
        #endregion

        #region "  Accessor Methods for Members  "
        #endregion

        #region "  Constructors and Destructors   "
        public clsResponse(string n, int t)
        {
            name = n;
            ticks = t;
        }

        ~clsResponse()
        {
        }
        #endregion

        #region "  Static Methods  "
        #endregion

        #region "  Initialization and Cleanup  "
        #endregion

        #region "  Public Methods  "
        #endregion

        #region "  Private Methods   "
        #endregion
    }
}
