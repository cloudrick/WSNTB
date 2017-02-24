using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Security.Cryptography ;

namespace WSNTBDelugeClient
{
    public partial class Form1 : Form
    {
        public static readonly string DELUGE_SERVER_ADDR = "140.115.52.114" ;
        public static readonly int    DELUGE_SERVER_PORT = 35330;

        private bool m_IsConnected;
        private Socket m_socket;

        private string username;
        private string password;

        private Thread thread_watchdog;
        private Thread thread_receiver;
        private bool m_IsRecStop;
        private Mutex mutIsRecStop = new Mutex();
        private Mutex mutReceiving = new Mutex();

        private delegate void AppendLogDelegate(string str);
        private delegate void SetLoginButtonTextDelegate(string str);

        public Form1()
        {
            InitializeComponent();
            m_IsConnected = false;
            m_IsRecStop = true;

            comboBox_NodeID_Ping.Items.Add("不指定");
            for (int i = 1; i <= 100; ++i)
            {
                comboBox_NodeID_Ping.Items.Add(i.ToString());
            }
            comboBox_NodeID_Ping.SelectedIndex = 0;

            comboBox_ImageNum_Inject.SelectedIndex = 0;
            openFileDialog1.FileName = "";

            comboBox_ImageNum_Reboot.SelectedIndex = 0;
            comboBox_ImageNum_Erase.SelectedIndex = 0;
            comboBox_ImageNum_Reset.SelectedIndex = 0;
        }

        private void button_LogInLogOut_Click(object sender, EventArgs e)
        {
            if (!m_IsConnected)
            {
                LoginDialog loginDlg = new LoginDialog();

                if (loginDlg.ShowDialog()== DialogResult.OK)
                {
                    username = loginDlg.UserName ;
                    password = loginDlg.Password ;
                    if ( Connect())
                    {
                        if (CheckAccessPermission(username, password))
                        {
                            m_IsRecStop = false;
                            thread_watchdog = new Thread(new ThreadStart(WatchDog));
                            thread_receiver = new Thread(new ThreadStart(SocketReceiver));
                            thread_watchdog.Start();
                            thread_receiver.Start();
                            m_IsConnected = true;
                            SetLoginButtonText("登出");
                            MessageBox.Show("登入成功!", "登入", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            ErrorMsgBox("帳號或密碼輸入錯誤 或 現在尚未執行您的工作!!");
                            Disconnect();
                        }
                        
                    }
                }
            }
            else
            {
                Disconnect();
                m_IsConnected = false;
                SetLoginButtonText("登入...");
                MessageBox.Show("已登出!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                try
                {
                    thread_watchdog.Abort();
                }
                catch (Exception)
                {

                }
            }
        }

        private bool Connect()
        {
            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(DELUGE_SERVER_ADDR);
                IPAddress   ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, DELUGE_SERVER_PORT);

                m_socket = new Socket(remoteEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                m_socket.Connect(remoteEP);
                m_socket.ReceiveTimeout = 5000;
                
            }
            catch (Exception)
            {
                ErrorMsgBox("無法連線到伺服器!");
                return false;
            }
            
            return true;
        }

        private void Disconnect()
        {
            mutIsRecStop.WaitOne();
            m_IsRecStop = true;
            mutIsRecStop.ReleaseMutex();

            if (thread_receiver!=null)
                thread_receiver.Join();

            m_socket.Send(Encoding.ASCII.GetBytes("DISCONNECT\0"));
            m_socket.Shutdown(SocketShutdown.Both);
            m_socket.Disconnect(true);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_IsConnected)
            {
                Disconnect();
            }
        }

        private bool CheckAccessPermission(string username, string password)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte [] tmp = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
            string md5pw = "";

            foreach (byte e in tmp)
            {
                string t = Convert.ToString(e, 16);
                if (t.Length == 1)
                    t = "0" + t;
                md5pw += t;
            }

            while (m_socket.Available != 0) ;
            mutReceiving.WaitOne();

            byte[] recbuff = new byte[16];
            int reclen =0;

            // send authentication message
            m_socket.Send(Encoding.ASCII.GetBytes("AUTH " + username + " " + md5pw + "\0"));

            // wait for authentication reply message
            while (m_socket.Available == 0) ;
        
            try
            {
                reclen = m_socket.Receive(recbuff);
            }
            catch (Exception)
            {
                reclen = 0;
            }

            mutReceiving.ReleaseMutex();

            if (reclen > 0)
            {
                if (Encoding.ASCII.GetString(recbuff, 0, reclen) == "AUTH OK\0")
                {
                    return true;
                }
                else if (Encoding.ASCII.GetString(recbuff, 0, reclen) == "AUTH NO\0")
                {
                    return false;
                }   
            }
            return false;
        }

