# Deepening Opportunity 5: Extract Frontend Filter State to Hook

**Severity:** 🟡 MEDIUM  
**Status:** Not started  
**Complexity:** Low

## Problem Statement

Filter state management logic is duplicated across multiple components. The same filter update logic exists in two places, violating DRY and creating maintenance friction.

### Current State

**Files:**
- `frontend/src/components/Toolbar.jsx` (filter logic)
- `frontend/src/components/FilterBar.jsx` (identical filter logic)

### Toolbar.jsx

```javascript
import { useTodoContext } from '../context/TodoContext';

export const Toolbar = () => {
  const { filters, setCompletedFilter, listTodos } = useTodoContext();

  const handleFilterClick = () => {
    const completed = !filters.completed;
    const nextFilters = { ...filters, completed };
    setCompletedFilter(completed);
    listTodos(nextFilters);
  };

  const handleSearchChange = (value) => {
    const nextFilters = { ...filters, search: value };
    listTodos(nextFilters);
  };

  return (
    <div>
      <button onClick={handleFilterClick}>
        {filters.completed ? 'Show All' : 'Show Completed'}
      </button>
      <input onChange={(e) => handleSearchChange(e.target.value)} />
    </div>
  );
};
```

### FilterBar.jsx

```javascript
import { useTodoContext } from '../context/TodoContext';

export const FilterBar = () => {
  const { filters, setCompletedFilter, listTodos } = useTodoContext();

  const handleCompletedClick = () => {
    const completed = !filters.completed;
    const nextFilters = { ...filters, completed };
    setCompletedFilter(completed);
    listTodos(nextFilters);
  };

  const handleSearchChange = (value) => {
    const nextFilters = { ...filters, search: value };
    listTodos(nextFilters);
  };

  return (
    <div>
      <button onClick={handleCompletedClick}>
        {filters.completed ? 'Completed' : 'All'}
      </button>
      <input onChange={(e) => handleSearchChange(e.target.value)} />
    </div>
  );
};
```

### Why This Is Shallow

- **Interface:** `<Toolbar />` and `<FilterBar />` render buttons and inputs
- **Implementation:** Each duplicates filter state logic (toggling, searching)
- **Maintenance burden:** Change filter logic once, update two components
- **No extraction point:** Filter logic is mixed with UI rendering; hard to test separately

### Testing Impact

**Current problem:**
- Cannot test filter logic independently from component rendering
- Must render component + mock TodoContext to test filter behavior
- Duplicated logic means duplicated tests

## Solution

Extract filter state management to a custom hook: `useFilterBar`

### 1. Create Custom Hook

**File:** `frontend/src/hooks/useFilterBar.js`

```javascript
import { useState, useCallback } from 'react';
import { useTodoContext } from '../context/TodoContext';

/**
 * Manages filter state and provides handlers for common filter operations.
 * Centralizes filter logic to avoid duplication across components.
 */
export const useFilterBar = () => {
  const { listTodos } = useTodoContext();
  const [filters, setFilters] = useState({
    completed: undefined,
    search: '',
  });

  /**
   * Toggle the completion filter (show completed vs. show all).
   */
  const toggleCompletedFilter = useCallback(() => {
    setFilters((prev) => {
      const completed = prev.completed === undefined ? false : !prev.completed;
      const nextFilters = { ...prev, completed };
      listTodos(nextFilters);
      return nextFilters;
    });
  }, [listTodos]);

  /**
   * Update the search filter and trigger list refresh.
   */
  const updateSearchFilter = useCallback(
    (searchText) => {
      setFilters((prev) => {
        const nextFilters = { ...prev, search: searchText };
        listTodos(nextFilters);
        return nextFilters;
      });
    },
    [listTodos]
  );

  /**
   * Clear all filters and reset to default state.
   */
  const clearFilters = useCallback(() => {
    setFilters({ completed: undefined, search: '' });
    listTodos({ completed: undefined, search: '' });
  }, [listTodos]);

  return {
    filters,
    toggleCompletedFilter,
    updateSearchFilter,
    clearFilters,
  };
};
```

### 2. Refactor Toolbar

**File:** `frontend/src/components/Toolbar.jsx`

```javascript
import { useFilterBar } from '../hooks/useFilterBar';

export const Toolbar = () => {
  const { filters, toggleCompletedFilter, updateSearchFilter, clearFilters } = useFilterBar();

  return (
    <div className="toolbar">
      <button onClick={toggleCompletedFilter}>
        {filters.completed === undefined
          ? 'Show All'
          : filters.completed
            ? 'Show Completed'
            : 'Show Incomplete'}
      </button>
      <input
        type="text"
        placeholder="Search..."
        value={filters.search}
        onChange={(e) => updateSearchFilter(e.target.value)}
      />
      <button onClick={clearFilters}>Clear Filters</button>
    </div>
  );
};
```

