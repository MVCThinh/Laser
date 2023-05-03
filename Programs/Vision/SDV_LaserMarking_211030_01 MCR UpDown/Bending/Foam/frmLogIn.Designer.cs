namespace Bending
{
    partial class frmLogIn
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogIn));
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnOperator = new System.Windows.Forms.Button();
            this.btnEngineer = new System.Windows.Forms.Button();
            this.btnMaker = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtLoginPW = new System.Windows.Forms.TextBox();
            this.panel10 = new System.Windows.Forms.Panel();
            this.btnLogInOk = new System.Windows.Forms.Button();
            this.btnLogInCancel = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.panel10.SuspendLayout();
            this.SuspendLayout();
            // 
            // label10
            // 
            this.label10.BackColor = System.Drawing.Color.LightGray;
            this.label10.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.Black;
            this.label10.Location = new System.Drawing.Point(1, 1);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(563, 36);
            this.label10.TabIndex = 46;
            this.label10.Text = "[USER SELECT]";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label11
            // 
            this.label11.BackColor = System.Drawing.Color.Transparent;
            this.label11.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.Color.Black;
            this.label11.Location = new System.Drawing.Point(110, 3);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(245, 27);
            this.label11.TabIndex = 51;
            this.label11.Text = "CHOOSE A USER";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel1.Location = new System.Drawing.Point(6, 6);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(95, 114);
            this.panel1.TabIndex = 52;
            // 
            // btnOperator
            // 
            this.btnOperator.BackColor = System.Drawing.Color.White;
            this.btnOperator.FlatAppearance.BorderColor = System.Drawing.Color.LightSkyBlue;
            this.btnOperator.FlatAppearance.BorderSize = 2;
            this.btnOperator.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnOperator.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnOperator.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOperator.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOperator.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnOperator.Location = new System.Drawing.Point(110, 40);
            this.btnOperator.Name = "btnOperator";
            this.btnOperator.Size = new System.Drawing.Size(140, 35);
            this.btnOperator.TabIndex = 53;
            this.btnOperator.Text = "Operator";
            this.btnOperator.UseVisualStyleBackColor = false;
            // 
            // btnEngineer
            // 
            this.btnEngineer.BackColor = System.Drawing.Color.White;
            this.btnEngineer.FlatAppearance.BorderColor = System.Drawing.Color.LightSkyBlue;
            this.btnEngineer.FlatAppearance.BorderSize = 2;
            this.btnEngineer.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnEngineer.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnEngineer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEngineer.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEngineer.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnEngineer.Location = new System.Drawing.Point(260, 40);
            this.btnEngineer.Name = "btnEngineer";
            this.btnEngineer.Size = new System.Drawing.Size(140, 35);
            this.btnEngineer.TabIndex = 54;
            this.btnEngineer.Text = "Engineer";
            this.btnEngineer.UseVisualStyleBackColor = false;
            this.btnEngineer.Click += new System.EventHandler(this.btnEngineer_Click);
            // 
            // btnMaker
            // 
            this.btnMaker.BackColor = System.Drawing.Color.White;
            this.btnMaker.FlatAppearance.BorderColor = System.Drawing.Color.LightSkyBlue;
            this.btnMaker.FlatAppearance.BorderSize = 2;
            this.btnMaker.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnMaker.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnMaker.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMaker.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMaker.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnMaker.Location = new System.Drawing.Point(410, 40);
            this.btnMaker.Name = "btnMaker";
            this.btnMaker.Size = new System.Drawing.Size(140, 35);
            this.btnMaker.TabIndex = 55;
            this.btnMaker.Text = "Maker";
            this.btnMaker.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(107, 82);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(342, 27);
            this.label1.TabIndex = 56;
            this.label1.Text = "Enter Password(4 Char) Please!";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtLoginPW
            // 
            this.txtLoginPW.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtLoginPW.Location = new System.Drawing.Point(111, 122);
            this.txtLoginPW.Name = "txtLoginPW";
            this.txtLoginPW.PasswordChar = '*';
            this.txtLoginPW.Size = new System.Drawing.Size(116, 22);
            this.txtLoginPW.TabIndex = 57;
            this.txtLoginPW.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtLoginPW_KeyDown);
            // 
            // panel10
            // 
            this.panel10.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel10.Controls.Add(this.panel1);
            this.panel10.Controls.Add(this.txtLoginPW);
            this.panel10.Controls.Add(this.label11);
            this.panel10.Controls.Add(this.label1);
            this.panel10.Controls.Add(this.btnOperator);
            this.panel10.Controls.Add(this.btnMaker);
            this.panel10.Controls.Add(this.btnEngineer);
            this.panel10.Location = new System.Drawing.Point(2, 43);
            this.panel10.Name = "panel10";
            this.panel10.Size = new System.Drawing.Size(562, 168);
            this.panel10.TabIndex = 58;
            // 
            // btnLogInOk
            // 
            this.btnLogInOk.BackColor = System.Drawing.Color.White;
            this.btnLogInOk.FlatAppearance.BorderColor = System.Drawing.Color.LightSkyBlue;
            this.btnLogInOk.FlatAppearance.BorderSize = 2;
            this.btnLogInOk.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnLogInOk.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnLogInOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogInOk.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLogInOk.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnLogInOk.Location = new System.Drawing.Point(161, 217);
            this.btnLogInOk.Name = "btnLogInOk";
            this.btnLogInOk.Size = new System.Drawing.Size(107, 35);
            this.btnLogInOk.TabIndex = 59;
            this.btnLogInOk.Text = "OK";
            this.btnLogInOk.UseVisualStyleBackColor = false;
            this.btnLogInOk.Click += new System.EventHandler(this.btnLogInOk_Click);
            // 
            // btnLogInCancel
            // 
            this.btnLogInCancel.BackColor = System.Drawing.Color.White;
            this.btnLogInCancel.FlatAppearance.BorderColor = System.Drawing.Color.LightSkyBlue;
            this.btnLogInCancel.FlatAppearance.BorderSize = 2;
            this.btnLogInCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnLogInCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnLogInCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogInCancel.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLogInCancel.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnLogInCancel.Location = new System.Drawing.Point(274, 217);
            this.btnLogInCancel.Name = "btnLogInCancel";
            this.btnLogInCancel.Size = new System.Drawing.Size(107, 35);
            this.btnLogInCancel.TabIndex = 60;
            this.btnLogInCancel.Text = "Cancel";
            this.btnLogInCancel.UseVisualStyleBackColor = false;
            this.btnLogInCancel.Click += new System.EventHandler(this.btnLogInCancel_Click);
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.White;
            this.btnExit.FlatAppearance.BorderColor = System.Drawing.Color.LightSkyBlue;
            this.btnExit.FlatAppearance.BorderSize = 2;
            this.btnExit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnExit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExit.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnExit.Location = new System.Drawing.Point(457, 217);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(107, 35);
            this.btnExit.TabIndex = 61;
            this.btnExit.Text = "Close";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // frmLogIn
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(565, 258);
            this.ControlBox = false;
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnLogInCancel);
            this.Controls.Add(this.btnLogInOk);
            this.Controls.Add(this.panel10);
            this.Controls.Add(this.label10);
            this.Name = "frmLogIn";
            this.Text = "frmLogIn";
            this.panel10.ResumeLayout(false);
            this.panel10.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label label10;
        public System.Windows.Forms.Label label11;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnOperator;
        private System.Windows.Forms.Button btnEngineer;
        private System.Windows.Forms.Button btnMaker;
        public System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtLoginPW;
        private System.Windows.Forms.Panel panel10;
        private System.Windows.Forms.Button btnLogInOk;
        private System.Windows.Forms.Button btnLogInCancel;
        private System.Windows.Forms.Button btnExit;
    }
}