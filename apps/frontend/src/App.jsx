import { useCallback, useEffect, useState } from 'react'
import { fetchHealth } from './api/health'
import './styles.css'

const initialHealth = {
  status: 'loading',
  data: null,
  error: null,
}

function formatTimestamp(timestamp) {
  return new Intl.DateTimeFormat('zh-TW', {
    dateStyle: 'medium',
    timeStyle: 'medium',
  }).format(new Date(timestamp))
}

function App() {
  const [health, setHealth] = useState(initialHealth)

  const loadHealth = useCallback(async () => {
    setHealth({ status: 'loading', data: null, error: null })

    try {
      const data = await fetchHealth()
      setHealth({ status: 'success', data, error: null })
    } catch (error) {
      setHealth({
        status: 'error',
        data: null,
        error: error instanceof Error ? error.message : '無法連線到 API。',
      })
    }
  }, [])

  useEffect(() => {
    loadHealth()
  }, [loadHealth])

  const statusLabel = {
    loading: '檢查 API 連線中…',
    success: 'API 連線正常',
    error: 'API 尚未連線',
  }[health.status]

  return (
    <main className="app-shell">
      <section className="status-card" aria-labelledby="page-title">
        <p className="eyebrow">VAT MONOREPO</p>
        <h1 id="page-title">開發環境已就緒</h1>
        <p className="intro">
          React + Vite 前端正在確認 .NET 10 Controllers API 的連線狀態。
        </p>

        <div
          className={`status-banner status-${health.status}`}
          role="status"
          aria-live="polite"
        >
          <span className="status-dot" aria-hidden="true" />
          <span>{statusLabel}</span>
        </div>

        {health.data && (
          <dl className="health-details">
            <div>
              <dt>服務</dt>
              <dd>{health.data.service}</dd>
            </div>
            <div>
              <dt>回應時間</dt>
              <dd>{formatTimestamp(health.data.timestamp)}</dd>
            </div>
          </dl>
        )}

        {health.error && (
          <p className="error-message" role="alert">
            {health.error}
          </p>
        )}

        <button type="button" onClick={loadHealth} disabled={health.status === 'loading'}>
          {health.status === 'loading' ? '檢查中…' : '重新檢查 API'}
        </button>
      </section>
    </main>
  )
}

export default App
