
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;

namespace WSNTBS
{

    public partial class Form1 : Form
    {
        // const
        private const int SLEEP_TIME = 10; // seconds
        private string WWW_ROOT ;

        // to record if the server is started
        private bool bIsServerStarted;

        // the listen socket
        private Socket socketListen ;

        // used for threads
        private Thread threadNetworkComm  ;
        private Thread threadWork ;
        
        private static Mutex mut = new Mutex();
        public ManualResetEvent HasAccepted = new ManualResetEvent(false);
        public ManualResetEvent InstallProcessLock = new ManualResetEvent(false);

        // used for installing image threads
        private classInstallImageThreadObject [] installobj;
        private Thread [] threadInstallImage;
        private Thread [] threadSearilPortListen;

        // timer
        private System.Threading.Timer timer;

        // used for delegates
        public delegate void WriteLogDelegate(String s);
        public delegate void WriteWorkListDelegate(ListViewItem item);
        public delegate void SelectWorkingItemDelegate();
        public delegate void RemoveWorkingItemDelegate();

        public Form1()
        {
            InitializeComponent();

            // initial server state
            bIsServerStarted = false;

            // initial socket
            socketListen= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // initial threads
            threadNetworkComm = new Thread(new ThreadStart(NetworkCommThread));
            threadWork = new Thread(new ThreadStart(WorkThread));
            threadInstallImage = new Thread[34] ;
            threadSearilPortListen = new Thread[34];
            installobj = new classInstallImageThreadObject[34];
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FileInfo file = new FileInfo("WSNTBS.conf");

            if (file.Exists)
            {
                StreamReader sr = new StreamReader("WSNTBS.conf");
                WWW_ROOT = sr.ReadLine();
                sr.Close();
            }
            else
            {
                StreamWriter sw = file.CreateText();
                WWW_ROOT = "C:\\www\\";
                sw.Write(WWW_ROOT);
                sw.Close();
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            #region Critical Section
            mut.WaitOne();
            bIsServerStarted = false;
            mut.ReleaseMutex();
            #endregion

            InstallProcessLock.Set();

            if( threadNetworkComm.IsAlive)
            {
                threadNetworkComm.Abort();
            }

            if (socketListen.Connected)
            {
                socketListen.Shutdown(SocketShutdown.Both);
                socketListen.Disconnect(false);
                socketListen.Close();
            }


            if (MessageBox.Show("按是強制結束\n按否等待目前工作結束", "是否強制停止Server", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == DialogResult.Yes)
            {
                if (threadWork.IsAlive)
                {
                    threadWork.Abort();
                }
            }
            else
            {
                if (threadWork.IsAlive)
                {
                    threadWork.Join();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (bIsServerStarted)
            {             

                #region Critical Section
                mut.WaitOne();
                bIsServerStarted = false;
                mut.ReleaseMutex();
                #endregion

               
                InstallProcessLock.Set();
                threadWork.Join();
             
                if (socketListen.Connected)
                {
                    socketListen.Shutdown(SocketShutdown.Both);
                    socketListen.Disconnect(true);
                }

                this.button1.Text = "啟動";
            }
            else
            {

                #region Critical Section
                mut.WaitOne();
                bIsServerStarted = true;
                mut.ReleaseMutex();
                #endregion

                if (threadWork.ThreadState == System.Threading.ThreadState.Stopped)
                {
                    threadWork = new Thread(new ThreadStart(WorkThread));
                }

                threadWork.Start();
                this.button1.Text = "停止";

            }
        }

        private void WorkThread()
        {
            for(;;)
            {
                #region Critical Section
                mut.WaitOne();
                if (!bIsServerStarted)
                {
                    mut.ReleaseMutex();
                    break;
                }
                mut.ReleaseMutex();
                #endregion
            
                String [] files = Directory.GetFiles( WWW_ROOT + "\\todo");

                #region update ListView
                for (int i = 0; i < files.Length; ++i)
                {
                    StreamReader sr = new StreamReader(
                        WWW_ROOT + "\\user_submission_info\\" +
                        files[i].Substring(files[i].LastIndexOf("\\") + 1) +
                        ".info");

                    String [] item = new String[4] ;
                    item[0] = files[i].Substring(files[i].LastIndexOf("\\") + 1);
                    item[1] = sr.ReadLine();
                    item[3] = sr.ReadLine();
                    item[2] = sr.ReadLine();
                    sr.Close();
                    WriteWorkList(new ListViewItem(item));
                }
                #endregion

                #region process submissions
                for (int i = 0; i < files.Length; ++i)
                {

                    #region Critical Section
                    mut.WaitOne();
                    if (!bIsServerStarted)
                    {
                        mut.ReleaseMutex();
                        break;
                    }
                    mut.ReleaseMutex();
                    #endregion

                    SelectWorkingItem();

                    #region
                    try
                    {

                        string sn = files[i].Substring(files[i].LastIndexOf("\\") + 1);

                        WriteLog("Case " + sn + "\n");

                        StreamReader sr = new StreamReader(WWW_ROOT + "\\user_submission_info\\" + sn + ".info");

                        string ex_userid = sr.ReadLine();
                        string ex_desc = sr.ReadLine();
                        int ex_time = int.Parse(sr.ReadLine());

                        for (int j = 0; j < 34; ++j)
                        {
                            installobj[j] = new classInstallImageThreadObject(j+1, "NONE");
                            installobj[j].ResultPath = WWW_ROOT + "\\result\\" + ex_userid + "\\" + sn + "\\" + (j+1).ToString() + ".log";

                            // if the RESULT directory does not exist , create it
                            DirectoryInfo d = new DirectoryInfo(WWW_ROOT + "\\result\\" + ex_userid);
                            if (!d.Exists)
                            {
                                d.Create();
                            }             

                            d = new DirectoryInfo(WWW_ROOT + "\\result\\" + ex_userid + "\\" + sn);
                            if (!d.Exists)
                            {
                                d.Create();
                            }

                        }

                        string line;
                        while ((line = sr.ReadLine()) != null && line != "")
                        {
                            string[] part = line.Split(' ');

                            int node_id = int.Parse(part[0]);
                            string image_name = part[1];

                            installobj[node_id - 1].ImagePathName = WWW_ROOT + "\\user_submission_image\\" + ex_userid + "\\" + image_name;                           
                            installobj[node_id - 1].WorkingDirectory = WWW_ROOT + "\\workdir\\" + node_id + "\\";
                        }

                        sr.Close();

                        for (int j = 0; j < 34; ++j)
                        {
                            threadInstallImage[j] = new Thread(new ThreadStart(installobj[j].Install));
                            threadInstallImage[j].Start() ;
                        }
                        
                        for(int j = 0; j < 34; ++j)
                        {
                            threadInstallImage[j].Join();
                            WriteLog("Node " + j.ToString() + " : Install Thread is Done, " + installobj[j].InstallResult + "\r\n");
                        }

                        WriteLog("Install completed!\r\n");
                    
                        // Start Listen Threads
                        
                        classInstallImageThreadObject.ListenDone = false;

                        for(int j=0 ; j<34 ; ++j)
                        {
                            threadSearilPortListen[j] = new Thread(new ThreadStart(installobj[j].Listen));
                            threadSearilPortListen[j].Start();
                        }
                        WriteLog("Listener Started!\r\n");
                        
                        // Start Timer and block for running application of motes
                        timer = new System.Threading.Timer(new TimerCallback(TimerFired));
                        timer.Change( ex_time * 60 * 1000, 0);

                        InstallProcessLock.Reset();         
                        InstallProcessLock.WaitOne();   // wait here
                        WriteLog("Timer is up!\r\n");

             
                        // Stop Listen threads
                        #region Critical Section 
                        classInstallImageThreadObject.mut.WaitOne();
                        classInstallImageThreadObject.ListenDone = true;
                        classInstallImageThreadObject.mut.ReleaseMutex() ;
                        #endregion
            
                        for (int j = 0; j < 34; ++j)
                        {
                            threadSearilPortListen[j].Join() ;
                            WriteLog("Node " + j.ToString() + " : Listen Thread is Done!\r\n");
                        }
                        
                        RemoveWorkingItem();
                        WriteLog("End Case\r\n");
                        
                        // work is finished

                        // clean todo file
                        FileInfo file = new FileInfo(WWW_ROOT + "\\todo\\" + sn );
                        file.Delete();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    #endregion
                }
                #endregion

                Thread.Sleep(SLEEP_TIME * 1000);
            }
        }

        private void TimerFired(object state)
        {
            InstallProcessLock.Set();
        }

        private void NetworkCommThread()
        {                
            socketListen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint end_point = new IPEndPoint(IPAddress.Any, 8000);
            socketListen.Bind( end_point );
            socketListen.Listen(10);

            while (true)
            {
                HasAccepted.Reset();
                socketListen.BeginAccept(new AsyncCallback(AcceptCallback), socketListen);
                HasAccepted.WaitOne();
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            HasAccepted.Set();
            MessageBox.Show("Accepted");
            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            byte[] buffer = new byte[64];

            while(true)
            {
                try
                {
                    int bytesRec = handler.Receive(buffer);

                    if (bytesRec > 0)
                    {
                        MessageBox.Show("Written");

                        #region Critical Section
                        mut.WaitOne();
                        WriteLog(Encoding.ASCII.GetString(buffer, 0, bytesRec));
                        mut.ReleaseMutex();
                        #endregion

                        handler.Send(buffer);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    break;
                }
            }
    
        }

        private void WriteLog(string s)
        {
            if (textBox1.InvokeRequired)
            {
                WriteLogDelegate d = new WriteLogDelegate(WriteLog);
                this.BeginInvoke(d, s);
            }
            else
            {
                textBox1.AppendText(s);
            }

        }

        private void WriteWorkList(ListViewItem item)
        {
            if (listView2.InvokeRequired)
            {
                WriteWorkListDelegate d = new WriteWorkListDelegate(WriteWorkList);
                this.Invoke(d,item) ;
            }
            else
            {
                listView2.Items.Add(item);
            }
        }

        private void SelectWorkingItem()
        {
            if (listView2.InvokeRequired)
            {
                SelectWorkingItemDelegate d = new SelectWorkingItemDelegate(SelectWorkingItem);
                this.Invoke(d);
            }
            else
            {
                listView2.TopItem.Selected = true;
            }
        }

        private void RemoveWorkingItem()
        {
            if (listView2.InvokeRequired)
            {
                RemoveWorkingItemDelegate d = new RemoveWorkingItemDelegate(RemoveWorkingItem);
                this.Invoke(d);
            }
            else
            {
                listView2.Items[0].Remove();
            }
        }

        private void CleanLog(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

        private void ServerSetup(object sender, EventArgs e)
        {
            ServerSetup dlg = new ServerSetup();
            dlg.strWWWPath = WWW_ROOT;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                WWW_ROOT = dlg.strWWWPath;

                StreamWriter sw = new StreamWriter("WSNTBS.conf");
                sw.Write( WWW_ROOT );
                sw.Close();
                
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
       
    }

    class classInstallImageThreadObject
    {
        public int NodeID;
        public string ImagePathName;       
        public string ResultPath ;
        public string WorkingDirectory;
        public string InstallResult;

        public static bool ListenDone = false;
        public static Mutex mut = new Mutex();

        // serial port mapping table
        public readonly string[] OCTOPUSX_COM = { 
                                                  // octopus1
                                                  "21", "22", "23", "24", "25", 
                                                  "26", "27", "28", "29", "30", 
                                                  "31", "32", "33", "34", "35", 
                                                  "36", "37",
                                                  // octopus2
                                                  "3" , "4" , "5" , "6" , "7" , 
                                                  "8" , "9" , "10", "11", "12", 
                                                  "13", "14", "15", "16", "17", 
                                                  "18", "19" };

        public classInstallImageThreadObject()
        {
            NodeID = -1;
            ImagePathName = "NONE";
        }

        public classInstallImageThreadObject(int id, string pn)
        {
            NodeID = id;
            ImagePathName = pn;
        }

        public void Install()
        {
            if (NodeID >= 0 && ImagePathName != "NONE")
            {
                try
                {
                    // install image allmost 3 time
                    InstallResult = "the installation is fail!";
                    for (int i = 0; i < 3; ++i)
                    {
                        #region Mote Install Process
                        Process proc = new Process();
                        proc.StartInfo.FileName = WorkingDirectory + "\\himg2.exe";
                        proc.StartInfo.WorkingDirectory = WorkingDirectory;
                        proc.StartInfo.UseShellExecute = false;
                        proc.StartInfo.CreateNoWindow = true;
                        proc.StartInfo.RedirectStandardOutput = true;
                        proc.StartInfo.Arguments = "-m0 -s2 -v -cCOM" + OCTOPUSX_COM[NodeID - 1] + " " + ImagePathName;

                        proc.Start();

                        String str = proc.StandardOutput.ReadToEnd();

                        proc.WaitForExit();
                        proc.Close();

                        if (str.IndexOf("completed") > 0 && str.IndexOf("ERROR") == -1)
                        {
                            InstallResult = "the installation is successful!";
                            break;
                        }

                        #endregion

                    }

                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.ToString());
                }
            }
            else if (NodeID >= 0 && ImagePathName == "NONE")
            {
                InstallResult = "the installation did not be processed";
            }
        }

        public void Listen()
        {
            try
            {

                SerialPort port = new SerialPort("COM" + OCTOPUSX_COM[NodeID - 1],
                                                 57600,
                                                 Parity.None,
                                                 8,
                                                 StopBits.One);

                port.Open();

                StreamWriter sw = new StreamWriter(ResultPath);

                int ch;
                int[] buffer = new int[128];
                int bytes = 0;

                for (; ; )
                {
                    #region Critical Section
                    mut.WaitOne();
                    if (ListenDone == true)
                    {
                        mut.ReleaseMutex();
                        break;
                    }
                    mut.ReleaseMutex();
                    #endregion

                    if (port.BytesToRead > 0)
                    {
                        ch = port.ReadByte();

                        if (ch == 0x7e)
                        {
                            if (bytes > 2)
                            {
                                if (buffer[0] == 0x7e)
                                {
                                    DateTime datetime = DateTime.Now;

                                    sw.Write( datetime.ToString("yyyy/MM/dd HH:mm:ss ") );
                                    for (int i = 0; i < bytes; ++i)
                                    {
                                        string tmp = Convert.ToString(buffer[i], 16);

                                        if (buffer[i] < 16)
                                        {
                                            tmp = "0" + tmp;
                                        }

                                        sw.Write(tmp + " ");
                                    }
                                    sw.Write("\r\n");
                                    sw.Flush();
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
                }
                sw.Close();
                port.Close();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }
    }
}