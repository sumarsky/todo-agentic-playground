# Deepening Opportunity 1: Extract Frontend API Layer

**Severity:** 🔴 CRITICAL  
**Status:** Not started  
**Complexity:** High

## Problem Statement

The TodoContext mixes three concerns:
1. **HTTP layer** — fetch calls to API endpoints
2. **State management** — React useState/useRef for todos, loading, errors
3. **Business logic** — list transformations, filter application, bulk update orchestration

This violates the **interface principle**: callers (and tests) cannot test TodoContext's state logic without mocking React hooks and HTTP simultaneously.

### Current State

```javascript
// frontend/src/context/TodoContext.jsx

const runTodoRequest = async (request, nextTodos) => {
    setLoading(true);
    setError(null);
    try {
        const result = await request();  // ← HTTP call hidden here
        const updated = nextTodos(result, todosRef.current);  // ← Business logic
        todosRef.current = updated;
        setTodos(updated);  // ← State mutation
        return updated;
    } catch (err) {
        setError(err.message);
        throw err;
    } finally {
        setLoading(false);
    }
};

const listTodos = (filters) =>
    runTodoRequest(
        () => fetch(`${API_BASE_URL}/todos?completed=${filters.completed}&search=${filters.search}`),
        (todos, current) => todos // Filter logic buried here
    );
```

**Why This Is Shallow:**
- Interface complexity (what callers see): `listTodos(filters)` → promise
- Implementation complexity: HTTP fetch + state mutation + error handling
- **No hiding of complexity:** caller must understand both HTTP details and React state structure

### Testing Impact

To test `listTodos`:
```javascript
// Must mock React hooks (unmaintainable)
jest.spyOn(React, 'useState').mockImplementation(...)
jest.spyOn(React, 'useRef').mockImplementation(...)

// Must mock global fetch
global.fetch = jest.fn(...)

// Must intercept state setter calls
// Must validate state mutations through ref

// Result: test is tightly coupled to React internals, not business behavior
```

### Scope

**Files affected:**
- `frontend/src/context/TodoContext.jsx` — move HTTP calls out
- `frontend/src/components/Toolbar.jsx` — uses TodoContext
- `frontend/src/components/FilterBar.jsx` — uses TodoContext
- `frontend/src/components/TodoItem.jsx` — uses TodoContext
- `frontend/src/components/DashboardPage.jsx` — uses TodoContext
- `frontend/src/components/LogsPage.jsx` — uses TodoContext

## Solution

Create a **seam** at the HTTP boundary:

### 1. New API Client Module

**File:** `frontend/src/services/apiClient.js`

```javascript
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

export const apiClient = {
  // Todos
  listTodos: async (filters = {}) => {
    const params = new URLSearchParams();
    if (filters.completed !== undefined) params.append('completed', filters.completed);
    if (filters.search) params.append('search', filters.search);
    
    const res = await fetch(`${API_BASE_URL}/todos?${params}`);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
  },

  createTodo: async (title) => {
    const res = await fetch(`${API_BASE_URL}/todos`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ title }),
    });
    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message);
    }
    return res.json();
  },

  updateTodoTitle: async (id, title) => {
    const res = await fetch(`${API_BASE_URL}/todos/${id}/title`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ title }),
    });
    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message);
    }
    return res.json();
  },

  toggleTodo: async (id) => {
    const res = await fetch(`${API_BASE_URL}/todos/${id}/toggle`, {
      method: 'PUT',
    });
    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message);
    }
    return res.json();
  },

  deleteTodo: async (id) => {
    const res = await fetch(`${API_BASE_URL}/todos/${id}`, {
      method: 'DELETE',
    });
    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message);
    }
  },

  bulkDeleteTodos: async (ids) => {
    const res = await fetch(`${API_BASE_URL}/todos?ids=${ids.join(',')}`, {
      method: 'DELETE',
    });
    if (!res.ok) {
      const error = await res.json();
      throw new Error(error.message);
    }
  },

  // Metrics & Logs
  getMetrics: async (window = '1h') => {
    const res = await fetch(`${API_BASE_URL}/metrics?window=${window}`);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
  },

  getLogs: async (filters = {}) => {
    const params = new URLSearchParams();
    if (filters.level) params.append('level', filters.level);
    if (filters.message) params.append('message', filters.message);
    if (filters.since) params.append('since', filters.since);
    
    const res = await fetch(`${API_BASE_URL}/logs?${params}`);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
  },
};
```

### 2. Refactored TodoContext

**File:** `frontend/src/context/TodoContext.jsx`

