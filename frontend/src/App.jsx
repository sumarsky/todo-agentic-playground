import { TodoContextProvider } from './context/TodoContext'
import { TodoList } from './components/TodoList'
import './App.css'

function App() {
  return (
    <TodoContextProvider>
      <main className="todo-app">
        <h1>Todos App</h1>
        <TodoList />
      </main>
    </TodoContextProvider>
  )
}

export default App
