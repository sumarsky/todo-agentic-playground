import { TodoContextProvider } from './context/TodoContext'
import { FilterBar } from './components/FilterBar'
import { TodoForm } from './components/TodoForm'
import { TodoList } from './components/TodoList'
import './App.css'

function App() {
  return (
    <TodoContextProvider>
      <main className="todo-app">
        <h1>Todos App</h1>
        <FilterBar />
        <TodoForm />
        <TodoList />
      </main>
    </TodoContextProvider>
  )
}

export default App
