using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Diagnostics;
using HtmlAgilityPack;
using System.Web;
using OCS_Parser_Minoss_GUI;
using System.Text.RegularExpressions;
using System.Threading;
using System.Reflection;
//using OCS_ADP = OCS_Database.MSSQL.OCSMsSqlDataAdapter;
using System.Data;
using System.Globalization;

namespace OCS_auto_minoss
{
    public partial class Auto_logbook_form : Form
    {
        // 靜態變數或者初始化的變數
        private int OCS_SITE_ID = 11;
        private int MAX_HOST = 40; //要填寫的維運設備數量(總設備數量+1)
        private int MAX_COLUMN = 8; //每一台維運設備要填寫的欄位數量要填寫的
        private string AM_URL = " https://am.cht.com.tw";
        private string MINOSS_URL = "https://minoss.cht.com.tw";
        private string NMOSS_URL = "https://nmoss.cht.com.tw";
        private string LOG_DIR = "LOG/";
        private string LOG_FILE = "LOG/OCS_GUI.log";
        Site_Authority.nmossAuthDataStruc nmossAuth = new Site_Authority.nmossAuthDataStruc();

        // 這些變數用來傳遞給其他Thread
        bool bIsLoginSSO = false;
        string response = null;
        WebClientEx client = null;

        /// <summary>
        /// Load global setting configuration from OCS_Database
        /// </summary>
        private void LoadSettingFromDB()
        {
            IEnumerable<DataRow> collection = null;
            string result = string.Empty;
            int ret = 1;
            MAX_COLUMN = 40;
            MAX_HOST = 8;
            //OCS_ADP ocs_data = new OCS_ADP();
            /*
            ret = ocs_data.GetMinossSetting(ref collection, ref result);
            if (ret == 0)
            {
                foreach (DataRow value in collection)
                {
                    MAX_COLUMN = value.Field<byte>(value.Table.Columns["MAX_COLUMN"].Ordinal);
                    MAX_HOST = value.Field<byte>(value.Table.Columns["MAX_HOST"].Ordinal);
                }
            }
            */
            return;
        }

        /// <summary>
        /// Write OCS log format to log file
        /// </summary>
        /// <param name="logMessage"></param>
        /// <param name="w"></param>
        public void Log(string logMessage)
        {
            logMessage = string.Copy(DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString() + " : " + logMessage+"\r\n");
            File.AppendAllText(LOG_FILE, logMessage, Encoding.Default);
        }

        public Auto_logbook_form(ParseResult parentform)
        {
            InitializeComponent();
            this.Tag = parentform;
            //Check log directory is exist, if NOT, we will create a directory.
            if (!Directory.Exists(LOG_DIR))
                Directory.CreateDirectory(LOG_DIR);
            LoadSettingFromDB();
        }

        private bool IsRetryable(WebException ex)
        {
            return
                ex.Status == WebExceptionStatus.ReceiveFailure ||
                ex.Status == WebExceptionStatus.Timeout ||
                ex.Status == WebExceptionStatus.ConnectFailure ||
                ex.Status == WebExceptionStatus.KeepAliveFailure ||
                ex.Status == WebExceptionStatus.ProtocolError ||
                ex.Status == WebExceptionStatus.UnknownError;
        }

        /// <summary>
        /// This function use WebRequest to Get web URL page and store cookie
        /// </summary>
        public class WebClientEx : WebClient
        {
            private CookieContainer _cookieContainer = new CookieContainer();
            Uri _responseUri;

            public WebClientEx(CookieContainer container)
            {
                if (container != null)
                    this._cookieContainer = container;
            }

            public CookieContainer CookieContainer
            {
                get { return _cookieContainer; }
                set { _cookieContainer = value; }
            }

            private void ReadCookies(WebResponse r)
            {
                var response = r as HttpWebResponse;
                if (response != null)
                {
                    CookieCollection cookies = response.Cookies;
                    _cookieContainer.Add(cookies);
                }
            }
            public Uri ResponseUri
            {
                get { return _responseUri; }
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest request = base.GetWebRequest(address);
                if (request is HttpWebRequest)
                {
                    (request as HttpWebRequest).ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                    (request as HttpWebRequest).CookieContainer = _cookieContainer;
                    (request as HttpWebRequest).KeepAlive = true;
                }
                return request;
            }
            protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
            {
                WebResponse response = base.GetWebResponse(request, result);
                ReadCookies(response);
                return response;
            }
            protected override WebResponse GetWebResponse(WebRequest request)
            {
                WebResponse response = null;
                try
                {
                    response = base.GetWebResponse(request);
                    _responseUri = response.ResponseUri;
                    ReadCookies(response);
                }
                catch (WebException ex)
                {
                    // Logger error code
                    WebResponse errResp = ex.Response;
                    if (errResp != null)
                    {
                        using (Stream respStream = errResp.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(respStream);
                            string text = reader.ReadToEnd();
                            File.AppendAllText("RespError.txt", text);
                        }
                    }
                    else
                        File.AppendAllText("RespError.txt", ex.Message);
                }
                return response;
            }
        }

        public void CHT_AM_Headers(WebClientEx client)
        {
            client.Headers.Set("Accept", "application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, */* ");
            client.Headers.Set("Accept-Encoding", "gzip, deflate");
            client.Headers.Set("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E; CMNTDFJS; InfoPath.3)");
            client.Headers.Set("Accept-Language", "zh-TW");
        }

        /// <summary>
        /// This function use try catch to package WebClient UploadValues function
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public int clientUploadValues(WebClientEx client, string url, NameValueCollection data, ref string response)
        {
            const int maxRetries = 5;
            bool done = false;
            int attempts = 0;
            response = string.Empty;
            while (!done)
            {
                attempts++;
                try
                {
                    Log("clientUploadValues url: " + url);
                    Log(client.Headers.ToString());
                    response = System.Text.Encoding.UTF8.GetString(client.UploadValues(url, data));
                    done = true;
                }
                catch (WebException ex)
                {
                    Log("Status:" + ex.Status);
                    if (!IsRetryable(ex))
                        throw;
                    if (attempts >= maxRetries)
                        return 1;
                    Thread.Sleep(attempts * 1000);
                }
            }
            return 0;
        }

        /// <summary>
        /// This function use try catch to package WebClient UploadData function
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <param name="postArray"></param>
        /// <returns></returns>
        public int clientUploadData(WebClientEx client, string url, byte[] postArray, ref string response)
        {
            const int maxRetries = 5;
            bool done = false;
            int attempts = 0;
            while (!done)
            {
                attempts++;
                try
                {
                    Log("clientUploadData url: " + url);
                    Log(client.Headers.ToString());
                    response = client.UploadData(url, postArray).ToString();
                    done = true;
                }
                catch (WebException ex)
                {
                    Log("Status:" + ex.Status);
                    if (!IsRetryable(ex))
                        throw;
                    if (attempts >= maxRetries)
                        return 1;
                    Thread.Sleep(attempts * 1000);
                }
            }
            return 0;
        }

        /// <summary>
        /// This function use try catch to package WebClient UploadString function
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public int clientUploadString(WebClientEx client, string url, string data, ref string response)
        {
            const int maxRetries = 5;
            bool done = false;
            int attempts = 0;
            response = string.Empty;
            while (!done)
            {
                attempts++;
                try
                {
                    Log("clientUploadValues url: " + url);
                    Log(client.Headers.ToString());
                    response = client.UploadString(url, data);
                    done = true;
                }
                catch (WebException ex)
                {
                    Log("Status:" + ex.Status);
                    if (!IsRetryable(ex))
                        throw;
                    if (attempts >= maxRetries)
                        return 1;
                    Thread.Sleep(attempts * 1000);
                }
            }
            return 0;
        }

        /// <summary>
        /// This function use try catch to package WebClient DownloadString function
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public int clientDownloadString(WebClientEx client, string url, ref string response)
        {
            const int maxRetries = 6;
            bool done = false;
            int attempts = 0;
            response = string.Empty;
            while (!done)
            {
                attempts++;
                try
                {
                    Log("clientDownloadString url: " + url);
                    Log(client.Headers.ToString());
                    //SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)(0xc0 | 0x300 | 0xc00);
                    response = client.DownloadString(url);
                    done = true;
                }
                catch (WebException ex)
                {
                    Log("Status:" + ex.Status);
                    if (!IsRetryable(ex))
                        throw;
                    if (attempts >= maxRetries)
                        return 1;
                    Thread.Sleep(attempts*2000);
                }
            }
            return 0;
        }

