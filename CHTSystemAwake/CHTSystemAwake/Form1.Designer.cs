namespace CHTSystemAwake
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
            this.SuspendLayout();
            // 
            // run
            // 
            this.run.Location = new System.Drawing.Point(27, 28);
            this.run.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.run.Name = "run";
            this.run.Size = new System.Drawing.Size(95, 28);
            this.run.TabIndex = 0;
            this.run.Text = "執行";
            this.run.UseVisualStyleBackColor = true;
            this.run.Click += new System.EventHandler(this.run_Click);
            // 
            // stop
            // 
            this.stop.Location = new System.Drawing.Point(188, 28);
            this.stop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.stop.Name = "stop";
            this.stop.Size = new System.Drawing.Size(95, 28);
            this.stop.TabIndex = 0;
            this.stop.Text = "停止";
            this.stop.UseVisualStyleBackColor = true;
            this.stop.Click += new System.EventHandler(this.stop_Click);
            // 
            // NotifyIcon
            // 
            this.NotifyIcon.Location = new System.Drawing.Point(188, 80);
            this.NotifyIcon.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.NotifyIcon.Name = "NotifyIcon";
            this.NotifyIcon.Size = new System.Drawing.Size(95, 28);
            this.NotifyIcon.TabIndex = 0;
            this.NotifyIcon.Text = "背景執行";
            this.NotifyIcon.UseVisualStyleBackColor = true;
            this.NotifyIcon.Click += new System.EventHandler(this.NotifyIcon_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(317, 306);
            this.Controls.Add(this.NotifyIcon);
            this.Controls.Add(this.stop);
            this.Controls.Add(this.run);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button run;
        private System.Windows.Forms.Button stop;
        private System.Windows.Forms.Button NotifyIcon;
    }
}

