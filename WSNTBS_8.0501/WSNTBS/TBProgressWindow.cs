using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WSNTBS
{
    public partial class TBProgressWindow : Form
    {
        public TBProgressWindow()
        {
            InitializeComponent();
        }

        public void SetProgressValue(int value)
        {
            if (value > 100)
                value = 100;
            if (value < 0)
                value = 0;
            progressBar1.Value = value;
            progressBar1.Update();
        }
    }
}