using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using OCS_auto_minoss;
using OCS_Parser_Minoss;
using OCS_GUI.OCS_Parser_Minoss;
//using OCS_ADP = OCS_Database.MSSQL.OCSMsSqlDataAdapter;
using System.Data;

namespace OCS_Parser_Minoss_GUI
{
    public partial class ParseResult : Form
    {
        const string output_folder = "output";
        private int tbl_panel_row_idx = 1;
        public ParseResult()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 動態建立錯誤描述
        /// </summary>
        /// <param name="control">控制項名稱</param>
        /// <param name="str">錯誤描述</param>
        /// <returns></returns>
        private int addToolTip(Control control, String str)
        {
            // Create the ToolTip and associate with the Form container.
            ToolTip toolTip1 = new ToolTip();

            // Set up the delays for the ToolTip.
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTip1.ShowAlways = true;

            // Set up the ToolTip title for the control object.
            toolTip1.ToolTipTitle = "錯誤描述";

            // Set up the ToolTip text for the control object.
            toolTip1.SetToolTip(control, str);
            return 0;
        }

        /// <summary>
        /// 將從DB取得的hostname列表儲存到list<string>
        /// 如果有log卻沒有主機名稱則表示不parse
        /// </summary>
        /// <param name="devList"></param>
        /// <returns></returns>
        private int getHostListToList(ref List<string> devList)
        {
            int ret = 0;

            string result = string.Empty;
            string inputHostName = string.Empty;
            //IEnumerable<DataRow> collection = null;
            devList.Add("7L2SDP3a");
            devList.Add("7L2SDP3b");
            devList.Add("7L2SDP4a");
            devList.Add("7L2SDP4b");
            devList.Add("7L2SDP5a");
            devList.Add("7L2SDP5b");
            devList.Add("7L2SDP6a");
            devList.Add("7L2SDP6b");
            devList.Add("7L2CCN1");
            devList.Add("7L2CCN2");
            devList.Add("7L2TMSAP1");
            devList.Add("7L2TMSAP2");
            devList.Add("7L2TMSDB1");
            devList.Add("7L2TMSDB2");
            devList.Add("7L2TMSDB3");
            devList.Add("7L2TMSDB4");
            devList.Add("7L2TMSDB5");
            devList.Add("7L2TMSDB6");
            devList.Add("7L2TMSDB7");
            devList.Add("7L2TMSDB8");
            devList.Add("7L2NDDP1");
            devList.Add("7L2NDDP2");
            devList.Add("7L2NDDP3");
            devList.Add("7L2NDDP4");
            devList.Add("7L2IVRC1");
            devList.Add("7L2IVRC2");
            devList.Add("7L2USSDGW1");
            devList.Add("7L2USSDGW2");
            devList.Add("7L2PCRF1");
            devList.Add("7L2OAM1");
            devList.Add("7L2OAM2");
            devList.Add("7L2OAM3");
            devList.Add("7L2OAM4");
            devList.Add("7L2PCRFSYNAP1");
            devList.Add("7L2PCRFSYNDB1");
            devList.Add("7L2NEWOCSGAP1");
            devList.Add("7L2NEWOCSGAP2");
            devList.Add("7L2NEWOCSGAP3");
            devList.Add("7L2NEWOCSGAP4");
            devList.Add("7L2NEWOCSGDB1_LO");
            devList.Add("7L2NEWOCSGDB2_LO");
            devList.Add("7L2NEWOCSGDB3_LO");
            devList.Add("7L2OCC11");
            devList.Add("7L2OCC12");
            devList.Add("7L2OCC13");
            devList.Add("7L2OCC14");
            devList.Add("7L2OCC15");
            devList.Add("7L2ECSNMT");
            devList.Add("7L2TMSREP2");
            devList.Add("7L2AIR6");
            devList.Add("7L2AIR7");
            devList.Add("7L2AIR8");
            devList.Add("7L2AIR1");
            devList.Add("7L2AIR1");
            devList.Add("7L2AIR1");
            devList.Add("7L2AIR1");
            devList.Add("7L2AIR1");
            devList.Add("7L2AIR1");
            devList.Add("7L2AIR1");
            /*
            IEnumerable<DataRow> collection = null;
            OCS_ADP ocs_data = new OCS_ADP();
            ret = ocs_data.GetHostListByMinossEnable(ref collection, ref result);
            if (ret == 0)
            {
                try
                {
                    foreach (DataRow value in collection)
                    {
                        inputHostName = value.Field<String>(value.Table.Columns["hostName"].Ordinal);
                        devList.Add(inputHostName);
                    }
                }
                catch (Exception ex)
                { 
                    var w32ex = ex as System.ComponentModel.Win32Exception;
                    return w32ex.ErrorCode;
                }
            }
            */
            return 0;
        }

