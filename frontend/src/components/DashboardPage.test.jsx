import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { DashboardPage } from './DashboardPage';

const API_BASE_URL = 'http://localhost:5000';

const renderDashboard = () => {
  return render(
    <MemoryRouter initialEntries={['/dashboard']}>
      <DashboardPage />
    </MemoryRouter>
  );
};

describe('DashboardPage', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn());
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });

  describe('empty state', () => {
    it('shows empty state when no metrics are returned', async () => {
      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: async () => [],
      });

      renderDashboard();

      expect(await screen.findByText('No data for this time period')).toBeInTheDocument();
    });

    it('fetches metrics with default 24h window', async () => {
      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: async () => [],
      });

      renderDashboard();

      await screen.findByText('No data for this time period');

      expect(globalThis.fetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/dashboard/metrics?window=24h`
      );
    });
  });

  describe('metric cards', () => {
    const mockMetrics = [
      { endpoint: 'POST /todos', failureCount: 5, avgDurationMs: 120.5, totalRequests: 150 },
      { endpoint: 'GET /todos', failureCount: 2, avgDurationMs: 45.3, totalRequests: 500 },
    ];

    it('renders metric cards when data is present', async () => {
      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: async () => mockMetrics,
      });

      renderDashboard();

      expect(await screen.findByText('POST /todos')).toBeInTheDocument();
      expect(screen.getByText('GET /todos')).toBeInTheDocument();
    });

    it('displays failure count, avg duration, and total requests for each endpoint', async () => {
      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: async () => mockMetrics,
      });

      renderDashboard();

      await screen.findByText('POST /todos');

      expect(screen.getByText('Failures: 5')).toBeInTheDocument();
      expect(screen.getByText('Avg Duration: 120.5ms')).toBeInTheDocument();
      expect(screen.getByText('Total Requests: 150')).toBeInTheDocument();

      expect(screen.getByText('Failures: 2')).toBeInTheDocument();
      expect(screen.getByText('Avg Duration: 45.3ms')).toBeInTheDocument();
      expect(screen.getByText('Total Requests: 500')).toBeInTheDocument();
    });
  });

  describe('time window selector', () => {
    it('renders time window dropdown with correct options', async () => {
      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: async () => [],
      });

      renderDashboard();

      await screen.findByText('No data for this time period');

      const selector = screen.getByRole('combobox', { name: /time window/i });
      expect(selector).toBeInTheDocument();
      expect(selector).toHaveValue('24h');

      expect(screen.getByRole('option', { name: '1h' })).toBeInTheDocument();
      expect(screen.getByRole('option', { name: '24h' })).toBeInTheDocument();
      expect(screen.getByRole('option', { name: '7d' })).toBeInTheDocument();
      expect(screen.getByRole('option', { name: '30d' })).toBeInTheDocument();
    });

    it('triggers re-fetch with correct window param when selector changes', async () => {
      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: async () => [],
      });

      renderDashboard();

      await screen.findByText('No data for this time period');

      const selector = screen.getByRole('combobox', { name: /time window/i });
      fireEvent.change(selector, { target: { value: '7d' } });

      await waitFor(() => {
        expect(globalThis.fetch).toHaveBeenCalledWith(
          `${API_BASE_URL}/api/dashboard/metrics?window=7d`
        );
      });
    });
  });
});
