import { useEffect, useRef } from 'react';
import { Search, Plus, Filter } from 'lucide-react';
import { useTodoFilterControls } from '../hooks/useTodoFilterControls';

export const Toolbar = ({ onAddClick, onSelectAll, isAllSelected, isIndeterminate }) => {
  const { filters, toggleCompletedFilter, updateSearchFilter } = useTodoFilterControls();
  const checkboxRef = useRef(null);

  useEffect(() => {
    if (checkboxRef.current) {
      checkboxRef.current.indeterminate = isIndeterminate;
    }
  }, [isIndeterminate]);

  const handleSearchChange = (event) => {
    updateSearchFilter(event.target.value);
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
      <button type="button" onClick={onAddClick} title="Add">
        <Plus size={18} />
        <span className="sr-only">Add todo</span>
      </button>
      <button type="button" aria-pressed={filters.completed} onClick={toggleCompletedFilter} title="Toggle completed">
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
