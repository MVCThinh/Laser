using System;
using System.IO;
using System.Windows.Forms;

namespace Bending
{
    public partial class ucSetting : UserControl
    {
        static object lock1 = new object();

        public static csConfig _cCFG = new csConfig();
        public static csConfig cCFG
        {
            get
            {
                lock (_cCFG)
                {
                    return _cCFG;
                }
            }
            set
            {
                lock (_cCFG)
                {
                    _cCFG = value;
                }
            }
        }

        csLog cLog = new csLog();
        public csVision[] Vision;

        public REVData revData = new REVData(Path.Combine(CONST.Folder, "EQData\\Config\\ModelData"), "VisionConfig.ini");      // jyh Insert
        //public REVData revData = new REVData("C: \\Users\\Trieu\\Desktop\\VISION\\LaserMarking\\SDV_LaserMarking_210819_01\\Bending\\VisionConfig");
        public ucSetting()
        {
            InitializeComponent();

            for (int i = 0; i < CONST.CAMCnt; i++)
            {
                if (Menu.frmAutoMain.Vision[i].CFG.Name != null)
                {
                    cboName.Items.Add(Menu.frmAutoMain.Vision[i].CFG.Name);
                }
            }

            string[] sLoginUserList = cCFG.LogInUserList();

            PasswordDisplay();

            cbInspMode.Items.Clear();
            cbInspMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cbInspMode.DataSource = Enum.GetValues(typeof(eInspMode));

            cbPatternSearchKind1.Items.Clear();
            cbPatternSearchKind1.DropDownStyle = ComboBoxStyle.DropDownList;
            cbPatternSearchKind1.DataSource = Enum.GetValues(typeof(eSearchKind));

            cbPatternSearchKind2.Items.Clear();
            cbPatternSearchKind2.DropDownStyle = ComboBoxStyle.DropDownList;
            cbPatternSearchKind2.DataSource = Enum.GetValues(typeof(eSearchKind));

            //탭정리..
            //210216 cjm 탭정리
            //210217 cjm Setting 탭정리 
            if (CONST.PCNo == 1 || CONST.PCNo == 2)
            {
                tcSetPC.TabPages.Remove(tpPC1);
                tcSetPC.TabPages.Remove(tpPC2);
                tcSetPC.TabPages.Remove(tpPC3);
                tcSetPC.TabPages.Remove(tpPC4);

                lblPre.Text = "PC" + CONST.PCNo + " MODEL";
                panel14.Location = panel2.Location;
                label86.Visible = false;
                txtSCFLengthSpec.Visible = false;

                // 210216 cjm 하드코딩
                label132.Location = new System.Drawing.Point(panel14.Location.X, panel14.Location.Y + 50);
                cbAttachRetryUse.Location = new System.Drawing.Point(label132.Location.X + 220, label132.Location.Y);
                gbLaserTor.Visible = true;


            }
            //else if (CONST.PCNo == 2)
            //{
            //    tcSetPC.TabPages.Remove(tpPC1);
            //    tcSetPC.TabPages.Remove(tpPC3);
            //    tcSetPC.TabPages.Remove(tpPC4);
            //}
            else if (CONST.PCNo == 3 || CONST.PCNo == 6)
            {
                tcSetPC.TabPages.Remove(tpPC2);
                tcSetPC.TabPages.Remove(tpPC1);
                tcSetPC.TabPages.Remove(tpPC4);

                lblBend.Text = "PC" + CONST.PCNo + " MODEL";               
            }
            else if (CONST.PCNo == 4 || CONST.PCNo == 7)
            {
                tcSetPC.TabPages.Remove(tpPC2);
                tcSetPC.TabPages.Remove(tpPC3);
                tcSetPC.TabPages.Remove(tpPC1);

                lblInsp.Text = "PC" + CONST.PCNo + " MODEL";
            }
            else if (CONST.PCNo == 8)
            {

                tcSetPC.TabPages.Remove(tpPC2);
                tcSetPC.TabPages.Remove(tpPC3);
                tcSetPC.TabPages.Remove(tpPC4);

                tpPC1.Text = "LASER";

                lblPre.Text = "PC" + CONST.PCNo + " MODEL";

                label86.Visible = false;
                txtSCFLengthSpec.Visible = false;

                panel14.Visible = false;

                // 210216 cjm 하드코딩
                label132.Visible = false;
                cbAttachRetryUse.Visible = false;
                gbLaserTor.Visible = true;
            }

            //pcy190404 프로그램 실행시 강제미사용 변경.
            //pc2
            cbBD1EdgeToEdgeUse.Checked = false;
            cbBD2EdgeToEdgeUse.Checked = false;
            cbBD3EdgeToEdgeUse.Checked = false;
            cbBDEdgeModeSearchMark.Checked = false;
            cbBDSCFInspection.Checked = false;
            cbBDSCFDistInspection.Checked = false;
            //cbInspPanelMarkSearch.Checked = false;
            //pc1
            //cbBDPreSCFInspection.Checked = false;
            cbBDPreSCFDistInsp.Checked = false;
            cbBDPreSCFInspResultPass.Checked = false;
            cbEMILogPatternSearch.Checked = false;

            if (cbLoadPreSCFInspection1.Checked) panelLDSCF1.Visible = true; else panelLDSCF1.Visible = false;
            if (cbLoadPreSCFInspection2.Checked) panelLDSCF2.Visible = true; else panelLDSCF2.Visible = false;
            if (cbLoadPreCOFInspection1.Checked) panelLDCOF1.Visible = true; else panelLDCOF1.Visible = false;
            if (cbLoadPreCOFInspection2.Checked) panelLDCOF2.Visible = true; else panelLDCOF2.Visible = false;

            //210215 cjm D-BD, T-BD -> ArmPreAlignLimit
            //if (cboName.SelectedIndex >= 0)
            //{
            //    if (Menu.frmAutoMain.Vision[cboName.SelectedIndex].CFG.eCamName == nameof(eCamNO.Bend1_1))
            //        pnArmPreAlignLimit.Visible = true;
            //}
            // 210217 cjm Change
            //if (CONST.PCNo == 3 || CONST.PCNo == 6)
            //{
            //    pnArmPreAlignLimit.Visible = true;
                
            //}
        }


        public bool ConfigDisplay(csVision.sCFG CFG)
        {
            bool result = false;

            try
            {
                if (CFG.Use) cbUse.Checked = true;
                else cbUse.Checked = false;

                txtFovX.Text = CFG.FOVX.ToString();
                txtFovY.Text = CFG.FOVY.ToString();
                txtResolution.Text = CFG.Resolution.ToString();
                txtSerial.Text = CFG.Serial.ToString();

                txtLight1Comport.Text = CFG.Light1Comport.ToString();
                txtLight1CH.Text = CFG.Light1CH.ToString();
                txtLight5VComport.Text = CFG.Light5VComport.ToString();
                txtLight5VCH.Text = CFG.Light5VCH.ToString();
                txtLight5VValue.Text = CFG.Light5VValue.ToString();

                txtSubLight1CH.Text = CFG.SubLight1CH.ToString();
                txtSubLight1Value.Text = CFG.SubLight1Value.ToString();
                cbPatternSearchMode.SelectedIndex = (int)CFG.PatternSearchMode;
                cbPatternSearchTool.SelectedIndex = (int)CFG.PatternSearchTool;
                cbCalType.SelectedIndex = (int)CFG.CalType;
                cbLightType.SelectedIndex = (int)CFG.LightType;
                cbImageSaveType.SelectedIndex = (int)CFG.ImageSaveType;

                txtImgAutoDelDay.Text = CFG.ImgAutoDelDay.ToString();
                cbReverseMode.SelectedIndex = (int)CFG.nReverseMode;
                cbPatternSearchKind1.SelectedIndex = (int)CFG.searchKind1;
                cbPatternSearchKind2.SelectedIndex = (int)CFG.searchKind2;
                txtGrabDelay.Text = CFG.GrabDelay.ToString();
                txtGrabDelay2.Text = CFG.GrabDelay2.ToString();
                txtLightDelay.Text = CFG.LightDelay.ToString();

                txtAlignLimitX.Text = CFG.AlignLimitX.ToString();
                txtAlignLimitY.Text = CFG.AlignLimitY.ToString();
                txtAlignLimitT.Text = CFG.AlignLimitT.ToString();

                txtAlignLimitCnt.Text = CFG.RetryLimitCnt.ToString();
                txtRetryCnt.Text = CFG.RetryCnt.ToString();

                cbPatternFailManualInput.Checked = CFG.PatternFailManualIn;
                cbManualWindow.Checked = CFG.ManualWindow;
                cbImageSave.Checked = CFG.ImageSave;

                cbSideVision.Checked = CFG.SideVision;
                cbSideImgSave.Checked = CFG.SideImgSave;

                cbAlignnotUseX.Checked = CFG.AlignNotUseX;
                cbAlignnotUseY.Checked = CFG.AlignNotUseY;
                cbAlignnotUseT.Checked = CFG.AlignNotUseT;

                cbXAxisRevers.Checked = CFG.XAxisRevers;
                cbYAxisRevers.Checked = CFG.YAxisRevers;
                cbTAxisRevers.Checked = CFG.TAxisRevers;

                //210215 cjm X-Y Axis Revers add
                cbXYAxisRevers.Checked = CFG.XYAxisRevers;

                //pcy190421
                cbOffsetXReverse.Checked = CFG.OffsetXReverse;

                //pcy190729
                cbYTCalUse.Checked = CFG.YTCalUse;

                cbCenterAlign.Checked = CFG.CenterAlign;
                cbAlignUse.Checked = CFG.AlignUse;

                //210116 cjm 딥러닝 사용유무
                cbDLUse.Checked = CFG.DLUse;

                //210119 cjm ArmPre 인터락
                txtArmPreAlignLimitX.Text = CFG.ArmPreAlignLimitX.ToString();
                txtArmPreAlignLimitY.Text = CFG.ArmPreAlignLimitY.ToString();
                txtArmPreAlignLimitT.Text = CFG.ArmPreAlignLimitT.ToString();

                REVTabUpdate();
                //20200926 cjm revData -> TextBox 쓰기
                Bending.Menu.frmRecipe.revDataTotxt();

                result = true;


            }
            catch (Exception EX)
            {
                cLog.ExceptionLogSave("ConfigDispl8ay" + "," + EX.GetType().Name + "," + EX.Message);
                result = false;
            }

            return result;
        }



        private void cboName_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (cboName.SelectedItem.ToString() == "Not Use") { pnCamSet.Visible = false; pnOffset.Visible = false; pnPreModel.Visible = false; pnBendModel.Visible = false; }
            //else if(CONST.PCName == "AAM_PC1" && cboName.SelectedItem.ToString() != "Not Use") { pnCamSet.Visible = true; pnOffset.Visible = true; pnPreModel.Visible = true; pnBendModel.Visible = false; }
            //else if(CONST.PCName == "AAM_PC2" && cboName.SelectedItem.ToString() != "Not Use") { pnCamSet.Visible = true; pnOffset.Visible = true; pnPreModel.Visible = false; pnBendModel.Visible = true; }

