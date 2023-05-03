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
        private void PPIDChange()
        {
            #region Current PPID Change
            //plc에서 pc리플라이 받고 1초후에 리퀘스트 끄기로 함(pc여러대 다 받을려면 바로끄면 간혹 한대가 안될수도 있으니깐..)
            if (CONST.bPLCReq[CONST.CUnit.BitControl.plcCurrentRecipeChangeReq] && pcResult[CONST.CUnit.Reply.pcCurrentRecipeChangeReply] == (int)ePCResult.WAIT)
            {
                if (!visionBackground[CONST.CUnit.Reply.pcCurrentRecipeChangeReply]) //여러번 들어오지말라고 해둠.
                {
                    visionBackground[CONST.CUnit.Reply.pcCurrentRecipeChangeReply] = true;
                    pcResult[CONST.CUnit.Reply.pcCurrentRecipeChangeReply] = (int)ePCResult.OK;

                    //2018.07.11 recipe 관련 추가 khs
                    CONST.RunRecipe.OldRecipeName = CONST.RunRecipe.RecipeName;

                    IF.SendData("RCPID," + CONST.PLCDeviceType + CONST.Address.PLC.RECIPEID.ToString());
                    IF.SendData("RCPPARAM," + CONST.PLCDeviceType + CONST.Address.PLC.RECIPEPARAM.ToString() + "," + Enum.GetValues(typeof(eRecipe)).Length);

                    if (CONST.RunRecipe.OldRecipeName != CONST.RunRecipe.RecipeName && CONST.RunRecipe.RecipeName != "")
                    {
                        //Menu.frmRecipe.RecipChangeDataCopy();
                        Menu.Config.RecipeNameWrite("RecipeName", "RecipeID", CONST.RunRecipe.OldRecipeName);
                    }
                    else if (CONST.RunRecipe.RecipeName == null || CONST.RunRecipe.RecipeName == "")
                    {
                        //MessageBox.Show("Recipe ID Check");
                    }

                    Menu.frmRecipe.RecipeParamDisp();

                    ParamChange = true;
                }
            }
            else if (!CONST.bPLCReq[CONST.CUnit.BitControl.plcCurrentRecipeChangeReq] && pcResult[CONST.CUnit.Reply.pcCurrentRecipeChangeReply] != (int)ePCResult.WAIT)
            {
                visionBackground[CONST.CUnit.Reply.pcCurrentRecipeChangeReply] = false;
                pcResult[CONST.CUnit.Reply.pcCurrentRecipeChangeReply] = (int)ePCResult.WAIT;
            }

            #endregion
        }
        private void TimeChange()
        {
            #region Time Change Request

            if (CONST.bPLCReq[CONST.CUnit.BitControl.plcTimeChangeReq] && pcResult[CONST.CUnit.Reply.pcTimeChangeReply] == (int)ePCResult.WAIT)
            {
                pcResult[CONST.CUnit.Reply.pcTimeChangeReply] = (int)ePCResult.OK;

                IF.SendData("TIME," + CONST.PLCDeviceType + (CONST.Address.PLC.CELLID1 - 28).ToString());
            }
            else if (!CONST.bPLCReq[CONST.CUnit.BitControl.plcTimeChangeReq] && pcResult[CONST.CUnit.Reply.pcTimeChangeReply] != (int)ePCResult.WAIT)
            {
                pcResult[CONST.CUnit.Reply.pcTimeChangeReply] = (int)ePCResult.WAIT;
            }
            #endregion
        }

        public void PLCStatusPC1()
        {
            //status1 -> initial, status10 -> Align
            ShowPLCStatus10(Vision_No.LoadingPre1, CONST.bPLCReq[plcRequest.LoadingPre1Align]);
            ShowPLCStatus10(Vision_No.LoadingPre2, CONST.bPLCReq[plcRequest.LoadingPre2Align]);
            ShowPLCStatus10(Vision_No.Laser1, CONST.bPLCReq[plcRequest.Laser1Align]);
            ShowPLCStatus10(Vision_No.Laser2, CONST.bPLCReq[plcRequest.Laser2Align]);

            ShowPLCStatus10(Vision_No.Laser1, CONST.bPLCReq[plcRequest.Laser1Inspection]);
            ShowPLCStatus10(Vision_No.Laser2, CONST.bPLCReq[plcRequest.Laser2Inspection]);

        }

        public void PCStatusPC1()
        {
            ShowPCStatus1(Vision_No.LoadingPre1, pcResult[pcReply.LoadingPre1Align]);
            ShowPCStatus1(Vision_No.LoadingPre2, pcResult[pcReply.LoadingPre2Align]);
            ShowPCStatus1(Vision_No.Laser1, pcResult[pcReply.Laser1Align]);
            ShowPCStatus1(Vision_No.Laser2, pcResult[pcReply.Laser2Align]);

            ShowPCStatus1(Vision_No.Laser1, pcResult[pcReply.Laser1Inspection]);
            ShowPCStatus1(Vision_No.Laser2, pcResult[pcReply.Laser2Inspection]);
        }
    }
}
