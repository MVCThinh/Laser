using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bending
{

    public class REVData : FileRW
    {
        

        public struct Offset
        {
            public XYT CalXYT;
            public XYT AlignOffsetXYT;
            public XYT AlignOffsetXYT2; //temp, emi용 (1cam 2vision offset 따로주기위함)
            public XXYY FirstOffsetYY;
            public XXYY LastOffsetYY;
            public XYT BendingPreOffsetXYT;    // PC2에서 가지고 있음 (Pre의 Offset 값 써 주기 위함)    
            //public XXYY Scale;
            //public XXYY EgdeToEdgeScale;
            public rs2DAlign.cs2DAlign.ptXY CalFixPosOffset;
            public XYT MoveAdd;
            public XYT OnceMoveLimit; //pcy200628
            public double PressOffsetY1;
            public double PressOffsetY2;

            //210119 cjm ArmPre 인터락


        }
        public Offset[] mOffset = new Offset[CONST.CAMCnt];

        //20.12.17 lkw DL
        public struct DL
        {
            public bool[] MarkSearch_Use;
            public string[] MarkSearch_LabelPath;
            public string[] MarkSearch_ModelPath;

            public bool[] DefectFind_Use;
            public string[] DefectFind_LabelPath;
            public string[] DefectFind_ModelPath;
        }
        public DL[] mDL = new DL[CONST.CAMCnt];

        //2019.07.03 LoadPre 박리 검사
        public struct sLoadPreInsp
        {
            public double SCFInspOffsetX1;
            public double SCFInspOffsetY1;
            public double SCFInspOffsetX2;
            public double SCFInspOffsetY2;

            public double COFInspOffsetX1;
            public double COFInspOffsetY1;
            public double COFInspOffsetX2;
            public double COFInspOffsetY2;

            public double SCFInspTH1;
            public double SCFInspTH2;
            public double COFInspTH1;
            public double COFInspTH2;

            public bool SCF1InspUse;
            public bool SCF2InspUse;
            public bool COF1InspUse;
            public bool COF2InspUse;

            public double DetachOffsetX1;
            public double DetachOffsetY1;
            public double DetachLimitTH1;
            public double DetachOffsetX2;
            public double DetachOffsetY2;
            public double DetachLimitTH2;

            public double DetachOffsetX3;
            public double DetachOffsetY3;
            public double DetachLimitTH3;
            public double DetachOffsetX4;
            public double DetachOffsetY4;
            public double DetachLimitTH4;

            public double AttachOffsetX1;
            public double AttachOffsetY1;
            public double AttachLimitTH1;
            public double AttachOffsetX2;
            public double AttachOffsetY2;
            public double AttachLimitTH2;

            public double AttachOffsetX3;
            public double AttachOffsetY3;
            public double AttachLimitTH3;
            public double AttachOffsetX4;
            public double AttachOffsetY4;
            public double AttachLimitTH4;

            //tt20210407 Check White/ Black add option
            public bool CheckWhiteDetach1;
            public bool CheckWhiteDetach2;
            public bool CheckWhiteDetach3;
            public bool CheckWhiteDetach4;

            public bool CheckWhiteAttach1;
            public bool CheckWhiteAttach2;
            public bool CheckWhiteAttach3;
            public bool CheckWhiteAttach4;
        }
        public struct sConveyor
        {
            public double DivideLimitX;
            public double DivideLimitY;
            public double DivideLimitT;
        }
        public struct sAttach
        {
            public bool AttachRetryUse;
            public double DetachOffsetX1_1;
            public double DetachOffsetY1_1;
            public double DetachLimitTH1_1;
            public double DetachOffsetX1_2;
            public double DetachOffsetY1_2;
            public double DetachLimitTH1_2;
            public double DetachOffsetX2_1;
            public double DetachOffsetY2_1;
            public double DetachLimitTH2_1;
            public double DetachOffsetX2_2;
            public double DetachOffsetY2_2;
            public double DetachLimitTH2_2;
        }
        public struct sBendingPre
        {
            public bool BDPreSCFInspection;
            public bool SCFDistInspection;
            public double BDPreSCFMarkToEdgeY1;
            public double BDPreSCFMarkToEdgeY2;
            public double BDPreSCFSearchLineOffsetX1;
            public double BDPreSCFSearchLineOffsetY1;
            public double BDPreSCFSearchLineOffsetX2;
            public double BDPreSCFSearchLineOffsetY2;
            public bool BDPreSCFInspResultPass;
            public bool EMILogPatternSearch;

            public double SCFTolerance;

            //20200919 cjm Bending Pre SCF Existence Offset 추가
            public double SCFExistOffsetX1;
            public double SCFExistOffsetY1;
            public double SCFExistTH1;
            public double SCFExistOffsetX2;
            public double SCFExistOffsetY2;
            public double SCFExistTH2;
            public double SCFExistOffsetX3;
            public double SCFExistOffsetY3;
            public double SCFExistTH3;

            //20200926 cjm Bending Pre SCF Insption Offset 추가
            public double SCFInspCam1OffsetX1;
            public double SCFInspCam1OffsetY1;
            public double SCFInspCam1OffsetX2;
            public double SCFInspCam1OffsetY2;

            public double SCFInspCam1TH;   //가로
            public double SCFInspCam1TH2;  //세로

            public double SCFInspCam2OffsetX1;
            public double SCFInspCam2OffsetY1;
            public double SCFInspCam2OffsetX2;
            public double SCFInspCam2OffsetY2;

            public double SCFInspCam2TH;
            public double SCFInspCam2TH2;

            public double SCFInspCam3OffsetX1;
            public double SCFInspCam3OffsetY1;
            public double SCFInspCam3OffsetX2;
            public double SCFInspCam3OffsetY2;

            public double SCFInspCam3TH;
            public double SCFInspCam3TH2;
            
            public double SCFInspCam3InRadius;
            public double SCFInspCam3OutRadius;
            public double SCFInspCam3InSearchLength;
            public double SCFInspCam3OutSearchLength;

            public double SCFInspCam3CalipherCount;
            public double SCFInspCam3IgnoreCount;
            public int SCFInspCam3InFind;
            public int SCFInspCam3OutFind;
            public double SCFInspCam3InThreshold;
            public double SCFInspCam3OutThreshold;
            public int SCFInspCam3OutPolarity;
            public int SCFInspCam3InPolarity;
        }

        //lkw 20170821 Foam Size Offset 추가
        public struct sSizeSpecRatio
        {
            //lkw 20170828 Inspection PanelShift Check
            public double InspPanelShiftSpecX;
            public double InspPanelShiftSpecY;

            //pcy 20170925 Foam Check Spec 추가
            public double FoamInspSpec;
            public double FoamCheckRefY;

            public double SCFLengthSpec;

            public double SCFCheckX;
            public double SCFCheckY;

            public double BDPreSCFCheckOffsetX;
            public double BDPreSCFCheckOffsetY;

            public double BDPreSCFAttachTH;

            public double BDSCFCheckOffsetX;
            public double BDSCFCheckOffsetY;

            //2018.05.21 khs
            public double BDSCFAttachTH;

            //20.10.24 lkw
            public double BD1AddMoveRatio;
            public double BD2AddMoveRatio;
            public double BD3AddMoveRatio;

            public double LDTagetPosRot;

            public double EMITCalXMoveValue;
            public double EMITCalYMoveValue;

            //2019.07.20 EMI Align 추가
            public double plusYMoveRatio;
            public double plusTMoveRatio;
            public double minusYMoveRatio;
            public double minusTMoveRatio;

            public double SCFReelPickUpMarkPosOffsetX1;
            public double SCFReelPickUpMarkPosOffsetY1;
            public double SCFReelPickUpMarkPosOffsetX2;
            public double SCFReelPickUpMarkPosOffsetY2;
            public bool SCFCenterAlign;
            public double SCFXShiftRatio;
            public double SCFYShiftRatio;

            public double SideInspSpec;
            public double SideInspRef;

            public double AttachToleranceX;
            public double AttachToleranceY;

            public double LaserPositionToleranceX;
            public double LaserPositionToleranceY;

            public double LaserMarkSizeX;
            public double LaserMarkSizeY;

            public double LaserAlignPosTor;
        }

        public struct sUseFindLine
        {
            public bool bscfReel;
            public bool bFoamMain;
            public bool bPsaPre;
            public bool bPsaMain;
            //2018.11.29 KHS
            public bool bLoadPre;
            public bool bLoadPreTheta;
        }

        //csh 20170726
        public struct sLcheck
        {
            //LCheckOffset 카메라 하나당 두군데 잴 수 있도록 잡아놓음. cam1은 panel간 cam2는 fpc간
            //카메라1개인데 2포지션에 panel fpc다검사하는경우는 없다고 가정함
            public double LCheckOffset1;
            public double LCheckOffset2;
        }


        public struct sBendingArm
        {
            public bool bSCFInspection;
            public bool bSCFDistInspection;
        
            public bool bInitialPanelMarkSearchPass;
            public bool b180PanelMarkSearchPass;

            public bool[] bBDEdgeToEdgeUse;
            public bool bInspEdgeToEdgeUse;
            public bool bBDEdgeModeSearchMark;

            public bool bInspEdgeModeSearchMark;
            //pcy190521
            public bool bInspFPCBSearchMark;
            public eInspMode iInspMode;

            //tt210407 display L-R swap
            public bool bDisplayLRSwapBD;
            public bool bDisplayLRSwapINSP;
            public bool bBDUseLRSwap;

            //pcy190718
            public CONST.eInspFindSeq iInspFindSeq;

            public bool bInspection3PointDistMeasrue;

            public double bInspDistOffsetLX;
            public double bInspDistOffsetLY;
            public double bInspDistOffsetRX;
            public double bInspDistOffsetRY;

            //pcy200720
            public double bInspEdgeDistOffsetLX;
            public double bInspEdgeDistOffsetLY;
            public double bInspEdgeDistOffsetRX;
            public double bInspEdgeDistOffsetRY;

            public double BDToleranceX;
            public double BDToleranceY;
            public double InspToleranceX;
            public double InspToleranceY;

            public bool bBDResultGraphics;

            //pcy190608
            public bool bBDLastSpecOffsetUse;
            public double dBDLastSpecOffset;

            //pcy190613
            public bool bBDFirstInNoRetryUse;
            public double dBDFirstInNoRetrySpec;

            public bool bInspBeforeSubmarkUse;
            //pcy200627
            public double dBDFirstInNGSpec;
            public bool bBDFirstInNGUse;

            public bool bManualBendingUse;
        }

        public struct sLaser
        {
            public eID MCRSearchKind;
            public eRefSearch refSearch;
            public eInspKind inspKind;

            public eBlobMass blobMass;
            public eBlobPoint blobPoint;

            public ePolarity polarity;
            public int MinPixel;

            public bool UseImageProcess;
            public bool MCRRight;
            public bool MCRUp;
        }

        public sBendingArm mBendingArm;
        public sBendingPre mBendingPre;
        public sConveyor mConveyor;
        public sAttach mAttach;
        public sLaser mLaser;
        public struct sStartGrabDleay
        {
            public int BDTrans1;
            public int BDTrans2;
            public int BDTrans3;
            public int BDArm1;
            public int BDArm2;
            public int BDArm3;
            public int BDLightGap;
        }

        public sStartGrabDleay mStartGrabDelay;

        public struct ScaleResolution
        {
            public double X;
            public double Y;
        }
        public sLcheck[] mLcheck = new sLcheck[CONST.CAMCnt];

        //public sFoamSource mFoamPos;
        
        public sUseFindLine mbUseFindLine;

        public bool FoamOneshotMode = false;

        public sLoadPreInsp mLoadPreInsp;

        public sSizeSpecRatio mSizeSpecRatio;

        public double BDScaleSpec;
        public double INSPScaleSpec;

        const double DefaultCalX = 3;
        const double DefaultCalY = 3;
        const double DefaultCalT = 2;
        //public Light[] m_Light = new Light[8];
        //public Inspection m_Inspection;
        public ScaleResolution[] mResolution = new ScaleResolution[CONST.CAMCnt];
        public REVData(string _path, string _fileName)
            : base(_path, _fileName)
        {

            mBendingArm = new sBendingArm();
            mLoadPreInsp = new sLoadPreInsp();
            //csh 20170726

            for (int i = 0; i < mOffset.Length; i++)
            {
                mOffset[i].CalXYT = new XYT();
                mOffset[i].AlignOffsetXYT = new XYT();
                mOffset[i].AlignOffsetXYT2 = new XYT();
                mOffset[i].LastOffsetYY = new XXYY();
                mOffset[i].FirstOffsetYY = new XXYY();
                mOffset[i].BendingPreOffsetXYT = new XYT();
                //mOffset[i].Scale = new XXYY();
                //mOffset[i].EgdeToEdgeScale = new XXYY();
                mOffset[i].CalFixPosOffset = new rs2DAlign.cs2DAlign.ptXY();
                mOffset[i].MoveAdd = new XYT();
                mOffset[i].OnceMoveLimit = new XYT();
            }
            
            //20.12.17 lkw DL
            int cntDL = 2;  //더 늘릴 일은 없을 듯.... Camera 당 4개 (Mark 2, Defect 2)
            for (int i = 0; i < mDL.Length; i++)
            {
                mDL[i].MarkSearch_Use = new bool[cntDL];
                mDL[i].MarkSearch_LabelPath = new string[cntDL];
                mDL[i].MarkSearch_ModelPath = new string[cntDL];

                mDL[i].DefectFind_Use = new bool[cntDL];
                mDL[i].DefectFind_LabelPath = new string[cntDL];
                mDL[i].DefectFind_ModelPath = new string[cntDL];
            }

            for (int i = 0; i < mLcheck.Length; i++)
            {
                mLcheck[i].LCheckOffset1 = 0;
                mLcheck[i].LCheckOffset2 = 0;
            }

            //for (int i = 0; i < 4; i++)
            //{
            //    m_Light[i].AutoLight1 = 0;
            //    m_Light[i].AutoLight2 = 0;
            //    m_Light[i].Expose = 0;
            //    m_Light[i].RefExpose = 0;
            //}
            for (int i = 0; i < mResolution.Length; i++)
            {
                mResolution[i] = new ScaleResolution();
            }

            //mBendingArm.bRefMarkSearchPass = new bool[5];
            mBendingArm.bBDEdgeToEdgeUse = new bool[5];

        }

        public void Initialize()
        {
        }
        public void ReadData(string _modelName = "")
        {
            double val = new double();
            string valStr;
            string Section = null;
            // [sFMLoadBufferPre]
            //CAM 번호는 eCAMNo 참조, PC1번 ~ PC3번의 카메라 번호를 0 ~ 7번 까지 각각 사용 

            Section = "Light";
            for (int i = 0; i < CONST.CAMCnt; i++)
            {
                //Menu.frmAutoMain.Vision[i].CFG.RefExposure = Convert.ToInt32(ReadValue(_modelName, Section, "CAM" + i.ToString() + ".RefExposure", 0));
                //Menu.frmAutoMain.Vision[i].CFG.SubExposure = Convert.ToInt32(ReadValue(_modelName, Section, "CAM" + i.ToString() + ".SubExposure", 0));
                //Menu.frmAutoMain.Vision[i].CFG.RefAutoLight = Convert.ToInt32(ReadValue(_modelName, Section, "CAM" + i.ToString() + ".RefAutoLight", 0));
                //Menu.frmAutoMain.Vision[i].CFG.SubAutoLight = Convert.ToInt32(ReadValue(_modelName, Section, "CAM" + i.ToString() + ".SubAutoLight", 0));
                //Menu.frmAutoMain.Vision[i].CFG.EdgeAutoLight = Convert.ToInt32(ReadValue(_modelName, Section, "CAM" + i.ToString() + ".EdgeAutoLight", 0));
                //Menu.frmAutoMain.Vision[i].CFG.EdgeSubAutoLight = Convert.ToInt32(ReadValue(_modelName, Section, "CAM" + i.ToString() + ".EdgeLineAutoLight", 0));
                ////pcy190521
                //Menu.frmAutoMain.Vision[i].CFG.FPCBAutoLight = Convert.ToInt32(ReadValue(_modelName, Section, "CAM" + i.ToString() + ".FPCBAutoLight", 0));
                //Menu.frmAutoMain.Vision[i].CFG.FPCBSubAutoLight = Convert.ToInt32(ReadValue(_modelName, Section, "CAM" + i.ToString() + ".FPCBSubAutoLight", 0));

                ////190624 cjm Contrast추가, 노출값으로 밝기가 해결 안되는 곳에 사용
                //Menu.frmAutoMain.Vision[i].CFG.Contrast = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".Contrast", 0.0);
                //Menu.frmAutoMain.Vision[i].CFG.SubContrast = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".SubContrast", 0.0);

                foreach (var s in Enum.GetValues(typeof(ePatternKind)))
                {
                    Menu.frmAutoMain.Vision[i].CFG.Light[(int)s] = Convert.ToInt32(ReadValue(_modelName, Section, "CAM" + i.ToString() + ".Light." + s, 0));
                    Menu.frmAutoMain.Vision[i].CFG.Exposure[(int)s] = Convert.ToInt32(ReadValue(_modelName, Section, "CAM" + i.ToString() + ".Exposure." + s, 0));
                    Menu.frmAutoMain.Vision[i].CFG.Contrast[(int)s] = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".Contrast." + s, 0.0);
                }
                foreach (var s in Enum.GetValues(typeof(eLineKind)))
                {
                    if ((eLineKind)s != eLineKind.Null)
                    {
                        Menu.frmAutoMain.Vision[i].CFG.LineLight[(int)s] = Convert.ToInt32(ReadValue(_modelName, Section, "CAM" + i.ToString() + ".LineLight." + s, 0));
                        Menu.frmAutoMain.Vision[i].CFG.LineExposure[(int)s] = Convert.ToInt32(ReadValue(_modelName, Section, "CAM" + i.ToString() + ".LineExposure." + s, 0));
                        Menu.frmAutoMain.Vision[i].CFG.LineContrast[(int)s] = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".LineContrast." + s, 0.0);
                    }
                }
            }

            Section = "Target";
            for (int i = 0; i < CONST.CAMCnt; i++)
            {
                foreach (var s in Enum.GetValues(typeof(ePatternKind)))
                {
                    Menu.frmAutoMain.Vision[i].CFG.TargetX[(int)s] = ReadValue(_modelName, Section, "CAM" + i + ".TargetX" + (int)s, 0);
                    Menu.frmAutoMain.Vision[i].CFG.TargetY[(int)s] = ReadValue(_modelName, Section, "CAM" + i + ".TargetY" + (int)s, 0);
                    Menu.frmAutoMain.Vision[i].CFG.TargetX2[(int)s] = ReadValue(_modelName, Section, "CAM" + i + ".TargetX2" + (int)s, 0);
                    Menu.frmAutoMain.Vision[i].CFG.TargetY2[(int)s] = ReadValue(_modelName, Section, "CAM" + i + ".TargetY2" + (int)s, 0);
                }
            }

            Section = "Offset";
            for (int i = 0; i < mOffset.Length; i++)
            {
                mOffset[i].PressOffsetY1 = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".PressOffsetY1", 0.0);
                mOffset[i].PressOffsetY2 = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".PressOffsetY2", 0.0);
                mOffset[i].CalXYT.X = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".CalXYT.X", DefaultCalX);
                mOffset[i].CalXYT.Y = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".CalXYT.Y", DefaultCalX);
                mOffset[i].CalXYT.T = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".CalXYT.T", DefaultCalX);
                mOffset[i].AlignOffsetXYT.X = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".Offset1XYT.X", 0.0);
                mOffset[i].AlignOffsetXYT.Y = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".Offset1XYT.Y", 0.0);
                mOffset[i].AlignOffsetXYT.T = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".Offset1XYT.T", 0.0);
                mOffset[i].AlignOffsetXYT2.X = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".Offset2XYT.X", 0.0);
                mOffset[i].AlignOffsetXYT2.Y = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".Offset2XYT.Y", 0.0);
                mOffset[i].AlignOffsetXYT2.T = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".Offset2XYT.T", 0.0);
                mOffset[i].BendingPreOffsetXYT.X = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".BendingPreOffsetXYT.X", 0.0);
                mOffset[i].BendingPreOffsetXYT.Y = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".BendingPreOffsetXYT.Y", 0.0);
                mOffset[i].BendingPreOffsetXYT.T = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".BendingPreOffsetXYT.T", 0.0);

                mOffset[i].CalFixPosOffset.X = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".CalFixPosOffset.X", 0.0);
                mOffset[i].CalFixPosOffset.Y = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".CalFixPosOffset.Y", 0.0);

            }

            //20.12.17 lkw DL
            Section = "DL";
            for (int i = 0; i < mDL.Length; i++)
            {
                for (int j = 0; j < mDL[i].MarkSearch_Use.Length; j++)
                {
                    //pcy210201 사용유무는 ModelPath유무로 함.
                    //valStr = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".MarkSearch_USE" + j.ToString(), "False");
                    //mDL[i].MarkSearch_Use[j] = valStr == "True" ? true : false;
                    valStr = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".MarkSearch_LabelPath" + j.ToString(), "");
                    mDL[i].MarkSearch_LabelPath[j] = valStr;
                    valStr = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".MarkSearch_ModelPath" + j.ToString(), "");
                    mDL[i].MarkSearch_ModelPath[j] = valStr;
                }

                for (int j = 0; j < mDL[i].DefectFind_Use.Length; j++)
                {
                    //pcy210201 사용유무는 ModelPath유무로 함.
                    //valStr = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".DefectFind_USE" + j.ToString(), "False");
                    //mDL[i].DefectFind_Use[j] = valStr == "True" ? true : false;
                    valStr = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".DefectFind_LabelPath" + j.ToString(), "");
                    mDL[i].DefectFind_LabelPath[j] = valStr;
                    valStr = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".DefectFind_ModelPath" + j.ToString(), "");
                    mDL[i].DefectFind_ModelPath[j] = valStr;
                }
            }

            Section = "BendingOffset";
            for (int i = 0; i < mOffset.Length; i++)
            {
                mOffset[i].FirstOffsetYY.Y1 = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".FirstOffsetYY.Y1", 0.0);
                mOffset[i].FirstOffsetYY.Y2 = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".FirstOffsetYY.Y2", 0.0);
                mOffset[i].LastOffsetYY.Y1 = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".LastOffsetYY.Y1", 0.0);
                mOffset[i].LastOffsetYY.Y2 = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".LastOffsetYY.Y2", 0.0);

                //mOffset[i].EgdeToEdgeScale.X1 = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".EdgeToEdgeScale.X1", 1.0);
                //mOffset[i].EgdeToEdgeScale.Y1 = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".EdgeToEdgeScale.Y1", 1.0);
                //mOffset[i].EgdeToEdgeScale.X2 = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".EdgeToEdgeScale.X2", 1.0);
                //mOffset[i].EgdeToEdgeScale.Y2 = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".EdgeToEdgeScale.Y2", 1.0);

                //mOffset[i].Scale.X1 = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".Scale.X1", 1.0);
                //mOffset[i].Scale.Y1 = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".Scale.Y1", 1.0);
                //mOffset[i].Scale.X2 = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".Scale.X2", 1.0);
                //mOffset[i].Scale.Y2 = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".Scale.Y2", 1.0);

                mOffset[i].MoveAdd.X = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".MoveAdd.X", 0.0);
                mOffset[i].MoveAdd.Y = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".MoveAdd.Y", 0.0);
                mOffset[i].MoveAdd.T = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".MoveAdd.T", 0.0);

                mOffset[i].OnceMoveLimit.X = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".OnceMoveLimit.X", 0.0);
                mOffset[i].OnceMoveLimit.Y = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".OnceMoveLimit.Y", 0.0);
                mOffset[i].OnceMoveLimit.T = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".OnceMoveLimit.T", 0.0);
            }

            


            Section = "LCheck";
            for (int i = 0; i < mLcheck.Length; i++)
            {
                mLcheck[i].LCheckOffset1 = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".LCheckOffset1", 0);
                mLcheck[i].LCheckOffset2 = ReadValue(_modelName, Section, "CAM" + i.ToString() + ".LCheckOffset2", 0);
            }

            //PC1Model
            Section = "PC1Model";
            valStr = ReadValue(_modelName, Section, "AttachRetryUse", "False");
            mAttach.AttachRetryUse = valStr == "True" ? true : false;

            val = ReadValue(_modelName, Section, "DetachOffsetX1_1", 0.0);
            mAttach.DetachOffsetX1_1 = val;
            val = ReadValue(_modelName, Section, "DetachOffsetY1_1", 0.0);
            mAttach.DetachOffsetY1_1 = val;
            val = ReadValue(_modelName, Section, "DetachLimitTH1_1", 0.0);
            mAttach.DetachLimitTH1_1 = val;
            val = ReadValue(_modelName, Section, "DetachOffsetX1_2", 0.0);
            mAttach.DetachOffsetX1_2 = val;
            val = ReadValue(_modelName, Section, "DetachOffsetY1_2", 0.0);
            mAttach.DetachOffsetY1_2 = val;
            val = ReadValue(_modelName, Section, "DetachLimitTH1_2", 0.0);
            mAttach.DetachLimitTH1_2 = val;


            valStr = ReadValue(_modelName, Section, "BDPreSCFInspection", "True");
            mBendingPre.BDPreSCFInspection = valStr == "True" ? true : false;

            valStr = ReadValue(_modelName, Section, "BDPreSCFDistInspection", "True");
            mBendingPre.SCFDistInspection = valStr == "True" ? true : false;

            val = ReadValue(_modelName, Section, "SCFLengthSpec", 0.0);
            mSizeSpecRatio.SCFLengthSpec = val;

            val = ReadValue(_modelName, Section, "BDPreSCFInspOffsetX", 0.0);
            mSizeSpecRatio.BDPreSCFCheckOffsetX = val;

            val = ReadValue(_modelName, Section, "BDPreSCFInspOffsetY", 0.0);
            mSizeSpecRatio.BDPreSCFCheckOffsetY = val;

            val = ReadValue(_modelName, Section, "BDPreSCFInspTH", 0);
            mSizeSpecRatio.BDPreSCFAttachTH = val;

            val = ReadValue(_modelName, Section, "BDPreSCFMarkToEdgeY1", 0);
            mBendingPre.BDPreSCFMarkToEdgeY1 = val;

            val = ReadValue(_modelName, Section, "BDPreSCFMarkToEdgeY2", 0);
            mBendingPre.BDPreSCFMarkToEdgeY2 = val;

            val = ReadValue(_modelName, Section, "BDPreSCFSearchLineOffsetX1", 0);
            mBendingPre.BDPreSCFSearchLineOffsetX1 = val;

            val = ReadValue(_modelName, Section, "BDPreSCFSearchLineOffsetY1", 0);
            mBendingPre.BDPreSCFSearchLineOffsetY1 = val;

            val = ReadValue(_modelName, Section, "BDPreSCFSearchLineOffsetX2", 0);
            mBendingPre.BDPreSCFSearchLineOffsetX2 = val;

            val = ReadValue(_modelName, Section, "BDPreSCFSearchLineOffsetY2", 0);
            mBendingPre.BDPreSCFSearchLineOffsetY2 = val;

            val = ReadValue(_modelName, Section, "SCFTolerance", 0);
            mBendingPre.SCFTolerance = val;

            valStr = ReadValue(_modelName, Section, "BDPreSCFInspResultPass", "True");
            mBendingPre.BDPreSCFInspResultPass = valStr == "True" ? true : false;

            valStr = ReadValue(_modelName, Section, "LDPreLineThetaUse", "True");
            mbUseFindLine.bLoadPreTheta = valStr == "True" ? true : false;

            //2018.11.29 khs
            valStr = ReadValue(_modelName, Section, "LoadPreLineSearchMode", "True");
            mbUseFindLine.bLoadPre = valStr == "True" ? true : false;

            val = ReadValue(_modelName, Section, "LoadPreTagetPosRotation", 0.0);
            mSizeSpecRatio.LDTagetPosRot = val;

            valStr = ReadValue(_modelName, Section, "EMILogPatternSearch", "True");
            mBendingPre.EMILogPatternSearch = valStr == "True" ? true : false;

            val = ReadValue(_modelName, Section, "EMITCalXMoveValue", 0.0);
            mSizeSpecRatio.EMITCalXMoveValue = val;

            val = ReadValue(_modelName, Section, "EMITCalYMoveValue", 0.0);
            mSizeSpecRatio.EMITCalYMoveValue = val;

            //2019.07.20 EMI Align 추가
            val = ReadValue(_modelName, Section, "plusYMoveRatio", 0.0);
            mSizeSpecRatio.plusYMoveRatio = val;
            val = ReadValue(_modelName, Section, "plusTMoveRatio", 0.0);
            mSizeSpecRatio.plusTMoveRatio = val;
            val = ReadValue(_modelName, Section, "minusYMoveRatio", 0.0);
            mSizeSpecRatio.minusYMoveRatio = val;
            val = ReadValue(_modelName, Section, "minusTMoveRatio", 0.0);
            mSizeSpecRatio.minusTMoveRatio = val;

            val = ReadValue(_modelName, Section, "SCFReelPickUpMarkPosOffsetX1", 0);
            mSizeSpecRatio.SCFReelPickUpMarkPosOffsetX1 = val;

            val = ReadValue(_modelName, Section, "SCFReelPickUpMarkPosOffsetY1", 0);
            mSizeSpecRatio.SCFReelPickUpMarkPosOffsetY1 = val;

            val = ReadValue(_modelName, Section, "SCFReelPickUpMarkPosOffsetX2", 0);
            mSizeSpecRatio.SCFReelPickUpMarkPosOffsetX2 = val;

            val = ReadValue(_modelName, Section, "SCFReelPickUpMarkPosOffsetY2", 0);
            mSizeSpecRatio.SCFReelPickUpMarkPosOffsetY2 = val;

            valStr = ReadValue(_modelName, Section, "SCFCenterAlign", "False");
            mSizeSpecRatio.SCFCenterAlign = valStr == "True" ? true : false;

            val = ReadValue(_modelName, Section, "SCFXShiftRatio", 0);
            mSizeSpecRatio.SCFXShiftRatio = val;

            val = ReadValue(_modelName, Section, "SCFYShiftRatio", 0);
            mSizeSpecRatio.SCFYShiftRatio = val;

            ////20200919 cjm Bending Pre SCF Inspection offset 추가
            val = ReadValue(_modelName, Section, "SCFExistOffsetX1", 0);
            mBendingPre.SCFExistOffsetX1 = val;
            val = ReadValue(_modelName, Section, "SCFExistOffsetY1", 0);
            mBendingPre.SCFExistOffsetY1 = val;
            val = ReadValue(_modelName, Section, "SCFExistTH1", 0);
            mBendingPre.SCFExistTH1 = val;

            val = ReadValue(_modelName, Section, "SCFExistOffsetX2", 0);
            mBendingPre.SCFExistOffsetX2 = val;
            val = ReadValue(_modelName, Section, "SCFExistOffsetY2", 0);
            mBendingPre.SCFExistOffsetY2 = val;
            val = ReadValue(_modelName, Section, "SCFExistTH2", 0);
            mBendingPre.SCFExistTH2 = val;

            val = ReadValue(_modelName, Section, "SCFExistOffsetX3", 0);
            mBendingPre.SCFExistOffsetX3 = val;
            val = ReadValue(_modelName, Section, "SCFExistOffsetY3", 0);
            mBendingPre.SCFExistOffsetY3 = val;
            val = ReadValue(_modelName, Section, "SCFExistTH3", 0);
            mBendingPre.SCFExistTH3 = val;

            //20200926 cjm Bending Pre SCF Inspection offset 추가
            val = ReadValue(_modelName, Section, "SCFInspCam1OffsetX1", 0);
            mBendingPre.SCFInspCam1OffsetX1 = val;
            val = ReadValue(_modelName, Section, "SCFInspCam1OffsetY1", 0);
            mBendingPre.SCFInspCam1OffsetY1 = val;
            val = ReadValue(_modelName, Section, "SCFInspCam1OffsetX2", 0);
            mBendingPre.SCFInspCam1OffsetX2 = val;
            val = ReadValue(_modelName, Section, "SCFInspCam1OffsetY2", 0);
            mBendingPre.SCFInspCam1OffsetY2 = val;

            val = ReadValue(_modelName, Section, "SCFInspCam1TH", 0);
            mBendingPre.SCFInspCam1TH = val;

            val = ReadValue(_modelName, Section, "SCFInspCam1TH2", 0);
            mBendingPre.SCFInspCam1TH2 = val;

            val = ReadValue(_modelName, Section, "SCFInspCam2OffsetX1", 0);
            mBendingPre.SCFInspCam2OffsetX1 = val;
            val = ReadValue(_modelName, Section, "SCFInspCam2OffsetY1", 0);
            mBendingPre.SCFInspCam2OffsetY1 = val;
            val = ReadValue(_modelName, Section, "SCFInspCam2OffsetX2", 0);
            mBendingPre.SCFInspCam2OffsetX2 = val;
            val = ReadValue(_modelName, Section, "SCFInspCam2OffsetY2", 0);
            mBendingPre.SCFInspCam2OffsetY2 = val;

            val = ReadValue(_modelName, Section, "SCFInspCam2TH", 0);
            mBendingPre.SCFInspCam2TH = val;

            val = ReadValue(_modelName, Section, "SCFInspCam2TH2", 0);
            mBendingPre.SCFInspCam2TH2 = val;

            val = ReadValue(_modelName, Section, "SCFInspCam3OffsetX1", 0);
            mBendingPre.SCFInspCam3OffsetX1 = val;
            val = ReadValue(_modelName, Section, "SCFInspCam3OffsetY1", 0);
            mBendingPre.SCFInspCam3OffsetY1 = val;
            val = ReadValue(_modelName, Section, "SCFInspCam3OffsetX2", 0);
            mBendingPre.SCFInspCam3OffsetX2 = val;
            val = ReadValue(_modelName, Section, "SCFInspCam3OffsetY2", 0);
            mBendingPre.SCFInspCam3OffsetY2 = val;

            val = ReadValue(_modelName, Section, "SCFInspCam3TH", 0);
            mBendingPre.SCFInspCam3TH = val;
            val = ReadValue(_modelName, Section, "SCFInspCam3TH2", 0);
            mBendingPre.SCFInspCam3TH2 = val;

            val = ReadValue(_modelName, Section, "SCFInspCam3InRadius", 0);
            mBendingPre.SCFInspCam3InRadius = val;
            val = ReadValue(_modelName, Section, "SCFInspCam3OutRadius", 0);
            mBendingPre.SCFInspCam3OutRadius = val;
            val = ReadValue(_modelName, Section, "SCFInspCam3InSearchLength", 0);
            mBendingPre.SCFInspCam3InSearchLength = val;
            val = ReadValue(_modelName, Section, "SCFInspCam3OutSearchLength", 0);
            mBendingPre.SCFInspCam3OutSearchLength = val;

            val = ReadValue(_modelName, Section, "SCFInspCam3CalipherCount", 0);
            mBendingPre.SCFInspCam3CalipherCount = val;
            val = ReadValue(_modelName, Section, "SCFInspCam3IgnoreCount", 0);
            mBendingPre.SCFInspCam3IgnoreCount = val;
            val = ReadValue(_modelName, Section, "SCFInspCam3InFind", 0);
            mBendingPre.SCFInspCam3InFind = (int)val;
            val = ReadValue(_modelName, Section, "SCFInspCam3OutFind", 0);
            mBendingPre.SCFInspCam3OutFind = (int)val;
            val = ReadValue(_modelName, Section, "SCFInspCam3InPolarity", 0);
            mBendingPre.SCFInspCam3InPolarity = (int)val;
            val = ReadValue(_modelName, Section, "SCFInspCam3OutPolarity", 0);
            mBendingPre.SCFInspCam3OutPolarity = (int)val;
            val = ReadValue(_modelName, Section, "SCFInspCam3InThreshold", 0);
            mBendingPre.SCFInspCam3InThreshold = val;
            val = ReadValue(_modelName, Section, "SCFInspCam3OutThreshold", 0);
            mBendingPre.SCFInspCam3OutThreshold = val;

            val = ReadValue(_modelName, Section, "DivideLimitX", 0);
            mConveyor.DivideLimitX = val;
            val = ReadValue(_modelName, Section, "DivideLimitY", 0);
            mConveyor.DivideLimitY = val;
            val = ReadValue(_modelName, Section, "DivideLimitT", 0);
            mConveyor.DivideLimitT = val;

            //DetachInspection
            Section = "DetachInspection";
            val = ReadValue(_modelName, Section, "DetachOffsetX1", 0);
            mLoadPreInsp.DetachOffsetX1 = val;
            val = ReadValue(_modelName, Section, "DetachOffsetY1", 0);
            mLoadPreInsp.DetachOffsetY1 = val;
            val = ReadValue(_modelName, Section, "DetachLimitTH1", 0);
            mLoadPreInsp.DetachLimitTH1 = val;
            val = ReadValue(_modelName, Section, "DetachOffsetX2", 0);
            mLoadPreInsp.DetachOffsetX2 = val;
            val = ReadValue(_modelName, Section, "DetachOffsetY2", 0);
            mLoadPreInsp.DetachOffsetY2 = val;
            val = ReadValue(_modelName, Section, "DetachLimitTH2", 0);
            mLoadPreInsp.DetachLimitTH2 = val;

            val = ReadValue(_modelName, Section, "DetachOffsetX3", 0);
            mLoadPreInsp.DetachOffsetX3 = val;
            val = ReadValue(_modelName, Section, "DetachOffsetY3", 0);
            mLoadPreInsp.DetachOffsetY3 = val;
            val = ReadValue(_modelName, Section, "DetachLimitTH3", 0);
            mLoadPreInsp.DetachLimitTH3 = val;
            val = ReadValue(_modelName, Section, "DetachOffsetX4", 0);
            mLoadPreInsp.DetachOffsetX4 = val;
            val = ReadValue(_modelName, Section, "DetachOffsetY4", 0);
            mLoadPreInsp.DetachOffsetY4 = val;
            val = ReadValue(_modelName, Section, "DetachLimitTH4", 0);
            mLoadPreInsp.DetachLimitTH4 = val;

            val = ReadValue(_modelName, Section, "AttachOffsetX1", 0);
            mLoadPreInsp.AttachOffsetX1 = val;
            val = ReadValue(_modelName, Section, "AttachOffsetY1", 0);
            mLoadPreInsp.AttachOffsetY1 = val;
            val = ReadValue(_modelName, Section, "AttachLimitTH1", 0);
            mLoadPreInsp.AttachLimitTH1 = val;
            val = ReadValue(_modelName, Section, "AttachOffsetX2", 0);
            mLoadPreInsp.AttachOffsetX2 = val;
            val = ReadValue(_modelName, Section, "AttachOffsetY2", 0);
            mLoadPreInsp.AttachOffsetY2 = val;
            val = ReadValue(_modelName, Section, "AttachLimitTH2", 0);
            mLoadPreInsp.AttachLimitTH2 = val;

            val = ReadValue(_modelName, Section, "AttachOffsetX3", 0);
            mLoadPreInsp.AttachOffsetX3 = val;
            val = ReadValue(_modelName, Section, "AttachOffsetY3", 0);
            mLoadPreInsp.AttachOffsetY3 = val;
            val = ReadValue(_modelName, Section, "AttachLimitTH3", 0);
            mLoadPreInsp.AttachLimitTH3 = val;
            val = ReadValue(_modelName, Section, "AttachOffsetX4", 0);
            mLoadPreInsp.AttachOffsetX4 = val;
            val = ReadValue(_modelName, Section, "AttachOffsetY4", 0);
            mLoadPreInsp.AttachOffsetY4 = val;
            val = ReadValue(_modelName, Section, "AttachLimitTH4", 0);
            mLoadPreInsp.AttachLimitTH4 = val;

            valStr = ReadValue(_modelName, Section, "CheckWhiteDetach1", "True");
            mLoadPreInsp.CheckWhiteDetach1 = valStr == "True" ? true : false;
            valStr = ReadValue(_modelName, Section, "CheckWhiteDetach2", "True");
            mLoadPreInsp.CheckWhiteDetach2 = valStr == "True" ? true : false;
            valStr = ReadValue(_modelName, Section, "CheckWhiteDetach3", "True");
            mLoadPreInsp.CheckWhiteDetach3 = valStr == "True" ? true : false;
            valStr = ReadValue(_modelName, Section, "CheckWhiteDetach4", "True");
            mLoadPreInsp.CheckWhiteDetach4 = valStr == "True" ? true : false;

            valStr = ReadValue(_modelName, Section, "CheckWhiteAttach1", "True");
            mLoadPreInsp.CheckWhiteAttach1 = valStr == "True" ? true : false;
            valStr = ReadValue(_modelName, Section, "CheckWhiteAttach2", "True");
            mLoadPreInsp.CheckWhiteAttach2 = valStr == "True" ? true : false;
            valStr = ReadValue(_modelName, Section, "CheckWhiteAttach3", "True");
            mLoadPreInsp.CheckWhiteAttach3 = valStr == "True" ? true : false;
            valStr = ReadValue(_modelName, Section, "CheckWhiteAttach4", "True");
            mLoadPreInsp.CheckWhiteAttach4 = valStr == "True" ? true : false;

            //PC2Model
            Section = "PC2Model";

            val = ReadValue(_modelName, Section, "DetachOffsetX2_1", 0.0);
            mAttach.DetachOffsetX2_1 = val;
            val = ReadValue(_modelName, Section, "DetachOffsetY2_1", 0.0);
            mAttach.DetachOffsetY2_1 = val;
            val = ReadValue(_modelName, Section, "DetachLimitTH2_1", 0.0);
            mAttach.DetachLimitTH2_1 = val;
            val = ReadValue(_modelName, Section, "DetachOffsetX2_2", 0.0);
            mAttach.DetachOffsetX2_2 = val;
            val = ReadValue(_modelName, Section, "DetachOffsetY2_2", 0.0);
            mAttach.DetachOffsetY2_2 = val;
            val = ReadValue(_modelName, Section, "DetachLimitTH2_2", 0.0);
            mAttach.DetachLimitTH2_2 = val;

            val = ReadValue(_modelName, Section, "BDTrans1StartGrabDelay", 0);
            mStartGrabDelay.BDTrans1 = Convert.ToInt32(val);

            val = ReadValue(_modelName, Section, "BDTrans2StartGrabDelay", 0);
            mStartGrabDelay.BDTrans2 = Convert.ToInt32(val);

            val = ReadValue(_modelName, Section, "BDTrans3StartGrabDelay", 0);
            mStartGrabDelay.BDTrans3 = Convert.ToInt32(val);

            val = ReadValue(_modelName, Section, "BDArm1StartGrabDelay", 0);
            mStartGrabDelay.BDArm1 = Convert.ToInt32(val);

            val = ReadValue(_modelName, Section, "BDArm2StartGrabDelay", 0);
            mStartGrabDelay.BDArm2 = Convert.ToInt32(val);

            val = ReadValue(_modelName, Section, "BDArm3StartGrabDelay", 0);
            mStartGrabDelay.BDArm3 = Convert.ToInt32(val);

            valStr = ReadValue(_modelName, Section, "bInitialPanelMarkSearchPass", "True");
            mBendingArm.bInitialPanelMarkSearchPass = valStr == "True" ? true : false;

            valStr = ReadValue(_modelName, Section, "b180PanelMarkSearchPass", "True");
            mBendingArm.b180PanelMarkSearchPass = valStr == "True" ? true : false;

            //valStr = ReadValue(_modelName, Section, "BD3RefMarkSearchPass", "True");
            //mBendingArm.bRefMarkSearchPass[2] = valStr == "True" ? true : false;

            valStr = ReadValue(_modelName, Section, "bDisplayLRSwapBD", "False");
            mBendingArm.bDisplayLRSwapBD = valStr == "True" ? true : false;

            valStr = ReadValue(_modelName, Section, "bDisplayLRSwapINSP", "False");
            mBendingArm.bDisplayLRSwapINSP = valStr == "True" ? true : false;

            valStr = ReadValue(_modelName, Section, "bBDUseLRSwap", "False");
            mBendingArm.bBDUseLRSwap = valStr == "True" ? true : false;

            valStr = ReadValue(_modelName, Section, "BDSCFInspection", "True");
            mBendingArm.bSCFInspection = valStr == "True" ? true : false;

            valStr = ReadValue(_modelName, Section, "BDSCFDistInspection", "True");
            mBendingArm.bSCFDistInspection = valStr == "True" ? true : false;

            //valStr = ReadValue(_modelName, Section, "BDEdgeRefPatternUse", "True");
            //mBendingArm.bEdgeBDRefPatternUse = valStr == "True" ? true : false;

            //SCF Inspection 위치 offset 추가
            val = ReadValue(_modelName, Section, "SCFInspX", 0.0);
            mSizeSpecRatio.BDSCFCheckOffsetX = val;

            val = ReadValue(_modelName, Section, "SCFInspY", 0.0);
            mSizeSpecRatio.BDSCFCheckOffsetY = val;

            val = ReadValue(_modelName, Section, "SCFInspTH", 0);
            mSizeSpecRatio.BDSCFAttachTH = val;

            valStr = ReadValue(_modelName, Section, "BD1EdgeToEdgeUse", "True");
            mBendingArm.bBDEdgeToEdgeUse[0] = valStr == "True" ? true : false;

            valStr = ReadValue(_modelName, Section, "BD2EdgeToEdgeUse", "True");
            mBendingArm.bBDEdgeToEdgeUse[1] = valStr == "True" ? true : false;

            valStr = ReadValue(_modelName, Section, "BD3EdgeToEdgeUse", "True");
            mBendingArm.bBDEdgeToEdgeUse[2] = valStr == "True" ? true : false;

            valStr = ReadValue(_modelName, Section, "InspEdgeToEdgeUse", "True");
            mBendingArm.bInspEdgeToEdgeUse = valStr == "True" ? true : false;

            val = ReadValue(_modelName, Section, "BD1AddMoveRatio", 0.0);
            mSizeSpecRatio.BD1AddMoveRatio = val;

            val = ReadValue(_modelName, Section, "BD2AddMoveRatio", 0.0);
            mSizeSpecRatio.BD2AddMoveRatio = val;

            val = ReadValue(_modelName, Section, "BD3AddMoveRatio", 0.0);
            mSizeSpecRatio.BD3AddMoveRatio = val;

            valStr = ReadValue(_modelName, Section, "BDEdgeModeSearchMark", "True");
            mBendingArm.bBDEdgeModeSearchMark = valStr == "True" ? true : false;

            valStr = ReadValue(_modelName, Section, "InspEdgeModeSearchMark", "True");
            mBendingArm.bInspEdgeModeSearchMark = valStr == "True" ? true : false;

            //pcy190521
            valStr = ReadValue(_modelName, Section, "bInspFPCBSearchMark", "True");
            mBendingArm.bInspFPCBSearchMark = valStr == "True" ? true : false;

            val = ReadValue(_modelName, Section, "InspMode", 0);
            mBendingArm.iInspMode = (eInspMode)val;
            //switch (val)
            //{
            //    case 0: { mBendingArm.iInspMode = CONST.eInspMode.MarkToMark; break; }
            //    case 1: { mBendingArm.iInspMode = CONST.eInspMode.PanelMarkFPCBMark; break; }
            //    case 2: { mBendingArm.iInspMode = CONST.eInspMode.PanelEdgeFPCBMark; break; }
            //    case 3: { mBendingArm.iInspMode = CONST.eInspMode.PanelMarkFPCBEdge; break; }
            //    case 4: { mBendingArm.iInspMode = CONST.eInspMode.PanelEdgeFPCBEdge; break; }
            //}

            //pcy190718
            val = ReadValue(_modelName, Section, "InspFindSeq", 0);
            mBendingArm.iInspFindSeq = (CONST.eInspFindSeq)val;
            //switch (val)
            //{
            //    case 0: { mBendingArm.iInspFindSeq = CONST.eInspFindSeq.PanelFPCB; break; }
            //    case 1: { mBendingArm.iInspFindSeq = CONST.eInspFindSeq.FPCBPanel; break; }
            //}

            valStr = ReadValue(_modelName, Section, "Inspection3PointDistMeasure", "True");
            mBendingArm.bInspection3PointDistMeasrue = valStr == "True" ? true : false;

            val = ReadValue(_modelName, Section, "InspDistOffsetLX", 0.0);
            mBendingArm.bInspDistOffsetLX = val;

            val = ReadValue(_modelName, Section, "InspDistOffsetLY", 0.0);
            mBendingArm.bInspDistOffsetLY = val;

            val = ReadValue(_modelName, Section, "InspDistOffsetRX", 0.0);
            mBendingArm.bInspDistOffsetRX = val;

            val = ReadValue(_modelName, Section, "InspDistOffsetRY", 0.0);
            mBendingArm.bInspDistOffsetRY = val;

            //pcy200720
            val = ReadValue(_modelName, Section, "InspEdgeDistOffsetLX", 0.0);
            mBendingArm.bInspEdgeDistOffsetLX = val;
            val = ReadValue(_modelName, Section, "InspEdgeDistOffsetLY", 0.0);
            mBendingArm.bInspEdgeDistOffsetLY = val;
            val = ReadValue(_modelName, Section, "InspEdgeDistOffsetRX", 0.0);
            mBendingArm.bInspEdgeDistOffsetRX = val;
            val = ReadValue(_modelName, Section, "InspEdgeDistOffsetRY", 0.0);
            mBendingArm.bInspEdgeDistOffsetRY = val;

            val = ReadValue(_modelName, Section, "BDToleranceX", 0.0);
            mBendingArm.BDToleranceX = val;
            val = ReadValue(_modelName, Section, "BDToleranceY", 0.0);
            mBendingArm.BDToleranceY = val;

            val = ReadValue(_modelName, Section, "InspToleranceX", 0.0);
            mBendingArm.InspToleranceX = val;
            val = ReadValue(_modelName, Section, "InspToleranceY", 0.0);
            mBendingArm.InspToleranceY = val;

            val = ReadValue(_modelName, Section, "AttachToleranceX", 0.0);
            mSizeSpecRatio.AttachToleranceX = val;
            val = ReadValue(_modelName, Section, "AttachToleranceY", 0.0);
            mSizeSpecRatio.AttachToleranceY = val;

            val = ReadValue(_modelName, Section, "LaserPositionToleranceX", 0.0);
            mSizeSpecRatio.LaserPositionToleranceX = val;
            val = ReadValue(_modelName, Section, "LaserPositionToleranceY", 0.0);
            mSizeSpecRatio.LaserPositionToleranceY = val;

            val = ReadValue(_modelName, Section, "LaserMarkSizeX", 0.0);
            mSizeSpecRatio.LaserMarkSizeX = val;
            val = ReadValue(_modelName, Section, "LaserMarkSizeY", 0.0);
            mSizeSpecRatio.LaserMarkSizeY = val;

            val = ReadValue(_modelName, Section, "LaserAlignPosTor", 0.0);
            mSizeSpecRatio.LaserAlignPosTor = val;

            valStr = ReadValue(_modelName, Section, "BDResultGraphics", "True");
            mBendingArm.bBDResultGraphics = valStr == "True" ? true : false;

            valStr = ReadValue(_modelName, Section, "InspBeforeSubmarkUse", "false");
            mBendingArm.bInspBeforeSubmarkUse = valStr == "True" ? true : false;

            val = ReadValue(_modelName, Section, "BDLastSpecOffset", 0.0);
            mBendingArm.dBDLastSpecOffset = val;

            valStr = ReadValue(_modelName, Section, "BDLastSpecOffsetUse", "false");
            mBendingArm.bBDLastSpecOffsetUse = valStr == "True" ? true : false;

            val = ReadValue(_modelName, Section, "BDFirstInNoRetrySpec", 0.1);
            mBendingArm.dBDFirstInNoRetrySpec = val;

            valStr = ReadValue(_modelName, Section, "BDFirstInNoRetryUse", "false");
            mBendingArm.bBDFirstInNoRetryUse = valStr == "True" ? true : false;

            //pcy200627
            valStr = ReadValue(_modelName, Section, "BDFirstInNGUse", "false");
            mBendingArm.bBDFirstInNGUse = valStr == "True" ? true : false;

            val = ReadValue(_modelName, Section, "BDFirstInNGSpec", 0.5);
            mBendingArm.dBDFirstInNGSpec = val;

            //2019.07.03 LoadPre 박리 검사 
            val = ReadValue(_modelName, Section, "LDSCFInspOffsetX1", 0.0);
            mLoadPreInsp.SCFInspOffsetX1 = val;
            val = ReadValue(_modelName, Section, "LDSCFInspOffsetY1", 0.0);
            mLoadPreInsp.SCFInspOffsetY1 = val;
            val = ReadValue(_modelName, Section, "LDSCFInspOffsetX2", 0.0);
            mLoadPreInsp.SCFInspOffsetX2 = val;
            val = ReadValue(_modelName, Section, "LDSCFInspOffsetY2", 0.0);
            mLoadPreInsp.SCFInspOffsetY2 = val;
            val = ReadValue(_modelName, Section, "LDSCFInspTH1", 0.0);
            mLoadPreInsp.SCFInspTH1 = val;
            val = ReadValue(_modelName, Section, "LDSCFInspTH2", 0.0);
            mLoadPreInsp.SCFInspTH2 = val;

            val = ReadValue(_modelName, Section, "LDCOFInspOffsetX1", 0.0);
            mLoadPreInsp.COFInspOffsetX1 = val;
            val = ReadValue(_modelName, Section, "LDCOFInspOffsetY1", 0.0);
            mLoadPreInsp.COFInspOffsetY1 = val;
            val = ReadValue(_modelName, Section, "LDCOFInspOffsetX2", 0.0);
            mLoadPreInsp.COFInspOffsetX2 = val;
            val = ReadValue(_modelName, Section, "LDCOFInspOffsetY2", 0.0);
            mLoadPreInsp.COFInspOffsetY2 = val;
            val = ReadValue(_modelName, Section, "LDCOFInspTH1", 0.0);
            mLoadPreInsp.COFInspTH1 = val;
            val = ReadValue(_modelName, Section, "LDCOFInspTH2", 0.0);
            mLoadPreInsp.COFInspTH2 = val;

            valStr = ReadValue(_modelName, Section, "LDSCF1InspUse", "false");
            mLoadPreInsp.SCF1InspUse = valStr == "True" ? true : false;
            valStr = ReadValue(_modelName, Section, "LDSCF2InspUse", "false");
            mLoadPreInsp.SCF2InspUse = valStr == "True" ? true : false;
            valStr = ReadValue(_modelName, Section, "LDCOF1InspUse", "false");
            mLoadPreInsp.COF1InspUse = valStr == "True" ? true : false;
            valStr = ReadValue(_modelName, Section, "LDCOF2InspUse", "false");
            mLoadPreInsp.COF2InspUse = valStr == "True" ? true : false;

            //val = ReadValue(_modelName, Section, "BDScaleSpec", 0.0);
            //BDScaleSpec = val;
            //val = ReadValue(_modelName, Section, "INSPScaleSpec", 0.0);
            //INSPScaleSpec = val;

            val = ReadValue(_modelName, Section, "sSizeSpecRatio.SideInspRef", 0);
            mSizeSpecRatio.SideInspRef = val;
            val = ReadValue(_modelName, Section, "sSizeSpecRatio.SideInspSpec", -1);
            mSizeSpecRatio.SideInspSpec = val;

            Section = "Laser";
            val = ReadValue(_modelName, Section, "mLaser.MCRSearchKind", 0);
            if (val == 0) mLaser.MCRSearchKind = eID.DataMatrix;
            else mLaser.MCRSearchKind = eID.QRCode;

            val = ReadValue(_modelName, Section, "mLaser.refSearch", 0);
            if (val == 0) mLaser.refSearch = eRefSearch.Line;
            else if (val == 1) mLaser.refSearch = eRefSearch.Mark;
            else mLaser.refSearch = eRefSearch.Blob;

            val = ReadValue(_modelName, Section, "mLaser.inspKind", 0);
            if (val == 0) mLaser.inspKind = eInspKind.Camera;
            else mLaser.inspKind = eInspKind.Mark;

            val = ReadValue(_modelName, Section, "mLaser.blobMass", 1);
            if (val == 0) mLaser.blobMass = eBlobMass.Left;
            else mLaser.blobMass = eBlobMass.Right;

            val = ReadValue(_modelName, Section, "mLaser.blobPoint", 2);
            if (val == 0) mLaser.blobPoint = eBlobPoint.LeftUp;
            else if (val == 1) mLaser.blobPoint = eBlobPoint.RightUp;
            else if (val == 2) mLaser.blobPoint = eBlobPoint.LeftDown;
            else mLaser.blobPoint = eBlobPoint.RightDown;

            val = ReadValue(_modelName, Section, "mLaser.polarity", 1);
            if (val == 0) mLaser.polarity = ePolarity.Dark;
            else mLaser.polarity = ePolarity.Light;

            val = ReadValue(_modelName, Section, "mLaser.MinPixel", 100000);
            mLaser.MinPixel = (int)val;

            valStr = ReadValue(_modelName, Section, " mLaser.UseImageProcess", "false");
            mLaser.UseImageProcess = valStr == "True" ? true : false;

            valStr = ReadValue(_modelName, Section, " mLaser.MCRRight", "false");
            mLaser.MCRRight = valStr == "True" ? true : false;

            valStr = ReadValue(_modelName, Section, " mLaser.MCRUp", "false");
            mLaser.MCRUp = valStr == "True" ? true : false;
        }

        public void WriteData(string _modelName = "")
        {
            //double val = new double();

            string Section = "Light";
            //for(int i = 0; i < CONST.CAMCnt; i++)
            //{
            //    //WriteValue(_modelName, Section, "CAM" + i.ToString() + ".RefExposure", Menu.frmAutoMain.Vision[i].CFG.RefExposure);
            //    //WriteValue(_modelName, Section, "CAM" + i.ToString() + ".SubExposure", Menu.frmAutoMain.Vision[i].CFG.SubExposure);
            //    //WriteValue(_modelName, Section, "CAM" + i.ToString() + ".RefAutoLight", Menu.frmAutoMain.Vision[i].CFG.RefAutoLight);
            //    //WriteValue(_modelName, Section, "CAM" + i.ToString() + ".SubAutoLight", Menu.frmAutoMain.Vision[i].CFG.SubAutoLight);
            //    //WriteValue(_modelName, Section, "CAM" + i.ToString() + ".EdgeAutoLight", Menu.frmAutoMain.Vision[i].CFG.EdgeAutoLight);
            //    //WriteValue(_modelName, Section, "CAM" + i.ToString() + ".EdgeLineAutoLight", Menu.frmAutoMain.Vision[i].CFG.EdgeSubAutoLight);
            //    ////pcy190521
            //    //WriteValue(_modelName, Section, "CAM" + i.ToString() + ".FPCBAutoLight", Menu.frmAutoMain.Vision[i].CFG.FPCBAutoLight);
            //    //WriteValue(_modelName, Section, "CAM" + i.ToString() + ".FPCBSubAutoLight", Menu.frmAutoMain.Vision[i].CFG.FPCBSubAutoLight);

            //    ////190624 cjm
            //    //WriteValue(_modelName, Section, "CAM" + i.ToString() + ".Contrast", Menu.frmAutoMain.Vision[i].CFG.Contrast);
            //    //WriteValue(_modelName, Section, "CAM" + i.ToString() + ".SubContrast", Menu.frmAutoMain.Vision[i].CFG.SubContrast);

            //    foreach (var s in Enum.GetValues(typeof(ePatternKind)))
            //    {
            //        WriteValue(_modelName, Section, "CAM" + i.ToString() + ".Light." + s, Menu.frmAutoMain.Vision[i].CFG.Light[(int)s]);
            //        WriteValue(_modelName, Section, "CAM" + i.ToString() + ".Exposure." + s, Menu.frmAutoMain.Vision[i].CFG.Exposure[(int)s]);
            //        WriteValue(_modelName, Section, "CAM" + i.ToString() + ".Contrast." + s, Menu.frmAutoMain.Vision[i].CFG.Contrast[(int)s]);
            //    }
            //    foreach (var s in Enum.GetValues(typeof(eLineKind)))
            //    {
            //        WriteValue(_modelName, Section, "CAM" + i.ToString() + ".Light." + s, Menu.frmAutoMain.Vision[i].CFG.Light[(int)s]);
            //        WriteValue(_modelName, Section, "CAM" + i.ToString() + ".Exposure." + s, Menu.frmAutoMain.Vision[i].CFG.Exposure[(int)s]);
            //        WriteValue(_modelName, Section, "CAM" + i.ToString() + ".Contrast." + s, Menu.frmAutoMain.Vision[i].CFG.Contrast[(int)s]);
            //    }
            //}

            Section = "Offset";
            for (int i = 0; i < mOffset.Length; i++)
            {
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".PressOffsetY1", mOffset[i].PressOffsetY1);
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".PressOffsetY2", mOffset[i].PressOffsetY2);
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".CalXYT.X", mOffset[i].CalXYT.X);
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".CalXYT.Y", mOffset[i].CalXYT.Y);
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".CalXYT.T", mOffset[i].CalXYT.T);

                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".Offset1XYT.X", mOffset[i].AlignOffsetXYT.X);
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".Offset1XYT.Y", mOffset[i].AlignOffsetXYT.Y);
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".Offset1XYT.T", mOffset[i].AlignOffsetXYT.T);

                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".Offset2XYT.X", mOffset[i].AlignOffsetXYT2.X);
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".Offset2XYT.Y", mOffset[i].AlignOffsetXYT2.Y);
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".Offset2XYT.T", mOffset[i].AlignOffsetXYT2.T);

                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".BendingPreOffsetXYT.X", mOffset[i].BendingPreOffsetXYT.X);
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".BendingPreOffsetXYT.Y", mOffset[i].BendingPreOffsetXYT.Y);
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".BendingPreOffsetXYT.T", mOffset[i].BendingPreOffsetXYT.T);

                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".CalFixPosOffset.X", mOffset[i].CalFixPosOffset.X);
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".CalFixPosOffset.Y", mOffset[i].CalFixPosOffset.Y);


            }

            //20.12.17 lkw DL
            Section = "DL";
            for (int i = 0; i < mDL.Length; i++)
            {
                for (int j = 0; j < mDL[i].MarkSearch_Use.Length; j++)
                {
                    WriteValue(_modelName, Section, "CAM" + i.ToString() + ".MarkSearch_USE" + j.ToString(), mDL[i].MarkSearch_Use[j].ToString());
                    WriteValue(_modelName, Section, "CAM" + i.ToString() + ".MarkSearch_LabelPath" + j.ToString(), mDL[i].MarkSearch_LabelPath[j]);
                    WriteValue(_modelName, Section, "CAM" + i.ToString() + ".MarkSearch_ModelPath" + j.ToString(), mDL[i].MarkSearch_ModelPath[j]);
                }

                for (int j = 0; j < mDL[i].DefectFind_Use.Length; j++)
                {
                    WriteValue(_modelName, Section, "CAM" + i.ToString() + ".DefectFind_USE" + j.ToString(), mDL[i].DefectFind_Use[j].ToString());
                    WriteValue(_modelName, Section, "CAM" + i.ToString() + ".DefectFind_LabelPath" + j.ToString(), mDL[i].DefectFind_LabelPath[j]);
                    WriteValue(_modelName, Section, "CAM" + i.ToString() + ".DefectFind_ModelPath" + j.ToString(), mDL[i].DefectFind_ModelPath[j]);
                }
            }

            Section = "BendingOffset";
            for (int i = 0; i < mOffset.Length; i++)
            {
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".FirstOffsetYY.Y1", mOffset[i].FirstOffsetYY.Y1);
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".FirstOffsetYY.Y2", mOffset[i].FirstOffsetYY.Y2);

                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".LastOffsetYY.Y1", mOffset[i].LastOffsetYY.Y1);
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".LastOffsetYY.Y2", mOffset[i].LastOffsetYY.Y2);

                //EdgeToEdge Scale
                //WriteValue(_modelName, Section, "CAM" + i.ToString() + ".EdgeToEdgeScale.X1", mOffset[i].EgdeToEdgeScale.X1);
                //WriteValue(_modelName, Section, "CAM" + i.ToString() + ".EdgeToEdgeScale.Y1", mOffset[i].EgdeToEdgeScale.Y1);
                //WriteValue(_modelName, Section, "CAM" + i.ToString() + ".EdgeToEdgeScale.X2", mOffset[i].EgdeToEdgeScale.X2);
                //WriteValue(_modelName, Section, "CAM" + i.ToString() + ".EdgeToEdgeScale.Y2", mOffset[i].EgdeToEdgeScale.Y2);

                //WriteValue(_modelName, Section, "CAM" + i.ToString() + ".Scale.X1", mOffset[i].Scale.X1);
                //WriteValue(_modelName, Section, "CAM" + i.ToString() + ".Scale.Y1", mOffset[i].Scale.Y1);
                //WriteValue(_modelName, Section, "CAM" + i.ToString() + ".Scale.X2", mOffset[i].Scale.X2);
                //WriteValue(_modelName, Section, "CAM" + i.ToString() + ".Scale.Y2", mOffset[i].Scale.Y2);

                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".MoveAdd.X", mOffset[i].MoveAdd.X);
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".MoveAdd.Y", mOffset[i].MoveAdd.Y);
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".MoveAdd.T", mOffset[i].MoveAdd.T);

                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".OnceMoveLimit.X", mOffset[i].OnceMoveLimit.X);
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".OnceMoveLimit.Y", mOffset[i].OnceMoveLimit.Y);
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".OnceMoveLimit.T", mOffset[i].OnceMoveLimit.T);

            }

            Section = "Target";
            for (int j = 0; j < Menu.frmAutoMain.Vision.Length; j++)
            {
                for (int i = 0; i < Menu.frmAutoMain.Vision[0].CFG.TargetX.Length; i++)
                {
                    WriteValue(_modelName, Section, "CAM" + Menu.frmAutoMain.Vision[j].CFG.Camno + ".TargetX" + i, Menu.frmAutoMain.Vision[j].CFG.TargetX[i]);
                    WriteValue(_modelName, Section, "CAM" + Menu.frmAutoMain.Vision[j].CFG.Camno + ".TargetY" + i, Menu.frmAutoMain.Vision[j].CFG.TargetY[i]);
                    WriteValue(_modelName, Section, "CAM" + Menu.frmAutoMain.Vision[j].CFG.Camno + ".TargetX2" + i, Menu.frmAutoMain.Vision[j].CFG.TargetX2[i]);
                    WriteValue(_modelName, Section, "CAM" + Menu.frmAutoMain.Vision[j].CFG.Camno + ".TargetY2" + i, Menu.frmAutoMain.Vision[j].CFG.TargetY2[i]);
                }
            }

            Section = "LCheck";
            for (int i = 0; i < mLcheck.Length; i++)
            {
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".LCheckOffset1", mLcheck[i].LCheckOffset1);
                WriteValue(_modelName, Section, "CAM" + i.ToString() + ".LCheckOffset2", mLcheck[i].LCheckOffset2);
            }

            //PC1Model
            Section = "PC1Model";
            WriteValue(_modelName, Section, "AttachRetryUse", mAttach.AttachRetryUse.ToString());

            WriteValue(_modelName, Section, "DetachOffsetX1_1", mAttach.DetachOffsetX1_1.ToString());
            WriteValue(_modelName, Section, "DetachOffsetY1_1", mAttach.DetachOffsetY1_1.ToString());
            WriteValue(_modelName, Section, "DetachLimitTH1_1", mAttach.DetachLimitTH1_1.ToString());
            WriteValue(_modelName, Section, "DetachOffsetX1_2", mAttach.DetachOffsetX1_2.ToString());
            WriteValue(_modelName, Section, "DetachOffsetY1_2", mAttach.DetachOffsetY1_2.ToString());
            WriteValue(_modelName, Section, "DetachLimitTH1_2", mAttach.DetachLimitTH1_2.ToString());


            WriteValue(_modelName, Section, "BDPreSCFInspection", mBendingPre.BDPreSCFInspection.ToString());
            WriteValue(_modelName, Section, "BDPreSCFDistInspection", mBendingPre.SCFDistInspection.ToString());
            WriteValue(_modelName, Section, "SCFLengthSpec", mSizeSpecRatio.SCFLengthSpec.ToString());

            WriteValue(_modelName, Section, "BDPreSCFInspOffsetX", mSizeSpecRatio.BDPreSCFCheckOffsetX.ToString());
            WriteValue(_modelName, Section, "BDPreSCFInspOffsetY", mSizeSpecRatio.BDPreSCFCheckOffsetY.ToString());
            WriteValue(_modelName, Section, "BDPreSCFInspTH", mSizeSpecRatio.BDPreSCFAttachTH.ToString());

            WriteValue(_modelName, Section, "BDPreSCFMarkToEdgeY1", mBendingPre.BDPreSCFMarkToEdgeY1.ToString());
            WriteValue(_modelName, Section, "BDPreSCFMarkToEdgeY2", mBendingPre.BDPreSCFMarkToEdgeY2.ToString());

            WriteValue(_modelName, Section, "BDPreSCFSearchLineOffsetX1", mBendingPre.BDPreSCFSearchLineOffsetX1.ToString());
            WriteValue(_modelName, Section, "BDPreSCFSearchLineOffsetY1", mBendingPre.BDPreSCFSearchLineOffsetY1.ToString());
            WriteValue(_modelName, Section, "BDPreSCFSearchLineOffsetX2", mBendingPre.BDPreSCFSearchLineOffsetX2.ToString());
            WriteValue(_modelName, Section, "BDPreSCFSearchLineOffsetY2", mBendingPre.BDPreSCFSearchLineOffsetY2.ToString());

            WriteValue(_modelName, Section, "SCFTolerance", mBendingPre.SCFTolerance.ToString());

            WriteValue(_modelName, Section, "BDPreSCFInspResultPass", mBendingPre.BDPreSCFInspResultPass.ToString());
            //2018.11.29 khs
            WriteValue(_modelName, Section, "LoadPreLineSearchMode", mbUseFindLine.bLoadPre.ToString());
            WriteValue(_modelName, Section, "LDPreLineThetaUse", mbUseFindLine.bLoadPreTheta.ToString());

            WriteValue(_modelName, Section, "LoadPreTagetPosRotation", mSizeSpecRatio.LDTagetPosRot.ToString());

            WriteValue(_modelName, Section, "EMILogPatternSearch", mBendingPre.EMILogPatternSearch.ToString());

            WriteValue(_modelName, Section, "EMITCalXMoveValue", mSizeSpecRatio.EMITCalXMoveValue.ToString());
            WriteValue(_modelName, Section, "EMITCalYMoveValue", mSizeSpecRatio.EMITCalYMoveValue.ToString());

            //2019.07.20 EMI Align 추가
            WriteValue(_modelName, Section, "plusYMoveRatio", mSizeSpecRatio.plusYMoveRatio.ToString());
            WriteValue(_modelName, Section, "plusTMoveRatio", mSizeSpecRatio.plusTMoveRatio.ToString());
            WriteValue(_modelName, Section, "minusYMoveRatio", mSizeSpecRatio.minusYMoveRatio.ToString());
            WriteValue(_modelName, Section, "minusTMoveRatio", mSizeSpecRatio.minusTMoveRatio.ToString());

            WriteValue(_modelName, Section, "SCFReelPickUpMarkPosOffsetX1", mSizeSpecRatio.SCFReelPickUpMarkPosOffsetX1.ToString());
            WriteValue(_modelName, Section, "SCFReelPickUpMarkPosOffsetY1", mSizeSpecRatio.SCFReelPickUpMarkPosOffsetY1.ToString());
            WriteValue(_modelName, Section, "SCFReelPickUpMarkPosOffsetX2", mSizeSpecRatio.SCFReelPickUpMarkPosOffsetX2.ToString());
            WriteValue(_modelName, Section, "SCFReelPickUpMarkPosOffsetY2", mSizeSpecRatio.SCFReelPickUpMarkPosOffsetY2.ToString());
            WriteValue(_modelName, Section, "SCFCenterAlign", mSizeSpecRatio.SCFCenterAlign.ToString());
            WriteValue(_modelName, Section, "SCFXShiftRatio", mSizeSpecRatio.SCFXShiftRatio.ToString());
            WriteValue(_modelName, Section, "SCFYShiftRatio", mSizeSpecRatio.SCFYShiftRatio.ToString());

            ////20200919 cjm Bending Pre SCF Inspection offset 추가
            WriteValue(_modelName, Section, "SCFExistOffsetX1", mBendingPre.SCFExistOffsetX1.ToString());
            WriteValue(_modelName, Section, "SCFExistOffsetY1", mBendingPre.SCFExistOffsetY1.ToString());
            WriteValue(_modelName, Section, "SCFExistTH1", mBendingPre.SCFExistTH1.ToString());
            WriteValue(_modelName, Section, "SCFExistOffsetX2", mBendingPre.SCFExistOffsetX2.ToString());
            WriteValue(_modelName, Section, "SCFExistOffsetY2", mBendingPre.SCFExistOffsetY2.ToString());
            WriteValue(_modelName, Section, "SCFExistTH2", mBendingPre.SCFExistTH2.ToString());
            WriteValue(_modelName, Section, "SCFExistOffsetX3", mBendingPre.SCFExistOffsetX3.ToString());
            WriteValue(_modelName, Section, "SCFExistOffsetY3", mBendingPre.SCFExistOffsetY3.ToString());
            WriteValue(_modelName, Section, "SCFExistTH3", mBendingPre.SCFExistTH3.ToString());

            //20200926 cjm Bending Pre SCF Inspection offset 추가
            WriteValue(_modelName, Section, "SCFInspCam1OffsetX1", mBendingPre.SCFInspCam1OffsetX1.ToString());
            WriteValue(_modelName, Section, "SCFInspCam1OffsetY1", mBendingPre.SCFInspCam1OffsetY1.ToString());
            WriteValue(_modelName, Section, "SCFInspCam1OffsetX2", mBendingPre.SCFInspCam1OffsetX2.ToString());
            WriteValue(_modelName, Section, "SCFInspCam1OffsetY2", mBendingPre.SCFInspCam1OffsetY2.ToString());
            WriteValue(_modelName, Section, "SCFInspCam1TH", mBendingPre.SCFInspCam1TH.ToString());
            WriteValue(_modelName, Section, "SCFInspCam1TH2", mBendingPre.SCFInspCam1TH2.ToString());

            WriteValue(_modelName, Section, "SCFInspCam2OffsetX1", mBendingPre.SCFInspCam2OffsetX1.ToString());
            WriteValue(_modelName, Section, "SCFInspCam2OffsetY1", mBendingPre.SCFInspCam2OffsetY1.ToString());
            WriteValue(_modelName, Section, "SCFInspCam2OffsetX2", mBendingPre.SCFInspCam2OffsetX2.ToString());
            WriteValue(_modelName, Section, "SCFInspCam2OffsetY2", mBendingPre.SCFInspCam2OffsetY2.ToString());
            WriteValue(_modelName, Section, "SCFInspCam2TH", mBendingPre.SCFInspCam2TH.ToString());
            WriteValue(_modelName, Section, "SCFInspCam2TH2", mBendingPre.SCFInspCam2TH2.ToString());

            WriteValue(_modelName, Section, "SCFInspCam3OffsetX1", mBendingPre.SCFInspCam3OffsetX1.ToString());
            WriteValue(_modelName, Section, "SCFInspCam3OffsetY1", mBendingPre.SCFInspCam3OffsetY1.ToString());
            WriteValue(_modelName, Section, "SCFInspCam3OffsetX2", mBendingPre.SCFInspCam3OffsetX2.ToString());
            WriteValue(_modelName, Section, "SCFInspCam3OffsetY2", mBendingPre.SCFInspCam3OffsetY2.ToString());
            WriteValue(_modelName, Section, "SCFInspCam3TH", mBendingPre.SCFInspCam3TH.ToString());
            WriteValue(_modelName, Section, "SCFInspCam3TH2", mBendingPre.SCFInspCam3TH2.ToString());

            WriteValue(_modelName, Section, "SCFInspCam3InRadius", mBendingPre.SCFInspCam3InRadius.ToString());
            WriteValue(_modelName, Section, "SCFInspCam3OutRadius", mBendingPre.SCFInspCam3OutRadius.ToString());
            WriteValue(_modelName, Section, "SCFInspCam3InSearchLength", mBendingPre.SCFInspCam3InSearchLength.ToString());
            WriteValue(_modelName, Section, "SCFInspCam3OutSearchLength", mBendingPre.SCFInspCam3OutSearchLength.ToString());

            WriteValue(_modelName, Section, "SCFInspCam3CalipherCount", mBendingPre.SCFInspCam3CalipherCount.ToString());
            WriteValue(_modelName, Section, "SCFInspCam3IgnoreCount", mBendingPre.SCFInspCam3IgnoreCount.ToString());
            WriteValue(_modelName, Section, "SCFInspCam3InFind", mBendingPre.SCFInspCam3InFind.ToString());
            WriteValue(_modelName, Section, "SCFInspCam3OutFind", mBendingPre.SCFInspCam3OutFind.ToString());
            WriteValue(_modelName, Section, "SCFInspCam3InPolarity", mBendingPre.SCFInspCam3InPolarity.ToString());
            WriteValue(_modelName, Section, "SCFInspCam3OutPolarity", mBendingPre.SCFInspCam3OutPolarity.ToString());
            WriteValue(_modelName, Section, "SCFInspCam3InThreshold", mBendingPre.SCFInspCam3InThreshold.ToString());
            WriteValue(_modelName, Section, "SCFInspCam3OutThreshold", mBendingPre.SCFInspCam3OutThreshold.ToString());

            WriteValue(_modelName, Section, "DivideLimitX", mConveyor.DivideLimitX.ToString());
            WriteValue(_modelName, Section, "DivideLimitY", mConveyor.DivideLimitY.ToString());
            WriteValue(_modelName, Section, "DivideLimitT", mConveyor.DivideLimitT.ToString());

            //DetachInspection
            Section = "DetachInspection";
            WriteValue(_modelName, Section, "DetachOffsetX1", mLoadPreInsp.DetachOffsetX1.ToString());
            WriteValue(_modelName, Section, "DetachOffsetY1", mLoadPreInsp.DetachOffsetY1.ToString());
            WriteValue(_modelName, Section, "DetachLimitTH1", mLoadPreInsp.DetachLimitTH1.ToString());
            WriteValue(_modelName, Section, "DetachOffsetX2", mLoadPreInsp.DetachOffsetX2.ToString());
            WriteValue(_modelName, Section, "DetachOffsetY2", mLoadPreInsp.DetachOffsetY2.ToString());
            WriteValue(_modelName, Section, "DetachLimitTH2", mLoadPreInsp.DetachLimitTH2.ToString());

            WriteValue(_modelName, Section, "DetachOffsetX3", mLoadPreInsp.DetachOffsetX3.ToString());
            WriteValue(_modelName, Section, "DetachOffsetY3", mLoadPreInsp.DetachOffsetY3.ToString());
            WriteValue(_modelName, Section, "DetachLimitTH3", mLoadPreInsp.DetachLimitTH3.ToString());
            WriteValue(_modelName, Section, "DetachOffsetX4", mLoadPreInsp.DetachOffsetX4.ToString());
            WriteValue(_modelName, Section, "DetachOffsetY4", mLoadPreInsp.DetachOffsetY4.ToString());
            WriteValue(_modelName, Section, "DetachLimitTH4", mLoadPreInsp.DetachLimitTH4.ToString());

            WriteValue(_modelName, Section, "AttachOffsetX1", mLoadPreInsp.AttachOffsetX1.ToString());
            WriteValue(_modelName, Section, "AttachOffsetY1", mLoadPreInsp.AttachOffsetY1.ToString());
            WriteValue(_modelName, Section, "AttachLimitTH1", mLoadPreInsp.AttachLimitTH1.ToString());
            WriteValue(_modelName, Section, "AttachOffsetX2", mLoadPreInsp.AttachOffsetX2.ToString());
            WriteValue(_modelName, Section, "AttachOffsetY2", mLoadPreInsp.AttachOffsetY2.ToString());
            WriteValue(_modelName, Section, "AttachLimitTH2", mLoadPreInsp.AttachLimitTH2.ToString());

            WriteValue(_modelName, Section, "AttachOffsetX3", mLoadPreInsp.AttachOffsetX3.ToString());
            WriteValue(_modelName, Section, "AttachOffsetY3", mLoadPreInsp.AttachOffsetY3.ToString());
            WriteValue(_modelName, Section, "AttachLimitTH3", mLoadPreInsp.AttachLimitTH3.ToString());
            WriteValue(_modelName, Section, "AttachOffsetX4", mLoadPreInsp.AttachOffsetX4.ToString());
            WriteValue(_modelName, Section, "AttachOffsetY4", mLoadPreInsp.AttachOffsetY4.ToString());
            WriteValue(_modelName, Section, "AttachLimitTH4", mLoadPreInsp.AttachLimitTH4.ToString());

            WriteValue(_modelName, Section, "CheckWhiteDetach1", mLoadPreInsp.CheckWhiteDetach1.ToString());
            WriteValue(_modelName, Section, "CheckWhiteDetach2", mLoadPreInsp.CheckWhiteDetach2.ToString());
            WriteValue(_modelName, Section, "CheckWhiteDetach3", mLoadPreInsp.CheckWhiteDetach3.ToString());
            WriteValue(_modelName, Section, "CheckWhiteDetach4", mLoadPreInsp.CheckWhiteDetach4.ToString());

            WriteValue(_modelName, Section, "CheckWhiteAttach1", mLoadPreInsp.CheckWhiteAttach1.ToString());
            WriteValue(_modelName, Section, "CheckWhiteAttach2", mLoadPreInsp.CheckWhiteAttach2.ToString());
            WriteValue(_modelName, Section, "CheckWhiteAttach3", mLoadPreInsp.CheckWhiteAttach3.ToString());
            WriteValue(_modelName, Section, "CheckWhiteAttach4", mLoadPreInsp.CheckWhiteAttach4.ToString());

            //PC2Model
            Section = "PC2Model";

            WriteValue(_modelName, Section, "DetachOffsetX2_1", mAttach.DetachOffsetX2_1.ToString());
            WriteValue(_modelName, Section, "DetachOffsetY2_1", mAttach.DetachOffsetY2_1.ToString());
            WriteValue(_modelName, Section, "DetachLimitTH2_1", mAttach.DetachLimitTH2_1.ToString());
            WriteValue(_modelName, Section, "DetachOffsetX2_2", mAttach.DetachOffsetX2_2.ToString());
            WriteValue(_modelName, Section, "DetachOffsetY2_2", mAttach.DetachOffsetY2_2.ToString());
            WriteValue(_modelName, Section, "DetachLimitTH2_2", mAttach.DetachLimitTH2_2.ToString());


            WriteValue(_modelName, Section, "BDTrans1StartGrabDelay", mStartGrabDelay.BDTrans1);
            WriteValue(_modelName, Section, "BDTrans2StartGrabDelay", mStartGrabDelay.BDTrans2);
            WriteValue(_modelName, Section, "BDTrans3StartGrabDelay", mStartGrabDelay.BDTrans3);
            WriteValue(_modelName, Section, "BDArm1StartGrabDelay", mStartGrabDelay.BDArm1);
            WriteValue(_modelName, Section, "BDArm2StartGrabDelay", mStartGrabDelay.BDArm2);
            WriteValue(_modelName, Section, "BDArm3StartGrabDelay", mStartGrabDelay.BDArm3);
            WriteValue(_modelName, Section, "bInitialPanelMarkSearchPass", mBendingArm.bInitialPanelMarkSearchPass.ToString());
            WriteValue(_modelName, Section, "b180PanelMarkSearchPass", mBendingArm.b180PanelMarkSearchPass.ToString());
            //WriteValue(_modelName, Section, "BD3RefMarkSearchPass", mBendingArm.bRefMarkSearchPass[2].ToString());
            WriteValue(_modelName, Section, "BDSCFInspection", mBendingArm.bSCFInspection.ToString());
            WriteValue(_modelName, Section, "BDSCFDistInspection", mBendingArm.bSCFDistInspection.ToString());
            //WriteValue(_modelName, Section, "BDEdgeRefPatternUse", mBendingArm.bEdgeBDRefPatternUse.ToString());

            WriteValue(_modelName, Section, "bDisplayLRSwapBD", mBendingArm.bDisplayLRSwapBD.ToString());
            WriteValue(_modelName, Section, "bDisplayLRSwapINSP", mBendingArm.bDisplayLRSwapINSP.ToString());
            WriteValue(_modelName, Section, "bBDUseLRSwap", mBendingArm.bBDUseLRSwap.ToString());

            //SCF Inspection 위치 offset 추가
            WriteValue(_modelName, Section, "SCFInspX", mSizeSpecRatio.BDSCFCheckOffsetX.ToString());
            WriteValue(_modelName, Section, "SCFInspY", mSizeSpecRatio.BDSCFCheckOffsetY.ToString());
            WriteValue(_modelName, Section, "SCFInspTH", mSizeSpecRatio.BDSCFAttachTH.ToString());

            WriteValue(_modelName, Section, "BD1EdgeToEdgeUse", mBendingArm.bBDEdgeToEdgeUse[0].ToString());
            WriteValue(_modelName, Section, "BD2EdgeToEdgeUse", mBendingArm.bBDEdgeToEdgeUse[1].ToString());
            WriteValue(_modelName, Section, "BD3EdgeToEdgeUse", mBendingArm.bBDEdgeToEdgeUse[2].ToString());

            WriteValue(_modelName, Section, "InspEdgeToEdgeUse", mBendingArm.bInspEdgeToEdgeUse.ToString());

            WriteValue(_modelName, Section, "BD1AddMoveRatio", mSizeSpecRatio.BD1AddMoveRatio.ToString());
            WriteValue(_modelName, Section, "BD2AddMoveRatio", mSizeSpecRatio.BD2AddMoveRatio.ToString());
            WriteValue(_modelName, Section, "BD3AddMoveRatio", mSizeSpecRatio.BD3AddMoveRatio.ToString());

            WriteValue(_modelName, Section, "BDEdgeModeSearchMark", mBendingArm.bBDEdgeModeSearchMark.ToString());

            WriteValue(_modelName, Section, "InspEdgeModeSearchMark", mBendingArm.bInspEdgeModeSearchMark.ToString());
            //pcy190521
            WriteValue(_modelName, Section, "bInspFPCBSearchMark", mBendingArm.bInspFPCBSearchMark.ToString());


            WriteValue(_modelName, Section, "InspMode", (int)mBendingArm.iInspMode);
            //switch (mBendingArm.iInspMode)
            //{
            //    case eInspMode.PanelMarkFPCBMark: { WriteValue(_modelName, Section, "InspMode", 0); break; }
            //    case eInspMode.PanelEdgeFPCBMark: { WriteValue(_modelName, Section, "InspMode", 1); break; }
            //    case eInspMode.PanelMarkFPCBEdge: { WriteValue(_modelName, Section, "InspMode", 2); break; }
            //    case eInspMode.PanelEdgeFPCBEdge: { WriteValue(_modelName, Section, "InspMode", 3); break; }
            //}

            //pcy190718
            WriteValue(_modelName, Section, "InspFindSeq", (int)mBendingArm.iInspFindSeq);
            //switch (mBendingArm.iInspFindSeq)
            //{
            //    case CONST.eInspFindSeq.PanelFPCB: { WriteValue(_modelName, Section, "InspFindSeq", 0); break; }
            //    case CONST.eInspFindSeq.FPCBPanel: { WriteValue(_modelName, Section, "InspFindSeq", 1); break; }
            //}

            WriteValue(_modelName, Section, "Inspection3PointDistMeasure", mBendingArm.bInspection3PointDistMeasrue.ToString());

            WriteValue(_modelName, Section, "InspDistOffsetLX", mBendingArm.bInspDistOffsetLX.ToString());
            WriteValue(_modelName, Section, "InspDistOffsetLY", mBendingArm.bInspDistOffsetLY.ToString());
            WriteValue(_modelName, Section, "InspDistOffsetRX", mBendingArm.bInspDistOffsetRX.ToString());
            WriteValue(_modelName, Section, "InspDistOffsetRY", mBendingArm.bInspDistOffsetRY.ToString());

            WriteValue(_modelName, Section, "BDToleranceX", mBendingArm.BDToleranceX.ToString());
            WriteValue(_modelName, Section, "BDToleranceY", mBendingArm.BDToleranceY.ToString());
            WriteValue(_modelName, Section, "InspToleranceX", mBendingArm.InspToleranceX.ToString());
            WriteValue(_modelName, Section, "InspToleranceY", mBendingArm.InspToleranceY.ToString());
            WriteValue(_modelName, Section, "AttachToleranceX", mSizeSpecRatio.AttachToleranceX.ToString());
            WriteValue(_modelName, Section, "AttachToleranceY", mSizeSpecRatio.AttachToleranceY.ToString());

            WriteValue(_modelName, Section, "LaserPositionToleranceX", mSizeSpecRatio.LaserPositionToleranceX.ToString());
            WriteValue(_modelName, Section, "LaserPositionToleranceY", mSizeSpecRatio.LaserPositionToleranceY.ToString());

            WriteValue(_modelName, Section, "LaserMarkSizeX", mSizeSpecRatio.LaserMarkSizeX.ToString());
            WriteValue(_modelName, Section, "LaserMarkSizeY", mSizeSpecRatio.LaserMarkSizeY.ToString());

            WriteValue(_modelName, Section, "LaserAlignPosTor", mSizeSpecRatio.LaserAlignPosTor.ToString());


            //pcy200720
            WriteValue(_modelName, Section, "InspEdgeDistOffsetLX", mBendingArm.bInspEdgeDistOffsetLX.ToString());
            WriteValue(_modelName, Section, "InspEdgeDistOffsetLY", mBendingArm.bInspEdgeDistOffsetLY.ToString());
            WriteValue(_modelName, Section, "InspEdgeDistOffsetRX", mBendingArm.bInspEdgeDistOffsetRX.ToString());
            WriteValue(_modelName, Section, "InspEdgeDistOffsetRY", mBendingArm.bInspEdgeDistOffsetRY.ToString());

            WriteValue(_modelName, Section, "BDResultGraphics", mBendingArm.bBDResultGraphics.ToString());
            WriteValue(_modelName, Section, "InspBeforeSubmarkUse", mBendingArm.bInspBeforeSubmarkUse.ToString());

            WriteValue(_modelName, Section, "BDLastSpecOffset", mBendingArm.dBDLastSpecOffset.ToString());
            WriteValue(_modelName, Section, "BDLastSpecOffsetUse", mBendingArm.bBDLastSpecOffsetUse.ToString());

            WriteValue(_modelName, Section, "BDFirstInNoRetrySpec", mBendingArm.dBDFirstInNoRetrySpec.ToString());
            WriteValue(_modelName, Section, "BDFirstInNoRetryUse", mBendingArm.bBDFirstInNoRetryUse.ToString());

            //pcy200627
            WriteValue(_modelName, Section, "BDFirstInNGSpec", mBendingArm.dBDFirstInNGSpec.ToString());
            WriteValue(_modelName, Section, "BDFirstInNGUse", mBendingArm.bBDFirstInNGUse.ToString());


            //2019.07.03 LoadPre 박리 검사 
            WriteValue(_modelName, Section, "LDSCFInspOffsetX1", mLoadPreInsp.SCFInspOffsetX1.ToString());
            WriteValue(_modelName, Section, "LDSCFInspOffsetY1", mLoadPreInsp.SCFInspOffsetY1.ToString());
            WriteValue(_modelName, Section, "LDSCFInspOffsetX2", mLoadPreInsp.SCFInspOffsetX2.ToString());
            WriteValue(_modelName, Section, "LDSCFInspOffsetY2", mLoadPreInsp.SCFInspOffsetY2.ToString());
            WriteValue(_modelName, Section, "LDSCFInspTH1", mLoadPreInsp.SCFInspTH1.ToString());
            WriteValue(_modelName, Section, "LDSCFInspTH2", mLoadPreInsp.SCFInspTH2.ToString());

            WriteValue(_modelName, Section, "LDCOFInspOffsetX1", mLoadPreInsp.COFInspOffsetX1.ToString());
            WriteValue(_modelName, Section, "LDCOFInspOffsetY1", mLoadPreInsp.COFInspOffsetY1.ToString());
            WriteValue(_modelName, Section, "LDCOFInspOffsetX2", mLoadPreInsp.COFInspOffsetX2.ToString());
            WriteValue(_modelName, Section, "LDCOFInspOffsetY2", mLoadPreInsp.COFInspOffsetY2.ToString());
            WriteValue(_modelName, Section, "LDCOFInspTH1", mLoadPreInsp.COFInspTH1.ToString());
            WriteValue(_modelName, Section, "LDCOFInspTH2", mLoadPreInsp.COFInspTH2.ToString());

            WriteValue(_modelName, Section, "LDSCF1InspUse", mLoadPreInsp.SCF1InspUse.ToString());
            WriteValue(_modelName, Section, "LDSCF2InspUse", mLoadPreInsp.SCF2InspUse.ToString());
            WriteValue(_modelName, Section, "LDCOF1InspUse", mLoadPreInsp.COF1InspUse.ToString());
            WriteValue(_modelName, Section, "LDCOF2InspUse", mLoadPreInsp.COF2InspUse.ToString());

            WriteValue(_modelName, Section, "sSizeSpecRatio.SideInspSpec", mSizeSpecRatio.SideInspSpec);
            WriteValue(_modelName, Section, "sSizeSpecRatio.SideInspRef", mSizeSpecRatio.SideInspRef);

            Section = "Laser";
            WriteValue(_modelName, Section, "mLaser.MCRSearchKind", (int)mLaser.MCRSearchKind);
            WriteValue(_modelName, Section, "mLaser.refSearch", (int)mLaser.refSearch);
            WriteValue(_modelName, Section, "mLaser.inspKind", (int)mLaser.inspKind);

            WriteValue(_modelName, Section, "mLaser.blobMass", (int)mLaser.blobMass);
            WriteValue(_modelName, Section, "mLaser.blobPoint", (int)mLaser.blobPoint);

            WriteValue(_modelName, Section, "mLaser.polarity", (int)mLaser.polarity);
            WriteValue(_modelName, Section, "mLaser.MinPixel", mLaser.MinPixel);

            WriteValue(_modelName, Section, "mLaser.UseImageProcess", mLaser.UseImageProcess.ToString());
            WriteValue(_modelName, Section, "mLaser.MCRRight", mLaser.MCRRight.ToString());
            WriteValue(_modelName, Section, "mLaser.MCRUp", mLaser.MCRUp.ToString());
        }
    }
}
