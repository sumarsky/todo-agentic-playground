import { Routes, Route } from 'react-router-dom'
import { TodoContextProvider } from './context/TodoContext'
import { TodoList } from './components/TodoList'
import { DashboardPage } from './components/DashboardPage'
import './App.css'

function App() {
  return (
    <Routes>
      <Route path="/" element={
        <TodoContextProvider>
          <main className="todo-app">
            <h1>Todos App</h1>
            <TodoList />
          </main>
        </TodoContextProvider>
      } />
      <Route path="/dashboard" element={<DashboardPage />} />
    </Routes>
  )
}

export default App
