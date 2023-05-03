using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using rs2DAlign;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using Cognex.VisionPro.Blob;
//using Excel = Microsoft.Office.Interop.Excel;

namespace Bending
{
    public partial class ucRecipe : UserControl
    {
        private eCalPos calPos_L;
        private eCalPos calPos_R;
        private eCalPos calPos_3;
        private eCalPos calPos_4;

        [DllImport("User32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        public class LiveEndEventArgs : EventArgs
        {
            public bool liveCheck = false;
        }

        // 마지막 메뉴 선택정보.
        private enum eLastMenuChoice
        {
            Open,
            Live,
        }

        public static csVision[] Vision = new csVision[4];
        public frmMessage frmMsg = new frmMessage();
        private csLog cLog = new csLog();
        private npointcal npoint = new npointcal();
        public csConfig cCFG = new csConfig(); // 20200904 cjm 추가
        public csVision cVision = new csVision(); // 20200904 cjm 추가

        [DllImport("kernel32")]
        public static extern int SetSystemTime(ref SYSTEMTIME lpSystemTime);

        public struct SYSTEMTIME
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

        public struct sTraceData
        {
            public double[] Value;
        }

        private sTraceData[] SData = new sTraceData[3];

        public enum eRecipeControlKind
        {
            [Description("ParamName")]
            lridx,

            [Description("ParamValue")]
            ridx,

            [Description("ParamUnit")]
            ruidx,

            [Description("ParamUpper")]
            rupidx,

            [Description("ParamLower")]
            rlwidx,
        }

        Button[] btnLightDown;
        TrackBar[] tb;
        Button[] btnLightUp;
        TextBox[] txtLight;
        Button[] btnExposureDown;
        TrackBar[] tbExposure;
        Button[] btnExposureUp;
        TextBox[] txtExposure;
        TextBox[] txtContrast;
        GroupBox[] gbCam;
        TextBox[] txtX;
        TextBox[] txtY;
        TextBox[] txtR;
        TextBox[] txtTargetX;
        TextBox[] txtTargetY;
        Button[] btnLightSave;
        TextBox[] txtIsLight;
        TextBox[] txtIsExposure;
        TextBox[] txtIsContrast;
        Button[] btnSet;

        public ucRecipe()
        {
            InitializeComponent();

            btnLightDown = new Button[] { btnLightDown1, btnLightDown2, btnLightDown3, btnLightDown4 };
            tb = new TrackBar[] { tb1, tb2, tb3, tb4 };
            btnLightUp = new Button[] { btnLightUp1, btnLightUp2, btnLightUp3, btnLightUp4 };
            txtLight = new TextBox[] { txtLight1, txtLight2, txtLight3, txtLight4 };
            btnExposureDown = new Button[] { btnExposureDown1, btnExposureDown2, btnExposureDown3, btnExposureDown4 };
            tbExposure = new TrackBar[] { tbExposure1, tbExposure2, tbExposure3, tbExposure4 };
            btnExposureUp = new Button[] { btnExposureUp1, btnExposureUp2, btnExposureUp3, btnExposureUp4 };
            txtExposure = new TextBox[] { txtExposure1, txtExposure2, txtExposure3, txtExposure4 };
            txtContrast = new TextBox[] { txtContrast1, txtContrast2, txtContrast3, txtContrast4 };
            gbCam = new GroupBox[] { gbCam1, gbCam2, gbCam3, gbCam4 };
            txtX = new TextBox[] { txtX1, txtX2, txtX3, txtX4 };
            txtY = new TextBox[] { txtY1, txtY2, txtY3, txtY4 };
            txtR = new TextBox[] { txtR1, txtR2, txtR3, txtR4 };
            txtTargetX = new TextBox[] { txtTargetX1, txtTargetX2, txtTargetX3, txtTargetX4 };
            txtTargetY = new TextBox[] { txtTargetY1, txtTargetY2, txtTargetY3, txtTargetY4 };
            btnLightSave = new Button[] { btnLightSave1, btnLightSave2, btnLightSave3, btnLightSave4 };
            txtIsLight = new TextBox[] { txtIsLight1, txtIsLight2, txtIsLight3, txtIsLight4 };
            txtIsExposure = new TextBox[] { txtIsExposure1, txtIsExposure2, txtIsExposure3, txtIsExposure4 };
            txtIsContrast = new TextBox[] { txtIsContrast1, txtIsContrast2, txtIsContrast3, txtIsContrast4 };
            btnSet = new Button[] { btnSet1, btnSet2, btnSet3, btnSet4 };

            cbRecognition.Items.Clear();
            cbRecognition.DropDownStyle = ComboBoxStyle.DropDownList;
            cbRecognition.DataSource = Enum.GetValues(typeof(ePatternKind));

            cbEdgeLine.Items.Clear();
            cbEdgeLine.DropDownStyle = ComboBoxStyle.DropDownList;
            cbEdgeLine.DataSource = Enum.GetValues(typeof(eLineKind));

            cbAlignMode.Items.Clear();
            cbAlignMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cbAlignMode.DataSource = Enum.GetValues(typeof(eInspMode));

            //20201003 cjm ComboBox에서 Cal 선택
            cbCal.Items.Clear();
            cbCal.DropDownStyle = ComboBoxStyle.DropDownList;
            cbCal.DataSource = Enum.GetValues(typeof(eCalPos));

            for (int i = 0; i < CONST.CAMCnt; i++)
            {
                if (Menu.frmAutoMain.Vision[i].CFG.Use)
                    cbCamList.Items.Add(Menu.frmAutoMain.Vision[i].CFG.Name);
                else
                    cbCamList.Items.Add("Not Use");
            }

            if (CONST.RunRecipe.RecipeName != "")
            {
                if (CONST.RunRecipe.OldRecipeName != CONST.RunRecipe.RecipeName)
                {
                    if (CONST.RunRecipe.RecipeName == null) CONST.RunRecipe.RecipeName = "TEST";

                    RecipChangeDataCopy();
                    CONST.RunRecipe.OldRecipeName = CONST.RunRecipe.RecipeName;
                    Menu.Config.RecipeNameWrite("RecipeName", "RecipeID", CONST.RunRecipe.OldRecipeName);
                    //Menu.frmAutoMain.HeightSpecRead();
                }
            }
            else
            {
                MessageBox.Show("Recipe ID Check");
            }

            tb1.Maximum = 255;
            tb2.Maximum = 255;

            RecipeParamDisp();

            for (int i = 0; i < 3; i++)
            {
                SData[i].Value = new double[100];
            }

            //tmr_RCP.Enabled = true;

            cbRecognition.SelectedIndex = 0;
            
            TraceDataList();
            for (int i = 0; i < 100; i++)
            {
                CONST.m_dMainTraceY[i] = 0;
                CONST.m_dMainTraceZ[i] = 0;
                CONST.m_dMainTraceT[i] = 0;

                CONST.m_dTraceY[0, i] = 0;
                CONST.m_dTraceY[1, i] = 0;
                CONST.m_dTraceY[2, i] = 0;
                CONST.m_dTraceZ[0, i] = 0;
                CONST.m_dTraceZ[1, i] = 0;
                CONST.m_dTraceZ[2, i] = 0;
                CONST.m_dTraceT[0, i] = 0;
                CONST.m_dTraceT[1, i] = 0;
                CONST.m_dTraceT[2, i] = 0;
            }

            dataGridView_MotionTrace.Columns.Add("caption", "Y Data");
            dataGridView_MotionTrace.Columns.Add("caption1", "Z Data");
            dataGridView_MotionTrace.Columns.Add("caption2", "T Data");

            dataGridView_MotionTrace.Columns[0].Width = 70;
            dataGridView_MotionTrace.Columns[1].Width = 70;
            dataGridView_MotionTrace.Columns[2].Width = 70;
            dataGridView_MotionTrace.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView_MotionTrace.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView_MotionTrace.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;

            //210217 cjm Change
            if (CONST.PCNo == 1) //etc..
            {
                pnInspLengthTest.Visible = false;
                tbTrace.TabPages.RemoveAt(4);   //Trace
                tbTrace.TabPages.RemoveAt(3);   //Height
            }
            
        }

        private int CalVisionNO;
        private eCalType CalType;
        eConvert TYRCal;

        private bool YTCal = false; //pcy190329 주석처리. //pcy190729 SDD 1층설비에 필요해서 다시 살림

        #region Calibration

        private int CalOK(int VisionNO, int calKind) //calKind : Calibration 종류 (ex : 0 BendingArm, 1 : Bending Transfer)
        {
            int iBitNo = 0;
            switch (VisionNO)
            {
                case Vision_No.LoadingPre1:
                        iBitNo = CalControl.pcRequest.LoadingPre1CalStart;
                    if (calKind == 1) iBitNo = CalControl.pcRequest.LoadingPre2CalStart;
                    break;
                case Vision_No.LoadingPre2:
                    iBitNo = CalControl.pcRequest.LoadingPre2CalStart;
                    break;

                case Vision_No.Laser1:
                    iBitNo = CalControl.pcRequest.LaserAlign1CalStart;
                    break;
                case Vision_No.Laser2:
                    iBitNo = CalControl.pcRequest.LaserAlign2CalStart;
                    break;
            }
            return iBitNo;
        }

        private bool CalOKCheck()
        {
            //PLC1,2 하나로 통합해도 됨
            return CONST.bCalReply[CalControl.plcReply.CalStartOK];
        }

        private bool CalNGCheck()
        {
            return CONST.bCalReply[CalControl.plcReply.CalStartNG];
        }

        private int PosMove(int VisionNO, int calKind)
        {
            int iBitNo = 0;
            switch (VisionNO)
            {
                case Vision_No.LoadingPre1:
                    iBitNo = CalControl.pcRequest.LoadingPre1CalMove;
                    if (calKind == 1) iBitNo = CalControl.pcRequest.LoadingPre2CalMove;
                    break;
                case Vision_No.LoadingPre2:
                    iBitNo = CalControl.pcRequest.LoadingPre2CalMove;
                    break;

                case Vision_No.Laser1:
                    iBitNo = CalControl.pcRequest.LaserAlign1CalMove;
                    break;
                case Vision_No.Laser2:
                    iBitNo = CalControl.pcRequest.LaserAlign2CalMove;
                    break;
            }
            return iBitNo;
        }

        private bool PosMoveComplete(int VisionNO, int calKind)
        {
            //켜졌는지 확인
            bool breturn = false;
            switch (VisionNO)
            {
                case Vision_No.LoadingPre1:
                        breturn = CONST.bCalReply[CalControl.plcReply.LoadingPre1CalMove];
                    if (calKind == 1) breturn = CONST.bCalReply[CalControl.plcReply.LoadingPre2CalMove];
                    break;
                case Vision_No.LoadingPre2:
                    breturn = CONST.bCalReply[CalControl.plcReply.LoadingPre2CalMove];
                    break;

                case Vision_No.Laser1:
                    breturn = CONST.bCalReply[CalControl.plcReply.LaserAlign1CalMove];
                    break;
                case Vision_No.Laser2:
                    breturn = CONST.bCalReply[CalControl.plcReply.LaserAlign2CalMove];
                    break;
            }
            return breturn;
        }

        private bool PosMoveReplyClear(int VisionNO, int calKind)
        {
            //꺼졌는지 확인
            bool breturn = !PosMoveComplete(VisionNO, calKind);
            return breturn;
        }

        #endregion Calibration

        public void Tb_trace_Init()
        {
            tbTrace.SelectedIndex = 0;
        }

        private class UVW
        {
            public double X1, X2, Y1, Y2;
        }

        private UVW startUVW = new UVW();
        private XYT startXYT = new XYT();

        //2019.07.20 EMI Align 추가
        private UVW[] calPosUVW = new UVW[9];

        private UVW[] calPosUVWBacklash = new UVW[9];
        private XYT[] calPosXYT = new XYT[9];
        private XYT[] calPosXYTBacklash = new XYT[9];

        private cs2DAlign.ptXXYY[] fixPosUVW = new cs2DAlign.ptXXYY[2];
        private cs2DAlign.ptXXYY[] fixPosUVWBacklash = new cs2DAlign.ptXXYY[2];
        private XYT[] fixPosXYT = new XYT[2];
        private XYT[] fixPosXYTBacklash = new XYT[2];
        private XYT[] fixPosTYR = new XYT[2];

        private List<cs2DAlign.ptXY> calPosCam1 = new List<cs2DAlign.ptXY>();
        private List<cs2DAlign.ptXY> calPosCam2 = new List<cs2DAlign.ptXY>();
        private List<cs2DAlign.ptXY> calPosCam3 = new List<cs2DAlign.ptXY>();
        private List<cs2DAlign.ptXY> EMICalPosCam1 = new List<cs2DAlign.ptXY>();
        private List<cs2DAlign.ptXY> YTCalPosCam1 = new List<cs2DAlign.ptXY>();
        private List<cs2DAlign.ptXY> YTCalPosCam2 = new List<cs2DAlign.ptXY>();
        private List<cs2DAlign.ptXY> YTCalPosCam3 = new List<cs2DAlign.ptXY>();

        private rs2DAlign.cs2DAlign.ptXY[] fixPosCam1 = new rs2DAlign.cs2DAlign.ptXY[2];
        private rs2DAlign.cs2DAlign.ptXY[] fixPosCam2 = new rs2DAlign.cs2DAlign.ptXY[2];
        private rs2DAlign.cs2DAlign.ptXY[] fixPosCam3 = new rs2DAlign.cs2DAlign.ptXY[2];
        private rs2DAlign.cs2DAlign.ptXY[] fixPosCam4 = new rs2DAlign.cs2DAlign.ptXY[2];

        private rs2DAlign.cs2DAlign.ptXY Resolution1 = new cs2DAlign.ptXY();
        private rs2DAlign.cs2DAlign.ptXY Resolution2 = new cs2DAlign.ptXY();
        private rs2DAlign.cs2DAlign.ptXY Resolution3 = new cs2DAlign.ptXY();
        private rs2DAlign.cs2DAlign.ptXY PixelCnt1 = new cs2DAlign.ptXY();
        private rs2DAlign.cs2DAlign.ptXY PixelCnt2 = new cs2DAlign.ptXY();
        private rs2DAlign.cs2DAlign.ptXY PixelCnt3 = new cs2DAlign.ptXY();

        private XYT firstCoord = new XYT();
        private XYT secondCoord = new XYT();

        private double dPixelX1 = new double();
        private double dPixelY1 = new double();
        private double dTheta1 = new double();
        private double dPixelX2 = new double();
        private double dPixelY2 = new double();
        private double dTheta2 = new double();
        private double dPixelX3 = new double();
        private double dPixelY3 = new double();
        private double dTheta3 = new double();

        private double calX = 1;
        private double calY = 1;
        private double calT = 0.5;
        private int calCnt = 0;
        //int EMICalCnt = 0;

        private Point2d Source1 = new Point2d();
        private Point2d Source2 = new Point2d();
        private Point2d Target1 = new Point2d();
        private Point2d Target2 = new Point2d();

        private Point2d FixSource1 = new Point2d();
        private Point2d FixSource2 = new Point2d();
        private Point2d FixTarget1 = new Point2d();
        private Point2d FixTarget2 = new Point2d();
        private XYT Offset = new XYT();

        private List<UVW> lstFixPosUVW = new List<UVW>();
        private List<XYT> lstFixPosXYT = new List<XYT>();
        private List<XYT> lstFixPosT = new List<XYT>();
        private List<Point2d> lstFixCoord1 = new List<Point2d>();
        private List<Point2d> lstFixCoord2 = new List<Point2d>();

        private int calKind;
        private bool uvrwCal;
        private int calMotionNo;
        private bool backlashuse;
        private bool calDataMove;
        private double[] calMove1 = new double[31];
        private double[] calMove2 = new double[31];
        private double[] calMove3 = new double[31];
        private double[] calMove4 = new double[31];
        public bool calDataReply;
        private bool teachPosSave;


        //MFQ
        private bool CallogStop = false;

        private int CalLogCnt = 1;

        //2019.07.20 EMI Align 추가
        private cs2DAlign.ptXY EMIY1 = new cs2DAlign.ptXY();

        private cs2DAlign.ptXY EMIY2 = new cs2DAlign.ptXY();
        private double tempResolution;
        private cs2DAlign.ptXY EMICenter1 = new cs2DAlign.ptXY();
        //private cs2DAlign.ptXY EMICenter2 = new cs2DAlign.ptXY();
        private cs2DAlign.ptXY EMILeft1 = new cs2DAlign.ptXY();
        private cs2DAlign.ptXY EMILeft2 = new cs2DAlign.ptXY();
        private cs2DAlign.ptXY EMIRight1 = new cs2DAlign.ptXY();

        private bool CenterPosMove = false;

        //bool CalStart = false;
        private rs2DAlign.cs2DAlign.ptXY Diff = new rs2DAlign.cs2DAlign.ptXY();         // Overlay의 중심값과 Cal 마크 중심값의 차이

        private rs2DAlign.cs2DAlign.ptXYT motorMove = new rs2DAlign.cs2DAlign.ptXYT();       // 실제 PLC가 이동해야하는 값

        private void tmrCal_Tick(object sender, EventArgs e)
        {

            switch (Vision[0].CalStep)
            {
                #region Calibration 표준화
                case 1000:
                    // 캘리브레이션 요청.
                    CONST.bPCCalReq[CalOK(CalVisionNO, calKind)] = true;
                    if (CalOKCheck())
                    {
                        // 포지선에 대한 리스트 클리어
                        calPosCam1.Clear();
                        calPosCam2.Clear();
                        calPosCam3.Clear();

                        YTCalPosCam1.Clear();
                        YTCalPosCam2.Clear();
                        YTCalPosCam3.Clear();

                        // Cnt 초기화
                        calCnt = 0;
                        //EMICalCnt = 0;

                        cs2DAlign.ptXY[] offsetXY = new cs2DAlign.ptXY[9];
                        cs2DAlign.ptXXYY[] uvw_offsetXY = new cs2DAlign.ptXXYY[9];

                        cs2DAlign.ptXY[] offsetXYBacklash = new cs2DAlign.ptXY[9];
                        cs2DAlign.ptXXYY[] uvw_offsetXYBacklash = new cs2DAlign.ptXXYY[9];

                        double FixMovePosOffset_Y = 0;
                        double FixMovePosOffset_X = 0;


                        double backlashOffset = 0.2;
                        // Cal 용 Robot Position 저장

                        // Dll 상에 Set 하는 기능이기 때문에 순서 바뀌면 안됨.
                        Menu.rsAlign.setRobotPos(calX + backlashOffset, calY + backlashOffset, ref offsetXYBacklash);
                        Menu.rsAlign.setRobotPos(calX, calY, ref offsetXY);
                        // UVW 포지션 저장 용 Offset 값 취득
                        if (uvrwCal)
                        {
                            Menu.rsAlign.getCal_UVRW_XY_Pos((int)calPos_L, calX, calY, ref uvw_offsetXY);
                            Menu.rsAlign.getCal_UVRW_XY_Pos((int)calPos_L, calX + backlashOffset, calY + backlashOffset, ref uvw_offsetXYBacklash);
                        }

                        if (teachPosSave)
                        {
                            Menu.frmAutoMain.IF.readPositionData(calMotionNo);
                            Menu.rsAlign.setCalTeachPos((int)calPos_L, Menu.frmAutoMain.visionPosition[calMotionNo].XPos, Menu.frmAutoMain.visionPosition[calMotionNo].YPos, Menu.frmAutoMain.visionPosition[calMotionNo].TPos);
                            Menu.rsAlign.setCalTeachPos((int)calPos_R, Menu.frmAutoMain.visionPosition[calMotionNo].XPos, Menu.frmAutoMain.visionPosition[calMotionNo].YPos, Menu.frmAutoMain.visionPosition[calMotionNo].TPos);
                        }

                        for (int i = 0; i < 9; i++)
                        {
                            //Cal 용 이동 위치 저장
                            if (YTCal)
                            {
                                calPosXYT[i] = new XYT();
                                calPosXYT[i].X = offsetXY[i].X;
                                calPosXYT[i].Y = offsetXY[i].Y + FixMovePosOffset_Y;
                                calPosXYT[i].T = 0;

                                calPosXYTBacklash[i] = new XYT();
                                calPosXYTBacklash[i].X = 0;
                                calPosXYTBacklash[i].Y = offsetXYBacklash[i].Y + FixMovePosOffset_Y;
                                calPosXYTBacklash[i].T = 0;
                            }
                            else
                            {
                                calPosXYT[i] = new XYT();
                                calPosXYT[i].X = offsetXY[i].X + FixMovePosOffset_X;
                                calPosXYT[i].Y = offsetXY[i].Y + FixMovePosOffset_Y;
                                calPosXYT[i].T = 0;

                                calPosXYTBacklash[i] = new XYT();
                                calPosXYTBacklash[i].X = offsetXYBacklash[i].X + FixMovePosOffset_X;
                                calPosXYTBacklash[i].Y = offsetXYBacklash[i].Y + FixMovePosOffset_Y;
                                calPosXYTBacklash[i].T = 0;
                            }

                            if (uvrwCal)
                            {
                                calPosUVW[i] = new UVW();
                                calPosUVW[i].X1 = startUVW.X1 + uvw_offsetXY[i].X1;
                                calPosUVW[i].X2 = startUVW.X2 + uvw_offsetXY[i].X2;
                                calPosUVW[i].Y1 = startUVW.Y1 + uvw_offsetXY[i].Y1;
                                calPosUVW[i].Y2 = startUVW.Y2 + uvw_offsetXY[i].Y2;

                                calPosUVWBacklash[i] = new UVW();
                                calPosUVWBacklash[i].X1 = startUVW.X1 + uvw_offsetXYBacklash[i].X1;
                                calPosUVWBacklash[i].X2 = startUVW.X2 + uvw_offsetXYBacklash[i].X2;
                                calPosUVWBacklash[i].Y1 = startUVW.Y1 + uvw_offsetXYBacklash[i].Y1;
                                calPosUVWBacklash[i].Y2 = startUVW.Y2 + uvw_offsetXYBacklash[i].Y2;
                            }
                        }

                        if (YTCal)
                        {
                            fixPosXYT[0] = new XYT();
                            fixPosXYT[0].X = 0;
                            fixPosXYT[0].Y = FixMovePosOffset_Y;
                            fixPosXYT[0].T = -calT;

                            fixPosXYT[1] = new XYT();
                            fixPosXYT[1].X = 0;
                            fixPosXYT[1].Y = FixMovePosOffset_Y;
                            fixPosXYT[1].T = calT;
                        }
                        else
                        {
                            fixPosXYT[0] = new XYT();
                            fixPosXYT[0].X = FixMovePosOffset_X;
                            fixPosXYT[0].Y = FixMovePosOffset_Y;
                            fixPosXYT[0].T = -calT;

                            fixPosXYT[1] = new XYT();
                            fixPosXYT[1].X = FixMovePosOffset_X;
                            fixPosXYT[1].Y = FixMovePosOffset_Y;
                            fixPosXYT[1].T = calT;
                        }

                        //if (TYRCal)
                        //{
                        //    fixPosTYR[0] = new XYT();
                        //    fixPosTYR[0].X = -1;
                        //    //fixPosTYR[0].Y = 0 + FixMovePosOffset_Y; //190618 cjm Dll변경
                        //    fixPosTYR[0].Y = 0;
                        //    fixPosTYR[0].T = -1;

                        //    fixPosTYR[1] = new XYT();
                        //    fixPosTYR[1].X = 1;
                        //    //fixPosTYR[1].Y = 0 + FixMovePosOffset_Y; //190618 cjm Dll변경
                        //    fixPosTYR[1].Y = 0;
                        //    fixPosTYR[1].T = 1;
                        //}

                        fixPosXYTBacklash[0] = new XYT();
                        //190618 cjm Dll변경
                        fixPosXYTBacklash[0].X = FixMovePosOffset_X;
                        fixPosXYTBacklash[0].Y = FixMovePosOffset_Y;
                        fixPosXYTBacklash[0].T = -calT - backlashOffset;

                        fixPosXYTBacklash[1] = new XYT();
                        //190618 cjm Dll변경
                        fixPosXYTBacklash[1].X = FixMovePosOffset_X;
                        fixPosXYTBacklash[1].Y = FixMovePosOffset_Y;
                        fixPosXYTBacklash[1].T = calT + backlashOffset;

                        if (uvrwCal)
                        {
                            Menu.rsAlign.getCal_UVRW_fix_Pos((int)calPos_L, -calT, calT, ref fixPosUVW);
                            Menu.rsAlign.getCal_UVRW_fix_Pos((int)calPos_L, -calT - backlashOffset, calT + backlashOffset, ref fixPosUVWBacklash);
                        }

                        Vision[0].CalStep++;
                    }
                    else if (CalNGCheck())
                    {
                        CalErrEnd();
                        return;
                    }
                    break;

                // 캘리브레이션을 하기 위한 1~9번위치 이동 시킨다. Backlash
                case 1001:
                    if (backlashuse)
                    {
                        if (!uvrwCal)
                        {
                            Menu.frmAutoMain.IF.writeAlignXYTOffset(calMotionNo, calPosXYTBacklash[calCnt].X, calPosXYTBacklash[calCnt].Y, calPosXYTBacklash[calCnt].T, eConvert.notUse, Vision[0].CFG.XYAxisRevers);
                        }
                        else
                        {
                            Console.WriteLine("writeAlignUVWOffset > 6");
                            Menu.frmAutoMain.IF.writeAlignUVWOffset(calMotionNo, calPosUVWBacklash[calCnt].X1, calPosUVWBacklash[calCnt].Y1, calPosUVWBacklash[calCnt].X2, calPosUVWBacklash[calCnt].Y2);
                        }

                        if (PosMoveReplyClear(CalVisionNO, calKind))
                        {
                            CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = true;
                            Vision[0].CalStep++;
                        }
                    }
                    else Vision[0].CalStep++;
                    break;

                case 1002:
                    if (backlashuse)
                    {
                        if (PosMoveComplete(CalVisionNO, calKind))
                        {
                            CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;
                            Vision[0].CalStep++;
                        }
                    }
                    else Vision[0].CalStep++;
                    break;

                // 캘리브레이션을 하기 위한 1~9번위치 이동 시킨다.
                case 1003:
                    if (!uvrwCal)
                    {

                        Menu.frmAutoMain.IF.writeAlignXYTOffset(calMotionNo, calPosXYT[calCnt].X, calPosXYT[calCnt].Y, calPosXYT[calCnt].T, TYRCal, Vision[0].CFG.XYAxisRevers);
                    }
                    else
                    {
                        Console.WriteLine("writeAlignUVWOffset > 7");
                        Menu.frmAutoMain.IF.writeAlignUVWOffset(calMotionNo, calPosUVW[calCnt].X1, calPosUVW[calCnt].Y1, calPosUVW[calCnt].X2, calPosUVW[calCnt].Y2);
                    }

                    // Move Reply 클리어.
                    if (PosMoveReplyClear(CalVisionNO, calKind))
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = true;
                        // 다음 스텝으로
                        Vision[0].CalStep++;
                    }

                    break;
                // 정지 상태 확인
                case 1004:   // 정지 신호 확인
                    if (PosMoveComplete(CalVisionNO, calKind))
                    {
                        // Cal Move Request 클리어
                        //if (CalType == eCalType.Cam2Type)
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;
                        // 다음 스텝으로
                        Vision[0].CalStep++;
                    }
                    break;

                case 1005:   //  Pattern Search
                    string calLog = "";
                    // 촬상
                    System.Threading.Thread.Sleep(500);
                    //2018.11.08 khs CalType 구분하여 찰상
                    if (CalType == eCalType.Cam1Type
                        || CalType == eCalType.Cam2Type
                        || CalType == eCalType.Cam3Type
                        || CalType == eCalType.Cam1Cal2)
                    {
                        Vision[0].Capture(true, true, false, true, "CAL" + ((int)calPos_L + 1) + "Data_" + calCnt);


                        if (Vision[0].PatternSearchEnum(ref dPixelX1, ref dPixelY1, ref dTheta1, ePatternKind.Cal))
                        {
                            rs2DAlign.cs2DAlign.ptXY pt1 = new rs2DAlign.cs2DAlign.ptXY();
                            pt1.X = dPixelX1;
                            pt1.Y = dPixelY1;

                            // 리스트에 저장.
                            calPosCam1.Add(pt1);

                            calMove1[2 * calCnt + 3] = dPixelX1;
                            calMove1[2 * calCnt + 4] = dPixelY1;
                        }
                        else
                        {
                            CalErrEnd();
                            return;
                        }
                        if (CalType == eCalType.Cam1Cal2)
                        {
                            if (Vision[0].PatternSearchEnum(ref dPixelX2, ref dPixelY2, ref dTheta1, ePatternKind.Cal2))
                            {
                                rs2DAlign.cs2DAlign.ptXY pt1 = new rs2DAlign.cs2DAlign.ptXY();
                                pt1.X = dPixelX2;
                                pt1.Y = dPixelY2;

                                // 리스트에 저장.
                                calPosCam2.Add(pt1);

                                calMove2[2 * calCnt + 3] = dPixelX2;
                                calMove2[2 * calCnt + 4] = dPixelY2;
                            }
                            else
                            {
                                CalErrEnd();
                                return;
                            }
                        }
                    }
                    if (CalType == eCalType.Cam2Type
                        || CalType == eCalType.Cam3Type)
                    {
                        Vision[1].Capture(true, true, false, true, "CAL" + ((int)calPos_R + 1) + "Data_" + calCnt);

                        // 카메라 1,2 패턴 찾기.
                        if (Vision[1].PatternSearchEnum(ref dPixelX2, ref dPixelY2, ref dTheta2, ePatternKind.Cal))
                        {
                            rs2DAlign.cs2DAlign.ptXY pt2 = new rs2DAlign.cs2DAlign.ptXY();
                            pt2.X = dPixelX2;
                            pt2.Y = dPixelY2;

                            // 리스트에 저장.
                            calPosCam2.Add(pt2);

                            calMove2[2 * calCnt + 3] = dPixelX2;
                            calMove2[2 * calCnt + 4] = dPixelY2;
                        }
                        else
                        {
                            CalErrEnd();
                            return;
                        }
                    }
                    if (CalType == eCalType.Cam3Type)
                    {

                        Vision[2].Capture(true, true, false, true, "CAL" + ((int)calPos_3 + 1) + "Data_" + calCnt);

                        // 카메라 1,2 패턴 찾기.
                        if (Vision[2].PatternSearchEnum(ref dPixelX3, ref dPixelY3, ref dTheta3, ePatternKind.Cal))
                        {
                            rs2DAlign.cs2DAlign.ptXY pt3 = new rs2DAlign.cs2DAlign.ptXY();
                            pt3.X = dPixelX3;
                            pt3.Y = dPixelY3;

                            calPosCam3.Add(pt3);

                            calMove3[2 * calCnt + 3] = dPixelX3;
                            calMove3[2 * calCnt + 4] = dPixelY3;
                        }
                        else
                        {
                            CalErrEnd();
                            return;
                        }
                    }

                    //MFQ
                    if (cbCalLog.Checked)
                    {
                        calLog = cbCamList.SelectedItem.ToString() + "," + calCnt.ToString("0") + "," +
                            dPixelX1.ToString("0.000") + "," + dPixelY1.ToString("0.000") + "," +
                            dPixelX2.ToString("0.000") + "," + dPixelY2.ToString("0.000") + "," +
                            dPixelX3.ToString("0.000") + "," + dPixelY3.ToString("0.000") + "," +
                            calPosXYT[calCnt].X.ToString() + "," + calPosXYT[calCnt].Y.ToString() + "," + calPosXYT[calCnt].T.ToString();

                        cLog.Save(LogKind.Calibration, calLog);
                    }
                    // 캘카운트가 9회 미만이면
                    if (calCnt < 8)
                    {
                        calCnt++;
                        Vision[0].CalStep = 1001;  // Calibration 계속 진행
                    }
                    else
                    {
                        if (YTCal)
                        {
                            if (CalType == eCalType.Cam1Type
                                || CalType == eCalType.Cam2Type
                                || CalType == eCalType.Cam3Type
                                || CalType == eCalType.Cam1Cal2)
                            {
                                YTCalPosCam1 = SetYTCalPos(calPosCam1);
                                if (CalType == eCalType.Cam1Cal2)
                                {
                                    YTCalPosCam2 = SetYTCalPos(calPosCam2);
                                }
                            }
                            if (CalType == eCalType.Cam2Type
                                || CalType == eCalType.Cam3Type)
                            {
                                YTCalPosCam2 = SetYTCalPos(calPosCam2);
                            }
                            if (CalType == eCalType.Cam3Type)
                            {
                                YTCalPosCam3 = SetYTCalPos(calPosCam3);
                            }
                        }
                        cs2DAlign.ptXY pixelCnt = new cs2DAlign.ptXY();

                        if (CalType == eCalType.Cam1Type
                                || CalType == eCalType.Cam2Type
                                || CalType == eCalType.Cam3Type
                                || CalType == eCalType.Cam1Cal2)
                        {
                            pixelCnt.X = Vision[0].ImgX;
                            pixelCnt.Y = Vision[0].ImgY;
                            calMove1[21] = pixelCnt.X;
                            calMove1[22] = pixelCnt.Y;
                            if (YTCal)
                            {
                                Menu.rsAlign.setCamCalibration((int)calPos_L, YTCalPosCam1, pixelCnt);
                                Copy9PointCalpos(calPos_L, YTCalPosCam1, pixelCnt); //pcy210118
                            }
                            else
                            {
                                Menu.rsAlign.setCamCalibration((int)calPos_L, calPosCam1, pixelCnt);
                                Copy9PointCalpos(calPos_L, calPosCam1, pixelCnt);
                            }
                            if (CalType == eCalType.Cam1Cal2)
                            {
                                calMove2[21] = pixelCnt.X;
                                calMove2[22] = pixelCnt.Y;
                                if (YTCal)
                                {
                                    Menu.rsAlign.setCamCalibration((int)calPos_R, YTCalPosCam2, pixelCnt);
                                    Copy9PointCalpos(calPos_R, YTCalPosCam2, pixelCnt);
                                }
                                else
                                {
                                    Menu.rsAlign.setCamCalibration((int)calPos_R, calPosCam2, pixelCnt);
                                    Copy9PointCalpos(calPos_R, calPosCam2, pixelCnt);
                                }
                            }
                        }
                        if (CalType == eCalType.Cam2Type
                            || CalType == eCalType.Cam3Type)
                        {
                            pixelCnt.X = Vision[1].ImgX;
                            pixelCnt.Y = Vision[1].ImgY;
                            calMove2[21] = pixelCnt.X;
                            calMove2[22] = pixelCnt.Y;
                            if (YTCal)
                            {
                                Menu.rsAlign.setCamCalibration((int)calPos_R, YTCalPosCam2, pixelCnt);
                                Copy9PointCalpos(calPos_R, YTCalPosCam2, pixelCnt);
                            }
                            else
                            {
                                Menu.rsAlign.setCamCalibration((int)calPos_R, calPosCam2, pixelCnt);
                                Copy9PointCalpos(calPos_R, calPosCam2, pixelCnt);
                            }
                        }
                        if (CalType == eCalType.Cam3Type)
                        {
                            pixelCnt.X = Vision[2].ImgX;
                            pixelCnt.Y = Vision[2].ImgY;
                            calMove3[21] = pixelCnt.X;
                            calMove3[22] = pixelCnt.Y;
                            if (YTCal)
                            {
                                Menu.rsAlign.setCamCalibration((int)calPos_3, YTCalPosCam3, pixelCnt);
                                Copy9PointCalpos(calPos_3, YTCalPosCam3, pixelCnt);
                            }
                            else
                            {
                                Menu.rsAlign.setCamCalibration((int)calPos_3, calPosCam3, pixelCnt);
                                Copy9PointCalpos(calPos_3, calPosCam3, pixelCnt);
                            }
                        }

                        calCnt = 0;
                        if (Vision[0].CFG.eCamName == nameof(eCamNO.Laser1) || Vision[0].CFG.eCamName == nameof(eCamNO.Laser2)) Vision[0].CalStep = 1011; //종료
                        else Vision[0].CalStep = 1006;
                    }
                    break;

                case 1006:
                    if (backlashuse)
                    {
                        if (!uvrwCal)
                        {
                            Menu.frmAutoMain.IF.writeAlignXYTOffset(calMotionNo, fixPosXYTBacklash[calCnt].X, fixPosXYTBacklash[calCnt].Y, fixPosXYTBacklash[calCnt].T, eConvert.notUse, Vision[0].CFG.XYAxisRevers);
                        }
                        else
                        {
                            Console.WriteLine("writeAlignUVWOffset > 8");
                            Menu.frmAutoMain.IF.writeAlignUVWOffset(calMotionNo, fixPosUVWBacklash[calCnt].X1, fixPosUVWBacklash[calCnt].Y1, fixPosUVWBacklash[calCnt].X2, fixPosUVWBacklash[calCnt].Y2);
                        }

                        // Move Reply 클리어.
                        if (PosMoveReplyClear(CalVisionNO, calKind))
                        {
                            CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = true;
                            Vision[0].CalStep++;
                        }
                    }
                    else Vision[0].CalStep++;
                    break;

                case 1007:
                    if (backlashuse)
                    {
                        if (PosMoveComplete(CalVisionNO, calKind))
                        {
                            CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;
                            Vision[0].CalStep++;
                        }
                    }
                    else Vision[0].CalStep++;
                    break;

                case 1008:
                    if (!uvrwCal)
                    {
                        Menu.frmAutoMain.IF.writeAlignXYTOffset(calMotionNo, fixPosXYT[calCnt].X, fixPosXYT[calCnt].Y, fixPosXYT[calCnt].T, TYRCal, Vision[0].CFG.XYAxisRevers);
                    }
                    else
                    {
                        Console.WriteLine("writeAlignUVWOffset > 9");
                        Menu.frmAutoMain.IF.writeAlignUVWOffset(calMotionNo, fixPosUVW[calCnt].X1, fixPosUVW[calCnt].Y1, fixPosUVW[calCnt].X2, fixPosUVW[calCnt].Y2);
                    }

                    // Move Reply 클리어.
                    if (PosMoveReplyClear(CalVisionNO, calKind))
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = true;
                        // 다음 스텝으로
                        Vision[0].CalStep++;
                    }
                    else Vision[0].CalStep++;
                    break;

                case 1009:   // 정지 신호 확인
                    if (PosMoveComplete(CalVisionNO, calKind))
                    {
                        //if (CalType == eCalType.Cam2Type)
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;
                        Vision[0].CalStep++;
                    }
                    break;

                case 1010:   //  FixPosition Pattern Search
                    System.Threading.Thread.Sleep(500);
                    //2018.11.08 khs CalType 구분하여 찰상
                    if (CalType == eCalType.Cam1Type
                        || CalType == eCalType.Cam2Type
                        || CalType == eCalType.Cam3Type
                        || CalType == eCalType.Cam1Cal2)
                    {
                        Vision[0].Capture(false, true, false, true);

                        if (Vision[0].PatternSearchEnum(ref dPixelX1, ref dPixelY1, ref dTheta1, ePatternKind.Cal))
                        {
                            SetCenterOfRotation(calCnt, ref fixPosCam1, dPixelX1, dPixelY1, calPos_L, bLoadPreCal);
                        }
                        else
                        {
                            CalErrEnd();
                            return;
                        }
                        if (CalType == eCalType.Cam1Cal2)
                        {
                            if (Vision[0].PatternSearchEnum(ref dPixelX2, ref dPixelY2, ref dTheta1, ePatternKind.Cal2))
                            {
                                SetCenterOfRotation(calCnt, ref fixPosCam2, dPixelX2, dPixelY2, calPos_R, bLoadPreCal);
                            }
                            else
                            {
                                CalErrEnd();
                                return;
                            }
                        }
                    }
                    if (CalType == eCalType.Cam2Type
                        || CalType == eCalType.Cam3Type)
                    {
                        Vision[1].Capture(false, true, false, true);
                        if (Vision[1].PatternSearchEnum(ref dPixelX2, ref dPixelY2, ref dTheta2, ePatternKind.Cal))
                        {
                            SetCenterOfRotation(calCnt, ref fixPosCam2, dPixelX2, dPixelY2, calPos_R, bLoadPreCal);
                        }
                        else
                        {
                            CalErrEnd();
                            return;
                        }
                    }
                    if (CalType == eCalType.Cam3Type)
                    {
                        Vision[2].Capture(false, true, false, true);
                        if (Vision[2].PatternSearchEnum(ref dPixelX3, ref dPixelY3, ref dTheta3, ePatternKind.Cal))
                        {
                            SetCenterOfRotation(calCnt, ref fixPosCam3, dPixelX3, dPixelY3, calPos_3, bLoadPreCal);
                        }
                        else
                        {
                            CalErrEnd();
                            return;
                        }
                    }
                    if (calCnt == 0) Vision[0].CalStep = 1006;
                    else Vision[0].CalStep++;
                    calCnt++;
                    break;

                case 1011:
                    if (!uvrwCal)
                    {
                        Menu.frmAutoMain.IF.writeAlignXYTOffset(calMotionNo, startXYT.X, startXYT.Y, startXYT.T, TYRCal, Vision[0].CFG.XYAxisRevers);
                    }
                    else
                    {
                        Console.WriteLine("writeAlignUVWOffset > 10");
                        Menu.frmAutoMain.IF.writeAlignUVWOffset(calMotionNo, startUVW.X1, startUVW.Y1, startUVW.X2, startUVW.Y2);
                    }

                    if (PosMoveReplyClear(CalVisionNO, calKind))
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = true;
                        Vision[0].CalStep++;
                    }
                    break;

                case 1012:   // 정지 신호 확인

                    if (PosMoveComplete(CalVisionNO, calKind))
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;
                        CONST.bPCCalReq[CalOK(CalVisionNO, calKind)] = false;
                        // Cal Data Save추가
                        if (CalType == eCalType.Cam1Type
                            || CalType == eCalType.Cam2Type
                            || CalType == eCalType.Cam3Type
                            || CalType == eCalType.Cam1Cal2)
                        {
                            Menu.rsAlign.getResolution((int)calPos_L, ref Resolution1, ref PixelCnt1);
                            Menu.frmAutoMain.Vision[CalVisionNO].CFG.FOVX = Convert.ToDouble((PixelCnt1.X * Resolution1.X).ToString("0.0000"));
                            Menu.frmAutoMain.Vision[CalVisionNO].CFG.FOVY = Convert.ToDouble((PixelCnt1.Y * Resolution1.Y).ToString("0.0000"));
                            Menu.Config.CAMconfig_Write(Menu.frmAutoMain.Vision[CalVisionNO].CFG, CalVisionNO);
                        }
                        if (CalType == eCalType.Cam2Type
                            || CalType == eCalType.Cam3Type)
                        {
                            Menu.rsAlign.getResolution((int)calPos_R, ref Resolution2, ref PixelCnt2);
                            Menu.frmAutoMain.Vision[CalVisionNO + 1].CFG.FOVX = Convert.ToDouble((PixelCnt2.X * Resolution2.X).ToString("0.000"));
                            Menu.frmAutoMain.Vision[CalVisionNO + 1].CFG.FOVY = Convert.ToDouble((PixelCnt2.Y * Resolution2.Y).ToString("0.000"))
                            Menu.Config.CAMconfig_Write(Menu.frmAutoMain.Vision[CalVisionNO + 1].CFG, CalVisionNO + 1);
                        }
                        if (CalType == eCalType.Cam3Type)
                        {
                            Menu.rsAlign.getResolution((int)calPos_3, ref Resolution3, ref PixelCnt3);
                            Menu.frmAutoMain.Vision[CalVisionNO + 2].CFG.FOVX = Convert.ToDouble((PixelCnt3.X * Resolution3.X).ToString("0.000"));
                            Menu.frmAutoMain.Vision[CalVisionNO + 2].CFG.FOVY = Convert.ToDouble((PixelCnt3.Y * Resolution3.Y).ToString("0.000"));
                            //Menu.frmAutoMain.Vision[CalVisionNO + 2].CFG.Resolution = Convert.ToDouble((Math.Abs((Resolution3.X + Resolution3.Y) / 2)).ToString("0.000000"));
                            Menu.Config.CAMconfig_Write(Menu.frmAutoMain.Vision[CalVisionNO + 2].CFG, CalVisionNO + 2);
                        }

                        #region 결과 점찍기..
                        CogPointMarker p = new CogPointMarker();
                        if (CalType == eCalType.Cam1Type
                            || CalType == eCalType.Cam2Type
                            || CalType == eCalType.Cam3Type
                            || CalType == eCalType.Cam1Cal2)
                        {
                            if (YTCal)
                            {
                                foreach (var s in YTCalPosCam1)
                                {
                                    p.X = s.X;
                                    p.Y = s.Y;
                                    Vision[0].PaintPoint(p);
                                }
                            }
                            else
                            {
                                foreach (var s in calPosCam1)
                                {
                                    p.X = s.X;
                                    p.Y = s.Y;
                                    Vision[0].PaintPoint(p);
                                }
                            }

                            p.X = fixPosCam1[0].X;
                            p.Y = fixPosCam1[0].Y;
                            Vision[0].PaintPoint(p);
                            p.X = fixPosCam1[1].X;
                            p.Y = fixPosCam1[1].Y;
                            Vision[0].PaintPoint(p);
                            // 20200911 cjm Calibration 움직이는 점 저장하는 곳
                            // 20201003 cjm Cal 점 찍는 곳 변경
                            // 20201004 cjm Cal 점 찍는 곳 주소 변경 calPos_L하나로
                            CalibrationPointSave(YTCalPosCam1, calPosCam1, fixPosCam1, calPos_L, calX, calY, calT);
                            if (CalType == eCalType.Cam1Cal2)
                            {
                                CogPointMarker p2 = new CogPointMarker();
                                p2.Color = CogColorConstants.Red;
                                if (YTCal)
                                {
                                    foreach (var s in YTCalPosCam2)
                                    {
                                        p2.X = s.X;
                                        p2.Y = s.Y;
                                        Vision[0].PaintPoint(p2);
                                    }
                                }
                                else
                                {
                                    foreach (var s in calPosCam2)
                                    {
                                        p2.X = s.X;
                                        p2.Y = s.Y;
                                        Vision[0].PaintPoint(p2);
                                    }
                                }

                                p2.X = fixPosCam2[0].X;
                                p2.Y = fixPosCam2[0].Y;
                                Vision[0].PaintPoint(p2);
                                p2.X = fixPosCam2[1].X;
                                p2.Y = fixPosCam2[1].Y;
                                Vision[0].PaintPoint(p2);
                                // 20200911 cjm Calibration 움직이는 점 저장하는 곳
                                // 20201003 cjm Cal 점 찍는 곳 변경
                                // 20201004 cjm Cal 점 찍는 곳 주소 변경 calPos_L하나로
                                CalibrationPointSave(YTCalPosCam2, calPosCam2, fixPosCam2, calPos_L, calX, calY, calT);
                            }
                        }

                        if (CalType == eCalType.Cam2Type
                            || CalType == eCalType.Cam3Type)
                        {
                            if (YTCal)
                            {
                                foreach (var s in YTCalPosCam2)
                                {
                                    p.X = s.X;
                                    p.Y = s.Y;
                                    Vision[1].PaintPoint(p);
                                }
                            }
                            else
                            {
                                foreach (var s in calPosCam2)
                                {
                                    p.X = s.X;
                                    p.Y = s.Y;
                                    Vision[1].PaintPoint(p);
                                }
                            }
                            p.X = fixPosCam2[0].X;
                            p.Y = fixPosCam2[0].Y;
                            Vision[1].PaintPoint(p);
                            p.X = fixPosCam2[1].X;
                            p.Y = fixPosCam2[1].Y;
                            Vision[1].PaintPoint(p);
                            // 20200911 cjm Calibration 움직이는 점 저장하는 곳
                            // 20201003 cjm Cal 점 찍는 곳 변경
                            // 20201004 cjm Cal 점 찍는 곳 주소 변경 calPos_L하나로
                            CalibrationPointSave(YTCalPosCam2, calPosCam2, fixPosCam2, calPos_L, calX, calY, calT);
                        }
                        if (CalType == eCalType.Cam3Type)
                        {
                            if (YTCal)
                            {
                                foreach (var s in YTCalPosCam3)
                                {
                                    p.X = s.X;
                                    p.Y = s.Y;
                                    Vision[2].PaintPoint(p);
                                }
                            }
                            else
                            {
                                foreach (var s in calPosCam3)
                                {
                                    p.X = s.X;
                                    p.Y = s.Y;
                                    Vision[2].PaintPoint(p);
                                }
                            }
                            p.X = fixPosCam3[0].X;
                            p.Y = fixPosCam3[0].Y;
                            Vision[2].PaintPoint(p);
                            p.X = fixPosCam3[1].X;
                            p.Y = fixPosCam3[1].Y;
                            Vision[2].PaintPoint(p);

                            // 20200923 cjm Calibration 움직이는 점 저장하는 곳 추가
                            // 20201003 cjm Cal 점 찍는 곳 변경
                            // 20201004 cjm Cal 점 찍는 곳 주소 변경 calPos_L하나로
                            CalibrationPointSave(YTCalPosCam3, calPosCam3, fixPosCam3, calPos_L, calX, calY, calT);
                        }
                        #endregion 결과 점찍기..

                        //MFQ
                        if (cbCalLog.Checked && !CallogStop)
                        {
                            int SetCalLogCnt = Convert.ToInt32(txtCalLogCnt.Text);

                            if (CalLogCnt == SetCalLogCnt) CallogStop = true;
                            else
                            {
                                CalLogCnt++;
                                Vision[0].CalStep = 1000;
                            }
                        }
                        dgvClear();
                        CalResutList(calPos_L, CalType); //20201003 cjm C:Drive 안 Calibration Data

                        if (!cbCalLog.Checked)
                        {
                            Vision[0].CalStep = 1200; //원점이동 후 target offset등록.
                        }
                    }
                    break;
                case 1200:
                    //1011에서 이미 원점으로 돌아가 있음.
                    if (cbCalSetTarget.Checked) //cal을 실제 자재로 하면 가능..
                    {
                        //1cam 2point 또는 2cam 2point일때 && recipe상 1번 Lcheck만 대응
                        System.Threading.Thread.Sleep(500);
                        bool b1 = false;
                        bool b2 = false;
                        if (CalType == eCalType.Cam2Type
                            || CalType == eCalType.Cam1Cal2)
                        {
                            Vision[0].Capture(false, true, false, true);

                            b1 = Vision[0].PatternSearchEnum(ref dPixelX1, ref dPixelY1, ref dTheta1, ePatternKind.Cal);
                            if (CalType == eCalType.Cam1Cal2)
                            {
                                b2 = Vision[0].PatternSearchEnum(ref dPixelX2, ref dPixelY2, ref dTheta1, ePatternKind.Cal2);
                            }
                        }
                        if (CalType == eCalType.Cam2Type)
                        {
                            Vision[1].Capture(false, true, false, true);
                            b2 = Vision[1].PatternSearchEnum(ref dPixelX2, ref dPixelY2, ref dTheta2, ePatternKind.Cal);

                        }
                        if (b1 && b2)
                        {
                            Menu.frmSetting.revData.mLcheck[Vision[0].CFG.Camno].LCheckOffset1 = CalcLcheckOffset(Vision[0], calPos_L, calPos_R, dPixelX1, dPixelY1, dPixelX2, dPixelY2);
                            Menu.frmSetting.revData.WriteData(CONST.RunRecipe.RecipeName);
                        }
                    }
                    Vision[0].CalStep = 1300;
                    break;
                case 1300:
                    if (calDataMove)
                    {
                        calDataReply = false;
                        Menu.frmAutoMain.IF.writeCalData(calMove1);
                        Menu.frmAutoMain.IF.SendData("CALDATAMOVE");
                        Vision[0].CalStep++;
                    }
                    else Vision[0].CalStep = 1500;
                    break;

