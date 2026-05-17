import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { useContext } from 'react';
import { TodoContextProvider } from './TodoContext';
import { TodoContext } from './TodoContextValue';

const StateCapture = () => {
  const ctx = useContext(TodoContext);
  return (
    <div>
      <span data-testid="todos-count">{ctx.todos.length}</span>
      <span data-testid="loading">{String(ctx.loading)}</span>
      <span data-testid="error">{ctx.error || 'none'}</span>
      <span data-testid="selected-count">{ctx.selectedIds?.length || 0}</span>
      <span data-testid="selected-ids">{(ctx.selectedIds || []).join(',')}</span>
    </div>
  );
};

describe('TodoContext', () => {
  it('has default state and action functions', () => {
    expect(TodoContext).toBeDefined();
    expect(TodoContext._currentValue).toEqual(expect.objectContaining({
      todos: [],
      loading: false,
      error: null,
      listTodos: expect.any(Function),
      addTodo: expect.any(Function),
      updateTodo: expect.any(Function),
      deleteTodo: expect.any(Function),
      bulkDeleteTodos: expect.any(Function),
    }));
  });
});

describe('TodoContextProvider - Actions', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn());
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });

  describe('load todos on mount', () => {
    it('fetches todos from GET /todos when component mounts', async () => {
      const mockTodos = [
        { id: '1', title: 'Task 1' },
        { id: '2', title: 'Task 2' },
      ];

      globalThis.fetch.mockResolvedValueOnce({
        json: async () => mockTodos,
      });

      render(
        <TodoContextProvider>
          <StateCapture />
        </TodoContextProvider>
      );

      await waitFor(() => {
        expect(screen.getByTestId('todos-count')).toHaveTextContent('2');
      });

      expect(globalThis.fetch).toHaveBeenCalledWith('http://localhost:5000/todos');
    });
  });

  describe('listTodos()', () => {
    it('sets loading true while request is pending, then false after success', async () => {
      const mockTodos = [{ id: '1', title: 'Task 1' }];
      let resolveRequest;
      const request = new Promise((resolve) => {
        resolveRequest = resolve;
      });

      globalThis.fetch.mockReturnValueOnce(request);

      let capturedActions;
      const StateCapture = () => {
        const ctx = useContext(TodoContext);
        capturedActions = ctx;
        return (
          <div>
            <span data-testid="loading">{String(ctx.loading)}</span>
          </div>
        );
      };

      render(
        <TodoContextProvider initialTodos={[{ id: 'stub' }]}>
          <StateCapture />
        </TodoContextProvider>
      );

      const resultPromise = capturedActions.listTodos();

      await waitFor(() => {
        expect(screen.getByTestId('loading')).toHaveTextContent('true');
      });

      resolveRequest({
        json: async () => mockTodos,
      });

      await expect(resultPromise).resolves.toEqual(mockTodos);

      await waitFor(() => {
        expect(screen.getByTestId('loading')).toHaveTextContent('false');
      });
    });

    it('fetches todos from GET /todos, manages loading state, returns resolved todos', async () => {
      const mockTodos = [
        { id: '1', title: 'Task 1' },
        { id: '2', title: 'Task 2' },
      ];

      globalThis.fetch.mockResolvedValueOnce({
        json: async () => mockTodos,
      });

      let capturedActions;
      const StateCapture = () => {
        const ctx = useContext(TodoContext);
        capturedActions = ctx;
        return (
          <div>
            <span data-testid="loading">{String(ctx.loading)}</span>
            <span data-testid="todos-count">{ctx.todos.length}</span>
            <span data-testid="error">{ctx.error || 'none'}</span>
          </div>
        );
      };

      render(
        <TodoContextProvider initialTodos={[{ id: 'stub' }]}>
          <StateCapture />
        </TodoContextProvider>
      );

      const result = await capturedActions.listTodos();

      await waitFor(() => {
        expect(screen.getByTestId('todos-count')).toHaveTextContent('2');
      });

      expect(globalThis.fetch).toHaveBeenCalledWith('http://localhost:5000/todos');
      expect(result).toEqual(mockTodos);
      expect(screen.getByTestId('loading')).toHaveTextContent('false');
      expect(screen.getByTestId('error')).toHaveTextContent('none');
    });

    it('fetches todos with completed and search query params', async () => {
      const mockTodos = [{ id: '1', title: 'Docs', completed: true }];

      globalThis.fetch.mockResolvedValueOnce({
        json: async () => mockTodos,
      });

      let capturedActions;
      const StateCapture = () => {
        const ctx = useContext(TodoContext);
        capturedActions = ctx;
        return <span data-testid="todos-count">{ctx.todos.length}</span>;
      };

      render(
        <TodoContextProvider initialTodos={[{ id: 'stub' }]}>
          <StateCapture />
        </TodoContextProvider>
      );

      await capturedActions.listTodos({ completed: true, search: 'docs' });

      expect(globalThis.fetch).toHaveBeenCalledWith(
        'http://localhost:5000/todos?completed=true&search=docs'
      );
    });
  });

  describe('addTodo(title)', () => {
    it('posts to /todos with title, appends todo to state, returns updated todos', async () => {
      const existingTodo = { id: '1', title: 'Existing' };
      const newTodo = { id: '2', title: 'New Task' };

      globalThis.fetch.mockResolvedValueOnce({
        json: async () => newTodo,
      });

      let capturedActions;
      const StateCapture = () => {
        const ctx = useContext(TodoContext);
        capturedActions = ctx;
        return (
          <div>
            <span data-testid="todos-count">{ctx.todos.length}</span>
            <span data-testid="error">{ctx.error || 'none'}</span>
          </div>
        );
      };

      render(
        <TodoContextProvider initialTodos={[existingTodo]}>
          <StateCapture />
        </TodoContextProvider>
      );

      expect(screen.getByTestId('todos-count')).toHaveTextContent('1');

      const result = await capturedActions.addTodo('New Task');

      await waitFor(() => {
        expect(screen.getByTestId('todos-count')).toHaveTextContent('2');
      });

      expect(globalThis.fetch).toHaveBeenCalledWith('http://localhost:5000/todos', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ title: 'New Task' }),
      });
      expect(result).toEqual([existingTodo, newTodo]);
      expect(screen.getByTestId('error')).toHaveTextContent('none');
    });
  });

  describe('Error handling', () => {
    it('failed request sets error state and clears loading', async () => {
      const networkError = new Error('Network failed');
      globalThis.fetch.mockRejectedValueOnce(networkError);

      let capturedActions;
      const StateCapture = () => {
        const ctx = useContext(TodoContext);
        capturedActions = ctx;
        return (
          <div>
            <span data-testid="loading">{String(ctx.loading)}</span>
            <span data-testid="error">{ctx.error || 'none'}</span>
          </div>
        );
      };

      render(
        <TodoContextProvider initialTodos={[{ id: 'stub' }]}>
          <StateCapture />
        </TodoContextProvider>
      );

      try {
        await capturedActions.listTodos();
      } catch {
        // expected to throw
      }

      await waitFor(() => {
        expect(screen.getByTestId('error')).toHaveTextContent('Network failed');
      });

      expect(screen.getByTestId('loading')).toHaveTextContent('false');
    });
  });

  describe('updateTodo(id, updates)', () => {
    it('puts title and completion changes to backend todo endpoints, merges todo in state, returns updated todos', async () => {
      const todoToUpdate = { id: '1', title: 'Old Title', completed: false };
      const updated = { id: '1', title: 'New Title', completed: true };

      globalThis.fetch
        .mockResolvedValueOnce({
          json: async () => ({ id: '1', title: 'New Title', completed: false }),
        })
        .mockResolvedValueOnce({
          json: async () => updated,
        });

      let capturedActions;
      const StateCapture = () => {
        const ctx = useContext(TodoContext);
        capturedActions = ctx;
        return (
          <div>
            <span data-testid="todos-count">{ctx.todos.length}</span>
            <span data-testid="todo-title">{ctx.todos[0]?.title}</span>
            <span data-testid="error">{ctx.error || 'none'}</span>
          </div>
        );
      };

      render(
        <TodoContextProvider initialTodos={[todoToUpdate]}>
          <StateCapture />
        </TodoContextProvider>
      );

      expect(screen.getByTestId('todo-title')).toHaveTextContent('Old Title');

      const result = await capturedActions.updateTodo('1', { title: 'New Title', completed: true });

      await waitFor(() => {
        expect(screen.getByTestId('todo-title')).toHaveTextContent('New Title');
      });

      expect(globalThis.fetch).toHaveBeenNthCalledWith(1, 'http://localhost:5000/todos/1/title', {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ title: 'New Title' }),
      });
      expect(globalThis.fetch).toHaveBeenNthCalledWith(2, 'http://localhost:5000/todos/1/toggle', {
        method: 'PUT',
      });
      expect(result).toEqual([updated]);
      expect(screen.getByTestId('error')).toHaveTextContent('none');
    });
  });

  describe('deleteTodo(id)', () => {
    it('deletes /todos/{id}, removes todo from state, returns updated todos', async () => {
      const todos = [
        { id: '1', title: 'Task 1' },
        { id: '2', title: 'Task 2' },
      ];

      globalThis.fetch.mockResolvedValueOnce({
        json: async () => ({}),
      });

      let capturedActions;
      const StateCapture = () => {
        const ctx = useContext(TodoContext);
        capturedActions = ctx;
        return (
          <div>
            <span data-testid="todos-count">{ctx.todos.length}</span>
            <span data-testid="error">{ctx.error || 'none'}</span>
          </div>
        );
      };

      render(
        <TodoContextProvider initialTodos={todos}>
          <StateCapture />
        </TodoContextProvider>
      );

      expect(screen.getByTestId('todos-count')).toHaveTextContent('2');

      const result = await capturedActions.deleteTodo('1');

      await waitFor(() => {
        expect(screen.getByTestId('todos-count')).toHaveTextContent('1');
      });

      expect(globalThis.fetch).toHaveBeenCalledWith('http://localhost:5000/todos/1', {
        method: 'DELETE',
      });
      expect(result).toEqual([{ id: '2', title: 'Task 2' }]);
      expect(screen.getByTestId('error')).toHaveTextContent('none');
    });
  });

  describe('bulkDeleteTodos(ids)', () => {
    it('deletes /todos with id array, removes multiple todos from state, returns updated todos', async () => {
      const todos = [
        { id: '1', title: 'Task 1' },
        { id: '2', title: 'Task 2' },
        { id: '3', title: 'Task 3' },
      ];

      globalThis.fetch.mockResolvedValueOnce({
        json: async () => ({}),
      });

      let capturedActions;
      const StateCapture = () => {
        const ctx = useContext(TodoContext);
        capturedActions = ctx;
        return (
          <div>
            <span data-testid="todos-count">{ctx.todos.length}</span>
            <span data-testid="error">{ctx.error || 'none'}</span>
          </div>
        );
      };

      render(
        <TodoContextProvider initialTodos={todos}>
          <StateCapture />
        </TodoContextProvider>
      );

      expect(screen.getByTestId('todos-count')).toHaveTextContent('3');

      const result = await capturedActions.bulkDeleteTodos(['1', '3']);

      await waitFor(() => {
        expect(screen.getByTestId('todos-count')).toHaveTextContent('1');
      });

      expect(globalThis.fetch).toHaveBeenCalledWith('http://localhost:5000/todos?ids=1%2C3', {
        method: 'DELETE',
      });
      expect(result).toEqual([{ id: '2', title: 'Task 2' }]);
      expect(screen.getByTestId('error')).toHaveTextContent('none');
    });
  });

  describe('selectAll()', () => {
    it('selects all visible todo ids', async () => {
      const todos = [
        { id: '1', title: 'Task 1' },
        { id: '2', title: 'Task 2' },
        { id: '3', title: 'Task 3' },
      ];

      let capturedActions;
      const StateCapture = () => {
        const ctx = useContext(TodoContext);
        capturedActions = ctx;
        return (
          <div>
            <span data-testid="selected-count">{ctx.selectedIds?.length || 0}</span>
            <span data-testid="selected-ids">{(ctx.selectedIds || []).join(',')}</span>
          </div>
        );
      };

      render(
        <TodoContextProvider initialTodos={todos}>
          <StateCapture />
        </TodoContextProvider>
      );

      expect(screen.getByTestId('selected-count')).toHaveTextContent('0');

      capturedActions.selectAll();

      await waitFor(() => {
        expect(screen.getByTestId('selected-count')).toHaveTextContent('3');
        expect(screen.getByTestId('selected-ids')).toHaveTextContent('1,2,3');
      });
    });
  });

  describe('deselectAll()', () => {
    it('clears all selected ids', async () => {
      const todos = [
        { id: '1', title: 'Task 1' },
        { id: '2', title: 'Task 2' },
      ];

      let capturedActions;
      const StateCapture = () => {
        const ctx = useContext(TodoContext);
        capturedActions = ctx;
        return (
          <div>
            <span data-testid="selected-ids">{(ctx.selectedIds || []).join(',')}</span>
          </div>
        );
      };

      render(
        <TodoContextProvider initialTodos={todos}>
          <StateCapture />
        </TodoContextProvider>
      );

      capturedActions.selectAll();
      await waitFor(() => {
        expect(screen.getByTestId('selected-ids')).toHaveTextContent('1,2');
      });

      capturedActions.deselectAll();
      await waitFor(() => {
        expect(screen.getByTestId('selected-ids')).toHaveTextContent('');
      });
    });
  });
});
