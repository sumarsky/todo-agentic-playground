import { TodoContextProvider } from './context/TodoContext'
import { TodoForm } from './components/TodoForm'
import { TodoList } from './components/TodoList'
import './App.css'

function App() {
  return (
    <TodoContextProvider>
      <main className="todo-app">
        <h1>Todos App</h1>
        <TodoForm />
        <TodoList />
      </main>
    </TodoContextProvider>
  )
}

export default App
