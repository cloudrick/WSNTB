using System;
using System.Collections.Generic;
using System.Text;

namespace WSNTBS
{
    class TBExpJob
    {
        // data member
        private int    m_ExpID;          // experiment id
        private string m_ExpUser;        // experiment user name (who)
        private int    m_ExpDuration;    // experiment duration  (time)
        private string m_ExpPurpose;     // experiment purpose   (why to make the job)
        private string m_ExpListen;      // listen request mapping
        private bool   m_IsCancel;

        private int    m_StartNodeID;    // the start node id
        private int    m_NumOfNodes;     // number of nodes 

        private string[] m_ExpFilename ; // To record image filename 
        // for example:
        // The file "Blink.ihex" will be installed to node i
        // I'll assign "Blink.ihex" to exp_filename[i-1] (exp_filename[i-1] = "Blink.ihex" ; )
        // and remember the index must minus 1


        // constructor
        public TBExpJob(int inID,string inUuser,int inDuration,string inPurpose,
                        int inStartNode,int inNumOfNodes,string inListen)
        {
            m_StartNodeID = inStartNode;
            m_NumOfNodes = inNumOfNodes;
            Initialize(inID, inUuser, inDuration, inPurpose, inListen);
        }

        // initialize data member
        protected void Initialize(int inID, string inUuser, int inDuration,
                                  string inPurpose, string inListen)
        {
            m_ExpID = inID;
            m_ExpUser = inUuser;
            m_ExpDuration = inDuration;
            m_ExpPurpose = inPurpose;
            m_ExpListen = inListen;
            m_IsCancel = false;
            m_ExpFilename = new string[m_NumOfNodes];

            for (int i = 0; i < m_ExpFilename.Length; ++i)
            {
                m_ExpFilename[i] = "";
            }
        }

        /// <summary>
        /// Get Experiment ID
        /// </summary>
        /// <returns> ExpID </returns>
        public int GetExpID()
        {
            return m_ExpID;
        }

        public string GetExpUser()
        {
            return m_ExpUser;
        }

        public int GetExpDuration()
        {
            return m_ExpDuration;
        }

        public string GetExpPurpose()
        {
            return m_ExpPurpose;
        }

        public bool IsCancel()
        {
            return m_IsCancel;
        }

        public void Cancel()
        {
            m_IsCancel = true;
        }

        // get filename by node id
        public string GetFileNameByNodeID(int inID)
        {
            int idx = inID - m_StartNodeID;
            if (idx >= 0 && idx < m_NumOfNodes)
            {
                if (m_ExpFilename[idx].EndsWith(".exe"))
                    return m_ExpFilename[idx] + ".ihex.out-" + inID.ToString();
                else
                    return m_ExpFilename[idx];
            }
            return "";
        }

        /// <summary>
        /// set filename by node id
        /// </summary>
        /// <param name="inNodeID"> Node ID</param>
        /// <param name="inFilename"> File Name</param>
        public void SetFileNameByNodeID(int inNodeID, string inFilename)
        {
            int idx = inNodeID - m_StartNodeID;
            if (idx >= 0 && idx < m_NumOfNodes)
            {
                m_ExpFilename[idx] = inFilename;
            }
        }

        // get is listen request by node id
        public bool GetListenRequest(int inNodeID)
        {
            if (inNodeID <= 0 || inNodeID > m_ExpListen.Length)
                return false;
            else
                return !(m_ExpListen[inNodeID-1] == '0');
        }
    }
}
