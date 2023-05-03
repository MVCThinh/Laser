using rs2DAlign;
//2020.09.25 lkw
using rsLinearConvert;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
namespace Bending
{
    public partial class frmIF : Form
    {
        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, ref Win32API.COPYDATASTRUCT lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);

        public class Win32API
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

        public frmIF()
        {
            InitializeComponent();
            Visible = true;
            //Connection(true);
        }

        public void KillMX()
        {
            try
            {
                foreach (Process process in Process.GetProcesses())
                {
                    if (process.ProcessName.StartsWith("MX_IF"))
                    {
                        process.Kill();
                    }
                }
            }
            catch
            {
            }
        }

        private void OpenMX()
        {
            ProcessStartInfo StartInfo = new ProcessStartInfo();
            StartInfo.WorkingDirectory = @"C:\MXComponent";
            StartInfo.FileName = "MX_IF";
            StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            Process.Start(StartInfo);
        }

    

        public void SendData(string sMsg)
        {
            if (hwnd != IntPtr.Zero)
            {
                Win32API.COPYDATASTRUCT cds = new Win32API.COPYDATASTRUCT();
                cds.dwData = IntPtr.Zero; //임의값
                cds.cbData = Encoding.Default.GetBytes(sMsg).Length + 1;
                cds.lpData = sMsg;
                SendMessage(hwnd, Win32API.WM_COPYDATA, IntPtr.Zero, ref cds);
            }
        }

        private IntPtr hwnd;

