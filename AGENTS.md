# Repository Instructions

這是本 repository 唯一的 `AGENTS.md`，適用於所有 agent 與自動化工作。除非使用者針對單一任務明確授權，否則必須遵守以下目錄邊界、檔案白名單與資料庫規則。不得在子目錄新增其他 `AGENTS.md`。

## 1. 目錄責任與修改邊界

| 路徑 | 責任範圍 | 規則 |
| --- | --- | --- |
| `apps/frontend/**` | React/Vite 前端 | 只放前端頁面、元件、樣式、前端設定與靜態資產。 |
| `apps/backend/**` | .NET API、migration 與 backend tests | 只放後端程式、後端設定、FluentMigrator runner、migration 與相關測試。 |
| `apps/backend/Vat.Migrations/Migrations/**` | FluentMigrator migrations | 只放資料庫 migration 類別，不放 API、領域服務或一般 SQL 腳本。 |
| `apps/backend/Vat.Migrations.Tests/**` | Migration tests | 只放 migration 的測試、測試 fixture 與測試設定。 |

前端工作只能修改 `apps/frontend/**`；後端或資料庫工作只能修改 `apps/backend/**`。若需求需要同時跨越 frontend/backend，或需要修改未列入白名單的根目錄檔案，必須先停止並請使用者明確授權；單一任務的授權不構成永久例外。

## 2. 根目錄檔案

根目錄工具檔只能為建置、測試、migration 或文件整合而修改。可受控修改的例外只有：

- `package.json`
- `package-lock.json`
- `NuGet.Config`
- `scripts/**`
- `README.md`
- `docs/**`
- 根目錄 `AGENTS.md`

其他根目錄檔案不得在未經使用者明確授權下新增或修改；根目錄不得放置前端、後端或資料庫業務程式碼。

## 3. 前端檔案白名單

規則套用於 `apps/frontend/**` 的新增或修改檔案：

- 原始碼只允許 `.js`、`.jsx`、`.css`。
- 頁面與設定只允許 `.html`、`.json`。
- 靜態資產只允許 `.svg`、`.png`。
- 既有例外為 `apps/frontend/README.md` 與 `apps/frontend/.gitignore`；保留這些既有檔案，但不得據此新增其他未列入白名單的檔案。
- 禁止新增或修改前端 TypeScript 檔案（`.ts`、`.tsx`），也禁止放置 `.cs`、`.csproj`、`.sql` 或其他未列入白名單的檔案。
- `apps/frontend/**` 不得放置後端程式、migration、資料庫 schema 或資料庫連線程式。

## 4. 後端檔案規則

- 後端程式與設定只能位於 `apps/backend/**`。
- 主要允許的檔案類型為 `.cs`、`.csproj`、`.json`；其他檔案類型須先取得使用者明確授權，且必須有後端建置、測試或設定的必要性。
- 後端 API 不得把 migration 執行綁在 API 啟動流程中；API 啟動不得自動執行 migration。
- 不得在 `apps/frontend/**` 放置任何後端程式或資料庫程式。

## 5. FluentMigrator 規則

- Migration 類別只能放在 `apps/backend/Vat.Migrations/Migrations/**`，並使用本專案的 FluentMigrator runner 掃描與執行。
- 每個 migration 必須有唯一版本號，且必須實作 `Up()` 與 `Down()`；`Down()` 應提供與 `Up()` 對應且可理解的回滾行為。
- 已套用的 migration 不得直接修改。需要變更 schema 時，必須新增版本更高的後續 migration。
- 只有在確有必要時才可使用 `Execute.Sql`；必須在 migration 程式碼或註解中說明原因，並確保可測試、可回滾且不繞過版本追蹤。
- 不得透過手動 SQL、API 啟動程式或其他 migration 外部流程直接修改 VAT 資料庫 schema；資料庫 schema 變更必須透過 migration。
- 唯讀連線檢查使用 `npm run migrate:vat:check`；執行 migration 使用 `npm run migrate:vat`；回滾最後一個 migration 使用 `npm run migrate:vat:down`。
- `migrate:vat:check` 只可做唯讀連線與資料庫名稱確認，不得建立、修改或刪除資料庫物件。
- 連線字串不得提交帳號密碼、token、金鑰或其他秘密；本機與正式環境應使用環境設定覆寫敏感值。
- 執行實際 migration（包括 `migrate:vat` 與 `migrate:vat:down`）前，必須取得使用者明確授權；不得僅因建置或測試而自行更新或回滾資料庫。

## 6. 共同工作流程與安全

- 凡進入規劃模式並形成實作方案，必須呼叫 OpenSpec（至少執行 `openspec list --json`，並依 change 狀態使用相應的 OpenSpec workflow）；不得只以對話中的方案取代 OpenSpec 規劃 artifacts。
- 若 `openspec` CLI 不可用、執行失敗或 change 狀態不明，必須停止並回報阻塞原因；在取得處理方向前不得繞過 OpenSpec 進入實作。
- 修改任何檔案前，先執行 `git status --short`，確認並保留既有未提交變更。
- 只修改完成任務所需的檔案；不得整理、重設、格式化或覆寫無關的既有變更。
- 不批量刪除檔案；批量刪除前必須先通知使用者並取得確認。
- 禁止使用 `git reset --hard`，也不得執行未經授權的 destructive 操作或會覆寫使用者工作的指令。
- 完成前至少檢查 `git diff --check` 與 `git status --short`，並依變更範圍執行適當的 lint、測試或建置。

## 7. 建議驗證指令

- 前端：`npm --workspace apps/frontend run lint`、`npm run build:frontend`
- 後端：`npm run test:backend`、`npm run build:backend`
- 全 repository：`npm run build`
- migration：先執行 `npm run migrate:vat:check`，取得明確授權後才執行 `npm run migrate:vat` 或 `npm run migrate:vat:down`

本文件只建立 repository instructions，不要求新增 boundary validator、CI workflow 或其他自動 enforcement script。
