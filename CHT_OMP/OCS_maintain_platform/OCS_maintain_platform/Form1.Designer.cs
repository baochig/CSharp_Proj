
namespace OCS_maintain_platform
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.parseMinossLog = new System.Windows.Forms.Button();
            this.maintainOAM = new System.Windows.Forms.Button();
            this.execMinossBatch = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // parseMinossLog
            // 
            this.parseMinossLog.Location = new System.Drawing.Point(185, 24);
            this.parseMinossLog.Name = "parseMinossLog";
            this.parseMinossLog.Size = new System.Drawing.Size(128, 29);
            this.parseMinossLog.TabIndex = 0;
            this.parseMinossLog.Text = "處理minoss log";
            this.parseMinossLog.UseVisualStyleBackColor = true;
            this.parseMinossLog.Click += new System.EventHandler(this.parseMinossLog_Click);
            // 
            // maintainOAM
            // 
            this.maintainOAM.Location = new System.Drawing.Point(329, 24);
            this.maintainOAM.Name = "maintainOAM";
            this.maintainOAM.Size = new System.Drawing.Size(94, 29);
            this.maintainOAM.TabIndex = 1;
            this.maintainOAM.Text = "OCS資料庫管理";
            this.maintainOAM.UseVisualStyleBackColor = true;
            // 
            // execMinossBatch
            // 
            this.execMinossBatch.Location = new System.Drawing.Point(12, 24);
            this.execMinossBatch.Name = "execMinossBatch";
            this.execMinossBatch.Size = new System.Drawing.Size(168, 29);
            this.execMinossBatch.TabIndex = 0;
            this.execMinossBatch.Text = "執行minoss script";
            this.execMinossBatch.UseVisualStyleBackColor = true;
            this.execMinossBatch.Click += new System.EventHandler(this.parseMinossLog_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.maintainOAM);
            this.Controls.Add(this.execMinossBatch);
            this.Controls.Add(this.parseMinossLog);
            this.Name = "Form1";
            this.Text = "選單";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button parseMinossLog;
        private System.Windows.Forms.Button maintainOAM;
        private System.Windows.Forms.Button execMinossBatch;
    }
}

