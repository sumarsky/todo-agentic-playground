import { useContext } from 'react';
import { TodoContext } from '../context/TodoContextValue';

export function useTodoFilterControls() {
  const { filters, setCompletedFilter, listTodos } = useContext(TodoContext);

  const toggleCompletedFilter = () => {
    const completed = !filters.completed;
    const nextFilters = { ...filters, completed };

    setCompletedFilter(completed);
    listTodos(nextFilters);
  };

  return {
    filters,
    toggleCompletedFilter,
  };
}