            //if(cboName.SelectedItem.ToString() == "EMI Attach") { lblXOffset.Visible = false; txtAlignXOffset1.Visible = false; lblTOffset.Visible = false; txtAlignTOffset1.Visible = false; }
            //else { lblXOffset.Visible = true; txtAlignXOffset1.Visible = true; lblTOffset.Visible = true; txtAlignTOffset1.Visible = true; }

            //2018.05.08 khs
            pnOnceMove.Visible = false;
            pnMoveAdd.Visible = false;
            pnBending.Visible = false;
            pnBDPreOffset.Visible = false;
            pnScale.Visible = false;
            pnEdgeToEdgeScale.Visible = false;
            pnAlignOffset2.Visible = false;
            pnPressOffset.Visible = false;
            pnArmPreAlignLimit.Visible = false;
            if (cboName.SelectedIndex >= 0)
            {
                cCFG.CAMconfig_Read(cboName.SelectedIndex, ref Menu.frmAutoMain.Vision[cboName.SelectedIndex].CFG);

                ConfigDisplay(Menu.frmAutoMain.Vision[cboName.SelectedIndex].CFG);
                //2018.05.08 khs Bending 일 때 Visible true,false 구분
                //if (cboName.Text == "BENDING1-1" || cboName.Text == "BENDING2-1" || cboName.Text == "BENDING3-1")
                //if (Menu.frmAutoMain.Vision[cboName.SelectedIndex].CFG.eCamName == nameof(eCamNO.Bend1_1))
                //|| Menu.frmAutoMain.Vision[cboName.SelectedIndex].CFG.eCamName == eCamNO3.Bending2_1.ToString()
                //|| Menu.frmAutoMain.Vision[cboName.SelectedIndex].CFG.eCamName == eCamNO3.Bending3_1.ToString())
                //{
                //    pnBending.Visible = true;
                //    pnBDPreOffset.Visible = true;
                //    pnMoveAdd.Visible = false;
                //    pnOnceMove.Visible = false;
                //    pnPressOffset.Visible = true; //pcy210119
                //    pnAlignOffset2.Visible = true;
                //    pnArmPreAlignLimit.Visible = true;
                //}
                //if(Menu.frmAutoMain.Vision[cboName.SelectedIndex].CFG.eCamName == eCamNO4.EMIAttach.ToString()
                //    || Menu.frmAutoMain.Vision[cboName.SelectedIndex].CFG.eCamName == eCamNO4.TempAttach.ToString())
                //{
                //    pnAlignOffset2.Visible = true;
                //}

                //210116 cjm 딥러닝 사용유무 //임시 주석(사용안해도 세팅필요한 경우 있음)
                //if (Menu.frmAutoMain.Vision[cboName.SelectedIndex].CFG.DLUse)
                //    panel16.Visible = true;
                //else
                //    panel16.Visible = false;

            }
        }
        public void SelectedIndex(int nCam)
        {
            cboName.SelectedIndex = nCam;
        }

        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (txtPassword.Text == CONST.Password)
                {
                    pnPassword.Visible = false;
                }
                else
                    MessageBox.Show("please enter the correct password.");

