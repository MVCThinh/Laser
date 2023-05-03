using rs2DAlign;
using System;
using System.Drawing;

namespace Bending
{
    public class CONST
    {
        // Side Vision 추가
        //public static bool[] BendingsideCapture = new bool[3];  //Transfer Align 완료 시점부터, Bending 궤적 동작 capture -> Bending Align 완료까지 체크

        //public static bool[] BendingPressCapture = new bool[3]; //Bending Align 완료 된 후, 1초간 Capture
        //public static int[] BendingCaptureNo = new int[3];      //Bending Capture No
        //public static bool[] bSideFileSave = new bool[3];
        //public static int[] bSideImageSize = new int[3];

        public static double rad = Math.PI / 180;
        public static double rad90 = Math.PI / 2;
        public static bool plcAutomode = false;
        public static bool pcAutomode = false;

        public static bool m_bSystemLog = false;
        public static bool m_bInterfaceLog = false;
        public static bool m_bMotorADJLog = false;
        public static bool m_bAlignXDeltaLog = false;
        public static bool m_bAlignPanelLog = false;
        public static bool m_bAlignNoPressLog = false;
        public static bool m_bAlignFPCLog = false;
        public static bool m_bMeasBendingLog = false;

        //KSJ 20170606 Main화면 외에서 자동동작 하지 않게 Add
        public static bool m_bAutoStart = false;

        public static bool m_bOverlayShow = false;

        //실제 100Point 값(옵셋적용후)
        public static double[] m_dMainTraceY = new double[100];
        public static double[] m_dMainTraceZ = new double[100];
        public static double[] m_dMainTraceT = new double[100];

        // Offset 적용 시 100Point 값(PLC 적용값)
        public static double[,] m_dTraceY = new double[3, 100];
        public static double[,] m_dTraceZ = new double[3, 100];
        public static double[,] m_dTraceT = new double[3, 100];

        // Trace Info
        public static double[] TracePoint = new double[3];

        public static double[] RadiusOfRotation = new double[3];
        public static double[] endPointZ = new double[3];

        //lkw 20170818 Transfer Align 시의 Panel Mark를 Bending에서 사용하기 위한 변수 선언 (Tact 단축 용)
        public static cs2DAlign.ptXYT[] TPanel1 = new cs2DAlign.ptXYT[8];
        public static cs2DAlign.ptXYT[] TPanel2 = new cs2DAlign.ptXYT[8];
        public static cs2DAlign.ptXYT[] AFPC1 = new cs2DAlign.ptXYT[8];
        public static cs2DAlign.ptXYT[] AFPC2 = new cs2DAlign.ptXYT[8];
        //public static double[] TrefX1 = new double[3];
        //public static double[] TrefY1 = new double[3];
        //public static double[] TrefX2 = new double[3];
        //public static double[] TrefY2 = new double[3];

        //pcy190520 Bending Mark결과를 저장
        //public static double[] RMarkX1 = new double[3];
        //public static double[] RMarkY1 = new double[3];
        //public static double[] RMarkX2 = new double[3];
        //public static double[] RMarkY2 = new double[3];

        public static double[] FirstAlignResult = new double[4];

        public static rs2DAlign.cs2DAlign.ptXYT[] bendPreOffset = new rs2DAlign.cs2DAlign.ptXYT[3];

        public static double[] TransFirstOffset = new double[3];

        public static double DdriveSpace = new double();

        public static string stringRcp = "Recipe";//오타방지

        // 20200904 cjm Calibration Result 목록
        // 20200918 cjm Calibration dgv 목록 줄임
        // 20200924 cjm Calibration 목록 변경
        public static string[] resultCal1 = new string[4];
        public static string[] resultCal2 = new string[4];
        public static string[] resultCal3 = new string[4];
        public static string[] resultCal4 = new string[4];
        public static string[] Pos = new string[2]; //FixPosX, FixPosY
        public static string sx; // ScaleX
        public static string sy; // ScaleY
        public static string mFX; // Mark Fix Position X
        public static string mFY; // Mark Fix Position Y

