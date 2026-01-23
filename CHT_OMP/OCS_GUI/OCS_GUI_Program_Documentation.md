# OCS_GUI 專案層級程式說明書

## 1. OCS_GUI 專案總覽

### 專案目的
OCS_GUI 為 Windows Forms 應用程式，核心目的為：
1. 解析並呈現 MINOSS 相關維運 log（設備健康檢查、機房維運相關 log），可於 UI 檢視並匯出 Excel。
2. 透過 SSO3 登入後，自動將解析結果填寫回 MINOSS 的工作日誌與事件紀錄頁面。

### OCS_GUI 在整體系統中的角色
OCS_GUI 是「維運 log 解析 → GUI 檢視/補充 → 自動回填 MINOSS」流程中的 GUI 中樞，提供操作介面與資料整合點。它同時擔任：
- 解析端：讀取本機 log 檔並轉換成 GUI 控制項結果。
- 回填端：將解析結果（含文字欄位補充）寫回 MINOSS。

> 上述角色以專案內程式碼的功能行為為依據；外部系統定位或部署流程需另行補充。【需人工補充】

## 2. 專案整體架構

### 主要子模組
- **OCS_Parser_Minoss**：解析 MINOSS log 的 GUI 與 Excel 匯出工具。提供解析、結果呈現與結果匯出。
- **OCS_auto_minoss**：透過 SSO3/NMOSS/MINOSS 網頁流程，自動填寫維運日誌與事件紀錄。
- **OCS_GUI (Root)**：專案入口與共用設定（app.config、Properties、專案參考）。

### 子模組互動關係
- `ParseResult`（OCS_Parser_Minoss GUI）為主要畫面；使用者在此解析 log 並檢視結果。
- 使用者在 `ParseResult` 點選「合併/自動填寫」後，會開啟 `Auto_logbook_form`（OCS_auto_minoss）。
- `Auto_logbook_form` 透過 `Tag` 取得 `ParseResult` 內的 CheckBox/TextBox 值，將結果回填至 MINOSS。
- `Auto_logbook_form` 需要 SSO3 登入成功後才能執行自動填寫流程。

## 3. 目錄結構說明

> 只列出 OCS_GUI 專案下的重要資料夾與檔案；並對應既有說明文件。

- `OCS_GUI.csproj`：專案檔，定義組件參考與編譯項目。
- `Program.cs`：主程式入口（目前僅啟用視覺樣式，未執行 `Application.Run`）。
- `app.config`：SSO3/NMOSS/MINOSS 相關 URL 與 runtime 參考設定。
- `OCS_Parser_Minoss/`
  - 既有說明文件：`Program_Documentation.md`
  - 內容涵蓋：解析流程、主要類別與 Excel 匯出
- `OCS_auto_minoss/`
  - 既有說明文件：`Program_Documentation.md`
  - 內容涵蓋：登入流程、自動填寫流程與 UI 控制項
- `Properties/`：專案資源與 Settings 設定

## 4. 核心流程與資料流

### 4.1 主要啟動流程
1. 使用者開啟 OCS_GUI（實際啟動入口需確認）。【需人工補充】
2. `ParseResult` 為主要操作畫面：
   - 解析 log 並在 UI 中顯示結果。
   - 可匯出 Excel。
   - 可開啟 `Auto_logbook_form` 進行自動填寫 MINOSS。

### 4.2 GUI 與後端模組互動方式
- **解析流程**：`ParseResult` 依日期與設備名稱組合檔名，讀取 log 後呼叫 `Parser_xshell_lib` 解析器；解析結果填入動態建立的 CheckBox / TextBox。
- **自動回填流程**：
  1. `Auto_logbook_form` 透過 SSO3 登入後取得 NMOSS/MINOSS 入口。
  2. `Auto_logbook_form` 在執行自動填寫時，從 `ParseResult` UI 控制項讀取解析結果（CheckBox/TextBox）。
  3. 以 WebClient 模擬 HTTP POST，將解析結果填入 MINOSS 維運日誌與事件紀錄。

