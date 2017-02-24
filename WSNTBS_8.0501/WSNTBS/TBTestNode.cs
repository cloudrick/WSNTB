using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace WSNTBS
{
    public partial class TBTestNode : Form
    {
        private TBConfiguration m_refToConfig;

        public TBTestNode(TBConfiguration inConfig)
        {
            InitializeComponent();

            m_refToConfig = inConfig;

            comboBox1.Items.Add("請選擇要測試的節點");
            comboBox1.SelectedIndex = 0;
            for (int i = 0; i < m_refToConfig.GetNumOfNodes(); ++i)
            {
                int node_id = m_refToConfig.GetStartNodeID() + i;
                if (m_refToConfig.GetCOMPort(node_id) != "")
                    comboBox1.Items.Add("Node " + node_id.ToString()); 
            }

            for (int i = 2; i <= m_refToConfig.GetNumOfNodes(); ++i)
            {
                comboBox2.Items.Add(i.ToString()); 
            }
            comboBox2.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != 0)
            {
                int select_node_num = Convert.ToInt32(
                                        comboBox1.SelectedItem.ToString().Substring(5),
                                        10);
                textBox1.Text = m_refToConfig.GetCOMPort(select_node_num);

            }
            else
                textBox1.Text = "";
        }

        private void radioButton2_Enter(object sender, EventArgs e)
        {
            comboBox2.Enabled = true;
        }

        private void radioButton1_Enter(object sender, EventArgs e)
        {
            comboBox2.Enabled = false;
        }

        private void button_TestSingleNode_Click(object sender, EventArgs e)
        {
            DisableUserInput();
            textBox2.Clear();
            progressBar1.Value = 0;

            textBox2.AppendText("---------------------- Test Start ----------------------\r\n") ;

            int node_id = Convert.ToInt32( comboBox1.SelectedItem.ToString().Substring(5),
                                           10) ;

            TBProgrammer tbprogrammer = new TBProgrammer( node_id,
                                                          "",
                                                          m_refToConfig.GetCOMPort(node_id)
                                                        );

            if (!Directory.Exists(ServerInfo.TEMP_DIRECTORY))
                Directory.CreateDirectory(ServerInfo.TEMP_DIRECTORY);
            tbprogrammer.Install();
            Directory.Delete(ServerInfo.TEMP_DIRECTORY, true);

            progressBar1.Value = 100;
            textBox2.AppendText("Node " + node_id.ToString() + ": " + tbprogrammer.GetResultLogs() + "\r\n") ;
            textBox2.AppendText("---------------------- Test End ----------------------\r\n");
            EnableUserInput();
        }

        private void button_TestAllNodes_Click(object sender, EventArgs e)
        {
            DisableUserInput();

            textBox2.Clear();
            progressBar1.Value = 0;

            textBox2.AppendText("---------------------- Test Start ----------------------\r\n");

            if (!Directory.Exists(ServerInfo.TEMP_DIRECTORY))
                Directory.CreateDirectory(ServerInfo.TEMP_DIRECTORY);

            if (radioButton1.Checked)
            {
                TBProgrammer.SetMaximumProgrammingThread(1);
                for (int j = 0; j < m_refToConfig.GetNumOfNodes(); ++j)
                {
                    int node_id = m_refToConfig.GetStartNodeID() + j;

                    TBProgrammer tbProgrammer = new TBProgrammer(node_id, "", m_refToConfig.GetCOMPort(node_id));
                    Thread thread_programmer = new Thread(new ThreadStart(tbProgrammer.Install));

                    thread_programmer.Start();
                    thread_programmer.Join();

                    progressBar1.Value = (int)((j + 1) * 100 / m_refToConfig.GetNumOfNodes());
                    textBox2.Text += "Node " + (node_id).ToString() + ": " + tbProgrammer.GetResultLogs() + "\r\n";
                }
            }
            else
            {
                TBProgrammer.SetMaximumProgrammingThread(Convert.ToInt32(comboBox2.SelectedItem.ToString(), 10));
                TBProgrammer[] tbProgrammer = new TBProgrammer[m_refToConfig.GetNumOfNodes()];
                Thread[] thread_programmer = new Thread[m_refToConfig.GetNumOfNodes()];

                for (int j = 0; j < m_refToConfig.GetNumOfNodes(); ++j)
                {
                    int node_id = m_refToConfig.GetStartNodeID() + j;

                    tbProgrammer[j] = new TBProgrammer(node_id, "", m_refToConfig.GetCOMPort(node_id));
                    thread_programmer[j] = new Thread(new ThreadStart(tbProgrammer[j].Install));
                    thread_programmer[j].Start();
                }
                for (int j = 0; j < m_refToConfig.GetNumOfNodes(); ++j)
                {
                    thread_programmer[j].Join();
                    progressBar1.Value = (int)((j + 1) * 100 / m_refToConfig.GetNumOfNodes());
                    textBox2.Text += "Node " + (m_refToConfig.GetStartNodeID() + j).ToString() + ": " + tbProgrammer[j].GetResultLogs() + "\r\n";
                }
            }

            Directory.Delete(ServerInfo.TEMP_DIRECTORY, true);
            textBox2.AppendText("---------------------- Test End ----------------------\r\n");
            EnableUserInput();
        }

        private void DisableUserInput()
        {
            comboBox1.Enabled = false;
            comboBox2.Enabled = false;
            radioButton1.Enabled = false;
            radioButton2.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
        }

        private void EnableUserInput()
        {
            comboBox1.Enabled = true;
            comboBox2.Enabled = true;
            radioButton1.Enabled = true;
            radioButton2.Enabled = true;
            button1.Enabled = true;
            button2.Enabled = true;
        }

    }
}