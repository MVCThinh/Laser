using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Windows.Forms;
using System.Threading;
using Cognex.VisionPro;
using System.Diagnostics;
using System.IO;
using rs2DAlign;
using Cognex.VisionPro.Dimensioning;
using Cognex.VisionPro.ImageProcessing;

namespace Bending
{
    public partial class ucAutoMain
    {
        
        //Stopwatch sBendPre = new Stopwatch();
        private void BendPre()
        {
            #region Bending Pre
            //TimeOutCheck2(CONST.AAM_PLC1.BitControl.plcBendPreAlignReq, eTimeOutType.Process);

            //if (CONST.bPLCReq[CONST.AAM_PLC1.BitControl.plcBendPreAlignReq] && pcResult[CONST.AAM_PLC1.Reply.pcBendPreAlignReply] == (int)ePCResult.WAIT)
            //{
            //    if (!visionBackground[CONST.AAM_PLC1.BitControl.plcBendPreAlignReq])
            //    {
            //        visionBackground[CONST.AAM_PLC1.BitControl.plcBendPreAlignReq] = true;
            //        TimeOutCheck2(CONST.AAM_PLC1.BitControl.plcBendPreAlignReq, eTimeOutType.Set);

            //        short visionNo1 = Vision_No.vsBendPre1;
            //        short visionNo2 = Vision_No.vsBendPre2;
            //        short visionNo3 = Vision_No.vsBendPre3;
            //        ePCResult iPCResult = ePCResult.WAIT;
            //        ePCResult iSCFResult = ePCResult.WAIT;
            //        cs2DAlign.ptAlignResult align = new cs2DAlign.ptAlignResult();
            //        cs2DAlign.ptAlignResult align180 = new cs2DAlign.ptAlignResult();

            //        Vision[visionNo1].cogDS.InteractiveGraphics.Clear();
            //        Vision[visionNo2].cogDS.InteractiveGraphics.Clear();
            //        Vision[visionNo3].cogDS.InteractiveGraphics.Clear();
                    
            //        sBendPre.Reset();
            //        sBendPre.Start();

            //        BackgroundWorker wkr = new BackgroundWorker();

            //        wkr.DoWork += delegate (object sender1, DoWorkEventArgs e1)
            //        {
            //            if (CONST.m_bSystemLog)
            //                cLog.Save(LogKind.System, "plcBendingPreReq on");

            //            cs2DAlign.ptXYT Mark1 = new cs2DAlign.ptXYT();
            //            cs2DAlign.ptXYT Mark2 = new cs2DAlign.ptXYT();
            //            cs2DAlign.ptXYT Mark3 = new cs2DAlign.ptXYT();

            //            bool b1Mark = false;
            //            bool b2Mark = false;
            //            bool b3Mark = false;
            //            double[] scorelogData = null;

            //            //마크찾기
            //            PParam param = new PParam();
            //            param.qkind.Enqueue(ePatternKind.Panel);
            //            param.qkind.Enqueue(ePatternKind.Panel);
            //            param.qkind.Enqueue(ePatternKind.Panel);
            //            FindThread(param, visionNo1, true, out List<cs2DAlign.ptXYT> pt, out List<bool> bResult);
            //            SetFindResult(pt, bResult, ref b1Mark, ref b2Mark, ref b3Mark, ref Mark1, ref Mark2, ref Mark3);

            //            //메뉴얼마크 또는 임시마크 사용(순서 메뉴얼마크좌표 -> 임시마크)
            //            if (!manualMark[visionNo1].MarkInfo[mPanel].selected)
            //            {
            //                if (!b1Mark)
            //                    b1Mark = Vision[visionNo1].RunTempPMAlign(ref Mark1, mPanel);
            //            }
            //            else
            //            {
            //                b1Mark = manualMark[visionNo1].manualMarkSelect(ref Mark1, mPanel, b1Mark);
            //            }
            //            if (!manualMark[visionNo2].MarkInfo[mPanel].selected)
            //            {
            //                if (!b2Mark)
            //                    b2Mark = Vision[visionNo2].RunTempPMAlign(ref Mark2, mPanel);
            //            }
            //            else
            //            {
            //                b2Mark = manualMark[visionNo2].manualMarkSelect(ref Mark2, mPanel, b2Mark);
            //            }

            //            //여기까지 못찾았으면 일단 마크못찾았다고 판단.
            //            if (!b1Mark || !b2Mark)
            //            {
            //                iPCResult = ePCResult.ERROR_MARK;
            //            }

            //            //길이체크
            //            //20.10.11 lkw L-Check 측정은 무조건 진행...
            //            if (iPCResult == ePCResult.WAIT)
            //            {
            //                double offset = Menu.frmSetting.revData.mLcheck[(short)eCamNO1.BendPre1].LCheckOffset1;
            //                iPCResult = Lcheck((int)eCalPos.BendPre1, Mark1, Mark2, eRecipe.BENDING_PRE_LCHECK_SPEC1, eRecipe.BENDING_PRE_LCHECK_TOLERANCE1, offset, ref LDist[2]);
            //                if (iPCResult != ePCResult.WAIT)
            //                {
            //                    b1Mark = false;
            //                    b2Mark = false;
            //                }
            //            }

            //            //여기까지 정상이 아니면 마크 못찾았다고 판단
            //            if (iPCResult != ePCResult.WAIT)
            //            {
            //                if (Vision[visionNo1].CFG.PatternFailManualIn)
            //                {
            //                    if (!b1Mark)
            //                    {
            //                        if (manualMark[visionNo1].MarkInfo[mPanel].Popupcnt < manualPopupCnt)
            //                        {
            //                            manualMark[visionNo1].MarkInfo[mPanel].manualPopup = true;
            //                            manualMark[visionNo1].MarkInfo[mPanel].patternKind = ePatternKind.Panel;
            //                        }
            //                    }
            //                    if (!b2Mark)
            //                    {
            //                        if (manualMark[visionNo2].MarkInfo[mPanel].Popupcnt < manualPopupCnt)
            //                        {
            //                            manualMark[visionNo2].MarkInfo[mPanel].manualPopup = true;
            //                            manualMark[visionNo2].MarkInfo[mPanel].patternKind = ePatternKind.Panel;
            //                        }
            //                    }
            //                    if (manualMark[visionNo1].MarkInfo[mPanel].manualPopup
            //                    || manualMark[visionNo2].MarkInfo[mPanel].manualPopup)
            //                        iPCResult = ePCResult.VISION_REPLY_WAIT;
            //                    else
            //                        iPCResult = ePCResult.ERROR_MARK;
            //                }
            //                else
            //                {
            //                    iPCResult = ePCResult.ERROR_MARK;
            //                }
            //            }

            //            if (iPCResult == ePCResult.WAIT) //정상진행
            //            {
            //                //mark ref 순서확인
            //                iPCResult = AlignXYT(visionNo1, (int)eCalPos.BendPre1, Mark1, Mark2, ref align);
            //                //iPCResult = AlignXYT(visionNo1, (int)eCalPos.BendPre1, Mark1, Mark2, ref align180, true, 180);

            //                NotUseXYT(visionNo1, ref align);
            //                //NotUseXYT(visionNo1, ref align180);
            //                ReverseXYT(visionNo1, ref align);
            //                //ReverseXYT(visionNo1, ref align180);
            //            }
            //            if (iPCResult == ePCResult.WAIT)
            //            {
            //                iPCResult = CompareLimit(visionNo1, align);
            //                //iPCResult = CompareLimit(visionNo1, align180);
            //            }

            //            if (iPCResult == ePCResult.WAIT)
            //            {
            //                //IF.writeAlignXYTOffset((int)CONST.AAM_PLC1.Position.BendPre.MotionNo, align.X, align.Y, align.T);
            //                IF.writeAlignXYTOffset(CONST.AAM_PLC1.OffsetAddress.BendPre0, align.X, align.Y, align.T);
            //                //IF.writeAlignXYTOffset(CONST.AAM_PLC1.OffsetAddress.BendPre180, align180.X, align180.Y, align180.T);
                            
            //            }
            //            //여기까지 align 결과값 보내기
            //            if (iPCResult == ePCResult.WAIT)
            //            {
            //                iPCResult = ePCResult.OK;
                            
            //            }
            //            SetResult(nameof(eCamNO1.BendPre1), (ePCResult)iPCResult, visionNo1, CONST.AAM_PLC1.Reply.pcBendPreAlignReply, align);

            //            if (iPCResult != ePCResult.RETRY && iPCResult != ePCResult.VISION_REPLY_WAIT)
            //            {
            //                sBendPre.Stop();
            //                //20.10.16 lkw
            //                CONST.PanelIDBendPre = "";
            //                IF.SendData("PNID,BP," + CONST.PLCDeviceType + CONST.Address.PLC.CELLID5.ToString());
            //                //DV 항목
            //                int iscorePanel = CONST.AAM_PLC1.MatchingScore.BendPre;
            //                double[] scoreData = { Vision[visionNo1].GetVisionSearchScorePanel(), Vision[visionNo2].GetVisionSearchScorePanel() };
            //                IF.DataWrite_Score_OneWord(iscorePanel, scoreData);
            //                scorelogData = scoreData;
            //            }
            //            if (iPCResult == ePCResult.OK && iSCFResult == ePCResult.WAIT)
            //            {
            //                if (Menu.frmSetting.revData.mBendingPre.BDPreSCFInspection)
            //                {
            //                    #region CHECK LOG .......... 20201101 ith add
            //                    if (DateTime.Now.ToString("yyyyMMdd") == "20201101")
            //                    {
            //                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "..." + "BPSCFInspection Start..."); 
            //                    }
            //                    #endregion

            //                    //Bending Pre OK 인 경우 SCF 검사 추가
            //                    if (!b3Mark)   //Mark 못 찾은 경우 Target 기준으로 검사 진행함.
            //                    {
            //                        Mark3.X = Vision[visionNo3].CFG.TargetX[0];
            //                        Mark3.Y = Vision[visionNo3].CFG.TargetY[0];
            //                    }

            //                    iSCFResult = BPSCFInspection(Mark1, Mark2, Mark3);
            //                    #region CHECK LOG .......... 20201101 ith add
            //                    if (DateTime.Now.ToString("yyyyMMdd") == "20201101")
            //                    {
            //                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "..." + "BPSCFInspection End...");
            //                    }
            //                    #endregion
            //                }
            //            }
            //            if (iPCResult == ePCResult.OK && iSCFResult == ePCResult.WAIT)
            //            {
            //                if (Menu.frmSetting.revData.mBendingPre.BDPreSCFInspection)
            //                {
            //                    #region CHECK LOG .......... 20201101 ith add
            //                    if (DateTime.Now.ToString("yyyyMMdd") == "20201101")
            //                    {
            //                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "..." + "BPSCFexistence Start...");
            //                    }
            //                    #endregion

            //                    if (!b3Mark)   //Mark 못 찾은 경우 Target 기준으로 검사 진행함.
            //                    {
            //                        Mark3.X = Vision[visionNo3].CFG.TargetX[0];
            //                        Mark3.Y = Vision[visionNo3].CFG.TargetY[0];
            //                    }
            //                    iSCFResult = BPSCFexistence(Mark1, Mark2, Mark3, true);
            //                    #region CHECK LOG .......... 20201101 ith add
            //                    if (DateTime.Now.ToString("yyyyMMdd") == "20201101")
            //                    {
            //                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "..." + "BPSCFexistence End...");
            //                    }
            //                    #endregion
            //                }
            //            }
            //            if (Menu.frmSetting.revData.mBendingPre.BDPreSCFInspResultPass) iSCFResult = ePCResult.WAIT; //물류(검사는 하되 OK만 주고싶을때)
            //            //SCF Inspection 결과값 보내기
            //            if (iSCFResult == ePCResult.WAIT)
            //            {
            //                iSCFResult = ePCResult.OK;
            //            }

            //            pcResult[CONST.AAM_PLC1.Reply.pcBendPreSCFInspectionReply] = (int)iSCFResult;
            //            LogDisp(visionNo1, "SCFInspection End(" + iSCFResult.ToString() + ")");

            //            if (iPCResult != ePCResult.RETRY && iPCResult != ePCResult.VISION_REPLY_WAIT)
            //            {
            //                bCalcCPK = true; //cpk 나중에 계산함.
            //                if (iPCResult != ePCResult.OK || iSCFResult != ePCResult.OK)
            //                {
            //                    Vision[visionNo1].ImageSave(false, Vision[visionNo1].CFG.ImageSaveType, iPCResult.ToString(), iSCFResult.ToString());
            //                    Vision[visionNo2].ImageSave(false, Vision[visionNo2].CFG.ImageSaveType, iPCResult.ToString(), iSCFResult.ToString());
            //                    if (Vision[visionNo1].CFG.AlignUse) Vision[visionNo3].ImageSave(false, Vision[visionNo3].CFG.ImageSaveType, iPCResult.ToString(), iSCFResult.ToString());
            //                }
            //                else
            //                {
            //                    if (Vision[visionNo1].CFG.ImageSave) Vision[visionNo1].ImageSave(true, Vision[visionNo1].CFG.ImageSaveType, iPCResult.ToString());
            //                    if (Vision[visionNo2].CFG.ImageSave) Vision[visionNo2].ImageSave(true, Vision[visionNo2].CFG.ImageSaveType, iPCResult.ToString());
            //                    if (Vision[visionNo3].CFG.ImageSave && Vision[visionNo1].CFG.AlignUse) Vision[visionNo3].ImageSave(true, Vision[visionNo3].CFG.ImageSaveType, iPCResult.ToString());
            //                }
            //                //sBendPre.Stop();
            //                WriteCycleTime(visionNo1, sBendPre.ElapsedMilliseconds);
            //                //로그 저장
            //                LogSavePre(LogKind.AlignPre, (eCamNO)eCamNO1.BendPre1, Mark1, Mark2, align, LDist[2], CONST.PanelIDBendPre, iPCResult.ToString(), (Convert.ToDouble(sBendPre.ElapsedMilliseconds) / 1000), scorelogData, Mark3);
            //            }
            //        };
            //        wkr.RunWorkerCompleted += delegate (object sender1, RunWorkerCompletedEventArgs e2)
            //        {
            //            if (CONST.m_bSystemLog)
            //                cLog.Save(LogKind.System, "plcBendingPre PatternSearch End");
            //            WritePanleID(visionNo1, CONST.PanelIDBendPre);
            //            visionBackground[CONST.AAM_PLC1.BitControl.plcBendPreAlignReq] = false;
            //            wkr.Dispose();
            //        };
            //        wkr.RunWorkerAsync();
            //    }
            //}
            //else if (!CONST.bPLCReq[CONST.AAM_PLC1.BitControl.plcBendPreAlignReq]
            //    && pcResult[CONST.AAM_PLC1.Reply.pcBendPreAlignReply] != (int)ePCResult.WAIT
            //    && pcResult[CONST.AAM_PLC1.Reply.pcBendPreSCFInspectionReply] != (int)ePCResult.WAIT)
            //{
            //    TimeOutCheck2(CONST.AAM_PLC1.BitControl.plcBendPreAlignReq, eTimeOutType.UnSet);
            //    pcResult[CONST.AAM_PLC1.Reply.pcBendPreAlignReply] = (int)ePCResult.WAIT;
            //    pcResult[CONST.AAM_PLC1.Reply.pcBendPreSCFInspectionReply] = (int)ePCResult.WAIT;
            //    ManualMarkInitial(Vision_No.vsBendPre1); //수동마크 초기화
            //}
            //else if (CONST.bPLCReq[CONST.AAM_PLC1.BitControl.plcBendPreAlignReq]
            //    && pcResult[CONST.AAM_PLC1.Reply.pcBendPreAlignReply] == (int)ePCResult.VISION_REPLY_WAIT
            //    && manualMark[Vision_No.vsBendPre1].bWorkerVisible && manualMark[Vision_No.vsBendPre1].bWorkerVisible)
            //{
            //    CheckManualMarkPopup(Vision_No.vsBendPre1);  //수동 마크창 세팅
            //    if (CheckManualMarkDone(Vision_No.vsBendPre1)) //수동마크 작업자가 완료했는지..
            //    {
            //        pcResult[CONST.AAM_PLC1.Reply.pcBendPreAlignReply] = (int)ePCResult.WAIT;
            //        pcResult[CONST.AAM_PLC1.Reply.pcBendPreSCFInspectionReply] = (int)ePCResult.WAIT;
            //    }
            //    else if (CheckManualMarkBypass(Vision_No.vsBendPre1)) //수동마크 bypass인지..
            //    {
            //        ePCResult iPCResult = ePCResult.WORKER_BY_PASS;
            //        SetResult(nameof(eCamNO1.BendPre1), (ePCResult)iPCResult, Vision_No.vsBendPre1, CONST.AAM_PLC1.Reply.pcBendPreAlignReply);
            //        if (iPCResult != ePCResult.OK)
            //        {
            //            Vision[Vision_No.vsBendPre1].ImageSave(false, Vision[Vision_No.vsBendPre1].CFG.ImageSaveType, iPCResult.ToString());
            //            Vision[Vision_No.vsBendPre2].ImageSave(false, Vision[Vision_No.vsBendPre2].CFG.ImageSaveType, iPCResult.ToString());
            //            if (Vision[Vision_No.vsBendPre1].CFG.AlignUse) Vision[Vision_No.vsBendPre3].ImageSave(false, Vision[Vision_No.vsBendPre3].CFG.ImageSaveType, iPCResult.ToString());
            //        }
            //        sBendPre.Stop();
            //        WriteCycleTime(Vision_No.vsBendPre1, sBendPre.ElapsedMilliseconds);
            //    }
            //}

            #endregion       
        }
        //double[] histomeanx1 = new double[20];
        //double[] histomeanx2 = new double[20];
        //int histocnt = 0;
        //bool bhisto = false;
        //bool bCalcCPK = false;
        //public ePCResult BPSCFInspection(cs2DAlign.ptXYT refPixel1, cs2DAlign.ptXYT refPixel2, cs2DAlign.ptXYT refPixel3)
        //{
        //    cs2DAlign.ptXY offset1 = new cs2DAlign.ptXY();  // 마크 기준 측정 위치 받아오는 것 추가 해야함.
        //    cs2DAlign.ptXY offset2 = new cs2DAlign.ptXY();  // 마크 기준 측정 위치 받아오는 것 추가 해야함.
        //    int TH1 = (int)Menu.frmSetting.revData.mBendingPre.SCFInspCam1TH;
        //    int TH2 = (int)Menu.frmSetting.revData.mBendingPre.SCFInspCam1TH2;

