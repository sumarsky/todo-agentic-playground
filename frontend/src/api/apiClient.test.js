import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { apiClient } from './apiClient';

describe('apiClient', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn());
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });

  describe('listTodos', () => {
    it('fetches GET /todos and returns parsed JSON', async () => {
      const todos = [{ id: 1, title: 'test', completed: false }];
      global.fetch.mockResolvedValue({
        ok: true,
        json: async () => todos,
      });

      const result = await apiClient.listTodos();

      expect(global.fetch).toHaveBeenCalledWith('http://localhost:5000/todos');
      expect(result).toEqual(todos);
    });

    it('builds query params from filters', async () => {
      global.fetch.mockResolvedValue({
        ok: true,
        json: async () => [],
      });

      await apiClient.listTodos({ completed: true, search: 'buy' });

      expect(global.fetch).toHaveBeenCalledWith(
        'http://localhost:5000/todos?completed=true&search=buy'
      );
    });

    it('omits empty filters from query', async () => {
      global.fetch.mockResolvedValue({
        ok: true,
        json: async () => [],
      });

      await apiClient.listTodos({ completed: false, search: '' });

      expect(global.fetch).toHaveBeenCalledWith('http://localhost:5000/todos');
    });

    it('throws on HTTP error with status code', async () => {
      global.fetch.mockResolvedValue({
        ok: false,
        status: 500,
      });

      await expect(apiClient.listTodos()).rejects.toThrow('HTTP 500');
    });

    it('throws with parsed error message when available', async () => {
      global.fetch.mockResolvedValue({
        ok: false,
        status: 400,
        json: async () => ({ message: 'Invalid request' }),
      });

      await expect(apiClient.listTodos()).rejects.toThrow('Invalid request');
    });
  });

  describe('createTodo', () => {
    it('POSTs /todos with title and returns created todo', async () => {
      const created = { id: 2, title: 'new', completed: false };
      global.fetch.mockResolvedValue({
        ok: true,
        json: async () => created,
      });

      const result = await apiClient.createTodo('new');

      expect(global.fetch).toHaveBeenCalledWith('http://localhost:5000/todos', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ title: 'new' }),
      });
      expect(result).toEqual(created);
    });
  });

  describe('updateTodoTitle', () => {
    it('PUTs /todos/:id/title and returns updated todo', async () => {
      const updated = { id: 1, title: 'changed', completed: false };
      global.fetch.mockResolvedValue({
        ok: true,
        json: async () => updated,
      });

      const result = await apiClient.updateTodoTitle(1, 'changed');

      expect(global.fetch).toHaveBeenCalledWith('http://localhost:5000/todos/1/title', {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ title: 'changed' }),
      });
      expect(result).toEqual(updated);
    });
  });

  describe('toggleTodo', () => {
    it('PUTs /todos/:id/toggle and returns updated todo', async () => {
      const toggled = { id: 1, title: 'test', completed: true };
      global.fetch.mockResolvedValue({
        ok: true,
        json: async () => toggled,
      });

      const result = await apiClient.toggleTodo(1);

      expect(global.fetch).toHaveBeenCalledWith('http://localhost:5000/todos/1/toggle', {
        method: 'PUT',
      });
      expect(result).toEqual(toggled);
    });
  });

  describe('deleteTodo', () => {
    it('DELETEs /todos/:id', async () => {
      global.fetch.mockResolvedValue({ ok: true });

      await apiClient.deleteTodo(1);

      expect(global.fetch).toHaveBeenCalledWith('http://localhost:5000/todos/1', {
        method: 'DELETE',
      });
    });
  });

  describe('bulkDeleteTodos', () => {
    it('DELETEs /todos?ids=1,2,3', async () => {
      global.fetch.mockResolvedValue({ ok: true });

      await apiClient.bulkDeleteTodos([1, 2, 3]);

      expect(global.fetch).toHaveBeenCalledWith('http://localhost:5000/todos?ids=1%2C2%2C3', {
        method: 'DELETE',
      });
    });
  });

  describe('environment configuration', () => {
    it('uses VITE_API_URL when set', async () => {
      vi.stubEnv('VITE_API_URL', 'http://custom-api:3000');
      // Need to reimport after env change
      vi.resetModules();
      const { apiClient: customClient } = await import('./apiClient');

      global.fetch.mockResolvedValue({
        ok: true,
        json: async () => [],
      });

      await customClient.listTodos();

      expect(global.fetch).toHaveBeenCalledWith('http://custom-api:3000/todos');

      vi.unstubAllEnvs();
    });
  });
});
