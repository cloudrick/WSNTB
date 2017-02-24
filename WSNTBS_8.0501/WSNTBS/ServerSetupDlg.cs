using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WSNTBS
{
    public partial class MySQLServerSetupForm : Form
    {
        private TBConfiguration m_refToConfig;

        public MySQLServerSetupForm(TBConfiguration config)
        {
            InitializeComponent();
            m_refToConfig = config;

            mysql_host = m_refToConfig.GetDatabaseHost();
            mysql_database = m_refToConfig.GetDatabase();
            mysql_username = m_refToConfig.GetDatabaseUsername();
            mysql_password = m_refToConfig.GetDatabasePassword();
        }       

        public string mysql_host
        {
            get { return textBox1.Text ;}
            set { textBox1.Text = value ;} 
        }

        public string mysql_database
        {
            get { return textBox2.Text; }
            set { textBox2.Text = value; } 
        }

        public string mysql_username
        {
            get { return textBox3.Text; }
            set { textBox3.Text = value; }
        }

        public string mysql_password
        {
            get { return textBox4.Text; }
            set { textBox4.Text = value; }
        }

        private int IsInputValid()
        {
            Regex reg = new Regex(@"^((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");

            if( !reg.Match(textBox1.Text).Success )
            {
                return -1 ;
            }
            else if(textBox2.Text == "" )
            {
                return -2;
            }
            else if(textBox3.Text == "" )
            {
                return -3;
            }
            else if(textBox4.Text == "" )
            {
                return -4;
            }
            else
            {
                return 0 ;
            }
        }

        private void MyOKbutton_Click(object sender, EventArgs e)
        {
            switch( IsInputValid() )
            {
                case 0 :
                    m_refToConfig.SetDatabaseHost(mysql_host);
                    m_refToConfig.SetDatabase(mysql_database);
                    m_refToConfig.SetDatabaseUsername(mysql_username);
                    m_refToConfig.SetDatabasePassword(mysql_password)
                        ;
                    this.DialogResult = DialogResult.OK;
                    break;
                case -1:
                    MessageBox.Show("伺服器位址不正確!","錯誤",MessageBoxButtons.OK,MessageBoxIcon.Error) ; break ;
                case -2:
                    MessageBox.Show("請填入資料庫名稱!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error); break;
                case -3:
                    MessageBox.Show("請填入使用者帳號!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error); break;
                case -4:
                    MessageBox.Show("請填入密碼!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error); break;
                default:
                    MessageBox.Show("預料外的錯誤!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error); break;

            }
        }

 

    }
}