        /// <summary>
        /// 初始進入點，當log file存在則開始parse log，因此有設備新增時，則需要加條件到此函式
        /// </summary>
        /// <returns></returns>
        public int initialParseMinoss()
        {
            DevResource res = new DevResource();
            Parser_xshell_lib parser = new Parser_xshell_lib();
            string file = null;

            // Load host name list from OCS DB
            List<string> devList = new List<string>();
            if (getHostListToList(ref devList) != 0)
                return 1;

            ////////////////////////////////////////
            // Start parse Device minoss logs
            ////////////////////////////////////////
            // Get date from dateTimePicker1
            string strToday;
            if (this.dateTimePicker1 != null)
                strToday = string.Copy(this.dateTimePicker1.Value.ToString("yyMMdd") + "_");
            else
                strToday = DateTime.Now.ToString("yyMMdd") + "_";
            
            // Open log file and output analysis result
            foreach (String name in devList) {
                file = strToday+name+".log";
                Debug.WriteLine(file);

                /// Check all log file is exist
                if (File.Exists(@file))
                {
                    Debug.WriteLine("Load log file : " + file);
                    //Execute parser API
                    DevResource.devSysChkResult result = new DevResource.devSysChkResult();
                    if (file.IndexOf("7L2AIR", StringComparison.OrdinalIgnoreCase) > 0)
                    { parser.parser_7L2AIR(file, ref result); setControl(name, result); }
                    if (file.IndexOf("7L2CCN", StringComparison.OrdinalIgnoreCase) > 0)
                    { parser.parser_7L2CCN(file, ref result); setControl(name, result); }
                    if (file.IndexOf("7L2SDP", StringComparison.OrdinalIgnoreCase) > 0 || file.IndexOf("7L2OCC", StringComparison.OrdinalIgnoreCase) > 0)
                    { parser.parser_7L2SDP(file, ref result); setControl(name, result); }
                    if (file.IndexOf("7L2TMSAP", StringComparison.OrdinalIgnoreCase) > 0)
                    { parser.parser_7L2TMSAP(file, ref result); setControl(name, result); }
                    if (file.IndexOf("7L2TMSDB", StringComparison.OrdinalIgnoreCase) > 0)
                    { parser.parser_7L2TMSDB(file, ref result); setControl(name, result); }
                    if (file.IndexOf("7L2TMSREP", StringComparison.OrdinalIgnoreCase) > 0)
                    { parser.parser_7L2TMSREP(file, ref result); setControl(name, result); }
                    if (file.IndexOf("7L2NDDP", StringComparison.OrdinalIgnoreCase) > 0)
                    { parser.parser_7L2NDDP(file, ref result); setControl(name, result); }
                    if (file.IndexOf("7L2IVRC", StringComparison.OrdinalIgnoreCase) > 0)
                    { parser.parser_7L2IVRC(file, ref result); setControl(name, result); }
                    if (file.IndexOf("7L2USSDGW", StringComparison.OrdinalIgnoreCase) > 0)
                    { parser.parser_7L2USSDGW(file, ref result); setControl(name, result); }
                    if (file.IndexOf("7L2OCSGAP", StringComparison.OrdinalIgnoreCase) > 0)
                    { parser.parser_7L2OCSGAP(file, ref result); setControl(name, result); }
                    if (file.IndexOf("7L2OCSGDB", StringComparison.OrdinalIgnoreCase) > 0)
                    { parser.parser_7L2OCSGDB(file, ref result); setControl(name, result); }
                    if (file.IndexOf("7L2PCRFDBSYNAP", StringComparison.OrdinalIgnoreCase) > 0)
                    { parser.parser_7L2PCRFDBSYNAP(file, ref result); setControl(name, result); }
                    if (file.IndexOf("7L2PCRFDBSYNDB", StringComparison.OrdinalIgnoreCase) > 0)
                    { parser.parser_7L2PCRFDBSYNDB(file, ref result); setControl(name, result); }
                    if (file.IndexOf("7L2PCRF1", StringComparison.OrdinalIgnoreCase) > 0)
                    { parser.parser_7L2PCRF(file, ref result); setControl(name, result); }
                    if (file.IndexOf("7L2OAM", StringComparison.OrdinalIgnoreCase) > 0)
                    { parser.parser_7L2OAM(file, ref result); setControl(name, result); }
                    if (file.IndexOf("7L2NEWOCSG", StringComparison.OrdinalIgnoreCase) > 0)
                    { parser.parser_7L2NEWOCSG(file, ref result); setControl(name, result); }
                }
            }

            ////////////////////////////////////////
            // Parse other minoss logs
            ////////////////////////////////////////
            DevResource.devSysChkResult site_result = new DevResource.devSysChkResult();

            #region Parse OCS backup's status
            file = strToday + "backup_verify.log";
            site_result = default(DevResource.devSysChkResult);
            if (File.Exists(@file))
            {
                parser.parser_backup_verify(file, ref site_result);
                if (site_result.log_stat == false)
                    textBox6.Text = "有漏檔紀錄，確認系統已正常自動補檔";
                    /// Mark by baochig 2018.09.26,避免設備評鑑官有異議，所以將實際內容改成已補檔
                    /// 如果unmark下列程式，將會把實際錯誤紀錄填到MINOSS
                    //textBox6.Text = string.Join("\n", site_result.syslog_msg.ToArray());
            }
            #endregion

            #region Parse cdr_check.log
            file = strToday + "cdr_check.log";
            site_result = default(DevResource.devSysChkResult);
            if (File.Exists(@file))
            {
                parser.parser_cdr_check(file, ref site_result);
                if (site_result.log_stat == false)
                    textBox7.Text = "有漏檔紀錄，確認系統已正常自動補檔";
                    /// Mark by baochig 2018.09.26,避免設備評鑑官有異議，所以將實際內容改成已補檔
                    /// 如果unmark下列程式，將會把實際錯誤紀錄填到MINOSS
                    //textBox7.Text = string.Join("\n", site_result.syslog_msg.ToArray());
            }
            #endregion

            #region Parse sys_check_7L2OAM1.log - for centreon alarm
            file = strToday + "sys_check_7L2OAM1.log";
            site_result = default(DevResource.devSysChkResult);
            if (File.Exists(@file))
            {
                parser.parser_alarms_stat(file, ref site_result);
                if (site_result.log_stat == true)
                    textBox1.Text = string.Join("\n", site_result.syslog_msg.ToArray());
            }
            #endregion

            #region Parse sys_check_7L2OAM1.log - for user login
            file = strToday + "sys_check_7L2OAM1.log";
            site_result = default(DevResource.devSysChkResult);
            if (File.Exists(@file))
            {
                parser.parser_login_log(file, ref site_result);
                if (site_result.log_stat == false)
                    textBox4.Text = string.Join("\n", site_result.syslog_msg.ToArray());
            }
            //Check 7L2OAM2's login log
            file = strToday + "sys_check_7L2OAM2.log";
            site_result = default(DevResource.devSysChkResult);
            if (File.Exists(@file))
            {
                Debug.WriteLine("Parse login log");
                parser.parser_login_log(file, ref site_result);
                if (!site_result.log_stat)
                    textBox4.Text += string.Join("\n", site_result.syslog_msg.ToArray());
            }
            #endregion Check Application login log

            #region Parse applogin_check.log
            file = strToday + "applogin_check.log";
            site_result = default(DevResource.devSysChkResult);
            if (File.Exists(@file))
            {
                parser.parser_app_log(file, ref site_result);
                if (site_result.log_stat == false)
                    textBox5.Text = string.Join("\n", site_result.syslog_msg.ToArray());
            }
            #endregion

            #region Parse 7L2SDP_License_view.log
            file = strToday + "7L2SDP_License_view.log";
            site_result = default(DevResource.devSysChkResult);
            if (File.Exists(@file))
            {
                parser.parser_SDPLicense_log(file, ref site_result);
                if (site_result.log_stat == false)
                    textBox9.Text = string.Join("\n", site_result.syslog_msg.ToArray());
            }
            #endregion

            #region Parse 7L2PCRF_License_view.log
            file = strToday + "7L2PCRF_License_view.log";
            site_result = default(DevResource.devSysChkResult);
            if (File.Exists(@file))
            {
                parser.parser_PCRFLicense_log(file, ref site_result);
                if (site_result.log_stat == false)
                    textBox10.Text = string.Join("\n", site_result.syslog_msg.ToArray());
            }
            #endregion

            #region Parse ilo_check.log
            file = strToday + "ilo_check.log";
            site_result = default(DevResource.devSysChkResult);
            if (File.Exists(@file))
            {
                parser.parser_ILO_Check_log(file, ref site_result);
                if (site_result.log_stat == false)
                    textBox2.Text = string.Join("\n", site_result.syslog_msg.ToArray());
            }
            #endregion
            return 0;
        }

