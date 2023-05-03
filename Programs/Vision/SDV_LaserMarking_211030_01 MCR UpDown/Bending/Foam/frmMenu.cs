using Microsoft.VisualBasic.Devices;
using rs2DAlign;
using rsLinearConvert;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
//20.12.17 lkw DL
using rsDL;

namespace Bending
{
    public partial class Menu : Form
    {
        //KSJ 20170606 Cpu,Ram,Hdd 표기 Add
        protected static PerformanceCounter cpuCounter;

        protected static PerformanceCounter ramCounter;

        public static cs2DAlign rsAlign = new cs2DAlign();

        //20.12.17 lkw DL
        public static csDL_IF rsDL = new csDL_IF();

        //2020.09.25 lkw
        public static csLinearConvert[] linearConverts = new csLinearConvert[4];  //0,1 : 가접, 2,3 : EMI

        public static frmLogIn frmlogin = new frmLogIn();
        public static csConfig Config = new csConfig();

        public static ucAutoMain frmAutoMain = new ucAutoMain();
        public static ucRecipe frmRecipe = new ucRecipe();
        public static ucSetting frmSetting = new ucSetting();
        public static ucHistory frmHistory = new ucHistory();

        //public static prograssbarcolor PrograssbarColor = new prograssbarcolor();

        private Button[] btnMenu;
        private csLog cLog = new csLog();

        public static string PATH_BASE = @"C:\EQData";
        public static string PATH_SYSTEM = "\\Calibration";
        public static string PATH_DATA = "\\Data";
        public static string PATH_DUMMY = "\\";
        public static string FILE_CALIBRATION = "Calibration";
        public static string Adjust_FILE_CALIBRATION = "Adjust";
        public static string EXTENSION_DAT = ".dat";

