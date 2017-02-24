
using System;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Drawing ;
using System.Drawing.Imaging;

namespace ACCClient
{
    public partial class Form1 : Form
    {
        private Socket socket ;
        private bool b_connected = false ;

        private const string ACC_DISCONNECT = "ACC_DISCONNECT\0" ;
        private const string ACC_FORWARD    = "ACC_FORWARD\0" ;
        private const string ACC_LEFT       = "ACC_LEFT\0";
        private const string ACC_RIGHT      = "ACC_RIGHT\0" ;
        private const string ACC_STOP       = "ACC_STOP\0" ;

        // Test only
        private static string prev_row12;
        private static string prev_row13;

        // used for thread
        private Thread thread_data_receiver ;
        private Thread thread_image_receiver;
        private bool thread_done ;
        private static Mutex mut = new Mutex();

        // used for delegate
        public delegate void WriteLogDelegate(String s);
        public delegate void UpdateGridDataDelegate(Object [] s);
        public delegate Graphics GetPicureBoxHwndDelegate(IntPtr p);


        public void ToWriteToGridData(Object [] s)
        {
            if (dataGridView1.InvokeRequired)
            {
                UpdateGridDataDelegate d = new UpdateGridDataDelegate(ToWriteToGridData);
                this.Invoke(d, new object [] {s}  );               
            }
            else
            {
                dataGridView1.Rows.Clear();
                dataGridView1.Rows.Add(s);
            }
        }
        //public 

        private void Thread_Image_Receiver()
        {
            for (; ; )
            {
               // pictureBox1.Load("http://192.168.1.10/cgi-bin/w3cam.cgi");
            }
        }

