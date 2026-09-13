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
import { deleteClient, fetchClients } from '../api/clients'
import { ClientDialog } from '../components/ClientDialog'
import { ManagementGrid } from '../components/ManagementGrid'

export function ClientsPage() {
  const [clients, setClients] = useState([])
  const [selectedClient, setSelectedClient] = useState(null)
  const [gridApi, setGridApi] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [dialog, setDialog] = useState(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [deleteError, setDeleteError] = useState('')
  const [deleting, setDeleting] = useState(false)

  const loadClients = useCallback(async () => {
    setLoading(true)
    setError('')

    try {
      setClients(await fetchClients())
    } catch (loadError) {
      setError(loadError instanceof Error ? loadError.message : '無法載入客戶資料。')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    loadClients()
  }, [loadClients])

  const columnDefs = useMemo(
    () => [
      { field: 'clientCode', headerName: '客編', width: 100 },
      { field: 'taxId', headerName: '統編', width: 140 },
      { field: 'fullName', headerName: '客戶全稱', flex: 2, minWidth: 220 },
      { field: 'shortName', headerName: '簡稱', flex: 1, minWidth: 140 },
      { field: 'responsiblePerson', headerName: '負責人', flex: 1, minWidth: 150 },
      { field: 'address', headerName: '地址', flex: 2, minWidth: 240 },
    ],
    [],
  )

  const clearSelection = useCallback(() => {
    gridApi?.deselectAll()
    setSelectedClient(null)
  }, [gridApi])

  const handleSaved = useCallback(async () => {
    await loadClients()
    setDialog(null)
    clearSelection()
  }, [clearSelection, loadClients])

  const handleDelete = async () => {
    if (!selectedClient) return

    setDeleting(true)
    setDeleteError('')
    try {
      await deleteClient(selectedClient.clientId)
      setDeleteDialogOpen(false)
      clearSelection()
      await loadClients()
    } catch (deleteRequestError) {
      setDeleteError(
        deleteRequestError instanceof Error
          ? deleteRequestError.message
          : '刪除客戶資料失敗。',
      )
    } finally {
      setDeleting(false)
    }
  }

  const handleRowDoubleClicked = (client) => {
    setSelectedClient(client)
    setDialog({ mode: 'edit', client })
  }

  return (
    <section className="management-page" aria-labelledby="client-page-title">
      <header className="page-header">
        <div>
          <p className="eyebrow">VAT ADMINISTRATION</p>
          <Typography component="h1" id="client-page-title" variant="h3">
            客戶管理
          </Typography>
          <p className="intro">管理 VAT 系統客戶主檔資料。</p>
        </div>
        <Typography className="record-count" variant="body2">
          共 {clients.length} 筆
        </Typography>
      </header>

      <Stack className="toolbar" direction="row" spacing={1}>
        <Button variant="contained" onClick={() => setDialog({ mode: 'create' })}>
          新增
        </Button>
        <Button
          variant="outlined"
          disabled={!selectedClient}
          onClick={() => setDialog({ mode: 'edit', client: selectedClient })}
        >
          修改
        </Button>
        <Button
          color="error"
          variant="outlined"
          disabled={!selectedClient}
          onClick={() => {
            setDeleteError('')
            setDeleteDialogOpen(true)
          }}
        >
          刪除
        </Button>
        <Button variant="text" onClick={loadClients} disabled={loading}>
          重新整理
        </Button>
      </Stack>

      {error && (
        <Alert severity="error" className="page-alert" onClose={() => setError('')}>
          {error}
        </Alert>
      )}

      {loading ? (
        <div className="grid-message" role="status">載入客戶資料中…</div>
      ) : (
        <ManagementGrid
          rowData={clients}
          columnDefs={columnDefs}
          rowIdField="clientId"
          onGridReady={setGridApi}
          onSelectionChanged={setSelectedClient}
          onRowDoubleClicked={handleRowDoubleClicked}
          noRowsMessage="目前沒有客戶資料。"
        />
      )}

      <ClientDialog
        open={Boolean(dialog)}
        mode={dialog?.mode || 'create'}
        client={dialog?.client}
        onClose={() => setDialog(null)}
        onSaved={handleSaved}
      />

      <Dialog
        open={deleteDialogOpen}
        onClose={deleting ? undefined : () => setDeleteDialogOpen(false)}
        aria-labelledby="client-delete-dialog-title"
      >
        <DialogTitle id="client-delete-dialog-title">確認刪除客戶</DialogTitle>
        <DialogContent>
          <DialogContentText>
            確定要刪除「{selectedClient?.fullName}」的客戶資料嗎？此動作無法復原。
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
