import { describe, it, expect, vi, afterEach } from 'vitest';

describe('FRONTEND_API_BASE_URL', () => {
  afterEach(() => {
    vi.unstubAllEnvs();
    vi.resetModules();
  });

  it('falls back to the local backend origin when VITE_API_URL is unset', async () => {
    vi.stubEnv('VITE_API_URL', undefined);
    vi.resetModules();

    const { FRONTEND_API_BASE_URL } = await import('./api');

    expect(FRONTEND_API_BASE_URL).toBe('http://localhost:5000');
  });

  it('uses VITE_API_URL when it is set', async () => {
    vi.stubEnv('VITE_API_URL', 'https://api.example.test');
    vi.resetModules();

    const { FRONTEND_API_BASE_URL } = await import('./api');

    expect(FRONTEND_API_BASE_URL).toBe('https://api.example.test');
  });
});
