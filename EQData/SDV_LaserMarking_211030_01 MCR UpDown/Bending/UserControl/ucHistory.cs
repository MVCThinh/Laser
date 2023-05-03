using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.ImageProcessing;

namespace Bending
{
    public partial class ucHistory : UserControl
    {
        csLog cLog = new csLog();

        double[] LogDataX1;
        double[] LogDataY1;
        double[] LogDataX2;
        double[] LogDataY2;

        double[] Max = new double[4];
        double[] Min = new double[4];
        double[] Gap = new double[4];
        double[] Avg = new double[4];
        double[] CenterValue = new double[4];
        double[] StandardDeviation = new double[4];
        double[] CP = new double[4];
        double[] CPK = new double[4];

        bool firstCheck = false;
        int StartPoint = 0;

        CogDisplay[] cogDS;

        LogKind hLogKind = new LogKind();

        string[] BmpFile1;
        string[] BmpFile2;

        Bitmap[] ImageLog1;
        Bitmap[] ImageLog2;

        public ucHistory()
        {
            InitializeComponent();

            cogDS = new CogDisplay[] { cogDS1, cogDS2 };
        }

        public void HistoryInitial()
        {
            if (CONST.PCNo == 3 || CONST.PCNo == 4)
            {
                listCamera.Items.Clear();
                listCamera.Items.Add("BENDING");
                foreach(var s in Menu.frmAutoMain.Vision)
                {
                    if(s.CFG.Use) listCamera.Items.Add(s.CFG.Name);
                }
                listCamera.Items.Add("INSPECTION_BD1");
                listCamera.Items.Add("INSPECTION_BD2");
                listCamera.Items.Add("INSPECTION_BD3");
            }
            else
            {
                listCamera.Items.Clear();

                foreach (var s in Menu.frmAutoMain.Vision)
                {
                    if (s.CFG.Use) listCamera.Items.Add(s.CFG.Name);
                }
            }

            cbLimitCountSet_CheckedChanged_1(this, null);
            rbOriginalImg.Checked = true;
            cbImgDisp.Checked = false;
            cbImgDisp_CheckedChanged(this, null);

            linfo.Clear();
            string[] sdirectories = Directory.GetDirectories(csLog.cLogPath);

            foreach (var s in sdirectories)
            {
                DirectoryInfo info = new DirectoryInfo(s);
                linfo.Add(info);
                listResult.Items.Add(info.Name);
            }
        }

        private void btnLogDisp_Click(object sender, EventArgs e)
        {
            bool NotFindFile = false;
            StartPoint = 0;

            if (listCamera.SelectedIndex > -1)
            {
                if(listResult.SelectedIndex <= -1)
                {
                    MessageBox.Show("Please Choose Result List");
                }

                if (listResult.SelectedIndex > -1)
                {
                    string Date = dtpLogStart.Value.ToShortDateString();
                    int LimitCnt = 0;
                    bool OriginalImage = false;
                    string CamSelectName = listCamera.SelectedItem.ToString(); 
                    string ResultSelectName = listResult.SelectedItem.ToString();

                    if (cbLimitCountSet.Checked)
                    {
                        if (txtCount.Text != "") LimitCnt = Int32.Parse(txtCount.Text);
                        else
                        {
                            MessageBox.Show("Input Limit Count Text Box Value");
                            return;
                        }
                    }
                    if (rbOriginalImg.Checked) OriginalImage = true;

                    if (ResultSelectName == "History") hLogKind = LogKind.BDHistory;
                    else Getlogkind(CamSelectName);

                    // Log Dispaly
                    //DeleteButtonDisp(ResultSelectName);
                    LogDisp(ResultSelectName, CamSelectName);
                    LogDataDispaly(CamSelectName, ResultSelectName, Date, hLogKind, cbLimitCountSet.Checked, LimitCnt);
                    
                    //if (CamSelectName.Substring(0, 7) != "BENDING")
                    if (!CamSelectName.Contains("BENDING"))
                    {
                        if ((ResultSelectName != "All" && ResultSelectName != "History") && dgvLog.Rows.Count > 0)
                            dgvLogCPKDataWrite(ResultSelectName, CamSelectName);
                        else dgvCPK.Rows.Clear();
                    }
                    //else if (CamSelectName.Substring(0, 7) == "BENDING")
                    else
                    {
                        if ((ResultSelectName != "All" && ResultSelectName != "History" && ResultSelectName != "Dist") && dgvLog.Rows.Count > 0)
                            dgvLogCPKDataWrite(ResultSelectName, CamSelectName);
                        else dgvCPK.Rows.Clear();
                    }

                    //Image Display
                    if (cbImgDisp.Checked)
                    {
                        if (CamSelectName != "BENDING")
                        {
                            GetImageFile(CamSelectName, ref NotFindFile, OriginalImage);
                            if (!NotFindFile) SetImageFile(1);
                        }
                        else
                        {
                            ImageLogInitial(NotFindFile);
                        }
                    }
                    else ImageLogInitial(false, true);
                }
                else MessageBox.Show("Please select a result type.");
            }
            else MessageBox.Show("please select the type of camera.");
        }

        public void ImageLogInitial(bool NotFindFile = false, bool NotUseImgLog = false)
        {
            cogDS[0].Image = null;
            cogDS[1].Image = null;
            lblCurrentCnt.Text = "0";
            lblTotalCnt.Text = "0";
            if (!NotUseImgLog)
            {
                if (!NotFindFile) lblMessage.Text = "Please select a bending number.";
                else lblMessage.Text = "The image file was not found.";
            }
            else lblMessage.Text = "";
            lblMessage.Font = new Font("Arial", 20, FontStyle.Bold);
        }