        public static Font font6Bold = new Font("Arial", 6, FontStyle.Bold);
        public static Font font10Bold = new Font("Arial", 10, FontStyle.Bold);
        public static Font font12Bold = new Font("Arial", 12, FontStyle.Bold);
        public static Font font15Bold = new Font("Arial", 15, FontStyle.Bold);

        public static Font GetFontBold(float size)
        {
            Font font = new Font("Arial", size, FontStyle.Bold);
            return font;
        }


        public static bool simulation;

        //tt210428 live display dist draw
        public static class DispChart
        {
            public static cs2DAlign.ptXXYY[] dist = new cs2DAlign.ptXXYY[8];
            public static bool[] bDrawPoint = new bool[8];
            public static int[] CountIn1 = new int[8];
            public static int[] CountIn2 = new int[8];
            public static int[] CountOut1 = new int[8];
            public static int[] CountOut2 = new int[8];
            public static int[] CountSquareOut1 = new int[8];
            public static int[] CountSquareOut2 = new int[8];
            public static int[] Count = new int[8];
            public static double[] SquareX = new double[8];
            public static double[] SquareY = new double[8];
        }

        //public static double CodeMarkingSizeX = 5;
        //public static double CodeMarkingSizeY = 5;

        public static int old_FFU_FAN_SPEED = 0;
        public class OK_NG1
        {
            //pcy200825 Name은 count에 귀속되도록하고 nIndex도 ++없도록 변경
            public OK_NG1(string name)
            {
                Name = name;
            }
            public int[] count = new int[Enum.GetValues(typeof(ePCResult)).Length];
            public string Name;
        }

        public static string EQTYPE;
        public static string EQPID;
        public static string UnitNo;
        public static int PCNo;
        public static string PCName;
        public static int LogKeepDay = 30;
        public static int CAMCnt = 8; //default
        public static string PLCIP;
        public static int StationNO;
        public static int PLCType;
        public static string PLCDeviceType;
        public static string Folder;
        public static string Folder2;

        public static bool RunMode;

        //Height Sensor IP
        public static string HeightIP;

        public static bool m_bPLCConnect = false;

        public static double BDScaleX1;
        public static double BDScaleY1;
        public static double BDScaleX2;
        public static double BDScaleY2;
        public static double INSPScaleX1;
        public static double INSPScaleY1;
        public static double INSPScaleX2;
        public static double INSPScaleY2;
        public static double AutoScaleSpec;

        public static cs2DAlign.ptXXYY INSPBDLAST;
        public static cs2DAlign.ptXXYY INSPBDAFTER;

        public static class Address
        {
            //[PLC -> PC Start Address]
            public struct PLC
            {
                //PLC -> PC
                public static int BITCONTROL;

                //public static int CIMIF;
                public static int REPLY;
                public static int MODE;
                public static int CURRENTPOSITION;
                public static int CHANGETIME;
                public static int CELLID1;
                public static int CELLID2;
                public static int CELLID3;
                public static int CELLID4;
                public static int CELLID5;
                public static int CELLID6;
                public static int CELLID7;
                public static int CELLID8;
                public static int INSPECTIONBENDINGLAST;
                public static int RECIPEID;
                public static int RECIPEPARAM;
                public static int POSITION;
                public static int PLC_CAL_MOVE;
                public static int BDSCALE;
                public static int INSPSCALE;
                public static int LaserSendCellID; //210118 cjm 레이저 로그
                public static int MCRCellID;    //210118 cjm 레이저 로그
            }

            public struct PC
            {
                //PC -> PLC
                public static int BITCONTROL;
                //public static int CIMIF;
                public static int CALIBRATION;
                public static int REPLY;
                public static int VISIONOFFSET;
                //public static int TraceInfoArm1;
                //public static int TraceInfoArm2;
                //public static int TraceInfoArm3;
                public static int BENDING1;
                public static int BENDING2;
                public static int BENDING3;
                public static int SV;
                public static int DV;
                public static int PC_CAL_MOVE;
                public static int TransferFirstOffset;
                public static int CPK;