        /// <summary>
        /// This function encode url string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string UrlEncode(string str)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in str)
            {
                if (HttpUtility.UrlEncode(c.ToString()).Length > 1)
                {
                    builder.Append(HttpUtility.UrlEncode(c.ToString()).ToUpper());
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        ///////////////////////////////////////////// Auto MINOSS Program /////////////////////////////////////////////
        /// <summary>
        /// 此函式能夠 Parse HTTP response回來的網頁, 將hidden的資料全部轉成NameValueCollection型態
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="nvcPostData"></param>
        private void parseHtmlHiddenNameVal(HtmlAgilityPack.HtmlDocument doc, ref NameValueCollection nvcPostData)
        {
            try
            {
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//input"))
                {
                    if (node != null)
                    {
                        if (node.Attributes["type"] != null)
                        {
                            if (String.Compare(node.Attributes["type"].Value, "hidden") == 0)
                            {
                                if (node.Attributes["value"] != null)
                                {
                                    if (nvcPostData[node.Attributes["name"].Value] == null)
                                        nvcPostData.Add(node.Attributes["name"].Value, node.Attributes["value"].Value);
                                    else
                                    {
                                        if (string.IsNullOrWhiteSpace(nvcPostData[node.Attributes["name"].Value]))
                                            nvcPostData.Set(node.Attributes["name"].Value, node.Attributes["value"].Value);
                                    }
                                }
                                else
                                {
                                    if (nvcPostData[node.Attributes["name"].Value] == null)
                                        nvcPostData.Add(node.Attributes["name"].Value, "");
                                    else
                                    {
                                        if (string.IsNullOrWhiteSpace(nvcPostData[node.Attributes["name"].Value]))
                                            nvcPostData.Set(node.Attributes["name"].Value, "");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Log("Parse make exception: " + ex.Message);
            }
            return;
        }

        /// <summary>
        /// 因為MINOSS 事件紀錄有自己的隱藏欄位格式, 所以需要特別的parsing後在儲存到nvcPostData
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="nvcPostData"></param>
        private void parseMinossHtmlHiddenFields(string  strWebResponse, ref NameValueCollection nvcPostData)
        {
            using (StringReader str = new StringReader(strWebResponse))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    if (line.IndexOf("hiddenField", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        string[] strs = line.Split(new string[] { "hiddenField" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string strHiddenLine in strs)
                        {
                            try
                            {
                                string[] strsInnStrs = strHiddenLine.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                if (string.Compare(strsInnStrs[0], "__VIEWSTATE") == 0)
                                {
                                    nvcPostData.Set("__VIEWSTATE", strsInnStrs[1]);
                                    Log("__VIEWSTATE: " + strsInnStrs[1]);
                                }
                                if (strHiddenLine.IndexOf("__EVENTVALIDATION", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    nvcPostData.Set("__EVENTVALIDATION", strsInnStrs[1]);
                                    Log("__EVENTVALIDATION: " + strsInnStrs[1]);
                                }
                                if (strHiddenLine.IndexOf("__VIEWSTATEGENERATOR", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    nvcPostData.Set("__VIEWSTATEGENERATOR", strsInnStrs[1]);
                                    Log("__VIEWSTATEGENERATOR: " + strsInnStrs[1]);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                }
            }
            return;
        }

        /// <summary>
        /// This function construct log book basic POST DATA
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="postData"></param>
        private int constructPostData(HtmlAgilityPack.HtmlDocument doc, ref NameValueCollection postData)
        {
            const string EVENTTARGET = "ctl00$ContentPlaceHolder1$MainBriefRoutineEQCheck_C_1$lbnSendAll";
            const string scriptManager1 = "ctl00$ContentPlaceHolder1$EQCheckUpdatePanel_C|ctl00$ContentPlaceHolder1$MainBriefRoutineEQCheck_C_1$lbnSendAll";

            //Construct POST data
            postData.Set("ctl00$ScriptManager1", scriptManager1);
            postData.Set("tvwFunction_ExpandState", "eununununnnnnenneunnnnnnnunnnnnnnnunnnnunnennnunnnnnunnnnn");
            postData.Set("tvwFunction_SelectedNode", "");
            postData.Set("tvwFunction_PopulateLog", "");
            postData.Set("ctl00$cb_menu_state", "on");
            postData.Set("ctl00$cb_logo_state", "on");
           
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//input"))
            {
                if (node != null)
                {
                    if (String.Compare(node.Attributes["type"].Value, "hidden") == 0)
                    {
                        if (node.Attributes["value"] != null)
                            postData.Set(node.Attributes["name"].Value, node.Attributes["value"].Value);
                        else
                            postData.Set(node.Attributes["name"].Value, "");
                    }
                }
            }
            postData.Set("__EVENTTARGET", EVENTTARGET);
            postData.Set("__EVENTARGUMENT", "");
            postData.Set("__LASTFOCUS", "");
            postData.Set("__ASYNCPOST", "true");

            // Warning!! 建構form textbox欄位，這些資料不能缺少
            HtmlNodeCollection hnc = doc.GetElementbyId("ContentPlaceHolder1_MainBriefRoutineEQCheck_C_1_grvBriefRoutineEQCheck1").SelectNodes(".//input");
            if (hnc != null)
            {
                foreach (HtmlNode node in hnc)
                {
                    if (String.Compare(node.Attributes["type"].Value, "text") == 0)
                        postData.Set(node.Attributes["name"].Value, "");
                }
            }
            else
                return 1;
#if DEBUG
            foreach (string key in postData)
                Debug.WriteLine("Key: " + key + ", val: " + postData[key]);
#endif
            return 0;
        }

        /// <summary>
        /// 此函式主要是填寫工作日誌的設備檢查欄位，Parse目前工作日誌的狀態後，並送出實際檢查的結果
        /// </summary>
        /// <param name="client"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private int procLogBook_devCheck(WebClientEx client, HtmlAgilityPack.HtmlDocument doc)
        {
            int iStatus = 0;
            string strXPath;
            string strUrl = string.Empty;
            string strQueryStr = null;
            string strPostResponse = null, strGetResponse = null;
            string strDevName = null;
            NameValueCollection nvPostData;
            HtmlNode hnCheckbox, hnTextbox, hnDevName, hnTempTextbox,hnTempResult;
            // Dependent date to decide POST URL.
            if (nmossAuth.bIsHistory)
                strUrl = MINOSS_URL + "/LogBook/Maintain/MainHistory.aspx?siteID=" + OCS_SITE_ID.ToString() + "&date=" + this.Submit_MINOSS_Info.Text;
            else
                strUrl = MINOSS_URL + "/LogBook/Maintain/Main.aspx";

            // Setup header information
            CHT_AM_Headers(client);
            client.Headers.Set("Content-Type", "application/x-www-form-urlencoded; charset=utf-8");
            client.Headers.Set("Cache-Control", "no-cache");
            client.Headers.Set("Pragma", "no-cache");
            client.Headers.Remove("Accept");
            //client.Headers.Set("Accept", "*/*");
            client.Headers.Set("Accept-Encoding", "gzip, deflate");
            client.Headers.Set("Accept-Language", "zh-tw");
            client.Headers.Set("x-requested-with", "XMLHttpRequest");
            //client.Headers.Set("Referer", strUrl);
            client.Headers.Set("x-microsoftajax", "Delta=true");
            
            // 根據MINOSS網站分析要送出的POST資料類型，並將控制表單的分析數值存進POST資料中並送出
            for (int j = 1; j < MAX_HOST; j++)
            {
                // Get device name
                strXPath = "//*[@id=\"ContentPlaceHolder1_MainBriefRoutineEQCheck_C_1_grvBriefRoutineEQCheck1\"]/tr[" + (j + 1) + "]/td[1]";
                hnDevName = doc.DocumentNode.SelectSingleNode(strXPath);

                #region 檢查機制 - 判斷是否要處理此設備的工作日誌 1)沒工作日誌 2)已填過 3)設備檢查溫度為0
                //判斷1.如果MINOSS上沒有此主機的工作日誌則略過
                if (hnDevName == null)
                {
                    Log("Scan ParseResult's device name error!! XPath:" + strXPath);
                    continue;
                }
                else
                {
                    strDevName = Regex.Replace(hnDevName.InnerText, "7L2", "");
                    Log("Scan device: " + strDevName);
                }

                //判斷2.如果MINOSS此設備已填過則略過
                strXPath = "//input[@id=\"ContentPlaceHolder1_MainBriefRoutineEQCheck_C_1_grvBriefRoutineEQCheck1_TextBox4_" + (j - 1) + "\"]";
                hnTempTextbox = doc.DocumentNode.SelectSingleNode(strXPath);
                if (hnTempTextbox == null)
                {
                    strXPath = "//span[@id=\"ContentPlaceHolder1_MainBriefRoutineEQCheck_C_1_grvBriefRoutineEQCheck1_lblResult4_" + (j - 1) + "\"]";
                    hnTempResult = doc.DocumentNode.SelectSingleNode(strXPath);
                    int val;
                    //如果溫度已有數值代表以填寫過
                    if (int.TryParse(hnTempResult.InnerText, out val))
                        continue;
                    Log(strDevName + "已寫過數值" + val);
                }

                //判斷3.如果設備檢查溫度欄位未被create略過填寫此設備
                string strTempCtrlItemName = strDevName + "_C5_chkbox";
                CheckBox ctrlTempCheckBox = ((ParseResult)this.Tag).Controls.Find(strTempCtrlItemName, true).FirstOrDefault() as CheckBox;
                if (ctrlTempCheckBox == null)
                    continue;
                Log("strCtrlItemName: " + strTempCtrlItemName + ", temperature" + ctrlTempCheckBox.Text);
                #endregion

                // Warning!! 每次變更工作日誌物件，Hidden的欄位 都有所變化，因此需要重新建構POST的資料
                nvPostData = new NameValueCollection();
                iStatus = constructPostData(doc, ref nvPostData);
                if (iStatus != 0)
                    continue;

                for (int i = 1; i <= MAX_COLUMN; i++)
                {
                    string[] kvp = { "key", "value" }; //Idx[0] = key, Idx[1] = value
                    kvp[0] = kvp[1] = null;

                    #region Step1.判斷處理結果填到網頁的是textbox項目還是checkbox，並將結果存進keyvalue
                    // 在工作日誌的欄位中有textbox, 就不會有checkbox, 而checkbox有數值, 程式將會進行邏輯判斷, 判斷是否有異常數值，
                    // 並存進key value array
                    // Get textbox of syscheck result
                    string strCtrlItemName = strDevName + "_C" + (i+1) + "_chkbox";
                    CheckBox ctrlActCheckBox = ((ParseResult)this.Tag).Controls.Find(strCtrlItemName, true).FirstOrDefault() as CheckBox;
                    hnTextbox = doc.GetElementbyId("ContentPlaceHolder1_MainBriefRoutineEQCheck_C_1_grvBriefRoutineEQCheck1_TextBox" + i + "_" + (j - 1) + "");

                    //如果設備檢查欄位有textbox項目, 則確認系統是否有正常Parse到值，有的話則帶入
                    if (hnTextbox != null)
                    {
                        kvp[0] = String.Copy(hnTextbox.Attributes["name"].Value);
                        // 如果要填寫的數值等於0也忽略，代表parse的結果有問題
                        if (ctrlActCheckBox != null)
                            kvp[1] = String.Copy(ctrlActCheckBox.Text);
                        else
                            continue;
                        Log("strCtrlItemName: " + strCtrlItemName + ", Val: " + ctrlActCheckBox.Text);
                    }
                    else // 如果設備檢查欄位不是textbox項目(就一定是checkbox), 則確認系統是否有正常Parse到值，有的話則帶入
                    {
                        hnCheckbox = doc.GetElementbyId("ContentPlaceHolder1_MainBriefRoutineEQCheck_C_1_grvBriefRoutineEQCheck1_CheckBox" + i + "_" + (j - 1) + "");
                        if (hnCheckbox != null)
                        {
                            kvp[0] = String.Copy(hnCheckbox.Attributes["name"].Value);
                            if (ctrlActCheckBox != null)
                            {
                                if (ctrlActCheckBox.Checked)
                                    kvp[1] = String.Copy("on");
                                else
                                    kvp[1] = String.Copy("off");
                            }
                        }
                        Log("strCtrlItemName: " + strCtrlItemName + ", val: " + ctrlActCheckBox.CheckState.ToString());
                    }
                    #endregion

                    #region Step2.將keyValue array放進POST data，並
                    if (kvp[0] != null && kvp[1] != null)
                    {
                        if (nvPostData[kvp[0]] == null)
                            nvPostData.Add(kvp[0], kvp[1]);
                        else
                            nvPostData[kvp[0]] = String.Copy(kvp[1]);
                    }
                    #endregion
                }

                #region 送出HTTP POST Request
                /// NameValueCollection convert to byte array, and submit POST request
                /// Warning!! 如果不重新設定header，nmoss將會擋掉此封包,伺服器會回傳ErrorPage.aspx
                strQueryStr = String.Join("&", nvPostData.AllKeys.Select(key => string.Format("{0}={1}", UrlEncode(key), UrlEncode(nvPostData[key]))));
                Log(strQueryStr);
                client.Headers.Set("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E; CMNTDFJS; InfoPath.3)");
                strPostResponse = client.UploadString(strUrl, strQueryStr);

                // Warning!! 每次送出後，隱藏欄位會變動，因此重新Get工作日誌中的hidden欄位，
                iStatus = clientDownloadString(client, strUrl, ref strGetResponse);
                doc.LoadHtml(strGetResponse);
                // Debug
                doc.Save(LOG_DIR + "doc.html");

                // Debug
                //foreach (string key in nvPostData) Log("Key: " + key + ", Value: " + nvPostData[key]);
                //File.WriteAllText(j + "_" + i + ".html", strQueryStr, Encoding.Default);

                //Clear post NameValueCollection
                nvPostData.Clear();

                #endregion
            }
            return 0;
        }

        /// <summary>
        /// 填寫機房維運日誌的內容
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private int procSiteMaintainItems(WebClientEx client, string queryStr, string result)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            int status = 0;
            string response = null;
            string url = MINOSS_URL + "/LogBook/Maintain/";

            //Get url from URL query string
            string updateUrl;
            var kvp = HttpUtility.ParseQueryString(queryStr);
            updateUrl = url + "RoutineSiteMaintainUpdate.aspx?mode=result" + "&SiteMaintain_RID=" + kvp[1];
            Log("updateUrl: " + updateUrl);

            // Get site maintain item response
            status = clientDownloadString(client, updateUrl, ref response);
            doc.LoadHtml(response);

            //Clear Content
            result = Regex.Replace(result, ">", "");
            result = Regex.Replace(result, "<", "");

            //Construct basic POST form data
            var postData = new NameValueCollection
                    {
                        { "ctl00$ContentPlaceHolder1$txbResultContent", result },
                        { "ctl00$ContentPlaceHolder1$btn_SiteMaintain_Update", "送出" },
                    };
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//input"))
            {
                if (String.Compare(node.Attributes["type"].Value, "hidden") == 0)
                {
                    if (node.Attributes["value"] != null)
                        postData.Add(node.Attributes["name"].Value, node.Attributes["value"].Value);
                }
            }

            // Setup header information
            // client.Headers.Set("Content-Type", "application/x-www-form-urlencoded; charset=utf-8");
            client.Headers.Set("Pragma", "no-cache");
            client.Headers.Set("Accept", "application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*");
            client.Headers.Set("Accept-Encoding", "gzip, deflate");
            client.Headers.Set("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E; CMNTDFJS; InfoPath.3)");
            client.Headers.Set("Accept-Language", "zh-TW");
            client.Headers.Set("Referer", url);

            // POST site maintain item
            status = clientUploadValues(client, updateUrl, postData, ref response);
            if (status != 0) { webBrowser1.DocumentText = response; return 1; }
            File.Delete(LOG_DIR + "procSiteMaintainItems.html");
            File.AppendAllText(LOG_DIR + "procSiteMaintainItems.html", response);
            return status;
        }

        /// <summary>
        /// 此函式是主要填寫工作日誌的機房維運
        /// </summary>
        /// <param name="client"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private int procLogBook_SiteMaintain(WebClientEx client, HtmlAgilityPack.HtmlDocument doc)
        {
            int status = 0;

            //Get all sit maintain log book items
            HtmlAgilityPack.HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//*[@id=\"ContentPlaceHolder1_MainBriefRoutineSiteMaintain1_grvBriefRoutineSiteMaintain\"]//a[contains(@href,'detail')]");
            if (nodes != null)
            {
                string strLastWeekStartDate = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek) - 6).ToString("MM/dd");
                string strLastWeekEndDate = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek)).ToString("MM/dd");
                foreach (HtmlAgilityPack.HtmlNode node in nodes)
                {
                    Log("InnerText: " + node.InnerText);
                    if (node.Attributes["href"] == null)
                        continue;
                    switch (node.InnerText)
                    {
                        case "OCS 系統設備巡視(含環境溫度紀錄,燈號目視)":
                            procSiteMaintainItems(client, node.Attributes["href"].Value, "檢查正常(環境溫度26度C 濕度53%)");
                            break;
                        case "OCS : 系統告警檢查":
                            TextBox ctrlActTextBox1 = ((ParseResult)this.Tag).Controls.Find("textBox1", true).FirstOrDefault() as TextBox;
                            if (ctrlActTextBox1 != null)
                                procSiteMaintainItems(client, node.Attributes["href"].Value, ctrlActTextBox1.Text);
                            break;
                        case "OCS : 系統障礙檢查":
                            TextBox ctrlActTextBox2 = ((ParseResult)this.Tag).Controls.Find("textBox2", true).FirstOrDefault() as TextBox;
                            if (ctrlActTextBox2 != null)
                                procSiteMaintainItems(client, node.Attributes["href"].Value, ctrlActTextBox2.Text);
                            break;
                        case "OCS_ISB trunk 中繼電路檢查":
                            TextBox ctrlActTextBox3 = ((ParseResult)this.Tag).Controls.Find("textBox3", true).FirstOrDefault() as TextBox;
                            if (ctrlActTextBox3 != null)
                                procSiteMaintainItems(client, node.Attributes["href"].Value, ctrlActTextBox3.Text);
                            break;
                        case "OCS系統主機syslog紀錄(含登入紀錄)查核":
                            TextBox ctrlActTextBox4 = ((ParseResult)this.Tag).Controls.Find("textBox4", true).FirstOrDefault() as TextBox;
                            if (ctrlActTextBox4 != null)
                                procSiteMaintainItems(client, node.Attributes["href"].Value, ctrlActTextBox4.Text);
                            break;
                        case "OCS系統主機應用程式log紀錄(含登入紀錄)查核":
                            TextBox ctrlActTextBox5 = ((ParseResult)this.Tag).Controls.Find("textBox5", true).FirstOrDefault() as TextBox;
                            if (ctrlActTextBox5 != null)
                                procSiteMaintainItems(client, node.Attributes["href"].Value, ctrlActTextBox5.Text);
                            break;
                        case "OCS系統資料庫備份紀錄檢查(每周三 full其餘差異備份)":
                            TextBox ctrlActTextBox6 = ((ParseResult)this.Tag).Controls.Find("textBox6", true).FirstOrDefault() as TextBox;
                            if (ctrlActTextBox6 != null)
                                procSiteMaintainItems(client, node.Attributes["href"].Value, ctrlActTextBox6.Text);
                            break;
                        case "OCS CDR傳檔紀錄檢查":
                            TextBox ctrlActTextBox7 = ((ParseResult)this.Tag).Controls.Find("textBox7", true).FirstOrDefault() as TextBox;
                            if (ctrlActTextBox7 != null)
                                procSiteMaintainItems(client, node.Attributes["href"].Value, ctrlActTextBox7.Text);
                            break;
                        case "OCS主要功能測試":
                            TextBox ctrlActTextBox8 = ((ParseResult)this.Tag).Controls.Find("textBox8", true).FirstOrDefault() as TextBox;
                            if (ctrlActTextBox8 != null)
                               procSiteMaintainItems(client, node.Attributes["href"].Value, ctrlActTextBox8.Text);
                            break;
                        case "3G DXC設備檢查(每日)":
                            procSiteMaintainItems(client, node.Attributes["href"].Value, "3G DXC設備檢查正常(每日)");
                            break;
                        case "ISO 27001 連線DVR主機並確認各攝影機錄影功能是否正常(每日) ":
                            procSiteMaintainItems(client, node.Attributes["href"].Value, "4樓編號: 4 & 19 & 20 及6樓編號:11 & 26 & 27，6台監控錄影設備功能&資訊&伺服器容量檢視正常");
                            break;
                        case "ISO 27001 連線DVR主機並確認各攝影機錄影日期及時間是否正常(每日)":
                            procSiteMaintainItems(client, node.Attributes["href"].Value, "4樓編號: 4 & 19 & 20 及6樓編號:11 & 26 & 27，6台監控錄影設備日期&時間同步檢視正常");
                            break;
                        case "系統日誌是否正常傳送至LogCollector(每週)":
                            procSiteMaintainItems(client, node.Attributes["href"].Value, strLastWeekStartDate + " ~ " + strLastWeekEndDate + " 系統日誌功能檢查正常");
                            break;

                        case "ISO 27001 檢查監控錄影紀錄資料是否完整(每週)":
                            procSiteMaintainItems(client, node.Attributes["href"].Value, strLastWeekStartDate + " ~ " + strLastWeekEndDate + "編號:4 & 11 & 19 & 20 & 26 & 27　6台DVR攝影機錄影功能檢視正常");
                            break;
                        case "SDP授權數及實際供裝數":
                            TextBox ctrlActTextBox9 = ((ParseResult)this.Tag).Controls.Find("textBox9", true).FirstOrDefault() as TextBox;
                            if (ctrlActTextBox9 != null)
                               procSiteMaintainItems(client, node.Attributes["href"].Value, ctrlActTextBox9.Text);
                            break;
                        case "PCRF授權數及高雄、台北IP session數":
                            TextBox ctrlActTextBox10 = ((ParseResult)this.Tag).Controls.Find("textBox10", true).FirstOrDefault() as TextBox;
                            if (ctrlActTextBox10 != null)
                                procSiteMaintainItems(client, node.Attributes["href"].Value, ctrlActTextBox10.Text);
                            break;
                        case "7MSC6及OCS機房巡檢(每日)":
                            procSiteMaintainItems(client, node.Attributes["href"].Value, "檢查正常(每日)");
                            break;
                    }
                }
            }
            else
            {
                MessageBox.Show("機房維運日誌 Parse Error!!");
            }
            return status;
        }

        /// <summary>
        /// 上傳USB使用紀錄
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private int procMinossEventUpload(WebClientEx client)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            int status = 0;
            string tagUrl = string.Empty;
            string today = DateTime.Now.ToString("yyyy/MM/dd");
            string formDataBoundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
            string strQueryStr = null;
            string strPersonalID = null;
            NameValueCollection nvcPostData = new NameValueCollection();
            Stream formDataStream = new System.IO.MemoryStream();

            #region Step1. GET Personal ID
            tagUrl = string.Copy(MINOSS_URL + "/LogBook/Maintain/Main.aspx");
            status = clientDownloadString(client, tagUrl, ref response);
            if (status != 0) { webBrowser1.DocumentText = response; MessageBox.Show("取得維運日誌網頁資料錯誤"); return 2; }
            doc.LoadHtml(response);
            HtmlNode hnPersonId = doc.GetElementbyId("ContentPlaceHolder1_MainBasic1_hdfPersonID");
            if (hnPersonId != null)
            {
                strPersonalID = String.Copy(hnPersonId.Attributes["value"].Value);
                Log("strPersonalID: " + strPersonalID);
            }
            else
                return 2;
            #endregion

            #region Step2. GET 新增事件紀錄 response html content
            tagUrl = string.Copy(MINOSS_URL + "/LogBook/Maintain/EventAdd.aspx?siteID=" + OCS_SITE_ID);
            client.Headers.Clear();
            client.Headers.Add("Accept", "*/*");
            client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E; CMNTDFJS; InfoPath.3)");
            status = clientDownloadString(client, tagUrl, ref response);
            if (status != 0) { webBrowser1.DocumentText = response; return 2; }
            doc.LoadHtml(response);
            doc.Save(LOG_DIR + "GET_Event.html");
            #endregion

            #region Step3-1. POST事件類別
            //Construct basic POST form data
            nvcPostData.Clear();
            nvcPostData = new NameValueCollection
                    {
                        { "ctl00$ContentPlaceHolder1$ScriptManager1", "ctl00$ContentPlaceHolder1$UpdatePanel1|ctl00$ContentPlaceHolder1$ddlServiceName" },
                        { "__EVENTTARGET", "ctl00$ContentPlaceHolder1$ddlServiceName" },
                        { "__EVENTARGUMENT", "" },//hidden field 
                        { "__LASTFOCUS", "" },//hidden field 
                        { "_contentChanged_ctl00_ContentPlaceHolder1_Editor2_ctl02", "" },//hidden field 
                        { "_contentForce_ctl00_ContentPlaceHolder1_Editor2_ctl02", "" },//hidden field 
                        { "_content_ctl00_ContentPlaceHolder1_Editor2_ctl02", "" },//hidden field 
                        { "_activeMode_ctl00_ContentPlaceHolder1_Editor2_ctl02", "" },//hidden field 
                        { "__VIEWSTATE", "" },//hidden field 
                        { "__VIEWSTATEGENERATOR", "" },//hidden field 
                        { "__EVENTVALIDATION", "" },//hidden field 
                        { "ctl00$ContentPlaceHolder1$ddlServiceName","14" },
                        { "ctl00$ContentPlaceHolder1$ddlWorkTypeName","0" },
                        { "ctl00$ContentPlaceHolder1$Editor2$ctl04$ctl01$dummy", "" },
                        { "ctl00$ContentPlaceHolder1$EmployeeMenu1$ddl_SMS_Maintainer","5050" },
                        { "ctl00$ContentPlaceHolder1$EmployeeMenu1$ddl_SMS_Maintainer1","5953" },
                        { "ctl00$ContentPlaceHolder1$EmployeeMenu1$ddl_SMS_Maintainer2","d72" },
                        { "ctl00$ContentPlaceHolder1$EmployeeMenu1$ddl_SMS_Maintainer3","j05" },
                        { "ctl00$ContentPlaceHolder1$EmployeeMenu1$hdfImportGroupID1","" },//Ignore
                        { "ctl00$ContentPlaceHolder1$EmployeeMenu1$hdfReturnMeunID1","" },//Ignore
                        { "ctl00$ContentPlaceHolder1$txbResults","" },
                        { "ctl00$ContentPlaceHolder1$txbMemo","" },
                        { "ctl00$ContentPlaceHolder1$TB_Date1",string.Format("{0:yyyy/MM/dd}", this.dtEventStartTime.Value) },
                        { "ctl00$ContentPlaceHolder1$ddlBeginTimeHour","" },
                        { "ctl00$ContentPlaceHolder1$ddlBeginTimeMin","" },
                        { "ctl00$ContentPlaceHolder1$TB_Date2",string.Format("{0:yyyy/MM/dd}", this.dtEventEndTime.Value) },
                        { "ctl00$ContentPlaceHolder1$ddlEndTimeHour","" },
                        { "ctl00$ContentPlaceHolder1$ddlEndTimeMin","" },
                        { "ctl00$ContentPlaceHolder1$hdfSiteID",OCS_SITE_ID.ToString() },//Ignore
                        { "ctl00$ContentPlaceHolder1$hdfPersonID","" },//Ignore
                        { "__ASYNCPOST", "true"},
                        { "", ""},
                    };
            parseHtmlHiddenNameVal(doc, ref nvcPostData);

            //重設定HTTP Header
            client.Headers.Clear();
            client.Headers.Add("Accept-Language", "zh-TW");
            client.Headers.Add("Accept-Encoding", "gzip, deflate");
            client.Headers.Add("Accept", "*//*");
            client.Headers.Add("Referer", tagUrl);
            client.Headers.Add("Cache-Control", "no-cache");
            client.Headers.Add("DNT", "1");
            client.Headers.Add("x-requested-with", "XMLHttpRequest");
            client.Headers.Add("x-microsoftajax", "Delta=true");

            strQueryStr = String.Join("&", nvcPostData.AllKeys.Select(key => string.Format("{0}={1}", UrlEncode(key), UrlEncode(nvcPostData[key]))));
            strQueryStr = strQueryStr.Remove(strQueryStr.Length - 1);
            client.Headers.Set("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E; CMNTDFJS; InfoPath.3)");
            status = clientUploadString(client, tagUrl, strQueryStr, ref response);
            //status = clientUploadValues(client, tagUrl, nvcPostData, ref response);
            if (status != 0) { webBrowser1.DocumentText = response; return 2; }
            File.Delete(LOG_DIR + "POST_EventType.html");
            File.AppendAllText(LOG_DIR + "POST_EventType.html", response);
            #endregion

            #region Step3-2. POST 人員清單
            //Construct basic POST form data
            // Parsing submit欄位 from response
            string strSubmitVal = null;
            string strSubmitName = "ctl00$ContentPlaceHolder1$EmployeeMenu1$btn_Sin_SMS_Ins";
            HtmlNode hnNode;
            hnNode = doc.DocumentNode.SelectSingleNode("//input[@name='" + strSubmitName + "']/@value");
            if (hnNode != null)
            {
                strSubmitVal = string.Copy(hnNode.Attributes["value"].Value);
                byte[] BIG5bytes = Encoding.BigEndianUnicode.GetBytes(strSubmitVal);
            }

            nvcPostData.Clear();
            nvcPostData = new NameValueCollection
                    {
                        { "ctl00$ContentPlaceHolder1$ScriptManager1", "ctl00$ContentPlaceHolder1$UpdatePanel3|ctl00$ContentPlaceHolder1$EmployeeMenu1$btn_Sin_SMS_Ins" },
                        { "ctl00$ContentPlaceHolder1$ddlServiceName","14" },
                        { "ctl00$ContentPlaceHolder1$ddlWorkTypeName", "110" },
                        { "ctl00$ContentPlaceHolder1$Editor2$ctl04$ctl01$dummy", "" },
                        { "ctl00$ContentPlaceHolder1$EmployeeMenu1$ddl_SMS_Maintainer","5050" },
                        { "ctl00$ContentPlaceHolder1$EmployeeMenu1$ddl_SMS_Maintainer1","5953" },
                        { "ctl00$ContentPlaceHolder1$EmployeeMenu1$ddl_SMS_Maintainer2","d72" },
                        { "ctl00$ContentPlaceHolder1$EmployeeMenu1$ddl_SMS_Maintainer3","j05" },
                        { "ctl00$ContentPlaceHolder1$EmployeeMenu1$lst_SMS_Name_List_Init",strPersonalID },
                        { "ctl00$ContentPlaceHolder1$EmployeeMenu1$hdfImportGroupID1","" },//Ignore
                        { "ctl00$ContentPlaceHolder1$EmployeeMenu1$hdfReturnMeunID1","" },//Ignore
                        { "ctl00$ContentPlaceHolder1$txbResults","test2" },
                        { "ctl00$ContentPlaceHolder1$txbMemo","" },
                        { "ctl00$ContentPlaceHolder1$TB_Date1",string.Format("{0:yyyy/MM/dd}", this.dtEventStartTime.Value)},
                        { "ctl00$ContentPlaceHolder1$ddlBeginTimeHour","" },
                        { "ctl00$ContentPlaceHolder1$ddlBeginTimeMin","" },
                        { "ctl00$ContentPlaceHolder1$TB_Date2",string.Format("{0:yyyy/MM/dd}", this.dtEventEndTime.Value)},
                        { "ctl00$ContentPlaceHolder1$ddlEndTimeHour","" },
                        { "ctl00$ContentPlaceHolder1$ddlEndTimeMin","" },
                        { "ctl00$ContentPlaceHolder1$hdfSiteID",OCS_SITE_ID.ToString()},//Ignore
                        { "ctl00$ContentPlaceHolder1$hdfPersonID", "" },//Ignore
                        { "__EVENTTARGET", "" },
                        { "__EVENTARGUMENT", "" },
                        { "__LASTFOCUS", "" },
                        { "__VIEWSTATE", "" },
                        { "__VIEWSTATEGENERATOR", "" },
                        { "__EVENTVALIDATION", "" },
                        { "_contentChanged_ctl00_ContentPlaceHolder1_Editor2_ctl02", "" },
                        { "_contentForce_ctl00_ContentPlaceHolder1_Editor2_ctl02", "" },
                        { "_content_ctl00_ContentPlaceHolder1_Editor2_ctl02", "test1" },
                        { "_activeMode_ctl00_ContentPlaceHolder1_Editor2_ctl02", "" },
                        { "__ASYNCPOST", "true" },
                        { "ctl00$ContentPlaceHolder1$EmployeeMenu1$btn_Sin_SMS_Ins", strSubmitVal },
                    };
            parseHtmlHiddenNameVal(doc, ref nvcPostData);
            parseMinossHtmlHiddenFields(response, ref nvcPostData);

            //重新建構HTTP Header
            client.Headers.Clear();
            client.Headers.Add("Accept-Language", "zh-TW");
            client.Headers.Add("Accept-Encoding", "gzip, deflate");
            client.Headers.Add("Accept", "*//*");
            client.Headers.Add("Referer", tagUrl);
            client.Headers.Add("Cache-Control", "no-cache");
            client.Headers.Add("DNT", "1");
            client.Headers.Add("x-requested-with", "XMLHttpRequest");
            client.Headers.Add("x-microsoftajax", "Delta=true");

            strQueryStr = String.Join("&", nvcPostData.AllKeys.Select(key => string.Format("{0}={1}", UrlEncode(key), UrlEncode(nvcPostData[key]))));
            client.Headers.Set("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E)");
            status = clientUploadString(client, tagUrl, strQueryStr, ref response);
            if (status != 0) { webBrowser1.DocumentText = response; return 2; }
            File.Delete(LOG_DIR + "POST_EventSMSMember.html");
            File.AppendAllText(LOG_DIR + "POST_EventSMSMember.html", response);
            #endregion

            #region Step3-3. 送出Event的資料
            //Construct basic POST form data
            nvcPostData = new NameValueCollection { };
            nvcPostData = new NameValueCollection
                    {
                        {"ctl00$ContentPlaceHolder1$ddlServiceName","14"},
                        {"ctl00$ContentPlaceHolder1$ddlWorkTypeName","110"},
                        {"ctl00$ContentPlaceHolder1$Editor2$ctl04$ctl01$dummy",""},//Ignore
                        {"ctl00$ContentPlaceHolder1$fulContent1",""},
                        {"ctl00$ContentPlaceHolder1$fulContent2",""},
                        {"ctl00$ContentPlaceHolder1$fulContent3",""},
                        {"ctl00$ContentPlaceHolder1$EmployeeMenu1$ddl_SMS_Maintainer","5050"},
                        {"ctl00$ContentPlaceHolder1$EmployeeMenu1$ddl_SMS_Maintainer1","5953"},
                        {"ctl00$ContentPlaceHolder1$EmployeeMenu1$ddl_SMS_Maintainer2","d72"},
                        {"ctl00$ContentPlaceHolder1$EmployeeMenu1$ddl_SMS_Maintainer3","j05"},
                        {"ctl00$ContentPlaceHolder1$EmployeeMenu1$hdfImportGroupID1",""},//Ignore
                        {"ctl00$ContentPlaceHolder1$EmployeeMenu1$hdfReturnMeunID1",""},//Ignore
                        {"ctl00$ContentPlaceHolder1$txbResults",this.eventContent.Text},
                        {"ctl00$ContentPlaceHolder1$txbMemo",""},
                        {"ctl00$ContentPlaceHolder1$TB_Date1",string.Format("{0:yyyy/MM/dd}", this.dtEventStartTime.Value)},
                        {"ctl00$ContentPlaceHolder1$ddlBeginTimeHour",string.Format("{0:HH}", this.dtEventStartTime.Value)},
                        {"ctl00$ContentPlaceHolder1$ddlBeginTimeMin",string.Format("{0:mm}", this.dtEventStartTime.Value)},
                        {"ctl00$ContentPlaceHolder1$TB_Date2",string.Format("{0:yyyy/MM/dd}", this.dtEventEndTime.Value)},
                        {"ctl00$ContentPlaceHolder1$ddlEndTimeHour",string.Format("{0:HH}", this.dtEventEndTime.Value)},
                        {"ctl00$ContentPlaceHolder1$ddlEndTimeMin",string.Format("{0:mm}", this.dtEventEndTime.Value)},
                        {"ctl00$ContentPlaceHolder1$btn_Event_Update","結案"},
                        {"ctl00$ContentPlaceHolder1$hdfSiteID",OCS_SITE_ID.ToString()},//Ignore
                        {"ctl00$ContentPlaceHolder1$hdfPersonID",""},//Ignore
                        {"__EVENTTARGET", ""},
                        {"__EVENTARGUMENT", ""},
                        {"__LASTFOCUS",""},
                        {"__VIEWSTATE",""},
                        {"__VIEWSTATEGENERATOR",""},
                        {"__EVENTVALIDATION",""},
                        {"_contentChanged_ctl00_ContentPlaceHolder1_Editor2_ctl02",""},
                        {"_contentForce_ctl00_ContentPlaceHolder1_Editor2_ctl02",""},
                        {"_content_ctl00_ContentPlaceHolder1_Editor2_ctl02",this.eventTitle.Text},
                        {"_activeMode_ctl00_ContentPlaceHolder1_Editor2_ctl02",""},
                    };
            parseHtmlHiddenNameVal(doc, ref nvcPostData);
            parseMinossHtmlHiddenFields(response, ref nvcPostData);

            // Convert NameValueCollection array to byte array
            foreach (string key in nvcPostData)
            {
                var value = nvcPostData[key];
                string postStr = string.Empty;
                if (key.IndexOf("fulContent") != -1)
                    postStr = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"\"\r\nContent-Type: application/octet-stream\r\n\r\n{2}\r\n",
                        formDataBoundary, key, value);
                else
                    postStr = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n",
                        formDataBoundary, key, value);
                formDataStream.Write(Encoding.UTF8.GetBytes(postStr), 0, Encoding.UTF8.GetByteCount(postStr));
            }

            // Add the end of the request.  Start with a newline
            string footer = "--" + formDataBoundary + "--\r\n";
            formDataStream.Write(Encoding.UTF8.GetBytes(footer), 0, Encoding.UTF8.GetByteCount(footer));

            // Dump the Stream into a byte[]
            formDataStream.Position = 0;
            byte[] formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();

            // Redefine header information
            client.Headers.Clear();
            client.Headers.Add("Accept", "application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, */*");
            client.Headers.Add("Referer", tagUrl);
            client.Headers.Add("Accept-Language", "zh-TW");
            client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/7.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E; CMNTDFJS; InfoPath.3)");
            client.Headers.Add("Content-Type", "multipart/form-data; boundary=" + formDataBoundary);
            client.Headers.Add("Accept-Encoding", "gzip, deflate");
            client.Headers.Add("DNT", "1");

            // POST site maintain item
            status = clientUploadData(client, tagUrl, formData, ref response);
            if (status != 0) { webBrowser1.DocumentText = response; MessageBox.Show("取得維運日誌網頁資料錯誤"); return 2; }
            File.Delete(LOG_DIR + "POST_EventContent.html");
            File.AppendAllText(LOG_DIR + "POST_EventContent.html", response);
            #endregion

            #region Step4. GET Main.aspx(維運日誌網頁資料)
            tagUrl = string.Copy(MINOSS_URL + "/LogBook/Maintain/Main.aspx");
            status = clientDownloadString(client, tagUrl, ref response);
            if (status != 0) { webBrowser1.DocumentText = response; MessageBox.Show("取得維運日誌網頁資料錯誤"); return 2; }
            File.Delete(LOG_DIR + "GET_MINOSS_MAIN.html");
            File.AppendAllText(LOG_DIR + "GET_MINOSS_MAIN.html", response);
            #endregion

            //Show result
            webBrowser1.DocumentText = response; 
            return status;
        }

        /// <summary>
        /// 主要完成自動填寫MINOSS工作日誌的功能
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private int procMinossJobs(WebClientEx client)
        {
            // Setup Client parameter
            client.Encoding = Encoding.UTF8;
            int status = 0;
            string tagUrl = string.Empty;
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            NameValueCollection nvcPostData = new NameValueCollection();

            #region Step1. POST Main.aspx(Click 維運日誌)
            tagUrl = MINOSS_URL + "/System_Maintenance/HttpHandler/RecordFunctionLog.ashx";
            var data6 = new NameValueCollection { { "path", "LogBook/Maintain/Main.aspx" }, };
            status = clientUploadValues(client, tagUrl, data6, ref response);
            if (status != 0) { webBrowser1.DocumentText = response; return 2; }
            File.Delete(LOG_DIR + "POST_MINOSS_MAIN.html");
            File.AppendAllText(LOG_DIR + "POST_MINOSS_MAIN.html", response);
            #endregion

            #region Step2-1. GET Main.aspx(維運日誌網頁資料)
            if(nmossAuth.bIsHistory)
                tagUrl = MINOSS_URL + "/LogBook/Maintain/MainHistory.aspx";
            else
                tagUrl = MINOSS_URL + "/LogBook/Maintain/Main.aspx";
            CHT_AM_Headers(client);
            client.Headers.Set("Referer", tagUrl);
            status = clientDownloadString(client, tagUrl, ref response);
            if (status != 0) { webBrowser1.DocumentText = response; MessageBox.Show("取得維運日誌網頁資料錯誤"); return 2; }
            var logBook = new HtmlAgilityPack.HtmlDocument();
            logBook.LoadHtml(response);
            File.Delete(LOG_DIR + "GET_MINOSS_MAIN.html");
            File.AppendAllText(LOG_DIR + "GET_MINOSS_MAIN.html", response);
            #endregion

            #region Step2-2. 如果是歷史維運日誌，則要選擇日期(歷史維運日誌資料)
            if (nmossAuth.bIsHistory)
            {
                tagUrl = MINOSS_URL + "/LogBook/Maintain/MainHistory.aspx";
                nvcPostData.Clear();
                nvcPostData = new NameValueCollection {};
                parseHtmlHiddenNameVal(logBook, ref nvcPostData);
                nvcPostData.Add("ctl00$cb_menu_state", "on");
                nvcPostData.Add("ctl00$cb_logo_state", "on");
                nvcPostData.Add("ctl00$ContentPlaceHolder1$TB_Date1",this.Submit_MINOSS_Info.Text);
                nvcPostData.Add("ctl00$ContentPlaceHolder1$ddlSite", OCS_SITE_ID.ToString());
                nvcPostData.Add("ctl00$ContentPlaceHolder1$btnSite", "選擇");
                // Setup header information
                CHT_AM_Headers(client);
                client.Headers.Set("Referer", tagUrl);
                status = clientUploadValues(client, tagUrl, nvcPostData, ref response);
                if (status != 0) { webBrowser1.DocumentText = response; return 2; }
                logBook = new HtmlAgilityPack.HtmlDocument();
                logBook.LoadHtml(response);
                File.Delete(LOG_DIR + "GET_MINOSS_MAIN2.html");
                File.AppendAllText(LOG_DIR + "GET_MINOSS_MAIN2.html", response);
            }
            #endregion

            #region Step3. POST minoss logbook 設備檢查
            // Callback procLogBook function to POST all log book request
            status = procLogBook_devCheck(client, logBook);
            if (status != 0) MessageBox.Show("填寫MINOSS工作日誌-設備檢查 錯誤");
            #endregion

            #region Step4. POST minoss logbook 機房維運
            status = procLogBook_SiteMaintain(client, logBook);
            if (status != 0) MessageBox.Show("填寫MINOSS工作日誌-機房維運 錯誤");
            #endregion

            /// 因為填寫過去的日誌需要用POST，考量到實用性，所以過去日誌就不顯示在windowBrowser
            #region Step5. 如果是填寫當天日誌會取得最後一次檢查結果並顯示在windowBrowser
            if (!nmossAuth.bIsHistory)
            {
                tagUrl = MINOSS_URL + "/LogBook/Maintain/Main.aspx";
                status = clientDownloadString(client, tagUrl, ref response);
                if (status != 0) { webBrowser1.DocumentText = response; MessageBox.Show("最後取得維運日誌網頁資料錯誤"); return 2; }
                webBrowser1.DocumentText = response;
            }
            #endregion

            #region Debug
            // 印出POST回傳的資料
            //webBrowser1.DocumentText = Encoding.Default.GetString(response8);
            // 印出Get回傳的資料
            //webBrowser1.DocumentText = response8;
            // 將Response存成檔案response.html
            //File.WriteAllText("response.html", Encoding.Default.GetString(response8), Encoding.Default);
            File.Delete(LOG_DIR + "response.html");
            File.AppendAllText(LOG_DIR + "response.html", response, Encoding.Default);
            #endregion
            MessageBox.Show("上傳資料完畢!!!"); 
            return 0;
        }

        ///////////////////////////////////////////// SSO 3 Next Certification Systen /////////////////////////////////////////////

        /// <summary>
        /// Process SSO previous certification, It is important that parse parameters(ex: ID) from response
        /// </summary>
        /// <param name="sso3"></param>
        /// <returns></returns>
        private int SSO3_login_portal(ref Site_Authority.SSO3CertStruc sso3)
        {
            int status = 0;
            string tagUrl = string.Empty;
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            HtmlNode hnNode;
            string response = null,responseUrl = null;
            NameValueCollection nvcPostData = null;

            #region  Step1. Get nmoss login url and cookie
            CHT_AM_Headers(client);
            tagUrl = string.Copy(OCS_GUI.Properties.Settings.Default.GET_OTP);
            status = clientDownloadString(client, tagUrl, ref response);
            responseUrl = string.Copy(client.ResponseUri.ToString());
            if (status != 0) { webBrowser1.DocumentText = response; MessageBox.Show("登入失敗, 請重新登入"); return -1; }
            File.Delete(LOG_DIR + "OPT_GET1.html");
            File.AppendAllText(LOG_DIR + "OPT_GET1.html", response, Encoding.Default);
            #endregion

            #region Step2. Parse _afrLoop parameter
            string afrLoop = null;
            string adf_Window_Id = null;
            using (StreamReader str = new StreamReader(LOG_DIR + "OPT_GET1.html", Encoding.Default))
            {
                String line;
                while (!str.EndOfStream)
                {
                    line = str.ReadLine();
                    if (line.IndexOf("'_afrLoop',", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        line = str.ReadLine();
                        string[] line_substr = line.Split('\'');
                        afrLoop = string.Copy(line_substr[1]);
                    }
                    if (line.IndexOf("'Adf-Window-Id',", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        line = str.ReadLine();
                        line = str.ReadLine();
                        line = str.ReadLine();
                        string[] line_substr = line.Split('\'');
                        adf_Window_Id = string.Copy(line_substr[1]);
                    }
                }
                str.Close();
            }
            #endregion

            #region Step3. Loop resend portal request
            CHT_AM_Headers(client);
            client.Headers.Set("Referer", responseUrl);
            tagUrl = string.Copy(responseUrl + "&_afrLoop=" + afrLoop + "&_afrWindowMode=0&Adf-Window-Id=" + adf_Window_Id + "&_afrFS=16&_afrMT=screen&_afrMFW=1280&_afrMFH=617&_afrMFDW=1280&_afrMFDH=720&_afrMFC=8&_afrMFCI=0&_afrMFM=0&_afrMFR=144&_afrMFG=0&_afrMFS=0&_afrMFO=0");
            status = clientDownloadString(client, tagUrl, ref response);
            if (status != 0) { webBrowser1.DocumentText = response; MessageBox.Show("登入失敗, 請重新登入"); return -1; }
            File.Delete(LOG_DIR + "OPT_GET2.html");
            File.AppendAllText(LOG_DIR + "OPT_GET2.html", response, Encoding.Default);
            #endregion
            
            #region Step4. Parse and construct SSO certification info
            doc.LoadHtml(response);
            TextBox name = this.Controls.Find("name", true).FirstOrDefault() as TextBox;
            TextBox pwd = this.Controls.Find("pwd", true).FirstOrDefault() as TextBox;
            TextBox otp = this.Controls.Find("otp", true).FirstOrDefault() as TextBox;
            sso3.it1_otplogin = string.Copy(name.Text); //Account
            sso3.it2_otplogin = string.Copy(pwd.Text); //Password
            sso3.it3_otplogin = string.Copy(otp.Text); //otp
            hnNode = doc.DocumentNode.SelectSingleNode("//input[@name='javax.faces.ViewState']/@value");
            if (hnNode != null) sso3.faces_ViewState = string.Copy(hnNode.Attributes["value"].Value);            
            hnNode = doc.DocumentNode.SelectSingleNode("//input[@name='org.apache.myfaces.trinidad.faces.FORM']/@value");
            if (hnNode != null) sso3.faces_FORM = string.Copy(hnNode.Attributes["value"].Value);
            sso3.windows_id = string.Copy(adf_Window_Id);
            hnNode = doc.GetElementbyId("f1");
            if (hnNode != null) sso3.FORM_action = string.Copy(hnNode.Attributes["action"].Value);
            Debug.WriteLine(sso3.FORM_action);
            sso3.responseUrl = string.Copy(responseUrl);
            #endregion

            #region Step5. Send GET request to IsValidToken.jsp
            CHT_AM_Headers(client);
            client.Headers.Set("Referer", responseUrl);
            tagUrl = string.Copy("https://am.cht.com.tw/asdk/IsValidToken.jsp");
            status = clientDownloadString(client, tagUrl, ref response);
            responseUrl = string.Copy(client.ResponseUri.ToString());
            if (status != 0) { webBrowser1.DocumentText = response; MessageBox.Show("登入失敗, 請重新登入"); return -1; }
            File.Delete(LOG_DIR + "OPT_GET2.html");
            File.AppendAllText(LOG_DIR + "OPT_GET2.html", response, Encoding.Default);
            #endregion

            #region Step6. First CHTOTP loop POST
            CHT_AM_Headers(client);
            client.Headers.Set("Accept", "*/*");
            client.Headers.Set("Adf-Ads-Page-Id", "2");
            client.Headers.Set("Adf-Rich-Message", "true");
            client.Headers.Set("Referer", responseUrl);

            nvcPostData = new NameValueCollection
                {
                    { "pt1:r1:0:it1_otplogin", sso3.it1_otplogin},
                    { "pt1:r1:0:it2_otplogin", ""},
                    { "pt1:r1:0:it3_otplogin", "" },
                    { "org.apache.myfaces.trinidad.faces.FORM", sso3.faces_FORM },
                    { "Adf-Window-Id", sso3.windows_id },
                    { "javax.faces.ViewState", sso3.faces_ViewState },
                    { "Adf-Page-Id", "1" },
                    { "event", "pt1:r1:0:it1_otplogin" },
                    { "event.pt1:r1:0:it1_otplogin", "<m xmlns=\"http://oracle.com/richClient/comm\"><k v=\"_custom\"><b>1</b></k><k v=\"isStepUp\"><s>false</s></k><k v=\"ldapLoginEmpNo\"><s></s></k><k v=\"immediate\"><b>1</b></k><k v=\"type\"><s>synStepUpInfo</s></k></m>" },
                    { "oracle.adf.view.rich.PROCESS", "pt1:r1:0:it1_otplogin" },
                };
            tagUrl = string.Copy(AM_URL + sso3.FORM_action + "?Adf-WindowId=" + sso3.windows_id + "&Adf-Page-Id=1");
            response = HttpServerUtility.UrlTokenEncode(client.UploadValues(tagUrl, nvcPostData));
            File.Delete(LOG_DIR + "OPT_LOOP1.html");
            File.AppendAllText(LOG_DIR + "OPT_LOOP1.html", response, Encoding.Default);
            #endregion
            return 0;
        }

        /// <summary>
        /// Login SSO3 depend account,password,OTP
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void submit_Click(object sender, EventArgs e)
        {
            int status = 0;
            CookieContainer cookie = new CookieContainer();
            using (client = new WebClientEx(cookie))
            {
                client.Encoding = Encoding.UTF8;
                string tagUrl = string.Empty,strXPath = string.Empty;
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                HtmlNode hnNode;
                NameValueCollection nvcPostData = new NameValueCollection();
                Site_Authority.SSO3CertStruc sso3 = new Site_Authority.SSO3CertStruc();

                #region Step1. Call SSO3 get portal and construct POST data
                if (SSO3_login_portal(ref sso3) > 0)
                    return;
                #endregion

                #region Step2. Post data to login SSO3
                CHT_AM_Headers(client);
                client.Headers.Set("Accept","*/*");
                client.Headers.Set("Referer", sso3.responseUrl);
                client.Headers.Set("Adf-Rich-Message", "true");
                client.Headers.Set("Adf-Ads-Page-Id", "2");
                var sso3_data = new NameValueCollection
                {
                    { "pt1:r1:0:it1_otplogin", sso3.it1_otplogin},
                    { "pt1:r1:0:it2_otplogin", sso3.it2_otplogin},
                    { "pt1:r1:0:it3_otplogin", sso3.it3_otplogin },
                    { "org.apache.myfaces.trinidad.faces.FORM", sso3.faces_FORM },
                    { "Adf-Window-Id", sso3.windows_id },
                    { "Adf-Page-Id", "1" },
                    { "javax.faces.ViewState", sso3.faces_ViewState },
                    { "oracle.adf.view.rich.RENDER", "pt1:r1" },
                    { "event", "pt1:r1:0:cb2_otplogin" },
                    { "event.pt1:r1:0:cb2_otplogin", "<m xmlns=\"http://oracle.com/richClient/comm\"><k v=\"type\"><s>action</s></k></m>" },
                    { "oracle.adf.view.rich.PROCESS", "pt1:r1,pt1:r1:0:cb2_otplogin" },
                };
                tagUrl = string.Copy(AM_URL + sso3.FORM_action);
                status = clientUploadValues(client, tagUrl, sso3_data, ref response);
                if (status != 0) { webBrowser1.DocumentText = response; MessageBox.Show("登入失敗, 請重新登入"); return; }
                File.Delete(LOG_DIR + "POST_Login1.html");
                File.AppendAllText(LOG_DIR + "POST_Login1.html", response, Encoding.Default);
                #endregion

                #region Step3-1. Parse POST data from POST_Login1.html
                string strUserInfoInput = null;
                doc.LoadHtml(response);
                strXPath = "//eval[3]";
                hnNode = doc.DocumentNode.SelectSingleNode(strXPath);
                if (hnNode != null)
                {
                    string[] strArray = hnNode.InnerText.Split(';');
                    foreach (string strKV in strArray)
                    if (strKV.IndexOf("userInfoInput.value", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        string[] itemArr;
                        itemArr = strKV.Replace(" ", "").Replace("\"", "").Split('=');
                        if (itemArr.Length > 1)
                            strUserInfoInput = string.Copy(itemArr[1]);
                    }
                }
                #endregion

                #region Step3-2. POST auth_cred_submit
                tagUrl = string.Copy(OCS_GUI.Properties.Settings.Default.auth_cred_submit);
                // Setup header information
                client.Headers.Remove("Adf-Rich-Message");
                CHT_AM_Headers(client);
                client.Headers.Set("Referer", sso3.responseUrl);
                nvcPostData = new NameValueCollection
                    {
                        { "username", sso3.it1_otplogin },
                        { "CustomAuthNModule", "CHTOTP" },
                        { "CardType", "cardType" },
                        { "UserInfo", strUserInfoInput },
                    };
                status = clientUploadValues(client, tagUrl, nvcPostData, ref response);
                sso3.responseUrl = string.Copy(client.ResponseUri.ToString());
                if (status != 0) { webBrowser1.DocumentText = response; MessageBox.Show("登入失敗, 請重新登入"); return; }
                File.Delete(LOG_DIR + "POST_auth_cred_submit.html");
                File.AppendAllText(LOG_DIR + "POST_auth_cred_submit.html", response);
                #endregion

                #region Step4-1 Parse _afrLoop parameter from POST_auth_cred_submit.html
                string afrLoop = null;
                using (StreamReader str = new StreamReader(LOG_DIR + "POST_auth_cred_submit.html", Encoding.Default))
                {
                    String line;
                    while (!str.EndOfStream)
                    {
                        line = str.ReadLine();
                        if (line.IndexOf("\"_afrLoop\"", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            string[] line_substr = line.Split(',');
                            afrLoop = string.Copy(line_substr[2].Replace(" ", "").Replace("\"", "").Replace(")", "").Replace(";", ""));
                            break;
                        }
                    }
                    str.Close();
                }
                #endregion

                #region Step4-2 POST data to CHTWelcome
                CHT_AM_Headers(client);
                tagUrl = string.Copy(sso3.responseUrl + "&_afrLoop=" + afrLoop + "&_afrWindowMode=0&_afrWindowId=160ptsmq14_1");
                status = clientDownloadString(client, tagUrl, ref response);
                sso3.responseUrl = string.Copy(client.ResponseUri.ToString());
                Debug.WriteLine(sso3.responseUrl);
                if (status != 0) { webBrowser1.DocumentText = response; MessageBox.Show("登入失敗, 請重新登入"); return; }
                File.Delete(LOG_DIR + "POST_CHTWelcome.html");
                File.AppendAllText(LOG_DIR + "POST_CHTWelcome.html", response, Encoding.Default);
                #endregion

                #region Step5-1 Parse data from CHTWelcome response
                doc.LoadHtml(response);
                hnNode = doc.DocumentNode.SelectSingleNode("//input[@name='javax.faces.ViewState']/@value");
                if (hnNode != null) sso3.faces_ViewState = string.Copy(hnNode.Attributes["value"].Value);
                hnNode = doc.DocumentNode.SelectSingleNode("//input[@name='org.apache.myfaces.trinidad.faces.FORM']/@value");
                if (hnNode != null) sso3.faces_FORM = string.Copy(hnNode.Attributes["value"].Value);
                hnNode = doc.GetElementbyId("f1");
                if (hnNode != null) sso3.FORM_action = string.Copy(hnNode.Attributes["action"].Value);
                #endregion

                #region Step5-2. Post data to login chtWelcome
                CHT_AM_Headers(client);
                client.Headers.Set("Accept", "*/*");
                client.Headers.Set("Referer", sso3.responseUrl);
                client.Headers.Set("Adf-Rich-Message", "true");
                client.Headers.Set("Adf-Property-Delta-Sync", "true");
                var chtWelcome_data = new NameValueCollection
                {
                    { "org.apache.myfaces.trinidad.faces.FORM", sso3.faces_FORM },
                    { "javax.faces.ViewState", sso3.faces_ViewState },
                };
                tagUrl = string.Copy(AM_URL + sso3.FORM_action);
                status = clientUploadValues(client, tagUrl, chtWelcome_data, ref response);
                if (status != 0) { webBrowser1.DocumentText = response; MessageBox.Show("登入失敗, 請重新登入"); return; }
                File.Delete(LOG_DIR + "POST_Login2.html");
                File.AppendAllText(LOG_DIR + "POST_Login2.html", response, Encoding.Default);
                #endregion

                #region Step6. Get NMOSS portal page
                client.Headers.Remove("Adf-Rich-Message");
                client.Headers.Remove("Adf-Property-Delta-Sync");
                tagUrl = string.Copy(NMOSS_URL+"/Login.aspx");            
                status = clientDownloadString(client, tagUrl, ref response);
                if (status != 0) { webBrowser1.DocumentText = response; return; }
                File.Delete(LOG_DIR + "GET_NMOSS_Login.html");
                File.AppendAllText(LOG_DIR + "GET_NMOSS_Login.html", response);
                #endregion

                #region Step5. Parsing nmoss Entry隱藏資料
                doc.LoadHtml(response);

                nmossAuth.strEventtarget = "";
                nmossAuth.strEventargument = "";

                HtmlNode hnViewstategenerator = doc.GetElementbyId("__VIEWSTATEGENERATOR");
                if (hnViewstategenerator != null)
                    nmossAuth.strViewstategenerator = String.Copy(hnViewstategenerator.Attributes["value"].Value);
                Log("viewstate: " + nmossAuth.strViewstategenerator);

                HtmlNode hnViewstate = doc.GetElementbyId("__VIEWSTATE");
                if (hnViewstate != null)
                    nmossAuth.strViewstate = String.Copy(hnViewstate.Attributes["value"].Value);
                Log("viewstate: " + nmossAuth.strViewstate);

                HtmlNode hnEventvalidation = doc.GetElementbyId("__EVENTVALIDATION");
                if (hnEventvalidation != null)
                    nmossAuth.strEventvalidation = String.Copy(hnEventvalidation.Attributes["value"].Value);
                Log("eventvalidation: " + nmossAuth.strEventvalidation);
                #endregion

                #region Step6. POST NMOSS Entry and connect to minoss(行動智網維運工具)
                tagUrl = NMOSS_URL + "/Entry.aspx";
                nvcPostData = new NameValueCollection
                {
                    { "__EVENTTARGET", nmossAuth.strEventtarget},
                    { "__EVENTARGUMENT", nmossAuth.strEventargument},
                    { "__VIEWSTATE", nmossAuth.strViewstate },
                    { "__VIEWSTATEGENERATOR", nmossAuth.strViewstategenerator },
                    { "__SCROLLPOSITIONX", "0" },
                    { "__SCROLLPOSITIONY", "0" },
                    { "__EVENTVALIDATION", nmossAuth.strEventvalidation },
                    { "imgcmdMin.x", "103" },
                    { "imgcmdMin.y", "25" },
                };
                status = clientUploadValues(client, tagUrl, nvcPostData, ref response);
                if (status != 0) { webBrowser1.DocumentText = response; MessageBox.Show("登入失敗, 請重新登入"); return; }
                File.Delete(LOG_DIR + "POST_NMOSS_Entry.html");
                File.AppendAllText(LOG_DIR + "POST_NMOSS_Entry.html", response);
                #endregion

                bIsLoginSSO = true;

                MessageBox.Show("登入成功!!");

                this.submit.Enabled = false;
            }
        }

        /// <summary>
        /// The function is get otp number from CHT SSO3 system
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getOTP_Click(object sender, EventArgs e)
        {
            CookieContainer cookie = new CookieContainer();
            using (client = new WebClientEx(cookie))
            {
                string tagUrl = string.Empty;
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                string response = null;
                Site_Authority.SSO3CertStruc sso3 = new Site_Authority.SSO3CertStruc();

                #region Step1. Call SSO3 get portal and construct POST data
                if (SSO3_login_portal(ref sso3) > 0)
                    return;
                #endregion

                #region Step2. Post data to get OPT
                var data = new NameValueCollection
                {
                    { "pt1:r1:0:it1_otplogin", sso3.it1_otplogin},
                    { "pt1:r1:0:it2_otplogin", sso3.it2_otplogin},
                    { "pt1:r1:0:it3_otplogin", "" },
                    { "org.apache.myfaces.trinidad.faces.FORM", sso3.faces_FORM },
                    { "Adf-Window-Id", sso3.windows_id },
                    { "Adf-Page-Id", "1" },
                    { "javax.faces.ViewState", sso3.faces_ViewState },
                    { "oracle.adf.view.rich.RENDER", "pt1:r1" },
                    { "event", "pt1:r1:0:cb1_otplogin" },
                    { "event.pt1:r1:0:cb1_otplogin", "<m xmlns=\"http://oracle.com/richClient/comm\"><k v=\"type\"><s>action</s></k></m>" },
                    { "oracle.adf.view.rich.PROCESS", "pt1:r1,pt1:r1:0:cb1_otplogin" },
                };
                tagUrl = string.Copy(AM_URL + sso3.FORM_action);
                try
                {
                    response = HttpServerUtility.UrlTokenEncode(client.UploadValues(tagUrl, data));
                    File.Delete(LOG_DIR + "OPT_POST.html");
                    File.AppendAllText(LOG_DIR + "OPT_POST.html", response, Encoding.Default);
                }
                catch (WebException ex)
                {
                    if (ex.Response != null)
                    {
                        HttpStatusCode statusCode = ((HttpWebResponse)ex.Response).StatusCode;
                        Log("An error occurred, status code: " + statusCode);
                    }
                    return;
                }
                #endregion
            }
        }

        ///////////////////////////////////////////// Sub Thread Program System /////////////////////////////////////////////
        /// <summary>
        /// 產生一個新的Thread, 並執行MINOSS結果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autoMINOSS(object sender, EventArgs e)
        {
            int ret = -1;
            if (!bIsLoginSSO)
            {
                MessageBox.Show("You NOT have login SSO.");
                return;
            }
            else
            {
                Thread minoss_thread = new Thread(() => { ret = procMinossJobs(client); });
                minoss_thread.Start();
                minoss_thread.Join();
                if (ret != 0)
                    MessageBox.Show("minoss_thread status:" + ret);
                else
                    MessageBox.Show("工作日誌填寫完成");
            }
        }

        private void autoMINOSSEvent(object sender, EventArgs e)
        {
            int ret = -1;
            if (!bIsLoginSSO)
            {
                MessageBox.Show("You NOT have login SSO.");
                return;
            }
            else
            {
                Thread minossEvent_thread = new Thread(() => { ret = procMinossEventUpload(client); });
                minossEvent_thread.Start();
                minossEvent_thread.Join();
                if (ret != 0)
                    MessageBox.Show("minoss_thread status:" + ret);
                else
                    MessageBox.Show("事件紀錄填寫完成");
            }
        }

        private void PanelVisible(string panelName)
        {
            foreach (var panel in this.Controls.OfType<Panel>())
            {
                Debug.WriteLine(panel.Name);
                if (string.Compare(panel.Name, panelName) == 0)
                    panel.Visible = true;
                else
                    panel.Visible = false;                
            }
        }

        /// <summary>
        /// 功能選單的開關
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void funcSelectBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int iSelectIdx = this.funcSelectBox.SelectedIndex;
            Debug.WriteLine("iSelectIdx: " + iSelectIdx);
            switch (iSelectIdx)
            {
                case 1:
                    PanelVisible("panel_autoMINOSS");
                    // 判斷是要填寫歷史維運日誌還是當天維運日至
                    DateTimePicker ctrlSelDateTimePicker = ((ParseResult)this.Tag).Controls.Find("dateTimePicker1", true).FirstOrDefault() as DateTimePicker;
                    if (ctrlSelDateTimePicker.Value.Date == DateTime.Now.Date)
                        nmossAuth.bIsHistory = false;
                    else
                        nmossAuth.bIsHistory = true;
                    this.Submit_MINOSS_Info.Text = ctrlSelDateTimePicker.Value.Date.ToString("yyyy/MM/dd");
                    #if DEBUG
                        this.Submit_MINOSS_Info.Visible = true;
                    #endif
                    break;
                case 2:
                    PanelVisible("panel_autoMINOSSEvent");
                    break;
            }
        }

        /// <summary>
        /// 讀取事件範本
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private int loadMinossEventTemplate(string file)
        {
            int ret = 0;
            string templatePath = "Template\\MINOSS\\";
            string strXPath = string.Empty;
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            HtmlNode node = null;

            doc.Load(templatePath + file);
            strXPath = "//select[@id='ContentPlaceHolder1_ddlServiceName']/option[@selected]/@value";
            node = doc.DocumentNode.SelectSingleNode(strXPath);
            if (node == null)
                return 2;
            this.eventSerivce.Text = node.Attributes["value"].Value;

            strXPath = "//select[@id='ContentPlaceHolder1_ddlWorkTypeName']/option[@selected]/@value";
            node = doc.DocumentNode.SelectSingleNode(strXPath);
            if (node == null)
                return 2;
            this.eventJob.Text = node.Attributes["value"].Value;

            strXPath = "//div/p[contains(@class, 'MsoNormal')]/span/textarea";
            node = doc.DocumentNode.SelectSingleNode(strXPath);
            if (node == null)
                return 2;
            this.eventTitle.Text = HttpUtility.HtmlDecode(node.InnerText);

            strXPath = "//textarea[@id='ContentPlaceHolder1_txbResult']";
            node = doc.DocumentNode.SelectSingleNode(strXPath);
            if (node == null)
                return 2;
            this.eventContent.Text = HttpUtility.HtmlDecode(node.InnerText);
            /*
            strXPath = "//textarea[@id='ContentPlaceHolder1_txbMemo']";
            node = doc.DocumentNode.SelectSingleNode(strXPath);
            if (node == null)
                Debug.WriteLine(HttpUtility.HtmlDecode(node.InnerText));
            */
            return ret;
        }

        /// <summary>
        /// 根據不同事件類型選擇不同的事件範本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_eventMenu_SelectedIndexChanged(object sender, EventArgs e)
        {
            int iSelectIdx = this.comboBox_eventMenu.SelectedIndex;
            int ret = 0;
            switch (iSelectIdx)
            {
                case 0:
                    ret = loadMinossEventTemplate("EventUpdate_0.htm");
                    break;
                case 1:
                    ret = loadMinossEventTemplate("EventUpdate_1.htm");
                    break;
                case 2:
                    ret = loadMinossEventTemplate("EventUpdate_2.htm");
                    break;
                case 3:
                    ret = loadMinossEventTemplate("EventUpdate_3.htm");
                    break;
                case 4:
                    ret = loadMinossEventTemplate("EventUpdate_4.htm");
                    break;
                case 5:
                    ret = loadMinossEventTemplate("EventUpdate_5.htm");
                    break;
                case 6:
                    ret = loadMinossEventTemplate("EventAddTemplateCustomer_OCS.htm");
                    break;
                case 7:
                    ret = loadMinossEventTemplate("EventAddTemplateCustomer_other.htm");
                    break;
            }
            if (ret != 0)
                MessageBox.Show("範本撰寫錯誤，請更改範本:" + this.funcSelectBox.Text);
        }
    }

    /// <summary>
    /// 儲存所有NMOSS三大子系統的認證資訊
    /// </summary>
    public class Site_Authority
    {
        public struct SSO3CertStruc
        {
            string __it1_otplogin;
            string __it2_otplogin;
            string __it3_otplogin;
            string __faces_FORM;
            string __windows_id;
            string __FORM_action;
            string __faces_ViewState;
            string __responseUrl;
            public string it1_otplogin
            {
                get { return __it1_otplogin; }
                set { __it1_otplogin = string.Copy(value); }
            }
            public string it2_otplogin
            {
                get { return __it2_otplogin; }
                set { __it2_otplogin = string.Copy(value); }
            }
            public string it3_otplogin
            {
                get { return __it3_otplogin; }
                set { __it3_otplogin = string.Copy(value); }
            }
            public string faces_FORM
            {
                get { return __faces_FORM; }
                set { __faces_FORM = string.Copy(value); }
            }
            public string windows_id
            {
                get { return __windows_id; }
                set { __windows_id = string.Copy(value); }
            }
            public string FORM_action
            {
                get { return __FORM_action; }
                set { __FORM_action = string.Copy(value); }
            }
            public string faces_ViewState
            {
                get { return __faces_ViewState; }
                set { __faces_ViewState = string.Copy(value); }
            }
            public string responseUrl
            {
                get { return __responseUrl; }
                set { __responseUrl = string.Copy(value); }
            }
        }
        public struct nmossAuthDataStruc
        {
            string __strEventtarget;
            string __strEventargument;
            string __strViewstategenerator;
            string __strViewstate;
            string __strEventvalidation;
            bool __bIsHistory;
            public string strEventtarget
            {
                get { return __strEventtarget; }
                set { __strEventtarget = string.Copy(value); }
            }
            public string strEventargument
            {
                get { return __strEventargument; }
                set { __strEventargument = string.Copy(value); }
            }
            public string strViewstategenerator
            {
                get { return __strViewstategenerator; }
                set { __strViewstategenerator = string.Copy(value); }
            }
            public string strViewstate
            {
                get { return __strViewstate; }
                set { __strViewstate = string.Copy(value); }
            }
            public string strEventvalidation
            {
                get { return __strEventvalidation; }
                set { __strEventvalidation = string.Copy(value); }
            }
            public bool bIsHistory
            {
                get { return __bIsHistory; }
                set { __bIsHistory = value; }
            }
        };
    }
}
