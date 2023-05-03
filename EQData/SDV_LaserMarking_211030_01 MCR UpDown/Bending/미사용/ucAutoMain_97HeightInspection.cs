using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Diagnostics;
using rs2DAlign;
using Cognex.VisionPro.Display;
using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.CalibFix;
using System.Threading;
namespace Bending
{
    public partial class ucAutoMain
    {
        Stopwatch swInspHeight = new Stopwatch();

        private void HeightInspection()
        {
            #region Height Inspection Initial 
            //TimeOutCheck2(CONST.AAM_PLC2.BitControl.plcHeightInspInitReq, eTimeOutType.Process);
            //if (CONST.bPLCReq[CONST.AAM_PLC2.BitControl.plcHeightInspInitReq] && pcResult[CONST.AAM_PLC2.Reply.pcHeightInspInitReply] == (int)ePCResult.WAIT)
            //{
            //    TimeOutCheck2(CONST.AAM_PLC2.BitControl.plcHeightInspInitReq, eTimeOutType.Set);

            //    CONST.PanelIDInspHeight = "";
            //    IF.SendData("PNID,H1," + CONST.PLCDeviceType + CONST.Address.PLC.CELLID5.ToString());

            //    swInspHeight.Reset();
            //    swInspHeight.Start();

            //    //초기화 후 트리거 보냄
            //    Bending.Menu.Config.sHResult.bOKNG = false;
            //    IF.SendData("HEIGHTTRIGGER");

            //    WriteCycleTime(Vision_No.vsInspHeight, 0);
            //    LogDisp(Vision_No.vsInspHeight, "HEIGHT INITIAL OK ");
            //    pcResult[CONST.AAM_PLC2.Reply.pcHeightInspInitReply] = (int)ePCResult.VISION_REPLY_WAIT;
            //}
            //else if (!CONST.bPLCReq[CONST.AAM_PLC2.BitControl.plcHeightInspInitReq]
            //    && pcResult[CONST.AAM_PLC2.Reply.pcHeightInspInitReply] != (int)ePCResult.WAIT)
            //{
            //    TimeOutCheck2(CONST.AAM_PLC2.BitControl.plcHeightInspInitReq, eTimeOutType.UnSet);

            //    pcResult[CONST.AAM_PLC2.Reply.pcHeightInspInitReply] = (int)ePCResult.WAIT;
            //    Bending.Menu.Config.sHResult.bOKNG = false;
            //    //bHeightSend = false;
            //}
            //else if (CONST.bPLCReq[CONST.AAM_PLC2.BitControl.plcHeightInspInitReq] && pcResult[CONST.AAM_PLC2.Reply.pcHeightInspInitReply] == (int)ePCResult.VISION_REPLY_WAIT)
            //{
            //    if(Bending.Menu.Config.sHResult.bOKNG)
            //    {
            //        WritePanleID(Vision_No.vsInspHeight, CONST.PanelIDInspHeight.Substring(0, 16));

            //        Bending.Menu.Config.sHResult.bOKNG = false;

            //        BackgroundWorker wkr = new BackgroundWorker();
            //        ePCResult iPCResult = ePCResult.WAIT;

            //        wkr.DoWork += delegate (object sender1, DoWorkEventArgs e1)
            //        {
            //            iPCResult = HeightInspection(iPCResult);
            //        };

            //        wkr.RunWorkerCompleted += delegate (object sender1, RunWorkerCompletedEventArgs e2)
            //        {
            //            if (CONST.m_bSystemLog)
            //                cLog.Save(LogKind.System, "plcHeightInspectionReq End");

            //            swInspHeight.Stop();
            //            WriteCycleTime(Vision_No.vsInspHeight, swInspHeight.ElapsedMilliseconds);

            //            //visionBackground[CONST.AAM_PLC2.BitControl.plcHeightInspReq] = false;
            //            wkr.Dispose();
            //        };

            //        wkr.RunWorkerAsync();
            //    }
            //}
            #endregion
        }
        //public ePCResult HeightInspection(ePCResult iPCResult)
        //{
        //    double dSpec1 = 0;
        //    double dSpec2 = 0;
        //    string sLog = "";

        //    try
        //    {
        //        Bending.Menu.Config.sHResult.dValueDisp = new string[Bending.Menu.Config.sHeight.Length];

