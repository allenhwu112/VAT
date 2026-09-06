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
import { deleteEmployee, fetchEmployees } from '../api/employees'
import { EmployeeDialog } from '../components/EmployeeDialog'
import { ManagementGrid } from '../components/ManagementGrid'

const genderLabels = { M: '男', F: '女' }

export function EmployeesPage() {
  const [employees, setEmployees] = useState([])
  const [selectedEmployee, setSelectedEmployee] = useState(null)
  const [gridApi, setGridApi] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [dialog, setDialog] = useState(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [deleteError, setDeleteError] = useState('')
  const [deleting, setDeleting] = useState(false)

  const loadEmployees = useCallback(async () => {
    setLoading(true)
    setError('')

    try {
      setEmployees(await fetchEmployees())
    } catch (loadError) {
      setError(loadError instanceof Error ? loadError.message : '無法載入員工資料。')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    loadEmployees()
  }, [loadEmployees])

  const columnDefs = useMemo(
    () => [
      { field: 'employeeId', headerName: '編號', width: 100, sort: 'desc' },
      { field: 'name', headerName: '姓名', flex: 1, minWidth: 150 },
      { field: 'shortName', headerName: '簡稱', flex: 1, minWidth: 120 },
      {
        field: 'gender',
        headerName: '性別',
        width: 110,
        valueFormatter: ({ value }) => genderLabels[value] || value,
      },
      { field: 'nationalId', headerName: '身份證字號', flex: 1, minWidth: 180 },
      { field: 'contactPhone', headerName: '聯絡電話', flex: 1, minWidth: 150 },
      { field: 'address', headerName: '地址', flex: 2, minWidth: 220 },
      { field: 'birthDate', headerName: '出生年月日', width: 150 },
    ],
    [],
  )

  const clearSelection = useCallback(() => {
    gridApi?.deselectAll()
    setSelectedEmployee(null)
  }, [gridApi])

  const handleSaved = useCallback(async () => {
    await loadEmployees()
    setDialog(null)
    clearSelection()
  }, [clearSelection, loadEmployees])

  const handleDelete = async () => {
    if (!selectedEmployee) return

    setDeleting(true)
    setDeleteError('')
    try {
      await deleteEmployee(selectedEmployee.employeeId)
      setDeleteDialogOpen(false)
      clearSelection()
      await loadEmployees()
    } catch (deleteRequestError) {
      setDeleteError(
        deleteRequestError instanceof Error
          ? deleteRequestError.message
          : '刪除員工資料失敗。',
      )
    } finally {
      setDeleting(false)
    }
  }

  const handleRowDoubleClicked = (employee) => {
    setSelectedEmployee(employee)
    setDialog({ mode: 'edit', employee })
  }

  return (
    <section className="management-page" aria-labelledby="employee-page-title">
      <header className="page-header">
        <div>
          <p className="eyebrow">VAT ADMINISTRATION</p>
          <Typography component="h1" id="employee-page-title" variant="h3">
            員工管理
          </Typography>
          <p className="intro">管理 VAT 系統員工基本資料與登入密碼。</p>
        </div>
        <Typography className="record-count" variant="body2">
          共 {employees.length} 筆
        </Typography>
      </header>

      <Stack className="toolbar" direction="row" spacing={1}>
        <Button variant="contained" onClick={() => setDialog({ mode: 'create' })}>
          新增
        </Button>
        <Button
          variant="outlined"
          disabled={!selectedEmployee}
          onClick={() => setDialog({ mode: 'edit', employee: selectedEmployee })}
        >
          修改
        </Button>
        <Button
          color="error"
          variant="outlined"
          disabled={!selectedEmployee}
          onClick={() => {
            setDeleteError('')
            setDeleteDialogOpen(true)
          }}
        >
          刪除
        </Button>
        <Button variant="text" onClick={loadEmployees} disabled={loading}>
          重新整理
        </Button>
      </Stack>

      {error && (
        <Alert severity="error" className="page-alert" onClose={() => setError('')}>
          {error}
        </Alert>
      )}

      {loading ? (
        <div className="grid-message" role="status">載入員工資料中…</div>
      ) : (
        <ManagementGrid
          rowData={employees}
          columnDefs={columnDefs}
          rowIdField="employeeId"
          onGridReady={setGridApi}
          onSelectionChanged={setSelectedEmployee}
          onRowDoubleClicked={handleRowDoubleClicked}
          noRowsMessage="目前沒有員工資料。"
        />
      )}

      <EmployeeDialog
        open={Boolean(dialog)}
        mode={dialog?.mode || 'create'}
        employee={dialog?.employee}
        onClose={() => setDialog(null)}
        onSaved={handleSaved}
      />

      <Dialog
        open={deleteDialogOpen}
        onClose={deleting ? undefined : () => setDeleteDialogOpen(false)}
        aria-labelledby="employee-delete-dialog-title"
      >
        <DialogTitle id="employee-delete-dialog-title">確認刪除員工</DialogTitle>
        <DialogContent>
          <DialogContentText>
            確定要刪除「{selectedEmployee?.name}」的員工資料嗎？此動作無法復原。
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
