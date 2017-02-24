using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WSNTBS
{
    public partial class COMPortSetupForm : Form
    {
        public ComboBox[] combobox = new ComboBox[50] ;
        public Label[] label = new Label[50];

        private TBConfiguration m_refToConfig;

        private int OldNumOfNodes;

        public COMPortSetupForm(TBConfiguration config)
        {
            InitializeComponent();

            m_refToConfig = config;

            // create reference for each Label
            label[0] = label1;
            label[1] = label2;
            label[2] = label3;
            label[3] = label4;
            label[4] = label5;
            label[5] = label6;
            label[6] = label7;
            label[7] = label8;
            label[8] = label9;
            label[9] = label10;
            label[10] = label11;
            label[11] = label12;
            label[12] = label13;
            label[13] = label14;
            label[14] = label15;
            label[15] = label16;
            label[16] = label17;
            label[17] = label18;
            label[18] = label19;
            label[19] = label20;
            label[20] = label21;
            label[21] = label22;
            label[22] = label23;
            label[23] = label24;
            label[24] = label25;
            label[25] = label26;
            label[26] = label27;
            label[27] = label28;
            label[28] = label29;
            label[29] = label30;
            label[30] = label31;
            label[31] = label32;
            label[32] = label33;
            label[33] = label34;
            label[34] = label35;
            label[35] = label36;
            label[36] = label37;
            label[37] = label38;
            label[38] = label39;
            label[39] = label40;
            label[40] = label41;
            label[41] = label42;
            label[42] = label43;
            label[43] = label44;
            label[44] = label45;
            label[45] = label46;
            label[46] = label47;
            label[47] = label48;
            label[48] = label49;
            label[49] = label50;

            // create reference for each ComboBox
            combobox[0] = comboBox1;
            combobox[1] = comboBox2;
            combobox[2] = comboBox3;
            combobox[3] = comboBox4;
            combobox[4] = comboBox5;
            combobox[5] = comboBox6;
            combobox[6] = comboBox7;
            combobox[7] = comboBox8;
            combobox[8] = comboBox9;
            combobox[9] = comboBox10;
            combobox[10] = comboBox11;
            combobox[11] = comboBox12;
            combobox[12] = comboBox13;
            combobox[13] = comboBox14;
            combobox[14] = comboBox15;
            combobox[15] = comboBox16;
            combobox[16] = comboBox17;
            combobox[17] = comboBox18;
            combobox[18] = comboBox19;
            combobox[19] = comboBox20;
            combobox[20] = comboBox21;
            combobox[21] = comboBox22;
            combobox[22] = comboBox23;
            combobox[23] = comboBox24;
            combobox[24] = comboBox25;
            combobox[25] = comboBox26;
            combobox[26] = comboBox27;
            combobox[27] = comboBox28;
            combobox[28] = comboBox29;
            combobox[29] = comboBox30;
            combobox[30] = comboBox31;
            combobox[31] = comboBox32;
            combobox[32] = comboBox33;
            combobox[33] = comboBox34;
            combobox[34] = comboBox35;
            combobox[35] = comboBox36;
            combobox[36] = comboBox37;
            combobox[37] = comboBox38;
            combobox[38] = comboBox39;
            combobox[39] = comboBox40;
            combobox[40] = comboBox41;
            combobox[41] = comboBox42;
            combobox[42] = comboBox43;
            combobox[43] = comboBox44;
            combobox[44] = comboBox45;
            combobox[45] = comboBox46;
            combobox[46] = comboBox47;
            combobox[47] = comboBox48;
            combobox[48] = comboBox49;
            combobox[49] = comboBox50;

            // initialize ComboBox (select COMPort)
            for (int i = 0; i < 50; ++i)
            {
                combobox[i].Items.Add("Disable");
                for (int j = 1; j <= 100; ++j)
                {
                    combobox[i].Items.Add("COM" + j.ToString());
                }
            }

            // initialize ComboBox (Start Node ID)
            for (int i = 1; i <= ServerInfo.MAX_NUM_OF_NODE; ++i)
            {
                comboBox_StartNodeID.Items.Add(i.ToString());
            }
            comboBox_StartNodeID.SelectedIndex = m_refToConfig.GetStartNodeID() - 1 ;

            // initialize ComboBox (Number of Nodes)
            for (int i = 0; i <= 50; ++i)
            {
                comboBox_NumOfNodes.Items.Add(i.ToString());
            }
            comboBox_NumOfNodes.SelectedIndex = m_refToConfig.GetNumOfNodes() ;
            OldNumOfNodes = m_refToConfig.GetNumOfNodes() ;

            // initialize ComboBox (Region)
            comboBox_Region.Items.Add("NCU1");
            comboBox_Region.Items.Add("NCU2");
            comboBox_Region.Items.Add("NTHU1");

            switch ( m_refToConfig.GetRegionText() )
            {
                case "ncu1" :
                    comboBox_Region.SelectedIndex = 0;
                    break;
                case "ncu2" :
                    comboBox_Region.SelectedIndex = 1;
                    break;
                case "nthu1":
                    comboBox_Region.SelectedIndex = 2;
                    break;
            }

            // initialize ComboBox (max programming threads)
            for (int i = 1; i <= 32 ; ++i)
            {
                comboBox_MaxProgThreads.Items.Add(i.ToString());
            }
            comboBox_MaxProgThreads.SelectedIndex = m_refToConfig.GetMaxProgrammerThreads()-1;

            UpdateComboBoxs();
         
        }

        public int GetStartNodeID()
        {
            return comboBox_StartNodeID.SelectedIndex+1;
        }

        public int GetNumberOfNodes()
        {
            return comboBox_NumOfNodes.SelectedIndex;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int s = GetStartNodeID();
            int n = GetNumberOfNodes();

            for (int i = 0; i < n; ++i)
            {
                string comport = combobox[i].SelectedItem.ToString();

                if (comport == "Disable")
                    m_refToConfig.SetCOMPort(s + i, 0);
                else
                    m_refToConfig.SetCOMPort(s + i, int.Parse(comport.Substring(3)));
            }

            m_refToConfig.SetStartNodeID(s) ;
            m_refToConfig.SetNumOfNodes(n) ;
            m_refToConfig.SetMaxProgrammerThreads(comboBox_MaxProgThreads.SelectedIndex + 1);

            switch (comboBox_Region.SelectedIndex)
            {
                case 0:
                    m_refToConfig.SetRegion("ncu1") ;
                    break;
                case 1:
                    m_refToConfig.SetRegion("ncu2");
                    break;
                case 2:
                    m_refToConfig.SetRegion("nthu1");
                    break;
            }

            DialogResult = DialogResult.OK;
        }

        private void comboBox_StartNodeID_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateComboBoxs();
        }

        private void comboBox_NumOfNodes_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateComboBoxs();
        }

        private void UpdateComboBoxs()
        {
            int s = comboBox_StartNodeID.SelectedIndex+1;
            int n = comboBox_NumOfNodes.SelectedIndex;

            if (s + n - 1 > ServerInfo.MAX_NUM_OF_NODE)
            {
                button2.Enabled = false;
                MessageBox.Show("節點編號不得超過 " + ServerInfo.MAX_NUM_OF_NODE.ToString());
                return;
            }

            for(int i=0 ; i<50 ; ++i)
            {
                if( i < n )
                {
                    label[i].Text = "Node " + (s + i).ToString() + "：";
                    combobox[i].Visible = true;

                    if (i < OldNumOfNodes)
                    {
                        if (m_refToConfig.GetCOMPort(i + s) != "")
                        {
                            combobox[i].SelectedIndex = int.Parse(m_refToConfig.GetCOMPort(i + s).Substring(3));
                        }
                        else
                            combobox[i].SelectedIndex = 0;
                    }
                    else
                        combobox[i].SelectedIndex = 0;
                }
                else
                {
                    label[i].Text = "";
                    combobox[i].SelectedIndex = 0;
                    combobox[i].Visible = false;
                }
            }

            button2.Enabled = true;

            OldNumOfNodes = n;
            this.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int s = comboBox_StartNodeID.SelectedIndex+1;
            int n = comboBox_NumOfNodes.SelectedIndex;

            for (int i = 0; i < n; ++i)
            {
                if (combobox[i].SelectedIndex != 0)
                {
                    for (int j = i + 1; j < n; ++j)
                    {
                        if (combobox[i].SelectedIndex + j - i + 1 <= ServerInfo.MAX_NUM_OF_NODE)
                        {
                            combobox[j].SelectedIndex = combobox[i].SelectedIndex + j - i;
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;
                }
            }
        }

    }
}