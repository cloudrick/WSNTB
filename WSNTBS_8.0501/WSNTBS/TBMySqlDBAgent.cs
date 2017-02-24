using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using MySql.Data.Types;
using MySql.Data.MySqlClient;

namespace WSNTBS
{
    class TBMySqlDBAgent : TBDatabaseAgent
    {
        private MySqlConnection m_MySqlConnection;
        private TBConfiguration m_refToConfig;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">reference to Testbed Configuration</param>
        public TBMySqlDBAgent(TBConfiguration inConfig)
        {
            m_MySqlConnection = new MySqlConnection();
            m_refToConfig = inConfig;

            m_MySqlConnection.ConnectionString = 
                "Persist Security Info=False;database=" + m_refToConfig.GetDatabase()
                + ";server=" + m_refToConfig.GetDatabaseHost()
                + ";Connect Timeout=10;user id=" + m_refToConfig.GetDatabaseUsername()
                + "; pwd=" + m_refToConfig.GetDatabasePassword();
        }

        /// <summary>
        /// Get Queued Jobs
        /// </summary>
        /// <returns></returns>
        public override TBExpJob[] GetQueuedJobs()
        {
            m_MySqlConnection.Open();
            MySqlDataReader QueryResult;

            QueryResult = GetQueryResult("SELECT * FROM exp_info WHERE (`is_" + 
                          m_refToConfig.GetRegionText() +
                          "_done` = 0 AND `is_cancel` = 0) ORDER BY `exp_id` ASC");

            if (QueryResult == null )
            {
                m_MySqlConnection.Close();
                return new TBExpJob[] { };
            }

            Queue<TBExpJob> q = new Queue<TBExpJob>();

            while ( QueryResult.Read() )
            {
                TBExpJob tb = new TBExpJob(QueryResult.GetInt32("exp_id"),
                                            QueryResult.GetString("owner"),
                                            QueryResult.GetInt32("duration"),
                                            QueryResult.GetString("description"),
                                            m_refToConfig.GetStartNodeID(),
                                            m_refToConfig.GetNumOfNodes(),
                                            QueryResult.GetString("listen_bits"));

                // use new connection to get file-node map
                MySqlConnection tmpConnection = new MySqlConnection();
                tmpConnection.ConnectionString = m_MySqlConnection.ConnectionString +
                                                 "; pwd=" + m_refToConfig.GetDatabasePassword();

                tmpConnection.Open();

                MySqlCommand tmpCommand = new MySqlCommand();
                tmpCommand.Connection = tmpConnection;
                tmpCommand.CommandText = "SELECT node_num ,filename FROM exp_node_file WHERE `exp_id` = " +
                                          tb.GetExpID().ToString();

                MySqlDataReader NodeFileQueryResult = tmpCommand.ExecuteReader();

                if (NodeFileQueryResult != null)
                {
                    while (NodeFileQueryResult.Read())
                    {
                        tb.SetFileNameByNodeID(NodeFileQueryResult.GetInt32("node_num"),
                                               NodeFileQueryResult.GetString("filename"));

                    }
                    NodeFileQueryResult.Close();
                }
                tmpConnection.Close();
                q.Enqueue(tb);
            }

            QueryResult.Close();
            m_MySqlConnection.Close();
            return q.ToArray();
        }

