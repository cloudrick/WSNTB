using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ACCClient
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        public string GetIPAddress()
        {
            return textBox1.Text ;
        }

        public string GetPort()
        {
            return textBox2.Text;
        }
    }
}