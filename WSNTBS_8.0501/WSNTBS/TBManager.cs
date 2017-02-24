
/*
 * TBManager.cs
 * Wireless Sensor Network Testbed Region Manager Class
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
using System.IO;
using System.Threading;
using System.Text;
using System.Windows.Forms;

namespace WSNTBS
{
    /// <summary>
    /// Region Manager Class
    /// </summary>
    class TBManager
    {
        // used for lock shared variable m_IsStarted
        private Mutex m_mutIsStarted;
        // used for record is the manager started
        private bool m_IsStarted ;

        // reference to Main Form (For UI)
        private MainForm m_refToMainForm;
        private TBConfiguration m_refToConfig;

        // record the nodes that the maneger will manage
        private int m_StartNodeID;
        private int m_NumberOfNodes;

        // programmer and listener and database agent
        private TBDatabaseAgent m_tbDatabase;
        private TBProgrammer[] m_tbProgrammer;
        private TBListener[] m_tbListener;

        // used for programmer and listener theads
        private Thread [] thread_programmer;
        private Thread [] thread_listener;

        private Mutex m_mutUnfinishedJobsID;
        private TBExpJob[] m_QueuedJobs;
        private int m_expid_now;

        private System.Threading.Timer timer;
        private ManualResetEvent m_ListenLock = new ManualResetEvent(false);


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="f"> ref to MainForm </param>
        /// <param name="config"> ref to TBConfiguration </param>
        public TBManager(MainForm inMainForm,TBConfiguration inConfig)
        {
            m_mutIsStarted = new Mutex();
            m_IsStarted = false;
            m_refToMainForm = inMainForm;
            m_refToConfig = inConfig;
            ReconfigureNodes();

            m_mutUnfinishedJobsID = new Mutex();
        }

        /// <summary>
        /// Reset Configuration
        /// </summary>
        public void ReconfigureNodes()
        {
            m_StartNodeID = m_refToConfig.GetStartNodeID();
            m_NumberOfNodes = m_refToConfig.GetNumOfNodes();
            m_tbProgrammer = new TBProgrammer[m_NumberOfNodes];
            m_tbListener = new TBListener[m_NumberOfNodes];
            thread_programmer = new Thread[m_NumberOfNodes];
            thread_listener = new Thread[m_NumberOfNodes];
        }

        /// <summary>
        /// Start to handling jobs
        /// </summary>
        public void Start()
        {
            // set manager status to "start"
            m_IsStarted = true;

            // create TBDatabaseAgent instance
            m_tbDatabase = new TBMySqlDBAgent(m_refToConfig);
            UIWriteLogs("Initialize TBDatabase\r\n");

            // update node status to database
            m_tbDatabase.UpdateNodeStatus();
            UIWriteLogs("Update Nodes' Status to Database\r\n");
            
            for (; ;)
            {
                // check if start or stop working
                #region Critical Section for accessing m_IsStarted
                m_mutIsStarted.WaitOne();
                if (!m_IsStarted)
                {
                    m_mutIsStarted.ReleaseMutex();
                    break;
                }
                m_mutIsStarted.ReleaseMutex();
                #endregion

                m_refToMainForm.RemoveAllItemsOfQueue();

                // get the queued jobs
                try
                {
                    m_QueuedJobs = m_tbDatabase.GetQueuedJobs();
                    //UIWriteLogs("Got Queued Jobs (" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ")\r\n");
                }
                catch (Exception)
                {
                    //MessageBox.Show(ex.ToString());
                    UIWriteLogs("Error: Exception Occur on GetQueuedJobs()\r\n") ;
                    Thread.Sleep(ServerInfo.SLEEP_TIME * 1000);
                    continue;
                }

                // if exsit queue job(s)
                if (m_QueuedJobs.Length != 0)
                {
                    // insert item to user interface
                    UIInsertItemsOfQueue(m_QueuedJobs);
                    UIWriteLogs("Got Queued Jobs\r\n");

                    for (int i = 0; i < m_QueuedJobs.Length; ++i)
                    {
                        // check if start or stop working
                        #region Critical Section for accessing m_IsStarted
                        m_mutIsStarted.WaitOne();
                        if (!m_IsStarted)
                        {
                            m_mutIsStarted.ReleaseMutex();
                            break;
                        }
                        m_mutIsStarted.ReleaseMutex();
                        #endregion

                        // check if the job cancelled
                        #region 
                        m_mutUnfinishedJobsID.WaitOne();
                        if (m_QueuedJobs[i].IsCancel())
                        {
                            m_mutUnfinishedJobsID.ReleaseMutex();
                            UIRemoveTopItemOfQueue();
                            continue;
                        }
                        m_mutUnfinishedJobsID.ReleaseMutex();
                        #endregion 

                        UISelectTopItemOfQueue();

                        int exp_id = m_QueuedJobs[i].GetExpID();
                        UIWriteLogs("Case " + exp_id.ToString() + " :\r\n");

                        UIWriteLogs("      set status WORKING\r\n");
                        m_tbDatabase.SetJobStatus(exp_id, true);

                        UIWriteLogs("      Begin of HandleJob.\r\n");

                        if (HandleJob(m_QueuedJobs[i]))
                        {
                            UIWriteLogs("         No error occur on HandleJob().\r\n");
                        }
                        else
                        {
                            UIWriteLogs("         Some error occur on HandleJob().\r\n");
                        }
                        UIWriteLogs("      End of HandleJob.\r\n");

                        UIWriteLogs("      set status DONE\r\n");
                        m_tbDatabase.SetJobStatusDone(exp_id);
                        m_tbDatabase.SetJobStatus(exp_id, false);

                        UIWriteLogs("End Case " + exp_id.ToString() + ".\r\n");
                        UIRemoveTopItemOfQueue();

                        UIWriteLogs("Waiting for other regions (job " + exp_id.ToString() + ").\r\n");
                        WaitForJobCompleted(exp_id);
                        m_tbDatabase.DeleteExpTempFile(exp_id);
                        UIWriteLogs("------------------------------------------------------------\r\n");
                    }
                }
                Thread.Sleep(ServerInfo.SLEEP_TIME * 1000);
            }
            
        }

        /// <summary>
        /// Stop to handling jobs
        /// </summary>
        public void Stop()
        {
            // Begin of Critical Section for changing m_IsStarted to false
            #region
            m_mutIsStarted.WaitOne();
            m_IsStarted = false;
            m_mutIsStarted.ReleaseMutex();
            #endregion
            // End of Critical Section

            // Stop Listen threads
            // Begin of Critical Section for setting TBListener.IsListenDone to true
            #region
            TBListener.mutIsListenDone.WaitOne();
            TBListener.IsListenDone = true;
            TBListener.mutIsListenDone.ReleaseMutex();
            #endregion
            // End of Critical Section
            m_ListenLock.Set();
        }

        /// <summary>
        /// cancel a job
        /// </summary>
        /// <param name="inExpID"> ExpID (JobID)</param>
        public void CancelAJob(int inExpID)
        {
            for (int i = 1; i < m_QueuedJobs.Length; ++i)
            {
                m_mutUnfinishedJobsID.WaitOne();
                if ( m_QueuedJobs[i].GetExpID() == inExpID)
                {
                    m_QueuedJobs[i].Cancel();
                    m_mutUnfinishedJobsID.ReleaseMutex();
                    break;
                }
                m_mutUnfinishedJobsID.ReleaseMutex();
            }
            m_tbDatabase.SetJobStatusCanceled(inExpID);
        }

        /// <summary>
        /// Handle a job
        /// </summary>
        /// <param name="inJob">the job will be handle</param>
        /// <returns>True(Successful) or False(Exception Occur)</returns>
        private bool HandleJob(TBExpJob inJob)
        { 
            m_expid_now = inJob.GetExpID();

            // initialize and setup the programmer and listener
            try
            {
                TBProgrammer.SetMaximumProgrammingThread( m_refToConfig.GetMaxProgrammerThreads() );

                for (int j = 0; j < m_NumberOfNodes; ++j)
                {
                    int node_id = m_StartNodeID + j;
                    string filename = inJob.GetFileNameByNodeID(node_id);

                    // prepare the binary file
                    bool fileReady = m_tbDatabase.GetImageFileAndSaveInLocal(inJob.GetExpUser(), 
                                                                             filename, 
                                                                             ServerInfo.TEMP_DIRECTORY) ;
                    // prepare TBProgrammer
                    m_tbProgrammer[j] = new TBProgrammer(node_id, 
                                                         (fileReady? filename : "") , 
                                                         m_refToConfig.GetCOMPort(node_id)
                                                        );
                    // prepare TBListener
                    m_tbListener[j] = new TBListener(m_refToConfig.GetCOMPort(node_id),
                                                     (fileReady ? inJob.GetListenRequest(node_id) : false ) 
                                                    );
                }
            }
            catch (Exception ex)
            {
                UIWriteLogs("         Exception Occur on HandleAJob() : initialization\r\n");
                MessageBox.Show(ex.ToString());
                return false;
            }

            // start programmer processes
            Program();

            // start listener processes
            Listen(inJob.GetExpDuration());

            // write the listening result to database

            for (int j = 0; j < m_NumberOfNodes; ++j)
            {
                int node_id = m_StartNodeID + j;
                string log = m_tbListener[j].GetListenLogs();
                if (inJob.GetListenRequest(node_id) && log.Length > 0)
                {
                    try
                    {
                        m_tbDatabase.WriteUARTLogs(inJob.GetExpID(),
                                                   node_id,
                                                   log);
                    }            
                    catch (Exception)
                    {
                        UIWriteLogs("         Exception Occur on HandleAJob() : WriteResultData(" +
                                    inJob.GetExpID().ToString() + "\r\n");
                        continue;
                    }
                }
            }

            if (Directory.Exists(ServerInfo.TEMP_DIRECTORY))
                Directory.Delete(ServerInfo.TEMP_DIRECTORY, true);

            return true;
        }

        /// <summary>
        /// Wait until job done
        /// </summary>
        /// <param name="inExpID">the waiting job</param>
        private void WaitForJobCompleted(int inExpID)
        {
            for (; ; )
            {
                // if    (1) the manager is stop
                //    or (2) the job is done
                //    or (3) the job is canceled
                // then stop waiting
                if (!m_IsStarted)
                    return;
                if (m_tbDatabase.IsJobDone(inExpID))
                    return;
                if (m_tbDatabase.IsJobCanceled(inExpID))
                    return;
                Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// start programming (install code to node)
        /// </summary>
        /// <returns></returns>
        private bool Program()
        {
            UIWriteLogs("         Begin of Programming\r\n");
            try
            {
                for (int j = 0; j < m_NumberOfNodes; ++j)
                {
                    thread_programmer[j] = new Thread(new ThreadStart(m_tbProgrammer[j].Install));
                    thread_programmer[j].Start();
                }

                //ThreadPool.QueueUserWorkItem

                for (int j = 0; j < m_NumberOfNodes; ++j)
                {
                    thread_programmer[j].Join();
                    UIWriteLogs("         Programmer(" + (m_StartNodeID + j).ToString() + ") Done: " + m_tbProgrammer[j].GetResultLogs() + "\r\n");
                }
            }
            catch (Exception)
            {
                UIWriteLogs("         Exception Occur on Program()\r\n");
                return false;
            }
            UIWriteLogs("         End of Programming\r\n");
            return true;
        }
        
        /// <summary>
        /// start listening (listen UART logs)
        /// </summary>
        /// <param name="inExpDuration"> the duration time to listen</param>
        /// <returns></returns>
        private bool Listen(int inExpDuration)
        {
            // set TBListener.IsListenDone to false
            TBListener.IsListenDone = false;
            UIWriteLogs("         Begin of Listening\r\n");

            try
            {
                for (int j = 0; j < m_NumberOfNodes; ++j)
                {
                    thread_listener[j] = new Thread(new ThreadStart(m_tbListener[j].Listen));
                    thread_listener[j].Start();
                }
            }
            catch (Exception)
            {
                UIWriteLogs("         Exception Occur on Listen() : start threads\r\n");
                return false;
            }

            // Start Timer and block for running application of motes
            timer = new System.Threading.Timer(new TimerCallback(TimerFired));
            timer.Change(inExpDuration * 60 * 1000, 0);

            Thread watch_bog = new Thread(new ThreadStart(WatchDogForCancelBit));
            watch_bog.Start();

            m_ListenLock.Reset();
            m_ListenLock.WaitOne();   // wait here

            UIWriteLogs("         Timer is up\r\n");

            // Set TBListener.IsListenDone to true
            #region Critical Section for accessing TBListener.IsListenDone
            TBListener.mutIsListenDone.WaitOne();
            TBListener.IsListenDone = true;
            TBListener.mutIsListenDone.ReleaseMutex();
            #endregion

            watch_bog.Join();

            for (int j = 0; j < m_NumberOfNodes; ++j)
            {
                thread_listener[j].Join();
                UIWriteLogs("         Listener( " + (m_StartNodeID + j).ToString() + ") Done:" +
                             m_tbListener[j].GetResultLogs() + "\r\n");
            }

            UIWriteLogs("         End of Listening\r\n");
            return true;

        }

        /// <summary>
        /// CALL BACK function when the timer is fired
        /// </summary>
        /// <param name="state"></param>
        private void TimerFired(object state)
        {
            m_ListenLock.Set();
        }

        /// <summary>
        /// a watch dog to detect if the working job is canceled
        /// </summary>
        private void WatchDogForCancelBit()
        {
            for (; ; )
            {
                TBListener.mutIsListenDone.WaitOne();
                if (TBListener.IsListenDone == true)
                {
                    TBListener.mutIsListenDone.ReleaseMutex();
                    break;
                }
                TBListener.mutIsListenDone.ReleaseMutex();

                if (m_tbDatabase.IsJobCanceled( m_expid_now ))
                {
                    m_ListenLock.Set();
                    break;
                }
                Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// user interface control function
        /// (insert item to queue)
        /// </summary>
        /// <param name="jb"></param>
        private void UIInsertItemsOfQueue(TBExpJob [] jb)
        {
            for (int i = 0; i < jb.Length; ++i)
            {
                m_refToMainForm.AddWorkListItem(jb[i].GetExpID(), 
                                                jb[i].GetExpUser(), 
                                                jb[i].GetExpDuration(), 
                                                jb[i].GetExpPurpose() );
            }
        }

        /// <summary>
        /// user interface control function
        /// (write text to log)
        /// </summary>
        /// <param name="log"></param>
        private void UIWriteLogs(string log)
        {
            m_refToMainForm.WriteLog(log);
        }

        /// <summary>
        /// user interface control function
        /// (select top item of queue)
        /// </summary>
        private void UISelectTopItemOfQueue()
        {
            m_refToMainForm.SelectTopItemOfQueue();
        }

        /// <summary>
        /// user interface control function
        /// (remove top item of queue)
        /// </summary>
        private void UIRemoveTopItemOfQueue()
        {
            m_refToMainForm.RemoveTopItemOfQueue();
        }
    }
}
