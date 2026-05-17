import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import App from './App';

const API_BASE_URL = 'http://localhost:5000';

const createBackendFetch = () => {
  let nextId = 1;
  let todos = [];

  const jsonResponse = (body, init = {}) => ({
    ok: init.status ? init.status < 400 : true,
    status: init.status ?? 200,
    json: async () => body,
  });

  const noContentResponse = () => ({
    ok: true,
    status: 204,
    json: async () => ({}),
  });

  return vi.fn(async (input, options = {}) => {
    const url = new URL(input);
    const method = options.method ?? 'GET';

    if (url.origin !== API_BASE_URL) {
      throw new Error(`Unexpected API origin: ${url.origin}`);
    }

    if (method === 'POST' && url.pathname === '/todos') {
      const body = JSON.parse(options.body);
      const title = body.title.trim();
      if (!title) {
        return jsonResponse({ statusCode: 400, message: 'Title cannot be empty' }, { status: 400 });
      }

      const todo = { id: String(nextId++), title, completed: false };
      todos = [...todos, todo];
      return jsonResponse(todo, { status: 201 });
    }

    if (method === 'GET' && url.pathname === '/todos') {
      const completed = url.searchParams.get('completed');
      const search = url.searchParams.get('search')?.toLowerCase();
      let result = todos;

      if (completed === 'true') {
        result = result.filter((todo) => todo.completed);
      }
      if (search) {
        result = result.filter((todo) => todo.title.toLowerCase().includes(search));
      }

      return jsonResponse(result);
    }

    const titleMatch = url.pathname.match(/^\/todos\/([^/]+)\/title$/);
    if (method === 'PUT' && titleMatch) {
      const id = titleMatch[1];
      const body = JSON.parse(options.body);
      const todo = todos.find((item) => item.id === id);
      if (!todo) {
        return jsonResponse({ statusCode: 404, message: 'Todo not found' }, { status: 404 });
      }

      const updated = { ...todo, title: body.title.trim() };
      todos = todos.map((item) => (item.id === id ? updated : item));
      return jsonResponse(updated);
    }

    const toggleMatch = url.pathname.match(/^\/todos\/([^/]+)\/toggle$/);
    if (method === 'PUT' && toggleMatch) {
      const id = toggleMatch[1];
      const todo = todos.find((item) => item.id === id);
      if (!todo) {
        return jsonResponse({ statusCode: 404, message: 'Todo not found' }, { status: 404 });
      }

      const updated = { ...todo, completed: !todo.completed };
      todos = todos.map((item) => (item.id === id ? updated : item));
      return jsonResponse(updated);
    }

    const deleteMatch = url.pathname.match(/^\/todos\/([^/]+)$/);
    if (method === 'DELETE' && deleteMatch) {
      const id = deleteMatch[1];
      todos = todos.filter((todo) => todo.id !== id);
      return noContentResponse();
    }

    if (method === 'DELETE' && url.pathname === '/todos') {
      const ids = url.searchParams.get('ids')?.split(',') ?? [];
      todos = todos.filter((todo) => !ids.includes(todo.id));
      return noContentResponse();
    }

    throw new Error(`Unexpected request: ${method} ${url.pathname}${url.search}`);
  });
};

describe('Todo app integration', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', createBackendFetch());
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });

  it('supports add, list, filter, update, delete, and bulk delete through backend-shaped requests', async () => {
    render(<App />);

    // Add first todo
    fireEvent.click(screen.getByRole('button', { name: /add todo/i }));
    let titleInput = await screen.findByLabelText(/todo title/i);
    fireEvent.change(titleInput, { target: { value: 'Write tests' } });
    fireEvent.submit(screen.getByRole('form', { name: /add todo/i }));

    expect(await screen.findByText('Write tests')).toBeInTheDocument();

    // Add second todo
    fireEvent.click(screen.getByRole('button', { name: /add todo/i }));
    titleInput = await screen.findByLabelText(/todo title/i);
    fireEvent.change(titleInput, { target: { value: 'Ship UI' } });
    fireEvent.submit(screen.getByRole('form', { name: /add todo/i }));

    expect(await screen.findByText('Ship UI')).toBeInTheDocument();

    // Add third todo
    fireEvent.click(screen.getByRole('button', { name: /add todo/i }));
    titleInput = await screen.findByLabelText(/todo title/i);
    fireEvent.change(titleInput, { target: { value: 'Fix docs' } });
    fireEvent.submit(screen.getByRole('form', { name: /add todo/i }));

    expect(await screen.findByText('Fix docs')).toBeInTheDocument();

    fireEvent.change(screen.getByLabelText(/search todos/i), {
      target: { value: 'Write' },
    });

    await waitFor(() => {
      expect(screen.getByText('Write tests')).toBeInTheDocument();
      expect(screen.queryByText('Ship UI')).not.toBeInTheDocument();
    });

    fireEvent.change(screen.getByLabelText(/search todos/i), {
      target: { value: '' },
    });

    await screen.findByText('Ship UI');

    fireEvent.click(screen.getByRole('button', { name: /edit write tests/i }));
    fireEvent.change(screen.getByLabelText(/edit todo title/i), {
      target: { value: 'Write integration tests' },
    });
    fireEvent.click(screen.getByRole('button', { name: /save todo/i }));

    expect(await screen.findByText('Write integration tests')).toBeInTheDocument();

    fireEvent.click(screen.getByText('Write integration tests'));
    fireEvent.click(screen.getByRole('button', { name: /filter completed/i }));

    await waitFor(() => {
      expect(screen.getByText('Write integration tests')).toBeInTheDocument();
      expect(screen.queryByText('Ship UI')).not.toBeInTheDocument();
    });

    fireEvent.click(screen.getByRole('button', { name: /filter completed/i }));
    await screen.findByText('Ship UI');

    fireEvent.click(screen.getByRole('button', { name: /delete ship ui/i }));

    await waitFor(() => {
      expect(screen.queryByText('Ship UI')).not.toBeInTheDocument();
    });

    const list = screen.getByRole('list', { name: /todos/i });
    fireEvent.click(within(list).getByRole('checkbox', { name: /select write integration tests/i }));
    fireEvent.click(within(list).getByRole('checkbox', { name: /select fix docs/i }));
    fireEvent.click(screen.getByRole('button', { name: /delete selected \(2\)/i }));

    await waitFor(() => {
      expect(screen.getByText(/no todos yet/i)).toBeInTheDocument();
    });

    expect(globalThis.fetch).toHaveBeenCalledWith(`${API_BASE_URL}/todos/1/title`, expect.objectContaining({
      method: 'PUT',
      body: JSON.stringify({ title: 'Write integration tests' }),
    }));
    expect(globalThis.fetch).toHaveBeenCalledWith(`${API_BASE_URL}/todos/1/toggle`, {
      method: 'PUT',
    });
    expect(globalThis.fetch).toHaveBeenCalledWith(`${API_BASE_URL}/todos?ids=1%2C3`, {
      method: 'DELETE',
    });
  });
});