        //    double[] result = new double[6];

        //    //20200919 cjm Bending Pre SCF Inspection offset 추가
        //    //20200926 cjm 변경
        //    offset1.X = Menu.frmSetting.revData.mBendingPre.SCFInspCam1OffsetX1;
        //    offset1.Y = Menu.frmSetting.revData.mBendingPre.SCFInspCam1OffsetY1;
        //    offset2.X = Menu.frmSetting.revData.mBendingPre.SCFInspCam1OffsetX2;
        //    offset2.Y = Menu.frmSetting.revData.mBendingPre.SCFInspCam1OffsetY2;
        //    //Cam1
        //    cs2DAlign.ptXY resolution = new cs2DAlign.ptXY();
        //    cs2DAlign.ptXY pixelCnt = new cs2DAlign.ptXY();
        //    Menu.rsAlign.getResolution((int)eCalPos.BendPre1, ref resolution, ref pixelCnt);
        //    Vision[Vision_No.vsBendPre1].scfInspResolutionX = resolution.X;
        //    Vision[Vision_No.vsBendPre1].scfInspResolutionY = resolution.Y;
        //    //20.10.06 lkw
        //    Vision[Vision_No.vsBendPre1].scfInspectionXSpec = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X1_SPEC];
        //    Vision[Vision_No.vsBendPre1].scfInspectionYSpec = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y1_SPEC];
        //    Vision[Vision_No.vsBendPre1].scfInspectionTor = Menu.frmSetting.revData.mBendingPre.SCFTolerance;

