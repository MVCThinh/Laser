using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;

namespace MX_IF
{
    public partial class MXIF : Form
    {
        #region DLL

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, ref Win32API.COPYDATASTRUCT lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        #endregion DLL

        #region Tham số
        public struct sPE
        {
            public int Connection;
            public int SensorData;
            public int GPOData;
            public int Offset;
            public int Distance;
        }

        private enum PENum
        {
            CHA,
            CHB,
            CHC,
        }

        private string cINIPath = "C:/EQData/INI/Config.ini";
        private cConfig cConfig = new cConfig();
        private csLog csLog = new csLog();

        //20.04.03 Height Sensor socket 추가
        private csSocketClient HeightSocket = new csSocketClient();

        private SerialPort FFUSP = new SerialPort();
        private SerialPort spLeftGMS = new SerialPort();
        private SerialPort spRightGMS = new SerialPort();
        private SerialPort spPE = new SerialPort();

        //210115 cjm 전력량계 통신 프로그램 추가
        private csAccura Accura;
        public NetworkStream[] nsUPS = new NetworkStream[1];

        public static class Win32API
        {
            public const Int32 WM_COPYDATA = 0x004A;

            public struct COPYDATASTRUCT
            {
                public IntPtr dwData;
                public int cbData;

                [MarshalAs(UnmanagedType.LPStr)]
                public string lpData;
            }
        }

        private bool m_bSendMsgRetry = false;
        private bool m_bSendMsgRetry1 = false;
        private int m_nSendMsgRetryCount = 0;
        private int m_nSendMsgRetryCount1 = 0;

        private bool bGMS;
        private bool bPE;
        private bool bFFU;
        #endregion
        public MXIF()
        {
            // Đọc dữ liệu từ File Config.
            cConfig.LoadConfig();
           // Accura = new csAccura();
            InitializeComponent();
            #region Hiển thị Label, đọc dữ liệu từ File Config.ini
            lbVersion.Text = "2021.08.19_LaserMarking";
            PLCType = int.Parse(INIFileRead("Config", "PLCTYPE"));
            PCNo = INIFileRead("Config", "PCNO");
            PLCDevice = INIFileRead("Config", "PLCDEVICE");
            PLCIP = INIFileRead("Config", "PLCIP");
            StationNO = int.Parse(INIFileRead("Config", "STATIONNO"));
            NetworkNO = int.Parse(INIFileRead("Config", "NETWORKNO"));
            #endregion
            #region Hiển thị Tab Height, Kết nối SocketClient, Hàm sự kiện nhận dữ liệu.
            if (PCNo == "4" && cConfig.HeightIPAddress.Contains("."))
            {
                HeightSocket.ConnectClient(cConfig.HeightIPAddress, int.Parse(cConfig.HeightPort));
                HeightSocket.RcvData += new csSocketClient.SocketEventHandler(HeightRcvData);
            }
            else
            {
                tabControl1.TabPages.Remove(tpHeight);
            }
            #endregion
            // Đọc dữ liệu địa chỉ từ File Address_
            Address_set();


            //tab순서 log, ffu, gms, pre, height
            //COM포트 적어놓은것만 시리얼포트 연결
            #region Hiển thị Tab FFU, Mở Port FFU, , Hiện thị dữ liêu FFU
            if (cConfig.FFU_SP.Contains("COM"))
            {
                FFUPORTOPEN();
                for (int i = 0; i < cConfig.FFU_CNT; i++)
                {
                    dgvFFU.Rows.Add();
                    dgvFFU.Rows[i].HeaderCell.Value = "FFU" + (i + 1).ToString();
                }
            }
            else
            {
                tabControl1.TabPages.Remove(tpFFU);
            }
            #endregion
            #region Hiển thị Tab GMS, Mở Port GMS, , Hiện thị dữ liêu GMS
            if (cConfig.LGMSPort.Contains("COM") || cConfig.RGMSPort.Contains("COM"))
            {
                GMSPortOpen();
                GMSDispSet();
            }
            else
            {
                tabControl1.TabPages.Remove(tpGMS);
            }
            #endregion
            #region Hiển thị Tab PRE, Mở Port PRE, , Hiện thị dữ liêu PRE
            if (cConfig.PEPort.Contains("COM"))
            {
                PEPortOpen();
                PREDispSet();
            }
            else
            {
                tabControl1.TabPages.Remove(tpPRE);
            }
            #endregion

            label5.BackColor = Color.Aqua;

            Accura = new csAccura(cConfig.UPSIP, cConfig.GPSIP);
            #region Hiển thị Tab UPS, Mở Port UPS, , Hiện thị dữ liêu UPS

            if (cConfig.UPSIP.Contains("192") || cConfig.GPSIP.Contains("192"))
            {
                //Accura = new csAccura(cConfig.UPSIP, cConfig.GPSIP);

                if (Accura.bCheckUPS && !Accura.bCheckGPS)
                {
                    gbUPS.Location = new Point(205, 44);
                    gbGPS.Visible = false;
                }
                else if (!Accura.bCheckUPS && Accura.bCheckGPS)
                {
                    gbGPS.Location = new Point(205, 44);
                    gbUPS.Visible = false;
                }

                //20201224 cjm 전력량계 통신 프로그램 추가
                if (Accura.bCheckUPS || Accura.bCheckGPS)
                {
                    tmrUPS.Enabled = true;
                }
            }
            else
            {
                tabControl1.TabPages.Remove(tbUPS);
            }
            #endregion
            if (Connection() == 0)
                tmrIF.Enabled = true;
        }

        private static bool bWin_com = false;
        private static bool bOLD_Wincom = false;
        private static int Win_Count = 0;         //Window Connection Check Count

        #region Mở Port GMS
        private void GMSPortOpen()
        {
            bGMS = true;
            try
            {
                if (!spLeftGMS.IsOpen && cConfig.LGMSPort.Contains("COM"))
                {
                    spLeftGMS.BaudRate = 19200;
                    spLeftGMS.DataBits = 8;
                    spLeftGMS.Parity = Parity.None;
                    spLeftGMS.StopBits = StopBits.One;
                    spLeftGMS.PortName = cConfig.LGMSPort;
                    spLeftGMS.DataReceived += new SerialDataReceivedEventHandler(spLeftGMS_DataReceived);
                    spLeftGMS.Open();
                }
                if (!spRightGMS.IsOpen && cConfig.RGMSPort.Contains("COM"))
                {
                    spRightGMS.BaudRate = 19200;
                    spRightGMS.DataBits = 8;
                    spRightGMS.Parity = Parity.None;
                    spRightGMS.StopBits = StopBits.One;
                    spRightGMS.PortName = cConfig.RGMSPort;
                    spRightGMS.DataReceived += new SerialDataReceivedEventHandler(spRightGMS_DataReceived);
                    spRightGMS.Open();
                }
            }
            catch
            {
            }
        }
        #endregion
        private void GMSDataSend(bool Left = false)
        {
            byte[] lcSendbyte = new byte[41];

            lcSendbyte[0] = 0x10;//DLE
            lcSendbyte[1] = 0x02;//STX

            lcSendbyte[2] = 0X4C;  //L
            lcSendbyte[3] = 0X4F;  //O
            lcSendbyte[4] = 0X41;  //A
            lcSendbyte[5] = 0X44;  //D
            lcSendbyte[6] = 0X57;  //W
            lcSendbyte[7] = 0X53;  //S

            lcSendbyte[8] = 0x20;  //
            lcSendbyte[9] = 0x20; // (sp)
            lcSendbyte[10] = 0x20;

            if (Left)
            {
                lcSendbyte[11] = 0x31; // 1
                lcSendbyte[12] = 0x31;
                lcSendbyte[13] = 0x31;
                lcSendbyte[14] = 0x31;
                lcSendbyte[15] = 0x31;
                lcSendbyte[16] = 0x31;
                lcSendbyte[17] = 0x31;
                lcSendbyte[18] = 0x31;
                lcSendbyte[19] = 0x31;

                lcSendbyte[20] = 0x31; lcSendbyte[21] = 0x31;
                lcSendbyte[22] = 0x31; lcSendbyte[23] = 0x31;
                lcSendbyte[24] = 0x31; lcSendbyte[25] = 0x31;
                lcSendbyte[26] = 0x31; lcSendbyte[27] = 0x31;
                lcSendbyte[28] = 0x31; lcSendbyte[29] = 0x31;
                lcSendbyte[30] = 0x31; lcSendbyte[31] = 0x31;
                lcSendbyte[32] = 0x31; lcSendbyte[33] = 0x31;
                lcSendbyte[34] = 0x31; lcSendbyte[35] = 0x31;
                lcSendbyte[36] = 0x31; lcSendbyte[37] = 0x31;
            }
            else
            {
                lcSendbyte[11] = 0x32; // 2
                lcSendbyte[12] = 0x32;
                lcSendbyte[13] = 0x32;
                lcSendbyte[14] = 0x32;
                lcSendbyte[15] = 0x32;
                lcSendbyte[16] = 0x32;
                lcSendbyte[17] = 0x32;
                lcSendbyte[18] = 0x32;
                lcSendbyte[19] = 0x32;

                lcSendbyte[20] = 0x32; lcSendbyte[21] = 0x32;
                lcSendbyte[22] = 0x32; lcSendbyte[23] = 0x32;
                lcSendbyte[24] = 0x32; lcSendbyte[25] = 0x32;
                lcSendbyte[26] = 0x32; lcSendbyte[27] = 0x32;
                lcSendbyte[28] = 0x32; lcSendbyte[29] = 0x32;
                lcSendbyte[30] = 0x32; lcSendbyte[31] = 0x32;
                lcSendbyte[32] = 0x32; lcSendbyte[33] = 0x32;
                lcSendbyte[34] = 0x32; lcSendbyte[35] = 0x32;
                lcSendbyte[36] = 0x32; lcSendbyte[37] = 0x32;
            }

            lcSendbyte[38] = 0x20;

            for (int i = 2; i < lcSendbyte.Length - 2; i++)
            {
                lcSendbyte[39] ^= lcSendbyte[i];
            }
            if (lcSendbyte[39] == 0x00 || lcSendbyte[39] == 0x10 || lcSendbyte[39] == 0x0D)
            {
                lcSendbyte[39] = 0xFF;
            }
            lcSendbyte[40] = 0x03;

            try
            {
                if (Left)
                {
                    if (spLeftGMS.IsOpen) spLeftGMS.Write(lcSendbyte, 0, lcSendbyte.Length);
                }
                else
                {
                    if (spRightGMS.IsOpen) spRightGMS.Write(lcSendbyte, 0, lcSendbyte.Length);
                }
            }
            catch { }
        }

        private List<byte> GMSRcvData = new List<byte>();

