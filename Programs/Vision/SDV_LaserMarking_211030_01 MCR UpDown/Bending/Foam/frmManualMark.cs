using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.PMAlign;
using rs2DAlign;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Bending
{
    public partial class frmManualMark : Form
    {

        public frmManualMark(csVision.sCFG _cfg, int _visionno)
        {
            InitializeComponent();
            CFG = _cfg;
            VisionNo = _visionno;
        }


        csVision.sCFG CFG;
        int VisionNo;
        csLog cLog = new csLog();

        public double markX;
        public double markY;
        public double refX;
        public double refY;

        public bool m_bMouseClick1 = false;
        public bool m_bMouseClick2 = false;

        //csh 20170601
        //public int m_nBitNo;
        public int m_nLimitTimeCount;
        public int m_nLimitTime = 20;

        public bool m_bMarkRun = false;
        public bool m_bRefMarkRun = false;

        bool EdgeAlignMode = false; //설정 필요

        //210118 DL이미지 저장시 마크 크기 정하기
        int iMarkWidth = 0;
        int iMarkHeight = 0;

        //csh 20170601
        public void setImg(Cognex.VisionPro.ICogImage img, string camName, bool LCheck = false)
        {
            //string ManualImgPath = "C:\\EQData\\INI";
            CheckForIllegalCrossThreadCalls = false;
            btnConfirm1.Enabled = false;
            btnConfirm2.Enabled = false;
            cogDS1.Enabled = false;
            cogDS2.Enabled = false;
            m_nLimitTimeCount = 0;
            //VisionNo = visionNo;
            //m_nBitNo = nBitNo;
            lbTitle.Text = CFG.Name.Trim() + " Manual Search " + pcresult.ToString();
            if (bLeft) lbLeftMark.Text = "Left Mark";
            else lbLeftMark.Text = "Panel Mark";
            if (bRight) lbRightMark.Text = "Right Mark";
            else lbRightMark.Text = "FPC Mark";


            tmrDisp.Enabled = LCheck;
            lbLCheck.Visible = LCheck;


            //Pattern 왼쪽 REF가 오른쪽 
            //if (EdgeAlignMode && !MarkInfo[mFPC].bDone)
            //{ lbTitle.Text += "Ref!"; }
            //if (EdgeAlignMode && !MarkInfo[mPanel].bDone)
            //{ lbTitle.Text += "Mark!"; }

            if (EdgeAlignMode)
            {
                cbCreateLine1.Checked = true;
                cbCreateLine2.Checked = true;
            }
            else
            {
                cbCreateLine1.Checked = false;
                cbCreateLine2.Checked = false;
            }

            m_bMouseClick1 = false;
            m_bMouseClick2 = false;

            cogDS1.Image = null;
            cogDS2.Image = null;

            string sPath = Path.Combine(CONST.cVisionPath, camName, CONST.stringRcp, CONST.RunRecipe.RecipeName, "PMAlign");

            if (!MarkInfo[mPanel].bDone || !MarkInfo[mLeft].bDone)// && !Bending.Menu.frmAutoMain.Vision[visionNo].manualMark[mPanel].selected)
            {
                //Image 및  Title 수정
                //if (EdgeAlignMode)
                //{
                //    string sPath = ManualImgPath + "\\EdgeMark.jpg";
                //    pictureBox1.Image = (Bitmap)Image.FromFile(sPath);
                //    //lbTitle.Text = cfg.Name.Trim() + "Manual Edge Mark Search";
                //}
                //else
                //{
                //    string sPath = ManualImgPath + "\\Mark.jpg";
                //    pictureBox1.Image = Image.FromFile(sPath);
                //    //lbTitle.Text = cfg.Name.Trim() + " Manual Mark Search";
                //}
                string path1;
                if (bLeft) path1 = Path.Combine(sPath, MarkInfo[mLeft].patternKind.ToString());
                else path1 = Path.Combine(sPath, MarkInfo[mPanel].patternKind.ToString());


                try
                {
                    string[] sFile = Directory.GetFiles(path1, "*.vpp");
                    CogPMAlignPattern cogPMAlignPattern = new CogPMAlignPattern();
                    cogPMAlignPattern = (CogPMAlignPattern)CogSerializer.LoadObjectFromFile(sFile[0]);

                    CogMaskGraphic ImageMask = new CogMaskGraphic();
                    ImageMask.Image = cogPMAlignPattern.TrainImageMask;
                    cogDSPattern1.InteractiveGraphics.Clear();
                    cogDSPattern1.InteractiveGraphics.Add(ImageMask, "mask", false);
                    cogDSPattern1.Image = cogPMAlignPattern.TrainImage;
                    cogDSPattern1.Fit(true);
                    iMarkWidth = cogPMAlignPattern.TrainImage.Width;
                    //iMarkHeight = cogPMAlignPattern.TrainImage.Width;
                    iMarkHeight = cogPMAlignPattern.TrainImage.Height;

                }
                catch
                { }
                btnConfirm1.Enabled = true;
                cogDS1.Enabled = true;
                try
                {
                    cogDS1.Image = img;

                    //ROI Display/////
                    string[] sFile = Directory.GetFiles(path1, "*.vpp");
                    CogRectangle cogrectangle = new CogRectangle();
                    for (int i = 0; i < sFile.Length; i++)
                    {
                        string[] Split = Path.GetFileName(sFile[i]).Split('_');
                        if (Split.Length > 8)
                        {
                            cogrectangle.SetXYWidthHeight(double.Parse(Split[7]), double.Parse(Split[8]), double.Parse(Split[9]), double.Parse(Split[10]));

                            cogrectangle.GraphicDOFEnable = CogRectangleDOFConstants.All;
                            cogrectangle.Interactive = true;
                            cogDS1.InteractiveGraphics.Add(cogrectangle, "Left Mark", false); // 검색 영역 표시 부분.
                        }
                    }

                    if (camName.ToUpper() == nameof(eCamNO.Laser1).ToUpper() || camName.ToUpper() == nameof(eCamNO.Laser2).ToUpper())
                    {
                        cs2DAlign.ptXY resol = new cs2DAlign.ptXY();
                        cs2DAlign.ptXY pixelcnt = new cs2DAlign.ptXY();
                        Bending.Menu.rsAlign.getResolution((int)eCalPos.Laser1, ref resol, ref pixelcnt);
                        int visionNo = (int)eCamNO.Laser1;
                        if (camName.ToUpper() == nameof(eCamNO.Laser2).ToUpper())
                        {
                            visionNo = (int)eCamNO.Laser2;
                            Bending.Menu.rsAlign.getResolution((int)eCalPos.Laser2, ref resol, ref pixelcnt);
                        }
                            CogRectangle markRegion = new CogRectangle();
                        double centerX = Bending.Menu.frmAutoMain.Vision[visionNo].CFG.TargetX[0];
                        double centerY = Bending.Menu.frmAutoMain.Vision[visionNo].CFG.TargetY[0];
                        double width = Bending.Menu.frmSetting.revData.mSizeSpecRatio.LaserAlignPosTor / resol.X;
                        double height = Bending.Menu.frmSetting.revData.mSizeSpecRatio.LaserAlignPosTor / resol.Y;


                        markRegion.SetCenterWidthHeight(centerX, centerY, width, height);
                        markRegion.Color = CogColorConstants.Red;
                        cogDS1.InteractiveGraphics.Add(markRegion, "Left Mark", false); 
                    }

                }
                catch
                {
                }
                cogDS1.AutoFit = true;
                m_bMarkRun = true;
                //경로적고 thread로 날림.
                //Task.Run(() =>
                //{
                //    CogImageFile imageFile = new CogImageFile();
                //    imageFile.Open(@"D:\EQData\Images", CogImageFileModeConstants.Write);
                //    imageFile.Append(cogDS1.Image);
                //    imageFile.Close();
                //});
            }
            else
                m_bMarkRun = false;

            if (!MarkInfo[mFPC].bDone || !MarkInfo[mRight].bDone)
            {
                //Image 및  Title 수정
                //if (EdgeAlignMode)
                //{
                //    string sPath = ManualImgPath + "\\EdgeRef.jpg";
                //    pictureBox1.Image = (Bitmap)Image.FromFile(sPath);
                //    //lbTitle.Text = cfg.Name.Trim() + "Manual Edge Ref Mark Search";
                //}
                //else
                //{
                //    string sPath = ManualImgPath + "\\Ref.jpg";
                //    pictureBox1.Image = (Bitmap)Image.FromFile(sPath);
                //    //lbTitle.Text = cfg.Name.Trim() + " Manual Ref Mark Search";
                //}
                string path2;
                if (bRight) path2 = Path.Combine(sPath, MarkInfo[mRight].patternKind.ToString());
                else path2 = Path.Combine(sPath, MarkInfo[mFPC].patternKind.ToString());

                try
                {
                    //sPath = Path.Combine(sPath, MarkInfo[mFPC].patternKind.ToString());
                    //string[] plist = Directory.GetFiles(sPath);
                    string[] sFile = Directory.GetFiles(path2, "*.vpp");
                    CogPMAlignPattern cogPMAlignPattern = new CogPMAlignPattern();
                    cogPMAlignPattern = (CogPMAlignPattern)CogSerializer.LoadObjectFromFile(sFile[0]);
                    CogMaskGraphic ImageMask = new CogMaskGraphic();
                    ImageMask.Image = cogPMAlignPattern.TrainImageMask;
                    cogDSPattern2.InteractiveGraphics.Clear();
                    cogDSPattern2.InteractiveGraphics.Add(ImageMask, "mask", false);
                    cogDSPattern2.Image = cogPMAlignPattern.TrainImage;
                    cogDSPattern2.Fit(true);


                }
                catch  { }

                btnConfirm2.Enabled = true;
                cogDS2.Enabled = true;
                try
                {
                    cogDS2.Image = img;

                    //ROI Display/////
                    string[] sFile = Directory.GetFiles(path2, "*.vpp");
                    CogRectangle cogrectangle = new CogRectangle();
                    for (int i = 0; i < sFile.Length; i++)
                    {
                        string[] Split;

                        Split = Path.GetFileName(sFile[i]).Split('_');
                        if (Split.Length > 8)
                        {
                            cogrectangle.SetXYWidthHeight(double.Parse(Split[7]), double.Parse(Split[8]), double.Parse(Split[9]), double.Parse(Split[10]));

                            cogrectangle.GraphicDOFEnable = CogRectangleDOFConstants.All;
                            cogrectangle.Interactive = true;
                            cogDS2.InteractiveGraphics.Add(cogrectangle, "Right Mark", false); // 검색 영역 표시 부분.
                        }
                    }

                    if (camName.ToUpper() == nameof(eCamNO.Laser1).ToUpper() || camName.ToUpper() == nameof(eCamNO.Laser2).ToUpper())
                    {
                        cs2DAlign.ptXY resol = new cs2DAlign.ptXY();
                        cs2DAlign.ptXY pixelcnt = new cs2DAlign.ptXY();
                        Bending.Menu.rsAlign.getResolution((int)eCalPos.Laser1, ref resol, ref pixelcnt);
                        int visionNo = (int)eCamNO.Laser1;
                        if (camName.ToUpper() == nameof(eCamNO.Laser2).ToUpper())
                        {
                            visionNo = (int)eCamNO.Laser2;
                            Bending.Menu.rsAlign.getResolution((int)eCalPos.Laser2, ref resol, ref pixelcnt);
                        }
                        CogRectangle markRegion = new CogRectangle();
                        double centerX = Bending.Menu.frmAutoMain.Vision[visionNo].CFG.TargetX[0];
                        double centerY = Bending.Menu.frmAutoMain.Vision[visionNo].CFG.TargetY[0];
                        double width = Bending.Menu.frmSetting.revData.mSizeSpecRatio.LaserAlignPosTor / resol.X;
                        double height = Bending.Menu.frmSetting.revData.mSizeSpecRatio.LaserAlignPosTor / resol.Y;


                        markRegion.SetCenterWidthHeight(centerX, centerY, width, height);
                        markRegion.Color = CogColorConstants.Red;
                        cogDS2.InteractiveGraphics.Add(markRegion, "Left Mark", false);
                    }
                }
                catch
                { }
                cogDS2.AutoFit = true;
                m_bRefMarkRun = true;
            }
            else
                m_bRefMarkRun = false;

            Visible = true;

            //KSJ 20170804 Last만 저장
            //Bending.Menu.frmAutoMain.Vision[VisionNo].BMPImageSave(false, CFG.ImageSaveType);
        }

        // 현재 Image 상의 Map position 반환.
        // 201102 cjm 수정 초기값 0으로 설정
        class MapPosition
        {
            public double x = 0;
            public double y = 0;

            public MapPosition(double x, double y)
            {
                this.x = x;
                this.y = y;
            }
        }

        private void DisplayPoint(MapPosition pos, CogDisplay ds)
        {
            ds.InteractiveGraphics.Clear();

            float ResolutionX = (float)(CFG.FOVX / (CFG.Resolution));
            float ResolutionY = (float)(CFG.FOVY / (CFG.Resolution));

            CogLineSegment cogLine1 = new CogLineSegment();

            //CogLine cogLine1 = new CogLine();

            double lc20umPixel = ResolutionX / CFG.FOVX * 20.0;

            int size = 36;

            cogLine1.StartX = pos.x - size;
            cogLine1.StartY = pos.y;// - lc20umPixel * 3 + 1;
            cogLine1.EndX = pos.x + size;
            cogLine1.EndY = pos.y;

            //cogLine1.lin
            cogLine1.Color = CogColorConstants.Orange;

            //cogLine1.SetFromStartXYEndXY(pos.x - 12, pos.y, pos.x + 12, pos.y);

            ds.InteractiveGraphics.Add(cogLine1, "PICK", false);

            /////////////////////////////////////////

            CogLineSegment cogLine2 = new CogLineSegment();

            cogLine2.StartX = pos.x;
            cogLine2.StartY = pos.y - size;// - lc20umPixel * 3 + 1;
            cogLine2.EndX = pos.x;
            cogLine2.EndY = pos.y + size;

            //cogLine1.SetFromStartXYEndXY(pos.x, pos.y - 12, pos.x, pos.y + 12);

            cogLine2.Color = CogColorConstants.Yellow;

            ds.InteractiveGraphics.Add(cogLine2, "PICK", false);

            //for (int i = 0; i < lcTempCogLineSegment.Length; i++)
            //    this.cogDisplay1.InteractiveGraphics.Add(lcTempCogLineSegment[i], "", false);
        }

        private void cogDS_MouseDown(object sender, MouseEventArgs e)
        {
            GetMousePickerPoint(e, cogDS1, mPanel);
            m_bMouseClick1 = true;
        }


        // 화면상에 십자선을 출력.
        private void DisplayPoint(Cognex.VisionPro.Display.CogDisplay ds, MapPosition pos, csVision.sCFG config)
        {
            ds.InteractiveGraphics.Clear();

            float ResolutionX = (float)(config.FOVX / (config.Resolution));
            float ResolutionY = (float)(config.FOVY / (config.Resolution));

            CogLineSegment cogLine1 = new CogLineSegment();

            //CogLine cogLine1 = new CogLine();

            double lc20umPixel = ResolutionX / config.FOVX * 20.0;

            int size = 200;

            cogLine1.StartX = pos.x - size;
            cogLine1.StartY = pos.y;// - lc20umPixel * 3 + 1;
            cogLine1.EndX = pos.x + size;
            cogLine1.EndY = pos.y;

            //cogLine1.lin
            cogLine1.Color = CogColorConstants.Red;

            //cogLine1.SetFromStartXYEndXY(pos.x - 12, pos.y, pos.x + 12, pos.y);

            ds.InteractiveGraphics.Add(cogLine1, "PICK", false);

            /////////////////////////////////////////

            CogLineSegment cogLine2 = new CogLineSegment();

            cogLine2.StartX = pos.x;
            cogLine2.StartY = pos.y - size;// - lc20umPixel * 3 + 1;
            cogLine2.EndX = pos.x;
            cogLine2.EndY = pos.y + size;

            //cogLine1.SetFromStartXYEndXY(pos.x, pos.y - 12, pos.x, pos.y + 12);

            cogLine2.Color = CogColorConstants.Red;

            ds.InteractiveGraphics.Add(cogLine2, "PICK", false);

            //for (int i = 0; i < lcTempCogLineSegment.Length; i++)
            //    this.cogDisplay1.InteractiveGraphics.Add(lcTempCogLineSegment[i], "", false);
        }

        private void GetMousePickerPoint(MouseEventArgs e, CogDisplay ds, int index)
        {
            MapPosition pos;
            pos = GetMouseMapPosition(ds, e.X, e.Y, index);

            DisplayPoint(ds, pos, CFG);


            destX[index] = pos.x;
            destY[index] = pos.y;
        }

        double[] destX = { 0, 0 };
        double[] destY = { 0, 0 };

        private MapPosition GetMouseMapPosition(CogDisplay display, int mouseX, int mouseY, int index)
        {
            ICogTransform2D iTransPosition;
            CogCoordinateSpaceTree cogCoordinateSpaceTree;

            cogCoordinateSpaceTree = display.UserDisplayTree;

            string lcTempName = "";

            if (cogCoordinateSpaceTree == null)
                lcTempName = "pos";
            else
                lcTempName = cogCoordinateSpaceTree.RootName;

            iTransPosition = display.GetTransform("#", lcTempName);
            iTransPosition.MapPoint(Convert.ToDouble(mouseX), Convert.ToDouble(mouseY), out destX[index], out destY[index]);

            return new MapPosition(destX[index], destY[index]);
        }

        private void cogDS2_MouseDown(object sender, MouseEventArgs e)
        {
            GetMousePickerPoint(e, cogDS2, mFPC);
            m_bMouseClick2 = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you cancel", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                Visible = false;
                //Bending.Menu.frmAutoMain.pcResult[m_nBitNo] = RUN_ERROR_FIND;
            }
        }
        public const int mPanel = 0;
        public const int mFPC = 1;
        public const int mLeft = 2;
        public const int mRight = 3;
        private void button2_Click(object sender, EventArgs e)
        {
            //왼쪽이 ref //(0 : Mark, 1 : Ref)
            if (!m_bMouseClick2)
            {
                MessageBox.Show("Ref Click Fail. Retry Ref Click");
                return;
            }

            //if (MessageBox.Show("Are you sure", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            //{
            double refX = destX[mFPC];
            double refY = destY[mFPC];
            btnConfirm2.Enabled = false;

            if (bRight)
            {
                setManualMark(VisionNo, mRight, refX, refY);
                MarkInfo[mRight].bDone = true;
                bRight = false;
            }
            else
            {
                setManualMark(VisionNo, mFPC, refX, refY);
                MarkInfo[mFPC].bDone = true;

                //pcy210107 DL이미지 저장 추가
                DLImageSave("NG", mFPC, refX, refY, cogDS2.Image.ToBitmap());
                if (Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[mFPC].ImageMemory != null)
                {
                    DLImageSave("OK", mFPC, Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[mFPC].PointX,
                    Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[mFPC].PointY,
                    Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[mFPC].ImageMemory.ToBitmap());
                }
            }
            //KSJ 20170717 임시마크
            MarkInfo[mFPC].bTempMark = Bending.Menu.frmAutoMain.Vision[VisionNo].SetTempPMAlignPatternRef(refX, refY);
            //Bending.Menu.frmAutoMain.Vision[VisionNo].TempMark.m_bPattern_Ref = true;
            //MarkInfo[mFPC].manualPopup = false;
            
            //Bending.Menu.frmAutoMain.pcResult[m_nBitNo] = ePCResult.WAIT;
            //if (CONST.PCName == "AAM_PC1")
            //    Bending.Menu.frmAutoMain.pcResult[m_nBitNo] = RUN_MANAUL_RETRY_SIG;

            if (!btnConfirm1.Enabled)
            {
                // 완료
                Visible = false;
                //tmrMark.Enabled = false;
            }
            //}
        }
        public void setManualMark(int visionNo, int kind, double X, double Y)
        {
            //(0 : Mark, 1 : Ref)
            MarkInfo[kind].selected = true;
            MarkInfo[kind].selectcnt++;
            MarkInfo[kind].selectX = X;
            MarkInfo[kind].selectY = Y;
        }
        private void btnConfirm1_Click(object sender, EventArgs e)
        {
            //우측은 mark
            if (!m_bMouseClick1)
            {
                MessageBox.Show("Pattern Click Fail. Retry Pattern Click");
                return;
            }

            //if (MessageBox.Show("Are you sure", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            //{
            double markX = destX[mPanel];
            double markY = destY[mPanel];
            btnConfirm1.Enabled = false;

            if (bLeft)
            {
                setManualMark(VisionNo, mLeft, markX, markY);
                MarkInfo[mLeft].bDone = true;
                bLeft = false;
            }
            else
            {
                setManualMark(VisionNo, mPanel, markX, markY);
                MarkInfo[mPanel].bDone = true;

                //pcy210107 DL이미지 저장 추가
                DLImageSave("NG", mPanel, markX, markY, cogDS1.Image.ToBitmap());
                //DLImageSave("OK", mPanel, markX, markY, cogDS1.Image.ToBitmap()); //테스트용

                if (Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[mPanel].ImageMemory != null)
                {
                    DLImageSave("OK", mPanel, Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[mPanel].PointX,
                        Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[mPanel].PointY,
                        Bending.Menu.frmAutoMain.Vision[VisionNo].searchDatas[mPanel].ImageMemory.ToBitmap());
                }

            }
            //KSJ 20170717 임시마크
            MarkInfo[mPanel].bTempMark = Bending.Menu.frmAutoMain.Vision[VisionNo].SetTempPMAlignPattern(markX, markY);
            
            
            //Bending.Menu.frmAutoMain.pcResult[m_nBitNo] = ePCResult.WAIT;

            if (!btnConfirm2.Enabled)
            {
                // 완료
                Visible = false;
                //tmrMark.Enabled = false;
            }
            //}
        }
        private void DLImageSave(string okng, int no, double dx, double dy, Bitmap bitmap)
        {
            if(Bending.Menu.frmSetting.revData.mDL[VisionNo].MarkSearch_ModelPath[no] == null
                || Bending.Menu.frmSetting.revData.mDL[VisionNo].MarkSearch_ModelPath[no] == "")
            {
                return; //아무것도 안함.
            }
            string trainpath = Path.GetFileNameWithoutExtension(Bending.Menu.frmSetting.revData.mDL[VisionNo].MarkSearch_ModelPath[no]);
            Bending.Menu.rsDL.TrainImageSetStart(trainpath);

            Point point = new Point(Convert.ToInt32(dx), Convert.ToInt32(dy));
            //210118 cjm DL이미지 저장시 마크 크기 정하기
            //Point size = new Point(100, 100); //임시
            Point size = new Point(iMarkWidth, iMarkHeight);

            Bending.Menu.rsDL.TrainImageSet(bitmap, point, okng, size);
        }

        public void ScreenHide()
        {
            //tmrMark.Enabled = false;
            //Visible = false;
        }

        public void ScreenClose()
        {
        }

        private void button_ZoomIn_Click(object sender, EventArgs e)
        {
            if (m_bRefMarkRun)
                cogDS2.MouseMode = CogDisplayMouseModeConstants.ZoomIn;
        }

        private void button_ZoomOut_Click(object sender, EventArgs e)
        {
            if (m_bRefMarkRun)
                cogDS2.MouseMode = CogDisplayMouseModeConstants.ZoomOut;
        }

        private void button_FitImage_Click(object sender, EventArgs e)
        {
            if (m_bRefMarkRun)
                cogDS2.Fit(true);
        }

        private void button_Point_Click(object sender, EventArgs e)
        {
            if (m_bRefMarkRun)
                cogDS2.MouseMode = CogDisplayMouseModeConstants.Pointer;
        }

        private void button_Pan_Click(object sender, EventArgs e)
        {
            if (m_bRefMarkRun)
                cogDS2.MouseMode = CogDisplayMouseModeConstants.Pan;
        }

        private void button_Grab1_Click(object sender, EventArgs e)
        {
            if (m_bRefMarkRun)
            {
                CogImage8Grey cogGrab = new CogImage8Grey();

                Bending.Menu.frmAutoMain.Vision[VisionNo].Grab(ref cogGrab);

                cogDS2.Image = cogGrab;
            }
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            if (m_bMarkRun)
                cogDS1.MouseMode = CogDisplayMouseModeConstants.ZoomIn;
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            if (m_bMarkRun)
                cogDS1.MouseMode = CogDisplayMouseModeConstants.ZoomOut;
        }

        private void btnFitImage_Click(object sender, EventArgs e)
        {
            if (m_bMarkRun)
                cogDS1.Fit(true);
        }

        private void btnPoint_Click(object sender, EventArgs e)
        {
            if (m_bMarkRun)
                cogDS1.MouseMode = CogDisplayMouseModeConstants.Pointer;
        }

        private void btnPan_Click(object sender, EventArgs e)
        {
            if (m_bMarkRun)
                cogDS1.MouseMode = CogDisplayMouseModeConstants.Pan;
        }

        private void button_Grab_Click(object sender, EventArgs e)
        {
            if (m_bMarkRun)
            {
                int nVision2 = 0;
                CogImage8Grey cogGrab = new CogImage8Grey();

                nVision2 = VisionNo;

                Bending.Menu.frmAutoMain.Vision[nVision2].Grab(ref cogGrab);

                cogDS1.Image = cogGrab;
            }
        }

        //csh 20170601
        private void tmrMark_Tick(object sender, EventArgs e)
        {
            if (btnConfirm1.Enabled)
            {
                if (btnConfirm1.BackColor != Color.Red)
                {
                    btnConfirm1.BackColor = Color.Red;
                    btnConfirm1.ForeColor = Color.White;
                }
                else if (btnConfirm1.BackColor != Color.Gray)
                {
                    btnConfirm1.BackColor = Color.Gray;
                    btnConfirm1.ForeColor = Color.Black;
                }
            }
            else
            {
                btnConfirm1.BackColor = Color.Gray;
                btnConfirm1.ForeColor = Color.Gray;
            }

            if (btnConfirm2.Enabled)
            {
                if (btnConfirm2.BackColor != Color.Red)
                {
                    btnConfirm2.BackColor = Color.Red;
                    btnConfirm2.ForeColor = Color.White;
                }
                else if (btnConfirm2.BackColor != Color.Gray)
                {
                    btnConfirm2.BackColor = Color.Gray;
                    btnConfirm2.ForeColor = Color.Black;
                }
            }
            else
            {
                btnConfirm2.BackColor = Color.Gray;
                btnConfirm2.ForeColor = Color.Gray;
            }

            m_nLimitTimeCount++;

            if (m_nLimitTimeCount * 500 > m_nLimitTime * 1000)
            {
                //Bending.Menu.frmAutoMain.pcResult[m_nBitNo] = RUN_MANUAL_TEACH_TIME_OVER;
                //타임오버처리
                timeover = true;
                m_nLimitTimeCount = 0;
            }
        }

        private void button_ByPass_Click(object sender, EventArgs e)
        {
            MarkInfo[0].NG = true; //0이든 1이든 아무거나 true로 처리하면됨
            Visible = false;
            //Bending.Menu.frmAutoMain.pcResult[m_nBitNo] = RUN_ERROR_BY_PASS;
        }

        private void Close_Click(object sender, EventArgs e)
        {
            //잠깐 닫고싶을때 사용함.
            //Bending.Menu.frmAutoMain.Vision[VisionNo].manualMarkInitial(false);
            bWorkerVisible = false;
            Visible = false;
        }
        public bool PopupCheck()
        {
            bool result = false;

            for (int i = 0; i < MarkInfo.Length; i++)
            {
                if (MarkInfo[i].manualPopup && MarkInfo[i].bDone)
                {
                    MarkInfo[i].manualPopup = false;
                    MarkInfo[i].bDone = false;
                    MarkInfo[i].Popupcnt++;
                    result = true;
                }
            }
            return result;
        }
        public bool DoneCheck()
        {
            //하나라도 false면 false반환
            bool result = true;
            for (int i = 0; i < MarkInfo.Length; i++)
            {
                if (result)
                {
                    if (!MarkInfo[i].bDone) result = false;
                }
            }
            return result;
        }
        public struct smanualMark
        {
            public ePatternKind patternKind; //패턴종류
            public bool manualPopup; //manual창 필요여부
            public int Popupcnt; //팝업 횟수(선택횟수랑 통합해야하는지..)
            public double selectX;
            public double selectY;
            public bool selected; //작업자가 선택했음 (선택 후 한번만 사용하기 위함)
            public int selectcnt; //manual mark 선택 횟수(현재 필요없음)
            public bool bTempMark; //임시마크도 여기서 사용(리트라이 없는곳에는 의미없음)
            public bool NG; //NG처리 하고싶을때 (평소 false, 작업자가 누르면 true)
            public bool bDone; //작업완료(평소 true, popup뜨면 false)

        }
        public bool bWorkerVisible = true; //잠깐 작업자가 닫고 싶을때 사용 (평소 true, 닫으면 false)
        public bool timeover = false; //(평소 false, 타임아웃시 true)
        public smanualMark[] MarkInfo = new smanualMark[4]; // Cam별 최대 2 Point 수동 마크 기능 필요함.  (0 : Mark, 1 : Ref)
        public string pcresult = string.Empty; //메뉴얼마크창 트는 이유 //ERROR_MARK = 12, ERROR_LCHECK = 14,
        public bool bLeft = false;
        public bool bRight = false;
        public bool manualMarkSelect(ref cs2DAlign.ptXYT pt, int Kind, bool bbefore)
        {
            if (MarkInfo[Kind].selected)
            {
                pt.X = MarkInfo[Kind].selectX;
                pt.Y = MarkInfo[Kind].selectY;
                MarkInfo[Kind].selected = false;
                return true;
            }
            else
                return bbefore; //기존결과 반환
        }
        public void manualMarkInitial()
        {
            //(0 : Mark, 1 : Ref)
            for (int i = 0; i < MarkInfo.Length; i++)
            {
                MarkInfo[i].manualPopup = false;
                MarkInfo[i].Popupcnt = 0;
                MarkInfo[i].selectX = 0;
                MarkInfo[i].selectY = 0;
                MarkInfo[i].selected = false;
                MarkInfo[i].selectcnt = 0;
                MarkInfo[i].bTempMark = false;
                MarkInfo[i].NG = false;
                MarkInfo[i].bDone = true;
            }
            pcresult = string.Empty;
            bWorkerVisible = true;
            if (this.Visible) Visible = false;
        }
        private void cogDS2_MouseMove(object sender, MouseEventArgs e)
        {
            if (cbCreateLine2.Checked)
            {
                double DistX = 0, DistY = 0;
                PointDown(e.X, e.Y, cogDS2, ref DistX, ref DistY);
                lineMove(DistX, DistY, ref cogDS2);

                pos = GetMouseMapPosition(cogDS2, e.X, e.Y, mPanel);

                txtImageX.Text = "X : " + pos.x.ToString("0.000");
                txtImageY.Text = "Y : " + pos.y.ToString("0.000");
                if (pos.x <= cogDS2.Image.Width &&
                    pos.x >= 0 &&
                    pos.y <= cogDS2.Image.Height &&
                    pos.y >= 0)
                {
                    cogView8Grey = (CogImage8Grey)cogDS2.Image;
                    txtImageGray.Text = "Grey : " + cogView8Grey.GetPixel((int)pos.x, (int)pos.y).ToString();
                }
            }
            else
            {
                try
                {
                    cogDS2.InteractiveGraphics.Remove("MoveLineX");
                    cogDS2.InteractiveGraphics.Remove("MoveLineY");
                }
                catch { }
            }
        }
        private MapPosition pos;
        private CogImage8Grey cogView8Grey = new CogImage8Grey();
        private void cogDS1_MouseMove(object sender, MouseEventArgs e)
        {
            if (cbCreateLine1.Checked)
            {
                double DistX = 0, DistY = 0;
                PointDown(e.X, e.Y, cogDS1, ref DistX, ref DistY);
                lineMove(DistX, DistY, ref cogDS1);

                pos = GetMouseMapPosition(cogDS1, e.X, e.Y, mFPC);

                txtImageX.Text = "X : " + pos.x.ToString("0.000");
                txtImageY.Text = "Y : " + pos.y.ToString("0.000");
                if (pos.x <= cogDS1.Image.Width &&
                    pos.x >= 0 &&
                    pos.y <= cogDS1.Image.Height &&
                    pos.y >= 0)
                {
                    cogView8Grey = (CogImage8Grey)cogDS1.Image;
                    txtImageGray.Text = "Grey : " + cogView8Grey.GetPixel((int)pos.x, (int)pos.y).ToString();
                }
            }
            else
            {
                try
                {
                    cogDS1.InteractiveGraphics.Remove("MoveLineX");
                    cogDS1.InteractiveGraphics.Remove("MoveLineY");
                }
                catch { }
            }
        }

        public void PointDown(double dX, double dY, CogDisplay cogDisplay, ref double DistX, ref double DistY)
        {
            ICogTransform2D iTransPosition;
            CogCoordinateSpaceTree cogCoordinateSpaceTree;

            cogCoordinateSpaceTree = cogDisplay.UserDisplayTree;

            string lcTempName = "";
            if (cogCoordinateSpaceTree == null) lcTempName = "pos";
            else lcTempName = cogCoordinateSpaceTree.RootName;

            iTransPosition = cogDisplay.GetTransform("#", lcTempName);
            iTransPosition.MapPoint(dX, dY, out DistX, out DistY);
        }

        public void lineMove(double dX, double dY, ref CogDisplay cogDisplay)
        {
            try
            {
                string sKind = "";
                double dist = 0;
                double pdist = 0;

                CogLineSegment MoveLineX = new CogLineSegment();
                MoveLineX.StartX = 0;
                MoveLineX.EndX = cogDisplay.Image.Width;
                MoveLineX.StartY = dY;
                MoveLineX.EndY = dY;
                sKind = "MoveLineX";

                LineDisplay(sKind, MoveLineX, dX, dY, dist, pdist, cogDisplay);

                CogLineSegment MoveLineY = new CogLineSegment();
                MoveLineY.StartX = dX;
                MoveLineY.EndX = dX;
                MoveLineY.StartY = 0;
                MoveLineY.EndY = cogDisplay.Image.Height;
                sKind = "MoveLineY";

                LineDisplay(sKind, MoveLineY, dX, dY, dist, pdist, cogDisplay);

            }
            catch { }
        }

        private void LineDisplay(string sKind, CogLineSegment cL, double dX, double dY, double dDist, double pDist, CogDisplay cogDisplay)
        {
            try
            {
                cogDisplay.InteractiveGraphics.Remove(sKind);
            }
            catch { }

            cL.Color = CogColorConstants.Green;
            cogDisplay.InteractiveGraphics.Add(cL, sKind, false);
            double scale = 1 / cogDisplay.Zoom;
        }

        //201030 cjm origin 움직이기
        // 201102 cjm 수정
        private MapPosition mp(double x, double y)
        {
            return new MapPosition(x, y);
        }

        private void btnOriginUP1_Click(object sender, EventArgs e)
        {
            ClickMessage(mPanel);

            pos = mp(destX[mPanel], destY[mPanel] - double.Parse(tbMove1.Text));

            //pos.x = destX[mPanel] ;
            //pos.y = destY[mPanel] - double.Parse(tbMove1.Text);

            DisplayPoint(cogDS1, pos, CFG);

            destX[mPanel] = pos.x;
            destY[mPanel] = pos.y;

            txtImageX.Text = "X : " + pos.x.ToString("0.000");
            txtImageY.Text = "Y : " + pos.y.ToString("0.000");
        }
        private void btOriginDown1_Click(object sender, EventArgs e)
        {
            ClickMessage(mPanel);

            pos = mp(destX[mPanel], destY[mPanel] + double.Parse(tbMove1.Text));

            DisplayPoint(cogDS1, pos, CFG);

            destX[mPanel] = pos.x;
            destY[mPanel] = pos.y;

            txtImageX.Text = "X : " + pos.x.ToString("0.000");
            txtImageY.Text = "Y : " + pos.y.ToString("0.000");
        }

        private void btnOriginRight1_Click(object sender, EventArgs e)
        {
            ClickMessage(mPanel);

            pos = mp(destX[mPanel] + double.Parse(tbMove1.Text), destY[mPanel]);

            //pos.x = destX[mPanel] + double.Parse(tbMove1.Text);
            //pos.y = destY[mPanel];

            DisplayPoint(cogDS1, pos, CFG);

            destX[mPanel] = pos.x;
            destY[mPanel] = pos.y;

            txtImageX.Text = "X : " + pos.x.ToString("0.000");
            txtImageY.Text = "Y : " + pos.y.ToString("0.000");
        }
        private void btnOriginLeft1_Click(object sender, EventArgs e)
        {
            ClickMessage(mPanel);

            pos = mp(destX[mPanel] - double.Parse(tbMove1.Text), destY[mPanel]);

            //pos.x = destX[mPanel] - double.Parse(tbMove1.Text);
            //pos.y = destY[mPanel];

            DisplayPoint(cogDS1, pos, CFG);

            destX[mPanel] = pos.x;
            destY[mPanel] = pos.y;

            txtImageX.Text = "X : " + pos.x.ToString("0.000");
            txtImageY.Text = "Y : " + pos.y.ToString("0.000");
        }

        private void btnOriginUP2_Click(object sender, EventArgs e)
        {
            ClickMessage(mFPC);

            pos = mp(destX[mFPC], destY[mFPC] - double.Parse(tbMove2.Text));

            //pos.x = destX[mFPC];
            //pos.y = destY[mFPC] - double.Parse(tbMove2.Text);

            DisplayPoint(cogDS2, pos, CFG);

            destX[mFPC] = pos.x;
            destY[mFPC] = pos.y;

            txtImageX.Text = "X : " + pos.x.ToString("0.000");
            txtImageY.Text = "Y : " + pos.y.ToString("0.000");
        }

        private void btnOriginDown2_Click(object sender, EventArgs e)
        {
            ClickMessage(mFPC);

            pos = mp(destX[mFPC], destY[mFPC] + double.Parse(tbMove2.Text));

            //pos.x = destX[mFPC];
            //pos.y = destY[mFPC] + double.Parse(tbMove2.Text);

            DisplayPoint(cogDS2, pos, CFG);

            destX[mFPC] = pos.x;
            destY[mFPC] = pos.y;

            txtImageX.Text = "X : " + pos.x.ToString("0.000");
            txtImageY.Text = "Y : " + pos.y.ToString("0.000");
        }

        private void btnOriginRight2_Click(object sender, EventArgs e)
        {
            ClickMessage(mFPC);

            pos = mp(destX[mFPC] + double.Parse(tbMove2.Text), destY[mFPC]);

            //pos.x = destX[mFPC] + double.Parse(tbMove2.Text);
            //pos.y = destY[mFPC];

            DisplayPoint(cogDS2, pos, CFG);

            destX[mFPC] = pos.x;
            destY[mFPC] = pos.y;

            txtImageX.Text = "X : " + pos.x.ToString("0.000");
            txtImageY.Text = "Y : " + pos.y.ToString("0.000");
        }

        private void btnOriginLeft2_Click(object sender, EventArgs e)
        {
            ClickMessage(mFPC);

            pos = mp(destX[mFPC] - double.Parse(tbMove2.Text), destY[mFPC]);

            //pos.x = destX[mFPC] - double.Parse(tbMove2.Text);
            //pos.y = destY[mFPC];

            DisplayPoint(cogDS2, pos, CFG);

            destX[mFPC] = pos.x;
            destY[mFPC] = pos.y;

            txtImageX.Text = "X : " + pos.x.ToString("0.000");
            txtImageY.Text = "Y : " + pos.y.ToString("0.000");
        }

        public void ClickMessage(int index)
        {
            if (index == 0)
            {
                if (!m_bMouseClick1)
                {
                    MessageBox.Show("Panel Click Fail. Retry Panel Click");
                    return;
                }
            }
            else
            {
                if (!m_bMouseClick2)
                {
                    MessageBox.Show("Ref Click Fail. Retry Ref Click");
                    return;
                }
            }
        }

        private void tmrDisp_Tick(object sender, EventArgs e)
        {
            if (lbLCheck.ForeColor != Color.Black) lbLCheck.ForeColor = Color.Black;
            else if (lbLCheck.ForeColor != Color.Red) lbLCheck.ForeColor = Color.Red;
        }
    }
}
