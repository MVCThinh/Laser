namespace Bending
{
    partial class ucHistory
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ucHistory));
            this.dtpLogStart = new System.Windows.Forms.DateTimePicker();
            this.PnDateSearch = new System.Windows.Forms.Panel();
            this.btnLogDisp2 = new System.Windows.Forms.Button();
            this.dtpLogEnd = new System.Windows.Forms.DateTimePicker();
            this.cbImgDisp = new System.Windows.Forms.CheckBox();
            this.rbDisplayImg = new System.Windows.Forms.RadioButton();
            this.rbOriginalImg = new System.Windows.Forms.RadioButton();
            this.txtCount = new System.Windows.Forms.TextBox();
            this.cbLimitCountSet = new System.Windows.Forms.CheckBox();
            this.btnLogDisp = new System.Windows.Forms.Button();
            this.listResult = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.listCamera = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.dgvLog = new System.Windows.Forms.DataGridView();
            this.dgvCPK = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnLog = new System.Windows.Forms.Panel();
            this.lblTotalCellCnt = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblSelectCellNumber = new System.Windows.Forms.Label();
            this.lblNumber = new System.Windows.Forms.Label();
            this.btnDelete = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.pnImage = new System.Windows.Forms.Panel();
            this.lblTotalCnt = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblCurrentCnt = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblMessage = new System.Windows.Forms.Label();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnPrevious = new System.Windows.Forms.Button();
            this.cogDS2 = new Cognex.VisionPro.Display.CogDisplay();
            this.cogDS1 = new Cognex.VisionPro.Display.CogDisplay();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.PnDateSearch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCPK)).BeginInit();
            this.pnLog.SuspendLayout();
            this.pnImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cogDS2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cogDS1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // dtpLogStart
            // 
            this.dtpLogStart.Location = new System.Drawing.Point(5, 28);
            this.dtpLogStart.Name = "dtpLogStart";
            this.dtpLogStart.Size = new System.Drawing.Size(171, 21);
            this.dtpLogStart.TabIndex = 274;
            // 
            // PnDateSearch
            // 
            this.PnDateSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PnDateSearch.Controls.Add(this.btnLogDisp2);
            this.PnDateSearch.Controls.Add(this.dtpLogEnd);
            this.PnDateSearch.Controls.Add(this.cbImgDisp);
            this.PnDateSearch.Controls.Add(this.rbDisplayImg);
            this.PnDateSearch.Controls.Add(this.rbOriginalImg);
            this.PnDateSearch.Controls.Add(this.txtCount);
            this.PnDateSearch.Controls.Add(this.cbLimitCountSet);
            this.PnDateSearch.Controls.Add(this.btnLogDisp);
            this.PnDateSearch.Controls.Add(this.listResult);
            this.PnDateSearch.Controls.Add(this.label3);
            this.PnDateSearch.Controls.Add(this.listCamera);
            this.PnDateSearch.Controls.Add(this.label2);
            this.PnDateSearch.Controls.Add(this.label8);
            this.PnDateSearch.Controls.Add(this.dtpLogStart);
            this.PnDateSearch.Location = new System.Drawing.Point(3, 3);
            this.PnDateSearch.Name = "PnDateSearch";
            this.PnDateSearch.Size = new System.Drawing.Size(364, 495);
            this.PnDateSearch.TabIndex = 280;
            // 
            // btnLogDisp2
            // 
            this.btnLogDisp2.BackColor = System.Drawing.Color.Black;
            this.btnLogDisp2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnLogDisp2.ForeColor = System.Drawing.Color.White;
            this.btnLogDisp2.Location = new System.Drawing.Point(264, 348);
            this.btnLogDisp2.Name = "btnLogDisp2";
            this.btnLogDisp2.Size = new System.Drawing.Size(95, 36);
            this.btnLogDisp2.TabIndex = 294;
            this.btnLogDisp2.Text = "DISPLAY";
            this.btnLogDisp2.UseVisualStyleBackColor = false;
            this.btnLogDisp2.Click += new System.EventHandler(this.btnLogDisp2_Click);
            // 
            // dtpLogEnd
            // 
            this.dtpLogEnd.Location = new System.Drawing.Point(188, 28);
            this.dtpLogEnd.Name = "dtpLogEnd";
            this.dtpLogEnd.Size = new System.Drawing.Size(171, 21);
            this.dtpLogEnd.TabIndex = 293;
            // 
            // cbImgDisp
            // 
            this.cbImgDisp.AutoSize = true;
            this.cbImgDisp.ForeColor = System.Drawing.Color.White;
            this.cbImgDisp.Location = new System.Drawing.Point(5, 408);
            this.cbImgDisp.Name = "cbImgDisp";
            this.cbImgDisp.Size = new System.Drawing.Size(105, 16);
            this.cbImgDisp.TabIndex = 292;
            this.cbImgDisp.Text = "Image Display";
            this.cbImgDisp.UseVisualStyleBackColor = true;
            this.cbImgDisp.Visible = false;
            this.cbImgDisp.CheckedChanged += new System.EventHandler(this.cbImgDisp_CheckedChanged);
            // 
            // rbDisplayImg
            // 
            this.rbDisplayImg.AutoSize = true;
            this.rbDisplayImg.ForeColor = System.Drawing.Color.White;
            this.rbDisplayImg.Location = new System.Drawing.Point(116, 450);
            this.rbDisplayImg.Name = "rbDisplayImg";
            this.rbDisplayImg.Size = new System.Drawing.Size(105, 16);
            this.rbDisplayImg.TabIndex = 291;
            this.rbDisplayImg.TabStop = true;
            this.rbDisplayImg.Text = "DisPlay Image";
            this.rbDisplayImg.UseVisualStyleBackColor = true;
            // 
            // rbOriginalImg
            // 
            this.rbOriginalImg.AutoSize = true;
            this.rbOriginalImg.ForeColor = System.Drawing.Color.White;
            this.rbOriginalImg.Location = new System.Drawing.Point(5, 450);
            this.rbOriginalImg.Name = "rbOriginalImg";
            this.rbOriginalImg.Size = new System.Drawing.Size(105, 16);
            this.rbOriginalImg.TabIndex = 290;
            this.rbOriginalImg.TabStop = true;
            this.rbOriginalImg.Text = "Original Image";
            this.rbOriginalImg.UseVisualStyleBackColor = true;
            // 
            // txtCount
            // 
            this.txtCount.Location = new System.Drawing.Point(129, 363);
            this.txtCount.Name = "txtCount";
            this.txtCount.Size = new System.Drawing.Size(100, 21);
            this.txtCount.TabIndex = 289;
            this.txtCount.TextChanged += new System.EventHandler(this.txtCount_TextChanged);
            // 
            // cbLimitCountSet
            // 
            this.cbLimitCountSet.AutoSize = true;
            this.cbLimitCountSet.ForeColor = System.Drawing.Color.White;
            this.cbLimitCountSet.Location = new System.Drawing.Point(5, 368);
            this.cbLimitCountSet.Name = "cbLimitCountSet";
            this.cbLimitCountSet.Size = new System.Drawing.Size(118, 16);
            this.cbLimitCountSet.TabIndex = 288;
            this.cbLimitCountSet.Text = "Limit Count Set :";
            this.cbLimitCountSet.UseVisualStyleBackColor = true;
            this.cbLimitCountSet.CheckedChanged += new System.EventHandler(this.cbLimitCountSet_CheckedChanged_1);
            // 
            // btnLogDisp
            // 
            this.btnLogDisp.BackColor = System.Drawing.Color.Black;
            this.btnLogDisp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnLogDisp.ForeColor = System.Drawing.Color.White;
            this.btnLogDisp.Location = new System.Drawing.Point(264, 439);
            this.btnLogDisp.Name = "btnLogDisp";
            this.btnLogDisp.Size = new System.Drawing.Size(95, 36);
            this.btnLogDisp.TabIndex = 287;
            this.btnLogDisp.Text = "DISPLAY";
            this.btnLogDisp.UseVisualStyleBackColor = false;
            this.btnLogDisp.Visible = false;
            this.btnLogDisp.Click += new System.EventHandler(this.btnLogDisp_Click);
            // 
            // listResult
            // 
            this.listResult.BackColor = System.Drawing.Color.Black;
            this.listResult.ForeColor = System.Drawing.Color.Yellow;
            this.listResult.FormattingEnabled = true;
            this.listResult.ItemHeight = 12;
            this.listResult.Location = new System.Drawing.Point(5, 226);
            this.listResult.Name = "listResult";
            this.listResult.Size = new System.Drawing.Size(354, 112);
            this.listResult.TabIndex = 284;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(3, 211);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 12);
            this.label3.TabIndex = 283;
            this.label3.Text = "Result List";
            // 
            // listCamera
            // 
            this.listCamera.BackColor = System.Drawing.Color.Black;
            this.listCamera.ForeColor = System.Drawing.Color.Yellow;
            this.listCamera.FormattingEnabled = true;
            this.listCamera.ItemHeight = 12;
            this.listCamera.Location = new System.Drawing.Point(5, 84);
            this.listCamera.Name = "listCamera";
            this.listCamera.Size = new System.Drawing.Size(354, 112);
            this.listCamera.TabIndex = 282;
            this.listCamera.Visible = false;
            this.listCamera.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listCamera_MouseClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(3, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 12);
            this.label2.TabIndex = 281;
            this.label2.Text = "Camera List";
            this.label2.Visible = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.ForeColor = System.Drawing.Color.White;
            this.label8.Location = new System.Drawing.Point(3, 7);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(37, 12);
            this.label8.TabIndex = 278;
            this.label8.Text = "DATE";
            // 
            // dgvLog
            // 
            this.dgvLog.AllowUserToAddRows = false;
            this.dgvLog.AllowUserToDeleteRows = false;
            this.dgvLog.BackgroundColor = System.Drawing.Color.Black;
            this.dgvLog.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvLog.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvLog.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLog.Location = new System.Drawing.Point(1, 30);
            this.dgvLog.Name = "dgvLog";
            this.dgvLog.ReadOnly = true;
            this.dgvLog.RowTemplate.Height = 23;
            this.dgvLog.Size = new System.Drawing.Size(735, 648);
            this.dgvLog.TabIndex = 281;
            this.dgvLog.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvLog_CellClick);
            this.dgvLog.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvLog_CellDoubleClick);
            // 
            // dgvCPK
            // 
            this.dgvCPK.AllowUserToAddRows = false;
            this.dgvCPK.AllowUserToDeleteRows = false;
            this.dgvCPK.BackgroundColor = System.Drawing.Color.Black;
            this.dgvCPK.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvCPK.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvCPK.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCPK.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4,
            this.Column5});
            this.dgvCPK.Location = new System.Drawing.Point(1, 683);
            this.dgvCPK.Name = "dgvCPK";
            this.dgvCPK.ReadOnly = true;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvCPK.RowsDefaultCellStyle = dataGridViewCellStyle8;
            this.dgvCPK.RowTemplate.Height = 23;
            this.dgvCPK.Size = new System.Drawing.Size(735, 185);
            this.dgvCPK.TabIndex = 282;
            // 
            // Column1
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Column1.DefaultCellStyle = dataGridViewCellStyle3;
            this.Column1.HeaderText = "Name";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 140;
            // 
            // Column2
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Column2.DefaultCellStyle = dataGridViewCellStyle4;
            this.Column2.HeaderText = "LX";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Width = 140;
            // 
            // Column3
            // 
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Column3.DefaultCellStyle = dataGridViewCellStyle5;
            this.Column3.HeaderText = "LY";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.Width = 140;
            // 
            // Column4
            // 
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomCenter;
            this.Column4.DefaultCellStyle = dataGridViewCellStyle6;
            this.Column4.HeaderText = "RX";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.Width = 140;
            // 
            // Column5
            // 
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Column5.DefaultCellStyle = dataGridViewCellStyle7;
            this.Column5.HeaderText = "RY";
            this.Column5.Name = "Column5";
            this.Column5.ReadOnly = true;
            this.Column5.Width = 140;
            // 
            // pnLog
            // 
            this.pnLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnLog.Controls.Add(this.lblTotalCellCnt);
            this.pnLog.Controls.Add(this.lblTotal);
            this.pnLog.Controls.Add(this.lblSelectCellNumber);
            this.pnLog.Controls.Add(this.lblNumber);
            this.pnLog.Controls.Add(this.btnDelete);
            this.pnLog.Controls.Add(this.label12);
            this.pnLog.Controls.Add(this.dgvLog);
            this.pnLog.Controls.Add(this.dgvCPK);
            this.pnLog.Location = new System.Drawing.Point(373, 3);
            this.pnLog.Name = "pnLog";
            this.pnLog.Size = new System.Drawing.Size(741, 874);
            this.pnLog.TabIndex = 283;
            // 
            // lblTotalCellCnt
            // 
            this.lblTotalCellCnt.AutoSize = true;
            this.lblTotalCellCnt.ForeColor = System.Drawing.Color.White;
            this.lblTotalCellCnt.Location = new System.Drawing.Point(437, 9);
            this.lblTotalCellCnt.Name = "lblTotalCellCnt";
            this.lblTotalCellCnt.Size = new System.Drawing.Size(11, 12);
            this.lblTotalCellCnt.TabIndex = 295;
            this.lblTotalCellCnt.Text = "0";
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.ForeColor = System.Drawing.Color.White;
            this.lblTotal.Location = new System.Drawing.Point(390, 9);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(41, 12);
            this.lblTotal.TabIndex = 294;
            this.lblTotal.Text = "Total :";
            // 
            // lblSelectCellNumber
            // 
            this.lblSelectCellNumber.AutoSize = true;
            this.lblSelectCellNumber.ForeColor = System.Drawing.Color.White;
            this.lblSelectCellNumber.Location = new System.Drawing.Point(590, 9);
            this.lblSelectCellNumber.Name = "lblSelectCellNumber";
            this.lblSelectCellNumber.Size = new System.Drawing.Size(11, 12);
            this.lblSelectCellNumber.TabIndex = 293;
            this.lblSelectCellNumber.Text = "0";
            // 
            // lblNumber
            // 
            this.lblNumber.AutoSize = true;
            this.lblNumber.ForeColor = System.Drawing.Color.White;
            this.lblNumber.Location = new System.Drawing.Point(461, 9);
            this.lblNumber.Name = "lblNumber";
            this.lblNumber.Size = new System.Drawing.Size(123, 12);
            this.lblNumber.TabIndex = 292;
            this.lblNumber.Text = "Select Cell Number :";
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(613, 3);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(95, 24);
            this.btnDelete.TabIndex = 290;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Visible = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // label12
            // 
            this.label12.BackColor = System.Drawing.Color.LightGray;
            this.label12.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.label12.ForeColor = System.Drawing.Color.Black;
            this.label12.Location = new System.Drawing.Point(0, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(244, 25);
            this.label12.TabIndex = 291;
            this.label12.Text = "Log";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnImage
            // 
            this.pnImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnImage.Controls.Add(this.lblTotalCnt);
            this.pnImage.Controls.Add(this.label7);
            this.pnImage.Controls.Add(this.lblCurrentCnt);
            this.pnImage.Controls.Add(this.label5);
            this.pnImage.Controls.Add(this.lblMessage);
            this.pnImage.Controls.Add(this.btnNext);
            this.pnImage.Controls.Add(this.btnPrevious);
            this.pnImage.Controls.Add(this.cogDS2);
            this.pnImage.Controls.Add(this.cogDS1);
            this.pnImage.Controls.Add(this.pictureBox1);
            this.pnImage.Controls.Add(this.label4);
            this.pnImage.Location = new System.Drawing.Point(1129, 4);
            this.pnImage.Name = "pnImage";
            this.pnImage.Size = new System.Drawing.Size(773, 443);
            this.pnImage.TabIndex = 286;
            this.pnImage.Visible = false;
            // 
            // lblTotalCnt
            // 
            this.lblTotalCnt.ForeColor = System.Drawing.Color.White;
            this.lblTotalCnt.Location = new System.Drawing.Point(736, 326);
            this.lblTotalCnt.Name = "lblTotalCnt";
            this.lblTotalCnt.Size = new System.Drawing.Size(17, 62);
            this.lblTotalCnt.TabIndex = 303;
            this.lblTotalCnt.Text = "1";
            this.lblTotalCnt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(713, 326);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(17, 62);
            this.label7.TabIndex = 302;
            this.label7.Text = "/";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCurrentCnt
            // 
            this.lblCurrentCnt.ForeColor = System.Drawing.Color.White;
            this.lblCurrentCnt.Location = new System.Drawing.Point(690, 326);
            this.lblCurrentCnt.Name = "lblCurrentCnt";
            this.lblCurrentCnt.Size = new System.Drawing.Size(17, 62);
            this.lblCurrentCnt.TabIndex = 301;
            this.lblCurrentCnt.Text = "0";
            this.lblCurrentCnt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(3, 306);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 12);
            this.label5.TabIndex = 300;
            this.label5.Text = "Message";
            // 
            // lblMessage
            // 
            this.lblMessage.ForeColor = System.Drawing.Color.White;
            this.lblMessage.Location = new System.Drawing.Point(3, 326);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(681, 62);
            this.lblMessage.TabIndex = 299;
            this.lblMessage.Text = "Message";
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnNext
            // 
            this.btnNext.Font = new System.Drawing.Font("Arial", 30F, System.Drawing.FontStyle.Bold);
            this.btnNext.Location = new System.Drawing.Point(389, 391);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(379, 47);
            this.btnNext.TabIndex = 298;
            this.btnNext.Text = ">";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnPrevious
            // 
            this.btnPrevious.Font = new System.Drawing.Font("Arial", 30F, System.Drawing.FontStyle.Bold);
            this.btnPrevious.Location = new System.Drawing.Point(3, 391);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(379, 47);
            this.btnPrevious.TabIndex = 297;
            this.btnPrevious.Text = "<";
            this.btnPrevious.UseVisualStyleBackColor = true;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // cogDS2
            // 
            this.cogDS2.ColorMapLowerClipColor = System.Drawing.Color.Black;
            this.cogDS2.ColorMapLowerRoiLimit = 0D;
            this.cogDS2.ColorMapPredefined = Cognex.VisionPro.Display.CogDisplayColorMapPredefinedConstants.None;
            this.cogDS2.ColorMapUpperClipColor = System.Drawing.Color.Black;
            this.cogDS2.ColorMapUpperRoiLimit = 1D;
            this.cogDS2.DoubleTapZoomCycleLength = 2;
            this.cogDS2.DoubleTapZoomSensitivity = 2.5D;
            this.cogDS2.Location = new System.Drawing.Point(389, 28);
            this.cogDS2.MouseWheelMode = Cognex.VisionPro.Display.CogDisplayMouseWheelModeConstants.Zoom1;
            this.cogDS2.MouseWheelSensitivity = 1D;
            this.cogDS2.Name = "cogDS2";
            this.cogDS2.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("cogDS2.OcxState")));
            this.cogDS2.Size = new System.Drawing.Size(379, 275);
            this.cogDS2.TabIndex = 296;
            // 
            // cogDS1
            // 
            this.cogDS1.ColorMapLowerClipColor = System.Drawing.Color.Black;
            this.cogDS1.ColorMapLowerRoiLimit = 0D;
            this.cogDS1.ColorMapPredefined = Cognex.VisionPro.Display.CogDisplayColorMapPredefinedConstants.None;
            this.cogDS1.ColorMapUpperClipColor = System.Drawing.Color.Black;
            this.cogDS1.ColorMapUpperRoiLimit = 1D;
            this.cogDS1.DoubleTapZoomCycleLength = 2;
            this.cogDS1.DoubleTapZoomSensitivity = 2.5D;
            this.cogDS1.Location = new System.Drawing.Point(3, 28);
            this.cogDS1.MouseWheelMode = Cognex.VisionPro.Display.CogDisplayMouseWheelModeConstants.Zoom1;
            this.cogDS1.MouseWheelSensitivity = 1D;
            this.cogDS1.Name = "cogDS1";
            this.cogDS1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("cogDS1.OcxState")));
            this.cogDS1.Size = new System.Drawing.Size(379, 275);
            this.cogDS1.TabIndex = 295;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(3, 28);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(0, 0);
            this.pictureBox1.TabIndex = 294;
            this.pictureBox1.TabStop = false;
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.LightGray;
            this.label4.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(244, 25);
            this.label4.TabIndex = 293;
            this.label4.Text = "Image";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ucHistory
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.pnImage);
            this.Controls.Add(this.pnLog);
            this.Controls.Add(this.PnDateSearch);
            this.Name = "ucHistory";
            this.Size = new System.Drawing.Size(1913, 880);
            this.PnDateSearch.ResumeLayout(false);
            this.PnDateSearch.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCPK)).EndInit();
            this.pnLog.ResumeLayout(false);
            this.pnLog.PerformLayout();
            this.pnImage.ResumeLayout(false);
            this.pnImage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cogDS2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cogDS1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DateTimePicker dtpLogStart;
        private System.Windows.Forms.Panel PnDateSearch;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox listResult;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox listCamera;
        private System.Windows.Forms.Button btnLogDisp;
        private System.Windows.Forms.TextBox txtCount;
        private System.Windows.Forms.CheckBox cbLimitCountSet;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DataGridView dgvLog;
        private System.Windows.Forms.DataGridView dgvCPK;
        private System.Windows.Forms.Panel pnLog;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Panel pnImage;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnPrevious;
        private Cognex.VisionPro.Display.CogDisplay cogDS2;
        private Cognex.VisionPro.Display.CogDisplay cogDS1;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblTotalCnt;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblCurrentCnt;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Label lblSelectCellNumber;
        private System.Windows.Forms.Label lblNumber;
        private System.Windows.Forms.RadioButton rbDisplayImg;
        private System.Windows.Forms.RadioButton rbOriginalImg;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label lblTotalCellCnt;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.CheckBox cbImgDisp;
        private System.Windows.Forms.DateTimePicker dtpLogEnd;
        private System.Windows.Forms.Button btnLogDisp2;
    }
}