        public Menu()
        {
            InitializeComponent();
            lbProgramVer.Text = "Ver 21.10.30_01";
            btnMenu = new Button[] { btnAutoMain, btnRecipe, btnSetting, btnHistory };

            frmAutoMain.IF.progressBar1.Value = 90;
            frmAutoMain.IF.lbProgress.Refresh();
            frmAutoMain.IF.lbProgress.Text = Convert.ToString(frmAutoMain.IF.progressBar1.Value) + "%";
            frmAutoMain.IF.progressBar1.Refresh();

            frmAutoMain.IF.progressBar1.Value = 100;
            frmAutoMain.IF.lbProgress.Text = Convert.ToString(frmAutoMain.IF.progressBar1.Value) + "%";
            frmAutoMain.IF.lbProgress.Refresh();
            frmAutoMain.IF.progressBar1.Refresh();

            pnMenu.Controls.Add(frmAutoMain);
            pnMenu.Controls.Add(frmRecipe);
            pnMenu.Controls.Add(frmSetting);
            pnMenu.Controls.Add(frmHistory);

            frmSetting.revData.ReadData(CONST.RunRecipe.RecipeName);
            foreach (var s in frmAutoMain.Vision)
            {
                if (s.CFG.Name == "NotUse") continue;

                int iexpo = -1;
                foreach (var d in s.CFG.Exposure)
                {
                    if (d != 0)
                    {
                        iexpo = d;
                        break;
                    }
                }
                s.setExposure(iexpo, out bool bexplow); //기본 노출값 설정

                double dcont = -1;
                foreach (var d in s.CFG.Contrast)
                {
                    if (d != 0)
                    {
                        dcont = d;
                        break;
                    }
                }
                s.setContrast(dcont, out bool bcontlow); //기본 컨트라스트 설정

                int ilight = -1;
                foreach (var d in s.CFG.Light)
                {
                    if (d != 0)
                    {
                        ilight = d;
                        break;
                    }
                }
                frmAutoMain.SetLight(s.CFG.Light1Comport, s.CFG.Light1CH, ilight, s.CFG.Camno, s.CFG.LightType); //기본 조명
                frmAutoMain.SetLight(s.CFG.Light1Comport, s.CFG.SubLight1CH, s.CFG.SubLight1Value, s.CFG.Camno, s.CFG.LightType); //기본 서브조명
                frmAutoMain.SetLight(s.CFG.Light5VComport, s.CFG.Light5VCH, s.CFG.Light5VValue, s.CFG.Camno, CONST.eLightType.Light5V); //기본 5V조명
            }

            for (int i = 0; i < linearConverts.Length; i++) linearConverts[i] = new csLinearConvert();

            DisplayChange(btnAutoMain.Name);

            for (int i = 0; i < btnMenu.Length; i++)
            {
                btnMenu[i].Click += new System.EventHandler(btnMenu_Click);
            }
            label1.Text = "SDV " + CONST.PCName;

            //Cpu,Ram,Hdd 표기 Add
            GetPCStatus();

            frmSetting.SelectedIndex(0);

            CONST.bProgramReset[1] = true;
            CONST.bProgramReset[2] = true;
            CONST.bProgramReset[3] = true;

            //HEIGHT SENSOR START
            Bending.Menu.frmAutoMain.IF.SendData("HEIGHTSTART");

            frmAutoMain.IF.Visible = false;

            frmSetting.revData.Initialize();
            InitialCalibration();

            //string strCvtCalPath = PATH_BASE + PATH_SYSTEM + PATH_DATA + PATH_DUMMY + CONST.RunRecipe.RecipeName + PATH_DUMMY + FILE_CALIBRATION + EXTENSION_DAT;
            //string strAdjustCvtCalPath = PATH_BASE + PATH_SYSTEM + PATH_DATA + PATH_DUMMY + CONST.RunRecipe.RecipeName + PATH_DUMMY + Adjust_FILE_CALIBRATION + EXTENSION_DAT;
            //rsAlign.Init(strCvtCalPath, strAdjustCvtCalPath);
            btnSideViewer.Visible = false;
            if (CONST.PCNo == 3 || CONST.PCNo == 6)
            {
                // UVRW Setting
                //rsAlign.setUVRW((int)eCalPos.Bend1Arm_L, 135, 315, 45, 225, 58.335);
                //rsAlign.setUVRW((int)eCalPos.Bend1Arm_R, 135, 315, 45, 225, 58.335);
                //rsAlign.setUVRW((int)eCalPos.Bend2Arm_L, 135, 315, 45, 225, 58.335);
                //rsAlign.setUVRW((int)eCalPos.Bend2Arm_R, 135, 315, 45, 225, 58.335);

                //이게 아니면 x1,x2각도를 서로 바꿔보자.
                //rsAlign.setUVRW((int)eCalPos.Bend1_1Arm, 45, 225, 315, 135, 103.25739);
                //rsAlign.setUVRW((int)eCalPos.Bend1_2Arm, 45, 225, 315, 135, 103.25739);
                //rsAlign.setUVRW((int)eCalPos.Bend2Arm_L, 45, 225, 315, 135, 115.95);
                //rsAlign.setUVRW((int)eCalPos.Bend2Arm_R, 45, 225, 315, 135, 115.95);
                //rsAlign.setUVRW((int)eCalPos.Bend3Arm_L, 45, 225, 315, 135, 115.95);
                //rsAlign.setUVRW((int)eCalPos.Bend3Arm_R, 45, 225, 315, 135, 115.95);
                //2번 잘됬던거
                //rsAlign.setUVRW((int)eCalPos.Bend2Arm_L, 270, 90, 180, 0, 100);
                //rsAlign.setUVRW((int)eCalPos.Bend2Arm_R, 270, 90, 180, 0, 100);

                //rsAlign.setUVRW((int)eCalPos.Bend3Arm_L, 270, 90, 180, 0, 100);
                //rsAlign.setUVRW((int)eCalPos.Bend3Arm_R, 270, 90, 180, 0, 100);

                //rsAlign.setUVRW((int)eCalPos.Bend1Trans_L, 270, 90, 0, 180, 100);
                //rsAlign.setUVRW((int)eCalPos.Bend1Trans_R, 270, 90, 0, 180, 100);
                //rsAlign.setUVRW((int)eCalPos.Bend2Trans_L, 270, 90, 0, 180, 100);
                //rsAlign.setUVRW((int)eCalPos.Bend2Trans_R, 270, 90, 0, 180, 100);
                //rsAlign.setUVRW((int)eCalPos.Bend3Trans_L, 270, 90, 0, 180, 100);
                //rsAlign.setUVRW((int)eCalPos.Bend3Trans_R, 270, 90, 0, 180, 100);

                //단동기꺼
                //rsAlign.setUVRW((int)eCalPos.Bend3Arm_L, 135, 315, 45, 225, 116.673);
                //rsAlign.setUVRW((int)eCalPos.Bend3Arm_R, 135, 315, 45, 225, 116.673);

                btnSideViewer.Visible = true;
            }
            //2020.09.25 lkw
            //if (CONST.PCNo == 4)
            //{
            //    btnSideViewer.Visible = true;
            //    //가접 과 EMI에 대한 설계치 셋팅함. Pick 시만 Align 사용하기 때문에 2개만 선언

            //    linearConverts[(int)eConvert.TempAttach1].setLinear(180, 60, 356.5, csLinearConvert.eAlignT.Raxis);
            //    linearConverts[(int)eConvert.TempAttach2].setLinear(180, 60, 356.5, csLinearConvert.eAlignT.Raxis);
            //    linearConverts[(int)eConvert.EMIAttach1].setLinear(180, 90, 316.5, csLinearConvert.eAlignT.Raxis);
            //    linearConverts[(int)eConvert.EMIAttach2].setLinear(180, 90, 316.5, csLinearConvert.eAlignT.Raxis);
            //}
            //116.67
            //rsAlign.setUVRW((int)eCalPos.BendingStage3_L, 135, 315, 225, 45, 56.568);
            //rsAlign.setUVRW((int)eCalPos.BendingStage3_R, 135, 315, 225, 45, 56.568);
            //58.335

            // 기구 설계 치 확인 필요함.
            //rsAlign.setUVRW((int)eCalPos.Bend1Arm_L, 315, 315, 45, 45, 56.568);
            //rsAlign.setUVRW((int)eCalPos.Bend1Arm_R, 315, 315, 45, 45, 56.568);
            //rsAlign.setUVRW((int)eCalPos.Bend2Arm_L, 315, 315, 45, 45, 56.568);
            //rsAlign.setUVRW((int)eCalPos.Bend2Arm_R, 315, 315, 45, 45, 56.568);

            //20.12.17 lkw DL
            DLStart();
        }

