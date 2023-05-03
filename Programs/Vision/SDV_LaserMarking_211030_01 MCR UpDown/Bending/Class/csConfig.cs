using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

//using Excel = Microsoft.Office.Interop.Excel;
namespace Bending
{
    public class csConfig
    {
        #region DLL

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string strSection, string strKey, string strValue, string strFilePath);

        private string cINIPath = null;
        private string cRecipeName = null;

        #endregion DLL

        private csLog cLog = new csLog();

        private string cRecipePath = @"C:\EQData\INI\RecipeDefault.csv";
        private StringBuilder Value = new StringBuilder(255);

        private const short MaxPositionCnt = 10;

        private string INIFileRead(string Section, string Key, string sdefault = "")
        {
            try
            {
                StringBuilder sb = new StringBuilder(500);
                int Flag = GetPrivateProfileString(Section, Key, sdefault, sb, 500, cINIPath);
                return sb.ToString();
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("INIFileRead" + "," + EX.GetType().Name + "," + EX.Message);
                return sdefault;
            }
        }

        private string RecipeNameRead(string Section, string Key)
        {
            try
            {
                StringBuilder sb = new StringBuilder(500);
                int Flag = GetPrivateProfileString(Section, Key, "", sb, 500, cRecipeName);
                return sb.ToString();
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("INIFileRead" + "," + EX.GetType().Name + "," + EX.Message);
                return "";
            }
        }

        public void RecipeNameWrite(string Section, string Key, string Value)
        {
            string dirPath = CONST.Folder + @"EQData\INI";
            string filePath = dirPath + @"\RecipeName.ini";

            if (!File.Exists(filePath))
            {
                if (!File.Exists(dirPath))
                {
                    if (!Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);

                    Directory.CreateDirectory(dirPath);
                }

                File.Create(filePath);
            }
            WritePrivateProfileString(Section, Key, Value, filePath);
        }

        public void INIFileWrite(string Section, string Key, string Value, int camNo)
        {
            string dirPath = CONST.Folder + @"EQData\INI\Camera Information" + "\\PC" + CONST.PCNo + "\\";
            string filePath = dirPath + "Camera" + camNo.ToString() + ".ini";

            if (!File.Exists(filePath))
            {
                if (!File.Exists(dirPath))
                {
                    if (!Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);

                    Directory.CreateDirectory(dirPath);
                }

                File.Create(filePath);
            }
            WritePrivateProfileString(Section, Key, Value, filePath);
        }

        private string RecipeDefaultRead(string Key)
        {
            try
            {
                GetPrivateProfileString("Recipe Parameter", Key, "", Value, 255, cRecipePath);
                return Value.ToString();
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("RecipeDefaultFileRead" + "," + EX.GetType().Name + "," + EX.Message);
                return "0";
            }
        }

