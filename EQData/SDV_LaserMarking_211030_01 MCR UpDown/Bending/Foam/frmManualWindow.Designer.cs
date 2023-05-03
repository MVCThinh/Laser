namespace Bending
{
    partial class frmManualWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmManualWindow));
            this.cogDS1 = new Cognex.VisionPro.Display.CogDisplay();
            this.lbTitle = new System.Windows.Forms.Label();
            this.cogDS2 = new Cognex.VisionPro.Display.CogDisplay();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnFitImage = new System.Windows.Forms.Button();
            this.btnZoomOut = new System.Windows.Forms.Button();
            this.btnZoomIn = new System.Windows.Forms.Button();
            this.button_Grab = new System.Windows.Forms.Button();
            this.button_Grab1 = new System.Windows.Forms.Button();
            this.button_FitImage = new System.Windows.Forms.Button();
            this.button_ZoomOut = new System.Windows.Forms.Button();
            this.button_ZoomIn = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnNG = new System.Windows.Forms.Button();
            this.btnRetry = new System.Windows.Forms.Button();
            this.btnMeasure = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lbDataLX = new System.Windows.Forms.Label();
            this.lbDataLY = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lbDataRY = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lbDataRX = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lbSpecRY = new System.Windows.Forms.Label();
            this.lbSpecRX = new System.Windows.Forms.Label();
            this.lbSpecLY = new System.Windows.Forms.Label();
            this.lbSpecLX = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.pnDataAndSpec = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.cogDS1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cogDS2)).BeginInit();
            this.pnDataAndSpec.SuspendLayout();
            this.SuspendLayout();
            // 
            // cogDS1
            // 
            this.cogDS1.ColorMapLowerClipColor = System.Drawing.Color.Black;
            this.cogDS1.ColorMapLowerRoiLimit = 0D;
            this.cogDS1.ColorMapPredefined = Cognex.VisionPro.Display.CogDisplayColorMapPredefinedConstants.None;
            this.cogDS1.ColorMapUpperClipColor = System.Drawing.Color.Black;
            this.cogDS1.ColorMapUpperRoiLimit = 1D;
            this.cogDS1.Location = new System.Drawing.Point(8, 148);
            this.cogDS1.MouseWheelMode = Cognex.VisionPro.Display.CogDisplayMouseWheelModeConstants.Zoom1;
            this.cogDS1.MouseWheelSensitivity = 1D;
            this.cogDS1.Name = "cogDS1";
            this.cogDS1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("cogDS1.OcxState")));
            this.cogDS1.Size = new System.Drawing.Size(700, 450);
            this.cogDS1.TabIndex = 10;
            // 
            // lbTitle
            // 
            this.lbTitle.BackColor = System.Drawing.Color.DimGray;
            this.lbTitle.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbTitle.ForeColor = System.Drawing.Color.White;
            this.lbTitle.Location = new System.Drawing.Point(338, 8);
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Size = new System.Drawing.Size(724, 38);
            this.lbTitle.TabIndex = 131;
            this.lbTitle.Text = "Manual Window";
            this.lbTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cogDS2
            // 
            this.cogDS2.ColorMapLowerClipColor = System.Drawing.Color.Black;
            this.cogDS2.ColorMapLowerRoiLimit = 0D;
            this.cogDS2.ColorMapPredefined = Cognex.VisionPro.Display.CogDisplayColorMapPredefinedConstants.None;
            this.cogDS2.ColorMapUpperClipColor = System.Drawing.Color.Black;
            this.cogDS2.ColorMapUpperRoiLimit = 1D;
            this.cogDS2.Location = new System.Drawing.Point(714, 148);
            this.cogDS2.MouseWheelMode = Cognex.VisionPro.Display.CogDisplayMouseWheelModeConstants.Zoom1;
            this.cogDS2.MouseWheelSensitivity = 1D;
            this.cogDS2.Name = "cogDS2";
            this.cogDS2.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("cogDS2.OcxState")));
            this.cogDS2.Size = new System.Drawing.Size(700, 450);
            this.cogDS2.TabIndex = 132;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Black;
            this.label2.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Yellow;
            this.label2.Location = new System.Drawing.Point(9, 124);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(699, 21);
            this.label2.TabIndex = 133;
            this.label2.Text = "Left";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Black;
            this.label3.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Yellow;
            this.label3.Location = new System.Drawing.Point(714, 124);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(700, 21);
            this.label3.TabIndex = 134;
            this.label3.Text = "Right";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnFitImage
            // 
            this.btnFitImage.BackColor = System.Drawing.Color.White;
            this.btnFitImage.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.btnFitImage.FlatAppearance.BorderSize = 2;
            this.btnFitImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFitImage.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFitImage.ForeColor = System.Drawing.Color.Black;
            this.btnFitImage.Location = new System.Drawing.Point(292, 604);
            this.btnFitImage.Name = "btnFitImage";
            this.btnFitImage.Size = new System.Drawing.Size(121, 48);
            this.btnFitImage.TabIndex = 297;
            this.btnFitImage.Text = "Fit Image";
            this.btnFitImage.UseVisualStyleBackColor = false;
            this.btnFitImage.Click += new System.EventHandler(this.btnFitImage_Click);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.BackColor = System.Drawing.Color.White;
            this.btnZoomOut.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.btnZoomOut.FlatAppearance.BorderSize = 2;
            this.btnZoomOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnZoomOut.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnZoomOut.ForeColor = System.Drawing.Color.Black;
            this.btnZoomOut.Location = new System.Drawing.Point(152, 604);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(121, 48);
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
            this.btnZoomIn.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnZoomIn.ForeColor = System.Drawing.Color.Black;
            this.btnZoomIn.Location = new System.Drawing.Point(12, 604);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new System.Drawing.Size(121, 48);
            this.btnZoomIn.TabIndex = 295;
            this.btnZoomIn.Text = "Zoom IN";
            this.btnZoomIn.UseVisualStyleBackColor = false;
            this.btnZoomIn.Click += new System.EventHandler(this.btnZoomIn_Click);
            // 
            // button_Grab
            // 
            this.button_Grab.BackColor = System.Drawing.Color.White;
            this.button_Grab.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.button_Grab.FlatAppearance.BorderSize = 2;
            this.button_Grab.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_Grab.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Grab.ForeColor = System.Drawing.Color.Black;
            this.button_Grab.Location = new System.Drawing.Point(432, 604);
            this.button_Grab.Name = "button_Grab";
            this.button_Grab.Size = new System.Drawing.Size(121, 48);
            this.button_Grab.TabIndex = 299;
            this.button_Grab.Text = "Image Grab";
            this.button_Grab.UseVisualStyleBackColor = false;
            this.button_Grab.Click += new System.EventHandler(this.button_Grab_Click);
            // 
            // button_Grab1
            // 
            this.button_Grab1.BackColor = System.Drawing.Color.White;
            this.button_Grab1.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.button_Grab1.FlatAppearance.BorderSize = 2;
            this.button_Grab1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_Grab1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Grab1.ForeColor = System.Drawing.Color.Black;
            this.button_Grab1.Location = new System.Drawing.Point(1293, 604);
            this.button_Grab1.Name = "button_Grab1";
            this.button_Grab1.Size = new System.Drawing.Size(121, 48);
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
            this.button_FitImage.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_FitImage.ForeColor = System.Drawing.Color.Black;
            this.button_FitImage.Location = new System.Drawing.Point(1152, 604);
            this.button_FitImage.Name = "button_FitImage";
            this.button_FitImage.Size = new System.Drawing.Size(121, 48);
            this.button_FitImage.TabIndex = 304;
            this.button_FitImage.Text = "Fit Image";
            this.button_FitImage.UseVisualStyleBackColor = false;
            this.button_FitImage.Click += new System.EventHandler(this.button_FitImage_Click);
            // 
            // button_ZoomOut
            // 
            this.button_ZoomOut.BackColor = System.Drawing.Color.White;
            this.button_ZoomOut.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.button_ZoomOut.FlatAppearance.BorderSize = 2;
            this.button_ZoomOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_ZoomOut.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_ZoomOut.ForeColor = System.Drawing.Color.Black;
            this.button_ZoomOut.Location = new System.Drawing.Point(1005, 604);
            this.button_ZoomOut.Name = "button_ZoomOut";
            this.button_ZoomOut.Size = new System.Drawing.Size(121, 48);
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
            this.button_ZoomIn.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_ZoomIn.ForeColor = System.Drawing.Color.Black;
            this.button_ZoomIn.Location = new System.Drawing.Point(866, 604);
            this.button_ZoomIn.Name = "button_ZoomIn";
            this.button_ZoomIn.Size = new System.Drawing.Size(121, 48);
            this.button_ZoomIn.TabIndex = 302;
            this.button_ZoomIn.Text = "Zoom IN";
            this.button_ZoomIn.UseVisualStyleBackColor = false;
            this.button_ZoomIn.Click += new System.EventHandler(this.button_ZoomIn_Click);
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(1307, 709);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(107, 40);
            this.btnClose.TabIndex = 307;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Visible = false;
            this.btnClose.Click += new System.EventHandler(this.Close_Click);
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.btnOK.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.btnOK.FlatAppearance.BorderSize = 2;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOK.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.ForeColor = System.Drawing.Color.White;
            this.btnOK.Location = new System.Drawing.Point(475, 58);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(139, 61);
            this.btnOK.TabIndex = 308;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnNG
            // 
            this.btnNG.BackColor = System.Drawing.Color.Red;
            this.btnNG.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.btnNG.FlatAppearance.BorderSize = 2;
            this.btnNG.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNG.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNG.ForeColor = System.Drawing.Color.White;
            this.btnNG.Location = new System.Drawing.Point(645, 58);
            this.btnNG.Name = "btnNG";
            this.btnNG.Size = new System.Drawing.Size(139, 61);
            this.btnNG.TabIndex = 309;
            this.btnNG.Text = "NG";
            this.btnNG.UseVisualStyleBackColor = false;
            this.btnNG.Click += new System.EventHandler(this.btnNG_Click);
            // 
            // btnRetry
            // 
            this.btnRetry.BackColor = System.Drawing.Color.White;
            this.btnRetry.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.btnRetry.FlatAppearance.BorderSize = 2;
            this.btnRetry.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRetry.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRetry.ForeColor = System.Drawing.Color.Black;
            this.btnRetry.Location = new System.Drawing.Point(812, 60);
            this.btnRetry.Name = "btnRetry";
            this.btnRetry.Size = new System.Drawing.Size(139, 61);
            this.btnRetry.TabIndex = 310;
            this.btnRetry.Text = "Retry";
            this.btnRetry.UseVisualStyleBackColor = false;
            this.btnRetry.Click += new System.EventHandler(this.btnRetry_Click);
            // 
            // btnMeasure
            // 
            this.btnMeasure.BackColor = System.Drawing.Color.Gold;
            this.btnMeasure.FlatAppearance.BorderColor = System.Drawing.Color.SkyBlue;
            this.btnMeasure.FlatAppearance.BorderSize = 2;
            this.btnMeasure.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMeasure.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMeasure.ForeColor = System.Drawing.Color.Black;
            this.btnMeasure.Location = new System.Drawing.Point(636, 604);
            this.btnMeasure.Name = "btnMeasure";
            this.btnMeasure.Size = new System.Drawing.Size(146, 48);
            this.btnMeasure.TabIndex = 311;
            this.btnMeasure.Text = "Measurement";
            this.btnMeasure.UseVisualStyleBackColor = false;
            this.btnMeasure.Click += new System.EventHandler(this.btnMeasure_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.label1.Location = new System.Drawing.Point(116, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 25);
            this.label1.TabIndex = 312;
            this.label1.Text = "LX";
            // 
            // lbDataLX
            // 
            this.lbDataLX.AutoSize = true;
            this.lbDataLX.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDataLX.Location = new System.Drawing.Point(100, 41);
            this.lbDataLX.Name = "lbDataLX";
            this.lbDataLX.Size = new System.Drawing.Size(71, 25);
            this.lbDataLX.TabIndex = 313;
            this.lbDataLX.Text = "4.095";
            // 
            // lbDataLY
            // 
            this.lbDataLY.AutoSize = true;
            this.lbDataLY.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDataLY.Location = new System.Drawing.Point(216, 41);
            this.lbDataLY.Name = "lbDataLY";
            this.lbDataLY.Size = new System.Drawing.Size(71, 25);
            this.lbDataLY.TabIndex = 315;
            this.lbDataLY.Text = "4.095";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.label6.Location = new System.Drawing.Point(230, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 25);
            this.label6.TabIndex = 314;
            this.label6.Text = "LY";
            // 
            // lbDataRY
            // 
            this.lbDataRY.AutoSize = true;
            this.lbDataRY.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDataRY.Location = new System.Drawing.Point(436, 41);
            this.lbDataRY.Name = "lbDataRY";
            this.lbDataRY.Size = new System.Drawing.Size(71, 25);
            this.lbDataRY.TabIndex = 319;
            this.lbDataRY.Text = "4.095";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.label8.Location = new System.Drawing.Point(436, 9);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(44, 25);
            this.label8.TabIndex = 318;
            this.label8.Text = "RY";
            // 
            // lbDataRX
            // 
            this.lbDataRX.AutoSize = true;
            this.lbDataRX.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDataRX.Location = new System.Drawing.Point(321, 41);
            this.lbDataRX.Name = "lbDataRX";
            this.lbDataRX.Size = new System.Drawing.Size(71, 25);
            this.lbDataRX.TabIndex = 317;
            this.lbDataRX.Text = "4.095";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.label10.Location = new System.Drawing.Point(327, 9);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(43, 25);
            this.label10.TabIndex = 316;
            this.label10.Text = "RX";
            // 
            // lbSpecRY
            // 
            this.lbSpecRY.AutoSize = true;
            this.lbSpecRY.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbSpecRY.Location = new System.Drawing.Point(441, 70);
            this.lbSpecRY.Name = "lbSpecRY";
            this.lbSpecRY.Size = new System.Drawing.Size(55, 24);
            this.lbSpecRY.TabIndex = 323;
            this.lbSpecRY.Text = "4.095";
            // 
            // lbSpecRX
            // 
            this.lbSpecRX.AutoSize = true;
            this.lbSpecRX.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbSpecRX.Location = new System.Drawing.Point(328, 70);
            this.lbSpecRX.Name = "lbSpecRX";
            this.lbSpecRX.Size = new System.Drawing.Size(55, 24);
            this.lbSpecRX.TabIndex = 322;
            this.lbSpecRX.Text = "4.095";
            // 
            // lbSpecLY
            // 
            this.lbSpecLY.AutoSize = true;
            this.lbSpecLY.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbSpecLY.Location = new System.Drawing.Point(223, 70);
            this.lbSpecLY.Name = "lbSpecLY";
            this.lbSpecLY.Size = new System.Drawing.Size(55, 24);
            this.lbSpecLY.TabIndex = 321;
            this.lbSpecLY.Text = "4.095";
            // 
            // lbSpecLX
            // 
            this.lbSpecLX.AutoSize = true;
            this.lbSpecLX.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbSpecLX.Location = new System.Drawing.Point(107, 70);
            this.lbSpecLX.Name = "lbSpecLX";
            this.lbSpecLX.Size = new System.Drawing.Size(55, 24);
            this.lbSpecLX.TabIndex = 320;
            this.lbSpecLX.Text = "4.095";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.label15.Location = new System.Drawing.Point(9, 41);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(72, 25);
            this.label15.TabIndex = 324;
            this.label15.Text = "DATA";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.label16.Location = new System.Drawing.Point(9, 70);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(60, 24);
            this.label16.TabIndex = 325;
            this.label16.Text = "SPEC";
            // 
            // pnDataAndSpec
            // 
            this.pnDataAndSpec.Controls.Add(this.label1);
            this.pnDataAndSpec.Controls.Add(this.label16);
            this.pnDataAndSpec.Controls.Add(this.lbDataLX);
            this.pnDataAndSpec.Controls.Add(this.label15);
            this.pnDataAndSpec.Controls.Add(this.label6);
            this.pnDataAndSpec.Controls.Add(this.lbSpecRY);
            this.pnDataAndSpec.Controls.Add(this.lbDataLY);
            this.pnDataAndSpec.Controls.Add(this.lbSpecRX);
            this.pnDataAndSpec.Controls.Add(this.label10);
            this.pnDataAndSpec.Controls.Add(this.lbSpecLY);
            this.pnDataAndSpec.Controls.Add(this.lbDataRX);
            this.pnDataAndSpec.Controls.Add(this.lbSpecLX);
            this.pnDataAndSpec.Controls.Add(this.label8);
            this.pnDataAndSpec.Controls.Add(this.lbDataRY);
            this.pnDataAndSpec.Location = new System.Drawing.Point(445, 658);
            this.pnDataAndSpec.Name = "pnDataAndSpec";
            this.pnDataAndSpec.Size = new System.Drawing.Size(516, 100);
            this.pnDataAndSpec.TabIndex = 326;
            // 
            // frmManualWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1426, 761);
            this.ControlBox = false;
            this.Controls.Add(this.pnDataAndSpec);
            this.Controls.Add(this.btnMeasure);
            this.Controls.Add(this.btnRetry);
            this.Controls.Add(this.btnNG);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.button_Grab1);
            this.Controls.Add(this.button_FitImage);
            this.Controls.Add(this.button_ZoomOut);
            this.Controls.Add(this.button_ZoomIn);
            this.Controls.Add(this.button_Grab);
            this.Controls.Add(this.btnFitImage);
            this.Controls.Add(this.btnZoomOut);
            this.Controls.Add(this.btnZoomIn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cogDS2);
            this.Controls.Add(this.lbTitle);
            this.Controls.Add(this.cogDS1);
            this.Name = "frmManualWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmManualMark";
            this.TopMost = true;
            this.VisibleChanged += new System.EventHandler(this.frmManualBending_VisibleChanged);
            ((System.ComponentModel.ISupportInitialize)(this.cogDS1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cogDS2)).EndInit();
            this.pnDataAndSpec.ResumeLayout(false);
            this.pnDataAndSpec.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Cognex.VisionPro.Display.CogDisplay cogDS1;
        public System.Windows.Forms.Label lbTitle;
        private Cognex.VisionPro.Display.CogDisplay cogDS2;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnFitImage;
        private System.Windows.Forms.Button btnZoomOut;
        private System.Windows.Forms.Button btnZoomIn;
        private System.Windows.Forms.Button button_Grab;
        private System.Windows.Forms.Button button_Grab1;
        private System.Windows.Forms.Button button_FitImage;
        private System.Windows.Forms.Button button_ZoomOut;
        private System.Windows.Forms.Button button_ZoomIn;
        public System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnNG;
        private System.Windows.Forms.Button btnRetry;
        private System.Windows.Forms.Button btnMeasure;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbDataLX;
        private System.Windows.Forms.Label lbDataLY;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lbDataRY;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lbDataRX;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label lbSpecRY;
        private System.Windows.Forms.Label lbSpecRX;
        private System.Windows.Forms.Label lbSpecLY;
        private System.Windows.Forms.Label lbSpecLX;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Panel pnDataAndSpec;
    }
}