using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bending
{
    public partial class frmMessage : Form
    {
        public frmMessage()
        {
            InitializeComponent();
        }

        public int calNo1;
        public int calNo2;
        public rs2DAlign.cs2DAlign.ptXY pixelRefPoint1 = new rs2DAlign.cs2DAlign.ptXY();
        public rs2DAlign.cs2DAlign.ptXY pixelTargetPoint1 = new rs2DAlign.cs2DAlign.ptXY();
        public rs2DAlign.cs2DAlign.ptXY pixelRefPoint2 = new rs2DAlign.cs2DAlign.ptXY();
        public rs2DAlign.cs2DAlign.ptXY pixelTargetPoint2 = new rs2DAlign.cs2DAlign.ptXY();
        public rs2DAlign.cs2DAlign.ptXY pixelCnt1 = new rs2DAlign.cs2DAlign.ptXY();
        public rs2DAlign.cs2DAlign.ptXY pixelCnt2 = new rs2DAlign.cs2DAlign.ptXY();

        public void ShowMessage(string strMessage, bool ZigSet = false)
        {
            lblMessage.Text = strMessage;
            lblMessage.ForeColor = Color.Blue;

            if (ZigSet)
            {
                this.Size = new Size(968, 354);
                txtResolutionLX.Text = "";
                txtResolutionLY.Text = "";
                txtResolutionRX.Text = "";
                txtResolutionRY.Text = "";
            }
            else
            {
                this.Size = new Size(968, 129);
            }
        }

        private void btnCalibration_Click(object sender, EventArgs e)
        {
            double markTomark = 0;
            rs2DAlign.cs2DAlign.ptXXYY jigLength = new rs2DAlign.cs2DAlign.ptXXYY();
            try
            {
                markTomark = double.Parse(txtMarkToMark.Text);
                jigLength.X1 = double.Parse(tb_MeasureLX.Text);
                jigLength.Y1 = double.Parse(tb_MeasureLY.Text);
                jigLength.X2 = double.Parse(tb_MeasureRX.Text);
                jigLength.Y2 = double.Parse(tb_MeasureRY.Text);
            }
            catch
            {
                MessageBox.Show("Confirm Input Value!");                
                return;
            }
            if (markTomark > 0 && jigLength.X1 > 0 && jigLength.X2 > 0 && jigLength.Y1 > 0 && jigLength.Y2 > 0)
            {
                //Bending.Menu.rsAlign.setRefresolution = 0.01;
                rs2DAlign.cs2DAlign.ptXXYY resolution = Bending.Menu.rsAlign.setCalibration_notUseCal(calNo1, pixelRefPoint1, pixelTargetPoint1, pixelCnt1, markTomark, calNo2, pixelRefPoint2, pixelTargetPoint2, pixelCnt2, jigLength, false);
                txtResolutionLX.Text = resolution.X1.ToString("0.00000");
                txtResolutionLY.Text = resolution.Y1.ToString("0.00000");
                txtResolutionRX.Text = resolution.X2.ToString("0.00000");
                txtResolutionRY.Text = resolution.Y2.ToString("0.00000");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Calibration Value Save?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                double markTomark = 0;
                rs2DAlign.cs2DAlign.ptXXYY jigLength = new rs2DAlign.cs2DAlign.ptXXYY();
                try
                {
                    markTomark = double.Parse(txtMarkToMark.Text);
                    jigLength.X1 = double.Parse(tb_MeasureLX.Text);
                    jigLength.Y1 = double.Parse(tb_MeasureLY.Text);
                    jigLength.X2 = double.Parse(tb_MeasureRX.Text);
                    jigLength.Y2 = double.Parse(tb_MeasureRY.Text);
                }
                catch
                {
                    MessageBox.Show("Confirm Input Value!");
                    return;
                }
                if (markTomark > 0 && jigLength.X1 > 0 && jigLength.X2 > 0 && jigLength.Y1 > 0 && jigLength.Y2 > 0)
                {
                    rs2DAlign.cs2DAlign.ptXXYY resolution = Bending.Menu.rsAlign.setCalibration_notUseCal(calNo1, pixelRefPoint1, pixelTargetPoint1, pixelCnt1, markTomark, calNo2, pixelRefPoint2, pixelTargetPoint2, pixelCnt2, jigLength);
                    txtResolutionLX.Text = resolution.X1.ToString("0.00000");
                    txtResolutionLY.Text = resolution.Y1.ToString("0.00000");
                    txtResolutionRX.Text = resolution.X2.ToString("0.00000");
                    txtResolutionRY.Text = resolution.Y2.ToString("0.00000");

                    //저장 추가
                    //if (CONST.PCName == "AAM_PC2")
                    //{
                    //    //Bending.Menu.frmAutoMain.Vision[CONST.PCInsp.Vision_No.vsInspection1].CFG.FOVX = Convert.ToDouble((Math.Abs(pixelCnt1.X * resolution.X1)).ToString("0.000"));
                    //    //Bending.Menu.frmAutoMain.Vision[CONST.PCInsp.Vision_No.vsInspection1].CFG.FOVY = Convert.ToDouble((Math.Abs(pixelCnt1.Y * resolution.Y1)).ToString("0.000"));
                    //    //Bending.Menu.frmAutoMain.Vision[CONST.PCInsp.Vision_No.vsInspection2].CFG.FOVX = Convert.ToDouble((Math.Abs(pixelCnt2.X * resolution.X2)).ToString("0.000"));
                    //    //Bending.Menu.frmAutoMain.Vision[CONST.PCInsp.Vision_No.vsInspection2].CFG.FOVY = Convert.ToDouble((Math.Abs(pixelCnt2.Y * resolution.Y2)).ToString("0.000"));

                    //    //Bending.Menu.frmAutoMain.Vision[CONST.PCInsp.Vision_No.vsInspection1].CFG.Resolution = Convert.ToDouble((Math.Abs((resolution.X1 + resolution.Y1) / 2)).ToString("0.00000"));
                    //    //Bending.Menu.frmAutoMain.Vision[CONST.PCInsp.Vision_No.vsInspection2].CFG.Resolution = Convert.ToDouble((Math.Abs((resolution.X2 + resolution.Y2) / 2)).ToString("0.000000"));

                    //    //Bending.Menu.Config.CAMconfig_Write(Bending.Menu.frmAutoMain.Vision[CONST.PCInsp.Vision_No.vsInspection1].CFG, CONST.PCInsp.Vision_No.vsInspection1);
                    //    //Bending.Menu.Config.CAMconfig_Write(Bending.Menu.frmAutoMain.Vision[CONST.PCInsp.Vision_No.vsInspection2].CFG, CONST.PCInsp.Vision_No.vsInspection2);
                    //}
                    //else if (CONST.PCName == "AAM_PC1")
                    //{
                    //    //Bending.Menu.frmAutoMain.Vision[CONST.AAM_PC1.Vision_No.vsSCFPanel1].CFG.FOVX = Convert.ToDouble((Math.Abs(pixelCnt1.X * resolution.X1)).ToString("0.0000"));
                    //    //Bending.Menu.frmAutoMain.Vision[CONST.AAM_PC1.Vision_No.vsSCFPanel1].CFG.FOVY = Convert.ToDouble((Math.Abs(pixelCnt1.Y * resolution.Y1)).ToString("0.000"));
                    //    //Bending.Menu.frmAutoMain.Vision[CONST.AAM_PC1.Vision_No.vsSCFPanel2].CFG.FOVX = Convert.ToDouble((Math.Abs(pixelCnt2.X * resolution.X2)).ToString("0.000"));
                    //    //Bending.Menu.frmAutoMain.Vision[CONST.AAM_PC1.Vision_No.vsSCFPanel2].CFG.FOVY = Convert.ToDouble((Math.Abs(pixelCnt2.Y * resolution.Y2)).ToString("0.000"));

                    //    //Bending.Menu.frmAutoMain.Vision[CONST.AAM_PC1.Vision_No.vsSCFPanel1].CFG.Resolution = Convert.ToDouble((Math.Abs((resolution.X1 + resolution.Y1) / 2)).ToString("0.000000"));
                    //    //Bending.Menu.frmAutoMain.Vision[CONST.AAM_PC1.Vision_No.vsSCFPanel2].CFG.Resolution = Convert.ToDouble((Math.Abs((resolution.X2 + resolution.Y2) / 2)).ToString("0.000000"));

                    //    //Bending.Menu.Config.CAMconfig_Write(Bending.Menu.frmAutoMain.Vision[CONST.AAM_PC1.Vision_No.vsSCFPanel1].CFG, CONST.AAM_PC1.Vision_No.vsSCFPanel1);
                    //    //Bending.Menu.Config.CAMconfig_Write(Bending.Menu.frmAutoMain.Vision[CONST.AAM_PC1.Vision_No.vsSCFPanel2].CFG, CONST.AAM_PC1.Vision_No.vsSCFPanel2);
                    //}
                    
                }
            }

            this.Visible = false;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }
    }
}
