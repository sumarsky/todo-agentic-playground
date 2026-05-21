import { useContext } from 'react';
import { TodoContext } from '../context/TodoContextValue';

export function useTodoFilterControls() {
  const { filters, setCompletedFilter, setSearchFilter, listTodos } = useContext(TodoContext);

  const toggleCompletedFilter = () => {
    const completed = !filters.completed;
    const nextFilters = { ...filters, completed };

    setCompletedFilter(completed);
    listTodos(nextFilters);
  };

  const updateSearchFilter = (search) => {
    const nextFilters = { ...filters, search };

    setSearchFilter(search);
    listTodos(nextFilters);
  };

  return {
    filters,
    toggleCompletedFilter,
    updateSearchFilter,
  };
}
