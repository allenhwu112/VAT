## Why

現有員工資料只能保存基本識別資訊，缺少聯絡電話、地址及出生年月日，導致員工基本資料不完整。這次補充欄位，讓後端與前端能一併維護完整的員工資料。

## What Changes

- 在 `dbo.Employees` 新增可為 NULL 的 `ContactPhone`、`Address`、`BirthDate` 欄位，避免既有資料因 migration 失敗。
- 擴充員工查詢與異動 Stored Procedure，查詢結果仍不回傳密碼。
- 擴充員工 CRUD API 的 request／response 欄位，維持 camelCase。
- 擴充 AG Grid 清單與 MUI Dialog，顯示及編輯三個新欄位。
- 更新 migration、API contract tests、前端驗證測試與建置驗證。

## Capabilities

### New Capabilities

- `employee-contact-details`: 員工可維護聯絡電話、地址及出生年月日；三個欄位均可留白。

### Modified Capabilities

無。

## Impact

- Database: 新增 FluentMigrator migration，並更新員工 Stored Procedure。
- Backend: employee DTO、ADO.NET repository、controller 及 API tests。
- Frontend: employee API mapping、AG Grid columns、MUI Dialog、表單驗證及測試。
- Existing employee records remain compatible because the new database columns are nullable.
