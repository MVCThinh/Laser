namespace Bending
{
    partial class Menu
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

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Menu));
            this.pnTOP = new System.Windows.Forms.Panel();
            this.cbSimulation = new System.Windows.Forms.CheckBox();
            this.pnDL = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.lbProgramVer = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lbClock = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lbMainCurrentRcp = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label_PLC = new System.Windows.Forms.Label();
            this.lblAuto = new System.Windows.Forms.Label();
            this.lbLogINUser = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pnMenu = new System.Windows.Forms.Panel();
            this.tmrDisplay = new System.Windows.Forms.Timer(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.lbBottom = new System.Windows.Forms.Panel();
            this.btnSideViewer = new System.Windows.Forms.Button();
            this.btnHistory = new System.Windows.Forms.Button();
            this.lbHddCP = new System.Windows.Forms.Label();
            this.lbRamP = new System.Windows.Forms.Label();
            this.lbHDDDP = new System.Windows.Forms.Label();
            this.lbCpuP = new System.Windows.Forms.Label();
            this.pgbCpu = new System.Windows.Forms.ProgressBar();
            this.lblHdc = new System.Windows.Forms.Label();
            this.lblRam = new System.Windows.Forms.Label();
            this.lblHdd = new System.Windows.Forms.Label();
            this.lblCpu = new System.Windows.Forms.Label();
            this.pgbHdc = new System.Windows.Forms.ProgressBar();
            this.pgbHdd = new System.Windows.Forms.ProgressBar();
            this.pgbRam = new System.Windows.Forms.ProgressBar();
            this.btnRecipe = new System.Windows.Forms.Button();
            this.btnSetting = new System.Windows.Forms.Button();
            this.btnAutoMain = new System.Windows.Forms.Button();
            this.btnLogIn = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.pnTOP.SuspendLayout();
            this.lbBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnTOP
            // 
            this.pnTOP.BackColor = System.Drawing.Color.Black;
            this.pnTOP.Controls.Add(this.cbSimulation);
            this.pnTOP.Controls.Add(this.pnDL);
            this.pnTOP.Controls.Add(this.label2);
            this.pnTOP.Controls.Add(this.lbProgramVer);
            this.pnTOP.Controls.Add(this.label11);
            this.pnTOP.Controls.Add(this.lbClock);
            this.pnTOP.Controls.Add(this.label9);
            this.pnTOP.Controls.Add(this.label1);
            this.pnTOP.Controls.Add(this.label8);
            this.pnTOP.Controls.Add(this.lbMainCurrentRcp);
            this.pnTOP.Controls.Add(this.label7);
            this.pnTOP.Controls.Add(this.label12);
            this.pnTOP.Controls.Add(this.label_PLC);
            this.pnTOP.Controls.Add(this.lblAuto);
            this.pnTOP.Controls.Add(this.lbLogINUser);
            this.pnTOP.Controls.Add(this.label3);
            this.pnTOP.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnTOP.Location = new System.Drawing.Point(0, 0);
            this.pnTOP.Name = "pnTOP";
            this.pnTOP.Size = new System.Drawing.Size(1902, 66);
            this.pnTOP.TabIndex = 27;
            // 
            // cbSimulation
            // 
            this.cbSimulation.AutoSize = true;
            this.cbSimulation.Location = new System.Drawing.Point(1491, 13);
            this.cbSimulation.Name = "cbSimulation";
            this.cbSimulation.Size = new System.Drawing.Size(15, 14);
            this.cbSimulation.TabIndex = 169;
            this.cbSimulation.UseVisualStyleBackColor = true;
            this.cbSimulation.CheckedChanged += new System.EventHandler(this.cbSimulation_CheckedChanged);
            // 
            // pnDL
            // 
            this.pnDL.Location = new System.Drawing.Point(1690, 42);
            this.pnDL.Name = "pnDL";
            this.pnDL.Size = new System.Drawing.Size(18, 16);
            this.pnDL.TabIndex = 168;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Black;
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(1686, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 26);
            this.label2.TabIndex = 167;
            this.label2.Text = "DL";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbProgramVer
            // 
            this.lbProgramVer.BackColor = System.Drawing.Color.White;
            this.lbProgramVer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbProgramVer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lbProgramVer.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.lbProgramVer.ForeColor = System.Drawing.Color.Black;
            this.lbProgramVer.Location = new System.Drawing.Point(1491, 34);
            this.lbProgramVer.Name = "lbProgramVer";
            this.lbProgramVer.Size = new System.Drawing.Size(189, 29);
            this.lbProgramVer.TabIndex = 28;
            this.lbProgramVer.Text = "lbProgramVer";
            this.lbProgramVer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label11
            // 
            this.label11.BackColor = System.Drawing.Color.Black;
            this.label11.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label11.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.label11.ForeColor = System.Drawing.Color.White;
            this.label11.Location = new System.Drawing.Point(1491, 3);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(189, 26);
            this.label11.TabIndex = 74;
            this.label11.Text = "Program Version";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbClock
            // 
            this.lbClock.BackColor = System.Drawing.Color.Black;
            this.lbClock.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbClock.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lbClock.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbClock.ForeColor = System.Drawing.Color.White;
            this.lbClock.Location = new System.Drawing.Point(1718, 3);
            this.lbClock.Name = "lbClock";
            this.lbClock.Size = new System.Drawing.Size(181, 60);
            this.lbClock.TabIndex = 17;
            this.lbClock.Text = "Time";
            this.lbClock.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label9
            // 
            this.label9.BackColor = System.Drawing.Color.Black;
            this.label9.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label9.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.White;
            this.label9.Location = new System.Drawing.Point(166, 4);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(134, 25);
            this.label9.TabIndex = 68;
            this.label9.Text = "Current Recipe";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Black;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label1.Font = new System.Drawing.Font("Arial", 25F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(768, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(717, 58);
            this.label1.TabIndex = 29;
            this.label1.Text = "Bending System";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.BackColor = System.Drawing.Color.White;
            this.label8.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.label8.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.label8.ForeColor = System.Drawing.Color.Black;
            this.label8.Image = ((System.Drawing.Image)(resources.GetObject("label8.Image")));
            this.label8.Location = new System.Drawing.Point(1, 1);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(159, 63);
            this.label8.TabIndex = 157;
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMainCurrentRcp
            // 
            this.lbMainCurrentRcp.BackColor = System.Drawing.Color.White;
            this.lbMainCurrentRcp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbMainCurrentRcp.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.lbMainCurrentRcp.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.lbMainCurrentRcp.ForeColor = System.Drawing.Color.Black;
            this.lbMainCurrentRcp.Location = new System.Drawing.Point(306, 4);
            this.lbMainCurrentRcp.Name = "lbMainCurrentRcp";
            this.lbMainCurrentRcp.Size = new System.Drawing.Size(155, 25);
            this.lbMainCurrentRcp.TabIndex = 62;
            this.lbMainCurrentRcp.Text = "EG1_5.1";
            this.lbMainCurrentRcp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.BackColor = System.Drawing.Color.Black;
            this.label7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label7.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(467, 35);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(134, 25);
            this.label7.TabIndex = 160;
            this.label7.Text = "P L C";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label12
            // 
            this.label12.BackColor = System.Drawing.Color.Black;
            this.label12.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label12.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.ForeColor = System.Drawing.Color.White;
            this.label12.Location = new System.Drawing.Point(166, 36);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(134, 25);
            this.label12.TabIndex = 70;
            this.label12.Text = "Operation Mode";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_PLC
            // 
            this.label_PLC.BackColor = System.Drawing.Color.White;
            this.label_PLC.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_PLC.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label_PLC.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold);
            this.label_PLC.ForeColor = System.Drawing.Color.Black;
            this.label_PLC.Location = new System.Drawing.Point(607, 36);
            this.label_PLC.Name = "label_PLC";
            this.label_PLC.Size = new System.Drawing.Size(155, 25);
            this.label_PLC.TabIndex = 155;
            this.label_PLC.Text = "Disconnect!";
            this.label_PLC.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAuto
            // 
            this.lblAuto.BackColor = System.Drawing.Color.White;
            this.lblAuto.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblAuto.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold);
            this.lblAuto.ForeColor = System.Drawing.Color.Black;
            this.lblAuto.Location = new System.Drawing.Point(306, 36);
            this.lblAuto.Name = "lblAuto";
            this.lblAuto.Size = new System.Drawing.Size(155, 25);
            this.lblAuto.TabIndex = 63;
            this.lblAuto.Text = "STOP";
            this.lblAuto.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbLogINUser
            // 
            this.lbLogINUser.BackColor = System.Drawing.Color.White;
            this.lbLogINUser.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbLogINUser.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lbLogINUser.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold);
            this.lbLogINUser.ForeColor = System.Drawing.Color.Black;
            this.lbLogINUser.Location = new System.Drawing.Point(607, 4);
            this.lbLogINUser.Name = "lbLogINUser";
            this.lbLogINUser.Size = new System.Drawing.Size(155, 25);
            this.lbLogINUser.TabIndex = 30;
            this.lbLogINUser.Text = "LogINUser";
            this.lbLogINUser.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbLogINUser.Click += new System.EventHandler(this.lbLogINUser_Click);
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Black;
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label3.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(467, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(134, 25);
            this.label3.TabIndex = 156;
            this.label3.Text = "Login Level";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.DodgerBlue;
            this.panel1.Location = new System.Drawing.Point(-9, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1950, 2);
            this.panel1.TabIndex = 291;
            // 
            // pnMenu
            // 
            this.pnMenu.BackColor = System.Drawing.Color.Black;
            this.pnMenu.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnMenu.Location = new System.Drawing.Point(1, 68);
            this.pnMenu.Name = "pnMenu";
            this.pnMenu.Size = new System.Drawing.Size(1919, 949);
            this.pnMenu.TabIndex = 55;
            // 
            // tmrDisplay
            // 
            this.tmrDisplay.Enabled = true;
            this.tmrDisplay.Interval = 500;
            this.tmrDisplay.Tick += new System.EventHandler(this.tmrDisplay_Tick);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.DodgerBlue;
            this.panel2.Location = new System.Drawing.Point(0, 67);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1920, 2);
            this.panel2.TabIndex = 56;
            // 
            // lbBottom
            // 
            this.lbBottom.BackColor = System.Drawing.Color.Black;
            this.lbBottom.Controls.Add(this.btnSideViewer);
            this.lbBottom.Controls.Add(this.btnHistory);
            this.lbBottom.Controls.Add(this.lbHddCP);
            this.lbBottom.Controls.Add(this.lbRamP);
            this.lbBottom.Controls.Add(this.lbHDDDP);
            this.lbBottom.Controls.Add(this.lbCpuP);
            this.lbBottom.Controls.Add(this.pgbCpu);
            this.lbBottom.Controls.Add(this.lblHdc);
            this.lbBottom.Controls.Add(this.lblRam);
            this.lbBottom.Controls.Add(this.lblHdd);
            this.lbBottom.Controls.Add(this.lblCpu);
            this.lbBottom.Controls.Add(this.pgbHdc);
            this.lbBottom.Controls.Add(this.pgbHdd);
            this.lbBottom.Controls.Add(this.pgbRam);
            this.lbBottom.Controls.Add(this.panel1);
            this.lbBottom.Controls.Add(this.btnRecipe);
            this.lbBottom.Controls.Add(this.btnSetting);
            this.lbBottom.Controls.Add(this.btnAutoMain);
            this.lbBottom.Controls.Add(this.btnLogIn);
            this.lbBottom.Controls.Add(this.btnExit);
            this.lbBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lbBottom.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lbBottom.Location = new System.Drawing.Point(0, 932);
            this.lbBottom.Name = "lbBottom";
            this.lbBottom.Size = new System.Drawing.Size(1902, 101);
            this.lbBottom.TabIndex = 38;
            // 
            // btnSideViewer
            // 
            this.btnSideViewer.BackColor = System.Drawing.Color.White;
            this.btnSideViewer.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnSideViewer.FlatAppearance.BorderSize = 2;
            this.btnSideViewer.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnSideViewer.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnSideViewer.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSideViewer.Image = global::LaserMarking.Properties.Resources.button;
            this.btnSideViewer.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnSideViewer.Location = new System.Drawing.Point(1172, 15);
            this.btnSideViewer.Name = "btnSideViewer";
            this.btnSideViewer.Size = new System.Drawing.Size(83, 78);
            this.btnSideViewer.TabIndex = 303;
            this.btnSideViewer.Text = "Side Viewer";
            this.btnSideViewer.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnSideViewer.UseVisualStyleBackColor = false;
            this.btnSideViewer.Click += new System.EventHandler(this.btnSideViewer_Click);
            // 
            // btnHistory
            // 
            this.btnHistory.BackColor = System.Drawing.Color.Black;
            this.btnHistory.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHistory.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnHistory.FlatAppearance.BorderSize = 2;
            this.btnHistory.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnHistory.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnHistory.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnHistory.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHistory.ForeColor = System.Drawing.Color.White;
            this.btnHistory.Image = ((System.Drawing.Image)(resources.GetObject("btnHistory.Image")));
            this.btnHistory.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnHistory.Location = new System.Drawing.Point(384, 11);
            this.btnHistory.Name = "btnHistory";
            this.btnHistory.Size = new System.Drawing.Size(120, 85);
            this.btnHistory.TabIndex = 302;
            this.btnHistory.Text = "HISTORY";
            this.btnHistory.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnHistory.UseVisualStyleBackColor = false;
            // 
            // lbHddCP
            // 
            this.lbHddCP.BackColor = System.Drawing.Color.Black;
            this.lbHddCP.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbHddCP.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbHddCP.ForeColor = System.Drawing.Color.White;
            this.lbHddCP.Location = new System.Drawing.Point(1556, 54);
            this.lbHddCP.Name = "lbHddCP";
            this.lbHddCP.Size = new System.Drawing.Size(54, 17);
            this.lbHddCP.TabIndex = 301;
            this.lbHddCP.Text = "00.00 %";
            this.lbHddCP.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbRamP
            // 
            this.lbRamP.BackColor = System.Drawing.Color.Black;
            this.lbRamP.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbRamP.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbRamP.ForeColor = System.Drawing.Color.White;
            this.lbRamP.Location = new System.Drawing.Point(1556, 32);
            this.lbRamP.Name = "lbRamP";
            this.lbRamP.Size = new System.Drawing.Size(54, 17);
            this.lbRamP.TabIndex = 300;
            this.lbRamP.Text = "00.00 %";
            this.lbRamP.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbHDDDP
            // 
            this.lbHDDDP.BackColor = System.Drawing.Color.Black;
            this.lbHDDDP.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbHDDDP.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbHDDDP.ForeColor = System.Drawing.Color.White;
            this.lbHDDDP.Location = new System.Drawing.Point(1556, 76);
            this.lbHDDDP.Name = "lbHDDDP";
            this.lbHDDDP.Size = new System.Drawing.Size(54, 17);
            this.lbHDDDP.TabIndex = 299;
            this.lbHDDDP.Text = "00.00 %";
            this.lbHDDDP.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbCpuP
            // 
            this.lbCpuP.BackColor = System.Drawing.Color.Black;
            this.lbCpuP.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbCpuP.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbCpuP.ForeColor = System.Drawing.Color.White;
            this.lbCpuP.Location = new System.Drawing.Point(1556, 10);
            this.lbCpuP.Name = "lbCpuP";
            this.lbCpuP.Size = new System.Drawing.Size(54, 17);
            this.lbCpuP.TabIndex = 298;
            this.lbCpuP.Text = "00.00 %";
            this.lbCpuP.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pgbCpu
            // 
            this.pgbCpu.BackColor = System.Drawing.Color.Maroon;
            this.pgbCpu.ForeColor = System.Drawing.SystemColors.ControlText;
            this.pgbCpu.Location = new System.Drawing.Point(1363, 10);
            this.pgbCpu.Name = "pgbCpu";
            this.pgbCpu.Size = new System.Drawing.Size(187, 17);
            this.pgbCpu.TabIndex = 294;
            // 
            // lblHdc
            // 
            this.lblHdc.BackColor = System.Drawing.Color.Black;
            this.lblHdc.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.lblHdc.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHdc.ForeColor = System.Drawing.Color.Yellow;
            this.lblHdc.Location = new System.Drawing.Point(1261, 54);
            this.lblHdc.Name = "lblHdc";
            this.lblHdc.Size = new System.Drawing.Size(94, 17);
            this.lblHdc.TabIndex = 297;
            this.lblHdc.Text = "H D D (C)";
            this.lblHdc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblRam
            // 
            this.lblRam.BackColor = System.Drawing.Color.Black;
            this.lblRam.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.lblRam.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRam.ForeColor = System.Drawing.Color.Yellow;
            this.lblRam.Location = new System.Drawing.Point(1261, 32);
            this.lblRam.Name = "lblRam";
            this.lblRam.Size = new System.Drawing.Size(94, 17);
            this.lblRam.TabIndex = 296;
            this.lblRam.Text = "R A M";
            this.lblRam.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblHdd
            // 
            this.lblHdd.BackColor = System.Drawing.Color.Black;
            this.lblHdd.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.lblHdd.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHdd.ForeColor = System.Drawing.Color.Yellow;
            this.lblHdd.Location = new System.Drawing.Point(1261, 76);
            this.lblHdd.Name = "lblHdd";
            this.lblHdd.Size = new System.Drawing.Size(94, 17);
            this.lblHdd.TabIndex = 295;
            this.lblHdd.Text = "H D D (D)";
            this.lblHdd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCpu
            // 
            this.lblCpu.BackColor = System.Drawing.Color.Black;
            this.lblCpu.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.lblCpu.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCpu.ForeColor = System.Drawing.Color.Yellow;
            this.lblCpu.Location = new System.Drawing.Point(1261, 10);
            this.lblCpu.Name = "lblCpu";
            this.lblCpu.Size = new System.Drawing.Size(94, 17);
            this.lblCpu.TabIndex = 294;
            this.lblCpu.Text = "C P U";
            this.lblCpu.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pgbHdc
            // 
            this.pgbHdc.Location = new System.Drawing.Point(1363, 54);
            this.pgbHdc.Name = "pgbHdc";
            this.pgbHdc.Size = new System.Drawing.Size(187, 17);
            this.pgbHdc.TabIndex = 297;
            // 
            // pgbHdd
            // 
            this.pgbHdd.Location = new System.Drawing.Point(1363, 76);
            this.pgbHdd.Name = "pgbHdd";
            this.pgbHdd.Size = new System.Drawing.Size(187, 17);
            this.pgbHdd.TabIndex = 296;
            // 
            // pgbRam
            // 
            this.pgbRam.Location = new System.Drawing.Point(1363, 32);
            this.pgbRam.Name = "pgbRam";
            this.pgbRam.Size = new System.Drawing.Size(187, 17);
            this.pgbRam.TabIndex = 295;
            // 
            // btnRecipe
            // 
            this.btnRecipe.BackColor = System.Drawing.Color.Black;
            this.btnRecipe.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRecipe.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnRecipe.FlatAppearance.BorderSize = 2;
            this.btnRecipe.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnRecipe.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnRecipe.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRecipe.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRecipe.ForeColor = System.Drawing.Color.White;
            this.btnRecipe.Image = ((System.Drawing.Image)(resources.GetObject("btnRecipe.Image")));
            this.btnRecipe.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnRecipe.Location = new System.Drawing.Point(132, 11);
            this.btnRecipe.Name = "btnRecipe";
            this.btnRecipe.Size = new System.Drawing.Size(120, 85);
            this.btnRecipe.TabIndex = 30;
            this.btnRecipe.Text = "RECIPE";
            this.btnRecipe.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnRecipe.UseVisualStyleBackColor = false;
            // 
            // btnSetting
            // 
            this.btnSetting.BackColor = System.Drawing.Color.Black;
            this.btnSetting.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSetting.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnSetting.FlatAppearance.BorderSize = 2;
            this.btnSetting.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnSetting.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnSetting.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSetting.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSetting.ForeColor = System.Drawing.Color.White;
            this.btnSetting.Image = ((System.Drawing.Image)(resources.GetObject("btnSetting.Image")));
            this.btnSetting.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnSetting.Location = new System.Drawing.Point(258, 11);
            this.btnSetting.Name = "btnSetting";
            this.btnSetting.Size = new System.Drawing.Size(120, 85);
            this.btnSetting.TabIndex = 31;
            this.btnSetting.Text = "SETTING";
            this.btnSetting.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnSetting.UseVisualStyleBackColor = false;
            // 
            // btnAutoMain
            // 
            this.btnAutoMain.BackColor = System.Drawing.Color.Black;
            this.btnAutoMain.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAutoMain.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnAutoMain.FlatAppearance.BorderSize = 2;
            this.btnAutoMain.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnAutoMain.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnAutoMain.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnAutoMain.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAutoMain.ForeColor = System.Drawing.Color.White;
            this.btnAutoMain.Image = ((System.Drawing.Image)(resources.GetObject("btnAutoMain.Image")));
            this.btnAutoMain.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnAutoMain.Location = new System.Drawing.Point(6, 11);
            this.btnAutoMain.Name = "btnAutoMain";
            this.btnAutoMain.Size = new System.Drawing.Size(120, 85);
            this.btnAutoMain.TabIndex = 29;
            this.btnAutoMain.Text = "AUTOMAIN";
            this.btnAutoMain.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnAutoMain.UseVisualStyleBackColor = false;
            // 
            // btnLogIn
            // 
            this.btnLogIn.BackColor = System.Drawing.Color.Black;
            this.btnLogIn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogIn.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnLogIn.FlatAppearance.BorderSize = 2;
            this.btnLogIn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnLogIn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnLogIn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLogIn.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLogIn.ForeColor = System.Drawing.Color.White;
            this.btnLogIn.Image = ((System.Drawing.Image)(resources.GetObject("btnLogIn.Image")));
            this.btnLogIn.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnLogIn.Location = new System.Drawing.Point(1644, 11);
            this.btnLogIn.Name = "btnLogIn";
            this.btnLogIn.Size = new System.Drawing.Size(120, 85);
            this.btnLogIn.TabIndex = 28;
            this.btnLogIn.Text = "LOG IN";
            this.btnLogIn.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnLogIn.UseVisualStyleBackColor = false;
            this.btnLogIn.Click += new System.EventHandler(this.btnLogIn_Click);
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.Black;
            this.btnExit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExit.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnExit.FlatAppearance.BorderSize = 2;
            this.btnExit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnExit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnExit.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExit.ForeColor = System.Drawing.Color.White;
            this.btnExit.Image = ((System.Drawing.Image)(resources.GetObject("btnExit.Image")));
            this.btnExit.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnExit.Location = new System.Drawing.Point(1770, 11);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(120, 85);
            this.btnExit.TabIndex = 33;
            this.btnExit.Text = "EXIT";
            this.btnExit.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // Menu
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1902, 1033);
            this.ControlBox = false;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.lbBottom);
            this.Controls.Add(this.pnTOP);
            this.Controls.Add(this.pnMenu);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximumSize = new System.Drawing.Size(1920, 1080);
            this.Name = "Menu";
            this.Text = "Bending";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Menu_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.lbSelectUser_FormClosed);
            this.pnTOP.ResumeLayout(false);
            this.pnTOP.PerformLayout();
            this.lbBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnTOP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnLogIn;
        private System.Windows.Forms.Label lbProgramVer;
        private System.Windows.Forms.Label lbClock;
        private System.Windows.Forms.Button btnSetting;
        private System.Windows.Forms.Button btnRecipe;
        private System.Windows.Forms.Button btnAutoMain;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Label lbLogINUser;
        private System.Windows.Forms.Panel pnMenu;
        private System.Windows.Forms.Timer tmrDisplay;
        private System.Windows.Forms.Panel panel2;
        public System.Windows.Forms.Label lbMainCurrentRcp;
        public System.Windows.Forms.Label label9;
        public System.Windows.Forms.Label label12;
        public System.Windows.Forms.Label label_PLC;
        private System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.Label lblAuto;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Panel lbBottom;
        public System.Windows.Forms.Label label8;
        public System.Windows.Forms.Label lblHdc;
        public System.Windows.Forms.Label lblRam;
        public System.Windows.Forms.Label lblHdd;
        public System.Windows.Forms.Label lblCpu;
        private System.Windows.Forms.ProgressBar pgbHdc;
        private System.Windows.Forms.ProgressBar pgbHdd;
        private System.Windows.Forms.ProgressBar pgbRam;
        private System.Windows.Forms.ProgressBar pgbCpu;
        public System.Windows.Forms.Label lbHddCP;
        public System.Windows.Forms.Label lbRamP;
        public System.Windows.Forms.Label lbHDDDP;
        public System.Windows.Forms.Label lbCpuP;
        private System.Windows.Forms.Button btnHistory;
        private System.Windows.Forms.Button btnSideViewer;
        private System.Windows.Forms.Panel pnDL;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbSimulation;
    }
}

