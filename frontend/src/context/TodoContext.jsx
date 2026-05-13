import { createContext } from 'react';

export const TodoContext = createContext({
  todos: [],
  loading: false,
  error: null,
});

export const TodoContextProvider = ({ children }) => {
  const value = {
    todos: [],
    loading: false,
    error: null,
  };

  return (
    <TodoContext.Provider value={value}>
      {children}
    </TodoContext.Provider>
  );
};