### 3. Refactor FilterBar

**File:** `frontend/src/components/FilterBar.jsx`

```javascript
import { useFilterBar } from '../hooks/useFilterBar';

export const FilterBar = () => {
  const { filters, toggleCompletedFilter, updateSearchFilter } = useFilterBar();

  return (
    <div className="filter-bar">
      <button onClick={toggleCompletedFilter}>
        {filters.completed ? 'Completed' : 'All'}
      </button>
      <input
        type="text"
        placeholder="Search todos..."
        value={filters.search}
        onChange={(e) => updateSearchFilter(e.target.value)}
      />
    </div>
  );
};
```

## Benefits

### Locality
- **Filter logic concentrated:** All state management and handlers live in one hook
- **Change efficiency:** To modify filter behavior, update hook; components automatically use new behavior
- **Clear responsibility:** Components render UI; hook manages state

### Leverage
- **Reusable hook:** Any component can import and use `useFilterBar` without duplicating logic
- **Consistent behavior:** All components using the hook have identical filter semantics
- **Extensible:** Adding new filter types (date range, status, etc.) requires hook change only

### Testing

**Before:** Filter logic tested by rendering components + mocking TodoContext
```javascript
// test/components/Toolbar.test.jsx
it('toggles completed filter', () => {
  // Must render component + mock TodoContext
  // Hard to test filter logic in isolation
});
```

**After:** Filter logic independently testable in hook
```javascript
// test/hooks/useFilterBar.test.js
import { renderHook, act } from '@testing-library/react';
import { useFilterBar } from '../useFilterBar';

describe('useFilterBar', () => {
  it('toggles completed filter', () => {
    const { result } = renderHook(() => useFilterBar(), {
      wrapper: ({ children }) => (
        <TodoProvider>{children}</TodoProvider>
      ),
    });

    expect(result.current.filters.completed).toBeUndefined();

    act(() => {
      result.current.toggleCompletedFilter();
    });

    expect(result.current.filters.completed).toBe(false);
  });

  it('updates search filter', () => {
    const { result } = renderHook(() => useFilterBar(), {
      wrapper: ({ children }) => (
        <TodoProvider>{children}</TodoProvider>
      ),
    });

    act(() => {
      result.current.updateSearchFilter('test query');
    });

    expect(result.current.filters.search).toBe('test query');
  });

  it('clears all filters', () => {
    const { result } = renderHook(() => useFilterBar(), {
      wrapper: ({ children }) => (
        <TodoProvider>{children}</TodoProvider>
      ),
    });

    act(() => {
      result.current.toggleCompletedFilter();
      result.current.updateSearchFilter('test');
    });

    expect(result.current.filters.completed).toBe(false);
    expect(result.current.filters.search).toBe('test');

    act(() => {
      result.current.clearFilters();
    });

    expect(result.current.filters.completed).toBeUndefined();
    expect(result.current.filters.search).toBe('');
  });
});
```

**Component tests are now simpler:**
```javascript
// test/components/Toolbar.test.jsx
it('calls toggleCompletedFilter when button clicked', () => {
  const mockToggle = jest.fn();
  
  jest.spyOn(useFilterBar, 'default').mockReturnValue({
    filters: { completed: false, search: '' },
    toggleCompletedFilter: mockToggle,
    updateSearchFilter: jest.fn(),
    clearFilters: jest.fn(),
  });

  render(<Toolbar />);
  fireEvent.click(screen.getByRole('button', { name: /completed/i }));

  expect(mockToggle).toHaveBeenCalled();
});
```

## Implementation Checklist

- [ ] Create `frontend/src/hooks/useFilterBar.js`
- [ ] Refactor `frontend/src/components/Toolbar.jsx` to use hook
- [ ] Refactor `frontend/src/components/FilterBar.jsx` to use hook
- [ ] Write unit tests for `useFilterBar`
- [ ] Write unit tests for refactored components
- [ ] Verify filtering works in app
- [ ] Test toggling completion filter
- [ ] Test search functionality
- [ ] Consider: should other components also use this hook?

## Future: Extend to More Filters

Once the hook exists, adding new filter types is centralized:

```javascript
export const useFilterBar = () => {
  const [filters, setFilters] = useState({
    completed: undefined,
    search: '',
    createdAfter: null,  // ← new
    createdBefore: null, // ← new
  });

  const updateDateFilter = useCallback((type, date) => {
    setFilters((prev) => ({
      ...prev,
      [type]: date,
    }));
  }, []);

  return {
    filters,
    toggleCompletedFilter,
    updateSearchFilter,
    updateDateFilter,
    clearFilters,
  };
};
```

Components using the hook automatically benefit from new functionality.

## Related Deepening Opportunities

- **Deepening 1:** Extract Frontend API Layer (complements this; API calls become testable too)
- **Deepening 4:** Extract Frontend API Configuration (independent; can be done in parallel)
