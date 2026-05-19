import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { LogsPage } from './LogsPage';

const API_BASE_URL = 'http://localhost:5000';

const renderLogs = () => {
  return render(
    <MemoryRouter initialEntries={['/logs']}>
      <LogsPage />
    </MemoryRouter>
  );
};

describe('LogsPage', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn());
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });

  describe('empty state', () => {
    it('shows empty state when no logs are returned', async () => {
      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: async () => [],
      });

      renderLogs();

      expect(await screen.findByText('No logs found')).toBeInTheDocument();
    });
  });

  describe('log entries', () => {
    const mockLogs = [
      {
        id: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
        timestamp: '2026-05-19T10:00:00Z',
        level: 'Info',
        source: 'api',
        message: 'Request completed',
        httpMethod: 'GET',
        httpPath: '/api/todos',
        httpStatus: 200,
        durationMs: 45.2,
      },
      {
        id: 'b2c3d4e5-f6a7-8901-bcde-f12345678901',
        timestamp: '2026-05-19T09:30:00Z',
        level: 'Error',
        source: 'api',
        message: 'Internal server error',
        httpMethod: 'POST',
        httpPath: '/api/todos',
        httpStatus: 500,
        durationMs: 120.5,
      },
    ];

    it('renders log entries when data is present', async () => {
      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: async () => mockLogs,
      });

      renderLogs();

      expect(await screen.findByText('Request completed')).toBeInTheDocument();
      expect(screen.getByText('Internal server error')).toBeInTheDocument();
    });
  });

  describe('level filter', () => {
    it('renders level dropdown with All, Info, Warning, Error options', async () => {
      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: async () => [],
      });

      renderLogs();

      await screen.findByText('No logs found');

      const selector = screen.getByRole('combobox', { name: /level/i });
      expect(selector).toBeInTheDocument();
      expect(selector).toHaveValue('');

      expect(screen.getByRole('option', { name: 'All' })).toBeInTheDocument();
      expect(screen.getByRole('option', { name: 'Info' })).toBeInTheDocument();
      expect(screen.getByRole('option', { name: 'Warning' })).toBeInTheDocument();
      expect(screen.getByRole('option', { name: 'Error' })).toBeInTheDocument();
    });

    it('triggers re-fetch with correct level param when selector changes', async () => {
      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: async () => [],
      });

      renderLogs();

      await screen.findByText('No logs found');

      const selector = screen.getByRole('combobox', { name: /level/i });
      fireEvent.change(selector, { target: { value: 'Error' } });

      await waitFor(() => {
        expect(globalThis.fetch).toHaveBeenCalledWith(
          `${API_BASE_URL}/api/logs?level=Error`
        );
      });
    });
  });

  describe('message search filter', () => {
    it('renders message search input', async () => {
      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: async () => [],
      });

      renderLogs();

      await screen.findByText('No logs found');

      const input = screen.getByRole('textbox', { name: /search messages/i });
      expect(input).toBeInTheDocument();
    });

    it('triggers re-fetch with correct message param when input changes', async () => {
      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: async () => [],
      });

      renderLogs();

      await screen.findByText('No logs found');

      const input = screen.getByRole('textbox', { name: /search messages/i });
      fireEvent.change(input, { target: { value: 'error' } });

      await waitFor(() => {
        expect(globalThis.fetch).toHaveBeenCalledWith(
          `${API_BASE_URL}/api/logs?message=error`
        );
      });
    });
  });

  describe('combined filters', () => {
    it('fetches with both level and message params when both filters are used', async () => {
      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: async () => [],
      });

      renderLogs();

      await screen.findByText('No logs found');

      const levelSelector = screen.getByRole('combobox', { name: /level/i });
      fireEvent.change(levelSelector, { target: { value: 'Error' } });

      await waitFor(() => {
        expect(globalThis.fetch).toHaveBeenCalledWith(
          `${API_BASE_URL}/api/logs?level=Error`
        );
      });

      const searchInput = screen.getByRole('textbox', { name: /search messages/i });
      fireEvent.change(searchInput, { target: { value: 'timeout' } });

      await waitFor(() => {
        expect(globalThis.fetch).toHaveBeenCalledWith(
          `${API_BASE_URL}/api/logs?level=Error&message=timeout`
        );
      });
    });
  });

  describe('log display', () => {
    const mockLogs = [
      {
        id: '1',
        timestamp: '2026-05-19T08:00:00Z',
        level: 'Info',
        source: 'api',
        message: 'First log',
        httpMethod: 'GET',
        httpPath: '/api/todos',
        httpStatus: 200,
      },
      {
        id: '2',
        timestamp: '2026-05-19T10:00:00Z',
        level: 'Error',
        source: 'api',
        message: 'Last log',
        httpMethod: 'POST',
        httpPath: '/api/todos',
        httpStatus: 500,
      },
      {
        id: '3',
        timestamp: '2026-05-19T09:00:00Z',
        level: 'Warning',
        source: 'api',
        message: 'Middle log',
      },
    ];

    it('displays logs in timestamp descending order', async () => {
      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: async () => mockLogs,
      });

      renderLogs();

      expect(await screen.findByText('Last log')).toBeInTheDocument();

      const entries = screen.getAllByRole('article');
      expect(entries[0]).toHaveTextContent('Last log');
      expect(entries[1]).toHaveTextContent('Middle log');
      expect(entries[2]).toHaveTextContent('First log');
    });

    it('displays level with color coding', async () => {
      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: async () => mockLogs,
      });

      renderLogs();

      await screen.findByText('Last log');

      const entries = screen.getAllByRole('article');
      const errorEntry = entries[0];
      const warningEntry = entries[1];
      const infoEntry = entries[2];

      expect(errorEntry.querySelector('.level-error')).toHaveTextContent('Error');
      expect(warningEntry.querySelector('.level-warning')).toHaveTextContent('Warning');
      expect(infoEntry.querySelector('.level-info')).toHaveTextContent('Info');
    });

    it('displays timestamp, source, and http details when available', async () => {
      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: async () => mockLogs,
      });

      renderLogs();

      await screen.findByText('Last log');

      const entries = screen.getAllByRole('article');
      const errorEntry = entries[0];
      const infoEntry = entries[2];

      expect(errorEntry.querySelector('.source')).toHaveTextContent('api');
      expect(errorEntry.querySelector('.http-method')).toHaveTextContent('POST');
      expect(errorEntry.querySelector('.http-path')).toHaveTextContent('/api/todos');
      expect(errorEntry.querySelector('.http-status')).toHaveTextContent('500');

      expect(infoEntry.querySelector('.http-method')).toHaveTextContent('GET');
      expect(infoEntry.querySelector('.http-path')).toHaveTextContent('/api/todos');
      expect(infoEntry.querySelector('.http-status')).toHaveTextContent('200');
    });
  });
});
