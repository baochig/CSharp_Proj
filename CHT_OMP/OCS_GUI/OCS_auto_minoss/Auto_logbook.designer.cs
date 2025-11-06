namespace OCS_auto_minoss
{
    partial class Auto_logbook_form
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
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
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.submit = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.name = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.pwd = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.getOTP = new System.Windows.Forms.Button();
            this.otp = new System.Windows.Forms.TextBox();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.Submit_MINOSS = new System.Windows.Forms.Button();
            this.submit_Event = new System.Windows.Forms.Button();
            this.funcSelectBox = new System.Windows.Forms.ComboBox();
            this.panel_autoMINOSSEvent = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.eventJob = new System.Windows.Forms.TextBox();
            this.eventSerivce = new System.Windows.Forms.TextBox();
            this.dtEventEndTime = new System.Windows.Forms.DateTimePicker();
            this.dtEventStartTime = new System.Windows.Forms.DateTimePicker();
            this.comboBox_eventMenu = new System.Windows.Forms.ComboBox();
            this.eventContent = new System.Windows.Forms.TextBox();
            this.eventTitle = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panel_autoMINOSS = new System.Windows.Forms.Panel();
            this.Submit_MINOSS_Info = new System.Windows.Forms.TextBox();
            this.panel_autoMINOSSEvent.SuspendLayout();
            this.panel_autoMINOSS.SuspendLayout();
            this.SuspendLayout();
            // 
            // submit
            // 
            this.submit.Location = new System.Drawing.Point(104, 297);
            this.submit.Margin = new System.Windows.Forms.Padding(4);
            this.submit.Name = "submit";
            this.submit.Size = new System.Drawing.Size(78, 34);
            this.submit.TabIndex = 4;
            this.submit.Text = "登入";
            this.submit.UseVisualStyleBackColor = true;
            this.submit.Click += new System.EventHandler(this.submit_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 18);
            this.label1.TabIndex = 9;
            this.label1.Text = "LDAP 帳號：";
            // 
            // name
            // 
            this.name.Location = new System.Drawing.Point(136, 9);
            this.name.Margin = new System.Windows.Forms.Padding(4);
            this.name.Name = "name";
            this.name.Size = new System.Drawing.Size(256, 29);
            this.name.TabIndex = 1;
            this.name.Text = "baochig";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 45);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 18);
            this.label2.TabIndex = 9;
            this.label2.Text = "LDAP 密碼：";
            // 
            // pwd
            // 
            this.pwd.Location = new System.Drawing.Point(136, 42);
            this.pwd.Margin = new System.Windows.Forms.Padding(4);
            this.pwd.Name = "pwd";
            this.pwd.Size = new System.Drawing.Size(256, 29);
            this.pwd.TabIndex = 2;
            this.pwd.Text = "1083edc$RFV";
            this.pwd.UseSystemPasswordChar = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 75);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 18);
            this.label3.TabIndex = 9;
            this.label3.Text = "OTP 密碼：";
            // 
            // getOTP
            // 
            this.getOTP.Location = new System.Drawing.Point(13, 297);
            this.getOTP.Margin = new System.Windows.Forms.Padding(4);
            this.getOTP.Name = "getOTP";
            this.getOTP.Size = new System.Drawing.Size(83, 50);
            this.getOTP.TabIndex = 3;
            this.getOTP.Text = "取得OTP";
            this.getOTP.UseVisualStyleBackColor = true;
            this.getOTP.Click += new System.EventHandler(this.getOTP_Click);
            // 
            // otp
            // 
            this.otp.Location = new System.Drawing.Point(136, 72);
            this.otp.Margin = new System.Windows.Forms.Padding(4);
            this.otp.Name = "otp";
            this.otp.Size = new System.Drawing.Size(256, 29);
            this.otp.TabIndex = 3;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Location = new System.Drawing.Point(-4, 357);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(30, 30);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.ScriptErrorsSuppressed = true;
            this.webBrowser1.Size = new System.Drawing.Size(1275, 597);
            this.webBrowser1.TabIndex = 9;
            // 
            // Submit_MINOSS
            // 
            this.Submit_MINOSS.Location = new System.Drawing.Point(29, 284);
            this.Submit_MINOSS.Margin = new System.Windows.Forms.Padding(4);
            this.Submit_MINOSS.Name = "Submit_MINOSS";
            this.Submit_MINOSS.Size = new System.Drawing.Size(123, 34);
            this.Submit_MINOSS.TabIndex = 4;
            this.Submit_MINOSS.Text = "送出MINOSS";
            this.Submit_MINOSS.UseVisualStyleBackColor = true;
            this.Submit_MINOSS.Click += new System.EventHandler(this.autoMINOSS);
            // 
            // submit_Event
            // 
            this.submit_Event.Location = new System.Drawing.Point(29, 284);
            this.submit_Event.Margin = new System.Windows.Forms.Padding(4);
            this.submit_Event.Name = "submit_Event";
            this.submit_Event.Size = new System.Drawing.Size(130, 34);
            this.submit_Event.TabIndex = 4;
            this.submit_Event.Text = "上傳事件紀錄";
            this.submit_Event.UseVisualStyleBackColor = true;
            this.submit_Event.Click += new System.EventHandler(this.autoMINOSSEvent);
            // 
            // funcSelectBox
            // 
            this.funcSelectBox.FormattingEnabled = true;
            this.funcSelectBox.ItemHeight = 18;
            this.funcSelectBox.Items.AddRange(new object[] {
            "",
            "上傳工作日誌",
            "填寫事件紀錄"});
            this.funcSelectBox.Location = new System.Drawing.Point(189, 302);
            this.funcSelectBox.Name = "funcSelectBox";
            this.funcSelectBox.Size = new System.Drawing.Size(211, 26);
            this.funcSelectBox.TabIndex = 5;
            this.funcSelectBox.SelectedIndexChanged += new System.EventHandler(this.funcSelectBox_SelectedIndexChanged);
            // 
            // panel_autoMINOSSEvent
            // 
            this.panel_autoMINOSSEvent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_autoMINOSSEvent.Controls.Add(this.label9);
            this.panel_autoMINOSSEvent.Controls.Add(this.eventJob);
            this.panel_autoMINOSSEvent.Controls.Add(this.eventSerivce);
            this.panel_autoMINOSSEvent.Controls.Add(this.dtEventEndTime);
            this.panel_autoMINOSSEvent.Controls.Add(this.dtEventStartTime);
            this.panel_autoMINOSSEvent.Controls.Add(this.comboBox_eventMenu);
            this.panel_autoMINOSSEvent.Controls.Add(this.eventContent);
            this.panel_autoMINOSSEvent.Controls.Add(this.eventTitle);
            this.panel_autoMINOSSEvent.Controls.Add(this.label5);
            this.panel_autoMINOSSEvent.Controls.Add(this.label8);
            this.panel_autoMINOSSEvent.Controls.Add(this.label7);
            this.panel_autoMINOSSEvent.Controls.Add(this.label10);
            this.panel_autoMINOSSEvent.Controls.Add(this.label6);
            this.panel_autoMINOSSEvent.Controls.Add(this.label4);
            this.panel_autoMINOSSEvent.Controls.Add(this.submit_Event);
            this.panel_autoMINOSSEvent.Location = new System.Drawing.Point(429, 12);
            this.panel_autoMINOSSEvent.Name = "panel_autoMINOSSEvent";
            this.panel_autoMINOSSEvent.Size = new System.Drawing.Size(819, 338);
            this.panel_autoMINOSSEvent.TabIndex = 11;
            this.panel_autoMINOSSEvent.Visible = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(26, 50);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(62, 18);
            this.label9.TabIndex = 5;
            this.label9.Text = "IN服務";
            // 
            // eventJob
            // 
            this.eventJob.Enabled = false;
            this.eventJob.Location = new System.Drawing.Point(368, 47);
            this.eventJob.Name = "eventJob";
            this.eventJob.Size = new System.Drawing.Size(153, 29);
            this.eventJob.TabIndex = 10;
            // 
            // eventSerivce
            // 
            this.eventSerivce.Enabled = false;
            this.eventSerivce.Location = new System.Drawing.Point(132, 47);
            this.eventSerivce.Name = "eventSerivce";
            this.eventSerivce.Size = new System.Drawing.Size(134, 29);
            this.eventSerivce.TabIndex = 10;
            // 
            // dtEventEndTime
            // 
            this.dtEventEndTime.CustomFormat = "yyyy/MM/dd HH:mm";
            this.dtEventEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtEventEndTime.Location = new System.Drawing.Point(448, 86);
            this.dtEventEndTime.Name = "dtEventEndTime";
            this.dtEventEndTime.Size = new System.Drawing.Size(200, 29);
            this.dtEventEndTime.TabIndex = 9;
            // 
            // dtEventStartTime
            // 
            this.dtEventStartTime.CustomFormat = "yyyy/MM/dd HH:mm";
            this.dtEventStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtEventStartTime.Location = new System.Drawing.Point(132, 86);
            this.dtEventStartTime.Name = "dtEventStartTime";
            this.dtEventStartTime.Size = new System.Drawing.Size(200, 29);
            this.dtEventStartTime.TabIndex = 9;
            // 
            // comboBox_eventMenu
            // 
            this.comboBox_eventMenu.FormattingEnabled = true;
            this.comboBox_eventMenu.Items.AddRange(new object[] {
            "使用root帳號修改帳號帳密",
            "使用root帳號查測系統",
            "承商專屬終端機使用紀錄",
            "排外終端機使用紀錄",
            "維運終端使用高權限",
            "POSS終端機使用高權限",
            "客製範本1(OCS)",
            "客製範本2(其他)"});
            this.comboBox_eventMenu.Location = new System.Drawing.Point(132, 10);
            this.comboBox_eventMenu.Name = "comboBox_eventMenu";
            this.comboBox_eventMenu.Size = new System.Drawing.Size(301, 26);
            this.comboBox_eventMenu.TabIndex = 8;
            this.comboBox_eventMenu.SelectedIndexChanged += new System.EventHandler(this.comboBox_eventMenu_SelectedIndexChanged);
            // 
            // eventContent
            // 
            this.eventContent.Location = new System.Drawing.Point(132, 172);
            this.eventContent.Multiline = true;
            this.eventContent.Name = "eventContent";
            this.eventContent.Size = new System.Drawing.Size(592, 105);
            this.eventContent.TabIndex = 7;
            // 
            // eventTitle
            // 
            this.eventTitle.Location = new System.Drawing.Point(132, 124);
            this.eventTitle.Name = "eventTitle";
            this.eventTitle.Size = new System.Drawing.Size(592, 29);
            this.eventTitle.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(26, 172);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 18);
            this.label5.TabIndex = 5;
            this.label5.Text = "工作說明";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(353, 97);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(80, 18);
            this.label8.TabIndex = 5;
            this.label8.Text = "結束時間";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(26, 97);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(80, 18);
            this.label7.TabIndex = 5;
            this.label7.Text = "開始時間";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(282, 50);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(80, 18);
            this.label10.TabIndex = 5;
            this.label10.Text = "工作類別";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(26, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(80, 18);
            this.label6.TabIndex = 5;
            this.label6.Text = "事件範本";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(26, 135);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(85, 18);
            this.label4.TabIndex = 5;
            this.label4.Text = "工作摘要 ";
            // 
            // panel_autoMINOSS
            // 
            this.panel_autoMINOSS.Controls.Add(this.Submit_MINOSS_Info);
            this.panel_autoMINOSS.Controls.Add(this.Submit_MINOSS);
            this.panel_autoMINOSS.Location = new System.Drawing.Point(429, 12);
            this.panel_autoMINOSS.Name = "panel_autoMINOSS";
            this.panel_autoMINOSS.Size = new System.Drawing.Size(819, 338);
            this.panel_autoMINOSS.TabIndex = 11;
            // 
            // Submit_MINOSS_Info
            // 
            this.Submit_MINOSS_Info.Location = new System.Drawing.Point(30, 22);
            this.Submit_MINOSS_Info.Name = "Submit_MINOSS_Info";
            this.Submit_MINOSS_Info.Size = new System.Drawing.Size(387, 29);
            this.Submit_MINOSS_Info.TabIndex = 5;
            // 
            // Auto_logbook_form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1272, 983);
            this.Controls.Add(this.panel_autoMINOSS);
            this.Controls.Add(this.funcSelectBox);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.otp);
            this.Controls.Add(this.pwd);
            this.Controls.Add(this.name);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.getOTP);
            this.Controls.Add(this.submit);
            this.Controls.Add(this.panel_autoMINOSSEvent);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Auto_logbook_form";
            this.Text = "Auto_logbook_form";
            this.panel_autoMINOSSEvent.ResumeLayout(false);
            this.panel_autoMINOSSEvent.PerformLayout();
            this.panel_autoMINOSS.ResumeLayout(false);
            this.panel_autoMINOSS.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button submit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox name;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox pwd;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button getOTP;
        private System.Windows.Forms.TextBox otp;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button Submit_MINOSS;
        private System.Windows.Forms.Button submit_Event;
        private System.Windows.Forms.ComboBox funcSelectBox;
        private System.Windows.Forms.Panel panel_autoMINOSSEvent;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox eventContent;
        private System.Windows.Forms.TextBox eventTitle;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBox_eventMenu;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.DateTimePicker dtEventStartTime;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DateTimePicker dtEventEndTime;
        private System.Windows.Forms.TextBox eventSerivce;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox eventJob;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Panel panel_autoMINOSS;
        private System.Windows.Forms.TextBox Submit_MINOSS_Info;
    }
}

