using System;
using System.Collections.Generic;
using System.Text;

namespace WSNTBS
{
    static class ServerInfo
    {
        //-------------- server configuration ---------------//
        // sleep time (seconds) between each round
        public static readonly int SLEEP_TIME = 5;
        public static readonly int MAX_NUM_OF_NODE = 100;
        public static string TEMP_DIRECTORY = "temp//";
    }

}
