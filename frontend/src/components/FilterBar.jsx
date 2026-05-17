import { useContext } from 'react';
import { Filter } from 'lucide-react';
import { TodoContext } from '../context/TodoContextValue';

export const FilterBar = () => {
  const { filters, setCompletedFilter, setSearchFilter, listTodos } = useContext(TodoContext);

  const handleCompletedClick = () => {
    const completed = !filters.completed;
    const nextFilters = { ...filters, completed };
    setCompletedFilter(completed);
    listTodos(nextFilters);
  };

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
        onClick={handleCompletedClick}
      >
        <Filter size={18} />
        <span className="sr-only">Show completed todos</span>
      </button>
    </div>
  );
};
