# 程式說明書（Program Documentation）

> 說明範圍：`CHT_OMP/OCS_GUI/OCS_Parser_Minoss/` 之原始碼與相關檔案。

## 1. 專案總覽（用途、系統角色）

本模組提供一個 Windows Forms GUI，用於讀取以日期與設備命名的 Minoss 日誌檔，解析設備健康資訊（CPU、記憶體、磁碟、溫度、介面、NTP、syslog 等），並在畫面上動態產生檢查表格與填入結果，必要時可匯出 Excel 報表。系統同時支援解析機房維運相關的日誌（如備份、CDR、登入、告警、授權資訊），並將結果寫入文字欄位供後續匯出。用途基於程式碼可觀察之行為描述，不包含外部未實作功能。【需人工補充】若此模組實際運行環境與整體系統角色（例如與其他服務的協作關係）需更完整描述，需補充外部資訊。【需人工補充】【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ParseResult.cs†L93-L377】【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ExportExcelLibrary.cs†L1-L235】

## 2. 目錄與模組結構說明（逐一說明子目錄與主要檔案）

### 主要檔案
- `DevResource.cs`：定義 `devSysChkResult` 結構，保存解析後的設備檢查結果（溫度、CPU、記憶體、磁碟、log、介面、NTP 等），並提供欄位限制與存取器。`initialResource()` 目前未實作行為（直接返回）。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/DevResource.cs†L1-L86】
- `Parser_xshell_lib.cs`：集中各類設備/日誌解析邏輯，包含共用解析函式（CPU/記憶體/磁碟/系統 log/介面/NTP 等）與多種設備解析函式（如 7L2AIR、7L2SDP、7L2TMSAP、7L2OAM 等），以及備份/登入/告警/授權相關日誌解析函式。各解析函式輸入 log 檔路徑，輸出 `devSysChkResult` 結構。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/Parser_xshell_lib.cs†L1-L1908】
- `ParseResult.cs`：Windows Forms 主窗體，負責：
  - 取得設備清單（目前以程式碼硬編列）
  - 依日期與設備名稱組合 log 檔名並呼叫解析器
  - 解析後動態建立表格列與勾選結果
  - 解析其他維運 log（備份、CDR、告警、登入、授權等）並寫入文字欄位
  - 提供 UI 事件（解析、匯出、搬移 log、合併等）
  - 提供對外取得控制項值的 API 供匯出用【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ParseResult.cs†L1-L458】
- `ParseResult.Designer.cs`：主窗體 UI 設計檔，定義表格、文字欄位、按鈕與標籤等控制項與版面配置。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ParseResult.Designer.cs†L1-L200】
- `ParseResult.resx`：Windows Forms 資源檔（設計器資源）。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ParseResult.resx†L1-L120】
- `ExportExcelLibrary.cs`：Excel 匯出功能；建立「設備檢查」與「機房維運」兩張工作表，讀取 UI 內容並填入指定欄位，最後輸出 XLS 檔案至執行檔所在路徑。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ExportExcelLibrary.cs†L1-L235】
- `README.md`：檔案為空，未提供說明內容。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/README.md†L1】

## 3. 核心流程說明（資料流 / 呼叫關係）

### 3.1 Log 解析與 UI 呈現流程
1. 使用者在主窗體點選「解析」按鈕後觸發 `parse_Click`。該事件呼叫 `initialParseMinoss()` 進行解析流程。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ParseResult.cs†L431-L440】
2. `initialParseMinoss()` 先取得設備清單（`getHostListToList`），再依 `dateTimePicker1` 的日期組合檔名格式 `yyMMdd_設備名稱.log`。若檔案存在，依檔名判斷設備種類並呼叫對應解析函式（例如 `parser_7L2AIR`、`parser_7L2SDP` 等），解析結果寫入 `DevResource.devSysChkResult`。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ParseResult.cs†L93-L214】
3. 解析完成後，`setControl()` 會在 `tableLayoutPanel1` 動態新增一列：第一欄為設備名稱 Label，其餘欄位為 8 個檢查項目勾選框，並依解析結果設置勾選狀態與警示色；若 syslog 有異常則使用 ToolTip 顯示訊息內容。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ParseResult.cs†L298-L377】