        // a thread for receive data from gateway
        private void Thread_Data_Receiver()
        {
            
            byte[] buffer = new byte[256];
            int buffer_length;
            //Graphics g;
            /*
            if (pictureBox1.InvokeRequired)
            {
                GetPicureBoxHwndDelegate d = new GetPicureBoxHwndDelegate( Graphics.FromHwnd );
                g = (Graphics)this.Invoke( d , pictureBox1.Handle );
            }
            else
            {
                g = Graphics.FromHwnd(pictureBox1.Handle);
            }
            
            Pen pen = new Pen( Color.Black ) ;
            */
            for (;;)
            {
                #region Critical Section
                // begin of critical section
                mut.WaitOne();
                if (thread_done)
                {
                    mut.ReleaseMutex();
                    break;
                }
                mut.ReleaseMutex();
                // end of critical section
                #endregion

                buffer_length = 0;

                #region get one of the received data
                for (; ; )
                {
                    byte [] ch = new byte[1] ;
                    if (socket.Receive(ch) == 1)
                    {    
                        if ( (ch[0] == 0x7e || ch[0] == 0x7a) && buffer_length !=0 )
                        {
                            break;
                        }
                        buffer[ buffer_length++ ] = ch[0] ;
                    }
                }
                #endregion

                if (buffer_length > 0)
                {
                    
                    StringBuilder output = new StringBuilder();
                    output.Append("Received data : ");
                    /*
                    for (int i = 0; i < buffer_length; ++i)
                    {
                        String tmp = Convert.ToString(buffer[i], 16);
                        if (tmp.Length == 1)
                            tmp = "0" + tmp;
                        output.Append(tmp);
                    }
                    output.Append("\r\n");
                    
                    ToWriteLog(output.ToString());
                    */

                    switch (buffer[0])
                    {
                        case 0x7e:
                                // parse mote data
                            if (Convert.ToString(buffer[7], 16) == "86")
                            {
                                string[] row1 = new string[] {
                                Convert.ToString(buffer[9], 16),  // row1[0] Node ID
                                "MTS420",                         // row1[1] Board ID
                                prev_row12,                       // row1[2] Temperature
                                prev_row13                        // row1[3] GPS data
                                };

                                if (buffer[8] == 1) // If packet ID = 1
                                {
                                    // cook Temperature
                                    int temp1 = buffer[16] * 256 + buffer[15];
                                    double fTemp;
                                    fTemp = -38.4 + 0.0098 * (Convert.ToDouble(temp1));
                                    row1[2] = Convert.ToString((int)fTemp);
                                    prev_row12 = row1[2];
                                }

                                if (buffer[8] == 2) // If packet ID = 2
                                {
                                    // cook GPS
                                    string GNS = ((buffer[27] >> 4) == 0) ? "南" : "北";
                                    string GEW = ((buffer[27] & 0xf) == 0) ? "東" : "西";

                                    string lat_deg = Convert.ToString(buffer[13], 16);
                                    string long_deg = Convert.ToString(buffer[14], 16);

                                    int temp2 = buffer[19] + buffer[20] * 256 + buffer[21] * 65536 + buffer[22] * 16777216;
                                    int temp3 = buffer[23] + buffer[24] * 256 + buffer[25] * 65536 + buffer[26] * 16777216;
                                    double lat_min = (Convert.ToDouble(temp2)) / 10000.0;
                                    double long_min = (Convert.ToDouble(temp3)) / 10000.0;
                                    string lat_dm = Convert.ToString(lat_min);
                                    string long_dm = Convert.ToString(long_min);

                                    row1[3] = GNS + "緯" + lat_deg + "度" + lat_dm + " " + GEW + "經" + long_deg + "度" + long_dm;
                                    prev_row13 = row1[3];
                                }

                                ToWriteToGridData(row1);
                            }
                            break;
                        case 0x7a:
                            // parse image data
                            /*
                            int x = (int)buffer[1] * 256 + buffer[2];
                            int y = (int)buffer[3] * 256 + buffer[4];

                            if (x >= 0 && x <= 351 && y >= 0 && y <= 287)
                            {
                                pen.Color = Color.FromArgb(buffer[5], buffer[6], buffer[7]) ;
                                g.DrawRectangle(pen, x, y, 1, 1);
                            }

                        //    pictureBox1.Invalidate();
                            */
                            break;
                        default :
                            // unknow data
                            break;
                    }
                    
                    // Convert buffer to cooked data
                  /*
                    if (Convert.ToString(buffer[7], 16) == "86")
                    {
                        string[] row1 = new string[] {
                            Convert.ToString(buffer[9], 16),  // row1[0] Node ID
                            "MTS420",                         // row1[1] Board ID
                            prev_row12,                       // row1[2] Temperature
                            prev_row13                        // row1[3] GPS data
                        };

                        if (buffer[8]==1) // If packet ID = 1
                        {
                            // cook Temperature
                            int temp1 = buffer[16] * 256 + buffer[15];
                            double fTemp;
                            fTemp = -38.4 + 0.0098 * (Convert.ToDouble(temp1));
                            row1[2] = Convert.ToString((int)fTemp);
                            prev_row12 = row1[2];
                        }

                        if (buffer[8]==2) // If packet ID = 2
                        {
                            // cook GPS
                            string GNS = ((buffer[27] >> 4) == 0) ? "南" : "北";
                            string GEW = ((buffer[27] & 0xf) == 0) ? "東" : "西";

                            string lat_deg = Convert.ToString(buffer[13], 16);
                            string long_deg = Convert.ToString(buffer[14], 16);

                            int temp2 = buffer[19] + buffer[20] * 256 + buffer[21] * 65536 + buffer[22] * 16777216;
                            int temp3 = buffer[23] + buffer[24] * 256 + buffer[25] * 65536 + buffer[26] * 16777216;
                            double lat_min = (Convert.ToDouble(temp2)) / 10000.0;
                            double long_min = (Convert.ToDouble(temp3)) / 10000.0;
                            string lat_dm = Convert.ToString(lat_min);
                            string long_dm = Convert.ToString(long_min);

                            row1[3] = GNS + "緯" + lat_deg + "度" + lat_dm + " " + GEW + "經" + long_deg + "度" + long_dm;
                            prev_row13 = row1[3];
                        }

                        ToWriteToGridData(row1);
                    }
                    */
                }        

            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            #region Critical Section
            // begin of critical section
            mut.WaitOne();
            thread_done = true;
            mut.ReleaseMutex();
            // end of critical section
            #endregion

            if (b_connected)
            {            
//                if (thread_data_receiver.IsAlive)
  //                  thread_data_receiver.Join();

                // Release the socket.
                socket.Send( Encoding.ASCII.GetBytes( ACC_DISCONNECT ) );
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                b_connected = false ;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] row1 = new string[] { "N/A", "N/A", "N/A", "N/A" };
            prev_row12 = "N/A";
            prev_row13 = "N/A";
            dataGridView1.Rows.Add(row1);
        }


        // connect to mote gateway(stargate)
        private void ToConnect(object sender, EventArgs e)
        {
            Form2 dlg = new Form2() ;

            if (dlg.ShowDialog() == DialogResult.OK )
            {
                try
                {
                    IPHostEntry ipHostInfo = Dns.GetHostEntry(dlg.GetIPAddress());
                    IPAddress ipAddress = ipHostInfo.AddressList[0];
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, int.Parse(dlg.GetPort()));
                        
                    // To release pre-created the socket
                    if (b_connected)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }

                    // Create a TCP/IP socket.                    
                    socket = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);

                    // Connect to the remote endpoint.
                    socket.Connect(remoteEP);

                    b_connected = true;

                    // set threads
                  //  thread_data_receiver = new Thread(new ThreadStart(Thread_Data_Receiver));
             //       thread_image_receiver = new Thread(new ThreadStart(Thread_Image_Receiver));
                    thread_done = false ;
               //     thread_data_receiver.Start() ;
               //     thread_image_receiver.Start() ;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

            }
        }
        
        // write logs
        private void ToWriteLog(string s)
        {
            if (textBox1.InvokeRequired )
            {
                WriteLogDelegate d = new WriteLogDelegate(ToWriteLog);
                this.Invoke(d,s);
            }
            else
            {
                textBox1.AppendText(s);
            }
            
        }

        // to forward
        private void Forward_Click(object sender, EventArgs e)
        {
            if (b_connected)
                socket.Send( Encoding.ASCII.GetBytes(ACC_FORWARD) ) ;
               
        }

        // turn left
        private void Left_Click(object sender, EventArgs e)
        {
            if (b_connected)
                socket.Send( Encoding.ASCII.GetBytes(ACC_LEFT) );
        }

        // turn right
        private void Right_Click(object sender, EventArgs e)
        {
            if (b_connected)
                socket.Send( Encoding.ASCII.GetBytes(ACC_RIGHT));
        }

        // to stop
        private void Stop_Click(object sender, EventArgs e)
        {
            if (b_connected)
                socket.Send( Encoding.ASCII.GetBytes(ACC_STOP) );
        }

        // for test only now 
        private void button5_Click(object sender, EventArgs e)
        {
           // Close();
        }

    }

}