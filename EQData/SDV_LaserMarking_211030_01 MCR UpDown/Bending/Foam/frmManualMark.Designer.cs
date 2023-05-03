namespace Bending
{
    partial class frmManualMark
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmManualMark));
            this.cogDS1 = new Cognex.VisionPro.Display.CogDisplay();
            this.lbTitle = new System.Windows.Forms.Label();
            this.cogDS2 = new Cognex.VisionPro.Display.CogDisplay();
            this.lbLeftMark = new System.Windows.Forms.Label();
            this.lbRightMark = new System.Windows.Forms.Label();
            this.btnConfirm1 = new System.Windows.Forms.Button();
            this.btnConfirm2 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.tmrMark = new System.Windows.Forms.Timer(this.components);
            this.btnFitImage = new System.Windows.Forms.Button();
            this.btnPoint = new System.Windows.Forms.Button();
            this.btnPan = new System.Windows.Forms.Button();
            this.btnZoomOut = new System.Windows.Forms.Button();
            this.btnZoomIn = new System.Windows.Forms.Button();
            this.button_Grab = new System.Windows.Forms.Button();
            this.button_Grab1 = new System.Windows.Forms.Button();
            this.button_FitImage = new System.Windows.Forms.Button();
            this.button_Point = new System.Windows.Forms.Button();
            this.button_Pan = new System.Windows.Forms.Button();
            this.button_ZoomOut = new System.Windows.Forms.Button();
            this.button_ZoomIn = new System.Windows.Forms.Button();
            this.button_ByPass = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.cbCreateLine1 = new System.Windows.Forms.CheckBox();
            this.cbCreateLine2 = new System.Windows.Forms.CheckBox();
            this.txtImageGray = new System.Windows.Forms.TextBox();
            this.txtImageY = new System.Windows.Forms.TextBox();
            this.txtImageX = new System.Windows.Forms.TextBox();
            this.txtImageGray2 = new System.Windows.Forms.TextBox();
            this.txtImageY2 = new System.Windows.Forms.TextBox();
            this.txtImageX2 = new System.Windows.Forms.TextBox();
            this.cogDSPattern1 = new Cognex.VisionPro.Display.CogDisplay();
            this.cogDSPattern2 = new Cognex.VisionPro.Display.CogDisplay();
            this.tbMove1 = new System.Windows.Forms.TextBox();
            this.btnOriginRight1 = new System.Windows.Forms.Button();
            this.btnOriginLeft1 = new System.Windows.Forms.Button();
            this.btnOriginUP1 = new System.Windows.Forms.Button();
            this.btnOriginDown1 = new System.Windows.Forms.Button();
            this.tbMove2 = new System.Windows.Forms.TextBox();
            this.btnOriginRight2 = new System.Windows.Forms.Button();
            this.btnOriginLeft2 = new System.Windows.Forms.Button();
            this.btnOriginUP2 = new System.Windows.Forms.Button();
            this.btnOriginDown2 = new System.Windows.Forms.Button();
            this.lbLCheck = new System.Windows.Forms.Label();
            this.tmrDisp = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.cogDS1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cogDS2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cogDSPattern1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cogDSPattern2)).BeginInit();
            this.SuspendLayout();
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
            this.cogDS1.Location = new System.Drawing.Point(5, 317);
            this.cogDS1.MouseWheelMode = Cognex.VisionPro.Display.CogDisplayMouseWheelModeConstants.Zoom1;
            this.cogDS1.MouseWheelSensitivity = 1D;
            this.cogDS1.Name = "cogDS1";
            this.cogDS1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("cogDS1.OcxState")));
            this.cogDS1.Size = new System.Drawing.Size(945, 660);
            this.cogDS1.TabIndex = 10;
            this.cogDS1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.cogDS_MouseDown);
            this.cogDS1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.cogDS1_MouseMove);
            // 
            // lbTitle
            // 
            this.lbTitle.BackColor = System.Drawing.Color.DimGray;
            this.lbTitle.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbTitle.ForeColor = System.Drawing.Color.White;
            this.lbTitle.Location = new System.Drawing.Point(338, 8);
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Size = new System.Drawing.Size(1232, 38);
            this.lbTitle.TabIndex = 131;
            this.lbTitle.Text = "Manual Mark Search";
            this.lbTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            this.cogDS2.Location = new System.Drawing.Point(956, 317);
            this.cogDS2.MouseWheelMode = Cognex.VisionPro.Display.CogDisplayMouseWheelModeConstants.Zoom1;
            this.cogDS2.MouseWheelSensitivity = 1D;
            this.cogDS2.Name = "cogDS2";
            this.cogDS2.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("cogDS2.OcxState")));
            this.cogDS2.Size = new System.Drawing.Size(945, 660);
            this.cogDS2.TabIndex = 132;
            this.cogDS2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.cogDS2_MouseDown);
            this.cogDS2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.cogDS2_MouseMove);
            // 
            // lbLeftMark
            // 
            this.lbLeftMark.BackColor = System.Drawing.Color.Black;
            this.lbLeftMark.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbLeftMark.ForeColor = System.Drawing.Color.Yellow;
            this.lbLeftMark.Location = new System.Drawing.Point(2, 275);
            this.lbLeftMark.Name = "lbLeftMark";
            this.lbLeftMark.Size = new System.Drawing.Size(882, 21);
            this.lbLeftMark.TabIndex = 133;
            this.lbLeftMark.Text = "Panel Mark";
            this.lbLeftMark.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbRightMark
            // 
            this.lbRightMark.BackColor = System.Drawing.Color.Black;
            this.lbRightMark.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbRightMark.ForeColor = System.Drawing.Color.Yellow;
            this.lbRightMark.Location = new System.Drawing.Point(960, 273);
            this.lbRightMark.Name = "lbRightMark";
            this.lbRightMark.Size = new System.Drawing.Size(879, 21);
            this.lbRightMark.TabIndex = 134;
            this.lbRightMark.Text = "FPC Mark";
            this.lbRightMark.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnConfirm1
            // 
            this.btnConfirm1.Enabled = false;
            this.btnConfirm1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConfirm1.Location = new System.Drawing.Point(5, 983);
            this.btnConfirm1.Name = "btnConfirm1";
            this.btnConfirm1.Size = new System.Drawing.Size(175, 46);
            this.btnConfirm1.TabIndex = 135;
            this.btnConfirm1.Text = "Confirm";
            this.btnConfirm1.UseVisualStyleBackColor = true;
            this.btnConfirm1.Click += new System.EventHandler(this.btnConfirm1_Click);
            // 
            // btnConfirm2
            // 
            this.btnConfirm2.Enabled = false;
            this.btnConfirm2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConfirm2.Location = new System.Drawing.Point(1536, 983);
            this.btnConfirm2.Name = "btnConfirm2";
            this.btnConfirm2.Size = new System.Drawing.Size(175, 46);
            this.btnConfirm2.TabIndex = 136;
            this.btnConfirm2.Text = "Confirm ";
            this.btnConfirm2.UseVisualStyleBackColor = true;
            this.btnConfirm2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button4
            // 
            this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button4.Location = new System.Drawing.Point(671, 983);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(175, 46);
            this.button4.TabIndex = 138;
            this.button4.Text = "Mark NG Out";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Visible = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // tmrMark
            // 
            this.tmrMark.Interval = 500;
            this.tmrMark.Tick += new System.EventHandler(this.tmrMark_Tick);
            // 
            // btnFitImage
            // 
            this.btnFitImage.BackColor = System.Drawing.Color.White;
            this.btnFitImage.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.btnFitImage.FlatAppearance.BorderSize = 2;
            this.btnFitImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFitImage.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFitImage.ForeColor = System.Drawing.Color.Black;
            this.btnFitImage.Location = new System.Drawing.Point(327, 49);
            this.btnFitImage.Name = "btnFitImage";
            this.btnFitImage.Size = new System.Drawing.Size(150, 70);
            this.btnFitImage.TabIndex = 297;
            this.btnFitImage.Text = "Fit Image";
            this.btnFitImage.UseVisualStyleBackColor = false;
            this.btnFitImage.Click += new System.EventHandler(this.btnFitImage_Click);
            // 
            // btnPoint
            // 
            this.btnPoint.BackColor = System.Drawing.Color.White;
            this.btnPoint.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.btnPoint.FlatAppearance.BorderSize = 2;
            this.btnPoint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPoint.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPoint.ForeColor = System.Drawing.Color.Black;
            this.btnPoint.Location = new System.Drawing.Point(5, 127);
            this.btnPoint.Name = "btnPoint";
            this.btnPoint.Size = new System.Drawing.Size(150, 70);
            this.btnPoint.TabIndex = 296;
            this.btnPoint.Text = "Point";
            this.btnPoint.UseVisualStyleBackColor = false;
            this.btnPoint.Click += new System.EventHandler(this.btnPoint_Click);
            // 
            // btnPan
            // 
            this.btnPan.BackColor = System.Drawing.Color.White;
            this.btnPan.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.btnPan.FlatAppearance.BorderSize = 2;
            this.btnPan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPan.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPan.ForeColor = System.Drawing.Color.Black;
            this.btnPan.Location = new System.Drawing.Point(166, 127);
            this.btnPan.Name = "btnPan";
            this.btnPan.Size = new System.Drawing.Size(150, 70);
            this.btnPan.TabIndex = 293;
            this.btnPan.Text = "Pan";
            this.btnPan.UseVisualStyleBackColor = false;
            this.btnPan.Click += new System.EventHandler(this.btnPan_Click);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.BackColor = System.Drawing.Color.White;
            this.btnZoomOut.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.btnZoomOut.FlatAppearance.BorderSize = 2;
            this.btnZoomOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnZoomOut.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnZoomOut.ForeColor = System.Drawing.Color.Black;
            this.btnZoomOut.Location = new System.Drawing.Point(166, 49);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(150, 70);
            this.btnZoomOut.TabIndex = 294;
            this.btnZoomOut.Text = "Zoom OUT";
            this.btnZoomOut.UseVisualStyleBackColor = false;
            this.btnZoomOut.Click += new System.EventHandler(this.btnZoomOut_Click);
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.BackColor = System.Drawing.Color.White;
            this.btnZoomIn.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.btnZoomIn.FlatAppearance.BorderSize = 2;
            this.btnZoomIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnZoomIn.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnZoomIn.ForeColor = System.Drawing.Color.Black;
            this.btnZoomIn.Location = new System.Drawing.Point(5, 49);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new System.Drawing.Size(150, 70);
            this.btnZoomIn.TabIndex = 295;
            this.btnZoomIn.Text = "Zoom IN";
            this.btnZoomIn.UseVisualStyleBackColor = false;
            this.btnZoomIn.Click += new System.EventHandler(this.btnZoomIn_Click);
            // 
            // button_Grab
            // 
            this.button_Grab.BackColor = System.Drawing.Color.Gold;
            this.button_Grab.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.button_Grab.FlatAppearance.BorderSize = 2;
            this.button_Grab.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_Grab.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Grab.ForeColor = System.Drawing.Color.Black;
            this.button_Grab.Location = new System.Drawing.Point(327, 127);
            this.button_Grab.Name = "button_Grab";
            this.button_Grab.Size = new System.Drawing.Size(150, 70);
            this.button_Grab.TabIndex = 299;
            this.button_Grab.Text = "Image Grab";
            this.button_Grab.UseVisualStyleBackColor = false;
            this.button_Grab.Click += new System.EventHandler(this.button_Grab_Click);
            // 
            // button_Grab1
            // 
            this.button_Grab1.BackColor = System.Drawing.Color.Gold;
            this.button_Grab1.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.button_Grab1.FlatAppearance.BorderSize = 2;
            this.button_Grab1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_Grab1.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Grab1.ForeColor = System.Drawing.Color.Black;
            this.button_Grab1.Location = new System.Drawing.Point(1742, 127);
            this.button_Grab1.Name = "button_Grab1";
            this.button_Grab1.Size = new System.Drawing.Size(150, 70);
            this.button_Grab1.TabIndex = 305;
            this.button_Grab1.Text = "Image Grab";
            this.button_Grab1.UseVisualStyleBackColor = false;
            this.button_Grab1.Click += new System.EventHandler(this.button_Grab1_Click);
            // 
            // button_FitImage
            // 
            this.button_FitImage.BackColor = System.Drawing.Color.White;
            this.button_FitImage.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.button_FitImage.FlatAppearance.BorderSize = 2;
            this.button_FitImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_FitImage.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_FitImage.ForeColor = System.Drawing.Color.Black;
            this.button_FitImage.Location = new System.Drawing.Point(1742, 49);
            this.button_FitImage.Name = "button_FitImage";
            this.button_FitImage.Size = new System.Drawing.Size(150, 70);
            this.button_FitImage.TabIndex = 304;
            this.button_FitImage.Text = "Fit Image";
            this.button_FitImage.UseVisualStyleBackColor = false;
            this.button_FitImage.Click += new System.EventHandler(this.button_FitImage_Click);
            // 
            // button_Point
            // 
            this.button_Point.BackColor = System.Drawing.Color.White;
            this.button_Point.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.button_Point.FlatAppearance.BorderSize = 2;
            this.button_Point.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_Point.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Point.ForeColor = System.Drawing.Color.Black;
            this.button_Point.Location = new System.Drawing.Point(1420, 127);
            this.button_Point.Name = "button_Point";
            this.button_Point.Size = new System.Drawing.Size(150, 70);
            this.button_Point.TabIndex = 303;
            this.button_Point.Text = "Point";
            this.button_Point.UseVisualStyleBackColor = false;
            this.button_Point.Click += new System.EventHandler(this.button_Point_Click);
            // 
            // button_Pan
            // 
            this.button_Pan.BackColor = System.Drawing.Color.White;
            this.button_Pan.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.button_Pan.FlatAppearance.BorderSize = 2;
            this.button_Pan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_Pan.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Pan.ForeColor = System.Drawing.Color.Black;
            this.button_Pan.Location = new System.Drawing.Point(1581, 127);
            this.button_Pan.Name = "button_Pan";
            this.button_Pan.Size = new System.Drawing.Size(150, 70);
            this.button_Pan.TabIndex = 300;
            this.button_Pan.Text = "Pan";
            this.button_Pan.UseVisualStyleBackColor = false;
            this.button_Pan.Click += new System.EventHandler(this.button_Pan_Click);
            // 
            // button_ZoomOut
            // 
            this.button_ZoomOut.BackColor = System.Drawing.Color.White;
            this.button_ZoomOut.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.button_ZoomOut.FlatAppearance.BorderSize = 2;
            this.button_ZoomOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_ZoomOut.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_ZoomOut.ForeColor = System.Drawing.Color.Black;
            this.button_ZoomOut.Location = new System.Drawing.Point(1581, 49);
            this.button_ZoomOut.Name = "button_ZoomOut";
            this.button_ZoomOut.Size = new System.Drawing.Size(150, 70);
            this.button_ZoomOut.TabIndex = 301;
            this.button_ZoomOut.Text = "Zoom OUT";
            this.button_ZoomOut.UseVisualStyleBackColor = false;
            this.button_ZoomOut.Click += new System.EventHandler(this.button_ZoomOut_Click);
            // 
            // button_ZoomIn
            // 
            this.button_ZoomIn.BackColor = System.Drawing.Color.White;
            this.button_ZoomIn.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.button_ZoomIn.FlatAppearance.BorderSize = 2;
            this.button_ZoomIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_ZoomIn.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_ZoomIn.ForeColor = System.Drawing.Color.Black;
            this.button_ZoomIn.Location = new System.Drawing.Point(1420, 49);
            this.button_ZoomIn.Name = "button_ZoomIn";
            this.button_ZoomIn.Size = new System.Drawing.Size(150, 70);
            this.button_ZoomIn.TabIndex = 302;
            this.button_ZoomIn.Text = "Zoom IN";
            this.button_ZoomIn.UseVisualStyleBackColor = false;
            this.button_ZoomIn.Click += new System.EventHandler(this.button_ZoomIn_Click);
            // 
            // button_ByPass
            // 
            this.button_ByPass.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_ByPass.Location = new System.Drawing.Point(872, 983);
            this.button_ByPass.Name = "button_ByPass";
            this.button_ByPass.Size = new System.Drawing.Size(175, 46);
            this.button_ByPass.TabIndex = 306;
            this.button_ByPass.Text = "Mark NG By-Pass";
            this.button_ByPass.UseVisualStyleBackColor = true;
            this.button_ByPass.Click += new System.EventHandler(this.button_ByPass_Click);
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(1717, 983);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(175, 46);
            this.btnClose.TabIndex = 307;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.Close_Click);
            // 
            // cbCreateLine1
            // 
            this.cbCreateLine1.AutoSize = true;
            this.cbCreateLine1.Location = new System.Drawing.Point(5, 300);
            this.cbCreateLine1.Name = "cbCreateLine1";
            this.cbCreateLine1.Size = new System.Drawing.Size(67, 17);
            this.cbCreateLine1.TabIndex = 308;
            this.cbCreateLine1.Text = "LineDisp";
            this.cbCreateLine1.UseVisualStyleBackColor = true;
            // 
            // cbCreateLine2
            // 
            this.cbCreateLine2.AutoSize = true;
            this.cbCreateLine2.Location = new System.Drawing.Point(960, 299);
            this.cbCreateLine2.Name = "cbCreateLine2";
            this.cbCreateLine2.Size = new System.Drawing.Size(67, 17);
            this.cbCreateLine2.TabIndex = 309;
            this.cbCreateLine2.Text = "LineDisp";
            this.cbCreateLine2.UseVisualStyleBackColor = true;
            // 
            // txtImageGray
            // 
            this.txtImageGray.BackColor = System.Drawing.Color.Black;
            this.txtImageGray.Font = new System.Drawing.Font("Gulim", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtImageGray.ForeColor = System.Drawing.Color.White;
            this.txtImageGray.Location = new System.Drawing.Point(853, 296);
            this.txtImageGray.Name = "txtImageGray";
            this.txtImageGray.ReadOnly = true;
            this.txtImageGray.Size = new System.Drawing.Size(98, 22);
            this.txtImageGray.TabIndex = 311;
            this.txtImageGray.Text = "Grey : 0";
            this.txtImageGray.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtImageY
            // 
            this.txtImageY.BackColor = System.Drawing.Color.Black;
            this.txtImageY.Font = new System.Drawing.Font("Gulim", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtImageY.ForeColor = System.Drawing.Color.White;
            this.txtImageY.Location = new System.Drawing.Point(749, 296);
            this.txtImageY.Name = "txtImageY";
            this.txtImageY.ReadOnly = true;
            this.txtImageY.Size = new System.Drawing.Size(98, 22);
            this.txtImageY.TabIndex = 312;
            this.txtImageY.Text = "Y : 0.0";
            this.txtImageY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtImageX
            // 
            this.txtImageX.BackColor = System.Drawing.Color.Black;
            this.txtImageX.Font = new System.Drawing.Font("Gulim", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtImageX.ForeColor = System.Drawing.Color.White;
            this.txtImageX.Location = new System.Drawing.Point(645, 296);
            this.txtImageX.Name = "txtImageX";
            this.txtImageX.ReadOnly = true;
            this.txtImageX.Size = new System.Drawing.Size(98, 22);
            this.txtImageX.TabIndex = 313;
            this.txtImageX.Text = "X : 0.0";
            this.txtImageX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtImageGray2
            // 
            this.txtImageGray2.BackColor = System.Drawing.Color.Black;
            this.txtImageGray2.Font = new System.Drawing.Font("Gulim", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtImageGray2.ForeColor = System.Drawing.Color.White;
            this.txtImageGray2.Location = new System.Drawing.Point(1803, 296);
            this.txtImageGray2.Name = "txtImageGray2";
            this.txtImageGray2.ReadOnly = true;
            this.txtImageGray2.Size = new System.Drawing.Size(98, 22);
            this.txtImageGray2.TabIndex = 314;
            this.txtImageGray2.Text = "Grey : 0";
            this.txtImageGray2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtImageY2
            // 
            this.txtImageY2.BackColor = System.Drawing.Color.Black;
            this.txtImageY2.Font = new System.Drawing.Font("Gulim", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtImageY2.ForeColor = System.Drawing.Color.White;
            this.txtImageY2.Location = new System.Drawing.Point(1699, 296);
            this.txtImageY2.Name = "txtImageY2";
            this.txtImageY2.ReadOnly = true;
            this.txtImageY2.Size = new System.Drawing.Size(98, 22);
            this.txtImageY2.TabIndex = 315;
            this.txtImageY2.Text = "Y : 0.0";
            this.txtImageY2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtImageX2
            // 
            this.txtImageX2.BackColor = System.Drawing.Color.Black;
            this.txtImageX2.Font = new System.Drawing.Font("Gulim", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtImageX2.ForeColor = System.Drawing.Color.White;
            this.txtImageX2.Location = new System.Drawing.Point(1595, 296);
            this.txtImageX2.Name = "txtImageX2";
            this.txtImageX2.ReadOnly = true;
            this.txtImageX2.Size = new System.Drawing.Size(98, 22);
            this.txtImageX2.TabIndex = 316;
            this.txtImageX2.Text = "X : 0.0";
            this.txtImageX2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // cogDSPattern1
            // 
            this.cogDSPattern1.ColorMapLowerClipColor = System.Drawing.Color.Black;
            this.cogDSPattern1.ColorMapLowerRoiLimit = 0D;
            this.cogDSPattern1.ColorMapPredefined = Cognex.VisionPro.Display.CogDisplayColorMapPredefinedConstants.None;
            this.cogDSPattern1.ColorMapUpperClipColor = System.Drawing.Color.Black;
            this.cogDSPattern1.ColorMapUpperRoiLimit = 1D;
            this.cogDSPattern1.DoubleTapZoomCycleLength = 2;
            this.cogDSPattern1.DoubleTapZoomSensitivity = 2.5D;
            this.cogDSPattern1.Location = new System.Drawing.Point(687, 46);
            this.cogDSPattern1.MouseWheelMode = Cognex.VisionPro.Display.CogDisplayMouseWheelModeConstants.Zoom1;
            this.cogDSPattern1.MouseWheelSensitivity = 1D;
            this.cogDSPattern1.Name = "cogDSPattern1";
            this.cogDSPattern1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("cogDSPattern1.OcxState")));
            this.cogDSPattern1.Size = new System.Drawing.Size(264, 221);
            this.cogDSPattern1.TabIndex = 317;
            // 
            // cogDSPattern2
            // 
            this.cogDSPattern2.ColorMapLowerClipColor = System.Drawing.Color.Black;
            this.cogDSPattern2.ColorMapLowerRoiLimit = 0D;
            this.cogDSPattern2.ColorMapPredefined = Cognex.VisionPro.Display.CogDisplayColorMapPredefinedConstants.None;
            this.cogDSPattern2.ColorMapUpperClipColor = System.Drawing.Color.Black;
            this.cogDSPattern2.ColorMapUpperRoiLimit = 1D;
            this.cogDSPattern2.DoubleTapZoomCycleLength = 2;
            this.cogDSPattern2.DoubleTapZoomSensitivity = 2.5D;
            this.cogDSPattern2.Location = new System.Drawing.Point(960, 46);
            this.cogDSPattern2.MouseWheelMode = Cognex.VisionPro.Display.CogDisplayMouseWheelModeConstants.Zoom1;
            this.cogDSPattern2.MouseWheelSensitivity = 1D;
            this.cogDSPattern2.Name = "cogDSPattern2";
            this.cogDSPattern2.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("cogDSPattern2.OcxState")));
            this.cogDSPattern2.Size = new System.Drawing.Size(264, 221);
            this.cogDSPattern2.TabIndex = 318;
            // 
            // tbMove1
            // 
            this.tbMove1.Location = new System.Drawing.Point(890, 273);
            this.tbMove1.Name = "tbMove1";
            this.tbMove1.Size = new System.Drawing.Size(50, 20);
            this.tbMove1.TabIndex = 325;
            this.tbMove1.Text = "1";
            // 
            // btnOriginRight1
            // 
            this.btnOriginRight1.Image = global::LaserMarking.Properties.Resources.Right;
            this.btnOriginRight1.Location = new System.Drawing.Point(599, 125);
            this.btnOriginRight1.Name = "btnOriginRight1";
            this.btnOriginRight1.Size = new System.Drawing.Size(50, 50);
            this.btnOriginRight1.TabIndex = 322;
            this.btnOriginRight1.UseVisualStyleBackColor = true;
            this.btnOriginRight1.Click += new System.EventHandler(this.btnOriginRight1_Click);
            // 
            // btnOriginLeft1
            // 
            this.btnOriginLeft1.Image = global::LaserMarking.Properties.Resources.Left;
            this.btnOriginLeft1.Location = new System.Drawing.Point(487, 125);
            this.btnOriginLeft1.Name = "btnOriginLeft1";
            this.btnOriginLeft1.Size = new System.Drawing.Size(50, 50);
            this.btnOriginLeft1.TabIndex = 321;
            this.btnOriginLeft1.UseVisualStyleBackColor = true;
            this.btnOriginLeft1.Click += new System.EventHandler(this.btnOriginLeft1_Click);
            // 
            // btnOriginUP1
            // 
            this.btnOriginUP1.Image = global::LaserMarking.Properties.Resources.Up;
            this.btnOriginUP1.Location = new System.Drawing.Point(543, 69);
            this.btnOriginUP1.Name = "btnOriginUP1";
            this.btnOriginUP1.Size = new System.Drawing.Size(50, 50);
            this.btnOriginUP1.TabIndex = 319;
            this.btnOriginUP1.UseVisualStyleBackColor = true;
            this.btnOriginUP1.Click += new System.EventHandler(this.btnOriginUP1_Click);
            // 
            // btnOriginDown1
            // 
            this.btnOriginDown1.Image = global::LaserMarking.Properties.Resources.Down;
            this.btnOriginDown1.Location = new System.Drawing.Point(543, 125);
            this.btnOriginDown1.Name = "btnOriginDown1";
            this.btnOriginDown1.Size = new System.Drawing.Size(50, 50);
            this.btnOriginDown1.TabIndex = 320;
            this.btnOriginDown1.UseVisualStyleBackColor = true;
            this.btnOriginDown1.Click += new System.EventHandler(this.btOriginDown1_Click);
            // 
            // tbMove2
            // 
            this.tbMove2.Location = new System.Drawing.Point(1845, 275);
            this.tbMove2.Name = "tbMove2";
            this.tbMove2.Size = new System.Drawing.Size(50, 20);
            this.tbMove2.TabIndex = 332;
            this.tbMove2.Text = "1";
            // 
            // btnOriginRight2
            // 
            this.btnOriginRight2.Image = global::LaserMarking.Properties.Resources.Right;
            this.btnOriginRight2.Location = new System.Drawing.Point(1342, 125);
            this.btnOriginRight2.Name = "btnOriginRight2";
            this.btnOriginRight2.Size = new System.Drawing.Size(50, 50);
            this.btnOriginRight2.TabIndex = 329;
            this.btnOriginRight2.UseVisualStyleBackColor = true;
            this.btnOriginRight2.Click += new System.EventHandler(this.btnOriginRight2_Click);
            // 
            // btnOriginLeft2
            // 
            this.btnOriginLeft2.Image = global::LaserMarking.Properties.Resources.Left;
            this.btnOriginLeft2.Location = new System.Drawing.Point(1230, 125);
            this.btnOriginLeft2.Name = "btnOriginLeft2";
            this.btnOriginLeft2.Size = new System.Drawing.Size(50, 50);
            this.btnOriginLeft2.TabIndex = 328;
            this.btnOriginLeft2.UseVisualStyleBackColor = true;
            this.btnOriginLeft2.Click += new System.EventHandler(this.btnOriginLeft2_Click);
            // 
            // btnOriginUP2
            // 
            this.btnOriginUP2.Image = global::LaserMarking.Properties.Resources.Up;
            this.btnOriginUP2.Location = new System.Drawing.Point(1286, 69);
            this.btnOriginUP2.Name = "btnOriginUP2";
            this.btnOriginUP2.Size = new System.Drawing.Size(50, 50);
            this.btnOriginUP2.TabIndex = 326;
            this.btnOriginUP2.UseVisualStyleBackColor = true;
            this.btnOriginUP2.Click += new System.EventHandler(this.btnOriginUP2_Click);
            // 
            // btnOriginDown2
            // 
            this.btnOriginDown2.Image = global::LaserMarking.Properties.Resources.Down;
            this.btnOriginDown2.Location = new System.Drawing.Point(1286, 125);
            this.btnOriginDown2.Name = "btnOriginDown2";
            this.btnOriginDown2.Size = new System.Drawing.Size(50, 50);
            this.btnOriginDown2.TabIndex = 327;
            this.btnOriginDown2.UseVisualStyleBackColor = true;
            this.btnOriginDown2.Click += new System.EventHandler(this.btnOriginDown2_Click);
            // 
            // lbLCheck
            // 
            this.lbLCheck.AutoSize = true;
            this.lbLCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbLCheck.ForeColor = System.Drawing.Color.Red;
            this.lbLCheck.Location = new System.Drawing.Point(12, 216);
            this.lbLCheck.Name = "lbLCheck";
            this.lbLCheck.Size = new System.Drawing.Size(165, 37);
            this.lbLCheck.TabIndex = 333;
            this.lbLCheck.Text = "L- CHECK";
            this.lbLCheck.Visible = false;
            // 
            // tmrDisp
            // 
            this.tmrDisp.Interval = 500;
            this.tmrDisp.Tick += new System.EventHandler(this.tmrDisp_Tick);
            // 
            // frmManualMark
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.ControlBox = false;
            this.Controls.Add(this.lbLCheck);
            this.Controls.Add(this.tbMove2);
            this.Controls.Add(this.btnOriginRight2);
            this.Controls.Add(this.btnOriginLeft2);
            this.Controls.Add(this.btnOriginUP2);
            this.Controls.Add(this.btnOriginDown2);
            this.Controls.Add(this.tbMove1);
            this.Controls.Add(this.btnOriginRight1);
            this.Controls.Add(this.btnOriginLeft1);
            this.Controls.Add(this.btnOriginUP1);
            this.Controls.Add(this.btnOriginDown1);
            this.Controls.Add(this.cogDSPattern2);
            this.Controls.Add(this.cogDSPattern1);
            this.Controls.Add(this.txtImageGray2);
            this.Controls.Add(this.txtImageY2);
            this.Controls.Add(this.txtImageX2);
            this.Controls.Add(this.txtImageGray);
            this.Controls.Add(this.txtImageY);
            this.Controls.Add(this.txtImageX);
            this.Controls.Add(this.cbCreateLine2);
            this.Controls.Add(this.cbCreateLine1);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.button_ByPass);
            this.Controls.Add(this.button_Grab1);
            this.Controls.Add(this.button_FitImage);
            this.Controls.Add(this.button_Point);
            this.Controls.Add(this.button_Pan);
            this.Controls.Add(this.button_ZoomOut);
            this.Controls.Add(this.button_ZoomIn);
            this.Controls.Add(this.button_Grab);
            this.Controls.Add(this.btnFitImage);
            this.Controls.Add(this.btnPoint);
            this.Controls.Add(this.btnPan);
            this.Controls.Add(this.btnZoomOut);
            this.Controls.Add(this.btnZoomIn);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.btnConfirm2);
            this.Controls.Add(this.btnConfirm1);
            this.Controls.Add(this.lbRightMark);
            this.Controls.Add(this.lbLeftMark);
            this.Controls.Add(this.cogDS2);
            this.Controls.Add(this.lbTitle);
            this.Controls.Add(this.cogDS1);
            this.Name = "frmManualMark";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmManualMark";
            this.TopMost = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.cogDS1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cogDS2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cogDSPattern1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cogDSPattern2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Cognex.VisionPro.Display.CogDisplay cogDS1;
        public System.Windows.Forms.Label lbTitle;
        private Cognex.VisionPro.Display.CogDisplay cogDS2;
        public System.Windows.Forms.Label lbLeftMark;
        public System.Windows.Forms.Label lbRightMark;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Timer tmrMark;
        public System.Windows.Forms.Button btnConfirm1;
        public System.Windows.Forms.Button btnConfirm2;
        private System.Windows.Forms.Button btnFitImage;
        private System.Windows.Forms.Button btnPoint;
        private System.Windows.Forms.Button btnPan;
        private System.Windows.Forms.Button btnZoomOut;
        private System.Windows.Forms.Button btnZoomIn;
        private System.Windows.Forms.Button button_Grab;
        private System.Windows.Forms.Button button_Grab1;
        private System.Windows.Forms.Button button_FitImage;
        private System.Windows.Forms.Button button_Point;
        private System.Windows.Forms.Button button_Pan;
        private System.Windows.Forms.Button button_ZoomOut;
        private System.Windows.Forms.Button button_ZoomIn;
        private System.Windows.Forms.Button button_ByPass;
        public System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.CheckBox cbCreateLine1;
        private System.Windows.Forms.CheckBox cbCreateLine2;
        private System.Windows.Forms.TextBox txtImageGray;
        private System.Windows.Forms.TextBox txtImageY;
        private System.Windows.Forms.TextBox txtImageX;
        private System.Windows.Forms.TextBox txtImageGray2;
        private System.Windows.Forms.TextBox txtImageY2;
        private System.Windows.Forms.TextBox txtImageX2;
        private Cognex.VisionPro.Display.CogDisplay cogDSPattern1;
        private Cognex.VisionPro.Display.CogDisplay cogDSPattern2;
        private System.Windows.Forms.TextBox tbMove1;
        private System.Windows.Forms.Button btnOriginRight1;
        private System.Windows.Forms.Button btnOriginLeft1;
        private System.Windows.Forms.Button btnOriginUP1;
        private System.Windows.Forms.Button btnOriginDown1;
        private System.Windows.Forms.TextBox tbMove2;
        private System.Windows.Forms.Button btnOriginRight2;
        private System.Windows.Forms.Button btnOriginLeft2;
        private System.Windows.Forms.Button btnOriginUP2;
        private System.Windows.Forms.Button btnOriginDown2;
        private System.Windows.Forms.Label lbLCheck;
        private System.Windows.Forms.Timer tmrDisp;
    }
}