                //pcy200506추가
                public static int FFU;
                public static int MatchingScore;
                public static int ESC;
                public static int GMS;
                public static int HeightInspection;

                public static int APNCode1;
                public static int APNCode2;
            }
        }

        public static double TextScore;
        public static int[] iNoPanelNo = new int[4];
        public static bool[] bProgramReset = new bool[4];

        public static string cSideVisionPath = "SideImage";
        public static string cImagePath = @"EQData\Images";
        public static string cVisionPath = @"EQData\Vision";
        public static string cRecipeSavePath = @"EQData\Recipe";
        public static string cTracePath = @"EQData\Trace";
        public static string cMotionTracePath = @"EQData\Trace\MotionTrace";
        public static string cConfigSavePath = @"EQData\Config"; // 20170107. lyw. config 저장용.
        public static string cDefaultDataPath = @"EQData\DefaultData"; // lyw. default data 용.
        public static string cTrainPath = @"EQData\Images\TrainImage";

        //2018.11.08 khs Cal Type 변경
        public enum eBendMode { armXYT = 0, armTafterXY = 1, armXTafterY = 2, armXY = 3, stageTarmXY = 4 };

        public enum eCamDirection { deg0 = 0, deg90 = 1, degM90 = 2 };

        //2018.11.29 Light Type 추가
        public enum eLightType { Light5V = 0, Light12V = 1 };

        //2018.11.30 khs
        public enum eImageSaveType { All = 0, Original = 1, Display = 2, Fail = 3 };

        public enum eImageSaveKind { normal = 0, LaserAlign1 = 1, LaserAlign2 = 2, LaserInspection1 = 3, LaserInspection2 = 4 };

        //pcy190718
        public enum eInspFindSeq { PanelFPCB = 0, FPCBPanel = 1 };

        //KSJ 20170606 Image Reverse Select
        //lkw 20170809 Reverse 항목 추가
        public enum eImageReverse { None = 0, XReverse = 1, YReverse = 2, AllReverse = 3, Reverse90 = 4, Reverse270 = 5, Reverse90XY = 6, Reverse270XY = 7 };

        // pattern 검색 스타일 설정.
        public enum ePatternSearchMode
        {
            LastBest = 0,
            AllBest = 1,
        }

        public enum ePatternSearchTool
        {
            PMAlign = 0,
            SearchMax = 1,
        }

        //LogIN
        public static string LoginID = "Operator";

        //config 사용 motor(X,Z.R)
        public static short AxisY = 0;
        public static short AxisZ = 1;
        public static short AxisR = 2;

        public static csRecipe RunRecipe = new csRecipe();

        //public static bool plcWrite;
        public static bool plcTrace1Write;
        public static bool plcTrace2Write;
        public static bool plcTrace3Write;

        public static bool[] bPLCReq = new bool[64]; //req1 + req2
        public static bool[] boldPLCReq = new bool[64];

        public static bool[] bPCCalReq = new bool[32];      // Calibration Request Bit (PC -> PLC)
        public static bool[] bCalReply = new bool[32];
        public static bool[] bPCRep = new bool[32];         // PC Reply Bit (PC -> PLC)

        //Password
        public static string Password = "1111";

        public static string VisionPassword = "1234";
        public static string TracePassword = "4567";

        //2019.07.20 EMI Align 추가
        public static double plusTMoveRatio;
        public static double plusYMoveRatio;
        public static double minusTMoveRatio;
        public static double minusYMoveRatio;
        public static double EMITargetPosX1;
        public static double EMITargetPosY1;
        public static double EMITargetPosX2;
        public static double EMITargetPosY2;

        public interface iBitControl
        {

        }