### 3.2 其他維運日誌解析流程
1. 在 `initialParseMinoss()` 中，依同日日期讀取指定維運 log 檔：
   - `backup_verify.log`（備份檢查）
   - `cdr_check.log`（CDR 傳檔）
   - `sys_check_7L2OAM1.log` / `sys_check_7L2OAM2.log`（告警與登入）
   - `applogin_check.log`（應用程式登入）
   - `7L2SDP_License_view.log`（SDP 授權）
   - `7L2PCRF_License_view.log`（PCRF 授權）
   - `ilo_check.log`（ILO 檢查）
2. 對應的 parser 方法被呼叫並將結果寫入文字欄位（如 `textBox1`～`textBox10`），供後續匯出或人工檢視。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ParseResult.cs†L224-L295】

### 3.3 匯出 Excel 流程
1. 使用者點選「匯出」按鈕觸發 `export_Click`，建立 `ExportExcelLibrary` 並呼叫 `InitialExportMinoss`。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ParseResult.cs†L442-L452】
2. `InitialExportMinoss` 建立 Excel 應用程式、建立工作簿與兩個工作表：
   - 「設備檢查」：遍歷固定設備清單與 8 個檢查項目，透過 `GetControlValByName` 讀取 UI 值並填入。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ExportExcelLibrary.cs†L12-L99】
   - 「機房維運」：使用 `SelectIDCContent` 針對特定欄位取值或固定字串，再填入維運檢查項目。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ExportExcelLibrary.cs†L101-L186】
3. Excel 檔案輸出至 `Application.ExecutablePath` 所在資料夾，檔名為 `yyMMdd_MINOSS_export.xls`。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ExportExcelLibrary.cs†L188-L235】

## 4. 重要類別 / 函式說明（含責任與關聯）

### 4.1 `DevResource`
- `devSysChkResult`：保存單一設備或站點解析結果，包含溫度、CPU、記憶體、磁碟使用率、log/介面/NTP 狀態，以及 syslog 訊息列表。該結構為多數 parser 的輸出型別。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/DevResource.cs†L17-L79】
- `initialResource()`：目前為空實作，未提供初始化行為。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/DevResource.cs†L80-L85】

### 4.2 `Parser_xshell_lib`
**共用解析函式**
- `comm_parse_syschk_cmd`：解析 CPU/記憶體命令輸出，回傳百分比數值（記憶體使用率以 `100 - Free` 轉換）。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/Parser_xshell_lib.cs†L33-L56】
- `comm_parse_df_cmd`：解析 `df` 類輸出，取得最高的磁碟使用率百分比。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/Parser_xshell_lib.cs†L58-L86】
- `comm_parse_syslog_cmd`：解析 syslog 內容，蒐集訊息並返回是否有錯誤記錄。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/Parser_xshell_lib.cs†L88-L116】
- `comm_parse_bond_status` / `comm_parse_mii_status` / `comm_parse_ntp_status`：解析介面與 NTP 狀態指令輸出，回傳布林值。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/Parser_xshell_lib.cs†L118-L189】

**設備解析函式**
- `parser_7L2AIR`：解析 7L2AIR 設備 log，蒐集溫度、CPU、記憶體、磁碟、syslog、bond 介面與 NTP 狀態。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/Parser_xshell_lib.cs†L209-L281】
- `parser_7L2SDP`：解析 7L2SDP/OCC 設備 log，除了 bond 介面外，增加 `mii-tool` 介面檢查結果。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/Parser_xshell_lib.cs†L284-L364】
- 其他設備解析（TMSAP/TMSDB/TMSREP/NDDP/IVRC/USSDGW/OCSGAP/OCSGDB/PCRF/OAM/NEWOCSG 等）：解析流程結構相似，依設備 log 內容抓取對應資訊。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/Parser_xshell_lib.cs†L447-L1379】

**站點/維運 log 解析函式**
- `parser_backup_verify`、`parser_cdr_check`：解析備份與 CDR 日誌，判斷是否有漏檔並收集訊息。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/Parser_xshell_lib.cs†L1405-L1510】
- `parser_alarms_stat`、`parser_login_log`、`parser_app_log`：解析告警、系統登入、應用登入資訊並回傳訊息或狀態。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/Parser_xshell_lib.cs†L1512-L1682】
- `parser_SDPLicense_log`、`parser_PCRFLicense_log`：解析授權資訊日誌，回傳內容或錯誤狀態。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/Parser_xshell_lib.cs†L1684-L1901】
- `parser_ILO_Check_log`：解析 ILO 檢查日誌內容。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/Parser_xshell_lib.cs†L1903-L1908】