        private void spLeftGMS_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string TempMsg = "";
            int startbyte = 38;
            int PortDataLength = 4;
            int PortCount = 5;
            int SendDataLength = 10;
            int count = 0;
            double[] GMSData = new double[PortCount];
            int[] iData = new int[10];
            try
            {
                //pcy200616 데이터 끊겨오는것 합치기 추가
                int DataLength = spLeftGMS.BytesToRead;
                bool bEnd = false;
                byte[] lcRcvData = new byte[DataLength];
                spLeftGMS.Read(lcRcvData, 0, DataLength);
                foreach (var s in lcRcvData)
                {
                    switch (s)
                    {
                        case 0x10: //DLE
                            GMSRcvData.Clear();
                            break;

                        case 0x03: //ETX
                            bEnd = true;
                            break;
                    }
                    GMSRcvData.Add(s);
                }
                if (GMSRcvData.Count > 1000) GMSRcvData.Clear(); //무작정 길어지면 클리어

                //int DataLength = spLeftGMS.BytesToRead;
                if (bEnd)//DataLength == 60)
                {
                    int idd = 0;
                    //byte[] lcRcvData = new byte[DataLength];
                    //spLeftGMS.Read(lcRcvData, 0, DataLength);

                    for (int j = 0; j < PortCount; j++)
                    {
                        count = j * PortDataLength;
                        TempMsg = "";
                        for (int k = 0; k < PortDataLength; k++)
                        {
                            idd = Convert.ToInt16(GMSRcvData[startbyte + count + k]);
                            char code = Convert.ToChar(idd);
                            TempMsg += code.ToString();
                        }
                        //pcy200704 단위맞춤
                        GMSData[j] = int.Parse(TempMsg);
                        GMS1_Data[j] = double.Parse(TempMsg) / 100;
                    }

                    iData[0] = Convert.ToInt32(GMSData[0]) & 0xFFFF;
                    iData[1] = (int)((Convert.ToInt32(GMSData[0]) & 0xFFFF0000) / 0x10000);
                    iData[2] = Convert.ToInt32(GMSData[1]) & 0xFFFF;
                    iData[3] = (int)((Convert.ToInt32(GMSData[1]) & 0xFFFF0000) / 0x10000);
                    iData[4] = Convert.ToInt32(GMSData[2]) & 0xFFFF;
                    iData[5] = (int)((Convert.ToInt32(GMSData[2]) & 0xFFFF0000) / 0x10000);
                    iData[6] = Convert.ToInt32(GMSData[3]) & 0xFFFF;
                    iData[7] = (int)((Convert.ToInt32(GMSData[3]) & 0xFFFF0000) / 0x10000);
                    iData[8] = Convert.ToInt32(GMSData[4]) & 0xFFFF;
                    iData[9] = (int)((Convert.ToInt32(GMSData[4]) & 0xFFFF0000) / 0x10000);

                    actIF.WriteDeviceBlock("ZR" + Address.PC.GMS, SendDataLength, ref iData[0]);
                }
            }
            catch
            {
                if (spLeftGMS.IsOpen) spLeftGMS.Close();
            }
        }
        private List<byte> GMSRcvData2 = new List<byte>();
        private void spRightGMS_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string TempMsg = "";
            int startbyte = 38;
            int PortDataLength = 4;
            int PortCount = 5;
            int SendDataLength = 10;
            int count = 0;
            int[] GMSData = new int[PortCount];
            int[] iData = new int[10];
            try
            {
                //pcy200616 데이터 끊겨오는것 합치기 추가
                int DataLength = spRightGMS.BytesToRead;
                bool bEnd = false;
                byte[] lcRcvData = new byte[DataLength];
                spRightGMS.Read(lcRcvData, 0, DataLength);
                foreach (var s in lcRcvData)
                {
                    switch (s)
                    {
                        case 0x10: //DLE
                            GMSRcvData2.Clear();
                            break;

                        case 0x03: //ETX
                            bEnd = true;
                            break;
                    }
                    GMSRcvData2.Add(s);
                }
                if (GMSRcvData2.Count > 1000) GMSRcvData2.Clear(); //무작정 길어지면 클리어

                //int DataLength = spRightGMS.BytesToRead;
                if (bEnd)//DataLength == 60)
                {
                    int idd = 0;
                    //byte[] lcRcvData = new byte[DataLength];
                    //spRightGMS.Read(lcRcvData, 0, DataLength);

                    for (int j = 0; j < PortCount; j++)
                    {
                        count = j * PortDataLength;
                        TempMsg = "";
                        for (int k = 0; k < PortDataLength; k++)
                        {
                            idd = Convert.ToInt16(GMSRcvData2[startbyte + count + k]);
                            char code = Convert.ToChar(idd);
                            TempMsg += code.ToString();
                        }
                        //pcy200704 단위맞춤
                        GMSData[j] = int.Parse(TempMsg);
                        GMS2_Data[j] = double.Parse(TempMsg) / 100;
                    }

                    iData[0] = Convert.ToInt32(GMSData[0]) & 0xFFFF;
                    iData[1] = (int)((Convert.ToInt32(GMSData[0]) & 0xFFFF0000) / 0x10000);
                    iData[2] = Convert.ToInt32(GMSData[1]) & 0xFFFF;
                    iData[3] = (int)((Convert.ToInt32(GMSData[1]) & 0xFFFF0000) / 0x10000);
                    iData[4] = Convert.ToInt32(GMSData[2]) & 0xFFFF;
                    iData[5] = (int)((Convert.ToInt32(GMSData[2]) & 0xFFFF0000) / 0x10000);
                    iData[6] = Convert.ToInt32(GMSData[3]) & 0xFFFF;
                    iData[7] = (int)((Convert.ToInt32(GMSData[3]) & 0xFFFF0000) / 0x10000);
                    iData[8] = Convert.ToInt32(GMSData[4]) & 0xFFFF;
                    iData[9] = (int)((Convert.ToInt32(GMSData[4]) & 0xFFFF0000) / 0x10000);

                    actIF.WriteDeviceBlock("ZR" + (Address.PC.GMS + 10), SendDataLength, ref iData[0]);
                }
            }
            catch
            {
                if (spRightGMS.IsOpen) spRightGMS.Close();
            }
        }

        private void GMSDisp()
        {
            for (int i = 0; i < GMSPortCount; i++)
            {
                dgvGMS[1, i].Value = (GMS1_Data[i]).ToString();
                dgvGMS[3, i].Value = (GMS2_Data[i]).ToString();
            }
        }

        private void PEPortOpen()
        {
            bPE = true;
            try
            {
                if (!spPE.IsOpen)
                {
                    spPE.BaudRate = 19200;
                    spPE.DataBits = 8;
                    spPE.Parity = Parity.None;
                    spPE.StopBits = StopBits.One;
                    spPE.PortName = cConfig.PEPort;
                    spPE.DataReceived += new SerialDataReceivedEventHandler(spPE_DataReceived);
                    spPE.Open();
                }
            }
            catch { }
        }

        private void PEDataSend()
        {
            byte[] lcSendbyte = new byte[8];

            lcSendbyte[0] = 0x24;//Start Code $
                                 //lcSendbyte[1] = 0x31;//Device ID 1
                                 //lcSendbyte[2] = 0X33;//Comand Length
            lcSendbyte[1] = 0x01;//Device ID 1
            lcSendbyte[2] = 0X03;//Comand Length

            lcSendbyte[3] = 0X52;//R
            lcSendbyte[4] = 0X45;//E
            lcSendbyte[5] = 0X51;//Q

            for (int i = 0; i < lcSendbyte.Length - 2; i++)
            {
                lcSendbyte[6] ^= lcSendbyte[i];//Check Sum
            }

            lcSendbyte[7] = 0x2A;// End Code *

            try
            {
                if (spPE.IsOpen) spPE.Write(lcSendbyte, 0, lcSendbyte.Length);
            }
            catch { }
        }

        private List<byte> PERcvData = new List<byte>();

        private void spPE_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                //pcy200616 데이터 끊겨오는것 합치기 추가
                int DataLength = spPE.BytesToRead;
                int[] iData = new int[6];
                bool bEnd = false;
                byte[] lcRcvData = new byte[DataLength];
                spPE.Read(lcRcvData, 0, DataLength);
                foreach (var s in lcRcvData)
                {
                    switch ((char)s)
                    {
                        case '$':
                            PERcvData.Clear();
                            break;

                        case '*':
                            bEnd = true;
                            break;
                    }
                    PERcvData.Add(s);
                }
                if (PERcvData.Count > 1000) PERcvData.Clear(); //무작정 길어지면 클리어
                if (bEnd)//DataLength == 29)
                {
                    string[] DataBuffer = new string[3];

                    DataBuffer[(int)PENum.CHA] = Convert.ToString(Convert.ToInt16(PERcvData[3]), 2).PadLeft(8, '0') + Convert.ToString(Convert.ToInt16(PERcvData[4]), 2).PadLeft(8, '0') +
                                 Convert.ToString(Convert.ToInt16(PERcvData[5]), 2).PadLeft(8, '0') + Convert.ToString(Convert.ToInt16(PERcvData[6]), 2).PadLeft(8, '0').Substring(0, 4);
                    if (DataBuffer[(int)PENum.CHA].Substring(0, 1) == "1")
                    {
                        PE[(int)PENum.CHA].SensorData = (Convert.ToInt32(DataBuffer[(int)PENum.CHA].Substring(1), 2)) * (-1);
                    }
                    else PE[(int)PENum.CHA].SensorData = Convert.ToInt32(DataBuffer[(int)PENum.CHA], 2);
                    PE[(int)PENum.CHA].Connection = Convert.ToInt32(Convert.ToString(Convert.ToInt16(PERcvData[6]), 2).PadLeft(8, '0').Substring(7), 2);
                    PE[(int)PENum.CHA].GPOData = Convert.ToInt16(Convert.ToString(Convert.ToInt16(PERcvData[6]), 2).PadLeft(8, '0').Substring(4, 3));
                    PE[(int)PENum.CHA].Offset = (Convert.ToUInt16(PERcvData[7]) << 8) | Convert.ToUInt16(PERcvData[8]);
                    PE[(int)PENum.CHA].Distance = (Convert.ToUInt16(PERcvData[9]) << 8) | Convert.ToUInt16(PERcvData[10]);

                    DataBuffer[(int)PENum.CHB] = Convert.ToString(Convert.ToInt16(PERcvData[11]), 2).PadLeft(8, '0') + Convert.ToString(Convert.ToInt16(PERcvData[12]), 2).PadLeft(8, '0') +
                                 Convert.ToString(Convert.ToInt16(PERcvData[13]), 2).PadLeft(8, '0') + Convert.ToString(Convert.ToInt16(PERcvData[14]), 2).PadLeft(8, '0').Substring(0, 4);
                    if (DataBuffer[(int)PENum.CHB].Substring(0, 1) == "1")
                    {
                        PE[(int)PENum.CHB].SensorData = (Convert.ToInt32(DataBuffer[(int)PENum.CHB].Substring(1), 2)) * (-1);
                    }
                    else PE[(int)PENum.CHB].SensorData = Convert.ToInt32(DataBuffer[(int)PENum.CHB], 2);
                    PE[(int)PENum.CHB].Connection = Convert.ToInt32(Convert.ToString(Convert.ToInt16(PERcvData[14]), 2).PadLeft(8, '0').Substring(7), 2);
                    PE[(int)PENum.CHB].GPOData = Convert.ToInt16(Convert.ToString(Convert.ToInt16(PERcvData[14]), 2).PadLeft(8, '0').Substring(4, 3));
                    PE[(int)PENum.CHB].Offset = (Convert.ToUInt16(PERcvData[15]) << 8) | Convert.ToUInt16(PERcvData[16]);
                    PE[(int)PENum.CHB].Distance = (Convert.ToUInt16(PERcvData[17]) << 8) | Convert.ToUInt16(PERcvData[18]);

                    DataBuffer[(int)PENum.CHC] = Convert.ToString(Convert.ToInt16(PERcvData[19]), 2).PadLeft(8, '0') + Convert.ToString(Convert.ToInt16(PERcvData[20]), 2).PadLeft(8, '0') +
                                 Convert.ToString(Convert.ToInt16(PERcvData[21]), 2).PadLeft(8, '0') + Convert.ToString(Convert.ToInt16(PERcvData[22]), 2).PadLeft(8, '0').Substring(0, 4);
                    if (DataBuffer[(int)PENum.CHC].Substring(0, 1) == "1")
                    {
                        PE[(int)PENum.CHC].SensorData = (Convert.ToInt32(DataBuffer[(int)PENum.CHC].Substring(1), 2)) * (-1);
                    }
                    else PE[(int)PENum.CHC].SensorData = Convert.ToInt32(DataBuffer[(int)PENum.CHC], 2);
                    PE[(int)PENum.CHC].Connection = Convert.ToInt32(Convert.ToString(Convert.ToInt16(PERcvData[22]), 2).PadLeft(8, '0').Substring(7), 2);
                    PE[(int)PENum.CHC].GPOData = Convert.ToInt16(Convert.ToString(Convert.ToInt16(PERcvData[22]), 2).PadLeft(8, '0').Substring(4, 3));
                    PE[(int)PENum.CHC].Offset = (Convert.ToUInt16(PERcvData[23]) << 8) | Convert.ToUInt16(PERcvData[24]);
                    PE[(int)PENum.CHC].Distance = (Convert.ToUInt16(PERcvData[25]) << 8) | Convert.ToUInt16(PERcvData[26]);

                    if (PE[(int)PENum.CHA].Connection != 1) PE[(int)PENum.CHA].SensorData = 0;
                    if (PE[(int)PENum.CHB].Connection != 1) PE[(int)PENum.CHB].SensorData = 0;
                    if (PE[(int)PENum.CHC].Connection != 1) PE[(int)PENum.CHC].SensorData = 0;

                    iData[0] = Convert.ToInt32(PE[(int)PENum.CHA].SensorData) & 0xFFFF;
                    iData[1] = (int)((Convert.ToInt32(PE[(int)PENum.CHA].SensorData) & 0xFFFF0000) / 0x10000);
                    iData[2] = Convert.ToInt32(PE[(int)PENum.CHB].SensorData) & 0xFFFF;
                    iData[3] = (int)((Convert.ToInt32(PE[(int)PENum.CHB].SensorData) & 0xFFFF0000) / 0x10000);
                    iData[4] = Convert.ToInt32(PE[(int)PENum.CHC].SensorData) & 0xFFFF;
                    iData[5] = (int)((Convert.ToInt32(PE[(int)PENum.CHC].SensorData) & 0xFFFF0000) / 0x10000);

                    actIF.WriteDeviceBlock("ZR" + Address.PC.ECS.ToString(), 6, ref iData[0]);
                }
            }
            catch { }
        }

        private void GMSDispSet()
        {
            dgvGMS.Rows.Clear();
            dgvGMS.Rows.Add(GMSPortCount);
            dgvGMS[0, 0].Value = "Stage1";
            dgvGMS[0, 1].Value = "Stage2";
            dgvGMS[0, 2].Value = "Stage3";
            dgvGMS[0, 3].Value = "Stage4";
            dgvGMS[0, 4].Value = "Stage5";

            dgvGMS[2, 0].Value = "Stage6";
            dgvGMS[2, 1].Value = "Stage7";
            dgvGMS[2, 2].Value = "Stage8";
            dgvGMS[2, 3].Value = "Stage9";
            dgvGMS[2, 4].Value = "Stage10";
        }

        private void PREDispSet()
        {
            dgvPRE.Rows.Clear();
            dgvPRE.Rows.Add(GMSPortCount); ;
            dgvPRE[0, 0].Value = "Status";
            dgvPRE[0, 1].Value = "Data";
            dgvPRE[0, 2].Value = "GPO";
            dgvPRE[0, 3].Value = "Offset";
            dgvPRE[0, 4].Value = "Distance";
        }

        private void PREDisp()
        {
            try
            {
                // 20200519 add test
                if (dgvPRE.RowCount == 0)
                    dgvPRE.Rows.Add(5);

                #region Chanel A

                if (PE[(int)PENum.CHA].Connection == 1)
                {
                    dgvPRE[1, 0].Value = "Connect";
                }
                else dgvPRE[1, 0].Value = "Disconnect";
                dgvPRE[1, 1].Value = PE[(int)PENum.CHA].SensorData.ToString();
                dgvPRE[1, 2].Value = PE[(int)PENum.CHA].GPOData.ToString();
                dgvPRE[1, 3].Value = PE[(int)PENum.CHA].Offset.ToString();
                dgvPRE[1, 4].Value = PE[(int)PENum.CHA].Distance.ToString();

                #endregion Chanel A

                #region Chanel B

                if (PE[(int)PENum.CHB].Connection == 1)
                {
                    dgvPRE[2, 0].Value = "Connect";
                }
                else dgvPRE[2, 0].Value = "Disconnect";
                dgvPRE[2, 1].Value = PE[(int)PENum.CHB].SensorData.ToString();
                dgvPRE[2, 2].Value = PE[(int)PENum.CHB].GPOData.ToString();
                dgvPRE[2, 3].Value = PE[(int)PENum.CHB].Offset.ToString();
                dgvPRE[2, 4].Value = PE[(int)PENum.CHB].Distance.ToString();

                #endregion Chanel B

                #region Chanel C

                if (PE[(int)PENum.CHC].Connection == 1)
                {
                    dgvPRE[3, 0].Value = "Connect";
                }
                else dgvPRE[3, 0].Value = "Disconnect";
                dgvPRE[3, 1].Value = PE[(int)PENum.CHC].SensorData.ToString();
                dgvPRE[3, 2].Value = PE[(int)PENum.CHC].GPOData.ToString();
                dgvPRE[3, 3].Value = PE[(int)PENum.CHC].Offset.ToString();
                dgvPRE[3, 4].Value = PE[(int)PENum.CHC].Distance.ToString();

                #endregion Chanel C
            }
            catch { }
        }

        private void Address_set()
        {
            //cINIPath = "C:/EQData/INI/Address_" + PCNo + ".ini";
            cINIPath = "C:/EQData/INI/Address_.ini";

            Address.PLC.BITCONTROL = int.Parse(INIFileRead("Address_", "PLCBITCONTROL"));
            Address.PLC.MOVEREPLY = int.Parse(INIFileRead("Address_", "PLCREPLY"));
            Address.PLC.CHANGETIME = int.Parse(INIFileRead("Address_", "PLCCHANGETIME"));
            Address.PLC.CELLID1 = int.Parse(INIFileRead("Address_", "PLCCELLID1"));
            Address.PLC.CELLID2 = int.Parse(INIFileRead("Address_", "PLCCELLID2"));
            Address.PLC.CELLID3 = int.Parse(INIFileRead("Address_", "PLCCELLID3"));
            Address.PLC.CELLID4 = int.Parse(INIFileRead("Address_", "PLCCELLID4"));
            Address.PLC.CELLID5 = int.Parse(INIFileRead("Address_", "PLCCELLID5"));
            Address.PLC.CELLID6 = int.Parse(INIFileRead("Address_", "PLCCELLID6"));
            Address.PLC.CELLID7 = int.Parse(INIFileRead("Address_", "PLCCELLID7"));
            Address.PLC.CELLID8 = int.Parse(INIFileRead("Address_", "PLCCELLID8"));
            Address.PC.BITCONTROL = int.Parse(INIFileRead("Address_", "PCBITCONTROL"));
            Address.PC.CALIBRATION = int.Parse(INIFileRead("Address_", "PCCALIBRATION"));
            Address.PC.REPLY = int.Parse(INIFileRead("Address_", "PCREPLY"));
            Address.PLC.PLC_CAL_MOVE = int.Parse(INIFileRead("Address_", "PLCCALMOVE"));
            Address.PC.PC_CAL_MOVE = int.Parse(INIFileRead("Address_", "PCCALMOVE"));
            Address.PC.FFU = int.Parse(INIFileRead("Address_", "PCFFU")); //일단 주석 처리 추후 해제
            //Address.PC.ECS = int.Parse(INIFileRead("Address_", "PCECS"));
            Address.PC.GMS = int.Parse(INIFileRead("Address_", "PCGMS"));
            Address.PC.UPS = int.Parse(INIFileRead("Address_", "PCUPS"));
            Address.PC.GPS = int.Parse(INIFileRead("Address_", "PCGPS"));
        }

        public static class Address
        {
            //[PLC -> PC Start Address]
            public struct PLC
            {
                public static int BITCONTROL;
                public static int MOVEREPLY;
                public static int CHANGETIME;
                public static int CELLID1;
                public static int CELLID2;
                public static int CELLID3;
                public static int CELLID4;
                public static int CELLID5;
                public static int CELLID6;
                public static int CELLID7;
                public static int CELLID8;
                public static int PLC_CAL_MOVE;
            }

            //[PC -> PLC Start Address]
            public struct PC
            {
                public static int BITCONTROL;
                public static int CALIBRATION;
                public static int REPLY;
                public static int PC_CAL_MOVE;
                public static int FFU;
                public static int ECS;
                public static int GMS;

                public static int UPS;
                public static int GPS;
            }
        }

        private string PCNo;
        private string PLCDevice;
        private int PLCType;
        private string PLCIP;
        private int StationNO;
        private int NetworkNO;

        private IntPtr hwnd;

        private int PESendCount = 0;

        public sPE[] PE = new sPE[3];

        private double[] GMS1_Data = new double[5];
        private double[] GMS2_Data = new double[5];

        private int GMSPortCount = 5;

        private void SendData(string sMsg)
        {
            if (hwnd == IntPtr.Zero)
            {
                hwnd = FindWindow(null, "BD_IF");
            }
            if (hwnd != IntPtr.Zero)
            {
                Win32API.COPYDATASTRUCT cds = new Win32API.COPYDATASTRUCT();
                cds.dwData = (IntPtr)(1024 + 604); ; //임의값
                cds.cbData = Encoding.Default.GetBytes(sMsg).Length + 1;
                cds.lpData = sMsg;
                SendMessage(hwnd, Win32API.WM_COPYDATA, IntPtr.Zero, ref cds);
                SetlbMXLabel(sMsg);
                csLog.LogSave("[MX -> Vision] : " + sMsg);
                if (lbMX.Items.Count > 100) lbMX.Items.Clear();
            }
        }

        private void SetlbMXLabel(string sMsg)
        {
            if (lbMX.InvokeRequired)
            {
                lbMX.Invoke(new MethodInvoker(delegate
                {
                    if (lbMX.Items.Count > 100) lbMX.Items.Clear();

                    lbMX.Items.Add(sMsg);
                }));
            }
            else
            {
                lbMX.Items.Add(sMsg);
            }
        }

        private void plcLogwrite(string sdata)
        {
            lbPLC.Items.Add(sdata);
            if (lbPLC.Items.Count > 100) lbPLC.Items.Clear();
        }

        private void mxLogwrite(string sdata)
        {
            lbplcMX.Items.Add(sdata);
            if (lbplcMX.Items.Count > 100) lbplcMX.Items.Clear();
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Win32API.WM_COPYDATA:
                    Win32API.COPYDATASTRUCT cds1 = (Win32API.COPYDATASTRUCT)m.GetLParam(typeof(Win32API.COPYDATASTRUCT));
                    lbMain.Items.Add(cds1.lpData);
                    csLog.LogSave("[Vision -> MX]" + cds1.lpData);
                    label5.BackColor = Color.Aqua;
                    if (lbMain.Items.Count > 100) lbMain.Items.Clear();
                    string[] sData = cds1.lpData.Split(new char[] { ',' });
                    if (sData[0] == "*1")  // Request Data
                    {
                        hwnd = IntPtr.Zero;
                        tmrIF.Enabled = true;
                        cConfig.Vision_Init = true;
                    }
                    else if (sData[0] == "*2")  // Cal Reply Data
                    {
                        if (sData[2] == "1")
                        {
                            bCalIF = true;
                        }
                        else bCalIF = false;
                    }
                    else if (sData[0] == "RCPID")
                    {
                        short sLength = 20;
                        string rcpID = StringRecive(sData[1], sLength);   // Recipe ID Read
                        SendData(sData[0] + "," + rcpID);
                    }
                    else if (sData[0] == "RCPPARAM")
                    {
                        short sLength = 150;
                        int[] iData = new int[sLength * 2];
                        actIF.ReadDeviceBlock(sData[1], sLength * 2, out iData[0]);

                        string reply = "";
                        for (int i = 0; i < sLength; i++)
                        {
                            if (i < (sLength * 2 - 1)) reply = reply + iData[2 * i].ToString() + "," + iData[2 * i + 1].ToString() + ",";
                            else reply = reply + iData[2 * i].ToString() + "," + iData[2 * i + 1].ToString();
                        }
                        SendData(sData[0] + "," + reply);
                    }
                    else if (sData[0] == "CALDATAREAD")
                    {
                        short sLength = 62;
                        int[] iData = new int[sLength * 2];
                        actIF.ReadDeviceBlock(PLCDevice + Address.PLC.PLC_CAL_MOVE, sLength * 2, out iData[0]);

                        string reply = "";
                        for (int i = 0; i < sLength; i++)
                        {
                            if (i < (sLength - 1)) reply = reply + iData[2 * i].ToString() + "," + iData[2 * i + 1].ToString() + ",";
                            else reply = reply + iData[2 * i].ToString() + "," + iData[2 * i + 1].ToString();
                        }
                        SendData(sData[0] + "," + reply);
                        iData[0] = 1;
                        actIF.WriteDeviceBlock(PLCDevice + (Address.PC.PC_CAL_MOVE + 2), 1, ref iData[0]);
                    }
                    else if (sData[0] == "CALDATAMOVE")
                    {
                        int iData = 1;
                        actIF.WriteDeviceBlock(PLCDevice + Address.PC.PC_CAL_MOVE, 1, ref iData);
                    }
                    else if (sData[0].IndexOf("POSITION") >= 0)
                    {
                        short sLength = short.Parse(sData[2]);
                        int[] iData = new int[sLength * 2];
                        actIF.ReadDeviceBlock(sData[1], sLength * 2, out iData[0]);

                        string reply = "";
                        for (int i = 0; i < sLength; i++) //sLength * 2 -> sLength 변경 2018.07.26 khs
                        {
                            if (i < (sLength * 2 - 1)) reply = reply + iData[2 * i].ToString() + "," + iData[2 * i + 1].ToString() + ",";
                            else reply = reply + iData[2 * i].ToString() + "," + iData[2 * i + 1].ToString();
                        }
                        SendData(sData[0] + "," + reply);
                    }
                    else if (sData[0].IndexOf("BENDPRE") == 0)
                    {
                        short sLength = short.Parse(sData[2]);
                        int[] iData = new int[sLength * 2];
                        actIF.ReadDeviceBlock(sData[1], sLength * 2, out iData[0]);

                        string reply = "";
                        for (int i = 0; i < sLength; i++)
                        {
                            if (i < (sLength * 2 - 1)) reply = reply + iData[2 * i].ToString() + "," + iData[2 * i + 1].ToString() + ",";
                            else reply = reply + iData[2 * i].ToString() + "," + iData[2 * i + 1].ToString();
                        }
                        SendData(sData[0] + "," + reply);
                    }
                    else if (sData[0].IndexOf("TrOffset") == 0)
                    {
                        short sLength = short.Parse(sData[2]);
                        int[] iData = new int[sLength * 2];
                        actIF.ReadDeviceBlock(sData[1], sLength * 2, out iData[0]);

                        string reply = "";
                        for (int i = 0; i < sLength; i++)
                        {
                            if (i < (sLength * 2 - 1)) reply = reply + iData[2 * i].ToString() + "," + iData[2 * i + 1].ToString() + ",";
                            else reply = reply + iData[2 * i].ToString() + "," + iData[2 * i + 1].ToString();
                        }
                        SendData(sData[0] + "," + reply);
                    }
                    else if (sData[0] == "PNID")
                    {
                        //20.10.16 lkw 40 word 변경함.
                        short sLength = 40;
                        //if (sData[1] == "I1" || sData[1] == "I2" || sData[1] == "H1") sLength = 24;
                        string sPnData = StringRecive(sData[2], sLength);   //쓰레기 값 들어오는거때문에 24->20으로 변경
                        SendData(sData[0] + "," + sData[1] + "," + sPnData);
                    }
                    else if (sData[0] == "INSP_ASP")
                    {
                        StringSend1(sData[1], short.Parse(sData[2]), sData[3]);
                    }
                    else if (sData[0] == "LaserSendCellID") //210118 cjm 레이저 로그
                    {
                        short sLength = 100;
                        string sPnData = StringRecive(sData[2], sLength);   
                        SendData(sData[0] + "," + sData[1] + "," + sPnData);
                    }
                    else if (sData[0] == "MCRCellID")  //210118 cjm 레이저 로그
                    {
                        short sLength = 100;
                        string sPnData = StringRecive(sData[2], sLength); 
                        SendData(sData[0] + "," + sData[1] + "," + sPnData);
                    }
                    else if (sData[0] == "TIME")
                    {
                        short sLength = 14;
                        string timeData = StringRecive(sData[1], sLength);
                        SendData(sData[0] + "," + timeData);
                    }
                    else if (sData[0] == "HEIGHTSTART")
                    {
                        Height_Send("START");
                    }
                    else if (sData[0] == "HEIGHTTRIGGER")    //Height Sensor Read Start
                    {
                        Height_Send("TRIGGER");
                    }
                    else if (sData[0] == "HEIGHTSTOP")
                    {
                        Height_Send("STOP");
                    }
                    else if (sData[0] == "HEIGHTLOADJOB")
                    {
                        Height_Send("LOADJOB," + sData[1].Trim().ToString() + ".job");
                    }
                    else if (sData.Length > 2 && sData[2].IndexOf("^") > 0)   //Data가 ^으로 구분된 경우
                    {
                        string[] DataValue = sData[2].Split(new char[] { '^' });
                        int[] iData = new int[DataValue.Length];
                        for (int i = 0; i < DataValue.Length; i++)
                        {
                            iData[i] = (int)(double.Parse(DataValue[i]));
                        }
                        actIF.WriteDeviceBlock(sData[0], int.Parse(sData[1]), ref iData[0]);
                        mxLogwrite(sData[0] + "," + sData[1] + "," + sData[2]);
                    }
                    else if (sData[0].IndexOf("ZR") == 0)                                                 //Data가 , 로 구분된 경우 (1Word)
                    {
                        int[] iData = new int[sData.Length - 2];
                        for (int i = 0; i < iData.Length; i++)
                        {
                            iData[i] = int.Parse(sData[i + 2]);
                        }

                        actIF.WriteDeviceBlock(sData[0], int.Parse(sData[1]), ref iData[0]);
                        mxLogwrite(sData[0] + "," + sData[1] + "," + sData[2]);
                    }
                    else if (sData[0].IndexOf("READZR") == 0)                                                 //Data가 , 로 구분된 경우 (1Word)
                    {
                        short sLength = short.Parse(sData[3]);
                        int[] iData = new int[sLength * 2];
                        actIF.ReadDeviceBlock(sData[2], sLength * 2, out iData[0]);

                        string reply = "";
                        for (int i = 0; i < sLength; i++)
                        {
                            if (i < (sLength * 2 - 1)) reply = reply + iData[2 * i].ToString() + "," + iData[2 * i + 1].ToString() + ",";
                            else reply = reply + iData[2 * i].ToString() + "," + iData[2 * i + 1].ToString();
                        }
                        SendData(sData[0] + "," + sData[1] + "," + reply);
                    }
                    else if(sData[0] == "SETFFU")
                    {
                        if(int.TryParse(sData[1], out int ivalue) && bFFU)
                        {
                            //ffu값 변경
                             MCUL_ValueSet(ivalue);
                        }

                    }
                    break;

                default:
                    break;
            }
            base.WndProc(ref m);
        }

        //mcprotocol 사용하면 mxif 안써도됨..
        private ACTETHERLib.ActQJ71E71TCP actIF = new ACTETHERLib.ActQJ71E71TCP();
        //private ActUtlTypeLib.ActMLUtlType actIF = new ActUtlTypeLib.ActMLUtlType(); //테스트 해보기

        public int Connection()
        {
            actIF.ActConnectUnitNumber = 0;
            actIF.ActCpuType = PLCType;
            actIF.ActDestinationPortNumber = 0;
            actIF.ActDestinationPortNumber = 5002;
            actIF.ActDidPropertyBit = 1;
            actIF.ActHostAddress = PLCIP;
            actIF.ActIONumber = 1023;
            actIF.ActMultiDropChannelNumber = 0;
            actIF.ActNetworkNumber = NetworkNO;
            actIF.ActSourceNetworkNumber = NetworkNO;
            actIF.ActSourceStationNumber = StationNO;
            actIF.ActStationNumber = 1;
            actIF.ActTimeOut = 3000;
            actIF.ActUnitNumber = 0;

            //int a = actIF.Open();

            return actIF.Open();
        }

        private string INIFileRead(string Section, string Key)
        {
            StringBuilder sb = new StringBuilder(500);
            int Flag = GetPrivateProfileString(Section, Key, "", sb, 500, cINIPath);
            return sb.ToString();
        }

        private int[] oldReq = new int[16];
        private int[] oldReq1 = new int[16];

        //int oldCIMReq;
        private int[] oldCal = new int[16];

        //string oldtime;
        //string oldRcp;
        //string oldrcpid;
        //string oldAPC;

        private int PEConnectionCheck = 0;

        //private bool[] bPLCReq = new bool[32];

        private int MX_ErrorCount = 0;
        private int iMXOldMsg = 0;
        private bool bCalIF;

        private string FFU_Message;

        private int GMSSendCount = 0;
        private int GMSConnectRetryCount = 0;

        private int HeightConnectCnt = 0;

        //210115 cjm 전력량계 통신 프로그램 추가 : 
        float[] fUPSCurrent = new float[4];
        float[] fUPSVoltage = new float[8];
        float[] fUPSPower1 = new float[4];
        int[] iUPSPower2 = new int[4];

        bool bUPSCurrent = false;
        bool bUPSVoltage = false;
        bool bUPSPower = false;

        float fUPSKWtotal = 0.0f;
        int iUPSKWhsum = 0;
        float fUPSRPhaseVoltage = 0.0f;
        float fUPSTPhaseVoltage = 0.0f;
        float fUPSRPhaseCurrent = 0.0f;
        float fUPSTPhaseCurrent = 0.0f;

        //210128 cjm GPS 추가
        float[] fGPSCurrent = new float[4];
        float[] fGPSVoltage = new float[8];
        float[] fGPSPower1 = new float[4];
        int[] iGPSPower2 = new int[4];

        bool bGPSCurrent = false;
        bool bGPSVoltage = false;
        bool bGPSPower = false;

        float fGPSKWtotal = 0.0f;
        int iGPSKWhsum = 0;
        float fGPSRPhaseVoltage = 0.0f;
        float fGPSTPhaseVoltage = 0.0f;
        float fGPSRPhaseCurrent = 0.0f;
        float fGPSTPhaseCurrent = 0.0f;
        //210115 cjm 전력량계 통신프로그램 추가 : MX_IF -> PLC 로 데이터 보내주기 위해서 UPS에서 올라오는 값을 ^로 구분하여 저장해주는 곳
        private string UPS_Message;

        private void tmrIF_Tick(object sender, EventArgs e)
        {
            if (bGMS)
            {
                if (GMSSendCount > int.Parse(cConfig.GMSGetTimeDelay) * 20)
                {
                    if (spLeftGMS.IsOpen) GMSDataSend(true);
                    if (spRightGMS.IsOpen) GMSDataSend();
                    GMSSendCount = 0;
                }
                else GMSSendCount++;

                if ((!spLeftGMS.IsOpen || !spRightGMS.IsOpen) && GMSConnectRetryCount > 300)
                {
                    GMSConnectRetryCount = 0;
                    GMSPortOpen();
                }
                else GMSConnectRetryCount++;

                GMSDisp();

                if (spLeftGMS.IsOpen) lbGMSLoad.BackColor = Color.Aqua;
                else lbGMSLoad.BackColor = Color.Transparent;
                if (spRightGMS.IsOpen) lbGMSInspection.BackColor = Color.Aqua;
                else lbGMSInspection.BackColor = Color.Transparent;
            }
            if (bFFU)
            {
                #region FFU ZR Send

                if (FFU_Count > 500)
                {
                    #region FFU

                    for (int i = 0; i < cConfig.FFU_CNT; i++)
                    {
                        if (i == cConfig.FFU_CNT - 1)
                        {
                            FFU_Message = FFU_Message + cConfig.FFU_Power[i] + "^" + cConfig.FFU_Alarm[i] + "^" + cConfig.FFU_RPM[i];
                        }
                        else
                        {
                            FFU_Message = FFU_Message + cConfig.FFU_Power[i] + "^" + cConfig.FFU_Alarm[i] + "^" + cConfig.FFU_RPM[i] + "^";
                        }
                    }

                    //FFU_Message = FFU_Message + cConfig.FFU_Power[0] + "^" + cConfig.FFU_Alarm[0] + "^" + cConfig.FFU_RPM[0] + "^" + cConfig.FFU_Power[1] + "^" + cConfig.FFU_Alarm[1] + "^" + cConfig.FFU_RPM[1] + "^"
                    //                          + cConfig.FFU_Power[2] + "^" + cConfig.FFU_Alarm[2] + "^" + cConfig.FFU_RPM[2] + "^" + cConfig.FFU_Power[3] + "^" + cConfig.FFU_Alarm[3] + "^" + cConfig.FFU_RPM[3] + "^"
                    //                          + cConfig.FFU_Power[4] + "^" + cConfig.FFU_Alarm[4] + "^" + cConfig.FFU_RPM[4] + "^" + cConfig.FFU_Power[5] + "^" + cConfig.FFU_Alarm[5] + "^" + cConfig.FFU_RPM[5] + "^"
                    //                          + cConfig.FFU_Power[6] + "^" + cConfig.FFU_Alarm[6] + "^" + cConfig.FFU_RPM[6] + "^" + cConfig.FFU_Power[7] + "^" + cConfig.FFU_Alarm[7] + "^" + cConfig.FFU_RPM[7] + "^"
                    //                          + cConfig.FFU_Power[8] + "^" + cConfig.FFU_Alarm[8] + "^" + cConfig.FFU_RPM[8] + "^" + cConfig.FFU_Power[9] + "^" + cConfig.FFU_Alarm[9] + "^" + cConfig.FFU_RPM[9] + "^"
                    //                          + cConfig.FFU_Power[10] + "^" + cConfig.FFU_Alarm[10] + "^" + cConfig.FFU_RPM[10];// + "^" + cConfig.FFU_Power[11] + "^" + cConfig.FFU_Alarm[11] + "^" + cConfig.FFU_RPM[11] + "^"
                    //+ cConfig.FFU_Power[12] + "^" + cConfig.FFU_Alarm[12] + "^" + cConfig.FFU_RPM[12] + "^" + cConfig.FFU_Power[13] + "^" + cConfig.FFU_Alarm[13] + "^" + cConfig.FFU_RPM[13] + "^"
                    //+ cConfig.FFU_Power[14] + "^" + cConfig.FFU_Alarm[14] + "^" + cConfig.FFU_RPM[14] + "^" + cConfig.FFU_Power[15] + "^" + cConfig.FFU_Alarm[15] + "^" + cConfig.FFU_RPM[15] + "^"
                    //+ cConfig.FFU_Power[16] + "^" + cConfig.FFU_Alarm[16] + "^" + cConfig.FFU_RPM[16] + "^" + cConfig.FFU_Power[17] + "^" + cConfig.FFU_Alarm[17] + "^" + cConfig.FFU_RPM[17] + "^"
                    //+ cConfig.FFU_Power[18] + "^" + cConfig.FFU_Alarm[18] + "^" + cConfig.FFU_RPM[18] + "^" + cConfig.FFU_Power[19] + "^" + cConfig.FFU_Alarm[19] + "^" + cConfig.FFU_RPM[19] + "^"
                    //+ cConfig.FFU_Power[20] + "^" + cConfig.FFU_Alarm[20] + "^" + cConfig.FFU_RPM[20];

                    FFU_PLCSend(FFU_Message);
                    FFU_Message = null;
                    FFU_Count = 0;
                    Console.WriteLine("FFU Message Clear !!");

                    #endregion FFU
                }
                else
                {
                    FFU_Count++;
                }

                #endregion FFU ZR Send
            }
            if (Accura.bCheckUPS)
            {
                //for (int i = 0; fUPSCurrent.Length > i; i++)
                //{
                //    UPS_Message += (fUPSCurrent[i] + "^");
                //}
                //for (int i = 0; fUPSVoltage.Length > i; i++)
                //{
                //    UPS_Message += (fUPSVoltage[i] + "^");
                //}
                //for (int i = 0; fUPSPower1.Length > i; i++)
                //{
                //    UPS_Message += (fUPSPower1[i] + "^");
                //}

                if (UPS_Cnt > 500)
                {
                    if (bUPSPower && bUPSVoltage && bUPSCurrent)
                    {
                        UPS_Message = fUPSKWtotal.ToString() + "," + iUPSKWhsum.ToString() + "," + fUPSRPhaseVoltage.ToString() + "," + fUPSTPhaseVoltage.ToString() + "," + fUPSRPhaseCurrent.ToString() + "," + fUPSTPhaseCurrent.ToString();

                        UPS_PLCSend(UPS_Message, Address.PC.UPS);
                        UPS_Cnt = 0;
                    }
                }
                else
                {
                    UPS_Cnt++;
                }

            }
            if (Accura.bCheckGPS)
            {
                if (GPS_Cnt > 500)
                {
                    if (bGPSPower && bGPSVoltage && bGPSCurrent)
                    {
                        UPS_Message = fGPSKWtotal.ToString() + "," + iGPSKWhsum.ToString() + "," + fGPSRPhaseVoltage.ToString() + "," + fGPSTPhaseVoltage.ToString() + "," + fGPSRPhaseCurrent.ToString() + "," + fGPSTPhaseCurrent.ToString();

                        UPS_PLCSend(UPS_Message, Address.PC.GPS);
                        GPS_Cnt = 0;
                    }
                }
                else
                {
                    GPS_Cnt++;
                }

            }
            if (bPE)
            {
                PREDisp();

                if (spPE.IsOpen) PEConnectionCheck = 1;
                else PEConnectionCheck = 0;

                //actIF.WriteDeviceBlock("ZR" + PEAddress.ToString(), 1, ref PEConnectionCheck);

                if (PESendCount > double.Parse(cConfig.PEGetTimeDelay) * 20)
                {
                    if (spPE.IsOpen) PEDataSend();
                    PESendCount = 0;
                }
                else PESendCount++;
            }

            //if (PCNo == "4")
            //{
            //    if (!HeightSocket.connectState)
            //    {
            //        if (HeightConnectCnt > 100)
            //        {
            //            HeightSocket.ConnectClient(cConfig.HeightIPAddress, int.Parse(cConfig.HeightPort));
            //            HeightConnectCnt = 0;
            //        }
            //        else
            //        {
            //            HeightConnectCnt++;
            //        }
            //    }
            //}

            if (hwnd == IntPtr.Zero)
            {
                hwnd = FindWindow(null, "BD_IF");
            }

            if (cConfig.Vision_Init)
            {
                if (bWin_com != bOLD_Wincom)
                {
                    label5.BackColor = Color.Aqua;
                    SendData("*Comm" + "," + Convert.ToInt32(bWin_com));
                    bOLD_Wincom = bWin_com;
                    Win_Count = 0;
                }
                else if (Win_Count < 500)
                {
                    Win_Count++;
                }
                else
                {
                    label5.BackColor = Color.Transparent;
                    hwnd = FindWindow(null, "BD_IF");
                    csLog.LogSave("Window Message ReConnection!!");
                    Win_Count = 0;
                }

                if (iMXOldMsg == cConfig.rclet && cConfig.rclet != 0)
                {
                    if (MX_ErrorCount < 200)
                    {
                        MX_ErrorCount = MX_ErrorCount + 1;
                    }
                    else
                    {
                        label7.BackColor = Color.Red;
                        actIF.Close();
                        MX_ErrorCount = 0;
                        csLog.LogSave("MX Component ReConnection!! _" + cConfig.rclet);
                        if (Connection() == 0) { }
                    }
                }
                else
                {
                    iMXOldMsg = cConfig.rclet;
                    MX_ErrorCount = 0;
                }

                if (cConfig.rclet != 0) label6.BackColor = Color.Transparent; else label6.BackColor = Color.Aqua;
                if (cConfig.rclet != 0) label7.BackColor = Color.Red; else label7.BackColor = Color.Transparent;
                label8.Text = Convert.ToString(cConfig.rclet);

                #region PLC BitControl

                //pcy200824 bit추가
                int[] Bitdata = new int[2];
                int[] Bitdata1 = new int[2];

                cConfig.rclet = actIF.ReadDeviceBlock(PLCDevice + Convert.ToString(Address.PLC.BITCONTROL), 2, out Bitdata[0]);
                cConfig.rclet = actIF.ReadDeviceBlock(PLCDevice + Convert.ToString(Address.PLC.BITCONTROL + 2), 2, out Bitdata1[0]);

                for (int i = 0; i < Bitdata.Length; i++)
                {
                    if (oldReq[i] != Bitdata[i] || m_bSendMsgRetry)
                    {
                        m_nSendMsgRetryCount = 0;
                        oldReq[i] = Bitdata[i];
                        plcLogwrite(PLCDevice + Address.PLC.BITCONTROL.ToString() + "," + Bitdata[i].ToString());
                        SendData("*1," + i.ToString() + "," + Bitdata[i].ToString());
                    }
                    else
                    {
                        m_nSendMsgRetryCount++;
                        if (m_nSendMsgRetryCount > 200)
                        {
                            m_nSendMsgRetryCount = 0;
                            m_bSendMsgRetry = true;
                            plcLogwrite("PLC Data Send Retry0");
                        }
                    }
                    if (oldReq1[i] != Bitdata1[i] || m_bSendMsgRetry1)
                    {
                        m_nSendMsgRetryCount1 = 0;
                        oldReq1[i] = Bitdata1[i];
                        plcLogwrite(PLCDevice + (Address.PLC.BITCONTROL + 2).ToString() + "," + Bitdata1[i].ToString());
                        SendData("*3," + i.ToString() + "," + Bitdata1[i].ToString());
                    }
                    else
                    {
                        m_nSendMsgRetryCount1++;
                        if (m_nSendMsgRetryCount1 > 200)
                        {
                            m_nSendMsgRetryCount1 = 0;
                            m_bSendMsgRetry1 = true;
                            plcLogwrite("PLC Data Send Retry1");
                        }
                    }
                }
                if (m_bSendMsgRetry)
                    m_bSendMsgRetry = false;
                if (m_bSendMsgRetry1)
                    m_bSendMsgRetry1 = false;

                if ((Bitdata[1] & 0x8000) > 0 && calCheckCnt > 50) // Manual 상태 이면
                {
                    calCheckCnt = 0;
                    //PLC Manual 상태에는 Calibration 진행 여부 체크함.
                    int[] calBit = new int[2];
                    actIF.ReadDeviceBlock(PLCDevice + Convert.ToString(Address.PLC.PLC_CAL_MOVE), 1, out calBit[0]); //14500
                    if (calBit[0] == 1)
                    {
                        SendData("CALMOVESTART");
                    }
                    else if (calBit[0] == 0)
                    {
                        int iData = 0;
                        actIF.WriteDeviceBlock(PLCDevice + (Address.PC.PC_CAL_MOVE + 2).ToString(), 1, ref iData);
                    }

                    actIF.ReadDeviceBlock(PLCDevice + Convert.ToString(Address.PLC.PLC_CAL_MOVE + 2), 1, out calBit[0]); //14502
                    if (calBit[0] == 1)
                    {
                        int iData = 0;
                        actIF.WriteDeviceBlock(PLCDevice + Address.PC.PC_CAL_MOVE, 1, ref iData);
                        SendData("CALMOVEFINISH");
                    }
                }
                else if ((Bitdata[1] & 0x8000) > 0) calCheckCnt++;

                //int Len = 0;

                //BDR Reqeust
                //if ((Bitdata[0] & (int)Math.Pow(2, 15)) / (int)Math.Pow(2, 15) == 1)
                //{
                //}

                #endregion PLC BitControl
            }

            #region Cal&Teach

            if (bCalIF)
            {
                int[] Caldata = new int[2];
                actIF.ReadDeviceBlock(PLCDevice + Convert.ToString(Address.PLC.MOVEREPLY), 2, out Caldata[0]);
                for (int i = 0; i < Caldata.Length; i++)
                {
                    if (oldCal[i] != Caldata[i])
                    {
                        oldCal[i] = Caldata[i];
                        plcLogwrite(PLCDevice + Address.PLC.MOVEREPLY.ToString() + "," + Caldata[i].ToString());
                        SendData("*2," + i.ToString() + "," + Caldata[i].ToString());
                    }
                }
            }

            #endregion Cal&Teach
        }

        private int calCheckCnt = 0;

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.Show();
        }

        private void MXIF_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Dispose();
        }

        public string StringRecive(string start, short DataSize)
        {
            string rcvData = "";
            try
            {
                ASCIIEncoding Ascii = new ASCIIEncoding();
                short lcDataSize = (short)(DataSize);
                //de_type = 24;
                int[] lcRcvData = new int[lcDataSize];
                actIF.ReadDeviceBlock(start, DataSize, out lcRcvData[0]);

                for (int i = 0; i < lcDataSize / 2; i++)
                {
                    byte[] lcTemp = new byte[2];
                    lcTemp[0] = (byte)(lcRcvData[i] & 0xFF);
                    lcTemp[1] = (byte)((lcRcvData[i] & 0xFF00) / 0x100);

                    if (lcTemp[0] == 0) lcTemp[0] = 0x20;
                    if (lcTemp[1] == 0) lcTemp[1] = 0x20;

                    rcvData = rcvData + Ascii.GetString(lcTemp);
                }
                return rcvData;
            }
            catch (Exception)
            {
                //LOG.ExceptionLogSave("StringRecive" + "," + EX.GetType().Name + "," + EX.Message);
                return rcvData;
            }
        }

        public void StringSend(string start, short DataSize, string sndData)
        {
            try
            {
                string[] DataValue = sndData.Split(new char[] { '^' });
                string ChangeValue = null;
                //if (DataSize > 19)
                //{
                //    //short lcDataSize = (short)(DataSize * 2);
                //    //de_type = 24;
                //    for (int k = 0; k < DataValue.Length; k++)
                //    {
                //        if (DataSize > DataValue[k].Length)
                //        {
                //            int lcEnd = ((DataSize / DataValue.Length) - DataValue[k].Length);
                //            ChangeValue = ChangeValue + DataValue[k];
                //            for (int i = 0; i < lcEnd + 20; i++)
                //            {
                //                ChangeValue = ChangeValue + " ";
                //            }
                //        }
                //    }
                //}
                //else
                //{
                    ChangeValue = sndData;
                //}
                ASCIIEncoding Ascii = new ASCIIEncoding();
                //byte[] lcbyte = Ascii.GetBytes(ChangeValue);
                //int[] lcData = new int[lcbyte.Length];
                byte[] lcbyte = Ascii.GetBytes(ChangeValue);
                int[] lcData = new int[lcbyte.Length / 2];
                if (DataSize != 1)
                {
                    for (int i = 0; i < lcData.Length; i++)
                    {
                        lcData[i] = (int)(lcbyte[2 * i] + lcbyte[2 * i + 1] * 0x100);
                    }
                    int[] ClrData = new int[2000];

                    //if (start == "W1300")
                    //{
                    //    int ret = actIF.WriteDeviceBlock(start, 2000, ref ClrData[0]);
                    //}

                    int ret1 = actIF.WriteDeviceBlock(start, lcData.Length, ref lcData[0]);
                }
                else
                {
                    //1word Nak Message (임시)
                    int lcData1 = (int)(lcbyte[0]); // * 0x100
                    //ICData1 Asckii 변환 필요
                    int ret1 = actIF.WriteDeviceBlock(start, 1, ref lcData1);
                }
            }
            catch (Exception)
            {
                // LOG.ExceptionLogSave("StringSend" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }


        //20210819 send string and auto clear word unuse. Datasize = number of word
        public void StringSend1(string start, short DataSize, string sndData)
        {
            try
            {
                byte[] lcbyte = new byte[DataSize * 2];
                int[] lcData = new int[DataSize];

                ASCIIEncoding Ascii = new ASCIIEncoding();

                for (int i = 0; i < sndData.Length; i++)
                {
                    lcbyte[i] = Ascii.GetBytes(sndData)[i];
                }

                //Clear word unuse
                for (int i = sndData.Length; i < lcbyte.Length; i++)
                {
                    lcbyte[i] = 0;
                }
                
                //write data
                for (int i = 0; i < lcData.Length; i++)
                {
                    lcData[i] = (int)(lcbyte[2 * i] + lcbyte[2 * i + 1] * 0x100);
                }
                actIF.WriteDeviceBlock(start, lcData.Length, ref lcData[0]);

            }
            catch (Exception)
            {
                // LOG.ExceptionLogSave("StringSend" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }


        //string Receive_FFU = null;
        //int readCnt = 0;
        //byte recvByte = 0;
        private byte[] recvBuf = new byte[99];

        private bool FFU_Comm;     //통신 체크

        //FFU RPM
        // 0 ~ 1300
        //FFU Status
        //0 : FFU Stop
        //1 : FFU Run
        //2 : Power Off
        //3 : Motor Lock
        //4 : Hall Erro
        //5 : Over Current
        //6 : Motor IC Fault
        //7 : Motor IC Heat

        //int FFU_FANNo = 0;

        public void FFUPORTOPEN()
        {
            bFFU = true;
            try
            {
                if (!FFUSP.IsOpen)
                {
                    FFUSP.PortName = cConfig.FFU_SP;
                    FFUSP.Encoding = Encoding.Default;
                    FFUSP.BaudRate = 9600;
                    FFUSP.DataBits = (int)8;
                    FFUSP.Parity = Parity.None;
                    FFUSP.StopBits = StopBits.One;
                    FFUSP.Handshake = Handshake.None;
                    FFUSP.DataReceived += new SerialDataReceivedEventHandler(FFUSP_DataReceived);
                    FFUSP.Open();

                    cConfig.FFU_CNCT = true;
                    timer1.Interval = 5000;
                    timer2.Interval = 3000;
                    timer1.Enabled = true;
                    timer2.Enabled = true;
                }
            }
            catch (Exception )
            {
                cConfig.FFU_CNCT = false;
            }
        }

        private string ReceiveData = "";

        //bool sendenable = false;
        //private void FFUSP_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        //{
        //    string TempMsg = "";
        //    int DataLength = FFUSP.BytesToRead;
        //    byte[] lcRcvData = new byte[DataLength];

        //    FFUSP.Read(lcRcvData, 0, DataLength);
        //    FFU_Comm = false;
        //    string temp = "";
        //    for (int i = 0; i < lcRcvData.Length; i++)
        //    {
        //        TempMsg = string.Format("{0:X}", lcRcvData[i]);
        //        temp = temp + TempMsg.PadLeft(2, '0');
        //    }
        //    if (temp != "")
        //    {
        //        if (temp.Substring(0, 2) == "02")
        //        {
        //            ReceiveData = temp;
        //            if (ReceiveData.Substring(0, 2) == "02" && ReceiveData.Substring(ReceiveData.Length - 2, 2) == "03")
        //            {
        //                FFUData(ReceiveData);
        //                ReceiveData = "";
        //            }
        //        }
        //        else
        //        {
        //            ReceiveData = ReceiveData + temp;
        //            if (ReceiveData.Substring(0, 2) == "02" && ReceiveData.Substring(ReceiveData.Length - 2, 2) == "03")
        //            {
        //                FFUData(ReceiveData);
        //                ReceiveData = "";
        //            }
        //        }
        //    }
        //}
        private List<byte> FFURcvData = new List<byte>();
        private void FFUSP_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            int DataLength = FFUSP.BytesToRead;
            byte[] lcRcvData = new byte[DataLength];
            bool bEnd = false;
            FFUSP.Read(lcRcvData, 0, DataLength);
            foreach (var s in lcRcvData)
            {
                switch (s)
                {
                    case 0x02: //STX
                        FFURcvData.Clear();
                        break;

                    case 0x03: //ETX
                        bEnd = true;
                        break;
                }
                FFURcvData.Add(s);
            }
            if (FFURcvData.Count > 1000) FFURcvData.Clear(); //무작정 길어지면 클리어
            if(bEnd)
            {
                FFUData2(FFURcvData);
            }
        }

        private void FFUData(string Msg)
        {
            try
            {
                //int count = 3;
                //if (Msg.Length == 22) count = 1;
                for (int i = 0; i < 1; i++)
                {
                    string temp = Msg.Substring(10 + (8 * i), 8);
                    int ffuNo = int.Parse(temp.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) - 129;
                    if (temp.Substring(4, 2) == "80" || temp.Substring(4, 2) == "81") cConfig.FFU_Alarm[ffuNo] = 0;
                    else cConfig.FFU_Alarm[ffuNo] = 1;
                    if (temp.Substring(4, 2) == "00" || temp.Substring(4, 2) == "99") cConfig.FFU_Power[ffuNo] = 0;
                    else cConfig.FFU_Power[ffuNo] = 1;

                    cConfig.FFU_RPM[ffuNo] = Convert.ToInt32(temp.Substring(2, 2), 16) * 10;

                    Console.WriteLine("FFU Receive " + ffuNo.ToString());
                    //FFU ON : 1 , OFF : 0
                    //ALARM : 1 , 정상 : 0
                    //FFU SPEED (RPM)
                }
            }
            catch
            {
                //FFU
            }
        }
        private void FFUData2(List<byte> Msg)
        {
            //응답bit 7연결상태(1정상 0알람) 6모터알람(1알람 0정상) 5과전류(1알람 0정상) 4안씀 3안씀 2안씀 1전원(1알람 0정상) 0제어상태(1로컬제어 0remote제어)
            //ex)0x80 = 통신정상 리모트제어
            //0x81 = 통신정상 로컬제어
            try
            {
                for (int i = 0; i < cConfig.FFU_CNT; i++)
                {
                    cConfig.FFU_RPM[i] = (int)Msg[6 + (i * 4)] * 10;
                    if (Msg[7 + (i * 4)] == 0x80 || Msg[7 + (i * 4)] == 0x81) cConfig.FFU_Alarm[i] = 0;
                    else cConfig.FFU_Alarm[i] = 1;
                    //if (Msg[7 + (i * 4)] == 0x00 || Msg[7 + (i * 4)] == 0x99) cConfig.FFU_Power[i] = 0;
                    //else cConfig.FFU_Power[i] = 1;
                    //알람이면 파워오프
                    if (cConfig.FFU_Alarm[i] == 0) cConfig.FFU_Power[i] = 1;
                    else cConfig.FFU_Power[i] = 0;
                }
            }
            catch
            {
                //FFU
            }
        }

        private void FFU_PLCSend(string Rdata)
        {
            try
            {
                string[] DataValue = Rdata.Split(new char[] { '^' });
                int[] iData = new int[DataValue.Length];

                for (int i = 0; i < DataValue.Length; i++)
                {
                    int ResultValue = Convert.ToInt32(DataValue[i]);
                    iData[i] = (int)(ResultValue) & 0xFFFF;
                }

                int ret = actIF.WriteDeviceBlock("ZR" + Address.PC.FFU, DataValue.Length, ref iData[0]);
                //int ret = actIF.WriteDeviceBlock("ZR" + "14462", DataValue.Length, ref iData[0]);

                //mxLogwrite(Address.Word.ACCURA + "," + iData.Length.ToString() + "," + Rdata.ToString());
                FFU_Count = 0;
            }
            catch
            {
            }
        }

        private void MCUL_Read2(int Count)
        {
            byte[] FFU_SData = new byte[9];

            FFU_SData[0] = 0x02;           //STX
            FFU_SData[1] = 0x8A;           //고정
            FFU_SData[2] = 0x87;           //고정
            FFU_SData[3] = 0x81;           //고정
            FFU_SData[4] = 0x9F;           //고정
            FFU_SData[5] = Convert.ToByte((int)cConfig.LCU_ID);            //START ID
            FFU_SData[6] = Convert.ToByte((int)cConfig.LCU_ID + Count);            //END ID
            string checksum = ((int)(FFU_SData[1] + FFU_SData[2] + FFU_SData[3] + FFU_SData[4] + FFU_SData[5] + FFU_SData[6])).ToString("X2");
            FFU_SData[7] = byte.Parse(checksum.Substring(1, 2), System.Globalization.NumberStyles.HexNumber); //checksum
            FFU_SData[8] = 0x03;           //ETX

            try
            {
                FFUSP.Write(FFU_SData, 0, FFU_SData.Length);
                Console.WriteLine("MCUL Read2");
                FFU_Comm = true;
            }
            catch
            {
                cConfig.FFU_CNCT = false;
                //FFU SEND Fail
            }
        }
        private void MCUL_ValueSet(int ivalue)
        {
            ivalue = ivalue / 10;
            byte[] FFU_SData = new byte[10];

            FFU_SData[0] = 0x02;           //STX
            FFU_SData[1] = 0x89;           //고정
            FFU_SData[2] = 0x84;           //고정
            FFU_SData[3] = 0x81;           //고정
            FFU_SData[4] = 0x9F;           //고정
            FFU_SData[5] = Convert.ToByte((int)cConfig.LCU_ID);            //START ID
            FFU_SData[6] = Convert.ToByte((int)cConfig.LCU_ID + cConfig.FFU_CNT);            //END ID
            FFU_SData[7] = Convert.ToByte(ivalue);
            string checksum = ((int)(FFU_SData[1] + FFU_SData[2] + FFU_SData[3] + FFU_SData[4] + FFU_SData[5] + FFU_SData[6] + FFU_SData[7])).ToString("X2");
            FFU_SData[8] = byte.Parse(checksum.Substring(1, 2), System.Globalization.NumberStyles.HexNumber); //checksum
            FFU_SData[9] = 0x03;           //ETX

            try
            {
                FFUSP.Write(FFU_SData, 0, FFU_SData.Length);
                Console.WriteLine("MCUL VALUE Send " + ivalue);
                //FFU_Comm = true;
            }
            catch
            {
                cConfig.FFU_CNCT = false;
                //FFU SEND Fail
            }
        }

        private int FFU_Count = 0;
       // private int FFU_CNCT_CNT = 0;
        private int UPS_Cnt = 0;
        private int GPS_Cnt = 0;

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (cConfig.FFU_CNCT)
            {
                MCUL_Read2(cConfig.FFU_CNT);
                //if (FFU_CNCT_CNT < cConfig.FFU_CNT)
                //{
                //    MCUL_Read(FFU_CNCT_CNT);
                //    FFU_CNCT_CNT++;
                //}
                //else
                //{
                //    FFU_CNCT_CNT = 0;
                //}
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (FFU_Comm) lblTimer.BackColor = Color.GreenYellow; else lblTimer.BackColor = Color.White;

            for (int i = 0; i < cConfig.FFU_CNT; i++)
            {
                dgvFFU.Rows[i].Cells[0].Value = cConfig.FFU_RPM[i];
                dgvFFU.Rows[i].Cells[1].Value = cConfig.FFU_Power[i];
                dgvFFU.Rows[i].Cells[2].Value = cConfig.FFU_Alarm[i];
            }
        }

        private void lbGMSLoad_Click(object sender, EventArgs e)
        {
            GMSPortOpen();
        }

        private void lbGMSInspection_Click(object sender, EventArgs e)
        {
            GMSPortOpen();
        }

        private string sCMD = null;

        public delegate void dgtSetBox(string sMsg);

        private void SetListBox(string sMsg)
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new MethodInvoker(delegate
                {
                    if (listBox1.Items.Count > 100) listBox1.Items.Clear();

                    listBox1.Items.Add(sMsg);
                }));
            }
            else
            {
                listBox1.Items.Add(sMsg);
            }
        }

        public void HeightRcvData(object sender, DataEventArgs e) //데이터 받을때 발생하는 이벤트. e.data에 데이터옴.
        {
            if (e.data != null)
            {
                //string[] sData = e.data.Split('*');
                string tData = e.data;
                //pcy200708 잡체인지 후 성공시 스타트보냄 에러여도 스타트가 꺼지니깐 기존꺼라도 사용하도록 스타트보냄.
                if (tData.Contains("OK") && tData.Contains("successfully"))
                {
                    Height_Send("START");
                }
                if (tData.Contains("ERROR") && tData.Contains("Failed") && tData.Contains("job"))
                {
                    Height_Send("START");
                }
                string[] lines = tData.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < lines.Length; i++)
                {
                    SetListBox("Recv <- " + lines[i]);
                    //DCR 데이터 전송 형식 : STX + (N) + * (Data) + ETX (N은 판독기 번 호, "*"는 분리 기호).
                    //이부분 다음에 DCR코드오면 작성필요.
                    try
                    {
                        switch (sCMD.ToUpper())
                        {
                            case "TRIGGER":  //
                                {
                                    if (lines[0].Length > 8)
                                    {
                                        SendData("HEIGHTVALUE" + "," + lines[lines.Length - 1].ToString());
                                        //SendData("HEIGHTVALUE" + "," + tData.ToString());
                                        //Height_Send("STOP");
                                    }
                                    //Thread.Sleep(3000);
                                    //Height_Send("VALUE");
                                    break;
                                }
                            case "VALUE":  //Height Point Value Return
                                {
                                    SendData("HEIGHTVALUE" + "_" + tData);
                                    //Height_Send("STOP");
                                    break;
                                }
                            case "JOBLOAD": //Job Name 정보 Return
                                {
                                    SendData("JOBLOAD");
                                    break;
                                }
                            case "STOP":   //Clear
                                {
                                    //Height_Send("START");
                                    break;
                                }
                        }
                    }
                    catch (Exception )
                    {
                    }
                }
            }
        }

        public void Height_Send(string sMsg)
        {
            if (HeightSocket.connectState)
            {
                HeightSocket.SendData(sMsg);
                sCMD = sMsg;
                SetListBox("Send -> " + sCMD);
            }
            else
            {
                SetListBox("Send -> " + sCMD + " fail - Not Connected");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Height_Send("TRIGGER");
            sCMD = "TRIGGER";
        }

        //210115 cjm 전력량계 통신 프로그램 추가
        private void lbUPSLoad_Click(object sender, EventArgs e)
        {
            Accura.init_UPSConnect();
        }

        

        public Label[] displayCurrent = null;
        public Label[] displayVoltage = null;
        public Label[] displayPower = null;

        float fKWtotal = 0.0f;
        int iKWhsum = 0;
        float fRPhaseVoltage = 0.0f;
        float fTPhaseVoltage = 0.0f;
        float fRPhaseCurrent = 0.0f;
        float fTPhaseCurrent = 0.0f;

        //210115 cjm 전력량계 통신 프로그램 추가 : Accura에서 Bool값, return값 불러오기
        private void tmrUPS_Tick(object sender, EventArgs e)
        {
            if (Accura.bCheckUPS)
            {
                label18.BackColor = Color.GreenYellow;

                try
                {
                    Accura.get_UPSCurrent(out bUPSCurrent);
                    Accura.get_UPSVoltage(out bUPSVoltage);
                    Accura.get_UPSPower(out bUPSPower, out fUPSPower1, out iUPSPower2);

                    if (bUPSCurrent)
                    {
                        for (int i = 0; 4 > i; i++)
                        {
                            fUPSCurrent[i] = Accura.getUPSCurrentValue[i];
                        }
                        //lblCurrentA.Text = fUPSCurrent[0].ToString();
                        //lblCurrentB.Text = fUPSCurrent[1].ToString();
                        //lblCurrentC.Text = fUPSCurrent[2].ToString();
                        //lblCurrentAvg.Text = fUPSCurrent[3].ToString();

                        fUPSRPhaseCurrent = fUPSCurrent[0];    // A상 전류 = R상 전류
                        fUPSTPhaseCurrent = fUPSCurrent[2];    // C상 전류 = T상 전류

                        lblUPSCurrentR.Text = fUPSRPhaseCurrent.ToString("N3");
                        lblUPSCurrentT.Text = fUPSTPhaseCurrent.ToString("N3");
                    }
                    if (bUPSVoltage)
                    {
                        for (int i = 0; 4 > i; i++)
                        {
                            fUPSVoltage[i] = Accura.getUPSVoltageValue[i];
                        }
                        //lblVoltageA.Text = fUPSVoltage[0].ToString();
                        //lblVoltageB.Text = fUPSVoltage[1].ToString();
                        //lblVoltageC.Text = fUPSVoltage[2].ToString();
                        //lblVoltageAvg.Text = fUPSVoltage[3].ToString();

                        fUPSRPhaseVoltage = fUPSVoltage[0];    // A상 전압 = R상 전압
                        fUPSTPhaseVoltage = fUPSVoltage[2];    // C상 전압 = T상 전압

                        lblUPSVoltageR.Text = fUPSRPhaseVoltage.ToString("N3");
                        lblUPSVoltageT.Text = fUPSTPhaseVoltage.ToString("N3");
                    }
                    if (bUPSPower)
                    {
                        //lblPowerA.Text = fUPSPower1[0].ToString();
                        //lblPowerB.Text = fUPSPower1[1].ToString();
                        //lblPowerC.Text = fUPSPower1[2].ToString();
                        //lblPowerAvg.Text = fUPSPower1[3].ToString();

                        fUPSKWtotal = fUPSPower1[3];
                        iUPSKWhsum = iUPSPower2[2];

                        lblUPSKWtotal.Text = fUPSKWtotal.ToString("N3");
                        lblUPSKWhsum.Text = iUPSKWhsum.ToString("N3");
                    }
                }
                catch
                {

                }
            }
            else label18.BackColor = Color.White;

            if (Accura.bCheckGPS)
            {
                label34.BackColor = Color.GreenYellow;

                try
                {
                    Accura.get_GPSCurrent(out bGPSCurrent);
                    Accura.get_GPSVoltage(out bGPSVoltage);
                    Accura.get_GPSPower(out bGPSPower, out fGPSPower1, out iGPSPower2);

                    if (bGPSCurrent)
                    {
                        for (int i = 0; 4 > i; i++)
                        {
                            fGPSCurrent[i] = Accura.getGPSCurrentValue[i];
                        }
                        //lblCurrentA.Text = fUPSCurrent[0].ToString();
                        //lblCurrentB.Text = fUPSCurrent[1].ToString();
                        //lblCurrentC.Text = fUPSCurrent[2].ToString();
                        //lblCurrentAvg.Text = fUPSCurrent[3].ToString();

                        fGPSRPhaseCurrent = fGPSCurrent[0];    // A상 전류 = R상 전류
                        fGPSTPhaseCurrent = fGPSCurrent[2];    // C상 전류 = T상 전류

                        lblGPSCurrentR.Text = fGPSRPhaseCurrent.ToString("N3");
                        lblGPSCurrentT.Text = fGPSTPhaseCurrent.ToString("N3");
                    }
                    if (bGPSVoltage)
                    {
                        for (int i = 0; 4 > i; i++)
                        {
                            fGPSVoltage[i] = Accura.getGPSVoltageValue[i];
                        }
                        //lblVoltageA.Text = fUPSVoltage[0].ToString();
                        //lblVoltageB.Text = fUPSVoltage[1].ToString();
                        //lblVoltageC.Text = fUPSVoltage[2].ToString();
                        //lblVoltageAvg.Text = fUPSVoltage[3].ToString();

                        fGPSRPhaseVoltage = fGPSVoltage[0];    // A상 전압 = R상 전압
                        fGPSTPhaseVoltage = fGPSVoltage[2];    // C상 전압 = T상 전압

                        lblGPSVoltageR.Text = fGPSRPhaseVoltage.ToString("N3");
                        lblGPSVoltageT.Text = fGPSTPhaseVoltage.ToString("N3");
                    }
                    if (bGPSPower)
                    {
                        //lblPowerA.Text = fUPSPower1[0].ToString();
                        //lblPowerB.Text = fUPSPower1[1].ToString();
                        //lblPowerC.Text = fUPSPower1[2].ToString();
                        //lblPowerAvg.Text = fUPSPower1[3].ToString();

                        fGPSKWtotal = fGPSPower1[3];
                        iGPSKWhsum = iGPSPower2[2];

                        lblGPSKWtotal.Text = fGPSKWtotal.ToString("N3");
                        lblGPSKWhsum.Text = iGPSKWhsum.ToString("N3");
                    }
                }
                catch
                {

                }
            }
            else label34.BackColor = Color.White;

        }

        //210115 cjm 전력량계 통신 프로그램 추가 : UPS 값을 PLC에 보내주기
        private void UPS_PLCSend(string Rdata, int iAddress)
        {
            try
            {
                string[] DataValue = Rdata.Split(',');
                double[] ResultValue = new double[12];
                int[] iData = new int[24];


                for (int i = 0; i < DataValue.Length; i++)
                {
                    ResultValue[i] = double.Parse(DataValue[i]);

                    //int iInteger = (int)(double.Parse(DataValue[i]));  //정수부분

                    //double dDifference = ResultValue[i] - iInteger; // 소수부분

                    //int iMinority = (int)(dDifference * 1000); //소수를 보내지 못해서 1000을 곱하여 보내기

                    //iData[i * 2] = iInteger;
                    //iData[i * 2 + 1] = iMinority;

                }

                iData[0] = Convert.ToInt32(ResultValue[0] * 1000) & 0xFFFF;
                iData[1] = (int)((Convert.ToInt32(ResultValue[0] * 1000) & 0xFFFF0000) / 0x10000);
                iData[2] = Convert.ToInt32(ResultValue[1] * 1000) & 0xFFFF;
                iData[3] = (int)((Convert.ToInt32(ResultValue[1] * 1000) & 0xFFFF0000) / 0x10000);
                iData[4] = Convert.ToInt32(ResultValue[2] * 1000) & 0xFFFF;
                iData[5] = (int)((Convert.ToInt32(ResultValue[2] * 1000) & 0xFFFF0000) / 0x10000);
                iData[6] = Convert.ToInt32(ResultValue[3] * 1000) & 0xFFFF;
                iData[7] = (int)((Convert.ToInt32(ResultValue[3] * 1000) & 0xFFFF0000) / 0x10000);
                iData[8] = Convert.ToInt32(ResultValue[4] * 1000) & 0xFFFF;
                iData[9] = (int)((Convert.ToInt32(ResultValue[4] * 1000) & 0xFFFF0000) / 0x10000);
                iData[10] = Convert.ToInt32(ResultValue[5] * 1000) & 0xFFFF;
                iData[11] = (int)((Convert.ToInt32(ResultValue[5] * 1000) & 0xFFFF0000) / 0x10000);

                int ret = actIF.WriteDeviceBlock("ZR" + iAddress, iData.Length, ref iData[0]); //어드레스는 PLC랑 협의 , ipdwData는 시작점
            }
            catch
            {
            }
        }
    }
}