        #region 공통

        //unit별 공통은 공통으로 쓰기.. CommonUnit
        public static class CUnit
        {
            public class BitControl
            {
                //PLC -> PC
                public const short plcCurrentRecipeChangeReq = 0xF;
                public const short plcTimeChangeReq = 0x1E;
                public const short plcAuto = 0x1F;

                //PC -> PLC
                public const short pcAlive = 0x0;
                public const short pcAutoManual = 0x1;
            }

            public static class Reply
            {
                public const short pcTimeChangeReply = 0x1E;
                public const short pcCurrentRecipeChangeReply = 0x1F;
            }

            public static class CalControl
            {
                //PLC -> PC
                public const short plcCalStartOK = 0x10;
                public const short plcCalStartNG = 0x11;
            }
        }

        #endregion 공통

        #region Main 화면 Align 결과 Display 관련

        public static string RESULT_TITLE1;
        public static string RESULT_TITLE2;
        public static string RESULT_TITLE3;
        public static string RESULT_TITLE4;

        //RESULT_DISP 의도는 표4개중에 어디에 표시할지 정하는 변수인것 같으나 사용하는곳 없음
        public static int RESULT1_DISP;
        public static int RESULT2_DISP;
        public static int RESULT3_DISP;
        public static int RESULT4_DISP;
        public static string RESULT1_TYPE;      // 결과 표시 종류 (Foam, BD, Insp)
        public static string RESULT2_TYPE;
        public static string RESULT3_TYPE;
        public static string RESULT4_TYPE;
        public static string RESULT5_TYPE;

        #endregion Main 화면 Align 결과 Display 관련

        public static string IndexNOBendPre;

        public static L4Logger Log { get; set; } = L4Logger.Instance;
    }

    public static class PanelID
    {
        //11inch
        public static string LoadingPre;
        public static string Attach1;
        public static string Attach2;
        public static string[] Bend = new string[1];
        public static string UpperInsp;
        public static string SideInsp;
        public static string Laser;
        //11inch
    }
    public static class Vision_No
    {
        //pc1
        public const short LoadingPre1 = 0;
        public const short LoadingPre2 = 1;
        public const short Laser1 = 2;
        public const short Laser2 = 3;
    }
    public static class Bit
    {
        //PC -> PLC
        public const short pcAlive = 0x0;
        public const short pcAutoManual = 0x1;
    }

    public static class cTrayLoader
    {
        //state = s
        public static eTray sReady;
        //public static int sOK = 0;
        //public static int sNG = 0;
        //public static int sPanelExist = 0;
        //public static int sOverPocket = 0;

        public static class ReplyAddress
        {
            //결과 워드 모음
            public const short Ready = 15121;
            public const short OK = 15122;
            public const short NG = 15123;
            public const short PanelExist = 17000;
            public const short OverPocket = 17002;
        }

    }

    public static class plcRequest
    {
        //PLC -> PC bit
        public const short LoadingPre1Align = 0x0;
        public const short LoadingPre2Align = 0x01;

        public const short Laser1Align = 0x03;
        public const short Laser2Align = 0x04;

        public const short Laser1Inspection = 0x06;
        public const short Laser2Inspection = 0x07;

        public const short Laser1CellLog = 0x10;
        public const short Laser2CellLog = 0x11;

        public const short MCRRead1 = 0x13;
        public const short MCRRead2 = 0x14;
    }
    public static class pcReply
    {
        public const short LoadingPre1Align = 0x0;
        public const short LoadingPre2Align = 0x01;

        public const short Laser1Align = 0x03;
        public const short Laser2Align = 0x04;

        public const short Laser1Inspection = 0x06;
        public const short Laser2Inspection = 0x07;

        public const short MarkingDataCompare1 = 0xA;
        public const short MarkingDataCompare2 = 0xB;

        public const short Laser1CellLog = 0x10;
        public const short Laser2CellLog = 0x11;

