## Why

VAT 目前只有員工管理，缺少可供帳務流程使用的客戶主檔；客戶統編、全稱、簡稱、負責人與地址無法集中維護。現在新增獨立客戶管理能力，可沿用既有員工 CRUD 模式並以明確的前後端 URL 區分兩個管理頁面。

## What Changes

- 新增 `dbo.Clients` 客戶資料表，使用 `ClientId` Identity 主鍵及唯一、格式受限的八碼統編。
- 新增客戶查詢與異動 Stored Procedures、.NET API DTO、Repository、Controller 及完整 CRUD API。
- 將員工與客戶資源 API 統一置於 `/VAT_API` 前綴，健康檢查也改用相同前綴。
- 新增 `/VAT_UI/clients` 客戶管理頁，並將員工頁固定為 `/VAT_UI/employees`。
- 將 AG Grid 升級為 Enterprise，兩個管理頁都支援儲存格範圍選取、剪貼簿複製、單列選取與雙擊進入修改模式。
- 新增客戶表單必填與統編／長度驗證、刪除確認、路由、API、migration 與 UI 測試。

## Capabilities

### New Capabilities

- `client-management`: 維護客戶主檔資料，提供必填欄位驗證、完整 CRUD、管理頁路由與 Enterprise Grid 操作。

### Modified Capabilities

- None.

## Impact

- Database: 新增可回滾的 FluentMigrator migrations、資料表、constraints、唯一索引與 Stored Procedures。
- Backend: 新增客戶 API 層，並變更員工與健康檢查的公開 route 前綴。
- Frontend: 新增客戶 API／Dialog／管理頁與前端路由，重用員工管理的單列選取及 CRUD 互動。
- Dependencies: `ag-grid-enterprise@36.1.0` 與現有 `ag-grid-community`／`ag-grid-react` 保持同版本。
