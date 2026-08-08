# VAT Monorepo

VAT 專案的 monorepo 基礎架構，包含 React + Vite 前端與 .NET 10 Controllers Web API 後端。

## 專案結構

```text
apps/
├─ frontend/          # React + Vite + JavaScript
└─ backend/Vat.Api/   # .NET 10 Controllers Web API
```

## 開發環境

- Node.js 與 npm
- .NET 10 SDK

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
npm run build          # 建置前端與後端
npm run build:frontend
npm run build:backend
```

## API

健康檢查端點：

```text
GET http://localhost:5000/api/health
```

OpenAPI 文件（Development）：

```text
GET http://localhost:5000/openapi/v1.json
```
