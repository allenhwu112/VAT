const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000').replace(
  /\/+$/,
  '',
)

export async function fetchHealth() {
  const response = await fetch(`${apiBaseUrl}/VAT_API/health`, {
    headers: {
      Accept: 'application/json',
    },
  })

  if (!response.ok) {
    throw new Error(`API 回應失敗（HTTP ${response.status}）。`)
  }

  return response.json()
}