                txtPassword.Text = "";
            }
        }



        public void PasswordDisplay()
        {
            lblPassword.Left = (Width / 2) - (lblPassword.Width / 2);
            txtPassword.Left = (Width / 2) - (lblPassword.Width / 2);

            pnPassword.Width = Width;
            pnPassword.Height = Height;
            pnPassword.Left = Left;
            pnPassword.Top = Top;
            pnPassword.Visible = true;
            txtPassword.Focus();
        }






        private void btnApply_Click(object sender, EventArgs e)
        {
            if (Menu.frmlogin.LogInCheck())
            {
                // config apply 하기.
                string runRecipeName = "";

                if (MessageBox.Show("Alert!.\r\n\r\nCurrent Recipe : " + runRecipeName + "\r\n\r\nThis Setting Apply?", "Information", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    // 모델 바꾸기 적용. 
                    //Menu.frmRecipe.ModelChange();
                    //노출값 바로 적용
                    //int camidx = (int)cboName.SelectedIndex;
                    //Menu.frmAutoMain.Vision[camidx].setExposure();
                }
            }
            else
            {
                MessageBox.Show("Confrm LoginUser");
            }

        }
        private void btnREVSave_Click(object sender, EventArgs e)
        {
            if (Menu.frmlogin.LogInCheck())
            {
                if (MessageBox.Show("REV Data Save?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {

                    int CamNo = cboName.SelectedIndex;
                    CamDataApply(CamNo);
                    REVDataApply();
                    for (int i = 0; i < CONST.CAMCnt; i++)
                    {
                        cCFG.CAMconfig_Write(Menu.frmAutoMain.Vision[i].CFG, i);
                    }

                    revData.WriteData(CONST.RunRecipe.RecipeName);

                    //Pre Offset Set
                    //if (CONST.PCNo == 3)
                    //{
                    //	double[] writeData = new double[3];

                    //	writeData[0] = revData.mOffset[Vision_No.vsBend1_1].BendingPreOffsetXYT.X;
                    //	writeData[1] = revData.mOffset[Vision_No.vsBend1_1].BendingPreOffsetXYT.Y;
                    //	writeData[2] = revData.mOffset[Vision_No.vsBend1_1].BendingPreOffsetXYT.T;
                    //	//Menu.frmAutoMain.IF.writeBendPreOffsetData(writeData, 0);

                    //	writeData[0] = revData.mOffset[Vision_No.vsBend2_1].BendingPreOffsetXYT.X;
                    //	writeData[1] = revData.mOffset[Vision_No.vsBend2_1].BendingPreOffsetXYT.Y;
                    //	writeData[2] = revData.mOffset[Vision_No.vsBend2_1].BendingPreOffsetXYT.T;
                    //	//Menu.frmAutoMain.IF.writeBendPreOffsetData(writeData, 1);

                    //	writeData[0] = revData.mOffset[Vision_No.vsBend3_1].BendingPreOffsetXYT.X;
                    //	writeData[1] = revData.mOffset[Vision_No.vsBend3_1].BendingPreOffsetXYT.Y;
                    //	writeData[2] = revData.mOffset[Vision_No.vsBend3_1].BendingPreOffsetXYT.T;
                    //	//Menu.frmAutoMain.IF.writeBendPreOffsetData(writeData, 2);
                    //}
                    Menu.frmAutoMain.ParamChange = true;
                }
            }
            else
            {
                MessageBox.Show("Confrm LoginUser");
            }
        }

        private void btnApply_Click_1(object sender, EventArgs e)
        {
            if (!REVOffsetDataValueOverCheck() || REVPitchDataValueOverCheck())
            {
                return;
            }

            REVDataApply();
        }

        public void SettingValueReSet()
        {
            this.btnConfigReset_Click(this, null);
        }

        //2018.05.09 khs    
        private void btnConfigReset_Click(object sender, EventArgs e)
        {
            cCFG.CAMconfig_Read(cboName.SelectedIndex, ref Menu.frmAutoMain.Vision[cboName.SelectedIndex].CFG);

            REVTabUpdate();
            ConfigDisplay(Menu.frmAutoMain.Vision[cboName.SelectedIndex].CFG);

            //20200926 cjm revData -> textBox 쓰기
            Bending.Menu.frmRecipe.revDataTotxt();
        }

        private void CamDataApply(int CamNo)
        {
            try
            {
                Menu.frmAutoMain.Vision[CamNo].CFG.Use = cbUse.Checked;

                Menu.frmAutoMain.Vision[CamNo].CFG.FOVX = double.Parse(txtFovX.Text);
                Menu.frmAutoMain.Vision[CamNo].CFG.FOVY = double.Parse(txtFovY.Text);
                Menu.frmAutoMain.Vision[CamNo].CFG.Resolution = double.Parse(txtResolution.Text);
                Menu.frmAutoMain.Vision[CamNo].CFG.Serial = txtSerial.Text;

                Menu.frmAutoMain.Vision[CamNo].CFG.Light1Comport = int.Parse(txtLight1Comport.Text);
                Menu.frmAutoMain.Vision[CamNo].CFG.Light1CH = int.Parse(txtLight1CH.Text);
                Menu.frmAutoMain.Vision[CamNo].CFG.SubLight1CH = int.Parse(txtSubLight1CH.Text);
                Menu.frmAutoMain.Vision[CamNo].CFG.SubLight1Value = int.Parse(txtSubLight1Value.Text);

                //pcy210119
                Menu.frmAutoMain.Vision[CamNo].CFG.Light5VComport = int.Parse(txtLight5VComport.Text);
                Menu.frmAutoMain.Vision[CamNo].CFG.Light5VCH = int.Parse(txtLight5VCH.Text);
                Menu.frmAutoMain.Vision[CamNo].CFG.Light5VValue = int.Parse(txtLight5VValue.Text);


                switch (cbImageSaveType.SelectedIndex)
                {
                    case 0: { Menu.frmAutoMain.Vision[CamNo].CFG.ImageSaveType = CONST.eImageSaveType.All; break; }
                    case 1: { Menu.frmAutoMain.Vision[CamNo].CFG.ImageSaveType = CONST.eImageSaveType.Original; break; }
                    case 2: { Menu.frmAutoMain.Vision[CamNo].CFG.ImageSaveType = CONST.eImageSaveType.Display; break; }
                }

                switch (cbLightType.SelectedIndex)
                {
                    case 0: { Menu.frmAutoMain.Vision[CamNo].CFG.LightType = CONST.eLightType.Light5V; break; }
                    case 1: { Menu.frmAutoMain.Vision[CamNo].CFG.LightType = CONST.eLightType.Light12V; break; }
                }

                switch (cbPatternSearchMode.SelectedIndex)
                {
                    case 0: { Menu.frmAutoMain.Vision[CamNo].CFG.PatternSearchMode = CONST.ePatternSearchMode.LastBest; break; }
                    case 1: { Menu.frmAutoMain.Vision[CamNo].CFG.PatternSearchMode = CONST.ePatternSearchMode.AllBest; break; }
                }
                switch (cbPatternSearchTool.SelectedIndex)
                {
                    case 0: { Menu.frmAutoMain.Vision[CamNo].CFG.PatternSearchTool = CONST.ePatternSearchTool.PMAlign; break; }
                    case 1: { Menu.frmAutoMain.Vision[CamNo].CFG.PatternSearchTool = CONST.ePatternSearchTool.SearchMax; break; }
                }
                //2018.11.08 khs CalType 변경
                switch (cbCalType.SelectedIndex)
                {
                    case 0: { Menu.frmAutoMain.Vision[CamNo].CFG.CalType = eCalType.Cam1Type; break; }
                    case 1: { Menu.frmAutoMain.Vision[CamNo].CFG.CalType = eCalType.Cam2Type; break; }
                    case 2: { Menu.frmAutoMain.Vision[CamNo].CFG.CalType = eCalType.Cam3Type; break; }
                    case 3: { Menu.frmAutoMain.Vision[CamNo].CFG.CalType = eCalType.Cam4Type; break; }
                    case 4: { Menu.frmAutoMain.Vision[CamNo].CFG.CalType = eCalType.Cam1Cal2; break; }
                        //case 2: { Menu.frmAutoMain.Vision[CamNo].CFG.CalType = eCalType.Cam2Point2; break; }
                        //case 3: { Menu.frmAutoMain.Vision[CamNo].CFG.CalType = eCalType.REVCal; break; }
                }

                switch (cbPatternSearchKind1.SelectedIndex)
                {
                    case 0: { Menu.frmAutoMain.Vision[CamNo].CFG.searchKind1 = eSearchKind.Pattern; break; }
                    case 1: { Menu.frmAutoMain.Vision[CamNo].CFG.searchKind1 = eSearchKind.Line; break; }
                }

                switch (cbPatternSearchKind2.SelectedIndex)
                {
                    case 0: { Menu.frmAutoMain.Vision[CamNo].CFG.searchKind2 = eSearchKind.Pattern; break; }
                    case 1: { Menu.frmAutoMain.Vision[CamNo].CFG.searchKind2 = eSearchKind.Line; break; }
                }

                switch (cbReverseMode.SelectedIndex)
                {
                    case 0: { Menu.frmAutoMain.Vision[CamNo].CFG.nReverseMode = CONST.eImageReverse.None; break; }
                    case 1: { Menu.frmAutoMain.Vision[CamNo].CFG.nReverseMode = CONST.eImageReverse.XReverse; break; }
                    case 2: { Menu.frmAutoMain.Vision[CamNo].CFG.nReverseMode = CONST.eImageReverse.YReverse; break; }
                    case 3: { Menu.frmAutoMain.Vision[CamNo].CFG.nReverseMode = CONST.eImageReverse.AllReverse; break; }
                    case 4: { Menu.frmAutoMain.Vision[CamNo].CFG.nReverseMode = CONST.eImageReverse.Reverse90; break; }
                    case 5: { Menu.frmAutoMain.Vision[CamNo].CFG.nReverseMode = CONST.eImageReverse.Reverse270; break; }
                    case 6: { Menu.frmAutoMain.Vision[CamNo].CFG.nReverseMode = CONST.eImageReverse.Reverse90XY; break; }
                    case 7: { Menu.frmAutoMain.Vision[CamNo].CFG.nReverseMode = CONST.eImageReverse.Reverse270XY; break; }
                }
                Menu.frmAutoMain.Vision[CamNo].CFG.GrabDelay = int.Parse(txtGrabDelay.Text);
                Menu.frmAutoMain.Vision[CamNo].CFG.GrabDelay2 = int.Parse(txtGrabDelay2.Text);
                Menu.frmAutoMain.Vision[CamNo].CFG.LightDelay = int.Parse(txtLightDelay.Text);
                Menu.frmAutoMain.Vision[CamNo].CFG.ImgAutoDelDay = int.Parse(txtImgAutoDelDay.Text);

                //Menu.frmAutoMain.Vision[CamNo].CFG.RefExposure = int.Parse(txtRefExposure.Text);
                //Menu.frmAutoMain.Vision[CamNo].CFG.SubExposure = int.Parse(txtSubExposure.Text);
                //Menu.frmAutoMain.Vision[CamNo].CFG.RefAutoLight = int.Parse(txtRefAutoLight.Text);
                //Menu.frmAutoMain.Vision[CamNo].CFG.SubAutoLight = int.Parse(txtSubAutoLight.Text);
                //Menu.frmAutoMain.Vision[CamNo].CFG.EdgeAutoLight = int.Parse(txtEdgeAutoLight.Text);
                //Menu.frmAutoMain.Vision[CamNo].CFG.EdgeSubAutoLight = int.Parse(txtEdgeSubAutoLight.Text);
                //pcy190521
                //Menu.frmAutoMain.Vision[CamNo].CFG.FPCBAutoLight = int.Parse(txtFPCBAutoLight.Text);
                //Menu.frmAutoMain.Vision[CamNo].CFG.FPCBSubAutoLight = int.Parse(txtFPCBSubAutoLight.Text);

                //190624 cjm
                //Menu.frmAutoMain.Vision[CamNo].CFG.RefContrast = double.Parse(txtContrast.Text);
                //Menu.frmAutoMain.Vision[CamNo].CFG.SubContrast = double.Parse(txtSubContrast.Text);

                Menu.frmAutoMain.Vision[CamNo].CFG.AlignLimitX = double.Parse(txtAlignLimitX.Text);
                Menu.frmAutoMain.Vision[CamNo].CFG.AlignLimitY = double.Parse(txtAlignLimitY.Text);
                Menu.frmAutoMain.Vision[CamNo].CFG.AlignLimitT = double.Parse(txtAlignLimitT.Text);

                Menu.frmAutoMain.Vision[CamNo].CFG.RetryLimitCnt = int.Parse(txtAlignLimitCnt.Text);
                //2018.07.10 RetryCnt 추가 khs
                Menu.frmAutoMain.Vision[CamNo].CFG.RetryCnt = int.Parse(txtRetryCnt.Text);

                Menu.frmAutoMain.Vision[CamNo].CFG.PatternFailManualIn = cbPatternFailManualInput.Checked;
                Menu.frmAutoMain.Vision[CamNo].CFG.ManualWindow = cbManualWindow.Checked;

                Menu.frmAutoMain.Vision[CamNo].CFG.ImageSave = cbImageSave.Checked;
                Menu.frmAutoMain.Vision[CamNo].CFG.SideVision = cbSideVision.Checked;
                Menu.frmAutoMain.Vision[CamNo].CFG.SideImgSave = cbSideImgSave.Checked;

                if (cbImageSave.Checked) Menu.frmAutoMain.Vision[CamNo].CFG.ImageSave = true;
                else Menu.frmAutoMain.Vision[CamNo].CFG.ImageSave = false;

                // 왜 하는지 모르겠음
                if (cbSideVision.Checked) Menu.frmAutoMain.Vision[CamNo].CFG.SideVision = true;
                else Menu.frmAutoMain.Vision[CamNo].CFG.SideVision = false;

                if (cbSideImgSave.Checked) Menu.frmAutoMain.Vision[CamNo].CFG.SideImgSave = true;
                else Menu.frmAutoMain.Vision[CamNo].CFG.SideImgSave = false;
                /////////////

                if (cbAlignnotUseX.Checked) Menu.frmAutoMain.Vision[CamNo].CFG.AlignNotUseX = true;
                else Menu.frmAutoMain.Vision[CamNo].CFG.AlignNotUseX = false;

                if (cbAlignnotUseY.Checked) Menu.frmAutoMain.Vision[CamNo].CFG.AlignNotUseY = true;
                else Menu.frmAutoMain.Vision[CamNo].CFG.AlignNotUseY = false;

                if (cbAlignnotUseT.Checked) Menu.frmAutoMain.Vision[CamNo].CFG.AlignNotUseT = true;
                else Menu.frmAutoMain.Vision[CamNo].CFG.AlignNotUseT = false;

                if (cbXAxisRevers.Checked) Menu.frmAutoMain.Vision[CamNo].CFG.XAxisRevers = true;
                else Menu.frmAutoMain.Vision[CamNo].CFG.XAxisRevers = false;

                if (cbYAxisRevers.Checked) Menu.frmAutoMain.Vision[CamNo].CFG.YAxisRevers = true;
                else Menu.frmAutoMain.Vision[CamNo].CFG.YAxisRevers = false;

                if (cbTAxisRevers.Checked) Menu.frmAutoMain.Vision[CamNo].CFG.TAxisRevers = true;
                else Menu.frmAutoMain.Vision[CamNo].CFG.TAxisRevers = false;

                //210215 cjm X-Y Axis Revers add
                if (cbXYAxisRevers.Checked) Menu.frmAutoMain.Vision[CamNo].CFG.XYAxisRevers = true;
                else Menu.frmAutoMain.Vision[CamNo].CFG.XYAxisRevers = false;

                //pcy190421
                if (cbOffsetXReverse.Checked) Menu.frmAutoMain.Vision[CamNo].CFG.OffsetXReverse = true;
                else Menu.frmAutoMain.Vision[CamNo].CFG.OffsetXReverse = false;

                //pcy190729
                if (cbYTCalUse.Checked) Menu.frmAutoMain.Vision[CamNo].CFG.YTCalUse = true;
                else Menu.frmAutoMain.Vision[CamNo].CFG.YTCalUse = false;

                if (cbCenterAlign.Checked) Menu.frmAutoMain.Vision[CamNo].CFG.CenterAlign = true;
                else Menu.frmAutoMain.Vision[CamNo].CFG.CenterAlign = false;

                if (cbAlignUse.Checked) Menu.frmAutoMain.Vision[CamNo].CFG.AlignUse = true;
                else Menu.frmAutoMain.Vision[CamNo].CFG.AlignUse = false;

                //210116 cjm 딥러닝 사용 유무
                if (cbDLUse.Checked) Menu.frmAutoMain.Vision[CamNo].CFG.DLUse = true;
                else Menu.frmAutoMain.Vision[CamNo].CFG.DLUse = false;

                //210119 cjm ArmPre 인터락
                Menu.frmAutoMain.Vision[CamNo].CFG.ArmPreAlignLimitX = double.Parse(txtArmPreAlignLimitX.Text);
                Menu.frmAutoMain.Vision[CamNo].CFG.ArmPreAlignLimitY = double.Parse(txtArmPreAlignLimitY.Text);
                Menu.frmAutoMain.Vision[CamNo].CFG.ArmPreAlignLimitT = double.Parse(txtArmPreAlignLimitT.Text);
            }
            catch
            {

            }
        }
        //2018.05.17 khs Motion Param 추가

        private void REVDataApply()
        {
            // PC1
            int CamNo = cboName.SelectedIndex;

            //pcy190404 강제미사용으로 임시 변경.
            //pc2
            cbBD1EdgeToEdgeUse.Checked = false;
            cbBD2EdgeToEdgeUse.Checked = false;
            cbBD3EdgeToEdgeUse.Checked = false;
            cbBDEdgeModeSearchMark.Checked = false;
            //cbBDSCFInspection.Checked = false;
            cbBDSCFDistInspection.Checked = false;
            //cbInspPanelMarkSearch.Checked = false;
            //pc1
            //cbBDPreSCFInspection.Checked = false;
            cbBDPreSCFDistInsp.Checked = false;
            cbBDPreSCFInspResultPass.Checked = false;
            cbEMILogPatternSearch.Checked = false;
            //pcy190608추가
            cbLDPreLineThetaUse.Checked = false;
            cbLoadPreLineSearchMode.Checked = false;

            
            revData.mOffset[CamNo].CalXYT.X = Convert.ToDouble(txtCalXOffset.Text);
            revData.mOffset[CamNo].CalXYT.Y = Convert.ToDouble(txtCalYOffset.Text);
            revData.mOffset[CamNo].CalXYT.T = Convert.ToDouble(txtCalTOffset.Text);

            revData.mOffset[CamNo].AlignOffsetXYT.X = Convert.ToDouble(txtAlignXOffset1.Text);
            revData.mOffset[CamNo].AlignOffsetXYT.Y = Convert.ToDouble(txtAlignYOffset1.Text);
            revData.mOffset[CamNo].AlignOffsetXYT.T = Convert.ToDouble(txtAlignTOffset1.Text);

            revData.mOffset[CamNo].AlignOffsetXYT2.X = Convert.ToDouble(txtAlignXOffset2.Text);
            revData.mOffset[CamNo].AlignOffsetXYT2.Y = Convert.ToDouble(txtAlignYOffset2.Text);
            revData.mOffset[CamNo].AlignOffsetXYT2.T = Convert.ToDouble(txtAlignTOffset2.Text);

            revData.mLcheck[CamNo].LCheckOffset1 = Convert.ToDouble(txtLengthOffset1.Text);
            //pcy190408 추가.
            revData.mLcheck[CamNo].LCheckOffset2 = Convert.ToDouble(txtLengthOffset2.Text);
            
            revData.mOffset[CamNo].CalFixPosOffset.X = Convert.ToDouble(txtCalFixPosOffsetX.Text);
            revData.mOffset[CamNo].CalFixPosOffset.Y = Convert.ToDouble(txtCalFixPosOffsetY.Text);

            //20.12.17 lkw DL
            //Label은 공통으로 사용 (OK,NG)
            //USE는 Ready할 때 File 있는 경우만 Use
            //pcy210201 Label은 pb파일과 항상 세트
            revData.mDL[CamNo].MarkSearch_ModelPath[0] = txtMarkSearchModel1.Text;
            revData.mDL[CamNo].MarkSearch_LabelPath[0] = SetLabelFile(revData.mDL[CamNo].MarkSearch_ModelPath[0]);
            //revData.mDL[CamNo].MarkSearch_LabelPath[0] = txtLabelPath.Text;

            revData.mDL[CamNo].MarkSearch_ModelPath[1] = txtMarkSearchModel2.Text;
            revData.mDL[CamNo].MarkSearch_LabelPath[1] = SetLabelFile(revData.mDL[CamNo].MarkSearch_ModelPath[1]);
            //revData.mDL[CamNo].MarkSearch_LabelPath[1] = txtLabelPath.Text;

            revData.mDL[CamNo].DefectFind_ModelPath[0] = txtInspectionModel1.Text;
            revData.mDL[CamNo].DefectFind_LabelPath[0] = SetLabelFile(revData.mDL[CamNo].DefectFind_ModelPath[0]);
            //revData.mDL[CamNo].DefectFind_LabelPath[0] = txtLabelPath.Text;

            revData.mDL[CamNo].DefectFind_ModelPath[1] = txtInspectionModel2.Text;
            revData.mDL[CamNo].DefectFind_LabelPath[1] = SetLabelFile(revData.mDL[CamNo].DefectFind_ModelPath[1]);
            //revData.mDL[CamNo].DefectFind_LabelPath[1] = txtInspectionModel2.Text;


            //PC1 Model
            revData.mAttach.AttachRetryUse = Convert.ToBoolean(cbAttachRetryUse.Checked);
            revData.mBendingPre.BDPreSCFInspection = Convert.ToBoolean(cbBDPreSCFInspection.Checked);
            revData.mBendingPre.SCFDistInspection = Convert.ToBoolean(cbBDPreSCFDistInsp.Checked);
            revData.mBendingPre.SCFDistInspection = Convert.ToBoolean(cbBDPreSCFDistInsp.Checked);

            revData.mSizeSpecRatio.SCFLengthSpec = Convert.ToDouble(txtSCFLengthSpec.Text);
            revData.mSizeSpecRatio.BDPreSCFCheckOffsetX = Convert.ToDouble(txtBDPreSCFInspOffsetX.Text);
            revData.mSizeSpecRatio.BDPreSCFCheckOffsetY = Convert.ToDouble(txtBDPreSCFInspOffsetY.Text);
            revData.mSizeSpecRatio.BDPreSCFAttachTH = Convert.ToDouble(txtBDPreSCFInspTH.Text);

            revData.mBendingPre.BDPreSCFMarkToEdgeY1 = Convert.ToDouble(txtBDPreInspMarkToEdgeY1.Text);
            revData.mBendingPre.BDPreSCFMarkToEdgeY2 = Convert.ToDouble(txtBDPreInspMarkToEdgeY2.Text);

            revData.mBendingPre.BDPreSCFSearchLineOffsetX1 = Convert.ToDouble(txtBDPreSearchLineOffsetX1.Text);
            revData.mBendingPre.BDPreSCFSearchLineOffsetY1 = Convert.ToDouble(txtBDPreSearchLineOffsetY1.Text);
            revData.mBendingPre.BDPreSCFSearchLineOffsetX2 = Convert.ToDouble(txtBDPreSearchLineOffsetX2.Text);
            revData.mBendingPre.BDPreSCFSearchLineOffsetY2 = Convert.ToDouble(txtBDPreSearchLineOffsetY2.Text);

            revData.mBendingPre.SCFTolerance = Convert.ToDouble(txtSCFTolerance.Text);

            //20200919 cjm Bending Pre SCF Inspection Offset 추가
            //20200926 cjm 제거중
            revData.mBendingPre.SCFExistOffsetX1 = Convert.ToDouble(txtBPSCFExistX1Offset.Text);
            revData.mBendingPre.SCFExistOffsetY1 = Convert.ToDouble(txtBPSCFExistY1Offset.Text);
            revData.mBendingPre.SCFExistTH1 = Convert.ToDouble(txtBPSCFExistTH1.Text);
            revData.mBendingPre.SCFExistOffsetX2 = Convert.ToDouble(txtBPSCFExistX2Offset.Text);
            revData.mBendingPre.SCFExistOffsetY2 = Convert.ToDouble(txtBPSCFExistY2Offset.Text);
            revData.mBendingPre.SCFExistTH2 = Convert.ToDouble(txtBPSCFExistTH2.Text);
            revData.mBendingPre.SCFExistOffsetX3 = Convert.ToDouble(txtBPSCFExistX3Offset.Text);
            revData.mBendingPre.SCFExistOffsetY3 = Convert.ToDouble(txtBPSCFExistY3Offset.Text);
            revData.mBendingPre.SCFExistTH3 = Convert.ToDouble(txtBPSCFExistTH3.Text);


            revData.mBendingPre.BDPreSCFInspResultPass = Convert.ToBoolean(cbBDPreSCFInspResultPass.Checked);
            //2018.11.29 khs
            revData.mbUseFindLine.bLoadPre = Convert.ToBoolean(cbLoadPreLineSearchMode.Checked);
            revData.mbUseFindLine.bLoadPreTheta = Convert.ToBoolean(cbLDPreLineThetaUse.Checked);

            //pcy200924
            revData.mConveyor.DivideLimitX = Convert.ToDouble(txtDivideLimitX.Text);
            revData.mConveyor.DivideLimitY = Convert.ToDouble(txtDivideLimitY.Text);
            revData.mConveyor.DivideLimitT = Convert.ToDouble(txtDivideLimitT.Text);

            //pcy190404 삭제
            //revData.mSizeSpecRatio.LDTagetPosRot = Convert.ToDouble(txtLDTagetPosRotation.Text);

            revData.mBendingPre.EMILogPatternSearch = Convert.ToBoolean(cbEMILogPatternSearch.Checked);
            revData.mSizeSpecRatio.EMITCalXMoveValue = Convert.ToDouble(txtEMITcalXMoveValue.Text);
            revData.mSizeSpecRatio.EMITCalYMoveValue = Convert.ToDouble(txtEMITcalYMoveValue.Text);

            revData.mSizeSpecRatio.SCFReelPickUpMarkPosOffsetX1 = Convert.ToDouble(txtSCFReelMarkPosX1.Text);
            revData.mSizeSpecRatio.SCFReelPickUpMarkPosOffsetY1 = Convert.ToDouble(txtSCFReelMarkPosY1.Text);
            revData.mSizeSpecRatio.SCFReelPickUpMarkPosOffsetX2 = Convert.ToDouble(txtSCFReelMarkPosX2.Text);
            revData.mSizeSpecRatio.SCFReelPickUpMarkPosOffsetY2 = Convert.ToDouble(txtSCFReelMarkPosY2.Text);
            revData.mSizeSpecRatio.SCFCenterAlign = Convert.ToBoolean(cbSCFCenterAlign.Checked);
            revData.mSizeSpecRatio.SCFXShiftRatio = Convert.ToDouble(txtSCFXShiftRatio.Text);
            revData.mSizeSpecRatio.SCFYShiftRatio = Convert.ToDouble(txtSCFYShiftRatio.Text);

            //pcy190608 퍼센트로 이동량 더주는 기능 추가.
            if (Convert.ToDouble(txtMoveX.Text) > 50)
            {
                txtMoveX.Text = "50";
                revData.mOffset[CamNo].MoveAdd.X = Convert.ToDouble(txtMoveX.Text);
            }
            else if (Convert.ToDouble(txtMoveX.Text) < -50)
            {
                txtMoveX.Text = "-50";
                revData.mOffset[CamNo].MoveAdd.X = Convert.ToDouble(txtMoveX.Text);
            }
            else
            {
                revData.mOffset[CamNo].MoveAdd.X = Convert.ToDouble(txtMoveX.Text);
            }

            if (Convert.ToDouble(txtMoveY.Text) > 50)
            {
                txtMoveY.Text = "50";
                revData.mOffset[CamNo].MoveAdd.Y = Convert.ToDouble(txtMoveY.Text);
            }
            else if (Convert.ToDouble(txtMoveY.Text) < -50)
            {
                txtMoveY.Text = "-50";
                revData.mOffset[CamNo].MoveAdd.Y = Convert.ToDouble(txtMoveY.Text);
            }
            else
            {
                revData.mOffset[CamNo].MoveAdd.Y = Convert.ToDouble(txtMoveY.Text);
            }

            if (Convert.ToDouble(txtMoveT.Text) > 50)
            {
                txtMoveT.Text = "50";
                revData.mOffset[CamNo].MoveAdd.T = Convert.ToDouble(txtMoveT.Text);
            }
            else if (Convert.ToDouble(txtMoveT.Text) < -50)
            {
                txtMoveT.Text = "-50";
                revData.mOffset[CamNo].MoveAdd.T = Convert.ToDouble(txtMoveT.Text);
            }
            else
            {
                revData.mOffset[CamNo].MoveAdd.T = Convert.ToDouble(txtMoveT.Text);
            }

            //pcy200628 이동량 제한 기능추가
            revData.mOffset[CamNo].OnceMoveLimit.X = Math.Abs(Convert.ToDouble(txtOnceMoveX.Text));
            revData.mOffset[CamNo].OnceMoveLimit.Y = Math.Abs(Convert.ToDouble(txtOnceMoveY.Text));
            revData.mOffset[CamNo].OnceMoveLimit.T = Math.Abs(Convert.ToDouble(txtOnceMoveT.Text));


            //PC2 Model
            if (int.Parse(txtBDTrans1StartGrabDelay.Text) > 1000)
            {
                txtBDTrans1StartGrabDelay.Text = "1000";
                revData.mStartGrabDelay.BDTrans1 = int.Parse(txtBDTrans1StartGrabDelay.Text);
            }
            else
            {
                revData.mStartGrabDelay.BDTrans1 = int.Parse(txtBDTrans1StartGrabDelay.Text);
            }
            if (int.Parse(txtBDTrans2StartGrabDelay.Text) > 1000)
            {
                txtBDTrans2StartGrabDelay.Text = "1000";
                revData.mStartGrabDelay.BDTrans2 = int.Parse(txtBDTrans2StartGrabDelay.Text);
            }
            else revData.mStartGrabDelay.BDTrans2 = int.Parse(txtBDTrans2StartGrabDelay.Text);

            if (int.Parse(txtBDTrans3StartGrabDelay.Text) > 1000)
            {
                txtBDTrans3StartGrabDelay.Text = "1000";
                revData.mStartGrabDelay.BDTrans3 = int.Parse(txtBDTrans3StartGrabDelay.Text);
            }
            else revData.mStartGrabDelay.BDTrans3 = int.Parse(txtBDTrans3StartGrabDelay.Text);

            if (int.Parse(txtBD1ArmStartGrabDelay.Text) > 1000)
            {
                txtBD1ArmStartGrabDelay.Text = "1000";
                revData.mStartGrabDelay.BDArm1 = int.Parse(txtBD1ArmStartGrabDelay.Text);
            }
            else revData.mStartGrabDelay.BDArm1 = int.Parse(txtBD1ArmStartGrabDelay.Text);

            if (int.Parse(txtBD2ArmStartGrabDelay.Text) > 1000)
            {
                txtBD2ArmStartGrabDelay.Text = "1000";
                revData.mStartGrabDelay.BDArm2 = int.Parse(txtBD2ArmStartGrabDelay.Text);
            }
            else revData.mStartGrabDelay.BDArm2 = int.Parse(txtBD2ArmStartGrabDelay.Text);

            if (int.Parse(txtBD3ArmStartGrabDelay.Text) > 1000)
            {
                txtBD3ArmStartGrabDelay.Text = "1000";
                revData.mStartGrabDelay.BDArm3 = int.Parse(txtBD3ArmStartGrabDelay.Text);
            }
            else revData.mStartGrabDelay.BDArm3 = int.Parse(txtBD3ArmStartGrabDelay.Text);

            revData.mBendingArm.bInitialPanelMarkSearchPass = Convert.ToBoolean(cbInitialMarkSearchPass.Checked);
            revData.mBendingArm.b180PanelMarkSearchPass = Convert.ToBoolean(cb180MarkSearchPass.Checked);
            //revData.mBendingArm.bRefMarkSearchPass[2] = Convert.ToBoolean(cbBD3RefMarkSearchPass.Checked);

            revData.mBendingArm.bSCFInspection = Convert.ToBoolean(cbBDSCFInspection.Checked);

            revData.mBendingArm.bSCFDistInspection = Convert.ToBoolean(cbBDSCFDistInspection.Checked);

            revData.mBendingArm.bDisplayLRSwapBD = Convert.ToBoolean(cbDisplayLRSwapBD.Checked);
            revData.mBendingArm.bDisplayLRSwapINSP = Convert.ToBoolean(cbDisplayLRSwapINSP.Checked);
            revData.mBendingArm.bBDUseLRSwap = Convert.ToBoolean(cbBDUseLRSwap.Checked);

            //SCF Inspection 위치 부착 offset 추가
            revData.mSizeSpecRatio.BDSCFCheckOffsetX = Convert.ToDouble(txtBDSCFInspOffsetX.Text);
            revData.mSizeSpecRatio.BDSCFCheckOffsetY = Convert.ToDouble(txtBDSCFInspOffsetY.Text);
            revData.mSizeSpecRatio.BDSCFAttachTH = Convert.ToInt32(txtBDSCFInspTH.Text);

            revData.mBendingArm.bBDEdgeToEdgeUse[0] = Convert.ToBoolean(cbBD1EdgeToEdgeUse.Checked);
            revData.mBendingArm.bBDEdgeToEdgeUse[1] = Convert.ToBoolean(cbBD2EdgeToEdgeUse.Checked);
            revData.mBendingArm.bBDEdgeToEdgeUse[2] = Convert.ToBoolean(cbBD3EdgeToEdgeUse.Checked);

            revData.mBendingArm.bInspEdgeToEdgeUse = Convert.ToBoolean(cbInspEdgeToEdgeUse.Checked);

            revData.mSizeSpecRatio.BD1AddMoveRatio = Convert.ToDouble(txtBDTransfer1FirstTOffset.Text);
            revData.mSizeSpecRatio.BD2AddMoveRatio = Convert.ToDouble(txtBDTransfer2FirstTOffset.Text);
            revData.mSizeSpecRatio.BD3AddMoveRatio = Convert.ToDouble(txtBDTransfer3FirstTOffset.Text);

            revData.mBendingArm.bBDEdgeModeSearchMark = Convert.ToBoolean(cbBDEdgeModeSearchMark.Checked);

            revData.mBendingArm.bInspEdgeModeSearchMark = Convert.ToBoolean(cbInspPanelMarkSearch.Checked);
            //pcy190521
            revData.mBendingArm.bInspFPCBSearchMark = Convert.ToBoolean(cbInspFPCBMarkSearch.Checked);

            revData.mBendingArm.bInspection3PointDistMeasrue = Convert.ToBoolean(cbInspection3PointDistMeas.Checked);

            revData.mBendingArm.bInspDistOffsetLX = Convert.ToDouble(txtInspDistOffsetLX.Text);
            revData.mBendingArm.bInspDistOffsetLY = Convert.ToDouble(txtInspDistOffsetLY.Text);
            revData.mBendingArm.bInspDistOffsetRX = Convert.ToDouble(txtInspDistOffsetRX.Text);
            revData.mBendingArm.bInspDistOffsetRY = Convert.ToDouble(txtInspDistOffsetRY.Text);

            //pcy200720
            revData.mBendingArm.bInspEdgeDistOffsetLX = Convert.ToDouble(txtInspEdgeDistOffsetLX.Text);
            revData.mBendingArm.bInspEdgeDistOffsetLY = Convert.ToDouble(txtInspEdgeDistOffsetLY.Text);
            revData.mBendingArm.bInspEdgeDistOffsetRX = Convert.ToDouble(txtInspEdgeDistOffsetRX.Text);
            revData.mBendingArm.bInspEdgeDistOffsetRY = Convert.ToDouble(txtInspEdgeDistOffsetRY.Text);

            revData.mBendingArm.bBDResultGraphics = Convert.ToBoolean(cbBDResultGraphicsUse.Checked);
            revData.mBendingArm.bInspBeforeSubmarkUse = Convert.ToBoolean(cbInspBeforeSubmarkUse.Checked);

            revData.mBendingArm.iInspMode = (eInspMode)cbInspMode.SelectedIndex;
            revData.mBendingArm.iInspFindSeq = (CONST.eInspFindSeq)cbInspFindSeq.SelectedIndex;

            revData.mBendingArm.BDToleranceX = Convert.ToDouble(txtBendToleranceX.Text);
            revData.mBendingArm.BDToleranceY = Convert.ToDouble(txtBendToleranceY.Text);
            revData.mBendingArm.InspToleranceX = Convert.ToDouble(txtInspToleranceX.Text);
            revData.mBendingArm.InspToleranceY = Convert.ToDouble(txtInspToleranceY.Text);
            revData.mSizeSpecRatio.AttachToleranceX = Convert.ToDouble(txtAttachToleranceX.Text);
            revData.mSizeSpecRatio.AttachToleranceY = Convert.ToDouble(txtAttachToleranceY.Text);

            revData.mSizeSpecRatio.LaserPositionToleranceX = Convert.ToDouble(txtMarkPosTorX.Text);
            revData.mSizeSpecRatio.LaserPositionToleranceY = Convert.ToDouble(txtMarkPosTorY.Text);

            revData.mSizeSpecRatio.LaserMarkSizeX = Convert.ToDouble(txtMarkSizeX.Text);
            revData.mSizeSpecRatio.LaserMarkSizeY = Convert.ToDouble(txtMarkSizeY.Text);
            revData.mSizeSpecRatio.LaserAlignPosTor = Convert.ToDouble(txtLaserAlignTor.Text);

            //         switch (cbInspMode.SelectedIndex)
            //{
            //	case 0: { revData.mBendingArm.iInspMode = eInspMode.PanelMarkFPCBMark; break; }
            //	case 1: { revData.mBendingArm.iInspMode = eInspMode.PanelEdgeFPCBMark; break; }
            //	case 2: { revData.mBendingArm.iInspMode = eInspMode.PanelMarkFPCBEdge; break; }
            //	case 3: { revData.mBendingArm.iInspMode = eInspMode.PanelEdgeFPCBEdge; break; }
            //}

            ////pcy190718
            //switch (cbInspFindSeq.SelectedIndex)
            //{
            //	case 0: { revData.mBendingArm.iInspFindSeq = CONST.eInspFindSeq.PanelFPCB; break; }
            //	case 1: { revData.mBendingArm.iInspFindSeq = CONST.eInspFindSeq.FPCBPanel; break; }
            //}

            revData.mBendingArm.dBDLastSpecOffset = Convert.ToDouble(txtBDLastSpecOffsetUse.Text);
            revData.mBendingArm.bBDLastSpecOffsetUse = Convert.ToBoolean(cbBDLastSpecOffsetUse.Checked);

            if (double.Parse(txtBendingFirstInNoRetrySpec.Text) > 0.5)
            {
                txtBendingFirstInNoRetrySpec.Text = "0.5";
                revData.mBendingArm.dBDFirstInNoRetrySpec = double.Parse(txtBendingFirstInNoRetrySpec.Text);
            }
            else if (double.Parse(txtBendingFirstInNoRetrySpec.Text) < 0)
            {
                txtBendingFirstInNoRetrySpec.Text = "0";
                revData.mBendingArm.dBDFirstInNoRetrySpec = double.Parse(txtBendingFirstInNoRetrySpec.Text);
            }
            else revData.mBendingArm.dBDFirstInNoRetrySpec = double.Parse(txtBendingFirstInNoRetrySpec.Text);

            revData.mBendingArm.dBDFirstInNoRetrySpec = double.Parse(txtBendingFirstInNoRetrySpec.Text);
            revData.mBendingArm.bBDFirstInNoRetryUse = Convert.ToBoolean(cbBendingFirstInNoRetryUse.Checked);
            //pcy200627
            revData.mBendingArm.dBDFirstInNGSpec = double.Parse(txtBendingFirstInNGSpec.Text);
            revData.mBendingArm.bBDFirstInNGUse = Convert.ToBoolean(cbBendingFirstInNGUse.Checked);

            //2019.07.03 LoadPre 박리 검사 
            revData.mLoadPreInsp.SCFInspOffsetX1 = double.Parse(txtLDSCFInspX1Offset.Text);
            revData.mLoadPreInsp.SCFInspOffsetY1 = double.Parse(txtLDSCFInspY1Offset.Text);
            revData.mLoadPreInsp.SCFInspOffsetX2 = double.Parse(txtLDSCFInspX2Offset.Text);
            revData.mLoadPreInsp.SCFInspOffsetY2 = double.Parse(txtLDSCFInspY2Offset.Text);
            revData.mLoadPreInsp.SCFInspTH1 = double.Parse(txtLDSCFInspTH1.Text);
            revData.mLoadPreInsp.SCFInspTH2 = double.Parse(txtLDSCFInspTH2.Text);

            revData.mLoadPreInsp.COFInspOffsetX1 = double.Parse(txtLDCOFInspX1Offset.Text);
            revData.mLoadPreInsp.COFInspOffsetY1 = double.Parse(txtLDCOFInspY1Offset.Text);
            revData.mLoadPreInsp.COFInspOffsetX2 = double.Parse(txtLDCOFInspX2Offset.Text);
            revData.mLoadPreInsp.COFInspOffsetY2 = double.Parse(txtLDCOFInspY2Offset.Text);
            revData.mLoadPreInsp.COFInspTH1 = double.Parse(txtLDCOFInspTH1.Text);
            revData.mLoadPreInsp.COFInspTH2 = double.Parse(txtLDCOFInspTH2.Text);

            revData.mLoadPreInsp.SCF1InspUse = Convert.ToBoolean(cbLoadPreSCFInspection1.Checked);
            revData.mLoadPreInsp.SCF2InspUse = Convert.ToBoolean(cbLoadPreSCFInspection2.Checked);
            revData.mLoadPreInsp.COF1InspUse = Convert.ToBoolean(cbLoadPreCOFInspection1.Checked);
            revData.mLoadPreInsp.COF2InspUse = Convert.ToBoolean(cbLoadPreCOFInspection2.Checked);

            revData.mSizeSpecRatio.SideInspSpec = double.Parse(txtSideInspectionSpec.Text);
            revData.mSizeSpecRatio.SideInspRef = double.Parse(txtSideInspectionRef.Text);

            //LaserMarking
            if (cbMCRSearch.SelectedIndex <= 0) revData.mLaser.MCRSearchKind = eID.DataMatrix;
            else revData.mLaser.MCRSearchKind = eID.QRCode;

            if (cbRefsearch.SelectedIndex <= 0) revData.mLaser.refSearch = eRefSearch.Line;
            else if (cbRefsearch.SelectedIndex == 1) revData.mLaser.refSearch = eRefSearch.Mark;
            else revData.mLaser.refSearch = eRefSearch.Blob;

            if (cbInspKind.SelectedIndex <= 0) revData.mLaser.inspKind = eInspKind.Camera;
            else revData.mLaser.inspKind = eInspKind.Mark;

            if (cbMassPosition.SelectedIndex <= 0) revData.mLaser.blobMass = eBlobMass.Left;
            else revData.mLaser.blobMass = eBlobMass.Right;

            if (cbBlobBox.SelectedIndex <= 0) revData.mLaser.blobPoint = eBlobPoint.LeftUp;
            else if (cbBlobBox.SelectedIndex == 1) revData.mLaser.blobPoint = eBlobPoint.RightUp;
            else if (cbBlobBox.SelectedIndex == 2) revData.mLaser.blobPoint = eBlobPoint.LeftDown;
            else revData.mLaser.blobPoint = eBlobPoint.RightDown;

            if (cbPolarity.SelectedIndex <= 0) revData.mLaser.polarity = ePolarity.Dark;
            else revData.mLaser.polarity = ePolarity.Light;

            revData.mLaser.MinPixel = int.Parse(txtMinPixel.Text);

            revData.mLaser.UseImageProcess = cbImageProcessing.Checked;
            revData.mLaser.MCRRight = cbMCRRight.Checked;
            revData.mLaser.MCRUp = cbMCRUp.Checked;
        }


        public void REVTabUpdate()
        {
            //2018.05.09 khs    
            int CamNo = cboName.SelectedIndex;
            try
            {
                // Offset
                txtCalXOffset.Text = revData.mOffset[CamNo].CalXYT.X.ToString("0.000");
                txtCalYOffset.Text = revData.mOffset[CamNo].CalXYT.Y.ToString("0.000");
                txtCalTOffset.Text = revData.mOffset[CamNo].CalXYT.T.ToString("0.000");

                txtAlignXOffset1.Text = revData.mOffset[CamNo].AlignOffsetXYT.X.ToString("0.000");
                txtAlignYOffset1.Text = revData.mOffset[CamNo].AlignOffsetXYT.Y.ToString("0.000");
                txtAlignTOffset1.Text = revData.mOffset[CamNo].AlignOffsetXYT.T.ToString("0.000");

                txtAlignXOffset2.Text = revData.mOffset[CamNo].AlignOffsetXYT2.X.ToString("0.000");
                txtAlignYOffset2.Text = revData.mOffset[CamNo].AlignOffsetXYT2.Y.ToString("0.000");
                txtAlignTOffset2.Text = revData.mOffset[CamNo].AlignOffsetXYT2.T.ToString("0.000");

                txtLengthOffset1.Text = revData.mLcheck[CamNo].LCheckOffset1.ToString("0.000");
                txtLengthOffset2.Text = revData.mLcheck[CamNo].LCheckOffset2.ToString("0.000");

                txtCalFixPosOffsetX.Text = revData.mOffset[CamNo].CalFixPosOffset.X.ToString("0.000");
                txtCalFixPosOffsetY.Text = revData.mOffset[CamNo].CalFixPosOffset.Y.ToString("0.000");

                //20.12.17 lkw DL
                txtLabelPath.Text = revData.mDL[CamNo].MarkSearch_LabelPath[0];
                txtMarkSearchModel1.Text = revData.mDL[CamNo].MarkSearch_ModelPath[0];
                txtMarkSearchModel2.Text = revData.mDL[CamNo].MarkSearch_ModelPath[1];
                txtInspectionModel1.Text = revData.mDL[CamNo].DefectFind_ModelPath[0];
                txtInspectionModel2.Text = revData.mDL[CamNo].DefectFind_ModelPath[1];

                // PC1Model
                cbAttachRetryUse.Checked = revData.mAttach.AttachRetryUse;
                cbBDPreSCFInspection.Checked = revData.mBendingPre.BDPreSCFInspection;
                cbBDPreSCFDistInsp.Checked = revData.mBendingPre.SCFDistInspection;
                txtSCFLengthSpec.Text = revData.mSizeSpecRatio.SCFLengthSpec.ToString("0.000");

                txtBDPreSCFInspOffsetX.Text = revData.mSizeSpecRatio.BDPreSCFCheckOffsetX.ToString("0.000");
                txtBDPreSCFInspOffsetY.Text = revData.mSizeSpecRatio.BDPreSCFCheckOffsetY.ToString("0.000");
                txtBDPreSCFInspTH.Text = revData.mSizeSpecRatio.BDPreSCFAttachTH.ToString("0");

                txtSCFReelMarkPosX1.Text = revData.mSizeSpecRatio.SCFReelPickUpMarkPosOffsetX1.ToString("0");
                txtSCFReelMarkPosY1.Text = revData.mSizeSpecRatio.SCFReelPickUpMarkPosOffsetY1.ToString("0");
                txtSCFReelMarkPosX2.Text = revData.mSizeSpecRatio.SCFReelPickUpMarkPosOffsetX2.ToString("0");
                txtSCFReelMarkPosY2.Text = revData.mSizeSpecRatio.SCFReelPickUpMarkPosOffsetY2.ToString("0");
                cbSCFCenterAlign.Checked = revData.mSizeSpecRatio.SCFCenterAlign;
                txtSCFXShiftRatio.Text = revData.mSizeSpecRatio.SCFXShiftRatio.ToString("0.000");
                txtSCFYShiftRatio.Text = revData.mSizeSpecRatio.SCFYShiftRatio.ToString("0.000");

                txtBDPreInspMarkToEdgeY1.Text = revData.mBendingPre.BDPreSCFMarkToEdgeY1.ToString("0.000");
                txtBDPreInspMarkToEdgeY2.Text = revData.mBendingPre.BDPreSCFMarkToEdgeY2.ToString("0.000");

                txtBDPreSearchLineOffsetX1.Text = revData.mBendingPre.BDPreSCFSearchLineOffsetX1.ToString("0.000");
                txtBDPreSearchLineOffsetY1.Text = revData.mBendingPre.BDPreSCFSearchLineOffsetY1.ToString("0.000");
                txtBDPreSearchLineOffsetX2.Text = revData.mBendingPre.BDPreSCFSearchLineOffsetX2.ToString("0.000");
                txtBDPreSearchLineOffsetY2.Text = revData.mBendingPre.BDPreSCFSearchLineOffsetY2.ToString("0.000");

                txtSCFTolerance.Text = revData.mBendingPre.SCFTolerance.ToString("0.000");

                cbBDPreSCFInspResultPass.Checked = revData.mBendingPre.BDPreSCFInspResultPass;
                //2018.11.29 khs
                cbLoadPreLineSearchMode.Checked = revData.mbUseFindLine.bLoadPre;
                cbLDPreLineThetaUse.Checked = revData.mbUseFindLine.bLoadPreTheta;

                //pcy190404 삭제
                //txtLDTagetPosRotation.Text = revData.mSizeSpecRatio.LDTagetPosRot.ToString("0.000");

                cbEMILogPatternSearch.Checked = revData.mBendingPre.EMILogPatternSearch;

                txtEMITcalXMoveValue.Text = revData.mSizeSpecRatio.EMITCalXMoveValue.ToString("0.000");
                txtEMITcalYMoveValue.Text = revData.mSizeSpecRatio.EMITCalYMoveValue.ToString("0.000");

                //20200919 cjm Bending Pre SCF Inspection offset 추가
                // 20200926 cjm 제거중
                txtBPSCFExistX1Offset.Text = revData.mBendingPre.SCFExistOffsetX1.ToString("0.000");
                txtBPSCFExistY1Offset.Text = revData.mBendingPre.SCFExistOffsetY1.ToString("0.000");
                txtBPSCFExistTH1.Text = revData.mBendingPre.SCFExistTH1.ToString("0.000");
                txtBPSCFExistX2Offset.Text = revData.mBendingPre.SCFExistOffsetX2.ToString("0.000");
                txtBPSCFExistY2Offset.Text = revData.mBendingPre.SCFExistOffsetY2.ToString("0.000");
                txtBPSCFExistTH2.Text = revData.mBendingPre.SCFExistTH2.ToString("0.000");
                txtBPSCFExistX3Offset.Text = revData.mBendingPre.SCFExistOffsetX3.ToString("0.000");
                txtBPSCFExistY3Offset.Text = revData.mBendingPre.SCFExistOffsetY3.ToString("0.000");
                txtBPSCFExistTH3.Text = revData.mBendingPre.SCFExistTH3.ToString("0.000");

                //PC2Model
                txtBDTrans1StartGrabDelay.Text = revData.mStartGrabDelay.BDTrans1.ToString("0");
                txtBDTrans2StartGrabDelay.Text = revData.mStartGrabDelay.BDTrans2.ToString("0");
                txtBDTrans3StartGrabDelay.Text = revData.mStartGrabDelay.BDTrans3.ToString("0");

                txtBD1ArmStartGrabDelay.Text = revData.mStartGrabDelay.BDArm1.ToString("0");
                txtBD2ArmStartGrabDelay.Text = revData.mStartGrabDelay.BDArm2.ToString("0");
                txtBD3ArmStartGrabDelay.Text = revData.mStartGrabDelay.BDArm3.ToString("0");

                cbInitialMarkSearchPass.Checked = revData.mBendingArm.bInitialPanelMarkSearchPass;
                cb180MarkSearchPass.Checked = revData.mBendingArm.b180PanelMarkSearchPass;
                //cbBD3RefMarkSearchPass.Checked = revData.mBendingArm.bRefMarkSearchPass[2];

                cbDisplayLRSwapBD.Checked = revData.mBendingArm.bDisplayLRSwapBD;
                cbDisplayLRSwapINSP.Checked = revData.mBendingArm.bDisplayLRSwapINSP;
                cbBDUseLRSwap.Checked = revData.mBendingArm.bBDUseLRSwap;

                cbBDSCFInspection.Checked = revData.mBendingArm.bSCFInspection;
                cbBDSCFDistInspection.Checked = revData.mBendingArm.bSCFDistInspection;

                txtBDSCFInspOffsetX.Text = revData.mSizeSpecRatio.BDSCFCheckOffsetX.ToString("0.000");
                txtBDSCFInspOffsetY.Text = revData.mSizeSpecRatio.BDSCFCheckOffsetY.ToString("0.000");
                txtBDSCFInspTH.Text = revData.mSizeSpecRatio.BDSCFAttachTH.ToString("0");

                cbBD1EdgeToEdgeUse.Checked = revData.mBendingArm.bBDEdgeToEdgeUse[0];
                cbBD2EdgeToEdgeUse.Checked = revData.mBendingArm.bBDEdgeToEdgeUse[1];
                cbBD3EdgeToEdgeUse.Checked = revData.mBendingArm.bBDEdgeToEdgeUse[2];

                cbInspEdgeToEdgeUse.Checked = revData.mBendingArm.bInspEdgeToEdgeUse;

                txtBDTransfer1FirstTOffset.Text = revData.mSizeSpecRatio.BD1AddMoveRatio.ToString("0.000");
                txtBDTransfer2FirstTOffset.Text = revData.mSizeSpecRatio.BD2AddMoveRatio.ToString("0.000");
                txtBDTransfer3FirstTOffset.Text = revData.mSizeSpecRatio.BD3AddMoveRatio.ToString("0.000");

                cbBDEdgeModeSearchMark.Checked = revData.mBendingArm.bBDEdgeModeSearchMark;

                cbInspPanelMarkSearch.Checked = revData.mBendingArm.bInspEdgeModeSearchMark;
                //pcy190521
                cbInspFPCBMarkSearch.Checked = revData.mBendingArm.bInspFPCBSearchMark;
                cbInspMode.SelectedIndex = Convert.ToInt32(revData.mBendingArm.iInspMode);

                //pcy190718
                cbInspFindSeq.SelectedIndex = Convert.ToInt32(revData.mBendingArm.iInspFindSeq);

                cbInspection3PointDistMeas.Checked = revData.mBendingArm.bInspection3PointDistMeasrue;

                txtInspDistOffsetLX.Text = revData.mBendingArm.bInspDistOffsetLX.ToString("0.000");
                txtInspDistOffsetLY.Text = revData.mBendingArm.bInspDistOffsetLY.ToString("0.000");
                txtInspDistOffsetRX.Text = revData.mBendingArm.bInspDistOffsetRX.ToString("0.000");
                txtInspDistOffsetRY.Text = revData.mBendingArm.bInspDistOffsetRY.ToString("0.000");

                //pcy200720
                txtInspEdgeDistOffsetLX.Text = revData.mBendingArm.bInspEdgeDistOffsetLX.ToString("0.000");
                txtInspEdgeDistOffsetLY.Text = revData.mBendingArm.bInspEdgeDistOffsetLY.ToString("0.000");
                txtInspEdgeDistOffsetRX.Text = revData.mBendingArm.bInspEdgeDistOffsetRX.ToString("0.000");
                txtInspEdgeDistOffsetRY.Text = revData.mBendingArm.bInspEdgeDistOffsetRY.ToString("0.000");

                txtBendToleranceX.Text = revData.mBendingArm.BDToleranceX.ToString("0.000");
                txtBendToleranceY.Text = revData.mBendingArm.BDToleranceY.ToString("0.000");
                txtInspToleranceX.Text = revData.mBendingArm.InspToleranceX.ToString("0.000");
                txtInspToleranceY.Text = revData.mBendingArm.InspToleranceY.ToString("0.000");

                txtAttachToleranceX.Text = revData.mSizeSpecRatio.AttachToleranceX.ToString("0.000");
                txtAttachToleranceY.Text = revData.mSizeSpecRatio.AttachToleranceY.ToString("0.000");

                txtMarkPosTorX.Text = revData.mSizeSpecRatio.LaserPositionToleranceX.ToString("0.000");
                txtMarkPosTorY.Text = revData.mSizeSpecRatio.LaserPositionToleranceY.ToString("0.000");

                txtMarkSizeX.Text = revData.mSizeSpecRatio.LaserMarkSizeX.ToString("0.000");
                txtMarkSizeY.Text = revData.mSizeSpecRatio.LaserMarkSizeY.ToString("0.000");
                txtLaserAlignTor.Text = revData.mSizeSpecRatio.LaserAlignPosTor.ToString("0.000");

                cbBDResultGraphicsUse.Checked = revData.mBendingArm.bBDResultGraphics;
                cbInspBeforeSubmarkUse.Checked = revData.mBendingArm.bInspBeforeSubmarkUse;

                txtMoveX.Text = revData.mOffset[CamNo].MoveAdd.X.ToString("0.000");
                txtMoveY.Text = revData.mOffset[CamNo].MoveAdd.Y.ToString("0.000");
                txtMoveT.Text = revData.mOffset[CamNo].MoveAdd.T.ToString("0.000");

                //pcy200628
                txtOnceMoveX.Text = Math.Abs(revData.mOffset[CamNo].OnceMoveLimit.X).ToString("0.000");
                txtOnceMoveY.Text = Math.Abs(revData.mOffset[CamNo].OnceMoveLimit.Y).ToString("0.000");
                txtOnceMoveT.Text = Math.Abs(revData.mOffset[CamNo].OnceMoveLimit.T).ToString("0.000");

                txtBDLastSpecOffsetUse.Text = revData.mBendingArm.dBDLastSpecOffset.ToString("0.000");
                cbBDLastSpecOffsetUse.Checked = revData.mBendingArm.bBDLastSpecOffsetUse;

                txtBendingFirstInNoRetrySpec.Text = revData.mBendingArm.dBDFirstInNoRetrySpec.ToString("0.000");
                cbBendingFirstInNoRetryUse.Checked = revData.mBendingArm.bBDFirstInNoRetryUse;
                //pcy200627
                txtBendingFirstInNGSpec.Text = revData.mBendingArm.dBDFirstInNGSpec.ToString("0.000");
                cbBendingFirstInNGUse.Checked = revData.mBendingArm.bBDFirstInNGUse;

                //2019.07.03 LoadPre 박리 검사 
                txtLDSCFInspX1Offset.Text = revData.mLoadPreInsp.SCFInspOffsetX1.ToString("0.0");
                txtLDSCFInspY1Offset.Text = revData.mLoadPreInsp.SCFInspOffsetY1.ToString("0.0");
                txtLDSCFInspX2Offset.Text = revData.mLoadPreInsp.SCFInspOffsetX2.ToString("0.0");
                txtLDSCFInspY2Offset.Text = revData.mLoadPreInsp.SCFInspOffsetY2.ToString("0.0");
                txtLDSCFInspTH1.Text = revData.mLoadPreInsp.SCFInspTH1.ToString("0.0");
                txtLDSCFInspTH2.Text = revData.mLoadPreInsp.SCFInspTH2.ToString("0.0");

                txtLDCOFInspX1Offset.Text = revData.mLoadPreInsp.COFInspOffsetX1.ToString("0.0");
                txtLDCOFInspY1Offset.Text = revData.mLoadPreInsp.COFInspOffsetY1.ToString("0.0");
                txtLDCOFInspX2Offset.Text = revData.mLoadPreInsp.COFInspOffsetX2.ToString("0.0");
                txtLDCOFInspY2Offset.Text = revData.mLoadPreInsp.COFInspOffsetY2.ToString("0.0");
                txtLDCOFInspTH1.Text = revData.mLoadPreInsp.COFInspTH1.ToString("0.0");
                txtLDCOFInspTH2.Text = revData.mLoadPreInsp.COFInspTH2.ToString("0.0");

                cbLoadPreSCFInspection1.Checked = revData.mLoadPreInsp.SCF1InspUse;
                cbLoadPreSCFInspection2.Checked = revData.mLoadPreInsp.SCF2InspUse;
                cbLoadPreCOFInspection1.Checked = revData.mLoadPreInsp.COF1InspUse;
                cbLoadPreCOFInspection2.Checked = revData.mLoadPreInsp.COF2InspUse;

                txtSideInspectionSpec.Text = revData.mSizeSpecRatio.SideInspSpec.ToString("0.000");
                txtSideInspectionRef.Text = revData.mSizeSpecRatio.SideInspRef.ToString("0.000");

                //pcy200924
                txtDivideLimitX.Text = revData.mConveyor.DivideLimitX.ToString("0.000");
                txtDivideLimitY.Text = revData.mConveyor.DivideLimitY.ToString("0.000");
                txtDivideLimitT.Text = revData.mConveyor.DivideLimitT.ToString("0.000");

                //LaserMarking..
                cbMCRSearch.SelectedIndex = (int)revData.mLaser.MCRSearchKind;

                cbRefsearch.SelectedIndex = (int)revData.mLaser.refSearch;
                cbInspKind.SelectedIndex = (int)revData.mLaser.inspKind;

                cbMassPosition.SelectedIndex = (int)revData.mLaser.blobMass;
                cbBlobBox.SelectedIndex = (int)revData.mLaser.blobPoint;

                cbPolarity.SelectedIndex = (int)revData.mLaser.polarity;
                txtMinPixel.Text = revData.mLaser.MinPixel.ToString();

                cbImageProcessing.Checked = revData.mLaser.UseImageProcess;
                cbMCRRight.Checked = revData.mLaser.MCRRight;
                cbMCRUp.Checked = revData.mLaser.MCRUp;
            }
            catch
            {

            }
            

        }

        private string SetLabelFile(string Model)
        {
            if (Model != "")
            {
                string templabel;
                templabel = Model.Replace("\\Model\\", "\\Label\\");
                templabel = templabel.Replace(".pb", ".txt");
                if (!File.Exists(templabel))
                {
                    DirectoryInfo temp = Directory.GetParent(templabel);
                    FileInfo[] temp2 = temp.GetFiles("*.txt");
                    if (temp2.Length > 0) File.Copy(temp2[0].FullName, templabel);
                }
                return templabel;
                //revData.mDL[CamNo].MarkSearch_LabelPath[0] = txtLabelPath.Text;
            }
            else
                return "";
        }

        public bool REVOffsetDataValueOverCheck()
        {
            //switch (CONST.PCName)
            //{
            //확인
            //case "AAM_PC1":
            //    {
            //        double[] dData = new double[30];
            //        string[] strName = new string[30];

            //        string strReturn = "";
            //        if (!REVDataValueOverCheck(dData, strName, 5.0, ref strReturn))
            //        {
            //            MessageBox.Show(strReturn + "ValueOver < 5");
            //            return false;
            //        }

            //        break;
            //    }

            //case "AAM_PC2":
            //    {
            //        double[] dData = new double[20];
            //        string[] strName = new string[20];
            //        double[] dData1 = new double[2];
            //        string[] strName1 = new string[2];

            //        string strReturn = "";
            //        string strReturn1 = "";
            //        bool bReturn = false;
            //        bool bReturn1 = false;

            //        bReturn = REVDataValueOverCheck(dData, strName, 5.0, ref strReturn);
            //        bReturn1 = REVDataValueOverCheck(dData1, strName1, 1.0, ref strReturn1);

            //        if (!bReturn|| !bReturn1)
            //        {
            //            if(!bReturn && bReturn1)
            //                MessageBox.Show("Limit Over\r\n" + strReturn + "ValueOver < 3");
            //            else if (bReturn && !bReturn1)
            //                MessageBox.Show("Limit Over\r\n" + strReturn1 + "ValueOver < 1");
            //            else if (!bReturn && !bReturn1)
            //                MessageBox.Show("Limit Over\r\n" + strReturn + "ValueOver < 3 \r\n" + strReturn1 + "ValueOver < 1");
            //            return false;
            //        }

            //        break;
            //    }

            //case "D_PC3":
            //    {
            //        double[] dData = new double[6];
            //        string[] strName = new string[6];

            //        double[] dData1 = new double[1];
            //        string[] strName1 = new string[1];

            //        string strReturn = "";
            //        string strReturn1 = "";
            //        bool bReturn = false;
            //        bool bReturn1 = false;

            //        bReturn = REVDataValueOverCheck(dData, strName, 3.0, ref strReturn);
            //        bReturn1 = REVDataValueOverCheck(dData1, strName1, 1.0, ref strReturn1);

            //        if (!bReturn || !bReturn1)
            //        {
            //            if (!bReturn && bReturn1)
            //                MessageBox.Show("Limit Over\r\n" + strReturn + "ValueOver < 3");
            //            else if (bReturn && !bReturn1)
            //                MessageBox.Show("Limit Over\r\n" + strReturn1 + "ValueOver < 1");
            //            else if (!bReturn && !bReturn1)
            //                MessageBox.Show("Limit Over\r\n" + strReturn + "ValueOver < 3 \r\n" + strReturn1 + "ValueOver < 1");
            //            return false;
            //        }

            //        break;
            //    }
            //}

            return true;
        }

        public bool REVPitchDataValueOverCheck()
        {
            //switch (CONST.PCName)
            //{
            //확인
            //case "AAM_PC1":
            //    {
            //        double[] dData = new double[15];
            //        string[] strName = new string[15];

            //        string strReturn = "";
            //        if (!REVDataValueOverCheck(dData, strName, 5.0, ref strReturn))
            //        {
            //            MessageBox.Show(strReturn + "ValueOver < 5");
            //            return false;
            //        }

            //        break;
            //    }

            //case "AAM_PC2":
            //case "D_PC3":
            //    {
            //        double[] dData = new double[9];
            //        string[] strName = new string[9];


            //        string strReturn = "";
            //        if (!REVDataValueOverCheck(dData, strName, 3.0, ref strReturn))
            //        {
            //            MessageBox.Show(strReturn + "ValueOver < 3");
            //            return false;
            //        }

            //        break;
            //    }
            //}

            return true;
        }

        public bool REVDataValueOverCheck(double[] dValueData, string[] strName, double dSpec, ref string strReturn)
        {
            string strData = "";
            bool bValueOver = true;

            for (int i = 0; i < dValueData.Length; i++)
            {
                if (-dSpec > dValueData[i] || dValueData[i] > dSpec)
                {
                    strData = strData + strName[i] + " : " + dValueData[i] + "\r\n";
                    bValueOver = false;
                }
            }

            strReturn = strData;
            return bValueOver;
        }

        private void lblExposure_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "First Camera Expose Value";
        }

        private void lblFovX_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Camera FOV X";
        }

        private void lblFovY_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Camera FOV Y";
        }

        private void lblResolution_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Camera Resolution";
        }

        private void lblSerial_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Camera Serial No";
        }

        private void lblReverseMode_Click(object sender, EventArgs e)
        {

            rtxtcontext.Text = "Camera View 방향";
        }

        private void lblImageSaveType_Click(object sender, EventArgs e)
        {

        }

        private void lblSubExposure_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Second Camera Expose Value";
        }

        private void lblRefAutoLight_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "First Light Value";
        }

        private void lblSubAutoLight_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Second Light Value";
        }

        private void lblEdgeAutoLight_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "When Find Edge, First Light Value";
        }

        private void lblEdgeSubAutoLight_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "When Find Edge, Second Light Value";
        }

        private void lblFPCBAutoLight_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "When Find FPCB Edge, First Light Value";
        }

        private void lblFPCBSubAutoLight_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "When Find FPCB Edge, Second Light Value";
        }

        private void lblAlignLimitX_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "When Align, Software Limit X,Y,T(Align값이 너무 크면 Error를 띄우기 위한 값)";
        }

        private void lblAlignLimitCnt_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Align Try Times";
        }

        private void lblRetryCnt_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Camera Capture Times";
        }

        private void lblPatternFailManualInput_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Manual Mark On/Off";
        }

        private void lblAlignnotUseX_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Not Use Align Result X,Y,T (Align 사용안함)";
        }

        private void lblOffsetXReverse_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Bending Align X Reverse(벤딩에만 사용)";
        }

        private void lblXOffset_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Align Offset X,Y,T";
        }

        private void lblCalXOffset_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "When Camera Calibration, Move Length X,Y,T";
        }

        private void lblFirstY1Offset_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Bending LeftCam Y Offset";
        }

        private void lblFirstY2Offset_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Bending RightCam Y Offset";
        }

        private void lblLastY1Offset_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Bending Left Y Spec Offset";
        }

        private void lblLastY2Offset_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Bending Right Y Spec Offset";
        }

        private void lblCalFixPosOffsetX_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "When Calibration, Length X CameraCenter <-> Cal Pattern";
        }

        private void lblCalFixPosOffsetY_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "When Calibration, Length Y CameraCenter <-> Cal Pattern";
        }

        private void lblBDTrans1StartGrabDelay_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Transfer1,2,3 Start Grab Delay(1 times)";
        }

        private void lblBD1ArmStartGrabDelay_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "BDArm1,2,3 Start Grab Delay(1 times)";
        }

        private void lblBD1RefMarkSearchPass_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "BDArm1,2,3 Panel Mark Capture Only 1 Times(첫 트랜스퍼 진입 때 ref마크 찍은것 align시 계속 사용)";
        }

        private void lblInspMode_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Inspection Mode Select(Need Setting Case by Case)";
        }

        private void lblBDResultGraphicsUse_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Display Last Bending Length";
        }

        private void lblInspDistOffsetLX_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Inspection Data Offset LX,LY,RX,RY(Inspection거리값에 offset 부여)";
        }

        private void lblLoadPreLineSearchMode_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "When Load Pre Pattern Search Fail, Try Line Search";
        }

        private void lblGrabDelay_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "매 촬상시 찍기 전 Delay";
        }

        private void lblLightDelay_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "조명 변경시 변경 전 Delay";
        }

        private void lblMoveAddX_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "Align 계산량보다 로봇에 이동량을 퍼센트로 더 주기위한 기능";
        }

        private void lblBDLastSpecOffsetUse_Click(object sender, EventArgs e)
        {
            rtxtcontext.Text = "마지막 Align시 Spec에 Offset을 주어 붙이는 기능";
        }

        private void cbLoadPreSCFInspection_CheckedChanged(object sender, EventArgs e)
        {
            if (cbLoadPreSCFInspection1.Checked) panelLDSCF1.Visible = true; else panelLDSCF1.Visible = false;

        }

        private void cbLoadPreSCFInspection2_CheckedChanged(object sender, EventArgs e)
        {
            if (cbLoadPreSCFInspection2.Checked) panelLDSCF2.Visible = true; else panelLDSCF2.Visible = false;
        }

        private void cbLoadPreCOFInspection1_CheckedChanged(object sender, EventArgs e)
        {
            if (cbLoadPreCOFInspection1.Checked) panelLDCOF1.Visible = true; else panelLDCOF1.Visible = false;
        }

        private void cbLoadPreCOFInspection2_CheckedChanged(object sender, EventArgs e)
        {
            if (cbLoadPreCOFInspection2.Checked) panelLDCOF2.Visible = true; else panelLDCOF2.Visible = false;
        }

        private void lbLabelPath_Click(object sender, EventArgs e)
        {
            //20.12.17 lkw DL
            //pcy210111 라벨명을 항상 pb파일명과 일치시켜야함
            OpenFileDialog openFile = new OpenFileDialog();
            string sName = (sender as Label).Name;
            openFile.RestoreDirectory = true;
            openFile.ShowDialog();
            if (openFile.FileNames.Length > 0)
            {
                if (sName == lbLabelPath.Name) txtLabelPath.Text = openFile.FileName;
                else if (sName == lbMarkSearchModel1.Name) txtMarkSearchModel1.Text = openFile.FileName;
                else if (sName == lbMarkSearchModel2.Name) txtMarkSearchModel2.Text = openFile.FileName;
                else if (sName == lbInspectionModel1.Name) txtInspectionModel1.Text = openFile.FileName;
                else if (sName == lbInspectionModel2.Name) txtInspectionModel2.Text = openFile.FileName;
            }
            else
            {
                //미사용
                if (sName == lbLabelPath.Name) txtLabelPath.Text = "";
                else if (sName == lbMarkSearchModel1.Name) txtMarkSearchModel1.Text = "";
                else if (sName == lbMarkSearchModel2.Name) txtMarkSearchModel2.Text = "";
                else if (sName == lbInspectionModel1.Name) txtInspectionModel1.Text = "";
                else if (sName == lbInspectionModel2.Name) txtInspectionModel2.Text = "";

            }
        }

        private void btnDLRestart_Click(object sender, EventArgs e)
        {
            //20.12.17 lkw DL  Interlock.??
            Menu.DLStart();
        }

        private void cbDisplayLRSwapINSP_CheckedChanged(object sender, EventArgs e)
        {
            if(cbDisplayLRSwapINSP.Checked == true)
            {
                label139.Visible = true;
                cbBDUseLRSwap.Visible = true;
            }
            else
            {
                label139.Visible = false;
                cbBDUseLRSwap.Visible = false;
                cbBDUseLRSwap.Checked = false;
            }
        }
    }
}
