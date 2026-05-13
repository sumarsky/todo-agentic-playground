import { useState } from 'react';
import { TodoContext } from './TodoContextValue';

const API_BASE_URL = 'http://localhost:5000';

export const TodoContextProvider = ({ children, initialTodos = [] }) => {
  const [todos, setTodos] = useState(initialTodos);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const runTodoRequest = async (request, nextTodos) => {
    setLoading(true);
    setError(null);
    try {
      const result = await request();
      const updated = nextTodos(result);
      setTodos(updated);
      return updated;
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const listTodos = async () =>
    runTodoRequest(async () => {
      const res = await fetch(`${API_BASE_URL}/todos`);
      return res.json();
    }, (data) => data);

  const addTodo = async (title) =>
    runTodoRequest(async () => {
      const res = await fetch(`${API_BASE_URL}/todos`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ title }),
      });
      return res.json();
    }, (newTodo) => [...todos, newTodo]);

  const updateTodo = async (id, updates) =>
    runTodoRequest(async () => {
      const res = await fetch(`${API_BASE_URL}/todos/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(updates),
      });
      return res.json();
    }, (updatedTodo) => todos.map((todo) => (todo.id === id ? updatedTodo : todo)));

  const deleteTodo = async (id) =>
    runTodoRequest(async () => {
      await fetch(`${API_BASE_URL}/todos/${id}`, {
        method: 'DELETE',
      });
    }, () => todos.filter((todo) => todo.id !== id));

  const bulkDeleteTodos = async (ids) =>
    runTodoRequest(async () => {
      await fetch(`${API_BASE_URL}/todos`, {
        method: 'DELETE',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ids }),
      });
    }, () => todos.filter((todo) => !ids.includes(todo.id)));

  const value = {
    todos,
    loading,
    error,
    listTodos,
    addTodo,
    updateTodo,
    deleteTodo,
    bulkDeleteTodos,
  };

  return (
    <TodoContext.Provider value={value}>
      {children}
    </TodoContext.Provider>
  );
};