### 4.3 `ParseResult`（主窗體）
- `initialParseMinoss()`：主解析入口；讀取設備清單與指定日期 log 檔，呼叫對應 parser，並將結果渲染到 UI；同時解析維運 log 並更新文字欄位。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ParseResult.cs†L93-L295】
- `setControl()`：動態建立設備列的 Label 與 CheckBox，並依解析結果設定狀態、顯示警示與 ToolTip。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ParseResult.cs†L298-L377】
- `GetControlValByName()`：提供外部查詢控制項值（CheckBox/TextBox/Label），供 Excel 匯出時讀取畫面內容。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ParseResult.cs†L252-L296】
- `move_Click()`：將當日 log 搬移至以日期格式命名的資料夾（MM.dd）。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ParseResult.cs†L378-L419】

### 4.4 `ExportExcelLibrary`
- `CreateDevCheckSheet()`：建立「設備檢查」工作表，依固定設備清單與 8 個檢查項目填入資料，並從 `ParseResult` 控制項讀取值。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ExportExcelLibrary.cs†L12-L99】
- `CreateIDCCheckSheet()`：建立「機房維運」工作表，依固定維運項目與 `SelectIDCContent()` 的 mapping 內容填入資料。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ExportExcelLibrary.cs†L128-L186】
- `InitialExportMinoss()`：Excel 匯出入口，建立工作簿、輸出兩工作表並儲存檔案。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ExportExcelLibrary.cs†L188-L235】

## 5. 設計假設與限制

- 設備清單目前以程式碼硬編列（`getHostListToList`），若實際設備名單需由 DB 或設定檔載入，必須人工補充與調整。【需人工補充】【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ParseResult.cs†L47-L92】
- Log 檔案命名格式與位置使用 `yyMMdd_設備名稱.log` 且在執行目錄，程式僅解析存在的檔案，無額外錯誤處理或重試策略。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ParseResult.cs†L93-L214】
- Excel 匯出使用 `Microsoft.Office.Interop.Excel`，需要本機安裝 Excel，否則匯出失敗並顯示提示訊息。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ExportExcelLibrary.cs†L176-L207】
- 解析結果中部分文字欄位內容會以固定字串填入，非來自 log（例如 IDC 工作項目）。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ExportExcelLibrary.cs†L101-L126】
- `README.md` 未提供資訊，對建置與部署步驟無明確描述。【需人工補充】【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/README.md†L1】

## 6. 維護與擴充注意事項

- 新增設備類型時，需同時：
  1. 在 `getHostListToList()` 增加設備名稱
  2. 在 `initialParseMinoss()` 新增對應檔名判斷與 parser 呼叫
  3. 在 `Parser_xshell_lib` 新增解析方法或擴充現有方法
  4. 若需匯出至 Excel，還需在 `ExportExcelLibrary` 中的設備清單或欄位 mapping 調整【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ParseResult.cs†L47-L214】【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ExportExcelLibrary.cs†L12-L99】
- 若 log 來源格式變更（指令輸出文字不同），需更新 `comm_parse_*` 解析邏輯以維持相容性。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/Parser_xshell_lib.cs†L33-L189】
- UI 是動態產生表格列，若需求變更（例如欄位數量改動），需同步更新 `setControl()` 及 Excel 匯出欄位數量（`chkItems`）。【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ParseResult.cs†L298-L377】【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ExportExcelLibrary.cs†L12-L24】

## 7. 未明確資訊清單（Needs Clarification）

- 設備清單的來源、更新頻率與正確性：目前僅看到硬編列字串，缺少實際維運流程說明。【需人工補充】【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ParseResult.cs†L47-L92】
- Excel 匯出欄位對應的業務意義（例如固定字串與欄位名稱的關係）未見說明。【需人工補充】【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ExportExcelLibrary.cs†L12-L186】
- `Auto_logbook_form` 類別的功能與資料來源未在本目錄中出現，無法判斷其角色與影響。【需人工補充】【F:CHT_OMP/OCS_GUI/OCS_Parser_Minoss/ParseResult.cs†L421-L425】
