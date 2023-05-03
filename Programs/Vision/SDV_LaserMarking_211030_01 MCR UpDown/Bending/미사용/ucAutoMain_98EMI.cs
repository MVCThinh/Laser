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
        Stopwatch sEMIAttach = new Stopwatch();
        private void EMIAttach()
        {
            #region EMI Attach

            //// ##################yw. timeout 추가.

            //TimeOutCheck2(CONST.AAM_PLC2.BitControl.plcEMIAttach1Req, eTimeOutType.Process);
            //// #########################

            //if (CONST.bPLCReq[CONST.AAM_PLC2.BitControl.plcEMIAttach1Req] && pcResult[CONST.AAM_PLC2.Reply.pcEMIAttachReply] == (int)ePCResult.WAIT)
            //{
            //    if (!visionBackground[CONST.AAM_PLC2.BitControl.plcEMIAttach1Req])
            //    {

            //        visionBackground[CONST.AAM_PLC2.BitControl.plcEMIAttach1Req] = true;
            //        TimeOutCheck2(CONST.AAM_PLC2.BitControl.plcEMIAttach1Req, eTimeOutType.Set);

            //        short visionNo1 = CONST.AAM_PLC2.Vision_No.vsEMIAttach;

            //        ePCResult iPCResult = ePCResult.WAIT;
            //        cs2DAlign.ptAlignResult align = new cs2DAlign.ptAlignResult();

            //        Vision[visionNo1].cogDS.InteractiveGraphics.Clear();

            //        CONST.PanelIDEMIAttach = "";
            //        IF.SendData("PNID,EA," + CONST.PLCDeviceType + CONST.Address.PLC.CELLID7.ToString());  //==> 이거 맞는지 확인 필요함. lkw

            //        bool bLeft = true;
            //        if (CONST.bPLCReq[CONST.AAM_PLC2.BitControl.plcEMICamPosition]) bLeft = false;


            //        sEMIAttach.Reset();
            //        sEMIAttach.Start();

            //        BackgroundWorker wkr = new BackgroundWorker();

            //        wkr.DoWork += delegate (object sender1, DoWorkEventArgs e1)
            //        {
            //            if (CONST.m_bSystemLog)
            //                cLog.Save(LogKind.System, "plcEMIAttachReq on");


            //            cs2DAlign.ptXYT Mark1 = new cs2DAlign.ptXYT();
            //            cs2DAlign.ptXYT Mark2 = new cs2DAlign.ptXYT();



            //            string OKNG = "";
            //            bool b1Mark = false;
            //            bool b2Mark = false;


            //            //마크찾기
            //            PParam param = new PParam();
            //            param.qkind.Enqueue(ePatternKind.Left_1cam);
            //            param.qkind.Enqueue(ePatternKind.Right_1cam);
            //            param.oneCamCapture = true;
            //            FindThread(param, visionNo1, true, out List<cs2DAlign.ptXYT> pt1, out List<bool> bResult1);
            //            SetFindResult(pt1, bResult1, ref b1Mark, ref b2Mark, ref Mark1, ref Mark2);


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
            //            if (!manualMark[visionNo1].MarkInfo[mFPC].selected)
            //            {
            //                if (!b2Mark)
            //                    b2Mark = Vision[visionNo1].RunTempPMAlign(ref Mark2, mFPC);
            //            }
            //            else
            //            {
            //                b2Mark = manualMark[visionNo1].manualMarkSelect(ref Mark2, mFPC, b2Mark);
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
            //                double offset = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.EMIAttach].LCheckOffset1;
            //                if (bLeft) offset = Menu.frmSetting.revData.mLcheck[(short)eCamNO4.EMIAttach].LCheckOffset2;

            //                if (bLeft)
            //                {
            //                    iPCResult = Lcheck((int)eCalPos.EMIAttach1_1, Mark1, Mark2, eRecipe.EMIATTACH_LCHECK_SPEC1, eRecipe.EMIATTACH_LCHECK_TOLERANCE1, offset, ref LDist[3]);
            //                }
            //                else
            //                {
            //                    iPCResult = Lcheck((int)eCalPos.EMIAttach2_1, Mark1, Mark2, eRecipe.EMIATTACH_LCHECK_SPEC1, eRecipe.EMIATTACH_LCHECK_TOLERANCE1, offset, ref LDist[3]);
            //                }
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
            //                            manualMark[visionNo1].MarkInfo[mPanel].patternKind = ePatternKind.Left_1cam;
            //                        }
            //                    }
            //                    if (!b2Mark)
            //                    {
            //                        if (manualMark[visionNo1].MarkInfo[mFPC].Popupcnt < manualPopupCnt)
            //                        {
            //                            manualMark[visionNo1].MarkInfo[mFPC].manualPopup = true;
            //                            manualMark[visionNo1].MarkInfo[mFPC].patternKind = ePatternKind.Right_1cam;
            //                        }
            //                    }
            //                    if (manualMark[visionNo1].MarkInfo[mPanel].manualPopup
            //                        || manualMark[visionNo1].MarkInfo[mFPC].manualPopup)
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

            //                if (bLeft) iPCResult = AlignXYT(visionNo1, (int)eCalPos.EMIAttach1_1, Mark1, Mark2, ref align, false);
            //                else iPCResult = AlignXYT(visionNo1, (int)eCalPos.EMIAttach2_1, Mark1, Mark2, ref align, false);

            //                NotUseXYT(visionNo1, ref align);
            //                ReverseXYT(visionNo1, ref align);
            //            }
            //            if (iPCResult == ePCResult.WAIT)
            //            {
            //                iPCResult = CompareLimit(visionNo1, align);
            //            }

            //            if (iPCResult == ePCResult.WAIT)
            //            {
            //                cs2DAlign.ptXYT alignRYT = new cs2DAlign.ptXYT();
            //                //2020.09.25 lkw
            //                if (bLeft) alignRYT = IF.writeAlignXYTOffset(CONST.AAM_PLC2.OffsetAddress.EMIAttach, align.X, align.Y, align.T, eConvert.EMIAttach1);
            //                else alignRYT = IF.writeAlignXYTOffset(CONST.AAM_PLC2.OffsetAddress.EMIAttach, align.X, align.Y, align.T, eConvert.EMIAttach2);

            //                align.X = alignRYT.X;
            //                align.Y = alignRYT.Y;
            //                align.T = alignRYT.T;

            //                //DV 항목
            //                int iscorePanel = CONST.AAM_PLC2.MatchingScore.EMIReel;
            //                double[] scoreData = { Vision[visionNo1].GetVisionSearchScorePanel(), Vision[visionNo1].GetVisionSearchScoreFPC() };
            //                IF.DataWrite_Score_OneWord(iscorePanel, scoreData);
            //            }

            //            if (iPCResult == ePCResult.WAIT)
            //            {
            //                iPCResult = ePCResult.OK;
            //            }
            //            //결과값 보내기
            //            SetResult(nameof(eCamNO4.EMIAttach), (ePCResult)iPCResult, visionNo1, CONST.AAM_PLC2.Reply.pcEMIAttachReply, align);

            //            if (iPCResult != ePCResult.RETRY && iPCResult != ePCResult.VISION_REPLY_WAIT)
            //            {
            //                if (iPCResult != ePCResult.OK)
            //                {
            //                    Vision[visionNo1].ImageSave(false, Vision[visionNo1].CFG.ImageSaveType, iPCResult.ToString());
            //                }
            //                else
            //                {
            //                    if (Vision[visionNo1].CFG.ImageSave) Vision[visionNo1].ImageSave(true, Vision[visionNo1].CFG.ImageSaveType, iPCResult.ToString());
            //                }
            //                sEMIAttach.Stop();
            //                WriteCycleTime(visionNo1, sEMIAttach.ElapsedMilliseconds);
            //                //로그 저장
            //                LogSavePre(LogKind.EMIAttach, (eCamNO)eCamNO4.EMIAttach, Mark1, Mark2, align, LDist[3], CONST.PanelIDEMIAttach, iPCResult.ToString(), (Convert.ToDouble(sEMIAttach.ElapsedMilliseconds) / 1000));
            //            }
            //        };
            //        wkr.RunWorkerCompleted += delegate (object sender1, RunWorkerCompletedEventArgs e2)
            //        {
            //            if (CONST.m_bSystemLog)
            //                cLog.Save(LogKind.System, "plcEMIAttach PatternSearch End");
            //            WritePanleID(visionNo1, CONST.PanelIDEMIAttach);
            //            visionBackground[CONST.AAM_PLC2.BitControl.plcEMIAttach1Req] = false;
            //            wkr.Dispose();
            //        };
            //        wkr.RunWorkerAsync();
            //    }
            //}
            //else if (!CONST.bPLCReq[CONST.AAM_PLC2.BitControl.plcEMIAttach1Req] && pcResult[CONST.AAM_PLC2.Reply.pcEMIAttachReply] != (int)ePCResult.WAIT)
            //{
            //    // ##################yw. timeout 추가.
            //    TimeOutCheck2(CONST.AAM_PLC2.BitControl.plcEMIAttach1Req, eTimeOutType.UnSet);
            //    // #########################

            //    pcResult[CONST.AAM_PLC2.Reply.pcEMIAttachReply] = (int)ePCResult.WAIT;

            //    ManualMarkInitial(CONST.AAM_PLC2.Vision_No.vsEMIAttach); //수동마크 초기화                                
            //}
            //else if (CONST.bPLCReq[CONST.AAM_PLC2.BitControl.plcEMIAttach1Req]
            //    && pcResult[CONST.AAM_PLC2.Reply.pcEMIAttachReply] == (int)ePCResult.VISION_REPLY_WAIT
            //    && manualMark[CONST.AAM_PLC2.Vision_No.vsEMIAttach].bWorkerVisible)
            //{
            //    CheckManualMarkPopup(CONST.AAM_PLC2.Vision_No.vsEMIAttach);  //수동 마크창 세팅
            //    if (CheckManualMarkDone(CONST.AAM_PLC2.Vision_No.vsEMIAttach)) //수동마크 작업자가 완료했는지..
            //    {
            //        pcResult[CONST.AAM_PLC2.Reply.pcEMIAttachReply] = (int)ePCResult.WAIT;
            //    }
            //    else if (CheckManualMarkBypass(CONST.AAM_PLC2.Vision_No.vsEMIAttach)) //수동마크 bypass인지..
            //    {
            //        ePCResult iPCResult = ePCResult.WORKER_BY_PASS;
            //        SetResult(nameof(eCamNO4.EMIAttach), (ePCResult)iPCResult, CONST.AAM_PLC2.Vision_No.vsEMIAttach, CONST.AAM_PLC2.Reply.pcEMIAttachReply);
            //        if (iPCResult != ePCResult.OK)
            //        {
            //            Vision[CONST.AAM_PLC2.Vision_No.vsEMIAttach].ImageSave(false, Vision[CONST.AAM_PLC2.Vision_No.vsEMIAttach].CFG.ImageSaveType, iPCResult.ToString());
            //        }
            //        sEMIAttach.Stop();
            //        WriteCycleTime(CONST.AAM_PLC2.Vision_No.vsEMIAttach, sEMIAttach.ElapsedMilliseconds);
            //    }
            //}

            #endregion
        }

    }


}
