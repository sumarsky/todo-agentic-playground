import { useState } from 'react';

export const useTodoSelection = () => {
  const [selectedIds, setSelectedIds] = useState([]);

  const selectAll = (count) => {
    const ids = Array.from({ length: count }, (_, i) => i + 1);
    setSelectedIds(ids);
  };

  const deselectAll = () => {
    setSelectedIds([]);
  };

  const selectTodo = (id) => {
    setSelectedIds((current) => current.includes(id) ? current : [...current, id]);
  };

  const deselectTodo = (id) => {
    setSelectedIds((current) => current.filter((todoId) => todoId !== id));
  };

  return { selectedIds, selectAll, deselectAll, selectTodo, deselectTodo };
};
