using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.PMAlign;
using rs2DAlign;
using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace Bending
{
    public partial class frmManualWindow : Form
    {
        public frmManualWindow()
        {
            InitializeComponent();
            Vision = new csVision[2];
        }
        
        int PCresultAddress;
        short VisionNo;
        csVision[] Vision; //참조로 가져와서 여러군데 활용
        public void setImg(int pcresultaddress, short visionno, params csVision[] vision)
        {
            //eManualWindow에서 번호 참고하기
            //좌우측 중 어느쪽이 ng인지 알고있어야함..(DL에 ng인것만 이미지 처리해야하니까..) bmWindowNG참고
            CheckForIllegalCrossThreadCalls = false;
            //cogDS2.Image = Limg;
            //cogDS1.Image = Rimg;
            this.PCresultAddress = pcresultaddress;
            this.VisionNo = visionno;
            //this.Vision = vision;

            //참조로 가져오기.. 좌우측 분리필요..
            if (vision.Length == 1)
            {
                Bending.Menu.frmAutoMain.setVision(ref this.Vision[0], vision[0].CFG.Camno);
                Vision[0].DispChange(cogDS1);
            }
            else //vision.Length == 2
            {
                Bending.Menu.frmAutoMain.setVision(ref this.Vision[0], vision[0].CFG.Camno);
                Bending.Menu.frmAutoMain.setVision(ref this.Vision[1], vision[1].CFG.Camno);
                Vision[0].DispChange(cogDS1);
                Vision[1].DispChange(cogDS2);
            }

            //if (vision[0].CFG.CalType == eCalType.Cam1Cal2 || vision[0].CFG.CalType == eCalType.Cam1Type)
            //{
            //    Bending.Menu.frmAutoMain.setVision(ref this.Vision[0], vision[0].CFG.Camno);
            //    Vision[0].DispChange(cogDS1);
            //}
            //else
            //{
            //    Bending.Menu.frmAutoMain.setVision(ref this.Vision[0], vision[0].CFG.Camno);
            //    Bending.Menu.frmAutoMain.setVision(ref this.Vision[1], vision[1].CFG.Camno);
            //    Vision[0].DispChange(cogDS1);
            //    Vision[1].DispChange(cogDS2);
            //}

            //특정한 상황에서만 retry버튼 보이기
            btnRetry.Visible = false;
            
            lbTitle.Text = Vision[0].CFG.Name + " Manual Window";
            Visible = true;
            pnDataAndSpec.Visible = false;
            Vision[0].Capture(false);
            Vision[1].Capture(false);
            btnMeasure_Click(null, null);

            //Bending.Menu.frmAutoMain.bManualBendingPopup[visionno] = false; //팝업 후 false
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

        private void button_Grab1_Click(object sender, EventArgs e)
        {
            cogDS2.InteractiveGraphics.Clear();
            Vision[1].Capture(false);
            //Grab(0);
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
            cogDS1.InteractiveGraphics.Clear();
            Vision[0].Capture(false);
            //Grab(1);
        }        

        private void Close_Click(object sender, EventArgs e)
        {
            //그냥 닫아버리면 무언정지임
            //if (MessageBox.Show("Are you Close", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            //{
                Visible = false;
            //}
        }

        private void btnMeasure_Click(object sender, EventArgs e)
        {
            cs2DAlign.ptXXYY dist = new cs2DAlign.ptXXYY();
            //cs2DAlign.ptXYT ref1Pixel = new cs2DAlign.ptXYT();
            //cs2DAlign.ptXYT mark1Pixel = new cs2DAlign.ptXYT();
            //cs2DAlign.ptXYT ref2Pixel = new cs2DAlign.ptXYT();
            //cs2DAlign.ptXYT mark2Pixel = new cs2DAlign.ptXYT();

            //csVision.sideResult sResult1 = new csVision.sideResult();
            //csVision.sideResult sResult2 = new csVision.sideResult();

            //switch (Vision[0].CFG.eCamName)
            //{
                
            //}
            lbDataLX.Text = dist.X1.ToString("0.000");
            lbDataLY.Text = dist.Y1.ToString("0.000");
            lbDataRX.Text = dist.X2.ToString("0.000");
            lbDataRY.Text = dist.Y2.ToString("0.000");
        }
        #region(구)
        //private void Grab(int dispNO)
        //{
        //    try
        //    {
        //        if (dispNO == 0)
        //        {
        //            cogDS1.InteractiveGraphics.Remove("Inspection1");
        //            cogDS1.InteractiveGraphics.Remove("Inspection2");
        //        }
        //        else if (dispNO == 1)
        //        {
        //            cogDS2.InteractiveGraphics.Remove("Inspection3");
        //            cogDS2.InteractiveGraphics.Remove("Inspection4");
        //        }
        //    }
        //    catch
        //    {
        //    }


        //    CogImage8Grey cogGrab = new CogImage8Grey();
        //    int VisionNo = Vision_No.Bend1_1;
        //    //if (ManualNo == 1) VisionNo = Vision_No.vsBend2_1;
        //    //else if (ManualNo == 2) VisionNo = Vision_No.vsBend3_1;

        //    if (dispNO == 1)
        //    {
        //        VisionNo = Vision_No.Bend1_2;
        //        //if (ManualNo == 1) VisionNo = Vision_No.vsBend2_2;
        //        //else if (ManualNo == 2) VisionNo = Vision_No.vsBend3_2;
        //    }
        //    Bending.Menu.frmAutoMain.Vision[VisionNo].Grab(ref cogGrab);
        //    if (dispNO == 0)
        //    {
        //        cogDS1.Image = cogGrab;
        //    }
        //    else if (dispNO == 1) cogDS2.Image = cogGrab;
        //}
        //private void BendingMeasureMent(int ManualNo, short visionno)
        //{
        //    cs2DAlign.ptXXYY dist = new cs2DAlign.ptXXYY();
        //    BendingInspection(ManualNo, visionno, ref dist);

        //    cogDS1.Image = Bending.Menu.frmAutoMain.Vision[visionno + 0].cogDS.Image;
        //    cogDS2.Image = Bending.Menu.frmAutoMain.Vision[visionno + 1].cogDS.Image;
        //    lbDataLX.Text = dist.X1.ToString("0.000");
        //    lbDataLY.Text = dist.Y1.ToString("0.000");
        //    lbDataRX.Text = dist.X2.ToString("0.000");
        //    lbDataRY.Text = dist.Y2.ToString("0.000");
        //    cogDS1.Fit(true);
        //    cogDS2.Fit(true);
        //}
        //private void BendingInspection(int BendingNo, int VisionNo, ref cs2DAlign.ptXXYY dist)
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

        //    dist.X1 = 0;
        //    dist.X2 = 0;
        //    dist.Y1 = 0;
        //    dist.Y2 = 0;

        //    Grab(0);
        //    Grab(1);
        //    //여기

        //    result1 = Bending.Menu.frmAutoMain.Vision[VisionNo + 0].PatternSearchEnum(ref dRefX1, ref dRefY1, ref dRefR, ePatternKind.Panel);
        //    result2 = Bending.Menu.frmAutoMain.Vision[VisionNo + 1].PatternSearchEnum(ref dRefX2, ref dRefY2, ref dRefR, ePatternKind.Panel);
        //    if (result1 && result2)
        //    {
        //        if (Bending.Menu.frmAutoMain.Vision[VisionNo + 0].PatternSearchEnum(ref markX1, ref markY1, ref dRefR, ePatternKind.FPC)
        //            && Bending.Menu.frmAutoMain.Vision[VisionNo + 1].PatternSearchEnum(ref markX2, ref markY2, ref dRefR, ePatternKind.FPC))
        //        {

        //            cs2DAlign.ptXY ref1 = new cs2DAlign.ptXY();
        //            cs2DAlign.ptXY mark1 = new cs2DAlign.ptXY();
        //            cs2DAlign.ptXY ref2 = new cs2DAlign.ptXY();
        //            cs2DAlign.ptXY mark2 = new cs2DAlign.ptXY();

        //            ref1.X = dRefX1;
        //            ref1.Y = dRefY1;
        //            mark1.X = markX1;
        //            mark1.Y = markY1;

        //            ref2.X = dRefX2;
        //            ref2.Y = dRefY2;
        //            mark2.X = markX2;
        //            mark2.Y = markY2;

        //            eCalPos calpos = eCalPos.Bend1_1Arm;

        //            if (BendingNo == 0)
        //            {
        //                calpos = eCalPos.Bend1_1Arm;
        //            }
        //            //else if (ManualNo == 1)
        //            //{
        //            //    calpos = eCalPos.Bend2Arm_L;
        //            //}
        //            //else if (ManualNo == 2)
        //            //{
        //            //    calpos = eCalPos.Bend3Arm_L;
        //            //}

        //            UpperInspectionREV(calpos, true, ref1, mark1, ref2, mark2, ref dist);
        //        }
        //        else
        //        {
        //           // MessageBox.Show("FPC Mark Search Fail");

        //        }
        //    }
        //    else
        //    {
        //      //  MessageBox.Show("Panel Mark Search Fail");
        //    }
        //}

        //public void UpperInspectionREV(eCalPos calNo, bool display, cs2DAlign.ptXY ref1XY, cs2DAlign.ptXY Mark1XY, cs2DAlign.ptXY ref2XY, cs2DAlign.ptXY Mark2XY, ref cs2DAlign.ptXXYY dist)
        //{
        //    //20.10.07 lkw
        //    dist = Bending.Menu.rsAlign.getDistXY((int)calNo, ref1XY, Mark1XY, (int)calNo + 1, ref2XY, Mark2XY);

        //    if (display)
        //    {
        //        Inspection(0, "Inspection1", ref1XY.X, ref1XY.X, Mark1XY.Y, ref1XY.Y, Math.Abs(dist.Y1));
        //        Inspection(0, "Inspection2", ref1XY.X, Mark1XY.X, Mark1XY.Y, Mark1XY.Y, Math.Abs(dist.X1));

        //        Inspection(1, "Inspection3", ref2XY.X, ref2XY.X, Mark2XY.Y, ref2XY.Y, Math.Abs(dist.Y2));
        //        Inspection(1, "Inspection4", ref2XY.X, Mark2XY.X, Mark2XY.Y, Mark2XY.Y, Math.Abs(dist.X2));
        //    }
        //}
        #endregion
        public void Display(cs2DAlign.ptXYT ref1XY, cs2DAlign.ptXYT Mark1XY, cs2DAlign.ptXYT ref2XY, cs2DAlign.ptXYT Mark2XY, cs2DAlign.ptXXYY dist)
        {
            DrawGraphics(0, "Inspection1", ref1XY.X, ref1XY.X, Mark1XY.Y, ref1XY.Y, Math.Abs(dist.Y1));
            DrawGraphics(0, "Inspection2", ref1XY.X, Mark1XY.X, Mark1XY.Y, Mark1XY.Y, Math.Abs(dist.X1));

            DrawGraphics(1, "Inspection3", ref2XY.X, ref2XY.X, Mark2XY.Y, ref2XY.Y, Math.Abs(dist.Y2));
            DrawGraphics(1, "Inspection4", ref2XY.X, Mark2XY.X, Mark2XY.Y, Mark2XY.Y, Math.Abs(dist.X2));
        }

        public void DrawGraphics(int Camno, string sKind, double X1, double X2, double Y1, double Y2, double dDist)
        {
            //그리는거 확인필요
            try
            {
                if (Camno == 0) cogDS1.InteractiveGraphics.Remove(sKind);
                else cogDS2.InteractiveGraphics.Remove(sKind);
            }
            catch { }

            CogLineSegment cogLine = new CogLineSegment();

            double tempX = X1;
            double tempY = Y1;

            double tempX2 = X2;
            double tempY2 = Y2;

            
            X1 = tempX;
            Y1 = tempY;

            cogLine.StartX = X1;
            cogLine.StartY = Y1;
            cogLine.EndX = X2;
            cogLine.EndY = Y2;

            cogLine.Color = CogColorConstants.Yellow;
            cogLine.LineWidthInScreenPixels = 3;
            if (Camno == 0) cogDS2.InteractiveGraphics.Add(cogLine, sKind, false);
            else cogDS1.InteractiveGraphics.Add(cogLine, sKind, false);
           
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            //switch (Vision[0].CFG.eCamName)
            //{
                
            //}
            //if (MessageBox.Show("[OK] Are you Close", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            //{
            Bending.Menu.frmAutoMain.pcResult[PCresultAddress] = (int)ePCResult.OK;
                //Bending.Menu.frmAutoMain.bManualBending[VisionNo] = Vision[0].CFG.ManualWindow;
                Bending.Menu.frmAutoMain.bManualBendingPopup[VisionNo] = false;
                Visible = false;
            //}
        }

        private void btnNG_Click(object sender, EventArgs e)
        {
            //switch(Vision[0].CFG.eCamName)
            //{
                
            //}

            //if (MessageBox.Show("[NG] Are you Close", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            //{
            Bending.Menu.frmAutoMain.pcResult[PCresultAddress] = (int)ePCResult.WORKER_BY_PASS;
                //Bending.Menu.frmAutoMain.bManualBending[VisionNo] = Vision[0].CFG.ManualWindow;
                Bending.Menu.frmAutoMain.bManualBendingPopup[VisionNo] = false;
                Visible = false;
            //}
        }

        private void btnRetry_Click(object sender, EventArgs e)
        {
            //if (MessageBox.Show("[RETRY] Are you Close", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            //{
                Bending.Menu.frmAutoMain.pcResult[PCresultAddress] = (int)ePCResult.RETRY;
                //Bending.Menu.frmAutoMain.bManualBending[VisionNo] = Bending.Menu.frmSetting.revData.mBendingArm.bManualBendingUse;
                //Bending.Menu.frmAutoMain.bManualBending[VisionNo] = Vision[0].CFG.ManualWindow;
                Bending.Menu.frmAutoMain.bManualBendingPopup[VisionNo] = false;
                Visible = false;
            //}
        }
        private void SetDLRegionAll()
        {
            //point 및 영역을 미리 지정하지 않았을때 전영역을 사용하기 위함
            if (Vision[0].CFG.CalType == eCalType.Cam1Cal2 || Vision[0].CFG.CalType == eCalType.Cam1Type)
            {
                Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[0].PointX = cogDS1.Image.Width / 2;
                Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[0].PointY = cogDS1.Image.Height / 2;
                Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[0].ImgWidth = cogDS1.Image.Width;
                Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[0].ImgHeight = cogDS1.Image.Height;
            }
            else
            {
                Bending.Menu.frmAutoMain.Vision[VisionNo + 1].searchDatas[0].PointX = cogDS2.Image.Width / 2;
                Bending.Menu.frmAutoMain.Vision[VisionNo + 1].searchDatas[0].PointY = cogDS2.Image.Height / 2;
                Bending.Menu.frmAutoMain.Vision[VisionNo + 1].searchDatas[0].ImgWidth = cogDS2.Image.Width;
                Bending.Menu.frmAutoMain.Vision[VisionNo + 1].searchDatas[0].ImgHeight = cogDS2.Image.Height;
            }
        }
        private void DLImageTask()
        {
            //manual window이미지 저장시에 기존 마크 주변을 동일하게 사용..
            //point가 0이 아닌걸로 데이터가 있는지 확인
            if (Vision[0].bMWindowNG && Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[0].PointX > 0)
            {
                //pcy210121 DL이미지 저장 추가 왼쪽
                DLImageSave("NG", 0,
                    Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[0].PointX,
                    Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[0].PointY,
                    Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[0].ImgWidth,
                    Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[0].ImgHeight,
                    cogDS1.Image.ToBitmap());
                DLImageSave("OK", 0,
                    Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[0].PointX,
                    Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[0].PointY,
                    Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[0].ImgWidth,
                    Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[0].ImgHeight,
                    Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[0].ImageMemory.ToBitmap());
            }
            if (Vision[1].bMWindowNG && Bending.Menu.frmAutoMain.Vision[VisionNo + 1].searchDatas[0].PointX > 0)
            {
                //pcy210121 DL이미지 저장 추가 오른쪽
                DLImageSave("NG", 0,
                    Bending.Menu.frmAutoMain.Vision[VisionNo + 1].searchDatas[0].PointX,
                    Bending.Menu.frmAutoMain.Vision[VisionNo + 1].searchDatas[0].PointY,
                    Bending.Menu.frmAutoMain.Vision[VisionNo + 1].searchDatas[0].ImgWidth,
                    Bending.Menu.frmAutoMain.Vision[VisionNo + 1].searchDatas[0].ImgHeight,
                    cogDS2.Image.ToBitmap());
                DLImageSave("OK", 0,
                    Bending.Menu.frmAutoMain.Vision[VisionNo + 1].searchDatas[0].PointX,
                    Bending.Menu.frmAutoMain.Vision[VisionNo + 1].searchDatas[0].PointY,
                    Bending.Menu.frmAutoMain.Vision[VisionNo + 1].searchDatas[0].ImgWidth,
                    Bending.Menu.frmAutoMain.Vision[VisionNo + 1].searchDatas[0].ImgHeight,
                    Bending.Menu.frmAutoMain.Vision[VisionNo + 1].searchDatas[0].ImageMemory.ToBitmap());
            }
        }
        private void DLImageSave(string okng, int no, double dx, double dy, double width, double height, Bitmap bitmap)
        {
            if (Bending.Menu.frmSetting.revData.mDL[VisionNo].DefectFind_ModelPath[no] == null
                || Bending.Menu.frmSetting.revData.mDL[VisionNo].DefectFind_ModelPath[no] == "")
            {
                return; //아무것도 안함.
            }
            string trainpath = Path.GetFileNameWithoutExtension(Bending.Menu.frmSetting.revData.mDL[VisionNo].DefectFind_ModelPath[no]);
            Bending.Menu.rsDL.TrainImageSetStart(trainpath);

            Point point = new Point(Convert.ToInt32(dx), Convert.ToInt32(dy));
            //210118 cjm DL이미지 저장시 마크 크기 정하기
            Point size = new Point((int)width, (int)height);

            Bending.Menu.rsDL.TrainImageSet(bitmap, point, okng, size);
        }
        private void frmManualBending_VisibleChanged(object sender, EventArgs e)
        {
            if(Visible == false) Bending.Menu.frmAutoMain.InitialDisp(); //끝나고 display 복원
        }
    }
}