        public const short MCRRead1 = 0x13;
        public const short MCRRead2 = 0x14;
    }
    public static class CalControl
    {
        public static class plcReply
        {
            //PLC -> PC
            public const short LoadingPre1CalMove = 0x0;
            public const short LoadingPre2CalMove = 0x01;

            public const short LaserAlign1CalMove = 0x02;
            public const short LaserAlign2CalMove = 0x03;

            public const short CalStartOK = 0x10;
            public const short CalStartNG = 0x11;
        }

        public static class pcRequest
        {
            //PC -> PLC
            public const short LoadingPre1CalMove = 0x0;
            public const short LoadingPre1CalStart = 0x10;
            public const short LoadingPre2CalMove = 0x01;
            public const short LoadingPre2CalStart = 0x11;

            public const short LaserAlign1CalMove = 0x02;
            public const short LaserAlign1CalStart = 0x12;
            public const short LaserAlign2CalMove = 0x03;
            public const short LaserAlign2CalStart = 0x13;
        }
    }
    public static class Address
    {
        //기본 주소값(ini)에 더해서 갖고있어야함.
        public static class VisionOffset
        {
            public static int LoadingPre1 = 0;
            public static int LoadingPre2 = 6;

            public static int LaserAlign1 = 12;
            public static int LaserAlign2 = 18;
        }
        public static class DV
        {
            public static int LaserInsp1 = 0;
            public static int LaserInsp2 = 10;
            public static int LaserInsp1_Ascii = 30;
            public static int LaserInsp1_ASP = 50;
            public static int LaserInsp2_Ascii = 60;
            public static int LaserInsp2_ASP = 80;
        }
        public static class MatchingScore
        {
            public static int LoadingPre1 = 0;
            public static int LoadingPre2 = 6;

            public static int Laser1Align = 12;
            public static int Laser2Align = 18;
        }
        public static class SV
        {
            public static int BendAlignMove = 0;
        }
    }
    public enum eInspMode
    {
        PanelMarkFPCBMark = 0,
        PanelEdgeFPCBEdge,
        PanelEdgeFPCBMark,
        PanelMarkFPCBEdge,
    }
    public enum eSearchKind
    {
        Pattern = 0,
        Line,
    }
    public enum eMarkSearchKind
    {
        Mark,
        EdgeMark,
        EdgeLine,
    }
    public enum ePatternKind
    {
        Panel = 0,
        FPC,
        SubPanel,
        SubFPC,
        Material, //자재 추가
        Left_1cam,
        Right_1cam,
        Cal,
        Cal2,
        CalX,
        CalY,
        FoamInsp,
    }
    public enum eLineKind
    {
        //cam1에서 두점찾을때 고려해야함(width각각두개 height각각 두개)
        Null = -1, //null이 필요한 곳에서 사용하고, -1로 해둬서 처리 잘못하면 프로그램 뻑날텐데 역추적하기 좋을듯
        PanelWidth1 = 0,
        PanelWidth2,
        PanelHeight1,
        PanelHeight2,
        FPCBWidth1,
        FPCBWidth2,
        FPCBHeight1,
        FPCBHeight2,
        FoamWidth1, //pcy201118
        FoamWidth2,
        FoamHeight1,
        FoamHeight2,
    }
    public enum eCamNO //사실 visionno와 같음 //tostring비교시 ecamNOpc번호로 변환필요
    {
        Null = -1,
        LoadingPre1 = 0,
        LoadingPre2,
        Laser1,
        Laser2,
    }
    public enum eCamNO1
    {
        LoadingPre1 = 0,
        LoadingPre2,
        Laser1,
        Laser2,
    }
    
    public enum eCalPos
    {
        Err = -1,
        LoadingPre1_1 = 0,
        LoadingPre1_2,
        LoadingPre2_1,
        LoadingPre2_2,
        Laser1,
        Laser2,
    }

