using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace WindowsMouseController
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.NotifyIcon notifyIcon1; //建立icon的容器
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer(); // 設定一個時鐘來模擬滑鼠
        Action<string> action; //儲存目前當前執行的Thread
        uint iRunMode = 0;

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static int m_HookHandle = 0;    // Hook handle
        private HookProc m_KbdHookProc;            // 鍵盤掛鉤函式指標

        //滑鼠控制常數
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        const int MOUSEEVENTF_MOVE = 0x0001;
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        const int MOUSEEVENTF_WHEEL = 0x0800;
        const int MOUSEEVENTF_XDOWN = 0x0080;
        const int MOUSEEVENTF_XUP = 0x1000;
        const int MOUSEEVENTF_HWHEEL = 0x01000;
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;

        //鍵盤控制常數
        const int WH_KEYBOARD_LL = 13; //for global hook
        const int KEYEVENTF_KEYUP = 0x0002;

        /// <summary>
        /// 透過windows user.dll，定義模擬滑鼠的各種API
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        //[DllImport("user32.dll", SetLastError = true)]
        //static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        //[DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        //public static extern IntPtr FindWindow(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint fwFlag, IntPtr dwExtraInfo);


        // 設置掛鉤.
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        // 將之前設置的掛鉤移除。記得在應用程式結束前呼叫此函式.
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

         // 呼叫下一個掛鉤處理常式（若不這麼做，會令其他掛鉤處理常式失效）.
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

        private void LeftClick()
        {
            //int xpos = Control.MousePosition.X;
            //int ypos = Control.MousePosition.Y;
            int xpos = Cursor.Position.X;
            int ypos = Cursor.Position.Y;
            this.textBox1.AppendText("Click position X = " + xpos.ToString() + ",Y = " + ypos.ToString() + "\r\n");
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }
        private void TimerEventMouseMove(Object myObject, EventArgs myEventArgs) {
            int xpos = System.Windows.Forms.Cursor.Position.X;
            int ypos = System.Windows.Forms.Cursor.Position.Y;
            int xpos2 = System.Windows.Forms.Cursor.Position.X + Properties.Settings.Default.move_position_x;
            int ypos2 = System.Windows.Forms.Cursor.Position.Y + Properties.Settings.Default.move_position_y;
            SetCursorPos(xpos2, ypos2);
            SetCursorPos(xpos, ypos);
            return;
        }

        /// <summary>
        /// 計時器觸發後當前座標滑鼠點擊
        /// </summary>
        /// <param name="myObject"></param>
        /// <param name="myEventArgs"></param>
        private void TimerEventMouseClick(Object myObject, EventArgs myEventArgs)
        {
            int xpos = System.Windows.Forms.Cursor.Position.X;
            int ypos = System.Windows.Forms.Cursor.Position.Y;
            //鍵盤控制
            if (string.IsNullOrEmpty(textKey.Text) != true)
            {
                int key = Convert.ToInt32(textKey.Text);
                if (key != 0)
                {
                    keybd_event((byte)key, 0, 0, IntPtr.Zero);
                    keybd_event((byte)key, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                }
            }
            Thread.Sleep(200);
            //滑鼠控制
            Debug.WriteLine("Move xpos: {0}, ypos: {1}", xpos, ypos);
            Thread.Sleep(200);
            SetCursorPos(xpos, ypos);
            Thread.Sleep(200);
            this.LeftClick();
            return;
        }

        /// <summary>
        /// 延長Higate時間的主程式
        /// </summary>
        /// <param name="myObject"></param>
        /// <param name="myEventArgs"></param>
        private void TimerEventSendMessageToHiGate(Object myObject, EventArgs myEventArgs)
        {
            IntPtr hwnd = IntPtr.Zero;
            //hwnd = FindWindow(null,Properties.Settings.Default.higate_window);
            if (hwnd == IntPtr.Zero)
            {
                MessageBox.Show("Couldn't find the " + Properties.Settings.Default.higate_window);
            }
            else
            {
                try
                {
                    SendMessage(hwnd, WM_LBUTTONDOWN, (IntPtr)0, IntPtr.Zero);
                    SendMessage(hwnd, WM_LBUTTONUP, (IntPtr)0, IntPtr.Zero);
                    //SendMessage(hwnd, BM_SETSTATE, 0, IntPtr.Zero);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// 計時器觸發後移動到指定座標並連點
        /// </summary>
        /// <param name="myObject"></param>
        /// <param name="myEventArgs"></param>
        private void TimerEventMouseMoveAndClick1(Object myObject, EventArgs myEventArgs)
        {
            int xpos = System.Windows.Forms.Cursor.Position.X;
            int ypos = System.Windows.Forms.Cursor.Position.Y;
            int xpos2 = System.Windows.Forms.Cursor.Position.X + Properties.Settings.Default.move_position_x;
            int ypos2 = System.Windows.Forms.Cursor.Position.Y + Properties.Settings.Default.move_position_y;
            //儲存當前座標
            Debug.WriteLine("Save: {0}, ypos: {1}", xpos, ypos);
            this.textBox1.AppendText("Orignal xpos = " + xpos + ", ypos = " + ypos + "\n");
            SetCursorPos(xpos, ypos);
            this.LeftClick();
            //移動到X座標move_position(Default=300)的地方
            Debug.WriteLine("Move xpos: {0}, ypos: {1}", xpos2, ypos);
            this.textBox1.AppendText("Move xpos = " + xpos2 + ", ypos = " + ypos + "\n");
            SetCursorPos(xpos2, ypos2);
            Debug.WriteLine("Move xpos2: {0}, ypos: {1}", xpos2, ypos);
            this.LeftClick();
            //返回原本座標
            this.textBox1.AppendText("Return xpos = " + xpos + ", ypos = " + ypos + "\n");
            Debug.WriteLine("Return xpos: {0}, ypos: {1}" , xpos, ypos);
            SetCursorPos(xpos, ypos);
            return;
        }
        public Form1()
        {
            InitializeComponent();

            //指定使用的容器
            this.components = new System.ComponentModel.Container();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            //建立NotifyIcon
            this.notifyIcon1.Icon = new Icon(SystemIcons.Information, 40, 40);
            this.notifyIcon1.Text = "Mouse Controller";
            //點兩下Icon呼叫程式
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon_Click);
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

        /// <summary>
        /// 滑鼠移動, 設定一個計時器，時間到執行滑鼠移動
        /// iRunMode = 1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void run_Click(object sender, EventArgs e)
        {
            // timer.Interval = 60 sec = 60000 msec
            int interval = Properties.Settings.Default.interval;
            timer.Interval = (interval * 1000);
            timer.Tick += TimerEventMouseMove;
            timer.Start();
            iRunMode = 1;
            return;
        }

        /// <summary>
        /// 滑鼠連點
        /// iRunMode = 2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void click_Click(object sender, EventArgs e)
        {
            // timer.Interval = 60 sec * 10 = 10 minute
            int interval = Convert.ToInt32(textKeyInterval.Text);
            if(interval == 0)
                interval = 1;
            timer.Interval = (interval * 1000);
            timer.Tick += TimerEventMouseClick;
            timer.Start();
            iRunMode = 2;
        }

        /// <summary>
        /// 左右連點
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MixRun_1_Click(object sender, EventArgs e)
        {
            // timer.Interval = 60 sec * 10 = 10 minute
            int interval = Properties.Settings.Default.interval;
            timer.Interval = (interval * 1000);
            timer.Tick += TimerEventMouseMoveAndClick1;
            timer.Start();
            iRunMode = 3;
        }

        private void ExtendHiGate_Click(object sender, EventArgs e)
        {
            // timer.Interval = 60 sec * 10 = 10 minute
            int interval = Properties.Settings.Default.interval;
            timer.Interval = (interval * 1000);
            timer.Tick += TimerEventSendMessageToHiGate;
            timer.Start();
            iRunMode = 4;
        }

        private bool _start;
        private void buttonSendKey_Click(object sender, EventArgs e)
        {
            _start = true;
            while (_start)
            {
                int key = Convert.ToInt32(textKey.Text);
                int interval = Convert.ToInt32(textKeyInterval.Text);
                if(key == 0 || interval == 0)
                    MessageBox.Show("鍵盤參數錯誤");
                const int KEYEVENTF_KEYUP = 0x0002;
                keybd_event((byte)key, 0, 0, IntPtr.Zero);
                keybd_event((byte)key, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                Thread.Sleep(interval * 1000);
                Application.DoEvents(); 
            }
        }

        /// <summary>
        /// 觸發計時器中止且釋放timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stop_Click(object sender, EventArgs e)
        {
            //For Mouse control
            if (timer.Enabled)
                timer.Stop();

            //To terminate runnning thread
            switch (iRunMode)
            {
                case 1:
                    timer.Tick -= TimerEventMouseMove;
                    break;
                case 2:
                    timer.Tick -= TimerEventMouseClick;
                    break;
                case 3:
                    timer.Tick -= TimerEventMouseMoveAndClick1;
                    break;
                case 4:
                    timer.Tick -= TimerEventSendMessageToHiGate;
                    break;
            }
            //For Keyboard control
            if (_start)
                _start = false;
            return;
        }
    }
}
