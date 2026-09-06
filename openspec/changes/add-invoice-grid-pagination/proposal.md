## Why

發票管理資料會隨客戶數量增加，單一長表會降低瀏覽與定位效率。為了保留現有 Enterprise Grid 的複製、儲存格選取與雙擊修改能力，發票頁需要加入可操作的分頁導覽。

## What Changes

- 在共用 Enterprise Grid 增加可選的 client-side pagination 設定，預設不改變員工與客戶頁的既有行為。
- 啟用發票管理頁分頁，提供頁碼導覽、目前筆數／總筆數與每頁筆數選擇。
- 分頁切換後仍保留單列選取、儲存格／範圍選取、剪貼簿複製及雙擊進入修改模式。
- 新增前端路由／Grid 契約測試與 build／lint 驗證；不修改 API、資料庫 schema 或 migration。

## Capabilities

### New Capabilities

- `invoice-grid-pagination`: 發票管理 Enterprise Grid 的 client-side 分頁、頁面導覽與既有互動相容性。

### Modified Capabilities

- None.

## Impact

- Frontend: `ManagementGrid.jsx`、`InvoicesPage.jsx` 及相關測試。
- API／Database: 不變；沿用現有 `GET /VAT_API/invoices` 一次取得資料。
- Dependencies: 不新增套件，沿用已註冊的 `ag-grid-enterprise`。
