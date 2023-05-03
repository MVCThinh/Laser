using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.SearchMax;
using rs2DAlign;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Bending
{
    public partial class frmPattern : Form
    {
        public csVision csvision = new csVision();

        CogRectangle cogrectangle = new CogRectangle();
        //CogRectangle PMSearchRegion = new CogRectangle();
        CogFindLineTool cogFindLine = new CogFindLineTool();
        CogRectangleAffine cogRectangle = new CogRectangleAffine();
        CogIPOneImageTool cogIPOneImageTool = new CogIPOneImageTool();

        CogCoordinateAxes cogCoordinateaxes = new CogCoordinateAxes();
        CogMaskGraphic cogmaskgraphic = new CogMaskGraphic();

        // cog pmAlign.
        CogPMAlignPattern cogPmAlignPattern = new CogPMAlignPattern();
        CogPMAlignTool cogPmAlignTool = new CogPMAlignTool();

        // cog SearchMax.
        CogSearchMaxPattern cogSearchMaxPattern = new CogSearchMaxPattern();
        CogSearchMaxTool cogSearchMaxTool = new CogSearchMaxTool();

        csLog cLog = new csLog();

        public static double dPatternX = 0;
        public static double dPatternY = 0;
        public static double dPatternR = 0;

        public bool m_bUseSearchRegion = false;

        //KSJ 20170608 Pattern Initial 관련 변수
        public int m_nCameraIndex = 0;

        bool _PatternTrain;

        bool PatternTrain
        {
            get
            {
                return _PatternTrain;
            }
            set
            {
                if (!value)
                {
                    this.btnPatternTrain.Enabled = true;
                    this.btnPatternTrain.BackColor = Color.RoyalBlue;
                    this.btnPatternTrain.ForeColor = Color.White;
                }
                else
                {
                    this.btnPatternTrain.Enabled = false;
                    this.btnPatternTrain.BackColor = Color.DarkGray;
                    this.btnPatternTrain.ForeColor = Color.Gray;
                }

                this._PatternTrain = value;
            }
        }

        public frmPattern()
        {
            InitializeComponent();
            //2018.05.11 khs cbCamName items 추가
            cboCamName.Items.Clear();
            for (int i = 0; i < CONST.CAMCnt; i++)
            {
                if (Bending.Menu.frmAutoMain.Vision[i].CFG.Use)
                    cboCamName.Items.Add(Bending.Menu.frmAutoMain.Vision[i].CFG.Name);
                else
                    cboCamName.Items.Add("Not Use");
            }
            cboCamName.SelectedIndex = Bending.Menu.frmRecipe.cbCamList.SelectedIndex;

            cboPatternKind.Items.Clear();
            cboPatternKind.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPatternKind.DataSource = Enum.GetValues(typeof(ePatternKind));

            cboFindLineKind.Items.Clear();
            cboFindLineKind.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFindLineKind.DataSource = Enum.GetValues(typeof(eLineKind));

            //string[] sList = ucSetting.DB.getCamList();
            //for (int i = 0; i < CONST.CAMCnt; i++)
            //{
            //             csVision.sCFG cfg = ucSetting.Cfg.CAMconfig_Read(i);
            //	//csVision.sCFG cfg = ucSetting.DB.getCAMConfig(i);

            //	if (cfg.Use) cboCamName.Items.Add(cfg.Name[i]);
            //	else cboCamName.Items.Add("Not Use");
            //}
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            csvision.cogDS = csvision.cogTemp;
            Dispose();
            //Visible = false;
        }

        private void btnImgLoad_Click(object sender, EventArgs e)
        {
            try
            {
                if (cboCamName.SelectedItem == null)
                {
                    MessageBox.Show("CamName is not selected");
                    return;
                }

                string path;
                string camName = cboCamName.SelectedItem.ToString().Trim();

                path = Path.Combine(CONST.cImagePath, cboCamName.SelectedItem.ToString().Trim());

                //if ((camName == "FOAM ATTACH 1" || camName == "FOAM ATTACH 2") && this.cboPatternKind.Text.Trim() == "Pattern_Arm2")
                //{
                //    path = Path.Combine(path, "FoamAttachArm2");
                //}

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path); // path 가 없을 시 만들기.
                }

                OpenFileDialog OF = new OpenFileDialog();

                OF.InitialDirectory = path; //Path.Combine(CONST.cVisionImgPath, cboCamName.SelectedItem.ToString().Trim(), "Images"); // lyw 수정.
                OF.FilterIndex = 2; // 기본으로 선택되는 확장자 2로 하면 모든 파일로 됨         
                if (OF.ShowDialog(this) == DialogResult.OK)
                {
                    //Cognex.VisionPro.CogImage8Grey adb = new Cognex.VisionPro.CogImage8Grey((Bitmap)Image.FromFile(OF.FileName));
                    //cogDSImage.Image = (Cognex.VisionPro.ICogImage)(adb);


                    //lkw 170814
                    Bitmap bmpTest = (Bitmap)Image.FromFile(OF.FileName);
                    //if (csvision.CFG.nReverseMode == CONST.eImageReverse.None)    // None
                    //{                        
                    //}
                    //else if (csvision.CFG.nReverseMode == CONST.eImageReverse.XReverse)    // X Reverse
                    //{
                    ////    bmpTest.RotateFlip(RotateFlipType.RotateNoneFlipX);                        
                    //}
                    //else if (csvision.CFG.nReverseMode == CONST.eImageReverse.YReverse)    // Y Reverse
                    //{
                    // //   bmpTest.RotateFlip(RotateFlipType.RotateNoneFlipY);                        
                    //}
                    //else if (csvision.CFG.nReverseMode == CONST.eImageReverse.AllReverse)    // All Reverse
                    //{
                    // //   bmpTest.RotateFlip(RotateFlipType.RotateNoneFlipXY);                        
                    //}
                    ////lkw 20170809 Reverse 항목 추가
                    //else if (csvision.CFG.nReverseMode == CONST.eImageReverse.Reverse90)    // 90 Reverse
                    //{
                    // //   bmpTest.RotateFlip(RotateFlipType.Rotate90FlipX);                        
                    //}
                    //else if (csvision.CFG.nReverseMode == CONST.eImageReverse.Reverse270)    // 270 Reverse
                    //{
                    // //   bmpTest.RotateFlip(RotateFlipType.Rotate270FlipX);                        
                    //}

                    cogDSImage.Image = new CogImage8Grey(bmpTest);
                    cogDSImage.AutoFit = true;

                    ImgCopy();
                }
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("btnImgLoad_Click" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }

        private void frmPattern_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            try
            {
                // mask image 초기화 하기. lyw. 잘못된 mask 로 save되는 현상이 있음으로 수정.
                cogImageMaskEditV21.MaskImage = null;

                cogDSPattern.Image = cogDSImage.Image;
                cogDSPattern.AutoFit = true;

                DISPLAY_Rectangle();

                PatternTrain = false;
                initaxesGraphic(false);

                double a = cogDSPattern.Size.Width;
                double b = cogDSPattern.Size.Height;

                double c = cogDSPattern.Image.Width;
                double d = cogDSPattern.Image.Height;

                //btnImageMask.Visible = true;
            }
            catch { };
        }

        private void ImgCopy()
        {
            try
            {
                // mask image 초기화 하기. lyw. 잘못된 mask 로 save되는 현상이 있음으로 수정.
                cogImageMaskEditV21.MaskImage = null;

                cogDSPattern.Image = cogDSImage.Image;
                cogDSPattern.AutoFit = true;

                DISPLAY_Rectangle();

                PatternTrain = false;
                initaxesGraphic(false);

                double a = cogDSPattern.Size.Width;
                double b = cogDSPattern.Size.Height;

                double c = cogDSPattern.Image.Width;
                double d = cogDSPattern.Image.Height;

                //btnImageMask.Visible = true;
            }
            catch { };
        }

        private void DISPLAY_Rectangle()
        {
            cogDSPattern.InteractiveGraphics.Clear();
            //try
            //{
            //    cogDSPattern.InteractiveGraphics.Remove("Rectangle");
            //}
            //catch { }
            cogRectangle.Interactive = true;
            cogRectangle.Color = CogColorConstants.Red;
            cogRectangle.GraphicDOFEnable = CogRectangleAffineDOFConstants.All;
            cogDSPattern.InteractiveGraphics.Add(cogRectangle, "Rectangle", false);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            cogPmAlignPattern.TrainImageMask = cogImageMaskEditV21.MaskImage;

            cogDSPattern.VisiblePixelMaskEnable = true;
            cogDSPattern.VisiblePixelMaskColor = Color.Red;
            //cogDSPattern.Image = cogPmAlignPattern.TrainImageMask;
        }

        private void PatternImgTrain(ref bool TrainResult)
        {
            try
            {
                //CogRectangleAffine lcTempCogRectangleAffine = new CogRectangleAffine();
                //CogTransform2DLinear lcTempCogTransform2DLinear = new CogTransform2DLinear();

                cogIPOneImageTool.Region = cogRectangle;
                //cogIPOneImageTool.RegionMode = CogRegionModeConstants.AffineTransform;
                cogIPOneImageTool.RegionMode = CogRegionModeConstants.PixelAlignedBoundingBox;
                cogIPOneImageTool.InputImage = cogDSPattern.Image;
                cogIPOneImageTool.Run();

                cogDSPattern.Image = cogIPOneImageTool.OutputImage;

                try
                {
                    cogDSPattern.InteractiveGraphics.Remove("mask");
                }
                catch { }

                if (cbImageMask.Checked == true)
                {
                    if (cogImageMaskEditV21.Image != null)
                    {
                        cogIPOneImageTool.InputImage = cogImageMaskEditV21.MaskImage;
                        cogIPOneImageTool.Run();

                        cogmaskgraphic.Image = (CogImage8Grey)cogIPOneImageTool.OutputImage;
                        cogDSPattern.InteractiveGraphics.Add(cogmaskgraphic, "trainmask", false);

                        if (radTypePMAlign.Checked)
                            cogPmAlignPattern.TrainImageMask = cogmaskgraphic.Image;
                        else
                            cogSearchMaxPattern.TrainImageMask = cogmaskgraphic.Image;
                    }
                    cbImageMask.Checked = false;
                }

                if (radTypePMAlign.Checked)
                {
                    cogPmAlignPattern.TrainAlgorithm = CogPMAlignTrainAlgorithmConstants.PatMax;
                    cogPmAlignPattern.TrainMode = CogPMAlignTrainModeConstants.Image;

                    cogPmAlignPattern.TrainImage = (CogImage8Grey)cogDSPattern.Image;
                    cogPmAlignPattern.TrainRegionMode = CogRegionModeConstants.PixelAlignedBoundingBoxAdjustMask;
                    cogPmAlignPattern.TrainRegion = cogIPOneImageTool.Region;

                    cogPmAlignPattern.Origin.TranslationX = cogCoordinateaxes.OriginX;
                    cogPmAlignPattern.Origin.TranslationY = cogCoordinateaxes.OriginY;

                    cogPmAlignPattern.Train();
                    cogPmAlignTool.Pattern = cogPmAlignPattern;

                    if (cbImageMask.Checked == true)
                    {
                        if (cogImageMaskEditV21.Image != null)
                        {
                            CogImage8Grey aa = cogPmAlignTool.Pattern.GetTrainedPatternImageMask();
                            cogDSPattern.Image = cogPmAlignPattern.GetTrainedPatternImage();
                        }
                        else
                        {
                            cogDSPattern.Image = cogPmAlignPattern.GetTrainedPatternImage();
                        }
                        cbImageMask.Checked = false;
                    }
                    else
                    {
                        cogDSPattern.Image = cogPmAlignPattern.GetTrainedPatternImage();
                    }
                }
                else
                {
                    // #########################
                    cogSearchMaxPattern.TrainMode = CogSearchMaxTrainModeConstants.EvaluateDOFsAtRuntime; // 실행시 자유도 평가.

                    cogSearchMaxPattern.TrainImage = (CogImage8Grey)cogDSPattern.Image;
                    cogSearchMaxPattern.TrainRegionMode = CogRegionModeConstants.PixelAlignedBoundingBoxAdjustMask;
                    cogSearchMaxPattern.TrainRegion = cogIPOneImageTool.Region;

                    cogSearchMaxPattern.Origin.TranslationX = cogCoordinateaxes.OriginX;
                    cogSearchMaxPattern.Origin.TranslationY = cogCoordinateaxes.OriginY;

                    cogSearchMaxPattern.Train();

                    cogSearchMaxTool.Pattern = cogSearchMaxPattern;

                    if (cbImageMask.Checked == true)
                    {
                        if (cogImageMaskEditV21.Image != null)
                        {
                            CogImage8Grey aa = cogSearchMaxTool.Pattern.GetTrainedPatternImageMask();
                            cogDSPattern.Image = cogSearchMaxTool.Pattern.GetTrainedPatternImage();
                            //cogDSPattern.Image = cogSearchMaxTool.GetTrainedPatternImage();
                        }
                        else
                        {
                            cogDSPattern.Image = cogSearchMaxTool.Pattern.GetTrainedPatternImage();
                        }
                        cbImageMask.Checked = false;
                    }
                    else
                    {
                        cogDSPattern.Image = cogSearchMaxTool.Pattern.GetTrainedPatternImage();
                    }
                }

                // ##########################

                cogDSPattern.Fit(true);

                TrainResult = true;
            }
            catch
            {
                TrainResult = false;
                MessageBox.Show("Train Fail");
            }
        }

        double Inspection1X;
        double Inspection1Y;

        private void btnPatternTrain_Click(object sender, EventArgs e)
        {
            bool TrainResult = false;

            if (cboCamName.SelectedIndex < 0)
            {
                MessageBox.Show("Camera not selected!");
                return;
            }

            try
            {
                if (!PatternTrain && !listPatternApply)
                {
                    PatternTrain = true;
                    PatternImgTrain(ref TrainResult);
                    //cogDSPattern.InteractiveGraphics.Remove("SCFEdgeLine");
                    //cogDSPattern.InteractiveGraphics.Remove("Point");
                }
                //else MessageBox.Show("You must click 'Copy to Pattern Image Button'");

                if (TrainResult || listPatternApply)
                {
                    if (radTypeSearchMax.Checked)
                    {
                        this.btnTest_searchMax_Click(this, null);
                    }
                    else if (radTypePMAlign.Checked)
                    {
                        this.btnTest_Click(this, null);
                    }

                    listPatternApply = false;

                    Inspection1X = double.Parse(lblX.Text);
                    Inspection1Y = double.Parse(lblY.Text);
                }

            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("btnPatternTrain_Click" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    cogDSImage.InteractiveGraphics.Clear();
                    cogDSImage.StaticGraphics.Clear();
                }
                catch { };

                this.lblScore.Text = "0.0";

                int iPattern = -1;
                cogPmAlignTool.InputImage = (CogImage8Grey)cogDSImage.Image;
                cogPmAlignTool.Pattern.TrainImage = (CogImage8Grey)cogDSPattern.Image;
                cogPmAlignTool.Pattern.Train();

                cogPmAlignTool.RunParams.RunAlgorithm = CogPMAlignRunAlgorithmConstants.PatMax;
                cogPmAlignTool.RunParams.ScoreUsingClutter = false;
                cogPmAlignTool.RunParams.ContrastThreshold = double.Parse(txtPatternContrastThreshold.Text);

                //20161030 ljh
                if (cbReverse.Checked)
                    cogPmAlignTool.Pattern.IgnorePolarity = true;
                else
                    cogPmAlignTool.Pattern.IgnorePolarity = false;

                if (m_bUseSearchRegion)
                {
                    cogDSImage.InteractiveGraphics.Add(cogrectangle, "SearchRegion", false);
                }

                if (txtAngleLow.Text.Trim() != "0" && txtAngleHigh.Text.Trim() != "0")
                {
                    cogPmAlignTool.RunParams.ZoneAngle.Configuration = CogPMAlignZoneConstants.LowHigh;

                    cogPmAlignTool.RunParams.ZoneAngle.Low = double.Parse(txtAngleLow.Text) * csvision.dRadian;
                    cogPmAlignTool.RunParams.ZoneAngle.High = double.Parse(txtAngleHigh.Text) * csvision.dRadian;
                }
                else
                {
                    cogPmAlignTool.RunParams.ZoneAngle.Configuration = CogPMAlignZoneConstants.Nominal;
                }
                cogPmAlignTool.RunParams.SaveMatchInfo = true;
                cogPmAlignTool.Run();

                double maxScore = 0;

                for (int i = 0; i < cogPmAlignTool.Results.Count; i++)
                {
                    lblScore.Text = cogPmAlignTool.Results[i].Score.ToString("0.000");
                    if (cogPmAlignTool.Results[i].Score > double.Parse(txtScoreLimit.Text) && maxScore < cogPmAlignTool.Results[i].Score)
                    {
                        maxScore = cogPmAlignTool.Results[i].Score;
                        iPattern = i; //최고점수
                                      //break;  // 생각해 봐야 함.
                    }
                }
                if (iPattern >= 0)
                {
                    double TrainCenterX = cogPmAlignTool.Results[iPattern].GetPose().TranslationX;
                    double TrainCenterY = cogPmAlignTool.Results[iPattern].GetPose().TranslationY;
                    double TrainCenterT = cogPmAlignTool.Results[iPattern].GetPose().Rotation;
                    this.lblScore.Text = maxScore.ToString(); // 최고점 표시.
                    this.lblX.Text = TrainCenterX.ToString("0.00");
                    this.lblY.Text = TrainCenterY.ToString("0.00");
                    this.lblT.Text = TrainCenterT.ToString("0.00000");

                    ICogGraphicInteractive[] iGraphic = new ICogGraphicInteractive[cogPmAlignTool.Results.Count];
                    for (int i = 0; i < iGraphic.Length; i++)
                    {
                        // hakim 20170608 boundbox -> matchregion 으로 변경(모두)
                        iGraphic[i] = cogPmAlignTool.Results[i].CreateResultGraphics(CogPMAlignResultGraphicConstants.MatchRegion | CogPMAlignResultGraphicConstants.MatchFeatures);

                        cogDSImage.InteractiveGraphics.Add(iGraphic[i], "0", false);
                    }

                    //hakim 20170608 인식 시 포인트 추가
                    CogPointMarker TrainCenter = new CogPointMarker();
                    TrainCenter.X = TrainCenterX;
                    TrainCenter.Y = TrainCenterY;
                    TrainCenter.Rotation = TrainCenterT;
                    TrainCenter.Color = CogColorConstants.Yellow;
                    TrainCenter.SizeInScreenPixels = 50;
                    cogDSImage.InteractiveGraphics.Add(TrainCenter, "Point", false);

                }
                else
                {
                    MessageBox.Show("Mark FInd NG");
                }
            }
            catch { }
        }

        private void btnPatternSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (cogDSPattern.Image == null)
                {
                    MessageBox.Show("Pattern Train First!");
                    return;
                }
                if (cboCamName.SelectedIndex < 0)
                {
                    MessageBox.Show("Camera not selected!");
                    return;
                }

                if (MessageBox.Show("Do you want to [" + (radTypeSearchMax.Checked ? "SearchMax" : "PMAlign") + "] Save?", "Information", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    //20161010 ljh
                    //csh 20170622  string RunRecipe = ucSetting.DB.getRunRecipeName().Trim();
                    string RunRecipe = CONST.RunRecipe.RecipeName;
                    string sPatternDir = "";

                    string camName = cboCamName.SelectedItem.ToString().Trim();

                    //sPatternDir = GetSelectCamPath(camName, RunRecipe, cboPatternKind.Text.Trim());
                    //기본경로
                    sPatternDir = Path.Combine(CONST.cVisionPath, camName, CONST.stringRcp, RunRecipe);

                    if (radTypePMAlign.Checked) //pmalign(기본)
                    {
                        sPatternDir = Path.Combine(sPatternDir, "PMAlign", cboPatternKind.Text.Trim());
                    }
                    else if (radTypeSearchMax.Checked)
                    {
                        sPatternDir = Path.Combine(sPatternDir, "SearchMax", cboPatternKind.Text.Trim());
                    }

                    if (cboPatternKind.Text.Trim() == "Cal" || cboPatternKind.Text.Trim() == "Cal2")
                    {
                        //if (listPattern.Items.Count >= 1)
                        //{
                        //    MessageBox.Show("Cannot Make Pattern, please remove Cal Pattern");
                        //    return;
                        //}
                    }
                    if (cb2ndMark.Checked)
                    {
                        sPatternDir = Path.Combine(sPatternDir, "2ndMark");
                    }

                    List<string> itemList = new List<string>();

                    if (radTypeSearchMax.Checked)
                        itemList.Add("SEARCHMAX");
                    else
                        itemList.Add("PMALIGN");

                    ///////////////////////////////////

                    //20161030
                    bool lcReverse = false;
                    if (cbReverse.Checked) lcReverse = true;
                    bool b2ndMark = false;
                    if (cb2ndMark.Checked) b2ndMark = true;

                    if (!Directory.Exists(sPatternDir))
                        Directory.CreateDirectory(sPatternDir);

                    //20161130 ljh 화면도 수정
                    //CogSerializer.SaveObjectToFile(cogPmAlignTool.Pattern, sPatternDir + "/" + "_" + txtPatternContrastThreshold.Text.Trim() + "_" + txtScoreLimit.Text.Trim() + "_" +
                    //                               txtAngleLow.Text.Trim() + "_" + txtAngleHigh.Text.Trim() + "_" + lcReverse + "_" + DateTime.Now.ToString("MM-dd-HH-mm-ss") + ".vpp");


                    //파일갯수세기
                    int cnt = 0;
                    DirectoryInfo di;
                    if (Directory.Exists(sPatternDir))
                    {
                        di = new DirectoryInfo(sPatternDir);
                        //di.GetFiles(@"*.vpp").OrderByDescending(s => s.CreationTime).ToArray();
                        foreach (FileInfo fi in di.GetFiles(@"*.vpp"))
                        {
                            cnt++;
                            //listPattern.Items.Add(fi.Name);
                        }
                    }

                    //pcy210115 Tray특수
                    if (cbTrayNO.Visible == true)
                    {
                        int.TryParse(cbTrayNO.Text, out int no);
                        cnt = no;
                    }

                    if (cbSearchRegion.Checked)
                    {
                        foreach (string item in itemList)
                        {
                            if (item == "PMALIGN")
                            {
                                CogSerializer.SaveObjectToFile(cogPmAlignTool.Pattern, sPatternDir + "/" + cnt.ToString("00") + "_" + txtPatternContrastThreshold.Text.Trim() + "_" + txtScoreLimit.Text.Trim() + "_" +
                                                            txtAngleLow.Text.Trim() + "_" + txtAngleHigh.Text.Trim() + "_" + lcReverse + "_" + b2ndMark + "_" + cogrectangle.X.ToString("0") + "_" + cogrectangle.Y.ToString("0") + "_" +
                                                            cogrectangle.Width.ToString("0") + "_" + cogrectangle.Height.ToString("0") + "_" + DateTime.Now.ToString("MM-dd-HH-mm-ss") + ".vpp");
                            }
                            else if (item == "SEARCHMAX")
                            {
                                CogSerializer.SaveObjectToFile(this.cogSearchMaxTool.Pattern, sPatternDir + "/" + cnt.ToString("00") + "_" + txtPatternContrastThreshold.Text.Trim() + "_" + txtScoreLimit.Text.Trim() + "_" +
                                                            txtAngleLow.Text.Trim() + "_" + txtAngleHigh.Text.Trim() + "_" + lcReverse + "_" + b2ndMark + "_" + cogrectangle.X.ToString("0") + "_" + cogrectangle.Y.ToString("0") + "_" +
                                                            cogrectangle.Width.ToString("0") + "_" + cogrectangle.Height.ToString("0") + "_" + DateTime.Now.ToString("MM-dd-HH-mm-ss") + ".vpp");
                            }
                        } // foreach.
                    }
                    else
                    {
                        foreach (string item in itemList)
                        {
                            if (item == "PMALIGN")
                            {
                                CogSerializer.SaveObjectToFile(cogPmAlignTool.Pattern, sPatternDir + "/" + cnt.ToString("00") + "_" + txtPatternContrastThreshold.Text.Trim() + "_" + txtScoreLimit.Text.Trim() + "_" +
                                                                txtAngleLow.Text.Trim() + "_" + txtAngleHigh.Text.Trim() + "_" + lcReverse + "_" + b2ndMark + "_" + DateTime.Now.ToString("MM-dd-HH-mm-ss") + ".vpp");
                            }
                            else if (item == "SEARCHMAX")
                            {
                                CogSerializer.SaveObjectToFile(cogSearchMaxTool.Pattern, sPatternDir + "/" + cnt.ToString("00") + "_" + txtPatternContrastThreshold.Text.Trim() + "_" + txtScoreLimit.Text.Trim() + "_" +
                                                                txtAngleLow.Text.Trim() + "_" + txtAngleHigh.Text.Trim() + "_" + lcReverse + "_" + b2ndMark + "_" + DateTime.Now.ToString("MM-dd-HH-mm-ss") + ".vpp");
                            }
                        } // foreach.
                    }

                    // PatternReload하는 부분 필요한지 모르겟음 일단 주석처리. khs
                    // recipe pattern reload. lyw.
                    //PatternReload();
                    LoadPatternList();
                }
            }
            catch
            {
                //              csLog.ExceptionLogSave("btnPatternSave_Click" + "," + EX.GetType().Name + "," + EX.Message);
            };
        }

        private void PatternReload()
        {
            //Bending.Menu.frmAutoMain.PatternLoad(cboCamName.SelectedIndex);

            // 현재 여기에 사용중인 pattern 도 reload 한다.
            this.csvision.GetPattern(); // 추후 해제

            LoadPatternList();
        }

        // pattern 적용하기.
        private void PatternApply(string fileName)
        {
            //cogCoordinateaxes
            if (!File.Exists(fileName))
            {
                MessageBox.Show("Can no find file = " + fileName);
                return;
            }
            FileInfo temp = new FileInfo(fileName);

            string[] Split = temp.Name.Split('_');

            txtScoreLimit.Text = Split[2]; // score limit.
            txtAngleLow.Text = Split[3]; // 각도.
            txtAngleHigh.Text = Split[4];
            cbReverse.Checked = bool.Parse(Split[5]);

            if (Split.Length > 8)
            {
                cogrectangle.SetXYWidthHeight(double.Parse(Split[7]), double.Parse(Split[8]), double.Parse(Split[9]), double.Parse(Split[10]));

                //PMSearchRegion.SetXYWidthHeight(double.Parse(Split[7]), double.Parse(Split[8]), double.Parse(Split[9]), double.Parse(Split[10]));

                if (radTypePMAlign.Checked)
                {
                    cogPmAlignTool.SearchRegion = cogrectangle;
                }
                else
                {
                    cogSearchMaxTool.SearchRegion = cogrectangle;
                }

                m_bUseSearchRegion = true;
            }
            else
            {
                cogPmAlignTool.SearchRegion = null;
                cogSearchMaxTool.SearchRegion = null;

                m_bUseSearchRegion = false;
            }


            #region 참조 source.

            //    cogSearchMax.RunParams.AcceptThreshold = double.Parse(Split[2]); // double.Parse(Split[1]);

            //    // 극성 무시 여부.
            //    cogSearchMax.RunParams.IgnorePolarity = bool.Parse(Split[5]);

            //    if (Split[3] != "0" && Split[4] != "0")
            //    {
            //        cogSearchMax.RunParams.ZoneAngle.Low = double.Parse(Split[3]) * dRadian;
            //        cogSearchMax.RunParams.ZoneAngle.High = double.Parse(Split[4]) * dRadian;

            //        cogSearchMax.RunParams.ZoneAngle.Configuration = CogSearchMaxZoneConstants.LowHigh;
            //    }
            //    else
            //        cogSearchMax.RunParams.ZoneAngle.Configuration = CogSearchMaxZoneConstants.Nominal;

            //    // PMAlign에서 score clutter 사용 여부인데 searchMax 에서는 해당 para 가 없음. 다른 tool 방식이기 때문인데 bFoam 에서 사용 여부는 무엇 때문인지??? 
            //    //if (!bFoam)
            //    //    cogPMAlign.RunParams.ScoreUsingClutter = true;
            //    //else
            //    //    cogPMAlign.RunParams.ScoreUsingClutter = false;

            //    // 검색 영역 저장된 값 영역으로 설정하기.

            //    if (Split.Length > 8)
            //    {
            //        cogrectangle.X = double.Parse(Split[6]);
            //        cogrectangle.Y = double.Parse(Split[7]);
            //        cogrectangle.Width = double.Parse(Split[8]);
            //        cogrectangle.Height = double.Parse(Split[9]);

            //        cogSearchMax.SearchRegion = cogrectangle; // 검색 영역 설정해주기.

            //        //cogrectangle.SetXYWidthHeight(cogrectangle.X, cogrectangle.Y, cogrectangle.Width, cogrectangle.Height);

            //        //cogrectangle.GraphicDOFEnable = CogRectangleDOFConstants.All;
            //        //cogrectangle.Interactive = true;

            //        //cogDS.InteractiveGraphics.Add(cogrectangle, "SearchRegion", false);
            //    }

            //    cogSearchMax.Run();

            //    double maxScore = 0;

            //    if (cogSearchMax.Results != null)
            //    {
            //        for (int j = 0; j < cogSearchMax.Results.Count; j++)
            //        {
            //            cLog.Save(csLog.LogKind.System, "Score=" + cogSearchMax.Results[j].Score.ToString() + " / Align=" + CFG.Name);

            //            if (cogSearchMax.Results[j].Score > double.Parse(Split[2]) &&/* (cogPMAlign.Results[j].FineStage || bRef) &&*/ maxScore < cogSearchMax.Results[j].Score)
            //            {
            //                maxScore = cogSearchMax.Results[j].Score;
            //                CONST.TextScore = (maxScore);

            //                results = new CogSearchMaxResults(cogSearchMax.Results);

            //                iSelect = i;
            //                iPattern = j;
            //                //break;  // 생각해 봐야 함.
            //            }
            //        }

            //        // score 에 맞게 검색 되면 더 이상 pattern 을 적용 찾기를 하지 않는다. AllBest일 경우 시간은 걸리지만 모든 파일을 검토하여 최고점을 찾도록 함.
            //        if (CFG.PatternSearchMode == CONST.ePatternSearchMode.LastBest)
            //        {
            //            if (iSelect >= 0)
            //                break;
            //        }
            //    } // if.
            //}
            //    }

            //    // pattern 찾은 경우에만..
            //    if (results != null)// && cogPMAlign.Results[0].Score > double.Parse(Split[2]))
            //    {
            //        ICogGraphicInteractive[] iGraphic = new ICogGraphicInteractive[cogSearchMax.Results.Count];

            //        for (int i = 0; i < iGraphic.Length; i++)
            //        {
            //            iGraphic[i] = cogSearchMax.Results[iPattern].CreateResultGraphics(CogSearchMaxResultGraphicConstants.BoundingBox);

            //            if (bRef) cogDS.InteractiveGraphics.Add(iGraphic[i], "Ref", false);
            //            else cogDS.InteractiveGraphics.Add(iGraphic[i], "Pattern", false);
            //        }

            //        double tempX = cogSearchMax.Results[iPattern].GetPose().TranslationX;
            //        double tempY = cogSearchMax.Results[iPattern].GetPose().TranslationY;

            //        dR = results[iPattern].GetPose().Rotation / dRadian;

            //        if (CFG.CamDirection == CONST.eCamDirection.deg0)
            //        {
            //            dX = tempX;
            //            dY = tempY;
            //        }
            //        else if (CFG.CamDirection == CONST.eCamDirection.deg90)
            //        {
            //            dX = tempY;
            //            dY = (CFG.FOVX / CFG.Resolution) - tempX;
            //        }
            //        else if (CFG.CamDirection == CONST.eCamDirection.degM90)
            //        {
            //            dX = (CFG.FOVY / CFG.Resolution) - tempY;
            //            dY = tempX;
            //        }

            //        //CogDSFontDisplay(false, dX, dY, cogSearchMax.Results[iPattern].Score);
            //    }
            //    else
            //    {
            //        CogDSFontDisplay(true, dX, dY, 0);
            //        bReturn = false;
            //    }

            //    // 다른 pattern 에 대해서도 검색하길 원할 경우.. retry등에서.. 해당 폴더에 여러개의 vpp pattern 이 정의된 경우에......
            //    // bsecond 는 ucRecipe calPattern1에서 호출, calPattern2에서 호출, this.patternSearch2에서 호출, ucRecipe 의 tmrCal_Tick 에서 호출.
            //    if (bsecond)
            //    {
            //        for (int i = 0; i < sFile.Length; i++)
            //        {
            //            if (i != iSelect)
            //            {
            //                if (sFile[i].IndexOf(".vpp") > 0)  // vpp File 이 있으면 
            //                {
            //                    sPatternFile = sFile[i];
            //                    Split = sFile[i].Split('_');

            //                    cogSearchMax.Pattern = (CogSearchMaxPattern)CogSerializer.LoadObjectFromFile(sPatternFile);
            //                    cogSearchMax.InputImage = (CogImage8Grey)cogDS.Image;

            //                    cogSearchMax.RunParams.AcceptThreshold = double.Parse(Split[1]);
            //                    //cogSearchMax.RunParams.ContrastThreshold = 2;
            //                    cogSearchMax.RunParams.ZoneAngle.Low = -10 * dRadian;
            //                    cogSearchMax.RunParams.ZoneAngle.High = 10 * dRadian;
            //                    cogSearchMax.RunParams.ZoneScale.Low = 0.8;
            //                    cogSearchMax.RunParams.ZoneScale.High = 1.2;
            //                    cogSearchMax.RunParams.ZoneAngle.Configuration = CogSearchMaxZoneConstants.LowHigh;
            //                    cogSearchMax.RunParams.ZoneScale.Configuration = CogSearchMaxZoneConstants.Nominal;

            //                    cogSearchMax.Run();

            //                    //if (cogPMAlign.Results.Count > 0)// && cogPMAlign.Results[0].Score > double.Parse(Split[2]))
            //                    //{
            //                    //    iSelect = i;
            //                    //    break;  // 생각해 봐야 함.
            //                    //}

            //                    for (int j = 0; j < cogSearchMax.Results.Count; j++)
            //                    {
            //                        if (cogSearchMax.Results[j].Score > double.Parse(Split[2]))
            //                        {
            //                            iSelect = i;
            //                            break;  // 생각해 봐야 함.
            //                        }
            //                    }
            //                    if (iSelect >= 0) break;
            //                }
            //            }
            //        }

            //        if (cogSearchMax.Results.Count > 0)// && cogPMAlign.Results[0].Score > double.Parse(Split[2]))
            //        {
            //            ICogGraphicInteractive[] iGraphic = new ICogGraphicInteractive[cogSearchMax.Results.Count];
            //            for (int i = 0; i < iGraphic.Length; i++)
            //            {
            //                iGraphic[i] = cogSearchMax.Results[0].CreateResultGraphics(CogSearchMaxResultGraphicConstants.BoundingBox);
            //                iGraphic[i].Color = CogColorConstants.Orange;
            //                cogDS.InteractiveGraphics.Add(iGraphic[i], "Pattern", false);
            //            }

            //            double tempX = cogSearchMax.Results[0].GetPose().TranslationX;
            //            double tempY = cogSearchMax.Results[0].GetPose().TranslationY;

            //            if (CFG.CamDirection == CONST.eCamDirection.deg0)
            //            {
            //                dX = tempX;
            //                dY = tempY;
            //            }
            //            else if (CFG.CamDirection == CONST.eCamDirection.deg90)
            //            {
            //                dX = tempY;
            //                dY = (CFG.FOVX / CFG.Resolution) - tempX;
            //            }
            //            else if (CFG.CamDirection == CONST.eCamDirection.degM90)
            //            {
            //                dX = (CFG.FOVY / CFG.Resolution) - tempY;
            //                dY = tempX;
            //            }
            //        }
            //        else
            //            bReturn = false;
            //    }

            //}
            //catch
            //{
            //    bReturn = false;
            //}

            //return bReturn;
            #endregion

            // #################

            // 2017 01 29
            if (radTypePMAlign.Checked)
            {
                if (fileName.Length > 0)
                    cogPmAlignTool.Pattern = (CogPMAlignPattern)CogSerializer.LoadObjectFromFile(fileName);

                cogmaskgraphic.Image = cogPmAlignTool.Pattern.TrainImageMask;
                cogDSPattern.Image = cogPmAlignTool.Pattern.TrainImage;

                cogDSPattern.InteractiveGraphics.Add(cogmaskgraphic, "mask", false);
                //cogDSPattern.Fit(true);
                cogDSPattern.AutoFit = true;

                cogDsPMAlign.Image = cogPmAlignTool.Pattern.TrainImage;
                cogDsPMAlign.InteractiveGraphics.Add(cogmaskgraphic, "mask", false);
                cogDsPMAlign.Fit(true);
            }
            else
            {
                if (fileName.Length > 0)
                    cogSearchMaxTool.Pattern = (CogSearchMaxPattern)CogSerializer.LoadObjectFromFile(fileName);

                cogDSPattern.Image = cogSearchMaxTool.Pattern.TrainImage;
                //cogDSPattern.Fit(true);
                cogDSPattern.AutoFit = true;

                cogDsSearchMax.Image = cogSearchMaxTool.Pattern.TrainImage;
                cogDsSearchMax.Fit(true);
            }
        }

        private void btnPatternLoad_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog OF = new OpenFileDialog();
                //20161010 ljh
                //csh 20170622  string RunRecipe = ucSetting.DB.getRunRecipeName().Trim();
                string RunRecipe = CONST.RunRecipe.RecipeName.Trim();
                string camName = cboCamName.SelectedItem.ToString().Trim();

                string sPatternDir = Path.Combine(CONST.cVisionPath, camName, CONST.stringRcp, RunRecipe);
                if (radTypePMAlign.Checked) //pmalign(기본)
                {
                    sPatternDir = Path.Combine(sPatternDir, "PMAlign", cboPatternKind.Text.Trim());
                }
                else if (radTypeSearchMax.Checked)
                {
                    sPatternDir = Path.Combine(sPatternDir, "SearchMax", cboPatternKind.Text.Trim());
                }

                if (cb2ndMark.Checked)
                {
                    sPatternDir = Path.Combine(sPatternDir, "2ndMark");
                }

                OF.InitialDirectory = sPatternDir;

                OF.DefaultExt = "*.vpp"; // 기본 확장자 설정
                OF.Filter = "VPP files (*.vpp)|*.vpp"; //필터 확장자명
                OF.FilterIndex = 1; // 기본으로 선택되는 확장자 2로 하면 모든 파일로 됨         

                if (OF.ShowDialog(this) == DialogResult.OK)
                {
                    PatternApply(OF.FileName);

                    //// 2017 01 29
                    //if (radTypePMAlign.Checked)
                    //{
                    //    cogPmAlignTool.Pattern = (CogPMAlignPattern)CogSerializer.LoadObjectFromFile(OF.FileName);
                    //    cogDSPattern.Image = cogPmAlignTool.Pattern.TrainImage;
                    //    cogDSPattern.Fit(true);

                    //    cogDsPMAlign.Image = cogPmAlignTool.Pattern.TrainImage;
                    //    cogDsPMAlign.Fit(true);
                    //}
                    //else
                    //{
                    //    cogSearchMaxTool.Pattern = (CogSearchMaxPattern)CogSerializer.LoadObjectFromFile(OF.FileName);
                    //    cogDSPattern.Image = cogSearchMaxTool.Pattern.TrainImage;
                    //    cogDSPattern.Fit(true);

                    //    cogDsSearchMax.Image = cogSearchMaxTool.Pattern.TrainImage;
                    //    cogDsSearchMax.Fit(true);
                    //}
                }
            }
            catch
            {
                //               csLog.ExceptionLogSave("btnPatternLoad_Click" + "," + EX.GetType().Name + "," + EX.Message);
            };
        }

        public csVision.sFindLineParam FindLineParamReadRun(ref CogCaliperScorerPosition cogposition, string kind = "", bool bInspection = false)
        {
            csVision.sFindLineParam FindLineParam = new csVision.sFindLineParam();
            //csh 20170622  string RunRecipe = ucSetting.DB.getRunRecipeName().Trim();
            string RunRecipe = CONST.RunRecipe.RecipeName;

            try
            {

                FindLineParam.Kind = kind;
                FindLineParam.NumCalipers = int.Parse(txtNumCalipers.Text);
                FindLineParam.CaliperSearchLength = double.Parse(txtCaliperSearchLength.Text);
                FindLineParam.CaliperProjectionLength = double.Parse(txtCaliperProjectionLength.Text);
                if (cboEdge0Polarity.Text.Trim() == "DarkToLight")
                    FindLineParam.Edge0Polarity = CogCaliperPolarityConstants.DarkToLight;
                else if (cboEdge0Polarity.Text.Trim() == "LightToDark")
                    FindLineParam.Edge0Polarity = CogCaliperPolarityConstants.LightToDark;
                else if (cboEdge0Polarity.Text.Trim() == "DontCare")
                    FindLineParam.Edge0Polarity = CogCaliperPolarityConstants.DontCare;

                if (cbCogPos.Text.Trim() == "True") cogposition.Enabled = true;
                else cogposition.Enabled = false;

                FindLineParam.CaliperSearchDirection = double.Parse(txtCaliperSearchDirection.Text); // *csvision.dRadian;
                FindLineParam.ContrastThreshold = double.Parse(txtFindLineContrastThreshold.Text);
                FindLineParam.NumToIgnore = int.Parse(txtNumTolgnore.Text);
                FindLineParam.FilterHalfSizeInPixels = int.Parse(txtFilterHalfSizeInPixels.Text);

                // 아래 부분 점검 필요....lyw. 왜 없는건지 검토 필요....!!!!!
                FindLineParam.ContrastThreshold = double.Parse(txtFindLineContrastThreshold.Text);
                FindLineParam.StartX = double.Parse(txtStartX.Text);
                FindLineParam.StartY = double.Parse(txtStartY.Text);
                FindLineParam.EndX = double.Parse(txtEndX.Text);
                FindLineParam.EndY = double.Parse(txtEndY.Text);
                FindLineParam.StartXDis = double.Parse(txtXDis.Text);
                FindLineParam.StartYDis = double.Parse(txtYDis.Text);
                FindLineParam.Distance = double.Parse(txtDistance.Text);
                FindLineParam.OffsetX = double.Parse(txtOffsetX.Text);
                FindLineParam.OffsetY = double.Parse(txtOffsetY.Text);

            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("FindLineParamReadRun" + "," + EX.GetType().Name + "," + EX.Message);
            }
            return FindLineParam;
        }

        private void FindLineParamSave_Rev(string Kind, bool bInspection = false)
        {
            try
            {
                //csh 20170622  string RunRecipe = ucSetting.DB.getRunRecipeName().Trim();
                string RunRecipe = CONST.RunRecipe.RecipeName;
                string lcDirectoryString = "";

                string camName = cboCamName.SelectedItem.ToString().Trim();

                lcDirectoryString = Path.Combine(CONST.cVisionPath, camName, CONST.stringRcp, RunRecipe, "Line");

                if (!Directory.Exists(lcDirectoryString))
                    Directory.CreateDirectory(lcDirectoryString);

                string lcFile = Kind + ".txt";

                StreamWriter FileInfo = new StreamWriter(lcDirectoryString + "/" + lcFile, false, Encoding.Default);
                FileInfo.WriteLine(txtNumCalipers.Text);
                FileInfo.WriteLine(txtCaliperSearchLength.Text);
                FileInfo.WriteLine(txtCaliperProjectionLength.Text);
                FileInfo.WriteLine(cboEdge0Polarity.Text);
                FileInfo.WriteLine(cbCogPos.Text); // 추가
                FileInfo.WriteLine(txtCaliperSearchDirection.Text);
                FileInfo.WriteLine(txtFindLineContrastThreshold.Text);
                FileInfo.WriteLine(txtNumTolgnore.Text);
                FileInfo.WriteLine(txtStartX.Text);
                FileInfo.WriteLine(txtStartY.Text);
                FileInfo.WriteLine(txtEndX.Text);
                FileInfo.WriteLine(txtEndY.Text);
                //아래 4개는 안쓰는걸로 보임.
                FileInfo.WriteLine(txtXDis.Text);
                FileInfo.WriteLine(txtYDis.Text);
                FileInfo.WriteLine(txtDistance.Text);
                FileInfo.WriteLine(txtFilterHalfSizeInPixels.Text);
                FileInfo.WriteLine(txtOffsetX.Text);
                FileInfo.WriteLine(txtOffsetY.Text);
                FileInfo.Close();
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("FindLineParamSave" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }

        private double RadianToDegree(double ang)
        {
            return ang / (Math.PI / 180.0f);
        }

        private void FindLineParamDisplay(csVision.sFindLineParam FindLineParam, CogCaliperScorerPosition cogposition)
        {
            txtNumCalipers.Text = FindLineParam.NumCalipers.ToString();
            txtCaliperSearchLength.Text = FindLineParam.CaliperSearchLength.ToString();
            txtCaliperProjectionLength.Text = FindLineParam.CaliperProjectionLength.ToString();
            cboEdge0Polarity.Text = FindLineParam.Edge0Polarity.ToString();
            cbCogPos.Text = cogposition.Enabled.ToString();
            txtCaliperSearchDirection.Text = FindLineParam.CaliperSearchDirection.ToString(); // RadianToDegree(FindLineParam.CaliperSearchDirection).ToString();
            txtFindLineContrastThreshold.Text = FindLineParam.ContrastThreshold.ToString();
            txtNumTolgnore.Text = FindLineParam.NumToIgnore.ToString();
            txtFilterHalfSizeInPixels.Text = FindLineParam.FilterHalfSizeInPixels.ToString();

            txtStartX.Text = FindLineParam.StartX.ToString();
            txtStartY.Text = FindLineParam.StartY.ToString();
            txtEndX.Text = FindLineParam.EndX.ToString();
            txtEndY.Text = FindLineParam.EndY.ToString();
            txtXDis.Text = FindLineParam.StartXDis.ToString();
            txtYDis.Text = FindLineParam.StartYDis.ToString();
            txtDistance.Text = FindLineParam.Distance.ToString();
            txtOffsetX.Text = FindLineParam.OffsetX.ToString();
            txtOffsetY.Text = FindLineParam.OffsetY.ToString();

            //FindLineParam.Distance = 1000;
            //    FindLineParam.StartX = 100;
            //    FindLineParam.StartY = 100;
            //    FindLineParam.EndX = 1100;
            //    FindLineParam.EndY = 100;
            //    FindLineParam.StartXDis = 0;
            //    FindLineParam.StartYDis = 0;
        }

        private void btnFindLineParamSave_Click(object sender, EventArgs e)
        {
            if (Bending.Menu.frmlogin.LogInCheck())
            {
                if (MessageBox.Show("Run Params Save!", "Information", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    FindLineParamSave_Rev(cboFindLineKind.Text.Trim(), cbInspection.Checked);

                    // lyw. save click 시에 line search 즉시 반영되도록 함.
                    //cogDSFindLine_MouseUp(this, null);
                }
            }
            else
            {
                MessageBox.Show("Confrm LoginUser");
            }
        }

        private void GraphicsRemove()
        {
            try
            {
                cogDSFindLine.InteractiveGraphics.Remove("Pattern");
            }
            catch { }
            try
            {
                cogDSFindLine.InteractiveGraphics.Remove("FindLine");
            }
            catch { }
            try
            {
                cogDSFindLine.InteractiveGraphics.Remove("ShapeSegmentRun");
            }
            catch { }
            try
            {
                cogDSFindLine.InteractiveGraphics.Remove("Line");
            }
            catch { }
            try
            {
                cogDSFindLine.InteractiveGraphics.Remove("Height1");
            }
            catch { }
            try
            {
                cogDSFindLine.InteractiveGraphics.Remove("Height1P");
            }
            catch { }
            try
            {
                cogDSFindLine.InteractiveGraphics.Remove("Height2");
            }
            catch { }
            try
            {
                cogDSFindLine.InteractiveGraphics.Remove("Height2P");
            }
            catch { }
            try
            {
                cogDSFindLine.InteractiveGraphics.Remove("Width");
            }
            catch { }
            try
            {
                cogDSFindLine.InteractiveGraphics.Remove("WidthP");
            }
            catch { }
        }

        private void btnFindLineImgLoad_Click(object sender, EventArgs e)
        {
            if (Bending.Menu.frmlogin.LogInCheck())
            {
                cogDSFindLine.Image = cogDSImage.Image;
                cogDSFindLine.AutoFit = true;

                if (cboPatternKind.SelectedItem != null)
                {
                    //lbSearchKind.Text = cboPatternKind.SelectedItem.ToString();
                }
                try
                {
                    cogDSFindLine.InteractiveGraphics.Clear();
                    string camName = cboCamName.SelectedItem.ToString().Trim();
                    string path;

                    path = Path.Combine(CONST.cImagePath, cboCamName.SelectedItem.ToString().Trim());

                    //if ((camName == "FOAM ATTACH 2" || camName == "PSA ATTACH 2") && this.cboPatternKind.Text.Trim() == "Pattern_Arm2")
                    //{
                    //    path = Path.Combine(path, "FoamAttachArm2");
                    //}

                    OpenFileDialog OF = new OpenFileDialog();

                    OF.InitialDirectory = path; // lyw. 수정.

                    OF.FilterIndex = 2; // 기본으로 선택되는 확장자 2로 하면 모든 파일로 됨         

                    if (OF.ShowDialog(this) == DialogResult.OK)
                    {
                        Cognex.VisionPro.CogImage8Grey adb = new Cognex.VisionPro.CogImage8Grey((Bitmap)Image.FromFile(OF.FileName));
                        cogDSFindLine.Image = (Cognex.VisionPro.ICogImage)(adb);
                        cogDSFindLine.AutoFit = true;
                    }
                }
                catch { }
            }
            else
            {
                MessageBox.Show("Confrm LoginUser");
            }
        }

        private void cboFindLineKind_SelectedIndexChanged(object sender, EventArgs e)
        {
            CogCaliperScorerPosition cogpos = new CogCaliperScorerPosition();

            //csVision.sFindLineParam FindLineParam = FindLineParamRead(csvision.CFG, ref cogpos, cboFindLineKind.Text.Trim(), true, cbInspection.Checked, this.chkAttachArm2.Checked);
            csVision.sFindLineParam FindLineParam = csvision.FindLineParamRead(csvision.CFG, ref cogpos, cboFindLineKind.Text.Trim());
            FindLineParamDisplay(FindLineParam, cogpos);
            csvision.Overay(false);
            cbDrawLine.Checked = false;
        }

        private void btnFindLineRun_Click(object sender, EventArgs e)
        {
            if (cbDrawLine.Checked)
            {
                try
                {
                    //CogFindLineTool cogFindLineTool = new CogFindLineTool();
                    CogLine cLine = new CogLine(); ;
                    CogGraphicCollection myRegions;
                    ICogRecord myRec;
                    CogLineSegment myLine;
                    CogCaliperScorerPosition cogPos = new CogCaliperScorerPosition();

                    myRec = cogFindLine.CreateCurrentRecord();
                    myLine = (CogLineSegment)myRec.SubRecords["InputImage"].SubRecords["ExpectedShapeSegment"].Content;
                    myRegions = (CogGraphicCollection)myRec.SubRecords["InputImage"].SubRecords["CaliperRegions"].Content;

                    cogDSFindLine.InteractiveGraphics.Clear();

                    if (double.Parse(txtNumCalipers.Text) < 2)
                    {
                        MessageBox.Show("Wrong Data in Number of Caliper!");
                    }

                    if (double.Parse(txtCaliperSearchLength.Text) < 1)
                    {
                        MessageBox.Show("Wrong Data in Search Length!");
                    }

                    if (double.Parse(txtCaliperProjectionLength.Text) < 1)
                    {
                        MessageBox.Show("Wrong Data in Projection Length!");
                    }

                    if (double.Parse(txtFindLineContrastThreshold.Text) < 0)
                    {
                        MessageBox.Show("Wrong Data in Threshold!");
                    }

                    if (double.Parse(txtNumTolgnore.Text) < 0)
                    {
                        MessageBox.Show("Wrong Data in Ignore Count!");
                    }


                    cogFindLine.RunParams.NumCalipers = int.Parse(txtNumCalipers.Text);
                    cogFindLine.RunParams.CaliperSearchLength = double.Parse(txtCaliperSearchLength.Text);
                    cogFindLine.RunParams.CaliperProjectionLength = double.Parse(txtCaliperProjectionLength.Text);

                    if (cboEdge0Polarity.SelectedIndex == 0)
                    {
                        cogFindLine.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DarkToLight;
                    }
                    else if (cboEdge0Polarity.SelectedIndex == 1)
                    {
                        cogFindLine.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DontCare;
                    }
                    else
                    {
                        cogFindLine.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.LightToDark;
                    }

                    cogFindLine.RunParams.CaliperSearchDirection = double.Parse(txtCaliperSearchDirection.Text);
                    cogFindLine.RunParams.CaliperRunParams.ContrastThreshold = double.Parse(txtFindLineContrastThreshold.Text);
                    cogFindLine.RunParams.NumToIgnore = int.Parse(txtNumTolgnore.Text);
                    if (txtStartX.Text == "0" || txtStartY.Text == "0" || txtEndX.Text == "0" || txtEndY.Text == "0")
                    {
                        cogFindLine.RunParams.ExpectedLineSegment.StartX = cogDSFindLine.Image.Width / 2 - 600;
                        cogFindLine.RunParams.ExpectedLineSegment.StartY = cogDSFindLine.Image.Height / 2;
                        cogFindLine.RunParams.ExpectedLineSegment.EndX = cogDSFindLine.Image.Width / 2 + 600;
                        cogFindLine.RunParams.ExpectedLineSegment.EndY = cogDSFindLine.Image.Height / 2;
                        txtStartX.Text = (cogDSFindLine.Image.Width / 2 - 200).ToString();
                        txtStartY.Text = (cogDSFindLine.Image.Height / 2).ToString();
                        txtEndX.Text = (cogDSFindLine.Image.Width / 2 + 200).ToString();
                        txtEndY.Text = (cogDSFindLine.Image.Height / 2).ToString();
                    }
                    else
                    {
                        cogFindLine.RunParams.ExpectedLineSegment.StartX = double.Parse(txtStartX.Text);
                        cogFindLine.RunParams.ExpectedLineSegment.StartY = double.Parse(txtStartY.Text);
                        cogFindLine.RunParams.ExpectedLineSegment.EndX = double.Parse(txtEndX.Text);
                        cogFindLine.RunParams.ExpectedLineSegment.EndY = double.Parse(txtEndY.Text);
                    }

                    if (cbCogPos.SelectedIndex == 0)
                    {
                        cogPos.Enabled = true;
                    }
                    else if (cbCogPos.SelectedIndex == 1)
                    {
                        cogPos.Enabled = false;
                    }

                    //cogpos 추가
                    cogFindLine.RunParams.CaliperRunParams.SingleEdgeScorers.Clear();
                    cogFindLine.RunParams.CaliperRunParams.SingleEdgeScorers.Add(cogPos);

                    //cogFindLine.RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.Pair;
                    cogFindLine.CurrentRecordEnable = CogFindLineCurrentRecordConstants.All;

                    cogDSFindLine.InteractiveGraphics.Add(myLine, "ShapeSegmentRun", false);
                    foreach (ICogGraphic g in myRegions)
                        cogDSFindLine.InteractiveGraphics.Add((ICogGraphicInteractive)g, "FindLine", false);

                    cogFindLine.Run();

                    cLine = cogFindLine.Results.GetLine();
                    cLine.Color = CogColorConstants.Green;
                    cogDSFindLine.InteractiveGraphics.Add(cLine, "Line", false);
                }
                catch { }
                return;
            }
            if (cboFindLineKind.SelectedIndex > -1)
            {
                dPatternX = 0;
                dPatternY = 0;
                dPatternR = 0;
                CogLine Line = new CogLine();
                CogCaliperScorerPosition cogposition = new CogCaliperScorerPosition();

                csvision.DispChange(cogDSFindLine);
                csvision.ClearInteractiveGraphic(cogDSFindLine);
                //csvision.GetPattern();

                bool bResult = false;
                //csvision.Capture(false);
                bResult = csvision.PatternSearchEnum(ref dPatternX, ref dPatternY, ref dPatternR, (ePatternKind)cboPatternKind.SelectedItem);

                txtXDis.Text = dPatternX.ToString("0.0");
                txtYDis.Text = dPatternY.ToString("0.0");
                cs2DAlign.ptXY resolution = new cs2DAlign.ptXY();
                cs2DAlign.ptXY pixelCnt = new cs2DAlign.ptXY();
                Bending.Menu.rsAlign.getResolution((int)eCalPos.Laser1, ref resolution, ref pixelCnt);
                if (resolution.X == 0)
                {
                    resolution.X = 0.0044;
                    resolution.Y = 0.0044;
                }

                csVision.sFindLineParam Findlineparam = FindLineParamReadRun(ref cogposition, cboFindLineKind.Text.Trim(), cbInspection.Checked);
                csvision.FindLine_Rev(resolution, ref Line, cogposition, Findlineparam, dPatternX, dPatternY, false);

            }
            else
            {
                cboFindLineKind.SelectedIndex = 0;
            }
        }

        // find run.
        private void FindRun()
        {
            if (cboFindLineKind.SelectedIndex > -1)
                btnFindLineRun_Click(this, null);
        }

        private void btnFindLineParamCancel_Click(object sender, EventArgs e)
        {
            if (tbVision.SelectedIndex == 1)
            {
                //csVision.sFindLineParam FindLineParam = FindLineParamRead(csvision.CFG, cboFindLineKind.Text.Trim());
                //FindLineParamDisplay(FindLineParam);

                // tab 이 전환될때 무조건 일단 run 하도록 함. 이미 적용된 것으로 착각 할 수 있기 때문에
                //this.FindRun();
            }

            //  FindLineParamDisplay(cboCamName.SelectedIndex, cboFindLineKind.Text.Trim());
        }

        private void cboCamName_SelectedIndexChanged(object sender, EventArgs e)
        {
            //KSJ 20170608
            m_nCameraIndex = cboCamName.SelectedIndex;
            cbChangeLight.Checked = false;
            txtLight1.ReadOnly = true;
            txtLight1.Text = "";
            txtLight2.ReadOnly = true;
            txtLight2.Text = "";

            PatternCameraSelect();

            this.LoadPatternList();
        }

        public void PatternCameraSelect()
        {
            Bending.Menu.frmAutoMain.setVision(ref csvision, m_nCameraIndex);
        }

        private void cogDSFindLine_MouseUp(object sender, MouseEventArgs e)
        {
            if (cbDrawLine.Checked == true)
            {
                txtStartX.Text = cogFindLine.RunParams.ExpectedLineSegment.StartX.ToString("0.00");
                txtStartY.Text = cogFindLine.RunParams.ExpectedLineSegment.StartY.ToString("0.00");
                txtEndX.Text = cogFindLine.RunParams.ExpectedLineSegment.EndX.ToString("0.00");
                txtEndY.Text = cogFindLine.RunParams.ExpectedLineSegment.EndY.ToString("0.00");
                txtCaliperSearchLength.Text = cogFindLine.RunParams.CaliperSearchLength.ToString("0.00");
                txtCaliperProjectionLength.Text = cogFindLine.RunParams.CaliperProjectionLength.ToString("0.00");
                txtCaliperSearchDirection.Text = cogFindLine.RunParams.CaliperSearchDirection.ToString("0.00");
            }
            else
            {
                dPatternX = 0;
                dPatternY = 0;
                dPatternR = 0;

                bool bResult = false;
                CogLine Line = new CogLine();
                csvision.DispChange(cogDSFindLine);

                bResult = csvision.PatternSearchEnum(ref dPatternX, ref dPatternY, ref dPatternR, (ePatternKind)cboPatternKind.SelectedItem);

                txtXDis.Text = dPatternX.ToString("0.0");
                txtYDis.Text = dPatternY.ToString("0.0");

                CogLineSegment cogLine = (CogLineSegment)csvision.GetInteractiveGraphic(cogDSFindLine, "ShapeSegmentRun");

                if (cogLine == null) return;
                txtStartX.Text = (cogLine.StartX - dPatternX).ToString("0.0");
                txtStartY.Text = (cogLine.StartY - dPatternY).ToString("0.0");
                txtEndX.Text = (cogLine.EndX - dPatternX).ToString("0.0");
                txtEndY.Text = (cogLine.EndY - dPatternY).ToString("0.0");

                return;
            }
        }

        private void initaxesGraphic(bool orginSkip)
        {


            cogCoordinateaxes.Color = CogColorConstants.Yellow;
            cogCoordinateaxes.LineStyle = CogGraphicLineStyleConstants.Solid;
            cogCoordinateaxes.GraphicDOFEnable = CogCoordinateAxesDOFConstants.All;
            //cogCoordinateaxes.SelectedSpaceName = "@"; //체커
            cogCoordinateaxes.SelectedSpaceName = "."; //체커
                                                       //. = use input image space
                                                       //# = use pixel space
                                                       //@ = use root space
                                                       //@\CheckerBoardCalibration

            if (!cbChangeLight.Checked)
            {
                if (orginSkip)
                {
                    double cx, cy;

                    if (this.radTypePMAlign.Checked)
                    {
                        //cx = this.cogDSPattern.DisplayRectangle.Left;
                        //cy = this.cogDSPattern.DisplayRectangle.Top;

                        cx = this.cogPmAlignPattern.Origin.TranslationX;
                        cy = this.cogPmAlignPattern.Origin.TranslationY;
                    }
                    else
                    {
                        cx = this.cogSearchMaxPattern.Origin.TranslationX;
                        cy = this.cogSearchMaxPattern.Origin.TranslationY;
                    }

                    //cogCoordinateaxes.
                    //cogCoordinateaxes.SetOriginLengthAspectRotationSkew(cx, cy, 20, 1, 0, 0);

                    cogCoordinateaxes.SetOriginLengthAspectRotationSkew(cogrectangle.CenterX, cogrectangle.CenterY, 20, 1, 0, 0);

                }
                else
                {
                    cogCoordinateaxes.SetOriginLengthAspectRotationSkew(cogrectangle.CenterX, cogrectangle.CenterY, 20, 1, 0, 0);

                }
            }
            //cogCoordinateaxes.SetOriginLengthAspectRotationSkew(cogrectangle.X, cogrectangle.Y, 20, 1, 0, 0);

            cogCoordinateaxes.Interactive = true;
            cogCoordinateaxes.Visible = true;
            cogDSPattern.InteractiveGraphics.Add(cogCoordinateaxes, "Origin", false);
        }

        private void btnImageMaskOk_Click(object sender, EventArgs e)
        {
            pnImageMaskEdit.Visible = false;

            lblLoadImage.Text = "LOAD IMAGE";
            ShowResult(true);
        }

        private void btnImageMaskCancel_Click(object sender, EventArgs e)
        {
            try
            {
                cogDSPattern.InteractiveGraphics.Remove("mask");
            }
            catch { }
            pnImageMaskEdit.Visible = false;
            cbImageMask.Checked = false;
            ShowResult(true);
        }

        private void btnImageMaskApply_Click(object sender, EventArgs e)
        {
            //cogPmAlignPattern.TrainImageMask = cogImageMaskEditV21.MaskImage;
            cogDSPattern.VisiblePixelMaskEnable = true;
            cogDSPattern.VisiblePixelMaskColor = Color.Red;
            cogmaskgraphic.Image = cogImageMaskEditV21.MaskImage;
            cogDSPattern.InteractiveGraphics.Add(cogmaskgraphic, "mask", false);
        }

        private void SearchRegionRectangle()
        {
            if (cbSearchRegion.Checked)
            {
                cogrectangle.SelectedSpaceName = "."; //체커 추가
                cogPmAlignTool.SearchRegion = cogrectangle;
                cogrectangle.SetXYWidthHeight(cogrectangle.X, cogrectangle.Y, cogrectangle.Width, cogrectangle.Height);
                cogrectangle.GraphicDOFEnable = CogRectangleDOFConstants.All;
                cogrectangle.Interactive = true;
                cogDSImage.InteractiveGraphics.Add(cogrectangle, "SearchRegion", false);

            }
            else
            {
                cogPmAlignTool.SearchRegion = null;
                cogrectangle.GraphicDOFEnable = CogRectangleDOFConstants.None;
                cogrectangle.Interactive = false;
                try
                {
                    cogDSImage.InteractiveGraphics.Remove("SearchRegion");
                }
                catch
                {

                }
            }
        }

        private void btnCenterOrigin_Click(object sender, EventArgs e)
        {
            try
            {
                cogCoordinateaxes.OriginX = cogRectangle.CenterX;
                cogCoordinateaxes.OriginY = cogRectangle.CenterY;
            }
            catch { }
        }

        private void cbSearchRegion_CheckedChanged(object sender, EventArgs e)
        {
            SearchRegionRectangle();
            
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            csvision.DispChange(cogDSFindCircle);
            csvision.FindCircle();
        }

        private void btnFindCircleImgLoad_Click(object sender, EventArgs e)
        {
            if (Bending.Menu.frmlogin.LogInCheck())
            {
                GraphicsRemove();
                try
                {
                    //string camName = Path.Combine(CONST.cImagePath, cboCamName.Text.Trim());
                    string path;

                    path = Path.Combine(CONST.cImagePath, cboCamName.Text.Trim());

                    //if ((camName == "FOAM ATTACH 1" || camName == "FOAM ATTACH 2") && this.cboPatternKind.Text.Trim() == "Pattern_Arm2")
                    //{
                    //    path = Path.Combine(path, "FoamAttachArm2");
                    //}

                    OpenFileDialog OF = new OpenFileDialog();

                    OF.InitialDirectory = path; // lyw수정.

                    OF.FilterIndex = 2; // 기본으로 선택되는 확장자 2로 하면 모든 파일로 됨         

                    if (OF.ShowDialog(this) == DialogResult.OK)
                    {
                        Cognex.VisionPro.CogImage8Grey adb = new Cognex.VisionPro.CogImage8Grey((Bitmap)Image.FromFile(OF.FileName));
                        cogDSFindCircle.Image = (Cognex.VisionPro.ICogImage)(adb);
                        cogDSFindCircle.AutoFit = true;
                    }
                }
                catch { }
            }
            else
            {
                MessageBox.Show("Confrm LoginUser");
            }

        }

        private void btnFindCircleParamSave_Click(object sender, EventArgs e)
        {
            if (Bending.Menu.frmlogin.LogInCheck())
            {
                if (MessageBox.Show("Find Circle Run Params Save!", "Information", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    FindCircleParamSave();
                }
            }
            else
            {
                MessageBox.Show("Confrm LoginUser");
            }
        }

        private void FindCircleParamSave()
        {
            try
            {
                //csh 20170622  string RunRecipe = ucSetting.DB.getRunRecipeName().Trim();
                string RunRecipe = CONST.RunRecipe.RecipeName;
                string camName = cboCamName.SelectedItem.ToString().Trim();
                string lcDirectoryString = Path.Combine(CONST.cVisionPath, camName, CONST.stringRcp, RunRecipe, "Line");

                if (!Directory.Exists(lcDirectoryString.ToLower()))
                    Directory.CreateDirectory(lcDirectoryString.ToLower());

                if (!File.Exists(lcDirectoryString + "/" + "FindCircle.txt"))
                {
                    StreamWriter FileInfo = new StreamWriter(lcDirectoryString + "/" + "FindCircle.txt", false, Encoding.Default);
                    FileInfo.WriteLine(txtCircleNumCalipers.Text);
                    FileInfo.WriteLine(txtCircleCaliperSearchLength.Text);
                    FileInfo.WriteLine(txtCircleCaliperProjectionLength.Text);
                    FileInfo.WriteLine(cboCircleEdge0Polarity.Text);
                    FileInfo.WriteLine(cboCircleSearchDirection.Text);
                    FileInfo.WriteLine(txtFindCircleContrastThreshold.Text);
                    FileInfo.WriteLine(txtCircleNumTolgnore.Text);
                    FileInfo.WriteLine(txtCircleCenterX.Text);
                    FileInfo.WriteLine(txtCircleCenterY.Text);
                    FileInfo.WriteLine(txtCircleRadius.Text);
                    FileInfo.WriteLine(txtCircleAngleStart.Text);
                    FileInfo.WriteLine(txtCircleAngleSpan.Text);
                    FileInfo.Close();
                }
                else
                {
                    StreamWriter FileInfo = new StreamWriter(lcDirectoryString + "/" + "FindCircle.txt", false, Encoding.Default);
                    FileInfo.WriteLine(txtNumCalipers.Text);
                    FileInfo.WriteLine(txtCaliperSearchLength.Text);
                    FileInfo.WriteLine(txtCaliperProjectionLength.Text);
                    FileInfo.WriteLine(cboEdge0Polarity.Text);
                    FileInfo.WriteLine(txtCaliperSearchDirection.Text);
                    FileInfo.WriteLine(txtFindLineContrastThreshold.Text);
                    FileInfo.WriteLine(txtNumTolgnore.Text);
                    FileInfo.WriteLine(txtStartX.Text);
                    FileInfo.WriteLine(txtStartY.Text);
                    FileInfo.WriteLine(txtEndX.Text);
                    FileInfo.WriteLine(txtEndY.Text);
                    FileInfo.WriteLine(txtXDis.Text);
                    FileInfo.WriteLine(txtYDis.Text);
                    FileInfo.WriteLine(txtDistance.Text);
                    FileInfo.Close();
                }
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("FindLineParamSave" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }

        public csVision.sFindCircleParam FindCircleParamRead(csVision.sCFG cfg, string kind = "")
        {
            string[] FileRead;
            csVision.sFindCircleParam FindCircleParam = new csVision.sFindCircleParam();
            //csh 20170622  string RunRecipe = ucSetting.DB.getRunRecipeName().Trim();
            string RunRecipe = CONST.RunRecipe.RecipeName;
            string camName = cboCamName.SelectedItem.ToString().Trim();
            try
            {
                string lcFile = Path.Combine(CONST.cVisionPath, camName, CONST.stringRcp, RunRecipe, "Line");

                lcFile = Path.Combine(lcFile, "FindCircle.txt");

                if (!File.Exists(lcFile))
                {
                    FindCircleParam.NumCalipers = 20;
                    FindCircleParam.CaliperSearchLength = 450;
                    FindCircleParam.CaliperProjectionLength = 130;
                    FindCircleParam.Edge0Polarity = CogCaliperPolarityConstants.DontCare;
                    FindCircleParam.SearchDirection = CogFindCircleSearchDirectionConstants.Outward;
                    FindCircleParam.ContrastThreshold = 5;
                    FindCircleParam.NumToIgnore = 10;
                    FindCircleParam.CenterX = 1000;
                    FindCircleParam.CenterY = 100;
                    FindCircleParam.Radius = 100;
                    FindCircleParam.AngleStart = 1100;
                    FindCircleParam.AngleSpan = 100;
                }
                else
                {
                    string sPatternDir = Path.Combine(CONST.cVisionPath, camName, CONST.stringRcp, RunRecipe, "Line");
                    string filePath = Path.Combine(sPatternDir, "FindCircle.txt"); //Path.Combine(CONST.cVisionImgPath, cfg.Name, CONST.stringRcp, RunRecipe.Trim(), "Pattern", "FindCircle.txt");
                    FileRead = File.ReadAllLines(filePath, Encoding.Default);

                    FindCircleParam.NumCalipers = int.Parse(FileRead[0]);
                    FindCircleParam.CaliperSearchLength = double.Parse(FileRead[1]);
                    FindCircleParam.CaliperProjectionLength = double.Parse(FileRead[2]);
                    if (FileRead[3] == "DarkToLight")
                        FindCircleParam.Edge0Polarity = CogCaliperPolarityConstants.DarkToLight;
                    else if (FileRead[3] == "LightToDark")
                        FindCircleParam.Edge0Polarity = CogCaliperPolarityConstants.LightToDark;
                    else if (FileRead[3] == "DontCare")
                        FindCircleParam.Edge0Polarity = CogCaliperPolarityConstants.DontCare;
                    if (FileRead[4] == "Outward")
                        FindCircleParam.SearchDirection = CogFindCircleSearchDirectionConstants.Outward;
                    else if (FileRead[4] == "Inward")
                        FindCircleParam.SearchDirection = CogFindCircleSearchDirectionConstants.Inward;
                    FindCircleParam.ContrastThreshold = double.Parse(FileRead[5]);
                    FindCircleParam.NumToIgnore = int.Parse(FileRead[6]);
                    FindCircleParam.CenterX = double.Parse(FileRead[7]);
                    FindCircleParam.CenterY = double.Parse(FileRead[8]);
                    FindCircleParam.Radius = double.Parse(FileRead[9]);
                    FindCircleParam.AngleStart = double.Parse(FileRead[10]);
                    FindCircleParam.AngleSpan = double.Parse(FileRead[11]);
                }
            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("FindCircleParamRead" + "," + EX.GetType().Name + "," + EX.Message);
            }
            return FindCircleParam;
        }

        private void FindCircleParamDisplay(csVision.sFindCircleParam FindCircleParam)
        {
            txtCircleNumCalipers.Text = FindCircleParam.NumCalipers.ToString();
            txtCircleCaliperSearchLength.Text = FindCircleParam.CaliperSearchLength.ToString();
            txtCircleCaliperProjectionLength.Text = FindCircleParam.CaliperProjectionLength.ToString();
            cboCircleEdge0Polarity.Text = FindCircleParam.Edge0Polarity.ToString();
            cboCircleSearchDirection.Text = FindCircleParam.SearchDirection.ToString();
            txtFindLineContrastThreshold.Text = FindCircleParam.ContrastThreshold.ToString();
            txtNumTolgnore.Text = FindCircleParam.NumToIgnore.ToString();
            txtStartX.Text = FindCircleParam.CenterX.ToString();
            txtStartY.Text = FindCircleParam.CenterY.ToString();
            txtEndX.Text = FindCircleParam.Radius.ToString();
            txtEndY.Text = FindCircleParam.AngleStart.ToString();
            txtXDis.Text = FindCircleParam.AngleSpan.ToString();
        }

        private void btnFindCircleParamCancel_Click(object sender, EventArgs e)
        {

        }

        CogIPOneImageTool cogIPOneImage = new CogIPOneImageTool();

        //20161218 ljh
        public void CogiPOneImage()
        {
            try
            {
                CogIPOneImageQuantize cogIPOneImageQuantize = new CogIPOneImageQuantize();
                cogIPOneImageQuantize.Levels = CogIPOneImageQuantizeLevelConstants.s2;

                cogIPOneImage.InputImage = (CogImage8Grey)cogDSImage.Image;
                cogIPOneImage.Operators.Add(cogIPOneImageQuantize);
                //cogIPOneImage.
                cogIPOneImage.Run();
            }
            catch
            {
            }
        }

        private void btnCapture1_Click(object sender, EventArgs e)
        {
            if (cboCamName.SelectedIndex < 0)
            {
                MessageBox.Show("Camera not selected!");
                return;
            }

            if (cbChangeLight.Checked)
            {
                int CamNo = cboCamName.SelectedIndex;

                if (cboCamName.SelectedIndex == 1 || cboCamName.SelectedIndex == 3 || cboCamName.SelectedIndex == 5 || cboCamName.SelectedIndex == 7) CamNo = CamNo - 1;

                if (txtLight1.Text != "")
                {
                    if (Bending.Menu.frmAutoMain.Vision[CamNo].CFG.LightType == CONST.eLightType.Light5V)
                    {
                        Bending.Menu.frmAutoMain.Light5VSet(Bending.Menu.frmAutoMain.Vision[CamNo].CFG.Light1Comport, Bending.Menu.frmAutoMain.Vision[CamNo].CFG.Light1CH, Convert.ToInt32(txtLight1.Text), CamNo);
                    }
                    else
                    {
                        Bending.Menu.frmAutoMain.Light12VSet(Bending.Menu.frmAutoMain.Vision[CamNo].CFG.Light1Comport, Bending.Menu.frmAutoMain.Vision[CamNo].CFG.Light1CH, Convert.ToInt32(txtLight1.Text), CamNo);
                    }
                }
                if (txtLight2.Text != "")
                {
                    if (Bending.Menu.frmAutoMain.Vision[CamNo + 1].CFG.LightType == CONST.eLightType.Light5V)
                    {
                        Bending.Menu.frmAutoMain.Light5VSet(Bending.Menu.frmAutoMain.Vision[CamNo + 1].CFG.Light1Comport, Bending.Menu.frmAutoMain.Vision[CamNo + 1].CFG.Light1CH, Convert.ToInt32(txtLight2.Text), CamNo + 1);
                    }
                    else
                    {
                        Bending.Menu.frmAutoMain.Light12VSet(Bending.Menu.frmAutoMain.Vision[CamNo + 1].CFG.Light1Comport, Bending.Menu.frmAutoMain.Vision[CamNo + 1].CFG.Light1CH, Convert.ToInt32(txtLight2.Text), CamNo + 1);
                    }
                }

            }

            System.Threading.Thread.Sleep(50);

            csvision.DispChange(cogDSImage);
            csvision.Capture(true, true, false, true);

            cogDSImage.InteractiveGraphics.Clear();

            ImgCopy();
        }

        private void frmPattern_Layout(object sender, LayoutEventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //frmPositionPicker f = new frmPositionPicker(cogDSImage);

            //f.Show();

        }

        private void frmPattern_Load(object sender, EventArgs e)
        {
            // pattern train 버튼 비활성화. lyw.
            this.PatternTrain = false;


            //4,515
            //675,294
            // 2017 01 29
            //panelPatternList.Visible = false;
            //panelPatternList.Location = new Point(4, 515);
            //panelPatternList.Size = new Size(675, 294);

        }

        private void cbInspection_CheckedChanged(object sender, EventArgs e)
        {
            cboFindLineKind_SelectedIndexChanged(this, null);
            this.FindRun();
        }

        private void btnTest_searchMax_Click(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    cogDSImage.InteractiveGraphics.Remove("0");
                }
                catch
                {
                };

                this.lblScore.Text = "0.0";

                int iPattern = -1;
                cogSearchMaxTool.InputImage = (CogImage8Grey)cogDSImage.Image;
                cogSearchMaxTool.Pattern.TrainImage = (CogImage8Grey)cogDSPattern.Image;
                cogSearchMaxTool.Pattern.Train();

                // searchMaxtool 에는 contrastThreadhold 는 없다. acceptthreadhold 는 일단 적용 하지 않도록 함.
                //cogSearchMaxTool.RunParams.ScoreUsingClutter = false;
                //cogSearchMaxTool.RunParams.AcceptThreshold  = double.Parse(txtPatternContrastThreshold.Text);

                if (cbReverse.Checked)
                    cogSearchMaxTool.RunParams.IgnorePolarity = true;
                else
                    cogSearchMaxTool.RunParams.IgnorePolarity = false;

                if (txtAngleLow.Text.Trim() != "0" && txtAngleHigh.Text.Trim() != "0")
                {
                    cogSearchMaxTool.RunParams.ZoneAngle.Low = double.Parse(txtAngleLow.Text) * csvision.dRadian;
                    cogSearchMaxTool.RunParams.ZoneAngle.High = double.Parse(txtAngleHigh.Text) * csvision.dRadian;
                    cogSearchMaxTool.RunParams.ZoneAngle.Configuration = CogSearchMaxZoneConstants.LowHigh;
                }

                cogSearchMaxTool.Run();

                double maxScore = 0;

                for (int i = 0; i < cogSearchMaxTool.Results.Count; i++)
                {
                    if (cogSearchMaxTool.Results[i].Score > double.Parse(txtScoreLimit.Text) && maxScore < cogSearchMaxTool.Results[i].Score)
                    {
                        maxScore = cogSearchMaxTool.Results[i].Score;
                        iPattern = i;
                        //break;  // 생각해 봐야 함.
                    }
                }

                if (iPattern >= 0)
                {
                    this.lblScore.Text = maxScore.ToString(); // 점수 표시. 
                    this.lblX.Text = cogSearchMaxTool.Results[0].GetPose().TranslationX.ToString("0.00");
                    this.lblY.Text = cogSearchMaxTool.Results[0].GetPose().TranslationY.ToString("0.00");
                    this.lblT.Text = cogSearchMaxTool.Results[0].GetPose().Rotation.ToString("0.00");

                    ICogGraphicInteractive[] iGraphic = new ICogGraphicInteractive[cogSearchMaxTool.Results.Count];
                    for (int i = 0; i < iGraphic.Length; i++)
                    {
                        iGraphic[i] = cogSearchMaxTool.Results[iPattern].CreateResultGraphics(CogSearchMaxResultGraphicConstants.BoundingBox);
                        iGraphic[i].Color = CogColorConstants.Blue;

                        cogDSImage.InteractiveGraphics.Add(iGraphic[i], "0", false);
                    }
                    //for (int i = 0; i < cogPmAlignTool.Results.Count; i++)
                    //{
                    //    cogDSImage.InteractiveGraphics.Add(cogPmAlignTool.Results[i].CreateResultGraphics(CogPMAlignResultGraphicConstants.BoundingBox), "0", false);
                    //}

                    //hakim 20170608 인식 시 포인트 추가
                    CogPointMarker TrainCenter = new CogPointMarker();

                    double TrainCenterX = cogSearchMaxTool.Results[0].GetPose().TranslationX;
                    double TrainCenterY = cogSearchMaxTool.Results[0].GetPose().TranslationY;
                    double TrainCenterT = cogSearchMaxTool.Results[0].GetPose().Rotation;

                    TrainCenter.X = TrainCenterX;
                    TrainCenter.Y = TrainCenterY;
                    TrainCenter.Rotation = TrainCenterT;
                    TrainCenter.Color = CogColorConstants.Yellow;
                    TrainCenter.SizeInScreenPixels = 50;
                    cogDSImage.InteractiveGraphics.Add(TrainCenter, "Point", false);
                }
                else
                {
                    MessageBox.Show("Mark FInd NG");
                }
            }
            catch
            {
            }
        }

        private void radTypePMAlign_CheckedChanged(object sender, EventArgs e)
        {
            btnTest_searchMax.Enabled = false;
            btnTest.Enabled = true;

            cogDSPattern.Image = cogPmAlignTool.Pattern.TrainImage;
            cogDSPattern.Fit(true);

            LoadPatternList();
        }

        private void radTypeSearchMax_CheckedChanged(object sender, EventArgs e)
        {
            btnTest_searchMax.Enabled = true;
            btnTest.Enabled = false;

            cogDSPattern.Image = cogSearchMaxTool.Pattern.TrainImage;
            cogDSPattern.Fit(true);

            LoadPatternList();
        }

        private void btnPatternManage_Click(object sender, EventArgs e)
        {
            listPattern.Items.Clear();
            panelPatternList.Visible = true;

            LoadPatternList();
        }

        private void btnHidePatternList_Click(object sender, EventArgs e)
        {
            panelPatternList.Visible = false;
        }

        private void btnLoadPatternList_Click(object sender, EventArgs e)
        {
            LoadPatternList();
        }

        private void LoadPatternList()
        {
            System.IO.DirectoryInfo di;

            //pcy200622 비우기 추가.
            Bending.Menu.frmAutoMain.Vision[cboCamName.SelectedIndex].clear_m_strFirstPattanSearchName();

            listPattern.Items.Clear();
            try
            {
                OpenFileDialog OF = new OpenFileDialog();
                //20161010 ljh
                //csh 20170622  string RunRecipe = ucSetting.DB.getRunRecipeName().Trim();
                string RunRecipe = CONST.RunRecipe.RecipeName;

                string camName = cboCamName.SelectedItem.ToString().Trim();
                string sPath = Path.Combine(CONST.cVisionPath, camName, CONST.stringRcp, RunRecipe);

                if (radTypePMAlign.Checked) //pmalign(기본)
                {
                    lblPatternType.Text = "PMAlign";
                    sPath = Path.Combine(sPath, "PMAlign", cboPatternKind.Text.Trim());
                }
                else if (radTypeSearchMax.Checked)
                {
                    lblPatternType.Text = "SearchMax";
                    sPath = Path.Combine(sPath, "SearchMax", cboPatternKind.Text.Trim());
                }

                if (cb2ndMark.Checked)
                {
                    sPath = Path.Combine(sPath, "2ndMark");
                }

                // lyw. 수정.
                // 2017 01 29
                OF.InitialDirectory = sPath;

                if (System.IO.Directory.Exists(sPath))
                {
                    di = new System.IO.DirectoryInfo(sPath);
                    foreach (System.IO.FileInfo fi in di.GetFiles(@"*.vpp"))
                    {
                        //fi.Delete();
                        listPattern.Items.Add(fi.Name);
                    }
                    //di.Delete();
                }
            }
            catch
            {
            }
        }

        private void listPattern_Click(object sender, EventArgs e)
        {
            if (listPattern.Items.Count < 1) return;
            if (listPattern.SelectedIndex < 0) return;
            string sFile = listPattern.SelectedItem.ToString();
            //csh 20170622  string RunRecipe = ucSetting.DB.getRunRecipeName().Trim();
            string RunRecipe = CONST.RunRecipe.RecipeName;
            string camName = cboCamName.SelectedItem.ToString().Trim();
            string sPathFile;

            CogMaskGraphic ImageMask = new CogMaskGraphic();
            string sPath = Path.Combine(CONST.cVisionPath, camName, CONST.stringRcp, RunRecipe);

            if (radTypePMAlign.Checked) //pmalign(기본)
            {
                lblPatternType.Text = "PMAlign";
                sPath = Path.Combine(sPath, "PMAlign", cboPatternKind.Text.Trim());
            }
            else if (radTypeSearchMax.Checked)
            {
                lblPatternType.Text = "SearchMax";
                sPath = Path.Combine(sPath, "SearchMax", cboPatternKind.Text.Trim());
            }

            if (cb2ndMark.Checked)
            {
                sPath = Path.Combine(sPath, "2ndMark");
            }

            try
            {
                sPathFile = Path.Combine(sPath, sFile);
                if (radTypePMAlign.Checked)
                {
                    CogPMAlignPattern cogPMAlignPattern = new CogPMAlignPattern();
                    cogPMAlignPattern = (CogPMAlignPattern)CogSerializer.LoadObjectFromFile(sPathFile);

                    ImageMask.Image = cogPMAlignPattern.TrainImageMask;
                    cogDsViewPattern.InteractiveGraphics.Clear();
                    cogDsViewPattern.InteractiveGraphics.Add(ImageMask, "mask", false);

                    cogDsViewPattern.Image = cogPMAlignPattern.TrainImage;
                    cogDsViewPattern.Fit(true);
                }
                else if (radTypeSearchMax.Checked)
                {
                    //CogSearchMaxTool cogSearchMaxTool = new CogSearchMaxTool();
                    CogSearchMaxPattern cogSearchMaxPattern = new CogSearchMaxPattern();
                    cogSearchMaxPattern = (CogSearchMaxPattern)CogSerializer.LoadObjectFromFile(sPathFile);

                    ImageMask.Image = cogSearchMaxPattern.TrainImageMask;
                    cogDsViewPattern.InteractiveGraphics.Clear();
                    cogDsViewPattern.InteractiveGraphics.Add(ImageMask, "mask", false);

                    cogDsViewPattern.Image = cogSearchMaxPattern.TrainImage;
                    cogDsViewPattern.Fit(true);
                }
                else return;
            }
            catch { }
        }

        private void btnApplyPattern_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to Pattern Apply?", "Information", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (listPattern.Items.Count < 1) return;
                if (listPattern.SelectedIndex < 0) return;
                string sFile = listPattern.SelectedItem.ToString();
                //csh 20170622  string RunRecipe = ucSetting.DB.getRunRecipeName().Trim();
                string RunRecipe = CONST.RunRecipe.RecipeName;
                string sPath, sPathFile;
                string camName = cboCamName.SelectedItem.ToString().Trim();

                try
                {
                    sPath = Path.Combine(CONST.cVisionPath, camName, CONST.stringRcp, RunRecipe);

                    if (radTypePMAlign.Checked) //pmalign(기본)
                    {
                        sPath = Path.Combine(sPath, "PMAlign", cboPatternKind.Text.Trim());
                    }
                    else if (radTypeSearchMax.Checked)
                    {
                        sPath = Path.Combine(sPath, "SearchMax", cboPatternKind.Text.Trim());
                    }

                    if (cb2ndMark.Checked)
                    {
                        sPath = Path.Combine(sPath, "2ndMark");
                    }

                    sPathFile = Path.Combine(sPath, sFile);

                    ////
                    ////

                    PatternApply(sPathFile);


                    initaxesGraphic(true);

                    //if (lblPatternType.Text == "PMAlign")
                    //{
                    //    sPath = Path.Combine(CONST.cVisionImgPath, cboCamName.SelectedItem.ToString().Trim(), CONST.stringRcp, RunRecipe.Trim(), "Pattern");
                    //    //CogPMAlignPattern cogPMAlignPattern = new CogPMAlignPattern();
                    //    sPathFile = sPath + "\\" + sFile;
                    //    cogPmAlignPattern = (CogPMAlignPattern)CogSerializer.LoadObjectFromFile(sPathFile);
                    //    cogDSPattern.Image = cogPmAlignPattern.TrainImage;
                    //    cogDSPattern.Fit(true);
                    //}
                    //else if (lblPatternType.Text == "SearchMax")
                    //{
                    //    sPath = Path.Combine(CONST.cVisionImgPath, cboCamName.SelectedItem.ToString().Trim(), CONST.stringRcp, RunRecipe.Trim(), "Pattern", "SearchMax");
                    //    //CogSearchMaxPattern cogSearchMaxPattern = new CogSearchMaxPattern();
                    //    sPathFile = sPath + "\\" + sFile;
                    //    cogSearchMaxPattern = (CogSearchMaxPattern)CogSerializer.LoadObjectFromFile(sPathFile);
                    //    cogDSPattern.Image = cogSearchMaxPattern.TrainImage;
                    //    cogDSPattern.Fit(true);
                    //}
                    //else return;

                }
                catch
                {
                }
            }
        }

        private void btnDeletePattern_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to Delete Selected Pattern?", "Information", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (listPattern.Items.Count < 1) return;
                if (listPattern.SelectedIndex < 0) return;
                string sFile = listPattern.SelectedItem.ToString();
                string RunRecipe = CONST.RunRecipe.RecipeName;
                string sPath, sPathFile;
                string camName = cboCamName.SelectedItem.ToString().Trim();

                try
                {
                    sPath = Path.Combine(CONST.cVisionPath, camName, CONST.stringRcp, RunRecipe);
                    if (radTypePMAlign.Checked) //pmalign(기본)
                    {
                        sPath = Path.Combine(sPath, "PMAlign", cboPatternKind.Text.Trim());
                    }
                    else if (radTypeSearchMax.Checked)
                    {
                        sPath = Path.Combine(sPath, "SearchMax", cboPatternKind.Text.Trim());
                    }

                    if (cb2ndMark.Checked)
                    {
                        sPath = Path.Combine(sPath, "2ndMark");
                    }

                    sPathFile = Path.Combine(sPath, sFile);

                    if (System.IO.Directory.Exists(sPath))
                    {
                        System.IO.File.Delete(sPathFile);
                    }
                    LoadPatternList();
                }
                catch
                {
                }
            }
        }

        private void btnPatternReload_Click(object sender, EventArgs e)
        {
            if (this.cboCamName.SelectedItem != null)
            {
                string camName;

                camName = cboCamName.SelectedItem.ToString();

                if (MessageBox.Show("Pattern Reload?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    PatternReload();
                }
            }
        }

        private void chkAttachArm2_CheckedChanged(object sender, EventArgs e)
        {
            // 현재 적용 으로 실행.
            cboFindLineKind_SelectedIndexChanged(this, null);
            this.FindRun();
        }

        private void cboPatternKind_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboCamName.SelectedIndex < 0)
            {
                cboCamName.SelectedIndex = 0;

                //            MessageBox.Show("Please Select Camera");
                //return;
            }
            PatternReload();
        }

        bool listPatternApply = false;
        private void listPattern_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            btnApplyPattern_Click(this, null);
            PatternTrain = false;
            listPatternApply = true;
            cogDSPattern.InteractiveGraphics.Clear();
        }

        private void btnCamCenterOrigin_Click(object sender, EventArgs e)
        {
            try
            {
                cogCoordinateaxes.OriginX = cogDSImage.Image.Width / 2;
                cogCoordinateaxes.OriginY = cogDSImage.Image.Height / 2;
            }
            catch { }
        }

        private void cbImageMask_CheckedChanged(object sender, EventArgs e)
        {
            //hakim 20170608 imagemask 관련 체크박스 추가.
            if (cbImageMask.Checked)
            {
                pnImageMaskEdit.Location = new System.Drawing.Point(1, 1);
                pnImageMaskEdit.Location = cogDSImage.Location;
                pnImageMaskEdit.Size = new Size(568, 500);
                lblLoadImage.Text = "Pattern Image Mask Edit"; // 글자 바꿈
                pnImageMaskEdit.Visible = true;
                cogImageMaskEditV21.Image = cogDSImage.Image;
                ShowResult(false);
            }
            else
            {
                try
                {
                    cogDSPattern.InteractiveGraphics.Remove("mask");
                }
                catch { }
                ShowResult(true);
                pnImageMaskEdit.Visible = false;
            }
        }

        private void ShowResult(bool bCheck)
        {
            if (bCheck)
            {
                lblX.Visible = true;
                lblY.Visible = true;
                lblT.Visible = true;
                lblScore.Visible = true;
                label49.Visible = true;
                label50.Visible = true;
                label52.Visible = true;
                label54.Visible = true;
                cbChangeLight.Visible = true;
                txtLight1.Visible = true;
                txtLight2.Visible = true;
                cbSearchRegion.Visible = true;
                panel7.Visible = true;
                btnImgLoad.Visible = true;
            }
            else
            {
                lblX.Visible = false;
                lblY.Visible = false;
                lblT.Visible = false;
                lblScore.Visible = false;
                label49.Visible = false;
                label50.Visible = false;
                label52.Visible = false;
                label54.Visible = false;
                cbChangeLight.Visible = false;
                txtLight1.Visible = false;
                txtLight2.Visible = false;
                cbSearchRegion.Visible = false;
                panel7.Visible = false;
                btnImgLoad.Visible = false;
            }
        }

        private void btnCaptureFL_Click(object sender, EventArgs e)
        {
            csvision.DispChange(cogDSFindLine);
            csvision.Capture(true, true, false, true);
        }

        //KSJ 20170609 Select
        public void RegistCamSelect(int nCamNum)
        {
            cboCamName.SelectedIndex = nCamNum;
        }

        private void btnImageClear_Click(object sender, EventArgs e)
        {
            cogDSImage.InteractiveGraphics.Clear();
        }

        private void cbDrawLine_CheckedChanged(object sender, EventArgs e)
        {
            if (cboFindLineKind.Text == "")
            {
                MessageBox.Show("Please Select Kind");
                cbDrawLine.Checked = false;
                return;
            }

            if (cbDrawLine.Checked)
            {
                CogLine Line = new CogLine();
                CogCaliperScorerPosition cogPos = new CogCaliperScorerPosition();
                cogFindLine.InputImage = (CogImage8Grey)cogDSFindLine.Image;

                if (int.Parse(txtNumCalipers.Text) < 2)
                {
                    MessageBox.Show("Number of Caliper is wrong!(at least over 2)");

                    return;
                }
                cogFindLine.RunParams.NumCalipers = int.Parse(txtNumCalipers.Text);
                cogFindLine.RunParams.CaliperSearchLength = double.Parse(txtCaliperSearchLength.Text);
                cogFindLine.RunParams.CaliperProjectionLength = double.Parse(txtCaliperProjectionLength.Text);

                if (cboEdge0Polarity.SelectedIndex == 0)
                {
                    cogFindLine.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DarkToLight;
                }
                else if (cboEdge0Polarity.SelectedIndex == 1)
                {
                    cogFindLine.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DontCare;
                }
                else
                {
                    cogFindLine.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.LightToDark;
                }

                cogFindLine.RunParams.CaliperSearchDirection = double.Parse(txtCaliperSearchDirection.Text);
                cogFindLine.RunParams.CaliperRunParams.ContrastThreshold = double.Parse(txtFindLineContrastThreshold.Text);
                cogFindLine.RunParams.NumToIgnore = int.Parse(txtNumTolgnore.Text);
                if (txtStartX.Text == "0" || txtStartY.Text == "0" || txtEndX.Text == "0" || txtEndY.Text == "0")
                {
                    cogFindLine.RunParams.ExpectedLineSegment.StartX = cogDSFindLine.Image.Width / 2 - 600;
                    cogFindLine.RunParams.ExpectedLineSegment.StartY = cogDSFindLine.Image.Height / 2;
                    cogFindLine.RunParams.ExpectedLineSegment.EndX = cogDSFindLine.Image.Width / 2 + 600;
                    cogFindLine.RunParams.ExpectedLineSegment.EndY = cogDSFindLine.Image.Height / 2;
                    txtStartX.Text = (cogDSFindLine.Image.Width / 2 - 200).ToString();
                    txtStartY.Text = (cogDSFindLine.Image.Height / 2).ToString();
                    txtEndX.Text = (cogDSFindLine.Image.Width / 2 + 200).ToString();
                    txtEndY.Text = (cogDSFindLine.Image.Height / 2).ToString();
                }
                else
                {
                    cogFindLine.RunParams.ExpectedLineSegment.StartX = double.Parse(txtStartX.Text);
                    cogFindLine.RunParams.ExpectedLineSegment.StartY = double.Parse(txtStartY.Text);
                    cogFindLine.RunParams.ExpectedLineSegment.EndX = double.Parse(txtEndX.Text);
                    cogFindLine.RunParams.ExpectedLineSegment.EndY = double.Parse(txtEndY.Text);
                }

                if (cbCogPos.SelectedIndex == 0)
                {
                    cogPos.Enabled = true;
                }
                else if (cbCogPos.SelectedIndex == 1)
                {
                    cogPos.Enabled = false;
                }

                cogFindLine.RunParams.CaliperRunParams.SingleEdgeScorers.Clear();
                cogFindLine.RunParams.CaliperRunParams.SingleEdgeScorers.Add(cogPos);


                cogFindLine.CurrentRecordEnable = CogFindLineCurrentRecordConstants.All;
                CogGraphicCollection myRegions;
                ICogRecord myRec;
                CogLineSegment myLine;

                myRec = cogFindLine.CreateCurrentRecord();

                myLine = (CogLineSegment)myRec.SubRecords["InputImage"].SubRecords["ExpectedShapeSegment"].Content;
                myRegions = (CogGraphicCollection)myRec.SubRecords["InputImage"].SubRecords["CaliperRegions"].Content;
                cogDSFindLine.InteractiveGraphics.Add(myLine, "ShapeSegmentRun", false);
                foreach (ICogGraphic g in myRegions)
                    cogDSFindLine.InteractiveGraphics.Add((ICogGraphicInteractive)g, "FindLine", false);
            }
            else
            {
                cogDSFindLine.InteractiveGraphics.Clear();
            }
        }

        private void btnFoamLineRun_Click(object sender, EventArgs e)
        {
            cogDSFindLine.InteractiveGraphics.Clear();
            CogLine cLine = new CogLine();
            csvision.FindLine(ref cLine, cboFindLineKind.Text.Trim());
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            double dX, dY, dWidth, dHeight;
            dX = double.Parse(textBox1.Text);
            dY = double.Parse(textBox2.Text);
            dWidth = double.Parse(textBox3.Text);
            dHeight = double.Parse(textBox4.Text);
            cogrectangle.SetXYWidthHeight(dX, dY, dWidth, dHeight);
        }

        private void cbChangeLight_CheckedChanged(object sender, EventArgs e)
        {
            if (cbChangeLight.Checked)
            {
                txtLight1.ReadOnly = false;
                txtLight2.ReadOnly = false;
            }
            else
            {
                txtLight1.ReadOnly = true;
                txtLight2.ReadOnly = true;
            }
        }

        private void cogDSPattern_MouseUp(object sender, MouseEventArgs e)
        {
            cs2DAlign.ptXY p1 = new cs2DAlign.ptXY();
            cs2DAlign.ptXY p2 = new cs2DAlign.ptXY();
            double offset = 0;


            try
            {
                txtOriginX.Text = cogCoordinateaxes.OriginX.ToString("N3");
                txtOriginY.Text = cogCoordinateaxes.OriginY.ToString("N3");

                p2.X = cogCoordinateaxes.OriginX;
                p2.Y = cogCoordinateaxes.OriginY;

                p1.X = Inspection1X;
                p1.Y = Inspection1Y;

                double dd = Bending.Menu.rsAlign.getLength((int)eCalPos.Laser1, (int)eCalPos.Laser1, p1, p2, offset);

                //if (Bending.Menu.frmSetting.revData.mLcheck[(short)eCamNO4.Inspection1].LCheckOffset != double.NaN)//|| double.TryParse(txtOffsetX.Text, out double d))
                //{
                //	dd += Bending.Menu.frmSetting.revData.mLcheck[(short)eCamNO4.Inspection1].LCheckOffset;
                //}

                //double ratio = 0.02307692307692308;
                //double ratio = 0.01907692307692308;
                //double cha = dd - 23;
                //dd = dd - cha * ratio;

                lblength.Text = dd.ToString("0.0000");
            }
            catch { }
        }
        private void lblength_Click(object sender, EventArgs e)
        {
            if (double.TryParse(lblX.Text, out double doriginX))
                cogCoordinateaxes.OriginX = doriginX;
            if (double.TryParse(lblY.Text, out double doriginY))
                cogCoordinateaxes.OriginY = doriginY;
        }

        private void btnBeforeOrigin_Click(object sender, EventArgs e)
        {
            if (double.TryParse(lblX.Text, out double doriginX))
                cogCoordinateaxes.OriginX = doriginX;
            if (double.TryParse(lblY.Text, out double doriginY))
                cogCoordinateaxes.OriginY = doriginY;
        }

        private void txtOriginY_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                if (double.TryParse(txtOriginY.Text, out double doriginY))
                    cogCoordinateaxes.OriginY = doriginY;
            }
        }

        private void txtOriginX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                if (double.TryParse(txtOriginX.Text, out double doriginX))
                    cogCoordinateaxes.OriginX = doriginX;
            }
        }
        // 20200924 cjm Origin 움직이기 변경
        private void btOriginUp_Click(object sender, EventArgs e)
        {
            double OriginDown = cogCoordinateaxes.OriginY;
            OriginDown = OriginDown - double.Parse(tbMove.Text); ;
            cogCoordinateaxes.OriginY = OriginDown;
            txtOriginY.Text = cogCoordinateaxes.OriginY.ToString();
        }
        private void btOriginDown_Click(object sender, EventArgs e)
        {
            double OriginDown = cogCoordinateaxes.OriginY;
            OriginDown = OriginDown + double.Parse(tbMove.Text); ;
            cogCoordinateaxes.OriginY = OriginDown;
            txtOriginY.Text = cogCoordinateaxes.OriginY.ToString();
        }

        private void btOriginRight_Click(object sender, EventArgs e)
        {
            double OriginRight = cogCoordinateaxes.OriginX;
            OriginRight = OriginRight + double.Parse(tbMove.Text); ;
            cogCoordinateaxes.OriginX = OriginRight;
            txtOriginX.Text = cogCoordinateaxes.OriginX.ToString();
        }

        private void btOriginLeft_Click(object sender, EventArgs e)
        {
            double OriginLeft = cogCoordinateaxes.OriginX;
            OriginLeft = OriginLeft - double.Parse(tbMove.Text); ;
            cogCoordinateaxes.OriginX = OriginLeft;
            txtOriginX.Text = cogCoordinateaxes.OriginX.ToString();
        }

        // 20200919 cjm Origin 이 90(degree)도 씩 움직임
        private void btClockwise_Click(object sender, EventArgs e)
        {

            cogCoordinateaxes.Rotation += DegreeToRadian(90);
        }
        private void btUnClockwise_Click(object sender, EventArgs e)
        {
            cogCoordinateaxes.Rotation -= DegreeToRadian(90);
        }
        public double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
        //20200924 cjm Origin 움직이기 픽셀 크기 워하는 값으로 추가
        private void tbMove_KeyPress(object sender, KeyPressEventArgs e)
        {
            //숫자만 입력되도록 필터링
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))    //숫자와 백스페이스를 제외한 나머지를 바로 처리
            {
                e.Handled = true;
            }
        }

        private void btnNumChange_Click(object sender, EventArgs e)
        {
            try
            {
                if (listPattern.Items.Count < 1) return;
                if (listPattern.SelectedIndex < 0) return;
                string sFile = listPattern.SelectedItem.ToString();
                //csh 20170622  string RunRecipe = ucSetting.DB.getRunRecipeName().Trim();
                string RunRecipe = CONST.RunRecipe.RecipeName;
                string sPath, sPathFile, sDestFile;
                string camName = cboCamName.SelectedItem.ToString().Trim();
                sPath = Path.Combine(CONST.cVisionPath, camName, CONST.stringRcp, RunRecipe);

                if (radTypePMAlign.Checked) //pmalign(기본)
                {
                    sPath = Path.Combine(sPath, "PMAlign", cboPatternKind.Text.Trim());
                }
                else if (radTypeSearchMax.Checked)
                {
                    sPath = Path.Combine(sPath, "SearchMax", cboPatternKind.Text.Trim());
                }

                if (cb2ndMark.Checked)
                {
                    sPath = Path.Combine(sPath, "2ndMark");
                }

                sPathFile = Path.Combine(sPath, sFile);

                if (int.TryParse(txtNum.Text, out int iresult))
                {
                    string sChangeNum = iresult.ToString("00");
                    string sNewFile = sChangeNum + sFile.Substring(2, sFile.Length - 2);
                    sDestFile = Path.Combine(sPath, sNewFile);
                    File.Move(sPathFile, sDestFile);
                }
                LoadPatternList();
            }
            catch 
            {

            }
        }

        //210216 cjm Origin Size Change
        private void tbOriginSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            //숫자만 입력되도록 필터링
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))    //숫자와 백스페이스를 제외한 나머지를 바로 처리
            {
                e.Handled = true;
            }

            if (e.KeyChar == Convert.ToChar(Keys.Enter))
            {
                double b = cogCoordinateaxes.DisplayedXAxisLength;

                double OriginSize = double.Parse(tbOriginSize.Text);

                cogCoordinateaxes.DisplayedXAxisLength = OriginSize;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                cogDSPattern.Image = Bending.Menu.frmRecipe.changeImage(cboCamName.Text, (CogImage8Grey)cogDSPattern.Image);
            }
            catch { }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                cogDSFindLine.Image = Bending.Menu.frmRecipe.changeImage(cboCamName.Text, (CogImage8Grey)cogDSFindLine.Image);
            }
            catch { }
        }

        private void cb2ndMark_CheckedChanged(object sender, EventArgs e)
        {
            PatternReload();
        }
    }
}