                case 1301:
                    if (calDataReply)
                    {
                        calDataReply = false;
                        Thread.Sleep(500);
                        Vision[0].CalStep++;
                    }
                    break;

                case 1302:
                    //pcy190725 caltype이 1이면 2번데이터 안보냄.
                    if (CalType == eCalType.Cam2Type)
                    {
                        calDataReply = false;
                        Menu.frmAutoMain.IF.writeCalData(calMove2);
                        Menu.frmAutoMain.IF.SendData("CALDATAMOVE");
                        Vision[0].CalStep++;
                    }
                    else if (CalType == eCalType.Cam1Type)
                    {
                        calDataReply = true;
                        Vision[0].CalStep++;
                    }
                    break;

                case 1303:
                    if (calDataReply)
                    {
                        calDataReply = false;
                        Vision[0].CalStep = 1500;
                    }
                    break;
                case 1500:
                    Vision[0].CalStep = 1600;
                    break;
                case 1600: //완료
                    // 20200908 cjm Calibration Result 나타내는 btLoadData 클릭 추가
                    // 20200919 cjm Calibration Result 나타내는 방법 변경
                    dgvClear();
                    CalResutList(calPos_L, Vision[0].CFG.CalType); //20201003 cjm C:Drive 안 Calibration Data
                    Vision[0].CalStep = 0;
                    tmrCal.Enabled = false;
                    frmMsg.Visible = false;
                    CallogStop = false; //MFQ
                    CalLogCnt = 0;  //MFQ
                    MessageBox.Show("Calibration Complete.");
                    break;

                //2019.07.20 EMI Align 추가
                case 2000:
                    double Pos1 = calY + (-1) * Menu.frmSetting.revData.mOffset[CalVisionNO].CalFixPosOffset.Y;
                    Menu.frmAutoMain.IF.writeAlignXYTOffset(calMotionNo, 0, Pos1, 0, eConvert.notUse, Vision[0].CFG.XYAxisRevers);
                    // Move Reply 클리어.
                    if (PosMoveReplyClear(CalVisionNO, calKind))
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = true;
                        // 다음 스텝으로
                        Vision[0].CalStep++;
                    }