        // 
        /// <summary>
        /// Get File and Save in Local Directory
        /// </summary>
        /// <param name="inExpUser"></param>
        /// <param name="inFilename"></param>
        /// <param name="inLocalDirectory"></param>
        /// <returns></returns>
        public override bool GetImageFileAndSaveInLocal(string inExpUser, 
                                                        string inFilename,
                                                        string inLocalDirectory)
        {
            // max size (KB) of binary file
            const int MAX_IMAGE_SIZE = 128;

            // if no file specified
            if (inFilename == "")
            {
                return true;
            }

            // if file already existed
            if( File.Exists( inLocalDirectory + "\\" + inFilename) )
            {
                return true;
            }

            // create the directory if it does not existed
            if (!Directory.Exists(inLocalDirectory))
            {
                Directory.CreateDirectory(inLocalDirectory);
            }

            m_MySqlConnection.Open();

            MySqlDataReader QueryResult;

            // we FIRST get from exp_tmp_file (user submit .exe file)
            QueryResult = GetQueryResult("SELECT content FROM exp_tmp_file WHERE `owner` = '" 
                                        + inExpUser + "' AND " + "`filename` = '" + inFilename + "' ");

            // if failed, get from exp_file again (user sublit .ihex file)
            if (QueryResult == null || !QueryResult.Read() )
            {
                if (QueryResult != null)
                    QueryResult.Close();
                      
                QueryResult = GetQueryResult("SELECT content FROM exp_file WHERE `owner` = '"
                                               + inExpUser + "' AND " + "`filename` = '" + inFilename + "' ");

                // if cound not find file
                if (QueryResult == null || !QueryResult.Read())
                {
                    if (QueryResult != null)
                        QueryResult.Close();
                    m_MySqlConnection.Close();
                    return false;
                }
            }

            // if get the file succussfully

            FileStream fs = new FileStream(inLocalDirectory + "\\temp.temp", FileMode.CreateNew);
            BinaryWriter bw = new BinaryWriter(fs);

            byte[] buff = new byte[MAX_IMAGE_SIZE * 1024];

            int read_bytes = (int)QueryResult.GetBytes(0, 0, buff, 0, MAX_IMAGE_SIZE * 1024);

            for (int i = 0; i < read_bytes; ++i)
            {
                bw.Write(buff[i]);
            }

            bw.Close();
            fs.Close();

            QueryResult.Close();
            m_MySqlConnection.Close();

            bool res = WriteTOSBootCode(inLocalDirectory + "\\temp.temp",
                                        inLocalDirectory + "\\" + inFilename);

            if (File.Exists(inLocalDirectory + "\\temp.temp"))
                File.Delete(inLocalDirectory + "\\temp.temp");

            return res;

        }

        /// <summary>
        /// Write UART Logs to Database
        /// </summary>
        /// <param name="inExpID"></param>
        /// <param name="inNodeID"></param>
        /// <param name="inLogs"></param>
        public override void WriteUARTLogs(int inExpID, int inNodeID, string inLogs)
        {
            m_MySqlConnection.Open();
            GetQueryResult("INSERT INTO exp_log (`exp_id` , `node_num` , `rawdata`) VALUES(" +
                            inExpID.ToString() + "," + 
                            inNodeID.ToString() + ",'" +
                            inLogs + "') ").Close();
            m_MySqlConnection.Close();
        }


        /// <summary>
        /// Set Job Status Done
        /// </summary>
        /// <param name="inExpID"> ExpID </param>
        public override void SetJobStatusDone(int inExpID)
        {
            m_MySqlConnection.Open();
            GetQueryResult("BEGIN WORK").Close();

            GetQueryResult(
                           "UPDATE exp_info SET `is_" + 
                           m_refToConfig.GetRegionText() +  
                           "_done` = 1 WHERE `exp_id` = " 
                           + inExpID.ToString() 
                          ).Close();

            GetQueryResult("COMMIT WORK").Close();
            m_MySqlConnection.Close();
        }

        /// <summary>
        /// Set Job Status
        /// </summary>
        /// <param name="inExpID"></param>
        /// <param name="inWorking"></param>
        public override void SetJobStatus(int inExpID,bool inWorking)
        {
            string working = inWorking ? "1" : "0";

            m_MySqlConnection.Open();
            GetQueryResult("BEGIN WORK").Close();

            GetQueryResult("UPDATE exp_info SET `is_" +
                            m_refToConfig.GetRegionText() +
                           "_working` = " + working +
                           " WHERE `exp_id` = " +
                            inExpID.ToString()
                          ).Close();

            GetQueryResult("COMMIT WORK").Close();
            m_MySqlConnection.Close();
        }

