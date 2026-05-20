import { useState, useCallback } from 'react';
import { apiClient } from '../api/apiClient';

export function useTodoApi({ apiClient: injectedClient, initialTodos = [] } = {}) {
  const [todos, setTodos] = useState(initialTodos);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const client = injectedClient || apiClient;

  const withMutation = useCallback(async (fn, onSuccess) => {
    setLoading(true);
    setError(null);
    try {
      const result = await fn();
      if (onSuccess) onSuccess(result);
      return result;
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const listTodos = useCallback((filters = {}) =>
    withMutation(
      () => client.listTodos(filters),
      (data) => setTodos(data)
    ), [client, withMutation]);

  const addTodo = useCallback((title) =>
    withMutation(
      () => client.createTodo(title),
      (todo) => setTodos(prev => [...prev, todo])
    ), [client, withMutation]);

  const updateTodo = useCallback((id, title) =>
    withMutation(
      () => client.updateTodoTitle(id, title),
      (updated) => setTodos(prev => prev.map(t => t.id === id ? updated : t))
    ), [client, withMutation]);

  const deleteTodo = useCallback((id) =>
    withMutation(
      () => client.deleteTodo(id),
      () => setTodos(prev => prev.filter(t => t.id !== id))
    ), [client, withMutation]);

  const bulkDeleteTodos = useCallback((ids) =>
    withMutation(
      () => client.bulkDeleteTodos(ids),
      () => setTodos(prev => prev.filter(t => !ids.includes(t.id)))
    ), [client, withMutation]);

  return { todos, loading, error, listTodos, addTodo, updateTodo, deleteTodo, bulkDeleteTodos };
}
