# VAT Monorepo

VAT 專案的 monorepo 基礎架構，包含 React + Vite 前端與 .NET 10 Controllers Web API 後端。

## 專案結構

```text
apps/
├─ frontend/          # React + Vite + JavaScript
└─ backend/
   ├─ Vat.Api/         # .NET 10 Controllers Web API
   ├─ Vat.Migrations/  # FluentMigrator runner and migrations
   ├─ Vat.Api.Tests/   # API contract tests
   └─ Vat.Migrations.Tests/ # Migration tests
```

## 開發環境

- Node.js 與 npm
- .NET 10 SDK
- 本機 SQL Server（預設執行個體 `MSSQLSERVER`）與 `VAT` 資料庫

本機 .NET SDK 安裝在 `%USERPROFILE%\.dotnet`。根目錄的 `scripts\dotnet.cmd` 會優先使用這個 SDK，避免受到系統既有 .NET runtime 的影響。

## 安裝依賴

```powershell
npm install
npm run restore:backend
```

目前 PowerShell 的執行原則會阻擋 `npm.ps1` 時，請使用等價的 `npm.cmd`：

```powershell
npm.cmd install
npm.cmd run restore:backend
npm.cmd start
```

## 啟動專案

從 repository 根目錄執行：

```powershell
npm start
```

會同時啟動：

- 前端：<http://localhost:5173>
- 後端：<http://localhost:5000>

如果 PowerShell 阻擋 npm script，使用：

```powershell
npm.cmd start
```

## 可用指令

```powershell
npm run frontend       # 只啟動 Vite
npm run backend        # 只啟動 .NET API
npm run restore:backend # 使用 repository NuGet 設定還原後端套件
npm run test:backend    # 執行 migration 與 API 測試
npm --workspace apps/frontend run test # 執行前端表單測試
npm run migrate:vat:check # 唯讀檢查 VAT 資料庫連線
npm run migrate:vat     # 套用待執行的 FluentMigrator migrations
npm run migrate:vat:down # 回滾最後一個 migration
npm run build          # 建置前端與後端
npm run build:frontend
npm run build:backend
```

## API

健康檢查端點：

```text
GET http://localhost:5000/api/health
```

員工管理端點：

```text
GET    http://localhost:5000/api/employees
GET    http://localhost:5000/api/employees/{employeeId}
POST   http://localhost:5000/api/employees
PUT    http://localhost:5000/api/employees/{employeeId}
DELETE http://localhost:5000/api/employees/{employeeId}
```

員工清單及新增／修改回應不會回傳 `Password`。目前資料表依需求暫存明碼密碼；正式環境使用前應改為不可逆密碼雜湊。

OpenAPI 文件（Development）：

```text
GET http://localhost:5000/openapi/v1.json
```

## 資料庫與 FluentMigrator

後端及獨立 migration runner 共用設定鍵 `ConnectionStrings:VatDatabase`。本機預設連線字串為：

```text
Server=localhost;Database=VAT;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=False
```

這是 Windows 驗證，不需要在 repository 中保存密碼。可用環境變數覆寫設定：

```powershell
$env:ConnectionStrings__VatDatabase = "Server=localhost;Database=VAT;Integrated Security=True;Encrypt=True;TrustServerCertificate=False"
```

`VAT` 資料庫必須先存在，執行 migration 的 Windows 帳號也必須具備建立及修改資料表的權限。migration runner 不會建立資料庫，也不會在 API 啟動時自動修改資料庫。

執行唯讀連線檢查：

```powershell
npm run migrate:vat:check
```

套用 migration 或回滾最後一個 migration：

```powershell
npm run migrate:vat
npm run migrate:vat:down
```

目前 migration 為 baseline `202608220001`、員工資料表 `202608220002` 及員工 Stored Procedure `202608220003`。FluentMigrator 會使用自己的 `VersionInfo` 表追蹤版本；已執行的 migration 不應直接修改，後續 schema 變更應建立更高版本的 migration。