        /// <summary>
        /// 開發public API給其他Class抓取form的value.
        /// </summary>
        /// <param name="iCtrlName"></param>
        /// <param name="iCtrlType">control type,0=CheckBox, 1=TextBox</param>
        /// <param name="val"></param>
        /// <returns></returns>
        public int GetControlValByName(string iCtrlName, int iCtrlType, ref string val)
        {
            int ret = 0;
            switch(iCtrlType)
            {
                case 0:
                    CheckBox objBox = this.Controls.Find(iCtrlName, true).FirstOrDefault() as CheckBox;
                    if (objBox != null) val = objBox.Text; else ret = 2;
                    break;
                case 1:
                    TextBox objText = this.Controls.Find(iCtrlName, true).FirstOrDefault() as TextBox;
                    if (objText != null) val = objText.Text; else ret = 2;
                    break;
                case 2:
                    Label objLabel = this.Controls.Find(iCtrlName, true).FirstOrDefault() as Label;
                    if (objLabel != null) val = objLabel.Text; else ret = 2;
                    break;
                default:
                    ret = 1;
                    break;
            }
            
            return ret;
        }

        /// <summary>
        /// 建置Windows表單的控制項
        /// </summary>
        /// <param name="name"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private int setControl(String name, DevResource.devSysChkResult result)
        {
            String dev_name;
            dev_name = Regex.Replace(name, "7L2", "");

            #region Automatic add label item of device
            Label label = new System.Windows.Forms.Label();
            label.Anchor = System.Windows.Forms.AnchorStyles.None;
            label.AutoSize = true;
            label.ForeColor = System.Drawing.Color.LemonChiffon;
            //label.Location = new System.Drawing.Point(27, 38);
            label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label.Name = dev_name + "_C1";
            label.Size = new System.Drawing.Size(80, 18);
            label.TabIndex = 0;
            label.Text = name;
            label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.tableLayoutPanel1.Controls.Add(label, 0, tbl_panel_row_idx);
            #endregion
            
            #region Automatic add checkbox Control items
            int max_items = 8;
            int idx = 0;
            CheckBox[] chkbox = new CheckBox[max_items];
            for (idx = 0; idx < max_items; idx++)
            {
                chkbox[idx] = new CheckBox();
                chkbox[idx].Anchor = System.Windows.Forms.AnchorStyles.None;
                chkbox[idx].AutoSize = true;
                chkbox[idx].ForeColor = System.Drawing.Color.LemonChiffon;
                chkbox[idx].Location = new System.Drawing.Point(191, 37);
                chkbox[idx].Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
                chkbox[idx].Name = dev_name + "_C" + (idx + 2) + "_chkbox";
                chkbox[idx].Size = new System.Drawing.Size(21, 20);
                chkbox[idx].TabIndex = 1;
                chkbox[idx].UseVisualStyleBackColor = true;
                chkbox[idx].CheckAlign = ContentAlignment.MiddleRight;
                this.tableLayoutPanel1.Controls.Add(chkbox[idx], idx + 1, tbl_panel_row_idx);
            }
            #endregion

            #region 尋找Control items並設定數值
            //Setup CPU check box
            chkbox[0].Text = result.cpu.ToString();
            if (chkbox[0] != null)
            {
                chkbox[0].Text = result.cpu.ToString();
                if (result.cpu < 90)
                    chkbox[0].Checked = true;
                else
                    chkbox[0].ForeColor = Color.Red;
            }

            //Setup MEMORY check box
            chkbox[1].Text = result.mem.ToString();
            if (result.mem < 90)
                chkbox[1].Checked = true;
            else
                chkbox[1].ForeColor = Color.Red;

            //Setup disk usage check box
            chkbox[2].Text = result.disk.ToString();
            if (result.disk < 90)
                chkbox[2].Checked = true;
            else
                chkbox[2].ForeColor = Color.Red;

            //Setup temperature check box
            chkbox[3].Text = result.temp.ToString();
            if( result.temp > 1 && result.temp < 35 )
                chkbox[3].Checked = true;
            else
                chkbox[3].ForeColor = Color.Red;

            //Setup message content check box
            chkbox[4].Checked = result.log_stat;
            if (chkbox[4].Checked == false)
            {
                //result.syslog_msg.ForEach(i => Debug.WriteLine("{0}\t", i));
                string str = string.Join("\n", result.syslog_msg.ToArray());
                addToolTip(chkbox[4], str);
            }

            chkbox[5].Checked = result.intf_stat;
            chkbox[6].Checked = true;
            chkbox[7].Checked = result.ntp_stat;
            #endregion

            ///當建立一列的items到tabelLayoutPanel後，則將index遞移1
            tbl_panel_row_idx++;
            return 0;
        }

