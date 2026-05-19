const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

async function checkResponse(res) {
  if (!res.ok) {
    let message = `HTTP ${res.status}`;
    try {
      const body = await res.json();
      if (body && body.message) {
        message = body.message;
      }
    } catch {
      // body not JSON or no message -> use status message
    }
    throw new Error(message);
  }
  return res;
}

export const apiClient = {
  async listTodos(filters = {}) {
    const params = new URLSearchParams();
    if (filters.completed) params.set('completed', 'true');
    if (filters.search) params.set('search', filters.search);
    const query = params.toString();
    const url = `${API_BASE_URL}/todos${query ? `?${query}` : ''}`;
    const res = await fetch(url);
    return (await checkResponse(res)).json();
  },

  async createTodo(title) {
    const res = await fetch(`${API_BASE_URL}/todos`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ title }),
    });
    return (await checkResponse(res)).json();
  },

  async updateTodoTitle(id, title) {
    const res = await fetch(`${API_BASE_URL}/todos/${id}/title`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ title }),
    });
    return (await checkResponse(res)).json();
  },

  async toggleTodo(id) {
    const res = await fetch(`${API_BASE_URL}/todos/${id}/toggle`, {
      method: 'PUT',
    });
    return (await checkResponse(res)).json();
  },

  async deleteTodo(id) {
    const res = await fetch(`${API_BASE_URL}/todos/${id}`, {
      method: 'DELETE',
    });
    await checkResponse(res);
  },

  async bulkDeleteTodos(ids) {
    const params = new URLSearchParams();
    params.set('ids', ids.join(','));
    const res = await fetch(`${API_BASE_URL}/todos?${params.toString()}`, {
      method: 'DELETE',
    });
    await checkResponse(res);
  },
};
