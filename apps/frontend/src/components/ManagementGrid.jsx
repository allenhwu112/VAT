import { useMemo } from 'react'
import { ModuleRegistry, themeQuartz } from 'ag-grid-community'
import { AllEnterpriseModule } from 'ag-grid-enterprise'
import { AgGridReact } from 'ag-grid-react'

ModuleRegistry.registerModules([AllEnterpriseModule])

export function ManagementGrid({
  rowData,
  columnDefs,
  rowIdField,
  onGridReady,
  onSelectionChanged,
  onRowDoubleClicked,
  noRowsMessage,
}) {
  const defaultColDef = useMemo(
    () => ({ sortable: true, filter: true, resizable: true }),
    [],
  )

  return (
    <div className="management-grid ag-theme-quartz">
      <AgGridReact
        rowData={rowData}
        columnDefs={columnDefs}
        defaultColDef={defaultColDef}
        rowSelection={{
          mode: 'singleRow',
          enableClickSelection: true,
          checkboxes: false,
        }}
        cellSelection
        getRowId={({ data }) => String(data[rowIdField])}
        onGridReady={({ api }) => onGridReady?.(api)}
        onSelectionChanged={({ api }) =>
          onSelectionChanged?.(api.getSelectedRows()[0] || null, api)
        }
        onRowDoubleClicked={({ data }) => data && onRowDoubleClicked?.(data)}
        theme={themeQuartz}
        overlayNoRowsTemplate={noRowsMessage}
      />
    </div>
  )
}