        public int Connection(bool first)
        {
            if (first)
            {
                KillMX();
                OpenMX();                
            }
            else
            {
                bool bFind = false;
                foreach (Process process in Process.GetProcesses())
                {
                    if (process.ProcessName.StartsWith("MX_IF"))
                    {
                        hwnd = FindWindow(null, "MX_IF");
                        bFind = true;
                        if (hwnd == IntPtr.Zero)
                        {
                            System.Threading.Thread.Sleep(1000);
                            hwnd = FindWindow(null, "MX_IF");
                            if (hwnd == IntPtr.Zero)
                            {
                                KillMX();
                                bFind = false;
                            }
                        }
                        break;
                    }
                }

                if (!bFind)
                {
                    OpenMX();
                    System.Threading.Thread.Sleep(1000);
                    foreach (Process process in Process.GetProcesses())
                    {
                        if (process.ProcessName.StartsWith("MX_IF"))
                        {
                            hwnd = FindWindow(null, "MX_IF");
                            if (hwnd == IntPtr.Zero)
                            {
                                System.Threading.Thread.Sleep(1000);
                                return -1;
                            }
                            else return 0;
                        }
                    }
                }
                else
                {
                    return 0;
                }
            }
            return -1;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Win32API.WM_COPYDATA:
                    Win32API.COPYDATASTRUCT cds1 = (Win32API.COPYDATASTRUCT)m.GetLParam(typeof(Win32API.COPYDATASTRUCT));
                    string[] sData = cds1.lpData.Split(new char[] { ',' });

                    if (sData[0] == "*1")  // Request Data
                    {
                        //if(sData[1].IndexOf("ZR") == 0)
                        //{
                        //    sData[1] = sData[1].Substring(2);
                        //}
                        CheckReq(int.Parse(sData[2]), int.Parse(sData[1]));
                    }
                    else if (sData[0] == "*2")  // Cal Reply Data
                    {
                        getCalRep(int.Parse(sData[2]), int.Parse(sData[1]));
                    }
                    else if (sData[0] == "*3")  // Request Data1
                    {
                        CheckReq(int.Parse(sData[2]), int.Parse(sData[1]), true);
                    }
                    else if (sData[0] == "PNID")
                    {
                        if (int.TryParse(sData[1], out int visionNo))
                        {
                            Bending.Menu.frmAutoMain.Vision[visionNo].PanelID = sData[2];
                            //if(CONST.PCNo == 4 || CONST.PCNo == 7)
                            //{
                            //    if (visionNo == Vision_No.UpperInsp1_1 || visionNo == Vision_No.SideInsp1_1)
                            //    {

                            //    }
                            //}
                        }

                        //if (sData[1] == "CON") ;//CONST.PanelIDConveyor = sData[2];
                        //else if (sData[1] == "LB") ;//CONST.PanelIDLoadingBuffer = sData[2];
                        //else if (sData[1] == "SP") CONST.PanelIDSCFPanel = sData[2];
                        //else if (sData[1] == "SR") CONST.PanelIDSCFAttach = sData[2];
                        ////20.10.16 lkw Bending Pre Index NO 추가
                        //else if (sData[1] == "BP")
                        //{
                        //    try
                        //    {
                        //        CONST.PanelIDBendPre = sData[2].Substring(0, 32);
                        //        CONST.IndexNOBendPre = sData[2].Substring(39, 1);
                        //    }
                        //    catch
                        //    {
                        //        CONST.PanelIDBendPre = sData[2];
                        //    }
                        //}
                        //else if (sData[1] == "B1") CONST.PanelIDBend[0] = sData[2];
                        //else if (sData[1] == "B2") CONST.PanelIDBend[1] = sData[2];
                        //else if (sData[1] == "B3") CONST.PanelIDBend[2] = sData[2];
                        //else if (sData[1] == "I1") CONST.PanelIDInsp = sData[2];
                        //else if (sData[1] == "H1") CONST.PanelIDInspHeight = sData[2];
                        //else if (sData[1] == "TA") CONST.PanelIDTempAttach = sData[2];
                        //else if (sData[1] == "EA") CONST.PanelIDEMIAttach = sData[2];
                    }
                    else if (sData[0] == "RCPID")
                    {
                        CONST.m_bPLCConnect = true;
                        if (sData[1].Trim() == "") CONST.RunRecipe.RecipeName = CONST.RunRecipe.OldRecipeName;
                        else CONST.RunRecipe.RecipeName = sData[1].Trim(); //2018.07.20 일단 주석처리 추후 해제
                    }
                    else if (sData[0] == "RCPPARAM")
                    {
                        lock (CONST.RunRecipe.Param) //lock걸어서 clear후 recipe받기전에 딴데서 사용하지 못하도록 함. //param갯수 있으면 clear후 add말고 배열로 받아도 괜찮을듯. //이건 pc모두 공통이니까 아닐듯..
                        {
                            Console.WriteLine("rcpread");
                            CONST.RunRecipe.Param.Clear();
                            int icnt = 0;
                            int[] iData = new int[2];
                            foreach (eRecipe s in Enum.GetValues(typeof(eRecipe)))
                            {
                                iData[0] = int.Parse(sData[2 * icnt + 1]);
                                iData[1] = int.Parse(sData[2 * icnt + 2]);
                                CONST.RunRecipe.Param.TryAdd(s, (iData[0] + iData[1] * 0x10000) / 10000.0);
                                //CONST.RunRecipe.Param.Add(s, (iData[0] + iData[1] * 0x10000) / 10000.0);
                                icnt++;
                            }
                        }
                        //레시피 받을때 fanspeed 보고 mcul로 값변경
                        if (CONST.old_FFU_FAN_SPEED != (int)CONST.RunRecipe.Param[eRecipe.FFU_FAN_SPEED])
                        {
                            CONST.old_FFU_FAN_SPEED = (int)CONST.RunRecipe.Param[eRecipe.FFU_FAN_SPEED];
                            SendData("SETFFU," + CONST.old_FFU_FAN_SPEED);
                        }
                    }
                    else if (sData[0].IndexOf("MOTOR") == 0)
                    {
                        int MotionNo = int.Parse(sData[0].Substring(8));
                        int iData1 = int.Parse(sData[1]);
                        int iData2 = int.Parse(sData[2]);
                        Bending.Menu.frmAutoMain.visionPosition[MotionNo].XPos = (iData1 + (iData2 * 0x10000)) / 10000.0;
                        iData1 = int.Parse(sData[3]);
                        iData2 = int.Parse(sData[4]);
                        Bending.Menu.frmAutoMain.visionPosition[MotionNo].YPos = (iData1 + (iData2 * 0x10000)) / 10000.0;
                        iData1 = int.Parse(sData[5]);
                        iData2 = int.Parse(sData[6]);
                        Bending.Menu.frmAutoMain.visionPosition[MotionNo].TPos = (iData1 + (iData2 * 0x10000)) / 10000.0;
                    }
                    else if (sData[0].IndexOf("POSITION") == 0)
                    {
                        int MotionNo = int.Parse(sData[0].Substring(8));
                        int iData1 = int.Parse(sData[1]);
                        int iData2 = int.Parse(sData[2]);
                        Bending.Menu.frmAutoMain.visionPosition[MotionNo].XPos = (iData1 + (iData2 * 0x10000)) / 10000.0;
                        iData1 = int.Parse(sData[3]);
                        iData2 = int.Parse(sData[4]);
                        Bending.Menu.frmAutoMain.visionPosition[MotionNo].YPos = (iData1 + (iData2 * 0x10000)) / 10000.0;
                        iData1 = int.Parse(sData[5]);
                        iData2 = int.Parse(sData[6]);
                        Bending.Menu.frmAutoMain.visionPosition[MotionNo].TPos = (iData1 + (iData2 * 0x10000)) / 10000.0;
                    }
                    else if (sData[0].IndexOf("BENDPRE") == 0)
                    {
                        int BendNo = int.Parse(sData[0].Substring(7, 1));
                        int iData1 = int.Parse(sData[1]);
                        int iData2 = int.Parse(sData[2]);
                        CONST.bendPreOffset[BendNo] = new rs2DAlign.cs2DAlign.ptXYT();
                        CONST.bendPreOffset[BendNo].X = (iData1 + (iData2 * 0x10000)) / 10000.0;
                        iData1 = int.Parse(sData[3]);
                        iData2 = int.Parse(sData[4]);
                        CONST.bendPreOffset[BendNo].Y = (iData1 + (iData2 * 0x10000)) / 10000.0;
                        iData1 = int.Parse(sData[5]);
                        iData2 = int.Parse(sData[6]);
                        CONST.bendPreOffset[BendNo].T = (iData1 + (iData2 * 0x10000)) / 10000.0;
                    }
                    //2019.07.20 EMI Align 추가
                    else if (sData[0].IndexOf("EMI_DATA") == 0)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            int iData1 = int.Parse(sData[2 * i + 1]);
                            int iData2 = int.Parse(sData[2 * i + 2]);
                            if (i == 0)
                            {
                                CONST.plusTMoveRatio = (iData1 + (iData2 * 0x10000)) / 10000.0;
                            }
                            else if (i == 1)
                            {
                                CONST.plusYMoveRatio = (iData1 + (iData2 * 0x10000)) / 10000.0;
                            }
                            else if (i == 2)
                            {
                                CONST.minusTMoveRatio = (iData1 + (iData2 * 0x10000)) / 10000.0;
                            }
                            else if (i == 3)
                            {
                                CONST.minusYMoveRatio = (iData1 + (iData2 * 0x10000)) / 10000.0;
                            }
                            else if (i == 4)
                            {
                                CONST.EMITargetPosX1 = (iData1 + (iData2 * 0x10000)) / 10000.0;
                            }
                            else if (i == 5)
                            {
                                CONST.EMITargetPosY1 = (iData1 + (iData2 * 0x10000)) / 10000.0;
                            }
                            else if (i == 6)
                            {
                                CONST.EMITargetPosX2 = (iData1 + (iData2 * 0x10000)) / 10000.0;
                            }
                            else if (i == 7)
                            {
                                CONST.EMITargetPosY2 = (iData1 + (iData2 * 0x10000)) / 10000.0;
                            }
                        }
                    }
                    else if (sData[0].IndexOf("TrOffset") == 0)
                    {
                        int TransNo = int.Parse(sData[0].Substring(8, 1));
                        int iData1 = int.Parse(sData[1]);
                        int iData2 = int.Parse(sData[2]);
                        CONST.TransFirstOffset[TransNo] = new double();
                        CONST.TransFirstOffset[TransNo] = (iData1 + (iData2 * 0x10000)) / 10000.0;
                        //iData1 = int.Parse(sData[3]);
                        //iData2 = int.Parse(sData[4]);
                        //CONST.TransFirstOffset[TransNo].Y1 = (iData1 + (iData2 * 0x10000)) / 10000.0;
                        //iData1 = int.Parse(sData[5]);
                        //iData2 = int.Parse(sData[6]);
                        //CONST.TransFirstOffset[TransNo].X2= (iData1 + (iData2 * 0x10000)) / 10000.0;
                        //iData1 = int.Parse(sData[7]);
                        //iData2 = int.Parse(sData[8]);
                        //CONST.TransFirstOffset[TransNo].Y2 = (iData1 + (iData2 * 0x10000)) / 10000.0;
                    }
                    else if (sData[0] == "*Comm")
                    {
                        if (Bending.Menu.frmAutoMain != null)
                        {
                            if (int.Parse(sData[1]) >= 0) CONST.m_bPLCConnect = true;
                            else CONST.m_bPLCConnect = false;
                        }
                    }
                    else if (sData[0] == "CALMOVESTART")
                    {
                        // 다른 PC의 Calibration Data 값을 받아서 동기화 시킴
                        SendData("CALDATAREAD");
                    }
                    else if (sData[0] == "CALDATAREAD")
                    {
                        double[] calData = new double[33];
                        for (int i = 0; i < calData.Length; i++)
                        {
                            int[] iData = new int[2];
                            iData[0] = int.Parse(sData[2 * i + 1]);
                            iData[1] = int.Parse(sData[2 * i + 2]);
                            calData[i] = (iData[0] + iData[1] * 0x10000) / 10000.0;
                        }

                        // Calibration 저장
                        Bending.Menu.frmRecipe.calDataSave(calData);
                    }
                    else if (sData[0] == "CALMOVEFINISH")
                    {
                        Bending.Menu.frmRecipe.calDataReply = true;
                    }
                    else if (sData[0] == "TIME")
                    {
                        set_DateTimeSet(sData[1]);
                    }
                    else if (sData[0] == "SCALE")
                    {
                        if (sData[1] == "BD")
                        {
                            CONST.BDScaleX1 = (int.Parse(sData[1]) + (int.Parse(sData[2]) * 0x10000)) / 10000.0;
                            CONST.BDScaleY1 = (int.Parse(sData[3]) + (int.Parse(sData[4]) * 0x10000)) / 10000.0;
                            CONST.BDScaleX2 = (int.Parse(sData[5]) + (int.Parse(sData[6]) * 0x10000)) / 10000.0;
                            CONST.BDScaleY2 = (int.Parse(sData[7]) + (int.Parse(sData[8]) * 0x10000)) / 10000.0;
                        }
                        else if (sData[1] == "INSP")
                        {
                            CONST.INSPScaleX1 = (int.Parse(sData[1]) + (int.Parse(sData[2]) * 0x10000)) / 10000.0;
                            CONST.INSPScaleY1 = (int.Parse(sData[3]) + (int.Parse(sData[4]) * 0x10000)) / 10000.0;
                            CONST.INSPScaleX2 = (int.Parse(sData[5]) + (int.Parse(sData[6]) * 0x10000)) / 10000.0;
                            CONST.INSPScaleY2 = (int.Parse(sData[7]) + (int.Parse(sData[8]) * 0x10000)) / 10000.0;
                        }
                    }
                    else if (sData[0] == "BDR")
                    {
                        int ipoint = (int.Parse(sData[1]) + (int.Parse(sData[2]) * 0x10000));  //포인트 개수
                        double dRadiusOfRotation = (int.Parse(sData[1]) + (int.Parse(sData[2]) * 0x10000)) / 10000.0; ; //반지름
                        double dLastYOffset = (int.Parse(sData[1]) + (int.Parse(sData[2]) * 0x10000)) / 10000.0; //마지막 Y이동 추가량.
                        double d90OffsetY = (int.Parse(sData[1]) + (int.Parse(sData[2]) * 0x10000)) / 10000.0; //0~90 offsetY
                        double d90OffsetZ = (int.Parse(sData[1]) + (int.Parse(sData[2]) * 0x10000)) / 10000.0; //0~90 offsetZ
                        double d180OffsetY = (int.Parse(sData[1]) + (int.Parse(sData[2]) * 0x10000)) / 10000.0; //90~180 offsetY
                        double d180OffsetZ = (int.Parse(sData[1]) + (int.Parse(sData[2]) * 0x10000)) / 10000.0; //90~180 offsetZ
                        double dStartPosY = (int.Parse(sData[1]) + (int.Parse(sData[2]) * 0x10000)) / 10000.0;
                        double dStartPosZ = (int.Parse(sData[1]) + (int.Parse(sData[2]) * 0x10000)) / 10000.0;

                        Bending.Menu.frmRecipe.BendingTraceAutoCreate(ipoint, dRadiusOfRotation, dLastYOffset, d90OffsetY, d90OffsetZ, d180OffsetY, d180OffsetZ, dStartPosY, dStartPosZ);
                    }
                    else if (sData[0] == "HEIGHTVALUE")
                    {
                        Bending.Menu.Config.sHResult.dValue = new double[Bending.Menu.Config.sHeight.Length];
                        Bending.Menu.Config.sHResult.iOKNG = (int)ePCResult.WAIT;
                        for (int j = 0; j < sData.Length - 1; j++)
                        {
                            if (sData[j + 1] == "ERROR")
                            {
                                Bending.Menu.Config.sHResult.iOKNG = (int)ePCResult.ERROR_MARK;
                                //Bending.Menu.Config.sHResult.dValue[j] = 0;
                                break;
                            }
                            else if (sData[j + 1] == "INVALID")
                            {
                                Bending.Menu.Config.sHResult.iOKNG = (int)ePCResult.INIT;
                                //Bending.Menu.Config.sHResult.dValue[j] = 0;
                                break;
                            }
                            else //정상
                            {
                                Bending.Menu.Config.sHResult.iOKNG = 1; //측정완료
                                Bending.Menu.Config.sHResult.dValue[j] = double.Parse(sData[j + 1]) / 1000;
                            }
                        }
                        //데이터를 다 받고 bOKNG를 바꿈.
                        Bending.Menu.Config.sHResult.bOKNG = true;
                    }
                    else if (sData[0] == "READZR")
                    {
                        double[] value = new double[(sData.Length - 2) / 2];
                        for (int i = 0; i < value.Length; i++)
                        {
                            value[i] = (int.Parse(sData[2 + (i * 2)]) + (int.Parse(sData[3 + (i * 2)]) * 0x10000)) / 10000.0;
                        }
                        //double value = (int.Parse(sData[2]) + (int.Parse(sData[3]) * 0x10000)) / 10000.0;
                        switch (sData[1])
                        {
                            case nameof(eConvert.TempAttach1):
                                //value += 62;
                                //Bending.Menu.linearConverts[(int)eConvert.TempAttach1].setLinear(90, 150, 356.5);
                                SetTaxisDegree(eConvert.TempAttach1, value[0]);
                                break;
                            case nameof(eConvert.TempAttach2):
                                SetTaxisDegree(eConvert.TempAttach2, value[0]);
                                break;
                            case nameof(eConvert.EMIAttach1):
                                SetTaxisDegree(eConvert.EMIAttach1, value[0]);
                                break;
                            case nameof(eConvert.EMIAttach2):
                                SetTaxisDegree(eConvert.EMIAttach2, value[0]);
                                break;
                            case "INLAST":
                                CONST.INSPBDLAST.X1 = value[0];
                                CONST.INSPBDLAST.Y1 = value[1];
                                CONST.INSPBDLAST.X2 = value[2];
                                CONST.INSPBDLAST.Y2 = value[3];
                                break;
                            case "INAFTER":
                                CONST.INSPBDAFTER.X1 = value[0];
                                CONST.INSPBDAFTER.Y1 = value[1];
                                CONST.INSPBDAFTER.X2 = value[2];
                                CONST.INSPBDAFTER.Y2 = value[3];
                                break;
                        }

                    }
                    else if (sData[0] == "LaserSendCellID")  //210118 cjm 레이저 로그
                    {
                        if (int.TryParse(sData[1], out int visionNo))
                        {
                            Bending.Menu.frmAutoMain.Vision[visionNo].LaserSendCellID = sData[2];
                        }
                    }
                    else if (sData[0] == "MCRCellID")  //210118 cjm 레이저 로그
                    {
                        if (int.TryParse(sData[1], out int visionNo))
                        {
                            Bending.Menu.frmAutoMain.Vision[visionNo].MCRCellID = sData[2];
                        }
                    }
                    break;


                default:
                    break;
            }
            base.WndProc(ref m);
        }

        [DllImport("kernel32.dll")]
        public static extern bool SetLocalTime(ref SystemTime systemTime);

        public struct SystemTime
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }

        public void set_DateTimeSet(string sTime)
        {
            try
            {
                SystemTime lcSetDateTime = new SystemTime();

                lcSetDateTime.wYear = Convert.ToInt16(sTime.Substring(0, 4));
                lcSetDateTime.wMonth = Convert.ToInt16(sTime.Substring(4, 2));
                lcSetDateTime.wDay = Convert.ToInt16(sTime.Substring(6, 2));
                lcSetDateTime.wHour = Convert.ToInt16(sTime.Substring(8, 2));
                lcSetDateTime.wMinute = Convert.ToInt16(sTime.Substring(10, 2));
                lcSetDateTime.wSecond = Convert.ToInt16(sTime.Substring(12, 2));

                SetLocalTime(ref lcSetDateTime);
            }
            catch
            {
            }
        }

        public void readBendPreOffset(int BendNo)
        {
            string Command = "BENDPRE" + BendNo.ToString();
            int address = CONST.Address.PLC.PLC_CAL_MOVE + 62;
            if (BendNo == 1) address = address + 6;
            else if (BendNo == 2) address = address + 12;
            SendData(Command + "," + CONST.PLCDeviceType + address.ToString() + ",6");
        }

        public void readTrOffset(int TransferNo)
        {
            string Command = "TrOffset" + TransferNo.ToString();
            int address = CONST.Address.PC.TransferFirstOffset;
            if (TransferNo == 1) address = address + 2;
            else if (TransferNo == 2) address = address + 4;
            SendData(Command + "," + CONST.PLCDeviceType + address.ToString() + ",2");
        }

        //2019.07.20 EMI Align 추가
        public void readEMIData()
        {
            string Command = "EMI_DATA";
            int address = CONST.Address.PLC.PLC_CAL_MOVE + 62;
            SendData(Command + "," + CONST.PLCDeviceType + address.ToString() + ",8");
        }
        public void readPositionData(eConvert kind)
        {
            //string Command = "READZR";
            //int address = CONST.Address.PLC.POSITION;
            //switch(kind)
            //{
            //    case eConvert.TempAttach1:
            //        address = CONST.AAM_PLC2.MotorAddress.TempAttachTLeft;
            //        break;
            //    case eConvert.TempAttach2:
            //        address = CONST.AAM_PLC2.MotorAddress.TempAttachTRight;
            //        break;
            //    case eConvert.EMIAttach1:
            //        address = CONST.AAM_PLC2.MotorAddress.EMIAttachTLeft;
            //        break;
            //    case eConvert.EMIAttach2:
            //        address = CONST.AAM_PLC2.MotorAddress.EMIAttachTRight;
            //        break;
            //}
            //SendData(Command + "," + kind.ToString() + "," + CONST.PLCDeviceType + address.ToString() + ",1");
        }
        public void readPositionData(int MotionNO)
        {
            //안씀
            //string Command = "POSITION" + MotionNO.ToString();
            //int address = CONST.Address.PLC.POSITION;
            //switch (MotionNO)
            //{
            //    case (int)CONST.AAM_PLC1.Position.Conveyor.MotionNo:
            //        address = address + 0;
            //        break;
            //    case (int)CONST.AAM_PLC1.Position.LoadingBuffer.MotionNo:
            //        address = address + 6;
            //        break;
            //    case (int)CONST.AAM_PLC1.Position.SCFPanel.MotionNo:
            //        address = address + 12;
            //        break;
            //    case (int)CONST.AAM_PLC1.Position.SCFReel.MotionNo:
            //        address = address + 18;
            //        break;
            //    case (int)CONST.AAM_PLC1.Position.SCFPick.MotionNo:
            //        address = address + 24;
            //        break;
            //    case (int)CONST.AAM_PLC1.Position.BendPre.MotionNo:
            //        address = address + 30;
            //        break;

            //    case (int)CONST.AAM_PLC2.Position.Bend1Trans.MotionNo:
            //        address = address + 24;
            //        break;
            //    case (int)CONST.AAM_PLC2.Position.Bend2Trans.MotionNo:
            //        address = address + 32;
            //        break;
            //    case (int)CONST.AAM_PLC2.Position.Bend3Trans.MotionNo:
            //        address = address + 40;
            //        break;
            //    case (int)CONST.AAM_PLC2.Position.Bend1Arm.MotionNo:
            //        address = address + 0;
            //        break;
            //    case (int)CONST.AAM_PLC2.Position.Bend2Arm.MotionNo:
            //        address = address + 8;
            //        break;
            //    case (int)CONST.AAM_PLC2.Position.Bend3Arm.MotionNo:
            //        address = address + 16;
            //        break;
            //    case (int)CONST.AAM_PLC2.Position.TempAttach.MotionNo:
            //        address = address + 48;
            //        break;
            //    case (int)CONST.AAM_PLC2.Position.EMIAttach.MotionNo:
            //        address = address + 54;
            //        break;
            //    case (int)CONST.AAM_PLC2.Position.EMIAttachInspection.MotionNo:
            //        address = address + 60;
            //        break;
            //}

            //SendData(Command + "," + CONST.PLCDeviceType + address.ToString() + ",6");
        }

        public void setReqStart()
        {
            SendData("*1," + CONST.PLCDeviceType + CONST.Address.PC.BITCONTROL + ",1");
        }

        public void setCalRepStart()
        {
            SendData("*2," + CONST.PLCDeviceType + CONST.Address.PC.CALIBRATION.ToString() + ",1");
        }

        public void setCalRepStop()
        {
            SendData("*2," + CONST.PLCDeviceType + CONST.Address.PC.CALIBRATION + 1 + ",0");
        }

        private bool oldAutoMode = false;
        static bool[] bPLCReq1 = new bool[32];
        static bool[] bPLCReq2 = new bool[32];
        private void CheckReq(int iData, int AdNo, bool bsecond = false)
        {

            // Test
            //return;
            if (CONST.simulation) return;

            int Len = 0;
            if (AdNo == 0)
            {
                Len = 0;
            }
            else if (AdNo == 1)
            {
                Len = 16;
                iData = iData * 0x10000;
            }
            if (bsecond)
            {
                for (int i = Len; i < Len + 16; i++)
                {
                    bPLCReq2[i] = (iData & (int)Math.Pow(2, i)) / (int)Math.Pow(2, i) == 1 ? true : false;
                }
                CONST.bPLCReq = bPLCReq1.Concat(bPLCReq2).ToArray();
            }
            else
            {
                for (int i = Len; i < Len + 16; i++)
                {
                    bPLCReq1[i] = (iData & (int)Math.Pow(2, i)) / (int)Math.Pow(2, i) == 1 ? true : false;
                }
                CONST.bPLCReq = bPLCReq1.Concat(bPLCReq2).ToArray();

                CONST.plcAutomode = !bPLCReq1[CONST.CUnit.BitControl.plcAuto];

                if (oldAutoMode != CONST.plcAutomode)
                {
                    oldAutoMode = CONST.plcAutomode;
                    if (CONST.plcAutomode)
                    {
                        // 사용 유무 테스트 후 결정
                        //if (CONST.PCName == "AAM_PC1")
                        //{
                        //    //Bending.Menu.frmAutoMain.setCurrentPos((int)CONST.AAM_PC1.Position.LOADPRE.MotionNo, eCalPos.LoadPre_L);
                        //    //Bending.Menu.frmAutoMain.setCurrentPos((int)CONST.AAM_PC1.Position.SCFPANEL.MotionNo, eCalPos.SCFPanel_L);
                        //    //Bending.Menu.frmAutoMain.setCurrentPos((int)CONST.AAM_PC1.Position.SCFREEL.MotionNo, eCalPos.SCFReel_L);
                        //    //Bending.Menu.frmAutoMain.setCurrentPos((int)CONST.AAM_PC1.Position.BENDPRE.MotionNo, eCalPos.BendingPre_L);
                        //    readBendPreOffset(0);
                        //    readBendPreOffset(1);
                        //    readBendPreOffset(2);

                        //    readTrOffset(0);
                        //    readTrOffset(1);
                        //    readTrOffset(2);

                        //    //삭제
                        //    // Attach 부의 Theta와 Vision 위치의 Theta가 다른 것을 보정하기 위해 사용함.
                        //    //readPositionData((int)CONST.AAM_PC1.Position.SCFATTACH.MotionNo);
                        //    //readPositionData((int)CONST.AAM_PC1.Position.LOADPICK.MotionNo);
                        //    //readPositionData((int)CONST.AAM_PC1.Position.LOADPRE.MotionNo);
                        //}
                        //else if (CONST.PCName == "AAM_PC2")
                        //{
                        //    //Bending.Menu.frmAutoMain.setCurrentPos((int)CONST.AAM_PC2.Position.BD1Arm.MotionNo, eCalPos.Bend1Arm_L);
                        //    //Bending.Menu.frmAutoMain.setCurrentPos((int)CONST.AAM_PC2.Position.BD2Arm.MotionNo, eCalPos.Bend2Arm_L);
                        //    //Bending.Menu.frmAutoMain.setCurrentPos((int)CONST.AAM_PC2.Position.BD3Arm.MotionNo, eCalPos.Bend3Arm_L);
                        //}
                    }
                }
            }

        }

        public void setResult(int sCnt, int Length, string value)
        {
            //체크 요망
            SendData(CONST.PLCDeviceType + Convert.ToString(sCnt) + "," + Length + "," + value.ToString());
        }

        public void setCalReq(string calreply)
        {
            int[] iData = new int[2];
            uint ResultValue = 0;
            ResultValue = Convert.ToUInt32(calreply, 2);

            iData[0] = Convert.ToInt32(ResultValue) & 0xFFFF;
            iData[1] = (int)(Convert.ToInt32(ResultValue) & 0xFFFF0000) / 0x10000;

            SendData(CONST.PLCDeviceType + CONST.Address.PC.CALIBRATION.ToString() + ",2," + iData[0] + "^" + iData[1]);
        }

        private void getCalRep(int iData, int AdNo)
        {
            int Len = 0;
            if (AdNo == 0)
            {
                Len = 0;
            }
            else if (AdNo == 1)
            {
                Len = 16;
                iData = iData * 0x10000;
            }

            for (int i = Len; i < Len + 16; i++)
            {
                CONST.bCalReply[i] = (iData & (int)Math.Pow(2, i)) / (int)Math.Pow(2, i) == 1 ? true : false;
            }
        }

        public patternSearchResult[] targetPosResult = new patternSearchResult[5];

        public int GetMotionAddress(int MotorNo)
        {
            //여기 들어오면 안됨.
            int address = CONST.Address.PC.VISIONOFFSET;
            //switch (MotorNo)
            //{
            //    case (int)CONST.AAM_PLC1.Position.Conveyor.MotionNo:
            //        address = address + 0;
            //        break;
            //    case (int)CONST.AAM_PLC1.Position.LoadingBuffer.MotionNo:
            //        address = address + 6;
            //        break;
            //    case (int)CONST.AAM_PLC1.Position.SCFPanel.MotionNo:
            //        address = address + 12;
            //        break;
            //    case (int)CONST.AAM_PLC1.Position.SCFReel.MotionNo:
            //        address = address + 18;
            //        break;
            //    case (int)CONST.AAM_PLC1.Position.SCFPick.MotionNo:
            //        address = address + 24;
            //        break;
            //    case (int)CONST.AAM_PLC1.Position.BendPre.MotionNo:
            //        address = address + 30;
            //        break;

            //    case (int)CONST.AAM_PLC2.Position.Bend1Trans.MotionNo:
            //        address = address + 30;
            //        break;
            //    case (int)CONST.AAM_PLC2.Position.Bend2Trans.MotionNo:
            //        address = address + 36;
            //        break;
            //    case (int)CONST.AAM_PLC2.Position.Bend3Trans.MotionNo:
            //        address = address + 42;
            //        break;
            //    case (int)CONST.AAM_PLC2.Position.Bend1Arm.MotionNo:
            //        address = address + 0;
            //        break;
            //    case (int)CONST.AAM_PLC2.Position.Bend2Arm.MotionNo:
            //        address = address + 10;
            //        break;
            //    case (int)CONST.AAM_PLC2.Position.Bend3Arm.MotionNo:
            //        address = address + 20;
            //        break;
            //    case (int)CONST.AAM_PLC2.Position.TempAttach.MotionNo:
            //        address = address + 48;
            //        break;
            //    case (int)CONST.AAM_PLC2.Position.EMIAttach.MotionNo:
            //        address = address + 54;
            //        break;
            //}

            return address;
        }



        public void writeCalData(double[] dData)
        {
            int[] iData = new int[dData.Length * 2];
            string WriteData = "";
            for (int i = 0; i < dData.Length; i++)
            {
                iData[2 * i + 0] = Convert.ToInt32(dData[i] * 10000) & 0xFFFF;
                iData[2 * i + 1] = (int)((Convert.ToInt32(dData[i] * 10000) & 0xFFFF0000) / 0x10000);
                if (i == dData.Length - 1) WriteData = WriteData + iData[2 * i + 0] + "^" + iData[2 * i + 1]; // 마지막 데이터 저장 부분 수정
                else WriteData = WriteData + iData[2 * i + 0] + "^" + iData[2 * i + 1] + "^";
            }
            int Address = CONST.Address.PC.PC_CAL_MOVE + 4;

            SendData("ZR" + Address + "," + iData.Length + "," + WriteData);         //Target Data
        }

        public void writeBendPreOffsetData(double[] dData, int bendNo)
        {
            int[] iData = new int[dData.Length * 2];
            string WriteData = "";
            for (int i = 0; i < dData.Length; i++)
            {
                iData[2 * i + 0] = Convert.ToInt32(dData[i] * 10000) & 0xFFFF;
                iData[2 * i + 1] = (int)((Convert.ToInt32(dData[i] * 10000) & 0xFFFF0000) / 0x10000);
                if (i == dData.Length - 1) WriteData = WriteData + iData[2 * i + 0] + "^" + iData[2 * i + 1];
                else WriteData = WriteData + iData[2 * i + 0] + "^" + iData[2 * i + 1] + "^";
            }
            int Address = CONST.Address.PC.PC_CAL_MOVE + 62;
            if (bendNo == 1) Address = Address + 6;
            else if (bendNo == 2) Address = Address + 12;

            SendData("ZR" + Address + "," + iData.Length + "," + WriteData);         //Target Data
        }

        //2019.07.20 EMI Align 추가
        //public void writeEMIData()
        //{
        //    double[] dData = new double[8];
        //    dData[0] = Bending.Menu.frmSetting.revData.mSizeSpecRatio.plusTMoveRatio;
        //    dData[1] = Bending.Menu.frmSetting.revData.mSizeSpecRatio.plusYMoveRatio;
        //    dData[2] = Bending.Menu.frmSetting.revData.mSizeSpecRatio.minusTMoveRatio;
        //    dData[3] = Bending.Menu.frmSetting.revData.mSizeSpecRatio.minusYMoveRatio;

        //    dData[4] = Bending.Menu.frmSetting.revData.mLcheck[CONST.AAM_PC1.Vision_No.vsEMIAttach].Target_Pos.X;
        //    dData[5] = Bending.Menu.frmSetting.revData.mLcheck[CONST.AAM_PC1.Vision_No.vsEMIAttach].Target_Pos.Y;
        //    dData[6] = Bending.Menu.frmSetting.revData.mLcheck[CONST.AAM_PC1.Vision_No.vsEMIAttach].SubTarget_Pos.X;
        //    dData[7] = Bending.Menu.frmSetting.revData.mLcheck[CONST.AAM_PC1.Vision_No.vsEMIAttach].SubTarget_Pos.Y;

        //    int[] iData = new int[dData.Length * 2];
        //    string WriteData = "";
        //    for (int i = 0; i < dData.Length; i++)
        //    {
        //        iData[2 * i + 0] = Convert.ToInt32(dData[i] * 10000) & 0xFFFF;
        //        iData[2 * i + 1] = (int)((Convert.ToInt32(dData[i] * 10000) & 0xFFFF0000) / 0x10000);
        //        if (i == dData.Length - 1) WriteData = WriteData + iData[2 * i + 0] + "^" + iData[2 * i + 1];
        //        else WriteData = WriteData + iData[2 * i + 0] + "^" + iData[2 * i + 1] + "^";
        //    }
        //    int Address = CONST.Address.PC.PC_CAL_MOVE + 62;
        //    SendData("ZR" + Address + "," + iData.Length + "," + WriteData);         //Target Data
        //}

        //2020.09.25 lkw
        public cs2DAlign.ptXYT writeAlignXYTOffset(int iStartMotorNo, double X, double Y, double T, eConvert convert = eConvert.notUse, bool XYAxisRevers = false)
        {
            cs2DAlign.ptXYT resultxyt = new cs2DAlign.ptXYT();

            #region TYRMove
            if (convert != eConvert.notUse)
            {
                csLinearConvert.ptXYT realValue = new csLinearConvert.ptXYT();
                realValue.X = X;
                realValue.Y = Y;
                realValue.T = T;
                csLinearConvert.ptYTR returnYTR = Bending.Menu.linearConverts[(int)convert].convert_XYTtoYTR(realValue);

                X = returnYTR.R;
                Y = returnYTR.Y;
                T = returnYTR.T;
            }
            #endregion TYRMove


            


            int[] iData = new int[6];
            int XYTLength = 6;
            int iAddress = CONST.Address.PC.VISIONOFFSET;

            if (iStartMotorNo < 100) //pcy200924
            {
                iAddress = GetMotionAddress(iStartMotorNo);
            }
            else
            {
                iAddress = iStartMotorNo;
            }

            //Load Pre 부 Vision 방향과 Motor 방향 다름...
            //if (iAddress == Address.VisionOffset.LoadingPre && CONST.PCNo != 1)
            //210215 cjm X-Y Axis Revers add and change
            if (XYAxisRevers)
            {
                double tempX = X;
                X = Y;
                Y = tempX;
            }

            iData[0] = Convert.ToInt32(X * 10000) & 0xFFFF;
            iData[1] = (int)((Convert.ToInt32(X * 10000) & 0xFFFF0000) / 0x10000);
            iData[2] = Convert.ToInt32(Y * 10000) & 0xFFFF;
            iData[3] = (int)((Convert.ToInt32(Y * 10000) & 0xFFFF0000) / 0x10000);
            iData[4] = Convert.ToInt32(T * 10000) & 0xFFFF;
            iData[5] = (int)((Convert.ToInt32(T * 10000) & 0xFFFF0000) / 0x10000);

            string WriteData = iData[0] + "^" + iData[1] + "^" + iData[2] + "^" + iData[3] + "^" + iData[4] + "^" + iData[5];

            SendData("ZR" + iAddress + "," + XYTLength.ToString() + "," + WriteData);         //Target Data

            resultxyt.X = X;
            resultxyt.Y = Y;
            resultxyt.T = T;

            return resultxyt;
        }

        public void writeAlignUVWOffset(int iStartMotorNo, double X1, double Y1, double X2, double Y2)
        {
            int[] iData = new int[8];
            int UVWLength = 8;
            //X2 = -X2;
            int Address = CONST.Address.PC.VISIONOFFSET;
            if (iStartMotorNo < 100) //pcy200924
            {
                Address = GetMotionAddress(iStartMotorNo);
            }
            else
            {
                Address = iStartMotorNo;
            }

            iData[0] = Convert.ToInt32(X1 * 10000) & 0xFFFF;
            iData[1] = (int)((Convert.ToInt32(X1 * 10000) & 0xFFFF0000) / 0x10000);
            iData[2] = Convert.ToInt32(X2 * 10000) & 0xFFFF;
            iData[3] = (int)((Convert.ToInt32(X2 * 10000) & 0xFFFF0000) / 0x10000);
            iData[4] = Convert.ToInt32(Y1 * 10000) & 0xFFFF;
            iData[5] = (int)((Convert.ToInt32(Y1 * 10000) & 0xFFFF0000) / 0x10000);
            iData[6] = Convert.ToInt32(Y2 * 10000) & 0xFFFF;
            iData[7] = (int)((Convert.ToInt32(Y2 * 10000) & 0xFFFF0000) / 0x10000);

            string WriteData = iData[0] + "^" + iData[1] + "^" + iData[2] + "^" + iData[3] + "^" + iData[4] + "^" + iData[5] + "^" + iData[6] + "^" + iData[7];

            SendData("ZR" + Address + "," + UVWLength.ToString() + "," + WriteData);         //Target Data
            Console.WriteLine("address" + Address.ToString() + "," + X1.ToString() + "," + X2.ToString() + "," + Y1.ToString() + "," + Y2.ToString());
        }

        public void TransferTOffset(int TransferNo, double Theta)
        {
            int[] iData = new int[8];
            int UVWLength = 2;

            int address = CONST.Address.PC.TransferFirstOffset;
            if (TransferNo == 1) address = address + 2;
            else if (TransferNo == 2) address = address + 4;

            //int address = CONST.Address.PC.TransferFirstOffset;
            //if (TransferNo == 1) address = address + 8;
            //else if (TransferNo == 2) address = address + 16;

            iData[0] = Convert.ToInt32(Theta * 10000) & 0xFFFF;
            iData[1] = (int)((Convert.ToInt32(Theta * 10000) & 0xFFFF0000) / 0x10000);
            //iData[2] = Convert.ToInt32(Y1 * 10000) & 0xFFFF;
            //iData[3] = (int)((Convert.ToInt32(Y1 * 10000) & 0xFFFF0000) / 0x10000);
            //iData[4] = Convert.ToInt32(X2 * 10000) & 0xFFFF;
            //iData[5] = (int)((Convert.ToInt32(X2 * 10000) & 0xFFFF0000) / 0x10000);
            //iData[6] = Convert.ToInt32(Y2 * 10000) & 0xFFFF;
            //iData[7] = (int)((Convert.ToInt32(Y2 * 10000) & 0xFFFF0000) / 0x10000);

            string WriteData = iData[0] + "^" + iData[1];// + "^" + iData[2] + "^" + iData[3] + "^" + iData[4] + "^" + iData[5] + "^" + iData[6] + "^" + iData[7];

            SendData("ZR" + address + "," + UVWLength.ToString() + "," + WriteData);         //Target Data
        }

        public void writeBendPreUVWOffset(rs2DAlign.cs2DAlign.ptXYT bend1, rs2DAlign.cs2DAlign.ptXYT bend2, rs2DAlign.cs2DAlign.ptXYT bend3)
        {
            int[] iData = new int[18];
            int UVWLength = 18;

            int Address = CONST.Address.PC.VISIONOFFSET + 24;

            iData[0] = Convert.ToInt32(bend1.X * 10000) & 0xFFFF;
            iData[1] = (int)((Convert.ToInt32(bend1.X * 10000) & 0xFFFF0000) / 0x10000);
            iData[2] = Convert.ToInt32(bend1.Y * 10000) & 0xFFFF;
            iData[3] = (int)((Convert.ToInt32(bend1.Y * 10000) & 0xFFFF0000) / 0x10000);
            iData[4] = Convert.ToInt32(bend1.T * 10000) & 0xFFFF;
            iData[5] = (int)((Convert.ToInt32(bend1.T * 10000) & 0xFFFF0000) / 0x10000);
            //iData[6] = Convert.ToInt32(bend1.Y2 * 10000) & 0xFFFF;
            //iData[7] = (int)((Convert.ToInt32(bend1.Y2 * 10000) & 0xFFFF0000) / 0x10000);

            iData[6] = Convert.ToInt32(bend2.X * 10000) & 0xFFFF;
            iData[7] = (int)((Convert.ToInt32(bend2.X * 10000) & 0xFFFF0000) / 0x10000);
            iData[8] = Convert.ToInt32(bend2.Y * 10000) & 0xFFFF;
            iData[9] = (int)((Convert.ToInt32(bend2.Y * 10000) & 0xFFFF0000) / 0x10000);
            iData[10] = Convert.ToInt32(bend2.T * 10000) & 0xFFFF;
            iData[11] = (int)((Convert.ToInt32(bend2.T * 10000) & 0xFFFF0000) / 0x10000);
            //iData[14] = Convert.ToInt32(bend2.Y2 * 10000) & 0xFFFF;
            //iData[15] = (int)((Convert.ToInt32(bend2.Y2 * 10000) & 0xFFFF0000) / 0x10000);

            iData[12] = Convert.ToInt32(bend3.X * 10000) & 0xFFFF;
            iData[13] = (int)((Convert.ToInt32(bend3.X * 10000) & 0xFFFF0000) / 0x10000);
            iData[14] = Convert.ToInt32(bend3.Y * 10000) & 0xFFFF;
            iData[15] = (int)((Convert.ToInt32(bend3.Y * 10000) & 0xFFFF0000) / 0x10000);
            iData[16] = Convert.ToInt32(bend3.T * 10000) & 0xFFFF;
            iData[17] = (int)((Convert.ToInt32(bend3.T * 10000) & 0xFFFF0000) / 0x10000);
            //iData[22] = Convert.ToInt32(bend3.Y2 * 10000) & 0xFFFF;
            //iData[23] = (int)((Convert.ToInt32(bend3.Y2 * 10000) & 0xFFFF0000) / 0x10000);

            string WriteData = iData[0] + "^" + iData[1] + "^" + iData[2] + "^" + iData[3] + "^" + iData[4] + "^" + iData[5] + "^" + iData[6] + "^" + iData[7] + "^" +
                               iData[8] + "^" + iData[9] + "^" + iData[10] + "^" + iData[11] + "^" + iData[12] + "^" + iData[13] + "^" + iData[14] + "^" + iData[15] + "^" +
                               iData[16] + "^" + iData[17];// + "^" + iData[18] + "^" + iData[19] + "^" + iData[20] + "^" + iData[21] + "^" + iData[22] + "^" + iData[23];

            SendData("ZR" + Address + "," + UVWLength.ToString() + "," + WriteData);         //Target Data
        }

        //public void writeBendPreUVWOffset(rs2DAlign.cs2DAlign.ptXXYY bend1, rs2DAlign.cs2DAlign.ptXXYY bend2, rs2DAlign.cs2DAlign.ptXXYY bend3)
        //{
        //    int[] iData = new int[24];
        //    int UVWLength = 24;

        //    int Address = CONST.Address.PC.VISIONOFFSET + 24;

        //    iData[0] = Convert.ToInt32(bend1.X1 * 10000) & 0xFFFF;
        //    iData[1] = (int)((Convert.ToInt32(bend1.X1 * 10000) & 0xFFFF0000) / 0x10000);
        //    iData[2] = Convert.ToInt32(bend1.X2 * 10000) & 0xFFFF;
        //    iData[3] = (int)((Convert.ToInt32(bend1.X2 * 10000) & 0xFFFF0000) / 0x10000);
        //    iData[4] = Convert.ToInt32(bend1.Y1 * 10000) & 0xFFFF;
        //    iData[5] = (int)((Convert.ToInt32(bend1.Y1 * 10000) & 0xFFFF0000) / 0x10000);
        //    iData[6] = Convert.ToInt32(bend1.Y2 * 10000) & 0xFFFF;
        //    iData[7] = (int)((Convert.ToInt32(bend1.Y2 * 10000) & 0xFFFF0000) / 0x10000);

        //    iData[8] = Convert.ToInt32(bend2.X1 * 10000) & 0xFFFF;
        //    iData[9] = (int)((Convert.ToInt32(bend2.X1 * 10000) & 0xFFFF0000) / 0x10000);
        //    iData[10] = Convert.ToInt32(bend2.X2 * 10000) & 0xFFFF;
        //    iData[11] = (int)((Convert.ToInt32(bend2.X2 * 10000) & 0xFFFF0000) / 0x10000);
        //    iData[12] = Convert.ToInt32(bend2.Y1 * 10000) & 0xFFFF;
        //    iData[13] = (int)((Convert.ToInt32(bend2.Y1 * 10000) & 0xFFFF0000) / 0x10000);
        //    iData[14] = Convert.ToInt32(bend2.Y2 * 10000) & 0xFFFF;
        //    iData[15] = (int)((Convert.ToInt32(bend2.Y2 * 10000) & 0xFFFF0000) / 0x10000);

        //    iData[16] = Convert.ToInt32(bend3.X1 * 10000) & 0xFFFF;
        //    iData[17] = (int)((Convert.ToInt32(bend3.X1 * 10000) & 0xFFFF0000) / 0x10000);
        //    iData[18] = Convert.ToInt32(bend3.X2 * 10000) & 0xFFFF;
        //    iData[19] = (int)((Convert.ToInt32(bend3.X2 * 10000) & 0xFFFF0000) / 0x10000);
        //    iData[20] = Convert.ToInt32(bend3.Y1 * 10000) & 0xFFFF;
        //    iData[21] = (int)((Convert.ToInt32(bend3.Y1 * 10000) & 0xFFFF0000) / 0x10000);
        //    iData[22] = Convert.ToInt32(bend3.Y2 * 10000) & 0xFFFF;
        //    iData[23] = (int)((Convert.ToInt32(bend3.Y2 * 10000) & 0xFFFF0000) / 0x10000);

        //    string WriteData = iData[0] + "^" + iData[1] + "^" + iData[2] + "^" + iData[3] + "^" + iData[4] + "^" + iData[5] + "^" + iData[6] + "^" + iData[7] + "^" +
        //                       iData[8] + "^" + iData[9] + "^" + iData[10] + "^" + iData[11] + "^" + iData[12] + "^" + iData[13] + "^" + iData[14] + "^" + iData[15] + "^" +
        //                       iData[16] + "^" + iData[17] + "^" + iData[18] + "^" + iData[19] + "^" + iData[20] + "^" + iData[21] + "^" + iData[22] + "^" + iData[23];

        //    SendData("ZR" + Address + "," + UVWLength.ToString() + "," + WriteData);         //Target Data

        //}
        //pcy190507 CPK PLC로 전송
        public void writeCPK(double[] dData)
        {
            int[] iData = new int[8];

            int Address = CONST.Address.PC.CPK;
            // iData[0] = Convert.ToInt32(bend1.X1 * 10000) & 0xFFFF;
            //    iData[1] = (int)((Convert.ToInt32(bend1.X1 * 10000) & 0xFFFF0000) / 0x10000);

            iData[0] = Convert.ToInt32(dData[0] * 10000) & 0xFFFF;
            iData[1] = (int)((Convert.ToInt32(dData[0] * 10000) & 0xFFFF0000) / 0x10000);
            iData[2] = Convert.ToInt32(dData[1] * 10000) & 0xFFFF;
            iData[3] = (int)((Convert.ToInt32(dData[1] * 10000) & 0xFFFF0000) / 0x10000);
            iData[4] = Convert.ToInt32(dData[2] * 10000) & 0xFFFF;
            iData[5] = (int)((Convert.ToInt32(dData[2] * 10000) & 0xFFFF0000) / 0x10000);
            iData[6] = Convert.ToInt32(dData[3] * 10000) & 0xFFFF;
            iData[7] = (int)((Convert.ToInt32(dData[3] * 10000) & 0xFFFF0000) / 0x10000);

            string WriteData = iData[0] + "^" + iData[1] + "^" + iData[2] + "^" + iData[3] + "^" + iData[4] + "^" + iData[5] + "^" + iData[6] + "^" + iData[7];

            SendData(CONST.PLCDeviceType + Address + "," + iData.Length + "," + WriteData);
        }

        public bool TraceDataRead(int iTraceNo, ref double[] dX, ref double[] dZ, ref double[] dR, ref double Point, ref double Roatation, ref double EndPosZ)
        {
            for (int i = 0; i < 100; i++)
            {
                //dX[i] = CONST.m_dMainTraceY[i];
                //dZ[i] = CONST.m_dMainTraceZ[i];
                //dR[i] = CONST.m_dMainTraceT[i];

                //2018.07.30 Trace Data 변경
                dX[i] = CONST.m_dTraceY[iTraceNo, i];
                dZ[i] = CONST.m_dTraceZ[iTraceNo, i];
                dR[i] = CONST.m_dTraceT[iTraceNo, i];
            }

            Point = CONST.TracePoint[iTraceNo];
            Roatation = CONST.RadiusOfRotation[iTraceNo];
            EndPosZ = CONST.endPointZ[iTraceNo];

            return true;
        }

        public void writeTraceData(int iTraceNo)
        {
            double[] dX = new double[100];
            double[] dZ = new double[100];
            double[] dR = new double[100];

            double tPoint = new double();
            double tRoatation = new double();
            double tEndPosZ = new double();

            if (TraceDataRead(iTraceNo, ref dX, ref dZ, ref dR, ref tPoint, ref tRoatation, ref tEndPosZ))
            {
                string sAddX = "";
                string sAddZ = "";
                string sAddR = "";

                string sXData = null;
                string sZData = null;
                string sRData = null;

                int iWordLength = 0;

                int[] iXData = new int[2];
                int[] iZData = new int[2];
                int[] iRData = new int[2];

                //string sAddPoint = "";
                //string sAddRotation = "";
                //string sAddEndPosZ = "";

                string sPoint = null;
                string sRotation = null;
                string sEndPosZ = null;

                int[] iPoint = new int[2];
                int[] iRotation = new int[2];
                int[] iEndPosZ = new int[2];

                if (iTraceNo == 0)
                {
                    sAddX = CONST.PLCDeviceType + CONST.Address.PC.BENDING1.ToString();
                    sAddZ = CONST.PLCDeviceType + (CONST.Address.PC.BENDING1 + 200).ToString();
                    sAddR = CONST.PLCDeviceType + (CONST.Address.PC.BENDING1 + 400).ToString();

                    //sAddPoint = CONST.PLCDeviceType + CONST.Address.PC.TraceInfoArm1.ToString();
                    //sAddRotation = CONST.PLCDeviceType + (CONST.Address.PC.TraceInfoArm1 + 2).ToString();
                    //sAddEndPosZ = CONST.PLCDeviceType + (CONST.Address.PC.TraceInfoArm1 + 4).ToString();
                }
                else if (iTraceNo == 1)
                {
                    sAddX = CONST.PLCDeviceType + CONST.Address.PC.BENDING2.ToString();
                    sAddZ = CONST.PLCDeviceType + (CONST.Address.PC.BENDING2 + 200).ToString();
                    sAddR = CONST.PLCDeviceType + (CONST.Address.PC.BENDING2 + 400).ToString();

                    //sAddPoint = CONST.PLCDeviceType + CONST.Address.PC.TraceInfoArm2.ToString();
                    //sAddRotation = CONST.PLCDeviceType + (CONST.Address.PC.TraceInfoArm2 + 2).ToString();
                    //sAddEndPosZ = CONST.PLCDeviceType + (CONST.Address.PC.TraceInfoArm2 + 4).ToString();
                }
                else if (iTraceNo == 2)
                {
                    sAddX = CONST.PLCDeviceType + CONST.Address.PC.BENDING3.ToString();
                    sAddZ = CONST.PLCDeviceType + (CONST.Address.PC.BENDING3 + 200).ToString();
                    sAddR = CONST.PLCDeviceType + (CONST.Address.PC.BENDING3 + 400).ToString();

                    //sAddPoint = CONST.PLCDeviceType + CONST.Address.PC.TraceInfoArm3.ToString();
                    //sAddRotation = CONST.PLCDeviceType + (CONST.Address.PC.TraceInfoArm3 + 2).ToString();
                    //sAddEndPosZ = CONST.PLCDeviceType + (CONST.Address.PC.TraceInfoArm3 + 4).ToString();
                }

                for (int i = 0; i < dX.Length; i++)
                {
                    iRData[0] = (int)(dR[i] * 10000) & 0xFFFF;
                    iRData[1] = (int)(((int)(dR[i] * 10000) & 0xFFFF0000) / 0x10000);
                    iXData[0] = (int)(dX[i] * 10000) & 0xFFFF;
                    iXData[1] = (int)(((int)(dX[i] * 10000) & 0xFFFF0000) / 0x10000);
                    iZData[0] = (int)(dZ[i] * 10000) & 0xFFFF;
                    iZData[1] = (int)(((int)(dZ[i] * 10000) & 0xFFFF0000) / 0x10000);

                    if (i == 99) sXData = sXData + Convert.ToString(iXData[0]) + "^" + Convert.ToString(iXData[1]); else sXData = sXData + Convert.ToString(iXData[0]) + "^" + Convert.ToString(iXData[1]) + "^";
                    if (i == 99) sZData = sZData + Convert.ToString(iZData[0]) + "^" + Convert.ToString(iZData[1]); else sZData = sZData + Convert.ToString(iZData[0]) + "^" + Convert.ToString(iZData[1]) + "^";
                    if (i == 99) sRData = sRData + Convert.ToString(iRData[0]) + "^" + Convert.ToString(iRData[1]); else sRData = sRData + Convert.ToString(iRData[0]) + "^" + Convert.ToString(iRData[1]) + "^";
                    iWordLength = iWordLength + 2;
                }

                iPoint[0] = (int)(tPoint * 10000) & 0xFFFF;
                iPoint[1] = (int)(((int)(tPoint * 10000) & 0xFFFF0000) / 0x10000);
                iRotation[0] = (int)(tRoatation * 10000) & 0xFFFF;
                iRotation[1] = (int)(((int)(tRoatation * 10000) & 0xFFFF0000) / 0x10000);
                iEndPosZ[0] = (int)(tEndPosZ * 10000) & 0xFFFF;
                iEndPosZ[1] = (int)(((int)(tEndPosZ * 10000) & 0xFFFF0000) / 0x10000);

                sPoint = Convert.ToString(iPoint[0]) + "^" + Convert.ToString(iPoint[1]);
                sRotation = Convert.ToString(iRotation[0]) + "^" + Convert.ToString(iRotation[1]);
                sEndPosZ = Convert.ToString(iEndPosZ[0]) + "^" + Convert.ToString(iEndPosZ[1]);

                SendData(sAddX + "," + iWordLength.ToString() + "," + sXData.ToString());
                SendData(sAddZ + "," + iWordLength.ToString() + "," + sZData.ToString());
                SendData(sAddR + "," + iWordLength.ToString() + "," + sRData.ToString());

                //SendData(sAddPoint + "," + "2" + "," + sPoint.ToString());
                //SendData(sAddRotation + "," + "2" + "," + sRotation.ToString());
                //SendData(sAddEndPosZ + "," + "2" + "," + sEndPosZ.ToString());

                // ucSetting.DB.TraceDataUpdate(iTraceNo, dX[dX.Length - 1], dZ[dZ.Length - 1], dR[dR.Length - 1]);
            }
        }
        public void DataWrite_Word(int Address, params double[] data) //pcy210119
        {
            int[] iData = new int[2];
            string WriteData = "";

            for (int i = 0; i < data.Length; i++)
            {
                iData[0] = Convert.ToInt32(data[i] * 10000) & 0xFFFF;
                iData[1] = (int)((Convert.ToInt32(data[i] * 10000) & 0xFFFF0000) / 0x10000);
                if (i > 0) WriteData += "^";
                WriteData += iData[0] + "^" + iData[1];
            }
            SendData(CONST.PLCDeviceType + Address + "," + (data.Length * 2) + "," + WriteData);
        }
        public void DV_DataWrite(int Address, double[] data)
        {
      //      int Address = CONST.Address.PC.DV + kind;
            int[] iData = new int[2];
            string WriteData = "";

            for (int i = 0; i < data.Length; i++)
            {
                iData[0] = Convert.ToInt32(data[i] * 10000) & 0xFFFF;
                iData[1] = (int)((Convert.ToInt32(data[i] * 10000) & 0xFFFF0000) / 0x10000);
                if (i > 0) WriteData += "^";
                WriteData += iData[0] + "^" + iData[1];
            }
            SendData(CONST.PLCDeviceType + Address + "," + (data.Length * 2) + "," + WriteData);
        }

        public void DataWrite_Score_OneWord(int Address, double[] data)
        {
            //int Address = CONST.Address.PC.MatchingScore + kind;
            int[] iData = new int[1];
            string WriteData = "";

            for (int i = 0; i < data.Length; i++)
            {
                iData[0] = Convert.ToInt32(data[i] * 100) & 0xFFFF;
                if (i > 0) WriteData += "^";
                WriteData += iData[0];
            }
            SendData(CONST.PLCDeviceType + Address + "," + data.Length + "," + WriteData);
        }

        public void DataWrite_OneWord(int Address, int data)
        {
            int[] iData = new int[1];
            string WriteData = "";

            //for (int i = 0; i < data.Length; i++)
            //{
            iData[0] = Convert.ToInt32(data * 1) & 0xFFFF;
            //    if (i > 0) WriteData += "^";
            WriteData += iData[0];
            //}
            SendData(CONST.PLCDeviceType + Address + "," + 1 + "," + WriteData);
        }

        public void WriteSideStart(int BendNo, double[] data) //확인
        {
            //int Address = CONST.Address.PC.DV + kind;
            //int[] iData = new int[data.Length * 2];
            //string WriteData = "";

            //for (int i = 0; i < data.Length; i++)
            //{
            //    iData[2 * i] = Convert.ToInt32(data[i] * 10000) & 0xFFFF;
            //    iData[2 * i + 1] = (int)((Convert.ToInt32(data[i] * 10000) & 0xFFFF0000) / 0x10000);
            //    if (i < data.Length - 1) WriteData = WriteData + iData[2 * i] + "^" + iData[2 * i + 1] + "^";
            //    else WriteData = WriteData + iData[2 * i] + "^" + iData[2 * i + 1];
            //}
            //SendData(CONST.PLCDeviceType + Address + "," + iData.Length + "," + WriteData);
        }

        //2020.09.25 lkw
        public void readTaxisDegree(eConvert kind)
        {
            // Teaching 위치 읽기
            Bending.Menu.frmAutoMain.IF.readPositionData(kind);
        }
        public void SetTaxisDegree(eConvert kind, double dvalue)
        {
            // 응답 받으면 
            Bending.Menu.linearConverts[(int)kind].setStartDegree(dvalue);
        }

        public string changeString(string sData, int cnt)
        {
            if (sData.Length < cnt)
            {
                int strcnt = cnt - sData.Length;
                for (int i = 0; i < strcnt; i++)
                {
                    sData = sData + '\0';
                }
            }
            return sData;
        }

        public void writeAPNCode(int Address, string apnCode)
        {
            //apnCode = changeString(apnCode, 200);
            //SendData("INSP_ASP," +  CONST.PLCDeviceType + Address + "," + apnCode.Length + "," + apnCode);
            SendData("INSP_ASP," + CONST.PLCDeviceType + Address + "," + "100" + "," + apnCode);
        }

        public void DataWrite_InspeAscii(int Address, cs2DAlign.ptXXYY dist)
        {
            SendData("INSP_ASP," + CONST.PLCDeviceType + Address.ToString() + "," + "4," + dist.X1.ToString("0.0000"));
            SendData("INSP_ASP," + CONST.PLCDeviceType + (Address + 4).ToString() + "," + "4," + dist.Y1.ToString("0.0000"));
            SendData("INSP_ASP," + CONST.PLCDeviceType + (Address + 8).ToString() + "," + "4," + dist.X2.ToString("0.0000"));
            SendData("INSP_ASP," + CONST.PLCDeviceType + (Address + 12).ToString() + "," + "4," + dist.Y2.ToString("0.0000"));
            //+ "^" + dist.Y1.ToString("0.000") + "^" + dist.X2.ToString("0.000") + "^" + dist.Y2.ToString("0.000"));
        }
    }

    public static class prograssbarcolor
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);

        public static void SetState(this ProgressBar pBar, int state)
        {
            SendMessage(pBar.Handle, 1040, (IntPtr)state, IntPtr.Zero);
        }
    }
}