        //    result[0] = Vision[Vision_No.vsBendPre1].SCFAttachInspection(refPixel1, offset1, TH1, csVision.escfInspKind.LefttoRight, out CogRectangleAffine dispBox0, true);  //가로 방향
        //    result[1] = Vision[Vision_No.vsBendPre1].SCFAttachInspection(refPixel1, offset2, TH2, csVision.escfInspKind.UptoDown, out CogRectangleAffine dispBox1);  //세로 방향

        //    //offset 설정.....
        //    //20200919 cjm Bending Pre SCF Inspection offset 추가
        //    //20200926 cjm 변경
        //    offset1.X = Menu.frmSetting.revData.mBendingPre.SCFInspCam2OffsetX1;
        //    offset1.Y = Menu.frmSetting.revData.mBendingPre.SCFInspCam2OffsetY1;
        //    offset2.X = Menu.frmSetting.revData.mBendingPre.SCFInspCam2OffsetX2;
        //    offset2.Y = Menu.frmSetting.revData.mBendingPre.SCFInspCam2OffsetY2;
        //    TH1 = (int)Menu.frmSetting.revData.mBendingPre.SCFInspCam2TH;
        //    TH2 = (int)Menu.frmSetting.revData.mBendingPre.SCFInspCam2TH2;
        //    //Cam2

