using System;
using System.IO;
using System.Text;

namespace Bending
{
    public enum LogKind
    {
        System,
        Interface,
        MeasBending1,
        MeasBending2,
        MeasBending3,
        AlignNoPress,
        AutoMain,
        BDHistory,
        AttachHistory,
        MeasBending,
        AlignLoadPre,
        AlignSCFPanel,
        AlignSCFReel,
        AlignSCFReelPickUp,
        AlignPre,//JJ , 20170410 , Bending Align Data
        AlignBendingVision,//JJ , 20170410 , Bending Vision Data
        BDPreSCFInspection,
        BDSCFInspection,
        UpperInspection,
        UpperInspectionDay,
        InspectionEdgetoMark,
        InspectionEdgetoMarkDay,
        BDCalibration,
        PRUpperInsData,
        BDRobot,
        AttachRobot,
        BDRobotFirstDay, //pcy190613 시간별 First로그 한군데로 모으기 귀찮아서 추가
        AttachFirstDay,
        BDHistoryDay, //pcy190613 시간별 로그 한군데로 모으기 귀찮아서 추가
        AttachHistoryDay,
        PixelInfo,
        EMIAttach,
        Calibration, //MFQ
        InspectionDist, //MFQ
        AlignMeasure, //MFQ
        Shif,
        SideInspection,
        SCALE,
        TempAttach,  //2020.09.15 lkw
        CalibrationResult, // 20200908 cjm Calibration 좌표 찍는 곳
        Conveyor,
        InspMFQ, //inspection mfq 가로세로 데이터
        AttachAlign,
        DetachInspection,
        Laser, // 210118 cjm 레이저 로그
        LaserAlign,
        LaserPositionInsp,
        TrayLoader, //210215 cjm TrayLoader Log Add
        CycleTimeBDAlign,
    };

    class csLog
    {
        //20160827

        public static string cLogPath;

        public static string cExceptionLogPath;
        public static string cSystemLogPath;
        public static string cInspectionLogPath;
        public static int LogKeepDay = 30;

        // 파일 삭제 추가. lyw. 170110.
        public csDirRemover logRemove = new csDirRemover();

        public void ExceptionLogSave(string lcString)
        {
            try
            {
                lcString = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "." + DateTime.Now.Millisecond.ToString() + " " + lcString;

                string lcdate = DateTime.Now.ToString("yyyy-MM-dd");
                DateTime lcDT = DateTime.Now;
                string lcHour = lcDT.Hour.ToString();

                if (!Directory.Exists(cExceptionLogPath + "/" + lcdate))
                    Directory.CreateDirectory(cExceptionLogPath + "/" + lcdate);
                StreamWriter FileInfo = new StreamWriter(cExceptionLogPath + "/" + lcdate + "/" + lcdate + "-" + lcHour + ".log", true);
                FileInfo.WriteLine(lcString);
                FileInfo.Close();

                // 파일 삭제 추가. lyw. 170110.
                logRemove.DirectoryRemove(cExceptionLogPath, 24 * 30, true, false);

            }
            catch
            { }
        }

        public void LogSave(string lcString)
        {
            try
            {
                lcString = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "." + DateTime.Now.Millisecond.ToString() + " " + lcString;

                string lcdate = DateTime.Now.ToString("yyyy-MM-dd");
                DateTime lcDT = DateTime.Now;
                string lcHour = lcDT.Hour.ToString();

                if (!Directory.Exists(cSystemLogPath + "/" + lcdate))
                    Directory.CreateDirectory(cSystemLogPath + "/" + lcdate);
                StreamWriter FileInfo = new StreamWriter(cSystemLogPath + "/" + lcdate + "/" + lcdate + "-" + lcHour + ".log", true);
                FileInfo.WriteLine(lcString);
                FileInfo.Close();

                // 파일 삭제 추가. lyw. 170110.
                logRemove.DirectoryRemove(cSystemLogPath, 24 * 30, true, false); // 30일에 한번 삭제.                
            }
            catch
            { }
        }

