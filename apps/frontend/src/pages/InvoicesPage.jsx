import { useCallback, useEffect, useMemo, useState } from 'react'
import Alert from '@mui/material/Alert'
import Button from '@mui/material/Button'
import Dialog from '@mui/material/Dialog'
import DialogActions from '@mui/material/DialogActions'
import DialogContent from '@mui/material/DialogContent'
import DialogContentText from '@mui/material/DialogContentText'
import DialogTitle from '@mui/material/DialogTitle'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { deleteInvoice, fetchInvoices } from '../api/invoices'
import { InvoiceDialog } from '../components/InvoiceDialog'
import { ManagementGrid } from '../components/ManagementGrid'

const invoiceQuantityLabels = {
  cashRegister: '收銀機',
  threeCashRegister: '三收銀',
  twoPartInvoice: '二聯式',
  twoPartInvoiceCopy: '二聯式副聯',
  threePartInvoice: '三聯式',
  threePartInvoiceCopy: '三聯式副聯',
}

const invoiceOptionColumns = [
  {
    field: 'electronicInvoice',
    headerName: '電子發票',
    width: 125,
    valueFormatter: ({ value }) => (value ? '是' : '否'),
  },
  ...Object.entries(invoiceQuantityLabels).map(([field, headerName]) => ({
    field,
    headerName,
    width: 125,
    type: 'numericColumn',
    valueFormatter: ({ value }) => (value ?? '').toString(),
  })),
]

export function InvoicesPage() {
  const [invoices, setInvoices] = useState([])
  const [selectedInvoice, setSelectedInvoice] = useState(null)
  const [gridApi, setGridApi] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [dialog, setDialog] = useState(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [deleteError, setDeleteError] = useState('')
  const [deleting, setDeleting] = useState(false)

  const loadInvoices = useCallback(async () => {
    setLoading(true)
    setError('')

    try {
      setInvoices(await fetchInvoices())
    } catch (loadError) {
      setError(loadError instanceof Error ? loadError.message : '無法載入發票管理資料。')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    loadInvoices()
  }, [loadInvoices])

  const columnDefs = useMemo(
    () => [
      { field: 'invoiceId', headerName: '編號', width: 100, sort: 'desc' },
      { field: 'taxId', headerName: '統編', width: 140 },
      { field: 'clientShortName', headerName: '客戶簡稱', flex: 1, minWidth: 150 },
      ...invoiceOptionColumns,
    ],
    [],
  )

  const clearSelection = useCallback(() => {
    gridApi?.deselectAll()
    setSelectedInvoice(null)
  }, [gridApi])

  const handlePaginationChanged = useCallback((api) => {
    api.deselectAll()
    setSelectedInvoice(null)
  }, [])

  const handleSaved = useCallback(async () => {
    await loadInvoices()
    setDialog(null)
    clearSelection()
  }, [clearSelection, loadInvoices])

  const handleDelete = async () => {
    if (!selectedInvoice) return

    setDeleting(true)
    setDeleteError('')
    try {
      await deleteInvoice(selectedInvoice.invoiceId)
      setDeleteDialogOpen(false)
      clearSelection()
      await loadInvoices()
    } catch (deleteRequestError) {
      setDeleteError(
        deleteRequestError instanceof Error
          ? deleteRequestError.message
          : '刪除發票管理資料失敗。',
      )
    } finally {
      setDeleting(false)
    }
  }

  const handleRowDoubleClicked = (invoice) => {
    setSelectedInvoice(invoice)
    setDialog({ mode: 'edit', invoice })
  }

  return (
    <section className="management-page" aria-labelledby="invoice-page-title">
      <header className="page-header">
        <div>
          <p className="eyebrow">VAT ADMINISTRATION</p>
          <Typography component="h1" id="invoice-page-title" variant="h3">
            發票管理
          </Typography>
          <p className="intro">管理客戶適用的電子發票、收銀機與紙本發票方式。</p>
        </div>
        <Typography className="record-count" variant="body2">
          共 {invoices.length} 筆
        </Typography>
      </header>

      <Stack className="toolbar" direction="row" spacing={1}>
        <Button variant="contained" onClick={() => setDialog({ mode: 'create' })}>
          新增
        </Button>
        <Button
          variant="outlined"
          disabled={!selectedInvoice}
          onClick={() => setDialog({ mode: 'edit', invoice: selectedInvoice })}
        >
          修改
        </Button>
        <Button
          color="error"
          variant="outlined"
          disabled={!selectedInvoice}
          onClick={() => {
            setDeleteError('')
            setDeleteDialogOpen(true)
          }}
        >
          刪除
        </Button>
        <Button variant="text" onClick={loadInvoices} disabled={loading}>
          重新整理
        </Button>
      </Stack>

      {error && (
        <Alert severity="error" className="page-alert" onClose={() => setError('')}>
          {error}
        </Alert>
      )}

      {loading ? (
        <div className="grid-message" role="status">載入發票管理資料中…</div>
      ) : (
        <ManagementGrid
          rowData={invoices}
          columnDefs={columnDefs}
          rowIdField="invoiceId"
          onGridReady={setGridApi}
          onSelectionChanged={setSelectedInvoice}
          onRowDoubleClicked={handleRowDoubleClicked}
          enablePagination
          onPaginationChanged={handlePaginationChanged}
          noRowsMessage="目前沒有發票管理資料。"
        />
      )}

      <InvoiceDialog
        open={Boolean(dialog)}
        mode={dialog?.mode || 'create'}
        invoice={dialog?.invoice}
        onClose={() => setDialog(null)}
        onSaved={handleSaved}
      />

      <Dialog
        open={deleteDialogOpen}
        onClose={deleting ? undefined : () => setDeleteDialogOpen(false)}
        aria-labelledby="invoice-delete-dialog-title"
      >
        <DialogTitle id="invoice-delete-dialog-title">確認刪除發票管理資料</DialogTitle>
        <DialogContent>
          <DialogContentText>
            確定要刪除「{selectedInvoice?.clientShortName}」的發票管理資料嗎？此動作無法復原。
          </DialogContentText>
          {deleteError && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {deleteError}
            </Alert>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)} disabled={deleting}>
            取消
          </Button>
          <Button color="error" variant="contained" onClick={handleDelete} disabled={deleting}>
            {deleting ? '刪除中…' : '確認刪除'}
          </Button>
        </DialogActions>
      </Dialog>
    </section>
  )
}
