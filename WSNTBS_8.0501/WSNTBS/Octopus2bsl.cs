using System;
using System.Collections.Generic;
using System.Text;

using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace WSNTBS
{
    class Octopus2bsl
    {
        public const byte ACTION_PROGRAM = 0x01;
        public const byte ACTION_VERIFY = 0x02;
        public const byte ACTION_ERASE_CHECK = 0x04;
        public const byte ACTION_PASSWD = 0x08;
        public const byte ACTION_ERASE_CHECK_FAST = 0x10;

        public const byte BSL_TXPWORD = 0x10; // Transmit password to boot loader
        public const byte BSL_TXBLK = 0x12; // Transmit block    to boot loader
        public const byte BSL_RXBLK = 0x14; // Receive  block  from boot loader
        public const byte BSL_MERAS = 0x18; // Erase complete FLASH memory
        public const byte BSL_SPEED = 0x20; // Change Speed
        public const byte BSL_ECHECK = 0x1C; // Erase Check Fast

        // Header Definitions:
        public const byte CMD_FAILED = 0x70;
        public const byte DATA_FRAME = 0x80;
        public const byte DATA_ACK = 0x90;
        public const byte DATA_NAK = 0xA0;

        // working comport
        public SerialPort comport;

        private byte seqNo, reqNo;
        private byte[] rxFrame = new byte[256];

        private byte[] Blkin = new byte[250]; // Receive buffer
        private byte[] Blkout = new byte[250]; // Transmit buffer 

        // Time in milliseconds until a timeout occurs:
        public int timeout = 300;
        // Factor by which the timeout after sending a frame is prolonged:
        public int prolongFactor = 10;

        private byte GetHexValue(char c)
        {
            if (c >= '0' && c <= '9')
                return (byte)(c - '0');
            else if (c >= 'A' && c <= 'Z')
                return (byte)(c - 'A' + 10);
            else if (c >= 'a' && c <= 'z')
                return (byte)(c - 'a' + 10);
            else
                return 0;
        }

        private bool hexToTxt(string hex_file, string ti_file)
        {
            try
            {
                if (!File.Exists(hex_file))
                {
                    return false;
                }

                StreamReader sr = new StreamReader(hex_file);
                StreamWriter sw = new StreamWriter(ti_file);

                int est_addr_index = -1;
                bool re_markAddr = false;
                char[] newline = new char[49];

                string line;
                while ((line = sr.ReadLine()) != null)
                {

                    int data_len = GetHexValue(line[1]) * 16 + GetHexValue(line[2]);
                    int addr_index = GetHexValue(line[3]) * 4096 + GetHexValue(line[4]) * 256 + GetHexValue(line[5]) * 16 + GetHexValue(line[6]);
                    if (line == ":00000001FF")
                        break;

                    if (addr_index == 0x0000)
                    {
                        est_addr_index = addr_index + data_len;
                        continue;
                    }

                    if (est_addr_index == -1 || est_addr_index != addr_index || re_markAddr)
                    {
                        sw.WriteLine("@" + line.Substring(3, 4));
                        re_markAddr = false;
                    }

                    for (int k = 0; k < 49; ++k)
                        newline[k] = ' ';

                    for (int k = 0; k < data_len; ++k)
                    {
                        newline[k * 3] = line[9 + k * 2];
                        newline[k * 3 + 1] = line[9 + k * 2 + 1];
                    }

                    if (data_len < 16)
                    {
                        re_markAddr = true;
                    }

                    est_addr_index = addr_index + data_len;
                    newline[data_len * 3 - 1] = '\r';
                    newline[data_len * 3] = '\n';

                    sw.Write(newline, 0, data_len * 3 + 1);

                }
                sw.Write("q");


                sr.Close();
                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
            return true;
        }

        private bool comInit(string lpszDevice)
        {
            try
            {
                seqNo = 0;
                reqNo = 0;

                comport = new SerialPort(lpszDevice, 9600, Parity.Even, 8, StopBits.One);
                comport.ReadBufferSize = 512;
                comport.WriteBufferSize = 512;

                comport.Open();

                comport.DtrEnable = true;
                comport.RtsEnable = true;

                comport.DiscardInBuffer();
                comport.DiscardOutBuffer();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void delay(int time)
        {
            Thread.Sleep(time);
        }

        private void SetRSTpin(bool level)
        {
            comport.DtrEnable = level ? false : true;
        }

        private void noADG_BSL()
        {
            SetRSTpin(false);
            SetTESTpin(false);
            delay(10);
            SetRSTpin(true);
            SetTESTpin(false);
            delay(10);
            SetRSTpin(true);
            SetTESTpin(true);
            delay(10);
            SetRSTpin(false);
            SetTESTpin(true);
            delay(10);
            SetTESTpin(false);
            delay(10);
            SetTESTpin(true);
            delay(10);
            SetTESTpin(false);
            delay(10);
            SetRSTpin(true);
            delay(10);
            SetTESTpin(true);
        }

        private void SetRSTpin_BSL(bool level)
        {
            comport.DtrEnable = level ? false : true;
        }

        private void SetTESTpin(bool level)
        {
            comport.RtsEnable = level ? false : true;
        }

        private void bslReset(bool invokeBSL)
        {
            SetRSTpin(true);
            SetTESTpin(true);
            delay(250);

            if (invokeBSL)
            {
                noADG_BSL();
            }
            else
            {
                SetRSTpin_BSL(false);
                delay(10);
                SetRSTpin_BSL(true);
            }

            delay(250);

            comport.DiscardOutBuffer();
            comport.DiscardInBuffer();
        }

        private int calcTimeout(int startTime)
        {
            return (Environment.TickCount - startTime);
        }

        private short calcChecksum(byte[] data, int length)
        {
            short i_data;
            short checksum = 0;

            for (int i = 1; i < length; i += 2)
            {
                i_data = (short)((data[i] << 8) | data[i - 1]);
                checksum ^= i_data;
            }

            return (short)(checksum ^ 0xffff);
        }

        private int comWaitForData(int count, int timeout)
        {
            int rxCount = 0;
            int startTime = Environment.TickCount;

            while (((rxCount = comport.BytesToRead) < count) &&
                     (calcTimeout(startTime) <= timeout)) ;

            return (rxCount);
        }

        private bool bslSync()
        {
            byte[] ch = new byte[1];

            for (int loopcnt = 0; loopcnt < 3; ++loopcnt)
            {
                comport.DiscardInBuffer();
                ch[0] = 0x80;
                comport.Write(ch, 0, 1);

                int rxCount = comWaitForData(1, 100);

                if (rxCount > 0)
                {
                    ch[0] = (byte)comport.ReadByte();
                    if (ch[0] == DATA_ACK)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool comRxHeader(ref byte rxHeader, ref byte rxNum, int timeout)
        {
            if (comWaitForData(1, timeout) >= 1)
            {
                byte Hdr = (byte)comport.ReadByte();
                rxHeader = (byte)(Hdr & 0xf0);
                rxNum = (byte)(Hdr & 0x0f);

                reqNo = 0;
                seqNo = 0;
                rxNum = 0;

                return true;
            }
            else
            {
                rxHeader = 0;
                rxNum = 0;
                return false;
            }
        }

        private bool comRxFrame(ref byte rxHeader, ref byte rxNum)
        {
            rxFrame[0] = (byte)(DATA_FRAME | rxNum);

            if (comWaitForData(3, timeout) >= 3)
            {
                comport.Read(rxFrame, 1, 3);

                if ((rxFrame[1] == 0) && (rxFrame[2] == rxFrame[3]))
                {
                    short rxLengthCRC = (short)(rxFrame[2] + 2);

                    if (comWaitForData(rxLengthCRC, timeout) >= rxLengthCRC)
                    {
                        comport.Read(rxFrame, 4, rxLengthCRC);
                        short checksum = calcChecksum(rxFrame, (int)(rxFrame[2] + 4));

                        if ((rxFrame[rxFrame[2] + 4] == (byte)checksum) &&
                            (rxFrame[rxFrame[2] + 5] == (byte)(checksum >> 8)))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool comTxRx(byte cmd, byte[] dataOut, byte length)
        {
            byte[] txFrame = new byte[256];
            short checksum = 0;
            int k = 0;
            int errCtr = 0;
            byte rxHeader = 0;
            byte rxNum = 0;
            int resentFrame = 0;

            // Prepare data for transmit
            if ((length % 2) != 0)
            {
                dataOut[length++] = 0xFF;
            }

            txFrame[0] = (byte)(DATA_FRAME | seqNo);
            txFrame[1] = cmd;
            txFrame[2] = length;
            txFrame[3] = length;

            reqNo = (byte)((seqNo + 1) % 16);

            //memcpy(&txFrame[4], dataOut, length);
            for (int i = 0; i < length; ++i)
            {
                txFrame[i + 4] = dataOut[i];
            }


            checksum = calcChecksum(txFrame, (int)(length + 4));
            txFrame[length + 4] = (byte)(checksum);
            txFrame[length + 5] = (byte)(checksum >> 8);

            k = 0;

            comport.DiscardInBuffer();

            do
            {
                comport.Write(txFrame, k++, 1);

            } while ((k < length + 6) && (comport.BytesToRead == 0));

            rxFrame[2] = 0;
            rxFrame[3] = 0;

            do
            {
                if (comRxHeader(ref rxHeader, ref rxNum, timeout * prolongFactor))
                {
                    do
                    {
                        resentFrame = 0;

                        switch (rxHeader)
                        {
                            case DATA_ACK:

                                if (rxNum == reqNo)
                                {
                                    seqNo = reqNo;
                                    return true;
                                }
                                break;

                            case DATA_NAK:
                                return false;

                            case DATA_FRAME:

                                if (rxNum == reqNo)
                                {
                                    if (comRxFrame(ref rxHeader, ref rxNum))
                                    {
                                        return true;
                                    }

                                }
                                break;

                            case CMD_FAILED:
                                return false;

                            default:
                                break;
                        }

                        errCtr = 5;
                    } while ((resentFrame == 0) && (errCtr < 5));
                }
                else
                {
                    errCtr = 5;
                }
            } while (errCtr < 5);

            return false;
        }

        public bool bslTxRx(byte cmd, uint addr, int len, byte[] blkout, byte[] blkin)
        {
            byte[] dataOut = new byte[256];
            int length = 4;

            if (cmd == BSL_TXBLK)
            {
                if ((addr % 2) != 0)
                {
                    addr--;
                    for (int i = len; i >= 1; --i)
                    {
                        blkout[i] = blkout[i - 1];
                    }

                    blkout[0] = 0xFF;
                    ++len;
                }
                if ((len % 2) != 0)
                {
                    blkout[len++] = 0xFF;
                }
            }

            if (cmd == BSL_RXBLK)
            {
                if ((addr % 2) != 0)
                {
                    addr--;
                    len++;
                }
                if ((len % 2) != 0)
                {
                    len++;
                }
            }

            if ((cmd == BSL_TXBLK) || (cmd == BSL_TXPWORD))
            {
                length = len + 4;
            }

            dataOut[0] = (byte)(addr & 0x00ff);
            dataOut[1] = (byte)((addr >> 8) & 0x00ff);
            dataOut[2] = (byte)(len & 0x00ff);
            dataOut[3] = (byte)((len >> 8) & 0x00ff);

            if (blkout != null)
            {
                for (int i = 0; i < len; ++i)
                {
                    dataOut[4 + i] = blkout[i];
                }
            }

            if (!bslSync())
            {
                return false;
            }

            bool no_error = comTxRx(cmd, dataOut, (byte)length);

            if (blkin != null)
            {
                //memcpy(blkin, &rxFrame[4], rxFrame[2]);
                for (int i = 0; i < rxFrame[2]; ++i)
                {
                    blkin[i] = rxFrame[i + 4];
                }
            }

            return no_error;
        }

        private bool txPasswd()
        {
            for (int i = 0; i < 0x20; i++)
            {
                Blkout[i] = 0xff;
            }
            return (bslTxRx(BSL_TXPWORD, 0xffe0, 0x0020, Blkout, Blkin));
        }

        private bool verifyBlk(uint addr, short len, byte action)
        {
            if ((action & (ACTION_VERIFY | ACTION_ERASE_CHECK)) != 0)
            {
                if (!bslTxRx(BSL_RXBLK, addr, len, null, Blkin))
                {
                    return false;
                }
                else
                {
                    for (int i = 0; i < len; i++)
                    {
                        if ((action & ACTION_VERIFY) != 0)
                        {
                            if (Blkin[i] != Blkout[i])
                            {
                                return false;
                            }
                            continue;
                        }
                        if ((action & ACTION_ERASE_CHECK) != 0)
                        {
                            if (Blkin[i] != 0xff)
                            {
                                return false;
                            }
                            continue;
                        }
                    }
                }
            }
            /*
	        else if ((action & ACTION_ERASE_CHECK_FAST) != 0) 
		    {
		
		        no_error= bslTxRx(BSL_ECHECK, addr, len, null, Blkin);
		
		        if ( !no_error ) 
			    { 
			        return false; 
			    }
		    }*/

            return true;
        }

        private bool programBlk(uint addr, short len, byte action)
        {
            if ((action & ACTION_PASSWD) != 0)
            {
                return (bslTxRx(BSL_TXPWORD, addr, len, Blkout, Blkin));
            }

            if ((action & ACTION_ERASE_CHECK) != 0)
            {
                if (!verifyBlk(addr, len, (byte)(action & ACTION_ERASE_CHECK)))
                    return false;
            }
            else if ((action & ACTION_ERASE_CHECK_FAST) != 0)
            {
                if (!verifyBlk(addr, len, (byte)(action & ACTION_ERASE_CHECK_FAST)))
                    return false;
            }

            if ((action & ACTION_PROGRAM) != 0)
            {
                if (!bslTxRx(BSL_TXBLK, addr, len, Blkout, Blkin))
                    return false;
            }
            return verifyBlk(addr, len, (byte)(action & ACTION_VERIFY));
        }

        private bool programTIText(string filename, byte action)
        {
            bool no_error = true;
            uint currentAddr = 0;
            short dataframelen = 0;

            if (!File.Exists(filename))
            {
                return false;
            }

            StreamReader infile = new StreamReader(filename);

            for (; ; )
            {
                string strdata = infile.ReadLine();

                if (strdata == null || strdata[0] == 'q')
                {
                    if (dataframelen > 0)
                    {
                        no_error = programBlk(currentAddr, dataframelen, action);
                        dataframelen = 0;
                    }
                    break;
                }

                int linelen = strdata.Length;

                if (strdata[0] == '@')
                {
                    if (dataframelen > 0)
                    {
                        no_error = programBlk(currentAddr, dataframelen, action);
                        dataframelen = 0;
                    }
                    currentAddr = (uint)(GetHexValue(strdata[1]) * 4096 + GetHexValue(strdata[2]) * 256 + GetHexValue(strdata[3]) * 16 + GetHexValue(strdata[4]));
                    continue;
                }

                for (int linepos = 0; linepos < linelen - 1; linepos += 3, dataframelen++)
                {
                    Blkout[dataframelen] = (byte)(GetHexValue(strdata[linepos]) * 16 + GetHexValue(strdata[linepos + 1]));
                }

                if (dataframelen > 240 - 16) //maxData - 16)
                {
                    no_error = programBlk(currentAddr, dataframelen, action);
                    currentAddr += (uint)dataframelen;
                    dataframelen = 0;
                }

                if (!no_error)
                {
                    break;
                }

            }

            infile.Close();

            return no_error;
        }


        public int program_start(string source_ihex_file, string port)
        {
            if (port == "")
                return 1;

            if ( !File.Exists(source_ihex_file))
            {
                return -1;
            }

            if (!hexToTxt(source_ihex_file, "temp\\_tmp_" + port + "_it.txt"))
            {
                return -2;
            }
            
            if (!comInit(port))
            {
                return -3;
            }

            bslReset(true);

            if (!bslTxRx(BSL_MERAS, 0xff00, 0xa506, null, Blkin))
            {
                comport.Close();
                return -4;
            }

            if (!txPasswd())
            {
                comport.Close();
                return -5;
            }

            if (!bslTxRx(BSL_RXBLK, 0x0ff0, 14, null, Blkin))
            {
                comport.Close();
                return -6;
            }

            if (!bslTxRx(BSL_SPEED, (0x87 << 8) | 0xE0, 2, null, Blkin))
            {
                comport.Close();
                return -7;
            }
            
            comport.BaudRate = 38400;
            delay(10);

            if (!programTIText("temp\\_tmp_" + port + "_it.txt", ACTION_ERASE_CHECK))
            {
                comport.Close();
                return -8;
            }

            if (!programTIText("temp\\_tmp_" + port + "_it.txt", ACTION_PROGRAM))
            {
                comport.Close();
                return -9;
            }

            bslReset(false);
            comport.Close();

            File.Delete("temp\\_tmp_" + port + "_it.txt");

            return 0 ;
        }

        public int Reset(string port)
        {
            if (port == "")
                return 1;

            try
            {

                if (!comInit(port))
                {
                    return -3;
                }

                bslReset(false);
                comport.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return -3;
            }
            return 0;
        }
    }
}