        private void cbLimitCountSet_CheckedChanged_1(object sender, EventArgs e)
        {
            if (cbLimitCountSet.Checked == false)
            {
                txtCount.Enabled = false;
                txtCount.Text = "";
            }
            else
            {
                txtCount.Enabled = true;
                txtCount.Text = "0";
            }
        }

        private void listCamera_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                //다음에
                //listResult.Items.Clear();

                //string CAMName = listCamera.SelectedItem.ToString();
                //if (CONST.PCName == "AAM_PC1")
                //{
                //    if (listCamera.SelectedIndex > -1)
                //    {
                //        if (CAMName == "LOAD PRE" || CAMName == "Detach Pre")
                //        {
                //            Getlogkind(CAMName);

                //            listResult.Items.Add("All");
                //            listResult.Items.Add("PixelMark");
                //        }
                //    }
                //}
                //else if (CONST.PCName == "AAM_PC2")
                //{
                //    if (listCamera.SelectedIndex > -1)
                //    {
                //        if (CAMName == "BENDING" || CAMName == "BENDING 1" || CAMName == "BENDING 2" || CAMName == "BENDING 3")
                //        {
                //            Getlogkind(CAMName);

                //            listResult.Items.Add("All");
                //            listResult.Items.Add("PixelMark");
                //            listResult.Items.Add("PixelRef");
                //            listResult.Items.Add("Dist");
                //            listResult.Items.Add("History");
                //        }
                //        else if (CAMName == "INSPECTION" || CAMName == "INSPECTION_BD1" || CAMName == "INSPECTION_BD2" || CAMName == "INSPECTION_BD3")
                //        {
                //            Getlogkind(CAMName);

