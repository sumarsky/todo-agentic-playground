import { createContext } from 'react';

export const TodoContext = createContext({
  todos: [],
  loading: false,
  error: null,
  listTodos: async () => [],
  addTodo: async () => [],
  updateTodo: async () => [],
  deleteTodo: async () => [],
  bulkDeleteTodos: async () => [],
});
