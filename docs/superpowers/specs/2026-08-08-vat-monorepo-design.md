# VAT Monorepo 設計規格

日期：2026-08-08

## 目標

將目前幾乎空白的 VAT 專案建立成 monorepo，包含：

- React + Vite + 純 JavaScript 前端
- .NET 10 ASP.NET Core Controllers Web API 後端
- 使用 npm workspaces 管理 JavaScript 專案
- 根目錄的 `npm start` 同時啟動前端與後端
- 安裝並驗證 .NET 10 SDK

## 已確認的技術決策

| 項目 | 決策 |
| --- | --- |
| Monorepo 管理 | npm workspaces |
| 前端 | React、Vite、JavaScript，不使用 TypeScript |
| 後端 | .NET 10 ASP.NET Core Controllers Web API |
| API 風格 | Controllers，不使用 Minimal API |
| 前端開發網址 | `http://localhost:5173` |
| 後端開發網址 | `http://localhost:5000` |
| 根目錄啟動指令 | `npm start` |

## 目錄結構

```text
VAT/
├─ apps/
│  ├─ frontend/
│  │  ├─ src/
│  │  │  ├─ api/
│  │  │  │  └─ health.js
│  │  │  ├─ App.jsx
│  │  │  ├─ main.jsx
│  │  │  └─ styles.css
│  │  ├─ index.html
│  │  ├─ package.json
│  │  └─ vite.config.js
│  │
│  └─ backend/
│     └─ Vat.Api/
│        ├─ Controllers/
│        │  └─ HealthController.cs
│        ├─ Properties/
│        │  └─ launchSettings.json
│        ├─ Program.cs
│        ├─ appsettings.json
│        ├─ appsettings.Development.json
│        └─ Vat.Api.csproj
│
├─ docs/
│  └─ superpowers/
│     └─ specs/
│        └─ 2026-08-08-vat-monorepo-design.md
├─ package.json
├─ package-lock.json
├─ README.md
├─ NuGet.Config
├─ scripts/
│  └─ dotnet.cmd
└─ .gitignore
```

目前不建立共用 `packages/`，因為前端是 JavaScript、後端是 C#，尚無需要共用的程式碼或型別。日後若有共用工具，再獨立加入 workspace package。

## 根目錄工作流程

根目錄 `package.json` 使用 npm workspace 管理 `apps/frontend`，並以 `concurrently` 啟動兩個應用程式。預計提供以下指令：

- `npm install`：安裝前端與根目錄開發依賴
- `npm start`：同時啟動前端與後端
- `npm run frontend`：只啟動 Vite
- `npm run backend`：只啟動 .NET API
- `npm run build`：依序建置前端與後端
- `npm run build:frontend`：只建置前端
- `npm run build:backend`：只建置後端

`npm start` 的兩個子程序如下：

```text
npm start
├─ npm run frontend
│  └─ Vite: http://localhost:5173
└─ npm run backend
   └─ ASP.NET Core: http://localhost:5000
```

任一子程序失敗時，`concurrently` 會停止另一個子程序，讓啟動指令回傳失敗狀態。

## 前端設計

前端採用 Vite 官方 React JavaScript 基礎結構，責任分工如下：

- `main.jsx`：建立 React root 並載入全域樣式
- `App.jsx`：顯示初始頁面與後端健康狀態
- `api/health.js`：集中處理 `/api/health` 請求
- `styles.css`：提供最小必要的可讀性與狀態樣式
- `vite.config.js`：設定 Vite 開發伺服器與 API 連線方式

前端透過 `VITE_API_BASE_URL` 指向後端，開發環境預設為 `http://localhost:5000`。健康檢查畫面至少呈現 loading、成功與失敗三種狀態。

## 後端設計

後端使用 ASP.NET Core Controllers Web API：

- `Program.cs`：註冊 Controllers、ProblemDetails、CORS 與 OpenAPI，並設定 HTTP 服務埠
- `HealthController.cs`：提供 `GET /api/health`
- `appsettings*.json`：保留環境設定檔入口
- `launchSettings.json`：設定本機開發服務埠

健康檢查回應為簡單 JSON 物件，至少包含：

```json
{
  "status": "ok",
  "service": "Vat.Api",
  "timestamp": "2026-08-08T00:00:00Z"
}
```

初始版本不加入資料庫、登入驗證、發票模組或營業稅業務規則；這些會在基礎架構可運作後另行設計。

## 前後端資料流

```text
React App
  → health.js
  → GET /api/health
  → HealthController
  → JSON response
  → App.jsx 顯示 API 狀態
```

後端 CORS 允許 `http://localhost:5173`，以支援本機前端直接呼叫 API。API 路徑集中以 `/api` 開頭，方便未來擴充各業務 Controller。

## 錯誤處理

- 後端啟用標準 `ProblemDetails`，讓未處理例外與 API 錯誤具有一致回應格式
- Controllers 使用 `[ApiController]`，由 ASP.NET Core 處理基本輸入驗證錯誤
- 前端 API 模組將網路錯誤與非成功 HTTP 回應轉成可顯示的錯誤訊息
- 初始畫面不會因 API 暫時無法連線而無限等待

## SDK 與依賴安裝

需要安裝 Windows x64 的 .NET 10 SDK，並以 `dotnet --version` 驗證結果為 `10.x`。完成 SDK 安裝後，再執行：

1. `npm install`
2. `npm run restore:backend`
3. `npm start`

安裝與下載使用官方 .NET SDK 來源；不在 repository 內提交 SDK 安裝檔。

## 驗證與完成條件

完成後必須確認：

1. `dotnet --version` 回傳 .NET 10 版本
2. `npm install` 成功完成
3. `npm run build:frontend` 成功
4. `npm run build:backend` 成功
5. `npm start` 可同時啟動前端與後端
6. `GET http://localhost:5000/api/health` 回傳成功 JSON
7. 前端頁面能顯示後端健康狀態

目前不建立獨立測試專案；本次以建置、健康檢查 API 與前後端啟動整合做為初始驗證基線。

## 不在本次範圍

- 資料庫與 Entity Framework Core
- 登入、權限與 JWT
- 發票、營業稅或其他 VAT 業務功能
- Docker、CI/CD 與雲端部署
- 共用 JavaScript/C# 型別產生流程
