using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Security.AccessControl;
using Microsoft.Win32;
using System.Threading;
using System.IO;

namespace Octopus2Programmer
{
    public partial class Form1 : Form
    {
        protected Octopus2bsl bsl = new Octopus2bsl();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RefrashComPort();
        }

        protected int GetRefCountByID(string id)
        {
            int ref_count = 0;
            try
            {
                RegistryKey reg_usb = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB\VID_0403&PID_6001\" + id + @"\Device Parameters\" );

                if (reg_usb == null)
                {
                    return 0;
                }

                string value = reg_usb.GetValue("SymbolicName").ToString();
                
                // i do not know why i got one more char ??
                if (value[value.Length - 1] != '}')
                    value = value.Remove(value.Length - 1);

                string[] sym = value.Split(new char[] { '\\', '#' });

                if (sym.Length >= 6)
                {
                    string devstring = sym[5] + @"\##?#" + sym[2] + "#" + sym[3] + "#" + sym[4] + "#" + sym[5] + @"\Control";

                    RegistryKey reg_dev = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\DeviceClasses\" + devstring );

                    if (reg_dev == null)
                    {
                        return 0;
                    }

                    ref_count = int.Parse(reg_dev.GetValue("ReferenceCount").ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return ref_count;
        }

        protected void RefrashComPort()
        {
            try
            {
                comboBox1.Items.Clear();

                RegistryKey reg_ftdibus = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\FTDIBUS");

                if (reg_ftdibus == null)
                {
                    return;
                }

                string[] reg_ftdibus_subkey = reg_ftdibus.GetSubKeyNames();

                foreach (string subkey in reg_ftdibus_subkey)
                {
                    if (subkey.Substring(0, 18) == "VID_0403+PID_6001+" && subkey.Length > 26)
                    {
                        string id = subkey.Substring(18, 8);

                        if (GetRefCountByID(id) > 0)
                        {
                            RegistryKey reg_comport = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\FTDIBUS\" + subkey + @"\0000\Device Parameters");

                            if (reg_comport == null)
                            {
                                return;
                            }

                            comboBox1.Items.Add(reg_comport.GetValue("PortName").ToString());
                        }
                    }
                }

                reg_ftdibus.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            if(comboBox1.Items.Count>0)
                comboBox1.SelectedIndex = 0;

        }

        private void button_ScanCOMPort_Click(object sender, EventArgs e)
        {
            RefrashComPort();
        }

        private void button_Browse_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog()==DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button_StartProgram_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("請選擇連接埠!");
                return;
            }
            if(openFileDialog1.FileName == "")
            {
                MessageBox.Show("請選擇檔案!");
                return;
            }

            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            comboBox1.Enabled = false;

            string image_filename = openFileDialog1.FileName ;

            if (checkBox1.Checked)
            {
                WriteTOSBootCode(image_filename, "_tmp_ihex.ihex");
                image_filename = "_tmp_ihex.ihex" ;
            }

            SetProgress(0);
            ClearLog();
            bsl.form = this;

            int start_time = Environment.TickCount;

            if (bsl.program_start(image_filename, comboBox1.SelectedItem.ToString()) < 0)
            {
                MessageBox.Show("燒錄時發生錯誤!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetProgress(0);
            }
            else
            {
                AppendLog("燒錄成功\r\n");
                AppendLog("費時 " + bsl.duration_time.ToString() + " 秒\r\n");
            }

            if (checkBox1.Checked)
            {
                File.Delete("_tmp_ihex.ihex");
            }

            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            comboBox1.Enabled = true;
        }

        public void SetProgress(int p)
        {
            progressBar1.Value = p;
            progressBar1.Update();
        }

        private void button_About_Click(object sender, EventArgs e)
        {
            About abdlg = new About();
            abdlg.ShowDialog();
        }

        public void AppendLog(string s)
        {
            textBox2.AppendText(s);
            textBox2.Update();
        }

        public void ClearLog()
        {
            textBox2.Clear();
            textBox2.Update();
        }

        private void WriteTOSBootCode(string s, string d)
        {
            StreamReader sr = new StreamReader(s);
            StreamWriter sw = new StreamWriter(d);

            sw.WriteLine(":10400000B240805A20013F40EE463E4000113D4004");
            sw.WriteLine(":1040100000110D9E0524FE4F00001E530E9DFB2B2C");
            sw.WriteLine(":104020003F4000113D4000110D9F0524CF4300008B");
            sw.WriteLine(":104030001F530F9DFB2B3040404030403E4000134B");
            sw.WriteLine(":104040003140F83832C20343B012CE41F24087FF0C");
            sw.WriteLine(":104050005700F240E0FF5600F2D080FF1E00F2D081");
            sw.WriteLine(":1040600010001E00F2D080FF1D000E435F42700062");
            sw.WriteLine(":104070007FF025007F902500A5244E930324F2F0C5");
            sw.WriteLine(":10408000DAFF7000D2437000F2D016007000F2D058");
            sw.WriteLine(":10409000A2FF7100E2437400C2437500C243730083");
            sw.WriteLine(":1040A000F2D040000400D2C370000E433F433F53A0");
            sw.WriteLine(":1040B000FE231E532E92FA3BC29328000E38B012F4");
            sw.WriteLine(":1040C000CE41B01212424E437F400700B01242422E");
            sw.WriteLine(":1040D000B140F800060012D106003B407010B0124B");
            sw.WriteLine(":1040E000BA424F9302243B40F0100D410F4B3E402B");
            sw.WriteLine(":1040F0000500ED4F00001F531D533E533E93F9231F");
            sw.WriteLine(":104100005E4105005EB304207ED0FDFFC14E050078");
            sw.WriteLine(":10411000B2403002A001B2400002A201F2409BFF77");
            sw.WriteLine(":1041200080003F4000363F50E5FFFD23A2D3A001B1");
            sw.WriteLine(":1041300092D3A00192B3A401FD27A2C3A0018243A0");
            sw.WriteLine(":10414000A0010F43B290670E400101281F437FF387");
            sw.WriteLine(":104150004F9306205F43B012E042B01218431D3C5B");
            sw.WriteLine(":10416000D1530400E19304001F287F400700B012E0");
            sw.WriteLine(":10417000E0420E433F400F00B01234436F931124CE");
            sw.WriteLine(":10418000B0121843F1430400E1D305003D4006009E");
            sw.WriteLine(":104190000E413F407000B0123046B0121242B012D1");
            sw.WriteLine(":1041A000004882432001EC3F3D4006000E413F4065");
            sw.WriteLine(":1041B0007000B0123046E1B30500E2232E411F41EA");
            sw.WriteLine(":1041C0000200DA3F1E43593F31523040C646F240AA");
            sw.WriteLine(":1041D0000E001B00F240E0FF2200C2432100F2402B");
            sw.WriteLine(":1041E0007B002A00F24010002900F240F1FF1A0083");
            sw.WriteLine(":1041F000C2431900F240FDFF1E00F240DDFF1D002A");
            sw.WriteLine(":10420000F2433200F2433100F2433600C24335003C");
            sw.WriteLine(":104210003041F2F0EFFF1D00F240B9FF7700D2B35A");
            sw.WriteLine(":10422000710005241F437FF34F93F927023C0F438E");
            sw.WriteLine(":10423000FA3FF2D010001D00F2F0BFFF04001F4350");
            sw.WriteLine(":1042400030410B120A120912494F4A4E3B400006F8");
            sw.WriteLine(":104250004F49B01280420F4B0B9302243F53FE2371");
            sw.WriteLine(":104260004F4AB01280423F4000060F8B02243F535A");
            sw.WriteLine(":10427000FE232B821B93EC3739413A413B413041BD");
            sw.WriteLine(":104280004E4F6FF31624F2F0DFFF31006EB20D24B3");
            sw.WriteLine(":10429000F2F0BFFF31005EB30424F2F0EFFF310013");
            sw.WriteLine(":1042A0003041F2D0100031003041F2D040003100F6");
            sw.WriteLine(":1042B000F23FF2D020003100E93F5E427F105F42C2");
            sw.WriteLine(":1042C000FF107E930B247F9307240D434E8F023003");
            sw.WriteLine(":1042D0000F4D30411D43FC3F0F4330411F433041E0");
            sw.WriteLine(":1042E0000B120A124A4F7B4003004F4AB012804221");
            sw.WriteLine(":1042F0006E423F433F53FE237E53FB234F43B01296");
            sw.WriteLine(":1043000080426E423F433F53FE237E53FB237B5349");
            sw.WriteLine(":10431000EC233A413B4130410B127F4007004B4FA9");
            sw.WriteLine(":1043200012C34B104E4BB01242424F4B4B93F723EC");
            sw.WriteLine(":104330003B4130410B120A120912081207120612F1");
            sw.WriteLine(":104340000512041231800602084E094F3E50070044");
            sw.WriteLine(":104350000F63B0125845B012E445474FF2D0100039");
            sw.WriteLine(":104360001D004F93EB247F93E9240E480F493E50E4");
            sw.WriteLine(":1043700010000F63814E0002814F0202B140000124");
            sw.WriteLine(":1043800004023B405003444344978F280F43449713");
            sw.WriteLine(":104390008A244F9302201E43D33C38509001096376");
            sw.WriteLine(":1043A0000E480F49B0125845B0121046064FB012D1");
            sw.WriteLine(":1043B0001046074F38520963369000480424F2D063");
            sw.WriteLine(":1043C00010001D00E83F0F9302200E43B93C0F463A");
            sw.WriteLine(":1043D0008F104A4F12C30A100E480F49B0125845A9");
            sw.WriteLine(":1043E0000F463FF0FF010B410B5FB012E445CB4F8E");
            sw.WriteLine(":1043F000000016531853096337534C240F468F108F");
            sw.WriteLine(":104400007FF312C30F100A9F02200793E923F2D013");
            sw.WriteLine(":1044100010001D004F4AB01280420E4A0F430A4E50");
            sw.WriteLine(":104420000B4F3C4000020D43B012CA460C410D43F5");
            sw.WriteLine(":104430003E90FFFE2D2CB24084A52A01B24000A57B");
            sw.WriteLine(":104440002C01B24002A528018E430000B24040A5D5");
            sw.WriteLine(":1044500028013E90FEFF18240F4D0F5F0F5CAE4FFA");
            sw.WriteLine(":1044600000001D532E533D900001F32BB24000A5D8");
            sw.WriteLine(":104470002801B24010A52C011F437FF32E434F9318");
            sw.WriteLine(":104480005F240793A423A13FBE4000400000E93F02");
            sw.WriteLine(":104490000F43F33FB0121046064FB0121046074FBD");
            sw.WriteLine(":1044A00038520963AB3F1F43743F4A440F4A0F5FC2");
            sw.WriteLine(":1044B0000E4F0F431E5100021F610202B0125845F9");
            sw.WriteLine(":1044C000B012E445464FB012E4457FF38F1006DF8B");
            sw.WriteLine(":1044D000F2D010001D001E4104020F431E510002C5");
            sw.WriteLine(":1044E0001F610202B012584505430B931420F2D00D");
            sw.WriteLine(":1044F00010001D001A533C4050040B430D43B012F2");
            sw.WriteLine(":10450000EA46814E04023B405004545344973E2FE8");
            sw.WriteLine(":104510000695CB273B3FB012E4457FF38F100FE5A4");
            sw.WriteLine(":104520007E420F9309340F5F3FE021107E53F92341");
            sw.WriteLine(":10453000054F3B53F023DB3F0F5FF83F0F43293F0D");
            sw.WriteLine(":104540000F4E315006023441354136413741384132");
            sw.WriteLine(":1045500039413A413B4130410B120A120B4E0C4F8C");
            sw.WriteLine(":10456000F2F0EFFF1D004E43F240ABFF7700C29325");
            sw.WriteLine(":1045700002002F34F2F07F0002001F437FF34F93BD");
            sw.WriteLine(":10458000F6275E537E900500EF2BF2D010001D0041");
            sw.WriteLine(":10459000F2F0EFFF1D003CD000037A4018004D4AB6");
            sw.WriteLine(":1045A0008D110E4B0F4C0D93052412C30F100E10DE");
            sw.WriteLine(":1045B0001D83FB23C24E7700C29302000834F2F041");
            sw.WriteLine(":1045C0007F0002001F437FF34F93F627043C0F4305");
            sw.WriteLine(":1045D000FA3F0F43D33F7A827AB08000E0373A4106");
            sw.WriteLine(":1045E0003B4130415F427600C2437700F2B0400069");
            sw.WriteLine(":1045F00002000824F2F0BFFF02001F437FF34F9335");
            sw.WriteLine(":10460000F527023C0F43FA3F5F4276007FF33041CB");
            sw.WriteLine(":104610000B12B012E4454B4FB012E4457FF38F10FC");
            sw.WriteLine(":104620000BDFB012E445B012E4450F4B3B41304183");
            sw.WriteLine(":104630000B120A1209120B4F094D0A4EB012BA4260");
            sw.WriteLine(":104640004F932D243B5000103E4000103C40801002");
            sw.WriteLine(":10465000B24084A52A01B24000A52C01B24002A5B7");
            sw.WriteLine(":104660002801CE430000B24040A528010D430E9B17");
            sw.WriteLine(":1046700004280F4B0F590E9F0E28EE4C00001D53BF");
            sw.WriteLine(":104680001E531C533D907F00F22B6F4C5F537F9362");
            sw.WriteLine(":104690000D204F430B3CEE4A00001A53F03F3B50B5");
            sw.WriteLine(":1046A00080103E4080103C400010D23FCE4F0000B2");
            sw.WriteLine(":1046B000B24000A52801B24010A52C011F4339418A");
            sw.WriteLine(":1046C0003A413B41304102DFFE3F0E430F43083C7D");
            sw.WriteLine(":1046D00012C30D100C1002280E5A0F6B0A5A0B6BE6");
            sw.WriteLine(":0E46E0000C93F6230D93F42330413040CA466C");
            sw.WriteLine(":0400000300004000B9");

            for (; ; )
            {
                string line = sr.ReadLine();

                if (line == null)
                {
                    break;
                }

                if (line == ":00000001FF")
                {
                    break;
                }
                else
                {
                    sw.WriteLine(line);
                }
            }

            sw.WriteLine(":00000001FF");


            sr.Close();
            sw.Close();
        }

    }
}