        //        for (int i = 0; i < Bending.Menu.Config.sHeight.Length; i++)
        //        {
        //            sLog = sLog + " " + Bending.Menu.Config.sHeight[i].HeightReference + " : " + Bending.Menu.Config.sHResult.dValue[i].ToString() + ",";
        //            Bending.Menu.Config.sHResult.dValueDisp[i] = Bending.Menu.Config.sHResult.dValue[i].ToString();
        //        }
        //        if (Bending.Menu.Config.sHResult.iOKNG == 1)
        //        {
        //            for (int i = 0; i < Bending.Menu.Config.sHeight.Length; i++)
        //            {
        //                //double dResultvalue = 0;
        //                dSpec1 = double.Parse(Bending.Menu.Config.sHeight[i].HeightSpec) + double.Parse(Bending.Menu.Config.sHeight[i].HeightTolerence);
        //                dSpec2 = double.Parse(Bending.Menu.Config.sHeight[i].HeightSpec) - double.Parse(Bending.Menu.Config.sHeight[i].HeightTolerence);

        //                if (Bending.Menu.Config.sHResult.dValue[i] > dSpec1 || Bending.Menu.Config.sHResult.dValue[i] < dSpec2)
        //                {
        //                    //LogDisp(Vision_No.vsInspHeight, "Height Inspection NG " + sLog);
        //                    LogDisp(Vision_No.vsInspHeight, "Height Inspection NG -> " + Bending.Menu.Config.sHeight[i].HeightReference + " : " + Bending.Menu.Config.sHResult.dValue[i].ToString());
        //                    iPCResult = ePCResult.SPEC_OVER;                         //HeightResult[i, CalculCnt] = sHResult.dValueDisp[i];
        //                                                                //AutoMainLogDatacalculator(i, HeightResult, ref CPK);
        //                                                                //dgvSpec1[i, 3].Value = CPK[i].ToString("0.000");
        //                }
        //            }
        //            if(iPCResult == ePCResult.WAIT)
        //            {
        //                LogDisp(Vision_No.vsInspHeight, "Height Inspection OK " + sLog);
        //            }
        //        }
        //        else if (Bending.Menu.Config.sHResult.iOKNG == 2)
        //        {
        //            LogDisp(Vision_No.vsInspHeight, "Height Inspection ERROR " + sLog);
        //            iPCResult = ePCResult.BY_PASS;
        //        }
        //        else if (Bending.Menu.Config.sHResult.iOKNG == 3)
        //        {
        //            LogDisp(Vision_No.vsInspHeight, "Height Inspection INVALID " + sLog);
        //            iPCResult = ePCResult.BY_PASS;
        //        }
        //        //pcy200708 임시 화면표시와 dv보고
        //        //DV 항목
        //        int iheight = CONST.AAM_PLC2.DVAddress.HeightInspection;
        //        double[] heightData = Menu.Config.sHResult.dValue;
        //        IF.DV_DataWrite(iheight, heightData);

        //        int toolno = 0;
        //        try
        //        {
        //            //int.TryParse(CONST.PanelIDInspHeight.Substring(21, 1), out history.RetryCnt);
        //            //history.PanelID = CONST.PanelIDInspHeight;
        //            int.TryParse(CONST.PanelIDInspHeight.Substring(32, 1), out toolno);
        //        }
        //        catch (Exception EX)
        //        {
        //            iPCResult = ePCResult.BY_PASS;
        //        }

        //        ViewDGVResult(dgvResult2, CONST.PanelIDInspHeight, toolno.ToString(), heightData);
        //        //CompareDgvSpec(dgvResult2, dgvSpec2);
        //        CalcDgvCPK(dgvResult2, dgvSpec2);
        //    }
        //    catch (Exception EX)
        //    {
        //        iPCResult = ePCResult.BY_PASS;
        //    }
        //    if (iPCResult == ePCResult.WAIT)
        //    {
        //        iPCResult = ePCResult.OK;
        //    }
        //    iPCResult = ePCResult.OK; //무조건 ok보내기로 함
        //    SetResult(nameof(eCamNO4.LMI), iPCResult, Vision_No.vsInspHeight, CONST.AAM_PLC2.Reply.pcHeightInspInitReply);  //이거 확인 필요함.lkw
        //    return iPCResult;
        //}
    }
}
