namespace WSNTBDelugeClient
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該公開 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改這個方法的內容。
        ///
        /// </summary>
        private void InitializeComponent()
        {
            this.richTextBox_SystemLog = new System.Windows.Forms.RichTextBox();
            this.button_LoginAndLogOut = new System.Windows.Forms.Button();
            this.button_About = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_NodeID_Ping = new System.Windows.Forms.ComboBox();
            this.button_Ping = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.comboBox_ImageNum_Inject = new System.Windows.Forms.ComboBox();
            this.button5 = new System.Windows.Forms.Button();
            this.button_Inject_browse = new System.Windows.Forms.Button();
            this.textBox_InjectFile = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.button_Reboot = new System.Windows.Forms.Button();
            this.comboBox_ImageNum_Reboot = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.button_Reset = new System.Windows.Forms.Button();
            this.comboBox_ImageNum_Reset = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.button_Erase = new System.Windows.Forms.Button();
            this.comboBox_ImageNum_Erase = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBox_SystemLog
            // 
            this.richTextBox_SystemLog.Location = new System.Drawing.Point(14, 28);
            this.richTextBox_SystemLog.Name = "richTextBox_SystemLog";
            this.richTextBox_SystemLog.ReadOnly = true;
            this.richTextBox_SystemLog.Size = new System.Drawing.Size(500, 233);
            this.richTextBox_SystemLog.TabIndex = 0;
            this.richTextBox_SystemLog.Text = "";
            // 
            // button_LoginAndLogOut
            // 
            this.button_LoginAndLogOut.Location = new System.Drawing.Point(520, 26);
            this.button_LoginAndLogOut.Name = "button_LoginAndLogOut";
            this.button_LoginAndLogOut.Size = new System.Drawing.Size(75, 23);
            this.button_LoginAndLogOut.TabIndex = 1;
            this.button_LoginAndLogOut.Text = "登入...";
            this.button_LoginAndLogOut.UseVisualStyleBackColor = true;
            this.button_LoginAndLogOut.Click += new System.EventHandler(this.button_LogInLogOut_Click);
            // 
            // button_About
            // 
            this.button_About.Location = new System.Drawing.Point(520, 55);
            this.button_About.Name = "button_About";
            this.button_About.Size = new System.Drawing.Size(75, 23);
            this.button_About.TabIndex = 4;
            this.button_About.Text = "關於...";
            this.button_About.UseVisualStyleBackColor = true;
            this.button_About.Click += new System.EventHandler(this.button_About_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "系統訊息：";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Controls.Add(this.tabPage6);
            this.tabControl1.Location = new System.Drawing.Point(14, 267);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(581, 149);
            this.tabControl1.TabIndex = 7;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.comboBox_NodeID_Ping);
            this.tabPage1.Controls.Add(this.button_Ping);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(573, 123);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Ping";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "Node ID：";
            // 
            // comboBox_NodeID_Ping
            // 
            this.comboBox_NodeID_Ping.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_NodeID_Ping.FormattingEnabled = true;
            this.comboBox_NodeID_Ping.Location = new System.Drawing.Point(69, 17);
            this.comboBox_NodeID_Ping.Name = "comboBox_NodeID_Ping";
            this.comboBox_NodeID_Ping.Size = new System.Drawing.Size(121, 20);
            this.comboBox_NodeID_Ping.TabIndex = 1;
            // 
            // button_Ping
            // 
            this.button_Ping.Location = new System.Drawing.Point(219, 17);
            this.button_Ping.Name = "button_Ping";
            this.button_Ping.Size = new System.Drawing.Size(75, 23);
            this.button_Ping.TabIndex = 0;
            this.button_Ping.Text = "Ping";
            this.button_Ping.UseVisualStyleBackColor = true;
            this.button_Ping.Click += new System.EventHandler(this.button_Ping_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.comboBox_ImageNum_Inject);
            this.tabPage2.Controls.Add(this.button5);
            this.tabPage2.Controls.Add(this.button_Inject_browse);
            this.tabPage2.Controls.Add(this.textBox_InjectFile);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(573, 123);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Inject";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // comboBox_ImageNum_Inject
            // 
            this.comboBox_ImageNum_Inject.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_ImageNum_Inject.FormattingEnabled = true;
            this.comboBox_ImageNum_Inject.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6"});
            this.comboBox_ImageNum_Inject.Location = new System.Drawing.Point(97, 15);
            this.comboBox_ImageNum_Inject.Name = "comboBox_ImageNum_Inject";
            this.comboBox_ImageNum_Inject.Size = new System.Drawing.Size(92, 20);
            this.comboBox_ImageNum_Inject.TabIndex = 8;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(8, 84);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 7;
            this.button5.Text = "Inject";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button_Inject_Click);
            // 
            // button_Inject_browse
            // 
            this.button_Inject_browse.Location = new System.Drawing.Point(455, 49);
            this.button_Inject_browse.Name = "button_Inject_browse";
            this.button_Inject_browse.Size = new System.Drawing.Size(75, 23);
            this.button_Inject_browse.TabIndex = 6;
            this.button_Inject_browse.Text = "瀏覽...";
            this.button_Inject_browse.UseVisualStyleBackColor = true;
            this.button_Inject_browse.Click += new System.EventHandler(this.button_Inject_browse_Click);
            // 
            // textBox_InjectFile
            // 
            this.textBox_InjectFile.Enabled = false;
            this.textBox_InjectFile.Location = new System.Drawing.Point(128, 49);
            this.textBox_InjectFile.Name = "textBox_InjectFile";
            this.textBox_InjectFile.Size = new System.Drawing.Size(311, 22);
            this.textBox_InjectFile.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 55);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(121, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "Inject File (tos_image)：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 20);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "Image Number：";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.button_Reboot);
            this.tabPage3.Controls.Add(this.comboBox_ImageNum_Reboot);
            this.tabPage3.Controls.Add(this.label6);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(573, 123);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Reboot";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // button_Reboot
            // 
            this.button_Reboot.Location = new System.Drawing.Point(217, 11);
            this.button_Reboot.Name = "button_Reboot";
            this.button_Reboot.Size = new System.Drawing.Size(75, 23);
            this.button_Reboot.TabIndex = 13;
            this.button_Reboot.Text = "Reboot";
            this.button_Reboot.UseVisualStyleBackColor = true;
            this.button_Reboot.Click += new System.EventHandler(this.button_Reboot_Click);
            // 
            // comboBox_ImageNum_Reboot
            // 
            this.comboBox_ImageNum_Reboot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_ImageNum_Reboot.FormattingEnabled = true;
            this.comboBox_ImageNum_Reboot.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6"});
            this.comboBox_ImageNum_Reboot.Location = new System.Drawing.Point(97, 15);
            this.comboBox_ImageNum_Reboot.Name = "comboBox_ImageNum_Reboot";
            this.comboBox_ImageNum_Reboot.Size = new System.Drawing.Size(92, 20);
            this.comboBox_ImageNum_Reboot.TabIndex = 12;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(87, 12);
            this.label6.TabIndex = 11;
            this.label6.Text = "Image Number：";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.button_Erase);
            this.tabPage4.Controls.Add(this.comboBox_ImageNum_Erase);
            this.tabPage4.Controls.Add(this.label8);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(573, 123);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Erase";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.button_Reset);
            this.tabPage5.Controls.Add(this.comboBox_ImageNum_Reset);
            this.tabPage5.Controls.Add(this.label7);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(573, 123);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Reset";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.label3);
            this.tabPage6.Location = new System.Drawing.Point(4, 22);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Size = new System.Drawing.Size(573, 123);
            this.tabPage6.TabIndex = 5;
            this.tabPage6.Text = "Dump";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "xml 檔案(*.xml)|*.xml|所有檔案(*.*)|*.*";
            // 
            // button_Reset
            // 
            this.button_Reset.Location = new System.Drawing.Point(217, 11);
            this.button_Reset.Name = "button_Reset";
            this.button_Reset.Size = new System.Drawing.Size(75, 23);
            this.button_Reset.TabIndex = 16;
            this.button_Reset.Text = "Reset";
            this.button_Reset.UseVisualStyleBackColor = true;
            this.button_Reset.Click += new System.EventHandler(this.button_Reset_Click);
            // 
            // comboBox_ImageNum_Reset
            // 
            this.comboBox_ImageNum_Reset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_ImageNum_Reset.FormattingEnabled = true;
            this.comboBox_ImageNum_Reset.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6"});
            this.comboBox_ImageNum_Reset.Location = new System.Drawing.Point(97, 15);
            this.comboBox_ImageNum_Reset.Name = "comboBox_ImageNum_Reset";
            this.comboBox_ImageNum_Reset.Size = new System.Drawing.Size(92, 20);
            this.comboBox_ImageNum_Reset.TabIndex = 15;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 20);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(87, 12);
            this.label7.TabIndex = 14;
            this.label7.Text = "Image Number：";
            // 
            // button_Erase
            // 
            this.button_Erase.Location = new System.Drawing.Point(217, 11);
            this.button_Erase.Name = "button_Erase";
            this.button_Erase.Size = new System.Drawing.Size(75, 23);
            this.button_Erase.TabIndex = 16;
            this.button_Erase.Text = "Erase";
            this.button_Erase.UseVisualStyleBackColor = true;
            this.button_Erase.Click += new System.EventHandler(this.button_Erase_Click);
            // 
            // comboBox_ImageNum_Erase
            // 
            this.comboBox_ImageNum_Erase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_ImageNum_Erase.FormattingEnabled = true;
            this.comboBox_ImageNum_Erase.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6"});
            this.comboBox_ImageNum_Erase.Location = new System.Drawing.Point(97, 15);
            this.comboBox_ImageNum_Erase.Name = "comboBox_ImageNum_Erase";
            this.comboBox_ImageNum_Erase.Size = new System.Drawing.Size(92, 20);
            this.comboBox_ImageNum_Erase.TabIndex = 15;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 20);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(87, 12);
            this.label8.TabIndex = 14;
            this.label8.Text = "Image Number：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "尚未實作";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(606, 428);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_About);
            this.Controls.Add(this.button_LoginAndLogOut);
            this.Controls.Add(this.richTextBox_SystemLog);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "WSNTB Deluge Client 1.0";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            this.tabPage6.ResumeLayout(false);
            this.tabPage6.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox_SystemLog;
        private System.Windows.Forms.Button button_LoginAndLogOut;
        private System.Windows.Forms.Button button_About;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button button_Ping;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_NodeID_Ping;
        private System.Windows.Forms.ComboBox comboBox_ImageNum_Inject;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button_Inject_browse;
        private System.Windows.Forms.TextBox textBox_InjectFile;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ComboBox comboBox_ImageNum_Reboot;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button_Reboot;
        private System.Windows.Forms.Button button_Reset;
        private System.Windows.Forms.ComboBox comboBox_ImageNum_Reset;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button_Erase;
        private System.Windows.Forms.ComboBox comboBox_ImageNum_Erase;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label3;
    }
}