## 5. 子模組補充說明

### 5.1 OCS_auto_minoss

#### 既有文件已說明的部分（簡要整理）
- SSO3 登入流程、MINOSS/NMOSS 的 HTTP 互動流程。
- 自動填寫維運日誌與事件紀錄的流程。
- 使用 `HtmlAgilityPack` 解析 HTML hidden 欄位與表單欄位。

#### 補充內容（此為補充內容）
- **資料來源與 UI 依賴**：
  - `Auto_logbook_form` 透過 `this.Tag` 取得 `ParseResult`，並依控制項命名規則讀取 CheckBox/TextBox 值作為 MINOSS 填寫資料來源。
  - 這表示 `OCS_auto_minoss` 與 `OCS_Parser_Minoss` 的 UI 控制項命名與結構具有強耦合。
- **事件範本來源**：
  - 事件紀錄內容由 `Template\MINOSS\` 目錄下 HTML 範本讀取，並從 `select/textarea` 欄位抽取 service/work type/title/content。
- **固定文字回填**：
  - 針對部分機房維運項目（如 DVR/ISO 相關項目），程式直接回填固定文字內容。
- **歷史日誌判斷邏輯**：
  - `Auto_logbook_form` 會比較 `ParseResult` 內 `dateTimePicker1` 的日期與今日日期，判斷是否填寫「歷史維運日誌」。

### 5.2 OCS_Parser_Minoss

#### 既有文件已說明的部分（簡要整理）
- 解析 log、動態建立 UI、匯出 Excel 的主流程。
- `Parser_xshell_lib` 內多種設備解析邏輯。

#### 補充內容（此為補充內容）
- **與 OCS_auto_minoss 的互動入口**：
  - `ParseResult` 的「合併/自動填寫」功能會建立 `Auto_logbook_form` 並將自身實例傳入，形成資料回填的橋接點。
- **設備清單硬編列且包含重複**：
  - `getHostListToList` 內設備名稱為硬編列清單，且 `7L2AIR1` 重複多次，代表解析結果可能出現重複列。
- **log 檔搬移功能**：
  - `move_Click` 會將目前目錄下的 `yyMMdd_*.log` 搬移至 `MM.dd` 資料夾，作為 log 整理流程的一部分。

## 6. 設計假設與系統限制

- 需要能連線至外部系統：SSO3（am.cht.com.tw）、NMOSS（nmoss.cht.com.tw）、MINOSS（minoss.cht.com.tw）。
- `Auto_logbook_form` 的運作依賴 `ParseResult` 控制項名稱與結構，不可任意更動 UI 命名規則。
- Excel 匯出依賴 `Microsoft.Office.Interop.Excel`，需安裝 Excel。
- `Program.cs` 內未呼叫 `Application.Run`，實際啟動入口或執行方式需確認。【需人工補充】

## 7. 維護與擴充注意事項

- 若 MINOSS/NMOSS/SSO3 的 HTML 結構變更，需要更新 hidden 欄位解析與 POST 組裝邏輯。
- 新增設備解析時需同步更新：
  1. `getHostListToList` 設備清單
  2. `initialParseMinoss` 設備類型判斷與 parser 呼叫
  3. `Parser_xshell_lib` 對應解析函式
  4. Excel 匯出 mapping
- 若事件範本（Template\MINOSS\*.htm）內容格式變更，需同步更新 `loadMinossEventTemplate` 的 XPath。

## 8. 未明確資訊（Needs Clarification）

- **實際啟動流程**：`Program.cs` 未啟動任何 Form，實際啟動方式、入口點或外部呼叫流程不明。【需人工補充】
- **SSO3/NMOSS/MINOSS 驗證流程的授權規範**：是否需特定帳號/權限或白名單設定未見說明。【需人工補充】
- **範本與 log 檔來源**：`Template\MINOSS\` 及 log 檔來源/產生方式未在本專案內說明。【需人工補充】
- **設備清單來源與維護機制**：目前為硬編列，是否應由 DB 或設定檔維護需確認。【需人工補充】
