using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.Dimensioning;
using rs2DAlign;

namespace Bending
{
    public partial class frmInspShift : Form
    {
        public frmInspShift()
        {
            InitializeComponent();
        }
        public struct sCalDist
        {
            public double distX1;
            public double distY1;
            public double distX2;
            public double distY2;
        }

        private void InspMeasureMent()
        {
            sCalDist dist = new sCalDist();
            sCalDist shift = new sCalDist();
            //Inspection(ref dist, ref shift);

            //cogDS1.Image = Bending.Menu.frmAutoMain.Vision[VisionNo1].cogDS.Image;
            //cogDS2.Image = Bending.Menu.frmAutoMain.Vision[VisionNo2].cogDS.Image;
            lbDataLX.Text = dist.distX1.ToString("0.000");
            lbDataLY.Text = dist.distY1.ToString("0.000");
            lbDataRX.Text = dist.distX2.ToString("0.000");
            lbDataRY.Text = dist.distY2.ToString("0.000");

            lbPaenlShiftX1.Text = shift.distX1.ToString("0.000");
            lbPaenlShiftY1.Text = shift.distY1.ToString("0.000");
            lbPaenlShiftX2.Text = shift.distX2.ToString("0.000");
            lbPaenlShiftY2.Text = shift.distY2.ToString("0.000");

            cogDS1.Fit(true);
            cogDS2.Fit(true);
        }


        //2017.09.27
        private void InspMeasureMentBendingPre()
        {
            rs2DAlign.cs2DAlign.ptXXYY dist = new rs2DAlign.cs2DAlign.ptXXYY();
            
            InspectionBendingPre(ref dist);

            //cogDS2.Image = Bending.Menu.frmAutoMain.Vision[VisionNo1].cogDS.Image;
            //cogDS1.Image = Bending.Menu.frmAutoMain.Vision[VisionNo2].cogDS.Image;
            lbDataLX.Text = dist.X1.ToString("0.000");
            lbDataLY.Text = dist.Y1.ToString("0.000");
            lbDataRX.Text = dist.X2.ToString("0.000");
            lbDataRY.Text = dist.Y2.ToString("0.000");

            

            cogDS2.Fit(true);
            cogDS1.Fit(true);
        }
        
        //int VisionNo1;
        //int VisionNo2;

        //2017.09.27 Inspection Manual Popup
        //double refX1 = 0;
        //double refY1 = 0;
        //double refX2 = 0;
        //double refY2 = 0;
        //bool bendingPre;
        //int bendPreCnt = 0;

        public void setImgBendingPre(double dX1, double dY1, double dX2, double dY2, int Cnt = 0)  //Ref Mark 위치 같이 받아 옴
        {
            //try
            //{
            //    cogDS1.InteractiveGraphics.Clear();
            //    cogDS2.InteractiveGraphics.Clear();
            //}
            //catch
            //{
            //}
            //CheckForIllegalCrossThreadCalls = false;

            ////pnResult.Visible = true;

            
            //bendingPre = true;
            //VisionNo1 = Vision_No.vsBendPre1;
            //VisionNo2 = Vision_No.vsBendPre2;
            ////lbTitle.Text = "BENDING 3 Manual Bending";

            //refX1 = dX1;
            //refY1 = dY1;
            //refX2 = dX2;
            //refY2 = dY2;
            //bendPreCnt = Cnt;
 
            //Visible = true;


            //InspMeasureMentBendingPre();
            ////lbSpecLX.Text = CONST.RunRecipe.Param[CONST.rcpFOAM_ATTACH_MARK_TO_EDGE_X].Value;
            ////lbSpecLY.Text = Bending.Menu.frmSetting.revData.mSizeSpecRatio.FoamCheckRefY.ToString("0.000");
            ////lbSpecRX.Text = CONST.RunRecipe.Param[CONST.rcpFOAM_ATTACH_MARK_TO_EDGE_X].Value;
            ////lbSpecRY.Text = Bending.Menu.frmSetting.revData.mSizeSpecRatio.FoamCheckRefY.ToString("0.000");

            //lbTitle.Text = "FOAM Inspection Check";
            //lbmX1.Visible = false;
            //lbmX2.Visible = false;
            //lbmY1.Visible = false;
            //lbmY2.Visible = false;

            //lbPaenlShiftX1.Visible = false;
            //lbPaenlShiftY1.Visible = false;
            //lbPaenlShiftX2.Visible = false;
            //lbPaenlShiftY2.Visible = false;

            //lbSpecX.Text = Bending.Menu.frmSetting.revData.mSizeSpecRatio.FoamInspSpec.ToString("0.000");
            //lbSpecY.Text = Bending.Menu.frmSetting.revData.mSizeSpecRatio.FoamInspSpec.ToString("0.000");
        }       

