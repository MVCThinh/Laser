using rs2DAlign;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Cognex.VisionPro;


namespace Bending
{
    public partial class ucAutoMain
    {
        //Stopwatch[] sLaserAlign = new Stopwatch;
        //Stopwatch[] sLaserCellIDLog = new Stopwatch();
        //Stopwatch sLaserInsp = new Stopwatch();
        private void LaserAlign(short sRequest, short sReply, eCalPos calPos, int motionNo, params short[] visionNo)
        {
            #region Laser Align

            TimeOutCheck2(sRequest, eTimeOutType.Process);

            if (CONST.bPLCReq[sRequest] && pcResult[sReply] == (int)ePCResult.WAIT)
            {
                if (!visionBackground[sRequest])
                {

                    visionBackground[sRequest] = true;
                    TimeOutCheck2(sRequest, eTimeOutType.Set);
                    if (sTime[sRequest] == null) sTime[sRequest] = new Stopwatch();
                    //short visionNo1 = visionNo[0];
                    //short visionNo2 = Vision_No.vsSCFReel2;
                    //short visionNo3 = Vision_No.vsSCFReel3;
                    ePCResult iPCResult = ePCResult.WAIT;
                    cs2DAlign.ptAlignResult align = new cs2DAlign.ptAlignResult();

                    //if (Vision[visionNo1].CFG.CalType == eCalType.Cam3Type) Vision[visionNo3].cogDS.InteractiveGraphics.Clear();

                    //Vision[visionNo[0]].PanelID = "";
                    //IF.SendData("PNID," + visionNo[0].ToString() + "," + CONST.PLCDeviceType + Vision[visionNo[0]].PanelIDAddress.ToString());

                    sTime[sRequest].Reset();
                    sTime[sRequest].Start();

                    BackgroundWorker wkr = new BackgroundWorker();

                    wkr.DoWork += delegate (object sender1, DoWorkEventArgs e1)
                    {
                    
                        Vision[visionNo[0]].cogDS.InteractiveGraphics.Clear();

                        Vision[visionNo[0]].cogDS.StaticGraphics.Clear();



                        bool bcam1mark1 = false;
                        cs2DAlign.ptXYT Mark1 = new cs2DAlign.ptXYT();

                        bool b1Mark = false;

                        double[] scorelogData = null;
                        CogImage8Grey img = null;

                        CONST.eImageSaveKind imgKind = CONST.eImageSaveKind.LaserAlign1;
                        if (sRequest == plcRequest.Laser2Align) imgKind = CONST.eImageSaveKind.LaserAlign2;                        

                        //마크찾기
                        if (Menu.frmSetting.revData.mLaser.refSearch == eRefSearch.Mark)
                        {
                            PParam param = new PParam();
                            param.qkind.Enqueue(ePatternKind.Panel);
                            FindThread(param, visionNo[0], !CONST.simulation, out List<cs2DAlign.ptXYT> pt, out List<bool> bResult, Menu.frmSetting.revData.mLaser.UseImageProcess);
                            SetFindResult(pt, bResult, ref b1Mark, ref Mark1);
                        }
                        else if (Menu.frmSetting.revData.mLaser.refSearch == eRefSearch.Line)
                        {
                            if (!CONST.simulation)
                            {
                                Vision[visionNo[0]].SetLightExpCont(eLineKind.FPCBWidth1);
                                Vision[visionNo[0]].Capture(false);
                                
                            }
                            if (Menu.frmSetting.revData.mLaser.UseImageProcess)
                            {
                                img = Menu.frmRecipe.changeImage(Vision[visionNo[0]].CFG.eCamName, (CogImage8Grey)Vision[visionNo[0]].cogDS.Image);
                            }
                            b1Mark = Vision[visionNo[0]].FindMarkingRef(ref Mark1, true, img);
                        }
                        else //Blob
                        {
                            if (!CONST.simulation)
                            {
                                Vision[visionNo[0]].SetLightExpCont(ePatternKind.Panel);
                                Vision[visionNo[0]].Capture(false);
                            }
                            int iTH = 0;
                            CogRectangle region = Menu.frmRecipe.HistoRegionRead(Vision[visionNo[0]].CFG.eCamName, ref iTH, eHistogram.RefPoint);
                            b1Mark = Vision[visionNo[0]].FindMarkingRefBlob(ref Mark1, iTH, region);
                        }

                        int TH = 0;
                        int ISGrey = 0;
                        CogRectangle Drect = Menu.frmRecipe.HistoRegionRead(Vision[visionNo[0]].CFG.eCamName, ref TH, eHistogram.Detach);
                        bool insp1 = Vision[visionNo[0]].MarkingHistoInspection(Drect, TH, ref ISGrey, true);

                        TH = 0;
                        ISGrey = 0;
                        Drect = Menu.frmRecipe.HistoRegionRead(Vision[visionNo[0]].CFG.eCamName, ref TH, eHistogram.MCRPre);
                        bool insp2 = Vision[visionNo[0]].MarkingHistoInspection(Drect, TH, ref ISGrey, false);


                        if (iPCResult == ePCResult.WAIT)
                        {
                            if (!insp1 || !insp2)
                            {
                                iPCResult = ePCResult.CHECK;
                                if (!insp1) LogDisp(visionNo[0], "Detach Fail!");
                                if (!insp2) LogDisp(visionNo[0], "MCR Pre Check Fail!");
                            }
                            else
                            {
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



                                //20.12.17 lkw Mark 찾는 부분 잘 이해 못하겠음?? 카메라 1개 or 2개
                                //이후 DL 추가.....


                                //여기까지 못찾았으면 일단 마크못찾았다고 판단.
                                if (!b1Mark)
                                {
                                    iPCResult = ePCResult.ERROR_MARK;
                                }
                            }
                        }
                        //길이체크
                        //20.10.11 lkw L-Check 측정은 무조건 진행...
                        //if (iPCResult == ePCResult.WAIT)
                        //{
                        //    double offset = Menu.frmSetting.revData.mLcheck[visionNo[0]].LCheckOffset1;
                        //    iPCResult = Lcheck((int)calPos, Mark1, Mark2, eRecipe.FOAM_REEL_LCHECK_SPEC1, eRecipe.FOAM_REEL_LCHECK_TOLERANCE1, offset, ref LDist[visionNo[0]]);
                        //    if (iPCResult != ePCResult.WAIT)
                        //    {
                        //        b1Mark = false;
                        //        b2Mark = false;
                        //    }
                        //}

                        cs2DAlign.ptXY diff = new cs2DAlign.ptXY();
                        if (iPCResult == ePCResult.WAIT)
                        {
                            string str = "";
                            
                            iPCResult = LaserAlignPosCheck(Mark1, visionNo[0], (int)calPos, ref str, ref diff);
                            if (iPCResult != ePCResult.WAIT)
                            {
                                b1Mark = false;
                                bLCheckError[visionNo[0]] = true;
                                LogDisp(visionNo[0], str);
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

                                if (manualMark[visionNo[0]].MarkInfo[mPanel].manualPopup)
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
                            //확인필요 1cam 1mark align 어떻게 할건지.. bcam1mark1로 1mark, 2mark 구분..
                            //mark ref 순서확인

                            //2021.02.08 lkw
                            //iPCResult = LaserAlignXY(visionNo[0], (int)calPos, Mark1, ref align);
                            iPCResult = AlignXYT(visionNo[0], (int)calPos, Mark1, Mark1, ref align);  //확인필요함.....lkw (레이져 헤드가 이동하는것으로...)

                            NotUseXYT(visionNo[0], ref align);
                            ReverseXYT(visionNo[0], ref align);
                        }

                        if (iPCResult == ePCResult.WAIT)
                        {
                            //iPCResult = CompareMarkDegree(visionNo[0], Mark1);
                        }


                        if (iPCResult == ePCResult.WAIT)
                        {
                            iPCResult = CompareLimit(visionNo[0], align);
                        }

                        if (iPCResult == ePCResult.WAIT)
                        {
                            //align.X = 0; align.Y = 0; align.T = 0; 
                            IF.writeAlignXYTOffset(motionNo, align.X, align.Y, align.T);

                            //정상인 경우 박리 검사 추가함.
                            //int ResultTH1 = 0;
                            //int attachResultTH = 0;
                            //double LimitTH1 = 0;
                            //bool Result1 = LPDetachInsp(visionNo[0], Mark1, eDetachInspPosition.Laser, ref ResultTH1, ref attachResultTH, ref LimitTH1);

                            //if (!Result1)
                            //{
                            //    iPCResult = ePCResult.CHECK;
                            //    LogDisp(visionNo[0], "Detach NG");
                            //}
                        }

                        if (iPCResult == ePCResult.WAIT)
                        {
                            iPCResult = ePCResult.OK;
                        }

                        if (iPCResult != ePCResult.RETRY && iPCResult != ePCResult.VISION_REPLY_WAIT)
                        {
                            sTime[sRequest].Stop();
                            ////DV 항목
                            //if (!bcam1mark1)
                            //{
                            //    double[] scoreData = { Vision[visionNo[0]].GetLimitScore((int)ePatternKind.Panel),Vision[visionNo[0]].GetSearchScore((int)ePatternKind.Panel),Vision[visionNo[0]].GetPatternNo((int)ePatternKind.Panel),
                            //                           Vision[visionNo[0]].GetLimitScore((int)ePatternKind.Right_1cam),Vision[visionNo[0]].GetSearchScore((int)ePatternKind.Right_1cam),Vision[visionNo[0]].GetPatternNo((int)ePatternKind.Right_1cam) };
                            //    IF.DataWrite_Score_OneWord(Vision[visionNo[0]].ReportAddress.MatchingScore, scoreData);
                            //    scorelogData = scoreData;
                            //}
                            //else
                            //{
                            //    double[] scoreData = { Vision[visionNo[0]].GetLimitScore((int)ePatternKind.Left_1cam), Vision[visionNo[0]].GetSearchScore((int)ePatternKind.Left_1cam), Vision[visionNo[0]].GetPatternNo((int)ePatternKind.Left_1cam) };
                            //    IF.DataWrite_Score_OneWord(Vision[visionNo[0]].ReportAddress.MatchingScore, scoreData);
                            //    scorelogData = scoreData;
                            //}
                        }
                        //결과값 보내기
                        SetResult(Vision[visionNo[0]].CFG.eCamName, iPCResult, visionNo[0], sReply, align);


                        Vision[visionNo[0]].PanelID = "";
                        IF.SendData("PNID," + visionNo[0].ToString() + "," + CONST.PLCDeviceType + Vision[visionNo[0]].PanelIDAddress.ToString());


                        if (iPCResult != ePCResult.RETRY && iPCResult != ePCResult.VISION_REPLY_WAIT)
                        {
                            //DV 항목
                            if (!bcam1mark1)
                            {
                                double[] scoreData = { Vision[visionNo[0]].GetLimitScore((int)ePatternKind.Panel),Vision[visionNo[0]].GetSearchScore((int)ePatternKind.Panel),Vision[visionNo[0]].GetPatternNo((int)ePatternKind.Panel),
                                                       Vision[visionNo[0]].GetLimitScore((int)ePatternKind.Right_1cam),Vision[visionNo[0]].GetSearchScore((int)ePatternKind.Right_1cam),Vision[visionNo[0]].GetPatternNo((int)ePatternKind.Right_1cam) };
                                IF.DataWrite_Score_OneWord(Vision[visionNo[0]].ReportAddress.MatchingScore, scoreData);
                                scorelogData = scoreData;
                            }
                            else
                            {
                                double[] scoreData = { Vision[visionNo[0]].GetLimitScore((int)ePatternKind.Left_1cam), Vision[visionNo[0]].GetSearchScore((int)ePatternKind.Left_1cam), Vision[visionNo[0]].GetPatternNo((int)ePatternKind.Left_1cam) };
                                IF.DataWrite_Score_OneWord(Vision[visionNo[0]].ReportAddress.MatchingScore, scoreData);
                                scorelogData = scoreData;
                            }



                            if (iPCResult != ePCResult.OK)
                            {
                                Vision[visionNo[0]].ImageSave(false, Vision[visionNo[0]].CFG.ImageSaveType, iPCResult.ToString(), "", imgKind);
                                //Vision[visionNo2].ImageSave(false, Vision[visionNo2].CFG.ImageSaveType, iPCResult.ToString());
                            }
                            else
                            {
                                if (Vision[visionNo[0]].CFG.ImageSave) Vision[visionNo[0]].ImageSave(true, Vision[visionNo[0]].CFG.ImageSaveType, iPCResult.ToString(), "", imgKind);
                                //if (Vision[visionNo2].CFG.ImageSave) Vision[visionNo2].ImageSave(true, Vision[visionNo2].CFG.ImageSaveType, iPCResult.ToString());
                            }

                            WriteCycleTime(visionNo[0], sTime[sRequest].ElapsedMilliseconds);
                            //로그 저장 //확인필요
                            //logSavePre(LogKind.AlignSCFReel, visionNo[0], Mark1, Mark2, align, LDist[visionNo[0]], "NoPanelID", iPCResult.ToString(), (Convert.ToDouble(sLaserAlign.ElapsedMilliseconds) / 1000), scorelogData, Mark3);

                            string apcOffsetX = CONST.RunRecipe.Param[eRecipe.LASER_MARKING_OFFSET_X1].ToString();
                            string apcOffsetY = CONST.RunRecipe.Param[eRecipe.LASER_MARKING_OFFSET_Y1].ToString();

                            if (sRequest == plcRequest.Laser2Align)
                            {
                                apcOffsetX = CONST.RunRecipe.Param[eRecipe.LASER_MARKING_OFFSET_X2].ToString();
                                apcOffsetY = CONST.RunRecipe.Param[eRecipe.LASER_MARKING_OFFSET_Y2].ToString();
                            }
                            //210118 cjm 레이저 로그
                            //baselog = "Date,No,CellID,OKNG,MarkX1,MarkY1,AlignX,AlignY,VisionOffsetX,VisionOffseY,APCOffsetX,APCOffsetY\r\n";
                            string logStr = Vision[visionNo[0]].CFG.eCamName + "," +
                                            Vision[visionNo[0]].PanelID + "," +
                                            iPCResult.ToString() + "," +
                                            Mark1.X + "," + Mark1.Y + "," + align.X + "," + align.Y + "," +
                                            Menu.frmSetting.revData.mOffset[visionNo[0]].AlignOffsetXYT.X + "," +
                                            Menu.frmSetting.revData.mOffset[visionNo[0]].AlignOffsetXYT.Y + "," +
                                            apcOffsetX + "," +
                                            apcOffsetY + "," +
                                            diff.X + "," +
                                            diff.Y;



                            cLog.Save(LogKind.LaserAlign, logStr);
                        }
                    };
                    wkr.RunWorkerCompleted += delegate (object sender1, RunWorkerCompletedEventArgs e2)
                    {
                        if (CONST.m_bSystemLog)
                            cLog.Save(LogKind.System, "plcSCFReel PatternSearch End");
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
                && manualMark[visionNo[0]].bWorkerVisible)
            //&& manualMark[Vision_No.vsSCFReel2].bWorkerVisible)
            {
                if (sRequest == plcRequest.Laser1Align)
                    CheckManualMarkPopup(visionNo[0], CONST.eImageSaveKind.LaserAlign1);  //수동 마크창 세팅
                else
                    CheckManualMarkPopup(visionNo[0], CONST.eImageSaveKind.LaserAlign2);  //수동 마크창 세팅

                if (CheckManualMarkDone(visionNo[0])) //수동마크 작업자가 완료했는지..
                {
                    pcResult[sReply] = (int)ePCResult.WAIT;
                }
                else if (CheckManualMarkBypass(visionNo[0])) //수동마크 bypass인지..
                {
                    ePCResult iPCResult = ePCResult.WORKER_BY_PASS;
                    SetResult(Vision[visionNo[0]].CFG.eCamName, iPCResult, visionNo[0], sReply);
                    if (iPCResult != ePCResult.OK)
                    {
                        if (sRequest == plcRequest.Laser1Align)
                            Vision[visionNo[0]].ImageSave(false, Vision[visionNo[0]].CFG.ImageSaveType, iPCResult.ToString(), "", CONST.eImageSaveKind.LaserAlign1);
                        else
                            Vision[visionNo[0]].ImageSave(false, Vision[visionNo[0]].CFG.ImageSaveType, iPCResult.ToString(), "", CONST.eImageSaveKind.LaserAlign2);
                        //Vision[Vision_No.vsSCFReel2].ImageSave(false, Vision[Vision_No.vsSCFReel2].CFG.ImageSaveType, iPCResult.ToString());
                    }
                    sTime[sRequest].Stop();
                    WriteCycleTime(visionNo[0], sTime[sRequest].ElapsedMilliseconds);
                }
            }
            #endregion           
        }
        // 210119 cjm 레이저 로그 
        private void LaserCellIDLog(short sRequest, short sReply, params short[] visionNo)
        {
            #region Laser Log Write

            TimeOutCheck2(sRequest, eTimeOutType.Process);

            if (CONST.bPLCReq[sRequest] && pcResult[sReply] == (int)ePCResult.WAIT)
            {
                if (!visionBackground[sRequest])
                {
                    visionBackground[sRequest] = true;
                    TimeOutCheck2(sRequest, eTimeOutType.Set);
                    if (sTime[sRequest] == null) sTime[sRequest] = new Stopwatch();
                    ePCResult iPCResult = ePCResult.WAIT;

                    sTime[sRequest].Reset();
                    sTime[sRequest].Start();

                    BackgroundWorker wkr = new BackgroundWorker();

                    wkr.DoWork += delegate (object sender1, DoWorkEventArgs e1)
                    {
                        if (CONST.m_bSystemLog)
                            cLog.Save(LogKind.System, "plc Laser Cell ID Log req on");

                        //panelid는 align할때 갖고있음
                        //Vision[0].PanelID = "";
                        Vision[visionNo[0]].LaserSendCellID = "  ";
                        Vision[visionNo[0]].MCRCellID = "  ";

                        //IF.SendData("PNID," + visionNo[0].ToString() + "," + CONST.PLCDeviceType + Vision[0].PanelIDAddress.ToString());
                        // Adress 1,2 
                        if (Vision[0].CFG.eCamName == nameof(eCamNO.Laser1))
                        {
                            Vision[0].LaserSendCellIDAddress = 10900;
                            Vision[0].MCRCellIDAddress = 11000;
                        }
                        else
                        {
                            Vision[0].LaserSendCellIDAddress = 11100;
                            Vision[0].MCRCellIDAddress = 11200;
                        }

                        IF.SendData("LaserSendCellID," + visionNo[0].ToString() + "," + CONST.PLCDeviceType + Vision[visionNo[0]].LaserSendCellIDAddress.ToString());
                        IF.SendData("MCRCellID," + visionNo[0].ToString() + "," + CONST.PLCDeviceType + Vision[visionNo[0]].MCRCellIDAddress.ToString());
                        Stopwatch timer = new Stopwatch();
                        timer.Start();

                        cLog.Save(LogKind.Laser, Vision[visionNo[0]].PanelID.Trim() + "," + Vision[visionNo[0]].LaserSendCellID.Trim() + "," + Vision[visionNo[0]].MCRCellID.Trim());
                       // //while(true)
                       // //{
                       // if (Vision[0].PanelID != "" && Vision[0].LaserSendCellID != "" && Vision[0].MCRCellID != "")
                       //     {
                       //         cLog.Save(LogKind.Laser, Vision[0].PanelID + "," + Vision[0].LaserSendCellID + "," + Vision[0].MCRCellID);
                       //         //break;
                       //     }
                       //     //else if(timer.ElapsedMilliseconds > 3000) //timeout
                       //     //{
                       //     //    timer.Stop();
                       //     //    iPCResult = ePCResult.BY_PASS;
                       //     //    break;
                       //     //}
                       //     //Thread.Sleep(1);
                       //// }
                        
                        //IF.SendData("PNID," + 0.ToString() + "," + CONST.PLCDeviceType + Vision[0].PanelIDAddress.ToString());
                        //IF.SendData("LaserSendCellID," + 0.ToString() + "," + CONST.PLCDeviceType + Vision[0].LaserSendCellIDAddress.ToString());
                        //IF.SendData("MCRCellID," + 0.ToString() + "," + CONST.PLCDeviceType + Vision[0].MCRCellIDAddress.ToString());
                        //cLog.Save(LogKind.Laser, Vision[0].PanelID + "," + Vision[0].LaserSendCellID + "," + Vision[0].MCRCellID);
                        if (iPCResult == ePCResult.WAIT)
                        {
                            iPCResult = ePCResult.OK;
                        }
                        //결과값 보내기
                        pcResult[sReply] = (int)iPCResult;

                    };
                    wkr.RunWorkerCompleted += delegate (object sender1, RunWorkerCompletedEventArgs e2)
                    {
                        if (CONST.m_bSystemLog)
                            cLog.Save(LogKind.System, "plc Laser Cell ID Log End");
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
            }
            #endregion           
        }

        private void LaserPositionInsp(short sRequest, short sReply, eCalPos calPos, short sCompareBit, params short[] visionNo)
        {
            #region Laser Position Insp

            TimeOutCheck2(sRequest, eTimeOutType.Process);

            if (CONST.bPLCReq[sRequest] && pcResult[sReply] == (int)ePCResult.WAIT)
            {
                if (!visionBackground[sRequest])
                {
                    visionBackground[sRequest] = true;
                    TimeOutCheck2(sRequest, eTimeOutType.Set);
                    if (sTime[sRequest] == null) sTime[sRequest] = new Stopwatch();
                    ePCResult iPCResult = ePCResult.WAIT;
                    cs2DAlign.ptAlignResult align = new cs2DAlign.ptAlignResult();

                    CONST.eImageSaveKind imgKind = CONST.eImageSaveKind.LaserInspection1;
                    if (sRequest == plcRequest.Laser2Inspection) imgKind = CONST.eImageSaveKind.LaserInspection2;

                    pcResult[sCompareBit] = (int)ePCResult.WAIT;

                    sTime[sRequest].Reset();
                    sTime[sRequest].Start();

                    BackgroundWorker wkr = new BackgroundWorker();

                    wkr.DoWork += delegate (object sender1, DoWorkEventArgs e1)
                    {

                        Vision[visionNo[0]].cogDS.InteractiveGraphics.Clear();
                        Vision[visionNo[0]].cogDS.StaticGraphics.Clear();

                        ////Vision[visionNo2].cogDS.InteractiveGraphics.Clear();
                        if (Vision[visionNo[0]].PanelID == "")
                        {
                            IF.SendData("PNID," + visionNo[0].ToString() + "," + CONST.PLCDeviceType + Vision[visionNo[0]].PanelIDAddress.ToString());
                        }

                        Vision[visionNo[0]].LaserSendCellID = "  "; 
                        
                        if (Vision[visionNo[0]].CFG.eCamName == nameof(eCamNO.Laser1))
                        {
                            Vision[visionNo[0]].LaserSendCellIDAddress = 10900;                        
                        }
                        else
                        {
                            Vision[visionNo[0]].LaserSendCellIDAddress = 11100;                            
                        }

                        //IF.SendData("LaserSendCellID," + visionNo[0].ToString() + "," + CONST.PLCDeviceType + Vision[visionNo[0]].LaserSendCellIDAddress.ToString());
                        

                        cs2DAlign.ptXYT Mark1 = new cs2DAlign.ptXYT();
                        
                        cs2DAlign.ptXY distL = new cs2DAlign.ptXY();
                        cs2DAlign.ptXY distR = new cs2DAlign.ptXY();
                        cs2DAlign.ptXXYY dist = new cs2DAlign.ptXXYY();
                        dist = default(cs2DAlign.ptXXYY);
                        string apnCode = "";
                        bool b1Mark = false;
                       
                        //마크찾기
                        PParam param = new PParam();
                        CogImage8Grey img = null;
                        
                        if (!manualMark[visionNo[0]].MarkInfo[mPanel].selected)
                        {
                            //param.qkind.Enqueue(ePatternKind.Panel);   //모양 다를 경우 변경해야함.
                            //                                           //param.qkind.Enqueue(ePatternKind.Right_1cam);
                            //FindThread(param, visionNo[0], true, out List<cs2DAlign.ptXYT> pt1, out List<bool> bResult1);
                            //SetFindResult(pt1, bResult1, ref b1Mark, ref Mark1);

                            if (Menu.frmSetting.revData.mLaser.refSearch == eRefSearch.Mark)
                            {
                                //PParam param = new PParam();
                                param.qkind.Enqueue(ePatternKind.Panel);
                                FindThread(param, visionNo[0], !CONST.simulation, out List<cs2DAlign.ptXYT> pt1, out List<bool> bResult1, Menu.frmSetting.revData.mLaser.UseImageProcess);
                                SetFindResult(pt1, bResult1, ref b1Mark, ref Mark1);
                            }
                            else if (Menu.frmSetting.revData.mLaser.refSearch == eRefSearch.Line)
                            {
                                if (!CONST.simulation)
                                {
                                    Vision[visionNo[0]].SetLightExpCont(eLineKind.FPCBWidth1);
                                    Vision[visionNo[0]].Capture(false);
                                    
                                }
                                if (Menu.frmSetting.revData.mLaser.UseImageProcess)
                                {
                                    img = Menu.frmRecipe.changeImage(Vision[visionNo[0]].CFG.eCamName, (CogImage8Grey)Vision[visionNo[0]].cogDS.Image);
                                }
                                b1Mark = Vision[visionNo[0]].FindMarkingRef(ref Mark1, true, img);
                            }
                            else //Blob
                            {
                                if (!CONST.simulation)
                                {
                                    Vision[visionNo[0]].SetLightExpCont(ePatternKind.Panel);
                                    Vision[visionNo[0]].Capture(false);
                                }
                                int iTH = 0;
                                CogRectangle region = Menu.frmRecipe.HistoRegionRead(Vision[visionNo[0]].CFG.eCamName, ref iTH, eHistogram.RefPoint);
                                b1Mark = Vision[visionNo[0]].FindMarkingRefBlob(ref Mark1, iTH, region);
                            }

                        }

                        //메뉴얼마크 또는 임시마크 사용(순서 메뉴얼마크좌표 -> 임시마크)
                        if (!manualMark[visionNo[0]].MarkInfo[mPanel].selected)
                        {
                            //if (!b1Mark)
                            //    b1Mark = Vision[visionNo[0]].RunTempPMAlign(ref Mark1, mPanel);
                        }
                        else
                        {
                            b1Mark = manualMark[visionNo[0]].manualMarkSelect(ref Mark1, mPanel, b1Mark);
                            manualMark[visionNo[0]].MarkInfo[mPanel].selected = true;
                        }


                        cs2DAlign.ptXY diff = new cs2DAlign.ptXY();

                        if (iPCResult == ePCResult.WAIT)
                        {
                            string str = "";
                            iPCResult = LaserAlignPosCheck(Mark1, visionNo[0], (int)calPos, ref str, ref diff);
                            if (iPCResult != ePCResult.WAIT)
                            {
                                b1Mark = false;
                                bLCheckError[visionNo[0]] = true;
                                LogDisp(visionNo[0], str);
                            }
                        }


                        //여기까지 못찾았으면 일단 마크못찾았다고 판단.
                        if (!b1Mark)
                        {
                            iPCResult = ePCResult.ERROR_MARK;
                        }

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
                                
                                if (manualMark[visionNo[0]].MarkInfo[mPanel].manualPopup
                                || manualMark[visionNo[0]].MarkInfo[mLeft].manualPopup || manualMark[visionNo[0]].MarkInfo[mRight].manualPopup)
                                    iPCResult = ePCResult.VISION_REPLY_WAIT;
                                else
                                    iPCResult = ePCResult.ERROR_MARK;
                            }
                            else
                            {
                                iPCResult = ePCResult.ERROR_MARK;
                            }
                        }


                        if (b1Mark)
                        {
                            // Matrix Read

                            cs2DAlign.ptXY[] codePoint = new cs2DAlign.ptXY[4];
                            int iTH = 0;
                            CogRectangle rect = Menu.frmRecipe.HistoRegionRead(Vision[visionNo[0]].CFG.eCamName, ref iTH, eHistogram.MCRRegion);
                            if (Vision[visionNo[0]].readID(ref apnCode, ref codePoint, Menu.frmSetting.revData.mLaser.MCRSearchKind, rect))
                            {
                                cs2DAlign.ptXY resol = new cs2DAlign.ptXY();
                                cs2DAlign.ptXY pixelCnt = new cs2DAlign.ptXY();
                                Menu.rsAlign.getResolution((int)calPos, ref resol, ref pixelCnt);
                                //if (sRequest == plcRequest.Laser1Inspection)
                                //{
                                //    IF.writeAPNCode(CONST.Address.PC.APNCode1, apnCode);
                                //}
                                //else
                                //{
                                //    IF.writeAPNCode(CONST.Address.PC.APNCode2, apnCode);
                                //}

                                cs2DAlign.ptXXYY dist1 = new cs2DAlign.ptXXYY();
                                cs2DAlign.ptXXYY dist2 = new cs2DAlign.ptXXYY();
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
                                dist1 = Menu.frmAutoMain.MarktoCodeDist(visionNo[0], (int)calPos, Mark1, codePoint[ileft]);
                                dist2 = Menu.frmAutoMain.MarktoCodeDist(visionNo[0], (int)calPos, Mark1, codePoint[iright]);

                                if (Menu.frmSetting.revData.mLaser.inspKind == eInspKind.Mark)
                                {
                                    distL.X = dist1.X2;
                                    distL.Y = dist1.Y2;
                                    distR.X = dist2.X2;
                                    distR.Y = dist2.Y2;
                                }
                                else
                                {
                                    distL.X = dist1.X1;
                                    distL.Y = dist1.Y1;
                                    distR.X = dist2.X1;
                                    distR.Y = dist2.Y1;
                                }
                                //distL.X = System.Math.Abs((codePoint[1].X - Mark1.X) * resol.X);
                                //distL.Y = System.Math.Abs((codePoint[1].Y - Mark1.Y) * resol.Y);

                                //distR.X = System.Math.Abs((codePoint[3].X - Mark1.X) * resol.X);
                                //distR.Y = System.Math.Abs((codePoint[3].Y - Mark1.Y) * resol.Y);
                            }
                            else
                            {
                                iPCResult = ePCResult.CHECK;
                                LogDisp(visionNo[0], "MCR Reading Fail!");
                            }
                        }

                        sHistory history = new sHistory();
                        history = default(sHistory);
                        history.LX = System.Math.Abs(distL.X);
                        history.LY = System.Math.Abs(distL.Y);
                        history.RX = System.Math.Abs(distR.X);
                        history.RY = System.Math.Abs(distR.Y);
                        history.PanelID = Vision[visionNo[0]].PanelID.Trim();
                        history.ApnCode = apnCode;

                        SetLaserInspectionSpec(out sSpec specX, out sSpec specY);

                        if (Vision[visionNo[0]].CFG.eCamName == nameof(eCamNO.Laser1))
                        {
                            history.camName = nameof(eCamNO.Laser1);
                            history.ToolNo = 1;
                            setResultHistory(history, 0, false, true);
                        }
                        else
                        {
                            history.camName = nameof(eCamNO.Laser2);
                            history.ToolNo = 2;
                            setResultHistory(history, 1, false, true);
                        }

                        dist.X1 = distL.X;
                        dist.Y1 = distL.Y;
                        dist.X2 = distR.X;
                        dist.Y2 = distR.Y;

                        cs2DAlign.ptXXYY calcDist = new cs2DAlign.ptXXYY();
                        calcDist.X1 = System.Math.Abs(dist.X1);
                        calcDist.Y1 = System.Math.Abs(dist.Y1);
                        calcDist.X2 = System.Math.Abs(dist.X2);
                        calcDist.Y2 = System.Math.Abs(dist.Y2);

                        CogLabeResultlDisplay(calcDist, visionNo[0], specX, specY, true);

                        CONST.DispChart.dist[visionNo[0]] = calcDist;
                        CONST.DispChart.bDrawPoint[visionNo[0]] = true;

                        if (iPCResult == ePCResult.WAIT) //정상진행
                        {
                            if (System.Math.Abs(calcDist.X1 - specX.Middle1) > specX.Spec || System.Math.Abs(calcDist.Y1 - specY.Middle1) > specY.Spec) iPCResult = ePCResult.SPEC_OVER;
                            else iPCResult = ePCResult.WAIT;                          
                        }

                        string sDistLog = "";
                        sDistLog = history.ToolNo + "," + calcDist.X1.ToString("0.000") + "," + calcDist.Y1.ToString("0.000") + "," + calcDist.X2.ToString("0.000") + "," + calcDist.Y2.ToString("0.000");
                        cLog.DistLogSave(Vision[visionNo[0]].CFG.eCamName, sDistLog);
                        

                        string OKNG = "OK";
                        if (iPCResult != ePCResult.WAIT) OKNG = iPCResult.ToString();

                        if (iPCResult == ePCResult.WAIT)
                        {
                            iPCResult = ePCResult.OK;
                        }

                        //결과값 보내기
                        SetResult(Vision[visionNo[0]].CFG.eCamName, iPCResult, visionNo[0], sReply, align, -1, true, calcDist);

                        if (iPCResult != ePCResult.RETRY && iPCResult != ePCResult.VISION_REPLY_WAIT)
                        {
                            sTime[sRequest].Stop();
                            WriteCycleTime(visionNo[0], sTime[sRequest].ElapsedMilliseconds);
                        }
                        //DV
                        double[] inspData = { calcDist.X1, calcDist.Y1, calcDist.X2, calcDist.Y2 };  //ABS
                        double[] APCData = { calcDist.X1, calcDist.Y1, calcDist.X2, calcDist.Y2 };

                        double[] ASPData = { dist.X1, dist.Y1 };

                        if (Vision[visionNo[0]].CFG.eCamName == nameof(eCamNO.Laser1))
                        {
                            IF.DV_DataWrite(Address.DV.LaserInsp1, inspData);
                            IF.DataWrite_InspeAscii(Address.DV.LaserInsp1_Ascii, dist);
                            IF.DV_DataWrite(Address.DV.LaserInsp1_ASP, ASPData);
                        }
                        else
                        {
                            IF.DV_DataWrite(Address.DV.LaserInsp2, inspData);
                            IF.DataWrite_InspeAscii(Address.DV.LaserInsp2_Ascii, dist);
                            IF.DV_DataWrite(Address.DV.LaserInsp2_ASP, ASPData);
                        }

                        if (iPCResult != ePCResult.RETRY && iPCResult != ePCResult.VISION_REPLY_WAIT)
                        {
                            //sTime[sRequest].Stop();
                            //WriteCycleTime(visionNo[0], sTime[sRequest].ElapsedMilliseconds);


                            if (iPCResult != ePCResult.OK)
                            {
                                Vision[visionNo[0]].ImageSave(false, Vision[visionNo[0]].CFG.ImageSaveType, iPCResult.ToString(), "", imgKind);
                                //Vision[visionNo2].ImageSave(false, Vision[visionNo2].CFG.ImageSaveType, iPCResult.ToString());
                            }
                            else
                            {
                                if (Vision[visionNo[0]].CFG.ImageSave) Vision[visionNo[0]].ImageSave(true, Vision[visionNo[0]].CFG.ImageSaveType, iPCResult.ToString(), "", imgKind);
                                //if (Vision[visionNo2].CFG.ImageSave) Vision[visionNo2].ImageSave(true, Vision[visionNo2].CFG.ImageSaveType, iPCResult.ToString());
                            }

                            IF.SendData("LaserSendCellID," + visionNo[0].ToString() + "," + CONST.PLCDeviceType + Vision[visionNo[0]].LaserSendCellIDAddress.ToString());
                            if (sRequest == plcRequest.Laser1Inspection)
                            {
                                IF.writeAPNCode(CONST.Address.PC.APNCode1, apnCode);
                            }
                            else
                            {
                                IF.writeAPNCode(CONST.Address.PC.APNCode2, apnCode);
                            }

                           

                            string APNOK = "OK";
                            if (Vision[visionNo[0]].LaserSendCellID.Trim() != apnCode)
                            {
                                APNOK = "NG";
                                pcResult[sCompareBit] = (int)ePCResult.SPEC_OVER;
                            }
                            else
                            {
                                pcResult[sCompareBit] = (int)ePCResult.OK;
                            }
                            string strLogSave = history.ToolNo + "," +
                                                history.PanelID + "," +
                                                OKNG + "," +
                                                calcDist.X1.ToString("0.000") + "," +
                                                calcDist.Y1.ToString("0.000") + "," +
                                                calcDist.X2.ToString("0.000") + "," +
                                                calcDist.Y2.ToString("0.000") + "," +
                                                System.Math.Abs(calcDist.X1 - specX.Middle1).ToString("0.000") + "," +
                                                System.Math.Abs(calcDist.Y1 - specY.Middle1).ToString("0.000") + "," +
                                                System.Math.Abs(calcDist.X2 - specX.Middle2).ToString("0.000") + "," +
                                                System.Math.Abs(calcDist.Y2 - specY.Middle2).ToString("0.000") + "," +
                                                diff.X + "," +
                                                diff.Y + "," +
                                                Vision[visionNo[0]].LaserSendCellID.Trim() + "," +
                                                apnCode + "," +
                                                APNOK;
                            cLog.Save(LogKind.LaserPositionInsp, strLogSave);



                            //로그 저장 //확인필요
                            //logSavePre(LogKind.AlignSCFReel, visionNo[0], Mark1, Mark2, align, LDist[visionNo[0]], "NoPanelID", iPCResult.ToString(), (Convert.ToDouble(sLaserAlign.ElapsedMilliseconds) / 1000), scorelogData, Mark3);

                            //210118 cjm 레이저 로그
                            //cLog.Save(LogKind.Laser, Vision[visionNo[0]].PanelID.Trim() + "," + Vision[visionNo[0]].LaserSendCellID + "," + Vision[visionNo[0]].MCRCellID);
                        }
                    };
                    wkr.RunWorkerCompleted += delegate (object sender1, RunWorkerCompletedEventArgs e2)
                    {
                        //if (CONST.m_bSystemLog)
                        //    cLog.Save(LogKind.System, "plcSCFReel PatternSearch End");
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
                //pcResult[sCompareBit] = (int)ePCResult.WAIT;
                ManualMarkInitial(visionNo[0]); //수동마크 초기화
            }
            else if (CONST.bPLCReq[sRequest]
                && pcResult[sReply] == (int)ePCResult.VISION_REPLY_WAIT
                && manualMark[visionNo[0]].bWorkerVisible)
            //&& manualMark[Vision_No.vsSCFReel2].bWorkerVisible)
            {
                if (sRequest == plcRequest.Laser1Inspection)
                    CheckManualMarkPopup(visionNo[0], CONST.eImageSaveKind.LaserInspection1);  //수동 마크창 세팅
                else
                    CheckManualMarkPopup(visionNo[0], CONST.eImageSaveKind.LaserInspection2);  //수동 마크창 세팅

                if (CheckManualMarkDone(visionNo[0])) //수동마크 작업자가 완료했는지..
                {
                    pcResult[sReply] = (int)ePCResult.WAIT;
                }
                else if (CheckManualMarkBypass(visionNo[0])) //수동마크 bypass인지..
                {
                    ePCResult iPCResult = ePCResult.WORKER_BY_PASS;
                    SetResult(Vision[visionNo[0]].CFG.eCamName, iPCResult, visionNo[0], sReply);
                    if (iPCResult != ePCResult.OK)
                    {
                        if (sRequest == plcRequest.Laser1Inspection)
                            Vision[visionNo[0]].ImageSave(false, Vision[visionNo[0]].CFG.ImageSaveType, iPCResult.ToString(), "", CONST.eImageSaveKind.LaserInspection1);
                        else
                            Vision[visionNo[0]].ImageSave(false, Vision[visionNo[0]].CFG.ImageSaveType, iPCResult.ToString(), "", CONST.eImageSaveKind.LaserInspection2);
                        //Vision[Vision_No.vsSCFReel2].ImageSave(false, Vision[Vision_No.vsSCFReel2].CFG.ImageSaveType, iPCResult.ToString());
                    }
                    sTime[sRequest].Stop();
                    WriteCycleTime(visionNo[0], sTime[sRequest].ElapsedMilliseconds);
                }
            }
            #endregion           
        }

        private void MCRRead(short sRequest, short sReply, params short[] visionNo)
        {
            #region MCR READ

            TimeOutCheck2(sRequest, eTimeOutType.Process);

            if (CONST.bPLCReq[sRequest] && pcResult[sReply] == (int)ePCResult.WAIT)
            {
                if (!visionBackground[sRequest])
                {
                    visionBackground[sRequest] = true;
                    TimeOutCheck2(sRequest, eTimeOutType.Set);
                    if (sTime[sRequest] == null) sTime[sRequest] = new Stopwatch();
                    ePCResult iPCResult = ePCResult.WAIT;
                    
                    sTime[sRequest].Reset();
                    sTime[sRequest].Start();

                    BackgroundWorker wkr = new BackgroundWorker();

                    wkr.DoWork += delegate (object sender1, DoWorkEventArgs e1)
                    {

                        Vision[visionNo[0]].cogDS.InteractiveGraphics.Clear();
                        Vision[visionNo[0]].cogDS.StaticGraphics.Clear();

                        ////Vision[visionNo2].cogDS.InteractiveGraphics.Clear();
                        if (Vision[visionNo[0]].PanelID == "")
                        {
                            IF.SendData("PNID," + visionNo[0].ToString() + "," + CONST.PLCDeviceType + Vision[visionNo[0]].PanelIDAddress.ToString());
                        }

                        string apnCode = "";
                        cs2DAlign.ptXY[] codePoint = new cs2DAlign.ptXY[4];

                        Vision[visionNo[0]].readID(ref apnCode, ref codePoint, Menu.frmSetting.revData.mLaser.MCRSearchKind);

                        iPCResult = ePCResult.OK;
                        pcResult[sReply] = (int)iPCResult;
                    };
                    wkr.RunWorkerCompleted += delegate (object sender1, RunWorkerCompletedEventArgs e2)
                    {
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
            }
            
            #endregion           
        }



        /// <summary>
        /// 마크 하나를 카메라 중심으로 xy만 이동시키기 위한 함수
        /// </summary>
        /// <param name="VisionNo"></param>
        /// <param name="CalPos"></param>
        /// <param name="Mark1"></param>
        /// <param name="Align"></param>
        private ePCResult LaserAlignXY(int VisionNo, int CalPos, cs2DAlign.ptXYT Mark1, ref cs2DAlign.ptAlignResult Align)
        {
            cs2DAlign.ptXY sourcePixel1 = new cs2DAlign.ptXY();
            cs2DAlign.ptXY targetPixel1 = new cs2DAlign.ptXY();
            sourcePixel1.X = Mark1.X;
            sourcePixel1.Y = Mark1.Y;
            targetPixel1.X = Vision[VisionNo].ImgX / 2.0;
            targetPixel1.Y = Vision[VisionNo].ImgY / 2.0;

            Align.X = (sourcePixel1.X - targetPixel1.X) * (-1);
            Align.Y = sourcePixel1.Y - targetPixel1.Y;

            cs2DAlign.ptXY resolution = new cs2DAlign.ptXY();
            cs2DAlign.ptXY pixelCnt = new cs2DAlign.ptXY();

            Menu.rsAlign.getResolution(CalPos, ref resolution, ref pixelCnt, true);

            Align.X *= resolution.X;
            Align.Y *= resolution.Y;

            return ePCResult.WAIT;
        }

        public ePCResult LaserAlignPosCheck(cs2DAlign.ptXYT mark, int visionNO, int calpos, ref string str, ref cs2DAlign.ptXY diff)
        {
            cs2DAlign.ptXY resol = new cs2DAlign.ptXY();
            cs2DAlign.ptXY pixelcnt = new cs2DAlign.ptXY();

            Menu.rsAlign.getResolution(calpos, ref resol, ref pixelcnt);

            diff.X = System.Math.Abs((mark.X - Vision[visionNO].CFG.TargetX[0]) * resol.X);
            diff.Y = System.Math.Abs((mark.Y - Vision[visionNO].CFG.TargetY[0]) * resol.Y);

            double spec = Menu.frmSetting.revData.mSizeSpecRatio.LaserAlignPosTor;



            if (spec > 0)
            {
                if (diff.X > spec || diff.Y > spec)
                {
                    str = "REF Position XDiff : " + diff.X.ToString("0.000") + "  YDiff : " + diff.Y.ToString("0.000");                    
                    return ePCResult.ERROR_LCHECK;
                }
            }
            return ePCResult.WAIT;


        }

        public cs2DAlign.ptXXYY MarktoCodeDist(int visionNO, int calpos, cs2DAlign.ptXYT Mark, cs2DAlign.ptXY CodeXY)
        {
            cs2DAlign.ptXXYY dist1 = new cs2DAlign.ptXXYY();
            
            cs2DAlign.ptXY resol = new cs2DAlign.ptXY();
            cs2DAlign.ptXY pixelCnt = new cs2DAlign.ptXY();
            Menu.rsAlign.getResolution(calpos, ref resol, ref pixelCnt);

            dist1.X1 = System.Math.Abs((CodeXY.X - Mark.X) * resol.X);
            dist1.Y1 = System.Math.Abs((CodeXY.Y - Mark.Y) * resol.Y);

            dist1.X2 = Vision[visionNO].gerMarkDist(Mark, Mark.T + CONST.rad90, CodeXY) * resol.X;
            dist1.Y2 = Vision[visionNO].gerMarkDist(Mark, Mark.T, CodeXY) * resol.Y;


            if (CodeXY.X < Mark.X)
            {
                dist1.X1 = (-1) * dist1.X1;
                dist1.X2 = (-1) * dist1.X2;
            }

            if (CodeXY.Y > Mark.Y)
            {
                dist1.Y1 = (-1) * dist1.Y1;
                dist1.Y2 = (-1) * dist1.Y2;
            }
            return dist1;
        }

    }


}
