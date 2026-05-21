import { useContext } from 'react';
import { Filter } from 'lucide-react';
import { TodoContext } from '../context/TodoContextValue';
import { useTodoFilterControls } from '../hooks/useTodoFilterControls';

export const FilterBar = () => {
  const { setSearchFilter, listTodos } = useContext(TodoContext);
  const { filters, toggleCompletedFilter } = useTodoFilterControls();

  const handleSearchChange = (event) => {
    const search = event.target.value;
    const nextFilters = { ...filters, search };
    setSearchFilter(search);
    listTodos(nextFilters);
  };

  return (
    <div className="filter-bar">
      <label htmlFor="todo-search">Search todos</label>
      <input
        id="todo-search"
        type="text"
        value={filters.search}
        onChange={handleSearchChange}
      />
      <button
        type="button"
        aria-pressed={filters.completed}
        title="Toggle completed"
        onClick={toggleCompletedFilter}
      >
        <Filter size={18} />
        <span className="sr-only">Show completed todos</span>
      </button>
    </div>
  );
};
