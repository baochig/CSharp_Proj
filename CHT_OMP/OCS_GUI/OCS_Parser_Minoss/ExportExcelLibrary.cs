using OCS_Parser_Minoss_GUI;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace OCS_GUI.OCS_Parser_Minoss
{
    class ExportExcelLibrary
    {
        object missing = Type.Missing;
        string path = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
        string[] chkDevs = {
                "7L2SDP1a", "7L2SDP1b", "7L2SDP2a", "7L2SDP2b", "7L2AIR1", "7L2AIR2",
                "7L2AIR3", "7L2AIR4", "7L2CCN1", "7L2TMSAP1", "7L2TMSAP2", "7L2TMSDB1",
                "7L2TMSDB2", "7L2TMSDB3", "7L2TMSDB4", "7L2TMSREP", "7L2NDDP1", "7L2NDDP2",
                "7L2NDDP3", "7L2NDDP4", "7L2IVRC1", "7L2IVRC2", "7L2USSDGW1", "7L2USSDGW2",
                "7L2OCSGAP1", "7L2OCSGAP2", "7L2OCSGAP3", "7L2OCSGAP4", "7L2OCSGDB3", "7L2OCSGDB4",
                "7L2PCRF1", "7L2OAM1", "7L2OAM2" };
        string[] chkItems = { "CPU Usage", "Memory Usage", "Disk Space", "溫度 %", "Access Log", "Interface", "流量", "NTP" };

        // 建置設備檢查資料框架
        private int CreateDevCheckSheet(ref Excel.Worksheet xlWorkSheet, ParseResult parser)
        {
            string strToday = DateTime.Now.ToString("yyyy/MM/dd");
            xlWorkSheet.Name = "設備檢查";
            xlWorkSheet.Cells[1, 1] = "機房";
            xlWorkSheet.Cells[1, 2] = "值班別";
            xlWorkSheet.Cells[1, 3] = "IN服務";
            xlWorkSheet.Cells[1, 4] = "設備類別";
            xlWorkSheet.Cells[1, 5] = "設備名稱";
            xlWorkSheet.Cells[1, 6] = "檢查項目";
            xlWorkSheet.Cells[1, 7] = "檢查結果";
            xlWorkSheet.Cells[1, 8] = "建立時間";
            xlWorkSheet.Cells[1, 9] = "填寫者";
            xlWorkSheet.Cells[1, 10] = "填寫時間";
            xlWorkSheet.Cells[1, 11] = "是否已完成";

            for (int iDevIdx = 0; iDevIdx <= 32; iDevIdx++)
            {
                //Debug.WriteLine(chkDevs[iDevIdx]);
                string strDevName = chkDevs[iDevIdx];
                //Detach "7L2" from device name
                string strDevName2 = chkDevs[iDevIdx].Replace("7L2", string.Empty);
                // Device name in form
                string strDevNameLabel = string.Empty;
                // Check export device is exist in minoss parser.
                bool bMatch = false;
                if (parser.GetControlValByName(strDevName2 + "_C1", 2, ref strDevNameLabel) == 0)
                    bMatch = true;

                //j = 檢測項目
                for (int j = 0; j < 8; j++)
                {
                    int row = ((iDevIdx * 8) + j) + 2; // 2 is align of sheets
                    xlWorkSheet.Cells[row, 1] = "高雄覺民6F OCS";
                    xlWorkSheet.Cells[row, 2] = "C班";
                    xlWorkSheet.Cells[row, 3] = "OCS";
                    xlWorkSheet.Cells[row, 4] = strDevName2;
                    xlWorkSheet.Cells[row, 5] = strDevName;
                    xlWorkSheet.Cells[row, 6] = chkItems[j];
                    if (bMatch == true)
                    {
                        string Checkval = string.Empty;
                        if (parser.GetControlValByName(strDevName2 + "_C" + (j + 2) + "_chkbox", 0, ref Checkval) == 0)
                            xlWorkSheet.Cells[row, 7] = Checkval;
                        Debug.WriteLine("Value" + Checkval);
                    }
                    else
                        xlWorkSheet.Cells[row, 7] = string.Empty;
                    xlWorkSheet.Cells[row, 8] = strToday;
                    xlWorkSheet.Cells[row, 10] = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
                    xlWorkSheet.Cells[row, 11] = "Y";
                }
            }
            return 0;
        }

        /// <summary>
        /// 因表單的物件編號跟實際內容有出入, 因此需要一個mapping的API返回正確的數值並匯到excel.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string SelectIDCContent(ParseResult parser, int iMaxIDCItemIdx)
        {
            string ret = "Select NOT found.";
            switch (iMaxIDCItemIdx)
            {
                case 0:
                    ret = string.Copy("檢查正常(環境溫度28度C 濕度53%)");
                    break;
                case 1:
                    parser.GetControlValByName("textBox1", 1, ref ret);
                    break;
                case 2:
                    parser.GetControlValByName("textBox2", 1, ref ret);
                    break;
                case 3:
                    parser.GetControlValByName("textBox3", 1, ref ret);
                    break;
                case 4:
                    parser.GetControlValByName("textBox4", 1, ref ret);
                    break;
                case 5:
                    parser.GetControlValByName("textBox5", 1, ref ret);
                    break;
                case 6:
                    parser.GetControlValByName("textBox6", 1, ref ret);
                    break;
                case 7:
                    parser.GetControlValByName("textBox7", 1, ref ret);
                    break;
                case 8:
                    parser.GetControlValByName("textBox8", 1, ref ret);
                    break;
                case 9:
                    ret = string.Copy("3G DXC設備檢查正常(每日)");
                    break;
                case 10:
                    ret = string.Copy("4樓編號: 19 & 20 及6樓編號:11，3台監控錄影設備功能檢視正常");
                    break;
                case 11:
                    ret = string.Copy("監控錄影設備日期時間檢視正常");
                    break;
                case 12:
                    parser.GetControlValByName("textBox9", 1, ref ret);
                    break;
                case 13:
                    parser.GetControlValByName("textBox10", 1, ref ret);
                    break;
                default:
                    break;
            }
            return ret;
        }

        //建置機房維運資料框架
        private int CreateIDCCheckSheet(ref Excel.Worksheet xlWorkSheet, ParseResult parser)
        {
            int iMaxIDCItems = 14;
            string strToday = DateTime.Now.ToString("yyyy/MM/dd");
            string[] chkIDCItems = {
                "OCS 系統設備巡視(含環境溫度紀錄,燈號目視)", "OCS : 系統告警檢查", "OCS : 系統障礙檢查", 
                "OCS_ISB trunk 中繼電路檢查", "OCS系統主機syslog紀錄(含登入紀錄)查核", "OCS系統主機應用程式log紀錄(含登入紀錄)查核",
                "OCS系統資料庫備份紀錄檢查(每周三 full其餘差異備份)", "OCS CDR傳檔紀錄檢查", "OCS主要功能測試","3G DXC設備檢查(每日)",
                "ISO 27001 檢查監控錄影設備功能是否正常(每日) ", "ISO 27001 檢查監控錄影設備日期時間是否正常(每日)",
                "SDP授權數及實際供裝數", "PCRF授權數及高雄、台北IP session數"
                                   };

            xlWorkSheet.Name = "機房維運";
            xlWorkSheet.Cells[1, 1] = "機房";
            xlWorkSheet.Cells[1, 2] = "值班別";
            xlWorkSheet.Cells[1, 3] = "工作項目";
            xlWorkSheet.Cells[1, 4] = "工作結果";
            xlWorkSheet.Cells[1, 5] = "週期";
            xlWorkSheet.Cells[1, 6] = "工作開始日期";
            xlWorkSheet.Cells[1, 7] = "工作結束日期";
            xlWorkSheet.Cells[1, 8] = "填寫者";
            xlWorkSheet.Cells[1, 9] = "填寫時間";
            xlWorkSheet.Cells[1, 10] = "是否已完成";
            // 填寫機房維運日誌
            for (int i = 0; i < iMaxIDCItems; i++)
            {
                int iCellidx = i + 2;
                xlWorkSheet.Cells[iCellidx, 1] = "高雄覺民6F OCS";
                xlWorkSheet.Cells[iCellidx, 2] = "C班";
                xlWorkSheet.Cells[iCellidx, 3] = chkIDCItems[i];
                xlWorkSheet.Cells[iCellidx, 4] = SelectIDCContent(parser,i);
                xlWorkSheet.Cells[iCellidx, 5] = "每日";
                xlWorkSheet.Cells[iCellidx, 6] = strToday;
                xlWorkSheet.Cells[iCellidx, 7] = strToday;
                xlWorkSheet.Cells[iCellidx, 9] = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
                xlWorkSheet.Cells[iCellidx, 10] = "Y";
            }
            return 0;
        }

        /// <summary>
        /// 建立一個Excel檔案, 並檢察環境問題
        /// </summary>
        /// <param name="xlApp"></param>
        /// <returns></returns>
        private int CreateExcelFile(out Excel.Application xlApp)
        {
            xlApp = new Excel.Application();
            if (xlApp == null)
            {
                MessageBox.Show("Excel is not properly installed!!");
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// 匯出初始進入點，當Minoss log parse 完畢後，執行此功能將會匯出一個Excel
        /// </summary>
        /// <returns></returns>
        public int InitialExportMinoss(ref string result, ParseResult parser)
        {
            int ret = 0;
            string strExportFile = DateTime.Now.ToString("yyMMdd") + "_MINOSS_export.xls";
            // Step1. Create a excel file.
            Excel.Application xlApp;
            ret = this.CreateExcelFile(out xlApp);
            if (ret != 0)
            {
                result = string.Copy("建立Excel檔案失敗, 請聯絡開發人員");
                return 1;
            }
            // Step2. Setup workbook parameters
            Excel.Workbook xlWorkBook = xlApp.Workbooks.Add(missing);
            
            // Step3. Create first sheet, and write device check resultto this sheet.
            Excel.Worksheet xlWorkSheet1;
            xlWorkSheet1 = (Excel.Worksheet)xlWorkBook.ActiveSheet;
            ret = CreateDevCheckSheet(ref xlWorkSheet1, parser);

            // Step4. Create second sheet, and write IDC check result to this sheet.
            Excel.Worksheet xlWorkSheet2;
            xlWorkSheet2 = (Excel.Worksheet)xlWorkBook.Sheets.Add(missing, missing, 1, missing);
            ret = CreateIDCCheckSheet(ref xlWorkSheet2, parser);

            // Step5. Save the data
            try
            {
                xlWorkBook.SaveAs(path + "\\" + strExportFile);
                xlWorkBook.Close(true, missing, missing);
                xlApp.Quit();
            }
            catch (Exception ex)
            {
                result = string.Copy(ex.Message);
            }
            return 0;
        }
    }
}