        private void button_ZoomIn_Click(object sender, EventArgs e)
        {
            cogDS2.MouseMode = CogDisplayMouseModeConstants.ZoomIn;
        }

        private void button_ZoomOut_Click(object sender, EventArgs e)
        {
            cogDS2.MouseMode = CogDisplayMouseModeConstants.ZoomOut;
        }

        private void button_FitImage_Click(object sender, EventArgs e)
        {
            cogDS2.Fit(true);
        }

        private void button_Point_Click(object sender, EventArgs e)
        {
            cogDS2.MouseMode = CogDisplayMouseModeConstants.Pointer;
        }

        private void button_Pan_Click(object sender, EventArgs e)
        {
            cogDS2.MouseMode = CogDisplayMouseModeConstants.Pan;
        }

        private void Grab(int dispNO)
        {
            //CogImage8Grey cogGrab = new CogImage8Grey();

            //int VisionNo = VisionNo1;
            
            //if (dispNO == 1)
            //{
            //    VisionNo = VisionNo2;                
            //}
            //Bending.Menu.frmAutoMain.Vision[VisionNo].Grab(ref cogGrab);
            //if (dispNO == 0) cogDS2.Image = cogGrab;
            //else if (dispNO == 1) cogDS1.Image = cogGrab;
        }

        private void button_Grab1_Click(object sender, EventArgs e)
        {
            Grab(0);
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            cogDS1.MouseMode = CogDisplayMouseModeConstants.ZoomIn;
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            cogDS1.MouseMode = CogDisplayMouseModeConstants.ZoomOut;
        }

        private void btnFitImage_Click(object sender, EventArgs e)
        {
            cogDS1.Fit(true);
        }

        private void btnPoint_Click(object sender, EventArgs e)
        {
            cogDS1.MouseMode = CogDisplayMouseModeConstants.Pointer;
        }

        private void btnPan_Click(object sender, EventArgs e)
        {
            cogDS1.MouseMode = CogDisplayMouseModeConstants.Pan;
        }

        private void button_Grab_Click(object sender, EventArgs e)
        {
            Grab(1);
        }        