        //    Menu.rsAlign.getResolution((int)eCalPos.BendPre2, ref resolution, ref pixelCnt);
        //    Vision[Vision_No.vsBendPre2].scfInspResolutionX = resolution.X;
        //    Vision[Vision_No.vsBendPre2].scfInspResolutionY = resolution.Y;

        //    //20.10.06 lkw
        //    Vision[Vision_No.vsBendPre2].scfInspectionXSpec = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X1_SPEC];
        //    Vision[Vision_No.vsBendPre2].scfInspectionYSpec = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y1_SPEC];
        //    Vision[Vision_No.vsBendPre2].scfInspectionTor = Menu.frmSetting.revData.mBendingPre.SCFTolerance;

        //    result[2] = Vision[Vision_No.vsBendPre2].SCFAttachInspection(refPixel2, offset1, TH1, csVision.escfInspKind.RighttoLeft, out CogRectangleAffine dispBox2, true);  //가로 방향
        //    result[3] = Vision[Vision_No.vsBendPre2].SCFAttachInspection(refPixel2, offset2, TH2, csVision.escfInspKind.UptoDown, out CogRectangleAffine dispBox3);  //세로 방향

        //    //최소값 구하기.
        //    double histomeanx1min = 999;
        //    //int cnt1 = 0;
        //    foreach (var s in histomeanx1)
        //    {
        //        if (s != 0)
        //        {
        //            if (histomeanx1min > s) histomeanx1min = s;
        //            //cnt1++;
        //        }
        //    }
        //    //histomeanx1min = histomeanx1min / cnt1;
        //    double histomeanx2min = 999;
        //    //int cnt2 = 0;
        //    foreach (var s in histomeanx2)
        //    {
        //        if (s != 0)
        //        {
        //            if (histomeanx2min > s) histomeanx2min = s;
        //            //histomeanx2min += s;
        //            //cnt2++;
        //        }
        //    }
        //    //histomeanx2min = histomeanx2min / cnt2;

