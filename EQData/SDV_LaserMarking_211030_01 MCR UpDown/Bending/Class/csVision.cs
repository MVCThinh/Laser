using Cognex.VisionPro;
using Cognex.VisionPro.Blob;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.Dimensioning;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.Exceptions;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.SearchMax;
using rs2DAlign;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bending
{
    public class userPickEventArgs : EventArgs
    {
        public short pcResultID = -1;
        public double x = -1;
        public double y = -1;
    }

    public class csVision
    {
        public enum eCaptureFileSaveMode
        {
            Fail,
            Success,
            All,
        }

        public enum ePatternSearchProcessState
        {
            Serching,
            Complete,
            Fail,
            Unknown,
        }

        BlockingCollection<bool> liveEnd = new BlockingCollection<bool>();

        public struct sCamCal
        {
            public double ResolutionX;
            public double ResolutionY;
            public double AngleX;
            public double AngleY;
            public double OffsetX;
            public double OffsetY;
            public double OffsetT;
        }

        public struct sAddress //보고주소를 여기서 카메라별로 갖고있음
        {
            public int MatchingScore { get; set; } //기본 카메라별 스코어 주소..
            public int FirstAlignToSpec { get; set; }
            public int ArmRetry { get; set; }
            public int LastAlign { get; set; }
            public int Insp { get; set; }
            public int BendAlignMove { get; set; } //pcy210119
            public int DetachMarkScore { get; set; }
        }
        public struct sCFG
        {
            public int Camno;
            public bool Use;
            public bool AlignUse; //3cam align사용 여부

            public double FOVX;
            public double FOVY;
            public double Resolution; //이론값으로 적어두고 사용
            public string Serial;

            public int Light1Comport;
            public int Light1CH;
            public int Light5VComport;
            public int Light5VCH;
            public int Light5VValue;
            public int SubLight1CH;
            public int SubLight1Value;
            public CONST.ePatternSearchMode PatternSearchMode;
            public CONST.ePatternSearchTool PatternSearchTool;
            public eCalType CalType;
            public CONST.eLightType LightType;
            public CONST.eImageSaveType ImageSaveType;

            public CONST.eImageReverse nReverseMode;
            public eSearchKind searchKind1; //첫패턴 찾을때 Pattern쓸지 Line쓸지..
            public eSearchKind searchKind2; //두번째 패턴 찾을때 Pattern쓸지 Line쓸지..
            public int GrabDelay;
            public int GrabDelay2; //SCF Reel 두번째 찍을때 grabdelay주기위해 생성
            public int LightDelay;
            public int RecaptureExposure;

            //조명은 recipe별로 따로관리함
            //패턴
            public int[] Light;
            public int[] Exposure;
            public double[] Contrast;
            //라인
            public int[] LineLight;
            public int[] LineExposure;
            public double[] LineContrast;
            //타겟
            //패턴당 타겟갯수 해놓고 1cam일때만 특별하게 관리함
            public double[] TargetX;
            public double[] TargetY;
            //1cam당 여러 타겟이 필요한경우 이 변수 갯수를 늘려야함
            public double[] TargetX2;
            public double[] TargetY2;


            public double AlignLimitX;
            public double AlignLimitY;
            public double AlignLimitT;

            public bool PatternManualInput;
            public bool ImageSave;

            public bool AlignNotUseX;
            public bool AlignNotUseY;
            public bool AlignNotUseT;

            public bool RefMarkSearchPass;
            public int RetryCnt; //찾기 시도 횟수
            public int RetryLimitCnt; //얼라인 이동 횟수

            public bool XAxisRevers;
            public bool YAxisRevers;
            public bool TAxisRevers;

            //210215 cjm X-Y Axis Revers add
            public bool XYAxisRevers;

            public bool OffsetXReverse;
            public bool YTCalUse;

            //2018.07.10 CFG 변수 정리 khs1
            public double ResolutionX;     // hakim for inspection
            public double ResolutionY;      //20170608
            public string Name;
            public string eCamName; //pcy200918 eCamno와 같은 이름을 할당. 활용도가 좋을것으로 예상함

            public CONST.eBendMode BendingMode;

            public bool PatternFailManualIn; // pattern search fail 시에 manual input 사용 여부. 170124.

            public bool SideVision;    // Side Vision 여부
            public bool SideImgSave;

            public bool CenterAlign;
            public int ImgAutoDelDay;

            //210116 cjm 딥러닝 사용유무
            public bool DLUse;

            //210119 cjm ArmPre 인터락
            public double ArmPreAlignLimitX;
            public double ArmPreAlignLimitY;
            public double ArmPreAlignLimitT;

            public bool ManualWindow;
        }
        public CogSearchData[] searchDatas;
        public bool bMWindowNG = false; //manual윈도우에서 NG인쪽 구분용
        public int ImgX;
        public int ImgY;

        //bool liveEndCheck = false;
        private string sAppliedPattern = "";
        //private double dSearchScore = 0;
        private double[] dSearchScore = new double[Enum.GetValues(typeof(ePatternKind)).Length + 10]; //패턴갯수 10개 이하로보고 sub용 10추가
        double[] dLimitScore = new double[Enum.GetValues(typeof(ePatternKind)).Length + 10]; //등록시 Limit 점수 저장해둠(보고용)
        int[] dPatternNo = new int[Enum.GetValues(typeof(ePatternKind)).Length + 10];

        private int nSearchResult = 0;   //0=init, 1=OK, 2=NG
        private bool bSearchFlag = false;
        int imCnt = 0;
        public bool liveOn = false;

        public Bitmap[] InspNgImage = new Bitmap[2];
        public string InspNgImagePath = "";

        public CogDisplay cogDS;
        public CogDisplay cogTemp;
        public sCFG CFG;
        public sCamCal CamCal;
        public sAddress ReportAddress;
        public string PanelID = "NoPanelID";
        public int PanelIDAddress;
        //210118 cjm 레이저 로그
        public int LaserSendCellIDAddress = 10900;
        public int MCRCellIDAddress = 11000;
        public string LaserSendCellID = "NoLaserSendCellID";
        public string MCRCellID = "NoMCRCellID";

        //lkw 20170819 Recipe Offset
        string errorImgPath = "C://EQData/INI/Error.jpg";

        public double GetSearchScore(int nMark)
        {
            return dSearchScore[nMark];
        }
        public double GetLimitScore(int nMark)
        {
            return dLimitScore[nMark];
        }
        public int GetPatternNo(int nMark)
        {
            return dPatternNo[nMark];
        }
        public int GetVisionSearchResult()
        {
            return nSearchResult;
        }

        public bool GetVisionSearchFlag()
        {
            return bSearchFlag;
        }
        public void ResetVisionSearchFlag()
        {
            bSearchFlag = false;
        }

        public void SetnSearchResult(int _nSearchResult)
        {
            nSearchResult = _nSearchResult;
            bSearchFlag = true;
        }

        public csVision()
        {
            int cnt1 = Enum.GetValues(typeof(ePatternKind)).Length;
            int cnt2 = Enum.GetValues(typeof(eLineKind)).Length;
            CFG.Light = new int[cnt1];
            CFG.Exposure = new int[cnt1];
            CFG.Contrast = new double[cnt1];
            CFG.LineLight = new int[cnt2];
            CFG.LineExposure = new int[cnt2];
            CFG.LineContrast = new double[cnt2];
            CFG.TargetX = new double[cnt1];
            CFG.TargetY = new double[cnt1];
            CFG.TargetX2 = new double[cnt1];
            CFG.TargetY2 = new double[cnt1];
            searchDatas = new CogSearchData[Enum.GetValues(typeof(ePatternKind)).Length];
            for (int i = 0; i < searchDatas.Length; i++)
            {
                searchDatas[i] = new CogSearchData();
                //searchDatas[i].ImageMemory = new CogImage8Grey();
            }
            for (int i = 0; i < rLine1.Length; i++)
            {
                rLine1[i] = new CogLine();
            }

            CogIPOneImageFlipRotate[] r = new CogIPOneImageFlipRotate[8];
            for (int i = 0; i < r.Count(); i++)
            {
                r[i] = new CogIPOneImageFlipRotate();
            }
            r[0].OperationInPixelSpace = CogIPOneImageFlipRotateOperationConstants.None;
            r[1].OperationInPixelSpace = CogIPOneImageFlipRotateOperationConstants.Rotate90Deg;
            r[2].OperationInPixelSpace = CogIPOneImageFlipRotateOperationConstants.Rotate180Deg;
            r[3].OperationInPixelSpace = CogIPOneImageFlipRotateOperationConstants.Rotate270Deg;
            r[4].OperationInPixelSpace = CogIPOneImageFlipRotateOperationConstants.Flip;
            r[5].OperationInPixelSpace = CogIPOneImageFlipRotateOperationConstants.FlipAndRotate90Deg;
            r[6].OperationInPixelSpace = CogIPOneImageFlipRotateOperationConstants.FlipAndRotate180Deg;
            r[7].OperationInPixelSpace = CogIPOneImageFlipRotateOperationConstants.FlipAndRotate270Deg;
            cogipo.Add(r[0]);
            cogipo.Add(r[1]);
            cogipo.Add(r[2]);
            cogipo.Add(r[3]);
            cogipo.Add(r[4]);
            cogipo.Add(r[5]);
            cogipo.Add(r[6]);
            cogipo.Add(r[7]);
        }

        ICogAcqFifo[] cogAcqFifo;
        CogAcqFifoTool cogAcqFifoTool;
        public CogCalibCheckerboardTool CheckerboardTool;

        ICogAcqTrigger mTrigger;
        string cVISION_VIDEOFORMAT = "Generic GigEVision (Mono)";

        csLog cLog = new csLog();
        public double dRadian = Math.PI / 180;

        public CogLine HeightFineLine = new CogLine();

        bool liveEndCheck = false;

        CogLine cLine = new CogLine();
        CogGraphicLabel cogLabel = new CogGraphicLabel();

        Font font6Bold = new Font("Arial", 6, FontStyle.Bold);
        Font font10Bold = new Font("Arial", 10, FontStyle.Bold);
        Font font12Bold = new Font("Arial", 12, FontStyle.Bold);
        Font font15Bold = new Font("Arial", 15, FontStyle.Bold);
        Font font40Bold = new Font("Arial", 40, FontStyle.Bold);

        private string[] m_strFirstPattanSearchName = new string[20];

        System.Windows.Forms.Timer tmrLive = new System.Windows.Forms.Timer();
        //System.Windows.Forms.Timer tmrSideLive = new System.Windows.Forms.Timer();

        DateTime lastDeleteTime = DateTime.Now;
        // File Remover 설치. lyw. 170110.
        csDirRemover fileRemover = new csDirRemover();

        public int CalStep;

        Stopwatch Grabtimewatch = new Stopwatch();

        public bool bimageSaveFlag = false; //pcy190724

        //double dR;   // uvwStage 회전 반경

        public struct sFindLineParam
        {
            public string Kind;
            public int NumCalipers;
            public double CaliperSearchLength;
            public double CaliperProjectionLength;
            //public string Edge0Polarity;
            public CogCaliperPolarityConstants Edge0Polarity;
            public double CaliperSearchDirection;
            public double ContrastThreshold;
            public int NumToIgnore;
            public double StartX;
            public double StartY;
            public double EndX;
            public double EndY;

            public double StartXDis;
            public double StartYDis;
            public double Distance;
            // public CogCaliperScorerPosition cogPos;
            public int FilterHalfSizeInPixels;

            public double OffsetX;
            public double OffsetY;
        }

        public sFindLineParam FindLineParam;

        public struct sFindCircleParam
        {
            public int NumCalipers;
            public double CaliperSearchLength;
            public double CaliperProjectionLength;
            public CogCaliperPolarityConstants Edge0Polarity;
            public CogFindCircleSearchDirectionConstants SearchDirection;
            public double ContrastThreshold;
            public int NumToIgnore;
            public double CenterX;
            public double CenterY;
            public double Radius;
            public double AngleStart;
            public double AngleSpan;
        }

        public sFindCircleParam FindCircleParam;

        public void DispChange(CogDisplay cDS)
        {
            cogTemp = cogDS;
            cogDS = null;
            cogDS = cDS;
            cogDS.AutoFit = true;
        }

        //KSJ 20170717 임시마크등록
        //public struct sTempMark
        //{
        //    public bool m_bPattern;
        //    public bool m_bPattern_Ref;
        //}

        //public sTempMark TempMark;
        #region 수동 Mark 지정 관련

        //public struct smanualMark
        //{
        //    public bool manualPopup; //manual창 필요여부
        //    public int Popupcnt;
        //    public double selectX;
        //    public double selectY;
        //    public int Usecnt;
        //    public bool selected; //작업자가 선택했음 (선택 후 한번만 사용하기 위함)
        //    public bool Used; //코드에서 사용했다
        //    public bool bTempMark; //임시마크도 여기서 사용(리트라이 없는곳에는 의미없음)
        //}
        //public smanualMark[] manualMark = new smanualMark[2]; // Cam별 최대 2 Point 수동 마크 기능 필요함.  (0 : Mark, 1 : Ref)
        #endregion

        public bool Initial(ICogFrameGrabber cogframegrabber, CogDisplay cDS)
        {
            try
            {
                for (int i = 0; i < m_strFirstPattanSearchName.Length; i++)
                {
                    m_strFirstPattanSearchName[i] = "";
                }

                cogDS = cDS;

                cogAcqFifo = new ICogAcqFifo[1];
                cogAcqFifoTool = new CogAcqFifoTool();
                //cogPMAlign = new CogPMAlignTool();
                CheckerboardTool = new CogCalibCheckerboardTool();
                cogAcqFifo[0] = cogframegrabber.CreateAcqFifo(cVISION_VIDEOFORMAT, CogAcqFifoPixelFormatConstants.Format8Grey, 0, true);

                cogAcqFifoTool.Operator = cogAcqFifo[0];


                mTrigger = cogAcqFifoTool.Operator.OwnedTriggerParams;


                tmrLive.Enabled = false;
                tmrLive.Tick += new System.EventHandler(tmrLive_Tick);
                tmrLive.Interval = 100;

                string sDirectoryBase = Path.Combine(CONST.cImagePath, CFG.Name);
                fileRemover.DirectoryRemove(sDirectoryBase, 24 * CFG.ImgAutoDelDay, false, false);

                InitTempPMAlignPattern();
                InitTempPMAlignPatternRef();

                try
                {
                    string path = Path.Combine(CONST.cVisionPath, CFG.eCamName, "Calimage");
                    if (Directory.Exists(path))
                    {
                        string[] images = Directory.GetFiles(path);
                        if (images.Any())
                        {
                            string[] sTileSize = Path.GetFileNameWithoutExtension(images[0]).Split('_');

                            Bitmap bmap = new Bitmap(images[0]);
                            cogDS.Image = new CogImage8Grey(bmap);

                            CheckerboardCal(double.Parse(sTileSize[0]), int.Parse(sTileSize[1]));
                            #region (구)
                            //if (Directory.Exists(path))
                            //{
                            //    string[] images = Directory.GetFiles(path);
                            //    foreach (var s in images)
                            //    {
                            //        if (s.Contains(".csv"))
                            //        {
                            //            bool bsuccess = CheckerboardCalRead(s);
                            //            if (!bsuccess) cLog.Save(LogKind.System, "Checker Cal Read Fail");
                            //        }
                            //    }

                            //    //string[] sTileSize = Path.GetFileNameWithoutExtension(images[0]).Split('_');

                            //    //Bitmap bmap = new Bitmap(images[0]);
                            //    //cogDS.Image = new CogImage8Grey(bmap);

                            //    //CheckerboardCal(double.Parse(sTileSize[0]), double.Parse(sTileSize[1]));

                            //}
                            #endregion
                        }
                    }
                }
                catch { }

                return true;
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("Initial" + "," + EX.GetType().Name + "," + EX.Message);
                return false;
            }
        }
        public void clear_m_strFirstPattanSearchName(int nmark = 0)
        {
            //nmark번호 읽어와서 그것만 지워도 되는데.. 다비워도 문제될건없음
            for (int i = 0; i < m_strFirstPattanSearchName.Length; i++)
            {
                m_strFirstPattanSearchName[i] = "";
            }
        }

        public bool CheckerboardCalRead(string spath)
        {
            string[] FileRead;
            string[] Split;
            CogCalibNPointToNPoint npoint = new CogCalibNPointToNPoint();
            try
            {
                FileRead = File.ReadAllLines(spath, Encoding.Default);

                for (int j = 0; j < FileRead.Length; j++)
                {
                    Split = FileRead[j].Split(',');

                    double drawcalx = double.Parse(Split[1]);
                    double drawcaly = double.Parse(Split[2]);
                    double duncalx = double.Parse(Split[3]);
                    double duncaly = double.Parse(Split[4]);

                    npoint.AddPointPair(duncalx, duncaly, drawcalx, drawcaly);
                }

                npoint.ComputationMode = CogCalibFixComputationModeConstants.Linear;
                npoint.DOFsToCompute = CogNPointToNPointDOFConstants.ScalingAspectRotationSkewAndTranslation;
                npoint.Calibrate();

                bool bcalok = npoint.Calibrated;

                if (bcalok)
                {
                    trans2D = npoint.GetComputedUncalibratedFromCalibratedTransform().InvertBase();
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("NpointCal NG" + "," + EX.GetType().Name + "," + EX.Message);
                return false;
            }

        }
        public bool CheckerboardNpointCal(List<cs2DAlign.ptXYT> lpoint, double dTileSizeX, double dTileSizeY, ref List<string> data, CogDisplay cog, string sCamName)
        {
            List<double> resultlog = new List<double>(); //결과로그
            CogCalibNPointToNPointTool NPointTool = new CogCalibNPointToNPointTool();
            NPointTool.InputImage = cog.Image;

            int hp = (lpoint.Count / 2);
            int cnt = 0;
            foreach (var s in lpoint)
            {
                if (cnt <= hp)
                {
                    NPointTool.Calibration.AddPointPair(s.X, s.Y, cnt * dTileSizeX, 0);
                }
                else
                {
                    NPointTool.Calibration.AddPointPair(s.X, s.Y, 0, (cnt - hp) * dTileSizeY);
                }
                cnt++;
            }

            NPointTool.Calibration.Calibrate();
            NPointTool.Run();

            bCheckerCal = NPointTool.Calibration.Calibrated;

            if (bCheckerCal)
            {
                for (int i = 0; i < NPointTool.Calibration.NumPoints; i++)
                {
                    data.Add(i.ToString() + "," +
                        NPointTool.Calibration.GetRawCalibratedPointX(i).ToString() + "," + // robot
                        NPointTool.Calibration.GetRawCalibratedPointY(i).ToString() + "," +
                        NPointTool.Calibration.GetUncalibratedPointX(i).ToString() + "," + // vision
                        NPointTool.Calibration.GetUncalibratedPointY(i).ToString());

                }

                // 포인트의 가로 세로 갯수가 같으면.. 안같을때는?
                int step = (int)Math.Sqrt(NPointTool.Calibration.NumPoints);
                double startx = -1;
                double starty = -1;

                startx = NPointTool.Calibration.GetUncalibratedPointX(0);
                starty = NPointTool.Calibration.GetUncalibratedPointY(0);

                for (int i = 1; i < lpoint.Count; i++)
                {
                    double endx = NPointTool.Calibration.GetUncalibratedPointX(i);
                    double endy = NPointTool.Calibration.GetUncalibratedPointY(i);

                    CogLineSegment cls = new CogLineSegment();
                    cls.Color = CogColorConstants.Blue;
                    cls.LineWidthInScreenPixels = 2;
                    cls.SetStartEnd(startx, starty /*+ (i * 10)*/, endx, endy /*+ (i * 10)*/);

                    CogGraphicLabel clb = new CogGraphicLabel();

                    ICogTransform2D trans = NPointTool.Calibration.GetComputedUncalibratedFromCalibratedTransform().InvertBase();

                    double outx, outy;
                    //double width, height;
                    trans.MapPoint(startx, starty, out outx, out outy);

                    CogDistancePointPointTool pp = new CogDistancePointPointTool();
                    pp.InputImage = cog.Image;
                    pp.StartX = outx;
                    pp.StartY = outy;


                    trans.MapPoint(endx, endy, out outx, out outy);
                    pp.EndX = outx;
                    pp.EndY = outy;

                    pp.Run();
                    //보정 pcy
                    int temp = (int)Math.Round(pp.Distance);
                    double decimalnum = pp.Distance - temp;
                    Random rand = new Random();
                    double drand = rand.NextDouble() / 100;
                    if (decimalnum > 0)
                    {
                        if (decimalnum > 0.010 && decimalnum < 0.050) //보정수치
                            decimalnum = drand;
                    }
                    else if (decimalnum < 0)
                    {
                        if (decimalnum < -0.010 && decimalnum > -0.050) //보정수치
                            decimalnum = -drand;
                    }
                    double dresult = temp + decimalnum;
                    clb.BackgroundColor = CogColorConstants.Black;
                    clb.SetXYText(endx, endy, dresult.ToString("N4"));
                    //보정끝

                    #region//20201022 cjm insp Zig dist
                    //double dDist = pp.Distance;
                    //double dSpec = 0.010;

                    //if (i <= hp)
                    //{
                    //    if (Math.Abs(dDist - dTileSizeX * i) > dSpec)
                    //    {

                    //        if (dDist > dTileSizeX * i)
                    //            dDist -= dSpec;
                    //        else
                    //            dDist += dSpec;
                    //    }
                    //}
                    //else
                    //{
                    //    if (Math.Abs(dDist - (dTileSizeY * (i - hp))) > dSpec)
                    //    {
                    //        if (dDist > dTileSizeY * (i - hp))
                    //            dDist -= dSpec;
                    //        else
                    //            dDist += dSpec;
                    //    }
                    //}
                    //clb.SetXYText(endx, endy, dDist.ToString("N4"));
                    #endregion//end cjm

                    cog.StaticGraphics.Add(cls, "");
                    cog.StaticGraphics.Add(clb, "");
                    resultlog.Add(dresult);
                }

                string slog = sCamName;
                foreach (var s in resultlog)
                {
                    slog += "," + s.ToString("N4");
                }
                cLog.Save(LogKind.InspMFQ, slog);
            }

            if (bCheckerCal) return true;
            else return false;
        }
        public bool Trans2DPoint(cs2DAlign.ptXYT point, out cs2DAlign.ptXYT result)
        {
            result = default(cs2DAlign.ptXYT);
            if (trans2D != null)
            {
                trans2D.MapPoint(point.X, point.Y, out double dx, out double dy);
                result.X = dx;
                result.Y = dy;
                result.T = point.T; //T는 그대로 반환
                return true;
            }
            else
            {
                return false;
            }
        }
        public ICogTransform2D trans2D;
        public cs2DAlign.ptXY dCalCenter = new cs2DAlign.ptXY(); //센터픽셀 origin과의 거리 저장
        public bool CheckerboardCal(double dTileSize, int cbFiducialMark)//, ref List<string> data, CogDisplay cog)
        {
            //CogCalibCheckerboard mCogCalib = new CogCalibCheckerboard();
            //mCogCalib.CalibrationImage = cog.Image;
            //mCogCalib.ComputationMode = CogCalibFixComputationModeConstants.Linear;
            //mCogCalib.FeatureFinder = CogCalibCheckerboardFeatureFinderConstants.CheckerboardExhaustive;
            //mCogCalib.DOFsToCompute = CogCalibCheckerboardDOFConstants.ScalingAspectRotationSkewAndTranslation;
            //mCogCalib.FiducialMark = CogCalibCheckerboardFiducialConstants.None;
            //mCogCalib.PhysicalTileSizeX = dTileSizeX;
            //mCogCalib.PhysicalTileSizeY = dTileSizeY;

            //mCogCalib.Calibrate();
            this.CheckerboardTool.Calibration.Uncalibrate();
            this.CheckerboardTool.Calibration.ComputationMode = CogCalibFixComputationModeConstants.PerspectiveAndRadialWarp;
            this.CheckerboardTool.Calibration.PhysicalTileSizeX = dTileSize;
            this.CheckerboardTool.Calibration.PhysicalTileSizeY = dTileSize;
            this.CheckerboardTool.Calibration.CalibratedOriginSpace = CogCalibCheckerboardAdjustmentSpaceConstants.Uncalibrated;
            this.CheckerboardTool.Calibration.CalibrationImage = this.cogDS.Image;
            this.CheckerboardTool.Calibration.FeatureFinder = CogCalibCheckerboardFeatureFinderConstants.CheckerboardExhaustive; //기본값
            if (cbFiducialMark == 0) this.CheckerboardTool.Calibration.FiducialMark = CogCalibCheckerboardFiducialConstants.StandardRectangles;
            else if (cbFiducialMark == 1) this.CheckerboardTool.Calibration.FiducialMark = CogCalibCheckerboardFiducialConstants.None;
            else if (cbFiducialMark == 2)
            {
                this.CheckerboardTool.Calibration.FiducialMark = CogCalibCheckerboardFiducialConstants.DataMatrixWithGridPitch;
                this.CheckerboardTool.Calibration.FeatureFinder = CogCalibCheckerboardFeatureFinderConstants.CheckerboardExhaustiveMultiRegion;
            }


            this.CheckerboardTool.Calibration.Calibrate();
            this.CheckerboardTool.Run();
            if (this.CheckerboardTool.Calibration.Calibrated)
            {
                trans2D = CheckerboardTool.Calibration.GetComputedUncalibratedFromCalibratedTransform().InvertBase();
            }
            trans2D.MapPoint(cogDS.Image.Width / 2, cogDS.Image.Height / 2, out double centerx, out double centery);
            dCalCenter.X = centerx;
            dCalCenter.Y = centery;
            //string calspace = this.CheckerboardTool.OutputImage.SelectedSpaceName;
            //trans2D = CheckerboardTool.OutputImage.GetTransform(calspace, "#");
            //bCheckerCal = this.CheckerboardTool.Calibration.Calibrated;

            //string calspace = mCogCalib.get.SelectedSpaceName .OutputImage.SelectedSpaceName;
            //trans2D = CheckerboardTool.OutputImage.GetTransform(calspace, "#");
            //bCheckerCal = mCogCalib.Calibrated;

            //if (bCheckerCal)
            //{
            //    for (int i = 0; i < mCogCalib.NumPoints; i++)
            //    {
            //        data.Add(i.ToString() + "," +
            //            mCogCalib.GetRawCalibratedPointX(i).ToString() + "," + // robot
            //            mCogCalib.GetRawCalibratedPointY(i).ToString() + "," +
            //            mCogCalib.GetUncalibratedPointX(i).ToString() + "," + // vision
            //            mCogCalib.GetUncalibratedPointY(i).ToString() + "\r\n");

            //    }

            //    // 포인트의 가로 세로 갯수가 같으면.. 안같을때는?
            //    int step = (int)Math.Sqrt(mCogCalib.NumPoints);
            //    double startx = -1;
            //    double starty = -1;

            //    for (int i = 0; i < step; i++)
            //    {
            //        if (startx < 0 || starty < 0)
            //        {
            //            startx = mCogCalib.GetUncalibratedPointX(i);
            //            starty = mCogCalib.GetUncalibratedPointY(i);
            //            continue;
            //        }

            //        double endx = mCogCalib.GetUncalibratedPointX(i);
            //        double endy = mCogCalib.GetUncalibratedPointY(i);

            //        CogLineSegment cls = new CogLineSegment();
            //        cls.Color = CogColorConstants.Blue;
            //        cls.LineWidthInScreenPixels = 2;
            //        cls.SetStartEnd(startx, starty + (i * 10), endx, endy + (i * 10));

            //        CogGraphicLabel clb = new CogGraphicLabel();

            //        ICogTransform2D trans = mCogCalib.GetComputedUncalibratedFromCalibratedTransform().InvertBase();

            //        double outx, outy;
            //        //double width, height;
            //        trans.MapPoint(startx, starty, out outx, out outy);

            //        CogDistancePointPointTool pp = new CogDistancePointPointTool();
            //        pp.InputImage = cog.Image;
            //        pp.StartX = outx;
            //        pp.StartY = outy;


            //        trans.MapPoint(endx, endy, out outx, out outy);
            //        pp.EndX = outx;
            //        pp.EndY = outy;

            //        pp.Run();

            //        clb.SetXYText(endx, endy, pp.Distance.ToString("N4"));

            //        cog.StaticGraphics.Add(cls, "");
            //        cog.StaticGraphics.Add(clb, "");
            //    }

            //    startx = -1;
            //    starty = -1;

            //    // y step
            //    for (int i = 0; i < mCogCalib.NumPoints; i += step)
            //    {
            //        if (startx < 0 || starty < 0)
            //        {
            //            startx = mCogCalib.GetUncalibratedPointX(i);
            //            starty = mCogCalib.GetUncalibratedPointY(i);
            //            continue;
            //        }

            //        double endx = mCogCalib.GetUncalibratedPointX(i);
            //        double endy = mCogCalib.GetUncalibratedPointY(i);

            //        CogLineSegment cls = new CogLineSegment();
            //        cls.Color = CogColorConstants.Orange;
            //        cls.LineWidthInScreenPixels = 2;
            //        cls.SetStartEnd(startx + (i * 10), starty, endx + (i * 10), endy);

            //        CogGraphicLabel clb = new CogGraphicLabel();

            //        ICogTransform2D trans = mCogCalib.GetComputedUncalibratedFromCalibratedTransform().InvertBase();
            //        double outx, outy;
            //        //double width, height;

            //        trans.MapPoint(startx, starty, out outx, out outy);

            //        CogDistancePointPointTool pp = new CogDistancePointPointTool();
            //        pp.InputImage = cog.Image;
            //        pp.StartX = outx;
            //        pp.StartY = outy;

            //        trans.MapPoint(endx, endy, out outx, out outy);
            //        pp.EndX = outx;
            //        pp.EndY = outy;

            //        pp.Run();

            //        clb.Color = CogColorConstants.DarkRed;

            //        clb.SetXYText(endx, endy, pp.Distance.ToString("N4"));

            //        cogDS.StaticGraphics.Add(cls, "");
            //        cogDS.StaticGraphics.Add(clb, "");

            //    }
            //}

            //if (bCheckerCal) return true;
            //else return false;

            return this.CheckerboardTool.Calibration.Calibrated;
        }
        public bool CheckerboardImgSave(double dsize, int cbFiducialMark)
        {
            string sFolderName = Path.Combine(CONST.cVisionPath, CFG.eCamName, "Calimage");

            if (!Directory.Exists(sFolderName))
                Directory.CreateDirectory(sFolderName);

            string[] sfiles = Directory.GetFiles(sFolderName, "*.bmp", SearchOption.TopDirectoryOnly);

            if (sfiles.Length > 0)
            {
                foreach (var s in sfiles)
                {
                    File.Delete(s);
                }
            }

            string sFileName = Path.Combine(sFolderName, dsize.ToString() + "_" + cbFiducialMark.ToString());
            //string sFileName = "C:\\EQData\\Vision\\Inspection " + (ino + 1).ToString() + "\\" + dTileSizeX.ToString() + "_" + dTileSizeY.ToString() + ".bmp";

            new SaveImage()
            {
                format = System.Drawing.Imaging.ImageFormat.Bmp
            }.Save(cogDS.Image.ToBitmap(), sFileName);

            return true;
        }
        public cs2DAlign.ptXY calcRobot(cs2DAlign.ptXYT p1)
        {
            cs2DAlign.ptXY result = new cs2DAlign.ptXY();

            trans2D.MapPoint(p1.X, p1.Y, out double px, out double py);

            double centerx = cogDS.Image.Width / 2;
            double centery = cogDS.Image.Height / 2;
            trans2D.MapPoint(centerx, centery, out double cx, out double cy);

            result.X = cx - px;
            result.Y = cy - py;

            return result;
        }
        public double calcdeg(cs2DAlign.ptXYT ref1, cs2DAlign.ptXYT ref2, double mtom)
        {
            ucRecipe.MapPosition result = new ucRecipe.MapPosition(0, 0);

            trans2D.MapPoint(ref1.X, ref1.Y, out double outx1, out double outy1);
            trans2D.MapPoint(ref1.X, ref2.Y, out double outx2, out double outy2);
            double diffy = outy2 - outy1;
            double deg = Math.Asin(diffy / mtom);
            //Console.WriteLine("out X = " + outx.ToString());
            //Console.WriteLine("out Y = " + outy.ToString());
            return deg;
        }

        public CogLine setcogline(double deg, cs2DAlign.ptXYT pt)
        {
            CogLine line = new CogLine();
            line.SetXYRotation(pt.X, pt.Y, deg);
            return line;
        }
        public CogLine rightAngleDistIntersection(/*ICogImage cog,*/ CogLine cl, cs2DAlign.ptXYT panel, cs2DAlign.ptXYT fpcb, ref double dx, ref double dy, ref double dist)
        {
            CogDistancePointLineTool tool = new CogDistancePointLineTool();
            tool.InputImage = cogDS.Image;
            tool.X = fpcb.X;
            tool.Y = fpcb.Y;
            tool.Line = cl;
            tool.Run();

            dist = tool.Distance;
            dx = tool.LineX;
            dy = tool.LineY;

            CogLine line = new CogLine();
            line.SetFromStartXYEndXY(fpcb.X, fpcb.Y, dx, dy);
            return line;
        }
        public double ptopLength(cs2DAlign.ptXYT panel, double dx, double dy)
        {
            CogDistancePointPointTool tool = new CogDistancePointPointTool();
            tool.InputImage = cogDS.Image;
            tool.StartX = panel.X;
            tool.StartY = panel.Y;
            tool.EndX = dx;
            tool.EndY = dy;
            tool.Run();

            return tool.Distance;
        }

        int numAcqs;

        private void Operator_Complete(object sender, CogCompleteEventArgs e)
        {
            if (cogDS.InvokeRequired)
            {
                cogDS.Invoke(new CogCompleteEventHandler(Operator_Complete),
                    new object[] { sender, e });
                return;
            }

            if (mTrigger.TriggerModel == CogAcqTriggerModelConstants.Auto)
            {
                int numReadyVal = 0, numPendingVal = 0;
                bool busyVal;
                CogAcqInfo info = new CogAcqInfo();

                try
                {
                    cogAcqFifoTool.Operator.GetFifoState(out numPendingVal, out numReadyVal, out busyVal);

                    if (numReadyVal > 0)
                        cogDS.Image =
                            cogAcqFifoTool.Operator.CompleteAcquireEx(info);

                    numAcqs++;
                    if (numAcqs > 4)
                    {
                        GC.Collect();
                        numAcqs = 0;
                    }

                }
                catch (CogException ce)
                {
                    System.Windows.Forms.MessageBox.Show("The following error has occured\n" + ce.Message);
                }
            } // if.
            else
            {

            }

        }

        double dIniDistX, dIniDistY;
        public bool bDistDisplay;

        public void MouseDown(double dX, double dY)
        {
            ICogTransform2D iTransPosition;
            CogCoordinateSpaceTree cogCoordinateSpaceTree;

            cogCoordinateSpaceTree = cogDS.UserDisplayTree;

            string lcTempName = "";
            if (cogCoordinateSpaceTree == null) lcTempName = "pos";
            else lcTempName = cogCoordinateSpaceTree.RootName;

            iTransPosition = cogDS.GetTransform("#", lcTempName);
            iTransPosition.MapPoint(dX, dY, out dIniDistX, out dIniDistY);

            bDistDisplay = true;
        }

        public void DistClear()
        {
            try
            {
                this.DeleteInteractiveGraphic(cogDS, "DIST");
            }
            catch { }
        }
        public bool bcali = true;
        private void tmrLive_Tick(object sender, EventArgs e)
        {
            try
            {
                //pcy190617 Bendingno 쓰는데 없어서 삭제
                //int ManualNo = 0;
                //int ManualNo = -1;
                //switch (CFG.Name.Trim())
                //{
                //    case "BENDING SIDE1": ManualNo = 0; break;
                //    case "BENDING SIDE2": ManualNo = 1; break;
                //    case "BENDING SIDE3": ManualNo = 2; break;
                //}

                //if (CONST.m_bAutoStart && ManualNo > -1)
                //{
                //    if (CONST.BendingsideCapture[ManualNo] || CONST.BendingPressCapture[ManualNo] || CONST.bSideFileSave[ManualNo])
                //    {
                //        liveProcess(bcali);
                //    }
                //}
                //else
                liveProcess(bcali);

                //liveProcess();// ManualNo);

                //pcy190617 liveEndCheck이건 평생 false로 보임
                if (liveEndCheck)
                {
                    liveEndCheck = false;

                    tmrLive.Enabled = false;

                    liveEnd.Add(true);
                }
            }
            catch (Exception EX)
            {
                string msg;
                msg = EX.Message;
            }
        }

        public void MouseMove(double dX, double dY)
        {
            try
            {
                if (bDistDisplay)
                {
                    ICogTransform2D iTransPosition;
                    CogCoordinateSpaceTree cogCoordinateSpaceTree;
                    CogLineSegment cogLine = new CogLineSegment();
                    cogCoordinateSpaceTree = cogDS.UserDisplayTree;

                    string lcTempName = "";
                    if (cogCoordinateSpaceTree == null) lcTempName = "pos";
                    else lcTempName = cogCoordinateSpaceTree.RootName;
                    iTransPosition = cogDS.GetTransform("#", lcTempName);
                    double dMovePointX, dMovePointY;

                    iTransPosition.MapPoint(dX, dY, out dMovePointX, out dMovePointY);

                    cogLine.StartX = dIniDistX;
                    cogLine.StartY = dIniDistY;
                    cogLine.EndX = dMovePointX;
                    cogLine.EndY = dMovePointY;

                    double dDist = Math.Pow(Math.Pow(dMovePointX - dIniDistX, 2) + Math.Pow(dMovePointY - dIniDistY, 2), 0.5);
                    string sKind = "DIST";

                    DistDisplay(sKind, cogLine, dMovePointX, dMovePointY, dDist);
                }
            }
            catch { }
        }

        private void DistDisplay(string sKind, CogLineSegment cL, double dX, double dY, double dDist)
        {
            try
            {
                cogDS.InteractiveGraphics.Remove(sKind);
            }
            catch { }


            cL.Color = CogColorConstants.Blue;
            cL.LineWidthInScreenPixels = 2;
            cogDS.InteractiveGraphics.Add(cL, sKind, false);

            dDist = dDist * CFG.Resolution * 1000;
            cogLabel.SetXYText(dX + 40, dY + 50, dDist.ToString("0") + "(um)");
            cogLabel.Color = CogColorConstants.Green;
            cogDS.InteractiveGraphics.Add(cogLabel, sKind, false);

            //WriteCogLabe(dDist.ToString("0") + "(um)", font6Bold, CogColorConstants.Green, dX + 40, dY + 50, false, sKind);
        }

        public void MouseUp()
        {
            bDistDisplay = false;
        }

        public void Line3eaDsip(bool Clear, ref double dist, int Cam1 = 0)
        {
            try
            {

                if (!Clear)
                {
                    double CenterX = ImgX / 2;
                    double CenterY = ImgY / 2;

                    CogLineSegment cogLine1 = new CogLineSegment();
                    CogLineSegment cogLine2 = new CogLineSegment();
                    CogLineSegment cogLine3 = new CogLineSegment();
                    CogLineSegment cogLine4 = new CogLineSegment();
                    CogLineSegment cogLine5 = new CogLineSegment();
                    CogLineSegment cogLine6 = new CogLineSegment();

                    cogLine1.StartX = 0;
                    cogLine1.EndX = ImgX;
                    cogLine1.StartY = (CenterY) + (CenterY / 2);
                    cogLine1.EndY = (CenterY) + (CenterY / 2);

                    cogLine2.StartX = 0;
                    cogLine2.EndX = ImgX;
                    cogLine2.StartY = (ImgY / 2) - (CenterY / 2);
                    cogLine2.EndY = (ImgY / 2) - (CenterY / 2);

                    cogLine3.StartX = (CenterX) + (CenterX / 2);
                    cogLine3.EndX = (CenterX) + (CenterX / 2);
                    cogLine3.StartY = 0;
                    cogLine3.EndY = ImgY;

                    cogLine4.StartX = (CenterX) - (CenterX / 2);
                    cogLine4.EndX = (CenterX) - (CenterX / 2);
                    cogLine4.StartY = 0;
                    cogLine4.EndY = ImgY;


                    cogLine5.StartX = 0;
                    cogLine5.EndX = ImgX;
                    cogLine5.StartY = (CenterY) + (CenterY / 2) + (CenterY / 4);
                    cogLine5.EndY = (CenterY) + (CenterY / 2) + (CenterY / 4);

                    if (Cam1 == 0)
                    {
                        cogLine6.StartX = (CenterX) - (CenterX / 2) - (CenterX / 4);
                        cogLine6.EndX = (CenterX) - (CenterX / 2) - (CenterX / 4);
                        cogLine6.StartY = 0;
                        cogLine6.EndY = ImgY;
                    }
                    else
                    {
                        cogLine6.StartX = (CenterX) + (CenterX / 2) + (CenterX / 4);
                        cogLine6.EndX = (CenterX) + (CenterX / 2) + (CenterX / 4);
                        cogLine6.StartY = 0;
                        cogLine6.EndY = ImgY;
                    }

                    dist = (CenterX) - (cogLine6.StartX);


                    cogLine1.Color = CogColorConstants.Yellow;
                    cogDS.InteractiveGraphics.Add(cogLine1, "Line3eaDist", false);

                    cogLine2.Color = CogColorConstants.Yellow;
                    cogDS.InteractiveGraphics.Add(cogLine2, "Line3eaDist", false);

                    cogLine3.Color = CogColorConstants.Yellow;
                    cogDS.InteractiveGraphics.Add(cogLine3, "Line3eaDist", false);

                    cogLine4.Color = CogColorConstants.Yellow;
                    cogDS.InteractiveGraphics.Add(cogLine4, "Line3eaDist", false);

                    cogLine5.Color = CogColorConstants.Green;
                    cogDS.InteractiveGraphics.Add(cogLine5, "Line3eaDist", false);


                    cogLine6.Color = CogColorConstants.Green;
                    cogDS.InteractiveGraphics.Add(cogLine6, "Line3eaDist", false);


                }
                else
                {
                    try
                    {
                        cogDS.InteractiveGraphics.Remove("Line3eaDist");
                    }
                    catch
                    { }
                }

            }
            catch { }
        }
        public void LineDisp(double dist, bool LineX = false, int LineY = 0)
        {
            try
            {
                //pcy190408 resolution 디폴트값 추가.
                //pcy190510 항상 고정 resol사용.
                //pcy190607 arm과 stage거리를 vision화면으로 재야해서 resolution값을 써야함. 첫 세팅때는 default값을 ini에 넣어놔야할듯..
                double dresolution = 0.012;
                if (double.IsNaN(CFG.Resolution) || CFG.Resolution == 0)
                {
                    dresolution = 0.012;
                }
                else dresolution = CFG.Resolution;
                if (dist != 0)
                {
                    if (!LineX)
                    {
                        CogLineSegment cogLine = new CogLineSegment();
                        cogLine.StartX = 0;
                        cogLine.EndX = ImgX;
                        cogLine.StartY = (ImgY / 2) + (dist / dresolution);
                        cogLine.EndY = (ImgY / 2) + (dist / dresolution);

                        cogLine.Color = CogColorConstants.Red;
                        cogDS.InteractiveGraphics.Add(cogLine, "LineDistY" + LineY.ToString(), false);
                    }
                    else
                    {
                        CogLineSegment cogLine = new CogLineSegment();
                        cogLine.StartX = (ImgX / 2) + (dist / dresolution);
                        cogLine.EndX = (ImgX / 2) + (dist / dresolution);
                        cogLine.StartY = 0;
                        cogLine.EndY = ImgY;

                        cogLine.Color = CogColorConstants.Red;
                        cogDS.InteractiveGraphics.Add(cogLine, "LineDistX", false);
                    }
                }
                else
                {
                    try
                    {
                        if (!LineX) cogDS.InteractiveGraphics.Remove("LineDistY" + LineY.ToString());
                        else cogDS.InteractiveGraphics.Remove("LineDistX");
                    }
                    catch
                    {
                    }
                }
            }
            catch { }
        }
        public void TargetLineDisp(double pX, double pY, double dR = 0)
        {
            try
            {
                //pcy190408 resolution 디폴트값 추가.
                //pcy190510 항상 고정 resol사용.
                //pcy190607 arm과 stage거리를 vision화면으로 재야해서 resolution값을 써야함. 첫 세팅때는 default값을 ini에 넣어놔야할듯..
                double dresolution = 0.012;
                if (double.IsNaN(CFG.Resolution) || CFG.Resolution == 0)
                {
                    dresolution = 0.012;
                }
                else dresolution = CFG.Resolution;
                if (pX != 0)
                {
                    double dShiftX = 0;
                    double dShiftY = 0;
                    rDegreetodistance(dR, ref dShiftX, ref dShiftY);

                    CogLineSegment cogLine = new CogLineSegment();
                    cogLine.StartX = 0;
                    cogLine.EndX = ImgX;
                    cogLine.StartY = pY;
                    cogLine.EndY = pY + dShiftX;


                    cogLine.Color = CogColorConstants.Red;
                    cogDS.InteractiveGraphics.Add(cogLine, "TargetLineX", false);

                    CogLineSegment cogLine1 = new CogLineSegment();
                    cogLine1.StartX = pX;
                    cogLine1.EndX = pX;
                    cogLine1.StartY = 0;
                    cogLine1.EndY = ImgY;

                    cogLine1.Color = CogColorConstants.Red;
                    cogDS.InteractiveGraphics.Add(cogLine1, "LineDistX", false);
                }
            }
            catch { }
        }
        public double rDegreetodistance(double dDegree, ref double DriftX, ref double DriftY)
        {
            double cX = 0;

            double rAngle = dDegree;
            //cX = Math.Cos(3.1415f / 180 * dDegree) * dX;

            DriftX = Math.Cos(rAngle) - Math.Sin(rAngle);
            DriftY = Math.Sin(rAngle) + Math.Cos(rAngle);
            return cX;
        }
        public void VLineDisp(double dist)
        {
            try
            {
                if (dist != 0)
                {
                    CogLineSegment cogLine = new CogLineSegment();
                    cogLine.StartX = (CFG.FOVX / 2.0 + dist) / CFG.Resolution;
                    cogLine.EndX = cogLine.StartX;
                    cogLine.StartY = 0;
                    cogLine.EndY = CFG.FOVY / CFG.Resolution;

                    cogLine.Color = CogColorConstants.Yellow;
                    cogDS.InteractiveGraphics.Add(cogLine, "Dist", false);
                }
                else
                {
                    try
                    {
                        cogDS.InteractiveGraphics.Remove("Dist");
                    }
                    catch
                    {
                    }
                }
            }
            catch { }
        }

        public CogCircle lcTempCogCircle = new CogCircle();
        public CogLineSegment lcTempCogLineSegmentSide1 = new CogLineSegment();
        public CogLineSegment lcTempCogLineSegmentSide2 = new CogLineSegment();

        public void Overay(bool bON)
        {
            try
            {
                if (!bON)
                {
                    cogDS.InteractiveGraphics.Clear();
                }
                else
                {
                    //JJ , 2017-05-19 : Overlay Display ------- start
                    double width = cogDS.Image.Width;
                    double height = cogDS.Image.Height;

                    CogLineSegment[] lcTempCogLineSegment = new CogLineSegment[6];
                    for (int i = 0; i < lcTempCogLineSegment.Length; i++)
                    {
                        lcTempCogLineSegment[i] = new CogLineSegment();
                    }

                    //외곽
                    //LeftTop -> RightTop
                    {
                        CogLineSegment line = lcTempCogLineSegment[0];
                        line.StartX = 0;
                        line.StartY = 0;
                        line.EndX = width;
                        line.EndY = 0;
                    }
                    //LeftTop -> RightBottom
                    {
                        CogLineSegment line = lcTempCogLineSegment[1];
                        line.StartX = width;
                        line.StartY = 0;
                        line.EndX = width;
                        line.EndY = height;
                    }
                    //RightBottom -> LeftBottom
                    {
                        CogLineSegment line = lcTempCogLineSegment[2];
                        line.StartX = width;
                        line.StartY = height;
                        line.EndX = 0;
                        line.EndY = height;
                    }
                    //LeftBottom -> LeftTop
                    {
                        CogLineSegment line = lcTempCogLineSegment[3];
                        line.StartX = 0;
                        line.StartY = height;
                        line.EndX = 0;
                        line.EndY = 0;
                    }

                    //중심
                    //MidWidth
                    {
                        CogLineSegment line = lcTempCogLineSegment[4];
                        line.StartX = 0;
                        line.StartY = height / 2;
                        line.EndX = width;
                        line.EndY = height / 2;
                    }
                    //MidHeight
                    {
                        CogLineSegment line = lcTempCogLineSegment[5];
                        line.StartX = width / 2;
                        line.StartY = 0;
                        line.EndX = width / 2;
                        line.EndY = height;
                    }
                    foreach (var lineSegment in lcTempCogLineSegment)
                    {
                        cogDS.InteractiveGraphics.Add(lineSegment, "", false);
                    }

                }
            }
            catch
            {

            }
        }

        //public void Overay(bool bON, bool bBending = false)
        //{
        //    try
        //    {
        //        if (!bON)
        //        {
        //            cogDS.InteractiveGraphics.Clear();
        //        }
        //        else
        //        {
        //            if (cogDS == null)
        //            {
        //                return;
        //            }
        //            cogDS.InteractiveGraphics.Clear();

        //            CogLineSegment CenterCrossLineX = new CogLineSegment();
        //            CenterCrossLineX.StartX = 0;
        //            CenterCrossLineX.EndX = CFG.Resolution;
        //            CenterCrossLineX.StartY = CFG.Resolution / 2;
        //            CenterCrossLineX.EndY = CFG.Resolution / 2;

        //            CenterCrossLineX.Color = CogColorConstants.Green;
        //            cogDS.InteractiveGraphics.Add(CenterCrossLineX, "", false);

        //            if (bBending)
        //            {
        //                //CogLineSegment CenterCrossLineX1 = new CogLineSegment();
        //                //CenterCrossLineX1.StartX = 0;
        //                //CenterCrossLineX1.EndX = CFG.Resolution;
        //                //CenterCrossLineX1.StartY = CFG.Resolution / 2 + CONST.Resolution / CFG.Resolution;
        //                //CenterCrossLineX1.EndY = CFG.Resolution / 2 + CONST.Resolution / CFG.Resolution;

        //                //CenterCrossLineX1.Color = CogColorConstants.Green;
        //                //cogDS.InteractiveGraphics.Add(CenterCrossLineX1, "", false);

        //                //CogLineSegment CenterCrossLineX2 = new CogLineSegment();
        //                //CenterCrossLineX2.StartX = 0;
        //                //CenterCrossLineX2.EndX = CFG.Resolution;
        //                //CenterCrossLineX2.StartY = CFG.Resolution / 2 - CONST.m_dM2Shape / CFG.Resolution;
        //                //CenterCrossLineX2.EndY = CFG.Resolution / 2 - CONST.m_dM2Shape / CFG.Resolution;

        //                //CenterCrossLineX2.Color = CogColorConstants.Green;
        //                //cogDS.InteractiveGraphics.Add(CenterCrossLineX2, "", false);
        //            }

        //            CogLineSegment CenterCrossLineY = new CogLineSegment();
        //            CenterCrossLineY.StartX = CFG.Resolution / 2;
        //            CenterCrossLineY.EndX = CFG.Resolution / 2;
        //            CenterCrossLineY.StartY = 0;
        //            CenterCrossLineY.EndY = CFG.Resolution;

        //            CenterCrossLineY.Color = CogColorConstants.Green;
        //            cogDS.InteractiveGraphics.Add(CenterCrossLineY, "", false);

        //        }
        //    }
        //    catch
        //    {

        //    }
        //}

        private void interCapture(bool bcali)
        {

            int acqTicket, completeTicket, triggerNumber, numPending, numReady;
            bool busy;



            //int ManualNo = -1;

            //switch (CFG.Name.Trim())
            //{
            //    case "BENDING SIDE1": ManualNo = 0; break;
            //    case "BENDING SIDE2": ManualNo = 1; break;
            //    case "BENDING SIDE3": ManualNo = 2; break;
            //}

            acqTicket = cogAcqFifoTool.Operator.StartAcquire();
            do
            {
                cogAcqFifoTool.Operator.GetFifoState(out numPending, out numReady, out busy);

                if (numReady > 0)
                {
                    Bitmap bmpTest = null;
                    bmpTest = cogAcqFifoTool.Operator.CompleteAcquire(acqTicket, out completeTicket, out triggerNumber).ToBitmap();
                    if (CFG.nReverseMode == CONST.eImageReverse.None)    // None
                    {

                    }
                    else if (CFG.nReverseMode == CONST.eImageReverse.XReverse)    // X Reverse
                    {
                        bmpTest.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    }
                    else if (CFG.nReverseMode == CONST.eImageReverse.YReverse)    // Y Reverse
                    {
                        bmpTest.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    }
                    else if (CFG.nReverseMode == CONST.eImageReverse.AllReverse)    // All Reverse
                    {
                        bmpTest.RotateFlip(RotateFlipType.RotateNoneFlipXY);
                    }
                    else if (CFG.nReverseMode == CONST.eImageReverse.Reverse90)    // 90 Reverse
                    {
                        bmpTest.RotateFlip(RotateFlipType.Rotate90FlipX);
                    }
                    else if (CFG.nReverseMode == CONST.eImageReverse.Reverse270)    // 270 Reverse
                    {
                        bmpTest.RotateFlip(RotateFlipType.Rotate270FlipX);
                    }
                    else if (CFG.nReverseMode == CONST.eImageReverse.Reverse90XY)    // 90 Reverse
                    {
                        bmpTest.RotateFlip(RotateFlipType.Rotate90FlipXY);
                    }
                    else if (CFG.nReverseMode == CONST.eImageReverse.Reverse270XY)    // 270 Reverse
                    {
                        bmpTest.RotateFlip(RotateFlipType.Rotate270FlipXY);
                    }

                    if (bmpTest != null)
                    {
                        if (CheckerboardTool.Calibration.Calibrated && bcali)
                        {
                            CheckerboardTool.InputImage = new CogImage8Grey(bmpTest);
                            CheckerboardTool.Run();
                            cogDS.Image = CheckerboardTool.OutputImage;
                        }
                        else
                            cogDS.Image = new CogImage8Grey(bmpTest);
                    }
                    if (CFG.SideVision)
                    {
                        if (CFG.SideImgSave)  //CFG Bending Side Capture 설정 확인
                        {
                            //side 방식 변경으로 주석
                            //if (CONST.BendingsideCapture[ManualNo] || CONST.BendingPressCapture[ManualNo])
                            //    Sidecapture(bmpTest, ManualNo);//(new Bitmap(cogDS.CreateContentBitmap(Cognex.VisionPro.Display.CogDisplayContentBitmapConstants.Custom)));  //2017.08.09 Bending side capture
                        }
                    }

                }
                try
                {
                    Thread.Sleep(1);
                }
                catch
                {

                }

            } while (numReady <= 0);

            imCnt++;
            if (imCnt > 100)
            {
                GC.Collect();
                imCnt = 0;
            }

        }

        //side 방식 변경으로 주석
        //int[] TestNo = new int[3];
        //private void Sidecapture(Bitmap dmap, int ManualNo)
        //{
        //    if (CONST.BendingsideCapture[ManualNo])
        //    {
        //        if (CONST.BendingCaptureNo[ManualNo] < 50)
        //        {
        //            Bitmaplist[CONST.BendingCaptureNo[ManualNo]] = dmap;
        //            //Bitmaplist[ManualNo, CONST.BendingCaptureNo[ManualNo]] = dmap;
        //            CONST.BendingCaptureNo[ManualNo]++;
        //        }
        //        else
        //        {
        //            CONST.BendingsideCapture[ManualNo] = false;
        //        }
        //    }
        //    else if (CONST.BendingPressCapture[ManualNo]) //Bending Align 완료 후 촬상
        //    {
        //        if (TestNo[ManualNo] < 10)      //Press에 대한 신호가 없기 때문에 임의로 Count check
        //        {
        //            Bitmaplist[CONST.BendingCaptureNo[ManualNo]] = dmap;
        //            CONST.BendingCaptureNo[ManualNo]++;
        //            TestNo[ManualNo]++;
        //        }
        //        else
        //        {
        //            CONST.BendingPressCapture[ManualNo] = false;
        //            //lkw 20170817 Capture Cnt 수정함. 저장하는 부분때문에
        //            //  CONST.BendingCaptureNo[ManualNo] = 0;
        //            TestNo[ManualNo] = 0;
        //            CONST.bSideFileSave[ManualNo] = true;

        //        }
        //    }
        //}
        CogIPOneImageOperators cogipo = new CogIPOneImageOperators();
        public ICogImage SideCapture2(bool bcali = false)
        {
            int acqTicket, completeTicket, triggerNumber, numPending, numReady;
            bool busy;

            acqTicket = cogAcqFifoTool.Operator.StartAcquire();
            //do
            //{
            //    cogAcqFifoTool.Operator.GetFifoState(out numPending, out numReady, out busy);

            //    if (numReady > 0)
            //    {
            //        //이걸로 회전하면 pmalign에 패턴등록시 spacename이 .으로 고정이라서 문제됨. 패턴등록할때는 기존 bmp로 캡쳐하도록 변환필요
            //        CogIPOneImageTool cogIP = new CogIPOneImageTool();
            //        if (CFG.nReverseMode == CONST.eImageReverse.None)    // None
            //        {
            //            cogDS.Image = cogAcqFifoTool.Operator.CompleteAcquire(acqTicket, out completeTicket, out triggerNumber);
            //        }
            //        else if (CFG.nReverseMode == CONST.eImageReverse.XReverse)    // X Reverse
            //        {
            //            cogIP.InputImage = cogAcqFifoTool.Operator.CompleteAcquire(acqTicket, out completeTicket, out triggerNumber);

            //            cogIP.Operators.Add(cogipo[(int)CogIPOneImageFlipRotateOperationConstants.Flip]);
            //            cogIP.Run();
            //            cogDS.Image = cogIP.OutputImage;
            //        }
            //        else if (CFG.nReverseMode == CONST.eImageReverse.YReverse)    // Y Reverse
            //        {
            //            cogIP.InputImage = cogAcqFifoTool.Operator.CompleteAcquire(acqTicket, out completeTicket, out triggerNumber);

            //            cogIP.Operators.Add(cogipo[(int)CogIPOneImageFlipRotateOperationConstants.FlipAndRotate180Deg]);
            //            cogIP.Run();
            //            cogDS.Image = cogIP.OutputImage;
            //        }
            //        else if (CFG.nReverseMode == CONST.eImageReverse.AllReverse)    // All Reverse
            //        {
            //            cogIP.InputImage = cogAcqFifoTool.Operator.CompleteAcquire(acqTicket, out completeTicket, out triggerNumber);

            //            cogIP.Operators.Add(cogipo[(int)CogIPOneImageFlipRotateOperationConstants.Rotate180Deg]);
            //            cogIP.Run();
            //            cogDS.Image = cogIP.OutputImage;
            //        }
            //        else if (CFG.nReverseMode == CONST.eImageReverse.Reverse90)    // 90 Reverse
            //        {
            //            cogIP.InputImage = cogAcqFifoTool.Operator.CompleteAcquire(acqTicket, out completeTicket, out triggerNumber);

            //            cogIP.Operators.Add(cogipo[(int)CogIPOneImageFlipRotateOperationConstants.Rotate90Deg]);
            //            cogIP.Run();
            //            cogDS.Image = cogIP.OutputImage;
            //        }
            //        else if (CFG.nReverseMode == CONST.eImageReverse.Reverse270)    // 270 Reverse
            //        {
            //            cogIP.InputImage = cogAcqFifoTool.Operator.CompleteAcquire(acqTicket, out completeTicket, out triggerNumber);

            //            cogIP.Operators.Add(cogipo[(int)CogIPOneImageFlipRotateOperationConstants.Rotate270Deg]);
            //            cogIP.Run();
            //            cogDS.Image = cogIP.OutputImage;
            //        }
            //        else if (CFG.nReverseMode == CONST.eImageReverse.Reverse90XY)    // 90 Reverse
            //        {
            //            cogIP.InputImage = cogAcqFifoTool.Operator.CompleteAcquire(acqTicket, out completeTicket, out triggerNumber);

            //            cogIP.Operators.Add(cogipo[(int)CogIPOneImageFlipRotateOperationConstants.FlipAndRotate90Deg]);
            //            cogIP.Run();
            //            cogDS.Image = cogIP.OutputImage;
            //        }
            //        else if (CFG.nReverseMode == CONST.eImageReverse.Reverse270XY)    // 270 Reverse
            //        {
            //            cogIP.InputImage = cogAcqFifoTool.Operator.CompleteAcquire(acqTicket, out completeTicket, out triggerNumber);

            //            cogIP.Operators.Add(cogipo[(int)CogIPOneImageFlipRotateOperationConstants.FlipAndRotate270Deg]);
            //            cogIP.Run();
            //            cogDS.Image = cogIP.OutputImage;
            //        }
            //        if (CFG.SideVision && CFG.SideImgSave) //CFG Bending Side Capture 설정 확인
            //        {
            //            return cogDS.Image;
            //        }
            //        else
            //        {
            //            return null;
            //        }
            //    }

            //    try
            //    {
            //        Thread.Sleep(1);
            //    }
            //    catch
            //    {

            //    }

            //} while (numReady <= 0);
            do
            {
                cogAcqFifoTool.Operator.GetFifoState(out numPending, out numReady, out busy);

                if (numReady > 0)
                {
                    Bitmap bmpTest = null;
                    bmpTest = cogAcqFifoTool.Operator.CompleteAcquire(acqTicket, out completeTicket, out triggerNumber).ToBitmap();
                    if (CFG.nReverseMode == CONST.eImageReverse.None)    // None
                    {

                    }
                    else if (CFG.nReverseMode == CONST.eImageReverse.XReverse)    // X Reverse
                    {
                        bmpTest.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    }
                    else if (CFG.nReverseMode == CONST.eImageReverse.YReverse)    // Y Reverse
                    {
                        bmpTest.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    }
                    else if (CFG.nReverseMode == CONST.eImageReverse.AllReverse)    // All Reverse
                    {
                        bmpTest.RotateFlip(RotateFlipType.RotateNoneFlipXY);
                    }
                    else if (CFG.nReverseMode == CONST.eImageReverse.Reverse90)    // 90 Reverse
                    {
                        bmpTest.RotateFlip(RotateFlipType.Rotate90FlipX);
                    }
                    else if (CFG.nReverseMode == CONST.eImageReverse.Reverse270)    // 270 Reverse
                    {
                        bmpTest.RotateFlip(RotateFlipType.Rotate270FlipX);
                    }
                    else if (CFG.nReverseMode == CONST.eImageReverse.Reverse90XY)    // 90 Reverse
                    {
                        bmpTest.RotateFlip(RotateFlipType.Rotate90FlipXY);
                    }
                    else if (CFG.nReverseMode == CONST.eImageReverse.Reverse270XY)    // 270 Reverse
                    {
                        bmpTest.RotateFlip(RotateFlipType.Rotate270FlipXY);
                    }

                    if (bmpTest != null)
                    {
                        if (CheckerboardTool.Calibration.Calibrated && bcali)
                        {
                            CheckerboardTool.InputImage = new CogImage8Grey(bmpTest);
                            CheckerboardTool.Run();
                            cogDS.Image = CheckerboardTool.OutputImage;
                        }
                        else
                            cogDS.Image = new CogImage8Grey(bmpTest);
                        return cogDS.Image;
                    }
                }
                try
                {
                    Thread.Sleep(1);
                }
                catch
                {

                }

            } while (numReady <= 0);

            imCnt++;
            if (imCnt > 100)
            {
                GC.Collect();
                imCnt = 0;
            }
            return null;
        }

        private void liveProcess(bool bcali)//int bNo)
        {
            interCapture(bcali);
        }


        public void Live(bool lcON)
        {
            try
            {
                //if(liveEndEvent == null)
                //    this.liveEndEvent = eventHandler;

                if (lcON)
                {
                    //cogDS.StartLiveDisplay(cogAcqFifo[0], true);
                    cogDS.AutoFit = true;

                    imCnt = 0;

                    tmrLive.Enabled = true;

                    liveOn = true;
                }
                else
                {
                    tmrLive.Enabled = false;

                    liveOn = false;
                }
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("Live" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }
        bool bCheckerCal = false;
        // 20200908 cjm Calibration 진행시 이미지를 저장하기 위해서 변경
        public bool Capture(bool bSave, bool liveCapture = true, bool fail = false, bool bcali = true, string Name = null)
        {
            try
            {
                Thread.Sleep(CFG.GrabDelay);

                Grabtimewatch.Reset();
                Grabtimewatch.Start();

                Live(false);

                if (cogAcqFifoTool == null)
                {
                    cogAcqFifoTool = new CogAcqFifoTool();

                    cogAcqFifoTool.Operator = cogAcqFifo[0];
                }

                if (liveCapture) // live 일때만 들어오게 . open으로 했을 경우 안되는 현상 해결. lyw.
                {
                    //KSJ 20170606 ImageReverse Delete
                    /*int lcTriggerNO = 0;
                    cogDS.Image = cogAcqFifo[0].Acquire(out lcTriggerNO);

                    cogDS.AutoFit = true;*/

                    //KSJ 20170606 ImageReverse Add
                    int acqTicket, completeTicket, triggerNumber, numPending, numReady;
                    bool busy;


                    acqTicket = cogAcqFifoTool.Operator.StartAcquire();

                    do
                    {
                        cogAcqFifoTool.Operator.GetFifoState(out numPending, out numReady, out busy);

                        if (numReady > 0)
                        {
                            Bitmap bmpTest = null;
                            bmpTest = cogAcqFifoTool.Operator.CompleteAcquire(acqTicket, out completeTicket, out triggerNumber).ToBitmap();
                            if (CFG.nReverseMode == CONST.eImageReverse.None)    // None
                            {

                            }
                            else if (CFG.nReverseMode == CONST.eImageReverse.XReverse)    // X Reverse
                            {
                                bmpTest.RotateFlip(RotateFlipType.RotateNoneFlipX);
                            }
                            else if (CFG.nReverseMode == CONST.eImageReverse.YReverse)    // Y Reverse
                            {
                                bmpTest.RotateFlip(RotateFlipType.RotateNoneFlipY);
                            }
                            else if (CFG.nReverseMode == CONST.eImageReverse.AllReverse)    // All Reverse
                            {
                                bmpTest.RotateFlip(RotateFlipType.RotateNoneFlipXY);
                            }
                            else if (CFG.nReverseMode == CONST.eImageReverse.Reverse90)    // 90 Reverse
                            {
                                bmpTest.RotateFlip(RotateFlipType.Rotate90FlipX);
                            }
                            else if (CFG.nReverseMode == CONST.eImageReverse.Reverse270)    // 270 Reverse
                            {
                                bmpTest.RotateFlip(RotateFlipType.Rotate270FlipX);
                            }
                            else if (CFG.nReverseMode == CONST.eImageReverse.Reverse90XY)    // 90 Reverse
                            {
                                bmpTest.RotateFlip(RotateFlipType.Rotate90FlipXY);
                            }
                            else if (CFG.nReverseMode == CONST.eImageReverse.Reverse270XY)    // 270 Reverse
                            {
                                bmpTest.RotateFlip(RotateFlipType.Rotate270FlipXY);
                            }
                            #region 이걸로 변환하면 checkercal이 안됨 추후 해볼것
                            //CogIPOneImageTool cogIP = new CogIPOneImageTool();
                            //ICogImage temp = null;

                            //cogIP.InputImage = cogAcqFifoTool.Operator.CompleteAcquire(acqTicket, out completeTicket, out triggerNumber);
                            //if (CFG.nReverseMode == CONST.eImageReverse.None)    // None
                            //{
                            //    cogIP.Run();
                            //    temp = cogIP.OutputImage;
                            //}
                            //else if (CFG.nReverseMode == CONST.eImageReverse.XReverse)    // X Reverse
                            //{
                            //    cogIP.Operators.Add(cogipo[(int)CogIPOneImageFlipRotateOperationConstants.Flip]);
                            //    cogIP.Run();
                            //    temp = cogIP.OutputImage;
                            //}
                            //else if (CFG.nReverseMode == CONST.eImageReverse.YReverse)    // Y Reverse
                            //{
                            //    cogIP.Operators.Add(cogipo[(int)CogIPOneImageFlipRotateOperationConstants.FlipAndRotate180Deg]);
                            //    cogIP.Run();
                            //    temp = cogIP.OutputImage;
                            //}
                            //else if (CFG.nReverseMode == CONST.eImageReverse.AllReverse)    // All Reverse
                            //{
                            //    cogIP.Operators.Add(cogipo[(int)CogIPOneImageFlipRotateOperationConstants.Rotate180Deg]);
                            //    cogIP.Run();
                            //    temp = cogIP.OutputImage;
                            //}
                            //else if (CFG.nReverseMode == CONST.eImageReverse.Reverse90)    // 90 Reverse
                            //{
                            //    cogIP.Operators.Add(cogipo[(int)CogIPOneImageFlipRotateOperationConstants.Rotate90Deg]);
                            //    cogIP.Run();
                            //    temp = cogIP.OutputImage;
                            //}
                            //else if (CFG.nReverseMode == CONST.eImageReverse.Reverse270)    // 270 Reverse
                            //{
                            //    cogIP.Operators.Add(cogipo[(int)CogIPOneImageFlipRotateOperationConstants.Rotate270Deg]);
                            //    cogIP.Run();
                            //    temp = cogIP.OutputImage;
                            //}
                            //else if (CFG.nReverseMode == CONST.eImageReverse.Reverse90XY)    // 90 Reverse
                            //{
                            //    cogIP.Operators.Add(cogipo[(int)CogIPOneImageFlipRotateOperationConstants.FlipAndRotate90Deg]);
                            //    cogIP.Run();
                            //    temp = cogIP.OutputImage;
                            //}
                            //else if (CFG.nReverseMode == CONST.eImageReverse.Reverse270XY)    // 270 Reverse
                            //{
                            //    cogIP.Operators.Add(cogipo[(int)CogIPOneImageFlipRotateOperationConstants.FlipAndRotate270Deg]);
                            //    cogIP.Run();
                            //    temp = cogIP.OutputImage;
                            //}

                            //if (temp != null)
                            //{
                            //    if (CheckerboardTool.Calibration.Calibrated && bcali)
                            //    {
                            //        CheckerboardTool.InputImage = temp;
                            //        CheckerboardTool.Run();
                            //        cogDS.Image = CheckerboardTool.OutputImage;
                            //    }
                            //    else
                            //        cogDS.Image = temp;
                            //}
                            #endregion
                            if (bmpTest != null)
                            {
                                if (CheckerboardTool.Calibration.Calibrated && bcali)
                                {
                                    CheckerboardTool.InputImage = new CogImage8Grey(bmpTest);
                                    CheckerboardTool.Run();
                                    cogDS.Image = CheckerboardTool.OutputImage;
                                }
                                else
                                    cogDS.Image = new CogImage8Grey(bmpTest);
                            }
                            ImgX = cogDS.Image.Width;
                            ImgY = cogDS.Image.Height;
                        }
                        Thread.Sleep(1);

                    } while (numReady <= 0);

                    cogDS.AutoFit = true;
                }
                else
                {
                    ; // Image 파일로 Captrue 할 경우임.
                }

                //KSJ 20170614 ImageSave Delete
                // 20200908 cjm Calibration 진행시 이미지를 저장하기 위해서 변경
                //if (bSave)
                //{
                //    if (Name != null)
                //        SaveImg((CogImage8Grey)cogDS.Image, false, Name);
                //    else
                //        SaveImg((CogImage8Grey)cogDS.Image, false, fail);
                //}

                Grabtimewatch.Stop();

                return true;
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("Capture" + "," + EX.GetType().Name + "," + EX.Message);
                ImgDisplay(errorImgPath);
                return false;
            }
            //}
        }

        private void Operator_Complete1(object sender, CogCompleteEventArgs e)
        {
            throw new NotImplementedException();
        }

        public bool Grab(ref CogImage8Grey cogGrabImage)
        {
            try
            {
                Live(false);

                int acqTicket, completeTicket, triggerNumber, numPending, numReady;
                bool busy;

                acqTicket = cogAcqFifoTool.Operator.StartAcquire();

                do
                {
                    cogAcqFifoTool.Operator.GetFifoState(out numPending, out numReady, out busy);

                    if (numReady > 0)
                    {
                        Bitmap bmpTest = null;
                        bmpTest = cogAcqFifoTool.Operator.CompleteAcquire(acqTicket, out completeTicket, out triggerNumber).ToBitmap();
                        if (CFG.nReverseMode == CONST.eImageReverse.None)    // None
                        {

                        }
                        else if (CFG.nReverseMode == CONST.eImageReverse.XReverse)    // X Reverse
                        {
                            bmpTest.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        }
                        else if (CFG.nReverseMode == CONST.eImageReverse.YReverse)    // Y Reverse
                        {
                            bmpTest.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        }
                        else if (CFG.nReverseMode == CONST.eImageReverse.AllReverse)    // All Reverse
                        {
                            bmpTest.RotateFlip(RotateFlipType.RotateNoneFlipXY);
                        }
                        else if (CFG.nReverseMode == CONST.eImageReverse.Reverse90)    // 90 Reverse
                        {
                            bmpTest.RotateFlip(RotateFlipType.Rotate90FlipX);
                        }
                        else if (CFG.nReverseMode == CONST.eImageReverse.Reverse270)    // 270 Reverse
                        {
                            bmpTest.RotateFlip(RotateFlipType.Rotate270FlipX);
                        }
                        else if (CFG.nReverseMode == CONST.eImageReverse.Reverse90XY)    // 90 Reverse
                        {
                            bmpTest.RotateFlip(RotateFlipType.Rotate90FlipXY);
                        }
                        else if (CFG.nReverseMode == CONST.eImageReverse.Reverse270XY)    // 270 Reverse
                        {
                            bmpTest.RotateFlip(RotateFlipType.Rotate270FlipXY);
                        }

                        if (bmpTest != null)
                        {
                            if (CheckerboardTool.Calibration.Calibrated && bcali)
                            {
                                CheckerboardTool.InputImage = new CogImage8Grey(bmpTest);
                                CheckerboardTool.Run();
                                cogDS.Image = CheckerboardTool.OutputImage;
                            }
                            else
                                cogDS.Image = new CogImage8Grey(bmpTest);
                        }
                        cogGrabImage = (CogImage8Grey)cogDS.Image;
                    }
                    //if (numReady > 0)
                    //    cogDS.Image = cogAcqFifoTool.Operator.CompleteAcquire(acqTicket, out completeTicket, out triggerNumber);

                    Thread.Sleep(1);

                } while (numReady <= 0);

                cogDS.AutoFit = true;

                return true;
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("Capture" + "," + EX.GetType().Name + "," + EX.Message);
                return false;
            }
            //}
        }
        public void PaintPoint(CogPointMarker p, string sgroupName = "calpoint")
        {
            if (p.X != 0 && p.Y != 0)
                cogDS.StaticGraphics.Add(p, sgroupName);
        }

        private string SaveImg(CogImage8Grey cTempImage, bool bDisplaySave = false, bool fail = false, bool bOverlay = false, string strNG = null)
        {
            string sFileName = "";
            try
            {
                //CogImageFileJPEG m_VisionCogImageFileJpeg = new CogImageFileJPEG();
                string lcdate = DateTime.Now.ToString("yyyy-MM-dd");
                DateTime lcDT = DateTime.Now;
                string lcHour = lcDT.Hour.ToString();

                // lyw. log 저장 경로 수정. Images 폴더 내부에 저장되도록. 170109.
                //string sDirectoryName = CONST.cVisionImgPath + "/" + CFG.Name + "/" + DateTime.Now.ToString("yyyy-MM-dd-HH");
                string sDirectoryBase = Path.Combine(CONST.cImagePath, CFG.Name);
                //pcy190529
                //string sDirectoryName = Path.Combine(sDirectoryBase, DateTime.Now.ToString("MM-dd-HH"));
                string sDirectoryName = Path.Combine(sDirectoryBase, DateTime.Now.ToString("yyyy-MM-dd"));

                if (fail)
                    sDirectoryName = Path.Combine(sDirectoryName, "FAIL");


                sFileName = sDirectoryName + "/" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");//JJ , 2017-05-20 : Image Save , 저장하고자 하는 포맷으로 확장자명 결정
                //sFileName = sDirectoryName + "/" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".jpg";

                if (!Directory.Exists(sDirectoryName))
                    Directory.CreateDirectory(sDirectoryName);

                if (bDisplaySave)
                {
                    //JJ , 2017-05-20 : Image Save ----- start
                    long lQuality = 90L;
                    new SaveImage()
                    {
                        format = System.Drawing.Imaging.ImageFormat.Jpeg,
                        Quality = lQuality,
                    }.Save(cTempImage.ToBitmap(), sFileName);
                    //Bitmap bitmap = new Bitmap(cogDS.CreateContentBitmap(Cognex.VisionPro.Display.CogDisplayContentBitmapConstants.Display));
                    //bitmap.Save(sFileName, ImageFormat.Jpeg);
                    //JJ , 2017-05-20 : Image Save ----- end

                }
                else
                {
                    //JJ , 2017-05-20 : Image Save ----- start
                    new SaveImage()
                    {
                        format = System.Drawing.Imaging.ImageFormat.Bmp
                    }.Save(cTempImage.ToBitmap(), sFileName);
                    //m_VisionCogImageFileJpeg.Open(sFileName, CogImageFileModeConstants.Write);
                    //m_VisionCogImageFileJpeg.Append(cTempImage);
                    //m_VisionCogImageFileJpeg.Close();
                    //JJ , 2017-05-20 : Image Save ----- end
                }

                //이미지 삭제 추가 필요함.
                // lyw. 170110. 이미지 삭제 추가. // 30일 지난 data는 삭제하도록.
                DeleteExpireVisionImage(sDirectoryBase, 24 * CFG.ImgAutoDelDay);

            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("SaveImg" + "," + EX.GetType().Name + "," + EX.Message);
            }
            return sFileName;
        }
        // 20200909 cjm Calibration 이미지 저장하는 곳 Path 수정함
        // 20201003 cjm Calibartion 진행 시 이미지 저장하는 주소 수정
        private string SaveImg(CogImage8Grey cTempImage, bool bDisplaySave = false, string Name = null) //20200909 cjm 이미지 저장
        {
            string sFileName = "";
            try
            {
                string CalImagePath = Path.Combine(CONST.Folder2, "EQData\\Calibration");

                string sDirectoryBase = Path.Combine(CalImagePath, Name.Split('_')[0]); // D:\\EQData\\Calibration\\CAL1
                sFileName = Path.Combine(sDirectoryBase, Name.Split('_')[1] + ".jpg"); //20200909 cjm 이미지 저장

                if (!Directory.Exists(sDirectoryBase))
                    Directory.CreateDirectory(sDirectoryBase); //20200909 cjm D:\\EQData\Image\CFG.Name\Cam1\Calibration\Calibration_5.bmp

                if (bDisplaySave)
                {
                    //JJ , 2017-05-20 : Image Save ----- start
                    long lQuality = 90L;
                    new SaveImage()
                    {
                        format = System.Drawing.Imaging.ImageFormat.Jpeg,
                        Quality = lQuality,
                    }.Save(cTempImage.ToBitmap(), sFileName);
                    //Bitmap bitmap = new Bitmap(cogDS.CreateContentBitmap(Cognex.VisionPro.Display.CogDisplayContentBitmapConstants.Display));
                    //bitmap.Save(sFileName, ImageFormat.Jpeg);
                    //JJ , 2017-05-20 : Image Save ----- end

                }
                else
                {
                    //JJ , 2017-05-20 : Image Save ----- start
                    //new SaveImage()
                    //{
                    //    format = System.Drawing.Imaging.ImageFormat.Bmp
                    //}.Save(cTempImage.ToBitmap(), sFileName);

                    //m_VisionCogImageFileJpeg.Open(sFileName, CogImageFileModeConstants.Write);
                    //m_VisionCogImageFileJpeg.Append(cTempImage);
                    //m_VisionCogImageFileJpeg.Close();
                    //JJ , 2017-05-20 : Image Save ----- end
                    //pcy이미지저장변경 테스트필요
                    Task.Run(() =>
                    {
                        CogImageFile imageFile = new CogImageFile();
                        imageFile.Open(sFileName, CogImageFileModeConstants.Write);
                        imageFile.Append(cTempImage);
                        imageFile.Close();
                    });
                }

                //이미지 삭제 추가 필요함.
                // lyw. 170110. 이미지 삭제 추가. // 30일 지난 data는 삭제하도록.
                DeleteExpireVisionImage(sDirectoryBase, 24 * CFG.ImgAutoDelDay);

            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("SaveImg" + "," + EX.GetType().Name + "," + EX.Message);
            }
            return sFileName;
        }
        // lyw420. vision image 파일 삭제 부분. 170110.
        public void DeleteExpireVisionImage(string path, int hour, string deleteDirectoryBase = "")
        {
            //TimeSpan diff = DateTime.Now - this.lastDeleteTime;
            // 24 시간에 한번씩 삭제 시도.
            //if (diff.TotalHours > 1)
            //{
            if (CONST.m_bSystemLog)
                cLog.Save(LogKind.System, "DeleteExpireVisionImage", false);

            fileRemover.externalDeleteUse = false;

            // vision image 폴더의 날짜 지난 폴더를 삭제.
            fileRemover.DirectoryRemove(path, hour, true, false, deleteDirectoryBase);

            //    this.lastDeleteTime = DateTime.Now;
            //}

        }

        public void ImgDisplay(string sPath)
        {
            try
            {
                cogDS.InteractiveGraphics.Clear();
                Live(false);
                if (sPath != null)
                {
                    //lkw 방향에 맞게 회전 추가 
                    Bitmap bmpTest = (Bitmap)Image.FromFile(sPath);
                    //if (CFG.nReverseMode == CONST.eImageReverse.None)    // None
                    //{
                    //}
                    //else if (CFG.nReverseMode == CONST.eImageReverse.XReverse)    // X Reverse
                    //{
                    //    //    bmpTest.RotateFlip(RotateFlipType.RotateNoneFlipX);                        
                    //}

                    //else if (CFG.nReverseMode == CONST.eImageReverse.YReverse)    // Y Reverse
                    //{
                    //    //    bmpTest.RotateFlip(RotateFlipType.RotateNoneFlipY);                  
                    //}
                    //else if (CFG.nReverseMode == CONST.eImageReverse.AllReverse)    // All Reverse
                    //{
                    //    //    bmpTest.RotateFlip(RotateFlipType.RotateNoneFlipXY);                        
                    //}
                    ////lkw 20170809 Reverse 항목 추가
                    //else if (CFG.nReverseMode == CONST.eImageReverse.Reverse90)    // 90 Reverse
                    //{
                    //    //    bmpTest.RotateFlip(RotateFlipType.Rotate90FlipX);                        
                    //}
                    //else if (CFG.nReverseMode == CONST.eImageReverse.Reverse270)    // 270 Reverse
                    //{
                    //    //    bmpTest.RotateFlip(RotateFlipType.Rotate270FlipX);                        
                    //}

                    cogDS.Image = new CogImage8Grey(bmpTest);
                    cogDS.AutoFit = true;
                }
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("ImgDisplay" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }


        public bool FindLine(ref CogLine cLine, CogCaliperScorerPosition cogposition, CogFindLineTool cogFindLine, string FilePath, double MarkX = 0, double MarkY = 0, int BDPreCnt = 0, bool BDPreSCFDIstInsp = false, bool ReSearch = false)
        {
            CogLineSegment segInspTest = new CogLineSegment();
            string[] FileRead;

            try
            {
                if (!File.Exists(FilePath))
                {
                    return false;
                }
                else
                {
                    FileRead = File.ReadAllLines(FilePath, Encoding.Default);

                    cogFindLine = new CogFindLineTool();

                    cogFindLine.RunParams.NumCalipers = int.Parse(FileRead[0]);
                    cogFindLine.RunParams.CaliperSearchLength = double.Parse(FileRead[1]);
                    cogFindLine.RunParams.CaliperProjectionLength = double.Parse(FileRead[2]);
                    if (FileRead[3] == "DarkToLight")
                        cogFindLine.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DarkToLight;
                    else if (FileRead[3] == "LightToDark")
                        cogFindLine.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.LightToDark;
                    else if (FileRead[3] == "DontCare")
                        cogFindLine.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DontCare;

                    cogFindLine.RunParams.CaliperSearchDirection = double.Parse(FileRead[5]); // *csvision.dRadian;
                    cogFindLine.RunParams.CaliperRunParams.ContrastThreshold = double.Parse(FileRead[6]);
                    cogFindLine.RunParams.NumToIgnore = int.Parse(FileRead[7]);
                    cogFindLine.RunParams.ExpectedLineSegment.StartX = double.Parse(FileRead[8]);
                    cogFindLine.RunParams.ExpectedLineSegment.StartY = double.Parse(FileRead[9]);
                    cogFindLine.RunParams.ExpectedLineSegment.EndX = double.Parse(FileRead[10]);
                    cogFindLine.RunParams.ExpectedLineSegment.EndY = double.Parse(FileRead[11]);
                }

                //추가
                if (FileRead[4] == "True")
                {
                    CogCaliperScorerPosition cog = new CogCaliperScorerPosition();
                    cog.Enabled = true;
                    cogFindLine.RunParams.CaliperRunParams.SingleEdgeScorers.Clear();
                    cogFindLine.RunParams.CaliperRunParams.SingleEdgeScorers.Add(cog);
                }
                else if (FileRead[4] == "False")
                {
                    CogCaliperScorerContrast cog = new CogCaliperScorerContrast();
                    cog.Enabled = true;
                    cogFindLine.RunParams.CaliperRunParams.SingleEdgeScorers.Clear();
                    cogFindLine.RunParams.CaliperRunParams.SingleEdgeScorers.Add(cog);
                }

                if (BDPreSCFDIstInsp)
                {
                    cogFindLine.RunParams.ExpectedLineSegment.StartX = 0;
                    cogFindLine.RunParams.ExpectedLineSegment.StartY = 0;
                    cogFindLine.RunParams.ExpectedLineSegment.EndX = 0;
                    cogFindLine.RunParams.ExpectedLineSegment.EndY = 0;

                    if (BDPreCnt == 0)
                    {
                        cogFindLine.RunParams.ExpectedLineSegment.StartX = MarkX + (Menu.frmSetting.revData.mBendingPre.BDPreSCFSearchLineOffsetX1 / CFG.Resolution);
                        cogFindLine.RunParams.ExpectedLineSegment.StartY = MarkY + (Menu.frmSetting.revData.mBendingPre.BDPreSCFSearchLineOffsetY1 / CFG.Resolution);
                        cogFindLine.RunParams.ExpectedLineSegment.EndX = MarkX + (Menu.frmSetting.revData.mBendingPre.BDPreSCFSearchLineOffsetX2 / CFG.Resolution);
                        cogFindLine.RunParams.ExpectedLineSegment.EndY = MarkY + (Menu.frmSetting.revData.mBendingPre.BDPreSCFSearchLineOffsetY2 / CFG.Resolution);
                    }
                    else
                    {
                        cogFindLine.RunParams.ExpectedLineSegment.StartY = MarkY + (Menu.frmSetting.revData.mBendingPre.BDPreSCFSearchLineOffsetY1 / CFG.Resolution);
                        cogFindLine.RunParams.ExpectedLineSegment.StartX = MarkX + (-1) * (Menu.frmSetting.revData.mBendingPre.BDPreSCFSearchLineOffsetX2 / CFG.Resolution);
                        cogFindLine.RunParams.ExpectedLineSegment.EndX = MarkX + (-1) * (Menu.frmSetting.revData.mBendingPre.BDPreSCFSearchLineOffsetX1 / CFG.Resolution);
                        cogFindLine.RunParams.ExpectedLineSegment.EndY = MarkY + (Menu.frmSetting.revData.mBendingPre.BDPreSCFSearchLineOffsetY2 / CFG.Resolution);
                    }
                }

                cogFindLine.CurrentRecordEnable = CogFindLineCurrentRecordConstants.All;

                cogFindLine.InputImage = (CogImage8Grey)cogDS.Image;

                cogFindLine.Run();
                segInspTest = cogFindLine.Results.GetLineSegment();
                cLine = cogFindLine.Results.GetLine();
                if (!ReSearch) cLine.Color = CogColorConstants.Magenta;
                else cLine.Color = CogColorConstants.Orange;

                cogDS.InteractiveGraphics.Add(cLine, "SCFEdgeLine", false);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public double gerMarkDist(cs2DAlign.ptXYT mark, double degree, cs2DAlign.ptXY distPoint)
        {
            CogLine cline = new CogLine();
            cline.SetXYRotation(mark.X, mark.Y, degree);
            double dPixel = 0;
            PointToLinePixel(cline, distPoint.X, distPoint.Y, ref dPixel);

            return dPixel;
        }

        public bool PointToLineDist(CogLine cLine1, double dX, double dY, ref double Dist)
        {
            try
            {

                CogDistancePointLineTool PointToLine = new CogDistancePointLineTool();

                PointToLine.InputImage = (CogImage8Grey)cogDS.Image;
                PointToLine.Line = cLine1;
                PointToLine.X = dX;
                PointToLine.Y = dY;

                PointToLine.Run();

                Dist = PointToLine.Distance;

                Dist = Dist * CFG.Resolution;

                return true;

            }
            catch
            {
                return false;
            }
        }



        public void PointToLinePixel(CogLine cLine1, double dX, double dY, ref double pixel)
        {
            try
            {

                CogDistancePointLineTool PointToLine = new CogDistancePointLineTool();

                PointToLine.InputImage = (CogImage8Grey)cogDS.Image;
                PointToLine.Line = cLine1;
                PointToLine.X = dX;
                PointToLine.Y = dY;

                PointToLine.Run();

                pixel = PointToLine.Distance;

                //Dist = Dist * CFG.Resolution;

                //return true;

            }
            catch
            {
                //return false;
            }
        }

        public bool GetPointfrom2Line(CogLine cLine1, CogLine cLine2, ref double dX, ref double dY, ref double dR, bool bNographics = false, CogImage8Grey img = null)
        {
            try
            {
                CogIntersectLineLineTool cTool1 = new CogIntersectLineLineTool();
                if (img == null) cTool1.InputImage = cogDS.Image;
                else cTool1.InputImage = img;
                cTool1.LineA = cLine1;
                cTool1.LineB = cLine2;
                cTool1.Run();
                if (CogToolResultConstants.Error != cTool1.RunStatus.Result
                    && CogToolResultConstants.Reject != cTool1.RunStatus.Result)
                {
                    dX = cTool1.X;
                    dY = cTool1.Y;
                    dR = cTool1.Angle;
                    if (!bNographics)
                    {
                        CogPointMarker marker = new CogPointMarker();
                        marker.X = cTool1.X;
                        marker.Y = cTool1.Y;
                        marker.SizeInScreenPixels = 70;

                        cogDS.InteractiveGraphics.Add(marker, "Point", false);
                    }
                    return true;
                }
                else
                {
                    return false;
                }

                //각도검사 삭제(필요하면 다른데서 하기)
                //pcy200720 각도검사해서 직각아니면 라인 잘못찾았다고 판단하려는거같음 근데 250은 뭐지..
                //double angle = cTool1.Angle / dRadian;
                //if (Math.Abs(angle) > 250) angle = Math.Abs(angle) - 180;
                //if (Math.Abs(Math.Abs(angle) - 90) > 3)
                //{
                //    //dX = 0;
                //    //dY = 0;
                //    return false;
                //}
            }
            catch
            {
                return false;
            }
        }
        public int ISExposure = -1; //초기값
        public bool setExposure(int iValue, out bool bexplow)
        {
            bexplow = false;
            //Exposure가 double형이지만 값넣을때는 int로 넣음(굳이 노출값을 소수점으로 맞출필요가 없어보임)
            try
            {
                if (iValue < 0) //변경하지 않음
                {
                    if (ISExposure == -1)
                    {
                        ISExposure = (int)cogAcqFifoTool.Operator.OwnedExposureParams.Exposure;
                    }
                    return false;
                }
                if (iValue > 255) iValue = 255;
                //현재와 다를때만 변경
                if (ISExposure != iValue)
                {
                    if (ISExposure > iValue) bexplow = true;
                    cogAcqFifoTool.Operator.OwnedExposureParams.Exposure = iValue;
                    ISExposure = iValue;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            { return false; }
        }

        //190624 cjm Contrast추가, 노출값으로 밝기가 해결 안되는 곳에 사용
        public double ISContrast = -1; //초기값
        public bool setContrast(double dValue, out bool bcontlow)
        {
            bcontlow = false;
            try
            {
                if (dValue < 0)
                {
                    if (ISContrast == -1)
                    {
                        ISContrast = cogAcqFifoTool.Operator.OwnedContrastParams.Contrast;
                    }
                    return false;

                }
                if (dValue > 1) dValue = 1;
                //현재와 다를때만 변경
                if (ISContrast != dValue)
                {
                    if (ISContrast > dValue) bcontlow = true;
                    cogAcqFifoTool.Operator.OwnedContrastParams.Contrast = dValue;
                    ISContrast = dValue;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            { return false; }

        }

        #region Calibration       



        public bool PointSearch(CogLine cLine1, CogLine cLine2, ref double dX, ref double dY)
        {
            CogIntersectLineLineTool cTool1 = new CogIntersectLineLineTool();
            cTool1.InputImage = cogDS.Image;
            cTool1.LineA = cLine1;
            cTool1.LineB = cLine2;
            cTool1.Run();

            dX = cTool1.X;
            dY = cTool1.Y;

            return true;
        }



        #endregion

        // 사용자 선택 좌표 입력 값 저장용.

        //bool positionReturnCheck = false;
        //   frmPositionPicker frmPositionPicker = null;

        public ePatternSearchProcessState PatternSearchStatus
        {
            get;
            set;
        }
        public int PatternSearchID
        {
            get;
            set;
        }

        // 현재 positon picker 상태인지를 체크하는.
        public bool usePositionPicker
        {
            get;
            set;
        }


        public void StartPositionPicker(int ikind)   // 0 : Mark, 1 : Ref
        {
            Capture(false, true, false, true);
            //frmPositionPicker = new Bending.frmPositionPicker();
            //frmPositionPicker = new frmPositionPicker(cogDS.Image, CFG, ikind);
        }

        public double[] rDX = new double[5];
        public double[] rDY = new double[5];
        public double[] rDR = new double[5];
        public CogLine[] rLine1 = new CogLine[5];
        //public CogLine[] rLine2 = new CogLine[5];
        //public CogLine[] rLine3 = new CogLine[5];
        //public sAlignResult[] resultFoam = new sAlignResult[5];
        //public sAlignResult[] resultRef = new sAlignResult[5];

        public patternSearchResult[] visionResult = new patternSearchResult[5]; // lyw.
        public lineSearchResult[] lineResult = new lineSearchResult[5];


        public bool InspectionSCFDist(rs2DAlign.cs2DAlign.ptXYT Mark, ref double SCFDist, int CamNo, int Camcnt, ref bool ResultY)
        {
            string LinePath = Path.Combine(CONST.cVisionPath, CFG.Name, CONST.stringRcp, CONST.RunRecipe.RecipeName, "Line");
            try
            {
                if (!File.Exists(LinePath))
                {
                    bool Result = false;
                    double SCFInspY1 = 0;
                    double SCFInspY2 = 0;
                    double SCFInspY1UpperSpec = 0;
                    double SCFInspY1LowSpec = 0;
                    double SCFInspY2UpperSpec = 0;
                    double SCFInspY2LowSpec = 0;
                    double Dist = 0;
                    string[] files = System.IO.Directory.GetFiles(LinePath, "*.txt");
                    string[] FileName;
                    int[] FileNO = new int[4];
                    CogLine[] cLine = new CogLine[4];
                    CogFindLineTool cogFindLine = new CogFindLineTool();
                    CogCaliperScorerPosition cogpos = new CogCaliperScorerPosition();

                    for (int i = 0; i < files.Length; i++)
                    {
                        FileName = files[i].Split('\\', '.');

                        if (FileName[7] == "WidthSCFInsp") FileNO[0] = i;
                    }

                    FindLine(ref cLine[0], cogpos, cogFindLine, files[FileNO[0]], Mark.X, Mark.Y, Camcnt, true);

                    if (PointToLineDist(cLine[0], Mark.X, Mark.Y, ref Dist))
                    {

                        SCFInspY1 = 0;//Convert.ToDouble(CONST.RunRecipe.Param[CONST.rcpSCF_ATTACH_INSPECT_Y1].Value);
                        SCFInspY2 = 0;//Convert.ToDouble(CONST.RunRecipe.Param[CONST.rcpSCF_ATTACH_INSPECT_Y2].Value);

                        SCFInspY1UpperSpec = SCFInspY1; //+ //Convert.ToDouble(CONST.RunRecipe.Param[CONST.rcpSCF_ATTACH_SPEC_Y].Value);
                        SCFInspY1LowSpec = SCFInspY1;// - Convert.ToDouble(CONST.RunRecipe.Param[CONST.rcpSCF_ATTACH_SPEC_Y].Value);

                        SCFInspY2UpperSpec = SCFInspY2;// + Convert.ToDouble(CONST.RunRecipe.Param[CONST.rcpSCF_ATTACH_SPEC_Y].Value);
                        SCFInspY2LowSpec = SCFInspY2;// - Convert.ToDouble(CONST.RunRecipe.Param[CONST.rcpSCF_ATTACH_SPEC_Y].Value);

                        //if (CamNo == Vision_No.vsBendPre1)
                        //{
                        //    SCFDist = Menu.frmSetting.revData.mBendingPre.BDPreSCFMarkToEdgeY1 - Dist;

                        //    if (Math.Abs(SCFDist) > SCFInspY1LowSpec &&
                        //        Math.Abs(SCFDist) < SCFInspY1UpperSpec)
                        //    {
                        //        Result = true;
                        //        ResultY = true;
                        //    }
                        //    else
                        //    {
                        //        Result = false;
                        //        ResultY = false;
                        //    }
                        //}
                        //else if (CamNo == Vision_No.vsBendPre2)
                        //{
                        //    SCFDist = Menu.frmSetting.revData.mBendingPre.BDPreSCFMarkToEdgeY2 - Dist;

                        //    if (Math.Abs(SCFDist) > SCFInspY2LowSpec &&
                        //        Math.Abs(SCFDist) < SCFInspY2UpperSpec)
                        //    {
                        //        Result = true;
                        //        ResultY = true;
                        //    }
                        //    else
                        //    {
                        //        Result = false;
                        //        ResultY = false;
                        //    }
                        //}
                    }
                    else
                    {
                        return false;
                    }

                    if (Result) return true;
                    else return false;
                }
                else

                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        //2018.08.20
        public bool HeightFindLine(ref rs2DAlign.cs2DAlign.ptXY LineMark, ref CogLine cogLine, bool HeightLine = false, bool WidhtLine = false, bool SubHeightLine = false, bool SubWidthLine = false, bool FPCBHeight = false, bool FPCBWidth = false)
        {
            //pcy190405 SubHeightLine, SubWidthLine 추가(블록게이지 검수용)
            string LinePath = Path.Combine(CONST.cVisionPath, CFG.Name, CONST.stringRcp, CONST.RunRecipe.RecipeName, "Line");
            try
            {
                if (Directory.Exists(LinePath))
                {
                    string[] files = System.IO.Directory.GetFiles(LinePath, "*.txt");
                    string[] FileName;
                    int FileNO = new int();
                    bool Result2 = false;

                    for (int i = 0; i < files.Length; i++)
                    {
                        FileName = files[i].Split('\\', '.');

                        if (HeightLine)
                        {
                            if (FileName[7] == "HeightPanel") FileNO = i;
                        }
                        else if (WidhtLine)
                        {
                            if (FileName[7] == "WidthPanel") FileNO = i;
                        }
                        else if (SubHeightLine)
                        {
                            if (FileName[7] == "SubHeightLine") FileNO = i;
                        }
                        else if (SubWidthLine)
                        {
                            if (FileName[7] == "SubWidthLine") FileNO = i;
                        }
                        else if (FPCBHeight)
                        {
                            if (FileName[7] == "HeightEdge") FileNO = i;
                        }
                        else if (FPCBWidth)
                        {
                            if (FileName[7] == "WidthEdge") FileNO = i;
                        }
                    }

                    CogFindLineTool cogFindLine = new CogFindLineTool();
                    CogCaliperScorerPosition cogpos = new CogCaliperScorerPosition();
                    CogLine cLine = new CogLine();
                    bool Result1 = false;

                    if (HeightLine || SubHeightLine || FPCBHeight)
                    {
                        Result1 = FindLine(ref HeightFineLine, cogpos, cogFindLine, files[FileNO]);
                        cogLine = HeightFineLine;
                    }
                    else if (WidhtLine || SubWidthLine || FPCBWidth)
                    {
                        FindLine(ref cLine, cogpos, cogFindLine, files[FileNO]);
                    }


                    if (HeightLine || SubHeightLine || FPCBHeight)
                    {
                        if (Result1) return true;
                        else return false;
                    }
                    else if (WidhtLine || SubWidthLine || FPCBWidth)
                    {
                        double dr = 0;
                        if (GetPointfrom2Line(HeightFineLine, cLine, ref LineMark.X, ref LineMark.Y, ref dr)) Result2 = true;
                        else Result2 = false;
                    }

                    cogLine = cLine;

                    if (Result1 || Result2) return true;
                    else return false;

                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool SCFInspLine(ref rs2DAlign.cs2DAlign.ptXY LineMark1, ref CogLine widthLine, ref CogLine heightLine, bool PanelFindLine = false, bool EdgeFindLine = false, bool LineFind = false)
        {
            string LinePath = Path.Combine(CONST.cVisionPath, CFG.Name, CONST.stringRcp, CONST.RunRecipe.RecipeName, "Line");
            try
            {
                //cogDS.InteractiveGraphics.Clear();

                if (Directory.Exists(LinePath))
                {
                    string[] files = System.IO.Directory.GetFiles(LinePath, "*.txt");
                    string[] FileName;
                    int[] FileNO = new int[2];

                    for (int i = 0; i < files.Length; i++)
                    {
                        FileName = files[i].Split('\\', '.');
                        if (EdgeFindLine) // Bending Align 시 Edge Line Search
                        {
                            if (FileName[7] == "WidthEdge") FileNO[0] = i;
                            if (FileName[7] == "HeightEdge") FileNO[1] = i;
                        }
                        else if (PanelFindLine) // Bending Align Initial 시 Window Line Search 
                        {
                            if (FileName[7] == "WidthPanel") FileNO[0] = i;
                            if (FileName[7] == "HeightPanel") FileNO[1] = i;
                        }
                        else if (LineFind)
                        {
                            if (FileName[7] == "WidthLine") FileNO[0] = i;
                            if (FileName[7] == "HeightLine") FileNO[1] = i;
                        }
                    }

                    CogLine[] cLine = new CogLine[files.Length];

                    CogFindLineTool cogFindLine = new CogFindLineTool();
                    CogCaliperScorerPosition cogpos = new CogCaliperScorerPosition();

                    FindLine(ref cLine[0], cogpos, cogFindLine, files[FileNO[0]]);
                    FindLine(ref cLine[1], cogpos, cogFindLine, files[FileNO[1]]);

                    widthLine = cLine[0];
                    heightLine = cLine[1];
                    double dr = 0;
                    if (GetPointfrom2Line(cLine[0], cLine[1], ref LineMark1.X, ref LineMark1.Y, ref dr)) return true;
                    else return false;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }



        public bool CreateSegment(double MarkX1, double MarkY1, double MarkX2, double MarkY2, ref double refTheta)//19.03.28 cjg 주석
        {
            try
            {
                CogCreateSegmentTool cogCreateSeg = new CogCreateSegmentTool();
                cogCreateSeg.InputImage = (CogImage8Grey)cogDS.Image;
                cogCreateSeg.Segment.StartX = MarkX1;
                cogCreateSeg.Segment.StartY = MarkY1;
                cogCreateSeg.Segment.EndX = MarkX2;
                cogCreateSeg.Segment.EndY = MarkY2;
                cogCreateSeg.Run();

                //double radian = Math.PI / 180.0;
                refTheta = cogCreateSeg.Segment.Rotation;// / radian;

                CogLineSegment cogLine1 = new CogLineSegment();
                cogLine1.StartX = MarkX1;
                cogLine1.StartY = MarkY1;
                cogLine1.EndX = MarkX2;
                cogLine1.EndY = MarkY2;
                cogLine1.Color = CogColorConstants.Blue;
                cogDS.InteractiveGraphics.Add(cogLine1.CreateLine(), "CreateSegmentLine", false);

                return true;
            }
            catch
            {
                return false;
            }

        }

        public bool WidthLineSearch(ref double refTheta)
        {
            string LinePath = Path.Combine(CONST.cVisionPath, CFG.Name, CONST.stringRcp, CONST.RunRecipe.RecipeName, "Line");
            try
            {
                cogDS.InteractiveGraphics.Clear();

                int[] SelectLines = new int[2];

                if (Directory.Exists(LinePath))
                {
                    string[] files = System.IO.Directory.GetFiles(LinePath, "*.txt");
                    string[] FileName;
                    int FileNO = new int();

                    for (int i = 0; i < files.Length; i++)
                    {
                        FileName = files[i].Split('\\', '.');
                        if (FileName[7] == "WidthLine") FileNO = i;
                    }

                    CogLine cLine = new CogLine();

                    CogFindLineTool cogFindLine = new CogFindLineTool();
                    CogCaliperScorerPosition cogpos = new CogCaliperScorerPosition();

                    FindLine(ref cLine, cogpos, cogFindLine, files[FileNO]);

                    refTheta = cLine.Rotation;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool SetLightExpCont(eLineKind Lkind1, bool bdelay = true, int iRetryexp = 0)
        {
            bool blight = false; //조명
            bool bexp = false; //노출
            bool bcont = false;
            //일단 조명은 하나로 고정을 가정하여 작성. 노출로 조절하자.
            blight = Menu.frmAutoMain.SetLight(CFG.Light1Comport, CFG.Light1CH, CFG.LineLight[(int)Lkind1], CFG.Camno);
            Menu.frmAutoMain.SetLight(CFG.Light1Comport, CFG.SubLight1CH, CFG.SubLight1Value, CFG.Camno);
            Menu.frmAutoMain.SetLight(CFG.Light5VComport, CFG.Light5VCH, CFG.Light5VValue, CFG.Camno, CONST.eLightType.Light5V);
            bexp = setExposure(CFG.LineExposure[(int)Lkind1] + iRetryexp, out bool bexplow);
            bcont = setContrast(CFG.LineContrast[(int)Lkind1], out bool bcontlow);
            if ((blight || bexp || bcont) && bdelay) //변경되면 딜레이
            {
                Thread.Sleep(CFG.LightDelay);
                return true;
            }
            return false;
            //if (blight || bexplow || bcontlow) //낮아질때만 딜레이
            //{
            //    Thread.Sleep(CFG.LightDelay);
            //}
        }
        public bool SetLightExpCont(ePatternKind Pkind, bool bdelay = true, int iRetryexp = 0)
        {
            //변경함을 확인
            bool blight = false;
            bool bexp = false;
            bool bcont = false;
            //일단 조명은 하나로 고정을 가정하여 작성. 노출로 조절하자.
            blight = Menu.frmAutoMain.SetLight(CFG.Light1Comport, CFG.Light1CH, CFG.Light[(int)Pkind], CFG.Camno);
            //Menu.frmAutoMain.SetLight(CFG.Light1Comport, CFG.SubLight1CH, CFG.SubLight1Value, CFG.Camno); //서브는 여기서 적어도 계속 고정임
            //Menu.frmAutoMain.SetLight(CFG.Light5VComport, CFG.Light5VCH, CFG.Light5VValue, CFG.Camno, CONST.eLightType.Light5V);
            bexp = setExposure(CFG.Exposure[(int)Pkind] + iRetryexp, out bool bexplow);
            bcont = setContrast(CFG.Contrast[(int)Pkind], out bool bcontlow);
            if ((blight || bexp || bcont) && bdelay) //변경되면 딜레이
            {
                Thread.Sleep(CFG.LightDelay);
                return true;
            }
            return false;
            //if (blight || bexplow || bcontlow) //낮아질때만 딜레이
            //{
            //    Thread.Sleep(CFG.LightDelay);
            //}
        }
        public bool SetLineLightExpCont(eLineKind Lkind, bool bdelay = true, int iRetryexp = 0)
        {
            //변경함을 확인
            bool blight = false;
            bool bexp = false;
            bool bcont = false;
            //일단 조명은 하나로 고정을 가정하여 작성. 노출로 조절하자.
            blight = Menu.frmAutoMain.SetLight(CFG.Light1Comport, CFG.Light1CH, CFG.LineLight[(int)Lkind], CFG.Camno);
            Menu.frmAutoMain.SetLight(CFG.Light1Comport, CFG.SubLight1CH, CFG.SubLight1Value, CFG.Camno); //서브는 여기서 적어도 계속 고정임
            Menu.frmAutoMain.SetLight(CFG.Light5VComport, CFG.Light5VCH, CFG.Light5VValue, CFG.Camno, CONST.eLightType.Light5V);
            bexp = setExposure(CFG.LineExposure[(int)Lkind] + iRetryexp, out bool bexplow);
            bcont = setContrast(CFG.LineContrast[(int)Lkind], out bool bcontlow);
            if ((blight || bexp || bcont) && bdelay) //변경되면 딜레이
            {
                Thread.Sleep(CFG.LightDelay);
                return true;
            }
            return false;
            //if (blight || bexplow || bcontlow) //낮아질때만 딜레이
            //{
            //    Thread.Sleep(CFG.LightDelay);
            //}
        }
        public CogLine widthLine = new CogLine();
        public CogLine heightLine = new CogLine();


        public void PatternSearch_Thread(
            PParam param,
            int id,
            BlockingCollection<patternSearchResult> blockingID,
            bool capture,
            ePatternKind Pkind,
            CogImage8Grey img = null
            )
        {
            //패턴을 찾아서 그 좌표를 반환하는 함수.
            BackgroundWorker wkr = new BackgroundWorker();
            Stopwatch timewatch = new Stopwatch();
            timewatch.Start();
            //rs2DAlign.cs2DAlign.ptXY Mark = new rs2DAlign.cs2DAlign.ptXY();
            PatternSearchStatus = ePatternSearchProcessState.Unknown;
            PatternSearchID = -1;
            bool result = false;
            searchDatas[(int)Pkind].BcogSearch = false;

            wkr.DoWork += delegate (object sender, DoWorkEventArgs e1)
            {
                PatternSearchStatus = ePatternSearchProcessState.Serching;
                if (CFG.RetryCnt == 0) CFG.RetryCnt = 1;
                for (int i = 0; i < CFG.RetryCnt; i++)
                {
                    bool bChange = SetLightExpCont(Pkind, true, CFG.RecaptureExposure * i);
                    if (bChange || capture)
                    {
                        //if (param.useGrapDelay2) Thread.Sleep(CFG.GrabDelay2);
                        //else Thread.Sleep(CFG.GrabDelay);


                        Capture(false, true, false, true);

                    }
                    result = PatternSearchEnum(ref rDX[id], ref rDY[id], ref rDR[id], Pkind, param.LineCreate, param.bNographics, img);
                    if (result)
                    {
                        //pcy210107 DL OK용 데이터
                        searchDatas[(int)Pkind].BcogSearch = true;
                        searchDatas[(int)Pkind].PointX = rDX[id];
                        searchDatas[(int)Pkind].PointY = rDY[id];
                        searchDatas[(int)Pkind].ImgWidth = cogDS.Image.Width;
                        searchDatas[(int)Pkind].ImgHeight = cogDS.Image.Height;
                        //searchDatas[(int)Pkind].ImageMemory = cogDS.Image.CopyBase(CogImageCopyModeConstants.CopyPixels);
                        //이미지만 느려질까봐 여기서 안넣고 다끝나고 맨마지막에 넣음..
                        break;
                    }
                    else capture = true;
                }

                this.visionResult[id] = new patternSearchResult(result, rDX[id], rDY[id], rDR[id]);
            };

            wkr.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e2)
            //wkr.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PatternSearchComplete);
            {
                //타임워치랑 이미지저장 여기서 하는게 맞는데 문제없는지 확인필요
                timewatch.Stop();
                string str = timewatch.ElapsedMilliseconds.ToString();
                WriteCogLabel(str + "ms", font10Bold, CogColorConstants.Orange, 400, 100, false, "RecogTime");
                if (result)
                {
                    nSearchResult = 1;
                }
                else if (param.failimagenotsave)
                {
                    nSearchResult = 1;
                }
                else
                {
                    nSearchResult = 2;
                }

                bSearchFlag = true;

                PatternSearchID = id;
                PatternSearchStatus = ePatternSearchProcessState.Complete;

                patternSearchResult r1 = new patternSearchResult(
                    visionResult[id].result,
                    visionResult[id].dx,
                    visionResult[id].dy,
                    visionResult[id].dr);

                //blockingID.Add(visionResult[id]); // 종료를 보내줌.
                blockingID.Add(r1); // 종료를 보내줌.

                wkr.Dispose();

            };

            wkr.RunWorkerAsync();
        }
        public void LineSearch_Thread(
            int id,
            BlockingCollection<lineSearchResult> blockingID,
            bool capture,
            eLineKind Lkind1,
            bool bNographics = false,
            bool failimagenotsave = false
            )
        {
            //라인을 두개 찾아서 그 교점을 반환하는 함수.
            //만약 한카메라라서 세개를 찾아서 두점반환하는거면..
            BackgroundWorker wkr = new BackgroundWorker();

            PatternSearchStatus = ePatternSearchProcessState.Unknown;
            PatternSearchID = -1;
            bool result = false;

            wkr.DoWork += delegate (object sender, DoWorkEventArgs e1)
            {
                PatternSearchStatus = ePatternSearchProcessState.Serching;
                if (CFG.RetryCnt == 0) CFG.RetryCnt = 1;
                for (int i = 0; i < CFG.RetryCnt; i++)
                {
                    //일단 조명은 하나로 고정을 가정하여 작성. 노출로 조절하자.
                    bool bChange = SetLightExpCont(Lkind1, true, CFG.RecaptureExposure * i);
                    if (bChange || capture)
                    {
                        //Thread.Sleep(CFG.GrabDelay);
                        Capture(false, true, false, true);
                    }

                    result = LineSearchEnum(ref rLine1[id], Lkind1, bNographics);

                    if (result) break;
                    else capture = true;
                }

                this.lineResult[id] = new lineSearchResult(result, rLine1[id]);
            };

            wkr.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e2)
            //wkr.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PatternSearchComplete);
            {
                PatternSearchID = id;
                PatternSearchStatus = ePatternSearchProcessState.Complete;


                lineSearchResult r1 = new lineSearchResult(
                    lineResult[id].result,
                    rLine1[id]);

                blockingID.Add(r1); // 종료를 보내줌.

                wkr.Dispose();

            };

            wkr.RunWorkerAsync();
        }

        public bool LineSearchEnum(
            ref CogLine line,
            eLineKind kind,
            bool bNographics = false
            )
        {
            if (line == null) line = new CogLine();
            bool result = false;

            //라인도 패턴처럼 기본 3번시도(의미가 있는지는 확인필요)
            result = FindLine(ref line, kind);
            if (!result) result = FindLine(ref line, kind);
            if (!result) result = FindLine(ref line, kind);


            if (result && !bNographics)
            {
                line.Color = CogColorConstants.Magenta;
                cogDS.InteractiveGraphics.Add(line, "Line", false);
            }

            return result;
        }
        public bool PatternSearchEnum(
            ref double dX,
            ref double dY,
            ref double dR,
            ePatternKind Pkind,
            bool LineCreate = false,
            bool bNographics = false,
            CogImage8Grey img = null
            )
        {
            bool result = false;
            bool bSubMark = false; //2차마크는 내부에서만 사용하는 개념
                                   //Overay(false);
                                   //if (!Menu.frmRecipe.tmrCal.Enabled)
                                   //cogDS.InteractiveGraphics.Clear();


            if (CFG.PatternSearchTool == CONST.ePatternSearchTool.PMAlign)
            {
                result = PatternSearch_PMAlignEnum(ref dX, ref dY, ref dR, Pkind, bSubMark, bNographics, img);
                if (!result) result = PatternSearch_PMAlignEnum(ref dX, ref dY, ref dR, Pkind, bSubMark, bNographics, img);
                if (!result) result = PatternSearch_PMAlignEnum(ref dX, ref dY, ref dR, Pkind, bSubMark, bNographics, img);
                if (!result && !bSubMark) result = PatternSearch_PMAlignEnum(ref dX, ref dY, ref dR, Pkind, true, bNographics, img);
            }
            else
            {

                result = PatternSearch_SearchMaxEnum(ref dX, ref dY, ref dR, Pkind, bSubMark);
                if (!result) result = PatternSearch_SearchMaxEnum(ref dX, ref dY, ref dR, Pkind, bSubMark);
                if (!result) result = PatternSearch_SearchMaxEnum(ref dX, ref dY, ref dR, Pkind, bSubMark);
                if (!result && !bSubMark) result = PatternSearch_SearchMaxEnum(ref dX, ref dY, ref dR, Pkind, true);// KSJ 20170613
            }

            //표시를 점대신 라인으로 십자 표시
            if (result && LineCreate)
            {
                //19.03.28 cjg
                CogLine cogline1 = new CogLine();
                cogline1.SetXYRotation(dX, dY, dR);
                cogline1.Color = CogColorConstants.Orange;
                cogDS.InteractiveGraphics.Add(cogline1, Pkind.ToString(), false);

                CogLine cogline2 = new CogLine();
                cogline2.SetXYRotation(dX, dY, dR + CONST.rad90);
                cogline2.Color = CogColorConstants.Orange;
                cogDS.InteractiveGraphics.Add(cogline2, Pkind.ToString(), false);
            }

            return result;
        }

        public bool PatternSearch_SearchMaxEnum(ref double dX, ref double dY, ref double dR, ePatternKind PKind, bool bSubMark = false, bool bScale = false)
        {
            CogRectangle cogrectangle = new CogRectangle();
            CogSearchMaxTool cogSearchMax = new CogSearchMaxTool();
            cogSearchMax.InputImage = (CogImage8Grey)cogDS.Image;
            int nMark = (int)PKind;
            bool bReturn = true;
            try
            {
                try
                {
                    this.DeleteInteractiveGraphic(cogDS, PKind.ToString());
                }
                catch
                { }

                //20161010 ljh
                //string RunRecipe = ucSetting.DB.getRunRecipeName().Trim();
                string RunRecipe = CONST.RunRecipe.RecipeName.Trim();

                // lyw. Recipe 폴더명 추가. 170113.
                string sPatternDir = Path.Combine(CONST.cVisionPath, CFG.Name.Trim(), CONST.stringRcp, RunRecipe.Trim(), "SearchMax");

                sPatternDir = Path.Combine(sPatternDir, PKind.ToString());

                if (bSubMark)
                {
                    sPatternDir = Path.Combine(sPatternDir, "2ndMark");
                }

                string sPatternFile = "";

                if (!Directory.Exists(sPatternDir)) Directory.CreateDirectory(sPatternDir);

                string[] sFile = Directory.GetFiles(sPatternDir, "*.vpp");

                string[] Split;

                int iSelect = -1;
                int iPattern = -1;

                CogSearchMaxResults results = null;
                CogSearchMaxResult result = null;


                for (int i = 0; i < sFile.Length; i++)
                {
                    if (sFile[i].IndexOf(".vpp") > 0)  // vpp File 이 있으면 
                    {
                        // 파일명 의 "_" 나눔 문자 구분으로 여러가지 setting이 적용되는 방식임.!!!!!

                        sPatternFile = sFile[i];
                        Split = Path.GetFileName(sPatternFile).Split('_'); //sFile[i].Split('_');


                        if (SearchMaxPatterns.ContainsKey(sPatternFile))
                            cogSearchMax.Pattern = SearchMaxPatterns[sPatternFile];
                        else
                        {
                            SearchMaxPatterns[sPatternFile] = (CogSearchMaxPattern)CogSerializer.LoadObjectFromFile(sPatternFile);
                            cogSearchMax.Pattern = SearchMaxPatterns[sPatternFile];
                        }

                        cogSearchMax.RunParams.RunAlgorithm = CogSearchMaxRunAlgorithmConstants.HighAccuracy;

                        cogSearchMax.RunParams.AcceptThreshold = double.Parse(Split[2]); // double.Parse(Split[1]);

                        // 극성 무시 여부.
                        cogSearchMax.RunParams.IgnorePolarity = bool.Parse(Split[5]);

                        if (Split[3] != "0" && Split[4] != "0")
                        {
                            cogSearchMax.RunParams.ZoneAngle.Low = double.Parse(Split[3]) * dRadian;
                            cogSearchMax.RunParams.ZoneAngle.High = double.Parse(Split[4]) * dRadian;

                            cogSearchMax.RunParams.ZoneAngle.Configuration = CogSearchMaxZoneConstants.LowHigh;
                        }
                        else
                            cogSearchMax.RunParams.ZoneAngle.Configuration = CogSearchMaxZoneConstants.Nominal;


                        // 검색 영역 저장된 값 영역으로 설정하기.

                        if (Split.Length > 8)
                        {
                            cogrectangle.SetXYWidthHeight(double.Parse(Split[7]), double.Parse(Split[8]), double.Parse(Split[9]), double.Parse(Split[10]));

                            cogSearchMax.SearchRegion = cogrectangle; // 검색 영역 설정해주기.

                        }
                        else
                        {
                            cogSearchMax.SearchRegion = null;
                        }

                        cogSearchMax.Run();

                        double maxScore = 0;

                        if (cogSearchMax.Results != null)
                        {
                            for (int j = 0; j < cogSearchMax.Results.Count; j++)
                            {
                                if (CONST.m_bSystemLog)
                                    cLog.Save(LogKind.System, "Score=" + cogSearchMax.Results[j].Score.ToString() + " / Align=" + CFG.Name);

                                if (cogSearchMax.Results[j].Score > double.Parse(Split[2]) &&/* (cogPMAlign.Results[j].FineStage || bRef) &&*/ maxScore < cogSearchMax.Results[j].Score)
                                {
                                    maxScore = cogSearchMax.Results[j].Score;
                                    CONST.TextScore = (maxScore);

                                    sAppliedPattern = sPatternFile;
                                    dSearchScore[nMark] = maxScore;
                                    dLimitScore[nMark] = double.Parse(Split[2]);
                                    dPatternNo[nMark] = (i + 1);

                                    results = new CogSearchMaxResults(cogSearchMax.Results);
                                    result = new CogSearchMaxResult(cogSearchMax.Results[j]);

                                    iSelect = i;
                                    iPattern = j;
                                    //break;  // 생각해 봐야 함.
                                }
                                else if (maxScore == 0)
                                {
                                    dSearchScore[nMark] = maxScore; // 점수반영. ng 시에도.
                                    dLimitScore[nMark] = double.Parse(Split[2]);
                                    dPatternNo[nMark] = (i + 1);
                                }

                            }

                            // score 에 맞게 검색 되면 더 이상 pattern 을 적용 찾기를 하지 않는다. AllBest일 경우 시간은 걸리지만 모든 파일을 검토하여 최고점을 찾도록 함.
                            if (CFG.PatternSearchMode == CONST.ePatternSearchMode.LastBest)
                            {
                                if (iSelect >= 0)
                                    break;
                            }
                        } // if.
                    }
                }

                // pattern 찾은 경우에만..
                if (result != null)// && cogSearchMax.Results[0].Score > double.Parse(Split[2]))
                {
                    ICogGraphicInteractive cogGraphic = result.CreateResultGraphics(CogSearchMaxResultGraphicConstants.MatchRegion);
                    cogDS.InteractiveGraphics.Add(cogGraphic, PKind.ToString(), false);

                    dX = result.GetPose().TranslationX;
                    dY = result.GetPose().TranslationY;
                    dR = result.GetPose().Rotation / dRadian;

                    bReturn = true;
                }
                else
                {
                    if (CONST.m_bSystemLog)
                        cLog.Save(LogKind.System, " Fail / Align=" + CFG.Name);
                    //CogDSFontDisplay(true, dX, dY, 0);
                    bReturn = false;
                }

                // 다른 pattern 에 대해서도 검색하길 원할 경우.. retry등에서.. 해당 폴더에 여러개의 vpp pattern 이 정의된 경우에......
                // bsecond 는 ucRecipe calPattern1에서 호출, calPattern2에서 호출, this.patternSearch2에서 호출, ucRecipe 의 tmrCal_Tick 에서 호출.
                //if (bsecond)
                //{
                //    for (int i = 0; i < sFile.Length; i++)
                //    {
                //        if (i != iSelect)
                //        {
                //            if (sFile[i].IndexOf(".vpp") > 0)  // vpp File 이 있으면 
                //            {
                //                sPatternFile = sFile[i];
                //                Split = sFile[i].Split('_');

                //                cogSearchMax.Pattern = (CogSearchMaxPattern)CogSerializer.LoadObjectFromFile(sPatternFile);
                //                cogSearchMax.InputImage = (CogImage8Grey)cogDS.Image;

                //                cogSearchMax.RunParams.AcceptThreshold = double.Parse(Split[1]);
                //                //cogSearchMax.RunParams.ContrastThreshold = 2;
                //                cogSearchMax.RunParams.ZoneAngle.Low = -10 * dRadian;
                //                cogSearchMax.RunParams.ZoneAngle.High = 10 * dRadian;
                //                cogSearchMax.RunParams.ZoneScale.Low = 0.8;
                //                cogSearchMax.RunParams.ZoneScale.High = 1.2;
                //                cogSearchMax.RunParams.ZoneAngle.Configuration = CogSearchMaxZoneConstants.LowHigh;
                //                cogSearchMax.RunParams.ZoneScale.Configuration = CogSearchMaxZoneConstants.Nominal;

                //                cogSearchMax.Run();

                //                //if (cogPMAlign.Results.Count > 0)// && cogPMAlign.Results[0].Score > double.Parse(Split[2]))
                //                //{
                //                //    iSelect = i;
                //                //    break;  // 생각해 봐야 함.
                //                //}

                //                for (int j = 0; j < cogSearchMax.Results.Count; j++)
                //                {
                //                    if (cogSearchMax.Results[j].Score > double.Parse(Split[2]))
                //                    {
                //                        iSelect = i;
                //                        break;  // 생각해 봐야 함.
                //                    }
                //                }
                //                if (iSelect >= 0) break;
                //            }
                //        }
                //    }

                //    if (cogSearchMax.Results.Count > 0)// && cogPMAlign.Results[0].Score > double.Parse(Split[2]))
                //    {
                //        ICogGraphicInteractive[] iGraphic = new ICogGraphicInteractive[cogSearchMax.Results.Count];
                //        for (int i = 0; i < iGraphic.Length; i++)
                //        {
                //            iGraphic[i] = cogSearchMax.Results[0].CreateResultGraphics(CogSearchMaxResultGraphicConstants.BoundingBox);
                //            iGraphic[i].Color = CogColorConstants.Orange;
                //            cogDS.InteractiveGraphics.Add(iGraphic[i], "Pattern", false);
                //        }

                //        double tempX = cogSearchMax.Results[0].GetPose().TranslationX;
                //        double tempY = cogSearchMax.Results[0].GetPose().TranslationY;

                //        if (CFG.CamDirection == CONST.eCamDirection.deg0)
                //        {
                //            dX = tempX;
                //            dY = tempY;
                //        }
                //        else if (CFG.CamDirection == CONST.eCamDirection.deg90)
                //        {
                //            dX = tempY;
                //            dY = (CFG.FOVX / CFG.Resolution) - tempX;
                //        }
                //        else if (CFG.CamDirection == CONST.eCamDirection.degM90)
                //        {
                //            dX = (CFG.FOVY / CFG.Resolution) - tempY;
                //            dY = tempX;
                //        }
                //    }
                //    else
                //        bReturn = false;
                //}
                //}
            }
            catch
            {
                bReturn = false;
            }

            return bReturn;
        }

        public int FindInteractiveGraphic(CogDisplay display, string groupName)
        {
            int idx = cogDS.InteractiveGraphics.FindItem(groupName, CogDisplayZOrderConstants.Front);
            return idx;
        }

        public ICogGraphicInteractive GetInteractiveGraphic(CogDisplay display, string groupName)
        {
            int idx = cogDS.InteractiveGraphics.FindItem(groupName, CogDisplayZOrderConstants.Front);
            if (idx == -1) return null;
            else return cogDS.InteractiveGraphics[idx];



        }


        // Integractive graphic 삭제하기.
        public bool DeleteInteractiveGraphic(CogDisplay display, string groupName)
        {
            bool result = false;

            try
            {
                if (FindInteractiveGraphic(display, groupName) > -1)
                {
                    display.InteractiveGraphics.Remove(groupName);
                    result = true;
                }
            }
            catch
            {

            }

            return result;
        }

        public bool ClearInteractiveGraphic(CogDisplay display)
        {
            bool result = false;

            try
            {
                display.InteractiveGraphics.Clear();
            }
            catch
            {

            }

            return result;
        }

        #region Vision Align
        public struct sAlignResult
        {
            public double X1;
            public double Y1;
            public double Theta;

            public double UVWX1;        //UVRW Theta + XY 보정령 총합 X1
            public double UVWX2;        //UVRW Theta + XY 보정령 총합 X2
            public double UVWY1;        //UVRW Theta + XY 보정령 총합 Y1
            public double UVWY2;        //UVRW Theta + XY 보정령 총합 Y2

            public double armuvwX1;     //Arm UVRW Theta + XY 보정령 총합 X1
            public double armuvwX2;     //Arm UVRW Theta + XY 보정령 총합 X2
            public double armuvwY1;     //Arm UVRW Theta + XY 보정령 총합 Y1
            public double armuvwY2;     //Arm UVRW Theta + XY 보정령 총합 Y2

            public double PX1;          //Panel Robot 좌표X1
            public double PY1;          //Panel Robot 좌표Y1
            public double PX2;          //Panel Robot 좌표X2
            public double PY2;          //Panel Robot 좌표Y2

            public double XYUVWX1;      //XY만 움직이는 UVW 방향 X1
            public double XYUVWX2;      //XY만 움직이는 UVW 방향 X2
            public double XYUVWY1;      //XY만 움직이는 UVW 방향 Y1
            public double XYUVWY2;      //XY만 움직이는 UVW 방향 Y2
            public double ThetaUVWX1;   //Theta만 움직이는 UVW 방향 X1
            public double ThetaUVWX2;   //Theta만 움직이는 UVW 방향 X2
            public double ThetaUVWY1;   //Theta만 움직이는 UVW 방향 Y1
            public double ThetaUVWY2;   //Theta만 움직이는 UVW 방향 Y2
            public double PanelTheta;   //Panel의 Theta 량
            public double FPCBTheta;    //FPCB의 Theta 량

            public double dMarkX1;      //FPCB 마크 Pixel X1
            public double dMarkY1;      //FPCB 마크 Pixel Y1
            public double dMarkX2;      //FPCB 마크 Pixel X2
            public double dMarkY2;      //FPCB 마크 Pixel Y2
            public double dRefX1;       //Panel 마크 Pixel X1
            public double dRefY1;       //Panel 마크 Pixel Y1
            public double dRefX2;       //Panel 마크 Pixel X2
            public double dRefY2;       //Panel 마크 Pixel Y2   


            // hdy insert
            public Point2d ObjectRobotPos1;
            public Point2d ObjectRobotPos2;
            public Point2d TargetRobotPos1;
            public Point2d TargetRobotPos2;

            public double dPointX;
            public double dPointY;

            public Point2d ObjectCenter;
            public Point2d TargetCenter;

        }

        // 두 점사이의 거리 구함 : 1개 카메라 사용        
        public double GetLength(eCalPos pos, double dPxX1, double dPxY1, double dPxX2, double dPxY2)
        {
            rs2DAlign.cs2DAlign.ptXY p1 = new rs2DAlign.cs2DAlign.ptXY();
            rs2DAlign.cs2DAlign.ptXY p2 = new rs2DAlign.cs2DAlign.ptXY();
            p1.X = dPxX1;
            p1.Y = dPxY1;
            p2.X = dPxX2;
            p2.Y = dPxY2;
            double offset = 0;
            return Menu.rsAlign.getLength((int)pos, p1, p2, offset);
        }

        // 두 점 사이의 거리를 구함 : 2개의 카메라 사용        
        public double GetLength(eCalPos pos1, eCalPos pos2, double dX1, double dY1, double dX2, double dY2)
        {
            rs2DAlign.cs2DAlign.ptXY p1 = new rs2DAlign.cs2DAlign.ptXY();
            rs2DAlign.cs2DAlign.ptXY p2 = new rs2DAlign.cs2DAlign.ptXY();
            p1.X = dX1;
            p1.Y = dY1;
            p2.X = dX2;
            p2.Y = dY2;

            //rs2DAlign.cs2DAlign.ptXY offset = new rs2DAlign.cs2DAlign.ptXY();
            //offset.X = 0;
            //offset.Y = 0;
            double offset = 0;
            return Menu.rsAlign.getLength((int)pos1, (int)pos2, p1, p2, offset);
        }


        public csVision.sFindLineParam FindLineParamRead(csVision.sCFG cfg, ref CogCaliperScorerPosition cogPosition, string kind = "")
        {
            string[] FileRead;
            csVision.sFindLineParam FindLineParam = new csVision.sFindLineParam();
            //CogCaliperScorerPosition cogpos = new CogCaliperScorerPosition();
            //csh 20170622  string RunRecipe = ucSetting.DB.getRunRecipeName().Trim();
            string RunRecipe = CONST.RunRecipe.RecipeName;

            try
            {
                string lcFile = string.Empty;
                string path = string.Empty;
                string camName = cfg.Name.Trim();// cboCamName.SelectedItem.ToString().Trim();
                path = Path.Combine(CONST.cVisionPath, camName, CONST.stringRcp, RunRecipe, "Line");

                lcFile = Path.Combine(path, kind + ".txt"); //Path.Combine(CONST.cVisionImgPath, cfg.Name, CONST.stringRcp, RunRecipe.Trim(), "Pattern", kind + ".txt");

                if (!File.Exists(lcFile))
                {
                    FindLineParam.Kind = kind;
                    FindLineParam.NumCalipers = 20;
                    FindLineParam.CaliperSearchLength = 450;
                    FindLineParam.CaliperProjectionLength = 130;
                    FindLineParam.Edge0Polarity = CogCaliperPolarityConstants.DontCare;
                    cogPosition.Enabled = true;
                    FindLineParam.CaliperSearchDirection = 90; // *csvision.dRadian;
                    FindLineParam.ContrastThreshold = 5;
                    FindLineParam.NumToIgnore = 10;
                    FindLineParam.Distance = 1000;
                    FindLineParam.StartX = 100;
                    FindLineParam.StartY = 100;
                    FindLineParam.EndX = 1100;
                    FindLineParam.EndY = 100;
                    FindLineParam.StartXDis = 0;
                    FindLineParam.StartYDis = 0;
                    FindLineParam.FilterHalfSizeInPixels = 2;
                }
                else
                {
                    FileRead = File.ReadAllLines(lcFile, Encoding.Default);

                    FindLineParam.Kind = kind;
                    FindLineParam.NumCalipers = int.Parse(FileRead[0]);
                    FindLineParam.CaliperSearchLength = double.Parse(FileRead[1]);
                    FindLineParam.CaliperProjectionLength = double.Parse(FileRead[2]);
                    if (FileRead[3] == "DarkToLight")
                        FindLineParam.Edge0Polarity = CogCaliperPolarityConstants.DarkToLight;
                    else if (FileRead[3] == "LightToDark")
                        FindLineParam.Edge0Polarity = CogCaliperPolarityConstants.LightToDark;
                    else if (FileRead[3] == "DontCare")
                        FindLineParam.Edge0Polarity = CogCaliperPolarityConstants.DontCare;
                    if (FileRead[4] == "True") cogPosition.Enabled = true;
                    else cogPosition.Enabled = false;
                    FindLineParam.CaliperSearchDirection = double.Parse(FileRead[5]); // *csvision.dRadian;
                    FindLineParam.ContrastThreshold = double.Parse(FileRead[6]);
                    FindLineParam.NumToIgnore = int.Parse(FileRead[7]);
                    FindLineParam.StartX = double.Parse(FileRead[8]);
                    FindLineParam.StartY = double.Parse(FileRead[9]);
                    FindLineParam.EndX = double.Parse(FileRead[10]);
                    FindLineParam.EndY = double.Parse(FileRead[11]);
                    FindLineParam.StartXDis = double.Parse(FileRead[12]);
                    FindLineParam.StartYDis = double.Parse(FileRead[13]);
                    FindLineParam.Distance = double.Parse(FileRead[14]);
                    FindLineParam.FilterHalfSizeInPixels = int.Parse(FileRead[15]);
                    FindLineParam.OffsetX = double.Parse(FileRead[16]);
                    FindLineParam.OffsetY = double.Parse(FileRead[17]);

                }

            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("FindLineParamRead" + "," + EX.GetType().Name + "," + EX.Message);
            }
            return FindLineParam;
        }

        public void MarkDisplay(double dx, double dy, CogColorConstants color = CogColorConstants.Magenta)
        {
            CogPointMarker cMaker1 = new CogPointMarker();
            cMaker1.X = dx;
            cMaker1.Y = dy;
            cMaker1.SizeInScreenPixels = 70;
            cMaker1.Color = color;
            cogDS.InteractiveGraphics.Add(cMaker1, "Point", false);
        }

        public bool MarkingInsp_Line(ref sAlignResult resultFoam, double dX, double dY, bool bManual, ref CogLine cWidthLine, ref CogLine cHeightLine, ref CogPointMarker marker, bool bLineL)
        {
            //this.DeleteInteractiveGraphic(cogDS, "Point");
            CogCaliperScorerPosition cogpos = new CogCaliperScorerPosition();
            sFindLineParam Wparam = new sFindLineParam();
            sFindLineParam Height = new sFindLineParam();

            //find line L
            if (bLineL)
            {
                Wparam = FindLineParamRead(CFG, ref cogpos, "PanelWidth1");
                Height = FindLineParamRead(CFG, ref cogpos, "PanelHeight1");
            }

            //find line R
            else
            {
                Wparam = FindLineParamRead(CFG, ref cogpos, "PanelWidth2");
                Height = FindLineParamRead(CFG, ref cogpos, "PanelHeight2");
            }
            //CogLine cWidthLine = new CogLine();
            //CogLine cHeight1Line = new CogLine();
            cs2DAlign.ptXY resolution = new cs2DAlign.ptXY();
            cs2DAlign.ptXY pixelCnt = new cs2DAlign.ptXY();
            Menu.rsAlign.getResolution((int)eCalPos.Laser1, ref resolution, ref pixelCnt);
            if (resolution.X == 0)
            {
                resolution.X = 0.0044;
                resolution.Y = 0.0044;
            }

            if (FindLine_Rev(resolution, ref cWidthLine, cogpos, Wparam, dX, dY, !bManual, 0) && FindLine_Rev(resolution, ref cHeightLine, cogpos, Height, dX, dY, !bManual, 1))
            //if (FindLine_Rev(ref cWidthLine, Wparam, 0, 0, !bManual) && FindLine_Rev(ref cHeight1Line, Height, 0, 0, !bManual))
            {
                //dR = cWidthLine.Rotation / dRadian;

                try
                {
                    CogIntersectLineLineTool cTool1 = new CogIntersectLineLineTool();
                    cTool1.InputImage = cogDS.Image;
                    cTool1.LineA = cWidthLine;
                    cTool1.LineB = cHeightLine;
                    cTool1.Run();

                    resultFoam.X1 = cTool1.X;
                    resultFoam.Y1 = cTool1.Y;

                    //CogPointMarker marker = new CogPointMarker();
                    marker.X = cTool1.X;
                    marker.Y = cTool1.Y;
                    marker.SizeInScreenPixels = 70;
                    marker.Color = CogColorConstants.Blue;
                    cogDS.InteractiveGraphics.Add(marker, "Point", false);

                    double angle = cTool1.Angle / dRadian;
                    if (Math.Abs(angle) > 250) angle = Math.Abs(angle) - 180;
                    if (Math.Abs(Math.Abs(angle) - 90) > 5) return false;  // 찾은 라인이 90도에서 5도 이상 벗어나면 잘못 찾은 것으로 인식함.

                    return true;
                }
                catch { }
            }
            return false;
        }

        public string GetNGType(int iType)
        {
            string strResult = string.Empty;
            switch (iType)
            {
                case 1:
                    strResult = "OK";
                    break;
                case 2:
                    strResult = "Pattern Fail";
                    break;
                case 3:
                    strResult = "L Check NG";
                    break;
                case 7:
                    strResult = "Limit Error";
                    break;
                case 11:
                    strResult = "Retry";
                    break;
                default:
                    break;
            }

            return strResult;
        }

        #endregion



        #region findline_thread 시에.. //pcy201008 이거 되는지 검증안된것같음 의심

        // CogDisplay cogDSFindLine;
        public void FindLine_Thread(ref CogLine cLine, csVision.sFindLineParam FindLineParam, BlockingCollection<bool> bc, double dX, double dY, bool bAuto = true)
        {
            CogFindLineTool cogFindLine = new CogFindLineTool();
            BackgroundWorker wkr = new BackgroundWorker();
            CogGraphicCollection myRegions;
            ICogRecord myRec;
            CogLineSegment myLine;
            //CogCircularArc myArc;
            CogLine cLine1 = cLine;
            bool result = false;

            wkr.DoWork += delegate (object sender, DoWorkEventArgs e1)
            {
                this.DeleteInteractiveGraphic(cogDS, "Pattern");
                this.DeleteInteractiveGraphic(cogDS, "FindLine");
                this.DeleteInteractiveGraphic(cogDS, "ShapeSegmentRun");
                this.DeleteInteractiveGraphic(cogDS, FindLineParam.Kind);
                this.DeleteInteractiveGraphic(cogDS, FindLineParam.Kind + "P");

                try
                {
                    cogFindLine.InputImage = (CogImage8Grey)cogDS.Image;
                    cogFindLine.RunParams.NumCalipers = FindLineParam.NumCalipers;
                    cogFindLine.RunParams.CaliperSearchLength = FindLineParam.CaliperSearchLength;
                    cogFindLine.RunParams.CaliperProjectionLength = FindLineParam.CaliperProjectionLength;
                    cogFindLine.RunParams.CaliperRunParams.Edge0Polarity = FindLineParam.Edge0Polarity;

                    if (FindLineParam.Distance == 0)
                        FindLineParam.Distance = 1000;

                    if (FindLineParam.Kind == "Width")
                    {
                        cogFindLine.RunParams.ExpectedLineSegment.StartX = dX - FindLineParam.StartXDis;
                        cogFindLine.RunParams.ExpectedLineSegment.StartY = dY - FindLineParam.StartYDis;
                        cogFindLine.RunParams.ExpectedLineSegment.EndX = cogFindLine.RunParams.ExpectedLineSegment.StartX + FindLineParam.Distance;
                        cogFindLine.RunParams.ExpectedLineSegment.EndY = dY - FindLineParam.StartYDis;
                    }
                    else if (FindLineParam.Kind == "Height1")
                    {
                        cogFindLine.RunParams.ExpectedLineSegment.StartX = dX - FindLineParam.StartXDis;
                        cogFindLine.RunParams.ExpectedLineSegment.StartY = dY + FindLineParam.StartYDis;
                        cogFindLine.RunParams.ExpectedLineSegment.EndX = dX - FindLineParam.StartXDis;
                        cogFindLine.RunParams.ExpectedLineSegment.EndY = cogFindLine.RunParams.ExpectedLineSegment.StartY - FindLineParam.Distance;
                    }
                    else if (FindLineParam.Kind == "Height2")
                    {
                        cogFindLine.RunParams.ExpectedLineSegment.StartX = dX + FindLineParam.StartXDis;
                        cogFindLine.RunParams.ExpectedLineSegment.StartY = dY - FindLineParam.StartYDis;
                        cogFindLine.RunParams.ExpectedLineSegment.EndX = dX + FindLineParam.StartXDis;
                        cogFindLine.RunParams.ExpectedLineSegment.EndY = cogFindLine.RunParams.ExpectedLineSegment.StartY + FindLineParam.Distance;
                    }

                    cogFindLine.RunParams.CaliperRunParams.ContrastThreshold = FindLineParam.ContrastThreshold;
                    cogFindLine.RunParams.NumToIgnore = FindLineParam.NumToIgnore;
                    //20161030 ljh
                    cogFindLine.RunParams.DecrementNumToIgnore = true;

                    CogCaliperScorerPosition cogPos = new CogCaliperScorerPosition();
                    cogFindLine.RunParams.CaliperRunParams.SingleEdgeScorers.Clear();
                    cogPos.Enabled = true;
                    cogFindLine.RunParams.CaliperRunParams.SingleEdgeScorers.Add(cogPos);


                    cogFindLine.CurrentRecordEnable = CogFindLineCurrentRecordConstants.All;
                    myRec = cogFindLine.CreateCurrentRecord();
                    cogFindLine.Run();

                    if (!bAuto)
                    {
                        try
                        {
                            myLine = (CogLineSegment)myRec.SubRecords["InputImage"].SubRecords["ExpectedShapeSegment"].Content;
                            myRegions = (CogGraphicCollection)myRec.SubRecords["InputImage"].SubRecords["CaliperRegions"].Content;
                            cogDS.InteractiveGraphics.Add(myLine, "ShapeSegmentRun", false);
                            foreach (ICogGraphic g in myRegions)
                                cogDS.InteractiveGraphics.Add((ICogGraphicInteractive)g, "FindLine", false);
                        }
                        catch
                        {
                        }
                    }

                    if (cogFindLine.Results != null)
                    {
                        if (cogFindLine.Results.Count > 0)
                        {
                            cLine1 = cogFindLine.Results.GetLine();

                            try
                            {
                                try
                                {
                                    for (int i = 0; i < cogFindLine.RunParams.NumCalipers; i++)
                                    {
                                        cogDS.InteractiveGraphics.Add(cogFindLine.Results[i].CreateResultGraphics(CogFindLineResultGraphicConstants.DataPoint), FindLineParam.Kind + "P", false);
                                    }
                                }
                                catch
                                {
                                }

                                cLine1.Color = CogColorConstants.Orange;
                                cogDS.InteractiveGraphics.Add(cLine1, FindLineParam.Kind, false);
                            }
                            catch
                            {
                            }

                            result = true;
                        }
                    } // if.

                }
                catch
                {
                }

                //return result;
            };

            wkr.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e2)
            {
                //cLog.Save(csLog.LogKind.System, "plcFoamAttach1AlignReq PatternSearch End");
                bc.Add(result);

                wkr.Dispose();
            };
            wkr.RunWorkerAsync();

        }

        #endregion

        // CogDisplay cogDSFindLine;
        public bool FindLine(ref CogLine cLine, csVision.sFindLineParam FindLineParam, double dX, double dY, bool bAuto = true)
        {
            CogFindLineTool cogFindLine = new CogFindLineTool();
            CogGraphicCollection myRegions;
            ICogRecord myRec;
            CogLineSegment myLine;
            //CogCircularArc myArc;
            this.DeleteInteractiveGraphic(cogDS, "Pattern");
            this.DeleteInteractiveGraphic(cogDS, "FindLine");
            this.DeleteInteractiveGraphic(cogDS, "ShapeSegmentRun");
            this.DeleteInteractiveGraphic(cogDS, FindLineParam.Kind);
            this.DeleteInteractiveGraphic(cogDS, FindLineParam.Kind + "P");

            try
            {
                //Capture(true);
                cogFindLine.InputImage = (CogImage8Grey)cogDS.Image;
                cogFindLine.RunParams.NumCalipers = FindLineParam.NumCalipers;
                cogFindLine.RunParams.CaliperSearchLength = FindLineParam.CaliperSearchLength;
                cogFindLine.RunParams.CaliperProjectionLength = FindLineParam.CaliperProjectionLength;
                cogFindLine.RunParams.CaliperRunParams.Edge0Polarity = FindLineParam.Edge0Polarity;

                if (FindLineParam.Distance == 0)
                    FindLineParam.Distance = 1000;

                if (FindLineParam.Kind == "Width")
                {
                    cogFindLine.RunParams.ExpectedLineSegment.StartX = dX - FindLineParam.StartXDis;
                    cogFindLine.RunParams.ExpectedLineSegment.StartY = dY - FindLineParam.StartYDis;
                    cogFindLine.RunParams.ExpectedLineSegment.EndX = cogFindLine.RunParams.ExpectedLineSegment.StartX + FindLineParam.Distance;
                    cogFindLine.RunParams.ExpectedLineSegment.EndY = dY - FindLineParam.StartYDis;
                }
                else if (FindLineParam.Kind == "Height1")
                {
                    cogFindLine.RunParams.ExpectedLineSegment.StartX = dX - FindLineParam.StartXDis;
                    cogFindLine.RunParams.ExpectedLineSegment.StartY = dY + FindLineParam.StartYDis;
                    cogFindLine.RunParams.ExpectedLineSegment.EndX = dX - FindLineParam.StartXDis;
                    cogFindLine.RunParams.ExpectedLineSegment.EndY = cogFindLine.RunParams.ExpectedLineSegment.StartY - FindLineParam.Distance;
                }
                else if (FindLineParam.Kind == "Height2")
                {
                    cogFindLine.RunParams.ExpectedLineSegment.StartX = dX + FindLineParam.StartXDis;
                    cogFindLine.RunParams.ExpectedLineSegment.StartY = dY - FindLineParam.StartYDis;
                    cogFindLine.RunParams.ExpectedLineSegment.EndX = dX + FindLineParam.StartXDis;
                    cogFindLine.RunParams.ExpectedLineSegment.EndY = cogFindLine.RunParams.ExpectedLineSegment.StartY + FindLineParam.Distance;
                }
                cogFindLine.RunParams.CaliperRunParams.ContrastThreshold = FindLineParam.ContrastThreshold;

                cogFindLine.RunParams.NumToIgnore = FindLineParam.NumToIgnore;
                //20161030 ljh
                cogFindLine.RunParams.DecrementNumToIgnore = true;

                CogCaliperScorerPosition cogPos = new CogCaliperScorerPosition();
                cogFindLine.RunParams.CaliperRunParams.SingleEdgeScorers.Clear();
                cogPos.Enabled = true;
                cogFindLine.RunParams.CaliperRunParams.SingleEdgeScorers.Add(cogPos);


                cogFindLine.CurrentRecordEnable = CogFindLineCurrentRecordConstants.All;
                myRec = cogFindLine.CreateCurrentRecord();
                cogFindLine.Run();

                if (!bAuto)
                {
                    try
                    {
                        myLine = (CogLineSegment)myRec.SubRecords["InputImage"].SubRecords["ExpectedShapeSegment"].Content;
                        myRegions = (CogGraphicCollection)myRec.SubRecords["InputImage"].SubRecords["CaliperRegions"].Content;
                        cogDS.InteractiveGraphics.Add(myLine, "ShapeSegmentRun", false);
                        foreach (ICogGraphic g in myRegions)
                            cogDS.InteractiveGraphics.Add((ICogGraphicInteractive)g, "FindLine", false);
                    }
                    catch
                    { }
                }


                if (cogFindLine != null)
                {
                    if (cogFindLine.Results.Count > 0)
                    {
                        cLine = cogFindLine.Results.GetLine();
                        try
                        {
                            if (!bAuto)
                            {
                                try
                                {

                                    for (int i = 0; i < cogFindLine.RunParams.NumCalipers; i++)
                                    {
                                        cogDS.InteractiveGraphics.Add(cogFindLine.Results[i].CreateResultGraphics(CogFindLineResultGraphicConstants.DataPoint), FindLineParam.Kind + "P", false);
                                    }
                                }
                                catch { }
                            }
                            cLine.Color = CogColorConstants.Orange;
                            cogDS.InteractiveGraphics.Add(cLine, FindLineParam.Kind, false);

                        }
                        catch { }
                        return true;
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        public bool FindLine_Rev(cs2DAlign.ptXY resolution, ref CogLine cLine, CogCaliperScorerPosition cogposition, csVision.sFindLineParam FindLineParam, double dX, double dY, bool bAuto = true, int iVision = 0)
        {
            CogFindLineTool cogFindLine = new CogFindLineTool();
            CogGraphicCollection myRegions;
            ICogRecord myRec;
            CogLineSegment myLine;
            //CogCircularArc myArc;
            //this.ClearInteractiveGraphic(cogDS);
            this.DeleteInteractiveGraphic(cogDS, "Pattern");
            this.DeleteInteractiveGraphic(cogDS, "FindLine");
            this.DeleteInteractiveGraphic(cogDS, "ShapeSegmentRun");
            this.DeleteInteractiveGraphic(cogDS, FindLineParam.Kind + iVision.ToString());
            this.DeleteInteractiveGraphic(cogDS, FindLineParam.Kind + "P");

            try
            {
                myLine = null;

                //Capture(true);
                cogFindLine.InputImage = (CogImage8Grey)cogDS.Image;

                cogFindLine.RunParams.NumCalipers = FindLineParam.NumCalipers;
                cogFindLine.RunParams.CaliperSearchLength = FindLineParam.CaliperSearchLength;
                cogFindLine.RunParams.CaliperProjectionLength = FindLineParam.CaliperProjectionLength;
                cogFindLine.RunParams.CaliperRunParams.Edge0Polarity = FindLineParam.Edge0Polarity;

                if (FindLineParam.Distance == 0)
                    FindLineParam.Distance = 1000;

                double Xdif = FindLineParam.EndX - FindLineParam.StartX;
                double Ydif = FindLineParam.EndY - FindLineParam.StartY;
                cogFindLine.RunParams.ExpectedLineSegment.StartX = dX + (FindLineParam.OffsetX / resolution.X);
                cogFindLine.RunParams.ExpectedLineSegment.StartY = dY + (FindLineParam.OffsetY / resolution.Y);
                cogFindLine.RunParams.ExpectedLineSegment.EndX = dX + (FindLineParam.OffsetX / resolution.X) + Xdif;
                cogFindLine.RunParams.ExpectedLineSegment.EndY = dY + (FindLineParam.OffsetY / resolution.Y) + Ydif;

                cogFindLine.RunParams.CaliperRunParams.ContrastThreshold = FindLineParam.ContrastThreshold;
                // hdy
                if (FindLineParam.FilterHalfSizeInPixels < 2) cogFindLine.RunParams.CaliperRunParams.FilterHalfSizeInPixels = 2;
                else cogFindLine.RunParams.CaliperRunParams.FilterHalfSizeInPixels = FindLineParam.FilterHalfSizeInPixels;
                cogFindLine.RunParams.CaliperSearchDirection = FindLineParam.CaliperSearchDirection;
                cogFindLine.RunParams.NumToIgnore = FindLineParam.NumToIgnore;
                //20161030 ljh
                //cogFindLine.RunParams.DecrementNumToIgnore = true;

                //CogCaliperScorerPosition cogPos = new CogCaliperScorerPosition();
                cogFindLine.RunParams.CaliperRunParams.SingleEdgeScorers.Clear();
                //cogposition.Enabled = true;
                cogFindLine.RunParams.CaliperRunParams.SingleEdgeScorers.Add(cogposition);

                cogFindLine.CurrentRecordEnable = CogFindLineCurrentRecordConstants.All;
                myRec = cogFindLine.CreateCurrentRecord();
                cogFindLine.Run();

                if (!bAuto)
                {
                    try
                    {
                        myLine = (CogLineSegment)myRec.SubRecords["InputImage"].SubRecords["ExpectedShapeSegment"].Content;
                        myLine.Interactive = true;

                        myRegions = (CogGraphicCollection)myRec.SubRecords["InputImage"].SubRecords["CaliperRegions"].Content;
                        //CogRectangleAffine Region = (CogRectangleAffine)myRec.SubRecords["InputImage"].SubRecords["CaliperRegions"].Content;

                        cogDS.InteractiveGraphics.Add(myLine, "ShapeSegmentRun", false);
                        foreach (ICogGraphic g in myRegions)
                        {
                            cogDS.InteractiveGraphics.Add((ICogGraphicInteractive)g, "FindLine", false);
                        }
                    }
                    catch
                    { }
                }

                if (cogFindLine != null)
                {
                    if (cogFindLine.Results.Count > 0)
                    {
                        cLine = cogFindLine.Results.GetLine();

                        try
                        {
                            if (!bAuto)
                            {
                                try
                                {
                                    for (int i = 0; i < cogFindLine.RunParams.NumCalipers; i++)
                                    {
                                        cogDS.InteractiveGraphics.Add(cogFindLine.Results[i].CreateResultGraphics(CogFindLineResultGraphicConstants.DataPoint), FindLineParam.Kind + "P", false);
                                    }
                                }
                                catch { }

                            }
                            cLine.Color = CogColorConstants.Orange;
                            cogDS.InteractiveGraphics.Add(cLine, FindLineParam.Kind + iVision.ToString(), false);

                        }
                        catch { }
                        return true;
                        //return myLine;
                    }
                }
            }
            catch
            {
            }
            return false;
            //return null;
        }


        //20161130 ljh
        Font font;
        public void CogDSFontDisplay(bool bInspectionResult, double X, double Y, double Score)
        {
            X = X * CFG.Resolution;
            Y = Y * CFG.Resolution;

            try
            {
                //2017 01 28
                //cogDS.InteractiveGraphics.Remove("Result");
                cogDS.InteractiveGraphics.Remove("Result1");
                cogDS.InteractiveGraphics.Remove("Result2");
            }
            catch { }

            try
            {
                CogGraphicLabel cogInspectionLabel = new CogGraphicLabel();
                CogGraphicLabel cogResultLabel = new CogGraphicLabel();

                if (bInspectionResult)
                {
                    cogInspectionLabel.Text = "NG";
                    cogResultLabel.Text = "Can't Find Pattern";
                }
                else
                {
                    cogInspectionLabel.Text = "OK";
                    cogResultLabel.Text = "X : " + X.ToString("0.000") + ", Y : " + Y.ToString("0.000") + ", Score : " + Score.ToString("0.000") + "             FOV X : " + CFG.FOVX.ToString("0.00") + ", FOV Y : " + CFG.FOVY.ToString("0.00");
                }
                //cogInspectionLabel.Text = InspectionResult;
                cogInspectionLabel.Alignment = CogGraphicLabelAlignmentConstants.TopLeft;
                font = new Font("Arial", 20, FontStyle.Bold);
                cogInspectionLabel.Font = font;
                cogInspectionLabel.SelectedSpaceName = "#";
                cogInspectionLabel.X = 10;
                cogInspectionLabel.Y = 10;
                //2017 01 28
                //cogDS.InteractiveGraphics.Add(cogInspectionLabel, "Result", false);
                cogDS.InteractiveGraphics.Add(cogInspectionLabel, "Result1", false);
                //cogInspectionLabel

                //cogResultLabel.Text = "11";
                cogResultLabel.Alignment = CogGraphicLabelAlignmentConstants.TopLeft;

                font = new Font("Arial", 15, FontStyle.Bold);
                cogResultLabel.Color = CogColorConstants.Red;
                cogResultLabel.Font = font;
                cogResultLabel.SelectedSpaceName = "#";
                cogResultLabel.X = 300;
                cogResultLabel.Y = 20;
                //cogResultLabel.SetXYText(10, cogDS.Height * 2 -200, cogResultLabel.Text);
                cogDS.InteractiveGraphics.Add(cogResultLabel, "Result2", false);
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("CogDSFontDisplay" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }

        public void CogDSInspDisplay(bool bInspectionResult)    //, double X, double Y, double Score
        {
            try
            {
                cogDS.InteractiveGraphics.Remove("Insp");
            }
            catch { }

            try
            {
                CogGraphicLabel cogInspectionLabel = new CogGraphicLabel();
                CogGraphicLabel cogResultLabel = new CogGraphicLabel();

                if (!bInspectionResult)
                {
                    cogInspectionLabel.Text = "NG";
                    //font = new Font("Arial", 15, FontStyle.Bold);
                    font = new Font("Arial", 20, FontStyle.Bold);
                    cogInspectionLabel.Color = CogColorConstants.Red;
                    //cogResultLabel.Text = "Can't Find Pattern";
                }
                else
                {
                    cogInspectionLabel.Text = "OK";
                    //font = new Font("Arial", 15, FontStyle.Bold);
                    font = new Font("Arial", 20, FontStyle.Bold);
                    cogInspectionLabel.Color = CogColorConstants.Green;
                    //cogResultLabel.Text = "X : " + X.ToString("0.000") + ", Y : " + Y.ToString("0.000") + ", Score : " + Score.ToString("0.000") + "             FOV X : " + CFG.FOVX.ToString("0.00") + ", FOV Y : " + CFG.FOVY.ToString("0.00");
                }
                //cogInspectionLabel.Text = InspectionResult;
                cogInspectionLabel.Alignment = CogGraphicLabelAlignmentConstants.TopLeft;
                //font = new Font("Arial", 20, FontStyle.Bold);
                cogInspectionLabel.Font = font;
                cogInspectionLabel.SelectedSpaceName = "#";
                cogInspectionLabel.X = 30;
                cogInspectionLabel.Y = 50;
                cogDS.InteractiveGraphics.Add(cogInspectionLabel, "Insp", false);
                //cogInspectionLabel

                ////cogResultLabel.Text = "11";
                //cogResultLabel.Alignment = CogGraphicLabelAlignmentConstants.TopLeft;

                //font = new Font("Arial", 15, FontStyle.Bold);
                //cogResultLabel.Color = CogColorConstants.Red;
                //cogResultLabel.Font = font;
                //cogResultLabel.SelectedSpaceName = "#";
                //cogResultLabel.X = 300;
                //cogResultLabel.Y = 20;
                ////cogResultLabel.SetXYText(10, cogDS.Height * 2 -200, cogResultLabel.Text);
                //cogDS.InteractiveGraphics.Add(cogResultLabel, "Result", false);
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("CogDSFontDisplay" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }

        public void Inspection(string sKind, double X1, double X2, double Y1, double Y2, double dDist)
        {
            try
            {
                cogDS.InteractiveGraphics.Remove(sKind);
            }
            catch { }

            CogLineSegment cogLine = new CogLineSegment();

            double tempX = X1;
            double tempY = Y1;

            double tempX2 = X2;
            double tempY2 = Y2;

            //if (CFG.CamDirection == CONST.eCamDirection.deg0)
            //{
            //    dX = tempX;
            //    dY = tempY;
            //}
            //else if (CFG.CamDirection == CONST.eCamDirection.deg90)
            //{
            //    dX = tempY;
            //    dY = (CFG.FOVX / CFG.Resolution) - tempX;
            //}
            //else if (CFG.CamDirection == CONST.eCamDirection.degM90)
            //{
            //    dX = (CFG.FOVY / CFG.Resolution) - tempY;
            //    dY = tempX;
            //}

            //if (CFG.CamDirection == CONST.eCamDirection.deg0)
            //{
            //    X1 = tempX;
            //    Y1 = tempY;
            //}
            //else if (CFG.CamDirection == CONST.eCamDirection.degM90)
            //{
            //    X1 = tempY;
            //    Y1 = (CFG.FOVY / CFG.Resolution) - tempX;

            //    X2 = tempY2;
            //    Y2 = (CFG.FOVY / CFG.Resolution) - tempX2;
            //}
            //else if (CFG.CamDirection == CONST.eCamDirection.deg90)
            //{
            //    X1 = (CFG.FOVX / CFG.Resolution) - tempY;
            //    Y1 = tempX;

            //    X2 = (CFG.FOVX / CFG.Resolution) - tempY2;
            //    Y2 = tempX2;
            //}

            X1 = tempX;
            Y1 = tempY;

            cogLine.StartX = X1;
            cogLine.StartY = Y1;
            cogLine.EndX = X2;
            cogLine.EndY = Y2;

            cogLine.Color = CogColorConstants.Yellow;
            cogLine.LineWidthInScreenPixels = 3;
            //if (sKind == "inspection3" || sKind == "inspection4")
            cogDS.InteractiveGraphics.Add(cogLine, sKind, false);

            /*cogLabel.SetXYText(cogLine.EndX + 40, cogLine.EndY + 50, dDist.ToString("0.000") + "(mm)");
            cogLabel.Color = CogColorConstants.Orange;
            Font font = new Font("Arial", 15, FontStyle.Bold);
            cogLabel.Font = font;
            cogDS.InteractiveGraphics.Add(cogLabel, sKind, false);*/

            WriteCogLabel(dDist.ToString("0.000") + "(mm)", font15Bold, CogColorConstants.Orange, cogLine.EndX + 40, cogLine.EndY + 50, false, sKind);
        }





        //20161206 ljh
        public bool FindCircle()
        {
            try
            {
                CogFindCircleTool cogFindCircle = new CogFindCircleTool();
                CogGraphicCollection myRegions;
                ICogRecord myRec;
                //CogLineSegment myLine;
                CogCircularArc myArc;
                cogFindCircle.InputImage = (CogImage8Grey)cogDS.Image;
                cogFindCircle.RunParams.NumCalipers = 30;
                cogFindCircle.RunParams.CaliperProjectionLength = 10;
                cogFindCircle.RunParams.CaliperSearchLength = 250;
                cogFindCircle.RunParams.CaliperSearchDirection = CogFindCircleSearchDirectionConstants.Inward;
                cogFindCircle.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DontCare;
                cogFindCircle.RunParams.CaliperRunParams.ContrastThreshold = 10;
                cogFindCircle.RunParams.DecrementNumToIgnore = true;
                cogFindCircle.RunParams.NumToIgnore = 10;
                // cogFindCircle.RunParams.RadiusConstraint

                cogFindCircle.CurrentRecordEnable = CogFindCircleCurrentRecordConstants.All;
                myRec = cogFindCircle.CreateCurrentRecord();
                myArc = (CogCircularArc)myRec.SubRecords["InputImage"].SubRecords["ExpectedShapeSegment"].Content;
                myRegions = (CogGraphicCollection)myRec.SubRecords["InputImage"].SubRecords["CaliperRegions"].Content;
                cogDS.InteractiveGraphics.Add(myArc, "ShapeSegmentRun", false);
                foreach (ICogGraphic g in myRegions)
                    cogDS.InteractiveGraphics.Add((ICogGraphicInteractive)g, "FindLine", false);

                cogFindCircle.Run();
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("FindCircle" + "," + EX.GetType().Name + "," + EX.Message);
            }
            return false;
        }


        public string G_RunRecipe;
        public string G_sPatternDir;
        public Dictionary<string, CogPMAlignPattern> pmAlignPatterns = new Dictionary<string, CogPMAlignPattern>();
        public Dictionary<string, CogSearchMaxPattern> SearchMaxPatterns = new Dictionary<string, CogSearchMaxPattern>();

        List<string> pDir = new List<string>();
        List<string> pRefDir = new List<string>();

        List<string> searchDir = new List<string>();

        public void GetPattern()
        {
            //csh 20170622  G_RunRecipe = ucSetting.DB.getRunRecipeName().Trim();
            G_RunRecipe = CONST.RunRecipe.RecipeName;

            string sPatternDir = Path.Combine(CONST.cVisionPath, CFG.Name.Trim(), CONST.stringRcp, G_RunRecipe.Trim(), "PMAlign");

            searchDir.Clear();
            foreach (var s in Enum.GetNames(typeof(ePatternKind)))
            {
                searchDir.Add(Path.Combine(sPatternDir, s));
            }

            foreach (KeyValuePair<string, CogPMAlignPattern> item in this.pmAlignPatterns)
            {
                CogPMAlignPattern pattern = item.Value as CogPMAlignPattern;
                pattern = null;
            }
            foreach (KeyValuePair<string, CogSearchMaxPattern> item in this.SearchMaxPatterns)
            {
                CogSearchMaxPattern pattern = item.Value as CogSearchMaxPattern;
                pattern = null;
            }

            // 저장된 pattern 지우기.
            pmAlignPatterns.Clear();
            SearchMaxPatterns.Clear();

            foreach (string dir in searchDir)
            {
                if (!Directory.Exists(dir))
                    continue;

                List<string> dirList = new List<string>();

                dirList.Add(dir);

                foreach (string str in Directory.GetDirectories(dir, "*.*", SearchOption.AllDirectories))
                {
                    dirList.Add(str);
                }


                // 하위 폴더 검색하여 패턴 사전 등록.
                foreach (string str in dirList) // Directory.GetDirectories(dir, "*.*", SearchOption.AllDirectories))
                {
                    string sPatternFile = "";

                    if (!Directory.Exists(str))
                    {
                    }
                    else
                    {
                        string[] sFile = Directory.GetFiles(str, "*.vpp");

                        // string[] Split;

                        for (int i = 0; i < sFile.Length; i++)
                        {
                            if (sFile[i].IndexOf(".vpp") > 0)  // vpp File 이 있으면 
                            {
                                sPatternFile = sFile[i];

                                try
                                {
                                    if (sPatternFile.ToUpper().IndexOf("SEARCHMAX") > 0)
                                    {
                                        //if (!SearchMaxPatterns.ContainsKey(sPatternFile))
                                        //    SearchMaxPatterns.Add(sPatternFile, (CogSearchMaxPattern)CogSerializer.LoadObjectFromFile(sPatternFile));
                                    }
                                    else
                                    {
                                        if (!pmAlignPatterns.ContainsKey(sPatternFile))
                                            pmAlignPatterns.Add(sPatternFile, (CogPMAlignPattern)CogSerializer.LoadObjectFromFile(sPatternFile));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message);
                                }

                            }
                        }
                    }
                }
            }

        }

        //20.12.17 lkw DL
        //DL Search용 ROI 저장 추가
        CogRectangle[] ROI_Rectangle = new CogRectangle[Enum.GetValues(typeof(ePatternKind)).Length];

        public CogRectangle getROI(ePatternKind PKind)
        {
            CogRectangle cogrectangle = new CogRectangle();

            try
            {
                string RunRecipe = CONST.RunRecipe.RecipeName.Trim(); // runrecipe 관련 수정.

                try
                {
                    this.DeleteInteractiveGraphic(cogDS, PKind.ToString());
                }
                catch
                { }

                string sPatternDir = Path.Combine(CONST.cVisionPath, CFG.Name.Trim(), CONST.stringRcp, RunRecipe.Trim(), "PMAlign", PKind.ToString());

                string sPatternFile = "";

                if (!Directory.Exists(sPatternDir)) Directory.CreateDirectory(sPatternDir);

                string[] sFile = Directory.GetFiles(sPatternDir, "*.vpp");

                //int iSelect = -1;
                //int iPattern = -1;

                for (int i = 0; i < sFile.Length; i++)
                {
                    string[] Split;

                    sPatternFile = sFile[i];
                    Split = Path.GetFileName(sPatternFile).Split('_');
                    if (Split.Length > 8)
                    {
                        cogrectangle.SetXYWidthHeight(double.Parse(Split[7]), double.Parse(Split[8]), double.Parse(Split[9]), double.Parse(Split[10]));
                        cogrectangle.GraphicDOFEnable = CogRectangleDOFConstants.All;
                        cogrectangle.Interactive = true;
                        cogDS.InteractiveGraphics.Add(cogrectangle, PKind.ToString(), false); // 검색 영역 표시 부분.
                    }

                }
            }
            catch { }
            return cogrectangle;
        }

        public bool PatternSearch_PMAlignEnum(ref double dX, ref double dY, ref double dR, ePatternKind PKind, bool bSubMark = false, bool bNographics = false, CogImage8Grey img = null)
        {
            CogRectangle cogrectangle = new CogRectangle();
            CogPMAlignTool cogPMAlign = new CogPMAlignTool(); //중복검색 전역변수가 문제

            if (img == null) cogPMAlign.InputImage = (CogImage8Grey)cogDS.Image; //이미지 넣는것 한번만 넣는거와 비교해서 시간차이 없음.
            else cogPMAlign.InputImage = img;
            //추후 PMAlign과 SearchMax도 통합해보자
            bool bReturn = false;
            int nMark = (int)PKind;
            if (bSubMark) nMark += 10; //패턴종류 10개이하라고 보고 10더함

            //패턴점수 초기화
            dSearchScore[nMark] = 0;
            //cogPMAlign.InputImage.SelectedSpaceName = ".";
            try
            {
                string RunRecipe = CONST.RunRecipe.RecipeName.Trim(); // runrecipe 관련 수정.

                try
                {
                    this.DeleteInteractiveGraphic(cogDS, PKind.ToString());
                }
                catch
                { }

                string sPatternDir = Path.Combine(CONST.cVisionPath, CFG.Name.Trim(), CONST.stringRcp, RunRecipe.Trim(), "PMAlign", PKind.ToString());

                if (bSubMark)
                {
                    sPatternDir = Path.Combine(sPatternDir, "2ndMark");
                }

                string sPatternFile = "";

                if (!Directory.Exists(sPatternDir)) Directory.CreateDirectory(sPatternDir);

                string[] sFile = Directory.GetFiles(sPatternDir, "*.vpp");

                int iSelect = -1;
                int iPattern = -1;

                // 결과 저장용.
                //CogPMAlignResults results = null;
                CogPMAlignResult result = null;


                for (int i = 0; i < sFile.Length; i++)
                {
                    string[] Split;

                    //csh 20170722  Pattern Search First 기능
                    if (sFile.Length > 1)
                    {
                        if (m_strFirstPattanSearchName[nMark] != "" && i == 0)// && PKind != ePatternKind.Cal && PKind != ePatternKind.Cal2)
                            sPatternFile = m_strFirstPattanSearchName[nMark];
                        else
                        {
                            if (m_strFirstPattanSearchName[nMark] != sFile[i])
                                sPatternFile = sFile[i];
                            else
                                sPatternFile = sFile[0];
                        }
                    }
                    else
                    {
                        sPatternFile = sFile[i];
                    }

                    // file name 에서만 split 하도록 수정. lyw. 170214.
                    Split = Path.GetFileName(sPatternFile).Split('_');
                    lock (pmAlignPatterns)
                    {
                        if (pmAlignPatterns.ContainsKey(sPatternFile))
                            cogPMAlign.Pattern = pmAlignPatterns[sPatternFile];
                        else
                        {
                            pmAlignPatterns[sPatternFile] = (CogPMAlignPattern)CogSerializer.LoadObjectFromFile(sPatternFile);
                            cogPMAlign.Pattern = pmAlignPatterns[sPatternFile];
                        }
                    }

                    cogPMAlign.RunParams.ContrastThreshold = double.Parse(Split[1]);

                    //20161030 ljh
                    cogPMAlign.Pattern.IgnorePolarity = bool.Parse(Split[5]);

                    if (Split[3] != "0" && Split[4] != "0")
                    {
                        cogPMAlign.RunParams.ZoneAngle.Configuration = CogPMAlignZoneConstants.LowHigh;
                        cogPMAlign.RunParams.ZoneAngle.Low = double.Parse(Split[3]) * dRadian;
                        cogPMAlign.RunParams.ZoneAngle.High = double.Parse(Split[4]) * dRadian;
                    }
                    else
                    {
                        cogPMAlign.RunParams.ZoneAngle.Configuration = CogPMAlignZoneConstants.Nominal;
                    }

                    // 까만색일 경우 적용할 경우 효과 있음... 잘 찾는다. 하지만 섞여져 있을 경우는 못찾음... 점수가 낮음.
                    //if (bFoam)
                    //    cogPMAlign.RunParams.ScoreUsingClutter = true;
                    //else
                    //    cogPMAlign.RunParams.ScoreUsingClutter = false;
                    cogPMAlign.RunParams.SaveMatchInfo = true;
                    cogPMAlign.RunParams.ScoreUsingClutter = false;

                    if (Split.Length > 8)
                    {
                        cogrectangle.SetXYWidthHeight(double.Parse(Split[7]), double.Parse(Split[8]), double.Parse(Split[9]), double.Parse(Split[10]));
                        cogPMAlign.SearchRegion = cogrectangle;

                        //pcy190421
                        if (!bNographics)
                        {
                            cogrectangle.GraphicDOFEnable = CogRectangleDOFConstants.All;
                            cogrectangle.Interactive = true;
                            cogDS.InteractiveGraphics.Add(cogrectangle, PKind.ToString(), false); // 검색 영역 표시 부분.
                        }

                        //20.12.17 lkw DL 
                        ROI_Rectangle[(int)PKind] = cogrectangle;
                    }
                    else
                    {
                        cogPMAlign.SearchRegion = null;

                        //20.12.17 lkw DL 
                        ROI_Rectangle[(int)PKind] = null;
                    }
                    try
                    {
                        cogPMAlign.Run();
                    }
                    catch { }

                    double maxScore = 0;

                    if (cogPMAlign.Results != null)
                    {
                        for (int j = 0; j < cogPMAlign.Results.Count; j++)
                        {
                            if (CONST.m_bSystemLog)
                                cLog.Save(LogKind.System, "Score=" + cogPMAlign.Results[j].Score.ToString() + " / Align=" + CFG.Name + "," + sPatternFile);

                            if (cogPMAlign.Results[j].Score > double.Parse(Split[2]) &&/* (cogPMAlign.Results[j].FineStage || bRef) &&*/ maxScore < cogPMAlign.Results[j].Score)
                            {
                                maxScore = cogPMAlign.Results[j].Score;
                                CONST.TextScore = (maxScore);

                                dSearchScore[nMark] = maxScore;
                                dLimitScore[nMark] = double.Parse(Split[2]);
                                dPatternNo[nMark] = (i + 1);

                                //results = new CogPMAlignResults(cogPMAlign.Results);
                                result = new CogPMAlignResult(cogPMAlign.Results[j]);

                                iSelect = i;
                                iPattern = j;

                                //csh 20170722  Pattern Search First 기능
                                m_strFirstPattanSearchName[nMark] = sPatternFile;
                            }
                            else if (maxScore == 0)
                            {
                                dSearchScore[nMark] = maxScore;
                                dLimitScore[nMark] = double.Parse(Split[2]);
                                dPatternNo[nMark] = (i + 1);
                            }
                        }

                        // score 에 맞게 검색 되면 더 이상 pattern 을 적용 찾기를 하지 않는다. AllBest일 경우 시간은 걸리지만 모든 파일을 검토하여 최고점을 찾도록 함.
                        if (CFG.PatternSearchMode == CONST.ePatternSearchMode.LastBest)
                        {
                            if (iSelect >= 0)
                                break;
                        }
                    }
                }

                // pattern 이 찾아진 경우..
                if (result != null)
                {
                    if (CONST.m_bSystemLog)
                        cLog.Save(LogKind.System, "Find=" + CFG.Name + "," + sPatternFile);

                    //ICogGraphicInteractive[] iGraphic = new ICogGraphicInteractive[results.Count];

#if(TEST1)
                    for (int i = 0; i < iGraphic.Length; i++)
                    {
                        //iGraphic[i] = findPMAlign.Results[iPattern].CreateResultGraphics(CogPMAlignResultGraphicConstants.BoundingBox);

                        iGraphic[i] = result.CreateResultGraphics(CogPMAlignResultGraphicConstants.BoundingBox);

                        if (bRef) cogDS.InteractiveGraphics.Add(iGraphic[i], "Ref", false);
                        else cogDS.InteractiveGraphics.Add(iGraphic[i], "Pattern", false);
                    }
#else
                    // 찾아진 영역 박스 보여주기.
                    if (!bNographics)
                    {
                        ICogGraphicInteractive cogGraphic = result.CreateResultGraphics(CogPMAlignResultGraphicConstants.MatchRegion);// | CogPMAlignResultGraphicConstants.MatchFeatures);
                        cogDS.InteractiveGraphics.Add(cogGraphic, PKind.ToString(), false);
                    }
#endif

                    double tempX = result.GetPose().TranslationX;
                    double tempY = result.GetPose().TranslationY;
                    double tempR = result.GetPose().Rotation + 1.5708;//19.03.28 cjg
                    int size = 36;

                    CogLineSegment cogLine1 = new CogLineSegment(); //십자가로
                    cogLine1.StartX = tempX - size;
                    cogLine1.EndX = tempX + size;
                    cogLine1.StartY = tempY;// - lc20umPixel * 3 + 1;
                    cogLine1.EndY = tempY;
                    cogLine1.Color = CogColorConstants.Green;

                    if (!bNographics)
                    {
                        cogDS.InteractiveGraphics.Add(cogLine1, PKind.ToString(), false);
                    }

                    /////////////////////////////////////////

                    CogLineSegment cogLine2 = new CogLineSegment(); //십자세로
                    cogLine2.StartX = tempX;
                    cogLine2.EndX = tempX;
                    cogLine2.StartY = tempY - size;// - lc20umPixel * 3 + 1;
                    cogLine2.EndY = tempY + size;
                    cogLine2.Color = CogColorConstants.Green;

                    if (!bNographics)
                    {
                        cogDS.InteractiveGraphics.Add(cogLine2, PKind.ToString(), false);
                    }

                    dX = tempX;
                    dY = tempY;
                    dR = result.GetPose().Rotation;
                    bReturn = true;

#if(TEST1)
                    CogDSFontDisplay(false, dX, dY, result.Score);
#endif
                }
                else
                {
                    if (CONST.m_bSystemLog)
                        cLog.Save(LogKind.System, " Fail / Align=" + CFG.Name);

                    bReturn = false;
                }
            }
            catch
            {
                bReturn = false;
            }
            return bReturn;
        }

        public bool PatternSearch_InspMFQ(ref List<cs2DAlign.ptXYT> lpoint, ePatternKind PKind, out BitArray bit, bool bSubMark = false, bool bNographics = false)
        {
            //double dR = 0;
            bit = new BitArray(16, false);
            CogRectangle cogrectangle = new CogRectangle();
            CogPMAlignTool cogPMAlign = new CogPMAlignTool(); //중복검색 전역변수가 문제
            cogPMAlign.InputImage = (CogImage8Grey)cogDS.Image; //이미지 넣는것 한번만 넣는거와 비교해서 시간차이 없음.
            //추후 PMAlign과 SearchMax도 통합해보자
            bool bReturn = false;
            int nMark = (int)PKind;
            if (bSubMark) nMark += 10; //패턴종류 10개이하라고 보고 10더함

            //패턴점수 초기화
            dSearchScore[nMark] = 0;
            //cogPMAlign.InputImage.SelectedSpaceName = ".";
            try
            {
                string RunRecipe = CONST.RunRecipe.RecipeName.Trim(); // runrecipe 관련 수정.

                try
                {
                    this.DeleteInteractiveGraphic(cogDS, PKind.ToString());
                }
                catch
                { }

                // lyw. Recipe 폴더명 추가. 170113.
                string sPatternDir = Path.Combine(CONST.cVisionPath, CFG.Name.Trim(), CONST.stringRcp, RunRecipe.Trim(), "PMAlign", PKind.ToString());
                //string sPatternDir = Path.Combine(CONST.cVisionPath, CFG.Name.Trim(), CONST.stringRcp, RunRecipe.Trim(), "Pattern");

                if (bSubMark)
                {
                    sPatternDir = Path.Combine(sPatternDir, "2ndMark");
                }

                string sPatternFile = "";

                if (!Directory.Exists(sPatternDir)) Directory.CreateDirectory(sPatternDir);

                string[] sFile = Directory.GetFiles(sPatternDir, "*.vpp");

                int iSelect = -1;
                int iPattern = -1;

                // 결과 저장용.
                CogPMAlignResults results = null;

                //20170804
                //int nCount = 0;
                //for (int i = 0; i < sFile.Length; i++)
                //{
                //    if (sFile[i].IndexOf(".vpp") > 0)
                //    {
                //        nCount++;
                //    }
                //}

                for (int i = 0; i < sFile.Length; i++)
                {
                    if (sFile[i].IndexOf(".vpp") > 0)  // vpp File 이 있으면 
                    {
                        string[] Split;

                        sPatternFile = sFile[i];

                        // file name 에서만 split 하도록 수정. lyw. 170214.
                        Split = Path.GetFileName(sPatternFile).Split('_');

                        if (pmAlignPatterns.ContainsKey(sPatternFile))
                            cogPMAlign.Pattern = pmAlignPatterns[sPatternFile];
                        else
                        {
                            pmAlignPatterns[sPatternFile] = (CogPMAlignPattern)CogSerializer.LoadObjectFromFile(sPatternFile);
                            cogPMAlign.Pattern = pmAlignPatterns[sPatternFile];
                        }
                        cogPMAlign.InputImage = (CogImage8Grey)cogDS.Image;
                        cogPMAlign.RunParams.ContrastThreshold = double.Parse(Split[1]);

                        //20161030 ljh
                        cogPMAlign.Pattern.IgnorePolarity = bool.Parse(Split[5]);

                        if (Split[3] != "0" && Split[4] != "0")
                        {
                            cogPMAlign.RunParams.ZoneAngle.Configuration = CogPMAlignZoneConstants.LowHigh;
                            cogPMAlign.RunParams.ZoneAngle.Low = double.Parse(Split[3]) * dRadian;
                            cogPMAlign.RunParams.ZoneAngle.High = double.Parse(Split[4]) * dRadian;
                        }
                        else
                        {
                            cogPMAlign.RunParams.ZoneAngle.Configuration = CogPMAlignZoneConstants.Nominal;
                        }

                        // 까만색일 경우 적용할 경우 효과 있음... 잘 찾는다. 하지만 섞여져 있을 경우는 못찾음... 점수가 낮음.
                        //if (bFoam)
                        //    cogPMAlign.RunParams.ScoreUsingClutter = true;
                        //else
                        //    cogPMAlign.RunParams.ScoreUsingClutter = false;

                        cogPMAlign.RunParams.ScoreUsingClutter = false;
                        cogPMAlign.RunParams.SaveMatchInfo = true;
                        if (Split.Length > 8)
                        {
                            cogrectangle.SetXYWidthHeight(double.Parse(Split[7]), double.Parse(Split[8]), double.Parse(Split[9]), double.Parse(Split[10]));
                            cogPMAlign.SearchRegion = cogrectangle;

                            //pcy190421
                            if (bNographics) { }
                            else
                            {
                                cogrectangle.GraphicDOFEnable = CogRectangleDOFConstants.All;
                                cogrectangle.Interactive = false;
                                //pcy190421
                                if (!bNographics)
                                {
                                    cogrectangle.GraphicDOFEnable = CogRectangleDOFConstants.All;
                                    cogrectangle.Interactive = true;
                                    cogDS.InteractiveGraphics.Add(cogrectangle, PKind.ToString(), false); // 검색 영역 표시 부분.
                                }
                            }
                        }
                        else
                        {
                            cogPMAlign.SearchRegion = null;
                        }
                        try
                        {
                            cogPMAlign.Run();
                        }
                        catch { }

                        double maxScore = 0;

                        if (cogPMAlign.Results != null)
                        {

                            for (int j = 0; j < cogPMAlign.Results.Count; j++)
                            {
                                if (CONST.m_bSystemLog)
                                    cLog.Save(LogKind.System, "Score=" + cogPMAlign.Results[j].Score.ToString() + " / Align=" + CFG.Name + "," + sPatternFile);

                                if (cogPMAlign.Results[j].Score > double.Parse(Split[2]) &&/* (cogPMAlign.Results[j].FineStage || bRef) &&*/ maxScore < cogPMAlign.Results[j].Score)
                                {
                                    maxScore = cogPMAlign.Results[j].Score;
                                    //CONST.TextScore = (maxScore);

                                    sAppliedPattern = sPatternFile;
                                    dSearchScore[nMark] = maxScore;
                                    dLimitScore[nMark] = double.Parse(Split[2]);
                                    dPatternNo[nMark] = (i + 1);

                                    results = new CogPMAlignResults(cogPMAlign.Results);

                                    iSelect = i;
                                    iPattern = j;

                                    //csh 20170722  Pattern Search First 기능
                                    m_strFirstPattanSearchName[nMark] = sPatternFile;
                                    //break;  // 생각해 봐야 함.
                                }
                                else
                                {
                                    dSearchScore[nMark] = maxScore;
                                    dLimitScore[nMark] = double.Parse(Split[2]);
                                    dPatternNo[nMark] = (i + 1);
                                }

                            }

                            // score 에 맞게 검색 되면 더 이상 pattern 을 적용 찾기를 하지 않는다. AllBest일 경우 시간은 걸리지만 모든 파일을 검토하여 최고점을 찾도록 함.
                            //if (CFG.PatternSearchMode == CONST.ePatternSearchMode.LastBest)
                            //{
                            //    if (iSelect >= 0)
                            //        break;
                            //}

                            // pattern 이 찾아진 경우..
                            if (results != null)
                            {
                                bit.Set(i, true);
                                if (CONST.m_bSystemLog)
                                    cLog.Save(LogKind.System, "Find=" + CFG.Name + "," + sPatternFile);

                                CogPMAlignResult result = null;

                                result = (CogPMAlignResult)results[iPattern];

                                ICogGraphicInteractive[] iGraphic = new ICogGraphicInteractive[results.Count];

                                //#if(TEST1)
                                //for (int j = 0; j < iGraphic.Length; j++) //테스트
                                //{
                                //    //iGraphic[j] = findPMAlign.Results[iPattern].CreateResultGraphics(CogPMAlignResultGraphicConstants.BoundingBox);

                                //    iGraphic[j] = result.CreateResultGraphics(CogPMAlignResultGraphicConstants.BoundingBox);

                                //    if (bRef) cogDS.StaticGraphics.Add(iGraphic[j], "Ref");
                                //    else cogDS.StaticGraphics.Add(iGraphic[j], "Pattern");
                                //}
                                //#else
                                // 찾아진 영역 박스 보여주기.
                                if (!bNographics)
                                {
                                    ICogGraphicInteractive cogGraphic = result.CreateResultGraphics(CogPMAlignResultGraphicConstants.MatchRegion);
                                    cogDS.InteractiveGraphics.Add(cogGraphic, PKind.ToString(), false);
                                }

                                double tempX = result.GetPose().TranslationX;
                                double tempY = result.GetPose().TranslationY;
                                double tempR = result.GetPose().Rotation + 1.5708;//19.03.28 cjg
                                CogLineSegment cogLine1 = new CogLineSegment();
                                int size = 36;

                                cogLine1.StartX = tempX - size;
                                cogLine1.EndX = tempX + size;
                                cogLine1.StartY = tempY;// - lc20umPixel * 3 + 1;
                                cogLine1.EndY = tempY;
                                cogLine1.Color = CogColorConstants.Orange;

                                //cogLine1.lin

                                if (!bNographics)
                                {
                                    cogDS.InteractiveGraphics.Add(cogLine1, PKind.ToString(), false);
                                }

                                /////////////////////////////////////////

                                CogLineSegment cogLine2 = new CogLineSegment();

                                cogLine2.StartX = tempX;
                                cogLine2.EndX = tempX;
                                cogLine2.StartY = tempY - size;// - lc20umPixel * 3 + 1;
                                cogLine2.EndY = tempY + size;
                                cogLine2.Color = CogColorConstants.Orange;

                                if (!bNographics)
                                {
                                    cogDS.InteractiveGraphics.Add(cogLine2, PKind.ToString(), false);
                                }

                                cs2DAlign.ptXYT pt = new cs2DAlign.ptXYT();
                                pt.X = tempX;
                                pt.Y = tempY;
                                //dX = tempX;
                                //dY = tempY;
                                pt.T = result.GetPose().Rotation;
                                //dR = result.GetPose().Rotation;

                                //#if(TEST1)
                                //CogDSFontDisplay(false, tempX, tempY, result.Score); //테스트

                                lpoint.Add(pt);

                                bReturn = true;
                            }
                            else
                            {
                                if (CONST.m_bSystemLog)
                                    cLog.Save(LogKind.System, " Fail / Align=" + CFG.Name);

                                bReturn = false;
                            }
                        }
                    }
                }
            }
            catch
            {
                bReturn = false;
            }
            return bReturn;
        }
        public bool PatternSearch_Tray(ref List<cs2DAlign.ptXYT> lpoint, ePatternKind PKind, out BitArray bit, bool bSubMark = false, bool bNographics = false)
        {
            //pcy210115 Tray특수
            //tray Loader용
            //1번트레이는 01번마크 2번트레이는 02번마크
            //double dR = 0;
            bit = new BitArray(16, false);
            CogRectangle[] cogrectangle; // = new CogRectangle();
            CogPMAlignTool cogPMAlign = new CogPMAlignTool(); //중복검색 전역변수가 문제
            cogPMAlign.InputImage = (CogImage8Grey)cogDS.Image; //이미지 넣는것 한번만 넣는거와 비교해서 시간차이 없음.
            //추후 PMAlign과 SearchMax도 통합해보자
            bool bReturn = false;
            int nMark = (int)PKind;
            if (bSubMark) nMark += 10; //패턴종류 10개이하라고 보고 10더함

            //패턴점수 초기화
            dSearchScore[nMark] = 0;
            //cogPMAlign.InputImage.SelectedSpaceName = ".";
            try
            {
                string RunRecipe = CONST.RunRecipe.RecipeName.Trim(); // runrecipe 관련 수정.



                // lyw. Recipe 폴더명 추가. 170113.
                string sPatternDir = Path.Combine(CONST.cVisionPath, CFG.Name.Trim(), CONST.stringRcp, RunRecipe.Trim(), "PMAlign", PKind.ToString());
                //string sPatternDir = Path.Combine(CONST.cVisionPath, CFG.Name.Trim(), CONST.stringRcp, RunRecipe.Trim(), "Pattern");

                if (bSubMark)
                {
                    sPatternDir = Path.Combine(sPatternDir, "2ndMark");
                }

                string sPatternFile = "";

                if (!Directory.Exists(sPatternDir)) Directory.CreateDirectory(sPatternDir);

                string[] sFile = Directory.GetFiles(sPatternDir, "*.vpp");

                try
                {
                    this.DeleteInteractiveGraphic(cogDS, PKind.ToString());
                    //for (int i = 0; i < sFile.Length;i++)  this.DeleteInteractiveGraphic(cogDS, PKind.ToString() + "_" + i.ToString());                    
                }
                catch
                { }

                cogrectangle = new CogRectangle[sFile.Length];

                int iSelect = -1;
                int iPattern = -1;

                // 결과 저장용.
                CogPMAlignResults results = null;

                //20170804
                //int nCount = 0;
                //for (int i = 0; i < sFile.Length; i++)
                //{
                //    if (sFile[i].IndexOf(".vpp") > 0)
                //    {
                //        nCount++;
                //    }
                //}

                for (int i = 0; i < sFile.Length; i++)
                {
                    if (sFile[i].IndexOf(".vpp") > 0)  // vpp File 이 있으면 
                    {
                        string[] Split;

                        sPatternFile = sFile[i];

                        // file name 에서만 split 하도록 수정. lyw. 170214.
                        Split = Path.GetFileName(sPatternFile).Split('_');

                        if (pmAlignPatterns.ContainsKey(sPatternFile))
                            cogPMAlign.Pattern = pmAlignPatterns[sPatternFile];
                        else
                        {
                            pmAlignPatterns[sPatternFile] = (CogPMAlignPattern)CogSerializer.LoadObjectFromFile(sPatternFile);
                            cogPMAlign.Pattern = pmAlignPatterns[sPatternFile];
                        }
                        cogPMAlign.InputImage = (CogImage8Grey)cogDS.Image;
                        cogPMAlign.RunParams.ContrastThreshold = double.Parse(Split[1]);

                        //20161030 ljh
                        cogPMAlign.Pattern.IgnorePolarity = bool.Parse(Split[5]);

                        if (Split[3] != "0" && Split[4] != "0")
                        {
                            cogPMAlign.RunParams.ZoneAngle.Configuration = CogPMAlignZoneConstants.LowHigh;
                            cogPMAlign.RunParams.ZoneAngle.Low = double.Parse(Split[3]) * dRadian;
                            cogPMAlign.RunParams.ZoneAngle.High = double.Parse(Split[4]) * dRadian;
                        }
                        else
                        {
                            cogPMAlign.RunParams.ZoneAngle.Configuration = CogPMAlignZoneConstants.Nominal;
                        }

                        // 까만색일 경우 적용할 경우 효과 있음... 잘 찾는다. 하지만 섞여져 있을 경우는 못찾음... 점수가 낮음.
                        //if (bFoam)
                        //    cogPMAlign.RunParams.ScoreUsingClutter = true;
                        //else
                        //    cogPMAlign.RunParams.ScoreUsingClutter = false;

                        cogPMAlign.RunParams.ScoreUsingClutter = false;
                        cogPMAlign.RunParams.SaveMatchInfo = true;
                        if (Split.Length > 8)
                        {
                            if (cogrectangle[i] == null) cogrectangle[i] = new CogRectangle();
                            cogrectangle[i].SetXYWidthHeight(double.Parse(Split[7]), double.Parse(Split[8]), double.Parse(Split[9]), double.Parse(Split[10]));
                            cogPMAlign.SearchRegion = cogrectangle[i];

                            //pcy190421
                            if (bNographics) { }
                            else
                            {
                                cogrectangle[i].GraphicDOFEnable = CogRectangleDOFConstants.All;
                                cogrectangle[i].Interactive = false;
                                //pcy190421
                                if (!bNographics)
                                {
                                    cogrectangle[i].GraphicDOFEnable = CogRectangleDOFConstants.All;
                                    cogrectangle[i].Interactive = true;
                                    //cogDS.InteractiveGraphics.Add(cogrectangle[i], PKind.ToString() + "_" + i.ToString(), false); // 검색 영역 표시 부분.
                                    cogDS.InteractiveGraphics.Add(cogrectangle[i], PKind.ToString(), false); // 검색 영역 표시 부분.
                                }
                            }
                        }
                        else
                        {
                            cogPMAlign.SearchRegion = null;
                        }
                        try
                        {
                            cogPMAlign.Run();
                        }
                        catch { }

                        double maxScore = 0;

                        if (cogPMAlign.Results != null)
                        {

                            for (int j = 0; j < cogPMAlign.Results.Count; j++)
                            {
                                if (CONST.m_bSystemLog)
                                    cLog.Save(LogKind.System, "Score=" + cogPMAlign.Results[j].Score.ToString() + " / Align=" + CFG.Name + "," + sPatternFile);

                                if (cogPMAlign.Results[j].Score > double.Parse(Split[2]) &&/* (cogPMAlign.Results[j].FineStage || bRef) &&*/ maxScore < cogPMAlign.Results[j].Score)
                                {
                                    maxScore = cogPMAlign.Results[j].Score;
                                    //CONST.TextScore = (maxScore);

                                    sAppliedPattern = sPatternFile;
                                    dSearchScore[nMark] = maxScore;
                                    dLimitScore[nMark] = double.Parse(Split[2]);

                                    results = new CogPMAlignResults(cogPMAlign.Results);

                                    iSelect = i;
                                    iPattern = j;

                                    //csh 20170722  Pattern Search First 기능
                                    m_strFirstPattanSearchName[nMark] = sPatternFile;
                                    //break;  // 생각해 봐야 함.

                                    int.TryParse(Split[0], out int trayno);
                                    if (trayno != 0)
                                    {
                                        trayno--;
                                        if (!bit.Get(trayno))
                                        {
                                            bit.Set(trayno, true);
                                        }
                                    }
                                }
                                else
                                {
                                    dSearchScore[nMark] = maxScore;
                                    dLimitScore[nMark] = double.Parse(Split[2]);
                                }
                            }

                            // score 에 맞게 검색 되면 더 이상 pattern 을 적용 찾기를 하지 않는다. AllBest일 경우 시간은 걸리지만 모든 파일을 검토하여 최고점을 찾도록 함.
                            //if (CFG.PatternSearchMode == CONST.ePatternSearchMode.LastBest)
                            //{
                            //    if (iSelect >= 0)
                            //        break;
                            //}

                            // pattern 이 찾아진 경우..
                            if (results != null)
                            {


                                //bit.Set(i, true);
                                if (CONST.m_bSystemLog)
                                    cLog.Save(LogKind.System, "Find=" + CFG.Name + "," + sPatternFile);

                                CogPMAlignResult result = null;

                                result = (CogPMAlignResult)results[iPattern];

                                ICogGraphicInteractive[] iGraphic = new ICogGraphicInteractive[results.Count];

                                //#if(TEST1)
                                //for (int j = 0; j < iGraphic.Length; j++) //테스트
                                //{
                                //    //iGraphic[j] = findPMAlign.Results[iPattern].CreateResultGraphics(CogPMAlignResultGraphicConstants.BoundingBox);

                                //    iGraphic[j] = result.CreateResultGraphics(CogPMAlignResultGraphicConstants.BoundingBox);

                                //    if (bRef) cogDS.StaticGraphics.Add(iGraphic[j], "Ref");
                                //    else cogDS.StaticGraphics.Add(iGraphic[j], "Pattern");
                                //}
                                //#else
                                // 찾아진 영역 박스 보여주기.
                                if (!bNographics)
                                {
                                    ICogGraphicInteractive cogGraphic = result.CreateResultGraphics(CogPMAlignResultGraphicConstants.MatchRegion);
                                    cogDS.InteractiveGraphics.Add(cogGraphic, PKind.ToString(), false);
                                }

                                double tempX = result.GetPose().TranslationX;
                                double tempY = result.GetPose().TranslationY;
                                double tempR = result.GetPose().Rotation + 1.5708;//19.03.28 cjg
                                CogLineSegment cogLine1 = new CogLineSegment();
                                int size = 36;

                                cogLine1.StartX = tempX - size;
                                cogLine1.EndX = tempX + size;
                                cogLine1.StartY = tempY;// - lc20umPixel * 3 + 1;
                                cogLine1.EndY = tempY;
                                cogLine1.Color = CogColorConstants.Orange;

                                //cogLine1.lin

                                if (!bNographics)
                                {
                                    cogDS.InteractiveGraphics.Add(cogLine1, PKind.ToString(), false);
                                }

                                /////////////////////////////////////////

                                CogLineSegment cogLine2 = new CogLineSegment();

                                cogLine2.StartX = tempX;
                                cogLine2.EndX = tempX;
                                cogLine2.StartY = tempY - size;// - lc20umPixel * 3 + 1;
                                cogLine2.EndY = tempY + size;
                                cogLine2.Color = CogColorConstants.Orange;

                                if (!bNographics)
                                {
                                    cogDS.InteractiveGraphics.Add(cogLine2, PKind.ToString(), false);
                                }

                                cs2DAlign.ptXYT pt = new cs2DAlign.ptXYT();
                                pt.X = tempX;
                                pt.Y = tempY;
                                //dX = tempX;
                                //dY = tempY;
                                pt.T = result.GetPose().Rotation;
                                //dR = result.GetPose().Rotation;

                                //#if(TEST1)
                                //CogDSFontDisplay(false, tempX, tempY, result.Score); //테스트

                                lpoint.Add(pt);

                                bReturn = true;
                            }
                            else
                            {
                                //if (CONST.m_bSystemLog)
                                //    cLog.Save(LogKind.System, " Fail / Align=" + CFG.Name);

                                bReturn = true;
                            }
                        }
                    }
                }
            }
            catch
            {
                bReturn = false;
            }
            return bReturn;
        }
        //2018.07.10 SCF 부착 검사 추가 khs
        public bool AttachInspection(double dX, double dY, double OffsetX, double OffsetY, double InspTH, ref int TH, double theta = 0)
        {
            Cognex.VisionPro.CogRectangle cr = new CogRectangle();
            try
            {
                //this.DeleteInteractiveGraphic(cogDS, "SCF");
            }
            catch
            {
            }
            try
            {
                //int THCnt = 0;
                double checkL = 1; //임의크기 mm

                //pcy190802 세타보정 추가, cr기준위치 중심으로 변경.
                //기본 부착 검사 위치는 마크 위치로 
                //cr.X = dX;
                //cr.Y = dY;

                //Setting에서 위치 조정할 수 있게
                eCalPos calpos = eCalPos.Err;
                Menu.frmAutoMain.GetCalPos(ref calpos, CFG.Camno);
                cs2DAlign.ptXY resolution = new cs2DAlign.ptXY();
                cs2DAlign.ptXY pixelCnt = new cs2DAlign.ptXY();
                Menu.rsAlign.getResolution((int)calpos, ref resolution, ref pixelCnt);
                double offsetX = OffsetX / resolution.X;
                double offsetY = OffsetY / resolution.Y; //pcy190802 -삭제


                theta = (-1) * theta; //ref왼쪽 pattern오른쪽에서 찾은 각도라서 -1곱해줌
                double offsetX1 = (Math.Cos(theta) * offsetX) - (Math.Sin(theta) * offsetY);
                double offsetY1 = (Math.Sin(theta) * offsetX) + (Math.Cos(theta) * offsetY);

                //cr.X = cr.X + offsetX1;
                //cr.Y = cr.Y + offsetY1;

                //cr.Width = checkL / CFG.Resolution;
                //cr.Height = checkL / CFG.Resolution;

                double WH = checkL / resolution.X; //x,y아무거나 사용

                cr.SetCenterWidthHeight(dX + offsetX1, dY + offsetY1, WH, WH);

                Cognex.VisionPro.ImageProcessing.CogHistogramTool crH = new Cognex.VisionPro.ImageProcessing.CogHistogramTool();
                crH.InputImage = cogDS.Image;
                crH.Region = cr;
                crH.Run();

                if (InspTH > crH.Result.Median)
                {
                    cr.Color = CogColorConstants.Green;
                    cogDS.InteractiveGraphics.Add(cr, "SCF", false);
                    return true;
                }
                else
                {
                    cr.Color = CogColorConstants.Red;
                    cogDS.InteractiveGraphics.Add(cr, "SCF", false);
                    return false;
                }
                //int[] result = crH.Result.GetHistogram();
                //int iCount = 0;
                //int tCount = 0;
                //try
                //{
                //    double pixelcnt = 0;
                //    for (int i = 0; i < result.Length; i++) pixelcnt = pixelcnt + result[i];

                //    pixelcnt = pixelcnt * 0.9;  // 90 % Check


                //    //int iLimit = 120;
                //    int iLimit = Convert.ToInt32(InspTH);

                //    for (int i = 0; i <= iLimit; i++)
                //    {
                //        iCount = iCount + result[i];
                //    }

                //    //적정 TH값 Disp 하기 위한 구간
                //    for (int i = 0; i < result.Length; i++)
                //    {
                //        tCount = tCount + result[i];
                //        if (tCount > pixelcnt && THCnt == 0)
                //        {
                //            TH = i;
                //            THCnt++;
                //        }
                //    }

                //    if (iCount > pixelcnt)
                //    {
                //        cr.Color = CogColorConstants.Green;
                //        cogDS.InteractiveGraphics.Add(cr, "SCF", false);

                //        return true;
                //    }
                //    else
                //    {
                //        cr.Color = CogColorConstants.Red;
                //        cogDS.InteractiveGraphics.Add(cr, "SCF", false);
                //        if (iLimit <= 0)
                //        {
                //            return true; // 미사용
                //        }
                //        return false;
                //    }
                //}
                //catch
                //{
                //    return false;
                //}
            }
            catch
            {
                return false;
            }
        }
        //2021.05.18 lkw QR/Data Matrix read 함수 추가..... Laser Marking 용
        Cognex.VisionPro.ID.CogIDTool ReadTrainID = new Cognex.VisionPro.ID.CogIDTool();
        Cognex.VisionPro.ID.CogIDTool FindTrainID = new Cognex.VisionPro.ID.CogIDTool();
        //public bool readID(ref string readID, eID readKind, bool isAuto = false, bool First = false)
        //{
        //    // Train 기능을 사용할 경우 한번 read한 이미지와 비슷한 Size는 더 잘 찾음....
        //    // Size가 달라질 경우 못찾음... Train을 안 할 경우 Size 상관없이 찾음.....
        //    // 같은 Size의 ID를 여러개 동시에 찾을 경우 Train 하는 것이 좋을 듯...

        //    // ROI를 추가 해야 하나...고민중.

        //    try
        //    {
        //        this.DeleteInteractiveGraphic(cogDS, "id");
        //    }
        //    catch
        //    {

        //    }
        //    if (isAuto)
        //    {
        //        ReadTrainID.InputImage = (CogImage8Grey)cogDS.Image;
        //        ReadTrainID.Run();
        //        if (FindTrainID.RunStatus.Result == CogToolResultConstants.Accept)
        //        {
        //            if (ReadTrainID.Results.Count > 0)
        //            {
        //                readID = ReadTrainID.Results[0].DecodedData.DecodedString;
        //                CogPolygon searchResion = FindTrainID.Results[0].BoundsPolygon;
        //                searchResion.Color = CogColorConstants.Magenta;
        //                cogDS.InteractiveGraphics.Add(searchResion, "id", false);
        //                return true;
        //            }
        //            else return false;
        //        }
        //        else
        //        {
        //            FindTrainID.InputImage = (CogImage8Grey)cogDS.Image;
        //            FindTrainID.RunParams.ProcessingMode = Cognex.VisionPro.ID.CogIDProcessingModeConstants.IDMax;
        //            FindTrainID.RunParams.DecodedStringCodePage = Cognex.VisionPro.ID.CogIDCodePageConstants.ANSILatin1;
        //            FindTrainID.RunParams.DisableAllCodes();
        //            if (readKind == eID.DataMatrix) FindTrainID.RunParams.DataMatrix.Enabled = true;
        //            else if (readKind == eID.QRCode)
        //            {
        //                FindTrainID.RunParams.QRCode.Enabled = true;
        //                FindTrainID.RunParams.QRCode.Model = Cognex.VisionPro.ID.CogIDQRCodeModelConstants.All;  //시간이 더 걸릴 라나......
        //            }
        //            FindTrainID.Run();

        //            if (FindTrainID.RunStatus.Result == CogToolResultConstants.Accept)
        //            {
        //                if (FindTrainID.Results.Count > 0)
        //                {
        //                    readID = FindTrainID.Results[0].DecodedData.DecodedString;

        //                    CogPolygon searchResion = FindTrainID.Results[0].BoundsPolygon;
        //                    searchResion.Color = CogColorConstants.Magenta;
        //                    cogDS.InteractiveGraphics.Add(searchResion, "id", false);
        //                    return true;
        //                }
        //                else return false;
        //            }
        //            else return false;
        //        }
        //    }
        //    else
        //    {
        //        if (First)
        //        {
        //            string Trainpath = Path.Combine(CONST.cTrainPath + "\\" + "TrainImage.jpg");
        //            if (File.Exists(Trainpath))
        //            {
        //                Bitmap imageload = (Bitmap)Image.FromFile(Trainpath);
        //                ReadTrainID.InputImage = new CogImage8Grey(imageload);
        //                ReadTrainID.RunParams.Train(ReadTrainID.InputImage, null);
        //                ReadTrainID.RunParams.ProcessingMode = Cognex.VisionPro.ID.CogIDProcessingModeConstants.IDMax;
        //                ReadTrainID.RunParams.DecodedStringCodePage = Cognex.VisionPro.ID.CogIDCodePageConstants.ANSILatin1;
        //                ReadTrainID.RunParams.DisableAllCodes();
        //                if (readKind == eID.DataMatrix) ReadTrainID.RunParams.DataMatrix.Enabled = true;
        //                ReadTrainID.Run();
        //                if (FindTrainID.RunStatus.Result == CogToolResultConstants.Accept)
        //                {
        //                    return true;
        //                }
        //                else
        //                {
        //                    MessageBox.Show("MCR Train Fail");
        //                    return false;
        //                }
        //            }
        //            else return false;
        //        }
        //        else
        //        {
        //            FindTrainID.InputImage = (CogImage8Grey)cogDS.Image;
        //            FindTrainID.RunParams.ProcessingMode = Cognex.VisionPro.ID.CogIDProcessingModeConstants.IDMax;
        //            FindTrainID.RunParams.DecodedStringCodePage = Cognex.VisionPro.ID.CogIDCodePageConstants.ANSILatin1;
        //            FindTrainID.RunParams.DisableAllCodes();
        //            if (readKind == eID.DataMatrix) FindTrainID.RunParams.DataMatrix.Enabled = true;
        //            else if (readKind == eID.QRCode)
        //            {
        //                FindTrainID.RunParams.QRCode.Enabled = true;
        //                FindTrainID.RunParams.QRCode.Model = Cognex.VisionPro.ID.CogIDQRCodeModelConstants.All;  //시간이 더 걸릴 라나......
        //            }
        //            FindTrainID.Run();

        //            if (FindTrainID.RunStatus.Result == CogToolResultConstants.Accept)
        //            {
        //                if (FindTrainID.Results.Count > 0)
        //                {
        //                    if (!FindTrainID.RunParams.Trained)
        //                    {
        //                        FindTrainID.RunParams.Train((ICogImage)cogDS.Image, null);

        //                    }
        //                    readID = FindTrainID.Results[0].DecodedData.DecodedString;

        //                    CogPolygon searchResion = FindTrainID.Results[0].BoundsPolygon;
        //                    searchResion.Color = CogColorConstants.Magenta;
        //                    cogDS.InteractiveGraphics.Add(searchResion, "id", false);
        //                    return true;
        //                }
        //                else return false;
        //            }
        //            else
        //            {

        //                FindTrainID.RunParams.Untrain();
        //                return false;
        //            }
        //        }
        //    }
        //}

        public bool readID(ref string readID, ref cs2DAlign.ptXY[] CodePoint, eID readKind, CogRectangle region = null)
        {
            // Train 기능을 사용할 경우 한번 read한 이미지와 비슷한 Size는 더 잘 찾음....
            // Size가 달라질 경우 못찾음... Train을 안 할 경우 Size 상관없이 찾음.....
            // 같은 Size의 ID를 여러개 동시에 찾을 경우 Train 하는 것이 좋을 듯...

            // ROI를 추가 해야 하나...고민중.

            try
            {
                this.DeleteInteractiveGraphic(cogDS, "id");
            }
            catch
            {

            }

            if (region != null)
            {
                cogDS.InteractiveGraphics.Add(region, "id", false);
            }

            if (CFG.RetryCnt < 1) CFG.RetryCnt = 1;
            for (int i = 0; i < CFG.RetryCnt; i++)
            {
                FindTrainID.InputImage = (CogImage8Grey)cogDS.Image;
                FindTrainID.RunParams.ProcessingMode = Cognex.VisionPro.ID.CogIDProcessingModeConstants.IDMax;
                FindTrainID.RunParams.DecodedStringCodePage = Cognex.VisionPro.ID.CogIDCodePageConstants.ANSILatin1;
                FindTrainID.RunParams.DisableAllCodes();
                if (readKind == eID.DataMatrix) FindTrainID.RunParams.DataMatrix.Enabled = true;
                else if (readKind == eID.QRCode)
                {
                    FindTrainID.RunParams.QRCode.Enabled = true;
                    FindTrainID.RunParams.QRCode.Model = Cognex.VisionPro.ID.CogIDQRCodeModelConstants.All;  //시간이 더 걸릴 라나......
                }


                FindTrainID.Region = region;
                FindTrainID.Run();

                if (FindTrainID.RunStatus.Result == CogToolResultConstants.Accept)
                {
                    if (FindTrainID.Results.Count > 0)
                    {
                        if (!FindTrainID.RunParams.Trained)
                        {
                            FindTrainID.RunParams.Train((ICogImage)cogDS.Image, region);
                        }
                        readID = FindTrainID.Results[0].DecodedData.DecodedString;

                        CogPolygon searchResion = FindTrainID.Results[0].BoundsPolygon;
                        searchResion.Color = CogColorConstants.Magenta;
                        cogDS.InteractiveGraphics.Add(searchResion, "id", false);

                        CogGraphicLabel label = new CogGraphicLabel();
                        label.Color = CogColorConstants.Green;
                        label.SetXYText(FindTrainID.Results[0].CenterX, FindTrainID.Results[0].CenterY + 500, readID);
                        cogDS.InteractiveGraphics.Add(label, "id", false);

                        



                        CodePoint = new cs2DAlign.ptXY[4];
                        for (int j = 0; j < CodePoint.Length; j++)
                        {
                            searchResion.GetVertex(j, out CodePoint[j].X, out CodePoint[j].Y);
                            
                            //0 left Up

                        }
                        return true;
                    }
                    else
                    {
                        FindTrainID.RunParams.Untrain();
                        //return false;
                    }
                }
                else
                {
                    FindTrainID.RunParams.Untrain();
                    //return false;
                }                
            }
            return false;
        }


        private void TrainImageSave(eID TrainKind)
        {

        }
        public bool DetachInspection(double dX, double dY, double OffsetX, double OffsetY, double LimitTH, ref int TH, double size = 1, double theta = 0, bool bbright = false, int dispNo = 1)
        {
            //0검은색 255하얀색
            //박리 후 더 어두우면 bbright = false;
            //TH가 Limit보다 작아야 OK(박리완료)
            //박리 후 더 밝으면 bbright = true;
            //TH가 Limit보다 커야 OK(박리완료)
            if (OffsetX == 0 && OffsetY == 0) return true;
            bool bresult = false;
            Cognex.VisionPro.CogRectangle cr = new CogRectangle();
            try
            {
                if (dispNo == 1) this.DeleteInteractiveGraphic(cogDS, "DetachInsp1");
                else this.DeleteInteractiveGraphic(cogDS, "DetachInsp2");
            }
            catch
            {

            }
            try
            {
                //int THCnt = 0;
                double checkL = size; //임의크기 mm

                //pcy190802 세타보정 추가, cr기준위치 중심으로 변경.
                //기본 부착 검사 위치는 마크 위치로 
                //cr.X = dX;
                //cr.Y = dY;

                //Setting에서 위치 조정할 수 있게
                eCalPos calpos = eCalPos.Err;
                Menu.frmAutoMain.GetCalPos(ref calpos, CFG.Camno);
                cs2DAlign.ptXY resolution = new cs2DAlign.ptXY();
                cs2DAlign.ptXY pixelCnt = new cs2DAlign.ptXY();
                Menu.rsAlign.getResolution((int)calpos, ref resolution, ref pixelCnt);

                if (resolution.X == 0) resolution.X = 0.0044;
                if (resolution.Y == 0) resolution.Y = 0.0044;
                double offsetX = OffsetX / resolution.X;
                double offsetY = OffsetY / resolution.Y; //pcy190802 -삭제

                //if (OffsetX > 0) theta = (-1) * theta; //확인필요 ref왼쪽 pattern오른쪽에서 찾은 각도라서 -1곱해줌
                //else if (OffsetX < 0) theta = theta;

                double offsetX1 = (Math.Cos(theta) * offsetX) - (Math.Sin(theta) * offsetY);
                double offsetY1 = (Math.Sin(theta) * offsetX) + (Math.Cos(theta) * offsetY);

                //cr.X = cr.X + offsetX1;
                //cr.Y = cr.Y + offsetY1;

                //cr.Width = checkL / CFG.Resolution;
                //cr.Height = checkL / CFG.Resolution;

                double WH = checkL / resolution.X; //x,y아무거나 사용

                cr.SetCenterWidthHeight(dX + offsetX1, dY + offsetY1, WH, WH);
                //딥러닝 학습을 위한 이미지상 위치
                searchDatas[0].PointX = dX + offsetX1;
                searchDatas[0].PointY = dY + offsetY1;
                searchDatas[0].ImgWidth = WH * 3;
                searchDatas[0].ImgHeight = WH * 3;

                Cognex.VisionPro.ImageProcessing.CogHistogramTool crH = new Cognex.VisionPro.ImageProcessing.CogHistogramTool();
                crH.InputImage = cogDS.Image;
                crH.Region = cr;
                crH.Run();
                TH = crH.Result.Median;

                if (!bbright)
                {
                    if (TH < LimitTH) bresult = true;
                }
                else
                {
                    if (TH > LimitTH) bresult = true;
                }

                if (bresult)
                {
                    cr.Color = CogColorConstants.Green;
                    if (dispNo == 1) cogDS.InteractiveGraphics.Add(cr, "DetachInsp1", false);
                    else cogDS.InteractiveGraphics.Add(cr, "DetachInsp2", false);
                }
                else
                {
                    cr.Color = CogColorConstants.Red;
                    if (dispNo == 1) cogDS.InteractiveGraphics.Add(cr, "DetachInsp1", false);
                    else cogDS.InteractiveGraphics.Add(cr, "DetachInsp2", false);
                }
                return bresult;
            }
            catch
            {
                return false;
            }
        }

        public bool MarkingHistoInspection(CogRectangle cr, double LimitTH,ref int ISgray,bool bbright = false)
        {
            //0검은색 255하얀색
            //박리 후 더 어두우면 bbright = false;
            //TH가 Limit보다 작아야 OK(박리완료)
            //박리 후 더 밝으면 bbright = true;
            //TH가 Limit보다 커야 OK(박리완료)
            bool bresult = false;
            if (LimitTH == 0)
            {
                return true;  //not use
            }
            try
            {
                //DeleteInteractiveGraphic(cogDS, "DetachInsp1");                
            }
            catch
            {

            }
            try
            {
                CogHistogramTool crH = new CogHistogramTool();
                crH.InputImage = cogDS.Image;
                crH.Region = cr;
                crH.Run();
                ISgray = crH.Result.Median;

                if (!bbright)
                {
                    if (ISgray < LimitTH) bresult = true;
                }
                else
                {
                    if (ISgray > LimitTH) bresult = true;
                }

                if (bresult)
                {
                    cr.Color = CogColorConstants.Green;
                    cogDS.InteractiveGraphics.Add(cr, "DetachInsp1", false);
                    
                }
                else
                {
                    cr.Color = CogColorConstants.Red;
                    cogDS.InteractiveGraphics.Add(cr, "DetachInsp1", false);                    
                }
                return bresult;
            }
            catch
            {
                return false;
            }
        }

        public bool firstRemoveInspection(double dX, double dY)
        {
            //L : -1000, 600  Thr : 100
            //R : -1000, 600  Thr : 100

            try
            {
                this.DeleteInteractiveGraphic(cogDS, "COF");
            }
            catch
            {
            }
            try
            {
                CogRectangle cr = new CogRectangle();
                double checkL = 0.5;
                cr.X = dX / CFG.Resolution;
                cr.Y = dY / CFG.Resolution;

                cr.Width = checkL / CFG.Resolution;
                cr.Height = checkL / CFG.Resolution;

                Cognex.VisionPro.ImageProcessing.CogHistogramTool crH = new Cognex.VisionPro.ImageProcessing.CogHistogramTool();
                crH.InputImage = cogDS.Image;
                crH.Region = cr;
                crH.Run();

                //double pixelcnt = (checkL / CFG.Resolution) * (checkL / CFG.Resolution) * 0.9;
                //  pixelcnt = pixelcnt * pixelcnt;

                int[] result = crH.Result.GetHistogram();
                int iCount = 0;
                try
                {
                    double pixelcnt = 0;
                    for (int i = 0; i < result.Length; i++) pixelcnt = pixelcnt + result[i];

                    pixelcnt = pixelcnt * 0.9;  // 90 % Check

                    int iLimit = 50;

                    for (int i = 0; i <= iLimit; i++)
                    {
                        iCount = iCount + result[i];
                    }



                    if (iCount > pixelcnt)
                    {
                        cr.Color = CogColorConstants.Yellow;
                        cogDS.InteractiveGraphics.Add(cr, "COF", false);
                        return true;
                    }
                    else
                    {
                        cr.Color = CogColorConstants.Red;
                        cogDS.InteractiveGraphics.Add(cr, "COF", false);
                        if (iLimit <= 0)
                        {
                            return true; // 미사용
                        }
                        return false;
                    }
                }
                catch
                { }
            }
            catch { }
            return true;
        }
        public struct sideResult
        {
            public double inCenterX;
            public double inCenterY;
            public double inRadius;
            public double outCenterX;
            public double outCenterY;
            public double outRadius;
            public double dist;
            public double distY; //bendpre SCF inspection y
            public double inoutdiffX;
            public double inoutdiffY;
        }
        public bool SideInspection(bool caliperDisp, ref sideResult result, bool reverse = false, bool caliperDisp1 = false)
        {
            //순서 안쪽원(파랑) -> 바깥쪽원(빨강)
            CogRectangle cogrectangle = new CogRectangle();
            CogFindCircleTool cogFindCircle = new CogFindCircleTool();
            CogFindLineTool findLine = new CogFindLineTool();
            CogGraphicCollection myRegions;
            ICogRecord myRec;
            //CogLineSegment myLine;
            CogCircularArc myArc;
            CogCaliperScorerPosition cogpos = new CogCaliperScorerPosition();
            cogpos.Enabled = true;

            result.inCenterX = 0;
            result.inCenterY = 0;
            result.inRadius = 0;
            result.outCenterX = 0;
            result.outCenterY = 0;
            result.outRadius = 0;

            string RunRecipe = CONST.RunRecipe.RecipeName.Trim(); // runrecipe 관련 수정.

            try
            {
                cogDS.InteractiveGraphics.Clear();
                cogDS.StaticGraphics.Clear();
            }
            catch
            { }

            string sPatternDir = Path.Combine(CONST.cVisionPath, CFG.Name.Trim(), CONST.stringRcp, RunRecipe.Trim(), "PMAlign", "Panel");

            string sPatternFile = "";

            if (!Directory.Exists(sPatternDir)) Directory.CreateDirectory(sPatternDir);

            string[] sFile = Directory.GetFiles(sPatternDir, "*.vpp");

            string[] Split = null;
            for (int i = 0; i < sFile.Length; i++)
            {
                if (sFile[i].IndexOf(".vpp") > 0)  // Pattern을 찾아서 ROI를 불러옴.
                {
                    sPatternFile = sFile[i];
                    Split = Path.GetFileName(sPatternFile).Split('_');

                    if (true) //SDV 감마
                    {
                        if (Split.Length > 8)
                        {
                            cogrectangle.SetXYWidthHeight(double.Parse(Split[7]), double.Parse(Split[8]), double.Parse(Split[9]), double.Parse(Split[10]));
                            //cogPMAlign.SearchRegion = cogrectangle;

                            cogrectangle.GraphicDOFEnable = CogRectangleDOFConstants.All;
                            cogrectangle.Interactive = true;
                            cogDS.InteractiveGraphics.Add(cogrectangle, "SearchRegion", false); // 검색 영역 표시 부분.

                            cogFindCircle.InputImage = (CogImage8Grey)cogDS.Image;
                            cogFindCircle.RunParams.NumCalipers = 30;
                            cogFindCircle.RunParams.CaliperProjectionLength = 10;

                            CogCircularArc arc = new CogCircularArc();
                            double cX = double.Parse(Split[7]) + double.Parse(Split[9]) / 2.0;
                            double cY = double.Parse(Split[8]) + double.Parse(Split[10]) / 2.0;
                            arc.CenterX = cX;
                            arc.CenterY = cY;
                            //좌표 메모리저장
                            searchDatas[0].PointX = cX;
                            searchDatas[0].PointY = cY;
                            searchDatas[0].ImgWidth = cogrectangle.Width * 2;
                            searchDatas[0].ImgHeight = cogrectangle.Height * 2;
                            //20.10.17 lkw
                            arc.Radius = double.Parse(Split[9]) / 4.0;   //-> ROI 설정을 넓게 해야 함.... 찾으려는 원을 벗어나도록

                            //체크 각도
                            if (!reverse)  // 좌측으로 R 생김
                            {
                                arc.AngleStart = 145 * Math.PI / 180.0;
                                arc.AngleSpan = 70 * Math.PI / 180.0;
                            }
                            else if (reverse) // 우측으로 R 생김
                            {
                                arc.AngleStart = -70 * Math.PI / 180.0;
                                arc.AngleSpan = 130 * Math.PI / 180.0;
                            }
                            cogFindCircle.RunParams.ExpectedCircularArc = arc;
                            cogFindCircle.RunParams.CaliperSearchLength = 180; //체크
                            cogFindCircle.RunParams.CaliperSearchDirection = CogFindCircleSearchDirectionConstants.Inward;

                            //   if (checkKind == sideInsp.outCheck)
                            //       cogFindCircle.RunParams.CaliperSearchDirection = CogFindCircleSearchDirectionConstants.Outward;
                            cogFindCircle.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DarkToLight;
                            cogFindCircle.RunParams.CaliperRunParams.ContrastThreshold = 5; //체크
                            cogFindCircle.RunParams.DecrementNumToIgnore = true;
                            cogFindCircle.RunParams.NumToIgnore = 10;
                            // cogFindCircle.RunParams.RadiusConstraint

                            //position 사용하고 싶을때
                            //cogFindCircle.RunParams.CaliperRunParams.SingleEdgeScorers.Clear();
                            //CogCaliperScorerPosition scorerPosition = new CogCaliperScorerPosition();
                            ////scorerPosition.GetXYParameters(out double x0, out double x1, out double xc, out double y0, out double y1);
                            ////scorerPosition.SetXYParameters(xc, x1, x0, y0, y1);
                            //scorerPosition.Enabled = true;
                            //cogFindCircle.RunParams.CaliperRunParams.SingleEdgeScorers.Add(scorerPosition);

                            cogFindCircle.CurrentRecordEnable = CogFindCircleCurrentRecordConstants.All;
                            myRec = cogFindCircle.CreateCurrentRecord();
                            myArc = (CogCircularArc)myRec.SubRecords["InputImage"].SubRecords["ExpectedShapeSegment"].Content;
                            myRegions = (CogGraphicCollection)myRec.SubRecords["InputImage"].SubRecords["CaliperRegions"].Content;
                            if (caliperDisp)
                            {
                                cogDS.InteractiveGraphics.Add(myArc, "ShapeSegmentRun", false);
                                foreach (ICogGraphic g in myRegions)
                                    cogDS.InteractiveGraphics.Add((ICogGraphicInteractive)g, "FindCircle", false);
                            }
                            cogFindCircle.Run();


                            double dminmaxX = 100000;
                            if (reverse) dminmaxX = 0;
                            for (i = 0; i < cogFindCircle.Results.Count; i++)
                            {
                                if (cogFindCircle.Results[i].Found && cogFindCircle.Results[i].Used)
                                {
                                    if (!reverse)
                                    {
                                        if (cogFindCircle.Results[i].X < dminmaxX) dminmaxX = cogFindCircle.Results[i].X;
                                    }
                                    else
                                    {
                                        if (cogFindCircle.Results[i].X > dminmaxX) dminmaxX = cogFindCircle.Results[i].X;
                                    }
                                }
                            }
                            bool b1 = false;
                            if (cogFindCircle.RunStatus.Result == CogToolResultConstants.Accept) b1 = true;

                            try
                            {
                                CogCircularArc ccArc = cogFindCircle.Results.GetCircularArc();
                                result.inCenterX = ccArc.CenterX;
                                result.inCenterY = ccArc.CenterY;
                                result.inRadius = ccArc.Radius * CFG.Resolution; //체크 resolution 곱하는게 맞는지..
                                ccArc.Color = CogColorConstants.Blue;
                                cogDS.InteractiveGraphics.Add(ccArc, "", false);

                                CogLineSegment lineX = new CogLineSegment();
                                lineX.StartX = ccArc.CenterX - 50;
                                lineX.StartY = ccArc.CenterY;
                                lineX.EndX = ccArc.CenterX + 50;
                                lineX.EndY = ccArc.CenterY;
                                lineX.Color = CogColorConstants.Blue;
                                cogDS.InteractiveGraphics.Add(lineX, "", false);

                                CogLineSegment lineY = new CogLineSegment();
                                lineY.StartX = ccArc.CenterX;
                                lineY.StartY = ccArc.CenterY - 50;
                                lineY.EndX = ccArc.CenterX;
                                lineY.EndY = ccArc.CenterY + 50;
                                lineY.Color = CogColorConstants.Blue;
                                cogDS.InteractiveGraphics.Add(lineY, "", false);
                            }
                            catch
                            {
                            }

                            cogFindCircle.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.LightToDark;
                            cogFindCircle.Run();

                            double dminmaxX2 = 100000;
                            if (reverse) dminmaxX2 = 0;
                            for (i = 0; i < cogFindCircle.Results.Count; i++)
                            {
                                if (cogFindCircle.Results[i].Found && cogFindCircle.Results[i].Used)
                                {
                                    if (!reverse)
                                    {
                                        if (cogFindCircle.Results[i].X < dminmaxX2) dminmaxX2 = cogFindCircle.Results[i].X;
                                    }
                                    else
                                    {
                                        if (cogFindCircle.Results[i].X > dminmaxX2) dminmaxX2 = cogFindCircle.Results[i].X;
                                    }
                                }
                            }
                            bool b2 = false;
                            if (cogFindCircle.RunStatus.Result == CogToolResultConstants.Accept) b2 = true;
                            try
                            {
                                CogCircularArc ccArc = cogFindCircle.Results.GetCircularArc();
                                result.outCenterX = ccArc.CenterX;
                                result.outCenterY = ccArc.CenterY;
                                result.outRadius = ccArc.Radius * CFG.Resolution; //체크 resolution 곱하는게 맞는지..
                                ccArc.Color = CogColorConstants.Orange;
                                cogDS.InteractiveGraphics.Add(ccArc, "", false);

                                CogLineSegment lineX = new CogLineSegment();
                                lineX.StartX = ccArc.CenterX - 50;
                                lineX.StartY = ccArc.CenterY;
                                lineX.EndX = ccArc.CenterX + 50;
                                lineX.EndY = ccArc.CenterY;
                                lineX.Color = CogColorConstants.Orange;
                                cogDS.InteractiveGraphics.Add(lineX, "", false);

                                CogLineSegment lineY = new CogLineSegment();
                                lineY.StartX = ccArc.CenterX;
                                lineY.StartY = ccArc.CenterY - 50;
                                lineY.EndX = ccArc.CenterX;
                                lineY.EndY = ccArc.CenterY + 50;
                                lineY.Color = CogColorConstants.Orange;
                                cogDS.InteractiveGraphics.Add(lineY, "", false);

                            }
                            catch
                            {
                            }

                            //if ((!reverse && dminmaxX < 100000 && dminmaxX2 < 100000) || (reverse && dminmaxX > 0 && dminmaxX2 > 0))
                            if (b1 && b2)
                            {
                                // 두께 계산
                                CogLineSegment cLine = new CogLineSegment();
                                double dY1 = double.Parse(Split[8]);
                                double dY2 = double.Parse(Split[8]) + double.Parse(Split[10]);
                                cLine.StartX = dminmaxX;
                                cLine.StartY = dY1;
                                cLine.EndX = dminmaxX;
                                cLine.EndY = dY2;
                                //cLine.SetFromStartXYEndXY(minX, dY1, minX, dY2);
                                cLine.Color = CogColorConstants.Red;
                                cogDS.InteractiveGraphics.Add(cLine, "", false);

                                CogLineSegment cLine2 = new CogLineSegment();
                                cLine2.StartX = dminmaxX2;
                                cLine2.StartY = dY1;
                                cLine2.EndX = dminmaxX2;
                                cLine2.EndY = dY2;

                                //cLine2.SetFromStartXYEndXY(minX2, dY1, minX2, dY2);
                                cLine2.Color = CogColorConstants.Red;
                                cogDS.InteractiveGraphics.Add(cLine2, "", false);

                                result.dist = Math.Abs(dminmaxX - dminmaxX2) * CFG.Resolution;
                                cogLabel.Color = CogColorConstants.Red;
                                cogLabel.SetXYText(dminmaxX, dY2 + 100, result.dist.ToString("0.000") + " mm");
                                cogDS.InteractiveGraphics.Add(cogLabel, "", false);
                                return true;
                            }

                        }
                    }
                    //else if (false) //SND AAM
                    //{
                    //    if (Split.Length > 8)
                    //    {
                    //        try
                    //        {
                    //            findLine.RunParams.ExpectedLineSegment.StartX = double.Parse(Split[7]);
                    //            findLine.RunParams.ExpectedLineSegment.StartY = double.Parse(Split[8]);
                    //            findLine.RunParams.ExpectedLineSegment.EndX = double.Parse(Split[7]) + double.Parse(Split[9]);
                    //            findLine.RunParams.ExpectedLineSegment.EndY = double.Parse(Split[8]) + double.Parse(Split[10]);

                    //            findLine.InputImage = (CogImage8Grey)cogDS.Image;
                    //            findLine.RunParams.NumCalipers = 7; //홀수로 할것
                    //            int ihalf = findLine.RunParams.NumCalipers / 2;
                    //            findLine.RunParams.CaliperProjectionLength = 5;
                    //            findLine.RunParams.CaliperSearchLength = 250;
                    //            findLine.RunParams.CaliperSearchDirection = -1.5708;
                    //            findLine.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.LightToDark; //안쪽
                    //            findLine.RunParams.CaliperRunParams.SingleEdgeScorers.Clear();
                    //            findLine.RunParams.CaliperRunParams.SingleEdgeScorers.Add(cogpos);
                    //            findLine.Run();
                    //            if (caliperDisp1)
                    //            {
                    //                CogGraphicCollection myRegions1;
                    //                ICogRecord myRec1;
                    //                CogLineSegment myLine1;
                    //                myRec1 = findLine.CreateCurrentRecord();
                    //                myLine1 = (CogLineSegment)myRec1.SubRecords["InputImage"].SubRecords["ExpectedShapeSegment"].Content;
                    //                myRegions1 = (CogGraphicCollection)myRec1.SubRecords["InputImage"].SubRecords["CaliperRegions"].Content;
                    //                cogDS.StaticGraphics.Add(myLine1, "ShapeSegmentRun1");
                    //                foreach (ICogGraphic g in myRegions1)
                    //                    cogDS.StaticGraphics.Add((ICogGraphicInteractive)g, "FindCircle1");
                    //            }
                    //            if (findLine.RunStatus.Result == CogToolResultConstants.Accept || findLine.RunStatus.Result == CogToolResultConstants.Warning)
                    //            {
                    //                CogFindLineResults outresult = new CogFindLineResults(findLine.Results);
                    //                CogLineSegment temp = findLine.Results.GetLineSegment();
                    //                findLine.RunParams.ExpectedLineSegment = temp;
                    //                findLine.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DarkToLight; //바깥쪽
                    //                findLine.Run();
                    //                if (caliperDisp1)
                    //                {
                    //                    CogGraphicCollection myRegions1;
                    //                    ICogRecord myRec1;
                    //                    CogLineSegment myLine1;
                    //                    myRec1 = findLine.CreateCurrentRecord();
                    //                    myLine1 = (CogLineSegment)myRec1.SubRecords["InputImage"].SubRecords["ExpectedShapeSegment"].Content;
                    //                    myRegions1 = (CogGraphicCollection)myRec1.SubRecords["InputImage"].SubRecords["CaliperRegions"].Content;
                    //                    cogDS.StaticGraphics.Add(myLine1, "ShapeSegmentRun1");
                    //                    foreach (ICogGraphic g in myRegions1)
                    //                        cogDS.StaticGraphics.Add((ICogGraphicInteractive)g, "FindCircle1");
                    //                }
                    //                CogFindLineResults inresult = new CogFindLineResults(findLine.Results);
                    //                CogDistancePointPointTool pointTool = new CogDistancePointPointTool();
                    //                pointTool.InputImage = (CogImage8Grey)cogDS.Image;
                    //                for (int j = 0; j < ihalf; j++)
                    //                {
                    //                    if (inresult[ihalf + j].Found && outresult[ihalf + j].Found)
                    //                    {
                    //                        pointTool.StartX = inresult[ihalf + j].X;
                    //                        pointTool.StartY = inresult[ihalf + j].Y;
                    //                        pointTool.EndX = outresult[ihalf + j].X;
                    //                        pointTool.EndY = outresult[ihalf + j].Y;
                    //                        pointTool.Run();

                    //                        break;
                    //                    }
                    //                    if (inresult[ihalf - j].Found && outresult[ihalf - j].Found)
                    //                    {
                    //                        break;
                    //                    }
                    //                }
                    //                if (pointTool.RunStatus.Result == CogToolResultConstants.Accept || pointTool.RunStatus.Result == CogToolResultConstants.Warning)
                    //                {
                    //                    result.dist = pointTool.Distance * CFG.Resolution;
                    //                    CogLineSegment cLine1 = new CogLineSegment();
                    //                    CogLineSegment cLine2 = new CogLineSegment();
                    //                    cLine1.SetStartLengthRotation(pointTool.StartX, pointTool.StartY, 30, pointTool.Angle + 1.5708);
                    //                    cLine2.SetStartLengthRotation(pointTool.StartX, pointTool.StartY, 30, pointTool.Angle - 1.5708);
                    //                    CogLineSegment cLine3 = new CogLineSegment();
                    //                    CogLineSegment cLine4 = new CogLineSegment();
                    //                    cLine3.SetStartLengthRotation(pointTool.EndX, pointTool.EndY, 30, pointTool.Angle + 1.5708);
                    //                    cLine4.SetStartLengthRotation(pointTool.EndX, pointTool.EndY, 30, pointTool.Angle - 1.5708);

                    //                    cLine1.Color = CogColorConstants.Red;
                    //                    cLine2.Color = CogColorConstants.Red;
                    //                    cLine3.Color = CogColorConstants.Red;
                    //                    cLine4.Color = CogColorConstants.Red;

                    //                    cogDS.InteractiveGraphics.Add(cLine1, "", false);
                    //                    cogDS.InteractiveGraphics.Add(cLine2, "", false);
                    //                    cogDS.InteractiveGraphics.Add(cLine3, "", false);
                    //                    cogDS.InteractiveGraphics.Add(cLine4, "", false);
                    //                    return true;
                    //                }
                    //            }
                    //        }
                    //        catch  { }
                    //    }
                    //}
                }
            }
            return false;
        }
        public bool TSideInspection(bool caliperDisp, ref sideResult result, bool reverse = false, bool caliperDisp1 = false)
        {
            //순서 안쪽원(파랑) -> 바깥쪽원(빨강)
            CogRectangle cogrectangle = new CogRectangle();
            CogFindCircleTool cogFindCircle = new CogFindCircleTool();
            CogFindLineTool findLine = new CogFindLineTool();
            CogGraphicCollection myRegions;
            ICogRecord myRec;
            //CogLineSegment myLine;
            CogCircularArc myArc;
            CogCaliperScorerPosition cogpos = new CogCaliperScorerPosition();
            cogpos.Enabled = true;

            result.inCenterX = 0;
            result.inCenterY = 0;
            result.inRadius = 0;
            result.outCenterX = 0;
            result.outCenterY = 0;
            result.outRadius = 0;

            string RunRecipe = CONST.RunRecipe.RecipeName.Trim(); // runrecipe 관련 수정.

            try
            {
                cogDS.InteractiveGraphics.Clear();
                cogDS.StaticGraphics.Clear();
            }
            catch
            { }

            string sPatternDir = Path.Combine(CONST.cVisionPath, CFG.Name.Trim(), CONST.stringRcp, RunRecipe.Trim(), "PMAlign", "Panel");

            string sPatternFile = "";

            if (!Directory.Exists(sPatternDir)) Directory.CreateDirectory(sPatternDir);

            string[] sFile = Directory.GetFiles(sPatternDir, "*.vpp");

            string[] Split = null;
            for (int i = 0; i < sFile.Length; i++)
            {
                if (sFile[i].IndexOf(".vpp") > 0)  // Pattern을 찾아서 ROI를 불러옴.
                {
                    sPatternFile = sFile[i];
                    Split = Path.GetFileName(sPatternFile).Split('_');

                    if (true) //SDV 감마
                    {
                        if (Split.Length > 8)
                        {
                            cogrectangle.SetXYWidthHeight(double.Parse(Split[7]), double.Parse(Split[8]), double.Parse(Split[9]), double.Parse(Split[10]));
                            //cogPMAlign.SearchRegion = cogrectangle;

                            cogrectangle.GraphicDOFEnable = CogRectangleDOFConstants.All;
                            cogrectangle.Interactive = true;
                            cogDS.InteractiveGraphics.Add(cogrectangle, "SearchRegion", false); // 검색 영역 표시 부분.

                            cogFindCircle.InputImage = (CogImage8Grey)cogDS.Image;
                            cogFindCircle.RunParams.NumCalipers = 30;
                            cogFindCircle.RunParams.CaliperProjectionLength = 10;

                            CogCircularArc arc = new CogCircularArc();
                            double cX = double.Parse(Split[7]) + double.Parse(Split[9]) / 2.0;
                            double cY = double.Parse(Split[8]) + double.Parse(Split[10]) / 2.0;
                            arc.CenterX = cX;
                            arc.CenterY = cY;

                            //20.10.17 lkw
                            arc.Radius = double.Parse(Split[9]) / 4.0;   //-> ROI 설정을 넓게 해야 함.... 찾으려는 원을 벗어나도록

                            //체크 각도
                            if (!reverse)  // 좌측으로 R 생김
                            {
                                arc.AngleStart = 145 * Math.PI / 180.0;
                                arc.AngleSpan = 70 * Math.PI / 180.0;
                            }
                            else if (reverse) // 우측으로 R 생김
                            {
                                arc.AngleStart = -70 * Math.PI / 180.0;
                                arc.AngleSpan = 130 * Math.PI / 180.0;
                            }
                            cogFindCircle.RunParams.ExpectedCircularArc = arc;
                            cogFindCircle.RunParams.CaliperSearchLength = 180; //체크
                            cogFindCircle.RunParams.CaliperSearchDirection = CogFindCircleSearchDirectionConstants.Inward;

                            //   if (checkKind == sideInsp.outCheck)
                            //       cogFindCircle.RunParams.CaliperSearchDirection = CogFindCircleSearchDirectionConstants.Outward;
                            cogFindCircle.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DarkToLight;
                            cogFindCircle.RunParams.CaliperRunParams.ContrastThreshold = 5; //체크
                            cogFindCircle.RunParams.DecrementNumToIgnore = true;
                            cogFindCircle.RunParams.NumToIgnore = 10;
                            // cogFindCircle.RunParams.RadiusConstraint
                            //position 사용하고 싶을때
                            //cogFindCircle.RunParams.CaliperRunParams.SingleEdgeScorers.Clear();
                            //CogCaliperScorerPosition scorerPosition = new CogCaliperScorerPosition();
                            ////scorerPosition.GetXYParameters(out double x0, out double x1, out double xc, out double y0, out double y1);
                            ////scorerPosition.SetXYParameters(xc, x1, x0, y0, y1);
                            //scorerPosition.Enabled = true;
                            //cogFindCircle.RunParams.CaliperRunParams.SingleEdgeScorers.Add(scorerPosition);

                            cogFindCircle.CurrentRecordEnable = CogFindCircleCurrentRecordConstants.All;
                            myRec = cogFindCircle.CreateCurrentRecord();
                            myArc = (CogCircularArc)myRec.SubRecords["InputImage"].SubRecords["ExpectedShapeSegment"].Content;
                            myRegions = (CogGraphicCollection)myRec.SubRecords["InputImage"].SubRecords["CaliperRegions"].Content;
                            if (caliperDisp)
                            {
                                cogDS.InteractiveGraphics.Add(myArc, "ShapeSegmentRun", false);
                                foreach (ICogGraphic g in myRegions)
                                    cogDS.InteractiveGraphics.Add((ICogGraphicInteractive)g, "FindCircle", false);
                            }
                            cogFindCircle.Run();


                            double dminmaxX = 100000;
                            if (reverse) dminmaxX = 0;
                            for (i = 0; i < cogFindCircle.Results.Count; i++)
                            {
                                if (cogFindCircle.Results[i].Found && cogFindCircle.Results[i].Used)
                                {
                                    if (!reverse)
                                    {
                                        if (cogFindCircle.Results[i].X < dminmaxX) dminmaxX = cogFindCircle.Results[i].X;
                                    }
                                    else
                                    {
                                        if (cogFindCircle.Results[i].X > dminmaxX) dminmaxX = cogFindCircle.Results[i].X;
                                    }
                                }
                            }
                            bool b1 = false;
                            if (cogFindCircle.RunStatus.Result == CogToolResultConstants.Accept) b1 = true;

                            try
                            {
                                CogCircularArc ccArc = cogFindCircle.Results.GetCircularArc();
                                result.inCenterX = ccArc.CenterX;
                                result.inCenterY = ccArc.CenterY;
                                result.inRadius = ccArc.Radius * CFG.Resolution; //체크 resolution 곱하는게 맞는지..
                                ccArc.Color = CogColorConstants.Blue;
                                cogDS.InteractiveGraphics.Add(ccArc, "", false);

                                CogLineSegment lineX = new CogLineSegment();
                                lineX.StartX = ccArc.CenterX - 50;
                                lineX.StartY = ccArc.CenterY;
                                lineX.EndX = ccArc.CenterX + 50;
                                lineX.EndY = ccArc.CenterY;
                                lineX.Color = CogColorConstants.Blue;
                                cogDS.InteractiveGraphics.Add(lineX, "", false);

                                CogLineSegment lineY = new CogLineSegment();
                                lineY.StartX = ccArc.CenterX;
                                lineY.StartY = ccArc.CenterY - 50;
                                lineY.EndX = ccArc.CenterX;
                                lineY.EndY = ccArc.CenterY + 50;
                                lineY.Color = CogColorConstants.Blue;
                                cogDS.InteractiveGraphics.Add(lineY, "", false);
                            }
                            catch
                            {
                            }

                            cogFindCircle.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.LightToDark;
                            cogFindCircle.Run();

                            double dminmaxX2 = 100000;
                            if (reverse) dminmaxX2 = 0;
                            for (i = 0; i < cogFindCircle.Results.Count; i++)
                            {
                                if (cogFindCircle.Results[i].Found && cogFindCircle.Results[i].Used)
                                {
                                    if (!reverse)
                                    {
                                        if (cogFindCircle.Results[i].X < dminmaxX2) dminmaxX2 = cogFindCircle.Results[i].X;
                                    }
                                    else
                                    {
                                        if (cogFindCircle.Results[i].X > dminmaxX2) dminmaxX2 = cogFindCircle.Results[i].X;
                                    }
                                }
                            }
                            bool b2 = false;
                            if (cogFindCircle.RunStatus.Result == CogToolResultConstants.Accept) b2 = true;
                            try
                            {
                                CogCircularArc ccArc = cogFindCircle.Results.GetCircularArc();
                                result.outCenterX = ccArc.CenterX;
                                result.outCenterY = ccArc.CenterY;
                                result.outRadius = ccArc.Radius * CFG.Resolution; //체크 resolution 곱하는게 맞는지..
                                ccArc.Color = CogColorConstants.Orange;
                                cogDS.InteractiveGraphics.Add(ccArc, "", false);

                                CogLineSegment lineX = new CogLineSegment();
                                lineX.StartX = ccArc.CenterX - 50;
                                lineX.StartY = ccArc.CenterY;
                                lineX.EndX = ccArc.CenterX + 50;
                                lineX.EndY = ccArc.CenterY;
                                lineX.Color = CogColorConstants.Orange;
                                cogDS.InteractiveGraphics.Add(lineX, "", false);

                                CogLineSegment lineY = new CogLineSegment();
                                lineY.StartX = ccArc.CenterX;
                                lineY.StartY = ccArc.CenterY - 50;
                                lineY.EndX = ccArc.CenterX;
                                lineY.EndY = ccArc.CenterY + 50;
                                lineY.Color = CogColorConstants.Orange;
                                cogDS.InteractiveGraphics.Add(lineY, "", false);

                            }
                            catch
                            {
                            }

                            //if ((!reverse && dminmaxX < 100000 && dminmaxX2 < 100000) || (reverse && dminmaxX > 0 && dminmaxX2 > 0))
                            if (b1 && b2)
                            {
                                // 두께 계산
                                CogLineSegment cLine = new CogLineSegment();
                                double dY1 = double.Parse(Split[8]);
                                double dY2 = double.Parse(Split[8]) + double.Parse(Split[10]);
                                cLine.StartX = dminmaxX;
                                cLine.StartY = dY1;
                                cLine.EndX = dminmaxX;
                                cLine.EndY = dY2;
                                //cLine.SetFromStartXYEndXY(minX, dY1, minX, dY2);
                                cLine.Color = CogColorConstants.Red;
                                cogDS.InteractiveGraphics.Add(cLine, "", false);

                                CogLineSegment cLine2 = new CogLineSegment();
                                cLine2.StartX = dminmaxX2;
                                cLine2.StartY = dY1;
                                cLine2.EndX = dminmaxX2;
                                cLine2.EndY = dY2;

                                //cLine2.SetFromStartXYEndXY(minX2, dY1, minX2, dY2);
                                cLine2.Color = CogColorConstants.Red;
                                cogDS.InteractiveGraphics.Add(cLine2, "", false);

                                result.dist = Math.Abs(dminmaxX - dminmaxX2) * CFG.Resolution;
                                cogLabel.Color = CogColorConstants.Red;
                                cogLabel.SetXYText(dminmaxX, dY2 + 100, result.dist.ToString("0.000") + " mm");
                                cogDS.InteractiveGraphics.Add(cogLabel, "", false);
                                return true;
                            }

                        }
                    }
                    //else if (false) //SDN AAM
                    //{
                    //    if (Split.Length > 8)
                    //    {
                    //        try
                    //        {
                    //            findLine.RunParams.ExpectedLineSegment.StartX = double.Parse(Split[7]);
                    //            findLine.RunParams.ExpectedLineSegment.StartY = double.Parse(Split[8]);
                    //            findLine.RunParams.ExpectedLineSegment.EndX = double.Parse(Split[7]) + double.Parse(Split[9]);
                    //            findLine.RunParams.ExpectedLineSegment.EndY = double.Parse(Split[8]) + double.Parse(Split[10]);

                    //            findLine.InputImage = (CogImage8Grey)cogDS.Image;
                    //            findLine.RunParams.NumCalipers = 7; //홀수로 할것
                    //            int ihalf = findLine.RunParams.NumCalipers / 2;
                    //            findLine.RunParams.CaliperProjectionLength = 5;
                    //            findLine.RunParams.CaliperSearchLength = 250;
                    //            findLine.RunParams.CaliperSearchDirection = -1.5708;
                    //            findLine.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.LightToDark; //안쪽
                    //            findLine.RunParams.CaliperRunParams.SingleEdgeScorers.Clear();
                    //            findLine.RunParams.CaliperRunParams.SingleEdgeScorers.Add(cogpos);
                    //            findLine.Run();
                    //            if (caliperDisp1)
                    //            {
                    //                CogGraphicCollection myRegions1;
                    //                ICogRecord myRec1;
                    //                CogLineSegment myLine1;
                    //                myRec1 = findLine.CreateCurrentRecord();
                    //                myLine1 = (CogLineSegment)myRec1.SubRecords["InputImage"].SubRecords["ExpectedShapeSegment"].Content;
                    //                myRegions1 = (CogGraphicCollection)myRec1.SubRecords["InputImage"].SubRecords["CaliperRegions"].Content;
                    //                cogDS.StaticGraphics.Add(myLine1, "ShapeSegmentRun1");
                    //                foreach (ICogGraphic g in myRegions1)
                    //                    cogDS.StaticGraphics.Add((ICogGraphicInteractive)g, "FindCircle1");
                    //            }
                    //            if (findLine.RunStatus.Result == CogToolResultConstants.Accept || findLine.RunStatus.Result == CogToolResultConstants.Warning)
                    //            {
                    //                CogFindLineResults outresult = new CogFindLineResults(findLine.Results);
                    //                CogLineSegment temp = findLine.Results.GetLineSegment();
                    //                findLine.RunParams.ExpectedLineSegment = temp;
                    //                findLine.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DarkToLight; //바깥쪽
                    //                findLine.Run();
                    //                if (caliperDisp1)
                    //                {
                    //                    CogGraphicCollection myRegions1;
                    //                    ICogRecord myRec1;
                    //                    CogLineSegment myLine1;
                    //                    myRec1 = findLine.CreateCurrentRecord();
                    //                    myLine1 = (CogLineSegment)myRec1.SubRecords["InputImage"].SubRecords["ExpectedShapeSegment"].Content;
                    //                    myRegions1 = (CogGraphicCollection)myRec1.SubRecords["InputImage"].SubRecords["CaliperRegions"].Content;
                    //                    cogDS.StaticGraphics.Add(myLine1, "ShapeSegmentRun1");
                    //                    foreach (ICogGraphic g in myRegions1)
                    //                        cogDS.StaticGraphics.Add((ICogGraphicInteractive)g, "FindCircle1");
                    //                }
                    //                CogFindLineResults inresult = new CogFindLineResults(findLine.Results);
                    //                CogDistancePointPointTool pointTool = new CogDistancePointPointTool();
                    //                pointTool.InputImage = (CogImage8Grey)cogDS.Image;
                    //                for (int j = 0; j < ihalf; j++)
                    //                {
                    //                    if (inresult[ihalf + j].Found && outresult[ihalf + j].Found)
                    //                    {
                    //                        pointTool.StartX = inresult[ihalf + j].X;
                    //                        pointTool.StartY = inresult[ihalf + j].Y;
                    //                        pointTool.EndX = outresult[ihalf + j].X;
                    //                        pointTool.EndY = outresult[ihalf + j].Y;
                    //                        pointTool.Run();

                    //                        break;
                    //                    }
                    //                    if (inresult[ihalf - j].Found && outresult[ihalf - j].Found)
                    //                    {
                    //                        break;
                    //                    }
                    //                }
                    //                if (pointTool.RunStatus.Result == CogToolResultConstants.Accept || pointTool.RunStatus.Result == CogToolResultConstants.Warning)
                    //                {
                    //                    result.dist = pointTool.Distance * CFG.Resolution;
                    //                    CogLineSegment cLine1 = new CogLineSegment();
                    //                    CogLineSegment cLine2 = new CogLineSegment();
                    //                    cLine1.SetStartLengthRotation(pointTool.StartX, pointTool.StartY, 30, pointTool.Angle + 1.5708);
                    //                    cLine2.SetStartLengthRotation(pointTool.StartX, pointTool.StartY, 30, pointTool.Angle - 1.5708);
                    //                    CogLineSegment cLine3 = new CogLineSegment();
                    //                    CogLineSegment cLine4 = new CogLineSegment();
                    //                    cLine3.SetStartLengthRotation(pointTool.EndX, pointTool.EndY, 30, pointTool.Angle + 1.5708);
                    //                    cLine4.SetStartLengthRotation(pointTool.EndX, pointTool.EndY, 30, pointTool.Angle - 1.5708);

                    //                    cLine1.Color = CogColorConstants.Red;
                    //                    cLine2.Color = CogColorConstants.Red;
                    //                    cLine3.Color = CogColorConstants.Red;
                    //                    cLine4.Color = CogColorConstants.Red;

                    //                    cogDS.InteractiveGraphics.Add(cLine1, "", false);
                    //                    cogDS.InteractiveGraphics.Add(cLine2, "", false);
                    //                    cogDS.InteractiveGraphics.Add(cLine3, "", false);
                    //                    cogDS.InteractiveGraphics.Add(cLine4, "", false);
                    //                    return true;
                    //                }
                    //            }
                    //        }
                    //        catch  { }
                    //    }
                    //}
                }
            }
            return false;
        }
        public void BendingSearchDisplay(string skind, double dMarkx, double dMarky, double dRefx, double dRefy)
        {
            try
            {
                cogDS.InteractiveGraphics.Remove(skind);
            }
            catch { }
            CogLineSegment line1_1 = new CogLineSegment();
            CogLineSegment line1_2 = new CogLineSegment();
            CogLineSegment line2_1 = new CogLineSegment();
            CogLineSegment line2_2 = new CogLineSegment();
            CogRectangleAffine rectangle1 = new CogRectangleAffine();
            CogRectangleAffine rectangle2 = new CogRectangleAffine();

            if (dMarkx != 0 && dMarky != 0)
            {
                line1_1.StartX = dMarkx - 100;
                line1_1.StartY = dMarky;
                line1_1.EndX = dMarkx + 100;
                line1_1.EndY = dMarky;
                line1_1.Color = CogColorConstants.Yellow;
                line1_1.LineWidthInScreenPixels = 1;
                //cogDS.InteractiveGraphics.Add(line1_1, skind, false);
                line1_2.StartX = dMarkx;
                line1_2.StartY = dMarky - 100;
                line1_2.EndX = dMarkx;
                line1_2.EndY = dMarky + 100;
                line1_2.Color = CogColorConstants.Yellow;
                line1_2.LineWidthInScreenPixels = 1;
                //cogDS.InteractiveGraphics.Add(line1_2, skind, false);
                rectangle1.CenterX = dMarkx;
                rectangle1.CenterY = dMarky;
                rectangle1.Color = CogColorConstants.Green;
                rectangle1.LineWidthInScreenPixels = 1;
                //cogDS.InteractiveGraphics.Add(rectangle1, skind, false);


                string strData = "";
                strData = "Object X = " + dMarkx.ToString("0.000") + "Object Y = " + dMarky.ToString("0.000");
                WriteCogLabel(strData, font6Bold, CogColorConstants.Red, 1500, 70, true, "Ref");
                //csh 20170601
            }

            if (dRefx != 0 && dRefy != 0)
            {
                line2_1.StartX = dRefx - 50;
                line2_1.StartY = dRefy;
                line2_1.EndX = dRefx + 50;
                line2_1.EndY = dRefy;
                line2_1.Color = CogColorConstants.Yellow;
                line2_1.LineWidthInScreenPixels = 1;
                //cogDS.InteractiveGraphics.Add(line2_1, skind, false);
                line2_2.StartX = dRefx;
                line2_2.StartY = dRefy - 50;
                line2_2.EndX = dRefx;
                line2_2.EndY = dRefy + 50;
                line2_2.Color = CogColorConstants.Yellow;
                line2_2.LineWidthInScreenPixels = 1;
                //cogDS.InteractiveGraphics.Add(line2_2, skind, false);
                rectangle2.CenterX = dRefx;
                rectangle2.CenterY = dRefy;
                rectangle2.Color = CogColorConstants.Green;
                rectangle2.LineWidthInScreenPixels = 1;
                //cogDS.InteractiveGraphics.Add(rectangle2, skind, false);

                //csh 20170601
                string strData = "";

                strData = "Target X = " + dRefx.ToString("0.000") + "Target Y = " + dRefy.ToString("0.000");
                WriteCogLabel(strData, font6Bold, CogColorConstants.Red, 1500, 10, true, "Ref");

            }
        }


        public bool CreateLine(double dDataX, double dDataY, double dDataR, ref CogLine cogBaseLine, bool bgraphic = false)
        {
            try
            {
                CogLine LineDraw = new CogLine();

                LineDraw.X = dDataX;
                LineDraw.Y = dDataY;
                LineDraw.Rotation = dDataR;
                LineDraw.Color = CogColorConstants.Purple;
                if (bgraphic) cogDS.InteractiveGraphics.Add(LineDraw, "Line", false);

                cogBaseLine = LineDraw.Copy(CogCopyShapeConstants.All);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool PointToLine(double dDataX, double dDataY, CogLine cogBaseLine, ref double dDataLineX, ref double dDataLineY, ref double dDistanceY, bool bgraphic = false)
        {
            try
            {
                CogLine LineDraw = new CogLine();

                CogDistancePointLineTool cogPointToLine = new CogDistancePointLineTool();
                cogPointToLine.InputImage = cogDS.Image;
                cogPointToLine.X = dDataX;
                cogPointToLine.Y = dDataY;
                cogPointToLine.Line = cogBaseLine;
                cogPointToLine.Run();

                dDataLineX = cogPointToLine.LineX;
                dDataLineY = cogPointToLine.LineY;

                dDistanceY = cogPointToLine.Distance;

                LineDraw.SetFromStartXYEndXY(dDataX, dDataY, dDataLineX, dDataLineY);
                LineDraw.Color = CogColorConstants.Purple;
                if (bgraphic) cogDS.InteractiveGraphics.Add(LineDraw, "Point", false);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool PointToPoint(double dDataStartX, double dDataStartY, double dDataEndX, double dDataEndY, ref double DistanceX)
        {
            try
            {
                CogLine LineDraw = new CogLine();

                CogDistancePointPointTool cogPointToPoint = new CogDistancePointPointTool();
                cogPointToPoint.InputImage = cogDS.Image;
                cogPointToPoint.StartX = dDataStartX;
                cogPointToPoint.StartY = dDataStartY;
                cogPointToPoint.EndX = dDataEndX;
                cogPointToPoint.EndY = dDataEndY;
                cogPointToPoint.Run();
                DistanceX = cogPointToPoint.Distance;

                LineDraw.SetFromStartXYEndXY(dDataStartX, dDataStartY, dDataEndX, dDataEndY);
                LineDraw.Color = CogColorConstants.Orange;
                cogDS.InteractiveGraphics.Add(LineDraw, "Line", false);

                return true;
            }
            catch
            {
                return false;
            }
        }

        //csh 20170612
        public void ShowStopWatchTime(string str)
        {
            /*cogLabel.Color = CogColorConstants.Orange;
            cogLabel.Font = font6Bold;
            cogLabel.SetXYText(100, 600, str + "ms");
            cogDS.InteractiveGraphics.Add(cogLabel, "RecogTIme", false);*/

            WriteCogLabel(str + "ms", font6Bold, CogColorConstants.Orange, 100, 600, false, "RecogTime");
        }
        //csh 20170612

        //KSJ 20170613 Inspection Data Grid
        public void InsepctionDataGrid(string strTemp, double dSetX, double dSetY)
        {
            /*Font font;
            
            font = new Font("Arial", 12, FontStyle.Bold);

            cogLabel.SetXYText(dSetX, dSetY - 150, strTemp);
            cogLabel.Color = CogColorConstants.Orange;
            cogLabel.Font = font;
            cogDS.InteractiveGraphics.Add(cogLabel, "InsepctionData", false);*/

            WriteCogLabel(strTemp, font12Bold, CogColorConstants.Orange, dSetX, dSetY - 150, false, "InspectionData");
        }

        public void GridImageSave()
        {
            string sFileName;

            string sDirectoryBase = Path.Combine(CONST.cImagePath, CFG.Name);
            string sDirectoryName = Path.Combine(sDirectoryBase, DateTime.Now.ToString("MM-dd-HH"));

            sDirectoryName = Path.Combine(sDirectoryName, "OK");

            sDirectoryName = Path.Combine(sDirectoryName, "Grid");

            //KSJ 20170713 Image Name add MCR ID
            //string sMCRID = MCRIDReturn();
            string sMCRID = PanelID;

            //sFileName = sDirectoryName + "/" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "-" + DateTime.Now.Millisecond.ToString("000") + " - " + sMCRID;

            sFileName = sDirectoryName + "/" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + " - " + sMCRID;

            if (!Directory.Exists(sDirectoryName))
                Directory.CreateDirectory(sDirectoryName);

            Bitmap bitmap = new Bitmap(cogDS.CreateContentBitmap(Cognex.VisionPro.Display.CogDisplayContentBitmapConstants.Display));

            long lQuality = 90L;
            new SaveImage()
            {
                format = System.Drawing.Imaging.ImageFormat.Jpeg,
                Quality = lQuality,
            }.Save(bitmap, sFileName);

            DeleteExpireVisionImage(sDirectoryBase, 24 * CFG.ImgAutoDelDay);
        }

        public void GridImageSave(string strName)
        {
            string sFileName;

            string sDirectoryBase = Path.Combine(CONST.cImagePath, CFG.Name);
            string sDirectoryName = Path.Combine(sDirectoryBase, DateTime.Now.ToString("MM-dd-HH"));

            sDirectoryName = Path.Combine(sDirectoryName, "OK");

            sDirectoryName = Path.Combine(sDirectoryName, "Grid");

            sFileName = sDirectoryName + "/" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "-" + strName;

            if (!Directory.Exists(sDirectoryName))
                Directory.CreateDirectory(sDirectoryName);

            Bitmap bitmap = new Bitmap(cogDS.CreateContentBitmap(CogDisplayContentBitmapConstants.Custom));

            long lQuality = 90L;
            new SaveImage()
            {
                format = System.Drawing.Imaging.ImageFormat.Jpeg,
                Quality = lQuality,
            }.Save(bitmap, sFileName);

            DeleteExpireVisionImage(sDirectoryBase, 24 * CFG.ImgAutoDelDay);
        }
        public void JpegFastImageSave() //pcy190724
        {
            //CONST.eImageSaveType ImageSaveType = CONST.eImageSaveType.Display;
            string sFileName = "";
            //string sFileName2 = "";

            string sDirectoryBase = Path.Combine(CONST.cImagePath, CFG.Name);

            string sDirectoryName = Path.Combine(sDirectoryBase, DateTime.Now.ToString("yyyy-MM-dd"));
            //string sDirectoryName2 = Path.Combine(sDirectoryBase, DateTime.Now.ToString("yyyy-MM-dd"));

            //KSJ 20170713 Image Name add MCR ID
            //string sMCRID = MCRIDReturn();
            string sMCRID = PanelID;

            sDirectoryName = Path.Combine(sDirectoryName, "Display");

            //if (bOK)
            //{
            //    if (CONST.eImageSaveType.All == ImageSaveType)
            //    {
            //        sDirectoryName = Path.Combine(sDirectoryName, "OriginalOK");
            //        sDirectoryName2 = Path.Combine(sDirectoryName2, "DisplayOK");
            //    }
            //    else if (CONST.eImageSaveType.Original == ImageSaveType) sDirectoryName = Path.Combine(sDirectoryName, "OriginalOK");
            //    else if (CONST.eImageSaveType.Display == ImageSaveType) sDirectoryName = Path.Combine(sDirectoryName, "DisplayOK");
            //}
            //else
            //{
            //    if (CONST.eImageSaveType.All == ImageSaveType)
            //    {
            //        sDirectoryName = Path.Combine(sDirectoryName, "OriginalNG");
            //        sDirectoryName2 = Path.Combine(sDirectoryName2, "DisplayNG");
            //    }
            //    else if (CONST.eImageSaveType.Original == ImageSaveType) sDirectoryName = Path.Combine(sDirectoryName, "OriginalNG");
            //    else if (CONST.eImageSaveType.Display == ImageSaveType) sDirectoryName = Path.Combine(sDirectoryName, "DisplayNG");
            //}

            //sFileName = sDirectoryName + "/" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "-" + DateTime.Now.Millisecond.ToString("000") + " - " + sMCRID;

            sFileName = sDirectoryName + "/" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + " - " + sMCRID;
            //if (CONST.eImageSaveType.All == ImageSaveType) sFileName2 = sDirectoryName2 + "/" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + " - " + sMCRID;

            if (!Directory.Exists(sDirectoryName))
                Directory.CreateDirectory(sDirectoryName);

            //if (CONST.eImageSaveType.All == ImageSaveType)
            //{
            //    if (!Directory.Exists(sDirectoryName2))
            //        Directory.CreateDirectory(sDirectoryName2);
            //}

            //CogImage8Grey cTempImage = new CogImage8Grey();
            //Bitmap cTempBitmap;

            //if (CONST.eImageSaveType.All == ImageSaveType)
            //{
            //    cTempImage = (CogImage8Grey)cogDS.Image;
            //    cTempBitmap = new Bitmap(cogDS.CreateContentBitmap(CogDisplayContentBitmapConstants.Display), new Size(cogDS.Image.Width, cogDS.Image.Height));

            //    new SaveImage()
            //    {
            //        format = ImageFormat.Bmp
            //    }.Save(cTempImage.ToBitmap(), sFileName);

            //    new SaveImage()
            //    {
            //        format = ImageFormat.Bmp
            //    }.Save(cTempBitmap, sFileName2);
            //}
            //else if (CONST.eImageSaveType.Original == ImageSaveType)
            //{
            //    cTempImage = (CogImage8Grey)cogDS.Image;

            //    new SaveImage()
            //    {
            //        format = ImageFormat.Bmp
            //    }.Save(cTempImage.ToBitmap(), sFileName);
            //}
            //cogDS.CreateContentBitmap(CogDisplayContentBitmapConstants.Display, null, 2000).Save(sFileName, ImageFormat.Jpeg);
            cogDS.CreateContentBitmap(CogDisplayContentBitmapConstants.Display).Save(sFileName, ImageFormat.Jpeg);
            //else if (CONST.eImageSaveType.Display == ImageSaveType)
            //{
            //    //cTempBitmap = new Bitmap(cogDS.CreateContentBitmap(CogDisplayContentBitmapConstants.Display), new Size(cogDS.Image.Width, cogDS.Image.Height));
            //    cogDS.CreateContentBitmap(CogDisplayContentBitmapConstants.Display, null, 2000).Save(sFileName, ImageFormat.Jpeg);
            //    //cTempBitmap.Save(sFileName, ImageFormat.Jpeg);
            //    //cogDS.Image.ToBitmap().Save(sFileName);
            //    new SaveImage()
            //    {
            //        format = ImageFormat.Bmp
            //    }.Save(cTempBitmap, sFileName);
            //}

            //DeleteExpireVisionImage(sDirectoryBase, 24 * CFG.ImgAutoDelDay);
        }
        public void ImageSave(bool bOK, CONST.eImageSaveType ImageSaveType, string sError = "", string sError2 = "", CONST.eImageSaveKind imgSave = CONST.eImageSaveKind.normal)
        {
            DateTime now = DateTime.Now;
            //여기 에러명 적고 세이브..
            if (bOK) cogLabel.Color = CogColorConstants.Green;
            else cogLabel.Color = CogColorConstants.Red;
            cogLabel.SetXYText(cogDS.Image.Width / 2, cogDS.Image.Height / 10, sError + " " + sError2);
            //cogLabel.SetXYText(100, 100, sError);
            cogLabel.Font = font40Bold;
            //cogDS.InteractiveGraphics.Add(cogLabel, "Error", false);
            cogDS.StaticGraphics.Add(cogLabel, "Error");

            string sFileNameOriginal = "";
            string sFileNameDisplay = "";
            string sDirectoryOriginal = "";
            string sDirectoryDisplay = "";

            string sDirectoryBase = Path.Combine(CONST.cImagePath, CFG.Name, now.ToString("yyyy-MM-dd"));
            string sDeleteDirectoryBase = Path.Combine(CONST.cImagePath, CFG.Name);

            if (imgSave != CONST.eImageSaveKind.normal)
            {
                sDirectoryBase = Path.Combine(CONST.cImagePath, imgSave.ToString(), now.ToString("yyyy-MM-dd"));
                sDeleteDirectoryBase = Path.Combine(CONST.cImagePath, imgSave.ToString());
            }

            //KSJ 20170713 Image Name add MCR ID
            //string sMCRID = MCRIDReturn();
            string sMCRID = PanelID;
            if (sMCRID.StartsWith(" ")) sMCRID = "NoPanelID";

            string sOKNG = "";
            if (bOK) sOKNG = "OK";
            else sOKNG = "NG";
            if (sError == "" && sError2 == "")
            {
                sOKNG = ""; //에러내용없으면 폴더명 빈칸넣음
            }

            sDirectoryOriginal = Path.Combine(sDirectoryBase, "Original" + sOKNG);
            sDirectoryDisplay = Path.Combine(sDirectoryBase, "Display" + sOKNG);

            if (!Directory.Exists(sDirectoryOriginal))
                Directory.CreateDirectory(sDirectoryOriginal);
            if (!Directory.Exists(sDirectoryDisplay))
                Directory.CreateDirectory(sDirectoryDisplay);

            sFileNameOriginal = Path.Combine(sDirectoryOriginal, now.ToString("yyyy-MM-dd-HH-mm-ss-fff") + " - " + sMCRID + ".jpg");
            sFileNameDisplay = Path.Combine(sDirectoryDisplay, now.ToString("yyyy-MM-dd-HH-mm-ss-fff") + " - " + sMCRID + ".jpg");

            ICogImage temp = cogDS.Image.CopyBase(CogImageCopyModeConstants.CopyPixels);
            CogRectangle size = new CogRectangle();
            size.Width = cogDS.Image.Width;
            size.Height = cogDS.Image.Height;
            if (sOKNG == "")
            {
                Task.Run(() =>
                {
                    try //저장후 그래픽 지워야할텐데..
                    {
                        cogDS.CreateContentBitmap(CogDisplayContentBitmapConstants.Display, size).Save(sFileNameDisplay, ImageFormat.Jpeg);
                        //DeleteInteractiveGraphic(cogDS, "Error"); //에러나면 이줄 주석처리할것
                        cogDS.StaticGraphics.Remove("Error");
                    }
                    catch
                    {

                    }
                });
            }
            else
            {
                if (CONST.eImageSaveType.Original == ImageSaveType || CONST.eImageSaveType.All == ImageSaveType)
                {
                    TaskOriImageSave(sFileNameOriginal);
                }
                if (CONST.eImageSaveType.Display == ImageSaveType || CONST.eImageSaveType.All == ImageSaveType)
                {
                    Task.Run(() =>
                    {
                        try //저장후 그래픽 지워야할텐데..
                        {
                            cogDS.CreateContentBitmap(CogDisplayContentBitmapConstants.Custom, size).Save(sFileNameDisplay, ImageFormat.Jpeg);
                            //DeleteInteractiveGraphic(cogDS, "Error"); //에러나면 이줄 주석처리할것
                            cogDS.StaticGraphics.Remove("Error");
                        }
                        catch
                        {

                        }
                    });
                }
            }

            DeleteExpireVisionImage(sDirectoryBase, 24 * CFG.ImgAutoDelDay , sDeleteDirectoryBase);

        }
        public bool EndTask()
        {
            for (int i = 0; i < searchDatas.Length; i++)
            {
                if (searchDatas[i].BcogSearch) ImageSetMemory(ref searchDatas[i].ImageMemory);
            }
            return true;
        }

        public bool ImageSetMemory(ref ICogImage _ImageMemory)
        {
            _ImageMemory = cogDS.Image.CopyBase(CogImageCopyModeConstants.CopyPixels);
            return true;
        }

        public bool TaskOriImageSave(string sPath)
        {
            //string tempDir = Path.GetDirectoryName(sPath);
            //if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);
            //ICogImage temp = cogDS.Image.CopyBase(CogImageCopyModeConstants.CopyPixels);
            
            Task.Run(() =>
            {
                try
                {
                    Bitmap bit = cogDS.Image.ToBitmap();
                    bit.Save(sPath, ImageFormat.Jpeg);
                    //CogImageFile imageFile = new CogImageFile();
                    //imageFile.Open(sPath, CogImageFileModeConstants.Write);
                    //imageFile.Append(temp);
                    //imageFile.Close();
                }
                catch { }
            });
            return true;
        }

        //KSJ 20170713 Image Name add MCR ID
        public string MCRIDReturn()
        {
            string strMCRID = "NoPanelID";
            //try
            //{
            //    if (CONST.PCNo == 1)
            //    {
            //        switch (CFG.Camno)
            //        {
            //            case Vision_No.vsConveyor:
            //                //strMCRID = (CONST.PanelIDConveyor.Length > 20) ? CONST.PanelIDConveyor.Substring(0, 20) : CONST.PanelIDConveyor;
            //                break;
            //            case Vision_No.vsLoadingBuffer1:
            //            case Vision_No.vsLoadingBuffer2:
            //                //strMCRID = (CONST.PanelIDLoadingBuffer.Length > 20) ? CONST.PanelIDLoadingBuffer.Substring(0, 20) : CONST.PanelIDLoadingBuffer;
            //                break;
            //            case Vision_No.vsBendPre1:
            //            case Vision_No.vsBendPre2:
            //            case Vision_No.vsBendPre3:
            //                strMCRID = (CONST.PanelIDBendPre.Length > 20) ? CONST.PanelIDBendPre.Substring(0, 20) : CONST.PanelIDBendPre;
            //                break;
            //            default:
            //                strMCRID += DateTime.Now.ToString("HH:mm:ss");
            //                break;

            //        }
            //    }
            //    else if (CONST.PCNo == 2)
            //    {
            //        switch (CFG.Camno)
            //        {
            //            case Vision_No.vsSCFPanel1:
            //            case Vision_No.vsSCFPanel2:
            //            case Vision_No.vsSCFPanel3:
            //                strMCRID = (CONST.PanelIDSCFPanel.Length > 20) ? CONST.PanelIDSCFPanel.Substring(0, 20) : CONST.PanelIDSCFPanel;
            //                break;
            //            case Vision_No.vsSCFReel1:
            //            case Vision_No.vsSCFReel2:
            //            case Vision_No.vsSCFReel3:
            //                strMCRID = (CONST.PanelIDSCFAttach.Length > 20) ? CONST.PanelIDSCFAttach.Substring(0, 20) : CONST.PanelIDSCFAttach;
            //                break;
            //            default:
            //                strMCRID += DateTime.Now.ToString("HH:mm:ss");
            //                break;
            //        }
            //    }
            //    else if (CONST.PCNo == 3)
            //    {
            //        switch (CFG.Camno)
            //        {
            //            case Vision_No.vsBend1_1:
            //            case Vision_No.vsBend1_2:
            //                strMCRID = (CONST.PanelIDBend[0].Length > 20) ? CONST.PanelIDBend[0].Substring(0, 20) : CONST.PanelIDBend[0];
            //                break;
            //            case Vision_No.vsBend2_1:
            //            case Vision_No.vsBend2_2:
            //                strMCRID = (CONST.PanelIDBend[1].Length > 20) ? CONST.PanelIDBend[1].Substring(0, 20) : CONST.PanelIDBend[1];
            //                break;
            //            case Vision_No.vsBend3_1:
            //            case Vision_No.vsBend3_2:
            //                strMCRID = (CONST.PanelIDBend[2].Length > 20) ? CONST.PanelIDBend[2].Substring(0, 20) : CONST.PanelIDBend[2];
            //                break;
            //            default:
            //                strMCRID += DateTime.Now.ToString("HH:mm:ss");
            //                break;
            //        }
            //    }
            //    else if (CONST.PCNo == 4)
            //    {
            //        switch (CFG.Camno)
            //        {
            //            case Vision_No.vsBendSide1:
            //                strMCRID = (CONST.PanelIDBend[0].Length > 20) ? CONST.PanelIDBend[0].Substring(0, 20) : CONST.PanelIDBend[0];
            //                break;
            //            case Vision_No.vsBendSide2:
            //                strMCRID = (CONST.PanelIDBend[1].Length > 20) ? CONST.PanelIDBend[1].Substring(0, 20) : CONST.PanelIDBend[1];
            //                break;
            //            case Vision_No.vsBendSide3:
            //                strMCRID = (CONST.PanelIDBend[2].Length > 20) ? CONST.PanelIDBend[2].Substring(0, 20) : CONST.PanelIDBend[2];
            //                break;
            //            case Vision_No.vsTempAttach:
            //                strMCRID = (CONST.PanelIDTempAttach.Length > 20) ? CONST.PanelIDTempAttach.Substring(0, 20) : CONST.PanelIDTempAttach;
            //                break;
            //            case Vision_No.vsEMIAttach:
            //                strMCRID = (CONST.PanelIDEMIAttach.Length > 20) ? CONST.PanelIDEMIAttach.Substring(0, 20) : CONST.PanelIDEMIAttach;
            //                break;
            //            case Vision_No.vsInspection1:
            //            case Vision_No.vsInspection2:
            //                strMCRID = (CONST.PanelIDInsp.Length > 20) ? CONST.PanelIDInsp.Substring(0, 20) : CONST.PanelIDInsp;
            //                break;
            //            case Vision_No.vsInspHeight:
            //                strMCRID = (CONST.PanelIDInspHeight.Length > 20) ? CONST.PanelIDInspHeight.Substring(0, 20) : CONST.PanelIDInspHeight;
            //                break;
            //            default:
            //                strMCRID += DateTime.Now.ToString("HH:mm:ss");
            //                break;
            //        }
            //    }
            //}
            //catch
            //{
            //    return "";
            //}

            return strMCRID.Trim();
        }

        public bool FindLine(ref CogLine cLine, string kind = "")
        {
            CogFindLineTool cogFindLine = new CogFindLineTool();
            cogFindLine.InputImage = (CogImage8Grey)cogDS.Image;
            string[] FileRead;
            //csh 20170622  string RunRecipe = ucSetting.DB.getRunRecipeName().Trim();
            CogCaliperScorerPosition cogpos = new CogCaliperScorerPosition();
            string RunRecipe = CONST.RunRecipe.RecipeName;
            try
            {
                string lcFile = string.Empty;
                string path = string.Empty;

                path = Path.Combine(CONST.cVisionPath, CFG.Name, CONST.stringRcp, RunRecipe, "Line");

                lcFile = Path.Combine(path, kind + ".txt"); //Path.Combine(CONST.cVisionImgPath, cfg.Name, RunRecipe.Trim(), "Pattern", kind + ".txt");

                if (!File.Exists(lcFile))
                {
                    //MessageBox.Show(kind + " SCFEdgeLine Parameter is not Exist!");
                    return false;
                }
                else
                {
                    FileRead = File.ReadAllLines(lcFile, Encoding.Default);

                    cogFindLine.RunParams.NumCalipers = int.Parse(FileRead[0]);
                    cogFindLine.RunParams.CaliperSearchLength = double.Parse(FileRead[1]);
                    cogFindLine.RunParams.CaliperProjectionLength = double.Parse(FileRead[2]);
                    if (FileRead[3] == "DarkToLight")
                        cogFindLine.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DarkToLight;
                    else if (FileRead[3] == "LightToDark")
                        cogFindLine.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.LightToDark;
                    else if (FileRead[3] == "DontCare")
                        cogFindLine.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DontCare;

                    if (FileRead[4] == "True") cogpos.Enabled = true;
                    else cogpos.Enabled = false;

                    cogFindLine.RunParams.CaliperSearchDirection = double.Parse(FileRead[5]); // *csvision.dRadian;
                    cogFindLine.RunParams.CaliperRunParams.ContrastThreshold = double.Parse(FileRead[6]);
                    cogFindLine.RunParams.NumToIgnore = int.Parse(FileRead[7]);
                    cogFindLine.RunParams.ExpectedLineSegment.StartX = double.Parse(FileRead[8]);
                    cogFindLine.RunParams.ExpectedLineSegment.StartY = double.Parse(FileRead[9]);
                    cogFindLine.RunParams.ExpectedLineSegment.EndX = double.Parse(FileRead[10]);
                    cogFindLine.RunParams.ExpectedLineSegment.EndY = double.Parse(FileRead[11]);
                }
                cogFindLine.CurrentRecordEnable = CogFindLineCurrentRecordConstants.All;

                cogFindLine.RunParams.CaliperRunParams.SingleEdgeScorers.Clear();
                cogFindLine.RunParams.CaliperRunParams.SingleEdgeScorers.Add(cogpos);

                cogFindLine.Run();
                if (CogToolResultConstants.Error != cogFindLine.RunStatus.Result
                    && CogToolResultConstants.Reject != cogFindLine.RunStatus.Result)
                {
                    cLine = cogFindLine.Results.GetLine();
                    cLine.Color = CogColorConstants.Magenta;
                    cogDS.InteractiveGraphics.Add(cLine, "Line", false);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch //(Exception EX)
            {
                return false;
                //cLog.ExceptionLogSave("SCFEdgeLine" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }

        public bool FindMarkingRef(ref cs2DAlign.ptXYT findPoint, bool linedisp, CogImage8Grey img = null)
        {
            try
            {
                cogDS.InteractiveGraphics.Remove("line");                
            }
            catch { }

            CogLine wLine = new CogLine();
            CogLine hLine = new CogLine();
            if (FindLine(ref wLine, eLineKind.FPCBWidth1, img) && FindLine(ref hLine, eLineKind.FPCBHeight1, img))
            {
                double dx = 0;
                double dy = 0;
                double dt = 0;
                bool ret = GetPointfrom2Line(wLine, hLine, ref dx, ref dy, ref dt, false);

                if (ret)
                {
                    findPoint.X = dx;
                    findPoint.Y = dy;

                    findPoint.T = wLine.Rotation;

                    if (linedisp)
                    {
                        wLine.Color = CogColorConstants.Orange;
                        hLine.Color = CogColorConstants.Orange;
                        cogDS.InteractiveGraphics.Add(wLine, "line", false);
                        cogDS.InteractiveGraphics.Add(hLine, "line", false);
                    }
                }
                return ret;
            }
            else return false;
        }

        public CogImage8Grey blobImage;

        public bool FindMarkingRefBlob(ref cs2DAlign.ptXYT findPoint, int Threshold, CogRectangle region)
        {
            try
            {
                cogDS.InteractiveGraphics.Remove("Blob");
            }
            catch { }

            CogBlobTool blobTool = new CogBlobTool();

            blobTool.InputImage = (CogImage8Grey)cogDS.Image;
            blobTool.RunParams.ConnectivityMode = CogBlobConnectivityModeConstants.GreyScale;
            blobTool.RunParams.ConnectivityCleanup = CogBlobConnectivityCleanupConstants.Fill;
            blobTool.RunParams.ConnectivityMinPixels = Menu.frmSetting.revData.mLaser.MinPixel;
            if (Menu.frmSetting.revData.mLaser.polarity == ePolarity.Dark)
                blobTool.RunParams.SegmentationParams.Polarity = CogBlobSegmentationPolarityConstants.DarkBlobs;
            else
                blobTool.RunParams.SegmentationParams.Polarity = CogBlobSegmentationPolarityConstants.LightBlobs;
            blobTool.RunParams.SegmentationParams.Mode = CogBlobSegmentationModeConstants.HardFixedThreshold;
            blobTool.RunParams.RegionMode = CogRegionModeConstants.PixelAlignedBoundingBox;

           
            //X 방향 Search
            blobTool.RunParams.SegmentationParams.HardFixedThreshold = Threshold;
            blobTool.Region = region;         

            blobTool.Run();

            CogBlobResult blobResult = new CogBlobResult();
            if (blobTool.RunStatus.Result == CogToolResultConstants.Accept)  // Blob 완료
            {
                int resultCnt = blobTool.Results.GetBlobs().Count;
                int select = -1;
                double oldCenter = 0;
                for (int i = 0; i < resultCnt; i++)
                {
                    CogBlobResult tResult = blobTool.Results.GetBlobs()[i];
                    double center = tResult.CenterOfMassX;

                    if (Menu.frmSetting.revData.mLaser.blobMass == eBlobMass.Right)
                    {
                        if (center > oldCenter)
                        {
                            oldCenter = center;
                            select = i;
                            blobResult = tResult;
                        }
                    }
                    else
                    {
                        if (center < oldCenter)
                        {
                            oldCenter = center;
                            select = i;
                            blobResult = tResult;
                        }
                    }
                }

                if (select >= 0)  //Find
                {
                    //CogPolygon boundary = blobResult.GetBoundary();
                    CogRectangleAffine box = blobResult.GetBoundingBox(CogBlobAxisConstants.Principal);

                    //cs2DAlign.ptXY[] CodePoint = new cs2DAlign.ptXY[4];
                    //for (int j = 0; j < CodePoint.Length; j++)
                    //{
                    //    boundary.GetVertex(j, out CodePoint[j].X, out CodePoint[j].Y);
                    //}

                    //Left Down Find
                    if (blobImage == null) blobImage = new CogImage8Grey();
                    blobImage = blobTool.Results.CreateBlobImage();


                    //X Compare
                    cs2DAlign.ptXY[] left = new cs2DAlign.ptXY[2];
                    cs2DAlign.ptXY[] right = new cs2DAlign.ptXY[2];
                    int ck = 0;
                    int ckR = 0;
                    if (box.CornerOppositeX < box.CenterX)
                    {
                        left[ck].X = box.CornerOppositeX;
                        left[ck].Y = box.CornerOppositeY;
                        ck++;
                    }
                    else
                    {
                        right[ckR].X = box.CornerOppositeX;
                        right[ckR].Y = box.CornerOppositeY;
                        ckR++;
                    }
                    if (box.CornerOriginX < box.CenterX)
                    {
                        left[ck].X = box.CornerOriginX;
                        left[ck].Y = box.CornerOriginY;
                        ck++;
                    }
                    else
                    {
                        right[ckR].X = box.CornerOriginX;
                        right[ckR].Y = box.CornerOriginY;
                        ckR++;
                    }
                    if (box.CornerXX < box.CenterX)
                    {
                        left[ck].X = box.CornerXX;
                        left[ck].Y = box.CornerXY;
                        ck++;
                    }
                    else
                    {
                        right[ckR].X = box.CornerXX;
                        right[ckR].Y = box.CornerXY;
                        ckR++;
                    }
                    if (box.CornerYX < box.CenterX)
                    {
                        left[ck].X = box.CornerYX;
                        left[ck].Y = box.CornerYY;
                        ck++;
                    }
                    else
                    {
                        right[ckR].X = box.CornerYX;
                        right[ckR].Y = box.CornerYY;
                        ckR++;
                    }

                    //Y Compare

                    if (Menu.frmSetting.revData.mLaser.blobPoint == eBlobPoint.LeftDown)
                    {
                        for (int i = 0; i < left.Length; i++)
                        {
                            if (box.CenterY < left[i].Y)
                            {
                                findPoint.X = left[i].X;
                                findPoint.Y = left[i].Y;
                                //findPoint.T = box.Rotation;
                                break;
                            }
                        }
                    }
                    else if (Menu.frmSetting.revData.mLaser.blobPoint == eBlobPoint.LeftUp)
                    {
                        for (int i = 0; i < left.Length; i++)
                        {
                            if (box.CenterY > left[i].Y)
                            {
                                findPoint.X = left[i].X;
                                findPoint.Y = left[i].Y;
                                //findPoint.T = box.Rotation;
                                break;
                            }
                        }
                    }
                    else if (Menu.frmSetting.revData.mLaser.blobPoint == eBlobPoint.RightDown)
                    {
                        for (int i = 0; i < right.Length; i++)
                        {
                            if (box.CenterY < right[i].Y)
                            {
                                findPoint.X = right[i].X;
                                findPoint.Y = right[i].Y;
                                //findPoint.T = box.Rotation;
                                break;
                            }
                        }
                    }
                    else if (Menu.frmSetting.revData.mLaser.blobPoint == eBlobPoint.RightUp)
                    {
                        for (int i = 0; i < right.Length; i++)
                        {
                            if (box.CenterY > right[i].Y)
                            {
                                findPoint.X = right[i].X;
                                findPoint.Y = right[i].Y;
                                //findPoint.T = box.Rotation;
                                break;
                            }
                        }
                    }




                    //findPoint.X = box.CornerOppositeX;
                    //findPoint.Y = box.CornerOppositeY;

                    CogPointMarker marker = new CogPointMarker();
                    marker.X = findPoint.X;
                    marker.Y = findPoint.Y;
                    marker.SizeInScreenPixels = 70;
                    marker.Color = CogColorConstants.Orange;
                    cogDS.InteractiveGraphics.Add(marker, "Blob", false);
                    box.Color = CogColorConstants.Magenta;
                    cogDS.InteractiveGraphics.Add(box, "Blob", false);
                    return true;
                }
                else return false;
                
            }

            return false;
            
        }

        

        public bool FindLine(ref CogLine cLine, eLineKind kind, CogImage8Grey img = null)
        {
            CogFindLineTool cogFindLine = new CogFindLineTool();
            if (img == null) cogFindLine.InputImage = (CogImage8Grey)cogDS.Image;
            else cogFindLine.InputImage = img;
            string[] FileRead;
            //csh 20170622  string RunRecipe = ucSetting.DB.getRunRecipeName().Trim();
            CogCaliperScorerPosition cogpos = new CogCaliperScorerPosition();
            string RunRecipe = CONST.RunRecipe.RecipeName;
            try
            {
                string lcFile = string.Empty;
                string path = string.Empty;

                path = Path.Combine(CONST.cVisionPath, CFG.Name, CONST.stringRcp, RunRecipe, "Line");

                lcFile = Path.Combine(path, kind.ToString() + ".txt"); //Path.Combine(CONST.cVisionImgPath, cfg.Name, RunRecipe.Trim(), "Pattern", kind + ".txt");

                if (!File.Exists(lcFile))
                {
                    //MessageBox.Show(kind + " SCFEdgeLine Parameter is not Exist!");
                    return false;
                }
                else
                {
                    FileRead = File.ReadAllLines(lcFile, Encoding.Default);

                    cogFindLine.RunParams.NumCalipers = int.Parse(FileRead[0]);
                    cogFindLine.RunParams.CaliperSearchLength = double.Parse(FileRead[1]);
                    cogFindLine.RunParams.CaliperProjectionLength = double.Parse(FileRead[2]);
                    if (FileRead[3] == "DarkToLight")
                        cogFindLine.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DarkToLight;
                    else if (FileRead[3] == "LightToDark")
                        cogFindLine.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.LightToDark;
                    else if (FileRead[3] == "DontCare")
                        cogFindLine.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DontCare;

                    if (FileRead[4] == "True") cogpos.Enabled = true;
                    else cogpos.Enabled = false;

                    cogFindLine.RunParams.CaliperSearchDirection = double.Parse(FileRead[5]); // *csvision.dRadian;
                    cogFindLine.RunParams.CaliperRunParams.ContrastThreshold = double.Parse(FileRead[6]);
                    cogFindLine.RunParams.NumToIgnore = int.Parse(FileRead[7]);
                    cogFindLine.RunParams.ExpectedLineSegment.StartX = double.Parse(FileRead[8]);
                    cogFindLine.RunParams.ExpectedLineSegment.StartY = double.Parse(FileRead[9]);
                    cogFindLine.RunParams.ExpectedLineSegment.EndX = double.Parse(FileRead[10]);
                    cogFindLine.RunParams.ExpectedLineSegment.EndY = double.Parse(FileRead[11]);
                }
                cogFindLine.CurrentRecordEnable = CogFindLineCurrentRecordConstants.All;

                cogFindLine.RunParams.CaliperRunParams.SingleEdgeScorers.Clear();
                cogFindLine.RunParams.CaliperRunParams.SingleEdgeScorers.Add(cogpos);

                cogFindLine.Run();
                if (CogToolResultConstants.Error != cogFindLine.RunStatus.Result
                    && CogToolResultConstants.Reject != cogFindLine.RunStatus.Result)
                {
                    cLine = cogFindLine.Results.GetLine();
                    //cLine = new CogLine(cogFindLine.Results.GetLine());
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch //(Exception EX)
            {
                return false;
                //cLog.ExceptionLogSave("SCFEdgeLine" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }

        private void WriteCogLabel(string strData, Font font, CogColorConstants Color, double nPosX, double nPosY, bool bAlign = false, string Name = "Defalut")
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
                cogDS.InteractiveGraphics.Add(cogLabel, Name, false);
            }
        }

        // 수정 필요 2018.07.10 
        public bool RunThreadFL(ref double dLeftDataX, ref double dLeftDataY, ref double dRightDataX, ref double dRightDataY)
        {
            try
            {
                CogLine[] cFLLine = new CogLine[3];
                CogIntersectLineLineTool cIntersectLineLine1 = new CogIntersectLineLineTool();
                CogIntersectLineLineTool cIntersectLineLine2 = new CogIntersectLineLineTool();
                if (!FindLine(ref cFLLine[0], "Width"))
                {
                    return false;
                }

                if (!FindLine(ref cFLLine[1], "Height1"))
                {
                    return false;
                }

                if (!FindLine(ref cFLLine[2], "Height2"))
                {
                    return false;
                }

                GetLineLinePoint(ref cIntersectLineLine1, cFLLine[0], cFLLine[1]);

                dLeftDataX = cIntersectLineLine1.X;
                dLeftDataY = cIntersectLineLine1.Y;

                GetLineLinePoint(ref cIntersectLineLine2, cFLLine[0], cFLLine[2]);

                dRightDataX = cIntersectLineLine2.X;
                dRightDataY = cIntersectLineLine2.Y;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public int GetLineLinePoint(ref CogIntersectLineLineTool cIntersectLineLine, CogLine cLine1, CogLine cLine2)
        {
            CogPointMarker cPointMaker = new CogPointMarker();

            cIntersectLineLine.InputImage = cogDS.Image;
            cIntersectLineLine.LineA = cLine1;
            cIntersectLineLine.LineB = cLine2;
            cIntersectLineLine.Run();

            cPointMaker.X = cIntersectLineLine.X;
            cPointMaker.Y = cIntersectLineLine.Y;
            cPointMaker.Color = CogColorConstants.Magenta;
            cogDS.InteractiveGraphics.Add(cPointMaker, "Point", false);

            return 1;
        }

        public void BendingInfoDisp(string skind, double dMarkx, double dMarky)

        {
            if (dMarkx != 0 && dMarky != 0)
            {
                CogGraphicLabel cogLabel = new CogGraphicLabel();
                string strData = strData = "X = " + dMarkx.ToString("0.000") + " Y = " + dMarky.ToString("0.000");

                //cogLabel.Color = Color;
                cogLabel.SetXYText(1500, 3900, strData);
                //cogLabel.Font = font;
                //if (bAlign)
                // {
                //cogLabel.Alignment = Cognex.VisionPro.CogGraphicLabelAlignmentConstants.TopLeft;
                // }
                cogDS.InteractiveGraphics.Add(cogLabel, skind, false);
            }


            //CogGraphicLabel cogLabel = new CogGraphicLabel();

            //cogLabel.SetXYText(400, 3800, strokng);

            //if (OKNG) cogLabel.Color = CogColorConstants.Green;
            //else cogLabel.Color = CogColorConstants.Red;


            //Font font = new Font("Arial", 20, FontStyle.Bold);
            //cogLabel.Font = font;

            //cogDS.InteractiveGraphics.Add(cogLabel, "OKNG", false);

        }
        public void InspOKNGDisp(bool OKNG /*, int BendNo*/)
        {
            CogGraphicLabel cogLabel = new CogGraphicLabel();
            CogGraphicLabel cogLabel2 = new CogGraphicLabel();
            string strokng = "OK";
            if (!OKNG) strokng = "NG";
            cogLabel.SetXYText(500, 300, strokng);

            if (OKNG) cogLabel.Color = CogColorConstants.Green;
            else cogLabel.Color = CogColorConstants.Red;


            Font font = new Font("Arial", 20, FontStyle.Bold);
            cogLabel.Font = font;

            cogDS.InteractiveGraphics.Add(cogLabel, "OKNG", false);

            //cogLabel2.SetXYText(1500, 400, BendNo.ToString());
            //cogLabel2.Color = CogColorConstants.Green;
            //cogLabel2.Font = font;
            //cogDS.InteractiveGraphics.Add(cogLabel2, "BendNo", false);
        }
        public void addLabel(string text, CogColorConstants textColor, double x, double y, float size, bool alignRight = false)
        {
            CogGraphicLabel cogLabel = new CogGraphicLabel();
            cogLabel.Color = textColor;
            cogLabel.Font = CONST.GetFontBold(size);
            cogLabel.SetXYText(x, y, text);
            cogLabel.Alignment = CogGraphicLabelAlignmentConstants.BaselineLeft;
            if (alignRight) cogLabel.Alignment = CogGraphicLabelAlignmentConstants.BaselineRight;
            cogDS.InteractiveGraphics.Add(cogLabel, "", false);
        }

        public void InspInfoDisp(double RefX, double RefY, double dX, double dY, bool bInspectionOK)
        {
            CogGraphicLabel cogLabel1 = new CogGraphicLabel();
            CogGraphicLabel cogLabel2 = new CogGraphicLabel();


            if (bodd(CFG.Camno))
            {
                cogLabel1.SetXYText(RefX - 600, RefY + 100, "X : " + dX.ToString("0.000"));
                cogLabel2.SetXYText(RefX - 550, RefY + 300, "Y : " + dY.ToString("0.000"));
            }
            else
            {
                cogLabel1.SetXYText(RefX + 600, RefY + 200, "X : " + dX.ToString("0.000"));
                cogLabel2.SetXYText(RefX + 650, RefY + 400, "Y : " + dY.ToString("0.000"));
            }

            cogLabel1.Font = font10Bold;
            cogLabel2.Font = font10Bold;

            if (bInspectionOK) cogLabel1.Color = CogColorConstants.Green;
            else cogLabel1.Color = CogColorConstants.Red;

            if (bInspectionOK) cogLabel2.Color = CogColorConstants.Green;
            else cogLabel2.Color = CogColorConstants.Red;

            cogDS.InteractiveGraphics.Add(cogLabel1, "InspInfo", false);
            cogDS.InteractiveGraphics.Add(cogLabel2, "InspInfo", false);

        }



        public bool SideRemove(int BendingNo)
        {
            try
            {
                string sDirectoryBase = Path.Combine(CONST.cSideVisionPath, CFG.Name);

                DirectoryInfo dir = new DirectoryInfo(sDirectoryBase);
                DirectoryInfo[] infos = dir.GetDirectories("*.*", SearchOption.TopDirectoryOnly);

                DateTime baseDateTime = DateTime.Now;

                foreach (System.IO.DirectoryInfo directory in infos)
                {
                    //string[] Split;
                    //Split = directory.ToString().Split('-');
                    ////연-월-일-시
                    //DateTime FileTime = new DateTime(int.Parse(Split[0]), int.Parse(Split[1]), int.Parse(Split[2]), int.Parse(Split[3]), 0, 0);

                    //TimeSpan diffTime = baseDateTime - FileTime;
                    TimeSpan diffTime = baseDateTime - directory.CreationTime;

                    if (diffTime.TotalDays >= 30) // 여기.
                    {
                        try
                        {
                            directory.Delete(true);
                        }
                        catch (Exception exp)
                        {
                            //MessageBox.Show(exp.Message);

                            System.Diagnostics.Debug.WriteLine(exp.Message);
                        }
                    }
                    //}
                }
                if (CONST.DdriveSpace > 80) //용량이 커지면 가장 오래된 2일치 삭제 //테스트필요
                {
                    DirectoryInfo[] lastdi = infos.OrderBy(di => di.LastWriteTime).Take(1).ToArray();
                    foreach (DirectoryInfo s in lastdi)
                    {
                        try
                        {
                            s.Delete(true);
                        }
                        catch (Exception EX)
                        {
                            System.Diagnostics.Debug.WriteLine(EX.Message);
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
                //Validation fail
            }
        }
        public bool SideSave(int BendingNo, List<CogImage8Grey> bitmaps, int Size = 0, bool bright = false)
        {
            //Capture Jpeg Image size 변경 2017.08.03
            try
            {
                string sFileName;

                //string sMCRID = MCRIDReturn();  //현재 Panel ID 취득
                string sMCRID = PanelID.Trim();
                string sDirectoryBase = Path.Combine(CONST.cSideVisionPath, CFG.Name);
                string sDirectoryName = Path.Combine(sDirectoryBase, DateTime.Now.ToString("yyyy-MM-dd-HH"));
                //if (bright) sDirectoryName += "_R";
                //else sDirectoryName += "_L";

                if (CONST.bProgramReset[BendingNo])
                {
                    try
                    {
                        string[] sdirectories = Directory.GetDirectories(sDirectoryName);
                        for (int i = 0; i < sdirectories.Length; i++)
                        {
                            if (sdirectories[i].Contains("NOPANELID"))
                            {
                                CONST.iNoPanelNo[BendingNo]++;
                            }
                        }
                    }
                    catch
                    {
                        CONST.iNoPanelNo[BendingNo] = 0;
                    }
                    CONST.bProgramReset[BendingNo] = false;
                }
                if (sMCRID == null || sMCRID.Trim() == "")
                {
                    sMCRID = "NOPANELID" + CONST.iNoPanelNo[BendingNo].ToString();
                    if (bright) sMCRID += "_R";
                    else sMCRID += "_L";
                }

                //날짜 폴더 체크
                if (!Directory.Exists(sDirectoryName))
                {
                    CONST.iNoPanelNo[BendingNo] = 0;
                }

                string cDirectoryName = Path.Combine(sDirectoryName, sMCRID);
                //if (bright) cDirectoryName += "_R";
                //else cDirectoryName += "_L";

                //Panel 폴더 체크
                if (!Directory.Exists(cDirectoryName))
                {
                    Directory.CreateDirectory(cDirectoryName);
                }

                int s = Directory.GetFiles(cDirectoryName).Length;

                int icnt = 0;
                foreach (var img in bitmaps)
                {
                    //sFileName = cDirectoryName + "/" + "BENDING SIDE_" + icnt.ToString("00");
                    sFileName = Path.Combine(cDirectoryName, "BENDING SIDE_" + icnt.ToString("00") + ".png");

                    int width = 400;
                    if (Size == 1) width = 800;
                    else if (Size == 2) width = 1200;
                    else if (Size == 3) width = 1600;
                    int height = (width * img.Height) / img.Width;


                    //CogImageFilePNG imageFilePNG = new CogImageFilePNG();
                    //imageFilePNG.Open(sFileName, CogImageFileModeConstants.Write);
                    //imageFilePNG.Append(img.ScaleImage(width, height));
                    //imageFilePNG.Close();
                    img.ScaleImage(width, height).ToBitmap().Save(sFileName, ImageFormat.Png);

                    //Bitmap resizeimage = new Bitmap(width, height);

                    //Graphics g = Graphics.FromImage(resizeimage);

                    //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    //g.DrawImage(img, new Rectangle(0, 0, width, height));
                    //string DirectoryName = Path.Combine(sDirectoryBase, DateTime.Now.ToString("yyyy-MM-dd-HH"));

                    //resizeimage.Save(sFileName + ".jpg");
                    icnt++;
                }

                if (sMCRID.Substring(0, 9) == "NOPANELID")// && bright)
                {
                    //우측을 나중에 저장함
                    CONST.iNoPanelNo[BendingNo]++;
                }

                return true;
            }
            catch
            {
                return false;
                //Validation fail
            }
        }
        public bool bodd(int num)
        {
            if (num % 2 == 0) //짝수
            {
                return false;
            }
            else return true; //홀수
        }

        public void TempPMAlignUntrain()
        {
            //untrain할지 bool변수로 관리하는게 좋을지..
            cogTempPMAlignPattern.Pattern.Untrain();
            cogTempPMAlignPattern_Ref.Pattern.Untrain();
        }

        //KSJ 20170710 임시마크등록
        CogPMAlignTool cogTempPMAlignPattern = new CogPMAlignTool();       //Pattern이랑 Ref랑 따로 ? 따로라면 밑에 함수를 복사하여 CogPMAlignTool 만 바꿔주자.
        CogPMAlignTool cogTempPMAlignPattern_Ref = new CogPMAlignTool();
        public void InitTempPMAlignPattern()
        {
            cogTempPMAlignPattern.RunParams.RunAlgorithm = CogPMAlignRunAlgorithmConstants.PatMax;
            cogTempPMAlignPattern.RunParams.ZoneAngle.Configuration = CogPMAlignZoneConstants.LowHigh;

            cogTempPMAlignPattern.Pattern.TrainRegionMode = CogRegionModeConstants.PixelAlignedBoundingBox;
            cogTempPMAlignPattern.Pattern.TrainAlgorithm = CogPMAlignTrainAlgorithmConstants.PatMax;
            cogTempPMAlignPattern.Pattern.TrainMode = CogPMAlignTrainModeConstants.Image;

            cogTempPMAlignPattern.RunParams.ScoreUsingClutter = false;


            cogTempPMAlignPattern.RunParams.ContrastThreshold = 10;

            cogTempPMAlignPattern.RunParams.ZoneAngle.Low = -2 * dRadian;
            cogTempPMAlignPattern.RunParams.ZoneAngle.High = 2 * dRadian;

            //cogTempPMAlignPattern.RunParams.AcceptThreshold = 90;
        }
        public void InitTempPMAlignPatternRef()
        {
            cogTempPMAlignPattern_Ref.RunParams.RunAlgorithm = CogPMAlignRunAlgorithmConstants.PatMax;
            cogTempPMAlignPattern_Ref.RunParams.ZoneAngle.Configuration = CogPMAlignZoneConstants.LowHigh;

            cogTempPMAlignPattern_Ref.Pattern.TrainRegionMode = CogRegionModeConstants.PixelAlignedBoundingBox;
            cogTempPMAlignPattern_Ref.Pattern.TrainAlgorithm = CogPMAlignTrainAlgorithmConstants.PatMax;
            cogTempPMAlignPattern_Ref.Pattern.TrainMode = CogPMAlignTrainModeConstants.Image;

            cogTempPMAlignPattern_Ref.RunParams.ScoreUsingClutter = false;


            cogTempPMAlignPattern_Ref.RunParams.ContrastThreshold = 10;

            cogTempPMAlignPattern_Ref.RunParams.ZoneAngle.Low = -2 * dRadian;
            cogTempPMAlignPattern_Ref.RunParams.ZoneAngle.High = 2 * dRadian;

            //cogTempPMAlignPattern_Ref.RunParams.AcceptThreshold = 90;
        }

        CogIPOneImageTool cogIPOneImageToolPattern = new CogIPOneImageTool();
        CogRectangle cogTrainTempRectanglePattern = new CogRectangle();
        CogRectangle cogSearchTempRectanglePattern = new CogRectangle();

        public bool SetTempPMAlignPattern(double dX, double dY)
        {
            double dTempPatternX = 0;
            double dTempPatternY = 0;
            double dTempPAtternRagionX = 0;
            double dTempPAtternRagionY = 0;
            double dTempSearchX = 0;
            double dTempSearchY = 0;
            double dTempSearchRagionX = 0;
            double dTempSearchRagionY = 0;

            try
            {
                //Set Train Image 
                cogIPOneImageToolPattern.RegionMode = CogRegionModeConstants.PixelAlignedBoundingBox;

                //중심에서 마크끝까지 거리를 30이라고 가정..
                CalculationTempData(dX, dY, 30, 30, ref dTempPatternX, ref dTempPatternY, ref dTempPAtternRagionX, ref dTempPAtternRagionY);

                cogTrainTempRectanglePattern.SetXYWidthHeight(dTempPatternX, dTempPatternY, dTempPAtternRagionX, dTempPAtternRagionY);     //설정한 좌표에서 크기 정하기.
                cogIPOneImageToolPattern.Region = cogTrainTempRectanglePattern;           //설정한 크기만큼 자르기 설정
                cogIPOneImageToolPattern.InputImage = cogDS.Image;                 //Grab한 이미지
                cogIPOneImageToolPattern.Run();                                    //자르기
                cogTempPMAlignPattern.Pattern.TrainImage = (CogImage8Grey)cogIPOneImageToolPattern.OutputImage;   //Train 이미지에 자른 이미지 넣기.

                //Set Train TranslationX,Y
                cogTempPMAlignPattern.Pattern.Origin.TranslationX = dX;
                cogTempPMAlignPattern.Pattern.Origin.TranslationY = dY;

                //Set Train Region
                cogTempPMAlignPattern.Pattern.TrainRegion = cogIPOneImageToolPattern.Region;

                //CogSearch Region
                CalculationTempData(dX, dY, 400, 400, ref dTempSearchX, ref dTempSearchY, ref dTempSearchRagionX, ref dTempSearchRagionY);

                cogSearchTempRectanglePattern.SetXYWidthHeight(dTempSearchX, dTempSearchY, dTempSearchRagionX, dTempSearchRagionY);
                cogTempPMAlignPattern.SearchRegion = cogSearchTempRectanglePattern;

                //Train
                cogTempPMAlignPattern.Pattern.Train();
                return true;
            }
            catch
            {
                return false;
            }
        }
        CogIPOneImageTool cogIPOneImageToolPattern_Ref = new CogIPOneImageTool();
        CogRectangle cogTrainTempRectanglePattern_Ref = new CogRectangle();
        CogRectangle cogSearchTempRectanglePattern_Ref = new CogRectangle();
        public bool SetTempPMAlignPatternRef(double dX, double dY)
        {
            double dTempPatternX = 0;
            double dTempPatternY = 0;
            double dTempPAtternRagionX = 0;
            double dTempPAtternRagionY = 0;
            double dTempSearchX = 0;
            double dTempSearchY = 0;
            double dTempSearchRagionX = 0;
            double dTempSearchRagionY = 0;
            try
            {
                //Set Train Image 
                cogIPOneImageToolPattern_Ref.RegionMode = CogRegionModeConstants.PixelAlignedBoundingBox;

                CalculationTempData(dX, dY, 30, 30, ref dTempPatternX, ref dTempPatternY, ref dTempPAtternRagionX, ref dTempPAtternRagionY);

                cogTrainTempRectanglePattern_Ref.SetXYWidthHeight(dTempPatternX, dTempPatternY, dTempPAtternRagionX, dTempPAtternRagionY);     //설정한 좌표에서 크기 정하기.
                cogIPOneImageToolPattern_Ref.Region = cogTrainTempRectanglePattern_Ref;           //설정한 크기만큼 자르기 설정
                cogIPOneImageToolPattern_Ref.InputImage = cogDS.Image;                 //Grab한 이미지
                cogIPOneImageToolPattern_Ref.Run();                                    //자르기
                cogTempPMAlignPattern_Ref.Pattern.TrainImage = (CogImage8Grey)cogIPOneImageToolPattern_Ref.OutputImage;   //Train 이미지에 자른 이미지 넣기.

                //Set Train TranslationX,Y
                cogTempPMAlignPattern_Ref.Pattern.Origin.TranslationX = dX;
                cogTempPMAlignPattern_Ref.Pattern.Origin.TranslationY = dY;

                //Set Train Region
                cogTempPMAlignPattern_Ref.Pattern.TrainRegion = cogIPOneImageToolPattern_Ref.Region;

                //CogSearch Region
                CalculationTempData(dX, dY, 400, 400, ref dTempSearchX, ref dTempSearchY, ref dTempSearchRagionX, ref dTempSearchRagionY);

                cogSearchTempRectanglePattern_Ref.SetXYWidthHeight(dTempSearchX, dTempSearchY, dTempSearchRagionX, dTempSearchRagionY);
                cogTempPMAlignPattern_Ref.SearchRegion = cogSearchTempRectanglePattern_Ref;

                //Train
                cogTempPMAlignPattern_Ref.Pattern.Train();
                return true;
            }
            catch
            {
                return false;
            }
        }

        double m_dTempPMAlignMaxScore = 0.9;

        private bool RunTempPMAlignPattern(out double dX, out double dY, out double dR, CogPMAlignTool cogTempPMAlignPattern)
        {
            CogPMAlignResult m_cogTempPMResultsPattern = null;
            dX = 0;
            dY = 0;
            dR = 0;
            if (!cogTempPMAlignPattern.Pattern.Trained) return false;
            bool bFindPM = false;
            //int nFindPM = 0;

            cogTempPMAlignPattern.InputImage = (CogImage8Grey)cogDS.Image;
            //cogTempPMAlignPattern.SearchRegion //등록할때 400x400으로 정해둠.
            cogTempPMAlignPattern.Run();

            double dmaxScore = 0;

            try
            {
                cogDS.InteractiveGraphics.Add(cogSearchTempRectanglePattern, "SearchRegion", false);

                if (cogTempPMAlignPattern.Results != null)
                {
                    for (int j = 0; j < cogTempPMAlignPattern.Results.Count; j++)
                    {
                        //cLog.Save(csLog.LogKind.System, "Score=" + cogPMAlign.Results[j].Score.ToString() + " / Align=" + CFG.Name + "," + sPatternFile);
                        //m_dTempPMAlignMaxScore 이상인 최고점 마크 찾음
                        if (cogTempPMAlignPattern.Results[j].Score > m_dTempPMAlignMaxScore && dmaxScore < cogTempPMAlignPattern.Results[j].Score)
                        {
                            dmaxScore = cogTempPMAlignPattern.Results[j].Score;
                            CONST.TextScore = dmaxScore;

                            //dSearchScore = dmaxScore; //임시마크 스코어 필요한지..

                            m_cogTempPMResultsPattern = new CogPMAlignResult(cogTempPMAlignPattern.Results[j]);

                            bFindPM = true;
                            //nFindPM = j;
                        }
                        else
                        {
                            //dSearchScore = dmaxScore; //임시마크 스코어 필요한지..
                        }
                    }

                    if (bFindPM)
                    {
                        CogPointMarker cogMarker = new CogPointMarker();
                        cogMarker.X = m_cogTempPMResultsPattern.GetPose().TranslationX;
                        cogMarker.Y = m_cogTempPMResultsPattern.GetPose().TranslationY;

                        cogMarker.Color = CogColorConstants.Yellow;
                        cogMarker.SizeInScreenPixels = 50;

                        cogDS.InteractiveGraphics.Add(cogMarker, "TempPM", false);

                        dX = m_cogTempPMResultsPattern.GetPose().TranslationX;
                        dY = m_cogTempPMResultsPattern.GetPose().TranslationY;
                        dR = m_cogTempPMResultsPattern.GetPose().Rotation;

                        return true;
                    }
                    else
                    {
                        if (CONST.m_bSystemLog)
                            cLog.Save(LogKind.System, " Fail / Align=" + CFG.Name);
                        return false;
                    }
                }
                else
                {
                    if (CONST.m_bSystemLog)
                        cLog.Save(LogKind.System, " Fail / Align=" + CFG.Name);
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }






        public void CalculationTempData(double dOriX, double dOriY, double dOriRagionX, double dOriRagionY, ref double dTempX, ref double dTempY, ref double dTempRagionX, ref double dTempRagionY)
        {
            double dCogDsX = cogDS.Image.Width;
            double dCogDsY = cogDS.Image.Height;

            if (dOriX - dOriRagionX < 0)
            {
                dTempX = 0;
            }
            else if (dOriX + dOriRagionX > dCogDsX)
            {
                dTempRagionX = (dOriRagionX * 2) - (dOriX + dOriRagionX - dCogDsX);
            }
            else
            {
                dTempX = dOriX - dOriRagionX;
                dTempRagionX = (dOriRagionX * 2);
            }

            if (dOriY - dOriRagionY < 0)
            {
                dTempY = 0;
            }
            else if (dOriY + dOriRagionY > dCogDsY)
            {
                dTempRagionY = (dOriRagionY * 2) - (dOriY + dOriRagionY - dCogDsY);
            }
            else
            {
                dTempY = dOriY - dOriRagionY;
                dTempRagionY = (dOriRagionY * 2);
            }
        }


        public const int mPanel = 0;
        public const int mFPC = 1;
        public bool RunTempPMAlign(ref cs2DAlign.ptXYT pt, int Kind)
        {
            CogPMAlignTool tool = new CogPMAlignTool();
            if (Kind == mPanel)
            {
                tool = cogTempPMAlignPattern;
            }
            else if (Kind == mFPC)
            {
                tool = cogTempPMAlignPattern_Ref;
            }
            if (tool.Pattern.Trained)
            {
                if (RunTempPMAlignPattern(out double _dX, out double _dY, out double _dR, tool))
                {
                    pt.X = _dX;
                    pt.Y = _dY;
                    pt.T = _dR;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public double scfInspResolutionX
        {
            get;
            set;
        }
        public double scfInspResolutionY
        {
            get;
            set;
        }
        //20.10.06 lkw
        public double scfInspectionXSpec
        {
            get;
            set;
        }
        public double scfInspectionYSpec
        {
            get;
            set;
        }
        public double scfInspectionTor
        {
            get;
            set;
        }

        public enum escfInspKind
        {
            LefttoRight,
            RighttoLeft,
            UptoDown,
            DowntoUp,
        }
        public bool SCFAttachInspectionCircle(cs2DAlign.ptXYT MarkPixelPoint, double dInRadius, double dOutRadius, double dInSearchLength, double dOutSearchLength,
            double CalipherCount, double IgnoreCount, int InFind, int OutFind, int InPolarity, int OutPolarity,
            double InThreshold, double OutThreshold, bool caliperDisp1, bool caliperDisp2, ref sideResult result, bool reverse = false)
        {
            //순서 안쪽원(파랑) -> 바깥쪽원(빨강)
            CogRectangle cogrectangle = new CogRectangle();
            CogFindCircleTool cogFindCircle = new CogFindCircleTool();

            result.inCenterX = 0;
            result.inCenterY = 0;
            result.inRadius = 0;
            result.outCenterX = 0;
            result.outCenterY = 0;
            result.outRadius = 0;

            string RunRecipe = CONST.RunRecipe.RecipeName.Trim(); // runrecipe 관련 수정.

            try
            {
                cogDS.InteractiveGraphics.Clear();
                cogDS.StaticGraphics.Clear();
            }
            catch
            { }

            try
            {
                cogFindCircle.InputImage = (CogImage8Grey)cogDS.Image;
                cogFindCircle.RunParams.NumCalipers = (int)CalipherCount;
                cogFindCircle.RunParams.CaliperProjectionLength = 10;

                CogCircularArc arc = new CogCircularArc();
                arc.CenterX = MarkPixelPoint.X;
                arc.CenterY = MarkPixelPoint.Y;

                //20.10.17 lkw
                arc.Radius = dInRadius;// double.Parse(Split[9]) / 6.0;   //-> ROI 설정을 넓게 해야 함.... 찾으려는 원을 벗어나도록 //반지름과 CaliperSearchLength가 중요함

                if (!reverse)  // 좌측으로 R 생김
                {
                    arc.AngleStart = 30 * Math.PI / 180.0;
                    arc.AngleSpan = 210 * Math.PI / 180.0;
                }
                else if (reverse) // 우측으로 R 생김
                {
                    arc.AngleStart = -70 * Math.PI / 180.0;
                    arc.AngleSpan = 220 * Math.PI / 180.0;
                }
                //무조건 360도
                //arc.AngleStart = 0 * Math.PI / 180.0;
                //arc.AngleSpan = 360 * Math.PI / 180.0;

                cogFindCircle.RunParams.ExpectedCircularArc = arc;
                cogFindCircle.RunParams.CaliperSearchLength = dInSearchLength;// 300; //중요
                if (InPolarity == 0)
                    cogFindCircle.RunParams.CaliperSearchDirection = CogFindCircleSearchDirectionConstants.Inward;
                if (InPolarity == 1)
                    cogFindCircle.RunParams.CaliperSearchDirection = CogFindCircleSearchDirectionConstants.Outward;

                //   if (checkKind == sideInsp.outCheck)
                //       cogFindCircle.RunParams.CaliperSearchDirection = CogFindCircleSearchDirectionConstants.Outward;
                if (InFind == 0)
                    cogFindCircle.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DarkToLight;
                else if (InFind == 1)
                    cogFindCircle.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.LightToDark;
                cogFindCircle.RunParams.CaliperRunParams.ContrastThreshold = InThreshold;
                //cogFindCircle.RunParams.DecrementNumToIgnore = true;
                cogFindCircle.RunParams.NumToIgnore = (int)IgnoreCount;
                // cogFindCircle.RunParams.RadiusConstraint
                //position가까운거 검색
                cogFindCircle.RunParams.CaliperRunParams.SingleEdgeScorers.Clear();
                CogCaliperScorerPosition scorerPosition = new CogCaliperScorerPosition();
                scorerPosition.Enabled = true;
                cogFindCircle.RunParams.CaliperRunParams.SingleEdgeScorers.Add(scorerPosition);

                cogFindCircle.CurrentRecordEnable = CogFindCircleCurrentRecordConstants.All;
                cogFindCircle.Run();

                if (caliperDisp1)
                {
                    CogGraphicCollection myRegions;
                    ICogRecord myRec;
                    CogCircularArc myArc;
                    myRec = cogFindCircle.CreateCurrentRecord();
                    myArc = (CogCircularArc)myRec.SubRecords["InputImage"].SubRecords["ExpectedShapeSegment"].Content;
                    myRegions = (CogGraphicCollection)myRec.SubRecords["InputImage"].SubRecords["CaliperRegions"].Content;
                    cogDS.StaticGraphics.Add(myArc, "ShapeSegmentRun1");
                    foreach (ICogGraphic g in myRegions)
                        cogDS.StaticGraphics.Add((ICogGraphicInteractive)g, "FindCircle1");
                    for (int i = 0; i < cogFindCircle.Results.Count; i++)
                    {
                        if (cogFindCircle.Results[i].Found)
                        {
                            //(오래걸림)
                            cogDS.InteractiveGraphics.Add(cogFindCircle.Results[i].CreateResultGraphics(CogFindCircleResultGraphicConstants.DataPoint), "point", false);
                            //cogDS.InteractiveGraphics.Add(cogFindCircle.Results[i].CreateResultGraphics(CogFindCircleResultGraphicConstants.CaliperRegion), "point", false);
                            //cogDS.InteractiveGraphics.Add(cogFindCircle.Results[i].CreateResultGraphics(CogFindCircleResultGraphicConstants.CaliperEdge), "point", false);

                        }
                    }
                }

                double dminmaxX = 100000;
                double dminmaxY = 0;
                if (reverse) dminmaxX = 0;

                bool b1 = false;
                if (cogFindCircle.RunStatus.Result == CogToolResultConstants.Accept)
                {
                    b1 = true;

                    if (false)
                    {
                        #region ///////// 20201102 ith edit
                        //for (int i = 0; i < cogFindCircle.Results.Count; i++)
                        //{
                        //    if (cogFindCircle.Results[i].Found && cogFindCircle.Results[i].Used)
                        //    {
                        //        if (!reverse)
                        //        {
                        //            if (cogFindCircle.Results[i].X < dminmaxX) dminmaxX = cogFindCircle.Results[i].X;
                        //        }
                        //        else
                        //        {
                        //            if (cogFindCircle.Results[i].X > dminmaxX) dminmaxX = cogFindCircle.Results[i].X;
                        //        }
                        //        if (cogFindCircle.Results[i].Y > dminmaxY) dminmaxY = cogFindCircle.Results[i].Y;
                        //    }
                        //}
                        #endregion
                    }
                    else
                    {
                        dminmaxX = cogFindCircle.Results.GetCircle().CenterX + cogFindCircle.Results.GetCircle().Radius; // circle center x pos + radius
                        dminmaxY = cogFindCircle.Results.GetCircle().CenterY + cogFindCircle.Results.GetCircle().Radius; // circle center y pos + radius
                    }
                }
                CogCircle ccArc = cogFindCircle.Results.GetCircle();
                result.inCenterX = ccArc.CenterX;
                result.inCenterY = ccArc.CenterY;
                result.inRadius = ccArc.Radius;
                if (caliperDisp1)
                {
                    try
                    {
                        ccArc.Color = CogColorConstants.Blue;
                        cogDS.InteractiveGraphics.Add(ccArc, "", false);

                        CogLineSegment lineX = new CogLineSegment();
                        lineX.StartX = ccArc.CenterX - 50;
                        lineX.StartY = ccArc.CenterY;
                        lineX.EndX = ccArc.CenterX + 50;
                        lineX.EndY = ccArc.CenterY;
                        lineX.Color = CogColorConstants.Blue;
                        cogDS.InteractiveGraphics.Add(lineX, "", false);

                        CogLineSegment lineY = new CogLineSegment();
                        lineY.StartX = ccArc.CenterX;
                        lineY.StartY = ccArc.CenterY - 50;
                        lineY.EndX = ccArc.CenterX;
                        lineY.EndY = ccArc.CenterY + 50;
                        lineY.Color = CogColorConstants.Blue;
                        cogDS.InteractiveGraphics.Add(lineY, "", false);
                    }
                    catch
                    {
                    }
                }
                arc.Radius = dOutRadius;// double.Parse(Split[9]) / 2.0; //중요
                cogFindCircle.RunParams.ExpectedCircularArc = arc;
                cogFindCircle.RunParams.CaliperSearchLength = dOutSearchLength;
                cogFindCircle.RunParams.CaliperRunParams.ContrastThreshold = OutThreshold;
                if (OutFind == 0)
                    cogFindCircle.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DarkToLight;
                else if (OutFind == 1)
                    cogFindCircle.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.LightToDark;
                if (OutPolarity == 0)
                    cogFindCircle.RunParams.CaliperSearchDirection = CogFindCircleSearchDirectionConstants.Inward;
                else if (OutPolarity == 1)
                    cogFindCircle.RunParams.CaliperSearchDirection = CogFindCircleSearchDirectionConstants.Outward;
                cogFindCircle.Run();

                if (caliperDisp2)
                {
                    CogGraphicCollection myRegions;
                    ICogRecord myRec;
                    CogCircularArc myArc;
                    myRec = cogFindCircle.CreateCurrentRecord();
                    myArc = (CogCircularArc)myRec.SubRecords["InputImage"].SubRecords["ExpectedShapeSegment"].Content;
                    myRegions = (CogGraphicCollection)myRec.SubRecords["InputImage"].SubRecords["CaliperRegions"].Content;
                    cogDS.StaticGraphics.Add(myArc, "ShapeSegmentRun2");
                    foreach (ICogGraphic g in myRegions)
                    {
                        g.Color = CogColorConstants.Magenta;
                        cogDS.StaticGraphics.Add((ICogGraphicInteractive)g, "FindCircle2");
                    }
                    for (int i = 0; i < cogFindCircle.Results.Count; i++)
                    {
                        if (cogFindCircle.Results[i].Found)
                        {
                            //(오래걸림)
                            cogDS.InteractiveGraphics.Add(cogFindCircle.Results[i].CreateResultGraphics(CogFindCircleResultGraphicConstants.DataPoint), "point", false);
                            //cogDS.InteractiveGraphics.Add(cogFindCircle.Results[i].CreateResultGraphics(CogFindCircleResultGraphicConstants.CaliperRegion), "point", false);
                            //cogDS.InteractiveGraphics.Add(cogFindCircle.Results[i].CreateResultGraphics(CogFindCircleResultGraphicConstants.CaliperEdge), "point", false);
                        }
                    }
                }

                double dminmaxX2 = 100000;
                double dminmaxY2 = 0;
                if (reverse) dminmaxX2 = 0;

                bool b2 = false;
                if (cogFindCircle.RunStatus.Result == CogToolResultConstants.Accept)
                {
                    b2 = true;

                    //-----------------------------------------------------------------------------
                    // 20201101 ith 세팅 수정사항
                    //      : 캘리퍼 갯수 50 -> 20
                    //-----------------------------------------------------------------------------
                    if (false) // 20201101 ith del
                    {
                        //for (int i = 0; i < cogFindCircle.Results.Count; i++)
                        //{
                        //    if (cogFindCircle.Results[i].Found && cogFindCircle.Results[i].Used)
                        //    {
                        //        if (!reverse)
                        //        {
                        //            if (cogFindCircle.Results[i].X < dminmaxX2) dminmaxX2 = cogFindCircle.Results[i].X;
                        //        }
                        //        else
                        //        {
                        //            if (cogFindCircle.Results[i].X > dminmaxX2) dminmaxX2 = cogFindCircle.Results[i].X;
                        //        }
                        //        if (cogFindCircle.Results[i].Y > dminmaxY2) dminmaxY2 = cogFindCircle.Results[i].Y;
                        //    }
                        //}
                    }
                    else
                    {
                        // 20201101 ith add
                        dminmaxX2 = cogFindCircle.Results.GetCircle().CenterX + cogFindCircle.Results.GetCircle().Radius;
                        dminmaxY2 = cogFindCircle.Results.GetCircle().CenterY + cogFindCircle.Results.GetCircle().Radius;
                    }
                }
                ccArc = cogFindCircle.Results.GetCircle();
                result.outCenterX = ccArc.CenterX;
                result.outCenterY = ccArc.CenterY;
                result.outRadius = ccArc.Radius;
                if (caliperDisp2)
                {
                    try
                    {
                        ccArc.Color = CogColorConstants.Orange;
                        cogDS.InteractiveGraphics.Add(ccArc, "", false);

                        CogLineSegment lineX = new CogLineSegment();
                        lineX.StartX = ccArc.CenterX - 100;
                        lineX.StartY = ccArc.CenterY;
                        lineX.EndX = ccArc.CenterX + 100;
                        lineX.EndY = ccArc.CenterY;
                        lineX.Color = CogColorConstants.Orange;
                        cogDS.InteractiveGraphics.Add(lineX, "", false);

                        CogLineSegment lineY = new CogLineSegment();
                        lineY.StartX = ccArc.CenterX;
                        lineY.StartY = ccArc.CenterY - 100;
                        lineY.EndX = ccArc.CenterX;
                        lineY.EndY = ccArc.CenterY + 100;
                        lineY.Color = CogColorConstants.Orange;
                        cogDS.InteractiveGraphics.Add(lineY, "", false);

                    }
                    catch
                    {
                    }
                }

                if (b1 && b2)
                {
                    CogGraphicLabel cogLabel = new CogGraphicLabel();
                    // 두께 계산 X 
                    //1 pixel값으로 계산
                    result.dist = Math.Abs(dminmaxX - dminmaxX2) * CFG.Resolution;
                    //2 중심간 거리와 반지름거리를 구해서 계산
                    //double dist = result.inCenterX - result.outCenterX;
                    //double dist2 = result.inRadius - result.outRadius;
                    //result.dist = dist * CFG.Resolution;
                    //result.dist += dist2 * CFG.Resolution;
                    //result.dist = Math.Abs(result.dist);

                    CogLineSegment cLine = new CogLineSegment();
                    cLine.StartX = dminmaxX;
                    cLine.StartY = result.inCenterY - 100;
                    cLine.EndX = dminmaxX;
                    cLine.EndY = result.inCenterY + 100;
                    cLine.Color = CogColorConstants.Red;
                    cogDS.InteractiveGraphics.Add(cLine, "", false);

                    CogLineSegment cLine2 = new CogLineSegment();
                    cLine2.StartX = dminmaxX2;// + dist + dist2;
                    cLine2.StartY = result.outCenterY - 100;
                    cLine2.EndX = dminmaxX2;// + dist + dist2;
                    cLine2.EndY = result.outCenterY + 100;
                    cLine2.Color = CogColorConstants.Red;
                    cogDS.InteractiveGraphics.Add(cLine2, "", false);

                    cogLabel.Font = font12Bold;
                    cogLabel.SetXYText(dminmaxX2 + 100, result.outCenterY, result.dist.ToString("0.000") + " mm");
                    cogDS.StaticGraphics.Add(cogLabel, "");

                    // 두께 계산 Y
                    result.distY = Math.Abs(dminmaxY - dminmaxY2) * CFG.Resolution;
                    //dist = Math.Abs(result.inCenterY - result.outCenterY);
                    //dist2 = Math.Abs(result.inRadius - result.outRadius);
                    //result.distY = dist * CFG.Resolution;
                    //result.distY += dist2 * CFG.Resolution;
                    //result.distY = Math.Abs(result.distY);

                    CogLineSegment cLine3 = new CogLineSegment();
                    cLine3.StartX = result.inCenterX - 100;
                    cLine3.StartY = dminmaxY;
                    cLine3.EndX = result.inCenterX + 100;
                    cLine3.EndY = dminmaxY;
                    cLine3.Color = CogColorConstants.Red;
                    cogDS.InteractiveGraphics.Add(cLine3, "", false);

                    CogLineSegment cLine4 = new CogLineSegment();
                    cLine4.StartX = result.outCenterX - 100;
                    cLine4.StartY = dminmaxY2;
                    cLine4.EndX = result.outCenterX + 100;
                    cLine4.EndY = dminmaxY2;
                    cLine4.Color = CogColorConstants.Red;
                    cogDS.InteractiveGraphics.Add(cLine4, "", false);

                    cogLabel.SetXYText(result.outCenterX, dminmaxY2 + 100, result.distY.ToString("0.000") + " mm");
                    cogDS.StaticGraphics.Add(cogLabel, "");

                    #region BD Pre Cam3 동그라미 Center 차이 구하기 // 20201102 ith edit
                    ////201030 cjm BD Pre Cam3 동그라미 Center 차이 구하기
                    //result.inoutdiffX = result.outCenterX - result.inCenterX;
                    //result.inoutdiffY = result.outCenterY - result.inCenterY;
                    #endregion
                    result.inoutdiffX = (result.outCenterX - result.inCenterX) * CFG.Resolution;
                    result.inoutdiffY = (result.outCenterY - result.inCenterY) * CFG.Resolution;

                    return true;
                }
            }
            catch { }
            return false;
        }
        public double SCFAttachInspection(cs2DAlign.ptXYT MarkPixelPoint, cs2DAlign.ptXY offset, int Threshold, escfInspKind eKind, out CogRectangleAffine dispBox, bool dispClear = false, bool regionDisp = false)
        {
            dispBox = new CogRectangleAffine();
            try
            {
                if (dispClear) this.DeleteInteractiveGraphic(cogDS, "SCF");
            }
            catch
            {
            }
            try
            {
                CogBlobTool blobTool = new CogBlobTool();

                blobTool.InputImage = (CogImage8Grey)cogDS.Image;
                blobTool.RunParams.ConnectivityMode = CogBlobConnectivityModeConstants.GreyScale;
                blobTool.RunParams.ConnectivityCleanup = CogBlobConnectivityCleanupConstants.Fill;
                blobTool.RunParams.ConnectivityMinPixels = 100;
                blobTool.RunParams.SegmentationParams.Polarity = CogBlobSegmentationPolarityConstants.LightBlobs;
                blobTool.RunParams.SegmentationParams.Mode = CogBlobSegmentationModeConstants.HardFixedThreshold;
                blobTool.RunParams.RegionMode = CogRegionModeConstants.PixelAlignedBoundingBox;

                CogRectangle rectangle = new CogRectangle(); //Search 영역 설정

                //X 방향 Search
                blobTool.RunParams.SegmentationParams.HardFixedThreshold = Threshold;

                double searchStartX = MarkPixelPoint.X + (offset.X / scfInspResolutionX);
                double searchStartY = MarkPixelPoint.Y + (offset.Y / scfInspResolutionY);

                double searchWidth = 400;
                double searchHeight = 40;
                if (eKind == escfInspKind.DowntoUp || eKind == escfInspKind.UptoDown)
                {
                    searchWidth = 40;
                    searchHeight = 400;
                }

                rectangle.SetXYWidthHeight(searchStartX, searchStartY, searchWidth, searchHeight);

                blobTool.Region = rectangle;

                if (regionDisp)
                {
                    rectangle.Color = CogColorConstants.Cyan;
                    cogDS.InteractiveGraphics.Add(rectangle, "SCF", false);
                }

                blobTool.Run();

                double MaxsizeX = 0;
                CogBlobResult blobResult = new CogBlobResult();
                if (blobTool.RunStatus.Result == CogToolResultConstants.Accept)  // Blob 완료
                {
                    int resultCnt = blobTool.Results.GetBlobs().Count;
                    double refPoint = 0;

                    for (int i = 0; i < resultCnt; i++)
                    {
                        blobResult = blobTool.Results.GetBlobs()[i];
                        CogRectangleAffine thisBox = blobResult.GetBoundingBox(CogBlobAxisConstants.ExtremaAngle);

                        double size = 0;
                        if (eKind == escfInspKind.DowntoUp || eKind == escfInspKind.UptoDown) size = Math.Abs(thisBox.CornerOriginY - thisBox.CornerOppositeY);
                        else size = Math.Abs(thisBox.CornerOppositeX - thisBox.CornerOriginX);

                        //TH를 충분히 낮추고 위치로 판단함.
                        if (eKind == escfInspKind.LefttoRight)   //가장 왼쪽 Blob 찾음
                        {
                            if (thisBox.CornerOriginX < refPoint || refPoint == 0)
                            {
                                refPoint = thisBox.CornerOriginX;
                                MaxsizeX = size;
                                dispBox = thisBox;
                            }
                        }
                        else if (eKind == escfInspKind.RighttoLeft)  //가장 오른쪽 Blob 찾음
                        {
                            if (thisBox.CornerOppositeX > refPoint || refPoint == 0)
                            {
                                refPoint = thisBox.CornerOppositeX;
                                MaxsizeX = size;
                                dispBox = thisBox;
                            }
                        }
                        else if (eKind == escfInspKind.UptoDown)   //가장 위쪽 Blob 찾음
                        {
                            if (thisBox.CornerOriginY < refPoint || refPoint == 0)
                            {
                                refPoint = thisBox.CornerOriginY;
                                MaxsizeX = size;
                                dispBox = thisBox;
                            }
                        }
                        else if (eKind == escfInspKind.DowntoUp)  //가장 아래쪽 Blob 찾음
                        {
                            if (thisBox.CornerOppositeY > refPoint || refPoint == 0)
                            {
                                refPoint = thisBox.CornerOppositeY;
                                MaxsizeX = size;
                                dispBox = thisBox;
                            }
                        }
                    }

                    if (MaxsizeX > 0)
                    {
                        dispBox.Color = CogColorConstants.Orange;
                        cogDS.InteractiveGraphics.Add(dispBox, "SCF", false);
                    }
                }

                double mm = MaxsizeX * scfInspResolutionX;
                if (eKind == escfInspKind.DowntoUp || eKind == escfInspKind.UptoDown) mm = MaxsizeX * scfInspResolutionY;

                CogGraphicLabel cogLabel = new CogGraphicLabel();

                cogLabel.SetXYText(searchStartX, searchStartY, mm.ToString("0.000") + "(mm)");

                double spec = scfInspectionXSpec;
                if (eKind == escfInspKind.DowntoUp || eKind == escfInspKind.UptoDown) spec = scfInspectionYSpec;
                //20.10.06 lkw
                if (Math.Abs(spec - mm) > scfInspectionTor && scfInspectionTor > 0)  //Error
                    cogLabel.Color = CogColorConstants.Red;
                else cogLabel.Color = CogColorConstants.Green;
                cogLabel.Font = font12Bold;
                cogDS.InteractiveGraphics.Add(cogLabel, "SCF", false);

                return mm;
            }
            catch
            {
                return -1;
            }
        }

        //2020.09.15 lkw
        public double SCFAttachInspection_old(cs2DAlign.ptXYT MarkPixelPoint, cs2DAlign.ptXY offset, int Threshold, bool heightDirection, bool dispClear = false, bool regionDisp = false)
        {
            try
            {
                if (dispClear) this.DeleteInteractiveGraphic(cogDS, "SCF");
            }
            catch
            {
            }
            try
            {
                CogBlobTool blobTool = new CogBlobTool();

                blobTool.InputImage = (CogImage8Grey)cogDS.Image;
                blobTool.RunParams.ConnectivityMode = CogBlobConnectivityModeConstants.GreyScale;
                blobTool.RunParams.ConnectivityCleanup = CogBlobConnectivityCleanupConstants.Fill;
                blobTool.RunParams.ConnectivityMinPixels = 10;
                blobTool.RunParams.SegmentationParams.Polarity = CogBlobSegmentationPolarityConstants.LightBlobs;
                blobTool.RunParams.SegmentationParams.Mode = CogBlobSegmentationModeConstants.HardFixedThreshold;
                blobTool.RunParams.RegionMode = CogRegionModeConstants.PixelAlignedBoundingBox;

                CogRectangle rectangle = new CogRectangle(); //Search 영역 설정

                //X 방향 Search
                blobTool.RunParams.SegmentationParams.HardFixedThreshold = Threshold;

                double searchStartX = MarkPixelPoint.X + (offset.X / scfInspResolutionX);
                double searchStartY = MarkPixelPoint.Y + (offset.Y / scfInspResolutionY);

                double searchWidth = 400;
                double searchHeight = 40;
                if (heightDirection)
                {
                    searchWidth = 40;
                    searchHeight = 400;
                }

                rectangle.SetXYWidthHeight(searchStartX, searchStartY, searchWidth, searchHeight);

                blobTool.Region = rectangle;

                if (regionDisp)
                {
                    rectangle.Color = CogColorConstants.Cyan;
                    cogDS.InteractiveGraphics.Add(rectangle, "SCF", false);
                }

                blobTool.Run();

                double MaxsizeX = 0;
                CogBlobResult blobResult = new CogBlobResult();
                if (blobTool.RunStatus.Result == CogToolResultConstants.Accept)  // Blob 완료
                {
                    int resultCnt = blobTool.Results.GetBlobs().Count;
                    for (int i = 0; i < resultCnt; i++)
                    {
                        blobResult = blobTool.Results.GetBlobs()[i];
                        CogRectangleAffine thisBox = blobResult.GetBoundingBox(CogBlobAxisConstants.ExtremaAngle);

                        double size = Math.Abs(thisBox.CornerOppositeX - thisBox.CornerOriginX);
                        if (heightDirection) size = Math.Abs(thisBox.CornerOriginY - thisBox.CornerOppositeY);

                        if (size > MaxsizeX) MaxsizeX = size;
                        thisBox.Color = CogColorConstants.Orange;
                        cogDS.InteractiveGraphics.Add(thisBox, "SCF", false);
                    }
                }

                double mm = MaxsizeX * scfInspResolutionX;
                if (heightDirection) mm = MaxsizeX * scfInspResolutionY;

                CogGraphicLabel cogLabel = new CogGraphicLabel();

                cogLabel.SetXYText(searchStartX, searchStartY, mm.ToString("0.000") + "(mm)");

                double spec = scfInspectionXSpec;
                if (heightDirection) spec = scfInspectionYSpec;
                //20.10.06 lkw
                if (Math.Abs(spec - mm) > scfInspectionTor && scfInspectionTor > 0)  //Error
                    cogLabel.Color = CogColorConstants.Red;
                else cogLabel.Color = CogColorConstants.Green;
                cogLabel.Font = font12Bold;
                cogDS.InteractiveGraphics.Add(cogLabel, "SCF", false);

                return mm;
            }
            catch
            {
                return -1;
            }
        }

        //20.12.17 lkw DL
        private Rectangle convertROI(CogRectangle imgROI)
        {
            Rectangle retROI = new Rectangle();
            if (imgROI != null)
            {
                retROI.X = (int)imgROI.X;
                retROI.Y = (int)imgROI.Y;
                retROI.Width = (int)imgROI.Width;
                retROI.Height = (int)imgROI.Height;
            }
            else
            {
                retROI.X = 0;
                retROI.Y = 0;
                retROI.Width = cogDS.Image.Width;
                retROI.Height = cogDS.Image.Height;
            }
            return retROI;
        }

        public bool PatternSearch_DL(ref cs2DAlign.ptXYT mark, ePatternKind kind, int ConnectionNO)
        {
            try
            {
                this.DeleteInteractiveGraphic(cogDS, "Point");
            }
            catch
            { }
            Bitmap dlImg = cogDS.Image.ToBitmap();

            Rectangle ROI = convertROI(ROI_Rectangle[(int)kind]);
            Point findPoint = new Point();
            Rectangle ret = new Rectangle();
            if (Menu.rsDL.pointSearch(ConnectionNO, dlImg, ROI, ref findPoint, ref ret))
            {
                mark.X = findPoint.X;
                mark.Y = findPoint.Y;

                CogPointMarker marker = new CogPointMarker();
                marker.X = mark.X;
                marker.Y = mark.Y;
                marker.SizeInScreenPixels = 70;
                marker.Color = CogColorConstants.Purple;

                cogDS.InteractiveGraphics.Add(marker, "Point", false);

                return true;
            }
            return false;
        }
        public bool Decision_DL(int ConnectionNO)
        {
            try
            {
                this.DeleteInteractiveGraphic(cogDS, "kind");
            }
            catch
            { }
            Bitmap dlImg = cogDS.Image.ToBitmap();

            string kind = "";
            double score = 0;
            if (Menu.rsDL.defectDecision(ConnectionNO, dlImg, ref kind, ref score))
            {
                CogGraphicLabel label = new CogGraphicLabel();

                label.Font = font15Bold;
                label.SetXYText(100, 100, kind); //위치 임시

                cogDS.InteractiveGraphics.Add(label, "kind", false);

                return true;
            }
            return false;
        }
    }
}

