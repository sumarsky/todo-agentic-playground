import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useTodoApi } from './useTodoApi';

describe('useTodoApi', () => {
  let mockClient;

  beforeEach(() => {
    mockClient = {
      listTodos: vi.fn(),
      createTodo: vi.fn(),
      updateTodoTitle: vi.fn(),
      toggleTodo: vi.fn(),
      deleteTodo: vi.fn(),
      bulkDeleteTodos: vi.fn(),
    };
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('initial state', () => {
    it('returns empty todos, not loading, no error', () => {
      const { result } = renderHook(() => useTodoApi({ apiClient: mockClient }));

      expect(result.current.todos).toEqual([]);
      expect(result.current.loading).toBe(false);
      expect(result.current.error).toBe(null);
    });
  });

  describe('listTodos success', () => {
    it('fetches todos and updates state', async () => {
      const fakeTodos = [{ id: 1, title: 'Test', completed: false }];
      mockClient.listTodos.mockResolvedValue(fakeTodos);

      const { result } = renderHook(() => useTodoApi({ apiClient: mockClient }));

      await act(async () => {
        await result.current.listTodos();
      });

      expect(mockClient.listTodos).toHaveBeenCalledWith({});
      expect(result.current.todos).toEqual(fakeTodos);
      expect(result.current.loading).toBe(false);
      expect(result.current.error).toBe(null);
    });

    it('passes filters to apiClient', async () => {
      mockClient.listTodos.mockResolvedValue([]);

      const { result } = renderHook(() => useTodoApi({ apiClient: mockClient }));

      await act(async () => {
        await result.current.listTodos({ completed: true, search: 'test' });
      });

      expect(mockClient.listTodos).toHaveBeenCalledWith({ completed: true, search: 'test' });
    });
  });

  describe('listTodos error', () => {
    it('sets error on failure', async () => {
      mockClient.listTodos.mockRejectedValue(new Error('Network error'));

      const { result } = renderHook(() => useTodoApi({ apiClient: mockClient }));

      await act(async () => {
        await result.current.listTodos();
      });

      expect(result.current.error).toBe('Network error');
      expect(result.current.todos).toEqual([]);
      expect(result.current.loading).toBe(false);
    });

    it('clears previous error on new request', async () => {
      mockClient.listTodos.mockRejectedValueOnce(new Error('First error'));
      mockClient.listTodos.mockResolvedValueOnce([]);

      const { result } = renderHook(() => useTodoApi({ apiClient: mockClient }));

      await act(async () => {
        await result.current.listTodos();
      });
      expect(result.current.error).toBe('First error');

      await act(async () => {
        await result.current.listTodos();
      });
      expect(result.current.error).toBe(null);
    });
  });

  describe('addTodo', () => {
    it('creates todo and appends to list', async () => {
      const newTodo = { id: 1, title: 'New', completed: false };
      mockClient.createTodo.mockResolvedValue(newTodo);

      const { result } = renderHook(() => useTodoApi({ apiClient: mockClient }));

      await act(async () => {
        await result.current.addTodo('New');
      });

      expect(mockClient.createTodo).toHaveBeenCalledWith('New');
      expect(result.current.todos).toContainEqual(newTodo);
      expect(result.current.loading).toBe(false);
    });

    it('sets error on failure', async () => {
      mockClient.createTodo.mockRejectedValue(new Error('Duplicate'));

      const { result } = renderHook(() => useTodoApi({ apiClient: mockClient }));

      await act(async () => {
        await result.current.addTodo('New');
      });

      expect(result.current.error).toBe('Duplicate');
    });
  });

  describe('updateTodo', () => {
    it('updates todo title in place', async () => {
      const existingTodos = [{ id: 1, title: 'Old', completed: false }];
      const updatedTodo = { id: 1, title: 'New', completed: false };
      mockClient.updateTodoTitle.mockResolvedValue(updatedTodo);

      const { result } = renderHook(() => useTodoApi({ apiClient: mockClient }));

      // Seed todos
      await act(async () => {
        mockClient.listTodos.mockResolvedValue(existingTodos);
        await result.current.listTodos();
      });

      await act(async () => {
        await result.current.updateTodo(1, 'New');
      });

      expect(mockClient.updateTodoTitle).toHaveBeenCalledWith(1, 'New');
      expect(result.current.todos).toContainEqual(updatedTodo);
      expect(result.current.todos.find(t => t.id === 1).title).toBe('New');
    });

    it('sets error on failure', async () => {
      mockClient.updateTodoTitle.mockRejectedValue(new Error('Not found'));

      const { result } = renderHook(() => useTodoApi({ apiClient: mockClient }));

      await act(async () => {
        await result.current.updateTodo(999, 'New');
      });

      expect(result.current.error).toBe('Not found');
    });
  });

  describe('deleteTodo', () => {
    it('removes todo from list', async () => {
      const existingTodos = [
        { id: 1, title: 'One', completed: false },
        { id: 2, title: 'Two', completed: false },
      ];
      mockClient.deleteTodo.mockResolvedValue(undefined);

      const { result } = renderHook(() => useTodoApi({ apiClient: mockClient }));

      await act(async () => {
        mockClient.listTodos.mockResolvedValue(existingTodos);
        await result.current.listTodos();
      });

      await act(async () => {
        await result.current.deleteTodo(1);
      });

      expect(mockClient.deleteTodo).toHaveBeenCalledWith(1);
      expect(result.current.todos).toHaveLength(1);
      expect(result.current.todos.find(t => t.id === 1)).toBeUndefined();
    });

    it('sets error on failure', async () => {
      mockClient.deleteTodo.mockRejectedValue(new Error('Not found'));

      const { result } = renderHook(() => useTodoApi({ apiClient: mockClient }));

      await act(async () => {
        await result.current.deleteTodo(999);
      });

      expect(result.current.error).toBe('Not found');
    });
  });

  describe('bulkDeleteTodos', () => {
    it('removes multiple todos from list', async () => {
      const existingTodos = [
        { id: 1, title: 'One', completed: false },
        { id: 2, title: 'Two', completed: false },
        { id: 3, title: 'Three', completed: false },
      ];
      mockClient.bulkDeleteTodos.mockResolvedValue(undefined);

      const { result } = renderHook(() => useTodoApi({ apiClient: mockClient }));

      await act(async () => {
        mockClient.listTodos.mockResolvedValue(existingTodos);
        await result.current.listTodos();
      });

      await act(async () => {
        await result.current.bulkDeleteTodos([1, 3]);
      });

      expect(mockClient.bulkDeleteTodos).toHaveBeenCalledWith([1, 3]);
      expect(result.current.todos).toHaveLength(1);
      expect(result.current.todos[0].id).toBe(2);
    });

    it('sets error on failure', async () => {
      mockClient.bulkDeleteTodos.mockRejectedValue(new Error('Server error'));

      const { result } = renderHook(() => useTodoApi({ apiClient: mockClient }));

      await act(async () => {
        await result.current.bulkDeleteTodos([1, 2]);
      });

      expect(result.current.error).toBe('Server error');
    });
  });
});