        //    CogHistogramTool histogramTool = new CogHistogramTool();
        //    histogramTool.InputImage = Vision[Vision_No.vsBendPre1].cogDS.Image;
        //    histogramTool.Region = dispBox0;
        //    histogramTool.Run();
        //    histomeanx1[histocnt] = histogramTool.Result.Mean;
        //    histogramTool.InputImage = Vision[Vision_No.vsBendPre2].cogDS.Image;
        //    histogramTool.Region = dispBox2;
        //    histogramTool.Run();
        //    histomeanx2[histocnt] = histogramTool.Result.Mean;
            

        //    //20200919 cjm Bending Pre SCF Inspection offset 추가
        //    //20200926 cjm 변경
        //    offset1.X = Menu.frmSetting.revData.mBendingPre.SCFInspCam3OffsetX1;
        //    offset1.Y = Menu.frmSetting.revData.mBendingPre.SCFInspCam3OffsetY1;
        //    offset2.X = Menu.frmSetting.revData.mBendingPre.SCFInspCam3OffsetX2;
        //    offset2.Y = Menu.frmSetting.revData.mBendingPre.SCFInspCam3OffsetY2;
        //    TH1 = (int)Menu.frmSetting.revData.mBendingPre.SCFInspCam3TH;
        //    TH2 = (int)Menu.frmSetting.revData.mBendingPre.SCFInspCam3TH2;
        //    //Cam3

        //    Menu.rsAlign.getResolution((int)eCalPos.BendPre3, ref resolution, ref pixelCnt);
        //    Vision[Vision_No.vsBendPre3].scfInspResolutionX = resolution.X;
        //    Vision[Vision_No.vsBendPre3].scfInspResolutionY = resolution.Y;

        //    //20.10.06 lkw
        //    Vision[Vision_No.vsBendPre3].scfInspectionXSpec = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X2_SPEC];
        //    Vision[Vision_No.vsBendPre3].scfInspectionYSpec = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y2_SPEC];
        //    Vision[Vision_No.vsBendPre3].scfInspectionTor = Menu.frmSetting.revData.mBendingPre.SCFTolerance;

        //    //result[4] = Vision[Vision_No.vsBendPre3].SCFAttachInspection(refPixel3, offset1, TH1, csVision.escfInspKind.DowntoUp, true);  //세로 방향
        //    //result[5] = Vision[Vision_No.vsBendPre3].SCFAttachInspection(refPixel3, offset2, TH2, csVision.escfInspKind.DowntoUp, true);  //세로 방향
        //    double dInRadius = Menu.frmSetting.revData.mBendingPre.SCFInspCam3InRadius;
        //    double dOutRadius = Menu.frmSetting.revData.mBendingPre.SCFInspCam3OutRadius;
        //    double dInSearchLength = Menu.frmSetting.revData.mBendingPre.SCFInspCam3InSearchLength;
        //    double dOutSearchLength = Menu.frmSetting.revData.mBendingPre.SCFInspCam3OutSearchLength;

        //    double SCFInspCam3CalipherCount = Menu.frmSetting.revData.mBendingPre.SCFInspCam3CalipherCount;
        //    double SCFInspCam3IgnoreCount = Menu.frmSetting.revData.mBendingPre.SCFInspCam3IgnoreCount;
        //    int SCFInspCam3InFind = Menu.frmSetting.revData.mBendingPre.SCFInspCam3InFind;
        //    int SCFInspCam3OutFind = Menu.frmSetting.revData.mBendingPre.SCFInspCam3OutFind;
        //    int SCFInspCam3InPolarity = Menu.frmSetting.revData.mBendingPre.SCFInspCam3InPolarity;
        //    int SCFInspCam3OutPolarity = Menu.frmSetting.revData.mBendingPre.SCFInspCam3OutPolarity;
        //    double SCFInspCam3InThreshold = Menu.frmSetting.revData.mBendingPre.SCFInspCam3InThreshold;
        //    double SCFInspCam3OutThreshold = Menu.frmSetting.revData.mBendingPre.SCFInspCam3OutThreshold;

