namespace Bending
{
    partial class frmMessage
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
            this.lblMessage = new System.Windows.Forms.Label();
            this.gbZigSet = new System.Windows.Forms.GroupBox();
            this.txtMarkToMark = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.label16 = new System.Windows.Forms.Label();
            this.txtResolutionRX = new System.Windows.Forms.TextBox();
            this.tb_MeasureRX = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.txtResolutionLX = new System.Windows.Forms.TextBox();
            this.tb_MeasureLX = new System.Windows.Forms.TextBox();
            this.label24 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.txtResolutionRY = new System.Windows.Forms.TextBox();
            this.tb_MeasureRY = new System.Windows.Forms.TextBox();
            this.label26 = new System.Windows.Forms.Label();
            this.txtResolutionLY = new System.Windows.Forms.TextBox();
            this.label27 = new System.Windows.Forms.Label();
            this.tb_MeasureLY = new System.Windows.Forms.TextBox();
            this.btnCalibration = new System.Windows.Forms.Button();
            this.lbl_ScaleName = new System.Windows.Forms.Label();
            this.gbZigSet.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblMessage
            // 
            this.lblMessage.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessage.Location = new System.Drawing.Point(14, 8);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(922, 75);
            this.lblMessage.TabIndex = 0;
            this.lblMessage.Text = "label1";
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // gbZigSet
            // 
            this.gbZigSet.Controls.Add(this.txtMarkToMark);
            this.gbZigSet.Controls.Add(this.label1);
            this.gbZigSet.Controls.Add(this.btnCancel);
            this.gbZigSet.Controls.Add(this.btnSave);
            this.gbZigSet.Controls.Add(this.label16);
            this.gbZigSet.Controls.Add(this.txtResolutionRX);
            this.gbZigSet.Controls.Add(this.tb_MeasureRX);
            this.gbZigSet.Controls.Add(this.label17);
            this.gbZigSet.Controls.Add(this.txtResolutionLX);
            this.gbZigSet.Controls.Add(this.tb_MeasureLX);
            this.gbZigSet.Controls.Add(this.label24);
            this.gbZigSet.Controls.Add(this.label25);
            this.gbZigSet.Controls.Add(this.txtResolutionRY);
            this.gbZigSet.Controls.Add(this.tb_MeasureRY);
            this.gbZigSet.Controls.Add(this.label26);
            this.gbZigSet.Controls.Add(this.txtResolutionLY);
            this.gbZigSet.Controls.Add(this.label27);
            this.gbZigSet.Controls.Add(this.tb_MeasureLY);
            this.gbZigSet.Controls.Add(this.btnCalibration);
            this.gbZigSet.Controls.Add(this.lbl_ScaleName);
            this.gbZigSet.Location = new System.Drawing.Point(12, 104);
            this.gbZigSet.Name = "gbZigSet";
            this.gbZigSet.Size = new System.Drawing.Size(843, 182);
            this.gbZigSet.TabIndex = 298;
            this.gbZigSet.TabStop = false;
            // 
            // txtMarkToMark
            // 
            this.txtMarkToMark.BackColor = System.Drawing.Color.White;
            this.txtMarkToMark.Location = new System.Drawing.Point(243, 38);
            this.txtMarkToMark.Name = "txtMarkToMark";
            this.txtMarkToMark.Size = new System.Drawing.Size(81, 21);
            this.txtMarkToMark.TabIndex = 313;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(182, 12);
            this.label1.TabIndex = 312;
            this.label1.Text = "Reference Mark to Mark Length";
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.White;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.LightSkyBlue;
            this.btnCancel.FlatAppearance.BorderSize = 2;
            this.btnCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnCancel.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ForeColor = System.Drawing.Color.Red;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnCancel.Location = new System.Drawing.Point(668, 126);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(125, 35);
            this.btnCancel.TabIndex = 311;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.White;
            this.btnSave.FlatAppearance.BorderColor = System.Drawing.Color.LightSkyBlue;
            this.btnSave.FlatAppearance.BorderSize = 2;
            this.btnSave.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnSave.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.Black;
            this.btnSave.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnSave.Location = new System.Drawing.Point(514, 126);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(125, 35);
            this.btnSave.TabIndex = 310;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(322, 73);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(21, 12);
            this.label16.TabIndex = 309;
            this.label16.Text = "RX";
            // 
            // txtResolutionRX
            // 
            this.txtResolutionRX.Location = new System.Drawing.Point(295, 135);
            this.txtResolutionRX.Name = "txtResolutionRX";
            this.txtResolutionRX.ReadOnly = true;
            this.txtResolutionRX.Size = new System.Drawing.Size(81, 21);
            this.txtResolutionRX.TabIndex = 307;
            // 
            // tb_MeasureRX
            // 
            this.tb_MeasureRX.BackColor = System.Drawing.Color.White;
            this.tb_MeasureRX.Location = new System.Drawing.Point(295, 91);
            this.tb_MeasureRX.Name = "tb_MeasureRX";
            this.tb_MeasureRX.Size = new System.Drawing.Size(81, 21);
            this.tb_MeasureRX.TabIndex = 308;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(135, 73);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(20, 12);
            this.label17.TabIndex = 305;
            this.label17.Text = "LX";
            // 
            // txtResolutionLX
            // 
            this.txtResolutionLX.Location = new System.Drawing.Point(107, 135);
            this.txtResolutionLX.Name = "txtResolutionLX";
            this.txtResolutionLX.ReadOnly = true;
            this.txtResolutionLX.Size = new System.Drawing.Size(81, 21);
            this.txtResolutionLX.TabIndex = 303;
            // 
            // tb_MeasureLX
            // 
            this.tb_MeasureLX.BackColor = System.Drawing.Color.White;
            this.tb_MeasureLX.Location = new System.Drawing.Point(107, 91);
            this.tb_MeasureLX.Name = "tb_MeasureLX";
            this.tb_MeasureLX.Size = new System.Drawing.Size(81, 21);
            this.tb_MeasureLX.TabIndex = 304;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(416, 73);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(21, 12);
            this.label24.TabIndex = 300;
            this.label24.Text = "RY";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(227, 73);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(20, 12);
            this.label25.TabIndex = 298;
            this.label25.Text = "LY";
            // 
            // txtResolutionRY
            // 
            this.txtResolutionRY.Location = new System.Drawing.Point(390, 135);
            this.txtResolutionRY.Name = "txtResolutionRY";
            this.txtResolutionRY.ReadOnly = true;
            this.txtResolutionRY.Size = new System.Drawing.Size(81, 21);
            this.txtResolutionRY.TabIndex = 294;
            // 
            // tb_MeasureRY
            // 
            this.tb_MeasureRY.BackColor = System.Drawing.Color.White;
            this.tb_MeasureRY.Location = new System.Drawing.Point(390, 91);
            this.tb_MeasureRY.Name = "tb_MeasureRY";
            this.tb_MeasureRY.Size = new System.Drawing.Size(81, 21);
            this.tb_MeasureRY.TabIndex = 293;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(30, 143);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(64, 12);
            this.label26.TabIndex = 297;
            this.label26.Text = "Resolution";
            // 
            // txtResolutionLY
            // 
            this.txtResolutionLY.Location = new System.Drawing.Point(199, 135);
            this.txtResolutionLY.Name = "txtResolutionLY";
            this.txtResolutionLY.ReadOnly = true;
            this.txtResolutionLY.Size = new System.Drawing.Size(81, 21);
            this.txtResolutionLY.TabIndex = 292;
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(30, 99);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(59, 12);
            this.label27.TabIndex = 299;
            this.label27.Text = "Zig Value";
            // 
            // tb_MeasureLY
            // 
            this.tb_MeasureLY.BackColor = System.Drawing.Color.White;
            this.tb_MeasureLY.Location = new System.Drawing.Point(199, 91);
            this.tb_MeasureLY.Name = "tb_MeasureLY";
            this.tb_MeasureLY.Size = new System.Drawing.Size(81, 21);
            this.tb_MeasureLY.TabIndex = 291;
            // 
            // btnCalibration
            // 
            this.btnCalibration.BackColor = System.Drawing.Color.White;
            this.btnCalibration.FlatAppearance.BorderColor = System.Drawing.Color.LightSkyBlue;
            this.btnCalibration.FlatAppearance.BorderSize = 2;
            this.btnCalibration.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnCalibration.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnCalibration.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnCalibration.ForeColor = System.Drawing.Color.Black;
            this.btnCalibration.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnCalibration.Location = new System.Drawing.Point(514, 82);
            this.btnCalibration.Name = "btnCalibration";
            this.btnCalibration.Size = new System.Drawing.Size(125, 35);
            this.btnCalibration.TabIndex = 290;
            this.btnCalibration.Text = "Calibration";
            this.btnCalibration.UseVisualStyleBackColor = false;
            this.btnCalibration.Click += new System.EventHandler(this.btnCalibration_Click);
            // 
            // lbl_ScaleName
            // 
            this.lbl_ScaleName.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.lbl_ScaleName.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.lbl_ScaleName.ForeColor = System.Drawing.Color.White;
            this.lbl_ScaleName.Location = new System.Drawing.Point(6, -2);
            this.lbl_ScaleName.Name = "lbl_ScaleName";
            this.lbl_ScaleName.Size = new System.Drawing.Size(337, 25);
            this.lbl_ScaleName.TabIndex = 282;
            this.lbl_ScaleName.Text = "Zig Setting (mm)";
            this.lbl_ScaleName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // frmMessage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(952, 89);
            this.ControlBox = false;
            this.Controls.Add(this.gbZigSet);
            this.Controls.Add(this.lblMessage);
            this.Location = new System.Drawing.Point(100, 800);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmMessage";
            this.Text = "Message";
            this.TopMost = true;
            this.gbZigSet.ResumeLayout(false);
            this.gbZigSet.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.GroupBox gbZigSet;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox txtResolutionRX;
        public System.Windows.Forms.TextBox tb_MeasureRX;
        private System.Windows.Forms.Label label17;
        public System.Windows.Forms.TextBox txtResolutionLX;
        public System.Windows.Forms.TextBox tb_MeasureLX;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.TextBox txtResolutionRY;
        public System.Windows.Forms.TextBox tb_MeasureRY;
        private System.Windows.Forms.Label label26;
        public System.Windows.Forms.TextBox txtResolutionLY;
        private System.Windows.Forms.Label label27;
        public System.Windows.Forms.TextBox tb_MeasureLY;
        private System.Windows.Forms.Button btnCalibration;
        public System.Windows.Forms.Label lbl_ScaleName;
        public System.Windows.Forms.TextBox txtMarkToMark;
        private System.Windows.Forms.Label label1;
    }
}