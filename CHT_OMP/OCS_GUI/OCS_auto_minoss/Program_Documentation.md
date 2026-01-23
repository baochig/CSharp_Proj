# 程式說明書

## 1. 專案概述
- **系統目的**：提供一個 Windows Forms 介面，透過 SSO3 登入後，自動填寫 MINOSS 維運日誌與事件紀錄，並可從介面上傳工作日誌或事件記錄內容。【需人工補充】（此目的係依程式碼中之行為推得，未見正式需求文件）
- **此模組在整體系統中的角色**：此模組位於 `OCS_GUI/OCS_auto_minoss`，負責自動化操作 MINOSS 的工作日誌/事件紀錄流程，並與外部 GUI（`OCS_Parser_Minoss_GUI.ParseResult`）互動以取得解析後的檢查結果與輸入內容。【需人工補充】（`ParseResult` 的來源與用途需由系統整體說明補充）

## 2. 目錄結構說明
- **檔案**
  - `Auto_logbook.cs`：主要功能實作，包含登入 SSO3、存取 NMOSS/MINOSS、填寫維運日誌與事件紀錄、HTML 解析、以及 UI 事件處理邏輯。
  - `Auto_logbook.designer.cs`：Windows Forms 設計檔，定義 UI 控制項（帳號、密碼、OTP、功能選單、事件/日誌面板、WebBrowser 等）。
  - `Auto_logbook.resx`：Form 的資源檔，目前僅包含 ResX 版本與讀寫器資訊。
  - `ChtSSO3.cs`：宣告空的 `ChtSSO3` 介面（目前未定義任何成員）。
  - `README.md`：空白檔案。
  - `Program_Documentation.md`：本文件。

## 3. 核心程式流程
- **主要執行流程**
  1. 使用者在 UI 中輸入 LDAP 帳密與 OTP，點擊「登入」觸發 `submit_Click`。
  2. `submit_Click` 透過 `SSO3_login_portal` 取得 SSO3 必要參數，並依序進行 SSO3 驗證、取得 NMOSS Entry 頁面所需隱藏欄位，再提交進入 MINOSS。
  3. 登入成功後，使用者可從下拉選單選擇功能：
     - **上傳工作日誌**：觸發 `autoMINOSS`，在新執行緒中呼叫 `procMinossJobs`。
     - **填寫事件紀錄**：觸發 `autoMINOSSEvent`，在新執行緒中呼叫 `procMinossEventUpload`。
  4. `procMinossJobs` 會依序：
     - 呼叫 RecordFunctionLog 以紀錄功能存取
     - 取得維運日誌頁面，必要時選擇歷史日期
     - 執行設備檢查欄位填寫（`procLogBook_devCheck`）
     - 執行機房維運項目填寫（`procLogBook_SiteMaintain`）
     - 取得最新日誌頁面並顯示於 `webBrowser1`（僅當天日誌）
  5. `procMinossEventUpload` 會依序：
     - 取得個人 ID
     - 進入事件新增頁面
     - 依序 POST 事件類別、更新人員清單、送出事件內容
     - 最後取得 Main.aspx 內容並顯示於 `webBrowser1`

- **模組／檔案間的呼叫與資料流**
  - UI 事件 → `Auto_logbook_form`：`submit_Click`、`autoMINOSS`、`autoMINOSSEvent`、`funcSelectBox_SelectedIndexChanged`、`comboBox_eventMenu_SelectedIndexChanged`。
  - HTML 解析：使用 `HtmlAgilityPack.HtmlDocument` 解析回傳 HTML，並取出 hidden 欄位/表單欄位。
  - 與 `ParseResult` 互動：`procLogBook_devCheck` 與 `procLogBook_SiteMaintain` 會透過 `this.Tag` 取得 `ParseResult`，並依名稱尋找 `CheckBox`/`TextBox` 的填寫值。
  - 網路流程：使用 `WebClientEx`（含 CookieContainer）對 AM/NMOSS/MINOSS 網站進行 GET/POST，並自行組裝 POST 資料與 Header。