        //20160827
        public void Save(LogKind lcKind, string lcString = "", bool bTimeexcept = false)
        {
            try
            {
                if (!bTimeexcept)
                {
                    lcString = DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss") + "." + DateTime.Now.Millisecond.ToString("000") + "," + lcString;
                }

                string lcPath = "";

                string lcdate = DateTime.Now.ToString("yyyy-MM-dd");
                DateTime lcDT = DateTime.Now;
                string lcHour = lcDT.Hour.ToString("00");

                //20201003 cjm Calibration 점찍는 저장 Path 변경
                string CalLogPath = Path.Combine(CONST.Folder2, @"EQData\Calibration");

                string RemovePath = Path.Combine(cLogPath, lcKind.ToString());

                //lcPath = Path.Combine(cLogPath, lcdate);
                //lcPath = Path.Combine(lcPath, lcKind.ToString());

                lcPath = Path.Combine(cLogPath, lcKind.ToString());
                lcPath = Path.Combine(lcPath, lcdate);

                string Filepath;

                if (lcKind == LogKind.AutoMain
                || lcKind == LogKind.System || lcKind == LogKind.Interface)
                {
                    Filepath = Path.Combine(lcPath, lcKind.ToString() + "-" + lcHour + ".txt");
                }
                else if (lcKind == LogKind.BDRobotFirstDay || lcKind == LogKind.BDHistoryDay || lcKind == LogKind.UpperInspectionDay || lcKind == LogKind.InspectionEdgetoMarkDay
                    || lcKind == LogKind.AttachFirstDay || lcKind == LogKind.AttachHistoryDay)
                {
                    //0~6시까지는 전날폴더에..
                    if (0 <= int.Parse(lcHour) && int.Parse(lcHour) < 7)
                    {
                        lcdate = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
                        lcPath = Path.Combine(cLogPath, lcKind.ToString());
                        lcPath = Path.Combine(lcPath, lcdate);
                        Filepath = Path.Combine(lcPath, lcKind.ToString() + "-night" + ".csv");
                    }
                    else if (7 <= int.Parse(lcHour) && int.Parse(lcHour) < 19)
                    {
                        Filepath = Path.Combine(lcPath, lcKind.ToString() + "-day" + ".csv");
                    }
                    else
                    {
                        Filepath = Path.Combine(lcPath, lcKind.ToString() + "-night" + ".csv");
                    }
                }
                else if (lcKind == LogKind.CalibrationResult) // 20200908 cjm 추가 Calibration 움직이는 점 저장하는 곳
                {
                    //lcPath = Path.GetDirectoryName(lcPath);  // 기존 경로 : cLogPath/CalibrationResult/오늘날짜/CalibrationResult-시간.csv => 현재경로 : cLogPath/CalibrationResult/CalibrationResult.csv 바꿈
                    //Filepath = Path.Combine(lcPath, lcString.Split(',')[1] + ".csv");
                    lcPath = CalLogPath;
                    lcPath = Path.Combine(lcPath, lcString.Split(',')[1]);
                    Filepath = Path.Combine(lcPath, lcString.Split(',')[1] + ".csv"); //20201003 cjm Calibration 점 찍는 곳 주소가 줄어들 변경
                }
                else
                {
                    Filepath = Path.Combine(lcPath, lcKind.ToString() + "-" + lcHour + ".csv");
                }

                if (!Directory.Exists(lcPath))
                    Directory.CreateDirectory(lcPath);

                string baselog = "";

                if (!File.Exists(Filepath))
                {
                    switch (lcKind)
                    {
                        case LogKind.SideInspection:
                            baselog = "Date,Unit,ID,InCenterX,InCenterY,InRadius,OutCenterX,OutCenterY,OutRadius,Dist," +
                                "DLPreInspResult1,DLPreInspResult2,DLInspUSE1,DLInspUSE2,DLInspResult1,DLInspResult2\r\n";
                            break;
                        case LogKind.PixelInfo:
                            baselog = "Data,CamKind,Kind,dX1,dY1,dX2,dY2\r\n";
                            break;
                        case LogKind.Conveyor: //201012 cjm 추가
                        case LogKind.AlignLoadPre:
                            baselog = "Data,CellID,AlignNO,OKNG,MarkX1,MarkY1,MarkX2,MarkY2,diffX1,diffY1,diffX2,diffY2,AlignX,AlignY,AlignT,Length,Theta,Time,DLPanelFind\r\n";
                            break;
                        //20.10.16 lkw
                        case LogKind.BDPreSCFInspection://201012 cjm
                            baselog = "Data,CellID,IndexNO,X1,Y1,X2,Y2,X3,Y3\r\n";
                            //baselog = "Data,CellID,AttachInsp,SCFInsp,dY1,dY2\r\n";
                            break;
                        case LogKind.BDSCFInspection:
                            baselog = "Data,CellID,BendNo,OKNG,dX1,dY1,dX2,dY2\r\n";
                            break;
                        case LogKind.BDRobot:
                        case LogKind.BDRobotFirstDay:
                            baselog = "Data,CellID,BendNo,Retry,MarkX1,MarkY1,RefX1,RefY1,MarkX2,MarkY2,RefX2,RefY2," +
                                "AlignX,AlignY,AlignT,DistX1,DistY1,DistX2,DistY2,MoveAddAlignX,MoveAddAlignY,MoveAddAlignT,SpecInOut,DLPanelFind,DLFPCFind," +
                                "RefScoreLimit1,RefScore1,PatternRefNo1,RefScoreLimit2,RefScore2,PatternRefNo2,PatternScoreLimit1,PatternScore1,PatternNo1,PatternScoreLimit2,PatternScore2,PatternNo2\r\n";
                            break;
                        case LogKind.AttachFirstDay:
                        case LogKind.AttachRobot:
                            baselog = "Data,CellID,AttachNo,Retry,MarkX1,MarkY1,RefX1,RefY1,MarkX2,MarkY2,RefX2,RefY2," +
                                "AlignX,AlignY,AlignT,DistX1,DistY1,DistX2,DistY2,MoveAddAlignX,MoveAddAlignY,MoveAddAlignT,SpecInOut,DLPanelFind,DLMaterialFind," +
                                "RefScoreLimit1,RefScore1,PatternRefNo1,RefScoreLimit2,RefScore2,PatternRefNo2,PatternScoreLimit1,PatternScore1,PatternNo1,PatternScoreLimit2,PatternScore2,PatternNo2\r\n";
                            break;
                        case LogKind.AlignBendingVision:
                            baselog = "Data,CellID,BendNo,OKNG,MarkX1,MarkY1,MarkX2,MarkY2,AlignX,AlignY,AlignT,Length,DLPanelFind\r\n";
                            break;
                        case LogKind.AlignNoPress:
                            baselog = "Date,Count,bendingNO,x1,y1,x2,y2\r\n";
                            break;
                        case LogKind.MeasBending1:
                        case LogKind.MeasBending2:
                        case LogKind.MeasBending3:
                            baselog = "Date,Count,Arm NO, X1,Y1,X2,Y2,Dx1,Dy1,Dx2,Dy2,Rx1,Ry1,Rx2,Ry2,CellID\r\n";
                            break;
                        case LogKind.AutoMain:
                        case LogKind.System:
                        case LogKind.Interface:
                            baselog = "Date,Message\r\n";
                            break;
                        case LogKind.BDHistory: //201012 cjm 오타 수정
                        case LogKind.BDHistoryDay: //pcy190613추가 //201012 cjm 오타 수정
                            baselog = "Date,CellID,ToolNO,FirstLX,FirstLY,FirstRX,FirstRY,Retry,LastLX,LastLY,LastRX,LastRY\r\n";
                            break;
                        case LogKind.AttachHistory:
                        case LogKind.AttachHistoryDay:
                            baselog = "Date,CellID,ToolNO,LX,LY,RX,RY\r\n";
                            break;
                        case LogKind.UpperInspection:
                        case LogKind.UpperInspectionDay:
                            baselog = "Date,CellID,OKNG,BendNo,Retry,LX,LY,RX,RY,LastBD_LX,LastBD_LY,LastBD_RX,LastBD_RY,AfterBD_LX,AfterBD_LY,AfterBD_RX,AfterBD_RY,DiffX1,DiffY1,DiffX2,DiffY2,RefX1,RefY1,MarkX1,MarkY1,RefX2,RefY2,MarkX2,MarkY2\r\n";
                            break;
                        case LogKind.InspectionEdgetoMark:
                        case LogKind.InspectionEdgetoMarkDay:
                            baselog = "Date,CellID,OKNG,BendNo,Retry,DistX1,DistY1,DistX2,DistY2,RefX1,RefY1,MarkX1,MarkY1,RefX2,RefY2,MarkX2,MarkY2\r\n";
                            break;
                        case LogKind.PRUpperInsData:
                            baselog = "Date,Pre,Arm,PanelID,LX,LY,RX,RY\r\n";
                            break;
                        case LogKind.BDCalibration:
                            baselog = "Date,CamName,CalCnt,MarkX1,MarkY1,MarkX2,MarkY2\r\n";
                            break;
                        //MFQ
                        case LogKind.Calibration:
                            baselog = "Date,CamName,TotalCalCnt,CalCnt,MarkX1,MarkY1,MarkX2,MarkY2,MoveX,MoveY,MoveT\r\n";
                            break;
                        //MFQ
                        case LogKind.InspectionDist:
                            baselog = "Date,CamName,DistX1,DistY1,DistX2,DistY2\r\n";
                            break;
                        case LogKind.Shif:
                            baselog = "Date,BendNo,X1,Y1,X2,Y2\r\n";
                            break;
                        //MFQ
                        case LogKind.AlignMeasure:
                            baselog = "Data,CamName,MarkX1,MarkY1,MarkT1,MarkX2,MarY2,MarkT2,MarkX3,MarY3,MarkT3,MarkX4,MarY4,MarkT4\r\n";
                            break;
                        case LogKind.SCALE:
                            baselog = "Date,CamName,OKNG,SDistX1,SDistY1,SDistX2,SDistY2,DistX1,DistY1,DistX2,DistY2\r\n";
                            break;
                        case LogKind.CalibrationResult:  // 20200908 cjm 추가 Calibration 움직이는 점 저장하는 곳
                            baselog = "Date,CamName, 1_x, 1_y, 2_x, 2_y, 3_x, 3_y, 4_x, 4_y, 5_x, 5_y, 6_x, 6_y, 7_x, 7_y, 8_x, 8_y, 9_x, 9_y, T1_x, T1_y, T2_x, T2_y,MoveX,MoveY,MoveT\r\n";
                            break;
                        //20.10.16 lkw
                        case LogKind.AlignPre:
                            baselog = "Data,CellID,IndexNO,OKNG,MarkX1,MarkY1,MarkX2,MarkY2,MarkX3,MarkY3,diffX1,diffY1,diffX2,diffY2," +
                                "AlignX,AlignY,AlignT,Length,Time,DLMarkFind,scoreLimit1,score1,PatternNo1,scoreLimit2,score2,PatternNo2\r\n";
                            break;

                        case LogKind.AlignSCFPanel:
                        case LogKind.AlignSCFReel:
                        case LogKind.EMIAttach:
                        case LogKind.TempAttach:
                            baselog = "Data,CellID,OKNG,MarkX1,MarkY1,MarkX2,MarkY2,MarkX3,MarkY3,diffX1,diffY1,diffX2,diffY2," +
                                "AlignX,AlignY,AlignT,Length,Time,DLMarkFind,scoreLimit1,score1,PatternNo1,scoreLimit2,score2,PatternNo2,scoreLimit3,score3,PatternNo3\r\n";
                            break;
                        case LogKind.AlignSCFReelPickUp:
                            baselog = "Data,CellID,OKNG,MarkX1,MarkY1,MarkX2,MarkY2,MarkX3,MarkY3,diffX1,diffY1,diffX2,diffY2," +
                                "AlignX,AlignY,AlignT,Length,LengthY,Time,DLMarkFind,scoreLimit1,score1,PatternNo1,scoreLimit2,score2,PatternNo2,scoreLimit3,score3,PatternNo3\r\n";
                            break;
                        case LogKind.AttachAlign:
                            baselog = "Data,CellID,OKNG,MarkX1,MarkY1,MarkX2,MarkY2,MarkX3,MarkY3,diffX1,diffY1,diffX2,diffY2," +
                                "AlignX,AlignY,AlignT,Length1,Length2,Time,DLMarkFind,scoreLimit1,score1,PatternNo1,scoreLimit2,score2,PatternNo2,scoreLimit3,score3,PatternNo3,scoreLimit4,score4,PatternNo4\r\n";
                            break;
                        case LogKind.DetachInspection:
                            baselog = "Data,CellID,OKNG,MarkX1,MarkY1,MarkT1,LimitTH,ResultTH,Time,DLMarkFind,DLInspPreResult1,DLInspUSE1,DLInspResult1,\r\n";
                            break;
                        case LogKind.Laser:  //210118 cjm 레이저 로그
                            baselog = "Date,No,CellID,LaserSendCellID,MCRCellID\r\n";
                            break;
                        case LogKind.LaserAlign:  //210118 cjm 레이저 로그
                            baselog = "Date,No,CellID,OKNG,MarkX1,MarkY1,AlignX,AlignY,VisionOffsetX,VisionOffseY,APCOffsetX,APCOffsetY,RefdiffX,RefdiffY\r\n";
                            break;
                        case LogKind.LaserPositionInsp:
                            baselog = "Date,NO,CeLLID,OKNG,LX,LY,RX,RY,LXdiff,LYdiff,RXdiff,RYdiff,RefDiffX,RefDiffY,PLC_APN,Read_APN,Compare_APN\r\n";
                            break;
                        //210215 cjm TrayLoader Log Add
                        case LogKind.TrayLoader:
                            baselog = "Date,PanelExist,Time\r\n";
                            break;
                        case LogKind.CycleTimeBDAlign:
                            baselog = "Date,CellID,MarkSeach,Process,Total,Retry\r\n";
                            break;
                    }
                    baselog = baselog + lcString;
                }
                else
                {
                    baselog = lcString;
                }

                StreamWriter FileInfo = new StreamWriter(Filepath, true);

                try
                {
                    FileInfo.WriteLine(baselog);
                }
                catch
                { }
                finally
                {
                    FileInfo.Close();
                }

                // log 삭제 하기 변경. lyw. 170110.
                //logRemove.DirectoryRemove(lcPath, 24 * 30, true, false);
                //2018.07.24
                logRemove.DirectoryRemove(RemovePath, 24 * 30, true, false);

            }
            catch (Exception EX)
            {
                ExceptionLogSave("Save" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }


        //public void AlignLogSave(string Name, string lcString = "")
        //{
        //    try
        //    {
        //       // lcString = DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss") + "." + DateTime.Now.Millisecond.ToString("000") + "," + lcString;                

        //        string lcPath = "";

        //        string lcdate = DateTime.Now.ToString("yyyy-MM-dd");
        //        DateTime lcDT = DateTime.Now;
        //        string lcHour = lcDT.Hour.ToString("00");

        //        lcPath = Path.Combine(CONST.cVisionImgPath, Name.Trim(), "Log", lcdate); 
        //        string Filepath;

        //        Filepath = Path.Combine(lcPath, "Align_" + lcHour + ".txt");               

        //        if (!Directory.Exists(lcPath))
        //            Directory.CreateDirectory(lcPath);             

        //        StreamWriter FileInfo = new StreamWriter(Filepath, true);

        //        try
        //        {
        //            FileInfo.WriteLine(lcString);
        //        }
        //        catch
        //        { }
        //        finally
        //        {
        //            FileInfo.Close();
        //            FileInfo.Dispose();
        //        }

        //        // log 삭제 하기 변경. lyw. 170110.
        //        logRemove.DirectoryRemove(lcPath, 24 * 30, true, false);

        //    }
        //    catch (Exception EX)
        //    {
        //        ExceptionLogSave("Save" + "," + EX.GetType().Name + "," + EX.Message);
        //    }
        //}
        //public void InspectionSave(string lcString, string ModulePos)
        //{
        //    try
        //    {
        //        //lcString = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "." + DateTime.Now.Millisecond.ToString() + "  =   " + lcString;
        //        lcString = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "." + DateTime.Now.Millisecond.ToString() + " " + lcString;

        //        string lcdate = DateTime.Now.ToString("yyyy-MM-dd");
        //        DateTime lcDT = DateTime.Now;
        //        string lcHour = lcDT.Hour.ToString();

        //        if (!Directory.Exists(cInspectionLogPath + "/" + ModulePos + "/" + lcdate))
        //            Directory.CreateDirectory(cInspectionLogPath + "/" + ModulePos + "/" + lcdate);
        //        StreamWriter FileInfo = new StreamWriter(cInspectionLogPath + "/" + ModulePos + "/" + lcdate + "/" + lcdate + "-" + lcHour + ".log", true);
        //        FileInfo.WriteLine(lcString);
        //        FileInfo.Close();
        //        FileInfo.Dispose();

        //        // log 삭제 하기 변경. lyw. 170110.
        //        logRemove.DirectoryRemove(cInspectionLogPath, 24 * 30, true, false);                
        //    }
        //    catch
        //    { }
        //}


        public void DistLogSave(string Name, string lcString = "")
        {
            try
            {
                string lcPath = "";
                long dateCnt = DateTime.Now.Year * 12 * 30;
                dateCnt = dateCnt + DateTime.Now.Month * 30;
                dateCnt = dateCnt + DateTime.Now.Day;

                lcPath = Path.Combine(CONST.cVisionPath, Name.Trim(), "Dist");
                string Filepath;

                Filepath = Path.Combine(lcPath, dateCnt.ToString() + ".txt");

                if (!Directory.Exists(lcPath))
                    Directory.CreateDirectory(lcPath);

                StreamWriter FileInfo = new StreamWriter(Filepath, true);

                try
                {
                    FileInfo.WriteLine(lcString);
                }
                catch
                { }
                finally
                {
                    FileInfo.Close();
                    FileInfo.Dispose();
                }

                // log 삭제 하기 변경. lyw. 170110.
                logRemove.DirectoryRemove(lcPath, 24 * 30, true, false);

            }
            catch (Exception EX)
            {
                ExceptionLogSave("DistLogSave" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }
        //pcy190506 Datetime비교형식으로 수정.
        public void ReadOKNGLog(string start, string end, ref int[,] LogData)
        {
            try
            {
                string lcPath = Path.Combine(cLogPath, "OKNGCount");
                int FileStartNo = -1;
                int FileEndNo = -1;
                int nNo = -1;
                bool bcheck = false;

                string[] sstarttime = start.Split('-');
                string[] sendtime = end.Split('-');
                DateTime Starttime = new DateTime(int.Parse(sstarttime[0]), int.Parse(sstarttime[1]), int.Parse(sstarttime[2]));
                DateTime Endtime = new DateTime(int.Parse(sendtime[0]), int.Parse(sendtime[1]), int.Parse(sendtime[2]));
                DateTime Foldertime = new DateTime();

                if (Directory.Exists(lcPath))
                {
                    string[] Folders = Directory.GetDirectories(lcPath);

                    for (int i = 0; i < Folders.Length; i++)
                    {
                        string[] Data = Folders[i].Split('\\');
                        //string[] sFoldertime = Data[1].Split('-');
                        string[] sFoldertime = Data[4].Split('-');
                        Foldertime = new DateTime(int.Parse(sFoldertime[0]), int.Parse(sFoldertime[1]), int.Parse(sFoldertime[2]));
                        if (Starttime >= Foldertime)
                        {
                            FileStartNo = i;
                        }
                        if (Endtime >= Foldertime)
                        {
                            FileEndNo = i;
                        }
                        if (Starttime == Foldertime || Endtime == Foldertime)
                        {
                            bcheck = true;
                        }
                        //if (start == Data[1]) FileStartNo = i;
                        //else if (end == Data[1]) FileEndNo = i;
                    }
                    //아무것도 못찾았을때 갖고있는 전부를 출력. (여기 들어올일 없지만 혹시나..)
                    if (FileStartNo == -1) FileStartNo = 0;
                    else if (FileEndNo == -1) FileEndNo = Folders.Length - 1;

                    if (!bcheck) return;

                    for (int i = FileStartNo; i < FileEndNo + 1; i++)
                    {
                        string[] file = Directory.GetFiles(Folders[i]);

                        for (int j = 0; j < file.Length; j++)
                        {
                            string[] CamName = file[j].Split('\\', '.');
                            //nNo = Bending.Menu.frmAutoMain.eCamNametoNo(CamName[2]);
                            nNo = Bending.Menu.frmAutoMain.eCamNametoNo(CamName[5]);

                            FileStream FilePath = new FileStream(file[j], FileMode.OpenOrCreate, FileAccess.Read);
                            StreamReader FileReader = new StreamReader(FilePath, Encoding.UTF8);
                            FileReader.ReadLine();
                            string strData = FileReader.ReadLine();
                            string[] Sprit;
                            Sprit = strData.Split(',');

                            for (int k = 0; k < Sprit.Length; k++)
                            {
                                if (int.TryParse(Sprit[k], out int value))
                                {
                                    LogData[nNo, k] = LogData[nNo, k] + value;
                                }
                            }

                            FileReader.Close();
                            FilePath.Close();
                        }
                    }

                }
            }
            catch
            {

            }
        }

        public void ReadDistLog(ref string[] logData, long start, long end, string Name)
        {
            try
            {
                string lcPath = Path.Combine(CONST.cVisionPath, Name.Trim(), "Dist");
                int dataCnt = 0;

                if (Directory.Exists(lcPath))
                {
                    string[] files = Directory.GetFiles(lcPath);

                    for (long i = start; i <= end; i++)
                    {
                        string Filepath = Path.Combine(lcPath, i.ToString() + ".txt");
                        if (File.Exists(Filepath))
                        {
                            string[] filedata = File.ReadAllLines(Filepath);
                            dataCnt = dataCnt + filedata.Length;
                        }
                    }
                }

                logData = new string[dataCnt];
                int startLog = 0;
                if (Directory.Exists(lcPath))
                {
                    string[] files = Directory.GetFiles(lcPath);

                    for (long i = start; i <= end; i++)
                    {
                        string Filepath = Path.Combine(lcPath, i.ToString() + ".txt");
                        if (File.Exists(Filepath))
                        {
                            string[] filedata = File.ReadAllLines(Filepath);
                            for (int j = 0; j < filedata.Length; j++)
                            {
                                logData[startLog + j] = filedata[j];
                            }
                            startLog = startLog + filedata.Length;
                        }
                    }
                }

            }
            catch { }
        }

        public void WriteOKNGCount(string Name, int[] nData, int nIndex)
        {
            try
            {
                string lcPath = "";
                string RemovePath = "";
                string lcdate = DateTime.Now.ToString("yyyy-MM-dd");
                DateTime lcDT = DateTime.Now;

                //lcPath = Path.Combine(cLogPath, lcdate);
                //lcPath = Path.Combine(lcPath, "OKNGCount");

                lcPath = Path.Combine(cLogPath, "OKNGCount");
                lcPath = Path.Combine(lcPath, lcdate);

                //삭제 경로 추가
                RemovePath = Path.Combine(cLogPath, "OKNGCount");

                string Filepath;

                Filepath = Path.Combine(lcPath, Name + ".csv");

                if (!Directory.Exists(lcPath))
                    Directory.CreateDirectory(lcPath);

                FileStream FilePath = new FileStream(Filepath, FileMode.OpenOrCreate, FileAccess.Write);

                StreamWriter FileWrite = new StreamWriter(FilePath, Encoding.UTF8);

                //여기는 노가다.
                //20.12.17 lkw DL
                FileWrite.WriteLine("WAIT,OK,BY_PASS,ALIGN_LIMIT,SPEC_OVER,CHECK,WORKER_BY_PASS,MANUAL_BENDING,INIT,RETRY_OVER,PANEL_SHIFT_NG,RETRY,ERROR_MARK,FIRST_LIMIT,ERROR_LCHECK,VISION_REPLY_WAIT,DL");

                string strData = String.Join(",", nData);

                FileWrite.WriteLine(strData);

                FileWrite.Close();
                FilePath.Close();

                logRemove.DirectoryRemove(RemovePath, 24 * 30, true, false);
            }
            catch (Exception EX)
            {
                ExceptionLogSave("OKNGLogSave" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }

        public void ReadOKNGCount(string Name, ref int[] nData)
        {
            try
            {
                string lcPath = "";

                string lcdate = DateTime.Now.ToString("yyyy-MM-dd");
                DateTime lcDT = DateTime.Now;

                //lcPath = Path.Combine(cLogPath, lcdate);
                //lcPath = Path.Combine(lcPath, "OKNGCount");

                lcPath = Path.Combine(cLogPath, "OKNGCount");
                lcPath = Path.Combine(lcPath, lcdate);

                string Filepath;

                Filepath = Path.Combine(lcPath, Name + ".csv");

                if (!File.Exists(Filepath))
                {
                    for (int i = 0; i < nData.Length; i++)
                    {
                        nData[i] = 0;
                    }
                }
                else
                {
                    FileStream FilePath = new FileStream(Filepath, FileMode.OpenOrCreate, FileAccess.Read);

                    StreamReader FileReader = new StreamReader(FilePath, Encoding.UTF8);

                    FileReader.ReadLine();

                    string strData = FileReader.ReadLine();

                    string[] Sprit;
                    Sprit = strData.Split(',');

                    for (int i = 0; i < nData.Length; i++)
                    {
                        nData[i] = int.Parse(Sprit[i].ToString());
                    }

                    FileReader.Close();
                    FilePath.Close();
                }

            }
            catch (Exception EX)
            {
                ExceptionLogSave("OKNGLogSave" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }

        //private void SaveMotionDataCSV(string strData)
        //{
        //    try
        //    {
        //        string lcPath = "";

        //        string lcdate = DateTime.Now.ToString("yyyy-MM-dd");
        //        DateTime lcDT = DateTime.Now;

        //        lcPath = Path.Combine(cLogPath, lcdate);
        //        lcPath = Path.Combine(lcPath, "MotionData");

        //        string Filepath;

        //        Filepath = Path.Combine(lcPath, "MotionData.csv");

        //        if (!Directory.Exists(lcPath))
        //            Directory.CreateDirectory(lcPath);

        //        FileStream FilePath = new FileStream(Filepath, FileMode.OpenOrCreate, FileAccess.Write);

        //        StreamWriter FileWrite = new StreamWriter(FilePath, Encoding.UTF8);


        //        FileWrite.WriteLine(strData);

        //        FileWrite.Close();
        //        FilePath.Close();
        //    }
        //    catch (Exception EX)
        //    {
        //        ExceptionLogSave("MotionDataSave" + "," + EX.GetType().Name + "," + EX.Message);
        //    }
        //}
        // 20200914 cjm 저장된 Calibration 점 읽는 곳
        // 20200923 cjm Cam1Cal2 찍는 방식 변경
        // 20201003 cjm 주소 변경
        public void CalibrationPointReader(int calPosNum, ref string[] Sprit, int iline)
        {
            //20200928 cjm Arm과 Transfer 구분
            //string path = Path.Combine(cLogPath, "CalibrationResult", CamName + ".csv");
            string path;
            //if (iCalkind < 1)
            //    path = Path.Combine(cLogPath, "CalibrationResult", CamName + ".csv");
            //else
            //    path = Path.Combine(cLogPath, "CalibrationResult", CamName + "_Arm" + ".csv");

            string CalPointLogPath = Path.Combine(CONST.Folder2, @"EQData\Calibration");

            CalPointLogPath = Path.Combine(CalPointLogPath, "CAL" + calPosNum + "Data");
            path = Path.Combine(CalPointLogPath, "CAL" + calPosNum + "Data.csv");

            string[] filedata = File.ReadAllLines(path);
            string Lastfiledata = filedata[filedata.Length + iline];

            Sprit = Lastfiledata.Split(',');
        }
    }
}
