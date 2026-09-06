## Why

VAT 目前已有員工與客戶主檔，但缺少集中維護客戶發票開立方式的管理能力。新增發票管理後，可用統編與客戶簡稱識別客戶，並以固定欄位維護電子發票、收銀機及二聯式／三聯式發票選項。

## What Changes

- 新增 `Invoices` 發票管理資料表，保存統編、客戶簡稱與七個發票方式布林欄位。
- 新增查詢與異動 Stored Procedures、.NET DTO／Repository／Controller，以及完整 CRUD API。
- 新增 `/VAT_API/invoices` 資源路由，沿用現有 `{ data: ... }` response envelope。
- 新增 `/VAT_UI/invoices` 管理頁、表單與導覽連結；保留員工 `/VAT_UI/employees` 與客戶 `/VAT_UI/clients` 的明確連結。
- 沿用員工表的表單驗證與單列選取互動，使用現有 AG Grid Enterprise 設定支援儲存格／範圍選取、剪貼簿複製及雙擊修改。
- 新增可回滾的 FluentMigrator migrations、API／migration／前端測試與使用說明中的明確 URL。

## Capabilities

### New Capabilities

- `invoice-management`: 維護客戶發票設定，提供欄位驗證、完整 CRUD、API／UI 路由與 Enterprise Grid 操作。

### Modified Capabilities

- None.

## Impact

- Database: `dbo.Invoices`、唯一統編索引、約束及 `Invoice_Query`／`Invoice_Command` Stored Procedures。
- Backend: 新增 invoice model、repository、controller，並註冊 DI。
- Frontend: 新增 invoice API、dialog、管理頁與路由導航；共用 `ManagementGrid` 的 Enterprise 設定。
- Tests and docs: 新增跨層契約測試，補充員工／客戶／發票三個 UI URL 與 API URL。