        //    csVision.sideResult rst = new csVision.sideResult();
        //    Vision[Vision_No.vsBendPre3].SCFAttachInspectionCircle(refPixel3, dInRadius, dOutRadius, dInSearchLength, dOutSearchLength,
        //        SCFInspCam3CalipherCount, SCFInspCam3IgnoreCount, SCFInspCam3InFind, SCFInspCam3OutFind,
        //        SCFInspCam3InPolarity, SCFInspCam3OutPolarity, SCFInspCam3InThreshold, SCFInspCam3OutThreshold, false, false, ref rst, true);  //세로 방향
        //    result[4] = rst.dist;
        //    result[5] = rst.distY;

        //    string sXYresult = "Hole Diff X = " + rst.inoutdiffX.ToString("N3") + ", Y = " + rst.inoutdiffY.ToString("N3");
        //    LogDisp(Vision[Vision_No.vsBendPre3].CFG.Camno, sXYresult);

        //    //========================================================================================================== 20201031 ith add    
        //    // 20201031 오토런 중에 블롭툴 결과 LX는 정상이고 RX가 0이 출력되어 테스트 중

        //    bool use20201031 = true;
        //    if (use20201031)
        //    {
        //        const double TOTAL_LEN_LXRX = 0.5;

        //        if (result[0] == 0)
        //        {
        //            // RX는 정상인 경우였으면.......
        //            if (result[2] > 0 && result[2] < 0.4)
        //            {
        //                // 결과 = 설계치 - 반대쪽 길이
        //                result[0] = Math.Abs(TOTAL_LEN_LXRX - result[2]);
        //                LogDisp(Vision_No.vsBendPre1, "> result[0] 0->" + result[0].ToString("0.000"));
        //            }
        //        }

        //        if (result[2] == 0)
        //        {
        //            // LX는 정상인 경우였으면................
        //            if (result[0] > 0 && result[0] < 0.4)
        //            {
        //                // 결과 = 설계치 - 반대쪽 길이
        //                result[2] = Math.Abs(TOTAL_LEN_LXRX - result[0]);
        //                LogDisp(Vision_No.vsBendPre1, "> result[2] 0->" + result[2].ToString("0.000"));
        //            }
        //        }
        //    }
        //    //========================================================================================================== 20201031 ith add    

        //    double spec = 0;  //spec 받아오는 부분 추가
        //    double tor = 0; //tor 받아오는 부분 추가
        //    bool ret = true;

        //    //20200918 cjm spec, tor 받아오는 부분 추가
        //    tor = Menu.frmSetting.revData.mBendingPre.SCFTolerance;

        //    if (tor > 0)
        //    {
        //        for (int i = 0; i < result.Length; i++)
        //        {
        //            switch (i)
        //            {
        //                case 0:
        //                    spec = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X1_SPEC];
        //                    break;
        //                case 1:
        //                    spec = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y1_SPEC];
        //                    break;
        //                case 2:
        //                    spec = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X1_SPEC];
        //                    break;
        //                case 3:
        //                    spec = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y1_SPEC];
        //                    break;
        //                case 4:
        //                    spec = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X2_SPEC];
        //                    break;
        //                case 5:
        //                    spec = CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_Y2_SPEC];
        //                    break;
        //            }
        //            if (result[i] >= 0)  //제대로 측정된 항목만 처리
        //            {
        //                if (Math.Abs(result[i] - spec) >= tor) ret = false;
        //            }
        //        }
        //    }
        //    if(ret && bhisto && result[0] + result[2] > CONST.RunRecipe.Param[eRecipe.SCF_INSPECTION_X1_SPEC] + tor)
        //    {
        //        //양쪽 크기의 합이 spec + tor을 넘어서고 밝기 차이가 최근 최저값보다 10이상이면 NG
        //        if (result[0] > 0.4)
        //        {
        //            if (Math.Abs(histomeanx2[histocnt] - histomeanx2min) > 10) 
        //            {
        //                ret = false;
        //                Console.WriteLine("HistoFail Histo : " + histomeanx2[histocnt].ToString() + "MIN : " + histomeanx2min);
        //                result[2] = 0;
        //            }
        //        }
        //        if (result[2] > 0.4)
        //        {
        //            if (Math.Abs(histomeanx1[histocnt] - histomeanx1min) > 10)
        //            {
        //                ret = false;
        //                Console.WriteLine("HistoFail Histo : " + histomeanx1[histocnt].ToString() + "MIN : " + histomeanx1min);
        //                result[0] = 0;
        //            }
        //        }
        //    }
        //    if (histocnt < histomeanx1.Length - 1)
        //        histocnt++;
        //    else
        //        histocnt = 0;
        //    if (!bhisto && histocnt > 10) bhisto = true;
            

        //    // 화면에 표시하는 기능 추가 필요함....로그도...
        //    //20200918 cjm 화면에 표시하는 기능 추가, 로그
        //    //20.10.16 lkw
        //    string log = CONST.PanelIDBendPre + "," + CONST.IndexNOBendPre + "," + result[0].ToString("0.000") + "," + result[1].ToString("0.000") + "," + result[2].ToString("0.000") + "," + result[3].ToString("0.000") + "," + result[4].ToString("0.000") + "," + result[5].ToString("0.000");
        //    cLog.Save(LogKind.BDPreSCFInspection, log);
        //    //dgvResult1.Rows.Insert(0, log.Split(','));

