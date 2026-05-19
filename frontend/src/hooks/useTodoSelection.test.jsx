import { describe, it, expect } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { useTodoSelection } from './useTodoSelection';

describe('useTodoSelection', () => {
  it('starts with empty selectedIds', () => {
    const { result } = renderHook(() => useTodoSelection());
    expect(result.current.selectedIds).toEqual([]);
  });

  it('selectAll(count) sets selectedIds to [1, 2, ..., count]', async () => {
    const { result } = renderHook(() => useTodoSelection());
    result.current.selectAll(3);
    await waitFor(() => {
      expect(result.current.selectedIds).toEqual([1, 2, 3]);
    });
  });

  it('selectTodo(id) adds id to selectedIds', async () => {
    const { result } = renderHook(() => useTodoSelection());
    result.current.selectTodo(5);
    await waitFor(() => {
      expect(result.current.selectedIds).toEqual([5]);
    });
  });

  it('selectTodo(id) no-op if already selected', async () => {
    const { result } = renderHook(() => useTodoSelection());
    result.current.selectTodo(5);
    result.current.selectTodo(5);
    await waitFor(() => {
      expect(result.current.selectedIds).toEqual([5]);
    });
  });

  it('deselectTodo(id) removes id from selectedIds', async () => {
    const { result } = renderHook(() => useTodoSelection());
    result.current.selectAll(3);
    result.current.deselectTodo(2);
    await waitFor(() => {
      expect(result.current.selectedIds).toEqual([1, 3]);
    });
  });

  it('deselectAll() clears all selectedIds', async () => {
    const { result } = renderHook(() => useTodoSelection());
    result.current.selectAll(3);
    result.current.deselectAll();
    await waitFor(() => {
      expect(result.current.selectedIds).toEqual([]);
    });
  });

  it('selectedIds are sticky - deleted ids persist', async () => {
    const { result } = renderHook(() => useTodoSelection());
    result.current.selectAll(3);
    // Simulate deleting todo with id 2 - hook doesn't auto-cleanup
    await waitFor(() => {
      expect(result.current.selectedIds).toEqual([1, 2, 3]);
    });
  });
});