        public void InitialCalibration()
        {
            string strCvtCalPath = PATH_BASE + PATH_SYSTEM + PATH_DATA + PATH_DUMMY + CONST.RunRecipe.RecipeName + PATH_DUMMY + FILE_CALIBRATION + EXTENSION_DAT;
            string strAdjustCvtCalPath = PATH_BASE + PATH_SYSTEM + PATH_DATA + PATH_DUMMY + CONST.RunRecipe.RecipeName + PATH_DUMMY + Adjust_FILE_CALIBRATION + EXTENSION_DAT;
            rsAlign.Init(strCvtCalPath, strAdjustCvtCalPath);

            // 기구 설계 치 확인 필요함.
            //rsAlign.setUVRW((int)eCalPos.Bend1Arm_L, 135, 315, 225, 45, 56.568);
            //rsAlign.setUVRW((int)eCalPos.Bend1Arm_R, 135, 315, 225, 45, 56.568);
            //rsAlign.setUVRW((int)eCalPos.Bend2Arm_L, 135, 315, 225, 45, 56.568);
            //rsAlign.setUVRW((int)eCalPos.Bend2Arm_R, 135, 315, 225, 45, 56.568);
            //rsAlign.setUVRW((int)eCalPos.Bend1Arm_L, 135, 315, 225, 45, 56.568);
            //rsAlign.setUVRW((int)eCalPos.Bend1Arm_R, 135, 315, 225, 45, 56.568);
        }