        /// <summary>
        /// Set Job Status Canceled
        /// </summary>
        /// <param name="inExpID"></param>
        public override void SetJobStatusCanceled(int inExpID)
        {
            m_MySqlConnection.Open();
            GetQueryResult("BEGIN WORK").Close();
            GetQueryResult("UPDATE exp_info SET `is_cancel` = 1 WHERE `exp_id` = " + inExpID.ToString()).Close();
            GetQueryResult("COMMIT WORK").Close();
            m_MySqlConnection.Close();
        }

        /// <summary>
        /// return TRUE if canceled , FALSE otherwise
        /// </summary>
        /// <param name="inExpID"> ExpID </param>
        /// <returns> true or false</returns>
        public override bool IsJobCanceled(int inExpID)
        {
            m_MySqlConnection.Open();
            MySqlDataReader QueryResult = GetQueryResult("SELECT is_cancel FROM exp_info WHERE `exp_id` = '" + inExpID.ToString() + "'");

            if (QueryResult == null)
            {
                m_MySqlConnection.Close();
                return false;
            }

            int res = 0;
            if (QueryResult.Read())
            {
                res = QueryResult.GetInt32("is_cancel");
            }
            QueryResult.Close();
            m_MySqlConnection.Close();

            return res == 1 ;
        }

        /// <summary>
        /// return TRUE if done , FALSE otherwise
        /// </summary>
        /// <param name="inExpID"></param>
        /// <returns> true or false </returns>
        public override bool IsJobDone(int inExpID)
        {
            m_MySqlConnection.Open();
            MySqlDataReader QueryResult = GetQueryResult("SELECT `is_ncu1_done`, `is_ncu2_done`, `is_nthu1_done` FROM exp_info WHERE `exp_id` = '" + inExpID.ToString() + "'");

            if (QueryResult == null)
            {
                m_MySqlConnection.Close();
                return false;
            }

            bool res = false;

            if (QueryResult.Read())
            {
                res = ( QueryResult.GetBoolean("is_ncu1_done") && 
                        QueryResult.GetBoolean("is_ncu2_done") && 
                        QueryResult.GetBoolean("is_nthu1_done")    );
            }

            QueryResult.Close();
            m_MySqlConnection.Close();

            return res;
        }

        /// <summary>
        /// Update Node Status
        /// </summary>
        public override void UpdateNodeStatus()
        {
            m_MySqlConnection.Open();

            for (int i = 0; i < m_refToConfig.GetNumOfNodes(); ++i)
            {
                int node_id = m_refToConfig.GetStartNodeID() + i;

                GetQueryResult("BEGIN WORK").Close();

                string sql = "UPDATE exp_node_status SET " ;
                if (m_refToConfig.GetCOMPort(node_id) == "")
                    sql += "`is_valid` = 0 ";
                else
                    sql += "`is_valid` = 1 ";
                sql += " WHERE `node_num` = " + (node_id).ToString();

                GetQueryResult(sql).Close();
                GetQueryResult("COMMIT WORK").Close();
            }
            
            m_MySqlConnection.Close();
        }

        /// <summary>
        /// Delete temp file
        /// </summary>
        /// <param name="inExpID"> ExpID </param>
        public override void DeleteExpTempFile(int inExpID)
        {
            m_MySqlConnection.Open();
            GetQueryResult("DELETE FROM `exp_tmp_file` WHERE `exp_id`=" + inExpID.ToString() ).Close();
            m_MySqlConnection.Close();
        }


