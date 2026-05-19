import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useTodoFilters } from './useTodoFilters';

describe('useTodoFilters', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn());
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });

  describe('default state', () => {
    it('returns filters with completed=null and search=undefined', () => {
      const { result } = renderHook(() => useTodoFilters());

      expect(result.current.filters).toEqual({
        completed: null,
        search: undefined,
      });
    });
  });

  describe('setCompletedFilter', () => {
    it('sets completed to true', () => {
      const { result } = renderHook(() => useTodoFilters());

      act(() => result.current.setCompletedFilter(true));

      expect(result.current.filters.completed).toBe(true);
    });

    it('sets completed to false', () => {
      const { result } = renderHook(() => useTodoFilters());

      act(() => result.current.setCompletedFilter(false));

      expect(result.current.filters.completed).toBe(false);
    });

    it('resets completed to null', () => {
      const { result } = renderHook(() => useTodoFilters());

      act(() => result.current.setCompletedFilter(true));
      act(() => result.current.setCompletedFilter(null));

      expect(result.current.filters.completed).toBe(null);
    });
  });

  describe('setSearchFilter', () => {
    it('sets search to a query string', () => {
      const { result } = renderHook(() => useTodoFilters());

      act(() => result.current.setSearchFilter('test query'));

      expect(result.current.filters.search).toBe('test query');
    });

    it('resets search to undefined', () => {
      const { result } = renderHook(() => useTodoFilters());

      act(() => result.current.setSearchFilter('query'));
      act(() => result.current.setSearchFilter(undefined));

      expect(result.current.filters.search).toBe(undefined);
    });
  });

  describe('no HTTP triggered', () => {
    it('filter changes do not call fetch', () => {
      const { result } = renderHook(() => useTodoFilters());

      act(() => result.current.setCompletedFilter(true));
      act(() => result.current.setSearchFilter('test'));

      expect(fetch).not.toHaveBeenCalled();
    });
  });

  describe('isolation', () => {
    it('works without any context provider', () => {
      const { result } = renderHook(() => useTodoFilters());

      expect(result.current.filters).toBeDefined();
      expect(result.current.setCompletedFilter).toBeDefined();
      expect(result.current.setSearchFilter).toBeDefined();
    });
  });
});
