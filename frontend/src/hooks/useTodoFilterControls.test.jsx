import { describe, it, expect, vi } from 'vitest';
import { act, renderHook } from '@testing-library/react';
import { TodoContext } from '../context/TodoContextValue';
import { useTodoFilterControls } from './useTodoFilterControls';

const renderUseTodoFilterControls = (contextOverrides = {}) => {
  const value = {
    todos: [],
    loading: false,
    error: null,
    filters: { completed: false, search: '' },
    selectedIds: [],
    listTodos: vi.fn(),
    setCompletedFilter: vi.fn(),
    setSearchFilter: vi.fn(),
    addTodo: vi.fn(),
    updateTodo: vi.fn(),
    deleteTodo: vi.fn(),
    bulkDeleteTodos: vi.fn(),
    selectAll: vi.fn(),
    deselectAll: vi.fn(),
    selectTodo: vi.fn(),
    deselectTodo: vi.fn(),
    ...contextOverrides,
  };

  const wrapper = ({ children }) => (
    <TodoContext.Provider value={value}>
      {children}
    </TodoContext.Provider>
  );

  return {
    ...renderHook(() => useTodoFilterControls(), { wrapper }),
    value,
  };
};

describe('useTodoFilterControls', () => {
  it('returns the current filters from todo context', () => {
    const filters = { completed: true, search: 'docs' };

    const { result } = renderUseTodoFilterControls({ filters });

    expect(result.current.filters).toBe(filters);
  });

  it('toggles completed on and loads todos with merged filters', () => {
    const setCompletedFilter = vi.fn();
    const listTodos = vi.fn();

    const { result } = renderUseTodoFilterControls({
      filters: { completed: false, search: 'docs' },
      setCompletedFilter,
      listTodos,
    });

    act(() => result.current.toggleCompletedFilter());

    expect(setCompletedFilter).toHaveBeenCalledWith(true);
    expect(listTodos).toHaveBeenCalledWith({ completed: true, search: 'docs' });
  });

  it('toggles completed off and loads todos with merged filters', () => {
    const setCompletedFilter = vi.fn();
    const listTodos = vi.fn();

    const { result } = renderUseTodoFilterControls({
      filters: { completed: true, search: 'docs' },
      setCompletedFilter,
      listTodos,
    });

    act(() => result.current.toggleCompletedFilter());

    expect(setCompletedFilter).toHaveBeenCalledWith(false);
    expect(listTodos).toHaveBeenCalledWith({ completed: false, search: 'docs' });
  });

  it('updates search and loads todos with merged filters', () => {
    const setSearchFilter = vi.fn();
    const listTodos = vi.fn();

    const { result } = renderUseTodoFilterControls({
      filters: { completed: true, search: 'docs' },
      setSearchFilter,
      listTodos,
    });

    act(() => result.current.updateSearchFilter('api'));

    expect(setSearchFilter).toHaveBeenCalledWith('api');
    expect(listTodos).toHaveBeenCalledWith({ completed: true, search: 'api' });
  });
});