                    break;
                // 정지 상태 확인
                case 2001:   // 정지 신호 확인
                    if (PosMoveComplete(CalVisionNO, calKind))
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;
                        Vision[0].CalStep++;
                    }
                    break;

                case 2002:   //  Pattern Search

                    // 촬상
                    Thread.Sleep(500);
                    Vision[0].Capture(false, true, false, true);
                    if (Vision[0].PatternSearchEnum(ref dPixelX1, ref dPixelY1, ref dTheta1, ePatternKind.Cal))
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;
                        EMIY1.X = dPixelX1;
                        EMIY1.Y = dPixelY1;
                        Vision[0].CalStep++;
                    }
                    else
                    {
                        CalErrEnd();
                        return;
                    }
                    break;

                case 2003:
                    double Pos2 = (-1) * calY + (-1) * Menu.frmSetting.revData.mOffset[CalVisionNO].CalFixPosOffset.Y;
                    Menu.frmAutoMain.IF.writeAlignXYTOffset(calMotionNo, 0, Pos2, 0, eConvert.notUse, Vision[0].CFG.XYAxisRevers);
                    // Move Reply 클리어.
                    if (PosMoveReplyClear(CalVisionNO, calKind))
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = true;
                        // 다음 스텝으로
                        Vision[0].CalStep++;
                    }

                    break;
                // 정지 상태 확인
                case 2004:   // 정지 신호 확인
                    if (PosMoveComplete(CalVisionNO, calKind))
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;
                        Vision[0].CalStep++;
                    }
                    break;

                case 2005:   //  Pattern Search

                    // 촬상
                    Thread.Sleep(500);
                    Vision[0].Capture(false, true, false, true);
                    if (Vision[0].PatternSearchEnum(ref dPixelX1, ref dPixelY1, ref dTheta1, ePatternKind.Cal))
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;
                        EMIY2.X = dPixelX1;
                        EMIY2.Y = dPixelY1;
                        tempResolution = (calY * 2) / (EMIY2.Y - EMIY1.Y);   //Y 차이값을 이용하여 임시 Resolution 구함. //카메라 틀어짐 보정 안하는듯
                        Vision[0].CalStep++;
                    }
                    else
                    {
                        CalErrEnd();
                        return;
                    }
                    break;

                case 2006:

                    Pos2 = (-1) * Menu.frmSetting.revData.mOffset[CalVisionNO].CalFixPosOffset.Y;
                    Menu.frmAutoMain.IF.writeAlignXYTOffset(calMotionNo, 0, Pos2, 0, eConvert.notUse, Vision[0].CFG.XYAxisRevers);  // Cal 위치 이동
                    // Move Reply 클리어.
                    if (PosMoveReplyClear(CalVisionNO, calKind))
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = true;
                        // 다음 스텝으로
                        Vision[0].CalStep++;
                    }

                    break;
                // 정지 상태 확인
                case 2007:   // 정지 신호 확인
                    if (PosMoveComplete(CalVisionNO, calKind))
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;
                        Vision[0].CalStep++;
                    }
                    break;

                case 2008:   //  Pattern Search

                    // 촬상
                    Thread.Sleep(500);
                    Vision[0].Capture(false, true, false, true);

                    if (Vision[0].PatternSearchEnum(ref dPixelX1, ref dPixelY1, ref dTheta1, ePatternKind.Cal))
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;
                        EMICenter1.X = dPixelX1;
                        EMICenter1.Y = dPixelY1;
                        Vision[0].CalStep++;
                        // Cal 위치의 Pixel 값 구함.
                    }
                    else
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;
                        CONST.bPCCalReq[CalOK(CalVisionNO, calKind)] = false;
                        Vision[0].CalStep = 0;
                        tmrCal.Enabled = false;
                        frmMsg.Visible = false;
                        MessageBox.Show("ERROR");
                    }
                    break;

                case 2009:
                    Pos2 = (-1) * Menu.frmSetting.revData.mOffset[CalVisionNO].CalFixPosOffset.Y;
                    Menu.frmAutoMain.IF.writeAlignXYTOffset(calMotionNo, calT, Pos2, calT, eConvert.notUse, Vision[0].CFG.XYAxisRevers);  // -1도 방향 이동 (임시)
                    // Move Reply 클리어.
                    if (PosMoveReplyClear(CalVisionNO, calKind))
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = true;
                        // 다음 스텝으로
                        Vision[0].CalStep++;
                    }

                    break;
                // 정지 상태 확인
                case 2010:   // 정지 신호 확인
                    if (PosMoveComplete(CalVisionNO, calKind))
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;
                        Vision[0].CalStep++;
                    }
                    break;

                case 2011:   //  Pattern Search

                    // 촬상
                    Thread.Sleep(500);
                    if (CalType == eCalType.Cam1Cal2
                        || CalType == eCalType.Cam1Type)
                    {
                        Vision[0].Capture(false, true, false, true);

                        if (Vision[0].PatternSearchEnum(ref dPixelX1, ref dPixelY1, ref dTheta1, ePatternKind.Cal))
                        {
                            CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;
                            EMILeft1.X = dPixelX1;
                            EMILeft1.Y = dPixelY1;
                        }
                        else
                        {
                            CalErrEnd();
                            return;
                        }
                    }
                    if (CalType == eCalType.Cam1Cal2)
                    {
                        Vision[1].Capture(false, true, false, true);

                        if (Vision[1].PatternSearchEnum(ref dPixelX2, ref dPixelY2, ref dTheta2, ePatternKind.Cal))
                        {
                            CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;
                            EMILeft2.X = dPixelX2;
                            EMILeft2.Y = dPixelY2;
                        }
                        else
                        {
                            CalErrEnd();
                        }
                    }
                    Vision[0].CalStep++;
                    break;

                case 2012:
                    Pos2 = (-1) * Menu.frmSetting.revData.mOffset[CalVisionNO].CalFixPosOffset.Y;
                    Menu.frmAutoMain.IF.writeAlignXYTOffset(calMotionNo, -calT, Pos2, -calT, eConvert.notUse, Vision[0].CFG.XYAxisRevers);  // 1도 방향 이동 (임시)
                    // Move Reply 클리어.
                    if (PosMoveReplyClear(CalVisionNO, calKind))
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = true;
                        // 다음 스텝으로
                        Vision[0].CalStep++;
                    }

                    break;
                // 정지 상태 확인
                case 2013:   // 정지 신호 확인
                    if (PosMoveComplete(CalVisionNO, calKind))
                    {
                        Vision[0].CalStep++;
                    }
                    break;

                case 2014:   //  Pattern Search
                    // 촬상
                    Thread.Sleep(500);
                    Vision[0].Capture(false, true, false, true);

                    if (Vision[0].PatternSearchEnum(ref dPixelX1, ref dPixelY1, ref dTheta1, ePatternKind.Cal))
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;
                        EMIRight1.X = dPixelX1;
                        EMIRight1.Y = dPixelY1;
                        Vision[0].CalStep++;

                        // X를 Plus 로 보내면 T 값이 + 가 나와야 함.  그렇게 했으니까.
                        Menu.frmSetting.revData.mSizeSpecRatio.plusTMoveRatio = (EMICenter1.X - EMIRight1.X) * (-1) * tempResolution;
                        Menu.frmSetting.revData.mSizeSpecRatio.plusYMoveRatio = (EMICenter1.Y - EMIRight1.Y) * (-1) * tempResolution;

                        Menu.frmSetting.revData.mSizeSpecRatio.minusTMoveRatio = (EMICenter1.X - EMILeft1.X) * tempResolution;
                        Menu.frmSetting.revData.mSizeSpecRatio.minusYMoveRatio = (EMICenter1.Y - EMILeft1.Y) * tempResolution;

                        Menu.frmSetting.revData.WriteData(CONST.RunRecipe.RecipeName);
                        Vision[0].CalStep = 1001;
                    }
                    else
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;
                        CONST.bPCCalReq[CalOK(CalVisionNO, calKind)] = false;
                        Vision[0].CalStep = 0;
                        tmrCal.Enabled = false;
                        frmMsg.Visible = false;
                        MessageBox.Show("ERROR");
                    }
                    break;

                case 3000:
                    if (!CenterPosMove)
                    {
                        rs2DAlign.cs2DAlign.ptXY Overlay = new rs2DAlign.cs2DAlign.ptXY();      // Overlay의 중심 값
                        rs2DAlign.cs2DAlign.ptXYT CalMark = new rs2DAlign.cs2DAlign.ptXYT();    // Cal 마크 중심 값

                        int VisionNo = cbCamList.SelectedIndex;
                        double dResolution = Menu.frmAutoMain.Vision[VisionNo].CFG.Resolution;

                        Overlay.X = cogDS.Image.Width / 2;
                        Overlay.Y = cogDS.Image.Height / 2;
                        System.Threading.Thread.Sleep(500);

                        Vision[0].Capture(false, true, false, true);
                        if (Vision[0].PatternSearchEnum(ref CalMark.X, ref CalMark.Y, ref CalMark.T, ePatternKind.Cal))
                        {
                            CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;

                            Diff.Y = (CalMark.Y - Overlay.Y) * dResolution; //모터방향과 화면방향 Y축 중요!

                            if (Math.Abs(Diff.Y) < 0.1)
                            {
                                CenterPosMove = true;
                                bBendingCal3000 = false;
                                Vision[0].CalStep = 1000;
                                return;
                            }

                            motorMove.X = 0;
                            motorMove.Y = motorMove.Y + Diff.Y;
                            motorMove.T = 0;

                            Menu.frmAutoMain.IF.writeAlignXYTOffset(calMotionNo, motorMove.X, motorMove.Y, motorMove.T, eConvert.notUse, Vision[0].CFG.XYAxisRevers);
                            Vision[0].CalStep++;
                        }
                        else
                        {
                            CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;
                            CONST.bPCCalReq[CalOK(CalVisionNO, calKind)] = false;
                            Vision[0].CalStep = 0;
                            tmrCal.Enabled = false;
                            frmMsg.Visible = false;
                            MessageBox.Show("ERROR");
                        }
                    }
                    break;

                case 3001:
                    // Move Reply 클리어.
                    if (PosMoveReplyClear(CalVisionNO, calKind))
                    {
                        CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = true;
                        // 다음 스텝으로
                        Vision[0].CalStep++;
                    }
                    break;

                case 3002:
                    if (PosMoveComplete(CalVisionNO, calKind))
                    {
                        Vision[0].CalStep = 3000;
                    }
                    break;
                    #endregion Calibration 표준화
            }
        }

        private List<cs2DAlign.ptXY> SetYTCalPos(List<cs2DAlign.ptXY> calPosCam1)
        {
            List<cs2DAlign.ptXY> YTCalPosCam1 = new List<cs2DAlign.ptXY>();
            rs2DAlign.cs2DAlign.ptXY pt1 = new rs2DAlign.cs2DAlign.ptXY();
            pt1.X = dPixelX1;
            pt1.Y = dPixelY1;
            #region
            if (Vision[0].CFG.eCamName == nameof(eCamNO.LoadingPre1) || Vision[0].CFG.eCamName == nameof(eCamNO.LoadingPre2))
            {
                double TempPixelY = 0;
                TempPixelY = (Math.Abs(calPosCam1[4].X - calPosCam1[3].X) + Math.Abs(calPosCam1[5].X - calPosCam1[4].X)) / 2;

                for (int i = 0; i < (calCnt + 1); i++)
                {
                    if (i < 3)
                    {
                        pt1.X = calPosCam1[i].X;
                        pt1.Y = calPosCam1[i].Y - TempPixelY;
                    }
                    else if (i < 6)
                    {
                        pt1.X = calPosCam1[i].X;
                        pt1.Y = calPosCam1[i].Y;
                    }
                    else if (i < 9)
                    {
                        pt1.X = calPosCam1[i].X;
                        pt1.Y = calPosCam1[i].Y + TempPixelY;
                    }
                    YTCalPosCam1.Add(pt1);
                }
            }
            else
            {
                double TempPixelX = 0;
                TempPixelX = (Math.Abs(calPosCam1[4].Y - calPosCam1[1].Y) + Math.Abs(calPosCam1[7].Y - calPosCam1[4].Y)) / 2;

                for (int i = 0; i < (calCnt + 1); i++)
                {
                    if (i % 3 == 0)
                    {
                        pt1.X = calPosCam1[i].X - TempPixelX;
                        pt1.Y = calPosCam1[i].Y;
                    }
                    else if (i % 3 == 1)
                    {
                        pt1.X = calPosCam1[i].X;
                        pt1.Y = calPosCam1[i].Y;
                    }
                    else if (i % 3 == 2)
                    {
                        pt1.X = calPosCam1[i].X + TempPixelX;
                        pt1.Y = calPosCam1[i].Y;
                    }
                    YTCalPosCam1.Add(pt1);
                }
            }
            #endregion
            return YTCalPosCam1;
        }

        private void SetCenterOfRotation(int calCnt, ref rs2DAlign.cs2DAlign.ptXY[] fixPosCam, double dPixelX1, double dPixelY1, eCalPos calPos, bool bsymbol = false)
        {
            if (calCnt == 0)
            {
                fixPosCam[0].X = dPixelX1;
                fixPosCam[0].Y = dPixelY1;

            }
            else
            {
                fixPosCam[1].X = dPixelX1;
                fixPosCam[1].Y = dPixelY1;

                cs2DAlign.ptXY CalFixPosOffset1 = new cs2DAlign.ptXY();

                if (bsymbol)
                {
                    CalFixPosOffset1.X = Menu.frmSetting.revData.mOffset[CalVisionNO].CalFixPosOffset.X;
                    CalFixPosOffset1.Y = Menu.frmSetting.revData.mOffset[CalVisionNO].CalFixPosOffset.Y;
                }
                else
                {
                    CalFixPosOffset1.X = (-1) * Menu.frmSetting.revData.mOffset[CalVisionNO].CalFixPosOffset.X;
                    CalFixPosOffset1.Y = (-1) * Menu.frmSetting.revData.mOffset[CalVisionNO].CalFixPosOffset.Y;
                }

                Menu.rsAlign.setCenterOfRotation((int)calPos, fixPosCam[0], fixPosCam[1], firstCoord.T, secondCoord.T, CalFixPosOffset1);
                CopyRotationCalpos(calPos, fixPosCam[0], fixPosCam[1], firstCoord.T, secondCoord.T, CalFixPosOffset1); //pcy210118
            }
        }
        //pcy210118
        private void Copy9PointCalpos(eCalPos iCalNO, List<cs2DAlign.ptXY> p_pxPos, cs2DAlign.ptXY PixelCount)
        {
            eCalPos temp = ChangeAttachtoAttachInsp(iCalNO);
            if (temp != iCalNO)
            {
                Menu.rsAlign.setCamCalibration((int)temp, p_pxPos, PixelCount);
            }
        }
        //pcy210118
        private void CopyRotationCalpos(eCalPos iCalNO, cs2DAlign.ptXY p_firstPoint, cs2DAlign.ptXY p_secondPoint, double firstTheta, double secondCoord, cs2DAlign.ptXY CalFixPosOffset1)
        {
            eCalPos temp = ChangeAttachtoAttachInsp(iCalNO);
            if (temp != iCalNO)
            {
                Menu.rsAlign.setCenterOfRotation((int)temp, p_firstPoint, p_secondPoint, firstTheta, secondCoord, CalFixPosOffset1);
            }
        }
        private void CalErrEnd(string swhy = "")
        {
            CONST.bPCCalReq[PosMove(CalVisionNO, calKind)] = false;
            CONST.bPCCalReq[CalOK(CalVisionNO, calKind)] = false;
            Vision[0].CalStep = 0;
            tmrCal.Enabled = false;
            frmMsg.Visible = false;
            MessageBox.Show(swhy + "ERROR");
        }
        private void SetTargetText(csVision[] Vision)
        {
            if (Vision[0] == null)
                return;
            int ino = cbRecognition.SelectedIndex;

            if (Vision[0] != null)
            {
                if (Vision[0].CFG.CalType == eCalType.Cam1Cal2 || Vision[0].CFG.CalType == eCalType.Cam1Type)
                {
                    txtTargetX[0].Text = Vision[0].CFG.TargetX[ino + 0].ToString("0.000");
                    txtTargetY[0].Text = Vision[0].CFG.TargetY[ino + 0].ToString("0.000");
                    txtTargetX[1].Text = Vision[0].CFG.TargetX[ino + 1].ToString("0.000");
                    txtTargetY[1].Text = Vision[0].CFG.TargetY[ino + 1].ToString("0.000");
 
                }
                if (Vision[0].CFG.CalType == eCalType.Cam2Type
                        || Vision[0].CFG.CalType == eCalType.Cam3Type
                        || Vision[0].CFG.CalType == eCalType.Cam4Type)
                {
                    txtTargetX[0].Text = Vision[0].CFG.TargetX[ino].ToString("0.000");
                    txtTargetY[0].Text = Vision[0].CFG.TargetY[ino].ToString("0.000");
                    txtTargetX[1].Text = Vision[1].CFG.TargetX[ino].ToString("0.000");
                    txtTargetY[1].Text = Vision[1].CFG.TargetY[ino].ToString("0.000");
                }
                if (Vision[0].CFG.CalType == eCalType.Cam3Type
                    || Vision[0].CFG.CalType == eCalType.Cam4Type)
                {
                    txtTargetX[2].Text = Vision[2].CFG.TargetX[ino].ToString("0.000");
                    txtTargetY[2].Text = Vision[2].CFG.TargetY[ino].ToString("0.000");

                }
                if (Vision[0].CFG.CalType == eCalType.Cam4Type)
                {
                    txtTargetX[3].Text = Vision[3].CFG.TargetX[ino].ToString("0.000");
                    txtTargetY[3].Text = Vision[3].CFG.TargetY[ino].ToString("0.000");
                }
            }
        }
        private void SetTargetCogGraphic(csVision[] Vision)
        {
            //타겟 그래픽 표시
            if (Vision[0] == null)
                return;
            int ino = cbRecognition.SelectedIndex;
            CogPointMarker p = new CogPointMarker();
            p.SizeInScreenPixels = 100;
            p.LineWidthInScreenPixels = 3;
            if (Vision[0].CFG.CalType == eCalType.Cam1Cal2 || Vision[0].CFG.CalType == eCalType.Cam1Type)
            {          
                p.X = Vision[0].CFG.TargetX[ino + 0];
                p.Y = Vision[0].CFG.TargetY[ino + 0];
                Vision[0].PaintPoint(p, "Target");
                p.X = Vision[0].CFG.TargetX[ino + 1];
                p.Y = Vision[0].CFG.TargetY[ino + 1];
                Vision[0].PaintPoint(p, "Target");
            }
            if (Vision[0].CFG.CalType == eCalType.Cam2Type
                    || Vision[0].CFG.CalType == eCalType.Cam3Type
                    || Vision[0].CFG.CalType == eCalType.Cam4Type)
            {
                p.X = Vision[0].CFG.TargetX[ino];
                p.Y = Vision[0].CFG.TargetY[ino];
                Vision[0].PaintPoint(p, "Target");
                p.X = Vision[1].CFG.TargetX[ino];
                p.Y = Vision[1].CFG.TargetY[ino];
                Vision[1].PaintPoint(p, "Target");
            }
            if (Vision[0].CFG.CalType == eCalType.Cam3Type
                || Vision[0].CFG.CalType == eCalType.Cam4Type)
            {
                p.X = Vision[2].CFG.TargetX[ino];
                p.Y = Vision[2].CFG.TargetY[ino];
                Vision[2].PaintPoint(p, "Target");

            }
            if (Vision[0].CFG.CalType == eCalType.Cam4Type)
            {
                p.X = Vision[3].CFG.TargetX[ino];
                p.Y = Vision[3].CFG.TargetY[ino];
                Vision[3].PaintPoint(p, "Target");
            }
        }
        private void LiveEndEvent(object sender, EventArgs e)
        {
            LiveEndEventArgs args = (LiveEndEventArgs)e;
        }

        public void SetInitRecipeCamSelect(int nSelectIndex = 0)
        {
            SetVision(nSelectIndex, Menu.frmAutoMain.Vision[nSelectIndex].CFG.CalType);
        }

        private void SetVision(int nCam1, eCalType CalType)
        {
            if (CalType == eCalType.Cam1Type || CalType == eCalType.Cam1Cal2)
            {
                Menu.frmAutoMain.setVision(ref Vision[0], nCam1);

                Vision[0].DispChange(cogDS);
                Vision[1] = null;
                Vision[2] = null;
                Vision[3] = null;

                //버튼 나중에..
                btnOpenCam2.Visible = false;
                btnOpenCam3.Visible = false;
                btnOpenCam1.Width = 216;
            }
            else if (CalType == eCalType.Cam2Type)
            {
                Menu.frmAutoMain.setVision(ref Vision[0], nCam1);
                Menu.frmAutoMain.setVision(ref Vision[1], nCam1 + 1);

                Vision[0].DispChange(cogDS);
                Vision[1].DispChange(cogDS2);
                Vision[2] = null;
                Vision[3] = null;

                btnOpenCam2.Visible = true;
                btnOpenCam1.Width = 105;
            }
            else if (CalType == eCalType.Cam3Type)
            {
                Menu.frmAutoMain.setVision(ref Vision[0], nCam1);
                Menu.frmAutoMain.setVision(ref Vision[1], nCam1 + 1);
                Menu.frmAutoMain.setVision(ref Vision[2], nCam1 + 2);

                Vision[0].DispChange(cogDS);
                Vision[1].DispChange(cogDS2);
                Vision[2].DispChange(cogDS3);
                Vision[3] = null;

                btnOpenCam2.Visible = true;
                btnOpenCam1.Width = 105;
            }
            else if (CalType == eCalType.Cam4Type)
            {
                Menu.frmAutoMain.setVision(ref Vision[0], nCam1);
                Menu.frmAutoMain.setVision(ref Vision[1], nCam1 + 1);
                Menu.frmAutoMain.setVision(ref Vision[2], nCam1 + 2);
                Menu.frmAutoMain.setVision(ref Vision[3], nCam1 + 3);

                Vision[0].DispChange(cogDS);
                Vision[1].DispChange(cogDS2);
                Vision[2].DispChange(cogDS3);
                Vision[3].DispChange(cogDS4);

                btnOpenCam2.Visible = true;
                btnOpenCam1.Width = 105;
            }
        }

        private void SetUseCameCount(eCalType calType, int VisionNo, int kind, ref eCalPos[] calpos, ref double[] sX, ref double[] sY)
        {
            if (calType == eCalType.Cam1Type || calType == eCalType.Cam1Cal2)
            {
                cogDS.InteractiveGraphics.Clear();

                Size cSize = new System.Drawing.Size(1240, 850);
                cogDS.Size = cSize;
                cogDS2.Visible = false;
                cogDS3.Visible = false;
                cogDS4.Visible = false;
            }
            else if (calType == eCalType.Cam2Type)
            {
                cogDS.InteractiveGraphics.Clear();
                cogDS2.InteractiveGraphics.Clear();

                Size cSize = new System.Drawing.Size(620, 850);
                cogDS.Size = cSize;
                cogDS2.Size = cSize;
                cogDS2.Visible = true;
                cogDS3.Visible = false;
                cogDS4.Visible = false;
            }
            else if (calType == eCalType.Cam3Type)
            {
                cogDS.InteractiveGraphics.Clear();
                cogDS2.InteractiveGraphics.Clear();
                cogDS3.InteractiveGraphics.Clear();
                cogDS4.InteractiveGraphics.Clear();

                Size cSize = new System.Drawing.Size(620, 425);
                cogDS.Size = cSize;
                cogDS2.Size = cSize;
                cogDS3.Size = cSize;
                cogDS4.Size = cSize;
                cogDS2.Visible = true;
                cogDS3.Visible = true;
                cogDS4.Visible = true;
            }
            else if (calType == eCalType.Cam4Type)
            {
                cogDS.InteractiveGraphics.Clear();
                cogDS2.InteractiveGraphics.Clear();
                cogDS3.InteractiveGraphics.Clear();
                cogDS4.InteractiveGraphics.Clear();

                Size cSize = new System.Drawing.Size(620, 425);
                cogDS.Size = cSize;
                cogDS2.Size = cSize;
                cogDS3.Size = cSize;
                cogDS4.Size = cSize;
                cogDS2.Visible = true;
                cogDS3.Visible = true;
                cogDS4.Visible = true;
            }

            for (int i = 0; i < Vision.Length; i++)
            {
                if (Vision[i] != null)
                {
                    Menu.frmAutoMain.GetCalPos(ref calpos[i], VisionNo + i, kind);
                    //pcy210118
                    calpos[i] = ChangeAttachtoAttachInsp(calpos[i]);
                    if (calpos[i] != eCalPos.Err)
                    {
                        Menu.rsAlign.getScale((int)calpos[i], ref sX[i], ref sY[i]);
                    }
                    else
                    {
                        sX[i] = 1;
                        sY[i] = 1;
                    }
                    gbCam[i].Visible = true;

                    if (CONST.eLightType.Light12V == Vision[i].CFG.LightType)
                    {
                        tb[i].Maximum = 255;
                    }
                    else if (CONST.eLightType.Light5V == Vision[i].CFG.LightType)
                    {
                        tb[i].Maximum = 1023;
                    }
                    //프로그램 시작시 초기값 넣어주니까 여기서는 현재값만 표시하면됨
                    ViewCurrentValue(i);

                    Vision[i].Capture(false, true, false, true);
                }
                else
                {
                    gbCam[i].Visible = false;
                }
            }

        }
        private void ViewCurrentValue(int i)
        {
            if (Vision[i].ISExposure > tbExposure[i].Minimum && Vision[i].ISExposure < tbExposure[i].Maximum)
                tbExposure[i].Value = Vision[i].ISExposure;
            txtExposure[i].Text = Vision[i].ISExposure.ToString();

            int ISLight = Menu.frmAutoMain.ISLight[Vision[i].CFG.Light1Comport, Vision[i].CFG.Light1CH];
            if (ISLight > tb[i].Minimum && ISLight < tb[i].Maximum)
                tb[i].Value = ISLight;
            txtLight[i].Text = ISLight.ToString();

            txtContrast[i].Text = Vision[i].ISContrast.ToString("0.0");
        }

        private void cbCamList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbCamList.SelectedIndex == -1) return;

            if (cbCamList.Text == "Not Use") tbTrace.Visible = false;
            else tbTrace.Visible = true;

            if (cbCamList.Text != "Not Use")
            {
                double sX1 = 0;
                double sY1 = 0;
                double sX2 = 0;
                double sY2 = 0;
                double sX3 = 0;
                double sY3 = 0;
                double sX4 = 0;
                double sY4 = 0;
                double[] sX = new double[] { sX1, sX2, sX3, sX4 };
                double[] sY = new double[] { sY1, sY2, sY3, sY4 };
                TextBox[] tb_ScaleX = new TextBox[] { tb_ScaleLX, tb_ScaleRX, tb_ScaleLX2, tb_ScaleRX2 };
                TextBox[] tb_ScaleY = new TextBox[] { tb_ScaleLY, tb_ScaleRY, tb_ScaleLY2, tb_ScaleRY2 };

                short CamNo = (short)cbCamList.SelectedIndex;
                string camName = cbCamList.SelectedItem.ToString();
                //카메라 선택 강제이동(이점이 많음)
                string stemp = camName.Substring(camName.Length - 1, 1);
                if (Menu.frmAutoMain.Vision[CamNo].CFG.CalType == eCalType.Cam2Type)
                {
                    if (stemp == "2")
                    {
                        cbCamList.SelectedIndex -= 1;
                    }
                }
                else if (Menu.frmAutoMain.Vision[CamNo].CFG.CalType == eCalType.Cam3Type)
                {
                    if (stemp == "2")
                    {
                        cbCamList.SelectedIndex -= 1;
                    }
                    if (stemp == "3")
                    {
                        cbCamList.SelectedIndex -= 2;
                    }
                }
                //강제이동 끝
                CamNo = (short)cbCamList.SelectedIndex;
                camName = cbCamList.SelectedItem.ToString();

                int VisionNo = cbCamList.SelectedIndex;
                int kind = cbBendSelect.SelectedIndex;
                kind = 1;

                eCalPos calpos1 = 0;
                eCalPos calpos2 = 0;
                eCalPos calpos3 = 0;
                eCalPos calpos4 = 0;
                eCalPos[] calpos = new eCalPos[] { calpos1, calpos2, calpos3, calpos4 };

                Live(false, LiveEndEvent);

                SetInitRecipeCamSelect(VisionNo);
                SetUseCameCount(Vision[0].CFG.CalType, VisionNo, kind, ref calpos, ref sX, ref sY);
                SetTargetText(Vision);
                SetTargetCogGraphic(Vision);


                gbPreOffset.Visible = false;
                cbBendSelect.Visible = false;
                //pcy190405 Tranfer cal없으니 항상 1로 사용.

                cbAlignMode.Visible = false;
                cbAlignMode.SelectedIndex = 0;
                pnSetTaget.Visible = false;
                gbUVRWcal.Visible = false;
                gbScale.Visible = false;
                gbCheckerCal.Visible = false;
                cbPointLine.Visible = true;
                cbCalImageTest.Visible = false;
                //LCheckTest.Visible = false;
                btnDetachInspection1.Visible = false;
                //pcy210203
                //btnDetachInspection2.Visible = false;
                gbDetachInspection.Visible = false;
                gbSCFInspOffsetbox.Visible = false;

                cbAttach.Visible = false;
                //20.09.17 lkw
                cbXYCal.Visible = false;
                lbJigX.Visible = false;
                lbJigY.Visible = false;
                txtJigX.Visible = false;
                txtJigY.Visible = false;
                lbJigXmm.Visible = false;
                lbJigYmm.Visible = false;

                //20200926 cjm
                SCFInspOffset();

                //19/06/2021 MCR Read. Add.  
                //btnMCRSearch.Visible = false;

                for (int i = 0; i < Vision.Length; i++)
                {
                    if (Vision[i] != null)
                    {
                        tb_ScaleX[i].Text = sX[i].ToString();
                        tb_ScaleY[i].Text = sY[i].ToString();
                    }
                }

                //기본위치
                cogDS.Location = new System.Drawing.Point(0, 0);
                cogDS2.Location = new System.Drawing.Point(620, 0);
                cogDS3.Location = new System.Drawing.Point(0, 425);
                cogDS4.Location = new System.Drawing.Point(620, 425);

                gbCam1.Location = new System.Drawing.Point(5, 623);
                gbCam2.Location = new System.Drawing.Point(313, 623);
                gbCam3.Location = new System.Drawing.Point(5, 727);
                gbCam4.Location = new System.Drawing.Point(313, 727);

                pnSelectReel.Visible = false;

                //cbRegion.Visible = false;
                //btnRegionSave.Visible = false;

                //cbDetachInsp.Visible = false;
                cbHistogram.Visible = false;
                txtTH.Visible = false;
                btnDetachInsp.Visible = false;
                gbRegion.Visible = false;

                if (Vision[0].CFG.eCamName == nameof(eCamNO.LoadingPre1) || Vision[0].CFG.eCamName == nameof(eCamNO.LoadingPre2))
                {
                    if (Vision[0].CFG.eCamName == nameof(eCamNO.LoadingPre1) || Vision[0].CFG.eCamName == nameof(eCamNO.LoadingPre2))
                    {
                        //Size sz = new Size(266, 200);
                        Size sz = new Size(425, 230);
                        gbDetachInspection.Size = sz;

                        lblNote.Location = new Point(268, 112);
                        groupBox12.Visible = false;
                        rbSelect2.Visible = false;
                    }
                    else
                    {
                        //Size sz = new Size(1063, 200);
                        Size sz = new Size(1235, 230);
                        gbDetachInspection.Size = sz;
                    }

                    pnSelectReel.Visible = true;
                    btnDetachInspection1.Visible = false;
                    gbDetachInspection.Visible = false;
                }
                else if (Vision[0].CFG.eCamName == nameof(eCamNO.Laser1) || Vision[0].CFG.eCamName == nameof(eCamNO.Laser2))
                {

                    //Size sz = new Size(266, 200);
                    Size sz = new Size(425, 230);
                    gbDetachInspection.Size = sz;

                    lblNote.Location = new Point(268, 112);
                    groupBox12.Visible = false;
                    gbScale.Visible = true;
                    rbSelect2.Visible = false;
                    //btnMCRSearch.Visible = true;

                    btnDetachInspection1.Visible = false;
                    gbDetachInspection.Visible = false;

                    int iTH = 0;
                    cogrectangle = HistoRegionRead(Vision[0].CFG.eCamName, ref iTH, eHistogram.MCRRegion);
                    //cbRegion.Visible = true;
                    //btnRegionSave.Visible = true;

                    cbHistogram.Visible = true;
                    cbHistogram.SelectedIndex = 0;
                    txtTH.Visible = true;
                    btnDetachInsp.Visible = true;
                    gbRegion.Visible = true;
                    gbCheckerCal.Visible = true;
                    cbCalImageTest.Visible = true;
                }
                //else if (Vision[0].CFG.eCamName == nameof(eCamNO.Attach1_1) || Vision[0].CFG.eCamName == nameof(eCamNO.Attach1_3) ||
                //    Vision[0].CFG.eCamName == nameof(eCamNO.Attach2_1) || Vision[0].CFG.eCamName == nameof(eCamNO.Attach2_3))
                //{
                //    gbSCFInspOffsetbox.Visible = true;
                //}
                //20.09.17 lkw
                //else if (CONST.PCNo == 2 && CamNo == Vision_No.vsSCFPanel1)
                //{
                //    cbXYCal.Visible = true;
                //    lbJigX.Visible = true;
                //    lbJigY.Visible = true;
                //    txtJigX.Visible = true;
                //    txtJigY.Visible = true;
                //    lbJigXmm.Visible = true;
                //    lbJigYmm.Visible = true;

                //    txtJigX.Enabled = true;
                //    txtJigY.Enabled = true;

                //    txtJigX.ReadOnly = false;
                //    txtJigY.ReadOnly = false;
                //}
                //else if (CONST.PCNo == 2 && CamNo == Vision_No.vsSCFReel1)
                //{
                //    cogDS.Location = new System.Drawing.Point(0, 425);
                //    cogDS2.Location = new System.Drawing.Point(620, 425);
                //    cogDS3.Location = new System.Drawing.Point(0, 0);
                //    cogDS4.Location = new System.Drawing.Point(620, 0);

                //    gbCam1.Location = new System.Drawing.Point(5, 727);
                //    gbCam2.Location = new System.Drawing.Point(313, 727);
                //    gbCam3.Location = new System.Drawing.Point(5, 623);
                //    gbCam4.Location = new System.Drawing.Point(313, 623);
                //}

                cbRecognition_SelectedIndexChanged(null, null);
                return;
            }
        }
        //JJ, 2017-05-19 : Auto Screen Full ---- start , cogDS 화면에 DoubleClick 이벤트를 등록시켜 놓는다
        private Panel parentCogDS = null;
        private CogDisplay selectCogDS = null;
        private Point pCogDS;
        private Size sizeCogDS;

        private void cogDS_DoubleClick(object sender, EventArgs e)
        {
            CogDisplay cogDS = sender as CogDisplay;
            if (parentCogDS == null) // 전체 화면 모드로 변경
            {
                parentCogDS = cogDS.Parent as Panel;
                selectCogDS = cogDS;
                sizeCogDS = cogDS.Size;
                pCogDS = cogDS.Location;

                //parentCogDS.Controls.Remove(cogDS);

                //this.Controls.Add(cogDS);

                int XMargin = 0;
                int YMargin = 0;

                cogDS.Location = new Point(XMargin, YMargin);
                cogDS.Size = new Size(panel1.Width - XMargin * 2, panel1.Bottom - YMargin * 2);
                //this.Controls.SetChildIndex(cogDS, 0);
            }
            else // 원래 화면으로 변경
            {
                if (selectCogDS.Equals(cogDS) == false)
                {
                    return; // 현재 보여 지고 있는 화면이 아니면 리턴
                }
                //this.Controls.Remove(cogDS);
                cogDS.Location = pCogDS;
                cogDS.Size = sizeCogDS;
                //parentCogDS.Controls.Add(cogDS);
                parentCogDS = null;
            }
        }

        private GroupBox gbParent = null;
        private CogDisplay selectCogDS1 = null;
        private Point pCogDS1;
        private Size sizeCogDS1;
        private void cogDS_DoubleClick1(object sender, EventArgs e)
        {
            CogDisplay cogDS = sender as CogDisplay;
            if (gbParent == null)
            {
                gbParent = cogDS.Parent as GroupBox;
                selectCogDS1 = cogDS;
                sizeCogDS1 = cogDS.Size;
                pCogDS1 = cogDS.Location;

                gbParent.Controls.Remove(cogDS);

                this.Controls.Add(cogDS);

                int XMargin = 250;
                int YMargin = 50;

                cogDS.Location = new Point(XMargin, YMargin);
                cogDS.Size = new Size(this.Width - XMargin * 2, this.Bottom - YMargin * 2);
                this.Controls.SetChildIndex(cogDS, 0);
            }
            else
            {
                if (selectCogDS1.Equals(cogDS) == false) return;
                this.Controls.Remove(cogDS);
                cogDS.Location = pCogDS1;
                cogDS.Size = sizeCogDS1;
                gbParent.Controls.Add(cogDS);
                gbParent = null;
            }
        }

        public void VisionPasswordDisplay()
        {
            pnVisionPassword.Width = tabPage5.Width;
            pnVisionPassword.Height = tabPage5.Height;
            pnVisionPassword.Left = 0;
            pnVisionPassword.Top = 0;
            pnVisionPassword.Visible = true;

            lblVisionPassword.Left = (tabPage5.Width / 2) - (lblVisionPassword.Width / 2);
            txtVisionPassword.Left = (tabPage5.Width / 2) - (lblVisionPassword.Width / 2);
            txtVisionPassword.Focus();
        }

        public void TracePasswordDisplay()
        {
            pnTracePassword.Width = tabPage5.Width;
            pnTracePassword.Height = tabPage5.Height;
            pnTracePassword.Left = 0;
            pnTracePassword.Top = 0;
            pnTracePassword.Visible = true;

            lblTracePassword.Left = (tabPage5.Width / 2) - (lblTracePassword.Width / 2);
            txtTracePassword.Left = (tabPage5.Width / 2) - (lblTracePassword.Width / 2);

            lblTranceLastPointAdd.Text = "Trace " + (Int32.Parse(txtTracePoint.Text) + 1).ToString() + "Point Position Y(mm)";

            txtTracePassword.Focus();
        }

        private void btnOpenCam1_Click(object sender, EventArgs e)
        {
            if (Vision[0] == null)
            {
                MessageBox.Show("CamName is not selected!");
                return;
            }

            if (functionInterlock())
            {
                try
                {
                    Live(false);

                    int CamNo = cbCamList.SelectedIndex;

                    string path = Path.Combine(CONST.cImagePath, Vision[0].CFG.Name.Trim());

                    OpenFileDialog OF = new OpenFileDialog();
                    OF.InitialDirectory = path;
                    OF.FilterIndex = 2;

                    if (OF.ShowDialog(this) == DialogResult.OK)
                    {
                        Vision[0].ImgDisplay(OF.FileName);
                    }

                    cLog.LogSave(Vision[0].CFG.Name + " Image Click");
                }
                catch (Exception EX)
                {
                    cLog.ExceptionLogSave("btnOpen1_Click" + "," + EX.GetType().Name + "," + EX.Message);
                }
            }
        }

        private void btnOpenCam2_Click(object sender, EventArgs e)
        {
            if (Vision[1] == null)
            {
                MessageBox.Show("CamName is not selected!");
                return;
            }

            if (functionInterlock())
            {
                try
                {
                    Live(false);

                    int CamNo = cbCamList.SelectedIndex;

                    string path = Path.Combine(CONST.cImagePath, Vision[1].CFG.Name.Trim());

                    OpenFileDialog OF = new OpenFileDialog();
                    OF.InitialDirectory = path;
                    OF.FilterIndex = 2;

                    if (OF.ShowDialog(this) == DialogResult.OK)
                    {
                        Vision[1].ImgDisplay(OF.FileName);
                    }

                    cLog.LogSave(Vision[1].CFG.Name + " Image Click");
                }
                catch (Exception EX)
                {
                    cLog.ExceptionLogSave("btnOpen3_Click" + "," + EX.GetType().Name + "," + EX.Message);
                }
            }
        }
        private void btnOpenCam3_Click(object sender, EventArgs e)
        {
            if (Vision[2] == null)
            {
                MessageBox.Show("CamName is not selected!");
                return;
            }

            if (functionInterlock())
            {
                try
                {
                    Live(false);

                    int CamNo = cbCamList.SelectedIndex;

                    string path = Path.Combine(CONST.cImagePath, Vision[2].CFG.Name.Trim());

                    OpenFileDialog OF = new OpenFileDialog();
                    OF.InitialDirectory = path;
                    OF.FilterIndex = 2;

                    if (OF.ShowDialog(this) == DialogResult.OK)
                    {
                        Vision[2].ImgDisplay(OF.FileName);
                    }

                    cLog.LogSave(Vision[2].CFG.Name + " Image Click");
                }
                catch (Exception EX)
                {
                    cLog.ExceptionLogSave("btnOpen3_Click" + "," + EX.GetType().Name + "," + EX.Message);
                }
            }
        }

        private void cbOveray_CheckedChanged(object sender, EventArgs e)
        {
            if (functionInterlock())
            {
                foreach (var s in Vision)
                {
                    if (s != null) s.Overay(cbOveray.Checked);
                }
            }
        }

        private void LiveButtonStatusChange(object sender, EventArgs e)
        {
            if (Vision[0] != null)
            {
                if (Vision[0].liveOn)
                {
                    btnLiveImage.BackColor = Color.Blue;
                    btnLiveImage.ForeColor = Color.White;
                }
                else
                {
                    btnLiveImage.BackColor = Color.White;
                    btnLiveImage.ForeColor = Color.Black;
                }
            }
        }

        public void Live(bool bOn, EventHandler liveEvent = null)
        {
            foreach (var s in Vision)
            {
                if (s != null) s.Live(bOn);
            }
            LiveButtonStatusChange(this, null);
        }

        private void btnLive_Click(object sender, EventArgs e)
        {
            if (Vision[0] == null)
            {
                MessageBox.Show("CamName is not selected!");
                return;
            }

            cogDS.InteractiveGraphics.Clear();
            cogDS2.InteractiveGraphics.Clear();
            cbOveray.Checked = false;

            if (functionInterlock())
            {
                if (btnLiveImage.BackColor != Color.Blue)
                {
                    Live(true);
                }
                else
                {
                    Live(false);
                }
            }
        }

        private bool functionInterlock()
        {
            if (cbCamList.Text == "Not Use")
            {
                return false;
            }

            return true;
        }

        //private short CalCount = 0;
        private bool bBendingCal = false;
        private bool bBendingCal3000 = false;
        private bool bLoadPreCal = false;

        private void btnCal_Click(object sender, EventArgs e)
        {
            if (Vision[0] == null)
            {
                MessageBox.Show("CamName is not selected!");
                return;
            }

            if (Menu.frmlogin.LogInCheck())
            {
                if (functionInterlock())
                {
                    short CamNo = (short)cbCamList.SelectedIndex;

                    if (MessageBox.Show("Calibration Start!", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        if (tmrCal.Enabled)
                        {
                            MessageBox.Show("Calibraition is not Finished");
                            return;
                        }
                        else
                        {            
                            frmMsg.ShowMessage(cbCamList.SelectedItem.ToString() + " : Calibration Running....");
                            frmMsg.Size = new Size(968, 133);
                            frmMsg.Visible = true;

                            SetREVCalStep();

                            CenterPosMove = false;   //Center Position Move Reset
                            tmrCal.Enabled = true;
                            Diff.Y = 0;

                            motorMove.Y = 0;
                            //}
                        }
                        cLog.LogSave(Vision[0].CFG.Name + " Calibration Click");
                    }
                }
            }
            else
            {
                MessageBox.Show("Confrm LoginUser");
            }
        }

        // jyh Insert
        private void SetREVCalStep()
        {

            string camName = cbCamList.SelectedItem.ToString();
            CalVisionNO = cbCamList.SelectedIndex;
            //2018.11.08 CalType 추가
            CalType = Vision[0].CFG.CalType;
            calX = Menu.frmSetting.revData.mOffset[CalVisionNO].CalXYT.X;
            calY = Menu.frmSetting.revData.mOffset[CalVisionNO].CalXYT.Y;
            calT = Menu.frmSetting.revData.mOffset[CalVisionNO].CalXYT.T;
            //if (cbCalCnt.Visible) CalCount = (short)cbCalCnt.SelectedIndex;
            //else CalCount = 0;
            if (cbCalPos.SelectedIndex < 0) cbCalPos.SelectedIndex = 0;
            calKind = cbCalPos.SelectedIndex;
            uvrwCal = false;  //UVRW를 사용하는 Cal일 경우
            calDataMove = false;  // 다른 PC로 CalData 넘길지 여부
            teachPosSave = false;  // Cal시의 Position 저장.

            //2020.09.25 lkw
            //TYRCal = false;
            TYRCal = eConvert.notUse;

            startUVW.X1 = 0;
            startUVW.Y1 = 0;
            startUVW.X2 = 0;
            startUVW.Y2 = 0;
            startXYT.X = 0;
            startXYT.Y = 0;
            startXYT.T = 0;

            firstCoord.X = 0;
            firstCoord.Y = 0;
            firstCoord.T = -calT;
            secondCoord.X = 0;
            secondCoord.Y = 0;
            secondCoord.T = calT;

            //if (calKind < 0) calKind = 0;
            if (cbFixType.SelectedIndex == -1) cbFixType.SelectedIndex = 0;
            if (cbFixType.SelectedIndex == 1) backlashuse = true;
            else backlashuse = false;

            Vision[0].CalStep = 1000;

            int VisionNo = cbCamList.SelectedIndex;
            //if (cbCamList.SelectedIndex == 1 || cbCamList.SelectedIndex == 3 || cbCamList.SelectedIndex == 5 || cbCamList.SelectedIndex == 7)
            //{
            //    VisionNo = cbCamList.SelectedIndex - 1;
            //}

            //2018.11.12 khs CalType 변경
            if (CalType == eCalType.Cam1Type
                || CalType == eCalType.Cam2Type
                || CalType == eCalType.Cam3Type
                || CalType == eCalType.Cam4Type)
            {
                if (pnSelectReel.Visible)
                {
                    if (rbReel1.Checked)
                    {
                        calKind = 0;
                        Menu.frmAutoMain.GetCalPos(ref calPos_L, VisionNo, 0);
                        
                    }
                    else if (rbReel2.Checked)
                    {
                        calKind = 1;
                        Menu.frmAutoMain.GetCalPos(ref calPos_L, VisionNo, 1);                        
                    }
                }
                else
                    Menu.frmAutoMain.GetCalPos(ref calPos_L, VisionNo, calKind);
            }
            if (CalType == eCalType.Cam2Type
                || CalType == eCalType.Cam3Type
                || CalType == eCalType.Cam4Type)
            {
                if (rbReel1.Checked)
                {
                    calKind = 0;
                    Menu.frmAutoMain.GetCalPos(ref calPos_R, VisionNo + 1, 0);

                }
                else if (rbReel2.Checked)
                {
                    calKind = 1;
                    Menu.frmAutoMain.GetCalPos(ref calPos_R, VisionNo + 1, 1);
                }
                else Menu.frmAutoMain.GetCalPos(ref calPos_R, VisionNo + 1, calKind);
            }
            if (CalType == eCalType.Cam3Type
                || CalType == eCalType.Cam4Type)
            {
                Menu.frmAutoMain.GetCalPos(ref calPos_3, VisionNo + 2, calKind);
            }
            if (CalType == eCalType.Cam4Type)
            {
                Menu.frmAutoMain.GetCalPos(ref calPos_4, VisionNo + 3, calKind);
            }
            if (CalType == eCalType.Cam1Cal2)
            {
                //cam1인데 calpos두개 쓸때
                if (rbReel1.Checked)
                {
                    Menu.frmAutoMain.GetCalPos(ref calPos_L, VisionNo, 0);
                    Menu.frmAutoMain.GetCalPos(ref calPos_R, VisionNo, 0, true);
                }
                else if (rbReel2.Checked)
                {
                    Menu.frmAutoMain.GetCalPos(ref calPos_L, VisionNo, 1);
                    Menu.frmAutoMain.GetCalPos(ref calPos_R, VisionNo, 1, true);
                }
            }

            //if (calPos_L == eCalPos.Bend1_1Arm) uvrwCal = true;


            //시간나면 변경..
            calMotionNo = eCamNametoMotion(Vision[0].CFG.eCamName, calKind);
            //else if (CONST.PCNo == 4)
            //{
            //    if (camName == Menu.frmAutoMain.Vision[Vision_No.vsTempAttach].CFG.Name)
            //    {
            //        calMotionNo = CONST.AAM_PLC2.OffsetAddress.TempAttach;
            //        if (calPos_L == eCalPos.TempAttach1_1) TYRCal = eConvert.TempAttach1;
            //        else if (calPos_L == eCalPos.TempAttach2_1) TYRCal = eConvert.TempAttach2;
            //        Menu.frmAutoMain.IF.readTaxisDegree(TYRCal);
            //    }
            //    else if (camName == Menu.frmAutoMain.Vision[Vision_No.vsEMIAttach].CFG.Name)
            //    {
            //        calMotionNo = CONST.AAM_PLC2.OffsetAddress.EMIAttach;
            //        if (calPos_L == eCalPos.EMIAttach1_1) TYRCal = eConvert.EMIAttach1;
            //        else if (calPos_L == eCalPos.EMIAttach2_1) TYRCal = eConvert.EMIAttach2;
            //        Menu.frmAutoMain.IF.readTaxisDegree(TYRCal);
            //    }
            //}

            YTCal = Vision[0].CFG.YTCalUse;
            if (uvrwCal) YTCal = false; //한군데서 방식이 다른 cal일때 주의

            calMove1[0] = (int)calPos_L;
            calMove1[1] = calX;
            calMove1[2] = calY;
            if (CalType == eCalType.Cam2Type
                || CalType == eCalType.Cam3Type)
            {
                calMove2[0] = (int)calPos_R;
                calMove2[1] = calX;
                calMove2[2] = calY;
            }
            if (CalType == eCalType.Cam3Type)
            {
                calMove3[0] = (int)calPos_3;
                calMove3[1] = calX;
                calMove3[2] = calY;
            }
        }
        public int eCamNametoMotion(string ecamname, int calKind)
        {
            //eCamName를 넣으면 관여된 Motor address를 리턴하는 함수
            int nNo = -1;
            switch (ecamname)
            {
                case nameof(eCamNO.LoadingPre1):
                    if (calKind == 0) nNo = Address.VisionOffset.LoadingPre1;
                    else nNo = Address.VisionOffset.LoadingPre2;
                    break;
                case nameof(eCamNO.LoadingPre2):
                    if (calKind == 0) nNo = Address.VisionOffset.LoadingPre1;
                    else nNo = Address.VisionOffset.LoadingPre2;
                    break;
                case nameof(eCamNO.Laser1):
                    nNo = Address.VisionOffset.LaserAlign1;
                    break;
                case nameof(eCamNO.Laser2):
                    nNo = Address.VisionOffset.LaserAlign2;
                    break;
            }
            return nNo;
        }
        // jyh Insert

        #region CogDisplay

        private void cogDS_MouseDown(object sender, MouseEventArgs e)
        {
            if (cbDist.Checked && Vision[0] != null)
            {
                Vision[0].MouseDown(e.X, e.Y);
            }
        }

        private void cogDS2_MouseDown(object sender, MouseEventArgs e)
        {
            if (cbDist.Checked && Vision[1] != null)
            {
                Vision[1].MouseDown(e.X, e.Y);
            }
        }
        private void cogDS3_MouseDown(object sender, MouseEventArgs e)
        {
            if (cbDist.Checked && Vision[2] != null)
            {
                Vision[2].MouseDown(e.X, e.Y);
            }
        }
        private void cogDS4_MouseDown(object sender, MouseEventArgs e)
        {
            if (cbDist.Checked && Vision[3] != null)
            {
                Vision[3].MouseDown(e.X, e.Y);
            }
        }

        private MapPosition pos;
        private CogImage8Grey cogView8Grey = new CogImage8Grey();

        private void cogDS_MouseMove(object sender, MouseEventArgs e)
        {
            if (functionInterlock() && Vision[0] != null)
            {
                if (cbDist.Checked && Vision[0].bDistDisplay)
                {
                    Vision[0].MouseMove(e.X, e.Y);
                }

                if (cbImageInfo.Checked)
                {
                    if (cogDS.Image != null)
                    {
                        pos = GetMouseMapPosition(e.X, e.Y, cogDS);

                        txtImageX.Text = "X : " + pos.x.ToString("0.000");
                        txtImageY.Text = "Y : " + pos.y.ToString("0.000");
                        if (pos.x <= cogDS.Image.Width &&
                            pos.x >= 0 &&
                            pos.y <= cogDS.Image.Height &&
                            pos.y >= 0)
                        {
                            cogView8Grey = (CogImage8Grey)cogDS.Image;
                            txtImageGray.Text = "Grey : " + cogView8Grey.GetPixel((int)pos.x, (int)pos.y).ToString();
                        }
                    }
                }
            }
        }

        private void cogDS2_MouseMove(object sender, MouseEventArgs e)
        {
            if (functionInterlock() && Vision[1] != null)
            {
                if (cbDist.Checked && Vision[1].bDistDisplay)
                {
                    Vision[1].MouseMove(e.X, e.Y);
                }

                if (cbImageInfo.Checked)
                {
                    if (cogDS2.Image != null)
                    {
                        MapPosition pos;
                        pos = GetMouseMapPosition(e.X, e.Y, cogDS2);
                        txtImageX.Text = "X : " + pos.x.ToString("0.000");
                        txtImageY.Text = "Y : " + pos.y.ToString("0.000");
                        if (pos.x <= cogDS2.Image.Width &&
                            pos.x >= 0 &&
                            pos.y <= cogDS2.Image.Height &&
                            pos.y >= 0)
                        {
                            cogView8Grey = (CogImage8Grey)cogDS2.Image;
                            txtImageGray.Text = "Grey : " + cogView8Grey.GetPixel((int)pos.x, (int)pos.y).ToString();
                        }
                    }
                }
            }
        }
        private void cogDS3_MouseMove(object sender, MouseEventArgs e)
        {
            if (functionInterlock() && Vision[2] != null)
            {
                if (cbDist.Checked && Vision[2].bDistDisplay)
                {
                    Vision[2].MouseMove(e.X, e.Y);
                }

                if (cbImageInfo.Checked)
                {
                    if (cogDS3.Image != null)
                    {
                        MapPosition pos;
                        pos = GetMouseMapPosition(e.X, e.Y, cogDS3);
                        txtImageX.Text = "X : " + pos.x.ToString("0.000");
                        txtImageY.Text = "Y : " + pos.y.ToString("0.000");
                        if (pos.x <= cogDS3.Image.Width &&
                            pos.x >= 0 &&
                            pos.y <= cogDS3.Image.Height &&
                            pos.y >= 0)
                        {
                            cogView8Grey = (CogImage8Grey)cogDS3.Image;
                            txtImageGray.Text = "Grey : " + cogView8Grey.GetPixel((int)pos.x, (int)pos.y).ToString();
                        }
                    }
                }
            }
        }
        private void cogDS4_MouseMove(object sender, MouseEventArgs e)
        {
            if (functionInterlock() && Vision[3] != null)
            {
                if (cbDist.Checked && Vision[3].bDistDisplay)
                {
                    Vision[3].MouseMove(e.X, e.Y);
                }

                if (cbImageInfo.Checked)
                {
                    if (cogDS4.Image != null)
                    {
                        MapPosition pos;
                        pos = GetMouseMapPosition(e.X, e.Y, cogDS4);
                        txtImageX.Text = "X : " + pos.x.ToString("0.000");
                        txtImageY.Text = "Y : " + pos.y.ToString("0.000");
                        if (pos.x <= cogDS4.Image.Width &&
                            pos.x >= 0 &&
                            pos.y <= cogDS4.Image.Height &&
                            pos.y >= 0)
                        {
                            cogView8Grey = (CogImage8Grey)cogDS4.Image;
                            txtImageGray.Text = "Grey : " + cogView8Grey.GetPixel((int)pos.x, (int)pos.y).ToString();
                        }
                    }
                }
            }
        }

        private void cogDS_MouseUp(object sender, MouseEventArgs e)
        {
            if (functionInterlock() && Vision[0] != null)
            {
                if (cbDist.Checked)
                {
                    Vision[0].MouseUp();
                }
            }
        }

        private void cogDS2_MouseUp(object sender, MouseEventArgs e)
        {
            if (functionInterlock() && Vision[1] != null)
            {
                if (cbDist.Checked)
                {
                    Vision[1].MouseUp();
                }
            }
        }
        private void cogDS3_MouseUp(object sender, MouseEventArgs e)
        {
            if (functionInterlock() && Vision[2] != null)
            {
                if (cbDist.Checked)
                {
                    Vision[2].MouseUp();
                }
            }
        }
        private void cogDS4_MouseUp(object sender, MouseEventArgs e)
        {
            if (functionInterlock() && Vision[3] != null)
            {
                if (cbDist.Checked)
                {
                    Vision[3].MouseUp();
                }
            }
        }

        private void btnZoomAll1_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (var s in Vision)
                {
                    if (s != null)
                    {
                        s.cogDS.AutoFit = true;
                        s.cogDS.Fit(true);
                    }
                }
            }
            catch { }
        }



        private void btnAllAutoFit_Click(object sender, EventArgs e)
        {
            if (Vision[0] == null)
            {
                MessageBox.Show("CamName is Not Selected!");
                return;
            }

            try
            {
                foreach (var s in Vision)
                {
                    if (s != null)
                    {
                        s.cogDS.AutoFit = true;
                        s.cogDS.AutoFitWithGraphics = true;
                        s.cogDS.Fit(true);
                    }

                }
            }
            catch
            { }
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            if (Vision[0] == null)
            {
                MessageBox.Show("CamName is not selected!");
                return;
            }

            try
            {
                foreach (var s in Vision)
                {
                    if (s != null)
                        s.cogDS.Zoom = s.cogDS.Zoom + 0.2;
                }
            }
            catch
            { }
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            if (Vision[0] == null)
            {
                MessageBox.Show("CamName is not selected!");
                return;
            }

            try
            {
                foreach (var s in Vision)
                {
                    if (s != null)
                        s.cogDS.Zoom = s.cogDS.Zoom - 0.2;
                }
            }
            catch { }
        }

        #endregion

        #region Light
        private void tbExposure_MouseUp(object sender, MouseEventArgs e)
        {
            string lcText = (sender as TrackBar).Name;
            for (short i = 0; i < tbExposure.Length; i++)
            {
                if (lcText == tbExposure[i].Name)
                {
                    ExposureChange_tb(i);
                }
            }

        }
        private void tb_MouseUp(object sender, MouseEventArgs e)
        {
            string lcText = (sender as TrackBar).Name;
            for (short i = 0; i < tb.Length; i++)
            {
                if (lcText == tb[i].Name)
                {
                    LightChange_tb(i);
                }
            }
        }

        private void ExposureChange_tb(int i)
        {
            if (Vision[i] != null)
            {
                txtExposure[i].Text = tbExposure[i].Value.ToString();
                Vision[i].setExposure(tbExposure[i].Value, out bool bexplow);
            }
        }
        private void LightChange_tb(int i)
        {
            if (Vision[i] != null)
            {
                txtLight[i].Text = tb[i].Value.ToString();
                Menu.frmAutoMain.SetLight(Vision[i].CFG.Light1Comport, Vision[i].CFG.Light1CH, tb[i].Value, i, Vision[i].CFG.LightType);
            }
        }

        private void txtLight_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    string lcText = (sender as TextBox).Name;
                    for (int i = 0; i < txtLight.Length; i++)
                    {
                        if (lcText == txtLight[i].Name)
                        {
                            if (Vision[i] != null) //아마 null인경우는 없겟지만..
                            {
                                if (int.TryParse(txtLight[i].Text, out int setValue))
                                {
                                    tb[i].Value = setValue;
                                    Menu.frmAutoMain.SetLight(Vision[i].CFG.Light1Comport, Vision[i].CFG.Light1CH, setValue, Vision[i].CFG.Camno, Vision[i].CFG.LightType);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }
        private void txtExposure_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    string lcText = (sender as TextBox).Name;
                    for (int i = 0; i < txtExposure.Length; i++)
                    {
                        if (lcText == txtExposure[i].Name)
                        {
                            if (Vision[i] != null)//아마 null인경우는 없겟지만..
                            {
                                if (int.TryParse(txtExposure[i].Text, out int setValue))
                                {
                                    tbExposure[i].Value = setValue;
                                    Vision[i].setExposure(setValue, out bool bexplow);
                                }
                            }

                        }
                    }
                }
            }
            catch { }
        }
        private void txtContrastTest_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    string lcText = (sender as TextBox).Name;
                    for (short i = 0; i < txtContrast.Length; i++)
                    {
                        if (lcText == txtContrast[i].Name)
                        {
                            if (Vision[i] != null)//아마 null인경우는 없겟지만..
                            {
                                if (double.TryParse(txtContrast[i].Text, out double setValue))
                                {
                                    Vision[i].setContrast(setValue, out bool bcontlow);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void btnExposureDown_Click(object sender, EventArgs e)
        {
            string lcText = (sender as Button).Name;
            for (int i = 0; i < btnExposureDown.Length; i++)
            {
                if (lcText == btnExposureDown[i].Name)
                {
                    if (tbExposure[i].Value > tbExposure[i].Minimum)
                        tbExposure[i].Value--;
                    ExposureChange_tb(i);
                }
            }

        }

        private void btnExposureUp_Click(object sender, EventArgs e)
        {
            string lcText = (sender as Button).Name;
            for (short i = 0; i < btnExposureUp.Length; i++)
            {
                if (lcText == btnExposureUp[i].Name)
                {
                    if (tbExposure[i].Value < tbExposure[i].Maximum)
                        tbExposure[i].Value++;
                    ExposureChange_tb(i);
                }
            }


        }

        private void btnLightDown_Click(object sender, EventArgs e)
        {
            string lcText = (sender as Button).Name;
            for (short i = 0; i < btnLightDown.Length; i++)
            {
                if (lcText == btnLightDown[i].Name)
                {
                    if (tb[i].Value > tb[i].Minimum)
                        tb[i].Value--;
                    LightChange_tb(i);
                }
            }
        }

        private void btnLightUp_Click(object sender, EventArgs e)
        {
            string lcText = (sender as Button).Name;
            for (short i = 0; i < btnLightUp.Length; i++)
            {
                if (lcText == btnLightUp[i].Name)
                {
                    if (tb[i].Value < tb[i].Maximum)
                        tb[i].Value++;
                    LightChange_tb(i);
                }
            }
        }

        #endregion

        private void cbDist_CheckedChanged(object sender, EventArgs e)
        {
            foreach (var s in Vision)
            {
                if (s != null) s.DistClear();
            }
        }

        private void btnPattern_Click(object sender, EventArgs e)
        {
            if (Menu.frmlogin.LogInCheck())
            {
                //pcy190617 버튼누를때 라이브상태면 끄도록함..
                Live(false);
                frmPattern Regist = new frmPattern();
                Regist.RegistCamSelect(cbCamList.SelectedIndex);
                Regist.ShowDialog();

                SetInitRecipeCamSelect(cbCamList.SelectedIndex);
            }
            else
            {
                MessageBox.Show("Confrm LoginUser");
            }
        }

        private void btnCalStop_Click(object sender, EventArgs e)
        {
            if (Vision[0] == null)
            {
                MessageBox.Show("CamName is not selected!");
                return;
            }

            if (Menu.frmlogin.LogInCheck())
            {
                frmMsg.Visible = false;
                tmrCal.Enabled = false;
                Vision[0].CalStep = 0;

                for (int i = 0; i < CONST.bPCCalReq.Length; i++) CONST.bPCCalReq[i] = false;
            }
            else
            {
                MessageBox.Show("Confrm LoginUser");
            }
        }

        private void TraceDataList(bool bDelete = false)
        {
            cboTrace.Items.Clear();
            DirectoryInfo RawTempDirectory = new DirectoryInfo(CONST.cTracePath);
            if (!Directory.Exists(CONST.cTracePath)) Directory.CreateDirectory(CONST.cTracePath);
            FileInfo[] RawTempFile = RawTempDirectory.GetFiles("*.csv");
            for (int i = 0; i < RawTempFile.Length; i++)
            {
                string[] ListName = RawTempFile[i].ToString().Split('_');
                if ((cboTrace.Text.Trim() == ListName[0]) && bDelete)
                {
                    RawTempFile[i].Delete();
                }
                else
                {
                    cboTrace.Items.Add(ListName[0]);
                }
            }
        }

        private void TraceDataRead(string sName)
        {
            string[] FileRead;
            string[] Split;
            bool bDataCheck = false;

            string[] file = Directory.GetFiles(CONST.cTracePath);
            for (int i = 0; i < file.Length; i++)
            {
                string fileName = Path.GetFileName(file[i]);

                string[] sTemp = fileName.Split(new char[] { '_' }); // 수정. filename 과 동일 data만 갖고 오게.

                int ArmNo = int.Parse(sTemp[0].Trim());

                double TracePoint = double.Parse(sTemp[1]);
                double TracePointHalf = TracePoint / 2 - 1;

                int TPoint = Convert.ToInt32(TracePoint) - 1;
                int TPointHalf = Convert.ToInt32(TracePointHalf);

                if ((sTemp[0]).Trim() == sName.Trim())
                //if (file[i].IndexOf(sName) > 0)
                {
                    try
                    {
                        FileRead = File.ReadAllLines(file[i], Encoding.Default);
                        for (int j = 0; j < FileRead.Length; j++)
                        {
                            Split = FileRead[j].Split(',');

                            SData[CONST.AxisR].Value[j] = double.Parse(Split[0]);
                            SData[CONST.AxisY].Value[j] = double.Parse(Split[1]);
                            SData[CONST.AxisZ].Value[j] = double.Parse(Split[2]);

                            CONST.m_dMainTraceT[j] = (double.Parse(Split[0]));
                            CONST.m_dMainTraceY[j] = (double.Parse(Split[1]));
                            CONST.m_dMainTraceZ[j] = (double.Parse(Split[2]));

                            if (CONST.m_dMainTraceY[j] < 0)
                                bDataCheck = true;
                            if (CONST.m_dMainTraceZ[j] < 0)
                                bDataCheck = true;
                            if (CONST.m_dMainTraceT[j] < 0)
                                bDataCheck = true;
                        }

                        if (bDataCheck)
                            MessageBox.Show("제품 궤적 Data Check Please! (Y Axis Data > 0, Z Axis Data > 0, T Axis Data > 0)");

                        try
                        {
                            txt180degOffsetY.Text = "0";
                            txt180degOffsetZ.Text = "0";
                            txt90degX.Text = CONST.m_dMainTraceY[TPointHalf].ToString("0.0000");
                            txt90degY.Text = CONST.m_dMainTraceZ[TPointHalf].ToString("0.0000");
                            txt180degX.Text = CONST.m_dMainTraceY[TPoint].ToString("0.0000");
                            txt180degY.Text = CONST.m_dMainTraceZ[TPoint].ToString("0.0000");
                        }
                        catch { }
                        break;
                    }
                    catch (Exception EX)
                    {
                        cLog.ExceptionLogSave("TraceDataRead" + "," + EX.GetType().Name + "," + EX.Message);
                    }
                }
            }
        }
        private bool CalcfactorAndView()
        {
            //고객사 궤적 역산 //미완
            //90도 offset 무조건 0이라고 가정
            //동심도 offset 무조건 0이라고 가정
            int point = 1;
            //int halfpoint;
            double radius = 0;
            double lastZ = 0;
            double lastYmin = 9999;
            double lastYmax = -9999;
            double lastYLength = 0;

            for (int i = 0; i < 100; i++)
            {
                //90도에 가장 근접한 값을 반지름으로 생각함.
                if (CONST.m_dMainTraceT[i] <= 90)
                {
                    radius = traceZ[i];
                }
                if (i > 0) //0만제외
                {
                    if (CONST.m_dMainTraceT[i - 1] < CONST.m_dMainTraceT[i])
                    {
                        point++;
                        lastZ = traceZ[i];
                    }
                    else //180도
                    {
                        //마지막 전진거리 계산
                        if (lastYmin > traceY[i - 1]) lastYmin = traceY[i - 1];
                        if (lastYmax < traceY[i - 1]) lastYmax = traceY[i - 1];
                    }
                }
            }
            lastYLength = lastYmax - lastYmin; //미완

            txtTracePoint.Text = point.ToString("0");
            txtRadiusOfRotation.Text = radius.ToString("0.0000");
            txt180degOffsetZ.Text = lastZ.ToString("0.0000");
            txtTrace20PointPosY.Text = lastYLength.ToString("0.0000");
            txtCenterY.Text = "0";
            txtCenterZ.Text = "0";
            txt90degOffsetY.Text = "0";
            txt90degOffsetZ.Text = "0";
            txt180degOffsetY.Text = "0";
            txtStartPosY.Text = "0";
            txtStartPosZ.Text = "0";

            return true;
        }
        private void MotionTraceDataRead(int nBendingArm, string sName = null)
        {
            string[] FileRead = new string[0];
            string[] Split;
            string[] sTemp = new string[0];
            bool btryRead = false;
            try
            {
                if (sName != null)
                {
                    FileRead = File.ReadAllLines(sName, Encoding.Default);
                    btryRead = true;
                    string fileName = Path.GetFileName(sName);

                    sTemp = fileName.Split(new char[] { '_' }); // 수정. filename 과 동일 data만 갖고 오게.
                }
                else
                {
                    string[] file = Directory.GetFiles(CONST.cMotionTracePath + "\\" + CONST.RunRecipe.RecipeName);
                    for (int i = 0; i < file.Length; i++)
                    {
                        string fileName = Path.GetFileName(file[i]);

                        sTemp = fileName.Split(new char[] { '_' }); // 수정. filename 과 동일 data만 갖고 오게.

                        if (sTemp[0] == (nBendingArm + 1).ToString("0"))
                        {
                            btryRead = true;
                            FileRead = File.ReadAllLines(file[i], Encoding.Default);
                        }
                    }
                }


                if (btryRead)
                {
                    try
                    {
                        for (int j = 0; j < FileRead.Length; j++)
                        {
                            Split = FileRead[j].Split(',');

                            if (dataGridView_MotionTrace.Rows.Count <= 100)
                            {
                                dataGridView_MotionTrace.Rows.Add();
                                dataGridView_MotionTrace.Rows[j].HeaderCell.Value = (j + 1).ToString("0");
                            }
                            //csh 20170321
                            //210217 cjm YValue Abs Change
                            CONST.m_dTraceY[nBendingArm, j] = Math.Abs(double.Parse(Split[0]));
                            CONST.m_dTraceZ[nBendingArm, j] = (double.Parse(Split[1]));
                            CONST.m_dTraceT[nBendingArm, j] = (double.Parse(Split[2]));

                            CONST.m_dMainTraceY[j] = double.Parse(Split[0]);
                            CONST.m_dMainTraceZ[j] = double.Parse(Split[1]);
                            CONST.m_dMainTraceT[j] = double.Parse(Split[2]);

                            dataGridView_MotionTrace.Rows[j].Cells[0].Value = CONST.m_dTraceY[nBendingArm, j].ToString("0.00000");
                            dataGridView_MotionTrace.Rows[j].Cells[1].Value = CONST.m_dTraceZ[nBendingArm, j].ToString("0.00000");
                            dataGridView_MotionTrace.Rows[j].Cells[2].Value = CONST.m_dTraceT[nBendingArm, j].ToString("0.00000");

                            if (Split.Length > 3)
                            {
                                traceY[j] = double.Parse(Split[3]);
                                traceZ[j] = double.Parse(Split[4]);

                            }
                            else
                            {
                                traceY[j] = CONST.m_dMainTraceY[j];
                                traceZ[j] = CONST.m_dMainTraceZ[j];
                            }
                        }

                        try
                        {
                            if (sTemp.Length > 13)
                            {
                                txtTracePoint.Text = sTemp[1];
                                txtRadiusOfRotation.Text = sTemp[2];
                                txtTrace20PointPosY.Text = sTemp[3];
                                txt90degOffsetY.Text = sTemp[4];
                                txt90degOffsetZ.Text = sTemp[5];
                                txt180degOffsetY.Text = sTemp[6];
                                txt180degOffsetZ.Text = sTemp[7];
                                txt90degX.Text = sTemp[8];
                                txt90degY.Text = sTemp[9];
                                txt180degX.Text = sTemp[10];
                                txt180degY.Text = sTemp[11];
                                txtCenterY.Text = sTemp[12];
                                txtCenterZ.Text = sTemp[13];

                                bool.TryParse(sTemp[14], out bool b);
                                if (b)
                                {
                                    btnAutoCreate.Visible = true;
                                    btnOffsetCreate.Visible = false;
                                }
                            }
                            else
                            {
                                btnAutoCreate.Visible = false;
                                btnOffsetCreate.Visible = true;
                            }
                        }
                        catch
                        {
                            btnAutoCreate.Visible = false;
                            btnOffsetCreate.Visible = true;
                        }
                    }
                    catch (Exception EX)
                    {
                        cLog.ExceptionLogSave("TraceDataRead" + "," + EX.GetType().Name + "," + EX.Message);
                    }
                }
            }
            catch { }
        }

        private void MotionTraceDataRead2(int nBendingArm, string sName)
        {
            string[] FileRead;
            string[] Split;
            try
            {
                FileRead = File.ReadAllLines(sName, Encoding.Default);
                for (int j = 0; j < FileRead.Length; j++)
                {
                    Split = FileRead[j].Split(',');

                    if (dataGridView_MotionTrace.Rows.Count <= 100)
                    {
                        dataGridView_MotionTrace.Rows.Add();
                        dataGridView_MotionTrace.Rows[j].HeaderCell.Value = (j + 1).ToString("0");
                    }
                    CONST.m_dTraceY[nBendingArm, j] = (double.Parse(Split[0]));
                    CONST.m_dTraceZ[nBendingArm, j] = (double.Parse(Split[1]));
                    CONST.m_dTraceT[nBendingArm, j] = (double.Parse(Split[2]));

                    CONST.m_dMainTraceY[j] = double.Parse(Split[0]);
                    CONST.m_dMainTraceZ[j] = double.Parse(Split[1]);
                    CONST.m_dMainTraceT[j] = double.Parse(Split[2]);

                    dataGridView_MotionTrace.Rows[j].Cells[0].Value = CONST.m_dTraceY[nBendingArm, j].ToString("0.00000");
                    dataGridView_MotionTrace.Rows[j].Cells[1].Value = CONST.m_dTraceZ[nBendingArm, j].ToString("0.00000");
                    dataGridView_MotionTrace.Rows[j].Cells[2].Value = CONST.m_dTraceT[nBendingArm, j].ToString("0.00000");
                }
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("TraceDataRead2" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }

        private void TraceFileRead(int nBendingArm, string sFileName)
        {
            string[] FileRead; //100줄..
            string[] Split; //각줄에서 항목 하나씩..
            try
            {
                try
                {
                    FileRead = File.ReadAllLines(sFileName, Encoding.Default);
                    for (int j = 0; j < FileRead.Length; j++)
                    {
                        Split = FileRead[j].Split(',');

                        if (dataGridView_MotionTrace.Rows.Count <= 100)
                        {
                            dataGridView_MotionTrace.Rows.Add();
                            dataGridView_MotionTrace.Rows[j].HeaderCell.Value = (j + 1).ToString("0");
                        }
                        //csh 20170321
                        CONST.m_dTraceY[nBendingArm, j] = (double.Parse(Split[0]));
                        CONST.m_dTraceZ[nBendingArm, j] = (double.Parse(Split[1]));
                        CONST.m_dTraceT[nBendingArm, j] = (double.Parse(Split[2]));

                        CONST.m_dMainTraceY[j] = double.Parse(Split[0]);
                        CONST.m_dMainTraceZ[j] = double.Parse(Split[1]);
                        CONST.m_dMainTraceT[j] = double.Parse(Split[2]);

                        dataGridView_MotionTrace.Rows[j].Cells[0].Value = CONST.m_dTraceY[nBendingArm, j].ToString("0.00000");
                        dataGridView_MotionTrace.Rows[j].Cells[1].Value = CONST.m_dTraceZ[nBendingArm, j].ToString("0.00000");
                        dataGridView_MotionTrace.Rows[j].Cells[2].Value = CONST.m_dTraceT[nBendingArm, j].ToString("0.00000");

                        if (Split.Length > 3)
                        {
                            traceY[j] = double.Parse(Split[3]);
                            traceZ[j] = double.Parse(Split[4]);
                        }
                    }
                }
                catch (Exception EX)
                {
                    cLog.ExceptionLogSave("TraceDataRead" + "," + EX.GetType().Name + "," + EX.Message);
                }
            }
            catch { }
        }

        private void TeachingDataSave(string sName, int nArmNo)
        {
            try
            {
                System.IO.DirectoryInfo di;
                string sPath = CONST.cMotionTracePath + "\\" + CONST.RunRecipe.RecipeName;

                string strTemp = "";
                if (System.IO.Directory.Exists(sPath))
                {
                    di = new System.IO.DirectoryInfo(sPath);
                    foreach (System.IO.FileInfo fi in di.GetFiles(@"*.csv"))
                    {
                        strTemp = fi.Name;

                        string a = strTemp.Substring(0, 1);

                        if (strTemp.Substring(0, 1) == (nArmNo + 1).ToString("0"))
                        {
                            fi.Delete();
                        }
                    }
                }

                double[] dYOri = new double[100];
                double[] dZOri = new double[100];
                double[] dThetaOri = new double[100];

                for (int i = 0; i < 100; i++)
                {
                    dYOri[i] = traceY[i];
                    dZOri[i] = traceZ[i];
                    dThetaOri[i] = CONST.m_dTraceT[nArmNo, i];

                    SData[CONST.AxisY].Value[i] = CONST.m_dTraceY[nArmNo, i];
                    SData[CONST.AxisZ].Value[i] = CONST.m_dTraceZ[nArmNo, i];
                    SData[CONST.AxisR].Value[i] = CONST.m_dTraceT[nArmNo, i];
                }
                bool bOffset = false;
                if (!teachZ.SequenceEqual(traceZ)) bOffset = true;              
                string sFile = (nArmNo + 1).ToString("0") + "_" + txtTracePoint.Text + "_" + txtRadiusOfRotation.Text + "_" + txtTrace20PointPosY.Text
                    + "_" + txt90degOffsetY.Text + "_" + txt90degOffsetZ.Text + "_" + txt180degOffsetY.Text + "_" + txt180degOffsetZ.Text
                    + "_" + txt90degX.Text + "_" + txt90degY.Text + "_" + txt180degX.Text + "_" + txt180degY.Text
                    + "_" + txtCenterY.Text + "_" + txtCenterZ.Text + "_" + bOffset.ToString() + "_.csv";

                if (!Directory.Exists(CONST.cMotionTracePath + "\\" + CONST.RunRecipe.RecipeName))
                    Directory.CreateDirectory(CONST.cMotionTracePath + "\\" + CONST.RunRecipe.RecipeName);

                StreamWriter FileInfo = new StreamWriter(Path.Combine(CONST.cMotionTracePath + "\\" + CONST.RunRecipe.RecipeName, sFile), false, Encoding.Default);
                for (int i = 0; i < SData[CONST.AxisY].Value.Length; i++)
                {
                    FileInfo.WriteLine(String.Join(",", SData[CONST.AxisY].Value[i], SData[CONST.AxisZ].Value[i], SData[CONST.AxisR].Value[i], dYOri[i], dZOri[i], dThetaOri[i]));
                }

                CONST.TracePoint[nArmNo] = Convert.ToDouble(txtTracePoint.Text);
                CONST.RadiusOfRotation[nArmNo] = Convert.ToDouble(txtRadiusOfRotation.Text);
                CONST.endPointZ[nArmNo] = Convert.ToDouble(txt180degY.Text);

                FileInfo.Close();
                TraceDataList();
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("TeachingDataSave" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }

        private void btnTeachingSave_Click(object sender, EventArgs e)
        {
            if (Menu.frmlogin.LogInCheck())
            {
                if (MessageBox.Show("Bending 100 Point Data Save", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    //확인 여기 plc recipe와 궤적파일명 비교해서 안맞으면 확인창 띄우기 필요. (오퍼레이터 실수방지) sdt multi 고객사 요청
                    //Recipe Trace No와 궤적 data가 맞지 않아도 모든 Bending 100Point를 PLC로 전송
                    if (cbBending.SelectedIndex == 0)
                    {
                        CONST.plcTrace1Write = true;
                        TeachingDataSave(cboTrace.Text.Trim(), cbBending.SelectedIndex);
                    }
                    else if (cbBending.SelectedIndex == 1)
                    {
                        CONST.plcTrace2Write = true;
                        TeachingDataSave(cboTrace.Text.Trim(), cbBending.SelectedIndex);
                    }
                    else if (cbBending.SelectedIndex == 2)
                    {
                        CONST.plcTrace3Write = true;
                        TeachingDataSave(cboTrace.Text.Trim(), cbBending.SelectedIndex);
                    }
                    else
                        MessageBox.Show("Not Select Bending No");
                }
                //}
            }
            else
            {
                MessageBox.Show("Confrm LoginUser");
            }
        }

        private void btnListDelete_Click(object sender, EventArgs e)
        {
            if (Menu.frmlogin.LogInCheck())
            {
                if (cboTrace.Text == "")
                {
                    MessageBox.Show("Trace Base Data Select Fail");
                    return;
                }

                if (MessageBox.Show("Are you sure?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    TraceDataList(true);
                }
            }
            else
            {
                MessageBox.Show("confrm LoginUser");
            }
        }

        //옵셋먹기전
        private double[] traceY = new double[100];
        private double[] traceZ = new double[100];

        //옵셋먹은것
        private double[] teachY = new double[100];
        private double[] teachZ = new double[100];

        public void BendingTraceAutoCreate(int ipoint, double dRadiusOfRotation, double dLastYOffset, double d90OffsetY, double d90OffsetZ, double d180OffsetY, double d180OffsetZ, double dStartPosY, double dStartPosZ)
        {
            bool bDataCheck = false;
            double[] dY = new double[100];
            double[] dZ = new double[100];
            double[] dTheta = new double[100];
            double dCenterY = double.Parse(txtCenterY.Text); //동심도추가
            double dCenterZ = double.Parse(txtCenterZ.Text);

            try
            {
                bDataCheck = Menu.rsAlign.traceCreate(ipoint, dRadiusOfRotation, dLastYOffset, (-1) * d90OffsetY, d90OffsetZ, d180OffsetY, d180OffsetZ, ref dY, ref dZ, ref dTheta);

                //pcy201229 동심도 옵셋 적용된 변수 추가
                double[] dYOffset = new double[100];
                double[] dZOffset = new double[100];
                double[] dThetaOffset = new double[100];

                for (int i = 0; i < 100; i++)
                {
                    dYOffset[i] = (dY[i] + dCenterY * Math.Sin(dTheta[i] * CONST.rad) + dCenterZ);
                    dZOffset[i] = (dZ[i] - dCenterY * Math.Cos(dTheta[i] * CONST.rad) + dCenterY);
                }
                int ipoint2 = ipoint / 2;
                if (ipoint % 2 == 0)
                {
                    ipoint2 -= 1;
                }

                if (!bDataCheck)
                {
                    Menu.frmAutoMain.IF.SendData("NG");
                    return;
                }

                int nBendingArm = 0;

                if (cbBending.SelectedIndex == -1)
                    nBendingArm = 0;
                else
                    nBendingArm = cbBending.SelectedIndex;

                for (int i = 0; i < 100; i++)
                {
                    teachY[i] = dYOffset[i];
                    teachZ[i] = dZOffset[i];

                    CONST.m_dTraceY[nBendingArm, i] = dYOffset[i];
                    CONST.m_dTraceZ[nBendingArm, i] = dZOffset[i];
                    CONST.m_dTraceT[nBendingArm, i] = dTheta[i];
                    traceY[i] = dY[i] + dStartPosY;
                    traceZ[i] = dZ[i] + dStartPosZ;
                    if (dataGridView_MotionTrace.Rows.Count <= 100)
                    {
                        dataGridView_MotionTrace.Rows.Add();
                        dataGridView_MotionTrace.Rows[i].HeaderCell.Value = (i + 1).ToString("0");
                    }
                    dataGridView_MotionTrace.Rows[i].Cells[0].Value = CONST.m_dTraceY[nBendingArm, i].ToString("0.00000");
                    dataGridView_MotionTrace.Rows[i].Cells[1].Value = CONST.m_dTraceZ[nBendingArm, i].ToString("0.00000");
                    dataGridView_MotionTrace.Rows[i].Cells[2].Value = CONST.m_dTraceT[nBendingArm, i].ToString("0.00000");
                }
                lblTranceLastPointAdd.Text = "Trace " + (Int32.Parse(txtTracePoint.Text) + 1).ToString() + "Point Position Y(mm)";
                //pcy190518
                txt90degX.Text = CONST.m_dTraceY[nBendingArm, Convert.ToInt32(ipoint2)].ToString("0.000");
                txt90degY.Text = CONST.m_dTraceZ[nBendingArm, Convert.ToInt32(ipoint2)].ToString("0.000");
                txt180degX.Text = CONST.m_dTraceY[nBendingArm, Convert.ToInt32(ipoint - 1)].ToString("0.000");
                txt180degY.Text = CONST.m_dTraceZ[nBendingArm, Convert.ToInt32(ipoint - 1)].ToString("0.000");

                drawTrace();
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("btnAutoCreate_Click" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }
        private void btnOffsetCreate_Click(object sender, EventArgs e)
        {
            double dCenterY = double.Parse(txtCenterY.Text); //동심도추가
            double dCenterZ = double.Parse(txtCenterZ.Text);

            int nBendingArm = 0;

            if (cbBending.SelectedIndex == -1)
                nBendingArm = 0;
            else
                nBendingArm = cbBending.SelectedIndex;

            try
            {
                for (int i = 0; i < 100; i++)
                {
                    teachY[i] = (traceY[i] + dCenterZ * Math.Sin(CONST.m_dMainTraceT[i] * CONST.rad) + dCenterY);
                    teachZ[i] = (traceZ[i] - dCenterZ * Math.Cos(CONST.m_dMainTraceT[i] * CONST.rad) + dCenterZ);
                    CONST.m_dTraceY[nBendingArm, i] = teachY[i];
                    CONST.m_dTraceZ[nBendingArm, i] = teachZ[i];
                    CONST.m_dTraceT[nBendingArm, i] = CONST.m_dMainTraceT[i];
                }
                drawTrace();
            }
            catch
            {

            }
        }
        private void btnAutoCreate_Click(object sender, EventArgs e)
        {
            bool bDataCheck = false;

            if (!Menu.frmlogin.LogInCheck())
            {
                MessageBox.Show("confrm LoginUser");
                return;
            }
            if (cbBending.SelectedIndex == -1)
            {
                MessageBox.Show("Select Bending Number");
                return;
            }
            try
            {
                int ipoint = int.Parse(txtTracePoint.Text); //포인트 개수
                double dCenterY = double.Parse(txtCenterY.Text); //동심도추가
                double dCenterZ = double.Parse(txtCenterZ.Text) / 2;
                double dRadiusOfRotation = double.Parse(txtRadiusOfRotation.Text); //반지름
                double dLastYOffset = double.Parse(txtTrace20PointPosY.Text); //마지막 Y이동 추가량.
                double d90OffsetY = double.Parse(txt90degOffsetY.Text); //0~90 offsetY
                double d90OffsetZ = double.Parse(txt90degOffsetZ.Text); //0~90 offsetZ
                double d180OffsetY = double.Parse(txt180degOffsetY.Text);// + dCenterY; //90~180 offsetY
                double d180OffsetZ = double.Parse(txt180degOffsetZ.Text);// + dCenterZ; //90~180 offsetZ
                double dStartPosY = double.Parse(txtStartPosY.Text);
                double dStartPosZ = double.Parse(txtStartPosZ.Text);

                double[] dY = new double[100];
                double[] dZ = new double[100];
                double[] dTheta = new double[100];

                //y에 -1곱해줘야 알아보기 쉽게생성됨
                bDataCheck = Menu.rsAlign.traceCreate(ipoint, dRadiusOfRotation, dLastYOffset, (-1) * d90OffsetY, d90OffsetZ, d180OffsetY, d180OffsetZ, ref dY, ref dZ, ref dTheta);
                //pcy201229 동심도 옵셋 적용
                for (int i = 0; i < 100; i++)
                {
                    teachY[i] = (dY[i] + dCenterZ * Math.Sin(dTheta[i] * CONST.rad) + dCenterY);
                    teachZ[i] = (dZ[i] - dCenterZ * Math.Cos(dTheta[i] * CONST.rad) + dCenterZ);
                }
                int ipoint2 = ipoint / 2;
                if (ipoint % 2 == 0)
                {
                    ipoint2 -= 1;
                }

                if (!bDataCheck)
                {
                    MessageBox.Show("제품 궤적 Data Check Please! (Y Axis Data > 0, Z Axis Data > 0, T Axis Data > 0)");
                    return;
                }

                int nBendingArm = 0;

                if (cbBending.SelectedIndex == -1)
                    nBendingArm = 0;
                else
                    nBendingArm = cbBending.SelectedIndex;

                for (int i = 0; i < 100; i++)
                {
                    CONST.m_dTraceY[nBendingArm, i] = teachY[i];
                    CONST.m_dTraceZ[nBendingArm, i] = teachZ[i];
                    CONST.m_dTraceT[nBendingArm, i] = dTheta[i];
                    traceY[i] = dY[i];// + dStartPosY;
                    traceZ[i] = dZ[i];// + dStartPosZ;
                    if (dataGridView_MotionTrace.Rows.Count <= 100)
                    {
                        dataGridView_MotionTrace.Rows.Add();
                        dataGridView_MotionTrace.Rows[i].HeaderCell.Value = (i + 1).ToString("0");
                    }
                    dataGridView_MotionTrace.Rows[i].Cells[0].Value = CONST.m_dTraceY[nBendingArm, i].ToString("0.00000");
                    dataGridView_MotionTrace.Rows[i].Cells[1].Value = CONST.m_dTraceZ[nBendingArm, i].ToString("0.00000");
                    dataGridView_MotionTrace.Rows[i].Cells[2].Value = CONST.m_dTraceT[nBendingArm, i].ToString("0.00000");
                }
                lblTranceLastPointAdd.Text = "Trace " + (Int32.Parse(txtTracePoint.Text) + 1).ToString() + "Point Position Y(mm)";
                //pcy190518
                txt90degX.Text = CONST.m_dTraceY[nBendingArm, Convert.ToInt32(ipoint2)].ToString("0.000");
                txt90degY.Text = CONST.m_dTraceZ[nBendingArm, Convert.ToInt32(ipoint2)].ToString("0.000");
                txt180degX.Text = CONST.m_dTraceY[nBendingArm, Convert.ToInt32(ipoint - 1)].ToString("0.000");
                txt180degY.Text = CONST.m_dTraceZ[nBendingArm, Convert.ToInt32(ipoint - 1)].ToString("0.000");

                drawTrace();
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("btnAutoCreate_Click" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }

        private double dMaxPoint = 0;
        private double dMinPoint = 0;

        private double dXMax = -9999;
        private double dXMin = 9999;

        private void drawTrace()
        {
            try
            {
                ctTrace.Series.Clear();
                ctTrace.Series.Add("Teach Point");
                ctTrace.Series.Add("Bending Arm Trace");
                dMaxPoint = traceZ[0];
                dMinPoint = traceZ[0];

                ctTrace.Series[0].Points.AddXY(0, 0);
                ctTrace.Series[1].Points.AddXY(0, 0);
                if (dMaxPoint < 0) dMaxPoint = 0;
                if (dMinPoint > 0) dMinPoint = 0;
                if (dMaxPoint < 0) dMaxPoint = 0;
                if (dMinPoint > 0) dMinPoint = 0;
                if (dXMin > 0) dXMin = 0;
                if (dXMax < 0) dXMax = 0;
                if (dXMin > 0) dXMin = 0;
                if (dXMax < 0) dXMax = 0;

                for (int i = 0; i < traceY.Length; i++)
                {
                    ctTrace.Series[0].Points.AddXY(traceY[i], traceZ[i]);
                    ctTrace.Series[1].Points.AddXY(teachY[i], teachZ[i]);
                    if (dMaxPoint < teachZ[i]) dMaxPoint = teachZ[i];
                    if (dMinPoint > teachZ[i]) dMinPoint = teachZ[i];
                    if (dMaxPoint < traceZ[i]) dMaxPoint = traceZ[i];
                    if (dMinPoint > traceZ[i]) dMinPoint = traceZ[i];
                    if (dXMin > teachY[i]) dXMin = teachY[i];
                    if (dXMax < teachY[i]) dXMax = teachY[i];
                    if (dXMin > traceY[i]) dXMin = traceY[i];
                    if (dXMax < traceY[i]) dXMax = traceY[i];

                }

                ctTrace.Series[0].Color = Color.DarkGreen;
                ctTrace.Series[0].BorderWidth = 1;
                ctTrace.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
                ctTrace.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.Blue;
                ctTrace.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.DashDot;
                ctTrace.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.Blue;
                ctTrace.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.DashDot;
                //ctTrace.ChartAreas[0].AxisY.Minimum = dY[0];

                ctTrace.Series[1].Color = Color.Red;
                ctTrace.Series[1].BorderWidth = 1;
                ctTrace.Series[1].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                ctTrace.ChartAreas[0].AxisY.Minimum = dMinPoint;
                ctTrace.ChartAreas[0].AxisY.Maximum = dMaxPoint + 2;

                ctTrace.ChartAreas[0].AxisX.IsReversed = true;
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("drawTrace" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }
        private void TeachtoGraph()
        {
            try 
            {           
                for (int i = 0; i < 100; i++)
                {
                    teachY[i] = CONST.m_dMainTraceY[i];
                    teachZ[i] = CONST.m_dMainTraceZ[i];
                }
                drawTrace();
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("TeachtoGraph" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }


        private void btnRead_Click(object sender, EventArgs e)
        {
            if (Menu.frmlogin.LogInCheck())
            {
                if (cboTrace.Text == "")
                {
                    MessageBox.Show("Trace Base Data Select Fail");
                    return;
                }

                TraceDataRead(cboTrace.Text.Trim());
                TeachtoGraph();
            }
            else
            {
                MessageBox.Show("confrm LoginUser");
            }
        }

        private void SaveREVFile(string _source, string _dest)
        {
            try
            {
                if (!Directory.Exists("C:\\EQData\\Config\\ModelData\\" + _dest))
                    Directory.CreateDirectory("C:\\EQData\\Config\\ModelData\\" + _dest);

                string[] files = Directory.GetFiles("C:\\EQData\\Config\\ModelData\\" + _source);
                string[] folders = Directory.GetDirectories("C:\\EQData\\Config\\ModelData\\" + _source);

                foreach (string file in files)
                {
                    string name = Path.GetFileName(file);
                    string dest = Path.Combine("C:\\EQData\\Config\\ModelData\\" + _dest, name);

                    File.Copy(file, dest, true);
                }
            }
            catch
            {
            }
        }

        //2018.07.11 Recipe Change File Copy khs
        public void RecipChangeDataCopy()
        {
            string CamName;
            string SourceFolder;
            string destFolder;
            string strPath;
            //Pattern Copy
            for (int i = 0; i < CONST.CAMCnt; i++)
            {
                CamName = cbCamList.Items[i].ToString();

                SourceFolder = Path.Combine(CONST.cVisionPath, CamName, CONST.stringRcp, CONST.RunRecipe.OldRecipeName);
                destFolder = Path.Combine(CONST.cVisionPath, CamName, CONST.stringRcp, CONST.RunRecipe.RecipeName);
                if (CamName != "Not Use") FileCopy(SourceFolder, destFolder, true);
            }

            //Setting Value Copy
            strPath = CONST.Folder + "EQData\\Config\\ModelData\\";

            SourceFolder = Path.Combine(strPath + CONST.RunRecipe.OldRecipeName);
            destFolder = Path.Combine(strPath + CONST.RunRecipe.RecipeName);

            FileCopy(SourceFolder, destFolder, true);

            //pcy200708 추가 Calibration Value Copy
            strPath = CONST.Folder + "EQData\\Calibration\\Data\\";

            SourceFolder = Path.Combine(strPath + CONST.RunRecipe.OldRecipeName);
            destFolder = Path.Combine(strPath + CONST.RunRecipe.RecipeName);

            FileCopy(SourceFolder, destFolder, true);

            if (CONST.PCNo == 3 || CONST.PCNo == 7)
            {
                //100Point Data Copy
                strPath = CONST.Folder + "EQData\\Trace\\MotionTrace\\";

                SourceFolder = Path.Combine(strPath + CONST.RunRecipe.OldRecipeName);
                destFolder = Path.Combine(strPath + CONST.RunRecipe.RecipeName);

                FileCopy(SourceFolder, destFolder, true);
            }
        }

        // 해당 폴더 내용 몽땅 복사.
        private void FileCopy(string SourceFolder, string DestFolder, bool overwrite)
        {
            try
            {
                if (Directory.Exists(SourceFolder))
                {
                    if (!Directory.Exists(DestFolder))
                    {
                        Directory.CreateDirectory(DestFolder);

                        string[] files = Directory.GetFiles(SourceFolder);
                        string[] folders = Directory.GetDirectories(SourceFolder);

                        foreach (string file in files)
                        {
                            string name = Path.GetFileName(file);
                            string dest = Path.Combine(DestFolder, name);

                            File.Copy(file, dest, overwrite);
                        }

                        foreach (string folder in folders)
                        {
                            string name = Path.GetFileName(folder);

                            string dest = Path.Combine(DestFolder, name);

                            FileCopy(folder, dest, overwrite);
                        }
                    }
                }
                else
                {
                    //pcy190504 로그추가.
                    //MessageBox.Show("Source Folder or File Not Exists");
                    cLog.ExceptionLogSave("Source Folder or File Not Exists" + " SourceFolder : " + SourceFolder);
                }
            }
            catch (Exception EX)
            {
                MessageBox.Show("FileCopy Fail! -> Check source Folder or Destination Folder, Exception = " + EX.Message);
                cLog.ExceptionLogSave("PatternCopy" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }

        private cs2DAlign.ptXYT ref1Pixel = new cs2DAlign.ptXYT();
        private cs2DAlign.ptXYT mark1Pixel = new cs2DAlign.ptXYT();
        private cs2DAlign.ptXYT ref2Pixel = new cs2DAlign.ptXYT();
        private cs2DAlign.ptXYT mark2Pixel = new cs2DAlign.ptXYT();

        private void btnInspection_Click(object sender, EventArgs e)
        {
            if (Vision[0] == null)
            {
                MessageBox.Show("CamName is not selected!");
                return;
            }

            //pcy190405 Tranfer cal없으니 항상 1로 사용.
            int kind = cbBendSelect.SelectedIndex;
            if (!cbBendSelect.Visible) kind = 1;

            cogDS.InteractiveGraphics.Clear();
            cogDS2.InteractiveGraphics.Clear();

            if (functionInterlock())
            {
                cs2DAlign.ptAlignResult AlignResult = new cs2DAlign.ptAlignResult();
                cs2DAlign.ptXYT Mark1 = new cs2DAlign.ptXYT();
                cs2DAlign.ptXYT Mark2 = new cs2DAlign.ptXYT();

                if (Vision[0].CFG.eCamName == nameof(eCamNO.LoadingPre1) || Vision[0].CFG.eCamName == nameof(eCamNO.LoadingPre2))
                {

                    if (Vision[0].PatternSearchEnum(ref Mark1.X, ref Mark1.Y, ref Mark1.T, ePatternKind.Panel)
                        && Vision[1].PatternSearchEnum(ref Mark2.X, ref Mark2.Y, ref Mark2.T, ePatternKind.Panel))
                    {
                        int calpos = (int)eCalPos.LoadingPre1_1;
                        if (Vision[0].CFG.eCamName == nameof(eCamNO.LoadingPre2)) calpos = (int)eCalPos.LoadingPre2_1;
                        Menu.frmAutoMain.AlignXYT(Vision[0].CFG.Camno, calpos, Mark1, Mark2, ref AlignResult, true);
                        //cs2DAlign.ptAlignResult align = new cs2DAlign.ptAlignResult();
                        txtInspX1.Text = Mark1.X.ToString("0.000");
                        txtInspY1.Text = Mark1.Y.ToString("0.000");
                        txtInspX2.Text = Mark2.X.ToString("0.000");
                        txtInspY2.Text = Mark2.Y.ToString("0.000");

                        listAlignResult.Items.Clear();
                        listAlignResult.Items.Add("Pre Align Value");

                        listAlignResult.Items.Add("X :" + AlignResult.X.ToString("0.000") + ",Y : " + AlignResult.Y.ToString("0.000") + ",T : " + AlignResult.T.ToString("0.000"));
                    }
                    else
                    {
                        MessageBox.Show("Pattern Search Fail!");
                    }
                }
                #region Laser
                else if (Vision[0].CFG.eCamName == nameof(eCamNO.Laser1) || Vision[0].CFG.eCamName == nameof(eCamNO.Laser2))
                {
                    //조명변환이 필요
                    //Vision[0].SetLightExpCont(ePatternKind.Panel);
                    //Vision[0].Capture(false, true, false, true);
                    cs2DAlign.ptXYT ref1 = default(cs2DAlign.ptXYT);
                    bool result1 = false;
                    CogImage8Grey img = null;
                    if (Menu.frmSetting.revData.mLaser.UseImageProcess && Menu.frmSetting.revData.mLaser.refSearch != eRefSearch.Blob)
                    {
                        img = changeImage(Vision[0].CFG.eCamName, (CogImage8Grey)Vision[0].cogDS.Image);
                        cogblobDS.Image = img;
                        cogblobDS.AutoFit = true;
                    }
                    if (Menu.frmSetting.revData.mLaser.refSearch == eRefSearch.Mark)
                    {                                              
                        result1 = Vision[0].PatternSearchEnum(ref ref1.X, ref ref1.Y, ref ref1.T, ePatternKind.Panel, false, false, img);
                    }
                    else if (Menu.frmSetting.revData.mLaser.refSearch == eRefSearch.Line)
                    {
                        result1 = Vision[0].FindMarkingRef(ref ref1, true, img);
                    }
                    else //Blob
                    {
                        int iiTH = 0;
                        CogRectangle region = HistoRegionRead(Vision[0].CFG.eCamName, ref iiTH, eHistogram.RefPoint);
                        result1 = Vision[0].FindMarkingRefBlob(ref ref1, iiTH, region);
                        if (result1)
                        {
                            cogblobDS.Image = Vision[0].blobImage;
                            cogblobDS.AutoFit = true;
                        }
                    }

                    int calpos = (int)eCalPos.Laser1;
                    if (Vision[0].CFG.eCamName == nameof(eCamNO.Laser2)) calpos = (int)eCalPos.Laser2;

                    cs2DAlign.ptAlignResult align = new cs2DAlign.ptAlignResult();

                    Menu.frmAutoMain.AlignXYT(Vision[0].CFG.Camno,calpos, ref1, ref1, ref align);  //확인필요함.....lkw (레이져 헤드가 이동하는것으로...)

                    string str = "";
                    
                    if (result1)
                    {
                        txtX[0].Text = ref1.X.ToString("0.000");
                        txtY[0].Text = ref1.Y.ToString("0.000");
                        cs2DAlign.ptXY diff = new cs2DAlign.ptXY();
                       ePCResult iPCResult = Menu.frmAutoMain.LaserAlignPosCheck(ref1, Vision[0].CFG.Camno, (int)calpos, ref str, ref diff);                      
                    }

                    //Detach 검사추가...

                    cs2DAlign.ptXY[] codePoint = new cs2DAlign.ptXY[4];
                    int iTH = 0;
                    CogRectangle rect = HistoRegionRead(Vision[0].CFG.eCamName, ref iTH, eHistogram.MCRRegion);
                    cs2DAlign.ptXXYY dist1 = new cs2DAlign.ptXXYY();
                    cs2DAlign.ptXXYY dist2 = new cs2DAlign.ptXXYY();
                    cs2DAlign.ptXXYY dist = new cs2DAlign.ptXXYY();
                    string apnCode = "";
                    //int ileft = 1;
                    //int iright = 3;
                    //if (Menu.frmSetting.revData.mLaser.MCRRight)
                    //{
                       int ileft = 3;  //left down code
                       int iright = 1; //right up code
                                       //}
                    if (Menu.frmSetting.revData.mLaser.MCRRight != Menu.frmSetting.revData.mLaser.MCRUp)
                    {
                        ileft = 0; //left up code
                        iright = 2; //right down code
                    }
                    if (Vision[0].readID(ref apnCode, ref codePoint, Menu.frmSetting.revData.mLaser.MCRSearchKind, rect))
                    {
                        if (Vision[0].CFG.eCamName == nameof(eCamNO.Laser1))
                        {
                            dist1 = Menu.frmAutoMain.MarktoCodeDist(Vision[0].CFG.Camno, (int)eCalPos.Laser1, ref1, codePoint[ileft]);
                            dist2 = Menu.frmAutoMain.MarktoCodeDist(Vision[0].CFG.Camno, (int)eCalPos.Laser1, ref1, codePoint[iright]);
                        }
                        else
                        {
                            dist1 = Menu.frmAutoMain.MarktoCodeDist(Vision[0].CFG.Camno, (int)eCalPos.Laser2, ref1, codePoint[ileft]);
                            dist2 = Menu.frmAutoMain.MarktoCodeDist(Vision[0].CFG.Camno, (int)eCalPos.Laser2, ref1, codePoint[iright]);

                        }

                        if (Menu.frmSetting.revData.mLaser.inspKind == eInspKind.Mark)
                        {
                            dist.X1 = Math.Abs(dist1.X2);
                            dist.Y1 = Math.Abs(dist1.Y2);
                            dist.X2 = Math.Abs(dist2.X2);
                            dist.Y2 = Math.Abs(dist2.Y2);
                        }
                        else
                        {
                            dist.X1 = Math.Abs(dist1.X1);
                            dist.Y1 = Math.Abs(dist1.Y1);
                            dist.X2 = Math.Abs(dist2.X1);
                            dist.Y2 = Math.Abs(dist2.Y1);
                        }
                        txtInspX1.Text = dist.X1.ToString("0.000");
                        txtInspY1.Text = dist.Y1.ToString("0.000");
                        txtInspX2.Text = dist.X2.ToString("0.000");
                        txtInspY2.Text = dist.Y2.ToString("0.000");
                    }


                    listAlignResult.Items.Clear();
                    listAlignResult.Items.Add(str);
                    listAlignResult.Items.Add("Align Result : X : " + align.X.ToString("0.000") + " Y : " + align.Y.ToString("0.000"));
                    listAlignResult.Items.Add("LX : " + dist.X1.ToString("0.000") + "  LY : " + dist.Y1.ToString("0.000") + "RX : " + dist.X2.ToString("0.000") + "  RY : " + dist.Y2.ToString("0.000"));
                    //listAlignResult.Items.Add("LX : " + dist2.X1.ToString("0.000") + "  LY : " + dist2.Y1.ToString("0.000") + "RX : " + dist2.X2.ToString("0.000") + "  RY : " + dist2.Y2.ToString("0.000"));
                    int TH = 0;
                    int ISGrey = 0;
                    CogRectangle Drect = HistoRegionRead(Vision[0].CFG.eCamName, ref TH, eHistogram.Detach);
                    bool insp = Vision[0].MarkingHistoInspection(Drect, TH, ref ISGrey, true);
                    if (insp) listAlignResult.Items.Add("Detach Inspection OK  : " + ISGrey);
                    else listAlignResult.Items.Add("Detach Inspection NG : "+ ISGrey);

                    TH = 0;
                    Drect = HistoRegionRead(Vision[0].CFG.eCamName, ref TH, eHistogram.MCRPre);
                    insp = Vision[0].MarkingHistoInspection(Drect, TH, ref ISGrey, false);
                    if (insp) listAlignResult.Items.Add("Mcr Pre Inspection OK : " + ISGrey);
                    else listAlignResult.Items.Add("MCR Pre Inspection NG : " + ISGrey);
                }
                #endregion
            }
        }
       
        public bool LaserMarkingInspection(ref cs2DAlign.ptXY dist, cs2DAlign.ptXYT panelPixel, bool bManual = false, csVision.sAlignResult rt1 = default(csVision.sAlignResult), bool bLineL = true, params csVision[] Vision)
        {
            bool result1 = false;


            //csVision.sAlignResult rt1 = new csVision.sAlignResult();
            CogLine cWidth1 = new CogLine();
            CogLine cHeight1 = new CogLine();
            CogPointMarker cMaker1 = new CogPointMarker();

            if (rt1.X1 == 0 && rt1.Y1 == 0)
            {
                result1 = Vision[0].MarkingInsp_Line(ref rt1, panelPixel.X, panelPixel.Y, bManual, ref cWidth1, ref cHeight1, ref cMaker1, bLineL);
            }
            else
            {
                Vision[0].MarkDisplay(rt1.X1, rt1.Y1);
                result1 = true;
            }



            cs2DAlign.ptXY LinePoint = new cs2DAlign.ptXY();
            LinePoint.X = rt1.X1;
            LinePoint.Y = rt1.Y1;


            if (result1)
            {
                cs2DAlign.ptXY resolution = new cs2DAlign.ptXY();
                cs2DAlign.ptXY pixelCnt = new cs2DAlign.ptXY();
                Menu.rsAlign.getResolution((int)eCalPos.Laser1, ref resolution, ref pixelCnt);
                if (resolution.X == 0)
                {
                    resolution.X = 0.0044;
                    resolution.Y = 0.0044;
                }

                dist.X = (panelPixel.X - LinePoint.X) * resolution.X;  //+
                dist.Y = (panelPixel.Y - LinePoint.Y) * resolution.Y;  // +

                if (dist.X > 0 && dist.Y > 0) result1 = true;
                else result1 = false;

                dist.X = Math.Abs((panelPixel.X - LinePoint.X) * resolution.X);
                dist.Y = Math.Abs((panelPixel.Y - LinePoint.Y) * resolution.Y);
            }
            return result1;
        }

        
        private void usetrans2Ddist(int iCalNO1, cs2DAlign.ptXYT ref1Pixel, cs2DAlign.ptXYT mark1Pixel, int iCalNO2, cs2DAlign.ptXYT ref2Pixel, cs2DAlign.ptXYT mark2Pixel, ref cs2DAlign.ptXXYY dist, ref double theta)
        {
            //새 인스펙션 좌표계 transform 이용
            double markTomark = CONST.RunRecipe.Param[eRecipe.PANEL_MARK_TO_MARK_LENGTH_X];

            //double diffy = ref2Pixel.Y - ref1Pixel.Y; //pixel계산할지 trans후 계산할지.. 고민

            double dT = 0;
            //Menu.rsAlign.getThetaScaleT(iCalNO1, ref dT, true); //이게 각도가 아니고 y/x만 나옴.. 미리 계산해둔 카메라간 각도(사용할지 말지는 테스트해보고 결정하자)
            double calT = Math.Atan(dT);

            //double deg = Vision[0].calcdeg(ref1Pixel, ref2Pixel, markTomark); //카메라에서 봤을 때 마크간 각도

            //체커보드 캘하면서 origin이바꼈는데 카메라 중심으로 기준점 맞춤.
            cs2DAlign.ptXY p1 = Vision[0].calcRobot(ref1Pixel);
            cs2DAlign.ptXY p2 = Vision[1].calcRobot(ref2Pixel);

            double diffy = p1.Y - p2.Y;
            double deg = Math.Asin(diffy / markTomark);

            deg += calT; //실제 각도(cal각도+체커보드각도) 테스트 필요.

            CogLine l0 = Vision[0].setcogline(deg, ref1Pixel);
            CogLine l1 = Vision[1].setcogline(deg, ref2Pixel);

            //cs2DAlign.ptXXYY dist = new cs2DAlign.ptXXYY();

            double lx = 0;
            double ly = 0;
            double rx = 0;
            double ry = 0;

            CogLine h0 = Vision[0].rightAngleDistIntersection(l0, ref1Pixel, mark1Pixel, ref lx, ref ly, ref dist.Y1);
            CogLine h1 = Vision[1].rightAngleDistIntersection(l1, ref2Pixel, mark2Pixel, ref rx, ref ry, ref dist.Y2);

            dist.X1 = Vision[0].ptopLength(ref1Pixel, lx, ly);
            dist.X2 = Vision[1].ptopLength(ref2Pixel, rx, ry);

            //그래픽 표시부
            Vision[0].cogDS.StaticGraphics.Add(l0, "wline");
            Vision[1].cogDS.StaticGraphics.Add(l1, "wline");
            Vision[0].cogDS.StaticGraphics.Add(h0, "hline");
            Vision[1].cogDS.StaticGraphics.Add(h1, "hline");
        }

        private void cbLineDispX_CheckedChanged(object sender, EventArgs e)
        {
            //pcy190404 예외처리
            if (double.TryParse(txtDistX1.Text, out double dist) && cbLineDispX1.Checked)
            {
                //pcy190510 세로선은 캠1,2 반전
                if (Vision[0] != null) Vision[0].LineDisp((-1) * dist, true);
            }
            else
            {
                if (Vision[0] != null) Vision[0].LineDisp(0, true);
            }
        }

        private void cbLineDispX2_CheckedChanged(object sender, EventArgs e)
        {
            //pcy190404 예외처리
            if (double.TryParse(txtDistX2.Text, out double dist) && cbLineDispX2.Checked)
            {
                //pcy190510 세로선은 캠1,2 반전
                if (Vision[1] != null) Vision[1].LineDisp(dist, true);
            }
            else
            {
                if (Vision[1] != null) Vision[1].LineDisp(0, true);
            }
        }

        private void cbLineDisp_CheckedChanged(object sender, EventArgs e)
        {
            if (double.TryParse(txtDistY1.Text, out double dist1) && cbLineDisp1.Checked)
            {
                if (Vision[0] != null) Vision[0].LineDisp(dist1, false, 1);
                if (Vision[1] != null) Vision[1].LineDisp(dist1, false, 1);
            }
            else if (!cbLineDisp1.Checked)
            {
                if (Vision[0] != null) Vision[0].LineDisp(0, false, 1);
                if (Vision[1] != null) Vision[1].LineDisp(0, false, 1);
            }
            if (double.TryParse(txtDistY2.Text, out double dist2) && cbLineDisp2.Checked)
            {
                if (Vision[0] != null) Vision[0].LineDisp(dist2, false, 2);
                if (Vision[1] != null) Vision[1].LineDisp(dist2, false, 2);
            }
            else if (!cbLineDisp2.Checked)
            {
                if (Vision[0] != null) Vision[0].LineDisp(0, false, 2);
                if (Vision[1] != null) Vision[1].LineDisp(0, false, 2);
            }
        }

        private void btnTargetPosSet_Click(object sender, EventArgs e)
        {
            //타겟 세이브
            //recognition에서 찍은거를 각각 target에 저장하고 target은 카메라 바꿀때마다 표시만..
            try
            {
                if (Menu.frmlogin.LogInCheck())
                {
                    DialogResult result = MessageBox.Show("Do you want to Set Taget data?", "Inspection", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        int ino = cbRecognition.SelectedIndex;

                        if (Vision[0].CFG.eCamName == nameof(eCamNO.Laser1) || Vision[0].CFG.eCamName == nameof(eCamNO.Laser2))
                        {
                            Vision[0].CFG.TargetX[0] = double.Parse(txtX[0].Text);
                            Vision[0].CFG.TargetY[0] = double.Parse(txtY[0].Text);
                            Menu.frmSetting.revData.WriteData(CONST.RunRecipe.RecipeName);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Confrm LoginUser");
                }
            }
            catch
            {
                MessageBox.Show("Search Reference Pattern Click");
            }
        }
        CogLine line = new CogLine();
        private void btnRecognition_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch(); // 20201103 ith add for test
            if (DateTime.Now.ToString("yyyyMMdd") == "20201103")
            {
                sw.Start();
            }

            if (Vision[0] == null && Vision[1] == null)
            {
                MessageBox.Show("CamName is not selected!");
                return;
            }

            double[] dX = new double[Vision.Length];
            double[] dY = new double[Vision.Length];
            double[] dT = new double[Vision.Length];

            CogLine cFLLine = new CogLine();
            CogGraphicLabel[] cogLabel = new CogGraphicLabel[Vision.Length];
            for (int i = 0; i < cogLabel.Length; i++)
            {
                cogLabel[i] = new CogGraphicLabel();
            }
            Font font;

            //20201003 cjm 화면에 Score 표시 추가
            CogGraphicLabel[] cogScoreLabel = new CogGraphicLabel[Vision.Length];
            for (int i = 0; i < cogScoreLabel.Length; i++)
            {
                cogScoreLabel[i] = new CogGraphicLabel();
            }

            bool[] bPatternSerch = new bool[4];


            string CamKind = cbCamList.SelectedItem.ToString();
            string kind = cbRecognition.SelectedItem.ToString();

            if (cbCamList.Text == "")
            {
                MessageBox.Show("Please Select Camera!");
                return;
            }

            cogDS.InteractiveGraphics.Clear();
            cogDS2.InteractiveGraphics.Clear();
            cogDS3.InteractiveGraphics.Clear();
            cogDS4.InteractiveGraphics.Clear();

            //카메라 고를때 vision[n]중 안쓰는건 null로 넣어둠.
            string bLog = cbCamList.SelectedItem.ToString();
            font = new Font("Arial", 30, FontStyle.Bold);
            string sLog = CamKind + "," + kind;
            for (int i = 0; i < Vision.Length; i++)
            {
                if (Vision[i] != null)
                {
                    if (rbPattern.Checked || rbDL1.Checked || rbDL2.Checked)
                    {
                        Vision[i].Overay(true);

                        if (rbPattern.Checked) bPatternSerch[i] = Vision[i].PatternSearchEnum(ref dX[i], ref dY[i], ref dT[i], (ePatternKind)cbRecognition.SelectedItem);
                        else
                        {
                            int index = cbCamList.SelectedIndex * Menu.frmSetting.revData.mDL[cbCamList.SelectedIndex].MarkSearch_Use.Length;
                            cs2DAlign.ptXYT xyt = new cs2DAlign.ptXYT();
                            if (rbDL1.Checked) bPatternSerch[i] = Vision[i].PatternSearch_DL(ref xyt, (ePatternKind)cbRecognition.SelectedItem, index + 0);
                            else if (rbDL2.Checked) bPatternSerch[i] = Vision[i].PatternSearch_DL(ref xyt, (ePatternKind)cbRecognition.SelectedItem, index + 1);

                            dX[i] = xyt.X;
                            dY[i] = xyt.Y;
                        }

                        string msg = "OK";
                        if (!bPatternSerch[i]) msg = "NG";
                        cogLabel[i].Color = CogColorConstants.Green;
                        cogLabel[i].Font = font;
                        cogLabel[i].SetXYText(200, cogDS.Image.Height - 50, msg);
                        Vision[i].cogDS.InteractiveGraphics.Add(cogLabel[i], "RecogResult", false);
                        txtX[i].Text = dX[i].ToString("0.000");
                        txtY[i].Text = dY[i].ToString("0.000");
                        txtR[i].Text = dT[i].ToString("0.000");
                        bLog += "," + dX[i].ToString("0.000") + "," + dY[i].ToString("0.000") + "," + dT[i].ToString("0.000");
                        sLog += "," + dX[i].ToString("0.000") + "," + dY[i].ToString("0.000");

                        // 20201003 cjm 화면 스코어 표시 추가
                        // 20201004 cjm 스코어표시 수정
                        string score = "0.00";
                        score = Vision[i].GetSearchScore((int)(ePatternKind)cbRecognition.SelectedItem).ToString("0.00");
                        cogScoreLabel[i].Color = CogColorConstants.Purple;
                        cogScoreLabel[i].Font = font;
                        cogScoreLabel[i].SetXYText(200 + 600, cogDS.Image.Height - 50, score);
                        Vision[i].cogDS.InteractiveGraphics.Add(cogScoreLabel[i], "RecogResult", false);
                    }
                    else if (rbLine.Checked)
                    {
                        Vision[0].LineSearchEnum(ref line, (eLineKind)cbRecognition.SelectedItem);
                        //추가필요 라인 개별로 찾는정도만..
                    }
                }
                else
                {
                    txtX[i].Text = "0.000";
                    txtY[i].Text = "0.000";
                    txtR[i].Text = "0.000";
                }
            }
            //cam1 cal2특수한 경우..
            if (Vision[0].CFG.CalType == eCalType.Cam1Cal2)
            {
                if (rbPattern.Checked)
                {
                    double dX1 = 0;
                    double dY1 = 0;
                    double dT1 = 0;
                    Vision[0].PatternSearchEnum(ref dX1, ref dY1, ref dT1, (ePatternKind)(cbRecognition.SelectedIndex + 1));
                    txtX[1].Text = dX1.ToString("0.000");
                    txtY[1].Text = dY1.ToString("0.000");
                    txtR[1].Text = dT1.ToString("0.000");
                    bLog += "," + dX1.ToString("0.000") + "," + dY1.ToString("0.000") + "," + dT1.ToString("0.000");
                    sLog += "," + dX1.ToString("0.000") + "," + dY1.ToString("0.000");
                }
            }

            if (DateTime.Now.ToString("yyyyMMdd") == "20201103")
            {
                sw.Stop();
                Console.WriteLine("Recipe Recognition " + sw.ElapsedMilliseconds.ToString() + " msec");
            }

            //로그 저장
            cLog.Save(LogKind.AlignMeasure, bLog);
            cLog.Save(LogKind.PixelInfo, sLog);
        }

        private void btnClearImage_Click(object sender, EventArgs e)
        {
            if (Vision[0] == null)
            {
                MessageBox.Show("CamName is not selected!");
                return;
            }
            try
            {
                cbOveray.Checked = false;
                cbDist.Checked = false;
                cbLineDisp1.Checked = false;
                cbLineDisp2.Checked = false;
                cbLineDispX1.Checked = false;
                cbLineDispX2.Checked = false;
                foreach (var s in Vision)
                {
                    if (s != null)
                    {
                        s.cogDS.InteractiveGraphics.Clear();
                        s.cogDS.StaticGraphics.Clear();
                    }
                }
            }
            catch { }
        }

        private void InspCalSave(eCalPos calNo1, eCalPos calNo2)
        {
            double scaleLX, scaleRX;
            double scaleLY, scaleRY;
            int VisionNo = cbCamList.SelectedIndex;
            if (cbCamList.SelectedIndex == 1 || cbCamList.SelectedIndex == 3 || cbCamList.SelectedIndex == 5 || cbCamList.SelectedIndex == 7)
            {
                VisionNo = cbCamList.SelectedIndex - 1;
            }
            scaleLX = double.Parse(tb_ScaleLX.Text);
            scaleLY = double.Parse(tb_ScaleLY.Text);
            scaleRX = double.Parse(tb_ScaleRX.Text);
            scaleRY = double.Parse(tb_ScaleRY.Text);

            Menu.frmSetting.revData.WriteData(CONST.RunRecipe.RecipeName);
            Menu.frmSetting.REVTabUpdate();

            //20200926 cjm revData -> textBox 쓰기
            revDataTotxt();

            Menu.rsAlign.setScale((int)calNo1, scaleLX, scaleLY);
            Menu.rsAlign.setScale((int)calNo2, scaleRX, scaleRY);
        }

        private void btnDrawLine_Click(object sender, EventArgs e)
        {
            try
            {
                cogDS.InteractiveGraphics.Clear();
                cogDS2.InteractiveGraphics.Clear();
                cogDS3.InteractiveGraphics.Clear();
                cogDS4.InteractiveGraphics.Clear();

                cogDS.InteractiveGraphics.Remove("SCFEdgeLine");
                cogDS2.InteractiveGraphics.Remove("SCFEdgeLine");
            }
            catch
            {
            }
            try
            {
                if (cbEdgeLine.SelectedIndex == -1)
                {
                    MessageBox.Show("Select Line Kind");
                    return;
                }

                string LineKind = cbEdgeLine.SelectedItem.ToString();

                CogLine cLine1 = new CogLine();
                CogLine cLine2 = new CogLine();

                Vision[0].FindLine(ref cLine1, LineKind);
                Vision[1].FindLine(ref cLine2, LineKind);
            }
            catch
            {
            }
        }

        public class MapPosition
        {
            public double x;
            public double y;

            public MapPosition(double x, double y)
            {
                this.x = x;
                this.y = y;
            }
        }

        private MapPosition GetMouseMapPosition(int mouseX, int mouseY, CogDisplay cogDisp)
        {
            double destX = 0;
            double destY = 0;

            ICogTransform2D iTransPosition;
            CogCoordinateSpaceTree cogCoordinateSpaceTree;

            cogCoordinateSpaceTree = cogDisp.UserDisplayTree;

            string lcTempName = "";

            if (cogCoordinateSpaceTree == null)
                lcTempName = "pos";
            else
                lcTempName = cogCoordinateSpaceTree.RootName;

            iTransPosition = cogDisp.GetTransform("#", lcTempName);
            iTransPosition.MapPoint(Convert.ToDouble(mouseX), Convert.ToDouble(mouseY), out destX, out destY);

            return new MapPosition(destX, destY);
        }

        private void cbBending_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nBendingArm = 0;

            if (cbBending.SelectedIndex == -1)
                nBendingArm = 0;
            else
                nBendingArm = cbBending.SelectedIndex;

            for (int i = 0; i < 100; i++)
            {
                if (dataGridView_MotionTrace.Rows.Count < 100)
                {
                    dataGridView_MotionTrace.Rows.Add();
                    dataGridView_MotionTrace.Rows[i].HeaderCell.Value = (i + 1).ToString("0");
                }

                dataGridView_MotionTrace.Rows[i].Cells[0].Value = CONST.m_dTraceY[nBendingArm, i].ToString("0.00000");
                dataGridView_MotionTrace.Rows[i].Cells[1].Value = CONST.m_dTraceZ[nBendingArm, i].ToString("0.00000");
                dataGridView_MotionTrace.Rows[i].Cells[2].Value = CONST.m_dTraceT[nBendingArm, i].ToString("0.00000");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Menu.frmlogin.LogInCheck())
            {
                if (cbBending.SelectedIndex < 0)
                {
                    MessageBox.Show("Select Bending Arm");
                    return;
                }

                MotionTraceDataRead(cbBending.SelectedIndex);
                TeachtoGraph();
            }
            else
            {
                MessageBox.Show("confrm LoginUser");
            }
        }

        private void cbBending_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            int nIndex = -1;
            nIndex = cbBending.SelectedIndex;

            string strFileName = "";

            string sPath = CONST.cMotionTracePath + "\\" + CONST.RunRecipe.RecipeName;

            DirectoryInfo RawTempDirectory = new DirectoryInfo(sPath);
            try
            {
                FileInfo[] RawTempFile = RawTempDirectory.GetFiles("*.csv");

                for (int i = 0; i < RawTempFile.Length; i++)
                {
                    string[] ListName = RawTempFile[i].ToString().Split('_');

                    if (ListName[0] == (nIndex + 1).ToString("0"))
                    {
                        strFileName = RawTempFile[i].ToString();
                    }
                }

                lblTraceFileName.Text = strFileName;
            }
            catch { }
        }
        public void Camera_Change()
        {
            cbCamList.SelectedIndex = 0;
        }

        //20.10.11 lkw
        private void LCheckTest_Click(object sender, EventArgs e)
        {
            //cam2type과 cam1cal2type만 작성함
            double dX1 = 0;
            double dY1 = 0;
            double dX2 = 0;
            double dY2 = 0;
            double dR = 0;
            eCalPos calpos_L = 0;
            eCalPos calpos_R = 0;
            int VisionNo = cbCamList.SelectedIndex;

            listAlignResult.Items.Clear();

            int iSpec = 1;
            if (rbLcheck2.Checked) iSpec = 2;
            GetLcheckSpecTolerence(Vision[0].CFG.eCamName, out double dSpec, out double dTolerence, iSpec);

            if (eCalType.Cam1Cal2 == Vision[0].CFG.CalType)
            {
                if (!cbBendSelect.Visible)
                {
                    Menu.frmAutoMain.GetCalPos(ref calpos_L, Vision[0].CFG.Camno, 1);
                    Menu.frmAutoMain.GetCalPos(ref calpos_R, Vision[0].CFG.Camno, 1);
                }
                else
                {
                    if (cbBendSelect.SelectedIndex < 0) cbBendSelect.SelectedIndex = 0;
                    Menu.frmAutoMain.GetCalPos(ref calpos_L, Vision[0].CFG.Camno, cbBendSelect.SelectedIndex, false);
                    Menu.frmAutoMain.GetCalPos(ref calpos_R, Vision[0].CFG.Camno, cbBendSelect.SelectedIndex, true);
                }

                if (Vision[0].PatternSearchEnum(ref dX1, ref dY1, ref dR, ePatternKind.Left_1cam)
                && Vision[0].PatternSearchEnum(ref dX2, ref dY2, ref dR, ePatternKind.Right_1cam))
                {
                    cs2DAlign.ptXY Point1 = new cs2DAlign.ptXY();
                    cs2DAlign.ptXY Point2 = new cs2DAlign.ptXY();

                    Point1.X = dX1;
                    Point1.Y = dY1;
                    Point2.X = dX2;
                    Point2.Y = dY2;
                    double offset = Menu.frmSetting.revData.mLcheck[VisionNo].LCheckOffset1;
                    double length = Menu.rsAlign.getLength((int)calpos_L, (int)calpos_R, Point1, Point2, offset);
                    //length = Menu.rsAlign.getLength((int)calpos_L, Point1, Point2, offset);
                    listAlignResult.Items.Add("LCheck Spec : " + dSpec.ToString("0.0000"));
                    listAlignResult.Items.Add("LCheck Test : " + length.ToString("0.0000"));
                }
                else
                {
                    listAlignResult.Items.Add("Mark Search Fail");
                }
            }
            //20.10.11 lkw
            else if (eCalType.Cam2Type == Vision[0].CFG.CalType || eCalType.Cam3Type == Vision[0].CFG.CalType)
            {
                Menu.frmAutoMain.GetCalPos(ref calpos_L, Vision[0].CFG.Camno, 0);
                Menu.frmAutoMain.GetCalPos(ref calpos_R, Vision[1].CFG.Camno, 0);

                ePatternKind p1 = ePatternKind.Panel;
                double offset = Menu.frmSetting.revData.mLcheck[VisionNo].LCheckOffset1; //기본 panel끼리
                //string swhere = "Panel";
                
                if (Vision[0].PatternSearchEnum(ref dX1, ref dY1, ref dR, p1)
                && Vision[1].PatternSearchEnum(ref dX2, ref dY2, ref dR, p1))
                {
                    cs2DAlign.ptXY Point1 = new cs2DAlign.ptXY();
                    cs2DAlign.ptXY Point2 = new cs2DAlign.ptXY();

                    Point1.X = dX1;
                    Point1.Y = dY1;
                    Point2.X = dX2;
                    Point2.Y = dY2;
                                      
                    double length = Menu.rsAlign.getLength((int)calpos_L, (int)calpos_R, Point1, Point2, offset);

                    listAlignResult.Items.Add("LCheck Spec : " + dSpec.ToString("0.0000"));
                    listAlignResult.Items.Add("LCheck Test : " + length.ToString("0.0000"));

                }
                else
                {
                    listAlignResult.Items.Add("Mark Search Fail");
                }
            }
        }

        public void calDataSave(double[] dData)
        {
            int calNO = (int)dData[2];
            cs2DAlign.ptXY[] pos = new cs2DAlign.ptXY[9];
            Menu.rsAlign.setRobotPos(dData[3], dData[4], ref pos);
            List<cs2DAlign.ptXY> pixelData = new List<cs2DAlign.ptXY>();
            for (int i = 0; i < 9; i++)
            {
                cs2DAlign.ptXY pixel = new cs2DAlign.ptXY();
                pixel.X = dData[2 * i + 5];
                pixel.Y = dData[2 * i + 6];
                pixelData.Add(pixel);
            }
            cs2DAlign.ptXY pixelCnt = new cs2DAlign.ptXY();
            pixelCnt.X = dData[23];
            pixelCnt.Y = dData[24];
            Menu.rsAlign.setCamCalibration(calNO, pixelData, pixelCnt);
            cs2DAlign.ptXY firstPos = new cs2DAlign.ptXY();
            cs2DAlign.ptXY secondPos = new cs2DAlign.ptXY();
            cs2DAlign.ptXY CalFixPosOffset = new cs2DAlign.ptXY();
            firstPos.X = dData[25];
            firstPos.Y = dData[26];
            secondPos.X = dData[27];
            secondPos.Y = dData[28];

            CalFixPosOffset.X = 0;
            CalFixPosOffset.Y = 0;

            Menu.rsAlign.setCenterOfRotation(calNO, firstPos, secondPos, dData[29], dData[30], CalFixPosOffset);
        }

        //라인 찾는거 추가
        private void btnPreSave_Click(object sender, EventArgs e)
        {
            if (Menu.frmlogin.LogInCheck())
            {
                if (MessageBox.Show("Do you want to Save?", "Information", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    int camNo = cbCamList.SelectedIndex;
                    Menu.frmSetting.REVTabUpdate();
                    revDataTotxt();
                }
            }
        }

        public void RecipeParamDisp()
        {
            int RcpNo = 1;
            if (Recipedgv.InvokeRequired)
            {
                Recipedgv.Invoke(new MethodInvoker(delegate
                {
                    Recipedgv.Rows.Clear();

                    foreach (var s in CONST.RunRecipe.Param)
                    {
                        //s.Key.ToString() 안하면 안되는듯?
                        Recipedgv.Rows.Add(RcpNo.ToString(), s.Key.ToString(), s.Value);
                        RcpNo += 1;
                    }
                }));
            }
            else
            {
                try //임시
                {
                    Recipedgv.Rows.Clear();
                    foreach (var s in CONST.RunRecipe.Param)
                    {
                        //s.Key.ToString() 안하면 안되는듯?
                        Recipedgv.Rows.Add(RcpNo.ToString(), s.Key.ToString(), s.Value);
                        RcpNo += 1;
                    }
                }
                catch { }
            }
        }

        //2020.09.15 lkw 
        private void btn_Run_Click(object sender, EventArgs e)
        {
            if (Menu.frmlogin.LogInCheck())
            {
                if (Vision[0] == null)
                {
                    MessageBox.Show("CamName is not selected!");
                    return;
                }

                //pcy190405 Tranfer cal없으니 항상 1로 사용.
                int kind = cbBendSelect.SelectedIndex;
                kind = 1;

                //마크찾기
                this.btnInspection_Click(this, null);

                tb_MeasureLX.Text = txtInspX1.Text;
                tb_MeasureLY.Text = txtInspY1.Text;
                tb_MeasureRX.Text = txtInspX2.Text;
                tb_MeasureRY.Text = txtInspY2.Text;

                int VisionNo = cbCamList.SelectedIndex;

                string camName = cbCamList.SelectedItem.ToString();

                eCalPos calNo1 = 0;
                eCalPos calNo2 = 0;

                Menu.frmAutoMain.GetCalPos(ref calNo1, VisionNo, kind);
                Menu.frmAutoMain.GetCalPos(ref calNo2, VisionNo + 1, kind);

                if (string.IsNullOrEmpty(txtInspY1.Text) || string.IsNullOrEmpty(txtInspY2.Text))
                {
                    MessageBox.Show("Master Panel Inspection must be Done!!");
                    return;
                }
                if (string.IsNullOrEmpty(tb_MasterLY.Text) || string.IsNullOrEmpty(tb_MasterRY.Text))
                {
                    MessageBox.Show("Measure Value must be Done!!");
                    return;
                }
                else if (MessageBox.Show("Are you Sure Run?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    #region 사용자의 마스터지그 입력값 오타로 인한 스케일 계산 문제 조치
                    //-------------------------------------------------------------------------
                    // 20201104 ith add
                    // 입력시 소수점을 콤마로 오기입하면 콤마는 무시되어 계산됨
                    // 9,123 은 9123으로 변한됨.......에러 발생 안 함
                    //-------------------------------------------------------------------------
                    tb_MasterLX.Text = tb_MasterLX.Text.Replace(',', '.').Replace(" ", "");
                    tb_MasterLY.Text = tb_MasterLY.Text.Replace(',', '.').Replace(" ", "");
                    tb_MasterRX.Text = tb_MasterRX.Text.Replace(',', '.').Replace(" ", "");
                    tb_MasterRY.Text = tb_MasterRY.Text.Replace(',', '.').Replace(" ", "");
                    //-------------------------------------------------------------------------
                    #endregion

                    //2020.10.02 lkw
                    //벤딩과 인스펙션만 스케일사용함.
                    if (Vision[0].CFG.eCamName == nameof(eCamNO.Laser1) || Vision[0].CFG.eCamName == nameof(eCamNO.Laser2))
                    {
                        double markTomark = 0;
                        rs2DAlign.cs2DAlign.ptXXYY jigLength = new rs2DAlign.cs2DAlign.ptXXYY();

                        cs2DAlign.ptXY pixelCnt1 = new cs2DAlign.ptXY();
                        pixelCnt1.X = Vision[0].ImgX;
                        pixelCnt1.Y = Vision[0].ImgY;
                        cs2DAlign.ptXY pixelCnt2 = new cs2DAlign.ptXY();
                        pixelCnt2.X = Vision[1].ImgX;
                        pixelCnt2.Y = Vision[1].ImgY;

                        try
                        {
                            markTomark = CONST.RunRecipe.Param[eRecipe.PANEL_MARK_TO_MARK_LENGTH_X];

                            if (markTomark == 0)
                            {
                                MessageBox.Show("Confirm Recipe Input Value (Mark to Mark)");
                                return;
                            }

                            jigLength.X1 = double.Parse(tb_MasterLX.Text);
                            jigLength.Y1 = double.Parse(tb_MasterLY.Text);
                            jigLength.X2 = double.Parse(tb_MasterRX.Text);
                            jigLength.Y2 = double.Parse(tb_MasterRY.Text);
                        }
                        catch
                        {
                            MessageBox.Show("Confirm Input Value!");
                            return;
                        }
                        if (markTomark > 0 && jigLength.X1 > 0 && jigLength.X2 > 0 && jigLength.Y1 > 0 && jigLength.Y2 > 0)
                        {
                            //20.10.06 lkw
                            cs2DAlign.ptXXYY resolution = new cs2DAlign.ptXXYY();

                            Menu.rsAlign.setRefresolution = 0.0099; //디폴트

                            Menu.frmAutoMain.SetXYfromXYT(out cs2DAlign.ptXY sourcePixel1, out cs2DAlign.ptXY targetPixel1, out cs2DAlign.ptXY sourcePixel2, out cs2DAlign.ptXY targetPixel2,
                                ref1Pixel, ref2Pixel, mark1Pixel, mark2Pixel, false);

                            //20.10.07 lkw
                            resolution = Menu.rsAlign.setCalibration_notUseCal((int)calNo1, sourcePixel1, targetPixel1, pixelCnt1, markTomark, (int)calNo2, sourcePixel2, targetPixel2, pixelCnt2, jigLength, false);
                            //resolution_new = Menu.rsAlign.setCalibration_notUseCal_new((int)calNo1 + 2, sourcePixel1, targetPixel1, pixelCnt1, markTomark, (int)calNo2 + 2, sourcePixel2, targetPixel2, PixelCnt2, jigLength, false);

                            tb_ScaleLX.Text = resolution.X1.ToString("0.00000000");
                            tb_ScaleLY.Text = resolution.Y1.ToString("0.00000000");
                            tb_ScaleRX.Text = resolution.X2.ToString("0.00000000");
                            tb_ScaleRY.Text = resolution.Y2.ToString("0.00000000");

                        }
                    }
                    else //bending, attach
                    {
                        //pcy210118
                        calNo1 = ChangeAttachtoAttachInsp(calNo1);
                        calNo2 = ChangeAttachtoAttachInsp(calNo2);

                        double scaleLX = 1.0;
                        double scaleRX = 1.0;
                        double scaleLY = 1.0;
                        double scaleRY = 1.0;

                        double CamScaleLX = 1.0;
                        double CamScaleLY = 1.0;
                        double CamScaleRX = 1.0;
                        double CamScaleRY = 1.0;

                        Menu.rsAlign.getScale((int)calNo1, ref CamScaleLX, ref CamScaleLY);
                        Menu.rsAlign.getScale((int)calNo2, ref CamScaleRX, ref CamScaleRY);

                        scaleLX = (Convert.ToDouble(tb_MasterLX.Text) * CamScaleLX) / Convert.ToDouble(tb_MeasureLX.Text);
                        scaleLY = (Convert.ToDouble(tb_MasterLY.Text) * CamScaleLY) / Convert.ToDouble(tb_MeasureLY.Text);
                        scaleRX = (Convert.ToDouble(tb_MasterRX.Text) * CamScaleRX) / Convert.ToDouble(tb_MeasureRX.Text);
                        scaleRY = (Convert.ToDouble(tb_MasterRY.Text) * CamScaleRY) / Convert.ToDouble(tb_MeasureRY.Text);

                        tb_ScaleLX.Text = scaleLX.ToString();
                        tb_ScaleLY.Text = scaleLY.ToString();
                        tb_ScaleRX.Text = scaleRX.ToString();
                        tb_ScaleRY.Text = scaleRY.ToString();
                    }
                    if ((MessageBox.Show("Are you new scale Save?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes))
                    {
                        this.btnSave_Click(this, null);
                    }
                }
            }
            else
            {
                MessageBox.Show("Comfirm LogIn Check");
            }
        }

        //2020.09.15 lkw
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (Menu.frmlogin.LogInCheck())
            {
                //pcy190405 Tranfer cal없으니 항상 1로 사용.
                int kind = cbBendSelect.SelectedIndex;
                kind = 1;

                int VisionNo = cbCamList.SelectedIndex;
                if (cbCamList.SelectedIndex == 1 || cbCamList.SelectedIndex == 3 || cbCamList.SelectedIndex == 5 || cbCamList.SelectedIndex == 7)
                {
                    VisionNo = cbCamList.SelectedIndex - 1;
                }

                string camName = cbCamList.SelectedItem.ToString();

                if (string.IsNullOrEmpty(txtInspY1.Text) || string.IsNullOrEmpty(txtInspY2.Text))
                {
                    MessageBox.Show("Master Panel Inspection must be Done!!");
                    return;
                }
                else if (MessageBox.Show("Are you Sure Save?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    eCalPos calNo1 = eCalPos.Laser1;
                    eCalPos calNo2 = eCalPos.Laser2;

                    Menu.frmAutoMain.GetCalPos(ref calNo1, VisionNo, kind);
                    Menu.frmAutoMain.GetCalPos(ref calNo2, VisionNo + 1, kind);

                    if (Vision[0].CFG.eCamName == nameof(eCamNO.Laser1) || Vision[0].CFG.eCamName == nameof(eCamNO.Laser2))
                    {
                        cs2DAlign.ptXY pixelCnt1 = new cs2DAlign.ptXY();
                        pixelCnt1.X = Vision[0].ImgX;
                        pixelCnt1.Y = Vision[0].ImgY;
                        cs2DAlign.ptXY pixelCnt2 = new cs2DAlign.ptXY();
                        pixelCnt2.X = Vision[1].ImgX;
                        pixelCnt2.Y = Vision[1].ImgY;

                        double markTomark = 0;
                        rs2DAlign.cs2DAlign.ptXXYY jigLength = new rs2DAlign.cs2DAlign.ptXXYY();
                        try
                        {
                            markTomark = CONST.RunRecipe.Param[eRecipe.PANEL_MARK_TO_MARK_LENGTH_X];
                            jigLength.X1 = double.Parse(tb_MasterLX.Text);
                            jigLength.Y1 = double.Parse(tb_MasterLY.Text);
                            jigLength.X2 = double.Parse(tb_MasterRX.Text);
                            jigLength.Y2 = double.Parse(tb_MasterRY.Text);
                        }
                        catch
                        {
                            MessageBox.Show("Confirm Input Value!");
                            return;
                        }
                        if (markTomark > 0 && jigLength.X1 > 0 && jigLength.X2 > 0 && jigLength.Y1 > 0 && jigLength.Y2 > 0)
                        {
                            //20.10.06 lkw
                            cs2DAlign.ptXXYY resolution = new cs2DAlign.ptXXYY();

                            Menu.rsAlign.setRefresolution = 0.0099; //확인

                            Menu.frmAutoMain.SetXYfromXYT(out cs2DAlign.ptXY sourcePixel1, out cs2DAlign.ptXY targetPixel1, out cs2DAlign.ptXY sourcePixel2, out cs2DAlign.ptXY targetPixel2,
                                ref1Pixel, ref2Pixel, mark1Pixel, mark2Pixel, false);

                            //20.10.07 lkw
                            resolution = Menu.rsAlign.setCalibration_notUseCal((int)calNo1, sourcePixel1, targetPixel1, pixelCnt1, markTomark, (int)calNo2, sourcePixel2, targetPixel2, pixelCnt2, jigLength);
                           
                            Menu.frmAutoMain.Vision[Vision_No.Laser1].CFG.FOVX = Convert.ToDouble((Math.Abs(pixelCnt1.X * resolution.X1)).ToString("0.000"));
                            Menu.frmAutoMain.Vision[Vision_No.Laser1].CFG.FOVY = Convert.ToDouble((Math.Abs(pixelCnt1.Y * resolution.Y1)).ToString("0.000"));
                            
                            Menu.Config.CAMconfig_Write(Bending.Menu.frmAutoMain.Vision[Vision_No.Laser1].CFG, Vision_No.Laser1);
                        }
                    }
                    else //bend, attach
                    {
                        //pcy210118
                        calNo1 = ChangeAttachtoAttachInsp(calNo1);
                        calNo2 = ChangeAttachtoAttachInsp(calNo2);

                        InspCalSave(calNo1, calNo2);
                    }
                }
            }
            else
            {
                MessageBox.Show("Confrim Login Check");
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            //190517 로그인체크 추가
            if (!Menu.frmlogin.LogInCheck())
            {
                MessageBox.Show("Confrim Login Check");
                return;
            }
            if (MessageBox.Show("Are you Sure Scale Value Reset?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                //pcy190405 Tranfer cal없으니 항상 1로 사용.
                int kind = cbBendSelect.SelectedIndex;
                kind = 1;
                int VisionNo = cbCamList.SelectedIndex;
                if (cbCamList.SelectedIndex == 1 || cbCamList.SelectedIndex == 3 || cbCamList.SelectedIndex == 5 || cbCamList.SelectedIndex == 7)
                {
                    VisionNo = cbCamList.SelectedIndex - 1;
                }

                double ScaleLX = 1;
                double ScaleLY = 1;
                double ScaleRX = 1;
                double ScaleRY = 1;

                eCalPos calNo1 = eCalPos.Laser1;
                eCalPos calNo2 = eCalPos.Laser2;

                Menu.frmAutoMain.GetCalPos(ref calNo1, VisionNo, kind);
                Menu.frmAutoMain.GetCalPos(ref calNo2, VisionNo + 1, kind);

                //pcy210118
                calNo1 = ChangeAttachtoAttachInsp(calNo1);
                calNo2 = ChangeAttachtoAttachInsp(calNo2);

                Menu.rsAlign.resetScale((int)calNo1);
                Menu.rsAlign.resetScale((int)calNo2);

                tb_ScaleLX.Text = ScaleLX.ToString("0.000");
                tb_ScaleLY.Text = ScaleLY.ToString("0.000");
                tb_ScaleRX.Text = ScaleRX.ToString("0.000");
                tb_ScaleRY.Text = ScaleRY.ToString("0.000");
            }
        }

        private void txtVisionPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (txtVisionPassword.Text == CONST.VisionPassword)
                {
                    pnVisionPassword.Visible = false;
                }
                else
                    MessageBox.Show("Please enter the correct password.");

                txtVisionPassword.Text = "";
            }
        }

        private void txtTracePassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (txtTracePassword.Text == CONST.TracePassword)
                {
                    pnTracePassword.Visible = false;
                }
                else
                    MessageBox.Show("please enter the correct password.");

                txtTracePassword.Text = "";
            }
        }


        private void btnuvrwTsave_Click(object sender, EventArgs e)
        {
            int VisionNo = cbCamList.SelectedIndex;
            if (cbCamList.SelectedIndex == 1 || cbCamList.SelectedIndex == 3 || cbCamList.SelectedIndex == 5 || cbCamList.SelectedIndex == 7)
            {
                VisionNo = cbCamList.SelectedIndex - 1;
            }   
        }

        private void btnCapture_Click(object sender, EventArgs e)
        {
            if (Vision[0] == null)
            {
                MessageBox.Show("CamName is not selected!");
                return;
            }

            if (functionInterlock())
            {
                foreach (var s in Vision)
                {
                    if (s != null)
                        s.Capture(false, true, false, !cbCalImageTest.Checked); //pcy210203
                }
            }
        }


        private void cbRefPos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbRefPos.SelectedIndex == 0) pnSetTaget.Visible = false;
            else pnSetTaget.Visible = true;
        }

        private void cbCalLog_CheckedChanged(object sender, EventArgs e)
        {
            if (cbCalLog.Checked)
            {
                txtCalLogCnt.Visible = true; txtCalLogCnt.Text = "1"; CallogStop = false;
            }
            else
            {
                txtCalLogCnt.Visible = false; txtCalLogCnt.Text = ""; CallogStop = true;
            }
        }

        private void txtImageX_Click(object sender, EventArgs e)
        {
            cbCalLog.Visible = true;
        }

        private void txtImageY_Click(object sender, EventArgs e)
        {
            cbCalLog.Checked = false;
            cbCalLog.Visible = false;
        }

        private CogLine PointLine = new CogLine();

        private void cbPointLine_CheckedChanged(object sender, EventArgs e)
        {
            if (cbPointLine.Checked)
            {
                cbImageInfo.Checked = true;
            }
            else
            {
                cbImageInfo.Checked = false;
            }
        }

        private List<MapPosition> Lnpoint = new List<MapPosition>();
        private Queue<MapPosition> LLength = new Queue<MapPosition>();

        private void cogDS_Click(object sender, EventArgs e)
        {
            if (Vision[0] != null)
            {
                if (cbPointLine.Checked)
                {
                    //string a = txtImageX.Text.Substring(4);
                    cogDS.InteractiveGraphics.Clear();

                    double MousePosX = Convert.ToDouble(txtImageX.Text.Substring(4));
                    double MousePosY = Convert.ToDouble(txtImageY.Text.Substring(4));

                    Vision[0].CreateLine(MousePosX, MousePosY, 0, ref PointLine);
                }
                if (cbNcalLeft.Checked)
                {
                    MapPosition temp = new MapPosition(pos.x, pos.y);
                    Lnpoint.Add(temp);
                }
                if (cblengthcheck.Checked)
                {
                    MapPosition temp = new MapPosition(pos.x, pos.y);
                    LLength.Enqueue(temp);
                }
            }
        }
        private void cogDS2_Click(object sender, EventArgs e)
        {
            if (Vision[1] != null)
            {
                if (cbPointLine.Checked)
                {
                    //string a = txtImageX.Text.Substring(4);
                    cogDS2.InteractiveGraphics.Clear();

                    double MousePosX = Convert.ToDouble(txtImageX.Text.Substring(4));
                    double MousePosY = Convert.ToDouble(txtImageY.Text.Substring(4));

                    Vision[1].CreateLine(MousePosX, MousePosY, 0, ref PointLine);
                }
                if (cbNcalLeft.Checked)
                {
                    MapPosition temp = new MapPosition(pos.x, pos.y);
                    Lnpoint.Add(temp);
                }
                if (cblengthcheck.Checked)
                {
                    MapPosition temp = new MapPosition(pos.x, pos.y);
                    LLength.Enqueue(temp);
                }
            }
        }
        private void cogDS3_Click(object sender, EventArgs e)
        {
            if (Vision[2] != null)
            {
                if (cbPointLine.Checked)
                {
                    //string a = txtImageX.Text.Substring(4);
                    cogDS3.InteractiveGraphics.Clear();

                    double MousePosX = Convert.ToDouble(txtImageX.Text.Substring(4));
                    double MousePosY = Convert.ToDouble(txtImageY.Text.Substring(4));

                    Vision[2].CreateLine(MousePosX, MousePosY, 0, ref PointLine);
                }
                if (cbNcalLeft.Checked)
                {
                    MapPosition temp = new MapPosition(pos.x, pos.y);
                    Lnpoint.Add(temp);
                }
                if (cblengthcheck.Checked)
                {
                    MapPosition temp = new MapPosition(pos.x, pos.y);
                    LLength.Enqueue(temp);
                }
            }
        }
        private void cogDS4_Click(object sender, EventArgs e)
        {
            if (Vision[3] != null)
            {
                if (cbPointLine.Checked)
                {
                    //string a = txtImageX.Text.Substring(4);
                    cogDS4.InteractiveGraphics.Clear();

                    double MousePosX = Convert.ToDouble(txtImageX.Text.Substring(4));
                    double MousePosY = Convert.ToDouble(txtImageY.Text.Substring(4));

                    Vision[3].CreateLine(MousePosX, MousePosY, 0, ref PointLine);
                }
                if (cbNcalLeft.Checked)
                {
                    MapPosition temp = new MapPosition(pos.x, pos.y);
                    Lnpoint.Add(temp);
                }
                if (cblengthcheck.Checked)
                {
                    MapPosition temp = new MapPosition(pos.x, pos.y);
                    LLength.Enqueue(temp);
                }
            }
        }
        private void btnInspCal_Click(object sender, EventArgs e)
        {
            double.TryParse(txtTileSizeX.Text, out double dTileSizeX);
            double.TryParse(txtTileSizeY.Text, out double dTileSizeY);
            if (dTileSizeX == 0 || dTileSizeY == 0)
            {
                MessageBox.Show("Input TileSize");
                return;
            }

            int camno = 0;
            if (rbR.Checked && Vision[1] != null) camno = 1;
            if (rbC.Checked && Vision[2] != null) camno = 2;
            Vision[camno].cogDS.InteractiveGraphics.Clear();
            Vision[camno].cogDS.StaticGraphics.Clear();
            List<cs2DAlign.ptXYT> lpoint = new List<cs2DAlign.ptXYT>();
            Vision[camno].PatternSearch_InspMFQ(ref lpoint, ePatternKind.CalX, out BitArray bitResult);

            List<string> data = new List<string>();
            bool bsuccess = Vision[camno].CheckerboardNpointCal(lpoint, dTileSizeX, dTileSizeY, ref data, Vision[camno].cogDS, Vision[camno].CFG.eCamName);
            Vision[camno].ImageSave(true, CONST.eImageSaveType.Display); //이미지 저장추가

            
        }
        private void tbTrace_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tbTrace.SelectedTab.Text)
            {
                case "Height":
                    {
                        pnHeight.BringToFront();
                        break;
                    }
                default:
                    {
                        pnHeight.SendToBack();
                        break;
                    }
            }
            btnAutoCreate.Visible = true;
            btnOffsetCreate.Visible = false;
        }

        private void btnCheckerCal_Click(object sender, EventArgs e)
        {
            bool b = false;
            if (cbFiducialMark.SelectedIndex < 0) cbFiducialMark.SelectedIndex = 0;
            double.TryParse(txtTileSize.Text, out double dsize);
            if (rbCheckerL.Checked)// && Vision[0].CFG.eCamName == nameof(eCamNO.UpperInsp1_1)) //pcy210203
            {
                b = Vision[0].CheckerboardCal(dsize, cbFiducialMark.SelectedIndex);
                if (b) Vision[0].CheckerboardImgSave(dsize, cbFiducialMark.SelectedIndex);
            }
            else if (rbCheckerR.Checked)// && Vision[1].CFG.eCamName == nameof(eCamNO.UpperInsp1_2)) //pcy210203
            {
                b = Vision[1].CheckerboardCal(dsize, cbFiducialMark.SelectedIndex);
                if (b) Vision[1].CheckerboardImgSave(dsize, cbFiducialMark.SelectedIndex);
            }
            if (b)
            {
                MessageBox.Show("Inspection CalOK");
            }
            else
            {
                MessageBox.Show("Cal NG");
            }
        }

        private void btntest_Click(object sender, EventArgs e)
        {
            if (cblengthcheck.Checked && LLength.Count > 1)
            {
                MapPosition pos1 = LLength.Dequeue();
                MapPosition pos2 = LLength.Dequeue();
                double dist = 0;
                double angle = 0;
                npoint.distance(Vision[0].cogDS.Image, pos1, pos2, ref dist, ref angle);
            }
            else
            {
                double.TryParse(txtSize.Text, out double d);
                npoint.NpointCal(Vision[0].cogDS.Image, Lnpoint, d);
            }
        }

        private void btnLNpointClear_Click(object sender, EventArgs e)
        {
            Lnpoint.Clear();
            LLength.Clear();
        }
        private void btnScaleTeachSave_Click(object sender, EventArgs e)
        {
            if (Menu.frmlogin.LogInCheck())
            {
                //pcy200609 Tranfer scale없으니 항상 1로 사용.
                int kind = cbBendSelect.SelectedIndex;
                kind = 1;

                eCalPos calNO1 = eCalPos.Err;
                eCalPos calNO2 = eCalPos.Err;

                int VisionNo = cbCamList.SelectedIndex;
                if (cbCamList.SelectedIndex == 1 || cbCamList.SelectedIndex == 3 || cbCamList.SelectedIndex == 5 || cbCamList.SelectedIndex == 7)
                {
                    VisionNo = cbCamList.SelectedIndex - 1;
                }

                Menu.frmAutoMain.GetCalPos(ref calNO1, VisionNo, kind);
                Menu.frmAutoMain.GetCalPos(ref calNO2, VisionNo + 1, kind);

                if (MessageBox.Show("Are you Sure Save?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    InspCalSave(calNO1, calNO2);
                    string str = "LX : " + tb_ScaleLX.Text + "LY : " + tb_ScaleLY.Text + "RX : " + tb_ScaleRX.Text + "RY : " + tb_ScaleRY.Text;
                    cLog.Save(LogKind.System, "Scale Manual Save " + str);
                }
                else
                {
                    MessageBox.Show("Scale Manual Save End");
                }
            }
            else
            {
                MessageBox.Show("Confrm LoginUser");
            }
        }

        private void btnTraceFileRead_Click(object sender, EventArgs e)
        {
            if (Menu.frmlogin.LogInCheck())
            {
                OpenFileDialog OF = new OpenFileDialog();
                OF.InitialDirectory = Path.Combine(CONST.cMotionTracePath + "\\" + CONST.RunRecipe.RecipeName);
                OF.DefaultExt = "*.csv"; // 기본 확장자 설정
                OF.Filter = "CSV files (*.csv)|*.csv"; //필터 확장자명
                OF.FilterIndex = 1; // 기본으로 선택되는 확장자 2로 하면 모든 파일로 됨
                if (OF.ShowDialog(this) == DialogResult.OK)
                {
                    MotionTraceDataRead(cbBending.SelectedIndex, OF.FileName);
                    CalcfactorAndView();
                    TeachtoGraph();
                }

            }
            else
            {
                MessageBox.Show("confrm LoginUser");
            }
        }
        private void btnInspPS_Click(object sender, EventArgs e)
        {
            int camno = 0;
            if (rbR.Checked && Vision[1] != null) camno = 1;
            if (rbC.Checked && Vision[2] != null) camno = 2;
            //double dR = 0;
            List<cs2DAlign.ptXYT> lpoint = new List<cs2DAlign.ptXYT>();
            Vision[camno].PatternSearch_InspMFQ(ref lpoint, ePatternKind.CalX, out BitArray bitResult);
        }
        public void SideViewer_Start(bool bFirst)
        {
            try
            {
                if (bFirst)
                {
                }
                else
                {
                    KillBendingSideViewer();
                    System.Diagnostics.ProcessStartInfo StartInfo = new System.Diagnostics.ProcessStartInfo();
                    StartInfo.WorkingDirectory = @CONST.Folder + "EQData\\BendingSideViewer";
                    StartInfo.FileName = "BendingSideViewer";
                    StartInfo.CreateNoWindow = false;
                    StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                    System.Diagnostics.Process.Start(StartInfo);
                }
            }
            catch { }
        }

        public void KillBendingSideViewer()
        {
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName.StartsWith("BendingSideViewer"))
                {
                    process.Kill();
                }
            }
        }
        public double CalcLcheckOffset(csVision vision, eCalPos calPos_L, eCalPos calPos_R, double dPixelX1, double dPixelY1, double dPixelX2, double dPixelY2)
        {
            cs2DAlign.ptXY p1 = new cs2DAlign.ptXY();
            cs2DAlign.ptXY p2 = new cs2DAlign.ptXY();
            p1.X = dPixelX1;
            p1.Y = dPixelY1;
            p2.X = dPixelX2;
            p2.Y = dPixelY2;

            int iSpec = 1;
            if (rbLcheck2.Checked) iSpec = 2;
            GetLcheckSpecTolerence(vision.CFG.eCamName, out double dSpec, out double dTolerence, iSpec);
            double length = 0;
            if (vision.CFG.eCamName == nameof(eCamNO.Laser1))
                length = Menu.rsAlign.getLength((int)calPos_L, (int)calPos_R, p1, p2, 0);
            else length = Menu.rsAlign.getLength((int)calPos_L, (int)calPos_R, p1, p2, 0);

            length = dSpec - length;
            return length;
        }
        public void GetLcheckSpecTolerence(string eCamName, out double dSpec, out double dTolerence, int iSpec = 1)
        {
            dSpec = -1;
            dTolerence = -1;
            switch (eCamName)
            {
                case nameof(eCamNO.LoadingPre1):
                case nameof(eCamNO.LoadingPre2):
                    dSpec = CONST.RunRecipe.Param[eRecipe.LOADING_PRE_LCHECK_SPEC1];
                    dTolerence = CONST.RunRecipe.Param[eRecipe.LOADING_PRE_LCHECK_TOLERANCE1];
                    if (iSpec == 2)
                    {
                        dSpec = CONST.RunRecipe.Param[eRecipe.LOADING_PRE_LCHECK_SPEC2];
                        dTolerence = CONST.RunRecipe.Param[eRecipe.LOADING_PRE_LCHECK_TOLERANCE2];
                    }
                    break;
            }
        }
        #region 20200908 cjm calibration 결과, 점 찍기
        private void btLoadData_Click(object sender, EventArgs e)
        {
            eCalType CalType = eCalType.Cam1Type;
            eCalPos calpos = eCalPos.Err;
            Change(cbCal, ref cbCamList, ref calpos);

            CalType = Vision[0].CFG.CalType;

            dgvClear();
            CalResutList(calpos, CalType); // CamList에 따른 Calibration 데이터 값 불러오기

            Live(false);

            CamImage(calpos, CalType);
        }


        public void Change(ComboBox cbCalList, ref ComboBox cbCamList, ref eCalPos calPos)
        {
            // 201029 cjm ComboBox에 따라서 CalibrationResult 불러오기 추가
            string sList = cbCalList.SelectedItem.ToString();
            int iCalNo = cbCalList.SelectedIndex;
            calPos = (eCalPos)iCalNo;
            cbCamList.SelectedItem = Vision[0].CFG.Name;
        }

        //20201003 cjm Calibration 이미지 불러오기 Path 변경
        public void CamImage(eCalPos calPos, eCalType calType)
        {
            string LoadImgPath = Path.Combine(CONST.Folder2, "EQData\\Calibration");
            LoadImgPath = Path.Combine(LoadImgPath, "CAL" + ((int)calPos + 1) + "Data");


            LoadImgPath = Path.Combine(LoadImgPath, "4.jpg");

            //20.10.07 lkw
            if (File.Exists(LoadImgPath))
            {
                Bitmap bmpTest = (Bitmap)Image.FromFile(LoadImgPath);  // 20200909 cjm 이미지 불러오기

                cogDS.Image = new CogImage8Grey(bmpTest);
                ImagePoint(cogDS, calPos);

                if (calType == eCalType.Cam2Type || calType == eCalType.Cam3Type || calType == eCalType.Cam4Type)
                {
                    LoadImgPath = Path.Combine(CONST.Folder2, "EQData\\Calibration");
                    LoadImgPath = Path.Combine(LoadImgPath, "CAL" + ((int)calPos + 2) + "Data");


                    LoadImgPath = Path.Combine(LoadImgPath, "4.jpg");


                    bmpTest = (Bitmap)Image.FromFile(LoadImgPath);  // 20200909 cjm 이미지 불러오기

                    cogDS2.Image = new CogImage8Grey(bmpTest);
                    ImagePoint(cogDS2, calPos);

                    if (calType == eCalType.Cam3Type || calType == eCalType.Cam4Type)
                    {
                        LoadImgPath = Path.Combine(CONST.Folder2, "EQData\\Calibration");
                        LoadImgPath = Path.Combine(LoadImgPath, "CAL" + ((int)calPos + 3) + "Data");


                        LoadImgPath = Path.Combine(LoadImgPath, "4.jpg");


                        bmpTest = (Bitmap)Image.FromFile(LoadImgPath);  // 20200909 cjm 이미지 불러오기
                        cogDS3.Image = new CogImage8Grey(bmpTest);
                        ImagePoint(cogDS3, calPos);

                        if (calType == eCalType.Cam4Type)
                        {
                            LoadImgPath = Path.Combine(CONST.Folder2, "EQData\\Calibration");
                            LoadImgPath = Path.Combine(LoadImgPath, "CAL" + ((int)calPos + 4) + "Data");


                            LoadImgPath = Path.Combine(LoadImgPath, "4.jpg");


                            bmpTest = (Bitmap)Image.FromFile(LoadImgPath);  // 20200909 cjm 이미지 불러오기
                            cogDS4.Image = new CogImage8Grey(bmpTest);
                            ImagePoint(cogDS4, calPos);
                        }
                    }
                }
            }
        }
        public void dgvClear()
        {
            dgvCalResult1_1.Rows.Clear(); // dgv 1_1번 초기화
            dgvCalResult1_2.Rows.Clear(); // dgv 1_2번 초기화
            dgvCalResult2_1.Rows.Clear(); // dgv 1_1번 초기화
            dgvCalResult2_2.Rows.Clear(); // dgv 1_2번 초기화
            dgvCalResult3_1.Rows.Clear(); // dgv 1_1번 초기화
            dgvCalResult3_2.Rows.Clear(); // dgv 1_2번 초기화
            dgvCalResult4_1.Rows.Clear(); // dgv 1_1번 초기화
            dgvCalResult4_2.Rows.Clear(); // dgv 1_2번 초기화
        }
        public void dgvCalResult(int i)
        {
            if (i == 1)
            {
                dgvCalResult1_1.Rows.Add(CONST.resultCal1);
                dgvCalResult1_1.Rows.Add(CONST.resultCal2);
                dgvCalResult1_1.Rows.Add(CONST.resultCal3);
                dgvCalResult1_1.Rows.Add(CONST.resultCal4);
                dgvCalResult1_2.Rows.Add(CONST.Pos);
                dgvCalResult1_2.Rows[0].Cells[2].Value = CONST.sx;
                dgvCalResult1_2.Rows[0].Cells[3].Value = CONST.sy;
                dgvCalResult1_2.Rows[0].Cells[4].Value = CONST.mFX;
                dgvCalResult1_2.Rows[0].Cells[5].Value = CONST.mFY;
            }
            else if (i == 2)
            {
                dgvCalResult2_1.Rows.Add(CONST.resultCal1);
                dgvCalResult2_1.Rows.Add(CONST.resultCal2);
                dgvCalResult2_1.Rows.Add(CONST.resultCal3);
                dgvCalResult2_1.Rows.Add(CONST.resultCal4);
                dgvCalResult2_2.Rows.Add(CONST.Pos);
                dgvCalResult2_2.Rows[0].Cells[2].Value = CONST.sx;
                dgvCalResult2_2.Rows[0].Cells[3].Value = CONST.sy;
                dgvCalResult2_2.Rows[0].Cells[4].Value = CONST.mFX;
                dgvCalResult2_2.Rows[0].Cells[5].Value = CONST.mFY;

            }
            else if (i == 3)
            {
                dgvCalResult3_1.Rows.Add(CONST.resultCal1);
                dgvCalResult3_1.Rows.Add(CONST.resultCal2);
                dgvCalResult3_1.Rows.Add(CONST.resultCal3);
                dgvCalResult3_1.Rows.Add(CONST.resultCal4);
                dgvCalResult3_2.Rows.Add(CONST.Pos);
                dgvCalResult3_2.Rows[0].Cells[2].Value = CONST.sx;
                dgvCalResult3_2.Rows[0].Cells[3].Value = CONST.sy;
                dgvCalResult3_2.Rows[0].Cells[4].Value = CONST.mFX;
                dgvCalResult3_2.Rows[0].Cells[5].Value = CONST.mFY;

            }
            else
            {
                dgvCalResult4_1.Rows.Add(CONST.resultCal1);
                dgvCalResult4_1.Rows.Add(CONST.resultCal2);
                dgvCalResult4_1.Rows.Add(CONST.resultCal3);
                dgvCalResult4_1.Rows.Add(CONST.resultCal4);
                dgvCalResult4_2.Rows.Add(CONST.Pos);
                dgvCalResult4_2.Rows[0].Cells[2].Value = CONST.sx;
                dgvCalResult4_2.Rows[0].Cells[3].Value = CONST.sy;
                dgvCalResult4_2.Rows[0].Cells[4].Value = CONST.mFX;
                dgvCalResult4_2.Rows[0].Cells[5].Value = CONST.mFY;
            }

        }

        public static string ResultSection1;
        public static string ResultSection2;
        public static string ResultSection3;
        public static string ResultSection4;

        public void CalResutList(eCalPos calPos, eCalType calType)
        {
            if (calType == eCalType.Cam1Type)
            {
                cam1type((int)calPos);
            }
            else if (calType == eCalType.Cam2Type)
            {
                cam2type((int)calPos, (int)calPos + 1);
            }
            else if (calType == eCalType.Cam3Type)
            {
                cam3type((int)calPos, (int)calPos + 1, (int)calPos + 2);
            }
            else if (calType == eCalType.Cam1Cal2)
            {
                cam1cal2((int)calPos);
            }
        }

        public void cam1type(int eCalPosNum1)
        {
            tbCalResult.TabPages.Clear();
            tbCalResult.TabPages.Add(tbResult_1);

            tbResult_1.Text = Vision[0].CFG.Name + "_1";

            ResultSection1 = "CAL" + (eCalPosNum1 + 1).ToString() + "Data";

            cCFG.CalResult(ResultSection1);
            dgvCalResult(1);
        }
        public void cam2type(int eCalPosNum1, int eCalPosNum2, int calkind = 0, int eCalPosNum3 = 0, int eCalPosNum4 = 0)
        {
            tbCalResult.TabPages.Clear();
            tbCalResult.TabPages.Add(tbResult_1);
            tbCalResult.TabPages.Add(tbResult_2);
            //tbCalResult.TabPages.Remove(tbResult_3);
            //tbCalResult.TabPages.Remove(tbResult_4);

            tbResult_1.Text = Vision[0].CFG.Name;
            tbResult_2.Text = Vision[1].CFG.Name;
            //20200926 cjm Calkind 선택 안할 시 값 -1 나옴, 수정

            ResultSection1 = "CAL" + (eCalPosNum1 + 1).ToString() + "Data";
            ResultSection2 = "CAL" + (eCalPosNum2 + 1).ToString() + "Data";


            cCFG.CalResult(ResultSection1);
            dgvCalResult(1);

            cCFG.CalResult(ResultSection2);
            dgvCalResult(2);
        }
        public void cam3type(int eCalPosNum1, int eCalPosNum2, int eCalPosNum3)
        {
            tbCalResult.TabPages.Clear();
            tbCalResult.TabPages.Add(tbResult_1);
            tbCalResult.TabPages.Add(tbResult_2);
            tbCalResult.TabPages.Add(tbResult_3);
            //tbCalResult.TabPages.Remove(tbResult_4);

            tbResult_1.Text = Vision[0].CFG.Name;
            tbResult_2.Text = Vision[1].CFG.Name;
            tbResult_3.Text = Vision[2].CFG.Name;

            ResultSection1 = "CAL" + (eCalPosNum1 + 1).ToString() + "Data";
            ResultSection2 = "CAL" + (eCalPosNum2 + 1).ToString() + "Data";
            ResultSection3 = "CAL" + (eCalPosNum3 + 1).ToString() + "Data";

            cCFG.CalResult(ResultSection1);
            dgvCalResult(1);

            cCFG.CalResult(ResultSection2);
            dgvCalResult(2);

            cCFG.CalResult(ResultSection3);
            dgvCalResult(3);
        }
        public void cam4type(int eCalPosNum1, int eCalPosNum2, int eCalPosNum3, int eCalPosNum4)
        {
            tbCalResult.TabPages.Clear();
            tbCalResult.TabPages.Add(tbResult_1);
            tbCalResult.TabPages.Add(tbResult_2);
            tbCalResult.TabPages.Add(tbResult_3);
            tbCalResult.TabPages.Add(tbResult_4);

            tbResult_1.Text = Vision[0].CFG.Name;
            tbResult_2.Text = Vision[1].CFG.Name;
            tbResult_3.Text = Vision[2].CFG.Name;
            tbResult_4.Text = Vision[3].CFG.Name;

            ResultSection1 = "CAL" + (eCalPosNum1 + 1).ToString() + "Data";
            ResultSection2 = "CAL" + (eCalPosNum2 + 1).ToString() + "Data";
            ResultSection3 = "CAL" + (eCalPosNum3 + 1).ToString() + "Data";
            ResultSection4 = "CAL" + (eCalPosNum4 + 1).ToString() + "Data";

            cCFG.CalResult(ResultSection1);
            dgvCalResult(1);

            cCFG.CalResult(ResultSection2);
            dgvCalResult(2);

            cCFG.CalResult(ResultSection3);
            dgvCalResult(3);

            cCFG.CalResult(ResultSection4);
            dgvCalResult(4);
        }
        public void cam1cal2(int eCalPosNum1)
        {
            tbCalResult.TabPages.Clear();
            tbCalResult.TabPages.Add(tbResult_1);
            tbCalResult.TabPages.Add(tbResult_2);

            tbResult_1.Text = Vision[0].CFG.Name + "_1";
            tbResult_2.Text = Vision[0].CFG.Name + "_2";

            ResultSection1 = "CAL" + (eCalPosNum1 + 1).ToString() + "Data";
            ResultSection2 = "CAL" + (eCalPosNum1 + 2).ToString() + "Data";

            cCFG.CalResult(ResultSection1);
            dgvCalResult(1);

            cCFG.CalResult(ResultSection2);
            dgvCalResult(2);
        }

        // 20200911 cjm Calibration 움직이는 점 저장하는 곳
        //20201003 cjm cal 저장하는 곳
        public void CalibrationPointSave(List<cs2DAlign.ptXY> YTCalPosCamNum, List<cs2DAlign.ptXY> CalPosCamNum, rs2DAlign.cs2DAlign.ptXY[] fixPosCamNum, eCalPos calPos, double calX, double calY, double calT)
        {
            //string[] array = new string[22]; // Calibration 9점 X,Y 각 2개씩 총 18개와 Theta 2점 X,Y 각 2개씩 총 4개를 합쳐서 22개
            List<string> array = new List<string>(); //pcy201022 list로 변경
            string SaveLog = "";

            // Calibration 9점 X,Y 각 2개씩 총 18개 찍는 곳
            if (YTCal)
            {
                for (int i = 0; i < YTCalPosCamNum.Count; i++)
                {
                    array.Add(YTCalPosCamNum[i].X.ToString());
                    array.Add(YTCalPosCamNum[i].Y.ToString());
                }
            }
            else
            {
                for (int i = 0; i < CalPosCamNum.Count; i++)
                {
                    array.Add(CalPosCamNum[i].X.ToString());
                    array.Add(CalPosCamNum[i].Y.ToString());
                }
            }

            array.Add(fixPosCamNum[0].X.ToString());
            array.Add(fixPosCamNum[0].Y.ToString());
            array.Add(fixPosCamNum[1].X.ToString());
            array.Add(fixPosCamNum[1].Y.ToString());

            //pcy201022 cal이동량 로그 추가
            array.Add(calX.ToString());
            array.Add(calY.ToString());
            array.Add(calT.ToString());

            string Point = string.Join(",", array);
            SaveLog = "CAL" + ((int)calPos + 1) + "Data" + "," + Point;

            cLog.Save(LogKind.CalibrationResult, SaveLog);
        }
        //20200924 cjm Cam1Cal2 찍는 방식 변경
        //20201003 cjm cal 저장하는 곳
        public void ImagePoint(CogDisplay cogdis, eCalPos calPos)
        {
            string[] Sprit = null;
            int iline = -1;
            if (Vision[0].CFG.CalType == eCalType.Cam1Cal2)
                iline = -2;
            else if (Vision[0].CFG.CalType == eCalType.Cam2Type)
            {
                if (cogdis == cogDS)
                    iline = -2;
                else if (cogdis == cogDS2)
                    iline = -1;
            }
            else if (Vision[0].CFG.CalType == eCalType.Cam3Type)
            {
                if (cogdis == cogDS)
                    iline = -3;
                else if (cogdis == cogDS2)
                    iline = -2;
                else
                    iline = -1;
            }
            else if (Vision[0].CFG.CalType == eCalType.Cam4Type)
            {
                if (cogdis == cogDS)
                    iline = -4;
                else if (cogdis == cogDS2)
                    iline = -3;
                else if (cogdis == cogDS3)
                    iline = -2;
                else
                    iline = -1;
            }
            try
            {
                //cLog.CalibrationPointReader(cbCamList.SelectedItem.ToString(), ref Sprit, iline, calKind);
                //20201003 cjm cal 저장하는 곳
                cLog.CalibrationPointReader((int)calPos + 1, ref Sprit, iline);

                // 20200928 cjm 9Point 와 Theta 색 구분
                CogPointMarker p = new CogPointMarker();
                p.SizeInScreenPixels = 50;
                for (int i = 0; i < 9; i++)
                {
                    p.X = double.Parse(Sprit[i * 2 + 2]);
                    p.Y = double.Parse(Sprit[i * 2 + 3]);

                    cogdis.StaticGraphics.Add(p, "calPoint");
                }

                CogPointMarker pp = new CogPointMarker();
                pp.Color = CogColorConstants.Orange;
                pp.SizeInScreenPixels = 50;
                for (int i = 9; i < 11; i++)
                {
                    pp.X = double.Parse(Sprit[i * 2 + 2]);
                    pp.Y = double.Parse(Sprit[i * 2 + 3]);

                    cogdis.StaticGraphics.Add(pp, "calPoint");
                }

                if (Vision[0].CFG.CalType == eCalType.Cam1Cal2)
                {
                    int a = (int)Vision[0].CFG.CalType;
                    //cLog.CalibrationPointReader(cbCamList.SelectedItem.ToString(), ref Sprit, -1, calKind);
                    //20201003 cjm cal 저장하는 곳
                    cLog.CalibrationPointReader((int)calPos + 1, ref Sprit, -1);

                    CogPointMarker p2 = new CogPointMarker();
                    p2.SizeInScreenPixels = 50;
                    for (int j = 0; j < 9; j++)
                    {
                        p2.X = double.Parse(Sprit[j * 2 + 2]);
                        p2.Y = double.Parse(Sprit[j * 2 + 3]);

                        cogdis.StaticGraphics.Add(p2, "calPoint");
                    }

                    CogPointMarker pp2 = new CogPointMarker();
                    pp2.Color = CogColorConstants.Orange;
                    pp2.SizeInScreenPixels = 50;
                    for (int j = 9; j < 11; j++)
                    {
                        pp2.X = double.Parse(Sprit[j * 2 + 2]);
                        pp2.Y = double.Parse(Sprit[j * 2 + 3]);

                        cogdis.StaticGraphics.Add(pp2, "calPoint");
                    }
                }
            }
            catch
            {

            }

        }
        #endregion

        private void btnLightSave_Click(object sender, EventArgs e)
        {
            string lcText = (sender as Button).Name;
            for (short i = 0; i < btnLightSave.Length; i++)
            {
                if (lcText == btnLightSave[i].Name)
                {
                    //저장부
                    string _modelName = CONST.RunRecipe.RecipeName;
                    int camno = cbCamList.SelectedIndex;
                    int ISLight = Menu.frmAutoMain.ISLight[Vision[i].CFG.Light1Comport, Vision[i].CFG.Light1CH];
                    string Section = "Light";

                    if (Enum.TryParse<ePatternKind>(cbRecognition.SelectedValue.ToString(), out ePatternKind status1))
                    {
                        Vision[i].CFG.Exposure[(int)cbRecognition.SelectedIndex] = Vision[i].ISExposure;
                        Vision[i].CFG.Contrast[(int)cbRecognition.SelectedIndex] = Vision[i].ISContrast;
                        Vision[i].CFG.Light[(int)cbRecognition.SelectedIndex] = ISLight;
                        Menu.frmSetting.revData.WriteValue(_modelName, Section, "CAM" + (camno + i).ToString() + ".Light." + cbRecognition.SelectedItem, ISLight);
                        Menu.frmSetting.revData.WriteValue(_modelName, Section, "CAM" + (camno + i).ToString() + ".Exposure." + cbRecognition.SelectedItem, Vision[i].ISExposure);
                        Menu.frmSetting.revData.WriteValue(_modelName, Section, "CAM" + (camno + i).ToString() + ".Contrast." + cbRecognition.SelectedItem, Vision[i].ISContrast);
                    }
                    else if (Enum.TryParse<eLineKind>(cbRecognition.SelectedValue.ToString(), out eLineKind status2))
                    {
                        Vision[i].CFG.LineExposure[(int)cbRecognition.SelectedIndex] = Vision[i].ISExposure;
                        Vision[i].CFG.LineContrast[(int)cbRecognition.SelectedIndex] = Vision[i].ISContrast;
                        Vision[i].CFG.LineLight[(int)cbRecognition.SelectedIndex] = ISLight;
                        Menu.frmSetting.revData.WriteValue(_modelName, Section, "CAM" + (camno + i).ToString() + ".LineLight." + cbRecognition.SelectedItem, ISLight);
                        Menu.frmSetting.revData.WriteValue(_modelName, Section, "CAM" + (camno + i).ToString() + ".LineExposure." + cbRecognition.SelectedItem, Vision[i].ISExposure);
                        Menu.frmSetting.revData.WriteValue(_modelName, Section, "CAM" + (camno + i).ToString() + ".LineContrast." + cbRecognition.SelectedItem, Vision[i].ISContrast);
                    }
                    ViewCurrentValue(i);
                }
            }
            SetLightDisplay(cbRecognition);
        }

        private void rbPattern_CheckedChanged(object sender, EventArgs e)
        {
            if (rbPattern.Checked)
            {
                cbRecognition.DataSource = Enum.GetValues(typeof(ePatternKind));
            }
            else if (rbLine.Checked)
            {
                cbRecognition.DataSource = Enum.GetValues(typeof(eLineKind));
            }
        }

        private void cbRecognition_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < Vision.Length; i++)
            {
                if (Vision[i] != null)
                    ViewCurrentValue(i);
            }

            SetLightDisplay(cbRecognition);
            SetTargetText(Vision);
            SetTargetCogGraphic(Vision);
        }

        public void SetLightDisplay(ComboBox cbRecognition)
        {
            int idata = -1;
            if (Enum.TryParse<ePatternKind>(cbRecognition.SelectedValue.ToString(), out ePatternKind status1))
                idata = 0;
            else if (Enum.TryParse<eLineKind>(cbRecognition.SelectedValue.ToString(), out eLineKind status2))
                idata = 1;

            for (int i = 0; i < Vision.Length; i++)
            {
                if (Vision[i] != null)
                {
                    Vision[i].cogDS.StaticGraphics.Clear();
                    if (idata == 0)
                    {
                        txtIsExposure[i].Text = Vision[i].CFG.Exposure[(int)cbRecognition.SelectedIndex].ToString();
                        txtIsContrast[i].Text = Vision[i].CFG.Contrast[(int)cbRecognition.SelectedIndex].ToString();
                        txtIsLight[i].Text = Vision[i].CFG.Light[(int)cbRecognition.SelectedIndex].ToString();
                    }
                    else if (idata == 1)
                    {
                        txtIsExposure[i].Text = Vision[i].CFG.LineExposure[(int)cbRecognition.SelectedIndex].ToString();
                        txtIsContrast[i].Text = Vision[i].CFG.LineContrast[(int)cbRecognition.SelectedIndex].ToString();
                        txtIsLight[i].Text = Vision[i].CFG.LineLight[(int)cbRecognition.SelectedIndex].ToString();
                    }

                }
            }
        }

        private void btnLCheckOffset_Click(object sender, EventArgs e)
        {
            //cam2type과 cam1cal2type만 작성함
            listAlignResult.Items.Clear();
            eCalType CalType = Vision[0].CFG.CalType;
            bool b1 = false;
            bool b2 = false;
            //20.10.11 lkw
            if (CalType == eCalType.Cam2Type || CalType == eCalType.Cam3Type)
            {
                if (rbLcheck1.Checked) //panel
                {
                    b1 = Vision[0].PatternSearchEnum(ref dPixelX1, ref dPixelY1, ref dTheta1, ePatternKind.Panel);
                    b2 = Vision[1].PatternSearchEnum(ref dPixelX2, ref dPixelY2, ref dTheta2, ePatternKind.Panel);
                }
                else if (rbLcheck2.Checked) //fpc
                {
                    b1 = Vision[0].PatternSearchEnum(ref dPixelX1, ref dPixelY1, ref dTheta1, ePatternKind.FPC);
                    b2 = Vision[1].PatternSearchEnum(ref dPixelX2, ref dPixelY2, ref dTheta2, ePatternKind.FPC);
                }
            }
            if (CalType == eCalType.Cam1Cal2)
            {
                Vision[0].Capture(false, true, false, true);
                b1 = Vision[0].PatternSearchEnum(ref dPixelX1, ref dPixelY1, ref dTheta1, ePatternKind.Left_1cam);
                b2 = Vision[0].PatternSearchEnum(ref dPixelX2, ref dPixelY2, ref dTheta2, ePatternKind.Right_1cam);
            }

            if (b1 && b2)
            {
                if (!cbBendSelect.Visible)
                {
                    Menu.frmAutoMain.GetCalPos(ref calPos_L, Vision[0].CFG.Camno, 0);
                    if (CalType == eCalType.Cam1Cal2) Menu.frmAutoMain.GetCalPos(ref calPos_R, Vision[0].CFG.Camno, 1, true);
                    else Menu.frmAutoMain.GetCalPos(ref calPos_R, Vision[1].CFG.Camno, 0);
                }
                else
                {
                    if (cbBendSelect.SelectedIndex < 0) cbBendSelect.SelectedIndex = 0;
                    Menu.frmAutoMain.GetCalPos(ref calPos_L, Vision[0].CFG.Camno, cbBendSelect.SelectedIndex);
                    Menu.frmAutoMain.GetCalPos(ref calPos_R, Vision[0].CFG.Camno, cbBendSelect.SelectedIndex, true);

                    
                }
                double length = CalcLcheckOffset(Vision[0], calPos_L, calPos_R, dPixelX1, dPixelY1, dPixelX2, dPixelY2);

                listAlignResult.Items.Add("Calc Lcheck Offset " + length.ToString("0.0000"));
                //20.10.11 lkw
                if (MessageBox.Show("L-Check Offset Save!", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    if (rbLcheck1.Checked) Menu.frmSetting.revData.mLcheck[Vision[0].CFG.Camno].LCheckOffset1 = length;
                    else if (rbLcheck2.Checked) Menu.frmSetting.revData.mLcheck[Vision[0].CFG.Camno].LCheckOffset2 = length;
                    Menu.frmSetting.revData.WriteData(CONST.RunRecipe.RecipeName);
                }
            }
            else
            {
                listAlignResult.Items.Add("Mark Find Fail");
            }
        }
        // 20200926 cjm Bending Pre SCF Inspection Offset 추가
        public void SCFInspOffset()
        {
            if (CONST.PCNo == 1)
            {
                if (Vision[0].CFG.Name == "BENDING PRE1")
                {
                    gbSCFInspOffsetbox.Visible = true;
                    revDataTotxt();
                }
                else
                    gbSCFInspOffsetbox.Visible = false;
            }
            else
                gbSCFInspOffsetbox.Visible = false;

            if (Vision[0].CFG.CalType == eCalType.Cam2Type)
                gbCam3List.Visible = false;
            else if (Vision[0].CFG.CalType == eCalType.Cam3Type)
                gbCam3List.Visible = true;

        }
        private eCalPos ChangeAttachtoAttachInsp(eCalPos from)
        {
            return from;
        }
        private void btnOffsetWrite_Click(object sender, EventArgs e)
        {
            txtTorevData();
            Menu.frmSetting.revData.WriteData(CONST.RunRecipe.RecipeName);
        }

        public void txtTorevData()
        {
            //recipe화면 세팅값 옮기기
            Menu.frmSetting.revData.mAttach.DetachOffsetX1_1 = double.Parse(txtCam1OffsetX_Left.Text);
            Menu.frmSetting.revData.mAttach.DetachOffsetY1_1 = double.Parse(txtCam1OffsetY_Left.Text);
            Menu.frmSetting.revData.mAttach.DetachOffsetX1_2 = double.Parse(txtCam2OffsetX_Left.Text);
            Menu.frmSetting.revData.mAttach.DetachOffsetY1_2 = double.Parse(txtCam2OffsetY_Left.Text);
            Menu.frmSetting.revData.mAttach.DetachLimitTH1_1 = double.Parse(txtCam1TH_Left.Text);
            Menu.frmSetting.revData.mAttach.DetachLimitTH1_2 = double.Parse(txtCam2TH_Left.Text);

            Menu.frmSetting.revData.mAttach.DetachOffsetX2_1 = double.Parse(txtCam1OffsetX_Right.Text);
            Menu.frmSetting.revData.mAttach.DetachOffsetY2_1 = double.Parse(txtCam1OffsetY_Right.Text);
            Menu.frmSetting.revData.mAttach.DetachOffsetX2_2 = double.Parse(txtCam2OffsetX_Right.Text);
            Menu.frmSetting.revData.mAttach.DetachOffsetY2_2 = double.Parse(txtCam2OffsetY_Right.Text);
            Menu.frmSetting.revData.mAttach.DetachLimitTH2_1 = double.Parse(txtCam1TH_Right.Text);
            Menu.frmSetting.revData.mAttach.DetachLimitTH2_2 = double.Parse(txtCam2TH_Right.Text);

            Menu.frmSetting.revData.mLoadPreInsp.DetachOffsetX1 = double.Parse(txtDetachInspOffsetX1.Text);
            Menu.frmSetting.revData.mLoadPreInsp.DetachOffsetY1 = double.Parse(txtDetachInspOffsetY1.Text);
            Menu.frmSetting.revData.mLoadPreInsp.DetachLimitTH1 = double.Parse(txtDetachInspLimitTH1.Text);
            Menu.frmSetting.revData.mLoadPreInsp.DetachOffsetX2 = double.Parse(txtDetachInspOffsetX2.Text);
            Menu.frmSetting.revData.mLoadPreInsp.DetachOffsetY2 = double.Parse(txtDetachInspOffsetY2.Text);
            Menu.frmSetting.revData.mLoadPreInsp.DetachLimitTH2 = double.Parse(txtDetachInspLimitTH2.Text);

            Menu.frmSetting.revData.mLoadPreInsp.DetachOffsetX3 = double.Parse(txtDetachInspOffsetX3.Text);
            Menu.frmSetting.revData.mLoadPreInsp.DetachOffsetY3 = double.Parse(txtDetachInspOffsetY3.Text);
            Menu.frmSetting.revData.mLoadPreInsp.DetachLimitTH3 = double.Parse(txtDetachInspLimitTH3.Text);
            Menu.frmSetting.revData.mLoadPreInsp.DetachOffsetX4 = double.Parse(txtDetachInspOffsetX4.Text);
            Menu.frmSetting.revData.mLoadPreInsp.DetachOffsetY4 = double.Parse(txtDetachInspOffsetY4.Text);
            Menu.frmSetting.revData.mLoadPreInsp.DetachLimitTH4 = double.Parse(txtDetachInspLimitTH4.Text);

            Menu.frmSetting.revData.mLoadPreInsp.AttachOffsetX1 = double.Parse(txtFoamExistX1.Text);
            Menu.frmSetting.revData.mLoadPreInsp.AttachOffsetY1 = double.Parse(txtFoamExistY1.Text);
            Menu.frmSetting.revData.mLoadPreInsp.AttachLimitTH1 = double.Parse(txtFoamExistTH1.Text);
            Menu.frmSetting.revData.mLoadPreInsp.AttachOffsetX2 = double.Parse(txtFoamExistX2.Text);
            Menu.frmSetting.revData.mLoadPreInsp.AttachOffsetY2 = double.Parse(txtFoamExistY2.Text);
            Menu.frmSetting.revData.mLoadPreInsp.AttachLimitTH2 = double.Parse(txtFoamExistTH2.Text);

            Menu.frmSetting.revData.mLoadPreInsp.AttachOffsetX3 = double.Parse(txtFoamExistX3.Text);
            Menu.frmSetting.revData.mLoadPreInsp.AttachOffsetY3 = double.Parse(txtFoamExistY3.Text);
            Menu.frmSetting.revData.mLoadPreInsp.AttachLimitTH3 = double.Parse(txtFoamExistTH3.Text);
            Menu.frmSetting.revData.mLoadPreInsp.AttachOffsetX4 = double.Parse(txtFoamExistX4.Text);
            Menu.frmSetting.revData.mLoadPreInsp.AttachOffsetY4 = double.Parse(txtFoamExistY4.Text);
            Menu.frmSetting.revData.mLoadPreInsp.AttachLimitTH4 = double.Parse(txtFoamExistTH4.Text);

            //tt20210409 Check White/ Black add option
            if (cbCheckWhiteDetach1.Checked) Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteDetach1 = true; else Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteDetach1 = false;
            if (cbCheckWhiteDetach2.Checked) Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteDetach2 = true; else Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteDetach2 = false;
            if (cbCheckWhiteDetach3.Checked) Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteDetach3 = true; else Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteDetach3 = false;
            if (cbCheckWhiteDetach4.Checked) Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteDetach4 = true; else Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteDetach4 = false;

            if (cbCheckWhiteAttach1.Checked) Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteAttach1 = true; else Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteAttach1 = false;
            if (cbCheckWhiteAttach2.Checked) Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteAttach2 = true; else Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteAttach2 = false;
            if (cbCheckWhiteAttach3.Checked) Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteAttach3 = true; else Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteAttach3 = false;
            if (cbCheckWhiteAttach4.Checked) Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteAttach4 = true; else Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteAttach4 = false;
        }

        public void revDataTotxt()
        {
            txtCam1OffsetX_Left.Text = Menu.frmSetting.revData.mAttach.DetachOffsetX1_1.ToString("0.0");
            txtCam1OffsetY_Left.Text = Menu.frmSetting.revData.mAttach.DetachOffsetY1_1.ToString("0.0");
            txtCam2OffsetX_Left.Text = Menu.frmSetting.revData.mAttach.DetachOffsetX1_2.ToString("0.0");
            txtCam2OffsetY_Left.Text = Menu.frmSetting.revData.mAttach.DetachOffsetY1_2.ToString("0.0");
            txtCam1TH_Left.Text = Menu.frmSetting.revData.mAttach.DetachLimitTH1_1.ToString("0");
            txtCam2TH_Left.Text = Menu.frmSetting.revData.mAttach.DetachLimitTH1_2.ToString("0");

            txtCam1OffsetX_Right.Text = Menu.frmSetting.revData.mAttach.DetachOffsetX2_1.ToString("0.0");
            txtCam1OffsetY_Right.Text = Menu.frmSetting.revData.mAttach.DetachOffsetY2_1.ToString("0.0");
            txtCam2OffsetX_Right.Text = Menu.frmSetting.revData.mAttach.DetachOffsetX2_2.ToString("0.0");
            txtCam2OffsetY_Right.Text = Menu.frmSetting.revData.mAttach.DetachOffsetY2_2.ToString("0.0");
            txtCam1TH_Right.Text = Menu.frmSetting.revData.mAttach.DetachLimitTH2_1.ToString("0");
            txtCam2TH_Right.Text = Menu.frmSetting.revData.mAttach.DetachLimitTH2_2.ToString("0");


            txtDetachInspOffsetX1.Text = Menu.frmSetting.revData.mLoadPreInsp.DetachOffsetX1.ToString("0.0");
            txtDetachInspOffsetY1.Text = Menu.frmSetting.revData.mLoadPreInsp.DetachOffsetY1.ToString("0.0");
            txtDetachInspLimitTH1.Text = Menu.frmSetting.revData.mLoadPreInsp.DetachLimitTH1.ToString("0.0");
            txtDetachInspOffsetX2.Text = Menu.frmSetting.revData.mLoadPreInsp.DetachOffsetX2.ToString("0.0");
            txtDetachInspOffsetY2.Text = Menu.frmSetting.revData.mLoadPreInsp.DetachOffsetY2.ToString("0.0");
            txtDetachInspLimitTH2.Text = Menu.frmSetting.revData.mLoadPreInsp.DetachLimitTH2.ToString("0.0");

            txtDetachInspOffsetX3.Text = Menu.frmSetting.revData.mLoadPreInsp.DetachOffsetX3.ToString("0.0");
            txtDetachInspOffsetY3.Text = Menu.frmSetting.revData.mLoadPreInsp.DetachOffsetY3.ToString("0.0");
            txtDetachInspLimitTH3.Text = Menu.frmSetting.revData.mLoadPreInsp.DetachLimitTH3.ToString("0.0");
            txtDetachInspOffsetX4.Text = Menu.frmSetting.revData.mLoadPreInsp.DetachOffsetX4.ToString("0.0");
            txtDetachInspOffsetY4.Text = Menu.frmSetting.revData.mLoadPreInsp.DetachOffsetY4.ToString("0.0");
            txtDetachInspLimitTH4.Text = Menu.frmSetting.revData.mLoadPreInsp.DetachLimitTH4.ToString("0.0");


            txtFoamExistX1.Text = Menu.frmSetting.revData.mLoadPreInsp.AttachOffsetX1.ToString("0.0");
            txtFoamExistY1.Text = Menu.frmSetting.revData.mLoadPreInsp.AttachOffsetY1.ToString("0.0");
            txtFoamExistTH1.Text = Menu.frmSetting.revData.mLoadPreInsp.AttachLimitTH1.ToString("0.0");
            txtFoamExistX2.Text = Menu.frmSetting.revData.mLoadPreInsp.AttachOffsetX2.ToString("0.0");
            txtFoamExistY2.Text = Menu.frmSetting.revData.mLoadPreInsp.AttachOffsetY2.ToString("0.0");
            txtFoamExistTH2.Text = Menu.frmSetting.revData.mLoadPreInsp.AttachLimitTH2.ToString("0.0");

            txtFoamExistX3.Text = Menu.frmSetting.revData.mLoadPreInsp.AttachOffsetX3.ToString("0.0");
            txtFoamExistY3.Text = Menu.frmSetting.revData.mLoadPreInsp.AttachOffsetY3.ToString("0.0");
            txtFoamExistTH3.Text = Menu.frmSetting.revData.mLoadPreInsp.AttachLimitTH3.ToString("0.0");
            txtFoamExistX4.Text = Menu.frmSetting.revData.mLoadPreInsp.AttachOffsetX4.ToString("0.0");
            txtFoamExistY4.Text = Menu.frmSetting.revData.mLoadPreInsp.AttachOffsetY4.ToString("0.0");
            txtFoamExistTH4.Text = Menu.frmSetting.revData.mLoadPreInsp.AttachLimitTH4.ToString("0.0");

            //tt20210409
            cbCheckWhiteDetach1.Checked = Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteDetach1;
            cbCheckWhiteDetach2.Checked = Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteDetach2;
            cbCheckWhiteDetach3.Checked = Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteDetach3;
            cbCheckWhiteDetach4.Checked = Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteDetach4;

            cbCheckWhiteAttach1.Checked = Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteAttach1;
            cbCheckWhiteAttach2.Checked = Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteAttach2;
            cbCheckWhiteAttach3.Checked = Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteAttach3;
            cbCheckWhiteAttach4.Checked = Menu.frmSetting.revData.mLoadPreInsp.CheckWhiteAttach4;
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            string lcText = (sender as Button).Name;
            for (short i = 0; i < btnSet.Length; i++)
            {
                if (lcText == btnSet[i].Name)
                {
                    if (Vision[i] != null) //아마 null인경우는 없겟지만..
                    {
                        if (int.TryParse(txtIsLight[i].Text, out int setValue))
                        {
                            tb[i].Value = setValue;
                            Menu.frmAutoMain.SetLight(Vision[i].CFG.Light1Comport, Vision[i].CFG.Light1CH, setValue, Vision[i].CFG.Camno, Vision[i].CFG.LightType);
                        }
                        //Set
                        Vision[i].setContrast(double.Parse(txtIsContrast[i].Text), out bool bcontlow);
                        Vision[i].setExposure(int.Parse(txtIsExposure[i].Text), out bool bexplow);

                        ViewCurrentValue(i);
                    }
                }
            }
        }

        private void cbCalImageTest_CheckedChanged(object sender, EventArgs e)
        {
            foreach (var s in Vision)
            {
                if (s != null)
                    s.bcali = !cbCalImageTest.Checked;
            }
        }
        

        private void btnDetachInspection_Click(object sender, EventArgs e)
        {
            
        }
        public bool DetachInspection(eDetachInspPosition detachPos, ref int ResultTH, ref int attachResultTH, ref double LimitTH, ref double attachLimitTH, params csVision[] Vision)
        {
            cs2DAlign.ptXYT Mark1 = new cs2DAlign.ptXYT();
            bool bMark = false;
            if (detachPos == eDetachInspPosition.DPre1 || detachPos == eDetachInspPosition.DPre2 || detachPos == eDetachInspPosition.DPre3 || detachPos == eDetachInspPosition.DPre4) bMark = true;
            else bMark = Vision[0].PatternSearchEnum(ref Mark1.X, ref Mark1.Y, ref Mark1.T, ePatternKind.Panel);
            if (bMark)
            {
                ResultTH = 0;
                LimitTH = 0;
                attachResultTH = 0;
                //double size = 1;

                sDetachParam detachParam = new sDetachParam();
                sDetachParam foamExistParam = new sDetachParam();

                //Menu.frmAutoMain.getDetachOffset(detachPos, ref detachParam, ref foamExistParam);


                bool bResult = Vision[0].DetachInspection(Mark1.X, Mark1.Y, detachParam.OffsetX, detachParam.OffsetY, detachParam.LimitTH, ref ResultTH, detachParam.size, Mark1.T, !detachParam.checkWhite);
                bool bResult2 = true;

                if (detachPos == eDetachInspPosition.DPre1 || detachPos == eDetachInspPosition.DPre2 || detachPos == eDetachInspPosition.DPre3 || detachPos == eDetachInspPosition.DPre4)
                    bResult2 = Vision[0].DetachInspection(Mark1.X, Mark1.Y, foamExistParam.OffsetX, foamExistParam.OffsetY, foamExistParam.LimitTH, ref attachResultTH, foamExistParam.size, Mark1.T, !foamExistParam.checkWhite, 2);

                LimitTH = detachParam.LimitTH;
                attachLimitTH = foamExistParam.LimitTH;

                return bResult & bResult2;
            }
            else
            {
                return false;
            }
        }

        private void btnDetachInspection2_Click(object sender, EventArgs e)
        {
            //if (Vision[0].CFG.eCamName == nameof(eCamNO.LoadingPre1_1))
            //{
            //    int ResultTH = 0;
            //    double LimitTH = 0;
            //    bool b = DetachInspection(1, ref ResultTH, ref LimitTH, Vision[0]);

            //    listAlignResult.Items.Clear();
            //    if (b) listAlignResult.Items.Add("Detach Inspection OK");
            //    else listAlignResult.Items.Add("Detach Inspection NG");
            //    listAlignResult.Items.Add("Threshold :" + ResultTH.ToString() + " Limit :" + LimitTH.ToString());

            //    //listAlignResult.Items.Clear();

            //    //txtInspX1.Text = dist.X1.ToString("0.000");
            //    //txtInspX2.Text = dist.X2.ToString("0.000");
            //    //txtInspY1.Text = dist.Y1.ToString("0.000");
            //    //txtInspY2.Text = dist.Y2.ToString("0.000");
            //}
        }

        //MCR Search 추가 2021/06/19
        private void btnMCRSearch_Click(object sender, EventArgs e)
        {
            if (Vision[0] == null)
            {
                MessageBox.Show("CamName is not selected!");
                return;
            }

            
            if (functionInterlock())
            {
                cogDS.InteractiveGraphics.Clear();
                listAlignResult.Items.Clear();
                if (Vision[0].CFG.eCamName == nameof(eCamNO.Laser1) || Vision[0].CFG.eCamName == nameof(eCamNO.Laser2))
                {
                    string id = "";
                    cs2DAlign.ptXY[] codePoint = new cs2DAlign.ptXY[4];
                    if (Vision[0].readID(ref id, ref codePoint, Menu.frmSetting.revData.mLaser.MCRSearchKind, cogrectangle))
                    {
                        listAlignResult.Items.Add(id);

                        listAlignResult.Items.Add(codePoint[0].X.ToString("0.000") + "," + codePoint[0].Y.ToString("0.000"));
                        listAlignResult.Items.Add(codePoint[1].X.ToString("0.000") + "," + codePoint[1].Y.ToString("0.000"));
                        listAlignResult.Items.Add(codePoint[2].X.ToString("0.000") + "," + codePoint[2].Y.ToString("0.000"));
                        listAlignResult.Items.Add(codePoint[3].X.ToString("0.000") + "," + codePoint[3].Y.ToString("0.000"));

                    }
                    else listAlignResult.Items.Add("Find Fail MCR");
                }
                else
                {
                    MessageBox.Show("Select Laser Camera");
                    return;
                }
            }
        }
        
        CogRectangle cogrectangle = new CogRectangle();
        CogRectangle cogDetachrec = new CogRectangle();

        private void HistoRegionRectangle(eHistogram kind)
        {
                int TH = 0;
                cogDetachrec = HistoRegionRead(Vision[0].CFG.eCamName, ref TH, kind);
                txtTH.Text = TH.ToString();
                if (cogDetachrec == null) cogDetachrec = new CogRectangle();
                cogDetachrec.SelectedSpaceName = "."; //체커 추가
                //cogPmAlignTool.SearchRegion = cogrectangle;
                cogDetachrec.SetXYWidthHeight(cogDetachrec.X, cogDetachrec.Y, cogDetachrec.Width, cogDetachrec.Height);
                cogDetachrec.GraphicDOFEnable = CogRectangleDOFConstants.All;
                cogDetachrec.Interactive = true;
                cogDS.InteractiveGraphics.Add(cogDetachrec, "SearchRegion", false);
        }
        

        private void HistoRegionSave(int TH, eHistogram kind)
        {
            string RunRecipe = CONST.RunRecipe.RecipeName;
            string sPatternDir = "";

            string camName = Vision[0].CFG.eCamName;

            sPatternDir = Path.Combine(CONST.cVisionPath, camName, CONST.stringRcp, RunRecipe, kind.ToString());

            string sFile = sPatternDir + "/region.txt";

            if (cogDetachrec == null)
            {
                File.Delete(sFile);
                return;
            }


            if (!Directory.Exists(sPatternDir))
                Directory.CreateDirectory(sPatternDir);

            StreamWriter FileInfo = new StreamWriter(sFile, false, Encoding.Default);

            FileInfo.WriteLine(cogDetachrec.X.ToString());
            FileInfo.WriteLine(cogDetachrec.Y.ToString());
            FileInfo.WriteLine(cogDetachrec.Width.ToString());
            FileInfo.WriteLine(cogDetachrec.Height.ToString());
            FileInfo.WriteLine(TH.ToString());
            FileInfo.Close();
        }

        public CogRectangle HistoRegionRead(string camName, ref int TH, eHistogram kind)
        {
            string RunRecipe = CONST.RunRecipe.RecipeName;
            string sPatternDir = "";

            //string camName = Vision[0].CFG.eCamName;
            CogRectangle rect = null;
            sPatternDir = Path.Combine(CONST.cVisionPath, camName, CONST.stringRcp, RunRecipe, kind.ToString());

            string sFile = sPatternDir + "/region.txt";

            if (File.Exists(sFile))
            {
                string[] FileRead = File.ReadAllLines(sFile, Encoding.Default);
                try
                {
                    rect = new CogRectangle();
                    rect.SetXYWidthHeight(double.Parse(FileRead[0]), double.Parse(FileRead[1]), double.Parse(FileRead[2]), double.Parse(FileRead[3]));
                    TH = int.Parse(FileRead[4]);
                }
                catch 
                {
                    rect = null;
                }
            }
            else rect = null;

            return rect;
        }

        //private void cbDetachInsp_CheckedChanged(object sender, EventArgs e)
        //{
        //    DetachRegionRectangle();
        //}

        private void btnDetachInsp_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Save", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                int TH = 0;
                try
                {
                    TH = int.Parse(txtTH.Text);
                }
                catch
                { }
                HistoRegionSave(TH, (eHistogram)cbHistogram.SelectedIndex);                
            }
        }

        private void cbHistogram_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                cogDS.InteractiveGraphics.Clear();
            }
            catch { }

            if ((eHistogram)cbHistogram.SelectedIndex != eHistogram.ImageProcessing) HistoRegionRectangle((eHistogram)cbHistogram.SelectedIndex);
            else
            {
                int iTH = 0;
                HistoRegionRead(Vision[0].CFG.eCamName, ref iTH, eHistogram.ImageProcessing);
                txtTH.Text = iTH.ToString();
            }
        }

        public CogImage8Grey changeImage(string eName, CogImage8Grey img)
        {
            int TH = 0;
            HistoRegionRead(eName, ref TH, eHistogram.ImageProcessing);

            return ImageProcessing(TH, img);
        }

        public CogImage8Grey ImageProcessing(int TH, CogImage8Grey img)
        {
            CogBlobTool blobTool = new CogBlobTool();

            blobTool.InputImage = img;
            blobTool.RunParams.ConnectivityMode = CogBlobConnectivityModeConstants.GreyScale;
            blobTool.RunParams.ConnectivityCleanup = CogBlobConnectivityCleanupConstants.Fill;
            blobTool.RunParams.ConnectivityMinPixels = Menu.frmSetting.revData.mLaser.MinPixel;
            if (Menu.frmSetting.revData.mLaser.polarity == ePolarity.Dark)
                blobTool.RunParams.SegmentationParams.Polarity = CogBlobSegmentationPolarityConstants.DarkBlobs;
            else
                blobTool.RunParams.SegmentationParams.Polarity = CogBlobSegmentationPolarityConstants.LightBlobs;
            blobTool.RunParams.SegmentationParams.Mode = CogBlobSegmentationModeConstants.HardFixedThreshold;
            //blobTool.RunParams.RegionMode = CogRegionModeConstants.PixelAlignedBoundingBox;
            blobTool.RunParams.SegmentationParams.HardFixedThreshold = TH;
            blobTool.Run();

            return blobTool.Results.CreateBlobImage();
        }
        private void btnTest_Click(object sender, EventArgs e)
        {
            listAlignResult.Items.Clear();
            eHistogram selectedCb = (eHistogram)cbHistogram.SelectedIndex;
            
            try
            {
                cogDS.InteractiveGraphics.Remove("DetachInsp1");
            }
            catch { }
            int THTest = Convert.ToInt32(txtTH.Text);
            int ISGrey = 0;
            int TH = 0;
            CogRectangle Drect = HistoRegionRead(Vision[0].CFG.eCamName, ref TH, selectedCb);
            switch (selectedCb)
            {
                case eHistogram.Detach:
                    {
                        bool insp = Vision[0].MarkingHistoInspection(Drect, THTest, ref ISGrey, true);
                        if (insp) listAlignResult.Items.Add("Detach Inspection OK  : " + ISGrey);
                        else listAlignResult.Items.Add("Detach Inspection NG : " + ISGrey);

                        break;
                    }
                case eHistogram.ImageProcessing:
                    {
                        CogImage8Grey img = (CogImage8Grey)cogDS.Image;
                        cogblobDS.Image = ImageProcessing(THTest, img);
                        cogblobDS.AutoFit = true;

                        break;
                    }
                case eHistogram.MCRPre:
                    {
                        bool insp = Vision[0].MarkingHistoInspection(Drect, THTest, ref ISGrey, false);
                        if (insp) listAlignResult.Items.Add("Mcr Pre Inspection OK : " + ISGrey);
                        else listAlignResult.Items.Add("MCR Pre Inspection NG : " + ISGrey);

                        break;
                    }
                case eHistogram.MCRRegion:
                    {
                        string id = "";
                        cs2DAlign.ptXY[] codePoint = new cs2DAlign.ptXY[4];
                        if (Vision[0].readID(ref id, ref codePoint, Menu.frmSetting.revData.mLaser.MCRSearchKind, Drect))
                        {
                            listAlignResult.Items.Add("ID read OK:  " + id);
                        }
                        else listAlignResult.Items.Add("Find Fail MCR");

                        break;
                    }
                case eHistogram.RefPoint:
                    {

                        break;
                    }
            }
        }
    }
}