# CHTSystemAwake

## 方案用途
這是一個 Windows Forms 小工具，用來避免電腦進入睡眠或關閉螢幕。點擊「執行」後會透過 `SetThreadExecutionState` 申請 `ES_CONTINUOUS`、`ES_DISPLAY_REQUIRED`、`ES_SYSTEM_REQUIRED`、`ES_AWAYMODE_REQUIRED`，讓系統保持喚醒與螢幕常亮；點擊「停止」則恢復成只有 `ES_CONTINUOUS`，讓系統回到正常的省電行為。【F:CHTSystemAwake/CHTSystemAwake/Form1.cs†L25-L56】

## 主要功能
- **執行**：開始防止睡眠/關閉螢幕。【F:CHTSystemAwake/CHTSystemAwake/Form1.Designer.cs†L36-L44】【F:CHTSystemAwake/CHTSystemAwake/Form1.cs†L37-L43】
- **停止**：停止防止睡眠，恢復正常節能行為。【F:CHTSystemAwake/CHTSystemAwake/Form1.Designer.cs†L45-L53】【F:CHTSystemAwake/CHTSystemAwake/Form1.cs†L45-L48】
- **背景執行**：點擊後會隱藏視窗並顯示系統匣圖示，再次雙擊圖示可切回視窗。【F:CHTSystemAwake/CHTSystemAwake/Form1.Designer.cs†L54-L62】【F:CHTSystemAwake/CHTSystemAwake/Form1.cs†L29-L35】【F:CHTSystemAwake/CHTSystemAwake/Form1.cs†L50-L64】

## 注意事項
- 防止睡眠的效果只在程式執行期間有效；關閉程式後會恢復為系統預設電源管理行為。【F:CHTSystemAwake/CHTSystemAwake/Form1.cs†L37-L48】
- 若需要常駐背景，請使用「背景執行」或雙擊系統匣圖示切換顯示/隱藏狀態。【F:CHTSystemAwake/CHTSystemAwake/Form1.cs†L29-L35】【F:CHTSystemAwake/CHTSystemAwake/Form1.cs†L50-L64】