        //    #region CHECK LOG .......... 20201101 ith add
        //    if (DateTime.Now.ToString("yyyyMMdd") == "20201101")
        //    {
        //        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "..." + "ViewDGVResult Start...");
        //    }
        //    #endregion
        //    //20.10.06 lkw
        //    ViewDGVResult(dgvResult1, CONST.PanelIDBendPre, CONST.IndexNOBendPre, result);
        //    #region CHECK LOG .......... 20201101 ith add
        //    if (DateTime.Now.ToString("yyyyMMdd") == "20201101")
        //    {
        //        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "..." + "ViewDGVResult End...");
        //    }
        //    #endregion
        //    //CompareDgvSpec(dgvResult1, dgvSpec1);

        //    #region CHECK LOG .......... 20201101 ith add
        //    if (DateTime.Now.ToString("yyyyMMdd") == "20201101")
        //    {
        //        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "..." + "CalcDgvCPK Start...");
        //    }
        //    #endregion
        //    if (false) //.....................................20201101 ith 우선 빼보고
        //    {
        //        CalcDgvCPK(dgvResult1, dgvSpec1); //.............80 ms
        //    }
            
        //    // 20201101 ith edit ........................
        //    //System.Threading.Tasks.Task.Run(() =>
        //    //{
        //    //    CalcDgvCPK(dgvResult1, dgvSpec1);
        //    //});

        //    #region CHECK LOG .......... 20201101 ith add
        //    if (DateTime.Now.ToString("yyyyMMdd") == "20201101")
        //    {
        //        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "..." + "CalcDgvCPK End...");
        //    }
        //    #endregion

        //    #region CHECK LOG .......... 20201101 ith add
        //    if (DateTime.Now.ToString("yyyyMMdd") == "20201101")
        //    {
        //        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "..." + "DV_DataWrite Start...");
        //    }
        //    #endregion
        //    // 20200918 cjm PLC Write
        //    int SCFInspection = CONST.AAM_PLC1.DVAddress.SCFInspectionResult;
        //    IF.DV_DataWrite(SCFInspection, result);
        //    #region CHECK LOG .......... 20201101 ith add
        //    if (DateTime.Now.ToString("yyyyMMdd") == "20201101")
        //    {
        //        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "..." + "DV_DataWrite End...");
        //    }
        //    #endregion

        //    #region CHECK LOG .......... 20201101 ith add
        //    if (DateTime.Now.ToString("yyyyMMdd") == "20201101")
        //    {
        //        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "..." + "DistLogSave Start...");
        //    }
        //    #endregion
        //    // 20201101 ith add task run.............5~10 ms
        //    System.Threading.Tasks.Task.Run(() =>
        //    {
        //        string sDistLog = "";
        //        sDistLog = result[0].ToString("0.000") + "," + result[1].ToString("0.000") + "," + result[2].ToString("0.000") + "," + result[3].ToString("0.000") + "," + result[4].ToString("0.000") + "," + result[5].ToString("0.000");
        //        cLog.DistLogSave(Vision[Vision_No.vsBendPre1].CFG.Name, sDistLog);
        //    });
        //    #region CHECK LOG .......... 20201101 ith add
        //    if (DateTime.Now.ToString("yyyyMMdd") == "20201101")
        //    {
        //        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "..." + "DistLogSave End...");
        //    }
        //    #endregion
            
        //    if (ret) return ePCResult.WAIT;
        //    else return ePCResult.SPEC_OVER;
        //}
        //private ePCResult BPSCFexistence(cs2DAlign.ptXYT Mark1, cs2DAlign.ptXYT Mark2, cs2DAlign.ptXYT Mark3 = default(cs2DAlign.ptXYT), bool bBendPre = false)
        //{
        //    //scf, cof검사를 위한 세타값 계산
        //    CogAnglePointPointTool temp = new CogAnglePointPointTool();
        //    temp.InputImage = Vision[Vision_No.vsBendPre1].cogDS.Image;
        //    double pixelXLength = CONST.RunRecipe.Param[eRecipe.PANEL_MARK_TO_MARK_LENGTH_X] / Vision[Vision_No.vsBendPre1].CFG.ResolutionX;
        //    temp.StartX = Mark1.X;
        //    temp.StartY = Mark1.Y;
        //    temp.EndX = Mark2.X + pixelXLength;
        //    temp.EndY = Mark2.Y;
        //    temp.Run();
        //    double insptheta = temp.Angle;

        //    double SCFOffsetX1 = 0;
        //    double SCFOffsetY1 = 0;
        //    double SCFTH1 = 0;
        //    double SCFOffsetX2 = 0;
        //    double SCFOffsetY2 = 0;
        //    double SCFTH2 = 0;
        //    double SCFOffsetX3 = 0;
        //    double SCFOffsetY3 = 0;
        //    double SCFTH3 = 0;
        //    int TH = 0;
        //    if (bBendPre)
        //    {
        //        //3카메라 다 쓸수는 있지만 실제로는 하나만쓸듯
        //        SCFOffsetX1 = Menu.frmSetting.revData.mBendingPre.SCFExistOffsetX1;
        //        SCFOffsetY1 = Menu.frmSetting.revData.mBendingPre.SCFExistOffsetY1;
        //        SCFTH1 = Menu.frmSetting.revData.mBendingPre.SCFExistTH1;
        //        SCFOffsetX2 = Menu.frmSetting.revData.mBendingPre.SCFExistOffsetX2;
        //        SCFOffsetY2 = Menu.frmSetting.revData.mBendingPre.SCFExistOffsetY2;
        //        SCFTH2 = Menu.frmSetting.revData.mBendingPre.SCFExistTH2;
        //        SCFOffsetX3 = Menu.frmSetting.revData.mBendingPre.SCFExistOffsetX3;
        //        SCFOffsetY3 = Menu.frmSetting.revData.mBendingPre.SCFExistOffsetY3;
        //        SCFTH3 = Menu.frmSetting.revData.mBendingPre.SCFExistTH3;
        //    }
        //    else
        //    {
        //        //SCFOffsetX1 = Menu.frmSetting.revData.mLoadPreInsp.SCFInspOffsetX1;
        //        //SCFOffsetY1 = Menu.frmSetting.revData.mLoadPreInsp.SCFInspOffsetY1;
        //        //SCFTH1 = Menu.frmSetting.revData.mLoadPreInsp.SCFInspTH1;
        //        //SCFOffsetX2 = Menu.frmSetting.revData.mLoadPreInsp.SCFInspOffsetX2;
        //        //SCFOffsetY2 = Menu.frmSetting.revData.mLoadPreInsp.SCFInspOffsetY2;
        //        //SCFTH2 = Menu.frmSetting.revData.mLoadPreInsp.SCFInspTH2;
        //    }

