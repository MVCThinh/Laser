using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;
using rs2DAlign;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Cognex.VisionPro.PMAlign;

namespace Bending
{
    public partial class ucAutoMain : UserControl
    {
        #region 1
        public string LastSaveTime;

        private csLog cLog = new csLog();
        public csVision[] Vision; //Vision no에 관한 그룹핑이 필요함.. 또는 연결순서랑 화면표시를 따로가져가든가.. 각각 장단점 생각해보기
        public frmManualMark[] manualMark; //이 form도 csvision만큼 생성
        private ICogFrameGrabber[] cogFramGrabber;
        private CogFrameGrabbers cogFramGrabbers;
        private Cognex.VisionPro.Display.CogDisplay[] cogDS;
        private Label[] lbTitle;
        private Panel[] pnView;
        private Panel[] pnInfo;

        //KSJ 20170620
        private CogGraphicLabel cogLabel = new CogGraphicLabel();

        private Font font6Bold = new Font("Arial", 6, FontStyle.Bold);
        private Font font15Bold = new Font("Arial", 15, FontStyle.Bold);

        private Label[] lbSearchResult;
        private Label[] lbScore;

        public CheckBox[] cbLive;
        public ComboBox[] cbSideImageSize;

        //csh 20170601
        private Button[] btnPLC1;

        private Button[] btnPLC10;
        private Button[] btnPC1;
        private Button[] btnImage;

        //csh 20170601
        private Label[] lblPPID;

        private Label[] lblCycleTime;
        private Label[] lblRetryCount;
        private GroupBox[] gbBendResult;
        private Label[] lbloffset;

        private Label[] lblct1Spec;
        private Label[] lblct2Spec;
        private Label[] lblct3Spec;
        private Label[] lblct4Spec;
        private Label[] lblct1_1Spec;

        private Label[] lbResultTitle;

        //세로선 찾는 변수
        private CogFindLineTool HeightLine1 = new CogFindLineTool();
        private CogFindLineTool HeightLine2 = new CogFindLineTool();

        private const short MaxMotionCnt = 150;
        public bool ParamChange = true;
        //private bool[] firstLog = new bool[3];
        private bool[] bFirstInNoRetry = new bool[3];

        public frmIF IF;
        public frmDL DL;

        public bool[] bPCRep = new bool[32];        // PC Reply Bit (PC -> PLC)
        //public bool[] bPCRep1 = new bool[32];        // PC Reply Bit (PC -> PLC)
        public int[] pcResult = new int[64]; // lyw 수정.
        private int[] pcOldResult = new int[64]; // lyw수정.
        public bool[] visionBackground = new bool[64]; // lyw. //한번만 실행하기 위함

        public Stopwatch[] sTime = new Stopwatch[32];

        //20.12.17 lkw DL 
        public bool[] bDLMarkFind = new bool[CONST.CAMCnt];
        public bool[] bDLInspFind = new bool[CONST.CAMCnt];

        public bool[] bLCheckError = new bool[CONST.CAMCnt];

        public bool[] bManualMarkInitComp = new bool[CONST.CAMCnt];

        private Thread mainThread = null;
        private Thread autoMainThread = null;

        private volatile bool threadAutoEnd = false;
        private volatile bool threadEnd = false; // 쓰레드용.

        //20161102 ljh
        private bool[] bCaptureFail = { false, false };

        private int[] Comport = new int[3];

        private ListBox[] lbList;
        private DataGridView[] dgvAlign;
        private TabControl[] TCList;
        private DataGridView[] dgvError;

        //lkw 20170828 Manual Bending 관련 변수 추가 (Manual Bending 상태인지 확인)
        //public bool[] bManualBending = new bool[8]; //사용여부
        public bool[] bManualBendingPopup = new bool[8]; //팝업여부

        private frmManualWindow[] frmmanualWindow = new frmManualWindow[8];

        //2018.07.15 BendinMode 변수 추가
        public bool BDModeEdge = false;

        private const short dispX = 0;
        private const short dispY = 1;
        private const short dispT = 2;
        private const short dispX1 = 0;
        private const short dispX2 = 1;
        private const short dispY1 = 2;
        private const short dispY2 = 3;

        private System.Windows.Forms.Timer DispTimer = new System.Windows.Forms.Timer();
        // timeout 용.
        #endregion
        public class timeCheck
        {
            public bool on;
            public short reqID;
            public DateTime time;

            public timeCheck(bool on, short reqID, DateTime time)
            {
                this.on = on;
                this.reqID = reqID;
                this.time = time;
            }
        }

        #region 2
        private timeCheck[] lastRunTime = null;
        public FieldInfo[] bitControls = null;
        public static int bitControlsCount = 0;

        public const int mPanel = 0;
        public const int mFPC = 1;
        public const int mLeft = 2;
        public const int mRight = 3;
        private int manualPopupCnt = 5; //메뉴얼마크 팝업창 실행횟수

        private bool[] MarkFail = new bool[32];

        //2017.09.27
        public bool BendingPreSCFInspCheck;

        public bool BendingPreSCFInspCheckPopup;

        #endregion
        public struct VisionPos
        {
            public double XPos;
            public double YPos;
            public double TPos;
        }

        public VisionPos[] visionPosition = new VisionPos[20];  //Vision 촬상 위치 저장
        private frmInspShift frminspshift = new frmInspShift();
        public static csConfig _cCFG = new csConfig();

        public static csConfig cCFG
        {
            get
            {
                lock (_cCFG)
                {
                    return _cCFG;
                }
            }
            set
            {
                lock (_cCFG)
                {
                    _cCFG = value;
                }
            }
        }
        public int[] CELLID; //이건 주소
        //2018.07.11 Lcheck Dist 추가 //LCheck길이 임시저장소
        double[] LDist = new double[10];
        public ucAutoMain()
        {
            InitializeComponent();

            #region 3
            lbList = new ListBox[] { listBox1, listBox2, listBox3, listBox4, listBox5 };  //20201005 cjm Height Insp Log창 추가
            dgvAlign = new DataGridView[] { dgvAlign1, dgvAlign2, dgvAlign3, dgvAlign4, dgvAlign5 };
            TCList = new TabControl[] { tabControl1, tabControl2, tabControl3, tabControl4, tabControl5 };
            dgvError = new DataGridView[] { dgvError1, dgvError2, dgvError3, dgvError4, dgvError5 };
            cogDS = new Cognex.VisionPro.Display.CogDisplay[] { cogDS1, cogDS2, cogDS3, cogDS4, cogDS5, cogDS6, cogDS7, cogDS8 };
            lbTitle = new Label[] { lb1, lb2, lb3, lb4, lb5, lb6, lb7, lb8 };
            pnView = new Panel[] { pn1, pn2, pn3, pn4, pn5, pn6, pn7, pn8 };
            pnInfo = new Panel[] { pnInfo1, pnInfo2, pnInfo3, pnInfo4, pnInfo5 };
            lbSearchResult = new Label[] { lblSearchResult1, lblSearchResult2, lblSearchResult3, lblSearchResult4, lblSearchResult5, lblSearchResult6, lblSearchResult7, lblSearchResult8 };
            lbScore = new Label[] { lblScore1, lblScore2, lblScore3, lblScore4, lblScore5, lblScore6, lblScore7, lblScore8 };
            cbLive = new CheckBox[] { cbLive1, cbLive2, cbLive3, cbLive4, cbLive5, cbLive6, cbLive7, cbLive8 };
            cbSideImageSize = new ComboBox[] { cbSideImageSize1, cbSideImageSize2, cbSideImageSize3, cbSideImageSize4, cbSideImageSize5, cbSideImageSize6, cbSideImageSize7, cbSideImageSize8 };
            btnPLC1 = new Button[] { button_PLC1, button_PLC2, button_PLC3, button_PLC4, button_PLC5, button_PLC6, button_PLC7, button_PLC8 };
            btnPLC10 = new Button[] { button_PLC11, button_PLC12, button_PLC13, button_PLC14, button_PLC15, button_PLC16, button_PLC17, button_PLC18 };
            btnPC1 = new Button[] { button_PC1, button_PC2, button_PC3, button_PC4, button_PC5, button_PC6, button_PC7, button_PC8 };
            lblPPID = new Label[] { lblPPID1, lblPPID2, lblPPID3, lblPPID4, lblPPID5 };
            lblCycleTime = new Label[] { lblCycleTime1, lblCycleTime2, lblCycleTime3, lblCycleTime4, lblCycleTime5 };
            lblRetryCount = new Label[] { lblRetryCount1, lblRetryCount2, lblRetryCount3, lblRetryCount4, lblRetryCount5 };
            gbBendResult = new GroupBox[] { gbBendResult1, gbBendResult2, gbBendResult3, gbBendResult4 };
            lbloffset = new Label[] { lblOFFSET0, lblOFFSET1, lblOFFSET2, lblOFFSET3 };
            ResultDist = new double[][] { ResultDist1, ResultDist2, ResultDist3, ResultDist4, ResultDist5, ResultDist6, ResultDist7, ResultDist8 };
            lbResultTitle = new Label[] { lbResult1Title, lbResult2Title, lbResult3Title, lbResult4Title };
            ResultDistInsp = new double[][] { ResultDistX1, ResultDistY1, ResultDistX2, ResultDistY2 };

            #endregion
            btnImage = new Button[] { btn1, btn2, btn3, btn4 };  //Simulator 용

            for (int i = 0; i < frmmanualWindow.Length; i++)
            {
                frmmanualWindow[i] = new frmManualWindow();
            }

            for (int i = 0; i < cbLive.Length; i++)
            {
                cbLive[i].CheckedChanged += new System.EventHandler(cbLive_CheckedChanged);
            }

            for (int i = 0; i < btnImage.Length; i++)
            {
                btnImage[i].Click += new EventHandler(btnImage_Click);
            }


            lblct1Spec = new Label[] { ct1_Xspec1_1, ct1_Yspec1_1, ct1_Xspec1_2, ct1_Yspec1_2 };
            lblct2Spec = new Label[] { ct2_XSpec2_1, ct2_YSpec2_1, ct2_XSpec2_2, ct2_YSpec2_2 };
            lblct3Spec = new Label[] { ct3_XSpec3_1, ct3_YSpec3_1, ct3_XSpec3_2, ct3_YSpec3_2 };
            lblct4Spec = new Label[] { ct4_Xspec4_1, ct4_Yspec4_1, ct4_Xspec4_2, ct4_Yspec4_2 };
            lblct1_1Spec = new Label[] { ct1_Xspec1_3, ct1_Yspec1_3, ct1_Xspec1_4, ct1_Yspec1_4 };

            if (!cCFG.Initial())
            {
                cLog.Save(LogKind.System, "Confirm Config.INI File");
                MessageBox.Show("Confirm Config.INI File");
            }
            CELLID = new int[] { CONST.Address.PLC.CELLID1, CONST.Address.PLC.CELLID2, CONST.Address.PLC.CELLID3, CONST.Address.PLC.CELLID4, CONST.Address.PLC.CELLID5, CONST.Address.PLC.CELLID6, CONST.Address.PLC.CELLID7, CONST.Address.PLC.CELLID8 };

            IF = new frmIF();
            DL = new frmDL();

            Vision = new csVision[CONST.CAMCnt];
            manualMark = new frmManualMark[CONST.CAMCnt];
            short realCnt = 0;
            Comport[0] = -1;
            Comport[1] = -1;
            Comport[2] = -1;

            bool Cam1Use = false;
            bool Cam2Use = false;

            for (int i = 0; i < Vision.Length; i++)
            {
                Vision[i] = new csVision();

                ucSetting.cCFG.CAMconfig_Read(i, ref Vision[i].CFG); 
                SetVisionAddress(ref Vision[i]);

                manualMark[i] = new frmManualMark(Vision[i].CFG, i);

                lbTitle[i].Text = Vision[i].CFG.Name;

                int Num = Convert.ToInt32(Math.Truncate(Convert.ToDouble(i) / 2));

                if (Vision[i].CFG.Use && Vision[i].CFG.Name != "Not Use")
                {
                    pnView[i].Visible = true;
                    pnInfo[i].Visible = true;
                    realCnt++;
                    if (Comport[0] == -1) Comport[0] = Vision[i].CFG.Light1Comport;
                    else if (Vision[i].CFG.Light1Comport != Comport[0] && Comport[1] == -1) Comport[1] = Vision[i].CFG.Light1Comport;
                    else if (Vision[i].CFG.Light1Comport != Comport[0] && Vision[i].CFG.Light1Comport != Comport[1]) Comport[2] = Vision[i].CFG.Light1Comport;

                    if (i == 0) Cam1Use = true;
                    else if (i % 2 == 0) Cam1Use = true;
                    else if (i % 2 == 1) Cam2Use = true;
                }
                else
                {
                    if (i == 0) Cam1Use = false;
                    else if (i % 2 == 0) Cam1Use = false;
                    else if (i % 2 == 1) Cam2Use = false;
                }

                if (i != 0 && i % 2 == 1)
                {
                    if (!Cam1Use && Cam2Use)
                    {
                        pnView[i - 1].Visible = false;
                        CamInfoSetting(Cam1Use, Cam2Use, Num);
                    }
                    else if (!Cam2Use && Cam1Use)
                    {
                        pnView[i].Visible = false;
                        CamInfoSetting(Cam1Use, Cam2Use, Num);
                    }
                    else if (!Cam1Use && !Cam2Use)
                    {
                        pnView[i - 1].Visible = false;
                        pnView[i].Visible = false;
                    }
                }
            }
            SetPanelIDAddress(ref Vision, CONST.PCNo);
            pnAPC.Visible = false;
            pnHeightDisp.Visible = false;
            if (Comport[0] != -1) spLight1.PortName = "COM" + Comport[0].ToString();
            if (Comport[1] != -1) spLight2.PortName = "COM" + Comport[1].ToString();
            if (Comport[2] != -1) spLight3.PortName = "COM" + Comport[2].ToString();

            try
            {
                if (Comport[0] != -1) spLight1.Open();
                if (Comport[1] != -1) spLight2.Open();
                if (Comport[2] != -1) spLight3.Open();
            }
            catch { }

            cogFramGrabber = new Cognex.VisionPro.ICogFrameGrabber[realCnt];
            cogFramGrabbers = new Cognex.VisionPro.CogFrameGrabbers();

            int camidx = 0;

            //KSJ 20170625 Serial Num Error;
            bool bCamSerialCheck = false;

            try
            {
                for (int i = 0; i < realCnt; i++)
                {
                    bCamSerialCheck = false;
                    cogFramGrabber[i] = cogFramGrabbers[i];
                    for (camidx = 0; camidx < CONST.CAMCnt; camidx++)
                    {
                        if (cogFramGrabber[i].SerialNumber == Vision[camidx].CFG.Serial)
                        {
                            bCamSerialCheck = true;
                            if (Vision[camidx].Initial(cogFramGrabber[i], cogDS[camidx])) break;
                            else
                            {
                                cLog.Save(LogKind.System, "CAM Setting Fail!, CamNo = " + camidx.ToString());
                                MessageBox.Show("CAM Setting Fail!, CamNo = " + camidx.ToString());
                            }
                        }
                    }

                    //KSJ 20170625 Serial Num Error;
                    if (bCamSerialCheck == false)
                    {
                        MessageBox.Show("DB Camera SerialNum Not Exist = " + cogFramGrabber[i].SerialNumber);
                    }

                    IF.progressBar1.Value = (i + 1) * 10;
                    IF.lbProgress.Text = Convert.ToString(IF.progressBar1.Value) + "%";
                    IF.lbProgress.Refresh();
                }
            }
            catch (Exception EX)
            {
                cLog.Save(LogKind.System, "CAM Setting Fail! - " + EX.Message);
                MessageBox.Show("CAM Setting Fail! - " + EX.Message);
            }


            cCFG.RecipeName_Read();    

            int ret = IF.Connection(false);
            if (ret != 0) ret = IF.Connection(false);
            if (ret != 0)
            {
                cLog.Save(LogKind.System, "Confirm PLC Connection (" + ret.ToString() + ")");
                MessageBox.Show("Confirm PLC Connection (" + ret.ToString() + ")");
            }
            else
            {
                IF.SendData("RCPID," + CONST.PLCDeviceType + CONST.Address.PLC.RECIPEID.ToString());

                IF.SendData("RCPPARAM," + CONST.PLCDeviceType + CONST.Address.PLC.RECIPEPARAM.ToString() + "," + Enum.GetValues(typeof(eRecipe)).Length);

                for (int i = 0; i < Vision.Length; i++)
                {
                    cbLive[i].Visible = false;
                    cbSideImageSize[i].Visible = false;

                    if (Vision[i].CFG.Use && Vision[i].CFG.Name != "NotUse")
                    {
                        if (Vision[i].CFG.SideVision)
                        {
                            cbSideImageSize[i].Visible = true;
                        }
                    }
                }
                this.mainThread = new Thread(new ThreadStart(Idle));
                mainThread.Start();

                this.autoMainThread = new Thread(new ThreadStart(AutoMainProcess));
                autoMainThread.Start();

                IF.setReqStart();
            }


            if (CONST.RunRecipe.RecipeName == "" || CONST.RunRecipe.RecipeName == null)
            {
                CONST.RunRecipe.RecipeName = CONST.RunRecipe.OldRecipeName;
            }

            bPCRep[pcAutoManual] = true;  // 초기 Bit Clear위해
            DispTimer.Tick += new EventHandler(DispTimer_Unit_Tick);
            this.lastRunTime = new timeCheck[64];

            InitOKNG1();

            if (CONST.m_bInterfaceLog)
                cLog.Save(LogKind.Interface, "Program Start");

            IF.Visible = false;
            DL.Visible = false;

            lbResult1Title.Text = CONST.RESULT_TITLE1;
            lbResult2Title.Text = CONST.RESULT_TITLE2;
            lbResult3Title.Text = CONST.RESULT_TITLE3;
            lbResult4Title.Text = CONST.RESULT_TITLE4;

            tabPage1.Text = CONST.RESULT_TITLE1;
            tabPage2.Text = CONST.RESULT_TITLE2;
            tabPage3.Text = CONST.RESULT_TITLE3;
            tabPage4.Text = CONST.RESULT_TITLE4;

            tcGraph.TabPages.RemoveAt(3);
            tcGraph.TabPages.RemoveAt(2);
            //tcGraph.TabPages.RemoveAt(1);

            DispTimer.Interval = 200; //기존 D_PC1,2는 실시간이었음.
            DispTimer.Enabled = true;

            for (int i = 0; i < sTime.Length; i++) sTime[i] = new Stopwatch();
        }
        private int SetPanelIDAddress(ref csVision[] vision, int pcno)
        {
            switch (pcno)
            {
                case 1:
                    vision[(int)eCamNO1.LoadingPre1].PanelIDAddress = CONST.Address.PLC.CELLID1;
                    vision[(int)eCamNO1.LoadingPre2].PanelIDAddress = CONST.Address.PLC.CELLID2;
                    vision[(int)eCamNO1.Laser1].PanelIDAddress = CONST.Address.PLC.CELLID3;
                    vision[(int)eCamNO1.Laser2].PanelIDAddress = CONST.Address.PLC.CELLID4;
                    break;
            }
            return pcno;
        }
        private bool SetVisionAddress(ref csVision vision)
        {
            switch (vision.CFG.eCamName)
            {
                case nameof(eCamNO.LoadingPre1):
                    vision.ReportAddress.MatchingScore = Address.MatchingScore.LoadingPre1;
                    break;
                case nameof(eCamNO.LoadingPre2):
                    vision.ReportAddress.MatchingScore = Address.MatchingScore.LoadingPre2;
                    break;
                case nameof(eCamNO.Laser1):
                    vision.ReportAddress.MatchingScore = Address.MatchingScore.Laser1Align;
                    //vision.ReportAddress.Insp = Address.DV.LaserInsp;
                    break;
                case nameof(eCamNO.Laser2):
                    vision.ReportAddress.MatchingScore = Address.MatchingScore.Laser2Align;
                    //vision.ReportAddress.Insp = Address.DV.LaserInsp;
                    break;
            }
            return true;
        }
        private bool SCFInspectionSpec_Write(DataGridView dgvSpec)
        {
            bool bResult = true;

            try
            {
                double Tolerance = Menu.frmSetting.revData.mBendingPre.SCFTolerance;
                if (dgvSpec1.Columns.Count == 0)
                {
                    dgvSpec1.Columns.Clear();
                    dgvSpec1.Columns.Add("Column1", "X1");
                    dgvSpec1.Columns.Add("Column2", "Y1");
                    dgvSpec1.Columns.Add("Column3", "X2");
                    dgvSpec1.Columns.Add("Column4", "Y2");
                    dgvSpec1.Columns.Add("Column5", "X3");
                    dgvSpec1.Columns.Add("Column6", "Y3");
                }

                lbloffset[0].Text = "CPK";
                if (dgvSpec.Rows.Count == 0)
                {
                    dgvSpec.Rows.Clear();
                    dgvSpec.Rows.Add(4);
                }
            }
            catch
            {
                bResult = false;
                return bResult;
            }

            return bResult;
        }
        private void DispTimer_Unit_Tick(object sender, EventArgs e)
        {
            PLCStatusPC1();
            PCStatusPC1();

        }

        public void ResultDisplayInit()
        {
            gbBendResult1.Visible = false;
            gbBendResult2.Visible = false;
            gbBendResult3.Visible = false;
            gbBendResult4.Visible = false;
        }

        public void ResultSpecDisplay(int DispCnt, int CamNo)
        {
            try
            {
                DataGridView dgvSpec = null;

                string strTemp = "";
                rs2DAlign.cs2DAlign.ptXXYY APCOffset = new cs2DAlign.ptXXYY();

                if (DispCnt == 0)
                {
                    gbBendResult1.Visible = true;
                    strTemp = CONST.RESULT1_TYPE;
                    dgvSpec = dgvSpec1;

                }
                else if (DispCnt == 1)
                {
                    gbBendResult2.Visible = true;
                    strTemp = CONST.RESULT2_TYPE;
                    dgvSpec = dgvSpec2;

                }
                else if (DispCnt == 2)
                {
                    gbBendResult3.Visible = true;
                    strTemp = CONST.RESULT3_TYPE;
                    dgvSpec = dgvSpec3;

                }
                else if (DispCnt == 3)
                {
                    gbBendResult4.Visible = true;
                    strTemp = CONST.RESULT4_TYPE;
                    dgvSpec = dgvSpec4;
                }
                dgvSpec.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                if (strTemp.Contains("BD1"))
                {
                    if (strTemp.Contains("1"))
                    {
                        APCOffset.Y1 = CONST.RunRecipe.Param[eRecipe.BENDING_OFFSET_LY];
                        APCOffset.Y2 = CONST.RunRecipe.Param[eRecipe.BENDING_OFFSET_RY];
                    }
                    double ToleranceX = Menu.frmSetting.revData.mBendingArm.BDToleranceX;
                    double ToleranceY = Menu.frmSetting.revData.mBendingArm.BDToleranceY;
                    if (dgvSpec.Columns.Count == 0)
                    {
                        dgvSpec.Columns.Clear();
                        dgvSpec.Columns.Add("Column1", "LX");
                        dgvSpec.Columns.Add("Column2", "LY");
                        dgvSpec.Columns.Add("Column3", "RX");
                        dgvSpec.Columns.Add("Column4", "RY");
                        dgvSpec.Columns[0].Width = 50;
                        dgvSpec.Columns[1].Width = 50;
                        dgvSpec.Columns[2].Width = 50;
                        dgvSpec.Columns[3].Width = 50;
                    }
                    dgvSpec.Rows.Clear();
                    dgvSpec.Rows.Add(4);
                    dgvSpec[0, 0].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LX] - ToleranceX;// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X1 ;
                    dgvSpec[0, 1].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LX];// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X1;
                    dgvSpec[0, 2].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LX] + ToleranceX;// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X1 ;
                    dgvSpec[0, 3].Value = Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X1;