        private void btnLogIn_Click(object sender, EventArgs e)
        {
            frmlogin.Visible = true;
        }

        private int dispNO = -1;
        private const int dispAuto = 0;

        //bool firstCH = false;
        private void DisplayChange(string lcBtnName)
        {
            for (int i = 0; i < pnMenu.Controls.Count; i++)
            {
                if (lcBtnName.ToLower().Substring(3, lcBtnName.Length - 3) == pnMenu.Controls[i].Name.ToLower().Substring(2, pnMenu.Controls[i].Name.Length - 2))
                {
                    pnMenu.Controls[i].Visible = true;
                    dispNO = i;
                    frmRecipe.Live(false, null);
                    //ChangeEQPID();

                    //System.Threading.Thread.Sleep(100);

                    //csh 20170612
                    if (pnMenu.Controls[i].Name == "ucAutoMain")
                    {
                        //frmRecipe.Camera_Change();

                        frmAutoMain.InitialDisp();

                        lbMainCurrentRcp.Text = CONST.RunRecipe.RecipeName;

                        //frmAutoMain.IF.readTrOffset(0);
                        //frmAutoMain.IF.readTrOffset(1);
                        //frmAutoMain.IF.readTrOffset(2);

                        //화면 전환 시 반영 : Tact 위해 비전 프로세스에서는 조명 전환 기능 On시에만 조명 Reset.
                        for (int j = 0; j < CONST.CAMCnt; j++)
                        {
                            if (frmAutoMain.Vision[j].CFG.Name != "NotUse")
                            {
                                //if (CONST.eLightType.Light5V == frmAutoMain.Vision[j].CFG.LightType) frmAutoMain.LightSet(frmAutoMain.Vision[j].CFG.Light1Comport, frmAutoMain.Vision[j].CFG.Light1CH, frmAutoMain.Vision[j].CFG.RefAutoLight, j);
                                //else frmAutoMain.Light12VSet(frmAutoMain.Vision[j].CFG.Light1Comport, frmAutoMain.Vision[j].CFG.Light1CH, frmAutoMain.Vision[j].CFG.RefAutoLight, j);

                                //기본노출
                                //frmAutoMain.Vision[j].setExposure(frmAutoMain.Vision[j].CFG.Exposure[0]);
                                //190624 cjm Contrast추가, 노출값으로 밝기가 해결 안되는 곳에 사용
                                //frmAutoMain.Vision[j].setContrast(frmAutoMain.Vision[j].CFG.Contrast[0]);
                            }
                        }
                        //2019.07.20 EMI Align 추가
                        //frmAutoMain.IF.readEMIData();

                        //2020.09.25 lkw
                        // Auto 전환 시에 EMI, 가접 T 축 Teaching 값 읽어 옴.... (Teaching 변경되었을 수도 있으니...)
                        frmAutoMain.IF.readTaxisDegree(eConvert.TempAttach1);
                        frmAutoMain.IF.readTaxisDegree(eConvert.TempAttach2);
                        frmAutoMain.IF.readTaxisDegree(eConvert.EMIAttach1);
                        frmAutoMain.IF.readTaxisDegree(eConvert.EMIAttach2);

                        CONST.m_bAutoStart = true;
                    }
                    else if (pnMenu.Controls[i].Name == "ucRecipe") //KSJ 20170606 Recipe 화면 Init관련 추가 Add
                    {
                        frmRecipe.SetInitRecipeCamSelect();
                        frmRecipe.VisionPasswordDisplay();
                        frmRecipe.TracePasswordDisplay();

                        frmRecipe.Camera_Change();

                        frmRecipe.Tb_trace_Init();
                        CONST.m_bAutoStart = false;
                    }
                    else if (pnMenu.Controls[i].Name == "ucSetting")
                    {
                        frmSetting.PasswordDisplay();

                        CONST.m_bAutoStart = false;
                    }
                    //2018.11.14 khs Log 추가
                    else if (pnMenu.Controls[i].Name == "ucHistory")
                    {
                        frmHistory.HistoryInitial();

                        CONST.m_bAutoStart = false;
                    }
                }
                else pnMenu.Controls[i].Visible = false;
            }
        }

