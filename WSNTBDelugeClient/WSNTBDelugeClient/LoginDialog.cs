using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WSNTBDelugeClient
{
    public partial class LoginDialog : Form
    {
        public LoginDialog()
        {
            InitializeComponent();
        }

        public string UserName
        {
            get { return textBox1.Text ;}
            set { textBox1.Text = value; }
        }

        public string Password
        {
            get { return textBox2.Text; }
            set { textBox2.Text = value; }
        }
    }
}