        /// <summary>
        /// Get Query Result
        /// </summary>
        /// <param name="inQueryString">Query String</param>
        /// <returns>Query Result</returns>
        private MySqlDataReader GetQueryResult(string inQueryString)
        {
            try
            {
                MySqlCommand Command = new MySqlCommand();
                Command.Connection = m_MySqlConnection;
                Command.CommandText = inQueryString;
                return Command.ExecuteReader();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// insert TOSBoot Code to the begin of a ihex file
        /// </summary>
        /// <param name="inSourceFile">Source File</param>
        /// <param name="inDestinationFile">Destination File</param>
        /// <returns></returns>
        private bool WriteTOSBootCode(string inSourceFile, string inDestinationFile)
        {
            if (!File.Exists(inSourceFile))
            {
                return false;
            }

            StreamReader sr = new StreamReader(inSourceFile);
            StreamWriter sw = new StreamWriter(inDestinationFile);

            // TOSBoot binary code
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(":10400000B240805A20013F4046473E4000113D40AB");
            sb.AppendLine(":1040100000110D9E0524FE4F00001E530E9DFB2B2C");
            sb.AppendLine(":104020003F4000113D4000110D9F0524CF4300008B");
            sb.AppendLine(":104030001F530F9DFB2B3040404030403E4000134B");
            sb.AppendLine(":104040003140FE3832C20343B012CA40F24087FF0B");
            sb.AppendLine(":104050005700F240E0FF5600F2D080FF1E00F2D081");
            sb.AppendLine(":1040600010001E00F2D080FF1D00F240170070000B");
            sb.AppendLine(":10407000F2D0A2FF7100E2437400C2437500F2D097");
            sb.AppendLine(":1040800040000400D2C37000C24302000E433F430D");
            sb.AppendLine(":104090003F53FE231E532E92FA3BC29328000F3843");
            sb.AppendLine(":1040A000B012CA40B0120E414E437F400700B0121A");
            sb.AppendLine(":1040B0004041B140F0000000B1D2000022D1B01266");
            sb.AppendLine(":1040C00030460F43215330401E47F2400E001B0084");
            sb.AppendLine(":1040D000F240E0FF2200C2432100F2407B002A00B0");
            sb.AppendLine(":1040E000F24010002900F240F1FF1A00C24319000B");
            sb.AppendLine(":1040F000F240FDFF1E00F240DDFF1D00F2433200E2");
            sb.AppendLine(":10410000F2433100F2433600C24335003041F2F051");
            sb.AppendLine(":10411000EFFF1D00F240B9FF77005F4271005FF3CF");
            sb.AppendLine(":104120007FF34F93FA27F2D010001D00C243040022");
            sb.AppendLine(":10413000D2437000D2437100C24372001F4330412A");
            sb.AppendLine(":104140000B120A120912494F4A4E3B4000064F49D2");
            sb.AppendLine(":10415000B0127E410F4B0B9302243F53FE234F4A74");
            sb.AppendLine(":10416000B0127E413F4000060F8B02243F53FE23D6");
            sb.AppendLine(":104170002B821B93EC3739413A413B4130414E4F42");
            sb.AppendLine(":104180006FF30424F2F0DFFF3100033CF2D0200093");
            sb.AppendLine(":1041900031004F4E6FF20424F2F0BFFF3100033CB8");
            sb.AppendLine(":1041A000F2D0400031004F4E5FF30424F2F0EFFFF5");
            sb.AppendLine(":1041B00031003041F2D01000310030410B120A12B0");
            sb.AppendLine(":1041C0004A4F7B4003004F4AB0127E416E423F434C");
            sb.AppendLine(":1041D0003F53FE237E53FB234F43B0127E416E427A");
            sb.AppendLine(":1041E0003F433F53FE237E53FB237B53EC233A4153");
            sb.AppendLine(":1041F0003B4130410B120A127F4007007A40030016");
            sb.AppendLine(":104200004B4F12C34B104E4BB01240417A534F4BA1");
            sb.AppendLine(":10421000F7233A413B413041B0120E414F93022403");
            sb.AppendLine(":104220001F43013C0F437FF330415E427F105F42EA");
            sb.AppendLine(":10423000FF107E9302201F4330417F9302200F43E3");
            sb.AppendLine(":1042400030410D434E8F7EB0800001341D430F4D31");
            sb.AppendLine(":1042500030410B120A12091208120712061231809D");
            sb.AppendLine(":1042600000020A4E0B4FB01240454F931A24084AE1");
            sb.AppendLine(":10427000094B3850900109630E480F49B0124A4368");
            sb.AppendLine(":10428000B012FA43064FB012FA43074F084A094BDF");
            sb.AppendLine(":10429000385098010963369000480524F2D0100088");
            sb.AppendLine(":1042A0001D001E43483C0F9345240F468F107FF39B");
            sb.AppendLine(":1042B0000A4F12C30A100E480F49B0124A430F4664");
            sb.AppendLine(":1042C0003FF0FF010B410B5FB012CE43CB4F00001C");
            sb.AppendLine(":1042D00016531853096337530820B012FA43064F98");
            sb.AppendLine(":1042E000B012FA43074F385209630F468F107FF31D");
            sb.AppendLine(":1042F00012C30F100A9F02200793E123F2D010008F");
            sb.AppendLine(":104300001D004F4AB0127E410E4A0F430A4E0B4F1A");
            sb.AppendLine(":104310003C4000020D43B01222470B4E0C4F3D4073");
            sb.AppendLine(":1043200000020E410F4BB012CA442E434F93032498");
            sb.AppendLine(":104330000793BB230E430F4E3150000236413741E5");
            sb.AppendLine(":10434000384139413A413B4130410B120A120912BE");
            sb.AppendLine(":10435000094E0A4FF2F0EFFF1D004B43F240ABFF56");
            sb.AppendLine(":104360007700B012BA434F93FC275B537B90050054");
            sb.AppendLine(":10437000F52BF2D010001D00F2F0EFFF1D003AD037");
            sb.AppendLine(":1043800000036B424F4B0F5F0F5F0F5F0D4F3D827E");
            sb.AppendLine(":104390000E490F4A0D93052412C30F100E101D83F2");
            sb.AppendLine(":1043A000FB23C24E7700B012BA434F93FC277B53D6");
            sb.AppendLine(":1043B000E92339413A413B413041C293020005347F");
            sb.AppendLine(":1043C000F2F07F0002001F4330410F4330415F4253");
            sb.AppendLine(":1043D0007600C24377005F4202007FF04000052470");
            sb.AppendLine(":1043E000F2F0BFFF02001F43013C0F437FF34F93E6");
            sb.AppendLine(":1043F000F2275F4276007FF330410B120A120A4324");
            sb.AppendLine(":104400004B43B012CE434E4F4F4B0F5F0F5F0D4FDC");
            sb.AppendLine(":104410000D5D0F4E0D9303240F5F1D83FD230ADFF7");
            sb.AppendLine(":104420005B536B92EE2B0F4A3A413B4130410B12EA");
            sb.AppendLine(":104430000A12091208120712094F074D084E395087");
            sb.AppendLine(":1044400000103A4000103B400010B0122A424F9337");
            sb.AppendLine(":1044500003243A408010043C395080003B508000D7");
            sb.AppendLine(":10446000B24084A52A01B24000A52C01B24002A5A9");
            sb.AppendLine(":104470002801CB430000B24040A528010F430D495D");
            sb.AppendLine(":104480000D570B9902280B9D0328EB4A0000033CB3");
            sb.AppendLine(":10449000EB48000018531F531B531A533F907F00E3");
            sb.AppendLine(":1044A000F02B6F4A5F537F9301204F43CB4F0000A7");
            sb.AppendLine(":1044B000B24000A52801B24010A52C011F4337418E");
            sb.AppendLine(":1044C000384139413A413B4130410B120A120B4D00");
            sb.AppendLine(":1044D0000C4F0A4E0E4312C30D100F4D3FE30C9FBD");
            sb.AppendLine(":1044E0002B2CB24084A52A01B24000A52C01B24079");
            sb.AppendLine(":1044F00002A528018C430000B24040A528010E9D72");
            sb.AppendLine(":10450000132C0D4B12C30D103C90FEFF06240F4ED2");
            sb.AppendLine(":104510000F5F0F5AAC4F0000033CBC40004000004E");
            sb.AppendLine(":104520001E532C530E9DF02BB24000A52801B24023");
            sb.AppendLine(":1045300010A52C011F43013C0F433A413B41304140");
            sb.AppendLine(":104540000B120A120912081207120612051204129F");
            sb.AppendLine(":10455000084E094F044305433E5007000F63B01255");
            sb.AppendLine(":104560004A43B012CE43474FF2D010001D004F9384");
            sb.AppendLine(":1045700002247F9302200F43523C385010000963FD");
            sb.AppendLine(":104580003A4000013B4050034643469F442C04956B");
            sb.AppendLine(":1045900042204F460F5F0E4F0F430E580F69B01267");
            sb.AppendLine(":1045A0004A43B012CE43444FB012CE437FF38F1034");
            sb.AppendLine(":1045B00004DFF2D010001D000E4A0F430E580F69A1");
            sb.AppendLine(":1045C000B0124A4305430B931224B012CE437FF33B");
            sb.AppendLine(":1045D0008F100FE57E420F9304340F5F3FE02110F0");
            sb.AppendLine(":1045E000013C0F5F7E53F723054F3B53EE23F2D080");
            sb.AppendLine(":1045F00010001D004F461F530A4F3C4050040B4310");
            sb.AppendLine(":104600000D43B01242470A4E3B4050045653469762");
            sb.AppendLine(":10461000022C0495BE270F43469701201F433441C7");
            sb.AppendLine(":10462000354136413741384139413A413B413041CA");
            sb.AppendLine(":104630000B1231800600B2403002A001B2400002ED");
            sb.AppendLine(":10464000A201F2409BFF80000F431F533F900036B2");
            sb.AppendLine(":10465000FC3BA2D3A00192D3A0011F42A4011FF3EF");
            sb.AppendLine(":10466000FC27A2C3A0018243A0010F43B290670EB2");
            sb.AppendLine(":10467000400101281F437FF34F9306205F43B01290");
            sb.AppendLine(":10468000BC41B012F441433C3B407010B0122A428E");
            sb.AppendLine(":104690004F9302243B40F0100D410E4B3F4006006B");
            sb.AppendLine(":1046A0003F533F930724ED4E00001E531D533F53CD");
            sb.AppendLine(":1046B0003F93F923D1530400E193040008287F407D");
            sb.AppendLine(":1046C0000700B012BC410E433F400F000D3C3D407F");
            sb.AppendLine(":1046D00006000E413F407000B0122E44C193050009");
            sb.AppendLine(":1046E00009202E411F410200B01252426F93022056");
            sb.AppendLine(":1046F00082432001B012F441F1430400D14305008C");
            sb.AppendLine(":104700003D4006000E413F407000B0122E44B012F2");
            sb.AppendLine(":104710001842B0120048315006003B41304102DFE0");
            sb.AppendLine(":10472000FE3F0E430F43083C12C30D100C1002282D");
            sb.AppendLine(":104730000E5A0F6B0A5A0B6B0C93F6230D93F4234E");
            sb.AppendLine(":0647400030413040224729");
            sb.AppendLine(":0400000300004000B9");

            // read first line to check the "address" 
            // if the address is less than 0x4800 (may be TinyOS-1.x code)
            // than write TOSBoot code first


            string first_line = sr.ReadLine();

            if (first_line == null || first_line == "")
                return false;
            try
            {
                if (Convert.ToInt32(first_line.Substring(3, 4), 16) >= 0x4800)
                {
                    sw.Write(sb.ToString());
                    sw.WriteLine(first_line);
                }
            }
            catch (Exception)
            {
                sr.Close();
                sw.Close();
                return false;
            }

            for (; ; )
            {
                string line = sr.ReadLine();

                if (line == null )
                {
                    break;
                }         

                sw.WriteLine(line);
            }

            sr.Close();
            sw.Close();
            return true;
        }
    }
}
