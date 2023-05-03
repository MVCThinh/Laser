using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bending
{
    public partial class frmLogIn : Form
    {
        Button[] btnLoginUser;
        string LoginUser;

        public frmLogIn()
        {
            InitializeComponent();

            btnLoginUser = new Button[] { btnOperator, btnEngineer, btnMaker };
            for (int i = 0; i < btnLoginUser.Length; i++)
            {
                btnLoginUser[i].Click += new System.EventHandler(btnLoginUser_Click);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Visible = false;
        }

        private void btnLoginUser_Click(object sender, EventArgs e)
        {
            string lcText = (sender as Button).Text;

            for (short i = 0; i < btnLoginUser.Length; i++)
            {
                if (lcText == btnLoginUser[i].Text)
                {
                    LoginUser = lcText;
                    btnLoginUser[i].BackColor = Color.LightSkyBlue;
                    btnLoginUser[i].ForeColor = Color.Black;
                }
                else
                {
                    btnLoginUser[i].BackColor = Color.White;
                    btnLoginUser[i].ForeColor = Color.Black;
                }
            }
        }

        private void Login()
        {
            //string LoginPW = ucSetting.DB.LoginIDPWCheck(LoginUser);
            string LoginPW = ucSetting.cCFG.LoginIDPWCheck(LoginUser);
            if (txtLoginPW.Text == LoginPW)
            {
                Bending.Menu.loginCheckStart = DateTime.Now;
                CONST.LoginID = LoginUser;
                btnExit.PerformClick();
            }
            else
            {
                MessageBox.Show("Confrm Password");
            }
            txtLoginPW.Text = "";
        }

        private void btnLogInOk_Click(object sender, EventArgs e)
        {
            Login();            
        }

        private void btnLogInCancel_Click(object sender, EventArgs e)
        {
            txtLoginPW.Text = ""; 
        }

        public bool LogInCheck()
        {
            if (CONST.LoginID == "Operator") return false;
            else if (CONST.LoginID == "Engineer") return true;
            else if (CONST.LoginID == "Maker") return true;
            return false;
        }

        private void btnEngineer_Click(object sender, EventArgs e)
        {

        }

        private void txtLoginPW_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                Login();
            }
        }
        


    }
}