    public enum eHistogram
    {
        Detach = 0,
        MCRPre = 1,
        RefPoint = 2,
        MCRRegion = 3,
        ImageProcessing = 4,
    }

    public enum ePCResult
    {
        WAIT = 0,
        OK = 1,
        BY_PASS = 2, // (Mark NG, L-Check NG, Manual Bending 등) 각종 모르는 이유..
        ALIGN_LIMIT = 3, //
        SPEC_OVER = 4, //각종 스펙오버
        CHECK = 5, //각종 유무
        WORKER_BY_PASS = 6, //작업자 bypass처리
        MANUAL_BENDING = 7, //강제벤딩
        INIT = 8, //이니셜 에러
        RETRY_OVER = 9, //
        PANEL_SHIFT_NG = 10,
        RETRY = 11,
        ERROR_MARK = 12,
        FIRST_LIMIT = 13,
        ERROR_LCHECK = 14,
        VISION_REPLY_WAIT = 15, //(Mark NG, L-Check NG, Manual Bending 등)
        //20.12.17 lkw DL
        DL = 16,
    }
    public enum eTray
    {
        OFF = 0,
        ON = 1,
    }
    public enum eID
    {
        DataMatrix = 0,
        QRCode = 1,
    }

    public enum eRefSearch
    {
        Line = 0,
        Mark = 1,
        Blob = 2,
    }

    public enum eBlobMass
    {
        Left = 0,
        Right = 1,
    }

    public enum eBlobPoint
    {
        LeftUp = 0,
        RightUp = 1,
        LeftDown = 2,
        RightDown = 3,
    }

    public enum ePolarity
    {
        Dark = 0,
        Light = 1,
    }

    public enum eInspKind
    {
        Camera = 0,
        Mark = 1,
    }
    public enum eDetachInspPosition
    {
        DPre1 = 0,
        DPre2 = 1,
        Attach1_1 = 2,
        Attach1_2 = 3,
        Attach2_1 = 4,
        Attach2_2 = 5,
        DPre3 = 6,
        DPre4 = 7,
        Laser = 8,
    }

    public struct sDetachParam
    {
        public double OffsetX;
        public double OffsetY;
        public double LimitTH;
        public double size;
        // public bool reverse;
        public bool checkWhite;
    }

    //public enum eOKNGcnt1
    //{
    //    Conveyor = 0, LoadingBuffer, BendPre,
    //}
    //public enum eOKNGcnt2
    //{
    //    SCFPanel = 0, SCFReel,
    //}
    //public enum eOKNGcnt3
    //{
    //    BendingTransfer1 = 0, BendingArm1, BendingTransfer2, BendingArm2, BendingTransfer3, BendingArm3,
    //}
    //public enum eOKNGcnt4
    //{
    //    BendSide1 = 0, BendSide2, BendSide3, TempAttach, Inspection, EMIAttach, LMI,
    //}

