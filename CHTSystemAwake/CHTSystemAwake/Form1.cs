using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace CHTSystemAwake
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.NotifyIcon notifyIcon1; //建立icon的容器
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001

            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }
        public Form1()
        {
            InitializeComponent();
            //指定使用的容器
            this.components = new System.ComponentModel.Container();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            //建立NotifyIcon
            this.notifyIcon1.Icon = new Icon(SystemIcons.Information, 40, 40);
            this.notifyIcon1.Text = "System Awaker";
            //點兩下Icon呼叫程式
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon_Click);
        }

        private void run_Click(object sender, EventArgs e)
        {
            Form1.SetThreadExecutionState(Form1.EXECUTION_STATE.ES_CONTINUOUS |
                                                          Form1.EXECUTION_STATE.ES_DISPLAY_REQUIRED |
                                                          Form1.EXECUTION_STATE.ES_SYSTEM_REQUIRED |
                                                          Form1.EXECUTION_STATE.ES_AWAYMODE_REQUIRED);
        }

        private void stop_Click(object sender, EventArgs e)
        {
            Form1.SetThreadExecutionState(Form1.EXECUTION_STATE.ES_CONTINUOUS);
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            if (this.notifyIcon1.Visible == false)
            {
                this.Hide();
                this.notifyIcon1.Visible = true;
            }
            else
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.notifyIcon1.Visible = false;
            }
        }
    }
}
