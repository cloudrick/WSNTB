using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WSNTBS
{
    public abstract class TBConfiguration
    {
        /// <summary>
        /// Serial Port Mapping Table
        /// </summary>
        private string[] ProtNumber = new string[ServerInfo.MAX_NUM_OF_NODE];

        /// <summary>
        /// Node Configuration
        /// m_StartNodeID : the NodeID of start node at this region
        /// m_NumOfNodes  : the number of nodes at this region
        /// </summary>
        private int m_StartNodeID;
        private int m_NumOfNodes;

        /// <summary>
        /// Maximum threads for TBProgrammer in TBManager
        /// </summary>
        private int m_MaxProgThread;

        /// <summary>
        /// Database configuration
        /// </summary>
        private string m_DBHost;
        private string m_DBPort;
        private string m_DBDatabase;
        private string m_DBUsername;
        private string m_DBPassword;

        /// <summary>
        /// Region configuration
        /// </summary>
        private enum Region
        {
            eNOSPECIFY,
            eNCU1, 
            eNCU2,
            eNTHU1
        };
        private Region m_Location;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public TBConfiguration()
        {
            LoadDefaultConfigure();
        }

        /// <summary>
        /// Load Default Configuration
        /// </summary>
        protected void LoadDefaultConfigure()
        {
            for (int i = 0; i < ServerInfo.MAX_NUM_OF_NODE; ++i)
            {
                ProtNumber[i] = "";
            }

            m_StartNodeID = 1;
            m_NumOfNodes = 0;
            m_MaxProgThread = 1;
            m_Location = Region.eNOSPECIFY;
            m_DBHost = "sample.com";
            m_DBPort = "3306";
            m_DBDatabase = "testbed";
            m_DBUsername = "user";
            m_DBPassword = "password";
        }

        //  Accessors of m_StartNodeID
        public int GetStartNodeID()
        {
            return m_StartNodeID;
        }
        public void SetStartNodeID(int inStartNodeID)
        {
            m_StartNodeID = inStartNodeID;
        }

        //  Accessors of m_NumOfNodes
        public int GetNumOfNodes()
        {
            return m_NumOfNodes;
        }
        public void SetNumOfNodes(int inNumOfNodes)
        {
            m_NumOfNodes = inNumOfNodes;
        }

        //  Accessors of m_MaxProgThread
        public int GetMaxProgrammerThreads()
        {
            return m_MaxProgThread;
        }
        public void SetMaxProgrammerThreads(int inMaxProgThread)
        {
            m_MaxProgThread = inMaxProgThread;
        }

        //  Accessors of m_DBHost
        public string GetDatabaseHost()
        {
            return m_DBHost;
        }
        public void SetDatabaseHost(string inHost)
        {
            m_DBHost = inHost;
        }

        //  Accessors of m_DBPort
        public int GetDatabasePort()
        {
            return int.Parse(m_DBPort);
        }
        public void SetDatabasePort(int inPort)
        {
            m_DBPort = inPort.ToString() ;
        }

        //  Accessors of m_DBDatabase
        public string GetDatabase()
        {
            return m_DBDatabase ;
        }
        public void SetDatabase(string inDatabase)
        {
            m_DBDatabase = inDatabase;
        }

        //  Accessors of m_DBUsername
        public string GetDatabaseUsername()
        {
            return m_DBUsername;
        }
        public void SetDatabaseUsername(string inUsername)
        {
            m_DBUsername = inUsername;
        }

        //  Accessors of m_DBPassword
        public string GetDatabasePassword()
        {
            return m_DBPassword;
        }
        public void SetDatabasePassword(string inPassword)
        {
            m_DBPassword = inPassword;
        }


        /// <summary>
        /// Set Database Configuration
        /// </summary>
        /// <param name="inHost"> host</param>
        /// <param name="inPort"> port </param>
        /// <param name="inDatabase"> database name </param>
        /// <param name="inUsername"> database username </param>
        /// <param name="inPassword"> database password </param>
        public void SetDatabaseConfig(string inHost, int inPort, string inDatabase, 
                                      string inUsername, string inPassword)
        {
            m_DBHost = inHost ;
            m_DBPort = inPort.ToString() ;
            m_DBDatabase = inDatabase ;
            m_DBUsername = inUsername ;
            m_DBPassword = inPassword ;
        }

        /// <summary>
        /// Get Region Text (used in database)
        /// </summary>
        /// <returns>Region Text</returns>
        public string GetRegionText()
        {
            return GetRegionText(true);
        }

        /// <summary>
        /// Get Region Text (used in database)
        /// </summary>
        /// <param name="inIsLowcase"> lowcase(true) or upcase(false)</param>
        /// <returns>Region Text</returns>
        public string GetRegionText(bool inIsLowcase)
        {
            switch (m_Location)
            {
                case Region.eNCU1 :
                    return inIsLowcase ? "ncu1" : "NCU1";
                case Region.eNCU2 :
                    return inIsLowcase ? "ncu2" : "NCU2";
                case Region.eNTHU1:
                    return inIsLowcase ? "nthu1" : "NTHU1";
                default :
                    return "";
            }
        }

        /// <summary>
        /// Set the Region of the region
        /// </summary>
        /// <param name="inRegionText"> region text from configuration file</param>
        public void SetRegion(string inRegionText)
        {
            switch (inRegionText)
            {
                case "NCU1":
                case "ncu1":
                    m_Location = Region.eNCU1;
                    break;
                case "NCU2":
                case "ncu2":
                    m_Location = Region.eNCU2;
                    break;
                case "NTHU1":
                case "nthu1":
                    m_Location = Region.eNTHU1;
                    break;
                default:
                    m_Location = Region.eNOSPECIFY;
                    break;
            }
        }


        /// <summary>
        /// Get COMPort Text by Node ID
        /// </summary>
        /// <param name="inNodeID"> Node ID</param>
        /// <returns>COMPort Text</returns>
        public string GetCOMPort(int inNodeID)
        {
            if (inNodeID >= 1)
            {
                if (ProtNumber[inNodeID - 1] != "")
                    return "COM" + ProtNumber[inNodeID - 1];
            }
            return "";
        }

        /// <summary>
        /// Set COMPort Text
        /// </summary>
        /// <param name="inNodeID"> Node ID</param>
        /// <param name="inNumberOfPort"> Number of Port</param>
        public void SetCOMPort(int inNodeID, int inNumberOfPort)
        {
            if (inNodeID >= 1)
            {
                if (inNumberOfPort <= 0)
                    ProtNumber[inNodeID - 1] = "";
                else
                    ProtNumber[inNodeID - 1] = inNumberOfPort.ToString();
            }
        }

        // Abstract Member Functions
        public abstract void LoadConfigure();
        public abstract void SaveConfigure();
    }
}
