import { createContext } from 'react';

export const TodoContext = createContext({
  todos: [],
  loading: false,
  error: null,
  filters: { completed: false, search: '' },
  listTodos: async () => [],
  setCompletedFilter: () => {},
  setSearchFilter: () => {},
  addTodo: async () => [],
  updateTodo: async () => [],
  deleteTodo: async () => [],
  bulkDeleteTodos: async () => [],
});