        private void WatchDog()
        {
            for (; ; )
            {
                Thread.Sleep(60000);
                if (!CheckAccessPermission(username, password))
                {
                    Disconnect();
                    m_IsConnected = false;
                    SetLoginButtonText( "登入...");
                    MessageBox.Show("強制登出，現在已非您的使用時段!!", "登出", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                }
            }
        }

        private void SocketReceiver()
        {
            for (; ; )
            {
                mutIsRecStop.WaitOne();
                if (m_IsRecStop)
                {
                    mutIsRecStop.ReleaseMutex();
                    break;
                }
                mutIsRecStop.ReleaseMutex();

                byte[] buf = new byte[256];
                int recbytes = 0;

                mutReceiving.WaitOne();

                if (m_socket.Available > 0)
                {
                    try
                    {
                        recbytes = m_socket.Receive(buf);
                    }
                    catch (Exception)
                    {
                        recbytes = 0;
                    }
                }

                mutReceiving.ReleaseMutex();

                if (recbytes > 0)
                {
                    AppendLog(Encoding.ASCII.GetString(buf));
                }

                Thread.Sleep(200);
            }
        }

        private void SetLoginButtonText(string text)
        {
            if (button_LoginAndLogOut.InvokeRequired)
            {
                SetLoginButtonTextDelegate d = new SetLoginButtonTextDelegate(SetLoginButtonText);
                this.BeginInvoke(d, text);
            }
            else
            {
                button_LoginAndLogOut.Text = text;
            }
        }

        private void button_Ping_Click(object sender, EventArgs e)
        {
            if (m_IsConnected)
            {
                #region
                richTextBox_SystemLog.Clear();
                m_socket.Send(Encoding.ASCII.GetBytes("PING " + comboBox_NodeID_Ping.SelectedIndex.ToString() + "\0" ));
                #endregion
            }
            else
            {
                ErrorMsgBox("請先登入系統!!");
            }
        }

        private void button_Inject_Click(object sender, EventArgs e)
        {
            if (m_IsConnected)
            {         
                if (openFileDialog1.FileName == "")
                {
                    ErrorMsgBox("請選擇要 Inject 的檔案!");
                    return;
                }

                #region
                richTextBox_SystemLog.Clear();
                mutReceiving.WaitOne();
                m_socket.Send(Encoding.ASCII.GetBytes("INJECT " + comboBox_ImageNum_Inject.SelectedIndex.ToString() + "\0"));

                FileStream   fs = new FileStream( openFileDialog1.FileName , FileMode.Open);
                BinaryReader br = new BinaryReader(fs);

                m_socket.Send( Encoding.ASCII.GetBytes("FILESIZE " + fs.Length.ToString() + "\0" ));

                Thread.Sleep(200);

                for (int i = 0; i < fs.Length; ++i)
                {
                    m_socket.Send(br.ReadBytes(1));
                }

                br.Close();
                fs.Close();
                mutReceiving.ReleaseMutex();
                #endregion
            }
            else
            {
                ErrorMsgBox("請先登入系統!!");
            }
        }

        private void button_Inject_browse_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog()==DialogResult.OK)
            {
                textBox_InjectFile.Text = openFileDialog1.FileName;
            }
        }

        private void button_Reboot_Click(object sender, EventArgs e)
        {
            if (m_IsConnected)
            {
                #region
                richTextBox_SystemLog.Clear();
                mutReceiving.WaitOne();
                m_socket.Send(Encoding.ASCII.GetBytes("REBOOT " + comboBox_ImageNum_Reboot.SelectedIndex.ToString() + "\0"));
                mutReceiving.ReleaseMutex();
                #endregion
            }
            else
            {
                ErrorMsgBox("請先登入系統!!");
            }
        }

        private void button_Erase_Click(object sender, EventArgs e)
        {
            if (m_IsConnected)
            {
                #region
                richTextBox_SystemLog.Clear();
                mutReceiving.WaitOne();
                m_socket.Send(Encoding.ASCII.GetBytes("ERASE " + (comboBox_ImageNum_Erase.SelectedIndex).ToString() + "\0"));
                mutReceiving.ReleaseMutex();
                #endregion
            }
            else
            {
                ErrorMsgBox("請先登入系統!!");
            }
        }

        private void button_Reset_Click(object sender, EventArgs e)
        {
            if (m_IsConnected)
            {
                #region
                richTextBox_SystemLog.Clear();
                mutReceiving.WaitOne();
                m_socket.Send(Encoding.ASCII.GetBytes("RESET " + (comboBox_ImageNum_Reset.SelectedIndex).ToString() + "\0"));
                mutReceiving.ReleaseMutex();
                #endregion
            }
            else
            {
                ErrorMsgBox("請先登入系統!!");
            }
        }

        private void ErrorMsgBox(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void AppendLog(string logtext)
        {
            if(richTextBox_SystemLog.InvokeRequired)
            {
                AppendLogDelegate d = new AppendLogDelegate( AppendLog) ;
                this.BeginInvoke(d,logtext);
            }
            else
            {
                richTextBox_SystemLog.AppendText(logtext) ;
                richTextBox_SystemLog.ScrollToCaret();
            }
        }

        private void button_About_Click(object sender, EventArgs e)
        {
            About abDlg = new About();
            abDlg.ShowDialog();
        }
    }
}