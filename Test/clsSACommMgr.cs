using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    public class clsSACommMgr
    {
        #region "  Enumerations and Constants   "
        private const string LOG_MESSAGES = "enviracom_sa_messages.log";
        #endregion

        #region "  Structures    "
        struct Message
        {
            public delegate void ProcessMessageDelegate(string sMsg);

            public string text;
            public string desc;
            public ProcessMessageDelegate proc;

            public Message(string sText, string sDesc, ProcessMessageDelegate DoW)
            {
                text = sText;
                desc = sDesc;
                proc = DoW;
            }
        };
        #endregion

        #region "   Members   "

        private Dictionary<string, Message> m_dctAdapterDictionary = new Dictionary<string, Message>();
        private clsEnviracomApp             m_objApp = clsEnviracomApp.GetInstance();
        private SerialPort                  m_spComPort = null;
        private string                      m_sComPortName = "COM0";
        private bool                        m_bRunning = false;
        private clsHvacController           m_objHvacUnit = null;
        private Thread                      m_thrReadThread = null;
        private ProcessingQueue<string>     m_queTransmitQueue = new ProcessingQueue<string>();
        private ProcessingQueue<string>     m_queReceiveQueue = new ProcessingQueue<string>();
        private EventWaitHandle             m_ewhWaitProcess = new EventWaitHandle(false, EventResetMode.AutoReset);
        #endregion

        #region "  Accessor Methods for Members  "
        public string Port
        {
            get
            {
                return m_sComPortName;
            }
        }

        #endregion

        #region "  Constructors and Destructors   "
        public clsSACommMgr()
        {
            m_dctAdapterDictionary.Add("[Idle]",     new Message("[Idle]",   "HVAC_IDLE", TraceAdapterMessage));
            m_dctAdapterDictionary.Add("[Ack]",      new Message("[Ack]", "HVAC_ACK", ReceiveAdapterAckMessage));
            m_dctAdapterDictionary.Add("[Nak]",      new Message("[Nak]", "HVAC_NAK", TraceAdapterMessage));
            m_dctAdapterDictionary.Add("[Reset]",    new Message("[Reset]", "HVAC_RESET", TraceAdapterMessage));
        }

        ~clsSACommMgr()
        {
        }
        #endregion

        #region "  Static Methods  "
        #endregion

        #region "  Initialization and Cleanup  "
        public void Initialize(clsHvacController hvacUnit)
        {
            //	Init members as necessary
            try
            {
                m_objHvacUnit = hvacUnit;
                m_sComPortName = m_objHvacUnit.Port;
                clsEnviracomApp.TraceEvent(TraceEventType.Verbose, clsEnviracomApp.PLUGIN_IN_NAME_SHORT + ": communications will be on port: " + m_sComPortName);
            }
            catch (Exception ex)
            {
                clsEnviracomApp.TraceEvent(TraceEventType.Critical, "clsSACommMgr::Initialize: Reporting exception: " + ex.ToString());
            }
        }

        public void Cleanup()
        {
            //	Cleanup members and set others to defaults
            m_objHvacUnit = null;
            m_sComPortName = "";
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsSACommMgr::Cleanup called ");
        }

        public void Startup()
        {
            //  Start listening for data
            try
            {
                m_bRunning = true;

                m_spComPort = new SerialPort(m_sComPortName, m_objApp.BaudRate, Parity.None);
                m_spComPort.ReadTimeout = m_objApp.MsgTimeOut;
                m_spComPort.Open();

                m_queReceiveQueue.DoWork += new ProcessingQueue<string>.DoWorkDlgt(OnDoReceive);
                m_queTransmitQueue.DoWork += new ProcessingQueue<string>.DoWorkDlgt(OnDoTransmit);
                
                m_thrReadThread = new Thread(ReceiveThread);
                m_thrReadThread.IsBackground = true;
                m_thrReadThread.Start();

                clsEnviracomApp.LogInformation("clsSACommMgr::Startup: Opened COM port: " + m_sComPortName + " for unit: " + m_objHvacUnit.UnitNumber);
            }
            catch (Exception ex)
            {
                //	This isn't good, log it appropriately
                clsEnviracomApp.TraceEvent(TraceEventType.Critical, "clsSACommMgr::Startup: Reporting exception: " + ex.ToString());
            }
        }

        public void Shutdown()
        {
            //  Cleanup some variables, have to wait for a callback to stop listening for the data
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsSACommMgr::Shutdown called ");

            m_bRunning = false;

            m_queTransmitQueue.Stop();
            m_queReceiveQueue.Stop();
            m_thrReadThread.Join();
            m_spComPort.Close();
            m_spComPort = null;
        }
        #endregion

        #region "  Public Methods  "
        public void TransmitMessage(string szMessage)
        {
            m_queTransmitQueue.QueueForWork(szMessage);
        }
        #endregion
        #region "  Private Methods   "
        #region "  Private Communications Methods  "
        private void OnDoReceive(object sender, ProcessingQueueEventArgs<string> args)
        {
            m_objApp.MessageMgr.Receive(args.Work, m_objHvacUnit.UnitNumber);
        }


        private void OnDoTransmit(object sender, ProcessingQueueEventArgs<string> args)
        {
            string szMessage = args.Work;
            try
            {
                //  Make sure we are running before processing
                if (m_bRunning)
                {
                    m_spComPort.WriteLine(szMessage + "\r");
                    clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsSACommMgr::Transmit: sent message: '" + szMessage + "' on port: " + m_sComPortName);
                    if (m_ewhWaitProcess.WaitOne(m_objApp.MsgWaitAck) == true)
                        clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsSACommMgr::Transmit: got ack on port: " + m_sComPortName);
                    else
                        clsEnviracomApp.TraceEvent(TraceEventType.Warning, "clsSACommMgr::Transmit: timeout on port: " + m_sComPortName);
                    Thread.Sleep(m_objApp.MsgDelay);
                }
                else
                {
                    //  We are no longer running, so just log this attempt and throw away this message
                    clsEnviracomApp.TraceEvent(TraceEventType.Warning, "clsSACommMgr::Transmit: tried to send message when communications not enabled: '" + szMessage + "' on port: " + m_sComPortName);
                }
            }
            catch (Exception ex)
            {
                //	This isn't good, log it appropriately
                clsEnviracomApp.TraceEvent(TraceEventType.Critical, "clsSACommMgr::Transmit: Reporting exception: " + ex.ToString() + " on port: " + m_sComPortName);
            }
        }
        private void ReceiveThread()
        {
            //  Make sure we are running before processing
            while (m_bRunning)
            {
                try
                {
                    String receiveBytes = m_spComPort.ReadLine();

                    //	Check if this is adapter message.  The adapter messages we are looking
                    //	for start with the string ">[" and end with the string "]\r".
                    if ((receiveBytes.StartsWith(">[") && receiveBytes.EndsWith("]\r")))
                    {
                        Message msg;

                        string sMessage = receiveBytes.TrimStart('>').TrimEnd('\r');
                        clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsSACommMgr::ReceiveThread: read adapter message: '" + receiveBytes.TrimEnd('\r') + "' on port: " + m_sComPortName);
                        if (m_dctAdapterDictionary.TryGetValue(sMessage, out msg) == true)
                            msg.proc(sMessage);
                        else
                            clsEnviracomApp.TraceEvent(TraceEventType.Warning, "clsSaCommMgr::ReceiveThread: did not find adapter message '" + sMessage + "' in dictionary");

                    }
                    //	Check if this is the correct message type.  The enviracom messages we are looking
                    //	for start with a '>' and end with a '\r'.
                    else if (receiveBytes.StartsWith(">") && receiveBytes.EndsWith("\r"))
                    {
                        string sMessage = receiveBytes.TrimStart('>').TrimEnd('\r');
                        clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsSACommMgr::ReceiveThread: read enviracom message: '" + receiveBytes.TrimEnd('\r') + "' on port: " + m_sComPortName);
                        m_queReceiveQueue.QueueForWork(sMessage);
                    }
                    else
                    {
                        clsEnviracomApp.TraceEvent(TraceEventType.Error, "clsSACommMgr::ReceiveThread: message not formatted correctly: '" + receiveBytes.TrimEnd('\r') + "' on port: " + m_sComPortName);
                    }
                }
                catch (TimeoutException)
                {
                    // we are continually getting timeout exceptions -- is this ok?
//                    clsEnviracomApp.TraceEvent(TraceEventType.Error, "clsSACommMgr::ReceiveThread: exception waiting for timeout on port: " + m_sComPortName);
                    if (!m_bRunning)
                        return;
                }
                catch (Exception ex)
                {
                        clsEnviracomApp.TraceEvent(TraceEventType.Critical, "clsSACommMgr::ReceiveThread: Reporting exception: " + ex.ToString() + " on port: " + m_sComPortName);
                }
            }
        }


        #endregion


        #region "  Private Internal Methods   "
        private void TraceAdapterMessage(string sMsg)
        {
            clsEnviracomApp.TraceEvent(TraceEventType.Verbose, "clsSACommMgr::TraceAdapterMessage: message string is '" + sMsg + "'");
        }

        private void ReceiveAdapterAckMessage(string sMsg)
        {
            m_ewhWaitProcess.Set();
            clsEnviracomApp.TraceEvent(TraceEventType.Information, "clsSACommMgr::ReceiveAdapterAckMessage: message string is '" + sMsg + "'");
        }
        #endregion

        #endregion
    }
}
