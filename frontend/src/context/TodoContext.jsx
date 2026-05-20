import { useEffect, useRef } from 'react';
import { TodoContext } from './TodoContextValue';
import { useTodoApi } from '../hooks/useTodoApi';
import { useTodoFilters } from '../hooks/useTodoFilters';
import { useTodoSelection } from '../hooks/useTodoSelection';
import { apiClient } from '../api/apiClient';

export const TodoContextProvider = ({ children, initialTodos = [] }) => {
  const { todos, loading, error, listTodos: apiListTodos, addTodo: apiAddTodo, updateTodo: apiUpdateTodo, deleteTodo: apiDeleteTodo, bulkDeleteTodos: apiBulkDeleteTodos } = useTodoApi({ initialTodos });
  const { filters: rawFilters, setCompletedFilter, setSearchFilter } = useTodoFilters();
  const { selectedIds, selectAll: apiSelectAll, deselectAll, selectTodo, deselectTodo } = useTodoSelection();
  const todosRef = useRef(todos);

  const filters = { completed: rawFilters.completed ?? false, search: rawFilters.search ?? '' };

  useEffect(() => {
    todosRef.current = todos;
  }, [todos]);

  useEffect(() => {
    if (initialTodos.length === 0) {
      listTodos();
    }
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  const listTodos = async (nextFilters = filters) => {
    const result = await apiListTodos(nextFilters);
    return result;
  };

  const addTodo = async (title) => {
    const newTodo = await apiAddTodo(title);
    return [...todosRef.current, newTodo];
  };

  const updateTodo = async (id, updates) => {
    const currentTodo = todosRef.current.find((todo) => todo.id === id);
    if (!currentTodo) {
      throw new Error('Todo not found');
    }

    let updatedTodo = currentTodo;

    if (updates.title !== currentTodo.title) {
      updatedTodo = await apiUpdateTodo(id, updates.title);
    }

    if (updates.completed !== currentTodo.completed) {
      updatedTodo = await apiClient.toggleTodo(id);
    }

    const newTodos = todosRef.current.map((todo) => (todo.id === id ? updatedTodo : todo));
    return newTodos;
  };

  const deleteTodo = async (id) => {
    await apiDeleteTodo(id);
    return todosRef.current.filter((todo) => todo.id !== id);
  };

  const bulkDeleteTodos = async (ids) => {
    await apiBulkDeleteTodos(ids);
    return todosRef.current.filter((todo) => !ids.includes(todo.id));
  };

  const selectAll = () => {
    apiSelectAll(todos.length);
  };

  const value = {
    todos,
    loading,
    error,
    filters,
    selectedIds,
    listTodos,
    setCompletedFilter,
    setSearchFilter,
    addTodo,
    updateTodo,
    deleteTodo,
    bulkDeleteTodos,
    selectAll,
    deselectAll,
    selectTodo,
    deselectTodo,
  };

  return (
    <TodoContext.Provider value={value}>
      {children}
    </TodoContext.Provider>
  );
};
