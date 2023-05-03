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

namespace Bending
{
    public partial class ucAutoMain
    {      
        private void LoadingPre(short sRequest, short sReply, eCalPos calPos, int motionNo, double Tdegree, bool addAlign, params short[] visionNo)
        {
            // ##################yw. timeout 추가.
            TimeOutCheck2(sRequest, eTimeOutType.Process);
            // #########################
            if (CONST.bPLCReq[sRequest] && pcResult[sReply] == (int)ePCResult.WAIT)
            {
                if (!visionBackground[sRequest])
                {
                    visionBackground[sRequest] = true;
                    TimeOutCheck2(sRequest, eTimeOutType.Set);
                    if (sTime[sRequest] == null) sTime[sRequest] = new Stopwatch();
                    ePCResult iPCResult = ePCResult.WAIT;
                    cs2DAlign.ptAlignResult align = new cs2DAlign.ptAlignResult();
                    Vision[visionNo[0]].PanelID = "";

                    sTime[sRequest].Reset();
                    sTime[sRequest].Start();

                    BackgroundWorker wkr = new BackgroundWorker();

                    wkr.DoWork += delegate (object sender1, DoWorkEventArgs e1)
                    {
                        //if (CONST.m_bSystemLog)
                        //    cLog.Save(LogKind.System, "plcPre1AlignReq on");

                        string AlignNO = "1";
                        if (sRequest == plcRequest.LoadingPre2Align) AlignNO = "2";
                        Vision[visionNo[0]].cogDS.InteractiveGraphics.Clear();
                        Vision[visionNo[1]].cogDS.InteractiveGraphics.Clear();

                        cs2DAlign.ptXYT Mark1 = new cs2DAlign.ptXYT();
                        cs2DAlign.ptXYT Mark2 = new cs2DAlign.ptXYT();
                        bool b1Mark = false;
                        bool b2Mark = false;

                        //마크찾기
                        PParam param = new PParam();
                        param.qkind.Enqueue(ePatternKind.Panel);
                        param.qkind.Enqueue(ePatternKind.Panel);
                        
                        FindThread(param, visionNo[0], !CONST.simulation, out List<cs2DAlign.ptXYT> pt, out List<bool> bResult);
                        SetFindResult(pt, bResult, ref b1Mark, ref b2Mark, ref Mark1, ref Mark2);

                        //메뉴얼마크 또는 임시마크 사용(순서 메뉴얼마크좌표 -> 임시마크)
                        if (!manualMark[visionNo[0]].MarkInfo[mPanel].selected)
                        {
                            if (!b1Mark)
                                b1Mark = Vision[visionNo[0]].RunTempPMAlign(ref Mark1, mPanel);
                        }
                        else
                        {
                            b1Mark = manualMark[visionNo[0]].manualMarkSelect(ref Mark1, mPanel, b1Mark);
                        }
                        
                        if (!manualMark[visionNo[1]].MarkInfo[mPanel].selected)
                        {
                            if (!b2Mark)
                                b2Mark = Vision[visionNo[1]].RunTempPMAlign(ref Mark2, mPanel);
                        }
                        else
                        {
                            b2Mark = manualMark[visionNo[1]].manualMarkSelect(ref Mark2, mPanel, b2Mark);
                        }

                        //20.12.17 lkw DL
                        bDLMarkFind[visionNo[0]] = false;  //log 저장용
                        //여기까지 못찾았으면 일단 마크못찾았다고 판단.
                        if (!b1Mark || !b2Mark)
                        {
                            if (Menu.rsDL.IsReady && Vision[visionNo[0]].CFG.DLUse)
                            {
                                if (!b1Mark && Menu.frmSetting.revData.mDL[visionNo[0]].MarkSearch_Use[0])
                                {
                                    b1Mark = Vision[visionNo[0]].PatternSearch_DL(ref Mark1, ePatternKind.Panel, DL_ConnectionNOMS(visionNo[0], 0));
                                }
                                if (!b2Mark && Menu.frmSetting.revData.mDL[visionNo[1]].MarkSearch_Use[0])
                                {
                                    b2Mark = Vision[visionNo[1]].PatternSearch_DL(ref Mark2, ePatternKind.Panel, DL_ConnectionNOMS(visionNo[1], 0));
                                }
                            }

                            if (!b1Mark || !b2Mark) iPCResult = ePCResult.ERROR_MARK;
                            else bDLMarkFind[visionNo[0]] = true;
                        }

                        //길이체크
                        //20.10.11 lkw L-Check 측정은 무조건 진행...
                        bLCheckError[visionNo[0]] = false;
                        if (iPCResult == ePCResult.WAIT)
                        {
                            double offset = Menu.frmSetting.revData.mLcheck[(short)eCamNO1.LoadingPre1].LCheckOffset1;
                            int LcheckCalNo = (int)eCalPos.LoadingPre1_1;  //
                            iPCResult = Lcheck(visionNo[0], LcheckCalNo, Mark1, Mark2, eRecipe.LOADING_PRE_LCHECK_SPEC1, eRecipe.LOADING_PRE_LCHECK_TOLERANCE1, offset, ref LDist[visionNo[0]]);
                            if (iPCResult != ePCResult.WAIT)
                            {
                                b1Mark = false;
                                b2Mark = false;

                                bLCheckError[visionNo[0]] = true;
                            }
                        }

                        //여기까지 정상이 아니면 마크 못찾았다고 판단
                        if (iPCResult != ePCResult.WAIT)
                        {
                            if (Vision[visionNo[0]].CFG.PatternFailManualIn)
                            {
                                if (!b1Mark)
                                {
                                    if (manualMark[visionNo[0]].MarkInfo[mPanel].Popupcnt < manualPopupCnt)
                                    {
                                        manualMark[visionNo[0]].pcresult = iPCResult.ToString();
                                        manualMark[visionNo[0]].MarkInfo[mPanel].manualPopup = true;
                                        manualMark[visionNo[0]].MarkInfo[mPanel].patternKind = ePatternKind.Panel;
                                    }
                                }
                                if (!b2Mark)
                                {
                                    if (manualMark[visionNo[1]].MarkInfo[mPanel].Popupcnt < manualPopupCnt)
                                    {
                                        manualMark[visionNo[1]].pcresult = iPCResult.ToString();
                                        manualMark[visionNo[1]].MarkInfo[mPanel].manualPopup = true;
                                        manualMark[visionNo[1]].MarkInfo[mPanel].patternKind = ePatternKind.Panel;
                                    }
                                }
                                if (manualMark[visionNo[0]].MarkInfo[mPanel].manualPopup
                                || manualMark[visionNo[1]].MarkInfo[mPanel].manualPopup)
                                    iPCResult = ePCResult.VISION_REPLY_WAIT;
                                else
                                    iPCResult = ePCResult.ERROR_MARK;
                            }
                            else
                            {
                                iPCResult = ePCResult.ERROR_MARK;
                            }
                        }
                        if (iPCResult == ePCResult.WAIT) //정상진행
                        {
                            Vision[visionNo[0]].CFG.CenterAlign = true;
                            iPCResult = AlignXYT(visionNo[0], (int)calPos, Mark1, Mark2, ref align, true, Tdegree);
                            NotUseXYT(visionNo[0], ref align);
                            ReverseXYT(visionNo[0], ref align);
                        }
                        if (iPCResult == ePCResult.WAIT)
                        {
                            iPCResult = CompareLimit(visionNo[0], align);
                        }

                        if (iPCResult == ePCResult.WAIT)
                        {

                            if (CONST.PCNo == 3) Vision[visionNo[0]].SetLightExpCont(ePatternKind.FoamInsp, false);
                            IF.writeAlignXYTOffset(motionNo, align.X, align.Y, align.T, eConvert.notUse, Vision[visionNo[0]].CFG.XYAxisRevers);

                        }

                        if (iPCResult == ePCResult.WAIT)
                        {
                            iPCResult = ePCResult.OK;

                        }
                        //결과값 보내기
                        if (sRequest == plcRequest.LoadingPre1Align) SetResult(Vision[visionNo[0]].CFG.eCamName, iPCResult, visionNo[0], sReply, align);
                        else SetResult(Vision[visionNo[1]].CFG.eCamName, iPCResult, visionNo[1], sReply, align);


                        if (iPCResult != ePCResult.RETRY && iPCResult != ePCResult.VISION_REPLY_WAIT) //완료
                        {
                            sTime[sRequest].Stop();
                            //DV 항목
                            double[] scoreData =  { Vision[visionNo[0]].GetLimitScore((int)ePatternKind.Panel), Vision[visionNo[0]].GetSearchScore((int)ePatternKind.Panel), Vision[visionNo[0]].GetPatternNo((int)ePatternKind.Panel),
                                                    Vision[visionNo[1]].GetLimitScore((int)ePatternKind.Panel), Vision[visionNo[1]].GetSearchScore((int)ePatternKind.Panel), Vision[visionNo[1]].GetPatternNo((int)ePatternKind.Panel) };
                            if (sRequest == plcRequest.LoadingPre1Align) IF.DataWrite_Score_OneWord(Address.MatchingScore.LoadingPre1, scoreData);
                            else IF.DataWrite_Score_OneWord(Address.MatchingScore.LoadingPre2, scoreData);
                        }

                        if (iPCResult != ePCResult.RETRY && iPCResult != ePCResult.VISION_REPLY_WAIT)
                        {
                            if (iPCResult != ePCResult.OK)
                            {
                                Vision[visionNo[0]].ImageSave(false, Vision[visionNo[0]].CFG.ImageSaveType, iPCResult.ToString());
                                Vision[visionNo[1]].ImageSave(false, Vision[visionNo[1]].CFG.ImageSaveType, iPCResult.ToString());
                            }
                            else
                            {
                                if (Vision[visionNo[0]].CFG.ImageSave) Vision[visionNo[0]].ImageSave(true, Vision[visionNo[0]].CFG.ImageSaveType, iPCResult.ToString());
                                if (Vision[visionNo[1]].CFG.ImageSave) Vision[visionNo[1]].ImageSave(true, Vision[visionNo[1]].CFG.ImageSaveType, iPCResult.ToString());
                            }
                            WriteCycleTime(visionNo[0], sTime[sRequest].ElapsedMilliseconds);
                            //로그 저장
                            //201012 cjm LogKind 수정 AlignPre -> AlignLoadPre
                            LogSaveLoadPre(LogKind.AlignLoadPre, visionNo[0], Mark1, Mark2, align, LDist[visionNo[0]], Vision[visionNo[0]].PanelID, iPCResult.ToString(), (Convert.ToDouble(sTime[sRequest].ElapsedMilliseconds) / 1000), 0, bDLMarkFind[visionNo[0]], AlignNO);
                        }
                    };
                    wkr.RunWorkerCompleted += delegate (object sender1, RunWorkerCompletedEventArgs e2)
                    {
                        WritePanelID(visionNo[0], Vision[visionNo[0]].PanelID);
                        visionBackground[sRequest] = false;
                        wkr.Dispose();
                    };
                    wkr.RunWorkerAsync();
                }
            }
            else if (!CONST.bPLCReq[sRequest] && pcResult[sReply] != (int)ePCResult.WAIT)
            {
                // ##################yw. timeout 추가.
                TimeOutCheck2(sRequest, eTimeOutType.UnSet);
                // #########################
                pcResult[sReply] = (int)ePCResult.WAIT;
                ManualMarkInitial(visionNo[0]); //수동마크 초기화
            }
            else if (CONST.bPLCReq[sRequest]
                && pcResult[sReply] == (int)ePCResult.VISION_REPLY_WAIT
                && manualMark[visionNo[0]].bWorkerVisible && manualMark[visionNo[1]].bWorkerVisible)
            {

                CheckManualMarkPopup(visionNo[0]);  //수동 마크창 세팅
                if (CheckManualMarkDone(visionNo[0])) //수동마크 작업자가 완료했는지..
                {
                    pcResult[sReply] = (int)ePCResult.WAIT;
                }
                else if (CheckManualMarkBypass(visionNo[0])) //수동마크 bypass인지..
                {
                    ePCResult iPCResult = ePCResult.WORKER_BY_PASS;
                    SetResult(nameof(eCamNO1.LoadingPre1), (ePCResult)iPCResult, visionNo[0], sReply);
                    if (iPCResult != ePCResult.OK)
                    {
                        Vision[visionNo[0]].ImageSave(false, Vision[visionNo[0]].CFG.ImageSaveType, iPCResult.ToString());
                        Vision[visionNo[1]].ImageSave(false, Vision[visionNo[1]].CFG.ImageSaveType, iPCResult.ToString());
                    }
                    sTime[sRequest].Stop();
                    WriteCycleTime(visionNo[0], sTime[sRequest].ElapsedMilliseconds);
                }
            }
        }
        
    }
}