        public bool RecipeName_Read()
        {
            try
            {
                cRecipeName = @"C:\EQData\INI\RecipeName.ini";

                CONST.RunRecipe.OldRecipeName = RecipeNameRead("RecipeName", "RecipeID");

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Initial()
        {
            try
            {
                cINIPath = @"C:\EQData\INI\Config.ini";

                //CONST.EQTYPE = INIFileRead("Config", "EQTYPE");
                CONST.EQPID = INIFileRead("Config", "EQPID");
                CONST.PCName = INIFileRead("Config", "PCNAME");// + "_PC" + INIFileRead("Config", "PCNO");
                CONST.UnitNo = INIFileRead("Config", "UNITNO");
                CONST.PCNo = int.Parse(INIFileRead("Config", "PCNO"));
                CONST.Folder = INIFileRead("Config", "FOLDER", "C");
                CONST.Folder2 = INIFileRead("Config", "FOLDER2", "D");
                CONST.CAMCnt = int.Parse(INIFileRead("Config", "CAMCOUNT"));
                CONST.PLCType = int.Parse(INIFileRead("Config", "PLCTYPE"));
                CONST.PLCIP = INIFileRead("Config", "PLCIP");
                CONST.StationNO = int.Parse(INIFileRead("Config", "STATIONNO"));
                CONST.PLCDeviceType = INIFileRead("Config", "PLCDEVICE");
                CONST.HeightIP = INIFileRead("Config", "HEIGHTIP");
                CONST.RESULT_TITLE1 = INIFileRead("Config", "RESULT_TITLE1");
                CONST.RESULT_TITLE2 = INIFileRead("Config", "RESULT_TITLE2");
                CONST.RESULT_TITLE3 = INIFileRead("Config", "RESULT_TITLE3");
                CONST.RESULT_TITLE4 = INIFileRead("Config", "RESULT_TITLE4");
                //CONST.RESULT1_DISP = int.Parse(INIFileRead("Config", "RESULT1_DISP"));
                //CONST.RESULT2_DISP = int.Parse(INIFileRead("Config", "RESULT2_DISP"));
                //CONST.RESULT3_DISP = int.Parse(INIFileRead("Config", "RESULT3_DISP"));
                //CONST.RESULT4_DISP = int.Parse(INIFileRead("Config", "RESULT4_DISP"));
                CONST.RESULT1_TYPE = INIFileRead("Config", "RESULT1_TYPE");
                CONST.RESULT2_TYPE = INIFileRead("Config", "RESULT2_TYPE");
                CONST.RESULT3_TYPE = INIFileRead("Config", "RESULT3_TYPE");
                CONST.RESULT4_TYPE = INIFileRead("Config", "RESULT4_TYPE");
                CONST.RESULT5_TYPE = INIFileRead("Config", "RESULT5_TYPE"); //pcy200609 Height추가
                // ########################
                CONST.cSideVisionPath = Path.Combine(CONST.Folder2, CONST.cSideVisionPath);
                CONST.cImagePath = Path.Combine(CONST.Folder2, CONST.cImagePath);
                CONST.cVisionPath = Path.Combine(CONST.Folder, CONST.cVisionPath); //CONST.Folder + CONST.cVisionImgPath;  //D Driver
                CONST.cRecipeSavePath = Path.Combine(CONST.Folder, CONST.cRecipeSavePath);
                CONST.cTracePath = Path.Combine(CONST.Folder, CONST.cTracePath);
                CONST.cMotionTracePath = Path.Combine(CONST.Folder, CONST.cMotionTracePath);
                CONST.cConfigSavePath = Path.Combine(CONST.Folder, CONST.cConfigSavePath); // lyw420. 20170107 추가.
                CONST.cDefaultDataPath = Path.Combine(CONST.Folder, CONST.cDefaultDataPath); // lyw. default config.

                csLog.cLogPath = Path.Combine(CONST.Folder2, @"EQData\Log\");
                csLog.cExceptionLogPath = Path.Combine(CONST.Folder2, CONST.PCName, "ExceptionLog"); //고객사 생산장비 보안 체크리스트에 맞게 작성
                csLog.cSystemLogPath = Path.Combine(CONST.Folder2, @"EQData\Log\SystemLog");
                csLog.cInspectionLogPath = Path.Combine(CONST.Folder2, @"EQData\Log\InspectionLog");
                // ########################

                Address_set();
                return true;
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("Initial" + "," + EX.GetType().Name + "," + EX.Message);
                return false;
            }
        }

        private StringBuilder sb = new StringBuilder();

        public bool CAMconfig_Read(int camNo, ref csVision.sCFG CFG)
        {
            //2018.07.10 INI 파일 경로 수정 khs
            //cINIPath = "C:/EQData/INI/Camera Information/" + CONST.RunRecipe.RecipeName + "/PC" + CONST.PCNo + "/Camera" + camNo.ToString() + ".ini";
            cINIPath = CONST.Folder + @"EQData\INI\Camera Information" + "\\PC" + CONST.PCNo + "\\Camera" + camNo.ToString() + ".ini";
            //csVision.sCFG CFG = new csVision.sCFG();
            try
            {
                CFG.Camno = camNo;
                CFG.Use = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.Use"));
                CFG.Name = INIFileRead("Camera" + camNo.ToString(), "CAM.Name");

                if (!CFG.Use) CFG.Name = "NotUse";
                CFG.FOVX = double.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.FOVX"));
                CFG.FOVY = double.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.FOVY"));
                CFG.Resolution = double.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.Resolution"));
                CFG.Serial = INIFileRead("Camera" + camNo.ToString(), "CAM.Serial");

                CFG.Light1Comport = int.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.Light1Comport"));
                CFG.Light1CH = int.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.Light1CH"));
                CFG.Light5VComport = int.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.Light5VComport", "0"));
                CFG.Light5VCH = int.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.Light5VCH", "0"));
                CFG.Light5VValue = int.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.Light5VValue", "0"));
                CFG.SubLight1CH = int.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.SubLight1CH", "0"));
                CFG.SubLight1Value = int.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.SubLight1Value", "0"));
                CFG.PatternSearchMode = (CONST.ePatternSearchMode)Conv2Enum(typeof(CONST.ePatternSearchMode), int.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.PatternSearchMode")));
                CFG.PatternSearchTool = (CONST.ePatternSearchTool)Conv2Enum(typeof(CONST.ePatternSearchTool), int.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.PatternSearchTool")));
                CFG.CalType = (eCalType)Conv2Enum(typeof(eCalType), int.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.CalType")));
                CFG.LightType = (CONST.eLightType)Conv2Enum(typeof(CONST.eLightType), int.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.LightType")));
                CFG.ImageSaveType = (CONST.eImageSaveType)Conv2Enum(typeof(CONST.eImageSaveType), int.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.ImageSaveType")));

                CFG.nReverseMode = (CONST.eImageReverse)Conv2Enum(typeof(CONST.eImageReverse), int.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.nReverseMode")));
                CFG.GrabDelay = int.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.GrabDelay"));
                CFG.GrabDelay2 = int.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.GrabDelay2", "0"));
                CFG.LightDelay = int.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.LightDelay"));
                if (CFG.LightDelay == 0) CFG.LightDelay = 150;  //default

                CFG.AlignLimitX = double.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.AlignLimitX"));
                CFG.AlignLimitY = double.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.AlignLimitY"));
                CFG.AlignLimitT = double.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.AlignLimitT"));

                CFG.ArmPreAlignLimitX = double.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.ArmPreAlignLimitX", "5"));
                CFG.ArmPreAlignLimitY = double.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.ArmPreAlignLimitY", "5"));
                CFG.ArmPreAlignLimitT = double.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.ArmPreAlignLimitT", "5"));

                CFG.RetryLimitCnt = int.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.RetryLimitCnt"));
                //2018.07.10 RetryCnt 추가 khs
                CFG.RetryCnt = int.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.RetryCnt"));

                CFG.PatternFailManualIn = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.PatternManualInput", "True"));
                CFG.ManualWindow = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.ManualWindow", "False"));
                //CFG.AlignResultDisp = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.AlignResultDisp"));
                //CFG.InspResultDisp = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.InspResultDisp"));
                CFG.ImageSave = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.ImageSave"));
                CFG.SideVision = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.SideVision"));
                CFG.SideImgSave = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.SideImgSave"));

                CFG.AlignNotUseX = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.AlignNotUseX"));
                CFG.AlignNotUseY = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.AlignNotUseY"));
                CFG.AlignNotUseT = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.AlignNotUseT"));

                CFG.XAxisRevers = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.XAxisRevers"));
                CFG.YAxisRevers = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.YAxisRevers"));
                CFG.TAxisRevers = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.TAxisRevers"));

                //210215 cjm X-Y Axis Revers add
                CFG.XYAxisRevers = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.XYAxisRevers"));

                //pcy190421
                CFG.OffsetXReverse = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.OffsetXReverse"));

                //pcy190729
                CFG.YTCalUse = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.YTCalUse"));

                CFG.CenterAlign = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.CenterAlign"));

                //pcy201230
                CFG.ImgAutoDelDay = int.Parse(INIFileRead("Camera" + camNo.ToString(), "CAM.ImgAutoDelDay", "30"));
                if (CFG.ImgAutoDelDay == 0) CFG.ImgAutoDelDay = 30;  //0일 경우 문제 됨...default 30일

                //pcy200918
                try
                {
                    CFG.eCamName = INIFileRead("Camera" + camNo.ToString(), "CAM.eCamName");
                }
                catch { }
                CFG.AlignUse = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.AlignUse", "True"));

                //210116 cjm 딥러닝 사용 유무
                //CFG.DLUse = Convert.ToBoolean(INIFileRead("Camera" + camNo.ToString(), "CAM.DeepLearningUse", "True"));

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CAMconfig_Write(csVision.sCFG CFG, int camNo)
        {
            int cValue = 0;
            try
            {
                INIFileWrite("Camera" + camNo.ToString(), "CAM.Use", CFG.Use.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.Name", CFG.Name.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.FOVX", CFG.FOVX.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.FOVY", CFG.FOVY.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.Resolution", CFG.Resolution.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.Serial", CFG.Serial.ToString(), camNo);

                INIFileWrite("Camera" + camNo.ToString(), "CAM.Light1Comport", CFG.Light1Comport.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.Light1CH", CFG.Light1CH.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.Light5VComport", CFG.Light5VComport.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.Light5VCH", CFG.Light5VCH.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.Light5VValue", CFG.Light5VValue.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.SubLight1CH", CFG.SubLight1CH.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.SubLight1Value", CFG.SubLight1Value.ToString(), camNo);

                switch (CFG.PatternSearchMode)
                {
                    case CONST.ePatternSearchMode.LastBest: cValue = 0; break;
                    case CONST.ePatternSearchMode.AllBest: cValue = 1; break;
                }
                INIFileWrite("Camera" + camNo.ToString(), "CAM.PatternSearchMode", cValue.ToString(), camNo);

                switch (CFG.PatternSearchTool)
                {
                    case CONST.ePatternSearchTool.PMAlign: cValue = 0; break;
                    case CONST.ePatternSearchTool.SearchMax: cValue = 1; break;
                }
                INIFileWrite("Camera" + camNo.ToString(), "CAM.PatternSearchTool", cValue.ToString(), camNo);

                switch (CFG.searchKind1)
                {
                    case eSearchKind.Pattern: cValue = 0; break;
                    case eSearchKind.Line: cValue = 1; break;
                }
                INIFileWrite("Camera" + camNo.ToString(), "CAM.searchKind1", cValue.ToString(), camNo);

                switch (CFG.searchKind2)
                {
                    case eSearchKind.Pattern: cValue = 0; break;
                    case eSearchKind.Line: cValue = 1; break;
                }
                INIFileWrite("Camera" + camNo.ToString(), "CAM.searchKind2", cValue.ToString(), camNo);

                switch (CFG.CalType)
                {
                    case eCalType.Cam1Type: cValue = 0; break;
                    case eCalType.Cam2Type: cValue = 1; break;
                    case eCalType.Cam3Type: cValue = 2; break;
                    case eCalType.Cam4Type: cValue = 3; break;
                    case eCalType.Cam1Cal2: cValue = 4; break;
                }
                INIFileWrite("Camera" + camNo.ToString(), "CAM.CalType", cValue.ToString(), camNo);

                //2018.11.29 Light Type 추가
                switch (CFG.LightType)
                {
                    case CONST.eLightType.Light5V: cValue = 0; break;
                    case CONST.eLightType.Light12V: cValue = 1; break;
                }
                INIFileWrite("Camera" + camNo.ToString(), "CAM.LightType", cValue.ToString(), camNo);

                switch (CFG.ImageSaveType)
                {
                    case CONST.eImageSaveType.All: cValue = 0; break;
                    case CONST.eImageSaveType.Original: cValue = 1; break;
                    case CONST.eImageSaveType.Display: cValue = 2; break;
                }
                INIFileWrite("Camera" + camNo.ToString(), "CAM.ImageSaveType", cValue.ToString(), camNo);

                switch (CFG.nReverseMode)
                {
                    case CONST.eImageReverse.None: cValue = 0; break;
                    case CONST.eImageReverse.XReverse: cValue = 1; break;
                    case CONST.eImageReverse.YReverse: cValue = 2; break;
                    case CONST.eImageReverse.AllReverse: cValue = 3; break;
                    case CONST.eImageReverse.Reverse90: cValue = 4; break;
                    case CONST.eImageReverse.Reverse270: cValue = 5; break;
                    case CONST.eImageReverse.Reverse90XY: cValue = 6; break;
                    case CONST.eImageReverse.Reverse270XY: cValue = 7; break;
                }
                INIFileWrite("Camera" + camNo.ToString(), "CAM.nReverseMode", cValue.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.GrabDelay", CFG.GrabDelay.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.GrabDelay2", CFG.GrabDelay2.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.LightDelay", CFG.LightDelay.ToString(), camNo);

                INIFileWrite("Camera" + camNo.ToString(), "CAM.ImgAutoDelDay", CFG.ImgAutoDelDay.ToString(), camNo);
                //조명은 레시피별로 관리

                INIFileWrite("Camera" + camNo.ToString(), "CAM.AlignLimitX", CFG.AlignLimitX.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.AlignLimitY", CFG.AlignLimitY.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.AlignLimitT", CFG.AlignLimitT.ToString(), camNo);

                INIFileWrite("Camera" + camNo.ToString(), "CAM.RetryLimitCnt", CFG.RetryLimitCnt.ToString(), camNo);
                //2018.07.10 RetryCnt 추가 khs
                INIFileWrite("Camera" + camNo.ToString(), "CAM.RetryCnt", CFG.RetryCnt.ToString(), camNo);

                INIFileWrite("Camera" + camNo.ToString(), "CAM.PatternManualInput", CFG.PatternFailManualIn.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.ManualWindow", CFG.ManualWindow.ToString(), camNo);
                //INIFileWrite("Camera" + camNo.ToString(), "CAM.AlignResultDisp", CFG.AlignResultDisp.ToString(), camNo);
                //INIFileWrite("Camera" + camNo.ToString(), "CAM.InspResultDisp", CFG.InspResultDisp.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.ImageSave", CFG.ImageSave.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.SideVision", CFG.SideVision.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.SideImgSave", CFG.SideImgSave.ToString(), camNo);

                INIFileWrite("Camera" + camNo.ToString(), "CAM.AlignNotUseX", CFG.AlignNotUseX.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.AlignNotUseY", CFG.AlignNotUseY.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.AlignNotUseT", CFG.AlignNotUseT.ToString(), camNo);

                INIFileWrite("Camera" + camNo.ToString(), "CAM.XAxisRevers", CFG.XAxisRevers.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.YAxisRevers", CFG.YAxisRevers.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.TAxisRevers", CFG.TAxisRevers.ToString(), camNo);

                //210215 cjm X-Y Axis Revers add
                INIFileWrite("Camera" + camNo.ToString(), "CAM.XYAxisRevers", CFG.XYAxisRevers.ToString(), camNo);

                //pcy190421
                INIFileWrite("Camera" + camNo.ToString(), "CAM.OffsetXReverse", CFG.OffsetXReverse.ToString(), camNo);

                //pcy190421
                INIFileWrite("Camera" + camNo.ToString(), "CAM.YTCalUse", CFG.YTCalUse.ToString(), camNo);

                INIFileWrite("Camera" + camNo.ToString(), "CAM.CenterAlign", CFG.CenterAlign.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.AlignUse", CFG.AlignUse.ToString(), camNo);

                //210116 cjm 딥러닝 사용 유무
                INIFileWrite("Camera" + camNo.ToString(), "CAM.DeepLearningUse", CFG.DLUse.ToString(), camNo);

                //210119 cjm ArmPre 인터락
                INIFileWrite("Camera" + camNo.ToString(), "CAM.ArmPreAlignLimitX", CFG.ArmPreAlignLimitX.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.ArmPreAlignLimitY", CFG.ArmPreAlignLimitY.ToString(), camNo);
                INIFileWrite("Camera" + camNo.ToString(), "CAM.ArmPreAlignLimitT", CFG.ArmPreAlignLimitT.ToString(), camNo);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Address_set()
        {
            cINIPath = CONST.Folder + @"EQData\INI\Address_.ini";

            //PLC
            CONST.Address.PLC.BITCONTROL = int.Parse(INIFileRead("Address_", "PLCBITCONTROL"));
            //CONST.Address.PLC.CIMIF = int.Parse(INIFileRead("Address_", "PLCCIMIF"));
            CONST.Address.PLC.REPLY = int.Parse(INIFileRead("Address_", "PLCREPLY"));
            CONST.Address.PLC.MODE = int.Parse(INIFileRead("Address_", "PLCMODE"));
            CONST.Address.PLC.CURRENTPOSITION = int.Parse(INIFileRead("Address_", "PLCCURRENTPOSITION"));
            CONST.Address.PLC.CHANGETIME = int.Parse(INIFileRead("Address_", "PLCCHANGETIME"));
            CONST.Address.PLC.CELLID1 = int.Parse(INIFileRead("Address_", "PLCCELLID1"));
            CONST.Address.PLC.CELLID2 = int.Parse(INIFileRead("Address_", "PLCCELLID2"));
            CONST.Address.PLC.CELLID3 = int.Parse(INIFileRead("Address_", "PLCCELLID3"));
            CONST.Address.PLC.CELLID4 = int.Parse(INIFileRead("Address_", "PLCCELLID4"));
            //20.10.16 lkw
            CONST.Address.PLC.CELLID5 = int.Parse(INIFileRead("Address_", "PLCCELLID5"));
            CONST.Address.PLC.CELLID6 = int.Parse(INIFileRead("Address_", "PLCCELLID6"));
            CONST.Address.PLC.CELLID7 = int.Parse(INIFileRead("Address_", "PLCCELLID7"));
            CONST.Address.PLC.CELLID8 = int.Parse(INIFileRead("Address_", "PLCCELLID8"));
            CONST.Address.PLC.RECIPEID = int.Parse(INIFileRead("Address_", "PLCRECIPEID"));
            CONST.Address.PLC.RECIPEPARAM = int.Parse(INIFileRead("Address_", "PLCRECIPEPARAM"));
            CONST.Address.PLC.POSITION = int.Parse(INIFileRead("Address_", "PLCPOSITION"));
            CONST.Address.PLC.PLC_CAL_MOVE = int.Parse(INIFileRead("Address_", "PLCCALMOVE"));
            CONST.Address.PLC.INSPECTIONBENDINGLAST = int.Parse(INIFileRead("Address_", "PLCINSPECTIONBENDINGLAST", "10350"));
            //210118 cjm 레이저 로그
            CONST.Address.PLC.LaserSendCellID = int.Parse(INIFileRead("Address_", "PLCLaserSendCellID", "10900"));
            CONST.Address.PLC.MCRCellID = int.Parse(INIFileRead("Address_", "PLCMCRCellID", "11000"));

            //PC
            CONST.Address.PC.BITCONTROL = int.Parse(INIFileRead("Address_", "PCBITCONTROL"));
            //CONST.Address.PC.CIMIF = int.Parse(INIFileRead("Address_", "PCCIMIF"));
            CONST.Address.PC.CALIBRATION = int.Parse(INIFileRead("Address_", "PCCALIBRATION"));
            CONST.Address.PC.REPLY = int.Parse(INIFileRead("Address_", "PCREPLY"));
            CONST.Address.PC.VISIONOFFSET = int.Parse(INIFileRead("Address_", "PCVISIONOFFSET"));
            //CONST.Address.PC.CHANGETIME = int.Parse(INIFileRead("Address_" , "PCCHANGETIME"));
            //CONST.Address.PC.TraceInfoArm1 = int.Parse(INIFileRead("Address_" , "PCTRACEINFO1")); //일단 주석 처리
            //CONST.Address.PC.TraceInfoArm2 = int.Parse(INIFileRead("Address_" , "PCTRACEINFO2"));
            //CONST.Address.PC.TraceInfoArm3 = int.Parse(INIFileRead("Address_" , "PCTRACEINFO3"));
            //벤딩궤적주소
            CONST.Address.PC.BENDING1 = int.Parse(INIFileRead("Address_", "PCBENDING1"));
            CONST.Address.PC.BENDING2 = int.Parse(INIFileRead("Address_", "PCBENDING2"));
            CONST.Address.PC.BENDING3 = int.Parse(INIFileRead("Address_", "PCBENDING3"));
            CONST.Address.PC.SV = int.Parse(INIFileRead("Address_", "PCSV"));
            CONST.Address.PC.DV = int.Parse(INIFileRead("Address_", "PCDV"));
            CONST.Address.PC.PC_CAL_MOVE = int.Parse(INIFileRead("Address_", "PCCALMOVE"));
            CONST.Address.PC.CPK = int.Parse(INIFileRead("Address_", "PCCPK"));
            //CONST.Address.PC.FFU = int.Parse(INIFileRead("Address_", "PCFFU"));
            CONST.Address.PC.MatchingScore = int.Parse(INIFileRead("Address_", "PCMATCHINGSCORE"));
            //CONST.Address.PC.TransferFirstOffset = int.Parse(INIFileRead("Address_" , "PCTransferOffset"));

            CONST.Address.PC.APNCode1 = int.Parse(INIFileRead("Address_", "PCAPNCODE1"));
            CONST.Address.PC.APNCode2 = int.Parse(INIFileRead("Address_", "PCAPNCODE2"));



            SetOffsetAddress(CONST.Address.PC.VISIONOFFSET);
            SetMotionAddress(CONST.Address.PLC.POSITION);
            //pcy210119
            SetDVAddress(CONST.Address.PC.DV);
            SetSVAddress(CONST.Address.PC.SV);
            SetMatchingScoreAddress(CONST.Address.PC.MatchingScore);
        }

        private int SetMotionAddress(int address)
        {
            //CONST.AAM_PLC2.MotorAddress.TempAttachTLeft = address + 0;
            //CONST.AAM_PLC2.MotorAddress.TempAttachTRight = address + 2;
            //CONST.AAM_PLC2.MotorAddress.EMIAttachTLeft = address + 4;
            //CONST.AAM_PLC2.MotorAddress.EMIAttachTRight = address + 6;
            return address;
        }
        private int SetOffsetAddress(int address)
        {
            //프로그램 실행시 한번 실행(Visionoffset읽은 후)
            Address.VisionOffset.LoadingPre1 += address;
            Address.VisionOffset.LoadingPre2 += address;
            Address.VisionOffset.LaserAlign1 += address;
            Address.VisionOffset.LaserAlign2 += address;

            return address;
        }
        private int SetDVAddress(int address)
        {
            Address.DV.LaserInsp1 += address;
            Address.DV.LaserInsp2 += address;
            Address.DV.LaserInsp1_Ascii += address;
            Address.DV.LaserInsp1_ASP += address;
            Address.DV.LaserInsp2_Ascii += address;
            Address.DV.LaserInsp2_ASP += address;
            return address;
        }
        private int SetSVAddress(int address)
        {
            //pcy210119
            //프로그램 실행시 한번 실행(Address읽은 후)
            Address.SV.BendAlignMove += address;

            return address;
        }
        private int SetMatchingScoreAddress(int address)
        {
            //pcy210119
            //프로그램 실행시 한번 실행(Address읽은 후)
            Address.MatchingScore.LoadingPre1 += address;
            Address.MatchingScore.LoadingPre2 += address;
            Address.MatchingScore.Laser1Align += address;
            Address.MatchingScore.Laser2Align += address;
            
            return address;
        }
        // 정해진 type을 enum으로 변환하기.
        private object Conv2Enum(Type eNumType, int value)
        {
            //Type eNumType = eNumObject.GetType();
            string[] eNames = Enum.GetNames(eNumType);
            object[] objects = new object[eNames.Length];

            int idx = 0;

            foreach (string name1 in Enum.GetNames(eNumType))
            {
                objects[idx++] = Enum.Parse(eNumType, name1);
            }

            //CFG.CalType = (eCalType)objects[idx];
            return objects[value];
        }

        public string[] LogInUserList() //0 : User ID, 1 : Password
        {
            //int iCnt = 0;

            string[] sData = new string[3];

            try
            {
                cINIPath = CONST.Folder + @"EQData\INI\Login.ini";

                FileInfo fi = new FileInfo(cINIPath);

                if (fi.Exists == true)
                {
                    StreamReader sr = new StreamReader(cINIPath);

                    string data = sr.ReadToEnd();       //파일을 끝까지 읽어서
                    string[] lines = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries); //행단위로 쪼갠다

                    for (int i = 1; i < lines.Length; i++)
                    {
                        string[] msg = lines[i].Split(new char[] { '=' }, StringSplitOptions.None); //행단위로 쪼갠다
                        sData[i - 1] = msg[0].Trim();
                    }
                    sr.Close();
                }
            }
            catch (Exception EX)
            {
                //DBrd.Close();
                cLog.ExceptionLogSave("LogInUserList" + "," + EX.GetType().Name + "," + EX.Message);
            }
            return sData;
        }

        public void LogInPWChange(string LoginPW, string LoginUser)
        {
        }

        public string LoginIDPWCheck(string LoginUser)
        {
            string PW = null;
            try
            {
                cINIPath = CONST.Folder + @"EQData\INI\Login.ini";

                FileInfo fi = new FileInfo(cINIPath);

                if (fi.Exists == true)
                {
                    StreamReader sr = new StreamReader(cINIPath);

                    string data = sr.ReadToEnd();       //파일을 끝까지 읽어서
                    string[] lines = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries); //행단위로 쪼갠다

                    for (int i = 1; i < lines.Length; i++)
                    {
                        string[] msg = lines[i].Split(new char[] { '=' }, StringSplitOptions.None); //행단위로 쪼갠다
                        if (LoginUser == msg[0])
                        {
                            PW = msg[1];
                        }
                    }
                    sr.Close();
                }
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("LoginIDPWCheck" + "," + EX.GetType().Name + "," + EX.Message);
            }
            return PW;
        }

        #region 2020.04.04 Height Sensor

        public struct sHeightResult
        {
            public bool bOKNG;
            public int iOKNG;
            public string JobName;
            public double[] dValue;
            public string[] dValueDisp;
        }

        public sHeightResult sHResult = new sHeightResult();

        public struct sHeightSpec
        {
            public string HeightReference;
            public string HeightSpec;
            public string HeightTolerence;
            public int[] HeightPoint;
        }

        public sHeightSpec[] sHeight;

        public sHeightSpec[] HeightReadPara()
        {
            //bool bResult = false;
            string cTempPath = Path.Combine(@"C:\EQData\Config\ModelData", CONST.RunRecipe.RecipeName, "Height Spec.txt");

            try
            {
                string strFilePath = cTempPath;

                FileInfo fi = new FileInfo(strFilePath);
                if (fi.Exists == true)
                {
                    StreamReader sr = new StreamReader(strFilePath);

                    string data = sr.ReadToEnd();		//파일을 끝까지 읽어서
                    sr.Close();
                    string[] lines = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries); //행단위로 쪼갠다

                    sHeight = new sHeightSpec[lines.Length - 2];

                    for (int i = 0; i < sHeight.Length; i++)
                    {
                        string[] sData = lines[i + 2].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries); //행단위로 쪼갠다

                        sHeight[i].HeightReference = sData[0];
                        sHeight[i].HeightSpec = sData[1];
                        sHeight[i].HeightTolerence = sData[2];

                        string[] sPoint = sData[3].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries); //행단위로 쪼갠다
                        sHeight[i].HeightPoint = new int[sPoint.Length];
                        for (int j = 0; j < sPoint.Length; j++)
                        {
                            sHeight[i].HeightPoint[j] = int.Parse(sPoint[j]);
                        }
                    }
                    //bResult = true;
                }
                else
                {
                    //bResult = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //bResult = false;
            }
            return sHeight;
        }

        #endregion 2020.04.04 Height Sensor

        public double[,] HeightResult = new double[10, 10];

        #region 20200904 Calibration Result를 dgv에 나열
        public bool CalResult(string CalResultSection)
        {
            try
            {
                // 20200908 cjm 경로 변경 필요
                // 20200918 cjm Calibration dgv 목록 줄임
                // 20200924 cjm 목록 변경
                //cINIPath = "C:/EQData/Calibration/Data/SETUP/Calibration.dat"; // "C:/EAData/Calibration/Data/" +Const.RunRecipe.RecipeName + "/Calibration.dat"; 
                string sCalPath = @"C:\EQData\Calibration\Data";
                cINIPath = Path.Combine(sCalPath, CONST.RunRecipe.RecipeName, "Calibration.dat");

                for (int i = 0; i < 4; i++)
                {
                    CONST.resultCal1[i] = INIFileRead(CalResultSection, "resultCal1." + i);
                    CONST.resultCal2[i] = INIFileRead(CalResultSection, "resultCal2." + i);
                    CONST.resultCal3[i] = INIFileRead(CalResultSection, "resultCal3." + i);
                    CONST.resultCal4[i] = INIFileRead(CalResultSection, "resultCal4." + i);
                }
                for (int i = 0; i < 2; i++)
                {
                    CONST.Pos[i] = INIFileRead(CalResultSection, "Pos" + (i + 1));
                }
                CONST.sx = INIFileRead(CalResultSection, "sx");
                CONST.sy = INIFileRead(CalResultSection, "sy");
                CONST.mFX = INIFileRead(CalResultSection, "mFX");
                CONST.mFY = INIFileRead(CalResultSection, "mFY");

                return true;
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("CalibrationResult" + "," + EX.GetType().Name + "," + EX.Message);
                return false;
            }
        }
        #endregion
    }
}