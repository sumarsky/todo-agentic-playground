import { TodoContextProvider } from './context/TodoContext'
import './App.css'

function App() {
  return (
    <TodoContextProvider>
      <div className="p-8">
        <h1>Todos App</h1>
      </div>
    </TodoContextProvider>
  )
}

export default App