## 4. 重要類別與函式
- **`Auto_logbook_form`（Windows Form）**
  - **職責**：提供登入介面、操作功能選單、串接 MINOSS 日誌/事件填寫流程。
  - **主要成員與方法**：
    - `LoadSettingFromDB()`：讀取設定（目前僅設定 `MAX_COLUMN=40`、`MAX_HOST=8`，原本 DB 取值被註解）。
    - `Log(string logMessage)`：寫入本地日誌檔案 `LOG/OCS_GUI.log`。
    - `WebClientEx`：自訂 `WebClient`，處理 Cookie 及 ResponseUri。
    - `clientUploadValues` / `clientUploadData` / `clientUploadString` / `clientDownloadString`：封裝網路呼叫並加入重試機制。
    - `parseHtmlHiddenNameVal` / `parseMinossHtmlHiddenFields`：解析 HTML hidden 欄位並組裝 POST 資料。
    - `constructPostData`：建構 MINOSS 設備檢查欄位的 POST 參數基底。
    - `procLogBook_devCheck`：依設備清單逐筆填寫設備檢查欄位。
    - `procSiteMaintainItems`：提交單一機房維運項目結果。
    - `procLogBook_SiteMaintain`：依維運項目文字內容分派不同填寫邏輯。
    - `procMinossEventUpload`：新增事件紀錄的完整流程。
    - `procMinossJobs`：填寫工作日誌的完整流程。
    - `SSO3_login_portal` / `submit_Click` / `getOTP_Click`：負責 SSO3 登入/OTP 流程。
    - `autoMINOSS` / `autoMINOSSEvent`：在子執行緒執行 MINOSS 日誌或事件上傳。
    - `loadMinossEventTemplate`：從 `Template\MINOSS\` 讀取事件範本 HTML，解析服務/工作類別/標題/內容。
  - **被誰呼叫、呼叫誰**：
    - UI 控制項事件呼叫上述方法（`submit_Click`, `autoMINOSS`, `autoMINOSSEvent` 等）。
    - 內部方法彼此呼叫，例如 `procMinossJobs` → `procLogBook_devCheck`/`procLogBook_SiteMaintain`。

- **`Site_Authority`**
  - **`SSO3CertStruc`**：保存 SSO3 登入用的帳號、密碼、OTP、ViewState、FORM action 等。
  - **`nmossAuthDataStruc`**：保存 NMOSS Entry 所需的 hidden 欄位資料與是否為歷史日誌標記。

- **`ChtSSO3`（介面）**
  - 目前為空介面，無任何成員或實作。

## 5. 設計假設與限制
- 需能透過 `OCS_GUI.Properties.Settings.Default` 取得 SSO3/NMOSS/MINOSS 相關 URL 設定；設定來源未在此模組中定義。【需人工補充】
- 必須可存取外部網站 `am.cht.com.tw`、`nmoss.cht.com.tw`、`minoss.cht.com.tw`；若環境無法連線則流程會失敗。
- 依賴 UI `ParseResult` 控制項命名規則（例如 `{設備名}_C{欄位}_chkbox` 及 `textBox1..10`），若命名或結構變動將導致自動填寫失敗。【需人工補充】
- HTML 解析邏輯與 XPath 直接依賴頁面 DOM 結構，頁面變動時可能導致解析失敗。

## 6. 維護與擴充注意事項
- 若 MINOSS/NMOSS/SSO3 網頁結構或表單欄位變更，需同步更新 XPath、hidden 欄位解析與 POST 參數組裝邏輯。
- 日誌路徑 `LOG/` 與檔名 `LOG/OCS_GUI.log` 固定，需確認執行目錄具有寫入權限。
- `LoadSettingFromDB` 中原本的 DB 存取被註解，若需動態設定 `MAX_COLUMN` 與 `MAX_HOST`，需重新接回 DB 來源。
- 事件範本路徑固定為 `Template\MINOSS\`，若檔案名稱或位置變更，`comboBox_eventMenu_SelectedIndexChanged` 與 `loadMinossEventTemplate` 必須同步調整。
- `WebClientEx` 使用同步呼叫並於多處寫入日誌/HTML 檔案，若要改為非同步或增強錯誤處理需整體檢視。

## 7. 未明確資訊（Needs Clarification）
- `OCS_Parser_Minoss_GUI.ParseResult` 的來源與資料生成邏輯。【需人工補充】
- `OCS_GUI.Properties.Settings.Default` 中 `GET_OTP`、`auth_cred_submit` 等設定值來源與定義位置。【需人工補充】
- `Template\MINOSS\` 目錄與範本 HTML 的實際內容與維護規範。【需人工補充】
- 與外部系統（MINOSS/NMOSS/SSO3）互動的完整流程或授權規範（是否有權限/流量限制等）。【需人工補充】

