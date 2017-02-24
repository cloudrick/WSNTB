
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Windows.Forms;

using System.Threading;
using System.IO;

namespace WSNTBS
{

    public partial class MainForm : Form
    {
        // to record if the server is started
        private bool m_IsServerStarted;

        // working thread
        private Thread threadWork;

        private TBManager tb_manager ;
        private TBConfiguration tb_config;

        // used for delegates
        public delegate void m_delegateWriteTBManagerLogs(String s);
        public delegate void m_delegateInsertItemToQueue(ListViewItem item);
        public delegate void m_delegateSelectTopItemOfQueue();
        private delegate void m_delegateRemoveTopItemOfQueue();
        private delegate void m_delegateRemoveAllItemsOfQueue();

        // used for UI
        private MySQLServerSetupForm mysql_dlg;
        private COMPortSetupForm comport_dlg;

        public MainForm()
        {
            InitializeComponent();
            m_IsServerStarted = false;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // create TBConfiguration instance
            tb_config = new TBXMLConfiguration();
            tb_config.LoadConfigure();

            // create TBManager instance
            tb_manager = new TBManager(this, tb_config);

            // create UI components
            mysql_dlg = new MySQLServerSetupForm(tb_config);
            comport_dlg = new COMPortSetupForm(tb_config);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            tb_manager.Stop();

            if (threadWork != null)
            {
                if (MessageBox.Show("按是強制結束\r\n按否等待工作完成再結束",
                                    "是否強制停止Server",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question
                                    ) == DialogResult.Yes)
                {
                    if (threadWork.IsAlive)
                        threadWork.Abort();

                }
                else
                {
                    if (threadWork.IsAlive)
                        threadWork.Join();
                }
            }
        }

        private void button_Start_Click(object sender, EventArgs e)
        {
            if (m_IsServerStarted)
            {             
                // stop manager
                tb_manager.Stop();

                // wait thread to join
                threadWork.Join();

                // enable buttons
                button_ServerSetup.Enabled = true;
                button_ComPortSetup.Enabled = true;
                button_TestNodes.Enabled = true;

                // change button text
                this.button_Start.Text = "啟動";

                // set flag to "Stop"
                m_IsServerStarted = false;

            }
            else
            {
                // disable buttons
                button_ServerSetup.Enabled = false;
                button_ComPortSetup.Enabled = false;
                button_TestNodes.Enabled = false;

                // change button text
                this.button_Start.Text = "停止";
                
                // set flag to "Start"
                m_IsServerStarted = true;

                // create Start thread
                threadWork = new Thread(new ThreadStart(tb_manager.Start));
                threadWork.Start();
            }
        }

        public void WriteLog(string inString)
        {
            if (textBox1.InvokeRequired)
            {
                m_delegateWriteTBManagerLogs d 
                    = new m_delegateWriteTBManagerLogs(WriteLog);
                this.BeginInvoke(d, inString);
            }
            else
            {
                textBox1.AppendText(inString);
            }

        }

        public void AddWorkListItem(int inExpID, string inExpUser, 
                                    int inExpDuration, string inExpPurpose)
        {
            string[] item = new string[4];
            item[0] = inExpID.ToString();
            item[1] = inExpUser;
            item[2] = inExpDuration.ToString();
            item[3] = inExpPurpose;
            WriteWorkList(new ListViewItem(item));
        }

        private void WriteWorkList(ListViewItem inItem)
        {
            if (listviewQueue.InvokeRequired)
            {
                m_delegateInsertItemToQueue d = 
                    new m_delegateInsertItemToQueue(WriteWorkList);
                this.BeginInvoke(d,inItem) ;
            }
            else
            {
                listviewQueue.Items.Add(inItem);
            }
        }

