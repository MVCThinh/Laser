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

namespace Bending
{
    public partial class ucAutoMain
    {
        //Stopwatch sPanelVision = new Stopwatch();
        //Stopwatch sSCFPick = new Stopwatch();
        //bool firstCk = false;
        private void SCF()
        {
            #region SCF Panel Align

            //// ##################yw. timeout 추가.
            //TimeOutCheck2(CONST.AAM_PLC1.BitControl.plcSCFPanelAlignReq, eTimeOutType.Process);
            //// #########################

            //if (CONST.bPLCReq[CONST.AAM_PLC1.BitControl.plcSCFPanelAlignReq] && pcResult[CONST.AAM_PLC1.Reply.pcSCFPanelAlignReply] == (int)ePCResult.WAIT)
            //{
            //    if (!visionBackground[CONST.AAM_PLC1.BitControl.plcSCFPanelAlignReq])
            //    {

            //        visionBackground[CONST.AAM_PLC1.BitControl.plcSCFPanelAlignReq] = true;
            //        TimeOutCheck2(CONST.AAM_PLC1.BitControl.plcSCFPanelAlignReq, eTimeOutType.Set);

            //        short visionNo1 = CONST.AAM_PLC1.Vision_No.vsSCFPanel1;
            //        short visionNo2 = CONST.AAM_PLC1.Vision_No.vsSCFPanel2;
            //        short visionNo3 = CONST.AAM_PLC1.Vision_No.vsSCFPanel3;
            //        ePCResult iPCResult = ePCResult.WAIT;
            //        cs2DAlign.ptAlignResult align = new cs2DAlign.ptAlignResult();

            //        Vision[visionNo1].cogDS.InteractiveGraphics.Clear();
            //        Vision[visionNo2].cogDS.InteractiveGraphics.Clear();

            //        if (Vision[visionNo1].CFG.CalType == eCalType.Cam3Type) Vision[visionNo3].cogDS.InteractiveGraphics.Clear();

            //        sPanelVision.Reset();
            //        sPanelVision.Start();

            //        BackgroundWorker wkr = new BackgroundWorker();

            //        wkr.DoWork += delegate (object sender1, DoWorkEventArgs e1)
            //        {
            //            if (CONST.m_bSystemLog)
            //                cLog.Save(LogKind.System, "plcSCFPanelReq on");


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
            //            if(Vision[visionNo1].CFG.AlignUse) param.qkind.Enqueue(ePatternKind.Panel);

            //            FindThread(param, visionNo1, true, out List<cs2DAlign.ptXYT> pt, out List<bool> bResult);
            //            if (Vision[visionNo1].CFG.AlignUse) SetFindResult(pt, bResult, ref b1Mark, ref b2Mark, ref b3Mark, ref Mark1, ref Mark2, ref Mark3);
            //            else SetFindResult(pt, bResult, ref b1Mark, ref b2Mark, ref Mark1, ref Mark2);

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
            //            //if (Vision[visionNo1].CFG.CalType == eCalType.Cam3Type)
            //            //{
            //            //    if (!b3Mark)
            //            //    {
            //            //        if (!manualMark[visionNo3].MarkInfo[mPanel].selected)
            //            //        {
            //            //            b3Mark = Vision[visionNo3].RunTempPMAlign(ref Mark3, mPanel);
            //            //        }
            //            //        else
            //            //        {
            //            //            b3Mark = manualMark[visionNo3].manualMarkSelect(ref Mark3, mPanel);
            //            //        }
            //            //    }
            //            //}

            //            //여기까지 못찾았으면 일단 마크못찾았다고 판단.
            //            if (!b1Mark || !b2Mark)// || !b3Mark)
            //            {
            //                iPCResult = ePCResult.ERROR_MARK;
            //            }

            //            //길이체크
            //            //20.10.11 lkw L-Check 측정은 무조건 진행...
            //            if (iPCResult == ePCResult.WAIT)
            //            {
            //                double offset = Menu.frmSetting.revData.mLcheck[(short)eCamNO2.SCFPanel1].LCheckOffset1;
            //                iPCResult = Lcheck((int)eCalPos.SCFPanel1, Mark1, Mark2, eRecipe.SCFPANEL_LCHECK_SPEC1, eRecipe.SCFPANEL_LCHECK_TOLERANCE1, offset, ref LDist[0]);
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

            //                    //if (!b3Mark)
            //                    //{
            //                    //    if (manualMark[visionNo3].MarkInfo[mPanel].Popupcnt < manualPopupCnt)
            //                    //    {
            //                    //        manualMark[visionNo3].MarkInfo[mPanel].manualPopup = true;
            //                    //        iPCResult = ePCResult.VISION_REPLY_WAIT;
            //                    //    }
            //                    //}
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
            //                if (Vision[visionNo1].CFG.AlignUse && b3Mark)
            //                    iPCResult = AlignXYT(visionNo1, (int)eCalPos.SCFPanel1, Mark1, Mark2, ref align, false, 0, Mark3);
            //                else
            //                    iPCResult = AlignXYT(visionNo1, (int)eCalPos.SCFPanel1, Mark1, Mark2, ref align, false);

            //                NotUseXYT(visionNo1, ref align);
            //                ReverseXYT(visionNo1, ref align);
            //            }
            //            if (iPCResult == ePCResult.WAIT)
            //            {
            //                iPCResult = CompareLimit(visionNo1, align);
            //            }

            //            if (iPCResult == ePCResult.WAIT)
            //            {
            //                IF.writeAlignXYTOffset(CONST.AAM_PLC1.OffsetAddress.SCFPanel, align.X, align.Y, align.T);
            //            }

            //            if (iPCResult == ePCResult.WAIT)
            //            {
            //                iPCResult = ePCResult.OK;
            //            }
            //            //결과값 보내기
            //            SetResult(nameof(eCamNO2.SCFPanel1), (ePCResult)iPCResult, visionNo1, CONST.AAM_PLC1.Reply.pcSCFPanelAlignReply, align);
                        
            //            if (iPCResult != ePCResult.RETRY && iPCResult != ePCResult.VISION_REPLY_WAIT)
            //            {
            //                sPanelVision.Stop();
            //                //DV 항목
            //                int iscorePanel = CONST.AAM_PLC1.MatchingScore.SCFPanel;
            //                double[] scoreData = { Vision[visionNo1].GetVisionSearchScorePanel(), Vision[visionNo2].GetVisionSearchScorePanel() };
            //                IF.DataWrite_Score_OneWord(iscorePanel, scoreData);
            //                scorelogData = scoreData;
            //            }
            //            CONST.PanelIDSCFPanel = "";
            //            IF.SendData("PNID,SP," + CONST.PLCDeviceType + CONST.Address.PLC.CELLID3.ToString());
            //            if (iPCResult != ePCResult.RETRY && iPCResult != ePCResult.VISION_REPLY_WAIT)
            //            {
            //                if (iPCResult != ePCResult.OK)
            //                {
            //                    Vision[visionNo1].ImageSave(false, Vision[visionNo1].CFG.ImageSaveType, iPCResult.ToString());
            //                    Vision[visionNo2].ImageSave(false, Vision[visionNo2].CFG.ImageSaveType, iPCResult.ToString());
            //                    if (Vision[visionNo1].CFG.AlignUse) Vision[visionNo3].ImageSave(false, Vision[visionNo3].CFG.ImageSaveType, iPCResult.ToString());
            //                }
            //                else
            //                {
            //                    if (Vision[visionNo1].CFG.ImageSave) Vision[visionNo1].ImageSave(true, Vision[visionNo1].CFG.ImageSaveType, iPCResult.ToString());
            //                    if (Vision[visionNo2].CFG.ImageSave) Vision[visionNo2].ImageSave(true, Vision[visionNo2].CFG.ImageSaveType, iPCResult.ToString());
            //                    if (Vision[visionNo3].CFG.ImageSave && Vision[visionNo1].CFG.AlignUse) Vision[visionNo3].ImageSave(true, Vision[visionNo3].CFG.ImageSaveType, iPCResult.ToString());
            //                }
                            
            //                WriteCycleTime(visionNo1, sPanelVision.ElapsedMilliseconds);
            //                //로그 저장
            //                LogSavePre(LogKind.AlignSCFPanel, (eCamNO)eCamNO2.SCFPanel1, Mark1, Mark2, align, LDist[0], CONST.PanelIDSCFPanel, iPCResult.ToString(), (Convert.ToDouble(sPanelVision.ElapsedMilliseconds) / 1000), scorelogData, Mark3);
            //            }
            //        };
            //        wkr.RunWorkerCompleted += delegate (object sender1, RunWorkerCompletedEventArgs e2)
            //        {
            //            if (CONST.m_bSystemLog)
            //                cLog.Save(LogKind.System, "plcSCFPaenl PatternSearch End");
            //            WritePanleID(visionNo1, CONST.PanelIDSCFPanel);
            //            visionBackground[CONST.AAM_PLC1.BitControl.plcSCFPanelAlignReq] = false;
            //            wkr.Dispose();
            //        };
            //        wkr.RunWorkerAsync();
            //    }
            //}
            //else if (!CONST.bPLCReq[CONST.AAM_PLC1.BitControl.plcSCFPanelAlignReq] && pcResult[CONST.AAM_PLC1.Reply.pcSCFPanelAlignReply] != (int)ePCResult.WAIT)
            //{
            //    TimeOutCheck2(CONST.AAM_PLC1.BitControl.plcSCFPanelAlignReq, eTimeOutType.UnSet);
            //    pcResult[CONST.AAM_PLC1.Reply.pcSCFPanelAlignReply] = (int)ePCResult.WAIT;
            //    ManualMarkInitial(CONST.AAM_PLC1.Vision_No.vsSCFPanel1); //수동마크 초기화
            //}
            //else if (CONST.bPLCReq[CONST.AAM_PLC1.BitControl.plcSCFPanelAlignReq]
            //    && pcResult[CONST.AAM_PLC1.Reply.pcSCFPanelAlignReply] == (int)ePCResult.VISION_REPLY_WAIT
            //    && manualMark[CONST.AAM_PLC1.Vision_No.vsSCFPanel1].bWorkerVisible
            //    && manualMark[CONST.AAM_PLC1.Vision_No.vsSCFPanel2].bWorkerVisible
            //    && manualMark[CONST.AAM_PLC1.Vision_No.vsSCFPanel3].bWorkerVisible)
            //{
            //    CheckManualMarkPopup(CONST.AAM_PLC1.Vision_No.vsSCFPanel1);  //수동 마크창 세팅
            //    if (CheckManualMarkDone(CONST.AAM_PLC1.Vision_No.vsSCFPanel1)) //수동마크 작업자가 완료했는지..
            //    {
            //        pcResult[CONST.AAM_PLC1.Reply.pcSCFPanelAlignReply] = (int)ePCResult.WAIT;
            //    }
            //    else if (CheckManualMarkBypass(CONST.AAM_PLC1.Vision_No.vsSCFPanel1)) //수동마크 bypass인지..
            //    {
            //        ePCResult iPCResult = ePCResult.WORKER_BY_PASS;
            //        SetResult(nameof(eCamNO2.SCFPanel1), (ePCResult)iPCResult, CONST.AAM_PLC1.Vision_No.vsSCFPanel1, CONST.AAM_PLC1.Reply.pcSCFPanelAlignReply);
            //        if (iPCResult != ePCResult.OK)
            //        {
            //            Vision[CONST.AAM_PLC1.Vision_No.vsSCFPanel1].ImageSave(false, Vision[CONST.AAM_PLC1.Vision_No.vsSCFPanel1].CFG.ImageSaveType, iPCResult.ToString());
            //            Vision[CONST.AAM_PLC1.Vision_No.vsSCFPanel2].ImageSave(false, Vision[CONST.AAM_PLC1.Vision_No.vsSCFPanel2].CFG.ImageSaveType, iPCResult.ToString());
            //            if (Vision[CONST.AAM_PLC1.Vision_No.vsSCFPanel1].CFG.AlignUse) Vision[CONST.AAM_PLC1.Vision_No.vsSCFPanel3].ImageSave(false, Vision[CONST.AAM_PLC1.Vision_No.vsSCFPanel3].CFG.ImageSaveType, iPCResult.ToString());
            //        }
            //        sPanelVision.Stop();
            //        WriteCycleTime(CONST.AAM_PLC1.Vision_No.vsSCFPanel1, sPanelVision.ElapsedMilliseconds);
            //    }
            //}

            #endregion

            #region SCF Pick up

            //// ##################yw. timeout 추가.


            //TimeOutCheck2(CONST.AAM_PLC1.BitControl.plcSCFPickAlignReq, eTimeOutType.Process);
            //// #########################

            //if (CONST.bPLCReq[CONST.AAM_PLC1.BitControl.plcSCFPickAlignReq] && pcResult[CONST.AAM_PLC1.Reply.pcSCFPickAlignReply] == (int)ePCResult.WAIT)
            //{
            //    if (!visionBackground[CONST.AAM_PLC1.BitControl.plcSCFPickAlignReq])
            //    {

            //        visionBackground[CONST.AAM_PLC1.BitControl.plcSCFPickAlignReq] = true;
            //        TimeOutCheck2(CONST.AAM_PLC1.BitControl.plcSCFPickAlignReq, eTimeOutType.Set);

            //        short visionNo1 = CONST.AAM_PLC1.Vision_No.vsSCFReel1;
            //        short visionNo2 = CONST.AAM_PLC1.Vision_No.vsSCFReel2;
            //        short visionNo3 = CONST.AAM_PLC1.Vision_No.vsSCFReel3;
            //        ePCResult iPCResult = ePCResult.WAIT;
            //        cs2DAlign.ptAlignResult align = new cs2DAlign.ptAlignResult();

            //        Vision[visionNo1].cogDS.InteractiveGraphics.Clear();
            //        Vision[visionNo2].cogDS.InteractiveGraphics.Clear();

            //        if (Vision[visionNo1].CFG.CalType == eCalType.Cam3Type) Vision[visionNo3].cogDS.InteractiveGraphics.Clear();

            //        //20.10.17 lkw 
            //        CONST.PanelIDSCFAttach = "";
            //        IF.SendData("PNID,SR," + CONST.PLCDeviceType + CONST.Address.PLC.CELLID4.ToString());

            //        sSCFPick.Reset();
            //        sSCFPick.Start();

            //        BackgroundWorker wkr = new BackgroundWorker();

            //        wkr.DoWork += delegate (object sender1, DoWorkEventArgs e1)
            //        {
            //            if (CONST.m_bSystemLog)
            //                cLog.Save(LogKind.System, "plcSCFPickReq on");


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
            //            param.LineCreate = true;
            //            param.useGrapDelay2 = true;
            //            if (Vision[visionNo1].CFG.AlignUse) param.qkind.Enqueue(ePatternKind.Panel);
            //            FindThread(param, visionNo1, true, out List<cs2DAlign.ptXYT> pt1, out List<bool> bResult1);
                        
            //            if (Vision[visionNo1].CFG.AlignUse) SetFindResult(pt1, bResult1, ref b1Mark, ref b2Mark, ref b3Mark, ref Mark1, ref Mark2, ref Mark3);
            //            else SetFindResult(pt1, bResult1, ref b1Mark, ref b2Mark, ref Mark1, ref Mark2);

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
            //            if (Vision[visionNo1].CFG.AlignUse)
            //            {
            //                if (!manualMark[visionNo3].MarkInfo[mPanel].selected)
            //                {
            //                    if (!b3Mark)
            //                        b3Mark = Vision[visionNo3].RunTempPMAlign(ref Mark3, mPanel);
            //                }
            //                else
            //                {
            //                    b3Mark = manualMark[visionNo3].manualMarkSelect(ref Mark3, mPanel, b3Mark);
            //                }
            //            }

            //            //여기까지 못찾았으면 일단 마크못찾았다고 판단.
            //            if (!b1Mark || !b2Mark || !b3Mark)
            //            {
            //                iPCResult = ePCResult.ERROR_MARK;
            //            }

            //            //길이체크
            //            //20.10.11 lkw L-Check 측정은 무조건 진행...
            //            if (iPCResult == ePCResult.WAIT)
            //            {
            //                double offset = Menu.frmSetting.revData.mLcheck[(short)eCamNO2.SCFReel1].LCheckOffset1;
            //                iPCResult = Lcheck((int)eCalPos.SCFReel1, Mark1, Mark2, eRecipe.SCFREEL_LCHECK_SPEC1, eRecipe.SCFREEL_LCHECK_TOLERANCE1, offset, ref LDist[2]);
            //                if (iPCResult != ePCResult.WAIT)
            //                {
            //                    b1Mark = false;
            //                    b2Mark = false;
            //                }
            //            }
            //            //Y길이 lcheck
            //            if (iPCResult == ePCResult.WAIT)
            //            {
            //                double offset = Menu.frmSetting.revData.mLcheck[(short)eCamNO2.SCFReel1].LCheckOffset2;
            //                Lcheck((int)eCalPos.SCFReel1, Mark1, Mark3, eRecipe.SCFREEL_LCHECK_SPEC2, eRecipe.SCFREEL_LCHECK_TOLERANCE2, offset, ref LDist[3], true);
            //                //if (iPCResult != ePCResult.WAIT)
            //                //{
            //                //    b1Mark = false;
            //                //    b3Mark = false;
            //                //}
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
            //                    if (!b3Mark)
            //                    {
            //                        if (manualMark[visionNo3].MarkInfo[mPanel].Popupcnt < manualPopupCnt)
            //                        {
            //                            manualMark[visionNo3].MarkInfo[mPanel].manualPopup = true;
            //                            manualMark[visionNo3].MarkInfo[mPanel].patternKind = ePatternKind.Panel;
            //                        }
            //                    }
            //                    if (manualMark[visionNo1].MarkInfo[mPanel].manualPopup
            //                    || manualMark[visionNo2].MarkInfo[mPanel].manualPopup
            //                    || manualMark[visionNo3].MarkInfo[mPanel].manualPopup)
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
            //                if (Vision[visionNo1].CFG.AlignUse) //3cam mark 찾았을 때 //3cam 무조건 사용해야함.
            //                    iPCResult = AlignXYT(visionNo1, (int)eCalPos.SCFReel1, Mark1, Mark2, ref align, true, 0, Mark3);
            //                else //3cam mark 못찾으면 2cam으로 진행
            //                    iPCResult = AlignXYT(visionNo1, (int)eCalPos.SCFReel1, Mark1, Mark2, ref align, true);

            //                NotUseXYT(visionNo1, ref align);
            //                ReverseXYT(visionNo1, ref align);
            //            }
            //            if (iPCResult == ePCResult.WAIT)
            //            {
            //                iPCResult = CompareLimit(visionNo1, align);
            //            }

            //            if (iPCResult == ePCResult.WAIT)
            //            {
            //                IF.writeAlignXYTOffset(CONST.AAM_PLC1.OffsetAddress.SCFPick, align.X, align.Y, align.T);
                            
            //            }

            //            if (iPCResult == ePCResult.WAIT)
            //            {
            //                iPCResult = ePCResult.OK;
            //            }
            //            //결과값 보내기
            //            //20.10.07 lkw 
            //            SetResult(nameof(eCamNO2.SCFReel2), (ePCResult)iPCResult, visionNo3, CONST.AAM_PLC1.Reply.pcSCFPickAlignReply, align);
                        
            //            if (iPCResult != ePCResult.RETRY && iPCResult != ePCResult.VISION_REPLY_WAIT)
            //            {
            //                sSCFPick.Stop();
            //                //DV 항목
            //                int iscorePanel = CONST.AAM_PLC1.MatchingScore.SCFPick;
            //                double[] scoreData = { Vision[visionNo1].GetVisionSearchScorePanel(), Vision[visionNo2].GetVisionSearchScorePanel(), Vision[visionNo3].GetVisionSearchScorePanel() };
            //                IF.DataWrite_Score_OneWord(iscorePanel, scoreData);
            //                scorelogData = scoreData;
            //            }
            //            if (iPCResult != ePCResult.RETRY && iPCResult != ePCResult.VISION_REPLY_WAIT)
            //            {
            //                if (iPCResult != ePCResult.OK)
            //                {
            //                    Vision[visionNo1].ImageSave(false, Vision[visionNo1].CFG.ImageSaveType, iPCResult.ToString());
            //                    Vision[visionNo2].ImageSave(false, Vision[visionNo2].CFG.ImageSaveType, iPCResult.ToString());
            //                    //if (Vision[visionNo1].CFG.AlignUse)
            //                        Vision[visionNo3].ImageSave(false, Vision[visionNo3].CFG.ImageSaveType, iPCResult.ToString());
            //                }
            //                else
            //                {
            //                    if (Vision[visionNo1].CFG.ImageSave) Vision[visionNo1].ImageSave(true, Vision[visionNo1].CFG.ImageSaveType, iPCResult.ToString());
            //                    if (Vision[visionNo2].CFG.ImageSave) Vision[visionNo2].ImageSave(true, Vision[visionNo2].CFG.ImageSaveType, iPCResult.ToString());
            //                    if (Vision[visionNo3].CFG.ImageSave /*&& Vision[visionNo1].CFG.AlignUse*/) Vision[visionNo3].ImageSave(true, Vision[visionNo3].CFG.ImageSaveType, iPCResult.ToString());
            //                }
                            
            //                WriteCycleTime(visionNo3, sSCFPick.ElapsedMilliseconds);
            //                //로그 저장
            //                LogSavePre(LogKind.AlignSCFReelPickUp, (eCamNO)eCamNO2.SCFReel1, Mark1, Mark2, align, LDist[2], CONST.PanelIDSCFAttach, iPCResult.ToString(), (Convert.ToDouble(sSCFPick.ElapsedMilliseconds) / 1000), scorelogData, Mark3, LDist[3]);
            //            }
            //        };
            //        wkr.RunWorkerCompleted += delegate (object sender1, RunWorkerCompletedEventArgs e2)
            //        {
            //            if (CONST.m_bSystemLog)
            //                cLog.Save(LogKind.System, "plcSCFPick PatternSearch End");
            //            WritePanleID(visionNo3, CONST.PanelIDSCFAttach);
            //            visionBackground[CONST.AAM_PLC1.BitControl.plcSCFPickAlignReq] = false;
            //            wkr.Dispose();
            //        };
            //        wkr.RunWorkerAsync();
            //    }
            //}
            //else if (!CONST.bPLCReq[CONST.AAM_PLC1.BitControl.plcSCFPickAlignReq] && pcResult[CONST.AAM_PLC1.Reply.pcSCFPickAlignReply] != (int)ePCResult.WAIT)
            //{
            //    // ##################yw. timeout 추가.
            //    TimeOutCheck2(CONST.AAM_PLC1.BitControl.plcSCFPickAlignReq, eTimeOutType.UnSet);
            //    // #########################

            //    pcResult[CONST.AAM_PLC1.Reply.pcSCFPickAlignReply] = (int)ePCResult.WAIT;
            //    ManualMarkInitial(CONST.AAM_PLC1.Vision_No.vsSCFReel1); //수동마크 초기화

            //    if (Vision[CONST.AAM_PLC1.Vision_No.vsSCFReel1].CFG.CalType == eCalType.Cam3Type) ManualMarkInitial(CONST.AAM_PLC1.Vision_No.vsSCFReel3); //수동마크 초기화
            //}
            //else if (CONST.bPLCReq[CONST.AAM_PLC1.BitControl.plcSCFPickAlignReq]
            //    && pcResult[CONST.AAM_PLC1.Reply.pcSCFPickAlignReply] == (int)ePCResult.VISION_REPLY_WAIT
            //    && manualMark[CONST.AAM_PLC1.Vision_No.vsSCFReel1].bWorkerVisible
            //    && manualMark[CONST.AAM_PLC1.Vision_No.vsSCFReel2].bWorkerVisible
            //    && manualMark[CONST.AAM_PLC1.Vision_No.vsSCFReel3].bWorkerVisible)
            //{
            //    CheckManualMarkPopup(CONST.AAM_PLC1.Vision_No.vsSCFReel1);  //수동 마크창 세팅
            //    if (CheckManualMarkDone(CONST.AAM_PLC1.Vision_No.vsSCFReel1)) //수동마크 작업자가 완료했는지..
            //    {
            //        pcResult[CONST.AAM_PLC1.Reply.pcSCFPickAlignReply] = (int)ePCResult.WAIT;
            //    }
            //    else if (CheckManualMarkBypass(CONST.AAM_PLC1.Vision_No.vsSCFReel1)) //수동마크 bypass인지..
            //    {
            //        ePCResult iPCResult = ePCResult.WORKER_BY_PASS;
            //        SetResult(nameof(eCamNO2.SCFReel1), (ePCResult)iPCResult, CONST.AAM_PLC1.Vision_No.vsSCFReel1, CONST.AAM_PLC1.Reply.pcSCFPickAlignReply);
            //        if (iPCResult != ePCResult.OK)
            //        {
            //            Vision[CONST.AAM_PLC1.Vision_No.vsSCFReel1].ImageSave(false, Vision[CONST.AAM_PLC1.Vision_No.vsSCFReel1].CFG.ImageSaveType, iPCResult.ToString());
            //            Vision[CONST.AAM_PLC1.Vision_No.vsSCFReel2].ImageSave(false, Vision[CONST.AAM_PLC1.Vision_No.vsSCFReel2].CFG.ImageSaveType, iPCResult.ToString());
            //            if (Vision[CONST.AAM_PLC1.Vision_No.vsSCFReel1].CFG.AlignUse) Vision[CONST.AAM_PLC1.Vision_No.vsSCFReel3].ImageSave(false, Vision[CONST.AAM_PLC1.Vision_No.vsSCFReel3].CFG.ImageSaveType, iPCResult.ToString());
            //        }
            //        sSCFPick.Stop();
            //        WriteCycleTime(CONST.AAM_PLC1.Vision_No.vsSCFReel3, sSCFPick.ElapsedMilliseconds);
            //    }
            //}

            #endregion               
        }

    }
}