        //    //double COFOffsetX1 = Menu.frmSetting.revData.mLoadPreInsp.COFInspOffsetX1;
        //    //double COFOffsetY1 = Menu.frmSetting.revData.mLoadPreInsp.COFInspOffsetY1;
        //    //double COFTH1 = Menu.frmSetting.revData.mLoadPreInsp.COFInspTH1;
        //    //double COFOffsetX2 = Menu.frmSetting.revData.mLoadPreInsp.COFInspOffsetX2;
        //    //double COFOffsetY2 = Menu.frmSetting.revData.mLoadPreInsp.COFInspOffsetY2;
        //    //double COFTH2 = Menu.frmSetting.revData.mLoadPreInsp.COFInspTH2;

        //    bool SCFInspection1Use = false;
        //    if (Menu.frmSetting.revData.mBendingPre.SCFExistTH1 > 0)
        //        SCFInspection1Use = true;
        //    bool SCFInspection2Use = false;
        //    if (Menu.frmSetting.revData.mBendingPre.SCFExistTH2 > 0)
        //        SCFInspection2Use = true;
        //    bool SCFInspection3Use = false;
        //    if (Menu.frmSetting.revData.mBendingPre.SCFExistTH3 > 0)
        //        SCFInspection3Use = true;


        //    //bool COFInspection1Use = Menu.frmSetting.revData.mLoadPreInsp.COF1InspUse;
        //    //bool COFInspection2Use = Menu.frmSetting.revData.mLoadPreInsp.COF2InspUse;
        //    bool SCFInspection1 = false;
        //    bool SCFInspection2 = false;
        //    bool SCFInspection3 = false;
        //    //bool COFInspection1 = false;
        //    //bool COFInspection2 = false;

        //    if (SCFInspection1Use) SCFInspection1 = (Vision[Vision_No.vsBendPre1].AttachInspection(Mark1.X, Mark1.Y, SCFOffsetX1, SCFOffsetY1, SCFTH1, ref TH, insptheta));
        //    else SCFInspection1 = true;
        //    if (SCFInspection2Use) SCFInspection2 = (Vision[Vision_No.vsBendPre2].AttachInspection(Mark2.X, Mark2.Y, SCFOffsetX2, SCFOffsetY2, SCFTH2, ref TH, insptheta));
        //    else SCFInspection2 = true;
        //    if (SCFInspection3Use) SCFInspection3 = (Vision[Vision_No.vsBendPre3].AttachInspection(Mark3.X, Mark3.Y, SCFOffsetX3, SCFOffsetY3, SCFTH3, ref TH, insptheta));
        //    else SCFInspection3 = true;
        //    //if (COFInspection1Use) COFInspection1 = (Vision[sVisionNo].AttachInspection(Mark1.X, Mark1.Y, COFOffsetX1, COFOffsetY1, COFTH1, ref TH, insptheta));
        //    //else COFInspection1 = true;
        //    //if (COFInspection2Use) COFInspection2 = (Vision[sVisionNo].AttachInspection(Mark2.X, Mark2.Y, COFOffsetX2, COFOffsetY2, COFTH2, ref TH, insptheta));
        //    //else COFInspection2 = true;

        //    if (SCFInspection1 && SCFInspection2 && SCFInspection3)// && COFInspection1 && COFInspection2)
        //    {
        //        //확인되면 로그삭제
        //        LogDisp(Vision_No.vsBendPre1, "SCF Existence Inspection OK");
        //        return ePCResult.WAIT;
        //    }
        //    else
        //    {
        //        LogDisp(Vision_No.vsBendPre1, "SCF Existence Inspection NG");
        //        return ePCResult.CHECK;
        //    }
        //}
        //private void ViewDGVResult(DataGridView dgvResult1, string sid, string sindexno, double[] result)
        //{
        //    string sresult = "";
        //    foreach(var s in result)
        //    {
        //        sresult += s.ToString("0.000") + ",";
        //    }
        //    sresult += sindexno + "," + sid;
        //    if (dgvResult1.InvokeRequired)
        //    {
        //        dgvResult1.Invoke(new MethodInvoker(delegate
        //        {
        //            if (dgvResult1.Rows.Count > 50)
        //            {
        //                dgvResult1.Rows.RemoveAt(dgvResult1.Rows.Count - 1);
        //            }
        //            dgvResult1.Rows.Insert(0, sresult.Split(','));
        //        }));
        //    }
        //    else
        //    {
        //        if (dgvResult1.Rows.Count > 50)
        //        {
        //            dgvResult1.Rows.RemoveAt(dgvResult1.Rows.Count - 1);
        //        }
        //        dgvResult1.Rows.Insert(0, sresult.Split(','));
        //    }
        //}
        
    }


}
