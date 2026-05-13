import { useEffect, useRef, useState } from 'react';
import { TodoContext } from './TodoContextValue';

const API_BASE_URL = 'http://localhost:5000';

export const TodoContextProvider = ({ children, initialTodos = [] }) => {
  const [todos, setTodos] = useState(initialTodos);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [filters, setFilters] = useState({ completed: false, search: '' });
  const todosRef = useRef(todos);

  useEffect(() => {
    todosRef.current = todos;
  }, [todos]);

  const runTodoRequest = async (request, nextTodos) => {
    setLoading(true);
    setError(null);
    try {
      const result = await request();
      const updated = nextTodos(result, todosRef.current);
      todosRef.current = updated;
      setTodos(updated);
      return updated;
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const listTodos = async (nextFilters = filters) =>
    runTodoRequest(async () => {
      const params = new URLSearchParams();
      if (nextFilters.completed) {
        params.set('completed', 'true');
      }
      if (nextFilters.search) {
        params.set('search', nextFilters.search);
      }
      const query = params.toString();
      const url = `${API_BASE_URL}/todos${query ? `?${query}` : ''}`;
      const res = await fetch(url);
      return res.json();
    }, (data) => data);

  const setCompletedFilter = (completed) => {
    setFilters((current) => ({ ...current, completed }));
  };

  const setSearchFilter = (search) => {
    setFilters((current) => ({ ...current, search }));
  };

  const addTodo = async (title) =>
    runTodoRequest(async () => {
      const res = await fetch(`${API_BASE_URL}/todos`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ title }),
      });
      return res.json();
    }, (newTodo, current) => [...current, newTodo]);

  const updateTodo = async (id, updates) =>
    runTodoRequest(async () => {
      const currentTodo = todosRef.current.find((todo) => todo.id === id);
      if (!currentTodo) {
        throw new Error('Todo not found');
      }

      let updatedTodo = currentTodo;

      if (updates.title !== currentTodo.title) {
        const res = await fetch(`${API_BASE_URL}/todos/${id}/title`, {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ title: updates.title }),
        });
        updatedTodo = await res.json();
      }

      if (updates.completed !== currentTodo.completed) {
        const res = await fetch(`${API_BASE_URL}/todos/${id}/toggle`, {
          method: 'PUT',
        });
        updatedTodo = await res.json();
      }

      return updatedTodo;
    }, (updatedTodo, current) => current.map((todo) => (todo.id === id ? updatedTodo : todo)));

  const deleteTodo = async (id) =>
    runTodoRequest(async () => {
      await fetch(`${API_BASE_URL}/todos/${id}`, {
        method: 'DELETE',
      });
    }, (_, current) => current.filter((todo) => todo.id !== id));

  const bulkDeleteTodos = async (ids) =>
    runTodoRequest(async () => {
      const params = new URLSearchParams();
      params.set('ids', ids.join(','));
      await fetch(`${API_BASE_URL}/todos?${params.toString()}`, {
        method: 'DELETE',
      });
    }, (_, current) => current.filter((todo) => !ids.includes(todo.id)));

  const value = {
    todos,
    loading,
    error,
    filters,
    listTodos,
    setCompletedFilter,
    setSearchFilter,
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
