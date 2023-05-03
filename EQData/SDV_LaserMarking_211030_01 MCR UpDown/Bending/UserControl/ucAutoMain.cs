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

        //public bool BendingPreInspCheck1;
        //public bool BendingPreInspCheckPopup1;
        //public bool BendingPreInspCheck2;
        //public bool BendingPreInspCheckPopup2;

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

                ucSetting.cCFG.CAMconfig_Read(i, ref Vision[i].CFG); //ucSetting.DB.getCAMConfig(i);
                                                                     //Vision[i].CamCal = ucSetting.DB.getCamCal(i);

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
                        //pnInfo[Num].Visible = false;
                    }
                }
            }
            SetPanelIDAddress(ref Vision, CONST.PCNo);
            if (CONST.PCNo == 1)
            {

            }

            pnAPC.Visible = false;
            pnHeightDisp.Visible = false;
            //20201005 cjm Height Insp 위치 변경
            //pcy201009 에러나서 주석처리함 고쳐주세요
            // 20201010 cjm 수정했습니다.
            // 210129 cjm heightINSP 사용 X
            //if (CONST.PCNo == 4)
            //{
            //    //pnHeightDisp.Size = new Size(674, 276);
            //    //pnHeightDisp.Visible = true;
            //    ////pcy200706
            //    //pnView[4].Visible = true;
            //    //pnInfo[2].Visible = true;
            //    //lbTitle[4].Text = "Height Inspection";

            //    pnHeightDisp.Size = new Size(337, 415);
            //    pictureBox1.Size = new Size(337, 276);
            //    pn8.Visible = true;
            //    pn8.Size = new Size(337, 555);
            //    pnHeightDisp.Visible = true;

            //    lbTitle[7].Text = "Height Inspection";

            //    pnInfo5.Location = new Point(1, 277);

            //    pnInfo5.Size = pnInfo4.Size;
            //    lblPPID5.Size = lblPPID4.Size;
            //    lblCycleTime5.Size = lblCycleTime4.Size;
            //    lblRetryCount5.Size = lblRetryCount4.Size;
            //    tabControl5.Size = tabControl4.Size;
            //    listBox5.Size = listBox4.Size;
            //    dgvAlign5.Size = dgvAlign4.Size;
            //    dgvError5.Size = dgvError4.Size;

            //    lblCycleTime5.Location = new Point(lblPPID5.Size.Width + lblPPID5.Location.X, lblCycleTime5.Location.Y);
            //    lblRetryCount5.Location = new Point(lblCycleTime5.Size.Width + lblCycleTime5.Location.X, lblRetryCount5.Location.Y);
            //}


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
            //int dl = DL.Connection(false);

            //if (dl != 0)
            //{
            //    cLog.Save(LogKind.System, "Confirm DLFTP (" + dl.ToString() + ")");
            //    MessageBox.Show("Confirm DLFTP (" + dl.ToString() + ")");
            //}
            //else
            //{
            //    DL.SendData("CONNECT," + CONST.PCNo);
            //}

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
                            //cbLive[i].Visible = true;
                            cbSideImageSize[i].Visible = true;
                        }

                        //Vision[i].GetPattern(); // 추후 해제
                    }
                }
                //if (CONST.PCName == "Insp")
                //{
                //    //2020.04.23 Height Sensor Spec Read
                //    HeightSpecRead(1);
                //}

                this.mainThread = new Thread(new ThreadStart(Idle));
                mainThread.Start();

                this.autoMainThread = new Thread(new ThreadStart(AutoMainProcess));
                autoMainThread.Start();

                IF.setReqStart();
            }


            if (CONST.RunRecipe.RecipeName == "" || CONST.RunRecipe.RecipeName == null)
            {
                //CONST.RunRecipe.RecipeName = "TEST";
                CONST.RunRecipe.RecipeName = CONST.RunRecipe.OldRecipeName;
            }

            bPCRep[pcAutoManual] = true;  // 초기 Bit Clear위해

            DispTimer.Tick += new EventHandler(DispTimer_Unit_Tick);
            //Type pcBitContol = null;
            //if (CONST.PCNo == 1)
            //{
            //    pcBitContol = typeof(CONST.AAM_PLC1.BitControl);
            //    DispTimer.Tick += new EventHandler(DispTimer_Unit1_Tick);

            //    //pnResult2.Visible = false;
            //}
            //else if (CONST.PCNo == 2)
            //{
            //    pcBitContol = typeof(CONST.AAM_PLC1.BitControl);
            //    DispTimer.Tick += new EventHandler(DispTimer_Unit2_Tick);
            //}
            //else if (CONST.PCNo == 3)
            //{
            //    pcBitContol = typeof(CONST.AAM_PLC2.BitControl);
            //    DispTimer.Tick += new EventHandler(DispTimer_Unit3_Tick);
            //}
            //else if (CONST.PCNo == 4)
            //{
            //    pcBitContol = typeof(CONST.AAM_PLC2.BitControl);
            //    DispTimer.Tick += new EventHandler(DispTimer_Unit4_Tick);
            //}

            //if (pcBitContol == null)
            //{
            //    cLog.Save(LogKind.System, "ucAutoMain 에서 pcBitControl 이 null 입니다.");
            //    MessageBox.Show("ucAutoMain 에서 pcBitControl 이 null 입니다.");
            //}

            //this.bitControls = pcBitContol.GetFields();
            //int len = bitControls.Length;
            //bitControlsCount = len; // lyw. 임시.

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

            //의미 모르겠음
            //int SideImageSizeCnt = 1;
            //for (int i = 0; i < cbLive.Length; i++)
            //{
            //    if (cbLive[i].Visible)
            //    {
            //        cbLive[i].Checked = true;
            //        cbSideImageSize[i].Visible = true;

            //        if (SideImageSizeCnt == 1)
            //        {
            //            CONST.bSideImageSize[0] = i;
            //            SideImageSizeCnt++;
            //        }
            //        else if (SideImageSizeCnt == 2)
            //        {
            //            CONST.bSideImageSize[1] = i;
            //            SideImageSizeCnt = 1;
            //        }
            //    }
            //}

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

        public void HeightSpecRead(int dispCnt)
        {
            DataGridView dgv = null;
            DataGridView dgvSpec = null;
            try
            {
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
                //2020.04.04 Height Sensor Spec Read
                Menu.Config.sHeight = Menu.Config.HeightReadPara();
                Menu.Config.HeightResult = new double[Menu.Config.sHeight.Length, 30]; //

                dgv.Columns.Clear();
                dgvSpec.Columns.Clear();
                for (int i = 0; i < Menu.Config.sHeight.Length; i++)
                {
                    dgv.Columns.Add("", Menu.Config.sHeight[i].HeightReference);
                    dgv.Columns[i].Width = Menu.Config.sHeight[i].HeightReference.Length * 2 + 55;
                    dgvSpec.Columns.Add("", "");
                    dgvSpec.Columns[i].Width = 45;
                    //dgvResult1.Rows.Add(sHeight[i].HeightReference, sHeight[i].HeightSpec, sHeight[i].HeightTolerence);
                }
                dgv.Columns.Add("", "ToolNo");
                dgv.Columns.Add("", "PanelID");
                DgvheightSpec_Write(dgv, dgvSpec);
            }
            catch
            {
            }
        }

        private bool DgvheightSpec_Write(DataGridView dgv, DataGridView dgvSpec)
        {
            bool bResult = true;

            try
            {
                dgvSpec.Rows.Clear();

                dgvSpec.Rows.Add(4);

                for (int i = 0; i < Menu.Config.sHeight.Length; i++)
                {
                    dgvSpec[i, 0].Value = double.Parse(Menu.Config.sHeight[i].HeightSpec) - double.Parse(Menu.Config.sHeight[i].HeightTolerence);
                    dgvSpec[i, 1].Value = Menu.Config.sHeight[i].HeightSpec;
                    dgvSpec[i, 2].Value = double.Parse(Menu.Config.sHeight[i].HeightSpec) + double.Parse(Menu.Config.sHeight[i].HeightTolerence);
                }
            }
            catch
            {
                bResult = false;
                return bResult;
            }

            return bResult;
        }
        // 20200918 cjm Bending Pre에서 SCF Inspection 실행 후 History 작성
        public void BendingPreSCFInspection()
        {
            //크기변경
            gbBendResult1.Size = new Size(528, 427);
            dgvResult1.Size = new Size(gbBendResult1.Width - 10, dgvResult1.Height);
            label1.Location = new Point(dgvResult1.Width - label1.Width, label1.Location.Y);
            label2.Location = new Point(dgvResult1.Width - label2.Width, label2.Location.Y);
            label3.Location = new Point(dgvResult1.Width - label3.Width, label3.Location.Y);
            lblOFFSET1.Location = new Point(dgvResult1.Width - lblOFFSET1.Width, lblOFFSET1.Location.Y);


            //
            dgvResult1.Columns.Clear();
            dgvResult1.Columns.Add("Column0", "X1 L");
            dgvResult1.Columns.Add("Column1", "Y1 L");

            dgvResult1.Columns.Add("Column2", "X2 R");
            dgvResult1.Columns.Add("Column3", "Y2 R");

            dgvResult1.Columns.Add("Column4", "X3");
            dgvResult1.Columns.Add("Column5", "Y3");
            dgvResult1.Columns.Add("Column0", "Index NO");
            dgvResult1.Columns.Add("Column6", "PanelID          ");

            for (int i = 0; i < dgvResult1.ColumnCount; i++)
            {
                dgvResult1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                //dgvResult1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                //int widthCol = dgvResult1.Columns[i].Width;
                //dgvResult1.Columns[i].Width = widthCol;
            }
            dgvResult1.Columns[0].Width = 50;
            dgvResult1.Columns[1].Width = 50;
            dgvResult1.Columns[2].Width = 50;
            dgvResult1.Columns[3].Width = 50;
            dgvResult1.Columns[4].Width = 50;
            dgvResult1.Columns[5].Width = 50;
            dgvResult1.Columns[6].Width = 50;
            dgvResult1.Columns[7].Width = 150;
            //dgvSpec1.Size = new Size(dgvResult1.Width - label1.Width - 50, dgvSpec1.Height);
            dgvSpec1.Size = new Size(dgvResult1.Columns[0].Width * 6, dgvSpec1.Height);
            //dgvSpec1.Columns.Clear();
            //dgvSpec1.Columns.Add("Column1", "X1");
            //dgvSpec1.Columns.Add("Column2", "Y1");
            //dgvSpec1.Columns.Add("Column3", "X2");
            //dgvSpec1.Columns.Add("Column4", "Y2");
            //dgvSpec1.Columns.Add("Column5", "X3");
            //dgvSpec1.Columns.Add("Column6", "Y3");

            //for (int i = 0; i < dgvSpec1.ColumnCount; i++)
            //{
            //    dgvSpec1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            //    int widthCol = dgvSpec1.Columns[i].Width;
            //    dgvSpec1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            //    dgvSpec1.Columns[i].Width = widthCol;
            //}

            //SCFInspectionSpec_Write(dgvSpec1);
        }

        // 20200918 cjm Bending Pre에서 SCF Inspection 실행 후 History 작성
        // 20200926 cjm Bending Pre에서 SCF Inspection 실행 후 History 작성 기능 수정
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
                //Cam1
                //dgvSpec[0, 0].Value = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X1_SPEC] - Tolerance;
                //dgvSpec[0, 1].Value = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X1_SPEC];
                //dgvSpec[0, 2].Value = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X1_SPEC] + Tolerance;
                ////dgvSpec[0, 4].Value = Menu.frmSetting.revData.mBendingPre.SCFInspCam1OffsetX1;

                //dgvSpec[1, 0].Value = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y1_SPEC] - Tolerance;
                //dgvSpec[1, 1].Value = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y1_SPEC];
                //dgvSpec[1, 2].Value = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y1_SPEC] + Tolerance;
                ////dgvSpec[1, 4].Value = Menu.frmSetting.revData.mBendingPre.SCFInspCam1OffsetY1;
                ////Cam2
                //dgvSpec[2, 0].Value = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X1_SPEC] - Tolerance;
                //dgvSpec[2, 1].Value = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X1_SPEC];
                //dgvSpec[2, 2].Value = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X1_SPEC] + Tolerance;
                ////dgvSpec[2, 4].Value = Menu.frmSetting.revData.mBendingPre.SCFInspCam1OffsetX2;

                //dgvSpec[3, 0].Value = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y1_SPEC] - Tolerance;
                //dgvSpec[3, 1].Value = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y1_SPEC];
                //dgvSpec[3, 2].Value = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y1_SPEC] + Tolerance;
                ////dgvSpec[3, 4].Value = Menu.frmSetting.revData.mBendingPre.SCFInspCam1OffsetY2;
                ////Cam2
                //dgvSpec[4, 0].Value = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X2_SPEC] - Tolerance;
                //dgvSpec[4, 1].Value = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X2_SPEC];
                //dgvSpec[4, 2].Value = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X2_SPEC] + Tolerance;
                ////dgvSpec[4, 4].Value = Menu.frmSetting.revData.mBendingPre.SCFInspCam2OffsetX1;

                //dgvSpec[5, 0].Value = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y2_SPEC] - Tolerance;
                //dgvSpec[5, 1].Value = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y2_SPEC];
                //dgvSpec[5, 2].Value = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y2_SPEC] + Tolerance;
                ////dgvSpec[5, 4].Value = Menu.frmSetting.revData.mBendingPre.SCFInspCam2OffsetY1;

                //dgvSpec[5, 0].Value = CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X4] - CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X4];
                //dgvSpec[5, 1].Value = CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X4];
                //dgvSpec[5, 2].Value = CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X4] + CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X4];
                //dgvSpec[5, 4].Value = Menu.frmSetting.revData.mBendingPre.SCFInspCam2OffsetX2;

                //dgvSpec[6, 0].Value = CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_Y4] - CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X4];
                //dgvSpec[6, 1].Value = CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_Y4];
                //dgvSpec[6, 2].Value = CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_Y4] + CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X4];
                //dgvSpec[6, 4].Value = Menu.frmSetting.revData.mBendingPre.SCFInspCam2OffsetY2;
                //Cam3
                //dgvSpec[7, 0].Value = CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X5] - CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X4];
                //dgvSpec[7, 1].Value = CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X5];
                //dgvSpec[7, 2].Value = CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X5] + CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X4];
                //dgvSpec[7, 4].Value = Menu.frmSetting.revData.mBendingPre.SCFInspCam2OffsetX1;

                //dgvSpec[8, 0].Value = CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_Y6] - CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X4];
                //dgvSpec[8, 1].Value = CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_Y6];
                //dgvSpec[8, 2].Value = CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_Y6] + CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X4];
                //dgvSpec[8, 4].Value = Menu.frmSetting.revData.mBendingPre.SCFInspCam2OffsetY1;

                //dgvSpec[9, 0].Value = CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X7] - CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X4];
                //dgvSpec[9, 1].Value = CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X7];
                //dgvSpec[9, 2].Value = CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X7] + CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X4];
                //dgvSpec[9, 4].Value = Menu.frmSetting.revData.mBendingPre.SCFInspCam2OffsetX2;

                //dgvSpec[10, 0].Value = CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_Y8] - CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X4];
                //dgvSpec[10, 1].Value = CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_Y8];
                //dgvSpec[10, 2].Value = CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_Y8] + CONST.RunRecipe.Param[eRecipe.SCF_ATTACH_INSPECTION_SPEC_X4];
                //dgvSpec[10, 4].Value = Menu.frmSetting.revData.mBendingPre.SCFInspCam2OffsetY2;
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
            //pnResult1.Visible = false;
            //pnResult2.Visible = false;
            gbBendResult1.Visible = false;
            gbBendResult2.Visible = false;
            gbBendResult3.Visible = false;
            gbBendResult4.Visible = false;
        }

        public void ResultSpecDisplay(int DispCnt, int CamNo)
        {
            try
            {
                //ResultDisplayInit();
                DataGridView dgvSpec = null; //(DispCnt == 0) ? dgvSpec1 : dgvSpec2;

                string strTemp = "";
                //int CamNo = 0;
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


                //bool[] BdAlignEdgeMode = new bool[5];

                //if (DispCnt != 3 || DispCnt != 4) BdAlignEdgeMode[DispCnt] = Menu.frmSetting.revData.mBendingArm.bBDEdgeToEdgeUse[DispCnt];

                dgvSpec.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                if (strTemp.Contains("BD1"))
                {
                    if (strTemp.Contains("1"))
                    {
                        APCOffset.Y1 = CONST.RunRecipe.Param[eRecipe.BENDING_OFFSET_LY];
                        APCOffset.Y2 = CONST.RunRecipe.Param[eRecipe.BENDING_OFFSET_RY];
                    }
                    //else if(strTemp.Contains("2"))
                    //{
                    //    APCOffset.Y1 = CONST.RunRecipe.Param[eRecipe.BENDING2_OFFSET_LY];
                    //    APCOffset.Y2 = CONST.RunRecipe.Param[eRecipe.BENDING2_OFFSET_RY];
                    //}
                    //else if(strTemp.Contains("3"))
                    //{
                    //    APCOffset.Y1 = CONST.RunRecipe.Param[eRecipe.BENDING3_OFFSET_LY];
                    //    APCOffset.Y2 = CONST.RunRecipe.Param[eRecipe.BENDING3_OFFSET_RY];
                    //}
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
                    #region Attach
                    //if (CamNo == Vision_No.Attach1_1)
                    //{
                    //    dgvSpec[0, 0].Value = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_LX] - ToleranceX;
                    //    dgvSpec[0, 1].Value = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_LX];
                    //    dgvSpec[0, 2].Value = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_LX] + ToleranceX;
                    //    //dgvSpec[0, 3].Value = Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X1;

                    //    dgvSpec[1, 0].Value = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_LY] - ToleranceY + insp;
                    //    dgvSpec[1, 1].Value = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_LY] + insp;
                    //    dgvSpec[1, 2].Value = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_LY] + ToleranceY + insp;
                    //    //dgvSpec[1, 3].Value = APCOffset.Y1 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y1;

                    //    dgvSpec[2, 0].Value = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_RX] - ToleranceX;
                    //    dgvSpec[2, 1].Value = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_RX];
                    //    dgvSpec[2, 2].Value = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_RX] + ToleranceX;
                    //    //dgvSpec[2, 3].Value = Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X2;

                    //    dgvSpec[3, 0].Value = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_RY] - ToleranceY + insp;
                    //    dgvSpec[3, 1].Value = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_RY] + insp;
                    //    dgvSpec[3, 2].Value = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_RY] + ToleranceY + insp;
                    //    //dgvSpec[3, 3].Value = APCOffset.Y2 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y2;
                    //}
                    //else if (CamNo == Vision_No.Attach2_1)
                    //{
                    //    dgvSpec[0, 0].Value = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_LX] - ToleranceX;
                    //    dgvSpec[0, 1].Value = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_LX];
                    //    dgvSpec[0, 2].Value = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_LX] + ToleranceX;
                    //    //dgvSpec[0, 3].Value = Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X1;

                    //    dgvSpec[1, 0].Value = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_LY] - ToleranceY + insp;
                    //    dgvSpec[1, 1].Value = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_LY] + insp;
                    //    dgvSpec[1, 2].Value = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_LY] + ToleranceY + insp;
                    //    //dgvSpec[1, 3].Value = APCOffset.Y1 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y1;

                    //    dgvSpec[2, 0].Value = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_RX] - ToleranceX;
                    //    dgvSpec[2, 1].Value = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_RX];
                    //    dgvSpec[2, 2].Value = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_RX] + ToleranceX;
                    //    //dgvSpec[2, 3].Value = Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.X2;

                    //    dgvSpec[3, 0].Value = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_RY] - ToleranceY + insp;
                    //    dgvSpec[3, 1].Value = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_RY] + insp;
                    //    dgvSpec[3, 2].Value = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_RY] + ToleranceY + insp;
                    //    //dgvSpec[3, 3].Value = APCOffset.Y2 + Menu.frmSetting.revData.mOffset[CamNo].LastOffsetYY.Y2;
                    //}
                    #endregion
                }
                else if (strTemp == "HEIGHT")
                {
                    //dgvSpec[0, 0].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_A1].Value) - double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_SPEC].Value)).ToString("0.000"));
                    //dgvSpec[0, 1].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_A1].Value)));
                    //dgvSpec[0, 2].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_A1].Value) + double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_SPEC].Value)).ToString("0.000"));

                    //dgvSpec[1, 0].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_B1].Value) - double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_SPEC].Value)).ToString("0.000"));
                    //dgvSpec[1, 1].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_B1].Value)));
                    //dgvSpec[1, 2].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_B1].Value) + double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_SPEC].Value)).ToString("0.000"));

                    //dgvSpec[2, 0].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_C1].Value) - double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_SPEC].Value)).ToString("0.000"));
                    //dgvSpec[2, 1].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_C1].Value)));
                    //dgvSpec[2, 2].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_C1].Value) + double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_SPEC].Value)).ToString("0.000"));

                    //dgvSpec[3, 0].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_A2].Value) - double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_SPEC].Value)).ToString("0.000"));
                    //dgvSpec[3, 1].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_A2].Value)));
                    //dgvSpec[3, 2].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_A2].Value) + double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_SPEC].Value)).ToString("0.000"));

                    //dgvSpec[4, 0].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_B2].Value) - double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_SPEC].Value)).ToString("0.000"));
                    //dgvSpec[4, 1].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_B2].Value)));
                    //dgvSpec[4, 2].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_B2].Value) + double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_SPEC].Value)).ToString("0.000"));

                    //dgvSpec[5, 0].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_C2].Value) - double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_SPEC].Value)).ToString("0.000"));
                    //dgvSpec[5, 1].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_C2].Value)));
                    //dgvSpec[5, 2].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_C2].Value) + double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_SPEC].Value)).ToString("0.000"));

                    //dgvSpec[6, 0].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_A3].Value) - double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_SPEC].Value)).ToString("0.000"));
                    //dgvSpec[6, 1].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_A3].Value)));
                    //dgvSpec[6, 2].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_A3].Value) + double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_SPEC].Value)).ToString("0.000"));

                    //dgvSpec[7, 0].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_B3].Value) - double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_SPEC].Value)).ToString("0.000"));
                    //dgvSpec[7, 1].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_B3].Value)));
                    //dgvSpec[7, 2].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_B3].Value) + double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_SPEC].Value)).ToString("0.000"));

                    //dgvSpec[8, 0].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_C3].Value) - double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_SPEC].Value)).ToString("0.000"));
                    //dgvSpec[8, 1].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_C3].Value)));
                    //dgvSpec[8, 2].Value = ((double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_INSPECTION_SPEC_C3].Value) + double.Parse(CONST.RunRecipe.Param[CONST.rcpHEIGHT_SPEC].Value)).ToString("0.000"));
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


        private ePCResult CheckRetryCnt(short visionNo, int RetryCnt)
        {
            RetryCnt++;

            if (RetryCnt > Vision[visionNo].CFG.RetryLimitCnt)
            {
                return ePCResult.RETRY_OVER;
            }
            else
            {
                return ePCResult.WAIT;
            }
        }
        private ePCResult CheckSpec(cs2DAlign.ptAlignResult align, cs2DAlign.ptXYT RetrySpec)
        {
            if (Math.Abs(align.X) > RetrySpec.X || Math.Abs(align.Y) > RetrySpec.Y || Math.Abs(align.T) > RetrySpec.T)
            {
                return ePCResult.RETRY;
            }
            else
            {
                return ePCResult.WAIT;
            }
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

        private ePCResult CompareMarkDegree(short sVisionNo, cs2DAlign.ptXYT mark)
        {
            double limit = Vision[sVisionNo].CFG.AlignLimitT;
            if (Math.Abs(mark.T) > limit) return ePCResult.ALIGN_LIMIT;
            else return ePCResult.WAIT;
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
        private ePCResult ArmPreCompareLimit(short sVisionNo, cs2DAlign.ptAlignResult align)
        {
            SetArmPreAlignLimit(sVisionNo, out double ArmPrealignXlimit, out double ArmPrealignYlimit, out double ArmPrealignTlimit);
            if (Math.Abs(align.X) > ArmPrealignXlimit || Math.Abs(align.Y) > ArmPrealignYlimit || Math.Abs(align.T) > ArmPrealignTlimit)
            {
                return ePCResult.ALIGN_LIMIT;
            }
            else
            {
                return ePCResult.WAIT;
            }

        }
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

        #region AttachDisp
        //private void AttachDisplay(short sRequest)
        //{
        //    //사용화면만 보이도록 함.
        //    if (sRequest == plcRequest.Attach1_1Initial)
        //    {
        //        if (pn3.InvokeRequired)
        //        {
        //            pn3.Invoke(new MethodInvoker(delegate
        //            {
        //                if (!pn3.Visible)
        //                {
        //                    pn3.Visible = true;
        //                }
        //            }));
        //        }
        //        else
        //        {
        //            if (!pn3.Visible)
        //            {
        //                pn3.Visible = true;
        //            }
        //        }
        //        if (pn4.InvokeRequired)
        //        {
        //            pn4.Invoke(new MethodInvoker(delegate
        //            {
        //                if (!pn4.Visible)
        //                {
        //                    pn4.Visible = true;
        //                }
        //            }));
        //        }
        //        else
        //        {
        //            if (!pn4.Visible)
        //            {
        //                pn4.Visible = true;
        //            }
        //        }
        //        if (pn5.InvokeRequired)
        //        {
        //            pn5.Invoke(new MethodInvoker(delegate
        //            {
        //                if (pn5.Visible)
        //                {
        //                    pn5.Visible = false;
        //                }
        //            }));
        //        }
        //        else
        //        {
        //            if (pn5.Visible)
        //            {
        //                pn5.Visible = false;
        //            }
        //        }
        //        if (pn6.InvokeRequired)
        //        {
        //            pn6.Invoke(new MethodInvoker(delegate
        //            {
        //                if (pn6.Visible)
        //                {
        //                    pn6.Visible = false;
        //                }
        //            }));
        //        }
        //        else
        //        {
        //            if (pn6.Visible)
        //            {
        //                pn6.Visible = false;
        //            }
        //        }
        //    }

        //    else if (sRequest == plcRequest.Attach1_2Initial)
        //    {
        //        if (pn3.InvokeRequired)
        //        {
        //            pn3.Invoke(new MethodInvoker(delegate
        //            {
        //                if (pn3.Visible)
        //                {
        //                    pn3.Visible = false;
        //                }
        //            }));
        //        }
        //        else
        //        {
        //            if (pn3.Visible)
        //            {
        //                pn3.Visible = false;
        //            }
        //        }
        //        if (pn4.InvokeRequired)
        //        {
        //            pn4.Invoke(new MethodInvoker(delegate
        //            {
        //                if (pn4.Visible)
        //                {
        //                    pn4.Visible = false;
        //                }
        //            }));
        //        }
        //        else
        //        {
        //            if (pn4.Visible)
        //            {
        //                pn4.Visible = false;
        //            }
        //        }
        //        if (pn5.InvokeRequired)
        //        {
        //            pn5.Invoke(new MethodInvoker(delegate
        //            {
        //                if (!pn5.Visible)
        //                {
        //                    pn5.Visible = true;
        //                }
        //            }));
        //        }
        //        else
        //        {
        //            if (!pn5.Visible)
        //            {
        //                pn5.Visible = true;
        //            }
        //        }
        //        if (pn6.InvokeRequired)
        //        {
        //            pn6.Invoke(new MethodInvoker(delegate
        //            {
        //                if (!pn6.Visible)
        //                {
        //                    pn6.Visible = true;
        //                }
        //            }));
        //        }
        //        else
        //        {
        //            if (!pn6.Visible)
        //            {
        //                pn6.Visible = true;
        //            }
        //        }
        //    }
        //    else if (sRequest == plcRequest.Attach2_1Initial)
        //    {
        //        if (pn1.InvokeRequired)
        //        {
        //            pn1.Invoke(new MethodInvoker(delegate
        //            {
        //                if (!pn1.Visible)
        //                {
        //                    pn1.Visible = true;
        //                }
        //            }));
        //        }
        //        else
        //        {
        //            if (!pn1.Visible)
        //            {
        //                pn1.Visible = true;
        //            }
        //        }
        //        if (pn2.InvokeRequired)
        //        {
        //            pn2.Invoke(new MethodInvoker(delegate
        //            {
        //                if (!pn2.Visible)
        //                {
        //                    pn2.Visible = true;
        //                }
        //            }));
        //        }
        //        else
        //        {
        //            if (!pn2.Visible)
        //            {
        //                pn2.Visible = true;
        //            }
        //        }
        //        if (pn3.InvokeRequired)
        //        {
        //            pn3.Invoke(new MethodInvoker(delegate
        //            {
        //                if (pn3.Visible)
        //                {
        //                    pn3.Visible = false;
        //                }
        //            }));
        //        }
        //        else
        //        {
        //            if (pn3.Visible)
        //            {
        //                pn3.Visible = false;
        //            }
        //        }
        //        if (pn4.InvokeRequired)
        //        {
        //            pn4.Invoke(new MethodInvoker(delegate
        //            {
        //                if (pn4.Visible)
        //                {
        //                    pn4.Visible = false;
        //                }
        //            }));
        //        }
        //        else
        //        {
        //            if (pn4.Visible)
        //            {
        //                pn4.Visible = false;
        //            }
        //        }
        //    }

        //    else if (sRequest == plcRequest.Attach2_2Initial)
        //    {
        //        if (pn1.InvokeRequired)
        //        {
        //            pn1.Invoke(new MethodInvoker(delegate
        //            {
        //                if (pn1.Visible)
        //                {
        //                    pn1.Visible = false;
        //                }
        //            }));
        //        }
        //        else
        //        {
        //            if (pn1.Visible)
        //            {
        //                pn1.Visible = false;
        //            }
        //        }
        //        if (pn2.InvokeRequired)
        //        {
        //            pn2.Invoke(new MethodInvoker(delegate
        //            {
        //                if (pn2.Visible)
        //                {
        //                    pn2.Visible = false;
        //                }
        //            }));
        //        }
        //        else
        //        {
        //            if (pn2.Visible)
        //            {
        //                pn2.Visible = false;
        //            }
        //        }
        //        if (pn3.InvokeRequired)
        //        {
        //            pn3.Invoke(new MethodInvoker(delegate
        //            {
        //                if (!pn3.Visible)
        //                {
        //                    pn3.Visible = true;
        //                }
        //            }));
        //        }
        //        else
        //        {
        //            if (!pn3.Visible)
        //            {
        //                pn3.Visible = true;
        //            }
        //        }
        //        if (pn4.InvokeRequired)
        //        {
        //            pn4.Invoke(new MethodInvoker(delegate
        //            {
        //                if (!pn4.Visible)
        //                {
        //                    pn4.Visible = true;
        //                }
        //            }));
        //        }
        //        else
        //        {
        //            if (!pn4.Visible)
        //            {
        //                pn4.Visible = true;
        //            }
        //        }
        //    }
        //}
        #endregion




        //190621 cjm AlignXYT 변경
        //pcy200902 인자로 ecamno를 받아와야함(targetpos 맞는것 사용하기 위함)
        // 2020.09.15 lkw Source, Target 구분 추가함. 3점 Align 추가함.
        // markMove 가 true 일 경우 Mark 가 source 가 됨.... 반대의 경우 Mark 가 Target
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
                        //if ((Vision[VisionNo].CFG.eCamName == eCamNO4.EMIAttach.ToString() && CONST.bPLCReq[CONST.AAM_PLC2.BitControl.plcEMICamPosition])
                        //    || (Vision[VisionNo].CFG.eCamName == eCamNO4.TempAttach.ToString() && CONST.bPLCReq[CONST.AAM_PLC2.BitControl.plcTempCamPosition]))
                        //{
                        //    targetPixel1.X = Vision[VisionNo + 0].CFG.TargetX2[(int)ePatternKind.Left_1cam];
                        //    targetPixel1.Y = Vision[VisionNo + 0].CFG.TargetY2[(int)ePatternKind.Left_1cam];
                        //    targetPixel2.X = Vision[VisionNo + 0].CFG.TargetX2[(int)ePatternKind.Right_1cam];
                        //    targetPixel2.Y = Vision[VisionNo + 0].CFG.TargetY2[(int)ePatternKind.Right_1cam];
                        //}
                    }
                    //else if(Vision[VisionNo].CFG.eCamName == eCamNO4.Inspection1.ToString())
                    //{
                    //    targetPixel1.X = Vision[VisionNo + 0].CFG.TargetX[(int)ePatternKind.FPC];
                    //    targetPixel1.Y = Vision[VisionNo + 0].CFG.TargetY[(int)ePatternKind.FPC];
                    //    targetPixel2.X = Vision[VisionNo + 1].CFG.TargetX[(int)ePatternKind.FPC];
                    //    targetPixel2.Y = Vision[VisionNo + 1].CFG.TargetY[(int)ePatternKind.FPC];
                    //    //targetPixel3.X = Vision[VisionNo + 2].CFG.TargetX[(int)ePatternKind.FPC];
                    //    //targetPixel3.Y = Vision[VisionNo + 2].CFG.TargetY[(int)ePatternKind.FPC];
                    //}
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
                        //if ((Vision[VisionNo].CFG.eCamName == eCamNO4.EMIAttach.ToString() && CONST.bPLCReq[CONST.AAM_PLC2.BitControl.plcEMICamPosition])
                        //    || (Vision[VisionNo].CFG.eCamName == eCamNO4.TempAttach.ToString() && CONST.bPLCReq[CONST.AAM_PLC2.BitControl.plcTempCamPosition]))
                        //{
                        //    sourcePixel1.X = Vision[VisionNo + 0].CFG.TargetX2[(int)ePatternKind.Left_1cam];
                        //    sourcePixel1.Y = Vision[VisionNo + 0].CFG.TargetY2[(int)ePatternKind.Left_1cam];
                        //    sourcePixel2.X = Vision[VisionNo + 0].CFG.TargetX2[(int)ePatternKind.Right_1cam];
                        //    sourcePixel2.Y = Vision[VisionNo + 0].CFG.TargetY2[(int)ePatternKind.Right_1cam];
                        //}
                    }
                    //else if (Vision[VisionNo].CFG.eCamName == eCamNO4.Inspection1.ToString())
                    //{
                    //    targetPixel1.X = Vision[VisionNo + 0].CFG.TargetX[(int)ePatternKind.FPC];
                    //    targetPixel1.Y = Vision[VisionNo + 0].CFG.TargetY[(int)ePatternKind.FPC];
                    //    targetPixel2.X = Vision[VisionNo + 1].CFG.TargetX[(int)ePatternKind.FPC];
                    //    targetPixel2.Y = Vision[VisionNo + 1].CFG.TargetY[(int)ePatternKind.FPC];
                    //    targetPixel3.X = Vision[VisionNo + 2].CFG.TargetX[(int)ePatternKind.FPC];
                    //    targetPixel3.Y = Vision[VisionNo + 2].CFG.TargetY[(int)ePatternKind.FPC];
                    //}
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

            //if ((Vision[VisionNo].CFG.eCamName == eCamNO4.EMIAttach.ToString() && CONST.bPLCReq[CONST.AAM_PLC2.BitControl.plcEMICamPosition]) 
            //    || (Vision[VisionNo].CFG.eCamName == eCamNO4.TempAttach.ToString() && CONST.bPLCReq[CONST.AAM_PLC2.BitControl.plcTempCamPosition]))
            //{
            //    //
            //    offset.X1 = Menu.frmSetting.revData.mOffset[VisionNo].AlignOffsetXYT2.X;
            //    offset.X2 = Menu.frmSetting.revData.mOffset[VisionNo].AlignOffsetXYT2.X;
            //    offset.Y1 = Menu.frmSetting.revData.mOffset[VisionNo].AlignOffsetXYT2.Y;
            //    offset.Y2 = Menu.frmSetting.revData.mOffset[VisionNo].AlignOffsetXYT2.Y;
            //    offset.T = Menu.frmSetting.revData.mOffset[VisionNo].AlignOffsetXYT2.T;
            //}
            //else if (Vision[VisionNo].CFG.eCamName == eCamNO2.SCFReel1.ToString() && CONST.bPLCReq[CONST.AAM_PLC1.BitControl.plcSCFPickAlignReq])
            //{
            //    //pick이후 align offset 추가(SCFReel2에 적용)
            //    offset.X1 = Menu.frmSetting.revData.mOffset[VisionNo + 1].AlignOffsetXYT2.X;
            //    offset.X2 = Menu.frmSetting.revData.mOffset[VisionNo + 1].AlignOffsetXYT2.X;
            //    offset.Y1 = Menu.frmSetting.revData.mOffset[VisionNo + 1].AlignOffsetXYT2.Y;
            //    offset.Y2 = Menu.frmSetting.revData.mOffset[VisionNo + 1].AlignOffsetXYT2.Y;
            //    offset.T = Menu.frmSetting.revData.mOffset[VisionNo + 1].AlignOffsetXYT2.T;
            //}
            //혹시모르니까 0으로 만듬.
            //else if (CalPos == (int)eCalPos.Bend1Trans_L || CalPos == (int)eCalPos.Bend2Trans_L || CalPos == (int)eCalPos.Bend3Trans_L)
            //{
            //    offset.X1 = 0;
            //    offset.X2 = 0;
            //    offset.Y1 = 0;
            //    offset.Y2 = 0;
            //    offset.T = 0;
            //}

            #region transfer apc plc로 넘김.
            //pcy210118 11inch는 transfer 없어서 vision에서 apc받아서 uvw변환하여 움직여야함 calpos로 구분안됨..
            //pcy200707 first용 apc 추가
            //if (CalPos == (int)eCalPos.Bend1Trans_L)
            //if (bArmPreAPCOffset[0])
            //{
            //    bArmPreAPCOffset[0] = false;
            //    offset.X1 += CONST.RunRecipe.Param[eRecipe.BENDING_ARM_PRE_OFFSET_X];
            //    offset.X2 += CONST.RunRecipe.Param[eRecipe.BENDING_ARM_PRE_OFFSET_X];
            //    offset.Y1 += CONST.RunRecipe.Param[eRecipe.BENDING_ARM_PRE_OFFSET_Y];
            //    offset.Y2 += CONST.RunRecipe.Param[eRecipe.BENDING_ARM_PRE_OFFSET_Y];
            //    offset.T += CONST.RunRecipe.Param[eRecipe.BENDING_ARM_PRE_OFFSET_T];
            //}
            //else if (CalPos == (int)eCalPos.Bend2Trans_L)
            //{
            //    offset.Y1 += CONST.RunRecipe.Param[eRecipe.BENDING2_TRANSFER_OFFSET_Y];
            //    offset.Y2 += CONST.RunRecipe.Param[eRecipe.BENDING2_TRANSFER_OFFSET_Y];
            //    offset.T += CONST.RunRecipe.Param[eRecipe.BENDING2_TRANSFER_OFFSET_T];
            //}
            //else if (CalPos == (int)eCalPos.Bend3Trans_L)
            //{
            //    offset.Y1 += CONST.RunRecipe.Param[eRecipe.BENDING3_TRANSFER_OFFSET_Y];
            //    offset.Y2 += CONST.RunRecipe.Param[eRecipe.BENDING3_TRANSFER_OFFSET_Y];
            //    offset.T += CONST.RunRecipe.Param[eRecipe.BENDING2_TRANSFER_OFFSET_T];
            //}
            #endregion

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
                //if (Vision[VisionNo].CFG.eCamName == eCamNO2.SCFReel1.ToString()) param.thetaCalc = cs2DAlign.eThetaCalc.Y1_Y3;
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
        //private void CompareDgvSpec(DataGridView dgv, DataGridView dgvspec)
        //{
        //    for (int i = 0; i < dgvspec.Columns.Count; i++)
        //    {
        //        double.TryParse(dgvspec[i, 0].Value.ToString(), out double low);
        //        double.TryParse(dgvspec[i, 2].Value.ToString(), out double high);
        //        double.TryParse(dgv.Rows[0].Cells[i].Value.ToString(), out double value);
        //        if(value < low || value > high)
        //        {
        //            dgv.Rows[0].Cells[i].Style.BackColor = Color.Red;
        //        }
        //    }
        //}
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
            //double XspecI = CONST.RunRecipe.Param[eRecipe.INSPECTION_SPEC_X];
            //double YspecI = CONST.RunRecipe.Param[eRecipe.INSPECTION_SPEC_Y];
            if (bCPK || bEdgetoMark) //cpk계산하는곳은 인스펙션이라고 생각함.
            {
                Xspec = Menu.frmSetting.revData.mBendingArm.InspToleranceX;
                Yspec = Menu.frmSetting.revData.mBendingArm.InspToleranceY;
            }

            if (history.camName == nameof(eCamNO.Laser1) || history.camName == nameof(eCamNO.Laser2))
            {
                //Xspec = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_TOLERANCEX];
                //Yspec = CONST.RunRecipe.Param[eRecipe.LASER_POSITION_TOLERANCEY];
                Xspec = Menu.frmSetting.revData.mSizeSpecRatio.LaserPositionToleranceX;
                Yspec = Menu.frmSetting.revData.mSizeSpecRatio.LaserPositionToleranceY;
            }
            //if (displayOnly)
            //{
            //    Xspec = 0.05;  //
            //    Yspec = 0.05;
            //}
            //pcy200707 스펙과 동일할때 스펙아웃처럼 보이는것 방지
            Xspec += 0.0001;
            Yspec += 0.0001;

            double lx = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LX];
            double ly = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LY];
            double rx = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RX];
            double ry = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RY];
            //if(bCPK) //bcpk가 인스펙션이니까 상황에따라 바꾸기..
            //{
            //    lx = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LX]);
            //    ly = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LY]);
            //    rx = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RX]);
            //    ry = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RY]);
            //}
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
                            //double spec = (i % 2 == 0) ? Xspec : Yspec;
                            dgv.Rows[_row].DefaultCellStyle.BackColor = Color.LightGray;

                            //bspecin = false;
                            //if (Math.Abs(double.Parse(dgv[i, _row].Value.ToString())) > spec)
                            //{
                            //    dgv.Rows[_row].Cells[i].Style.BackColor = Color.Orange;
                            //}
                            //else
                            //{
                            //    dgv.Rows[_row].Cells[i].Style.BackColor = Color.LightGray;
                            //}
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
                //ResultDistX1[CalculCnt] = Convert.ToDouble(dgv[0, _row].Value);
                //ResultDistY1[CalculCnt] = Convert.ToDouble(dgv[1, _row].Value);
                //ResultDistX2[CalculCnt] = Convert.ToDouble(dgv[2, _row].Value);
                //ResultDistY2[CalculCnt] = Convert.ToDouble(dgv[3, _row].Value);

                ////이러면 0~31까지니까 32개 계산될듯?
                //if (CalculCnt < 31)
                //{
                //    CalculCnt++;
                //}
                //else
                //{
                //    CalculCnt = 0;
                //    bSendCPK = true;
                //}

                //// 0=X1, 1=Y1, 2=X2, 3=Y2
                //CPK[0] = AutoMainLogDatacalculator(ResultDistX1, lx, Xspec);
                //CPK[1] = AutoMainLogDatacalculator(ResultDistY1, ly, Yspec);
                //CPK[2] = AutoMainLogDatacalculator(ResultDistX2, rx, Xspec);
                //CPK[3] = AutoMainLogDatacalculator(ResultDistY2, ry, Yspec);

                //if (dgvSpec.InvokeRequired)
                //{
                //    dgvSpec.Invoke(new MethodInvoker(delegate
                //    {
                //        dgvSpec[0, 3].Value = CPK[0].ToString("0.000");
                //        dgvSpec[1, 3].Value = CPK[1].ToString("0.000");
                //        dgvSpec[2, 3].Value = CPK[2].ToString("0.000");
                //        dgvSpec[3, 3].Value = CPK[3].ToString("0.000");
                //    }));
                //}
                //else
                //{
                //    dgvSpec[0, 3].Value = CPK[0].ToString("0.000");
                //    dgvSpec[1, 3].Value = CPK[1].ToString("0.000");
                //    dgvSpec[2, 3].Value = CPK[2].ToString("0.000");
                //    dgvSpec[3, 3].Value = CPK[3].ToString("0.000");
                //}

                //pcy190507 첫30개부터 CPK보고
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
        private void ShowPLCStatus1(int nCam, bool bOn)
        {
            if (btnPLC1[nCam].InvokeRequired)
            {
                btnPLC1[nCam].Invoke(new MethodInvoker(delegate
                {
                    if (bOn)
                        btnPLC1[nCam].BackColor = Color.Green;
                    else
                        btnPLC1[nCam].BackColor = Color.Gray;
                }));
            }
            else
            {
                if (bOn)
                    btnPLC1[nCam].BackColor = Color.Green;
                else
                    btnPLC1[nCam].BackColor = Color.Gray;
            }
        }

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

        private void SetLabelText(Label labelControl, double value)
        {
            if (labelControl.InvokeRequired)
            {
                labelControl.Invoke(new MethodInvoker(delegate
                {
                    labelControl.Text = value.ToString("0.000");
                }));
            }
            else
                labelControl.Text = value.ToString("0.000");
        }

        private void SetLabelText(Label labelControl, string value)
        {
            if (labelControl.InvokeRequired)
            {
                labelControl.Invoke(new MethodInvoker(delegate
                {
                    labelControl.Text = value;
                }));
            }
            else
                labelControl.Text = value;
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

        //private const short cBending1 = 3;
        //private const short cBending2 = 4;
        //private const short cBending3 = 5;

        public class dTimeOutClass
        {
            public bool checkStart;
            public short address;
            public short initCheck;
            public DateTime time;

            public dTimeOutClass(short address, DateTime time, short initCheck)
            {
                this.address = address;
                this.time = time;
                this.initCheck = initCheck;
            }
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
        //Type baseBitControl = null;

        // InitCheck 인지를 검사.
        private bool isInitCheck(Type baseBitControl, short bitReqNo)
        {
            bool result = false;

            foreach (KeyValuePair<timeOut_initCheck, short> item in initCheckValue)
            {
                if (item.Key.bitControl == baseBitControl && item.Key.bitControlNo == bitReqNo)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

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

                //if (this.lastRunTime[reqID] != null)
                //{
                //    timeCheck time = this.lastRunTime[reqID];
                //    time.on = false;
                //}
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
                            //                           cLog.Save(csLog.LogKind.System, "Sequence OK, Type BitID No = " + reqID.ToString());

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

        // position picker call back용.
        private void positionPickerCallBack(object sender, userPickEventArgs e)
        {
            short pcReultID = e.pcResultID;

            if (pcReultID > -1)
                pcResult[pcReultID] = 0; // 0으로 초기화해서 계속 진행되도록 함.
        }

        private int nTimerFlag = 0;
        //int iLoadprecnt = 0;
        //   double[] dratio = new double[2];

        private void AutoMainProcess()
        {
            //short cnt = 0;
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

        private void GetLabel(Label label)
        {
            string rvalue = "";
            if (label.InvokeRequired)
            {
                label.Invoke(new MethodInvoker(delegate
                {
                    rvalue = label.Text;
                }));
            }
            else
                rvalue = label.Text;
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
                            ////pcy190527
                            //if (CONST.PCNo == 1 && Vision[i] == Vision[CONST.PCDetach.Vision_No.vsDetachPre1] && iLoadprecnt == 0)
                            //{
                            //    iLoadprecnt++;
                            //    //표시만딴데다가..
                            //    SetLabel(lblLoadpreScore, Vision[i].GetVisionSearchScore().ToString("0.00"));
                            //}
                            //else
                            //{
                            //    iLoadprecnt = 0;
                            //    SetLabel(lbScore[i], Vision[i].GetVisionSearchScore().ToString("0.00"));
                            //}
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
                        catch
                        {

                        }
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

            //csh 20170719
            if (CONST.m_bInterfaceLog)
            {
                //if (swInterface.ElapsedMilliseconds > 1000)
                //{
                //    swInterface.Stop();
                //    swInterface.Reset();
                //    string strTemp = "";

                //    //if (CONST.PCName == "AAM_PC1")
                //    //{
                //    //    strTemp = "LoadPreAlign1 : " + CONST.bPLCReq[CONST.AAM_PLC1.BitControl.plcLoadingConveyorAlignReq].ToString() +
                //    //              " , LoadPreAlign1 Flag : " + visionBackground[CONST.AAM_PLC1.BitControl.plcLoadingConveyorAlignReq].ToString() +
                //    //              " , BendingPre1 : " + CONST.bPLCReq[CONST.PCDetach.BitControl.plcDetachPreAlignReq].ToString() +
                //    //              " , BendingPre1 Flag : " + visionBackground[CONST.PCDetach.BitControl.plcDetachPreAlignReq].ToString();
                //    //}
                //    //else if (CONST.PCName == "AAM_PC2")
                //    //{
                //    //    strTemp = " , BendingArm1Init : " + CONST.bPLCReq[CONST.PCBend.BitControl.plcBend1AlignInitReq].ToString() +
                //    //              " , BendingArm1Align : " + CONST.bPLCReq[CONST.PCBend.BitControl.plcBend1AlignReq].ToString() +
                //    //              " , BendingArm1Align Flag : " + visionBackground[CONST.PCBend.BitControl.plcBend1AlignReq].ToString() +
                //    //              " , BendingArm2Init : " + CONST.bPLCReq[CONST.PCBend.BitControl.plcBend2AlignInitReq].ToString() +
                //    //              " , BendingArm2Align : " + CONST.bPLCReq[CONST.PCBend.BitControl.plcBend2AlignReq].ToString() +
                //    //              " , BendingArm2Align Flag : " + visionBackground[CONST.PCBend.BitControl.plcBend2AlignReq].ToString() +
                //    //              " , Inspection : " + CONST.bPLCReq[CONST.AAM_PC2.BitControl.plcInspectionReq].ToString() +
                //    //              " , Inspection Flag : " + visionBackground[CONST.AAM_PC2.BitControl.plcInspectionReq].ToString();
                //    //}
                //    cLog.Save(LogKind.Interface, strTemp);
                //}
                //else
                //{
                //    swInterface.Start();
                //}
            }

            /////////////////////////////////

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

        //private int[] thbendNo = new int[8];
        //private int[] thVNo = new int[8];
        //private cs2DAlign.ptAlignResult[] thalign = new cs2DAlign.ptAlignResult[8];
        //private bool[] thResult = new bool[8];
        //private cs2DAlign.ptXXYY[] thdist = new cs2DAlign.ptXXYY[8];

        //private void setThreadParam(int bendingNO, int visionNo, cs2DAlign.ptAlignResult align, bool bResult, cs2DAlign.ptXXYY dist)
        //{
        //    thbendNo[bendingNO] = bendingNO;
        //    thVNo[bendingNO] = visionNo;
        //    thalign[bendingNO] = align;
        //    thResult[bendingNO] = bResult;
        //    thdist[bendingNO] = dist;
        //}

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

        private void threadDisp(int bendingNO)
        {
            //로그와 화면표시를 thread로 돌려서 택타임을 줄이려는 용도 사용안함 주석처리

            ////int bendingNO = 0;
            //int visionNo = thVNo[bendingNO];
            //cs2DAlign.ptAlignResult align = thalign[bendingNO];
            //bool bResult = thResult[bendingNO];
            //cs2DAlign.ptXXYY dist = thdist[bendingNO];

            ////함수 들어오기 직전 firstLog[bendingNO]를 false로 바로 바꾸는데 이코드가 의미가 있는지 궁금함
            ////if (firstLog[bendingNO])
            ////{
            ////    LogDisp(visionNo, CONST.PanelIDBend[bendingNO]);
            ////}
            ////LogDisp(visionNo, "Bending Align " + " X : " + align.X.ToString("0.000") + ", Y : " + align.Y.ToString("0.000") + ", T : " + align.T.ToString("0.000"));
            ////LogDisp(visionNo, "Align( " + retryCnt[bendingNO].ToString() + ")" + " X : " + align.X.ToString("0.000") + ", Y : " + align.Y.ToString("0.000") + ", T : " + align.T.ToString("0.000"));

            //string slog = "";

            //if (firstLog[bendingNO])
            //{
            //    firstLog[bendingNO] = false;
            //    firstLogBD[bendingNO] = dist.X1.ToString("0.000") + "," + dist.Y1.ToString("0.000") + "," + dist.X2.ToString("0.000") + "," + dist.Y2.ToString("0.000");

            //    slog = string.Format("First,{0:0},{1:0.000},{2:0.000},{3:0.000},{4:0.000},{5:0.000},{6:0.000}," + CONST.PanelIDBend[bendingNO],
            //        bendingNO + 1,
            //        dist.X1,
            //        dist.Y1,
            //        dist.X2,
            //        dist.Y2,
            //        Math.Abs(align.Y),
            //        align.T,
            //        Vision[visionNo].GetVisionSearchScore().ToString("0.00"),
            //        Vision[visionNo + 1].GetVisionSearchScore().ToString("0.00")
            //        );
            //    LogDisp(visionNo, "Length[0], LX: " + dist.X1.ToString("0.000") + ", LY: " + dist.Y1.ToString("0.000") + ", RX: " + dist.X2.ToString("0.000") + ", RY: " + dist.Y2.ToString("0.000"));
            //}
            //else
            //{
            //    lastLogBD[bendingNO] = dist.X1.ToString("0.000") + "," + dist.Y1.ToString("0.000") + "," + dist.X2.ToString("0.000") + "," + dist.Y2.ToString("0.000");

            //    slog = string.Format("(r " + ArmRetryCnt[bendingNO].ToString() + "),{0:0},{1:0.000},{2:0.000},{3:0.000},{4:0.000},{5:0.000},{6:0.000}," + CONST.PanelIDBend[bendingNO],
            //        bendingNO + 1,
            //        dist.X1,
            //        dist.Y1,
            //        dist.X2,
            //        dist.Y2,
            //        Math.Abs(align.Y),
            //        align.T,
            //        Vision[visionNo].GetVisionSearchScore().ToString("0.00"),
            //        Vision[visionNo + 1].GetVisionSearchScore().ToString("0.00")
            //        );

            //    //LogDisp(visionNo, "Align(" + retryCnt[bendingNO].ToString() + "), LX : " + dist.X1.ToString("0.000") + ", LY : " + dist.Y1.ToString("0.000") + ", RX : " + dist.X2.ToString("0.000") + ", RY : " + dist.Y2.ToString("0.000"));
            //}

            //if (CONST.m_bMeasBendingLog)
            //{
            //    if (bendingNO == 0) cLog.Save(LogKind.MeasBending1, slog + "," + CONST.PanelIDBend[0]);
            //    else if (bendingNO == 1) cLog.Save(LogKind.MeasBending2, slog + "," + CONST.PanelIDBend[1]);
            //    else if (bendingNO == 2) cLog.Save(LogKind.MeasBending3, slog + "," + CONST.PanelIDBend[2]);
            //}

            //if (bResult)
            //{
            //    slog = string.Format("AF,{0:0},{1:0.000},{2:0.000},{3:0.000},{4:0.000}," + CONST.PanelIDBend[bendingNO],
            //        bendingNO + 1,
            //        dist.X1.ToString("0.000"),
            //        dist.Y1.ToString("0.000"),
            //        dist.X2.ToString("0.000"),
            //        dist.Y2.ToString("0.000"));

            //    //JJ , 2017-05-20 : Log PPID ----- start
            //    if (CONST.m_bMeasBendingLog)
            //    {
            //        if (bendingNO == 0) cLog.Save(LogKind.MeasBending1, slog + "," + CONST.PanelIDBend[0]);
            //        else if (bendingNO == 1) cLog.Save(LogKind.MeasBending2, slog + "," + CONST.PanelIDBend[1]);
            //        else if (bendingNO == 2) cLog.Save(LogKind.MeasBending3, slog + "," + CONST.PanelIDBend[2]);
            //    }

            //    //LogDisp(visionNo, "Align(Finish), LX : " + dist.X1.ToString("0.000") + ", LY : " + dist.Y1.ToString("0.000") + ", RX : " + dist.X2.ToString("0.000") + ", RY : " + dist.Y2.ToString("0.000"));
            //    //string sDistLog = "0," + dist.X1.ToString("0.000") + "," + dist.Y1.ToString("0.000") + "," + dist.X2.ToString("0.000") + "," + dist.Y2.ToString("0.000");
            //    //cLog.DistLogSave(Vision[visionNo].CFG.Name, sDistLog);
            //}
        }

        public void setVision(ref csVision org, int targetNo)
        {
            //automain쪽 Vision을 다른 csVision쪽으로 참조시킴.
            org = Vision[targetNo];
            //Vision[targetNo].DispChange(org.cogDS);
            //try
            //{
            //    if (org.cogDS != null)
            //    {
            //        org.cogDS.InteractiveGraphics.Clear();
            //        org.cogDS.StaticGraphics.Clear();
            //    }
            //}
            //catch { }
            //string[] strTemp = new string[8];

            //for (int i = 0; i < CONST.CAMCnt; i++)
            //{
            //    strTemp[i] = Vision[i].CFG.Name;
            //}
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

            //if (CONST.PCNo == 3 || CONST.PCNo == 4 || CONST.PCNo == 6 || CONST.PCNo == 7)
            ////{
            //    cbResult.Checked = true;
            ////}
            ////20200924 cjm 진행중
            ////210215 cjm Change
            //else if (CONST.PCNo == 1 || CONST.PCNo == 2)
            //{
            //    pnResult2.Visible = false;
            //    pngraph.Visible = false;
            //    cbResult.Visible = false;
            //    cbResult.Checked = false;

            //    pnResult1.Visible = true;
            //    pnResult1.Location = new Point(1, 451);
            //    //lbResult1Title.Text = "Bending Pre SCF Insp Result";  // AAM에서 사용된 SCF Insp


            //    cbSCFInspResult.Text = "FoamAttachResult";
            //    cbSCFInspResult.Visible = true;
            //    cbSCFInspResult.Location = cbResult.Location;
            //    cbSCFInspResult.Checked = true;

            //    //BendingPreSCFInspection();
            //}
            //else
            //{
            //    cbResult.Visible = false;
            //    cbResult.Checked = false;
            //    pnResult2.Visible = false;
            //    pngraph.Visible = false;
            //    pnResult1.Visible = true;
            //    pnResult1.Location = new Point(1, 451);

            //    cbSCFInspResult.Text = "Result";
            //    cbSCFInspResult.Visible = true;
            //    cbSCFInspResult.Location = cbResult.Location;
            //    cbSCFInspResult.Checked = true;
            //}

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
            //Vision[CamNo].ISLight = Value;
            ISLight[PortNO, CH] = Value;
            //Thread.Sleep(Vision[CamNo].CFG.LightDelay); //light exp cont한꺼번에 delay먹도록 변경해서 뺌
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
                //Vision[CamNo].ISLight = iValue;
                //if (iValue2 != 0) Vision[CamNo + 1].ISLight = iValue2;
                ISLight[PortNO, CH] = iValue;
            }
            //Thread.Sleep(Vision[CamNo].CFG.LightDelay);
        }

        public void LightSet2CH(string PortNo, int CH, int iValue, int CamNo)
        {
            char[] valData = iValue.ToString().PadLeft(3, '0').ToCharArray();
            char cCH = char.Parse(CH.ToString());
            byte[] sendData = new byte[6];
            sendData[0] = Convert.ToByte(cCH);
            sendData[1] = Convert.ToByte(valData[0]);
            sendData[2] = Convert.ToByte(valData[1]);
            sendData[3] = Convert.ToByte(valData[2]);
            sendData[4] = bCR;
            sendData[5] = bLF;
            if (spLight1.IsOpen && spLight1.PortName.Substring(3, 1) == PortNo) spLight1.Write(sendData, 0, sendData.Length);
            else if (spLight2.IsOpen && spLight2.PortName.Substring(3, 1) == PortNo) spLight2.Write(sendData, 0, sendData.Length);
            else if (spLight3.IsOpen && spLight3.PortName.Substring(3, 1) == PortNo) spLight3.Write(sendData, 0, sendData.Length);
            //Vision[CamNo].ISLight = iValue;
        }

        //private int[] retryCnt = new int[3];

        // 해당 cam no 에 대한 pattern를 load한다. lyw.
        public void PatternLoad(int camNo)
        {
            Vision[camNo].GetPattern(); // 추후 해제
        }

        //private void cbLive_CheckedChanged(object sender, EventArgs e)
        //{
        //    string sName = (sender as CheckBox).Name;
        //    for (short i = 0; i < cbLive.Length; i++)
        //    {
        //        if (cbLive[i].Name == sName)
        //        {
        //            Vision[i].Live(cbLive[i].Checked);
        //        }
        //    }
        //}

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

        //20200926 cjm 변경 : CamNo에 따라서 AutoMain에서 Data 값이 나오는 DGV 위치 변경
        public void LogDisp(int CamNo, string sLog, bool bSave = true)
        {
            string str = DateTime.Now.ToString("HH:mm:ss") + "." + DateTime.Now.Millisecond.ToString("000") + " - " + sLog;
            //int Num = Convert.ToInt32(Math.Truncate(Convert.ToDouble(CamNo) / 2));

            int Num = SelectResultBox(CamNo);

            if (lbList[Num].InvokeRequired)
            {
                lbList[Num].Invoke(new MethodInvoker(delegate
                {
                    if (lbList[Num].Items.Count > 50)
                    {
                        //pcy200609 테스트 필요
                        //lbList[Num].Items.Clear();
                        lbList[Num].Items.RemoveAt(lbList[Num].Items.Count - 1);
                    }
                    lbList[Num].Items.Insert(0, str);
                }));
            }
            else
            {
                if (lbList[Num].Items.Count > 50)
                {
                    //lbList[Num].Items.Clear();
                    lbList[Num].Items.RemoveAt(lbList[Num].Items.Count - 1);
                }
                lbList[Num].Items.Insert(0, str);
            }

            //if (lbListRetry[Num].InvokeRequired)
            //{
            //    lbListRetry[Num].Invoke(new MethodInvoker(delegate
            //    {
            //        if (lbListRetry[Num].Items.Count > 30) lbListRetry[Num].Items.Clear();
            //        else lbListRetry[Num].Items.Insert(0, str);
            //    }));
            //}
            //else
            //{
            //    if (lbListRetry[Num].Items.Count > 30) lbListRetry[Num].Items.Clear();
            //    else lbListRetry[Num].Items.Insert(0, str);
            //}

            //if (bSave)
            //{
            //    sLog = DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss") + "." + DateTime.Now.Millisecond.ToString("000") + "," + sLog;
            //    cLog.AlignLogSave(Vision[CamNo].CFG.Name, sLog);
            //}
        }

        //pcy190401 최근 자재 1개 얼라인 데이터만 표시.
        //pcy190408 logdisp꺼 동일하게 표시하고 이함수로 리셋만 시키는것도 괜찮을듯 테스트후 결정하기.
        //public void LogDispRetry(int CamNo, string sLog, bool bClear = false)
        //{
        //    //string str = DateTime.Now.ToString("HH:mm:ss") + "." + DateTime.Now.Millisecond.ToString("000") + " - " + sLog;
        //    int Num = Convert.ToInt32(Math.Truncate(Convert.ToDouble(CamNo) / 2));

        //    //if (lbListRetry[Num].InvokeRequired)
        //    //{
        //    //    lbListRetry[Num].Invoke(new MethodInvoker(delegate
        //    //    {
        //    //        if (lbListRetry[Num].Items.Count > 30 || bClear) lbListRetry[Num].Items.Clear();
        //    //        //else lbListRetry[Num].Items.Insert(0, str);
        //    //    }));
        //    //}
        //    //else
        //    //{
        //    //    if (lbListRetry[Num].Items.Count > 30 || bClear) lbListRetry[Num].Items.Clear();
        //    //    //else lbListRetry[Num].Items.Insert(0, str);
        //    //}

        //    //if (bSave)
        //    //{
        //    //    sLog = DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss") + "." + DateTime.Now.Millisecond.ToString("000") + "," + sLog;
        //    //    cLog.AlignLogSave(Vision[CamNo].CFG.Name, sLog);
        //    //}
        //}


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
                    //Chart1.Series[3].Color = Color.Indigo;
                }

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

                //string[] sLog1 = null;
                //string[] sLog2 = null;
                string[] sLog = null;
                cLog.ReadDistLog(ref sLog, start, end, Vision[CamNo].CFG.Name);
                //cLog.ReadDistLog(ref sLog2, start, end, Vision[CamNo+1].CFG.Name);
                //string[] sLog = new string[sLog1.Length + sLog2.Length];
                //for (int i = 0; i < sLog.Length; i++) sLog[i] = sLog1[i];
                //for (int i = sLog1.Length; i < sLog1.Length + sLog2.Length; i++) sLog[i] = sLog2[i - sLog1.Length];
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
                        //ChartX1_1 = FirstX1Spec + chartlengthX;
                        //ChartY1_1 = FirstY1Target + chartlengthY;
                        //ChartX1_2 = FirstX1Spec - chartlengthX;
                        //ChartY1_2 = FirstY1Target - chartlengthY;

                        //ChartX2_1 = FirstX2Spec + chartlengthX;
                        //ChartY2_1 = FirstY2Target + chartlengthY;
                        //ChartX2_2 = FirstX2Spec - chartlengthX;
                        //ChartY2_2 = FirstY2Target - chartlengthY;
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
                        //ChartX1_1 = FirstX1Spec + chartlengthX;
                        //ChartY1_1 = FirstY1Target + chartlengthY;
                        //ChartX1_2 = FirstX1Spec - chartlengthX;
                        //ChartY1_2 = FirstY1Target - chartlengthY;

                        //ChartX2_1 = FirstX2Spec + chartlengthX;
                        //ChartY2_1 = FirstY2Target + chartlengthY;
                        //ChartX2_2 = FirstX2Spec - chartlengthX;
                        //ChartY2_2 = FirstY2Target - chartlengthY;
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
                        //ChartX1_1 = FirstX1Spec + 0.6;
                        //ChartY1_1 = FirstY1Target + 65;
                        //ChartX1_2 = FirstX1Spec - 0.6;
                        //ChartY1_2 = FirstY1Target - 0.65;

                        //ChartX2_1 = FirstX2Spec + 0.6;
                        //ChartY2_1 = FirstY2Target + 0.65;
                        //ChartX2_2 = FirstX2Spec - 0.6;
                        //ChartY2_2 = FirstY2Target - 0.65;
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
                        //ChartX1_1 = FirstX1Spec + 0.6;
                        //ChartY1_1 = FirstY1Target + 65;
                        //ChartX1_2 = FirstX1Spec - 0.6;
                        //ChartY1_2 = FirstY1Target - 0.65;

                        //ChartX2_1 = FirstX2Spec + 0.6;
                        //ChartY2_1 = FirstY2Target + 0.65;
                        //ChartX2_2 = FirstX2Spec - 0.6;
                        //ChartY2_2 = FirstY2Target - 0.65;
                        break;
                        //case "SCF1":
                        //    FirstX1Spec = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X1_SPEC];
                        //    FirstX2Spec = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X1_SPEC];
                        //    FirstY1Target = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y1_SPEC];
                        //    FirstY2Target = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y1_SPEC];

                        //    dXspec = Menu.frmSetting.revData.mBendingPre.SCFTolerance;
                        //    dYspec = Menu.frmSetting.revData.mBendingPre.SCFTolerance;

                        //    chartlengthX = dXspec * 1.8;
                        //    chartlengthY = dYspec * 1.8;
                        //    ChartX1_1 = FirstX1Spec + chartlengthX;
                        //    ChartY1_1 = FirstY1Target + chartlengthY;
                        //    ChartX1_2 = FirstX1Spec - chartlengthX;
                        //    ChartY1_2 = FirstY1Target - chartlengthY;

                        //    ChartX2_1 = FirstX2Spec + chartlengthX;
                        //    ChartY2_1 = FirstY2Target + chartlengthY;
                        //    ChartX2_2 = FirstX2Spec - chartlengthX;
                        //    ChartY2_2 = FirstY2Target - chartlengthY;
                        //    break;
                        //case "SCF2":
                        //    FirstX1Spec = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X2_SPEC];
                        //    FirstX2Spec = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X2_SPEC];
                        //    FirstY1Target = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y2_SPEC];
                        //    FirstY2Target = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y2_SPEC];

                        //    dXspec = Menu.frmSetting.revData.mBendingPre.SCFTolerance;
                        //    dYspec = Menu.frmSetting.revData.mBendingPre.SCFTolerance;

                        //    chartlengthX = dXspec * 1.8;
                        //    chartlengthY = dYspec * 1.8;
                        //    ChartX1_1 = FirstX1Spec + chartlengthX;
                        //    ChartY1_1 = FirstY1Target + chartlengthY;
                        //    ChartX1_2 = FirstX1Spec - chartlengthX;
                        //    ChartY1_2 = FirstY1Target - chartlengthY;

                        //    ChartX2_1 = FirstX2Spec + chartlengthX;
                        //    ChartY2_1 = FirstY2Target + chartlengthY;
                        //    ChartX2_2 = FirstX2Spec - chartlengthX;
                        //    ChartY2_2 = FirstY2Target - chartlengthY;
                        //    break;
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
                        //else if(CamName == "SCF1")
                        //{
                        //    dX1 = Math.Abs(double.Parse(sData[0]));
                        //    dY1 = Math.Abs(double.Parse(sData[1]));
                        //    dX2 = Math.Abs(double.Parse(sData[2]));
                        //    dY2 = Math.Abs(double.Parse(sData[3]));
                        //    specoutcheck = true;
                        //}
                        //else if (CamName == "SCF2")
                        //{
                        //    //dX1 = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X2_SPEC];
                        //    //dY1 = Math.Abs(double.Parse(sData[4]));
                        //    //dX2 = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X2_SPEC];
                        //    //dY2 = Math.Abs(double.Parse(sData[5]));
                        //    dX1 = Math.Abs(double.Parse(sData[4]));
                        //    dY1 = Math.Abs(double.Parse(sData[5]));
                        //    dX2 = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X2_SPEC];
                        //    dY2 = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y2_SPEC];
                        //    specoutcheck = true;
                        //}
                        else
                        {
                            dX1 = Math.Abs(double.Parse(sData[1]));
                            dY1 = Math.Abs(double.Parse(sData[2]));
                            dX2 = Math.Abs(double.Parse(sData[3]));
                            dY2 = Math.Abs(double.Parse(sData[4]));
                            if (sData.Length == 6) dRetryCnt = double.Parse(sData[5]);
                            specoutcheck = true;
                        }

                        //double dX1data;
                        //double dY1data;
                        //double dX2data;
                        //double dY2data;

                        //if (CamNo == Vision_No.vsInspection1 && !(Menu.frmSetting.revData.mBendingArm.iInspMode == CONST.eInspMode.MarkToMark))
                        //{
                        //    dX1data = dX1 - CONST.RunRecipe.Param[eRecipe.BENDING_EDGE_TO_EDGE_X1]);
                        //    dY1data = dY1 - FirstY1Target;
                        //    dX2data = dX2 - CONST.RunRecipe.Param[eRecipe.BENDING_EDGE_TO_EDGE_X2]);
                        //    dY2data = dY2 - FirstY2Target;
                        //}
                        //else
                        //{
                        //    dX1data = dX1 - CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LX]);
                        //    dY1data = dY1 - FirstY1Target;
                        //    dX2data = dX2 - CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RX]);
                        //    dY2data = dY2 - FirstY2Target;
                        //}

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

                        #region 참고

                        ////pcy190605 double로 계산하니 소수점 찌꺼기가 남아서 스펙에 딱 걸치는 점이 불량판정됨 int로 변경.
                        //if (Convert.ToInt32(dX1 * 1000) > Convert.ToInt32((FirstX1Spec + dsquareX) * 1000) || Convert.ToInt32(dX1 * 1000) < Convert.ToInt32((FirstX1Spec - dsquareX) * 1000)
                        //    || Convert.ToInt32(dY1 * 1000) > Convert.ToInt32((FirstY1Target + dsquareY + OffsetY1) * 1000) || Convert.ToInt32(dY1 * 1000) < Convert.ToInt32((FirstY1Target - dsquareY + OffsetY1) * 1000))
                        //{
                        //    Chart1.Series[0].Points[i].Color = Color.Orange;
                        //    if (specoutcheck) isquareout1++;

                        //    if (Convert.ToInt32(dX1 * 1000) > Convert.ToInt32((FirstX1Spec + dXspec) * 1000) || Convert.ToInt32(dX1 * 1000) < Convert.ToInt32((FirstX1Spec - dXspec) * 1000)
                        //        || Convert.ToInt32(dY1 * 1000) > Convert.ToInt32((FirstY1Target + dYspec + OffsetY1) * 1000) || Convert.ToInt32(dY1 * 1000) < Convert.ToInt32((FirstY1Target - dYspec + OffsetY1) * 1000))
                        //    {
                        //        Chart1.Series[0].Points[i].Color = Color.Red;
                        //        if (specoutcheck) ispecout1++;
                        //    }
                        //    else
                        //    {
                        //        //square 아웃이지만 스펙인임.
                        //        if (specoutcheck) ispecin1++;
                        //    }

                        //    if(dRetryCnt == 9) Chart1.Series[0].Points[i].Color = Color.Black;
                        //}
                        //else
                        //{
                        //    Chart1.Series[0].Points[i].Color = Color.DarkGreen;
                        //    if (specoutcheck) ispecin1++;
                        //    if (dRetryCnt == 9) Chart1.Series[0].Points[i].Color = Color.Black;
                        //}

                        //if (Convert.ToInt32(dX2 * 1000) > Convert.ToInt32((FirstX2Spec + dsquareX) * 1000) || Convert.ToInt32(dX2 * 1000) < Convert.ToInt32((FirstX2Spec - dsquareX) * 1000)
                        //        || Convert.ToInt32(dY2 * 1000) > Convert.ToInt32((FirstY2Target + dsquareY + OffsetY2) * 1000) || Convert.ToInt32(dY2 * 1000) < Convert.ToInt32((FirstY2Target - dsquareY + OffsetY2) * 1000))
                        //{
                        //    Chart2.Series[0].Points[i].Color = Color.Orange;
                        //    if (specoutcheck) isquareout2++;

                        //    if (Convert.ToInt32(dX2 * 1000) > Convert.ToInt32((FirstX2Spec + dXspec) * 1000) || Convert.ToInt32(dX2 * 1000) < Convert.ToInt32((FirstX2Spec - dXspec) * 1000)
                        //        || Convert.ToInt32(dY2 * 1000) > Convert.ToInt32((FirstY2Target + dYspec + OffsetY2) * 1000) || Convert.ToInt32(dY2 * 1000) < Convert.ToInt32((FirstY2Target - dYspec + OffsetY2) * 1000))
                        //    {
                        //        Chart2.Series[0].Points[i].Color = Color.Red;
                        //        if (specoutcheck) ispecout2++;
                        //    }
                        //    else
                        //    {
                        //        if (specoutcheck) ispecin2++;
                        //    }
                        //    if(dRetryCnt == 9) Chart2.Series[0].Points[i].Color = Color.Black;
                        //}
                        //else
                        //{
                        //    Chart2.Series[0].Points[i].Color = Color.DarkGreen;
                        //    if (specoutcheck) ispecin2++;
                        //    if (dRetryCnt == 9) Chart2.Series[0].Points[i].Color = Color.Black;
                        //}

                        //if (Math.Abs(dX1data) > dXspec || dY1data > (dYspec + OffsetY1) || dY1data < (-dYspec + OffsetY1))
                        //{
                        //    Chart1.Series[0].Points[i].Color = Color.Red;
                        //    if(specoutcheck) ispecout1++;
                        //}
                        //else Chart1.Series[0].Points[i].Color = Color.DarkGreen;
                        //if (Math.Abs(dX2data) > dXspec || dY2data > (dYspec + OffsetY2) || dY2data < (-dYspec + OffsetY2))
                        //{
                        //    Chart2.Series[0].Points[i].Color = Color.Red;
                        //    if(specoutcheck) ispecout2++;
                        //}
                        //else Chart2.Series[0].Points[i].Color = Color.DarkGreen;

                        #endregion 참고
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

            //GetCamNameFromVisionNo(VisionNo, out string camName);
            //switch (camName)
            //{
            //case nameof(Vision_No.Bend1_1):
            //    dXspec = Menu.frmSetting.revData.mBendingArm.BDToleranceX;
            //    dYspec = Menu.frmSetting.revData.mBendingArm.BDToleranceY;
            //    chartlengthX = dXspec * 1.25;
            //    chartlengthY = dYspec * 1.25;
            //    break;

            //case nameof(Vision_No.UpperInsp1_1):
            //    FirstX1Spec = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LX];
            //    FirstX2Spec = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RX];
            //    FirstY1Target = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LY];
            //    FirstY2Target = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RY];
            //    if (Menu.frmSetting.revData.mBendingArm.bDisplayLRSwapINSP)
            //    {
            //        FirstX1Spec = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RX];
            //        FirstX2Spec = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LX];
            //        FirstY1Target = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RY];
            //        FirstY2Target = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LY];
            //    }

            //    dXspec = Menu.frmSetting.revData.mBendingArm.InspToleranceX;
            //    dYspec = Menu.frmSetting.revData.mBendingArm.InspToleranceY;
            //    chartlengthX = dXspec + 0.2;
            //    chartlengthY = dYspec + 0.2;
            //    break;

            //case nameof(Vision_No.Attach1_1):

            //    FirstX1Spec = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_LX];
            //    FirstX2Spec = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_RX];
            //    FirstY1Target = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_LY];
            //    FirstY2Target = CONST.RunRecipe.Param[eRecipe.FOAM1_ATTACH_SPEC_RY];

            //    dXspec = Menu.frmSetting.revData.mSizeSpecRatio.AttachToleranceX;
            //    dYspec = Menu.frmSetting.revData.mSizeSpecRatio.AttachToleranceY;
            //    chartlengthX = dXspec * 1.5;
            //    chartlengthY = dYspec * 1.5;
            //    break;
            //case nameof(Vision_No.Attach2_3):
            //    FirstX1Spec = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_LX];
            //    FirstX2Spec = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_RX];
            //    FirstY1Target = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_LY];
            //    FirstY2Target = CONST.RunRecipe.Param[eRecipe.FOAM2_ATTACH_SPEC_RY];

            //    dXspec = Menu.frmSetting.revData.mSizeSpecRatio.AttachToleranceX;
            //    dYspec = Menu.frmSetting.revData.mSizeSpecRatio.AttachToleranceY;
            //    chartlengthX = dXspec * 1.5;
            //    chartlengthY = dYspec * 1.5;
            //    break;
            //case nameof(Vision_No.Laser1):
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
                //if (camName == nameof(Vision_No.UpperInsp1_1))
                //{
                //    Chart1.Series.Add(" ");
                //    //Chart1.Series[3].Color = Color.Indigo;
                //}

                Chart2.Series.Clear();
                Chart2.Series.Add("RX/RY");
                Chart2.Series.Add("SpecIn");
                Chart2.Series.Add("SpecLine");
                Chart2.Series[1].Color = Color.Blue;
                //if (camName == nameof(Vision_No.UpperInsp1_1))
                //{
                //    Chart2.Series.Add(" ");
                //    //Chart2.Series[3].Color = Color.Indigo;
                //    //Chart2.Series[3].IsValueShownAsLabel = false;
                //}
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


                lblctSpec[0].Text = FirstX1Spec.ToString("0.000");
                lblctSpec[1].Text = FirstY1Target.ToString("0.000");
                lblctSpec[2].Text = FirstX2Spec.ToString("0.000");
                lblctSpec[3].Text = FirstY2Target.ToString("0.000");

                //if (camName == nameof(Vision_No.UpperInsp1_1))
                //{
                //    //LX, LY 스펙 인 에어리어 박스 생성
                //    Chart1.Series[3].Points.AddXY(FirstX1Spec + dsquareX, FirstY1Target + dsquareY);
                //    Chart1.Series[3].Points.AddXY(FirstX1Spec + dsquareX, FirstY1Target - dsquareY);
                //    Chart1.Series[3].Points.AddXY(FirstX1Spec - dsquareX, FirstY1Target - dsquareY);
                //    Chart1.Series[3].Points.AddXY(FirstX1Spec - dsquareX, FirstY1Target + dsquareY);
                //    Chart1.Series[3].Points.AddXY(FirstX1Spec - dsquareX, FirstY1Target + dsquareY);
                //    Chart1.Series[3].Points.AddXY(FirstX1Spec + dsquareX, FirstY1Target + dsquareY);
                //    Chart1.Series[3].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                //    Chart1.Series[3].BorderWidth = 1;

                //    //RX, RY 스펙 인 에어리어 박스 생성
                //    Chart2.Series[3].Points.AddXY(FirstX2Spec + dsquareX, FirstY2Target + dsquareY);
                //    Chart2.Series[3].Points.AddXY(FirstX2Spec + dsquareX, FirstY2Target - dsquareY);
                //    Chart2.Series[3].Points.AddXY(FirstX2Spec - dsquareX, FirstY2Target - dsquareY);
                //    Chart2.Series[3].Points.AddXY(FirstX2Spec - dsquareX, FirstY2Target + dsquareY);
                //    Chart2.Series[3].Points.AddXY(FirstX2Spec - dsquareX, FirstY2Target + dsquareY);
                //    Chart2.Series[3].Points.AddXY(FirstX2Spec + dsquareX, FirstY2Target + dsquareY);
                //    Chart2.Series[3].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                //    Chart2.Series[3].BorderWidth = 1;
                //}
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
            //switch (CONST.PCNo)
            //{
            //    case 1:
            //        if (visionNo == Vision_No.Attach1_1) camName = nameof(Vision_No.Attach1_1);
            //        else if (visionNo == Vision_No.Attach1_2) camName = nameof(Vision_No.Attach1_1);
            //        else if (visionNo == Vision_No.Attach1_3) camName = nameof(Vision_No.Attach1_3);
            //        else if (visionNo == Vision_No.Attach1_4) camName = nameof(Vision_No.Attach1_3);
            //        break;
            //    case 2:
            //        if (visionNo == Vision_No.Attach2_1) camName = nameof(Vision_No.Attach2_1);
            //        else if (visionNo == Vision_No.Attach2_2) camName = nameof(Vision_No.Attach2_1);
            //        else if (visionNo == Vision_No.Attach2_3) camName = nameof(Vision_No.Attach2_3);
            //        else if (visionNo == Vision_No.Attach2_4) camName = nameof(Vision_No.Attach2_3);
            //        break;
            //    case 3:
            //    case 6:
            //        if (visionNo == Vision_No.Bend1_1) camName = nameof(Vision_No.Bend1_1);
            //        else if (visionNo == Vision_No.Bend1_2) camName = nameof(Vision_No.Bend1_1);
            //        break;
            //    case 4:
            //    case 7:
            //        if (visionNo == Vision_No.UpperInsp1_1) camName = nameof(Vision_No.UpperInsp1_1);
            //        else if (visionNo == Vision_No.UpperInsp1_2) camName = nameof(Vision_No.UpperInsp1_1);
            //        break;
            //    case 8:
            //        if (visionNo == Vision_No.Laser1) camName = nameof(Vision_No.Laser1);
            //        break;

            //}

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
            //if (CONST.PCName == "AAM_PC2")
            //    Vision[0].Overay(checkBox_Overlay1.Checked, true);
            //else
            //    Vision[0].Overay(checkBox_Overlay1.Checked);
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
                    //if (nCamNo == 0 || nCamNo == 1) //pre
                    //    Num = 0;
                    //else if (nCamNo == 2 || nCamNo == 3 || nCamNo == 4 || nCamNo == 5) //attach  Here
                    //    Num = 1;
                    //else if (nCamNo == 6) //reel
                    //    Num = 2;
                    //else if (nCamNo == 7) //tray
                    //    Num = 3;
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

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            lbResult3Title.Text = CONST.RESULT_TITLE3; //"BENDING3 RESULT";  //추후 INI 파일에서불러오도록 수정
            lbResult4Title.Text = CONST.RESULT_TITLE4; //"INSPECTION RESULT";  //추후 INI 파일에서불러오도록 수정

            pnResult1.Visible = false;
            pnResult2.Visible = true;

            //하드코딩
            pnResult2.Location = new Point(1, 28);
            pnResult2.Size = new Size(521, 399);

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

        private void WriteCogLabel(string strData, int nCamNum, Font font, CogColorConstants Color, int nPosX, int nPosY, bool bAlign = false, string Name = "Defalut")
        {
            if (CONST.m_bOverlayShow)
            {
                cogLabel.Color = Color;
                cogLabel.SetXYText(nPosX, nPosY, strData);
                cogLabel.Font = font;

                if (bAlign)
                {
                    cogLabel.Alignment = Cognex.VisionPro.CogGraphicLabelAlignmentConstants.TopLeft;
                }
                Vision[nCamNum].cogDS.InteractiveGraphics.Add(cogLabel, Name, false);
            }
        }

        private cs2DAlign.ptXYT Setmoveadd(short visionNo, cs2DAlign.ptAlignResult align)
        {
            cs2DAlign.ptXYT result = new cs2DAlign.ptXYT();
            result.X = align.X;
            result.Y = align.Y;
            result.T = align.T;

            XYT MoveAdd = new XYT();
            MoveAdd.X = (Menu.frmSetting.revData.mOffset[visionNo].MoveAdd.X + 100) / 100;
            MoveAdd.Y = (Menu.frmSetting.revData.mOffset[visionNo].MoveAdd.Y + 100) / 100;
            MoveAdd.T = (Menu.frmSetting.revData.mOffset[visionNo].MoveAdd.T + 100) / 100;

            result.X = result.X * MoveAdd.X;
            result.Y = result.Y * MoveAdd.Y;
            result.T = result.T * MoveAdd.T;

            return result;
        }

        private cs2DAlign.ptXYT Setoncemovelimit(short visionNo, cs2DAlign.ptAlignResult align)
        {
            cs2DAlign.ptXYT result = new cs2DAlign.ptXYT();
            result.X = align.X;
            result.Y = align.Y;
            result.T = align.T;

            if (Menu.frmSetting.revData.mOffset[visionNo].OnceMoveLimit.X != 0
                && Math.Abs(result.X) > Menu.frmSetting.revData.mOffset[visionNo].OnceMoveLimit.X)
            {
                if (result.X > 0)
                {
                    LogDisp(visionNo, "OnceMoveLimit X :" + result.X.ToString("0.000") + "->" + Menu.frmSetting.revData.mOffset[visionNo].OnceMoveLimit.X.ToString("0.000"));
                    result.X = Menu.frmSetting.revData.mOffset[visionNo].OnceMoveLimit.X;
                }
                else
                {
                    LogDisp(visionNo, "OnceMoveLimit X :" + result.X.ToString("0.000") + "->-" + Menu.frmSetting.revData.mOffset[visionNo].OnceMoveLimit.X.ToString("0.000"));
                    result.X = (-1) * Menu.frmSetting.revData.mOffset[visionNo].OnceMoveLimit.X;
                }
            }
            if (Menu.frmSetting.revData.mOffset[visionNo].OnceMoveLimit.Y != 0
                && Math.Abs(result.Y) > Menu.frmSetting.revData.mOffset[visionNo].OnceMoveLimit.Y)
            {
                if (result.Y > 0)
                {
                    LogDisp(visionNo, "OnceMoveLimit Y :" + result.Y.ToString("0.000") + "->" + Menu.frmSetting.revData.mOffset[visionNo].OnceMoveLimit.Y.ToString("0.000"));
                    result.Y = Menu.frmSetting.revData.mOffset[visionNo].OnceMoveLimit.Y;
                }
                else
                {
                    LogDisp(visionNo, "OnceMoveLimit Y :" + result.Y.ToString("0.000") + "->-" + Menu.frmSetting.revData.mOffset[visionNo].OnceMoveLimit.Y.ToString("0.000"));
                    result.Y = (-1) * Menu.frmSetting.revData.mOffset[visionNo].OnceMoveLimit.Y;
                }
            }
            if (Menu.frmSetting.revData.mOffset[visionNo].OnceMoveLimit.T != 0
                && Math.Abs(result.T) > Menu.frmSetting.revData.mOffset[visionNo].OnceMoveLimit.T)
            {
                if (result.T > 0)
                {
                    LogDisp(visionNo, "OnceMoveLimit T :" + result.T.ToString("0.000") + "->" + Menu.frmSetting.revData.mOffset[visionNo].OnceMoveLimit.T.ToString("0.000"));
                    result.T = Menu.frmSetting.revData.mOffset[visionNo].OnceMoveLimit.T;
                }
                else
                {
                    LogDisp(visionNo, "OnceMoveLimit T :" + result.T.ToString("0.000") + "->-" + Menu.frmSetting.revData.mOffset[visionNo].OnceMoveLimit.T.ToString("0.000"));
                    result.T = (-1) * Menu.frmSetting.revData.mOffset[visionNo].OnceMoveLimit.T;
                }
            }

            return result;
        }

        public bool GetTargetPos(eCalPos calpos)
        {
            //타겟값 반환(필요하면 조금수정해서 사용하자)
            //cs2DAlign.ptXY TargetMarkPixel1 = new cs2DAlign.ptXY();
            //cs2DAlign.ptXY TargetMarkPixel2 = new cs2DAlign.ptXY();

            //if (calpos == eCalPos.Conveyor1_1)
            //{
            //    TargetMarkPixel1.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO1.Conveyor].Target_Pos1.X;
            //    TargetMarkPixel1.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO1.Conveyor].Target_Pos1.Y;
            //    TargetMarkPixel2.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO1.Conveyor].Target_Pos2.X;
            //    TargetMarkPixel2.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO1.Conveyor].Target_Pos2.Y;
            //}
            //else if (calpos == eCalPos.LoadingBuffer1 || calpos == eCalPos.LoadingBuffer2)
            //{
            //    TargetMarkPixel1.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO1.LoadingBuffer1].Target_Pos1.X;
            //    TargetMarkPixel1.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO1.LoadingBuffer1].Target_Pos1.Y;
            //    TargetMarkPixel2.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO1.LoadingBuffer2].Target_Pos1.X;
            //    TargetMarkPixel2.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO1.LoadingBuffer2].Target_Pos1.Y;
            //}
            //else if (calpos == eCalPos.SCFPanel1 || calpos == eCalPos.SCFPanel2)
            //{
            //    TargetMarkPixel1.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO2.SCFPanel1].Target_Pos1.X;
            //    TargetMarkPixel1.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO2.SCFPanel1].Target_Pos1.Y;
            //    TargetMarkPixel2.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO2.SCFPanel2].Target_Pos1.X;
            //    TargetMarkPixel2.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO2.SCFPanel2].Target_Pos1.Y;
            //}
            //else if (calpos == eCalPos.SCFReel1 || calpos == eCalPos.SCFReel2)
            //{
            //    TargetMarkPixel1.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO2.SCFReel1].Target_Pos1.X;
            //    TargetMarkPixel1.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO2.SCFReel1].Target_Pos1.Y;
            //    TargetMarkPixel2.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO2.SCFReel2].Target_Pos1.X;
            //    TargetMarkPixel2.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO2.SCFReel2].Target_Pos1.Y;
            //}
            //else if (calpos == eCalPos.BendPre1 || calpos == eCalPos.BendPre2)
            //{
            //    TargetMarkPixel1.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO1.BendPre1].Target_Pos1.X;
            //    TargetMarkPixel1.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO1.BendPre1].Target_Pos1.Y;
            //    TargetMarkPixel2.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO1.BendPre2].Target_Pos1.X;
            //    TargetMarkPixel2.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO1.BendPre2].Target_Pos1.Y;
            //}
            //else if (calpos == eCalPos.Bend1Arm_L || calpos == eCalPos.Bend1Trans_L)
            //{
            //    calpos = eCalPos.Bend1Arm_L;

            //    TargetMarkPixel1.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO3.Bending1_1].Target_Pos1.X;
            //    TargetMarkPixel1.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO3.Bending1_1].Target_Pos1.Y;
            //    TargetMarkPixel2.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO3.Bending1_2].Target_Pos1.X;
            //    TargetMarkPixel2.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO3.Bending1_2].Target_Pos1.Y;
            //}
            //else if (calpos == eCalPos.Bend2Arm_L || calpos == eCalPos.Bend2Trans_L)
            //{
            //    calpos = eCalPos.Bend2Arm_L;

            //    TargetMarkPixel1.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO3.Bending2_1].Target_Pos1.X;
            //    TargetMarkPixel1.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO3.Bending2_1].Target_Pos1.Y;
            //    TargetMarkPixel2.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO3.Bending2_2].Target_Pos1.X;
            //    TargetMarkPixel2.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO3.Bending2_2].Target_Pos1.Y;
            //}
            //else if (calpos == eCalPos.Bend3Arm_L || calpos == eCalPos.Bend3Trans_L)
            //{
            //    calpos = eCalPos.Bend3Arm_L;

            //    TargetMarkPixel1.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO3.Bending3_1].Target_Pos1.X;
            //    TargetMarkPixel1.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO3.Bending3_1].Target_Pos1.Y;
            //    TargetMarkPixel2.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO3.Bending3_2].Target_Pos1.X;
            //    TargetMarkPixel2.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO3.Bending3_2].Target_Pos1.Y;
            //}
            //else if (calpos == eCalPos.Inspection1 || calpos == eCalPos.Inspection2)
            //{
            //    TargetMarkPixel1.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.Inspection1].Target_Pos1.X;
            //    TargetMarkPixel1.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.Inspection1].Target_Pos1.Y;
            //    TargetMarkPixel2.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.Inspection2].Target_Pos1.X;
            //    TargetMarkPixel2.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.Inspection2].Target_Pos1.Y;
            //}
            //else if (calpos == eCalPos.TempAttach1_1)
            //{
            //    TargetMarkPixel1.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.TempAttach].Target_Pos1.X;
            //    TargetMarkPixel1.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.TempAttach].Target_Pos1.Y;
            //    TargetMarkPixel2.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.TempAttach].Target_Pos2.X;
            //    TargetMarkPixel2.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.TempAttach].Target_Pos2.Y;
            //}
            //else if (calpos == eCalPos.TempAttach2_1)
            //{
            //    TargetMarkPixel1.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.TempAttach].Target_Pos3.X;
            //    TargetMarkPixel1.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.TempAttach].Target_Pos3.Y;
            //    TargetMarkPixel2.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.TempAttach].Target_Pos4.X;
            //    TargetMarkPixel2.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.TempAttach].Target_Pos4.Y;
            //}
            //else if (calpos == eCalPos.EMIAttach1_1)
            //{
            //    TargetMarkPixel1.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.EMIAttach].Target_Pos1.X;
            //    TargetMarkPixel1.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.EMIAttach].Target_Pos1.Y;
            //    TargetMarkPixel2.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.EMIAttach].Target_Pos2.X;
            //    TargetMarkPixel2.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.EMIAttach].Target_Pos2.Y;
            //}
            //else if (calpos == eCalPos.EMIAttach2_1)
            //{
            //    TargetMarkPixel1.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.EMIAttach].Target_Pos3.X;
            //    TargetMarkPixel1.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.EMIAttach].Target_Pos3.Y;
            //    TargetMarkPixel2.X = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.EMIAttach].Target_Pos4.X;
            //    TargetMarkPixel2.Y = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.EMIAttach].Target_Pos4.Y;
            //}

            return true;
        }


        public void ViewPRInspecData(double dLX, double dLY, double dRX, double dRY, string panelID)
        {
            //int nPreNumData = 0;
            //int nArmNumData = 0;
            //string strPanelIDData = "";

            //object[] dData = new object[7];

            //try
            //{
            //    nPreNumData = int.Parse(panelID.Substring(20, 1));
            //    nArmNumData = int.Parse(panelID.Substring(22, 1));
            //    strPanelIDData = panelID.Substring(0, 20);
            //}
            //catch
            //{
            //    nPreNumData = -10;
            //    nArmNumData = -10;
            //    strPanelIDData = "";
            //}

            //dData[0] = ((dLX - Vision[Vision_No.UpperInsp1_1].cogDS.Image.Width / 2) * Vision[Vision_No.UpperInsp1_1].CFG.ResolutionX).ToString("0.000");
            //dData[1] = ((dLY - Vision[Vision_No.UpperInsp1_1].cogDS.Image.Height / 2) * Vision[Vision_No.UpperInsp1_1].CFG.ResolutionY).ToString("0.000");
            //dData[2] = ((dRX - Vision[Vision_No.UpperInsp1_2].cogDS.Image.Width / 2) * Vision[Vision_No.UpperInsp1_2].CFG.ResolutionX).ToString("0.000");
            //dData[3] = ((dRY - Vision[Vision_No.UpperInsp1_2].cogDS.Image.Height / 2) * Vision[Vision_No.UpperInsp1_2].CFG.ResolutionY).ToString("0.000");
            //dData[4] = nPreNumData;
            //dData[5] = nArmNumData;
            //dData[6] = strPanelIDData;

            //cLog.Save(LogKind.PRUpperInsData, dData[4] + "," + dData[5] + "," + dData[6] + ","
            //                         + dData[0] + "," + dData[1] + "," + dData[2] + "," + dData[3]);
        }

        public bool SCFInspection(eCalPos calpos, cs2DAlign.ptXY RefMark1, cs2DAlign.ptXY RefMark2, cs2DAlign.ptXY LineMark1, cs2DAlign.ptXY LineMark2, ref cs2DAlign.ptXXYY Dist)
        {
            //scf찾아서 거리잼..
            //20.10.07 lkw
            Dist = Menu.rsAlign.getDistXY((int)calpos, RefMark1, LineMark1, (int)calpos + 1, RefMark2, LineMark2);

            //2018.07.10 추후 Spec들 추가
            double XSpec = 0;//double.Parse(CONST.RunRecipe.Param[CONST.rcpSCF_ATTACH_SPEC_X].Value);
            double YSpec = 0;//double.Parse(CONST.RunRecipe.Param[CONST.rcpSCF_ATTACH_SPEC_Y].Value);

            double SCFInspSpecX1 = 0;//double.Parse(CONST.RunRecipe.Param[CONST.rcpSCF_ATTACH_INSPECT_X1].Value);
            double SCFInspSpecY1 = 0;//double.Parse(CONST.RunRecipe.Param[CONST.rcpSCF_ATTACH_INSPECT_Y1].Value);
            double SCFInspSpecX2 = 0;//.Parse(CONST.RunRecipe.Param[CONST.rcpSCF_ATTACH_INSPECT_X2].Value);
            double SCFInspSpecY2 = 0;//double.Parse(CONST.RunRecipe.Param[CONST.rcpSCF_ATTACH_INSPECT_Y2].Value);

            if (Math.Abs(Dist.X1 - SCFInspSpecX1) > XSpec && Math.Abs(Dist.Y1 - SCFInspSpecY1) > YSpec &&
                Math.Abs(Dist.X2 - SCFInspSpecX2) > XSpec && Math.Abs(Dist.Y2 - SCFInspSpecY2) > YSpec)
            {
                //Inspection OK
                return true;
            }
            else
            {
                //Inspection NG
                return false;
            }
        }

        public void setCurrentPos(int MotionNo, eCalPos calpos)
        {
            Menu.frmAutoMain.IF.readPositionData(MotionNo);

            double CurrentPosX = visionPosition[MotionNo].XPos;
            double CurrentPosY = visionPosition[MotionNo].YPos;
            double CurrentPosT = visionPosition[MotionNo].TPos;

            Menu.rsAlign.setCurentPos((int)calpos, CurrentPosX, CurrentPosY, CurrentPosT);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int[] ab = new int[3];

            cLog.WriteOKNGCount("a", ab, 0);
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

        public void InspectionDistMeasure(int VisionNo, eCalPos calpos, CogLine[] heightLine, CogLine[] widthLine, cs2DAlign.ptXY mark1Pixel, cs2DAlign.ptXY mark2Pixel, ref cs2DAlign.ptXXYY dist)
        {
            double pixeldX1 = 0;
            double pixeldY1 = 0;
            Vision[VisionNo].PointToLinePixel(heightLine[0], mark1Pixel.X, mark1Pixel.Y, ref pixeldX1);
            Vision[VisionNo].PointToLinePixel(widthLine[0], mark1Pixel.X, mark1Pixel.Y, ref pixeldY1);

            double pixeldX2 = 0;
            double pixeldY2 = 0;
            Vision[VisionNo + 1].PointToLinePixel(heightLine[1], mark2Pixel.X, mark2Pixel.Y, ref pixeldX2);
            Vision[VisionNo + 1].PointToLinePixel(widthLine[1], mark2Pixel.X, mark2Pixel.Y, ref pixeldY2);

            cs2DAlign.ptXY resolution1 = new cs2DAlign.ptXY();
            cs2DAlign.ptXY resolution2 = new cs2DAlign.ptXY();
            cs2DAlign.ptXY pixelCnt = new cs2DAlign.ptXY();
            Menu.rsAlign.getResolution((int)calpos, ref resolution1, ref pixelCnt);
            Menu.rsAlign.getResolution((int)calpos + 1, ref resolution2, ref pixelCnt);

            dist.X1 = Math.Abs(pixeldX1 * resolution1.X);
            dist.Y1 = Math.Abs(pixeldY1 * resolution1.Y);
            dist.X2 = Math.Abs(pixeldX2 * resolution2.X);
            dist.Y2 = Math.Abs(pixeldY2 * resolution2.Y);
        }

        private void rbGraphDraw_CheckedChanged(object sender, EventArgs e)
        {
            this.btnDisp1_Click(this, null);
        }

        //2019.07.20 EMI Align 추가
        //public cs2DAlign.ptXY InspectionToEMIPixelTrans(int VisionNO, cs2DAlign.ptXY inspPixel)
        //{
        //    //카메라 셋팅 정확하고 이동량 미비할 것으로 판단하고 간단히 산수로 함.
        //    double diffX = 0;
        //    double diffY = 0;

        //    cs2DAlign.ptXY EMIresol = new cs2DAlign.ptXY();
        //    cs2DAlign.ptXY EMIPixelCnt = new cs2DAlign.ptXY();
        //    Menu.rsAlign.getResolution((int)eCalPos.EMIAttach, ref EMIresol, ref EMIPixelCnt);

        //    cs2DAlign.ptXY Inspection1resol = new cs2DAlign.ptXY();
        //    cs2DAlign.ptXY Inspection1PixelCnt = new cs2DAlign.ptXY();
        //    Menu.rsAlign.getResolution((int)eCalPos.Inspection_L, ref Inspection1resol, ref Inspection1PixelCnt);

        //    cs2DAlign.ptXY Inspection2resol = new cs2DAlign.ptXY();
        //    cs2DAlign.ptXY Inspection2PixelCnt = new cs2DAlign.ptXY();
        //    Menu.rsAlign.getResolution((int)eCalPos.Inspection_R, ref Inspection2resol, ref Inspection2PixelCnt);

        //    EMIresol.X = Math.Abs(EMIresol.X);
        //    EMIresol.Y = Math.Abs(EMIresol.Y);

        //    Inspection1resol.X = Math.Abs(Inspection1resol.X);
        //    Inspection1resol.Y = Math.Abs(Inspection1resol.Y);

        //    Inspection2resol.X = Math.Abs(Inspection2resol.X);
        //    Inspection2resol.Y = Math.Abs(Inspection2resol.Y);

        //    cs2DAlign.ptXY returnData = new cs2DAlign.ptXY();
        //    if (VisionNO == Vision_No.vsInspection1)
        //    {
        //        diffX = (CONST.EMITargetPosX1 * EMIresol.X) - (Menu.frmSetting.revData.mLcheck[(short)eCamNO4.Inspection1].Target_Pos.X * Inspection1resol.X);  //mm 차이
        //        diffY = (CONST.EMITargetPosY1 * EMIresol.Y) - (Menu.frmSetting.revData.mLcheck[(short)eCamNO4.Inspection1].Target_Pos.Y * Inspection1resol.Y);

        //        returnData.X = ((inspPixel.X * Inspection1resol.X) + diffX) / EMIresol.X;
        //        returnData.Y = ((inspPixel.Y * Inspection1resol.Y) + diffY) / EMIresol.Y;
        //    }
        //    else if (VisionNO == Vision_No.vsInspection2)
        //    {
        //        diffX = (CONST.EMITargetPosX2 * EMIresol.X) - (Menu.frmSetting.revData.mLcheck[(short)eCamNO4.Inspection2].Target_Pos.X * Inspection2resol.X);
        //        diffY = (CONST.EMITargetPosY2 * EMIresol.Y) - (Menu.frmSetting.revData.mLcheck[(short)eCamNO4.Inspection2].Target_Pos.Y * Inspection2resol.Y);

        //        returnData.X = ((inspPixel.X * Inspection2resol.X) + diffX) / EMIresol.X;
        //        returnData.Y = ((inspPixel.Y * Inspection2resol.Y) + diffY) / EMIresol.Y;
        //    }

        //    return returnData;
        //}

        public void AutoScale(int BendingNO)
        {
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

        private void tmrSave_Tick(object sender, EventArgs e)
        {
            //if (cbSideImageSize[CONST.bSideImageSize[0]].SelectedIndex == 0) cbSideImageSize[CONST.bSideImageSize[0]].BackColor = Color.White;
            //else if (cbSideImageSize[CONST.bSideImageSize[0]].SelectedIndex == 1 || cbSideImageSize[CONST.bSideImageSize[0]].SelectedIndex == 2) cbSideImageSize[CONST.bSideImageSize[0]].BackColor = Color.Orange;
            //else if (cbSideImageSize[CONST.bSideImageSize[0]].SelectedIndex == 3) cbSideImageSize[CONST.bSideImageSize[0]].BackColor = Color.Red;

            //if (cbSideImageSize[CONST.bSideImageSize[1]].SelectedIndex == 0) cbSideImageSize[CONST.bSideImageSize[1]].BackColor = Color.White;
            //else if (cbSideImageSize[CONST.bSideImageSize[1]].SelectedIndex == 1 || cbSideImageSize[CONST.bSideImageSize[1]].SelectedIndex == 2) cbSideImageSize[CONST.bSideImageSize[1]].BackColor = Color.Orange;
            //else if (cbSideImageSize[CONST.bSideImageSize[1]].SelectedIndex == 3) cbSideImageSize[CONST.bSideImageSize[1]].BackColor = Color.Red;

            //if (cbSideImageSize[CONST.bSideImageSize[2]].SelectedIndex == 0) cbSideImageSize[CONST.bSideImageSize[2]].BackColor = Color.White;
            //else if (cbSideImageSize[CONST.bSideImageSize[2]].SelectedIndex == 1 || cbSideImageSize[CONST.bSideImageSize[2]].SelectedIndex == 2) cbSideImageSize[CONST.bSideImageSize[2]].BackColor = Color.Orange;
            //else if (cbSideImageSize[CONST.bSideImageSize[2]].SelectedIndex == 3) cbSideImageSize[CONST.bSideImageSize[2]].BackColor = Color.Red;

            //if (cbSideImageSize[CONST.bSideImageSize[0]].SelectedIndex < 0) cbSideImageSize[CONST.bSideImageSize[0]].SelectedIndex = 0;
            //if (cbSideImageSize[CONST.bSideImageSize[1]].SelectedIndex < 0) cbSideImageSize[CONST.bSideImageSize[1]].SelectedIndex = 0;
            //if (cbSideImageSize[CONST.bSideImageSize[2]].SelectedIndex < 0) cbSideImageSize[CONST.bSideImageSize[2]].SelectedIndex = 0;

            //if (CONST.bSideFileSave[1])
            //{
            //    //Vision[Vision_No.vsBendSide1].Capture2(1, cbSideImageSize[CONST.bSideImageSize[0]].SelectedIndex); //확인 사이드 두개다 저장해야함.
            //    //Vision[Vision_No.vsBendSide1_2].Capture2(1, cbSideImageSize[CONST.bSideImageSize[0]].SelectedIndex, true);
            //    CONST.bSideFileSave[1] = false;
            //    //2017.12.04
            //    if (cbSideImageSize[CONST.bSideImageSize[0]].SelectedIndex == 0)
            //    {
            //        CaptureCnt1 = 0;
            //    }
            //    else if (cbSideImageSize[CONST.bSideImageSize[0]].SelectedIndex == 1 || cbSideImageSize[CONST.bSideImageSize[0]].SelectedIndex == 2 || cbSideImageSize[CONST.bSideImageSize[0]].SelectedIndex == 3)
            //    {
            //        CaptureCnt1++;
            //        if (CaptureCnt1 == 10)
            //        {
            //            cbSideImageSize[CONST.bSideImageSize[0]].SelectedIndex = 0;
            //            CaptureCnt1 = 0;
            //        }
            //    }
            //}
            //if (CONST.bSideFileSave[2])
            //{
            //    //Vision[Vision_No.vsBendSide2].Capture2(2, cbSideImageSize[CONST.bSideImageSize[1]].SelectedIndex);
            //    //Vision[Vision_No.vsBendSide2_2].Capture2(2, cbSideImageSize[CONST.bSideImageSize[1]].SelectedIndex, true);
            //    CONST.bSideFileSave[2] = false;
            //    //2017.12.04
            //    if (cbSideImageSize[CONST.bSideImageSize[1]].SelectedIndex == 0)
            //    {
            //        CaptureCnt2 = 0;
            //    }
            //    else if (cbSideImageSize[CONST.bSideImageSize[1]].SelectedIndex == 1 || cbSideImageSize[CONST.bSideImageSize[1]].SelectedIndex == 2 || cbSideImageSize[CONST.bSideImageSize[1]].SelectedIndex == 3)
            //    {
            //        CaptureCnt2++;
            //        if (CaptureCnt2 == 10)
            //        {
            //            cbSideImageSize[CONST.bSideImageSize[1]].SelectedIndex = 0;
            //            CaptureCnt2 = 0;
            //        }
            //    }
            //}
            //if (CONST.bSideFileSave[3])
            //{
            //    //Vision[Vision_No.vsBendSide3].Capture2(3, cbSideImageSize[CONST.bSideImageSize[2]].SelectedIndex);
            //    //Vision[Vision_No.vsBendSide3_2].Capture2(3, cbSideImageSize[CONST.bSideImageSize[2]].SelectedIndex, true);
            //    CONST.bSideFileSave[3] = false;
            //    //2017.12.04
            //    if (cbSideImageSize[CONST.bSideImageSize[2]].SelectedIndex == 0)
            //    {
            //        CaptureCnt3 = 0;
            //    }
            //    else if (cbSideImageSize[CONST.bSideImageSize[2]].SelectedIndex == 1 || cbSideImageSize[CONST.bSideImageSize[2]].SelectedIndex == 2 || cbSideImageSize[CONST.bSideImageSize[2]].SelectedIndex == 3)
            //    {
            //        CaptureCnt3++;
            //        if (CaptureCnt3 == 10)
            //        {
            //            cbSideImageSize[CONST.bSideImageSize[2]].SelectedIndex = 0;
            //            CaptureCnt3 = 0;
            //        }
            //    }
            //}
        }

        private void SideVar_init(int BendingNo)
        {
            //CONST.BendingsideCapture[ManualNo] = false;
            //CONST.BendingPressCapture[ManualNo] = false;
            //CONST.BendingCaptureNo[ManualNo] = 0;
            //TestNo[ManualNo] = 0;
        }

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
        private List<bool> FindLineThread(PParam param, short visionNo1, bool capture1, out List<cs2DAlign.ptXYT> pt, out List<bool> result)
        {
            //라인은 한카메라씩찾음
            Stopwatch timewatch = new Stopwatch();
            timewatch.Start();
            //오직 2라인 1점 or 3라인 2점 계산
            //라인 3개일때 순서 세로세로가로 또는 가로가로세로
            int iCnt = param.qLkind.Count;

            pt = new List<cs2DAlign.ptXYT>();
            result = new List<bool>();

            BlockingCollection<lineSearchResult>[] bc = new BlockingCollection<lineSearchResult>[iCnt];
            CogLine[] lines = new CogLine[iCnt];
            bool[] lcresult = new bool[iCnt];

            for (int i = 0; i < iCnt; i++)
            {
                bc[i] = new BlockingCollection<lineSearchResult>();
                lines[i] = new CogLine();
                lcresult[i] = new bool();
            }

            for (int i = 0; i < iCnt; i++)
            {
                int no = visionNo1;
                if (param.oneCamCapture)
                {
                    if (i == 0)
                    {
                        Vision[no].SetLineLightExpCont(param.qLkind.Peek());
                        Vision[no].Capture(false, true, false, true);//1cam 2마크찾기일때는 캡쳐를 밖에서한번 함 두번째 패턴찾기가 이미지 갱신전에 찾음.
                    }
                }

                if (param.oneCamCapture) Vision[no].LineSearch_Thread(i, bc[i], false, param.qLkind.Dequeue(), param.bNographics);
                else Vision[no].LineSearch_Thread(i, bc[i], capture1, param.qLkind.Dequeue(), param.bNographics);

                bc[i].TryTake(out lineSearchResult rvalue1, -1);
                lines[i] = rvalue1.line;
                lcresult[i] = rvalue1.result;
                bc[i].Dispose();
            }

            bool btemp1 = false;
            bool btemp2 = false;
            if (iCnt == 2) //2라인 1포인트
            {
                cs2DAlign.ptXYT temp = new cs2DAlign.ptXYT();
                if (lcresult[0] && lcresult[1])
                {
                    btemp1 = Vision[visionNo1].GetPointfrom2Line(lines[0], lines[1], ref temp.X, ref temp.Y, ref temp.T, param.bNographics);
                }
                pt.Add(temp);
                result.Add(btemp1);
                btemp2 = btemp1;
            }
            else if (iCnt == 3) //3라인 2포인트
            {
                cs2DAlign.ptXYT temp1 = new cs2DAlign.ptXYT();
                if (lcresult[0] && lcresult[2])
                {
                    btemp1 = Vision[visionNo1].GetPointfrom2Line(lines[0], lines[2], ref temp1.X, ref temp1.Y, ref temp1.T, param.bNographics);
                }
                pt.Add(temp1);
                result.Add(btemp1);

                cs2DAlign.ptXYT temp2 = new cs2DAlign.ptXYT();
                if (lcresult[1] && lcresult[2])
                {
                    btemp2 = Vision[visionNo1].GetPointfrom2Line(lines[1], lines[2], ref temp2.X, ref temp2.Y, ref temp2.T, param.bNographics);
                }
                pt.Add(temp2);
                result.Add(btemp2);
            }

            timewatch.Stop();

            string str = timewatch.ElapsedMilliseconds.ToString();

            WriteCogLabel(str + "ms", visionNo1, CONST.font10Bold, CogColorConstants.Orange, 400, 100, false, "RecogTime");

            if (btemp1 && btemp2)
            {
                Vision[visionNo1].SetnSearchResult(1);
            }
            else if (param.failimagenotsave)
            {
                Vision[visionNo1].SetnSearchResult(1);
            }
            else
            {
                Vision[visionNo1].SetnSearchResult(2);
            }

            return result;

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
        //SetFindResult함수를 결과개수마다 오버라이드 해놔야 편할듯..
        //마크 찾기전 조건을 걸어주는 경우가 있어서 out보다 ref사용
        //20.10.07 lkw 이건 좀 바꿔야 할듯......
        private void SetFindResult(cs2DAlign.ptXYT[] ptinput, bool[] binput, ref bool b1, ref bool b2, ref cs2DAlign.ptXYT pt1, ref cs2DAlign.ptXYT pt2)
        {
            //2개용
            b1 = binput[0];
            pt1 = ptinput[0];

            b2 = binput[1];
            pt2 = ptinput[1];
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
        private void SetFindResult(List<cs2DAlign.ptXYT> ptinput, List<bool> binput, ref bool b1, ref bool b2, ref bool b3, ref cs2DAlign.ptXYT pt1, ref cs2DAlign.ptXYT pt2, ref cs2DAlign.ptXYT pt3)
        {
            //3개용
            b1 = binput[0];
            b2 = binput[1];
            b3 = binput[2];
            pt1 = ptinput[0];
            pt2 = ptinput[1];
            pt3 = ptinput[2];
        }
        private void SetFindResult(List<cs2DAlign.ptXYT> ptinput, List<bool> binput, ref bool b1, ref bool b2, ref bool b3, ref bool b4,
            ref cs2DAlign.ptXYT pt1, ref cs2DAlign.ptXYT pt2, ref cs2DAlign.ptXYT pt3, ref cs2DAlign.ptXYT pt4)
        {
            //4개용
            b1 = binput[0];
            b2 = binput[1];
            b3 = binput[2];
            b4 = binput[3];
            pt1 = ptinput[0];
            pt2 = ptinput[1];
            pt3 = ptinput[2];
            pt4 = ptinput[3];
        }
        //20200925 cjm SCF Inspection Result <-> Graph
        private void cbSCFInspResult_CheckedChanged(object sender, EventArgs e)
        {

            //    if (cbSCFInspResult.Checked)
            //    {
            //        pngraph.Visible = false;
            //        pnResult1.Visible = true;
            //        //BendingPreSCFInspection();
            //    }
            //    else
            //    {
            //        pngraph.Visible = true;
            //        pnResult1.Visible = false;
            //    }
        }

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
        private bool compareLight(short visionNO, ePatternKind condition1, ePatternKind condition2)
        {
            int light1 = Vision[visionNO].CFG.Light[(int)condition1];
            int light2 = Vision[visionNO].CFG.Light[(int)condition2];

            int exposure1 = Vision[visionNO].CFG.Exposure[(int)condition1];
            int exposure2 = Vision[visionNO].CFG.Exposure[(int)condition2];

            double contrast1 = Vision[visionNO].CFG.Contrast[(int)condition1];
            double contrast2 = Vision[visionNO].CFG.Contrast[(int)condition2];

            if (light1 == light2 && exposure1 == exposure2 && contrast1 == contrast2) return true;
            else return false;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            //CogPMAlignPattern temp1 = new CogPMAlignPattern();
            //CogPMAlignPattern temp2 = new CogPMAlignPattern();
            //CogPMAlignPattern temp3 = temp1;
            //Console.WriteLine(temp1.GetHashCode());
            //Console.WriteLine(temp2.GetHashCode());
            //Console.WriteLine(temp3.GetHashCode());
            //L4Logger.Instance.Add2("123");
            //CONST.Log.Add("test");
            //ManualMarkInitial(0);
            //Bitmap bmp = new Bitmap(@"C:\Users\PCY\Pictures\1.bmp");
            //CogImage8Grey temp = new CogImage8Grey(bmp);
            //Vision[0].cogDS.Image = temp;
            //manualMark[0].pcresult = "test";
            //manualMark[0].MarkInfo[mPanel].manualPopup = true;
            //manualMark[0].MarkInfo[mPanel].patternKind = ePatternKind.Panel;
            //CheckManualMarkPopup(0);  //수동 마크창 세팅
            if (comboBox1.SelectedIndex < 0) comboBox1.SelectedIndex = 0;
            int selectI = comboBox1.SelectedIndex;
            CONST.bPLCReq[selectI] = true;

        }

        //20.12.17 lkw DL
        private int DL_ConnectionNOMS(short visionNO, int index)
        {
            //MarkSearch_Use.Length와 DefectFind_Use.Length개수가 동일하니까 공통으로 써도 상관없음
            return visionNO * Menu.frmSetting.revData.mDL[visionNO].MarkSearch_Use.Length + index;
        }
        private int DL_ConnectionNODF(short visionNO, int index)
        {
            //MarkSearch_Use.Length와 DefectFind_Use.Length개수가 동일하니까 공통으로 써도 상관없음
            return visionNO * Menu.frmSetting.revData.mDL[visionNO].DefectFind_Use.Length + index;
        }

        public void getDetachOffset(string visionNO, ref double offsetX, ref double offsetY, ref double ckTH)
        {
            //switch (visionNO)
            //{
            //    case nameof(eCamNO.Attach1_1):
            //        offsetX = Menu.frmSetting.revData.mAttach.DetachOffsetX1_1;
            //        offsetY = Menu.frmSetting.revData.mAttach.DetachOffsetY1_1;
            //        ckTH = Menu.frmSetting.revData.mAttach.DetachLimitTH1_1;
            //        break;
            //    case nameof(eCamNO.Attach1_2):
            //        offsetX = Menu.frmSetting.revData.mAttach.DetachOffsetX1_2;
            //        offsetY = Menu.frmSetting.revData.mAttach.DetachOffsetY1_2;
            //        ckTH = Menu.frmSetting.revData.mAttach.DetachLimitTH1_2;
            //        break;
            //    case nameof(eCamNO.Attach1_3):
            //        offsetX = Menu.frmSetting.revData.mAttach.DetachOffsetX2_1;
            //        offsetY = Menu.frmSetting.revData.mAttach.DetachOffsetY2_1;
            //        ckTH = Menu.frmSetting.revData.mAttach.DetachLimitTH2_1;
            //        break;
            //    case nameof(eCamNO.Attach1_4):
            //        offsetX = Menu.frmSetting.revData.mAttach.DetachOffsetX2_2;
            //        offsetY = Menu.frmSetting.revData.mAttach.DetachOffsetY2_2;
            //        ckTH = Menu.frmSetting.revData.mAttach.DetachLimitTH2_2;
            //        break;
            //    case nameof(eCamNO.Attach2_1):
            //        offsetX = Menu.frmSetting.revData.mAttach.DetachOffsetX1_1;
            //        offsetY = Menu.frmSetting.revData.mAttach.DetachOffsetY1_1;
            //        ckTH = Menu.frmSetting.revData.mAttach.DetachLimitTH1_1;
            //        break;
            //    case nameof(eCamNO.Attach2_2):
            //        offsetX = Menu.frmSetting.revData.mAttach.DetachOffsetX1_2;
            //        offsetY = Menu.frmSetting.revData.mAttach.DetachOffsetY1_2;
            //        ckTH = Menu.frmSetting.revData.mAttach.DetachLimitTH1_2;
            //        break;
            //    case nameof(eCamNO.Attach2_3):
            //        offsetX = Menu.frmSetting.revData.mAttach.DetachOffsetX2_1;
            //        offsetY = Menu.frmSetting.revData.mAttach.DetachOffsetY2_1;
            //        ckTH = Menu.frmSetting.revData.mAttach.DetachLimitTH2_1;
            //        break;
            //    case nameof(eCamNO.Attach2_4):
            //        offsetX = Menu.frmSetting.revData.mAttach.DetachOffsetX2_2;
            //        offsetY = Menu.frmSetting.revData.mAttach.DetachOffsetY2_2;
            //        ckTH = Menu.frmSetting.revData.mAttach.DetachLimitTH2_2;
            //        break;
            //}
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
    public class AlignParam
    {
        //Align함수를 돌리기 위한 각종 값들 모음
    }


}