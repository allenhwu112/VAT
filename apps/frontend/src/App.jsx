import { useCallback, useEffect, useState } from 'react'
import { ClientsPage } from './pages/ClientsPage'
import { EmployeesPage } from './pages/EmployeesPage'
import { getUiPath, resolveUiPage } from './routes'
import './styles.css'

function App() {
  const [page, setPage] = useState(() => resolveUiPage(window.location.pathname))

  const navigate = useCallback((nextPage) => {
    const nextPath = getUiPath(nextPage)
    if (window.location.pathname !== nextPath) {
      window.history.pushState({}, '', nextPath)
    }
    setPage(nextPage)
  }, [])

  useEffect(() => {
    const handlePopState = () => {
      setPage(resolveUiPage(window.location.pathname))
    }

    window.addEventListener('popstate', handlePopState)
    return () => window.removeEventListener('popstate', handlePopState)
  }, [])

  useEffect(() => {
    const resolvedPage = resolveUiPage(window.location.pathname)
    const resolvedPath = getUiPath(resolvedPage)

    if (window.location.pathname !== resolvedPath) {
      window.history.replaceState({}, '', resolvedPath)
    }

    document.title = resolvedPage === 'clients' ? 'VAT 客戶管理' : 'VAT 員工管理'
  }, [page])

  const handleNavigation = (event, nextPage) => {
    event.preventDefault()
    navigate(nextPage)
  }

  return (
    <main className="app-shell">
      <nav className="app-navigation" aria-label="VAT 管理功能">
        <a
          className={page === 'employees' ? 'navigation-link active' : 'navigation-link'}
          href={getUiPath('employees')}
          aria-current={page === 'employees' ? 'page' : undefined}
          onClick={(event) => handleNavigation(event, 'employees')}
        >
          員工管理
        </a>
        <a
          className={page === 'clients' ? 'navigation-link active' : 'navigation-link'}
          href={getUiPath('clients')}
          aria-current={page === 'clients' ? 'page' : undefined}
          onClick={(event) => handleNavigation(event, 'clients')}
        >
          客戶管理
        </a>
      </nav>

      {page === 'clients' ? <ClientsPage /> : <EmployeesPage />}
    </main>
  )
}

export default App