        private TimeSpan checkLogin = new TimeSpan();
        public static DateTime loginCheckStart = new DateTime();

        private void btnMenu_Click(object sender, EventArgs e)
        {
            string lcText = (sender as Button).Text;
            //   CONST.LOG.Save(csLog.LogKind.System, lcText + " BUTTON Click");

            for (short i = 0; i < btnMenu.Length; i++)
            {
                if (lcText == btnMenu[i].Text)
                {
                    DisplayChange(btnMenu[i].Name);
                    //btnMenu[i].BackColor = Color.Gray;
                    btnMenu[i].ForeColor = Color.Yellow;
                }
                else
                {
                    //btnMenu[i].BackColor = Color.White;
                    btnMenu[i].ForeColor = Color.White;
                }
            }
            loginCheckStart = DateTime.Now;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Program Close?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                frmAutoMain.IF.SendData("close," + "1,1");
                frmAutoMain.IF.KillMX();
                frmAutoMain.DL.KillDL();
                frmRecipe.KillBendingSideViewer();

                Close();

                if (CONST.m_bSystemLog)
                    cLog.Save(LogKind.System, "Program Close");
                if (CONST.m_bInterfaceLog)
                    cLog.Save(LogKind.Interface, "Program Exit");
            }
        }

        //bool FirstAutoMode = false;

        private void lbSelectUser_FormClosed(object sender, FormClosedEventArgs e)
        {
            frmAutoMain.camDisconnect();
        }

        private string old_rcpName = "";

