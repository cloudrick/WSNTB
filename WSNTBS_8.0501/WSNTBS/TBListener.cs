
/*
 * TBListener.cs
 * Wireless Sensor Network Testbed Listenner Class
 *
 * Author:
 * Wei-Sheng Yang (Rick) rick@axp1.csie.ncu.edu.tw
 *
 * Copyright By
 * High Speed Communication and Computing Lab (HSCC)
 * National Central University (NCU)
 * National Tsing Hua University (NTHU)
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace WSNTBS
{
    class TBListener
    {
        // Const
        public const int MAX_BYTE_OF_LINE = 512;

        // Data member
        private string m_COMPort;           // COMPort to listen
        private bool m_IsListenRequest ;    // if Listener will be started
        private StringBuilder m_LogBuffer;  // save the COMPort log
        private string m_ResultLogs;        // save the listen resuslt

        public static bool IsListenDone = false;
        public static Mutex mutIsListenDone = new Mutex();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inCOMPort"> The Serail Port to listen</param>
        /// <param name="inIsListenRequest"> Listen Request from user </param>
        public TBListener(string inCOMPort,bool inIsListenRequest)
        {
            m_COMPort = inCOMPort;
            m_IsListenRequest = inIsListenRequest ;
            m_LogBuffer = new StringBuilder();
            m_ResultLogs = "Successful";
        }

        /// <summary>
        /// Get Listen Logs (UART Logs)
        /// </summary>
        /// <returns></returns>
        public string GetListenLogs()
        {
            return m_LogBuffer.ToString();
        }

        /// <summary>
        /// Get Result Logs (Listening Logs)
        /// </summary>
        /// <returns></returns>
        public string GetResultLogs()
        {
            return m_ResultLogs;
        }

        /// <summary>
        /// To Listen the UART Logs
        /// </summary>
        public void Listen()
        {
            if ( m_COMPort == "" || !m_IsListenRequest)
            {
                return;
            }

            try
            {
                m_LogBuffer.Remove(0, m_LogBuffer.Length);

                SerialPort port = new SerialPort(m_COMPort,57600,Parity.None,8,StopBits.One);

                port.Open();

                int[] buffer = new int[MAX_BYTE_OF_LINE];
                int bytes = 0;

                // allow maximun log length to 10MB
                while ( m_LogBuffer.Length < 10*Math.Pow(2,20) )
                {
                    #region Critical Section for accessing TBListener.IsListenDone
                    mutIsListenDone.WaitOne();
                    if (IsListenDone == true)
                    {
                        mutIsListenDone.ReleaseMutex();
                        break;
                    }
                    mutIsListenDone.ReleaseMutex();
                    #endregion

                    if (port.BytesToRead > 0)
                    {
                        int ch = port.ReadByte();

                        if (ch == 0x7e || bytes >= MAX_BYTE_OF_LINE)
                        {
                            if (bytes > 2)
                            {
                                if (buffer[0] == 0x7e)
                                {
                                    DateTime datetime = DateTime.Now;

                                    m_LogBuffer.Append(datetime.ToString("yyyy/MM/dd HH:mm:ss "));
                                    for (int i = 0; i < bytes; ++i)
                                    {
                                        string tmp = Convert.ToString(buffer[i], 16);

                                        if (buffer[i] < 16)
                                        {
                                            tmp = "0" + tmp;
                                        }

                                        m_LogBuffer.Append(tmp + " ");
                                    }
                                    m_LogBuffer.Append("\r\n");
                                }
                                bytes = 0;
                            }
                            else
                            {
                                buffer[0] = 0x7e;
                                bytes = 1;
                            }
                        }
                        else
                        {
                            buffer[bytes++] = ch;
                        }
                    }

                    //Thread.Sleep(500);
                }
                port.Close();
                return;
            }
            catch (Exception)
            {
                m_ResultLogs = "Some Exceptions Occur";
                return;
            }
        }
    }
}