                    dgvSpec[1, 0].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LY] - ToleranceY; //+ Math.Round(APCOffset.Y1 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y1, 3);
                    dgvSpec[1, 1].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LY];// + Math.Round(APCOffset.Y1 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y1, 3);
                    dgvSpec[1, 2].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LY] + ToleranceY;// + Math.Round(APCOffset.Y1 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y1, 3) ;
                    dgvSpec[1, 3].Value = APCOffset.Y1 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y1;

                    dgvSpec[2, 0].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RX] - ToleranceX;// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X2 ;
                    dgvSpec[2, 1].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RX];// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X2;
                    dgvSpec[2, 2].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RX] + ToleranceX;// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X2 ;
                    dgvSpec[2, 3].Value = Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X2;

                    dgvSpec[3, 0].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RY] - ToleranceY;// + Math.Round(APCOffset.Y2 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y2, 3) ;
                    dgvSpec[3, 1].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RY];// + Math.Round(APCOffset.Y2 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y2, 3);
                    dgvSpec[3, 2].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RY] + ToleranceY;// + Math.Round(APCOffset.Y2 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y2, 3) ;
                    dgvSpec[3, 3].Value = APCOffset.Y2 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y2;

                    if (Menu.frmSetting.revData.mBendingArm.bDisplayLRSwapBD)
                    {
                        dgvSpec[2, 0].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LX] - ToleranceX;// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X1 ;
                        dgvSpec[2, 1].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LX];// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X1;
                        dgvSpec[2, 2].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LX] + ToleranceX;// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X1 ;
                        dgvSpec[2, 3].Value = Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X1;

                        dgvSpec[3, 0].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LY] - ToleranceY; //+ Math.Round(APCOffset.Y1 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y1, 3);
                        dgvSpec[3, 1].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LY];// + Math.Round(APCOffset.Y1 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y1, 3);
                        dgvSpec[3, 2].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LY] + ToleranceY;// + Math.Round(APCOffset.Y1 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y1, 3) ;
                        dgvSpec[3, 3].Value = APCOffset.Y1 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y1;

                        dgvSpec[0, 0].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RX] - ToleranceX;// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X2 ;
                        dgvSpec[0, 1].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RX];// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X2;
                        dgvSpec[0, 2].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RX] + ToleranceX;// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X2 ;
                        dgvSpec[0, 3].Value = Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X2;

                        dgvSpec[1, 0].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RY] - ToleranceY;// + Math.Round(APCOffset.Y2 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y2, 3) ;
                        dgvSpec[1, 1].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RY];// + Math.Round(APCOffset.Y2 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y2, 3);
                        dgvSpec[1, 2].Value = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RY] + ToleranceY;// + Math.Round(APCOffset.Y2 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y2, 3) ;
                        dgvSpec[1, 3].Value = APCOffset.Y2 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y2;
                    }

                }
                else if (strTemp.Contains("ATTACH"))
                {
                    double ToleranceX = Menu.frmSetting.revData.mSizeSpecRatio.AttachToleranceX;
                    double ToleranceY = Menu.frmSetting.revData.mSizeSpecRatio.AttachToleranceY;
                    if (dgvSpec.Columns.Count == 0)
                    {
                        dgvSpec.Columns.Clear();
                        dgvSpec.Columns.Add("Column1", "LX");
                        dgvSpec.Columns.Add("Column2", "LY");
                        dgvSpec.Columns.Add("Column3", "RX");
                        dgvSpec.Columns.Add("Column4", "RY");
                        dgvSpec.Columns[0].Width = 50;
                        dgvSpec.Columns[1].Width = 50;
                        dgvSpec.Columns[2].Width = 50;
                        dgvSpec.Columns[3].Width = 50;
                    }

                    lbloffset[0].Text = "CPK";


                    dgvSpec.Rows.Clear();
                    dgvSpec.Rows.Add(4);


                    double insp = CONST.RunRecipe.Param[eRecipe.FOAM_LENGTH_Y];
                    insp = 0;  // Foam Length 미사용
                }
                else if (strTemp == "SCF")
                {
                    SCFInspectionSpec_Write(dgvSpec1);
                }
                else if (strTemp.Contains("INSP"))
                {

                    lbloffset[DispCnt].Text = "CPK";
                    if (dgvSpec.Columns.Count == 0)
                    {
                        dgvSpec.Columns.Clear();
                        dgvSpec.Columns.Add("Column1", "LX");
                        dgvSpec.Columns.Add("Column2", "LY");
                        dgvSpec.Columns.Add("Column3", "RX");
                        dgvSpec.Columns.Add("Column4", "RY");
                        dgvSpec.Columns[0].Width = 48;
                        dgvSpec.Columns[1].Width = 48;
                        dgvSpec.Columns[2].Width = 48;
                        dgvSpec.Columns[3].Width = 48;
                    }
                    if (CamNo == Vision_No.Laser1 || CamNo == Vision_No.Laser2)
                    {
                        SetLaserInspectionSpec(out sSpec SpecX, out sSpec SpecY);
                        SetdgvSpec(SpecX, SpecY, ref dgvSpec);
                    }
                    else
                    {
                        SetInspectionSpec(out sSpec SpecX, out sSpec SpecY);

                        SetdgvSpec(SpecX, SpecY, ref dgvSpec);
                    }
                }
            }
            catch { }
        }
        public struct sHistory
        {
            //KSJ 20170611
            public string PanelID;

            public double LX;
            public double LY;
            public double RX;
            public double RY;
            public int RetryCnt;
            public int TimeCnt;
            public int ToolNo;
            public string camName;
            public string ApnCode;
        }

        //210119 cjm ArmPre 인터락
        private ePCResult CompareLimit(short sVisionNo, cs2DAlign.ptAlignResult align)
        {
            SetAlignLimit(sVisionNo, out double alignXlimit, out double alignYlimit, out double alignTlimit);
            if (Math.Abs(align.X) > alignXlimit || Math.Abs(align.Y) > alignYlimit || Math.Abs(align.T) > alignTlimit)
            {
                return ePCResult.ALIGN_LIMIT;
            }
            else
            {
                return ePCResult.WAIT;
            }

        }
        private void SetAlignLimit(short sVisionNo, out double alignXlimit, out double alignYlimit, out double alignTlimit)
        {
            alignXlimit = Vision[sVisionNo].CFG.AlignLimitX;
            if (alignXlimit == 0) alignXlimit = 9999;
            alignYlimit = Vision[sVisionNo].CFG.AlignLimitY;
            if (alignYlimit == 0) alignYlimit = 9999;
            alignTlimit = Vision[sVisionNo].CFG.AlignLimitT;
            if (alignTlimit == 0) alignTlimit = 9999;
        }

        //210119 cjm ArmPre 인터락
        private void SetArmPreAlignLimit(short sVisionNo, out double ArmPrealignXlimit, out double ArmPrealignYlimit, out double ArmPrealignTlimit)
        {
            ArmPrealignXlimit = Vision[sVisionNo].CFG.ArmPreAlignLimitX;
            if (ArmPrealignXlimit == 0) ArmPrealignXlimit = 9999;
            ArmPrealignYlimit = Vision[sVisionNo].CFG.ArmPreAlignLimitY;
            if (ArmPrealignYlimit == 0) ArmPrealignYlimit = 9999;
            ArmPrealignTlimit = Vision[sVisionNo].CFG.ArmPreAlignLimitT;
            if (ArmPrealignTlimit == 0) ArmPrealignTlimit = 9999;
        }
        private void NotUseXYT(short sVisionNo, ref cs2DAlign.ptAlignResult align)
        {
            string msg = "";
            if (Vision[sVisionNo].CFG.AlignNotUseX || Vision[sVisionNo].CFG.AlignNotUseY || Vision[sVisionNo].CFG.AlignNotUseT)
            {
                msg += "Align NotUse ";
            }
            if (Vision[sVisionNo].CFG.AlignNotUseX)
            {
                align.X = 0;
                msg += "X ";
            }
            if (Vision[sVisionNo].CFG.AlignNotUseY)
            {
                align.Y = 0;
                msg += "Y ";
            }
            if (Vision[sVisionNo].CFG.AlignNotUseT)
            {
                align.T = 0;
                msg += "T ";
            }
            if (msg != "") LogDisp(sVisionNo, msg);
        }

        public void ReverseXYT(short sVisionNo, ref cs2DAlign.ptAlignResult align)
        {
            if (Vision[sVisionNo].CFG.XAxisRevers)
            {
                align.X = -align.X;
            }
            if (Vision[sVisionNo].CFG.YAxisRevers)
            {
                align.Y = -align.Y;
            }
            if (Vision[sVisionNo].CFG.TAxisRevers)
            {
                align.T = -align.T;
            }
        }

        //2020.09.15 lkw 수정
        //20.10.11 lkw
        public ePCResult Lcheck(short visionNO, int iCalNo, cs2DAlign.ptXYT pixel1, cs2DAlign.ptXYT pixel2, eRecipe Spec, eRecipe Tolerence, double offset, ref double lengthresult, bool bY = false)
        {
            //확인필요
            bool bpanelLength = true;
            double Lspec = CONST.RunRecipe.Param[Spec];
            double LTolerence = CONST.RunRecipe.Param[Tolerence];

            //bool notMotorUse = false;

            cs2DAlign.ptXY Point1 = new cs2DAlign.ptXY();
            cs2DAlign.ptXY Point2 = new cs2DAlign.ptXY();

            Point1.X = pixel1.X;
            Point1.Y = pixel1.Y;
            Point2.X = pixel2.X;
            Point2.Y = pixel2.Y;
            double length = 0;
            if (bY)
                length = Menu.rsAlign.getLength_Y(iCalNo, iCalNo + 1, Point1, Point2, offset);
            else
            {
                length = Menu.rsAlign.getLength(iCalNo, iCalNo + 1, Point1, Point2, offset);
            }

            lengthresult = length;
            if (LTolerence != 0)
            {
                if (Math.Abs(length - Lspec) > LTolerence)
                {
                    bpanelLength = false;
                    string sLog = "L-Check Error : " + length.ToString("0.000") + "(Spec : " + Lspec.ToString("0.000") + ")";
                    LogDisp(visionNO, sLog);
                }
            }
            if (!bpanelLength)
            {
                return ePCResult.ERROR_LCHECK;
            }
            else //정상
            {
                return ePCResult.WAIT;
            }
        }

        public void setResultHistory(ucAutoMain.sHistory history, int dispCnt, bool first = false, bool bCPK = false, bool bEdgetoMark = false, bool useInspDiffBD = false)
        {
            try
            {
                if (history.PanelID.Length > 20)
                    history.PanelID = history.PanelID.Substring(0, 16);

                ResultHistoryDisplay(history, dispCnt, first, bCPK, bEdgetoMark, useInspDiffBD);
            }
            catch (Exception EX)
            {
                //DBrd.Close();
                cLog.ExceptionLogSave("setResultHistory" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }
        public void ResultHistoryDisplay(sHistory history, int dispCnt, bool first = false, bool bCPK = false, bool bEdgetoMark = false, bool useInspDiffBD = false)
        {
            //hakim 20170622
            DataGridView dgv = null;
            DataGridView dgvSpec = null;

            //double OffsetY1 = 0;
            //double OffsetY2 = 0;

            if (dispCnt == 0)
            {
                dgv = dgvResult1;
                dgvSpec = dgvSpec1;
            }
            else if (dispCnt == 1)
            {
                dgv = dgvResult2;
                dgvSpec = dgvSpec2;
            }
            else if (dispCnt == 2)
            {
                dgv = dgvResult3;
                dgvSpec = dgvSpec3;
            }
            else if (dispCnt == 3)
            {
                dgv = dgvResult4;
                dgvSpec = dgvSpec4;
            }

            if (bCPK && lbloffset[dispCnt].Text != "CPK")
            {
                if (lbloffset[dispCnt].InvokeRequired)
                {
                    lbloffset[dispCnt].Invoke(new MethodInvoker(delegate
                    {
                        lbloffset[dispCnt].Text = "CPK";
                        lbloffset[dispCnt].Refresh();
                    }));
                }
                else
                {
                    lbloffset[dispCnt].Text = "CPK";
                    lbloffset[dispCnt].Refresh();
                }
            }

            if (dgv.InvokeRequired)
            {
                dgv.Invoke(new MethodInvoker(delegate
                {
                    //임시
                    WriteResultRow(dgv, dgvSpec, history, 0, dispCnt, first, bCPK, bEdgetoMark, useInspDiffBD);
                    //pcy190527 31 -> 30
                    if (dgv.Rows.Count > 30)
                    {
                        int nCount = 0;
                        nCount = dgv.Rows.Count - 30;
                        for (int i = 0; i < nCount; i++)
                        {
                            dgv.Rows.RemoveAt(30);
                        }
                    }
                }));
            }
            else
            {
                //임시
                WriteResultRow(dgv, dgvSpec, history, 0, dispCnt, first, bCPK, bEdgetoMark, useInspDiffBD);

                if (dgv.Rows.Count > 30)
                {
                    int nCount = 0;
                    nCount = dgv.Rows.Count - 30;
                    for (int i = 0; i < nCount; i++)
                    {
                        dgv.Rows.RemoveAt(30);
                    }
                }
            }
        }

        public ePCResult AlignXYT(int VisionNo, int CalPos, cs2DAlign.ptXYT Mark1, cs2DAlign.ptXYT Mark2, ref cs2DAlign.ptAlignResult Align,
            bool markMove = true, double Toffset = 0, bool retry = false, bool notUseOffset = false, cs2DAlign.ptXYT Mark3 = default(cs2DAlign.ptXYT))
        {
            bool bgetoffset = false;
            // 화면 중심으로 Align
            cs2DAlign.ptXY sourcePixel1 = new cs2DAlign.ptXY();
            cs2DAlign.ptXY sourcePixel2 = new cs2DAlign.ptXY();
            cs2DAlign.ptXY sourcePixel3 = new cs2DAlign.ptXY();
            cs2DAlign.ptXY targetPixel1 = new cs2DAlign.ptXY();
            cs2DAlign.ptXY targetPixel2 = new cs2DAlign.ptXY();
            cs2DAlign.ptXY targetPixel3 = new cs2DAlign.ptXY();
            //double sourceTheta = 0;            

            if (markMove)
            {
                sourcePixel1.X = Mark1.X;
                sourcePixel1.Y = Mark1.Y;
                sourcePixel2.X = Mark2.X;
                sourcePixel2.Y = Mark2.Y;
                sourcePixel3.X = Mark3.X;
                sourcePixel3.Y = Mark3.Y;

                if (Vision[VisionNo].CFG.CenterAlign)
                {
                    targetPixel1.X = Vision[VisionNo].ImgX / 2.0;
                    targetPixel1.Y = Vision[VisionNo].ImgY / 2.0;
                    targetPixel2.X = Vision[VisionNo].ImgX / 2.0;
                    targetPixel2.Y = Vision[VisionNo].ImgY / 2.0;
                    targetPixel3.X = Vision[VisionNo].ImgX / 2.0;
                    targetPixel3.Y = Vision[VisionNo].ImgY / 2.0;
                }
                else
                {
                    if (Vision[VisionNo].CFG.CalType == eCalType.Cam1Cal2 || Vision[VisionNo].CFG.CalType == eCalType.Cam1Type)
                    {
                        if (CalPos != (int)eCalPos.Laser1 || CalPos != (int)eCalPos.Laser2)
                        {
                            targetPixel1.X = Vision[VisionNo + 0].CFG.TargetX[(int)ePatternKind.Left_1cam];
                            targetPixel1.Y = Vision[VisionNo + 0].CFG.TargetY[(int)ePatternKind.Left_1cam];
                            targetPixel2.X = Vision[VisionNo + 0].CFG.TargetX[(int)ePatternKind.Right_1cam];
                            targetPixel2.Y = Vision[VisionNo + 0].CFG.TargetY[(int)ePatternKind.Right_1cam];
                        }
                        else
                        {
                            targetPixel1.X = Vision[VisionNo + 0].CFG.TargetX[(int)ePatternKind.Panel];
                            targetPixel1.Y = Vision[VisionNo + 0].CFG.TargetY[(int)ePatternKind.Panel];
                            targetPixel2.X = Vision[VisionNo + 0].CFG.TargetX[(int)ePatternKind.Panel];
                            targetPixel2.Y = Vision[VisionNo + 0].CFG.TargetY[(int)ePatternKind.Panel];
                        }
                    }
                    else
                    {
                        targetPixel1.X = Vision[VisionNo + 0].CFG.TargetX[(int)ePatternKind.Panel];
                        targetPixel1.Y = Vision[VisionNo + 0].CFG.TargetY[(int)ePatternKind.Panel];
                        targetPixel2.X = Vision[VisionNo + 1].CFG.TargetX[(int)ePatternKind.Panel];
                        targetPixel2.Y = Vision[VisionNo + 1].CFG.TargetY[(int)ePatternKind.Panel];
                        targetPixel3.X = Vision[VisionNo + 2].CFG.TargetX[(int)ePatternKind.Panel];
                        targetPixel3.Y = Vision[VisionNo + 2].CFG.TargetY[(int)ePatternKind.Panel];
                    }
                }
            }
            else
            {
                targetPixel1.X = Mark1.X;
                targetPixel1.Y = Mark1.Y;
                targetPixel2.X = Mark2.X;
                targetPixel2.Y = Mark2.Y;
                targetPixel3.X = Mark3.X;
                targetPixel3.Y = Mark3.Y;

                if (Vision[VisionNo].CFG.CenterAlign)  //Laser일 경우 무조건 Center Align
                {
                    sourcePixel1.X = Vision[VisionNo].ImgX / 2.0;
                    sourcePixel1.Y = Vision[VisionNo].ImgY / 2.0;
                    sourcePixel2.X = Vision[VisionNo].ImgX / 2.0;
                    sourcePixel2.Y = Vision[VisionNo].ImgY / 2.0;
                    sourcePixel3.X = Vision[VisionNo].ImgX / 2.0;
                    sourcePixel3.Y = Vision[VisionNo].ImgY / 2.0;
                }
                else
                {
                    if (Vision[VisionNo].CFG.CalType == eCalType.Cam1Cal2 || Vision[VisionNo].CFG.CalType == eCalType.Cam1Type)
                    {

                        if (CalPos != (int)eCalPos.Laser1 || CalPos != (int)eCalPos.Laser2)
                        {
                            sourcePixel1.X = Vision[VisionNo + 0].CFG.TargetX[(int)ePatternKind.Left_1cam];
                            sourcePixel1.Y = Vision[VisionNo + 0].CFG.TargetY[(int)ePatternKind.Left_1cam];
                            sourcePixel2.X = Vision[VisionNo + 0].CFG.TargetX[(int)ePatternKind.Right_1cam];
                            sourcePixel2.Y = Vision[VisionNo + 0].CFG.TargetY[(int)ePatternKind.Right_1cam];
                        }
                        else
                        {
                            sourcePixel1.X = Vision[VisionNo + 0].CFG.TargetX[(int)ePatternKind.Panel];
                            sourcePixel1.Y = Vision[VisionNo + 0].CFG.TargetY[(int)ePatternKind.Panel];
                            sourcePixel2.X = Vision[VisionNo + 0].CFG.TargetX[(int)ePatternKind.Panel];
                            sourcePixel2.Y = Vision[VisionNo + 0].CFG.TargetY[(int)ePatternKind.Panel];
                        }
                    }
                    else
                    {
                        sourcePixel1.X = Vision[VisionNo + 0].CFG.TargetX[(int)ePatternKind.Panel];
                        sourcePixel1.Y = Vision[VisionNo + 0].CFG.TargetY[(int)ePatternKind.Panel];
                        sourcePixel2.X = Vision[VisionNo + 1].CFG.TargetX[(int)ePatternKind.Panel];
                        sourcePixel2.Y = Vision[VisionNo + 1].CFG.TargetY[(int)ePatternKind.Panel];
                        sourcePixel3.X = Vision[VisionNo + 2].CFG.TargetX[(int)ePatternKind.Panel];
                        sourcePixel3.Y = Vision[VisionNo + 2].CFG.TargetY[(int)ePatternKind.Panel];
                    }
                }
            }

            cs2DAlign.ptOffset offset = new cs2DAlign.ptOffset();
            //2019.07.20 EMI Align 추가
            // Inspection Data는 Offset 사용 안함.


            offset.X1 = Menu.frmSetting.revData.mOffset[VisionNo].AlignOffsetXYT.X;
            offset.X2 = Menu.frmSetting.revData.mOffset[VisionNo].AlignOffsetXYT.X;
            offset.Y1 = Menu.frmSetting.revData.mOffset[VisionNo].AlignOffsetXYT.Y;
            offset.Y2 = Menu.frmSetting.revData.mOffset[VisionNo].AlignOffsetXYT.Y;
            offset.T = Menu.frmSetting.revData.mOffset[VisionNo].AlignOffsetXYT.T;

            if (CalPos == (int)eCalPos.LoadingPre2_1)
            {
                offset.X1 = Menu.frmSetting.revData.mOffset[VisionNo + 1].AlignOffsetXYT.X;
                offset.X2 = Menu.frmSetting.revData.mOffset[VisionNo + 1].AlignOffsetXYT.X;
                offset.Y1 = Menu.frmSetting.revData.mOffset[VisionNo + 1].AlignOffsetXYT.Y;
                offset.Y2 = Menu.frmSetting.revData.mOffset[VisionNo + 1].AlignOffsetXYT.Y;
                offset.T = Menu.frmSetting.revData.mOffset[VisionNo + 1].AlignOffsetXYT.T;
            }


            //APC
            if (CalPos == (int)eCalPos.Laser1)
            {
                offset.X1 -= CONST.RunRecipe.Param[eRecipe.LASER_MARKING_OFFSET_X1];
                offset.X2 -= CONST.RunRecipe.Param[eRecipe.LASER_MARKING_OFFSET_X1];
                offset.Y1 += CONST.RunRecipe.Param[eRecipe.LASER_MARKING_OFFSET_Y1];
                offset.Y2 += CONST.RunRecipe.Param[eRecipe.LASER_MARKING_OFFSET_Y1];

            }
            else if (CalPos == (int)eCalPos.Laser2)
            {
                offset.X1 -= CONST.RunRecipe.Param[eRecipe.LASER_MARKING_OFFSET_X2];
                offset.X2 -= CONST.RunRecipe.Param[eRecipe.LASER_MARKING_OFFSET_X2];
                offset.Y1 += CONST.RunRecipe.Param[eRecipe.LASER_MARKING_OFFSET_Y2];
                offset.Y2 += CONST.RunRecipe.Param[eRecipe.LASER_MARKING_OFFSET_Y2];
            }
            //20.10.12 lkw 
            cs2DAlign.ptXXYY dist = new cs2DAlign.ptXXYY();

            cs2DAlign.stAlign param = new cs2DAlign.stAlign();
            param.OtherCalUse = false;
            param.Toffset = Toffset;
            //param.Ydirection = Ydirection; //주석처리

            if (notUseOffset)
            {
                offset = default(cs2DAlign.ptOffset);
            }

            Menu.rsAlign.setAlignParam(CalPos, param);

            if (CalPos != (int)eCalPos.Laser1 && CalPos != (int)eCalPos.Laser2)
                Align = Menu.rsAlign.getAlign(CalPos, sourcePixel1, targetPixel1, CalPos + 1, sourcePixel2, targetPixel2, offset, ref dist, retry, bgetoffset);
            else
                Align = Menu.rsAlign.getAlign(CalPos, sourcePixel1, targetPixel1, CalPos, sourcePixel2, targetPixel2, offset, ref dist, retry, bgetoffset);

            //2020.09.29 lkw
            if (Vision[VisionNo].CFG.AlignUse && Mark3.X != 0 && Mark3.Y != 0)  // Reel Pre는 빼기 위해...
            {
                param.Point3Align = true;
                param.Point3CalNo = CalPos + 2;
                param.Point3sourcePixel = sourcePixel3;
                param.Point3targetPixel = targetPixel3;

                Menu.rsAlign.setAlignParam(CalPos, param);
                cs2DAlign.ptAlignResult Align_3Point = Menu.rsAlign.getAlign(CalPos, sourcePixel1, targetPixel1, CalPos + 1, sourcePixel2, targetPixel2, offset, ref dist, retry, bgetoffset);

                Align.Y = Align_3Point.Y;
                Align.T = Align_3Point.T;
                ////20.10.17 lkw 3점 Align 정상화
                Align.X = Align_3Point.X;
            }

            return ePCResult.WAIT;
        }

        #region LogSave

        public void LogSaveBendingPre(LogKind lckind, int VisionNo, cs2DAlign.ptXY RefMark1, cs2DAlign.ptXY RefMark2, cs2DAlign.ptAlignResult Align, double LCheckDist, string PNID, string OKNG, double Time)
        {
            double dX1 = RefMark1.X;// * Vision[VisionNo].CFG.Resolution;
            double dY1 = RefMark1.Y;// * Vision[VisionNo].CFG.Resolution;
            double dX2 = RefMark2.X;// * Vision[VisionNo + 1].CFG.Resolution;
            double dY2 = RefMark2.Y;// * Vision[VisionNo + 1].CFG.Resolution;

            double diffX1 = ((Vision[VisionNo].ImgX / 2) - RefMark1.X) * Vision[VisionNo].CFG.Resolution;
            double diffY1 = ((Vision[VisionNo].ImgY / 2) - RefMark1.Y) * Vision[VisionNo].CFG.Resolution;
            double diffX2 = ((Vision[VisionNo + 1].ImgX / 2) - RefMark2.X) * Vision[VisionNo + 1].CFG.Resolution;
            double diffY2 = ((Vision[VisionNo + 1].ImgY / 2) - RefMark2.Y) * Vision[VisionNo + 1].CFG.Resolution;

            string strLog = "";

            strLog = PNID.Trim() + "," + OKNG + "," + dX1.ToString("0.000") + "," + dY1.ToString("0.000") + "," + dX2.ToString("0.000") + "," + dY2.ToString("0.000") + "," + diffX1.ToString("0.0000") + "," + diffY1.ToString("0.000") + "," +
                diffX2.ToString("0.000") + "," + diffY2.ToString("0.000") + "," + Align.X.ToString("0.000") + "," + Align.Y.ToString("0.000") + "," + Align.T.ToString("0.000")
                + "," + LCheckDist.ToString("0.000") + "," + Time.ToString("0.000");

            cLog.Save(lckind, strLog);
        }

        public void LogSaveLoadPre(LogKind lckind, int VisionNo, cs2DAlign.ptXYT RefMark1, cs2DAlign.ptXYT RefMark2, cs2DAlign.ptAlignResult align,
            double LCheckDist, string PNID, string OKNG, double Time, double Theta, bool bDLPanelFind, string AlignNO)
        {
            double dX1 = RefMark1.X;
            double dY1 = RefMark1.Y;
            double dX2 = RefMark2.X;
            double dY2 = RefMark2.Y;

            double diffX1 = 0;
            double diffY1 = 0;
            double diffX2 = 0;
            double diffY2 = 0;
            eCalPos calpos = 0;
            cs2DAlign.ptXY resolution1 = new cs2DAlign.ptXY();
            cs2DAlign.ptXY resolution2 = new cs2DAlign.ptXY();
            cs2DAlign.ptXY pixelcnt = new cs2DAlign.ptXY();
            GetCalPos(ref calpos, VisionNo);
            Menu.rsAlign.getResolution((int)calpos, ref resolution1, ref pixelcnt);
            Menu.rsAlign.getResolution((int)calpos + 1, ref resolution2, ref pixelcnt);

            if (Vision[VisionNo].CFG.CalType == eCalType.Cam1Cal2)
            {
                diffX1 = (Vision[VisionNo + 0].CFG.TargetX[(int)ePatternKind.Left_1cam] - RefMark1.X) * resolution1.X;
                diffY1 = (Vision[VisionNo + 0].CFG.TargetY[(int)ePatternKind.Left_1cam] - RefMark1.Y) * resolution1.Y;
                diffX2 = (Vision[VisionNo + 0].CFG.TargetX[(int)ePatternKind.Right_1cam] - RefMark2.X) * resolution2.X;
                diffY2 = (Vision[VisionNo + 0].CFG.TargetY[(int)ePatternKind.Right_1cam] - RefMark2.Y) * resolution2.Y;
            }
            else
            {
                diffX1 = (Vision[VisionNo + 0].CFG.TargetX[(int)ePatternKind.Panel] - RefMark1.X) * resolution1.X;
                diffY1 = (Vision[VisionNo + 0].CFG.TargetY[(int)ePatternKind.Panel] - RefMark1.Y) * resolution1.Y;
                diffX2 = (Vision[VisionNo + 1].CFG.TargetX[(int)ePatternKind.Panel] - RefMark2.X) * resolution2.X;
                diffY2 = (Vision[VisionNo + 1].CFG.TargetY[(int)ePatternKind.Panel] - RefMark2.Y) * resolution2.Y;
            }

            string strLog = "";

            strLog = PNID.Trim() + "," + AlignNO + "," + OKNG + "," + dX1.ToString("0.000") + "," + dY1.ToString("0.000") + "," + dX2.ToString("0.000") + "," + dY2.ToString("0.000") + "," + diffX1.ToString("0.0000") + "," + diffY1.ToString("0.000") + "," +
                diffX2.ToString("0.000") + "," + diffY2.ToString("0.000") + "," + align.X.ToString("0.000") + "," + align.Y.ToString("0.000") + "," + align.T.ToString("0.000") + "," +
                LCheckDist.ToString("0.000") + "," + Theta.ToString("0.000") + "," + Time.ToString("0.000") + "," + bDLPanelFind.ToString();

            cLog.Save(lckind, strLog);
        }

        public void LogSaveBDPreSCFInspection(LogKind lckind, string PNID, cs2DAlign.ptXXYY SCFDist, string OKNG)
        {
            string strLog = "";

            strLog = PNID + "," + OKNG + "," + SCFDist.X1.ToString("0.000") + "," + SCFDist.Y1.ToString("0.000") + "," + SCFDist.X2.ToString("0.000") + "," + SCFDist.Y2.ToString("0.000");

            cLog.Save(lckind, strLog);
        }

        public void LogSaveBDSCFInspection(LogKind lckind, int BendNo, string PNID, cs2DAlign.ptXXYY SCFDist, string OKNG)
        {
            string strLog = "";

            strLog = PNID + "," + BendNo.ToString("0") + "," + OKNG + "," + SCFDist.X1.ToString("0.000") + "," + SCFDist.Y1.ToString("0.000") + "," + SCFDist.X2.ToString("0.000") + "," + SCFDist.Y2.ToString("0.000");

            cLog.Save(lckind, strLog);
        }
        //201012 cjm strLog 수정
        public void LogSaveRobot(LogKind lckind, string PNID, int BendNo, int Retry, cs2DAlign.ptXYT Mark1, cs2DAlign.ptXYT RefMark1, cs2DAlign.ptXYT Mark2, cs2DAlign.ptXYT RefMark2,
            double AlignX, double AlignY, double AlignT, cs2DAlign.ptXXYY Dist, double AlignMoveAddX = 0, double AlignMoveAddY = 0, double AlignMoveAddT = 0,
            bool bspecinout = false, bool DLPanelFind = false, bool DLFPCFind = false, double[] score = null) //이줄 여기 
        {
            //string strLog = "";
            string specinout = "IN";
            if (bspecinout) specinout = "OUT";
            if (score == null) specinout = "BDINSP"; //여기 
            string strScore = "";
            if (score != null)
            {
                for (int i = 0; i < score.Length - 1; i++)
                {
                    strScore = strScore + score[i].ToString("0.000") + ",";
                }
                strScore = strScore + score[score.Length - 1];
            }
            string strLog = "";
            if (Menu.frmSetting.revData.mBendingArm.bDisplayLRSwapBD)
            {
                strLog = PNID.Trim() + "," +
                (BendNo + 1).ToString("0") + "," +
                Retry.ToString("0") + "," +
                Mark1.X.ToString("0.000") + "," +
                Mark1.Y.ToString("0.000") + "," +
                RefMark1.X.ToString("0.000") + "," +
                RefMark1.Y.ToString("0.000") + "," +
                Mark2.X.ToString("0.000") + "," +
                Mark2.Y.ToString("0.000") + "," +
                RefMark2.X.ToString("0.000") + "," +
                RefMark2.Y.ToString("0.000") + "," +
                AlignX.ToString("0.000") + "," +
                AlignY.ToString("0.000") + "," +
                AlignT.ToString("0.000") + "," +
                Dist.X2.ToString("0.000") + "," +
                Dist.Y2.ToString("0.000") + "," +
                Dist.X1.ToString("0.000") + "," +
                Dist.Y1.ToString("0.000") + "," +
                AlignMoveAddX.ToString("0.000") + "," +
                AlignMoveAddY.ToString("0.000") + "," +
                AlignMoveAddT.ToString("0.000") + "," +
                specinout + "," +//여기부터 
                DLPanelFind.ToString() + "," +
                DLFPCFind.ToString() + "," +
                strScore;
            }
            else
            {
                strLog = PNID.Trim() + "," +
                (BendNo + 1).ToString("0") + "," +
                Retry.ToString("0") + "," +
                Mark1.X.ToString("0.000") + "," +
                Mark1.Y.ToString("0.000") + "," +
                RefMark1.X.ToString("0.000") + "," +
                RefMark1.Y.ToString("0.000") + "," +
                Mark2.X.ToString("0.000") + "," +
                Mark2.Y.ToString("0.000") + "," +
                RefMark2.X.ToString("0.000") + "," +
                RefMark2.Y.ToString("0.000") + "," +
                AlignX.ToString("0.000") + "," +
                AlignY.ToString("0.000") + "," +
                AlignT.ToString("0.000") + "," +
                Dist.X1.ToString("0.000") + "," +
                Dist.Y1.ToString("0.000") + "," +
                Dist.X2.ToString("0.000") + "," +
                Dist.Y2.ToString("0.000") + "," +
                AlignMoveAddX.ToString("0.000") + "," +
                AlignMoveAddY.ToString("0.000") + "," +
                AlignMoveAddT.ToString("0.000") + "," +
                specinout + "," +//여기부터 
                DLPanelFind.ToString() + "," +
                DLFPCFind.ToString() + "," +
                strScore;
            }
            //StringBuilder slog = new StringBuilder();
            //slog.Append(PNID);
            //slog.Append((BendNo + 1).ToString("0"));
            //slog.Append(Retry.ToString("0"));
            //slog.Append(Mark1.X.ToString("0.000"));
            //slog.Append(Mark1.Y.ToString("0.000"));
            //slog.Append(RefMark1.X.ToString("0.000"));
            //slog.Append(RefMark1.Y.ToString("0.000"));
            //slog.Append(Mark2.X.ToString("0.000"));
            //slog.Append(Mark2.Y.ToString("0.000"));
            //slog.Append(RefMark2.X.ToString("0.000"));
            //slog.Append(RefMark2.Y.ToString("0.000"));
            //slog.Append(AlignX.ToString("0.000"));
            //slog.Append(AlignY.ToString("0.000"));
            //slog.Append(AlignT.ToString("0.000"));
            //slog.Append(Dist.X1.ToString("0.000"));
            //slog.Append(Dist.Y1.ToString("0.000"));
            //slog.Append(Dist.X2.ToString("0.000"));
            //slog.Append(Dist.Y2.ToString("0.000"));
            //slog.Append(AlignMoveAddX.ToString("0.000"));
            //slog.Append(AlignMoveAddY.ToString("0.000"));
            //slog.Append(AlignMoveAddT.ToString("0.000"));
            //slog.Append(specinout);
            //slog.Append(DLPanelFind.ToString());
            //slog.Append(DLFPCFind.ToString());
            //if (score != null)
            //{
            //    foreach(var s in score)
            //    {
            //        slog.Append(s.ToString());
            //    }
            //}
            //cLog.Save(lckind, String.Join(",", slog));

            cLog.Save(lckind, strLog);
        }

        public void LogSaveBDVision(LogKind lckind, cs2DAlign.ptXYT RefMark1, cs2DAlign.ptXYT RefMark2, cs2DAlign.ptAlignResult align,
             double LCheckDist, string PNID, string OKNG, int BendingNo, bool bDLPanelFind)
        {
            //StringBuilder slog = new StringBuilder();
            //slog.Append(PNID);
            //slog.Append(BendingNo.ToString("0"));
            //slog.Append(OKNG);
            //slog.Append(RefMark1.X.ToString("0.000"));
            //slog.Append(RefMark1.Y.ToString("0.000"));
            //slog.Append(RefMark2.X.ToString("0.000"));
            //slog.Append(RefMark2.Y.ToString("0.000"));
            //slog.Append(align.X.ToString("0.000"));
            //slog.Append(align.Y.ToString("0.000"));
            //slog.Append(align.T.ToString("0.000"));
            //slog.Append(LCheckDist.ToString("0.000"));

            //cLog.Save(lckind, String.Join(",", slog));

            string strLog = PNID + "," +
                (BendingNo + 1).ToString("0") + "," +
                OKNG + "," +
                RefMark1.X.ToString("0.000") + "," +
                RefMark1.Y.ToString("0.000") + "," +
                RefMark2.X.ToString("0.000") + "," +
                RefMark2.Y.ToString("0.000") + "," +
                align.X.ToString("0.000") + "," +
                align.Y.ToString("0.000") + "," +
                align.T.ToString("0.000") + "," +
                LCheckDist.ToString("0.000");

            cLog.Save(lckind, strLog);
        }

        #endregion LogSave
        //최대점갯수만큼 이거 선언..
        private double[] ResultDist1 = new double[32];
        private double[] ResultDist2 = new double[32];
        private double[] ResultDist3 = new double[32];
        private double[] ResultDist4 = new double[32];
        private double[] ResultDist5 = new double[32];
        private double[] ResultDist6 = new double[32];
        private double[] ResultDist7 = new double[32];
        private double[] ResultDist8 = new double[32];
        double[][] ResultDist;
        private int CalculCnt1 = 0;
        private void CalcDgvCPK(DataGridView dgv, DataGridView dgvSpec)
        {

            double tor = 0;
            double[] dCPK = new double[dgvSpec.Columns.Count];
            bool bspecin = true;
            for (int i = 0; i < dgvSpec.Columns.Count; i++)
            {
                double.TryParse(dgvSpec[i, 0].Value.ToString(), out double low);
                double.TryParse(dgvSpec[i, 2].Value.ToString(), out double high);
                double.TryParse(dgv.Rows[0].Cells[i].Value.ToString(), out double value);
                if (value < low || value > high || value == 0)
                {
                    dgv.Rows[0].Cells[i].Style.BackColor = Color.Red;
                    bspecin = false;
                }
            }

            if (bspecin)
            {
                for (int i = 0; i < dgvSpec.Columns.Count; i++)
                {
                    //201024 cjm Height Insp CPK 마이너스 값 나온 거 수정함
                    tor = Convert.ToDouble(dgvSpec[i, 1].Value) - Convert.ToDouble(dgvSpec[i, 0].Value);

                    ResultDist[i][CalculCnt1] = Convert.ToDouble(dgv[i, 0].Value);
                    dCPK[i] = AutoMainLogDatacalculator(ResultDist[i], Convert.ToDouble(dgvSpec[i, 1].Value), tor);
                }

                //이러면 0~31까지니까 32개 계산
                if (CalculCnt1 < 31)
                {
                    CalculCnt1++;
                }
                else
                {
                    CalculCnt1 = 0;
                    bSendCPK = true;
                }

                if (dgvSpec.InvokeRequired)
                {
                    dgvSpec.Invoke(new MethodInvoker(delegate
                    {
                        for (int i = 0; i < dCPK.Length; i++)
                        {
                            dgvSpec[i, 3].Value = dCPK[i].ToString("0.000");
                        }
                    }));
                }
                else
                {
                    for (int i = 0; i < dCPK.Length; i++)
                    {
                        dgvSpec[i, 3].Value = dCPK[i].ToString("0.000");
                    }
                }
            }
        }

        private double[] ResultDistX1 = new double[32];
        private double[] ResultDistY1 = new double[32];
        private double[] ResultDistX2 = new double[32];
        private double[] ResultDistY2 = new double[32];
        double[][] ResultDistInsp; //인스펙션 전용

        //private double[] CPK = new double[6];
        private int CalculCnt = 0;
        private bool bSendCPK = false;

        private void WriteResultRow(DataGridView dgv, DataGridView dgvSpec, sHistory history, int _row, int dispCnt, bool first = false, bool bCPK = false, bool bEdgetoMark = false, bool useInspDiffBD = false)
        {
            //DataGridView dgv, dgvSpec;

            double Xspec = Menu.frmSetting.revData.mBendingArm.BDToleranceX;
            double Yspec = Menu.frmSetting.revData.mBendingArm.BDToleranceY;
            if (bCPK || bEdgetoMark) //cpk계산하는곳은 인스펙션이라고 생각함.
            {
                Xspec = Menu.frmSetting.revData.mBendingArm.InspToleranceX;
                Yspec = Menu.frmSetting.revData.mBendingArm.InspToleranceY;
            }

            if (history.camName == nameof(eCamNO.Laser1) || history.camName == nameof(eCamNO.Laser2))
            {
                Xspec = Menu.frmSetting.revData.mSizeSpecRatio.LaserPositionToleranceX;
                Yspec = Menu.frmSetting.revData.mSizeSpecRatio.LaserPositionToleranceY;
            }
            //pcy200707 스펙과 동일할때 스펙아웃처럼 보이는것 방지
            Xspec += 0.0001;
            Yspec += 0.0001;

            double lx = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LX];
            double ly = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LY];
            double rx = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RX];
            double ry = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RY];
            if (bCPK || bEdgetoMark)
            {
                lx = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LX];
                ly = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LY];
                rx = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RX];
                ry = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RY];
            }

            dgv.Rows.Insert(_row);

            //dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgv[0, _row].Value = Math.Round(history.LX, 3);
            dgv[1, _row].Value = Math.Round(history.LY, 3);
            dgv[2, _row].Value = Math.Round(history.RX, 3);
            dgv[3, _row].Value = Math.Round(history.RY, 3);
            try
            {
                dgv[4, _row].Value = history.PanelID;
                dgv[5, _row].Value = history.ApnCode;
                dgv[6, _row].Value = history.ToolNo;
            }
            catch
            { }

            //pcy190413 스펙아웃은 CPK계산에서 제외
            bool bspecin = true;

            //First Align 색상 표시
            for (int i = 0; i < 4; i++)
            {
                if (history.RetryCnt == 0 && !bCPK) // First IN , dispCnt == 3 이면 Inspection임
                {
                    //pcy190614 first값을 retry로 구분하지않고 first변수로 확인함(retry 0이어도 붙이는 경우가 생김)
                    if (first)
                    {
                        dgv.Rows[_row].DefaultCellStyle.BackColor = Color.LightGray;
                    }
                    else
                    {
                        if (useInspDiffBD)
                        {
                            dgv.Rows[_row].DefaultCellStyle.BackColor = Color.LightGray;
                        }
                        else
                            dgv.Rows[_row].DefaultCellStyle.BackColor = Color.Gray;
                    }

                    if (first)
                    {
                        dgv[4, _row].Value = "IN";
                    }
                    if (useInspDiffBD)
                    {
                        dgv[4, _row].Value = "Diff";
                    }
                }
                else if (history.RetryCnt == -1) //-1이면 Arm 다운 후 찍을때..
                {
                    dgv.Rows[_row].DefaultCellStyle.BackColor = Color.Gray;
                    dgv[4, _row].Value = "AfterBD";
                }
                else //First IN을 제외한 부분을 여기로 들어오면 될 듯
                {
                    //dgv.Columns[4].Width = 30;

                    //pcy200609 빨간불 여기가 문제..
                    double spec = (i % 2 == 0) ? Xspec : Yspec;

                    if (i == 0 || i == 2) //x
                    {
                        if (Math.Abs(double.Parse(dgvSpec[i, 1].Value.ToString()) - double.Parse(dgv[i, _row].Value.ToString())) > spec)
                        {
                            if (history.camName == nameof(eCamNO.Laser1) || history.camName == nameof(eCamNO.Laser2))
                                dgv.Rows[_row].Cells[i].Style.BackColor = Color.Red;
                            else dgv.Rows[_row].Cells[i].Style.BackColor = Color.LightGray;
                            bspecin = false; //pcy190414 x스펙아웃도 제외
                        }
                        else
                            dgv.Rows[_row].Cells[i].Style.BackColor = Color.Gray;

                    }
                    else //y
                    {
                        if (Math.Abs(double.Parse(dgvSpec[i, 1].Value.ToString()) - double.Parse(dgv[i, _row].Value.ToString())) > spec)
                        {
                            dgv.Rows[_row].Cells[i].Style.BackColor = Color.Red;
                            bspecin = false;
                        }
                        else
                        {
                            dgv.Rows[_row].Cells[i].Style.BackColor = Color.Gray;
                        }
                    }


                }
            }

            //CPK Disp
            //pcy190412 CPK계산부분 버그수정 & 하나라도 스펙아웃이면 cpk계산 안하도록 하는 bool변수 추가.
            if ((bCPK || bEdgetoMark) && bspecin) //CPK 계산은 Inspection만 들어와야함.. dispcnt로는 이제 구분할수 없다.
            {
                double tor = Menu.frmSetting.revData.mBendingPre.SCFTolerance;
                double[] dCPK = new double[dgvSpec.Columns.Count];
                if (bspecin)
                {
                    for (int i = 0; i < dgvSpec.Columns.Count; i++)
                    {

                        ResultDistInsp[i][CalculCnt] = Convert.ToDouble(dgv[i, 0].Value);
                        if (i == 0 || i == 2)
                            dCPK[i] = AutoMainLogDatacalculator(ResultDistInsp[i], Convert.ToDouble(dgvSpec[i, 1].Value), Xspec);
                        else if (i == 1 || i == 3)
                            dCPK[i] = AutoMainLogDatacalculator(ResultDistInsp[i], Convert.ToDouble(dgvSpec[i, 1].Value), Yspec);
                    }

                    //이러면 0~31까지니까 32개 계산
                    if (CalculCnt < 31)
                    {
                        CalculCnt++;
                    }
                    else
                    {
                        CalculCnt = 0;
                        bSendCPK = true;
                    }

                    if (dgvSpec.InvokeRequired)
                    {
                        dgvSpec.Invoke(new MethodInvoker(delegate
                        {
                            for (int i = 0; i < dCPK.Length; i++)
                            {
                                dgvSpec[i, 3].Value = dCPK[i].ToString("0.000");
                            }
                        }));
                    }
                    else
                    {
                        for (int i = 0; i < dCPK.Length; i++)
                        {
                            dgvSpec[i, 3].Value = dCPK[i].ToString("0.000");
                        }
                    }
                }                
                if (bSendCPK && bCPK) IF.writeCPK(dCPK);
            }
        }

        public void camDisconnect()
        {
            for (int i = 0; i < cogFramGrabber.Length; i++)
            {
                try
                {
                    cogFramGrabber[i].Disconnect(false);
                }
                catch { }
            }
        }

        //csh 20170601
        private void ShowPLCStatus10(int nCam, bool bOn)
        {
            if (btnPLC10[nCam].InvokeRequired)
            {
                btnPLC10[nCam].Invoke(new MethodInvoker(delegate
                {
                    if (bOn)
                        btnPLC10[nCam].BackColor = Color.Green;
                    else
                        btnPLC10[nCam].BackColor = Color.Gray;
                }));
            }
            else
            {
                if (bOn)
                    btnPLC10[nCam].BackColor = Color.Green;
                else
                    btnPLC10[nCam].BackColor = Color.Gray;
            }
        }

        private void ShowPCStatus1(int nCam, int nStatus, int nStatus1 = 0)
        {
            if (btnPC1[nCam].InvokeRequired)
            {
                btnPC1[nCam].Invoke(new MethodInvoker(delegate
                {
                    if (nStatus == 0)
                        btnPC1[nCam].BackColor = Color.White;
                    else if (nStatus == 1)
                        btnPC1[nCam].BackColor = Color.Green;
                    else if (nStatus == 11)
                        btnPC1[nCam].BackColor = Color.Yellow;
                    else
                        btnPC1[nCam].BackColor = Color.Red;

                    btnPC1[nCam].Text = nStatus.ToString() + "," + nStatus1.ToString();
                }));
            }
            else
            {
                if (nStatus == 0)
                    btnPC1[nCam].BackColor = Color.White;
                else if (nStatus == 1)
                    btnPC1[nCam].BackColor = Color.Green;
                else if (nStatus == 11)
                    btnPC1[nCam].BackColor = Color.Yellow;
                else
                    btnPC1[nCam].BackColor = Color.Red;

                btnPC1[nCam].Text = nStatus.ToString() + "," + nStatus1.ToString();
            }
        }

        #region Bit 선언

        private const short pcAlive = 0;
        private const short pcAutoManual = 1;
        //const short plcAutoMode = 0x1C;

        #endregion Bit 선언

        private int aliveCnt = 0;
        //private int tmrcnt = 0;
        private string oldReply;

        private bool oldCheck;

        public static double[] SCFRefX1 = { 0, 0 };
        public static double[] SCFRefX2 = { 0, 0 };
        public static double[] SCFRefY1 = { 0, 0 };
        public static double[] SCFRefY2 = { 0, 0 };

        private bool[] SCFRetry = new bool[2];
        private bool[] psaRetry = new bool[2];
        public class dTimeOutClass
        {
            public bool checkStart;
            public short address;
            public short initCheck;
            public DateTime time;

        }

        public List<dTimeOutClass> dTimeOut = null;

        private class pcList
        {
            public Type constPCBit;
            public FieldInfo[] constPCBitControlFields;
            public CONST.iBitControl constPCBitControlInstance;
            // public short initCheck;

            public pcList(Type constPCBitControlType, CONST.iBitControl constPCBitControlInstance)
            {
                this.constPCBit = constPCBitControlType;
                this.constPCBitControlFields = constPCBitControlType.GetFields();
                //this.constPCBitControlFields = constPCBitControlFields;
                this.constPCBitControlInstance = constPCBitControlInstance;
            }
        }

        private class timeOut_initCheck
        {
            public Type bitControl;
            public int bitControlNo;

            public timeOut_initCheck(Type pc, int bitControlNo)
            {
                this.bitControl = pc;
                this.bitControlNo = bitControlNo;
            }
        }

        private Dictionary<timeOut_initCheck, short> initCheckValue = new Dictionary<timeOut_initCheck, short>();
        private enum eTimeOutType
        {
            Set,
            UnSet,
            Error,
            Process,
        }

        private void TimeOutCheck2(short reqID, eTimeOutType type)
        {
            if (type == eTimeOutType.Set)
            {
                this.lastRunTime[reqID] = new timeCheck(true, reqID, DateTime.Now);
            }
            else if (type == eTimeOutType.UnSet || type == eTimeOutType.Error)
            {
                this.lastRunTime[reqID] = null; // 없앰.
            }
            else //Process
            {
                if (this.lastRunTime[reqID] != null)
                {
                    if (this.lastRunTime[reqID].on)
                    {
                        // plc 가 on인데 pc 가 0이 아닌 경우는 반환한 경우로 정상임.

                        // 이건 정상 case.
                        if (CONST.bPLCReq[reqID] && pcResult[reqID] != 0)
                        {
                            TimeOutCheck2(reqID, eTimeOutType.UnSet);
                        }
                        else if (CONST.bPLCReq[reqID] && pcResult[reqID] == (int)ePCResult.VISION_REPLY_WAIT) // pattern 을 못 찾을 때 사용자에게 좌표 입력 화면을 띄운다. lyw.
                        {
                            if (CONST.m_bSystemLog)
                                cLog.Save(LogKind.System, "Pattern Find Fail, Type BitID No = " + reqID.ToString());
                        }
                        else
                        {
                            if (CONST.bPLCReq[reqID] && pcResult[reqID] == (int)ePCResult.WAIT)
                            {
                                timeCheck time = this.lastRunTime[reqID];
                                TimeSpan diffTime = DateTime.Now - time.time;

                                if (diffTime.TotalMilliseconds > 5000) //pc wait상태에서 5초가 넘어가면..
                                {
                                    // 해당 reqID 초기화.
                                    //pcResult[reqID] = 0;

                                    // 일단 log를 통해서 검증하고... 적용..
                                    if (CONST.m_bSystemLog)
                                        cLog.Save(LogKind.System, "Sequence Time Out, Type BitID No = " + reqID.ToString());

                                    TimeOutCheck2(reqID, eTimeOutType.UnSet);
                                }
                            } // if.
                        }
                    }
                }
            }
        }

        private int nTimerFlag = 0;
        private void AutoMainProcess()
        {
            while (!threadAutoEnd)
            {
                tmrIF_Tick(this, null);

                Thread.Sleep(1);
            }
        }

        private void SetLabel(Label label, string value)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new MethodInvoker(delegate
                {
                    label.Text = value;
                }));
            }
            else
                label.Text = value;
        }
        private void SetLabelColor(Label label, Color color)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new MethodInvoker(delegate
                {
                    label.ForeColor = color;
                }));
            }
            else
                label.ForeColor = color;
        }

        private DateTime dOldToday = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
        private Stopwatch swInterface = new Stopwatch();
        private bool oldplcAutoMode = false;
        bool bCalcCPK = false;
        private void tmrIF_Tick(object sender, EventArgs e)
        {
            //엄청빨리 도는 함수(타이머 아님)
            nTimerFlag++;
            if (nTimerFlag > 4)
            {
                nTimerFlag = 0;
                for (int i = 0; i < Vision.Length; i++)
                {
                    if (Vision[i].GetVisionSearchFlag())
                    {
                        Vision[i].ResetVisionSearchFlag();
                        try
                        {
                            if ((i == 4 && CONST.PCNo == 2) || (i == 6 && CONST.PCNo == 1))
                            {
                                SetLabel(lblRightScore2, "R:" + Vision[i].GetSearchScore((int)ePatternKind.Right_1cam).ToString("0.00"));
                                SetLabel(lblRightScore1, "R:" + Vision[i].GetSearchScore((int)ePatternKind.Right_1cam).ToString("0.00"));
                                SetLabel(lbScore[i], "L:" + Vision[i].GetSearchScore((int)ePatternKind.Left_1cam).ToString("0.00"));
                            }
                            else
                            {
                                SetLabel(lbScore[i], Vision[i].GetSearchScore((int)ePatternKind.Panel).ToString("0.00"));
                            }

                            if (Vision[i].GetVisionSearchResult() == 1)
                            {
                                SetLabel(lbSearchResult[i], "OK");
                                SetLabelColor(lbSearchResult[i], Color.Cyan); //Color.GreenYellow);
                            }
                            else if (Vision[i].GetVisionSearchResult() == 2)
                            {
                                //190430
                                //Vision[i].ImageSave(false, Vision[i].CFG.ImageSaveType);

                                SetLabel(lbSearchResult[i], "NG");
                                SetLabelColor(lbSearchResult[i], Color.Crimson);
                            }
                            else
                            {
                                SetLabel(lbSearchResult[i], "");
                            }
                        }
                        catch (Exception ex)
                        {
                            string exstring = ex.Message;
                        }
                    }
                    if (Vision[i].bimageSaveFlag) //pcy190724
                    {
                        Vision[i].bimageSaveFlag = false;
                        try
                        {
                            //Vision[i].JpegFastImageSave();
                        }
                        catch { }
                    }
                }
                if (bCalcCPK)
                {
                    //bdpre cpk 계산.
                    bCalcCPK = false;
                    CalcDgvCPK(dgvResult1, dgvSpec1);
                }


            }
            //pcy190604 하루한번 datetimepicker변경
            //okng카운트초기화 추가
            if (aliveCnt > 5000)
            {
                if (DateTime.Now.Day != dOldToday.Day)
                {
                    dOldToday = DateTime.Now;
                    if (dtpStart.InvokeRequired)
                    {
                        dtpStart.Invoke(new MethodInvoker(delegate
                        {
                            dtpStart.Value = dOldToday;
                        }));
                    }
                    else
                        dtpStart.Value = dOldToday;

                    if (dtpEnd.InvokeRequired)
                    {
                        dtpEnd.Invoke(new MethodInvoker(delegate
                        {
                            dtpEnd.Value = dOldToday;
                        }));
                    }
                    else
                        dtpEnd.Value = dOldToday;

                    if (dtpokngStart.InvokeRequired)
                    {
                        dtpokngStart.Invoke(new MethodInvoker(delegate
                        {
                            dtpokngStart.Value = dOldToday;
                        }));
                    }
                    else
                        dtpokngStart.Value = dOldToday;

                    if (dtpokngEnd.InvokeRequired)
                    {
                        dtpokngEnd.Invoke(new MethodInvoker(delegate
                        {
                            dtpokngEnd.Value = dOldToday;
                        }));
                    }
                    else
                        dtpokngEnd.Value = dOldToday;

                    foreach (var s in tlist)
                    {
                        for (int i = 0; i < s.count.Length; i++)
                        {
                            s.count[i] = 0;
                        }
                    }
                    InitialDispChart();
                    drawChartNoDist();
                }
            }

            #region PLC Request 처리
            if (aliveCnt > 5000)
            {
                bPCRep[pcAlive] = !bPCRep[pcAlive];
                aliveCnt = 0;
            }
            else
            {
                aliveCnt++;
            }

            if (!CONST.m_bAutoStart && bPCRep[pcAutoManual])
            {
                bPCRep[pcAutoManual] = false;
            }
            else if (CONST.m_bAutoStart && !bPCRep[pcAutoManual])
            {
                bPCRep[pcAutoManual] = true;
            }
            #region PC IF
            if (CONST.m_bAutoStart)
            {
                switch (CONST.PCNo)
                {
                    case 1:
                        LoadingPre(plcRequest.LoadingPre1Align, pcReply.LoadingPre1Align, eCalPos.LoadingPre1_1, Address.VisionOffset.LoadingPre1, -90, true, Vision_No.LoadingPre1, Vision_No.LoadingPre2);
                        LoadingPre(plcRequest.LoadingPre2Align, pcReply.LoadingPre2Align, eCalPos.LoadingPre2_1, Address.VisionOffset.LoadingPre2, -90, true, Vision_No.LoadingPre1, Vision_No.LoadingPre2);

                        LaserAlign(plcRequest.Laser1Align, pcReply.Laser1Align, eCalPos.Laser1, Address.VisionOffset.LaserAlign1, Vision_No.Laser1);
                        LaserAlign(plcRequest.Laser2Align, pcReply.Laser2Align, eCalPos.Laser2, Address.VisionOffset.LaserAlign2, Vision_No.Laser2);
                        // 210119 cjm 레이저 로그
                        LaserCellIDLog(plcRequest.Laser1CellLog, pcReply.Laser1CellLog, Vision_No.Laser1);
                        LaserCellIDLog(plcRequest.Laser2CellLog, pcReply.Laser2CellLog, Vision_No.Laser2);

                        LaserPositionInsp(plcRequest.Laser1Inspection, pcReply.Laser1Inspection, eCalPos.Laser1, pcReply.MarkingDataCompare1, Vision_No.Laser1);
                        LaserPositionInsp(plcRequest.Laser2Inspection, pcReply.Laser2Inspection, eCalPos.Laser2, pcReply.MarkingDataCompare2, Vision_No.Laser2);

                        MCRRead(plcRequest.MCRRead1, pcReply.MCRRead1, Vision_No.Laser1);
                        MCRRead(plcRequest.MCRRead2, pcReply.MCRRead2, Vision_No.Laser2);
                        break;
                }
            }

            if (oldplcAutoMode != CONST.plcAutomode)
            {
                oldplcAutoMode = CONST.plcAutomode;
            }

            PPIDChange();
            TimeChange();

            #endregion PC IF

            #endregion PLC Request 처리

            if (!CONST.RunMode)
            {
                #region Calibration Request 처리

                string sStatus = "";
                for (int i = 0; i < CONST.bPCCalReq.Length; i++)
                {
                    sStatus = Convert.ToInt32(CONST.bPCCalReq[i]) + sStatus;
                }

                if (sStatus != sCalstring)
                {
                    sCalstring = sStatus;
                    IF.setCalReq(sCalstring);
                }

                #endregion Calibration Request 처리
            }

            #region 궤적 전송

            if (CONST.plcTrace1Write)
            {
                CONST.plcTrace1Write = false;
                IF.writeTraceData(0);
            }

            if (CONST.plcTrace2Write)
            {
                CONST.plcTrace2Write = false;
                IF.writeTraceData(1);
            }

            if (CONST.plcTrace3Write)
            {
                CONST.plcTrace3Write = false;
                IF.writeTraceData(2);
            }

            #endregion

            if (oldCheck != Menu.frmRecipe.tmrCal.Enabled)
            {
                oldCheck = Menu.frmRecipe.tmrCal.Enabled;
                if (oldCheck) IF.setCalRepStart();
                else IF.setCalRepStop();
            }
        }

        public void ThreadDispose()
        {
            if (this.autoMainThread != null)
            {
                this.threadAutoEnd = true;

                while (autoMainThread.IsAlive)
                {
                    Application.DoEvents();
                    Thread.Sleep(1);
                }

                this.autoMainThread.Join();
            }

            if (mainThread != null)
            {
                this.threadEnd = true;

                while (mainThread.IsAlive)
                {
                    Application.DoEvents();
                    Thread.Sleep(1);
                }

                this.mainThread.Join();
            }
        }

        private bool firstCheck = false;

        // idle.
        private void Idle()
        {
            while (!threadEnd)
            {
                string sReply = "";
                CONST.bPCRep = bPCRep;

                for (int i = 0; i < CONST.bPCRep.Length; i++)
                {
                    sReply = Convert.ToInt32(CONST.bPCRep[i]) + sReply;
                }
                int[] iData = new int[2];
                if (oldReply != sReply)
                {
                    oldReply = sReply;
                    uint ResultValue = 0;
                    ResultValue = Convert.ToUInt32(sReply, 2);

                    iData[0] = (int)(Convert.ToInt64(ResultValue) & 0xFFFF);
                    iData[1] = (int)((Convert.ToInt64(ResultValue) & 0xFFFF0000) / 0x10000);

                    IF.setResult(CONST.Address.PC.BITCONTROL, 2, iData[0] + "^" + iData[1]);
                }

                for (int i = 0; i < pcResult.Length; i++)
                {
                    if (pcResult[i] != pcOldResult[i] || !firstCheck)
                    {
                        pcOldResult[i] = pcResult[i];
                        IF.setResult(CONST.Address.PC.REPLY + i, 1, Convert.ToString(pcResult[i]));
                        //                      cLog.Save(csLog.LogKind.System, "End");
                    }
                }

                firstCheck = true;
                //Application.DoEvents();
                Thread.Sleep(1);
            }
        }

        private string sCalstring = "";

        private string[] firstLogBD = new string[8];
        private string[] lastLogBD = new string[8];
        public int eCamNametoNo(string ecamname)
        {
            //eCamName를 넣으면 camno를 리턴받는 함수
            int nNo = -1;
            switch (ecamname)
            {
                case nameof(eCamNO.LoadingPre1):
                    nNo = (int)eCamNO.LoadingPre1;
                    break;
                case nameof(eCamNO.LoadingPre2):
                    nNo = (int)eCamNO.LoadingPre2;
                    break;
                case nameof(eCamNO.Laser1):
                    nNo = (int)eCamNO.Laser1;
                    break;
                case nameof(eCamNO.Laser2):
                    nNo = (int)eCamNO.Laser2;
                    break;
            }
            return nNo;
        }
        public eCalPos GetCalPos(ref eCalPos calpos, int VisionNo, int Kind = 0, bool bCam1Cal2 = false)
        {
            //visionno를 넣으면 calpos를 리턴받는 함수
            calpos = eCalPos.Err;
            if (CONST.PCNo == 1)
            {
                switch (VisionNo)
                {
                    case Vision_No.LoadingPre1:
                        if (Kind == 0) calpos = eCalPos.LoadingPre1_1;
                        else calpos = eCalPos.LoadingPre2_1;
                        break;
                    case Vision_No.LoadingPre2:
                        if (Kind == 0) calpos = eCalPos.LoadingPre1_2;
                        else calpos = eCalPos.LoadingPre2_2;
                        break;

                    case Vision_No.Laser1:
                        calpos = eCalPos.Laser1;
                        break;
                    case Vision_No.Laser2:
                        calpos = eCalPos.Laser2;
                        break;
                }
            }
            return calpos;
        }
        public void setVision(ref csVision org, int targetNo)
        {
            org = Vision[targetNo];
        }

        public void InitialDisp()
        {
            for (int i = 0; i < Vision.Length; i++)
            {
                if (Vision[i].CFG.Use)
                {
                    Vision[i].DispChange(cogDS[i]);
                    //if (Vision[i].CFG.SideVision) Vision[i].SideLive(true);
                }
            }
            cbResult.Checked = true;
        }
        public int[,] ISLight = new int[10, 10]; //넉넉히 선언 포트넘버,채널
        public bool SetLight(int PortNO, int CH, int Value, int CamNo, CONST.eLightType type = CONST.eLightType.Light12V)
        {
            if (CH == 0) return false;
            //현재값과 다를때만 변경
            if (ISLight[PortNO, CH] != Value)
            {
                //12V를 많이쓰니까 디폴트로 12V를 사용
                switch (type)
                {
                    case CONST.eLightType.Light5V:
                        Light5VSet(PortNO, CH, Value, CamNo);
                        break;
                    case CONST.eLightType.Light12V:
                    default:
                        Light12VSet(PortNO, CH, Value, CamNo);
                        break;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public void Light12VSet(int PortNO, int CH, int Value, int CamNo = 0)
        {
            string Mode = "L";
            byte b12CR = 0x0D;
            byte b12LF = 0x0A;

            ASCIIEncoding ACSII = new ASCIIEncoding();

            if (Value > 255) Value = 255;
            if (Value < 0) Value = 0;

            char[] ValData = Value.ToString().PadLeft(3, '0').ToCharArray();
            char cCH = char.Parse(CH.ToString());
            byte[] sendData = new byte[6];

            sendData[0] = Convert.ToByte(cCH);
            sendData[1] = Convert.ToByte(ValData[0]);
            sendData[2] = Convert.ToByte(ValData[1]);
            sendData[3] = Convert.ToByte(ValData[2]);
            sendData[4] = b12CR;
            sendData[5] = b12LF;

            string Data = Mode + ACSII.GetString(sendData);

            if (spLight1.IsOpen && spLight1.PortName.Substring(3, 1) == PortNO.ToString()) spLight1.Write(Data);
            else if (spLight2.IsOpen && spLight2.PortName.Substring(3, 1) == PortNO.ToString()) spLight2.Write(Data);
            else if (spLight3.IsOpen && spLight3.PortName.Substring(3, 1) == PortNO.ToString()) spLight3.Write(Data);

            ISLight[PortNO, CH] = Value;
        }

        private byte bStart = 0x3A;
        //csh 20170620  byte bNumber = 0x30;
        private byte bNumber = 0x31;
        private byte bComma = 0x2C;
        private byte bCR = 0x0D;
        private byte bLF = 0x0A;

        public void Light5VSet(int PortNO, int CH, int iValue, int CamNo = 0, int iValue2 = 0)
        {
            //value2 뭔지 확인필요
            char[] valData = iValue.ToString().PadLeft(4, '0').ToCharArray();
            char cCH = char.Parse(CH.ToString());
            byte[] sendData = null;
            if (iValue2 == 0)
            {
                sendData = new byte[9];
                sendData[0] = bStart;
                sendData[1] = bNumber;
                sendData[2] = Convert.ToByte(cCH);
                sendData[3] = Convert.ToByte(valData[0]);
                sendData[4] = Convert.ToByte(valData[1]);
                sendData[5] = Convert.ToByte(valData[2]);
                sendData[6] = Convert.ToByte(valData[3]);
                sendData[7] = bCR;
                sendData[8] = bLF;
            }
            else
            {
                char[] valData2 = iValue2.ToString().PadLeft(4, '0').ToCharArray();
                sendData = new byte[14];
                sendData[0] = bStart;
                sendData[1] = bNumber;
                sendData[2] = Convert.ToByte(cCH);
                sendData[3] = Convert.ToByte(valData[0]);
                sendData[4] = Convert.ToByte(valData[1]);
                sendData[5] = Convert.ToByte(valData[2]);
                sendData[6] = Convert.ToByte(valData[3]);
                sendData[7] = bComma;
                sendData[8] = Convert.ToByte(valData2[0]);
                sendData[9] = Convert.ToByte(valData2[1]);
                sendData[10] = Convert.ToByte(valData2[2]);
                sendData[11] = Convert.ToByte(valData2[3]);
                sendData[12] = bCR;
                sendData[13] = bLF;
            }

            if (iValue2 == 0)
            {
                if (spLight1.IsOpen && spLight1.PortName.Substring(3, 1) == PortNO.ToString()) spLight1.Write(sendData, 0, sendData.Length);
                else if (spLight2.IsOpen && spLight2.PortName.Substring(3, 1) == PortNO.ToString()) spLight2.Write(sendData, 0, sendData.Length);
                else if (spLight3.IsOpen && spLight3.PortName.Substring(3, 1) == PortNO.ToString()) spLight3.Write(sendData, 0, sendData.Length);
                ISLight[PortNO, CH] = iValue;
            }
        }
        public void LogDispError1(int CamNo, string cellID, string ErrorList)
        {
            int index = Convert.ToInt32(Math.Truncate(Convert.ToDouble(CamNo) / 2));
            index = SelectResultBox(CamNo);
            if (dgvError[index].InvokeRequired)
            {
                dgvError[index].Invoke(new MethodInvoker(delegate
                {
                    dgvError[index].Rows.Insert(0);
                    dgvError[index][0, 0].Value = cellID;
                    dgvError[index][1, 0].Value = ErrorList;
                    if (dgvError[index].Rows.Count > 31)
                    {
                        int nCount = 0;
                        nCount = dgvError[index].Rows.Count - 31;
                        for (int i = 0; i < nCount; i++)
                        {
                            dgvError[index].Rows.RemoveAt(30);
                        }
                    }
                }));
            }
            else
            {
                dgvError[index].Rows.Insert(0);
                dgvError[index][0, 0].Value = cellID;
                dgvError[index][1, 0].Value = ErrorList;
                if (dgvError[index].Rows.Count > 31)
                {
                    int nCount = 0;
                    nCount = dgvError[index].Rows.Count - 31;
                    for (int i = 0; i < nCount; i++)
                    {
                        dgvError[index].Rows.RemoveAt(30);
                    }
                }
            }
        }
        public void LogDisp(int CamNo, string sLog, bool bSave = true)
        {
            string str = DateTime.Now.ToString("HH:mm:ss") + "." + DateTime.Now.Millisecond.ToString("000") + " - " + sLog;
            int Num = SelectResultBox(CamNo);
            if (lbList[Num].InvokeRequired)
            {
                lbList[Num].Invoke(new MethodInvoker(delegate
                {
                    if (lbList[Num].Items.Count > 50)
                    {
                        lbList[Num].Items.RemoveAt(lbList[Num].Items.Count - 1);
                    }
                    lbList[Num].Items.Insert(0, str);
                }));
            }
            else
            {
                if (lbList[Num].Items.Count > 50)
                {
                    lbList[Num].Items.RemoveAt(lbList[Num].Items.Count - 1);
                }
                lbList[Num].Items.Insert(0, str);
            }
        }
        //First -> Last Log
        private void drawDist(long start, long end, int CamNo, double dsquareX = 0, double dsquareY = 0)
        {
            try
            {

                GetChartDispFromVisionNo(CamNo, out Chart Chart1, out Chart Chart2, out Label lblNG1, out Label lblNG2, out Label[] SpecText);
                GetCamNameFromVisionNo(CamNo, out string CamName);

                Chart1.Series.Clear();
                Chart1.Series.Add("LX/LY");
                Chart1.Series.Add("SpecIn");
                Chart1.Series.Add("SpecLine");
                Chart1.Series[1].Color = Color.Blue;
                if (CamName == "Insp")
                {
                    Chart1.Series.Add(" ");
                }

                Chart2.Series.Clear();
                Chart2.Series.Add("RX/RY");
                Chart2.Series.Add("SpecIn");
                Chart2.Series.Add("SpecLine");
                Chart2.Series[1].Color = Color.Blue;
                if (CamName == "Insp")
                {
                    Chart2.Series.Add(" ");
                }

                int ispecout1 = 0;
                int ispecout2 = 0;
                int isquareout1 = 0;
                int isquareout2 = 0;
                int ispecin1 = 0;
                int ispecin2 = 0;

                double OffsetY1 = 0;
                double OffsetY2 = 0;
                double FirstY1Target = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LY];// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y1;
                double FirstY2Target = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RY];// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y2;
                double FirstX1Spec = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LX];
                double FirstX2Spec = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RX];
                if (Menu.frmSetting.revData.mBendingArm.bDisplayLRSwapBD)
                {
                    FirstY1Target = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RY];// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y2;
                    FirstY2Target = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LY];// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y1;
                    FirstX1Spec = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RX];
                    FirstX2Spec = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LX];
                }
                string[] sLog = null;
                cLog.ReadDistLog(ref sLog, start, end, Vision[CamNo].CFG.Name);
                if (sLog != null) CONST.DispChart.Count[CamNo] = sLog.Length;
                double dYspec = 0;
                double dXspec = 0;

                double ChartX1_1 = 0;
                double ChartY1_1 = 0;
                double ChartX1_2 = 0;
                double ChartY1_2 = 0;

                double ChartX2_1 = 0;
                double ChartY2_1 = 0;
                double ChartX2_2 = 0;
                double ChartY2_2 = 0;
                double chartlengthX = 0;
                double chartlengthY = 0;
                switch (CamName)
                {
                    case "Bend1":
                    case "Bend2":
                    case "Bend3":
                        dXspec = Menu.frmSetting.revData.mBendingArm.BDToleranceX;
                        dYspec = Menu.frmSetting.revData.mBendingArm.BDToleranceY;
                        chartlengthX = dXspec * 1.25;
                        chartlengthY = dYspec * 1.25;
                        break;
                    case "Insp":
                        FirstX1Spec = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LX];
                        FirstX2Spec = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RX];
                        FirstY1Target = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LY];
                        FirstY2Target = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RY];
                        if (Menu.frmSetting.revData.mBendingArm.bDisplayLRSwapINSP)
                        {
                            FirstX1Spec = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RX];
                            FirstX2Spec = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LX];
                            FirstY1Target = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RY];
                            FirstY2Target = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LY];
                        }

                        dXspec = Menu.frmSetting.revData.mBendingArm.InspToleranceX;
                        dYspec = Menu.frmSetting.revData.mBendingArm.InspToleranceY;
                        chartlengthX = dXspec + 0.2;
                        chartlengthY = dYspec + 0.2;
                        break;
                    case "Attach":
                        if (CONST.PCNo == 1)
                        {
                            FirstX1Spec = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_LX];
                            FirstX2Spec = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_RX];
                            FirstY1Target = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_LY];
                            FirstY2Target = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_RY];
                        }
                        else
                        {
                            FirstX1Spec = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_LX];
                            FirstX2Spec = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_RX];
                            FirstY1Target = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_LY];
                            FirstY2Target = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_RY];
                        }
                        dXspec = Menu.frmSetting.revData.mSizeSpecRatio.AttachToleranceX;
                        dYspec = Menu.frmSetting.revData.mSizeSpecRatio.AttachToleranceY;
                        chartlengthX = dXspec * 1.5;
                        chartlengthY = dYspec * 1.5;
                        break;

                    case nameof(Vision_No.Laser1):
                    case nameof(Vision_No.Laser2):
                        SetLaserInspectionSpec(out sSpec specX, out sSpec specY);

                        FirstX1Spec = specX.Middle1;
                        FirstX2Spec = specX.Middle2;
                        FirstY1Target = specY.Middle1;
                        FirstY2Target = specY.Middle2;

                        dXspec = specX.Spec;
                        dYspec = specY.Spec;
                        chartlengthX = dXspec * 1.5;
                        chartlengthY = dYspec * 1.5;
                        break;
                }

                ChartX1_1 = FirstX1Spec + chartlengthX;
                ChartY1_1 = FirstY1Target + chartlengthY;
                ChartX1_2 = FirstX1Spec - chartlengthX;
                ChartY1_2 = FirstY1Target - chartlengthY;

                ChartX2_1 = FirstX2Spec + chartlengthX;
                ChartY2_1 = FirstY2Target + chartlengthY;
                ChartX2_2 = FirstX2Spec - chartlengthX;
                ChartY2_2 = FirstY2Target - chartlengthY;


                SpecText[0].Text = FirstX1Spec.ToString("0.000");
                SpecText[1].Text = FirstY1Target.ToString("0.000");
                SpecText[2].Text = FirstX2Spec.ToString("0.000");
                SpecText[3].Text = FirstY2Target.ToString("0.000");

                //LX, LY 스펙 인 에어리어 박스 생성
                Chart1.Series[1].Points.AddXY(FirstX1Spec + dXspec, FirstY1Target + dYspec);
                Chart1.Series[1].Points.AddXY(FirstX1Spec + dXspec, FirstY1Target - dYspec);
                Chart1.Series[1].Points.AddXY(FirstX1Spec - dXspec, FirstY1Target - dYspec);
                Chart1.Series[1].Points.AddXY(FirstX1Spec - dXspec, FirstY1Target + dYspec);
                Chart1.Series[1].Points.AddXY(FirstX1Spec - dXspec, FirstY1Target + dYspec);
                Chart1.Series[1].Points.AddXY(FirstX1Spec + dXspec, FirstY1Target + dYspec);
                Chart1.Series[1].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                Chart1.Series[1].BorderWidth = 1;
                //LX, LY 스펙 Line 생성
                Chart1.Series[2].Points.AddXY(ChartX1_1, FirstY1Target);
                Chart1.Series[2].Points.AddXY(ChartX1_2, FirstY1Target);
                Chart1.Series[2].Points.AddXY(FirstX1Spec, FirstY1Target);
                Chart1.Series[2].Points.AddXY(FirstX1Spec, ChartY1_1);
                Chart1.Series[2].Points.AddXY(FirstX1Spec, ChartY1_2);
                Chart1.Series[2].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                Chart1.Series[2].BorderWidth = 1;

                //RX, RY 스펙 인 에어리어 박스 생성
                Chart2.Series[1].Points.AddXY(FirstX2Spec + dXspec, FirstY2Target + dYspec);
                Chart2.Series[1].Points.AddXY(FirstX2Spec + dXspec, FirstY2Target - dYspec);
                Chart2.Series[1].Points.AddXY(FirstX2Spec - dXspec, FirstY2Target - dYspec);
                Chart2.Series[1].Points.AddXY(FirstX2Spec - dXspec, FirstY2Target + dYspec);
                Chart2.Series[1].Points.AddXY(FirstX2Spec - dXspec, FirstY2Target + dYspec);
                Chart2.Series[1].Points.AddXY(FirstX2Spec + dXspec, FirstY2Target + dYspec);
                Chart2.Series[1].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                Chart2.Series[1].BorderWidth = 1;
                //RX, RY 스펙 인 에어리어 박스 생성
                Chart2.Series[2].Points.AddXY(ChartX2_1, FirstY2Target);
                Chart2.Series[2].Points.AddXY(ChartX2_2, FirstY2Target);
                Chart2.Series[2].Points.AddXY(FirstX2Spec, FirstY2Target);
                Chart2.Series[2].Points.AddXY(FirstX2Spec, ChartY2_1);
                Chart2.Series[2].Points.AddXY(FirstX2Spec, ChartY2_2);
                Chart2.Series[2].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                Chart2.Series[2].BorderWidth = 1;

                #region 추가스펙 박스생성

                if (CamName == "Insp")
                {
                    //LX, LY 스펙 인 에어리어 박스 생성
                    Chart1.Series[3].Points.AddXY(FirstX1Spec + dsquareX, FirstY1Target + dsquareY);
                    Chart1.Series[3].Points.AddXY(FirstX1Spec + dsquareX, FirstY1Target - dsquareY);
                    Chart1.Series[3].Points.AddXY(FirstX1Spec - dsquareX, FirstY1Target - dsquareY);
                    Chart1.Series[3].Points.AddXY(FirstX1Spec - dsquareX, FirstY1Target + dsquareY);
                    Chart1.Series[3].Points.AddXY(FirstX1Spec - dsquareX, FirstY1Target + dsquareY);
                    Chart1.Series[3].Points.AddXY(FirstX1Spec + dsquareX, FirstY1Target + dsquareY);
                    Chart1.Series[3].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    Chart1.Series[3].BorderWidth = 1;
                    //LX, LY 스펙 Line 생성
                    //Chart1.Series[2].Points.AddXY(ChartX1_1, FirstY1Target);
                    //Chart1.Series[2].Points.AddXY(ChartX1_2, FirstY1Target);
                    //Chart1.Series[2].Points.AddXY(FirstX1Spec, FirstY1Target);
                    //Chart1.Series[2].Points.AddXY(FirstX1Spec, ChartY1_1);
                    //Chart1.Series[2].Points.AddXY(FirstX1Spec, ChartY1_2);
                    //Chart1.Series[2].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    //Chart1.Series[2].BorderWidth = 1;

                    //RX, RY 스펙 인 에어리어 박스 생성
                    Chart2.Series[3].Points.AddXY(FirstX2Spec + dsquareX, FirstY2Target + dsquareY);
                    Chart2.Series[3].Points.AddXY(FirstX2Spec + dsquareX, FirstY2Target - dsquareY);
                    Chart2.Series[3].Points.AddXY(FirstX2Spec - dsquareX, FirstY2Target - dsquareY);
                    Chart2.Series[3].Points.AddXY(FirstX2Spec - dsquareX, FirstY2Target + dsquareY);
                    Chart2.Series[3].Points.AddXY(FirstX2Spec - dsquareX, FirstY2Target + dsquareY);
                    Chart2.Series[3].Points.AddXY(FirstX2Spec + dsquareX, FirstY2Target + dsquareY);
                    Chart2.Series[3].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    Chart2.Series[3].BorderWidth = 1;
                    //RX, RY 스펙 인 에어리어 박스 생성
                    //Chart2.Series[2].Points.AddXY(ChartX2_1, FirstY2Target);
                    //Chart2.Series[2].Points.AddXY(ChartX2_2, FirstY2Target);
                    //Chart2.Series[2].Points.AddXY(FirstX2Spec, FirstY2Target);
                    //Chart2.Series[2].Points.AddXY(FirstX2Spec, ChartY2_1);
                    //Chart2.Series[2].Points.AddXY(FirstX2Spec, ChartY2_2);
                    //Chart2.Series[2].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    //Chart2.Series[2].BorderWidth = 1;
                }

                #endregion 추가스펙 박스생성

                bool specoutcheck = false;
                if (dsquareX == 0) dsquareX = dXspec;
                if (dsquareY == 0) dsquareY = dYspec;

                for (int i = 0; i < sLog.Length; i++)
                {
                    try
                    {
                        double dX1 = 0;
                        double dY1 = 0;
                        double dX2 = 0;
                        double dY2 = 0;
                        double dRetryCnt = 0;
                        string[] sData = sLog[i].Split(',');
                        specoutcheck = false;

                        if (CamName == "Insp" && !rbGraphTotal.Checked)
                        {
                            if (rbGraphBD1.Checked)
                            {
                                if (sData[0] == "1")
                                {
                                    dX1 = Math.Abs(double.Parse(sData[1]));
                                    dY1 = Math.Abs(double.Parse(sData[2]));
                                    dX2 = Math.Abs(double.Parse(sData[3]));
                                    dY2 = Math.Abs(double.Parse(sData[4]));
                                    if (sData.Length == 6) dRetryCnt = double.Parse(sData[5]);
                                    specoutcheck = true;
                                }
                            }
                            else if (rbGraphBD2.Checked)
                            {
                                if (sData[0] == "2")
                                {
                                    dX1 = Math.Abs(double.Parse(sData[1]));
                                    dY1 = Math.Abs(double.Parse(sData[2]));
                                    dX2 = Math.Abs(double.Parse(sData[3]));
                                    dY2 = Math.Abs(double.Parse(sData[4]));
                                    if (sData.Length == 6) dRetryCnt = double.Parse(sData[5]);
                                    specoutcheck = true;
                                }
                            }
                            else if (rbGraphBD3.Checked)
                            {
                                if (sData[0] == "3")
                                {
                                    dX1 = Math.Abs(double.Parse(sData[1]));
                                    dY1 = Math.Abs(double.Parse(sData[2]));
                                    dX2 = Math.Abs(double.Parse(sData[3]));
                                    dY2 = Math.Abs(double.Parse(sData[4]));
                                    if (sData.Length == 6) dRetryCnt = double.Parse(sData[5]);
                                    specoutcheck = true;
                                }
                            }
                        }
                        else
                        {
                            dX1 = Math.Abs(double.Parse(sData[1]));
                            dY1 = Math.Abs(double.Parse(sData[2]));
                            dX2 = Math.Abs(double.Parse(sData[3]));
                            dY2 = Math.Abs(double.Parse(sData[4]));
                            if (sData.Length == 6) dRetryCnt = double.Parse(sData[5]);
                            specoutcheck = true;
                        }
                        Chart1.Series[0].Points.AddXY(dX1, dY1);
                        Chart2.Series[0].Points.AddXY(dX2, dY2);


                        //pcy190613 double로 계산하니 소수점 찌꺼기가 남아서 스펙에 딱 걸치는 점이 불량판정됨(ex FirstX1Spec + dsquareX가 14인데 14.00000001이런식..) round로 반올림하여 계산(테스트완료)
                        //retry9는 마지막 retry시 Specoffset으로 겨우 접은 것 따로 표시하기 위함.
                        if (dX1 > Math.Round(FirstX1Spec + dsquareX, 3) || dX1 < Math.Round(FirstX1Spec - dsquareX, 3)
                            || dY1 > Math.Round(FirstY1Target + dsquareY + OffsetY1, 3) || dY1 < Math.Round(FirstY1Target - dsquareY + OffsetY1, 3))
                        {
                            Chart1.Series[0].Points[i].Color = Color.Orange;
                            if (specoutcheck) isquareout1++;

                            if (dX1 > Math.Round(FirstX1Spec + dXspec, 3) || dX1 < Math.Round(FirstX1Spec - dXspec, 3)
                                || dY1 > Math.Round(FirstY1Target + dYspec + OffsetY1, 3) || dY1 < Math.Round(FirstY1Target - dYspec + OffsetY1, 3))
                            {
                                Chart1.Series[0].Points[i].Color = Color.Red;
                                if (specoutcheck) ispecout1++;
                            }
                            else
                            {
                                //square 아웃이지만 스펙인임.
                                if (specoutcheck) ispecin1++;
                            }

                            if (dRetryCnt == 9) Chart1.Series[0].Points[i].Color = Color.Black;
                        }
                        else
                        {
                            Chart1.Series[0].Points[i].Color = Color.DarkGreen;
                            if (specoutcheck) ispecin1++;
                            if (dRetryCnt == 9) Chart1.Series[0].Points[i].Color = Color.Black;
                        }

                        if (dX2 > Math.Round(FirstX2Spec + dsquareX, 3) || dX2 < Math.Round(FirstX2Spec - dsquareX, 3)
                                || dY2 > Math.Round(FirstY2Target + dsquareY + OffsetY2, 3) || dY2 < Math.Round(FirstY2Target - dsquareY + OffsetY2, 3))
                        {
                            Chart2.Series[0].Points[i].Color = Color.Orange;
                            if (specoutcheck) isquareout2++;

                            if (dX2 > Math.Round(FirstX2Spec + dXspec, 3) || dX2 < Math.Round(FirstX2Spec - dXspec, 3)
                                || dY2 > Math.Round(FirstY2Target + dYspec + OffsetY2, 3) || dY2 < Math.Round(FirstY2Target - dYspec + OffsetY2, 3))
                            {
                                Chart2.Series[0].Points[i].Color = Color.Red;
                                if (specoutcheck) ispecout2++;
                            }
                            else
                            {
                                if (specoutcheck) ispecin2++;
                            }
                            if (dRetryCnt == 9) Chart2.Series[0].Points[i].Color = Color.Black;
                        }
                        else
                        {
                            Chart2.Series[0].Points[i].Color = Color.DarkGreen;
                            if (specoutcheck) ispecin2++;
                            if (dRetryCnt == 9) Chart2.Series[0].Points[i].Color = Color.Black;
                        }
                    }
                    catch { }

                }
                Chart1.Series[0].BorderWidth = 1;
                Chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
                Chart1.ChartAreas[0].AxisX.Maximum = ChartX1_1;
                Chart1.ChartAreas[0].AxisX.Minimum = ChartX1_2;
                Chart1.ChartAreas[0].AxisY.Maximum = ChartY1_1;
                Chart1.ChartAreas[0].AxisY.Minimum = ChartY1_2;

                Chart2.Series[0].BorderWidth = 1;
                Chart2.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
                Chart2.ChartAreas[0].AxisX.Maximum = ChartX2_1;
                Chart2.ChartAreas[0].AxisX.Minimum = ChartX2_2;
                Chart2.ChartAreas[0].AxisY.Maximum = ChartY2_1;
                Chart2.ChartAreas[0].AxisY.Minimum = ChartY2_2;

                CONST.DispChart.CountIn1[CamNo] = ispecin1;
                CONST.DispChart.CountIn2[CamNo] = ispecin2;
                CONST.DispChart.CountOut1[CamNo] = ispecout1;
                CONST.DispChart.CountOut2[CamNo] = ispecout2;
                CONST.DispChart.CountSquareOut1[CamNo] = isquareout1;
                CONST.DispChart.CountSquareOut2[CamNo] = isquareout2;
                CONST.DispChart.SquareX[CamNo] = dsquareX;
                CONST.DispChart.SquareY[CamNo] = dsquareY;
                if (CamName == "Insp")
                {
                    lblNG1.Text = "OK: " + ispecin1.ToString() + "  NG: " + ispecout1.ToString() + "  SquareNG: " + isquareout1.ToString();
                    lblNG2.Text = "OK: " + ispecin2.ToString() + "  NG: " + ispecout2.ToString() + "  SquareNG: " + isquareout2.ToString();
                }
                else
                {
                    lblNG1.Text = "OK: " + ispecin1.ToString() + "  NG: " + ispecout1.ToString();
                    lblNG2.Text = "OK: " + ispecin2.ToString() + "  NG: " + ispecout2.ToString();
                }

            }
            catch { }

        }

        private void DrawPoint(int VisionNo)
        {
            GetChartDispFromVisionNo(VisionNo, out Chart Chart1, out Chart Chart2, out Label lblNG1, out Label lblNG2, out Label[] lblctSpec);

            int count = CONST.DispChart.Count[VisionNo];
            double dsquareX = CONST.DispChart.SquareX[VisionNo];
            double dsquareY = CONST.DispChart.SquareY[VisionNo];

            double OffsetY1 = 0;
            double OffsetY2 = 0;
            double FirstY1Target = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LY];// + Menu.frmSetting.revData.mOffset[VisionNo].LastOffsetYY.Y1;
            double FirstY2Target = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RY];// + Menu.frmSetting.revData.mOffset[VisionNo].LastOffsetYY.Y2;
            double FirstX1Spec = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LX];
            double FirstX2Spec = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RX];
            if (Menu.frmSetting.revData.mBendingArm.bDisplayLRSwapBD)
            {
                FirstY1Target = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RY];// + Menu.frmSetting.revData.mOffset[VisionNo].LastOffsetYY.Y2;
                FirstY2Target = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LY];// + Menu.frmSetting.revData.mOffset[VisionNo].LastOffsetYY.Y1;
                FirstX1Spec = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RX];
                FirstX2Spec = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LX];
            }

            double dYspec = 0;
            double dXspec = 0;

            double ChartX1_1 = 0;
            double ChartY1_1 = 0;
            double ChartX1_2 = 0;
            double ChartY1_2 = 0;

            double ChartX2_1 = 0;
            double ChartY2_1 = 0;
            double ChartX2_2 = 0;
            double ChartY2_2 = 0;
            double chartlengthX = 0;
            double chartlengthY = 0;

            SetLaserInspectionSpec(out sSpec specX, out sSpec specY);

            FirstX1Spec = specX.Middle1;
            FirstX2Spec = specX.Middle2;
            FirstY1Target = specY.Middle1;
            FirstY2Target = specY.Middle2;

            dXspec = specX.Spec;
            dYspec = specY.Spec;
            chartlengthX = dXspec * 1.5;
            chartlengthY = dYspec * 1.5;
            // break;
            //}

            ChartX1_1 = FirstX1Spec + chartlengthX;
            ChartY1_1 = FirstY1Target + chartlengthY;
            ChartX1_2 = FirstX1Spec - chartlengthX;
            ChartY1_2 = FirstY1Target - chartlengthY;

            ChartX2_1 = FirstX2Spec + chartlengthX;
            ChartY2_1 = FirstY2Target + chartlengthY;
            ChartX2_2 = FirstX2Spec - chartlengthX;
            ChartY2_2 = FirstY2Target - chartlengthY;

            if (dsquareX == 0) dsquareX = dXspec;
            if (dsquareY == 0) dsquareY = dYspec;

            //first point draw new chart
            if (count == 0)
            {
                Chart1.Series.Clear();
                Chart1.Series.Add("LX/LY");
                Chart1.Series.Add("SpecIn");
                Chart1.Series.Add("SpecLine");
                Chart1.Series[1].Color = Color.Blue;

                Chart2.Series.Clear();
                Chart2.Series.Add("RX/RY");
                Chart2.Series.Add("SpecIn");
                Chart2.Series.Add("SpecLine");
                Chart2.Series[1].Color = Color.Blue;

                Chart1.Series[1].Points.AddXY(FirstX1Spec + dXspec, FirstY1Target + dYspec);
                Chart1.Series[1].Points.AddXY(FirstX1Spec + dXspec, FirstY1Target - dYspec);
                Chart1.Series[1].Points.AddXY(FirstX1Spec - dXspec, FirstY1Target - dYspec);
                Chart1.Series[1].Points.AddXY(FirstX1Spec - dXspec, FirstY1Target + dYspec);
                Chart1.Series[1].Points.AddXY(FirstX1Spec - dXspec, FirstY1Target + dYspec);
                Chart1.Series[1].Points.AddXY(FirstX1Spec + dXspec, FirstY1Target + dYspec);
                Chart1.Series[1].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                Chart1.Series[1].BorderWidth = 1;
                //LX, LY 스펙 Line 생성
                Chart1.Series[2].Points.AddXY(ChartX1_1, FirstY1Target);
                Chart1.Series[2].Points.AddXY(ChartX1_2, FirstY1Target);
                Chart1.Series[2].Points.AddXY(FirstX1Spec, FirstY1Target);
                Chart1.Series[2].Points.AddXY(FirstX1Spec, ChartY1_1);
                Chart1.Series[2].Points.AddXY(FirstX1Spec, ChartY1_2);
                Chart1.Series[2].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                Chart1.Series[2].BorderWidth = 1;

                //RX, RY 스펙 인 에어리어 박스 생성
                Chart2.Series[1].Points.AddXY(FirstX2Spec + dXspec, FirstY2Target + dYspec);
                Chart2.Series[1].Points.AddXY(FirstX2Spec + dXspec, FirstY2Target - dYspec);
                Chart2.Series[1].Points.AddXY(FirstX2Spec - dXspec, FirstY2Target - dYspec);
                Chart2.Series[1].Points.AddXY(FirstX2Spec - dXspec, FirstY2Target + dYspec);
                Chart2.Series[1].Points.AddXY(FirstX2Spec - dXspec, FirstY2Target + dYspec);
                Chart2.Series[1].Points.AddXY(FirstX2Spec + dXspec, FirstY2Target + dYspec);
                Chart2.Series[1].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                Chart2.Series[1].BorderWidth = 1;
                //RX, RY 스펙 인 에어리어 박스 생성
                Chart2.Series[2].Points.AddXY(ChartX2_1, FirstY2Target);
                Chart2.Series[2].Points.AddXY(ChartX2_2, FirstY2Target);
                Chart2.Series[2].Points.AddXY(FirstX2Spec, FirstY2Target);
                Chart2.Series[2].Points.AddXY(FirstX2Spec, ChartY2_1);
                Chart2.Series[2].Points.AddXY(FirstX2Spec, ChartY2_2);
                Chart2.Series[2].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                Chart2.Series[2].BorderWidth = 1;


                lblctSpec[0].Text = FirstX1Spec.ToString("0.000");
                lblctSpec[1].Text = FirstY1Target.ToString("0.000");
                lblctSpec[2].Text = FirstX2Spec.ToString("0.000");
                lblctSpec[3].Text = FirstY2Target.ToString("0.000");

            }


            try
            {
                double dX1 = CONST.DispChart.dist[VisionNo].X1;
                double dY1 = CONST.DispChart.dist[VisionNo].Y1;
                double dX2 = CONST.DispChart.dist[VisionNo].X2;
                double dY2 = CONST.DispChart.dist[VisionNo].Y2;

                Chart1.Series[0].Points.AddXY(dX1, dY1);
                Chart2.Series[0].Points.AddXY(dX2, dY2);

                if (dX1 > Math.Round(FirstX1Spec + dsquareX, 3) || dX1 < Math.Round(FirstX1Spec - dsquareX, 3)
                    || dY1 > Math.Round(FirstY1Target + dsquareY + OffsetY1, 3) || dY1 < Math.Round(FirstY1Target - dsquareY + OffsetY1, 3))
                {
                    Chart1.Series[0].Points[count].Color = Color.Orange;
                    CONST.DispChart.CountSquareOut1[VisionNo]++;

                    if (dX1 > Math.Round(FirstX1Spec + dXspec, 3) || dX1 < Math.Round(FirstX1Spec - dXspec, 3)
                        || dY1 > Math.Round(FirstY1Target + dYspec + OffsetY1, 3) || dY1 < Math.Round(FirstY1Target - dYspec + OffsetY1, 3))
                    {
                        Chart1.Series[0].Points[count].Color = Color.Red;
                        CONST.DispChart.CountOut1[VisionNo]++;
                    }
                    else
                    {
                        CONST.DispChart.CountIn1[VisionNo]++;
                    }

                }
                else
                {
                    Chart1.Series[0].Points[count].Color = Color.DarkGreen;
                    CONST.DispChart.CountIn1[VisionNo]++;
                }

                if (dX2 > Math.Round(FirstX2Spec + dsquareX, 3) || dX2 < Math.Round(FirstX2Spec - dsquareX, 3)
                        || dY2 > Math.Round(FirstY2Target + dsquareY + OffsetY2, 3) || dY2 < Math.Round(FirstY2Target - dsquareY + OffsetY2, 3))
                {
                    Chart2.Series[0].Points[count].Color = Color.Orange;
                    CONST.DispChart.CountSquareOut2[VisionNo]++;

                    if (dX2 > Math.Round(FirstX2Spec + dXspec, 3) || dX2 < Math.Round(FirstX2Spec - dXspec, 3)
                        || dY2 > Math.Round(FirstY2Target + dYspec + OffsetY2, 3) || dY2 < Math.Round(FirstY2Target - dYspec + OffsetY2, 3))
                    {
                        Chart2.Series[0].Points[count].Color = Color.Red;
                        CONST.DispChart.CountOut2[VisionNo]++;
                    }
                    else
                    {
                        CONST.DispChart.CountIn2[VisionNo]++;
                    }
                }
                else
                {
                    Chart2.Series[0].Points[count].Color = Color.DarkGreen;
                    CONST.DispChart.CountIn2[VisionNo]++;
                }

            }
            catch { }

            Chart1.Series[0].BorderWidth = 1;
            Chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            Chart1.ChartAreas[0].AxisX.Maximum = ChartX1_1;
            Chart1.ChartAreas[0].AxisX.Minimum = ChartX1_2;
            Chart1.ChartAreas[0].AxisY.Maximum = ChartY1_1;
            Chart1.ChartAreas[0].AxisY.Minimum = ChartY1_2;

            Chart2.Series[0].BorderWidth = 1;
            Chart2.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            Chart2.ChartAreas[0].AxisX.Maximum = ChartX2_1;
            Chart2.ChartAreas[0].AxisX.Minimum = ChartX2_2;
            Chart2.ChartAreas[0].AxisY.Maximum = ChartY2_1;
            Chart2.ChartAreas[0].AxisY.Minimum = ChartY2_2;

            CONST.DispChart.Count[VisionNo]++;

            //if (camName == nameof(Vision_No.UpperInsp1_1))
            //{
            //    lblNG1.Text = "OK: " + CONST.DispChart.CountIn1[VisionNo].ToString() + "  NG: " + CONST.DispChart.CountOut1[VisionNo].ToString() + "  SquareNG: " + CONST.DispChart.CountSquareOut1[VisionNo].ToString();
            //    lblNG2.Text = "OK: " + CONST.DispChart.CountIn2[VisionNo].ToString() + "  NG: " + CONST.DispChart.CountOut2[VisionNo].ToString() + "  SquareNG: " + CONST.DispChart.CountSquareOut2[VisionNo].ToString();
            //}
            //else
            //{
            lblNG1.Text = "OK: " + CONST.DispChart.CountIn1[VisionNo].ToString() + "  NG: " + CONST.DispChart.CountOut1[VisionNo].ToString();
            lblNG2.Text = "OK: " + CONST.DispChart.CountIn2[VisionNo].ToString() + "  NG: " + CONST.DispChart.CountOut2[VisionNo].ToString();
            //}
        }

        private void GetChartDispFromVisionNo(int camNo, out Chart chart1, out Chart chart2, out Label lblNG1, out Label lblNG2, out Label[] lblctSpec)
        {
            chart1 = ctDisp1_1;
            chart2 = ctDisp1_2;
            lblNG1 = lblSpecOut1_1;
            lblNG2 = lblSpecOut1_2;
            lblctSpec = lblct1Spec;
            GetCamNameFromVisionNo(camNo, out string camName);
            switch (camName)
            {
                //case nameof(Vision_No.Attach1_1):
                //case nameof(Vision_No.Attach2_3):
                //    chart1 = ctDisp1_1;
                //    chart2 = ctDisp1_2;
                //    break;
                //case nameof(Vision_No.Bend1_1):
                //    chart1 = ctDisp1_1;
                //    chart2 = ctDisp1_2;
                //    break;
                //case nameof(Vision_No.UpperInsp1_1):
                //    chart1 = ctDisp1_1;
                //    chart2 = ctDisp1_2;
                //    break;
                case nameof(Vision_No.Laser1):
                    chart1 = ctDisp1_1;
                    chart2 = ctDisp1_2;
                    lblNG1 = lblSpecOut1_1;
                    lblNG2 = lblSpecOut1_2;
                    lblctSpec = lblct1Spec;
                    break;
                case nameof(Vision_No.Laser2):
                    chart1 = ctDisp2_1;
                    chart2 = ctDisp2_2;
                    lblNG1 = lblSpecOut2_1;
                    lblNG2 = lblSpecOut2_2;
                    lblctSpec = lblct2Spec;
                    break;
            }

        }
        private void GetCamNameFromVisionNo(int camNo, out string camName)
        {
            camName = "";
            if (camNo == Vision_No.Laser1) camName = nameof(Vision_No.Laser1);
            if (camNo == Vision_No.Laser2) camName = nameof(Vision_No.Laser2);
        }
        private void btnDispReset_Click(object sender, EventArgs e)
        {
            long dateCnt = DateTime.Now.Year * 12 * 30;
            dateCnt = dateCnt + DateTime.Now.Month * 30;
            dateCnt = dateCnt + DateTime.Now.Day;

            string lcPath = "";
            string Filepath;
            int iselect = tcGraph.SelectedIndex;
            string sname = "";
            try
            {
                if (sname != "")
                {
                    lcPath = Path.Combine(CONST.cVisionPath, sname, "Dist");
                    Filepath = Path.Combine(lcPath, dateCnt.ToString() + ".txt");
                    FileInfo removefileb1 = new FileInfo(Filepath);
                    removefileb1.Delete();
                    btnDisp1.PerformClick();
                }
            }
            catch { }
        }
        private void btnDisp1_Click(object sender, EventArgs e)
        {
            try
            {
                long sdateCnt = dtpStart.Value.Year * 12 * 30;
                sdateCnt = sdateCnt + dtpStart.Value.Month * 30;
                sdateCnt = sdateCnt + dtpStart.Value.Day;

                long edateCnt = dtpEnd.Value.Year * 12 * 30;
                edateCnt = edateCnt + dtpEnd.Value.Month * 30;
                edateCnt = edateCnt + dtpEnd.Value.Day;

                drawDist(sdateCnt, edateCnt, Vision_No.Laser1);
                drawDist(sdateCnt, edateCnt, Vision_No.Laser2);
            }
            catch
            {
            }
        }
        public void drawChartNoDist()
        {
            drawChart(Vision_No.Laser1);
            drawChart(Vision_No.Laser2);
        }
        void drawChart(int camNo, double dsquareX = 0, double dsquareY = 0)
        {
            try
            {
                GetChartDispFromVisionNo(camNo, out Chart Chart1, out Chart Chart2, out Label lblNG1, out Label lblNG2, out Label[] SpecText);
                GetCamNameFromVisionNo(camNo, out string CamName);

                double FirstY1Target = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LY];// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y1;
                double FirstY2Target = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RY];// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y2;
                double FirstX1Spec = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LX];
                double FirstX2Spec = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RX];
                if (Menu.frmSetting.revData.mBendingArm.bDisplayLRSwapBD)
                {
                    FirstY1Target = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RY];// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y2;
                    FirstY2Target = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LY];// + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y1;
                    FirstX1Spec = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RX];
                    FirstX2Spec = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LX];
                }

                double dYspec = 0;
                double dXspec = 0;

                double ChartX1_1 = 0;
                double ChartY1_1 = 0;
                double ChartX1_2 = 0;
                double ChartY1_2 = 0;

                double ChartX2_1 = 0;
                double ChartY2_1 = 0;
                double ChartX2_2 = 0;
                double ChartY2_2 = 0;
                double chartlengthX = 0;
                double chartlengthY = 0;
                switch (CamName)
                {
                    case "Bend1":
                    case "Bend2":
                    case "Bend3":
                        dXspec = Menu.frmSetting.revData.mBendingArm.BDToleranceX;
                        dYspec = Menu.frmSetting.revData.mBendingArm.BDToleranceY;
                        chartlengthX = dXspec * 1.25;
                        chartlengthY = dYspec * 1.25;

                        break;
                    case "Insp":
                        FirstX1Spec = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LX];
                        FirstX2Spec = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RX];
                        FirstY1Target = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LY];
                        FirstY2Target = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RY];
                        if (Menu.frmSetting.revData.mBendingArm.bDisplayLRSwapINSP)
                        {
                            FirstX1Spec = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RX];
                            FirstX2Spec = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LX];
                            FirstY1Target = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RY];
                            FirstY2Target = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LY];
                        }

                        dXspec = Menu.frmSetting.revData.mBendingArm.InspToleranceX;
                        dYspec = Menu.frmSetting.revData.mBendingArm.InspToleranceY;
                        chartlengthX = dXspec + 0.2;
                        chartlengthY = dYspec + 0.2;

                        break;
                    case "Attach":
                        if (CONST.PCNo == 1)
                        {
                            FirstX1Spec = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_LX];
                            FirstX2Spec = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_RX];
                            FirstY1Target = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_LY];
                            FirstY2Target = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_RY];
                        }
                        else
                        {
                            FirstX1Spec = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_LX];
                            FirstX2Spec = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_RX];
                            FirstY1Target = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_LY];
                            FirstY2Target = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_RY];
                        }
                        dXspec = Menu.frmSetting.revData.mSizeSpecRatio.AttachToleranceX;
                        dYspec = Menu.frmSetting.revData.mSizeSpecRatio.AttachToleranceY;
                        chartlengthX = dXspec * 1.5;
                        chartlengthY = dYspec * 1.5;

                        break;

                    case nameof(Vision_No.Laser1):
                    case nameof(Vision_No.Laser2):
                        SetLaserInspectionSpec(out sSpec specX, out sSpec specY);

                        FirstX1Spec = specX.Middle1;
                        FirstX2Spec = specX.Middle2;
                        FirstY1Target = specY.Middle1;
                        FirstY2Target = specY.Middle2;

                        dXspec = specX.Spec;
                        dYspec = specY.Spec;
                        chartlengthX = dXspec * 1.5;
                        chartlengthY = dYspec * 1.5;

                        break;

                }

                ChartX1_1 = FirstX1Spec + chartlengthX;
                ChartY1_1 = FirstY1Target + chartlengthY;
                ChartX1_2 = FirstX1Spec - chartlengthX;
                ChartY1_2 = FirstY1Target - chartlengthY;

                ChartX2_1 = FirstX2Spec + chartlengthX;
                ChartY2_1 = FirstY2Target + chartlengthY;
                ChartX2_2 = FirstX2Spec - chartlengthX;
                ChartY2_2 = FirstY2Target - chartlengthY;

                if (dsquareX == 0) dsquareX = dXspec;
                if (dsquareY == 0) dsquareY = dYspec;

                CONST.DispChart.SquareX[camNo] = dsquareX;
                CONST.DispChart.SquareY[camNo] = dsquareY;

                //draw chart1
                if (Chart1.InvokeRequired)
                {
                    Chart1.Invoke(new MethodInvoker(delegate
                    {
                        Chart1.Series.Clear();
                        Chart1.Series.Add("LX/LY");
                        Chart1.Series.Add("SpecIn");
                        Chart1.Series.Add("SpecLine");
                        Chart1.Series[1].Color = Color.Blue;
                        if (CamName == "Insp")
                        {
                            Chart1.Series.Add(" ");
                            //Chart1.Series[3].Color = Color.Indigo;
                        }

                        Chart1.Series[0].BorderWidth = 1;
                        Chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
                        Chart1.ChartAreas[0].AxisX.Maximum = ChartX1_1;
                        Chart1.ChartAreas[0].AxisX.Minimum = ChartX1_2;
                        Chart1.ChartAreas[0].AxisY.Maximum = ChartY1_1;
                        Chart1.ChartAreas[0].AxisY.Minimum = ChartY1_2;

                        //LX, LY 스펙 인 에어리어 박스 생성
                        Chart1.Series[1].Points.AddXY(FirstX1Spec + dXspec, FirstY1Target + dYspec);
                        Chart1.Series[1].Points.AddXY(FirstX1Spec + dXspec, FirstY1Target - dYspec);
                        Chart1.Series[1].Points.AddXY(FirstX1Spec - dXspec, FirstY1Target - dYspec);
                        Chart1.Series[1].Points.AddXY(FirstX1Spec - dXspec, FirstY1Target + dYspec);
                        Chart1.Series[1].Points.AddXY(FirstX1Spec - dXspec, FirstY1Target + dYspec);
                        Chart1.Series[1].Points.AddXY(FirstX1Spec + dXspec, FirstY1Target + dYspec);
                        Chart1.Series[1].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                        Chart1.Series[1].BorderWidth = 1;

                        //LX, LY 스펙 Line 생성
                        Chart1.Series[2].Points.AddXY(ChartX1_1, FirstY1Target);
                        Chart1.Series[2].Points.AddXY(ChartX1_2, FirstY1Target);
                        Chart1.Series[2].Points.AddXY(FirstX1Spec, FirstY1Target);
                        Chart1.Series[2].Points.AddXY(FirstX1Spec, ChartY1_1);
                        Chart1.Series[2].Points.AddXY(FirstX1Spec, ChartY1_2);
                        Chart1.Series[2].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                        Chart1.Series[2].BorderWidth = 1;

                        if (CamName == "Insp")
                        {
                            //LX, LY 스펙 인 에어리어 박스 생성
                            Chart1.Series[3].Points.AddXY(FirstX1Spec + dsquareX, FirstY1Target + dsquareY);
                            Chart1.Series[3].Points.AddXY(FirstX1Spec + dsquareX, FirstY1Target - dsquareY);
                            Chart1.Series[3].Points.AddXY(FirstX1Spec - dsquareX, FirstY1Target - dsquareY);
                            Chart1.Series[3].Points.AddXY(FirstX1Spec - dsquareX, FirstY1Target + dsquareY);
                            Chart1.Series[3].Points.AddXY(FirstX1Spec - dsquareX, FirstY1Target + dsquareY);
                            Chart1.Series[3].Points.AddXY(FirstX1Spec + dsquareX, FirstY1Target + dsquareY);
                            Chart1.Series[3].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                            Chart1.Series[3].BorderWidth = 1;
                        }
                    }
                    ));
                }
                else
                {
                    Chart1.Series.Clear();
                    Chart1.Series.Add("LX/LY");
                    Chart1.Series.Add("SpecIn");
                    Chart1.Series.Add("SpecLine");
                    Chart1.Series[1].Color = Color.Blue;
                    if (CamName == "Insp")
                    {
                        Chart1.Series.Add(" ");
                        //Chart1.Series[3].Color = Color.Indigo;
                    }

                    Chart1.Series[0].BorderWidth = 1;
                    Chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
                    Chart1.ChartAreas[0].AxisX.Maximum = ChartX1_1;
                    Chart1.ChartAreas[0].AxisX.Minimum = ChartX1_2;
                    Chart1.ChartAreas[0].AxisY.Maximum = ChartY1_1;
                    Chart1.ChartAreas[0].AxisY.Minimum = ChartY1_2;

                    //LX, LY 스펙 인 에어리어 박스 생성
                    Chart1.Series[1].Points.AddXY(FirstX1Spec + dXspec, FirstY1Target + dYspec);
                    Chart1.Series[1].Points.AddXY(FirstX1Spec + dXspec, FirstY1Target - dYspec);
                    Chart1.Series[1].Points.AddXY(FirstX1Spec - dXspec, FirstY1Target - dYspec);
                    Chart1.Series[1].Points.AddXY(FirstX1Spec - dXspec, FirstY1Target + dYspec);
                    Chart1.Series[1].Points.AddXY(FirstX1Spec - dXspec, FirstY1Target + dYspec);
                    Chart1.Series[1].Points.AddXY(FirstX1Spec + dXspec, FirstY1Target + dYspec);
                    Chart1.Series[1].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    Chart1.Series[1].BorderWidth = 1;

                    //LX, LY 스펙 Line 생성
                    Chart1.Series[2].Points.AddXY(ChartX1_1, FirstY1Target);
                    Chart1.Series[2].Points.AddXY(ChartX1_2, FirstY1Target);
                    Chart1.Series[2].Points.AddXY(FirstX1Spec, FirstY1Target);
                    Chart1.Series[2].Points.AddXY(FirstX1Spec, ChartY1_1);
                    Chart1.Series[2].Points.AddXY(FirstX1Spec, ChartY1_2);
                    Chart1.Series[2].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    Chart1.Series[2].BorderWidth = 1;

                    if (CamName == "Insp")
                    {
                        //LX, LY 스펙 인 에어리어 박스 생성
                        Chart1.Series[3].Points.AddXY(FirstX1Spec + dsquareX, FirstY1Target + dsquareY);
                        Chart1.Series[3].Points.AddXY(FirstX1Spec + dsquareX, FirstY1Target - dsquareY);
                        Chart1.Series[3].Points.AddXY(FirstX1Spec - dsquareX, FirstY1Target - dsquareY);
                        Chart1.Series[3].Points.AddXY(FirstX1Spec - dsquareX, FirstY1Target + dsquareY);
                        Chart1.Series[3].Points.AddXY(FirstX1Spec - dsquareX, FirstY1Target + dsquareY);
                        Chart1.Series[3].Points.AddXY(FirstX1Spec + dsquareX, FirstY1Target + dsquareY);
                        Chart1.Series[3].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                        Chart1.Series[3].BorderWidth = 1;
                    }
                }

                //draw chart2
                if (Chart2.InvokeRequired)
                {
                    Chart2.Invoke(new MethodInvoker(delegate
                    {
                        Chart2.Series.Clear();
                        Chart2.Series.Add("RX/RY");
                        Chart2.Series.Add("SpecIn");
                        Chart2.Series.Add("SpecLine");
                        Chart2.Series[1].Color = Color.Blue;
                        if (CamName == "Insp")
                        {
                            Chart2.Series.Add(" ");
                            //Chart2.Series[3].Color = Color.Indigo;
                            //Chart2.Series[3].IsValueShownAsLabel = false;
                        }

                        Chart2.Series[0].BorderWidth = 1;
                        Chart2.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
                        Chart2.ChartAreas[0].AxisX.Maximum = ChartX2_1;
                        Chart2.ChartAreas[0].AxisX.Minimum = ChartX2_2;
                        Chart2.ChartAreas[0].AxisY.Maximum = ChartY2_1;
                        Chart2.ChartAreas[0].AxisY.Minimum = ChartY2_2;

                        //RX, RY 스펙 인 에어리어 박스 생성
                        Chart2.Series[1].Points.AddXY(FirstX2Spec + dXspec, FirstY2Target + dYspec);
                        Chart2.Series[1].Points.AddXY(FirstX2Spec + dXspec, FirstY2Target - dYspec);
                        Chart2.Series[1].Points.AddXY(FirstX2Spec - dXspec, FirstY2Target - dYspec);
                        Chart2.Series[1].Points.AddXY(FirstX2Spec - dXspec, FirstY2Target + dYspec);
                        Chart2.Series[1].Points.AddXY(FirstX2Spec - dXspec, FirstY2Target + dYspec);
                        Chart2.Series[1].Points.AddXY(FirstX2Spec + dXspec, FirstY2Target + dYspec);
                        Chart2.Series[1].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                        Chart2.Series[1].BorderWidth = 1;

                        //RX, RY 스펙 인 에어리어 박스 생성
                        Chart2.Series[2].Points.AddXY(ChartX2_1, FirstY2Target);
                        Chart2.Series[2].Points.AddXY(ChartX2_2, FirstY2Target);
                        Chart2.Series[2].Points.AddXY(FirstX2Spec, FirstY2Target);
                        Chart2.Series[2].Points.AddXY(FirstX2Spec, ChartY2_1);
                        Chart2.Series[2].Points.AddXY(FirstX2Spec, ChartY2_2);
                        Chart2.Series[2].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                        Chart2.Series[2].BorderWidth = 1;

                        if (CamName == "Insp")
                        {
                            //RX, RY 스펙 인 에어리어 박스 생성
                            Chart2.Series[3].Points.AddXY(FirstX2Spec + dsquareX, FirstY2Target + dsquareY);
                            Chart2.Series[3].Points.AddXY(FirstX2Spec + dsquareX, FirstY2Target - dsquareY);
                            Chart2.Series[3].Points.AddXY(FirstX2Spec - dsquareX, FirstY2Target - dsquareY);
                            Chart2.Series[3].Points.AddXY(FirstX2Spec - dsquareX, FirstY2Target + dsquareY);
                            Chart2.Series[3].Points.AddXY(FirstX2Spec - dsquareX, FirstY2Target + dsquareY);
                            Chart2.Series[3].Points.AddXY(FirstX2Spec + dsquareX, FirstY2Target + dsquareY);
                            Chart2.Series[3].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                            Chart2.Series[3].BorderWidth = 1;
                        }
                    }));
                }
                else
                {
                    Chart2.Series.Clear();
                    Chart2.Series.Add("RX/RY");
                    Chart2.Series.Add("SpecIn");
                    Chart2.Series.Add("SpecLine");
                    Chart2.Series[1].Color = Color.Blue;
                    if (CamName == "Insp")
                    {
                        Chart2.Series.Add(" ");
                        //Chart2.Series[3].Color = Color.Indigo;
                        //Chart2.Series[3].IsValueShownAsLabel = false;
                    }

                    Chart2.Series[0].BorderWidth = 1;
                    Chart2.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
                    Chart2.ChartAreas[0].AxisX.Maximum = ChartX2_1;
                    Chart2.ChartAreas[0].AxisX.Minimum = ChartX2_2;
                    Chart2.ChartAreas[0].AxisY.Maximum = ChartY2_1;
                    Chart2.ChartAreas[0].AxisY.Minimum = ChartY2_2;

                    //RX, RY 스펙 인 에어리어 박스 생성
                    Chart2.Series[1].Points.AddXY(FirstX2Spec + dXspec, FirstY2Target + dYspec);
                    Chart2.Series[1].Points.AddXY(FirstX2Spec + dXspec, FirstY2Target - dYspec);
                    Chart2.Series[1].Points.AddXY(FirstX2Spec - dXspec, FirstY2Target - dYspec);
                    Chart2.Series[1].Points.AddXY(FirstX2Spec - dXspec, FirstY2Target + dYspec);
                    Chart2.Series[1].Points.AddXY(FirstX2Spec - dXspec, FirstY2Target + dYspec);
                    Chart2.Series[1].Points.AddXY(FirstX2Spec + dXspec, FirstY2Target + dYspec);
                    Chart2.Series[1].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    Chart2.Series[1].BorderWidth = 1;

                    //RX, RY 스펙 인 에어리어 박스 생성
                    Chart2.Series[2].Points.AddXY(ChartX2_1, FirstY2Target);
                    Chart2.Series[2].Points.AddXY(ChartX2_2, FirstY2Target);
                    Chart2.Series[2].Points.AddXY(FirstX2Spec, FirstY2Target);
                    Chart2.Series[2].Points.AddXY(FirstX2Spec, ChartY2_1);
                    Chart2.Series[2].Points.AddXY(FirstX2Spec, ChartY2_2);
                    Chart2.Series[2].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    Chart2.Series[2].BorderWidth = 1;

                    if (CamName == "Insp")
                    {
                        //RX, RY 스펙 인 에어리어 박스 생성
                        Chart2.Series[3].Points.AddXY(FirstX2Spec + dsquareX, FirstY2Target + dsquareY);
                        Chart2.Series[3].Points.AddXY(FirstX2Spec + dsquareX, FirstY2Target - dsquareY);
                        Chart2.Series[3].Points.AddXY(FirstX2Spec - dsquareX, FirstY2Target - dsquareY);
                        Chart2.Series[3].Points.AddXY(FirstX2Spec - dsquareX, FirstY2Target + dsquareY);
                        Chart2.Series[3].Points.AddXY(FirstX2Spec - dsquareX, FirstY2Target + dsquareY);
                        Chart2.Series[3].Points.AddXY(FirstX2Spec + dsquareX, FirstY2Target + dsquareY);
                        Chart2.Series[3].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                        Chart2.Series[3].BorderWidth = 1;
                    }
                }

                if (lblNG1.InvokeRequired)
                {
                    lblNG1.Invoke(new MethodInvoker(delegate
                        {
                            if (CamName == "Insp")
                            {
                                lblNG1.Text = "OK: 0" + "  NG: 0" + "  SquareNG: 0";
                            }
                            else
                            {
                                lblNG1.Text = "OK: 0" + "  NG: 0";
                            }
                        }));
                }
                else
                {
                    if (CamName == "Insp")
                    {
                        lblNG1.Text = "OK: 0" + "  NG: 0" + "  SquareNG: 0";
                    }
                    else
                    {
                        lblNG1.Text = "OK: 0" + "  NG: 0";
                    }
                }
                if (lblNG2.InvokeRequired)
                {
                    lblNG2.Invoke(new MethodInvoker(delegate
                    {
                        if (CamName == "Insp")
                        {
                            lblNG2.Text = "OK: 0" + "  NG: 0" + "  SquareNG: 0";
                        }
                        else
                        {
                            lblNG2.Text = "OK: 0" + "  NG: 0";
                        }
                    }));
                }
                else
                {
                    if (CamName == "Insp")
                    {
                        lblNG2.Text = "OK: 0" + "  NG: 0" + "  SquareNG: 0";
                    }
                    else
                    {
                        lblNG2.Text = "OK: 0" + "  NG: 0";
                    }
                }

                SpecText[0].Text = FirstX1Spec.ToString("0.000");
                SpecText[1].Text = FirstY1Target.ToString("0.000");
                SpecText[2].Text = FirstX2Spec.ToString("0.000");
                SpecText[3].Text = FirstY2Target.ToString("0.000");

            }
            catch { }
        }
        private void btnClearDisp_Click(object sender, EventArgs e)
        {
            InitialDispChart();
            drawChartNoDist();
        }
        public void InitialDispChart()
        {
            for (int i = 0; i < 8; i++)
            {
                CONST.DispChart.CountIn1[i] = 0;
                CONST.DispChart.CountIn2[i] = 0;
                CONST.DispChart.CountOut1[i] = 0;
                CONST.DispChart.CountOut2[i] = 0;
                CONST.DispChart.CountSquareOut1[i] = 0;
                CONST.DispChart.CountSquareOut2[i] = 0;
                CONST.DispChart.Count[i] = 0;
            }
        }
        //JJ, 2017-05-19 : Auto Screen Full ---- start , cogDS 화면에 DoubleClick 이벤트를 등록시켜 놓는다
        private Panel parentCogDS = null;

        private Cognex.VisionPro.Display.CogDisplay selectCogDS = null;
        private Point pCogDS;
        private Size sizeCogDS;

        private void cogDS_DoubleClick(object sender, EventArgs e)
        {
            Cognex.VisionPro.Display.CogDisplay cogDS = sender as Cognex.VisionPro.Display.CogDisplay;
            if (parentCogDS == null) // 전체 화면 모드로 변경
            {
                parentCogDS = cogDS.Parent as Panel;
                selectCogDS = cogDS;
                sizeCogDS = cogDS.Size;
                pCogDS = cogDS.Location;

                parentCogDS.Controls.Remove(cogDS);

                this.Controls.Add(cogDS);

                int XMargin = 100;
                int YMargin = 50;

                cogDS.Location = new Point(XMargin, YMargin);
                cogDS.Size = new Size(this.Width - XMargin * 2, this.Bottom - YMargin * 2);
                this.Controls.SetChildIndex(cogDS, 0);
            }
            else // 원래 화면으로 변경
            {
                if (selectCogDS.Equals(cogDS) == false) return; // 현제 보여 지고 있는 화면이 아니면 리턴
                this.Controls.Remove(cogDS);
                cogDS.Location = pCogDS;
                cogDS.Size = sizeCogDS;
                parentCogDS.Controls.Add(cogDS);
                parentCogDS = null;
            }
        }

        //csh 20170601
        private void checkBox_Overlay1_CheckedChanged(object sender, EventArgs e)
        {
            if (Vision[0] == null)
            {
                checkBox_Overlay1.Checked = false;
                return;
            }
            Vision[0].Overay(checkBox_Overlay1.Checked);
        }
        private void checkBox_Overlay2_CheckedChanged(object sender, EventArgs e)
        {
            if (Vision[1] == null)
            {
                checkBox_Overlay2.Checked = false;
                return;
            }
            Vision[1].Overay(checkBox_Overlay2.Checked);
        }
        private void checkBox_Overlay3_CheckedChanged(object sender, EventArgs e)
        {
            if (Vision[2] == null)
            {
                checkBox_Overlay3.Checked = false;
                return;
            }
            Vision[2].Overay(checkBox_Overlay3.Checked);
        }

        private void checkBox_Overlay4_CheckedChanged(object sender, EventArgs e)
        {
            if (Vision[3] == null)
            {
                checkBox_Overlay4.Checked = false;
                return;
            }

            Vision[3].Overay(checkBox_Overlay4.Checked);
        }

        private void checkBox_Overlay5_CheckedChanged(object sender, EventArgs e)
        {
            if (Vision[4] == null)
            {
                checkBox_Overlay5.Checked = false;
                return;
            }

            Vision[4].Overay(checkBox_Overlay5.Checked);
        }

        private void checkBox_Overlay6_CheckedChanged(object sender, EventArgs e)
        {
            if (Vision[5] == null)
            {
                checkBox_Overlay6.Checked = false;
                return;
            }

            Vision[5].Overay(checkBox_Overlay6.Checked);
        }

        private void checkBox_Overlay7_CheckedChanged(object sender, EventArgs e)
        {
            if (Vision[6] == null)
            {
                checkBox_Overlay7.Checked = false;
                return;
            }

            Vision[6].Overay(checkBox_Overlay7.Checked);
        }

        private void checkBox_Overlay8_CheckedChanged(object sender, EventArgs e)
        {
            if (Vision[7] == null)
            {
                checkBox_Overlay8.Checked = false;
                return;
            }

            Vision[7].Overay(checkBox_Overlay8.Checked);
        }

        private int SelectResultBox(int nCamNo)
        {
            int Num = 0;
            //20201003 cjm SDN AAM에 맞춰서  panelID 쓰기
            //210215 cjm Change
            switch (CONST.PCNo)
            {
                case 1:
                    Num = nCamNo;
                    break;
                case 2:
                    if (nCamNo == 0 || nCamNo == 1 || nCamNo == 2 || nCamNo == 3) //attach  Here
                        Num = 0;
                    else if (nCamNo == 4) //reel
                        Num = 2;
                    break;
                case 3:
                case 6:
                    if (nCamNo == 0 || nCamNo == 1) //pre
                        Num = 0;
                    else if (nCamNo == 2 || nCamNo == 3) //bend
                        Num = 1;
                    else if (nCamNo == 4 || nCamNo == 5) //bendside
                        Num = 2;
                    break;
                case 4:
                case 7:
                    if (nCamNo == 0 || nCamNo == 1) //sideinsp
                        Num = 0;
                    else if (nCamNo == 2 || nCamNo == 3) //upperinsp
                        Num = 1;
                    break;
                case 8:
                    if (nCamNo == 0 || nCamNo == 1) //pre
                        Num = 0;
                    else if (nCamNo == 2) //laser
                        Num = 1;
                    break;
            }

            return Num;
        }
        private void WritePanelID(int nCamNo, string strPanelID)
        {
            int Num;// = Convert.ToInt32(Math.Truncate(Convert.ToDouble(nCamNo) / 2));

            Num = SelectResultBox(nCamNo);

            if (strPanelID == null)
            {
                return;
            }
            if (lblPPID[Num].InvokeRequired)
            {
                try
                {
                    lblPPID[Num].Invoke(new MethodInvoker(delegate
                    {
                        lblPPID[Num].Text = "Cell ID : " + strPanelID;
                    }));
                }
                catch
                {
                }
            }
            else
            {
                lblPPID[Num].Text = strPanelID;
            }
        }

        private void WriteCycleTime(int nCamNo, double dTime)
        {
            int Num;// = Convert.ToInt32(Math.Truncate(Convert.ToDouble(nCamNo) / 2));

            Num = SelectResultBox(nCamNo);

            if (lblCycleTime[Num].InvokeRequired)
            {
                try
                {
                    lblCycleTime[Num].Invoke(new MethodInvoker(delegate
                    {
                        lblCycleTime[Num].Text = "C/T : " + (dTime / 1000).ToString("0.000") + "s";
                    }));
                }
                catch
                {
                }
            }
            else
            {
                lblCycleTime[Num].Text = "C/T : " + (dTime / 1000).ToString("0.000") + "s";
            }
        }

        private void WriteRetryCount(int nCamNo, int nTime)
        {
            int Num;// = Convert.ToInt32(Math.Truncate(Convert.ToDouble(nCamNo) / 2));

            Num = SelectResultBox(nCamNo);

            if (lblRetryCount[Num].InvokeRequired)
            {
                try
                {
                    lblRetryCount[Num].Invoke(new MethodInvoker(delegate
                    {
                        lblRetryCount[Num].Text = "Retry : " + nTime.ToString("0");
                    }));
                }
                catch
                {
                }
            }
            else
            {
                lblRetryCount[Num].Text = "Retry : " + nTime.ToString("0");
            }
        }

        class cResult
        {
            public int OK { get; set; }
            public int BY_PASS { get; set; }
            public int ALIGN_LIMIT { get; set; }
            public int SPEC_OVER { get; set; }
            public int CHECK { get; set; }
            public int WORKER_BY_PASS { get; set; }
            public int MANUAL_BENDING { get; set; }
            public int INIT { get; set; }
            public int RETRY_OVER { get; set; }
            public int PANEL_SHIFT_NG { get; set; }
            public int ERROR_MARK { get; set; }
            public int FIRST_LIMIT { get; set; }
            public int ERROR_LCHECK { get; set; }
            //20.12.17 lkw DL
            public int DL { get; set; }
        }
        List<cResult> viewOKNG = new List<cResult>();
        public void DisplayOKNG1(List<CONST.OK_NG1> tlist, int j)
        {
            if (dgvOKNG.Rows[0].HeaderCell.Value == null)
            {
                for (int i = 0; i < viewOKNG.Count; i++)
                {
                    dgvOKNG.Rows[i].HeaderCell.Value = tlist[i].Name;
                }
            }
            viewOKNG[j].OK = tlist[j].count[(int)ePCResult.OK];
            viewOKNG[j].BY_PASS = tlist[j].count[(int)ePCResult.BY_PASS];
            viewOKNG[j].ALIGN_LIMIT = tlist[j].count[(int)ePCResult.ALIGN_LIMIT];
            viewOKNG[j].SPEC_OVER = tlist[j].count[(int)ePCResult.SPEC_OVER];
            viewOKNG[j].CHECK = tlist[j].count[(int)ePCResult.CHECK];
            viewOKNG[j].WORKER_BY_PASS = tlist[j].count[(int)ePCResult.WORKER_BY_PASS];
            viewOKNG[j].MANUAL_BENDING = tlist[j].count[(int)ePCResult.MANUAL_BENDING];
            viewOKNG[j].INIT = tlist[j].count[(int)ePCResult.INIT];
            viewOKNG[j].RETRY_OVER = tlist[j].count[(int)ePCResult.RETRY_OVER];
            viewOKNG[j].PANEL_SHIFT_NG = tlist[j].count[(int)ePCResult.PANEL_SHIFT_NG];
            viewOKNG[j].ERROR_MARK = tlist[j].count[(int)ePCResult.ERROR_MARK];
            viewOKNG[j].FIRST_LIMIT = tlist[j].count[(int)ePCResult.FIRST_LIMIT];
            viewOKNG[j].ERROR_LCHECK = tlist[j].count[(int)ePCResult.ERROR_LCHECK];
            //20.12.17 lkw DL
            viewOKNG[j].DL = tlist[j].count[(int)ePCResult.DL];
            if (dgvOKNG.InvokeRequired) //여기서 리프레쉬
            {
                dgvOKNG.Invoke(new MethodInvoker(delegate
                {
                    dgvOKNG.Refresh();
                }));
            }
            else
            {
                dgvOKNG.Refresh();
            }
        }
        public void DisplayOKNG2(int[,] okngData, int j)
        {
            viewOKNG[j].OK = okngData[j, (int)ePCResult.OK];
            viewOKNG[j].BY_PASS = okngData[j, (int)ePCResult.BY_PASS];
            viewOKNG[j].ALIGN_LIMIT = okngData[j, (int)ePCResult.ALIGN_LIMIT];
            viewOKNG[j].SPEC_OVER = okngData[j, (int)ePCResult.SPEC_OVER];
            viewOKNG[j].CHECK = okngData[j, (int)ePCResult.CHECK];
            viewOKNG[j].WORKER_BY_PASS = okngData[j, (int)ePCResult.WORKER_BY_PASS];
            viewOKNG[j].MANUAL_BENDING = okngData[j, (int)ePCResult.MANUAL_BENDING];
            viewOKNG[j].INIT = okngData[j, (int)ePCResult.INIT];
            viewOKNG[j].RETRY_OVER = okngData[j, (int)ePCResult.RETRY_OVER];
            viewOKNG[j].PANEL_SHIFT_NG = okngData[j, (int)ePCResult.PANEL_SHIFT_NG];
            viewOKNG[j].ERROR_MARK = okngData[j, (int)ePCResult.ERROR_MARK];
            viewOKNG[j].FIRST_LIMIT = okngData[j, (int)ePCResult.FIRST_LIMIT];
            viewOKNG[j].ERROR_LCHECK = okngData[j, (int)ePCResult.ERROR_LCHECK];
            //20.12.17 lkw DL
            viewOKNG[j].DL = okngData[j, (int)ePCResult.DL];
        }

        //2018.06.04 khs
        private void rbGraph_CheckedChanged(object sender, EventArgs e)
        {
            tcGraph.Visible = true;
            pnGrahpData.Visible = true;
            dgvOKNG.Visible = false;
        }

        private void rbOKNG_CheckedChanged(object sender, EventArgs e)
        {
            dgvOKNG.Visible = true;
            tcGraph.Visible = false;
            pnGrahpData.Visible = false;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            lbResult1Title.Text = CONST.RESULT_TITLE1; //"BENDING1 RESULT"; //추후 INI 파일에서불러오도록 수정
            lbResult2Title.Text = CONST.RESULT_TITLE2; //"BENDING2 RESULT";  //추후 INI 파일에서불러오도록 수정

            pnResult1.Visible = true;
            pnResult2.Visible = false;

            //하드코딩
            pnResult1.Location = new Point(1, 28);
            pnResult1.Size = new Size(521, 399);

            //UpdateResultDisplay();
        }
        List<CONST.OK_NG1> tlist = new List<CONST.OK_NG1>();
        private void InitOKNG1()
        {
            dgvOKNG.Columns.Clear();
            dgvOKNG.Rows.Clear();
            //리스트 생성
            string[] sPC1;

            //count표 헤더를 바꾸려면 여기를 바꾸면 됨.
            sPC1 = Enum.GetNames(typeof(eCamNO1));

            foreach (var s in sPC1)
            {
                cResult result = new cResult();
                viewOKNG.Add(result); //datagridview 출력을 위한 값
                CONST.OK_NG1 temp = new CONST.OK_NG1(s);
                tlist.Add(temp); //로그 저장하기 위한 값

            }
            dgvOKNG.DataSource = viewOKNG;
            //
            for (int i = 0; i < dgvOKNG.Columns.Count; i++)
            {
                dgvOKNG.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgvOKNG.Columns[i].Resizable = DataGridViewTriState.False;
                dgvOKNG.Columns[i].Width = 40;
            }

            //
            int dgvokngsize = 0;

            dgvOKNG.RowHeadersWidth = 100;

            for (int i = 0; i < viewOKNG.Count; i++)
            {
                dgvOKNG.Rows[i].HeaderCell.Value = tlist[i].Name;
                dgvOKNG.Rows[i].Height = 43;
                ReadInitOKNGCount1(ref tlist, i);

                dgvokngsize += dgvOKNG.Rows[i].Height;
            }

            dgvOKNG.Size = new Size(522, dgvokngsize + 80);

        }

        public void ReadInitOKNGCount1(ref List<CONST.OK_NG1> tlist, int i)
        {
            cLog.ReadOKNGCount(tlist[i].Name, ref tlist[i].count);
            DisplayOKNG1(tlist, i);
        }

        public void SetOKNGCount2(string strName, ePCResult _nIndex, int visionNo)
        {
            //20.12.17 lkw DL
            if (_nIndex == ePCResult.OK && bDLMarkFind[visionNo])
            {
                bDLMarkFind[visionNo] = false;
                _nIndex = ePCResult.DL;
            }

            int index = 0;
            int nIndex = (int)_nIndex;
            for (int i = 0; i < tlist.Count; i++)
            {
                if (tlist[i].Name.Equals(strName))
                {
                    tlist[i].count[nIndex]++;
                    index = i;
                    break;
                }
            }
            cLog.WriteOKNGCount(tlist[index].Name, tlist[index].count, nIndex);
            DisplayOKNG1(tlist, index);

            //190430
            string ErrorList = _nIndex.ToString();

            LogDispError1(visionNo, Vision[visionNo].PanelID, ErrorList);
        }

        private void cbResult_CheckedChanged(object sender, EventArgs e)
        {
            if (cbResult.Checked)
            {
                pnResult1.Visible = true;
                pnResult2.Visible = false;
                pnTimeData.Visible = false;
                dgvOKNG.Visible = false;
                pngraph.Visible = true;
                //pngraph.Visible = false;

            }
            else
            {
                pnTimeData.Visible = true;
                dgvOKNG.Visible = true;
                pngraph.Visible = false;
                //pngraph.Visible = true;
                pnResult1.Visible = false;
                pnResult2.Visible = false;
            }
        }

        private void btnDisp_Click(object sender, EventArgs e)
        {
            string StartData = dtpokngStart.Value.ToShortDateString();
            string EndData = dtpokngEnd.Value.ToShortDateString();

            int[,] okngData = new int[8, 17];
            cLog.ReadOKNGLog(StartData, EndData, ref okngData);
            for (int i = 0; i < viewOKNG.Count; i++)
            {
                DisplayOKNG2(okngData, i);
            }
            if (dgvOKNG.InvokeRequired) //여기서 리프레쉬
            {
                dgvOKNG.Invoke(new MethodInvoker(delegate
                {
                    dgvOKNG.Refresh();
                }));
            }
            else
            {
                dgvOKNG.Refresh();
            }
        }

        private void CamInfoSetting(bool Cam1Use, bool Cam2Use, int Num)
        {
            int AddLocation = 337;
            if (!Cam1Use)
            {
                lblPPID[Num].Size = new Size(new Point(172, 20));
                lblPPID[Num].Location = new Point(AddLocation, 0);
                lblCycleTime[Num].Size = new Size(new Point(93, 20));
                lblCycleTime[Num].Location = new Point(171 + AddLocation, 0);
                lblRetryCount[Num].Size = new Size(new Point(75, 20));
                lblRetryCount[Num].Location = new Point(262 + AddLocation, 0);
                lbList[Num].Size = new Size(new Point(337, 88));
                lbList[Num].Location = new Point(AddLocation, 0);
                //lbListRetry[Num].Size = new Size(new Point(337, 100));
                //lbListRetry[Num].Location = new Point(AddLocation, 0);
                TCList[Num].Size = new Size(new Point(339, 118));
                TCList[Num].Location = new Point(AddLocation, 0);
            }
            else if (!Cam2Use)
            {
                lblPPID[Num].Size = new Size(new Point(172, 20));
                lblCycleTime[Num].Size = new Size(new Point(93, 20));
                lblCycleTime[Num].Location = new Point(171, 0);
                lblRetryCount[Num].Size = new Size(new Point(75, 20));
                lblRetryCount[Num].Location = new Point(262, 0);
                //lbList[Num].Size = new Size(new Point(337, 100));
                //lbListRetry[Num].Size = new Size(new Point(337, 100));
                TCList[Num].Size = new Size(new Point(339, 118));

                //190703 cjm Auto화면 2배 증가
                if (Num == 0)
                {
                    lblPPID[Num].Size = new Size(new Point(344, 20));
                    lblCycleTime[Num].Size = new Size(new Point(186, 20));
                    lblCycleTime[Num].Location = new Point(337, 0);
                    lblRetryCount[Num].Size = new Size(new Point(150, 20));
                    lblRetryCount[Num].Location = new Point(511, 0);
                    //lbList[Num].Size = new Size(new Point(337, 100));
                    //lbListRetry[Num].Size = new Size(new Point(337, 100));
                    TCList[Num].Size = new Size(new Point(678, 118));
                }
            }
        }

        private void btnTodayDisp_Click(object sender, EventArgs e)
        {
            string StartData = DateTime.Now.ToString("yyyy-MM-dd");
            string EndData = DateTime.Now.ToString("yyyy-MM-dd");

            int[,] okngData = new int[8, 17];
            //int CamCnt = 0;
            cLog.ReadOKNGLog(StartData, EndData, ref okngData);
            for (int i = 0; i < viewOKNG.Count; i++)
            {
                DisplayOKNG2(okngData, i);
            }
            if (dgvOKNG.InvokeRequired) //여기서 리프레쉬
            {
                dgvOKNG.Invoke(new MethodInvoker(delegate
                {
                    dgvOKNG.Refresh();
                }));
            }
            else
            {
                dgvOKNG.Refresh();
            }
            //if (CONST.PCName == "AAM_PC1") CamCnt = 5;
            //else if (CONST.PCName == "AAM_PC2") CamCnt = 7;
            //CamCnt = okngData.Length / 8;

            //try
            //{
            //    for (int i = 0; i < CamCnt; i++)
            //    {
            //        for (int j = 0; j < 16; j++)
            //        {
            //            dgvOKNG.Rows[i].Cells[j].Value = okngData[i, j].ToString();
            //        }
            //    }
            //}
            //catch(Exception EX)
            //{

            //}
        }

        //bool firstCPKCheck = true;
        public double AutoMainLogDatacalculator(double[] Data, double Spec, double Tolerence)
        {
            //Cnt가 0=X1, 1=Y1, 2=X2, 3=Y2
            double USL = 0;
            double LSL = 0;

            double Max = -999;// = new double[4];
            double Min = 999;// = new double[4];
            double Gap = 0;// = new double[4];
            double Avg = 0;// = new double[4];
            double StandardDeviation = 0;// = new double[4];
            double CP = 0;// = new double[4];

            int datacnt = 0;
            double dAvg = 0;
            for (int i = 0; i < Data.Length; i++)
            {
                if (Data[i] != 0)
                {
                    if (Data[i] >= Max) Max = Data[i];
                    if (Data[i] <= Min) Min = Data[i];
                    dAvg += Data[i];
                    datacnt++;
                }
            }
            dAvg = dAvg / datacnt;
            Avg = dAvg;

            USL = Spec + Tolerence;
            LSL = Spec - Tolerence;

            Gap = Math.Abs(Max - Min);
            StandardDeviation = Menu.frmHistory.Stdev(Data, Avg, datacnt);

            CP = (USL - LSL) / (6 * StandardDeviation);
            double CPKValue = Math.Min((USL - Avg), (Avg - LSL)) / (3 * StandardDeviation);
            return CPKValue;
        }
        private void rbGraphDraw_CheckedChanged(object sender, EventArgs e)
        {
            this.btnDisp1_Click(this, null);
        }
        private void cbLive_CheckedChanged(object sender, EventArgs e)
        {
            string sName = (sender as CheckBox).Name;
            for (short i = 0; i < cbLive.Length; i++)
            {
                if (cbLive[i].Name == sName)
                {
                    //Vision[i].SideLive(cbLive[i].Checked);
                    Vision[i].Live(cbLive[i].Checked);
                }
            }
        }
        public struct sSpec
        {
            public double Spec; //tolerence양
            //1왼쪽 2오른쪽
            public double Upper1;
            public double Middle1;
            public double Lower1;

            public double Upper2;
            public double Middle2;
            public double Lower2;

            public double ExceptLastOffsetUpper1;
            public double ExceptLastOffsetLower1;
            public double ExceptLastOffsetUpper2;
            public double ExceptLastOffsetLower2;
        }

        private bool SetdgvSpec(sSpec SpecX, sSpec SpecY, ref DataGridView dgvSpec)//, double[] dFour)
        {
            try
            {
                if (dgvSpec.Columns.Count == 0)
                {
                    dgvSpec.Columns.Clear();
                    dgvSpec.Columns.Add("Column1", "LX");
                    dgvSpec.Columns.Add("Column2", "LY");
                    dgvSpec.Columns.Add("Column3", "RX");
                    dgvSpec.Columns.Add("Column4", "RY");
                    dgvSpec.Columns[0].Width = 50;
                    dgvSpec.Columns[1].Width = 50;
                    dgvSpec.Columns[2].Width = 50;
                    dgvSpec.Columns[3].Width = 50;
                }
                if (dgvSpec.Rows.Count == 0)
                {
                    dgvSpec.Rows.Clear();
                    dgvSpec.Rows.Add(4);
                }
                dgvSpec[0, 0].Value = SpecX.Lower1;
                dgvSpec[0, 1].Value = SpecX.Middle1;
                dgvSpec[0, 2].Value = SpecX.Upper1;
                //dgvSpec[0, 3].Value = 0;// dFour[0];

                dgvSpec[1, 0].Value = SpecY.Lower1;
                dgvSpec[1, 1].Value = SpecY.Middle1;
                dgvSpec[1, 2].Value = SpecY.Upper1;
                //dgvSpec[1, 3].Value = 0;// dFour[1];

                dgvSpec[2, 0].Value = SpecX.Lower2;
                dgvSpec[2, 1].Value = SpecX.Middle2;
                dgvSpec[2, 2].Value = SpecX.Upper2;
                //dgvSpec[2, 3].Value = 0;// dFour[2];

                dgvSpec[3, 0].Value = SpecY.Lower2;
                dgvSpec[3, 1].Value = SpecY.Middle2;
                dgvSpec[3, 2].Value = SpecY.Upper2;
                //dgvSpec[3, 3].Value = 0;// dFour[3];

                if (Menu.frmSetting.revData.mBendingArm.bDisplayLRSwapINSP)
                {
                    dgvSpec[2, 0].Value = SpecX.Lower1;
                    dgvSpec[2, 1].Value = SpecX.Middle1;
                    dgvSpec[2, 2].Value = SpecX.Upper1;

                    dgvSpec[3, 0].Value = SpecY.Lower1;
                    dgvSpec[3, 1].Value = SpecY.Middle1;
                    dgvSpec[3, 2].Value = SpecY.Upper1;

                    dgvSpec[0, 0].Value = SpecX.Lower2;
                    dgvSpec[0, 1].Value = SpecX.Middle2;
                    dgvSpec[0, 2].Value = SpecX.Upper2;

                    dgvSpec[1, 0].Value = SpecY.Lower2;
                    dgvSpec[1, 1].Value = SpecY.Middle2;
                    dgvSpec[1, 2].Value = SpecY.Upper2;
                }
                return true;
            }
            catch
            {
                return false;
            }

        }
        public bool SetXYfromXYT(out cs2DAlign.ptXY sourcePixel1, out cs2DAlign.ptXY targetPixel1, out cs2DAlign.ptXY sourcePixel2, out cs2DAlign.ptXY targetPixel2,
            cs2DAlign.ptXYT Panel1, cs2DAlign.ptXYT Panel2, cs2DAlign.ptXYT FPC1, cs2DAlign.ptXYT FPC2, bool RobotArmAlign = false)
        {
            if (RobotArmAlign)
            {
                sourcePixel1.X = FPC1.X;
                sourcePixel1.Y = FPC1.Y;
                sourcePixel2.X = FPC2.X;
                sourcePixel2.Y = FPC2.Y;

                targetPixel1.X = Panel1.X;
                targetPixel1.Y = Panel1.Y;
                targetPixel2.X = Panel2.X;
                targetPixel2.Y = Panel2.Y;
            }
            else
            {
                sourcePixel1.X = Panel1.X;
                sourcePixel1.Y = Panel1.Y;
                sourcePixel2.X = Panel2.X;
                sourcePixel2.Y = Panel2.Y;

                targetPixel1.X = FPC1.X;
                targetPixel1.Y = FPC1.Y;
                targetPixel2.X = FPC2.X;
                targetPixel2.Y = FPC2.Y;
            }
            return true;
        }

        private bool SetInspectionSpec(out sSpec SpecX, out sSpec SpecY)
        {
            SpecX = default(sSpec);
            SpecY = default(sSpec);
            SpecX.Spec = Menu.frmSetting.revData.mBendingArm.InspToleranceX;// CONST.RunRecipe.Param[eRecipe.INSPECTION_SPEC_X];
            SpecY.Spec = Menu.frmSetting.revData.mBendingArm.InspToleranceY;// CONST.RunRecipe.Param[eRecipe.INSPECTION_SPEC_Y];

            SpecY.Upper1 = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LY] + SpecY.Spec;
            SpecY.Middle1 = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LY];
            SpecY.Lower1 = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LY] - SpecY.Spec;
            SpecY.Upper2 = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RY] + SpecY.Spec;
            SpecY.Middle2 = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RY];
            SpecY.Lower2 = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RY] - SpecY.Spec;

            SpecX.Upper1 = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LX] + SpecX.Spec;
            SpecX.Middle1 = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LX];
            SpecX.Lower1 = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LX] - SpecX.Spec;
            SpecX.Upper2 = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RX] + SpecX.Spec;
            SpecX.Middle2 = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RX];
            SpecX.Lower2 = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RX] - SpecX.Spec;

            return true;
        }
        private bool SetLaserInspectionSpec(out sSpec SpecX, out sSpec SpecY)
        {
            SpecX = default(sSpec);
            SpecY = default(sSpec);
            //SpecX.Spec = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_TOLERANCEX];
            //SpecY.Spec = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_TOLERANCEY];

            SpecX.Spec = Menu.frmSetting.revData.mSizeSpecRatio.LaserPositionToleranceX;
            SpecY.Spec = Menu.frmSetting.revData.mSizeSpecRatio.LaserPositionToleranceY;

            double markSizeX = Menu.frmSetting.revData.mSizeSpecRatio.LaserMarkSizeX;
            double markSizeY = Menu.frmSetting.revData.mSizeSpecRatio.LaserMarkSizeY;

            if (Menu.frmSetting.revData.mLaser.MCRRight)
            {
                SpecY.Upper1 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECY] + SpecY.Spec;
                SpecY.Middle1 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECY];
                SpecY.Lower1 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECY] - SpecY.Spec;


                SpecX.Upper1 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECX] + SpecX.Spec;
                SpecX.Middle1 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECX];
                SpecX.Lower1 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECX] - SpecX.Spec;




                SpecY.Upper2 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECY] + SpecY.Spec + markSizeY;
                SpecY.Middle2 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECY] + markSizeY;
                SpecY.Lower2 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECY] - SpecY.Spec + markSizeY;


                SpecX.Upper2 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECX] + SpecX.Spec + markSizeX;
                SpecX.Middle2 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECX] + markSizeX;
                SpecX.Lower2 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECX] - SpecX.Spec + markSizeX;
            }
            else
            {
                SpecY.Upper2 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECY] + SpecY.Spec;
                SpecY.Middle2 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECY];
                SpecY.Lower2 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECY] - SpecY.Spec;


                SpecX.Upper2 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECX] + SpecX.Spec;
                SpecX.Middle2 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECX];
                SpecX.Lower2 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECX] - SpecX.Spec;




                SpecY.Upper1 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECY] + SpecY.Spec + markSizeY;
                SpecY.Middle1 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECY] + markSizeY;
                SpecY.Lower1 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECY] - SpecY.Spec + markSizeY;


                SpecX.Upper1 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECX] + SpecX.Spec + markSizeX;
                SpecX.Middle1 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECX] + markSizeX;
                SpecX.Lower1 = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_SPECX] - SpecX.Spec + markSizeX;
            }
            return true;
        }
        public int CaptureCnt1 = 0;
        public int CaptureCnt2 = 0;
        public int CaptureCnt3 = 0;

        private void btnManualMark_Click(object sender, EventArgs e)
        {
            foreach (var s in manualMark)
            {
                if (!s.bWorkerVisible)
                {
                    s.Visible = true;
                    s.bWorkerVisible = true;
                }
            }
        }
        private bool CheckManualMarkPopup(int visionno, CONST.eImageSaveKind ikind = CONST.eImageSaveKind.normal)
        {
            if (!bManualMarkInitComp[visionno]) ManualMarkInitial(visionno); //수동마크 초기화

            bool result = false;
            eCalType type = Vision[visionno].CFG.CalType;

            int cnt = (int)type + 1;
            if (type == eCalType.Cam1Cal2) cnt = 1;

            for (int i = 0; i < cnt; i++)
            {
                if (manualMark[visionno + i].PopupCheck())
                {
                    Vision[visionno + i].ImageSave(false, Vision[visionno + i].CFG.ImageSaveType, "Manual Mark", "", ikind);
                    manualMark[visionno + i].setImg(cogDS[visionno + i].Image, Vision[visionno + i].CFG.Name, bLCheckError[visionno]);
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }
        private bool CheckManualMarkDone(int visionno)
        {
            //하나라도 false면 false반환
            bool result = true;
            eCalType type = Vision[visionno].CFG.CalType;

            int cnt = (int)type + 1;
            if (type == eCalType.Cam1Cal2) cnt = 1;

            for (int i = 0; i < cnt; i++)
            {
                if (!manualMark[visionno + i].DoneCheck())
                {
                    result = false;
                }
            }
            return result;
        }
        private bool CheckManualMarkBypass(int visionno)
        {
            bool result = false;
            eCalType type = Vision[visionno].CFG.CalType;

            int cnt = (int)type + 1;
            if (type == eCalType.Cam1Cal2) cnt = 1;

            for (int i = 0; i < cnt; i++)
            {
                for (int j = 0; j < manualMark[0].MarkInfo.Length; j++)
                {
                    if (manualMark[visionno + i].MarkInfo[j].NG)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }
        private bool ManualMarkInitial(int visionno)
        {
            bManualMarkInitComp[visionno] = true;
            bool result = false;
            eCalType type = Vision[visionno].CFG.CalType;

            int cnt = (int)type + 1;
            if (type == eCalType.Cam1Cal2) cnt = 1;

            for (int i = 0; i < cnt; i++)
            {
                for (int j = 0; j < manualMark[0].MarkInfo.Length; j++)
                {
                    manualMark[visionno + i].manualMarkInitial();
                    Vision[visionno + i].TempPMAlignUntrain();
                }
            }
            return result;
        }
        bool bWorkerVisible = false;
        bool flick = false; //tt
        private void tmr1Sec_Tick(object sender, EventArgs e)
        {
            flick = !flick;
            //1초마다 확인하는 타이머(디스플레이용)
            bWorkerVisible = false;
            if (manualMark != null)
            {
                foreach (var s in manualMark)
                {
                    if (!s.bWorkerVisible)
                    {
                        this.bWorkerVisible = true;
                        break;
                    }
                }
            }
            if (bWorkerVisible && flick)
            {
                btnManualMark.BackColor = Color.Red;
            }
            else
            {
                btnManualMark.BackColor = Color.Transparent;
            }

            if (CONST.simulation && !btnOn.Visible)
            {
                btnOn.Visible = true;
                btnOff.Visible = true;
                comboBox1.Visible = true;
                for (int i = 0; i < btnImage.Length; i++) btnImage[i].Visible = true;
            }
            else if (!CONST.simulation && btnOn.Visible)
            {
                btnOn.Visible = false;
                btnOff.Visible = false;
                comboBox1.Visible = false;
                for (int i = 0; i < btnImage.Length; i++) btnImage[i].Visible = false;
            }
        }
        private void btnImage_Click(object sender, EventArgs e)
        {
            int CamNo = 0;
            string sName = (sender as Button).Name;
            for (short i = 0; i < btnImage.Length; i++)
            {
                if (btnImage[i].Name == sName)
                {
                    CamNo = i;
                    break;
                }
            }

            string path = Path.Combine(CONST.cImagePath, Vision[CamNo].CFG.Name.Trim());

            OpenFileDialog OF = new OpenFileDialog();
            OF.InitialDirectory = path;
            OF.FilterIndex = 2;

            if (OF.ShowDialog(this) == DialogResult.OK)
            {
                Bitmap bmpTest = (Bitmap)Image.FromFile(OF.FileName);


                cogDS[CamNo].Image = new CogImage8Grey(bmpTest);
                cogDS[CamNo].AutoFit = true;
            }
        }
        private void SetResult(string CamName, ePCResult iPCResult, int visionNo, short sreplyaddress = -1,
            cs2DAlign.ptAlignResult align = default(cs2DAlign.ptAlignResult), int iretry = -1, bool bdist = false,
            cs2DAlign.ptXXYY uvrw = default(cs2DAlign.ptXXYY), cs2DAlign.ptXYT xyt = default(cs2DAlign.ptXYT))
        {
            bool buvrw = false;
            string sCamName = CamName; //retry가 붙음.
            if (iretry > -1) sCamName += "[" + iretry + "]";
            if (!uvrw.Equals(default(cs2DAlign.ptXXYY))) buvrw = true;

            if (CamName == nameof(eCamNO.Laser1) || CamName == nameof(eCamNO.Laser2))
                buvrw = false;

            bool bRYT = false;
            //if(CamName == eCamNO4.EMIAttach.ToString() || CamName == eCamNO4.TempAttach.ToString())
            //{
            //    bRYT = true;
            //}
            if (sreplyaddress != -1) pcResult[sreplyaddress] = (int)iPCResult;

            //결과값 마지막에 보내기 //결과를 마지막에 보내는건 좋은데 화면로그를 같이 표시하는건 좋은방법은 아닌듯..
            switch (iPCResult)
            {
                case ePCResult.WAIT:
                    break;
                case ePCResult.OK: //복잡함
                    if (bRYT)
                    {
                        LogDisp(visionNo, sCamName + " OK, R:" + align.X.ToString("0.000") + ", Y: " + align.Y.ToString("0.000") + ", T: " + align.T.ToString("0.000"));
                        break;
                    }
                    //else if(CamName == nameof(eCamNO4.LMI))
                    //    LogDisp(visionNo, sCamName + "OK");
                    //else if(sreplyaddress == CONST.AAM_PLC2.Reply.pcBend1TransAlignReply || sreplyaddress == CONST.AAM_PLC2.Reply.pcBend2TransAlignReply || sreplyaddress == CONST.AAM_PLC2.Reply.pcBend3TransAlignReply)
                    //    LogDisp(visionNo, sCamName + "Transfer OK X:" + align.X.ToString("0.000") + ", Y: " + align.Y.ToString("0.000") + ", T: " + align.T.ToString("0.000"));
                    if (buvrw)
                    {
                        LogDisp(visionNo, sCamName + " OK, UVRW X1 :" + uvrw.X1.ToString("0.000") + "," + "X2 :" + uvrw.X2.ToString("0.000") + "," + "Y1 :" + uvrw.Y1.ToString("0.000") + "," + "Y2 :" + uvrw.Y2.ToString("0.000"));
                        break;
                    }
                    if (bdist)
                    {
                        if (Menu.frmSetting.revData.mBendingArm.bDisplayLRSwapBD || Menu.frmSetting.revData.mBendingArm.bDisplayLRSwapINSP)
                        {
                            LogDisp(visionNo, sCamName + " OK, LX:" + uvrw.X2.ToString("0.000") + "," + "LY :" + uvrw.Y2.ToString("0.000") + "," + "RX :" + uvrw.X1.ToString("0.000") + "," + "RY :" + uvrw.Y1.ToString("0.000"));
                        }
                        else
                        {
                            LogDisp(visionNo, sCamName + " OK, LX:" + uvrw.X1.ToString("0.000") + "," + "LY :" + uvrw.Y1.ToString("0.000") + "," + "RX :" + uvrw.X2.ToString("0.000") + "," + "RY :" + uvrw.Y2.ToString("0.000"));
                        }
                        break;
                    }
                    if (!align.Equals(default(cs2DAlign.ptAlignResult)))
                    {
                        LogDisp(visionNo, sCamName + " OK, X:" + align.X.ToString("0.000") + ", Y: " + align.Y.ToString("0.000") + ", T: " + align.T.ToString("0.000"));
                        break;
                    }
                    //LogDisp(visionNo, sCamName + "OK");
                    break;
                case ePCResult.BY_PASS: //영문을 모르는 바이패스처리..
                    LogDisp(visionNo, "BY Pass");
                    break;
                case ePCResult.ALIGN_LIMIT:
                    if (bRYT) LogDisp(visionNo, sCamName + "Limit NG R:" + align.X.ToString("0.000") + ", Y: " + align.Y.ToString("0.000") + ", T: " + align.T.ToString("0.000"));
                    else LogDisp(visionNo, sCamName + "Limit NG X:" + align.X.ToString("0.000") + ", Y: " + align.Y.ToString("0.000") + ", T: " + align.T.ToString("0.000"));
                    break;
                case ePCResult.SPEC_OVER:
                    LogDisp(visionNo, "SPEC over");
                    break;
                case ePCResult.CHECK:
                    //LogDisp(visionNo, "Error Inspection");
                    break;
                case ePCResult.WORKER_BY_PASS:
                    LogDisp(visionNo, "Worker Bypass NG");
                    break;
                case ePCResult.MANUAL_BENDING:
                    LogDisp(visionNo, "Manual Bending");
                    break;
                case ePCResult.INIT:
                    LogDisp(visionNo, "Error Vision Initial");
                    break;
                case ePCResult.RETRY_OVER:
                    LogDisp(visionNo, "Retry Over NG " + sCamName + "/ [" + Vision[visionNo].CFG.RetryLimitCnt + "]");
                    break;
                case ePCResult.PANEL_SHIFT_NG:
                    //if (nameof(CamNo) == eCamNO4.Inspection1.ToString()) LogDisp(visionNo, CamName + "NG LX:" + uvrw.X1.ToString("0.000") + "," + "LY :" + uvrw.Y1.ToString("0.000") + "," + "RX :" + uvrw.X2.ToString("0.000") + "," + "RY :" + uvrw.Y2.ToString("0.000"));
                    LogDisp(visionNo, "Panel Shift NG");
                    break;
                case ePCResult.RETRY:
                    LogDisp(visionNo, sCamName + "Retry X:" + align.X.ToString("0.000") + "(" + xyt.X.ToString("0.000") + ")" + ", Y: " + align.Y.ToString("0.000") + "(" + xyt.Y.ToString("0.000") + ")" + ", T: " + align.T.ToString("0.000") + "(" + xyt.T.ToString("0.000") + ")");
                    if (buvrw) LogDisp(visionNo, sCamName + "Retry UVRW X1 :" + uvrw.X1.ToString("0.000") + "," + "X2 :" + uvrw.X2.ToString("0.000") + "," + "Y1 :" + uvrw.Y1.ToString("0.000") + "," + "Y2 :" + uvrw.Y2.ToString("0.000"));
                    break;
                case ePCResult.ERROR_MARK:
                    LogDisp(visionNo, "Find Mark NG");
                    break;
                case ePCResult.FIRST_LIMIT:
                    LogDisp(visionNo, "Bending First Limit NG");
                    break;
                case ePCResult.ERROR_LCHECK:
                    LogDisp(visionNo, "LCheck NG");
                    break;
                case ePCResult.VISION_REPLY_WAIT:
                    LogDisp(visionNo, "Manual Mark");
                    //아무것도 안함
                    break;
            }
            if (!(CamName == nameof(eCamNO.Laser1) || CamName == nameof(eCamNO.Laser2)) || bdist || iPCResult != ePCResult.OK)
                SetOKNGCount2(CamName, iPCResult, visionNo);
        }
        private List<bool> FindThread(PParam param, short visionNo1, bool capture1, out List<cs2DAlign.ptXYT> pt, out List<bool> result, bool imgProcess = false)
        {
            //추후 csvision을 그룹핑하면 visionno를 배열로 받아서 처리하면 좋을듯
            //여러 카메라에 한꺼번에 찾기 시도
            int iCapcnt = param.qkind.Count;

            BlockingCollection<patternSearchResult>[] bc = new BlockingCollection<patternSearchResult>[iCapcnt];
            pt = new List<cs2DAlign.ptXYT>();
            result = new List<bool>();

            for (int i = 0; i < iCapcnt; i++)
            {
                int no = visionNo1 + i;
                if (param.oneCamCapture)
                {
                    no = visionNo1;
                    if (i == 0)
                    {
                        Vision[no].SetLightExpCont(param.qkind.Peek());

                        Vision[no].Capture(false, true, false, true); //1cam 2마크찾기일때는 캡쳐를 밖에서한번 함 두번째 패턴찾기가 이미지 갱신전에 찾음.

                    }
                }
                else if (param.use1Cap2Find)
                {
                    no = (int)i / 2;
                    if (i % 2 == 0) capture1 = true;
                    else if (i % 2 == 1) capture1 = false;
                }

                bc[i] = new BlockingCollection<patternSearchResult>();


                CogImage8Grey img = null;
                if (imgProcess)
                {
                    img = Menu.frmRecipe.changeImage(Vision[no].CFG.eCamName, (CogImage8Grey)Vision[no].cogDS.Image);
                }
                if (param.oneCamCapture) Vision[no].PatternSearch_Thread(param, i, bc[i], false, param.qkind.Dequeue(), img);
                else Vision[no].PatternSearch_Thread(param, i, bc[i], capture1, param.qkind.Dequeue(), img);
            }

            for (int i = 0; i < iCapcnt; i++)
            {
                cs2DAlign.ptXYT temp = new cs2DAlign.ptXYT();
                bc[i].TryTake(out patternSearchResult rvalue1, -1);
                temp.X = rvalue1.dx;
                temp.Y = rvalue1.dy;
                temp.T = rvalue1.dr;
                result.Add(rvalue1.result);
                pt.Add(temp);
                bc[i].Dispose();

                #region CHECK LOG .......... 20201103 ith add for check
                //if (param.oneCamCapture)
                {
                    if (DateTime.Now.ToString("yyyyMMdd") == "20201103")
                    {
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "...[" + i.ToString() + "] PatternSearch_Thread() end...");
                    }
                }
                #endregion
            }

            return result;
        }
        private void SetFindResult(List<cs2DAlign.ptXYT> ptinput, List<bool> binput, ref bool b1, ref cs2DAlign.ptXYT pt1)
        {
            //1개용
            b1 = binput[0];
            pt1 = ptinput[0];
        }
        private void SetFindResult(List<cs2DAlign.ptXYT> ptinput, List<bool> binput, ref bool b1, ref bool b2, ref cs2DAlign.ptXYT pt1, ref cs2DAlign.ptXYT pt2)
        {
            //2개용
            b1 = binput[0];
            pt1 = ptinput[0];
            b2 = binput[1];
            pt2 = ptinput[1];
        }

        //20200925 cjm SCF Inspection Result <-> Graph

        private void lbResultTitle_Click(object sender, EventArgs e)
        {
            string lcText = (sender as Label).Name;
            for (short j = 0; j < lbResultTitle.Length; j++)
            {
                if (lcText == lbResultTitle[j].Name)
                {
                    DataGridView dgv = null;
                    DataGridView dgvSpec = null;
                    if (j == 0)
                    {
                        dgv = dgvResult1;
                        dgvSpec = dgvSpec1;
                    }
                    else if (j == 1)
                    {
                        dgv = dgvResult2;
                        dgvSpec = dgvSpec2;
                    }
                    else if (j == 2)
                    {
                        dgv = dgvResult3;
                        dgvSpec = dgvSpec3;
                    }
                    else if (j == 3)
                    {
                        dgv = dgvResult4;
                        dgvSpec = dgvSpec4;
                    }
                    if (lbloffset[j].Text == "CPK")
                    {
                        double[] dCPK = new double[dgvSpec.Columns.Count];
                        for (int i = 0; i < dgvSpec.Columns.Count; i++)
                        {
                            double tor = Convert.ToDouble(dgvSpec[i, 1].Value) - Convert.ToDouble(dgvSpec[i, 0].Value);
                            if (lbResultTitle[j].Text == "INSPECTION") dCPK[i] = AutoMainLogDatacalculator(ResultDistInsp[i], Convert.ToDouble(dgvSpec[i, 1].Value), tor);
                            else dCPK[i] = AutoMainLogDatacalculator(ResultDist[i], Convert.ToDouble(dgvSpec[i, 1].Value), tor);
                        }

                        if (dgvSpec.InvokeRequired)
                        {
                            dgvSpec.Invoke(new MethodInvoker(delegate
                            {
                                for (int i = 0; i < dCPK.Length; i++)
                                {
                                    dgvSpec[i, 3].Value = dCPK[i].ToString("0.000");
                                }
                            }));
                        }
                        else
                        {
                            for (int i = 0; i < dCPK.Length; i++)
                            {
                                dgvSpec[i, 3].Value = dCPK[i].ToString("0.000");
                            }
                        }
                    }
                }
            }
        }

        private void btnCPK_Click(object sender, EventArgs e)
        {
            //카운트 리셋필요. 보고변수 필요
            if (MessageBox.Show("ALL CPK Reset Are you Sure", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                for (int i = 0; i < ResultDist.Length; i++)
                {
                    for (int j = 0; j < ResultDist[i].Length; j++)
                    {
                        ResultDist[i][j] = 0;
                    }
                }
                CalculCnt1 = 0;
                for (int i = 0; i < ResultDistInsp.Length; i++)
                {
                    for (int j = 0; j < ResultDistInsp[i].Length; j++)
                    {
                        ResultDistInsp[i][j] = 0;
                    }
                }
                CalculCnt = 0;
                bSendCPK = false;
            }
            DL.SendData("CONNECT," + CONST.PCNo);
        }

        private void button1_Click_1(object sender, EventArgs e)
        { 
            if (comboBox1.SelectedIndex < 0) comboBox1.SelectedIndex = 0;
            int selectI = comboBox1.SelectedIndex;
            CONST.bPLCReq[selectI] = true;

        }

        //20.12.17 lkw DL
        private int DL_ConnectionNOMS(short visionNO, int index)
        {
            return visionNO * Menu.frmSetting.revData.mDL[visionNO].MarkSearch_Use.Length + index;
        }
        private int DL_ConnectionNODF(short visionNO, int index)
        { 
            return visionNO * Menu.frmSetting.revData.mDL[visionNO].DefectFind_Use.Length + index;
        }
        private void button1_Click_2(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex < 0) comboBox1.SelectedIndex = 0;
            int selectI = comboBox1.SelectedIndex;
            CONST.bPLCReq[selectI] = false;
        }

        private void ctDisp1_1_Click(object sender, EventArgs e)
        {

        }

        private void cbLive3_CheckedChanged(object sender, EventArgs e)
        {

        }

        //tt insp result display
        private void CogLabeResultlDisplay(cs2DAlign.ptXXYY dist, int camNum, sSpec SpecX, sSpec SpecY, bool bOneCam = false, bool insp = false)
        {
            CogGraphicLabel[] cogLabel = new CogGraphicLabel[4];
            float fontSize = 12;
            if (insp) fontSize = 15;
            for (int i = 0; i < cogLabel.Length; i++)
            {
                cogLabel[i] = new CogGraphicLabel();
                cogLabel[i].Color = CogColorConstants.Black;
                cogLabel[i].Font = CONST.GetFontBold(fontSize);
                cogLabel[i].BackgroundColor = CogColorConstants.Yellow;
                if (i < 2) cogLabel[i].Alignment = CogGraphicLabelAlignmentConstants.BaselineLeft;
                else cogLabel[i].Alignment = CogGraphicLabelAlignmentConstants.BaselineRight;

            }

            //position label
            if (bOneCam)
            {
                cogLabel[0].SetXYText(100, Vision[camNum].cogDS.Image.Height - 250, "LX: " + dist.X1.ToString("0.000"));
                cogLabel[1].SetXYText(100, Vision[camNum].cogDS.Image.Height - 100, "LY: " + dist.Y1.ToString("0.000"));
                cogLabel[2].SetXYText(Vision[camNum].cogDS.Image.Width - 100, Vision[camNum].cogDS.Image.Height - 250, "RX: " + dist.X2.ToString("0.000"));
                cogLabel[3].SetXYText(Vision[camNum].cogDS.Image.Width - 100, Vision[camNum].cogDS.Image.Height - 100, "RY: " + dist.Y2.ToString("0.000"));
            }
            else
            {
                if (Menu.frmSetting.revData.mBendingArm.bDisplayLRSwapINSP)
                {
                    cogLabel[0].SetXYText(50, Vision[camNum].cogDS.Image.Height - 350, "RX: " + dist.X1.ToString("0.000"));
                    cogLabel[1].SetXYText(50, Vision[camNum].cogDS.Image.Height - 100, "RY: " + dist.Y1.ToString("0.000"));
                    cogLabel[2].SetXYText(Vision[camNum + 1].cogDS.Image.Width - 50, Vision[camNum + 1].cogDS.Image.Height - 350, "LX: " + dist.X2.ToString("0.000"));
                    cogLabel[3].SetXYText(Vision[camNum + 1].cogDS.Image.Width - 50, Vision[camNum + 1].cogDS.Image.Height - 100, "LY: " + dist.Y2.ToString("0.000"));
                }
                else
                {
                    cogLabel[0].SetXYText(50, Vision[camNum].cogDS.Image.Height - 350, "LX: " + dist.X1.ToString("0.000"));
                    cogLabel[1].SetXYText(50, Vision[camNum].cogDS.Image.Height - 100, "LY: " + dist.Y1.ToString("0.000"));
                    cogLabel[2].SetXYText(Vision[camNum + 1].cogDS.Image.Width - 50, Vision[camNum + 1].cogDS.Image.Height - 350, "RX: " + dist.X2.ToString("0.000"));
                    cogLabel[3].SetXYText(Vision[camNum + 1].cogDS.Image.Width - 50, Vision[camNum + 1].cogDS.Image.Height - 100, "RY: " + dist.Y2.ToString("0.000"));
                }
            }

            //make color label NG = RED, OK = YELLOW
            if (dist.X1 > SpecX.Upper1 || dist.X1 < SpecX.Lower1)
            {
                cogLabel[0].BackgroundColor = CogColorConstants.Red;
            }
            if (dist.Y1 > SpecY.Upper1 || dist.Y1 < SpecY.Lower1)
            {
                cogLabel[1].BackgroundColor = CogColorConstants.Red;
            }
            if (dist.X2 > SpecX.Upper2 || dist.X2 < SpecX.Lower2)
            {
                cogLabel[2].BackgroundColor = CogColorConstants.Red;
            }
            if (dist.Y2 > SpecY.Upper2 || dist.Y2 < SpecY.Lower2)
            {
                cogLabel[3].BackgroundColor = CogColorConstants.Red;
            }

            //display
            for (int i = 0; i < cogLabel.Length; i++)
            {
                if (i < 2)
                    Vision[camNum].cogDS.InteractiveGraphics.Add(cogLabel[i], "InspResult", false);
                else
                {
                    if (!bOneCam) Vision[camNum + 1].cogDS.InteractiveGraphics.Add(cogLabel[i], "InspResult", false);
                    else Vision[camNum].cogDS.InteractiveGraphics.Add(cogLabel[i], "InspResult", false);
                }
            }

        }

        private void tmrDispChart_Tick(object sender, EventArgs e)
        {
            bool[] bDrawPoint = CONST.DispChart.bDrawPoint;
            for (int i = 0; i < bDrawPoint.Length; i++)
            {
                if (bDrawPoint[i])
                {
                    DrawPoint(i);
                    bDrawPoint[i] = false;
                    break;
                }
            }
        }

        //tt210504 Double click result chart to full screen
        private TabPage tabParent = null;
        private Chart selectChart = null;
        private Point pChart;
        private Size sizeChart;
        private void Chart_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Chart chart = sender as Chart;
            if (tabParent == null)
            {
                tabParent = chart.Parent as TabPage;
                selectChart = chart;
                sizeChart = chart.Size;
                pChart = chart.Location;

                tabParent.Controls.Remove(chart);

                this.Controls.Add(chart);

                int XMargin = 100;
                int YMargin = 50;

                chart.Location = new Point(XMargin, YMargin);
                chart.Size = new Size(this.Width - XMargin * 2, this.Bottom - YMargin * 2);
                this.Controls.SetChildIndex(chart, 0);
            }
            else
            {
                if (selectChart.Equals(chart) == false) return;
                this.Controls.Remove(chart);
                chart.Location = pChart;
                chart.Size = sizeChart;
                tabParent.Controls.Add(chart);
                tabParent = null;
            }
        }
    }

    public class patternSearchResult
    {
        public bool result = false;
        public double dx = 0.0d;
        public double dy = 0.0d;
        public double dr = 0.0d;

        public patternSearchResult(bool result, double dx, double dy, double dr)
        {
            this.result = result;
            this.dx = dx;
            this.dy = dy;
            this.dr = dr;
        }
    }

    public class lineSearchResult
    {
        public bool result = false;
        public CogLine line;

        public lineSearchResult(bool result, CogLine line)
        {
            this.result = result;
            this.line = line;
            //this.line = new CogLine(line);
        }
    }
    public class PParam
    {
        public bool LineCreate { get; set; }
        public bool bNographics { get; set; }
        public bool failimagenotsave { get; set; }
        public bool oneCamCapture { get; set; }
        public Queue<ePatternKind> qkind { get; set; }
        public Queue<eLineKind> qLkind { get; set; }
        public bool useGrapDelay2 { get; set; }
        public bool use1Cap2Find { get; set; }

        public PParam()
        {
            qkind = new Queue<ePatternKind>();
            qLkind = new Queue<eLineKind>();
        }
    }
}