        private void tmrDisplay_Tick(object sender, EventArgs e)
        {
            if (old_rcpName != CONST.RunRecipe.RecipeName)
            {
                old_rcpName = CONST.RunRecipe.RecipeName;

                //pcy200708 위치이동
                Bending.Menu.frmRecipe.RecipChangeDataCopy();

                frmSetting.revData.ReadData(CONST.RunRecipe.RecipeName);

                frmSetting.REVTabUpdate();
                //Calibration Init
                InitialCalibration();

                checkLogin = DateTime.Now - loginCheckStart;
                if (checkLogin.TotalMinutes > 30)
                {
                    loginCheckStart = DateTime.Now;
                    CONST.LoginID = "Operator";
                }

                frmRecipe.frmMsg.Visible = false;


            }

            if (frmAutoMain.ParamChange)
            {
                frmAutoMain.ParamChange = false;
                frmAutoMain.ResultDisplayInit();
                //필요한 UI만 보이게함

                frmAutoMain.ResultSpecDisplay(0, Vision_No.Laser1);
                frmAutoMain.ResultSpecDisplay(1, Vision_No.Laser2);

                //여기
                //if (CONST.PCNo == 4)
                //{
                //    Bending.Menu.frmAutoMain.HeightSpecRead(1);
                //}
                //if (CONST.PCNo == 1) //20200918 cjm 
                //{
                //    Bending.Menu.frmAutoMain.BendingPreSCFInspection();
                //}

                // 20200926 cjm revData -> textBox 쓰기
                //frmRecipe.revDataTotxt();

                //Setting Value UpData
                for (int i = 0; i < CONST.CAMCnt; i++)
                {
                    ucSetting.cCFG.CAMconfig_Read(i, ref frmAutoMain.Vision[i].CFG);
                }
                frmSetting.revData.ReadData(CONST.RunRecipe.RecipeName);
                frmSetting.SettingValueReSet();

                frmAutoMain.InitialDispChart();
                frmAutoMain.drawChartNoDist();
            }

            string lcDW = DateTime.Now.DayOfWeek.ToString();
            lbClock.Text = DateTime.Now.ToString("yyyy.MM.dd") + "  " + DateTime.Now.ToString("HH.mm.ss");
            lbClock.Font = new Font("Arial", 10, FontStyle.Bold);

            lbLogINUser.Text = CONST.LoginID;
            //lbMainCurrentRcp.Text = ucSetting.DB.getRunRecipeName().Trim();
            //Recipe Disp 추가
            lbMainCurrentRcp.Text = CONST.RunRecipe.RecipeName;
            //123qwe
            //Auto Manual 정보 표시 추가
            if (CONST.plcAutomode)
            {
                lblAuto.Text = "AUTO";
                lblAuto.BackColor = Color.White;
                lblAuto.ForeColor = Color.Black;

                //일단 주석
                //if (FirstAutoMode)
                //{
                //    btnMenu_Click(btnAutoMain, null);

                //    btnMenu[1].Enabled = false;
                //    btnMenu[2].Enabled = false;
                //    btnMenu[3].Enabled = false;

                //    FirstAutoMode = false;
                //}
            }
            else
            {
                lblAuto.Text = "STOP";
                lblAuto.BackColor = Color.Red;
                lblAuto.ForeColor = Color.White;

                //일단 주석
                //if (!FirstAutoMode)
                //{
                //    btnMenu[1].Enabled = true;
                //    btnMenu[2].Enabled = true;
                //    btnMenu[3].Enabled = true;

                //    FirstAutoMode = true;
                //}
            }

            //csh 20170601
            if (CONST.m_bPLCConnect)
            {
                label_PLC.Text = "Connect";
                label_PLC.BackColor = Color.White;
                label_PLC.ForeColor = Color.Black;
            }
            else
            {
                label_PLC.Text = "Disconnect";
                label_PLC.BackColor = Color.Red;
                label_PLC.ForeColor = Color.White;
            }

            //2018.07.10 dispNo == 1 -> dispNo == 0 수정 khs
            if (dispNO == 0 && CONST.RunRecipe.RecipeName != "")
                CONST.m_bAutoStart = true;
            else
            {
                CONST.m_bAutoStart = false;
            }

            //20.12.17 lkw DL
            if (rsDL.IsReady)
            {
                pnDL.BackColor = Color.Green;
            }
            else
            {
                pnDL.BackColor = Color.Orange;
            }
        }

        private void Menu_FormClosing(object sender, FormClosingEventArgs e)
        {
            frmRecipe.Live(false);
            Bending.Menu.frmAutoMain.IF.SendData("HEIGHTSTOP");
            frmAutoMain.ThreadDispose();
            //frmRecipe.CloseBrowser();
        }

        public void GetPCStatus()
        {
            cpuCounter = new PerformanceCounter();
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";
            try
            {

                ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            //}
            //catch
            //{
            //}

            //try
            //{
                //pcy190402 타이머변경
                System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
                t.Interval = 3000;
                t.Enabled = true;
                t.Tick += new EventHandler(TimerElapsed);
                //System.Timers.Timer t = new System.Timers.Timer(5000);
                //t.Elapsed += new ElapsedEventHandler(TimerElapsed);
                //t.Start();
                //System.Threading.Thread.Sleep(1000);
            }
            catch
            {
            }
        }

