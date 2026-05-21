import { useState, useEffect } from 'react'

import { FRONTEND_API_BASE_URL } from '../config/api'

export function DashboardPage() {
  const [metrics, setMetrics] = useState([])
  const [window, setWindow] = useState('24h')

  useEffect(() => {
    const fetchMetrics = async () => {
      const res = await fetch(`${FRONTEND_API_BASE_URL}/api/dashboard/metrics?window=${window}`)
      const data = await res.json()
      setMetrics(data)
    }
    fetchMetrics()
  }, [window])

  return (
    <main className="dashboard-page">
      <h1>Dashboard</h1>
      <select
        value={window}
        onChange={(e) => setWindow(e.target.value)}
        aria-label="Time window"
      >
        <option value="1h">1h</option>
        <option value="24h">24h</option>
        <option value="7d">7d</option>
        <option value="30d">30d</option>
      </select>
      {metrics.length === 0 ? (
        <div className="empty-state">No data for this time period</div>
      ) : (
        <div className="metrics-grid">
          {metrics.map((metric) => (
            <div key={metric.endpoint} className="metric-card">
              <h2>{metric.endpoint}</h2>
              <p>Failures: {metric.failureCount}</p>
              <p>Avg Duration: {metric.avgDurationMs}ms</p>
              <p>Total Requests: {metric.totalRequests}</p>
            </div>
          ))}
        </div>
      )}
    </main>
  )
}