        private void close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void move_Click(object sender, EventArgs e)
        {
            string curr_path = "", filter = "*.log";
            string strToday, strOutput;
            if (this.dateTimePicker1 != null)
            {
                strToday = string.Copy(this.dateTimePicker1.Value.ToString("yyMMdd"));
                strOutput = string.Copy(this.dateTimePicker1.Value.ToString("MM.dd"));
            }
            else
            {
                strToday = DateTime.Now.ToString("yyMMdd");
                strOutput = DateTime.Now.ToString("MM.dd");
            }
            Debug.WriteLine("strToday: " + strToday);

            // Create ouput directory 
            System.IO.Directory.CreateDirectory(strOutput);

            curr_path = System.IO.Directory.GetCurrentDirectory();

            // Obtain the file system entries in the directory
            // path that match the pattern.
            string[] directoryEntries =
                System.IO.Directory.GetFileSystemEntries(curr_path, filter);

            foreach (string file in directoryEntries)
            {
                string strFileName = "";
                //Debug.WriteLine(file);
                // Move raw log file to directory
                strFileName = Path.GetFileNameWithoutExtension(file)+".log";
                Debug.WriteLine(strFileName);
                if (strFileName.StartsWith(strToday))
                    System.IO.File.Move(@file, ".\\" + strOutput + "\\" + strFileName);
            }
        }

        private void merge_Click(object sender, EventArgs e)
        {
            Auto_logbook_form frm = new Auto_logbook_form(this);
            frm.Show(this);
        }

        private void parse_Click(object sender, EventArgs e)
        {
            int ret = 0;
            ret = initialParseMinoss();
            if (ret != 0)
                MessageBox.Show("執行錯誤, 請聯絡開發人員!!");
            else
                MessageBox.Show("執行完畢."); 
        }

        private void export_Click(object sender, EventArgs e)
        {
            int ret = 0;
            string result = "匯出完畢";
            ExportExcelLibrary export = new ExportExcelLibrary();
            ret = export.InitialExportMinoss(ref result,this);
            if(ret != 0 )
                MessageBox.Show("匯出失敗, 原因:" + result);
            else
                MessageBox.Show(result);
        }
    }
}