                //            listResult.Items.Add("All");
                //            listResult.Items.Add("PixelMark");
                //            listResult.Items.Add("PixelRef");
                //            listResult.Items.Add("Dist");
                //        }
                //    }
                //}
            }
            catch { }
        }

        public void Getlogkind(string CamName)
        {
            if (CamName == "LOAD PRE") hLogKind = LogKind.AlignLoadPre;
            else if (CamName == "Detach Pre") hLogKind = LogKind.AlignPre;
            else if (CamName == "BENDING" || CamName == "BENDING 1" || CamName == "BENDING 2" || CamName == "BENDING 3")
                hLogKind = LogKind.BDRobot;
            else if (CamName == "INSPECTION" || CamName == "INSPECTION_BD1" || CamName == "INSPECTION_BD2" || CamName == "INSPECTION_BD3")
                hLogKind = LogKind.UpperInspection;
        }

        public void LogDisp(string Kind, string CamName)
        {
            if (CamName.Substring(0, 7) != "BENDING")
            {
                if (Kind == "Dist" || Kind == "PixelMark" || Kind == "PixelRef")
                {
                    dgvCPK.Visible = true;
                    dgvLog.Size = new Size(749, 648);
                }
                else
                {
                    dgvCPK.Visible = false;
                    dgvLog.Size = new Size(749, 835);
                }
            }
            else
            {
                if (Kind == "PixelMark" || Kind == "PixelRef")
                {
                    dgvCPK.Visible = true;
                    dgvLog.Size = new Size(749, 648);
                }
                else
                {
                    dgvCPK.Visible = false;
                    dgvLog.Size = new Size(749, 835);
                }
            }
        }
        public void LogDataDispaly(string CamName, string Kind, string Date, LogKind logkind, bool LimitCntUse, int LimitCnt = 0)
        {
            string LogPath = Path.Combine(csLog.cLogPath, logkind.ToString(), Date);
            firstCheck = true;
            int[] LogDataNum = new int[30];
            int TotalCnt = 0;
            dgvLog.Rows.Clear();
            dgvLog.Columns.Clear();

            try
            {
                if (Directory.Exists(LogPath))
                {
                    string[] file = Directory.GetFiles(LogPath);

                    for (int i = 0; i < file.Length; i++)
                    {
                        FileStream LogFile = new FileStream(file[i], FileMode.OpenOrCreate, FileAccess.Read);
                        StreamReader FileReader = new StreamReader(LogFile, Encoding.UTF8);

                        string BendName = listCamera.SelectedItem.ToString().Trim();
                        string BendNo = "";
                        int _row = 0;
                        string[] strData = File.ReadAllLines(file[i]);


                        for (int j = 1; j < strData.Length; j++)
                        {
                            string[] Data;
                            string[] HeadName;
                            string[] ArrayHeadData = new string[30];
                            string[] ArrayData = new string[30];

                            if (Kind == "All" || Kind == "History")
                            {
                                HeadName = strData[0].Split(',');
                                Data = strData[j].Split(',');
                                if (CamName == "BENDING 1" || CamName == "BENDING 2" || CamName == "BENDING 3")
                                {
                                    BendNo = BendName.Substring(BendName.Length - 1).Trim();
                                    if (Data[2].Trim() == BendNo)
                                    {
                                        dgvLogDataWrite(HeadName, Data, _row, Data.Length);
                                        if (LimitCntUse && _row == LimitCnt - 1)
                                        {
                                            TotalCnt++;
                                            break;
                                        }
                                        else
                                        {
                                            _row++;
                                            TotalCnt++;
                                        }
                                    }
                                }
                                else if (CamName == "INSPECTION_BD1" || CamName == "INSPECTION_BD2" || CamName == "INSPECTION_BD3")
                                {
                                    BendNo = BendName.Substring(BendName.Length - 1).Trim();
                                    if (Data[3].Trim() == BendNo)
                                    {
                                        dgvLogDataWrite(HeadName, Data, _row, Data.Length);
                                        if (LimitCntUse && _row == LimitCnt - 1)
                                        {
                                            TotalCnt++;
                                            break;
                                        }
                                        else
                                        {
                                            _row++;
                                            TotalCnt++;
                                        }
                                    }
                                }
                                else
                                {
                                    dgvLogDataWrite(HeadName, Data, _row, Data.Length);
                                    if (LimitCntUse && _row == LimitCnt - 1)
                                    {
                                        TotalCnt++;
                                        break;
                                    }
                                    else
                                    {
                                        _row++;
                                        TotalCnt++;
                                    }
                                }
                            }
                            else if (Kind == "PixelRef" || Kind == "PixelMark" || Kind == "Dist")
                            {
                                HeadName = strData[0].Split(',');
                                Data = strData[j].Split(',');

                                LogHeadDataArray(Kind, HeadName, ref LogDataNum, ref ArrayHeadData);
                                LogDataArrayData(Data, ref LogDataNum, ref ArrayData);

                                if (CamName == "BENDING 1" || CamName == "BENDING 2" || CamName == "BENDING 3")
                                {
                                    BendNo = BendName.Substring(BendName.Length - 1).Trim();
                                    if (Data[2].Trim() == BendNo)
                                    {
                                        dgvLogDataWrite(ArrayHeadData, ArrayData, _row, ArrayHeadData.Length);
                                        if (LimitCntUse && _row == LimitCnt - 1)
                                        {
                                            TotalCnt++;
                                            break;
                                        }
                                        else
                                        {
                                            _row++;
                                            TotalCnt++;
                                        }
                                    }
                                }
                                else if (CamName == "INSPECTION_BD1" || CamName == "INSPECTION_BD2" || CamName == "INSPECTION_BD3")
                                {
                                    BendNo = BendName.Substring(BendName.Length - 1).Trim();
                                    if (Data[3].Trim() == BendNo)
                                    {
                                        dgvLogDataWrite(ArrayHeadData, ArrayData, _row, ArrayHeadData.Length);
                                        if (LimitCntUse && _row == LimitCnt - 1)
                                        {
                                            TotalCnt++;
                                            break;
                                        }
                                        else
                                        {
                                            _row++;
                                            TotalCnt++;
                                        }
                                    }
                                }
                                else
                                {
                                    dgvLogDataWrite(ArrayHeadData, ArrayData, _row, ArrayHeadData.Length);
                                    if (LimitCntUse && _row == LimitCnt - 1)
                                    {
                                        TotalCnt++;
                                        break;
                                    }
                                    else
                                    {
                                        _row++;
                                        TotalCnt++;
                                    }
                                }
                            }
                        }
                        lblTotalCellCnt.Text = TotalCnt.ToString();
                        //lblTotalCellCnt.Text = TotalCnt.ToString();
                    }
                }
                else
                {
                    MessageBox.Show("The log file for that date not exist.");
                }
            }
            catch { }
        }
        public void dgvLogCPKDataWrite(string Kind, string CamName)
        {
            int Cnt = 0;
            string KindName = "";

            if (Kind == "Dist") Cnt = 7;
            else Cnt = 4;

            LogDataX1 = new double[dgvLog.Rows.Count];
            LogDataY1 = new double[dgvLog.Rows.Count];
            LogDataX2 = new double[dgvLog.Rows.Count];
            LogDataY2 = new double[dgvLog.Rows.Count];

            for (int i = 0; i < dgvLog.Rows.Count; i++)
            {
                LogDataX1[i] = Convert.ToDouble(dgvLog[StartPoint, i].Value);
                LogDataY1[i] = Convert.ToDouble(dgvLog[StartPoint + 1, i].Value);
                LogDataX2[i] = Convert.ToDouble(dgvLog[StartPoint + 2, i].Value);
                LogDataY2[i] = Convert.ToDouble(dgvLog[StartPoint + 3, i].Value);
            }

            LogDatacalculator(0, LogDataX1, Kind, CamName);
            LogDatacalculator(1, LogDataY1, Kind, CamName);
            LogDatacalculator(2, LogDataX2, Kind, CamName);
            LogDatacalculator(3, LogDataY2, Kind, CamName);

            dgvCPK.Rows.Clear();

            for (int i = 0; i < Cnt; i++)
            {
                KindName = dgvCPKHeadName(i);
                dgvCPK.Rows.Insert(i);
                dgvCPK[0, i].Value = KindName;
                dgvCPK[0, i].Style.BackColor = Color.Black;
                dgvCPK[0, i].Style.ForeColor = Color.Yellow;
                dgvCPK[0, i].ReadOnly = true;
                dgvCPK[0, i].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                for (int j = 1; j < 5; j++)
                {
                    if (Kind == "Dist")
                    {
                        if (i == 0) dgvCPK[j, i].Value = Max[j - 1];
                        else if (i == 1) dgvCPK[j, i].Value = Min[j - 1];
                        else if (i == 2) dgvCPK[j, i].Value = Gap[j - 1];
                        else if (i == 3) dgvCPK[j, i].Value = Avg[j - 1];
                        else if (i == 4) dgvCPK[j, i].Value = StandardDeviation[j - 1];
                        else if (i == 5) dgvCPK[j, i].Value = CP[j - 1];
                        else if (i == 6) dgvCPK[j, i].Value = CPK[j - 1];
                    }
                    else
                    {
                        if (i == 0) dgvCPK[j, i].Value = Max[j - 1];
                        else if (i == 1) dgvCPK[j, i].Value = Min[j - 1];
                        else if (i == 2) dgvCPK[j, i].Value = Gap[j - 1];
                        else if (i == 3) dgvCPK[j, i].Value = Avg[j - 1];
                    }

                    dgvCPK[j, i].Style.BackColor = Color.Black;
                    dgvCPK[j, i].Style.ForeColor = Color.Linen;
                    dgvCPK[j, i].ReadOnly = true;
                    dgvCPK[j, i].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
        }

        public string dgvCPKHeadName(int Count)
        {
            string Name = "";

            switch (Count)
            {
                case 0:
                    Name = "Max";
                    break;
                case 1:
                    Name = "Min";
                    break;
                case 2:
                    Name = "Gap";
                    break;
                case 3:
                    Name = "Avg";
                    break;
                case 4:
                    Name = "STDEV";
                    break;
                case 5:
                    Name = "CP";
                    break;
                case 6:
                    Name = "CPK";
                    break;
            }

            return Name;
        }

        public void LogDatacalculator(int Cnt, double[] Data, string Kind, string CamName)
        {
            double USL = 0;
            double LSL = 0;
            double BDSpec = 0;// "0.2";

            int datacnt = 0;
            double dAvg = 0;
            for (int i = 0; i < Data.Length; i++)
            {
                if (Data[i] != 0)
                {
                    if (Data[i] >= Max[Cnt]) Max[Cnt] = Data[i];
                    if (Data[i] <= Min[Cnt]) Min[Cnt] = Data[i];
                    dAvg += Data[i];
                    datacnt++;
                }
            }
            dAvg = dAvg / datacnt;
            Avg[Cnt] = dAvg;

            //추후 PLC 연동 시 주석 해제
            if (CamName.Substring(0, 7) == "BENDING")
            {
                switch (Cnt)
                {
                    case 0: //X1
                        BDSpec = Menu.frmSetting.revData.mBendingArm.BDToleranceX;
                        USL = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LX] + BDSpec;
                        LSL = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LX] - BDSpec;
                        break;
                    case 1: //Y1
                        BDSpec = Menu.frmSetting.revData.mBendingArm.BDToleranceY;
                        USL = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LY] + BDSpec;
                        LSL = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_LY] - BDSpec;
                        break;
                    case 2: //X2
                        BDSpec = Menu.frmSetting.revData.mBendingArm.BDToleranceX;
                        USL = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RX] + BDSpec;
                        LSL = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RX] - BDSpec;
                        break;
                    case 3: //Y2
                        BDSpec = Menu.frmSetting.revData.mBendingArm.BDToleranceY;
                        USL = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RY] + BDSpec;
                        LSL = CONST.RunRecipe.Param[eRecipe.BENDING_SPEC_RY] - BDSpec;
                        break;
                }
            }
            else
            {
                switch (Cnt)
                {
                    case 0: //X1
                        BDSpec = Menu.frmSetting.revData.mBendingArm.InspToleranceX;// CONST.RunRecipe.Param[eRecipe.INSPECTION_SPEC_X];
                        USL = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LX] + BDSpec;
                        LSL = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LX] - BDSpec;
                        break;
                    case 1: //Y1
                        BDSpec = Menu.frmSetting.revData.mBendingArm.InspToleranceY; //CONST.RunRecipe.Param[eRecipe.INSPECTION_SPEC_Y];
                        USL = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LY] + BDSpec;
                        LSL = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_LY] - BDSpec;
                        break;
                    case 2: //X2
                        BDSpec = Menu.frmSetting.revData.mBendingArm.InspToleranceX; //CONST.RunRecipe.Param[eRecipe.INSPECTION_SPEC_X];
                        USL = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RX] + BDSpec;
                        LSL = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RX] - BDSpec;
                        break;
                    case 3: //Y2
                        BDSpec = Menu.frmSetting.revData.mBendingArm.InspToleranceY; //CONST.RunRecipe.Param[eRecipe.INSPECTION_SPEC_Y];
                        USL = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RY] + BDSpec;
                        LSL = CONST.RunRecipe.Param[eRecipe.BENDING_INSPECTION_SPEC_RY] - BDSpec;
                        break;
                }
            }

            //Max[Cnt] = Data.Max();
            //Min[Cnt] = Data.Min();
            Gap[Cnt] = Math.Abs(Max[Cnt] - Min[Cnt]);
            //Avg[Cnt] = Data.Average();
            if (Kind == "Dist")
            {
                StandardDeviation[Cnt] = Stdev(Data, Avg[Cnt], datacnt);
                //CP[Cnt] = (1.85 - 1.05) / (6 * StandardDeviation[Cnt]);
                //CPK[Cnt] = Math.Min((1.85 - Avg[Cnt]), (Avg[Cnt] - 1.05)) / (3 * StandardDeviation[Cnt]);
                //추후 PLC 연동 시 주석 해제
                CP[Cnt] = (USL - LSL) / (6 * StandardDeviation[Cnt]);
                CPK[Cnt] = Math.Min((USL - Avg[Cnt]), (Avg[Cnt] - LSL)) / (3 * StandardDeviation[Cnt]);
            }
        }
        public double Stdev(double[] Data, double Avg, int datacnt)
        {
            double[] DispersionData = new double[datacnt];
            double Disperstion = 0;
            //double Varlance = 0;
            double StandardDev = 0;

            for (int i = 0; i < datacnt; i++)
            {
                DispersionData[i] = Math.Pow(Data[i] - Avg, 2);
            }

            Disperstion = DispersionData.Sum();

            //Varlance = Math.Sqrt(Disperstion / (DispersionData.Length - 1));

            return StandardDev = Math.Sqrt(Disperstion / (DispersionData.Length - 1));
        }
        public void dgvLogDataWrite(string[] HeadName, string[] Data, int _row, int TotalCnt)
        {
            if (firstCheck)
            {
                for (int j = 0; j < TotalCnt; j++)
                {
                    dgvLog.Columns.Add(HeadName[j].ToString(), HeadName[j].ToString());
                }
                firstCheck = false;
            }

            dgvLog.Rows.Insert(_row);

            for (int i = 0; i < TotalCnt; i++)
            {
                dgvLog[i, _row].Value = Data[i];

                //if (Data[2] == "NG")
                //{
                //    dgvLog[i, _row].Style.BackColor = Color.Red;
                //    dgvLog[i, _row].Style.ForeColor = Color.Black;
                //}
                //else
                //{
                dgvLog[i, _row].Style.BackColor = Color.Black;
                dgvLog[i, _row].Style.ForeColor = Color.Linen;
                //}
                dgvLog[i, _row].ReadOnly = true;
                dgvLog[i, _row].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }


        public void LogHeadDataArray(string Kind, string[] Data, ref int[] LogDataNum, ref string[] ArrayHeadData)
        {
            int NumCnt = 0;

            for (int i = 0; i < Data.Length; i++)
            {
                if (Kind == "PixelMark")
                {
                    switch (Data[i])
                    {
                        case "Data":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "CellID":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "OKNG":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "BendNo":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "Retry":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "MarkX1":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; StartPoint = NumCnt; NumCnt++;
                            break;
                        case "MarkY1":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "MarkX2":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "MarkY2":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                    }
                }
                else if (Kind == "PixelRef")
                {
                    switch (Data[i])
                    {
                        case "Data":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "CellID":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "OKNG":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "BendNo":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "Retry":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "RefX1":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; StartPoint = NumCnt; NumCnt++;
                            break;
                        case "RefY1":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "RefX2":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "RefY2":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                    }
                }
                else if (Kind == "Dist")
                {
                    switch (Data[i])
                    {
                        case "Data":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "CellID":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "OKNG":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "BendNo":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "Retry":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "DistX1":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; StartPoint = NumCnt; NumCnt++;
                            break;
                        case "DistY1":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "DistX2":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                        case "DistY2":
                            ArrayHeadData[NumCnt] = Data[i]; LogDataNum[NumCnt] = i; NumCnt++;
                            break;
                    }
                }
            }
            Array.Resize(ref ArrayHeadData, NumCnt);
            Array.Resize(ref LogDataNum, NumCnt);
        }
        public void LogDataArrayData(string[] Data, ref int[] LogDataNum, ref string[] ArrayData)
        {
            int NumCnt = 0;

            for (int i = 0; i < Data.Length; i++)
            {
                for (int j = 0; j < LogDataNum.Length; j++)
                {
                    if (i == LogDataNum[j])
                    {
                        ArrayData[NumCnt] = Data[i];
                        NumCnt++;
                    }
                }
            }
            Array.Resize(ref ArrayData, NumCnt);
        }

        string[] FileOK1;
        string[] FileNG1;
        string[] FileOK2;
        string[] FileNG2;
        public void GetImageFile(string CamName, ref bool NotFindFile, bool OriginalImg)
        {
            string CamAllName1 = "";
            string CamAllName2 = "";

            int TotalCnt1 = 0;
            int TotalCnt2 = 0;

            int BmpCnt1 = 0;
            int BmpCnt2 = 0;

            bool FileExistsOK1 = false;
            bool FileExistsNG1 = false;
            bool FileExistsOK2 = false;
            bool FileExistsNG2 = false;

            string ImagePathOK1 = "";
            string ImagePathNG1 = "";
            string ImagePathOK2 = "";
            string ImagePathNG2 = "";

            if (CamName != "BENDING")
            {
                string Date = dtpLogStart.Value.ToShortDateString();

                if (CamName != "LOAD PRE")
                {
                    CamAllName1 = GetCamName(CamName, 0);
                    CamAllName2 = GetCamName(CamName, 1);
                }
                else CamAllName1 = CamName;


                if (OriginalImg)
                {
                    ImagePathOK1 = CONST.cImagePath + "\\" + CamAllName1 + "\\" + Date + "\\" + "OriginalOK";
                    ImagePathNG1 = CONST.cImagePath + "\\" + CamAllName1 + "\\" + Date + "\\" + "OriginalNG";
                    if (CamName != "LOAD PRE")
                    {
                        ImagePathOK2 = CONST.cImagePath + "\\" + CamAllName2 + "\\" + Date + "\\" + "OriginalOK";
                        ImagePathNG2 = CONST.cImagePath + "\\" + CamAllName2 + "\\" + Date + "\\" + "OriginalNG";
                    }
                }
                else
                {
                    ImagePathOK1 = CONST.cImagePath + "\\" + CamAllName1 + "\\" + Date + "\\" + "DisplayOK";
                    ImagePathNG1 = CONST.cImagePath + "\\" + CamAllName1 + "\\" + Date + "\\" + "DisplayNG";
                    if (CamName != "LOAD PRE")
                    {
                        ImagePathOK2 = CONST.cImagePath + "\\" + CamAllName2 + "\\" + Date + "\\" + "DisplayOK";
                        ImagePathNG2 = CONST.cImagePath + "\\" + CamAllName2 + "\\" + Date + "\\" + "DisplayNG";
                    }
                }

                FileExistsOK1 = Directory.Exists(ImagePathOK1);
                FileExistsNG1 = Directory.Exists(ImagePathNG1);
                FileExistsOK2 = Directory.Exists(ImagePathOK2);
                FileExistsNG2 = Directory.Exists(ImagePathNG2);

                string[] a = new string[10];

                if (FileExistsOK1 || FileExistsNG1 || FileExistsOK2 || FileExistsNG2)
                {
                    if (FileExistsOK1) FileOK1 = Directory.GetFiles(ImagePathOK1);
                    else FileOK1 = new string[0];
                    if (FileExistsNG1) FileNG1 = Directory.GetFiles(ImagePathNG1);
                    else FileNG1 = new string[0];
                    if (FileExistsOK2) FileOK2 = Directory.GetFiles(ImagePathOK2);
                    else FileOK2 = new string[0];
                    if (FileExistsNG2) FileNG2 = Directory.GetFiles(ImagePathNG2);
                    else FileNG2 = new string[0];

                    //MCR 비교한 후 정렬 할 진 전에 할 지... 
                    ImageDataArray(ref FileOK1);
                    ImageDataArray(ref FileNG1);
                    ImageDataArray(ref FileOK2);
                    ImageDataArray(ref FileNG1);

                    //if (FileOK1.Length != FileOK2.Length) ImageMCRcompare(ref FileOK1, ref FileOK2);
                    //if (FileNG1.Length != FileNG2.Length) ImageMCRcompare(ref FileNG1, ref FileNG2);

                    ImageMCRcompare(ref FileOK1, ref FileOK2);
                    ImageMCRcompare(ref FileNG1, ref FileNG2);

                    TotalCnt1 = FileOK1.Length + FileNG1.Length;
                    TotalCnt2 = FileOK2.Length + FileNG2.Length;

                    BmpFile1 = new string[TotalCnt1];
                    BmpFile2 = new string[TotalCnt2];

                    ImageLog1 = new Bitmap[TotalCnt1];
                    ImageLog2 = new Bitmap[TotalCnt2];

                    for (int i = 0; i < FileOK1.Length; i++)
                    {
                        BmpFile1[BmpCnt1] = FileOK1[i];
                        ImageLog1[BmpCnt1] = new Bitmap(FileOK1[i]);
                        BmpCnt1++;
                    }

                    for (int i = 0; i < FileNG1.Length; i++)
                    {
                        BmpFile1[BmpCnt1] = FileNG1[i];
                        ImageLog1[BmpCnt1] = new Bitmap(FileNG1[i]);
                        BmpCnt1++;
                    }

                    for (int i = 0; i < FileOK2.Length; i++)
                    {
                        BmpFile2[BmpCnt2] = FileOK2[i];
                        ImageLog2[BmpCnt2] = new Bitmap(FileOK2[i]);
                        BmpCnt2++;
                    }

                    for (int i = 0; i < FileNG2.Length; i++)
                    {
                        BmpFile2[BmpCnt2] = FileNG2[i];
                        ImageLog2[BmpCnt2] = new Bitmap(FileNG2[i]);
                        BmpCnt2++;
                    }
                }

                if (!FileExistsOK1 && !FileExistsNG1 && !FileExistsOK2 && !FileExistsNG2)
                {
                    NotFindFile = true;
                    ImageLogInitial(NotFindFile);
                }
            }
        }

        public void ImageDataArray(ref string[] Files)
        {
            int Cnt = 0;

            IEnumerable<string> query = from date in Files orderby date select date;

            foreach(string str in query)
            {
                Files[Cnt] = str;
                Cnt++;
            }
        }

        public void SetImageFile(int Count)
        {
            string[] FileInfo;
            int TotalCnt = 0;

            CogImageConvertTool ImgConvert = new CogImageConvertTool();

            FileInfo = BmpFile1[Count - 1].Split('\\', '.');
            if (FileInfo[1] == "jpg") FileInfo = BmpFile2[Count - 1].Split('\\', '.');

            if (ImageLog1.Length > ImageLog2.Length) TotalCnt = ImageLog1.Length;
            else TotalCnt = ImageLog2.Length;

            //if (CamName != "LOAD PRE")
            //{
            //    cogDS[0].Visible = true;
            //    cogDS[1].Visible = true;
            //}
            //else
            //{
            //    cogDS[0].Visible = true;
            //    cogDS[1].Visible = false;
            //}

            try
            {
                ImgConvert.InputImage = new CogImage8Grey(ImageLog1[Count - 1]);
                ImgConvert.Run();
                cogDS[0].Image = ImgConvert.OutputImage;
                //cogDS[0].Image = new CogImage8Grey(ImageLog1[Count - 1]);
            }
            catch
            {
                cogDS[0].Image = null;
            }

            try
            {
                ImgConvert.InputImage = new CogImage8Grey(ImageLog2[Count - 1]);
                ImgConvert.Run();
                cogDS[1].Image = ImgConvert.OutputImage;
                //cogDS[1].Image = new CogImage8Grey(ImageLog2[Count - 1]);
            }
            catch
            {
                cogDS[1].Image = null;
            }

            cogDS[0].AutoFit = true;
            cogDS[1].AutoFit = true;

            string CameraName = FileInfo[3];

            string[] strSplit = FileInfo[6].Split('-');
            string Date = strSplit[0] + "-" + strSplit[1] + "-" + strSplit[2] + "-" + strSplit[3] + "-" + strSplit[4] + "-" + strSplit[5];
            string MCRName = strSplit[6];
            string OKNG = FileInfo[5];

            lblMessage.Text = CameraName + "," + Date + "," + MCRName + "," + OKNG; 
            lblMessage.Font = new Font("Arial", 13, FontStyle.Bold);

            lblCurrentCnt.Text = Count.ToString();
            lblTotalCnt.Text = TotalCnt.ToString();
        }

        public void ImageMCRcompare(ref string[] File1 , ref string[] File2)
        {
            string ErrorPath = "D://EQData/INI/Error.jpg";

            int File1Cnt = 0;
            int File2Cnt = 0;

            bool MCRCompare = false;
            
            string[] sFile1 = new string[File1.Length + 100];
            string[] sFile2 = new string[File2.Length + 100];

            //예제
            for (int i = 0; i < File1.Length; i++)
            {
                for (int j = 0; j < File2.Length; j++)
                {
                    string[] Split1 = File1[i].Split(',', '-', '.');
                    string[] Split2 = File2[j].Split(',', '-', '.');

                    if (Split1[9] == Split2[9])
                    {
                        sFile1[File1Cnt] = File1[i];
                        sFile2[File2Cnt] = File2[j];

                        File1Cnt++;
                        File2Cnt++;
                        MCRCompare = true;
                        break;
                    }
                    else MCRCompare = false;
                }

                if (!MCRCompare)
                {
                    sFile1[File1Cnt] = File1[i];
                    sFile2[File2Cnt] = ErrorPath;

                    File1Cnt++;
                    File2Cnt++;
                }
            }

            for (int i = 0; i < File2.Length; i++)
            {
                for (int j = 0; j < File1.Length; j++)
                {
                    string[] Split1 = File2[i].Split(',', '-', '.');
                    string[] Split2 = File1[j].Split(',', '-', '.');

                    if (Split1[9] == Split2[9])
                    {
                        MCRCompare = true;
                        break;
                    }
                    else MCRCompare = false;
                }

                if (!MCRCompare)
                {
                    sFile1[File1Cnt] = ErrorPath;
                    sFile2[File2Cnt] = File2[i];

                    File1Cnt++;
                    File2Cnt++;
                }
            }

            Array.Resize(ref sFile1, File1Cnt);
            Array.Resize(ref sFile2, File1Cnt);

            File1 = new string[sFile1.Length];
            File2 = new string[sFile2.Length];

            File1 = sFile1;
            File2 = sFile2;
        }

        public string GetCamName(string CamName, int Cnt)
        {
            string CameraName = "";

            if (Cnt == 0)
            {
                if (CamName == "LOAD PRE" || CamName == "Detach Pre") CameraName = CamName + " 1";
                else CameraName = CamName + "-1";
            }
            else if (Cnt == 1)
            {
                if (CamName == "LOAD PRE" || CamName == "Detach Pre") CameraName = CamName + " 2";
                else CameraName = CamName + "-2";
            }

            return CameraName;
        }

        int SelectIndex = -1;
        int SelectNumbers = 0;
        private void dgvLog_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            SelectIndex = e.RowIndex;

            SelectNumbers = SelectIndex + 1;

            lblSelectCellNumber.Text = (SelectNumbers).ToString();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvLog.Rows.Count > 1)
            {
                if (MessageBox.Show("Are you sure you want to delete this cell?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    if (SelectIndex != -1)
                    {
                        int ColumCount = dgvLog.ColumnCount;
                        int TotalCnt = dgvLog.Rows.Count;
                        int ReloadCnt = 0;
                        string[,] RowsValue = new string[TotalCnt, ColumCount];

                        for (int i = 0; i < TotalCnt; i++)
                        {
                            if (i != SelectNumbers - 1)
                            {
                                for (int j = 0; j < ColumCount; j++)
                                {
                                    RowsValue[ReloadCnt, j] = dgvLog[j, i].Value.ToString();
                                }
                                ReloadCnt++;
                            }
                        }

                        dgvLog.Rows.Clear();

                        for (int i = 0; i < TotalCnt - 1; i++)
                        {
                            dgvLog.Rows.Insert(i);
                            for (int j = 0; j < ColumCount; j++)
                            {
                                dgvLog[j, i].Value = RowsValue[i, j];

                                dgvLog[j, i].Style.BackColor = Color.Black;
                                dgvLog[j, i].Style.ForeColor = Color.Linen;
                                dgvLog[j, i].ReadOnly = true;
                                dgvLog[j, i].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            }
                        }

                        if (listResult.SelectedItem.ToString() != "All" && dgvLog.Rows.Count > 0) dgvLogCPKDataWrite(listResult.SelectedItem.ToString(), listCamera.SelectedItem.ToString());

                        lblTotalCellCnt.Text = ReloadCnt.ToString();
                        SelectIndex = -1;
                        lblSelectCellNumber.Text = "0";
                    }
                    else
                    {
                        MessageBox.Show("Select Cell");
                    }
                }
            }
        }


        private void dgvLog_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int Index = e.RowIndex;
            bool Search = false;
            int Cnt = -1;

            if (Index > -1)
            {
                string CellID = dgvLog[1, Index].Value.ToString();

                if (BmpFile1 != null)
                {
                    for (int i = 0; i < BmpFile1.Length; i++)
                    {
                        string[] FileInfo;
                        FileInfo = BmpFile1[i].Split('-', '.');
                        if (FileInfo[1] == "jpg") FileInfo = BmpFile2[i].Split('-', '.');


                        if (CellID == FileInfo[9])
                        {
                            Cnt = i;
                            Search = true;
                            break;
                        }
                    }
                    if (Search && Cnt > -1) SetImageFile(Cnt + 1);
                }
            }
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (lblCurrentCnt.Text != "0" || lblTotalCnt.Text != "0")
            {
                if (Convert.ToInt32(lblCurrentCnt.Text) <= Convert.ToInt32(lblTotalCnt.Text) && Convert.ToInt32(lblCurrentCnt.Text) != 1)
                {
                    int Cnt = Convert.ToInt32(lblCurrentCnt.Text);

                    Cnt -= 1;
                    lblCurrentCnt.Text = Cnt.ToString();

                    SetImageFile(Cnt);
                }
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (lblCurrentCnt.Text != "0" || lblTotalCnt.Text != "0")
            {
                if (Convert.ToInt32(lblCurrentCnt.Text) < Convert.ToInt32(lblTotalCnt.Text))
                {
                    int Cnt = Convert.ToInt32(lblCurrentCnt.Text);

                    Cnt += 1;
                    lblCurrentCnt.Text = Cnt.ToString();

                    SetImageFile(Cnt);
                }
            }
        }
        public void DeleteButtonDisp(string ResultKind)
        {
            if (ResultKind == "All" || ResultKind == "History")
            {
                lblNumber.Visible = false;
                lblSelectCellNumber.Visible = false;
                btnDelete.Visible = false;
                //lblTotal.Location = new Point(648, 9);
                //lblTotalCellCnt.Location = new Point(695, 9);
            }
            else
            {
                lblNumber.Visible = true;
                lblSelectCellNumber.Visible = true;
                btnDelete.Visible = true;
                //lblTotal.Location = new Point(382, 9);
                //lblTotalCellCnt.Location = new Point(429, 9);
            }
        }

        private void cbImgDisp_CheckedChanged(object sender, EventArgs e)
        {
            if(cbImgDisp.Checked)
            {
                rbOriginalImg.Visible = true;
                rbDisplayImg.Visible = true;
            }
            else
            {
                rbOriginalImg.Visible = false;
                rbDisplayImg.Visible = false;
            }
        }
        List<DirectoryInfo> linfo = new List<DirectoryInfo>();
        private void btnLogDisp2_Click(object sender, EventArgs e)
        {
            try
            {
                //초기화
                dgvLog.Rows.Clear();
                dgvLog.Columns.Clear();
                List<DirectoryInfo> ldirinfo = new List<DirectoryInfo>();
                //초기화 끝

                if (listResult.SelectedIndex < 0)
                {
                    MessageBox.Show("Select log");
                    return;
                }
                string sselectlog = listResult.SelectedItem.ToString();
                listResult.ClearSelected();

                //listResult.Items.Clear();
                //csLog.cLogPath;
                //CONST.cImagePath;

                //선택한 항목과 같은 로그폴더를 찾는다.
                string spath = "";
                foreach (var s in linfo)
                {
                    if (s.Name == sselectlog)
                    {
                        spath = s.FullName;
                        break;
                    }
                }

                DateTime stime = dtpLogStart.Value.Date;
                DateTime etime = dtpLogEnd.Value.Date;
                string[] sfoler = Directory.GetDirectories(spath);

                //폴더 날짜를 비교하여 해당 폴더만 리스트에 추가.
                foreach (var s in sfoler)
                {
                    DirectoryInfo info = new DirectoryInfo(s);
                    string[] ymd = info.Name.Split('-');

                    DateTime dtfolder = new DateTime(int.Parse(ymd[0]), int.Parse(ymd[1]), int.Parse(ymd[2]));

                    int a = DateTime.Compare(stime, dtfolder);
                    int b = DateTime.Compare(dtfolder, etime);
                    if (a <= 0 && b <= 0)
                    {
                        ldirinfo.Add(info);
                    }
                }

                //리스트에 있는 파일을 읽음
                bool bfirst = false;
                foreach (var s in ldirinfo)
                {
                    string[] files = Directory.GetFiles(s.FullName);

                    foreach (var d in files)
                    {
                        int cnt = 0;
                        string[] stext = File.ReadAllLines(d);
                        if (!bfirst) //첫 파일의 항목을 보고 columns을 정함.
                        {
                            string[] scolumns = stext[0].Split(',');
                            foreach (var c in scolumns)
                            {
                                dgvLog.Columns.Add(c, c);
                            }
                            bfirst = true;
                        }
                        foreach (var f in stext)
                        {
                            //첫줄제외하고 둘째줄부터 값을 표시.
                            if (cnt == 0)
                            {
                                cnt++;
                                continue;
                            }
                            else
                            {
                                string[] ff = f.Split(',');
                                dgvLog.Rows.Add(ff);
                                cnt++;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void txtCount_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