        private void Close_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you Close", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                Visible = false;
            }
        }

        private void btnMeasure_Click(object sender, EventArgs e)
        {
            //if (!bendingPre) InspMeasureMent();
            //else if (bendingPre) InspMeasureMentBendingPre();
        }

        //private void Inspection(ref sCalDist dist, ref sCalDist shift)
        //{
        //    double dRefX1 = 0;
        //    double dRefY1 = 0;
        //    double dRefX2 = 0;
        //    double dRefY2 = 0;
        //    double dRefR = 0;

        //    double markX1 = 0;
        //    double markY1 = 0;
        //    double markX2 = 0;
        //    double markY2 = 0;

        //    bool result1 = false;
        //    bool result2 = false;
        //    int Cam1NO = VisionNo1;
        //    int Cam2NO = VisionNo2;

        //    Grab(0);
        //    Grab(1);

        //    double[] m_dInspection1_RefGlobal = new double[2];
        //    double[] m_dInspection2_RefGlobal = new double[2];
        //    double[] m_dInspection1_CurGlobal = new double[2];
        //    double[] m_dInspection2_CurGlobal = new double[2];
        //    double m_dInspectionAngle = 0;
        //    CogLine cogBaseLine1 = new CogLine();
        //    CogLine cogBaseLine2 = new CogLine();
        //    double[] m_dBending_Upper_L2P1 = new double[2];
        //    double[] m_dBending_Upper_L2P2 = new double[2];
        //    double[] m_dBending_Upper_P2P1 = new double[2];
        //    double[] m_dBending_Upper_P2P2 = new double[2];
        //    double[] m_dBending_Upper_Distance1 = new double[2];
        //    double[] m_dBending_Upper_Distance2 = new double[2];

        //    eCalPos calpos1 = eCalPos.Laser1;
        //    eCalPos calpos2 = eCalPos.Laser2;

        //    if (bendingPre)
        //    {
        //        //calpos1 = eCalPos.BendPre1;
        //        //calpos2 = eCalPos.BendPre2;
        //        //camNO1 = (eCamNO)eCamNO1.BendPre1;
        //        //camNO2 = (eCamNO)eCamNO1.BendPre2;
        //    }
        //    cs2DAlign.ptXY resolution1 = new cs2DAlign.ptXY();
        //    cs2DAlign.ptXY pixelCnt = new cs2DAlign.ptXY();
        //    cs2DAlign.ptXY resolution2 = new cs2DAlign.ptXY();

        //    Bending.Menu.rsAlign.getResolution((int)calpos1, ref resolution1, ref pixelCnt);
        //    Bending.Menu.rsAlign.getResolution((int)calpos2, ref resolution2, ref pixelCnt);

        //    result1 = Bending.Menu.frmAutoMain.Vision[Cam1NO].PatternSearchEnum(ref dRefX1, ref dRefY1, ref dRefR, ePatternKind.Panel);
        //    result2 = Bending.Menu.frmAutoMain.Vision[Cam2NO].PatternSearchEnum(ref dRefX2, ref dRefY2, ref dRefR, ePatternKind.Panel);
        //    if (result1 && result2)
        //    {
        //        //shift.distX1 = Math.Abs(dRefX1 - Bending.Menu.frmSetting.revData.mLcheck.Insp_Panel_ShiftRef_X1) * Bending.Menu.frmAutoMain.Vision[Cam1NO].CFG.ResolutionX;
        //        //shift.distY1 = Math.Abs(dRefY1 - Bending.Menu.frmSetting.revData.mLcheck.Insp_Panel_ShiftRef_Y1) * Bending.Menu.frmAutoMain.Vision[Cam1NO].CFG.ResolutionY;

        //        //shift.distX2 = Math.Abs(dRefX2 - Bending.Menu.frmSetting.revData.mLcheck.Insp_Panel_ShiftRef_X2) * Bending.Menu.frmAutoMain.Vision[Cam2NO].CFG.ResolutionX;
        //        //shift.distY2 = Math.Abs(dRefY2 - Bending.Menu.frmSetting.revData.mLcheck.Insp_Panel_ShiftRef_Y2) * Bending.Menu.frmAutoMain.Vision[Cam2NO].CFG.ResolutionY;

        //        shift.distX1 = Math.Abs(dRefX1 - Bending.Menu.frmAutoMain.Vision[(short)camNO1].CFG.TargetX[0]) * resolution1.X;
        //        shift.distY1 = Math.Abs(dRefY1 - Bending.Menu.frmAutoMain.Vision[(short)camNO1].CFG.TargetY[0]) * resolution1.Y;

        //        shift.distX2 = Math.Abs(dRefX2 - Bending.Menu.frmAutoMain.Vision[(short)camNO2].CFG.TargetX[0]) * resolution2.X;
        //        shift.distY2 = Math.Abs(dRefY2 - Bending.Menu.frmAutoMain.Vision[(short)camNO2].CFG.TargetY[0]) * resolution2.Y;

        //        if (Bending.Menu.frmAutoMain.Vision[Cam1NO].PatternSearchEnum(ref markX1, ref markY1, ref dRefR, ePatternKind.FPC) && Bending.Menu.frmAutoMain.Vision[Cam2NO].PatternSearchEnum(ref markX2, ref markY2, ref dRefR, ePatternKind.FPC))
        //        {
        //            //GetInspectionGlobalPos(Cam1NO, true, false, Bending.Menu.frmAutoMain.Vision[Cam1NO].CFG.InsTargetX, Bending.Menu.frmAutoMain.Vision[Cam1NO].CFG.InsTargetY, ref m_dInspection1_RefGlobal);
        //            //GetInspectionGlobalPos(Cam2NO, true, false, Bending.Menu.frmAutoMain.Vision[Cam2NO].CFG.InsTargetX, Bending.Menu.frmAutoMain.Vision[Cam2NO].CFG.InsTargetY, ref m_dInspection2_RefGlobal);
        //            GetInspectionGlobalPos(Cam1NO, true, false, Bending.Menu.frmAutoMain.Vision[(short)camNO1].CFG.TargetX[0], Bending.Menu.frmAutoMain.Vision[(short)camNO1].CFG.TargetY[0], ref m_dInspection1_RefGlobal, resolution1, resolution2);
        //            GetInspectionGlobalPos(Cam2NO, true, false, Bending.Menu.frmAutoMain.Vision[(short)camNO2].CFG.TargetX[0], Bending.Menu.frmAutoMain.Vision[(short)camNO2].CFG.TargetY[0], ref m_dInspection2_RefGlobal, resolution1, resolution2);

        //            //획득 포지션을 글로벌 좌표로
        //            GetInspectionGlobalPos(Cam1NO, false, false, dRefX1, dRefY1, ref m_dInspection1_CurGlobal, resolution1, resolution2);
        //            GetInspectionGlobalPos(Cam2NO, false, false, dRefX2, dRefY2, ref m_dInspection2_CurGlobal, resolution1, resolution2);

        //            //위에서 나온 두 선의 각도를 구하는 함수.
        //            GetLineLineAngle(m_dInspection1_RefGlobal, m_dInspection2_RefGlobal, m_dInspection1_CurGlobal, m_dInspection2_CurGlobal, ref m_dInspectionAngle);

        //            m_dInspectionAngle = -m_dInspectionAngle;

        //            //** 구한 각도를 이용하여 선긋고 길이 구하기 **//

        //            CreateLine(0, dRefX1, dRefY1, m_dInspectionAngle, ref cogBaseLine1);
        //            CreateLine(1, dRefX2, dRefY2, m_dInspectionAngle, ref cogBaseLine2);

        //            PointToLine(0, markX1, markY1, cogBaseLine1, ref m_dBending_Upper_L2P1[0], ref m_dBending_Upper_L2P1[1], ref m_dBending_Upper_Distance1[1]);
        //            PointToLine(1, markX2, markY2, cogBaseLine2, ref m_dBending_Upper_L2P2[0], ref m_dBending_Upper_L2P2[1], ref m_dBending_Upper_Distance2[1]);

        //            PointToPoint(0, dRefX1, dRefY1, m_dBending_Upper_L2P1[0], m_dBending_Upper_L2P1[1], ref m_dBending_Upper_Distance1[0]);
        //            PointToPoint(1, dRefX2, dRefY2, m_dBending_Upper_L2P2[0], m_dBending_Upper_L2P2[1], ref m_dBending_Upper_Distance2[0]);


        //            dist.distX1 = m_dBending_Upper_Distance1[0] * resolution1.X;
        //            dist.distY1 = m_dBending_Upper_Distance1[1] * resolution1.Y;
        //            dist.distX2 = m_dBending_Upper_Distance2[0] * resolution2.X;
        //            dist.distY2 = m_dBending_Upper_Distance2[1] * resolution2.Y;
        //        }
        //        else
        //        {
        //            //MessageBox.Show("FPC Mark Search Fail");
        //        }
        //    }
        //    else
        //    {
        //        //MessageBox.Show("Panel Mark Search Fail");
        //    }
        //}
        //public void GetInspectionGlobalPos(int nCamNum, bool bRef, bool bTeach, double dPointX, double dPointY, ref double[] dGlobalPos, cs2DAlign.ptXY resolution1, cs2DAlign.ptXY resolution2)
        //{
        //    hakim 20170518
        //    double dGapDistance;
        //    double dM2MDistance;
        //    double dPosXMark1;
        //    double dPosXMark2;
        //    double dPosYMark1;
        //    double dPosYMark2;
        //    double dGapXDistance;
        //    double dResolutionLX;
        //    double dResolutionLY;
        //    double dResolutionRX;
        //    double dResolutionRY;

        //    if (bTeach == true)
        //    {
        //        dResolutionLX = 0.0044;
        //        dResolutionLY = 0.0044;
        //        dResolutionRX = 0.0044;
        //        dResolutionRY = 0.0044;
        //    }
        //    else
        //    {
        //        dResolutionLX = Vision[Vision_No.vsInspection1].CFG.ResolutionX;
        //        dResolutionLY = Vision[Vision_No.vsInspection1].CFG.ResolutionY;
        //        dResolutionRX = Vision[Vision_No.vsInspection2].CFG.ResolutionX;
        //        dResolutionRY = Vision[Vision_No.vsInspection2].CFG.ResolutionY;
        //        dResolutionLX = resolution1.X;
        //        dResolutionLY = resolution1.Y;
        //        dResolutionRX = resolution2.X;
        //        dResolutionRY = resolution2.Y;
        //    }

        //    eCamNO camNO1 = (eCamNO)eCamNO4.UpperInsp1_1;
        //    eCamNO camNO2 = (eCamNO)eCamNO4.UpperInsp1_2;
        //    if (bendingPre)
        //    {
        //        camNO1 = (eCamNO)eCamNO1.BendPre1;
        //        camNO2 = (eCamNO)eCamNO1.BendPre2;
        //    }

        //    dM2MDistance = CONST.RunRecipe.Param[eRecipe.PANEL_MARK_TO_MARK_LENGTH_X]; //레고블럭 마크간 거리(mm)
        //    dPosXMark1 = Bending.Menu.frmAutoMain.Vision[(short)camNO1].CFG.TargetX[0]; //1번 레고블럭 마크 인식 위치(pixel)
        //    dPosYMark1 = Bending.Menu.frmAutoMain.Vision[(short)camNO1].CFG.TargetY[0];
        //    dPosXMark2 = Bending.Menu.frmAutoMain.Vision[(short)camNO2].CFG.TargetX[0]; //2번 레고블럭 마크 인식 위치(pixel)
        //    dPosYMark2 = Bending.Menu.frmAutoMain.Vision[(short)camNO2].CFG.TargetY[0];

        //    우선 타겟을 거의 수평으로 놓겠지만, 실제로 그렇지 못하다.그러므로 실제 gap를 구하려려면 계산이 필요.
        //    dGapXDistance = Math.Sqrt(Math.Pow(dM2MDistance, 2) - Math.Pow((dPosYMark2 - dPosYMark1) * dResolutionRY, 2));

        //    Gap(mm) = Mark to Mark Distance X(mm) -(Cam1 FOV X(mm) -Mark1 X(mm)) -Mark2 X(mm)
        //    FOV를 따다 쓰지않고 레졸루션x픽셀로 한 이유는, 레졸루션을 3d측정값 기준으로 역산하여 넣을 예정이기 때문.
        //    dGapDistance = dGapXDistance - (cogDS1.Image.Width * dResolutionLX - dPosXMark1 * dResolutionLX) - dPosXMark2 * dResolutionRX;

        //    if (nCamNum == Vision_No.UpperInsp1_1)
        //    {
        //        1번 카메라의 경우 기존 좌표를 그대로 사용한다.레졸루션만 곱해서 픽셀값->치수로 변경.
        //       dGlobalPos[0] = dPointX * dResolutionLX;
        //        dGlobalPos[1] = dPointY * dResolutionLY;
        //    }
        //    else if (nCamNum == Vision_No.UpperInsp1_2)
        //    {
        //        Global Position X = FOV X + Gap + Mark2 X
        //        Offset를 Inspection2 Y만 받아오는 이유는, 1번 카메라의 좌표를 기준으로 2번이 따라가는 개념이기 때문에, Ref인식 후 1번마크와 2번마크의 Y 픽셀 차이만 넣어주는 개념으로 했다.(아직 넣지않았음.차이는 1픽셀정도)
        //        이렇게 offset를 넣어줌으로써 선에 기울기를 넣어줄 수 있다.
        //        물론 이렇게 넣어봐야 결론적으론 X / Y보정만 하는개념, T틀어짐은 보정이 불가능하다.하지만 어차피 제품도 거의 비슷비슷하게 들어 올 예정이므로, 잘 맞춰주면 어느정도 들어가지 않을까 싶다.
        //        사전에 카메라 수평을 나름 신경써서 맞추어 놓았기 때문에, 카메라 각도 편차에 따른 값 변화가 크지 않을것으로 예상(11번라인 기준, 만약 이 함수를 쭉 쓰게된다면, 나머지 라인도 카메라 수평을 잘 맞춰야한다).
        //        Offset입력은 Inspect Offset를 이용하지만, 입력값은 스케일이 아닌 픽셀값으로 입력한다(이 부분도 확정되면 한번 더 정리하도록 한다)
        //        if (bRef == true)
        //        {
        //            dGlobalPos[0] = cogDS1.Image.Width * dResolutionLX + dGapDistance + dPointX * dResolutionRX;

        //            dGlobalPos[1] = (dPointY + Vision[Vision_No.vsInspection2].CFG.InspectionOffsetY) * dResolutionRY;
        //        }
        //        else
        //        {
        //            dGlobalPos[0] = cogDS1.Image.Width * dResolutionLX + dGapDistance + dPointX * dResolutionRX;

        //            dGlobalPos[1] = dPointY * dResolutionRY;
        //        }
        //    }
        //    else
        //    {
        //        return;
        //    }
        //}
        public void GetLineLineAngle(double[] dRefPos1, double[] dRefPos2, double[] dCurPos1, double[] dCurPos2, ref double dAngle)
        {
            //hakim 20170521
            //angle = atan(Y1X2-X1Y2, X1X2+Y1Y2)
            double dX1, dX2, dY1, dY2;

            //위의 각도를 구하는 공식을 사용하려면, pos1이 0이 되어야한다.

            dX1 = dRefPos2[0] - dRefPos1[0];
            dY1 = dRefPos2[1] - dRefPos1[1];
            dX2 = dCurPos2[0] - dCurPos1[0];
            dY2 = dCurPos2[1] - dCurPos1[1];

            dAngle = Math.Atan2(dY1 * dX2 - dX1 * dY2, dX1 * dX2 + dY1 * dY2);
        }
        private void InspectionBendingPre(ref rs2DAlign.cs2DAlign.ptXXYY dist)
        {
            try
            {
                cogDS1.InteractiveGraphics.Clear();
                cogDS2.InteractiveGraphics.Clear();
            }
            catch
            {
            }
            Grab(0);
            Grab(1);
            CogRectangle cr = new CogRectangle();
            //int TH = 0;
            double SCFOffsetX = Bending.Menu.frmSetting.revData.mSizeSpecRatio.BDPreSCFCheckOffsetX;
            double SCFOffsetY = Bending.Menu.frmSetting.revData.mSizeSpecRatio.BDPreSCFCheckOffsetY;
            double SCFTH = Bending.Menu.frmSetting.revData.mSizeSpecRatio.BDPreSCFAttachTH;
            //if (Bending.Menu.frmAutoMain.Vision[Vision_No.vsBendPre1].AttachInspection(refX1, refY1, SCFOffsetX, SCFOffsetY, SCFTH, ref TH))
            //{
            //    //lbFoamAttach.Text = "OK";
            //    //lbFoamAttach.ForeColor = Color.Blue;
            //}
            //else
            //{
            //    //lbFoamAttach.Text = "NG";
            //    //lbFoamAttach.ForeColor = Color.Red;
            //}

            //double chPosX1 = Bending.Menu.frmAutoMain.Vision[CONST.PCDetach.Vision_No.vsDetachPre1].CFG.PSACheckInspX;
            //double chPosY1 = Bending.Menu.frmAutoMain.Vision[CONST.PCDetach.Vision_No.vsDetachPre1].CFG.PSACheckInspY;
            //double chPosX2 = Bending.Menu.frmAutoMain.Vision[CONST.PCDetach.Vision_No.vsDetachPre2].CFG.PSACheckInspX;
            //double chPosY2 = Bending.Menu.frmAutoMain.Vision[CONST.PCDetach.Vision_No.vsDetachPre2].CFG.PSACheckInspY;

            CogRectangle cr1 = new CogRectangle();
            //if (Bending.Menu.frmAutoMain.Vision[CONST.PCDetach.Vision_No.vsDetachPre1].psaAttachInspection(chPosX1, chPosY1, ref cr1))
            //{
            //    lbPSAAttach.Text = "OK";
            //    lbPSAAttach.ForeColor = Color.Blue;
            //}
            //else
            //{
            //    lbPSAAttach.Text = "NG";
            //    lbPSAAttach.ForeColor = Color.Red;
            //}

            //CogRectangle cr2 = new CogRectangle();
            //if (Bending.Menu.frmAutoMain.Vision[CONST.PCDetach.Vision_No.vsDetachPre2].psaRemoveInspection(chPosX2, chPosY2, ref cr2))
            //{
            //    lbPSARemove.Text = "OK";
            //    lbPSARemove.ForeColor = Color.Blue;
            //}
            //else
            //{
            //    lbPSARemove.Text = "NG";
            //    lbPSARemove.ForeColor = Color.Red;
            //}


            CogLine[] cWLine = new CogLine[2];
            CogLine[] cHLine = new CogLine[2];
            CogPointMarker[] marker = new CogPointMarker[2];
            marker[0] = new CogPointMarker();
            marker[1] = new CogPointMarker();
            //Bending.Menu.frmAutoMain.BendingPreFoamLineInsp(refX1, refY1, refX2, refY2, ref dist, false, ref cWLine, ref cHLine, ref marker);


            double dSpecX1 = 0;//double.Parse(CONST.RunRecipe.Param[CONST.rcpSCF_ATTACH_INSPECT_X1].Value);
            double dSpecX2 = 0;//double.Parse(CONST.RunRecipe.Param[CONST.rcpSCF_ATTACH_INSPECT_X2].Value);
            double dSpecY1 = 0;//double.Parse(CONST.RunRecipe.Param[CONST.rcpSCF_ATTACH_INSPECT_Y1].Value);
            double dSpecY2 = 0;//double.Parse(CONST.RunRecipe.Param[CONST.rcpSCF_ATTACH_INSPECT_Y2].Value);
            double dSpecX = 0;//double.Parse(CONST.RunRecipe.Param[CONST.rcpSCF_ATTACH_SPEC_X].Value);
            double dSpecY = 0;//double.Parse(CONST.RunRecipe.Param[CONST.rcpSCF_ATTACH_SPEC_Y].Value);
            if (Math.Abs(dist.X1 - dSpecX1) > dSpecX || Math.Abs(dist.X2 - dSpecX2) > dSpecX || Math.Abs(dist.Y1 - dSpecY1) > dSpecY || Math.Abs(dist.Y2 - dSpecY2) > dSpecY)
            {
                //lbFoamPos.Text = "NG";
                //lbFoamPos.ForeColor = Color.Red;
            }
            else
            {
                //lbFoamPos.Text = "OK";
                //lbFoamPos.ForeColor = Color.Blue;
            }

            try
            {
                cogDS2.InteractiveGraphics.Add(cr, "disp", false);
                cogDS2.InteractiveGraphics.Add(cr1, "disp", false);
                cogDS2.InteractiveGraphics.Add(cWLine[0], "disp", false);
                cogDS2.InteractiveGraphics.Add(cHLine[0], "disp", false);
                cogDS2.InteractiveGraphics.Add(marker[0], "disp", false);

                //cogDS1.InteractiveGraphics.Add(cr2, "disp", false);
                cogDS1.InteractiveGraphics.Add(cWLine[1], "disp", false);
                cogDS1.InteractiveGraphics.Add(cHLine[1], "disp", false);
                cogDS1.InteractiveGraphics.Add(marker[1], "disp", false);
            }
            catch { }

        }        

        public bool CreateLine(int Camno, double dDataX, double dDataY, double dDataR, ref CogLine cogBaseLine)
        {
            try
            {
                CogLine LineDraw = new CogLine();

                CogCreateLineTool cogCreateLine = new CogCreateLineTool();

                if (Camno == 0) cogCreateLine.InputImage = cogDS2.Image;
                else cogCreateLine.InputImage = cogDS1.Image;

                cogCreateLine.Line.X = dDataX;
                cogCreateLine.Line.Y = dDataY;
                cogCreateLine.Line.Rotation = dDataR;
                cogCreateLine.Run();
                LineDraw = cogCreateLine.GetOutputLine();
                cogBaseLine = LineDraw;

                LineDraw.Color = CogColorConstants.Green;
                if (Camno == 0) cogDS2.InteractiveGraphics.Add(LineDraw, "Line", false);
                else cogDS1.InteractiveGraphics.Add(LineDraw, "Line", false);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool PointToLine(int Cammno, double dDataX, double dDataY, CogLine cogBaseLine, ref double dDataLineX, ref double dDataLineY, ref double dDistanceY)
        {
            try
            {
                CogLine LineDraw = new CogLine();

                CogDistancePointLineTool cogPointToLine = new CogDistancePointLineTool();
                if (Cammno == 0) cogPointToLine.InputImage = cogDS2.Image;
                else cogPointToLine.InputImage = cogDS1.Image;
                cogPointToLine.X = dDataX;
                cogPointToLine.Y = dDataY;
                cogPointToLine.Line = cogBaseLine;
                cogPointToLine.Run();

                dDataLineX = cogPointToLine.LineX;
                dDataLineY = cogPointToLine.LineY;

                dDistanceY = cogPointToLine.Distance;

                LineDraw.SetFromStartXYEndXY(dDataX, dDataY, dDataLineX, dDataLineY);
                LineDraw.Color = CogColorConstants.Magenta;
                if (Cammno == 0) cogDS2.InteractiveGraphics.Add(LineDraw, "Line", false);
                else cogDS1.InteractiveGraphics.Add(LineDraw, "Line", false);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool PointToPoint(int Camno, double dDataStartX, double dDataStartY, double dDataEndX, double dDataEndY, ref double DistanceX)
        {
            try
            {
                CogLine LineDraw = new CogLine();

                CogDistancePointPointTool cogPointToPoint = new CogDistancePointPointTool();
                if (Camno == 0) cogPointToPoint.InputImage = cogDS2.Image;
                else cogPointToPoint.InputImage = cogDS1.Image;
                cogPointToPoint.StartX = dDataStartX;
                cogPointToPoint.StartY = dDataStartY;
                cogPointToPoint.EndX = dDataEndX;
                cogPointToPoint.EndY = dDataEndY;
                cogPointToPoint.Run();
                DistanceX = cogPointToPoint.Distance;

                LineDraw.SetFromStartXYEndXY(dDataStartX, dDataStartY, dDataEndX, dDataEndY);
                LineDraw.Color = CogColorConstants.Orange;
                if (Camno == 0) cogDS2.InteractiveGraphics.Add(LineDraw, "Line", false);
                else cogDS1.InteractiveGraphics.Add(LineDraw, "Line", false);

                return true;
            }
            catch
            {
                return false;
            }
        }


        //private void btnOK_Click(object sender, EventArgs e)
        //{
        //    //if (MessageBox.Show("[OK] Are you Sure", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
        //   // {
        //        if (!bendingPre)
        //        {
        //            Bending.Menu.frmAutoMain.pcResult[pcReply.Upper1Insp] = (int)ePCResult.OK;
        //            Bending.Menu.frmAutoMain.PanelShiftCheck = false;
        //            Bending.Menu.frmAutoMain.PanelShiftCheckPopup = false;
        //        }
        //        //else if (bendingPre)
        //        //{
        //        //    if (bendPreCnt == 0)
        //        //    {
        //        //        Bending.Menu.frmAutoMain.pcResult[CONST.AAM_PLC1.Reply.pcBendPreAlignReply] = (int)ePCResult.OK;
        //        //        //Bending.Menu.frmAutoMain.BendingPreInspCheck1 = false;
        //        //        //Bending.Menu.frmAutoMain.BendingPreInspCheckPopup1 = false;
        //        //    }
        //        //    else
        //        //    {
        //        //        Bending.Menu.frmAutoMain.pcResult[CONST.AAM_PLC1.Reply.pcBendPreAlignReply] = (int)ePCResult.OK;
        //        //        //Bending.Menu.frmAutoMain.BendingPreInspCheck2 = false;
        //        //        //Bending.Menu.frmAutoMain.BendingPreInspCheckPopup2 = false;
        //        //    }
                    
        //        //}
        //        Visible = false;
        //    //}
        //}

        //private void btnNG_Click(object sender, EventArgs e)
        //{
        //  //  if (MessageBox.Show("[NG] Are you Sure", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
        //  //  {
        //        if (!bendingPre)
        //        {
        //            Bending.Menu.frmAutoMain.pcResult[pcReply.Upper1Insp] = (int)ePCResult.WORKER_BY_PASS;
        //            Bending.Menu.frmAutoMain.PanelShiftCheck = false;
        //            Bending.Menu.frmAutoMain.PanelShiftCheckPopup = false;
        //        }
        //        //else if (bendingPre)
        //        //{
        //        //    if (bendPreCnt == 0)
        //        //    {
        //        //        Bending.Menu.frmAutoMain.pcResult[CONST.AAM_PLC1.Reply.pcBendPreAlignReply] = (int)ePCResult.WORKER_BY_PASS;
        //        //        //Bending.Menu.frmAutoMain.BendingPreInspCheck1 = false;
        //        //        //Bending.Menu.frmAutoMain.BendingPreInspCheckPopup1 = false;
        //        //    }
        //        //    else
        //        //    {
        //        //        Bending.Menu.frmAutoMain.pcResult[CONST.AAM_PLC1.Reply.pcBendPreAlignReply] = (int)ePCResult.WORKER_BY_PASS;
        //        //        //Bending.Menu.frmAutoMain.BendingPreInspCheck2 = false;
        //        //        //Bending.Menu.frmAutoMain.BendingPreInspCheckPopup2 = false;
        //        //    }
                    
        //        //}
        //        Visible = false;
        // //   }
        //}       
        
    }
}