        public void SelectTopItemOfQueue()
        {
            if (listviewQueue.InvokeRequired)
            {
                m_delegateSelectTopItemOfQueue d = 
                    new m_delegateSelectTopItemOfQueue(SelectTopItemOfQueue);
                this.BeginInvoke(d);
            }
            else
            {
                listviewQueue.TopItem.Selected = true;
            }
        }

        public void RemoveTopItemOfQueue()
        {
            if (listviewQueue.InvokeRequired)
            {
                m_delegateRemoveTopItemOfQueue d = 
                    new m_delegateRemoveTopItemOfQueue(RemoveTopItemOfQueue);
                this.BeginInvoke(d);
            }
            else
            {
                listviewQueue.Items[0].Remove();
            }
        }

        public void RemoveAllItemsOfQueue()
        {
            if (listviewQueue.InvokeRequired)
            {
                m_delegateRemoveAllItemsOfQueue d = 
                    new m_delegateRemoveAllItemsOfQueue(RemoveAllItemsOfQueue);
                this.BeginInvoke(d);
            }
            else
            {
                listviewQueue.Items.Clear();
            }
        }

        private void CleanLog(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

        private void ServerSetup(object sender, EventArgs e)
        {
            if (mysql_dlg.ShowDialog() == DialogResult.OK)
            {
                tb_config.SaveConfigure();
            }
        }

        private void COMPortSetup(object sender, EventArgs e)
        {
            if (comport_dlg.ShowDialog() == DialogResult.OK)
            {
                tb_config.SaveConfigure();
                tb_manager.ReconfigureNodes();
            }    
        }

        private void button_CancelJob_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection selected = listviewQueue.SelectedItems;

            if (selected.Count == 1)
            {
                if(selected[0].Index == 0)
                {
                    MessageBox.Show("無法刪除正在執行中的工作") ;
                    return ;
                }
                tb_manager.CancelAJob( int.Parse(selected[0].SubItems[0].Text) );
                selected[0].Remove();
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            try
            {


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /*
        private bool TestAllNodes()
        {
            // disable other buttoms
            button_ServerSetup.Enabled = false;
            button_ComPortSetup.Enabled = false;
            button_TestNodes.Enabled = false;

            WriteLog("-------------- Test Start --------------\r\n");

            // initial a Proress bar
            TBProgressWindow tbProgress = new TBProgressWindow();
            tbProgress.Text = "Test Progress";
            tbProgress.TopLevel = true;
            tbProgress.Show();

            try
            {
                if (!Directory.Exists( ServerInfo.TEMP_DIRECTORY ))
                    Directory.CreateDirectory( ServerInfo.TEMP_DIRECTORY );

                for (int j = 0; j < tb_config.GetNumOfNodes() ; ++j)
                {
                    int node_id = tb_config.GetStartNodeID() + j;

                    TBProgrammer tbProgrammer = new TBProgrammer(node_id, "", tb_config.GetCOMPort(node_id));
                    Thread thread_programmer = new Thread(new ThreadStart(tbProgrammer.Install));

                    thread_programmer.Start();
                    thread_programmer.Join();
                    tbProgress.SetProgressValue((int)((j + 1) * 100 / tb_config.GetNumOfNodes()));
                    WriteLog("Node " + (node_id).ToString() + ": " + tbProgrammer.GetResultLogs() + "\r\n");
                }

                Directory.Delete(ServerInfo.TEMP_DIRECTORY,true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }


            tbProgress.Hide();
            tbProgress.Dispose();

            WriteLog("-------------- End Test --------------\r\n");

            // enable other buttons
            button_ServerSetup.Enabled = true;
            button_ComPortSetup.Enabled = true;
            button_TestNodes.Enabled = true;

            return true;
        }*/

        private void button_TestNodes_Click(object sender, EventArgs e)
        {
            TBTestNode tbTestDlg = new TBTestNode( tb_config );
            tbTestDlg.ShowDialog();
            tbTestDlg.Dispose();
        }

        private void button_CopyToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox1.Text);
        }
   
    }
}