```javascript
import { createContext, useContext, useState, useRef } from 'react';
import { apiClient } from '../services/apiClient';

const TodoContext = createContext();

export const TodoProvider = ({ children }) => {
  const [todos, setTodos] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const todosRef = useRef(todos);

  // State is now independent of HTTP; API client is injected
  const listTodos = async (filters = {}) => {
    setLoading(true);
    setError(null);
    try {
      const result = await apiClient.listTodos(filters);
      setTodos(result);
      todosRef.current = result;
      return result;
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const createTodo = async (title) => {
    setError(null);
    try {
      const newTodo = await apiClient.createTodo(title);
      const updated = [...todos, newTodo];
      setTodos(updated);
      todosRef.current = updated;
      return newTodo;
    } catch (err) {
      setError(err.message);
      throw err;
    }
  };

  const updateTodo = async (id, updates) => {
    setError(null);
    try {
      let updated = todos.find((t) => t.id === id);
      if (!updated) throw new Error('Todo not found');

      // Each property change is a separate API call
      if (updates.title !== undefined && updates.title !== updated.title) {
        updated = await apiClient.updateTodoTitle(id, updates.title);
      }

      if (updates.completed !== undefined && updates.completed !== updated.completed) {
        updated = await apiClient.toggleTodo(id);
      }

      const nextTodos = todos.map((t) => (t.id === id ? updated : t));
      setTodos(nextTodos);
      todosRef.current = nextTodos;
      return updated;
    } catch (err) {
      setError(err.message);
      throw err;
    }
  };

  const deleteTodo = async (id) => {
    setError(null);
    try {
      await apiClient.deleteTodo(id);
      const updated = todos.filter((t) => t.id !== id);
      setTodos(updated);
      todosRef.current = updated;
    } catch (err) {
      setError(err.message);
      throw err;
    }
  };

  const bulkDeleteTodos = async (ids) => {
    setError(null);
    try {
      await apiClient.bulkDeleteTodos(ids);
      const updated = todos.filter((t) => !ids.includes(t.id));
      setTodos(updated);
      todosRef.current = updated;
    } catch (err) {
      setError(err.message);
      throw err;
    }
  };

  return (
    <TodoContext.Provider
      value={{
        todos,
        loading,
        error,
        listTodos,
        createTodo,
        updateTodo,
        deleteTodo,
        bulkDeleteTodos,
      }}
    >
      {children}
    </TodoContext.Provider>
  );
};

export const useTodoContext = () => {
  const ctx = useContext(TodoContext);
  if (!ctx) throw new Error('useTodoContext must be called within TodoProvider');
  return ctx;
};
```

## Benefits

### Locality
- **API client logic concentrated:** All HTTP concerns (fetch, error handling, URL construction) live in one file
- **State management isolated:** TodoContext focuses only on state mutations and coordination
- **Change efficiency:** To modify API format, edit one file; to modify state behavior, edit another

### Leverage
- **Testable API client:** Mock `apiClient` in tests without touching React hooks
- **Reusable HTTP layer:** Other components can import and use `apiClient` directly
- **Swappable implementations:** Easy to swap fetch for axios or add retry logic in one place

### Testing

**Before:** Unit testing TodoContext required mocking React internals
```javascript
// Impossible to test without mocking useState/useRef
```

**After:** API client is independently testable
```javascript
// test/services/apiClient.test.js
describe('apiClient', () => {
  it('lists todos with filters', async () => {
    global.fetch = jest.fn().mockResolvedValue({
      ok: true,
      json: async () => [{ id: '1', title: 'Test' }],
    });

    const todos = await apiClient.listTodos({ completed: false });
    expect(todos).toEqual([{ id: '1', title: 'Test' }]);
  });
});

// test/context/TodoContext.test.js
describe('TodoContext', () => {
  it('updates todos after create', async () => {
    jest.spyOn(apiClient, 'createTodo').mockResolvedValue({
      id: '2',
      title: 'New Todo',
    });

    // Render with TodoProvider, call createTodo
    // Assert state changed — no mocking React hooks
  });
});
```

## Implementation Checklist

- [ ] Create `frontend/src/services/apiClient.js`
- [ ] Update `frontend/src/context/TodoContext.jsx` to use apiClient
- [ ] Remove hardcoded `API_BASE_URL` from context
- [ ] Update components to remove duplicate `API_BASE_URL` imports
- [ ] Add `VITE_API_URL` to `.env.example`
- [ ] Write tests for apiClient
- [ ] Write tests for TodoContext (now simplified)
- [ ] Verify all components still work with refactored context

## Related Deepening Opportunities

- **Deepening 4:** Extract Frontend API Configuration (environment-based URL selection)
- **Deepening 5:** Extract Frontend Filter State to Hook (complement this by moving filter logic to custom hook)
