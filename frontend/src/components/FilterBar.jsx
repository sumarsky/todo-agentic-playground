import { Filter } from 'lucide-react';
import { useTodoFilterControls } from '../hooks/useTodoFilterControls';

export const FilterBar = () => {
  const { filters, toggleCompletedFilter, updateSearchFilter } = useTodoFilterControls();

  const handleSearchChange = (event) => {
    updateSearchFilter(event.target.value);
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