        //KSJ 20170606 Cpu,Ram,Hdd 표기 Add
        //public void TimerElapsed(object source, ElapsedEventArgs e)
        public void TimerElapsed(object source, EventArgs e)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            double Dc = 0;
            double Cc = 0;
            foreach (DriveInfo d in allDrives)
            {
                double Da = (Convert.ToDouble(d.TotalSize) - Convert.ToDouble(d.AvailableFreeSpace));
                double Db = Convert.ToDouble(d.TotalSize);
                double Calc = Da / Db * 100;

                if (d.Name.Equals(@"D:\"))
                {
                    Dc = Calc;
                    CONST.DdriveSpace = Dc;
                }
                else if (d.Name.Equals(@"C:\"))
                {
                    Cc = Calc;
                }
            }

            float cpu = cpuCounter.NextValue();
            float ram = ramCounter.NextValue();

            ComputerInfo ComInfo = new ComputerInfo();
            long TotalMemory = long.Parse(ComInfo.TotalPhysicalMemory.ToString());
            double RamUsage;
            RamUsage = ((Convert.ToDouble(TotalMemory) - Convert.ToDouble(ram * 1048576)) / Convert.ToDouble(TotalMemory)) * 100;

            //IF.progressBar1.Value = (i + 1) * 10;
            //IF.lbProgress.Text = Convert.ToString(IF.progressBar1.Value) + "%";

            pgbCpu.Value = Convert.ToInt32(cpu);

            prograssbarcolor.SetState(pgbCpu, 1);

            if (cpu > 80)
            {
                prograssbarcolor.SetState(pgbCpu, 2);
            }
            lbCpuP.Text = cpu.ToString("0.00") + " %";

            pgbRam.Value = Convert.ToInt32(RamUsage);

            prograssbarcolor.SetState(pgbRam, 1);
            if (RamUsage > 80)
            {
                prograssbarcolor.SetState(pgbRam, 2);
            }
            lbRamP.Text = RamUsage.ToString("0.00") + " %";

            pgbHdd.Value = Convert.ToInt32(Dc);

            prograssbarcolor.SetState(pgbHdd, 1);

            if (Dc > 80)
            {
                prograssbarcolor.SetState(pgbHdd, 2);
            }
            lbHDDDP.Text = Dc.ToString("0.00") + " %";

            pgbHdc.Value = Convert.ToInt32(Cc);

            prograssbarcolor.SetState(pgbHdc, 1);

            if (Cc > 80)
            {
                prograssbarcolor.SetState(pgbHdc, 2);
            }
            lbHddCP.Text = Cc.ToString("0.00") + " %";
            //pcy190402 타이머 변경으로 인보크부분 밖으로 뺌.
            #region
            //if (pgbCpu.InvokeRequired)
            //{
            //    try
            //    {
            //        pgbCpu.Invoke(new MethodInvoker(delegate
            //        {
            //            pgbCpu.Value = Convert.ToInt32(cpu);

            //            prograssbarcolor.SetState(pgbCpu, 1);

            //            if (cpu > 80)
            //            {
            //                prograssbarcolor.SetState(pgbCpu, 2);
            //            }
            //            lbCpuP.Text = cpu.ToString("0.00") + " %";
            //        }));

            //    }
            //    catch { };
            //}
            //if (pgbRam.InvokeRequired)
            //{
            //    try
            //    {
            //        pgbRam.Invoke(new MethodInvoker(delegate
            //        {
            //            pgbRam.Value = Convert.ToInt32(RamUsage);

            //            prograssbarcolor.SetState(pgbRam, 1);
            //            if (RamUsage > 80)
            //            {
            //                prograssbarcolor.SetState(pgbRam, 2);
            //            }
            //            lbRamP.Text = RamUsage.ToString("0.00") + " %";
            //        }));

            //    }
            //    catch { };
            //}
            //if (pgbHdd.InvokeRequired)
            //{
            //    try
            //    {
            //        pgbHdd.Invoke(new MethodInvoker(delegate
            //        {
            //            pgbHdd.Value = Convert.ToInt32(Dc);

            //            prograssbarcolor.SetState(pgbHdd, 1);

            //            if (Dc > 80)
            //            {
            //                prograssbarcolor.SetState(pgbHdd, 2);
            //            }
            //            lbHDDDP.Text = Dc.ToString("0.00") + " %";
            //        }));

            //    }
            //    catch { };
            //}
            //if (pgbHdc.InvokeRequired)
            //{
            //    try
            //    {
            //        pgbHdc.Invoke(new MethodInvoker(delegate
            //        {
            //            pgbHdc.Value = Convert.ToInt32(Cc);

            //            prograssbarcolor.SetState(pgbHdc, 1);

            //            if (Cc > 80)
            //            {
            //                prograssbarcolor.SetState(pgbHdc, 2);
            //            }
            //            lbHddCP.Text = Cc.ToString("0.00") + " %";
            //        }));

            //    }
            //    catch { };
            //}
            #endregion
        }

        private void lbLogINUser_Click(object sender, EventArgs e)
        {
            //CONST.LoginID = "Maker";
        }

        private void btnSideViewer_Click(object sender, EventArgs e)
        {
            frmRecipe.SideViewer_Start(false);
        }

        //20.12.17 lkw DL
        public static void DLStart()
        {
            //공용으로 사용하려 하니..... Cam당 2개씩 연결 할 경우 최소 16개의 연결이 필요함.
            //사용하는 부분만 정해서 고정으로 써야 할 듯....... 우선은 16개 연결 가능하도록 Python 변경~

            for (int i = 0; i < frmAutoMain.Vision.Length; i++)
            {
                if (frmAutoMain.Vision[i].CFG.Name == "NotUse") continue;
                for (int j = 0; j < frmSetting.revData.mDL[i].MarkSearch_Use.Length; j++)
                {
                    frmSetting.revData.mDL[i].MarkSearch_Use[j] = false;
                    if (frmSetting.revData.mDL[i].MarkSearch_LabelPath[j].Length > 0 && frmSetting.revData.mDL[i].MarkSearch_ModelPath[j].Length > 0)
                    {
                        if (File.Exists(frmSetting.revData.mDL[i].MarkSearch_LabelPath[j]) && File.Exists(frmSetting.revData.mDL[i].MarkSearch_ModelPath[j]))
                        {
                            //정상 셋팅되어 있는 부분 만 사용으로 변경함. (File 있고 경로 설정되어 있는 경우)
                            frmSetting.revData.mDL[i].MarkSearch_Use[j] = true;
                            rsDL.setParamPath(i * frmSetting.revData.mDL[i].MarkSearch_Use.Length + j, csDL_IF.Dlkind.MarkSearch, frmSetting.revData.mDL[i].MarkSearch_LabelPath[j], frmSetting.revData.mDL[i].MarkSearch_ModelPath[j]);
                        }
                    }
                }

                for (int j = 0; j < frmSetting.revData.mDL[i].DefectFind_Use.Length; j++)
                {
                    frmSetting.revData.mDL[i].DefectFind_Use[j] = false;
                    if (frmSetting.revData.mDL[i].DefectFind_LabelPath[j].Length > 0 && frmSetting.revData.mDL[i].DefectFind_ModelPath[j].Length > 0)
                    {
                        if (File.Exists(frmSetting.revData.mDL[i].DefectFind_LabelPath[j]) && File.Exists(frmSetting.revData.mDL[i].DefectFind_ModelPath[j]))
                        {
                            frmSetting.revData.mDL[i].DefectFind_Use[j] = true;
                            rsDL.setParamPath(i * frmSetting.revData.mDL[i].DefectFind_Use.Length + j, csDL_IF.Dlkind.defectdecision, frmSetting.revData.mDL[i].DefectFind_LabelPath[j], frmSetting.revData.mDL[i].DefectFind_ModelPath[j]);
                        }
                    }
                }
            }

            //rsDL.py 실행 폴더 설정
            string dlFolder = "C://EQData/DL/Python";
            rsDL.setDlfolder(dlFolder);

            //Ready
            rsDL.DL_Ready();  //ready 완료 되면 isReady True....

        }

        private void cbSimulation_CheckedChanged(object sender, EventArgs e)
        {
            CONST.simulation = cbSimulation.Checked;
            
            
        }
    }
}