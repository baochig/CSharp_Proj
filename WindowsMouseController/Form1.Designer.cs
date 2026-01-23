namespace WindowsMouseController
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
            this.run = new System.Windows.Forms.Button();
            this.stop = new System.Windows.Forms.Button();
            this.NotifyIcon = new System.Windows.Forms.Button();
            this.click = new System.Windows.Forms.Button();
            this.MixRun_1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.ExtendHiGate = new System.Windows.Forms.Button();
            this.textControlWindowName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSendKey = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textKey = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textKeyInterval = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chBoxKeyPress = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // run
            // 
            this.run.Location = new System.Drawing.Point(34, 42);
            this.run.Name = "run";
            this.run.Size = new System.Drawing.Size(87, 23);
            this.run.TabIndex = 0;
            this.run.Text = "滑鼠移動";
            this.run.UseVisualStyleBackColor = true;
            this.run.Click += new System.EventHandler(this.run_Click);
            // 
            // stop
            // 
            this.stop.Location = new System.Drawing.Point(158, 42);
            this.stop.Name = "stop";
            this.stop.Size = new System.Drawing.Size(85, 23);
            this.stop.TabIndex = 1;
            this.stop.Text = "停止";
            this.stop.UseVisualStyleBackColor = true;
            this.stop.Click += new System.EventHandler(this.stop_Click);
            // 
            // NotifyIcon
            // 
            this.NotifyIcon.Location = new System.Drawing.Point(158, 102);
            this.NotifyIcon.Name = "NotifyIcon";
            this.NotifyIcon.Size = new System.Drawing.Size(85, 23);
            this.NotifyIcon.TabIndex = 2;
            this.NotifyIcon.Text = "背景執行";
            this.NotifyIcon.UseVisualStyleBackColor = true;
            this.NotifyIcon.Click += new System.EventHandler(this.NotifyIcon_Click);
            // 
            // click
            // 
            this.click.Location = new System.Drawing.Point(34, 72);
            this.click.Name = "click";
            this.click.Size = new System.Drawing.Size(87, 23);
            this.click.TabIndex = 3;
            this.click.Text = "座標自訂";
            this.click.UseVisualStyleBackColor = true;
            this.click.Click += new System.EventHandler(this.click_Click);
            // 
            // MixRun_1
            // 
            this.MixRun_1.Location = new System.Drawing.Point(34, 102);
            this.MixRun_1.Name = "MixRun_1";
            this.MixRun_1.Size = new System.Drawing.Size(87, 23);
            this.MixRun_1.TabIndex = 4;
            this.MixRun_1.Text = "左右連點";
            this.MixRun_1.UseVisualStyleBackColor = true;
            this.MixRun_1.Click += new System.EventHandler(this.MixRun_1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(34, 174);
            this.textBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(648, 285);
            this.textBox1.TabIndex = 5;
            // 
            // ExtendHiGate
            // 
            this.ExtendHiGate.Location = new System.Drawing.Point(158, 71);
            this.ExtendHiGate.Name = "ExtendHiGate";
            this.ExtendHiGate.Size = new System.Drawing.Size(85, 23);
            this.ExtendHiGate.TabIndex = 4;
            this.ExtendHiGate.Text = "延長時間";
            this.ExtendHiGate.UseVisualStyleBackColor = true;
            this.ExtendHiGate.Click += new System.EventHandler(this.ExtendHiGate_Click);
            // 
            // textControlWindowName
            // 
            this.textControlWindowName.Location = new System.Drawing.Point(393, 40);
            this.textControlWindowName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textControlWindowName.Name = "textControlWindowName";
            this.textControlWindowName.Size = new System.Drawing.Size(104, 25);
            this.textControlWindowName.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(295, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 15);
            this.label1.TabIndex = 7;
            this.label1.Text = "Process ID: ";
            // 
            // buttonSendKey
            // 
            this.buttonSendKey.Location = new System.Drawing.Point(514, 37);
            this.buttonSendKey.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonSendKey.Name = "buttonSendKey";
            this.buttonSendKey.Size = new System.Drawing.Size(85, 27);
            this.buttonSendKey.TabIndex = 8;
            this.buttonSendKey.Text = "重複按鍵";
            this.buttonSendKey.UseVisualStyleBackColor = true;
            this.buttonSendKey.Click += new System.EventHandler(this.buttonSendKey_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(295, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 15);
            this.label2.TabIndex = 7;
            this.label2.Text = "Key Sending: ";
            // 
            // textKey
            // 
            this.textKey.Location = new System.Drawing.Point(393, 76);
            this.textKey.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textKey.Name = "textKey";
            this.textKey.Size = new System.Drawing.Size(104, 25);
            this.textKey.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(271, 110);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 15);
            this.label3.TabIndex = 7;
            this.label3.Text = "計時器週期(Sec):";
            // 
            // textKeyInterval
            // 
            this.textKeyInterval.Location = new System.Drawing.Point(393, 105);
            this.textKeyInterval.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textKeyInterval.Name = "textKeyInterval";
            this.textKeyInterval.Size = new System.Drawing.Size(104, 25);
            this.textKeyInterval.TabIndex = 6;
            this.textKeyInterval.Text = "5";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(271, 141);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 15);
            this.label4.TabIndex = 7;
            this.label4.Text = "按鍵按著:";
            // 
            // chBoxKeyPress
            // 
            this.chBoxKeyPress.AutoSize = true;
            this.chBoxKeyPress.Location = new System.Drawing.Point(393, 141);
            this.chBoxKeyPress.Name = "chBoxKeyPress";
            this.chBoxKeyPress.Size = new System.Drawing.Size(58, 19);
            this.chBoxKeyPress.TabIndex = 9;
            this.chBoxKeyPress.Text = "Press";
            this.chBoxKeyPress.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(702, 468);
            this.Controls.Add(this.chBoxKeyPress);
            this.Controls.Add(this.buttonSendKey);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textKeyInterval);
            this.Controls.Add(this.textKey);
            this.Controls.Add(this.textControlWindowName);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.ExtendHiGate);
            this.Controls.Add(this.MixRun_1);
            this.Controls.Add(this.click);
            this.Controls.Add(this.NotifyIcon);
            this.Controls.Add(this.stop);
            this.Controls.Add(this.run);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button run;
        private System.Windows.Forms.Button stop;
        private System.Windows.Forms.Button NotifyIcon;
        private System.Windows.Forms.Button click;
        private System.Windows.Forms.Button MixRun_1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button ExtendHiGate;
        private System.Windows.Forms.TextBox textControlWindowName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonSendKey;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textKey;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textKeyInterval;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chBoxKeyPress;

    }
}