    public enum eCalType
    {
        Cam1Type = 0,
        Cam2Type = 1,
        Cam3Type = 2,
        Cam4Type = 3,
        Cam1Cal2 = 4,
    }
    public enum eRecipe
    {
        PANEL_LENGTH_X = 0,
        PANEL_LENGTH_Y,
        PANEL_MARK_TO_MARK_LENGTH_X,
        FOAM_LENGTH_Y,   //2.5
        FOAM1_ATTACH_SPEC_LX,
        FOAM1_ATTACH_SPEC_LY,
        FOAM1_ATTACH_SPEC_RX,
        FOAM1_ATTACH_SPEC_RY,
        BENDING_SPEC_LX,
        BENDING_SPEC_LY,
        BENDING_SPEC_RX,
        BENDING_SPEC_RY,
        BENDING_INSPECTION_SPEC_LX,
        BENDING_INSPECTION_SPEC_LY,
        BENDING_INSPECTION_SPEC_RX,
        BENDING_INSPECTION_SPEC_RY,
        FOAM_ATTACH1_OFFSET_X,
        FOAM_ATTACH1_OFFSET_Y,
        FOAM_ATTACH1_OFFSET_T,
        FOAM_ATTACH2_OFFSET_X,
        FOAM_ATTACH2_OFFSET_Y,
        FOAM_ATTACH2_OFFSET_T,
        BENDING_ARM_PRE_OFFSET_X,
        BENDING_ARM_PRE_OFFSET_Y,
        BENDING_ARM_PRE_OFFSET_T,
        BENDING_OFFSET_LY,
        BENDING_OFFSET_RY,
        LOADING_PRE_LCHECK_SPEC1,
        LOADING_PRE_LCHECK_TOLERANCE1,
        LOADING_PRE_LCHECK_SPEC2,
        LOADING_PRE_LCHECK_TOLERANCE2,
        FOAM_REEL_LCHECK_SPEC1,
        FOAM_REEL_LCHECK_TOLERANCE1,
        FOAM_REEL_LCHECK_SPEC2,
        FOAM_REEL_LCHECK_TOLERANCE2,
        FOAM_ATTACH_LCHECK_SPEC1,
        FOAM_ATTACH_LCHECK_TOLERANCE1,
        FOAM_ATTACH_LCHECK_SPEC2,
        FOAM_ATTACH_LCHECK_TOLERANCE2,
        BENDING_LCHECK_SPEC1,
        BENDING_LCHECK_TOLERANCE1,
        BENDING_LCHECK_SPEC2,
        BENDING_LCHECK_TOLERANCE2,
        BENDINGSIDE_LCHECK_SPEC1,
        BENDINGSIDE_LCHECK_TOLERANCE1,
        BENDINGSIDE_LCHECK_SPEC2,
        BENDINGSIDE_LCHECK_TOLERANCE2,
        SIDEINSPECTION_LCHECK_SPEC1,
        SIDEINSPECTION_LCHECK_TOLERANCE1,
        SIDEINSPECTION_LCHECK_SPEC2,
        SIDEINSPECTION_LCHECK_TOLERANCE2,
        UPPERINSPECTION_LCHECK_SPEC1,
        UPPERINSPECTION_LCHECK_TOLERANCE1,
        UPPERINSPECTION_LCHECK_SPEC2,
        UPPERINSPECTION_LCHECK_TOLERANCE2,
        FFU_FAN_SPEED,
        LASER_POSITION_SPECX,
        LASER_POSITION_SPECY,
        Reserve1,
        Reserve2,
        LASER_MARKING_OFFSET_X1,
        LASER_MARKING_OFFSET_Y1,
        LASER_MARKING_OFFSET_X2,
        LASER_MARKING_OFFSET_Y2,
        FOAM2_ATTACH_SPEC_LX,
        FOAM2_ATTACH_SPEC_LY,
        FOAM2_ATTACH_SPEC_RX,
        FOAM2_ATTACH_SPEC_RY,
    }

    public enum eOK_NG
    {
        //Conveyor, = "Conveyor";
        //LoadingBuffer = "LoadingBuffer";
        //BendPre = "BendPre";
        //PanelVision = "PanelVision";
        //SCFReel = "SCFReel";
        //SCFPick = "SCFPick";
        //Bend1Trans = "Bend1Trans";
        //Bend2Trans = "Bend2Trans";
        //Bend3Trans = "Bend3Trans";
        //Bend1Arm = "Bend1Arm";
        //Bend2Arm = "Bend2Arm";
        //Bend3Arm = "Bend3Arm";
        //Side1 = "Side1";
        //Side2 = "Side2";
        //Side3 = "Side3";
        //TempAttach = "TempAttach";
        //LMI = "LMI";
        //Inspection = "Inspection";
        //EMI = "EMI";
    }
    //2020.09.25 lkw
    public enum eConvert
    {
        TempAttach1 = 0,
        TempAttach2 = 1,
        EMIAttach1 = 2,
        EMIAttach2 = 3,
        notUse = 4,
    }
}