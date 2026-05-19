import { useState, useEffect } from 'react'

const API_BASE_URL = 'http://localhost:5000'

export function LogsPage() {
  const [logs, setLogs] = useState([])
  const [level, setLevel] = useState('')
  const [message, setMessage] = useState('')

  useEffect(() => {
    const fetchLogs = async () => {
      const params = new URLSearchParams()
      if (level) params.set('level', level)
      if (message) params.set('message', message)
      const query = params.toString()
      const url = `${API_BASE_URL}/api/logs${query ? `?${query}` : ''}`
      const res = await fetch(url)
      const data = await res.json()
      const sorted = [...data].sort((a, b) => new Date(b.timestamp) - new Date(a.timestamp))
      setLogs(sorted)
    }
    fetchLogs()
  }, [level, message])

  return (
    <main className="logs-page">
      <h1>Logs</h1>
      <div className="logs-filters">
        <select
          value={level}
          onChange={(e) => setLevel(e.target.value)}
          aria-label="Level"
        >
          <option value="">All</option>
          <option value="Info">Info</option>
          <option value="Warning">Warning</option>
          <option value="Error">Error</option>
        </select>
        <input
          type="text"
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          placeholder="Search messages"
          aria-label="Search messages"
        />
      </div>
      {logs.length === 0 ? (
        <div className="empty-state">No logs found</div>
      ) : (
        <div className="logs-list">
          {logs.map((log) => (
            <div key={log.id} className="log-entry" role="article">
              <span className={`level-${log.level.toLowerCase()}`}>{log.level}</span>
              <time>{new Date(log.timestamp).toLocaleString()}</time>
              <span className="source">{log.source}</span>
              <span className="message">{log.message}</span>
              {log.httpMethod && <span className="http-method">{log.httpMethod}</span>}
              {log.httpPath && <span className="http-path">{log.httpPath}</span>}
              {log.httpStatus != null && <span className="http-status">{log.httpStatus}</span>}
            </div>
          ))}
        </div>
      )}
    </main>
  )
}
