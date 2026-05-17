import { useContext, useEffect, useRef } from 'react';
import { Search, Plus, Filter } from 'lucide-react';
import { TodoContext } from '../context/TodoContextValue';

export const Toolbar = ({ onAddClick, onSelectAll, isAllSelected, isIndeterminate }) => {
  const { filters, setSearchFilter, setCompletedFilter, listTodos } = useContext(TodoContext);
  const checkboxRef = useRef(null);

  useEffect(() => {
    if (checkboxRef.current) {
      checkboxRef.current.indeterminate = isIndeterminate;
    }
  }, [isIndeterminate]);

  const handleSearchChange = (event) => {
    const search = event.target.value;
    const nextFilters = { ...filters, search };
    setSearchFilter(search);
    listTodos(nextFilters);
  };

  const handleFilterClick = () => {
    const completed = !filters.completed;
    const nextFilters = { ...filters, completed };
    setCompletedFilter(completed);
    listTodos(nextFilters);
  };

  return (
    <div className="toolbar">
      <label htmlFor="todo-search" className="sr-only">Search todos</label>
      <Search size={18} />
      <input
        id="todo-search"
        type="text"
        placeholder="Search todos"
        value={filters.search}
        onChange={handleSearchChange}
      />
      <button type="button" onClick={onAddClick}>
        <Plus size={18} />
        <span className="sr-only">Add todo</span>
      </button>
      <button type="button" aria-pressed={filters.completed} onClick={handleFilterClick}>
        <Filter size={18} />
        <span className="sr-only">Filter completed</span>
      </button>
      <input
        ref={checkboxRef}
        type="checkbox"
        checked={isAllSelected}
        onChange={onSelectAll}
        aria-label="Select all"
      />
    </div>
  );
};
